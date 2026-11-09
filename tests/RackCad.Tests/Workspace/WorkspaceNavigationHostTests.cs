#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F2-T2A-NAVIGATION-HOST: the navigation port and its immutable result (INV-F2T2A-07), the pure runner that rejects by
// session and by revalidation over a simulated effects port (INV-F2T2A-02), the lazy purge of the sessions that no longer live
// (INV-F2T2A-09) and the source guards of the three Plugin files and of the exact-file exceptions (INV-F2T2A-01, 03, 04, 05, 06,
// 10, 11, 12). The Core test project cannot reference the Plugin (it needs AutoCAD), so the Plugin files are read as text.
public class WorkspaceNavigationHostTests
{
    private const string HostPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs";
    private const string FactsPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs";
    private const string EffectsPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string PortPath = "src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs";

    private static readonly SessionId S1 = new(1);
    private static readonly SessionId S2 = new(2);

    // ------------------------------------------------------------------ the port and its immutable result

    [Fact]
    public void TheResultIsAnImmutableReadonlyRecordStructWithTheFiveStatuses()
    {
        Assert.Equal(
            new[] { "Executed", "Unavailable", "Rejected", "Stale", "Failed" },
            Enum.GetNames(typeof(WorkspaceNavigationStatus)));

        var type = typeof(WorkspaceNavigationResult);
        Assert.True(type.IsValueType);
        Assert.Contains(type.GetCustomAttributes(false), a => a.GetType().Name == "IsReadOnlyAttribute");
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance));
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var setter = property.SetMethod;
            Assert.True(
                setter == null || setter.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.Name == "IsExternalInit"),
                property.Name + " is mutable");
        }

        var executed = WorkspaceNavigationResult.Executed();
        var changed = executed with { RefreshRequired = true };
        Assert.False(executed.RefreshRequired);
        Assert.True(changed.RefreshRequired);
        Assert.Equal(WorkspaceNavigationStatus.Executed, executed.Status);
    }

    [Fact]
    public void EachStatusHasItsFactoryAndOnlyExecutedIsExecuted()
    {
        Assert.Equal(WorkspaceNavigationStatus.Unavailable, WorkspaceNavigationResult.Unavailable("x").Status);
        Assert.Equal(WorkspaceNavigationStatus.Rejected, WorkspaceNavigationResult.Rejected("x").Status);
        Assert.Equal(WorkspaceNavigationStatus.Stale, WorkspaceNavigationResult.Stale("x", true).Status);
        Assert.Equal(WorkspaceNavigationStatus.Failed, WorkspaceNavigationResult.Failed("x").Status);
        Assert.True(WorkspaceNavigationResult.Executed().IsExecuted);
        Assert.False(WorkspaceNavigationResult.Failed("x").IsExecuted);
        Assert.False(WorkspaceNavigationResult.Stale("x", false).IsExecuted);
    }

    [Theory]
    [InlineData(NavigationActionVerdict.Act, WorkspaceNavigationStatus.Executed, false)]
    [InlineData(NavigationActionVerdict.RejectedNoTargets, WorkspaceNavigationStatus.Unavailable, false)]
    [InlineData(NavigationActionVerdict.RejectedSessionMismatch, WorkspaceNavigationStatus.Rejected, false)]
    [InlineData(NavigationActionVerdict.RejectedSessionDestroyed, WorkspaceNavigationStatus.Stale, false)]
    [InlineData(NavigationActionVerdict.RejectedTargetGone, WorkspaceNavigationStatus.Stale, true)]
    [InlineData(NavigationActionVerdict.RejectedNotAttributable, WorkspaceNavigationStatus.Stale, true)]
    public void EachVerdictOfThePolicyHasOneStatusAndTheRefreshOnlyForVanishedTargets(
        NavigationActionVerdict verdict, WorkspaceNavigationStatus status, bool refresh)
    {
        var decision = new NavigationActionDecision(verdict, Array.Empty<long>(), refresh, refresh);

        var result = WorkspaceNavigationResult.From(decision);

        Assert.Equal(status, result.Status);
        Assert.Equal(refresh, result.RefreshRequired);
        Assert.Equal(verdict, result.Verdict);
    }

    [Fact]
    public void ThePortHasTheExactSignatureAndNamesNoAutoCadType()
    {
        var methods = typeof(IWorkspaceNavigationPort).GetMethods();
        var execute = Assert.Single(methods);
        Assert.Equal("ExecuteAsync", execute.Name);
        Assert.Equal(typeof(Task<WorkspaceNavigationResult>), execute.ReturnType);

        var parameters = execute.GetParameters();
        Assert.Equal(
            new[] { "origin", "rackId", "action", "copyIndex", "view", "locateWith" },
            parameters.Select(p => p.Name).ToArray());
        Assert.Equal(
            new[]
            {
                typeof(WorkspaceRequest), typeof(string), typeof(NavigationActionKind), typeof(int),
                typeof(NavigationViewOrder?), typeof(NavigationActionKind),
            },
            parameters.Select(p => p.ParameterType).ToArray());
        Assert.Equal(0, parameters[3].DefaultValue);
        Assert.Null(parameters[4].DefaultValue);
        Assert.Equal((int)NavigationActionKind.View, Convert.ToInt32(parameters[5].DefaultValue));

        var code = Src.Code(PortPath);
        Assert.Matches(@"Task<WorkspaceNavigationResult>\s+ExecuteAsync\s*\(", code);
        Assert.DoesNotMatch(new Regex(@"\bAutodesk\b|\bDocument\b|\bEditor\b|\bDatabase\b|\bObjectId\b|\bEntity\b|\bBlockReference\b|System\.Windows"), code);
    }

    [Fact]
    public void TheHostPortOfF1IsUntouchedAndStillKnowsNothingOfNavigation()
    {
        Assert.DoesNotMatch(new Regex(@"Navigation|Task"), Src.Code("src/RackCad.Application/Workspace/IWorkspaceHostPort.cs"));
        Assert.DoesNotMatch(new Regex(@"Navigation"), Src.Code("src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs"));
    }

    // ------------------------------------------------------------------ the pure runner over a simulated effects port

    private sealed class ShiftingFacts : INavigationFactsPort
    {
        private readonly Func<int, IReadOnlyList<NavigationDefinitionFact>> _byRead;

        public ShiftingFacts(Func<int, IReadOnlyList<NavigationDefinitionFact>> byRead)
        {
            _byRead = byRead;
        }

        public int Reads { get; private set; }

        public IReadOnlyList<NavigationDefinitionFact> ReadDefinitionFacts(SessionId session) => _byRead(++Reads);
    }

    private sealed class ThrowingEffects : INavigationEffects
    {
        public void SelectReferences(SessionId session, IReadOnlyList<long> handles) => throw new InvalidOperationException("boom");

        public void ViewReference(SessionId session, long handle) => throw new InvalidOperationException("boom");

        public void CenterOnReference(SessionId session, long handle) => throw new InvalidOperationException("boom");
    }

    private sealed class Rig
    {
        public Rig(INavigationFactsPort? port = null)
        {
            Facts = new FakeNavigationFacts()
                .With(
                    S1,
                    NavFact.Member(10, "R-1", "frontal", 0, 0x101, 0x102),
                    NavFact.Member(11, "R-1", "lateral", 0, 0x103),
                    NavFact.Member(12, "R-9", "frontal", 0, 0x901))
                .With(S2, NavFact.Member(20, "R-1", "frontal", 0, 0x201));
            Targets = new NavigationTargets(port ?? Facts);
            Policy = new NavigationActionPolicy(Targets);
            Runner = new WorkspaceNavigationRunner(Targets, Policy, Echo, id => Alive.Contains(id));
        }

        public HashSet<SessionId> Alive { get; } = new() { S1, S2 };

        public FakeNavigationFacts Facts { get; }

        public NavigationTargets Targets { get; }

        public NavigationActionPolicy Policy { get; }

        public NavigationEcho Echo { get; } = new();

        public WorkspaceNavigationRunner Runner { get; }

        public RecordingNavigationEffects Effects { get; } = new();

        public WorkspaceNavigationResult Run(
            NavigationActionKind action,
            int copy = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View,
            RequestDecision decision = RequestDecision.Accepted,
            bool modelSpace = true,
            SessionId? session = null,
            string rackId = "R-1",
            INavigationEffects? effects = null) =>
            Runner.Run(
                decision,
                modelSpace,
                new WorkspaceNavigationRequest(new WorkspaceRequest(session ?? S1), rackId, action, copy, view, locateWith),
                effects ?? Effects);
    }

    [Fact]
    public void AnAcceptedSelectActsOnceOnEveryReferenceAndRemembersTheirHandlesAsTheEcho()
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.Select);

        Assert.Equal(WorkspaceNavigationStatus.Executed, result.Status);
        Assert.Equal(new[] { "select:1:257,258,259" }, rig.Effects.Calls);
        Assert.True(rig.Echo.HasExpectation(S1));
        rig.Echo.Observe(S1, new long[] { 0x103, 0x102, 0x101 });
        Assert.True(rig.Echo.IsEcho(S1));
    }

    [Fact]
    public void ViewAndCenterTakeTheCopyOfTheRequestAndLeaveNoEchoBecauseTheySelectNothing()
    {
        var rig = new Rig();

        Assert.True(rig.Run(NavigationActionKind.View, 1).IsExecuted);
        Assert.True(rig.Run(NavigationActionKind.Center, 2).IsExecuted);

        Assert.Equal(new[] { "view:1:258", "center:1:259" }, rig.Effects.Calls);
        Assert.False(rig.Echo.HasExpectation(S1));
    }

    [Fact]
    public void LocateViewAppliesTheChosenEffectToTheCopyOfThatView()
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.LocateView, 0, NavigationViewOrder.Lateral, NavigationActionKind.Center);

        Assert.True(result.IsExecuted);
        Assert.Equal(new[] { "center:1:259" }, rig.Effects.Calls);
    }

    [Theory]
    [InlineData(RequestDecision.RejectedSessionMismatch, WorkspaceNavigationStatus.Rejected)]
    [InlineData(RequestDecision.RejectedSessionDestroyed, WorkspaceNavigationStatus.Stale)]
    public void ARequestRejectedByTheSessionHasZeroEffectsAndReadsNothingOfTheDrawing(RequestDecision decision, WorkspaceNavigationStatus status)
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.Select, decision: decision);

        Assert.Equal(status, result.Status);
        Assert.Empty(rig.Effects.Calls);
        Assert.Equal(0, rig.Facts.Reads);
        Assert.False(rig.Echo.HasExpectation(S1));
    }

    [Fact]
    public void ARequestOfADestroyedSessionEndsItsNavigationStateForGood()
    {
        var rig = new Rig();
        rig.Run(NavigationActionKind.Select);

        var result = rig.Run(NavigationActionKind.Select, decision: RequestDecision.RejectedSessionDestroyed);

        Assert.Equal(WorkspaceNavigationStatus.Stale, result.Status);
        Assert.True(rig.Targets.IsSessionEnded(S1));
        Assert.True(rig.Echo.IsEnded(S1));
        Assert.False(rig.Targets.IsCached(S1, "R-1"));
    }

    [Fact]
    public void OutsideModelSpaceTheActionIsUnavailableWithZeroEffectsAndNoRead()
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.Select, modelSpace: false);

        Assert.Equal(WorkspaceNavigationStatus.Unavailable, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Reason));
        Assert.Empty(rig.Effects.Calls);
        Assert.Equal(0, rig.Facts.Reads);
    }

    [Fact]
    public void ARackWithoutTargetsIsUnavailableAndActsOnNothing()
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.View, rackId: "R-NOPE");

        Assert.Equal(WorkspaceNavigationStatus.Unavailable, result.Status);
        Assert.Equal(NavigationActionVerdict.RejectedNoTargets, result.Verdict);
        Assert.Empty(rig.Effects.Calls);
    }

    [Fact]
    public void ATargetThatVanishedRightBeforeActingIsStaleWithZeroEffectsAndTheCacheOfTheRackDropped()
    {
        var port = new ShiftingFacts(read => read == 1
            ? new[] { NavFact.Member(10, "R-1", "frontal", 0, 0x101, 0x102) }
            : new[] { NavFact.Member(10, "R-1", "frontal", 0, 0x101) });
        var rig = new Rig(port);

        var result = rig.Run(NavigationActionKind.Select);

        Assert.Equal(WorkspaceNavigationStatus.Stale, result.Status);
        Assert.Equal(NavigationActionVerdict.RejectedTargetGone, result.Verdict);
        Assert.True(result.RefreshRequired);
        Assert.Empty(rig.Effects.Calls);
        Assert.False(rig.Targets.IsCached(S1, "R-1"));
        Assert.False(rig.Echo.HasExpectation(S1));
    }

    [Fact]
    public void AReferenceThatLeftTheRackRightBeforeActingIsStaleWithZeroEffects()
    {
        var port = new ShiftingFacts(read => read == 1
            ? new[] { NavFact.Member(10, "R-1", "frontal", 0, 0x101, 0x102) }
            : new[] { NavFact.Member(10, "R-1", "frontal", 0, 0x101), NavFact.Member(11, "R-2", "frontal", 0, 0x102) });
        var rig = new Rig(port);

        var result = rig.Run(NavigationActionKind.View, 1);

        Assert.Equal(WorkspaceNavigationStatus.Stale, result.Status);
        Assert.Equal(NavigationActionVerdict.RejectedNotAttributable, result.Verdict);
        Assert.True(result.RefreshRequired);
        Assert.Empty(rig.Effects.Calls);
        Assert.False(rig.Targets.IsCached(S1, "R-1"));
    }

    [Fact]
    public void AFailingEffectIsAFailedValueNotAnExceptionAndLeavesNoEcho()
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.Select, effects: new ThrowingEffects());

        Assert.Equal(WorkspaceNavigationStatus.Failed, result.Status);
        Assert.Equal("boom", result.Reason);
        Assert.False(rig.Echo.HasExpectation(S1));
    }

    // ------------------------------------------------------------------ the lazy purge of the sessions that no longer live

    [Fact]
    public void ThePurgeEndsTheTargetsTheEchoAndTheStateOfEverySessionThatNoLongerLivesAndOnlyThose()
    {
        var rig = new Rig();
        rig.Run(NavigationActionKind.Select, session: S1);
        rig.Run(NavigationActionKind.Select, session: S2);
        Assert.Equal(2, rig.Runner.KnownSessionCount);
        rig.Alive.Remove(S2);

        var purged = rig.Runner.PurgeDeadSessions();

        Assert.Equal(new[] { S2 }, purged);
        Assert.True(rig.Targets.IsSessionEnded(S2));
        Assert.False(rig.Targets.IsCached(S2, "R-1"));
        Assert.True(rig.Echo.IsEnded(S2));
        Assert.False(rig.Echo.HasExpectation(S2));
        Assert.Equal(1, rig.Runner.KnownSessionCount);
        Assert.False(rig.Targets.IsSessionEnded(S1));
        Assert.True(rig.Targets.IsCached(S1, "R-1"));
        Assert.True(rig.Echo.HasExpectation(S1));
    }

    [Fact]
    public void APurgedSessionIsNeverRevivedFromStaleState()
    {
        var rig = new Rig();
        rig.Run(NavigationActionKind.Select, session: S2);
        rig.Alive.Remove(S2);
        rig.Runner.PurgeDeadSessions();
        var reads = rig.Facts.Reads;
        var calls = rig.Effects.Calls.Count;

        // The authority would even answer for that id again: a purged id is still a dead one.
        rig.Alive.Add(S2);
        var result = rig.Run(NavigationActionKind.Select, session: S2);
        rig.Echo.Expect(S2, new long[] { 0x201 });

        Assert.Equal(WorkspaceNavigationStatus.Stale, result.Status);
        Assert.Equal(calls, rig.Effects.Calls.Count);
        Assert.Equal(reads, rig.Facts.Reads);
        Assert.False(rig.Echo.HasExpectation(S2));
        Assert.True(rig.Targets.Resolve(S2, "R-1").IsEmpty);
    }

    [Fact]
    public void TheLazyPurgeRunsRightBeforeEveryAction()
    {
        var rig = new Rig();
        rig.Run(NavigationActionKind.Select, session: S2);
        rig.Alive.Remove(S2);
        Assert.False(rig.Targets.IsSessionEnded(S2));

        rig.Run(NavigationActionKind.View, session: S1);

        Assert.True(rig.Targets.IsSessionEnded(S2));
        Assert.True(rig.Echo.IsEnded(S2));
    }

    [Fact]
    public void ADrainPurgesFirstAndThenAppliesTheCacheInvalidationOfItsSessionOnly()
    {
        var rig = new Rig();
        rig.Run(NavigationActionKind.Select, session: S1);
        rig.Run(NavigationActionKind.Select, session: S2);
        rig.Alive.Remove(S2);

        rig.Runner.OnDrained(new DrainOutcome(S1, new DrainResult(true, new[] { DrainOperation.ReadImplicitSelection })));
        Assert.True(rig.Targets.IsCached(S1, "R-1"));
        Assert.True(rig.Targets.IsSessionEnded(S2));

        rig.Runner.OnDrained(new DrainOutcome(S1, new DrainResult(true, new[] { DrainOperation.InvalidateNavigationCaches })));
        Assert.False(rig.Targets.IsCached(S1, "R-1"));
    }

    // ------------------------------------------------------------------ INV-F2T2A-01, 02, 03, 04: the host and its approved async pattern

    private static string Host() => Src.Code(HostPath);

    [Fact]
    public void TheHostInstantiatesExactlyOneTargetsAuthorityAndOnePolicyAndNothingElseDoes()
    {
        var host = Host();

        Assert.Equal(1, Src.Count(host, @"\bnew\s+NavigationTargets\s*\("));
        Assert.Equal(1, Src.Count(host, @"\bnew\s+NavigationActionPolicy\s*\("));
        Assert.Equal(new[] { HostPath }, Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bnew\s+NavigationTargets\s*\(")).Select(f => f.Path).ToArray());
        Assert.Equal(new[] { HostPath }, Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bnew\s+NavigationActionPolicy\s*\(")).Select(f => f.Path).ToArray());
        Assert.Matches(@"\bclass\s+WorkspaceNavigationHost\s*:\s*IWorkspaceNavigationPort\b", host);
        Assert.Equal(new[] { HostPath }, Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"class\s+\w+\s*:\s*[^{]*\bIWorkspaceNavigationPort\b")).Select(f => f.Path).ToArray());
    }

    [Fact]
    public void TheOnlyAsynchronousExecutionIsTheCommandContextOfTheHostAwaitedOnce()
    {
        var host = Host();

        Assert.Equal(1, Src.Count(host, @"\basync\b"));
        Assert.Equal(1, Src.Count(host, @"\bawait\b"));
        Assert.Matches(@"await\s+[\w.]+\.ExecuteInCommandContextAsync\s*\(", host);
        Assert.Equal(1, Src.Count(host, @"\.ExecuteInCommandContextAsync\s*\("));
        var withAsync = Src.PluginFiles().Concat(Src.ApplicationFiles()).Where(f => Regex.IsMatch(f.Code, @"\basync\b|\bawait\b")).Select(f => f.Path).ToArray();
        Assert.Equal(new[] { HostPath }, withAsync);
        Assert.Equal(new[] { HostPath }, Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"ExecuteInCommandContextAsync")).Select(f => f.Path).ToArray());
    }

    [Fact]
    public void TheHostNeverBlocksDetachesOrSchedulesAndUsesNoOtherWayToRunACommand()
    {
        var forbidden = new Regex(
            @"\.Wait\s*\(|\.Result\b|GetAwaiter|GetResult|ConfigureAwait|Task\.Run|Task\.Factory|StartNew|ContinueWith|Task\.Delay|\bThread\b|ThreadPool|\bTimer\b|BackgroundWorker|Parallel\.|SynchronizationContext|Dispatcher|SendStringToExecute|\[CommandMethod|StartCommand|Autodesk\.AutoCAD\.Internal|DllImport|HwndSource|SetFocus|user32");

        foreach (var path in new[] { HostPath, FactsPath, EffectsPath, ControllerPath, "src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs" })
        {
            var code = Src.Code(path);
            Assert.True(!forbidden.IsMatch(code), path + " uses " + forbidden.Match(code).Value);
        }

        foreach (var (path, code) in Src.ApplicationFiles())
            Assert.DoesNotMatch(new Regex(@"GetAwaiter|ConfigureAwait|\.Wait\s*\(|Task\.Run|Task\.Factory|\bThread\b"), code);
    }

    [Fact]
    public void EveryReadAndEveryEffectOfTheActionHappensInsideTheCommandContextCallbackInTheApprovedOrder()
    {
        var host = Host();
        var execute = Src.BodyOf(host, @"public\s+async\s+Task<WorkspaceNavigationResult>\s+ExecuteAsync\s*\(");
        var callback = Src.BodyOf(host, @"private\s+WorkspaceNavigationResult\s+RunInCommandContext\s*\(");

        Assert.DoesNotMatch(new Regex(@"MdiActiveDocument|TileMode|_authorize|_runner\.Run|_effects|LockDocument|StartTransaction"), execute);
        Assert.Contains("RunInCommandContext(request)", execute);
        var document = callback.IndexOf("MdiActiveDocument", StringComparison.Ordinal);
        var session = callback.IndexOf("_authorize(", StringComparison.Ordinal);
        var run = callback.IndexOf("_runner.Run(", StringComparison.Ordinal);
        var focus = callback.IndexOf("ReturnFocusToDrawing(", StringComparison.Ordinal);
        Assert.True(document >= 0 && session > document && run > session && focus > run,
            "order: active document, session, policy and effects, then the focus");
        Assert.Contains("_effects", callback);
        Assert.Contains("TileMode", callback);
        Assert.Contains("catch (System.Exception", callback);
    }

    [Fact]
    public void TheFocusReturnsToTheDrawingInOneIsolatedMethodWithThePublicWindowApiAndFailsSafely()
    {
        var host = Host();
        var focus = Src.BodyOf(host, @"private\s+static\s+void\s+ReturnFocusToDrawing\s*\(");

        Assert.Equal(2, Src.Count(host, @"\bReturnFocusToDrawing\s*\("));
        Assert.Equal(1, Src.Count(host, @"\.Focus\s*\("));
        Assert.Contains(".Window.Focus()", focus);
        Assert.Contains("catch (System.Exception", focus);
        Assert.DoesNotMatch(new Regex(@"_runner|_effects|ExecuteAsync|SetImpliedSelection|SetCurrentView|Commit"), focus);
        foreach (var (path, code) in Src.PluginFiles().Where(f => f.Path != HostPath))
            Assert.DoesNotMatch(new Regex(@"ReturnFocusToDrawing"), code);
    }

    [Fact]
    public void TheHostNamesNoEventSideTypeAndTheLoggingIsOnlyInsideTheFaultMethod()
    {
        var host = Host();

        Assert.DoesNotMatch(new Regex(@"\bIHostEventSource\b|\bHintKind\b|\bEnqueueHint\b|\bEventBridgeCore\b|\bIWorkspaceBridgeLink\b|\bAutoCadEventBridge\b"), host);
        Assert.Equal(1, Src.Count(host, @"RackLog\.Exception\s*\("));
        Assert.Matches(@"static\s+void\s+LogFault\s*\(\s*string\s+\w+\s*,\s*System\.Exception\s+\w+\s*\)\s*=>\s*RackLog\.Exception", host);
        Assert.DoesNotMatch(new Regex(@"\+=|-="), host);
    }

    [Fact]
    public void TheHostReconcilesWithTheSessionAuthorityOnEveryDrainAndOnEveryAction()
    {
        var host = Host();
        var drained = Src.BodyOf(host, @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");

        Assert.Contains("_runner.OnDrained(", drained);
        var runner = Src.Code(PortPath);
        var onDrained = Src.BodyOf(runner, @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        Assert.True(onDrained.IndexOf("PurgeDeadSessions()", StringComparison.Ordinal) < onDrained.IndexOf("ApplyDrain(", StringComparison.Ordinal));
        var run = Src.BodyOf(runner, @"public\s+WorkspaceNavigationResult\s+Run\s*\(");
        Assert.True(run.IndexOf("PurgeDeadSessions()", StringComparison.Ordinal) < run.IndexOf("_policy.Plan(", StringComparison.Ordinal));
        Assert.True(Src.Count(runner, @"\.EndSession\s*\(") == 1 && Src.Count(runner, @"\.TryGetSession\s*\(") == 0);
    }

    // ------------------------------------------------------------------ INV-F2T2A-05: the facts adapter

    [Fact]
    public void TheFactsAdapterConsumesTheIntegratedScanAndReadsOnlyTheDirectModelSpaceReferenceHandles()
    {
        var facts = Src.Code(FactsPath);

        Assert.Matches(@"\bclass\s+WorkspaceNavigationFacts\s*:\s*INavigationFactsPort\b", facts);
        Assert.Equal(new[] { FactsPath }, Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"class\s+\w+\s*:\s*[^{]*\bINavigationFactsPort\b")).Select(f => f.Path).ToArray());
        Assert.Equal(1, Src.Count(facts, @"RackBlockFinder\.ScanEnvelopes\s*\("));
        Assert.Matches(@"GetBlockReferenceIds\s*\(\s*directOnly:\s*true", facts);
        Assert.Contains("OwnerId", facts);
        Assert.Contains("ModelSpaceReferenceHandles", facts);
        Assert.DoesNotMatch(new Regex(@"\.Commit\s*\(|OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|AppendEntity|\.Erase\s*\(|SetImpliedSelection|SetCurrentView|SelectImplied|RackBlockData|RackEmbedStore|NavigationTargets|NavigationActionPolicy|\.ReadDefinitionFacts\("), facts);
        Assert.Matches(@"using\s*\(\s*\w+\.LockDocument\s*\(\s*\)\s*\)", facts);
        Assert.Matches(@"using\s*\(\s*var\s+\w+\s*=\s*[\w.]+\.StartTransaction\s*\(\s*\)\s*\)", facts);
    }

    [Fact]
    public void TheIntegratedAuthoritiesOfF1AreNotMentionedByNavigationAndStayAsTheyWere()
    {
        foreach (var path in new[]
                 {
                     "src/RackCad.Plugin/RackBlockFinder.cs", "src/RackCad.Plugin/RackInventarioCommands.cs",
                     "src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs", "src/RackCad.Plugin/PluginInitializer.cs",
                     "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs", "src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs",
                     "src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs", "src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs",
                     "src/RackCad.Application/Workspace/EventBridgeCore.cs", "src/RackCad.Application/Workspace/NavigationTargets.cs",
                     "src/RackCad.Application/Workspace/NavigationActions.cs", "src/RackCad.Application/Workspace/NavigationFacts.cs",
                 })
            Assert.DoesNotMatch(new Regex(@"WorkspaceNavigation|NavigationEcho|IWorkspaceNavigationPort"), Src.Code(path));
    }

    // ------------------------------------------------------------------ INV-F2T2A-06: the effects

    [Fact]
    public void TheEffectsAdapterImplementsTheEffectsPortAndIsTheOnlyWriterOfTheSelectionAndTheView()
    {
        var effects = Src.Code(EffectsPath);

        Assert.Matches(@"\bclass\s+WorkspaceNavigationEffects\s*:\s*INavigationEffects\b", effects);
        Assert.Equal(new[] { EffectsPath }, Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"class\s+\w+\s*:\s*[^{]*\bINavigationEffects\b") && f.Path.StartsWith("src/RackCad.Plugin/", StringComparison.Ordinal)).Select(f => f.Path).ToArray());
        Assert.Equal(1, Src.Count(effects, @"\bSetImpliedSelection\s*\("));
        Assert.Equal(2, Src.Count(effects, @"\bSetCurrentView\s*\("));
        Assert.Equal(new[] { EffectsPath }, Src.PluginFiles().Where(f => Regex.IsMatch(f.Code, @"\bSetImpliedSelection\b|\bSetCurrentView\b")).Select(f => f.Path).ToArray());
        Assert.Matches(@"1\.10", effects);
    }

    [Fact]
    public void TheEffectsAdapterReadsOnlyTheAlreadySelectedTargetAndDiscoversNothing()
    {
        var effects = Src.Code(EffectsPath);

        Assert.Equal(1, Src.Count(effects, @"\bGetObject\s*\("));
        Assert.Matches(@"GetObject\s*\(\s*id\s*,\s*OpenMode\.ForRead\s*\)", effects);
        Assert.DoesNotMatch(
            new Regex(@"ScanEnvelopes|RackBlockFinder|RackBlockData|RackEmbed\w*|\bBlockTable\b|BlockTableRecord|GetBlockReferenceIds|NavigationTargets|ModelSpaceReferenceHandles|ReadDefinitionFacts|NavigationActionPolicy|SelectImplied|GetSelection|SelectAll|\bforeach\s*\([^)]*\bin\s+(?:modelSpace|blockTable|space|btr)\b"),
            effects);
    }

    [Fact]
    public void TheEffectsAdapterWritesNothingAuthoredNoSystemVariableNoLayoutAndNoTileMode()
    {
        var effects = Src.Code(EffectsPath);

        Assert.DoesNotMatch(
            new Regex(@"TileMode|SetSystemVariable|LayoutManager|CurrentLayout|\bXrecord\b|\bXData\b|\bDBDictionary\b|NamedObjectsDictionary|AppendEntity|UpgradeOpen|DowngradeOpen|\.Commit\s*\(|OpenMode\.(?:ForWrite|ForNotify)|\.Erase\s*\(|Zoom|Regen|UpdateScreen|Highlight|System\.IO|\bFile\."),
            effects);
        Assert.Matches(@"using\s*\(\s*\w+\.LockDocument\s*\(\s*\)\s*\)", effects);
        Assert.Matches(@"using\s*\(\s*var\s+\w+\s*=\s*[\w.]+\.StartTransaction\s*\(\s*\)\s*\)", effects);
        Assert.DoesNotMatch(new Regex(@"\b(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:Autodesk\S*\.)?(?:Document|Database|Editor|ObjectId|Transaction)\s+_\w+\s*[;=]"), effects);
    }

    // ------------------------------------------------------------------ INV-F2T2A-07, 09, 12: the controller

    [Fact]
    public void TheControllerCreatesTheNavigationHostLazilySharesTheEchoAndSubscribesToNothingNew()
    {
        var controller = Src.Code(ControllerPath);
        var constructor = Src.BodyOf(controller, @"private\s+WorkspaceController\s*\(\s*\)");

        Assert.Equal(1, Src.Count(controller, @"\bnew\s+WorkspaceNavigationHost\s*\("));
        Assert.DoesNotContain("WorkspaceNavigationHost", constructor);
        Assert.Matches(@"public\s+IWorkspaceNavigationPort\s+Navigation\b", controller);
        Assert.Contains("_navigation.OnDrained(outcome)", Src.BodyOf(controller, @"private\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)"));
        Assert.Equal(3, Src.Count(controller, @"\+="));
        Assert.Equal(0, Src.Count(controller, @"-="));
        Assert.DoesNotMatch(new Regex(@"\bINavigationEffects\b|\bNavigationActionPolicy\b|\bNavigationActionPlan\b|\bTask\b|\basync\b"), controller);
    }

    [Fact]
    public void EveryNewPluginFileIsAnExactFileOfTheFolderAndTheFolderHoldsNineFiles()
    {
        var files = Src.PluginFiles().Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray();

        Assert.Equal(9, files.Length);
        Assert.Contains(HostPath, files);
        Assert.Contains(FactsPath, files);
        Assert.Contains(EffectsPath, files);
        Assert.Empty(files.Where(f => f.Substring(Src.PluginWorkspace.Length).Contains('/')));
    }
}
