#nullable enable
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 T6 st (the Owner on 2026-11-27, «Afinarlo»: only the commands that change the drawing make a reading possibly out of date): the session counts,
// in memory, every change of the database of its drawing that the bridge gives it (own or not), and the snapshot of the host carries the count of the
// active session. A command, a selection or a bookkeeping event of the associative framework is not a change; nothing else of the session changes.
public class WorkspaceDrawingChangeCountTests
{
    [Fact]
    public void EveryObjectEventOwnOrForeignCountsAndNothingElseDoes()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var session = rig.Open(1);
        var live = rig.SessionOf(session);
        Assert.Equal(0, live.DrawingChangeCount);

        rig.Core.OnSourceEvent(session, BridgeSource.CommandWillStart);
        rig.Core.OnSourceEvent(session, BridgeSource.ImpliedSelectionChanged);
        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        rig.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);
        Assert.Equal(0, live.DrawingChangeCount);

        foreach (var source in WorkspaceBridgeRig.ObjectSources)
            rig.Core.OnObjectEvent(session, source, ObjectEventSubject.AssociativeBookkeeping);
        Assert.Equal(0, live.DrawingChangeCount);

        var n = 0;
        foreach (var source in WorkspaceBridgeRig.ObjectSources)
        {
            rig.Core.OnObjectEvent(session, source, ObjectEventSubject.Drawing);
            Assert.Equal(++n, live.DrawingChangeCount);
        }

        // The own mutation of the update of the pilot changes the drawing too: it counts, and its hints stay apart as before.
        rig.Core.OpenOwnMutation(session);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, ObjectEventSubject.Drawing);
        Assert.Equal(n + 1, live.DrawingChangeCount);

        // A drain does not reset it: it is a count, not a pending hint.
        rig.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);
        rig.Core.OnIdle();
        Assert.Equal(n + 1, live.DrawingChangeCount);
    }

    [Fact]
    public void TheSnapshotCarriesTheCountOfTheActiveSessionAndZeroWithoutADocument()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        Assert.Equal(0, rig.Host.GetSnapshot().DrawingChangeCount);

        var first = rig.ActiveSession;
        rig.Bridge.Core.OnObjectEvent(first, BridgeSource.ObjectErased, ObjectEventSubject.Drawing);
        rig.Bridge.Core.OnObjectEvent(first, BridgeSource.ObjectAppended, ObjectEventSubject.Drawing);
        Assert.Equal(2, rig.Host.GetSnapshot().DrawingChangeCount);

        // Another drawing has its own count.
        rig.Activate(200);
        Assert.Equal(0, rig.Host.GetSnapshot().DrawingChangeCount);
        rig.Activate(100);
        Assert.Equal(2, rig.Host.GetSnapshot().DrawingChangeCount);

        rig.CloseActive(100);
        Assert.Equal(0, rig.Host.GetSnapshot().DrawingChangeCount);
    }

    // Review of 5114345e, M-1: the snapshot carries the commands that started in the active session, as the bridge counts them for the update of
    // the pilot (ForeignCommandStartsOf); the RackCad window takes a command as the one of its door only when exactly one started.
    [Fact]
    public void TheSnapshotCarriesTheCommandStartsOfTheActiveSessionAndZeroWithoutADocument()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        var first = rig.ActiveSession;
        Assert.Equal(0, rig.Host.GetSnapshot().CommandStartCount);

        rig.Bridge.Core.NoteForeignCommandStart(first);
        rig.Bridge.Core.NoteForeignCommandStart(first);
        Assert.Equal(2, rig.Host.GetSnapshot().CommandStartCount);

        rig.Activate(200);
        Assert.Equal(0, rig.Host.GetSnapshot().CommandStartCount);
        rig.Activate(100);
        Assert.Equal(2, rig.Host.GetSnapshot().CommandStartCount);

        rig.CloseActive(100);
        Assert.Equal(0, rig.Host.GetSnapshot().CommandStartCount);
    }
}
