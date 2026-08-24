using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — la PLANTA de un rack compuesto. Es la vista donde los dos sentidos comparten la misma calle, asi que es
    /// la que mas necesita distinguirlos.
    ///
    /// <para>
    /// Se compone como el lateral: la ESTRUCTURA se dibuja UNA vez, desde el sistema compuesto (cabeceras,
    /// separadores —el central incluido—, postes derivados, placas y seguridad), y encima se monta el contenido de
    /// cada lado, tomado de su planta local y llevado al rack con la MISMA reflexion rigida que usa todo lo demas del
    /// lado B. Ningun poste, cabecera ni placa se dibuja dos veces por el hecho de que el rack tenga dos sentidos.
    /// </para>
    /// <para>
    /// La planta colapsa los niveles, pero proyecta la UNION DE PIEZAS FISICAS REALES: pregunta a
    /// <see cref="PushBackRuns"/> que ranuras tienen de verdad una cama que descarga en cada pasillo y cuales tienen
    /// de verdad un larguero posterior en cada interfaz. Una ranura cuyos niveles sean TODOS camas corridas no
    /// dibuja larguero posterior en la interfaz de su lado bajo, porque alli no hay ninguno; basta con que UN nivel
    /// lo requiera para que aparezca. No se inventan piezas por el hecho de que la planta no tenga selector de nivel.
    /// </para>
    /// </summary>
    public static class PushBackCompositePlanta
    {
        public static HeaderRunPlan Build(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            var composite = system?.Composite;
            if (structure == null || composite == null)
            {
                return new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            // 1) La estructura, una sola vez. Se retiran los largueros de extremo del dinamico: los aporta cada lado.
            var result = new DynamicSystemPlantaBuilder()
                .BuildPlan(structure, catalog)
                .Flatten()
                .Instances
                .Where(instance => !PushBackPlanComposer.IsDynamicSpecific(instance))
                .ToList();

            // 2) El contenido de cada lado, en su marco, reflejado el del lado B — y SOLO las piezas que existen.
            var runs = PushBackRuns.Resolve(system);
            AppendSide(result, system, catalog, composite.SideA, runs, PushBackSide.A, reflected: false, mirrorAxis: 0.0);
            AppendSide(result, system, catalog, composite.SideB, runs, PushBackSide.B, reflected: true,
                mirrorAxis: PushBackMirror.AxisOf(structure));

            // 3) Los INTERMEDIOS. Pertenecen a la CAMA, no a la estructura, asi que el paso 1 los retira con el
            //    resto de piezas del dinamico y aqui se reponen POR CAMA, en el marco de cada una. Sin este paso la
            //    planta de un rack compuesto salia sin un solo intermedio.
            AppendIntermediates(result, catalog, runs);

            // 4) Las etiquetas A/B, por el pipeline de anotaciones que ya existe. Nunca al BOM.
            result.AddRange(PushBackSideAnnotations.Planta(system));

            return HeaderInstanceGrouper.Group(result, "PB_PLANTA_PIEZA");
        }

        /// <summary>
        /// El contenido de un lado: su larguero IN/OUT del pasillo y su larguero posterior de troquel redondo con su
        /// tope. Se toma de la planta LOCAL del lado —el mismo builder de un Push Back de un sentido— en DOS pasadas,
        /// cada una sobre una copia de la sub-estructura donde solo estan activas las ranuras que realmente tienen
        /// esa pieza. Asi la planta proyecta la union de piezas fisicas reales sin necesitar un selector de nivel que
        /// no tiene, y sin identificar piezas por coordenada.
        /// </summary>
        private static void AppendSide(
            List<HeaderBlockInstance> target,
            PushBackSystem system,
            RackCatalog catalog,
            PushBackSideSystem side,
            PushBackRunSet runs,
            PushBackSide which,
            bool reflected,
            double mirrorAxis)
        {
            if (side == null || !side.IsPresent || side.Local?.Structure == null)
            {
                return;
            }

            var highId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var inOutId = string.IsNullOrWhiteSpace(side.Local.Structure.InOutBeamCatalogId)
                ? DynamicRackDefaults.InOutBeamCatalogId
                : side.Local.Structure.InOutBeamCatalogId;

            var lowSlots = new HashSet<int>(runs.Runs.Where(run => run.LowSide == which).Select(run => run.Slot));
            var highSlots = new HashSet<int>(runs.Runs.Where(run => run.HighSide == which).Select(run => run.Slot));

            var content = new List<HeaderBlockInstance>();
            content.AddRange(Pieces(side.Local, catalog, lowSlots)
                .Where(instance => string.Equals(instance.PieceId, inOutId, StringComparison.OrdinalIgnoreCase)));
            content.AddRange(Pieces(side.Local, catalog, highSlots)
                .Where(instance => instance.Role == HeaderBlockRole.Tope
                    || string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase)));

            target.AddRange(reflected ? PushBackMirror.Instances(content, mirrorAxis) : content);
        }

        /// <summary>
        /// Los LARGUEROS INTERMEDIOS de la planta, uno por cama y no uno por estructura.
        ///
        /// <para>
        /// La planta colapsa los niveles, asi que lo que proyecta es la UNION de los apoyos que usan las camas de esa
        /// ranura. Cada cama los resuelve en SU marco —el local del lado, o el sintetico de la corrida, que atraviesa
        /// la interfaz— con el mismo builder dinamico de siempre, y el resultado se refleja igual que el resto del
        /// contenido de esa cama. Por eso una corrida corta obtiene intermedios en TODO su recorrido, incluida la
        /// parte que pisa el otro lado, y no obtiene ninguno en el tramo de estructura que no usa.
        /// </para>
        /// <para>
        /// Se deduplica por pieza y posicion: dos camas encontradas comparten ranura, y si alguna vez coincidieran en
        /// un apoyo seria el MISMO larguero, no dos.
        /// </para>
        /// </summary>
        private static void AppendIntermediates(
            List<HeaderBlockInstance> target, RackCatalog catalog, PushBackRunSet runs)
        {
            var added = new HashSet<string>();

            // La planta colapsa los niveles, asi que dos camas de la misma ranura que compartan marco proyectan lo
            // MISMO: se agrupan y se construye una sola vez. Sin esto un rack de muchos niveles reconstruiria la
            // planta de un lado una vez por celda.
            var batches = runs.Runs
                .Where(run => run?.Source?.Structure != null)
                .GroupBy(run => (run.Source, run.SourceFrontIndex, run.Reflected))
                .Select(group => group.First());

            foreach (var run in batches)
            {
                var structure = run.Source.Structure;
                if (structure == null || run.SourceFrontIndex < 0 || run.SourceFrontIndex >= structure.Fronts.Count)
                {
                    continue;
                }

                var slot = run.SourceFrontIndex;
                var restricted = PushBackMirror.Clone(structure, index => index == slot);
                var pieces = new DynamicSystemPlantaBuilder()
                    .BuildPlan(restricted, catalog)
                    .Flatten()
                    .Instances
                    .Where(PushBackPlanComposer.IsDynamicIntermediate)
                    .ToList();
                if (pieces.Count == 0)
                {
                    continue;
                }

                var placed = run.Reflected ? PushBackMirror.Instances(pieces, runs.MirrorAxis) : pieces;
                foreach (var instance in placed)
                {
                    var key = string.Join(
                        "|",
                        instance.PieceId,
                        instance.Insertion.X.ToString("0.####", CultureInfo.InvariantCulture),
                        instance.Insertion.Y.ToString("0.####", CultureInfo.InvariantCulture),
                        instance.MirroredX);
                    if (added.Add(key))
                    {
                        target.Add(instance);
                    }
                }
            }
        }

        /// <summary>
        /// La planta de un lado con SOLO las ranuras indicadas activas. Las demas viajan en blanco (I-33), asi que no
        /// aportan larguero, tope ni cama — la regla ya existe y no hace falta una segunda.
        /// </summary>
        private static IReadOnlyList<HeaderBlockInstance> Pieces(
            PushBackSystem local, RackCatalog catalog, ICollection<int> slots)
        {
            if (slots.Count == 0)
            {
                return new List<HeaderBlockInstance>();
            }

            var restricted = new PushBackSystem
            {
                Structure = PushBackMirror.Clone(local.Structure, slot => slots.Contains(slot)),
                HighEndBeamCatalogId = local.HighEndBeamCatalogId,
                RearTope = local.RearTope
            };
            foreach (var resolved in local.HighEndBeams)
            {
                restricted.HighEndBeams.Add(resolved);
            }

            return new PushBackSystemPlantaBuilder().BuildPlan(restricted, catalog).Flatten().Instances;
        }

    }
}
