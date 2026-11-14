#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T4A2-PREPARE-UI (INV-F4T4A2-10 and 16; scenarios 2 to 8, 13, 14 and 15 of the order of the Owner, section 5.2 of the package): the host
// core with the real draft session and a double of the reader of the drawing. The general check carries the diagnostic of the admission AND the
// hint of the classic editor in a Conflict as well as in a Confirmed; "keep" and "discard and reload" refuse a read that was not admitted and
// change nothing; the three operations leave the pending text exactly as typed (or empty it, for the discard); a session that never existed
// and a document that is not the active one are rejected without reading. Every expectation that something throws, or does not, uses
// Record.Exception, so a behavior that is missing fails by ASSERTION and never by an escaped exception.
public class WorkspacePilotReconciliationHostTests
{
    private const string RackA = EditIntentKit.RackA;

    private sealed class Rig
    {
        public Rig()
        {
            Host = new WorkspaceHostRig().WithDocument(100);
            Host.Host.OpenPanel();
            Session = Host.ActiveSession;
            Host.Idle();
            Host.Reader.Reads.Clear();
            Reader = new EditIntentFakeReader();
            Host.Host.SetEditIntentReader(Reader);
        }

        public WorkspaceHostRig Host { get; }

        public WorkspaceHostCore Core => Host.Host;

        public EditIntentFakeReader Reader { get; }

        public SessionId Session { get; }

        public EditIntentRequest Request(string rackId = RackA, SessionId? session = null) => new(session ?? Session, rackId, null, null);

        public DraftSession<PushBackPilotDraftState> Open()
        {
            Assert.Equal(EditIntentStatus.Admitted, Core.TryBeginPilotEdit(Request()).Status);
            Reader.Calls.Clear();
            return Core.OpenDrafts.Find<PushBackPilotDraftState>(Session, RackA)!;
        }

        public static void Edit(DraftSession<PushBackPilotDraftState> draft, double scale = 2.25) =>
            draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = scale;
    }

    // What the three operations must leave alone when they do not apply.
    // The base and the current state are held by reference. The initial state is a COPY by contract of the draft (N-2: every read of
    // InitialDraftState makes a new one), so its reference can never be the same: it is held as the copy that was made when it was
    // held and compared with the pilot's own comparer, which is the equality the draft itself uses to decide that it is dirty.
    private sealed record Held(
        DraftStaleBase Base,
        PushBackPilotDraftState Initial,
        PushBackPilotDraftState Current,
        string InitialOrigin,
        double Scale,
        IReadOnlyList<KeyValuePair<string, string>> Pending,
        DraftStatus Status,
        bool IsPossiblyStale,
        bool IsDirty);

    private static Held Hold(DraftSession<PushBackPilotDraftState> draft) => new(
        draft.StaleBase,
        draft.InitialDraftState,
        draft.CurrentDraftState,
        draft.InitialDraftState.Origin,
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale,
        draft.PendingText.OrderBy(p => p.Key, StringComparer.Ordinal).ToArray(),
        draft.Status,
        draft.IsPossiblyStale,
        draft.IsDirty);

    private static void AssertHeld(Held before, DraftSession<PushBackPilotDraftState> draft)
    {
        Assert.Same(before.Base, draft.StaleBase);
        Assert.Same(before.Current, draft.CurrentDraftState);
        Assert.True(PushBackPilotStateComparer.Instance.Equals(before.Initial, draft.InitialDraftState), "the initial state of the draft changed");
        Assert.Equal(before.InitialOrigin, draft.InitialDraftState.Origin);
        Assert.Equal(before.Scale, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.Equal(before.Pending, draft.PendingText.OrderBy(p => p.Key, StringComparer.Ordinal).ToArray());
        Assert.Equal(before.Status, draft.Status);
        Assert.Equal(before.IsPossiblyStale, draft.IsPossiblyStale);
        Assert.Equal(before.IsDirty, draft.IsDirty);
    }

    private static string Lateral(string rack) => IntentKit.Variant("pb-01-base", rack, view: "lateral", section: 3);

    // The envelope of a view with a design that differs from pb-01-base only where the edit says, found by its text and not assumed. Every
    // view of a rack carries the same design, so an edit made outside the panel is made in all of them.
    private static string Edited(string rack, string? view, int? section, params (string From, string To)[] edits) =>
        IntentKit.Rewrite(IntentKit.Variant("pb-01-base", rack, view: view, section: section), embed =>
        {
            foreach (var (from, to) in edits)
            {
                Assert.Contains(from, embed.Design, StringComparison.Ordinal);
                embed.Design = embed.Design.Replace(from, to, StringComparison.Ordinal);
            }
        });

    private const string ScaleFrom = "\"AnnotationScale\": 1.5";
    private const string ScaleTo = "\"AnnotationScale\": 2";
    private const string ToleranceFrom = "\"PalletTolerance\": 4";
    private const string ToleranceTo = "\"PalletTolerance\": 5";

    private static EditIntentReadResult OriginChanged(string rack, params (string From, string To)[] edits) =>
        EditIntentKit.ReadOf(IntentKit.Scan(
            rack,
            "A1",
            IntentKit.Member("A1", Edited(rack, null, null, edits)),
            IntentKit.Member("B2", Edited(rack, "lateral", 3, edits))));

    // ------------------------------------------------------------------ scenarios 2, 3 and 4: what changed outside the panel

    [Fact]
    public void ConfirmWithAChangeOnlyOfTheTextOfTheScaleIsAConflictOfTheOriginAndNothingElse()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        rig.Reader.Answer = rack => OriginChanged(rack, (ScaleFrom, ScaleTo));

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Conflict, outcome.Status);
        Assert.Equal(new[] { "A1:EnvelopeText", "B2:EnvelopeText" }, outcome.Differences.ToArray());
        Assert.Null(outcome.AdmissionDiagnostic);
        Assert.Null(outcome.ClassicEditorHint);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void ConfirmWithAChangeOnlyOfAnotherFieldIsAConflictToo()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        rig.Reader.Answer = rack => OriginChanged(rack, (ToleranceFrom, ToleranceTo));

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Conflict, outcome.Status);
        Assert.Equal(new[] { "A1:EnvelopeText", "B2:EnvelopeText" }, outcome.Differences.ToArray());
        Assert.Null(outcome.AdmissionDiagnostic);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
    }

    [Fact]
    public void ConfirmWithAChangeOfTheScaleAndOfAnotherFieldIsAConflictAndKeepAfterwardsKeepsTheScaleOfTheDraftOnTheNewVersion()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        rig.Reader.Answer = rack => OriginChanged(rack, (ScaleFrom, ScaleTo), (ToleranceFrom, ToleranceTo));

        var confirm = rig.Core.TryConfirmPilotBase(rig.Request());
        Assert.Equal(EditConfirmStatus.Conflict, confirm.Status);
        Assert.Equal(new[] { "A1:EnvelopeText", "B2:EnvelopeText" }, confirm.Differences.ToArray());
        Assert.Equal(DraftStatus.Conflict, draft.Status);

        var keep = rig.Core.TryKeepPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Kept, keep.Status);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Equal(5.0, draft.InitialDraftState.Inputs.PalletTolerance);
    }

    // ------------------------------------------------------------------ scenario 5: the rack was deleted from the drawing

    private static EditIntentReadResult RackDeleted(string rack) => EditIntentKit.ReadOf(IntentKit.Scan(rack, "A1"));

    [Fact]
    public void ARackDeletedFromTheDrawingReadsAnEmptyBaseAndThatAdmissionIsNotAdmittedWithTheMissingOrigin()
    {
        var read = RackDeleted(RackA).PilotRead!;

        Assert.Empty(read.StaleBase.Entries);
        Assert.False(read.Read.IsAdmitted);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, read.Read.Admission.Diagnostic);
    }

    [Fact]
    public void ConfirmOfADeletedRackIsAConflictWithTheDiagnosticAndTheHintOfTheClassicEditor()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        rig.Reader.Answer = RackDeleted;

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Conflict, outcome.Status);
        Assert.NotEmpty(outcome.Differences);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, outcome.AdmissionDiagnostic);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
    }

    [Fact]
    public void KeepAndDiscardOfADeletedRackAnswerNotAdmittedWithTheDiagnosticAndTheHintAndChangeNothing()
    {
        foreach (var operation in new[] { "keep", "discard" })
        {
            var rig = new Rig();
            var draft = rig.Open();
            Rig.Edit(draft);
            draft.MarkPossiblyStale();
            draft.SetPendingText("field", "typed");
            rig.Reader.Answer = RackDeleted;
            var before = Hold(draft);

            var outcome = operation == "keep" ? rig.Core.TryKeepPilotEdit(rig.Request()) : rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

            Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
            Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, outcome.Diagnostic);
            Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
            AssertHeld(before, draft);
            Assert.Single(rig.Reader.Calls);
        }
    }

    // ------------------------------------------------------------------ scenario 6: the rack is no longer editable

    [Fact]
    public void ConfirmOfARackThatIsNoLongerEditableWithTheSameBaseIsConfirmedWithTheDiagnosticAndTheHint()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        var same = draft.StaleBase;
        rig.Reader.Answer = _ => EditIntentReadResult.FromPilot(PrepareKit.Reread(same, notAdmitted: PushBackPilotIntentReader.OutOfPilotDiagnostic));

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Confirmed, outcome.Status);
        Assert.Empty(outcome.Differences);
        Assert.Equal(PushBackPilotIntentReader.OutOfPilotDiagnostic, outcome.AdmissionDiagnostic);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void ConfirmOfARackThatIsNoLongerEditableWithAnotherBaseIsAConflictWithBothThings()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        var other = EditIntentKit.MemberBecameBlocking(RackA).PilotRead!.StaleBase;
        rig.Reader.Answer = _ => EditIntentReadResult.FromPilot(PrepareKit.Reread(other, notAdmitted: PushBackPilotIntentReader.OutOfPilotDiagnostic));

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Conflict, outcome.Status);
        Assert.NotEmpty(outcome.Differences);
        Assert.Equal(PushBackPilotIntentReader.OutOfPilotDiagnostic, outcome.AdmissionDiagnostic);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
    }

    [Fact]
    public void ADiagnosticAndAHintOnlyComeWithAReadThatWasNotAdmitted()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);

        var match = rig.Core.TryConfirmPilotBase(rig.Request());
        Assert.Equal(EditConfirmStatus.Confirmed, match.Status);
        Assert.Null(match.AdmissionDiagnostic);
        Assert.Null(match.ClassicEditorHint);

        rig.Reader.Answer = rack => OriginChanged(rack, (ScaleFrom, ScaleTo));
        var conflict = rig.Core.TryConfirmPilotBase(rig.Request());
        Assert.Equal(EditConfirmStatus.Conflict, conflict.Status);
        Assert.Null(conflict.AdmissionDiagnostic);
        Assert.Null(conflict.ClassicEditorHint);
    }

    // ------------------------------------------------------------------ scenario 7: an envelope that cannot be read

    private static EditIntentReadResult Unreadable(string rack) => EditIntentKit.ReadOf(IntentKit.Scan(
        rack,
        "A1",
        IntentKit.Unreadable("A1", rack),
        IntentKit.Member("B2", Lateral(rack))));

    [Fact]
    public void ConfirmOfAnUnreadableEnvelopeCarriesTheDiagnosticOfAnUnreadableOrFutureEnvelopeAndTheHint()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        rig.Reader.Answer = Unreadable;

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Conflict, outcome.Status);
        Assert.Equal(PushBackPilotIntentReader.UnreadableDiagnostic, outcome.AdmissionDiagnostic);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
    }

    [Fact]
    public void KeepAndDiscardOfAnUnreadableEnvelopeAnswerNotAdmittedWithItsDiagnosticAndChangeNothing()
    {
        foreach (var operation in new[] { "keep", "discard" })
        {
            var rig = new Rig();
            var draft = rig.Open();
            Rig.Edit(draft);
            draft.MarkPossiblyStale();
            rig.Reader.Answer = Unreadable;
            var before = Hold(draft);

            var outcome = operation == "keep" ? rig.Core.TryKeepPilotEdit(rig.Request()) : rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

            Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
            Assert.Equal(PushBackPilotIntentReader.UnreadableDiagnostic, outcome.Diagnostic);
            Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
            AssertHeld(before, draft);
        }
    }

    // ------------------------------------------------------------------ scenario 8: an exception of the read or of the comparison

    [Fact]
    public void ADefaultReadResultAnswersFailedForTheThreeOperationsAndChangesNothing()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        rig.Reader.Answer = _ => default;
        var before = Hold(draft);

        var confirm = rig.Core.TryConfirmPilotBase(rig.Request());
        var keep = rig.Core.TryKeepPilotEdit(rig.Request());
        var discard = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

        Assert.Equal(EditConfirmStatus.Failed, confirm.Status);
        Assert.Equal(EditIntentStatus.Failed, keep.Status);
        Assert.Equal(EditIntentStatus.Failed, discard.Status);
        Assert.Equal(3, rig.Reader.Calls.Count);
        AssertHeld(before, draft);
        Assert.Equal(WorkspaceHostStatus.Ready, rig.Core.GetSnapshot().Status);
    }

    [Fact]
    public void AnExceptionInsideTheComparisonOfTheGeneralCheckIsFailedReportedAndChangesNothing()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        // A base that was never constructed: comparing against it throws inside ConfirmBase, after the one read.
        var broken = (DraftStaleBase)RuntimeHelpers.GetUninitializedObject(typeof(DraftStaleBase));
        rig.Reader.Answer = _ => EditIntentReadResult.FromPilot(PrepareKit.Reread(broken));
        var before = Hold(draft);

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Failed, outcome.Status);
        Assert.False(string.IsNullOrWhiteSpace(outcome.Reason));
        Assert.Null(outcome.AdmissionDiagnostic);
        Assert.Null(outcome.ClassicEditorHint);
        AssertHeld(before, draft);
        var fault = Assert.Single(rig.Host.Faults);
        Assert.Equal("WorkspaceHostCore.TryConfirmPilotBase", fault.Context);
        Assert.False(rig.Host.Bridge.Core.IsDegraded(rig.Session));
        Assert.Single(rig.Reader.Calls);
    }

    // ------------------------------------------------------------------ scenario 13: an invalid pending text and the three operations

    private const string InvalidText = "  1:5x \t";

    [Fact]
    public void AnInvalidPendingTextDoesNotBlockTheThreeOperationsConfirmAndKeepLeaveItExactlyAsTypedAndDiscardEmptiesIt()
    {
        var confirmRig = new Rig();
        var confirmDraft = confirmRig.Open();
        Rig.Edit(confirmDraft);
        confirmDraft.SetPendingText("field", InvalidText);
        var before = Hold(confirmDraft);
        Assert.Equal(EditConfirmStatus.Confirmed, confirmRig.Core.TryConfirmPilotBase(confirmRig.Request()).Status);
        AssertHeld(before, confirmDraft);
        Assert.Equal(InvalidText, confirmDraft.PendingText["field"]);

        var keepRig = new Rig();
        var keepDraft = keepRig.Open();
        Rig.Edit(keepDraft);
        keepDraft.SetPendingText("field", InvalidText);
        keepDraft.MarkPossiblyStale();
        keepRig.Reader.Answer = rack => OriginChanged(rack, (ToleranceFrom, ToleranceTo));
        Assert.Equal(EditIntentStatus.Kept, keepRig.Core.TryKeepPilotEdit(keepRig.Request()).Status);
        Assert.Equal(InvalidText, keepDraft.PendingText["field"]);
        Assert.Equal(2.25, keepDraft.CurrentDraftState.Inputs.Annotations.AnnotationScale);

        var discardRig = new Rig();
        var discardDraft = discardRig.Open();
        Rig.Edit(discardDraft);
        discardDraft.SetPendingText("field", InvalidText);
        discardRig.Reader.Answer = rack => OriginChanged(rack, (ToleranceFrom, ToleranceTo));
        Assert.Equal(EditIntentStatus.Reloaded, discardRig.Core.TryDiscardAndReloadPilotEdit(discardRig.Request()).Status);
        Assert.False(discardDraft.HasPendingText);
    }

    [Fact]
    public void ConfirmOfAConflictWithAnInvalidPendingTextKeepsTheTextTheConflictAndTheDifferences()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.SetPendingText("field", InvalidText);
        rig.Reader.Answer = rack => OriginChanged(rack, (ScaleFrom, ScaleTo));

        var first = rig.Core.TryConfirmPilotBase(rig.Request());
        var second = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Conflict, first.Status);
        Assert.Equal(EditConfirmStatus.Conflict, second.Status);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Equal(InvalidText, draft.PendingText["field"]);
        Assert.Equal(2, rig.Reader.Calls.Count);
    }

    // ------------------------------------------------------------------ scenarios 14 and 15: the session and the document

    [Fact]
    public void ASessionThatNeverExistedIsRejectedForTheThreeOperationsWithoutReadingAndWithoutChange()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        var before = Hold(draft);
        var never = new SessionId(987654);

        var confirm = rig.Core.TryConfirmPilotBase(rig.Request(session: never));
        var keep = rig.Core.TryKeepPilotEdit(rig.Request(session: never));
        var discard = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request(session: never));

        Assert.Equal(EditConfirmStatus.Rejected, confirm.Status);
        Assert.Equal(EditIntentStatus.Rejected, keep.Status);
        Assert.Equal(EditIntentStatus.Rejected, discard.Status);
        Assert.Equal("La petición no es del documento activo.", confirm.Reason);
        Assert.Equal("La petición no es del documento activo.", keep.Reason);
        Assert.Equal("La petición no es del documento activo.", discard.Reason);
        Assert.Empty(rig.Reader.Calls);
        AssertHeld(before, draft);
    }

    [Fact]
    public void AnotherActiveDocumentRejectsTheThreeOperationsWithoutReadingAndLeavesTheBaseTheInitialTheCurrentAndThePendingTextIntact()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.SetPendingText("field", InvalidText);
        draft.MarkPossiblyStale();
        var before = Hold(draft);
        var first = rig.Session;

        rig.Host.Activate(200);
        rig.Core.RefreshPanel();
        rig.Host.Idle();
        Assert.NotEqual(first, rig.Host.ActiveSession);

        var confirm = rig.Core.TryConfirmPilotBase(rig.Request());
        var keep = rig.Core.TryKeepPilotEdit(rig.Request());
        var discard = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

        Assert.Equal(EditConfirmStatus.Rejected, confirm.Status);
        Assert.Equal(EditIntentStatus.Rejected, keep.Status);
        Assert.Equal(EditIntentStatus.Rejected, discard.Status);
        Assert.Empty(rig.Reader.Calls);
        AssertHeld(before, draft);
        Assert.Equal(InvalidText, draft.PendingText["field"]);
    }

    // ------------------------------------------------------------------ the value of the answer (INV-F4T4A2-10 and 16)

    [Fact]
    public void TheFactoriesOfTheAnswerCarryTheDiagnosticAndTheHintOnlyWhereTheyAreGivenAndTheOtherAnswersCarryNone()
    {
        var withBoth = EditConfirmOutcome.Confirmed("diagnostic", "hint");
        Assert.Equal("diagnostic", withBoth.AdmissionDiagnostic);
        Assert.Equal("hint", withBoth.ClassicEditorHint);

        var conflict = EditConfirmOutcome.Conflict(new[] { "A1:EnvelopeText" }, "diagnostic", "hint");
        Assert.Equal(EditConfirmStatus.Conflict, conflict.Status);
        Assert.Equal("diagnostic", conflict.AdmissionDiagnostic);
        Assert.Equal("hint", conflict.ClassicEditorHint);
        Assert.Equal(new[] { "A1:EnvelopeText" }, conflict.Differences.ToArray());

        foreach (var plain in new[]
                 {
                     EditConfirmOutcome.Confirmed(), EditConfirmOutcome.Conflict(new[] { "k:Membership" }), EditConfirmOutcome.Failed("x"),
                     EditConfirmOutcome.Unavailable("x"), EditConfirmOutcome.Waiting("x"), EditConfirmOutcome.Rejected("x"),
                     EditConfirmOutcome.NotFound("x"), EditConfirmOutcome.NotApplicable("x"), default,
                 })
        {
            Assert.Null(plain.AdmissionDiagnostic);
            Assert.Null(plain.ClassicEditorHint);
        }

        Assert.Equal("diagnostic", EditConfirmOutcome.Confirmed("diagnostic").AdmissionDiagnostic);
        Assert.Null(EditConfirmOutcome.Confirmed("diagnostic").ClassicEditorHint);
        Assert.Throws<ArgumentNullException>(() => EditConfirmOutcome.Conflict(null!, "d", "h"));
    }

    [Fact]
    public void TheAnswerOfTheGeneralCheckOnlyCarriesAStateTextAndListsOfTextSoItCannotAuthorizeAWrite()
    {
        var allowed = new[] { typeof(EditConfirmStatus), typeof(string), typeof(IReadOnlyList<string>) };
        var properties = typeof(EditConfirmOutcome).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var fields = typeof(EditConfirmOutcome).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.All(properties, p => Assert.Contains(p.PropertyType, allowed));
        Assert.All(fields.Where(f => !f.Name.Contains("k__BackingField", StringComparison.Ordinal)), f => Assert.Contains(f.FieldType, allowed));
        Assert.All(fields.Where(f => f.Name.Contains("k__BackingField", StringComparison.Ordinal)), f => Assert.Contains(f.FieldType, allowed));
    }

    [Fact]
    public void TheThreeOperationsStillReadTheDrawingExactlyOnceAndTheGeneralCheckStillComparesWithTheBaseOfTheDraft()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();

        var confirm = Record.Exception(() => rig.Core.TryConfirmPilotBase(rig.Request()));
        Assert.Null(confirm);
        Assert.Single(rig.Reader.Calls);
        rig.Reader.Calls.Clear();

        rig.Reader.Answer = rack => OriginChanged(rack, (ToleranceFrom, ToleranceTo));
        draft.MarkPossiblyStale();
        Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(rig.Request()).Status);
        Assert.Single(rig.Reader.Calls);
        rig.Reader.Calls.Clear();

        rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());
        Assert.Single(rig.Reader.Calls);
    }
}
