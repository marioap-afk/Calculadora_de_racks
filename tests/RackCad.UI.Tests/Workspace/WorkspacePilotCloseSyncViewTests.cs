#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
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
    /// I-64 F4-T3B2-EDIT-CLOSE-SYNC (INV-F4T3B2-03, 12, 13): the inline confirmation, the rows of notice, the discard buttons and the keyboard
    /// repetition (N-3) in the real view, on the STA thread of the suite and with the double of the edit port. The clicks are raised as the
    /// input system raises them; the keys are raised in the tunnel with the repetition flag that the system sets. Nothing reads a drawing and
    /// nothing is written. The keyboard, the focus and the modal of AutoCAD are for Smoke-2.
    /// </summary>
    public class WorkspacePilotCloseSyncViewTests
    {
        private static readonly SessionId A = new(7);
        private static readonly SessionId B = new(8);

        private static (WorkspacePanelView View, WorkspacePilotEditPortDouble Pilot, WorkspaceNavigationPortDouble Navigation) Build(
            bool withPilot = true, Action<WorkspacePilotEditPortDouble>? prepare = null)
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var navigation = new WorkspaceNavigationPortDouble();
            var pilot = new WorkspacePilotEditPortDouble();
            prepare?.Invoke(pilot);
            var view = new WorkspacePanelView(
                new WorkspacePanelViewModel(host, _ => { }, navigation, new WorkspaceBrowserSummaryPortDouble(), withPilot ? pilot : null));
            view.ViewModel.Refresh();
            Lay(view);
            return (view, pilot, navigation);
        }

        // The tab R-1 is pinned, its draft was opened and holds pending text, so it is protected.
        private static (WorkspacePanelView View, WorkspacePilotEditPortDouble Pilot) Protected(Action<WorkspacePilotEditPortDouble>? prepare = null)
        {
            var (view, pilot, _) = Build(prepare: prepare);
            view.ViewModel.Browser.PinTab("R-1");
            view.ViewModel.Browser.BeginFieldEdit();
            view.ViewModel.Browser.Field.Text = "1:75";
            Lay(view);
            Assert.Equal("Nombre R-1 *", view.ViewModel.Browser.Tabs.FirstOrDefault()?.Title);
            pilot.Calls.Clear();
            return (view, pilot);
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

        private static Button? ByAutomation(DependencyObject root, string name) =>
            Descendants<Button>(root).FirstOrDefault(b => AutomationProperties.GetName(b) == name);

        private static Button TabButton(WorkspacePanelView view, string rack, string automationName)
        {
            var found = Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip"))
                .FirstOrDefault(b => b.DataContext is WorkspaceBrowserTabItem item && item.RackId == rack && AutomationProperties.GetName(b) == automationName);
            Assert.NotNull(found);
            return found!;
        }

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static Rect Bounds(FrameworkElement element, UIElement root) =>
            element.TransformToAncestor(root).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

        private static KeyEventArgs Preview(UIElement element, Key key, bool repeat = false)
        {
            using var source = new HwndSource(0, 0, 0, 0, 0, "t", IntPtr.Zero);
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
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

        // ------------------------------------------------------------------ the elements, their names and their availability

        [Fact]
        public void TheNewElementsExistWithTheirAutomationNamesAndAreHiddenUntilTheyHaveSomethingToShow()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();

                Assert.Equal(Visibility.Visible, Named<Button>(view, "BrowserDiscardDocumentButton").Visibility);
                Assert.Equal("Descartar borradores de este documento", Named<Button>(view, "BrowserDiscardDocumentButton").Content);
                Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Equal("Descartar", Named<Button>(view, "BrowserConfirmDiscardButton").Content);
                Assert.Equal("Confirmar el descarte", AutomationProperties.GetName(Named<Button>(view, "BrowserConfirmDiscardButton")));
                Assert.Equal("Cancelar", Named<Button>(view, "BrowserCancelDiscardButton").Content);
                Assert.Equal("Cancelar el descarte", AutomationProperties.GetName(Named<Button>(view, "BrowserCancelDiscardButton")));
                Assert.Equal(string.Empty, Named<TextBlock>(view, "BrowserNoticeText").Text);
                Assert.Empty(Named<ItemsControl>(view, "BrowserNoticeList").Items);
                var discard = Named<Button>(view, "BrowserFieldDiscardButton");
                Assert.Equal("Descartar", discard.Content);
                Assert.Equal("Descartar el borrador", AutomationProperties.GetName(discard));
                Assert.Equal(Visibility.Collapsed, discard.Visibility);
                Assert.True(discard.IsDescendantOf(Named<Border>(view, "BrowserSurface")));
            });
        }

        [Fact]
        public void WithoutTheEditPortNothingOfTheTaskIsOfferedAndNoCallReachesAnyPort()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build(withPilot: false);
                view.ViewModel.Browser.PinTab("R-1");
                Lay(view);

                Assert.Equal(Visibility.Collapsed, Named<Button>(view, "BrowserDiscardDocumentButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(view, "BrowserFieldDiscardButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserConfirmationPanel").Visibility);

                Click(TabButton(view, "R-1", "Cerrar pestaña"));
                Lay(view);

                Assert.Empty(view.ViewModel.Browser.Tabs);
                Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Empty(pilot.Calls);
            });
        }

        // ------------------------------------------------------------------ 12, 13, 14: the inline confirmation

        [Fact]
        public void T12_ClosingAProtectedTabShowsTheInlineConfirmationWithExactlyDescartarAndCancelarAndDoesNotClose()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Protected();

                Click(TabButton(view, "R-1", "Cerrar pestaña"));
                Lay(view);

                var panel = Named<Border>(view, "BrowserConfirmationPanel");
                Assert.Equal(Visibility.Visible, panel.Visibility);
                Assert.Contains("Nombre R-1", Named<TextBlock>(view, "BrowserConfirmationText").Text);
                var buttons = Descendants<Button>(panel).ToList();
                Assert.Equal(2, buttons.Count);
                Assert.Equal(new[] { "Descartar", "Cancelar" }, buttons.Select(b => (string?)b.Content).ToArray());
                Assert.All(buttons, b =>
                {
                    Assert.False(b.IsDefault);
                    Assert.False(b.IsCancel);
                });
                Assert.Empty(Descendants<Window>(view));
                Assert.Null(Window.GetWindow(view));
                Assert.Single(view.ViewModel.Browser.Tabs);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            });
        }

        [Fact]
        public void T13_CancelarRemovesTheConfirmationKeepsTheTabAndCallsNothing()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Protected();
                Click(TabButton(view, "R-1", "Cerrar pestaña"));
                Lay(view);
                pilot.Calls.Clear();

                Click(Named<Button>(view, "BrowserCancelDiscardButton"));
                Lay(view);

                Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Empty(pilot.Calls);
                Assert.Equal("Nombre R-1 *", view.ViewModel.Browser.Tabs.FirstOrDefault()?.Title);
                Assert.Equal("1:75", Named<TextBox>(view, "BrowserFieldBox").Text);
                Assert.Equal("1:75", pilot.Current(A, "R-1").PendingText);
            });
        }

        [Fact]
        public void T14_DescartarCallsDiscardRackWithThatSessionAndRackAndClosesTheTab()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Protected(p => p.DiscardAsTheHostDoes());
                Click(TabButton(view, "R-1", "Cerrar pestaña"));
                Lay(view);
                pilot.Calls.Clear();

                Click(Named<Button>(view, "BrowserConfirmDiscardButton"));
                Lay(view);

                var discard = Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
                Assert.Equal(A, discard.Origin);
                Assert.Equal("R-1", discard.RackId);
                Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Empty(view.ViewModel.Browser.Tabs);
                Assert.Empty(Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")));
                Assert.False(pilot.Current(A, "R-1").Exists);
                Assert.Empty(pilot.Reads());
                Assert.False(view.ViewModel.HasError);
            });
        }

        [Fact]
        public void TheRefusalOfTheDiscardShowsItsNoticeOnTheViewAndTheTabStays()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Protected();
                Click(TabButton(view, "R-1", "Cerrar pestaña"));
                Lay(view);

                Click(Named<Button>(view, "BrowserConfirmDiscardButton"));
                Lay(view);

                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
                Assert.Contains("No se cerró la pestaña", Named<TextBlock>(view, "BrowserNoticeText").Text);
                Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Single(view.ViewModel.Browser.Tabs);
            });
        }

        [Fact]
        public void ACleanTabClosesWithNoConfirmationAndAFailingPortNeverEscapesTheClick()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                view.ViewModel.Browser.PinTab("R-1");
                view.ViewModel.Browser.BeginFieldEdit();
                pilot.Throws[WorkspacePilotEditPortDouble.DiscardRackOp] = new InvalidOperationException("se cayó el puerto");
                Lay(view);

                var thrown = Record.Exception(() => Click(TabButton(view, "R-1", "Cerrar pestaña")));
                Lay(view);

                Assert.Null(thrown);
                Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Empty(view.ViewModel.Browser.Tabs);
                Assert.Contains("se cayó el puerto", Named<TextBlock>(view, "BrowserNoticeText").Text);
                Assert.False(view.ViewModel.HasError);
            });
        }

        // ------------------------------------------------------------------ the discard buttons of the document and of the field

        [Fact]
        public void TheDiscardDocumentButtonWithNoRacksAtRiskShowsTheNoticeAndWithSomeAsksInline()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                view.ViewModel.Browser.PinTab("R-1");
                Lay(view);

                Click(Named<Button>(view, "BrowserDiscardDocumentButton"));
                Lay(view);

                Assert.Equal("No hay borradores en riesgo en este documento; no se descartó nada.", Named<TextBlock>(view, "BrowserNoticeText").Text);
                Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));

                pilot.RisksResponder = _ => new PilotRiskView(new[] { "R-1" }, new[] { "R-4" });
                Click(Named<Button>(view, "BrowserDiscardDocumentButton"));
                Lay(view);

                Assert.Equal(Visibility.Visible, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Contains("R-4", Named<TextBlock>(view, "BrowserConfirmationText").Text);
                Assert.Equal(string.Empty, Named<TextBlock>(view, "BrowserNoticeText").Text);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            });
        }

        [Fact]
        public void TheDiscardButtonOfTheFieldInErrorAsksInlineAndDescartarDiscardsTheDraftAndTheTabStays()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                view.ViewModel.Browser.PinTab("R-1");
                pilot.DiscardAsTheHostDoes();
                pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Error, pending: "1:75", isProtected: true, canEdit: false, committedReason: "No se pudo leer el estado."));
                view.ViewModel.Refresh();
                Lay(view);
                var discard = Named<Button>(view, "BrowserFieldDiscardButton");
                Assert.Equal(Visibility.Visible, discard.Visibility);
                Assert.Equal(Visibility.Visible, Named<Button>(view, "BrowserFieldReloadButton").Visibility);
                pilot.Calls.Clear();

                Click(discard);
                Lay(view);

                Assert.Equal(Visibility.Visible, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));

                Click(Named<Button>(view, "BrowserConfirmDiscardButton"));
                Lay(view);

                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
                Assert.Equal(Visibility.Collapsed, discard.Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(view, "BrowserFieldReloadButton").Visibility);
                Assert.Equal("Sin borrador.", Named<TextBlock>(view, "BrowserFieldStateText").Text);
                Assert.Single(view.ViewModel.Browser.Tabs);
                Assert.Empty(pilot.Reads());
            });
        }

        // ------------------------------------------------------------------ the rows of notice

        [Fact]
        public void TheRowOfAVetoOfThePresentedSessionHasItsDiscardButtonAndUnderstoodRemovesTheRow()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build(prepare: p => p.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false)));
                view.ViewModel.Browser.PinTab("R-1");
                view.ViewModel.Refresh();
                Lay(view);
                var list = Named<ItemsControl>(view, "BrowserNoticeList");
                Assert.Single(list.Items);
                var discard = ByAutomation(list, "Descartar los borradores del documento del aviso");
                var understood = ByAutomation(list, "Quitar el aviso");
                Assert.NotNull(discard);
                Assert.NotNull(understood);
                Assert.Equal(Visibility.Visible, discard!.Visibility);
                Assert.Equal("Descartar borradores de este documento", discard.Content);
                Assert.Equal("Entendido", understood!.Content);
                Assert.Contains("No se cerró el documento", Descendants<TextBlock>(list).Select(t => t.Text).FirstOrDefault(t => t.StartsWith("No se cerró", StringComparison.Ordinal)));
                pilot.RisksResponder = _ => new PilotRiskView(new[] { "R-1" }, Array.Empty<string>());

                Click(discard);
                Lay(view);

                Assert.Equal(Visibility.Visible, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Equal(1, pilot.Count(WorkspacePilotEditPortDouble.RisksOp));

                Click(understood);
                Lay(view);

                Assert.Empty(list.Items);
                Assert.Empty(view.ViewModel.Browser.Notices);
            });
        }

        [Fact]
        public void TheRowOfAVetoOfAnotherSessionHasNoDiscardButtonAndTheRowOfALossIsRemovedOnlyByUnderstood()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build(prepare: p =>
                {
                    p.CloseQueue.Enqueue(new PilotCloseView(B, new[] { "R-9" }, false));
                    p.LossQueue.Enqueue(new PilotLossView(B, new[] { "R-7" }, new[] { "R-8" }, new Dictionary<string, string> { ["R-8"] = "falló" }));
                });
                view.ViewModel.Refresh();
                Lay(view);
                var list = Named<ItemsControl>(view, "BrowserNoticeList");

                Assert.Equal(2, list.Items.Count);
                var discards = Descendants<Button>(list).Where(b => AutomationProperties.GetName(b) == "Descartar los borradores del documento del aviso").ToList();
                Assert.Equal(2, discards.Count);
                Assert.All(discards, b => Assert.Equal(Visibility.Collapsed, b.Visibility));
                Assert.Contains(Descendants<TextBlock>(list), t => t.Text.StartsWith("Se perdieron borradores", StringComparison.Ordinal) && t.Text.Contains("R-8 (falló)"));

                view.ViewModel.Refresh();
                view.ViewModel.Refresh();
                Lay(view);
                Assert.Equal(2, list.Items.Count);

                // The strip of rows is rebuilt after each click, so the next button is looked for again.
                for (var i = 0; i < 2; i++)
                {
                    var understood = ByAutomation(list, "Quitar el aviso");
                    Assert.NotNull(understood);
                    Click(understood!);
                    Lay(view);
                }

                Assert.Empty(list.Items);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            });
        }

        // ------------------------------------------------------------------ layout: between the strip and the surface, outside the strip, no fixed sizes

        [Fact]
        public void TheNewElementsAreBetweenTheStripAndTheSurfaceOutsideTheStripAndHaveNoFixedSize()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Protected(p => p.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false)));
                view.ViewModel.Refresh();
                Click(TabButton(view, "R-1", "Cerrar pestaña"));
                Lay(view);

                var strip = Named<FrameworkElement>(view, "BrowserTabStrip");
                var surface = Named<FrameworkElement>(view, "BrowserSurface");
                var between = new[] { "BrowserDiscardDocumentButton", "BrowserConfirmationPanel", "BrowserNoticeList" };
                Assert.Equal(Visibility.Visible, Named<Border>(view, "BrowserConfirmationPanel").Visibility);
                Assert.Single(Named<ItemsControl>(view, "BrowserNoticeList").Items);

                foreach (var name in between)
                {
                    var element = Named<FrameworkElement>(view, name);
                    Assert.False(element.IsDescendantOf(strip), name + " is inside the strip");
                    Assert.True(Bounds(element, view).Top >= Bounds(strip, view).Bottom - 0.5, name + " is not below the strip");
                    Assert.True(Bounds(element, view).Bottom <= Bounds(surface, view).Top + 0.5, name + " is not above the surface");
                }

                foreach (var name in new[]
                         {
                             "BrowserDiscardDocumentButton", "BrowserConfirmationPanel", "BrowserConfirmationText", "BrowserConfirmDiscardButton",
                             "BrowserCancelDiscardButton", "BrowserNoticeText", "BrowserNoticeList", "BrowserFieldDiscardButton",
                         })
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

                // The buttons of the strip are still the three of every tab: the new ones are not counted there.
                Assert.DoesNotContain(Descendants<Button>(strip), b => b.Name.StartsWith("Browser", StringComparison.Ordinal));
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            });
        }

        // ------------------------------------------------------------------ N-3: the repetition of the key that launched the first gesture

        [Fact]
        public void N3_TheRepetitionOfTheKeyThatLaunchedTheGestureIsHandledOnTheEditableBoxAndTheRepetitionOfAnotherKeyIsNot()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");
                Assert.True(box.IsReadOnly);

                var first = Preview(box, Key.D5);

                Assert.True(first.Handled);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
                Assert.False(box.IsReadOnly);

                Assert.True(Preview(box, Key.D5, repeat: true).Handled, "the repetition of the key that launched the gesture");
                Assert.True(Preview(box, Key.D5, repeat: true).Handled, "and the next repetition");
                Assert.False(Preview(box, Key.D6, repeat: true).Handled, "the repetition of another key");
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            });
        }

        [Fact]
        public void N3_TheFirstPressThatIsNotARepetitionForgetsTheKeyAndItFollowsTheRuleOfF4T3B1()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");
                Preview(box, Key.D5);
                Assert.True(Preview(box, Key.D5, repeat: true).Handled);

                // Another key pressed for the first time (a character key reaches the editor untouched) ends the memory.
                Assert.False(Preview(box, Key.A).Handled);
                Assert.False(Preview(box, Key.D5, repeat: true).Handled);
            });
        }

        [Fact]
        public void N3_AGestureThatDidNotComeFromTheKeyLeavesNoMemoryAndTheKeyIsNotHandledWhenItRepeats()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                view.ViewModel.Browser.PinTab("R-1");
                view.ViewModel.Browser.BeginFieldEdit();
                Lay(view);
                var box = Named<TextBox>(view, "BrowserFieldBox");
                Assert.False(box.IsReadOnly);
                pilot.Calls.Clear();

                Assert.False(Preview(box, Key.D5, repeat: true).Handled);
                Assert.False(Preview(box, Key.D5).Handled);
            });
        }

        [Fact]
        public void N3_EnterAndEscKeepTheirRuleAndTheRepetitionOfEnterThatLaunchedTheGestureDoesNotCommit()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");

                Assert.True(Preview(box, Key.Enter).Handled);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
                Assert.False(box.IsReadOnly);
                box.Text = "1:75";
                pilot.Calls.Clear();

                // The held Enter that launched the gesture keeps repeating: it is marked and it does not commit.
                Assert.True(Preview(box, Key.Enter, repeat: true).Handled);
                Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));

                // A new press of Enter commits as in F4-T3B1, and Esc is handled with nothing else.
                Assert.True(Preview(box, Key.Enter).Handled);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
                Assert.True(Preview(box, Key.Escape).Handled);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            });
        }

        [Fact]
        public void N3_AReadOnlyBoxStillHandlesTheRepetitionsAndLaunchesOnlyOnce()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot, _) = Build();
                var box = Named<TextBox>(view, "BrowserFieldBox");
                pilot.BeginResponder = _ => EditIntentOutcome.NotAdmitted("No editable aquí.", "detalle", "Usa RACKEDITAR.", EditWitnessStatus.NotSupplied);

                Assert.True(Preview(box, Key.D5).Handled);
                Assert.True(box.IsReadOnly);
                Assert.True(Preview(box, Key.D5, repeat: true).Handled);
                Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
            });
        }
    }
}
