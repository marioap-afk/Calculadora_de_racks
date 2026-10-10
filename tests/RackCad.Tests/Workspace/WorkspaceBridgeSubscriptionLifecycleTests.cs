#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T2-BRIDGE: D-04 / INV-F1-T2-01, 09, 10 with a simulated event source.
public class WorkspaceBridgeSubscriptionLifecycleTests
{
    private static readonly string[] FrozenSources =
    {
        "DocumentCreated", "DocumentActivated", "DocumentToBeDestroyed",
        "BeginDocumentClose", "ImpliedSelectionChanged", "CommandWillStart", "CommandEnded", "CommandCancelled",
        "CommandFailed", "EnteringQuiescentState",
        "ObjectAppended", "ObjectErased", "ObjectModified", "ObjectUnappended", "ObjectReappended",
        "Idle",
    };

    [Fact]
    public void TheGovernedSourcesAreExactlyTheFrozenSetOfD04()
    {
        var names = BridgeSources.All.Select(s => s.ToString()).OrderBy(n => n).ToArray();
        Assert.Equal(FrozenSources.OrderBy(n => n).ToArray(), names);
        Assert.Equal(WorkspaceBridgeRig.CollectionSources, BridgeSources.DocumentCollection.Count);
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, BridgeSources.PerSession.Count);
        Assert.Equal(BridgeSource.BeginDocumentClose, BridgeSources.PerSession[0]);
        Assert.False(BridgeSources.IsSynchronization(BridgeSource.BeginDocumentClose));
        Assert.All(BridgeSources.PerSession.Skip(1), s => Assert.True(BridgeSources.IsSynchronization(s)));
    }

    [Fact]
    public void NothingIsAttachedBeforeTheFirstPanelOpen()
    {
        var rig = new WorkspaceBridgeRig();

        // Loading the plugin or receiving notifications before RACKPANEL must not hook anything.
        rig.Core.OnDocumentCreated(1);
        rig.Core.OnDocumentActivated(1, true);
        rig.Core.OnIdle();

        Assert.False(rig.Core.IsStarted);
        Assert.Equal(0, rig.Source.Total);
        Assert.Equal(0, rig.Source.AttachCalls);
        Assert.Equal(0, rig.Core.LiveSessionCount);
    }

    [Fact]
    public void TheFirstPanelOpenAttachesTheCollectionSourcesOnceAndNothingElse()
    {
        var rig = new WorkspaceBridgeRig();

        rig.Core.Start();

        Assert.True(rig.Core.IsStarted);
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(0), rig.Source.Total);
        foreach (var source in BridgeSources.DocumentCollection) Assert.Equal(1, rig.Source.Count(source));
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
        Assert.Equal(0, rig.Core.LiveSessionCount);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void ADocumentCreatedButNotActivatedGetsNoSessionAndNoHookups()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();

        rig.Core.OnDocumentCreated(5);

        Assert.Equal(0, rig.Core.LiveSessionCount);
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(0), rig.Source.Total);
    }

    [Fact]
    public void ActivatingADocumentAttachesExactlyOneHookupPerPerSessionSource()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();

        var a = rig.Open(100);

        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(1), rig.Source.Total);
        foreach (var source in BridgeSources.PerSession) Assert.Equal(1, rig.Source.CountFor(a, source));
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(a));
        Assert.True(rig.Source.AllHooksAreSingle);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void RepeatedPanelOpensNeverMultiplyTheHookups()
    {
        var rig = new WorkspaceBridgeRig();

        for (var i = 0; i < 5; i++)
        {
            rig.Core.Start();
            rig.Open(100);
            rig.Open(200);
        }

        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(2), rig.Source.Total);
        Assert.True(rig.Source.AllHooksAreSingle);
        Assert.Empty(rig.Source.Violations);
        Assert.Equal(2, rig.Core.LiveSessionCount);
    }

    [Fact]
    public void ActivatingAThenBThenAKeepsExactlyOneSetOfHookupsPerDocument()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();

        var a = rig.Open(100);
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(1), rig.Source.Total);

        var b = rig.Open(200);
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(2), rig.Source.Total);

        var aAgain = rig.Open(100);
        Assert.Equal(a, aAgain);
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(2), rig.Source.Total);
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(a));
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(b));
        Assert.Equal<SessionId?>(aAgain, rig.Core.ActiveSession);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void HidingAndShowingThePanelNeverChangesTheHookups()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        rig.Open(100);
        rig.Open(200);
        var expected = WorkspaceBridgeRig.ExpectedHookups(2);

        for (var i = 0; i < 4; i++)
        {
            rig.Core.SetPanelVisible(false);
            Assert.Equal(expected, rig.Source.Total);
            rig.Core.SetPanelVisible(true);
            Assert.Equal(expected, rig.Source.Total);
        }

        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void DestroyingADocumentRetiresItsSessionAndEveryHookupOfIt()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);

        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal(0, rig.Source.CountForSession(a));
        Assert.Equal(0, rig.Source.CountFor(a, BridgeSource.BeginDocumentClose));
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(b));
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(1), rig.Source.Total);
        Assert.False(rig.Core.TryGetSession(a, out _));
        Assert.Equal(1, rig.Core.LiveSessionCount);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void DestroyingTheLastDocumentLeavesOnlyTheCollectionSources()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        rig.Open(100);

        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(0), rig.Source.Total);
        Assert.Null(rig.Core.ActiveSession);
        Assert.Equal(0, rig.Core.LiveSessionCount);
    }

    [Fact]
    public void DestroyingAnUnknownDocumentChangesNothing()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        rig.Open(100);
        var before = rig.Source.Total;

        rig.Core.OnDocumentToBeDestroyed(999);

        Assert.Equal(before, rig.Source.Total);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void ARecycledNativeIdentifierGetsAFreshSessionAndNeverPairsWithTheRetiredOne()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var old = rig.Open(100);
        rig.Core.OnSourceEvent(old, BridgeSource.ObjectModified);
        rig.Core.OnDocumentToBeDestroyed(100);

        var fresh = rig.Open(100);

        Assert.NotEqual(old, fresh);
        Assert.Equal(0, rig.SessionOf(fresh).PendingHintCount);
        Assert.False(rig.Core.TryGetSession(old, out _));
        Assert.Equal(0, rig.Source.CountForSession(old));
        Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(fresh));

        // A late notification that still carries the retired identity is dropped: it never reaches the new session.
        rig.Core.OnSourceEvent(old, BridgeSource.ObjectModified);
        Assert.Equal(0, rig.SessionOf(fresh).PendingHintCount);
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(1), rig.Source.Total);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void IdleIsAttachedOnlyWhileHintsArePending()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
        Assert.False(rig.Core.IsIdleAttached);

        for (var cycle = 0; cycle < 3; cycle++)
        {
            rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);
            Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));
            Assert.True(rig.Core.IsIdleAttached);

            // More hints never add a second Idle hookup.
            for (var i = 0; i < 10; i++) rig.Core.OnSourceEvent(a, BridgeSource.ImpliedSelectionChanged);
            Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));

            var drained = rig.Core.OnIdle();
            Assert.Single(drained);
            Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
            Assert.False(rig.Core.IsIdleAttached);
        }

        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(1), rig.Source.Total);
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void IdleStaysAttachedWhileAnotherSessionStillHasPendingHints()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);
        rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified);

        // B is active and drains; A still has a pending hint.
        rig.Core.OnIdle();
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));

        rig.Core.OnDocumentToBeDestroyed(100);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(1), rig.Source.Total);
    }

    [Fact]
    public void DestroyingTheOnlyDocumentWithPendingHintsAlsoReleasesIdle()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectErased);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));

        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(0), rig.Source.Total);
    }

    [Fact]
    public void StopReleasesEveryHookupAndReturnsToTheInitialState()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        rig.Open(200);
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);

        rig.Core.Stop();

        Assert.Equal(0, rig.Source.Total);
        Assert.False(rig.Core.IsStarted);
        Assert.Equal(0, rig.Core.LiveSessionCount);
        Assert.Empty(rig.Source.Violations);

        // And it can be started again from zero.
        rig.Core.Start();
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(0), rig.Source.Total);
    }

    [Fact]
    public void TheHookupsAreAlwaysTheExactFunctionOfTheLiveDocumentsAndPendingHints()
    {
        var rig = new WorkspaceBridgeRig();
        var random = new Random(64);
        var live = new HashSet<long>();
        var pending = new HashSet<long>();
        var retired = new List<SessionId>();
        long? active = null;
        var visible = true;
        rig.Core.Start();

        for (var step = 0; step < 600; step++)
        {
            var native = random.Next(1, 5);
            switch (random.Next(0, 7))
            {
                case 0:
                case 1:
                    rig.Core.OnDocumentActivated(native, true);
                    live.Add(native);
                    active = native;
                    break;
                case 2:
                    if (live.Contains(native)) retired.Add(rig.IdOf(native));
                    rig.Core.OnDocumentToBeDestroyed(native);
                    live.Remove(native);
                    pending.Remove(native);
                    if (active == native) active = null;
                    break;
                case 3:
                    if (live.Contains(native))
                    {
                        rig.Core.OnSourceEvent(rig.IdOf(native), WorkspaceBridgeRig.ObjectSources[random.Next(WorkspaceBridgeRig.ObjectSources.Length)]);
                        pending.Add(native);
                    }

                    break;
                case 4:
                    rig.Core.OnIdle();
                    if (active is { } current && visible) pending.Remove(current);
                    break;
                case 5:
                    visible = !visible;
                    rig.Core.SetPanelVisible(visible);
                    break;
                default:
                    rig.Core.Start(); // a repeated RACKPANEL shows the panel again
                    visible = true;
                    break;
            }

            Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(live.Count, pending.Count > 0), rig.Source.Total);
            Assert.Equal(pending.Count > 0 ? 1 : 0, rig.Source.Count(BridgeSource.Idle));
            Assert.Empty(rig.Source.Violations);
            Assert.True(rig.Source.AllHooksAreSingle);
            foreach (var id in retired) Assert.Equal(0, rig.Source.CountForSession(id));
            foreach (var n in live) Assert.Equal(WorkspaceBridgeRig.SourcesPerSession, rig.Source.CountForSession(rig.IdOf(n)));
        }
    }
}
