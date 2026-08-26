using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42, ERROR 5 — el ESCALÓN del larguero de salida apunta al CENTRO DE LA CABECERA a la que se conecta, y una
    /// cama tiene UN solo larguero alto.
    ///
    /// <para>
    /// La regla del dueño es física y se deriva de la geometría, no de un caso especial por índice de frente: para
    /// cada larguero alto hay que saber (1) a qué cabecera se conecta, (2) qué lado de esa cabecera ocupa la cama y
    /// (3) dónde queda su centro. La cabecera es el MÓDULO en el que la cama termina — el que acaba en esa línea si
    /// la cama avanza hacia +X, el que empieza en ella si avanza hacia −X—, y el escalón tiene que apuntar hacia
    /// dentro de ese módulo.
    /// </para>
    /// <para>
    /// Elegir el módulo por proximidad NO sirve: en la línea de la interfaz terminan DOS cabeceras, una por lado, y
    /// cada cama se conecta a la suya. Ese era el error de medida que hacía parecer invertidos los largueros del
    /// lado B.
    /// </para>
    /// </summary>
    public class PushBackHighEndHandTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs(bool reinforced = false)
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            inputs.DerivedPostReinforced = reinforced;
            return inputs;
        }

        private static PushBackSystem Build(
            PushBackCellTopology topology,
            PushBackRunDirection direction,
            int slots = 2,
            int deepA = 4,
            int deepB = 4,
            bool reinforced = false)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetDefaults(topology, direction);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SideA.Structure.Fronts[slot].PalletsDeep = deepA;
                state.SideB.Structure.Fronts[slot].PalletsDeep = deepB;
            }

            return new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(reinforced), Catalog).System;
        }

        public static IEnumerable<object[]> Cases()
        {
            foreach (var topology in new[]
            {
                PushBackCellTopology.SoloA, PushBackCellTopology.SoloB,
                PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida
            })
            {
                foreach (var direction in new[] { PushBackRunDirection.AToB, PushBackRunDirection.BToA })
                {
                    yield return new object[] { topology, direction, 4, 4 };   // simetrico
                    yield return new object[] { topology, direction, 3, 6 };   // frente corto contra largo
                }
            }
        }

        /// <summary>
        /// El escalón de CADA larguero alto apunta hacia el centro de la cabecera en la que su cama termina, en las
        /// cuatro topologías, los dos sentidos y con frentes de fondos distintos.
        /// </summary>
        [Theory]
        [MemberData(nameof(Cases))]
        public void TheHighBeamStep_PointsTowardTheHeaderCenter(
            PushBackCellTopology topology, PushBackRunDirection direction, int deepA, int deepB)
        {
            var catalog = Catalog;
            var system = Build(topology, direction, deepA: deepA, deepB: deepB);
            var runs = PushBackRuns.Resolve(system);
            var structure = system.Structure;

            // Las dos aristas medidas del bloque: su diferencia ES el escalón, en coordenadas locales.
            var left = CatalogLookup.Local(catalog, PushBackDefaults.HighEndBeamCatalogId, "INICIO_IZQUIERDO", "LATERAL");
            var right = CatalogLookup.Local(catalog, PushBackDefaults.HighEndBeamCatalogId, "INICIO_DERECHO", "LATERAL");
            Assert.NotEqual(left.X, right.X, 6);

            var checkedRuns = 0;
            foreach (var run in runs.Runs)
            {
                var placements = PushBackPlacements.Resolve(run.Source, run.Front());
                var rear = placements.FirstOrDefault(p => p.LevelNumber == run.SourceLevel && p.IsEntrance);
                if (rear == null)
                {
                    continue;
                }

                var worldX = run.Reflected ? PushBackMirror.X(runs.MirrorAxis, rear.X) : rear.X;
                var mirrored = run.Reflected ? !rear.MirroredX : rear.MirroredX;
                var step = (right.X - left.X) * (mirrored ? -1.0 : 1.0);

                // La CABECERA a la que se conecta: el modulo en el que la cama TERMINA. Si avanza hacia +X es el que
                // acaba en esa linea; si avanza hacia -X, el que empieza en ella. En la interfaz acaban dos, una por
                // lado, y cada cama se conecta a la suya.
                var forward = !run.Reflected;
                var header = structure.Modules
                    .Where(module => module.IsHeader)
                    .FirstOrDefault(module => Math.Abs((forward ? module.EndX : module.StartX) - worldX) < 1e-6);
                if (header == null)
                {
                    continue;   // la cama no acaba en una cabecera: su apoyo es un separador, otra regla
                }

                var toCenter = (header.StartX + header.EndX) / 2.0 - worldX;
                Assert.True(
                    Math.Sign(step) == Math.Sign(toCenter),
                    $"{topology}/{direction} {deepA}/{deepB}: el escalon del larguero alto en X={worldX:0.##} "
                        + $"apunta a {(step > 0 ? "+X" : "-X")} y el centro de su cabecera esta a "
                        + $"{(toCenter > 0 ? "+X" : "-X")}");
                checkedRuns++;
            }

            Assert.True(checkedRuns > 0, "ninguna cama acabo en una cabecera: el caso no comprueba nada");
        }

        /// <summary>
        /// Y el TOPE hereda la mano de SU larguero alto: se resuelve desde la misma terminación, no desde el lado
        /// A/B en bruto. Si los dos no coincidieran, el tope quedaría montado del revés sobre su propio larguero.
        /// </summary>
        [Theory]
        [MemberData(nameof(Cases))]
        public void TheRearTope_InheritsTheHandOfItsHighBeam(
            PushBackCellTopology topology, PushBackRunDirection direction, int deepA, int deepB)
        {
            var system = Build(topology, direction, deepA: deepA, deepB: deepB);
            var builder = new PushBackSystemLateralBuilder();
            var cuts = builder.Cortes(system, Catalog);

            var pairs = 0;
            for (var cut = 0; cut < cuts.Count; cut++)
            {
                var instances = builder.Build(system, Catalog, cut).Flatten().Instances;
                var beams = instances
                    .Where(i => string.Equals(i.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var topes = instances.Where(i => i.Role == HeaderBlockRole.Tope).ToList();
                if (topes.Count == 0)
                {
                    continue;
                }

                // Se comparan como CONJUNTOS por banda de elevacion: con camas encontradas los dos largueros altos
                // caen en la misma linea y emparejarlos uno a uno por cercania seria ambiguo. Lo que se exige es que
                // las manos que hay sean exactamente las que sus largueros imponen.
                IReadOnlyList<bool> Hands(IEnumerable<HeaderBlockInstance> source, Func<bool, bool> map)
                    => source.Select(i => map(i.MirroredX)).OrderBy(x => x).ToList();

                // La relacion es la del MARCO, no una constante: en el marco de la cama el tope va sin espejo
                // (decision del dueño de 2026-07-24) y su larguero alto SI lo lleva. Al reflejar el lado B los dos se
                // invierten juntos, asi que la pareja «larguero espejado / tope sin espejo» se conserva en las dos
                // manos. Comprobarlo con la constante mediria la decision, no el marco.
                Assert.Equal(
                    Hands(beams, hand => !hand),
                    Hands(topes, hand => hand));
                pairs += topes.Count;
            }

            Assert.True(pairs > 0, "no se materializo ningun tope: el caso no comprueba nada");
        }

        /// <summary>
        /// UN larguero alto por cama, tambien con POSTE REFORZADO. Una cabecera reforzada tiene mas perfiles, pero no
        /// crea camas: el dueño vio DOS largueros donde hay uno.
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReinforcedPost_EmitsOneHighBeamPerRun(bool reinforced)
        {
            var system = Build(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, reinforced: reinforced);
            var runs = PushBackRuns.Resolve(system);
            var builder = new PushBackSystemLateralBuilder();
            var cuts = builder.Cortes(system, Catalog);

            for (var cut = 0; cut < cuts.Count; cut++)
            {
                var beams = builder.Build(system, Catalog, cut).Flatten().Instances
                    .Where(i => string.Equals(i.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                    .Select(i => FormattableString.Invariant(
                        $"{i.Insertion.X:0.####}|{i.Insertion.Y:0.####}|{i.MirroredX}"))
                    .ToList();

                Assert.Equal(beams.Count, beams.Distinct().Count());
            }

            // Y el BOM cuenta uno por cama, ni uno mas: el refuerzo no aporta largueros.
            var quoted = PushBackBomBuilder.Build(system, Catalog).Components
                .Where(c => string.Equals(c.Category, PushBackBomBuilder.HighEndBeam, StringComparison.Ordinal))
                .Sum(c => c.Quantity);
            Assert.Equal(runs.Runs.Count, quoted);
        }
    }
}
