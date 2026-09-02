using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1B-D7, contrato del dueño) — LA PLANTA DIBUJA CADA LARGUERO DE CAMA UNA VEZ, Y NO FUNDE DOS.
    ///
    /// <para>
    /// La planta colapsa la ELEVACION: un rack de tres niveles dibuja un larguero de entrada en su linea, no tres.
    /// Esa convencion la aplicaba el builder de un solo sentido dentro de cada lote, asi que una ranura cuyos
    /// niveles se resuelven en marcos distintos —uno Solo A y otro CORRIDO— dibujaba el mismo simbolo dos veces,
    /// superpuesto. Lo contrario tambien tiene que seguir siendo cierto: dos camas ENCONTRADAS topan en la interfaz
    /// y con hueco cero proyectan sus dos largueros altos en el MISMO punto, y ahi hay dos piezas.
    /// </para>
    /// <para>
    /// Nada de esto toca al BOM, que cuenta CAMAS y no instancias de una vista.
    /// </para>
    /// </summary>
    public class PushBackPlantBedBeamIdentityTests
    {
        private const string InOutBeam = "LARGUERO_IN_OUT_C6";
        private const string HighBeam = "LARGUERO_ESCALON_TROQUEL_REDONDO";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Una ranura, dos niveles, con la topologia que se pida en cada uno.</summary>
        private static PushBackSystem Build(
            PushBackCellTopology level1,
            PushBackCellTopology level2,
            PushBackRunDirection direction = PushBackRunDirection.AToB,
            double gap = 0.0)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 1, slotsB: 1, deepA: 3, deepB: 3, levelsA: 2, levelsB: 2, gap: gap);
            design.Composite.DefaultTopology = level1;
            design.Composite.DefaultDirection = direction;
            design.Composite.Topologies.Add(new PushBackTopologyCell
            {
                Frente = 0,
                Level = 1,
                Topology = level2,
                Direction = direction,
            });
            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static IReadOnlyList<HeaderBlockInstance> Plant(PushBackSystem system)
            => new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances.ToList();

        private static IReadOnlyList<HeaderBlockInstance> Beams(PushBackSystem system, string pieceId)
            => Plant(system)
                .Where(instance => string.Equals(instance.PieceId, pieceId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        private static int At(PushBackSystem system, string pieceId, double x)
            => Beams(system, pieceId).Count(instance => Math.Abs(instance.Insertion.X - x) < 1e-6);

        // ---------------------------------------------------------------- misma pieza, un solo simbolo

        [Fact]
        public void CompositePlant_DeduplicatesSharedPhysicalBedBeam()
        {
            // El nivel 1 es Solo A y el 2 CORRIDO: dos camas, dos marcos, dos lotes. Sus dos largueros de entrada
            // caen en la MISMA linea de carga de A, y la planta —que colapsa la elevacion— dibuja un simbolo.
            var mixed = Build(PushBackCellTopology.SoloA, PushBackCellTopology.Corrida);
            var runs = PushBackRuns.Resolve(mixed);

            Assert.Equal(2, runs.Runs.Count);
            Assert.Equal(2, runs.Runs.Select(run => run.Source).Distinct().Count());   // dos marcos distintos
            Assert.All(runs.Runs, run => Assert.Equal(PushBackSide.A, run.LowSide));   // el mismo pasillo de carga

            Assert.Equal(1, At(mixed, InOutBeam, 0.0));
        }

        [Fact]
        public void CompositePlant_LevelsCollapseTheSameWayInsideAndAcrossBatches()
        {
            // El CONTROL: los dos niveles en el mismo marco. La planta ya dibujaba un solo simbolo, y esa es la
            // convencion que el caso mixto tiene que respetar.
            var control = Build(PushBackCellTopology.SoloA, PushBackCellTopology.SoloA);
            var mixed = Build(PushBackCellTopology.SoloA, PushBackCellTopology.Corrida);

            Assert.Equal(1, At(control, InOutBeam, 0.0));
            Assert.Equal(At(control, InOutBeam, 0.0), At(mixed, InOutBeam, 0.0));
        }

        [Fact]
        public void CompositePlant_CorridaDoesNotDuplicateSamePhysicalBoundary()
        {
            // Una corrida es UNA cama aunque atraviese los dos lados: ni su entrada ni su larguero alto se dibujan
            // dos veces por existir dos representaciones locales.
            var system = Build(PushBackCellTopology.Corrida, PushBackCellTopology.Corrida);

            Assert.All(
                Beams(system, InOutBeam).GroupBy(PushBackPlanComposer.PhysicalKey),
                group => Assert.Single(group));
            Assert.All(
                Beams(system, HighBeam).GroupBy(PushBackPlanComposer.PhysicalKey),
                group => Assert.Single(group));
        }

        // ---------------------------------------------------------------- dos piezas que coinciden

        [Fact]
        public void CompositePlant_DoesNotDeduplicateDistinctCoincidentBedBeams()
        {
            // Camas ENCONTRADAS con hueco CERO: los dos largueros altos se topan en la interfaz y proyectan en la
            // MISMA X. Son dos piezas fisicas —una de cada cama— y las dos tienen que sobrevivir.
            var system = Build(PushBackCellTopology.Encontradas, PushBackCellTopology.Encontradas);
            var runs = PushBackRuns.Resolve(system);
            var interfaceX = system.Composite.Of(PushBackSide.A).InnerX;

            Assert.Equal(2, runs.Runs.Select(run => (run.Source, run.Reflected)).Distinct().Count());
            Assert.Equal(2, At(system, HighBeam, interfaceX));

            // Y son distinguibles: comparten punto, pero no identidad fisica.
            var coincident = Beams(system, HighBeam)
                .Where(instance => Math.Abs(instance.Insertion.X - interfaceX) < 1e-6)
                .ToList();
            Assert.Equal(2, coincident.Select(PushBackPlanComposer.PhysicalKey).Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void CompositePlant_EncontradasPreserveDistinctPhysicalBedBeams()
        {
            // Con hueco, las dos camas encontradas ya ni siquiera comparten punto: la prueba vale para los dos casos
            // y protege el contrato de que su numero no depende del hueco.
            foreach (var gap in new[] { 0.0, 12.0 })
            {
                var system = Build(PushBackCellTopology.Encontradas, PushBackCellTopology.Encontradas, gap: gap);

                Assert.Equal(2, Beams(system, HighBeam).Count);       // un alto por cama
                Assert.Equal(2, Beams(system, InOutBeam).Count);      // y una entrada por pasillo
            }
        }

        // ---------------------------------------------------------------- niveles distintos

        [Fact]
        public void CompositePlant_DistinctLevelsProjectOneSymbolButRemainTwoPieces()
        {
            // Dos niveles son dos largueros fisicos a distinta elevacion. La planta dibuja UN simbolo —colapsa la
            // elevacion— y el BOM sigue comprando las dos camas: la vista no decide cuantas piezas hay.
            var control = Build(PushBackCellTopology.SoloA, PushBackCellTopology.SoloA);
            var runs = PushBackRuns.Resolve(control);

            Assert.Equal(2, runs.Runs.Count);                  // dos camas fisicas, una por nivel
            Assert.Equal(1, At(control, InOutBeam, 0.0));      // un solo simbolo en planta

            var beds = PushBackBomBuilder.Build(control, Catalog).Components
                .Where(component => component.Category != null && component.Category.Contains("Cama"))
                .Sum(component => component.Quantity);
            Assert.True(beds >= 2, "el BOM sigue contando una cama por nivel");
        }

        // ---------------------------------------------------------------- el BOM no se toca

        [Fact]
        public void PlantDeduplication_DoesNotChangeTheBom()
        {
            var mixed = Build(PushBackCellTopology.SoloA, PushBackCellTopology.Corrida);
            var bom = PushBackBomBuilder.Build(mixed, Catalog);

            // Las dos camas siguen en el BOM aunque la planta dibuje un solo larguero de entrada.
            Assert.Equal(2, PushBackRuns.Resolve(mixed).Runs.Count);
            Assert.NotEmpty(bom.Lines);
            Assert.Contains(bom.Lines, line => string.Equals(
                line.ProfileId, InOutBeam, StringComparison.OrdinalIgnoreCase));
        }

        // ---------------------------------------------------------------- un rack de un solo sentido

        [Fact]
        public void SingleSidedPlant_IsUnchanged()
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 3, deepB: 3, levelsA: 2, levelsB: 2, gap: 0.0);
            design.Composite.DefaultTopology = PushBackCellTopology.SoloA;
            var system = new PushBackResolver(Catalog).Resolve(design);

            // Cada ranura dibuja su entrada y su alto una sola vez, como siempre.
            Assert.All(
                Plant(system)
                    .Where(instance => instance.Role == HeaderBlockRole.Beam)
                    .GroupBy(PushBackPlanComposer.PhysicalKey),
                group => Assert.Single(group));
        }
    }
}
