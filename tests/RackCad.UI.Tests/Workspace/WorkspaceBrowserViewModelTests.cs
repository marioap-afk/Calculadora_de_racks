#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F3-T2-BROWSER-UI: the browser view-model and its wiring in the panel (INV-F3T2-01 to 09, 13, 14, 15, 16). It shows only the
    /// tabs of the session of the presented snapshot, synchronizes only with a one-rack context, asks the read-only port only for the
    /// tab that becomes the live surface, and follows the active tab in the navigation without touching its port.
    /// </summary>
    public class WorkspaceBrowserViewModelTests
    {
        private static readonly SessionId A = new(7);
        private static readonly SessionId B = new(8);

        private static WorkspaceBrowserViewModel Browser(WorkspaceBrowserSummaryPortDouble? port = null, Action? changed = null) =>
            new(port ?? new WorkspaceBrowserSummaryPortDouble(), changed);

        private static void Present(
            WorkspaceBrowserViewModel browser,
            SessionId? session,
            SelectionContext context,
            bool canRead = true,
            WorkspaceActionBlock block = WorkspaceActionBlock.None,
            string blockText = "") =>
            browser.Present(session, context, canRead, block, blockText);

        private static string[] Racks(WorkspaceBrowserViewModel browser) => browser.Tabs.Select(t => t.RackId).ToArray();

        private static (WorkspacePanelViewModel Panel, WorkspacePortDouble Host, WorkspaceNavigationPortDouble Navigation, WorkspaceBrowserSummaryPortDouble Summary) Panel(
            WorkspaceHostSnapshot? snapshot = null)
        {
            var host = new WorkspacePortDouble { Snapshot = snapshot ?? WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var navigation = new WorkspaceNavigationPortDouble();
            var summary = new WorkspaceBrowserSummaryPortDouble();
            return (new WorkspacePanelViewModel(host, _ => { }, navigation, summary), host, navigation, summary);
        }

        // ------------------------------------------------------------------ INV-F3T2-01: tabs belong to the session of the snapshot

        [Fact]
        public void TheTabsAreThoseOfTheSessionOfThePresentedSnapshotAndASwitchABAConservesTheTabsOfA()
        {
            var browser = Browser();

            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal(new[] { "R-1" }, Racks(browser));

            Present(browser, B, SelectionContext.One("R-9"));
            Assert.Equal(new[] { "R-9" }, Racks(browser));

            Present(browser, A, SelectionContext.None);
            Assert.Equal(new[] { "R-1" }, Racks(browser));
            Assert.Equal("R-1", browser.ActiveRackId);
        }

        [Fact]
        public void PinnedTabsOfASessionAreNeverMixedWithThoseOfAnother()
        {
            var browser = Browser();
            Present(browser, A, SelectionContext.One("R-1"));
            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            Present(browser, B, SelectionContext.One("R-1"));
            browser.PinTab("R-1");

            Assert.Equal(new[] { "R-1" }, Racks(browser));
            Present(browser, A, SelectionContext.None);
            Assert.Equal(new[] { "R-1", "R-2" }, Racks(browser));
            Present(browser, B, SelectionContext.None);
            Assert.Equal(new[] { "R-1" }, Racks(browser));
        }

        [Fact]
        public void WithoutASessionThereAreNoTabsAndThePreviousOnesComeBackWithTheirSession()
        {
            var browser = Browser();
            Present(browser, A, SelectionContext.One("R-1"));

            Present(browser, null, SelectionContext.None, canRead: false, block: WorkspaceActionBlock.NoDocument);

            Assert.Empty(browser.Tabs);
            Assert.Null(browser.ActiveRackId);
            Assert.Equal(0, browser.LiveSurfaceCount);
            Assert.False(browser.HasSurface);
            Assert.Contains("documento", browser.EmptyText, StringComparison.Ordinal);

            Present(browser, A, SelectionContext.None);
            Assert.Equal(new[] { "R-1" }, Racks(browser));
        }

        [Fact]
        public void ASessionThatIsNotAliveIsDestroyedAtTheNextPresentationAndItsTabsAreNeverShownAgain()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);
            Present(browser, A, SelectionContext.One("R-1"));
            Present(browser, B, SelectionContext.One("R-9"));

            port.Dead.Add(A);
            Present(browser, B, SelectionContext.None);
            Assert.Equal(new[] { "R-9" }, Racks(browser));

            // A is gone for good: not even a snapshot that names it again, or a port that revives it, brings its tabs back.
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Empty(browser.Tabs);
            Assert.Null(browser.ActiveRackId);
            port.Dead.Remove(A);
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Empty(browser.Tabs);
            Present(browser, B, SelectionContext.None);
            Assert.Equal(new[] { "R-9" }, Racks(browser));
        }

        // ------------------------------------------------------------------ INV-F3T2-02: the one automatic synchronization

        [Fact]
        public void OnlyAOneRackContextCreatesOrChangesTabs()
        {
            var browser = Browser();
            Present(browser, A, SelectionContext.One("R-1"));

            foreach (var context in new[]
                     {
                         SelectionContext.None, SelectionContext.Selection(2, 1), SelectionContext.NoIdentity,
                         SelectionContext.Diagnostic, SelectionContext.Selection(3, 0), SelectionContext.None,
                     })
            {
                Present(browser, A, context);
                Assert.Equal(new[] { "R-1" }, Racks(browser));
                Assert.Equal("R-1", browser.ActiveRackId);
            }

            Present(browser, A, SelectionContext.One("R-2"));
            Assert.Equal(new[] { "R-2" }, Racks(browser));
        }

        [Fact]
        public void RepeatingTheSameContextAfterAManualChangeDoesNotReturnTheActiveTabToTheSelectedRack()
        {
            var browser = Browser();
            Present(browser, A, SelectionContext.One("R-1"));
            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            Assert.Equal("R-2", browser.ActiveRackId);

            browser.ActivateTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            Present(browser, A, SelectionContext.One("R-2"));
            Assert.Equal("R-1", browser.ActiveRackId);

            // A real transition does synchronize again.
            Present(browser, A, SelectionContext.None);
            Present(browser, A, SelectionContext.One("R-2"));
            Assert.Equal("R-2", browser.ActiveRackId);
        }

        // ------------------------------------------------------------------ INV-F3T2-03: the navigation follows the active tab

        [Fact]
        public void WithAnActiveTabTheNavigationReceivesOneAndTheRackOfTheTabWithTheSessionAndAvailabilityOfTheSnapshot()
        {
            var (panel, host, navigation, _) = Panel();
            panel.Refresh();

            // The context becomes None: without the browser the navigation would be disabled; the active tab keeps it on its rack.
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.None);
            panel.Refresh();
            Assert.Equal("R-1", panel.Browser.ActiveRackId);
            Assert.True(panel.Navigation.CanNavigate);

            _ = panel.Navigation.SelectAsync();
            var call = Assert.Single(navigation.Calls);
            Assert.Equal(new SessionId(7), call.Origin.Origin);
            Assert.Equal("R-1", call.RackId);
        }

        [Fact]
        public void TheAvailabilityOfTheSnapshotStillGovernsTheNavigationOfTheActiveTab()
        {
            var (panel, host, navigation, _) = Panel();
            panel.Refresh();
            Assert.True(panel.Navigation.CanNavigate);

            host.Snapshot = new WorkspaceHostSnapshot(
                WorkspaceHostStatus.Ready,
                new SessionId(7),
                true,
                new WorkspaceActionAvailability(false, false, WorkspaceActionBlock.CommandActive),
                SelectionContext.One("R-1"),
                false,
                false);
            panel.Refresh();

            Assert.Equal("R-1", panel.Browser.ActiveRackId);
            Assert.False(panel.Navigation.CanNavigate);
            _ = panel.Navigation.SelectAsync();
            Assert.Empty(navigation.Calls);
            Assert.False(string.IsNullOrWhiteSpace(panel.Navigation.BlockedReason));
        }

        [Fact]
        public void ChangingPinningOrClosingATabPresentsTheNavigationAgainWithTheLastSnapshotAndAsksNothing()
        {
            var (panel, host, navigation, summary) = Panel();
            panel.Refresh();
            panel.Browser.PinTab("R-1");
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
            panel.Refresh();
            panel.Browser.PinTab("R-2");
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-3"));
            panel.Refresh();
            panel.Browser.PinTab("R-3");
            var snapshots = host.SnapshotCalls;
            var reports = host.ReportCalls;

            panel.Browser.ActivateTab("R-1");
            _ = panel.Navigation.ViewAsync();
            Assert.Equal("R-1", Assert.Single(navigation.Calls).RackId);

            panel.Browser.CloseTab("R-1");
            Assert.Equal("R-2", panel.Browser.ActiveRackId);
            _ = panel.Navigation.ViewAsync();
            Assert.Equal("R-2", navigation.Calls.Last().RackId);

            panel.Browser.PinTab("R-2");
            panel.Browser.ActivateTab("R-3");
            _ = panel.Navigation.ViewAsync();
            Assert.Equal("R-3", navigation.Calls.Last().RackId);

            // Only the explicit navigation clicks reached the navigation port, and the host was never asked again.
            Assert.Equal(3, navigation.Calls.Count);
            Assert.All(navigation.Calls, c => Assert.Equal(NavigationActionKind.View, c.Action));
            Assert.Equal(snapshots, host.SnapshotCalls);
            Assert.Equal(reports, host.ReportCalls);
            Assert.Equal(0, host.AuthorizeCalls);
            Assert.Equal(0, host.SelectionCalls);
            Assert.All(summary.Calls, c => Assert.Equal(new SessionId(7), c.Origin.Origin));
        }

        [Fact]
        public void ChangingTabDoesNotRepresentTheNavigationWhileThePanelShowsAnError()
        {
            var (panel, host, _, _) = Panel();
            panel.Refresh();
            panel.Browser.PinTab("R-1");
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
            panel.Refresh();
            host.SnapshotFault = new InvalidOperationException("boom");
            panel.Refresh();
            Assert.Equal(WorkspacePanelState.Error, panel.State);
            Assert.False(panel.Navigation.CanNavigate);

            panel.Browser.ActivateTab("R-1");

            Assert.False(panel.Navigation.CanNavigate);
            Assert.Equal(WorkspacePanelState.Error, panel.State);
        }

        [Fact]
        public void WithoutAnActiveTabTheNavigationFollowsTheContextOfTheSnapshotAsBefore()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var navigation = new WorkspaceNavigationPortDouble();
            var panel = new WorkspacePanelViewModel(host, _ => { }, navigation);

            panel.Refresh();

            Assert.Empty(panel.Browser.Tabs);
            Assert.Null(panel.Browser.ActiveRackId);
            Assert.True(panel.Navigation.CanNavigate);
            _ = panel.Navigation.SelectAsync();
            Assert.Equal("R-1", Assert.Single(navigation.Calls).RackId);

            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.None);
            panel.Refresh();
            Assert.False(panel.Navigation.CanNavigate);
        }

        // ------------------------------------------------------------------ INV-F3T2-04: the lazy summary

        [Fact]
        public void TheSummaryIsAskedOnlyForTheTabThatBecomesTheLiveSurfaceOncePerMaterializationAndNeverForTheOthers()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);

            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal(new[] { "R-1" }, port.Calls.Select(c => c.RackId).ToArray());

            // The same tab presented again, with other contexts or the same one, is not a new materialization.
            Present(browser, A, SelectionContext.One("R-1"));
            Present(browser, A, SelectionContext.None);
            Present(browser, A, SelectionContext.Selection(2, 0));
            Assert.Single(port.Calls);

            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            Assert.Equal(new[] { "R-1", "R-2" }, port.Calls.Select(c => c.RackId).ToArray());

            // Activating a tab that already has a summary materializes it again, and only that one.
            browser.ActivateTab("R-1");
            Assert.Equal(new[] { "R-1", "R-2", "R-1" }, port.Calls.Select(c => c.RackId).ToArray());
            Assert.All(port.Calls, c => Assert.Equal(A, c.Origin.Origin));
        }

        [Fact]
        public void TheSummaryIsOnlyAskedWhileTheDrawingCanBeReadAndNothingBlocksAndABlockedTabIsRetriedAtTheNextPresentation()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);

            Present(browser, A, SelectionContext.One("R-1"), canRead: false, block: WorkspaceActionBlock.Degraded, blockText: "La sesión está degradada y no se lee.");
            Assert.Empty(port.Received);
            Assert.Contains("no se leyó el dibujo", browser.SurfaceStatusText, StringComparison.Ordinal);
            Assert.Contains("degradada", browser.SurfaceStatusText, StringComparison.Ordinal);

            Present(browser, A, SelectionContext.One("R-1"), canRead: true, block: WorkspaceActionBlock.CommandActive, blockText: "Hay un comando en curso.");
            Assert.Empty(port.Received);
            Assert.Contains("comando en curso", browser.SurfaceStatusText, StringComparison.Ordinal);
            Present(browser, A, SelectionContext.One("R-1"), canRead: false, block: WorkspaceActionBlock.None);
            Assert.Empty(port.Received);

            // The tab is the same one: when the drawing can be read again it is asked exactly once.
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal(new[] { "R-1" }, port.Calls.Select(c => c.RackId).ToArray());
            Assert.Equal("Resumen cargado.", browser.SurfaceStatusText);
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Single(port.Calls);
        }

        [Fact]
        public void ABlockedTabChangedByHandIsNotReadAndTheNewLiveSurfaceWaitsForTheNextPresentation()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);
            Present(browser, A, SelectionContext.One("R-1"));
            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"), canRead: false, block: WorkspaceActionBlock.ModalActive);
            Assert.Single(port.Calls);

            browser.ActivateTab("R-1");

            Assert.Single(port.Calls);
            Assert.Equal("R-1", browser.ActiveRackId);
            Present(browser, A, SelectionContext.None);
            Assert.Equal(new[] { "R-1", "R-1" }, port.Calls.Select(c => c.RackId).ToArray());
        }

        [Fact]
        public void TheSummaryEntersTheTabOnlyThroughApplySummaryAndTheViewsLiveOnlyInTheSurface()
        {
            var port = new WorkspaceBrowserSummaryPortDouble
            {
                Responder = call => WorkspaceBrowserSummaryPortDouble.Ready("Rack " + call.RackId, "Selective", "frontal", "lateral", "planta"),
            };
            var browser = Browser(port);

            Present(browser, A, SelectionContext.One("R-1"), canRead: false, block: WorkspaceActionBlock.Degraded);
            var preview = Assert.Single(browser.Tabs);
            Assert.Equal("R-1", preview.Title);
            Assert.True(preview.IsPreview);
            Assert.True(preview.IsActive);
            Assert.Equal("R-1", browser.SurfaceTitle);
            Assert.Equal(string.Empty, browser.SurfaceKindText);
            Assert.Equal(string.Empty, browser.SurfaceViewsText);

            Present(browser, A, SelectionContext.One("R-1"));

            var tab = Assert.Single(browser.Tabs);
            Assert.Equal("Rack R-1", tab.Title);
            Assert.Equal("Rack R-1", browser.SurfaceTitle);
            Assert.Equal("Tipo: Selective", browser.SurfaceKindText);
            Assert.Equal("Vistas: frontal, lateral, planta", browser.SurfaceViewsText);
            Assert.Equal("Resumen cargado.", browser.SurfaceStatusText);

            // The views are not part of any tab value.
            Assert.DoesNotContain(typeof(WorkspaceBrowserTabItem).GetProperties(), p => p.Name.Contains("View", StringComparison.Ordinal));
            Assert.DoesNotContain(typeof(BrowserTabRecord).GetProperties(), p => p.Name.Contains("View", StringComparison.Ordinal));
        }

        [Fact]
        public void EachStateOfTheSummaryHasItsOwnSpanishTextWithTheReason()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);
            var texts = new Dictionary<string, string>();

            foreach (var (name, answer) in new (string, BrowserRackSummary)[]
                     {
                         ("ready", WorkspaceBrowserSummaryPortDouble.Ready("Uno", "Selective", "frontal")),
                         ("notfound", BrowserRackSummary.Without(BrowserSummaryStatus.NotFound, "x")),
                         ("rejected", BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "x")),
                         ("failed", BrowserRackSummary.Without(BrowserSummaryStatus.Failed, "disco-lleno")),
                         ("busy", BrowserRackSummary.Without(BrowserSummaryStatus.Busy, "x")),
                     })
            {
                port.Responder = _ => answer;
                var session = new SessionId(100 + texts.Count);
                Present(browser, session, SelectionContext.One("R-1"));
                texts[name] = browser.SurfaceStatusText;
            }

            Present(browser, new SessionId(200), SelectionContext.One("R-1"), canRead: false, block: WorkspaceActionBlock.ModalActive, blockText: "Hay un cuadro de diálogo de RackCad abierto.");
            texts["blocked"] = browser.SurfaceStatusText;

            Assert.Equal("Resumen cargado.", texts["ready"]);
            Assert.Contains("ya no está en el dibujo", texts["notfound"], StringComparison.Ordinal);
            Assert.Contains("otro documento", texts["rejected"], StringComparison.Ordinal);
            Assert.Equal("No se pudo leer el resumen: disco-lleno", texts["failed"]);
            Assert.Contains("comando en curso", texts["busy"], StringComparison.Ordinal);
            Assert.Contains("cuadro de diálogo", texts["blocked"], StringComparison.Ordinal);
            Assert.Equal(6, texts.Values.Distinct().Count());
        }

        [Theory]
        [InlineData(BrowserSummaryStatus.NotFound)]
        [InlineData(BrowserSummaryStatus.Rejected)]
        [InlineData(BrowserSummaryStatus.Failed)]
        public void ANonReadyAnswerLeavesTheTabWithoutNameAndKindAndTheSurfaceShowsNoSummary(BrowserSummaryStatus status)
        {
            var port = new WorkspaceBrowserSummaryPortDouble { Responder = _ => BrowserRackSummary.Without(status, "motivo") };
            var browser = Browser(port);

            Present(browser, A, SelectionContext.One("R-1"));

            Assert.Equal("R-1", Assert.Single(browser.Tabs).Title);
            Assert.Equal(string.Empty, browser.SurfaceKindText);
            Assert.Equal(string.Empty, browser.SurfaceViewsText);
            Assert.False(string.IsNullOrWhiteSpace(browser.SurfaceStatusText));
        }

        [Fact]
        public void ABusyAnswerIsRetriedAtTheNextPresentationButNotByATabChange()
        {
            var port = new WorkspaceBrowserSummaryPortDouble { Responder = _ => BrowserRackSummary.Without(BrowserSummaryStatus.Busy, "A command is in progress.") };
            var browser = Browser(port);
            Present(browser, A, SelectionContext.One("R-1"));
            browser.PinTab("R-1");
            Assert.Single(port.Calls);

            port.Responder = null;
            Present(browser, A, SelectionContext.One("R-1"));

            Assert.Equal(2, port.Calls.Count);
            Assert.Equal("Resumen cargado.", browser.SurfaceStatusText);
        }

        // ------------------------------------------------------------------ INV-F3T2-21 (T2-F2): a presentation is not a user action

        [Fact]
        public void APresentationAsksOnItsOwnAccountAndEveryExplicitInteractionAsksOnTheUsers()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);

            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal(new[] { false }, port.Calls.Select(c => c.UserRequested).ToArray());

            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            Assert.Equal(new[] { false, false }, port.Calls.Select(c => c.UserRequested).ToArray());

            browser.ActivateTab("R-1");
            Assert.Equal(("R-1", true), (port.Calls.Last().RackId, port.Calls.Last().UserRequested));

            browser.PinTab("R-2");
            browser.ActivateTab("R-2");
            browser.CloseTab("R-2");
            Assert.Equal(("R-1", true), (port.Calls.Last().RackId, port.Calls.Last().UserRequested));

            browser.ReloadSummary();
            Assert.Equal(("R-1", true), (port.Calls.Last().RackId, port.Calls.Last().UserRequested));

            // Every presentation afterwards is again on its own account.
            Present(browser, A, SelectionContext.None);
            Assert.All(port.Calls.Take(2), c => Assert.False(c.UserRequested));
            Assert.All(port.Calls.Skip(2), c => Assert.True(c.UserRequested));
        }

        [Fact]
        public void ABusyPresentationInsideANoticeIsRetriedAtTheNextOneWhileAUserActionReadsAnyway()
        {
            var port = new WorkspaceBrowserSummaryPortDouble { RefuseUnrequested = true };
            var browser = Browser(port);

            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Single(port.Received);
            Assert.Empty(port.Calls);
            Assert.Contains("comando en curso", browser.SurfaceStatusText, StringComparison.Ordinal);

            // The next presentation tries again; still refused, it asks once more and still reads nothing.
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal(2, port.Received.Count);
            Assert.Empty(port.Calls);

            // The window is open at that presentation: it is answered, and a ready surface is never asked for again.
            port.RefuseUnrequested = false;
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal(("R-1", false), (Assert.Single(port.Calls).RackId, port.Calls[0].UserRequested));
            Assert.Equal("Resumen cargado.", browser.SurfaceStatusText);
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal(3, port.Received.Count);

            // A click of the user does not depend on that window.
            port.RefuseUnrequested = true;
            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            Assert.Equal(4, port.Received.Count);
            browser.ActivateTab("R-1");
            Assert.Equal(("R-1", true), (port.Calls.Last().RackId, port.Calls.Last().UserRequested));
            Assert.Equal("Resumen cargado.", browser.SurfaceStatusText);
        }

        // ------------------------------------------------------------------ INV-F3T2-24 (T2-F4): the explicit reread

        [Theory]
        [InlineData(BrowserSummaryStatus.Ready)]
        [InlineData(BrowserSummaryStatus.NotFound)]
        [InlineData(BrowserSummaryStatus.Rejected)]
        [InlineData(BrowserSummaryStatus.Failed)]
        [InlineData(BrowserSummaryStatus.Busy)]
        public void ReloadingFromAnyStateOfTheSurfaceAsksThePortAgainAsAUserActionAndPresentsTheNewResult(BrowserSummaryStatus first)
        {
            var port = new WorkspaceBrowserSummaryPortDouble
            {
                Responder = _ => first == BrowserSummaryStatus.Ready
                    ? WorkspaceBrowserSummaryPortDouble.Ready("Viejo", "Selective", "frontal")
                    : BrowserRackSummary.Without(first, "motivo-viejo"),
            };
            var changes = 0;
            var browser = Browser(port, () => changes++);
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Single(port.Received);
            Assert.NotEqual("Nuevo", browser.SurfaceTitle);

            port.Responder = _ => WorkspaceBrowserSummaryPortDouble.Ready("Nuevo", "Cantilever", "lateral", "planta");
            browser.ReloadSummary();

            Assert.Equal(2, port.Received.Count);
            Assert.Equal(("R-1", true), (port.Received[1].RackId, port.Received[1].UserRequested));
            Assert.Equal(A, port.Received[1].Origin.Origin);
            Assert.Equal("Nuevo", browser.SurfaceTitle);
            Assert.Equal("Tipo: Cantilever", browser.SurfaceKindText);
            Assert.Equal("Vistas: lateral, planta", browser.SurfaceViewsText);
            Assert.Equal("Resumen cargado.", browser.SurfaceStatusText);
            Assert.Equal("Nuevo", Assert.Single(browser.Tabs).Title);
            Assert.Equal("R-1", browser.ActiveRackId);
            Assert.Equal(0, changes);
        }

        [Fact]
        public void ReloadingAFailedSurfaceThatFailsAgainShowsTheNewReasonAndNothingEscapes()
        {
            var port = new WorkspaceBrowserSummaryPortDouble { Responder = _ => BrowserRackSummary.Without(BrowserSummaryStatus.Failed, "primero") };
            var browser = Browser(port);
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal("No se pudo leer el resumen: primero", browser.SurfaceStatusText);

            port.Responder = null;
            port.ThrowOnRead = new InvalidOperationException("segundo");
            var thrown = Record.Exception(() => browser.ReloadSummary());

            Assert.Null(thrown);
            Assert.Equal("No se pudo leer el resumen: segundo", browser.SurfaceStatusText);
            Assert.Equal(2, port.Received.Count);
        }

        [Fact]
        public void ReloadingWhileTheDrawingIsBlockedAsksNothingAndLeavesTheSurfacePendingUntilTheNextPresentation()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Single(port.Received);

            Present(browser, A, SelectionContext.One("R-1"), canRead: false, block: WorkspaceActionBlock.Degraded, blockText: "La sesión está degradada y no se lee.");
            browser.ReloadSummary();

            Assert.Single(port.Received);
            Assert.Contains("no se leyó el dibujo", browser.SurfaceStatusText, StringComparison.Ordinal);
            Assert.Contains("degradada", browser.SurfaceStatusText, StringComparison.Ordinal);
            Assert.Equal(string.Empty, browser.SurfaceKindText);

            Present(browser, A, SelectionContext.One("R-1"), canRead: true, block: WorkspaceActionBlock.CommandActive, blockText: "Hay un comando en curso.");
            browser.ReloadSummary();
            Assert.Single(port.Received);

            // The same pending surface is asked once when the drawing can be read again, on the account of the presentation.
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Equal(2, port.Received.Count);
            Assert.False(port.Received[1].UserRequested);
            Assert.Equal("Resumen cargado.", browser.SurfaceStatusText);
        }

        [Fact]
        public void ReloadingNeverChangesTheTabsAndNeverReachesTheNavigationNorTheHost()
        {
            var (panel, host, navigation, summary) = Panel();
            panel.Refresh();
            panel.Browser.PinTab("R-1");
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
            panel.Refresh();
            panel.Browser.ActivateTab("R-1");
            var snapshots = host.SnapshotCalls;
            var tabs = panel.Browser.Tabs.ToArray();
            var reads = summary.Received.Count;

            panel.Browser.ReloadSummary();

            Assert.Equal(reads + 1, summary.Received.Count);
            Assert.Equal(tabs.Select(t => (t.RackId, t.IsActive, t.IsPreview)), panel.Browser.Tabs.Select(t => (t.RackId, t.IsActive, t.IsPreview)));
            Assert.Equal("R-1", panel.Browser.ActiveRackId);
            Assert.Empty(navigation.Calls);
            Assert.Equal(snapshots, host.SnapshotCalls);
            Assert.Equal(0, host.AuthorizeCalls);
            Assert.Equal(0, host.SelectionCalls);
        }

        [Fact]
        public void APresentationNeverReadsAgainASurfaceThatIsReadyEvenAfterAReload()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);
            Present(browser, A, SelectionContext.One("R-1"));
            browser.ReloadSummary();
            Assert.Equal(2, port.Received.Count);

            Present(browser, A, SelectionContext.One("R-1"));
            Present(browser, A, SelectionContext.None);
            Present(browser, A, SelectionContext.One("R-1"));

            Assert.Equal(2, port.Received.Count);
        }

        [Fact]
        public void ReloadingWithoutATabOrWithoutAPortIsInert()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);
            browser.ReloadSummary();
            Present(browser, null, SelectionContext.None, canRead: false, block: WorkspaceActionBlock.NoDocument);
            browser.ReloadSummary();
            Assert.Empty(port.Received);

            var inert = new WorkspaceBrowserViewModel(null);
            var thrown = Record.Exception(() => inert.ReloadSummary());
            Assert.Null(thrown);
            Assert.Empty(inert.Tabs);
        }

        // ------------------------------------------------------------------ INV-F3T2-26 (T2-F6): the strip keeps its instance

        [Fact]
        public void APresentationWithNoChangeKeepsTheInstanceOfTheTabsAndDoesNotNotifyThem()
        {
            var browser = Browser();
            var raised = new List<string?>();
            browser.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            Present(browser, A, SelectionContext.One("R-1"));
            var strip = browser.Tabs;
            Assert.Contains(nameof(WorkspaceBrowserViewModel.Tabs), raised);

            raised.Clear();
            Present(browser, A, SelectionContext.One("R-1"));
            Present(browser, A, SelectionContext.None);
            Present(browser, A, SelectionContext.Selection(2, 0));
            browser.ReloadSummary();

            Assert.Same(strip, browser.Tabs);
            Assert.DoesNotContain(nameof(WorkspaceBrowserViewModel.Tabs), raised);
            Assert.Contains(nameof(WorkspaceBrowserViewModel.SurfaceStatusText), raised);
        }

        [Fact]
        public void ARealChangeOfNameOrOfTheActiveTabReplacesTheTabsAndNotifiesThem()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);
            var raised = new List<string?>();
            browser.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Waiting: the tab is titled by its rack id. When the summary arrives its name is the title: a real change.
            Present(browser, A, SelectionContext.One("R-1"), canRead: false, block: WorkspaceActionBlock.Degraded);
            var waiting = browser.Tabs;
            Assert.Equal("R-1", Assert.Single(waiting).Title);
            raised.Clear();
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.NotSame(waiting, browser.Tabs);
            Assert.Equal("Nombre R-1", Assert.Single(browser.Tabs).Title);
            Assert.Contains(nameof(WorkspaceBrowserViewModel.Tabs), raised);

            // A rename of the same rack on a reload is a real change too.
            var named = browser.Tabs;
            raised.Clear();
            port.Responder = _ => WorkspaceBrowserSummaryPortDouble.Ready("Otro nombre", "Selective", "frontal");
            browser.ReloadSummary();
            Assert.NotSame(named, browser.Tabs);
            Assert.Equal("Otro nombre", Assert.Single(browser.Tabs).Title);
            Assert.Contains(nameof(WorkspaceBrowserViewModel.Tabs), raised);

            // Pinning and changing the active tab are real changes.
            var before = browser.Tabs;
            raised.Clear();
            browser.PinTab("R-1");
            Assert.NotSame(before, browser.Tabs);
            Assert.False(Assert.Single(browser.Tabs).IsPreview);
            Assert.Contains(nameof(WorkspaceBrowserViewModel.Tabs), raised);

            Present(browser, A, SelectionContext.One("R-2"));
            var two = browser.Tabs;
            raised.Clear();
            browser.ActivateTab("R-1");
            Assert.NotSame(two, browser.Tabs);
            Assert.Equal(new[] { true, false }, browser.Tabs.Select(t => t.IsActive).ToArray());
            Assert.Contains(nameof(WorkspaceBrowserViewModel.Tabs), raised);
        }

        [Fact]
        public void ARefreshOfThePanelWithTheSameSnapshotKeepsTheInstanceOfTheTabs()
        {
            var (panel, _, _, _) = Panel();
            panel.Refresh();
            var strip = panel.Browser.Tabs;

            panel.Refresh();
            panel.Refresh();

            Assert.Same(strip, panel.Browser.Tabs);
        }

        // ------------------------------------------------------------------ INV-F3T2-07, 08, 09: one surface, values, memory only

        [Fact]
        public void TheLiveSurfaceCountIsZeroWithoutTabsAndExactlyOneWithAnyNumberOfTabs()
        {
            var browser = Browser();
            Assert.Equal(0, browser.LiveSurfaceCount);
            Assert.False(browser.HasSurface);

            for (var i = 1; i <= 4; i++)
            {
                Present(browser, A, SelectionContext.One("R-" + i));
                browser.PinTab("R-" + i);
                Assert.Equal(1, browser.LiveSurfaceCount);
                Assert.True(browser.HasSurface);
            }

            Assert.Equal(4, browser.Tabs.Count);
            Assert.Equal(1, browser.Tabs.Count(t => t.IsActive));
            Assert.True(browser.LiveSurfaceCount <= BrowserSession.MaxLiveSurfaces);
        }

        [Fact]
        public void TheTabItemIsAValueWithNoEventDelegateAutoCadObjectOrVisualReference()
        {
            var type = typeof(WorkspaceBrowserTabItem);
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            Assert.Empty(type.GetEvents(flags));
            Assert.False(typeof(INotifyPropertyChanged).IsAssignableFrom(type));
            Assert.True(typeof(IEquatable<WorkspaceBrowserTabItem>).IsAssignableFrom(type));
            Assert.Equal(
                new[] { "IsActive:Boolean", "IsPreview:Boolean", "RackId:String", "Title:String" },
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(p => p.Name + ":" + p.PropertyType.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());

            var used = type.GetFields(flags).Select(f => f.FieldType)
                .Concat(type.GetProperties(flags).Select(p => p.PropertyType))
                .Concat(type.GetMethods(flags).SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType)))
                .Where(t => t != typeof(Type) && t != typeof(void))
                .ToList();
            Assert.All(used, t =>
            {
                Assert.False(typeof(Delegate).IsAssignableFrom(t), "delegate " + t);
                Assert.False((t.Namespace ?? string.Empty).StartsWith("Autodesk", StringComparison.Ordinal), t.ToString());
                Assert.False((t.Namespace ?? string.Empty).StartsWith("System.Windows", StringComparison.Ordinal), t.ToString());
            });

            Assert.Equal(new WorkspaceBrowserTabItem("R-1", "Uno", true, false), new WorkspaceBrowserTabItem("R-1", "Uno", true, false));
        }

        [Fact]
        public void ActivatingPinningAndClosingOnlyChangeMemoryAndNeverReachAnythingButTheReadOnlySummaryPort()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var changes = 0;
            var browser = Browser(port, () => changes++);
            Present(browser, A, SelectionContext.One("R-1"));
            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            var methods = typeof(IWorkspaceBrowserSummaryPort).GetMethods().Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            Assert.Equal(new[] { "IsSessionAlive", "ReadSummary" }, methods);
            changes = 0;

            browser.ActivateTab("R-1");
            browser.PinTab("R-2");
            browser.CloseTab("R-2");

            Assert.Equal(3, changes);
            Assert.Equal(new[] { "R-1" }, Racks(browser));
            Assert.All(port.Calls, c => Assert.True(c.RackId == "R-1" || c.RackId == "R-2"));
        }

        [Fact]
        public void ClosingTheActiveTabMaterializesTheNextOneDeterministicallyWithoutAskingAnything()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);
            foreach (var rack in new[] { "R-1", "R-2", "R-3" })
            {
                Present(browser, A, SelectionContext.One(rack));
                browser.PinTab(rack);
            }

            browser.ActivateTab("R-2");
            var before = port.Calls.Count;

            browser.CloseTab("R-2");
            Assert.Equal("R-3", browser.ActiveRackId);
            Assert.Equal(before + 1, port.Calls.Count);
            Assert.Equal("R-3", port.Calls.Last().RackId);

            browser.CloseTab("R-3");
            Assert.Equal("R-1", browser.ActiveRackId);

            browser.CloseTab("R-1");
            Assert.Null(browser.ActiveRackId);
            Assert.Empty(browser.Tabs);
            Assert.Equal(0, browser.LiveSurfaceCount);
            Assert.Equal(string.Empty, browser.SurfaceTitle);
        }

        [Fact]
        public void AnUnknownTabIsIgnoredAndDoesNotRepresentTheNavigation()
        {
            var changes = 0;
            var browser = Browser(changed: () => changes++);
            Present(browser, A, SelectionContext.One("R-1"));

            browser.ActivateTab("R-404");
            browser.PinTab(null);
            browser.CloseTab(" ");
            browser.ActivateTab("R-1");

            Assert.Equal(0, changes);
            Assert.Equal(new[] { "R-1" }, Racks(browser));
        }

        // ------------------------------------------------------------------ INV-F3T2-13: the session of the request

        [Fact]
        public void EveryRequestCarriesTheSessionOfThePresentedSnapshotAndTheActiveRackOfThatSameSession()
        {
            var port = new WorkspaceBrowserSummaryPortDouble();
            var browser = Browser(port);

            Present(browser, A, SelectionContext.One("R-1"));
            Present(browser, B, SelectionContext.One("R-9"));
            Present(browser, A, SelectionContext.None);
            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            browser.ActivateTab("R-1");

            Assert.Equal(
                new[] { (A, "R-1"), (B, "R-9"), (A, "R-1"), (A, "R-2"), (A, "R-1") },
                port.Calls.Select(c => (c.Origin.Origin, c.RackId)).ToArray());
        }

        [Fact]
        public void ARequestOfSessionAWhileBIsActiveIsRejectedWithoutAReadAndTheTabsOfAAndBStayApart()
        {
            var port = new WorkspaceBrowserSummaryPortDouble { ActiveSession = B };
            var browser = Browser(port);

            Present(browser, A, SelectionContext.One("R-1"));

            Assert.Single(port.Received);
            Assert.Empty(port.Calls);
            Assert.Equal(new[] { "R-1" }, Racks(browser));
            Assert.Contains("otro documento", browser.SurfaceStatusText, StringComparison.Ordinal);

            Present(browser, B, SelectionContext.One("R-9"));
            Assert.Equal(new[] { "R-9" }, Racks(browser));
            Assert.Equal("Nombre R-9", browser.SurfaceTitle);
            Assert.Equal((B, "R-9"), (Assert.Single(port.Calls).Origin.Origin, port.Calls[0].RackId));
        }

        // ------------------------------------------------------------------ INV-F3T2-14: no work by time, inert without a port

        [Fact]
        public void RefreshStillMakesOneQueryOfTheSnapshotAndNeverAuthorizesOrAsksForTheContext()
        {
            var (panel, host, _, summary) = Panel();
            var snapshots = host.SnapshotCalls;

            panel.Refresh();

            Assert.Equal(snapshots + 1, host.SnapshotCalls);
            Assert.Equal(0, host.AuthorizeCalls);
            Assert.Equal(0, host.SelectionCalls);
            Assert.Single(summary.Calls);
        }

        [Fact]
        public void BuildingThePanelAsksNeitherTheSummaryPortNorTheNavigationPort()
        {
            var (_, _, navigation, summary) = Panel();

            Assert.Empty(summary.Received);
            Assert.Equal(0, summary.AliveCalls);
            Assert.Empty(navigation.Calls);
        }

        [Fact]
        public void WithoutASummaryPortTheBrowserIsInertAndThePanelBehavesExactlyAsBefore()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var panel = new WorkspacePanelViewModel(host, _ => { });

            panel.Refresh();
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
            panel.Refresh();

            Assert.False(panel.Browser.IsAvailable);
            Assert.Empty(panel.Browser.Tabs);
            Assert.Null(panel.Browser.ActiveRackId);
            Assert.Equal(0, panel.Browser.LiveSurfaceCount);
            Assert.False(panel.Browser.HasSurface);
            Assert.Equal(string.Empty, panel.Browser.SurfaceTitle);
            Assert.Equal("R-2", panel.RackId);
            Assert.Equal(WorkspacePanelState.Ready, panel.State);
            Assert.False(panel.Navigation.IsAvailable);

            panel.Browser.ActivateTab("R-2");
            panel.Browser.PinTab("R-2");
            panel.Browser.CloseTab("R-2");
            Assert.Empty(panel.Browser.Tabs);
        }

        [Fact]
        public void TheBrowserNeedsNoThreadTimerOrTaskAndEveryEntryReturnsBeforeTheCallerContinues()
        {
            var forbidden = new[] { typeof(System.Threading.Timer), typeof(System.Threading.Thread), typeof(System.Threading.Tasks.Task) };
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            var type = typeof(WorkspaceBrowserViewModel);

            Assert.All(type.GetFields(flags), f => Assert.DoesNotContain(f.FieldType, forbidden));
            Assert.All(type.GetMethods(flags), m =>
            {
                Assert.False(typeof(System.Threading.Tasks.Task).IsAssignableFrom(m.ReturnType), m.Name + " returns a task");
                Assert.False(m.GetCustomAttributes().Any(a => a.GetType().Name == "AsyncStateMachineAttribute"), m.Name + " is async");
            });
            Assert.Empty(type.GetEvents(flags));
            Assert.True(type.IsSealed);
        }

        // ------------------------------------------------------------------ INV-F3T2-15: failures are contained

        [Fact]
        public void AFailingSummaryPortBecomesAnErrorOfTheTabWithItsReasonAndNothingEscapes()
        {
            var port = new WorkspaceBrowserSummaryPortDouble { ThrowOnRead = new InvalidOperationException("boom-lectura") };
            var browser = Browser(port);

            var thrown = Record.Exception(() => Present(browser, A, SelectionContext.One("R-1")));

            Assert.Null(thrown);
            Assert.Equal("No se pudo leer el resumen: boom-lectura", browser.SurfaceStatusText);
            Assert.Equal(new[] { "R-1" }, Racks(browser));

            // The error is not retried by time or by presentation: it is one materialization that failed.
            Present(browser, A, SelectionContext.One("R-1"));
            Assert.Single(port.Received);
        }

        [Fact]
        public void AFailingInteractionIsContainedAsAnErrorOfTheLiveTab()
        {
            var changes = 0;
            var browser = Browser(changed: () =>
            {
                changes++;
                throw new InvalidOperationException("boom-pestaña");
            });
            Present(browser, A, SelectionContext.One("R-1"));
            browser.PinTab("R-1");
            Present(browser, A, SelectionContext.One("R-2"));
            changes = 0;

            var thrown = Record.Exception(() => browser.ActivateTab("R-1"));

            Assert.Null(thrown);
            Assert.Equal(1, changes);
            Assert.Equal("No se pudo leer el resumen: boom-pestaña", browser.SurfaceStatusText);
            Assert.Equal("R-1", browser.ActiveRackId);
        }

        [Fact]
        public void AFailureOfTheLivenessCheckReachesThePanelAsTheContainedErrorState()
        {
            var (panel, _, _, summary) = Panel();
            panel.Refresh();
            Assert.Equal(WorkspacePanelState.Ready, panel.State);

            summary.ThrowOnAlive = new InvalidOperationException("boom-vida");
            var thrown = Record.Exception(() => panel.Refresh());

            Assert.Null(thrown);
            Assert.Equal(WorkspacePanelState.Error, panel.State);
            Assert.Contains("boom-vida", panel.ErrorMessage, StringComparison.Ordinal);
        }
    }
}
