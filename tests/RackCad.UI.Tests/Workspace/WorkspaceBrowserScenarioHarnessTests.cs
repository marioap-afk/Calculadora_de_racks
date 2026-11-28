#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;
using Xunit.Abstractions;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F3-T2-BROWSER-UI (INV-F3T2-17, 18, D-18): the automated scenario harnesses of the browser, with no AutoCAD. Scenario 4
    /// (a change of rack or of context), scenario 5 (a change of tab) and scenario 6 (ten light tabs) over the simulated ports, and
    /// the cost characterization, which only MEASURES and writes through the test output: it asserts no time.
    /// </summary>
    public class WorkspaceBrowserScenarioHarnessTests
    {
        private readonly ITestOutputHelper _output;

        public WorkspaceBrowserScenarioHarnessTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static (WorkspacePanelViewModel Panel, WorkspacePortDouble Host, WorkspaceNavigationPortDouble Navigation, WorkspaceBrowserSummaryPortDouble Summary) Rig(
            SelectionContext? context = null)
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: context ?? SelectionContext.None) };
            var navigation = new WorkspaceNavigationPortDouble();
            var summary = new WorkspaceBrowserSummaryPortDouble();
            return (new WorkspacePanelViewModel(host, _ => { }, navigation, summary), host, navigation, summary);
        }

        private static void Show(WorkspacePortDouble host, WorkspacePanelViewModel panel, SelectionContext context)
        {
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: context);
            panel.Refresh();
        }

        private static string[] Racks(WorkspacePanelViewModel panel) => panel.Browser.Tabs.Select(t => t.RackId).ToArray();

        private static string Ms(Stopwatch watch) => watch.Elapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture);

        // ------------------------------------------------------------------ scenario 4: a change of rack or of context

        [Fact]
        public void Scenario4AChangeOfRackOrOfContextOnlyMovesTheTabsThroughTheOneSynchronizationAndReadsTheSummaryOncePerMaterialization()
        {
            var (panel, host, navigation, summary) = Rig();
            var steps = new (SelectionContext Context, string[] Tabs, string? Active, int Reads)[]
            {
                (SelectionContext.None, Array.Empty<string>(), null, 0),
                (SelectionContext.One("R-1"), new[] { "R-1" }, "R-1", 1),
                (SelectionContext.None, new[] { "R-1" }, "R-1", 1),
                (SelectionContext.Selection(2, 1), new[] { "R-1" }, "R-1", 1),
                (SelectionContext.NoIdentity, new[] { "R-1" }, "R-1", 1),
                (SelectionContext.Diagnostic, new[] { "R-1" }, "R-1", 1),
                (SelectionContext.One("R-1"), new[] { "R-1" }, "R-1", 1),
                (SelectionContext.One("R-2"), new[] { "R-2" }, "R-2", 2),
                (SelectionContext.One("R-2"), new[] { "R-2" }, "R-2", 2),
                (SelectionContext.One("R-3"), new[] { "R-3" }, "R-3", 3),
            };

            foreach (var step in steps)
            {
                Show(host, panel, step.Context);

                Assert.Equal(step.Tabs, Racks(panel));
                Assert.Equal(step.Active, panel.Browser.ActiveRackId);
                Assert.Equal(step.Reads, summary.Calls.Count);
                Assert.Equal(step.Active == null ? 0 : 1, panel.Browser.LiveSurfaceCount);
                Assert.Empty(navigation.Calls);
                _output.WriteLine("SCENARIO-4 " + step.Context.Kind + " " + step.Context.RackId + " tabs=" + string.Join(",", step.Tabs) + " reads=" + step.Reads);
            }

            Assert.Equal(new[] { "R-1", "R-2", "R-3" }, summary.Calls.Select(c => c.RackId).ToArray());
        }

        [Fact]
        public void Scenario4ThePinnedRackSurvivesAChangeOfRackAndTheNavigationFollowsTheActiveTab()
        {
            var (panel, host, navigation, _) = Rig();
            Show(host, panel, SelectionContext.One("R-1"));
            panel.Browser.PinTab("R-1");

            Show(host, panel, SelectionContext.One("R-2"));
            Show(host, panel, SelectionContext.None);

            Assert.Equal(new[] { "R-1", "R-2" }, Racks(panel));
            Assert.Equal("R-2", panel.Browser.ActiveRackId);
            Assert.True(panel.Navigation.CanNavigate);
            _ = panel.Navigation.SelectAsync();
            Assert.Equal("R-2", Assert.Single(navigation.Calls).RackId);
        }

        // ------------------------------------------------------------------ scenario 5: a change of tab

        [Fact]
        public void Scenario5AChangeOfTabMaterializesTheSurfaceOnceAndNeverCallsTheNavigationOrAsksTheHostAgain()
        {
            var (panel, host, navigation, summary) = Rig();
            foreach (var rack in new[] { "R-1", "R-2", "R-3" })
            {
                Show(host, panel, SelectionContext.One(rack));
                panel.Browser.PinTab(rack);
            }

            var snapshots = host.SnapshotCalls;
            var reads = summary.Calls.Count;

            foreach (var (rack, expectedReads) in new[] { ("R-1", 1), ("R-2", 2), ("R-1", 3), ("R-3", 4) })
            {
                panel.Browser.ActivateTab(rack);

                Assert.Equal(rack, panel.Browser.ActiveRackId);
                Assert.Equal(reads + expectedReads, summary.Calls.Count);
                Assert.Equal(rack, summary.Calls.Last().RackId);
                Assert.Equal("Nombre " + rack, panel.Browser.SurfaceTitle);
                Assert.Empty(navigation.Calls);
                Assert.Equal(snapshots, host.SnapshotCalls);
                _output.WriteLine("SCENARIO-5 active=" + rack + " reads=" + (summary.Calls.Count - reads));
            }

            // A repeated snapshot with the same context does not undo the manual change.
            Show(host, panel, SelectionContext.One("R-3"));
            panel.Browser.ActivateTab("R-1");
            Show(host, panel, SelectionContext.One("R-3"));
            Assert.Equal("R-1", panel.Browser.ActiveRackId);
            Assert.Empty(navigation.Calls);
        }

        // ------------------------------------------------------------------ scenario 6: ten light tabs

        [Fact]
        public void Scenario6TenLightTabsAreTenRecordsWithExactlyOneLiveSurfaceNoSubscriptionPerTabAndNoNavigationCalls()
        {
            StaTestRunner.Run(() =>
            {
                var (panel, host, navigation, summary) = Rig();
                var view = new WorkspacePanelView(panel);
                Lay(view, 400);
                var subscribersBefore = Subscribers(panel.Browser);

                for (var i = 1; i <= 10; i++)
                {
                    Show(host, panel, SelectionContext.One("R-" + i));
                    panel.Browser.PinTab("R-" + i);
                    Assert.True(panel.Browser.LiveSurfaceCount <= BrowserSession.MaxLiveSurfaces);
                }

                Lay(view, 400);
                Assert.Equal(10, panel.Browser.Tabs.Count);
                Assert.Equal(10, panel.Browser.Tabs.Select(t => t.RackId).Distinct(StringComparer.Ordinal).Count());
                Assert.Equal(1, panel.Browser.LiveSurfaceCount);
                Assert.Single(Surfaces(view));
                Assert.Equal(1, panel.Browser.Tabs.Count(t => t.IsActive));

                // Zero subscriptions per tab: the view-model has the same listeners with ten tabs as it had with none, and a tab is a value.
                Assert.Equal(subscribersBefore, Subscribers(panel.Browser));
                Assert.All(panel.Browser.Tabs, tab =>
                {
                    Assert.False(typeof(INotifyPropertyChanged).IsAssignableFrom(tab.GetType()));
                    Assert.Empty(tab.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
                });

                // No object of AutoCAD, of WPF or of a delegate is retained per tab.
                var retained = typeof(WorkspaceBrowserTabItem).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(f => f.FieldType).ToList();
                Assert.All(retained, t => Assert.True(t == typeof(string) || t == typeof(bool), "a tab retains " + t));

                // Walking through every tab: no call to the navigation, and the surface count never exceeds K.
                var surfaceBefore = Surfaces(view).Single();
                var snapshots = host.SnapshotCalls;
                var reads = summary.Calls.Count;
                foreach (var rack in Enumerable.Range(1, 10).Select(n => "R-" + n))
                {
                    panel.Browser.ActivateTab(rack);
                    Lay(view, 400);

                    Assert.Equal(rack, panel.Browser.ActiveRackId);
                    Assert.Equal(1, panel.Browser.LiveSurfaceCount);
                    Assert.Same(surfaceBefore, Surfaces(view).Single());
                    Assert.Empty(navigation.Calls);
                }

                Assert.Equal(snapshots, host.SnapshotCalls);
                Assert.True(summary.Calls.Count - reads <= 10);
                Assert.True(summary.Calls.Skip(reads).All(c => c.Origin.Origin == new SessionId(7)));
                _output.WriteLine("SCENARIO-6 tabs=10 surfaces=1 subscribers=" + subscribersBefore + " navigation-calls=0 reads-while-walking=" + (summary.Calls.Count - reads));
            });
        }

        private static void Lay(WorkspacePanelView view, double width)
        {
            view.Measure(new Size(width, 6000));
            view.Arrange(new Rect(0, 0, width, 6000));
            view.UpdateLayout();
        }

        private static System.Collections.Generic.List<Border> Surfaces(DependencyObject root)
        {
            var found = new System.Collections.Generic.List<Border>();
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is Border border && border.Name == "BrowserSurface") found.Add(border);
                found.AddRange(Surfaces(child));
            }

            return found;
        }

        private static int Subscribers(WorkspaceBrowserViewModel browser)
        {
            var field = typeof(ObservableObject).GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            var handler = field!.GetValue(browser) as Delegate;
            return handler == null ? 0 : handler.GetInvocationList().Length;
        }

        // ------------------------------------------------------------------ INV-F3T2-18: the cost, measured and never asserted

        [Fact]
        public void CostCharacterizationMeasuresThePreviewTheActivationTheSwitchOfTenTabsAndTheSurfaceWithoutAssertingAnyTime()
        {
            var (panel, host, navigation, summary) = Rig();
            var watch = new Stopwatch();

            // 1. Create the preview, then replace it (a one-rack context while the previous tab is only a preview).
            watch.Restart();
            Show(host, panel, SelectionContext.One("R-1"));
            watch.Stop();
            _output.WriteLine("SCENARIO-COST create-preview ms=" + Ms(watch) + " tabs=" + panel.Browser.Tabs.Count);
            Assert.Equal(new[] { "R-1" }, Racks(panel));

            watch.Restart();
            Show(host, panel, SelectionContext.One("R-2"));
            watch.Stop();
            _output.WriteLine("SCENARIO-COST replace-preview ms=" + Ms(watch) + " tabs=" + panel.Browser.Tabs.Count);
            Assert.Equal(new[] { "R-2" }, Racks(panel));

            // 2. Activate an existing tab.
            panel.Browser.PinTab("R-2");
            Show(host, panel, SelectionContext.One("R-1"));
            panel.Browser.PinTab("R-1");
            watch.Restart();
            panel.Browser.ActivateTab("R-2");
            watch.Stop();
            _output.WriteLine("SCENARIO-COST activate-existing-tab ms=" + Ms(watch));
            Assert.Equal("R-2", panel.Browser.ActiveRackId);

            // 3. Switch among ten tabs: one full round.
            for (var i = 3; i <= 10; i++)
            {
                Show(host, panel, SelectionContext.One("R-" + i));
                panel.Browser.PinTab("R-" + i);
            }

            Assert.Equal(10, panel.Browser.Tabs.Count);
            var order = panel.Browser.Tabs.Select(t => t.RackId).ToArray();
            watch.Restart();
            foreach (var rack in order) panel.Browser.ActivateTab(rack);
            watch.Stop();
            _output.WriteLine("SCENARIO-COST switch-10-tabs total-ms=" + Ms(watch) + " switches=" + order.Length);

            // 4. Materialize the summary surface with K = 1: one presentation that makes a tab the live surface.
            var reads = summary.Calls.Count;
            watch.Restart();
            Show(host, panel, SelectionContext.One("R-11"));
            watch.Stop();
            _output.WriteLine("SCENARIO-COST materialize-surface-k1 ms=" + Ms(watch) + " reads=" + (summary.Calls.Count - reads) + " surfaces=" + panel.Browser.LiveSurfaceCount);

            Assert.Equal(1, panel.Browser.LiveSurfaceCount);
            Assert.Equal(1, summary.Calls.Count - reads);
            Assert.Empty(navigation.Calls);
        }
    }
}
