#nullable enable
using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F1-T3B-HOST-UI (INV-F1-T3B-02, 10, 11): the root control of the panel is a UserControl labelled "Workspace"
    /// whose view-model is its data context; it merges Themes/AppStyles.xaml in its own resources and resolves them with
    /// no App.xaml and no application resources; and its controls reach the view-model only through explicit user
    /// interactions.
    /// </summary>
    public class WorkspacePanelViewTests
    {
        private static WorkspacePanelView Build(WorkspacePortDouble port, Action<bool>? toggle = null) =>
            new(new WorkspacePanelViewModel(port, toggle ?? (_ => { })));

        [Fact]
        public void TheRootIsAUserControlLabelledWorkspaceWhoseDataContextIsTheViewModel()
        {
            StaTestRunner.Run(() =>
            {
                var port = new WorkspacePortDouble();
                var view = Build(port);

                Assert.IsAssignableFrom<UserControl>(view);
                Assert.NotNull(view.Content);
                Assert.Same(view.ViewModel, view.DataContext);
                Assert.Equal("Workspace", AutomationProperties.GetName(view));
                Assert.Equal("Workspace", view.ViewModel.Label);
            });
        }

        [Fact]
        public void TheRootMergesAppStylesInItsOwnResourcesAndResolvesThemWithoutApplicationResources()
        {
            StaTestRunner.Run(() =>
            {
                var view = Build(new WorkspacePortDouble());

                var merged = view.Resources.MergedDictionaries
                    .Concat(view.Resources.MergedDictionaries.SelectMany(d => d.MergedDictionaries))
                    .Where(d => d.Source != null)
                    .Select(d => d.Source.OriginalString)
                    .ToList();

                Assert.Contains("/RackCad.UI;component/Themes/AppStyles.xaml", merged);
                Assert.NotNull(view.TryFindResource("SectionTitle"));
                Assert.NotNull(view.TryFindResource("WindowBackgroundBrush"));
                Assert.False(System.Windows.Application.Current.Resources.Contains("SectionTitle"));
                Assert.False(System.Windows.Application.Current.Resources.Contains("WindowBackgroundBrush"));
            });
        }

        [Fact]
        public void TheViewShowsTheLabelAndTheStateOfTheViewModel()
        {
            StaTestRunner.Run(() =>
            {
                var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 11, selection: SelectionContext.One("RACK-9")) };
                var view = Build(port);
                view.ViewModel.Refresh();
                view.Measure(new Size(400, 600));
                view.Arrange(new Rect(0, 0, 400, 600));
                view.UpdateLayout();

                var texts = AllTextBlocks(view).Select(t => t.Text).ToList();

                Assert.Contains("Workspace", texts);
                Assert.Contains(texts, t => t.Contains("RACK-9", StringComparison.Ordinal));
                Assert.Contains(texts, t => t.Contains("11", StringComparison.Ordinal));
            });
        }

        [Fact]
        public void TheRetryButtonAsksForTheSnapshotAgainAndRecoversFromError()
        {
            StaTestRunner.Run(() =>
            {
                var port = new WorkspacePortDouble { SnapshotFault = new InvalidOperationException("boom") };
                var view = Build(port);
                view.ViewModel.Refresh();
                Assert.Equal(WorkspacePanelState.Error, view.ViewModel.State);

                var retry = Named<Button>(view, "RetryButton");
                port.SnapshotFault = null;
                port.Snapshot = WorkspacePanelFixtures.Ready(session: 2);
                retry.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                Assert.Equal(WorkspacePanelState.Ready, view.ViewModel.State);
                Assert.Equal(2, port.SnapshotCalls);
                Assert.Equal(0, port.AuthorizeCalls);
                Assert.Equal(0, port.SelectionCalls);
            });
        }

        [Fact]
        public void TheRetryButtonIsOnlyVisibleInTheErrorState()
        {
            StaTestRunner.Run(() =>
            {
                var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
                var view = Build(port);
                view.ViewModel.Refresh();
                view.UpdateLayout();
                var retry = Named<Button>(view, "RetryButton");
                Assert.NotEqual(Visibility.Visible, retry.Visibility);

                port.SnapshotFault = new InvalidOperationException("boom");
                view.ViewModel.Refresh();
                view.UpdateLayout();
                Assert.Equal(Visibility.Visible, retry.Visibility);
            });
        }

        [Fact]
        public void TheInstrumentationToggleSendsTheSwitchToTheInjectedDelegate()
        {
            StaTestRunner.Run(() =>
            {
                var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
                var received = new System.Collections.Generic.List<bool>();
                var view = Build(port, received.Add);
                view.ViewModel.Refresh();

                var toggle = Named<CheckBox>(view, "InstrumentationToggle");
                toggle.IsChecked = true;
                toggle.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                Assert.Equal(new[] { true }, received.ToArray());
            });
        }

        [Fact]
        public void AFailingToggleDelegateDoesNotEscapeTheControlAndShowsTheError()
        {
            StaTestRunner.Run(() =>
            {
                var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
                var view = Build(port, _ => throw new InvalidOperationException("boom-toggle"));
                view.ViewModel.Refresh();

                var toggle = Named<CheckBox>(view, "InstrumentationToggle");
                toggle.IsChecked = true;
                var thrown = Record.Exception(() => toggle.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));

                Assert.Null(thrown);
                Assert.Equal(WorkspacePanelState.Error, view.ViewModel.State);
            });
        }

        [Fact]
        public void TheViewRejectsAMissingViewModel()
        {
            StaTestRunner.Run(() => Assert.Throws<ArgumentNullException>(() => new WorkspacePanelView(null!)));
        }

        private static T Named<T>(WorkspacePanelView view, string name)
            where T : class
        {
            var found = view.FindName(name);
            Assert.NotNull(found);
            return Assert.IsType<T>(found);
        }

        private static System.Collections.Generic.IEnumerable<TextBlock> AllTextBlocks(DependencyObject root)
        {
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is TextBlock block) yield return block;
                foreach (var nested in AllTextBlocks(child)) yield return nested;
            }
        }
    }
}
