#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack.Pilot;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4-T4C1-HOST (D-T4C-17): the doubles of the host of the update of the pilot. Nothing here loads AutoCAD: the primitives, the target, the
/// lock and the writer are plain objects, and the events of the command context are simulated by the test. Every double records what it was
/// asked for, in order, in one shared list of events.
/// </summary>
internal enum PilotHostMode
{
    /// <summary>The callback runs inside the request, and the completion arrives right after it.</summary>
    Synchronous,

    /// <summary>Nothing runs until the test runs the callback and the completion by hand: the real case measured by H0.</summary>
    Deferred,

    /// <summary>Neither the callback nor the completion ever arrive: a native refusal, which neither throws nor completes (H-50).</summary>
    Never,
}

/// <summary>The lock of the target: it records its release and can be told to throw when it is released.</summary>
internal sealed class PilotHostLockDouble : IDisposable
{
    private readonly List<string> _events;

    public PilotHostLockDouble(List<string> events, bool throwOnDispose)
    {
        _events = events;
        ThrowOnDispose = throwOnDispose;
    }

    public bool ThrowOnDispose { get; }

    public int Disposed { get; private set; }

    public void Dispose()
    {
        Disposed++;
        _events.Add("Unlock");
        if (ThrowOnDispose) throw new InvalidOperationException("unlock boom");
    }
}

/// <summary>The opaque target of one run: the lock, the writer and the retirement, with a mode for each way to fail.</summary>
internal sealed class PilotHostTargetDouble : IPushBackPilotHostTarget
{
    private readonly List<string> _events;

    public PilotHostTargetDouble(List<string> events, IPushBackPilotUpdatePort writer)
    {
        _events = events;
        Writer = writer;
    }

    public IPushBackPilotUpdatePort Writer { get; }

    public bool ThrowOnLock { get; set; }

    public bool ThrowOnUnlock { get; set; }

    public bool ThrowOnCreateWriter { get; set; }

    public bool ThrowOnRetire { get; set; }

    /// <summary>The target answers a null lock: a host that does not honor the contract of Lock (M-06 (d)).</summary>
    public bool NullLock { get; set; }

    public int Locks { get; private set; }

    public int CreateWriters { get; private set; }

    public int Retires { get; private set; }

    public PilotHostLockDouble? LastLock { get; private set; }

    public IPushBackPilotUpdatePort? Retired { get; private set; }

    public IDisposable Lock()
    {
        Locks++;
        _events.Add("Lock");
        if (ThrowOnLock) throw new InvalidOperationException("lock boom");
        if (NullLock) return null!;
        LastLock = new PilotHostLockDouble(_events, ThrowOnUnlock);
        return LastLock;
    }

    public IPushBackPilotUpdatePort CreateWriter()
    {
        CreateWriters++;
        _events.Add("CreateWriter");
        if (ThrowOnCreateWriter) throw new InvalidOperationException("create boom");
        return Writer;
    }

    public void Retire(IPushBackPilotUpdatePort writer)
    {
        Retires++;
        Retired = writer;
        _events.Add("Retire");
        if (ThrowOnRetire) throw new InvalidOperationException("retire boom");
    }
}

/// <summary>
/// The primitives of the host: when the command context is entered, which document is open and active, and how the callback and the completion
/// arrive. The events around the callback (the entries into rest and the start of the command measured by H0) are supplied by the test.
/// </summary>
internal sealed class PilotHostPrimitivesDouble : IPushBackPilotHostPrimitives
{
    private readonly List<string> _events;

    public PilotHostPrimitivesDouble(List<string> events)
    {
        _events = events;
    }

    public PilotHostMode Mode { get; set; } = PilotHostMode.Synchronous;

    public bool ThrowOnEnter { get; set; }

    /// <summary>The host registers the callback and then throws: a callback that is queued although the request failed.</summary>
    public bool ThrowAfterRegistering { get; set; }

    public bool ThrowAfterSynchronousCallback { get; set; }

    public bool RepeatCallback { get; set; }

    /// <summary>The target that a document open and active resolves to, and its native identity.</summary>
    public PilotHostTargetDouble? Target { get; set; }

    public long TargetNativeId { get; set; }

    /// <summary>The native identity of the document that is open and active now; null when none is.</summary>
    public long? ActiveNativeId { get; set; }

    /// <summary>The target that a recycled identity resolves to: another document with the same native identity.</summary>
    public PilotHostTargetDouble? RecycledTarget { get; set; }

    public bool ThrowOnOpen { get; set; }

    /// <summary>What happens in the host before the callback of the deferred mode (the measured order of H0) and after it.</summary>
    public Action? BeforeCallback { get; set; }

    public Action? AfterCallback { get; set; }

    public Action? Callback { get; private set; }

    public Action? Completed { get; private set; }

    public int Enters { get; private set; }

    public int Opens { get; private set; }

    public void EnterCommandContext(Action callback, Action completed)
    {
        Enters++;
        _events.Add("Enter");
        if (ThrowOnEnter) throw new InvalidOperationException("enter boom");
        Callback = callback;
        Completed = completed;
        if (ThrowAfterRegistering) throw new InvalidOperationException("registered boom");
        if (Mode != PilotHostMode.Synchronous) return;

        callback();
        if (RepeatCallback) callback();
        if (ThrowAfterSynchronousCallback) throw new InvalidOperationException("late boom");
        completed();
    }

    public bool TryOpenTarget(long documentNativeId, out IPushBackPilotHostTarget? target, out string reason)
    {
        Opens++;
        _events.Add("TryOpenTarget");
        target = null;
        reason = "closed";
        if (ThrowOnOpen) throw new InvalidOperationException("open boom");
        if (ActiveNativeId != documentNativeId) return false;
        target = documentNativeId == TargetNativeId ? Target : RecycledTarget;
        return target != null;
    }

    /// <summary>The deferred callback, with the events that H0 measured before and after it.</summary>
    public void RunCallback()
    {
        Assert.True(Callback != null, "the executor was never asked for the command context: there is no callback to run");
        BeforeCallback?.Invoke();
        Callback!();
        if (RepeatCallback) Callback!();
        AfterCallback?.Invoke();
    }

    public void Complete()
    {
        Assert.True(Completed != null, "the executor was never asked for the command context: there is no completion to deliver");
        Completed!();
    }

    /// <summary>The whole deferred sequence: the events before, the callback, the events after and the completion.</summary>
    public void RunDeferred()
    {
        RunCallback();
        Complete();
    }
}

/// <summary>
/// The executor of the update that the host core receives: it counts the requests, hands the run to the real runner over the primitives of the
/// test, and can answer a failure of its own without asking anything.
/// </summary>
internal sealed class PilotUpdateHostExecutorDouble : IPushBackPilotUpdateHost
{
    private readonly PilotHostPrimitivesDouble _primitives;

    public PilotUpdateHostExecutorDouble(PilotHostPrimitivesDouble primitives, List<(string Context, Exception Error)> logged)
    {
        _primitives = primitives;
        Logged = logged;
    }

    public List<(string Context, Exception Error)> Logged { get; }

    public int TryRequests { get; private set; }

    public IPushBackPilotHostRun? LastRun { get; private set; }

    public long LastDocumentNativeId { get; private set; }

    /// <summary>When set, the request fails at once with this cause and nothing is asked of the primitives.</summary>
    public string? RefuseWith { get; set; }

    public bool Throw { get; set; }

    /// <summary>The request runs through the real runner and THEN the executor throws: the callback already began (M-06 (b)).</summary>
    public bool ThrowAfterRun { get; set; }

    public bool TryRequest(long documentNativeId, IPushBackPilotHostRun run, out string failure)
    {
        TryRequests++;
        LastRun = run;
        LastDocumentNativeId = documentNativeId;
        if (Throw) throw new InvalidOperationException("executor boom");
        if (RefuseWith != null)
        {
            failure = RefuseWith;
            return false;
        }

        var requested = PushBackPilotHostRunner.Request(documentNativeId, run, _primitives, (context, error) => Logged.Add((context, error)), out failure);
        if (ThrowAfterRun) throw new InvalidOperationException("executor boom after the run");
        return requested;
    }
}

/// <summary>
/// The recorder of the run, for the tests of the runner: it logs the five operations in order and can be told to refuse the admission or to
/// throw at any of them. It is a total implementation only when the test says so: the runner has to cope with one that is not.
/// </summary>
internal sealed class PilotHostRunRecorder : IPushBackPilotHostRun
{
    private readonly List<string> _events;

    public PilotHostRunRecorder(List<string> events)
    {
        _events = events;
    }

    public bool Admits { get; set; } = true;

    public string AdmitReason { get; set; } = "refused";

    public HashSet<string> ThrowAt { get; } = new(StringComparer.Ordinal);

    public int AdmitCalls { get; private set; }

    public int Begins { get; private set; }

    public int Executes { get; private set; }

    public int Completions { get; private set; }

    public List<(PushBackPilotHostFailure Kind, string Cause)> NotRuns { get; } = new();

    public IPushBackPilotUpdatePort? Port { get; private set; }

    /// <summary>Runs inside Execute: how a test sees the order of the steps from the point of view of the run.</summary>
    public Action? OnExecute { get; set; }

    public bool Admit(out string reason)
    {
        AdmitCalls++;
        _events.Add("Admit");
        reason = AdmitReason;
        if (ThrowAt.Contains("Admit")) throw new InvalidOperationException("admit boom");
        return Admits;
    }

    public void Begin()
    {
        Begins++;
        _events.Add("Begin");
        if (ThrowAt.Contains("Begin")) throw new InvalidOperationException("begin boom");
    }

    public void Execute(IPushBackPilotUpdatePort port)
    {
        Executes++;
        Port = port;
        _events.Add("Execute");
        OnExecute?.Invoke();
        if (ThrowAt.Contains("Execute")) throw new InvalidOperationException("execute boom");
    }

    public void NotRun(PushBackPilotHostFailure kind, string cause)
    {
        NotRuns.Add((kind, cause));
        _events.Add("NotRun:" + kind);
        if (ThrowAt.Contains("NotRun")) throw new InvalidOperationException("notrun boom");
    }

    public void Completed()
    {
        Completions++;
        _events.Add("Completed");
        if (ThrowAt.Contains("Completed")) throw new InvalidOperationException("completed boom");
    }

    /// <summary>The number of terminal calls: the one of Execute and the ones of NotRun.</summary>
    public int Terminals => Executes + NotRuns.Count;
}

/// <summary>The common arrangement of the tests of the runner and of the request: the events, the writer of T4B and the three doubles.</summary>
internal sealed class PilotHostKit
{
    public const long DocumentId = 100;

    public PilotHostKit(IPushBackPilotUpdatePort? writer = null)
    {
        Events = new List<string>();
        Target = new PilotHostTargetDouble(Events, writer ?? new PilotUpdateScopeDouble());
        Primitives = new PilotHostPrimitivesDouble(Events)
        {
            Target = Target,
            TargetNativeId = DocumentId,
            ActiveNativeId = DocumentId,
        };
        Logged = new List<(string Context, Exception Error)>();
        Executor = new PilotUpdateHostExecutorDouble(Primitives, Logged);
        Run = new PilotHostRunRecorder(Events);
    }

    public List<string> Events { get; }

    public PilotHostTargetDouble Target { get; }

    public PilotHostPrimitivesDouble Primitives { get; }

    public PilotUpdateHostExecutorDouble Executor { get; }

    public PilotHostRunRecorder Run { get; }

    public List<(string Context, Exception Error)> Logged { get; }

    /// <summary>The request of the recorder through the real runner.</summary>
    public bool Request(out string failure, long documentId = DocumentId) =>
        PushBackPilotHostRunner.Request(documentId, Run, Primitives, (context, error) => Logged.Add((context, error)), out failure);

    public int Count(string prefix) => Events.Count(e => e.StartsWith(prefix, StringComparison.Ordinal));
}
