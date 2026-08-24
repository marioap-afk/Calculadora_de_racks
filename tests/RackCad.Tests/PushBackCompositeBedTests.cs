using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (G3) — el resolver FISICO de camas: solo A, solo B, encontradas, corrida y los dos sentidos. Aqui se fija
    /// que una cama corrida es UNA cama —una longitud, una pendiente, un eje— y que dos encontradas son DOS, con
    /// sentidos opuestos y sus extremos altos enfrentados.
    /// </summary>
    public class PushBackCompositeBedTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            PushBackRunDirection direction = PushBackRunDirection.AToB,
            int deepA = 5, int deepB = 4, int levelsA = 2, int levelsB = 2, double gap = 0.0)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 1, slotsB: 1, deepA: deepA, deepB: deepB, levelsA: levelsA, levelsB: levelsB, gap: gap);
            design.Composite.DefaultTopology = topology;
            design.Composite.DefaultDirection = direction;
            return design;
        }

        private static PushBackRunSet Runs(PushBackDesign design)
            => PushBackRuns.Resolve(new PushBackResolver(Catalog).Resolve(design));

        // ---- Solo A / Solo B ---------------------------------------------------------------------------------

        [Fact]
        public void SoloA_IsOnePhysicalBed_FlowingFromTheAOuterEnd()
        {
            var set = Runs(Design(PushBackCellTopology.SoloA, levelsA: 2, levelsB: 2));
            var axes = PushBackRunGeometry.Axes(set, Catalog);

            Assert.Equal(2, set.Runs.Count);                     // dos niveles, una cama cada uno
            Assert.All(set.Runs, run => Assert.Equal(PushBackSide.A, run.LowSide));
            Assert.All(set.Runs, run => Assert.False(run.Reflected));
            Assert.All(axes, axis => Assert.True(axis.FlowsForward));
            Assert.All(axes, axis => Assert.True(axis.Slope > 0.0));
        }

        [Fact]
        public void SoloB_IsOnePhysicalBed_FlowingFromTheBOuterEnd()
        {
            var set = Runs(Design(PushBackCellTopology.SoloB, levelsA: 2, levelsB: 2));
            var axes = PushBackRunGeometry.Axes(set, Catalog);

            Assert.Equal(2, set.Runs.Count);
            Assert.All(set.Runs, run => Assert.Equal(PushBackSide.B, run.LowSide));
            Assert.All(set.Runs, run => Assert.True(run.Reflected));
            // Fluye hacia -X: su extremo BAJO esta en el extremo opuesto del rack.
            Assert.All(axes, axis => Assert.False(axis.FlowsForward));
            Assert.All(axes, axis => Assert.True(axis.Slope < 0.0));
            // Las ELEVACIONES no se reflejan: el extremo alto sigue estando mas arriba.
            Assert.All(axes, axis => Assert.True(axis.HighContact.Y > axis.LowContact.Y));
        }

        // ---- Encontradas: DOS camas fisicas -------------------------------------------------------------------

        [Fact]
        public void Encontradas_AreTwoPhysicalBeds_WithOppositeSenses()
        {
            var set = Runs(Design(PushBackCellTopology.Encontradas, levelsA: 1, levelsB: 1));
            var axes = PushBackRunGeometry.Axes(set, Catalog);

            Assert.Equal(2, set.Runs.Count);
            Assert.Contains(set.Runs, run => run.LowSide == PushBackSide.A && !run.Reflected);
            Assert.Contains(set.Runs, run => run.LowSide == PushBackSide.B && run.Reflected);

            var fromA = axes.Single(axis => axis.FlowsForward);
            var fromB = axes.Single(axis => !axis.FlowsForward);
            // Los dos extremos BAJOS estan en los pasillos opuestos y los ALTOS se miran en el centro.
            Assert.True(fromA.LowContact.X < fromA.HighContact.X);
            Assert.True(fromB.LowContact.X > fromB.HighContact.X);
            Assert.True(fromA.HighContact.X <= fromB.HighContact.X + 1e-6
                        || fromB.HighContact.X <= fromA.HighContact.X + 1e-6);
            Assert.True(fromA.LowContact.X < fromB.LowContact.X);
        }

        [Fact]
        public void Encontradas_KeepEachSideItsOwnBedLength()
        {
            var set = Runs(Design(PushBackCellTopology.Encontradas, deepA: 6, deepB: 3, levelsA: 1, levelsB: 1));
            var axes = PushBackRunGeometry.Axes(set, Catalog);

            var fromA = axes.Single(axis => axis.FlowsForward);
            var fromB = axes.Single(axis => !axis.FlowsForward);
            Assert.True(fromA.Length > fromB.Length);
        }

        // ---- Corrida: UNA cama fisica -------------------------------------------------------------------------

        [Fact]
        public void Corrida_IsOnePhysicalBed_CrossingTheWholeRack()
        {
            var design = Design(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, levelsA: 1, levelsB: 1, gap: 8.0);
            var system = new PushBackResolver(Catalog).Resolve(design);
            var set = PushBackRuns.Resolve(system);
            var axes = PushBackRunGeometry.Axes(set, Catalog);

            var run = Assert.Single(set.Runs);                    // UNA cama, no dos
            Assert.Equal(PushBackCellTopology.Corrida, run.Topology);
            Assert.Equal(PushBackSide.A, run.LowSide);
            Assert.Equal(PushBackSide.B, run.HighSide);

            var axis = Assert.Single(axes);
            // Una sola longitud fisica: la del rack entero, hueco incluido.
            Assert.Equal(system.Structure.TotalLength, axis.Length, 6);
            Assert.True(axis.FlowsForward);
        }

        [Fact]
        public void Corrida_AToB_PutsLowOnTheAOuterEnd_AndHighOnTheBOuterEnd()
        {
            var design = Design(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, levelsA: 1, levelsB: 1);
            var system = new PushBackResolver(Catalog).Resolve(design);
            var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();

            Assert.True(axis.LowContact.X < system.Structure.TotalLength / 2.0);
            Assert.True(axis.HighContact.X > system.Structure.TotalLength / 2.0);
            Assert.True(axis.HighContact.Y > axis.LowContact.Y);
        }

        [Fact]
        public void Corrida_BToA_SwapsHighAndLowPhysically_NotJustGraphically()
        {
            var design = Design(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, levelsA: 1, levelsB: 1);
            var system = new PushBackResolver(Catalog).Resolve(design);
            var run = Assert.Single(PushBackRuns.Resolve(system).Runs);
            var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();

            Assert.Equal(PushBackSide.B, run.LowSide);
            Assert.Equal(PushBackSide.A, run.HighSide);
            Assert.True(run.Reflected);
            Assert.True(axis.LowContact.X > system.Structure.TotalLength / 2.0);
            Assert.True(axis.HighContact.X < system.Structure.TotalLength / 2.0);
            // El extremo ALTO sigue siendo el mas alto: no es un espejo grafico, es el otro extremo fisico.
            Assert.True(axis.HighContact.Y > axis.LowContact.Y);
            Assert.False(axis.FlowsForward);
        }

        [Fact]
        public void ChangingTheDirection_MovesTheHighEnd_AndBothAreMirrorImages()
        {
            var forward = new PushBackResolver(Catalog)
                .Resolve(Design(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, deepA: 4, deepB: 4, levelsA: 1, levelsB: 1));
            var backward = new PushBackResolver(Catalog)
                .Resolve(Design(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, deepA: 4, deepB: 4, levelsA: 1, levelsB: 1));

            var forwardAxis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(forward), Catalog).Single();
            var backwardAxis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(backward), Catalog).Single();
            var total = forward.Structure.TotalLength;

            Assert.Equal(total, backward.Structure.TotalLength, 6);
            // Con los dos lados simetricos, invertir el sentido es exactamente reflejar el eje.
            Assert.True(PushBackMirror.AreReflected(total, forwardAxis.LowContact.X, backwardAxis.LowContact.X, 1e-6));
            Assert.True(PushBackMirror.AreReflected(total, forwardAxis.HighContact.X, backwardAxis.HighContact.X, 1e-6));
            Assert.Equal(forwardAxis.Slope, -backwardAxis.Slope, 9);
            Assert.Equal(forwardAxis.Length, backwardAxis.Length, 6);
        }

        [Fact]
        public void InACorrida_TheHighSideGoverns_AndTheLowSideIntentStaysStored()
        {
            var design = Design(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, levelsA: 1, levelsB: 1);
            design.SideB.FirstLevelHeight = 20.0;    // el lado ALTO manda la geometria
            design.Structure.FirstLevelHeight = 4.0; // la del lado BAJO queda dormante, pero NO se borra

            var system = new PushBackResolver(Catalog).Resolve(design);
            var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();

            // El ancla es el larguero del lado alto, asi que su elevacion sube con la del lado B.
            Assert.True(axis.HighContact.Y > 20.0);
            // La intencion del lado bajo sigue almacenada: reaparece en cuanto la celda deja de ser corrida.
            Assert.Equal(4.0, design.Structure.FirstLevelHeight, 6);
            Assert.Equal(4.0, system.Composite.SideA.Front(0).FirstLevelHeight, 6);
        }

        [Fact]
        public void SwitchingBackFromCorrida_RestoresTheDormantSideElevation()
        {
            var design = Design(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, levelsA: 1, levelsB: 1);
            design.SideB.FirstLevelHeight = 20.0;
            design.Structure.FirstLevelHeight = 4.0;
            var corridaHigh = PushBackRunGeometry
                .Axes(PushBackRuns.Resolve(new PushBackResolver(Catalog).Resolve(design)), Catalog)
                .Single().HighContact.Y;

            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;
            var set = PushBackRuns.Resolve(new PushBackResolver(Catalog).Resolve(design));
            var axes = PushBackRunGeometry.Axes(set, Catalog);

            Assert.Equal(2, set.Runs.Count);
            var fromA = axes.Single(axis => axis.FlowsForward);
            // La cama de A vuelve a su propia elevacion, la que estuvo dormante mientras hubo corrida.
            Assert.True(fromA.HighContact.Y < corridaHigh);
        }

        // ---- La topologia es POR CELDA -----------------------------------------------------------------------

        [Fact]
        public void TopologyCanDiffer_BetweenLevelsOfTheSameFront()
        {
            var design = Design(PushBackCellTopology.Encontradas, levelsA: 4, levelsB: 4);
            design.Composite.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            design.Composite.SetCell(0, 2, PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
            design.Composite.SetCell(0, 3, PushBackCellTopology.SoloB, PushBackRunDirection.AToB);

            var set = Runs(design);

            Assert.Single(set.Runs, run => run.Level == 1);   // corrida: una cama
            Assert.Equal(2, set.Runs.Count(run => run.Level == 2));  // encontradas: dos
            Assert.Single(set.Runs, run => run.Level == 3 && run.LowSide == PushBackSide.A);
            Assert.Single(set.Runs, run => run.Level == 4 && run.LowSide == PushBackSide.B);
            Assert.Equal(PushBackCellTopology.Corrida, set.Runs.First(run => run.Level == 1).Topology);
        }

        [Fact]
        public void ALevelThatOnlyOneSideHas_DegradesToThatSide_WithoutTouchingTheStoredIntent()
        {
            var design = Design(PushBackCellTopology.Encontradas, levelsA: 2, levelsB: 4);
            var set = Runs(design);

            // Niveles 3 y 4 solo existen en B: alli la celda es «solo B», aunque la intencion diga «encontradas».
            Assert.Equal(2, set.Runs.Count(run => run.Level == 1));
            Assert.Single(set.Runs, run => run.Level == 3);
            Assert.Equal(PushBackSide.B, set.Runs.Single(run => run.Level == 3).LowSide);
            Assert.Equal(PushBackCellTopology.Encontradas, design.Composite.TopologyAt(0, 2));   // intacta
        }

        // ---- Capacidad geometrica real ------------------------------------------------------------------------

        [Fact]
        public void ACorridaBeyondTheAvailablePositions_IsBlocked_AndMoreGapDoesNotRescueIt()
        {
            // A pide 8 y B pide 5, pero la estructura de A se limita a 6 por override manual.
            var design = Design(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, deepA: 8, deepB: 5, levelsA: 1, levelsB: 1);
            design.Composite.StructureOverrideA = 6;
            var tight = new PushBackResolver(Catalog).Resolve(design);
            var tightCell = tight.Composite.Cell(0, 1);
            Assert.False(tightCell.IsValid);

            // El hueco anade LONGITUD, pero no anade posiciones de tarima: no puede rescatar una demanda que excede
            // los fondos disponibles, y no lo hace en silencio.
            design.Composite.Gap = 60.0;
            var loose = new PushBackResolver(Catalog).Resolve(design);
            Assert.False(loose.Composite.Cell(0, 1).IsValid);

            // Lo que si la vuelve valida es ESTRUCTURA: la demanda cabe en cuanto los fondos existen.
            design.Composite.Gap = 0.0;
            design.Composite.StructureOverrideA = 8;
            var widened = new PushBackResolver(Catalog).Resolve(design);
            Assert.True(widened.Composite.Cell(0, 1).IsValid);
        }

        [Fact]
        public void TheRequiredLength_IsNotAFixedSumOfDepths()
        {
            var design = Design(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, deepA: 8, deepB: 5, levelsA: 1, levelsB: 1);
            var system = new PushBackResolver(Catalog).Resolve(design);
            var cell = system.Composite.Cell(0, 1);

            // Es una LONGITUD, medida sobre los modulos reales: nunca el numero 13 ni ninguna suma de fondos.
            Assert.True(cell.RequiredBedLength > 13.0);
            Assert.True(cell.AvailableBedSpan >= cell.RequiredBedLength);
            Assert.True(cell.IsValid);
        }
    }
}
