#nullable enable
using System;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T2-BRIDGE: D-04 / INV-F1-T2-04, 05 with a simulated event source.
public class WorkspaceBridgeDegradedModeTests
{
    private static (WorkspaceBridgeRig Rig, SessionId A, SessionId B) TwoDocuments()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);
        return (rig, a, b);
    }

    [Fact]
    public void AFaultOfTheBridgeDegradesOnlyTheAffectedSessionAndRemovesOnlyItsSynchronizationHookups()
    {
        var (rig, a, b) = TwoDocuments();
        rig.Source.FailAttach = x => x.Source == BridgeSource.Idle;

        var thrown = Record.Exception(() => rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified));

        Assert.Null(thrown);
        Assert.True(rig.Core.IsDegraded(a));
        Assert.False(rig.Core.IsDegraded(b));
        // A keeps only BeginDocumentClose; B is untouched.
        Assert.Equal(1, rig.Source.CountForSession(a));
        Assert.Equal(1, rig.Source.CountFor(a, BridgeSource.BeginDocumentClose));
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(b));
        Assert.Equal(WorkspaceBridgeRig.CollectionSources + 1 + WorkspaceBridgeRig.SourcesPerSession, rig.Source.Total);
        var fault = Assert.Single(rig.Faults);
        Assert.IsType<InvalidOperationException>(fault.Error);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void ADegradedSessionIsLocalTheOtherSessionKeepsSynchronizingNormally()
    {
        var (rig, a, b) = TwoDocuments();
        rig.Source.FailAttach = x => x.Source == BridgeSource.Idle;
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);
        rig.Source.FailAttach = null;

        rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));
        Assert.Equal(1, rig.SessionOf(b).PendingHintCount);

        var outcome = Assert.Single(rig.Core.OnIdle());
        Assert.Equal(b, outcome.Session);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
    }

    [Fact]
    public void ADegradedSessionNoLongerEnqueuesAndNoLongerKeepsIdleAlive()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified); // pending hint, idle attached
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));

        rig.Core.ReportFault(a, "test", new InvalidOperationException("boom"));

        Assert.True(rig.Core.IsDegraded(a));
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));

        var pending = rig.SessionOf(a).PendingHintCount;
        rig.Core.OnSourceEvent(a, BridgeSource.ImpliedSelectionChanged); // a stale in-flight notification
        Assert.Equal(pending, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
    }

    [Fact]
    public void ADegradedSessionIsNeverDrainedAutomatically()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);
        rig.Core.OnDocumentActivated(100, true); // A becomes the active document
        rig.Core.ReportFault(a, "test", new InvalidOperationException("boom"));

        Assert.Empty(rig.Core.OnIdle());
    }

    [Fact]
    public void BeginDocumentCloseStaysAttachedForTheWholeLifeOfADegradedSession()
    {
        var (rig, a, b) = TwoDocuments();
        rig.Core.ReportFault(a, "test", new InvalidOperationException("boom"));

        Assert.Equal(1, rig.Source.CountFor(a, BridgeSource.BeginDocumentClose));
        foreach (var source in BridgeSources.PerSession.Skip(1)) Assert.Equal(0, rig.Source.CountFor(a, source));

        // A second fault, a repeated activation and a drain of the other session do not remove it.
        rig.Core.ReportFault(a, "again", new InvalidOperationException("boom 2"));
        rig.Core.OnDocumentActivated(100, true);
        rig.Core.OnDocumentActivated(200, true);
        rig.Core.OnIdle();
        Assert.Equal(1, rig.Source.CountFor(a, BridgeSource.BeginDocumentClose));

        // Only the destruction of the document removes it.
        rig.Core.OnDocumentToBeDestroyed(100);
        Assert.Equal(0, rig.Source.CountFor(a, BridgeSource.BeginDocumentClose));
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(1), rig.Source.Total);
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(b));
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void TheCloseHandlerUsesOnlyInMemoryStateAndTouchesNoHookupNorHost()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.ReportFault(a, "test", new InvalidOperationException("boom"));
        var attachCalls = rig.Source.AttachCalls;
        var detachCalls = rig.Source.DetachCalls;
        var total = rig.Source.Total;

        var healthy = rig.Core.OnBeginDocumentClose(rig.IdOf(200));
        var degraded = rig.Core.OnBeginDocumentClose(a);

        Assert.Equal(CloseDecision.Allow, healthy);
        Assert.Equal(CloseDecision.Allow, degraded);
        Assert.Equal(attachCalls, rig.Source.AttachCalls);
        Assert.Equal(detachCalls, rig.Source.DetachCalls);
        Assert.Equal(total, rig.Source.Total);
        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);
    }

    [Fact]
    public void TheCloseHandlerNeverThrowsEvenForARetiredOrUnknownSession()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.OnDocumentToBeDestroyed(100);

        var thrown = Record.Exception(() => rig.Core.OnBeginDocumentClose(a));

        Assert.Null(thrown);
        Assert.Null(Record.Exception(() => rig.Core.OnBeginDocumentClose(new SessionId(9999))));
    }

    [Fact]
    public void AFailureWhileAttachingASessionDegradesOnlyThatSessionAndKeepsItsCloseHandler()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var b = rig.Open(200);
        rig.Source.FailAttach = x => x.Source == BridgeSource.ObjectModified;

        var thrown = Record.Exception(() => rig.Open(100));

        Assert.Null(thrown);
        var a = rig.IdOf(100);
        Assert.True(rig.Core.IsDegraded(a));
        Assert.False(rig.Core.IsDegraded(b));
        Assert.Equal(1, rig.Source.CountForSession(a));
        Assert.Equal(1, rig.Source.CountFor(a, BridgeSource.BeginDocumentClose));
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(b));
        Assert.NotEmpty(rig.Faults);
    }

    [Fact]
    public void NoHandlerEverPropagatesAnExceptionToTheHost()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Source.FailAttach = _ => true;
        rig.Source.FailDetach = _ => true;

        var thrown = Record.Exception(() =>
        {
            rig.Core.Start();
            rig.Core.OnDocumentCreated(100);
            rig.Core.OnDocumentActivated(100, true);
            rig.Core.OnDocumentActivated(200, false);
            foreach (var source in BridgeSources.All)
            {
                rig.Core.OnSourceEvent(new SessionId(1), source);
                rig.Core.OnSourceEvent(new SessionId(2), source);
            }

            rig.Core.OnIdle();
            rig.Core.OnBeginDocumentClose(new SessionId(1));
            rig.Core.OnDocumentToBeDestroyed(100);
            rig.Core.OnDocumentToBeDestroyed(200);
            rig.Core.Stop();
        });

        Assert.Null(thrown);
        Assert.NotEmpty(rig.Faults);
    }

    [Fact]
    public void AFaultInTheLoggerNeverBecomesASecondFault()
    {
        var source = new WorkspaceSimulatedHostSource { FailAttach = x => x.Source == BridgeSource.Idle };
        var core = new EventBridgeCore(source, (_, _) => throw new InvalidOperationException("the log is broken"));
        core.Start();
        core.OnDocumentActivated(100, true);
        core.TryGetSession(100, out var session);

        var thrown = Record.Exception(() => core.OnSourceEvent(session!.Id, BridgeSource.ObjectModified));

        Assert.Null(thrown);
        Assert.True(core.IsDegraded(session!.Id));
    }

    [Fact]
    public void AFailingDetachDuringDestructionStillRetiresTheSessionAndTheOthersAreUntouched()
    {
        var (rig, a, b) = TwoDocuments();
        rig.Source.FailDetach = x => x.Source == BridgeSource.ObjectModified;

        var thrown = Record.Exception(() => rig.Core.OnDocumentToBeDestroyed(100));

        Assert.Null(thrown);
        Assert.False(rig.Core.TryGetSession(a, out _));
        Assert.Equal(1, rig.Source.CountForSession(a)); // only the hookup whose removal failed is reported as alive
        Assert.Equal(1, rig.Source.CountFor(a, BridgeSource.ObjectModified));
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(b));
        Assert.NotEmpty(rig.Faults);
    }
}
