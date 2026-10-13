#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F1-T3B-HOST-UI: source guards of the visual host of the workspace panel. The Core test project cannot reference
// the Plugin (it needs AutoCAD) nor load WPF, so these read the sources as text, exactly like the guards of T2 and T3A:
// the palette host (INV-F1-T3B-01, 02, 06, 09, 12), the command delta (03), the bridge notification (04), the controller
// relay (05), the boundary of the UI folder (07, 11, 13) and the refinements G1/G2/G3 (14).
public class WorkspacePaletteHostSourceGuardTests
{
    private const string HostPath = "src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs";
    private const string CommandPath = "src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs";
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string GuardPath = "tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs";
    private const string UiFolder = "src/RackCad.UI/Workspace/";
    private const string ViewXamlPath = "src/RackCad.UI/Workspace/WorkspacePanelView.xaml";
    private const string ViewCodePath = "src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs";
    private const string ViewModelPath = "src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs";

    private static string Host() => Src.Code(HostPath);

    private static string Command() => Src.Code(CommandPath);

    private static string Bridge() => Src.Code(BridgePath);

    private static string Controller() => Src.Code(ControllerPath);

    private static string StripXamlComments(string xaml) => Regex.Replace(xaml, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

    // The sources of the UI folder: the code files without comments and the XAML files without comments.
    private static IReadOnlyList<(string Path, string Code)> UiSources()
    {
        var code = Src.Files(UiFolder);
        var xaml = new List<(string, string)>();
        var dir = System.IO.Path.Combine(Src.RepoRoot(), "src", "RackCad.UI", "Workspace");
        if (System.IO.Directory.Exists(dir))
        {
            foreach (var file in System.IO.Directory.GetFiles(dir, "*.xaml", System.IO.SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
            {
                var relative = System.IO.Path.GetRelativePath(Src.RepoRoot(), file).Replace(System.IO.Path.DirectorySeparatorChar, '/');
                xaml.Add((relative, StripXamlComments(System.IO.File.ReadAllText(file))));
            }
        }

        return code.Concat(xaml).ToList();
    }

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

    // ------------------------------------------------------------------ INV-F1-T3B-01: the one palette, lazily

    [Fact]
    public void ThePaletteHostIsOneExactNewFileWithOneSealedClass()
    {
        var code = Host();

        Assert.Matches(@"namespace\s+RackCad\.Plugin\.Workspace\b", code);
        Assert.Equal(1, Src.Count(code, @"\bsealed\s+class\s+WorkspacePaletteHost\b"));
        Assert.Equal(1, Src.Count(code, @"\bclass\s+\w+"));
    }

    [Fact]
    public void ExactlyOnePaletteSetIsConstructedInTheWholeProductAndOnlyByTheHost()
    {
        var builders = Src.AllProduct()
            .Select(f => (f.Path, Count: Src.Count(f.Code, @"\bnew\s+(?:[\w.]+\.)?PaletteSet\s*\(")))
            .Where(x => x.Count > 0)
            .ToList();

        var owner = Assert.Single(builders);
        Assert.Equal(HostPath, owner.Path);
        Assert.Equal(1, owner.Count);
        Assert.Equal(new[] { HostPath }, Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bPaletteSet\b")).Select(f => f.Path).ToArray());
    }

    [Fact]
    public void ThePaletteIsBuiltWithOnlyTheTitleRackCadAndNoToolIdentifier()
    {
        var code = Host();

        Assert.Matches(@"\bnew\s+PaletteSet\s*\(\s*""RackCad""\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"\bGuid\b|toolID|ToolId|new\s+PaletteSet\s*\(\s*""[^""]*""\s*,"), code);
    }

    [Fact]
    public void ThePaletteHasNoLoadNorSaveHandlers()
    {
        var code = Host();

        Assert.DoesNotMatch(new Regex(@"\bLoad\b|\bSave\b|\bSaving\b|PalettePersist|\.Load\s*\+=|\.Save\s*\+="), code);
    }

    [Fact]
    public void TheHostIsCreatedLazilyThroughTheHolderAndNeverByAStaticInitializer()
    {
        var code = Host();

        Assert.Matches(@"static\s+readonly\s+WorkspaceLazyInstance<WorkspacePaletteHost>\s+\w+\s*=", code);
        Assert.Matches(@"\(\s*\)\s*=>\s*new\s+WorkspacePaletteHost\s*\(", code);
        Assert.Matches(@"private\s+WorkspacePaletteHost\s*\(\s*\)", code);
        Assert.Matches(@"public\s+static\s+WorkspacePaletteHost\s+GetOrCreate\s*\(", code);
        Assert.Equal(1, Src.Count(code, @"\bnew\s+WorkspacePaletteHost\s*\("));
        Assert.DoesNotMatch(new Regex(@"static\s+WorkspacePaletteHost\s*\(\s*\)|ModuleInitializer|IExtensionApplication|\[assembly"), code);
        Assert.DoesNotMatch(new Regex(@"static\s+readonly\s+WorkspacePaletteHost\b|static\s+WorkspacePaletteHost\s+\w+\s*=\s*new\b"), code);
    }

    [Fact]
    public void NothingIsCreatedWhenThePluginLoadsAndTheHostIsNeverReleased()
    {
        var initializer = Src.Text("src/RackCad.Plugin/PluginInitializer.cs");
        Assert.DoesNotContain("PaletteHost", initializer);
        Assert.DoesNotContain("PaletteSet", initializer);

        var code = Host();
        Assert.DoesNotMatch(new Regex(@"\bDispose\b|\bTerminate\b|\.Close\s*\(|\.Remove\s*\(|\bIsDisposed\b|AutoDelete"), code);
    }

    [Fact]
    public void OnlyTheCommandAndTheHostNameThePaletteHost()
    {
        var users = Src.AllProduct()
            .Where(f => Regex.IsMatch(f.Code, @"\bWorkspacePaletteHost\b"))
            .Select(f => f.Path)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { HostPath, CommandPath }, users);
    }

    [Fact]
    public void ClosingThePaletteHidesItAndTheHostMirrorsShowAndHideToTheController()
    {
        var code = Host();
        var handler = Src.BodyOf(code, @"private\s+void\s+OnPaletteStateChanged\s*\(");

        Assert.Matches(@"\w+\.StateChanged\s*\+=\s*OnPaletteStateChanged\s*;", code);
        Assert.Contains("StateEventIndex.Show", handler);
        Assert.Contains("StateEventIndex.Hide", handler);
        Assert.Matches(@"_controller\.ShowPanel\s*\(\s*\)", handler);
        Assert.Matches(@"_controller\.HidePanel\s*\(\s*\)", handler);
        Assert.Matches(@"_palette\.Visible\s*=\s*true\s*;", Src.BodyOf(code, @"public\s+void\s+Show\s*\(\s*\)"));
    }

    [Fact]
    public void ThePaletteIsAcoplableAndFloatingWithTheCloseButton()
    {
        var code = Host();

        Assert.Matches(@"DockEnabled\s*=", code);
        Assert.Matches(@"PaletteSetStyles\.ShowCloseButton", code);
    }

    // ------------------------------------------------------------------ INV-F1-T3B-02: one root control, injected port

    [Fact]
    public void TheHostAddsExactlyOneRootControlLabelledWorkspace()
    {
        var code = Host();

        Assert.Equal(1, Src.Count(code, @"\bAddVisual\s*\("));
        Assert.Matches(@"_palette\.AddVisual\s*\(\s*""Workspace""\s*,\s*_root\s*\)", code);
        Assert.Equal(1, Src.Count(code, @"\bnew\s+WorkspacePanelView\s*\("));
        Assert.Equal(1, Src.Count(code, @"\bnew\s+WorkspacePanelViewModel\s*\("));
        Assert.DoesNotMatch(new Regex(@"\.Add\s*\(\s*""|AddVisualBrowser|new\s+System\.Windows\.Window\b|\bnew\s+Window\b"), code);
    }

    [Fact]
    public void TheViewModelIsFedOnlyByTheInjectedPortAndTheNeutralToggleDelegate()
    {
        var code = Host();

        Assert.Matches(@"new\s+WorkspacePanelViewModel\s*\(\s*_controller\.Port\s*,\s*_controller\.SetInstrumentationEnabled\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"_controller\.(?:ActiveSession|TryGetSession|Authorize|RequestReconciliation|RefreshPanel|OpenPanel)"), code);
        Assert.DoesNotMatch(new Regex(@"AutoCadEventBridge|WorkspaceHostCore|WorkspaceSelectionReader|WorkspaceSessionRegistry|EventBridgeCore"), code);
    }

    // ------------------------------------------------------------------ INV-F1-T3B-06 and 09: event-driven refresh, contained

    [Fact]
    public void TheHostSubscribesToTheControllerNoticeAndToThePaletteVisibilityAndNothingElse()
    {
        var code = Host();

        Assert.Equal(2, Src.Count(code, @"\+="));
        Assert.Equal(0, Src.Count(code, @"-="));
        Assert.Matches(@"_controller\.StateChanged\s*\+=\s*OnControllerStateChanged\s*;", code);
        Assert.Matches(@"_palette\.StateChanged\s*\+=\s*OnPaletteStateChanged\s*;", code);
    }

    [Fact]
    public void TheControllerNoticeOnlyAsksTheViewModelToPullTheSnapshotAndOnlyWhileThePanelIsVisible()
    {
        var code = Host();
        var handler = Src.BodyOf(code, @"private\s+void\s+OnControllerStateChanged\s*\(\s*\)");

        Assert.Matches(@"_viewModel\.Refresh\s*\(\s*\)", handler);
        Assert.Contains("PanelVisible", handler);
        Assert.DoesNotMatch(new Regex(@"GetSnapshot|GetMeasurementReport|Authorize|GetSelectionContext"), code);
    }

    [Fact]
    public void EveryEntryOfThePaletteHostIsContainedAndLoggedOnlyThroughTheFaultMethod()
    {
        var code = Host();

        foreach (var signature in new[] { @"private\s+void\s+OnPaletteStateChanged\s*\(", @"private\s+void\s+OnControllerStateChanged\s*\(\s*\)" })
        {
            var body = Src.BodyOf(code, signature);
            Assert.Contains("try", body);
            Assert.Contains("catch (System.Exception", body);
            Assert.Contains("LogFault(", body);
        }

        Assert.Matches(@"private\s+static\s+void\s+LogFault\s*\(\s*string\s+\w+\s*,\s*System\.Exception\s+\w+\s*\)\s*=>\s*RackLog\.Exception\s*\(", code);
        Assert.Equal(1, Src.Count(code, @"\bRackLog\."));
        Assert.DoesNotMatch(new Regex(@"RackLog\.Warning"), code);
    }

    [Fact]
    public void ThePaletteHostHasNoTimerNoDispatcherNoPollingNoThreadNoAsyncAndNoGlobalHandler()
    {
        var code = Host();

        Assert.DoesNotMatch(
            new Regex(@"\bDispatcher\w*|\w*Timer\b|System\.Timers|\bTask\b|\basync\b|\bawait\b|\bThread\b|ThreadPool|BackgroundWorker|SynchronizationContext|Parallel\.|UnhandledException|AppDomain|Sleep\s*\("),
            code);
    }

    [Fact]
    public void ThePaletteHostNeverReadsOrWritesTheDrawingNorTheCommandLineNorTheDisk()
    {
        var code = Host();

        Assert.DoesNotMatch(
            new Regex(@"StartTransaction|StartOpenCloseTransaction|LockDocument|GetObject|SelectImplied|MdiActiveDocument|DocumentManager|\bEditor\b|\bDatabase\b|WriteMessage|System\.IO|\bFile\.|Registry|UserSettings|XData|Xrecord|NamedObjectsDictionary"),
            code);
    }

    // ------------------------------------------------------------------ INV-F1-T3B-12: focus through public APIs only

    [Fact]
    public void FocusUsesOnlyThePublicKeepFocusOfThePaletteSet()
    {
        var code = Host();

        Assert.Matches(@"_palette\.KeepFocus\s*=\s*true\s*;", code);
        Assert.DoesNotMatch(
            new Regex(@"Autodesk\.AutoCAD\.Internal|SetFocusToDwgView|SetFocus\b|IKeyboardInputSink|HwndSource|DllImport|user32|System\.Reflection|BindingFlags|GetMethod|GetField|GetProperty|Activator\.|Keyboard\.|InputManager"),
            code);
    }

    [Fact]
    public void NoNewWorkspaceFileUsesTheInternalAutoCadNamespaceOrReflectsOverAutoCad()
    {
        foreach (var (path, code) in Src.PluginFiles().Concat(Src.Files(UiFolder)))
            Assert.DoesNotMatch(new Regex(@"Autodesk\.AutoCAD\.Internal|SetFocusToDwgView|IKeyboardInputSink|DllImport"), code);
    }

    [Fact]
    public void ThePaletteHostPublicSurfaceIsTheLazyAccessorAndShowAndNamesNoAutoCadType()
    {
        var code = Host();
        var names = code.Split('\n')
            .Select(l => Regex.Match(l, @"^\s*public\s+(?:static\s+)?[\w<>?,\s\.]+?\s+(\w+)\s*(?:\(|=>|;|\{)"))
            .Where(m => m.Success)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "GetOrCreate", "IsCreated", "Show" }, names);
        foreach (var line in code.Split('\n').Where(l => Regex.IsMatch(l, @"^\s*(?:public|protected)\b")))
            Assert.DoesNotMatch(new Regex(@"PaletteSet|\w+EventArgs|Autodesk|\bDocument\b|\bEditor\b"), line);
    }

    // ------------------------------------------------------------------ INV-F1-T3B-03: the delta of RACKPANEL

    [Fact]
    public void TheCommandObtainsTheControllerOpensThePanelThenShowsThePaletteThroughTheHost()
    {
        var code = Command();

        var open = Regex.Match(code, @"WorkspaceController\.GetOrCreate\(\)\s*\.OpenPanel\s*\(\s*\)");
        var show = Regex.Match(code, @"WorkspacePaletteHost\.GetOrCreate\(\)\s*\.Show\s*\(\s*\)");
        Assert.True(open.Success, "the command no longer opens the panel through the controller");
        Assert.True(show.Success, "the command does not show the palette through the host");
        Assert.True(open.Index < show.Index, "the palette is shown before the panel state is opened");
    }

    [Fact]
    public void TheCommandKeepsNoPaletteLogicAndNoUiTypeAndTheMessageNoLongerSaysTheInterfaceIsNotShown()
    {
        var code = Command();

        Assert.DoesNotMatch(new Regex(@"PaletteSet|AddVisual|System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|UserControl|Dispatcher"), code);
        Assert.DoesNotMatch(new Regex(@"no\s+muestra|aun\s+no|todavia\s+no|not\s+shown", RegexOptions.IgnoreCase), code);
    }

    [Fact]
    public void TheCommandStaysTheOnlyOneWithOneAttributeNoAliasAndTheSharedReporter()
    {
        var code = Command();

        Assert.Equal(1, Src.Count(code, @"\[\s*CommandMethod\b"));
        Assert.Equal(1, Src.Count(code, @"\[CommandMethod\(""RACKPANEL""\)\]"));
        Assert.Contains("catch (System.Exception ex)", code);
        Assert.Contains("RackCommandSupport.Report(ex)", code);
        Assert.DoesNotMatch(new Regex(@"CommandFlags|CommandClass|LispFunction"), code);
    }

    // ------------------------------------------------------------------ INV-F1-T3B-04: the notice of the bridge

    [Fact]
    public void TheBridgeDeclaresOneInternalActiveSessionChangedNoticeWithoutArguments()
    {
        var code = Bridge();

        Assert.Equal(1, Src.Count(code, @"\binternal\s+event\s+Action\s+ActiveSessionChanged\s*;"));
        Assert.DoesNotMatch(new Regex(@"public\s+event\s+Action\s+ActiveSessionChanged"), code);
        Assert.DoesNotMatch(new Regex(@"event\s+Action<[^>]*>\s+ActiveSessionChanged"), code);
    }

    [Fact]
    public void TheNoticeIsRaisedOnlyTwiceFromTheExistingDocumentHandlersAfterTheCoreHasBeenUpdated()
    {
        var code = Bridge();

        Assert.Equal(3, Src.Count(code, @"\bActiveSessionChanged\b"));
        Assert.Equal(2, Src.Count(code, @"\bActiveSessionChanged\s*\?\.\s*Invoke\s*\(\s*\)"));

        var activated = CaseBlock(code, "DocumentActivated");
        var contain = activated.IndexOf("Contain(\"DocumentActivated\"", StringComparison.Ordinal);
        var core = activated.IndexOf("_core.OnDocumentActivated(", StringComparison.Ordinal);
        var raise = activated.IndexOf("ActiveSessionChanged?.Invoke()", StringComparison.Ordinal);
        var hook = activated.IndexOf("documents.DocumentActivated +=", StringComparison.Ordinal);
        Assert.True(contain >= 0 && contain < core && core < raise && raise < hook, "DocumentActivated: the notice is not raised inside Contain after the core");

        var destroyed = CaseBlock(code, "DocumentToBeDestroyed");
        var containD = destroyed.IndexOf("Contain(\"DocumentToBeDestroyed\"", StringComparison.Ordinal);
        var before = destroyed.IndexOf("_core.ActiveSession", StringComparison.Ordinal);
        var coreD = destroyed.IndexOf("_core.OnDocumentToBeDestroyed(", StringComparison.Ordinal);
        var raiseD = destroyed.IndexOf("ActiveSessionChanged?.Invoke()", StringComparison.Ordinal);
        var hookD = destroyed.IndexOf("documents.DocumentToBeDestroyed +=", StringComparison.Ordinal);
        Assert.True(containD >= 0 && containD < before && before < coreD && coreD < raiseD && raiseD < hookD, "DocumentToBeDestroyed: the notice is not raised inside Contain after the core");
        Assert.True(Src.Count(destroyed, @"_core\.ActiveSession\b") >= 2, "the active session is not compared before and after");
        Assert.Matches(@"!=", destroyed.Substring(coreD, raiseD - coreD));
    }

    [Fact]
    public void NoOtherBridgeMemberRaisesTheNoticeAndTheBridgeAddsNoSubscriptionNorHandler()
    {
        var code = Bridge();

        foreach (var source in new[] { "DocumentCreated", "BeginDocumentClose", "ImpliedSelectionChanged", "CommandEnded", "Idle" })
            Assert.DoesNotContain("ActiveSessionChanged", CaseBlock(code, source));
        foreach (var signature in new[] { @"public\s+void\s+Start\s*\(\s*\)", @"public\s+void\s+Dispose\s*\(\s*\)", @"private\s+void\s+OnIdle\s*\(\s*\)", @"private\s+Action\s+Hook\s*\(" })
        {
            var body = Src.BodyOf(code, signature);
            if (signature.Contains("Hook", StringComparison.Ordinal))
                body = body.Replace(CaseBlock(code, "DocumentActivated"), string.Empty).Replace(CaseBlock(code, "DocumentToBeDestroyed"), string.Empty);
            Assert.DoesNotContain("ActiveSessionChanged", body);
        }

        Assert.Equal(16, Src.Count(code, @"\+="));
        Assert.Equal(16, Src.Count(code, @"-="));
        Assert.Equal(16, Src.Count(code, @"\bhandler\s*=\s*\("));
    }

    [Fact]
    public void TheNoticeNeverQueuesAReconciliationAndTheCoreIsUntouchedByIt()
    {
        var code = Bridge();
        foreach (var line in code.Split('\n').Where(l => Regex.IsMatch(l, @"\bhandler\s*=\s*\(")))
        {
            Assert.DoesNotContain("RequestReconciliation", line);
            Assert.Matches(@"Contain(?:Session)?\(", line);
        }

        var core = Src.Code("src/RackCad.Application/Workspace/EventBridgeCore.cs");
        Assert.DoesNotContain("ActiveSessionChanged", core);
        Assert.DoesNotMatch(new Regex(@"\bpublic\s+event\b"), core);
    }

    // ------------------------------------------------------------------ INV-F1-T3B-05: the relay of the controller

    [Fact]
    public void TheControllerSubscribesToExactlyTheDrainedAndTheActiveSessionNoticesOfTheBridge()
    {
        var code = Controller();
        var constructor = Src.BodyOf(code, @"private\s+WorkspaceController\s*\(\s*\)");

        Assert.Equal(2, Src.Count(code, @"\+="));
        Assert.Equal(0, Src.Count(code, @"-="));
        Assert.Matches(@"_bridge\.Drained\s*\+=\s*OnDrained\s*;", constructor);
        Assert.Matches(@"_bridge\.ActiveSessionChanged\s*\+=\s*OnActiveSessionChanged\s*;", constructor);
        Assert.DoesNotContain(".Start(", constructor);
        Assert.DoesNotContain("OpenPanel", constructor);
        Assert.DoesNotContain("RequestReconciliation", constructor);
    }

    [Fact]
    public void TheControllerExposesOneInMemoryStateChangedNoticeWithoutArguments()
    {
        var code = Controller();

        Assert.Equal(1, Src.Count(code, @"\bpublic\s+event\s+Action\s+StateChanged\s*;"));
        Assert.Equal(1, Src.Count(code, @"\bevent\b"));
        Assert.Equal(1, Src.Count(code, @"StateChanged\s*\?\.\s*Invoke\s*\(\s*\)"));
    }

    [Fact]
    public void ANoticeOfTheActiveSessionRefreshesThePanelOnlyWhileVisibleAndThenAnnouncesTheState()
    {
        var code = Controller();
        var body = Src.BodyOf(code, @"private\s+void\s+OnActiveSessionChanged\s*\(\s*\)");

        var guard = Regex.Match(body, @"if\s*\(\s*PanelVisible\s*\)\s*_host\.RefreshPanel\s*\(\s*\)\s*;");
        Assert.True(guard.Success, "RefreshPanel is not guarded by PanelVisible");
        var announce = body.IndexOf("NotifyStateChanged()", StringComparison.Ordinal);
        Assert.True(announce > guard.Index, "the state is announced before the refresh");
        Assert.Contains("catch (System.Exception ex)", body);
        Assert.Contains("LogFault(", body);
        Assert.DoesNotContain("RequestReconciliation", body);
        Assert.DoesNotContain("_bridge.", body);
    }

    [Fact]
    public void ADrainKeepsTheHostCallAndItsContainmentAndThenAnnouncesTheState()
    {
        var code = Controller();
        var body = Src.BodyOf(code, @"private\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");

        var hostCall = body.IndexOf("_host.OnDrained(outcome)", StringComparison.Ordinal);
        var announce = body.IndexOf("NotifyStateChanged()", StringComparison.Ordinal);
        Assert.True(hostCall >= 0, "the drain no longer reaches the host core");
        Assert.True(announce > hostCall, "the state is not announced after the drain");
        Assert.Contains("catch (System.Exception ex)", body);
        Assert.Contains("LogFault(\"WorkspaceController.OnDrained\"", body);
    }

    [Fact]
    public void TheNoticeItselfIsContainedAndEveryPanelTransitionAnnouncesTheStateAfterTheHostCall()
    {
        var code = Controller();
        var notify = Src.BodyOf(code, @"private\s+void\s+NotifyStateChanged\s*\(\s*\)");

        Assert.Contains("StateChanged?.Invoke()", notify);
        Assert.Contains("catch (System.Exception ex)", notify);
        Assert.Contains("LogFault(", notify);

        foreach (var (signature, call) in new[]
                 {
                     (@"public\s+void\s+OpenPanel\s*\(\s*\)", "_host.OpenPanel()"),
                     (@"public\s+void\s+ShowPanel\s*\(\s*\)", "_host.ShowPanel()"),
                     (@"public\s+void\s+HidePanel\s*\(\s*\)", "_host.HidePanel()"),
                 })
        {
            var body = Src.BodyOf(code, signature);
            var hostCall = body.IndexOf(call, StringComparison.Ordinal);
            Assert.True(hostCall >= 0, call + " missing");
            Assert.True(body.IndexOf("NotifyStateChanged()", StringComparison.Ordinal) > hostCall, signature + " does not announce the state after its transition");
        }
    }

    [Fact]
    public void TheControllerKeepsTheRestOfItsMembersAsTheyWereAndStartsNoThreadNorTimer()
    {
        var code = Controller();

        Assert.Matches(@"public\s+void\s+RefreshPanel\s*\(\s*\)\s*=>\s*_host\.RefreshPanel\s*\(\s*\)\s*;", code);
        Assert.Matches(@"public\s+void\s+SetModalRackCadActive\s*\(\s*bool\s+active\s*\)\s*=>\s*_host\.SetModalRackCadActive\s*\(\s*active\s*\)\s*;", code);
        Assert.Matches(@"public\s+void\s+SetInstrumentationEnabled\s*\(\s*bool\s+enabled\s*\)\s*=>\s*_host\.SetInstrumentationEnabled\s*\(\s*enabled\s*\)\s*;", code);
        Assert.Equal(1, Src.Count(code, @"_bridge\.Start\s*\("));
        Assert.DoesNotMatch(new Regex(@"\bTimer\b|\bThread\b|\bTask\b|\basync\b|Dispatcher|UnhandledException"), code);
    }

    // ------------------------------------------------------------------ INV-F1-T3B-07, 11, 13: the boundary of the UI folder

    [Fact]
    public void TheUiFolderHoldsTheRootControlItsViewModelAndNoWindow()
    {
        var files = UiSources();

        Assert.Contains(files, f => f.Path == ViewXamlPath);
        Assert.Contains(files, f => f.Path == ViewCodePath);
        Assert.Contains(files, f => f.Path == ViewModelPath);
        var xaml = files.Single(f => f.Path == ViewXamlPath).Code;
        Assert.Matches(@"<UserControl\b", xaml);
        Assert.Equal(1, Src.Count(xaml, @"<UserControl(?=[\s>])"));
        Assert.Matches(@"x:Class=""RackCad\.UI\.Workspace\.WorkspacePanelView""", xaml);
        foreach (var (path, code) in files)
            Assert.DoesNotMatch(new Regex(@"<Window\b|:\s*Window\b|\bnew\s+Window\b|ShowDialog|MessageBox"), code);
    }

    [Fact]
    public void TheUiFolderNeverNamesAutodeskThePluginTheControllerTheHostOrTheConcretePort()
    {
        var files = UiSources();
        Assert.True(files.Count >= 4, "the UI folder is missing files: " + files.Count);

        foreach (var (path, code) in files)
        {
            Assert.DoesNotMatch(new Regex(@"Autodesk|RackCad\.Plugin"), code);
            Assert.DoesNotMatch(new Regex(@"\bWorkspaceController\b|\bWorkspacePaletteHost\b|\bWorkspaceHostPort\b|\bWorkspaceSelectionReader\b|\bWorkspaceHostCore\b|\bAutoCadEventBridge\b|\bEventBridgeCore\b"), code);
        }
    }

    [Fact]
    public void TheUiFolderUsesOnlyTheNeutralContractsOfTheApplicationWorkspace()
    {
        var model = Src.Code(ViewModelPath);

        Assert.Matches(@"using\s+RackCad\.Application\.Workspace\s*;", model);
        Assert.Matches(@"sealed\s+class\s+WorkspacePanelViewModel\s*:\s*ObservableObject\b", model);
        Assert.Matches(@"public\s+WorkspacePanelViewModel\s*\(\s*IWorkspaceHostPort\s+\w+\s*,\s*Action<bool>\s+\w+\s*\)", model);
        Assert.Matches(@"\.GetSnapshot\s*\(\s*\)", model);
        Assert.Matches(@"\.GetMeasurementReport\s*\(\s*\)", model);
        Assert.DoesNotMatch(new Regex(@"\.Authorize\s*\(|\.GetSelectionContext\s*\("), model);
        Assert.DoesNotMatch(new Regex(@"\bApplication\.Current\b|\bGetOrCreate\b|ServiceLocator|\bSingleton\b|\.Instance\b|static\s+(?!readonly\s+string\b)\w+\s+\w+\s*;"), model);
    }

    [Fact]
    public void TheUiFolderHasNoTimerNoPollingNoThreadNoAsyncAndNoGlobalExceptionHandler()
    {
        foreach (var (path, code) in UiSources())
        {
            Assert.DoesNotMatch(
                new Regex(@"DispatcherTimer|\bDispatcher\b|\w*Timer\b|System\.Timers|\bTask\b|\basync\b|\bawait\b|\bThread\b|ThreadPool|BackgroundWorker|SynchronizationContext|Parallel\.|Sleep\s*\(|UnhandledException|AppDomain"),
                code);
        }
    }

    [Fact]
    public void TheUiFolderWritesNothingAndCarriesNoMetricsConceptOrNamesOfI63()
    {
        foreach (var (path, code) in UiSources())
        {
            Assert.DoesNotMatch(new Regex(@"System\.IO|\bFile\.|\bDirectory\.|Registry|UserSettings|RackLog|XData|Xrecord"), code);
            Assert.DoesNotMatch(new Regex(@"Metric|Provider|\bBom|ProjectSummary|TotalRacks|Aggregat|ComputedParameter", RegexOptions.IgnoreCase), code);
        }
    }

    [Fact]
    public void TheUiCodeFilesDeclareOneBlockScopedNamespaceOfTheirFolder()
    {
        foreach (var (path, code) in Src.Files(UiFolder))
        {
            var text = Src.Text(path);
            var declarations = text.Split('\n').Where(l => l.StartsWith("namespace ", StringComparison.Ordinal)).Select(l => l.TrimEnd()).ToList();
            Assert.Equal(new[] { "namespace RackCad.UI.Workspace" }, declarations);
        }
    }

    [Fact]
    public void TheRootControlMergesTheSharedStylesInItsOwnResourcesWithComponentUris()
    {
        var xaml = StripXamlComments(Src.Text(ViewXamlPath));

        Assert.Matches(@"<UserControl\.Resources>", xaml);
        Assert.Matches(@"<ResourceDictionary\s+Source=""/RackCad\.UI;component/Themes/AppStyles\.xaml""\s*/>", xaml);
        foreach (Match source in Regex.Matches(xaml, @"Source=""(?<s>[^""]+\.xaml)"""))
            Assert.Contains(";component/", source.Groups["s"].Value);
        Assert.False(Src.Exists("src/RackCad.UI/App.xaml"), "an App.xaml appeared");
        Assert.DoesNotMatch(new Regex(@"Application\.Resources|Application\.Current"), xaml);
        Assert.Matches(@"AutomationProperties\.Name=""Workspace""", xaml);
    }

    [Fact]
    public void TheRootControlReachesTheViewModelOnlyThroughExplicitInteractionsAndContainsThem()
    {
        var code = Src.Code(ViewCodePath);

        Assert.Matches(@"partial\s+class\s+WorkspacePanelView\s*:\s*UserControl\b", code);
        Assert.Matches(@"public\s+const\s+string\s+RootLabel\s*=\s*""Workspace""\s*;", code);
        Assert.Matches(@"_viewModel\.Retry\s*\(", code);
        Assert.Matches(@"_viewModel\.SetInstrumentation\s*\(", code);
        Assert.DoesNotMatch(new Regex(@"_viewModel\.Refresh\s*\(|\bLoaded\b|\bIsVisibleChanged\b|\bSizeChanged\b|\bCompositionTarget\b"), code);
        Assert.Contains("catch (System.Exception", code);

        var xaml = Src.Text(ViewXamlPath);
        Assert.Contains("x:Name=\"RetryButton\"", xaml);
        Assert.Contains("x:Name=\"InstrumentationToggle\"", xaml);
    }

    // ------------------------------------------------------------------ INV-F1-T3B-14: G1, G2 and G3 are exact

    [Fact]
    public void TheRefinementsG1G2AndG3AreExactAndNothingWasRemovedFromTheT3AGuardFile()
    {
        var text = Src.Text(GuardPath);
        var code = Src.CodeOnly(text);

        // G1: by exact file, never by folder; Internal, any Dispatcher and any Timer stay forbidden there.
        Assert.Matches(@"string\.Equals\(path,\s*PaletteHostPath,\s*StringComparison\.Ordinal\)", code);
        Assert.Matches(@"PaletteHostPath\s*=\s*""src/RackCad\.Plugin/Workspace/WorkspacePaletteHost\.cs""", code);
        Assert.Matches(@"paletteHostForbidden\s*=\s*new\s+Regex\(@""Autodesk\\\.AutoCAD\\\.Internal\|\\bDispatcher\\b\|\\bTimer\\b""\)", code);
        // G2: whole identifiers, so the UI may name the port interface and still cannot name the controller.
        Assert.Contains(@"\bWorkspaceController\b|\bWorkspaceHostPort\b|\bWorkspaceSelectionReader\b|\bWorkspaceHostCore\b", code);
        // G3: the two bridge notices and no unsubscription.
        Assert.Matches(@"_bridge\\\.ActiveSessionChanged\\s\*\\\+=", code);
        Assert.Matches(@"Assert\.Equal\(2,\s*Src\.Count\(code,\s*@""\\\+=""\)\)", code);
        Assert.DoesNotMatch(new Regex(@"Skip\s*=|\[Fact\(Skip|\[Theory\(Skip"), text);
        Assert.Equal(43, Src.Count(code, @"\[(?:Fact|Theory)\]"));
    }

    // ------------------------------------------------------------------ F1-T3B-HOST-UI-C1: deterministic first placement (D-01)
    // Smoke-1 R20261013T011654Z-184b stopped at SM-03: the palette, created with no size and no position, floated, became
    // unusable and left the screen before the Owner could see the panel. The palette is now placed ONCE, when the process
    // creates it: a device-independent size and minimum size, a floating rectangle that fits a 1024 x 768 primary screen and
    // an initial dock to the right. Show() only shows it, so any move or dock of the user is kept for the rest of the process;
    // nothing is persisted (no identity, no handlers of persistence, no registry, no profile).

    private const string PlacementSignature = @"private\s+void\s+ApplyInitialPlacement\s*\(\s*\)";
    private const string ConstructorSignature = @"private\s+WorkspacePaletteHost\s*\(\s*\)";

    // The geometry members of the palette: they may be touched only inside the first placement.
    private static readonly string[] GeometryPatterns =
    {
        @"\.Dock\s*=(?!=)", @"\bDeviceIndependentSize\b", @"\bDeviceIndependentMinimumSize\b", @"\bDeviceIndependentLocation\b",
        @"\.Size\s*=(?!=)", @"\.MinimumSize\s*=(?!=)", @"\.Location\s*=(?!=)", @"\bInitializeFloatingPosition\b",
        @"\bRecalculateDockSiteLayout\b", @"\bFloatControl\b", @"\bRolledUp\b", @"\bAutoRollUp\b", @"\bTitleBarLocation\b",
    };

    private static double ConstantOf(string code, string name)
    {
        var match = Regex.Match(code, @"private\s+const\s+(?:double|int)\s+" + name + @"\s*=\s*(\d+(?:\.\d+)?)\s*;");
        Assert.True(match.Success, "constant not found: " + name);
        return double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
    }

    [Fact]
    public void TheFirstCreationConfiguresAnExplicitDeviceIndependentPlacementAndDocksThePalette()
    {
        var code = Host();
        var placement = Src.BodyOf(code, PlacementSignature);

        Assert.Matches(@"_palette\.DeviceIndependentSize\s*=\s*new\s+System\.Windows\.Size\s*\(\s*InitialWidth\s*,\s*InitialHeight\s*\)\s*;", placement);
        Assert.Matches(@"_palette\.DeviceIndependentMinimumSize\s*=\s*new\s+System\.Drawing\.Size\s*\(\s*MinimumWidth\s*,\s*MinimumHeight\s*\)\s*;", placement);
        Assert.Matches(@"_palette\.InitializeFloatingPosition\s*\(\s*new\s+System\.Windows\.Rect\s*\(\s*FloatingLeft\s*,\s*FloatingTop\s*,\s*InitialWidth\s*,\s*InitialHeight\s*\)\s*\)\s*;", placement);
        Assert.Matches(@"_palette\.Dock\s*=\s*DockSides\.Right\s*;", placement);
        Assert.True(placement.IndexOf(".Dock", StringComparison.Ordinal) > placement.IndexOf("DeviceIndependentSize", StringComparison.Ordinal),
            "the palette is docked after it has its size");

        // The initial dock side is one of the enabled sides, so the palette stays acoplable and floating (D-01).
        Assert.Matches(@"_palette\.DockEnabled\s*=\s*DockSides\.Left\s*\|\s*DockSides\.Right\s*;", code);

        // Visibly usable: a real size, a minimum that does not exceed it, and a floating rectangle on a 1024 x 768 primary screen.
        var width = ConstantOf(code, "InitialWidth");
        var height = ConstantOf(code, "InitialHeight");
        var minWidth = ConstantOf(code, "MinimumWidth");
        var minHeight = ConstantOf(code, "MinimumHeight");
        var left = ConstantOf(code, "FloatingLeft");
        var top = ConstantOf(code, "FloatingTop");
        Assert.InRange(width, 240, 600);
        Assert.InRange(height, 300, 700);
        Assert.InRange(minWidth, 1, width);
        Assert.InRange(minHeight, 1, height);
        Assert.InRange(left, 0, 1024 - width);
        Assert.InRange(top, 0, 768 - height);

        // A failure of the placement is contained and logged: the panel is still shown.
        Assert.Contains("try", placement);
        Assert.Contains("catch (System.Exception", placement);
        Assert.Contains("LogFault(", placement);
    }

    [Fact]
    public void RepeatedShowNeverReappliesTheFirstPlacementAndTheUserKeepsMovesAndDocking()
    {
        var code = Host();
        var constructor = Src.BodyOf(code, ConstructorSignature);
        var placement = Src.BodyOf(code, PlacementSignature);
        var show = Src.BodyOf(code, @"public\s+void\s+Show\s*\(\s*\)");

        // The placement runs once per process: one declaration and one call, the call in the constructor, after the root
        // control is added and before the subscriptions (which stay the last step).
        Assert.Equal(2, Src.Count(code, @"\bApplyInitialPlacement\s*\("));
        Assert.Equal(1, Src.Count(constructor, @"\bApplyInitialPlacement\s*\(\s*\)\s*;"));
        var add = constructor.IndexOf("AddVisual", StringComparison.Ordinal);
        var place = constructor.IndexOf("ApplyInitialPlacement", StringComparison.Ordinal);
        var subscribe = constructor.IndexOf("+=", StringComparison.Ordinal);
        Assert.True(add >= 0 && place > add && subscribe > place, "the placement goes after AddVisual and before the subscriptions");

        // Show() only shows: it never touches the geometry, so a repeated RACKPANEL never resets what the user moved or docked.
        var showStatements = Regex.Replace(show.Substring(show.IndexOf('{') + 1).TrimEnd().TrimEnd('}'), @"\s+", " ").Trim();
        Assert.Equal("_palette.Visible = true;", showStatements);
        Assert.Equal(1, Src.Count(code, @"\.Visible\s*=(?!=)"));

        // Every geometry member of the palette is touched only inside the first placement.
        foreach (var pattern in GeometryPatterns)
            Assert.True(Src.Count(code, pattern) == Src.Count(placement, pattern), "geometry outside the first placement: " + pattern);
    }

    [Fact]
    public void TheFirstPlacementAddsNoPersistenceIdentityAutoloadProfileOrSecurityMechanism()
    {
        var code = Host();

        Assert.Matches(@"\bnew\s+PaletteSet\s*\(\s*""RackCad""\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"\bGuid\b|toolID|ToolId|\bLoad\b|\bSave\b|\bSaving\b|PalettePersist"), code);
        Assert.DoesNotMatch(
            new Regex(@"Registry|UserSettings|settings\.json|System\.IO|\bFile\.|Environment\.SetEnvironmentVariable|SetSystemVariable|SetEnvironmentVariable|TRUSTEDPATHS|SECURELOAD|CPROFILE|\bProfiles?\b|ApplicationPlugins|PackageContents|LOADCTRLS|DemandLoad|Preferences"),
            code);

        // The placement follows no move nor size of the user and keeps nothing: still exactly the two subscriptions of T3B.
        Assert.Equal(2, Src.Count(code, @"\+="));
        Assert.DoesNotMatch(new Regex(@"PaletteSetMoved|PaletteSetHostMoved|PaletteSetEnterSizeMove|SizeChanged|PaletteSetDestroy"), code);

        // No new file carries the placement: the plugin workspace folder keeps its six files.
        Assert.Equal(
            new[]
            {
                "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs", "src/RackCad.Plugin/Workspace/WorkspaceController.cs",
                "src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs", "src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs",
                "src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs", "src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs",
            },
            Src.PluginFiles().Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void TheExistingStyleDockingCapabilityAndKeepFocusOfThePaletteAreIntact()
    {
        var code = Host();
        var constructor = Src.BodyOf(code, ConstructorSignature);

        Assert.Equal(1, Src.Count(code, @"\bnew\s+PaletteSet\s*\("));
        Assert.Matches(
            @"_palette\.Style\s*=\s*PaletteSetStyles\.ShowPropertiesMenu\s*\|\s*PaletteSetStyles\.ShowAutoHideButton\s*\|\s*PaletteSetStyles\.ShowCloseButton\s*;",
            constructor);
        Assert.Matches(@"_palette\.DockEnabled\s*=\s*DockSides\.Left\s*\|\s*DockSides\.Right\s*;", constructor);
        Assert.Matches(@"_palette\.KeepFocus\s*=\s*true\s*;", constructor);
        Assert.Matches(@"_palette\.AddVisual\s*\(\s*""Workspace""\s*,\s*_root\s*\)\s*;", constructor);
        Assert.Equal(1, Src.Count(code, @"\.Style\s*=(?!=)"));
        Assert.Equal(1, Src.Count(code, @"\.DockEnabled\s*=(?!=)"));
        Assert.Equal(1, Src.Count(code, @"\.KeepFocus\s*=(?!=)"));
    }
}
