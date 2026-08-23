using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-41 · PB-016 — TARIMA CONFIGURABLE POR CELDA Push Back.
    ///
    /// <c>DrawPallet</c> es autoridad POR CELDA, su default legacy es FALSE (por eso un rack anterior a I-41 dibuja
    /// exactamente lo que dibujaba: nada), la tarima se coloca segun frente, nivel, fondo efectivo y ALTURA de la
    /// celda, y NUNCA entra al BOM. La altura sigue viviendo en la autoridad por celda que ya existia
    /// (<c>DynamicEditorCell.PalletHeight</c>); I-41 no crea una segunda.
    ///
    /// Todas estas pruebas FALLAN sin la implementacion de I-41.
    /// </summary>
    public class PushBackCellPalletTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

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
                state.AdjustLevels(front, 0);
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

        private static List<HeaderBlockInstance> Pallets(HeaderRunPlan plan)
            => plan.Flatten().Instances.Where(i => i.Role == HeaderBlockRole.Pallet).ToList();

        private static List<HeaderBlockInstance> LateralPallets(PushBackSystem system)
            => Pallets(new PushBackSystemLateralBuilder().Build(system, Catalog, postIndex: 0));

        // ---- Default legacy: FALSE --------------------------------------------------------------------------

        [Fact]
        public void ANewCell_DoesNotDrawItsPallet()
        {
            var (state, inputs, assembler) = Editor();

            Assert.False(state.Cell(0, 0).DrawPallet);
            var system = Resolve(state, inputs, assembler);
            Assert.False(system.DrawPalletAt(0, 0));
            Assert.Empty(LateralPallets(system));
        }

        [Fact]
        public void ALegacyDesign_WithNoDrawPalletField_EqualsFalseEverywhere()
        {
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
            design.Fronts.Add(new PushBackFrontConfig());   // entrada por frente SIN DrawPallets

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.False(system.DrawPalletAt(0, 0));
            Assert.False(system.DrawPalletAt(0, 1));
            Assert.Empty(LateralPallets(system));
            Assert.Empty(Pallets(new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida)));
            Assert.Empty(Pallets(new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.Posterior)));
        }

        // ---- Autoridad POR CELDA ----------------------------------------------------------------------------

        [Fact]
        public void DrawPallet_IsPerCell_OneLevelOnAndItsNeighboursOff()
        {
            var (state, inputs, assembler) = Editor(levels: 3);
            state.ToggleCell(0, 1, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);

            Assert.False(system.DrawPalletAt(0, 0));
            Assert.True(system.DrawPalletAt(0, 1));
            Assert.False(system.DrawPalletAt(0, 2));
        }

        [Fact]
        public void OnlyTheMarkedCell_EmitsPalletsInTheLateral()
        {
            var (state, inputs, assembler) = Editor(levels: 3, palletsDeep: 4);
            state.ToggleCell(0, 1, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);
            var pallets = LateralPallets(system);

            // Una tarima por POSICION de fondo de esa celda, y ninguna de los otros dos niveles.
            Assert.Equal(4, pallets.Count);
            Assert.All(pallets, pallet => Assert.Equal(SelectiveRackDefaults.PalletPieceId, pallet.PieceId));
            // Todas pertenecen al nivel 2: cada una se apoya en la linea de origen de ESA cama y de ninguna otra.
            var axis = PushBackFlowBedGeometry.Resolve(system, Catalog, system.Structure.Fronts[0])
                .Single(candidate => candidate.LevelNumber == 2);
            Assert.All(pallets, pallet =>
                Assert.Equal(axis.RailOriginYAt(pallet.Insertion.X), pallet.Insertion.Y, 6));
        }

        // ---- La tarima se coloca segun frente, nivel, FONDO EFECTIVO y ALTURA -------------------------------

        [Fact]
        public void TheLateralPalletCount_FollowsTheCellsEffectiveDepth()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.All);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);   // solo el nivel 1 se acorta

            var system = Resolve(state, inputs, assembler);
            var front = system.Structure.Fronts[0];

            // Cada tarima cabalga la pendiente de SU cama, asi que la Y no es constante dentro de un nivel: se
            // asigna al eje cuya linea de origen pasa por ella.
            var axes = PushBackFlowBedGeometry.Resolve(system, Catalog, front).ToList();
            var byLevel = LateralPallets(system)
                .GroupBy(pallet => axes
                    .OrderBy(axis => Math.Abs(axis.RailOriginYAt(pallet.Insertion.X) - pallet.Insertion.Y))
                    .First().LevelNumber)
                .OrderBy(group => group.Key)
                .ToList();

            Assert.Equal(2, byLevel.Count);
            Assert.Equal(2, byLevel[0].Count());   // fondo efectivo 2
            Assert.Equal(4, byLevel[1].Count());   // fondo efectivo 4
            // Y ninguna rebasa el larguero posterior de su propia celda.
            var shallowRearX = PushBackCellDepth.RearX(system, front, 1);
            Assert.All(byLevel[0], pallet => Assert.True(pallet.Insertion.X <= shallowRearX + 1e-6));
        }

        [Fact]
        public void TheLateralPallet_CarriesTheCellsOwnHeight_AndTheFlowDepthAsItsLength()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 3);
            // Alturas DISTINTAS entre niveles, por celda (la autoridad ya existente).
            state.Structure.Fronts[0].Cells[0].PalletHeight = 40.0;
            state.Structure.Fronts[0].Cells[1].PalletHeight = 72.0;
            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.All);

            var system = Resolve(state, inputs, assembler);
            var alturas = LateralPallets(system)
                .Select(pallet => pallet.DynamicParameters[SelectiveRackDefaults.PalletAltoParam])
                .Distinct()
                .OrderBy(value => value)
                .ToList();

            Assert.Equal(new[] { 40.0, 72.0 }, alturas);
            // LONGITUD en el lateral = el FONDO de la tarima (va de canto, a lo largo de la calle).
            Assert.All(LateralPallets(system), pallet =>
                Assert.Equal(system.Structure.Pallet.Depth,
                    pallet.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam], 6));
        }

        [Fact]
        public void TheLateralPallets_RideTheSlopedBedLine_NotAFlatY()
        {
            var (state, inputs, assembler) = Editor(levels: 1, palletsDeep: 4);
            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);
            var pallets = LateralPallets(system).OrderBy(pallet => pallet.Insertion.X).ToList();

            Assert.Equal(4, pallets.Count);
            // La cama sube hacia +X, asi que cada tarima se apoya mas alto que la anterior.
            for (var index = 1; index < pallets.Count; index++)
            {
                Assert.True(pallets[index].Insertion.Y > pallets[index - 1].Insertion.Y,
                    $"la tarima {index} deberia apoyarse mas alto que la {index - 1}");
            }
        }

        [Fact]
        public void TheFrontalCuts_DrawOneRowPerLane_ForTheMarkedCellOnly()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            state.Structure.Fronts[0].PalletCount = 3;   // tres calles lado a lado
            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Cell);

            var system = Resolve(state, inputs, assembler);
            var builder = new PushBackSystemFrontalBuilder();

            var baja = Pallets(builder.BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida));
            var posterior = Pallets(builder.BuildPlan(system, Catalog, PushBackFrontalEnd.Posterior));

            Assert.Equal(3, baja.Count);
            Assert.Equal(3, posterior.Count);
            // En el frontal la LONGITUD es el FRENTE de la tarima.
            Assert.All(baja, pallet => Assert.Equal(
                system.Structure.Pallet.Front,
                pallet.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam], 6));
            // Los dos cortes muestran la misma calle desde extremos distintos: el posterior se apoya mas alto.
            Assert.True(posterior[0].Insertion.Y > baja[0].Insertion.Y);
        }

        [Fact]
        public void ThePlanta_DrawsNoPallet_BecauseTheBlockHasNoPlantaView()
        {
            var (state, inputs, assembler) = Editor(levels: 2);
            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.All);

            var system = Resolve(state, inputs, assembler);

            Assert.Empty(Pallets(new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog)));
        }

        // ---- Nunca en el BOM --------------------------------------------------------------------------------

        [Fact]
        public void PalletsNeverReachTheBom_NorChangeAnyQuantity()
        {
            var (state, inputs, assembler) = Editor(fronts: 2, levels: 2);
            var without = PushBackBomBuilder.Build(Resolve(state, inputs, assembler), Catalog);

            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.All);
            var with = PushBackBomBuilder.Build(Resolve(state, inputs, assembler), Catalog);

            Assert.DoesNotContain(with.Components,
                component => string.Equals(component.ProfileId, SelectiveRackDefaults.PalletPieceId,
                    StringComparison.OrdinalIgnoreCase));
            // El BOM es EXACTAMENTE el mismo: las tarimas son referencia visual, no producto.
            Assert.Equal(
                without.Components.Select(c => (c.Category, c.ProfileId, c.Length, c.Quantity)).OrderBy(t => t.ToString()).ToList(),
                with.Components.Select(c => (c.Category, c.ProfileId, c.Length, c.Quantity)).OrderBy(t => t.ToString()).ToList());
        }

        // ---- Alcances: seleccion multiple y una sola propiedad ----------------------------------------------

        [Fact]
        public void ApplyDrawPallet_ToTheSelection_MarksTheCtrlClickedCellsOnly()
        {
            var (state, _, _) = Editor(fronts: 2, levels: 3);
            state.ToggleCell(0, 0, false);
            state.ToggleCell(1, 2, true);
            state.ToggleCell(0, 1, true);

            Assert.Equal(3, state.ApplyDrawPallet(true, DynamicRackCellScope.Selected));

            Assert.True(state.Cell(0, 0).DrawPallet);
            Assert.True(state.Cell(0, 1).DrawPallet);
            Assert.True(state.Cell(1, 2).DrawPallet);
            Assert.False(state.Cell(0, 2).DrawPallet);
            Assert.False(state.Cell(1, 0).DrawPallet);
        }

        [Theory]
        [InlineData(DynamicRackCellScope.Cell, 1)]
        [InlineData(DynamicRackCellScope.Level, 2)]
        [InlineData(DynamicRackCellScope.Front, 3)]
        [InlineData(DynamicRackCellScope.All, 6)]
        public void ApplyDrawPallet_HonoursEveryExistingScope(DynamicRackCellScope scope, int expected)
        {
            var (state, _, _) = Editor(fronts: 2, levels: 3);
            state.ToggleCell(0, 1, false);

            Assert.Equal(expected, state.ApplyDrawPallet(true, scope));
        }

        [Fact]
        public void ApplyDrawPallet_ChangesOnlyThePalletFlag_NeverAnotherFieldOfTheSourceCell()
        {
            var (state, _, _) = Editor(levels: 2);
            state.Structure.Fronts[0].Cells[0].ClearHeight = 8.0;
            state.Structure.Fronts[0].Cells[1].ClearHeight = 14.0;
            state.Cell(0, 1).PalletsDeepOverride = 7;
            state.Cell(0, 1).HighEndBeamPeralte = 5.0;

            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Front);

            Assert.True(state.Cell(0, 1).DrawPallet);
            Assert.Equal(14.0, state.Structure.Fronts[0].Cells[1].ClearHeight);
            Assert.Equal(7, state.Cell(0, 1).PalletsDeepOverride);
            Assert.Equal(5.0, state.Cell(0, 1).HighEndBeamPeralte);
        }

        [Fact]
        public void RestoringDrawPallet_ReturnsToTheLegacyFalse()
        {
            var (state, inputs, assembler) = Editor(levels: 2);
            state.ToggleCell(0, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.All);
            Assert.NotEmpty(LateralPallets(Resolve(state, inputs, assembler)));

            state.ApplyDrawPallet(false, DynamicRackCellScope.All);

            var system = Resolve(state, inputs, assembler);
            Assert.False(system.DrawPalletAt(0, 0));
            Assert.False(system.DrawPalletAt(0, 1));
            Assert.Empty(LateralPallets(system));
        }

        // ---- Persistencia y reapertura ----------------------------------------------------------------------

        [Fact]
        public void SaveLoad_PreservesTheFlagPerCell()
        {
            var (state, inputs, assembler) = Editor(fronts: 2, levels: 3);
            state.ToggleCell(0, 2, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Cell);
            state.ToggleCell(1, 0, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Cell);

            var design = assembler.BuildDesign(state, inputs);
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var reloaded = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
            var system = new PushBackResolver(Catalog).Resolve(reloaded);

            Assert.True(system.DrawPalletAt(0, 2));
            Assert.True(system.DrawPalletAt(1, 0));
            Assert.False(system.DrawPalletAt(0, 0));
            Assert.False(system.DrawPalletAt(1, 1));
        }

        [Fact]
        public void ReopeningTheEditor_RestoresTheFlagPerCell()
        {
            var (state, inputs, assembler) = Editor(levels: 3);
            state.ToggleCell(0, 1, false);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Cell);

            var reopened = new PushBackEditorState();
            reopened.LoadFromDesign(assembler.BuildDesign(state, inputs), assembler.Resolver);

            Assert.False(reopened.Cell(0, 0).DrawPallet);
            Assert.True(reopened.Cell(0, 1).DrawPallet);
            Assert.False(reopened.Cell(0, 2).DrawPallet);
        }

        [Fact]
        public void ADesignWithNoPallet_WritesNoDrawPalletFieldAtAll()
        {
            var (state, inputs, assembler) = Editor(levels: 2);

            var json = JsonSerializer.Serialize(
                PushBackDesignDocument.FromDomain(assembler.BuildDesign(state, inputs)));

            Assert.DoesNotContain("\"DrawPallets\":[", json.Replace(" ", string.Empty));
        }

        // ---- Convive con el fondo por celda -----------------------------------------------------------------

        [Fact]
        public void DepthAndPallet_AreIndependentAuthorities()
        {
            var (state, inputs, assembler) = Editor(levels: 2, palletsDeep: 4);
            state.ToggleCell(0, 0, false);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);   // fondo si, tarima no

            var system = Resolve(state, inputs, assembler);

            Assert.Equal(6, system.EffectivePalletsDeepAt(0, 0));
            Assert.False(system.DrawPalletAt(0, 0));
            Assert.Empty(LateralPallets(system));
        }
    }
}
