#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T2B2B-LIFECYCLE (INV-F4T2B2B-01 to 04, 06 to 09, 11 and 12): source guards of the lifecycle of the drafts. The Core test project
// cannot reference the Plugin (it needs AutoCAD), so the Plugin files are read as text, like the rest of the guards of the workspace.
// These guards are ADDITIVE and live in a new file: no existing guard was changed or weakened except the two declared changes (the list of
// public members of the bridge core and the names of EditIntentStatus). Every guard fails by ASSERTION when the text it expects is
// missing. By the rule of I-1 (F4-E32, F4-E33) none of them prohibits for ever what belongs to F4-T3 or F4-T4: the hook of the live text
// before the advice of the close (a wrapper of the advisor), the exposure of the reload, the discard and the note by the port or the UI,
// the consumption of the queue of notices, the discard of ONE draft by the host, the calls to NotePilotEdit in Update, new states. They fix
// only what this task needs, for example "Veto() is called if and only if the core decides Veto", never "the handler does nothing else".
// Evidence named by the package: obligation 5 (no write: the lifecycle files name no write API), 14 (the retirement and its notice) and 18
// (no object of the host lives past the call: the lifecycle files hold no Document, Transaction or database object).
public class WorkspaceLifecycleGuardTests
{
    private const string AdvisorPath = "src/RackCad.Application/Workspace/IWorkspaceLifecycleAdvisor.cs";
    private const string BridgeCorePath = "src/RackCad.Application/Workspace/EventBridgeCore.cs";
    private const string HostCorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string IntentPath = "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs";
    private const string RegistryPath = "src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs";
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";

    private static string BridgeCore() => Src.Code(BridgeCorePath);

    private static string Host() => Src.Code(HostCorePath);

    private static string Registry() => Src.Code(RegistryPath);

    private static string Bridge() => Src.Code(BridgePath);

    private static string Controller() => Src.Code(ControllerPath);

    private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

    private static int IndexOf(string text, string needle) => text.IndexOf(needle, StringComparison.Ordinal);

    private static void AssertInOrder(string text, params string[] needles)
    {
        var previous = -1;
        foreach (var needle in needles)
        {
            var at = IndexOf(text, needle);
            Assert.True(at >= 0, "missing: " + needle);
            Assert.True(at > previous, "out of order: " + needle);
            previous = at;
        }
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-01: the interface of the advisor

    [Fact]
    public void TheAdvisorIsAPublicInterfaceOfTheWorkspaceFolderWithExactlyTheTwoMethodsOfThePackage()
    {
        var text = Src.Text(AdvisorPath);
        var code = Src.Code(AdvisorPath);

        Assert.StartsWith("#nullable enable", text.TrimStart((char)0xFEFF));
        Assert.Equal(1, Src.Count(code, @"\bnamespace\s+RackCad\.Application\.Workspace\s*\{"));
        Assert.Matches(@"\bpublic\s+interface\s+IWorkspaceLifecycleAdvisor\b", code);
        Assert.Equal(1, Src.Count(code, @"\bCloseDecision\s+AdviseClose\s*\(\s*SessionId\s+\w+\s*\)\s*;"));
        Assert.Equal(1, Src.Count(code, @"\bvoid\s+SessionDestroyed\s*\(\s*SessionId\s+\w+\s*,\s*bool\s+closeSequenceIntact\s*\)\s*;"));
        // F4-T3A (M-2): the interface is fixed by its two signatures; the count of statements ends with the file, so a later member does not
        // trip a guard that is about these two.
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-01: the pure core of the bridge

    [Fact]
    public void TheCoreAssignsTheAdvisorByTheOneNewPublicMemberAndItsConstructorHasNoPartInIt()
    {
        var code = BridgeCore();

        Assert.Matches(@"public\s+void\s+SetLifecycleAdvisor\s*\(\s*IWorkspaceLifecycleAdvisor\s+advisor\s*\)", code);
        var body = Src.BodyOf(code, @"public\s+void\s+SetLifecycleAdvisor\s*\(\s*IWorkspaceLifecycleAdvisor\s+advisor\s*\)");
        Assert.Matches(@"_advisor\s*=\s*advisor\s*\?\?\s*throw\s+new\s+ArgumentNullException\s*\(\s*nameof\s*\(\s*advisor\s*\)\s*\)", body);
        Assert.Equal(1, Src.Count(code, @"private\s+IWorkspaceLifecycleAdvisor\?\s+_advisor\s*;"));

        var constructor = Src.BodyOf(code, @"public\s+EventBridgeCore\s*\(\s*IHostEventSource\s+source\s*,\s*Action<string,\s*Exception>\?\s+logFault\s*=\s*null\s*\)");
        Assert.DoesNotMatch(new Regex(@"_advisor|_lastAllowedClose|Advisor"), constructor);
    }

    [Fact]
    public void TheCloseSequenceIsOneNullableSessionFieldThatIsOnlyAssignedNullOrTheSessionOfAnAllowedClose()
    {
        var code = BridgeCore();

        Assert.Equal(1, Src.Count(code, @"private\s+SessionId\?\s+_lastAllowedClose\s*;"));
        Assert.Equal(1, Src.Count(code, @"\b_lastAllowedClose\s*=\s*session\s*;"));
        Assert.Equal(0, Src.Count(code, @"\b_lastAllowedClose\s*=\s*(?!null\b|session\b)\w"));
        // One forgetting per entry of the core that changes its state: the twelve below and the one of the close itself.
        Assert.Equal(13, Src.Count(code, @"\b_lastAllowedClose\s*=\s*null\s*;"));
        // The memory is not part of the state of a session and forgetting never walks the sessions.
        var state = Src.BodyOf(code, @"private\s+sealed\s+class\s+SessionState\b");
        Assert.DoesNotMatch(new Regex(@"SessionId\?|Allowed|Close"), state);
        Assert.DoesNotMatch(new Regex(@"_lastAllowedClose[^;]*_states|_states[^;]*_lastAllowedClose"), code);
        // Nothing new is named after a hookup, a destruction, the idle or a stop of the host.
        foreach (var name in new[] { "_advisor", "_lastAllowedClose", "SetLifecycleAdvisor", "TellAdvisorTheSessionEnded" })
            Assert.DoesNotMatch(new Regex(@"Destroy|Detach|OnIdle|Stop"), name);
    }

    // The entries of the core that change its state, each with the signature that finds its body.
    private static readonly (string Name, string Signature)[] ForgettingEntries =
    {
        ("Start", @"public\s+void\s+Start\s*\(\s*\)"),
        ("Stop", @"public\s+void\s+Stop\s*\(\s*\)"),
        ("OnDocumentCreated", @"public\s+void\s+OnDocumentCreated\s*\(\s*long\s+\w+\s*\)"),
        ("OnDocumentActivated", @"public\s+void\s+OnDocumentActivated\s*\(\s*long\s+\w+\s*,\s*bool\s+\w+\s*\)"),
        ("OnNoActiveDocument", @"public\s+void\s+OnNoActiveDocument\s*\(\s*\)"),
        ("OnDocumentToBeDestroyed", @"public\s+void\s+OnDocumentToBeDestroyed\s*\(\s*long\s+\w+\s*\)"),
        ("OnSourceEvent", @"public\s+bool\s+OnSourceEvent\s*\(\s*SessionId\s+\w+\s*,\s*BridgeSource\s+\w+\s*\)"),
        ("OnBeginDocumentClose", @"public\s+CloseDecision\s+OnBeginDocumentClose\s*\(\s*SessionId\s+\w+\s*\)"),
        ("OnIdle", @"public\s+IReadOnlyList<DrainOutcome>\s+OnIdle\s*\(\s*\)"),
        ("ReportFault", @"public\s+void\s+ReportFault\s*\(\s*SessionId\s+\w+\s*,\s*string\s+\w+\s*,\s*Exception\s+\w+\s*\)"),
        ("SetPanelVisible", @"public\s+void\s+SetPanelVisible\s*\(\s*bool\s+\w+\s*\)"),
        ("SetModalRackCadActive", @"public\s+void\s+SetModalRackCadActive\s*\(\s*bool\s+\w+\s*\)"),
        ("RequestReconciliation", @"public\s+void\s+RequestReconciliation\s*\(\s*\)"),
    };

    private static readonly Regex StartedGuard = new(@"if\s*\(\s*!_started\s*\)\s*return[^;]*;");

    private static readonly Regex FirstBranch = new(@"\b(?:try|switch|case|return|foreach|if)\b");

    // The text of a body from the point where the entry may begin to branch: after the "not started" return where there is one.
    private static string AfterTheStartedReturn(string body)
    {
        var started = StartedGuard.Match(body);
        return started.Success ? body.Substring(started.Index + started.Length) : body.Substring(body.IndexOf('{') + 1);
    }

    // True when the body forgets (assigns null) before any other return, switch, case, branch or try.
    private static bool ForgetsBeforeAnyBranch(string body)
    {
        var rest = AfterTheStartedReturn(body);
        var forget = rest.IndexOf("_lastAllowedClose = null;", StringComparison.Ordinal);
        if (forget < 0) return false;
        var branch = FirstBranch.Match(rest);
        return !branch.Success || forget < branch.Index;
    }

    [Fact]
    public void EveryEntryThatChangesTheStateOfTheCoreForgetsTheCloseSequenceRightAfterTheNotStartedReturnAndBeforeAnyBranch()
    {
        var code = BridgeCore();

        Assert.Equal(13, ForgettingEntries.Length);
        foreach (var (name, signature) in ForgettingEntries)
        {
            var body = Src.BodyOf(code, signature);
            Assert.True(Src.Count(body, @"_lastAllowedClose\s*=\s*null\s*;") == 1, name + " does not forget exactly once");
            Assert.True(ForgetsBeforeAnyBranch(body), name + " forgets after a return, a switch, a case or a branch");
        }

        // The entries that have a "not started" return keep it first, and the case literals of the command state stay as they were.
        Assert.Matches(@"case\s+BridgeSource\.CommandWillStart\s*:\s*return\s+SetQuiescent\s*\(\s*state\s*,\s*false\s*\)\s*;", code);
        Assert.Matches(@"case\s+BridgeSource\.EnteringQuiescentState\s*:\s*return\s+SetQuiescent\s*\(\s*state\s*,\s*true\s*\)\s*;", code);
    }

    [Theory]
    [InlineData("{ if (!_started) return; if (_active is not { } id) return; _lastAllowedClose = null; }")]
    [InlineData("{ if (!_started) return false; switch (source) { case X: break; } _lastAllowedClose = null; return false; }")]
    [InlineData("{ try { _lastAllowedClose = null; } catch { } }")]
    [InlineData("{ if (!_started) return; var x = 1; }")]
    public void TheForgettingGuardDetectsAForgetAfterABranchOrNoForgetAtAll(string body)
    {
        Assert.False(ForgetsBeforeAnyBranch(body));
        Assert.True(ForgetsBeforeAnyBranch("{ if (!_started) return; _lastAllowedClose = null; if (_active is not { } id) return; }"));
    }

    [Fact]
    public void TheQueriesAndTheAssignmentOfTheAdvisorNeverForgetTheCloseSequence()
    {
        var code = BridgeCore();

        foreach (var signature in new[]
                 {
                     @"public\s+RequestDecision\s+Authorize\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*long\s+\w+\s*\)",
                     @"public\s+bool\s+TryGetSession\s*\(\s*SessionId\s+\w+\s*,\s*out\s+WorkspaceSession\?\s+\w+\s*\)",
                     @"public\s+bool\s+IsDegraded\s*\(\s*SessionId\s+\w+\s*\)",
                     @"public\s+void\s+SetLifecycleAdvisor\s*\(\s*IWorkspaceLifecycleAdvisor\s+\w+\s*\)",
                 })
            Assert.DoesNotContain("_lastAllowedClose", Src.BodyOf(code, signature));
    }

    [Fact]
    public void TheCloseHandlerOfTheCoreAsksTheAdvisorTurnsItsFailureIntoAVetoWithoutFaultAndRemembersOnlyAnAllow()
    {
        var body = Src.BodyOf(BridgeCore(), @"public\s+CloseDecision\s+OnBeginDocumentClose\s*\(\s*SessionId\s+\w+\s*\)");

        AssertInOrder(body, "_lastAllowedClose = null;", "advisor.AdviseClose(session)", "catch (Exception ex)", "Log(\"OnBeginDocumentClose\", ex);", "_lastAllowedClose = session;");
        Assert.Matches(@"if\s*\(\s*advisor\s*==\s*null\s*\)\s*return\s+CloseDecision\.Allow\s*;", body);
        Assert.Matches(@"if\s*\(\s*decision\s*==\s*CloseDecision\.Allow\s*\)\s*_lastAllowedClose\s*=\s*session\s*;", body);
        // A failure of the advisor is a Veto that is logged: it is never a fault of the session.
        Assert.Matches(@"var\s+decision\s*=\s*CloseDecision\.Veto\s*;", body);
        Assert.DoesNotMatch(new Regex(@"\bFault\s*\(|Degrade|ReportFault|_states|_registry|_source|\bthrow\b"), body);
    }

    [Fact]
    public void TheDestructionReadsTheDegradationBeforeTheStateGoesAndTellsTheAdvisorAfterTheRegistryAndTheActiveSlotAndBeforeTheNextSync()
    {
        var code = BridgeCore();
        var body = Src.BodyOf(code, @"public\s+void\s+OnDocumentToBeDestroyed\s*\(\s*long\s+\w+\s*\)");

        AssertInOrder(body, "var allowedBefore = _lastAllowedClose;", "_lastAllowedClose = null;", "!IsDegraded(id)", "_states.Remove(id)", "_registry.Destroy(id);", "if (_active == id) _active = null;", "TellAdvisorTheSessionEnded(id, sequenceIntact);", "TrySyncIdle();");
        Assert.Matches(@"var\s+sequenceIntact\s*=\s*allowedBefore\s*==\s*id\s*&&\s*!IsDegraded\s*\(\s*id\s*\)\s*;", body);

        var tell = Src.BodyOf(code, @"private\s+void\s+TellAdvisorTheSessionEnded\s*\(\s*SessionId\s+\w+\s*,\s*bool\s+closeSequenceIntact\s*\)");
        Assert.Matches(@"try\s*\{\s*advisor\.SessionDestroyed\s*\(\s*id\s*,\s*closeSequenceIntact\s*\)\s*;\s*\}\s*catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*Log\s*\(", tell);
        Assert.DoesNotMatch(new Regex(@"\bFault\s*\(|Degrade|_source|_states|_registry"), tell);
        Assert.Equal(1, Src.Count(code, @"\.SessionDestroyed\s*\("));
    }

    [Fact]
    public void TheCoreStillSubscribesToNothingAndReadsNothingOfTheDrawingWhenItAsksTheAdvisor()
    {
        var code = BridgeCore();

        Assert.DoesNotMatch(new Regex(@"\+=|-=|\bpublic\s+event\b"), code);
        Assert.DoesNotMatch(new Regex(WriteApis), code);
        Assert.DoesNotMatch(new Regex(@"\bAutodesk\.|RackCad\.Plugin|RackCad\.UI|System\.Windows"), code);
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-01: the wiring of the Plugin

    [Fact]
    public void TheBridgeForwardsTheAdvisorThroughOneInternalMethodAndAddsNoSubscriptionHandlerOrField()
    {
        var code = Bridge();

        Assert.Equal(
            "internalvoidSetLifecycleAdvisor(IWorkspaceLifecycleAdvisoradvisor)=>_core.SetLifecycleAdvisor(advisor);",
            Squash(Src.BodyOf(code, @"internal\s+void\s+SetLifecycleAdvisor\s*\(\s*IWorkspaceLifecycleAdvisor\s+advisor\s*\)")));
        Assert.DoesNotMatch(new Regex(@"public\s+void\s+SetLifecycleAdvisor"), code);
        Assert.Equal(1, Src.Count(code, @"\bIWorkspaceLifecycleAdvisor\b"));
        Assert.Equal(16, Src.Count(code, @"\+="));
        Assert.Equal(16, Src.Count(code, @"-="));
        Assert.Equal(16, Src.Count(code, @"\bhandler\s*=\s*\("));
    }

    [Fact]
    public void TheCloseHandlerKeepsItsLineAndCallsVetoIfAndOnlyIfTheCoreDecidesVeto()
    {
        var code = Bridge();
        var line = Assert.Single(code.Split('\n'), l => Regex.IsMatch(l, @"\bDocumentBeginCloseEventHandler\s+handler\s*=\s*\("));

        // The three literals that the older guards fix stay on the same line.
        Assert.Matches(@"\bhandler\s*=\s*\(", line);
        Assert.Contains("ContainSession(", line);
        Assert.Contains("_core.OnBeginDocumentClose(a.Session)", line);
        AssertInOrder(line, "_core.OnBeginDocumentClose(a.Session)", "== CloseDecision.Veto", "e.Veto()");
        Assert.Matches(@"if\s*\(\s*_core\.OnBeginDocumentClose\s*\(\s*a\.Session\s*\)\s*==\s*CloseDecision\.Veto\s*\)\s*e\.Veto\s*\(\s*\)\s*;", line);
        // Veto() is called nowhere else of the bridge, so it is called only when the core decides Veto.
        Assert.Equal(1, Src.Count(code, @"\.Veto\s*\("));
        Assert.Equal(1, Src.Count(code, @"\bDocumentBeginCloseEventHandler\b"));
    }

    [Theory]
    [InlineData("DocumentBeginCloseEventHandler handler = (s, e) => ContainSession(a, () => { _core.OnBeginDocumentClose(a.Session); e.Veto(); });")]
    [InlineData("DocumentBeginCloseEventHandler handler = (s, e) => ContainSession(a, () => { e.Veto(); if (_core.OnBeginDocumentClose(a.Session) == CloseDecision.Veto) { } });")]
    public void TheVetoGuardDetectsAVetoThatIsNotConditionalOnTheDecisionOfTheCore(string injected)
    {
        var pattern = @"if\s*\(\s*_core\.OnBeginDocumentClose\s*\(\s*a\.Session\s*\)\s*==\s*CloseDecision\.Veto\s*\)\s*e\.Veto\s*\(\s*\)\s*;";

        Assert.DoesNotMatch(pattern, injected);
        var real = Assert.Single(Bridge().Split('\n'), l => Regex.IsMatch(l, @"\bDocumentBeginCloseEventHandler\s+handler\s*=\s*\("));
        Assert.Matches(pattern, real);
    }

    [Fact]
    public void TheControllerWiresTheHostAsTheAdvisorOnceInItsConstructorAfterCreatingTheHostWithNoNewSubscription()
    {
        var code = Controller();
        var constructor = Src.BodyOf(code, @"private\s+WorkspaceController\s*\(\s*\)");

        Assert.Equal(1, Src.Count(code, @"\.SetLifecycleAdvisor\s*\("));
        AssertInOrder(constructor, "_host = new WorkspaceHostCore(", "_bridge.SetLifecycleAdvisor(_host);");
        Assert.Equal(3, Src.Count(code, @"\+="));
        Assert.Equal(0, Src.Count(code, @"-="));
        Assert.DoesNotContain(".Start(", constructor);
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-01 and 03: the explicit advisor of the host core

    [Fact]
    public void TheHostCoreImplementsTheAdvisorExplicitlySoItIsNotPartOfItsPublicSurface()
    {
        var code = Host();

        Assert.Matches(@"public\s+sealed\s+class\s+WorkspaceHostCore\s*:\s*IWorkspaceLifecycleAdvisor\b", code);
        Assert.Matches(@"CloseDecision\s+IWorkspaceLifecycleAdvisor\.AdviseClose\s*\(\s*SessionId\s+\w+\s*\)", code);
        Assert.Matches(@"void\s+IWorkspaceLifecycleAdvisor\.SessionDestroyed\s*\(\s*SessionId\s+\w+\s*,\s*bool\s+closeSequenceIntact\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"public\s+(?:CloseDecision\s+AdviseClose|void\s+SessionDestroyed)\b"), code);
        Assert.Null(typeof(WorkspaceHostCore).GetMethod("AdviseClose"));
        Assert.Null(typeof(WorkspaceHostCore).GetMethod("SessionDestroyed"));
    }

    private static string AdviseCloseBody() => Src.BodyOf(Host(), @"CloseDecision\s+IWorkspaceLifecycleAdvisor\.AdviseClose\s*\(\s*SessionId\s+\w+\s*\)");

    private static string SessionDestroyedBody() =>
        Src.BodyOf(Host(), @"void\s+IWorkspaceLifecycleAdvisor\.SessionDestroyed\s*\(\s*SessionId\s+\w+\s*,\s*bool\s+closeSequenceIntact\s*\)");

    // What the body of AdviseClose of the host never does: it neither reads, nor authorizes, nor discards, nor retires (D-T2B2-14). The
    // guard is about THIS body; a wrapper of F4-T3 that flushes the live text first is not prohibited.
    private static readonly Regex AdviseCloseForbidden = new(
        @"ReadIntent|_editIntentReader|_reader\b|Authorize|_probe|_bridge|\.Discard\w*\s*\(|\.Retire\w*\s*\(|DiscardAndReload|PassGate|_drafts\b|MarkPossiblyStale|_lossNotices|\.Begin\s*\(|CheckNow");

    [Fact]
    public void TheAdviceOfTheHostIsTheClosePolicyThePolicyFailureContainedAsAVetoAndTheMarkAndNothingElse()
    {
        var body = AdviseCloseBody();

        // F4-T3A (D-T3-07): the order of the mark and the prohibitions stay; the only addition is the notice of the veto of the panel, queued in
        // every way that ends in a Veto and dropped before the mark is put by an Allow.
        AssertInOrder(body, "_knownClean.Remove(", "_closePolicy.EvaluateClose(", "DropCloseNotice(", "_knownClean.Add(", "return CloseDecision.Allow;");
        Assert.Matches(@"_knownClean\.Add\s*\(\s*\w+\s*\)\s*;\s*return\s+CloseDecision\.Allow\s*;", body);
        Assert.Matches(@"\.IsVeto\s*\)\s*\{\s*QueueCloseNotice\s*\([^;]*\)\s*;\s*return\s+CloseDecision\.Veto\s*;\s*\}", body);
        Assert.Contains("Report(\"WorkspaceHostCore.AdviseClose\", ex);", body);
        Assert.Matches(@"catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*Report\s*\([^;]*\)\s*;\s*QueueCloseNotice\s*\([^;]*,\s*true\s*\)\s*;\s*return\s+CloseDecision\.Veto\s*;", body);
        Assert.Equal(2, Src.Count(body, @"\bQueueCloseNotice\s*\("));
        Assert.Equal(1, Src.Count(body, @"\bDropCloseNotice\s*\("));
        // Every return of a Veto is preceded by the queueing of its notice.
        Assert.Equal(Src.Count(body, @"return\s+CloseDecision\.Veto\s*;"), Src.Count(body, @"\bQueueCloseNotice\s*\("));
        Assert.Equal(1, Src.Count(body, @"_knownClean\.Add\s*\("));
        Assert.Equal(1, Src.Count(Host(), @"_knownClean\.Add\s*\("));
        Assert.DoesNotMatch(AdviseCloseForbidden, body);
        Assert.DoesNotMatch(new Regex(@"_faulted|\bContain\s*\("), body);
    }

    [Theory]
    [InlineData("var read = _editIntentReader!.ReadIntent(session, rack, null, null);")]
    [InlineData("var gate = Authorize(new WorkspaceRequest(session));")]
    [InlineData("_drafts.DiscardAllByUser(session);")]
    [InlineData("_drafts.RetireConservative(session);")]
    [InlineData("var live = _reader.Read(session, 1);")]
    [InlineData("_bridge.RequestReconciliation();")]
    public void TheAdviceGuardDetectsAReadAnAuthorizationADiscardOrARetirementInTheRealBodyOfTheAdvice(string injected)
    {
        var real = AdviseCloseBody();

        Assert.DoesNotMatch(AdviseCloseForbidden, real);
        Assert.Matches(AdviseCloseForbidden, real + "\n" + injected);
    }

    private static bool VetoNoticeHolds(string body) =>
        Src.Count(body, @"return\s+CloseDecision\.Veto\s*;") == Src.Count(body, @"\bQueueCloseNotice\s*\(")
        && Src.Count(body, @"\bDropCloseNotice\s*\(") == 1
        && Regex.IsMatch(body, @"\.IsVeto\s*\)\s*\{\s*QueueCloseNotice");

    [Fact]
    public void TheNoticeOfTheVetoGuardDetectsAVetoWithoutItsNoticeAnAllowThatDoesNotDropItAndAnExtraVeto()
    {
        var real = AdviseCloseBody();
        var noNotice = Regex.Replace(real, @"QueueCloseNotice\s*\(\s*session\s*,\s*decision\.RackIds\s*,\s*false\s*\)\s*;", string.Empty);
        var noDrop = Regex.Replace(real, @"DropCloseNotice\s*\(\s*session\s*\)\s*;", string.Empty);
        var extraVeto = real.Replace("return CloseDecision.Allow;", "return CloseDecision.Veto;", StringComparison.Ordinal);

        Assert.True(VetoNoticeHolds(real));
        foreach (var mutated in new[] { noNotice, noDrop, extraVeto })
        {
            Assert.NotEqual(real, mutated);
            Assert.False(VetoNoticeHolds(mutated));
        }
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-04 and 07: the way a session is retired and its notice

    [Fact]
    public void TheDestructionRetiresKnownSafeOnlyWithTheMarkAndAnIntactSequenceConservativelyOtherwiseAndContainsTheRetirementApartFromTheNotice()
    {
        var body = SessionDestroyedBody();

        AssertInOrder(body, "_knownClean.Remove(", "RetireKnownSafe(", "RetireConservative(", "HasLoss", "_lossNotices.Enqueue(", "\"WorkspaceHostCore.DraftLoss\"");
        Assert.Matches(@"marked\s*&&\s*closeSequenceIntact\s*\?\s*_drafts\.RetireKnownSafe\s*\(\s*\w+\s*\)\s*:\s*_drafts\.RetireConservative\s*\(\s*\w+\s*\)", body);
        Assert.Matches(@"var\s+marked\s*=\s*_knownClean\.Remove\s*\(\s*\w+\s*\)\s*;", body);
        Assert.Equal(2, Src.Count(body, @"\btry\b"));
        Assert.Equal(2, Src.Count(body, @"catch\s*\(\s*Exception\s+ex\s*\)"));
        Assert.Matches(@"new\s+InvalidOperationException\s*\(\s*DescribeLoss\s*\(", body);
        Assert.DoesNotMatch(new Regex(@"_bridge|RetireSession|\.Discard\w*\s*\(|_editIntentReader|ReadIntent|_faulted|\bContain\s*\("), body);
        Assert.Equal(1, Src.Count(body, @"RetireKnownSafe\s*\("));
        Assert.Equal(1, Src.Count(body, @"RetireConservative\s*\("));
    }

    [Fact]
    public void TheQueueOfNoticesIsInternalAndTheLogOfALossNamesTheLostRacksAndApartTheUncertainOnes()
    {
        var code = Host();

        Assert.Matches(@"private\s+readonly\s+Queue<DraftLossNotice>\s+_lossNotices\s*=\s*new\s*\(\s*\)\s*;", code);
        Assert.Matches(@"internal\s+IReadOnlyCollection<DraftLossNotice>\s+LossNotices\s*=>\s*_lossNotices\s*;", code);
        Assert.DoesNotMatch(new Regex(@"public\s+[\w<>]+\s+(?:LossNotices|_lossNotices)"), code);
        // F4-T3A (M-2): the queue stays internal; the one other way to take from it is the explicit TakeLossNotices of the edit port, which
        // is not public on the host either.
        var take = Src.BodyOf(code, @"IReadOnlyList<PilotLossView>\s+IWorkspacePilotEditPort\.TakeLossNotices\s*\(\s*\)");
        Assert.Contains("_lossNotices.Dequeue()", take);
        Assert.Equal(1, Src.Count(code, @"_lossNotices\.Dequeue\s*\("));
        Assert.DoesNotMatch(new Regex(@"public\s+[\w<>]+\s+TakeLossNotices"), code);
        var describe = Src.BodyOf(code, @"private\s+static\s+string\s+DescribeLoss\s*\(\s*DraftLossNotice\s+\w+\s*\)");
        Assert.Contains("LostRackIds", describe);
        Assert.Contains("UncertainRackIds", describe);
        Assert.Contains("Uncertain", describe);
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-04: where the mark is set and where it is removed

    [Fact]
    public void TheMarkIsRemovedByTheNineOperationsOfThePackageAndByNothingElseAndTheTwoOperationsOfT2B2AOnlyTouchTheFieldSimply()
    {
        var host = Host();

        Assert.Matches(@"private\s+readonly\s+HashSet<SessionId>\s+_knownClean\s*=\s*new\s*\(\s*\)\s*;", host);
        // Advice (1), destruction (1), intention (1), check (2), reload (1), discard (1), note (1), drain (1): nine is a LOWER bound since
        // F4-T3A (M-2). A new operation that changes a draft must call NotePilotEdit or remove the mark itself (guard below).
        Assert.True(Src.Count(host, @"_knownClean\.Remove\s*\(") >= 9);

        var intent = Src.BodyOf(host, @"public\s+EditIntentOutcome\s+TryBeginPilotEdit\s*\(");
        Assert.Equal(1, Src.Count(intent, @"_knownClean"));
        Assert.Matches(@"case\s+DraftBeginStatus\.Created\s*:\s*_knownClean\.Remove\s*\(\s*request\.Session\s*\)\s*;\s*return\s+EditIntentOutcome\.Admitted\s*\(", intent);

        var check = Src.BodyOf(host, @"public\s+EditCheckOutcome\s+TryCheckPilotEdit\s*\(");
        Assert.Equal(2, Src.Count(check, @"_knownClean"));
        Assert.Matches(@"case\s+DraftCheckStatus\.Recovered\s*:\s*_knownClean\.Remove\s*\(\s*request\.Session\s*\)\s*;\s*return\s+EditCheckOutcome\.Recovered\s*\(", check);
        Assert.Matches(@"case\s+DraftCheckStatus\.Conflict\s*:\s*_knownClean\.Remove\s*\(\s*request\.Session\s*\)\s*;\s*return\s+EditCheckOutcome\.Conflict\s*\(", check);
    }

    // The explicit operations of the edit port of the host (F4-T3A) that change a draft: each one calls an API that changes it, and then
    // it has to end the mark of "known clean" (through NotePilotEdit, through the discard of the session or by removing the mark itself).
    private static readonly string[] PortMutatingOperations =
    {
        @"PilotFieldResult\s+IWorkspacePilotEditPort\.SetPendingText\s*\(",
        @"PilotFieldResult\s+IWorkspacePilotEditPort\.CommitField\s*\(",
        @"PilotBoundaryResult\s+IWorkspacePilotEditPort\.CrossBoundary\s*\(",
        @"EditDiscardOutcome\s+IWorkspacePilotEditPort\.DiscardRack\s*\(",
        @"EditDiscardOutcome\s+IWorkspacePilotEditPort\.DiscardDocument\s*\(",
    };

    private static readonly Regex ChangesADraft = new(@"\.SetPendingText\s*\(|\.RemovePendingText\s*\(|\.CrossBoundary\s*\(|\.SetCurrentDraftState\s*\(|\.Discard\w*\s*\(|DiscardPilotEdits\s*\(");

    private static readonly Regex EndsTheMark = new(@"NotePilotEdit\s*\(|_knownClean\.Remove\s*\(|DiscardPilotEdits\s*\(");

    private static bool MutationEndsTheMark(string body) => !ChangesADraft.IsMatch(body) || EndsTheMark.IsMatch(body);

    [Fact]
    public void EveryOperationOfTheEditPortThatChangesADraftEndsTheMarkOfKnownClean()
    {
        var host = Host();

        foreach (var signature in PortMutatingOperations)
        {
            var body = Src.BodyOf(host, signature);
            Assert.True(ChangesADraft.IsMatch(body), signature + " no longer changes a draft: the guard would be empty");
            Assert.True(MutationEndsTheMark(body), signature + " changes a draft and does not end the mark of known clean");
        }
    }

    [Fact]
    public void TheMarkGuardDetectsAnOperationThatChangesADraftWithoutEndingTheMark()
    {
        Assert.False(MutationEndsTheMark("{ var e = draft.SetPendingText(key, text); return e; }"));
        Assert.False(MutationEndsTheMark("{ var d = _drafts.DiscardByUser(session, rack); return d; }"));
        Assert.True(MutationEndsTheMark("{ var e = draft.SetPendingText(key, text); NotePilotEdit(session); return e; }"));
        Assert.True(MutationEndsTheMark("{ var d = _drafts.DiscardByUser(session, rack); _knownClean.Remove(session); return d; }"));
        Assert.True(MutationEndsTheMark("{ return Describe(session); }"));

        // In the real bodies: without the call that ends the mark, only the operation that delegates to the discard of the session is still fine.
        var host = Host();
        var withoutNote = 0;
        foreach (var signature in PortMutatingOperations)
        {
            var body = Src.BodyOf(host, signature);
            var mutated = Regex.Replace(body, @"NotePilotEdit\s*\(|_knownClean\.Remove\s*\(", "Skip(");
            if (mutated != body) withoutNote++;
            Assert.Equal(body.Contains("DiscardPilotEdits(", StringComparison.Ordinal), MutationEndsTheMark(mutated));
        }

        Assert.True(withoutNote >= 4);
    }

    [Fact]
    public void TheNoteOfAnEditIsAPublicOperationWithoutGateThatOnlyRemovesTheMark()
    {
        var host = Host();

        Assert.Equal(
            "publicvoidNotePilotEdit(SessionIdsession)=>_knownClean.Remove(session);",
            Squash(Src.BodyOf(host, @"public\s+void\s+NotePilotEdit\s*\(\s*SessionId\s+session\s*\)")));
    }

    [Fact]
    public void TheDrainEndsTheMarkAndMarksTheDraftsInsideTheTryBeforeTheEchoPurgeAndTheEarlyReturnsAndNamesNoOperationOfTheIntention()
    {
        var body = Src.BodyOf(Host(), @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");

        var guard = Regex.Match(body, @"if\s*\(\s*!_started\s*\|\|\s*!outcome\.Result\.Drained\s*\)\s*return\s*;");
        var tryAt = Regex.Match(body, @"\btry\b").Index;
        Assert.True(guard.Success);
        var removeAt = IndexOf(body, "_knownClean.Remove(outcome.Session);");
        var markAt = IndexOf(body, "MarkPilotDraftsPossiblyStale(outcome.Session)");
        var purgeAt = IndexOf(body, "PurgeDead(");
        var sessionAt = IndexOf(body, "_bridge.TryGetSession(");
        Assert.True(guard.Index < tryAt && tryAt < removeAt && removeAt < markAt && markAt < purgeAt && purgeAt < sessionAt, "order of the drain");
        Assert.Matches(@"if\s*\(\s*outcome\.Result\.Operations\.Contains\s*\(\s*DrainOperation\.MarkDraftsPossiblyStale\s*\)\s*\)\s*MarkPilotDraftsPossiblyStale", body);
        // Only the stale hint reaches the drafts here: nothing of the intention, the reload, the discard, the retirement or the reader of the intention.
        Assert.DoesNotMatch(new Regex(@"TryReloadPilotEdit|DiscardPilotEdits|NotePilotEdit|RetireKnownSafe|RetireConservative|DiscardAllByUser|DiscardAndReload|ReadIntent|_editIntentReader|PassGate"), body);
    }

    [Fact]
    public void TheMarkingOnlyTouchesTheDraftsOfThePilotOfTheSessionAndIsOnlyAHint()
    {
        var body = Src.BodyOf(Host(), @"private\s+void\s+MarkPilotDraftsPossiblyStale\s*\(\s*SessionId\s+\w+\s*\)");

        Assert.Contains("_drafts.RackIdsOf(session)", body);
        Assert.Contains("_drafts.Find<PushBackPilotDraftState>(session, rackId)", body);
        Assert.Contains(".MarkPossiblyStale()", body);
        Assert.DoesNotMatch(new Regex(@"ReadIntent|_editIntentReader|CheckNow|DiscardAndReload|EnterError|SetCurrentDraftState|\bThrow\b"), body);
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-06: the reload

    private static string ReloadBody() => Src.BodyOf(Host(), @"public\s+EditIntentOutcome\s+TryReloadPilotEdit\s*\(\s*EditIntentRequest\s+request\s*\)");

    [Fact]
    public void TheReloadIsAMethodOfItsOwnWithTheGateAndTheEightStepsOfTheTableAndOneReadThatOnlyReachesDiscardAndReload()
    {
        var body = ReloadBody();

        AssertInOrder(
            body,
            "string.IsNullOrWhiteSpace(request.RackId)",
            "HasWitnessWithoutKey(request)",
            "PassGate(",
            "_drafts.Find<PushBackPilotDraftState>(",
            ".NeedsLazyReload",
            "_editIntentReader!.ReadIntent(",
            ".DiscardAndReload(pilot.Read);");
        Assert.Equal(1, Src.Count(body, @"\.ReadIntent\s*\("));
        Assert.Equal(1, Src.Count(body, @"_editIntentReader\s*[.!]"));
        Assert.Equal(1, Src.Count(body, @"\bpilot\.Read\b"));
        Assert.Matches(@"\.DiscardAndReload\s*\(\s*pilot\.Read\s*\)\s*;", body);
        // F4-T3A (M-2): the call is fixed in the body of the reload, not in the whole file, so a later operation of the host that reloads
        // does not trip a guard that is about this one.
        Assert.Equal(1, Src.Count(body, @"\.DiscardAndReload\s*\("));
        Assert.Equal(0, Src.Count(body, @"\.StaleBase\b|\.CheckNow\s*\(|\.Begin\s*\("));
        Assert.DoesNotMatch(new Regex(@"\.(?:RetireSession|DiscardAll|EvaluateClose|MarkPossiblyStale|KeepMineOnCurrentVersion)\s*\("), body);
    }

    [Fact]
    public void TheReloadTranslatesTheOutcomesOfTheDraftAsThePackageSaysAndContainsEveryExceptionAsFailed()
    {
        var body = ReloadBody();

        Assert.Matches(@"case\s+DraftReloadStatus\.Reloaded\s*:\s*_knownClean\.Remove\s*\(\s*request\.Session\s*\)\s*;\s*return\s+EditIntentOutcome\.Reloaded\s*\(\s*witness\s*\)\s*;", body);
        Assert.Matches(@"case\s+DraftReloadStatus\.NotAdmitted\s*:\s*return\s+EditIntentOutcome\.NotAdmitted\s*\(", body);
        Assert.Matches(@"case\s+DraftReloadStatus\.NotAllowed\s*:\s*return\s+EditIntentOutcome\.Waiting\s*\(", body);
        Assert.Matches(@"default\s*:\s*return\s+EditIntentOutcome\.Failed\s*\(", body);
        Assert.Matches(@"if\s*\(\s*draft\s*==\s*null\s*\)\s*return\s+EditIntentOutcome\.NotFound\s*\(", body);
        Assert.Matches(@"if\s*\(\s*!draft\.NeedsLazyReload\s*\)\s*return\s+EditIntentOutcome\.NotNeeded\s*\(", body);
        Assert.Matches(@"catch\s*\(\s*Exception\s+ex\s*\)", body);
        Assert.Contains("Report(\"WorkspaceHostCore.TryReloadPilotEdit\", ex);", body);
        Assert.Matches(@"\.Failed\s*\(\s*ex\.Message\s*\)", body);
        Assert.DoesNotMatch(new Regex(@"_faulted|\bContain\s*\(|\bthrow\s*;"), body);
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-08: the explicit discard of the host

    private static string DiscardBody() => Src.BodyOf(Host(), @"public\s+EditDiscardOutcome\s+DiscardPilotEdits\s*\(\s*SessionId\s+session\s*\)");

    private static readonly Regex DiscardForbidden = new(@"PassGate|_editIntentReader|ReadIntent|_reader\b|_probe|_modalRackCadActive|EditGate|IsActiveDocumentQuiescent|ActiveDocumentNativeId|_bridge");

    [Fact]
    public void TheDiscardOfTheHostOnlyAsksTheAuthorizationThenDiscardsWithoutTheComparerAndEndsTheMark()
    {
        var body = DiscardBody();

        AssertInOrder(body, "Authorize(new WorkspaceRequest(session))", "_drafts.DiscardAllByUser(session)", "_knownClean.Remove(session);", "EditDiscardOutcome.Discarded(");
        Assert.Matches(@"if\s*\(\s*Authorize\s*\(\s*new\s+WorkspaceRequest\s*\(\s*session\s*\)\s*\)\s*!=\s*RequestDecision\.Accepted\s*\)\s*return\s+EditDiscardOutcome\.Rejected\s*\(", body);
        Assert.DoesNotMatch(DiscardForbidden, body);
        Assert.DoesNotMatch(new Regex(@"\.Discard\s*\(|\.DiscardAll\s*\(|RetireSession|EvaluateClose|\.IsDirty"), body);
        Assert.Contains("Report(\"WorkspaceHostCore.DiscardPilotEdits\", ex);", body);
        Assert.Matches(@"EditDiscardOutcome\.Failed\s*\(\s*ex\.Message\s*\)", body);
        Assert.DoesNotMatch(new Regex(@"_faulted|\bContain\s*\("), body);
    }

    [Theory]
    [InlineData("var gate = PassGate(session, out var reason);")]
    [InlineData("var read = _editIntentReader!.ReadIntent(session, rack, null, null);")]
    [InlineData("if (_modalRackCadActive) return EditDiscardOutcome.Rejected(\"modal\");")]
    [InlineData("if (!_probe.IsActiveDocumentQuiescent) return EditDiscardOutcome.Rejected(\"command\");")]
    public void TheDiscardGuardDetectsAGateThatTheDiscardMustNotHaveInTheRealBody(string injected)
    {
        var real = DiscardBody();

        Assert.DoesNotMatch(DiscardForbidden, real);
        Assert.Matches(DiscardForbidden, real + "\n" + injected);
    }

    [Fact]
    public void NothingOfTheLifecycleIsCalledFromTheIdlePassOfTheCoreOrTheBridge()
    {
        var names = new Regex(@"TryReloadPilotEdit|DiscardPilotEdits|NotePilotEdit|DiscardAllByUser|RetireKnownSafe|RetireConservative|SessionDestroyed|AdviseClose");

        Assert.DoesNotMatch(names, Src.BodyOf(BridgeCore(), @"public\s+IReadOnlyList<DrainOutcome>\s+OnIdle\s*\(\s*\)"));
        Assert.DoesNotMatch(names, Src.BodyOf(Bridge(), @"private\s+void\s+OnIdle\s*\(\s*\)"));
        Assert.DoesNotMatch(names, Src.BodyOf(Controller(), @"private\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)"));
    }

    // ------------------------------------------------------------------ obligations 5 and 18: no write and no object of the host in the lifecycle

    private const string WriteApis =
        @"OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|DowngradeOpen|AppendEntity|\.Erase\s*\(|\.Commit\s*\(|RackBlockData\.Write|SetImpliedSelection|SetCurrentView|"
        + @"StartTransaction|StartOpenCloseTransaction|LockDocument|SetSystemVariable|SendStringToExecute|\bSaveAs\b|\bSave\s*\(";

    [Fact]
    public void TheFilesOfTheLifecycleWriteNothingAndHoldNoObjectNorTransactionOfTheHost()
    {
        foreach (var path in new[] { AdvisorPath, BridgeCorePath, HostCorePath, IntentPath, RegistryPath, BridgePath, ControllerPath })
        {
            var code = Src.Code(path);

            Assert.True(!Regex.IsMatch(code, WriteApis), path + " uses " + Regex.Match(code, WriteApis).Value);
        }

        foreach (var path in new[] { AdvisorPath, BridgeCorePath, HostCorePath, IntentPath, RegistryPath })
            Assert.DoesNotMatch(new Regex(@"\b(?:Transaction|Database|Document|DBObject|ObjectId)\b\s+_?\w+\s*[;=]"), Src.Code(path));
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-02: the registry

    private static string RegistryBody(string signature) => Src.BodyOf(Registry(), signature);

    private static readonly string[] ExplicitOperations =
    {
        @"public\s+DraftUserDiscardResult\s+DiscardByUser\s*\(\s*SessionId\s+session\s*,\s*string\s+rackId\s*\)",
        @"public\s+DraftUserDiscardAllResult\s+DiscardAllByUser\s*\(\s*SessionId\s+session\s*\)",
        @"public\s+DraftLossNotice\s+RetireKnownSafe\s*\(\s*SessionId\s+session\s*\)",
        @"public\s+DraftLossNotice\s+RetireConservative\s*\(\s*SessionId\s+session\s*\)",
    };

    [Fact]
    public void TheFourExplicitOperationsLiveInTheRegistryFileAndNoneOfThemMentionsTheApplyingSlotOrAWrite()
    {
        foreach (var signature in ExplicitOperations)
        {
            var body = RegistryBody(signature);

            Assert.DoesNotMatch(new Regex(@"ReleaseApplySlot|_applying|TryAcquireApplySlot"), body);
            Assert.Contains(".Detach()", body);
            Assert.DoesNotMatch(new Regex(WriteApis), body);
        }

        Assert.Empty(MissingDraftFiles(Src.Files(DraftsFolder).Select(f => f.Path)));
    }

    // F4-T3A (D-1): the folder of the drafts holds the five files of the task as a LOWER bound that names each one. The exact set of the
    // folder is fixed by WorkspaceDraftBoundaryTests, which does not change.
    private const string DraftsFolder = "src/RackCad.Application/Workspace/Drafts/";

    private static readonly string[] DraftFileNames =
    {
        "DraftApplyOutcome.cs", "DraftClosePolicy.cs", "DraftRegistry.cs", "DraftSession.cs", "DraftStaleBase.cs",
    };

    private static IReadOnlyList<string> MissingDraftFiles(IEnumerable<string> paths)
    {
        var present = paths.Select(p => p.Substring(p.LastIndexOf('/') + 1)).ToHashSet(StringComparer.Ordinal);
        return DraftFileNames.Where(n => !present.Contains(n)).ToArray();
    }

    [Fact]
    public void TheDraftsFolderBoundNamesEachOfTheFiveFilesAndDoesNotFixTheirNumber()
    {
        var real = Src.Files(DraftsFolder).Select(f => f.Path).ToArray();

        Assert.Empty(MissingDraftFiles(real));
        Assert.Equal(new[] { "DraftApplyOutcome.cs" }, MissingDraftFiles(real.Where(p => !p.EndsWith("/DraftApplyOutcome.cs", StringComparison.Ordinal))));
        Assert.Empty(MissingDraftFiles(real.Concat(new[] { DraftsFolder + "DraftExtra.cs" })));
    }

    [Fact]
    public void TheThreeOperationsThatDoNotDependOnTheComparerNeverEvaluateDirtyOrCallTheComparer()
    {
        foreach (var signature in new[] { ExplicitOperations[0], ExplicitOperations[1], ExplicitOperations[2] })
        {
            var body = RegistryBody(signature);

            Assert.DoesNotMatch(new Regex(@"\.IsDirty\b|_comparer|\.Equals\s*\(|\.Status\b|RacksAtRiskOf"), body);
        }

        Assert.Contains("HasPendingText", RegistryBody(ExplicitOperations[0]));
        Assert.Contains("HasPendingText", RegistryBody(ExplicitOperations[1]));
        Assert.Contains("HasPendingText", RegistryBody(ExplicitOperations[2]));
    }

    [Fact]
    public void TheConservativeRetirementAsksThePendingTextFirstAndThenTheComparerContainedDraftByDraftAndKeepsTheReasons()
    {
        var body = RegistryBody(ExplicitOperations[3]);

        var pendingAt = IndexOf(body, "entry.HasPendingText");
        var tryAt = Regex.Match(body, @"\btry\b").Index;
        var dirtyAt = IndexOf(body, "entry.IsDirty");
        Assert.True(pendingAt >= 0 && tryAt > pendingAt && dirtyAt > tryAt, "the pending text, then the contained comparer");
        Assert.Matches(@"catch\s*\(\s*Exception\s+ex\s*\)", body);
        Assert.Contains("ex.Message", body);
        Assert.Contains("uncertain.Add(", body);
        Assert.Matches(@"new\s+SortedDictionary<string,\s*string>\s*\(\s*StringComparer\.Ordinal\s*\)", body);
        Assert.Matches(@"new\s+ReadOnlyDictionary<string,\s*string>\s*\(\s*reasons\s*\)", body);
        Assert.Equal(1, Src.Count(body, @"entry\.IsDirty"));
        Assert.Equal(2, Src.Count(body, @"_retired\.Add\s*\(\s*session\s*\)"));
    }

    [Fact]
    public void TheListsOfTheExplicitOperationsAreOrderedOrdinallyAndTheRetirementsMarkTheSessionAsRetired()
    {
        foreach (var signature in ExplicitOperations.Skip(1))
            Assert.Contains("StringComparer.Ordinal", RegistryBody(signature));
        foreach (var signature in ExplicitOperations.Skip(2))
            Assert.Matches(@"_retired\.Add\s*\(\s*session\s*\)", RegistryBody(signature));
        foreach (var signature in ExplicitOperations.Take(2))
            Assert.DoesNotContain("_retired", RegistryBody(signature));
    }

    [Fact]
    public void TheOrdinaryOperationsOfF4T1KeepTheirOrderDecideFromTheComparerBeforeTheyRemoveAnything()
    {
        var discard = RegistryBody(@"public\s+DraftDiscardResult\s+Discard\s*\(\s*SessionId\s+session\s*,\s*string\s+rackId\s*\)");
        Assert.True(IndexOf(discard, "entry.IsDirty") < IndexOf(discard, "racks.Remove(rackId)"), "Discard decides before it removes");

        var all = RegistryBody(@"public\s+DraftDiscardAllResult\s+DiscardAll\s*\(\s*SessionId\s+session\s*\)");
        Assert.True(IndexOf(all, "e.IsDirty") < IndexOf(all, "_bySession.Remove(session)"), "DiscardAll decides before it removes");

        var retire = RegistryBody(@"public\s+DraftLossNotice\s+RetireSession\s*\(\s*SessionId\s+session\s*\)");
        Assert.Matches(@"e\.IsDirty[\s\S]*_retired\.Add\s*\(\s*session\s*\)\s*;\s*_bySession\.Remove\s*\(\s*session\s*\)", retire);
    }

    [Fact]
    public void TheNoticeOfLossGainsTheUncertainMembersAdditivelyInTheRegistryFile()
    {
        var code = Registry();

        Assert.Matches(@"public\s+IReadOnlyList<string>\s+UncertainRackIds\s*\{\s*get;\s*\}", code);
        Assert.Matches(@"public\s+IReadOnlyDictionary<string,\s*string>\s+UncertainReasons\s*\{\s*get;\s*\}", code);
        Assert.Matches(@"internal\s+DraftLossNotice\s*\(\s*SessionId\s+session\s*,\s*IReadOnlyList<string>\s+lostRackIds\s*\)", code);
        Assert.Matches(@"public\s+readonly\s+record\s+struct\s+DraftUserDiscardResult\s*\(\s*bool\s+Removed\s*,\s*bool\s+HadPendingText\s*\)", code);
        Assert.Matches(@"public\s+sealed\s+class\s+DraftUserDiscardAllResult\b", code);
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-09: N-4 of F4-T2B2A, the three ways to Rejected

    [Fact]
    public void TheDocumentationOfRejectedNamesTheThreeWaysTheGateTheAnswerOfTheReaderAndTheRetiredSessionOfStepEight()
    {
        var text = Src.Text(IntentPath);
        var start = IndexOf(text, "public enum EditIntentStatus");
        var rejected = text.IndexOf("        Rejected,", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && rejected > start);
        var summary = text.LastIndexOf("<summary>", rejected, StringComparison.Ordinal);
        var doc = text.Substring(summary, rejected - summary);

        Assert.Contains("INV-22", doc);
        Assert.Contains("before any read", doc);
        Assert.Contains("the reader answers Rejected", doc);
        Assert.Contains("SessionRetired", doc);
        Assert.Contains("step 8", doc);
        Assert.True(Src.Count(doc, @"after the gate") >= 2, "the last two ways are after the gate");
    }

    // ------------------------------------------------------------------ INV-F4T2B2B-11 and 12: the enums, the folders and the guards that stay

    [Fact]
    public void TheNewAndExtendedEnumsAreFixedAsOrderedPrefixesSoLaterStatesAreNotProhibited()
    {
        var intent = Enum.GetNames<EditIntentStatus>();
        var discard = Enum.GetNames<EditDiscardStatus>();

        Assert.True(intent.Length >= 10);
        Assert.Equal(
            new[] { "Unavailable", "Waiting", "Rejected", "AlreadyOpen", "NotAdmitted", "Admitted", "Failed", "NotFound", "NotNeeded", "Reloaded" },
            intent.Take(10).ToArray());
        Assert.True(discard.Length >= 3);
        Assert.Equal(new[] { "Rejected", "Discarded", "Failed" }, discard.Take(3).ToArray());
    }

    [Fact]
    public void TheFoldersAndTheGuardsOfTheOlderTasksKeepTheirShape()
    {
        Assert.Equal(9, Src.PluginFiles().Count);
        Assert.Empty(MissingDraftFiles(Src.Files(DraftsFolder).Select(f => f.Path)));
        Assert.True(Src.Exists(AdvisorPath));
        Assert.Equal(45, Src.Count(Src.Text("tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs"), @"\[(?:Fact|Theory)\b"));
    }
}
