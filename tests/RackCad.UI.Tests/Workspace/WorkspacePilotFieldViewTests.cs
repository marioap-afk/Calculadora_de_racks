#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4-T3B1-EDIT-SURFACE (INV-F4T3B1-02 to 04, 08): the field of the pilot in the real view, with a real TextBox on the STA thread of
    /// the suite and the double of the edit port. The gestures are raised as the input system raises them: the tunnel event for the
    /// previews (RaiseEvent does not chain the tunnel with the bubble) and the bubble event for the keys. The box has no handler of the
    /// bubble KeyDown: a character key reaches the editor without Handled and produces its text input (correction of attempt 2).
    /// Nothing reads a drawing and nothing is written.
    /// </summary>
    public class WorkspacePilotFieldViewTests
    {
        private static (WorkspacePanelView View, WorkspacePilotEditPortDouble Pilot, WorkspaceNavigationPortDouble Navigation) Build(bool withPilot = true)
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var navigation = new WorkspaceNavigationPortDouble();
            var pilot = new WorkspacePilotEditPortDouble();
            var view = new WorkspacePanelView(
                new WorkspacePanelViewModel(host, _ => { }, navigation, new WorkspaceBrowserSummaryPortDouble(), withPilot ? pilot : null));
            view.ViewModel.Refresh();
            Lay(view);
            return (view, pilot, navigation);
        }

        // The tab R-1 is pinned and its draft was opened by the first gesture: the box is editable.
        private static (WorkspacePanelView View, WorkspacePilotEditPortDouble Pilot, TextBox Box) Editable()
        {
            var (view, pilot, _) = Build();
            view.ViewModel.Browser.PinTab("R-1");
            view.ViewModel.Browser.BeginFieldEdit();
            Lay(view);
            var box = Named<TextBox>(view, "BrowserFieldBox");
            Assert.False(box.IsReadOnly);
            Assert.Equal("1:50", box.Text);
            pilot.Calls.Clear();
            return (view, pilot, box);
        }

        private static T Named<T>(WorkspacePanelView view, string name)
            where T : class
        {
            var found = view.FindName(name);
            Assert.NotNull(found);
            return Assert.IsAssignableFrom<T>(found);
        }

        private static void Lay(WorkspacePanelView view)
        {
            view.Measure(new Size(400, 6000));
            view.Arrange(new Rect(0, 0, 400, 6000));
            view.UpdateLayout();
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        private static Button TabButton(WorkspacePanelView view, string rack) =>
            Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip"))
                .Single(b => b.DataContext is WorkspaceBrowserTabItem item
                             && item.RackId == rack
                             && System.Windows.Automation.AutomationProperties.GetName(b) == "Activar pestaña");

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static KeyEventArgs RaiseKey(UIElement element, RoutedEvent routed, Key key, bool repeat = false)
        {
            using var source = new HwndSource(0, 0, 0, 0, 0, "t", IntPtr.Zero);
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = routed };
            if (repeat) MarkRepeat(args);
            element.RaiseEvent(args);
            return args;
        }

        // The auto-repeat of a held key: WPF marks it with an internal method of the arguments, which the system raises and a test cannot.
        private static void MarkRepeat(KeyEventArgs args)
        {
            var setter = typeof(KeyEventArgs).GetMethod("SetRepeat", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(setter);
            setter!.Invoke(args, new object[] { true });
            Assert.True(args.IsRepeat);
        }

        private static KeyEventArgs Preview(UIElement element, Key key, bool repeat = false) => RaiseKey(element, Keyboard.PreviewKeyDownEvent, key, repeat);

        private static KeyEventArgs Bubble(UIElement element, Key key) => RaiseKey(element, Keyboard.KeyDownEvent, key);

        private static void RaiseMouse(UIElement element, RoutedEvent routed) =>
            element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = routed });

        private static void LostFocus(UIElement element) => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        // ------------------------------------------------------------------ availability

        [Fact]
        public void TheFieldPanelIsShownOnlyWithThePilotPort()
        {
            StaTestRunner.Run(() =>
            {
                var (with, _, _) = Build();
                var (without, pilot, _) = Build(withPilot: false);

                Assert.Equal(Visibility.Visible, Named<StackPanel>(with, "BrowserFieldPanel").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<StackPanel>(without, "BrowserFieldPanel").Visibility);
                Assert.Empty(pilot.Calls);
            });
        }

        // ------------------------------------------------------------------ the first gesture

        [Fact]
        public void TheEditButtonPinsThePreviewAndAsksForTheIntentionAndTheBoxBecomesEditable()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");
                var button = Named<Button>(view, "BrowserFieldEditButton");
                bool? previewInsideTheRead = null;
                pilot.ReadHook = _ => previewInsideTheRead = view.ViewModel.Browser.Tabs.Single().IsPreview;
                Assert.True(box.IsReadOnly);
                Assert.Equal(Visibility.Visible, button.Visibility);

                Click(button);
                Lay(view);

                Assert.False(previewInsideTheRead);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
                Assert.False(box.IsReadOnly);
                Assert.Equal("1:50", box.Text);
                Assert.Equal(Visibility.Collapsed, button.Visibility);
                Assert.Equal("Listo para editar", Named<TextBlock>(view, "BrowserFieldStateText").Text);
            });
        }

        [Fact]
        public void AClickOnTheReadOnlyBoxIsTheFirstGestureAndAClickOnAnEditableBoxIsNot()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");
                bool? previewInsideTheRead = null;
                pilot.ReadHook = _ => previewInsideTheRead = view.ViewModel.Browser.Tabs.Single().IsPreview;
                Assert.True(view.ViewModel.Browser.Tabs.Single().IsPreview);

                RaiseMouse(box, UIElement.PreviewMouseLeftButtonDownEvent);

                // The tab is pinned inside the read, before it, and not after it.
                Assert.False(previewInsideTheRead);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
                Assert.False(box.IsReadOnly);

                RaiseMouse(box, UIElement.PreviewMouseLeftButtonDownEvent);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
            });
        }

        [Fact]
        public void AKeyOnTheReadOnlyBoxIsTheFirstGestureItIsHandledAndTheBoxTextDoesNotChange()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");
                Assert.True(box.IsReadOnly);
                var before = box.Text;
                bool? previewInsideTheRead = null;
                pilot.ReadHook = _ => previewInsideTheRead = view.ViewModel.Browser.Tabs.Single().IsPreview;
                Assert.True(view.ViewModel.Browser.Tabs.Single().IsPreview);

                var args = Preview(box, Key.D5);

                // The tab is pinned inside the read, before it, and not after it.
                Assert.False(previewInsideTheRead);
                Assert.True(args.Handled);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
                Assert.False(box.IsReadOnly);
                Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, before);
                Assert.Equal("1:50", box.Text);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            });
        }

        [Fact]
        public void OnAReadOnlyBoxEscIsHandledWithNoGestureAndAModifierOrTabLaunchesNothingAndIsNotHandled()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");

                Assert.True(Preview(box, Key.Escape).Handled);
                foreach (var key in new[] { Key.Tab, Key.LeftCtrl, Key.RightShift, Key.LeftAlt, Key.LWin })
                    Assert.False(Preview(box, key).Handled, key.ToString());

                Assert.DoesNotContain(pilot.Calls, c => c.Operation != WorkspacePilotEditPortDouble.GetFieldOp);
                Assert.True(box.IsReadOnly);
            });
        }

        [Fact]
        public void TheDoubleClickOnTheButtonOfTheActiveTabIsAGesture()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var browser = view.ViewModel.Browser;
                browser.PinTab("R-1");
                view.ViewModel.Refresh();
                Lay(view);

                RaiseMouse(TabButton(view, "R-1"), Control.MouseDoubleClickEvent);

                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
                Assert.False(Named<TextBox>(view, "BrowserFieldBox").IsReadOnly);
            });
        }

        [Fact]
        public void TheDoubleClickOnATabThatIsNotActiveAfterTheClicksCallsNothing()
        {
            StaTestRunner.Run(() =>
            {
                var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
                var pilot = new WorkspacePilotEditPortDouble { Validate = t => t == "bad" ? "La escala no es válida." : null };
                var view = new WorkspacePanelView(new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), new WorkspaceBrowserSummaryPortDouble(), pilot));
                view.ViewModel.Refresh();
                view.ViewModel.Browser.PinTab("R-1");
                view.ViewModel.Browser.BeginFieldEdit();
                // R-2 is a preview that is not the active tab: the selection opened it and the user went back to R-1, which has the draft.
                host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
                view.ViewModel.Refresh();
                view.ViewModel.Browser.ActivateTab("R-1");
                Assert.True(view.ViewModel.Browser.Tabs.Single(t => t.RackId == "R-2").IsPreview);
                view.ViewModel.Browser.Field.Text = "bad";
                Lay(view);
                pilot.Calls.Clear();

                // The click of the double click aborts the activation (the text is not valid); the second part must do nothing, and in
                // particular it must not pin the preview.
                var tab = TabButton(view, "R-2");
                Click(tab);
                RaiseMouse(tab, Control.MouseDoubleClickEvent);

                Assert.Equal("R-1", view.ViewModel.Browser.ActiveRackId);
                Assert.Empty(pilot.Reads());
                Assert.True(view.ViewModel.Browser.Tabs.Single(t => t.RackId == "R-2").IsPreview);
            });
        }

        [Fact]
        public void AKeyThatIsARepetitionOfAHeldKeyIsConsumedAndLaunchesNothing()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");
                Assert.True(box.IsReadOnly);
                Assert.Equal(Visibility.Visible, Named<Button>(view, "BrowserFieldEditButton").Visibility);

                var repeated = Preview(box, Key.Left, repeat: true);

                Assert.True(repeated.Handled);
                Assert.Empty(pilot.Reads());
                Assert.True(box.IsReadOnly);

                // The first press of the same key is the gesture.
                Assert.True(Preview(box, Key.Left).Handled);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
                Assert.False(box.IsReadOnly);
            });
        }

        [Fact]
        public void WithANotAdmittedDiagnosticOfTheSameSurfaceAClickAndAKeyAndTheDoubleClickDoNotReadAndOnlyTheEditButtonReadsAgain()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");
                var button = Named<Button>(view, "BrowserFieldEditButton");
                pilot.BeginResponder = _ => EditIntentOutcome.NotAdmitted(
                    "No editable aquí: el rack tiene una definición propia.", "detalle", "Usa RACKEDITAR para editarlo.", EditWitnessStatus.NotSupplied);
                Click(button);
                Lay(view);
                Assert.Single(pilot.Reads());
                Assert.Equal("No editable aquí", Named<TextBlock>(view, "BrowserFieldStateText").Text);
                Assert.Equal(Visibility.Visible, button.Visibility);

                RaiseMouse(box, UIElement.PreviewMouseLeftButtonDownEvent);
                Assert.True(Preview(box, Key.D5).Handled);
                RaiseMouse(TabButton(view, "R-1"), Control.MouseDoubleClickEvent);
                Lay(view);

                Assert.Single(pilot.Reads());
                Assert.Equal("No editable aquí: el rack tiene una definición propia.", Named<TextBlock>(view, "BrowserFieldNotEditableText").Text);
                Assert.Equal("Usa RACKEDITAR para editarlo.", Named<TextBlock>(view, "BrowserFieldHintText").Text);

                Click(button);
                Lay(view);

                Assert.Equal(2, pilot.Reads().Count());
            });
        }

        [Fact]
        public void WithAnErrorOfTheFieldNoClickNorKeyNorDoubleClickReadsAndOnlyReloadActs()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, box) = Editable();
                pilot.Throws[WorkspacePilotEditPortDouble.SetPendingOp] = new InvalidOperationException("falló el puerto");
                box.Text = "1:75";
                Lay(view);
                Assert.True(box.IsReadOnly);
                var reload = Named<Button>(view, "BrowserFieldReloadButton");
                Assert.Equal(Visibility.Visible, reload.Visibility);
                pilot.Throws.Clear();
                pilot.Calls.Clear();

                RaiseMouse(box, UIElement.PreviewMouseLeftButtonDownEvent);
                Assert.True(Preview(box, Key.D5).Handled);
                RaiseMouse(TabButton(view, "R-1"), Control.MouseDoubleClickEvent);

                Assert.Empty(pilot.Reads());
                Assert.True(box.IsReadOnly);
                Assert.Contains("falló el puerto", Named<TextBlock>(view, "BrowserFieldStateText").Text);

                Click(reload);
                Lay(view);

                Assert.Empty(pilot.Reads());
                Assert.False(box.IsReadOnly);
            });
        }

        [Fact]
        public void TheDoubleClickOnTheActiveTabWithAnEditableBoxDoesNothingAndKeepsTheNoticeOfTheSurface()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, box) = Editable();
                pilot.SetPendingResponder = (_, _, _) => PilotFieldResult.Busy("Hay una lectura en curso.");
                box.Text = "1:75";
                Lay(view);
                var notice = Named<TextBlock>(view, "BrowserFieldNoticeText").Text;
                Assert.Contains("Hay una lectura en curso.", notice);
                pilot.Calls.Clear();

                RaiseMouse(TabButton(view, "R-1"), Control.MouseDoubleClickEvent);
                Lay(view);

                Assert.Empty(pilot.Reads());
                Assert.Equal(notice, Named<TextBlock>(view, "BrowserFieldNoticeText").Text);
                Assert.False(box.IsReadOnly);
            });
        }

        // ------------------------------------------------------------------ editing and the keys

        [Fact]
        public void Enter_OnAnEditableBoxWithPendingTextCommitsAndIsHandledAndEscIsHandledWithoutChangingAnything()
        {
            StaTestRunner.Run(() =>
            {
                var (_, pilot, box) = Editable();
                box.Text = "1:75";
                pilot.Calls.Clear();

                Assert.True(Preview(box, Key.Escape).Handled);
                Assert.DoesNotContain(pilot.Calls, c => c.Operation != WorkspacePilotEditPortDouble.GetFieldOp);
                Assert.Equal("1:75", box.Text);

                Assert.True(Preview(box, Key.Enter).Handled);

                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
                Assert.Equal("1:75", box.Text);
            });
        }

        [Fact]
        public void LeavingTheBoxCommitsOnlyWithPendingText()
        {
            StaTestRunner.Run(() =>
            {
                var (_, pilot, box) = Editable();

                LostFocus(box);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));

                box.Text = "1:75";
                LostFocus(box);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            });
        }

        [Fact]
        public void AnInvalidTextStaysInTheBoxWithItsMessageAfterEnter()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, box) = Editable();
                pilot.Validate = t => t == "bad" ? "La escala no es válida." : null;

                box.Text = "bad";
                Preview(box, Key.Enter);

                Assert.Equal("bad", box.Text);
                Assert.Equal("La escala no es válida.", Named<TextBlock>(view, "BrowserFieldMessageText").Text);
                Assert.False(box.IsReadOnly);
            });
        }

        // The keys that produce a character reach the editor untouched: the KeyDown of the bubble is not marked by the panel, so the text input is
        // generated (a KeyDown marked Handled suppresses it, as the experiment of the session showed). Only Enter and Esc are consumed, in the tunnel.
        [Theory]
        [InlineData(Key.A)]
        [InlineData(Key.D5)]
        [InlineData(Key.OemComma)]
        [InlineData(Key.OemPeriod)]
        public void ACharacterKeyOnAnEditableBoxIsNotHandledInTheTunnelNorInTheBubbleSoItProducesItsTextInput(Key key)
        {
            StaTestRunner.Run(() =>
            {
                var (_, pilot, box) = Editable();

                Assert.False(Preview(box, key).Handled, "PreviewKeyDown " + key);
                Assert.False(Bubble(box, key).Handled, "KeyDown " + key);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            });
        }

        // The editor itself consumes some keys in its own class handler (Space and Delete among them), and that is not the panel's business: the
        // editable box must answer every key exactly as a plain TextBox does, so nothing of the panel marks a key that the editor did not.
        [Theory]
        [InlineData(Key.Space)]
        [InlineData(Key.Delete)]
        [InlineData(Key.Back)]
        [InlineData(Key.Left)]
        [InlineData(Key.Tab)]
        [InlineData(Key.Z)]
        [InlineData(Key.A)]
        [InlineData(Key.D5)]
        [InlineData(Key.OemComma)]
        [InlineData(Key.OemPeriod)]
        public void AnEditableBoxAnswersEveryKeyExactlyAsAPlainTextBoxDoesInTheTunnelAndInTheBubble(Key key)
        {
            StaTestRunner.Run(() =>
            {
                var (_, _, box) = Editable();
                var plain = new TextBox();

                Assert.Equal(Preview(plain, key).Handled, Preview(box, key).Handled);
                Assert.Equal(Bubble(plain, key).Handled, Bubble(box, key).Handled);
            });
        }

        [Fact]
        public void TheBoxHasNoHandlerOfTheBubbleKeyDownAttachedToIt()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, box) = Editable();

                Assert.Equal(0, InstanceHandlersOf(box, Keyboard.KeyDownEvent));
                Assert.Equal(0, InstanceHandlersOf(box, Keyboard.KeyUpEvent));
                Assert.True(InstanceHandlersOf(box, Keyboard.PreviewKeyDownEvent) > 0);
                Assert.Equal(0, InstanceHandlersOf(Named<StackPanel>(view, "BrowserFieldPanel"), Keyboard.KeyDownEvent));
            });
        }

        // The handlers that the XAML (or any code) attached to this very element for a routed event; the class handlers of TextBox are not counted.
        private static int InstanceHandlersOf(UIElement element, RoutedEvent routed)
        {
            var property = typeof(UIElement).GetProperty("EventHandlersStore", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(property);
            var store = property!.GetValue(element);
            if (store == null) return 0;
            var method = store.GetType().GetMethod("GetRoutedEventHandlers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            Assert.NotNull(method);
            var handlers = (RoutedEventHandlerInfo[]?)method!.Invoke(store, new object[] { routed });
            return handlers?.Length ?? 0;
        }

        // ------------------------------------------------------------------ the binding and the setter

        [Fact]
        public void A_SettingTheTextOfTheBoxWithoutLosingFocusCallsSetPendingTextWithTheExactText()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, box) = Editable();

                box.Text = " 1:7 ";

                var typed = Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
                Assert.Equal(" 1:7 ", typed.Text);
                Assert.Equal("R-1", typed.RackId);
                Assert.Equal(new SessionId(7), typed.Origin);
                Assert.Equal(" 1:7 ", view.ViewModel.Browser.Field.Text);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            });
        }

        [Fact]
        public void B_ShowingAndPresentingWithAPendingTextNeverCallSetPendingText()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                pilot.Set(new SessionId(7), "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:75", isProtected: true));
                view.ViewModel.Refresh();
                view.ViewModel.Browser.Field.Show(new SessionId(7), "R-1", false);
                view.ViewModel.Refresh();
                Lay(view);

                Assert.Equal("1:75", Named<TextBox>(view, "BrowserFieldBox").Text);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            });
        }

        [Fact]
        public void C_ARejectedTextRestoresTheTextOfTheBox()
        {
            StaTestRunner.Run(() =>
            {
                var (_, pilot, box) = Editable();
                pilot.SetPendingResponder = (_, _, _) => PilotFieldResult.Rejected("La petición no es de este documento.");

                box.Text = "zz";

                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
                Assert.Equal("1:50", box.Text);
            });
        }

        [Fact]
        public void ATextTypedInAReadOnlyBoxStateNeverReachesTheDraft()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");

                box.Text = "typed";

                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
                Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, box.Text);
            });
        }

        // ------------------------------------------------------------------ the buttons and the states

        [Fact]
        public void TheReloadButtonOfTheErrorStateShowsTheDraftAgainWithoutAnyRead()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var session = new SessionId(7);
                view.ViewModel.Browser.PinTab("R-1");
                pilot.Set(session, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Error, canEdit: false, isProtected: true, committedReason: "No se pudo leer el estado."));
                view.ViewModel.Refresh();
                Lay(view);
                var reload = Named<Button>(view, "BrowserFieldReloadButton");
                var box = Named<TextBox>(view, "BrowserFieldBox");

                Assert.Equal(Visibility.Visible, reload.Visibility);
                Assert.True(box.IsReadOnly);
                Assert.Contains("No se pudo leer el estado.", Named<TextBlock>(view, "BrowserFieldStateText").Text);

                pilot.Set(session, "R-1", WorkspacePilotEditPortDouble.View());
                Click(reload);
                Lay(view);

                Assert.Empty(pilot.Reads());
                Assert.False(box.IsReadOnly);
                Assert.Equal(Visibility.Collapsed, reload.Visibility);
                Assert.False(view.ViewModel.HasError);
            });
        }

        [Fact]
        public void AFailureInsideTheSetterReachesTheViewAsTheErrorOfTheFieldAndNotAsAnErrorOfThePanel()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, box) = Editable();
                pilot.Throws[WorkspacePilotEditPortDouble.SetPendingOp] = new InvalidOperationException("falló el puerto");

                box.Text = "1:75";
                Lay(view);

                Assert.False(view.ViewModel.HasError);
                Assert.Equal("1:50", box.Text);
                Assert.True(box.IsReadOnly);
                Assert.Contains("falló el puerto", Named<TextBlock>(view, "BrowserFieldStateText").Text);
                Assert.Equal(Visibility.Visible, Named<Button>(view, "BrowserFieldReloadButton").Visibility);
            });
        }

        [Fact]
        public void TheNotAdmittedDiagnosticAndTheHintReachTheViewByBinding()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                pilot.BeginResponder = _ => EditIntentOutcome.NotAdmitted(
                    "No editable aquí: el rack tiene una definición propia.", "detalle", "Usa RACKEDITAR para editarlo.", EditWitnessStatus.NotSupplied);

                Click(Named<Button>(view, "BrowserFieldEditButton"));
                Lay(view);

                Assert.Equal("No editable aquí: el rack tiene una definición propia.", Named<TextBlock>(view, "BrowserFieldNotEditableText").Text);
                Assert.Equal("Usa RACKEDITAR para editarlo.", Named<TextBlock>(view, "BrowserFieldHintText").Text);
                Assert.Equal("No editable aquí", Named<TextBlock>(view, "BrowserFieldStateText").Text);
                Assert.True(Named<TextBox>(view, "BrowserFieldBox").IsReadOnly);
            });
        }

        // ------------------------------------------------------------------ no fixed sizes

        [Fact]
        public void D_NoNewControlOfTheFieldHasAFixedWidthHeightMinimumOrMaximum()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();
                var names = new[]
                {
                    "BrowserFieldPanel", "BrowserFieldBox", "BrowserFieldStateText", "BrowserFieldMessageText", "BrowserFieldNoticeText",
                    "BrowserFieldNotEditableText", "BrowserFieldHintText", "BrowserFieldEditButton", "BrowserFieldReloadButton",
                };

                foreach (var name in names)
                {
                    var element = Named<FrameworkElement>(view, name);
                    foreach (var property in new[]
                             {
                                 FrameworkElement.WidthProperty, FrameworkElement.MinWidthProperty, FrameworkElement.MaxWidthProperty,
                                 FrameworkElement.HeightProperty, FrameworkElement.MinHeightProperty, FrameworkElement.MaxHeightProperty,
                             })
                        Assert.True(element.ReadLocalValue(property) == DependencyProperty.UnsetValue, name + " sets " + property.Name);
                    Assert.True(double.IsNaN(element.Width) && double.IsNaN(element.Height), name + " has a fixed size");
                }
            });
        }

        [Fact]
        public void ThereIsOneBoxAndItIsInsideTheOneSurface()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();
                var surface = Named<Border>(view, "BrowserSurface");

                // The box of the field is one and is inside the surface; the view says nothing about how many other boxes it may have.
                Assert.Single(Descendants<TextBox>(view), b => b.Name == "BrowserFieldBox");
                Assert.True(Named<TextBox>(view, "BrowserFieldBox").IsDescendantOf(surface));
                Assert.Equal(Visibility.Visible, surface.Visibility);
            });
        }
    }
}
