#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F2-T2B-NAVIGATION-UI (INV-F2T2B-02, 06, 09, 10, 13): the navigation controls of the root panel. Each click is one
    /// explicit interaction that reaches the navigation view-model and, through it, the port; a disabled action reaches nothing;
    /// a failure never escapes the control; and no navigation control has a fixed width, a minimum or a maximum: they reflow
    /// from a narrow bar to a wide surface.
    /// </summary>
    public class WorkspaceNavigationViewTests
    {
        private static readonly string[] ActionButtons =
        {
            "NavigationSelectButton", "NavigationViewButton", "NavigationCenterButton", "NavigationLocateButton",
        };

        private static readonly string[] NavigationControls =
        {
            "NavigationSelectButton", "NavigationViewButton", "NavigationCenterButton", "NavigationNextCopyButton",
            "NavigationCopyText", "NavigationViewSelector", "NavigationLocateCenteredCheck", "NavigationLocateButton",
            "NavigationBlockedText", "NavigationResultText", "NavigationActionsPanel", "NavigationCopyPanel", "NavigationLocatePanel",
        };

        private static (WorkspacePanelView View, WorkspacePortDouble Host, WorkspaceNavigationPortDouble Port) Build(WorkspaceHostSnapshot? snapshot = null)
        {
            var host = new WorkspacePortDouble { Snapshot = snapshot ?? WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var port = new WorkspaceNavigationPortDouble();
            var view = new WorkspacePanelView(new WorkspacePanelViewModel(host, _ => { }, port));
            view.ViewModel.Refresh();
            Lay(view, 400);
            return (view, host, port);
        }

        private static T Named<T>(WorkspacePanelView view, string name)
            where T : class
        {
            var found = view.FindName(name);
            Assert.NotNull(found);
            return Assert.IsAssignableFrom<T>(found);
        }

        private static void Click(WorkspacePanelView view, string name) =>
            Named<ButtonBase>(view, name).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static void Lay(WorkspacePanelView view, double width)
        {
            view.Measure(new Size(width, 1200));
            view.Arrange(new Rect(0, 0, width, 1200));
            view.UpdateLayout();
        }

        private static Rect Bounds(FrameworkElement element, FrameworkElement root) =>
            element.TransformToAncestor(root).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

        private static int Rows(WorkspacePanelView view, string panelName)
        {
            var panel = Named<WrapPanel>(view, panelName);
            return panel.Children.OfType<FrameworkElement>()
                .Where(c => c.Visibility == Visibility.Visible)
                .Select(c => Math.Round(System.Windows.Controls.Primitives.LayoutInformation.GetLayoutSlot(c).Top))
                .Distinct()
                .Count();
        }

        // ------------------------------------------------------------------ the clicks reach the view-model and the port

        [Theory]
        [InlineData("NavigationSelectButton", NavigationActionKind.Select)]
        [InlineData("NavigationViewButton", NavigationActionKind.View)]
        [InlineData("NavigationCenterButton", NavigationActionKind.Center)]
        [InlineData("NavigationLocateButton", NavigationActionKind.LocateView)]
        public void EachActionButtonSendsOneRequestWithTheSessionAndTheRackOfThePresentedSnapshot(string button, NavigationActionKind expected)
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, port) = Build();
                var snapshotCalls = host.SnapshotCalls;

                Click(view, button);

                var call = Assert.Single(port.Calls);
                Assert.Equal(expected, call.Action);
                Assert.Equal(new SessionId(7), call.Origin.Origin);
                Assert.Equal("R-1", call.RackId);
                Assert.Equal(snapshotCalls, host.SnapshotCalls);
            });
        }

        [Fact]
        public void TheActionButtonsAreEnabledOnlyForExactlyOneRack()
        {
            StaTestRunner.Run(() =>
            {
                var (one, _, _) = Build();
                var (none, _, _) = Build(WorkspacePanelFixtures.Ready(session: 7));
                var (many, _, _) = Build(WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.Selection(2, 1)));
                var (blocked, _, _) = Build(WorkspacePanelFixtures.WithBlock(WorkspaceActionBlock.CommandActive));

                foreach (var name in ActionButtons)
                {
                    Assert.True(Named<Button>(one, name).IsEnabled, name);
                    Assert.False(Named<Button>(none, name).IsEnabled, name);
                    Assert.False(Named<Button>(many, name).IsEnabled, name);
                    Assert.False(Named<Button>(blocked, name).IsEnabled, name);
                }
            });
        }

        [Fact]
        public void ADisabledActionExplainsWhyOnTheViewAndAClickOnItReachesNoPort()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, port) = Build(WorkspacePanelFixtures.Ready(session: 7));
                Lay(view, 400);

                foreach (var name in ActionButtons) Click(view, name);

                Assert.Empty(port.Calls);
                var reason = Named<TextBlock>(view, "NavigationBlockedText");
                Assert.Equal(Visibility.Visible, reason.Visibility);
                Assert.Equal(view.ViewModel.Navigation.BlockedReason, reason.Text);
                Assert.False(string.IsNullOrWhiteSpace(reason.Text));
                Assert.Equal(1, host.SnapshotCalls);
            });
        }

        [Fact]
        public void WhenTheActionsAreEnabledTheReasonDisappears()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();
                Lay(view, 400);

                var reason = Named<TextBlock>(view, "NavigationBlockedText");
                Assert.Equal(string.Empty, reason.Text);
                Assert.NotEqual(Visibility.Visible, reason.Visibility);
            });
        }

        [Fact]
        public void TheNextCopyButtonIsOfferedOnlyOnceMIsKnownAndMovesTheCopyThatTheNextClickSends()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, port) = Build();
                var next = Named<Button>(view, "NavigationNextCopyButton");
                var copy = Named<TextBlock>(view, "NavigationCopyText");
                Assert.False(next.IsEnabled);
                Assert.Equal("Copia 1 de ?", copy.Text);

                Click(view, "NavigationViewButton");
                Assert.True(next.IsEnabled);
                Assert.Equal("Copia 1 de 3", copy.Text);

                Click(view, "NavigationNextCopyButton");
                Assert.Equal("Copia 2 de 3", copy.Text);
                Click(view, "NavigationViewButton");

                Assert.Equal(new[] { 0, 1 }, port.Calls.Select(c => c.CopyIndex).ToArray());
            });
        }

        [Fact]
        public void TheViewSelectorOffersTheFourViewsAndLocateUsesTheOneChosen()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, port) = Build();
                var selector = Named<ComboBox>(view, "NavigationViewSelector");
                Assert.Equal(4, selector.Items.Count);
                Assert.Equal(NavigationViewOrder.Frontal, selector.SelectedValue);

                selector.SelectedValue = NavigationViewOrder.Lateral;
                Assert.Equal(NavigationViewOrder.Lateral, view.ViewModel.Navigation.SelectedView);
                Click(view, "NavigationLocateButton");

                var call = Assert.Single(port.Calls);
                Assert.Equal(NavigationActionKind.LocateView, call.Action);
                Assert.Equal(NavigationViewOrder.Lateral, call.View);
                Assert.Equal(NavigationActionKind.View, call.LocateWith);
            });
        }

        [Fact]
        public void TheCenteringChoiceOfLocateReachesThePort()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, port) = Build();
                var center = Named<CheckBox>(view, "NavigationLocateCenteredCheck");
                Assert.NotEqual(true, center.IsChecked);

                center.IsChecked = true;
                Click(view, "NavigationLocateButton");

                Assert.Equal(NavigationActionKind.Center, Assert.Single(port.Calls).LocateWith);
            });
        }

        [Fact]
        public void TheAnswerOfThePortIsShownAsText()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, port) = Build();
                port.Result = WorkspaceNavigationResult.Unavailable("Navigation needs the Model Space.");
                Lay(view, 400);

                Click(view, "NavigationViewButton");

                var result = Named<TextBlock>(view, "NavigationResultText");
                Assert.Contains("Espacio Modelo", result.Text, StringComparison.Ordinal);
                Assert.Equal(Visibility.Visible, result.Visibility);
            });
        }

        [Fact]
        public void AFailureOfThePortNeverEscapesTheControlAndIsShownInTheNavigationSection()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, port) = Build();
                port.ThrowSynchronously = new InvalidOperationException("boom-navegación");

                var thrown = Record.Exception(() => Click(view, "NavigationCenterButton"));

                Assert.Null(thrown);
                Assert.Contains("boom-navegación", Named<TextBlock>(view, "NavigationResultText").Text, StringComparison.Ordinal);
                Assert.NotEqual(WorkspacePanelState.Error, view.ViewModel.State);
                Assert.True(Named<Button>(view, "NavigationCenterButton").IsEnabled);
                Assert.Equal(1, host.SnapshotCalls);
            });
        }

        [Fact]
        public void AnAnswerThatRequiresARefreshCostsExactlyOneQueryOfTheSnapshot()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, port) = Build();
                port.Result = WorkspaceNavigationResult.Stale("A target of the rack is gone.", true, NavigationActionVerdict.RejectedTargetGone);
                var before = host.SnapshotCalls;

                Click(view, "NavigationViewButton");

                Assert.Equal(before + 1, host.SnapshotCalls);
                Assert.Equal(0, host.AuthorizeCalls);
                Assert.Equal(0, host.SelectionCalls);
            });
        }

        // ------------------------------------------------------------------ INV-F2T2B-13: the view does nothing by itself

        [Fact]
        public void ConstructingShowingAndWaitingNeverAsksThePortForNavigation()
        {
            WorkspacePanelView? view = null;
            var port = new WorkspaceNavigationPortDouble();
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            StaTestRunner.Run(() =>
            {
                view = new WorkspacePanelView(new WorkspacePanelViewModel(host, _ => { }, port));
                view.ViewModel.Refresh();
                Lay(view, 300);
                Lay(view, 900);
            });
            Assert.NotNull(view);

            Thread.Sleep(300);
            StaTestRunner.Run(() => { });

            Assert.Empty(port.Calls);
        }

        // ------------------------------------------------------------------ INV-F2T2B-09: no fixed widths, they reflow

        [Fact]
        public void NoNavigationControlHasAFixedWidthAMinimumOrAMaximum()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();

                foreach (var name in NavigationControls)
                {
                    var element = Named<FrameworkElement>(view, name);
                    foreach (var property in new[] { FrameworkElement.WidthProperty, FrameworkElement.MinWidthProperty, FrameworkElement.MaxWidthProperty })
                        Assert.True(element.ReadLocalValue(property) == DependencyProperty.UnsetValue, name + " sets " + property.Name);
                    Assert.True(double.IsNaN(element.Width), name + " has a fixed Width");
                    Assert.True(double.IsPositiveInfinity(element.MaxWidth), name + " has a MaxWidth");
                }

                foreach (var name in NavigationControls.Where(n => n != "NavigationViewSelector"))
                    Assert.Equal(0, Named<FrameworkElement>(view, name).MinWidth);
            });
        }

        [Fact]
        public void TheNavigationControlsLiveInWrapPanelsThatReflowWithTheWidthOfTheSurface()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();

                foreach (var name in new[] { "NavigationActionsPanel", "NavigationCopyPanel", "NavigationLocatePanel" })
                    Assert.IsType<WrapPanel>(Named<FrameworkElement>(view, name));
                foreach (var name in ActionButtons.Concat(new[] { "NavigationNextCopyButton", "NavigationViewSelector", "NavigationLocateCenteredCheck" }))
                    Assert.IsType<WrapPanel>(Named<FrameworkElement>(view, name).Parent);
            });
        }

        [Fact]
        public void TheControlsFitTheNarrowBarAndTheWideSurfaceAndWrapWhenTheSpaceIsShort()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();

                foreach (var width in new[] { 240d, 320d, 600d, 1400d })
                {
                    Lay(view, width);
                    foreach (var name in NavigationControls.Where(n => n.EndsWith("Panel", StringComparison.Ordinal) == false))
                    {
                        var element = Named<FrameworkElement>(view, name);
                        if (element.Visibility != Visibility.Visible) continue;
                        Assert.True(Bounds(element, view).Right <= width + 0.5, name + " overflows at " + width.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                }

                // Wide: every WrapPanel is one row. Very short: the actions break into one row per button.
                Lay(view, 1400);
                Assert.Equal(1, Rows(view, "NavigationActionsPanel"));
                Assert.Equal(1, Rows(view, "NavigationCopyPanel"));
                Assert.Equal(1, Rows(view, "NavigationLocatePanel"));
                var wideHeight = Named<WrapPanel>(view, "NavigationActionsPanel").ActualHeight;

                Lay(view, 100);
                Assert.True(Rows(view, "NavigationActionsPanel") >= 3);
                Assert.True(Named<WrapPanel>(view, "NavigationActionsPanel").ActualHeight > wideHeight);
            });
        }

        [Fact]
        public void TheNavigationIsPartOfTheOneRootControlWithNoWindowAndNoDialog()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _) = Build();

                Assert.Equal("Workspace", System.Windows.Automation.AutomationProperties.GetName(view));
                foreach (var name in NavigationControls)
                    Assert.True(Named<FrameworkElement>(view, name).IsDescendantOf(view), name);
            });
        }
    }
}
