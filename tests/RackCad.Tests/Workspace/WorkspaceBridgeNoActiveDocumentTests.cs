#nullable enable
using System;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3-HOST-C2 (C2-B). Smoke-1 R20261103T175351Z-0a77 failed SM-39: AutoCAD raises DocumentActivated with no document
// when the Start tab becomes active; the bridge failed on it (a contained NullReferenceException in RackLog) and the panel
// kept showing the session of the previous document over the Start page. The pure core now has an explicit
// "no active document" transition: the active session becomes none, every session stays alive with its hookups, hints and
// context, and activating a document again makes its own session active.
public class WorkspaceBridgeNoActiveDocumentTests
{
    private static (WorkspaceBridgeRig Rig, SessionId A, SessionId B) TwoDocuments()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200); // B is the active document
        return (rig, a, b);
    }

    // ------------------------------------------------------------------ 1, 2 and 3

    [Fact]
    public void NoActiveDocumentProducesNoExceptionAndNoFault()
    {
        var (rig, _, _) = TwoDocuments();

        var thrown = Record.Exception(() => rig.Core.OnNoActiveDocument());

        Assert.Null(thrown);
        Assert.Empty(rig.Faults);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void TheActiveSessionBecomesNone()
    {
        var (rig, _, b) = TwoDocuments();
        Assert.Equal(b, rig.Core.ActiveSession);

        rig.Core.OnNoActiveDocument();

        Assert.Null(rig.Core.ActiveSession);
    }

    [Fact]
    public void EverySessionStaysAliveWithItsHookupsItsHintsAndItsContext()
    {
        var (rig, a, b) = TwoDocuments();
        var contextA = SelectionContext.One("R-A");
        var contextB = SelectionContext.Selection(2, 1);
        Assert.True(rig.SessionOf(a).ApplyContext(contextA));
        Assert.True(rig.SessionOf(b).ApplyContext(contextB));
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);
        var attachCalls = rig.Source.AttachCalls;
        var detachCalls = rig.Source.DetachCalls;
        var hookups = rig.Source.Total;

        rig.Core.OnNoActiveDocument();

        Assert.Equal(2, rig.Core.LiveSessionCount);
        Assert.Equal(a, rig.IdOf(100));
        Assert.Equal(b, rig.IdOf(200));
        Assert.False(rig.Core.IsDegraded(a));
        Assert.False(rig.Core.IsDegraded(b));
        Assert.Equal(contextA, rig.SessionOf(a).Selection);
        Assert.Equal(contextB, rig.SessionOf(b).Selection);
        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(attachCalls, rig.Source.AttachCalls);
        Assert.Equal(detachCalls, rig.Source.DetachCalls);
        Assert.Equal(hookups, rig.Source.Total);
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(2, idleAttached: true), rig.Source.Total);
        Assert.Equal(12, rig.Source.CountForSession(a));
        Assert.Equal(12, rig.Source.CountForSession(b));
    }

    [Fact]
    public void WithoutAnActiveDocumentNothingDrainsAndNoReconciliationIsQueued()
    {
        var (rig, a, b) = TwoDocuments();
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);

        rig.Core.OnNoActiveDocument();
        rig.Core.RequestReconciliation();

        Assert.Empty(rig.Core.OnIdle());
        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(0, rig.SessionOf(b).PendingHintCount);
        Assert.False(rig.Core.IsCommandActiveInActiveSession);
    }

    // ------------------------------------------------------------------ 5: a real document is activated again

    [Fact]
    public void ActivatingAnExistingDocumentAfterwardsRestoresItsOwnSessionWithoutANewHookup()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.OnNoActiveDocument();
        var attachCalls = rig.Source.AttachCalls;

        rig.Core.OnDocumentActivated(100, true);

        Assert.Equal(a, rig.Core.ActiveSession);
        Assert.Equal(2, rig.Core.LiveSessionCount);
        Assert.Equal(attachCalls, rig.Source.AttachCalls);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void TheHintsOfADocumentWaitWhileNoDocumentIsActiveAndDrainWhenItIsActivatedAgain()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.OnSourceEvent(a, BridgeSource.ImpliedSelectionChanged);
        rig.Core.OnNoActiveDocument();
        Assert.Empty(rig.Core.OnIdle());

        rig.Core.OnDocumentActivated(100, true);

        var outcome = Assert.Single(rig.Core.OnIdle());
        Assert.Equal(a, outcome.Session);
    }

    [Fact]
    public void ANewDocumentActivatedAfterwardsGetsItsOwnNewSession()
    {
        var (rig, a, b) = TwoDocuments();
        rig.Core.OnNoActiveDocument();

        var c = rig.Open(300);

        Assert.Equal(c, rig.Core.ActiveSession);
        Assert.NotEqual(a, c);
        Assert.NotEqual(b, c);
        Assert.Equal(3, rig.Core.LiveSessionCount);
    }

    [Fact]
    public void BeforeStartNoActiveDocumentChangesNothingAndAfterStopNothingIsLeft()
    {
        var rig = new WorkspaceBridgeRig();

        Assert.Null(Record.Exception(() => rig.Core.OnNoActiveDocument()));
        Assert.Equal(0, rig.Source.AttachCalls);
        Assert.False(rig.Core.IsStarted);

        rig.Core.Start();
        rig.Open(100);
        rig.Core.OnNoActiveDocument();
        rig.Core.Stop();

        Assert.Equal(0, rig.Source.Total);
        Assert.Null(rig.Core.ActiveSession);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void ClosingTheLastDocumentAfterTheStartTabWasActiveRetiresOnlyThatSession()
    {
        var (rig, a, b) = TwoDocuments();
        rig.Core.OnNoActiveDocument();

        rig.Core.OnDocumentToBeDestroyed(200);

        Assert.Null(rig.Core.ActiveSession);
        Assert.Equal(1, rig.Core.LiveSessionCount);
        Assert.Equal(a, rig.IdOf(100));
        Assert.False(rig.Core.TryGetSession(b, out _));
        Assert.Empty(rig.Source.Violations);
    }

    // ------------------------------------------------------------------ the panel: "Sin documento", no stale context

    [Fact]
    public void ThePanelShowsTheNoDocumentStateAndNoContextOfThePreviousDocument()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("IN-A"));
        rig.Host.OpenPanel();
        rig.Idle();
        Assert.Equal(SelectionContext.One("IN-A"), rig.Host.GetSnapshot().Selection);
        var reads = rig.Reader.ReadCalls;

        rig.Bridge.Core.OnNoActiveDocument();
        rig.Host.RefreshPanel();
        rig.Idle();
        var snapshot = rig.Host.GetSnapshot();

        Assert.Equal(WorkspaceHostStatus.NoDocument, snapshot.Status);
        Assert.Null(snapshot.ActiveSession);
        Assert.Equal(SelectionContext.None, snapshot.Selection);
        Assert.False(snapshot.Actions.CanReadDrawing);
        Assert.False(snapshot.Actions.CanWriteDrawing);
        Assert.Equal(WorkspaceActionBlock.NoDocument, snapshot.Actions.Block);
        Assert.Equal(reads, rig.Reader.ReadCalls);
        Assert.Equal(1, rig.Bridge.Core.LiveSessionCount);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void ComingBackFromTheStartTabShowsTheContextOfTheActivatedDocumentOnly()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("IN-A"));
        rig.Host.OpenPanel();
        rig.Idle();
        var a = rig.ActiveSession;
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("IN-B"));
        var b = rig.Activate(200);
        rig.Host.RefreshPanel();
        rig.Idle();
        Assert.Equal(SelectionContext.One("IN-B"), rig.Host.GetSnapshot().Selection);

        rig.Bridge.Core.OnNoActiveDocument();
        rig.Probe.ActiveDocumentNativeId = null;
        Assert.Equal(SelectionContext.None, rig.Host.GetSnapshot().Selection);

        rig.Activate(100);

        var snapshot = rig.Host.GetSnapshot();
        Assert.Equal(a, snapshot.ActiveSession);
        Assert.NotEqual(b, snapshot.ActiveSession);
        Assert.Equal(SelectionContext.One("IN-A"), snapshot.Selection);
        Assert.Equal(WorkspaceHostStatus.Ready, snapshot.Status);
    }
}
