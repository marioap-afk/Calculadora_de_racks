using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — el DESVIADOR de una cama Push Back, resuelto en el marco de la CAMA.
    ///
    /// <para>
    /// Contrato físico: <c>Diverter(run) = LowEnd(run)</c>. Un desviador guía la tarima al ENTRAR, así que pertenece
    /// siempre al extremo por el que se carga, y ese extremo lo sabe la cama. No depende del lado A/B, ni de
    /// izquierda/derecha en coordenadas de rack, ni del extremo alto, ni de la elevación que el resolver compartido
    /// le dio al nivel.
    /// </para>
    /// <para>
    /// Existe porque el desviador se había quedado fuera del pipeline por camas. El corte lateral compuesto lo
    /// heredaba del builder dinámico, que conserva la regla de un rack de un solo sentido —«izquierda = extremo
    /// bajo, derecha = extremo alto»— y en un rack compuesto eso es falso: el lado B tiene su extremo bajo a la
    /// DERECHA. El lado A funcionaba por coincidencia y el B salía a la elevación del extremo contrario. Además una
    /// clasificación por línea no puede expresar que, en la MISMA línea, el nivel 1 corra A→B y el nivel 2 B→A; la
    /// cama sí tiene esa granularidad.
    /// </para>
    /// <para>
    /// En el marco de una cama el flujo avanza SIEMPRE hacia +X, así que aquí el extremo bajo es el arranque del
    /// frente y la pieza se emite sin espejo. Llevarla al mundo —incluida la mano— es trabajo de la MISMA reflexión
    /// rígida que mueve el riel, los rodillos, los largueros y el tope de esa cama.
    /// </para>
    /// </summary>
    /// <summary>
    /// I-42 (A1B-D1) — UN DESVIADOR FISICO YA RESUELTO. Su identidad es <b>linea de postes × nivel × pasillo</b>, y
    /// se traza siempre a la CAMA que lo justifica: un desviador guia la tarima al entrar, asi que existe porque hay
    /// una cama que descarga por ese pasillo en ese nivel.
    /// </summary>
    public sealed class ResolvedDiverter
    {
        /// <summary>La linea transversal de postes donde va la pieza.</summary>
        public int PostLine { get; set; }

        /// <summary>El nivel de carga (1-based), el mismo que numeran las camas y los largueros.</summary>
        public int Level { get; set; }

        /// <summary>El pasillo al que pertenece: el lado por el que carga la cama.</summary>
        public PushBackSide LowSide { get; set; }

        /// <summary>La ranura de la cama que lo justifica, para poder trazarlo a un run concreto.</summary>
        public int Slot { get; set; }

        /// <summary>True cuando esa cama corre reflejada (su pasillo es el extremo lejano del rack).</summary>
        public bool Reflected { get; set; }

        public string PieceId { get; set; }

        /// <summary>La X del PASILLO: el contacto bajo de la cama que lo justifica.</summary>
        public double LowX { get; set; }

        /// <summary>La X de su linea de postes, que es donde lo ancla un corte frontal.</summary>
        public double LineX { get; set; }

        /// <summary>
        /// La identidad fisica. Dos desviadores con la misma identidad son la MISMA pieza; ninguna otra lo es —ni
        /// aunque compartan coordenadas por proyeccion, ni aunque sean del mismo id de catalogo—.
        /// </summary>
        public string Identity
            => FormattableString.Invariant($"L{PostLine}|N{Level}|{LowSide}");
    }

    public static class PushBackDiverterPlan
    {
        private const string View = "LATERAL";

        private static readonly IReadOnlyList<ResolvedDiverter> NoDiverters = new List<ResolvedDiverter>();

        /// <summary>
        /// I-42 (A1B-D1, contrato del dueño) — EL CONJUNTO FISICO DE DESVIADORES del rack, resuelto UNA vez desde
        /// las camas.
        ///
        /// <para>
        /// <b>La regla.</b> Un desviador pertenece al extremo BAJO —entrada/salida— de la cama que lo justifica, y a
        /// la linea de postes donde se atornilla. Nunca al extremo alto, nunca al pasillo contrario, y nunca deducido
        /// de un lado historico, de un espejo ni del nombre de una vista: la cama sabe por donde carga.
        /// </para>
        /// <para>
        /// <b>Lo que corrige.</b> El BOM enumeraba los desviadores recorriendo los DOS cortes frontales de la
        /// estructura compuesta, sin preguntar que pasillos existen de verdad. Medido con 3 lineas y 2 niveles: en
        /// solo A, en solo B y en corrida —un unico pasillo, 6 piezas fisicas— el BOM cobraba 12. En encontradas
        /// acertaba por casualidad, porque ahi los dos pasillos existen.
        /// </para>
        /// <para>
        /// Las celdas desactivadas del desviador (su rejilla por poste y nivel) se respetan tal cual: esta autoridad
        /// hereda esa decision, no la reinterpreta.
        /// </para>
        /// </summary>
        public static IReadOnlyList<ResolvedDiverter> Resolve(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            if (structure == null || catalog == null)
            {
                return NoDiverters;
            }

            var selection = SelectiveSafetyFamilies.SelectedOfType(
                structure.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DesviadorType);
            if (selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
            {
                return NoDiverters;
            }

            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            if (layout?.PostPositions == null)
            {
                return NoDiverters;
            }

            var beds = Beds(system, catalog);
            var off = SelectiveSafetyGrid.OffCellKeys(selection.DesviadorOffCells);
            var result = new List<ResolvedDiverter>();

            for (var line = 0; line < layout.PostPositions.Count; line++)
            {
                // La seguridad indexada por POSTE se atornilla a esa frontera; si no existe, no se coloca (I-33).
                if (!DynamicFrontActivation.BoundaryExists(structure, line))
                {
                    continue;
                }

                var cell = SelectiveDesviadorPlan.CellKey(selection, line, structure.Fronts.Count);

                // Una linea la sirven las camas de las ranuras que la tocan: la de su izquierda y la de su derecha.
                foreach (var bed in beds.Where(bed => bed.Slot == line - 1 || bed.Slot == line))
                {
                    if (off.Contains((cell, bed.Level - 1)))
                    {
                        continue;   // esa celda tiene el desviador desactivado
                    }

                    var identity = FormattableString.Invariant($"L{line}|N{bed.Level}|{bed.LowSide}");
                    if (result.Any(existing => string.Equals(existing.Identity, identity, StringComparison.Ordinal)))
                    {
                        continue;   // dos ranuras vecinas comparten la MISMA linea: es una sola pieza
                    }

                    result.Add(new ResolvedDiverter
                    {
                        PostLine = line,
                        Level = bed.Level,
                        LowSide = bed.LowSide,
                        Slot = bed.Slot,
                        Reflected = bed.Reflected,
                        PieceId = selection.ElementId,
                        LowX = bed.LowX,
                        LineX = layout.PostPositions[line],
                    });
                }
            }

            return result;
        }

        /// <summary>Una cama fisica reducida a lo que el desviador necesita saber de ella.</summary>
        private readonly struct Bed
        {
            public Bed(int slot, int level, PushBackSide lowSide, bool reflected, double lowX)
            {
                Slot = slot;
                Level = level;
                LowSide = lowSide;
                Reflected = reflected;
                LowX = lowX;
            }

            public int Slot { get; }

            public int Level { get; }

            public PushBackSide LowSide { get; }

            public bool Reflected { get; }

            public double LowX { get; }
        }

        /// <summary>
        /// Las camas fisicas del rack. Un COMPUESTO las tiene resueltas —<see cref="PushBackRuns"/> es su autoridad—;
        /// uno de un solo sentido no tiene esa estructura y sus camas son las de siempre: una por frente y nivel, que
        /// carga por el unico pasillo que el rack tiene.
        /// </summary>
        private static IReadOnlyList<Bed> Beds(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system.Structure;
            if (system.IsComposite)
            {
                var runs = PushBackRuns.Resolve(system);
                return runs.Runs
                    .Select(run =>
                    {
                        var axis = PushBackRunGeometry.Axis(run, catalog, runs.MirrorAxis);
                        return new Bed(
                            run.Slot, run.Level, run.LowSide, run.Reflected,
                            axis.HasValue ? axis.Value.LowContact.X : 0.0);
                    })
                    .ToList();
            }

            var result = new List<Bed>();
            for (var front = 0; front < structure.Fronts.Count; front++)
            {
                foreach (var level in DynamicFrontGeometry.LoadBeamLevels(structure, structure.Fronts[front]))
                {
                    result.Add(new Bed(
                        front, level.LevelNumber, PushBackSide.A, false, structure.Fronts[front].StartX));
                }
            }

            return result;
        }

        /// <summary>
        /// Los desviadores de las camas de <paramref name="front"/> en el sistema <paramref name="system"/>, todos
        /// en su extremo BAJO. <paramref name="levels"/> acota a los niveles que esta llamada materializa (un nivel
        /// puede pertenecer a una cama corrida y no a la de este lado); null = todos.
        /// </summary>
        public static IReadOnlyList<HeaderBlockInstance> Lateral(
            PushBackSystem system,
            RackCatalog catalog,
            DynamicRackFront front,
            IReadOnlyCollection<int> levels = null,
            int postIndex = -1)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null || front == null)
            {
                return result;
            }

            var selection = SelectiveSafetyFamilies.SelectedOfType(
                structure.SafetySelections, catalog?.SafetyElements, SelectiveSafetyDefaults.DesviadorType);
            if (selection == null)
            {
                return result;
            }

            var block = CatalogLookup.Block(catalog, selection.ElementId, View);
            if (string.IsNullOrWhiteSpace(block))
            {
                return result;
            }

            var longitud = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorLongitud)
                ? selection.DesviadorLongitud
                : SelectiveSafetyDefaults.DesviadorLongitud;
            var firstHeight = SelectiveDesviadorPlan.IsValidEvenAbove8(selection.DesviadorPrimerNivelAltura)
                ? selection.DesviadorPrimerNivelAltura
                : SelectiveSafetyDefaults.DesviadorPrimerNivelAltura;

            // El primer nivel conserva su contrato SELECTIVO: mide desde el primer troquel del poste, no desde el
            // larguero. Es la regla aprobada y no la toca esta corrección.
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var firstY = FirstTroquelY(catalog, postId, DynamicFrontGeometry.PostPeralte(structure, catalog, postId))
                + firstHeight;

            // LA AUTORIDAD: la elevación del larguero de ENTRADA de cada cama de este frente.
            var low = PushBackElevations.LowInsertions(system, catalog, front);
            var off = SelectiveSafetyGrid.OffCellKeys(selection.DesviadorOffCells);
            var cell = SelectiveDesviadorPlan.CellKey(
                selection, postIndex >= 0 ? postIndex : front.Index, structure.Fronts.Count);

            foreach (var level in DynamicFrontGeometry.LoadBeamLevels(structure, front))
            {
                if (levels != null && !levels.Contains(level.LevelNumber))
                {
                    continue;
                }

                if (off.Contains((cell, level.LevelNumber - 1)))
                {
                    continue;   // esta celda tiene el desviador desactivado
                }

                var y = level.LevelNumber <= 1
                    ? firstY
                    : (low.TryGetValue(level.LevelNumber, out var resolved) ? resolved : level.ExitElevation)
                      - SelectiveDesviadorPlan.BeamYOffset;

                result.Add(new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Safety,
                    PieceId = selection.ElementId,
                    BlockName = block,
                    View = View,
                    // En el marco de la cama el extremo BAJO es el arranque del frente, y la pieza va sin espejo.
                    // La mano y la posición en el mundo las pone la reflexión rígida de esa cama.
                    Insertion = new Point2D(front.StartX, y),
                    ConnectionAnchor = new Point2D(front.StartX, y),
                    MirroredX = false
                });

                if (longitud > 0.0)
                {
                    result[result.Count - 1].DynamicParameters[SelectiveRackDefaults.LengthParam] = longitud;
                }
            }

            return result;
        }

        /// <summary>
        /// Si una instancia ya colocada es un DESVIADOR, segun el catalogo. Lo usa el corte compuesto para retirar
        /// los que emitio el builder dinamico antes de reponerlos por cama: dos autoridades para la misma pieza es
        /// exactamente lo que esta correccion elimina.
        /// </summary>
        public static bool IsDiverter(HeaderBlockInstance instance, RackCatalog catalog)
        {
            if (instance == null || instance.Role != HeaderBlockRole.Safety)
            {
                return false;
            }

            return SelectiveSafetyFamilies
                .VariantsOfType(catalog?.SafetyElements, SelectiveSafetyDefaults.DesviadorType)
                .Any(entry => string.Equals(entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>El primer troquel del poste, que es desde donde mide el primer nivel (contrato Selectivo).</summary>
        private static double FirstTroquelY(RackCatalog catalog, string postId, double peralte)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(
                postId, SelectiveRackDefaults.PostBeamPoint, SelectiveRackDefaults.View);
            return SelectivePostGeometry.Resolve(entry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = peralte
            }).Y;
        }
    }
}
