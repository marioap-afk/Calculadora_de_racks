#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T3C2-COMMAND-ADVISOR (INV-F4T3C2-02, 04, 06, 07, 08, 10, 14; D-T3C2-03, 05, 07, 10): source guards of the command
// RACKPANELDESCARTAR, of the wiring of the wrapper of the advisor and of the Spanish reasons of the Plugin. The Core test project cannot
// reference the Plugin (it needs AutoCAD), so the Plugin files are read as text. Every guard is a function over text that looks at ONE
// member body (never at the whole file when the member is what matters), and each one has its self-test in the negative: the real file
// passes and a text that breaks the rule is flagged. The exact installation of the wrapper, in order and by expressions that tolerate
// spaces and line breaks, lives in WorkspaceLifecycleGuardTests.
public class WorkspaceCommandAdvisorGuardTests
{
    private const string CommandPath = WorkspaceBridgeSourceGuardTests.CommandAdapterPath;
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string NavigationHostPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs";
    private const string IntentAdapterPath = "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs";
    private const string AdvisorPath = "src/RackCad.Application/Workspace/IWorkspaceLifecycleAdvisor.cs";
    private const string DiscardCorePath = "src/RackCad.Application/Workspace/WorkspaceDocumentDiscardCommand.cs";
    private const string RelayPath = "src/RackCad.Application/Workspace/WorkspaceCloseAdvisorRelay.cs";
    private const string LazyPath = "src/RackCad.Application/Workspace/WorkspaceLazyInstance.cs";

    private const string RackPanelSignature = @"public\s+void\s+RackPanel\s*\(\s*\)";
    private const string RackPanelDiscardSignature = @"public\s+void\s+RackPanelDiscard\s*\(\s*\)";
    private const string ConfirmSignature = @"private\s+static\s+bool\s+ConfirmOnCommandLine\s*\(\s*string\s+\w+\s*\)";

    private static readonly string[] ThePair = { "RACKPANEL", "RACKPANELDESCARTAR" };

    // ------------------------------------------------------------------ the pair of commands, exactly

    private static IEnumerable<string> PairViolations(string code)
    {
        var names = Regex.Matches(code, @"\[\s*CommandMethod\s*\(\s*""(?<name>[^""]*)""\s*\)\s*\]")
            .Select(m => m.Groups["name"].Value)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();
        if (!names.SequenceEqual(ThePair)) yield return "the commands are [" + string.Join(", ", names) + "]";
        if (Src.Count(code, @"\[\s*CommandMethod\b") != 2) yield return "the file does not hold exactly two attributes";
        if (Regex.IsMatch(code, @"CommandFlags|\[assembly|CommandClass|LispFunction|IExtensionApplication"))
            yield return "a flag, an alias or a load-time entry point";
    }

    [Fact]
    public void TheCommandFileDeclaresExactlyRackPanelAndRackPanelDiscardWithNoFlagsAndNoAlias()
    {
        Assert.Empty(PairViolations(Src.Code(CommandPath)));
    }

    [Theory]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {}")]
    [InlineData("[CommandMethod(\"RACKPANELDESCARTAR\")] void A() {}")]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {} [CommandMethod(\"RACKPANELDESCARTAR\")] void B() {} [CommandMethod(\"RPN\")] void C() {}")]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {} [CommandMethod(\"RACKPANELDESCARTAR\", CommandFlags.Modal)] void B() {}")]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {} [CommandMethod(\"RACKPANELDESCARTAR\")] [CommandMethod(\"RACKPANELDESCARTAR\")] void B() {}")]
    [InlineData("[CommandMethod(\"RACKPANEL\")] void A() {} [CommandMethod(\"RACKPANELDESCARTAR\")] void B() {} [assembly: CommandClass(typeof(C))]")]
    public void ThePairGuardFlagsAnythingButTheExactPair(string injected)
    {
        Assert.NotEmpty(PairViolations(injected));
    }

    [Fact]
    public void ThePairGuardAcceptsTheExactPairInEitherOrder()
    {
        Assert.Empty(PairViolations("[CommandMethod(\"RACKPANEL\")] void A() {} [CommandMethod(\"RACKPANELDESCARTAR\")] void B() {}"));
        Assert.Empty(PairViolations("[CommandMethod(\"RACKPANELDESCARTAR\")] void B() {} [CommandMethod(\"RACKPANEL\")] void A() {}"));
    }

    // ------------------------------------------------------------------ M-12: the one GetOrCreate of the file is inside RackPanel

    private static IEnumerable<string> GetOrCreateViolations(string code)
    {
        const string call = @"WorkspaceController\.GetOrCreate\s*\(";
        if (Src.Count(code, call) != 1) yield return "the file does not call WorkspaceController.GetOrCreate( exactly once";
        var rackPanel = Src.BodyOf(code, RackPanelSignature);
        if (Src.Count(rackPanel, call) != 1) yield return "the one call is not inside RackPanel";
    }

    [Fact]
    public void TheCommandFileCallsGetOrCreateExactlyOnceAndInsideRackPanel()
    {
        Assert.Empty(GetOrCreateViolations(Src.Code(CommandPath)));
    }

    [Theory]
    [InlineData("class C { public void RackPanel() { } public void RackPanelDiscard() { WorkspaceController.GetOrCreate(); } }")]
    [InlineData("class C { public void RackPanel() { WorkspaceController.GetOrCreate(); } public void RackPanelDiscard() { WorkspaceController.GetOrCreate(); } }")]
    [InlineData("class C { public void RackPanel() { WorkspaceController.GetOrCreate(); WorkspaceController.GetOrCreate(); } }")]
    [InlineData("class C { public void RackPanel() { } }")]
    public void TheGetOrCreateGuardFlagsAnExtraMissingOrMisplacedCall(string injected)
    {
        Assert.NotEmpty(GetOrCreateViolations(injected));
    }

    // ------------------------------------------------------------------ the body of RackPanelDiscard

    private static IEnumerable<string> DiscardBodyViolations(string code)
    {
        var body = Src.BodyOf(code, RackPanelDiscardSignature);
        if (!Regex.IsMatch(body, @"WorkspaceController\.Existing\b")) yield return "it does not take WorkspaceController.Existing";
        if (!Regex.IsMatch(body, @"WorkspaceDocumentDiscardCommand\.Run\s*\(")) yield return "it does not call the core of the command";
        if (!Regex.IsMatch(body, @"\.Authorize\s*\(")) yield return "it does not authorize the target against the document";
        if (!Regex.IsMatch(body, @"catch\s*\(\s*System\.Exception\s+ex\s*\)\s*\{\s*RackCommandSupport\.Report\s*\(\s*ex\s*\)\s*;\s*\}"))
            yield return "it does not report through the shared reporter";
        var forbidden = Regex.Match(body, @"\b(?:GetOrCreate|OpenPanel|Show|IsCreated)\b");
        if (forbidden.Success) yield return "it names " + forbidden.Value;
    }

    [Fact]
    public void RackPanelDiscardUsesExistingAndNeverCreatesNorOpensNorShowsAnything()
    {
        Assert.Empty(DiscardBodyViolations(Src.Code(CommandPath)));
    }

    [Theory]
    [InlineData("class C { public void RackPanelDiscard() { try { WorkspaceDocumentDiscardCommand.Run(null, null, s => c.Port.Authorize(s), null, null); } catch (System.Exception ex) { RackCommandSupport.Report(ex); } } }")]
    [InlineData("class C { public void RackPanelDiscard() { var c = WorkspaceController.Existing; WorkspaceController.GetOrCreate(); WorkspaceDocumentDiscardCommand.Run(null, null, s => c.Port.Authorize(s), null, null); try { } catch (System.Exception ex) { RackCommandSupport.Report(ex); } } }")]
    [InlineData("class C { public void RackPanelDiscard() { var c = WorkspaceController.Existing; c.OpenPanel(); WorkspaceDocumentDiscardCommand.Run(null, null, s => c.Port.Authorize(s), null, null); try { } catch (System.Exception ex) { RackCommandSupport.Report(ex); } } }")]
    [InlineData("class C { public void RackPanelDiscard() { var c = WorkspaceController.Existing; WorkspacePaletteHost.GetOrCreate().Show(); WorkspaceDocumentDiscardCommand.Run(null, null, s => c.Port.Authorize(s), null, null); try { } catch (System.Exception ex) { RackCommandSupport.Report(ex); } } }")]
    [InlineData("class C { public void RackPanelDiscard() { var c = WorkspaceController.Existing; WorkspaceDocumentDiscardCommand.Run(null, null, s => true, null, null); try { } catch (System.Exception ex) { RackCommandSupport.Report(ex); } } }")]
    [InlineData("class C { public void RackPanelDiscard() { var c = WorkspaceController.Existing; WorkspaceDocumentDiscardCommand.Run(null, null, s => c.Port.Authorize(s), null, null); } }")]
    public void TheDiscardBodyGuardFlagsEachWayOfCreatingOpeningShowingOrSkippingTheTarget(string injected)
    {
        Assert.NotEmpty(DiscardBodyViolations(injected));
    }

    // ------------------------------------------------------------------ GetKeywords only in ConfirmOnCommandLine, with the pattern of yes and no

    private static IEnumerable<string> KeywordsViolations(string code)
    {
        if (Src.Count(code, @"\bGetKeywords\b") != 1) yield return "GetKeywords is not named exactly once";
        var body = Src.BodyOf(code, ConfirmSignature);
        if (Src.Count(body, @"\.GetKeywords\s*\(") != 1) yield return "the one GetKeywords is not inside ConfirmOnCommandLine";
        foreach (var needle in new[]
                 {
                     @"Keywords\.Add\s*\(\s*""Si""\s*\)", @"Keywords\.Add\s*\(\s*""No""\s*\)",
                     @"Keywords\.Default\s*=\s*""No""\s*;", @"AllowNone\s*=\s*true\s*;",
                     @"\.Status\b", @"\.StringResult\b", @"StringComparison\.Ordinal\b", @"PromptStatus\.OK\b",
                 })
            if (!Regex.IsMatch(body, needle)) yield return "ConfirmOnCommandLine lacks " + needle;
        // The answer of GetKeywords is read with Status and StringResult only: nothing is named Result (a property of other types is banned).
        if (Regex.IsMatch(code, @"\bResult\b")) yield return "an identifier is named Result";
    }

    [Fact]
    public void TheCommandFileAsksYesOrNoOnlyInConfirmOnCommandLineWithNoAsTheDefault()
    {
        Assert.Empty(KeywordsViolations(Src.Code(CommandPath)));
    }

    [Theory]
    [InlineData("class C { void Other(Editor ed) { ed.GetKeywords(o); } private static bool ConfirmOnCommandLine(string q) { } }")]
    [InlineData("class C { private static bool ConfirmOnCommandLine(string q) { var o = new PromptKeywordOptions(q); o.Keywords.Add(\"Si\"); o.Keywords.Add(\"No\"); o.AllowNone = true; var a = ed.GetKeywords(o); return a.Status == PromptStatus.OK && string.Equals(a.StringResult, \"Si\", StringComparison.Ordinal); } }")]
    [InlineData("class C { private static bool ConfirmOnCommandLine(string q) { var o = new PromptKeywordOptions(q); o.Keywords.Add(\"Si\"); o.Keywords.Add(\"No\"); o.Keywords.Default = \"Si\"; o.AllowNone = true; var a = ed.GetKeywords(o); return a.Status == PromptStatus.OK && string.Equals(a.StringResult, \"Si\", StringComparison.Ordinal); } }")]
    [InlineData("class C { private static bool ConfirmOnCommandLine(string q) { var o = new PromptKeywordOptions(q); o.Keywords.Add(\"Si\"); o.Keywords.Add(\"No\"); o.Keywords.Default = \"No\"; o.AllowNone = true; var Result = ed.GetKeywords(o); return Result.Status == PromptStatus.OK && string.Equals(Result.StringResult, \"Si\", StringComparison.Ordinal); } }")]
    [InlineData("class C { private static bool ConfirmOnCommandLine(string q) { var o = new PromptKeywordOptions(q); o.Keywords.Add(\"Si\"); o.Keywords.Add(\"No\"); o.Keywords.Default = \"No\"; o.AllowNone = true; var a = ed.GetKeywords(o); return a.Status == PromptStatus.OK; } }")]
    public void TheKeywordsGuardFlagsAnotherPlaceAnotherDefaultNoAllowNoneOrAnIdentifierCalledResult(string injected)
    {
        Assert.NotEmpty(KeywordsViolations(injected));
    }

    // ------------------------------------------------------------------ the exception of GetKeywords: the command file, by exact path, and nothing else

    [Fact]
    public void TheCommandFileMayAskGetKeywordsAndEveryOtherNameStaysForbiddenThere()
    {
        var guard = WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(CommandPath);

        Assert.DoesNotMatch(guard, "var a = ed.GetKeywords(o);");
        Assert.Matches(guard, "var a = ed.GetString(o);");
        Assert.Matches(guard, "var a = ed.GetSelection();");
        Assert.Matches(guard, "System.Windows.MessageBox.Show(\"x\");");
        Assert.Matches(guard, "Application.ShowModalDialog(w);");
        Assert.Matches(guard, "w.ShowDialog();");
        Assert.Matches(guard, "var o = tr.GetObject(id, OpenMode.ForRead);");
        Assert.Matches(guard, "var r = ed.SelectImplied();");
    }

    [Theory]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceController.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspacePanelCommand2.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/Sub/WorkspacePanelCommand.cs")]
    [InlineData("src/RackCad.Plugin/Workspace/workspacepanelcommand.cs")]
    public void GetKeywordsStaysForbiddenInEveryOtherFileIncludingNamesThatLookLikeTheCommandFile(string path)
    {
        Assert.Matches(WorkspaceBridgeSourceGuardTests.HandlerForbiddenCallsFor(path), "var a = ed.GetKeywords(o);");
    }

    [Fact]
    public void NothingOutsideTheCommandFileOfThePluginNamesGetKeywords()
    {
        foreach (var (path, code) in Src.PluginFiles().Where(f => f.Path != CommandPath))
            Assert.DoesNotMatch(new Regex(@"\bGetKeywords\b"), code);
    }

    // ------------------------------------------------------------------ no window, no modal, no dialog, no focus, no async

    private static IEnumerable<string> BoundaryViolations(string code)
    {
        var found = Regex.Match(
            code,
            @"\bEnterPanelModal\b|\bMessageBox\b|\bShowDialog\b|\bShowModalDialog\b|\bWindow\b|\bDispatcher\b|\bTimer\b|\bTask\b|\basync\b|\bawait\b|\bFocus\b|\bActivate\s*\(|\bSetFocus\b|\bBringToFront\b");
        if (found.Success) yield return "names " + found.Value;
    }

    [Fact]
    public void TheCommandFileOpensNoSubdialogNoWindowAndTouchesNoFocus()
    {
        Assert.Empty(BoundaryViolations(Src.Code(CommandPath)));
    }

    [Theory]
    [InlineData("var w = new Window();")]
    [InlineData("using (controller.EnterPanelModal()) { }")]
    [InlineData("System.Windows.MessageBox.Show(\"x\");")]
    [InlineData("await Task.Delay(1);")]
    [InlineData("Dispatcher.CurrentDispatcher.Invoke(a);")]
    [InlineData("window.Focus();")]
    public void TheBoundaryGuardFlagsEachWayOfOpeningADialogOrTouchingTheFocus(string injected)
    {
        Assert.NotEmpty(BoundaryViolations(injected));
    }

    [Fact]
    public void TheControllerAddsNoAsyncNoTimerNoDispatcherAndNoMessageBox()
    {
        var code = Src.Code(ControllerPath);

        Assert.DoesNotMatch(new Regex(@"\bMessageBox\b|\bShowDialog\b|\bDispatcher\b|\bTimer\b|\bTask\b|\basync\b|\bawait\b|\bSetFocus\b|\bBringToFront\b"), code);
    }

    // ------------------------------------------------------------------ the controller: Existing, the relevo and the wrapper

    private static IEnumerable<string> ControllerViolations(string code)
    {
        if (!Regex.IsMatch(code, @"public\s+static\s+WorkspaceController\s+Existing\s*=>\s*Holder\.Existing\s*;"))
            yield return "Existing is not Holder.Existing";
        if (Regex.IsMatch(code, @"\bWriteMessage\b")) yield return "the controller names WriteMessage";

        var relay = Src.BodyOf(code, @"private\s+void\s+OnActiveSessionChanged\s*\(\s*\)");
        var write = Regex.Match(relay, @"\bWriteCloseMessages\s*\(\s*\)\s*;");
        var notify = Regex.Match(relay, @"\bNotifyStateChanged\s*\(\s*\)\s*;");
        if (!write.Success) yield return "the relevo does not write the pending messages";
        else if (!notify.Success || write.Index > notify.Index) yield return "the messages are not written before NotifyStateChanged()";
        if (Regex.IsMatch(relay, @"_bridge\.|RequestReconciliation|DrainNotice")) yield return "the relevo reaches the bridge";

        var writeBody = Src.BodyOf(code, @"private\s+void\s+WriteCloseMessages\s*\(\s*\)");
        if (!Regex.IsMatch(writeBody, @"_closeAdvisor\.TakeMessages\s*\(\s*_bridge\.ActiveSession\s*\)"))
            yield return "the messages are not taken with the active session of the bridge";
        if (!Regex.IsMatch(writeBody, @"WorkspacePanelCommand\.WriteToCommandLine\s*\(")) yield return "the messages are not written through the one writer";
        if (!Regex.IsMatch(writeBody, @"catch\s*\(\s*System\.Exception\s+\w+\s*\)\s*\{\s*LogFault\s*\(\s*""WorkspaceController\.WriteCloseMessages""\s*,"))
            yield return "a failure of the write is not logged with its context";

        var name = Src.BodyOf(code, @"private\s+string\s+DocumentNameOf\s*\(\s*SessionId\s+\w+\s*\)");
        foreach (var needle in new[] { @"_bridge\.TryGetSession\s*\(", @"\bIsDestroyed\b", @"AcApplication\.DocumentManager\b", @"UnmanagedObject\.ToInt64\s*\(\s*\)", @"\.Name\b" })
            if (!Regex.IsMatch(name, needle)) yield return "DocumentNameOf lacks " + needle;
        if (Regex.IsMatch(name, @"\b(?:StartTransaction|LockDocument|GetObject|Database)\b")) yield return "DocumentNameOf touches the drawing";
    }

    [Fact]
    public void TheControllerExposesExistingWritesThePendingMessagesInTheRelevoAndReadsTheNameFromMemoryOnly()
    {
        Assert.Empty(ControllerViolations(Src.Code(ControllerPath)));
    }

    private static string GoodController() =>
        "class W { public static WorkspaceController Existing => Holder.Existing; public static WorkspaceController GetOrCreate() => Holder.GetOrCreate(); "
        + "private void OnActiveSessionChanged() { try { } catch (System.Exception ex) { } WriteCloseMessages(); NotifyStateChanged(); } "
        + "private void WriteCloseMessages() { try { foreach (var text in _closeAdvisor.TakeMessages(_bridge.ActiveSession)) WorkspacePanelCommand.WriteToCommandLine(text); } catch (System.Exception ex) { LogFault(\"WorkspaceController.WriteCloseMessages\", ex); } } "
        + "private string DocumentNameOf(SessionId id) { if (!_bridge.TryGetSession(id, out var s) || s.IsDestroyed) return null; foreach (Document d in AcApplication.DocumentManager) if (d.UnmanagedObject.ToInt64() == s.NativeInstanceId) return d.Name; return null; } }";

    [Fact]
    public void TheControllerGuardAcceptsAControllerThatFollowsTheRulesAndFlagsEachBrokenRule()
    {
        var good = GoodController();
        Assert.Empty(ControllerViolations(good));

        Assert.NotEmpty(ControllerViolations(good.Replace("Existing => Holder.Existing", "Existing => null")));
        Assert.NotEmpty(ControllerViolations(good.Replace("WriteCloseMessages(); NotifyStateChanged();", "NotifyStateChanged(); WriteCloseMessages();")));
        Assert.NotEmpty(ControllerViolations(good.Replace("WriteCloseMessages(); NotifyStateChanged();", "NotifyStateChanged();")));
        Assert.NotEmpty(ControllerViolations(good.Replace("try { } catch (System.Exception ex) { } WriteCloseMessages();", "_bridge.ActiveSession.ToString(); WriteCloseMessages();")));
        Assert.NotEmpty(ControllerViolations(good.Replace("TakeMessages(_bridge.ActiveSession)", "TakeMessages(null)")));
        Assert.NotEmpty(ControllerViolations(good.Replace("WorkspacePanelCommand.WriteToCommandLine(text)", "ed.WriteMessage(text)")));
        Assert.NotEmpty(ControllerViolations(good.Replace("LogFault(\"WorkspaceController.WriteCloseMessages\", ex);", string.Empty)));
        Assert.NotEmpty(ControllerViolations(good.Replace("AcApplication.DocumentManager", "db.TransactionManager")));
        Assert.NotEmpty(ControllerViolations(good + " class X { void M() { ed.WriteMessage(\"x\"); } }"));
        Assert.NotEmpty(ControllerViolations(good.Replace("return d.Name;", "{ using (var tr = db.TransactionManager.StartTransaction()) { } return d.Name; }")));
    }

    // ------------------------------------------------------------------ the 10 Spanish reasons of the Plugin (T-3), only inside the three bodies

    private static readonly (string English, string Spanish, string Body)[] ReasonTable =
    {
        ("There is no active document.", "No hay documento activo.", "ReadOnActiveDocument"),
        ("A command is in progress.", "Hay un comando en curso.", "ReadOnActiveDocument"),
        ("The request does not belong to the active document.", "La petición no es del documento activo.", "ReadOnActiveDocument"),
        ("The catalog is not available.", "El catálogo no está disponible.", "LoadCatalog"),
        ("The catalog could not be loaded: ", "No se pudo cargar el catálogo: ", "LoadCatalog"),
        ("The preview witness has no key.", "El testigo de la vista previa no tiene clave.", "Read"),
        ("The scan of the drawing carries no raw envelopes.", "La exploración del dibujo no trajo las envolventes en bruto.", "Read"),
        ("The document is locked by another operation.", "El documento está bloqueado por otra operación.", "Read"),
        ("The drawing could not be read: ", "No se pudo leer el dibujo: ", "Read"),
        ("The edit intention could not be read: ", "No se pudo leer la intención de edición: ", "Read"),
    };

    private static IEnumerable<string> ReasonViolations(string bodyName, string body)
    {
        foreach (var (english, spanish, owner) in ReasonTable.Where(r => r.Body == bodyName))
        {
            if (body.Contains("\"" + english + "\"", StringComparison.Ordinal)) yield return bodyName + " still says in English: " + english;
            if (!body.Contains("\"" + spanish + "\"", StringComparison.Ordinal)) yield return bodyName + " lacks: " + spanish;
            if (owner != bodyName) yield return "the table is inconsistent";
        }
    }

    private static string HostBody() =>
        Src.BodyOf(Src.Code(NavigationHostPath), @"private\s+EditIntentReadResult\s+ReadOnActiveDocument\s*\(");

    private static string CatalogBody() =>
        Src.BodyOf(Src.Code(IntentAdapterPath), @"internal\s+static\s+RackPilotCatalogResult\s+LoadCatalog\s*\(\s*\)");

    private static string ReadBody() =>
        Src.BodyOf(Src.Code(IntentAdapterPath), @"internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(");

    [Fact]
    public void TheTenReasonsOfThePluginAreInSpanishInTheirThreeBodiesAndOnlyThere()
    {
        Assert.Equal(10, ReasonTable.Length);
        Assert.Equal(3, ReasonTable.Count(r => r.Body == "ReadOnActiveDocument"));
        Assert.Equal(2, ReasonTable.Count(r => r.Body == "LoadCatalog"));
        Assert.Equal(5, ReasonTable.Count(r => r.Body == "Read"));

        Assert.Empty(ReasonViolations("ReadOnActiveDocument", HostBody()));
        Assert.Empty(ReasonViolations("LoadCatalog", CatalogBody()));
        Assert.Empty(ReasonViolations("Read", ReadBody()));
    }

    [Fact]
    public void TheReasonsThatCarryTheMessageOfTheExceptionStillCarryIt()
    {
        var catalog = CatalogBody();
        var read = ReadBody();

        Assert.Matches(@"""No se pudo cargar el catálogo: ""\s*\+\s*ex\.Message", catalog);
        Assert.Matches(@"""No se pudo leer el dibujo: ""\s*\+\s*ex\.Message", read);
        Assert.Matches(@"""No se pudo leer la intención de edición: ""\s*\+\s*ex\.Message", read);
        Assert.Equal(1, Src.Count(catalog, @"ex\.Message"));
        Assert.Equal(2, Src.Count(read, @"ex\.Message"));
    }

    [Fact]
    public void TheCatalogBodyDoesNotNameTheDocumentTypeAndNoSpanishReasonUsesAWordThatTheNeighbouringGuardsForbid()
    {
        // The body of LoadCatalog may not name the type Document (an older guard), nor may any new text use the words of the other guards.
        Assert.DoesNotMatch(new Regex(@"\bDocument\b"), CatalogBody());
        foreach (var row in ReasonTable)
            Assert.DoesNotMatch(new Regex(@"\bDocument\b|\bLock|Metric|Provider|\bBom|Aggregat|Transaction|\bDatabase\b"), row.Spanish);
    }

    [Fact]
    public void TheReasonGuardFlagsAnEnglishReasonAndAMissingSpanishOne()
    {
        var good = "{ return X(\"No hay documento activo.\"); return Y(\"Hay un comando en curso.\"); return Z(\"La petición no es del documento activo.\"); }";
        var english = good.Replace("\"No hay documento activo.\"", "\"There is no active document.\"");
        var missing = good.Replace("\"Hay un comando en curso.\"", "\"otra cosa\"");

        Assert.Empty(ReasonViolations("ReadOnActiveDocument", good));
        Assert.NotEmpty(ReasonViolations("ReadOnActiveDocument", english));
        Assert.NotEmpty(ReasonViolations("ReadOnActiveDocument", missing));
        Assert.NotEmpty(ReasonViolations("ReadOnActiveDocument", good + " return W(\"A command is in progress.\");"));
    }

    // ------------------------------------------------------------------ the advisor interface: only its documentation changes

    private static IEnumerable<string> AdvisorViolations(string code)
    {
        if (Src.Count(code, @"\bpublic\s+interface\s+IWorkspaceLifecycleAdvisor\b") != 1) yield return "the interface is not declared once";
        if (Src.Count(code, @"\bCloseDecision\s+AdviseClose\s*\(\s*SessionId\s+\w+\s*\)\s*;") != 1) yield return "AdviseClose is not the member of the package";
        if (Src.Count(code, @"\bvoid\s+SessionDestroyed\s*\(\s*SessionId\s+\w+\s*,\s*bool\s+closeSequenceIntact\s*\)\s*;") != 1)
            yield return "SessionDestroyed is not the member of the package";
        var body = Src.BodyOf(code, @"\bpublic\s+interface\s+IWorkspaceLifecycleAdvisor\b");
        if (Src.Count(body, @";") != 2) yield return "the body of the interface does not hold exactly two statements";
    }

    [Fact]
    public void TheAdvisorInterfaceKeepsExactlyItsTwoMembersWithOnlyItsDocumentationChanged()
    {
        Assert.Empty(AdvisorViolations(Src.Code(AdvisorPath)));
    }

    [Theory]
    [InlineData("public interface IWorkspaceLifecycleAdvisor { CloseDecision AdviseClose(SessionId s); }")]
    [InlineData("public interface IWorkspaceLifecycleAdvisor { CloseDecision AdviseClose(SessionId s); void SessionDestroyed(SessionId s, bool closeSequenceIntact); string Name { get; } }")]
    [InlineData("public interface IWorkspaceLifecycleAdvisor { CloseDecision AdviseClose(SessionId s); void SessionDestroyed(SessionId s, bool closeSequenceIntact); void Other(); }")]
    [InlineData("public interface IWorkspaceLifecycleAdvisor { CloseDecision AdviseClose(SessionId s, bool force); void SessionDestroyed(SessionId s, bool closeSequenceIntact); }")]
    public void TheAdvisorGuardFlagsAThirdMemberAMissingOneOrAChangedSignature(string injected)
    {
        Assert.NotEmpty(AdvisorViolations(injected));
    }

    // ------------------------------------------------------------------ the two new files of Application: only the allowed operations of the port

    private static IEnumerable<string> PortCallViolations(string code, string receiver, params string[] allowed)
    {
        var calls = Regex.Matches(code, @"\b" + receiver + @"\.(?<name>\w+)\s*\(")
            .Select(m => m.Groups["name"].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();
        if (!calls.SequenceEqual(allowed.OrderBy(n => n, StringComparer.Ordinal))) yield return "calls [" + string.Join(", ", calls) + "]";
        var forbidden = Regex.Match(
            code, @"\b(?:DiscardRack|GetField|SetPendingText|CommitField|CrossBoundary|TryBeginEdit|TryReload|TryCheckNow|TakeLossNotices|TakeCloseNotices)\b");
        if (forbidden.Success) yield return "names " + forbidden.Value;
    }

    [Fact]
    public void TheCoreOfTheCommandCallsOnlyTheRisksAndTheDiscardOfTheDocumentAndTheWrapperOnlyTheRisks()
    {
        Assert.Empty(PortCallViolations(Src.Code(DiscardCorePath), "port", "GetRacksAtRisk", "DiscardDocument"));
        Assert.Empty(PortCallViolations(Src.Code(RelayPath), "_risks", "GetRacksAtRisk"));
    }

    [Theory]
    [InlineData("port.GetRacksAtRisk(r); port.DiscardDocument(r); port.DiscardRack(r, x);")]
    [InlineData("port.GetRacksAtRisk(r); port.DiscardDocument(r); port.GetField(r, x);")]
    [InlineData("port.GetRacksAtRisk(r);")]
    [InlineData("port.GetRacksAtRisk(r); port.DiscardDocument(r); port.TakeCloseNotices();")]
    [InlineData("port.GetRacksAtRisk(r); port.DiscardDocument(r); port.CommitField(r, x);")]
    public void ThePortGuardFlagsAnotherOperationAndAMissingOne(string injected)
    {
        Assert.NotEmpty(PortCallViolations(injected, "port", "GetRacksAtRisk", "DiscardDocument"));
    }

    [Fact]
    public void TheTwoNewFilesOfApplicationAreNullableEnabledPublicAndOfTheWorkspaceNamespace()
    {
        foreach (var path in new[] { DiscardCorePath, RelayPath })
        {
            var text = Src.Text(path);
            var code = Src.Code(path);

            Assert.StartsWith("#nullable enable", text.TrimStart((char)0xFEFF), StringComparison.Ordinal);
            Assert.Equal(1, Src.Count(code, @"\bnamespace\s+RackCad\.Application\.Workspace\s*\{"));
            Assert.DoesNotMatch(new Regex(@"\bAutodesk\.|System\.IO|\bRackLog\b|System\.Windows|RackCad\.Plugin|RackCad\.UI"), code);
        }

        Assert.Matches(@"\bpublic\s+static\s+class\s+WorkspaceDocumentDiscardCommand\b", Src.Code(DiscardCorePath));
        Assert.Matches(@"\bpublic\s+sealed\s+class\s+WorkspaceCloseAdvisorRelay\s*:\s*IWorkspaceLifecycleAdvisor\b", Src.Code(RelayPath));
    }

    [Fact]
    public void TheRelayStartsFromVetoNeverFromTheDefaultAndNeverCallsTheBridgeNorHoldsAnyStateOfDrafts()
    {
        var code = Src.Code(RelayPath);

        var advise = Src.BodyOf(code, @"public\s+CloseDecision\s+AdviseClose\s*\(\s*SessionId\s+\w+\s*\)");
        Assert.Matches(@"var\s+decision\s*=\s*CloseDecision\.Veto\s*;", advise);
        Assert.DoesNotMatch(new Regex(@"CloseDecision\s+decision\s*;|default\s*\(\s*CloseDecision\s*\)"), advise);

        var destroyed = Src.BodyOf(code, @"public\s+void\s+SessionDestroyed\s*\(\s*SessionId\s+\w+\s*,\s*bool\s+closeSequenceIntact\s*\)");
        Assert.Equal(1, Src.Count(destroyed, @"_inner\.SessionDestroyed\s*\(\s*\w+\s*,\s*closeSequenceIntact\s*\)"));
        Assert.Equal(1, Src.Count(code, @"_inner\.SessionDestroyed\s*\("));
        Assert.Equal(1, Src.Count(code, @"_inner\.AdviseClose\s*\("));
        Assert.DoesNotMatch(new Regex(@"_documentNameOf|_isActive|_writeNow|\btry\b|\bcatch\b"), destroyed);
        Assert.DoesNotMatch(new Regex(@"\b[Bb]ridge\b|\bDraft|DraftRegistry|OpenDrafts"), code);
    }

    // ------------------------------------------------------------------ the lazy instance

    [Fact]
    public void ExistingOfTheLazyInstanceIsAReadUnderTheSameLockThatNeverCallsTheFactory()
    {
        var code = Src.Code(LazyPath);
        var body = Src.BodyOf(code, @"public\s+T\?\s+Existing\b");

        Assert.Matches(@"lock\s*\(\s*_gate\s*\)\s*return\s+_instance\s*;", body);
        Assert.DoesNotMatch(new Regex(@"_factory|_createCount"), body);
        Assert.Equal(1, Src.Count(code, @"\b_factory\s*\("));
    }
}
