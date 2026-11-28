#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F1-T2-BRIDGE test double: a simulated event source. It counts the native hookups the pure core asks for,
/// per attachment, independently of what the core believes it holds. A second hookup of the same attachment, or a
/// removal of one that is not hooked, is recorded as a violation.
/// </summary>
public sealed class WorkspaceSimulatedHostSource : IHostEventSource
{
    private readonly Dictionary<SourceAttachment, int> _hooks = new();

    public List<string> Violations { get; } = new();

    public int AttachCalls { get; private set; }

    public int DetachCalls { get; private set; }

    public Func<SourceAttachment, bool>? FailAttach { get; set; }

    public Func<SourceAttachment, bool>? FailDetach { get; set; }

    public void Attach(SourceAttachment attachment)
    {
        AttachCalls++;
        if (FailAttach?.Invoke(attachment) == true)
            throw new InvalidOperationException("simulated attach failure: " + attachment.Source);

        _hooks.TryGetValue(attachment, out var count);
        if (count >= 1) Violations.Add("second hookup of " + attachment);
        _hooks[attachment] = count + 1;
    }

    public void Detach(SourceAttachment attachment)
    {
        DetachCalls++;
        if (FailDetach?.Invoke(attachment) == true)
            throw new InvalidOperationException("simulated detach failure: " + attachment.Source);

        if (!_hooks.TryGetValue(attachment, out var count) || count == 0)
        {
            Violations.Add("removal of a hookup that does not exist: " + attachment);
            return;
        }

        if (count == 1) _hooks.Remove(attachment);
        else _hooks[attachment] = count - 1;
    }

    /// <summary>Total native hookups currently alive.</summary>
    public int Total => _hooks.Values.Sum();

    public int Count(BridgeSource source) => _hooks.Where(p => p.Key.Source == source).Sum(p => p.Value);

    public int CountForSession(SessionId session) => _hooks.Where(p => p.Key.Session == session).Sum(p => p.Value);

    public int CountFor(SessionId session, BridgeSource source) =>
        _hooks.Where(p => p.Key.Session == session && p.Key.Source == source).Sum(p => p.Value);

    public bool AllHooksAreSingle => _hooks.Values.All(v => v == 1);
}

/// <summary>A core wired to a simulated source, with the faults it logged.</summary>
public sealed class WorkspaceBridgeRig
{
    /// <summary>Frozen D-04: creation, activation and destruction of documents.</summary>
    public const int CollectionSources = 3;

    /// <summary>Frozen D-04: 7 document events (BeginDocumentClose, ImpliedSelectionChanged, 4 command events, EnteringQuiescentState) + 5 object events.</summary>
    public const int SourcesPerSession = 12;

    public WorkspaceBridgeRig()
    {
        Core = new EventBridgeCore(Source, (context, error) => Faults.Add((context, error)));
    }

    public WorkspaceSimulatedHostSource Source { get; } = new();

    public List<(string Context, Exception Error)> Faults { get; } = new();

    public EventBridgeCore Core { get; }

    public static int ExpectedHookups(int liveDocuments, bool idleAttached = false) =>
        CollectionSources + (SourcesPerSession * liveDocuments) + (idleAttached ? 1 : 0);

    /// <summary>Activates a document and returns its session id.</summary>
    public SessionId Open(long nativeId, bool quiescent = true)
    {
        Core.OnDocumentActivated(nativeId, quiescent);
        return IdOf(nativeId);
    }

    public SessionId IdOf(long nativeId)
    {
        Assert.True(Core.TryGetSession(nativeId, out var session), "no live session for native id " + nativeId);
        return session!.Id;
    }

    public WorkspaceSession SessionOf(SessionId id)
    {
        Assert.True(Core.TryGetSession(id, out var session), "no live session " + id);
        return session!;
    }

    public static readonly BridgeSource[] ObjectSources =
    {
        BridgeSource.ObjectAppended, BridgeSource.ObjectErased, BridgeSource.ObjectModified,
        BridgeSource.ObjectUnappended, BridgeSource.ObjectReappended,
    };
}
