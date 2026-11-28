#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F2-T2A-NAVIGATION-HOST: INV-F2T2A-08 (C-F2T2-06, INV-03, D-07). The echo of the navigation is recognized by CONTENT: the
// set of handles RackCad wrote against the set of handles the reader read on the normal drain. No token of the next event, no
// flag, no time window and no timer. An echo changes no context, no rack and no tab, and never calls a selection or view effect.
public class WorkspaceNavigationEchoTests
{
    private static readonly SessionId S1 = new(1);
    private static readonly SessionId S2 = new(2);

    // ------------------------------------------------------------------ the memory by content

    [Fact]
    public void AnEqualSetOfHandlesIsTheEchoWhateverTheOrderOrTheDuplicates()
    {
        var echo = new NavigationEcho();
        echo.Expect(S1, new long[] { 0x103, 0x101, 0x102 });

        echo.Observe(S1, new long[] { 0x101, 0x102, 0x103, 0x103 });

        Assert.True(echo.IsEcho(S1));
    }

    [Fact]
    public void ADifferentSetIsNotAnEchoAndTheFirstDifferentSelectionResumesTheNormalFlow()
    {
        var echo = new NavigationEcho();
        echo.Expect(S1, new long[] { 0x101, 0x102 });
        echo.Observe(S1, new long[] { 0x101, 0x102 });
        Assert.True(echo.IsEcho(S1));

        echo.Observe(S1, new long[] { 0x101, 0x102, 0x103 });
        Assert.False(echo.IsEcho(S1));

        // The expectation is gone: the very same set as before is a user selection now.
        Assert.False(echo.HasExpectation(S1));
        echo.Observe(S1, new long[] { 0x101, 0x102 });
        Assert.False(echo.IsEcho(S1));
    }

    [Fact]
    public void ASubsetOrASupersetOfWhatRackCadWroteIsNeverTheEcho()
    {
        var echo = new NavigationEcho();
        echo.Expect(S1, new long[] { 1, 2, 3 });
        echo.Observe(S1, new long[] { 1, 2, 3 });
        Assert.True(echo.IsEcho(S1));

        echo.Observe(S1, new long[] { 1, 2 });
        Assert.False(echo.IsEcho(S1));

        echo.Expect(S1, new long[] { 1, 2, 3 });
        echo.Observe(S1, new long[] { 1, 2, 3, 4 });
        Assert.False(echo.IsEcho(S1));
    }

    [Fact]
    public void EachSessionKeepsItsOwnExpectationAndItsOwnObservation()
    {
        var echo = new NavigationEcho();
        echo.Expect(S1, new long[] { 1, 2 });
        echo.Expect(S2, new long[] { 9 });

        echo.Observe(S2, new long[] { 9 });
        Assert.True(echo.IsEcho(S2));
        echo.Observe(S1, new long[] { 9 });
        Assert.False(echo.IsEcho(S1));
        Assert.False(echo.HasExpectation(S1));
        Assert.True(echo.HasExpectation(S2));
    }

    [Fact]
    public void AnObservationIsConsumedByTheQuestionAndAnEmptyExpectationExpectsNothing()
    {
        var echo = new NavigationEcho();
        echo.Expect(S1, new long[] { 1 });
        echo.Observe(S1, new long[] { 1 });
        Assert.True(echo.IsEcho(S1));
        Assert.False(echo.IsEcho(S1));

        echo.Expect(S1, Array.Empty<long>());
        Assert.False(echo.HasExpectation(S1));
    }

    [Fact]
    public void AnEndedSessionLosesItsStateAndIsNeverRevivedByALateWriteOrRead()
    {
        var echo = new NavigationEcho();
        echo.Expect(S1, new long[] { 1 });
        echo.Observe(S1, new long[] { 1 });

        echo.End(S1);

        Assert.True(echo.IsEnded(S1));
        Assert.False(echo.HasExpectation(S1));
        Assert.DoesNotContain(S1, echo.TrackedSessions);
        echo.Expect(S1, new long[] { 1 });
        echo.Observe(S1, new long[] { 1 });
        Assert.False(echo.HasExpectation(S1));
        Assert.False(echo.IsEcho(S1));
    }

    [Fact]
    public void ThePurgeEndsOnlyTheSessionsTheAuthorityNoLongerFindsAlive()
    {
        var echo = new NavigationEcho();
        echo.Expect(S1, new long[] { 1 });
        echo.Expect(S2, new long[] { 2 });

        var purged = echo.PurgeDead(id => id == S1);

        Assert.Equal(new[] { S2 }, purged);
        Assert.True(echo.HasExpectation(S1));
        Assert.False(echo.HasExpectation(S2));
        Assert.True(echo.IsEnded(S2));
        Assert.False(echo.IsEnded(S1));
    }

    // ------------------------------------------------------------------ the host core: the echo changes no context

    // A reader double that behaves like the real one: it feeds the shared echo with the handles it read, on every read.
    private sealed class FeedingReader : IWorkspaceSelectionReader
    {
        private readonly NavigationEcho _echo;

        public FeedingReader(NavigationEcho echo)
        {
            _echo = echo;
        }

        public WorkspaceSelectionRead Result { get; set; } = new(Array.Empty<DefinitionFact>(), 0);

        public long[] Handles { get; set; } = Array.Empty<long>();

        public int ReadCalls { get; private set; }

        public WorkspaceSelectionRead Read(SessionId session, long nativeInstanceId)
        {
            ReadCalls++;
            _echo.Observe(session, Handles);
            return Result;
        }

        public void Invalidate(SessionId session)
        {
        }
    }

    private sealed class EchoRig
    {
        public EchoRig()
        {
            Probe.ActiveDocumentNativeId = 100;
            Link = new WorkspaceSimulatedBridgeLink(Bridge, Probe);
            Reader = new FeedingReader(Echo);
            Host = new WorkspaceHostCore(Link, Probe, Reader, new WorkspaceInstrumentation(), null, Echo);
            Host.OpenPanel();
            Idle();
        }

        public WorkspaceBridgeRig Bridge { get; } = new();

        public WorkspaceSimulatedProbe Probe { get; } = new();

        public NavigationEcho Echo { get; } = new();

        public WorkspaceSimulatedBridgeLink Link { get; }

        public FeedingReader Reader { get; }

        public WorkspaceHostCore Host { get; }

        public SessionId Session => Bridge.Core.ActiveSession!.Value;

        public SelectionContext Context => Bridge.SessionOf(Session).Selection;

        public void Idle()
        {
            foreach (var outcome in Bridge.Core.OnIdle()) Host.OnDrained(outcome);
        }

        public void UserSelects(string rackId, params long[] handles)
        {
            Reader.Result = new WorkspaceSelectionRead(new[] { new DefinitionFact(true, rackId) }, 0);
            Reader.Handles = handles;
            Bridge.Core.OnSourceEvent(Session, BridgeSource.ImpliedSelectionChanged);
            Idle();
        }
    }

    [Fact]
    public void TheEchoOfASelectWithAPreviousDifferentContextAdvancesNothingAndCallsNoEffect()
    {
        var rig = new EchoRig();
        var effects = new RecordingNavigationEffects();
        rig.UserSelects("R-PREVIOUS", 0x900);
        Assert.Equal(SelectionContext.One("R-PREVIOUS"), rig.Context);
        var readsBefore = rig.Reader.ReadCalls;

        // RackCad writes the selection of rack R-1 (three references); the reader then reads exactly that set.
        rig.Echo.Expect(rig.Session, new long[] { 0x101, 0x102, 0x103 });
        rig.UserSelects("R-1", 0x103, 0x101, 0x102);

        Assert.Equal(readsBefore + 1, rig.Reader.ReadCalls);
        Assert.Equal(SelectionContext.One("R-PREVIOUS"), rig.Context);
        Assert.Equal(SelectionContext.One("R-PREVIOUS"), rig.Host.GetSnapshot().Selection);
        Assert.Empty(effects.Calls);
    }

    [Fact]
    public void TheFirstDifferentSelectionAfterAnEchoResumesTheNormalSynchronization()
    {
        var rig = new EchoRig();
        rig.UserSelects("R-PREVIOUS", 0x900);
        rig.Echo.Expect(rig.Session, new long[] { 0x101, 0x102 });
        rig.UserSelects("R-1", 0x101, 0x102);
        Assert.Equal(SelectionContext.One("R-PREVIOUS"), rig.Context);

        rig.UserSelects("R-2", 0x201);

        Assert.Equal(SelectionContext.One("R-2"), rig.Context);
    }

    [Fact]
    public void ASelectionThatIsNotWhatRackCadWroteStillAdvancesTheContext()
    {
        var rig = new EchoRig();
        rig.UserSelects("R-PREVIOUS", 0x900);
        rig.Echo.Expect(rig.Session, new long[] { 0x101, 0x102 });

        rig.UserSelects("R-1", 0x101);

        Assert.Equal(SelectionContext.One("R-1"), rig.Context);
    }

    [Fact]
    public void WithoutAnyExpectationTheSelectionIsAlwaysSynchronized()
    {
        var rig = new EchoRig();

        rig.UserSelects("R-1", 0x101);
        rig.UserSelects("R-2", 0x201);

        Assert.Equal(SelectionContext.One("R-2"), rig.Context);
    }

    [Fact]
    public void TheHostCoreStillWorksWithoutAnEchoMemory()
    {
        var bridge = new WorkspaceBridgeRig();
        var probe = new WorkspaceSimulatedProbe { ActiveDocumentNativeId = 100 };
        var reader = new WorkspaceSimulatedReader { Result = new WorkspaceSelectionRead(new[] { new DefinitionFact(true, "R-1") }, 0) };
        var host = new WorkspaceHostCore(new WorkspaceSimulatedBridgeLink(bridge, probe), probe, reader, new WorkspaceInstrumentation());
        host.OpenPanel();
        foreach (var outcome in bridge.Core.OnIdle()) host.OnDrained(outcome);

        var session = bridge.Core.ActiveSession!.Value;
        bridge.Core.OnSourceEvent(session, BridgeSource.ImpliedSelectionChanged);
        foreach (var outcome in bridge.Core.OnIdle()) host.OnDrained(outcome);

        Assert.Equal(SelectionContext.One("R-1"), bridge.SessionOf(session).Selection);
    }

    [Fact]
    public void ADrainPurgesTheEchoStateOfASessionThatNoLongerLives()
    {
        var rig = new EchoRig();
        var first = rig.Session;
        rig.Echo.Expect(first, new long[] { 0x101 });
        rig.Bridge.Core.OnDocumentToBeDestroyed(100);
        rig.Probe.ActiveDocumentNativeId = 200;
        var second = rig.Bridge.Open(200);

        rig.Bridge.Core.OnSourceEvent(second, BridgeSource.ImpliedSelectionChanged);
        rig.Idle();

        Assert.True(rig.Echo.IsEnded(first));
        Assert.False(rig.Echo.HasExpectation(first));
    }

    // ------------------------------------------------------------------ the source: content, never a token, a flag or time

    [Fact]
    public void TheEchoIsPureAndCarriesNoTokenFlagWindowOrTimer()
    {
        var code = Src.Code("src/RackCad.Application/Workspace/NavigationEcho.cs");

        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"ignoreNext|IgnoreNext|Stopwatch|DateTime|TimeSpan|\bTimer\b|Environment\.TickCount|\bTask\b|Thread"), code);
        Assert.Contains("SetEquals(", code);
        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"Autodesk\.|System\.IO|Transaction"), code);
        Assert.StartsWith("#nullable enable", Src.Text("src/RackCad.Application/Workspace/NavigationEcho.cs").TrimStart('﻿'));
    }

    [Fact]
    public void TheEchoIsSharedByTheReaderAndTheHostCoreAndFlowsThroughTheControllerOnly()
    {
        var controller = Src.Code("src/RackCad.Plugin/Workspace/WorkspaceController.cs");
        var reader = Src.Code("src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs");
        var core = Src.Code("src/RackCad.Application/Workspace/WorkspaceHostCore.cs");

        Assert.Equal(1, Src.Count(controller, @"\bnew\s+NavigationEcho\s*\("));
        Assert.Matches(@"\bnew\s+WorkspaceSelectionReader\s*\(\s*_echo\s*\)", controller);
        Assert.Matches(@"LogFault\s*,\s*_echo\s*\)", controller);
        Assert.Contains(".Observe(", reader);
        Assert.Contains("GetObjectIds()", reader);
        Assert.Equal(1, Src.Count(core, @"_echo\.IsEcho\s*\("));

        // The echo is asked after the reader returned and before the mapped context is applied.
        var drain = Src.BodyOf(core, @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        var read = drain.IndexOf("_reader.Read(", StringComparison.Ordinal);
        var echo = drain.IndexOf("_echo.IsEcho(", StringComparison.Ordinal);
        var apply = drain.IndexOf(".ApplyContext(", StringComparison.Ordinal);
        Assert.True(read >= 0 && echo > read && apply > echo, "the echo must be asked between the read and ApplyContext");
    }

    [Fact]
    public void NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles()
    {
        var read = Src.Code("src/RackCad.Application/Workspace/WorkspaceSelectionRead.cs");
        var session = Src.Code("src/RackCad.Application/Workspace/WorkspaceSession.cs");

        // The selection read carries no echo and no handle transport.
        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"Echo|Handles|Handle\b"), read);

        // The session carries no echo state, property, field or transport of handles. The one existing symbol that names
        // navigation, the DrainOperation of the cache invalidation (F2-T1), is not an echo transport and is the only allowed one.
        var withoutTheCacheOperation = session.Replace("DrainOperation.InvalidateNavigationCaches", string.Empty);
        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"Echo|Observe\s*\(|Expect\s*\(|[Hh]andle|Navigation"), withoutTheCacheOperation);

        // The approved mechanism is the shared NavigationEcho of the exact authorized reader and the host core, not the session.
        Assert.DoesNotContain("NavigationEcho", session);
        Assert.Contains("NavigationEcho", Src.Code("src/RackCad.Application/Workspace/WorkspaceHostCore.cs"));
        Assert.Contains("NavigationEcho", Src.Code("src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs"));
    }

    [Fact]
    public void NoEventNorContextChangeWritesTheSelectionOrTheViewAndTheEchoNamesNoEffect()
    {
        var echo = Src.Code("src/RackCad.Application/Workspace/NavigationEcho.cs");
        var core = Src.Code("src/RackCad.Application/Workspace/WorkspaceHostCore.cs");

        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"INavigationEffects|SelectReferences|ViewReference|CenterOnReference|NavigationActionPolicy"), echo);
        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"INavigationEffects|SelectReferences|ViewReference|CenterOnReference|NavigationActionPolicy"), core);
    }

    [Fact]
    public void TheCountOfTrackedSessionsGrowsOnlyWithObservationsAndExpectations()
    {
        var echo = new NavigationEcho();
        Assert.Empty(echo.TrackedSessions);

        echo.Observe(S1, new long[] { 1 });
        echo.Expect(S2, new long[] { 2 });

        Assert.Equal(new[] { S1, S2 }.OrderBy(s => s.Value), echo.TrackedSessions.OrderBy(s => s.Value));
    }
}
