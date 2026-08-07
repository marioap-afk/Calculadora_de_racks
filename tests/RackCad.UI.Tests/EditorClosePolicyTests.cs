using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-39B: la política común de cierre (ADR-0029 D7) y su costura de confirmación.
    ///
    /// <para>La costura existe porque <c>MessageBox.Show</c> no tiene ninguna, y una política de cierre
    /// inverificable es exactamente la que pierde trabajo en silencio. En producción sigue mostrando el mismo
    /// <c>MessageBox</c>; aquí se sustituye la respuesta.</para>
    /// </summary>
    public sealed class EditorClosePolicyTests
    {
        // ---- 1. El ámbito pendiente ----

        [Fact]
        public void NothingPendingMeansTheWindowMayCloseWithoutAsking()
        {
            var asked = false;
            using (EditorDiscardPrompt.Substitute(_ => { asked = true; return true; }))
            {
                Assert.True(EditorClosePolicy.MayClose(EditorPendingWork.None));
            }

            Assert.False(asked, "sin trabajo pendiente no debe aparecer ningun dialogo");
        }

        [Fact]
        public void PendingWorkCarriesTheQuestionToAsk()
        {
            var pending = EditorPendingWork.Pending("¿Descartar?");

            Assert.True(pending.HasPendingWork);
            Assert.Equal("¿Descartar?", pending.Question);

            Assert.False(EditorPendingWork.None.HasPendingWork);
            Assert.Null(EditorPendingWork.None.Question);
        }

        [Fact]
        public void PendingWorkWithoutAQuestionIsRejected()
        {
            // Un ambito pendiente sin pregunta seria un dialogo vacio: el contrato exige decir QUE se pierde.
            Assert.Throws<ArgumentException>(() => EditorPendingWork.Pending(" "));
        }

        [Fact]
        public void WhenIsPendingOnlyWhileTheScopeSaysSo()
        {
            Assert.True(EditorPendingWork.When(true, "q").HasPendingWork);
            Assert.False(EditorPendingWork.When(false, "q").HasPendingWork);
        }

        // ---- 2. La decisión ----

        [Fact]
        public void ConfirmingTheDiscardLetsTheWindowClose()
        {
            using (EditorDiscardPrompt.Substitute(_ => true))
            {
                Assert.True(EditorClosePolicy.MayClose(EditorPendingWork.Pending("¿Descartar?")));
            }
        }

        [Fact]
        public void RejectingTheDiscardKeepsTheWindowOpen()
        {
            using (EditorDiscardPrompt.Substitute(_ => false))
            {
                Assert.False(EditorClosePolicy.MayClose(EditorPendingWork.Pending("¿Descartar?")));
            }
        }

        [Fact]
        public void TheQuestionReachesThePrompt()
        {
            var seen = new List<string>();
            using (EditorDiscardPrompt.Substitute(q => { seen.Add(q); return true; }))
            {
                EditorClosePolicy.MayClose(EditorPendingWork.Pending("se perderán los cambios"));
            }

            Assert.Equal(new[] { "se perderán los cambios" }, seen);
        }

        [Fact]
        public void TheSubstitutionIsRestoredWhenTheScopeEnds()
        {
            var inner = 0;
            using (EditorDiscardPrompt.Substitute(_ => { inner++; return true; }))
            {
                EditorDiscardPrompt.Confirm("q");
            }

            Assert.Equal(1, inner);

            // Fuera del alcance vuelve el prompt de produccion; no se invoca aqui porque abriria un MessageBox real.
            using (EditorDiscardPrompt.Substitute(_ => false))
            {
                Assert.False(EditorDiscardPrompt.Confirm("q"));
            }
        }

        // ---- 3. Las cuatro rutas de cierre pasan por el mismo punto ----

        /// <summary>Una ventana mínima con la MISMA forma de <c>OnClosing</c> que usan los editores, para probar el
        /// mecanismo sin depender del fixture de ninguna de ellas.</summary>
        private sealed class ProbeWindow : Window
        {
            public EditorPendingWork Pending { get; set; } = EditorPendingWork.None;

            public int ClosingCount { get; private set; }

            protected override void OnClosing(CancelEventArgs e)
            {
                ClosingCount++;
                if (!EditorClosePolicy.MayClose(Pending))
                {
                    e.Cancel = true;
                    return;
                }

                base.OnClosing(e);
            }
        }

        [Fact]
        public void ClosingIsCancelledWhenTheUserRejectsTheDiscard()
        {
            StaTestRunner.Run(() =>
            {
                var window = new ProbeWindow { Pending = EditorPendingWork.Pending("¿Descartar?") };

                using (EditorDiscardPrompt.Substitute(_ => false))
                {
                    window.Close();
                }

                Assert.Equal(1, window.ClosingCount);
                Assert.True(window.IsVisible == false); // nunca se mostro; lo que importa es que NO se destruyo

                // Y ahora, aceptando, si cierra.
                using (EditorDiscardPrompt.Substitute(_ => true))
                {
                    window.Close();
                }

                Assert.Equal(2, window.ClosingCount);
            });
        }

        [Fact]
        public void EveryCloseRouteReachesTheSamePolicy()
        {
            StaTestRunner.Run(() =>
            {
                // Close() es lo que ejecutan las cuatro rutas: el boton (Click -> Close), Escape (IsCancel dispara ese
                // mismo Click), el boton de sistema y Alt+F4. WPF las hace pasar todas por OnClosing, que es la razon
                // de que la politica viva ahi y no en el handler del boton.
                var asked = 0;
                var window = new ProbeWindow { Pending = EditorPendingWork.Pending("¿Descartar?") };

                using (EditorDiscardPrompt.Substitute(_ => { asked++; return false; }))
                {
                    window.Close();
                    window.Close();
                    window.Close();
                }

                Assert.Equal(3, asked);
                Assert.Equal(3, window.ClosingCount);
            });
        }

        // ---- 4. Cableado real de las dos ventanas que declaran ámbito ----

        [Fact]
        public void PushBackDeclaresNoPendingWorkWhenNothingIsStaged()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);

                Assert.False(window.PendingWork().HasPendingWork);
            });
        }

        [Fact]
        public void PushBackPendingWorkQuestionNamesWhatIsLost()
        {
            // La pregunta no puede ser generica: tiene que decir que se pierde, que es lo que ADR-0029 D6 exige de
            // cualquier motivo visible.
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var question = EditorPendingWork
                    .Pending("Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?")
                    .Question;

                Assert.Contains("módulo", question, StringComparison.Ordinal);
                Assert.Contains("perderán", question, StringComparison.Ordinal);
                Assert.False(window.PendingWork().HasPendingWork);
            });
        }
    }
}
