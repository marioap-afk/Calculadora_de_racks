#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F1-T3-HOST-C2: source guards of the two corrections ordered after Smoke-1 R20261103T175351Z-0a77. The Core test project
// cannot reference the Plugin (it needs AutoCAD), so these read the sources as text, like the guards of T2, T3A, T3B and C1.
// C2-A (SM-13, SM-34): the handlers of CommandWillStart and EnteringQuiescentState raise one internal, argument-free notice
// when the core reports that the command state changed; the controller relays it as StateChanged and nothing else; the probe
// counts a command the bridge saw starting. C2-B (SM-39): a DocumentActivated without document is the explicit
// "no active document" transition of the core, never a silent return, and the active-session notice is still raised.
public class WorkspaceHostC2SourceGuardTests
{
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string CorePath = "src/RackCad.Application/Workspace/EventBridgeCore.cs";

    private static string Bridge() => Src.Code(BridgePath);

    private static string Controller() => Src.Code(ControllerPath);

    private static string Core() => Src.Code(CorePath);

    private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

    // The text of one `case BridgeSource.X:` block of the bridge, up to the next case.
    private static string CaseBlock(string code, string source)
    {
        var start = code.IndexOf("case BridgeSource." + source + ":", StringComparison.Ordinal);
        Assert.True(start >= 0, "case not found: " + source);
        var next = code.IndexOf("case BridgeSource.", start + 10, StringComparison.Ordinal);
        var end = next < 0 ? code.IndexOf("default:", start, StringComparison.Ordinal) : next;
        Assert.True(end > start);
        return code.Substring(start, end - start);
    }

    private static readonly string[] CommandStateSources = { "CommandWillStart", "EnteringQuiescentState" };

    private static readonly string[] OtherSources =
    {
        "DocumentCreated", "DocumentActivated", "DocumentToBeDestroyed", "BeginDocumentClose", "ImpliedSelectionChanged",
        "CommandEnded", "CommandCancelled", "CommandFailed", "ObjectAppended", "ObjectErased", "ObjectModified",
        "ObjectUnappended", "ObjectReappended", "Idle",
    };

    // ------------------------------------------------------------------ C2-A: the notice of the bridge

    [Fact]
    public void TheBridgeDeclaresOneInternalCommandStateNoticeWithoutArguments()
    {
        var code = Bridge();

        Assert.Equal(1, Src.Count(code, @"\binternal\s+event\s+Action\s+CommandStateChanged\s*;"));
        Assert.DoesNotMatch(new Regex(@"public\s+event\s+\w+\s+CommandStateChanged"), code);
        Assert.DoesNotMatch(new Regex(@"event\s+Action<[^>]*>\s+CommandStateChanged"), code);
    }

    [Fact]
    public void TheNoticeIsRaisedOnlyFromTheTwoCommandStateHandlersAndOnlyWhenTheCoreReportsAChange()
    {
        var code = Bridge();

        foreach (var source in CommandStateSources)
        {
            var block = CaseBlock(code, source);
            var contain = block.IndexOf("ContainSession(a,", StringComparison.Ordinal);
            var decision = Regex.Match(block, @"if\s*\(\s*_core\.OnSourceEvent\s*\(\s*a\.Session\s*,\s*a\.Source\s*\)\s*\)\s*RaiseCommandStateChanged\s*\(\s*\)\s*;");
            var hook = block.IndexOf("." + source + " +=", StringComparison.Ordinal);
            Assert.True(contain >= 0, source + ": the handler is not contained per session");
            Assert.True(decision.Success, source + ": the notice is not raised on the answer of the core");
            Assert.True(contain < decision.Index && decision.Index < hook, source + ": the notice is not raised inside the contained handler");
        }

        foreach (var source in OtherSources)
            Assert.DoesNotContain("CommandStateChanged", CaseBlock(code, source));

        // Two raises (one per command-state handler), one invocation inside the contained raise, and no other member names it.
        Assert.Equal(2, Src.Count(code, @"\bRaiseCommandStateChanged\s*\(\s*\)\s*;"));
        Assert.Equal(1, Src.Count(code, @"\bCommandStateChanged\s*\?\.\s*Invoke\s*\(\s*\)"));
        var elsewhere = code
            .Replace(Src.BodyOf(code, @"private\s+void\s+RaiseCommandStateChanged\s*\(\s*\)"), string.Empty)
            .Replace(CaseBlock(code, "CommandWillStart"), string.Empty)
            .Replace(CaseBlock(code, "EnteringQuiescentState"), string.Empty);
        Assert.Equal(1, Src.Count(elsewhere, @"CommandStateChanged\b"));
        Assert.Equal(1, Src.Count(elsewhere, @"\binternal\s+event\s+Action\s+CommandStateChanged\s*;"));
    }

    [Fact]
    public void RaisingTheNoticeIsContainedAndNeverDegradesTheSession()
    {
        var body = Src.BodyOf(Bridge(), @"private\s+void\s+RaiseCommandStateChanged\s*\(\s*\)");

        Assert.Contains("CommandStateChanged?.Invoke()", body);
        Assert.Contains("catch (System.Exception ex)", body);
        Assert.Contains("LogFault(\"AutoCadEventBridge.CommandStateChanged\"", body);
        Assert.DoesNotMatch(new Regex(@"ReportFault|_core\."), body);
    }

    [Fact]
    public void TheCommandStateHandlersQueueNoHintNoReconciliationNoIdleAndReadNothing()
    {
        var code = Bridge();

        foreach (var source in CommandStateSources)
        {
            var block = CaseBlock(code, source);
            Assert.DoesNotMatch(
                new Regex(@"RequestReconciliation|EnqueueHint|OnIdle|TryDrain|Drained|AcApplication\.Idle|SelectImplied|GetObject|StartTransaction|LockDocument|ActiveSessionChanged"),
                block);
        }
    }

    [Fact]
    public void TheBridgeTellsTheCommandStateOfTheActiveSessionOnlyByDelegatingToTheCore()
    {
        var code = Bridge();

        Assert.Matches(@"internal\s+bool\s+IsCommandActiveInActiveSession\s*=>\s*_core\.IsCommandActiveInActiveSession\s*;", code);
        Assert.DoesNotMatch(new Regex(@"public\s+bool\s+IsCommandActiveInActiveSession"), code);
        Assert.Equal(2, Src.Count(code, @"\bIsCommandActiveInActiveSession\b"));
    }

    // ------------------------------------------------------------------ C2-A: the relay of the controller and the probe

    [Fact]
    public void TheControllerRelaysTheCommandStateNoticeAsItsOnlyNewSubscription()
    {
        var code = Controller();
        var constructor = Src.BodyOf(code, @"private\s+WorkspaceController\s*\(\s*\)");

        Assert.Matches(@"_bridge\.CommandStateChanged\s*\+=\s*OnCommandStateChanged\s*;", constructor);
        Assert.Equal(1, Src.Count(code, @"_bridge\.CommandStateChanged\s*\+="));
        Assert.Equal(3, Src.Count(code, @"\+="));
        Assert.Equal(0, Src.Count(code, @"-="));
    }

    [Fact]
    public void ARelayedCommandStateChangeOnlyAnnouncesTheState()
    {
        var body = Src.BodyOf(Controller(), @"private\s+void\s+OnCommandStateChanged\s*\(\s*\)");
        var squashed = Squash(body);

        Assert.True(
            squashed == "privatevoidOnCommandStateChanged()=>NotifyStateChanged();"
            || squashed == "privatevoidOnCommandStateChanged(){NotifyStateChanged();}",
            "the relay does more than announce the state: " + body);
    }

    [Fact]
    public void TheProbeCountsACommandThatTheBridgeSawStartingAsACommandInCourse()
    {
        var code = Controller();
        var probe = Src.BodyOf(code, @"public\s+bool\s+IsActiveDocumentQuiescent\b");

        Assert.Contains("Editor.IsQuiescent", probe);
        Assert.Contains("!_bridge.IsCommandActiveInActiveSession", probe);
        Assert.DoesNotMatch(new Regex(@"GetSystemVariable|CMDACTIVE|SetSystemVariable|Database"), probe);
        Assert.Matches(@"new\s+HostProbe\s*\(\s*_bridge\s*\)", code);
    }

    // ------------------------------------------------------------------ C2-B: the activation without document

    [Fact]
    public void AnActivationWithoutDocumentIsTheExplicitTransitionAndStillAnnouncesTheActiveSession()
    {
        var block = CaseBlock(Bridge(), "DocumentActivated");

        var contain = block.IndexOf("Contain(\"DocumentActivated\"", StringComparison.Ordinal);
        var nullCheck = Regex.Match(block, @"if\s*\(\s*e\.Document\s*==\s*null\s*\)\s*_core\.OnNoActiveDocument\s*\(\s*\)\s*;");
        var otherwise = Regex.Match(block, @"else\s+_core\.OnDocumentActivated\s*\(\s*NativeId\s*\(\s*e\.Document\s*\)\s*,\s*e\.Document\.Editor\.IsQuiescent\s*\)\s*;");
        var raise = block.IndexOf("ActiveSessionChanged?.Invoke()", StringComparison.Ordinal);
        var hook = block.IndexOf("documents.DocumentActivated +=", StringComparison.Ordinal);

        Assert.True(nullCheck.Success, "a DocumentActivated without document is not routed to OnNoActiveDocument");
        Assert.True(otherwise.Success, "a DocumentActivated with a document no longer activates its session");
        Assert.True(contain >= 0 && contain < nullCheck.Index && nullCheck.Index < otherwise.Index && otherwise.Index < raise && raise < hook,
            "DocumentActivated: the transition is not taken inside Contain before the notice");

        // Never a silent `if (e.Document == null) return;`: the handler body has no return at all, so the notice always follows.
        var handlerBody = block.Substring(contain, hook - contain);
        Assert.DoesNotMatch(new Regex(@"\breturn\b"), handlerBody);
        Assert.Equal(1, Src.Count(block, @"\bNativeId\s*\(\s*e\.Document\s*\)"));
    }

    [Fact]
    public void TheCorrectionsAddNoAutoCadSubscriptionNorHandler()
    {
        var code = Bridge();

        Assert.Equal(16, Src.Count(code, @"\+="));
        Assert.Equal(16, Src.Count(code, @"-="));
        Assert.Equal(16, Src.Count(code, @"\bhandler\s*=\s*\("));
    }

    // ------------------------------------------------------------------ the pure core

    [Fact]
    public void TheCommandStateCasesOfTheCoreOnlyFlipTheInMemoryFlag()
    {
        var code = Core();
        var handler = Src.BodyOf(code, @"public\s+bool\s+OnSourceEvent\s*\(\s*SessionId\s+\w+\s*,\s*BridgeSource\s+\w+\s*\)");
        var flip = Src.BodyOf(code, @"private\s+static\s+bool\s+SetQuiescent\s*\(\s*SessionState\s+\w+\s*,\s*bool\s+\w+\s*\)");

        Assert.Matches(@"case\s+BridgeSource\.CommandWillStart\s*:\s*return\s+SetQuiescent\s*\(\s*state\s*,\s*false\s*\)\s*;", handler);
        Assert.Matches(@"case\s+BridgeSource\.EnteringQuiescentState\s*:\s*return\s+SetQuiescent\s*\(\s*state\s*,\s*true\s*\)\s*;", handler);
        Assert.DoesNotMatch(new Regex(@"EnqueueHint|SyncIdle|_source\.|TryDrain|_active|_registry|_log"), flip);
    }

    [Fact]
    public void TheNoActiveDocumentTransitionOfTheCoreOnlyClearsTheActiveSession()
    {
        var body = Src.BodyOf(Core(), @"public\s+void\s+OnNoActiveDocument\s*\(\s*\)");

        Assert.Contains("_started", body);
        Assert.Matches(@"_active\s*=\s*null\s*;", body);
        Assert.DoesNotMatch(new Regex(@"Destroy|Detach|_source\.|EnqueueHint|SyncIdle|GetOrCreate|_states\.|_byNative\.|_registry\.|Stop\s*\("), body);
    }

    [Fact]
    public void TheCoreTellsTheCommandStateOnlyForAHealthyActiveSession()
    {
        var body = Src.BodyOf(Core(), @"public\s+bool\s+IsCommandActiveInActiveSession\b");

        Assert.Matches(@"IsCommandActiveInActiveSession\s*=>", body);
        Assert.Contains("_started", body);
        Assert.Contains("_active", body);
        Assert.Contains("Degraded", body);
        Assert.Contains("Quiescent", body);
        Assert.DoesNotMatch(new Regex(@"EnqueueHint|SyncIdle|_source\.|TryDrain|_registry\.|GetOrCreate"), body);
    }

    [Fact]
    public void TheCoreStillHasNoPublicEventAndNamesNothingOfAutoCad()
    {
        var code = Core();

        Assert.DoesNotMatch(new Regex(@"\bpublic\s+event\b"), code);
        Assert.DoesNotMatch(new Regex(@"\bAutodesk\.|RackCad\.Plugin|RackCad\.UI|System\.Windows"), code);
    }

    // ------------------------------------------------------------------ 7 of C2-B: no registry, profile or file write

    private static readonly Regex PersistenceOrAuthoredWrite = new(
        @"System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\.|\bUserSettings|\bAtomicFile\b|"
        + @"NamedObjectsDictionary|GetNamedObjectsDictionary|\bDBDictionary\b|\bXrecord\b|\bXData\b|\bXdata\b|\bResultBuffer\b|"
        + @"AppendEntity|UpgradeOpen|DowngradeOpen|\.Commit\(|\.Erase\(|SetXDictionary|SaveAs|\bSave\(|DocumentExtension|"
        + @"SetSystemVariable|CPROFILE|TRUSTEDPATHS|SECURELOAD");

    [Fact]
    public void TheThreeFilesOfTheCorrectionsWriteNoRegistryProfileFileOrDrawing()
    {
        foreach (var path in new[] { BridgePath, ControllerPath, CorePath })
        {
            var code = Src.Code(path);
            Assert.True(!PersistenceOrAuthoredWrite.IsMatch(code), path + " uses " + PersistenceOrAuthoredWrite.Match(code).Value);
        }
    }
}
