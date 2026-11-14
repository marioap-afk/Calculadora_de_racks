#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4-T4A1-PREPARE-CORE: the builders of the scans, the drafts and the re-reads of the tests of the read-only PREPARE of an update.
/// The realistic racks have a frontal view with section 0, a lateral one on an existing post and a plan view: the fixture envelopes carry
/// a frontal view with section -1, which the descriptor codec of Push Back does not accept (RACKEDITAR would abort on it too).
/// </summary>
internal static class PrepareKit
{
    public const string Rack = IntentKit.Rack;

    public static string Frontal(string fixture = "pb-01-base", int section = 0, string? name = null) =>
        IntentKit.Variant(fixture, Rack, view: "frontal", section: section, name: name);

    public static string Lateral(int post, string fixture = "pb-01-base", string? name = null) =>
        IntentKit.Variant(fixture, Rack, view: "lateral", section: post, name: name);

    public static string Planta(string fixture = "pb-01-base", string? name = null) =>
        IntentKit.Variant(fixture, Rack, view: "planta", section: -1, name: name);

    /// <summary>The first post that has a lateral cut in the system of pb-01-base: a lateral view there is available.</summary>
    public static int ExistingPost()
    {
        var state = PilotKit.State("pb-01-base");
        var design = new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(state.Editor, state.Inputs);
        var system = new PushBackResolver(PilotKit.Catalog).Resolve(design);
        return new PushBackSystemLateralBuilder().Cortes(system, PilotKit.Catalog).First().PostIndex;
    }

    public const int MissingPost = 999;

    public static PushBackPilotIntentScan Scan(params IntentItem[] members) => IntentKit.Scan(Rack, "A1", members);

    /// <summary>The usual realistic rack: the frontal view (the origin), a lateral one and the plan view, all of the same design.</summary>
    public static PushBackPilotIntentScan RealScan(string fixture = "pb-01-base") =>
        Scan(
            IntentKit.Member("A1", Frontal(fixture)),
            IntentKit.Member("B2", Lateral(ExistingPost(), fixture)),
            IntentKit.Member("C3", Planta(fixture)));

    public static DraftStaleBase BaseOf(params (string Key, string Text)[] members) =>
        DraftStaleBase.Create(members.Select(m =>
        {
            var embed = new RackEmbedStore().Deserialize(m.Text)!;
            return new DraftStaleBaseEntry(m.Key, DraftMembership.Mutable, embed.View, embed.Section.ToString(CultureInfo.InvariantCulture), m.Text);
        }));

    /// <summary>A result of the read built by hand, so a test can give it any base and any admission.</summary>
    public static PushBackPilotIntentResult Reread(DraftStaleBase staleBase, PushBackPilotDraftState? state = null, string? notAdmitted = null) =>
        new(
            staleBase,
            notAdmitted == null
                ? DraftIntentRead<PushBackPilotDraftState>.Admitted(staleBase, state ?? PilotKit.State("pb-01-base"))
                : DraftIntentRead<PushBackPilotDraftState>.NotAdmitted(notAdmitted),
            "A1",
            null,
            notAdmitted == null ? null : PushBackPilotIntentReader.ClassicEditorHint,
            PushBackPilotPreviewWitnessStatus.NotSupplied);

    public static DraftSession<PushBackPilotDraftState> DraftOf(PushBackPilotIntentResult read, string rack = Rack, SessionId? session = null)
    {
        var begun = new DraftRegistry().Begin(
            session ?? DraftKit.S1, rack, read.Read, PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);
        return begun.Draft!;
    }

    /// <summary>A draft whose base is the given one and whose initial state is materialized from the given origin text.</summary>
    public static DraftSession<PushBackPilotDraftState> DraftOf(DraftStaleBase staleBase, string originText)
    {
        var begun = new DraftRegistry().Begin(
            DraftKit.S1, Rack, DraftIntentRead<PushBackPilotDraftState>.Admitted(staleBase, PilotKit.Admitted(originText)),
            PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);
        return begun.Draft!;
    }

    public static void SetScale(DraftSession<PushBackPilotDraftState> draft, double scale) =>
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = scale;

    public static PushBackPilotPrepareResult Prepare(DraftSession<PushBackPilotDraftState> draft, PushBackPilotIntentResult reread) =>
        PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), reread, PilotKit.Catalog);
}

// I-64 F4-T4A1-PREPARE-CORE (INV-F4T4A1-02, 03, 04, 05, 06, 07, 08, 09, 10, 11): ConfirmBase, the atomic keep, the three operations of the
// host and of the port, the pure PREPARE with its immutable plan, and the zero writes of the drawing. Everything runs in Core with the real
// draft session, the real host and the real planner, and doubles for the reader of the drawing. Every expectation that something throws, or
// does not, uses Record.Exception, so a behavior that is missing fails by ASSERTION and never by an escaped exception.
public class WorkspacePilotPrepareTests
{
    private static readonly SessionId S1 = DraftKit.S1;

    private static DraftStaleBase OtherBase() => DraftKit.Base(
        DraftKit.Mutable("D1"),
        DraftKit.Mutable("D2", "Back", "S2", "{\"v\":3}"),
        DraftKit.Mutable("D3"));

    private static DraftStaleBase YetAnotherBase() => DraftKit.Base(DraftKit.Mutable("D1", text: "{\"v\":9}"));

    // What a draft holds that ConfirmBase must never change.
    private sealed record Held(
        DraftStaleBase Base,
        DraftForm Initial,
        DraftForm Current,
        IReadOnlyDictionary<string, string> Pending,
        string? LastApplyCause,
        IReadOnlyList<string> Warnings,
        bool IsDirty,
        DraftUnknownOrigin? UnknownOrigin,
        string? ErrorReason);

    private static Held Hold(DraftSession<DraftForm> draft) => new(
        draft.StaleBase, draft.InitialDraftState, draft.CurrentDraftState, draft.PendingText, draft.LastApplyCause, draft.Warnings,
        draft.IsDirty, draft.UnknownOrigin, draft.ErrorReason);

    private static void AssertHeld(Held before, DraftSession<DraftForm> draft)
    {
        Assert.Same(before.Base, draft.StaleBase);
        Assert.Equal(before.Initial, draft.InitialDraftState);
        Assert.Equal(before.Current, draft.CurrentDraftState);
        Assert.Equal(before.Pending.ToArray(), draft.PendingText.ToArray());
        Assert.Equal(before.LastApplyCause, draft.LastApplyCause);
        Assert.Equal(before.Warnings.ToArray(), draft.Warnings.ToArray());
        Assert.Equal(before.IsDirty, draft.IsDirty);
        Assert.Equal(before.UnknownOrigin, draft.UnknownOrigin);
        Assert.Equal(before.ErrorReason, draft.ErrorReason);
    }

    // ================================================================== INV-F4T4A1-02: ConfirmBase over the draft session

    [Fact]
    public void ConfirmBaseDoesNotApplyToADraftThatIsNotRegisteredApplyingUnknownOrInError()
    {
        var detached = DraftKit.Born();
        var registry = new DraftRegistry();
        var removed = DraftKit.Born(registry);
        registry.DiscardByUser(DraftKit.S1, "R1");
        var cases = new Dictionary<string, DraftSession<DraftForm>>
        {
            ["Applying"] = DraftKit.ToApplying(DraftKit.Dirty()),
            ["UnknownCommit"] = DraftKit.ToUnknownCommit(DraftKit.Dirty()),
            ["UnknownPost"] = DraftKit.ToUnknownPost(DraftKit.Dirty()),
            ["Error"] = DraftKit.ToError(DraftKit.Dirty()),
            ["Detached"] = removed,
        };
        Assert.False(removed.IsRegistered);
        Assert.True(detached.IsRegistered);

        foreach (var (name, draft) in cases)
        {
            var before = Hold(draft);
            var status = draft.Status;
            var stale = draft.IsPossiblyStale;

            foreach (var reread in new[] { draft.StaleBase, OtherBase() })
            {
                var result = draft.ConfirmBase(reread);

                Assert.Equal(DraftConfirmStatus.NotApplicable, result.Status);
                Assert.Equal(status, result.DraftStatus);
                Assert.Empty(result.Differences);
                Assert.Equal(status, draft.Status);
                Assert.Equal(stale, draft.IsPossiblyStale);
                Assert.Empty(draft.ConflictDifferences);
                AssertHeld(before, draft);
            }

            Assert.True(name.Length > 0);
        }
    }

    [Fact]
    public void AMatchInACleanOrDirtyDraftOnlyClearsThePossiblyStaleMarkAndNeverCleansDirty()
    {
        foreach (var dirty in new[] { false, true })
        {
            var draft = dirty ? DraftKit.Dirty() : DraftKit.Born();
            if (dirty) draft.SetPendingText("name", "typed");
            draft.MarkPossiblyStale();
            var before = Hold(draft);
            var status = draft.Status;
            Assert.Equal(dirty ? DraftStatus.Dirty : DraftStatus.Clean, status);

            var result = draft.ConfirmBase(DraftKit.DefaultBase());

            Assert.Equal(DraftConfirmStatus.Confirmed, result.Status);
            Assert.Equal(status, result.DraftStatus);
            Assert.Empty(result.Differences);
            Assert.False(draft.IsPossiblyStale);
            Assert.Equal(status, draft.Status);
            Assert.Equal(dirty, draft.CurrentDraftState != draft.InitialDraftState);
            Assert.Equal(dirty, draft.HasPendingText);
            AssertHeld(before, draft);
        }
    }

    [Fact]
    public void ADifferenceInACleanOrDirtyDraftPutsItInConflictWithTheOrderedDifferencesAndChangesNothingElse()
    {
        foreach (var dirty in new[] { false, true })
        {
            var draft = dirty ? DraftKit.Dirty() : DraftKit.Born();
            if (dirty) draft.SetPendingText("width", "42");
            draft.MarkPossiblyStale();
            var before = Hold(draft);
            Assert.Equal(dirty ? DraftStatus.Dirty : DraftStatus.Clean, draft.Status);
            var expected = draft.StaleBase.CompareWith(OtherBase()).Differences.ToArray();
            Assert.True(expected.Length > 1);

            var result = draft.ConfirmBase(OtherBase());

            Assert.Equal(DraftConfirmStatus.Conflict, result.Status);
            Assert.Equal(DraftStatus.Conflict, result.DraftStatus);
            Assert.Equal(expected, result.Differences.ToArray());
            Assert.Equal(DraftStatus.Conflict, draft.Status);
            Assert.Equal(expected, draft.ConflictDifferences.ToArray());
            Assert.True(draft.IsPossiblyStale);
            Assert.Equal(dirty, draft.HasPendingText);
            AssertHeld(before, draft);
            // The differences come ordered by key (ordinal), then by kind.
            Assert.Equal(expected.OrderBy(d => d.Key, StringComparer.Ordinal).ThenBy(d => d.Kind).ToArray(), expected);
        }
    }

    [Fact]
    public void AMatchInAConflictAnswersConfirmedAndTheDraftStaysInConflictWithAllItsData()
    {
        var draft = DraftKit.ToConflict(DraftKit.Dirty());
        draft.SetPendingText("name", "typed");
        draft.MarkPossiblyStale();
        var before = Hold(draft);
        var differences = draft.ConflictDifferences.ToArray();

        var result = draft.ConfirmBase(DraftKit.DefaultBase());

        Assert.Equal(DraftConfirmStatus.Confirmed, result.Status);
        Assert.Equal(DraftStatus.Conflict, result.DraftStatus);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Equal(differences, draft.ConflictDifferences.ToArray());
        Assert.True(draft.IsPossiblyStale);
        Assert.True(draft.IsDirty);
        AssertHeld(before, draft);
    }

    [Fact]
    public void ADifferenceInAConflictReplacesTheDifferencesAndTheDraftStaysInConflict()
    {
        var draft = DraftKit.ToConflict(DraftKit.Dirty());
        var before = Hold(draft);
        var old = draft.ConflictDifferences.ToArray();
        var expected = draft.StaleBase.CompareWith(YetAnotherBase()).Differences.ToArray();
        Assert.NotEqual(old, expected);

        var result = draft.ConfirmBase(YetAnotherBase());

        Assert.Equal(DraftConfirmStatus.Conflict, result.Status);
        Assert.Equal(DraftStatus.Conflict, result.DraftStatus);
        Assert.Equal(expected, result.Differences.ToArray());
        Assert.Equal(expected, draft.ConflictDifferences.ToArray());
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        AssertHeld(before, draft);
    }

    [Fact]
    public void ConfirmBaseNeverTakesTheSlotOfTheUpdateAndNeverReadsAnything()
    {
        var registry = new DraftRegistry();
        var first = DraftKit.Dirty(registry, DraftKit.S1, "R1");
        var second = DraftKit.Dirty(registry, DraftKit.S1, "R2");

        var confirmed = first.ConfirmBase(DraftKit.DefaultBase());
        var conflict = first.ConfirmBase(OtherBase());

        Assert.Equal(DraftConfirmStatus.Confirmed, confirmed.Status);
        Assert.Equal(DraftConfirmStatus.Conflict, conflict.Status);
        Assert.False(registry.HasUpdateInProgress);
        // The other draft can still take the one slot of the registry, and the first one is in Conflict so it cannot.
        Assert.True(second.TryBeginApply().IsStarted);
        Assert.True(registry.HasUpdateInProgress);
    }

    [Fact]
    public void ConfirmBaseWithoutAReReadThrowsAndChangesNothing()
    {
        var draft = DraftKit.Dirty();
        var before = Hold(draft);

        var thrown = Record.Exception(() => draft.ConfirmBase(null!));

        Assert.IsType<ArgumentNullException>(thrown);
        AssertHeld(before, draft);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void ConfirmBaseDoesNotChangeWhatCheckNowDoesForAnUnknownDraft()
    {
        var draft = DraftKit.ToUnknownCommit(DraftKit.Dirty());

        Assert.Equal(DraftConfirmStatus.NotApplicable, draft.ConfirmBase(OtherBase()).Status);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
        // CheckNow still serves it, exactly as before.
        Assert.Equal(DraftCheckStatus.Recovered, draft.CheckNow(DraftKit.DefaultBase()).Status);
    }

    // ================================================================== INV-F4T4A1-03: the atomic keep

    private static DraftSession<DraftForm> Keepable(bool conflict, DraftRegistry? registry = null)
    {
        var draft = DraftKit.Dirty(registry);
        if (conflict) DraftKit.ToConflict(draft);
        draft.SetPendingText("name", "typed");
        draft.MarkPossiblyStale();
        return draft;
    }

    private static DraftIntentRead<DraftForm> NewRead(string name = "Newer", int width = 55) =>
        DraftIntentRead<DraftForm>.Admitted(OtherBase(), DraftKit.Form(name, width, "{new}"));

    [Fact]
    public void KeepWithANullKeepOnThrowsBeforeAnythingElseAndChangesNothing()
    {
        var draft = Keepable(conflict: true);
        var before = Hold(draft);

        var thrown = Assert.IsType<ArgumentNullException>(Record.Exception(() => draft.KeepMineOnCurrentVersion(NewRead(), null!, confirmed: true)));
        var withoutRead = Assert.IsType<ArgumentNullException>(Record.Exception(() => draft.KeepMineOnCurrentVersion(null!, null!, confirmed: false)));

        Assert.Equal("keepOn", thrown.ParamName);
        Assert.Equal("keepOn", withoutRead.ParamName);
        AssertHeld(before, draft);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
    }

    [Fact]
    public void KeepChecksNotAllowedThenNotConfirmedThenNotAdmittedInTheOrderOfTheOtherVersionAndNeverCallsKeepOnWithoutAdmission()
    {
        var calls = 0;
        Func<DraftForm, DraftForm, DraftForm> keepOn = (initial, current) =>
        {
            calls++;
            return current;
        };

        var clean = DraftKit.Born();
        var cleanBefore = Hold(clean);
        var dirty = DraftKit.Dirty();
        var dirtyBefore = Hold(dirty);
        var notAdmitted = DraftIntentRead<DraftForm>.NotAdmitted("not admitted");

        // 1. Not allowed: a clean draft, and one that is Applying, whatever the confirmation and the admission.
        Assert.Equal(DraftReloadStatus.NotAllowed, clean.KeepMineOnCurrentVersion(NewRead(), keepOn, confirmed: true).Status);
        Assert.Equal(DraftReloadStatus.NotAllowed, clean.KeepMineOnCurrentVersion(notAdmitted, keepOn, confirmed: false).Status);
        var applying = DraftKit.ToApplying(DraftKit.Dirty());
        Assert.Equal(DraftReloadStatus.NotAllowed, applying.KeepMineOnCurrentVersion(NewRead(), keepOn, confirmed: true).Status);
        // 2. Not confirmed, before the admission is looked at.
        Assert.Equal(DraftReloadStatus.NotConfirmed, dirty.KeepMineOnCurrentVersion(notAdmitted, keepOn, confirmed: false).Status);
        Assert.Equal(DraftReloadStatus.NotConfirmed, dirty.KeepMineOnCurrentVersion(NewRead(), keepOn, confirmed: false).Status);
        // 3. Not admitted: the answer carries the diagnostic.
        var refused = dirty.KeepMineOnCurrentVersion(notAdmitted, keepOn, confirmed: true);
        Assert.Equal(DraftReloadStatus.NotAdmitted, refused.Status);
        Assert.Equal("not admitted", refused.Admission.Diagnostic);

        Assert.Equal(0, calls);
        AssertHeld(cleanBefore, clean);
        AssertHeld(dirtyBefore, dirty);
    }

    [Fact]
    public void KeepReplacesTheBaseTheInitialAndTheCurrentStateInOneStepAndKeepsThePendingTextAndTheEdits()
    {
        foreach (var conflict in new[] { false, true })
        {
            var draft = Keepable(conflict);
            var oldCurrent = draft.CurrentDraftState;
            var read = NewRead();
            DraftForm? firstArgument = null;
            DraftForm? secondArgument = null;
            var calls = 0;
            var result = draft.KeepMineOnCurrentVersion(
                read,
                (initial, current) =>
                {
                    calls++;
                    firstArgument = initial;
                    secondArgument = current;
                    return current with { Name = initial.Name + "+" + current.Name, Width = initial.Width };
                },
                confirmed: true);

            Assert.Equal(DraftReloadStatus.Reloaded, result.Status);
            Assert.Equal(1, calls);
            // keepOn receives a COPY of the new initial state and the current state itself, and what it returns is copied again.
            Assert.NotSame(read.InitialState, firstArgument);
            Assert.Equal(read.InitialState, firstArgument);
            Assert.Same(oldCurrent, secondArgument);
            Assert.Equal(new DraftForm("Newer+Edited", 55, oldCurrent.Source), draft.CurrentDraftState);
            Assert.NotSame(secondArgument, draft.CurrentDraftState);
            Assert.Equal(read.InitialState, draft.InitialDraftState);
            Assert.NotSame(read.InitialState, draft.InitialDraftState);
            Assert.Same(read.StaleBase, draft.StaleBase);
            Assert.False(draft.IsPossiblyStale);
            Assert.Empty(draft.ConflictDifferences);
            Assert.Equal("typed", draft.PendingText["name"]);
            Assert.True(draft.IsDirty);
            Assert.Equal(DraftStatus.Dirty, draft.Status);
        }
    }

    [Fact]
    public void AKeepOnThatThrowsOrACopyThatThrowsLeavesTheDraftExactlyAsItWas()
    {
        // The copier fails at its n-th call after it is armed: the two copies of the initial state and the copy of the substitute are
        // all made before anything is assigned.
        foreach (var failAt in new[] { 1, 2, 3 })
        {
            var armed = 0;
            var counting = false;
            DraftForm Copy(DraftForm form)
            {
                if (counting && ++armed == failAt) throw new InvalidOperationException("copy-boom");
                return form with { };
            }

            var registry = new DraftRegistry();
            var draft = registry.Begin(DraftKit.S1, "R1", DraftKit.Read(), DraftKit.Comparer, Copy).Draft!;
            draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "Edited" });
            DraftKit.ToConflict(draft);
            draft.SetPendingText("name", "typed");
            draft.MarkPossiblyStale();
            var before = Hold(draft);
            counting = true;

            var thrown = Record.Exception(() => draft.KeepMineOnCurrentVersion(NewRead(), (initial, current) => current, confirmed: true));

            Assert.NotNull(thrown);
            Assert.Equal("copy-boom", thrown!.Message);
            counting = false;
            AssertHeld(before, draft);
            Assert.Equal(DraftStatus.Conflict, draft.Status);
            Assert.True(draft.IsPossiblyStale);
            Assert.NotEmpty(draft.ConflictDifferences);
        }

        var plain = Keepable(conflict: true);
        var plainBefore = Hold(plain);
        var failure = Record.Exception(() => plain.KeepMineOnCurrentVersion(NewRead(), (initial, current) => throw new InvalidOperationException("keep-boom"), confirmed: true));

        Assert.Equal("keep-boom", failure?.Message);
        AssertHeld(plainBefore, plain);
        Assert.Equal(DraftStatus.Conflict, plain.Status);
        Assert.NotEmpty(plain.ConflictDifferences);
    }

    [Fact]
    public void TheVersionOfTwoParametersStillKeepsTheCurrentStateAsItWas()
    {
        var draft = Keepable(conflict: true);
        var current = draft.CurrentDraftState;

        var result = draft.KeepMineOnCurrentVersion(NewRead(), confirmed: true);

        Assert.Equal(DraftReloadStatus.Reloaded, result.Status);
        Assert.Same(current, draft.CurrentDraftState);
        Assert.Equal("Edited", draft.CurrentDraftState.Name);
    }

    // ================================================================== INV-F4T4A1-04 and 05: the operations of the host and of the port

    private const string RackA = EditIntentKit.RackA;

    private sealed class Rig
    {
        public Rig(bool assignReader = true)
        {
            Host = new WorkspaceHostRig().WithDocument(100);
            Host.Host.OpenPanel();
            Session = Host.ActiveSession;
            Host.Idle();
            Host.Reader.Reads.Clear();
            Reader = new EditIntentFakeReader();
            if (assignReader) Host.Host.SetEditIntentReader(Reader);
        }

        public WorkspaceHostRig Host { get; }

        public WorkspaceHostCore Core => Host.Host;

        public IWorkspacePilotEditPort Port => Host.Host;

        public EditIntentFakeReader Reader { get; }

        public SessionId Session { get; }

        public EditIntentRequest Request(string rackId = RackA, SessionId? session = null, EditIntentWitness? witness = null) =>
            new(session ?? Session, rackId, null, witness);

        public DraftSession<PushBackPilotDraftState> Open(string rackId = RackA)
        {
            Assert.Equal(EditIntentStatus.Admitted, Core.TryBeginPilotEdit(Request(rackId)).Status);
            Reader.Calls.Clear();
            return Core.OpenDrafts.Find<PushBackPilotDraftState>(Session, rackId)!;
        }

        public static void Edit(DraftSession<PushBackPilotDraftState> draft, double scale = 2.25) =>
            draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = scale;

        public PilotFieldView Field(string rackId = RackA) => Port.GetField(new WorkspaceRequest(Session), rackId);
    }

    /// <summary>A read of the same rack, admitted, whose members changed: its base differs and its origin is another design.</summary>
    private static EditIntentReadResult Changed(string rack) => EditIntentKit.ReadOf(IntentKit.Scan(
        rack,
        "A1",
        IntentKit.Member("A1", IntentKit.Variant("pb-02-modulos", rack)),
        IntentKit.Member("B2", IntentKit.Variant("pb-02-modulos", rack, view: "lateral", section: 3))));

    /// <summary>The same base as <see cref="EditIntentKit.Admitted"/> but with an origin that is not in the scan: not admitted.</summary>
    private static EditIntentReadResult SameBaseNotAdmitted(string rack) => EditIntentKit.ReadOf(IntentKit.Scan(
        rack,
        "ZZ",
        IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rack)),
        IntentKit.Member("B2", IntentKit.Variant("pb-01-base", rack, view: "lateral", section: 3))));

    // ------------------------------------------------------------------ the request and the gate: nothing is read

    [Fact]
    public void TheThreeOperationsAnswerUnavailableWithoutAReaderAndReadNothing()
    {
        var rig = new Rig(assignReader: false);

        Assert.Equal(EditConfirmStatus.Unavailable, rig.Core.TryConfirmPilotBase(rig.Request()).Status);
        Assert.Equal(EditIntentStatus.Unavailable, rig.Core.TryKeepPilotEdit(rig.Request()).Status);
        Assert.Equal(EditIntentStatus.Unavailable, rig.Core.TryDiscardAndReloadPilotEdit(rig.Request()).Status);
        Assert.Empty(rig.Reader.Calls);
    }

    [Fact]
    public void ABlankRackIdIsAnsweredBeforeAnythingElseWithTheTextsOfTheOtherOperationsAndReadsNothing()
    {
        foreach (var assign in new[] { true, false })
        {
            foreach (var blank in new[] { "", " ", "\t" })
            {
                var rig = new Rig(assign);

                var confirm = rig.Core.TryConfirmPilotBase(rig.Request(blank));
                var keep = rig.Core.TryKeepPilotEdit(rig.Request(blank));
                var discard = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request(blank));

                Assert.Equal(EditConfirmStatus.NotFound, confirm.Status);
                Assert.Equal("Un rack sin identificador no tiene borrador.", confirm.Reason);
                foreach (var outcome in new[] { keep, discard })
                {
                    Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
                    Assert.Equal(PushBackPilotIntentReader.NoIdentityDiagnostic, outcome.Diagnostic);
                    Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
                    Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
                }

                Assert.Empty(rig.Reader.Calls);
                Assert.Empty(rig.Host.Faults);
            }
        }
    }

    [Fact]
    public void AWitnessWithoutAKeyIsAFailureOfTheRequestBeforeTheGateTheDraftAndTheReader()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        var witness = new EditIntentWitness(" ", "text");

        var confirm = rig.Core.TryConfirmPilotBase(rig.Request(witness: witness));
        var keep = rig.Core.TryKeepPilotEdit(rig.Request(witness: witness));
        var discard = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request(witness: witness));

        Assert.Equal(EditConfirmStatus.Failed, confirm.Status);
        Assert.Equal("El testigo de la vista previa no tiene clave.", confirm.Reason);
        Assert.Equal(EditIntentStatus.Failed, keep.Status);
        Assert.Equal("El testigo de la vista previa no tiene clave.", keep.Reason);
        Assert.Equal(EditIntentStatus.Failed, discard.Status);
        Assert.Equal("El testigo de la vista previa no tiene clave.", discard.Reason);
        Assert.Empty(rig.Reader.Calls);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void TheGateAnswersWaitingOrRejectedForTheThreeOperationsAndReadsNothing()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);

        void Check(string reason, EditConfirmStatus confirmStatus, EditIntentStatus intentStatus)
        {
            var confirm = rig.Core.TryConfirmPilotBase(rig.Request());
            var keep = rig.Core.TryKeepPilotEdit(rig.Request());
            var discard = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

            Assert.Equal(confirmStatus, confirm.Status);
            Assert.Equal(reason, confirm.Reason);
            Assert.Equal(intentStatus, keep.Status);
            Assert.Equal(reason, keep.Reason);
            Assert.Equal(intentStatus, discard.Status);
            Assert.Equal(reason, discard.Reason);
            Assert.Empty(rig.Reader.Calls);
            Assert.Equal(DraftStatus.Dirty, draft.Status);
        }

        rig.Host.Probe.ActiveDocumentNativeId = null;
        Check("No hay documento activo.", EditConfirmStatus.Waiting, EditIntentStatus.Waiting);
        rig.Host.Probe.ActiveDocumentNativeId = 100;

        rig.Core.SetModalRackCadActive(true);
        Check("Hay una ventana de RackCad abierta.", EditConfirmStatus.Waiting, EditIntentStatus.Waiting);
        rig.Core.SetModalRackCadActive(false);

        rig.Host.CommandStarts(rig.Session);
        Check("Hay un comando en curso.", EditConfirmStatus.Waiting, EditIntentStatus.Waiting);
        rig.Host.CommandEnds(rig.Session);

        var first = rig.Session;
        rig.Host.Activate(200);
        rig.Core.RefreshPanel();
        rig.Host.Idle();
        Assert.NotEqual(first, rig.Host.ActiveSession);
        Check("La petición no es del documento activo.", EditConfirmStatus.Rejected, EditIntentStatus.Rejected);
    }

    [Fact]
    public void AMissingDraftIsNotFoundWithoutReading()
    {
        var rig = new Rig();

        var confirm = rig.Core.TryConfirmPilotBase(rig.Request());
        var keep = rig.Core.TryKeepPilotEdit(rig.Request());
        var discard = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

        Assert.Equal(EditConfirmStatus.NotFound, confirm.Status);
        Assert.Equal("No hay borrador para el rack.", confirm.Reason);
        Assert.Equal(EditIntentStatus.NotFound, keep.Status);
        Assert.Equal("No hay borrador para el rack.", keep.Reason);
        Assert.Equal(EditIntentStatus.NotFound, discard.Status);
        Assert.Equal("No hay borrador para el rack.", discard.Reason);
        Assert.Empty(rig.Reader.Calls);
    }

    // ------------------------------------------------------------------ the applicability, decided BEFORE reading

    private static IEnumerable<(string Name, Action<Rig, DraftSession<PushBackPilotDraftState>> Put)> Statuses()
    {
        yield return ("Clean", (rig, draft) => { });
        yield return ("Dirty", (rig, draft) => Rig.Edit(draft));
        yield return ("DirtyStale", (rig, draft) => { Rig.Edit(draft); draft.MarkPossiblyStale(); });
        yield return ("Conflict", (rig, draft) => { Rig.Edit(draft); DraftToConflict(draft); });
        yield return ("Applying", (rig, draft) => { Rig.Edit(draft); Assert.True(draft.TryBeginApply().IsStarted); });
        yield return ("UnknownCommit", (rig, draft) =>
        {
            Rig.Edit(draft);
            Assert.True(draft.TryBeginApply().IsStarted);
            draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the commit threw"));
        });
        yield return ("UnknownPost", (rig, draft) =>
        {
            Rig.Edit(draft);
            Assert.True(draft.TryBeginApply().IsStarted);
            draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.AppliedPostRereadFailed("the post reread failed"));
        });
        yield return ("Error", (rig, draft) => { Rig.Edit(draft); draft.EnterError("the surface failed"); });
    }

    private static void DraftToConflict(DraftSession<PushBackPilotDraftState> draft)
    {
        Assert.True(draft.TryBeginApply().IsStarted);
        draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedConflict(new[] { new DraftDifference("A1", DraftDifferenceKind.EnvelopeText) }));
        Assert.Equal(DraftStatus.Conflict, draft.Status);
    }

    public static TheoryData<string> StatusNames()
    {
        var data = new TheoryData<string>();
        foreach (var status in Statuses()) data.Add(status.Name);
        return data;
    }

    [Theory]
    [MemberData(nameof(StatusNames))]
    public void ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing(string name)
    {
        var rig = new Rig();
        var draft = rig.Open();
        Statuses().Single(s => s.Name == name).Put(rig, draft);
        var applies = name is "Clean" or "Dirty" or "DirtyStale" or "Conflict";

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        if (applies)
        {
            Assert.Equal(EditConfirmStatus.Confirmed, outcome.Status);
            Assert.Single(rig.Reader.Calls);
        }
        else
        {
            Assert.Equal(EditConfirmStatus.NotApplicable, outcome.Status);
            Assert.Equal("El borrador no admite comprobación en su estado actual.", outcome.Reason);
            Assert.Empty(rig.Reader.Calls);
        }
    }

    [Theory]
    [MemberData(nameof(StatusNames))]
    public void KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading(string name)
    {
        var rig = new Rig();
        var draft = rig.Open();
        Statuses().Single(s => s.Name == name).Put(rig, draft);
        rig.Reader.Answer = Changed;

        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());

        switch (name)
        {
            case "DirtyStale":
            case "Conflict":
                Assert.Equal(EditIntentStatus.Kept, outcome.Status);
                Assert.Single(rig.Reader.Calls);
                break;
            case "Applying":
                Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
                Assert.Equal("El borrador se está aplicando o ya no está registrado.", outcome.Reason);
                Assert.Empty(rig.Reader.Calls);
                break;
            default:
                Assert.Equal(EditIntentStatus.NotNeeded, outcome.Status);
                Assert.Equal("Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.", outcome.Reason);
                Assert.Empty(rig.Reader.Calls);
                break;
        }
    }

    [Theory]
    [MemberData(nameof(StatusNames))]
    public void DiscardAndReloadAppliesToEveryStatusButApplyingWhichWaitsAllWithTheOneRead(string name)
    {
        var rig = new Rig();
        var draft = rig.Open();
        Statuses().Single(s => s.Name == name).Put(rig, draft);

        var outcome = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

        if (name == "Applying")
        {
            Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
            Assert.Equal("El borrador se está aplicando o ya no está registrado.", outcome.Reason);
            Assert.Empty(rig.Reader.Calls);
            Assert.Equal(DraftStatus.Applying, draft.Status);
        }
        else
        {
            Assert.Equal(EditIntentStatus.Reloaded, outcome.Status);
            Assert.Single(rig.Reader.Calls);
            Assert.Equal(DraftStatus.Clean, draft.Status);
            Assert.False(draft.IsDirty);
            Assert.False(draft.IsPossiblyStale);
            Assert.False(draft.HasPendingText);
            Assert.Null(draft.UnknownOrigin);
            Assert.Null(draft.ErrorReason);
        }
    }

    // ------------------------------------------------------------------ the read: one, with the mark, and the mapping of what it answers

    [Fact]
    public void EachOperationReadsOnceAndSetsTheMarkOfReadInProgressAroundTheReadAndResetsItAfterwards()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        var nested = new List<(string Operation, string Status, string? Reason)>();
        rig.Reader.Answer = rack =>
        {
            var confirm = rig.Port.TryConfirmBase(rig.Request());
            var keep = rig.Port.TryKeepMine(rig.Request());
            var discard = rig.Port.TryDiscardAndReload(rig.Request());
            var begin = rig.Port.TryBeginEdit(rig.Request());
            nested.Add(("confirm", confirm.Status.ToString(), confirm.Reason));
            nested.Add(("keep", keep.Status.ToString(), keep.Reason));
            nested.Add(("discard", discard.Status.ToString(), discard.Reason));
            nested.Add(("begin", begin.Status.ToString(), begin.Reason));
            return EditIntentKit.Admitted(rack);
        };

        var results = new string[3];
        results[0] = rig.Port.TryConfirmBase(rig.Request()).Status.ToString();
        draft.MarkPossiblyStale();
        results[1] = rig.Port.TryKeepMine(rig.Request()).Status.ToString();
        results[2] = rig.Port.TryDiscardAndReload(rig.Request()).Status.ToString();

        // Each of the three outer calls read once; the nested ones answered Waiting without reading and without effect.
        Assert.Equal(3, rig.Reader.Calls.Count);
        Assert.Equal(12, nested.Count);
        Assert.All(nested, n =>
        {
            Assert.Equal("Waiting", n.Status);
            Assert.Equal("Hay una lectura del piloto en curso.", n.Reason);
        });
        Assert.Equal(new[] { "Confirmed", "Kept", "Reloaded" }, results);
        // The mark is reset afterwards: the next call reads again.
        rig.Reader.Answer = EditIntentKit.Admitted;
        Assert.Equal(EditConfirmStatus.Confirmed, rig.Port.TryConfirmBase(rig.Request()).Status);
        Assert.Equal(4, rig.Reader.Calls.Count);
    }

    [Fact]
    public void AnExceptionOfTheReaderIsFailedWithTheMessageReportedAndTheMarkAndTheSessionAreNotTouched()
    {
        foreach (var operation in new[] { "confirm", "keep", "discard" })
        {
            var rig = new Rig();
            var draft = rig.Open();
            Rig.Edit(draft);
            draft.MarkPossiblyStale();
            rig.Reader.Throw = new InvalidOperationException("the reader threw");

            string? message;
            string context;
            switch (operation)
            {
                case "confirm":
                    var confirm = rig.Port.TryConfirmBase(rig.Request());
                    Assert.Equal(EditConfirmStatus.Failed, confirm.Status);
                    message = confirm.Reason;
                    context = "WorkspaceHostCore.TryConfirmPilotBase";
                    break;
                case "keep":
                    var keep = rig.Port.TryKeepMine(rig.Request());
                    Assert.Equal(EditIntentStatus.Failed, keep.Status);
                    message = keep.Reason;
                    context = "WorkspaceHostCore.TryKeepPilotEdit";
                    break;
                default:
                    var discard = rig.Port.TryDiscardAndReload(rig.Request());
                    Assert.Equal(EditIntentStatus.Failed, discard.Status);
                    message = discard.Reason;
                    context = "WorkspaceHostCore.TryDiscardAndReloadPilotEdit";
                    break;
            }

            Assert.Equal("the reader threw", message);
            var fault = Assert.Single(rig.Host.Faults);
            Assert.Equal(context, fault.Context);
            Assert.Equal(WorkspaceHostStatus.Ready, rig.Core.GetSnapshot().Status);
            Assert.False(rig.Host.Bridge.Core.IsDegraded(rig.Session));
            Assert.Equal(DraftStatus.Dirty, draft.Status);
            Assert.True(draft.IsPossiblyStale);
            // The mark of the read was released by the finally: a second call reads again.
            rig.Reader.Throw = null;
            Assert.NotEqual("Hay una lectura del piloto en curso.", rig.Port.TryBeginEdit(rig.Request()).Reason);
        }
    }

    [Theory]
    [InlineData(EditIntentReadStatus.NoActiveDocument, "no document", "Waiting")]
    [InlineData(EditIntentReadStatus.Busy, "busy", "Waiting")]
    [InlineData(EditIntentReadStatus.Rejected, "not yours", "Rejected")]
    [InlineData(EditIntentReadStatus.Failed, "could not read", "Failed")]
    public void EveryAnswerOfTheReaderThatIsNotAReadIsMappedWithItsReasonAndChangesNothing(EditIntentReadStatus read, string reason, string expected)
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        rig.Reader.Answer = _ => read switch
        {
            EditIntentReadStatus.NoActiveDocument => EditIntentReadResult.NoActiveDocument(reason),
            EditIntentReadStatus.Busy => EditIntentReadResult.Busy(reason),
            EditIntentReadStatus.Rejected => EditIntentReadResult.Rejected(reason),
            _ => EditIntentReadResult.Failed(reason),
        };

        var confirm = rig.Core.TryConfirmPilotBase(rig.Request());
        var keep = rig.Core.TryKeepPilotEdit(rig.Request());
        var discard = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

        Assert.Equal(expected, confirm.Status.ToString());
        Assert.Equal(expected, keep.Status.ToString());
        Assert.Equal(expected, discard.Status.ToString());
        Assert.Equal(reason, confirm.Reason);
        Assert.Equal(reason, keep.Reason);
        Assert.Equal(reason, discard.Reason);
        Assert.Equal(3, rig.Reader.Calls.Count);
        Assert.Empty(rig.Host.Faults);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.True(draft.IsPossiblyStale);
    }

    [Fact]
    public void AReadWithoutReasonOrWithoutResultUsesTheTextsOfTheOtherOperations()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();

        rig.Reader.Answer = _ => EditIntentReadResult.Busy(null!);
        Assert.Equal("El dibujo no está listo para leerse.", rig.Core.TryConfirmPilotBase(rig.Request()).Reason);
        Assert.Equal("El dibujo no está listo para leerse.", rig.Core.TryKeepPilotEdit(rig.Request()).Reason);
        Assert.Equal("El dibujo no está listo para leerse.", rig.Core.TryDiscardAndReloadPilotEdit(rig.Request()).Reason);

        rig.Reader.Answer = _ => EditIntentReadResult.Rejected(null!);
        Assert.Equal("La petición no es del documento activo.", rig.Core.TryConfirmPilotBase(rig.Request()).Reason);
        Assert.Equal("La petición no es del documento activo.", rig.Core.TryKeepPilotEdit(rig.Request()).Reason);
        Assert.Equal("La petición no es del documento activo.", rig.Core.TryDiscardAndReloadPilotEdit(rig.Request()).Reason);

        rig.Reader.Answer = _ => EditIntentReadResult.Failed(null!);
        Assert.Equal("Falló la relectura.", rig.Core.TryConfirmPilotBase(rig.Request()).Reason);
        Assert.Equal("Falló la lectura para la recarga.", rig.Core.TryKeepPilotEdit(rig.Request()).Reason);
        Assert.Equal("Falló la lectura para la recarga.", rig.Core.TryDiscardAndReloadPilotEdit(rig.Request()).Reason);
    }

    // ------------------------------------------------------------------ the consumption of the read

    [Fact]
    public void ConfirmWithAMatchAnswersConfirmedWithoutDiagnosticAndKeepsTheStatusAndTheEdits()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        var stale = draft.StaleBase;

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Confirmed, outcome.Status);
        Assert.Null(outcome.AdmissionDiagnostic);
        Assert.Null(outcome.Reason);
        Assert.Empty(outcome.Differences);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.False(draft.IsPossiblyStale);
        Assert.Same(stale, draft.StaleBase);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void ConfirmWithAMatchThatIsNotAdmittedCarriesTheDiagnosticOfTheAdmissionAndOnlyThen()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        rig.Reader.Answer = SameBaseNotAdmitted;

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Confirmed, outcome.Status);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, outcome.AdmissionDiagnostic);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void ConfirmWithADifferenceAnswersConflictWithTheDescribedDifferencesAndPutsTheDraftInConflict()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        var before = draft.StaleBase;
        rig.Reader.Answer = EditIntentKit.MemberBecameBlocking;
        var expected = draft.StaleBase.CompareWith(EditIntentKit.MemberBecameBlocking(RackA).PilotRead!.StaleBase).Differences
            .Select(d => (d.Key ?? "(variable registry)") + ":" + d.Kind).ToArray();
        Assert.NotEmpty(expected);

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Conflict, outcome.Status);
        Assert.Equal(expected, outcome.Differences.ToArray());
        Assert.Null(outcome.AdmissionDiagnostic);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Same(before, draft.StaleBase);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void ConfirmWithAMatchInConflictAnswersConfirmedAndTheDraftStaysInConflict()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        DraftToConflict(draft);

        var outcome = rig.Core.TryConfirmPilotBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Confirmed, outcome.Status);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Single(draft.ConflictDifferences);
    }

    [Fact]
    public void KeepAfterAChangeOfTheDrawingStaysOnTheNewVersionWithTheScaleAndThePendingTextOfTheDraft()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        draft.SetPendingText("field", "typed");
        var reread = Changed(RackA).PilotRead!;
        rig.Reader.Answer = Changed;
        var expectedCurrent = PushBackPilotMaterializer.KeepPilotEditsOn(
            PilotKit.Admitted(IntentKit.Variant("pb-02-modulos", RackA)), draft.CurrentDraftState);

        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Kept, outcome.Status);
        Assert.Equal(EditWitnessStatus.NotSupplied, outcome.Witness);
        Assert.Null(outcome.Reason);
        Assert.True(draft.StaleBase.CompareWith(reread.StaleBase).IsMatch);
        Assert.Equal(IntentKit.Variant("pb-02-modulos", RackA), draft.InitialDraftState.Origin);
        Assert.False(draft.IsPossiblyStale);
        Assert.True(draft.IsDirty);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.True(PushBackPilotStateComparer.Instance.Equals(expectedCurrent, draft.CurrentDraftState));
        Assert.Equal("typed", draft.PendingText["field"]);
    }

    [Fact]
    public void KeepAConflictLeavesItAndTheDraftIsDirtyOnTheNewVersion()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        DraftToConflict(draft);
        rig.Reader.Answer = Changed;

        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Kept, outcome.Status);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Empty(draft.ConflictDifferences);
    }

    [Fact]
    public void KeepWithAReadThatIsNotAdmittedKeepsTheDraftAsItWasAndCarriesTheDiagnostic()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        var before = draft.StaleBase;
        rig.Reader.Answer = EditIntentKit.OriginMissing;

        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, outcome.Diagnostic);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
        Assert.Same(before, draft.StaleBase);
        Assert.True(draft.IsPossiblyStale);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void KeepThatFindsTheDraftBeingAppliedAfterTheReadAnswersWaitingAndChangesNothing()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.MarkPossiblyStale();
        rig.Reader.Answer = rack =>
        {
            Assert.True(draft.TryBeginApply().IsStarted);
            return Changed(rack);
        };
        var before = draft.StaleBase;

        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
        Assert.Equal("El borrador se está aplicando o ya no está registrado.", outcome.Reason);
        Assert.Equal(DraftStatus.Applying, draft.Status);
        Assert.Same(before, draft.StaleBase);
    }

    [Fact]
    public void DiscardAndReloadReplacesTheDraftFromTheOneReadAndAReadThatIsNotAdmittedKeepsIt()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        draft.SetPendingText("field", "typed");
        draft.MarkPossiblyStale();
        rig.Reader.Answer = EditIntentKit.OriginMissing;
        var before = draft.StaleBase;

        var refused = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.NotAdmitted, refused.Status);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, refused.Diagnostic);
        Assert.Same(before, draft.StaleBase);
        Assert.True(draft.IsDirty);
        Assert.Equal("typed", draft.PendingText["field"]);

        rig.Reader.Answer = Changed;
        var reloaded = rig.Core.TryDiscardAndReloadPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Reloaded, reloaded.Status);
        Assert.Equal(EditWitnessStatus.NotSupplied, reloaded.Witness);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Equal(IntentKit.Variant("pb-02-modulos", RackA), draft.InitialDraftState.Origin);
        Assert.False(draft.HasPendingText);
    }

    [Fact]
    public void DiscardAndReloadTakesAnUnknownAfterTheWriteOutOfItAndAnErrorToo()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        Assert.True(draft.TryBeginApply().IsStarted);
        draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.AppliedPostRereadFailed("the reread failed"));
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);
        Assert.Equal(EditConfirmStatus.NotApplicable, rig.Core.TryConfirmPilotBase(rig.Request()).Status);

        Assert.Equal(EditIntentStatus.Reloaded, rig.Core.TryDiscardAndReloadPilotEdit(rig.Request()).Status);

        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Null(draft.UnknownOrigin);
        Assert.Null(draft.LastApplyCause);
    }

    // ------------------------------------------------------------------ the mark of "known clean": the operations that change a draft end it

    // After the Allow, a draft is seeded whose comparer ALWAYS throws: retiring with the mark never asks the comparer, retiring
    // conservatively does, and the draft is then reported as lost and uncertain. The notice tells which way the host retired the session.
    private static string PathAfter(Func<Rig, DraftSession<PushBackPilotDraftState>> before, Action<Rig, DraftSession<PushBackPilotDraftState>> between)
    {
        var rig = new Rig();
        var draft = before(rig);
        Assert.Equal(CloseDecision.Allow, ((IWorkspaceLifecycleAdvisor)rig.Core).AdviseClose(rig.Session));
        between(rig, draft);
        var seeded = rig.Core.OpenDrafts.Begin(rig.Session, "R9", DraftKit.Read(), new DraftThrowingComparer { Throw = true }, DraftKit.CopyForm);
        Assert.Equal(DraftBeginStatus.Created, seeded.Status);

        ((IWorkspaceLifecycleAdvisor)rig.Core).SessionDestroyed(rig.Session, closeSequenceIntact: true);

        if (rig.Core.LossNotices.Count == 0) return "known-safe";
        Assert.Contains("R9", Assert.Single(rig.Core.LossNotices).UncertainRackIds);
        return "conservative";
    }

    [Fact]
    public void AConfirmedCheckKeepsTheMarkAndAConflictFoundByTheCheckEndsIt()
    {
        Assert.Equal("known-safe", PathAfter(rig => rig.Open(), (rig, draft) =>
            Assert.Equal(EditConfirmStatus.Confirmed, rig.Core.TryConfirmPilotBase(rig.Request()).Status)));

        Assert.Equal("conservative", PathAfter(rig => rig.Open(), (rig, draft) =>
        {
            rig.Reader.Answer = EditIntentKit.MemberBecameBlocking;
            Assert.Equal(EditConfirmStatus.Conflict, rig.Core.TryConfirmPilotBase(rig.Request()).Status);
        }));
    }

    [Fact]
    public void AKeepThatIsKeptEndsTheMarkAndOneThatIsNotNeededOrNotAdmittedDoesNot()
    {
        Assert.Equal("conservative", PathAfter(rig => rig.Open(), (rig, draft) =>
        {
            Rig.Edit(draft);
            draft.MarkPossiblyStale();
            rig.Reader.Answer = Changed;
            Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(rig.Request()).Status);
        }));

        Assert.Equal("known-safe", PathAfter(rig => rig.Open(), (rig, draft) =>
            Assert.Equal(EditIntentStatus.NotNeeded, rig.Core.TryKeepPilotEdit(rig.Request()).Status)));

        Assert.Equal("known-safe", PathAfter(rig => rig.Open(), (rig, draft) =>
        {
            Rig.Edit(draft);
            draft.MarkPossiblyStale();
            rig.Reader.Answer = EditIntentKit.OriginMissing;
            Assert.Equal(EditIntentStatus.NotAdmitted, rig.Core.TryKeepPilotEdit(rig.Request()).Status);
        }));
    }

    [Fact]
    public void ADiscardAndReloadThatReloadsEndsTheMarkAndOneThatIsNotAdmittedDoesNot()
    {
        Assert.Equal("conservative", PathAfter(rig => rig.Open(), (rig, draft) =>
            Assert.Equal(EditIntentStatus.Reloaded, rig.Core.TryDiscardAndReloadPilotEdit(rig.Request()).Status)));

        Assert.Equal("known-safe", PathAfter(rig => rig.Open(), (rig, draft) =>
        {
            rig.Reader.Answer = EditIntentKit.OriginMissing;
            Assert.Equal(EditIntentStatus.NotAdmitted, rig.Core.TryDiscardAndReloadPilotEdit(rig.Request()).Status);
        }));
    }

    // ------------------------------------------------------------------ the gate of notices and the state of the field

    [Fact]
    public void InsideANoticeTheGateAnswersWaitingForTheThreeOperationsWithoutDelegatingAndOutsideItDelegates()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Rig.Edit(draft);
        var depth = new WorkspaceNoticeDepth();
        IWorkspacePilotEditPort gate = new WorkspacePilotEditGate(rig.Core, depth);
        depth.Enter();

        var confirm = gate.TryConfirmBase(rig.Request());
        var keep = gate.TryKeepMine(rig.Request());
        var discard = gate.TryDiscardAndReload(rig.Request());

        Assert.Equal(EditConfirmStatus.Waiting, confirm.Status);
        Assert.Equal(WorkspacePilotEditGate.InsideNoticeReason, confirm.Reason);
        Assert.Equal(EditIntentStatus.Waiting, keep.Status);
        Assert.Equal(WorkspacePilotEditGate.InsideNoticeReason, keep.Reason);
        Assert.Equal(EditIntentStatus.Waiting, discard.Status);
        Assert.Equal(WorkspacePilotEditGate.InsideNoticeReason, discard.Reason);
        Assert.Empty(rig.Reader.Calls);

        depth.Leave();
        Assert.Equal(EditConfirmStatus.Confirmed, gate.TryConfirmBase(rig.Request()).Status);
        Assert.Equal(EditIntentStatus.NotNeeded, gate.TryKeepMine(rig.Request()).Status);
        Assert.Equal(EditIntentStatus.Reloaded, gate.TryDiscardAndReload(rig.Request()).Status);
        Assert.Equal(2, rig.Reader.Calls.Count);
    }

    [Fact]
    public void TheFieldViewCarriesTheOriginOfUnknownTheTextOfTheConflictAndTheLastCauseAsTheDraftHoldsThem()
    {
        var rig = new Rig();
        var draft = rig.Open();

        // Clean: the three are inert.
        var clean = rig.Field();
        Assert.Equal(PilotFieldState.Clean, clean.State);
        Assert.Equal(PilotUnknownOrigin.None, clean.UnknownOrigin);
        Assert.Null(clean.ConflictText);
        Assert.Null(clean.LastCause);

        // The last cause travels as the draft holds it, also outside Unknown (an update that was not applied).
        Rig.Edit(draft);
        Assert.True(draft.TryBeginApply().IsStarted);
        draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedPreflight("a layer is locked"));
        var dirty = rig.Field();
        Assert.Equal(PilotFieldState.Dirty, dirty.State);
        Assert.Equal("a layer is locked", dirty.LastCause);
        Assert.Equal(PilotUnknownOrigin.None, dirty.UnknownOrigin);
        Assert.Null(dirty.ConflictText);

        // Conflict: the differences as text, only in this state.
        rig.Reader.Answer = EditIntentKit.MemberBecameBlocking;
        var conflict = rig.Core.TryConfirmPilotBase(rig.Request());
        Assert.Equal(EditConfirmStatus.Conflict, conflict.Status);
        var inConflict = rig.Field();
        Assert.Equal(PilotFieldState.Conflict, inConflict.State);
        Assert.Equal(string.Join(", ", conflict.Differences), inConflict.ConflictText);
        Assert.Equal(PilotUnknownOrigin.None, inConflict.UnknownOrigin);
        Assert.Equal("a layer is locked", inConflict.LastCause);

        // Unknown, by origin.
        foreach (var (outcome, origin) in new[]
                 {
                     (DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the commit threw"), PilotUnknownOrigin.CommitThrew),
                     (DraftApplyOutcome<PushBackPilotDraftState>.AppliedPostRereadFailed("the reread failed"), PilotUnknownOrigin.PostReread),
                 })
        {
            var other = new Rig();
            var unknown = other.Open();
            Rig.Edit(unknown);
            Assert.True(unknown.TryBeginApply().IsStarted);
            unknown.CompleteApply(outcome);

            var view = other.Field();

            Assert.Equal(PilotFieldState.Unknown, view.State);
            Assert.Equal(origin, view.UnknownOrigin);
            Assert.Null(view.ConflictText);
            Assert.Equal(outcome.Cause, view.LastCause);
        }
    }

    [Fact]
    public void TheDefaultFieldViewHasTheThreeNewValuesInert()
    {
        var unset = default(PilotFieldView);

        Assert.Equal(PilotUnknownOrigin.None, unset.UnknownOrigin);
        Assert.Null(unset.ConflictText);
        Assert.Null(unset.LastCause);
        Assert.Equal(new[] { "None", "CommitThrew", "PostReread" }, Enum.GetNames<PilotUnknownOrigin>());
        var built = new PilotFieldView(true, PilotFieldState.Unknown, false, true, false, "1", null, null, null, null, PilotUnknownOrigin.PostReread, "text", "cause");
        Assert.Equal(PilotUnknownOrigin.PostReread, built.UnknownOrigin);
        Assert.Equal("text", built.ConflictText);
        Assert.Equal("cause", built.LastCause);
        var oldShape = new PilotFieldView(true, PilotFieldState.Clean, false, false, true, "1", null, null, null, "reason");
        Assert.Equal("reason", oldShape.ErrorReason);
        Assert.Equal(PilotUnknownOrigin.None, oldShape.UnknownOrigin);
    }

    [Fact]
    public void TheNewOutcomesAreValuesWithTheStatesOfThePackageAndTheDefaultIsUnavailable()
    {
        Assert.True(typeof(EditConfirmOutcome).IsValueType);
        Assert.Equal(
            new[] { "Unavailable", "Waiting", "Rejected", "NotFound", "NotApplicable", "Confirmed", "Conflict", "Failed" },
            Enum.GetNames<EditConfirmStatus>());
        var unset = default(EditConfirmOutcome);
        Assert.Equal(EditConfirmStatus.Unavailable, unset.Status);
        Assert.Equal(EditConfirmStatus.Unavailable, default(EditConfirmStatus));
        Assert.NotNull(unset.Differences);
        Assert.Empty(unset.Differences);
        Assert.Null(unset.AdmissionDiagnostic);

        Assert.Equal(EditConfirmStatus.Confirmed, EditConfirmOutcome.Confirmed().Status);
        Assert.Null(EditConfirmOutcome.Confirmed().AdmissionDiagnostic);
        Assert.Equal("why", EditConfirmOutcome.Confirmed("why").AdmissionDiagnostic);
        var conflict = EditConfirmOutcome.Conflict(new[] { "B2:Membership" });
        Assert.Equal(new[] { "B2:Membership" }, conflict.Differences.ToArray());
        Assert.Null(conflict.AdmissionDiagnostic);
        foreach (var outcome in new[]
                 {
                     EditConfirmOutcome.Unavailable("x"), EditConfirmOutcome.Waiting("x"), EditConfirmOutcome.Rejected("x"),
                     EditConfirmOutcome.NotFound("x"), EditConfirmOutcome.NotApplicable("x"), EditConfirmOutcome.Failed("x"),
                 })
        {
            Assert.Equal("x", outcome.Reason);
            Assert.Empty(outcome.Differences);
            Assert.Null(outcome.AdmissionDiagnostic);
        }

        // Kept is the one new state of the intention, after the ten that stay as an ordered prefix; Reloaded is reused by the discard.
        var intent = Enum.GetNames<EditIntentStatus>();
        Assert.Equal(11, intent.Length);
        Assert.Equal("Kept", intent[10]);
        Assert.Equal(EditIntentStatus.Kept, EditIntentOutcome.Kept(EditWitnessStatus.Changed).Status);
        Assert.Equal(EditWitnessStatus.Changed, EditIntentOutcome.Kept(EditWitnessStatus.Changed).Witness);
    }

    // ================================================================== INV-F4T4A1-07 and 08: the snapshot and the order of PREPARE

    [Fact]
    public void TheSnapshotIsBuiltOnlyByFromWithCopiesAndTheStatusBeforeAnAttemptWhileApplying()
    {
        var read = IntentKit.Read(PrepareKit.RealScan());
        var draft = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(draft, 2.25);

        var input = PushBackPilotPrepareInput.From(draft);

        Assert.Equal(PrepareKit.Rack, input.RackId);
        Assert.Same(draft.StaleBase, input.StaleBase);
        Assert.Equal(DraftStatus.Dirty, input.ConsideredStatus);
        Assert.True(input.IsDirty);
        Assert.False(input.HasPendingText);
        Assert.NotSame(draft.CurrentDraftState, input.CurrentDraftState);
        Assert.Equal(2.25, input.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.NotSame(draft.InitialDraftState, input.InitialDraftState);
        // Changing the copies changes nothing of the draft.
        input.CurrentDraftState.Inputs.Annotations.AnnotationScale = 9.0;
        input.InitialDraftState.Inputs.Annotations.AnnotationScale = 9.0;
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.NotEqual(9.0, draft.InitialDraftState.Inputs.Annotations.AnnotationScale);

        draft.SetPendingText("AnnotationScale", "3");
        Assert.True(PushBackPilotPrepareInput.From(draft).HasPendingText);
        draft.RemovePendingText("AnnotationScale");

        // Under Applying the considered status is the one before the attempt (N-7), not Applying.
        Assert.True(draft.TryBeginApply().IsStarted);
        Assert.Equal(DraftStatus.Applying, draft.Status);
        Assert.Equal(DraftStatus.Dirty, PushBackPilotPrepareInput.From(draft).ConsideredStatus);

        var clean = PrepareKit.DraftOf(IntentKit.Read(PrepareKit.RealScan()));
        Assert.True(clean.TryBeginApply().IsStarted);
        Assert.Equal(DraftStatus.Clean, PushBackPilotPrepareInput.From(clean).ConsideredStatus);
        Assert.Throws<ArgumentNullException>(() => PushBackPilotPrepareInput.From(null!));
    }

    [Fact]
    public void TheSevenOutcomesAreEvaluatedInTheDeclaredOrder()
    {
        Assert.Equal(
            new[] { "NotApplicable", "PendingText", "NothingToApply", "Conflict", "NotAdmitted", "Invalid", "Ready" },
            Enum.GetNames<PushBackPilotPrepareKind>());
    }

    [Fact]
    public void ADraftThatIsNotCleanOrDirtyIsNotApplicableWhateverTheReread()
    {
        var read = IntentKit.Read(PrepareKit.RealScan());
        var other = IntentKit.Read(PrepareKit.RealScan("pb-02-modulos"));

        foreach (var name in new[] { "Conflict", "UnknownCommit", "UnknownPost", "Error" })
        {
            var draft = PrepareKit.DraftOf(read);
            PrepareKit.SetScale(draft, 2.25);
            Assert.True(draft.TryBeginApply().IsStarted);
            switch (name)
            {
                case "Conflict":
                    draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedConflict(new[] { new DraftDifference("A1", DraftDifferenceKind.EnvelopeText) }));
                    break;
                case "UnknownCommit":
                    draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("c"));
                    break;
                case "UnknownPost":
                    draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.AppliedPostRereadFailed("p"));
                    break;
                default:
                    draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure("f"));
                    draft.EnterError("boom");
                    break;
            }

            Assert.NotEqual(DraftStatus.Dirty, draft.Status);
            Assert.NotEqual(DraftStatus.Clean, draft.Status);
            foreach (var reread in new[] { read, other })
            {
                var result = PrepareKit.Prepare(draft, reread);

                Assert.Equal(PushBackPilotPrepareKind.NotApplicable, result.Kind);
                Assert.Null(result.Plan);
                Assert.Null(result.Reason);
                Assert.Empty(result.Differences);
            }
        }
    }

    [Fact]
    public void PendingTextComesBeforeEverythingButNotApplicableAndBeforeTheReread()
    {
        var read = IntentKit.Read(PrepareKit.RealScan());
        var draft = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(draft, 2.25);
        draft.SetPendingText("AnnotationScale", "3");

        foreach (var reread in new[] { read, IntentKit.Read(PrepareKit.RealScan("pb-02-modulos")), null })
        {
            var result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), reread!, PilotKit.Catalog);

            Assert.Equal(PushBackPilotPrepareKind.PendingText, result.Kind);
            Assert.Null(result.Plan);
        }
    }

    [Fact]
    public void ACleanDraftHasNothingToApplyAndTheRereadIsNotLookedAt()
    {
        var draft = PrepareKit.DraftOf(IntentKit.Read(PrepareKit.RealScan()));
        PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), IntentKit.Read(PrepareKit.RealScan()), PilotKit.Catalog);

        foreach (var reread in new PushBackPilotIntentResult?[] { null, IntentKit.Read(PrepareKit.RealScan("pb-02-modulos")), PrepareKit.Reread(DraftKit.DefaultBase(), notAdmitted: "x") })
        {
            PushBackPilotPrepareResult? result = null;
            var thrown = Record.Exception(() => result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), reread!, null!));

            Assert.Null(thrown);
            Assert.Equal(PushBackPilotPrepareKind.NothingToApply, result!.Kind);
            Assert.Null(result.Plan);
        }

        // A clean draft that began an update is NothingToApply too (N-8): it never takes the slot.
        Assert.True(draft.TryBeginApply().IsStarted);
        Assert.Equal(PushBackPilotPrepareKind.NothingToApply, PrepareKit.Prepare(draft, IntentKit.Read(PrepareKit.RealScan())).Kind);
    }

    [Fact]
    public void ADifferenceOfTheBaseIsAConflictBeforeTheAdmissionAndWithTheOrderedDifferences()
    {
        var read = IntentKit.Read(PrepareKit.RealScan());
        var draft = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(draft, 2.25);

        // Different and admitted.
        var changed = IntentKit.Read(PrepareKit.RealScan("pb-02-modulos"));
        Assert.True(changed.Read.IsAdmitted);
        // Different and NOT admitted: the conflict is still the answer, because the comparison goes first (INV-06).
        var notAdmitted = IntentKit.Read(IntentKit.Scan(PrepareKit.Rack, "ZZ", IntentKit.Member("A1", PrepareKit.Frontal("pb-02-modulos"))));
        Assert.False(notAdmitted.Read.IsAdmitted);

        foreach (var reread in new[] { changed, notAdmitted })
        {
            var expected = draft.StaleBase.CompareWith(reread.StaleBase).Differences.ToArray();
            Assert.NotEmpty(expected);

            var result = PrepareKit.Prepare(draft, reread);

            Assert.Equal(PushBackPilotPrepareKind.Conflict, result.Kind);
            Assert.Equal(expected, result.Differences.ToArray());
            Assert.Null(result.Plan);
            Assert.Null(result.Reason);
        }
    }

    [Fact]
    public void ARereadThatMatchesButIsNotAdmittedIsNotAdmittedWithItsDiagnostic()
    {
        var scan = PrepareKit.RealScan();
        var draft = PrepareKit.DraftOf(IntentKit.Read(scan));
        PrepareKit.SetScale(draft, 2.25);
        var notAdmitted = PrepareKit.Reread(draft.StaleBase, notAdmitted: PushBackPilotIntentReader.OriginMissingDiagnostic);

        var result = PrepareKit.Prepare(draft, notAdmitted);

        Assert.Equal(PushBackPilotPrepareKind.NotAdmitted, result.Kind);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, result.Reason);
        Assert.Null(result.Plan);
        Assert.Empty(result.Differences);
    }

    // ================================================================== INV-F4T4A1-07, 08, 09: the Invalid cases (a) to (e) and the plan

    [Fact]
    public void InvalidA_ACurrentStateThatDoesNotComposeIsInvalidWithTheReasonOfTheCompositionAndOverTheCarryOversOfTheMaterializer()
    {
        var read = IntentKit.Read(PrepareKit.RealScan());

        // Content that the pilot does not edit.
        var content = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(content, 2.25);
        content.CurrentDraftState.Inputs.PalletTolerance += 1.0;
        var rejected = PrepareKit.Prepare(content, read);
        Assert.Equal(PushBackPilotPrepareKind.Invalid, rejected.Kind);
        Assert.Equal(PushBackPilotMaterializer.NonPilotContentReason, rejected.Reason);

        // A scale that is not valid (6), with the scale that Dirty needs to be set apart.
        foreach (var scale in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0.0, -1.0 })
        {
            var invalid = PrepareKit.DraftOf(read);
            PrepareKit.SetScale(invalid, scale);

            var result = PrepareKit.Prepare(invalid, read);

            Assert.Equal(PushBackPilotPrepareKind.Invalid, result.Kind);
            Assert.Equal(PushBackPilotMaterializer.InvalidScaleReason, result.Reason);
        }
    }

    [Fact]
    public void InvalidB_NoMutableMemberWithTheTextOfTheOriginOrSeveralOfThemIsInvalid()
    {
        var a1 = PrepareKit.Frontal();
        var b2 = PrepareKit.Lateral(PrepareKit.ExistingPost());
        var scan = PrepareKit.RealScan();

        // The base and the state of the draft come from different texts: no member has the origin of the state.
        var none = PrepareKit.DraftOf(PushBackPilotIntentReader.BuildStaleBase(scan), PilotKit.Text("pb-02-modulos"));
        PrepareKit.SetScale(none, 2.25);
        Assert.NotEqual(a1, none.InitialDraftState.Origin);
        var noneResult = PrepareKit.Prepare(none, PrepareKit.Reread(none.StaleBase));
        Assert.Equal(PushBackPilotPrepareKind.Invalid, noneResult.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.NoOriginReason, noneResult.Reason);

        // Two members carry exactly the text of the origin.
        var origin = PilotKit.Text("pb-01-base");
        var twice = PrepareKit.BaseOf(("A1", origin), ("B2", origin), ("C3", b2));
        var several = PrepareKit.DraftOf(twice, origin);
        PrepareKit.SetScale(several, 2.25);
        var severalResult = PrepareKit.Prepare(several, PrepareKit.Reread(twice));
        Assert.Equal(PushBackPilotPrepareKind.Invalid, severalResult.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.SeveralOriginsReason, severalResult.Reason);
    }

    [Fact]
    public void InvalidC_AnInnerDesignOfAnotherKindOrOfANewerMajorAbortsThePreflightAndTheUpdateIsInvalid()
    {
        var origin = PilotKit.Text("pb-01-base");
        var post = PrepareKit.ExistingPost();
        var wrongKind = IntentKit.Rewrite(PrepareKit.Lateral(post), embed =>
            embed.Design = new RackProjectStore().Serialize(RackProject.ForDynamic(DimensionViewScenarios.DynamicPersistedDesign(RackCad.Domain.Systems.Shared.DimensionDetail.Standard, PilotKit.Catalog))));
        var newerMajor = IntentKit.Rewrite(PrepareKit.Lateral(post), embed =>
        {
            var inner = JsonNode.Parse(embed.Design)!.AsObject();
            inner["SchemaVersion"] = "9.0";
            embed.Design = inner.ToJsonString();
        });

        foreach (var (text, reason) in new[]
                 {
                     (wrongKind, PushBackPilotUpdatePlanner.InnerKindReason),
                     (newerMajor, PushBackPilotUpdatePlanner.InnerMajorReason),
                 })
        {
            var members = PrepareKit.BaseOf(("A1", origin), ("B2", text));
            var draft = PrepareKit.DraftOf(members, origin);
            PrepareKit.SetScale(draft, 2.25);

            var result = PrepareKit.Prepare(draft, PrepareKit.Reread(members));

            Assert.Equal(PushBackPilotPrepareKind.Invalid, result.Kind);
            Assert.Equal(reason, result.Reason);
            Assert.Null(result.Plan);
        }
    }

    [Fact]
    public void InvalidA_AnOriginThatNoLongerMaterializesIsInvalidWithTheReasonOfTheMaterializerBeforeAnythingElse()
    {
        // The inner design of the origin cannot be read: the composition (a) is the first to find it, so the reason is the one of the materializer.
        var origin = IntentKit.Rewrite(PilotKit.Text("pb-01-base"), embed => embed.Design = "{ not json");
        var members = PrepareKit.BaseOf(("A1", origin));
        var state = PilotKit.State("pb-01-base");
        var draft = new DraftRegistry().Begin(
            DraftKit.S1, PrepareKit.Rack, DraftIntentRead<PushBackPilotDraftState>.Admitted(members, new PushBackPilotDraftState(state.Editor, state.Inputs, origin)),
            PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy).Draft!;
        PrepareKit.SetScale(draft, 2.25);

        var result = PrepareKit.Prepare(draft, PrepareKit.Reread(members));

        Assert.Equal(PushBackPilotPrepareKind.Invalid, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic, result.Reason);
    }

    [Fact]
    public void InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten()
    {
        var scan = PrepareKit.RealScan();
        var read = IntentKit.Read(scan);
        var draft = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(draft, 2.25);

        // A catalog that counts its touches and fails after the n-th one. The touches of the stages of PREPARE are measured with the
        // real planner: the composition (a) and the builder (d) both read the catalog, and the failure is armed to come in the builder.
        var armed = new CountingCatalog(PilotKit.Catalog, int.MaxValue);
        var ready = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), read, armed.Catalog);
        Assert.Equal(PushBackPilotPrepareKind.Ready, ready.Kind);
        var total = armed.Touches;
        Assert.True(total > 0);

        var failures = new List<PushBackPilotPrepareResult>();
        for (var allowed = 0; allowed < total; allowed++)
        {
            var failing = new CountingCatalog(PilotKit.Catalog, allowed);
            failures.Add(PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), read, failing.Catalog));
        }

        // Every failure is a typed Invalid and never an exception; at least one of them is the failure of the builder (d).
        Assert.All(failures, f =>
        {
            Assert.Equal(PushBackPilotPrepareKind.Invalid, f.Kind);
            Assert.Null(f.Plan);
            Assert.False(string.IsNullOrEmpty(f.Reason));
        });
        Assert.Contains(failures, f => f.Reason!.StartsWith(PushBackPilotUpdatePlanner.SystemFailurePrefix, StringComparison.Ordinal));
        Assert.Contains(failures, f => f.Reason!.Contains("catalog-boom", StringComparison.Ordinal));
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    private sealed class CountingList<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _inner;
        private readonly Func<bool> _fails;
        private readonly Action _touched;

        public CountingList(IReadOnlyList<T> inner, Action touched, Func<bool> fails)
        {
            _inner = inner;
            _touched = touched;
            _fails = fails;
        }

        private void Touch()
        {
            _touched();
            if (_fails()) throw new InvalidOperationException("catalog-boom");
        }

        public int Count
        {
            get
            {
                Touch();
                return _inner.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                Touch();
                return _inner[index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            Touch();
            return _inner.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CountingCatalog
    {
        private readonly int _allowed;

        public CountingCatalog(RackCatalog real, int allowed)
        {
            _allowed = allowed;
            Catalog = new RackCatalog
            {
                PostProfiles = new CountingList<ProfileCatalogEntry>(real.PostProfiles, () => Touches++, Fails),
                BeamProfiles = new CountingList<BeamProfileCatalogEntry>(real.BeamProfiles, () => Touches++, Fails),
                SafetyElements = new CountingList<SafetyElementCatalogEntry>(real.SafetyElements, () => Touches++, Fails),
                TrussProfiles = real.TrussProfiles,
                BasePlates = real.BasePlates,
                FlowBedProfiles = real.FlowBedProfiles,
                Mensulas = real.Mensulas,
                SpacerProfiles = real.SpacerProfiles,
                ConnectionPoints = real.ConnectionPoints,
                ConnectionLayout = real.ConnectionLayout,
                Views = real.Views,
                Blocks = real.Blocks,
                Defaults = real.Defaults,
            };
        }

        public RackCatalog Catalog { get; }

        public int Touches { get; private set; }

        private bool Fails() => Touches > _allowed;
    }

    [Fact]
    public void InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView()
    {
        var origin = PilotKit.Text("pb-01-base");
        var originFrontal = PrepareKit.Frontal();
        var cases = new Dictionary<string, string>
        {
            // A lateral cut at a post that does not exist: a phantom that RACKEDITAR would erase and the pilot never does.
            ["phantom-lateral"] = PrepareKit.Lateral(PrepareKit.MissingPost),
            // The frontal view of side B in a rack that has only side A.
            ["side-b"] = PrepareKit.Frontal(section: 2),
            // A frontal view with the section of the fixtures (-1) is a corrupt descriptor: it is never coerced into another view.
            ["corrupt-descriptor"] = PilotKit.Text("pb-01-base"),
            ["section-out-of-range"] = PrepareKit.Frontal(section: 9),
        };

        foreach (var (name, text) in cases)
        {
            var members = PrepareKit.BaseOf(("A1", originFrontal), ("B2", text));
            var draft = PrepareKit.DraftOf(members, originFrontal);
            PrepareKit.SetScale(draft, 2.25);

            var result = PrepareKit.Prepare(draft, PrepareKit.Reread(members));

            Assert.True(result.Kind == PushBackPilotPrepareKind.Invalid, name);
            Assert.Null(result.Plan);
            Assert.StartsWith(PushBackPilotUpdatePlanner.ViewUnavailablePrefix + "B2", result.Reason, StringComparison.Ordinal);
            Assert.EndsWith("RACKEDITAR.", result.Reason, StringComparison.Ordinal);
        }

        Assert.NotEqual(origin, originFrontal);
    }

    [Fact]
    public void InvalidE_AnEnvelopeThatDoesNotDecodeIsInvalidWithItsKey()
    {
        var garbage = new DraftStaleBaseEntry("B2", DraftMembership.Mutable, "lateral", "1", "{ not an envelope");
        var good = new DraftStaleBaseEntry("A1", DraftMembership.Mutable, "frontal", "0", PrepareKit.Frontal());
        var members = DraftStaleBase.Create(new[] { good, garbage });
        var draft = PrepareKit.DraftOf(members, PrepareKit.Frontal());
        PrepareKit.SetScale(draft, 2.25);

        var result = PrepareKit.Prepare(draft, PrepareKit.Reread(members));

        Assert.Equal(PushBackPilotPrepareKind.Invalid, result.Kind);
        Assert.StartsWith(PushBackPilotUpdatePlanner.UnreadableViewPrefix + "B2", result.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void PrepareIsTotalAMissingPieceIsInvalidAndNeverAnException()
    {
        var read = IntentKit.Read(PrepareKit.RealScan());
        var draft = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(draft, 2.25);
        var input = PushBackPilotPrepareInput.From(draft);

        foreach (var (name, call) in new (string, Func<PushBackPilotPrepareResult>)[]
                 {
                     ("input", () => PushBackPilotUpdatePlanner.Prepare(null!, read, PilotKit.Catalog)),
                     ("reread", () => PushBackPilotUpdatePlanner.Prepare(input, null!, PilotKit.Catalog)),
                     ("catalog", () => PushBackPilotUpdatePlanner.Prepare(input, read, null!)),
                 })
        {
            PushBackPilotPrepareResult? result = null;
            var thrown = Record.Exception(() => result = call());

            Assert.True(thrown == null, name);
            Assert.Equal(PushBackPilotPrepareKind.Invalid, result!.Kind);
            Assert.Equal(PushBackPilotUpdatePlanner.MissingInputReason, result.Reason);
        }
    }

    // ------------------------------------------------------------------ the plan

    private static (DraftSession<PushBackPilotDraftState> Draft, PushBackPilotIntentResult Read, PushBackPilotUpdatePlan Plan) ReadyPlan(
        Func<PushBackPilotIntentScan>? scan = null, double scale = 2.25)
    {
        scan ??= () => PrepareKit.RealScan();
        var read = IntentKit.Read(scan());
        Assert.True(read.Read.IsAdmitted);
        var draft = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(draft, scale);
        var result = PrepareKit.Prepare(draft, IntentKit.Read(scan()));
        Assert.True(result.Kind == PushBackPilotPrepareKind.Ready, result.Reason);
        Assert.NotNull(result.Plan);
        Assert.Null(result.Reason);
        Assert.Empty(result.Differences);
        return (draft, read, result.Plan!);
    }

    [Fact]
    public void TheReadyPlanHoldsTheRackTheOriginTheExpectedBaseTheScaleAndTheMembersByKeyInOrdinalOrder()
    {
        var (draft, read, plan) = ReadyPlan();
        var post = PrepareKit.ExistingPost();

        Assert.Equal(PrepareKit.Rack, plan.RackId);
        Assert.Equal("A1", plan.OriginKey);
        Assert.Equal(new[] { "A1", "B2", "C3" }, plan.ExpectedBase.Select(e => e.Key).ToArray());
        Assert.Equal(draft.StaleBase.Entries.Values.OrderBy(e => e.Key, StringComparer.Ordinal).ToArray(), plan.ExpectedBase.ToArray());
        Assert.Equal(draft.StaleBase.VariableRegistryText, plan.ExpectedVariableRegistryText);
        Assert.Equal(2.25, plan.AnnotationScale);
        Assert.Equal(new[] { "A1", "B2", "C3" }, plan.Members.Select(m => m.Key).ToArray());
        // The view and the section as RACKEDITAR writes them: frontal with the section of the envelope, lateral with the post, plan with -1.
        Assert.Equal(new[] { ("frontal", 0), ("lateral", post), ("planta", -1) }, plan.Members.Select(m => (m.View, m.Section)).ToArray());
        Assert.True(draft.StaleBase.CompareWith(read.StaleBase).IsMatch);
    }

    [Fact]
    public void TheOutgoingDesignAndEnvelopeOfEachViewComeFromTheRereadEnvelopeAndNeverFromTheDraft()
    {
        // The three views carry a name that is not the one of the fixture and an unknown JSON member that a newer build wrote, and the
        // envelope that goes out keeps both. The reader of the intention does not admit an envelope with unknown members, so the re-read is
        // built by hand over the same texts, exactly as a later read of the drawing would hand them over.
        var post = PrepareKit.ExistingPost();
        var a1 = WithExtra(PrepareKit.Frontal(name: "Name of the rack"), "x1");
        var members = PrepareKit.BaseOf(
            ("A1", a1),
            ("B2", WithExtra(PrepareKit.Lateral(post, name: "Name of the rack"), "x2")),
            ("C3", WithExtra(PrepareKit.Planta(name: "Name of the rack"), "x3")));
        var draft = PrepareKit.DraftOf(members, a1);
        PrepareKit.SetScale(draft, 2.25);
        var ready = PrepareKit.Prepare(draft, PrepareKit.Reread(members));
        Assert.True(ready.Kind == PushBackPilotPrepareKind.Ready, ready.Reason);
        var plan = ready.Plan!;
        var composition = PushBackPilotMaterializer.ComposeForWrite(draft.InitialDraftState, draft.CurrentDraftState, PilotKit.Catalog);
        var design = new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(composition.Editor!, composition.Inputs!);
        var store = new RackProjectStore();

        foreach (var member in plan.Members)
        {
            var source = new RackEmbedStore().Deserialize(members.Entries[member.Key].EnvelopeText!)!;
            var expectedDesign = store.Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(store.Deserialize(source.Design)));
            var envelope = new RackEmbedStore().Deserialize(member.EnvelopeText)!;

            Assert.Equal(expectedDesign, member.DesignText);
            Assert.Equal(member.DesignText, envelope.Design);
            Assert.Equal(RackEmbedDocument.KindPushBack, envelope.Kind);
            Assert.Equal(source.Id, envelope.Id);
            // Name never comes from the draft: it is the name of the envelope that was re-read for that view.
            Assert.Equal(source.Name, envelope.Name);
            Assert.Equal("Name of the rack", envelope.Name);
            Assert.Equal(member.View, envelope.View);
            Assert.Equal(member.Section, envelope.Section);
            Assert.NotNull(envelope.ExtensionData);
            Assert.Contains(envelope.ExtensionData!.Keys, k => k.StartsWith("x", StringComparison.Ordinal));
            // The scale is the only thing the pilot changed in the design that goes out.
            Assert.Contains("2.25", member.DesignText, StringComparison.Ordinal);
        }

        Assert.Equal(plan.Members.Select(m => m.EnvelopeText).ToArray(), plan.PostExpectation.Envelopes.Select(e => e.Text).ToArray());
        Assert.Equal(plan.Members.Select(m => m.Key).ToArray(), plan.PostExpectation.Envelopes.Select(e => e.Key).ToArray());
        Assert.Equal(2.25, plan.PostExpectation.AnnotationScale);
    }

    private static string WithExtra(string text, string member) =>
        IntentKit.Rewrite(text, embed =>
            embed.ExtensionData = new Dictionary<string, System.Text.Json.JsonElement>
            {
                [member] = System.Text.Json.JsonDocument.Parse("\"value of " + member + "\"").RootElement.Clone(),
            });

    [Fact]
    public void ThePlanIsImmutableAndItsEqualityIsExplicitOrdinalElementByElementAndInOrder()
    {
        var (_, _, plan) = ReadyPlan();
        var (_, _, same) = ReadyPlan();
        var (_, _, otherScale) = ReadyPlan(scale: 3.5);

        Assert.NotSame(plan, same);
        Assert.True(plan.Equals(same));
        Assert.True(plan.Equals((object)same));
        Assert.Equal(plan.GetHashCode(), same.GetHashCode());
        Assert.True(plan.Members[0].Equals(same.Members[0]));
        Assert.True(plan.PostExpectation.Equals(same.PostExpectation));
        Assert.False(plan.Equals(otherScale));
        Assert.False(plan.Equals(null));
        Assert.False(plan.Equals((object)"plan"));

        // The collections cannot be changed through their interfaces, and the plan keeps only strings and values.
        Assert.True(((System.Collections.Generic.ICollection<PushBackPilotMemberPlan>)plan.Members).IsReadOnly);
        Assert.True(((System.Collections.Generic.ICollection<DraftStaleBaseEntry>)plan.ExpectedBase).IsReadOnly);
        Assert.True(((System.Collections.Generic.ICollection<PushBackPilotKeyedText>)plan.PostExpectation.Envelopes).IsReadOnly);
        foreach (var type in new[] { typeof(PushBackPilotUpdatePlan), typeof(PushBackPilotMemberPlan), typeof(PushBackPilotPostExpectation) })
        {
            Assert.All(type.GetProperties(), p => Assert.True(p.SetMethod == null, type.Name + "." + p.Name + " has a setter"));
            Assert.All(type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic),
                f => Assert.True(f.IsInitOnly, type.Name + "." + f.Name + " is mutable"));
        }

        // Ordinal: the same text in another case is another text.
        var member = plan.Members[0];
        var lowered = new PushBackPilotMemberPlan(member.Key, member.View, member.Section, member.DesignText, member.EnvelopeText.ToLowerInvariant());
        Assert.False(member.Equals(lowered));
        Assert.True(member.Equals(new PushBackPilotMemberPlan(member.Key, member.View, member.Section, member.DesignText, member.EnvelopeText)));
        // In order: the same members in another order are another plan.
        var reversed = new PushBackPilotUpdatePlan(
            plan.RackId, plan.OriginKey, plan.ExpectedBase, plan.ExpectedVariableRegistryText, plan.AnnotationScale,
            plan.Members.Reverse(), plan.PostExpectation);
        Assert.False(plan.Equals(reversed));
        var sameAgain = new PushBackPilotUpdatePlan(
            plan.RackId, plan.OriginKey, plan.ExpectedBase, plan.ExpectedVariableRegistryText, plan.AnnotationScale,
            plan.Members, plan.PostExpectation);
        Assert.True(plan.Equals(sameAgain));
        // The scale is compared with Equals: NaN equals NaN and zero equals zero.
        Assert.True(new PushBackPilotPostExpectation(double.NaN, Array.Empty<PushBackPilotKeyedText>())
            .Equals(new PushBackPilotPostExpectation(double.NaN, Array.Empty<PushBackPilotKeyedText>())));
    }

    [Fact]
    public void ThePlanIsBuiltOnlyFromTheRereadSoAChangeOfTheCurrentScaleOnlyChangesTheScaleAndTheTextsThatCarryIt()
    {
        var (_, _, first) = ReadyPlan(scale: 2.25);
        var (_, _, second) = ReadyPlan(scale: 3.5);

        Assert.Equal(first.ExpectedBase.ToArray(), second.ExpectedBase.ToArray());
        Assert.Equal(first.Members.Select(m => (m.Key, m.View, m.Section)).ToArray(), second.Members.Select(m => (m.Key, m.View, m.Section)).ToArray());
        Assert.NotEqual(first.AnnotationScale, second.AnnotationScale);
        Assert.All(first.Members.Zip(second.Members), pair => Assert.NotEqual(pair.First.EnvelopeText, pair.Second.EnvelopeText));
    }

    [Fact]
    public void ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan()
    {
        var origin = PrepareKit.Frontal();
        var withPhantom = PrepareKit.BaseOf(("A1", origin), ("B2", PrepareKit.Lateral(PrepareKit.MissingPost)));
        var draft = PrepareKit.DraftOf(withPhantom, origin);
        PrepareKit.SetScale(draft, 2.25);

        var result = PrepareKit.Prepare(draft, PrepareKit.Reread(withPhantom));

        Assert.Equal(PushBackPilotPrepareKind.Invalid, result.Kind);
        Assert.Null(result.Plan);

        // The same rack without the phantom is Ready, and the plan redraws only the views that exist.
        var without = PrepareKit.BaseOf(("A1", origin), ("B2", PrepareKit.Lateral(PrepareKit.ExistingPost())));
        var clean = PrepareKit.DraftOf(without, origin);
        PrepareKit.SetScale(clean, 2.25);
        var ready = PrepareKit.Prepare(clean, PrepareKit.Reread(without));
        Assert.Equal(PushBackPilotPrepareKind.Ready, ready.Kind);
        Assert.Equal(new[] { "A1", "B2" }, ready.Plan!.Members.Select(m => m.Key).ToArray());
    }

    [Fact]
    public void ReadOnlyAndNonMutableMembersOfTheRereadNeverEnterThePlan()
    {
        var origin = PrepareKit.Frontal();
        var lateral = PrepareKit.Lateral(PrepareKit.ExistingPost());
        var entries = new[]
        {
            new DraftStaleBaseEntry("A1", DraftMembership.Mutable, "frontal", "0", origin),
            new DraftStaleBaseEntry("B2", DraftMembership.Mutable, "lateral", PrepareKit.ExistingPost().ToString(CultureInfo.InvariantCulture), lateral),
            new DraftStaleBaseEntry("X9", DraftMembership.ReadOnly, "planta", "-1", PrepareKit.Planta()),
        };
        var members = DraftStaleBase.Create(entries);
        var draft = PrepareKit.DraftOf(members, origin);
        PrepareKit.SetScale(draft, 2.25);

        var result = PrepareKit.Prepare(draft, PrepareKit.Reread(members));

        Assert.Equal(PushBackPilotPrepareKind.Ready, result.Kind);
        Assert.Equal(new[] { "A1", "B2" }, result.Plan!.Members.Select(m => m.Key).ToArray());
        Assert.Equal(new[] { "A1", "B2", "X9" }, result.Plan.ExpectedBase.Select(e => e.Key).ToArray());
    }

    // ------------------------------------------------------------------ PREPARE changes nothing

    private sealed record DraftHeld(
        DraftStatus Status,
        bool IsDirty,
        bool IsPossiblyStale,
        DraftStaleBase Base,
        PushBackPilotDraftState Initial,
        PushBackPilotDraftState Current,
        string[] Pending,
        string? LastApplyCause,
        int Warnings,
        int ConflictDifferences,
        string Scale);

    private static DraftHeld Hold(DraftSession<PushBackPilotDraftState> draft) => new(
        draft.Status, draft.IsDirty, draft.IsPossiblyStale, draft.StaleBase,
        PushBackPilotStateComparer.Copy(draft.InitialDraftState), PushBackPilotStateComparer.Copy(draft.CurrentDraftState),
        draft.PendingText.Select(p => p.Key + "=" + p.Value).ToArray(), draft.LastApplyCause, draft.Warnings.Count, draft.ConflictDifferences.Count,
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale.ToString("R", CultureInfo.InvariantCulture));

    [Fact]
    public void PrepareNeverChangesTheDraftWhateverItsOutcome()
    {
        var read = IntentKit.Read(PrepareKit.RealScan());
        var changed = IntentKit.Read(PrepareKit.RealScan("pb-02-modulos"));
        var notAdmitted = PrepareKit.Reread(read.StaleBase, notAdmitted: "x");
        var cases = new List<(string Name, DraftSession<PushBackPilotDraftState> Draft, PushBackPilotIntentResult Reread)>();

        var ready = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(ready, 2.25);
        cases.Add(("ready", ready, read));

        var conflict = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(conflict, 2.25);
        cases.Add(("conflict", conflict, changed));

        var refused = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(refused, 2.25);
        cases.Add(("not admitted", refused, notAdmitted));

        var invalid = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(invalid, 0.0);
        cases.Add(("invalid", invalid, read));

        var clean = PrepareKit.DraftOf(read);
        cases.Add(("nothing to apply", clean, read));

        var pending = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(pending, 2.25);
        pending.SetPendingText("AnnotationScale", "3");
        cases.Add(("pending text", pending, read));

        var stale = PrepareKit.DraftOf(read);
        PrepareKit.SetScale(stale, 2.25);
        stale.MarkPossiblyStale();
        cases.Add(("possibly stale", stale, read));

        foreach (var (name, draft, reread) in cases)
        {
            var before = Hold(draft);
            var comparer = PushBackPilotStateComparer.Instance;

            PrepareKit.Prepare(draft, reread);
            PrepareKit.Prepare(draft, reread);

            var after = Hold(draft);
            Assert.True(before.Status == after.Status, name);
            Assert.Equal(before.IsDirty, after.IsDirty);
            Assert.Equal(before.IsPossiblyStale, after.IsPossiblyStale);
            Assert.Same(before.Base, after.Base);
            Assert.True(comparer.Equals(before.Initial, after.Initial), name);
            Assert.True(comparer.Equals(before.Current, after.Current), name);
            Assert.Equal(before.Pending, after.Pending);
            Assert.Equal(before.LastApplyCause, after.LastApplyCause);
            Assert.Equal(before.Warnings, after.Warnings);
            Assert.Equal(before.ConflictDifferences, after.ConflictDifferences);
            Assert.Equal(before.Scale, after.Scale);
        }
    }

    [Fact]
    public void PrepareDoesNotPutTheDraftInConflictNorCleanDirtyNorTakeTheSlotOfTheUpdate()
    {
        var registry = new DraftRegistry();
        var read = IntentKit.Read(PrepareKit.RealScan());
        var begun = registry.Begin(DraftKit.S1, PrepareKit.Rack, read.Read, PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy);
        var draft = begun.Draft!;
        PrepareKit.SetScale(draft, 2.25);
        var changed = IntentKit.Read(PrepareKit.RealScan("pb-02-modulos"));

        var conflict = PrepareKit.Prepare(draft, changed);

        Assert.Equal(PushBackPilotPrepareKind.Conflict, conflict.Kind);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Empty(draft.ConflictDifferences);
        Assert.False(registry.HasUpdateInProgress);
        Assert.True(draft.IsDirty);
    }

    // ================================================================== the types of the plan name nothing of the drawing

    [Fact]
    public void ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing()
    {
        foreach (var type in new[]
                 {
                     typeof(PushBackPilotPrepareInput), typeof(PushBackPilotPrepareResult), typeof(PushBackPilotUpdatePlan),
                     typeof(PushBackPilotMemberPlan), typeof(PushBackPilotPostExpectation), typeof(PushBackPilotKeyedText),
                     typeof(PushBackPilotUpdatePlanner), typeof(PushBackPilotPrepareKind),
                 })
        {
            Assert.Equal("RackCad.Application.Systems.PushBack.Pilot", type.Namespace);
        }

        var members = typeof(PushBackPilotUpdatePlanner).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly);
        var prepare = Assert.Single(members, m => m.Name == "Prepare");
        Assert.Equal(
            new[] { typeof(PushBackPilotPrepareInput), typeof(PushBackPilotIntentResult), typeof(RackCatalog) },
            prepare.GetParameters().Select(p => p.ParameterType).ToArray());
        Assert.Equal(typeof(PushBackPilotPrepareResult), prepare.ReturnType);
    }
}
