#nullable enable
using System;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-06 (the fine read happens only in the drain), 07 (Classify / ApplyContext),
// 09 (panel-visible policy) and the containment of a faulty read, with a simulated host and a simulated reader.
public class WorkspaceHostDrainTests
{
    private static WorkspaceHostRig OpenRig(long nativeId = 100)
    {
        var rig = new WorkspaceHostRig().WithDocument(nativeId);
        rig.Host.OpenPanel();
        rig.Idle();
        rig.Reader.Reads.Clear();
        rig.Reader.Calls.Clear();
        rig.Reader.Invalidations.Clear();
        return rig;
    }

    private static void Select(WorkspaceHostRig rig, WorkspaceSelectionRead read)
    {
        rig.Reader.Result = read;
        rig.SelectionChanged(rig.ActiveSession);
        rig.Idle();
    }

    // ------------------------------------------------------------------ the five contexts, through Classify and ApplyContext

    [Fact]
    public void ASelectionOfOneRackAppliesTheOneContextToTheSession()
    {
        var rig = OpenRig();

        Select(rig, WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1")));

        Assert.Equal(SelectionContext.One("RACK-1"), rig.Bridge.SessionOf(rig.ActiveSession).Selection);
        Assert.Equal(SelectionContext.One("RACK-1"), rig.Host.GetSnapshot().Selection);
    }

    public static TheoryData<string, SelectionContextKind> FiveContexts() => new()
    {
        { "none", SelectionContextKind.None },
        { "one", SelectionContextKind.One },
        { "selection", SelectionContextKind.Selection },
        { "no identity", SelectionContextKind.NoIdentity },
        { "diagnostic", SelectionContextKind.Diagnostic },
    };

    [Theory]
    [MemberData(nameof(FiveContexts))]
    public void EachOfTheFiveContextsReachesTheSessionAndTheSnapshot(string name, SelectionContextKind kind)
    {
        var rig = OpenRig();
        // Start from a different context so that the change is visible.
        Select(rig, WorkspaceHostRig.Read(5));
        var read = name switch
        {
            "none" => WorkspaceHostRig.Read(0),
            "one" => WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("R")),
            "selection" => WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("A"), WorkspaceHostRig.Rack("B")),
            "no identity" => WorkspaceHostRig.Read(0, new DefinitionFact(true, " ")),
            _ => WorkspaceHostRig.Read(0, new DefinitionFact(false, null)),
        };

        Select(rig, read);

        Assert.Equal(kind, rig.Bridge.SessionOf(rig.ActiveSession).Selection.Kind);
        Assert.Equal(kind, rig.Host.GetSnapshot().Selection.Kind);
    }

    [Fact]
    public void TheContextOfTheSessionIsExactlyWhatClassifyGives()
    {
        var rig = OpenRig();
        var facts = new[] { new DefinitionFact(true, "A"), new DefinitionFact(true, null), new DefinitionFact(false, null, "X") };

        Select(rig, new WorkspaceSelectionRead(facts, 3));

        Assert.Equal(SelectionContext.Classify(facts, 3), rig.Bridge.SessionOf(rig.ActiveSession).Selection);
    }

    [Fact]
    public void AProbedIdNeverMakesAnIdentityOnTheWayToTheSession()
    {
        var rig = OpenRig();

        Select(rig, WorkspaceHostRig.Read(0, new DefinitionFact(false, null, ProbedId: "RACK-9")));

        var context = rig.Bridge.SessionOf(rig.ActiveSession).Selection;
        Assert.Equal(SelectionContext.Diagnostic, context);
        Assert.Null(context.RackId);
    }

    [Fact]
    public void AnEqualContextDoesNotChangeTheSessionAgain()
    {
        var rig = OpenRig();
        Select(rig, WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1")));
        var first = rig.Bridge.SessionOf(rig.ActiveSession).Selection;

        Select(rig, WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1")));

        Assert.Same(first, rig.Bridge.SessionOf(rig.ActiveSession).Selection);
    }

    [Fact]
    public void WithoutImpliedSelectionBecausePickFirstIsOffTheContextIsNoneAndTheStateSaysSo()
    {
        var rig = OpenRig();
        Select(rig, WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1")));

        Select(rig, new WorkspaceSelectionRead(Array.Empty<DefinitionFact>(), 0, PickFirstEnabled: false));

        var snapshot = rig.Host.GetSnapshot();
        Assert.True(snapshot.PickFirstDisabled);
        Assert.Equal(SelectionContext.None, snapshot.Selection);
    }

    [Fact]
    public void PickFirstOnAgainClearsTheState()
    {
        var rig = OpenRig();
        Select(rig, new WorkspaceSelectionRead(Array.Empty<DefinitionFact>(), 0, PickFirstEnabled: false));

        Select(rig, WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1")));

        Assert.False(rig.Host.GetSnapshot().PickFirstDisabled);
    }

    [Fact]
    public void ByDefaultPickFirstIsNotReportedAsDisabled()
    {
        var rig = OpenRig();

        Assert.False(rig.Host.GetSnapshot().PickFirstDisabled);
    }

    [Fact]
    public void AFailedReadLeavesTheContextUnchangedAndDoesNotDegradeTheSession()
    {
        var rig = OpenRig();
        Select(rig, WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1")));

        Select(rig, WorkspaceSelectionRead.Failed);

        Assert.Equal(SelectionContext.One("RACK-1"), rig.Bridge.SessionOf(rig.ActiveSession).Selection);
        Assert.Equal(WorkspaceHostStatus.Ready, rig.Host.GetSnapshot().Status);
        Assert.Empty(rig.Faults);
    }

    // ------------------------------------------------------------------ the read happens only in the drain

    [Fact]
    public void NotificationsAloneNeverReadAnything()
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;

        for (var i = 0; i < 1000; i++)
        {
            rig.SelectionChanged(session);
            rig.Bridge.Core.OnSourceEvent(session, BridgeSource.ObjectModified);
            rig.Bridge.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        }

        Assert.Equal(0, rig.Reader.ReadCalls);
        Assert.Empty(rig.Reader.Calls);
    }

    [Fact]
    public void ManyHintsCoalesceIntoOneReadAtTheNextIdle()
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1"));

        for (var i = 0; i < 1000; i++) rig.SelectionChanged(session);
        rig.Idle();

        Assert.Equal(1, rig.Reader.ReadCalls);
    }

    [Fact]
    public void DatabaseChangeHintsInvalidateTheDefinitionCacheWithoutReadingTheSelection()
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;

        rig.Bridge.Core.OnSourceEvent(session, BridgeSource.ObjectModified);
        rig.Idle();

        Assert.Equal(new[] { session }, rig.Reader.Invalidations);
        Assert.Equal(0, rig.Reader.ReadCalls);
    }

    [Theory]
    [InlineData(BridgeSource.CommandEnded)]
    [InlineData(BridgeSource.CommandCancelled)]
    [InlineData(BridgeSource.CommandFailed)]
    [InlineData(BridgeSource.ObjectAppended)]
    [InlineData(BridgeSource.ObjectErased)]
    [InlineData(BridgeSource.ObjectUnappended)]
    [InlineData(BridgeSource.ObjectReappended)]
    public void EveryHintThatInvalidatesNavigationAlsoInvalidatesTheDefinitionCache(BridgeSource source)
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;

        rig.Bridge.Core.OnSourceEvent(session, source);
        rig.Idle();

        Assert.Single(rig.Reader.Invalidations);
        Assert.Equal(0, rig.Reader.ReadCalls);
    }

    [Fact]
    public void ASelectionHintAloneDoesNotInvalidateTheCache()
    {
        var rig = OpenRig();

        rig.SelectionChanged(rig.ActiveSession);
        rig.Idle();

        Assert.Empty(rig.Reader.Invalidations);
        Assert.Equal(1, rig.Reader.ReadCalls);
    }

    [Fact]
    public void WhenBothArePendingTheCacheIsInvalidatedBeforeTheSelectionIsRead()
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;

        rig.Bridge.Core.OnSourceEvent(session, BridgeSource.ObjectModified);
        rig.SelectionChanged(session);
        rig.Idle();

        Assert.Equal(new[] { "invalidate", "read" }, rig.Reader.Calls);
    }

    [Fact]
    public void TheReaderIsAskedForTheSessionAndTheNativeIdentityOfItsDocument()
    {
        var rig = OpenRig(777);
        var session = rig.ActiveSession;

        rig.SelectionChanged(session);
        rig.Idle();

        Assert.Equal((session, 777L), Assert.Single(rig.Reader.Reads));
    }

    [Fact]
    public void ALargeSelectionIsOneReadWhoseWorkIsTheReadersAndNothingElseIsReadPerReference()
    {
        var rig = OpenRig();
        var facts = Enumerable.Range(0, 5000).Select(i => new DefinitionFact(true, "RACK-" + (i % 50))).ToArray();

        Select(rig, new WorkspaceSelectionRead(facts, 2000));

        Assert.Equal(1, rig.Reader.ReadCalls);
        Assert.Equal(SelectionContextKind.Selection, rig.Host.GetSnapshot().Selection.Kind);
        Assert.Equal(50, rig.Host.GetSnapshot().Selection.RackCount);
        Assert.Equal(2000, rig.Host.GetSnapshot().Selection.OtherCount);
    }

    // ------------------------------------------------------------------ defense in depth: a drain the policy refuses

    [Fact]
    public void ADrainDeliveredWhileThePanelIsHiddenReadsNothing()
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;
        rig.Host.HidePanel();

        rig.Host.OnDrained(new DrainOutcome(session, new DrainResult(true, new[] { DrainOperation.ReadImplicitSelection, DrainOperation.RecomputeSelectionContext })));

        Assert.Equal(0, rig.Reader.ReadCalls);
    }

    [Fact]
    public void ADrainDeliveredWhileACommandRunsReadsNothing()
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;
        rig.CommandStarts(session);

        rig.Host.OnDrained(new DrainOutcome(session, new DrainResult(true, new[] { DrainOperation.ReadImplicitSelection })));

        Assert.Equal(0, rig.Reader.ReadCalls);
    }

    [Fact]
    public void ADrainDeliveredWhileAModalIsOpenReadsNothing()
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;
        rig.Host.SetModalRackCadActive(true);

        rig.Host.OnDrained(new DrainOutcome(session, new DrainResult(true, new[] { DrainOperation.ReadImplicitSelection })));

        Assert.Equal(0, rig.Reader.ReadCalls);
    }

    [Fact]
    public void ADrainForASessionThatIsNotTheActiveOneReadsNothing()
    {
        var rig = OpenRig();
        var a = rig.ActiveSession;
        rig.Activate(200);

        rig.Host.OnDrained(new DrainOutcome(a, new DrainResult(true, new[] { DrainOperation.ReadImplicitSelection })));

        Assert.Equal(0, rig.Reader.ReadCalls);
    }

    [Fact]
    public void ADrainForARetiredSessionReadsNothing()
    {
        var rig = OpenRig();
        var session = rig.ActiveSession;
        rig.CloseActive(100);

        rig.Host.OnDrained(new DrainOutcome(session, new DrainResult(true, new[] { DrainOperation.ReadImplicitSelection })));

        Assert.Equal(0, rig.Reader.ReadCalls);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void ADrainThatIsNotDrainedChangesNothing()
    {
        var rig = OpenRig();

        rig.Host.OnDrained(new DrainOutcome(rig.ActiveSession, new DrainResult(false, Array.Empty<DrainOperation>())));

        Assert.Equal(0, rig.Reader.ReadCalls);
        Assert.Empty(rig.Reader.Invalidations);
    }

    [Fact]
    public void ADrainBeforeThePanelWasEverOpenedReadsNothing()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        rig.Host.OnDrained(new DrainOutcome(new SessionId(1), new DrainResult(true, new[] { DrainOperation.ReadImplicitSelection })));

        Assert.Equal(0, rig.Reader.ReadCalls);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void AMarkOnlyDrainReadsNothingAndInvalidatesNothing()
    {
        var rig = OpenRig();

        rig.Host.OnDrained(new DrainOutcome(rig.ActiveSession, new DrainResult(true, new[] { DrainOperation.MarkDraftsPossiblyStale })));

        Assert.Equal(0, rig.Reader.ReadCalls);
        Assert.Empty(rig.Reader.Invalidations);
    }

    // ------------------------------------------------------------------ containment of a faulty read

    [Fact]
    public void AReaderThatThrowsIsContainedAndNothingReachesTheHost()
    {
        var rig = OpenRig();
        rig.Reader.Throw = new InvalidOperationException("simulated read failure");

        var exception = Record.Exception(() =>
        {
            rig.SelectionChanged(rig.ActiveSession);
            rig.Idle();
        });

        Assert.Null(exception);
        Assert.Single(rig.Faults);
        Assert.IsType<InvalidOperationException>(rig.Faults[0].Error);
    }

    [Fact]
    public void AReaderThatThrowsLeavesTheContextAsItWasAndDegradesOnlyThatSession()
    {
        var rig = OpenRig();
        Select(rig, WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1")));
        rig.Reader.Throw = new InvalidOperationException("simulated read failure");

        Select(rig, WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-2")));

        var snapshot = rig.Host.GetSnapshot();
        Assert.Equal(WorkspaceHostStatus.Degraded, snapshot.Status);
        Assert.Equal(WorkspaceActionBlock.Degraded, snapshot.Actions.Block);
        Assert.False(snapshot.Actions.CanReadDrawing);
        Assert.Equal(SelectionContext.One("RACK-1"), snapshot.Selection);
    }

    [Fact]
    public void ADegradedSessionIsNotReadAnyMore()
    {
        var rig = OpenRig();
        rig.Reader.Throw = new InvalidOperationException("simulated read failure");
        Select(rig, WorkspaceHostRig.Read(0));
        var reads = rig.Reader.ReadCalls;
        rig.Reader.Throw = null;

        for (var i = 0; i < 5; i++)
        {
            rig.SelectionChanged(rig.ActiveSession);
            rig.Idle();
        }

        Assert.Equal(reads, rig.Reader.ReadCalls);
    }

    [Fact]
    public void AnotherDocumentIsNotDegradedByTheFaultOfTheFirst()
    {
        var rig = OpenRig();
        rig.Reader.Throw = new InvalidOperationException("simulated read failure");
        Select(rig, WorkspaceHostRig.Read(0));
        rig.Reader.Throw = null;

        rig.Activate(200);
        rig.Host.RefreshPanel();
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("B"));
        rig.Idle();

        Assert.Equal(WorkspaceHostStatus.Ready, rig.Host.GetSnapshot().Status);
        Assert.Equal(SelectionContext.One("B"), rig.Host.GetSnapshot().Selection);
    }

    [Fact]
    public void AFaultInTheLogCallbackNeverBecomesASecondFault()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        var host = new WorkspaceHostCore(rig.Link, rig.Probe, rig.Reader, rig.Instrumentation, (_, _) => throw new InvalidOperationException("logging failed"));
        host.OpenPanel();
        rig.Reader.Throw = new InvalidOperationException("simulated read failure");

        var exception = Record.Exception(() =>
        {
            rig.SelectionChanged(rig.ActiveSession);
            rig.Idle();
            foreach (var outcome in rig.Drains) host.OnDrained(outcome);
        });

        Assert.Null(exception);
    }

    // ------------------------------------------------------------------ the measurement of a drain

    [Fact]
    public void ADrainIsMeasuredOnlyWhileTheInstrumentationIsOn()
    {
        var rig = OpenRig();
        rig.SelectionChanged(rig.ActiveSession);
        rig.Idle();
        Assert.Empty(rig.Host.GetMeasurementReport().Points);

        rig.Host.SetInstrumentationEnabled(true);
        rig.SelectionChanged(rig.ActiveSession);
        rig.Idle();

        var points = rig.Host.GetMeasurementReport().Points.Select(p => p.Point).ToArray();
        Assert.Contains(WorkspaceMeasurementPoints.Drain, points);
        Assert.Contains(WorkspaceMeasurementPoints.SelectionRead, points);
    }

    [Fact]
    public void AnInvalidationIsMeasuredAsItsOwnPoint()
    {
        var rig = OpenRig();
        rig.Host.SetInstrumentationEnabled(true);

        rig.Bridge.Core.OnSourceEvent(rig.ActiveSession, BridgeSource.ObjectErased);
        rig.Idle();

        Assert.Contains(rig.Host.GetMeasurementReport().Points, p => p.Point == WorkspaceMeasurementPoints.CacheInvalidate);
    }
}
