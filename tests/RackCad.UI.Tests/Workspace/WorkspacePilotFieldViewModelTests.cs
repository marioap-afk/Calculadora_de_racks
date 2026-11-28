#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack.Pilot;
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
            // F4-T3B2 (N-1, P-05): the double answers Kept as the host does, so the crossing mutates nothing and the pending text stays in the draft.
            var keptCrossing = Assert.Single(kept.Pilot.Of(WorkspacePilotEditPortDouble.BoundaryOp), c => c.Kind == PilotBoundaryKind.TabChange);
            Is(keptCrossing, A, "R-1");
            Assert.Equal(" 1:7 ", kept.Pilot.Current(A, "R-1").PendingText);
            Assert.Equal("1:50", kept.Pilot.Current(A, "R-1").CommittedText);

            kept.Browser.ActivateTab("R-1");

            Assert.Equal(" 1:7 ", kept.Field.Text);
            Assert.Empty(kept.Pilot.Of(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Equal(" 1:7 ", kept.Pilot.Current(A, "R-1").PendingText);
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

        // ------------------------------------------------------------------ I-64 F4-T3C1 (D-T3C1-05, D-T3C1-06)

        [Fact]
        public void TheErrorOfTheFieldShowsTheReasonOfTheErrorThatCrossedThePort()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(
                PilotFieldState.Error, isProtected: true, canEdit: false, committedReason: "El rack no tiene escala de anotación.", errorReason: "la causa del error"));

            rig.Present(A, SelectionContext.One("R-1"));

            Assert.Equal("Error: la causa del error", rig.Field.StateText);
            Assert.True(rig.Field.ShowReloadButton);
            Assert.True(rig.Field.ShowDiscardButton);
        }

        [Theory]
        [InlineData(null, null, "Error: sin detalle.")]
        [InlineData("   ", "", "Error: sin detalle.")]
        [InlineData(null, "No se pudo leer el estado del campo.", "Error: No se pudo leer el estado del campo.")]
        [InlineData(" ", "El rack no tiene escala de anotación.", "Error: El rack no tiene escala de anotación.")]
        public void TheErrorOfTheFieldFallsBackToTheCurrentReasonAndThenToNoDetail(string? errorReason, string? committedReason, string expected)
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(
                PilotFieldState.Error, isProtected: true, canEdit: false, committedReason: committedReason, errorReason: errorReason));

            rig.Present(A, SelectionContext.One("R-1"));

            Assert.Equal(expected, rig.Field.StateText);
        }

        [Fact]
        public void TheReasonOfTheErrorOnlyAppearsInTheStateError()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:75", isProtected: true, errorReason: "no debe verse"));

            rig.Present(A, SelectionContext.One("R-1"));

            Assert.DoesNotContain("no debe verse", rig.Field.StateText);
            Assert.DoesNotContain("Error", rig.Field.StateText);
        }

        [Theory]
        [InlineData("Busy", "Hay una lectura del piloto en curso.", "El texto no se aplicó: Hay una lectura del piloto en curso.")]
        [InlineData("NotEditable", "El campo no se puede editar ahora.", "El texto no se aplicó: El campo no se puede editar ahora.")]
        [InlineData("Busy", null, "El texto no se aplicó: hay una lectura en curso.")]
        [InlineData("NotEditable", " ", "El texto no se aplicó: el campo no se puede editar ahora.")]
        public void ABusyOrNotEditableRefusalShowsTheReasonOfThePortOnceAndTheGenericTextOnlyWithoutOne(string status, string? reason, string expected)
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.SetPendingResponder = (_, _, _) => status == "Busy"
                ? PilotFieldResult.Busy(reason!)
                : PilotFieldResult.NotEditable(reason!, WorkspacePilotEditPortDouble.NoDraft());

            rig.Field.Text = "1:75";

            Assert.Equal(expected, rig.Field.NoticeText);
        }

        [Fact]
        public void ARejectedRefusalKeepsItsText()
        {
            var rig = new Rig();
            rig.Opened(A, "R-1");
            rig.Pilot.SetPendingResponder = (_, _, _) => PilotFieldResult.Rejected("La petición no es del documento activo.");

            rig.Field.Text = "1:75";

            Assert.Equal("El texto no se aplicó: La petición no es del documento activo.", rig.Field.NoticeText);
        }

        // ================================================================== I-64 F4-T4C2 (D-T4C-11, D-T4C-03): the update of the rack
        //
        // The gesture Update() of the field, over the double of the edit port: the button, the order of the five steps, the boundary of the update, the
        // presentation of every outcome WITHOUT reclassifying it, the five rules of the presentation (once, nested repaint, per surface and after IsMine,
        // the start value in the gesture, a new request never erases an outcome) and the texts, each one written here in full and byte for byte. Nothing
        // reads a drawing and nothing is written: the double records what the field asked of the port.

        private const string Rk = "R-1";

        private const string UAppliedText = "Actualizado: el cambio se aplicó al dibujo.";
        private const string UCommitThrewText = "Estado desconocido; comprueba.";
        private const string UPostRereadText = "Aplicado; recarga para seguir editando.";
        private const string USurfaceChangedText = "La superficie cambió durante la lectura; el resultado no se mostró.";

        private static PilotFieldView DirtyView(string? pending = null, bool stale = false, bool canEdit = true) =>
            WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, "2.5", pending, null, stale, true, canEdit);

        private static PilotFieldView CleanView() => WorkspacePilotEditPortDouble.View(PilotFieldState.Clean, "2.5");

        private static EditUpdateOutcome Applied() => EditUpdateOutcome.Closed(EditUpdateStatus.Applied, null, Array.Empty<string>(), Array.Empty<string>());

        private static EditUpdateOutcome Closed(EditUpdateStatus status, string? reason = null, string[]? differences = null, string[]? warnings = null) =>
            EditUpdateOutcome.Closed(status, reason, differences ?? Array.Empty<string>(), warnings ?? Array.Empty<string>());

        // An outcome and a start with a status that no enumerator names, built through the private constructor of the value.
        private static T ForgeUpdate<T>(params object?[] arguments)
            where T : struct =>
            (T)Activator.CreateInstance(typeof(T), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, arguments, null)!;

        private static EditUpdateOutcome UnknownOutcome() =>
            ForgeUpdate<EditUpdateOutcome>((EditUpdateStatus)99, "forged", Array.Empty<string>(), Array.Empty<string>(), EditUpdateStatus.Unavailable, null);

        private static EditUpdateStart UnknownStart() => ForgeUpdate<EditUpdateStart>((EditUpdateStartStatus)99, "forged");

        private sealed class UpdateRig
        {
            public readonly WorkspacePilotEditPortDouble Port = new();
            public readonly WorkspacePilotFieldViewModel Field;
            public int Changed;

            public UpdateRig(PilotFieldView? view = null, bool preview = false)
            {
                Field = new WorkspacePilotFieldViewModel(Port, () => Changed++);
                Port.Set(A, Rk, view ?? DirtyView());
                Field.Show(A, Rk, preview);
                Port.Calls.Clear();
                Port.UpdateLog.Clear();
                Changed = 0;
            }

            public void Set(PilotFieldView view) => Port.Set(A, Rk, view);

            // The register of the update as text: the operation and, for the boundary, its kind.
            public string[] Order() => Port.UpdateLog.Select(e => e.Operation + (e.Kind.HasValue ? ":" + e.Kind.Value : string.Empty)).ToArray();

            // The request is accepted and the update is in flight, as the host answers when the callback is deferred.
            public void AcceptInFlight() => Port.UpdateStartResponder = _ =>
            {
                Port.SetProgress(A, Rk, true);
                return EditUpdateStart.Accepted();
            };

            // The request is accepted and the outcome is already there, as the host answers when the callback ran inside the call.
            public void AcceptWith(EditUpdateOutcome outcome, PilotFieldView? after = null) => Port.UpdateStartResponder = _ =>
            {
                Port.SetProgress(A, Rk, false, outcome);
                if (after.HasValue) Set(after.Value);
                return EditUpdateStart.Accepted();
            };
        }

        private static readonly string[] TheFiveSteps =
        {
            WorkspacePilotEditPortDouble.TakeProgressOp,
            WorkspacePilotEditPortDouble.BoundaryOp + ":Update",
            WorkspacePilotEditPortDouble.RequestUpdateOp,
            WorkspacePilotEditPortDouble.TakeProgressOp,
        };

        // ------------------------------------------------------------------ the button (D-T4C-11, INV-F4T4C-11)

        public static TheoryData<string, bool> UpdateOffers()
        {
            var data = new TheoryData<string, bool>
            {
                { "Dirty", true },
                { "DirtyPossiblyStale", true },
                { "DirtyWithPendingText", true },
                { "Clean", false },
                { "CleanPossiblyStale", false },
                { "Conflict", false },
                { "UnknownCommitThrew", false },
                { "UnknownPostReread", false },
                { "UnknownNoOrigin", false },
                { "Applying", false },
                { "Error", false },
                { "Undecidable", false },
                { "NotOpen", false },
            };
            return data;
        }

        private static PilotFieldView UpdateViewNamed(string name) => name switch
        {
            "Dirty" => DirtyView(),
            "DirtyPossiblyStale" => DirtyView(stale: true),
            "DirtyWithPendingText" => DirtyView(pending: "3"),
            "Clean" => CleanView(),
            "CleanPossiblyStale" => WorkspacePilotEditPortDouble.View(PilotFieldState.Clean, "2.5", stale: true),
            "Conflict" => WorkspacePilotEditPortDouble.View(PilotFieldState.Conflict, "2.5", isProtected: true, conflictText: "A1:EnvelopeText"),
            "UnknownCommitThrew" => WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, "2.5", isProtected: true, unknownOrigin: PilotUnknownOrigin.CommitThrew),
            "UnknownPostReread" => WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, "2.5", isProtected: true, unknownOrigin: PilotUnknownOrigin.PostReread),
            "UnknownNoOrigin" => WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, "2.5", isProtected: true),
            "Applying" => WorkspacePilotEditPortDouble.View(PilotFieldState.Applying, "2.5", isProtected: true),
            "Error" => WorkspacePilotEditPortDouble.View(PilotFieldState.Error, "2.5", isProtected: true),
            "Undecidable" => WorkspacePilotEditPortDouble.View(PilotFieldState.Undecidable, "2.5", isProtected: true),
            _ => WorkspacePilotEditPortDouble.NoDraft(),
        };

        [Theory]
        [MemberData(nameof(UpdateOffers))]
        public void U01_TheUpdateButtonIsOfferedOnAPinnedSurfaceOnlyForADirtyDraftAlsoPossiblyStaleAndNeverInTheOtherStates(string name, bool offered)
        {
            var rig = new UpdateRig(UpdateViewNamed(name));

            Assert.Equal(offered, rig.Field.ShowUpdateButton);
            Assert.Empty(rig.Port.Calls);
        }

        [Fact]
        public void U02_TheUpdateButtonIsNotOfferedOnAPreviewSurfaceNorWithAFailureOfTheFieldAndTheReloadOfTheFailureOffersItAgain()
        {
            var preview = new UpdateRig(DirtyView(), preview: true);
            Assert.False(preview.Field.ShowUpdateButton);

            var failed = new UpdateRig();
            Assert.True(failed.Field.ShowUpdateButton);
            failed.Field.Fail(new InvalidOperationException("falló el puerto"));
            Assert.False(failed.Field.ShowUpdateButton);
            failed.Field.Rematerialize();
            Assert.True(failed.Field.ShowUpdateButton);
        }

        [Fact]
        public void U03_AFieldWithoutThePortHasNoUpdateButtonAndItsGestureDoesNothing()
        {
            var field = new WorkspacePilotFieldViewModel(null);

            field.Show(A, Rk, false);
            field.Update();

            Assert.False(field.ShowUpdateButton);
        }

        [Fact]
        public void U04_ADirtyDraftWithAPersistentDiagnosticOffersTheUpdateButtonToo()
        {
            var rig = new UpdateRig(DirtyView(stale: true));
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed("DIAG", "HINT");
            rig.Field.CheckNow();

            Assert.Equal("DIAG", rig.Field.NotEditableText);
            Assert.True(rig.Field.IsReadOnly);
            Assert.True(rig.Field.ShowUpdateButton);
        }

        // ------------------------------------------------------------------ the gesture: the button first, and the five steps in order

        [Fact]
        public void U05_TheGestureChecksItsButtonFirstAndWithoutItNothingIsAskedOfThePort()
        {
            foreach (var name in new[] { "Clean", "Conflict", "UnknownCommitThrew", "Applying", "Error", "NotOpen" })
            {
                var rig = new UpdateRig(UpdateViewNamed(name));

                rig.Field.Update();

                Assert.Empty(rig.Port.UpdateLog);
                Assert.Empty(rig.Port.Calls);
                Assert.Equal(0, rig.Changed);
            }

            var preview = new UpdateRig(DirtyView(), preview: true);
            preview.Field.Update();
            Assert.Empty(preview.Port.UpdateLog);
            Assert.Empty(preview.Port.Calls);
        }

        [Fact]
        public void U06_AnAcceptedRequestTakesWhatIsPendingCrossesTheBoundaryOfTheUpdateAsksOnceAndTakesTheProgressAndShowsApplyingWhileItIsInFlight()
        {
            var rig = new UpdateRig();
            rig.AcceptInFlight();

            rig.Field.Update();

            Assert.Equal(TheFiveSteps, rig.Order());
            var boundary = Assert.Single(rig.Port.UpdateOf(WorkspacePilotEditPortDouble.BoundaryOp));
            Assert.Equal(PilotBoundaryKind.Update, boundary.Kind);
            Assert.Equal(A, boundary.Origin);
            Assert.Equal(Rk, boundary.RackId);
            var request = Assert.Single(rig.Port.UpdateOf(WorkspacePilotEditPortDouble.RequestUpdateOp));
            Assert.Equal(A, request.Origin);
            Assert.Equal(Rk, request.RackId);
            Assert.All(rig.Port.UpdateOf(WorkspacePilotEditPortDouble.TakeProgressOp), e => Assert.Equal((A, Rk), (e.Origin, e.RackId)));
            Assert.Equal("Aplicando", rig.Field.StateText);
            Assert.False(rig.Field.ShowUpdateButton);
            Assert.Equal(string.Empty, rig.Field.NoticeText);
            Assert.Empty(rig.Port.Reads());
            Assert.Equal(1, rig.Changed);
        }

        [Fact]
        public void U07_ASecondGestureWithTheButtonAlreadyHiddenDoesNotCallThePort()
        {
            var rig = new UpdateRig();
            rig.AcceptInFlight();
            rig.Field.Update();
            Assert.False(rig.Field.ShowUpdateButton);
            var before = rig.Port.UpdateLog.Count;

            rig.Field.Update();
            rig.Field.Update();

            Assert.Equal(before, rig.Port.UpdateLog.Count);
            Assert.Equal(1, rig.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));
        }

        [Fact]
        public void U08_AnAcceptedRequestWhoseOutcomeIsAlreadyThereShowsItAtOnceAndTheStateIsTheOneOfTheMemoryOfThePort()
        {
            var rig = new UpdateRig();
            rig.AcceptWith(Applied(), CleanView());

            rig.Field.Update();

            Assert.Equal(TheFiveSteps, rig.Order());
            Assert.Equal(UAppliedText, rig.Field.NoticeText);
            Assert.Equal("Limpio", rig.Field.StateText);
            Assert.False(rig.Field.ShowUpdateButton);
            Assert.Equal(1, rig.Changed);
            // K = 1: the one live surface is painted again from the memory of the port, which is the authority of the state.
            Assert.True(rig.Port.Count(WorkspacePilotEditPortDouble.GetFieldOp) > 0);
        }

        [Fact]
        public void U09_AnAcceptedRequestWithoutOutcomeAndNotInFlightBecauseANestedRepaintPresentedItTouchesNothing()
        {
            var rig = new UpdateRig();
            rig.Port.UpdateStartResponder = _ => EditUpdateStart.Accepted();
            rig.Field.ShowNotice("aviso previo");

            rig.Field.Update();

            Assert.Equal(TheFiveSteps, rig.Order());
            // A new gesture ends the notice of the previous one, and nothing was presented: no outcome and no flight invented.
            Assert.Equal(string.Empty, rig.Field.NoticeText);
            Assert.Equal("Modificado", rig.Field.StateText);
            Assert.True(rig.Field.ShowUpdateButton);
        }

        // ------------------------------------------------------------------ step 1: what is pending is presented first, and then nothing is asked

        [Fact]
        public void U10_AnOutcomeThatWasPendingIsPresentedAndNothingIsAskedAndItIsPresentedOnce()
        {
            var rig = new UpdateRig();
            rig.Port.SetProgress(A, Rk, false, Applied());

            rig.Field.Update();

            Assert.Equal(new[] { WorkspacePilotEditPortDouble.TakeProgressOp }, rig.Order());
            Assert.Equal(UAppliedText, rig.Field.NoticeText);
            Assert.Equal(0, rig.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));
            Assert.Equal(0, rig.Port.UpdateCount(WorkspacePilotEditPortDouble.BoundaryOp));
            Assert.Equal(1, rig.Changed);

            // The outcome was consumed: the next gesture finds nothing pending, asks, and shows what the start says, not the old outcome again.
            rig.Port.UpdateStartResponder = _ => EditUpdateStart.Busy("Hay una actualización en curso: espera a que termine.");
            rig.Field.Update();

            Assert.Equal("Ocupado: Hay una actualización en curso: espera a que termine. No se actualizó.", rig.Field.NoticeText);
        }

        // ------------------------------------------------------------------ step 2: the boundary of the update (H-27)

        [Fact]
        public void U11_ProceededWithAValidPendingTextAndWithoutTextAsksForTheUpdate()
        {
            var withText = new UpdateRig(DirtyView(pending: "3"));
            withText.AcceptInFlight();
            withText.Field.Update();
            Assert.Equal(TheFiveSteps, withText.Order());
            Assert.Equal(1, withText.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));

            var withoutText = new UpdateRig(DirtyView());
            withoutText.AcceptInFlight();
            withoutText.Field.Update();
            Assert.Equal(TheFiveSteps, withoutText.Order());
        }

        [Fact]
        public void U12_AnInvalidPendingTextAbortsTheBoundaryAndNothingIsAskedAndTheMessageOfTheFieldIsShown()
        {
            var rig = new UpdateRig(DirtyView(pending: "3x"));
            rig.Port.Validate = text => text == "3x" ? "El texto no es válido." : null;
            rig.Port.UpdateStartResponder = _ => EditUpdateStart.Accepted();

            rig.Field.Update();

            Assert.Equal(new[] { WorkspacePilotEditPortDouble.TakeProgressOp, WorkspacePilotEditPortDouble.BoundaryOp + ":Update" }, rig.Order());
            Assert.Equal("No se actualizó: El texto no es válido.", rig.Field.NoticeText);
            Assert.Equal(0, rig.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));
        }

        [Fact]
        public void U13_AnAbortWithoutAMessageShowsItsReasonAndAnAbortWithNeitherShowsTheTextOfNoDetail()
        {
            var reason = new UpdateRig();
            reason.Port.BoundaryResponder = (_, _, kind) =>
                new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, Array.Empty<PilotFieldProblem>(), "La petición no es del documento activo.");
            reason.Field.Update();
            Assert.Equal("No se actualizó: La petición no es del documento activo.", reason.Field.NoticeText);
            Assert.Equal(0, reason.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));

            var neither = new UpdateRig();
            neither.Port.BoundaryResponder = (_, _, kind) =>
                new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, Array.Empty<PilotFieldProblem>(), null);
            neither.Field.Update();
            Assert.Equal("No se actualizó: sin detalle.", neither.Field.NoticeText);
            Assert.Equal(0, neither.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));
        }

        [Fact]
        public void U14_KeptAndBusyAndAnUnknownDecisionDoNotAskAndShowTheirText()
        {
            var kept = new UpdateRig(DirtyView(pending: "3", canEdit: false));
            kept.Port.UpdateStartResponder = _ => EditUpdateStart.Accepted();
            kept.Field.Update();
            Assert.Equal("No se actualizó: el borrador no se puede actualizar ahora.", kept.Field.NoticeText);
            Assert.Equal(0, kept.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));

            var busy = new UpdateRig();
            busy.Port.BoundaryResponder = (_, _, kind) =>
                new PilotBoundaryResult(kind, PilotBoundaryDecision.Busy, false, Array.Empty<PilotFieldProblem>(), "Hay una lectura del piloto en curso.");
            busy.Field.Update();
            Assert.Equal("Ocupado: Hay una lectura del piloto en curso. No se actualizó.", busy.Field.NoticeText);
            Assert.Equal(0, busy.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));

            var unknown = new UpdateRig();
            unknown.Port.BoundaryResponder = (_, _, kind) =>
                new PilotBoundaryResult(kind, (PilotBoundaryDecision)99, false, Array.Empty<PilotFieldProblem>(), null);
            unknown.Field.Update();
            Assert.Equal("No se actualizó: el borrador no se puede actualizar ahora.", unknown.Field.NoticeText);
            Assert.Equal(0, unknown.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));
        }

        // ------------------------------------------------------------------ step 5: every start value that is not Accepted is shown in the gesture, with its cause

        public static TheoryData<EditUpdateStart, string> StartValues() => new()
        {
            { EditUpdateStart.Waiting("hay un comando."), "En espera: hay un comando. No se actualizó." },
            { EditUpdateStart.Rejected("otro documento."), "Rechazado: otro documento. No se actualizó." },
            { EditUpdateStart.Busy("hay una lectura."), "Ocupado: hay una lectura. No se actualizó." },
            { EditUpdateStart.Unavailable("no hay ejecutor."), "No disponible: no hay ejecutor. No se actualizó." },
            { EditUpdateStart.Failed("no se pudo pedir el contexto."), "Fallo: no se pudo pedir el contexto. No se actualizó." },
            { EditUpdateStart.Failed(PushBackPilotHostTexts.HostNotRunReason), "Fallo: La actualización no llegó a ejecutarse; no se escribió nada en el dibujo. No se actualizó." },
            { EditUpdateStart.Failed(" "), "Fallo: sin detalle. No se actualizó." },
            { default(EditUpdateStart), "No disponible: sin detalle. No se actualizó." },
            { UnknownStart(), "No se pudo actualizar: forged No se actualizó." },
        };

        [Theory]
        [MemberData(nameof(StartValues))]
        public void U15_AStartValueThatIsNotAcceptedIsShownInTheGestureWithItsCauseAndTheProgressIsNotTakenAfterIt(EditUpdateStart start, string expected)
        {
            var rig = new UpdateRig();
            rig.Port.UpdateStartResponder = _ => start;

            rig.Field.Update();

            Assert.Equal(
                new[] { WorkspacePilotEditPortDouble.TakeProgressOp, WorkspacePilotEditPortDouble.BoundaryOp + ":Update", WorkspacePilotEditPortDouble.RequestUpdateOp },
                rig.Order());
            Assert.Equal(expected, rig.Field.NoticeText);
            Assert.NotEqual("Aplicando", rig.Field.StateText);
            Assert.True(rig.Field.ShowUpdateButton);
        }

        [Fact]
        public void U16_AFailedStartDoesNotLeaveAnythingInTheStoreAndTheNextGestureAsksAgain()
        {
            var rig = new UpdateRig();
            rig.Port.UpdateStartResponder = _ => EditUpdateStart.Failed(PushBackPilotHostTexts.HostNotRunReason);
            rig.Field.Update();
            rig.Field.Update();

            Assert.Equal(2, rig.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));
            // What is pending is taken once per gesture, and a start value that is not Accepted is never followed by a take.
            Assert.Equal(2, rig.Port.UpdateCount(WorkspacePilotEditPortDouble.TakeProgressOp));
        }

        // ------------------------------------------------------------------ the presentation of every outcome, without reclassifying it

        private static readonly string[] OneDifference = { "A1:EnvelopeText" };

        public static TheoryData<EditUpdateOutcome, string> Outcomes() => new()
        {
            { Applied(), UAppliedText },
            { Closed(EditUpdateStatus.AppliedWithWarnings, null, null, new[] { "aviso uno", "aviso dos" }), "Actualizado con avisos: aviso uno; aviso dos" },
            { Closed(EditUpdateStatus.AppliedWithWarnings), "Actualizado con avisos: sin detalle." },
            { Closed(EditUpdateStatus.NotAppliedConflict, null, new[] { "A1:EnvelopeText", "B2:Membership" }), "No se aplicó: el dibujo cambió fuera del panel. Diferencias: A1:EnvelopeText; B2:Membership" },
            { Closed(EditUpdateStatus.NotAppliedConflict), "No se aplicó: el dibujo cambió fuera del panel. Diferencias: sin detalle." },
            { Closed(EditUpdateStatus.NotAppliedAdmission, "el rack ya no es editable."), "No se aplicó: el rack ya no es editable." },
            { Closed(EditUpdateStatus.NotAppliedPreflight, "falta una vista."), "No se aplicó: falta una vista." },
            { Closed(EditUpdateStatus.NotAppliedFailure, "falló la escritura."), "No se aplicó: falló la escritura." },
            { Closed(EditUpdateStatus.NotAppliedFailure), "No se aplicó: sin detalle." },
            { Closed(EditUpdateStatus.CommitThrew, "el cierre lanzó."), UCommitThrewText },
            { Closed(EditUpdateStatus.AppliedPostRereadFailed, "falló la relectura."), UPostRereadText },
            { EditUpdateOutcome.ClosedDetached(), "El borrador se descartó o su sesión terminó durante la actualización." },
            { EditUpdateOutcome.Rejected("otro documento."), "Rechazado: otro documento." },
            { EditUpdateOutcome.NoDraft("no hay borrador."), "Sin borrador: no hay borrador." },
            { EditUpdateOutcome.NothingToApply("el borrador está limpio."), "Nada que aplicar: el borrador está limpio." },
            { EditUpdateOutcome.PendingText("hay texto sin aplicar."), "Hay texto pendiente: hay texto sin aplicar." },
            { EditUpdateOutcome.NotApplicable("el borrador no se puede actualizar."), "No aplicable: el borrador no se puede actualizar." },
            { EditUpdateOutcome.AnotherUpdateInProgress(), "Ya hay una actualización en curso." },
            { EditUpdateOutcome.Refused("el borrador no empezó."), "El borrador no admitió la actualización: el borrador no empezó." },
            { EditUpdateOutcome.Failed(PushBackPilotHostTexts.HostNotRunReason), "Fallo: La actualización no llegó a ejecutarse; no se escribió nada en el dibujo." },
            { EditUpdateOutcome.Failed(string.Empty), "Fallo: sin detalle." },
            { UnknownOutcome(), "Resultado de la actualización no reconocido." },
        };

        [Theory]
        [MemberData(nameof(Outcomes))]
        public void U17_EveryOutcomeIsShownByItsClassWithItsTextAndItsCauseAndOnce(EditUpdateOutcome outcome, string expected)
        {
            var rig = new UpdateRig();
            rig.AcceptWith(outcome);

            rig.Field.Update();

            Assert.Equal(TheFiveSteps, rig.Order());
            Assert.Equal(expected, rig.Field.NoticeText);

            // Once: a repaint finds nothing and leaves the notice as it was.
            rig.Field.Show(A, Rk, false);
            Assert.Equal(expected, rig.Field.NoticeText);
        }

        public static TheoryData<EditUpdateOutcome, string> ClosedInErrorOutcomes() => new()
        {
            {
                EditUpdateOutcome.ClosedInError("El borrador no aceptó el cierre de la actualización.", EditUpdateStatus.Applied, "la causa", Array.Empty<string>(), new[] { "aviso" }),
                "El borrador quedó en error al cerrar la actualización. Clasificación: aplicada. El borrador no aceptó el cierre de la actualización. Causa: la causa Avisos: aviso"
            },
            {
                EditUpdateOutcome.ClosedInError("no se rebasó.", EditUpdateStatus.AppliedWithWarnings, null, Array.Empty<string>(), new[] { "uno", "dos" }),
                "El borrador quedó en error al cerrar la actualización. Clasificación: aplicada con avisos. no se rebasó. Avisos: uno; dos"
            },
            {
                EditUpdateOutcome.ClosedInError("no se rebasó.", EditUpdateStatus.NotAppliedConflict, null, OneDifference, Array.Empty<string>()),
                "El borrador quedó en error al cerrar la actualización. Clasificación: no aplicada por conflicto. no se rebasó. Diferencias: A1:EnvelopeText"
            },
            {
                EditUpdateOutcome.ClosedInError(null, EditUpdateStatus.CommitThrew, "el cierre lanzó", Array.Empty<string>(), Array.Empty<string>()),
                "El borrador quedó en error al cerrar la actualización. Clasificación: desconocida (el cierre falló). Causa: el cierre lanzó"
            },
            {
                EditUpdateOutcome.ClosedInError(null, EditUpdateStatus.AppliedPostRereadFailed, null, Array.Empty<string>(), Array.Empty<string>()),
                "El borrador quedó en error al cerrar la actualización. Clasificación: aplicada con la relectura fallida."
            },
            {
                EditUpdateOutcome.ClosedInError(null, EditUpdateStatus.NotAppliedAdmission, null, Array.Empty<string>(), Array.Empty<string>()),
                "El borrador quedó en error al cerrar la actualización. Clasificación: no aplicada por la admisión."
            },
            {
                EditUpdateOutcome.ClosedInError(null, EditUpdateStatus.NotAppliedPreflight, null, Array.Empty<string>(), Array.Empty<string>()),
                "El borrador quedó en error al cerrar la actualización. Clasificación: no aplicada por la comprobación previa."
            },
            {
                EditUpdateOutcome.ClosedInError(null, EditUpdateStatus.NotAppliedFailure, null, Array.Empty<string>(), Array.Empty<string>()),
                "El borrador quedó en error al cerrar la actualización. Clasificación: no aplicada por un fallo."
            },
            {
                EditUpdateOutcome.ClosedInError(null, EditUpdateStatus.Unavailable, null, Array.Empty<string>(), Array.Empty<string>()),
                "El borrador quedó en error al cerrar la actualización. Clasificación: sin clasificar."
            },
        };

        [Theory]
        [MemberData(nameof(ClosedInErrorOutcomes))]
        public void U18_AClosedInErrorOutcomeIsShownWithItsClassificationAndNeverAsASuccessWhateverTheClassificationIs(EditUpdateOutcome outcome, string expected)
        {
            var rig = new UpdateRig();
            rig.AcceptWith(outcome);

            rig.Field.Update();

            Assert.Equal(expected, rig.Field.NoticeText);
            Assert.DoesNotContain("Actualizado", rig.Field.NoticeText);
            Assert.NotEqual(UAppliedText, rig.Field.NoticeText);
        }

        [Fact]
        public void U19_NoClassificationOfAClosedInErrorOutcomeEverProducesTheTextOfASuccess()
        {
            foreach (var classification in Enum.GetValues<EditUpdateStatus>())
            {
                var rig = new UpdateRig();
                rig.AcceptWith(EditUpdateOutcome.ClosedInError("r", classification, "c", Array.Empty<string>(), Array.Empty<string>()));

                rig.Field.Update();

                Assert.StartsWith("El borrador quedó en error al cerrar la actualización. Clasificación: ", rig.Field.NoticeText, StringComparison.Ordinal);
                Assert.DoesNotContain("Actualizado", rig.Field.NoticeText);
            }
        }

        [Fact]
        public void U20_UnknownIsNeverShownAsAFailureNorAsASuccessAndNoOutcomeShowsNothing()
        {
            foreach (var unknown in new[] { Closed(EditUpdateStatus.CommitThrew, "x"), Closed(EditUpdateStatus.AppliedPostRereadFailed, "x") })
            {
                var rig = new UpdateRig();
                rig.AcceptWith(unknown);
                rig.Field.Update();
                Assert.DoesNotMatch("^(Fallo|Actualizado|No se aplicó|Rechazado)", rig.Field.NoticeText);
            }

            // An outcome that says nothing (the default value, or an Unavailable one) is not an outcome: nothing is presented and nothing is invented.
            foreach (var none in new[] { default(EditUpdateOutcome), EditUpdateOutcome.Unavailable("no hay") })
            {
                var rig = new UpdateRig();
                rig.AcceptWith(none);
                rig.Field.Update();
                Assert.Equal(string.Empty, rig.Field.NoticeText);
                Assert.Equal("Modificado", rig.Field.StateText);
            }
        }

        // ------------------------------------------------------------------ the five rules of the presentation (D-T4C-03)

        [Fact]
        public void U21_ANestedRepaintThatTakesTheOutcomeFirstLeavesTheGestureWithInFlightFalseAndNothingToPresentAndItTouchesNothing()
        {
            var rig = new UpdateRig();
            rig.Port.UpdateHook = _ =>
            {
                rig.Port.SetProgress(A, Rk, false, Applied());
                rig.Field.Show(A, Rk, false);
            };
            rig.Port.UpdateStartResponder = _ => EditUpdateStart.Accepted();

            rig.Field.Update();

            Assert.Equal(UAppliedText, rig.Field.NoticeText);
            Assert.NotEqual("Aplicando", rig.Field.StateText);
            Assert.Equal(
                new[]
                {
                    WorkspacePilotEditPortDouble.TakeProgressOp,
                    WorkspacePilotEditPortDouble.BoundaryOp + ":Update",
                    WorkspacePilotEditPortDouble.RequestUpdateOp,
                    WorkspacePilotEditPortDouble.TakeProgressOp,
                    WorkspacePilotEditPortDouble.TakeProgressOp,
                },
                rig.Order());
        }

        [Fact]
        public void U22_ARepaintWithInFlightFalseAndNoOutcomeNeverTouchesWhatWasPresentedNorInventsAnOutcome()
        {
            var rig = new UpdateRig();
            rig.AcceptWith(Applied(), CleanView());
            rig.Field.Update();
            Assert.Equal(UAppliedText, rig.Field.NoticeText);

            rig.Field.Show(A, Rk, false);
            rig.Field.Show(A, Rk, false);

            Assert.Equal(UAppliedText, rig.Field.NoticeText);
            Assert.Equal("Limpio", rig.Field.StateText);
        }

        [Fact]
        public void U23_ApplyingComesOnlyFromInFlightOrFromTheApplyingStateOfTheFieldAndOtherwiseTheFieldIsPaintedFromItsState()
        {
            var rig = new UpdateRig();
            rig.AcceptInFlight();
            rig.Field.Update();
            Assert.Equal("Aplicando", rig.Field.StateText);

            // Not in flight any more and no outcome yet (nothing invented): the state is the one the memory of the port says.
            rig.Port.SetProgress(A, Rk, false);
            rig.Field.Show(A, Rk, false);
            Assert.Equal("Modificado", rig.Field.StateText);
            Assert.True(rig.Field.ShowUpdateButton);

            // The state Applying of the draft is enough to say it, with no flight.
            rig.Set(UpdateViewNamed("Applying"));
            rig.Field.Show(A, Rk, false);
            Assert.Equal("Aplicando", rig.Field.StateText);
            Assert.False(rig.Field.ShowUpdateButton);
        }

        [Fact]
        public void U24_ARepaintOfTheLiveSurfacePresentsTheOutcomeThatArrivedAfterTheGestureOnceAndNeverReplacesALaterNotice()
        {
            var rig = new UpdateRig();
            rig.AcceptInFlight();
            rig.Field.Update();
            Assert.Equal("Aplicando", rig.Field.StateText);
            Assert.False(rig.Field.ShowUpdateButton);

            rig.Port.SetProgress(A, Rk, false, Applied());
            rig.Set(CleanView());
            rig.Field.Show(A, Rk, false);

            Assert.Equal(UAppliedText, rig.Field.NoticeText);
            Assert.Equal("Limpio", rig.Field.StateText);

            rig.Field.ShowNotice("otro aviso");
            rig.Field.Show(A, Rk, false);
            Assert.Equal("otro aviso", rig.Field.NoticeText);
        }

        [Fact]
        public void U25_OnlyTheLiveSurfaceOfTheRackTakesTheOutcomeAndOnlyForItsOwnRackAndSession()
        {
            var rig = new UpdateRig();
            rig.Port.SetProgress(A, "R-2", false, Applied());
            rig.Port.SetProgress(B, Rk, false, Applied());

            rig.Field.Show(A, Rk, false);
            rig.Field.Show(A, Rk, false);

            Assert.All(rig.Port.UpdateOf(WorkspacePilotEditPortDouble.TakeProgressOp), e => Assert.Equal((A, Rk), (e.Origin, e.RackId)));
            Assert.Equal(string.Empty, rig.Field.NoticeText);
            // The outcomes of the other rack and of the other session are still in the store: nobody took them.
            Assert.Equal(EditUpdateStatus.Applied, rig.Port.TakeUpdateProgress(new WorkspaceRequest(A), "R-2").Outcome.Status);
            Assert.Equal(EditUpdateStatus.Applied, rig.Port.TakeUpdateProgress(new WorkspaceRequest(B), Rk).Outcome.Status);
        }

        [Fact]
        public void U26_AnOutcomeIsNotTakenOnAPreviewSurfaceNorWithAFailureOfTheFieldAndItStaysInTheStore()
        {
            var preview = new UpdateRig(DirtyView(), preview: true);
            preview.Port.SetProgress(A, Rk, false, Applied());
            preview.Field.Show(A, Rk, true);
            Assert.Equal(0, preview.Port.UpdateCount(WorkspacePilotEditPortDouble.TakeProgressOp));
            Assert.Equal(string.Empty, preview.Field.NoticeText);

            var failed = new UpdateRig();
            failed.Field.Fail(new InvalidOperationException("falló el puerto"));
            failed.Port.UpdateLog.Clear();
            failed.Port.SetProgress(A, Rk, false, Applied());
            failed.Field.Show(A, Rk, false);
            Assert.Equal(0, failed.Port.UpdateCount(WorkspacePilotEditPortDouble.TakeProgressOp));
            Assert.Equal(EditUpdateStatus.Applied, failed.Port.TakeUpdateProgress(new WorkspaceRequest(A), Rk).Outcome.Status);
        }

        [Fact]
        public void U27_IfTheSurfaceChangedInsideTheRequestTheResultIsNotShownHereNorTakenAndItStaysInTheStore()
        {
            var rig = new UpdateRig();
            rig.Port.UpdateHook = _ =>
            {
                rig.Port.SetProgress(A, Rk, false, Applied());
                rig.Field.Show(A, "R-2", false);
            };
            rig.Port.UpdateStartResponder = _ => EditUpdateStart.Accepted();

            rig.Field.Update();

            Assert.Equal(USurfaceChangedText, rig.Field.NoticeText);
            // Taken once for R-1 (before the request); after the request the gesture did not take it, and the repaint of the other rack took its own.
            var takesOfTheRack = rig.Port.UpdateOf(WorkspacePilotEditPortDouble.TakeProgressOp).Where(e => e.RackId == Rk).ToList();
            Assert.Single(takesOfTheRack);
            Assert.Equal(EditUpdateStatus.Applied, rig.Port.TakeUpdateProgress(new WorkspaceRequest(A), Rk).Outcome.Status);
        }

        [Fact]
        public void U28_IfTheSurfaceChangedInsideTheBoundaryNothingIsAskedAndTheSurfaceChangeIsShown()
        {
            var rig = new UpdateRig();
            rig.Port.BoundaryResponder = (_, _, kind) =>
            {
                rig.Field.Show(A, "R-2", false);
                return new PilotBoundaryResult(kind, PilotBoundaryDecision.Proceeded, false, Array.Empty<PilotFieldProblem>(), null);
            };

            rig.Field.Update();

            Assert.Equal(USurfaceChangedText, rig.Field.NoticeText);
            Assert.Equal(0, rig.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));
        }

        [Fact]
        public void U29_ARequestNeverErasesAnOutcomeThatWasNotPresentedBecauseTheGestureTakesWhatIsPendingFirst()
        {
            var rig = new UpdateRig();
            rig.Port.SetProgress(A, Rk, false, Closed(EditUpdateStatus.NotAppliedConflict, null, OneDifference));
            rig.Port.UpdateStartResponder = _ => EditUpdateStart.Accepted();

            rig.Field.Update();

            Assert.Equal("No se aplicó: el dibujo cambió fuera del panel. Diferencias: A1:EnvelopeText", rig.Field.NoticeText);
            Assert.Equal(0, rig.Port.UpdateCount(WorkspacePilotEditPortDouble.RequestUpdateOp));
        }

        // ------------------------------------------------------------------ failures of the port are contained, and the field never gives a false state

        [Fact]
        public void U30_AFailureOfTheRequestTheBoundaryOrTheProgressIsContainedAsTheErrorOfTheFieldAndNeverEscapes()
        {
            foreach (var operation in new[] { WorkspacePilotEditPortDouble.RequestUpdateOp, WorkspacePilotEditPortDouble.BoundaryOp, WorkspacePilotEditPortDouble.TakeProgressOp })
            {
                var rig = new UpdateRig();
                rig.Port.UpdateStartResponder = _ => EditUpdateStart.Accepted();
                rig.Port.Throws[operation] = new InvalidOperationException("falló " + operation);

                var thrown = Record.Exception(() => rig.Field.Update());

                Assert.Null(thrown);
                Assert.StartsWith("Error: falló " + operation, rig.Field.StateText, StringComparison.Ordinal);
                Assert.False(rig.Field.ShowUpdateButton);
                Assert.True(rig.Field.ShowReloadButton);
            }
        }

        [Fact]
        public void U31_AFailureOfTheProgressInTheRepaintIsContainedAsTheErrorOfTheField()
        {
            var rig = new UpdateRig();
            rig.Port.Throws[WorkspacePilotEditPortDouble.TakeProgressOp] = new InvalidOperationException("falló el progreso");

            var thrown = Record.Exception(() => rig.Field.Show(A, Rk, false));

            Assert.Null(thrown);
            Assert.StartsWith("Error: falló el progreso", rig.Field.StateText, StringComparison.Ordinal);
        }

        [Fact]
        public void U32_TheGestureNeverReadsTheDrawingNorWritesNorTouchesTheTextOrTheDraftByItself()
        {
            var rig = new UpdateRig();
            rig.AcceptWith(Applied(), CleanView());

            rig.Field.Update();

            Assert.Empty(rig.Port.Reads());
            Assert.Equal(0, rig.Port.Count(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Equal(0, rig.Port.Count(WorkspacePilotEditPortDouble.CommitOp));
            Assert.Equal(0, rig.Port.Count(WorkspacePilotEditPortDouble.DiscardRackOp));
            Assert.Equal(0, rig.Port.Count(WorkspacePilotEditPortDouble.DiscardDocumentOp));
        }

        [Fact]
        public void U33_TheDoubleRecordsTheUpdateInItsOwnRegisterAndTheSetsOfTheOtherOperationsDoNotChange()
        {
            var rig = new UpdateRig();
            rig.AcceptInFlight();

            rig.Field.Update();

            // INV-F4T4C-25: Calls, Operations() and Reads() keep the exact sets that the closing tests assert; only CrossBoundary, which is not new, appears.
            Assert.DoesNotContain(rig.Port.Calls, c => c.Operation == WorkspacePilotEditPortDouble.RequestUpdateOp || c.Operation == WorkspacePilotEditPortDouble.TakeProgressOp);
            Assert.DoesNotContain(WorkspacePilotEditPortDouble.RequestUpdateOp, rig.Port.Operations());
            Assert.DoesNotContain(WorkspacePilotEditPortDouble.TakeProgressOp, rig.Port.Operations());
            Assert.Contains(WorkspacePilotEditPortDouble.BoundaryOp, rig.Port.Operations());
            Assert.Empty(rig.Port.Reads());
        }

        [Fact]
        public void U34_ThroughTheBrowserARepaintOfThePanelTakesTheOutcomeOfTheLiveSurfaceAndPresentsItOnce()
        {
            var rig = new Rig();
            rig.Pinned(A, "R-1");
            rig.Pilot.Set(A, "R-1", DirtyView());
            rig.Present(A, SelectionContext.One("R-1"));
            rig.Pilot.SetProgress(A, "R-1", false, Applied());
            rig.Pilot.Set(A, "R-1", CleanView());

            rig.Present(A, SelectionContext.One("R-1"));

            Assert.Equal(UAppliedText, rig.Field.NoticeText);
            Assert.Equal("Limpio", rig.Field.StateText);
            var takes = rig.Pilot.UpdateOf(WorkspacePilotEditPortDouble.TakeProgressOp).ToList();
            Assert.NotEmpty(takes);
            Assert.All(takes, e => Assert.Equal((A, "R-1"), (e.Origin, e.RackId)));

            rig.Present(A, SelectionContext.One("R-1"));
            Assert.Equal(UAppliedText, rig.Field.NoticeText);
        }
    }
}
