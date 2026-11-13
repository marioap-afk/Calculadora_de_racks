#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T3B2-EDIT-CLOSE-SYNC (INV-F4T3B2-03, 07, 09, 12, 15; D-T3B2-07; section 4 of the package): source guards of the close, the discard,
// the notices and the retention of the panel. The Core test project cannot reference the Plugin nor load WPF, so those files are read as text.
// The guards look for CALLS (a dot, the name and a parenthesis, or an unqualified call that is not the declaration) and never for a substring:
// CanDiscardDocument, BrowserDiscardDocumentButton and the handlers contain the text DiscardDocument, and the declaration of TakeNotices
// contains TakeNotices(. They are written by the body of a member, and each one has a self-test that mutates a synthetic source.
public class WorkspacePilotCloseSyncGuardTests
{
    private const string UiFolder = "src/RackCad.UI/";
    private const string UiWorkspace = "src/RackCad.UI/Workspace/";
    private const string BrowserModelPath = UiWorkspace + "WorkspaceBrowserViewModel.cs";
    private const string XamlPath = UiWorkspace + "WorkspacePanelView.xaml";
    private const string CodeBehindPath = UiWorkspace + "WorkspacePanelView.xaml.cs";
    private const string SessionPath = "src/RackCad.Application/Workspace/BrowserSession.cs";

    // ------------------------------------------------------------------ helpers: calls and bodies

    private static readonly HashSet<string> NotATypeBeforeAName = new(StringComparer.Ordinal)
    {
        "return", "new", "await", "throw", "else", "in", "case", "is", "not", "and", "or", "do", "yield",
    };

    // The calls of a member name in a text of code: qualified (.Name() or unqualified and not the declaration of the member.
    private static int CallCount(string code, string name)
    {
        var qualified = Regex.Matches(code, @"\.\s*" + Regex.Escape(name) + @"\s*\(").Count;
        var unqualified = 0;
        foreach (Match m in Regex.Matches(code, @"(?<![\w.])" + Regex.Escape(name) + @"\s*\("))
            if (!IsDeclaration(code, m.Index)) unqualified++;
        return qualified + unqualified;
    }

    // A declaration has a type (a word) right before the name; a call has an operator, a bracket, a semicolon or a statement keyword.
    private static bool IsDeclaration(string code, int index)
    {
        var before = code.Substring(0, index).TrimEnd();
        var match = Regex.Match(before, @"([\w>\]\?]+)$");
        return match.Success && !NotATypeBeforeAName.Contains(match.Groups[1].Value);
    }

    private static string? BodyOfMember(string code, string name)
    {
        var match = Regex.Match(code, @"\b(?:void|bool|string)\s+" + Regex.Escape(name) + @"\s*\(");
        return match.Success ? Src.BodyOf(code, @"\b(?:void|bool|string)\s+" + Regex.Escape(name) + @"\s*\(") : null;
    }

    // The body of a member that the guard needs: its absence is an assertion, never an exception.
    private static string RequiredBody(string code, string name)
    {
        var body = BodyOfMember(code, name);
        Assert.True(body != null, "the member " + name + " does not exist");
        return body!;
    }

    // The calls of one name in the files, outside the bodies of the allowed members of the browser view-model; the other files allow none.
    private static IEnumerable<string> CallsOutside(IEnumerable<(string Path, string Code)> files, string name, params string[] allowedMembers)
    {
        foreach (var (path, code) in files)
        {
            var total = CallCount(code, name);
            if (total == 0) continue;
            if (!path.EndsWith("WorkspaceBrowserViewModel.cs", StringComparison.Ordinal))
            {
                yield return path + " calls " + name;
                continue;
            }

            var inside = 0;
            foreach (var member in allowedMembers)
            {
                var body = BodyOfMember(code, member);
                if (body != null) inside += CallCount(body, name);
            }

            if (inside != total) yield return path + " calls " + name + " outside " + string.Join(", ", allowedMembers);
        }
    }

    private static IReadOnlyList<(string Path, string Code)> Ui() => Src.Files(UiFolder);

    private static string Browser() => Src.Code(BrowserModelPath);

    // ------------------------------------------------------------------ the discards and the risks: only the gestures that own them

    [Fact]
    public void DiscardRackIsCalledOnlyInCloseTabDiscardFieldAndConfirmDiscardOfTheBrowserViewModel()
    {
        Assert.Empty(CallsOutside(Ui(), "DiscardRack", "CloseTab", "DiscardField", "ConfirmDiscard"));

        var code = Browser();
        foreach (var member in new[] { "CloseTab", "DiscardField", "ConfirmDiscard" })
            Assert.True(CallCount(RequiredBody(code, member), "DiscardRack") >= 1, member + " does not discard the rack");
    }

    [Fact]
    public void DiscardDocumentIsCalledOnlyInConfirmDiscardAndTheRisksOnlyInRequestDocumentDiscardAndConfirmDiscard()
    {
        Assert.Empty(CallsOutside(Ui(), "DiscardDocument", "ConfirmDiscard"));
        Assert.Empty(CallsOutside(Ui(), "GetRacksAtRisk", "RequestDocumentDiscard", "ConfirmDiscard"));

        var code = Browser();
        Assert.Equal(1, CallCount(RequiredBody(code, "ConfirmDiscard"), "DiscardDocument"));
        Assert.Equal(1, CallCount(RequiredBody(code, "RequestDocumentDiscard"), "GetRacksAtRisk"));
        Assert.Equal(1, CallCount(RequiredBody(code, "ConfirmDiscard"), "GetRacksAtRisk"));
    }

    [Fact]
    public void TheGuardOfTheDiscardsSeesACallOutsideItsMembersAndIgnoresTheNamesThatContainThem()
    {
        const string good = "class C { public void CloseTab(string r) { _p.DiscardRack(o, r); } public void DiscardField() { _p.DiscardRack(o, r); } "
            + "public void ConfirmDiscard() { _p.DiscardRack(o, r); } public bool CanDiscardDocument => true; private void OnBrowserDiscardDocumentClick() { RequestDocumentDiscard(); } }";
        const string elsewhere = "class C { public void CloseTab(string r) { _p.DiscardRack(o, r); } public void Present() { _p.DiscardRack(o, r); } }";
        const string unqualified = "class C { public void Present() { DiscardRack(o, r); } }";

        Assert.Empty(CallsOutside(new[] { (BrowserModelPath, good) }, "DiscardRack", "CloseTab", "DiscardField", "ConfirmDiscard"));
        Assert.Empty(CallsOutside(new[] { (BrowserModelPath, good) }, "DiscardDocument", "ConfirmDiscard"));
        Assert.NotEmpty(CallsOutside(new[] { (BrowserModelPath, elsewhere) }, "DiscardRack", "CloseTab", "DiscardField", "ConfirmDiscard"));
        Assert.NotEmpty(CallsOutside(new[] { (BrowserModelPath, unqualified) }, "DiscardRack", "CloseTab", "DiscardField", "ConfirmDiscard"));
        Assert.NotEmpty(CallsOutside(new[] { (UiWorkspace + "WorkspacePanelViewModel.cs", good) }, "DiscardRack", "CloseTab", "DiscardField", "ConfirmDiscard"));
        Assert.NotEmpty(CallsOutside(new[] { (BrowserModelPath, "class C { public void DiscardField() { _p.DiscardDocument(o); } }") }, "DiscardDocument", "ConfirmDiscard"));
        Assert.NotEmpty(CallsOutside(new[] { (BrowserModelPath, "class C { public void CloseTab(string r) { _p.GetRacksAtRisk(o); } }") }, "GetRacksAtRisk", "RequestDocumentDiscard", "ConfirmDiscard"));
    }

    // ------------------------------------------------------------------ the queues of notices: only TakeNotices, only from Present

    [Fact]
    public void TheQueuesOfNoticesAreTakenOnlyInTakeNoticesAndTakeNoticesIsCalledOnlyInPresent()
    {
        Assert.Empty(CallsOutside(Ui(), "TakeCloseNotices", "TakeNotices"));
        Assert.Empty(CallsOutside(Ui(), "TakeLossNotices", "TakeNotices"));

        var code = Browser();
        var take = RequiredBody(code, "TakeNotices");
        Assert.Equal(1, CallCount(take, "TakeCloseNotices"));
        Assert.Equal(1, CallCount(take, "TakeLossNotices"));
        // Each queue in its own try, so a failure of one does not stop the presentation nor the other queue.
        Assert.Equal(2, Src.Count(take, @"\btry\b"));

        Assert.Equal(1, CallCount(code, "TakeNotices"));
        Assert.Equal(1, CallCount(RequiredBody(code, "Present"), "TakeNotices"));
        Assert.Matches(@"private\s+void\s+TakeNotices\s*\(\s*\)", code);
        Assert.Empty(CallsOutside(Ui().Where(f => !f.Path.EndsWith("WorkspaceBrowserViewModel.cs", StringComparison.Ordinal)), "TakeNotices"));
    }

    [Fact]
    public void TheQueuesOfNoticesAreNamedInTheHostSideOnlyByThePortTheHostAndTheTwoForwarders()
    {
        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs",
            "src/RackCad.Application/Workspace/WorkspaceHostCore.cs",
            "src/RackCad.Application/Workspace/WorkspacePilotSupport.cs",
            "src/RackCad.Plugin/Workspace/WorkspaceController.cs",
        };

        Assert.Empty(NoticeQueueViolations(Src.Files("src/RackCad.Plugin/").Concat(Src.Files("src/RackCad.Application/")), allowed));
        // The allowed files really name them, so the guard is not vacuous.
        foreach (var path in allowed)
            Assert.Matches(@"\bTake(?:Close|Loss)Notices\b", Src.Code(path));
    }

    private static IEnumerable<string> NoticeQueueViolations(IEnumerable<(string Path, string Code)> files, HashSet<string> allowed)
    {
        foreach (var (path, code) in files)
            if (!allowed.Contains(path) && Regex.IsMatch(code, @"\bTake(?:Close|Loss)Notices\b"))
                yield return path + " names a queue of notices";
    }

    [Fact]
    public void TheGuardOfTheQueuesSeesAnotherFileAndAnotherMember()
    {
        var allowed = new HashSet<string>(StringComparer.Ordinal) { "a.cs" };

        Assert.Empty(NoticeQueueViolations(new[] { ("a.cs", "x.TakeLossNotices();") }, allowed));
        Assert.NotEmpty(NoticeQueueViolations(new[] { ("b.cs", "x.TakeCloseNotices();") }, allowed));
        Assert.NotEmpty(CallsOutside(new[] { (BrowserModelPath, "class C { public void ConfirmDiscard() { _p.TakeLossNotices(); } private void TakeNotices() { _p.TakeLossNotices(); } }") }, "TakeLossNotices", "TakeNotices"));
        Assert.Empty(CallsOutside(new[] { (BrowserModelPath, "class C { private void TakeNotices() { try { _p.TakeLossNotices(); } catch { } } }") }, "TakeLossNotices", "TakeNotices"));
        // The declaration of TakeNotices is not a call of it; a call from another body is.
        Assert.Equal(0, CallCount("class C { private void TakeNotices() { } }", "TakeNotices"));
        Assert.Equal(1, CallCount("class C { public void Present() { TakeNotices(); } private void TakeNotices() { } }", "TakeNotices"));
        Assert.Equal(1, CallCount("class C { public void CloseTab() { this.TakeNotices(); } }", "TakeNotices"));
        Assert.Equal(1, CallCount("class C { public void CloseTab() { return TakeNotices(); } }", "TakeNotices"));
    }

    // ------------------------------------------------------------------ the retention: the view-model passes keepActive; the session has the exact signature

    [Fact]
    public void TheBrowserViewModelPassesKeepActiveToApplySelectionContext()
    {
        var code = Browser();

        Assert.Matches(@"\.ApplySelectionContext\s*\(\s*\w+\s*,\s*keepActive\s*\)", code);
        Assert.Equal(1, CallCount(code, "ApplySelectionContext"));
    }

    [Fact]
    public void ApplySelectionContextHasTheExactSignatureWithTheOneOptionalParameterAndNoOverload()
    {
        var code = Src.Code(SessionPath);

        Assert.Matches(@"public\s+BrowserChange\s+ApplySelectionContext\s*\(\s*SelectionContext\s+context\s*,\s*bool\s+keepActive\s*=\s*false\s*\)", code);
        Assert.Equal(1, Src.Count(code, @"\bApplySelectionContext\s*\("));
        Assert.DoesNotMatch(new Regex(@"\bApplySelectionContext\s*\(\s*SelectionContext\s+context\s*\)"), code);
    }

    [Fact]
    public void BrowserSessionKeepsTheVocabularyOfItsGuardsAndAddsNoTypeNorValue()
    {
        var code = Src.Code(SessionPath);

        // The words that the guards of the session forbid in its code; the text of this task uses neutral words only.
        Assert.DoesNotMatch(new Regex(@"Draft|\bDirty\b|\bEditor\b|Subscription|Commit\(|Transaction|Provider|Metric", RegexOptions.IgnoreCase), code);        Assert.DoesNotMatch(new Regex(@"\bstatic\b|\block\b|\bAction\b|\bFunc<|\bDateTime\b"), code);
        Assert.Equal(new[] { "None", "Activated", "Created", "Replaced", "Pinned", "Closed", "SummaryApplied" }, Enum.GetNames<RackCad.Application.Workspace.BrowserChange>());
        // No type is added: the file declares the same four types as before (two enums, one record and the session).
        Assert.Equal(4, Src.Count(code, @"\b(?:enum|record|class)\s+\w+"));
    }

    // ------------------------------------------------------------------ the shape of the UI: the view-model and the record

    [Fact]
    public void TheBrowserViewModelGainsExactlyThePublicMembersOfTheContractAndTheNoticeRecord()
    {
        var code = Browser();

        Assert.Matches(@"public\s+sealed\s+record\s+WorkspaceBrowserNoticeItem\s*\(\s*string\s+Key\s*,\s*string\s+Text\s*,\s*bool\s+CanDiscardDocument\s*\)\s*;", code);
        foreach (var signature in new[]
                 {
                     @"public\s+void\s+CloseTab\s*\(\s*string\s+rackId\s*\)",
                     @"public\s+void\s+DiscardField\s*\(\s*\)",
                     @"public\s+void\s+RequestDocumentDiscard\s*\(\s*\)",
                     @"public\s+void\s+ConfirmDiscard\s*\(\s*\)",
                     @"public\s+void\s+CancelDiscard\s*\(\s*\)",
                     @"public\s+void\s+DismissNotice\s*\(\s*string\s+key\s*\)",
                     @"public\s+bool\s+CanDiscardDocument\s*=>",
                     @"public\s+bool\s+HasConfirmation\s*=>",
                     @"public\s+string\s+ConfirmationText\s*=>",
                     @"public\s+string\s+NoticeText\s*=>",
                     @"public\s+IReadOnlyList<WorkspaceBrowserNoticeItem>\s+Notices\s*=>",
                     @"public\s+bool\s+HasNotices\s*=>",
                 })
            Assert.Equal(1, Src.Count(code, signature));

        // The record of the tab is the one of F3 and the field gains only the discard button.
        Assert.Matches(@"public\s+sealed\s+record\s+WorkspaceBrowserTabItem\s*\(\s*string\s+RackId\s*,\s*string\s+Title\s*,\s*bool\s+IsActive\s*,\s*bool\s+IsPreview\s*\)\s*;", code);
        Assert.Equal(1, Src.Count(Src.Code(UiWorkspace + "WorkspacePilotFieldViewModel.cs"), @"public\s+bool\s+ShowDiscardButton\s*=>"));
    }

    [Fact]
    public void TheBrowserAndTheFieldViewModelsAddNoSubscriptionNoEventAndNoStaticState()
    {
        foreach (var code in new[] { Browser(), Src.Code(UiWorkspace + "WorkspacePilotFieldViewModel.cs") })
        {
            // Texts are composed with a = a + b: no += and no -= anywhere, and nothing that schedules.
            Assert.DoesNotMatch(new Regex(@"\+=|-=|\bevent\b|\bdelegate\b|\bstatic\s+(?!readonly\s+string\b)\w+\s+\w+\s*;"), code);
            Assert.DoesNotMatch(new Regex(@"\bTask\b|\basync\b|\bawait\b|\bDispatcher\b|\w*Timer\b|\bThread\b|\.Result\b|\.Wait\s*\("), code);
        }

        // The browser still asks the port for the liveness of a session and for a summary exactly once, and holds one registry.
        var browser = Browser();
        Assert.Equal(1, Src.Count(browser, @"\.IsSessionAlive\s*\("));
        Assert.Equal(1, Src.Count(browser, @"\.ReadSummary\s*\("));
        Assert.Equal(1, Src.Count(browser, @"\bBrowserSessionRegistry\b"));
        // The purge of the notices of veto is in the same loop of PurgeDeadSessions.
        var purge = RequiredBody(browser, "PurgeDeadSessions");
        Assert.Equal(1, Src.Count(purge, @"\.IsSessionAlive\s*\("));
        Assert.Matches(@"RemoveAll", purge);
    }

    [Fact]
    public void TheGuardOfTheSubscriptionsSeesTheComposedTextWithPlusEquals()
    {
        var forbidden = new Regex(@"\+=|-=|\bevent\b|\bdelegate\b");

        Assert.Matches(forbidden, "var a = x; a += y;");
        Assert.Matches(forbidden, "text -= y;");
        Assert.DoesNotMatch(forbidden, "var a = x; a = a + y;");
    }

    [Fact]
    public void ThePanelViewModelOnlyChangesTheDocumentationOfPilotEdit()
    {
        var code = Src.Code(UiWorkspace + "WorkspacePanelViewModel.cs");

        Assert.Matches(
            @"public\s+WorkspacePanelViewModel\s*\(\s*IWorkspaceHostPort\s+\w+\s*,\s*Action<bool>\s+\w+\s*,\s*IWorkspaceNavigationPort\s+\w+\s*=\s*null\s*,\s*IWorkspaceBrowserSummaryPort\s+browserSummary\s*=\s*null\s*,\s*IWorkspacePilotEditPort\s+pilotEdit\s*=\s*null\s*\)",
            code);
        Assert.Matches(@"new\s+WorkspaceBrowserViewModel\s*\(\s*browserSummary\s*,\s*RepresentNavigation\s*,\s*pilotEdit\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"\.TakeCloseNotices|\.TakeLossNotices|\.DiscardRack|\.DiscardDocument|\.GetRacksAtRisk"), code);
        Assert.DoesNotContain("Building the panel never calls it", Src.Text(UiWorkspace + "WorkspacePanelViewModel.cs"));
    }

    // ------------------------------------------------------------------ the XAML: names, placement, no default or cancel button, no fixed size

    private static string Xaml() => Regex.Replace(Src.Text(XamlPath), @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

    private static string BrowserSection(string xaml)
    {
        var start = xaml.IndexOf("Text=\"Navegador de racks\"", StringComparison.Ordinal);
        var navigation = xaml.IndexOf("Text=\"Navegación\"", StringComparison.Ordinal);
        Assert.True(start >= 0 && navigation > start, "the browser section is not where it should be");
        return xaml.Substring(start, navigation - start);
    }

    // The element opening tag that carries the name, from its '<' to its '>'.
    private static string TagOf(string xaml, string name)
    {
        var at = xaml.IndexOf("x:Name=\"" + name + "\"", StringComparison.Ordinal);
        Assert.True(at >= 0, "the XAML has no element " + name);
        var open = xaml.LastIndexOf('<', at);
        var close = xaml.IndexOf('>', at);
        return xaml.Substring(open, close - open + 1);
    }

    [Fact]
    public void TheXamlHasTheNewElementsWithTheirNamesAutomationNamesAndHandlersBetweenTheStripAndTheSurface()
    {
        var xaml = Xaml();
        var section = BrowserSection(xaml);
        var strip = section.IndexOf("x:Name=\"BrowserTabStrip\"", StringComparison.Ordinal);
        var stripEnd = section.IndexOf("</ItemsControl>", strip, StringComparison.Ordinal);
        var surface = section.IndexOf("x:Name=\"BrowserSurface\"", StringComparison.Ordinal);
        Assert.True(strip >= 0 && stripEnd > strip && surface > stripEnd);

        foreach (var (name, handler, automation) in new[]
                 {
                     ("BrowserDiscardDocumentButton", "OnBrowserDiscardDocumentClick", "Descartar borradores de este documento"),
                     ("BrowserConfirmDiscardButton", "OnBrowserConfirmDiscardClick", "Confirmar el descarte"),
                     ("BrowserCancelDiscardButton", "OnBrowserCancelDiscardClick", "Cancelar el descarte"),
                 })
        {
            var at = section.IndexOf("x:Name=\"" + name + "\"", StringComparison.Ordinal);
            Assert.True(at > stripEnd && at < surface, name + " is not between the strip and the surface");
            var tag = TagOf(section, name);
            Assert.Contains("Click=\"" + handler + "\"", tag);
            Assert.Contains("AutomationProperties.Name=\"" + automation + "\"", tag);
        }

        foreach (var name in new[] { "BrowserConfirmationPanel", "BrowserConfirmationText", "BrowserNoticeText", "BrowserNoticeList" })
        {
            var at = section.IndexOf("x:Name=\"" + name + "\"", StringComparison.Ordinal);
            Assert.True(at > stripEnd && at < surface, name + " is not between the strip and the surface");
        }

        Assert.Contains("Content=\"Descartar\"", TagOf(section, "BrowserConfirmDiscardButton"));
        Assert.Contains("Content=\"Cancelar\"", TagOf(section, "BrowserCancelDiscardButton"));
        Assert.Contains("{Binding Browser.CanDiscardDocument", TagOf(section, "BrowserDiscardDocumentButton"));
        Assert.Contains("{Binding Browser.HasConfirmation", TagOf(section, "BrowserConfirmationPanel"));
        Assert.Contains("{Binding Browser.NoticeText}", TagOf(section, "BrowserNoticeText"));
        Assert.Contains("{Binding Browser.Notices}", TagOf(section, "BrowserNoticeList"));
        Assert.Contains("{Binding Browser.ConfirmationText}", TagOf(section, "BrowserConfirmationText"));

        // The buttons of a row and the button of the field, with their handlers and automation names.
        var list = section.Substring(section.IndexOf("x:Name=\"BrowserNoticeList\"", StringComparison.Ordinal), surface - section.IndexOf("x:Name=\"BrowserNoticeList\"", StringComparison.Ordinal));
        Assert.Contains("Click=\"OnBrowserNoticeDiscardDocumentClick\"", list);
        Assert.Contains("Click=\"OnBrowserNoticeDismissClick\"", list);
        Assert.Contains("AutomationProperties.Name=\"Descartar los borradores del documento del aviso\"", list);
        Assert.Contains("AutomationProperties.Name=\"Quitar el aviso\"", list);
        Assert.Contains("Content=\"Entendido\"", list);
        var field = TagOf(section, "BrowserFieldDiscardButton");
        Assert.True(section.IndexOf("x:Name=\"BrowserFieldDiscardButton\"", StringComparison.Ordinal) > surface);
        Assert.Contains("Click=\"OnBrowserFieldDiscardClick\"", field);
        Assert.Contains("AutomationProperties.Name=\"Descartar el borrador\"", field);
        Assert.Contains("Content=\"Descartar\"", field);
        Assert.Contains("{Binding Browser.Field.ShowDiscardButton", field);
    }

    [Fact]
    public void TheNewElementsAreOutsideTheStripHaveNoFixedSizeNoContentControlNoDefaultNorCancelButtonAndNoWindow()
    {
        var xaml = Xaml();
        var section = BrowserSection(xaml);
        var strip = section.Substring(section.IndexOf("x:Name=\"BrowserTabStrip\"", StringComparison.Ordinal));
        strip = strip.Substring(0, strip.IndexOf("</ItemsControl>", StringComparison.Ordinal));

        foreach (var name in new[] { "BrowserDiscardDocumentButton", "BrowserConfirmationPanel", "BrowserConfirmDiscardButton", "BrowserCancelDiscardButton", "BrowserNoticeText", "BrowserNoticeList" })
            Assert.DoesNotContain("x:Name=\"" + name + "\"", strip);

        Assert.DoesNotMatch(new Regex(@"\b(?:Min|Max)?Width\s*=|\b(?:Min|Max)?Height\s*="), section);
        Assert.DoesNotMatch(new Regex(@"<ContentControl|<Window\b|ShowDialog|MessageBox"), section);
        Assert.DoesNotMatch(new Regex(@"\bIsDefault\s*=|\bIsCancel\s*="), xaml);
        Assert.Equal(1, Src.Count(section, @"x:Name=""BrowserSurface"""));
    }

    [Fact]
    public void TheGuardOfTheButtonsSeesIsDefaultAndIsCancelAndTheFixedSizes()
    {
        var bad = new Regex(@"\bIsDefault\s*=|\bIsCancel\s*=");

        Assert.Matches(bad, "<Button x:Name=\"BrowserConfirmDiscardButton\" IsDefault=\"True\" />");
        Assert.Matches(bad, "<Button x:Name=\"BrowserCancelDiscardButton\" IsCancel = \"True\" />");
        Assert.DoesNotMatch(bad, "<Button x:Name=\"BrowserCancelDiscardButton\" Content=\"Cancelar\" />");
        Assert.Matches(new Regex(@"\b(?:Min|Max)?Width\s*="), "<Border MinWidth=\"3\" />");
    }

    [Fact]
    public void TheSurfaceKeepsOneEditOneReloadAndOneReadAgainAndTheDiscardButtonIsNotARepeat()
    {
        var xaml = Xaml();
        var name = xaml.IndexOf("x:Name=\"BrowserSurface\"", StringComparison.Ordinal);
        var start = xaml.LastIndexOf("<Border", name, StringComparison.Ordinal);
        var end = xaml.IndexOf("Text=\"Navegación\"", name, StringComparison.Ordinal);
        var surface = xaml.Substring(start, end - start);

        Assert.Equal(1, Src.Count(surface, "Content=\"Editar\""));
        Assert.Equal(1, Src.Count(surface, "Content=\"Recargar\""));
        Assert.Equal(1, Src.Count(surface, "Content=\"Volver a leer\""));
        Assert.Equal(1, Src.Count(surface, "Content=\"Descartar\""));
    }

    // ------------------------------------------------------------------ the code-behind: handlers inside Contained, N-3 with Key and IsRepeat only

    [Fact]
    public void EveryNewHandlerDelegatesInsideContainedToItsMemberOfTheBrowser()
    {
        var code = Src.Code(CodeBehindPath);

        foreach (var (handler, call) in new[]
                 {
                     ("OnBrowserFieldDiscardClick", @"_viewModel\.Browser\.DiscardField\s*\(\s*\)"),
                     ("OnBrowserDiscardDocumentClick", @"_viewModel\.Browser\.RequestDocumentDiscard\s*\(\s*\)"),
                     ("OnBrowserConfirmDiscardClick", @"_viewModel\.Browser\.ConfirmDiscard\s*\(\s*\)"),
                     ("OnBrowserCancelDiscardClick", @"_viewModel\.Browser\.CancelDiscard\s*\(\s*\)"),
                     ("OnBrowserNoticeDiscardDocumentClick", @"_viewModel\.Browser\.RequestDocumentDiscard\s*\(\s*\)"),
                     ("OnBrowserNoticeDismissClick", @"_viewModel\.Browser\.DismissNotice\s*\(\s*NoticeKeyOf\s*\(\s*sender\s*\)\s*\)"),
                 })
            Assert.Matches(
                @"private\s+void\s+" + handler + @"\s*\(\s*object\s+sender\s*,\s*RoutedEventArgs\s+e\s*\)\s*=>\s*Contained\s*\(\s*\(\s*\)\s*=>\s*" + call + @"\s*\)\s*;",
                code);
    }

    [Fact]
    public void TheKeyHandlerOfTheBoxUsesOnlyTheKeyAndTheRepeatFlagAndTheBoxHasNoKeyUp()
    {
        var code = Src.Code(CodeBehindPath);
        var body = Src.BodyOf(code, @"private\s+void\s+OnBrowserFieldBoxPreviewKeyDown\s*\(");
        var xaml = Xaml();

        Assert.Matches(@"\bKey\?\s+_launchKey\b", code);
        Assert.Contains("e.IsRepeat", body);
        Assert.Contains("e.Key", body);
        Assert.DoesNotMatch(new Regex(@"e\.KeyboardDevice|\bKeyboard\b|\bKeyUp\b|\.Focus\s*\(|FocusManager|e\.SystemKey|e\.ImeProcessedKey|e\.IsDown|e\.IsToggled"), code);
        Assert.DoesNotMatch(new Regex(@"\bKeyUp\b"), xaml);
        Assert.Equal(1, Src.Count(xaml, @"\bPreviewKeyDown\s*="));
    }

    [Fact]
    public void TheGuardOfTheKeyHandlerSeesTheKeyboardDeviceAndKeyUp()
    {
        var bad = new Regex(@"e\.KeyboardDevice|\bKeyboard\b|\bKeyUp\b|\.Focus\s*\(|FocusManager");

        Assert.Matches(bad, "if (e.KeyboardDevice.IsKeyDown(Key.A)) { }");
        Assert.Matches(bad, "Keyboard.Focus(box);");
        Assert.Matches(bad, "box.KeyUp += OnUp;");
        Assert.DoesNotMatch(bad, "if (e.IsRepeat && e.Key == _launchKey.Value) { e.Handled = true; }");
    }

    // ------------------------------------------------------------------ the limits of the task: nothing of the host, the port, the palette or a command changes

    [Fact]
    public void TheUiOfTheTaskReachesNeitherTheIntentionNorTheDrawingNorTheFocusNorADialog()
    {
        var reads = new Regex(@"\b(?:TryBeginEdit|TryReload|TryCheckNow)\b");
        var drawing = new Regex(@"\bTransaction|\bDatabase\b|\.Commit\s*\(|SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync|Autodesk");
        var focus = new Regex(@"\bKeyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager");
        var dialogs = new Regex(@"EnterPanelModal|ShowModalWindow|ShowDialog|MessageBox|\bRegistry\b|\bRackLog\b");

        // The browser view-model names none of the three reads; the one that gestures of the field own stays in the field.
        Assert.DoesNotMatch(reads, Browser());
        foreach (var path in new[] { BrowserModelPath, CodeBehindPath })
        {
            var code = Src.Code(path);
            Assert.DoesNotMatch(drawing, code);
            Assert.DoesNotMatch(focus, code);
            Assert.DoesNotMatch(dialogs, code);
        }
    }

    // ------------------------------------------------------------------ the test filters: the new test classes start with Workspace

    [Fact]
    public void TheNewTestClassesAreSelectedByTheFiltersOfTheTask()
    {
        var root = Src.RepoRoot();
        var core = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "tests", "RackCad.Tests", "Workspace", "WorkspacePilotCloseSyncGuardTests.cs"));
        Assert.Matches(@"(?m)^public\s+class\s+WorkspacePilotCloseSyncGuardTests\b", core);
        Assert.Matches(@"(?m)^namespace\s+RackCad\.Tests\s*;", core);

        foreach (var (file, cls) in new[]
                 {
                     ("WorkspacePilotCloseSyncTests.cs", "WorkspacePilotCloseSyncTests"),
                     ("WorkspacePilotCloseSyncViewTests.cs", "WorkspacePilotCloseSyncViewTests"),
                 })
        {
            var text = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "tests", "RackCad.UI.Tests", "Workspace", file));
            Assert.Matches(@"(?m)^namespace\s+RackCad\.UI\.Tests\b", text);
            Assert.Matches(@"(?m)^\s*public\s+class\s+" + cls + @"\b", text);
        }
    }
}
