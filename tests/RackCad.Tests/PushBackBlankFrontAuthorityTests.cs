using System;
using System.Linq;
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
    /// I-42 (ronda post-82e918b) — «EN BLANCO» ES LA UNICA AUTORIDAD del caso.
    ///
    /// <para>
    /// Convivian dos controles para la misma decision: «En blanco» (I-33) y «Frente presente en este lado». Uno
    /// quitaba cabeceras donde no debia y el otro no, y el segundo solo funcionaba bien con el lado activo en
    /// «Ambos». Ahora la intencion es una: un frente EN BLANCO conserva su claro y su estructura y no lleva ninguna
    /// carga en ese lado, que es exactamente lo que significa «este frente no tiene lado B». La presencia por lado
    /// se DERIVA de ella.
    /// </para>
    /// </summary>
    public class PushBackBlankFrontAuthorityTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState Composite(int slots = 3)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            return state;
        }

        private static PushBackSystem Build(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        /// <summary>La presencia por lado se LEE de «En blanco»: no hay dos almacenes que puedan discrepar.</summary>
        [Theory]
        [InlineData(PushBackSide.A)]
        [InlineData(PushBackSide.B)]
        public void Presence_IsDerivedFromTheBlankFlag(PushBackSide side)
        {
            var state = Composite();
            var matrix = state.Of(side).Structure;

            Assert.True(state.IsSlotPresent(side, 1));
            matrix.Fronts[1].IsActive = false;
            Assert.False(state.IsSlotPresent(side, 1));

            matrix.Fronts[1].IsActive = true;
            Assert.True(state.IsSlotPresent(side, 1));
        }

        /// <summary>Y funciona con CUALQUIER lado activo, no solo con «Ambos».</summary>
        [Theory]
        [InlineData(PushBackSideSelection.A)]
        [InlineData(PushBackSideSelection.B)]
        [InlineData(PushBackSideSelection.Both)]
        public void BlankFront_WorksWhateverTheActiveSide(PushBackSideSelection selection)
        {
            var state = Composite();
            state.SetActiveSelection(selection);
            var side = selection == PushBackSideSelection.B ? PushBackSide.B : PushBackSide.A;

            Assert.True(state.SetSlotPresent(side, 2, false));
            Assert.False(state.IsSlotPresent(side, 2));
            Assert.Equal(
                side == PushBackSide.A ? PushBackCellTopology.SoloB : PushBackCellTopology.SoloA,
                state.TopologyAt(2, 0));
        }

        /// <summary>Un lado puede quedarse ENTERO en blanco mientras el otro sostenga el rack.</summary>
        [Fact]
        public void OneSide_CanBeFullyBlank_WhileTheOtherHoldsTheRack()
        {
            var state = Composite(2);
            for (var slot = 0; slot < 2; slot++)
            {
                Assert.True(state.SetSlotPresent(PushBackSide.B, slot, false));
            }

            Assert.False(state.IsSlotPresent(PushBackSide.B, 0));
            Assert.False(state.IsSlotPresent(PushBackSide.B, 1));

            // Y el rack vuelve a ser fisicamente el de un solo sentido.
            var system = Build(state);
            Assert.False(system.IsComposite);
        }

        /// <summary>Pero dejar el RACK entero en blanco se rehusa: la guarda es del rack, no de cada lado.</summary>
        [Fact]
        public void TheWholeRack_CannotBeBlank()
        {
            var state = Composite(1);
            Assert.True(state.SetSlotPresent(PushBackSide.B, 0, false));
            Assert.False(state.SetSlotPresent(PushBackSide.A, 0, false));
            Assert.True(state.IsSlotPresent(PushBackSide.A, 0));
        }

        /// <summary>
        /// Un frente en blanco QUITA su carga y conserva su estructura: es el contrato de I-33 y sigue valiendo en
        /// compuesto. Se mide sobre las camas, no sobre un conteo global.
        /// </summary>
        [Fact]
        public void BlankFront_RemovesItsBeds_AndKeepsTheGrid()
        {
            var state = Composite();
            var before = Build(state);
            var beforeRuns = PushBackRuns.Resolve(before).Runs.Count(run => run.Slot == 1);
            var beforeFronts = before.Structure.Fronts.Count;
            Assert.True(beforeRuns > 0);

            state.SetSlotPresent(PushBackSide.A, 1, false);
            state.SetSlotPresent(PushBackSide.B, 1, false);
            var after = Build(state);

            // La ranura ya no tiene ninguna cama...
            Assert.Empty(PushBackRuns.Resolve(after).Runs.Where(run => run.Slot == 1));
            // ...y la retícula transversal NO se rompe: el frente sigue ahi, con su claro.
            Assert.Equal(beforeFronts, after.Structure.Fronts.Count);
        }

        /// <summary>Y sobrevive al round trip: la intencion viaja en el diseño, no en un almacen paralelo.</summary>
        [Fact]
        public void BlankFront_RoundTrips()
        {
            var state = Composite();
            state.SetSlotPresent(PushBackSide.A, 2, false);
            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());

            var json = System.Text.Json.JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var restored = System.Text.Json.JsonSerializer
                .Deserialize<PushBackDesignDocument>(json).ToDomain();

            var before = new PushBackResolver(Catalog).Resolve(design);
            var after = new PushBackResolver(Catalog).Resolve(restored);

            Assert.Equal(before.Composite.Cell(2, 1).Topology, after.Composite.Cell(2, 1).Topology);
            Assert.Equal(PushBackCellTopology.SoloB, after.Composite.Cell(2, 1).Topology);
            Assert.Equal(before.Structure.Fronts.Count, after.Structure.Fronts.Count);
        }
    }
}
