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
    public void TheNavigationRunsSynchronouslyAndNoProductFileUsesTheInternalCommand()
    {
        var host = Host();

        Assert.Equal(0, Src.Count(host, @"\basync\b"));
        Assert.Equal(0, Src.Count(host, @"\bawait\b"));
        Assert.Matches(@"public\s+Task<WorkspaceNavigationResult>\s+ExecuteAsync\s*\(", host);
        Assert.Contains("Task.FromResult(", host);
        Assert.Empty(Src.PluginFiles().Concat(Src.ApplicationFiles()).Where(f => Regex.IsMatch(f.Code, @"\basync\b|\bawait\b")).Select(f => f.Path));
        Assert.Empty(Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"ExecuteInCommandContextAsync")).Select(f => f.Path));
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
    public void EveryReadAndEveryEffectOfTheActionHappensUnderTheDocumentLockInTheApprovedOrder()
    {
        var host = Host();
        var execute = Src.BodyOf(host, @"public\s+Task<WorkspaceNavigationResult>\s+ExecuteAsync\s*\(");
        var run = Src.BodyOf(host, @"private\s+WorkspaceNavigationResult\s+RunOnActiveDocument\s*\(");

        Assert.DoesNotMatch(new Regex(@"TileMode|_authorize|_runner\.Run|_effects|LockDocument|StartTransaction"), execute);
        Assert.Contains("RunOnActiveDocument(request)", execute);
        var document = run.IndexOf("MdiActiveDocument", StringComparison.Ordinal);
        var command = run.IndexOf("CommandInProgress", StringComparison.Ordinal);
        var locking = run.IndexOf("LockDocument()", StringComparison.Ordinal);
        var session = run.IndexOf("_authorize(", StringComparison.Ordinal);
        var action = run.IndexOf("_runner.Run(", StringComparison.Ordinal);
        var focus = run.IndexOf("ReturnFocusToDrawing(", StringComparison.Ordinal);
        Assert.True(document >= 0 && command > document && locking > command && session > locking && action > session && focus > action,
            "order: active document, no command running, lock, session, policy and effects, then the focus");
        Assert.Contains("_effects", run);
        Assert.Contains("TileMode", run);
        Assert.Contains("catch (System.Exception", execute);
    }

    [Fact]
    public void AnActionNeverRunsWhileACommandIsInProgress()
    {
        var run = Src.BodyOf(Host(), @"private\s+WorkspaceNavigationResult\s+RunOnActiveDocument\s*\(");

        Assert.Matches(@"string\.IsNullOrEmpty\(\s*\w+\.CommandInProgress\s*\)", run);
        Assert.Matches(@"\.Editor\.IsQuiescent", run);
        Assert.Contains("WorkspaceNavigationResult.Stale(", run);
    }

    [Fact]
    public void TheViewIsAppliedAfterTheReadTransactionClosesSoItIsNotRolledBack()
    {
        var effects = Src.Code(EffectsPath);
        var view = Src.BodyOf(effects, @"public\s+void\s+ViewReference\s*\(");
        var center = Src.BodyOf(effects, @"public\s+void\s+CenterOnReference\s*\(");
        var readBlock = new Regex(@"using\s*\(\s*var\s+\w+\s*=\s*[\w.]+\.StartTransaction\s*\(\s*\)\s*\)\s*\{\s*extents\s*=\s*ExtentsOfTarget\s*\([^;]*;\s*\}");

        foreach (var body in new[] { view, center })
        {
            var match = readBlock.Match(body);
            Assert.True(match.Success, "the transaction only reads the extents");
            Assert.True(body.IndexOf("SetCurrentView(", StringComparison.Ordinal) > match.Index + match.Length, "the view is written after the transaction is closed");
            Assert.True(body.IndexOf("GetCurrentView(", StringComparison.Ordinal) > match.Index + match.Length, "the view is read after the transaction is closed");
        }

        Assert.DoesNotMatch(new Regex(@"\.Commit\s*\("), effects);
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

    // F2T2A-KL-01 (accepted, diagnostic-only): the integrated scan does not expose the raw probe identity, so the facts adapter
    // leaves ProbedId null. That must not make an unreadable member navigable, and neither the attribution nor the revalidation
    // of a target may depend on ProbedId.
    [Fact]
    public void AnUnreadableFactWithoutEnvelopeAndWithoutProbedIdIsNoTargetAndAttributionAndRevalidationDoNotDependOnProbedId()
    {
        var unreadableWithoutProbe = NavFact.Raw(30, false, false, null, null, null, null, -1, 0x301);
        var unreadableWithProbe = NavFact.Raw(30, false, false, null, "R-1", null, null, -1, 0x301);

        foreach (var unreadable in new[] { unreadableWithoutProbe, unreadableWithProbe })
        {
            var facts = new FakeNavigationFacts().With(S1, NavFact.Member(10, "R-1", "frontal", 0, 0x101), unreadable);
            var targets = new NavigationTargets(facts);
            var policy = new NavigationActionPolicy(targets);

            // The unreadable member is no target, whether the probe identity is known or not.
            Assert.Equal(new long[] { 0x101 }, NavFact.Handles(targets.Resolve(S1, "R-1")));
            Assert.True(targets.Resolve(S1, "R-30").IsEmpty);
            Assert.True(targets.Resolve(S1, "").IsEmpty);

            // Its reference is attributed to nobody: revalidation reports it as not attributable, with or without the probe.
            Assert.Equal(NavigationRevalidationVerdict.Valid, targets.Revalidate(S1, "R-1", new long[] { 0x101 }).Verdict);
            Assert.Equal(NavigationRevalidationVerdict.NotAttributable, targets.Revalidate(S1, "R-1", new long[] { 0x301 }).Verdict);

            // An action acts only on the readable member.
            var effects = new RecordingNavigationEffects();
            var decision = policy.Execute(policy.Plan(new WorkspaceRequest(S1), "R-1", NavigationActionKind.Select), S1, effects);
            Assert.Equal(NavigationActionVerdict.Act, decision.Verdict);
            Assert.Equal(new[] { "select:1:257" }, effects.Calls);
        }

        // The only difference the probe makes is a diagnostic, never a target.
        var withProbe = new NavigationTargets(new FakeNavigationFacts().With(S1, unreadableWithProbe)).Resolve(S1, "R-1");
        var withoutProbe = new NavigationTargets(new FakeNavigationFacts().With(S1, unreadableWithoutProbe)).Resolve(S1, "R-1");
        Assert.True(withProbe.IsEmpty);
        Assert.True(withoutProbe.IsEmpty);
        Assert.Empty(withoutProbe.Diagnostics);

        // The adapter states plainly that it leaves ProbedId null, and reads no authored data to fill it.
        var adapter = Src.Code(FactsPath);
        Assert.Matches(@"ProbedId:\s*null", adapter);
        Assert.DoesNotMatch(new Regex(@"RackBlockData|RackEmbedStore|Xrecord|XData|ExtensionDictionary"), adapter);
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

    // ------------------------------------------------------------------ I-64 F2-T2B (INV-F2T2B-07): TargetCount, the additive field
    // The panel presents a copy "k of M" without counting targets itself: M is the number of targets of the set of the requested
    // action, calculated by the runner from NavigationTargets.Resolve. In the rig, S1 / R-1 has two Frontal references (0x101,
    // 0x102) and one Lateral (0x103).

    [Fact]
    public void TargetCountIsAnAdditiveNullableFieldThatDefaultsToNullInEveryFactory()
    {
        var property = typeof(WorkspaceNavigationResult).GetProperty("TargetCount");

        Assert.NotNull(property);
        Assert.Equal(typeof(int?), property!.PropertyType);
        Assert.Null(WorkspaceNavigationResult.Executed().TargetCount);
        Assert.Null(WorkspaceNavigationResult.Unavailable("x").TargetCount);
        Assert.Null(WorkspaceNavigationResult.Rejected("x").TargetCount);
        Assert.Null(WorkspaceNavigationResult.Stale("x", true).TargetCount);
        Assert.Null(WorkspaceNavigationResult.Failed("x").TargetCount);
        Assert.Null(new WorkspaceNavigationResult(WorkspaceNavigationStatus.Executed, NavigationActionVerdict.Act, false, null).TargetCount);
        Assert.Equal(4, (WorkspaceNavigationResult.Executed() with { TargetCount = 4 }).TargetCount);
    }

    [Theory]
    [InlineData(NavigationActionKind.Select, 3)]
    [InlineData(NavigationActionKind.View, 3)]
    [InlineData(NavigationActionKind.Center, 3)]
    public void TheRunnerAnswersWithTheNumberOfAllTheTargetsOfTheRackForSelectViewAndCenter(NavigationActionKind action, int expected)
    {
        var rig = new Rig();

        var result = rig.Run(action);

        Assert.Equal(WorkspaceNavigationStatus.Executed, result.Status);
        Assert.Equal(expected, result.TargetCount);
    }

    [Theory]
    [InlineData(NavigationViewOrder.Frontal, 2)]
    [InlineData(NavigationViewOrder.Lateral, 1)]
    [InlineData(NavigationViewOrder.Planta, 0)]
    [InlineData(NavigationViewOrder.Other, 0)]
    public void ForLocateViewTheCountIsOnlyThatOfTheRequestedView(NavigationViewOrder view, int expected)
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.LocateView, 0, view);

        Assert.Equal(expected, result.TargetCount);
        Assert.Equal(expected > 0 ? WorkspaceNavigationStatus.Executed : WorkspaceNavigationStatus.Unavailable, result.Status);
    }

    [Fact]
    public void ALocateViewWithoutARequestedViewCountsNoTargetAndActsOnNothing()
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.LocateView, 0, null);

        Assert.Equal(0, result.TargetCount);
        Assert.Equal(WorkspaceNavigationStatus.Unavailable, result.Status);
        Assert.Empty(rig.Effects.Calls);
    }

    [Fact]
    public void ARackWithoutTargetsAnswersZeroTargetsAndAnotherRackOfTheSessionDoesNotCount()
    {
        var rig = new Rig();

        Assert.Equal(0, rig.Run(NavigationActionKind.View, rackId: "R-NOPE").TargetCount);
        Assert.Equal(1, rig.Run(NavigationActionKind.View, rackId: "R-9").TargetCount);
        Assert.Equal(1, rig.Run(NavigationActionKind.View, session: S2).TargetCount);
    }

    [Fact]
    public void TheCountDoesNotDependOnTheCopyAskedFor()
    {
        var rig = new Rig();

        Assert.Equal(3, rig.Run(NavigationActionKind.View, 0).TargetCount);
        Assert.Equal(3, rig.Run(NavigationActionKind.View, 2).TargetCount);
        Assert.Equal(3, rig.Run(NavigationActionKind.Center, 7).TargetCount);
    }

    [Fact]
    public void ARequestThatEndsBeforeTheTargetsWereAskedForHasNoCountAndReadsNothing()
    {
        var rig = new Rig();

        Assert.Null(rig.Run(NavigationActionKind.View, decision: RequestDecision.RejectedSessionMismatch).TargetCount);
        Assert.Null(rig.Run(NavigationActionKind.View, decision: RequestDecision.RejectedSessionDestroyed).TargetCount);
        Assert.Null(new Rig().Run(NavigationActionKind.View, modelSpace: false).TargetCount);
        Assert.Equal(0, rig.Facts.Reads);
    }

    [Fact]
    public void ATargetThatVanishedKeepsTheCountOfTheSetTheActionWasPlannedFrom()
    {
        var port = new ShiftingFacts(read => read == 1
            ? new[] { NavFact.Member(10, "R-1", "frontal", 0, 0x101, 0x102) }
            : new[] { NavFact.Member(10, "R-1", "frontal", 0, 0x101) });
        var rig = new Rig(port);

        var result = rig.Run(NavigationActionKind.Select);

        Assert.Equal(WorkspaceNavigationStatus.Stale, result.Status);
        Assert.Equal(2, result.TargetCount);
        Assert.False(rig.Targets.IsCached(S1, "R-1"));
    }

    [Fact]
    public void AFailingEffectIsStillAFailedValueAndCarriesTheCount()
    {
        var rig = new Rig();

        var result = rig.Run(NavigationActionKind.Select, effects: new ThrowingEffects());

        Assert.Equal(WorkspaceNavigationStatus.Failed, result.Status);
        Assert.Equal("boom", result.Reason);
        Assert.Equal(3, result.TargetCount);
    }

    [Fact]
    public void CountingTheTargetsCostsNoReadBeyondThePlanAndTheRevalidation()
    {
        var rig = new Rig();

        rig.Run(NavigationActionKind.LocateView, 0, NavigationViewOrder.Frontal);

        // One read to plan from the authority, one to revalidate right before acting; the count adds none.
        Assert.Equal(2, rig.Facts.Reads);
    }

    [Fact]
    public void TheCountChangesNeitherTheStatusesNorTheVerdictsNorTheEffects()
    {
        var rig = new Rig();

        var executed = rig.Run(NavigationActionKind.View, 1);
        var unavailable = rig.Run(NavigationActionKind.View, rackId: "R-NOPE");

        Assert.Equal(WorkspaceNavigationStatus.Executed, executed.Status);
        Assert.Equal(NavigationActionVerdict.Act, executed.Verdict);
        Assert.Equal(WorkspaceNavigationStatus.Unavailable, unavailable.Status);
        Assert.Equal(NavigationActionVerdict.RejectedNoTargets, unavailable.Verdict);
        Assert.Equal(new[] { "view:1:258" }, rig.Effects.Calls);
    }

    [Fact]
    public void TheCountIsCalculatedInTheRunnerFromTheAuthorityAndTheHostDoesNotChange()
    {
        var port = Src.Code(PortPath);
        var run = Src.BodyOf(port, @"public\s+WorkspaceNavigationResult\s+Run\s*\(");

        Assert.Matches(@"int\?\s+TargetCount\s*=\s*null", port);
        Assert.Contains("TargetCount", run);
        Assert.Matches(@"_targets\.Resolve\s*\(", Src.BodyOf(port, @"private\s+int\s+CountTargets\s*\("));
        Assert.DoesNotMatch(new Regex(@"\bAutodesk\b|\bDocument\b|\bEditor\b|\bDatabase\b|\bObjectId\b|System\.Windows"), port);
        Assert.DoesNotContain("TargetCount", Src.Code(HostPath));
        Assert.DoesNotContain("TargetCount", Src.Code(EffectsPath));
        Assert.DoesNotContain("TargetCount", Src.Code(FactsPath));
    }

    // ------------------------------------------------------------------ I-64 F2-T2B: the panel side of the navigation (source guards)
    // The Core test project cannot load WPF, so the files of the UI folder and the palette host are read as text.

    private const string UiFolder = "src/RackCad.UI/Workspace/";
    private const string PanelHostPath = "src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs";

    private static string NavigationViewModel() => Src.Code(UiFolder + "WorkspaceNavigationViewModel.cs");

    [Fact]
    public void TheUiNamesOnlyTheNavigationTypesItIsAllowedToAndNeverTheAuthority()
    {
        var forbidden = new Regex(
            @"\b(?:NavigationTargets|NavigationTargetSet|NavigationTarget|NavigationDefinitionFact|INavigationFactsPort|NavigationActionPolicy|NavigationActionPlan|NavigationActionDecision|INavigationEffects|NavigationActionVerdict|NavigationEcho|WorkspaceNavigationRunner|WorkspaceNavigationHost|WorkspaceNavigationRequest)\b");
        var allowed = new[] { "IWorkspaceNavigationPort", "WorkspaceNavigationResult", "WorkspaceNavigationStatus", "WorkspaceRequest", "NavigationActionKind", "NavigationViewOrder" };

        var files = Src.Files(UiFolder);
        Assert.Contains(files, f => f.Path == UiFolder + "WorkspaceNavigationViewModel.cs");
        foreach (var (path, code) in files)
            Assert.DoesNotMatch(forbidden, code);

        var model = NavigationViewModel();
        foreach (var name in allowed)
            Assert.Matches(@"\b" + name + @"\b", model);
    }

    [Fact]
    public void TheNavigationViewModelDerivesNoTargetNeverOrdersReferencesAndHoldsNoInventory()
    {
        var model = NavigationViewModel();

        Assert.DoesNotMatch(new Regex(@"\.Sort\s*\(|OrderBy|\.Resolve\s*\(|ReferenceHandle|ModelSpaceReferenceHandles|DefinitionHandle|\.Targets\b|Handles\b"), model);
        Assert.Matches(@"await\s+_port\.ExecuteAsync\s*\(", model);
        Assert.DoesNotMatch(new Regex(@"Dictionary<|HashSet<|List<(?!WorkspaceViewChoice)"), model);
    }

    [Fact]
    public void ThePanelViewModelNeverCallsTheNavigationPortWhenItRefreshesPresentsOrChangesContext()
    {
        var panel = Src.Code(UiFolder + "WorkspacePanelViewModel.cs");

        Assert.DoesNotMatch(new Regex(@"ExecuteAsync|NavigateAsync|SelectAsync|ViewAsync|CenterAsync|LocateAsync"), panel);
        Assert.Equal(1, Src.Count(panel, @"\bnew\s+WorkspaceNavigationViewModel\s*\("));
        Assert.Matches(@"new\s+WorkspaceNavigationViewModel\s*\(\s*navigation\s*,\s*Refresh\s*\)", panel);
    }

    [Fact]
    public void OnlyTheNavigationViewModelCallsTheNavigationPortAndTheViewReachesItOnlyThroughExplicitClicks()
    {
        foreach (var (path, code) in Src.Files(UiFolder))
        {
            var calls = Src.Count(code, @"\.ExecuteAsync\s*\(");
            Assert.Equal(path == UiFolder + "WorkspaceNavigationViewModel.cs" ? 1 : 0, calls);
        }

        var view = Src.Code(UiFolder + "WorkspacePanelView.xaml.cs");
        foreach (var handler in new[] { "OnNavigationSelectClick", "OnNavigationViewClick", "OnNavigationCenterClick", "OnNavigationLocateClick", "OnNavigationNextCopyClick" })
            Assert.Contains(handler, view);
        Assert.DoesNotMatch(new Regex(@"\bLoaded\b|\bIsVisibleChanged\b|\bSizeChanged\b|\bCompositionTarget\b|Navigation\.Present\s*\("), view);
    }

    [Fact]
    public void TheUiOffersNoActionThatWritesAndNeverTouchesFocusLayoutOrSystemVariables()
    {
        foreach (var (path, code) in Src.Files(UiFolder))
        {
            Assert.DoesNotMatch(new Regex(@"Keyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager"), code);
            Assert.DoesNotMatch(new Regex(@"TileMode|TILEMODE|LayoutManager|SetSystemVariable|DBDictionary|NamedObjectsDictionary|Xrecord|XData|AppendEntity|\.Commit\s*\("), code);
        }
    }

    [Fact]
    public void TheNavigationControlsOfTheViewHaveNoFixedWidthNoMinimumAndNoMaximumAndReflowInWrapPanels()
    {
        var xaml = Regex.Replace(Src.Text(UiFolder + "WorkspacePanelView.xaml"), @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
        var start = xaml.IndexOf("Text=\"Navegación\"", StringComparison.Ordinal);
        var end = xaml.IndexOf("Text=\"Instrumentación\"", StringComparison.Ordinal);

        Assert.True(start >= 0 && end > start, "the navigation section of the view is missing");
        var section = xaml.Substring(start, end - start);
        Assert.DoesNotMatch(new Regex(@"\b(?:Min|Max)?Width\s*=|\b(?:Min|Max)?Height\s*="), section);
        Assert.True(Src.Count(section, @"<WrapPanel\b") >= 3, "the navigation controls do not reflow in WrapPanels");
        Assert.DoesNotMatch(new Regex(@"<Window\b|ShowDialog|MessageBox"), section);
        foreach (var name in new[] { "NavigationSelectButton", "NavigationViewButton", "NavigationCenterButton", "NavigationNextCopyButton", "NavigationViewSelector", "NavigationLocateButton" })
            Assert.Contains("x:Name=\"" + name + "\"", section);
    }

    [Fact]
    public void ThePaletteHostOnlyGainsTheNavigationPortAsTheThirdArgumentAndNothingElse()
    {
        var host = Src.Code(PanelHostPath);

        Assert.Equal(1, Src.Count(host, @"_controller\.Navigation\b"));
        // I-64 F3-T2 (INV-F3T2-16): the fourth argument is the summary port of the browser; the palette host still names no port type.
        Assert.Matches(@"new\s+WorkspacePanelViewModel\s*\(\s*_controller\.Port\s*,\s*_controller\.SetInstrumentationEnabled\s*,\s*_controller\.Navigation\s*,\s*_controller\.BrowserSummary\s*\)", host);
        Assert.Equal(
            new[] { "Autodesk.AutoCAD.Windows", "RackCad.Application.Diagnostics", "RackCad.Application.Workspace", "RackCad.UI.Workspace" },
            Regex.Matches(host, @"^\s*using\s+([\w.]+)\s*;", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToArray());
        Assert.Equal(2, Src.Count(host, @"\+="));
        Assert.DoesNotMatch(new Regex(@"IWorkspaceNavigationPort|WorkspaceNavigation|NavigationEcho|NavigationTargets|NavigationAction"), host);
        Assert.Matches(@"private\s+const\s+double\s+InitialWidth\s*=\s*320\s*;", host);
        Assert.Matches(@"private\s+const\s+int\s+MinimumWidth\s*=\s*240\s*;", host);
        Assert.Matches(@"_palette\.DockEnabled\s*=\s*DockSides\.Left\s*\|\s*DockSides\.Right\s*;", host);
    }
}
