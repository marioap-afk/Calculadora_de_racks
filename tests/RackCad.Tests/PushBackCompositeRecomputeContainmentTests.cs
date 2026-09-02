using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1C/H12, contrato del dueño) — UN ESTADO DE EDITOR INVALIDO SE DECLARA, NO TUMBA LA VENTANA.
    ///
    /// <para>
    /// El armado del diseño compuesto se evaluaba como argumento, fuera de cualquier try: una validacion que
    /// lanzara —el refuerzo del poste derivado medido contra el propio poste— escapaba como excepcion no capturada
    /// dentro de AutoCAD. La contencion ya estaba implementada; lo que faltaba era esta prueba, que alcanza la
    /// excepcion REAL y no una simulada.
    /// </para>
    /// </summary>
    public class PushBackCompositeRecomputeContainmentTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>El estado compuesto minimo del editor, con los dos lados poblados.</summary>
        private static PushBackCompositeEditorState CompositeState()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(2);
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(2);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            return state;
        }

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        [Fact]
        public void CompositeRecompute_ReinforcementValidationExceptionIsContained()
        {
            // El refuerzo del poste derivado mas alto que el propio poste: la validacion REAL lanza. El armado del
            // diseño compuesto esta dentro de la ruta protegida, asi que el editor recibe una computacion invalida
            // con el motivo y no una excepcion que tumbe la ventana.
            var state = CompositeState();
            var inputs = Inputs();
            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = 100000.0;

            // La validacion lanza de verdad: si dejara de hacerlo, esta prueba estaria midiendo aire.
            var thrown = Record.Exception(
                () => PushBackAdvancedRackParameters.ValidateReinforcementAgainstPost(inputs, 120.0));
            Assert.IsType<InvalidOperationException>(thrown);

            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog);

            Assert.False(computation.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(computation.Error));
            Assert.Contains("refuerzo", computation.Error, StringComparison.OrdinalIgnoreCase);
            Assert.Null(computation.System);
        }

        [Fact]
        public void CompositeRecompute_ExceptionDoesNotPartiallyCommitState()
        {
            var state = CompositeState();
            var inputs = Inputs();
            var before = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog);

            Assert.True(before.IsValid);
            var slotsBefore = state.SlotCount;
            var sideBBefore = state.SideBPresent;
            var baselineBefore = state.SideA.WorkingBaseline;

            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = 100000.0;
            var failed = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog);

            Assert.False(failed.IsValid);
            Assert.Equal(slotsBefore, state.SlotCount);
            Assert.Equal(sideBBefore, state.SideBPresent);
            Assert.Same(baselineBefore, state.SideA.WorkingBaseline);   // la baseline no avanza con un fallo
        }
    }
}
