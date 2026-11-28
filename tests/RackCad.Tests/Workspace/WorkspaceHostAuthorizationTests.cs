#nullable enable
using System;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-03 (every request carries its SessionId and is authorized; a rejected request has no effect).
public class WorkspaceHostAuthorizationTests
{
    private static WorkspaceHostRig TwoDocuments(out SessionId a, out SessionId b)
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        a = rig.ActiveSession;
        rig.Idle();
        b = rig.Activate(200);
        rig.Host.RefreshPanel();
        rig.Idle();
        rig.Reader.Reads.Clear();
        return rig;
    }

    [Fact]
    public void ARequestOfTheSessionOfTheActiveDocumentIsAccepted()
    {
        var rig = TwoDocuments(out _, out var b);

        Assert.Equal(RequestDecision.Accepted, rig.Host.Authorize(new WorkspaceRequest(b)));
    }

    [Fact]
    public void ARequestOfAnotherDocumentSessionIsRejectedAsAMismatch()
    {
        var rig = TwoDocuments(out var a, out _);

        Assert.Equal(RequestDecision.RejectedSessionMismatch, rig.Host.Authorize(new WorkspaceRequest(a)));
    }

    [Fact]
    public void ARequestOfADestroyedSessionIsRejectedAsDestroyed()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        var a = rig.ActiveSession;
        rig.CloseActive(100);

        Assert.Equal(RequestDecision.RejectedSessionDestroyed, rig.Host.Authorize(new WorkspaceRequest(a)));
    }

    [Fact]
    public void ARequestOfASessionThatNeverExistedIsRejectedAsDestroyed()
    {
        var rig = TwoDocuments(out _, out _);

        Assert.Equal(RequestDecision.RejectedSessionDestroyed, rig.Host.Authorize(new WorkspaceRequest(new SessionId(9999))));
    }

    [Fact]
    public void ARecycledNativeIdentityNeverRevivesTheOldSession()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        var old = rig.ActiveSession;
        rig.CloseActive(100);

        var fresh = rig.Activate(100);

        Assert.NotEqual(old, fresh);
        Assert.Equal(RequestDecision.RejectedSessionDestroyed, rig.Host.Authorize(new WorkspaceRequest(old)));
        Assert.Equal(RequestDecision.Accepted, rig.Host.Authorize(new WorkspaceRequest(fresh)));
    }

    [Fact]
    public void WithNoDocumentOpenNoRequestIsAccepted()
    {
        var rig = new WorkspaceHostRig();
        rig.Host.OpenPanel();

        Assert.NotEqual(RequestDecision.Accepted, rig.Host.Authorize(new WorkspaceRequest(new SessionId(1))));
    }

    [Fact]
    public void BeforeThePanelIsOpenedNoRequestIsAccepted()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        Assert.NotEqual(RequestDecision.Accepted, rig.Host.Authorize(new WorkspaceRequest(new SessionId(1))));
    }

    [Fact]
    public void TheVerdictIsTheSameAsTheRegistryOfTheSingleBridgeCore()
    {
        var rig = TwoDocuments(out var a, out var b);

        foreach (var origin in new[] { a, b, new SessionId(0), new SessionId(77) })
        {
            var expected = rig.Bridge.Core.Authorize(new WorkspaceRequest(origin), 200);
            Assert.Equal(expected, rig.Host.Authorize(new WorkspaceRequest(origin)));
        }
    }

    [Fact]
    public void ARejectedRequestHasNoEffectAtAll()
    {
        var rig = TwoDocuments(out var a, out var b);
        var selectionOfA = rig.Bridge.SessionOf(a).Selection;
        var pendingA = rig.Bridge.SessionOf(a).PendingHintCount;
        var attach = rig.Bridge.Source.AttachCalls;
        var detach = rig.Bridge.Source.DetachCalls;
        var reconciliations = rig.Link.ReconciliationCalls;

        var response = rig.Host.GetSelectionContext(new WorkspaceRequest(a));
        rig.Host.Authorize(new WorkspaceRequest(a));
        rig.Host.Authorize(new WorkspaceRequest(new SessionId(9999)));

        Assert.Equal(RequestDecision.RejectedSessionMismatch, response.Decision);
        Assert.Null(response.Context);
        Assert.Equal(0, rig.Reader.ReadCalls);
        Assert.Empty(rig.Reader.Invalidations);
        Assert.Equal(selectionOfA, rig.Bridge.SessionOf(a).Selection);
        Assert.Equal(pendingA, rig.Bridge.SessionOf(a).PendingHintCount);
        Assert.Equal(attach, rig.Bridge.Source.AttachCalls);
        Assert.Equal(detach, rig.Bridge.Source.DetachCalls);
        Assert.Equal(reconciliations, rig.Link.ReconciliationCalls);
        Assert.Equal(b, rig.Bridge.Core.ActiveSession);
    }

    [Fact]
    public void AnAcceptedSelectionRequestReturnsTheContextOfItsSessionWithoutReading()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1"));
        rig.Host.OpenPanel();
        rig.Idle();
        var session = rig.ActiveSession;
        var reads = rig.Reader.ReadCalls;

        var response = rig.Host.GetSelectionContext(new WorkspaceRequest(session));

        Assert.Equal(RequestDecision.Accepted, response.Decision);
        Assert.Equal(SelectionContext.One("RACK-1"), response.Context);
        Assert.Equal(reads, rig.Reader.ReadCalls);
    }

    [Fact]
    public void EveryRejectionKindYieldsAnEmptyResponse()
    {
        var rig = TwoDocuments(out var a, out _);

        foreach (var origin in new[] { a, new SessionId(555) })
        {
            var response = rig.Host.GetSelectionContext(new WorkspaceRequest(origin));
            Assert.NotEqual(RequestDecision.Accepted, response.Decision);
            Assert.Null(response.Context);
        }
    }

    [Fact]
    public void ASessionTheUserLeftIsStillAuthorizedAsItselfOnceItsDocumentIsActiveAgain()
    {
        var rig = TwoDocuments(out var a, out var b);
        Assert.NotEqual(RequestDecision.Accepted, rig.Host.Authorize(new WorkspaceRequest(a)));

        rig.Activate(100);

        Assert.Equal(RequestDecision.Accepted, rig.Host.Authorize(new WorkspaceRequest(a)));
        Assert.NotEqual(RequestDecision.Accepted, rig.Host.Authorize(new WorkspaceRequest(b)));
    }

    [Fact]
    public void TheAuthorizationGoesThroughTheBridgeLinkAndNotThroughAnotherRegistry()
    {
        var rig = TwoDocuments(out _, out var b);
        var live = rig.Bridge.Core.LiveSessionCount;

        for (var i = 0; i < 50; i++) rig.Host.Authorize(new WorkspaceRequest(b));

        Assert.Equal(live, rig.Bridge.Core.LiveSessionCount);
        Assert.Equal(2, live);
    }
}
