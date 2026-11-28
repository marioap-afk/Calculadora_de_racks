#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T2C (IC-04): no read of the drawing inside a callback of AutoCAD. The idle callback only schedules; the continuation
// (the idle pass, the selection read of the drain and then the summary read of the presentation) runs after the callback returned.
// The harness below composes the pure pieces the way the Plugin does and measures the STACK, not only the count: every simulated
// callback of AutoCAD runs inside a depth scope, the doubles of the readers record that depth at every read, and every test
// asserts that no read happened at a depth greater than 0. The post of the harness only queues: the real post of the Plugin
// (BeginInvoke at background priority) is accredited by the source guards of WorkspaceIdleSchedulingGuardTests, and the stack
// timing in the real host by Smoke-2.

/// <summary>
/// The harness of F4-T2C. It composes the real pure core of the bridge, the real scheduler (check: <c>core.CanDrainNow</c>) and the real
/// host core with a selection reader of its own, plus a DECLARED substitute of the presentation of the panel that imitates the order of
/// <c>WorkspaceNavigationHost.ReadSummaryOnActiveDocument</c> over the real <see cref="BrowserRackSummaries"/>.
/// </summary>
internal sealed class WorkspaceIdleHarness
{
    public const long DocumentA = 100;
    public const long DocumentB = 200;

    private readonly IdleDepthReader _reader;

    public WorkspaceIdleHarness(bool postImmediately = false)
    {
        _reader = new IdleDepthReader(this);
        Bridge = new WorkspaceBridgeRig();
        Link = new WorkspaceSimulatedBridgeLink(Bridge, Probe);
        Host = new WorkspaceHostCore(Link, Probe, _reader, new WorkspaceInstrumentation(), (context, error) => HostFaults.Add((context, error)));
        Summaries = new BrowserRackSummaries(ReadFacts, id => Core.TryGetSession(id, out _));
        Scheduler = new WorkspaceIdleScheduler(Core.CanDrainNow, Continuation, (context, error) => SchedulerFaults.Add((context, error)));
        if (postImmediately) Scheduler.SetPost(action => action());
        else Scheduler.SetPost(action => Posts.Enqueue(action));
    }

    public WorkspaceBridgeRig Bridge { get; }

    public EventBridgeCore Core => Bridge.Core;

    public WorkspaceSimulatedProbe Probe { get; } = new();

    public WorkspaceSimulatedBridgeLink Link { get; }

    public WorkspaceHostCore Host { get; }

    public BrowserRackSummaries Summaries { get; }

    public WorkspaceIdleScheduler Scheduler { get; }

    public IdleDepthReader Reader => _reader;

    public Queue<Action> Posts { get; } = new();

    public List<(string Context, Exception Error)> HostFaults { get; } = new();

    public List<(string Context, Exception Error)> SchedulerFaults { get; } = new();

    public List<DrainOutcome> Drains { get; } = new();

    public List<BrowserRackSummary> SummaryResults { get; } = new();

    /// <summary>The simulated depth of AutoCAD callbacks: 0 outside every callback.</summary>
    public int Depth { get; private set; }

    /// <summary>How many times the continuation called the idle pass of the core.</summary>
    public int PassCalls { get; private set; }

    /// <summary>The depth of every read of the facts of the summary (the second kind of read of the drawing).</summary>
    public List<int> FactsDepths { get; } = new();

    public Exception? FactsThrow { get; set; }

    /// <summary>The rack the substitute presentation asks the summary of.</summary>
    public string PresentedRack { get; set; } = "R-1";

    /// <summary>K = 1 (F3-T2): the surface of the rack is materialized once; only a new, pending or busy surface reads.</summary>
    public bool SurfaceNeedsRead { get; set; } = true;

    /// <summary>The window of the notice of the drain, open only around the notice, as the controller does.</summary>
    public bool NoticeOpen { get; private set; }

    public int TotalReads => Reader.Depths.Count + FactsDepths.Count;

    public IReadOnlyList<int> AllReadDepths => Reader.Depths.Concat(FactsDepths).ToList();

    // ------------------------------------------------------------------ the host side

    /// <summary>The first RACKPANEL over an open and active document: the session exists and one reconciliation hint is pending.</summary>
    public SessionId OpenPanel(long native = DocumentA)
    {
        Probe.ActiveDocumentNativeId = native;
        Probe.IsActiveDocumentQuiescent = true;
        Host.OpenPanel();
        return Bridge.IdOf(native);
    }

    public WorkspaceSession Session(SessionId id) => Bridge.SessionOf(id);

    // ------------------------------------------------------------------ the callbacks of AutoCAD (each one inside a depth scope)

    public void Callback(Action body)
    {
        Depth++;
        try
        {
            body();
        }
        finally
        {
            Depth--;
        }
    }

    public void Tick() => Callback(Scheduler.OnIdleTick);

    public void Ticks(int count)
    {
        for (var i = 0; i < count; i++) Tick();
    }

    public void SelectionChanged(SessionId id) => Callback(() => Core.OnSourceEvent(id, BridgeSource.ImpliedSelectionChanged));

    public void ObjectChanged(SessionId id) => Callback(() => Core.OnSourceEvent(id, BridgeSource.ObjectModified));

    public void CommandStarts(SessionId id) => Callback(() =>
    {
        Probe.IsActiveDocumentQuiescent = false;
        Core.OnSourceEvent(id, BridgeSource.CommandWillStart);
    });

    public void CommandEnds(SessionId id) => Callback(() =>
    {
        Core.OnSourceEvent(id, BridgeSource.CommandEnded);
        Core.OnSourceEvent(id, BridgeSource.EnteringQuiescentState);
        Probe.IsActiveDocumentQuiescent = true;
    });

    public void Activate(long native) => Callback(() =>
    {
        Probe.ActiveDocumentNativeId = native;
        Probe.IsActiveDocumentQuiescent = true;
        Core.OnDocumentActivated(native, true);
    });

    public void NoActiveDocument() => Callback(() =>
    {
        Probe.ActiveDocumentNativeId = null;
        Core.OnNoActiveDocument();
    });

    public void Destroy(long native) => Callback(() =>
    {
        Core.OnDocumentToBeDestroyed(native);
        if (Probe.ActiveDocumentNativeId == native) Probe.ActiveDocumentNativeId = null;
    });

    public void CloseAllowed(SessionId id) => Callback(() => Core.OnBeginDocumentClose(id));

    // ------------------------------------------------------------------ outside every callback

    /// <summary>The dispatcher pumps what was posted: it runs outside every callback of AutoCAD.</summary>
    public void RunQueue()
    {
        Assert.Equal(0, Depth);
        var count = Posts.Count;
        for (var i = 0; i < count; i++) Posts.Dequeue()();
    }

    // The continuation of the Plugin: the idle pass, and for every drain what the controller does with it.
    private void Continuation()
    {
        PassCalls++;
        foreach (var outcome in Core.OnIdle())
        {
            Drains.Add(outcome);
            try
            {
                Host.OnDrained(outcome);
                Summaries.OnDrained(outcome);
            }
            catch (Exception ex)
            {
                HostFaults.Add(("controller", ex));
            }

            NoticeOpen = true;
            try
            {
                Present();
            }
            finally
            {
                NoticeOpen = false;
            }
        }
    }

    // The substitute presentation: one surface per presentation (K = 1). It asks the summary only while the surface needs it.
    private void Present()
    {
        if (!SurfaceNeedsRead) return;
        if (Core.ActiveSession is not { } id) return;

        var summary = ReadSummary(new WorkspaceRequest(id), PresentedRack, userRequested: false);
        SummaryResults.Add(summary);
        SurfaceNeedsRead = summary.Status == BrowserSummaryStatus.Busy;
    }

    // The order of ReadSummaryOnActiveDocument: the active document, at rest, a read nobody asked for only inside the notice of a
    // drain, the session authorized for that document, and only then the memory or the read; an exception is contained as Failed.
    private BrowserRackSummary ReadSummary(WorkspaceRequest origin, string rackId, bool userRequested)
    {
        try
        {
            if (Probe.ActiveDocumentNativeId is not { } native)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "There is no active document.");
            if (!Probe.IsActiveDocumentQuiescent)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Busy, "A command is in progress.");
            if (!userRequested && !NoticeOpen)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Busy, "Not inside the notice of a drain.");
            if (Core.Authorize(origin, native) != RequestDecision.Accepted)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "The request does not belong to the active document.");

            return Summaries.Read(origin.Origin, rackId);
        }
        catch (Exception ex)
        {
            return BrowserRackSummary.Without(BrowserSummaryStatus.Failed, ex.Message);
        }
    }

    private IReadOnlyList<BrowserRackFact> ReadFacts(SessionId session)
    {
        FactsDepths.Add(Depth);
        if (FactsThrow != null) throw FactsThrow;
        return new[] { new BrowserRackFact(true, "R-1", "selectivo", "frontal", "Rack uno") };
    }
}

/// <summary>The selection reader of F4-T2C: it records the simulated depth of every read.</summary>
internal sealed class IdleDepthReader : IWorkspaceSelectionReader
{
    private readonly WorkspaceIdleHarness _harness;

    public IdleDepthReader(WorkspaceIdleHarness harness)
    {
        _harness = harness;
    }

    public List<int> Depths { get; } = new();

    public List<(SessionId Session, long NativeId)> Reads { get; } = new();

    public Exception? Throw { get; set; }

    public WorkspaceSelectionRead Result { get; set; } = new(new[] { WorkspaceHostRig.Rack("R-1") }, 0);

    public WorkspaceSelectionRead Read(SessionId session, long nativeInstanceId)
    {
        Depths.Add(_harness.Depth);
        Reads.Add((session, nativeInstanceId));
        if (Throw != null) throw Throw;
        return Result;
    }

    public void Invalidate(SessionId session)
    {
        // Forgets what was learned; it reads nothing.
    }
}

public class WorkspaceIdleSchedulerTests
{
    private static void AssertNoReadInsideACallback(WorkspaceIdleHarness h) =>
        Assert.All(h.AllReadDepths, depth => Assert.Equal(0, depth));

    // ================================================================== the scheduler, by itself

    private sealed class SchedulerRig
    {
        public bool Can = true;
        public int CanCalls;
        public int Runs;
        public Exception? CanThrow;
        public Exception? ContinuationThrow;
        public Exception? PostThrow;
        public Exception? LogThrow;
        public Queue<Action> Posts { get; } = new();
        public List<(string Context, Exception Error)> Faults { get; } = new();
        public WorkspaceIdleScheduler Scheduler { get; }

        public SchedulerRig(bool withPost = true)
        {
            Scheduler = new WorkspaceIdleScheduler(
                () =>
                {
                    CanCalls++;
                    if (CanThrow != null) throw CanThrow;
                    return Can;
                },
                () =>
                {
                    Runs++;
                    if (ContinuationThrow != null) throw ContinuationThrow;
                },
                (context, error) =>
                {
                    Faults.Add((context, error));
                    if (LogThrow != null) throw LogThrow;
                });
            if (withPost)
                Scheduler.SetPost(action =>
                {
                    if (PostThrow != null) throw PostThrow;
                    Posts.Enqueue(action);
                });
        }

        public void RunQueue()
        {
            var count = Posts.Count;
            for (var i = 0; i < count; i++) Posts.Dequeue()();
        }
    }

    [Fact]
    public void TheSchedulerRejectsNullArgumentsExceptTheLogger()
    {
        var noCheck = Record.Exception(() => new WorkspaceIdleScheduler(null!, () => { }));
        var noContinuation = Record.Exception(() => new WorkspaceIdleScheduler(() => true, null!));
        var noLogger = Record.Exception(() => new WorkspaceIdleScheduler(() => true, () => { }, null));
        var rig = new SchedulerRig(withPost: false);
        var noPost = Record.Exception(() => rig.Scheduler.SetPost(null!));

        Assert.IsType<ArgumentNullException>(noCheck);
        Assert.IsType<ArgumentNullException>(noContinuation);
        Assert.Null(noLogger);
        Assert.IsType<ArgumentNullException>(noPost);
    }

    [Fact]
    public void ManyTicksScheduleOneContinuationAndTheSecondTickDoesNotEvenAskTheCheck()
    {
        var rig = new SchedulerRig();

        for (var i = 0; i < 6; i++) rig.Scheduler.OnIdleTick();

        Assert.Single(rig.Posts);
        Assert.True(rig.Scheduler.IsScheduled);
        Assert.Equal(1, rig.CanCalls);
        Assert.Equal(0, rig.Runs);
    }

    [Fact]
    public void ATickNeverRunsTheContinuationNorTheRunByItself()
    {
        var rig = new SchedulerRig();

        rig.Scheduler.OnIdleTick();

        Assert.Equal(0, rig.Runs);
        Assert.Equal(0, rig.Scheduler.ContinuationsRun);
        Assert.Equal(0, rig.Scheduler.ContinuationsSkipped);
        Assert.Single(rig.Posts);
        Assert.True(rig.Scheduler.IsScheduled);
    }

    [Fact]
    public void WithoutAPostATickDoesNothingAndDoesNotAskTheCheck()
    {
        var rig = new SchedulerRig(withPost: false);

        var error = Record.Exception(() => rig.Scheduler.OnIdleTick());

        Assert.Null(error);
        Assert.False(rig.Scheduler.IsScheduled);
        Assert.Equal(0, rig.CanCalls);
        Assert.Equal(0, rig.Runs);
        Assert.Empty(rig.Faults);

        // The moment a post exists the very next tick schedules: nothing was left half done.
        var posted = 0;
        rig.Scheduler.SetPost(_ => posted++);
        rig.Scheduler.OnIdleTick();
        Assert.Equal(1, posted);
        Assert.True(rig.Scheduler.IsScheduled);
    }

    [Fact]
    public void WhileTheCheckIsFalseATickPostsNothingNoMatterHowManyTimes()
    {
        var rig = new SchedulerRig { Can = false };

        for (var i = 0; i < 10; i++) rig.Scheduler.OnIdleTick();

        Assert.Empty(rig.Posts);
        Assert.False(rig.Scheduler.IsScheduled);
        Assert.Equal(10, rig.CanCalls);

        rig.Can = true;
        rig.Scheduler.OnIdleTick();
        Assert.Single(rig.Posts);
    }

    [Fact]
    public void TheRunUnmarksAsksTheCheckAgainAndRunsTheContinuationOnlyIfItStillHolds()
    {
        var rig = new SchedulerRig();
        rig.Scheduler.OnIdleTick();
        Assert.True(rig.Scheduler.IsScheduled);

        rig.Can = false;
        rig.RunQueue();

        Assert.False(rig.Scheduler.IsScheduled);
        Assert.Equal(0, rig.Runs);
        Assert.Equal(2, rig.CanCalls);
        Assert.Equal(1, rig.Scheduler.ContinuationsSkipped);
        Assert.Equal(0, rig.Scheduler.ContinuationsRun);
        Assert.Empty(rig.Posts);

        // Un-marked: the next eligible tick schedules again, and this time the run executes.
        rig.Can = true;
        rig.Scheduler.OnIdleTick();
        rig.RunQueue();
        Assert.Equal(1, rig.Runs);
        Assert.Equal(1, rig.Scheduler.ContinuationsRun);
        Assert.Equal(1, rig.Scheduler.ContinuationsSkipped);
        Assert.False(rig.Scheduler.IsScheduled);
    }

    [Fact]
    public void TheRunNeverSchedulesItselfAgain()
    {
        var rig = new SchedulerRig();
        rig.Scheduler.OnIdleTick();

        rig.RunQueue();

        Assert.Equal(1, rig.Runs);
        Assert.Empty(rig.Posts);
        Assert.False(rig.Scheduler.IsScheduled);
    }

    [Fact]
    public void AFailingCheckAtTheTickIsContainedAndLeavesNothingScheduled()
    {
        var rig = new SchedulerRig { CanThrow = new InvalidOperationException("check") };

        var error = Record.Exception(() => rig.Scheduler.OnIdleTick());

        Assert.Null(error);
        Assert.False(rig.Scheduler.IsScheduled);
        Assert.Empty(rig.Posts);
        Assert.Single(rig.Faults);
        Assert.Equal(0, rig.Runs);
    }

    [Fact]
    public void AFailingPostIsContainedAndLeavesNothingScheduled()
    {
        var rig = new SchedulerRig { PostThrow = new InvalidOperationException("post") };

        var error = Record.Exception(() => rig.Scheduler.OnIdleTick());

        Assert.Null(error);
        Assert.False(rig.Scheduler.IsScheduled);
        Assert.Single(rig.Faults);

        // Not stuck: once the post works again, the next tick schedules.
        rig.PostThrow = null;
        rig.Scheduler.OnIdleTick();
        Assert.Single(rig.Posts);
    }

    [Fact]
    public void AFailingContinuationIsContainedAndTheSchedulerIsFreeAgain()
    {
        var rig = new SchedulerRig { ContinuationThrow = new InvalidOperationException("continuation") };
        rig.Scheduler.OnIdleTick();

        var error = Record.Exception(() => rig.RunQueue());

        Assert.Null(error);
        Assert.Equal(1, rig.Runs);
        Assert.False(rig.Scheduler.IsScheduled);
        Assert.Empty(rig.Posts);
        Assert.Single(rig.Faults);

        rig.ContinuationThrow = null;
        rig.Scheduler.OnIdleTick();
        rig.RunQueue();
        Assert.Equal(2, rig.Runs);
    }

    [Fact]
    public void AFailingCheckInTheRunIsContainedAndTheContinuationDoesNotRun()
    {
        var rig = new SchedulerRig();
        rig.Scheduler.OnIdleTick();
        rig.CanThrow = new InvalidOperationException("check at run");

        var error = Record.Exception(() => rig.RunQueue());

        Assert.Null(error);
        Assert.Equal(0, rig.Runs);
        Assert.False(rig.Scheduler.IsScheduled);
        Assert.Equal(1, rig.Scheduler.ContinuationsSkipped);
        Assert.Single(rig.Faults);
    }

    [Fact]
    public void AFailingLoggerIsContainedToo()
    {
        var rig = new SchedulerRig { CanThrow = new InvalidOperationException("check"), LogThrow = new InvalidOperationException("log") };

        var atTick = Record.Exception(() => rig.Scheduler.OnIdleTick());
        rig.CanThrow = null;
        rig.ContinuationThrow = new InvalidOperationException("continuation");
        rig.Scheduler.OnIdleTick();
        var atRun = Record.Exception(() => rig.RunQueue());

        Assert.Null(atTick);
        Assert.Null(atRun);
        Assert.Equal(2, rig.Faults.Count);
        Assert.False(rig.Scheduler.IsScheduled);
    }

    [Fact]
    public void TheCountersAreInMemoryAndTheQueriesHaveNoEffects()
    {
        var rig = new SchedulerRig();
        rig.Scheduler.OnIdleTick();
        rig.RunQueue();
        rig.Can = false;
        rig.Scheduler.OnIdleTick();
        rig.Can = true;
        rig.Scheduler.OnIdleTick();
        rig.Can = false;
        rig.RunQueue();

        var calls = rig.CanCalls;
        _ = rig.Scheduler.IsScheduled;
        _ = rig.Scheduler.ContinuationsRun;
        _ = rig.Scheduler.ContinuationsSkipped;

        Assert.Equal(1, rig.Scheduler.ContinuationsRun);
        Assert.Equal(1, rig.Scheduler.ContinuationsSkipped);
        Assert.Equal(calls, rig.CanCalls);
        Assert.Empty(rig.Faults);
    }

    // ================================================================== the tests A to J of the order of the Owner

    [Fact]
    public void A_TheNotificationsOfTheDrawingOnlyMarkAndNothingIsReadInsideTheirCallbackOrAfterIt()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();

        h.Callback(() =>
        {
            h.Core.OnSourceEvent(id, BridgeSource.ObjectModified);
            h.Core.OnSourceEvent(id, BridgeSource.CommandEnded);
            Assert.Equal(0, h.TotalReads);
            Assert.Empty(h.Posts);
        });

        Assert.Equal(0, h.TotalReads);
        Assert.Empty(h.Posts);
        Assert.Empty(h.Drains);
        // Marked, and the idle pass could drain: the next tick would schedule. Nothing was read to learn it.
        Assert.True(h.Session(id).PendingHintCount > 0);
        Assert.True(h.Core.CanDrainNow());
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void B_AnEligibleIdleTickOnlySchedulesOneContinuationAndReadsNothing()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();

        h.Callback(() =>
        {
            h.Scheduler.OnIdleTick();
            Assert.Equal(0, h.TotalReads);
        });

        Assert.Equal(0, h.TotalReads);
        Assert.Single(h.Posts);
        Assert.True(h.Scheduler.IsScheduled);
        Assert.Equal(0, h.PassCalls);
        Assert.Empty(h.Drains);
        Assert.True(h.Session(id).PendingHintCount > 0);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void C_TheQueuedContinuationReadsTheSelectionAndThenTheSummaryOutsideEveryCallback()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        h.Tick();
        Assert.Single(h.Posts);

        h.RunQueue();

        Assert.Equal(new[] { 0 }, h.Reader.Depths);
        Assert.Equal(new[] { 0 }, h.FactsDepths);
        Assert.Equal(new[] { (id, WorkspaceIdleHarness.DocumentA) }, h.Reader.Reads);
        Assert.Single(h.Drains);
        Assert.Single(h.SummaryResults);
        Assert.Equal(BrowserSummaryStatus.Ready, h.SummaryResults[0].Status);
        Assert.Equal(1, h.PassCalls);
        Assert.Equal(1, h.Scheduler.ContinuationsRun);
        Assert.Empty(h.Posts);
        Assert.False(h.Scheduler.IsScheduled);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void D_ManyHintsAndManyTicksBeforeTheQueueRunsGiveOneContinuationOneDrainAndOneSummaryRead()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        for (var i = 0; i < 3; i++)
        {
            h.SelectionChanged(id);
            h.ObjectChanged(id);
        }

        h.Ticks(5);

        Assert.Single(h.Posts);
        Assert.Equal(0, h.TotalReads);

        h.RunQueue();

        Assert.Single(h.Drains);
        Assert.Single(h.Reader.Reads);
        Assert.Single(h.FactsDepths);
        Assert.Single(h.SummaryResults);
        Assert.Equal(1, h.PassCalls);

        // The hints were consumed by that drain: later ticks schedule nothing and read nothing.
        h.Ticks(4);
        h.RunQueue();
        Assert.Empty(h.Posts);
        Assert.Equal(2, h.TotalReads);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void E_ASessionDestroyedAfterSchedulingAndBeforeTheContinuationReadsNothingAndTheRunDoesNotCallThePass()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        h.Tick();
        Assert.Single(h.Posts);

        h.Destroy(WorkspaceIdleHarness.DocumentA);
        h.RunQueue();

        Assert.Equal(0, h.TotalReads);
        Assert.Equal(0, h.PassCalls);
        Assert.Equal(1, h.Scheduler.ContinuationsSkipped);
        Assert.Equal(0, h.Scheduler.ContinuationsRun);
        Assert.Empty(h.Drains);
        Assert.False(h.Core.TryGetSession(id, out _));
        Assert.False(h.Scheduler.IsScheduled);
    }

    [Fact]
    public void E_AnInactiveSessionOfAStartTabReadsNothingEither()
    {
        var h = new WorkspaceIdleHarness();
        h.OpenPanel();
        h.Tick();
        Assert.Single(h.Posts);

        h.NoActiveDocument();
        h.RunQueue();

        Assert.Equal(0, h.TotalReads);
        Assert.Equal(0, h.PassCalls);
        Assert.Equal(1, h.Scheduler.ContinuationsSkipped);

        // Nothing is active, so later ticks schedule nothing either.
        h.Ticks(3);
        Assert.Empty(h.Posts);
    }

    [Fact]
    public void F_AHiddenPanelSchedulesNothingAndReadsNothingWithPendingHints()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        h.Host.HidePanel();

        h.Ticks(3);
        h.RunQueue();

        Assert.True(h.Session(id).PendingHintCount > 0);
        Assert.Empty(h.Posts);
        Assert.False(h.Scheduler.IsScheduled);
        Assert.Equal(0, h.TotalReads);
        Assert.Equal(0, h.PassCalls);
        // The hint is still there, so the idle source stays attached, and the tick is the in-memory one.
        Assert.True(h.Core.IsIdleAttached);
        Assert.False(h.Core.CanDrainNow());
    }

    [Fact]
    public void F_ContinuationAlreadyScheduledWhenThePanelIsHiddenDoesNotCallThePassAndReadsNothing()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        h.Tick();
        Assert.Single(h.Posts);

        h.Host.HidePanel();
        h.RunQueue();

        Assert.Equal(0, h.PassCalls);
        Assert.Equal(1, h.Scheduler.ContinuationsSkipped);
        Assert.Equal(0, h.TotalReads);
        Assert.True(h.Session(id).PendingHintCount > 0);

        // Shown again, it reconciles at the next rest: one drain, one selection read.
        h.Host.ShowPanel();
        h.Tick();
        h.RunQueue();
        Assert.Single(h.Reader.Reads);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void G_ASwitchOfDocumentBeforeTheContinuationNeverReadsTheOldDocumentAgainstTheNewOne()
    {
        var h = new WorkspaceIdleHarness();
        var a = h.OpenPanel(WorkspaceIdleHarness.DocumentA);
        h.Tick();
        Assert.Single(h.Posts);

        h.Activate(WorkspaceIdleHarness.DocumentB);
        var b = h.Bridge.IdOf(WorkspaceIdleHarness.DocumentB);
        h.RunQueue();

        // B has no pending hint: the continuation is no longer eligible, A is not read, and A keeps its hint for when it is active again.
        Assert.Equal(0, h.TotalReads);
        Assert.Equal(0, h.PassCalls);
        Assert.Equal(1, h.Scheduler.ContinuationsSkipped);
        Assert.True(h.Session(a).PendingHintCount > 0);
        Assert.Equal(0, h.Session(b).PendingHintCount);
    }

    [Fact]
    public void G_WhenTheNewActiveDocumentIsEligibleOnlyItsOwnSessionIsRead()
    {
        var h = new WorkspaceIdleHarness();
        var a = h.OpenPanel(WorkspaceIdleHarness.DocumentA);
        h.Tick();
        h.Activate(WorkspaceIdleHarness.DocumentB);
        var b = h.Bridge.IdOf(WorkspaceIdleHarness.DocumentB);
        h.SelectionChanged(b);

        h.RunQueue();

        Assert.Equal(new[] { (b, WorkspaceIdleHarness.DocumentB) }, h.Reader.Reads);
        Assert.DoesNotContain(h.Reader.Reads, read => read.Session == a);
        Assert.True(h.Session(a).PendingHintCount > 0);
        Assert.Equal(0, h.Session(b).PendingHintCount);
        AssertNoReadInsideACallback(h);
    }

    private static void Block(WorkspaceIdleHarness h, SessionId id, string what)
    {
        if (what == "modal") h.Host.SetModalRackCadActive(true);
        else h.CommandStarts(id);
    }

    [Theory]
    [InlineData("modal")]
    [InlineData("command")]
    public void H_ACommandOrARackCadModalPostsNothingAndReadsNothingForManyTicks(string what)
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        Block(h, id, what);

        h.Ticks(5);
        h.RunQueue();

        Assert.Empty(h.Posts);
        Assert.False(h.Scheduler.IsScheduled);
        Assert.Equal(0, h.TotalReads);
        Assert.Equal(0, h.PassCalls);
        Assert.True(h.Session(id).PendingHintCount > 0);
    }

    [Theory]
    [InlineData("modal")]
    [InlineData("command")]
    public void H_ACommandOrAModalThatStartsAfterSchedulingDelaysTheReadEvenWithTheContinuationAlreadyScheduled(string what)
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        h.Tick();
        Assert.Single(h.Posts);

        Block(h, id, what);
        h.RunQueue();

        Assert.Equal(0, h.PassCalls);
        Assert.Equal(1, h.Scheduler.ContinuationsSkipped);
        Assert.Equal(0, h.TotalReads);
        Assert.True(h.Session(id).PendingHintCount > 0);
    }

    [Theory]
    [InlineData("modal")]
    [InlineData("command")]
    public void I_WhenTheCommandOrTheModalEndsTheNextTickSchedulesOnceAndTheQueueReadsOnce(string what)
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        Block(h, id, what);
        h.Ticks(4);
        Assert.Empty(h.Posts);

        if (what == "modal") h.Host.SetModalRackCadActive(false);
        else h.CommandEnds(id);
        h.Tick();
        h.Tick();

        Assert.Single(h.Posts);
        Assert.Equal(0, h.TotalReads);

        h.RunQueue();

        Assert.Single(h.Reader.Reads);
        Assert.Single(h.FactsDepths);
        Assert.Single(h.Drains);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void J_AFailingSelectionReaderIsContainedTheCoreIsNotDegradedAndNothingIsScheduledAgain()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        h.Reader.Throw = new InvalidOperationException("selection reader");
        h.Tick();

        var error = Record.Exception(() => h.RunQueue());

        Assert.Null(error);
        // The host marks the session as faulted exactly as it does today: that treatment does not change.
        Assert.Contains(h.HostFaults, fault => fault.Context == "WorkspaceHostCore.OnDrained");
        Assert.False(h.Core.IsDegraded(id));
        Assert.Single(h.Reader.Reads);
        Assert.Empty(h.Posts);
        Assert.False(h.Scheduler.IsScheduled);
        Assert.Empty(h.SchedulerFaults);

        var readsBefore = h.TotalReads;
        h.Ticks(5);
        h.RunQueue();
        Assert.Empty(h.Posts);
        Assert.Equal(readsBefore, h.TotalReads);
        Assert.Single(h.Reader.Reads);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void J_AFailingSummaryReaderIsContainedAsFailedAndThereAreNoMoreReads()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        h.FactsThrow = new InvalidOperationException("facts reader");
        h.Tick();

        var error = Record.Exception(() => h.RunQueue());

        Assert.Null(error);
        Assert.Single(h.SummaryResults);
        Assert.Equal(BrowserSummaryStatus.Failed, h.SummaryResults[0].Status);
        Assert.Single(h.FactsDepths);
        Assert.False(h.Core.IsDegraded(id));
        Assert.Empty(h.HostFaults);
        Assert.Empty(h.Posts);

        var readsBefore = h.TotalReads;
        h.Ticks(5);
        h.RunQueue();
        Assert.Empty(h.Posts);
        Assert.Equal(readsBefore, h.TotalReads);
        AssertNoReadInsideACallback(h);
    }

    // ================================================================== the harness measures the stack

    [Fact]
    public void TheHarnessTellsAReadInsideTheIdleCallbackFromAReadAfterIt()
    {
        // A post that runs at once is the mistake IC-04 forbids: the continuation then reads while the callback is still on the stack.
        var hasty = new WorkspaceIdleHarness(postImmediately: true);
        hasty.OpenPanel();
        hasty.Tick();

        Assert.Contains(1, hasty.AllReadDepths);

        // With a queued post the same tick reads nothing, and the queue reads at depth 0.
        var calm = new WorkspaceIdleHarness();
        calm.OpenPanel();
        calm.Tick();
        Assert.Empty(calm.AllReadDepths);
        calm.RunQueue();
        Assert.NotEmpty(calm.AllReadDepths);
        AssertNoReadInsideACallback(calm);
    }

    [Fact]
    public void TheIdleSourceIsDetachedByThePassOfTheContinuationOnceNoHintsAreLeft()
    {
        var h = new WorkspaceIdleHarness();
        h.OpenPanel();
        Assert.True(h.Core.IsIdleAttached);
        Assert.Equal(1, h.Bridge.Source.Count(BridgeSource.Idle));
        h.Tick();

        h.RunQueue();

        Assert.False(h.Core.IsIdleAttached);
        Assert.Equal(0, h.Bridge.Source.Count(BridgeSource.Idle));
        Assert.Empty(h.Bridge.Source.Violations);
        Assert.Empty(h.Bridge.Faults);
    }

    // ================================================================== CanDrainNow against what the pass really does

    [Theory]
    [InlineData("eligible", true)]
    [InlineData("not-started", false)]
    [InlineData("no-hints", false)]
    [InlineData("command", false)]
    [InlineData("modal", false)]
    [InlineData("hidden", false)]
    [InlineData("degraded", false)]
    [InlineData("destroyed", false)]
    [InlineData("no-active-document", false)]
    public void CanDrainNowAgreesWithTheDrainForEveryCondition(string scenario, bool expected)
    {
        var rig = new WorkspaceBridgeRig();
        var core = rig.Core;
        if (scenario != "not-started")
        {
            core.Start();
            var id = rig.Open(WorkspaceIdleHarness.DocumentA);
            if (scenario != "no-hints") core.OnSourceEvent(id, BridgeSource.ObjectModified);
            switch (scenario)
            {
                case "command": core.OnSourceEvent(id, BridgeSource.CommandWillStart); break;
                case "modal": core.SetModalRackCadActive(true); break;
                case "hidden": core.SetPanelVisible(false); break;
                case "degraded": core.ReportFault(id, "test", new InvalidOperationException("degrade")); break;
                case "destroyed": core.OnDocumentToBeDestroyed(WorkspaceIdleHarness.DocumentA); break;
                case "no-active-document": core.OnNoActiveDocument(); break;
            }
        }
        else
        {
            // Never started: even a hint cannot exist, and the query must not throw.
            Assert.False(core.IsStarted);
        }

        var can = core.CanDrainNow();
        var drained = core.OnIdle().Count == 1;

        Assert.Equal(expected, can);
        Assert.Equal(drained, can);
    }

    [Fact]
    public void CanDrainNowIsAQueryItChangesNothingAndNeverThrows()
    {
        var rig = new WorkspaceBridgeRig();
        Assert.False(rig.Core.CanDrainNow());
        rig.Core.Start();
        var id = rig.Open(WorkspaceIdleHarness.DocumentA);
        rig.Core.OnSourceEvent(id, BridgeSource.ObjectModified);
        var attaches = rig.Source.AttachCalls;
        var detaches = rig.Source.DetachCalls;

        var results = new List<bool>();
        var error = Record.Exception(() =>
        {
            for (var i = 0; i < 5; i++) results.Add(rig.Core.CanDrainNow());
        });

        Assert.Null(error);
        Assert.Equal(new[] { true, true, true, true, true }, results);
        Assert.Equal(1, rig.SessionOf(id).PendingHintCount);
        Assert.Equal(attaches, rig.Source.AttachCalls);
        Assert.Equal(detaches, rig.Source.DetachCalls);
        Assert.Empty(rig.Faults);
    }

    // ================================================================== a close that was allowed and is still pending

    private sealed class AllowingAdvisor : IWorkspaceLifecycleAdvisor
    {
        public List<(SessionId Session, bool Intact)> Ended { get; } = new();

        public CloseDecision AdviseClose(SessionId session) => CloseDecision.Allow;

        public void SessionDestroyed(SessionId session, bool closeSequenceIntact) => Ended.Add((session, closeSequenceIntact));
    }

    [Fact]
    public void AnAllowedCloseOfTheActiveSessionMakesCanDrainNowFalseUntilTheNextEntryOfTheCore()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        h.Core.SetLifecycleAdvisor(new AllowingAdvisor());
        Assert.True(h.Core.CanDrainNow());

        h.CloseAllowed(id);

        Assert.False(h.Core.CanDrainNow());
        h.Ticks(3);
        Assert.Empty(h.Posts);
        Assert.False(h.Core.CanDrainNow());
        Assert.True(h.Session(id).PendingHintCount > 0);

        // The next entry of the core (here the panel is shown) ends the pending close and the hint is still there to drain.
        h.Core.SetPanelVisible(true);
        Assert.True(h.Core.CanDrainNow());
        Assert.Equal(0, h.TotalReads);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void AnAllowedCloseOfADocumentThatIsNotActiveAlsoMakesCanDrainNowFalse()
    {
        var h = new WorkspaceIdleHarness();
        var a = h.OpenPanel(WorkspaceIdleHarness.DocumentA);
        h.Core.SetLifecycleAdvisor(new AllowingAdvisor());
        h.Activate(WorkspaceIdleHarness.DocumentB);
        var b = h.Bridge.IdOf(WorkspaceIdleHarness.DocumentB);
        h.SelectionChanged(b);
        Assert.True(h.Core.CanDrainNow());

        h.CloseAllowed(a);

        Assert.False(h.Core.CanDrainNow());
        h.Ticks(3);
        Assert.Empty(h.Posts);

        h.Core.SetPanelVisible(true);
        Assert.True(h.Core.CanDrainNow());
        Assert.Equal(0, h.TotalReads);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void AContinuationScheduledBeforeAnAllowedCloseDoesNotCallThePassAndTheCloseSequenceStaysIntact()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        var advisor = new AllowingAdvisor();
        h.Core.SetLifecycleAdvisor(advisor);
        h.Tick();
        Assert.Single(h.Posts);

        h.CloseAllowed(id);
        h.RunQueue();

        Assert.Equal(0, h.PassCalls);
        Assert.Equal(1, h.Scheduler.ContinuationsSkipped);
        Assert.Equal(0, h.TotalReads);

        h.Destroy(WorkspaceIdleHarness.DocumentA);

        Assert.Equal(new[] { (id, true) }, advisor.Ended);
    }

    [Fact]
    public void CanDrainNowDoesNotEndThePendingCloseSequence()
    {
        var h = new WorkspaceIdleHarness();
        var id = h.OpenPanel();
        var advisor = new AllowingAdvisor();
        h.Core.SetLifecycleAdvisor(advisor);
        Assert.True(h.Core.CanDrainNow());

        h.CloseAllowed(id);
        for (var i = 0; i < 4; i++) Assert.False(h.Core.CanDrainNow());
        h.Destroy(WorkspaceIdleHarness.DocumentA);

        Assert.Equal(new[] { (id, true) }, advisor.Ended);
        Assert.Equal(0, h.TotalReads);
        AssertNoReadInsideACallback(h);
    }

    [Fact]
    public void ADestroyedSessionEndsThePendingCloseAndNeverBlocksTheNextDocument()
    {
        var h = new WorkspaceIdleHarness();
        var a = h.OpenPanel(WorkspaceIdleHarness.DocumentA);
        h.Core.SetLifecycleAdvisor(new AllowingAdvisor());
        h.Activate(WorkspaceIdleHarness.DocumentB);
        var b = h.Bridge.IdOf(WorkspaceIdleHarness.DocumentB);
        h.SelectionChanged(b);
        Assert.True(h.Core.CanDrainNow());

        h.CloseAllowed(a);
        Assert.False(h.Core.CanDrainNow());
        h.Destroy(WorkspaceIdleHarness.DocumentA);

        // The destruction forgot the pending close first: B drains again.
        Assert.True(h.Core.CanDrainNow());
        h.Tick();
        h.RunQueue();
        Assert.Single(h.Reader.Reads);
        AssertNoReadInsideACallback(h);
    }
}
