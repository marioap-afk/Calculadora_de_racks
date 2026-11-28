#nullable enable
using System;
using System.Linq;
using System.Reflection;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T2-BRIDGE: D-04 / INV-F1-T2-03, 06, 10 with a simulated event source.
public class WorkspaceBridgeHintAndDrainTests
{
    private static (WorkspaceBridgeRig Rig, SessionId A, SessionId B) TwoDocuments()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200); // B is the active document
        return (rig, a, b);
    }

    public static TheoryData<BridgeSource> HintSources() => new()
    {
        BridgeSource.ImpliedSelectionChanged,
        BridgeSource.CommandEnded,
        BridgeSource.CommandCancelled,
        BridgeSource.CommandFailed,
        BridgeSource.ObjectAppended,
        BridgeSource.ObjectErased,
        BridgeSource.ObjectModified,
        BridgeSource.ObjectUnappended,
        BridgeSource.ObjectReappended,
    };

    [Theory]
    [MemberData(nameof(HintSources))]
    public void AnOrdinaryHandlerOnlyEnqueuesAHintOnTheSessionNamedByTheNotification(BridgeSource source)
    {
        var (rig, a, b) = TwoDocuments();
        var attachCalls = rig.Source.AttachCalls;
        var detachCalls = rig.Source.DetachCalls;

        rig.Core.OnSourceEvent(a, source);

        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(0, rig.SessionOf(b).PendingHintCount);
        Assert.Equal(SelectionContext.None, rig.SessionOf(a).Selection);
        // The only hookup change a hint may cause is the one idle hookup; no other call reaches the host.
        Assert.Equal(attachCalls + 1, rig.Source.AttachCalls);
        Assert.Equal(detachCalls, rig.Source.DetachCalls);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));
        Assert.Empty(rig.Faults);
    }

    [Theory]
    [InlineData(BridgeSource.CommandWillStart)]
    [InlineData(BridgeSource.EnteringQuiescentState)]
    [InlineData(BridgeSource.BeginDocumentClose)]
    [InlineData(BridgeSource.DocumentActivated)]
    [InlineData(BridgeSource.Idle)]
    public void SourcesThatAreNotHintsQueueNothingAndNeverAttachIdle(BridgeSource source)
    {
        var (rig, a, _) = TwoDocuments();
        var attachCalls = rig.Source.AttachCalls;

        rig.Core.OnSourceEvent(a, source);

        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(attachCalls, rig.Source.AttachCalls);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
    }

    [Fact]
    public void ANotificationNeverDrainsByItself()
    {
        var (rig, _, b) = TwoDocuments();

        for (var i = 0; i < 50; i++) rig.Core.OnSourceEvent(b, BridgeSource.ImpliedSelectionChanged);
        rig.Core.OnSourceEvent(b, BridgeSource.EnteringQuiescentState);

        // The session is quiescent and ready, yet nothing drained: the drain is deferred to the idle pass.
        Assert.True(rig.SessionOf(b).PendingHintCount > 0);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));
    }

    [Fact]
    public void NHintsCoalesceIntoASingleDrainOfTheActiveSession()
    {
        var (rig, _, b) = TwoDocuments();
        for (var i = 0; i < 100; i++) rig.Core.OnSourceEvent(b, BridgeSource.ImpliedSelectionChanged);

        var first = rig.Core.OnIdle();
        var second = rig.Core.OnIdle();

        var outcome = Assert.Single(first);
        Assert.Equal(b, outcome.Session);
        Assert.True(outcome.Result.Drained);
        Assert.Equal(
            new[] { DrainOperation.ReadImplicitSelection, DrainOperation.RecomputeSelectionContext },
            outcome.Result.Operations);
        Assert.Empty(second);
        Assert.Equal(0, rig.SessionOf(b).PendingHintCount);
    }

    [Fact]
    public void ObjectChangesDrainOnlyTheCacheInvalidationAndTheStaleMarking()
    {
        var (rig, _, b) = TwoDocuments();
        rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified);
        rig.Core.OnSourceEvent(b, BridgeSource.CommandEnded);

        var outcome = Assert.Single(rig.Core.OnIdle());

        Assert.Equal(
            new[] { DrainOperation.InvalidateNavigationCaches, DrainOperation.MarkDraftsPossiblyStale },
            outcome.Result.Operations);
    }

    [Fact]
    public void WithoutPendingHintsAnIdlePassDoesNothing()
    {
        var (rig, _, _) = TwoDocuments();

        for (var i = 0; i < 5; i++) Assert.Empty(rig.Core.OnIdle());

        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
    }

    [Fact]
    public void NothingDrainsWhileACommandIsRunningAndTheHintsWaitForQuiescence()
    {
        var (rig, _, b) = TwoDocuments();

        rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart);
        rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified);
        rig.Core.OnSourceEvent(b, BridgeSource.ImpliedSelectionChanged);

        Assert.Empty(rig.Core.OnIdle());
        Assert.Equal(2, rig.SessionOf(b).PendingHintCount);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));

        rig.Core.OnSourceEvent(b, BridgeSource.CommandEnded);
        Assert.Empty(rig.Core.OnIdle()); // the command ended but the editor is not at rest yet

        rig.Core.OnSourceEvent(b, BridgeSource.EnteringQuiescentState);
        var outcome = Assert.Single(rig.Core.OnIdle());
        Assert.Equal(b, outcome.Session);
        Assert.Equal(0, rig.SessionOf(b).PendingHintCount);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
    }

    [Fact]
    public void ASessionActivatedWhileNotAtRestWaitsForQuiescence()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100, quiescent: false);
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectAppended);

        Assert.Empty(rig.Core.OnIdle());

        rig.Core.OnSourceEvent(a, BridgeSource.EnteringQuiescentState);
        Assert.Single(rig.Core.OnIdle());
    }

    [Fact]
    public void NothingDrainsWhileARackCadModalIsActiveButHintsKeepQueueing()
    {
        var (rig, _, b) = TwoDocuments();
        rig.Core.SetModalRackCadActive(true);

        // Hints may arrive at any time, also during a modal: correctness never depends on silence.
        rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified);
        rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified);
        rig.Core.OnSourceEvent(b, BridgeSource.ImpliedSelectionChanged);
        Assert.Empty(rig.Core.OnIdle());
        Assert.Equal(2, rig.SessionOf(b).PendingHintCount);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));

        rig.Core.SetModalRackCadActive(false);
        var outcome = Assert.Single(rig.Core.OnIdle());
        Assert.Equal(b, outcome.Session);
        Assert.Equal(0, rig.SessionOf(b).PendingHintCount);
    }

    [Fact]
    public void AFaultWhileQueueingAHintIsContainedAndDegradesOnlyThatSession()
    {
        var (rig, _, b) = TwoDocuments();
        rig.Core.SetModalRackCadActive(true);
        rig.Source.FailAttach = x => x.Source == BridgeSource.Idle;

        var thrown = Record.Exception(() => rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified));

        Assert.Null(thrown);
        Assert.True(rig.Core.IsDegraded(b));
    }

    [Fact]
    public void WithTheDocumentHiddenOrThePanelHiddenNothingDrains()
    {
        var (rig, _, b) = TwoDocuments();
        rig.Core.SetPanelVisible(false);
        rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified);

        Assert.Empty(rig.Core.OnIdle());
        Assert.Equal(1, rig.SessionOf(b).PendingHintCount);

        rig.Core.SetPanelVisible(true);
        Assert.Single(rig.Core.OnIdle());
    }

    [Fact]
    public void OnlyTheActiveDocumentDrainsAndTheOthersKeepTheirHintsUntilActivated()
    {
        var (rig, a, b) = TwoDocuments();
        rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified);
        Assert.Empty(rig.Core.OnIdle()); // B is active and has nothing; A must not drain
        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));

        rig.Core.OnSourceEvent(b, BridgeSource.ObjectModified);
        var outcomeB = Assert.Single(rig.Core.OnIdle());
        Assert.Equal(b, outcomeB.Session);
        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);

        rig.Open(100);
        var outcomeA = Assert.Single(rig.Core.OnIdle());
        Assert.Equal(a, outcomeA.Session);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
    }

    [Fact]
    public void ANotificationForARetiredSessionIsDroppedWithoutEffect()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.OnDocumentToBeDestroyed(100);
        var attachCalls = rig.Source.AttachCalls;

        var thrown = Record.Exception(() => rig.Core.OnSourceEvent(a, BridgeSource.ObjectModified));

        Assert.Null(thrown);
        Assert.Equal(attachCalls, rig.Source.AttachCalls);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void NoEventAndNoDrainWritesTheSelectionContextOfAnySession()
    {
        var (rig, a, b) = TwoDocuments();
        var contextA = SelectionContext.One("R-A");
        var contextB = SelectionContext.Selection(2, 1);
        Assert.True(rig.SessionOf(a).ApplyContext(contextA));
        Assert.True(rig.SessionOf(b).ApplyContext(contextB));

        foreach (var source in BridgeSources.PerSession)
        {
            rig.Core.OnSourceEvent(a, source);
            rig.Core.OnSourceEvent(b, source);
        }

        rig.Core.OnIdle();
        rig.Core.OnDocumentActivated(100, true);
        rig.Core.OnIdle();

        Assert.Equal(contextA, rig.SessionOf(a).Selection);
        Assert.Equal(contextB, rig.SessionOf(b).Selection);
    }

    [Fact]
    public void TheBridgeSurfaceHasNoWayToWriteSelectionOrViewOrAnythingAuthored()
    {
        var core = typeof(EventBridgeCore);
        foreach (var method in core.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.NotEqual(typeof(SelectionContext), method.ReturnType);
            Assert.DoesNotContain(method.GetParameters(), p => p.ParameterType == typeof(SelectionContext));
        }

        // The host seam only moves hookups: two methods, nothing to read or to write.
        var seam = typeof(IHostEventSource).GetMethods().Select(m => m.Name).OrderBy(n => n).ToArray();
        Assert.Equal(new[] { "Attach", "Detach" }, seam);
        Assert.All(typeof(IHostEventSource).GetMethods(), m => Assert.Equal(typeof(void), m.ReturnType));
    }

    [Fact]
    public void TheCoreRequiresNoSpecificHostAndTakesOnlyAHookupSeam()
    {
        var constructor = Assert.Single(typeof(EventBridgeCore).GetConstructors());
        var types = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
        Assert.Equal(typeof(IHostEventSource), types[0]);
        Assert.Equal(typeof(Action<string, Exception>), types[1]);
        Assert.Equal(2, types.Length);
    }
}
