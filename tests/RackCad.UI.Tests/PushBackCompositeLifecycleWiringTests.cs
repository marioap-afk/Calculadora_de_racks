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
    /// I-42 (A5-WIRE, contrato del dueño) — LA VENTANA NO DECIDE SI EL RACK ES COMPUESTO, Y LA COLA DORMIDA SE
    /// CONSUME AL ACEPTAR, NO AL ARMAR.
    ///
    /// <para>
    /// <b>A4V-1.</b> La autoridad de ciclo de vida vivia en Application, pero el recalculo de la ventana elegia
    /// ensamblador con un ternario sobre la casilla «Lado B»: sin ella, armaba por el camino de un solo sentido y se
    /// saltaba la compositividad efectiva, el aparcado de la cola y el filtrado del diseño. Medido por la ventana
    /// real: al desmarcar la casilla, <c>M2</c> del lado A volvia de 30" a 48", la cola NO se aparcaba, el informe
    /// declaraba conservados el hueco y <c>B:M2</c> que el diseño ya no tenia, y al volver a marcarla la mitad B
    /// regresaba estandar con su override de linea perdido.
    /// </para>
    ///
    /// <para>
    /// <b>A4V-2.</b> La cola se borraba en cuanto el armado la usaba. Pero la unidad transaccional de un recalculo
    /// es armar, resolver y ADOPTAR: medido, un recalculo que falla despues del armado —«al menos un frente debe
    /// permanecer activo»— dejaba la cola vacia, y el siguiente recalculo valido devolvia <c>B:M2</c> en 48" con el
    /// override perdido. Ahora el armado solo MARCA el consumo y quien adopta lo confirma.
    /// </para>
    /// </summary>
    public class PushBackCompositeLifecycleWiringTests
    {
        private static CheckBox Check(RackPushBackSystemWindow window, string name)
            => (CheckBox)window.FindName(name);

        private static NumericField Field(RackPushBackSystemWindow window, string name)
            => (NumericField)window.FindName(name);

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        /// <summary>Un recalculo natural de la ventana: se sale de un campo que no es de la celda.</summary>
        private static void Recompute(RackPushBackSystemWindow window)
            => LoseFocus(Field(window, "GapBox"));

        /// <summary>Un compuesto con personalizaciones de I-40 en los DOS lados y un override por linea en B.</summary>
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

        private static double LengthOf(RackPushBackSystemWindow window, string moduleId)
            => window.LastComputation.System.Structure.Modules
                .Single(module => string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal))
                .Length;

        private static bool HasTailModules(RackPushBackSystemWindow window)
            => window.LastComputation.System.Structure.Modules
                .Any(module => PushBackCompositeStructure.IsCompositeTailId(module.ModuleId));

        private static double LineOverride(RackPushBackSystemWindow window, int postIndex, string moduleId)
            => window.LastComputation.System.Structure.HeaderLineOverrides
                .Single(line => line.PostIndex == postIndex
                    && string.Equals(line.ModuleId, moduleId, StringComparison.Ordinal))
                .Header.Height;

        private static void AssertCompositeCustomizationsAreBack(RackPushBackSystemWindow window)
        {
            Assert.True(window.LastComputation.Design.IsComposite);
            Assert.Equal(30.0, LengthOf(window, "M2"), 6);
            Assert.Equal(40.0, LengthOf(window, "B:M2"), 6);
            Assert.Equal(137.0, LineOverride(window, 1, "B:M6"), 6);
            Assert.Contains(
                window.LastComputation.System.Structure.Modules,
                module => string.Equals(module.ModuleId, "GAP", StringComparison.Ordinal));
        }

        // ---------------------------------------------------------------- A4V-1: el cableado de la ventana

        [Fact]
        public void SideBCheckbox_DisablePreservesAAndParksBCompositeIntent()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();
                AssertCompositeCustomizationsAreBack(window);

                Check(window, "SideBPresentCheck").IsChecked = false;

                // El diseño que se resuelve es el del lado A, y la personalizacion de ese lado no se pierde.
                Assert.False(window.LastComputation.Design.IsComposite);
                Assert.False(HasTailModules(window));
                Assert.Equal(30.0, LengthOf(window, "M2"), 6);

                // La intencion de la mitad B queda APARCADA, con su override de linea.
                Assert.Contains(window.CompositeState.DormantCompositeTail, module => module.ModuleId == "GAP");
                Assert.Equal(
                    40.0,
                    window.CompositeState.DormantCompositeTail.Single(module => module.ModuleId == "B:M2").Length,
                    6);
                Assert.Contains(
                    window.CompositeState.DormantTailLineOverrides,
                    line => line.PostIndex == 1 && line.ModuleId == "B:M6");

                // Y el informe habla solo de la secuencia resuelta.
                var report = window.CompositeState.SideA.LastModuleReconciliation;
                Assert.DoesNotContain("B:M2", report.Preserved);
                Assert.DoesNotContain("GAP", report.Preserved);
                Assert.False(report.LostAnything);
            });
        }

        [Fact]
        public void SideBCheckbox_ReenableRestoresDormantBCompositeIntent()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();
                Check(window, "SideBPresentCheck").IsChecked = false;
                Recompute(window);
                Recompute(window);
                Assert.Equal(30.0, LengthOf(window, "M2"), 6);

                Check(window, "SideBPresentCheck").IsChecked = true;
                ActivateBSlots(window);
                Recompute(window);

                AssertCompositeCustomizationsAreBack(window);
            });
        }

        [Fact]
        public void SideBPresentButNoEffectiveSlots_UsesSameCompositeLifecycleAuthority()
        {
            StaTestRunner.Run(() =>
            {
                // El interruptor sigue marcado: lo que hace el rack de un solo sentido es no tener ranuras efectivas.
                var window = Customized();
                ActivateBSlots(window, active: false);
                Recompute(window);

                Assert.True(window.CompositeState.SideBPresent);
                Assert.False(window.LastComputation.Design.IsComposite);
                Assert.False(HasTailModules(window));
                Assert.Equal(30.0, LengthOf(window, "M2"), 6);
                Assert.Contains(window.CompositeState.DormantCompositeTail, module => module.ModuleId == "B:M2");

                ActivateBSlots(window);
                Recompute(window);
                AssertCompositeCustomizationsAreBack(window);
            });
        }

        [Fact]
        public void SingleSidedPushBack_ThroughCompositeAssembler_RemainsSingleSided()
        {
            StaTestRunner.Run(() =>
            {
                // Un rack que jamas tuvo lado B: la ruta unica no puede volverlo compuesto por dentro.
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var session = window.CompositeState.SideA.ModuleSession;
                Assert.True(session.SetLength("M2", 30.0));
                window.CompositeState.SideA.CommitModuleEdits();
                Recompute(window);

                Assert.False(window.LastComputation.Design.IsComposite);
                Assert.Null(window.LastComputation.Design.SideB);
                Assert.False(HasTailModules(window));
                Assert.Empty(window.CompositeState.DormantCompositeTail);
                Assert.Equal(30.0, LengthOf(window, "M2"), 6);

                // Y lo que se guarda sigue siendo un documento de un solo sentido.
                var saved = PushBackDesignDocument.FromDomain(window.LastComputation.Design).ToDomain();
                Assert.False(saved.IsComposite);
                Assert.DoesNotContain(
                    saved.Structure.Modules,
                    module => PushBackCompositeStructure.IsCompositeTailId(module.ModuleId));
            });
        }

        // ---------------------------------------------------------------- A4V-2: la transaccion

        [Fact]
        public void DormantTail_FailedWakeRecomputeDoesNotConsumeAuthoredIntent()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();
                Check(window, "SideBPresentCheck").IsChecked = false;
                var parked = window.CompositeState.DormantCompositeTail.Count;
                Assert.True(parked > 0);

                // Se rompe algo que SOLO el resolver rechaza, con el lado B todavia dormido.
                BreakSideA(window, broken: true);
                Recompute(window);
                Assert.False(window.CurrentInputsAreValid);

                // Y ahora se despierta la cola en un recalculo que va a fallar.
                Check(window, "SideBPresentCheck").IsChecked = true;
                ActivateBSlots(window);
                Recompute(window);
                Assert.False(window.CurrentInputsAreValid);

                // La cola sigue entera: un recalculo que no se acepta no consume nada.
                Assert.Equal(parked, window.CompositeState.DormantCompositeTail.Count);
                Assert.Equal(
                    40.0,
                    window.CompositeState.DormantCompositeTail.Single(module => module.ModuleId == "B:M2").Length,
                    6);
                Assert.Contains(
                    window.CompositeState.DormantTailLineOverrides,
                    line => line.PostIndex == 1 && line.ModuleId == "B:M6");

                // Se corrige y el reintento devuelve la mitad B tal y como estaba.
                BreakSideA(window, broken: false);
                Recompute(window);
                Assert.True(window.CurrentInputsAreValid);
                AssertCompositeCustomizationsAreBack(window);
            });
        }

        [Fact]
        public void DormantTail_IsClearedOnlyAfterSuccessfulAcceptedCompositeRecompute()
        {
            StaTestRunner.Run(() =>
            {
                var window = Customized();
                Check(window, "SideBPresentCheck").IsChecked = false;
                Assert.NotEmpty(window.CompositeState.DormantCompositeTail);

                Check(window, "SideBPresentCheck").IsChecked = true;
                ActivateBSlots(window);
                Recompute(window);

                // Aceptada: la cola ya esta en el rack y no puede quedar una segunda autoridad viva.
                Assert.True(window.CurrentInputsAreValid);
                AssertCompositeCustomizationsAreBack(window);
                Assert.Empty(window.CompositeState.DormantCompositeTail);
                Assert.Empty(window.CompositeState.DormantTailLineOverrides);
                Assert.False(window.CompositeState.DormantTailConsumptionPending);
            });
        }

        [Fact]
        public void LoadExistingComposite_FailureAndRetryDoNotDegradeThePersistedCustomizations()
        {
            StaTestRunner.Run(() =>
            {
                var saved = PushBackDesignDocument.FromDomain(Customized().LastComputation.Design).ToDomain();

                var reopened = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                reopened.LoadExisting(saved, "GUID-A5-WIRE", "PB compuesto");
                Recompute(reopened);
                AssertCompositeCustomizationsAreBack(reopened);

                // HECHO medido: la carga misma hace un recalculo que se ACEPTA, asi que la cola persistida ya se
                // consumio —esta en el rack, no aparcada—. Por eso el escenario «primer recalculo fallido con cola
                // pendiente» no es alcanzable por esta ruta, y lo que hay que proteger es lo siguiente: que un fallo
                // POSTERIOR no degrade lo cargado.
                Assert.Empty(reopened.CompositeState.DormantCompositeTail);

                BreakSideA(reopened, broken: true);
                Recompute(reopened);
                Assert.False(reopened.CurrentInputsAreValid);

                BreakSideA(reopened, broken: false);
                Recompute(reopened);

                Assert.True(reopened.CurrentInputsAreValid);
                AssertCompositeCustomizationsAreBack(reopened);
            });
        }

    }
}
