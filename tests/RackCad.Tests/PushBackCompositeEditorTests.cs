using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (G6) — el ESTADO DEL EDITOR compuesto: selector de lado, matriz Frente x Nivel del lado activo, los cinco
    /// alcances dentro de ese lado, topologia por celda, gap, separador, estructura efectiva con restauracion,
    /// diagnosticos y rollback transaccional. Sin WPF: lo que se fija aqui es el modelo que la ventana conduce.
    /// </summary>
    public class PushBackCompositeEditorTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(int frontsA = 2, int frontsB = 2, bool sideB = true)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.SetFrontCount(frontsA);
            state.SetSideBPresent(sideB);
            if (sideB)
            {
                state.SideB.SetFrontCount(frontsB);
            }

            return state;
        }

        // ---- Selector de lado ---------------------------------------------------------------------------------

        [Fact]
        public void TheActiveSide_DrivesTheMatrix_AndSwitchingKeepsBothConfigurations()
        {
            var state = State();
            Assert.Equal(PushBackSide.A, state.ActiveSide);
            Assert.Same(state.SideA, state.Active);

            state.Active.ToggleCell(1, 0, extendSelection: false);
            state.SetActiveSide(PushBackSide.B);

            Assert.Same(state.SideB, state.Active);
            // La seleccion del lado A no se perdio al cambiar de lado.
            Assert.Equal(1, state.SideA.Structure.SelectedFrontIndex);
        }

        [Fact]
        public void SideB_CannotBeActivated_WhenTheRackHasOnlyOneSide()
        {
            var state = State(sideB: false);
            state.SetActiveSide(PushBackSide.B);

            Assert.Equal(PushBackSide.A, state.ActiveSide);
            Assert.False(state.SideBPresent);
        }

        [Fact]
        public void RetiringSideB_DoesNotDestroyItsConfiguration()
        {
            var state = State(frontsB: 3);
            state.SetActiveSide(PushBackSide.B);
            state.SideB.AdjustLevels(0, 2);
            var levels = state.SideB.Structure.Fronts[0].LoadLevels;

            state.SetSideBPresent(false);
            Assert.Equal(PushBackSide.A, state.ActiveSide);
            state.SetSideBPresent(true);

            Assert.Equal(3, state.SideB.Structure.Count);
            Assert.Equal(levels, state.SideB.Structure.Fronts[0].LoadLevels);
        }

        // ---- Ranuras presentes solo en un lado -----------------------------------------------------------------

        [Fact]
        public void ASlotCanBeRetiredFromOneSide_AndItsConfigurationStaysDormant()
        {
            var state = State(frontsA: 3, frontsB: 3);
            state.SetActiveSide(PushBackSide.B);
            state.SideB.AdjustLevels(2, 1);
            var dormant = state.SideB.Structure.Fronts[2].LoadLevels;

            state.SetSlotPresent(PushBackSide.B, 2, false);
            Assert.False(state.IsSlotPresent(PushBackSide.B, 2));
            Assert.Null(state.BuildSideB().Fronts[2]);

            state.SetSlotPresent(PushBackSide.B, 2, true);
            Assert.Equal(dormant, state.SideB.Structure.Fronts[2].LoadLevels);
        }

        [Fact]
        public void AThreeByFourRack_ResolvesFourPhysicalSlots_WithTheFourthOnlyInB()
        {
            var state = State(frontsA: 3, frontsB: 4);
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);

            Assert.True(computation.IsValid);
            Assert.Equal(4, computation.System.Structure.Fronts.Count);
            Assert.Null(computation.System.Composite.SideA.Front(3));
            Assert.NotNull(computation.System.Composite.SideB.Front(3));
        }

        // ---- Topologia por celda con los cinco alcances --------------------------------------------------------

        [Fact]
        public void TopologyUsesTheSameFiveScopes_InsideTheActiveSide()
        {
            var state = State(frontsA: 2, frontsB: 2);
            state.SideA.AdjustLevels(0, 2);   // 3 niveles en el frente 0
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);

            state.Active.ToggleCell(0, 1, extendSelection: false);
            Assert.Equal(1, state.ApplyTopology(
                PushBackCellTopology.Corrida, PushBackRunDirection.AToB, DynamicRackCellScope.Cell));
            Assert.Equal(PushBackCellTopology.Corrida, state.TopologyAt(0, 1));
            Assert.Equal(PushBackCellTopology.Encontradas, state.TopologyAt(0, 0));

            var written = state.ApplyTopology(
                PushBackCellTopology.SoloA, PushBackRunDirection.AToB, DynamicRackCellScope.Front);
            Assert.True(written >= 3);
            Assert.Equal(PushBackCellTopology.SoloA, state.TopologyAt(0, 0));
            Assert.Equal(PushBackCellTopology.SoloA, state.TopologyAt(0, 1));
        }

        [Fact]
        public void WritingTheDefaultTopology_ClearsTheStoredCell()
        {
            var state = State();
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            state.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            Assert.Single(state.BuildComposite().Topologies);

            state.SetCell(0, 0, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            Assert.Empty(state.BuildComposite().Topologies);
        }

        // ---- Interfaz central ----------------------------------------------------------------------------------

        [Fact]
        public void TheGapAndTheCentralSeparator_TravelToTheDesign()
        {
            var state = State();
            state.SetGap(-5.0);
            Assert.Equal(0.0, state.Gap, 6);

            state.SetGap(12.0);
            state.SetCentralSeparator(true);
            Assert.False(state.CentralSeparatorWithoutGap);

            var composite = state.BuildComposite();
            Assert.Equal(12.0, composite.Gap, 6);
            Assert.True(composite.CentralSeparator);
        }

        [Fact]
        public void ACentralSeparatorWithoutGap_IsFlaggedBeforeResolving()
        {
            var state = State();
            state.SetCentralSeparator(true);
            Assert.True(state.CentralSeparatorWithoutGap);
        }

        // ---- Estructura efectiva y restauracion -----------------------------------------------------------------

        [Fact]
        public void RestoringTheStructure_ClearsTheOverride_AndFollowsTheCurrentProposal()
        {
            var state = State();
            state.SetStructureOverride(PushBackSide.B, 9);
            Assert.Equal(9, state.StructureOverride(PushBackSide.B));

            state.RestoreStructure(PushBackSide.B);
            Assert.Null(state.StructureOverride(PushBackSide.B));
            Assert.Null(state.BuildComposite().StructureOverrideB);
        }

        [Fact]
        public void AnOverrideBelowTheProposal_IsAWarning_NotASilentCorrection()
        {
            var state = State(frontsA: 1, frontsB: 1);
            state.SetStructureOverride(PushBackSide.A, 2);
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);

            Assert.True(computation.IsValid);
            Assert.Contains(computation.Diagnostics, diagnostic =>
                diagnostic.Code == PushBackCompositeCodes.StructureOverrideBelowProposal
                && diagnostic.Severity == PushBackCompositeSeverity.Warning);
            Assert.Equal(2, computation.System.Composite.SideA.EffectiveStructure);
        }

        // ---- Diagnosticos --------------------------------------------------------------------------------------

        [Fact]
        public void ACellThatDoesNotFit_IsReportedAsBlocking_WithItsAddress()
        {
            var state = State(frontsA: 1, frontsB: 1);
            state.SideA.ApplyPalletsDeep(8, DynamicRackCellScope.All);
            state.SetStructureOverride(PushBackSide.A, 3);

            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);

            Assert.True(computation.HasBlocking);
            var blocking = computation.Diagnostics.First(diagnostic => diagnostic.IsBlocking);
            Assert.Equal(PushBackCompositeCodes.CellDoesNotFit, blocking.Code);
            Assert.True(blocking.FrontIndex >= 0);
            Assert.True(blocking.LevelNumber >= 1);
        }

        [Fact]
        public void AValidCompositeRack_HasNoBlockingDiagnostics()
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(State(), Inputs(), Catalog);

            Assert.True(computation.IsValid);
            Assert.False(computation.HasBlocking);
            Assert.True(computation.System.IsComposite);
        }

        // ---- Rollback transaccional -----------------------------------------------------------------------------

        [Fact]
        public void TheSnapshot_RollsBackBothSidesAndTheInterface()
        {
            var state = State(frontsA: 2, frontsB: 2);
            state.SetGap(10.0);
            state.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            var snapshot = state.Snapshot();

            state.SetGap(30.0);
            state.SetCentralSeparator(true);
            state.SetStructureOverride(PushBackSide.A, 12);
            state.SetSideBPresent(false);
            state.SetCell(0, 0, PushBackCellTopology.SoloA, PushBackRunDirection.AToB);
            state.SideA.SetFrontCount(5);

            state.Restore(snapshot);

            Assert.Equal(10.0, state.Gap, 6);
            Assert.False(state.CentralSeparator);
            Assert.Null(state.StructureOverrideA);
            Assert.True(state.SideBPresent);
            Assert.Equal(PushBackCellTopology.Corrida, state.TopologyAt(0, 0));
            Assert.Equal(PushBackRunDirection.BToA, state.DirectionAt(0, 0));
            Assert.Equal(2, state.SideA.Structure.Count);
        }

        // ---- Un rack de un solo sentido produce el diseno de siempre ---------------------------------------------

        [Fact]
        public void WithoutSideB_TheAssembledDesign_IsTheSingleSidedOne()
        {
            var state = State(frontsA: 2, sideB: false);
            var inputs = Inputs();

            var composite = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, inputs);
            var single = new PushBackEditorDesignAssembler(Catalog).BuildDesign(state.SideA, inputs);

            Assert.Null(composite.SideB);
            Assert.False(composite.IsComposite);
            Assert.Equal(single.Structure.Fronts.Count, composite.Structure.Fronts.Count);
            Assert.Equal(single.Fronts.Count, composite.Fronts.Count);
        }
    }
}
