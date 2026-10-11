#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

/// <summary>I-64 F1-T3A-HOST-CORE test double: the in-memory facts of the host (active document, quiescence).</summary>
public sealed class WorkspaceSimulatedProbe : IWorkspaceHostProbe
{
    public long? ActiveDocumentNativeId { get; set; }

    public bool IsActiveDocumentQuiescent { get; set; } = true;
}

/// <summary>I-64 F1-T3A-HOST-CORE test double: a simulated fine selection reader that records every call.</summary>
public sealed class WorkspaceSimulatedReader : IWorkspaceSelectionReader
{
    public WorkspaceSelectionRead Result { get; set; } = new(Array.Empty<DefinitionFact>(), 0);

    public Exception? Throw { get; set; }

    public List<(SessionId Session, long NativeId)> Reads { get; } = new();

    public List<SessionId> Invalidations { get; } = new();

    /// <summary>"read" and "invalidate" in the order the host asked for them.</summary>
    public List<string> Calls { get; } = new();

    public int ReadCalls => Reads.Count;

    public WorkspaceSelectionRead Read(SessionId session, long nativeInstanceId)
    {
        Calls.Add("read");
        Reads.Add((session, nativeInstanceId));
        if (Throw != null) throw Throw;
        return Result;
    }

    public void Invalidate(SessionId session)
    {
        Calls.Add("invalidate");
        Invalidations.Add(session);
    }
}

/// <summary>
/// I-64 F1-T3A-HOST-CORE test double: the link to the bridge, over the REAL pure bridge core and the simulated event
/// source. It counts what the host asked for, independently of what the core believes it holds.
/// </summary>
public sealed class WorkspaceSimulatedBridgeLink : IWorkspaceBridgeLink
{
    private readonly WorkspaceSimulatedProbe _probe;

    public WorkspaceSimulatedBridgeLink(WorkspaceBridgeRig rig, WorkspaceSimulatedProbe probe)
    {
        Rig = rig;
        _probe = probe;
    }

    public WorkspaceBridgeRig Rig { get; }

    public int StartCalls { get; private set; }

    public int ReconciliationCalls { get; private set; }

    public List<bool> VisibleHistory { get; } = new();

    public List<bool> ModalHistory { get; } = new();

    public SessionId? ActiveSession => Rig.Core.ActiveSession;

    public void Start()
    {
        StartCalls++;
        Rig.Core.Start();
        if (_probe.ActiveDocumentNativeId is { } id) Rig.Core.OnDocumentActivated(id, _probe.IsActiveDocumentQuiescent);
    }

    public void SetPanelVisible(bool visible)
    {
        VisibleHistory.Add(visible);
        Rig.Core.SetPanelVisible(visible);
    }

    public void SetModalRackCadActive(bool active)
    {
        ModalHistory.Add(active);
        Rig.Core.SetModalRackCadActive(active);
    }

    public bool TryGetSession(SessionId id, out WorkspaceSession? session) => Rig.Core.TryGetSession(id, out session);

    public RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId) =>
        Rig.Core.Authorize(request, executingNativeInstanceId);

    public void RequestReconciliation()
    {
        ReconciliationCalls++;
        Rig.Core.RequestReconciliation();
    }
}

/// <summary>
/// A host core wired to a simulated bridge, probe and reader. Documents open and close the way the host reports them;
/// <see cref="Idle"/> is the idle pass: the bridge drains and the host core receives the outcomes (the Drained event).
/// </summary>
public sealed class WorkspaceHostRig
{
    public WorkspaceHostRig()
    {
        Bridge = new WorkspaceBridgeRig();
        Link = new WorkspaceSimulatedBridgeLink(Bridge, Probe);
        Instrumentation = new WorkspaceInstrumentation();
        Host = new WorkspaceHostCore(Link, Probe, Reader, Instrumentation, (context, error) => Faults.Add((context, error)));
    }

    public WorkspaceBridgeRig Bridge { get; }

    public WorkspaceSimulatedProbe Probe { get; } = new();

    public WorkspaceSimulatedReader Reader { get; } = new();

    public WorkspaceSimulatedBridgeLink Link { get; }

    public WorkspaceInstrumentation Instrumentation { get; }

    public WorkspaceHostCore Host { get; }

    public List<(string Context, Exception Error)> Faults { get; } = new();

    public List<DrainOutcome> Drains { get; } = new();

    /// <summary>The document is already open and active before the panel is: its native id.</summary>
    public WorkspaceHostRig WithDocument(long nativeId)
    {
        Probe.ActiveDocumentNativeId = nativeId;
        return this;
    }

    /// <summary>The host reports that a document was activated (the bridge creates its session).</summary>
    public SessionId Activate(long nativeId)
    {
        Probe.ActiveDocumentNativeId = nativeId;
        Probe.IsActiveDocumentQuiescent = true;
        return Bridge.Open(nativeId);
    }

    public void CloseActive(long nativeId)
    {
        Bridge.Core.OnDocumentToBeDestroyed(nativeId);
        Probe.ActiveDocumentNativeId = null;
    }

    public SessionId ActiveSession
    {
        get
        {
            var active = Bridge.Core.ActiveSession;
            Assert.True(active.HasValue, "no active session");
            return active!.Value;
        }
    }

    public void SelectionChanged(SessionId session) => Bridge.Core.OnSourceEvent(session, BridgeSource.ImpliedSelectionChanged);

    public void CommandStarts(SessionId session)
    {
        Probe.IsActiveDocumentQuiescent = false;
        Bridge.Core.OnSourceEvent(session, BridgeSource.CommandWillStart);
    }

    public void CommandEnds(SessionId session)
    {
        Bridge.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        Bridge.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);
        Probe.IsActiveDocumentQuiescent = true;
    }

    /// <summary>One idle pass: the bridge drains at most once and the host receives each outcome.</summary>
    public IReadOnlyList<DrainOutcome> Idle()
    {
        var outcomes = Bridge.Core.OnIdle();
        foreach (var outcome in outcomes)
        {
            Drains.Add(outcome);
            Host.OnDrained(outcome);
        }

        return outcomes;
    }

    public static WorkspaceSelectionRead Read(int nonRack, params DefinitionFact[] references) =>
        new(references, nonRack);

    public static DefinitionFact Rack(string id) => new(true, id);
}
