#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RackCad.Application.Diagnostics;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: source guards of the host adapters of src/RackCad.Plugin/Workspace/ (INV-F1-T3A-01, 02, 06, 08,
// 11, 12, 13). The Core test project cannot reference the Plugin (it needs AutoCAD), so these read the sources as text.
public class WorkspaceHostSourceGuardTests
{
    private const string CommandPath = WorkspaceBridgeSourceGuardTests.CommandAdapterPath;
    private const string ReaderPath = WorkspaceBridgeSourceGuardTests.ReaderAdapterPath;
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string PaletteHostPath = "src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs";
    private const string HostCorePath ="src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string InstrumentationPath = "src/RackCad.Application/Workspace/WorkspaceInstrumentation.cs";

    // I-64 F2-T2A-NAVIGATION-HOST: the exceptions of the navigation files, each by EXACT FILE and never by folder or prefix.
    private const string NavigationHostPath = WorkspaceBridgeSourceGuardTests.HostAdapterPath;
    private const string NavigationFactsPath = WorkspaceBridgeSourceGuardTests.FactsAdapterPath;
    private const string NavigationEffectsPath = WorkspaceBridgeSourceGuardTests.EffectsAdapterPath;

    private static IEnumerable<(string Path, string Code)> PluginOutside(string path) =>
        Src.PluginFiles().Where(f => f.Path != path);

    // ------------------------------------------------------------------ INV-F1-T3A-02: the RACKPANEL command

    [Fact]
    public void RackPanelIsRegisteredExactlyOnceAndOnlyInTheCommandAdapter()
    {
        var owners = Src.AllProduct()
            .Select(f => (f.Path, Count: Src.Count(f.Code, @"\[CommandMethod\(""RACKPANEL""\)\]")))
            .Where(x => x.Count > 0)
            .ToList();

        var owner = Assert.Single(owners);
        Assert.Equal(CommandPath, owner.Path);
        Assert.Equal(1, owner.Count);
    }

    [Fact]
    public void TheCommandAdapterHoldsNoOtherCommandAndNoAlias()
    {
        var code = Src.Code(CommandPath);

        Assert.Equal(1, Src.Count(code, @"\[\s*CommandMethod\b"));
        Assert.DoesNotMatch(new Regex(@"CommandFlags|\[assembly|CommandClass|LispFunction|IExtensionApplication"), code);
    }

    [Fact]
    public void NoOtherFileOfThePluginWorkspaceDeclaresACommand()
    {
        foreach (var (path, code) in PluginOutside(CommandPath))
            Assert.DoesNotMatch(new Regex(@"\[\s*CommandMethod\b"), code);
    }

    [Fact]
    public void TheCommandOnlyObtainsTheControllerAndOpensThePanel()
    {
        var code = Src.Code(CommandPath);

        Assert.Contains("WorkspaceController.GetOrCreate()", code);
        Assert.Matches(@"\.OpenPanel\s*\(\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"\bnew\s+WorkspaceController\b|PaletteSet|AddVisual|System\.Windows|AutoCadEventBridge|StartTransaction|LockDocument|SelectImplied|GetObject"), code);
    }

    [Fact]
    public void TheCommandContainsEveryExceptionAndReportsItThroughTheSharedReporter()
    {
        var code = Src.Code(CommandPath);

        Assert.Contains("catch (System.Exception", code);
        Assert.Contains("RackCommandSupport.Report(", code);
    }

    [Fact]
    public void OnlyTheCommandAdapterWritesToTheCommandLine()
    {
        foreach (var (path, code) in PluginOutside(CommandPath))
            Assert.DoesNotMatch(new Regex(@"\bWriteMessage\b"), code);
    }

    // ------------------------------------------------------------------ INV-F1-T3A-01: one lazy controller

    [Fact]
    public void TheControllerIsASingleNewFileCreatedLazilyThroughTheHolder()
    {
        var code = Src.Code(ControllerPath);

        Assert.Matches(@"\bsealed\s+class\s+WorkspaceController\b", code);
        Assert.Matches(@"WorkspaceLazyInstance<WorkspaceController>", code);
        Assert.Equal(1, Src.Count(code, @"\bnew\s+WorkspaceController\s*\("));
        Assert.Matches(@"\(\s*\)\s*=>\s*new\s+WorkspaceController\s*\(", code);
        Assert.Matches(@"private\s+WorkspaceController\s*\(", code);
        Assert.Matches(@"public\s+static\s+WorkspaceController\s+GetOrCreate\s*\(", code);
    }

    [Fact]
    public void NoStaticConstructorNorModuleInitializerCreatesAnythingWhenThePluginLoads()
    {
        foreach (var (path, code) in Src.PluginFiles())
        {
            Assert.DoesNotMatch(new Regex(@"\bstatic\s+(?:Workspace\w+|AutoCadEventBridge)\s*\(\s*\)"), code);
            Assert.DoesNotMatch(new Regex(@"ModuleInitializer|IExtensionApplication|\[assembly"), WithoutTheRackPanelAttribute(path, code));
        }
    }

    private static string WithoutTheRackPanelAttribute(string path, string code) =>
        path == CommandPath ? code.Replace("[CommandMethod(\"RACKPANEL\")]", string.Empty) : code;

    [Fact]
    public void TheControllerIsNeverBuiltByAStaticInitializer()
    {
        var code = Src.Code(ControllerPath);

        Assert.DoesNotMatch(new Regex(@"static\s+readonly\s+WorkspaceController\b"), code);
        Assert.DoesNotMatch(new Regex(@"static\s+WorkspaceController\s+\w+\s*=\s*new\b"), code);
    }

    [Fact]
    public void ExactlyOneBridgeIsConstructedAndOnlyByTheController()
    {
        var builders = Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bnew\s+AutoCadEventBridge\s*\(")).Select(f => f.Path).ToList();

        Assert.Equal(new[] { ControllerPath }, builders);
        Assert.Equal(1, Src.Count(Src.Code(ControllerPath), @"\bnew\s+AutoCadEventBridge\s*\("));
    }

    [Fact]
    public void TheBridgeIsStartedOnlyFromTheOpenPathNeverFromTheConstructor()
    {
        var code = Src.Code(ControllerPath);

        Assert.Equal(1, Src.Count(code, @"_bridge\.Start\s*\("));
        var constructor = Src.BodyOf(code, @"private\s+WorkspaceController\s*\(\s*\)");
        Assert.DoesNotContain(".Start(", constructor);
        Assert.DoesNotContain("OpenPanel", constructor);
        Assert.DoesNotContain("RequestReconciliation", constructor);
    }

    [Fact]
    public void NothingOutsideThePluginWorkspaceReachesTheController()
    {
        var offenders = Src.AllProduct()
            .Where(f => !f.Path.StartsWith(Src.PluginWorkspace, StringComparison.Ordinal))
            .Where(f => Regex.IsMatch(f.Code, @"\bWorkspaceController\b|\bWorkspaceHostPort\b|\bWorkspaceSelectionReader\b|\bWorkspaceHostCore\b"))
            .Where(f => !f.Path.StartsWith(Src.ApplicationWorkspace, StringComparison.Ordinal))
            .Select(f => f.Path)
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void ThePluginInitializerIsUntouched()
    {
        var initializer = Src.Text("src/RackCad.Plugin/PluginInitializer.cs");

        Assert.DoesNotContain("Workspace", initializer);
        Assert.DoesNotContain("RACKPANEL", initializer);
        Assert.DoesNotContain("Controller", initializer);
    }

    [Fact]
    public void TheControllerGoesThroughTheFacadesOfTheSingleBridge()
    {
        var code = Src.Code(ControllerPath);

        Assert.Matches(@"_bridge\.ActiveSession", code);
        Assert.Matches(@"_bridge\.TryGetSession\s*\(", code);
        Assert.Matches(@"_bridge\.Authorize\s*\(", code);
        Assert.Matches(@"_bridge\.RequestReconciliation\s*\(", code);
        Assert.DoesNotMatch(new Regex(@"WorkspaceSessionRegistry|EventBridgeCore"), code);
    }

    [Fact]
    public void TheControllerSubscribesOnlyToTheDrainedEventOfTheBridge()
    {
        var code = Src.Code(ControllerPath);

        // I-64 F1-T3B (G3): exactly the bridge-internal notifications, and no unsubscription. I-64 F1-T3-HOST-C2 (C2-A) adds
        // exactly one: the command-state notice.
        Assert.Equal(3, Src.Count(code, @"\+="));
        Assert.Matches(@"_bridge\.Drained\s*\+=", code);
        Assert.Matches(@"_bridge\.ActiveSessionChanged\s*\+=", code);
        Assert.Matches(@"_bridge\.CommandStateChanged\s*\+=", code);
        Assert.DoesNotMatch(new Regex(@"-="), code);
    }

    [Fact]
    public void ThePanelVisiblePolicyIsMirroredToTheBridgeThroughTheLink()
    {
        var code = Src.Code(ControllerPath);

        Assert.Matches(@"_bridge\.SetPanelVisible\s*\(", code);
        Assert.Matches(@"_bridge\.SetModalRackCadActive\s*\(", code);
    }

    // ------------------------------------------------------------------ INV-F1-T3A-06: the fine selection reader

    [Fact]
    public void TheReaderIsASingleFileThatImplementsTheReaderAbstraction()
    {
        var code = Src.Code(ReaderPath);

        Assert.Matches(@"\bclass\s+WorkspaceSelectionReader\s*:\s*IWorkspaceSelectionReader\b", code);
        var implementers = Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"class\s+\w+\s*:\s*[^{]*\bIWorkspaceSelectionReader\b")).Select(f => f.Path).ToList();
        Assert.Equal(new[] { ReaderPath }, implementers);
    }

    [Fact]
    public void TheReaderReadsTheImplicitSelectionAndTheDefinitionInReadOnlyModeWithTheIntegratedReader()
    {
        var code = Src.Code(ReaderPath);

        Assert.Matches(@"\bSelectImplied\s*\(", code);
        Assert.Matches(@"\bGetObject\s*\(", code);
        Assert.Contains("OpenMode.ForRead", code);
        Assert.Matches(@"RackBlockData\.Read\s*\(", code);
        Assert.Matches(@"new\s+RackEmbedStore\s*\(\s*\)\s*\.Deserialize\s*\(", code);
    }

    [Fact]
    public void TheReaderNeverOpensForWriteNorUpgradesNorCommitsNorCreatesANewCodec()
    {
        var code = Src.Code(ReaderPath);

        Assert.DoesNotMatch(new Regex(@"OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|DowngradeOpen|\.Commit\s*\(|AppendEntity|\.Erase\s*\("), code);
        Assert.DoesNotMatch(new Regex(@"JsonSerializer|JsonDocument|Utf8JsonReader|Newtonsoft|XmlSerializer"), code);
    }

    [Fact]
    public void TheReaderLocksTheDocumentAndOpensItsShortTransactionOnlyInsideUsing()
    {
        var code = Src.Code(ReaderPath);

        Assert.Matches(@"using\s*\(\s*\w+\.LockDocument\s*\(\s*\)\s*\)", code);
        Assert.Matches(@"using\s*\(\s*var\s+\w+\s*=\s*[\w.]+\.StartTransaction\s*\(\s*\)\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"StartOpenCloseTransaction|OpenCloseTransaction"), code);
    }

    [Fact]
    public void TheReaderChecksInDebugThatNoTransactionIsLeftOpen()
    {
        var code = Src.Code(ReaderPath);

        Assert.Contains("TopTransaction", code);
        Assert.Matches(@"Debug\.Assert\s*\(", code);
    }

    [Fact]
    public void TheReaderPrefiltersByClassAndCountsNestedExternalAndPaperSpaceReferencesAsNotRack()
    {
        var code = Src.Code(ReaderPath);

        Assert.Contains("ObjectClass", code);
        Assert.Contains("OwnerId", code);
        Assert.Matches(@"IsFromExternalReference", code);
        Assert.Matches(@"IsFromOverlayReference", code);
    }

    [Fact]
    public void TheReaderReflectsPickFirstAsStateAndNeverChangesIt()
    {
        var code = Src.Code(ReaderPath);

        Assert.Matches(@"GetSystemVariable\s*\(\s*""PICKFIRST""\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"SetSystemVariable"), code);
    }

    [Fact]
    public void TheReaderKeepsADefinitionCacheScopedBySessionAndInvalidatesItOnRequest()
    {
        var code = Src.Code(ReaderPath);

        Assert.Contains("WorkspaceDefinitionCache", code);
        Assert.Matches(@"_cache\.TryGet\s*\(", code);
        Assert.Matches(@"_cache\.Store\s*\(", code);
        Assert.Matches(@"_cache\.Invalidate\s*\(", code);
    }

    [Fact]
    public void TheReaderApisAreUsedInNoOtherFileOfThePluginWorkspace()
    {
        var readerApis = new Regex(@"\bSelectImplied\b|\bGetObject\b|RackBlockData|RackEmbedStore|WorkspaceDefinitionCache");

        // I-64 F2-T2A (INV-F2T2A-05, 06, 10): two more files, each by EXACT FILE, may READ with GetObject and nothing else of
        // the reader APIs: the facts adapter (to collect the facts) and the effects adapter (the one already-selected target).
        var readOnly = new Regex(@"\bSelectImplied\b|RackBlockData|RackEmbedStore|WorkspaceDefinitionCache");

        foreach (var (path, code) in PluginOutside(ReaderPath))
        {
            var allowsRead = string.Equals(path, NavigationFactsPath, StringComparison.Ordinal)
                             || string.Equals(path, NavigationEffectsPath, StringComparison.Ordinal);
            var rule = allowsRead ? readOnly : readerApis;
            Assert.True(!rule.IsMatch(code), path + " uses " + rule.Match(code).Value);
        }

        // The READ of the effects adapter is not a discovery authority: one GetObject, read-only, on the one target the plan
        // named; no enumeration of Model Space or of the block table, no sibling or alternative reference, no derivation of targets.
        var effects = Src.Code(NavigationEffectsPath);
        Assert.Equal(1, Src.Count(effects, @"\bGetObject\s*\("));
        Assert.Matches(@"GetObject\s*\(\s*id\s*,\s*OpenMode\.ForRead\s*\)", effects);
        Assert.DoesNotMatch(
            new Regex(@"ScanEnvelopes|RackBlockFinder|RackEmbed\w*|\bBlockTable\b|BlockTableRecord|GetBlockReferenceIds|NavigationTargets|ModelSpaceReferenceHandles|ReadDefinitionFacts|\bforeach\s*\([^)]*\bin\s+(?:modelSpace|blockTable|space|btr)\b"),
            effects);
    }

    [Fact]
    public void TheEffectsWriteExceptionOpensNoOtherWriteNoSystemVariableNoLayoutAndNoAuthoredData()
    {
        var effects = Src.Code(NavigationEffectsPath);

        Assert.Equal(1, Src.Count(effects, @"\bSetImpliedSelection\s*\("));
        Assert.Equal(2, Src.Count(effects, @"\bSetCurrentView\s*\("));
        Assert.DoesNotMatch(new Regex(@"TileMode|LayoutManager|CurrentLayout|OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|AppendEntity|\.Erase\s*\(|\.Commit\s*\(|SetSystemVariable"), effects);
        var writers = Src.PluginFiles().Where(f => Regex.IsMatch(f.Code, @"\bSetImpliedSelection\b|\bSetCurrentView\b")).Select(f => f.Path).ToArray();
        Assert.Equal(new[] { NavigationEffectsPath }, writers);
    }

    [Fact]
    public void TheReaderIsBuiltOnlyByTheControllerAndCalledOnlyFromTheDrain()
    {
        var users = Src.PluginFiles().Where(f => f.Path != ReaderPath && Regex.IsMatch(f.Code, @"\bWorkspaceSelectionReader\b")).Select(f => f.Path).ToList();
        Assert.Equal(new[] { ControllerPath }, users);
        Assert.Equal(1, Src.Count(Src.Code(ControllerPath), @"\bnew\s+WorkspaceSelectionReader\s*\("));

        var core = Src.Code(HostCorePath);
        Assert.Equal(1, Src.Count(core, @"_reader\.Read\s*\("));
        var drain = Src.BodyOf(core, @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        Assert.Contains("_reader.Read(", drain);
        Assert.Contains("MayRead", drain);
    }

    [Fact]
    public void TheDrainInvalidatesTheCacheFirstAndOnlyHintsSayWhen()
    {
        var core = Src.Code(HostCorePath);
        var drain = Src.BodyOf(core, @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");

        Assert.Equal(1, Src.Count(core, @"_reader\.Invalidate\s*\("));
        Assert.True(drain.IndexOf("_reader.Invalidate(", StringComparison.Ordinal) < drain.IndexOf("_reader.Read(", StringComparison.Ordinal));
        Assert.Contains("DrainOperation.InvalidateNavigationCaches", drain);
        Assert.Contains("DrainOperation.ReadImplicitSelection", drain);
    }

    [Fact]
    public void TheContextPathIsClassifyThenApplyContextAndNothingElseWritesTheContext()
    {
        var mapper = Src.Code("src/RackCad.Application/Workspace/WorkspaceSelectionRead.cs");
        var core = Src.Code(HostCorePath);

        Assert.Matches(@"SelectionContext\.Classify\s*\(", mapper);
        Assert.Equal(1, Src.Count(core, @"\.ApplyContext\s*\("));
        Assert.Equal(1, Src.Count(core, @"WorkspaceSelectionMapper\.ToContext\s*\("));
        Assert.DoesNotMatch(new Regex(@"SelectionContext\.(?:One|Selection)\s*\("), core);
    }

    [Fact]
    public void NoRackIdIsEverStampedOrInventedByTheHost()
    {
        foreach (var (path, code) in Src.PluginFiles().Concat(Src.ApplicationFiles()))
            Assert.DoesNotMatch(new Regex(@"Guid\.NewGuid|new\s+RackEmbedDocument|RackEmbedComposer|RackBlockData\.Write"), code);
    }

    // ------------------------------------------------------------------ INV-F1-T3A-08 and 13: no write, no persistence, no background work

    [Fact]
    public void NoHostFileWritesTheSelectionTheViewOrAnySystemVariable()
    {
        var forbidden = new Regex(@"\b(?:SetImpliedSelection|SetCurrentView|SetSystemVariable|Zoom\w*|Regen|UpdateScreen|Highlight|Unhighlight|SendStringToExecute|SetImpliedSelectionAsync|PanTo)\b");

        // I-64 F2-T2A (INV-F2T2A-06, 10): the effects adapter, by EXACT FILE, is the only one that writes the implicit selection
        // and the view; the system variables, every zoom, the regeneration, the highlight and the command strings stay forbidden there.
        var effectsForbidden = new Regex(@"\b(?:SetSystemVariable|Zoom\w*|Regen|UpdateScreen|Highlight|Unhighlight|SendStringToExecute|SetImpliedSelectionAsync|PanTo)\b");

        foreach (var (path, code) in Src.PluginFiles().Concat(Src.ApplicationFiles()))
        {
            var rule = string.Equals(path, NavigationEffectsPath, StringComparison.Ordinal) ? effectsForbidden : forbidden;
            Assert.True(!rule.IsMatch(code), path + " uses " + rule.Match(code).Value);
        }
    }

    [Fact]
    public void NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType()
    {
        var forbidden = new Regex(@"Autodesk\.AutoCAD\.Internal|PaletteSet|AddVisual|System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|\bUserControl\b|\bDispatcher\b");

        // I-64 F1-T3B (G1): the visual host of the palette, by EXACT FILE and never by folder, may name the palette, the
        // root control and the public WPF host types; it keeps the internal AutoCAD namespace, any Dispatcher and any
        // Timer forbidden (the guards of background work and of DWG access keep applying to it as to every other file).
        var paletteHostForbidden = new Regex(@"Autodesk\.AutoCAD\.Internal|\bDispatcher\b|\bTimer\b");

        // I-64 F4-T2C (D-T2C-04, IC-04): the event bridge, by EXACT FILE and never by folder, may name the Dispatcher and
        // System.Windows.Threading, only to post its idle continuation with BeginInvoke at background priority. Everything else
        // stays forbidden there, and so do the other ways of running or waiting on the dispatcher: any name ending in Timer,
        // DispatcherFrame, PushFrame, Yield, InvokeAsync and the Invoke call on the dispatcher.
        var bridgeForbidden = new Regex(@"Autodesk\.AutoCAD\.Internal|PaletteSet|AddVisual|RackCad\.UI|Autodesk\.AutoCAD\.Windows|\bUserControl\b|System\.Windows(?!\.Threading\b)|\w*Timer\b|DispatcherFrame|PushFrame|\bYield\b|InvokeAsync|(?:_dispatcher|CurrentDispatcher)\s*\.\s*Invoke\b");

        foreach (var (path, code) in Src.PluginFiles().Concat(Src.ApplicationFiles()))
        {
            var rule = string.Equals(path, PaletteHostPath, StringComparison.Ordinal) ? paletteHostForbidden
                : string.Equals(path, BridgePath, StringComparison.Ordinal) ? bridgeForbidden
                : forbidden;
            Assert.True(!rule.IsMatch(code), path + " uses " + rule.Match(code).Value);
        }
    }

    [Fact]
    public void NoHostFileStartsABackgroundThreadOrAsyncWork()
    {
        var forbidden = new Regex(@"Task\.Run|Task\.Factory|\bnew\s+Thread\b|ThreadPool|System\.Threading\.Timer|System\.Timers|\bTimer\b|\basync\b|\bawait\b|Parallel\.|BackgroundWorker|SynchronizationContext");

        // I-64 F2-T2A (INV-F2T2A-02, 03, 10): the navigation host, by EXACT FILE, was the only one allowed async and await; since F2-T2-HOST-C1 it runs
        // synchronously in application context (IC-01) and uses neither. Everything else that starts background work stays forbidden there, and so does every way
        // of blocking on a task, detaching one or dropping the synchronization context.
        var hostForbidden = new Regex(@"Task\.Run|Task\.Factory|\bnew\s+Thread\b|ThreadPool|System\.Threading\.Timer|System\.Timers|\bTimer\b|Parallel\.|BackgroundWorker|SynchronizationContext|\.Wait\s*\(|\.Result\b|GetAwaiter|GetResult|ConfigureAwait|StartNew|ContinueWith|Task\.Delay|async\s+void");
        var blocking = new Regex(@"GetAwaiter|ConfigureAwait|\.Wait\s*\(");

        foreach (var (path, code) in Src.PluginFiles().Concat(Src.ApplicationFiles()))
        {
            var rule = string.Equals(path, NavigationHostPath, StringComparison.Ordinal) ? hostForbidden : forbidden;
            Assert.True(!rule.IsMatch(code), path + " uses " + rule.Match(code).Value);
            Assert.True(!blocking.IsMatch(code), path + " blocks on or reconfigures a task: " + blocking.Match(code).Value);
        }

        foreach (var (path, code) in Src.PluginFiles())
            Assert.DoesNotMatch(new Regex(@"\.Result\b"), code);
    }

    [Fact]
    public void TheNavigationHostHasNoAsynchronousExecutionAndNoInternalCommand()
    {
        var host = Src.Code(NavigationHostPath);

        Assert.Equal(0, Src.Count(host, @"\basync\b"));
        Assert.Equal(0, Src.Count(host, @"\bawait\b"));
        var holders = Src.PluginFiles().Concat(Src.ApplicationFiles()).Where(f => Regex.IsMatch(f.Code, @"\basync\b|\bawait\b")).Select(f => f.Path).ToArray();
        Assert.Empty(holders);
        Assert.DoesNotMatch(new Regex(@"ExecuteInCommandContextAsync|SendStringToExecute|\[CommandMethod|Autodesk\.AutoCAD\.Internal|DllImport|HwndSource"), host);
    }

    [Fact]
    public void NoHostFileWritesAFileTheRegistryTheProfileOrTheStoredData()
    {
        var forbidden = new Regex(@"System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\b|\bXrecord\b|\bXData\b|\bDBDictionary\b|NamedObjectsDictionary|AtomicFile|UserSettings|Environment\.SetEnvironmentVariable");

        foreach (var (path, code) in Src.PluginFiles().Concat(Src.ApplicationFiles()))
            Assert.True(!forbidden.IsMatch(code), path + " uses " + forbidden.Match(code).Value);
    }

    [Fact]
    public void EveryTransactionAndLockInTheHostIsInsideUsingAndNoTypeOfTheHostIsHeldInAField()
    {
        foreach (var (path, code) in Src.PluginFiles())
        {
            foreach (var line in code.Split('\n').Where(l => Regex.IsMatch(l, @"\b(?:StartTransaction|StartOpenCloseTransaction|LockDocument)\s*\(")))
                Assert.True(Regex.IsMatch(line, @"\busing\b"), path + ": " + line.Trim());

            foreach (var line in code.Split('\n').Where(l => Regex.IsMatch(l, @"^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:Transaction|DBObject|Document|Database|Entity|BlockReference|ObjectId|Editor)\b[^(]*[;=]")))
                Assert.Fail(path + " keeps a host object: " + line.Trim());
        }
    }

    // ------------------------------------------------------------------ INV-F1-T3A-11: honest, in-memory instrumentation

    [Fact]
    public void RackLogKeepsItsTwoOriginalOperationsAndNothingElse()
    {
        var names = typeof(RackLog)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "Exception", "Warning" }, names);
    }

    [Fact]
    public void NoHostFileLogsAMeasurementAsAWarning()
    {
        foreach (var (path, code) in Src.PluginFiles().Concat(Src.ApplicationFiles()))
            Assert.DoesNotMatch(new Regex(@"RackLog\.Warning"), code);
    }

    [Fact]
    public void ThePureHostCodeNeverTouchesTheLog()
    {
        foreach (var (path, code) in Src.ApplicationFiles())
            Assert.DoesNotMatch(new Regex(@"RackLog|RackCad\.Application\.Diagnostics"), code);
    }

    [Fact]
    public void TheNewPluginFilesLogOnlyFaultsAndOnlyInsideAFaultMethod()
    {
        var newFiles = Src.PluginFiles().Where(f => f.Path != BridgePath).ToList();
        Assert.NotEmpty(newFiles);
        foreach (var (path, code) in newFiles)
        {
            var remaining = code;
            var match = Regex.Match(code, @"static\s+void\s+LogFault\s*\([^)]*\)");
            if (match.Success) remaining = code.Replace(Src.BodyOf(code, @"static\s+void\s+LogFault\s*\([^)]*\)"), string.Empty);
            Assert.DoesNotMatch(new Regex(@"RackLog\."), remaining);
        }
    }

    [Fact]
    public void TheInstrumentationWritesNowhere()
    {
        var code = Src.Code(InstrumentationPath);

        Assert.DoesNotMatch(new Regex(@"RackLog|Console\.|Trace\.|Debug\.|EventLog|\bFile\b|Directory|System\.IO|Registry|Logger|SetEnvironmentVariable"), code);
    }

    [Fact]
    public void ThereIsNoGlobalLoggingApiInTheHostFolders()
    {
        foreach (var (path, code) in Src.PluginFiles().Concat(Src.ApplicationFiles()))
            Assert.DoesNotMatch(new Regex(@"\b(?:class|interface|struct|record)\s+\w*Log\w*\b"), code);
    }

    [Fact]
    public void TheInstrumentationNamesCarryNoMetricsConcept()
    {
        var names = typeof(WorkspaceInstrumentation).Assembly.GetTypes()
            .Where(t => t.Namespace == "RackCad.Application.Workspace" && t.Name.Contains("Measurement", StringComparison.Ordinal))
            .Select(t => t.Name)
            .ToList();

        Assert.NotEmpty(names);
        Assert.All(names, n => Assert.DoesNotMatch(new Regex("Metric|Provider|Bom|Aggregat|ProjectSummary|TotalRacks", RegexOptions.IgnoreCase), n));
    }

    // ------------------------------------------------------------------ INV-F1-T3A-18 and the folder discipline

    [Fact]
    public void TheNewFilesOfBothFoldersCarryNoConceptOfI63()
    {
        var concepts = new Regex(@"ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom", RegexOptions.IgnoreCase);

        foreach (var (path, code) in Src.PluginFiles().Concat(Src.ApplicationFiles()))
            Assert.True(!concepts.IsMatch(code), path + " mentions " + concepts.Match(code).Value);
    }

    [Fact]
    public void TheNewFilesAreExactlyTheOnesTheContractNames()
    {
        var plugin = Src.PluginFiles().Select(f => f.Path).ToHashSet(StringComparer.Ordinal);
        foreach (var expected in new[] { CommandPath, ReaderPath, ControllerPath, "src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs", BridgePath })
            Assert.Contains(expected, plugin);

        var application = Src.ApplicationFiles().Select(f => f.Path).ToHashSet(StringComparer.Ordinal);
        foreach (var expected in new[]
                 {
                     "IWorkspaceHostPort.cs", "WorkspaceHostSnapshot.cs", "WorkspaceHostPolicy.cs", "WorkspaceLazyInstance.cs",
                     "WorkspaceSelectionRead.cs", "WorkspaceInstrumentation.cs", "IWorkspaceBridgeLink.cs", "WorkspaceHostCore.cs",
                 })
            Assert.Contains(Src.ApplicationWorkspace + expected, application);
    }
}
