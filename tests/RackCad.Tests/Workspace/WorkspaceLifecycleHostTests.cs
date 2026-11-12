#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>I-64 F4-T2B2B test double of <see cref="IWorkspaceLifecycleAdvisor"/>: it records every call and answers as the test programs it.</summary>
internal sealed class LifecycleScriptedAdvisor : IWorkspaceLifecycleAdvisor
{
    public Func<SessionId, CloseDecision> Advise { get; set; } = _ => CloseDecision.Allow;

    public Action<SessionId, bool>? OnDestroyed { get; set; }

    public List<SessionId> Advised { get; } = new();

    public List<(SessionId Session, bool Intact)> Destroyed { get; } = new();

    public CloseDecision AdviseClose(SessionId session)
    {
        Advised.Add(session);
        return Advise(session);
    }

    public void SessionDestroyed(SessionId session, bool closeSequenceIntact)
    {
        Destroyed.Add((session, closeSequenceIntact));
        OnDestroyed?.Invoke(session, closeSequenceIntact);
    }
}

// I-64 F4-T2B2B-LIFECYCLE (INV-F4T2B2B-01, 03 to 08 and 10; D-T2B2-09 to 15; F4-E29): the lifecycle of the drafts of the pilot in the host.
// The pure bridge core asks an advisor about the close of a document and tells it about the destruction of a session; the host core is
// that advisor. No AutoCAD, no UI and no write to the drawing: doubles of the bridge, the probe and the readers, and the real pure cores.
// Evidence named by the package: obligation 5 (no write: WorkspaceLifecycleGuardTests), 14 (destroying the document retires the session
// and its drafts, and a loss is noticed), 15 (a dirty or pending draft takes part in the veto) and 18 (no object of the host lives past the
// call: the cores hold none). Every expectation that a failure is contained uses Record.Exception, so a behavior that is missing fails by
// ASSERTION in the RED and never by an escaped exception.
public class WorkspaceLifecycleHostTests
{
    private const string RackA = EditIntentKit.RackA;
    private const string RackB = EditIntentKit.RackB;

    private sealed class Env
    {
        public Env(bool wire = true, bool reader = true)
        {
            Hr = new WorkspaceHostRig().WithDocument(100);
            Hr.Host.OpenPanel();
            Session = Hr.ActiveSession;
            Hr.Idle();
            Hr.Reader.Reads.Clear();
            Hr.Reader.Invalidations.Clear();
            Reader = new EditIntentFakeReader();
            if (reader) Core.SetEditIntentReader(Reader);
            if (wire) Bridge.SetLifecycleAdvisor(Core);
        }

        public WorkspaceHostRig Hr { get; }

        public WorkspaceHostCore Core => Hr.Host;

        public EventBridgeCore Bridge => Hr.Bridge.Core;

        public IWorkspaceLifecycleAdvisor Advisor => Core;

        public EditIntentFakeReader Reader { get; }

        public SessionId Session { get; }

        public CloseDecision Close(SessionId? session = null) => Bridge.OnBeginDocumentClose(session ?? Session);

        public void Destroy(long nativeId = 100) => Bridge.OnDocumentToBeDestroyed(nativeId);

        public EditIntentRequest Request(string rack = RackA, SessionId? session = null) => new(session ?? Session, rack, null, null);

        /// <summary>A draft of the generic state, born without any read (the comparer is the one the test chooses).</summary>
        public DraftSession<DraftForm> Seed(string rack, IEqualityComparer<DraftForm>? comparer = null, SessionId? session = null)
        {
            var begun = Core.OpenDrafts.Begin(session ?? Session, rack, DraftKit.Read(), comparer ?? new DraftFormComparer(), DraftKit.CopyForm);
            Assert.Equal(DraftBeginStatus.Created, begun.Status);
            return begun.Draft!;
        }

        /// <summary>A draft of the pilot, born through the intention of the host from the admitted read of the double.</summary>
        public DraftSession<PushBackPilotDraftState> Pilot(string rack = RackA, SessionId? session = null)
        {
            var outcome = Core.TryBeginPilotEdit(Request(rack, session));
            Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
            var draft = Core.OpenDrafts.Find<PushBackPilotDraftState>(session ?? Session, rack);
            Assert.NotNull(draft);
            return draft!;
        }

        public DraftSession<PushBackPilotDraftState> UnknownPilot(string rack = RackA)
        {
            var draft = Pilot(rack);
            Assert.True(draft.TryBeginApply().IsStarted);
            draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the commit threw"));
            Assert.Equal(DraftStatus.Unknown, draft.Status);
            return draft;
        }

        /// <summary>A hint that carries the stale mark, then the idle pass that drains it (the host receives the outcome).</summary>
        public void DrainWithStaleHint(BridgeSource source = BridgeSource.ObjectModified)
        {
            Bridge.OnSourceEvent(Session, source);
            Hr.Idle();
        }

        public DraftSession<PushBackPilotDraftState> StalePilot(string rack = RackA)
        {
            var draft = Pilot(rack);
            DrainWithStaleHint();
            Assert.True(draft.IsPossiblyStale);
            Assert.True(draft.NeedsLazyReload);
            return draft;
        }

        public static void MakeDirty(DraftSession<PushBackPilotDraftState> draft) =>
            draft.CurrentDraftState.Inputs.Annotations.AnnotationScale += 100.0;
    }

    private static DrainOutcome Outcome(SessionId session, params DrainOperation[] operations) => new(session, new DrainResult(true, operations));

    // ------------------------------------------------------------------ INV-F4T2B2B-01: the advisor and the bridge

    [Fact]
    public void TheAdvisorIsAssignedByAssignmentAndANullAdvisorIsRefused()
    {
        var rig = new WorkspaceBridgeRig();

        var thrown = Record.Exception(() => rig.Core.SetLifecycleAdvisor(null!));

        Assert.IsType<ArgumentNullException>(thrown);
        Assert.Single(typeof(EventBridgeCore).GetConstructors());
        Assert.Equal(2, typeof(EventBridgeCore).GetConstructors()[0].GetParameters().Length);
    }

    [Fact]
    public void WithoutAnAdvisorTheCloseIsAllowedAsBeforeAndTheDestructionNeedsNone()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);

        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));
        var thrown = Record.Exception(() => rig.Core.OnDocumentToBeDestroyed(100));

        Assert.Null(thrown);
        Assert.False(rig.Core.TryGetSession(a, out _));
        Assert.Empty(rig.Faults);
    }

    [Theory]
    [InlineData(CloseDecision.Allow)]
    [InlineData(CloseDecision.Veto)]
    public void WithAnAdvisorTheDecisionOfTheCloseIsTheOneOfTheAdvisor(CloseDecision answer)
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);
        var advisor = new LifecycleScriptedAdvisor { Advise = session => session == a ? answer : CloseDecision.Allow };
        rig.Core.SetLifecycleAdvisor(advisor);

        Assert.Equal(answer, rig.Core.OnBeginDocumentClose(a));
        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(b));
        Assert.Equal(new[] { a, b }, advisor.Advised.ToArray());
    }

    [Fact]
    public void ASecondCallToSetLifecycleAdvisorReplacesTheAdvisor()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var first = new LifecycleScriptedAdvisor { Advise = _ => CloseDecision.Veto };
        var second = new LifecycleScriptedAdvisor { Advise = _ => CloseDecision.Allow };
        rig.Core.SetLifecycleAdvisor(first);
        rig.Core.SetLifecycleAdvisor(second);

        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));
        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Empty(first.Advised);
        Assert.Empty(first.Destroyed);
        Assert.Single(second.Advised);
        Assert.Single(second.Destroyed);
    }

    [Fact]
    public void AnAdvisorThatThrowsGivesVetoIsLoggedWithoutFaultAndDoesNotDegradeTheSession()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var advisor = new LifecycleScriptedAdvisor { Advise = _ => throw new InvalidOperationException("advisor boom") };
        rig.Core.SetLifecycleAdvisor(advisor);
        var hookups = rig.Source.Total;

        CloseDecision decision = CloseDecision.Allow;
        var thrown = Record.Exception(() => decision = rig.Core.OnBeginDocumentClose(a));

        Assert.Null(thrown);
        Assert.Equal(CloseDecision.Veto, decision);
        var fault = Assert.Single(rig.Faults);
        Assert.Contains("OnBeginDocumentClose", fault.Context);
        Assert.Equal("advisor boom", fault.Error.Message);
        Assert.False(rig.Core.IsDegraded(a));
        Assert.Equal(hookups, rig.Source.Total);
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(a));

        // The advisor is asked anew at the next attempt: a Veto is never remembered as a decision.
        advisor.Advise = _ => CloseDecision.Allow;
        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));
    }

    [Fact]
    public void TheCloseOfADegradedSessionStillGoesThroughTheAdvisorSoItSurvivesTheDegradation()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var advisor = new LifecycleScriptedAdvisor { Advise = _ => CloseDecision.Veto };
        rig.Core.SetLifecycleAdvisor(advisor);
        rig.Core.ReportFault(a, "test", new InvalidOperationException("degrade it"));
        Assert.True(rig.Core.IsDegraded(a));

        Assert.Equal(CloseDecision.Veto, rig.Core.OnBeginDocumentClose(a));
        Assert.Equal(new[] { a }, advisor.Advised.ToArray());
        Assert.Equal(1, rig.Source.CountFor(a, BridgeSource.BeginDocumentClose));
    }

    [Fact]
    public void TheDestructionTellsTheAdvisorSynchronouslyAfterTheSessionLeftTheRegistryAndTheActiveSlotForAnActiveAndAnInactiveSession()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);
        var seen = new List<(SessionId Session, bool Registered, SessionId? Active)>();
        var advisor = new LifecycleScriptedAdvisor
        {
            OnDestroyed = (session, intact) => seen.Add((session, rig.Core.TryGetSession(session, out _), rig.Core.ActiveSession)),
        };
        rig.Core.SetLifecycleAdvisor(advisor);

        rig.Core.OnDocumentToBeDestroyed(100);
        var afterInactive = advisor.Destroyed.ToArray();
        rig.Core.OnDocumentToBeDestroyed(200);

        Assert.Equal(new[] { a }, afterInactive.Select(d => d.Session).ToArray());
        Assert.Equal(new[] { a, b }, advisor.Destroyed.Select(d => d.Session).ToArray());
        Assert.Equal((a, false, (SessionId?)b), seen[0]);
        Assert.Equal((b, false, (SessionId?)null), seen[1]);
        Assert.Equal(2, seen.Count);
    }

    [Fact]
    public void ADestructionWithAnUnknownNativeIdTellsNobody()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        rig.Open(100);
        var advisor = new LifecycleScriptedAdvisor();
        rig.Core.SetLifecycleAdvisor(advisor);

        var thrown = Record.Exception(() => rig.Core.OnDocumentToBeDestroyed(999));

        Assert.Null(thrown);
        Assert.Empty(advisor.Destroyed);
        Assert.Equal(1, rig.Core.LiveSessionCount);
    }

    [Fact]
    public void AnAdvisorThatThrowsWhenTheSessionIsDestroyedStopsNothingOfTheDestructionAndTheNextIdIsANewSession()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);
        Assert.True(rig.Core.IsIdleAttached);
        var advisor = new LifecycleScriptedAdvisor { OnDestroyed = (_, _) => throw new InvalidOperationException("destroyed boom") };
        rig.Core.SetLifecycleAdvisor(advisor);

        var thrown = Record.Exception(() => rig.Core.OnDocumentToBeDestroyed(100));

        Assert.Null(thrown);
        Assert.Single(advisor.Destroyed);
        Assert.False(rig.Core.TryGetSession(a, out _));
        Assert.Null(rig.Core.ActiveSession);
        Assert.Equal(0, rig.Core.LiveSessionCount);
        Assert.False(rig.Core.IsIdleAttached);
        Assert.Equal(0, rig.Source.CountForSession(a));
        Assert.Equal(RequestDecision.RejectedSessionDestroyed, rig.Core.Authorize(new WorkspaceRequest(a), 100));
        Assert.Contains(rig.Faults, f => f.Error.Message == "destroyed boom");

        // The native identifier is recycled by the host: it is a NEW session, never the destroyed one.
        var recycled = rig.Open(100);
        Assert.NotEqual(a, recycled);
        Assert.Equal(RequestDecision.RejectedSessionDestroyed, rig.Core.Authorize(new WorkspaceRequest(a), 100));
        Assert.Equal(RequestDecision.Accepted, rig.Core.Authorize(new WorkspaceRequest(recycled), 100));
    }

    private sealed class HostThenThrowingAdvisor : IWorkspaceLifecycleAdvisor
    {
        private readonly IWorkspaceLifecycleAdvisor _inner;

        public HostThenThrowingAdvisor(IWorkspaceLifecycleAdvisor inner)
        {
            _inner = inner;
        }

        public CloseDecision AdviseClose(SessionId session) => _inner.AdviseClose(session);

        public void SessionDestroyed(SessionId session, bool closeSequenceIntact)
        {
            _inner.SessionDestroyed(session, closeSequenceIntact);
            throw new InvalidOperationException("after the host");
        }
    }

    [Fact]
    public void ARecycledNativeIdAfterAnAdvisorThatThrewHasANewSessionWithoutDraftsAndTheOldOneIsRetired()
    {
        var env = new Env(wire: false);
        var old = env.Session;
        env.Seed("R1", new DraftThrowingComparer { Throw = true });
        env.Bridge.SetLifecycleAdvisor(new HostThenThrowingAdvisor(env.Core));

        var thrown = Record.Exception(() => env.Destroy());
        env.Hr.Probe.ActiveDocumentNativeId = null;
        var fresh = env.Hr.Activate(100);

        Assert.Null(thrown);
        Assert.NotEqual(old, fresh);
        Assert.True(env.Core.OpenDrafts.IsRetired(old));
        Assert.Equal(0, env.Core.OpenDrafts.CountOf(old));
        Assert.Equal(0, env.Core.OpenDrafts.CountOf(fresh));
        Assert.False(env.Core.OpenDrafts.IsRetired(fresh));
        Assert.Equal(RequestDecision.RejectedSessionDestroyed, env.Core.Authorize(new WorkspaceRequest(old)));
        Assert.Equal(RequestDecision.Accepted, env.Core.Authorize(new WorkspaceRequest(fresh)));
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-04: the close sequence observed by the bridge

    private static (WorkspaceBridgeRig Rig, LifecycleScriptedAdvisor Advisor, SessionId A, SessionId B) SequenceRig()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);
        rig.Core.OnDocumentActivated(100, true);
        var advisor = new LifecycleScriptedAdvisor();
        rig.Core.SetLifecycleAdvisor(advisor);
        return (rig, advisor, a, b);
    }

    [Fact]
    public void AnAllowedCloseFollowedDirectlyByTheDestructionLeavesTheSequenceIntactForAnActiveAndAnInactiveSession()
    {
        var (rig, advisor, a, b) = SequenceRig();

        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));
        rig.Core.OnDocumentToBeDestroyed(100);
        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(b));
        rig.Core.OnDocumentToBeDestroyed(200);

        Assert.Equal(new[] { (a, true), (b, true) }, advisor.Destroyed.ToArray());
    }

    [Theory]
    [InlineData("source-event")]
    [InlineData("idle")]
    [InlineData("document-created")]
    [InlineData("activated-same")]
    [InlineData("activated-other")]
    [InlineData("no-active-document")]
    [InlineData("reconciliation")]
    [InlineData("fault-of-another-session")]
    [InlineData("panel-visible")]
    [InlineData("modal")]
    [InlineData("start")]
    [InlineData("destroy-unknown-native-id")]
    [InlineData("destroy-another-document")]
    [InlineData("close-of-another-session")]
    [InlineData("a-second-close-of-the-same-session-that-is-vetoed")]
    public void AnyOtherInputThatChangesTheStateOfTheCoreBetweenTheAllowAndTheDestructionBreaksTheSequence(string input)
    {
        var (rig, advisor, a, b) = SequenceRig();
        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));

        switch (input)
        {
            case "source-event": rig.Core.OnSourceEvent(a, BridgeSource.CommandEnded); break;
            case "idle": rig.Core.OnIdle(); break;
            case "document-created": rig.Core.OnDocumentCreated(300); break;
            case "activated-same": rig.Core.OnDocumentActivated(100, true); break;
            case "activated-other": rig.Core.OnDocumentActivated(200, true); break;
            case "no-active-document": rig.Core.OnNoActiveDocument(); break;
            case "reconciliation": rig.Core.RequestReconciliation(); break;
            case "fault-of-another-session": rig.Core.ReportFault(b, "test", new InvalidOperationException("boom")); break;
            case "panel-visible": rig.Core.SetPanelVisible(false); break;
            case "modal": rig.Core.SetModalRackCadActive(true); break;
            case "start": rig.Core.Start(); break;
            case "destroy-unknown-native-id": rig.Core.OnDocumentToBeDestroyed(999); break;
            case "destroy-another-document": rig.Core.OnDocumentToBeDestroyed(200); break;
            case "close-of-another-session": rig.Core.OnBeginDocumentClose(b); break;
            default:
                advisor.Advise = _ => CloseDecision.Veto;
                Assert.Equal(CloseDecision.Veto, rig.Core.OnBeginDocumentClose(a));
                break;
        }

        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal((a, false), Assert.Single(advisor.Destroyed, d => d.Session == a));
    }

    [Fact]
    public void ADestructionWithoutItsOwnCloseNeverHasAnIntactSequence()
    {
        var (rig, advisor, a, b) = SequenceRig();

        rig.Core.OnDocumentToBeDestroyed(100);
        rig.Core.OnDocumentToBeDestroyed(200);

        Assert.Equal(new[] { (a, false), (b, false) }, advisor.Destroyed.ToArray());
    }

    [Fact]
    public void AnAllowedCloseOfAnotherSessionDoesNotMakeTheSequenceOfThisOneIntact()
    {
        var (rig, advisor, a, b) = SequenceRig();

        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(b));
        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal((a, false), Assert.Single(advisor.Destroyed));
    }

    [Fact]
    public void AnAdvisorThatThrowsAtTheSecondCloseBreaksTheSequenceOfTheFirstAllow()
    {
        var (rig, advisor, a, _) = SequenceRig();
        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));
        advisor.Advise = _ => throw new InvalidOperationException("boom");
        Assert.Equal(CloseDecision.Veto, rig.Core.OnBeginDocumentClose(a));

        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal((a, false), Assert.Single(advisor.Destroyed));
    }

    [Fact]
    public void ASessionDegradedBeforeTheAllowIsNeverIntactEvenWithNoInputBetweenTheAllowAndTheDestruction()
    {
        var (rig, advisor, a, _) = SequenceRig();
        rig.Core.ReportFault(a, "test", new InvalidOperationException("degrade it"));
        Assert.True(rig.Core.IsDegraded(a));

        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));
        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal((a, false), Assert.Single(advisor.Destroyed));
    }

    [Fact]
    public void TheQueriesAndTheAssignmentOfTheAdvisorDoNotEndTheSequence()
    {
        var (rig, advisor, a, _) = SequenceRig();
        Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));

        _ = rig.Core.Authorize(new WorkspaceRequest(a), 100);
        _ = rig.Core.TryGetSession(a, out _);
        _ = rig.Core.TryGetSession(100L, out _);
        _ = rig.Core.IsDegraded(a);
        _ = rig.Core.ActiveSession;
        _ = rig.Core.IsStarted;
        _ = rig.Core.IsIdleAttached;
        _ = rig.Core.LiveSessionCount;
        _ = rig.Core.AttachedSources;
        _ = rig.Core.IsCommandActiveInActiveSession;
        rig.Core.SetLifecycleAdvisor(advisor);
        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal((a, true), Assert.Single(advisor.Destroyed));
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-03: the table of the Owner (the veto)

    [Fact]
    public void ADraftThatIsDirtyForAKnownComparerVetoesTheClose()
    {
        var env = new Env();
        var draft = env.Seed("R1");
        draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "Edited" });

        Assert.Equal(CloseDecision.Veto, env.Close());
        Assert.True(draft.IsRegistered);
        Assert.Equal(1, env.Core.OpenDrafts.CountOf(env.Session));
    }

    [Fact]
    public void ADraftWithPendingTextVetoesTheCloseWithoutAskingTheComparer()
    {
        var env = new Env();
        var comparer = new ContainmentCountingComparer { Throw = true };
        var draft = env.Seed("R1", comparer);
        draft.SetPendingText("name", "typed but not applied");
        var before = comparer.Calls;

        Assert.Equal(CloseDecision.Veto, env.Close());
        Assert.Equal(before, comparer.Calls);
        Assert.Empty(env.Hr.Faults);
        Assert.True(draft.IsRegistered);
    }

    [Fact]
    public void ADraftThatMayBeDirtyBecauseTheComparerFailedVetoesTheCloseIsLoggedAndDoesNotDegradeTheSession()
    {
        var env = new Env();
        var comparer = new DraftThrowingComparer();
        var draft = env.Seed("R1", comparer);
        comparer.Throw = true;

        CloseDecision decision = CloseDecision.Allow;
        var thrown = Record.Exception(() => decision = env.Close());

        Assert.Null(thrown);
        Assert.Equal(CloseDecision.Veto, decision);
        var fault = Assert.Single(env.Hr.Faults);
        Assert.Equal("WorkspaceHostCore.AdviseClose", fault.Context);
        Assert.IsType<InvalidOperationException>(fault.Error);
        Assert.Empty(env.Hr.Bridge.Faults);
        Assert.False(env.Bridge.IsDegraded(env.Session));
        Assert.Equal(WorkspaceHostStatus.Ready, env.Core.GetSnapshot().Status);
        Assert.True(draft.IsRegistered);
    }

    [Fact]
    public void ADraftThatIsKnownCleanLetsTheNormalCloseProceedAndTheCloseRemovesNothing()
    {
        var env = new Env();
        var draft = env.Seed("R1");
        var other = env.Pilot();

        Assert.Equal(CloseDecision.Allow, env.Close());

        Assert.True(draft.IsRegistered);
        Assert.True(other.IsRegistered);
        Assert.Equal(2, env.Core.OpenDrafts.CountOf(env.Session));
        Assert.False(env.Core.OpenDrafts.IsRetired(env.Session));
    }

    [Fact]
    public void ADirtyDraftOfThePilotVetoesTheCloseAndACleanOneAllowsIt()
    {
        var env = new Env();
        var draft = env.Pilot();
        Assert.Equal(CloseDecision.Allow, env.Close());

        Env.MakeDirty(draft);

        Assert.Equal(CloseDecision.Veto, env.Close());
        Assert.True(draft.IsDirty);
    }

    [Fact]
    public void AnExplicitDiscardOfTheHostRemovesTheDraftsAndTheNextCloseProceedsEvenWithAComparerThatThrows()
    {
        var env = new Env();
        var comparer = new DraftThrowingComparer();
        var failing = env.Seed("R1", comparer);
        var dirty = env.Seed("R2");
        dirty.SetCurrentDraftState(dirty.CurrentDraftState with { Name = "Edited" });
        comparer.Throw = true;
        Assert.Equal(CloseDecision.Veto, env.Close());

        EditDiscardOutcome outcome = default;
        var thrown = Record.Exception(() => outcome = env.Core.DiscardPilotEdits(env.Session));

        Assert.Null(thrown);
        Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
        Assert.Equal(new[] { "R1", "R2" }, outcome.RemovedRackIds.ToArray());
        Assert.Empty(outcome.PendingTextRackIds);
        Assert.False(failing.IsRegistered);
        Assert.False(dirty.IsRegistered);
        Assert.Equal(CloseDecision.Allow, env.Close());
    }

    [Fact]
    public void AnExplicitDiscardOfTheRegistryThatNeedsNoComparerAlsoLetsTheNextCloseProceed()
    {
        var env = new Env();
        var comparer = new DraftThrowingComparer();
        env.Seed("R1", comparer);
        comparer.Throw = true;
        Assert.Equal(CloseDecision.Veto, env.Close());

        DraftUserDiscardAllResult? result = null;
        var thrown = Record.Exception(() => result = env.Core.OpenDrafts.DiscardAllByUser(env.Session));

        Assert.Null(thrown);
        Assert.Equal(new[] { "R1" }, result!.RemovedRackIds.ToArray());
        Assert.Equal(CloseDecision.Allow, env.Close());
    }

    [Fact]
    public void EachSuccessiveAttemptToCloseIsVetoedAgainRepeatingIsNeverConsent()
    {
        var env = new Env();
        var draft = env.Seed("R1");
        draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "Edited" });

        for (var attempt = 0; attempt < 4; attempt++)
            Assert.Equal(CloseDecision.Veto, env.Close());

        Assert.True(draft.IsRegistered);
        Assert.True(draft.IsDirty);
        Assert.Equal(1, env.Core.OpenDrafts.CountOf(env.Session));
        Assert.Empty(env.Core.LossNotices);

        draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "Rack" });
        Assert.Equal(CloseDecision.Allow, env.Close());
    }

    [Fact]
    public void ASessionWithoutDraftsAllowsTheCloseAndTheCloseReadsAndWritesNothing()
    {
        var env = new Env();

        Assert.Equal(CloseDecision.Allow, env.Close());

        Assert.Empty(env.Reader.Calls);
        Assert.Equal(0, env.Hr.Reader.ReadCalls);
        Assert.Empty(env.Hr.Reader.Invalidations);
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void TheVetoIsPerSessionADirtyDraftOfAnotherDocumentDoesNotVetoThisOne()
    {
        var env = new Env();
        var b = env.Hr.Activate(200);
        var dirty = env.Seed("R1", session: b);
        dirty.SetCurrentDraftState(dirty.CurrentDraftState with { Name = "Edited" });

        Assert.Equal(CloseDecision.Allow, env.Close(env.Session));
        Assert.Equal(CloseDecision.Veto, env.Close(b));
    }

    [Fact]
    public void TheCloseOfADegradedSessionWithADirtyDraftIsVetoedAndTheNextOneProceedsAfterTheDiscard()
    {
        // Degraded, edited, dirty, closed, vetoed: the close survives the degradation (BridgeSource, EventBridgeCore).
        var env = new Env();
        var draft = env.Seed("R1");
        env.Bridge.ReportFault(env.Session, "test", new InvalidOperationException("degrade it"));
        Assert.True(env.Bridge.IsDegraded(env.Session));
        draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "Edited" });

        Assert.Equal(CloseDecision.Veto, env.Close());
        Assert.Equal(CloseDecision.Veto, env.Close());
        Assert.Equal(EditDiscardStatus.Discarded, env.Core.DiscardPilotEdits(env.Session).Status);
        Assert.Equal(CloseDecision.Allow, env.Close());
        Assert.Equal(1, env.Hr.Bridge.Source.CountFor(env.Session, BridgeSource.BeginDocumentClose));
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-04: the mark of the host and the way a session is retired

    // After the Allow, a draft is seeded whose comparer ALWAYS throws and whose state is clean. Retiring with RetireKnownSafe never asks the
    // comparer, so nothing is lost; RetireConservative asks it, and the draft is reported as lost and UNCERTAIN. The notice therefore
    // tells which way the host retired the session, without any other probe.
    private static string PathAfter(Action<Env>? before, Action<Env>? between, bool intact = true, bool allow = true, Action<Env>? afterwards = null)
    {
        var env = new Env(wire: false);
        before?.Invoke(env);
        if (allow) Assert.Equal(CloseDecision.Allow, env.Advisor.AdviseClose(env.Session));
        between?.Invoke(env);
        env.Seed("R9", new DraftThrowingComparer { Throw = true });
        afterwards?.Invoke(env);

        var thrown = Record.Exception(() => env.Advisor.SessionDestroyed(env.Session, intact));

        Assert.Null(thrown);
        Assert.True(env.Core.OpenDrafts.IsRetired(env.Session));
        Assert.Equal(0, env.Core.OpenDrafts.CountOf(env.Session));
        if (env.Core.LossNotices.Count == 0) return "known-safe";
        var notice = Assert.Single(env.Core.LossNotices);
        Assert.Contains("R9", notice.UncertainRackIds);
        return "conservative";
    }

    [Fact]
    public void AnAllowFollowedByTheDestructionWithAnIntactSequenceRetiresWithoutAskingTheComparer()
    {
        Assert.Equal("known-safe", PathAfter(null, null));
    }

    [Fact]
    public void ADestructionThatTheBridgeDidNotSeeAfterItsAllowRetiresConservativelyEvenWithTheMarkOfTheHost()
    {
        Assert.Equal("conservative", PathAfter(null, null, intact: false));
    }

    [Fact]
    public void ADestructionWithoutTheMarkOfTheHostRetiresConservativelyEvenWithAnIntactSequence()
    {
        Assert.Equal("conservative", PathAfter(null, null, allow: false));
    }

    [Theory]
    [InlineData("a-veto")]
    [InlineData("a-failure-of-the-evaluation")]
    [InlineData("an-admitted-intention")]
    [InlineData("a-check-that-finds-a-conflict")]
    [InlineData("a-check-that-recovers")]
    [InlineData("a-reload")]
    [InlineData("a-discard-of-the-host")]
    [InlineData("a-drain")]
    [InlineData("a-drain-with-the-stale-hint")]
    [InlineData("a-note-of-an-edit")]
    public void EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively(string operation)
    {
        Action<Env>? before = operation switch
        {
            "a-check-that-finds-a-conflict" or "a-check-that-recovers" => env => env.UnknownPilot(),
            "a-reload" => env => env.StalePilot(),
            _ => null,
        };

        Action<Env> between = operation switch
        {
            "a-veto" => env =>
            {
                var dirty = env.Seed("R1");
                dirty.SetCurrentDraftState(dirty.CurrentDraftState with { Name = "Edited" });
                Assert.Equal(CloseDecision.Veto, env.Advisor.AdviseClose(env.Session));
                env.Core.OpenDrafts.DiscardByUser(env.Session, "R1");
            },
            "a-failure-of-the-evaluation" => env =>
            {
                var comparer = new DraftThrowingComparer();
                env.Seed("R1", comparer);
                comparer.Throw = true;
                Assert.Equal(CloseDecision.Veto, env.Advisor.AdviseClose(env.Session));
                env.Core.OpenDrafts.DiscardByUser(env.Session, "R1");
            },
            "an-admitted-intention" => env => Assert.Equal(EditIntentStatus.Admitted, env.Core.TryBeginPilotEdit(env.Request()).Status),
            "a-check-that-finds-a-conflict" => env =>
            {
                env.Reader.Answer = EditIntentKit.MemberBecameBlocking;
                Assert.Equal(EditCheckStatus.Conflict, env.Core.TryCheckPilotEdit(env.Request()).Status);
            },
            "a-check-that-recovers" => env => Assert.Equal(EditCheckStatus.Recovered, env.Core.TryCheckPilotEdit(env.Request()).Status),
            "a-reload" => env => Assert.Equal(EditIntentStatus.Reloaded, env.Core.TryReloadPilotEdit(env.Request()).Status),
            "a-discard-of-the-host" => env => Assert.Equal(EditDiscardStatus.Discarded, env.Core.DiscardPilotEdits(env.Session).Status),
            "a-drain" => env => env.Core.OnDrained(Outcome(env.Session)),
            "a-drain-with-the-stale-hint" => env => env.Core.OnDrained(Outcome(env.Session, DrainOperation.MarkDraftsPossiblyStale)),
            _ => env => env.Core.NotePilotEdit(env.Session),
        };

        Assert.Equal("conservative", PathAfter(before, between));
    }

    [Fact]
    public void ADiscardThatTheAuthorizationRefusesHasNoEffectSoItDoesNotEndTheMark()
    {
        var path = PathAfter(null, env => Assert.Equal(EditDiscardStatus.Rejected, RefusedDiscard(env).Status));

        Assert.Equal("known-safe", path);
    }

    private static EditDiscardOutcome RefusedDiscard(Env env)
    {
        env.Hr.Activate(200);
        return env.Core.DiscardPilotEdits(env.Session);
    }

    [Fact]
    public void AnInPlaceEditAfterTheAllowThatIsNotNotedIsTheExactResidualOfTheDesignAndANoteMakesTheRetirementConservative()
    {
        // The residual declared by D-T2B2-10: a mutation WITHOUT NotePilotEdit after an Allow, with a destruction that the bridge saw as
        // intact, is retired as known safe. With the note, the retirement is conservative and the dirty draft is reported as lost.
        string Run(bool noted)
        {
            var env = new Env();
            var draft = env.Pilot();
            Assert.Equal(CloseDecision.Allow, env.Close());
            Env.MakeDirty(draft);
            if (noted) env.Core.NotePilotEdit(env.Session);

            env.Destroy();

            return env.Core.LossNotices.Count == 0 ? "silent" : "reported:" + string.Join(",", Assert.Single(env.Core.LossNotices).LostRackIds);
        }

        Assert.Equal("silent", Run(noted: false));
        Assert.Equal("reported:" + RackA, Run(noted: true));
    }

    [Fact]
    public void TheMarkIsPerSessionAnAllowOfOneSessionNeverMarksAnother()
    {
        var env = new Env(wire: false);
        var b = env.Hr.Activate(200);
        env.Seed("R1", new DraftThrowingComparer { Throw = true }, b);
        Assert.Equal(CloseDecision.Allow, env.Advisor.AdviseClose(env.Session));

        env.Advisor.SessionDestroyed(b, closeSequenceIntact: true);

        var notice = Assert.Single(env.Core.LossNotices);
        Assert.Equal(b, notice.Session);
        Assert.Equal(new[] { "R1" }, notice.UncertainRackIds.ToArray());
    }

    [Fact]
    public void AVetoEndsTheMarkOfAnEarlierAllowAndTheAdvisorOfTheHostReadsAuthorizesDiscardsAndRetiresNothing()
    {
        var env = new Env(wire: false);
        var draft = env.Seed("R1");
        Assert.Equal(CloseDecision.Allow, env.Advisor.AdviseClose(env.Session));
        draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "Edited" });

        Assert.Equal(CloseDecision.Veto, env.Advisor.AdviseClose(env.Session));

        // The advice itself changed nothing: the draft is registered, the session is alive and nothing was read or noticed.
        Assert.True(draft.IsRegistered);
        Assert.False(env.Core.OpenDrafts.IsRetired(env.Session));
        Assert.Empty(env.Reader.Calls);
        Assert.Equal(0, env.Hr.Reader.ReadCalls);
        Assert.Empty(env.Core.LossNotices);

        // And the mark of the earlier Allow is gone: once the dirty draft is out of the way, an intact sequence still retires conservatively.
        env.Core.OpenDrafts.DiscardByUser(env.Session, "R1");
        env.Seed("R9", new DraftThrowingComparer { Throw = true });
        env.Advisor.SessionDestroyed(env.Session, closeSequenceIntact: true);

        Assert.Equal(new[] { "R9" }, Assert.Single(env.Core.LossNotices).UncertainRackIds.ToArray());
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-07: the retirement and the notice of loss

    [Fact]
    public void ADestroyedActiveSessionRetiresItsDraftsAndNoticesTheLossInTheQueueAndInTheLog()
    {
        var env = new Env();
        var dirty = env.Seed("R1");
        dirty.SetCurrentDraftState(dirty.CurrentDraftState with { Name = "Edited" });
        env.Seed("R2");

        var thrown = Record.Exception(() => env.Destroy());

        Assert.Null(thrown);
        Assert.True(env.Core.OpenDrafts.IsRetired(env.Session));
        Assert.Equal(0, env.Core.OpenDrafts.CountOf(env.Session));
        Assert.False(dirty.IsRegistered);
        var notice = Assert.Single(env.Core.LossNotices);
        Assert.Equal(env.Session, notice.Session);
        Assert.Equal(new[] { "R1" }, notice.LostRackIds.ToArray());
        var fault = Assert.Single(env.Hr.Faults, f => f.Context == "WorkspaceHostCore.DraftLoss");
        Assert.IsType<InvalidOperationException>(fault.Error);
        Assert.Contains("R1", fault.Error.Message);
        Assert.DoesNotContain("R2", fault.Error.Message);
        Assert.Equal(RequestDecision.RejectedSessionDestroyed, env.Core.Authorize(new WorkspaceRequest(env.Session)));
    }

    [Fact]
    public void ADestroyedInactiveSessionIsRetiredAndNoticedWhileTheDraftsOfTheActiveOneAreUntouched()
    {
        var env = new Env();
        var a = env.Session;
        var lostInA = env.Seed("R1");
        lostInA.SetPendingText("name", "typed");
        var b = env.Hr.Activate(200);
        var keptInB = env.Seed("R1", session: b);
        keptInB.SetCurrentDraftState(keptInB.CurrentDraftState with { Name = "Edited" });

        env.Destroy(100);

        Assert.True(env.Core.OpenDrafts.IsRetired(a));
        Assert.Equal(0, env.Core.OpenDrafts.CountOf(a));
        var notice = Assert.Single(env.Core.LossNotices);
        Assert.Equal(a, notice.Session);
        Assert.Equal(new[] { "R1" }, notice.LostRackIds.ToArray());
        Assert.Single(env.Hr.Faults, f => f.Context == "WorkspaceHostCore.DraftLoss");
        Assert.False(env.Core.OpenDrafts.IsRetired(b));
        Assert.True(keptInB.IsRegistered);
        Assert.Equal(1, env.Core.OpenDrafts.CountOf(b));
    }

    [Fact]
    public void ASessionWithoutLossQueuesNothingAndLogsNothingSoTheQueueDoesNotGrowWithEveryClosedDocument()
    {
        var env = new Env();
        env.Seed("R1");
        env.Seed("R2");

        env.Destroy();

        Assert.True(env.Core.OpenDrafts.IsRetired(env.Session));
        Assert.Equal(0, env.Core.OpenDrafts.CountOf(env.Session));
        Assert.Empty(env.Core.LossNotices);
        Assert.Empty(env.Hr.Faults);

        var second = new Env();
        second.Destroy();
        Assert.Empty(second.Core.LossNotices);
        Assert.True(second.Core.OpenDrafts.IsRetired(second.Session));
    }

    [Fact]
    public void TheLogOfALossListsTheLostRacksAndApartTheUncertainOnesWithTheReasonOfTheComparer()
    {
        var env = new Env();
        var broken = new ContainmentCountingComparer { Throw = true, Message = "comparer exploded" };
        env.Seed("R1", broken);
        var knownDirty = env.Seed("R2");
        knownDirty.SetCurrentDraftState(knownDirty.CurrentDraftState with { Name = "Edited" });
        env.Seed("R3");

        env.Destroy();

        var notice = Assert.Single(env.Core.LossNotices);
        Assert.Equal(new[] { "R1", "R2" }, notice.LostRackIds.ToArray());
        Assert.Equal(new[] { "R1" }, notice.UncertainRackIds.ToArray());
        var message = Assert.Single(env.Hr.Faults, f => f.Context == "WorkspaceHostCore.DraftLoss").Error.Message;
        var uncertainAt = message.IndexOf("Uncertain", StringComparison.Ordinal);
        Assert.True(uncertainAt > 0, message);
        var lostPart = message.Substring(0, uncertainAt);
        var uncertainPart = message.Substring(uncertainAt);
        Assert.Contains("R1", lostPart);
        Assert.Contains("R2", lostPart);
        Assert.DoesNotContain("R3", message);
        Assert.Contains("R1", uncertainPart);
        Assert.DoesNotContain("R2", uncertainPart);
        Assert.Contains("comparer exploded", uncertainPart);
    }

    [Fact]
    public void ADestructionWithoutItsCloseWithADirtyDraftLosesItAndNoticesIt()
    {
        // Obligation 14: destroying the document retires the session and its drafts, and the loss is noticed.
        var env = new Env();
        var draft = env.Pilot();
        Env.MakeDirty(draft);

        env.Destroy();

        var notice = Assert.Single(env.Core.LossNotices);
        Assert.Equal(new[] { RackA }, notice.LostRackIds.ToArray());
        Assert.Empty(notice.UncertainRackIds);
        Assert.False(draft.IsRegistered);
    }

    [Fact]
    public void AFailingLogDoesNotStopTheRetirementNorTheQueueNorTheDestruction()
    {
        var hr = new WorkspaceHostRig().WithDocument(100);
        hr.Host.OpenPanel();
        var session = hr.ActiveSession;
        var core = new WorkspaceHostCore(hr.Link, hr.Probe, hr.Reader, new WorkspaceInstrumentation(), (_, _) => throw new InvalidOperationException("the log is broken"));
        hr.Bridge.Core.SetLifecycleAdvisor(core);
        var begun = core.OpenDrafts.Begin(session, "R1", DraftKit.Read(), new DraftThrowingComparer { Throw = true }, DraftKit.CopyForm);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);

        var thrown = Record.Exception(() => hr.Bridge.Core.OnDocumentToBeDestroyed(100));

        Assert.Null(thrown);
        Assert.True(core.OpenDrafts.IsRetired(session));
        Assert.Equal(0, core.OpenDrafts.CountOf(session));
        var notice = Assert.Single(core.LossNotices);
        Assert.Equal(new[] { "R1" }, notice.LostRackIds.ToArray());
        Assert.False(hr.Bridge.Core.TryGetSession(session, out _));
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-05: the stale hint of the drain

    [Fact]
    public void ADrainWithAChangeOfTheDrawingMarksEveryDraftOfThePilotOfTheSessionAsPossiblyStaleWithoutAReadOrAConflict()
    {
        var env = new Env();
        var one = env.Pilot(RackA);
        var two = env.Pilot(RackB);
        var readsBefore = env.Reader.Calls.Count;

        env.DrainWithStaleHint(BridgeSource.ObjectModified);

        Assert.True(one.IsPossiblyStale);
        Assert.True(two.IsPossiblyStale);
        Assert.Equal(DraftStatus.Clean, one.Status);
        Assert.Equal(DraftStatus.Clean, two.Status);
        Assert.Empty(one.ConflictDifferences);
        Assert.Equal(readsBefore, env.Reader.Calls.Count);
        Assert.Equal(0, env.Hr.Reader.ReadCalls);
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void TheEndOfACommandAlsoMarksTheDraftsBecauseItProducesTheSameHint()
    {
        var env = new Env();
        var draft = env.Pilot();

        env.DrainWithStaleHint(BridgeSource.CommandEnded);

        Assert.True(draft.IsPossiblyStale);
    }

    [Fact]
    public void ADrainThatOnlyCarriesTheSelectionMarksNothing()
    {
        var env = new Env();
        var draft = env.Pilot();

        env.Bridge.OnSourceEvent(env.Session, BridgeSource.ImpliedSelectionChanged);
        env.Hr.Idle();

        Assert.False(draft.IsPossiblyStale);
        Assert.False(draft.NeedsLazyReload);
        Assert.True(env.Hr.Reader.ReadCalls > 0);
    }

    [Fact]
    public void TheMarkOfADrainTouchesTheDraftsOfThatSessionAndNoneOfAnother()
    {
        var env = new Env();
        var inA = env.Pilot(RackA);
        var b = env.Hr.Activate(200);
        var inB = env.Pilot(RackA, b);
        var formInB = env.Seed("R1", session: b);
        env.Hr.Activate(100);

        env.DrainWithStaleHint();

        Assert.True(inA.IsPossiblyStale);
        Assert.False(inB.IsPossiblyStale);
        Assert.False(formInB.IsPossiblyStale);
    }

    [Fact]
    public void TheMarkIsAppliedBeforeTheEarlyReturnsOfTheDrainSoADrainOfAnInactiveSessionOrOfAHiddenPanelMarksToo()
    {
        var env = new Env();
        var inA = env.Pilot(RackA);
        var b = env.Hr.Activate(200);
        var inB = env.Pilot(RackA, b);
        env.Hr.Activate(100);

        // B is not the active session: the policy refuses the drain, yet the hint marks the drafts of B (and only those of B).
        env.Core.OnDrained(Outcome(b, DrainOperation.MarkDraftsPossiblyStale));
        Assert.True(inB.IsPossiblyStale);
        Assert.False(inA.IsPossiblyStale);

        // A hidden panel refuses the drain too.
        env.Core.HidePanel();
        env.Core.OnDrained(Outcome(env.Session, DrainOperation.MarkDraftsPossiblyStale));
        Assert.True(inA.IsPossiblyStale);
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void AnEchoAndAChangeOfTheDrawingInTheSameDrainStillMarkTheDrafts()
    {
        var hr = new WorkspaceHostRig().WithDocument(100);
        var echo = new NavigationEcho();
        var core = new WorkspaceHostCore(hr.Link, hr.Probe, hr.Reader, new WorkspaceInstrumentation(), (_, _) => { }, echo);
        core.OpenPanel();
        var session = hr.ActiveSession;
        core.SetEditIntentReader(new EditIntentFakeReader());
        Assert.Equal(EditIntentStatus.Admitted, core.TryBeginPilotEdit(new EditIntentRequest(session, RackA, null, null)).Status);
        var draft = core.OpenDrafts.Find<PushBackPilotDraftState>(session, RackA)!;
        echo.Expect(session, new long[] { 0x101 });
        echo.Observe(session, new long[] { 0x101 });

        core.OnDrained(Outcome(
            session,
            DrainOperation.ReadImplicitSelection,
            DrainOperation.RecomputeSelectionContext,
            DrainOperation.InvalidateNavigationCaches,
            DrainOperation.MarkDraftsPossiblyStale));

        Assert.True(draft.IsPossiblyStale);
        Assert.True(hr.Reader.ReadCalls > 0);
    }

    [Fact]
    public void ADraftOfAnotherStateTypeIsNotMarkedAndNothingFailsBecauseOfIt()
    {
        var env = new Env();
        var form = env.Seed("R1");
        var pilot = env.Pilot(RackA);

        env.DrainWithStaleHint();

        Assert.True(pilot.IsPossiblyStale);
        Assert.False(form.IsPossiblyStale);
        Assert.Empty(env.Hr.Faults);
        Assert.Equal(WorkspaceHostStatus.Ready, env.Core.GetSnapshot().Status);
    }

    [Fact]
    public void ADirtyDraftIsMarkedAsPossiblyStaleButStaysDirtyAndNeverReloadsLazily()
    {
        var env = new Env();
        var draft = env.Pilot();
        Env.MakeDirty(draft);

        env.DrainWithStaleHint();

        Assert.True(draft.IsPossiblyStale);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.False(draft.NeedsLazyReload);
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-06: the lazy reload

    [Fact]
    public void TheReloadOfACleanDraftMarkedAsPossiblyStaleDoesOneReadAndReplacesTheDraftFromIt()
    {
        var env = new Env();
        var draft = env.StalePilot();
        var reads = env.Reader.Calls.Count;
        var before = draft.StaleBase;

        var outcome = env.Core.TryReloadPilotEdit(env.Request());

        Assert.Equal(EditIntentStatus.Reloaded, outcome.Status);
        Assert.Equal(EditWitnessStatus.NotSupplied, outcome.Witness);
        Assert.Equal(reads + 1, env.Reader.Calls.Count);
        Assert.False(draft.IsPossiblyStale);
        Assert.False(draft.NeedsLazyReload);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.NotSame(before, draft.StaleBase);
        Assert.Same(draft, env.Core.OpenDrafts.Find<PushBackPilotDraftState>(env.Session, RackA));
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void TheReloadReadsOnlyWhenTheDraftNeedsIt()
    {
        var env = new Env();
        var clean = env.Pilot(RackA);
        var dirty = env.Pilot(RackB);
        Env.MakeDirty(dirty);
        env.DrainWithStaleHint();
        Assert.True(clean.NeedsLazyReload);
        Assert.True(dirty.IsPossiblyStale);
        var reads = env.Reader.Calls.Count;

        var needsIt = env.Core.TryReloadPilotEdit(env.Request(RackA));
        var dirtyOne = env.Core.TryReloadPilotEdit(env.Request(RackB));
        var again = env.Core.TryReloadPilotEdit(env.Request(RackA));

        Assert.Equal(EditIntentStatus.Reloaded, needsIt.Status);
        Assert.Equal(EditIntentStatus.NotNeeded, dirtyOne.Status);
        Assert.Equal(EditIntentStatus.NotNeeded, again.Status);
        Assert.False(string.IsNullOrEmpty(dirtyOne.Reason));
        Assert.Equal(reads + 1, env.Reader.Calls.Count);
        Assert.True(dirty.IsPossiblyStale);
        Assert.False(clean.IsPossiblyStale);
    }

    [Fact]
    public void ADraftThatWasNeverMarkedIsNotReloadedAndNothingIsRead()
    {
        var env = new Env();
        var draft = env.Pilot();
        var reads = env.Reader.Calls.Count;

        var outcome = env.Core.TryReloadPilotEdit(env.Request());

        Assert.Equal(EditIntentStatus.NotNeeded, outcome.Status);
        Assert.Equal(reads, env.Reader.Calls.Count);
        Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
        Assert.False(draft.IsPossiblyStale);
    }

    [Fact]
    public void TheReloadOfARackWithoutDraftIsNotFoundAndReadsNothing()
    {
        var env = new Env();

        var outcome = env.Core.TryReloadPilotEdit(env.Request(RackB));

        Assert.Equal(EditIntentStatus.NotFound, outcome.Status);
        Assert.False(string.IsNullOrEmpty(outcome.Reason));
        Assert.Empty(env.Reader.Calls);
    }

    [Fact]
    public void AReloadThatIsNotAdmittedKeepsTheDraftMarkedAndCarriesTheDiagnosticTheDetailAndTheClassicEditorHint()
    {
        var env = new Env();
        var draft = env.StalePilot();
        env.Reader.Answer = EditIntentKit.OriginMissing;

        var outcome = env.Core.TryReloadPilotEdit(env.Request());

        Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, outcome.Diagnostic);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
        Assert.True(draft.IsPossiblyStale);
        Assert.True(draft.NeedsLazyReload);
    }

    [Fact]
    public void ADraftThatLeavesTheRegistryWhileTheReadRunsMakesTheReloadWaitInsteadOfFailing()
    {
        var env = new Env();
        env.StalePilot();
        env.Reader.Answer = rack =>
        {
            env.Core.OpenDrafts.DiscardByUser(env.Session, rack);
            return EditIntentKit.Admitted(rack);
        };

        var outcome = env.Core.TryReloadPilotEdit(env.Request());

        Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
        Assert.False(env.Core.OpenDrafts.Contains(env.Session, RackA));
    }

    [Fact]
    public void TheReloadGoesThroughTheSameGateAsTheIntention()
    {
        var env = new Env();
        env.StalePilot();
        var reads = env.Reader.Calls.Count;

        env.Core.SetModalRackCadActive(true);
        Assert.Equal(EditIntentStatus.Waiting, env.Core.TryReloadPilotEdit(env.Request()).Status);
        env.Core.SetModalRackCadActive(false);

        env.Hr.CommandStarts(env.Session);
        Assert.Equal(EditIntentStatus.Waiting, env.Core.TryReloadPilotEdit(env.Request()).Status);
        env.Hr.CommandEnds(env.Session);

        var b = env.Hr.Activate(200);
        Assert.Equal(EditIntentStatus.Rejected, env.Core.TryReloadPilotEdit(env.Request(session: env.Session)).Status);
        Assert.Equal(b, env.Hr.ActiveSession);

        Assert.Equal(reads, env.Reader.Calls.Count);
        Assert.Equal(EditIntentStatus.Unavailable, new Env(reader: false).Core.TryReloadPilotEdit(env.Request()).Status);
    }

    [Fact]
    public void TheReloadOfABlankRackAndANullRequestAreContainedWithoutReadingAndWithoutDegradingTheSession()
    {
        var env = new Env();

        EditIntentOutcome blank = default;
        EditIntentOutcome none = default;
        var thrown = Record.Exception(() =>
        {
            blank = env.Core.TryReloadPilotEdit(env.Request(" "));
            none = env.Core.TryReloadPilotEdit(null!);
        });

        Assert.Null(thrown);
        Assert.Equal(EditIntentStatus.NotAdmitted, blank.Status);
        Assert.Equal(PushBackPilotIntentReader.NoIdentityDiagnostic, blank.Diagnostic);
        Assert.Equal(EditIntentStatus.Failed, none.Status);
        Assert.Empty(env.Reader.Calls);
        Assert.Equal(WorkspaceHostStatus.Ready, env.Core.GetSnapshot().Status);
    }

    [Fact]
    public void AnExceptionOfTheReaderDuringTheReloadIsContainedAsFailedLoggedAndDoesNotDegradeTheSession()
    {
        var env = new Env();
        var draft = env.StalePilot();
        env.Reader.Throw = new InvalidOperationException("reader boom");

        var outcome = env.Core.TryReloadPilotEdit(env.Request());

        Assert.Equal(EditIntentStatus.Failed, outcome.Status);
        Assert.Equal("reader boom", outcome.Reason);
        var fault = Assert.Single(env.Hr.Faults);
        Assert.Equal("WorkspaceHostCore.TryReloadPilotEdit", fault.Context);
        Assert.True(draft.NeedsLazyReload);
        Assert.Equal(WorkspaceHostStatus.Ready, env.Core.GetSnapshot().Status);
        env.Hr.SelectionChanged(env.Session);
        env.Hr.Idle();
        Assert.True(env.Hr.Reader.ReadCalls > 0);
    }

    [Fact]
    public void NoDrainEverReloadsADraftAndNoDrainReadsForTheLifecycle()
    {
        var env = new Env();
        var draft = env.StalePilot();
        var reads = env.Reader.Calls.Count;

        for (var i = 0; i < 3; i++)
        {
            env.DrainWithStaleHint(BridgeSource.CommandEnded);
            env.Hr.SelectionChanged(env.Session);
            env.Hr.Idle();
        }

        Assert.Equal(reads, env.Reader.Calls.Count);
        Assert.True(draft.IsPossiblyStale);
        Assert.True(draft.NeedsLazyReload);
        Assert.Empty(env.Core.LossNotices);
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-08: the explicit discard of the host

    [Fact]
    public void DiscardPilotEditsRemovesEveryDraftOfTheSessionAndSaysWhichOnesHadPendingText()
    {
        var env = new Env();
        var pilot = env.Pilot(RackB);
        var form = env.Seed("R1");
        form.SetPendingText("name", "typed");
        var b = env.Hr.Activate(200);
        var other = env.Seed("R1", session: b);
        env.Hr.Activate(100);
        var reads = env.Reader.Calls.Count;

        var outcome = env.Core.DiscardPilotEdits(env.Session);

        Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
        Assert.Equal(new[] { "R1", RackB }.OrderBy(r => r, StringComparer.Ordinal).ToArray(), outcome.RemovedRackIds.ToArray());
        Assert.Equal(new[] { "R1" }, outcome.PendingTextRackIds.ToArray());
        Assert.Null(outcome.Reason);
        Assert.False(pilot.IsRegistered);
        Assert.False(form.IsRegistered);
        Assert.True(other.IsRegistered);
        Assert.Equal(0, env.Core.OpenDrafts.CountOf(env.Session));
        Assert.False(env.Core.OpenDrafts.IsRetired(env.Session));
        Assert.Equal(reads, env.Reader.Calls.Count);
        Assert.Equal(0, env.Hr.Reader.ReadCalls);
    }

    [Fact]
    public void DiscardPilotEditsWorksWithTheProbeNotAtRestWithARackCadModalOpenAndWithoutAReader()
    {
        var env = new Env(reader: false);
        var draft = env.Seed("R1");
        env.Hr.CommandStarts(env.Session);
        env.Core.SetModalRackCadActive(true);
        Assert.False(env.Hr.Probe.IsActiveDocumentQuiescent);

        var outcome = env.Core.DiscardPilotEdits(env.Session);

        Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
        Assert.Equal(new[] { "R1" }, outcome.RemovedRackIds.ToArray());
        Assert.False(draft.IsRegistered);
    }

    [Fact]
    public void DiscardPilotEditsWithoutDraftsIsDiscardedWithEmptyListsThatAreNeverNull()
    {
        var env = new Env();

        var outcome = env.Core.DiscardPilotEdits(env.Session);

        Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
        Assert.NotNull(outcome.RemovedRackIds);
        Assert.NotNull(outcome.PendingTextRackIds);
        Assert.Empty(outcome.RemovedRackIds);
        Assert.Empty(outcome.PendingTextRackIds);
    }

    [Fact]
    public void DiscardPilotEditsIsRejectedWithoutEffectsWhenAnotherSessionIsActiveWhenThereIsNoActiveDocumentOrTheSessionIsDestroyed()
    {
        var env = new Env();
        var draft = env.Seed("R1");
        var a = env.Session;

        var b = env.Hr.Activate(200);
        var other = env.Core.DiscardPilotEdits(a);
        env.Hr.Probe.ActiveDocumentNativeId = null;
        var none = env.Core.DiscardPilotEdits(a);
        env.Hr.Probe.ActiveDocumentNativeId = 200;
        env.Destroy(200);
        var gone = env.Core.DiscardPilotEdits(b);

        Assert.Equal(EditDiscardStatus.Rejected, other.Status);
        Assert.Equal(EditDiscardStatus.Rejected, none.Status);
        Assert.Equal(EditDiscardStatus.Rejected, gone.Status);
        Assert.False(string.IsNullOrEmpty(other.Reason));
        Assert.Empty(other.RemovedRackIds);
        Assert.Empty(none.PendingTextRackIds);
        Assert.True(draft.IsRegistered);
        Assert.Equal(1, env.Core.OpenDrafts.CountOf(a));
    }

    [Fact]
    public void DiscardPilotEditsWithAComparerThatThrowsIsDiscardedAndTheNextCloseProceeds()
    {
        var env = new Env();
        var comparer = new DraftThrowingComparer();
        env.Seed("R1", comparer);
        comparer.Throw = true;
        Assert.Equal(CloseDecision.Veto, env.Close());

        EditDiscardOutcome outcome = default;
        var thrown = Record.Exception(() => outcome = env.Core.DiscardPilotEdits(env.Session));

        Assert.Null(thrown);
        Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
        Assert.Equal(CloseDecision.Allow, env.Close());
    }

    private sealed class ThrowingProbe : IWorkspaceHostProbe
    {
        public long? ActiveDocumentNativeId => throw new InvalidOperationException("the probe failed");

        public bool IsActiveDocumentQuiescent => true;
    }

    [Fact]
    public void AnExceptionOfTheDiscardIsContainedAsFailedAndDoesNotDegradeTheSession()
    {
        var hr = new WorkspaceHostRig().WithDocument(100);
        hr.Host.OpenPanel();
        var session = hr.ActiveSession;
        var faults = new List<(string Context, Exception Error)>();
        var core = new WorkspaceHostCore(hr.Link, new ThrowingProbe(), hr.Reader, new WorkspaceInstrumentation(), (c, e) => faults.Add((c, e)));

        EditDiscardOutcome outcome = default;
        var thrown = Record.Exception(() => outcome = core.DiscardPilotEdits(session));

        Assert.Null(thrown);
        Assert.Equal(EditDiscardStatus.Failed, outcome.Status);
        Assert.Equal("the probe failed", outcome.Reason);
        Assert.Empty(outcome.RemovedRackIds);
        Assert.Empty(outcome.PendingTextRackIds);
        var fault = Assert.Single(faults);
        Assert.Equal("WorkspaceHostCore.DiscardPilotEdits", fault.Context);
    }

    [Fact]
    public void TheDefaultValueOfTheDiscardOutcomeIsTheInertRejectedWithListsThatAreNeverNull()
    {
        var unset = default(EditDiscardOutcome);

        Assert.Equal(EditDiscardStatus.Rejected, unset.Status);
        Assert.NotNull(unset.RemovedRackIds);
        Assert.NotNull(unset.PendingTextRackIds);
        Assert.Empty(unset.RemovedRackIds);
        Assert.Empty(unset.PendingTextRackIds);
        Assert.True(typeof(EditDiscardOutcome).IsValueType);
        Assert.Equal(new[] { "Rejected", "Discarded", "Failed" }, Enum.GetNames<EditDiscardStatus>().Take(3).ToArray());
        Assert.Equal(EditDiscardStatus.Rejected, default(EditDiscardStatus));
    }

    [Fact]
    public void TheDiscardOutcomeNamesNeitherADraftNorATypeOfThePilot()
    {
        var types = typeof(EditDiscardOutcome).GetProperties().Select(p => p.PropertyType)
            .Concat(typeof(EditDiscardOutcome).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
                .SelectMany(m => new[] { m.ReturnType }.Concat(m.GetParameters().Select(p => p.ParameterType))))
            .SelectMany(t => t.IsGenericType ? t.GetGenericArguments().Append(t) : new[] { t })
            .ToArray();

        Assert.NotEmpty(types);
        Assert.All(types, t =>
        {
            var ns = t.Namespace ?? string.Empty;
            Assert.False(ns.StartsWith("RackCad.Application.Workspace.Drafts", StringComparison.Ordinal), t.FullName);
            Assert.False(ns.StartsWith("RackCad.Application.Systems", StringComparison.Ordinal), t.FullName);
        });
    }

    [Fact]
    public void TheExtendedStatesOfTheIntentionAreAnOrderedPrefixAndTheNewOutcomesCarryTheirWitness()
    {
        Assert.Equal(
            new[] { "Unavailable", "Waiting", "Rejected", "AlreadyOpen", "NotAdmitted", "Admitted", "Failed", "NotFound", "NotNeeded", "Reloaded" },
            Enum.GetNames<EditIntentStatus>().Take(10).ToArray());
        Assert.Equal(EditIntentStatus.Unavailable, default(EditIntentStatus));
        Assert.Equal(EditIntentStatus.Unavailable, default(EditIntentOutcome).Status);
        Assert.Equal(EditIntentStatus.Reloaded, EditIntentOutcome.Reloaded(EditWitnessStatus.Unchanged).Status);
        Assert.Equal(EditWitnessStatus.Unchanged, EditIntentOutcome.Reloaded(EditWitnessStatus.Unchanged).Witness);
        Assert.Equal(EditIntentStatus.NotFound, EditIntentOutcome.NotFound("x").Status);
        Assert.Equal(EditIntentStatus.NotNeeded, EditIntentOutcome.NotNeeded("x").Status);
        Assert.Equal(EditWitnessStatus.NotEvaluated, EditIntentOutcome.NotNeeded("x").Witness);
    }
}
