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

    public static void SetScale(DraftSession<PushBackPilotDraftState> draft, double scale) =>
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = scale;

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

    // ================================================================== INV-F4T4A1-02: ConfirmBase over the draft session

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
    public void TheThreeOperationsAnswerUnavailableWithoutAReaderAndReadNothing()
    {
        var rig = new Rig(assignReader: false);

        Assert.Equal(EditConfirmStatus.Unavailable, rig.Core.TryConfirmPilotBase(rig.Request()).Status);
        Assert.Equal(EditIntentStatus.Unavailable, rig.Core.TryKeepPilotEdit(rig.Request()).Status);
        Assert.Equal(EditIntentStatus.Unavailable, rig.Core.TryDiscardAndReloadPilotEdit(rig.Request()).Status);
        Assert.Empty(rig.Reader.Calls);
    }

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Theory(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Theory(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Theory(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Theory(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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
        Assert.Equal(PushBackPilotIntentReader.UnreadableDiagnostic, outcome.AdmissionDiagnostic);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Same(before, draft.StaleBase);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
    }

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
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

    [Fact(Skip = "I-64 F4B PB-T4b: sustituida por su copia en WorkspacePilotPrepareFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).")]
    public void TheSevenOutcomesAreEvaluatedInTheDeclaredOrder()
    {
        Assert.Equal(
            new[] { "NotApplicable", "PendingText", "NothingToApply", "Conflict", "NotAdmitted", "Invalid", "Ready" },
            Enum.GetNames<PushBackPilotPrepareKind>());
    }

    // ================================================================== INV-F4T4A1-07, 08, 09: the Invalid cases (a) to (e) and the plan

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

    // M-6 (I-64 F4-T4B): the missing catalog is answered at the start of Build, so a difference of the base and a read that is not admitted
    // are told as such even when the catalog is missing; with a read that matches and is admitted, the missing catalog is still Invalid.

    // ------------------------------------------------------------------ the plan

    // ------------------------------------------------------------------ PREPARE changes nothing

    // ================================================================== the types of the plan name nothing of the drawing

}
