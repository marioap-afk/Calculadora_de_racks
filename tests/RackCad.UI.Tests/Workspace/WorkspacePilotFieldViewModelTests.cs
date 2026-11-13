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
    /// I-64 F4-T3B1-EDIT-SURFACE: the field of the bounded pilot and its integration with the browser (INV-F4T3B1-01 to 09). Every test
    /// drives the real view-models over the double of the edit port; no drawing is read and nothing is written. The numbered tests are
    /// the coverage 1-10 and 15-18 of the order of the Owner at the level of the UI (11-14 belong to F4-T3B2); 18, the absence of a
    /// subdialog, is held by the guards of the surface and by the fact that the double has no modal to enter.
    /// </summary>
    public class WorkspacePilotFieldViewModelTests
    {
        private static readonly SessionId A = new(7);
        private static readonly SessionId B = new(8);

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

            // The rack becomes a pinned tab of the session and the active one.
            public void Pinned(SessionId session, string rack)
            {
                Present(session, SelectionContext.One(rack));
                Browser.PinTab(rack);
            }

            // A pinned tab whose draft is clean and was opened by the first gesture.
            public void Opened(SessionId session, string rack)
            {
                Pinned(session, rack);
                Browser.BeginFieldEdit();
            }

            public WorkspaceBrowserTabItem Tab(string rack) => Browser.Tabs.Single(t => t.RackId == rack);
        }

        private static void Is(WorkspacePilotEditPortDouble.Call call, SessionId session, string rack)
        {
            Assert.Equal(session, call.Origin);
            Assert.Equal(rack, call.RackId);
        }

        private static (WorkspacePanelViewModel Panel, WorkspacePortDouble Host, WorkspaceNavigationPortDouble Navigation, WorkspacePilotEditPortDouble Pilot) PanelWithPilot(bool refresh = true)
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var navigation = new WorkspaceNavigationPortDouble();
            var pilot = new WorkspacePilotEditPortDouble();
            var panel = new WorkspacePanelViewModel(host, _ => { }, navigation, new WorkspaceBrowserSummaryPortDouble(), pilot);
            if (refresh) panel.Refresh();
            return (panel, host, navigation, pilot);
        }

        // ------------------------------------------------------------------ the cabling and the absence of the port

        [Fact]
        public void TheFieldIsNeverNullAndWithoutThePortTheBrowserIsTheOneOfBefore()
        {
            var rig = new Rig(withPilot: false);

            Assert.NotNull(rig.Field);
            Assert.False(rig.Field.IsAvailable);

            rig.Pinned(A, "R-1");
            rig.Browser.BeginFieldEdit();
            rig.Browser.ActivateTab("R-1");
            rig.Present(B, SelectionContext.One("R-9"));

            Assert.Empty(rig.Pilot.Calls);
            Assert.DoesNotContain(rig.Browser.Tabs, t => t.Title.EndsWith(" *", StringComparison.Ordinal));
            Assert.False(rig.Field.ShowEditButton);
            Assert.False(rig.Field.IsEditable);
        }

        [Fact]
        public void ThePanelPassesThePilotPortToTheBrowserAndBuildingItCallsNothing()
        {
            var (panel, _, _, pilot) = PanelWithPilot(refresh: false);

            Assert.True(panel.Browser.Field.IsAvailable);
            AssertNoReadAndNoMutation(pilot);

            panel.Refresh();
            panel.Refresh();

            AssertNoReadAndNoMutation(pilot);
        }

        // The presentation and the construction read nothing and mutate nothing. They say nothing about the queues of notices:
        // F4-T3B2 may consume them from the presentation.
        private static void AssertNoReadAndNoMutation(WorkspacePilotEditPortDouble pilot)
        {
            Assert.Empty(pilot.Reads());
            foreach (var mutation in new[]
            {
                WorkspacePilotEditPortDouble.SetPendingOp,
                WorkspacePilotEditPortDouble.CommitOp,
                WorkspacePilotEditPortDouble.DiscardRackOp,
                WorkspacePilotEditPortDouble.DiscardDocumentOp,
            })
                Assert.Equal(0, pilot.Count(mutation));
        }

        // ------------------------------------------------------------------ 1, 2, 3: the text and the commit

        [Fact]
        public void T01_AValidTextAndEnterKeepTheExactTextAndCommitItAndTheFieldShowsTheCommittedOneWithNoPending()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");

            Assert.True(rig.Field.IsEditable);
            Assert.Equal("1:50", rig.Field.Text);
            Assert.Equal("Listo para editar", rig.Field.StateText);

            rig.Field.Text = "1:100";
            rig.Field.CommitPending();

            var typed = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Equal("1:100", typed.Text);
            Assert.Equal(A, typed.Origin);
            Assert.Equal("R-1", typed.RackId);
            var commit = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            Is(commit, A, "R-1");
            Assert.Equal("1:100", rig.Field.Text);
            Assert.Null(rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Equal("1:100", rig.Pilot.Current(A, "R-1").CommittedText);
            Assert.Equal(string.Empty, rig.Field.MessageText);
        }

        [Fact]
        public void T02_AnInvalidTextStaysInTheBoxWithItsMessageAfterTheCommitAnswersInvalid()
        {
            var rig = new Rig();
            rig.Pilot.Validate = t => t == "bad" ? "La escala no es válida." : null;
            rig.Opened(A, "R-1");

            rig.Field.Text = "bad";
            rig.Field.CommitPending();

            Assert.Equal("bad", rig.Field.Text);
            Assert.Equal("La escala no es válida.", rig.Field.MessageText);
            Assert.True(rig.Field.IsEditable);
            Assert.Equal("bad", rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
        }

        [Fact]
        public void T03_AnInvalidTextMakesTheTabShowTheMark()
        {
            var rig = new Rig();
            rig.Pilot.Validate = t => t == "bad" ? "La escala no es válida." : null;
            rig.Opened(A, "R-1");
            Assert.Equal("Nombre R-1", rig.Tab("R-1").Title);

            rig.Field.Text = "bad";

            Assert.Equal("Nombre R-1 *", rig.Tab("R-1").Title);
            Assert.False(rig.Tab("R-1").IsPreview);
        }

        [Fact]
        public void TheMarkFollowsTheProtectionAndTheStripKeepsItsInstanceWhenNothingChanges()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            var before = rig.Browser.Tabs;

            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Same(before, rig.Browser.Tabs);

            rig.Field.Text = "1:75";
            Assert.NotSame(before, rig.Browser.Tabs);
            var marked = rig.Browser.Tabs;
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Same(marked, rig.Browser.Tabs);

            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View());
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Equal("Nombre R-1", rig.Tab("R-1").Title);
        }

        // ------------------------------------------------------------------ 4, 5, 6, 7, 8: the boundaries

        [Fact]
        public void T04_ActivatingAnotherTabWithAnInvalidTextAbortsTheCrossingKeepsTheTabAndTheTextAndShowsANotice()
        {
            var rig = new Rig();
            rig.Pilot.Validate = t => t == "bad" ? "La escala no es válida." : null;
            rig.Pinned(A, "R-2");
            rig.Opened(A, "R-1");
            rig.Field.Text = "bad";
            rig.Pilot.Calls.Clear();

            rig.Browser.ActivateTab("R-2");

            var crossing = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));
            Assert.Equal(PilotBoundaryKind.TabChange, crossing.Kind);
            Is(crossing, A, "R-1");
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Equal("bad", rig.Field.Text);
            Assert.Contains("La escala no es válida.", rig.Field.NoticeText);
            Assert.Equal("bad", rig.Pilot.Current(A, "R-1").PendingText);
        }

        [Fact]
        public void AnAbortWithoutFieldsShowsItsReason()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Opened(A, "R-1");
            rig.Field.Text = "1:75";
            rig.Pilot.BoundaryResponder = (_, _, kind) =>
                new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, Array.Empty<PilotFieldProblem>(), "La petición no es de este documento.");

            rig.Browser.ActivateTab("R-2");

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Contains("La petición no es de este documento.", rig.Field.NoticeText);
            Assert.Equal("1:75", rig.Field.Text);
        }

        [Fact]
        public void ABusyCrossingDoesNotActivateAndKeepsTheText()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Opened(A, "R-1");
            rig.Field.Text = "1:75";
            rig.Pilot.BoundaryResponder = (_, _, kind) =>
                new PilotBoundaryResult(kind, PilotBoundaryDecision.Busy, false, Array.Empty<PilotFieldProblem>(), "Hay una lectura en curso.");

            rig.Browser.ActivateTab("R-2");

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Contains("Hay una lectura en curso.", rig.Field.NoticeText);
            Assert.Equal("1:75", rig.Field.Text);
        }

        [Fact]
        public void T05_ADocumentChangeWithAnInvalidTextProceedsCrossesDocumentChangeOnTheTabOfAAndValidatesNothing()
        {
            var rig = new Rig();
            var validations = 0;
            rig.Pilot.Validate = t =>
            {
                validations++;
                return t == "bad" ? "La escala no es válida." : null;
            };
            rig.Opened(A, "R-1");
            rig.Field.Text = "bad";
            var validationsBefore = validations;
            rig.Pilot.Calls.Clear();

            rig.Present(B, SelectionContext.One("R-9"));

            var crossing = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));
            Assert.Equal(PilotBoundaryKind.DocumentChange, crossing.Kind);
            Is(crossing, A, "R-1");
            Assert.Equal(validationsBefore, validations);
            Assert.Equal("R-9", rig.Browser.ActiveRackId);
            Assert.Equal(new[] { "R-9" }, rig.Browser.Tabs.Select(t => t.RackId).ToArray());
        }

        [Fact]
        public void T05_ActivatingAnotherTabFromADraftThatCannotBeEditedWithPendingTextKeepsAndTheChangeProceeds()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, pending: "1:75", isProtected: true, canEdit: false));
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.False(rig.Field.IsEditable);
            rig.Pilot.BoundaryResponder = (_, _, kind) =>
                new PilotBoundaryResult(kind, PilotBoundaryDecision.Kept, false, Array.Empty<PilotFieldProblem>(), null);
            rig.Pilot.Calls.Clear();

            rig.Browser.ActivateTab("R-2");

            var crossing = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));
            Assert.Equal(PilotBoundaryKind.TabChange, crossing.Kind);
            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Equal("1:75", rig.Pilot.Current(A, "R-1").PendingText);
        }

        [Fact]
        public void T06_ComingBackToTheTabShowsTheExactTextOfTheDraftAfterAProceededCrossingAndAfterAKeptOne()
        {
            // A valid pending text proceeds: the host applies it and clears the pending text, so the tab shows the committed one again.
            var rig = new Rig();
            rig.Pilot.Set(A, "R-2", WorkspacePilotEditPortDouble.View());
            rig.Pinned(A, "R-2");
            rig.Opened(A, "R-1");
            rig.Field.Text = " 1:7 ";
            Assert.Equal(" 1:7 ", rig.Pilot.Current(A, "R-1").PendingText);

            rig.Browser.ActivateTab("R-2");

            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Equal("1:50", rig.Field.Text);
            Assert.Null(rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Equal(" 1:7 ", rig.Pilot.Current(A, "R-1").CommittedText);

            rig.Browser.ActivateTab("R-1");

            Assert.Equal(" 1:7 ", rig.Field.Text);
            Assert.True(rig.Field.IsEditable);
            Assert.Equal("Nombre R-1 *", rig.Tab("R-1").Title);
            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));

            // A draft that cannot be edited keeps its pending text through a Kept crossing, and the tab shows it exactly when it comes back.
            var kept = new Rig();
            kept.Pilot.Set(A, "R-2", WorkspacePilotEditPortDouble.View());
            kept.Pinned(A, "R-2");
            kept.Pinned(A, "R-1");
            kept.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, pending: " 1:7 ", isProtected: true, canEdit: false));
            kept.Present(A, SelectionContext.One("R-1"));
            kept.Browser.ActivateTab("R-2");
            Assert.Equal("R-2", kept.Browser.ActiveRackId);
            Assert.Equal("1:50", kept.Field.Text);

            kept.Browser.ActivateTab("R-1");

            Assert.Equal(" 1:7 ", kept.Field.Text);
            Assert.Empty(kept.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
        }

        [Fact]
        public void T07_AfterADocumentChangeThePendingTextOfAStaysInA()
        {
            var rig = new Rig();
            rig.Pilot.Validate = t => t == "bad" ? "La escala no es válida." : null;
            rig.Opened(A, "R-1");
            rig.Field.Text = "bad";
            rig.Pilot.Calls.Clear();

            rig.Present(B, SelectionContext.One("R-9"));

            Assert.Equal("bad", rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Null(rig.Pilot.Current(B, "R-9").PendingText);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.DiscardRackOp));
        }

        [Fact]
        public void T08_ComingBackToDocumentAShowsThePendingTextOfA()
        {
            var rig = new Rig();
            rig.Pilot.Validate = t => t == "bad" ? "La escala no es válida." : null;
            rig.Opened(A, "R-1");
            rig.Field.Text = "bad";
            rig.Present(B, SelectionContext.One("R-9"));
            Assert.NotEqual("bad", rig.Field.Text);

            rig.Present(A, SelectionContext.None);

            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.Equal("bad", rig.Field.Text);
            Assert.Equal("La escala no es válida.", rig.Field.MessageText);
            Assert.Equal("Nombre R-1 *", rig.Tab("R-1").Title);
        }

        // ------------------------------------------------------------------ 9, 10: the first gesture

        [Fact]
        public void T09_TheFirstGesturePinsThePreviewBeforeAskingForTheIntentionWithTheEditButtonAndWithTheDoubleClick()
        {
            foreach (var gesture in new Action<Rig>[] { r => r.Browser.BeginFieldEdit(), r => r.Browser.BeginFieldEditFromTab("R-1") })
            {
                var rig = new Rig();
                rig.Present(A, SelectionContext.One("R-1"));
                Assert.True(rig.Tab("R-1").IsPreview);
                bool? previewInsideTheRead = null;
                rig.Pilot.ReadHook = _ => previewInsideTheRead = rig.Tab("R-1").IsPreview;

                gesture(rig);

                Assert.False(previewInsideTheRead);
                var request = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
                Is(request, A, "R-1");
                Assert.False(rig.Tab("R-1").IsPreview);
                Assert.True(rig.Field.IsEditable);
            }
        }

        [Fact]
        public void TheIntentionTravelsWithoutAWitnessOrAChosenReference()
        {
            var rig = new Rig();
            EditIntentRequest? seen = null;
            rig.Pilot.BeginResponder = request =>
            {
                seen = request;
                return EditIntentOutcome.Admitted(EditWitnessStatus.NotSupplied);
            };
            rig.Present(A, SelectionContext.One("R-1"));

            rig.Browser.BeginFieldEdit();

            Assert.NotNull(seen);
            Assert.Equal(new EditIntentRequest(A, "R-1", null, null), seen);
        }

        [Fact]
        public void T10_ATabWithADraftIsNeverEditableWhileItIsAPreviewAndTheGestureFixesItAndTheDraftIsAlreadyOpen()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View());
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.True(rig.Tab("R-1").IsPreview);

            Assert.False(rig.Field.IsEditable);
            Assert.True(rig.Field.ShowEditButton);
            rig.Field.Text = "x";
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Equal("1:50", rig.Field.Text);

            rig.Browser.BeginFieldEdit();

            Assert.False(rig.Tab("R-1").IsPreview);
            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
            Assert.True(rig.Field.IsEditable);
            Assert.Equal("Listo para editar", rig.Field.StateText);
        }

        [Fact]
        public void T10_AClosedTabOfACleanDraftThatComesBackAsAPreviewIsReadOnlyUntilTheFirstGesture()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Browser.CloseTab("R-1");
            Assert.Empty(rig.Browser.Tabs);

            rig.Present(A, SelectionContext.One("R-2"));
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.True(rig.Tab("R-1").IsPreview);
            Assert.False(rig.Field.IsEditable);
            rig.Field.Text = "typed";
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));

            rig.Pilot.Calls.Clear();
            rig.Browser.BeginFieldEdit();

            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
            Assert.True(rig.Field.IsEditable);
        }

        [Fact]
        public void T10_APreviewWithAProtectedDraftIsPinnedWhenTheStripIsRebuiltAndTheFieldIsAlreadyEditable()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-2", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:75", isProtected: true));

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.False(rig.Tab("R-2").IsPreview);
            Assert.Equal("Nombre R-2 *", rig.Tab("R-2").Title);
            Assert.True(rig.Field.IsEditable);
            Assert.Equal("1:75", rig.Field.Text);
            Assert.Empty(rig.Pilot.Reads());
        }

        [Fact]
        public void APinnedTabIsNoLongerTreatedAsAPreviewInTheSameRebuild()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View());
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.False(rig.Field.IsEditable);

            rig.Browser.PinTab("R-1");

            Assert.True(rig.Field.IsEditable);
            Assert.False(rig.Field.ShowEditButton);
        }

        // ------------------------------------------------------------------ the results of the first gesture

        [Fact]
        public void AdmittedAndAlreadyOpenRenderTheFieldFromTheDraftAndTheGestureDoesNotApplyAnything()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            Assert.Equal("Sin borrador.", rig.Field.StateText);
            Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, rig.Field.Text);
            Assert.True(rig.Field.ShowEditButton);
            Assert.False(rig.Field.IsEditable);

            rig.Browser.BeginFieldEdit();

            Assert.Equal("1:50", rig.Field.Text);
            Assert.True(rig.Field.IsEditable);
            Assert.False(rig.Field.ShowEditButton);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
        }

        [Theory]
        [InlineData(PilotFieldState.Applying)]
        [InlineData(PilotFieldState.Unknown)]
        [InlineData(PilotFieldState.Undecidable)]
        public void AlreadyOpenOnADraftThatCannotBeEditedShowsItsStateAndNotTheReadyNote(PilotFieldState state)
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(state, pending: "1:75", isProtected: true, canEdit: false));

            rig.Browser.BeginFieldEdit();

            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
            Assert.False(rig.Field.IsEditable);
            Assert.DoesNotContain("Listo para editar", rig.Field.StateText);
            Assert.Equal("1:75", rig.Field.Text);
            Assert.False(rig.Field.ShowEditButton);
        }

        [Fact]
        public void WaitingUnavailableRejectedAndFailedLeaveANoticeWithTheirReasonAndChangeNothing()
        {
            var outcomes = new (string Reason, Func<string, EditIntentOutcome> Make)[]
            {
                ("Hay un comando en curso.", EditIntentOutcome.Waiting),
                ("No hay lector.", EditIntentOutcome.Unavailable),
                ("La petición no es de este documento.", reason => EditIntentOutcome.Rejected(reason)),
                ("Falló el lector.", EditIntentOutcome.Failed),
            };

            foreach (var (reason, make) in outcomes)
            {
                var rig = new Rig();
                rig.Pilot.BeginResponder = _ => make(reason);
                rig.Pinned(A, "R-1");

                rig.Browser.BeginFieldEdit();

                Assert.Contains(reason, rig.Field.NoticeText);
                Assert.False(rig.Field.IsEditable);
                Assert.True(rig.Field.ShowEditButton);
                Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, rig.Field.Text);
                Assert.Equal(string.Empty, rig.Field.NotEditableText);
            }
        }

        [Fact]
        public void NotAdmittedShowsNotEditableHereWithTheDiagnosticAndTheHintAndKeepsThemUntilANewGestureOrAChangeOfSurface()
        {
            var rig = new Rig();
            rig.Pilot.BeginResponder = _ => EditIntentOutcome.NotAdmitted(
                "No editable aquí: el rack tiene una definición propia.", "detalle técnico", "Usa RACKEDITAR para editarlo.", EditWitnessStatus.NotSupplied);
            rig.Pinned(A, "R-2");
            rig.Pinned(A, "R-1");

            rig.Browser.BeginFieldEdit();

            Assert.Equal("No editable aquí", rig.Field.StateText);
            Assert.Equal("No editable aquí: el rack tiene una definición propia.", rig.Field.NotEditableText);
            Assert.Equal("Usa RACKEDITAR para editarlo.", rig.Field.ClassicEditorHintText);
            Assert.False(rig.Field.IsEditable);
            Assert.DoesNotContain("detalle técnico", rig.Field.NotEditableText);

            // The presentation shows the field again but keeps the diagnostic of the same surface.
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Equal("No editable aquí: el rack tiene una definición propia.", rig.Field.NotEditableText);
            Assert.Equal("Usa RACKEDITAR para editarlo.", rig.Field.ClassicEditorHintText);
            Assert.Equal("No editable aquí", rig.Field.StateText);

            // A change of surface ends it.
            rig.Browser.ActivateTab("R-2");
            Assert.Equal(string.Empty, rig.Field.NotEditableText);
            Assert.Equal(string.Empty, rig.Field.ClassicEditorHintText);

            // So does a new gesture.
            rig.Browser.ActivateTab("R-1");
            Assert.Equal(string.Empty, rig.Field.NotEditableText);
            rig.Browser.BeginFieldEdit();
            Assert.Equal("No editable aquí", rig.Field.StateText);
            rig.Pilot.BeginResponder = null;
            rig.Browser.BeginFieldEdit();
            Assert.Equal(string.Empty, rig.Field.NotEditableText);
            Assert.True(rig.Field.IsEditable);
        }

        // ------------------------------------------------------------------ the lazy reload (O-01)

        [Fact]
        public void ACleanDraftThatIsPossiblyStaleIsReadOnlyTypingDoesNotCallSetPendingTextAndTheGestureCallsTheReload()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(stale: true));
            rig.Pinned(A, "R-1");

            Assert.False(rig.Field.IsEditable);
            Assert.True(rig.Field.ShowEditButton);
            Assert.Contains("posiblemente obsoleto", rig.Field.StateText);
            rig.Field.Text = "x";
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Equal("1:50", rig.Field.Text);

            rig.Browser.BeginFieldEdit();

            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.ReloadOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.BeginOp));
            Assert.True(rig.Field.IsEditable);
            Assert.DoesNotContain("obsoleto", rig.Field.StateText);
        }

        [Fact]
        public void TheStaleMarkThatArrivesWithTheSurfaceAliveMakesTheBoxReadOnlyAtTheNextPresentation()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            Assert.True(rig.Field.IsEditable);

            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(stale: true));
            rig.Present(A, SelectionContext.One("R-1"));

            Assert.False(rig.Field.IsEditable);
            Assert.Contains("posiblemente obsoleto", rig.Field.StateText);
            Assert.Empty(rig.Pilot.Reads().Skip(1));
        }

        [Fact]
        public void AReloadThatIsNotAdmittedKeepsTheDraftAsItWasWithTheNoticeAndTheDiagnostic()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(stale: true));
            rig.Pilot.ReloadResponder = _ => EditIntentOutcome.NotAdmitted(
                "No editable aquí: cambió el rack.", null, "Usa RACKEDITAR.", EditWitnessStatus.NotSupplied);
            rig.Pinned(A, "R-1");

            rig.Browser.BeginFieldEdit();

            Assert.False(rig.Field.IsEditable);
            Assert.Equal("1:50", rig.Field.Text);
            Assert.True(rig.Pilot.Current(A, "R-1").IsPossiblyStale);
            Assert.Equal("No editable aquí: cambió el rack.", rig.Field.NotEditableText);
            Assert.Equal("Usa RACKEDITAR.", rig.Field.ClassicEditorHintText);
            Assert.NotEqual(string.Empty, rig.Field.NoticeText);
            Assert.Contains("posiblemente obsoleto", rig.Field.StateText);
        }

        [Fact]
        public void AReloadThatWaitsLeavesTheNoticeAndTheStaleDraft()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(stale: true));
            rig.Pilot.ReloadResponder = _ => EditIntentOutcome.Waiting("Hay un comando en curso.");
            rig.Pinned(A, "R-1");

            rig.Browser.BeginFieldEdit();

            Assert.Contains("Hay un comando en curso.", rig.Field.NoticeText);
            Assert.False(rig.Field.IsEditable);
        }

        // ------------------------------------------------------------------ the reentry of a gesture

        [Fact]
        public void AReadThatChangesTheTabAndTheDocumentInsideItDoesNotRenderItsResultOnTheNewSurface()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Pinned(A, "R-1");
            rig.Pilot.ReadHook = _ =>
            {
                rig.Browser.ActivateTab("R-2");
                rig.Present(B, SelectionContext.One("R-9"));
            };

            rig.Browser.BeginFieldEdit();

            // The draft of A was opened by the read, but the surface is now R-9 of B and it shows its own state.
            Assert.NotNull(rig.Pilot.Current(A, "R-1").CommittedText);
            Assert.Equal("R-9", rig.Browser.ActiveRackId);
            Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, rig.Field.Text);
            Assert.False(rig.Field.IsEditable);
            Assert.DoesNotContain("Listo para editar", rig.Field.StateText);
            Assert.NotEqual(string.Empty, rig.Field.NoticeText);
        }

        [Fact]
        public void AReadThatOnlyChangesTheTabInsideTheSameSessionDoesNotRenderItsResultOnTheOtherRack()
        {
            // The session is the same before and after the read: only the rack tells the surface apart, so a check of identity that
            // looked at the session alone would render the result of R-1 on R-2.
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Pinned(A, "R-1");
            rig.Pilot.ReadHook = _ => rig.Browser.ActivateTab("R-2");

            rig.Browser.BeginFieldEdit();

            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.NotNull(rig.Pilot.Current(A, "R-1").CommittedText);
            Assert.False(rig.Pilot.Current(A, "R-2").Exists);
            Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, rig.Field.Text);
            Assert.False(rig.Field.IsEditable);
            Assert.DoesNotContain("Listo para editar", rig.Field.StateText);
            Assert.NotEqual(string.Empty, rig.Field.NoticeText);
        }

        // ------------------------------------------------------------------ TabChange

        [Fact]
        public void AnotherClickOnTheActiveTabCallsNothingAndAChangeFromACleanDraftDoesNotCrossTheBoundary()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-2");
            rig.Opened(A, "R-1");
            rig.Pilot.Calls.Clear();

            rig.Browser.ActivateTab("R-1");
            Assert.Empty(rig.Pilot.Calls);

            rig.Browser.ActivateTab("R-2");
            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));
        }

        [Fact]
        public void ActivatingATabThatDoesNotExistCallsNoBoundary()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Field.Text = "1:75";
            rig.Pilot.Calls.Clear();

            rig.Browser.ActivateTab("R-404");

            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
        }

        [Fact]
        public void AValidPendingTextProceedsWhenAnotherTabIsActivated()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-2", WorkspacePilotEditPortDouble.View());
            rig.Pinned(A, "R-2");
            rig.Opened(A, "R-1");
            rig.Field.Text = "1:75";

            rig.Browser.ActivateTab("R-2");

            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Equal("1:50", rig.Field.Text);
            Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp), c => c.Kind == PilotBoundaryKind.TabChange);
        }

        [Fact]
        public void AnAbortedActivationDoesNotPinNorAskForTheIntentionWithTheDoubleClickOfThatTab()
        {
            var rig = new Rig();
            rig.Pilot.Validate = t => t == "bad" ? "La escala no es válida." : null;
            rig.Opened(A, "R-1");

            // R-2 is a preview tab that is not the active one: the selection opened it and the user went back to R-1.
            rig.Present(A, SelectionContext.One("R-2"));
            rig.Browser.ActivateTab("R-1");
            Assert.True(rig.Tab("R-2").IsPreview);
            rig.Field.Text = "bad";
            rig.Pilot.Calls.Clear();

            rig.Browser.ActivateTab("R-2");
            rig.Browser.BeginFieldEditFromTab("R-2");

            Assert.Empty(rig.Pilot.Reads());
            Assert.Equal("R-1", rig.Browser.ActiveRackId);
            Assert.True(rig.Tab("R-2").IsPreview);
        }

        // ------------------------------------------------------------------ Dematerialize, only in the presentation

        [Fact]
        public void ASelectionThatActivatesAnotherRackCrossesDematerializeOnThePreviousTabWhenItHadADraftWithNoPending()
        {
            // The previous tab is only dirty, with no pending text: the case that F4-T3B2 keeps (R-B6) and that T3B1 crosses as an observed boundary.
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, committed: "1:75", isProtected: true));
            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Null(rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Equal("Nombre R-1 *", rig.Tab("R-1").Title);
            rig.Pilot.Calls.Clear();

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            var crossing = Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));
            Assert.Equal(PilotBoundaryKind.Dematerialize, crossing.Kind);
            Is(crossing, A, "R-1");
            Assert.DoesNotContain(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp), c => c.Kind == PilotBoundaryKind.TabChange);
        }

        [Fact]
        public void ASelectionThatLeavesATabWithoutADraftCrossesNothingAndATabChangeNeverCrossesDematerialize()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Pilot.Calls.Clear();

            rig.Present(A, SelectionContext.One("R-3"));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp));

            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View());
            rig.Browser.ActivateTab("R-1");
            Assert.DoesNotContain(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp), c => c.Kind == PilotBoundaryKind.Dematerialize);
            rig.Browser.CloseTab("R-1");
            Assert.DoesNotContain(rig.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp), c => c.Kind == PilotBoundaryKind.Dematerialize);
        }

        // ------------------------------------------------------------------ the notice is kept by the rendering

        [Fact]
        public void TheRenderingKeepsTheNoticeOfTheSameSurfaceUntilANewGesture()
        {
            var rig = new Rig();
            rig.Pilot.BeginResponder = _ => EditIntentOutcome.Waiting("Hay un comando en curso.");
            rig.Pinned(A, "R-1");
            rig.Browser.BeginFieldEdit();
            var notice = rig.Field.NoticeText;
            Assert.Contains("Hay un comando en curso.", notice);

            rig.Present(A, SelectionContext.One("R-1"));
            rig.Browser.ReloadSummary();

            Assert.Equal(notice, rig.Field.NoticeText);
            rig.Pilot.BeginResponder = null;
            rig.Browser.BeginFieldEdit();
            Assert.Equal(string.Empty, rig.Field.NoticeText);
        }

        [Fact]
        public void ACommittedTextThatIsNullShowsAnEmptyBoxAndTheReasonInTheState()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(committed: null, committedReason: "El rack no tiene escala de anotación."));
            rig.Pinned(A, "R-1");

            Assert.Equal(string.Empty, rig.Field.Text);
            Assert.Contains("El rack no tiene escala de anotación.", rig.Field.StateText);
            Assert.True(rig.Field.IsEditable);
        }

        // ------------------------------------------------------------------ the setter

        [Fact]
        public void TheSetterOfAFieldThatIsNotEditableDoesNothingAndRestoresTheText()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            var changed = new List<string?>();
            rig.Field.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            rig.Field.Text = "typed";

            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, rig.Field.Text);
            Assert.Contains(nameof(WorkspacePilotFieldViewModel.Text), changed);
        }

        [Fact]
        public void AnExceptionInsideTheSetterStaysInTheErrorOfTheFieldWithTheBoxRestoredAndThePanelDoesNotFail()
        {
            var (panel, _, _, pilot) = PanelWithPilot();
            var field = panel.Browser.Field;
            panel.Browser.PinTab("R-1");
            panel.Browser.BeginFieldEdit();
            Assert.True(field.IsEditable);
            pilot.Throws[WorkspacePilotEditPortDouble.SetPendingOp] = new InvalidOperationException("falló el puerto");

            field.Text = "1:75";

            Assert.Equal("1:50", field.Text);
            Assert.False(field.IsEditable);
            Assert.Contains("falló el puerto", field.StateText);
            Assert.True(field.ShowReloadButton);
            Assert.NotEqual(WorkspacePanelState.Error, panel.State);
            Assert.False(panel.HasError);
        }

        [Fact]
        public void ARejectedNotEditableOrBusyTextRestoresTheBoxFromTheDraftAndShowsANotice()
        {
            var answers = new Func<PilotFieldResult>[]
            {
                () => PilotFieldResult.Rejected("La petición no es de este documento."),
                () => PilotFieldResult.NotEditable("El campo no se puede editar ahora.", WorkspacePilotEditPortDouble.NoDraft()),
                () => PilotFieldResult.Busy("Hay una lectura en curso."),
            };
            var reasons = new[] { "La petición no es de este documento.", "El campo no se puede editar ahora.", "Hay una lectura en curso." };

            for (var i = 0; i < answers.Length; i++)
            {
                var answer = answers[i];
                var rig = new Rig();
                rig.Opened(A, "R-1");
                rig.Pilot.SetPendingResponder = (_, _, _) => answer();

                rig.Field.Text = "1:75";

                Assert.Equal("1:50", rig.Field.Text);
                Assert.Contains(reasons[i], rig.Field.NoticeText);
                Assert.Single(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            }
        }

        [Fact]
        public void ShowAndPresentNeverCallSetPendingTextEvenWithAPendingText()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:75", isProtected: true));
            rig.Pinned(A, "R-1");

            rig.Field.Show(A, "R-1", false);
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Present(B, SelectionContext.One("R-9"));
            rig.Present(A, SelectionContext.One("R-1"));

            Assert.Equal("1:75", rig.Field.Text);
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
        }

        // ------------------------------------------------------------------ CommitField

        [Fact]
        public void CommitFieldIsNotCalledWithoutPendingTextAndALostFocusAfterAWaitingGestureDoesNotOverwriteTheNotice()
        {
            var rig = new Rig();
            rig.Pilot.BeginResponder = _ => EditIntentOutcome.Waiting("Hay un comando en curso.");
            rig.Pinned(A, "R-1");
            rig.Browser.BeginFieldEdit();
            var notice = rig.Field.NoticeText;

            rig.Field.CommitPending();

            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            Assert.Equal(notice, rig.Field.NoticeText);

            rig.Pilot.BeginResponder = null;
            rig.Browser.BeginFieldEdit();
            rig.Field.CommitPending();
            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
        }

        [Fact]
        public void CommitFieldIsNotCalledWithPendingTextOnADraftThatCannotBeEdited()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, pending: "1:75", isProtected: true, canEdit: false));
            rig.Pinned(A, "R-1");

            rig.Field.CommitPending();

            Assert.Empty(rig.Pilot.Of(WorkspacePilotEditPortDouble.CommitOp));
            Assert.Equal("1:75", rig.Field.Text);
        }

        [Fact]
        public void ACommitThatIsRefusedLeavesTheTextAndANotice()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Field.Text = "1:75";
            rig.Pilot.CommitResponder = (_, _) => PilotFieldResult.Busy("Hay una lectura en curso.");

            rig.Field.CommitPending();

            Assert.Equal("1:75", rig.Field.Text);
            Assert.Contains("Hay una lectura en curso.", rig.Field.NoticeText);
        }

        // ------------------------------------------------------------------ the states and the containment

        [Theory]
        [InlineData(PilotFieldState.Clean, true)]
        [InlineData(PilotFieldState.Dirty, true)]
        [InlineData(PilotFieldState.Conflict, true)]
        public void CleanDirtyAndConflictWithTheRightToEditOnAPinnedTabGiveAnEditableBox(PilotFieldState state, bool editable)
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(state, isProtected: state != PilotFieldState.Clean));
            rig.Pinned(A, "R-1");

            Assert.Equal(editable, rig.Field.IsEditable);
            Assert.Equal(!editable, rig.Field.IsReadOnly);
        }

        [Theory]
        [InlineData(PilotFieldState.Applying, "Aplicando")]
        [InlineData(PilotFieldState.Unknown, "Desconocido")]
        [InlineData(PilotFieldState.Undecidable, "Sin decidir")]
        public void ApplyingUnknownAndUndecidableLeaveTheBoxReadOnlyWithTheirText(PilotFieldState state, string name)
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(state, pending: "1:75", isProtected: true, canEdit: false));
            rig.Pinned(A, "R-1");

            Assert.False(rig.Field.IsEditable);
            Assert.Equal("1:75", rig.Field.Text);
            Assert.Contains(name, rig.Field.StateText);
        }

        [Fact]
        public void AnErrorFromTheDraftLeavesTheBoxReadOnlyWithTheReasonAndTheReloadButtonThatReadsNothing()
        {
            var (panel, _, _, pilot) = PanelWithPilot();
            var field = panel.Browser.Field;
            pilot.Set(new SessionId(7), "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Error, canEdit: false, isProtected: true, committedReason: "No se pudo leer el estado."));
            panel.Browser.PinTab("R-1");

            Assert.False(field.IsEditable);
            Assert.Contains("No se pudo leer el estado.", field.StateText);
            Assert.True(field.ShowReloadButton);
            Assert.False(field.ShowEditButton);

            pilot.Set(new SessionId(7), "R-1", WorkspacePilotEditPortDouble.View());
            panel.Browser.ReloadField();

            Assert.Empty(pilot.Reads());
            Assert.True(field.IsEditable);
            Assert.False(field.ShowReloadButton);
            Assert.NotEqual(WorkspacePanelState.Error, panel.State);
        }

        [Fact]
        public void AFailureOfTheViewModelIsContainedInTheFieldAndTheReloadButtonRecoversWithoutAnyRead()
        {
            var (panel, _, _, pilot) = PanelWithPilot();
            var field = panel.Browser.Field;
            panel.Browser.PinTab("R-1");
            panel.Browser.BeginFieldEdit();
            field.Text = "1:75";
            pilot.Throws[WorkspacePilotEditPortDouble.CommitOp] = new InvalidOperationException("falló el compromiso");

            field.CommitPending();

            Assert.False(field.IsEditable);
            Assert.Contains("falló el compromiso", field.StateText);
            Assert.True(field.ShowReloadButton);
            Assert.False(panel.HasError);

            pilot.Throws.Clear();
            pilot.Calls.Clear();
            panel.Browser.ReloadField();

            Assert.Empty(pilot.Reads());
            Assert.True(field.IsEditable);
            Assert.Equal("1:75", field.Text);
            Assert.False(panel.HasError);
        }

        [Fact]
        public void AFailureOfTheFirstGestureStaysInTheFieldAndNeverBecomesAPanelError()
        {
            var (panel, _, _, pilot) = PanelWithPilot();
            panel.Browser.PinTab("R-1");
            pilot.Throws[WorkspacePilotEditPortDouble.BeginOp] = new InvalidOperationException("falló la lectura");

            panel.Browser.BeginFieldEdit();

            Assert.Contains("falló la lectura", panel.Browser.Field.StateText);
            Assert.True(panel.Browser.Field.ShowReloadButton);
            Assert.False(panel.HasError);
            Assert.Equal(WorkspacePanelState.Ready, panel.State);
        }

        [Fact]
        public void PresentAndRefreshNeverCallTheThreeReads()
        {
            var (panel, host, _, pilot) = PanelWithPilot();
            pilot.Set(new SessionId(7), "R-1", WorkspacePilotEditPortDouble.View(stale: true));
            panel.Refresh();
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
            panel.Refresh();
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 8, selection: SelectionContext.One("R-1"));
            panel.Refresh();
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1"));
            panel.Refresh();
            panel.Retry();

            Assert.Empty(pilot.Reads());
        }

        // ------------------------------------------------------------------ 15, 16, 17: the oracle, K = 1 and isolation

        [Fact]
        public void T15_ChangingTabsCallsNoNavigationAndNeverDiscardsNorChecksNorReadsTheRisks()
        {
            var (panel, host, navigation, pilot) = PanelWithPilot();
            var browser = panel.Browser;
            browser.PinTab("R-1");
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
            panel.Refresh();
            browser.PinTab("R-2");
            browser.BeginFieldEdit();
            browser.Field.Text = "1:75";
            browser.Field.CommitPending();
            browser.ActivateTab("R-1");
            browser.BeginFieldEdit();
            browser.Field.Text = "1:100";
            browser.ActivateTab("R-2");
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 8, selection: SelectionContext.One("R-9"));
            panel.Refresh();
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
            panel.Refresh();

            Assert.Empty(navigation.Calls);
            // F4-T3B1 reads and mutates only through the six operations below; it never discards, never checks and never asks for the
            // racks at risk. Nothing is said about the queues of notices (TakeLossNotices, TakeCloseNotices): F4-T3B2 may consume them
            // from the presentation.
            var allowedOrNotices = new HashSet<string>(StringComparer.Ordinal)
            {
                WorkspacePilotEditPortDouble.GetFieldOp,
                WorkspacePilotEditPortDouble.SetPendingOp,
                WorkspacePilotEditPortDouble.CommitOp,
                WorkspacePilotEditPortDouble.BoundaryOp,
                WorkspacePilotEditPortDouble.BeginOp,
                WorkspacePilotEditPortDouble.ReloadOp,
                WorkspacePilotEditPortDouble.LossOp,
                WorkspacePilotEditPortDouble.CloseOp,
            };
            Assert.True(pilot.Operations().IsSubsetOf(allowedOrNotices), string.Join(",", pilot.Operations().Except(allowedOrNotices)));
            foreach (var never in new[]
            {
                WorkspacePilotEditPortDouble.DiscardRackOp,
                WorkspacePilotEditPortDouble.DiscardDocumentOp,
                WorkspacePilotEditPortDouble.RisksOp,
                WorkspacePilotEditPortDouble.CheckOp,
            })
                Assert.Equal(0, pilot.Count(never));
        }

        [Fact]
        public void T16_ThereIsOneSurfaceAndOneFieldAndTheFieldFollowsTheActiveTab()
        {
            var rig = new Rig();
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(committed: "1:50"));
            rig.Pilot.Set(A, "R-2", WorkspacePilotEditPortDouble.View(committed: "1:75"));
            rig.Pinned(A, "R-1");
            rig.Pinned(A, "R-2");
            var field = rig.Browser.Field;

            Assert.Equal(1, rig.Browser.LiveSurfaceCount);
            Assert.Equal("1:75", field.Text);

            rig.Browser.ActivateTab("R-1");

            Assert.Same(field, rig.Browser.Field);
            Assert.Equal(1, rig.Browser.LiveSurfaceCount);
            Assert.Equal("1:50", field.Text);

            // K = 1 while the user edits: the typing goes to the draft of the live tab and the surface is still the one.
            field.Text = "1:60";
            Assert.Equal(1, rig.Browser.LiveSurfaceCount);
            Assert.Same(field, rig.Browser.Field);
            Assert.Equal("1:60", rig.Pilot.Current(A, "R-1").PendingText);
            Assert.Equal("1:75", rig.Pilot.Current(A, "R-2").CommittedText);

            rig.Browser.ActivateTab("R-2");

            Assert.Equal(1, rig.Browser.LiveSurfaceCount);
            Assert.Same(field, rig.Browser.Field);
            Assert.Equal("1:75", field.Text);

            rig.Browser.ActivateTab("R-1");

            Assert.Equal(1, rig.Browser.LiveSurfaceCount);
            Assert.Equal("1:60", field.Text);
        }

        // ------------------------------------------------------------------ the gestures depend on the state (minor 4 of the independent review)

        [Fact]
        public void TheDoubleClickOnTheActiveTabWithAnEditableBoxDoesNothingAndKeepsTheNoticeOfTheSurface()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.SetPendingResponder = (_, _, _) => PilotFieldResult.Busy("Hay una lectura en curso.");
            rig.Field.Text = "1:75";
            var notice = rig.Field.NoticeText;
            Assert.Contains("Hay una lectura en curso.", notice);
            Assert.True(rig.Field.IsEditable);
            rig.Pilot.Calls.Clear();

            rig.Browser.BeginFieldEditFromTab("R-1");

            Assert.Empty(rig.Pilot.Reads());
            Assert.Equal(notice, rig.Field.NoticeText);
        }

        [Fact]
        public void WithANotAdmittedDiagnosticOfTheSameSurfaceTheDoubleClickDoesNotReadAndOnlyTheEditButtonReadsAgain()
        {
            var rig = new Rig();
            rig.Pilot.BeginResponder = _ => EditIntentOutcome.NotAdmitted(
                "No editable aquí: el rack tiene una definición propia.", "detalle técnico", "Usa RACKEDITAR para editarlo.", EditWitnessStatus.NotSupplied);
            rig.Pinned(A, "R-1");
            rig.Browser.BeginFieldEdit();
            Assert.Equal("No editable aquí", rig.Field.StateText);
            Assert.True(rig.Field.ShowEditButton);
            Assert.Single(rig.Pilot.Reads());

            rig.Browser.BeginFieldEditFromTab("R-1");

            Assert.Single(rig.Pilot.Reads());
            Assert.Equal("No editable aquí: el rack tiene una definición propia.", rig.Field.NotEditableText);
            Assert.Equal("Usa RACKEDITAR para editarlo.", rig.Field.ClassicEditorHintText);

            rig.Browser.BeginFieldEdit();

            Assert.Equal(2, rig.Pilot.Reads().Count());
        }

        [Fact]
        public void WithAnErrorOfTheFieldNoGestureReadsAndOnlyReloadActs()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.SetPendingOp] = new InvalidOperationException("falló el puerto");
            rig.Field.Text = "1:75";
            Assert.False(rig.Field.IsEditable);
            Assert.True(rig.Field.ShowReloadButton);
            Assert.False(rig.Field.ShowEditButton);
            rig.Pilot.Throws.Clear();
            rig.Pilot.Calls.Clear();

            rig.Browser.BeginFieldEditFromTab("R-1");

            Assert.Empty(rig.Pilot.Reads());
            Assert.Contains("falló el puerto", rig.Field.StateText);

            rig.Browser.ReloadField();

            Assert.Empty(rig.Pilot.Reads());
            Assert.True(rig.Field.IsEditable);
        }

        // ------------------------------------------------------------------ a failure of the previous surface and the conservative mark

        [Fact]
        public void AFailureCrossingTheDocumentChangeOfThePreviousSurfaceIsContainedAndDoesNotPutTheNewFieldInError()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.BoundaryResponder = (_, _, kind) => kind == PilotBoundaryKind.DocumentChange
                ? throw new InvalidOperationException("falló la frontera de A")
                : new PilotBoundaryResult(kind, PilotBoundaryDecision.Kept, false, Array.Empty<PilotFieldProblem>(), null);

            rig.Present(B, SelectionContext.One("R-9"));

            Assert.Equal("R-9", rig.Browser.ActiveRackId);
            Assert.Equal(WorkspacePilotFieldViewModel.NoDraftMarker, rig.Field.Text);
            Assert.Equal("Sin borrador.", rig.Field.StateText);
            Assert.DoesNotContain("falló la frontera de A", rig.Field.StateText);
            Assert.False(rig.Field.ShowReloadButton);
            Assert.True(rig.Field.ShowEditButton);
        }

        [Fact]
        public void AFailureCrossingDematerializeOfThePreviousTabIsContainedAndDoesNotPutTheNewFieldInError()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.BoundaryResponder = (_, _, kind) => kind == PilotBoundaryKind.Dematerialize
                ? throw new InvalidOperationException("falló la frontera de R-1")
                : new PilotBoundaryResult(kind, PilotBoundaryDecision.Kept, false, Array.Empty<PilotFieldProblem>(), null);

            rig.Present(A, SelectionContext.One("R-2"));

            Assert.Equal("R-2", rig.Browser.ActiveRackId);
            Assert.Equal("Sin borrador.", rig.Field.StateText);
            Assert.False(rig.Field.ShowReloadButton);
            Assert.True(rig.Field.ShowEditButton);
        }

        [Fact]
        public void WhenThePortCannotSayTheTabIsTreatedAsProtectedWithTheMarkAndAPreviewIsPinned()
        {
            var rig = new Rig();
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("falló la lectura de memoria");

            rig.Present(A, SelectionContext.One("R-1"));

            Assert.Equal("Nombre R-1 *", rig.Tab("R-1").Title);
            Assert.False(rig.Tab("R-1").IsPreview);
            Assert.Empty(rig.Pilot.Reads());
        }

        [Fact]
        public void T17_EveryCallCarriesTheSessionAndTheRackOfItsOwnSurfaceAndTheOnesOfANeverCarryB()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pinned(A, "R-2");
            rig.Browser.BeginFieldEdit();
            rig.Field.Text = "1:75";
            rig.Present(B, SelectionContext.One("R-9"));
            rig.Browser.PinTab("R-9");
            rig.Browser.BeginFieldEdit();
            rig.Field.Text = "1:100";
            rig.Field.CommitPending();
            rig.Present(A, SelectionContext.None);

            Assert.NotEmpty(rig.Pilot.Calls);
            foreach (var call in rig.Pilot.Calls.Where(c => c.RackId != null))
            {
                var expected = call.RackId == "R-9" ? B : A;
                Assert.Equal(expected, call.Origin);
            }

            Assert.Contains(rig.Pilot.Calls, c => c.RackId == "R-9" && c.Origin == B);
            Assert.Contains(rig.Pilot.Calls, c => c.RackId == "R-2" && c.Origin == A);
        }
    }
}
