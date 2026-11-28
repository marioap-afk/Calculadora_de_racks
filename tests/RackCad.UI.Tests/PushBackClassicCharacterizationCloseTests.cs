using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using RackCad.UI.Controls;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// F4-T1B (I-64) — CARACTERIZACION de los caminos de cierre de la ventana CLASICA de Push Back
    /// (<see cref="RackPushBackSystemWindow"/>): Escape, «Cerrar», el cierre en modal, <c>Close()</c> (que hace las
    /// veces de la X y de Alt+F4), el cierre que acompana a Insertar y Actualizar, y el cierre con ediciones fuera del
    /// ambito declarado. CT-02 a CT-06 y CT-28 del paquete v5.
    ///
    /// <para><b>Fija lo que HAY, no lo que ADR-0029 querria</b> (D13). Lo que contradice a ADR-0029 va marcado
    /// «INCUMPLIMIENTO CARACTERIZADO» y NO se corrige aqui: la deuda queda registrada (D-T1B-06).</para>
    ///
    /// <para>Los gestos de teclado se ejercen por la ruta de la tecla de acceso (<c>AccessKeyManager.ProcessKey</c>
    /// sobre el <c>PresentationSource</c> de la ventana mostrada). Su CONTROL POSITIVO, medido sobre la base, es el
    /// primer caso de CT-02: sin trabajo pendiente, el mismo gesto cierra la ventana. Lo que se caracteriza es la ruta
    /// de la tecla de acceso, no el enrutado desde el elemento con foco. «Cerrar» se pulsa por el patron de
    /// automatizacion (<c>ButtonAutomationPeer</c> + <c>IInvokeProvider</c>), que pasa por <c>Button.OnClick</c>, no
    /// por un <c>ClickEvent</c> elevado a mano.</para>
    ///
    /// <para>Ninguna prueba muestra un <c>MessageBox</c> real: el aviso de descarte se sustituye (y se serializa en
    /// <see cref="EditorDiscardPromptCollection"/>). Ningun <c>Assert</c> va dentro de un delegado que ejecuta el
    /// producto: los valores se capturan dentro y se aseveran fuera.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackClassicCharacterizationCloseTests
    {
        private const string EscapeKey = "\x1B";

        private const string PendingQuestion =
            "Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?";

        /// <summary>El aviso de descarte sustituido: cuenta, recuerda la pregunta y el dueno, y responde lo que se le diga.</summary>
        private sealed class PromptProbe
        {
            public PromptProbe(bool answer) { Answer = answer; }

            public bool Answer { get; set; }
            public int Asked { get; private set; }
            public List<string> Questions { get; } = new List<string>();
            public bool EveryOwnerWasNull { get; private set; } = true;

            public bool Ask(string question, Window owner)
            {
                Asked++;
                Questions.Add(question);
                if (owner != null)
                {
                    EveryOwnerWasNull = false;
                }

                return Answer;
            }
        }

        private static RackPushBackSystemWindow Shown()
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                ShowActivated = false,
                Left = -10000,
                Top = -10000,
            };
            window.Show();
            window.UpdateLayout();
            return window;
        }

        private static ComboBox Combo(Window window, string name) => (ComboBox)window.FindName(name);
        private static NumericField Field(Window window, string name) => (NumericField)window.FindName(name);
        private static Button Btn(Window window, string name) => (Button)window.FindName(name);
        private static void Click(ButtonBase button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        /// <summary>Escenifica una edicion de modulo SIN confirmarla: es el unico ambito que la ventana declara.</summary>
        private static void StageModuleEdit(RackPushBackSystemWindow window)
        {
            ((CheckBox)window.FindName("AdvancedModulesToggle")).IsChecked = true;
            var module = window.EditorStateForTest.ModuleSession.Modules.First(m => m.IsLengthBearing);
            Combo(window, "ModuleBox").SelectedIndex = module.Index;
            var length = Field(window, "ModuleLengthBox");
            length.SetNumber(module.Length + 8.0);
            length.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));
        }

        /// <summary>Bombea el dispatcher por prioridad, sin esperas fijas ni temporizadores.</summary>
        private static void Pump()
            => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

        /// <summary>El gesto SIN aserciones: es el que se encola dentro de un delegado (CT-04).</summary>
        private static void RawEscape(Window window)
            => AccessKeyManager.ProcessKey(PresentationSource.FromVisual(window), EscapeKey, false);

        private static void PressEscape(Window window)
        {
            Assert.NotNull(PresentationSource.FromVisual(window));
            RawEscape(window);
        }

        private static void InvokeClose(Window window)
        {
            var button = Btn(window, "CloseButton");
            Assert.True(button.IsEnabled);
            var invoke = new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            Assert.NotNull(invoke);
            invoke.Invoke();
            Pump();
        }

        private static void CloseQuietly(RackPushBackSystemWindow window, PromptProbe probe)
        {
            probe.Answer = true;
            if (window.IsVisible)
            {
                window.Close();
            }
        }

        // ================================================================================== CT-02 Escape (no modal)

        [Fact]
        public void EscapeWithoutPendingWorkClosesWithoutAsking()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        Assert.True(window.IsVisible);
                        Assert.False(window.PendingWork().HasPendingWork);

                        // CONTROL POSITIVO del gesto (medido sobre la base): sin trabajo pendiente, Escape por la ruta de
                        // la tecla de acceso llega al boton IsCancel y la ventana se cierra.
                        PressEscape(window);

                        Assert.True(closed, "Escape sin trabajo pendiente debe cerrar la ventana");
                        Assert.False(window.IsVisible);
                        Assert.Equal(0, probe.Asked);
                        Assert.False(window.InsertRequested);
                        Assert.Null(window.InsertionRequest);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        [Fact]
        public void EscapeWithAStagedModuleAsksOnceWithTheExactQuestionAndNoOwner()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(false);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        StageModuleEdit(window);
                        Assert.True(window.PendingWork().HasPendingWork);

                        PressEscape(window);

                        Assert.Equal(1, probe.Asked);
                        Assert.Equal(PendingQuestion, probe.Questions[0]);
                        Assert.True(probe.EveryOwnerWasNull, "el aviso de descarte no tiene dueno WPF explicito");
                        Assert.False(closed);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        [Fact]
        public void EscapeAnsweredNoKeepsTheWindowWithItsPendingWorkAndNoRequest()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(false);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        StageModuleEdit(window);
                        Assert.True(window.PendingWork().HasPendingWork);

                        PressEscape(window);

                        Assert.Equal(1, probe.Asked);
                        Assert.Equal(PendingQuestion, probe.Questions[0]);
                        Assert.False(closed);
                        Assert.True(window.IsVisible);
                        Assert.True(window.PendingWork().HasPendingWork);
                        Assert.False(window.InsertRequested);
                        Assert.Null(window.InsertionRequest);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        [Fact]
        public void EscapeAnsweredYesClosesWithoutARequestAndTheStagedEditIsNotConfirmed()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        var totalBefore = window.EditorStateForTest.WorkingBaseline.Structure.TotalLength;
                        StageModuleEdit(window);
                        Assert.True(window.PendingWork().HasPendingWork);

                        PressEscape(window);

                        Assert.Equal(1, probe.Asked);
                        Assert.Equal(PendingQuestion, probe.Questions[0]);
                        Assert.True(closed);
                        Assert.False(window.InsertRequested);
                        Assert.Null(window.InsertionRequest);
                        // La edicion escenificada no se confirmo: seguia pendiente y la estructura no cambio.
                        Assert.True(window.PendingWork().HasPendingWork);
                        Assert.Equal(totalBefore, window.EditorStateForTest.WorkingBaseline.Structure.TotalLength, 4);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        // ============================================================================== CT-03 «Cerrar» real (peer)

        [Fact]
        public void CloseButtonWithoutPendingWorkClosesWithoutAsking()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        Assert.False(window.PendingWork().HasPendingWork);

                        InvokeClose(window);

                        Assert.True(closed, "«Cerrar» sin trabajo pendiente debe cerrar la ventana");
                        Assert.Equal(0, probe.Asked);
                        Assert.False(window.InsertRequested);
                        Assert.Null(window.InsertionRequest);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        [Fact]
        public void CloseButtonWithAStagedModuleAsksOnceWithTheExactQuestionAndNoOwner()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(false);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        StageModuleEdit(window);
                        Assert.True(window.PendingWork().HasPendingWork);

                        InvokeClose(window);

                        Assert.Equal(1, probe.Asked);
                        Assert.Equal(PendingQuestion, probe.Questions[0]);
                        Assert.True(probe.EveryOwnerWasNull, "el aviso de descarte no tiene dueno WPF explicito");
                        Assert.False(closed);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        [Fact]
        public void CloseButtonAnsweredNoKeepsTheWindowWithItsPendingWorkAndNoRequest()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(false);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        StageModuleEdit(window);
                        Assert.True(window.PendingWork().HasPendingWork);

                        InvokeClose(window);

                        Assert.Equal(1, probe.Asked);
                        Assert.Equal(PendingQuestion, probe.Questions[0]);
                        Assert.False(closed);
                        Assert.True(window.IsVisible);
                        Assert.True(window.PendingWork().HasPendingWork);
                        Assert.False(window.InsertRequested);
                        Assert.Null(window.InsertionRequest);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        [Fact]
        public void CloseButtonAnsweredYesClosesWithoutARequestAndTheStagedEditIsNotConfirmed()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        var totalBefore = window.EditorStateForTest.WorkingBaseline.Structure.TotalLength;
                        StageModuleEdit(window);
                        Assert.True(window.PendingWork().HasPendingWork);

                        InvokeClose(window);

                        Assert.Equal(1, probe.Asked);
                        Assert.Equal(PendingQuestion, probe.Questions[0]);
                        Assert.True(closed);
                        Assert.False(window.InsertRequested);
                        Assert.Null(window.InsertionRequest);
                        Assert.True(window.PendingWork().HasPendingWork);
                        Assert.Equal(totalBefore, window.EditorStateForTest.WorkingBaseline.Structure.TotalLength, 4);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        // ======================================================================================= CT-04 modal (H-08)

        /// <summary>Lo que un <c>ShowDialog</c> real con una edicion de modulo escenificada deja observable.</summary>
        private sealed class ModalOutcome
        {
            public int Asked;
            public List<string> Questions = new List<string>();
            public bool PendingAtTheEnd;
            public bool VisibleAtTheEnd;
            public bool Closed;
            public bool InsertRequested;
            public Exception Captured;
        }

        /// <summary>
        /// Abre la ventana con <c>ShowDialog</c> (fuera de pantalla, sin activar) con las acciones ENCOLADAS antes: la
        /// edicion de modulo escenificada, el gesto de cierre y, a <c>ApplicationIdle</c>, el RESPALDO que garantiza que
        /// ningun modal queda vivo: cancela el modulo y cierra, sin lanzar nunca (conmuta una bandera local que lee el
        /// aviso sustituido). Cada accion captura su excepcion; la prueba la relanza cuando <c>ShowDialog</c> vuelve y
        /// aborta toda operacion que siga pendiente. Los valores se aseveran FUERA, despues.
        /// </summary>
        private static ModalOutcome RunModal(Action<RackPushBackSystemWindow> gesture)
        {
            var outcome = new ModalOutcome();
            var fallbackRan = false;
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                ShowActivated = false,
                Left = -10000,
                Top = -10000,
            };
            window.Closed += (s, e) => outcome.Closed = true;
            var operations = new List<DispatcherOperation>();
            using (EditorDiscardPrompt.Substitute((question, owner) =>
                   {
                       outcome.Asked++;
                       outcome.Questions.Add(question);
                       return fallbackRan;
                   }))
            {
                operations.Add(window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    try { StageModuleEdit(window); }
                    catch (Exception ex) { outcome.Captured = outcome.Captured ?? ex; }
                })));
                operations.Add(window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    try { gesture(window); }
                    catch (Exception ex) { outcome.Captured = outcome.Captured ?? ex; }
                })));
                operations.Add(window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                {
                    try
                    {
                        outcome.PendingAtTheEnd = window.PendingWork().HasPendingWork;
                        outcome.VisibleAtTheEnd = window.IsVisible;
                        outcome.InsertRequested = window.InsertRequested;
                        fallbackRan = true;
                        Click(Btn(window, "CancelModuleButton"));
                    }
                    catch (Exception ex) { outcome.Captured = outcome.Captured ?? ex; }
                    finally
                    {
                        try
                        {
                            if (window.IsVisible)
                            {
                                window.Close();
                            }
                        }
                        catch (Exception ex) { outcome.Captured = outcome.Captured ?? ex; }
                    }
                })));

                window.ShowDialog();

                foreach (var operation in operations)
                {
                    if (operation.Status == DispatcherOperationStatus.Pending)
                    {
                        operation.Abort();
                    }
                }
            }

            if (outcome.Captured != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(outcome.Captured).Throw();
            }

            return outcome;
        }

        [Fact]
        public void ModalCloseButtonAnsweredNoAsksTwiceInTheDialogLoop()
        {
            StaTestRunner.Run(() =>
            {
                // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D7, OD-7): dentro de ShowDialog, «Cerrar» es IsCancel, asi que
                // WPF ejecuta ademas el cierre de dialogo y la politica de cierre pregunta DOS veces ante un «No».
                var outcome = RunModal(window =>
                {
                    var invoke = new ButtonAutomationPeer(Btn(window, "CloseButton")).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invoke.Invoke();
                });

                Assert.Equal(2, outcome.Asked);
                Assert.Equal(PendingQuestion, outcome.Questions[0]);
                Assert.Equal(PendingQuestion, outcome.Questions[1]);
                Assert.True(outcome.PendingAtTheEnd);
                Assert.True(outcome.VisibleAtTheEnd);
                Assert.False(outcome.InsertRequested);
                Assert.True(outcome.Closed);
            });
        }

        [Fact]
        public void ModalEscapeAnsweredNoAsksTwiceInTheDialogLoop()
        {
            StaTestRunner.Run(() =>
            {
                // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D7, OD-7): el mismo efecto por la tecla Escape.
                var outcome = RunModal(RawEscape);

                Assert.Equal(2, outcome.Asked);
                Assert.Equal(PendingQuestion, outcome.Questions[0]);
                Assert.Equal(PendingQuestion, outcome.Questions[1]);
                Assert.True(outcome.PendingAtTheEnd);
                Assert.True(outcome.VisibleAtTheEnd);
                Assert.False(outcome.InsertRequested);
                Assert.True(outcome.Closed);
            });
        }

        // ================================================================== CT-05 Close() (X y Alt+F4), no mostrada y mostrada

        private static void AssertInitialContractAfterAPlainClose(RackPushBackSystemWindow window, bool closed, PromptProbe probe)
        {
            Assert.True(closed);
            Assert.Equal(0, probe.Asked);
            Assert.False(window.InsertRequested);
            Assert.False(window.UpdateOnly);
            Assert.Null(window.InsertView);
            Assert.Equal(-1, window.InsertSection);
            Assert.Null(window.InsertionRequest);
            Assert.Null(window.SystemToInsert);
            Assert.Null(window.DesignToInsert);
            Assert.Null(window.SourceProjectToInsert);
            Assert.Null(window.RackId);
        }

        [Fact]
        public void PlainCloseOfAnUnshownWindowAsksNothingAndKeepsTheInitialContract()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    ((TextBox)window.FindName("NameBox")).Text = "  Rack X  ";

                    window.Close();

                    AssertInitialContractAfterAPlainClose(window, closed, probe);
                    Assert.Equal("Rack X", window.RackName);
                }
            });
        }

        [Fact]
        public void PlainCloseOfAShownWindowAsksNothingAndKeepsTheInitialContract()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        Assert.True(window.IsVisible);
                        ((TextBox)window.FindName("NameBox")).Text = "  Rack X  ";

                        window.Close();

                        AssertInitialContractAfterAPlainClose(window, closed, probe);
                        Assert.Equal("Rack X", window.RackName);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        // ================================================================== CT-06 el cierre que acompana a la peticion

        [Theory]
        [InlineData("InsertButton")]
        [InlineData("InsertLateralButton")]
        [InlineData("InsertFrontalEntradaButton")]
        [InlineData("InsertFrontalPosteriorButton")]
        [InlineData("InsertPlantaButton")]
        public void EveryInsertActionClosesTheWindowWithoutAsking(string buttonName)
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        Click(Btn(window, buttonName));

                        Assert.True(closed, buttonName + " debe cerrar la ventana al pedir la insercion");
                        Assert.Equal(0, probe.Asked);
                        Assert.True(window.InsertRequested);
                        Assert.False(window.UpdateOnly);
                    }
                    finally { if (!closed) { probe.Answer = true; window.Close(); } }
                }
            });
        }

        [Fact]
        public void UpdateOfAnExistingRackClosesTheWindowWithoutAsking()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                    window.LoadExisting(window.LastComputation.Design, "22222222-2222-2222-2222-222222222222", "Existente");
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        Click(Btn(window, "UpdateButton"));

                        Assert.True(closed, "Actualizar debe cerrar la ventana al pedir la actualizacion");
                        Assert.Equal(0, probe.Asked);
                        Assert.True(window.InsertRequested);
                        Assert.True(window.UpdateOnly);
                    }
                    finally { if (!closed) { probe.Answer = true; window.Close(); } }
                }
            });
        }

        [Fact]
        public void InsertWithAStagedModuleConfirmsItBeforeAndClosesWithoutAsking()
        {
            StaTestRunner.Run(() =>
            {
                var probe = new PromptProbe(true);
                using (EditorDiscardPrompt.Substitute(probe.Ask))
                {
                    var window = Shown();
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        StageModuleEdit(window);
                        Assert.True(window.PendingWork().HasPendingWork);

                        Click(Btn(window, "InsertButton"));

                        Assert.True(closed, "Insertar debe cerrar la ventana");
                        Assert.Equal(0, probe.Asked);
                        // Se confirma el modulo escenificado ANTES de pedir: no queda nada pendiente.
                        Assert.False(window.PendingWork().HasPendingWork);
                        Assert.True(window.InsertRequested);
                    }
                    finally { CloseQuietly(window, probe); }
                }
            });
        }

        // ================================================================== CT-28 cierre con ediciones fuera del ambito

        private static void AssertClosesWithoutAskingAndWithoutCommittingTheText(Action<RackPushBackSystemWindow> closeGesture)
        {
            var probe = new PromptProbe(true);
            using (EditorDiscardPrompt.Substitute(probe.Ask))
            {
                var window = Shown();
                var closed = false;
                window.Closed += (s, e) => closed = true;
                try
                {
                    // Una edicion YA APLICADA de la matriz (un frente nuevo): recalcula en el acto.
                    var initialPasses = window.RecomputePassesForTest;
                    Click(Btn(window, "AddFrontButton"));
                    Assert.True(window.RecomputePassesForTest > initialPasses);

                    // Y un campo numerico con texto SIN comprometer: se teclea y no se sale del campo.
                    var tolerance = Field(window, "ToleranceBox");
                    var typed = (tolerance.Value ?? 1.0) + 1.0;
                    var computation = window.LastComputation;
                    var passes = window.RecomputePassesForTest;
                    tolerance.SetNumber(typed);
                    Assert.Equal(passes, window.RecomputePassesForTest);
                    Assert.False(window.PendingWork().HasPendingWork);

                    closeGesture(window);

                    // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D7, OD-13): el texto sin comprometer se pierde en silencio.
                    Assert.True(closed);
                    Assert.Equal(0, probe.Asked);
                    Assert.Equal(passes, window.RecomputePassesForTest);
                    Assert.Same(computation, window.LastComputation);
                    Assert.False(window.InsertRequested);
                    Assert.Null(window.InsertionRequest);
                }
                finally { CloseQuietly(window, probe); }
            }
        }

        [Fact]
        public void EscapeWithAnUncommittedFieldAndAnAppliedMatrixEditClosesWithoutAskingOrRecomputing()
            => StaTestRunner.Run(() => AssertClosesWithoutAskingAndWithoutCommittingTheText(PressEscape));

        [Fact]
        public void PlainCloseWithAnUncommittedFieldAndAnAppliedMatrixEditClosesWithoutAskingOrRecomputing()
            => StaTestRunner.Run(() => AssertClosesWithoutAskingAndWithoutCommittingTheText(window => window.Close()));
    }
}
