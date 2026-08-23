using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-41 · PB-015 — FONDO INDEPENDIENTE POR CELDA Push Back.
    ///
    /// Fija la regla completa: precedencia <c>override ?? default</c>, la envolvente derivada del frente, el consumo
    /// del fondo efectivo por toda la geometria y el BOM, las cinco operaciones de alcance, la restauracion, el
    /// comportamiento determinista al agregar/quitar frentes y niveles, el round trip y la preservacion de I-40.
    ///
    /// Todas estas pruebas FALLAN sin la implementacion de I-41 (antes de ella el fondo era una propiedad del frente y
    /// ni el modelo ni la persistencia tenian donde guardar un override).
    /// </summary>
    public class PushBackCellDepthTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ---- Helpers ----------------------------------------------------------------------------------------

        /// <summary>Un estado de editor con <paramref name="fronts"/> frentes de <paramref name="levels"/> niveles.</summary>
        private static (PushBackEditorState State, PushBackEditorInputs Inputs, PushBackEditorDesignAssembler Assembler)
            Editor(int fronts = 1, int levels = 3, int palletsDeep = 4)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.PalletsDeep = palletsDeep;
            state.SetFrontCount(fronts);
            for (var front = 0; front < fronts; front++)
            {
                state.Structure.Fronts[front].LoadLevels = levels;
                state.Structure.Fronts[front].PalletsDeep = palletsDeep;
                state.AdjustLevels(front, 0);   // re-sincroniza las celdas paralelas
            }

            return (state, inputs, assembler);
        }

        private static PushBackSystem Resolve(PushBackEditorState state, PushBackEditorInputs inputs,
            PushBackEditorDesignAssembler assembler)
        {
            var computation = assembler.Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            return computation.System;
        }

        private static void Select(PushBackEditorState state, int frontIndex, int levelIndex, bool extend = false)
            => state.ToggleCell(frontIndex, levelIndex, extend);

        // ---- La regla de precedencia, desnuda ----------------------------------------------------------------

        [Theory]
        [InlineData(null, 4, 4)]   // sin override: hereda el default del frente
        [InlineData(6, 4, 6)]      // override mayor
        [InlineData(2, 5, 2)]      // override menor
        [InlineData(1, 5, 2)]      // por debajo del minimo fisico se acota a 2, nunca a 1
        [InlineData(null, 1, 2)]   // un default invalido tampoco baja de 2
        public void Effective_IsOverrideThenFrontDefault_NeverBelowTwo(int? cellOverride, int frontDefault, int expected)
            => Assert.Equal(expected, PushBackCellDepth.Effective(cellOverride, frontDefault));

        [Fact]
        public void Envelope_IsTheDeepestActiveLevel_NotTheFrontDefault()
        {
            var overrides = new List<int?> { null, 7, 3 };

            Assert.Equal(7, PushBackCellDepth.Envelope(frontDefault: 4, overrides, activeLevels: 3));
            // Con menos niveles activos, el nivel profundo no cuenta.
            Assert.Equal(4, PushBackCellDepth.Envelope(frontDefault: 4, overrides, activeLevels: 1));
            // Un frente en blanco (0 niveles activos) conserva su propia estructura.
            Assert.Equal(4, PushBackCellDepth.Envelope(frontDefault: 4, overrides, activeLevels: 0));
        }

        // ---- Fondos distintos por celda llegan al modelo resuelto --------------------------------------------

        [Fact]
        public void DifferentDepthsPerCell_SurviveIntoTheResolvedSystem()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 4);
            Select(state, 0, 1);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);

            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 0));
            Assert.Equal(6, system.EffectivePalletsDeepAt(0, 1));
            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 2));
            Assert.Equal(4, system.DefaultPalletsDeepAt(0));
        }

        [Fact]
        public void TheFrontStructuralPalletsDeep_BecomesTheDerivedEnvelope()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 4);
            Select(state, 0, 2);
            state.ApplyPalletsDeep(7, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);

            // La estructura se dimensiona por el nivel mas profundo...
            Assert.Equal(7, system.Structure.Fronts[0].PalletsDeep);
            // ...pero el fondo POR DEFECTO del frente sigue siendo el que el usuario escribio.
            Assert.Equal(4, system.DefaultPalletsDeepAt(0));
        }

        [Fact]
        public void AnOverrideDeeperThanTheStructure_IsClampedToTheFrontsEnvelope()
        {
            // Un documento incoherente —un override mas profundo que la estructura que lo sostiene— no puede colgar
            // un larguero en el aire: el resolver lo acota a la envolvente que el frente REALMENTE tiene.
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            var config = new PushBackFrontConfig { DefaultPalletsDeep = 4 };
            config.PalletsDeepOverrides.Add(99);
            config.PalletsDeepOverrides.Add(null);
            design.Fronts.Add(config);

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 0));
            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 1));
            // Y su larguero posterior sigue apoyado en el ultimo modulo del frente, no fuera del rack.
            Assert.Equal(
                system.Structure.Fronts[0].EndX,
                PushBackCellDepth.RearX(system, system.Structure.Fronts[0], 1), 6);
        }

        // ---- La geometria consume el fondo EFECTIVO ---------------------------------------------------------

        [Fact]
        public void StaggeredCells_PlaceTheirRearBeamsAtDifferentX()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 4);
            Select(state, 0, 0);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);
            var front = system.Structure.Fronts[0];
            var rear = PushBackLoadBeamGeometry.HighBeams(system, Catalog, 0, front)
                .OrderBy(beam => beam.Insertion.Y)
                .ToList();

            Assert.Equal(3, rear.Count);
            // El nivel 1 termina antes que los otros dos: su X es estrictamente menor.
            Assert.True(rear[0].Insertion.X < rear[1].Insertion.X,
                $"nivel 1 en X={rear[0].Insertion.X}, nivel 2 en X={rear[1].Insertion.X}");
            Assert.Equal(rear[1].Insertion.X, rear[2].Insertion.X, 6);
            Assert.Equal(front.EndX, rear[1].Insertion.X, 6);
        }

        [Fact]
        public void StaggeredCells_GiveEachLevelItsOwnBedLength()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            Select(state, 0, 0);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);
            var front = system.Structure.Fronts[0];

            var shallow = PushBackCellDepth.BedLength(system, front, 1);
            var deep = PushBackCellDepth.BedLength(system, front, 2);

            Assert.True(shallow > 0.0 && deep > 0.0);
            Assert.True(shallow < deep, $"cama nivel 1 = {shallow}, cama nivel 2 = {deep}");
            Assert.Equal(front.EndX - front.StartX, deep, 6);
        }

        [Fact]
        public void StaggeredCells_ProduceOneBedDefinitionPerDistinctDepth()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 4);
            Select(state, 0, 0);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);
            var front = system.Structure.Fronts[0];

            var groups = new PushBackFlowBedLateralBuilder().BuildLateralGroups(system, Catalog, front);

            // Dos fondos distintos (2 y 4) => dos definiciones anidadas; la profunda agrupa dos niveles.
            Assert.Equal(2, groups.Count);
            Assert.Equal(new[] { 1, 2 }, groups.Select(g => g.Placements.Count).OrderBy(count => count).ToArray());
        }

        [Fact]
        public void WithoutAnyOverride_TheBedStillProducesASingleSharedDefinition()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 4);

            var system = Resolve(state, inputs, assembler);
            var front = system.Structure.Fronts[0];

            var groups = new PushBackFlowBedLateralBuilder().BuildLateralGroups(system, Catalog, front);

            var group = Assert.Single(groups);
            Assert.Equal(3, group.Placements.Count);      // el patron ARRAY se conserva
            Assert.Equal("Cama push back F1", group.Name); // y tambien el nombre historico
        }

        [Fact]
        public void ShallowCells_DropTheIntermediateSupportsBehindTheirRearBeam()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 6);
            var deepOnly = Resolve(state, inputs, assembler);
            var deepCount = new PushBackIntermediateBeamLateralBuilder()
                .Build(deepOnly, Catalog, postIndex: 0).Flatten().Instances.Count;

            Select(state, 0, 0);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);
            var staggered = Resolve(state, inputs, assembler);
            var staggeredCount = new PushBackIntermediateBeamLateralBuilder()
                .Build(staggered, Catalog, postIndex: 0).Flatten().Instances.Count;

            Assert.True(staggeredCount < deepCount,
                $"con un nivel corto deberia haber MENOS intermedios: {staggeredCount} vs {deepCount}");
        }

        // ---- Geometria escalonada con VARIOS frentes y niveles, en las cuatro vistas ------------------------

        [Fact]
        public void SeveralFrontsAndLevels_EachCellKeepsItsOwnRearXAndBedLength()
        {
            var (state, inputs, assembler) = Editor(fronts: 3, levels: 3, palletsDeep: 5);
            // Un escalonado distinto en cada frente.
            Select(state, 0, 0); state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);
            Select(state, 1, 1); state.ApplyPalletsDeep(3, DynamicRackCellScope.Cell);
            Select(state, 2, 2); state.ApplyPalletsDeep(7, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);

            Assert.Equal(new[] { 2, 5, 5 }, Enumerable.Range(0, 3).Select(l => system.EffectivePalletsDeepAt(0, l)).ToArray());
            Assert.Equal(new[] { 5, 3, 5 }, Enumerable.Range(0, 3).Select(l => system.EffectivePalletsDeepAt(1, l)).ToArray());
            Assert.Equal(new[] { 5, 5, 7 }, Enumerable.Range(0, 3).Select(l => system.EffectivePalletsDeepAt(2, l)).ToArray());

            // Envolventes: solo el tercer frente crece.
            Assert.Equal(5, system.Structure.Fronts[0].PalletsDeep);
            Assert.Equal(5, system.Structure.Fronts[1].PalletsDeep);
            Assert.Equal(7, system.Structure.Fronts[2].PalletsDeep);

            // Y cada celda coloca su posterior donde su fondo dice, nunca donde lo dice el frente.
            for (var frontIndex = 0; frontIndex < 3; frontIndex++)
            {
                var front = system.Structure.Fronts[frontIndex];
                for (var level = 1; level <= 3; level++)
                {
                    var expectedDeep = system.EffectivePalletsDeepAt(frontIndex, level - 1);
                    var rearX = PushBackCellDepth.RearX(system, front, level);
                    var bed = PushBackCellDepth.BedLength(system, front, level);
                    Assert.Equal(rearX - front.StartX, bed, 6);
                    Assert.True(bed > 0.0);
                    if (expectedDeep == front.PalletsDeep)
                    {
                        Assert.Equal(front.EndX, rearX, 6);   // el nivel mas profundo llega al final del frente
                    }
                    else
                    {
                        Assert.True(rearX < front.EndX, $"F{frontIndex + 1} N{level} deberia terminar antes");
                    }
                }
            }
        }

        [Fact]
        public void TheFourViews_AllBuildWithStaggeredDepths_AndTheRearCutShowsEveryCell()
        {
            var (state, inputs, assembler) = Editor(fronts: 2, levels: 3, palletsDeep: 4);
            Select(state, 0, 0); state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);
            Select(state, 1, 2); state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);

            var computation = assembler.Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);

            // Las cuatro vistas se construyen y ninguna queda vacia.
            Assert.NotEmpty(computation.LateralPlan.Flatten().Instances);
            Assert.NotEmpty(computation.FrontalEntradaSalida.Flatten().Instances);
            Assert.NotEmpty(computation.FrontalPosterior.Flatten().Instances);
            Assert.NotEmpty(computation.PlantaPlan.Flatten().Instances);
            Assert.NotEmpty(computation.LateralCortes);

            // El corte POSTERIOR es un CORTE (I-40): muestra el larguero redondo de CADA celda, no una envolvente.
            var redondos = computation.FrontalPosterior.Flatten().Instances
                .Count(instance => string.Equals(instance.PieceId, PushBackDefaults.HighEndBeamCatalogId,
                    StringComparison.OrdinalIgnoreCase));
            Assert.Equal(6, redondos);   // 2 frentes x 3 niveles

            // La PLANTA colapsa los niveles, asi que su linea posterior es la ENVOLVENTE del frente.
            Assert.Equal(4, computation.System.Structure.Fronts[0].PalletsDeep);
            Assert.Equal(6, computation.System.Structure.Fronts[1].PalletsDeep);
        }

        // ---- El BOM cotiza la cama de cada celda ------------------------------------------------------------

        [Fact]
        public void TheBom_QuotesOneBedLengthPerCell_NotOnePerFront()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            Select(state, 0, 0);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);
            var camas = PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == "Cama")
                .OrderBy(component => component.Length)
                .ToList();

            Assert.Equal(2, camas.Count);
            Assert.True(camas[0].Length < camas[1].Length);
            Assert.Equal(1, camas[0].Quantity);
            Assert.Equal(1, camas[1].Quantity);
            Assert.Equal(
                PushBackCellDepth.BedLength(system, system.Structure.Fronts[0], 1),
                camas[0].Length, 3);
        }

        // ---- Las cinco operaciones de alcance ---------------------------------------------------------------

        [Fact]
        public void ApplyDepth_ToTheCell_WritesOnlyThatCell()
        {
            var (state, _, _) = Editor(levels: 3, palletsDeep: 4);
            Select(state, 0, 1);

            Assert.Equal(1, state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell));

            Assert.Null(state.Cell(0, 0).PalletsDeepOverride);
            Assert.Equal(6, state.Cell(0, 1).PalletsDeepOverride);
            Assert.Null(state.Cell(0, 2).PalletsDeepOverride);
        }

        [Fact]
        public void ApplyDepth_ToTheSelection_WritesTheCtrlClickedCellsOnly()
        {
            var (state, _, _) = Editor(fronts: 2, levels: 3, palletsDeep: 4);
            Select(state, 0, 0);
            Select(state, 1, 2, extend: true);

            Assert.Equal(2, state.ApplyPalletsDeep(6, DynamicRackCellScope.Selected));

            Assert.Equal(6, state.Cell(0, 0).PalletsDeepOverride);
            Assert.Equal(6, state.Cell(1, 2).PalletsDeepOverride);
            Assert.Null(state.Cell(0, 1).PalletsDeepOverride);
            Assert.Null(state.Cell(1, 0).PalletsDeepOverride);
        }

        [Fact]
        public void ApplyDepth_ToTheLevel_WritesThatLevelAcrossEveryFront()
        {
            var (state, _, _) = Editor(fronts: 3, levels: 3, palletsDeep: 4);
            Select(state, 1, 1);

            Assert.Equal(3, state.ApplyPalletsDeep(6, DynamicRackCellScope.Level));

            Assert.Equal(6, state.Cell(0, 1).PalletsDeepOverride);
            Assert.Equal(6, state.Cell(1, 1).PalletsDeepOverride);
            Assert.Equal(6, state.Cell(2, 1).PalletsDeepOverride);
            Assert.Null(state.Cell(0, 0).PalletsDeepOverride);
            Assert.Null(state.Cell(2, 2).PalletsDeepOverride);
        }

        [Fact]
        public void ApplyDepth_ToTheFront_WritesEveryLevelOfThatFrontOnly()
        {
            var (state, _, _) = Editor(fronts: 2, levels: 3, palletsDeep: 4);
            Select(state, 1, 0);

            Assert.Equal(3, state.ApplyPalletsDeep(5, DynamicRackCellScope.Front));

            Assert.All(Enumerable.Range(0, 3), level => Assert.Equal(5, state.Cell(1, level).PalletsDeepOverride));
            Assert.All(Enumerable.Range(0, 3), level => Assert.Null(state.Cell(0, level).PalletsDeepOverride));
        }

        [Fact]
        public void ApplyDepth_ToAll_WritesTheWholeGrid()
        {
            var (state, _, _) = Editor(fronts: 2, levels: 2, palletsDeep: 4);
            Select(state, 0, 0);

            Assert.Equal(4, state.ApplyPalletsDeep(5, DynamicRackCellScope.All));

            for (var front = 0; front < 2; front++)
            {
                for (var level = 0; level < 2; level++)
                {
                    Assert.Equal(5, state.Cell(front, level).PalletsDeepOverride);
                }
            }
        }

        [Fact]
        public void ApplyDepth_ChangesOnlyTheDepth_NeverAnotherFieldOfTheSourceCell()
        {
            var (state, _, _) = Editor(fronts: 1, levels: 2, palletsDeep: 4);
            // Dos celdas deliberadamente DISTINTAS en todo lo demas.
            state.Structure.Fronts[0].Cells[0].ClearHeight = 8.0;
            state.Structure.Fronts[0].Cells[1].ClearHeight = 12.0;
            state.Structure.Fronts[0].Cells[0].PalletHeight = 60.0;
            state.Structure.Fronts[0].Cells[1].PalletHeight = 48.0;
            state.Cell(0, 0).HighEndBeamPeralte = 4.0;
            state.Cell(0, 1).HighEndBeamPeralte = 5.0;

            Select(state, 0, 0);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Front);

            // El fondo viajo a las dos celdas...
            Assert.Equal(6, state.Cell(0, 0).PalletsDeepOverride);
            Assert.Equal(6, state.Cell(0, 1).PalletsDeepOverride);
            // ...y NADA mas de la celda origen se copio.
            Assert.Equal(12.0, state.Structure.Fronts[0].Cells[1].ClearHeight);
            Assert.Equal(48.0, state.Structure.Fronts[0].Cells[1].PalletHeight);
            Assert.Equal(5.0, state.Cell(0, 1).HighEndBeamPeralte);
        }

        [Fact]
        public void AnIndividualOverride_SurvivesAfterAMassApplication()
        {
            var (state, inputs, assembler) = Editor(fronts: 1, levels: 3, palletsDeep: 4);
            Select(state, 0, 0);
            state.ApplyPalletsDeep(5, DynamicRackCellScope.All);
            Select(state, 0, 2);
            state.ApplyPalletsDeep(8, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);

            Assert.Equal(5, system.EffectivePalletsDeepAt(0, 0));
            Assert.Equal(5, system.EffectivePalletsDeepAt(0, 1));
            Assert.Equal(8, system.EffectivePalletsDeepAt(0, 2));
        }

        // ---- Restauracion -----------------------------------------------------------------------------------

        [Fact]
        public void RestoringTheDepth_RemovesTheOverrideAndReturnsToTheFrontDefault()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            Select(state, 0, 0);
            state.ApplyPalletsDeep(7, DynamicRackCellScope.Cell);
            Assert.Equal(7, Resolve(state, inputs, assembler).EffectivePalletsDeepAt(0, 0));

            state.ApplyPalletsDeep(null, DynamicRackCellScope.Cell);

            Assert.Null(state.Cell(0, 0).PalletsDeepOverride);
            var system = Resolve(state, inputs, assembler);
            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 0));
            // Y la envolvente vuelve a bajar con el: no queda estructura sobrante.
            Assert.Equal(4, system.Structure.Fronts[0].PalletsDeep);
        }

        // ---- Determinismo al agregar/quitar niveles y frentes -----------------------------------------------

        [Fact]
        public void AddingALevel_ClonesTheLastCellsDepth_AndRemovingItLeavesNoOrphanOverride()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            Select(state, 0, 1);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);

            state.AdjustLevels(0, 1);   // el nivel nuevo clona el ultimo, como toda la matriz
            Assert.Equal(6, state.Cell(0, 2).PalletsDeepOverride);

            state.AdjustLevels(0, -1);
            var system = Resolve(state, inputs, assembler);

            Assert.Equal(2, system.HighEndBeams[0].PalletsDeep.Count);   // sin celda huerfana
            Assert.Equal(6, system.EffectivePalletsDeepAt(0, 1));
            Assert.Equal(6, system.Structure.Fronts[0].PalletsDeep);
        }

        [Fact]
        public void RemovingTheDeepestLevel_ShrinksTheEnvelopeBackDown()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 4);
            Select(state, 0, 2);
            state.ApplyPalletsDeep(9, DynamicRackCellScope.Cell);
            Assert.Equal(9, Resolve(state, inputs, assembler).Structure.Fronts[0].PalletsDeep);

            state.AdjustLevels(0, -1);   // se va el nivel profundo

            Assert.Equal(4, Resolve(state, inputs, assembler).Structure.Fronts[0].PalletsDeep);
        }

        [Fact]
        public void AddingAFront_ClonesTheSelectedFrontsOverrides_AndRemovingItLeavesNone()
        {
            var (state, inputs, assembler) = Editor(fronts: 1, levels: 2, palletsDeep: 4);
            Select(state, 0, 0);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);

            state.SetFrontCount(2);
            Assert.Equal(6, state.Cell(1, 0).PalletsDeepOverride);

            state.SetFrontCount(1);
            var system = Resolve(state, inputs, assembler);

            Assert.Single(system.HighEndBeams);
            Assert.Equal(6, system.EffectivePalletsDeepAt(0, 0));
        }

        // ---- Round trip -------------------------------------------------------------------------------------

        [Fact]
        public void SaveLoad_PreservesTheFrontDefaultAndEveryCellOverride()
        {
            var (state, inputs, assembler) = Editor(fronts: 2, levels: 3, palletsDeep: 4);
            Select(state, 0, 1);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);
            Select(state, 1, 2);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);

            var design = assembler.BuildDesign(state, inputs);
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var reloaded = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
            var system = new PushBackResolver(Catalog).Resolve(reloaded);

            Assert.Equal(4, system.DefaultPalletsDeepAt(0));
            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 0));
            Assert.Equal(6, system.EffectivePalletsDeepAt(0, 1));
            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 2));
            Assert.Equal(2, system.EffectivePalletsDeepAt(1, 2));
        }

        [Fact]
        public void ReopeningTheEditor_RestoresTheDefaultAndTheOverrides_Idempotently()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 4);
            Select(state, 0, 1);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);
            var design = assembler.BuildDesign(state, inputs);

            var reopened = new PushBackEditorState();
            reopened.LoadFromDesign(design, assembler.Resolver);

            Assert.Equal(4, reopened.Structure.Fronts[0].PalletsDeep);   // el DEFAULT, no la envolvente
            Assert.Null(reopened.Cell(0, 0).PalletsDeepOverride);
            Assert.Equal(6, reopened.Cell(0, 1).PalletsDeepOverride);
            Assert.Null(reopened.Cell(0, 2).PalletsDeepOverride);

            // Un segundo ciclo no debe mover nada (la envolvente no puede comerse el default).
            var again = new PushBackEditorState();
            again.LoadFromDesign(assembler.BuildDesign(reopened, inputs), assembler.Resolver);
            Assert.Equal(4, again.Structure.Fronts[0].PalletsDeep);
            Assert.Equal(6, again.Cell(0, 1).PalletsDeepOverride);
        }

        [Fact]
        public void ResolveSnapshotResolve_IsStable()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            Select(state, 0, 0);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);

            var resolver = new PushBackResolver(Catalog);
            var first = resolver.Resolve(assembler.BuildDesign(state, inputs));
            var second = resolver.Resolve(resolver.Snapshot(first));

            Assert.Equal(first.DefaultPalletsDeepAt(0), second.DefaultPalletsDeepAt(0));
            Assert.Equal(first.EffectivePalletsDeepAt(0, 0), second.EffectivePalletsDeepAt(0, 0));
            Assert.Equal(first.EffectivePalletsDeepAt(0, 1), second.EffectivePalletsDeepAt(0, 1));
            Assert.Equal(first.Structure.Fronts[0].PalletsDeep, second.Structure.Fronts[0].PalletsDeep);
        }

        // ---- Legacy -----------------------------------------------------------------------------------------

        [Fact]
        public void ALegacyDesign_WithNoI41Field_ResolvesEveryCellToTheStructuralDepth()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 5,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(5, system.DefaultPalletsDeepAt(0));
            Assert.All(Enumerable.Range(0, 3), level => Assert.Equal(5, system.EffectivePalletsDeepAt(0, level)));
            Assert.Equal(5, system.Structure.Fronts[0].PalletsDeep);
        }

        [Fact]
        public void ALegacyJson_WithoutTheI41Fields_LoadsAndDrawsExactlyAsBefore()
        {
            // Un documento como los que escribian las versiones anteriores: entrada por frente CON peraltes y SIN
            // ninguno de los tres campos de I-41.
            const string legacy = @"{
                ""SchemaVersion"": ""1.0"",
                ""Structure"": {
                    ""PalletFront"": 42.0, ""PalletDepth"": 48.0, ""PalletHeight"": 60.0, ""PalletWeight"": 1000.0,
                    ""PalletWeightUnit"": ""kg"", ""PalletsDeep"": 4, ""LoadLevels"": 2,
                    ""FirstLevelHeight"": 6.0, ""BeamDepth"": 4.0
                },
                ""Fronts"": [ { ""HighEndBeamPeraltes"": [ 3.5, 3.5 ] } ]
            }";

            var design = JsonSerializer.Deserialize<PushBackDesignDocument>(legacy).ToDomain();
            var system = new PushBackResolver(Catalog).Resolve(design);
            var front = system.Structure.Fronts[0];

            Assert.Equal(4, system.DefaultPalletsDeepAt(0));
            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 0));
            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 1));
            // Los dos largueros posteriores siguen en la MISMA X, como antes de I-41.
            var rear = PushBackLoadBeamGeometry.HighBeams(system, Catalog, 0, front);
            Assert.Single(rear.Select(beam => Math.Round(beam.Insertion.X, 6)).Distinct());
            // Y ninguna celda dibuja tarima.
            Assert.False(system.DrawPalletAt(0, 0));
            Assert.False(system.DrawPalletAt(0, 1));
        }

        // ---- I-40 se preserva -------------------------------------------------------------------------------

        [Fact]
        public void ChangingAnInnerDepthWithoutMovingTheEnvelope_PreservesModulesAndLineOverrides()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 6);
            var before = Resolve(state, inputs, assembler);
            assembler.AcceptComputation(state, assembler.Build(state, inputs));
            var moduleIds = before.Structure.Modules.Select(module => module.ModuleId).ToList();

            // Un fondo INTERNO baja; el mas profundo (y con el la envolvente) no se mueve.
            Select(state, 0, 0);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);
            var after = Resolve(state, inputs, assembler);

            Assert.Equal(6, after.Structure.Fronts[0].PalletsDeep);   // envolvente intacta
            Assert.Equal(moduleIds, after.Structure.Modules.Select(module => module.ModuleId).ToList());
            Assert.Equal(2, after.EffectivePalletsDeepAt(0, 0));
        }

        [Fact]
        public void ChangingTheEnvelope_ReconcilesWithoutLeavingOrphanLineOverrides()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            assembler.AcceptComputation(state, assembler.Build(state, inputs));

            Select(state, 0, 1);
            state.ApplyPalletsDeep(8, DynamicRackCellScope.Cell);   // la envolvente SI crece
            var after = Resolve(state, inputs, assembler);

            Assert.Equal(8, after.Structure.Fronts[0].PalletsDeep);
            // Toda configuracion por linea que sobreviva apunta a un modulo de cabecera que existe.
            var headerIds = new HashSet<string>(
                after.Structure.Modules.Where(module => module.IsHeader).Select(module => module.ModuleId),
                StringComparer.Ordinal);
            Assert.All(after.Structure.HeaderLineOverrides, line => Assert.Contains(line.ModuleId, headerIds));
        }

        // ---- Frente en blanco -------------------------------------------------------------------------------

        [Fact]
        public void ABlankFront_KeepsItsCellDepthsDormantAndRestoresThemWhenReactivated()
        {
            var (state, inputs, assembler) = Editor(fronts: 2, levels: 2, palletsDeep: 4);
            Select(state, 1, 0);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);

            Assert.True(state.SetActive(1, false));
            var blank = Resolve(state, inputs, assembler);
            Assert.Empty(blank.HighEndBeams[1].PalletsDeep);          // en blanco: sin nivel efectivo
            Assert.Equal(6, state.Cell(1, 0).PalletsDeepOverride);    // pero la intencion sigue dormida

            Assert.True(state.SetActive(1, true));
            Assert.Equal(6, Resolve(state, inputs, assembler).EffectivePalletsDeepAt(1, 0));
        }
    }
}
