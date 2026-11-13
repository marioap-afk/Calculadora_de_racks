#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F3-T2-BROWSER-UI: source guards of the host side and of the panel side of the browser (INV-F3T2-05, 08, 10, 11, 12, 14, 16).
// The Core test project cannot reference the Plugin (it needs AutoCAD) nor load WPF, so those files are read as text.
public class WorkspaceBrowserHostGuardTests
{
    private const string HostPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs";
    private const string FactsPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string PaletteHostPath = "src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs";
    private const string UiFolder = "src/RackCad.UI/Workspace/";
    private const string PanelModelPath = UiFolder + "WorkspacePanelViewModel.cs";
    private const string BrowserModelPath = UiFolder + "WorkspaceBrowserViewModel.cs";
    private const string XamlPath = UiFolder + "WorkspacePanelView.xaml";

    private static string Host() => Src.Code(HostPath);

    private static string Facts() => Src.Code(FactsPath);

    // ------------------------------------------------------------------ INV-F3T2-12: the host implements the port, in the same file

    [Fact]
    public void TheNavigationHostImplementsTheSummaryPortAndNoOtherFileDoesNorIsANewPluginFileNeeded()
    {
        var host = Host();

        Assert.Matches(@"\bclass\s+WorkspaceNavigationHost\s*:\s*IWorkspaceNavigationPort\s*,\s*IWorkspaceBrowserSummaryPort\b", host);
        Assert.Equal(
            new[] { HostPath },
            Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"class\s+\w+\s*:\s*[^{]*\bIWorkspaceBrowserSummaryPort\b")).Select(f => f.Path).ToArray());
        Assert.Equal(9, Src.PluginFiles().Count);
        Assert.Empty(Src.PluginFiles().Where(f => Regex.IsMatch(f.Path, @"Browser|Summary")));
    }

    [Fact]
    public void ReadSummaryAnswersInTheApprovedOrderWithAValueAndWithoutEffects()
    {
        var host = Host();
        var read = Src.BodyOf(host, @"private\s+BrowserRackSummary\s+ReadSummaryOnActiveDocument\s*\(");

        var document = read.IndexOf("MdiActiveDocument", StringComparison.Ordinal);
        var command = read.IndexOf("CommandInProgress", StringComparison.Ordinal);
        var quiescent = read.IndexOf("IsQuiescent", StringComparison.Ordinal);
        var window = read.IndexOf("_drainNoticeOpen", StringComparison.Ordinal);
        var authorize = read.IndexOf("_authorize(", StringComparison.Ordinal);
        var memory = read.IndexOf("_summaries.Read(", StringComparison.Ordinal);
        Assert.True(document >= 0 && command > document && quiescent > document && window > command && window > quiescent && authorize > window && memory > authorize,
            "order: active document, no command and the editor at rest, a read nobody asked for only in the drain window, session authorized, and only then the memory or the read");
        Assert.Matches(@"string\.IsNullOrEmpty\(\s*\w+\.CommandInProgress\s*\)", read);
        Assert.Matches(@"\.Editor\.IsQuiescent", read);
        Assert.Contains("BrowserSummaryStatus.Rejected", read);
        Assert.Contains("BrowserSummaryStatus.Busy", read);
        Assert.Equal(1, Src.Count(read, @"_summaries\.Read\s*\("));

        // INV-F3T2-21: a read that no explicit action asked for, with the window closed, answers Busy before the authorization.
        Assert.Matches(
            @"if\s*\(\s*!userRequested\s*&&\s*!_drainNoticeOpen\s*\)\s*return\s+BrowserRackSummary\.Without\s*\(\s*BrowserSummaryStatus\.Busy\b",
            read);
        Assert.Matches(@"private\s+BrowserRackSummary\s+ReadSummaryOnActiveDocument\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*bool\s+userRequested\s*\)", host);

        // No effect: it writes no selection, no view and nothing of the drawing, takes no lock of its own and returns no focus.
        Assert.DoesNotMatch(
            new Regex(@"LockDocument|StartTransaction|_effects|_runner|ReturnFocusToDrawing|SetImpliedSelection|SetCurrentView|GetObject|\.Commit\s*\(|\.Focus\s*\(|OpenMode|ExecuteAsync"),
            read);
        Assert.Equal(2, Src.Count(host, @"\bReturnFocusToDrawing\s*\("));
        Assert.Equal(1, Src.Count(host, @"\.Focus\s*\("));
    }

    [Fact]
    public void AFailureOfTheSummaryIsContainedLoggedOnceAndReturnedAsTheFailedState()
    {
        var host = Host();
        var read = Src.BodyOf(host, @"public\s+BrowserRackSummary\s+ReadSummary\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*bool\s+userRequested\s*\)");

        Assert.Contains("ReadSummaryOnActiveDocument(", read);
        Assert.Matches(@"ReadSummaryOnActiveDocument\s*\(\s*\w+\s*,\s*\w+\s*,\s*userRequested\s*\)", read);
        Assert.Contains("catch (System.Exception", read);
        Assert.Contains("LogFault(\"WorkspaceNavigationHost.ReadSummary\"", read);
        Assert.Contains("BrowserSummaryStatus.Failed", read);
        Assert.DoesNotMatch(new Regex(@"\bthrow\b"), read);
        Assert.Equal(1, Src.Count(host, @"RackLog\.Exception\s*\("));
        Assert.Matches(@"public\s+bool\s+IsSessionAlive\s*\(\s*SessionId\s+\w+\s*\)", host);
    }

    [Fact]
    public void TheHostBuildsOneSummaryMemoryOverTheFactsAdapterAndFeedsItWithEveryDrain()
    {
        var host = Host();
        var drained = Src.BodyOf(host, @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");

        Assert.Equal(1, Src.Count(host, @"\bnew\s+BrowserRackSummaries\s*\("));
        Assert.Equal(1, Src.Count(host, @"\bnew\s+WorkspaceNavigationFacts\s*\("));
        Assert.Equal(1, Src.Count(host, @"\bnew\s+NavigationTargets\s*\("));
        Assert.Matches(@"new\s+BrowserRackSummaries\s*\(\s*\w+\.ReadBrowserFacts\s*,\s*_isSessionAlive\s*\)", host);
        Assert.Contains("_runner.OnDrained(", drained);
        Assert.Contains("_summaries.OnDrained(", drained);
        Assert.True(drained.IndexOf("_runner.OnDrained(", StringComparison.Ordinal) < drained.IndexOf("_summaries.OnDrained(", StringComparison.Ordinal));
        Assert.DoesNotMatch(new Regex(@"\+=|-="), host);
        Assert.Equal(new[] { HostPath }, Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bnew\s+BrowserRackSummaries\s*\(")).Select(f => f.Path).ToArray());
    }

    [Fact]
    public void TheFactsAdapterKeepsOneIntegratedScanForBothReadsAndLeaksNoAutoCadObject()
    {
        var facts = Facts();
        var browserFacts = Src.BodyOf(facts, @"public\s+IReadOnlyList<BrowserRackFact>\s+ReadBrowserFacts\s*\(\s*SessionId\s+\w+\s*\)");
        var definitionFacts = Src.Code(FactsPath);

        Assert.Equal(1, Src.Count(facts, @"RackBlockFinder\.ScanEnvelopes\s*\("));
        Assert.Matches(@"using\s*\(\s*\w+\.LockDocument\s*\(\s*\)\s*\)", facts);
        Assert.Matches(@"using\s*\(\s*var\s+\w+\s*=\s*[\w.]+\.StartTransaction\s*\(\s*\)\s*\)", facts);
        Assert.Matches(@"public\s+IReadOnlyList<NavigationDefinitionFact>\s+ReadDefinitionFacts\s*\(\s*SessionId\s+\w+\s*\)\s*=>\s*Scan\s*\(", definitionFacts);
        // INV-F3T2-23 (T2-F3): it uses the same scan, with the references of Model Space, and omits a definition that has none, so an erased
        // rack (its definition stays until a PURGE) is not a present rack. The handles themselves never leave the adapter.
        Assert.Contains("Scan(session, includeReferences: true)", browserFacts);
        Assert.DoesNotContain("includeReferences: false", facts);
        Assert.Matches(@"if\s*\(\s*\w+\.ModelSpaceReferenceHandles\.Count\s*==\s*0\s*\)\s*continue\s*;", browserFacts);
        Assert.Equal(1, Src.Count(browserFacts, @"ModelSpaceReferenceHandles"));
        Assert.Contains("new BrowserRackFact(", browserFacts);
        Assert.Matches(@"new\s+BrowserRackFact\s*\([^;]*\.Section\s*\)", browserFacts);
        Assert.DoesNotMatch(new Regex(@"new\s+BrowserRackFact\s*\([^;]*ModelSpaceReferenceHandles"), browserFacts);
        Assert.DoesNotMatch(new Regex(@"ScanEnvelopes|LockDocument|StartTransaction|GetBlockReferenceIds|OpenMode"), browserFacts);

        // Nothing public of the adapter names an AutoCAD type.
        foreach (Match member in Regex.Matches(facts, @"\bpublic\s+[^{;=]*\("))
            Assert.DoesNotMatch(new Regex(@"\b(?:ObjectId|Transaction|Document|Database|BlockTableRecord|BlockReference|Entity|DBObject)\b"), member.Value);
        Assert.DoesNotMatch(new Regex(@"\.Commit\s*\(|OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|AppendEntity|\.Erase\s*\(|SetImpliedSelection|SetCurrentView|SelectImplied|RackBlockData|RackEmbedStore|NavigationTargets|\.ReadDefinitionFacts\("), facts);
    }

    // ------------------------------------------------------------------ INV-F3T2-16: the exact wiring

    [Fact]
    public void TheControllerExposesTheSummaryPortFromTheSameLazyNavigationHostAndSubscribesToNothingNew()
    {
        var controller = Src.Code(ControllerPath);
        var constructor = Src.BodyOf(controller, @"private\s+WorkspaceController\s*\(\s*\)");

        Assert.Matches(@"public\s+IWorkspaceBrowserSummaryPort\s+BrowserSummary\s*=>\s*\(IWorkspaceBrowserSummaryPort\)Navigation\s*;", controller);
        Assert.Equal(1, Src.Count(controller, @"\bnew\s+WorkspaceNavigationHost\s*\("));
        Assert.DoesNotContain("BrowserSummary", constructor);
        Assert.Equal(3, Src.Count(controller, @"\+="));
        Assert.Equal(0, Src.Count(controller, @"-="));
        Assert.Matches(@"public\s+IWorkspaceNavigationPort\s+Navigation\b", controller);
    }

    // ------------------------------------------------------------------ INV-F3T2-21 (T2-F2): the drain window

    [Fact]
    public void TheHostHasOneBooleanDrainWindowThatOnlyTheTwoNoticeMethodsChange()
    {
        var host = Host();

        Assert.Matches(@"private\s+bool\s+_drainNoticeOpen\s*;", host);
        Assert.Matches(@"public\s+void\s+OpenDrainNotice\s*\(\s*\)\s*=>\s*_drainNoticeOpen\s*=\s*true\s*;", host);
        Assert.Matches(@"public\s+void\s+CloseDrainNotice\s*\(\s*\)\s*=>\s*_drainNoticeOpen\s*=\s*false\s*;", host);
        Assert.Equal(2, Src.Count(host, @"\b_drainNoticeOpen\s*="));
        // Besides the declaration, the one place that reads it is the answer of the summary.
        Assert.Equal(2, Src.Count(host, @"\b_drainNoticeOpen\b(?!\s*=)"));
        Assert.Equal(1, Src.Count(Src.BodyOf(host, @"private\s+BrowserRackSummary\s+ReadSummaryOnActiveDocument\s*\("), @"\b_drainNoticeOpen\b"));
        Assert.Equal(1, Src.Count(host, @"\bpublic\s+void\s+OpenDrainNotice\b"));
        Assert.Equal(1, Src.Count(host, @"\bpublic\s+void\s+CloseDrainNotice\b"));

        // The window is not a general permission: nothing else of the host reads it, and the actions of navigation do not depend on it.
        Assert.DoesNotContain("_drainNoticeOpen", Src.BodyOf(host, @"private\s+WorkspaceNavigationResult\s+RunOnActiveDocument\s*\("));
    }

    [Fact]
    public void TheControllerOpensTheWindowOnlyAroundTheNoticeOfADrainWithTryAndFinallyAndOnlyWhenTheHostExists()
    {
        var controller = Src.Code(ControllerPath);
        var body = Src.BodyOf(controller, @"private\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");

        var hostCall = body.IndexOf("_host.OnDrained(outcome)", StringComparison.Ordinal);
        var open = body.IndexOf("_navigation.OpenDrainNotice()", StringComparison.Ordinal);
        var announce = body.IndexOf("NotifyStateChanged()", StringComparison.Ordinal);
        var close = body.IndexOf("_navigation.CloseDrainNotice()", StringComparison.Ordinal);
        Assert.True(hostCall >= 0 && open > hostCall && announce > open && close > announce, "order: the drain, the window, the notice, the window closed");
        Assert.Matches(@"if\s*\(\s*_navigation\s*!=\s*null\s*\)\s*_navigation\.OpenDrainNotice\s*\(\s*\)\s*;", body);
        Assert.Matches(
            @"try\s*\{\s*NotifyStateChanged\s*\(\s*\)\s*;\s*\}\s*finally\s*\{\s*if\s*\(\s*_navigation\s*!=\s*null\s*\)\s*_navigation\.CloseDrainNotice\s*\(\s*\)\s*;\s*\}",
            body);
        Assert.Equal(1, Src.Count(body, @"NotifyStateChanged\s*\("));

        // Only here and nowhere else of the controller, and nobody else of the product opens or closes the window.
        Assert.Equal(1, Src.Count(controller, @"\.OpenDrainNotice\s*\("));
        Assert.Equal(1, Src.Count(controller, @"\.CloseDrainNotice\s*\("));
        Assert.Equal(
            new[] { ControllerPath },
            Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\.(?:Open|Close)DrainNotice\s*\(")).Select(f => f.Path).ToArray());

        // The relays of the other notices are untouched: a command state change only announces, and the document switch does not open it.
        Assert.Matches(@"private\s+void\s+OnCommandStateChanged\s*\(\s*\)\s*=>\s*NotifyStateChanged\s*\(\s*\)\s*;", controller);
        var session = Src.BodyOf(controller, @"private\s+void\s+OnActiveSessionChanged\s*\(\s*\)");
        Assert.DoesNotContain("DrainNotice", session);
        Assert.DoesNotContain("DrainNotice", Src.BodyOf(controller, @"private\s+void\s+OnCommandStateChanged\s*\(\s*\)"));
        Assert.DoesNotContain("DrainNotice", Src.BodyOf(controller, @"private\s+void\s+NotifyStateChanged\s*\(\s*\)"));
    }

    [Fact]
    public void ThePaletteHostOnlyPassesTheSummaryPortOfTheControllerAndNeverNamesItsType()
    {
        var host = Src.Code(PaletteHostPath);

        Assert.Equal(1, Src.Count(host, @"_controller\.BrowserSummary\b"));
        Assert.Equal(1, Src.Count(host, @"_controller\.Navigation\b"));
        // I-64 F4-T3B1 (INV-F4T3B1-01, G-01): the fifth argument is the edit port of the pilot of the controller; the palette host names no type of it.
        Assert.Matches(
            @"new\s+WorkspacePanelViewModel\s*\(\s*_controller\.Port\s*,\s*_controller\.SetInstrumentationEnabled\s*,\s*_controller\.Navigation\s*,\s*_controller\.BrowserSummary\s*,\s*_controller\.PilotEdit\s*\)",
            host);
        Assert.DoesNotMatch(new Regex(@"IWorkspaceBrowserSummaryPort|BrowserRackSummar|WorkspaceBrowser"), host);
        Assert.Equal(2, Src.Count(host, @"\+="));
    }

    [Fact]
    public void ThePanelViewModelGainsOnlyTheFourthOptionalSummaryParameterAndBuildsOneBrowser()
    {
        var panel = Src.Code(PanelModelPath);

        // I-64 F4-T3B1 (INV-F4T3B1-01, G-02 and G-03): the edit port of the pilot is the fifth, optional parameter, and the browser receives it as its third.
        Assert.Matches(
            @"public\s+WorkspacePanelViewModel\s*\(\s*IWorkspaceHostPort\s+\w+\s*,\s*Action<bool>\s+\w+\s*,\s*IWorkspaceNavigationPort\s+\w+\s*=\s*null\s*,\s*IWorkspaceBrowserSummaryPort\s+browserSummary\s*=\s*null\s*,\s*IWorkspacePilotEditPort\s+pilotEdit\s*=\s*null\s*\)",
            panel);
        Assert.Equal(1, Src.Count(panel, @"\bpublic\s+WorkspacePanelViewModel\s*\("));
        Assert.Matches(@"new\s+WorkspaceBrowserViewModel\s*\(\s*browserSummary\s*,\s*RepresentNavigation\s*,\s*pilotEdit\s*\)", panel);
        Assert.Equal(1, Src.Count(panel, @"\bnew\s+WorkspaceBrowserViewModel\s*\("));
        Assert.Equal(1, Src.Count(panel, @"\.GetSnapshot\s*\("));
        Assert.DoesNotMatch(new Regex(@"\.Authorize\s*\(|\.GetSelectionContext\s*\(|\.ReadSummary\s*\(|\.IsSessionAlive\s*\("), panel);
    }

    // ------------------------------------------------------------------ INV-F3T2-05, 08, 11, 14: the panel side

    [Fact]
    public void TheUiNeverNamesTheNavigationAuthorityTheHostOrTheFactsAndOnlyTheBrowserViewModelReadsTheSummaryPort()
    {
        var forbidden = new Regex(@"\b(?:NavigationTargets|NavigationTargetSet|NavigationTarget|NavigationDefinitionFact|INavigationFactsPort|BrowserRackFact|BrowserRackSummaries|WorkspaceNavigationHost|WorkspaceNavigationFacts)\b");

        foreach (var (path, code) in Src.Files(UiFolder))
        {
            Assert.DoesNotMatch(forbidden, code);
            Assert.Equal(path == BrowserModelPath ? 1 : 0, Src.Count(code, @"\.ReadSummary\s*\("));
            Assert.Equal(path == BrowserModelPath ? 1 : 0, Src.Count(code, @"\.IsSessionAlive\s*\("));
        }

        // The one in-memory registry of the panel: declared and created in ONE statement of the browser view-model, and named nowhere else.
        Assert.Equal(1, Src.Files(UiFolder).Sum(f => Src.Count(f.Code, @"\bBrowserSessionRegistry\b")));
        Assert.Matches(@"private\s+readonly\s+BrowserSessionRegistry\s+_registry\s*=\s*new\s*\(\s*\)\s*;", Src.Code(BrowserModelPath));
    }

    [Fact]
    public void NeitherTheUiNorTheHostLaunchesCommandsOrOffersTheClassicEditor()
    {
        var command = new Regex(@"SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync");

        foreach (var (path, code) in Src.Files(UiFolder).Concat(new[] { (HostPath, Host()), (FactsPath, Facts()) }))
            Assert.True(!command.IsMatch(code), path + " uses " + command.Match(code).Value);
        Assert.DoesNotMatch(new Regex(@"Abrir en editor|editor clásico|editor clasico", RegexOptions.IgnoreCase), Src.Text(XamlPath));
    }

    [Fact]
    public void TheBrowserViewModelIsSynchronousHasNoSubscriptionAndHoldsNoStaticStateNorHostObject()
    {
        var code = Src.Code(BrowserModelPath);

        Assert.DoesNotMatch(
            new Regex(@"\bTask\b|\basync\b|\bawait\b|\bDispatcher\b|\w*Timer\b|\bThread\b|ThreadPool|Sleep\s*\(|SynchronizationContext|Parallel\.|BackgroundWorker|\.Wait\s*\(|\.Result\b|GetAwaiter"),
            code);
        Assert.DoesNotMatch(new Regex(@"\+=|-=|\bevent\b|\bdelegate\b|\bstatic\s+(?!readonly\s+string\b)\w+\s+\w+\s*;"), code);
        Assert.DoesNotMatch(new Regex(@"Autodesk|RackCad\.Plugin|IWorkspaceNavigationPort|IWorkspaceHostPort|ExecuteAsync|NavigateAsync"), code);
        Assert.Matches(@"public\s+sealed\s+record\s+WorkspaceBrowserTabItem\s*\(\s*string\s+RackId\s*,\s*string\s+Title\s*,\s*bool\s+IsActive\s*,\s*bool\s+IsPreview\s*\)\s*;", code);
        Assert.Matches(@"sealed\s+class\s+WorkspaceBrowserViewModel\s*:\s*ObservableObject\b", code);
    }

    [Fact]
    public void TheDefaultBorderOfATabIsSetByTheStyleAndNotByAttributesSoTheTriggerOfTheActiveTabHasEffect()
    {
        // INV-F3T2-22 (T2-F1): a value set as an attribute of the Border outranks a trigger of its style, so the active tab would look like the
        // others. The defaults live in setters of the style; the trigger of IsActive overrides them.
        var xaml = Regex.Replace(Src.Text(XamlPath), @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
        var strip = xaml.Substring(xaml.IndexOf("x:Name=\"BrowserTabStrip\"", StringComparison.Ordinal));
        var border = Regex.Match(strip, @"<Border\b[^>]*>").Value;

        Assert.False(string.IsNullOrEmpty(border), "the template of the tab has no Border");
        Assert.DoesNotMatch(new Regex(@"\bBorderThickness\s*=|\bBorderBrush\s*="), border);

        var style = Regex.Match(strip, @"<Style\s+TargetType=""Border"">(?<defaults>.*?)<Style\.Triggers>(?<triggers>.*?)</Style\.Triggers>", RegexOptions.Singleline);
        Assert.True(style.Success, "the Border has no style with triggers");
        Assert.Matches(@"<Setter\s+Property=""BorderThickness""\s+Value=""1""\s*/>", style.Groups["defaults"].Value);
        Assert.Matches(@"<Setter\s+Property=""BorderBrush""\s+Value=""\{DynamicResource\s+ShellBorderBrush\}""\s*/>", style.Groups["defaults"].Value);
        Assert.Matches(@"<DataTrigger\s+Binding=""\{Binding\s+IsActive\}""\s+Value=""True"">", style.Groups["triggers"].Value);
        Assert.Matches(@"<Setter\s+Property=""BorderThickness""\s+Value=""2""\s*/>", style.Groups["triggers"].Value);
        Assert.Matches(@"<Setter\s+Property=""BorderBrush""\s+Value=""\{DynamicResource\s+ShellSelectionPrimaryBrush\}""\s*/>", style.Groups["triggers"].Value);
    }

    // I-64 F4-T3B1 (G-04): the surface is the COMPLETE element BrowserSurface, from its opening Border to the Border that balances it.
    // The openings counted are `<Border` followed by a space or `>` that are not closed by themselves (`/>`); `<Border.Style>` does not
    // count, and each `</Border>` closes one. Cutting at the first `</Border>` would end the surface at the first Border nested inside it.
    private static string SurfaceElement(string xaml)
    {
        var name = xaml.IndexOf("x:Name=\"BrowserSurface\"", StringComparison.Ordinal);
        Assert.True(name >= 0, "the XAML has no element BrowserSurface");
        var start = xaml.LastIndexOf("<Border", name, StringComparison.Ordinal);
        Assert.True(start >= 0, "BrowserSurface is not a Border");

        var depth = 0;
        foreach (Match tag in Regex.Matches(xaml, @"<Border(?=[\s>])[^>]*>|</Border>"))
        {
            if (tag.Index < start) continue;
            if (tag.Value == "</Border>") depth--;
            else if (!tag.Value.EndsWith("/>", StringComparison.Ordinal)) depth++;
            if (depth == 0) return xaml.Substring(start, tag.Index + tag.Length - start);
        }

        throw new InvalidOperationException("the Borders of BrowserSurface do not balance");
    }

    [Fact]
    public void TheSurfaceHasTheReloadButtonHandledInTheCodeBehindInsideContained()
    {
        var xaml = Regex.Replace(Src.Text(XamlPath), @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
        var surface = SurfaceElement(xaml);
        var code = Src.Code(UiFolder + "WorkspacePanelView.xaml.cs");

        // A lower bound, not an exact count: the later tasks add their buttons to the surface without touching this guard.
        Assert.True(Src.Count(surface, @"<Button\b") >= 3, "the surface has fewer buttons than F4-T3B1 declares");
        foreach (var (content, handler) in new[]
                 {
                     ("Volver a leer", "OnBrowserSurfaceReloadClick"),
                     ("Editar", "OnBrowserFieldEditClick"),
                     ("Recargar", "OnBrowserFieldReloadClick"),
                 })
        {
            Assert.Equal(1, Src.Count(surface, "Content=\"" + content + "\""));
            Assert.Equal(1, Src.Count(surface, "Click=\"" + handler + "\""));
            Assert.Matches(@"private\s+void\s+" + handler + @"\s*\(\s*object\s+sender\s*,\s*RoutedEventArgs\s+e\s*\)\s*=>\s*Contained\s*\(", code);
        }

        Assert.Matches(@"private\s+void\s+OnBrowserSurfaceReloadClick\s*\(\s*object\s+sender\s*,\s*RoutedEventArgs\s+e\s*\)\s*=>\s*Contained\s*\(\s*\(\s*\)\s*=>\s*_viewModel\.Browser\.ReloadSummary\s*\(\s*\)\s*\)\s*;", code);
        Assert.Equal(1, Src.Count(Src.Code(BrowserModelPath), @"\bpublic\s+void\s+ReloadSummary\s*\(\s*\)"));
        Assert.Equal(1, Src.Count(code, @"\.ReloadSummary\s*\("));
    }

    [Fact]
    public void TheSurfaceTramoIsTheCompleteElementWithABorderNestedInsideIt()
    {
        var surface = SurfaceElement(
            "<Border x:Name=\"BrowserSurface\"><StackPanel><Border><TextBlock /></Border><Button Content=\"inside\" /></StackPanel></Border><Button Content=\"outside\" />");

        Assert.Contains("Content=\"inside\"", surface);
        Assert.DoesNotContain("outside", surface);
        Assert.EndsWith("</Border>", surface);
    }

    [Fact]
    public void TheSurfaceTramoDoesNotCountABorderThatClosesItself()
    {
        var surface = SurfaceElement(
            "<Border x:Name=\"BrowserSurface\"><Border Margin=\"1\" /><Button Content=\"inside\" /></Border><Button Content=\"outside\" />");

        Assert.Contains("Content=\"inside\"", surface);
        Assert.DoesNotContain("outside", surface);
    }

    [Fact]
    public void TheSurfaceTramoDoesNotCountTheBorderStyleElement()
    {
        var surface = SurfaceElement(
            "<Border x:Name=\"BrowserSurface\"><Border.Style><Style TargetType=\"Border\" /></Border.Style><Button Content=\"inside\" /></Border><Button Content=\"outside\" />");

        Assert.Contains("Content=\"inside\"", surface);
        Assert.DoesNotContain("outside", surface);
    }

    [Fact]
    public void TheBrowserSectionIsBeforeTheNavigationHasNoFixedSizesAndReflowsInAWrapPanel()
    {
        var xaml = Regex.Replace(Src.Text(XamlPath), @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
        var start = xaml.IndexOf("Text=\"Navegador de racks\"", StringComparison.Ordinal);
        var navigation = xaml.IndexOf("Text=\"Navegación\"", StringComparison.Ordinal);
        var context = xaml.IndexOf("Text=\"Selección\"", StringComparison.Ordinal);

        Assert.True(context >= 0 && start > context && navigation > start, "the browser section goes after the selection and before the navigation");
        var section = xaml.Substring(start, navigation - start);
        Assert.DoesNotMatch(new Regex(@"\b(?:Min|Max)?Width\s*=|\b(?:Min|Max)?Height\s*="), section);
        Assert.DoesNotMatch(new Regex(@"<Window\b|ShowDialog|MessageBox"), section);
        Assert.Matches(@"<ItemsPanelTemplate>\s*<WrapPanel\b", section);
        Assert.Equal(1, Src.Count(section, @"x:Name=""BrowserSurface"""));
        Assert.DoesNotMatch(new Regex(@"<ItemsControl[^>]*BrowserSurface|<ContentControl|<TabControl|<TabItem", RegexOptions.Singleline), section);
        foreach (var handler in new[] { "OnBrowserTabActivateClick", "OnBrowserTabPinClick", "OnBrowserTabCloseClick" })
        {
            Assert.Contains(handler, section);
            Assert.Contains(handler, Src.Code(UiFolder + "WorkspacePanelView.xaml.cs"));
        }
    }
}
