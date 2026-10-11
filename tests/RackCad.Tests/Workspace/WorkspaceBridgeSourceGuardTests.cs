#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T2-BRIDGE: source guards over src/RackCad.Plugin/Workspace/ (the one place the real AutoCAD subscriber
// may live) and over the whole product tree. INV-F1-T2-02, 03, 07, 08, 09, 12, 13, 14.
// The Core test project cannot reference the Plugin (it needs AutoCAD), so these read the sources as text.
public class WorkspaceBridgeSourceGuardTests
{
    private static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "RackCad.sln")))
            dir = Path.GetDirectoryName(dir);
        Assert.NotNull(dir);
        return dir!;
    }

    private static string CodeOnly(string text) =>
        Regex.Replace(Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\r\n]*", "");

    private static string Rel(string path) =>
        Path.GetRelativePath(RepoRoot(), path).Replace(Path.DirectorySeparatorChar, '/');

    private static IReadOnlyList<(string Path, string Code)> SourcesIn(params string[] segments)
    {
        var dir = Path.Combine(new[] { RepoRoot() }.Concat(segments).ToArray());
        if (!Directory.Exists(dir)) return Array.Empty<(string, string)>();
        return Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                        && !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
            .Select(f => (Rel(f), CodeOnly(File.ReadAllText(f))))
            .ToList();
    }

    private static IReadOnlyList<(string Path, string Code)> PluginWorkspace() => SourcesIn("src", "RackCad.Plugin", "Workspace");

    private static IReadOnlyList<(string Path, string Code)> ApplicationWorkspace() => SourcesIn("src", "RackCad.Application", "Workspace");

    private static IReadOnlyList<(string Path, string Code)> AllProduct() => SourcesIn("src");

    private const string PluginWorkspacePrefix = "src/RackCad.Plugin/Workspace/";

    // I-64 F1-T3A-HOST-CORE (INV-F1-T3A-14, C-T3-04): the ONLY two refinements of this guard, each by EXACT FILE and
    // never by folder. RACKPANEL may be declared in the command adapter; the read APIs of D-06 may be used in the reader
    // adapter. Every other file under src/RackCad.Plugin/Workspace/ keeps all the prohibitions below.
    internal const string CommandAdapterPath = "src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs";
    internal const string ReaderAdapterPath = "src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs";

    // ------------------------------------------------------------------ INV-F1-T2-02: hookups only in the bridge folder

    // The AutoCAD events the workspace governs plus the neighbours a second subscriber would reach for.
    private const string AutoCadEvents =
        "DocumentCreated|DocumentActivated|DocumentToBeDestroyed|DocumentDestroyed|DocumentToBeActivated|DocumentToBeDeactivated|"
        + "DocumentActivationChanged|DocumentBecameCurrent|DocumentLockModeChanged|DocumentLockModeWillChange|"
        + "ImpliedSelectionChanged|CommandWillStart|CommandEnded|CommandCancelled|CommandFailed|BeginDocumentClose|"
        + "EnteringQuiescentState|LeavingQuiescentState|ObjectAppended|ObjectErased|ObjectModified|ObjectUnappended|"
        + "ObjectReappended|ObjectOpenedForModify|ObjectClosed|SelectionAdded|SelectionRemoved|SystemVariableChanged|"
        + "BeginQuit|QuitWillStart|Idle";

    private static readonly Regex AutoCadEventHookup = new(@"\.(?:" + AutoCadEvents + @")\s*(?:\+=|-=)");

    private static readonly string[] GovernedEvents =
    {
        "DocumentCreated", "DocumentActivated", "DocumentToBeDestroyed", "ImpliedSelectionChanged", "CommandWillStart",
        "CommandEnded", "CommandCancelled", "CommandFailed", "BeginDocumentClose", "EnteringQuiescentState",
        "ObjectAppended", "ObjectErased", "ObjectModified", "ObjectUnappended", "ObjectReappended", "Idle",
    };

    [Fact]
    public void AutoCadEventHookupsExistOnlyUnderPluginWorkspace()
    {
        var all = AllProduct();
        Assert.True(all.Count > 200, "the sweep found too few product files: " + all.Count);

        var offenders = all
            .Where(s => !s.Path.StartsWith(PluginWorkspacePrefix, StringComparison.Ordinal))
            .Where(s => AutoCadEventHookup.IsMatch(s.Code))
            .Select(s => s.Path)
            .ToList();

        Assert.True(offenders.Count == 0, "AutoCAD event += / -= outside src/RackCad.Plugin/Workspace/:\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void ThePluginWorkspaceHoldsExactlyOneSubscriberFileHookingEachGovernedSourceExactlyOnce()
    {
        var sources = PluginWorkspace();
        Assert.NotEmpty(sources);

        var subscribers = sources.Where(s => AutoCadEventHookup.IsMatch(s.Code)).ToList();
        var subscriber = Assert.Single(subscribers);

        foreach (var name in GovernedEvents)
        {
            Assert.True(Regex.Matches(subscriber.Code, @"\." + name + @"\s*\+=").Count == 1, name + " must be hooked exactly once");
            Assert.True(Regex.Matches(subscriber.Code, @"\." + name + @"\s*-=").Count == 1, name + " must be unhooked exactly once");
        }

        // No hookup of an event outside the governed set.
        var hooked = AutoCadEventHookup.Matches(subscriber.Code)
            .Select(m => Regex.Match(m.Value, @"\.(\w+)").Groups[1].Value)
            .Distinct()
            .OrderBy(n => n)
            .ToArray();
        Assert.Equal(GovernedEvents.OrderBy(n => n).ToArray(), hooked);
    }

    [Theory]
    [InlineData("doc.CommandEnded += OnEnded;")]
    [InlineData("ed.EnteringQuiescentState -= h;")]
    [InlineData("db.ObjectModified += (s, e) => { };")]
    [InlineData("Application.Idle += OnIdle;")]
    [InlineData("AcApplication.DocumentManager.DocumentActivated += h;")]
    public void TheHookupGuardDetectsEachFamilyOfAutoCadEventOperator(string injected)
    {
        Assert.Matches(AutoCadEventHookup, injected);
        Assert.DoesNotMatch(AutoCadEventHookup, "button.Click += OnClick;");
    }

    // ------------------------------------------------------------------ INV-F1-T2-03: events never write selection or view

    private const string ForbiddenCallNames =
        @"SetImpliedSelection|SetCurrentView|ZoomExtents|ZoomWindow|Zoom|PanTo|UpdateScreen|Regen|SendStringToExecute|StartCommand|"
        + @"GetSelection|GetEntity|GetString|GetPoint|GetKeywords|GetInteger|GetDouble|SelectImplied|SelectAll|SelectAtPoint|"
        + @"ShowModalDialog|ShowModelessDialog|ShowDialog|MessageBox|GetObject|OpenCloseTransaction|Highlight|Unhighlight";

    private static readonly Regex HandlerForbiddenCalls = new(@"\b(?:" + ForbiddenCallNames + @")\b");

    // The reader adapter alone may call SelectImplied and GetObject (D-06); every other name stays forbidden there too.
    private static readonly Regex ReaderForbiddenCalls = new(
        @"\b(?:" + string.Join("|", ForbiddenCallNames.Split('|').Where(n => n != "SelectImplied" && n != "GetObject")) + @")\b");

    internal static Regex HandlerForbiddenCallsFor(string path) =>
        string.Equals(path, ReaderAdapterPath, StringComparison.Ordinal) ? ReaderForbiddenCalls : HandlerForbiddenCalls;

    [Fact]
    public void TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs()
    {
        var sources = PluginWorkspace();
        Assert.NotEmpty(sources);
        foreach (var (path, code) in sources)
        {
            var forbidden = HandlerForbiddenCallsFor(path);
            Assert.True(!forbidden.IsMatch(code), path + " calls " + forbidden.Match(code).Value);
        }
    }

    [Fact]
    public void EveryNativeHandlerIsContainedSoNoExceptionReachesAutoCad()
    {
        var subscriber = Assert.Single(PluginWorkspace().Where(s => AutoCadEventHookup.IsMatch(s.Code)));
        var handlerLines = subscriber.Code.Split('\n').Where(l => Regex.IsMatch(l, @"\bhandler\s*=\s*\(")).ToList();
        Assert.Equal(GovernedEvents.Length, handlerLines.Count);
        Assert.All(handlerLines, l => Assert.Matches(@"\bContain(?:Session)?\(", l));
    }

    [Theory]
    [InlineData("ed.SetImpliedSelection(ids);")]
    [InlineData("ed.SetCurrentView(view);")]
    [InlineData("var o = tr.GetObject(id, OpenMode.ForRead);")]
    [InlineData("var r = ed.GetSelection();")]
    [InlineData("Application.ShowModalDialog(w);")]
    public void TheHandlerGuardDetectsEachForbiddenFamily(string injected)
    {
        Assert.Matches(HandlerForbiddenCalls, injected);
    }

    // ------------------------------------------------------------------ INV-F1-T2-07: lifetime safety, no AutoCAD in the public API

    private static readonly Regex TransactionOrLockCall =
        new(@"\b(?:StartTransaction|StartOpenCloseTransaction|LockDocument)\s*\(");

    private static IEnumerable<string> UnguardedTransactionLines(string code) =>
        code.Split('\n').Where(l => TransactionOrLockCall.IsMatch(l) && !Regex.IsMatch(l, @"\busing\b"));

    private static IEnumerable<string> ForbiddenFieldLines(string code)
    {
        foreach (var line in code.Split('\n'))
        {
            var m = Regex.Match(line, @"^\s*(?:public|private|protected|internal)\s+(?<decl>[^=;{(]*)(?<term>[=;{(])");
            if (!m.Success || m.Groups["term"].Value == "(") continue;
            if (Regex.IsMatch(m.Groups["decl"].Value, @"\b(?:Transaction|OpenCloseTransaction|DBObject|Document|Entity|BlockReference)\b"))
                yield return line.Trim();
        }
    }

    private static IEnumerable<string> PublicSurfaceLeaks(string code)
    {
        foreach (var line in code.Split('\n'))
        {
            if (!Regex.IsMatch(line, @"^\s*(?:public|protected)\b")) continue;
            var signature = Regex.Replace(line, @"(=>|\{|;).*$", "");
            if (Regex.IsMatch(signature, @"\b(?:Document|DocumentCollection|Editor|Database|Transaction|OpenCloseTransaction|DBObject|ObjectId|Entity|Autodesk)\b|\w+EventArgs\b"))
                yield return line.Trim();
        }
    }

    [Fact]
    public void TransactionsAndDocumentLocksAppearOnlyInsideUsingInThePluginWorkspace()
    {
        var sources = PluginWorkspace();
        Assert.NotEmpty(sources);
        foreach (var (path, code) in sources)
            Assert.True(!UnguardedTransactionLines(code).Any(), path + " opens a transaction or lock outside a using");
    }

    [Fact]
    public void ThePluginWorkspaceDeclaresNoFieldOfATransactionDocumentOrDatabaseObjectType()
    {
        var sources = PluginWorkspace();
        Assert.NotEmpty(sources);
        foreach (var (path, code) in sources)
            Assert.True(!ForbiddenFieldLines(code).Any(), path + ": " + string.Join(" | ", ForbiddenFieldLines(code)));
    }

    [Fact]
    public void ThePublicApiOfTheBridgeExposesNoAutoCadType()
    {
        var sources = PluginWorkspace();
        Assert.NotEmpty(sources);
        foreach (var (path, code) in sources)
            Assert.True(!PublicSurfaceLeaks(code).Any(), path + ": " + string.Join(" | ", PublicSurfaceLeaks(code)));
    }

    [Theory]
    [InlineData("var tr = db.TransactionManager.StartTransaction();", true)]
    [InlineData("tr = db.TransactionManager.StartOpenCloseTransaction();", true)]
    [InlineData("var l = doc.LockDocument();", true)]
    [InlineData("using (var tr = db.TransactionManager.StartTransaction()) { }", false)]
    [InlineData("using var l = doc.LockDocument();", false)]
    public void TheTransactionGuardFlagsOnlyWhatIsOutsideAUsing(string injected, bool flagged)
    {
        Assert.Equal(flagged, UnguardedTransactionLines(injected).Any());
    }

    [Theory]
    [InlineData("private Transaction _tr;")]
    [InlineData("private readonly Document _doc = null;")]
    [InlineData("public DBObject Current { get; set; }")]
    [InlineData("internal static Entity last;")]
    public void TheFieldGuardFlagsEachForbiddenFieldShape(string injected)
    {
        Assert.NotEmpty(ForbiddenFieldLines(injected));
        Assert.Empty(ForbiddenFieldLines("private static Document FindDocument(long id) => null;"));
        Assert.Empty(ForbiddenFieldLines("private readonly Dictionary<SourceAttachment, Action> _detachers = new();"));
    }

    [Theory]
    [InlineData("public Document Current { get; }")]
    [InlineData("public void Attach(Database db)")]
    [InlineData("public event EventHandler<DocumentCollectionEventArgs> Changed;")]
    [InlineData("protected Editor Ed => null;")]
    public void TheApiGuardFlagsEachLeakOfAnAutoCadType(string injected)
    {
        Assert.NotEmpty(PublicSurfaceLeaks(injected));
        Assert.Empty(PublicSurfaceLeaks("public event Action<DrainOutcome> Drained;"));
    }

    // ------------------------------------------------------------------ INV-F1-T2-08: no persistence and no write authority

    private static readonly Regex PersistenceOrAuthoredWrite = new(
        @"System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\.|\bUserSettings|\bAtomicFile\b|"
        + @"NamedObjectsDictionary|GetNamedObjectsDictionary|\bDBDictionary\b|\bXrecord\b|\bXData\b|\bXdata\b|\bResultBuffer\b|"
        + @"AppendEntity|UpgradeOpen|DowngradeOpen|\.Commit\(|\.Erase\(|SetXDictionary|SaveAs|\bSave\(|DocumentExtension");

    [Fact]
    public void TheBridgeHasNoPersistenceNorAuthoredNorNodNorXdataWriteAuthority()
    {
        var sources = PluginWorkspace();
        Assert.NotEmpty(sources);
        foreach (var (path, code) in sources)
            Assert.True(!PersistenceOrAuthoredWrite.IsMatch(code), path + " uses " + PersistenceOrAuthoredWrite.Match(code).Value);
    }

    [Theory]
    [InlineData("File.WriteAllText(p, t);")]
    [InlineData("Registry.CurrentUser.SetValue(k, v);")]
    [InlineData("Microsoft.Win32.Registry.CurrentUser.CreateSubKey(k);")]
    [InlineData("var nod = db.NamedObjectsDictionaryId;" + " var d = GetNamedObjectsDictionary();")]
    [InlineData("rb = new ResultBuffer();")]
    [InlineData("tr.Commit();")]
    [InlineData("btr.AppendEntity(e);")]
    [InlineData("UserSettingsStore.Save();")]
    public void ThePersistenceGuardDetectsEachWriteFamily(string injected)
    {
        Assert.Matches(PersistenceOrAuthoredWrite, injected);
    }

    // ------------------------------------------------------------------ INV-F1-T2-09 and 13: no load-time start, no command change

    [Fact]
    public void NothingIsHookedWhenThePluginLoadsAndNoCommandOrInitializerIsAdded()
    {
        var initializer = File.ReadAllText(Path.Combine(RepoRoot(), "src", "RackCad.Plugin", "PluginInitializer.cs"));
        Assert.DoesNotContain("Workspace", initializer);
        Assert.DoesNotContain("EventBridge", initializer);

        var sources = PluginWorkspace();
        Assert.NotEmpty(sources);
        foreach (var (path, code) in sources)
            Assert.True(!DeclaresEntryPoint(path, code), path + " declares a load-time entry point or a command");
    }

    private static readonly Regex EntryPoints =
        new(@"IExtensionApplication|ExtensionApplication|\[CommandMethod|\[assembly\s*:|ModuleInitializer|static\s+AutoCadEventBridge\s*\(");

    /// <summary>
    /// A load-time entry point or a command. The command adapter, by exact file, may hold ONE <c>[CommandMethod("RACKPANEL")]</c>
    /// and nothing else of this kind: no alias, no second command, no other name.
    /// </summary>
    internal static bool DeclaresEntryPoint(string path, string code)
    {
        if (string.Equals(path, CommandAdapterPath, StringComparison.Ordinal))
        {
            var attributes = Regex.Matches(code, @"\[\s*CommandMethod\b").Count;
            var allowed = Regex.Matches(code, @"\[CommandMethod\(""RACKPANEL""\)\]").Count;
            if (attributes != 1 || allowed != 1) return true;
            code = Regex.Replace(code, @"\[CommandMethod\(""RACKPANEL""\)\]", "");
        }

        return EntryPoints.IsMatch(code);
    }

    [Fact]
    public void NoExistingPluginCommandFileReferencesTheWorkspaceBridge()
    {
        foreach (var (path, code) in SourcesIn("src", "RackCad.Plugin").Where(s => !s.Path.StartsWith(PluginWorkspacePrefix, StringComparison.Ordinal)))
            Assert.True(!Regex.IsMatch(code, @"AutoCadEventBridge|RackCad\.Plugin\.Workspace|RackCad\.Application\.Workspace"), path + " references the workspace bridge");
    }

    // ------------------------------------------------------------------ INV-F1-T2-12: nothing of I-63

    private static readonly Regex I63Concepts = new(
        @"ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom", RegexOptions.IgnoreCase);

    [Fact]
    public void TheBridgeFilesCarryNoMetricsProviderProjectSummaryOrAggregationConcept()
    {
        var sources = PluginWorkspace().Concat(ApplicationWorkspace()).ToList();
        Assert.NotEmpty(PluginWorkspace());
        foreach (var (path, code) in sources)
            Assert.True(!I63Concepts.IsMatch(code), path + " mentions " + I63Concepts.Match(code).Value);
    }

    [Theory]
    [InlineData("var x = ComputedParameters.Evaluate();")]
    [InlineData("class RackMetricsProvider {}")]
    [InlineData("var s = new ProjectSummary();")]
    [InlineData("int TotalRacks;")]
    [InlineData("Aggregate(items);")]
    [InlineData("BomBuilder.Build();")]
    public void TheI63GuardDetectsEachConcept(string injected)
    {
        Assert.Matches(I63Concepts, injected);
    }

    // ------------------------------------------------------------------ INV-F1-T2-14: no internal AutoCAD API

    [Fact]
    public void NoBridgeFileUsesTheInternalAutoCadNamespace()
    {
        Assert.NotEmpty(PluginWorkspace());
        foreach (var (path, code) in PluginWorkspace().Concat(ApplicationWorkspace()))
            Assert.DoesNotMatch(new Regex(@"Autodesk\.AutoCAD\.Internal"), code);
    }

    [Theory]
    [InlineData("using Autodesk.AutoCAD.Internal;")]
    [InlineData("Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();")]
    public void TheInternalNamespaceGuardDetectsItsTargets(string injected)
    {
        Assert.Matches(new Regex(@"Autodesk\.AutoCAD\.Internal"), injected);
    }

    // ------------------------------------------------------------------ INV-F1-T2-11: the model stays free of AutoCAD

    [Fact]
    public void TheApplicationWorkspaceFilesStayFreeOfAutoCadAndOfThePlugin()
    {
        var files = ApplicationWorkspace();
        Assert.True(files.Count >= 5, "expected the model and the pure bridge core");
        Assert.Contains(files, f => f.Path.EndsWith("/EventBridgeCore.cs", StringComparison.Ordinal));
        foreach (var (_, code) in files)
        {
            Assert.DoesNotMatch(new Regex(@"\bAutodesk\."), code);
            Assert.DoesNotMatch(new Regex(@"RackCad\.Plugin|RackCad\.UI|System\.Windows"), code);
        }
    }
}
