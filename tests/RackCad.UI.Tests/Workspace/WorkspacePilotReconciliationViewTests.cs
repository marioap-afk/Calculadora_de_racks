#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4-T4A2-PREPARE-UI (INV-F4T4A2-02, 05, 06): the reconciliation of the field in the real view, with real buttons on the STA thread of
    /// the suite and the double of the edit port. A click is raised as the input system raises it, so what is proved is the wiring of the XAML
    /// and of the code-behind to the view-model: the content, the automation name and the visibility of each button, the order of Yes and No,
    /// and what a real click on each of them asks of the port. Nothing reads a drawing and nothing is written.
    /// </summary>
    public class WorkspacePilotReconciliationViewTests
    {
        private static readonly SessionId Session = new(7);
        private const string Rack = "R-1";

        private static PilotFieldView Conflict(bool isProtected, string? pending = null, string? committed = "2.5") =>
            WorkspacePilotEditPortDouble.View(
                PilotFieldState.Conflict, committed, pending, null, false, isProtected, true, null, null, PilotUnknownOrigin.None, "A1:EnvelopeText", null);

        // The tab R-1 is pinned and its draft is the given one.
        private static (WorkspacePanelView View, WorkspacePilotEditPortDouble Pilot) Pinned(PilotFieldView draft)
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One(Rack)) };
            var pilot = new WorkspacePilotEditPortDouble();
            var view = new WorkspacePanelView(
                new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), new WorkspaceBrowserSummaryPortDouble(), pilot));
            view.ViewModel.Refresh();
            pilot.Set(Session, Rack, draft);
            view.ViewModel.Browser.PinTab(Rack);
            Lay(view);
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

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static string AutomationNameOf(Button button) => AutomationProperties.GetName(button);

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

        // ------------------------------------------------------------------ the buttons: content, automation name and visibility

        [Fact]
        public void TheFiveButtonsHaveTheirContentTheirAutomationNameAndTheOrderOfTheTable()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _) = Pinned(Conflict(true));

                var check = Named<Button>(view, "BrowserFieldCheckButton");
                var discard = Named<Button>(view, "BrowserFieldDiscardAndReloadButton");
                var keep = Named<Button>(view, "BrowserFieldKeepMineButton");
                var yes = Named<Button>(view, "BrowserFieldChoiceYesButton");
                var no = Named<Button>(view, "BrowserFieldChoiceNoButton");

                Assert.Equal("Comprobar ahora", check.Content);
                Assert.Equal("Comprobar el borrador ahora", AutomationNameOf(check));
                Assert.Equal("Descartar y recargar", discard.Content);
                Assert.Equal("Descartar el borrador y recargarlo", AutomationNameOf(discard));
                Assert.Equal("Conservar mi borrador", keep.Content);
                Assert.Equal("Conservar el borrador sobre la versión actual", AutomationNameOf(keep));
                Assert.Equal("Sí", yes.Content);
                Assert.Equal("Confirmar la elección", AutomationNameOf(yes));
                Assert.Equal("No", no.Content);
                Assert.Equal("Cancelar la elección", AutomationNameOf(no));
                Assert.All(new[] { check, discard, keep, yes, no }, b => Assert.False(b.IsDefault || b.IsCancel));
            });
        }

        [Fact]
        public void TheButtonsAreVisibleOnlyWhereTheViewModelOffersThem()
        {
            StaTestRunner.Run(() =>
            {
                var (conflict, _) = Pinned(Conflict(true));
                Assert.Equal(Visibility.Visible, Named<Button>(conflict, "BrowserFieldCheckButton").Visibility);
                Assert.Equal(Visibility.Visible, Named<Button>(conflict, "BrowserFieldDiscardAndReloadButton").Visibility);
                Assert.Equal(Visibility.Visible, Named<Button>(conflict, "BrowserFieldKeepMineButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Border>(conflict, "BrowserFieldChoicePanel").Visibility);

                var (dirty, _) = Pinned(WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, "2.5", isProtected: true));
                Assert.Equal(Visibility.Visible, Named<Button>(dirty, "BrowserFieldCheckButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(dirty, "BrowserFieldDiscardAndReloadButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(dirty, "BrowserFieldKeepMineButton").Visibility);

                var (pending, _) = Pinned(Conflict(true, pending: "2x"));
                Assert.Equal(Visibility.Visible, Named<Button>(pending, "BrowserFieldDiscardAndReloadButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(pending, "BrowserFieldKeepMineButton").Visibility);

                var (none, _) = Pinned(WorkspacePilotEditPortDouble.NoDraft());
                Assert.Equal(Visibility.Collapsed, Named<Button>(none, "BrowserFieldCheckButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(none, "BrowserFieldDiscardAndReloadButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(none, "BrowserFieldKeepMineButton").Visibility);
            });
        }

        [Fact]
        public void TheChoicePanelHoldsYesAndNoInThatOrderAndNothingElseThatActs()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _) = Pinned(Conflict(true));
                var panel = Named<Border>(view, "BrowserFieldChoicePanel");

                var buttons = Descendants<Button>(panel).ToList();

                Assert.Equal(2, buttons.Count);
                Assert.Equal("Sí", buttons[0].Content);
                Assert.Equal("No", buttons[1].Content);
                Assert.Same(Named<Button>(view, "BrowserFieldChoiceYesButton"), buttons[0]);
                Assert.Same(Named<Button>(view, "BrowserFieldChoiceNoButton"), buttons[1]);
                Assert.Equal(Visibility.Visible, buttons[0].Visibility);
                Assert.Equal(Visibility.Visible, buttons[1].Visibility);
            });
        }

        // ------------------------------------------------------------------ real clicks

        [Fact]
        public void ARealClickOnTheCheckButtonCallsTheGeneralCheckOnceAndShowsItsText()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Pinned(WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, "2.5", stale: true, isProtected: true));
                pilot.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed();

                Click(Named<Button>(view, "BrowserFieldCheckButton"));
                Lay(view);

                var call = Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.ConfirmBaseOp));
                Assert.Equal(Session, call.Origin);
                Assert.Equal(Rack, call.RackId);
                Assert.Equal(0, pilot.Count(WorkspacePilotEditPortDouble.CheckOp));
                Assert.Equal("Sin cambios: el dibujo coincide con la base del borrador.", Named<TextBlock>(view, "BrowserFieldNoticeText").Text);
            });
        }

        [Fact]
        public void ARealClickOnAnOptionOnlyAsksForTheConfirmationAndShowsTheQuestionAndTheTwoButtons()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Pinned(Conflict(true, committed: "2.5"));
                var box = Named<TextBox>(view, "BrowserFieldBox");
                Assert.False(box.IsReadOnly);

                Click(Named<Button>(view, "BrowserFieldKeepMineButton"));
                Lay(view);

                Assert.Equal(
                    "¿Conservar tu escala de anotación (2.5) sobre la versión actual del dibujo? El resto del rack se toma del dibujo y tu escala sustituye a la del dibujo.",
                    Named<TextBlock>(view, "BrowserFieldChoiceText").Text);
                Assert.Equal(Visibility.Visible, Named<Border>(view, "BrowserFieldChoicePanel").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(view, "BrowserFieldCheckButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(view, "BrowserFieldDiscardAndReloadButton").Visibility);
                Assert.Equal(Visibility.Collapsed, Named<Button>(view, "BrowserFieldKeepMineButton").Visibility);
                Assert.True(box.IsReadOnly);
                Assert.DoesNotContain(pilot.Calls, c => c.Operation != WorkspacePilotEditPortDouble.GetFieldOp);
            });
        }

        [Fact]
        public void ARealClickOnYesCallsTheChosenOperationOnceAndNeverTheOtherForEachOption()
        {
            StaTestRunner.Run(() =>
            {
                var (keep, keepPilot) = Pinned(Conflict(true));
                keepPilot.KeepMineResponder = _ =>
                {
                    keepPilot.Set(Session, Rack, WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, "2.5", isProtected: true));
                    return EditIntentOutcome.Kept(EditWitnessStatus.NotSupplied);
                };
                Click(Named<Button>(keep, "BrowserFieldKeepMineButton"));
                Click(Named<Button>(keep, "BrowserFieldChoiceYesButton"));
                Lay(keep);

                var kept = Assert.Single(keepPilot.Of(WorkspacePilotEditPortDouble.KeepMineOp));
                Assert.Equal(Session, kept.Origin);
                Assert.Equal(Rack, kept.RackId);
                Assert.Equal(0, keepPilot.Count(WorkspacePilotEditPortDouble.DiscardAndReloadOp));
                Assert.Equal("Borrador conservado sobre la versión actual del dibujo.", Named<TextBlock>(keep, "BrowserFieldNoticeText").Text);
                Assert.Equal(Visibility.Collapsed, Named<Border>(keep, "BrowserFieldChoicePanel").Visibility);

                var (discard, discardPilot) = Pinned(Conflict(true));
                discardPilot.DiscardAndReloadResponder = _ =>
                {
                    discardPilot.Set(Session, Rack, WorkspacePilotEditPortDouble.View(PilotFieldState.Clean, "2.5"));
                    return EditIntentOutcome.Reloaded(EditWitnessStatus.NotSupplied);
                };
                Click(Named<Button>(discard, "BrowserFieldDiscardAndReloadButton"));
                Click(Named<Button>(discard, "BrowserFieldChoiceYesButton"));
                Lay(discard);

                var reloaded = Assert.Single(discardPilot.Of(WorkspacePilotEditPortDouble.DiscardAndReloadOp));
                Assert.Equal(Session, reloaded.Origin);
                Assert.Equal(Rack, reloaded.RackId);
                Assert.Equal(0, discardPilot.Count(WorkspacePilotEditPortDouble.KeepMineOp));
                Assert.Equal("Borrador recargado desde el dibujo.", Named<TextBlock>(discard, "BrowserFieldNoticeText").Text);
            });
        }

        [Fact]
        public void ARealClickOnNoCallsNothingAndLeavesTheDraftAsItWasForEachOption()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var keep in new[] { false, true })
                {
                    var (view, pilot) = Pinned(Conflict(true));
                    Click(Named<Button>(view, keep ? "BrowserFieldKeepMineButton" : "BrowserFieldDiscardAndReloadButton"));
                    var before = pilot.Current(Session, Rack);
                    pilot.Calls.Clear();

                    Click(Named<Button>(view, "BrowserFieldChoiceNoButton"));
                    Lay(view);

                    Assert.Empty(pilot.Calls);
                    Assert.Equal(before, pilot.Current(Session, Rack));
                    Assert.Equal("Elección cancelada: el borrador no cambió.", Named<TextBlock>(view, "BrowserFieldNoticeText").Text);
                    Assert.Equal(Visibility.Collapsed, Named<Border>(view, "BrowserFieldChoicePanel").Visibility);
                    Assert.Equal(Visibility.Visible, Named<Button>(view, "BrowserFieldDiscardAndReloadButton").Visibility);
                    Assert.False(Named<TextBox>(view, "BrowserFieldBox").IsReadOnly);
                }
            });
        }

        [Fact]
        public void YesAndNoAreNotSwappedAClickOnEachDoesWhatItsLabelSays()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Pinned(Conflict(true));
                pilot.KeepMineResponder = _ => EditIntentOutcome.Kept(EditWitnessStatus.NotSupplied);

                Click(Named<Button>(view, "BrowserFieldKeepMineButton"));
                Click(Named<Button>(view, "BrowserFieldChoiceNoButton"));
                Assert.Equal(0, pilot.Count(WorkspacePilotEditPortDouble.KeepMineOp));

                Click(Named<Button>(view, "BrowserFieldKeepMineButton"));
                Click(Named<Button>(view, "BrowserFieldChoiceYesButton"));
                Assert.Equal(1, pilot.Count(WorkspacePilotEditPortDouble.KeepMineOp));
            });
        }

        [Fact]
        public void TheReconcileTextIsShownForAConflictAndLeavingTheBoxDoesNotCommitWhileTheChoiceWaits()
        {
            StaTestRunner.Run(() =>
            {
                var (view, pilot) = Pinned(Conflict(false, pending: "2x"));
                Assert.Equal("Qué cambió: A1:EnvelopeText Con texto pendiente no se puede conservar el borrador: aplícalo o corrígelo antes.", Named<TextBlock>(view, "BrowserFieldReconcileText").Text);

                Click(Named<Button>(view, "BrowserFieldDiscardAndReloadButton"));
                view.ViewModel.Browser.Field.CommitPending();
                Lay(view);

                Assert.Equal(0, pilot.Count(WorkspacePilotEditPortDouble.CommitOp));
                Assert.Equal("2x", pilot.Current(Session, Rack).PendingText);
                Assert.Equal("Qué cambió: A1:EnvelopeText", Named<TextBlock>(view, "BrowserFieldReconcileText").Text);
            });
        }
    }
}
