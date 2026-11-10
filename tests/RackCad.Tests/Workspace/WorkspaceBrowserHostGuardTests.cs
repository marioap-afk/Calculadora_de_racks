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
        var authorize = read.IndexOf("_authorize(", StringComparison.Ordinal);
        var memory = read.IndexOf("_summaries.Read(", StringComparison.Ordinal);
        Assert.True(document >= 0 && command > document && quiescent > document && authorize > command && authorize > quiescent && memory > authorize,
            "order: active document, no command and the editor at rest, session authorized, and only then the memory or the read");
        Assert.Matches(@"string\.IsNullOrEmpty\(\s*\w+\.CommandInProgress\s*\)", read);
        Assert.Matches(@"\.Editor\.IsQuiescent", read);
        Assert.Contains("BrowserSummaryStatus.Rejected", read);
        Assert.Contains("BrowserSummaryStatus.Busy", read);
        Assert.Equal(1, Src.Count(read, @"_summaries\.Read\s*\("));

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
        var read = Src.BodyOf(host, @"public\s+BrowserRackSummary\s+ReadSummary\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)");

        Assert.Contains("ReadSummaryOnActiveDocument(", read);
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
        Assert.Contains("includeReferences: false", browserFacts);
        Assert.Contains("new BrowserRackFact(", browserFacts);
        Assert.DoesNotMatch(new Regex(@"ScanEnvelopes|LockDocument|StartTransaction|ModelSpaceReferenceHandles|GetBlockReferenceIds|OpenMode"), browserFacts);

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

    [Fact]
    public void ThePaletteHostOnlyPassesTheSummaryPortOfTheControllerAndNeverNamesItsType()
    {
        var host = Src.Code(PaletteHostPath);

        Assert.Equal(1, Src.Count(host, @"_controller\.BrowserSummary\b"));
        Assert.Equal(1, Src.Count(host, @"_controller\.Navigation\b"));
        Assert.Matches(
            @"new\s+WorkspacePanelViewModel\s*\(\s*_controller\.Port\s*,\s*_controller\.SetInstrumentationEnabled\s*,\s*_controller\.Navigation\s*,\s*_controller\.BrowserSummary\s*\)",
            host);
        Assert.DoesNotMatch(new Regex(@"IWorkspaceBrowserSummaryPort|BrowserRackSummar|WorkspaceBrowser"), host);
        Assert.Equal(2, Src.Count(host, @"\+="));
    }

    [Fact]
    public void ThePanelViewModelGainsOnlyTheFourthOptionalSummaryParameterAndBuildsOneBrowser()
    {
        var panel = Src.Code(PanelModelPath);

        Assert.Matches(
            @"public\s+WorkspacePanelViewModel\s*\(\s*IWorkspaceHostPort\s+\w+\s*,\s*Action<bool>\s+\w+\s*,\s*IWorkspaceNavigationPort\s+\w+\s*=\s*null\s*,\s*IWorkspaceBrowserSummaryPort\s+browserSummary\s*=\s*null\s*\)",
            panel);
        Assert.Equal(1, Src.Count(panel, @"\bpublic\s+WorkspacePanelViewModel\s*\("));
        Assert.Matches(@"new\s+WorkspaceBrowserViewModel\s*\(\s*browserSummary\s*,\s*RepresentNavigation\s*\)", panel);
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
