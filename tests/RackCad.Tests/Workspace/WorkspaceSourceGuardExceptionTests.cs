#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-14. The guard of T2 is refined by TWO exceptions of EXACT FILE, never by folder, and
// everything else it forbade stays forbidden. These tests exercise the refined guard itself on injected text.
public class WorkspaceSourceGuardExceptionTests
{
    private const string Command = WorkspaceBridgeSourceGuardTests.CommandAdapterPath;
    private const string Reader = WorkspaceBridgeSourceGuardTests.ReaderAdapterPath;
    private const string Other = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";

    private const string RackPanel = "[CommandMethod(\"RACKPANEL\")]";

    [Fact]
    public void TheTwoExceptionsAreNamedFilesOfThePluginWorkspaceFolder()
    {
        Assert.Equal("src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs", Command);
        Assert.Equal("src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs", Reader);
        Assert.NotEqual(Command, Reader);
        Assert.All(new[] { Command, Reader }, p => Assert.EndsWith(".cs", p, StringComparison.Ordinal));
        Assert.All(new[] { Command, Reader }, p => Assert.DoesNotContain("*", p, StringComparison.Ordinal));
    }

    [Fact]
    public void TheCommandExceptionAllowsTheSingleRackPanelAttributeInTheCommandAdapter()
    {
        Assert.False(WorkspaceBridgeSourceGuardTests.DeclaresEntryPoint(Command, "class C { " + RackPanel + " public void M() {} }"));
    }

    [Theory]
    [InlineData("[CommandMethod(\"RACKPANEL\")] [CommandMethod(\"RACKPANEL\")] void M() {}")]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {} [CommandMethod(\"RPN\")] void B() {}")]
    [InlineData("[CommandMethod(\"RPN\")] void A() {}")]
    [InlineData("[CommandMethod(\"rackpanel\")] void A() {}")]
    [InlineData("[CommandMethod(\"RACKPANEL\", CommandFlags.Modal)] void A() {}")]
    [InlineData("[CommandMethod(\"RACKPANELS\")] void A() {}")]
    [InlineData("void A() {}")]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {} [assembly: CommandClass(typeof(C))]")]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {} class E : IExtensionApplication {}")]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {} [ModuleInitializer] static void I() {}")]
    public void TheCommandExceptionAllowsNoAliasNoSecondCommandAndNoOtherEntryPoint(string text)
    {
        Assert.True(WorkspaceBridgeSourceGuardTests.DeclaresEntryPoint(Command, text));
    }

    [Theory]
    [InlineData(Other)]
    [InlineData(Reader)]
    [InlineData("src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspacePanelCommand2.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/Sub/WorkspacePanelCommand.cs")]
    public void RackPanelIsForbiddenInEveryOtherFileEvenInTheSameFolder(string path)
    {
        Assert.True(WorkspaceBridgeSourceGuardTests.DeclaresEntryPoint(path, "class C { " + RackPanel + " public void M() {} }"));
    }

    [Fact]
    public void TheReaderExceptionAllowsSelectImpliedAndGetObjectInTheReaderAdapter()
    {
        var guard = WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(Reader);

        Assert.DoesNotMatch(guard, "var result = ed.SelectImplied();");
        Assert.DoesNotMatch(guard, "var o = tr.GetObject(id, OpenMode.ForRead);");
    }

    [Theory]
    [InlineData(Other)]
    [InlineData(Command)]
    [InlineData("src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceSelectionReader2.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/Sub/WorkspaceSelectionReader.cs")]
    public void TheReadApisStayForbiddenInEveryOtherFile(string path)
    {
        var guard = WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(path);

        Assert.Matches(guard, "var result = ed.SelectImplied();");
        Assert.Matches(guard, "var o = tr.GetObject(id, OpenMode.ForRead);");
    }

    [Theory]
    [InlineData("ed.SetImpliedSelection(ids);")]
    [InlineData("ed.SetCurrentView(view);")]
    [InlineData("ed.SelectAll();")]
    [InlineData("ed.SelectAtPoint(p);")]
    [InlineData("var r = ed.GetSelection();")]
    [InlineData("var r = ed.GetEntity(o);")]
    [InlineData("using (var tr = new OpenCloseTransaction()) { }")]
    [InlineData("ed.Regen();")]
    [InlineData("ed.UpdateScreen();")]
    [InlineData("doc.SendStringToExecute(\"X\", true, false, false);")]
    [InlineData("entity.Highlight();")]
    [InlineData("System.Windows.MessageBox.Show(\"x\");")]
    [InlineData("Application.ShowModalDialog(w);")]
    [InlineData("ed.ZoomExtents();")]
    public void TheReaderAdapterStillCannotWriteTheSelectionOrTheViewNorAskNorOpenDialogs(string injected)
    {
        Assert.Matches(WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(Reader), injected);
    }

    [Fact]
    public void TheRealWorkspaceFilesPassTheRefinedGuard()
    {
        var files = Src.PluginFiles();
        Assert.NotEmpty(files);
        foreach (var (path, code) in files)
        {
            var forbidden = WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(path);
            Assert.True(!forbidden.IsMatch(code), path + " calls " + forbidden.Match(code).Value);
            Assert.True(!WorkspaceBridgeSourceGuardTests.DeclaresEntryPoint(path, code), path + " declares an entry point");
        }
    }

    // ------------------------------------------------------------------ I-64 F2-T2A-NAVIGATION-HOST (INV-F2T2A-10, C-F2T2-08)
    // Three more exceptions, each by EXACT FILE. READ, DISCOVERY and WRITE are distinguished: the facts adapter READS to collect
    // facts; the effects adapter WRITES only the implicit selection and the view and READS only the already-selected target; no
    // file DISCOVERS but the facts adapter; the host holds the asynchronous command-context pattern and nothing else.

    private const string Facts = WorkspaceBridgeSourceGuardTests.FactsAdapterPath;
    private const string Effects = WorkspaceBridgeSourceGuardTests.EffectsAdapterPath;
    private const string NavigationHost = WorkspaceBridgeSourceGuardTests.HostAdapterPath;

    [Fact]
    public void TheThreeNavigationExceptionsAreNamedFilesOfThePluginWorkspaceFolder()
    {
        Assert.Equal("src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs", Facts);
        Assert.Equal("src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs", Effects);
        Assert.Equal("src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs", NavigationHost);
        Assert.Equal(5, new[] { Facts, Effects, NavigationHost, Command, Reader }.Distinct().Count());
        Assert.All(new[] { Facts, Effects, NavigationHost }, p => Assert.EndsWith(".cs", p, StringComparison.Ordinal));
        Assert.All(new[] { Facts, Effects, NavigationHost }, p => Assert.DoesNotMatch(new Regex(@"[*?]"), p));
        Assert.All(new[] { Facts, Effects, NavigationHost }, p => Assert.Equal(1, p.Substring("src/RackCad.Plugin/Workspace/".Length).Split('/').Length));
    }

    [Fact]
    public void TheEffectsExceptionWritesOnlyTheSelectionAndTheViewAndReadsOnlyTheSelectedTarget()
    {
        var guard = WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(Effects);

        Assert.DoesNotMatch(guard, "ed.SetImpliedSelection(ids);");
        Assert.DoesNotMatch(guard, "ed.SetCurrentView(view);");
        Assert.DoesNotMatch(guard, "var o = tr.GetObject(id, OpenMode.ForRead);");
    }

    [Theory]
    [InlineData("var result = ed.SelectImplied();")]
    [InlineData("ed.SelectAll();")]
    [InlineData("ed.SelectAtPoint(p);")]
    [InlineData("var r = ed.GetSelection();")]
    [InlineData("var r = ed.GetEntity(o);")]
    [InlineData("using (var tr = new OpenCloseTransaction()) { }")]
    [InlineData("ed.Regen();")]
    [InlineData("ed.UpdateScreen();")]
    [InlineData("doc.SendStringToExecute(\"X\", true, false, false);")]
    [InlineData("entity.Highlight();")]
    [InlineData("entity.Unhighlight();")]
    [InlineData("Application.ShowModalDialog(w);")]
    [InlineData("ed.ZoomExtents();")]
    [InlineData("ed.ZoomWindow(a, b);")]
    [InlineData("ed.PanTo(p);")]
    [InlineData("ed.StartCommand(c);")]
    public void TheEffectsAdapterStillCannotDiscoverZoomRegenerateHighlightOrAsk(string injected)
    {
        Assert.Matches(WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(Effects), injected);
    }

    [Fact]
    public void TheFactsExceptionAllowsTheReadOnlyAndNeverAWriteOfTheSelectionOrTheView()
    {
        var guard = WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(Facts);

        Assert.DoesNotMatch(guard, "var o = tr.GetObject(id, OpenMode.ForRead);");
        Assert.Matches(guard, "ed.SetImpliedSelection(ids);");
        Assert.Matches(guard, "ed.SetCurrentView(view);");
        Assert.Matches(guard, "var result = ed.SelectImplied();");
        Assert.Matches(guard, "ed.ZoomExtents();");
    }

    [Fact]
    public void TheHostHasNoReadAndNoWriteExceptionAtAll()
    {
        var guard = WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(NavigationHost);

        Assert.Matches(guard, "ed.SetImpliedSelection(ids);");
        Assert.Matches(guard, "ed.SetCurrentView(view);");
        Assert.Matches(guard, "var o = tr.GetObject(id, OpenMode.ForRead);");
        Assert.Matches(guard, "var result = ed.SelectImplied();");
    }

    [Theory]
    [InlineData(Other)]
    [InlineData(Command)]
    [InlineData(Reader)]
    [InlineData(Facts)]
    [InlineData(NavigationHost)]
    [InlineData("src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects2.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs.bak")]
    [InlineData("src/RackCad.Plugin/Workspace/Sub/WorkspaceNavigationEffects.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/Workspace*.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/")]
    public void TheWritesOfTheSelectionAndTheViewStayForbiddenInEveryFileButTheEffectsAdapter(string path)
    {
        var guard = WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(path);

        Assert.Matches(guard, "ed.SetImpliedSelection(ids);");
        Assert.Matches(guard, "ed.SetCurrentView(view);");
    }

    [Theory]
    [InlineData(Other)]
    [InlineData(Command)]
    [InlineData(NavigationHost)]
    [InlineData(Effects + ".bak")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts2.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/Sub/WorkspaceNavigationFacts.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/Workspace*.cs")]
    public void TheReadOfTheDrawingStaysForbiddenInEveryFileButTheReaderTheFactsAndTheEffects(string path)
    {
        Assert.Matches(WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(path), "var o = tr.GetObject(id, OpenMode.ForRead);");
    }

    [Fact]
    public void TheNavigationExceptionsAreComparedByFullPathEqualityNeverByPrefixOrFolder()
    {
        var guard = Src.Code("tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs");

        Assert.Equal(2, Src.Count(guard, @"string\.Equals\(path, (?:Facts|Effects)AdapterPath, StringComparison\.Ordinal\)"));
        Assert.DoesNotMatch(new Regex(@"\.(?:StartsWith|EndsWith|Contains)\s*\([^)]*Adapter"), guard);
        Assert.DoesNotMatch(new Regex(@"Skip\s*=|\[Fact\(Skip"), guard);
    }

    [Fact]
    public void TheAsynchronousPatternIsAllowedOnlyInTheHostFileAndOnlyInTheCommandContextForm()
    {
        var hostCode = Src.Code(NavigationHost);

        // The exception is the exact file: no other file of the two workspace folders may use async or await.
        var users = Src.PluginFiles().Concat(Src.ApplicationFiles()).Where(f => Regex.IsMatch(f.Code, @"\basync\b|\bawait\b")).Select(f => f.Path).ToArray();
        Assert.Equal(new[] { NavigationHost }, users);
        Assert.Matches(@"await\s+[\w.]+\.ExecuteInCommandContextAsync\s*\(", hostCode);
        Assert.DoesNotMatch(new Regex(@"\.Wait\s*\(|\.Result\b|GetAwaiter|GetResult|ConfigureAwait|Task\.Run|Task\.Factory|StartNew|ContinueWith|async\s+void"), hostCode);
    }

    [Fact]
    public void NoFolderWideExclusionWasIntroducedInTheT2Guard()
    {
        var guard = Src.Code("tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs");

        Assert.DoesNotMatch(new Regex(@"Skip\s*=|\[Fact\(Skip"), guard);
        // The two exceptions are compared by full path equality.
        Assert.Equal(2, Src.Count(guard, @"string\.Equals\(path, (?:Reader|Command)AdapterPath, StringComparison\.Ordinal\)"));
    }

    [Fact]
    public void TheT2GuardKeepsItsOtherProhibitionsAsTests()
    {
        var guard = Src.Code("tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs");

        foreach (var name in new[]
                 {
                     "AutoCadEventHookupsExistOnlyUnderPluginWorkspace",
                     "ThePluginWorkspaceHoldsExactlyOneSubscriberFileHookingEachGovernedSourceExactlyOnce",
                     "EveryNativeHandlerIsContainedSoNoExceptionReachesAutoCad",
                     "TransactionsAndDocumentLocksAppearOnlyInsideUsingInThePluginWorkspace",
                     "ThePluginWorkspaceDeclaresNoFieldOfATransactionDocumentOrDatabaseObjectType",
                     "ThePublicApiOfTheBridgeExposesNoAutoCadType",
                     "TheBridgeHasNoPersistenceNorAuthoredNorNodNorXdataWriteAuthority",
                     "NothingIsHookedWhenThePluginLoadsAndNoCommandOrInitializerIsAdded",
                     "NoExistingPluginCommandFileReferencesTheWorkspaceBridge",
                     "TheBridgeFilesCarryNoMetricsProviderProjectSummaryOrAggregationConcept",
                     "NoBridgeFileUsesTheInternalAutoCadNamespace",
                     "TheApplicationWorkspaceFilesStayFreeOfAutoCadAndOfThePlugin",
                 })
            Assert.Contains(name, guard);
    }
}
