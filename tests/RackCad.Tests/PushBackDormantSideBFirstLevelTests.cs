using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (NP-1, contrato del dueño) — DORMIR UN LADO NO ES BORRARLO.
    ///
    /// <para>
    /// El lado B puede retirarse: el rack vuelve a ser de un solo sentido y su configuracion queda DORMANTE, lista
    /// para reaparecer tal cual. La igualacion del «Alto 1er nivel» —los dos lados arrancan en el mismo troquel—
    /// es una regla de NACIMIENTO: existe para que un lado B recien creado a partir de A no nazca con medio paso de
    /// diferencia. No es una sincronizacion permanente.
    /// </para>
    ///
    /// <para>
    /// <b>Medido antes del cambio.</b> Con A = 6" y B = 14" declarados por el usuario, apagar el lado B conservaba
    /// sus 14" —tambien a traves de dos recalculos—, pero volver a declararlo lo dejaba en 6": la igualacion corria
    /// ANTES de la guarda que distingue un lado dormante de uno nuevo, asi que reactivar escribia el valor de A
    /// encima del authored de B. La perdida era la misma con el lado A activo y con el B activo.
    /// </para>
    /// </summary>
    public class PushBackDormantSideBFirstLevelTests
    {
        private const double SideAFirstLevel = 6.0;
        private const double SideBFirstLevel = 14.0;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        /// <summary>Un compuesto con los dos lados declarados y presentes en todas sus ranuras.</summary>
        private static PushBackCompositeEditorState State(int slots = 2)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            return state;
        }

        /// <summary>El caso del hallazgo: cada lado con su propio «Alto 1er nivel» declarado.</summary>
        private static PushBackCompositeEditorState Configured(PushBackSide activeWhenSleeping = PushBackSide.A)
        {
            var state = State();
            SetFirstLevel(state.SideA, SideAFirstLevel);
            SetFirstLevel(state.SideB, SideBFirstLevel);
            state.SetActiveSide(activeWhenSleeping);
            return state;
        }

        private static void SetFirstLevel(PushBackEditorState side, double value)
        {
            for (var slot = 0; slot < side.Structure.Count; slot++)
            {
                side.Structure.Fronts[slot].FirstLevelHeight = value;
            }
        }

        private static void AssertFirstLevel(PushBackEditorState side, double expected)
            => Assert.All(
                side.Structure.Fronts,
                front => Assert.Equal(expected, front.FirstLevelHeight, 6));

        /// <summary>Un recalculo real del editor compuesto.</summary>
        private static void Recompute(PushBackCompositeEditorState state)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());
            var computation = assembler.BuildFrom(design, PushBackSide.A);
            Assert.True(computation.IsValid, computation.Error);
            assembler.AcceptComputation(state.SideA, computation);
        }

        // ---------------------------------------------------------------- el hallazgo

        [Fact]
        public void DormantSideB_PreservesItsAuthoredFirstLevelWhenReactivated()
        {
            var state = Configured();

            state.SetSideBPresent(false);
            state.SetSideBPresent(true);

            AssertFirstLevel(state.SideB, SideBFirstLevel);
            AssertFirstLevel(state.SideA, SideAFirstLevel);
        }

        [Fact]
        public void DormantSideB_DoesNotReceiveSideAFirstLevelDuringRecompute()
        {
            var state = Configured();
            state.SetSideBPresent(false);

            // Se mira el lado DORMANTE, sin esperar a reactivarlo: tampoco ahi puede recibir el valor de A.
            Recompute(state);
            AssertFirstLevel(state.SideB, SideBFirstLevel);

            Recompute(state);
            AssertFirstLevel(state.SideB, SideBFirstLevel);

            // Y sigue estando cuando el lado vuelve: un arreglo que solo sobreviviera una transicion no valdria.
            state.SetSideBPresent(true);
            AssertFirstLevel(state.SideB, SideBFirstLevel);
        }

        [Fact]
        public void DormantSideB_FirstLevelPreservationDoesNotDependOnActiveSide()
        {
            foreach (var active in new[] { PushBackSide.A, PushBackSide.B })
            {
                var state = Configured(active);

                state.SetSideBPresent(false);
                Recompute(state);
                state.SetSideBPresent(true);

                AssertFirstLevel(state.SideB, SideBFirstLevel);
            }
        }

        [Fact]
        public void DormantSideB_SurvivesSeveralSleepAndWakeCycles()
        {
            var state = Configured();

            for (var cycle = 0; cycle < 3; cycle++)
            {
                state.SetSideBPresent(false);
                Recompute(state);
                state.SetSideBPresent(true);
                Recompute(state);
                AssertFirstLevel(state.SideB, SideBFirstLevel);
            }
        }

        // ---------------------------------------------------------------- lo que NO se rompe

        [Fact]
        public void NewSideB_StillUsesExistingInitializationRule()
        {
            // La regla de NACIMIENTO sigue: un lado B que nunca existio arranca en el troquel del lado A.
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(2);
            SetFirstLevel(state.SideA, SideAFirstLevel);

            state.SetSideBPresent(true);

            AssertFirstLevel(state.SideB, SideAFirstLevel);
        }

        [Fact]
        public void DormantSideB_NewSlotsInheritItsOwnFirstLevel()
        {
            // Si el rack crece mientras B duerme, las ranuras nuevas de B son suyas: heredan SU intencion, no la de A.
            var state = Configured();
            state.SetSideBPresent(false);
            state.SetSlotCount(3);

            state.SetSideBPresent(true);

            Assert.Equal(3, state.SideB.Structure.Count);
            AssertFirstLevel(state.SideB, SideBFirstLevel);
            AssertFirstLevel(state.SideA, SideAFirstLevel);
        }

        // ---------------------------------------------------------------- la frontera documental, medida

        [Fact]
        public void DormantSideB_IsNotPersistedToday_SoThePreservationIsSessionScoped()
        {
            // El diseño de un rack con el lado B retirado NO lleva lado B: guardarlo y reabrirlo no puede devolver
            // una configuracion dormante que el documento no contiene. Es la deuda H14, que esta ronda NO abre; se
            // fija aqui como HECHO medido para que el alcance de NP-1 —la sesion viva— quede dicho y no supuesto.
            var state = Configured();
            state.SetSideBPresent(false);

            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());

            Assert.Null(design.SideB);
            AssertFirstLevel(state.SideB, SideBFirstLevel);   // en la sesion viva si se conserva
        }
    }
}
