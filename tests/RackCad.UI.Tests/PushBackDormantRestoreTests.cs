using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (A7-DORMANT-RESTORE, decision del dueño) — DORMIR NO ES BORRAR, TAMPOCO AL RESTAURAR.
    ///
    /// <para>
    /// <b>La decision.</b> «Restaurar valores» actua sobre el sistema EFECTIVO. Con el lado B dormido, su intencion
    /// no forma parte de ese sistema: ni se restaura ni se borra, y al despertar reaparece exactamente como estaba.
    /// Con el lado B ACTIVO el rack efectivo lo incluye, asi que el reset le alcanza como a cualquier otra parte.
    /// </para>
    ///
    /// <para>
    /// <b>Medido antes del cambio (A6V-1).</b> Restaurar con el lado B dormido dejaba la cola y sus lineas vacias
    /// —el restauro reutilizaba el borrado de la CARGA—, y al despertar <c>B:M2</c> volvia de 40" a 48" con el
    /// override (1, B:M6) perdido.
    /// </para>
    ///
    /// <para>
    /// <b>Medido antes del cambio (A6V-2).</b> El baseline solo avanza cuando una computacion se acepta, asi que una
    /// edicion CONFIRMADA cuyo recalculo fallo por otra causa vive en el commit y todavia no en el baseline: dormir
    /// el lado B aparcaba <c>B:M2 = 40"</c> y tiraba el 44" que el usuario ya habia confirmado.
    /// </para>
    /// </summary>
    public class PushBackDormantRestoreTests
    {
        private static CheckBox Check(RackPushBackSystemWindow window, string name)
            => (CheckBox)window.FindName(name);

        private static NumericField Field(RackPushBackSystemWindow window, string name)
            => (NumericField)window.FindName(name);

        private static Button Button(RackPushBackSystemWindow window, string name)
            => (Button)window.FindName(name);

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        private static void Click(ButtonBase button)
            => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

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

        /// <summary>Un compuesto con personalizaciones en los dos lados y un override por linea en B, aceptado.</summary>
        private static RackPushBackSystemWindow Customized(int slots = 1)
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            Check(window, "SideBPresentCheck").IsChecked = true;
            if (slots > 1)
            {
                window.CompositeState.SetSlotCount(slots);
            }

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

        private static double? Parked(RackPushBackSystemWindow window, string moduleId)
            => window.CompositeState.DormantCompositeTail
                .FirstOrDefault(module => string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal))?.Length;

        private static string SlotPattern(RackPushBackSystemWindow window)
        {
            var matrix = window.CompositeState.Of(PushBackSide.B).Structure;
            return string.Concat(Enumerable.Range(0, matrix.Count).Select(slot => matrix.IsActive(slot) ? "1" : "0"));
        }

        private static void AssertDormantIntentIsIntact(RackPushBackSystemWindow window)
        {
            Assert.Equal(40.0, Parked(window, "B:M2").Value, 6);
            Assert.Contains(window.CompositeState.DormantCompositeTail, module => module.ModuleId == "GAP");
            Assert.Contains(
                window.CompositeState.DormantTailLineOverrides,
                line => line.PostIndex == 1 && line.ModuleId == "B:M6");
        }

        private static void AssertWakesExactly(RackPushBackSystemWindow window)
        {
            Assert.True(window.LastComputation.Design.IsComposite);
            Assert.Equal(40.0, LengthOf(window, "B:M2"), 6);
            Assert.Equal(
                137.0,
                window.LastComputation.System.Structure.HeaderLineOverrides
                    .Single(line => line.PostIndex == 1 && line.ModuleId == "B:M6").Header.Height,
                6);
            Assert.Contains(
                window.LastComputation.System.Structure.Modules,
                module => string.Equals(module.ModuleId, "GAP", StringComparison.Ordinal));
        }

        // ---------------------------------------------------------------- A6V-1

        [Fact]
        public void DormantSideB_RestoreValuesPreservesDormantBIntent()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();
                Check(window, "SideBPresentCheck").IsChecked = false;
                AssertDormantIntentIsIntact(window);

                Click(Button(window, "RestoreButton"));

                // El restauro actua sobre el sistema efectivo; lo dormido no forma parte de el.
                AssertDormantIntentIsIntact(window);

                Check(window, "SideBPresentCheck").IsChecked = true;
                ActivateBSlots(window);
                Recompute(window);
                AssertWakesExactly(window);
            });
        }

        [Fact]
        public void ActiveSideB_RestoreValuesTreatsBExactlyLikeA()
        {
            StaTestRunner.Run(() =>
            {
                // MEDIDO: este boton «Restaurar» recarga el ULTIMO SISTEMA VALIDO —lo dice su propio estado: «Valores
                // restaurados al último sistema válido»—, no resetea a la receta estandar. Con el lado B ACTIVO el
                // rack efectivo incluye los dos lados y los dos se comportan IGUAL: ninguno queda exento. El reset
                // total a calculado es otra accion, la del panel de modulos («restaurar estandar», A3V-MOD-RESTORE),
                // y sigue cubierta por sus propias pruebas.
                var window = Customized();
                Assert.Equal(30.0, LengthOf(window, "M2"), 6);
                Assert.Equal(40.0, LengthOf(window, "B:M2"), 6);

                Click(Button(window, "RestoreButton"));
                Recompute(window);

                Assert.True(window.LastComputation.Design.IsComposite);
                Assert.Equal(30.0, LengthOf(window, "M2"), 6);
                Assert.Equal(40.0, LengthOf(window, "B:M2"), 6);

                // Y con el lado B activo no hay nada dormido que preservar: la cola sigue vacia.
                Assert.Empty(window.CompositeState.DormantCompositeTail);
            });
        }

        [Fact]
        public void DormantSideB_RestoreValuesPreservesSlotPresencePattern()
        {
            StaTestRunner.Run(() =>
            {
                // Un patron heterogeneo de ranuras: la presencia tambien es intencion dormida.
                var window = Customized(slots: 3);
                var matrix = window.CompositeState.Of(PushBackSide.B).Structure;
                matrix.Fronts[1].IsActive = false;
                Recompute(window);
                var pattern = SlotPattern(window);
                Assert.Equal("101", pattern);

                Check(window, "SideBPresentCheck").IsChecked = false;
                Click(Button(window, "RestoreButton"));

                Check(window, "SideBPresentCheck").IsChecked = true;
                Recompute(window);

                Assert.Equal(pattern, SlotPattern(window));
            });
        }

        // ---------------------------------------------------------------- A6V-2

        [Fact]
        public void DormantSideB_ParksConfirmedPendingBModuleIntent()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();
                Assert.Equal(
                    40.0,
                    window.CompositeState.SideA.WorkingBaseline.Structure.Modules
                        .Single(module => module.ModuleId == "B:M2").Length,
                    6);

                // El usuario cambia B:M2 y CONFIRMA; otra causa deja el recalculo invalido.
                Assert.True(window.CompositeState.SideA.ModuleSession.SetLength("B:M2", 44.0));
                var commit = window.CompositeState.SideA.CommitModuleEdits();
                Assert.Equal(44.0, commit.Modules.Single(module => module.ModuleId == "B:M2").Length, 6);
                BreakSideA(window, broken: true);
                Recompute(window);
                Assert.False(window.CurrentInputsAreValid);
                Assert.Equal(
                    40.0,
                    window.CompositeState.SideA.WorkingBaseline.Structure.Modules
                        .Single(module => module.ModuleId == "B:M2").Length,
                    6);   // el baseline no avanzo: la intencion vive en el commit

                Check(window, "SideBPresentCheck").IsChecked = false;

                // Dormir no descarta lo confirmado.
                Assert.Equal(44.0, Parked(window, "B:M2").Value, 6);

                BreakSideA(window, broken: false);
                Recompute(window);
                Check(window, "SideBPresentCheck").IsChecked = true;
                ActivateBSlots(window);
                Recompute(window);

                Assert.Equal(44.0, LengthOf(window, "B:M2"), 6);
                Assert.Single(
                    window.LastComputation.System.Structure.Modules.Where(module => module.ModuleId == "B:M2"));
            });
        }

        [Fact]
        public void DormantSideB_DoesNotParkUnconfirmedTextboxEdit()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();

                // Escenificado en la sesion pero SIN confirmar: no es intencion authored.
                Assert.True(window.CompositeState.SideA.ModuleSession.SetLength("B:M2", 46.0));

                Check(window, "SideBPresentCheck").IsChecked = false;

                Assert.Equal(40.0, Parked(window, "B:M2").Value, 6);
            });
        }

        [Fact]
        public void DormantSideB_PendingBCommitDoesNotBleedIntoSideA()
        {
            StaTestRunner.Run(() =>
            {
                // «M2» y «B:M2» son identidades distintas: proyectar el commit de la cola no puede tocar el lado A.
                var window = Customized();
                Assert.True(window.CompositeState.SideA.ModuleSession.SetLength("B:M2", 44.0));
                window.CompositeState.SideA.CommitModuleEdits();
                BreakSideA(window, broken: true);
                Recompute(window);

                Check(window, "SideBPresentCheck").IsChecked = false;

                Assert.Equal(44.0, Parked(window, "B:M2").Value, 6);
                Assert.Null(Parked(window, "M2"));

                BreakSideA(window, broken: false);
                Recompute(window);
                Assert.Equal(30.0, LengthOf(window, "M2"), 6);

                // Y el resto de la mitad B no se mueve.
                Assert.Equal(48.0, Parked(window, "B:M3").Value, 6);
            });
        }

        // ---------------------------------------------------------------- el contrato de carga sigue en pie

        [Fact]
        public void LoadingAnotherRack_StillClearsThePreviousDormantIntent()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();
                Check(window, "SideBPresentCheck").IsChecked = false;
                AssertDormantIntentIsIntact(window);

                var other = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var simple = PushBackDesignDocument.FromDomain(other.LastComputation.Design).ToDomain();
                window.LoadExisting(simple, "GUID-A7", "Otro rack");

                Assert.Empty(window.CompositeState.DormantCompositeTail);
                Assert.Empty(window.CompositeState.DormantTailLineOverrides);
            });
        }
    }
}
