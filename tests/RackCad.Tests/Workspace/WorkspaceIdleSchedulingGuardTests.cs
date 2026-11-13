#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T2C (IC-04), INV-F4T2C-08: the source guards G-1 to G-10 of the scheduling of the reads. The Core test project cannot reference
// the Plugin (it needs AutoCAD), so they read the sources as text. By the rule of I-1 (F4-E32, F4-E33) they are negative, or limited to
// the bodies of the members this task owns, with bounds and ordered prefixes instead of the exact set of a whole file, and none of them
// forbids for ever what belongs to F4-T3 or F4-T4 (for example wrapping the advisor or the close handler). Each one has a self-test:
// the real source passes and the text that must trip it trips it.
public class WorkspaceIdleSchedulingGuardTests
{
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string CorePath = "src/RackCad.Application/Workspace/EventBridgeCore.cs";
    private const string SchedulerPath = "src/RackCad.Application/Workspace/WorkspaceIdleScheduler.cs";
    private const string HostGuardPath = "tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs";

    private static string Bridge() => Src.Code(BridgePath);

    private static string Controller() => Src.Code(ControllerPath);

    private static string Core() => Src.Code(CorePath);

    private static string Scheduler() => Src.Code(SchedulerPath);

    private static string Mutate(string code, string from, string to)
    {
        Assert.Contains(from, code);
        return code.Replace(from, to);
    }

    private static string OnIdleBody(string bridge) => Src.BodyOf(bridge, @"private\s+void\s+OnIdle\s*\(\s*\)");

    private static string TickBody(string bridge) => Src.BodyOf(bridge, @"private\s+void\s+OnIdleTick\s*\(\s*\)");

    // The body of the continuation of the bridge (the method the scheduler is built with).
    private static string ContinuationBody(string bridge) => Src.BodyOf(bridge, @"private\s+void\s+RunIdleContinuation\s*\(\s*\)");

    private static readonly Regex OnIdleWithoutTick = new(@"(?<![\w.])OnIdle\b");

    // I-64 F4-T3A (T2C 1): the name of the intermediate method that the constructor of the bridge hands to the scheduler (the one that G-10
    // extracts), or null when the continuation is a lambda. G-1 and G-2 forbid it in the handlers and in OnIdleTick too.
    private static string? IntermediateName(string bridge)
    {
        var constructor = Src.BodyOf(bridge, @"public\s+AutoCadEventBridge\s*\(\s*\)");
        var build = Regex.Match(constructor, @"new\s+WorkspaceIdleScheduler\s*\(\s*_core\.CanDrainNow\s*,\s*(.+?)\s*,\s*LogFault\s*\)");
        return build.Success && Regex.IsMatch(build.Groups[1].Value, @"^\w+$") ? build.Groups[1].Value : null;
    }

    private static Regex Named(string name) => new(@"(?<![\w.])" + Regex.Escape(name) + @"\b");

    private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

    // ================================================================== G-1: the handlers of the notifications only mark

    // What no case block and no handler line of the bridge may contain: the idle pass, the drain notice, the dispatcher, the scheduler
    // and any API that reads. The Idle block is the one exception, and only through its own line: it calls OnIdleTick.
    private static readonly Regex HandlerForbidden = new(
        @"(?<![\w.])OnIdle\b|\bDrained\b|BeginInvoke|_idleScheduler|\bTryDrain\b|StartTransaction|LockDocument|GetObject|SelectImplied|GetSelection|\.Read\w*\s*\(|\bReadSummary\b|\b_reader\b|OpenMode");

    private static IEnumerable<(string Source, string Block)> CaseBlocks(string bridge)
    {
        var hook = Src.BodyOf(bridge, @"private\s+Action\s+Hook\s*\(");
        var starts = Regex.Matches(hook, @"\bcase\s+BridgeSource\.(\w+)\s*:").Cast<Match>().ToList();
        var defaultAt = hook.IndexOf("default:", StringComparison.Ordinal);
        for (var i = 0; i < starts.Count; i++)
        {
            var end = i + 1 < starts.Count ? starts[i + 1].Index : (defaultAt > starts[i].Index ? defaultAt : hook.Length);
            yield return (starts[i].Groups[1].Value, hook.Substring(starts[i].Index, end - starts[i].Index));
        }
    }

    private static IEnumerable<string> HandlerViolations(string bridge)
    {
        var blocks = CaseBlocks(bridge).ToList();
        if (blocks.Count < 16) yield return "the hook has fewer case blocks than sources: " + blocks.Count;

        foreach (var (source, block) in blocks)
            foreach (Match match in HandlerForbidden.Matches(block))
                yield return source + " block contains " + match.Value;

        foreach (var line in bridge.Split('\n').Where(l => Regex.IsMatch(l, @"\bhandler\s*=\s*\(")))
            foreach (Match match in HandlerForbidden.Matches(line))
                yield return "handler line contains " + match.Value + ": " + line.Trim();

        // T2C 1: the intermediate method of the continuation is not named by any case block or handler line either.
        var intermediate = IntermediateName(bridge);
        if (intermediate != null)
        {
            var named = Named(intermediate);
            foreach (var (source, block) in blocks)
                if (named.IsMatch(block)) yield return source + " block names " + intermediate;
            foreach (var line in bridge.Split('\n').Where(l => Regex.IsMatch(l, @"\bhandler\s*=\s*\(")))
                if (named.IsMatch(line)) yield return "handler line names " + intermediate + ": " + line.Trim();
        }

        var idle = blocks.FirstOrDefault(b => b.Source == "Idle").Block ?? string.Empty;
        if (!Regex.IsMatch(idle, @"EventHandler\s+handler\s*=\s*\(\s*s\s*,\s*e\s*\)\s*=>\s*Contain\s*\(\s*""Idle""\s*,\s*OnIdleTick\s*\)\s*;"))
            yield return "the Idle handler is not (s, e) => Contain(\"Idle\", OnIdleTick)";
    }

    [Fact]
    public void G1_NoCaseBlockNorHandlerLineOfTheBridgeRunsTheIdlePassNotifiesTheDrainPostsOrReads()
    {
        Assert.Empty(HandlerViolations(Bridge()));
    }

    [Theory]
    [InlineData("Contain(\"Idle\", OnIdleTick)", "Contain(\"Idle\", OnIdle)")]
    [InlineData("Contain(\"Idle\", OnIdleTick)", "Contain(\"Idle\", () => { OnIdleTick(); Drained?.Invoke(default); })")]
    [InlineData("_core.OnDocumentCreated(NativeId(e.Document))", "_core.OnDocumentCreated(NativeId(e.Document)); _dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { }))")]
    [InlineData("_core.OnDocumentCreated(NativeId(e.Document))", "_core.OnDocumentCreated(NativeId(e.Document)); _idleScheduler.OnIdleTick()")]
    [InlineData("ActiveSessionChanged?.Invoke();", "ActiveSessionChanged?.Invoke(); ReadSummary(null);")]
    [InlineData("ActiveSessionChanged?.Invoke();", "ActiveSessionChanged?.Invoke(); var l = LockDocument();")]
    public void G1_ItTripsOnAHandlerThatRunsTheIdlePassNotifiesTheDrainPostsOrReads(string from, string to)
    {
        Assert.NotEmpty(HandlerViolations(Mutate(Bridge(), from, to)));
    }

    [Theory]
    [InlineData("_core.OnDocumentCreated(NativeId(e.Document))", "_core.OnDocumentCreated(NativeId(e.Document)); RunIdleContinuation()")]
    [InlineData("ActiveSessionChanged?.Invoke();", "ActiveSessionChanged?.Invoke(); RunIdleContinuation();")]
    [InlineData("ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));", "ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source)); RunIdleContinuation();")]
    public void G1_ItTripsOnAHandlerOrACaseBlockThatNamesTheIntermediateMethodOfTheContinuation(string from, string to)
    {
        Assert.Equal("RunIdleContinuation", IntermediateName(Bridge()));
        Assert.Empty(HandlerViolations(Bridge()));
        Assert.NotEmpty(HandlerViolations(Mutate(Bridge(), from, to)));
    }

    // ================================================================== G-2: OnIdleTick only schedules; OnIdle is the pass and only the continuation names it

    private static IEnumerable<string> TickAndPassViolations(string bridge)
    {
        var tick = TickBody(bridge);
        if (!tick.Contains("_idleScheduler.OnIdleTick()", StringComparison.Ordinal)) yield return "OnIdleTick does not call _idleScheduler.OnIdleTick()";
        foreach (Match match in Regex.Matches(tick, @"_core\.OnIdle|Drained|OnIdle\s*\(|LockDocument|StartTransaction|BeginInvoke"))
            yield return "OnIdleTick contains " + match.Value;

        // T2C 1: the intermediate method is named exactly twice (its declaration and the argument of the scheduler), never in the tick.
        var intermediate = IntermediateName(bridge);
        if (intermediate != null)
        {
            if (Named(intermediate).IsMatch(tick)) yield return "OnIdleTick names " + intermediate;
            var intermediateNamed = Named(intermediate).Matches(bridge).Count;
            if (intermediateNamed != 2) yield return intermediate + " is named " + intermediateNamed + " times, expected 2 (declaration and the scheduler)";
        }

        // T2C 1: the only members of the scheduler that the bridge calls are OnIdleTick and SetPost.
        var members = Regex.Matches(bridge, @"_idleScheduler\s*\.\s*(\w+)").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToArray();
        if (!members.SequenceEqual(new[] { "OnIdleTick", "SetPost" })) yield return "the bridge calls the scheduler members: " + string.Join(", ", members);

        // OnIdle without Tick is named exactly twice: its declaration and the continuation.
        var named = OnIdleWithoutTick.Matches(bridge).Count;
        if (named != 2) yield return "OnIdle is named " + named + " times, expected 2 (declaration and continuation)";

        var pass = OnIdleBody(bridge);
        // T2C 1: ".OnIdle(" with ANY receiver appears once (an alias of the core does not escape the count).
        var coreCalls = Regex.Matches(bridge, @"\.OnIdle\s*\(").Count;
        if (coreCalls != 1) yield return ".OnIdle( appears " + coreCalls + " times, expected 1";
        if (!Regex.IsMatch(pass, @"_core\.OnIdle\s*\(")) yield return "_core.OnIdle( is not inside OnIdle";

        var announced = Regex.Matches(bridge, @"Drained\?\.Invoke\(").Count;
        if (announced != 1) yield return "Drained?.Invoke( appears " + announced + " times, expected 1";
        if (!pass.Contains("Drained?.Invoke(", StringComparison.Ordinal)) yield return "Drained?.Invoke( is not inside OnIdle";
    }

    [Fact]
    public void G2_OnIdleTickOnlySchedulesAndTheIdlePassIsNamedOnlyByItsDeclarationAndTheContinuation()
    {
        Assert.Empty(TickAndPassViolations(Bridge()));
    }

    [Theory]
    [InlineData("_idleScheduler.OnIdleTick();", "_core.OnIdle();")]
    [InlineData("_idleScheduler.OnIdleTick();", "_idleScheduler.OnIdleTick(); OnIdle();")]
    [InlineData("_idleScheduler.OnIdleTick();", "_idleScheduler.OnIdleTick(); Drained?.Invoke(default);")]
    [InlineData("_idleScheduler.OnIdleTick();", "_idleScheduler.OnIdleTick(); _dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { }));")]
    [InlineData("_idleScheduler.OnIdleTick();", "")]
    public void G2_ItTripsWhenTheTickDoesMoreThanScheduleOrNothing(string from, string to)
    {
        Assert.NotEmpty(TickAndPassViolations(Mutate(Bridge(), from, to)));
    }

    [Theory]
    [InlineData("\nprivate void Extra() { OnIdle(); }")]
    [InlineData("\nprivate void Extra() { _core.OnIdle(); }")]
    [InlineData("\nprivate void Extra() { Drained?.Invoke(default); }")]
    public void G2_ItTripsWhenAnotherMemberNamesTheIdlePassOrAnnouncesTheDrain(string appended)
    {
        Assert.NotEmpty(TickAndPassViolations(Bridge() + appended));
    }

    [Theory]
    [InlineData("\nprivate void Extra() { RunIdleContinuation(); }")]
    [InlineData("\nprivate void Extra() { var core = _core; core.OnIdle(); }")]
    [InlineData("\nprivate void Extra() { _idleScheduler.Reset(); }")]
    [InlineData("\nprivate void Extra() { _idleScheduler.Run(); }")]
    public void G2_ItTripsOnAnAliasOfTheCoreAnExtraNameOfTheIntermediateMethodOrAnExtraMemberOfTheScheduler(string appended)
    {
        Assert.Empty(TickAndPassViolations(Bridge()));
        Assert.NotEmpty(TickAndPassViolations(Bridge() + appended));
    }

    [Fact]
    public void G2_ItTripsWhenTheTickNamesTheIntermediateMethod()
    {
        Assert.NotEmpty(TickAndPassViolations(Mutate(Bridge(), "_idleScheduler.OnIdleTick();", "_idleScheduler.OnIdleTick(); RunIdleContinuation();")));
    }

    // ================================================================== G-3: the post is BeginInvoke at background priority, and the exception of the file is exact

    // The expression of the exception by exact file of NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType for the bridge (D-T2C-04):
    // it stops forbidding Dispatcher and System.Windows.Threading and keeps forbidding the rest, plus every other way of running or
    // waiting on the dispatcher. The guard fixes this text literally; a copy of it is proved here against injected text.
    private const string BridgeExceptionPattern =
        @"Autodesk\.AutoCAD\.Internal|PaletteSet|AddVisual|RackCad\.UI|Autodesk\.AutoCAD\.Windows|\bUserControl\b|System\.Windows(?!\.Threading\b)|\w*Timer\b|DispatcherFrame|PushFrame|\bYield\b|InvokeAsync|(?:_dispatcher|CurrentDispatcher)\s*\.\s*Invoke\b";

    private static readonly Regex BackgroundBeginInvoke = new(
        @"BeginInvoke\s*\(\s*(?:DispatcherPriority\.Background\s*,\s*[^,()]+|[^,()]+\s*,\s*DispatcherPriority\.Background)\s*\)");

    // I-64 F4-T3A (T2C 2): the argument of SetPost, flattened, is exactly this lambda, in one of the two orders of the overload.
    private static readonly string[] ExactPostArguments =
    {
        "action=>_dispatcher.BeginInvoke(DispatcherPriority.Background,action)",
        "action=>_dispatcher.BeginInvoke(action,DispatcherPriority.Background)",
    };

    private static string? PostArgument(string bridge)
    {
        var match = Regex.Match(bridge, @"\.SetPost\s*\((?<arg>[^;]*)\)\s*;");
        return match.Success ? Squash(match.Groups["arg"].Value) : null;
    }

    private static IEnumerable<string> PostViolations(string bridge)
    {
        var begins = Regex.Matches(bridge, @"\bBeginInvoke\b").Count;
        if (begins != 1) yield return "BeginInvoke appears " + begins + " times, expected 1";
        if (!BackgroundBeginInvoke.IsMatch(bridge)) yield return "the post is not BeginInvoke with DispatcherPriority.Background";
        if (Regex.IsMatch(bridge, @"DispatcherPriority\.(?!Background\b)")) yield return "another priority is named";

        var argument = PostArgument(bridge);
        if (argument == null || !ExactPostArguments.Contains(argument, StringComparer.Ordinal)) yield return "the argument of SetPost is not the exact post: " + argument;

        // T2C 2: no other way to run the continuation in the application context or to run the dispatcher.
        foreach (Match match in Regex.Matches(bridge, @"ExecuteInApplicationContext|Dispatcher\.Run\s*\("))
            yield return "the bridge names " + match.Value;
    }

    [Fact]
    public void G3_ThePostIsExactlyBeginInvokeWithDispatcherPriorityBackground()
    {
        Assert.Empty(PostViolations(Bridge()));
    }

    [Theory]
    [InlineData("BeginInvoke(DispatcherPriority.Background, action)", "BeginInvoke(DispatcherPriority.Normal, action)")]
    [InlineData("BeginInvoke(DispatcherPriority.Background, action)", "Invoke(DispatcherPriority.Background, action)")]
    [InlineData("BeginInvoke(DispatcherPriority.Background, action)", "BeginInvoke(action)")]
    [InlineData("BeginInvoke(DispatcherPriority.Background, action)", "BeginInvoke(DispatcherPriority.Background, action)); _dispatcher.BeginInvoke(DispatcherPriority.Background, action")]
    public void G3_ItTripsOnAPostThatIsNotBeginInvokeAtBackgroundPriority(string from, string to)
    {
        Assert.NotEmpty(PostViolations(Mutate(Bridge(), from, to)));
    }

    [Theory]
    [InlineData("_idleScheduler.SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action));", "_idleScheduler.SetPost(a => _dispatcher.BeginInvoke(DispatcherPriority.Background, a));")]
    [InlineData("_idleScheduler.SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action));", "_idleScheduler.SetPost(action => { _dispatcher.BeginInvoke(DispatcherPriority.Background, action); });")]
    [InlineData("_idleScheduler.SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action));", "_idleScheduler.SetPost(action => Post(action));")]
    public void G3_ItTripsWhenTheArgumentOfSetPostIsNotTheExactPost(string from, string to)
    {
        Assert.Empty(PostViolations(Bridge()));
        Assert.NotEmpty(PostViolations(Mutate(Bridge(), from, to)));
    }

    [Fact]
    public void G3_TheOtherOrderOfTheOverloadIsAnExactPostToo()
    {
        var swapped = Mutate(Bridge(), "BeginInvoke(DispatcherPriority.Background, action)", "BeginInvoke(action, DispatcherPriority.Background)");

        Assert.Empty(PostViolations(swapped));
    }

    [Theory]
    [InlineData("ExecuteInApplicationContext(() => { }, null);")]
    [InlineData("Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.ExecuteInApplicationContext(x, null);")]
    [InlineData("Dispatcher.Run();")]
    [InlineData("Dispatcher.CurrentDispatcher.Run ();")]
    public void G3_ItTripsOnAnotherWayToRunTheContinuationInTheApplicationContextOrToRunTheDispatcher(string injected)
    {
        Assert.Empty(PostViolations(Bridge()));
        Assert.NotEmpty(PostViolations(Bridge() + "\n" + injected));
    }

    [Fact]
    public void G3_BothOrdersOfTheOverloadAreRecognizedAsTheBackgroundPost()
    {
        Assert.Matches(BackgroundBeginInvoke, "_dispatcher.BeginInvoke(DispatcherPriority.Background, action)");
        Assert.Matches(BackgroundBeginInvoke, "_dispatcher.BeginInvoke(action, DispatcherPriority.Background)");
        Assert.DoesNotMatch(BackgroundBeginInvoke, "_dispatcher.BeginInvoke(DispatcherPriority.Normal, action)");
    }

    [Fact]
    public void G3_TheExpressionOfTheExceptionIsFixedLiterallyInTheHostGuardFile()
    {
        var text = Src.Text(HostGuardPath);

        Assert.Contains("new Regex(@\"" + BridgeExceptionPattern + "\")", text);
        Assert.Matches(@"string\.Equals\(path,\s*BridgePath,\s*StringComparison\.Ordinal\)\s*\?\s*bridgeForbidden", text);
        // The exception of the palette and its text are the ones the metaguard fixes: nothing of them changed.
        Assert.Contains(@"string.Equals(path, PaletteHostPath, StringComparison.Ordinal)", text);
        Assert.Contains(@"new Regex(@""Autodesk\.AutoCAD\.Internal|\bDispatcher\b|\bTimer\b"")", text);
    }

    [Fact]
    public void G3_TheRealBridgePassesTheExceptionAndOnlyTheExceptionLetsItNameTheDispatcher()
    {
        var exception = new Regex(BridgeExceptionPattern);
        var common = new Regex(@"Autodesk\.AutoCAD\.Internal|PaletteSet|AddVisual|System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|\bUserControl\b|\bDispatcher\b");
        var bridge = Bridge();

        Assert.DoesNotMatch(exception, bridge);
        // The generic rule of every other file still forbids what the bridge now names: only the exact file has the exception.
        Assert.Matches(common, bridge);
        Assert.Matches(@"using\s+System\.Windows\.Threading\s*;", bridge);
    }

    [Theory]
    [InlineData("var t = new DispatcherTimer();")]
    [InlineData("var t = new System.Timers.Timer();")]
    [InlineData("Dispatcher.PushFrame(frame);")]
    [InlineData("var frame = new DispatcherFrame();")]
    [InlineData("_dispatcher.InvokeAsync(action);")]
    [InlineData("_dispatcher.Invoke(action);")]
    [InlineData("Dispatcher.CurrentDispatcher.Invoke(action);")]
    [InlineData("await Task.Yield();")]
    [InlineData("var c = new System.Windows.Controls.Button();")]
    [InlineData("var p = new PaletteSet(\"x\");")]
    [InlineData("using RackCad.UI;")]
    [InlineData("var u = new UserControl();")]
    [InlineData("using Autodesk.AutoCAD.Internal;")]
    public void G3_ACopyOfTheExpressionTripsOnEachInjectedText(string injected)
    {
        var copy = new Regex(BridgeExceptionPattern);

        Assert.Matches(copy, injected);
        Assert.Matches(copy, Bridge() + "\n" + injected);
    }

    [Theory]
    [InlineData("using System.Windows.Threading;")]
    [InlineData("private readonly Dispatcher _dispatcher;")]
    [InlineData("_dispatcher = Dispatcher.CurrentDispatcher;")]
    [InlineData("_idleScheduler.SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action));")]
    public void G3_ACopyOfTheExpressionLetsTheBridgeNameTheDispatcherAndPostWithBeginInvoke(string allowed)
    {
        Assert.DoesNotMatch(new Regex(BridgeExceptionPattern), allowed);
    }

    // ================================================================== G-4: the scheduler is pure and keeps its order

    private static readonly Regex SchedulerForbidden = new(
        @"Autodesk|System\.Windows|Dispatcher|\bTask\b|\basync\b|\bawait\b|\bThread\b|Timer|RackLog|\bevent\b|\bstatic\b");

    private static readonly Regex PureCodeTraps = new(@"Transaction|\bDatabase\b|LockDocument|\bCommit\(");

    private static readonly Regex PureCodeNamesTraps = new(@"Provider|Metric|Aggregat|\bBom|TotalRacks|ProjectSummary", RegexOptions.IgnoreCase);

    private static IEnumerable<string> SchedulerViolations(string code)
    {
        foreach (Match match in SchedulerForbidden.Matches(code)) yield return "the scheduler names " + match.Value;
        foreach (Match match in PureCodeTraps.Matches(code)) yield return "the scheduler names " + match.Value;
        foreach (Match match in PureCodeNamesTraps.Matches(code)) yield return "the scheduler names " + match.Value;

        var calls = Regex.Matches(code, @"\bpost\s*\(").Count;
        if (calls != 1) yield return "post( is called " + calls + " times, expected 1";

        var tick = Src.BodyOf(code, @"public\s+void\s+OnIdleTick\s*\(\s*\)");
        var scheduled = tick.IndexOf("if (_scheduled) return;", StringComparison.Ordinal);
        var noPost = tick.IndexOf("post == null", StringComparison.Ordinal);
        var check = tick.IndexOf("_canRunNow()", StringComparison.Ordinal);
        var call = Regex.Match(tick, @"\bpost\s*\(").Index;
        if (scheduled < 0 || noPost < 0 || check < 0 || !Regex.IsMatch(tick, @"\bpost\s*\(")) yield return "OnIdleTick lacks one of its three checks or the post";
        else if (!(scheduled < noPost && noPost < check && check < call)) yield return "the post is not behind the three checks, in order";

        var run = Src.BodyOf(code, @"private\s+void\s+Run\s*\(\s*\)");
        var unmark = run.IndexOf("_scheduled = false;", StringComparison.Ordinal);
        var again = run.IndexOf("_canRunNow()", StringComparison.Ordinal);
        var execute = run.IndexOf("_continuation()", StringComparison.Ordinal);
        if (unmark < 0 || again < 0 || execute < 0) yield return "Run lacks the unmark, the check or the continuation";
        else if (!(unmark < again && again < execute)) yield return "Run is not unmark, then check, then continuation";
        if (Regex.IsMatch(run.Substring(run.IndexOf('{')), @"\bpost\s*\(|\bRun\s*\(")) yield return "Run schedules itself";
    }

    [Fact]
    public void G4_TheSchedulerIsPureAndTheSchedulingOrderIsKept()
    {
        Assert.Empty(SchedulerViolations(Scheduler()));
        Assert.StartsWith("#nullable enable", Src.Text(SchedulerPath).TrimStart((char)0xFEFF));
        Assert.Matches(@"\bpublic\s+sealed\s+class\s+WorkspaceIdleScheduler\b", Scheduler());
    }

    [Theory]
    [InlineData("private static int _x;")]
    [InlineData("public System.Windows.Threading.Dispatcher D;")]
    [InlineData("private Task _t;")]
    [InlineData("private System.Threading.Thread _t;")]
    [InlineData("private System.Timers.Timer _t;")]
    [InlineData("public event System.Action Changed;")]
    [InlineData("private void L() { RackLog.Exception(\"x\", null); }")]
    [InlineData("private void L() { using var t = db.TransactionManager; }")]
    [InlineData("private void L() { var x = Provider; }")]
    [InlineData("private void L() { var x = TotalRacks; }")]
    public void G4_ItTripsOnAnImpureMember(string appended)
    {
        Assert.NotEmpty(SchedulerViolations(Scheduler() + "\n" + appended));
    }

    private const string GoodScheduler =
        "public void OnIdleTick() { if (_scheduled) return; var post = _post; if (post == null) return; if (!_canRunNow()) return; _scheduled = true; post(Run); } "
        + "private void Run() { _scheduled = false; if (!_canRunNow()) return; _continuation(); }";

    [Theory]
    [InlineData("public void OnIdleTick() { if (_scheduled) return; var post = _post; if (post == null) return; _scheduled = true; post(Run); if (!_canRunNow()) return; } private void Run() { _scheduled = false; if (!_canRunNow()) return; _continuation(); }")]
    [InlineData("public void OnIdleTick() { if (!_canRunNow()) return; if (_scheduled) return; var post = _post; if (post == null) return; post(Run); } private void Run() { _scheduled = false; if (!_canRunNow()) return; _continuation(); }")]
    [InlineData("public void OnIdleTick() { if (_scheduled) return; var post = _post; if (post == null) return; if (!_canRunNow()) return; post(Run); } private void Run() { if (!_canRunNow()) return; _scheduled = false; _continuation(); }")]
    [InlineData("public void OnIdleTick() { if (_scheduled) return; var post = _post; if (post == null) return; if (!_canRunNow()) return; post(Run); } private void Run() { _scheduled = false; _continuation(); if (!_canRunNow()) return; }")]
    [InlineData("public void OnIdleTick() { if (_scheduled) return; var post = _post; if (post == null) return; if (!_canRunNow()) return; post(Run); } private void Run() { _scheduled = false; if (!_canRunNow()) return; _continuation(); post(Run); }")]
    public void G4_ItTripsWhenTheOrderOfTheChecksOrOfTheRunIsBroken(string code)
    {
        Assert.Empty(SchedulerViolations(GoodScheduler));
        Assert.NotEmpty(SchedulerViolations(code));
    }

    // ================================================================== G-5: CanDrainNow and OnIdle share the eligibility, and the query mutates nothing

    private static readonly Regex AnyFieldAssignment = new(@"(?<![\w.])_\w+\s*=(?![=>])");

    private static readonly Regex CloseAssignment = new(@"_lastAllowedClose\s*=(?!=)");

    private static IEnumerable<string> EligibilityViolations(string core)
    {
        var can = Src.BodyOf(core, @"public\s+bool\s+CanDrainNow\s*\(\s*\)");
        var pass = Src.BodyOf(core, @"public\s+IReadOnlyList<DrainOutcome>\s+OnIdle\s*\(\s*\)");

        var shared = Regex.Matches(core, @"private\s+bool\s+(\w+)\s*\(\s*out\s+SessionId\b").Cast<Match>().ToList();
        if (shared.Count != 1) { yield return "the eligibility method is declared " + shared.Count + " times, expected 1"; yield break; }

        var name = shared[0].Groups[1].Value;
        if (!Regex.IsMatch(can, @"\b" + name + @"\s*\(")) yield return "CanDrainNow does not call " + name;
        if (!Regex.IsMatch(pass, @"\b" + name + @"\s*\(")) yield return "OnIdle does not call " + name;

        foreach (Match match in Regex.Matches(can, @"TryDrain|EnqueueHint|SyncIdle|_source\."))
            yield return "CanDrainNow contains " + match.Value;
        if (AnyFieldAssignment.IsMatch(can)) yield return "CanDrainNow assigns a field";
        if (CloseAssignment.IsMatch(can)) yield return "CanDrainNow assigns the close sequence";
        if (!Regex.IsMatch(can, @"_lastAllowedClose\.HasValue")) yield return "CanDrainNow lacks its own condition, _lastAllowedClose.HasValue";
        if (can.Contains("_states", StringComparison.Ordinal)) yield return "CanDrainNow names _states";
        if (Regex.Matches(can, @"_lastAllowedClose").Count != 1) yield return "CanDrainNow names _lastAllowedClose more than once";

        var eligibility = Src.BodyOf(core, @"private\s+bool\s+" + name + @"\s*\(");
        if (AnyFieldAssignment.IsMatch(eligibility)) yield return "the eligibility method assigns a field";
        if (Regex.IsMatch(eligibility, @"_lastAllowedClose")) yield return "the eligibility is not the place of the close condition";
        if (!pass.Contains("TryDrain(", StringComparison.Ordinal)) yield return "OnIdle no longer calls TryDrain";
    }

    [Fact]
    public void G5_CanDrainNowAndTheIdlePassShareTheEligibilityAndTheQueryMutatesNothing()
    {
        Assert.Empty(EligibilityViolations(Core()));
    }

    [Theory]
    [InlineData("return !_lastAllowedClose.HasValue;", "_lastAllowedClose = null; return true;")]
    [InlineData("return !_lastAllowedClose.HasValue;", "_panelVisible = true; return !_lastAllowedClose.HasValue;")]
    [InlineData("return !_lastAllowedClose.HasValue;", "return !_lastAllowedClose.HasValue && !_states.ContainsKey(default);")]
    [InlineData("return !_lastAllowedClose.HasValue;", "SyncIdle(); return !_lastAllowedClose.HasValue;")]
    [InlineData("return !_lastAllowedClose.HasValue;", "return true;")]
    [InlineData("if (!TryGetDrainable(out _, out _, out _)) return false;", "if (!_started) return false;")]
    [InlineData("if (TryGetDrainable(out var id, out var state, out var session))", "if (_active is { } id && _states.TryGetValue(id, out var state) && _registry.TryGet(id, out var session))")]
    public void G5_ItTripsOnAQueryThatMutatesDrainsOrStopsSharingTheEligibility(string from, string to)
    {
        Assert.NotEmpty(EligibilityViolations(Mutate(Core(), from, to)));
    }

    [Fact]
    public void G5_TheAssignmentRegexSeesAnAssignmentAndNotAComparison()
    {
        Assert.Matches(CloseAssignment, "_lastAllowedClose = null;");
        Assert.Matches(CloseAssignment, "_lastAllowedClose=session;");
        Assert.DoesNotMatch(CloseAssignment, "if (_lastAllowedClose == null) return false;");
        Assert.DoesNotMatch(CloseAssignment, "return !_lastAllowedClose.HasValue;");
        Assert.Matches(AnyFieldAssignment, "_panelVisible = true;");
        Assert.DoesNotMatch(AnyFieldAssignment, "if (_started == false) return;");
        Assert.DoesNotMatch(AnyFieldAssignment, "var f = x => _active;");
    }

    // ================================================================== G-6: no new += or -= anywhere

    private static IEnumerable<string> SubscriptionViolations(string bridge, string controller, string scheduler)
    {
        var plus = Regex.Matches(bridge, @"\+=").Count;
        var minus = Regex.Matches(bridge, @"-=").Count;
        if (plus != 16) yield return "the bridge has " + plus + " += (expected 16)";
        if (minus != 16) yield return "the bridge has " + minus + " -= (expected 16)";
        var handlers = Regex.Matches(bridge, @"\bhandler\s*=\s*\(").Count;
        if (handlers != 16) yield return "the bridge has " + handlers + " handlers (expected 16)";
        var controllerPlus = Regex.Matches(controller, @"\+=").Count;
        if (controllerPlus != 3) yield return "the controller has " + controllerPlus + " += (expected 3)";
        if (Regex.IsMatch(scheduler, @"\bevent\b|\+=|-=")) yield return "the scheduler declares or hooks an event";
    }

    [Fact]
    public void G6_TheBridgeAndTheControllerAddNoSubscriptionAndTheSchedulerDeclaresNoEvent()
    {
        Assert.Empty(SubscriptionViolations(Bridge(), Controller(), Scheduler()));
    }

    [Theory]
    [InlineData("bridge", "x += 1;")]
    [InlineData("bridge", "AcApplication.Idle += OnIdleTick;")]
    [InlineData("bridge", "x -= 1;")]
    [InlineData("controller", "_count += 1;")]
    [InlineData("scheduler", "public event System.Action Changed;")]
    [InlineData("scheduler", "private void L() { _a += 1; }")]
    public void G6_ItTripsOnANewSubscriptionOperatorOrEvent(string where, string appended)
    {
        var bridge = Bridge();
        var controller = Controller();
        var scheduler = Scheduler();
        if (where == "bridge") bridge += "\n" + appended;
        else if (where == "controller") controller += "\n" + appended;
        else scheduler += "\n" + appended;

        Assert.NotEmpty(SubscriptionViolations(bridge, controller, scheduler));
    }

    // ================================================================== G-7: the Debug check of INV-14 only logs

    private static IEnumerable<string> DebugCheckViolations(string bridge)
    {
        var signature = @"\[Conditional\(""DEBUG""\)\]\s*private\s+static\s+void\s+CheckNoOpenTransactionAfterTheDrain\s*\(\s*\)";
        if (!Regex.IsMatch(bridge, signature)) { yield return "the Debug check is not a [Conditional(\"DEBUG\")] method"; yield break; }

        var body = Src.BodyOf(bridge, signature);
        if (!body.Contains("TransactionManager.TopTransaction", StringComparison.Ordinal)) yield return "the check does not read TransactionManager.TopTransaction";
        if (!body.Contains("LogFault(", StringComparison.Ordinal)) yield return "the check does not log with LogFault";
        if (Regex.IsMatch(body, @"\.Database\b")) yield return "the check reaches the database instead of the document";
        foreach (Match match in Regex.Matches(body, @"Debug\.(?:Assert|Fail)|RackLog\.Warning"))
            yield return "the check contains " + match.Value;

        foreach (Match match in Regex.Matches(bridge, @"Debug\.(?:Assert|Fail)")) yield return "the bridge contains " + match.Value;
        if (Regex.Matches(bridge, @"RackLog\.Warning").Count > 0) yield return "the bridge logs a measurement as a warning";
    }

    [Fact]
    public void G7_TheDebugCheckIsConditionalReadsTheTopTransactionOfTheDocumentAndOnlyLogs()
    {
        Assert.Empty(DebugCheckViolations(Bridge()));
    }

    [Theory]
    [InlineData("document?.TransactionManager.TopTransaction", "document?.Database.TransactionManager.TopTransaction")]
    [InlineData("document?.TransactionManager.TopTransaction", "document?.TransactionManager.GetObject()")]
    [InlineData("LogFault(\"AutoCadEventBridge.IdleContinuation.OpenTransaction\", new InvalidOperationException(", "Debug.Assert(open == null, \"x\"); LogFault(\"AutoCadEventBridge.IdleContinuation.OpenTransaction\", new InvalidOperationException(")]
    [InlineData("if (open != null)", "Debug.Fail(\"x\"); if (open != null)")]
    [InlineData("if (open != null)", "RackLog.Warning(\"x\"); if (open != null)")]
    public void G7_ItTripsOnADebugCheckThatReachesTheDatabaseOrAsserts(string from, string to)
    {
        Assert.NotEmpty(DebugCheckViolations(Mutate(Bridge(), from, to)));
    }

    [Fact]
    public void G7_ItTripsOnADebugCheckThatIsNotConditional()
    {
        var unconditional = Regex.Replace(Bridge(), @"\[Conditional\(""DEBUG""\)\]\s*(?=private\s+static\s+void\s+CheckNoOpenTransactionAfterTheDrain)", string.Empty);

        Assert.NotEqual(Bridge(), unconditional);
        Assert.NotEmpty(DebugCheckViolations(unconditional));
    }

    [Fact]
    public void G7_TheBridgeAsAWholeHasNoAssertion()
    {
        Assert.DoesNotMatch(new Regex(@"Debug\.Assert|Debug\.Fail"), Bridge());
        Assert.NotEmpty(DebugCheckViolations(Bridge() + "\nprivate void X() { Debug.Assert(true); }"));
    }

    // ================================================================== G-8: no API of writing in the three product files

    private static readonly Regex WriteApi = new(
        @"OpenMode\.ForWrite|UpgradeOpen|\bCommit\s*\(|SetImpliedSelection|SetCurrentView|SetSystemVariable|AppendEntity|ExtensionDictionary|RackEmbedStore|RackEmbedComposer|RackBlockData\.Write|new\s+RackEmbedDocument|\.Erase\s*\(|\.Write[A-Z]\w*\s*\(|File\.Write|\bSave\s*\(");

    private static IEnumerable<string> WriteViolations(IEnumerable<(string Path, string Code)> files)
    {
        foreach (var (path, code) in files)
            foreach (Match match in WriteApi.Matches(code))
                yield return path + " names " + match.Value;
    }

    private static IEnumerable<(string Path, string Code)> ProductFilesOfTheChange() => new[]
    {
        (SchedulerPath, Scheduler()),
        (CorePath, Core()),
        (BridgePath, Bridge()),
    };

    [Fact]
    public void G8_TheThreeProductFilesOfTheChangeNameNoApiOfWriting()
    {
        Assert.Empty(WriteViolations(ProductFilesOfTheChange()));
    }

    [Theory]
    [InlineData("var m = OpenMode.ForWrite;")]
    [InlineData("obj.UpgradeOpen();")]
    [InlineData("tr.Commit();")]
    [InlineData("ed.SetImpliedSelection(ids);")]
    [InlineData("ed.SetCurrentView(view);")]
    [InlineData("db.SetSystemVariable(\"X\", 1);")]
    [InlineData("btr.AppendEntity(e);")]
    [InlineData("var d = RackEmbedStore.Open(x);")]
    public void G8_ItTripsOnAnyApiOfWritingInAnyOfTheThreeFiles(string injected)
    {
        foreach (var index in new[] { 0, 1, 2 })
        {
            var files = ProductFilesOfTheChange().ToList();
            files[index] = (files[index].Path, files[index].Code + "\n" + injected);
            Assert.NotEmpty(WriteViolations(files));
        }
    }

    // ================================================================== G-9: the checks of INV-14 are in place

    private static IEnumerable<string> TransactionCheckViolations(string controller, string bridge)
    {
        var drained = Src.BodyOf(controller, @"private\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        if (!drained.Contains("AssertNoOpenTransaction()", StringComparison.Ordinal)) yield return "the controller no longer calls AssertNoOpenTransaction in OnDrained";
        if (!Regex.IsMatch(controller, @"\[Conditional\(""DEBUG""\)\]\s*private\s+static\s+void\s+AssertNoOpenTransaction\s*\(")) yield return "AssertNoOpenTransaction is no longer a Debug-only method";

        var continuation = ContinuationBody(bridge);
        var pass = Regex.Match(continuation, @"(?<![\w.])OnIdle\b");
        var check = continuation.IndexOf("CheckNoOpenTransactionAfterTheDrain()", StringComparison.Ordinal);
        if (!pass.Success) yield return "the continuation does not run OnIdle";
        else if (check < 0) yield return "the continuation does not call the Debug check";
        else if (check < pass.Index) yield return "the Debug check is before OnIdle in the continuation";
    }

    [Fact]
    public void G9_TheControllerKeepsItsCheckAndTheContinuationRunsTheDebugCheckAfterTheIdlePass()
    {
        Assert.Empty(TransactionCheckViolations(Controller(), Bridge()));
    }

    [Theory]
    [InlineData("controller", "AssertNoOpenTransaction();", "")]
    [InlineData("bridge", "CheckNoOpenTransactionAfterTheDrain();", "")]
    [InlineData("bridge", "Contain(\"IdleContinuation.Pass\", OnIdle);", "")]
    public void G9_ItTripsWhenACheckIsMissing(string where, string from, string to)
    {
        var controller = Controller();
        var bridge = Bridge();
        if (where == "controller") controller = Mutate(controller, from, to);
        else
        {
            // The call of the Debug check also appears in the declaration of the continuation only once: remove just the call in it.
            var continuation = ContinuationBody(bridge);
            bridge = bridge.Replace(continuation, Mutate(continuation, from, to));
        }

        Assert.NotEmpty(TransactionCheckViolations(controller, bridge));
    }

    [Fact]
    public void G9_ItTripsWhenTheDebugCheckComesBeforeTheIdlePass()
    {
        var bridge = Bridge();
        var continuation = ContinuationBody(bridge);
        var reversed = continuation
            .Replace("Contain(\"IdleContinuation.Pass\", OnIdle);", "@@PASS@@")
            .Replace("CheckNoOpenTransactionAfterTheDrain();", "Contain(\"IdleContinuation.Pass\", OnIdle);")
            .Replace("@@PASS@@", "CheckNoOpenTransactionAfterTheDrain();");

        Assert.NotEqual(continuation, reversed);
        Assert.NotEmpty(TransactionCheckViolations(Controller(), bridge.Replace(continuation, reversed)));
    }

    // ================================================================== T2C 4: the documentation and the logging contexts of the bridge

    [Fact]
    public void T2C4_TheDocumentationOfTheClassNamesTheSchedulerAndThePost()
    {
        var text = Src.Text(BridgePath);
        var classAt = text.IndexOf("public sealed class AutoCadEventBridge", StringComparison.Ordinal);
        Assert.True(classAt > 0);
        var summaryAt = text.LastIndexOf("<summary>", classAt, StringComparison.Ordinal);
        var doc = text.Substring(summaryAt, classAt - summaryAt);

        Assert.Contains("WorkspaceIdleScheduler", doc);
        Assert.Contains("BeginInvoke", doc);
        Assert.Contains("background priority", doc);
    }

    [Fact]
    public void T2C4_TheFailureOfThePassAndTheDebugNoticeUseDifferentLoggingContexts()
    {
        var bridge = Bridge();

        Assert.Equal(1, Src.Count(bridge, @"Contain\s*\(\s*""IdleContinuation\.Pass""\s*,\s*OnIdle\s*\)"));
        Assert.Equal(1, Src.Count(bridge, @"LogFault\s*\(\s*""AutoCadEventBridge\.IdleContinuation\.OpenTransaction"""));
        // The context that the two of them used to share is gone, and the contexts are not the same text.
        Assert.Equal(0, Src.Count(bridge, @"""(?:AutoCadEventBridge\.)?IdleContinuation"""));
        Assert.NotEqual("AutoCadEventBridge.IdleContinuation.Pass", "AutoCadEventBridge.IdleContinuation.OpenTransaction");
    }

    // ================================================================== G-10: the wiring, once, in the constructor of the bridge

    private static IEnumerable<string> WiringViolations(IReadOnlyList<(string Path, string Code)> product)
    {
        var builders = product.Where(f => Regex.IsMatch(f.Code, @"\bnew\s+WorkspaceIdleScheduler\s*\(")).ToList();
        var total = product.Sum(f => Regex.Matches(f.Code, @"\bnew\s+WorkspaceIdleScheduler\s*\(").Count);
        if (total != 1) yield return "new WorkspaceIdleScheduler( appears " + total + " times in src, expected 1";
        if (builders.Any(f => f.Path != BridgePath)) yield return "the scheduler is built outside the bridge";

        var posts = product.Sum(f => Regex.Matches(f.Code, @"\.SetPost\s*\(").Count);
        if (posts != 1) yield return ".SetPost( is called " + posts + " times in src, expected 1";
        if (product.Any(f => f.Path != BridgePath && Regex.IsMatch(f.Code, @"\.SetPost\s*\("))) yield return "SetPost is called outside the bridge";

        var bridge = product.FirstOrDefault(f => f.Path == BridgePath).Code;
        if (bridge == null) { yield return "the bridge is not in src"; yield break; }

        var constructor = Src.BodyOf(bridge, @"public\s+AutoCadEventBridge\s*\(\s*\)");
        var build = Regex.Match(constructor, @"new\s+WorkspaceIdleScheduler\s*\(\s*_core\.CanDrainNow\s*,\s*(.+?)\s*,\s*LogFault\s*\)");
        if (!build.Success) { yield return "the constructor does not build the scheduler with _core.CanDrainNow first"; }
        else
        {
            var continuation = build.Groups[1].Value;
            var contained = new Regex(@"Contain\s*\(\s*""[^""]*""\s*,\s*OnIdle\s*\)");
            if (Regex.IsMatch(continuation, @"^\w+$"))
            {
                var intermediate = Src.BodyOf(bridge, @"private\s+void\s+" + continuation + @"\s*\(\s*\)");
                if (!contained.IsMatch(intermediate)) yield return "the continuation " + continuation + " does not run OnIdle inside Contain";
            }
            else if (!contained.IsMatch(continuation))
            {
                yield return "the continuation does not run OnIdle inside Contain";
            }
        }

        var post = Regex.Match(constructor, @"\.SetPost\s*\([^;]*;");
        if (!post.Success) yield return "the constructor does not assign the post";
        else
        {
            if (!post.Value.Contains("BeginInvoke", StringComparison.Ordinal)) yield return "the post is not BeginInvoke";
            if (!post.Value.Contains("DispatcherPriority.Background", StringComparison.Ordinal)) yield return "the post is not at background priority";

            // T2C 2: and the argument is exactly the lambda of the package, in one of the two orders.
            var argument = PostArgument(constructor);
            if (argument == null || !ExactPostArguments.Contains(argument, StringComparer.Ordinal)) yield return "the argument of SetPost is not the exact post: " + argument;
        }

        if (!Regex.IsMatch(constructor, @"_dispatcher\s*=\s*Dispatcher\.CurrentDispatcher\s*;")) yield return "the dispatcher is not captured from Dispatcher.CurrentDispatcher in the constructor";
        if (Regex.Matches(bridge, @"Dispatcher\.CurrentDispatcher").Count != 1) yield return "Dispatcher.CurrentDispatcher is not named exactly once";
    }

    private static IReadOnlyList<(string Path, string Code)> RealProduct() => Src.AllProduct();

    [Fact]
    public void G10_TheSchedulerIsBuiltOnceAndThePostIsAssignedOnceBothInTheConstructorOfTheBridge()
    {
        Assert.Empty(WiringViolations(RealProduct()));
    }

    [Fact]
    public void G10_ItTripsOnASecondSchedulerOrASecondPostOutsideTheBridge()
    {
        var product = RealProduct().ToList();

        var second = product.Concat(new[] { ("src/RackCad.Plugin/Workspace/Other.cs", "var s = new WorkspaceIdleScheduler(() => true, () => { });") }).ToList();
        var secondPost = product.Concat(new[] { ("src/RackCad.Plugin/Workspace/Other.cs", "scheduler.SetPost(a => a());") }).ToList();

        Assert.NotEmpty(WiringViolations(second));
        Assert.NotEmpty(WiringViolations(secondPost));
    }

    [Theory]
    [InlineData("new WorkspaceIdleScheduler(_core.CanDrainNow,", "new WorkspaceIdleScheduler(() => true,")]
    [InlineData("_dispatcher.BeginInvoke(DispatcherPriority.Background, action)", "action()")]
    [InlineData("_dispatcher.BeginInvoke(DispatcherPriority.Background, action)", "_dispatcher.BeginInvoke(DispatcherPriority.Normal, action)")]
    [InlineData("_dispatcher = Dispatcher.CurrentDispatcher;", "_dispatcher = null;")]
    [InlineData("Contain(\"IdleContinuation.Pass\", OnIdle);", "OnIdle();")]
    [InlineData("_core.CanDrainNow, RunIdleContinuation,", "_core.CanDrainNow, OnIdle,")]
    public void G10_ItTripsWhenTheWiringOfTheConstructorIsNotTheOneOfThePackage(string from, string to)
    {
        var product = RealProduct().ToList();
        var index = product.FindIndex(f => f.Path == BridgePath);
        Assert.True(index >= 0);
        product[index] = (BridgePath, Mutate(product[index].Code, from, to));

        Assert.NotEmpty(WiringViolations(product));
    }

    [Theory]
    [InlineData("_idleScheduler.SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action));", "_idleScheduler.SetPost(a => _dispatcher.BeginInvoke(DispatcherPriority.Background, a));")]
    [InlineData("_idleScheduler.SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action));", "_idleScheduler.SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, () => action()));")]
    public void G10_ItTripsWhenTheArgumentOfSetPostIsNotTheExactLambda(string from, string to)
    {
        var product = RealProduct().ToList();
        var index = product.FindIndex(f => f.Path == BridgePath);
        Assert.Empty(WiringViolations(product));
        product[index] = (BridgePath, Mutate(product[index].Code, from, to));

        Assert.NotEmpty(WiringViolations(product));
    }

    [Fact]
    public void G10_TheContinuationMayBeALambdaOrASingleIntermediateMethodThatRunsTheIdlePassContained()
    {
        var product = RealProduct().ToList();
        var index = product.FindIndex(f => f.Path == BridgePath);
        var lambda = Mutate(product[index].Code, "_core.CanDrainNow, RunIdleContinuation,", "_core.CanDrainNow, () => Contain(\"IdleContinuation.Pass\", OnIdle),");
        product[index] = (BridgePath, lambda);

        Assert.Empty(WiringViolations(product));
    }
}
