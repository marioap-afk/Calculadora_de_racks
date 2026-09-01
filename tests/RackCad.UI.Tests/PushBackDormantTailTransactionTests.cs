using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (A6-TAIL-TX, contrato del dueño) — UNA COMPUTACION SOLO PUEDE CONSUMIR LA COLA QUE ELLA MISMA LEYO.
    ///
    /// <para>
    /// A5 dejo el consumo atado al ACEPTAR, pero la marca de consumo era global y sobrevivia de un armado al
    /// siguiente. Medido por la ventana real: con la cola aparcada, un despertar que FALLA deja la marca encendida;
    /// el siguiente recalculo VALIDO —uno de un solo sentido, que ni siquiera mira la cola— la confirma al aceptar y
    /// borra la intencion aparcada; al volver a declarar el lado B, <c>B:M2</c> regresa en 48" y el override de
    /// linea (1, B:M6) ha desaparecido.
    /// </para>
    ///
    /// <para>
    /// Ahora cada armado ABRE su propio intento: la candidatura nace apagada y solo se enciende si ESE armado
    /// adjunta la cola. Un aparcado invalida cualquier candidatura anterior, una fuente VACIA no borra lo aparcado
    /// —dormir no es borrar— y el borrado explicito sigue existiendo para la unica accion que lo significa: cargar
    /// otro rack.
    /// </para>
    /// </summary>
    public class PushBackDormantTailTransactionTests
    {
        private static CheckBox Check(RackPushBackSystemWindow window, string name)
            => (CheckBox)window.FindName(name);

        private static NumericField Field(RackPushBackSystemWindow window, string name)
            => (NumericField)window.FindName(name);

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        private static void Recompute(RackPushBackSystemWindow window)
            => LoseFocus(Field(window, "GapBox"));

        private static void ActivateBSlots(RackPushBackSystemWindow window, bool active = true)
        {
            var matrix = window.CompositeState.Of(PushBackSide.B).Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                matrix.Fronts[front].IsActive = active;
            }
        }

        /// <summary>Deja el lado A sin ningun frente activo: el resolver lo rechaza DESPUES del armado.</summary>
        private static void BreakSideA(RackPushBackSystemWindow window, bool broken)
        {
            var matrix = window.CompositeState.SideA.Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                matrix.Fronts[front].IsActive = !broken;
            }
        }

        private static RackPushBackSystemWindow Customized()
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            Check(window, "SideBPresentCheck").IsChecked = true;
            ActivateBSlots(window);
            var gap = Field(window, "GapBox");
            gap.SetNumber(54.0);
            LoseFocus(gap);

            var session = window.CompositeState.SideA.ModuleSession;
            Assert.True(session.SetLength("M2", 30.0));
            Assert.True(session.SetLength("B:M2", 40.0));
            var configuration = session.HeaderConfigurationCopy("B:M6", 1);
            Assert.NotNull(configuration);
            configuration.Height = 137.0;
            Assert.True(session.ApplyHeaderConfigurationToLine(configuration, 1, new[] { "B:M6" }).Applied);
            window.CompositeState.SideA.CommitModuleEdits();
            Recompute(window);
            return window;
        }

        private static double LengthOf(RackPushBackSystemWindow window, string moduleId)
            => window.LastComputation.System.Structure.Modules
                .Single(module => string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal))
                .Length;

        private static void AssertParkedIntentIsIntact(RackPushBackSystemWindow window)
        {
            Assert.Equal(
                40.0,
                window.CompositeState.DormantCompositeTail.Single(module => module.ModuleId == "B:M2").Length,
                6);
            Assert.Contains(
                window.CompositeState.DormantTailLineOverrides,
                line => line.PostIndex == 1 && line.ModuleId == "B:M6");
        }

        private static void AssertCompositeCustomizationsAreBack(RackPushBackSystemWindow window)
        {
            Assert.True(window.LastComputation.Design.IsComposite);
            Assert.Equal(30.0, LengthOf(window, "M2"), 6);
            Assert.Equal(40.0, LengthOf(window, "B:M2"), 6);
            Assert.Equal(
                137.0,
                window.LastComputation.System.Structure.HeaderLineOverrides
                    .Single(line => line.PostIndex == 1 && line.ModuleId == "B:M6").Header.Height,
                6);
        }

        /// <summary>Deja la ventana con la cola aparcada y una candidatura RANCIA de un despertar fallido.</summary>
        private static RackPushBackSystemWindow WithStaleCandidate()
        {
            var window = Customized();
            Check(window, "SideBPresentCheck").IsChecked = false;
            AssertParkedIntentIsIntact(window);

            BreakSideA(window, broken: true);
            Recompute(window);
            Assert.False(window.CurrentInputsAreValid);

            Check(window, "SideBPresentCheck").IsChecked = true;
            ActivateBSlots(window);
            Recompute(window);
            Assert.False(window.CurrentInputsAreValid);   // el despertar fallo: la cola sigue aparcada
            AssertParkedIntentIsIntact(window);
            return window;
        }

        // ---------------------------------------------------------------- A5V-1

        [Fact]
        public void DormantTail_FailedWakeThenSingleSidedSuccess_DoesNotConsumeParkedIntent()
        {
            StaTestRunner.Run(() =>
            {
                var window = WithStaleCandidate();

                // Se vuelve a dormir el lado B y se corrige el fallo: recalculo VALIDO de un solo sentido.
                Check(window, "SideBPresentCheck").IsChecked = false;
                BreakSideA(window, broken: false);
                Recompute(window);
                Assert.True(window.CurrentInputsAreValid);
                Assert.False(window.LastComputation.Design.IsComposite);

                // Esa computacion no leyo la cola, asi que no puede consumirla.
                AssertParkedIntentIsIntact(window);
                Assert.False(window.CompositeState.DormantTailConsumptionPending);

                // Y al declarar otra vez el lado B, vuelve entera.
                Check(window, "SideBPresentCheck").IsChecked = true;
                ActivateBSlots(window);
                Recompute(window);
                AssertCompositeCustomizationsAreBack(window);
            });
        }

        [Fact]
        public void DormantTail_ConsumptionCandidateDoesNotSurviveIntoNextBuild()
        {
            StaTestRunner.Run(() =>
            {
                var window = WithStaleCandidate();

                // El armado SIGUIENTE abre su propio intento: la candidatura del anterior no llega hasta el.
                Check(window, "SideBPresentCheck").IsChecked = false;
                BreakSideA(window, broken: false);
                Recompute(window);

                Assert.False(window.CompositeState.DormantTailConsumptionPending);
                AssertParkedIntentIsIntact(window);
            });
        }

        [Fact]
        public void SingleSidedRecompute_NeverCommitsDormantTailConsumption()
        {
            StaTestRunner.Run(() =>
            {
                var window = WithStaleCandidate();
                Check(window, "SideBPresentCheck").IsChecked = false;
                BreakSideA(window, broken: false);

                // Varios recalculos validos de un solo sentido, incluido el que sigue al armado compuesto fallido.
                for (var round = 0; round < 3; round++)
                {
                    Recompute(window);
                    Assert.True(window.CurrentInputsAreValid);
                    Assert.False(window.LastComputation.Design.IsComposite);
                    AssertParkedIntentIsIntact(window);
                    Assert.False(window.CompositeState.DormantTailConsumptionPending);
                }
            });
        }

        // ---------------------------------------------------------------- lo que SI consume

        [Fact]
        public void DormantTail_SuccessfulCompositeWakeStillConsumesExactlyOnce()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();
                Check(window, "SideBPresentCheck").IsChecked = false;
                Assert.NotEmpty(window.CompositeState.DormantCompositeTail);

                Check(window, "SideBPresentCheck").IsChecked = true;
                ActivateBSlots(window);
                Recompute(window);

                AssertCompositeCustomizationsAreBack(window);
                Assert.Empty(window.CompositeState.DormantCompositeTail);
                Assert.Empty(window.CompositeState.DormantTailLineOverrides);
                Assert.False(window.CompositeState.DormantTailConsumptionPending);

                // Un segundo recalculo no duplica nada: la cola ya no existe y el rack sigue igual.
                Recompute(window);
                AssertCompositeCustomizationsAreBack(window);
                Assert.Single(
                    window.LastComputation.System.Structure.Modules.Where(module => module.ModuleId == "B:M2"));
            });
        }

        [Fact]
        public void DormantTail_FailedCompositeWakeThenCompositeRetryRestoresAndConsumes()
        {
            StaTestRunner.Run(() =>
            {
                var window = WithStaleCandidate();

                // Se corrige SIN dormir el lado B: el reintento compuesto restaura y consume.
                BreakSideA(window, broken: false);
                Recompute(window);

                Assert.True(window.CurrentInputsAreValid);
                AssertCompositeCustomizationsAreBack(window);
                Assert.Empty(window.CompositeState.DormantCompositeTail);
                Assert.False(window.CompositeState.DormantTailConsumptionPending);
            });
        }

        // ---------------------------------------------------------------- A5V-2: aparcar no borra

        [Fact]
        public void ParkingEmptySource_DoesNotEraseExistingDormantIntent()
        {
            StaTestRunner.Run(() =>
            {
                // Un recalculo de un solo sentido vuelve a aparcar: su diseño ya no lleva lineas de la cola, y eso
                // NO puede borrar las aparcadas. Es una ruta neutra —ni carga ni restaurar—, sin decision de dueño.
                var window = Customized();
                Check(window, "SideBPresentCheck").IsChecked = false;
                AssertParkedIntentIsIntact(window);

                Recompute(window);
                Recompute(window);

                AssertParkedIntentIsIntact(window);

                // Y el contrato vive en la autoridad, no en la ruta: aparcar una fuente VACIA no borra nada.
                window.CompositeState.ParkDormantTail(null, null);
                AssertParkedIntentIsIntact(window);
                window.CompositeState.ParkDormantTail(
                    Array.Empty<RackCad.Domain.Systems.Dynamic.DynamicRackModuleDesign>(),
                    Array.Empty<RackCad.Domain.Systems.Dynamic.DynamicHeaderLineOverride>());
                AssertParkedIntentIsIntact(window);
            });
        }

        [Fact]
        public void ParkingDormantTail_InvalidatesPreviousConsumptionCandidate()
        {
            StaTestRunner.Run(() =>
            {
                var window = WithStaleCandidate();
                Assert.True(window.CompositeState.DormantTailConsumptionPending);

                // Aparcar de nuevo —aunque sea la misma cola— invalida cualquier candidatura anterior.
                window.CompositeState.ParkDormantTail(
                    window.CompositeState.DormantCompositeTail.ToList(),
                    window.CompositeState.DormantTailLineOverrides.ToList());

                Assert.False(window.CompositeState.DormantTailConsumptionPending);
                AssertParkedIntentIsIntact(window);
            });
        }

        [Fact]
        public void LoadingAnotherRack_ClearsTheDormantIntentExplicitly()
        {
            StaTestRunner.Run(() =>
            {
                // El borrado explicito sigue existiendo, y su unica accion es CARGAR otro rack: la cola aparcada
                // pertenece al que estaba abierto y no puede resucitar en el siguiente.
                var window = Customized();
                Check(window, "SideBPresentCheck").IsChecked = false;
                AssertParkedIntentIsIntact(window);

                var other = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var simple = PushBackDesignDocument.FromDomain(other.LastComputation.Design).ToDomain();
                Assert.False(simple.IsComposite);

                window.LoadDesignForNew(simple, "Otro rack");

                Assert.Empty(window.CompositeState.DormantCompositeTail);
                Assert.Empty(window.CompositeState.DormantTailLineOverrides);
            });
        }
    }
}
