#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4-T3B2-EDIT-CLOSE-SYNC: closing a tab, discarding the field and the drafts of the document, the notices of veto and loss and the
    /// retention of the active tab by a selection (INV-F4T3B2-01 to 12), over the real view-models and the double of the edit port. The tests
    /// 11 to 14 are the coverage of the order of the Owner at the level of the UI. Nothing reads a drawing and nothing is written: the oracle
    /// is the exact set of operations of the port that were called. Assertions are written so that a missing behavior is always an assertion.
    /// </summary>
    public class WorkspacePilotCloseSyncTests
    {
        private static readonly SessionId A = new(7);
        private static readonly SessionId B = new(8);

        private const string NoRisk = "No hay borradores en riesgo en este documento; no se descartó nada.";

        private sealed class Rig
        {
            public readonly WorkspacePilotEditPortDouble Pilot = new();
            public readonly WorkspaceBrowserSummaryPortDouble Summary = new();
            public readonly WorkspaceBrowserViewModel Browser;

            public Rig(bool withPilot = true)
            {
                Browser = new WorkspaceBrowserViewModel(Summary, null, withPilot ? Pilot : null);
            }

            public WorkspacePilotFieldViewModel Field => Browser.Field;

            public void Present(SessionId? session, SelectionContext context) =>
                Browser.Present(session, context, true, WorkspaceActionBlock.None, string.Empty);

            public void Pinned(SessionId session, string rack)
            {
                Present(session, SelectionContext.One(rack));
                Browser.PinTab(rack);
            }

            // A pinned tab whose draft was opened by the first gesture (clean, not protected).
            public void Opened(SessionId session, string rack)
            {
                Pinned(session, rack);
                Browser.BeginFieldEdit();
            }

            // A pinned tab with a protected draft that carries pending text.
            public void Protected(SessionId session, string rack, string pending = "1:75")
            {
                Pinned(session, rack);
                Pilot.Set(session, rack, WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: pending, isProtected: true));
                Present(session, SelectionContext.One(rack));
            }

            public WorkspaceBrowserTabItem? Tab(string rack) => Browser.Tabs.FirstOrDefault(t => t.RackId == rack);

            public bool IsOpen(string rack) => Browser.Tabs.Any(t => t.RackId == rack);
        }

        private static PilotRiskView Risk(string[] known, string[]? uncertain = null) => new(known, uncertain ?? Array.Empty<string>());

        private static HashSet<string> Ops(WorkspacePilotEditPortDouble pilot) => pilot.Operations();

        private static void AssertOnly(WorkspacePilotEditPortDouble pilot, params string[] operations) =>
            Assert.True(pilot.Operations().SetEquals(operations), "called: " + string.Join(",", pilot.Operations().OrderBy(o => o, StringComparer.Ordinal)));

        private static (WorkspacePanelViewModel Panel, WorkspacePortDouble Host, WorkspaceNavigationPortDouble Navigation, WorkspacePilotEditPortDouble Pilot) PanelWithPilot(
            Action<WorkspacePilotEditPortDouble>? prepare = null, bool refresh = true)
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var navigation = new WorkspaceNavigationPortDouble();
            var pilot = new WorkspacePilotEditPortDouble();
            prepare?.Invoke(pilot);
            var panel = new WorkspacePanelViewModel(host, _ => { }, navigation, new WorkspaceBrowserSummaryPortDouble(), pilot);
            if (refresh) panel.Refresh();
            return (panel, host, navigation, pilot);
        }

        // ------------------------------------------------------------------ 11: discarding the field in Error and the drafts of the document

        [Fact]
        public void T11_DiscardingTheFieldInErrorRemovesTheDraftAndThePendingTextAndCallsExactlyTheDiscardAndReadsNothing()
        {
            var (panel, _, navigation, pilot) = PanelWithPilot();
            var browser = panel.Browser;
            browser.PinTab("R-1");
            pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Error, pending: "1:75", isProtected: true, canEdit: false, committedReason: "No se pudo leer el estado."));
            pilot.DiscardAsTheHostDoes();
            panel.Refresh();
            Assert.True(browser.Field.ShowDiscardButton);
            Assert.Equal("Nombre R-1 *", browser.Tabs.FirstOrDefault()?.Title);
            pilot.Calls.Clear();

            browser.DiscardField();

            Assert.True(browser.HasConfirmation);
            Assert.Empty(pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));

            browser.ConfirmDiscard();

            var discard = Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.Equal(A, discard.Origin);
            Assert.Equal("R-1", discard.RackId);
            Assert.False(pilot.Current(A, "R-1").Exists);
            Assert.Null(pilot.Current(A, "R-1").PendingText);
            Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, browser.Field.Text);
            Assert.Equal("Sin borrador.", browser.Field.StateText);
            Assert.False(browser.Field.ShowReloadButton);
            Assert.False(browser.Field.ShowDiscardButton);
            Assert.False(browser.HasConfirmation);
            Assert.Equal("Nombre R-1", browser.Tabs.FirstOrDefault()?.Title);
            // The oracle: the exact set of operations, no read of the intention, no navigation and nothing written.
            AssertOnly(pilot, WorkspacePilotEditPortDouble.GetFieldOp, WorkspacePilotEditPortDouble.DiscardRackOp);
            Assert.Empty(pilot.Reads());
            Assert.Empty(navigation.Calls);
        }

        [Fact]
        public void T11_DiscardingTheDraftsOfTheDocumentRemovesThemAndThePendingTextAndCallsExactlyTheRisksAndTheDiscard()
        {
            var (panel, _, navigation, pilot) = PanelWithPilot();
            var browser = panel.Browser;
            browser.PinTab("R-1");
            pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:75", isProtected: true));
            pilot.Set(A, "R-2", WorkspacePilotEditPortDouble.View());
            pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            pilot.DiscardAsTheHostDoes();
            panel.Refresh();
            pilot.Calls.Clear();

            browser.RequestDocumentDiscard();
            browser.ConfirmDiscard();

            Assert.Equal(2, pilot.Count(WorkspacePilotEditPortDouble.RisksOp));
            var discard = Assert.Single(pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.Equal(A, discard.Origin);
            Assert.False(pilot.Current(A, "R-1").Exists);
            Assert.False(pilot.Current(A, "R-2").Exists);
            Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, browser.Field.Text);
            Assert.Equal("Sin borrador.", browser.Field.StateText);
            Assert.Equal("Nombre R-1", browser.Tabs.FirstOrDefault()?.Title);
            Assert.True(browser.Tabs.Count == 1 && browser.ActiveRackId == "R-1");
            AssertOnly(pilot, WorkspacePilotEditPortDouble.GetFieldOp, WorkspacePilotEditPortDouble.RisksOp, WorkspacePilotEditPortDouble.DiscardDocumentOp);
            Assert.Empty(pilot.Reads());
            Assert.Empty(navigation.Calls);
        }

        // ------------------------------------------------------------------ 12, 13, 14: a protected tab, Cancel and Discard

        [Fact]
        public void T12_ClosingAProtectedTabAsksInlineAndDoesNotCloseIt()
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");
            rig.Pilot.Calls.Clear();

            rig.Browser.CloseTab("R-1");

            Assert.True(rig.Browser.HasConfirmation);
            Assert.Contains("Nombre R-1", rig.Browser.ConfirmationText);
            Assert.DoesNotContain(" *", rig.Browser.ConfirmationText);
            Assert.True(rig.IsOpen("R-1"));
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
        }

        [Fact]
        public void T12_AProtectedTabIsAlsoOneWhenThePortCannotSayAndItAsks()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("no se pudo leer");

            rig.Browser.CloseTab("R-1");

            Assert.True(rig.Browser.HasConfirmation);
            Assert.True(rig.IsOpen("R-1"));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
        }

        [Fact]
        public void T13_CancelKeepsTheTabTheDraftThePendingTextAndTheMarkAndCallsNothing()
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");
            rig.Browser.CloseTab("R-1");
            Assert.True(rig.Browser.HasConfirmation);
            rig.Pilot.Calls.Clear();

            rig.Browser.CancelDiscard();

            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(string.Empty, rig.Browser.ConfirmationText);
            Assert.Empty(rig.Pilot.Calls);
            Assert.True(rig.IsOpen("R-1"));
            Assert.Equal("Nombre R-1 *", rig.Tab("R-1")?.Title);
            Assert.Equal("1:75", rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Equal("1:75", rig.Field.Text);
        }

        [Fact]
        public void T14_DiscardCallsDiscardRackWithThatSessionAndRackClosesTheTabAndRemovesTheState()
        {
            var rig = new Rig();
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Protected(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Browser.CloseTab("R-1");
            rig.Pilot.Calls.Clear();

            rig.Browser.ConfirmDiscard();

            var discard = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.Equal(A, discard.Origin);
            Assert.Equal("R-1", discard.RackId);
            Assert.False(rig.IsOpen("R-1"));
            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.False(rig.Pilot.Current(A, "R-1").Exists);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(string.Empty, rig.Browser.NoticeText);
            Assert.Empty(rig.Pilot.Reads());
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
        }

        [Fact]
        public void TheRejectionOfTheConfirmationDoesNotCloseTheTabAndSaysWhy()
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");
            rig.Browser.CloseTab("R-1");

            rig.Browser.ConfirmDiscard();

            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.True(rig.IsOpen("R-1"));
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Contains("No se cerró la pestaña", rig.Browser.NoticeText);
            Assert.Contains("The double does not discard.", rig.Browser.NoticeText);
            Assert.Equal("1:75", rig.Pilot.Current(A, "R-1").PendingText);
        }

        [Theory]
        [InlineData("Busy")]
        [InlineData("Failed")]
        public void AnotherAnswerOfTheConfirmationDoesNotCloseTheTabEither(string status)
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");
            rig.Pilot.DiscardRackResponder = (_, _) => status == "Busy" ? EditDiscardOutcome.Busy("hay una lectura") : EditDiscardOutcome.Failed("falló el host");
            rig.Browser.CloseTab("R-1");

            rig.Browser.ConfirmDiscard();

            Assert.True(rig.IsOpen("R-1"));
            Assert.Contains("No se cerró la pestaña", rig.Browser.NoticeText);
            Assert.Contains(status == "Busy" ? "hay una lectura" : "falló el host", rig.Browser.NoticeText);
        }

        // ------------------------------------------------------------------ closing a tab with no draft, with a clean draft and with a failing port

        [Fact]
        public void ATabWithNoDraftClosesWithoutAnyDiscard()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Pilot.Calls.Clear();

            rig.Browser.CloseTab("R-2");

            Assert.False(rig.IsOpen("R-2"));
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(string.Empty, rig.Browser.NoticeText);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            AssertOnly(rig.Pilot, WorkspacePilotEditPortDouble.GetFieldOp);
        }

        [Fact]
        public void T14b_AKnownCleanTabClosesWithoutAnAuthoredWriteAndWithoutAnUpdate()
        {
            var rig = new Rig();
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Opened(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Pilot.Calls.Clear();

            rig.Browser.CloseTab("R-1");

            var discard = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.Equal("R-1", discard.RackId);
            Assert.False(rig.IsOpen("R-1"));
            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(string.Empty, rig.Browser.NoticeText);
            // Nothing is written and nothing is read: only the memory of the port and the discard of the draft.
            AssertOnly(rig.Pilot, WorkspacePilotEditPortDouble.GetFieldOp, WorkspacePilotEditPortDouble.DiscardRackOp);
        }

        [Theory]
        [InlineData("Rejected")]
        [InlineData("Busy")]
        [InlineData("Failed")]
        [InlineData("Throws")]
        public void ACleanDraftClosesWhateverTheDiscardAnswersAndTheNoticeSaysItStayedInMemory(string answer)
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Present(A, SelectionContext.One("R-1"));
            switch (answer)
            {
                case "Busy": rig.Pilot.DiscardRackResponder = (_, _) => EditDiscardOutcome.Busy("hay una lectura en curso"); break;
                case "Failed": rig.Pilot.DiscardRackResponder = (_, _) => EditDiscardOutcome.Failed("falló el host"); break;
                case "Throws": rig.Pilot.Throws[WorkspacePilotEditPortDouble.DiscardRackOp] = new InvalidOperationException("se cayó el puerto"); break;
            }

            rig.Browser.CloseTab("R-1");

            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.False(rig.IsOpen("R-1"));
            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Contains("Se cerró la pestaña; su borrador limpio quedó en memoria:", rig.Browser.NoticeText);
            var reason = answer switch
            {
                "Busy" => "hay una lectura en curso",
                "Failed" => "falló el host",
                "Throws" => "se cayó el puerto",
                _ => "The double does not discard.",
            };
            Assert.Contains(reason, rig.Browser.NoticeText);
            // The draft is still in memory: coming back reopens it as already open.
            Assert.True(rig.Pilot.Current(A, "R-1").Exists);
            Assert.False(rig.Browser.HasConfirmation);
        }

        [Fact]
        public void AnExceptionWhenClosingTheActiveCleanTabClosesItSaysWhyAndDoesNotPutTheFieldOfTheNewSurfaceInError()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.DiscardRackOp] = new InvalidOperationException("se cayó el puerto");

            rig.Browser.CloseTab("R-1");

            Assert.False(rig.IsOpen("R-1"));
            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Contains("se cayó el puerto", rig.Browser.NoticeText);
            Assert.DoesNotContain("Error", rig.Field.StateText);
            Assert.Equal("Sin borrador.", rig.Field.StateText);
            Assert.False(rig.Field.ShowReloadButton);
        }

        [Fact]
        public void ClosingATabThatIsNotTheActiveOneKeepsTheActiveTabAndSurface()
        {
            var rig = new Rig();
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Pinned(A, "R-1");
            rig.Opened(A, "R-2");
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            rig.Pilot.Calls.Clear();

            rig.Browser.CloseTab("R-2");

            Assert.False(rig.IsOpen("R-2"));
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Equal("R-1", rig.Browser.SurfaceTitle.Replace("Nombre ", string.Empty));
            Assert.Equal("R-2", Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp)).RackId);
        }

        [Fact]
        public void ClosingATabThatDoesNotExistChangesNothingAndCallsNothingToDiscard()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.Calls.Clear();

            rig.Browser.CloseTab("R-9");
            rig.Browser.CloseTab(null!);

            Assert.True(rig.IsOpen("R-1"));
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
        }

        [Fact]
        public void T14c_ClosingNeverReadsTheIntentionNorWritesAndNeverTouchesTheNavigation()
        {
            var (panel, _, navigation, pilot) = PanelWithPilot();
            var browser = panel.Browser;
            pilot.DiscardAsTheHostDoes();
            browser.PinTab("R-1");
            browser.BeginFieldEdit();
            browser.Field.Text = "1:75";
            pilot.Calls.Clear();

            browser.CloseTab("R-1");
            browser.ConfirmDiscard();

            Assert.Empty(browser.Tabs);
            Assert.Empty(pilot.Reads());
            foreach (var never in new[] { WorkspacePilotEditPortDouble.SetPendingOp, WorkspacePilotEditPortDouble.CommitOp, WorkspacePilotEditPortDouble.BoundaryOp })
                Assert.Equal(0, pilot.Count(never));
            Assert.Empty(navigation.Calls);
        }

        // ------------------------------------------------------------------ the confirmation: one at a time and its lifetime

        [Fact]
        public void TheConfirmationNamesATabThatIsNotActiveAndANewRequestReplacesIt()
        {
            var rig = new Rig();
            rig.Protected(A, "R-2");
            rig.Protected(A, "R-1");
            // The pending text of R-2 held the selection of R-1 behind it; the user goes to R-1 (a valid text proceeds and stays protected).
            rig.Browser.ActivateTab("R-1");
            Assert.Equal("R-1", rig.Browser.ActiveRackId);

            rig.Browser.CloseTab("R-2");
            Assert.True(rig.Browser.HasConfirmation);
            Assert.Contains("R-2", rig.Browser.ConfirmationText);
            Assert.DoesNotContain("R-1", rig.Browser.ConfirmationText);

            rig.Browser.CloseTab("R-1");

            Assert.True(rig.Browser.HasConfirmation);
            Assert.Contains("R-1", rig.Browser.ConfirmationText);
            Assert.DoesNotContain("R-2", rig.Browser.ConfirmationText);

            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1", "R-2" });
            rig.Browser.RequestDocumentDiscard();

            Assert.True(rig.Browser.HasConfirmation);
            Assert.Contains("documento", rig.Browser.ConfirmationText);
            Assert.Contains("R-2", rig.Browser.ConfirmationText);
        }

        [Fact]
        public void AChangeOfThePresentedSessionRemovesTheConfirmation()
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");
            rig.Browser.CloseTab("R-1");
            Assert.True(rig.Browser.HasConfirmation);
            rig.Pilot.Calls.Clear();

            rig.Present(B, SelectionContext.One("R-9"));

            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(string.Empty, rig.Browser.ConfirmationText);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));

            // A leftover Discard of a confirmation that is gone asks nothing of the port.
            rig.Browser.ConfirmDiscard();
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
        }

        [Fact]
        public void TheConfirmationOfTheFieldGoesWhenTheLiveSurfaceChanges()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Error, isProtected: true, canEdit: false, committedReason: "x"));
            rig.Pinned(A, "R-1");
            rig.Browser.DiscardField();
            Assert.True(rig.Browser.HasConfirmation);

            rig.Browser.ActivateTab("R-2");

            Assert.False(rig.Browser.HasConfirmation);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
        }

        // ------------------------------------------------------------------ the field in Error

        [Fact]
        public void TheDiscardButtonOfTheFieldIsTheReloadButtonWithADraft()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            Assert.False(rig.Field.ShowDiscardButton);

            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Error, isProtected: true, canEdit: false, committedReason: "x"));
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.True(rig.Field.ShowReloadButton);
            Assert.True(rig.Field.ShowDiscardButton);

            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View());
            rig.Browser.ReloadField();
            Assert.False(rig.Field.ShowReloadButton);
            Assert.False(rig.Field.ShowDiscardButton);

            // A failure of the port without a draft shows Reload but not Discard.
            var empty = new Rig();
            empty.Pinned(A, "R-1");
            empty.Pilot.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("boom");
            empty.Present(A, SelectionContext.One("R-1"));
            Assert.True(empty.Field.ShowReloadButton);
            Assert.False(empty.Field.ShowDiscardButton);
        }

        [Fact]
        public void DiscardingTheFieldOfAnUnprotectedDraftInAFailureDiscardsAtOnceAndTheTabStays()
        {
            var rig = new Rig();
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Opened(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.SetPendingOp] = new InvalidOperationException("falló el puerto");
            rig.Field.Text = "1:75";
            rig.Pilot.Throws.Clear();
            Assert.True(rig.Field.ShowDiscardButton);
            Assert.Contains("Error", rig.Field.StateText);
            rig.Pilot.Calls.Clear();

            rig.Browser.DiscardField();

            Assert.False(rig.Browser.HasConfirmation);
            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.True(rig.IsOpen("R-1"));
            Assert.Equal("Sin borrador.", rig.Field.StateText);
            Assert.False(rig.Field.ShowReloadButton);
        }

        [Fact]
        public void DiscardingTheFieldWhenThePortRefusesLeavesTheFieldAsItWasAndSaysWhy()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Error, pending: "1:75", isProtected: true, canEdit: false, committedReason: "x"));
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Browser.DiscardField();

            rig.Browser.ConfirmDiscard();

            Assert.Contains("No se descartó el borrador", rig.Browser.NoticeText);
            Assert.True(rig.IsOpen("R-1"));
            Assert.True(rig.Field.ShowDiscardButton);
            Assert.Equal("1:75", rig.Pilot.Current(A, "R-1").PendingText);
        }

        [Fact]
        public void AnExceptionWhileDiscardingTheFieldSaysWhyRemovesTheConfirmationAndShowsTheErrorOfTheFieldWithItsDraft()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Error, pending: "1:75", isProtected: true, canEdit: false, committedReason: "x"));
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Browser.DiscardField();
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.DiscardRackOp] = new InvalidOperationException("se cayó el puerto");

            rig.Browser.ConfirmDiscard();

            Assert.False(rig.Browser.HasConfirmation);
            Assert.Contains("se cayó el puerto", rig.Browser.NoticeText);
            Assert.Contains("se cayó el puerto", rig.Field.StateText);
            Assert.True(rig.IsOpen("R-1"));
            Assert.Equal("1:75", rig.Pilot.Current(A, "R-1").PendingText);
        }

        [Fact]
        public void AnExceptionWhileConfirmingTheCloseOfAnInactiveTabDoesNotPutTheActiveFieldInError()
        {
            var rig = new Rig();
            rig.Protected(A, "R-2");
            rig.Pinned(A, "R-1");
            rig.Browser.ActivateTab("R-1");
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            rig.Browser.CloseTab("R-2");
            Assert.True(rig.Browser.HasConfirmation);
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.DiscardRackOp] = new InvalidOperationException("se cayó el puerto");

            rig.Browser.ConfirmDiscard();

            Assert.True(rig.IsOpen("R-2"));
            Assert.Contains("se cayó el puerto", rig.Browser.NoticeText);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.DoesNotContain("Error", rig.Field.StateText);
        }

        // ------------------------------------------------------------------ the drafts of the document

        [Fact]
        public void WithNoRacksAtRiskTheDocumentDiscardOnlySaysSoAndHasNoEffect()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.Calls.Clear();

            rig.Browser.RequestDocumentDiscard();

            Assert.Equal(NoRisk, rig.Browser.NoticeText);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(1, rig.Pilot.Count(WorkspacePilotEditPortDouble.RisksOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
        }

        [Fact]
        public void WithRacksAtRiskTheConfirmationListsThemByTitleAndTheUncertainOnesApart()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1", "R-3" }, new[] { "R-4" });
            rig.Pilot.Calls.Clear();

            rig.Browser.RequestDocumentDiscard();

            Assert.True(rig.Browser.HasConfirmation);
            var text = rig.Browser.ConfirmationText;
            Assert.Contains("Nombre R-1, R-3", text);
            Assert.Contains("No se pudo decidir: R-4.", text);
            var asked = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.RisksOp));
            Assert.Equal(A, asked.Origin);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.True(rig.Browser.CanDiscardDocument);
        }

        [Fact]
        public void ConfirmingTheDocumentDiscardReadsTheRisksAgainAndDiscardsWhenTheyDidNotChange()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.Set(A, "R-3", WorkspacePilotEditPortDouble.View());
            rig.Pilot.Set(A, "R-5", WorkspacePilotEditPortDouble.View());
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1", "R-3" });
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Browser.RequestDocumentDiscard();
            rig.Pilot.Calls.Clear();

            rig.Browser.ConfirmDiscard();

            Assert.Equal(1, rig.Pilot.Count(WorkspacePilotEditPortDouble.RisksOp));
            var discard = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.Equal(A, discard.Origin);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Contains("Se descartaron los borradores en riesgo: Nombre R-1, R-3.", rig.Browser.NoticeText);
            Assert.Contains("Borradores limpios retirados además: 1.", rig.Browser.NoticeText);
            Assert.False(rig.Pilot.Current(A, "R-1").Exists);
            Assert.False(rig.Pilot.Current(A, "R-5").Exists);
            // The tabs are not closed, they lose the mark and the field says there is no draft.
            Assert.True(rig.IsOpen("R-1"));
            Assert.Equal("Nombre R-1", rig.Tab("R-1")?.Title);
            Assert.Equal("Sin borrador.", rig.Field.StateText);
        }

        [Fact]
        public void TheTocTouListThatChangedIsNotDiscardedAndIsShownAgainWithItsNoticeAndTheOneThatDidNotChangeIsDiscarded()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            rig.Browser.RequestDocumentDiscard();
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1", "R-2" });
            rig.Pilot.Calls.Clear();

            rig.Browser.ConfirmDiscard();

            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.True(rig.Browser.HasConfirmation);
            Assert.Contains("R-2", rig.Browser.ConfirmationText);
            Assert.Equal("La lista de borradores en riesgo cambió; revísala.", rig.Browser.NoticeText);

            rig.Browser.ConfirmDiscard();

            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.Equal(2, rig.Pilot.Count(WorkspacePilotEditPortDouble.RisksOp));
        }

        [Fact]
        public void TheTocTouComparisonIsInOrderAndOrdinalAndAnEmptyNewListRemovesTheConfirmation()
        {
            var order = new Rig();
            order.Opened(A, "R-1");
            order.Pilot.RisksResponder = _ => Risk(new[] { "R-1", "R-2" });
            order.Browser.RequestDocumentDiscard();
            order.Pilot.RisksResponder = _ => Risk(new[] { "R-2", "R-1" });
            order.Browser.ConfirmDiscard();
            Assert.Empty(order.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.True(order.Browser.HasConfirmation);

            var ordinal = new Rig();
            ordinal.Opened(A, "R-1");
            ordinal.Pilot.RisksResponder = _ => Risk(new[] { "r-1" });
            ordinal.Browser.RequestDocumentDiscard();
            ordinal.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            ordinal.Browser.ConfirmDiscard();
            Assert.Empty(ordinal.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));

            var uncertain = new Rig();
            uncertain.Opened(A, "R-1");
            uncertain.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            uncertain.Browser.RequestDocumentDiscard();
            uncertain.Pilot.RisksResponder = _ => Risk(new[] { "R-1" }, new[] { "R-7" });
            uncertain.Browser.ConfirmDiscard();
            Assert.Empty(uncertain.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));

            var empty = new Rig();
            empty.Opened(A, "R-1");
            empty.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            empty.Browser.RequestDocumentDiscard();
            empty.Pilot.RisksResponder = _ => Risk(Array.Empty<string>());
            empty.Browser.ConfirmDiscard();
            Assert.Empty(empty.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.False(empty.Browser.HasConfirmation);
            Assert.Equal(NoRisk, empty.Browser.NoticeText);
        }

        [Fact]
        public void CancelOfTheDocumentDiscardCallsNothing()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            rig.Browser.RequestDocumentDiscard();
            rig.Pilot.Calls.Clear();

            rig.Browser.CancelDiscard();

            Assert.False(rig.Browser.HasConfirmation);
            Assert.Empty(rig.Pilot.Calls);
        }

        [Fact]
        public void TheDocumentDiscardRefusedBySomethingOtherThanDiscardedSaysWhyAndAFailingPortIsContained()
        {
            var refused = new Rig();
            refused.Opened(A, "R-1");
            refused.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            refused.Browser.RequestDocumentDiscard();
            refused.Browser.ConfirmDiscard();
            Assert.Contains("No se descartaron los borradores del documento", refused.Browser.NoticeText);
            Assert.False(refused.Browser.HasConfirmation);

            var throwsRisks = new Rig();
            throwsRisks.Opened(A, "R-1");
            throwsRisks.Pilot.Throws[WorkspacePilotEditPortDouble.RisksOp] = new InvalidOperationException("sin riesgos");
            throwsRisks.Browser.RequestDocumentDiscard();
            Assert.Contains("sin riesgos", throwsRisks.Browser.NoticeText);
            Assert.False(throwsRisks.Browser.HasConfirmation);

            var throwsDiscard = new Rig();
            throwsDiscard.Opened(A, "R-1");
            throwsDiscard.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            throwsDiscard.Browser.RequestDocumentDiscard();
            throwsDiscard.Pilot.Throws[WorkspacePilotEditPortDouble.DiscardDocumentOp] = new InvalidOperationException("sin descartar");
            throwsDiscard.Browser.ConfirmDiscard();
            Assert.Contains("sin descartar", throwsDiscard.Browser.NoticeText);
            Assert.False(throwsDiscard.Browser.HasConfirmation);
            Assert.False(throwsDiscard.Field.StateText.StartsWith("Error", StringComparison.Ordinal));
        }

        // ------------------------------------------------------------------ the notices of veto and loss

        [Fact]
        public void TheVetoOfThePresentedSessionHasItsTextAndTheDiscardButtonAndNamesNoWayToApply()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1", "R-3" }, false));

            rig.Present(A, SelectionContext.One("R-1"));

            var row = Assert.Single(rig.Browser.Notices);
            Assert.True(rig.Browser.HasNotices);
            Assert.True(row.CanDiscardDocument);
            Assert.Equal(
                "No se cerró el documento: hay borradores sin aplicar en Nombre R-1, R-3. Descártalos con \"Descartar borradores de este documento\" o con el comando RACKPANELDESCARTAR; cada pestaña también se descarta con su \"Cerrar\".",
                row.Text);
            Assert.DoesNotContain("Actualizar", row.Text);
            Assert.DoesNotContain("Aplicar", row.Text);
            // I-64 F4-T3C2 (D-T3C2-06): the notice names the two ways to discard, the panel and the command.
            Assert.Contains("RACKPANELDESCARTAR", row.Text);
            Assert.False(string.IsNullOrWhiteSpace(row.Key));
        }

        // I-64 F4-T3C2 (D-T3C2-06): the declared deviation of D-T3B2-04. A discard of the document by the COMMAND goes through the port and
        // not through the panel, so the row of veto stays until "Entendido", a new veto or the death of the session; its button then says that
        // nothing is at risk. This is the cheap evidence of that behavior: it is a declared limitation, not a defect to fix here.
        [Fact]
        public void AVetoRowStaysAfterTheDocumentWasDiscardedOutsideThePanelAndItsButtonThenSaysThatNothingIsAtRisk()
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false));
            rig.Present(A, SelectionContext.One("R-1"));
            var row = Assert.Single(rig.Browser.Notices);
            Assert.True(row.CanDiscardDocument);

            var outcome = rig.Pilot.DiscardDocument(new WorkspaceRequest(A));
            Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
            rig.Present(A, SelectionContext.One("R-1"));

            Assert.False(rig.Pilot.Current(A, "R-1").Exists);
            Assert.Null(rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, rig.Field.Text);
            var stays = Assert.Single(rig.Browser.Notices);
            Assert.Equal(row.Key, stays.Key);
            Assert.Contains("RACKPANELDESCARTAR", stays.Text);

            rig.Pilot.RisksResponder = _ => Risk(Array.Empty<string>());
            rig.Browser.RequestDocumentDiscard();

            Assert.Equal(NoRisk, rig.Browser.NoticeText);
            Assert.False(rig.Browser.HasConfirmation);
        }

        // ------------------------------------------------------------------ I-64 F4-T3C2 (D-T3C2-11): the minor findings of F4-T3C1

        [Fact]
        public void R1_ACleanTabClosedWhileTheDiscardIsBusyWithAReasonSaysExactlyThatReason()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Pilot.DiscardRackResponder = (_, _) => EditDiscardOutcome.Busy("Hay una lectura del piloto en curso.");

            rig.Browser.CloseTab("R-1");

            Assert.False(rig.IsOpen("R-1"));
            Assert.Equal(
                "Se cerró la pestaña; su borrador limpio quedó en memoria: Hay una lectura del piloto en curso.",
                rig.Browser.NoticeText);
        }

        [Fact]
        public void R1_ACleanTabClosedWhileTheDiscardIsBusyWithoutAReasonSaysThatAReadIsInProgress()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Present(A, SelectionContext.One("R-1"));
            // A blank reason stands for "no reason": the generic text of the panel takes its place.
            rig.Pilot.DiscardRackResponder = (_, _) => EditDiscardOutcome.Busy(" ");

            rig.Browser.CloseTab("R-1");

            Assert.False(rig.IsOpen("R-1"));
            Assert.Equal(
                "Se cerró la pestaña; su borrador limpio quedó en memoria: hay una lectura en curso.",
                rig.Browser.NoticeText);
        }

        [Fact]
        public void R2_TwoClosesOfTheSameTabInARowWithoutCancellingAskTheUsualTextTheSecondTime()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("no se pudo leer");

            rig.Browser.CloseTab("R-1");

            Assert.True(rig.Browser.HasConfirmation);
            Assert.Equal(
                "No se pudo comprobar la pestaña Nombre R-1; puede tener trabajo sin aplicar. ¿Descartar lo que tenga y cerrarla?",
                rig.Browser.ConfirmationText);

            // No CancelDiscard in between: the second request replaces the question and the memory of the first one.
            rig.Pilot.Throws.Clear();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:75", isProtected: true));
            rig.Browser.CloseTab("R-1");

            Assert.True(rig.Browser.HasConfirmation);
            Assert.Equal(
                "La pestaña Nombre R-1 tiene un borrador con trabajo sin aplicar. ¿Descartarlo y cerrar la pestaña?",
                rig.Browser.ConfirmationText);
            Assert.True(rig.IsOpen("R-1"));
        }

        [Fact]
        public void AnUncertainVetoSaysSoAndTheVetoOfAnotherSessionHasNoButtonUntilItIsPresented()
        {
            var uncertain = new Rig();
            uncertain.Pinned(A, "R-1");
            uncertain.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, Array.Empty<string>(), true));
            uncertain.Present(A, SelectionContext.One("R-1"));
            var uncertainRow = Assert.Single(uncertain.Browser.Notices);
            Assert.Contains("no se pudo decidir qué borradores están en riesgo", uncertainRow.Text);
            Assert.True(uncertainRow.CanDiscardDocument);

            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(B, new[] { "R-9" }, false));
            rig.Present(A, SelectionContext.One("R-1"));
            var other = Assert.Single(rig.Browser.Notices);
            Assert.Equal("No se cerró otro documento con borradores sin aplicar; actívalo para descartarlos con el panel o con el comando RACKPANELDESCARTAR.", other.Text);
            Assert.False(other.CanDiscardDocument);

            rig.Present(B, SelectionContext.One("R-9"));

            var presented = Assert.Single(rig.Browser.Notices);
            Assert.Equal(other.Key, presented.Key);
            Assert.True(presented.CanDiscardDocument);
            Assert.Contains("hay borradores sin aplicar en", presented.Text);
        }

        [Fact]
        public void ANewVetoOfTheSameSessionReplacesTheOldOneAndTheButtonOfTheRowStartsTheSameDiscardAsTheOneOfTheSection()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false));
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1", "R-2" }, false));
            rig.Present(A, SelectionContext.One("R-1"));

            var row = Assert.Single(rig.Browser.Notices);
            Assert.Contains("Nombre R-1, R-2", row.Text);

            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1", "R-2" });
            rig.Browser.RequestDocumentDiscard();
            Assert.True(rig.Browser.HasConfirmation);
        }

        [Fact]
        public void ALossNamesTheLostRacksAndApartTheUncertainOnesWithTheirReasonAndOnlyUnderstoodRemovesIt()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.LossQueue.Enqueue(new PilotLossView(
                B, new[] { "R-1", "R-2" }, new[] { "R-3" }, new Dictionary<string, string> { ["R-3"] = "falló la comparación" }));

            rig.Present(A, SelectionContext.One("R-1"));
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Present(B, SelectionContext.One("R-9"));
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Summary.Dead.Add(B);
            rig.Present(A, SelectionContext.One("R-1"));

            var row = Assert.Single(rig.Browser.Notices);
            Assert.Equal("Se perdieron borradores al cerrar un documento: R-1, R-2. Sin decidir: R-3 (falló la comparación).", row.Text);
            Assert.False(row.CanDiscardDocument);

            rig.Browser.DismissNotice(row.Key);

            Assert.Empty(rig.Browser.Notices);
            Assert.False(rig.Browser.HasNotices);
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Empty(rig.Browser.Notices);
        }

        [Fact]
        public void UnderstoodRemovesOnlyItsRowAndCallsNothingOfThePort()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.LossQueue.Enqueue(new PilotLossView(B, new[] { "R-1" }, Array.Empty<string>(), new Dictionary<string, string>()));
            rig.Pilot.LossQueue.Enqueue(new PilotLossView(B, new[] { "R-2" }, Array.Empty<string>(), new Dictionary<string, string>()));
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Equal(2, rig.Browser.Notices.Count);
            Assert.NotEqual(rig.Browser.Notices.FirstOrDefault()?.Key, rig.Browser.Notices.LastOrDefault()?.Key);
            var first = rig.Browser.Notices.FirstOrDefault();
            rig.Pilot.Calls.Clear();

            rig.Browser.DismissNotice(first?.Key ?? "none");

            var left = Assert.Single(rig.Browser.Notices);
            Assert.Contains("R-2", left.Text);
            // Only the memory of the port is read to redraw the strip: nothing is discarded, asked or taken.
            Assert.DoesNotContain(rig.Pilot.Calls, c => c.Operation != WorkspacePilotEditPortDouble.GetFieldOp);
            rig.Browser.DismissNotice("unknown");
            Assert.Single(rig.Browser.Notices);
        }

        [Fact]
        public void TheRowsKeepTheirInstanceWhileNothingChangesByValue()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false));
            rig.Pilot.LossQueue.Enqueue(new PilotLossView(B, new[] { "R-2" }, Array.Empty<string>(), new Dictionary<string, string>()));
            rig.Present(A, SelectionContext.One("R-1"));
            var rows = rig.Browser.Notices;
            Assert.Equal(2, rows.Count);
            var first = rows.FirstOrDefault();

            rig.Present(A, SelectionContext.One("R-1"));
            rig.Browser.ReloadSummary();

            Assert.Same(rows, rig.Browser.Notices);
            Assert.Same(first, rig.Browser.Notices.FirstOrDefault());
        }

        [Fact]
        public void TheVetoOfASessionThatNoLongerLivesIsPurgedInTheLoopOfTheDeadSessionsWithOneQuestionPerSession()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(B, new[] { "R-9" }, false));
            rig.Pilot.LossQueue.Enqueue(new PilotLossView(B, new[] { "R-9" }, Array.Empty<string>(), new Dictionary<string, string>()));
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Equal(2, rig.Browser.Notices.Count);
            var asked = rig.Summary.AliveCalls;

            rig.Present(A, SelectionContext.One("R-1"));

            // The known session and the session of the veto: one question each.
            Assert.Equal(2, rig.Summary.AliveCalls - asked);
            rig.Summary.Dead.Add(B);

            rig.Present(A, SelectionContext.One("R-1"));

            var left = Assert.Single(rig.Browser.Notices);
            Assert.Contains("Se perdieron borradores", left.Text);
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
        }

        [Fact]
        public void AfterDiscardingARackItLeavesTheVetoOfItsSessionAndTheRowGoesWhenItIsEmptyAndNotUncertain()
        {
            var rig = new Rig();
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Opened(A, "R-1");
            rig.Opened(A, "R-2");
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1", "R-2" }, false));
            rig.Present(A, SelectionContext.One("R-2"));
            Assert.Contains("Nombre R-1, Nombre R-2", Assert.Single(rig.Browser.Notices).Text);

            rig.Browser.CloseTab("R-1");

            var row = Assert.Single(rig.Browser.Notices);
            Assert.Contains("sin aplicar en Nombre R-2.", row.Text);
            Assert.DoesNotContain("R-1", row.Text);

            rig.Browser.CloseTab("R-2");

            Assert.Empty(rig.Browser.Notices);

            var uncertain = new Rig();
            uncertain.Pilot.DiscardAsTheHostDoes();
            uncertain.Opened(A, "R-1");
            uncertain.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, Array.Empty<string>(), true));
            uncertain.Present(A, SelectionContext.One("R-1"));
            uncertain.Browser.CloseTab("R-1");
            Assert.Single(uncertain.Browser.Notices);
        }

        [Fact]
        public void TheDocumentDiscardRemovesTheVetoOfThatSessionButNotTheOneOfAnotherNorALoss()
        {
            var rig = new Rig();
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Opened(A, "R-1");
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false));
            rig.Pilot.CloseQueue.Enqueue(new PilotCloseView(B, new[] { "R-9" }, false));
            rig.Pilot.LossQueue.Enqueue(new PilotLossView(B, new[] { "R-7" }, Array.Empty<string>(), new Dictionary<string, string>()));
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Equal(3, rig.Browser.Notices.Count);
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });

            rig.Browser.RequestDocumentDiscard();
            rig.Browser.ConfirmDiscard();

            Assert.Equal(2, rig.Browser.Notices.Count);
            Assert.DoesNotContain(rig.Browser.Notices, n => n.CanDiscardDocument);
            Assert.Contains(rig.Browser.Notices, n => n.Text.StartsWith("No se cerró otro documento", StringComparison.Ordinal));
            Assert.Contains(rig.Browser.Notices, n => n.Text.StartsWith("Se perdieron borradores", StringComparison.Ordinal));
        }

        [Fact]
        public void TheQueuesAreConsumedOnlyByThePresentationAndNeverByAGesture()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Pilot.Calls.Clear();

            rig.Browser.ActivateTab("R-1");
            rig.Browser.PinTab("R-1");
            rig.Browser.CloseTab("R-2");
            rig.Browser.RequestDocumentDiscard();
            rig.Browser.DiscardField();
            rig.Browser.CancelDiscard();
            rig.Browser.ReloadSummary();
            rig.Browser.DismissNotice("none");

            Assert.Equal(0, rig.Pilot.Count(WorkspacePilotEditPortDouble.LossOp));
            Assert.Equal(0, rig.Pilot.Count(WorkspacePilotEditPortDouble.CloseOp));

            rig.Present(A, SelectionContext.One("R-1"));

            Assert.Equal(1, rig.Pilot.Count(WorkspacePilotEditPortDouble.LossOp));
            Assert.Equal(1, rig.Pilot.Count(WorkspacePilotEditPortDouble.CloseOp));
        }

        [Fact]
        public void ThePanelConsumesTheQueuesWhenItIsBuiltAndOnEveryRefresh()
        {
            var (panel, host, _, pilot) = PanelWithPilot(
                p => p.LossQueue.Enqueue(new PilotLossView(B, new[] { "R-8" }, Array.Empty<string>(), new Dictionary<string, string>())),
                refresh: false);

            Assert.Equal(1, pilot.Count(WorkspacePilotEditPortDouble.LossOp));
            Assert.Equal(1, pilot.Count(WorkspacePilotEditPortDouble.CloseOp));
            Assert.Single(panel.Browser.Notices);

            pilot.CloseQueue.Enqueue(new PilotCloseView(B, new[] { "R-9" }, false));
            panel.Refresh();

            Assert.Equal(2, pilot.Count(WorkspacePilotEditPortDouble.LossOp));
            Assert.Equal(2, panel.Browser.Notices.Count);
            Assert.NotNull(host);
        }

        [Fact]
        public void AFailureOfAQueueLeavesANoticeOfTheBrowserAndDoesNotStopThePresentationNorTheOtherQueue()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.CloseOp] = new InvalidOperationException("sin avisos de cierre");
            rig.Pilot.LossQueue.Enqueue(new PilotLossView(B, new[] { "R-7" }, Array.Empty<string>(), new Dictionary<string, string>()));

            var thrown = Record.Exception(() => rig.Present(A, SelectionContext.One("R-2")));

            Assert.Null(thrown);
            Assert.Contains("No se pudieron leer los avisos", rig.Browser.NoticeText);
            Assert.Contains("sin avisos de cierre", rig.Browser.NoticeText);
            Assert.Single(rig.Browser.Notices);
            Assert.Equal("R-2", rig.Browser.ActiveRackId);

            var both = new Rig();
            both.Pinned(A, "R-1");
            both.Pilot.Throws[WorkspacePilotEditPortDouble.LossOp] = new InvalidOperationException("sin avisos de pérdida");
            both.Present(A, SelectionContext.One("R-1"));
            Assert.Contains("sin avisos de pérdida", both.Browser.NoticeText);
        }

        // ------------------------------------------------------------------ the notice of the browser: its lifetime

        [Fact]
        public void TheNoticeOfTheBrowserSurvivesAPresentationWithoutChangeOfSurfaceAndGoesWithAChangeOfTheActiveTabAlsoByAGestureOfF4T3B1()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Pinned(A, "R-1");
            rig.Browser.RequestDocumentDiscard();
            Assert.Equal(NoRisk, rig.Browser.NoticeText);

            rig.Present(A, SelectionContext.One("R-1"));
            rig.Browser.ReloadSummary();
            Assert.Equal(NoRisk, rig.Browser.NoticeText);

            rig.Browser.ActivateTab("R-2");

            Assert.Equal(string.Empty, rig.Browser.NoticeText);

            rig.Browser.RequestDocumentDiscard();
            Assert.Equal(NoRisk, rig.Browser.NoticeText);
            rig.Present(B, SelectionContext.One("R-9"));
            Assert.Equal(string.Empty, rig.Browser.NoticeText);
        }

        [Fact]
        public void EveryGestureOfTheTaskStartsByEndingTheNoticeOfTheBrowser()
        {
            foreach (var gesture in new Action<WorkspaceBrowserViewModel>[]
                     {
                         b => b.RequestDocumentDiscard(),
                         b => b.CancelDiscard(),
                         b => b.DiscardField(),
                         b => b.ConfirmDiscard(),
                         b => b.DismissNotice("none"),
                         b => b.CloseTab("R-1"),
                     })
            {
                var rig = new Rig();
                rig.Pinned(A, "R-1");
                rig.Pinned(A, "R-2");
                rig.Pilot.Set(A, "R-2", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:75", isProtected: true));
                rig.Present(A, SelectionContext.One("R-2"));
                rig.Browser.RequestDocumentDiscard();
                Assert.Equal(NoRisk, rig.Browser.NoticeText);
                // From now on a request has something to ask, so no gesture says anything by itself.
                rig.Pilot.RisksResponder = _ => Risk(new[] { "R-2" });

                gesture(rig.Browser);

                Assert.Equal(string.Empty, rig.Browser.NoticeText);
            }
        }

        // ------------------------------------------------------------------ the retention by selection

        [Fact]
        public void ASelectionWithPendingTextOpensTheRackBehindWithANoticeAndKeepsTheActiveTab()
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.True(rig.IsOpen("R-2"));
            Assert.True(rig.Tab("R-2")?.IsPreview);
            Assert.Equal("1:75", rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Equal("1:75", rig.Field.Text);
            Assert.Equal("La selección abrió R-2 en segundo plano; la pestaña activa conserva su texto pendiente.", rig.Browser.NoticeText);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));
        }

        [Fact]
        public void ASelectionWithPendingTextReplacesThePreviewThatIsNotActiveInPlace()
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");
            rig.Present(A, SelectionContext.One("R-9"));
            rig.Present(A, SelectionContext.None);

            rig.Present(A, SelectionContext.One("R-8"));

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.False(rig.IsOpen("R-9"));
            Assert.True(rig.IsOpen("R-8"));
            Assert.Equal(2, rig.Browser.Tabs.Count);
            Assert.Equal("La selección abrió R-8 en segundo plano; la pestaña activa conserva su texto pendiente.", rig.Browser.NoticeText);
        }

        [Fact]
        public void ASelectionOfAnOpenRackWithPendingTextDoesNotActivateItAndSaysSoAndAnotherSelectionInBetweenWarnsAgain()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Protected(A, "R-1");
            Assert.Equal("R-1", rig.Browser.ActiveRackId);

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Equal("La selección de R-2 no cambió la pestaña activa, que conserva su texto pendiente.", rig.Browser.NoticeText);

            // The same context again is consumed: nothing opens and it does not warn again.
            rig.Browser.CancelDiscard();
            rig.Present(A, SelectionContext.One("R-2"));
            Assert.Equal(string.Empty, rig.Browser.NoticeText);

            // A real re-selection, with another context in between, warns again.
            rig.Present(A, SelectionContext.None);
            rig.Present(A, SelectionContext.One("R-2"));
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Contains("no cambió la pestaña activa", rig.Browser.NoticeText);
        }

        [Fact]
        public void ARetainedContextIsConsumedAndPresentingTheSameSelectionDoesNotOpenIt()
        {
            var rig = new Rig();
            rig.Protected(A, "R-1");
            rig.Present(A, SelectionContext.One("R-2"));
            Assert.True(rig.IsOpen("R-2"));
            rig.Browser.CloseTab("R-2");
            Assert.False(rig.IsOpen("R-2"));

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.False(rig.IsOpen("R-2"));
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
        }

        [Fact]
        public void AFieldThatThrowsRetainsTheActiveTab()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("no se pudo leer");

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.True(rig.IsOpen("R-2"));
        }

        [Theory]
        [InlineData("Error")]
        [InlineData("Undecidable")]
        public void AnErrorOrAnUndecidableStateWithoutPendingTextDoesNotRetainTheActiveTab(string state)
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(
                state == "Error" ? PilotFieldState.Error : PilotFieldState.Undecidable, isProtected: true, canEdit: false, committedReason: "x"));
            rig.Present(A, SelectionContext.One("R-1"));

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Equal(string.Empty, rig.Browser.NoticeText);
        }

        [Fact]
        public void AnErrorOrAnUndecidableStateWithItsPendingTextRetainsTheActiveTab()
        {
            foreach (var state in new[] { PilotFieldState.Error, PilotFieldState.Undecidable })
            {
                var rig = new Rig();
                rig.Pinned(A, "R-1");
                rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(state, pending: "1:75", isProtected: true, canEdit: false, committedReason: "x"));
                rig.Present(A, SelectionContext.One("R-1"));

                rig.Present(A, SelectionContext.One("R-2"));

                Assert.Equal("R-1", rig.Browser.ActiveRackId);
                Assert.True(rig.IsOpen("R-2"));
            }
        }

        [Fact]
        public void ATabThatIsOnlyDirtyIsNotRetainedTheRackIsActivatedAndThePreviousOneCrossesDematerialize()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, committed: "1:75", isProtected: true));
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Pilot.Calls.Clear();

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            var crossing = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));
            Assert.Equal(PilotBoundaryKind.Dematerialize, crossing.Kind);
            Assert.Equal("R-1", crossing.RackId);
            Assert.Equal(string.Empty, rig.Browser.NoticeText);
        }

        [Fact]
        public void ARetentionOnlyConcernsThePresentedSessionAndATabWithoutPendingTextIsActivatedAsBefore()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Equal(string.Empty, rig.Browser.NoticeText);
        }

        // ------------------------------------------------------------------ without the edit port the browser is the one of before

        [Fact]
        public void WithoutTheEditPortClosingAndPresentingAreTheOnesOfBeforeWithNoCallsAndNoRetention()
        {
            var rig = new Rig(withPilot: false);
            rig.Pinned(A, "R-1");
            rig.Pinned(A, "R-2");

            rig.Browser.CloseTab("R-2");
            rig.Present(A, SelectionContext.One("R-3"));
            rig.Browser.CloseTab("R-3");
            rig.Browser.RequestDocumentDiscard();
            rig.Browser.DiscardField();
            rig.Browser.ConfirmDiscard();

            Assert.Empty(rig.Pilot.Calls);
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(string.Empty, rig.Browser.NoticeText);
            Assert.Empty(rig.Browser.Notices);
            Assert.False(rig.Browser.CanDiscardDocument);
            Assert.False(rig.IsOpen("R-3"));
        }

        // ------------------------------------------------------------------ the double itself

        [Fact]
        public void TheDoubleAnswersKeptWithoutMutatingToABlockingBoundaryOfADraftWithPendingTextThatCannotBeEdited()
        {
            var pilot = new WorkspacePilotEditPortDouble();
            pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, pending: "1:7", isProtected: true, canEdit: false));

            var result = pilot.CrossBoundary(new WorkspaceRequest(A), "R-1", PilotBoundaryKind.TabChange);

            Assert.Equal(PilotBoundaryDecision.Kept, result.Decision);
            Assert.False(result.StateChanged);
            Assert.Equal("1:7", pilot.Current(A, "R-1").PendingText);
        }

        [Fact]
        public void TheDoubleKeepsItsDefaultAnswersAndHonoursItsRespondersQueuesAndThrows()
        {
            var pilot = new WorkspacePilotEditPortDouble();
            var origin = new WorkspaceRequest(A);

            Assert.Equal(EditDiscardStatus.Rejected, pilot.DiscardRack(origin, "R-1").Status);
            Assert.Equal(EditDiscardStatus.Rejected, pilot.DiscardDocument(origin).Status);
            Assert.Empty(pilot.GetRacksAtRisk(origin).RackIds);
            Assert.Empty(pilot.TakeLossNotices());
            Assert.Empty(pilot.TakeCloseNotices());

            pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "x", isProtected: true));
            pilot.Set(A, "R-2", WorkspacePilotEditPortDouble.View());
            pilot.Set(B, "R-3", WorkspacePilotEditPortDouble.View());
            pilot.DiscardAsTheHostDoes();
            var one = pilot.DiscardRack(origin, "R-1");
            Assert.Equal(EditDiscardStatus.Discarded, one.Status);
            Assert.Equal(new[] { "R-1" }, one.RemovedRackIds);
            Assert.Equal(new[] { "R-1" }, one.PendingTextRackIds);
            Assert.False(pilot.Current(A, "R-1").Exists);
            var all = pilot.DiscardDocument(origin);
            Assert.Equal(new[] { "R-2" }, all.RemovedRackIds);
            Assert.Empty(all.PendingTextRackIds);
            Assert.True(pilot.Current(B, "R-3").Exists);

            pilot.LossQueue.Enqueue(new PilotLossView(A, new[] { "R-1" }, Array.Empty<string>(), new Dictionary<string, string>()));
            Assert.Single(pilot.TakeLossNotices());
            Assert.Empty(pilot.TakeLossNotices());
            pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false));
            Assert.Single(pilot.TakeCloseNotices());
            Assert.Empty(pilot.TakeCloseNotices());

            foreach (var operation in new[]
                     {
                         WorkspacePilotEditPortDouble.DiscardRackOp, WorkspacePilotEditPortDouble.DiscardDocumentOp, WorkspacePilotEditPortDouble.RisksOp,
                         WorkspacePilotEditPortDouble.LossOp, WorkspacePilotEditPortDouble.CloseOp,
                     })
                pilot.Throws[operation] = new InvalidOperationException(operation);
            Assert.Throws<InvalidOperationException>(() => pilot.DiscardRack(origin, "R-1"));
            Assert.Throws<InvalidOperationException>(() => pilot.DiscardDocument(origin));
            Assert.Throws<InvalidOperationException>(() => pilot.GetRacksAtRisk(origin));
            Assert.Throws<InvalidOperationException>(() => pilot.TakeLossNotices());
            Assert.Throws<InvalidOperationException>(() => pilot.TakeCloseNotices());
        }

        // ------------------------------------------------------------------ the oracle of the whole task: nothing is written and the intention is never read

        [Fact]
        public void TheWholeCloseAndDiscardScenarioNeverReadsTheIntentionNorWritesNorChecksNorTouchesTheNavigation()
        {
            var (panel, host, navigation, pilot) = PanelWithPilot();
            var browser = panel.Browser;
            pilot.DiscardAsTheHostDoes();
            pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false));
            browser.PinTab("R-1");
            browser.BeginFieldEdit();
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
            panel.Refresh();
            browser.PinTab("R-2");
            pilot.Calls.Clear();

            browser.CloseTab("R-2");
            browser.RequestDocumentDiscard();
            browser.ConfirmDiscard();
            browser.DiscardField();
            browser.CloseTab("R-1");
            panel.Refresh();

            Assert.Empty(navigation.Calls);
            Assert.Empty(pilot.Reads());
            foreach (var never in new[] { WorkspacePilotEditPortDouble.SetPendingOp, WorkspacePilotEditPortDouble.CommitOp, WorkspacePilotEditPortDouble.BoundaryOp })
                Assert.Equal(0, pilot.Count(never));
            var allowed = new HashSet<string>(StringComparer.Ordinal)
            {
                WorkspacePilotEditPortDouble.GetFieldOp,
                WorkspacePilotEditPortDouble.DiscardRackOp,
                WorkspacePilotEditPortDouble.DiscardDocumentOp,
                WorkspacePilotEditPortDouble.RisksOp,
                WorkspacePilotEditPortDouble.LossOp,
                WorkspacePilotEditPortDouble.CloseOp,
            };
            Assert.True(Ops(pilot).IsSubsetOf(allowed), string.Join(",", Ops(pilot).Except(allowed)));
            Assert.Equal(1, pilot.Count(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.False(panel.HasError);
        }

        // ------------------------------------------------------------------ I-64 F4-T3C1: the unknown risk, M-1, M-2, M-3 and M-7

        private const string UnknownRisk = "No se pudo decidir qué borradores están en riesgo en este documento; no se descartó nada.";
        private const string UnknownRiskAgain = "No se pudieron volver a comprobar los borradores en riesgo; no se descartó nada.";

        [Fact]
        public void AnUnknownRiskIsNeverNothingAtRiskAndAsksNothingAndDiscardsNothing()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.RisksResponder = _ => WorkspacePilotEditPortDouble.UnknownRisk();
            rig.Pilot.Calls.Clear();

            rig.Browser.RequestDocumentDiscard();

            Assert.Equal(UnknownRisk, rig.Browser.NoticeText);
            Assert.NotEqual(NoRisk, rig.Browser.NoticeText);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(1, rig.Pilot.Count(WorkspacePilotEditPortDouble.RisksOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.True(rig.Pilot.Current(A, "R-1").Exists);
        }

        [Fact]
        public void AnUnknownRiskRemovesAPendingConfirmationOfTheDocumentAndKeepsTheOneOfATab()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            rig.Browser.RequestDocumentDiscard();
            Assert.True(rig.Browser.HasConfirmation);

            rig.Pilot.RisksResponder = _ => WorkspacePilotEditPortDouble.UnknownRisk();
            rig.Browser.RequestDocumentDiscard();

            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(string.Empty, rig.Browser.ConfirmationText);
            Assert.Equal(UnknownRisk, rig.Browser.NoticeText);

            // The confirmation of a tab is not the one of the document: it stays.
            rig.Protected(A, "R-2");
            rig.Browser.CloseTab("R-2");
            Assert.True(rig.Browser.HasConfirmation);
            rig.Browser.RequestDocumentDiscard();
            Assert.True(rig.Browser.HasConfirmation);
            Assert.Contains("R-2", rig.Browser.ConfirmationText);
            Assert.Equal(UnknownRisk, rig.Browser.NoticeText);
        }

        [Fact]
        public void AnUnknownSecondLookDoesNotDiscardRemovesTheConfirmationAndSaysSo()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.DiscardAsTheHostDoes();
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            rig.Browser.RequestDocumentDiscard();
            Assert.True(rig.Browser.HasConfirmation);
            rig.Pilot.Calls.Clear();

            rig.Pilot.RisksResponder = _ => WorkspacePilotEditPortDouble.UnknownRisk();
            rig.Browser.ConfirmDiscard();

            Assert.Equal(UnknownRiskAgain, rig.Browser.NoticeText);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(1, rig.Pilot.Count(WorkspacePilotEditPortDouble.RisksOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
            Assert.True(rig.Pilot.Current(A, "R-1").Exists);
            Assert.True(rig.IsOpen("R-1"));
        }

        [Fact]
        public void AKnownEmptySecondLookStillSaysThereIsNothingAtRisk()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            rig.Browser.RequestDocumentDiscard();
            rig.Pilot.RisksResponder = _ => Risk(Array.Empty<string>());

            rig.Browser.ConfirmDiscard();

            Assert.Equal(NoRisk, rig.Browser.NoticeText);
            Assert.False(rig.Browser.HasConfirmation);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardDocumentOp));
        }

        [Fact]
        public void WithOnlyUncertainRacksTheConfirmationSaysItCouldNotBeDecidedAndWithBothItKeepsTheCurrentText()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.RisksResponder = _ => Risk(Array.Empty<string>(), new[] { "R-4" });

            rig.Browser.RequestDocumentDiscard();

            Assert.True(rig.Browser.HasConfirmation);
            Assert.Equal("¿Descartar los borradores de este documento? No se pudo decidir si tienen trabajo sin aplicar: R-4.", rig.Browser.ConfirmationText);
            rig.Browser.CancelDiscard();

            rig.Pilot.RisksResponder = _ => Risk(new[] { "R-1" }, new[] { "R-4" });
            rig.Browser.RequestDocumentDiscard();
            Assert.Equal(
                "¿Descartar los borradores de este documento? Se perderá el trabajo sin aplicar de: Nombre R-1. No se pudo decidir: R-4.",
                rig.Browser.ConfirmationText);
        }

        [Fact]
        public void AFieldThatThrowsRetainsTheActiveTabAndTheNoticeSaysItCouldNotBeChecked()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("no se pudo leer");

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.True(rig.IsOpen("R-2"));
            Assert.Equal(
                "La selección abrió R-2 en segundo plano; no se pudo comprobar si la pestaña activa tiene texto pendiente, así que se conserva.",
                rig.Browser.NoticeText);
        }

        [Fact]
        public void AFieldThatThrowsWithTheRackAlreadyOpenKeepsTheActiveTabAndSaysItCouldNotBeChecked()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Pinned(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("no se pudo leer");

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Equal("La selección de R-2 no cambió la pestaña activa: no se pudo comprobar si tiene texto pendiente.", rig.Browser.NoticeText);
        }

        [Fact]
        public void TheClosingConfirmationOfATabThatCouldNotBeReadIsNeutralAndAnyOtherRequestRemovesTheMemoryOfIt()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("no se pudo leer");

            rig.Browser.CloseTab("R-1");

            Assert.True(rig.Browser.HasConfirmation);
            Assert.Equal("No se pudo comprobar la pestaña Nombre R-1; puede tener trabajo sin aplicar. ¿Descartar lo que tenga y cerrarla?", rig.Browser.ConfirmationText);
            Assert.True(rig.IsOpen("R-1"));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));

            // Another request asks again: with a protected tab that can be read, the text is the usual one.
            rig.Browser.CancelDiscard();
            Assert.Equal(string.Empty, rig.Browser.ConfirmationText);
            rig.Pilot.Throws.Clear();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:75", isProtected: true));
            rig.Browser.CloseTab("R-1");
            Assert.Equal("La pestaña Nombre R-1 tiene un borrador con trabajo sin aplicar. ¿Descartarlo y cerrar la pestaña?", rig.Browser.ConfirmationText);
        }

        [Fact]
        public void ACleanTabThatCannotBeDiscardedRemovesTheConfirmationOfAnotherTabBeforeItCloses()
        {
            var rig = new Rig();
            rig.Opened(A, "R-2");
            rig.Protected(A, "R-1");
            rig.Browser.CloseTab("R-1");
            Assert.True(rig.Browser.HasConfirmation);
            Assert.Contains("R-1", rig.Browser.ConfirmationText);
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.DiscardRackOp] = new InvalidOperationException("se cayó el puerto");

            rig.Browser.CloseTab("R-2");

            Assert.False(rig.Browser.HasConfirmation);
            Assert.Equal(string.Empty, rig.Browser.ConfirmationText);
            Assert.False(rig.IsOpen("R-2"));
            Assert.True(rig.IsOpen("R-1"));
            Assert.Contains("Se cerró la pestaña; su borrador limpio quedó en memoria:", rig.Browser.NoticeText);
            Assert.Contains("se cayó el puerto", rig.Browser.NoticeText);
        }

        [Fact]
        public void ABuildOfThePanelWithAVetoInTheQueueAndALivenessCheckThatThrowsDoesNotThrowAndLeavesThePanelInError()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var pilot = new WorkspacePilotEditPortDouble();
            pilot.CloseQueue.Enqueue(new PilotCloseView(A, new[] { "R-1" }, false));
            var summary = new WorkspaceBrowserSummaryPortDouble { ThrowOnAlive = new InvalidOperationException("boom-vida") };
            WorkspacePanelViewModel? panel = null;

            var thrown = Record.Exception(() => panel = new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), summary, pilot));

            Assert.Null(thrown);
            Assert.NotNull(panel);
            Assert.Equal(WorkspacePanelState.Error, panel!.State);
            Assert.True(panel.HasError);
            Assert.Contains("boom-vida", panel.ErrorMessage, StringComparison.Ordinal);

            // The retry recovers the normal presentation once the check works again.
            summary.ThrowOnAlive = null;
            panel.Retry();
            Assert.Equal(WorkspacePanelState.Ready, panel.State);
        }

        [Fact]
        public void ABuildOfThePanelWithoutAVetoInTheQueueAsksNothingAboutTheLivenessAndStaysInItsInitialState()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var summary = new WorkspaceBrowserSummaryPortDouble { ThrowOnAlive = new InvalidOperationException("boom-vida") };

            var panel = new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), summary, new WorkspacePilotEditPortDouble());

            Assert.False(panel.HasError);
            Assert.Equal(WorkspacePanelState.NotStarted, panel.State);
        }

        // M-7: a failure of the port in a gesture of the closing never puts the surface in the Error of ContainFailure.
        [Fact]
        public void AFailureOfThePortInAGestureOfTheClosingNeverPutsTheSurfaceInTheErrorOfTheBrowser()
        {
            const string message = "se cayó el puerto-M7";

            // CloseTab, clean branch: the tab closes (the surface changes legitimately) and the failure is a notice.
            var close = new Rig();
            close.Opened(A, "R-1");
            close.Pilot.Throws[WorkspacePilotEditPortDouble.DiscardRackOp] = new InvalidOperationException(message);
            close.Browser.CloseTab("R-1");
            Assert.DoesNotContain(message, close.Browser.SurfaceStatusText);
            Assert.Contains(message, close.Browser.NoticeText);

            // DiscardField: a draft that is not protected is discarded at once and the port throws.
            var field = new Rig();
            field.Opened(A, "R-1");
            field.Pilot.Throws[WorkspacePilotEditPortDouble.DiscardRackOp] = new InvalidOperationException(message);
            var fieldBefore = field.Browser.SurfaceStatusText;
            field.Browser.DiscardField();
            Assert.DoesNotContain(message, field.Browser.SurfaceStatusText);
            Assert.Equal(fieldBefore, field.Browser.SurfaceStatusText);
            Assert.Contains(message, field.Browser.NoticeText);

            // RequestDocumentDiscard: the read of the racks at risk throws.
            var request = new Rig();
            request.Opened(A, "R-1");
            request.Pilot.Throws[WorkspacePilotEditPortDouble.RisksOp] = new InvalidOperationException(message);
            var requestBefore = request.Browser.SurfaceStatusText;
            request.Browser.RequestDocumentDiscard();
            Assert.DoesNotContain(message, request.Browser.SurfaceStatusText);
            Assert.Equal(requestBefore, request.Browser.SurfaceStatusText);
            Assert.Contains(message, request.Browser.NoticeText);

            // ConfirmDiscard: the second look throws.
            var confirm = new Rig();
            confirm.Opened(A, "R-1");
            confirm.Pilot.RisksResponder = _ => Risk(new[] { "R-1" });
            confirm.Browser.RequestDocumentDiscard();
            Assert.True(confirm.Browser.HasConfirmation);
            confirm.Pilot.Throws[WorkspacePilotEditPortDouble.RisksOp] = new InvalidOperationException(message);
            var confirmBefore = confirm.Browser.SurfaceStatusText;
            confirm.Browser.ConfirmDiscard();
            Assert.DoesNotContain(message, confirm.Browser.SurfaceStatusText);
            Assert.Equal(confirmBefore, confirm.Browser.SurfaceStatusText);
            Assert.Contains(message, confirm.Browser.NoticeText);
            Assert.False(confirm.Browser.HasConfirmation);
        }
    }
}
