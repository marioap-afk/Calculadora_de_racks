#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T4A2-PREPARE-UI (INV-F4T4A2-01, 02, 04, 05, 12; rules 1 to 5 of D-T4A2-07): the source guards of the visible, read-only reconciliation.
// Each guard is a function of a text that returns its violations. It is run on the real source (which must have none) and on a text made from the
// REAL source by ONE Replace (which must have some): every self-test first asserts that the text it replaces is in the real file, so a guard that
// reads nothing, or a replacement that no longer applies, fails instead of passing in silence. The Core test project cannot reference the UI or the
// Plugin, so their files are read as text from the root of the repository.
public class WorkspacePilotReconciliationGuardTests
{
    private const string UiFolder = "src/RackCad.UI/";
    private const string WorkspaceUiFolder = "src/RackCad.UI/Workspace/";
    private const string FieldPath = WorkspaceUiFolder + "WorkspacePilotFieldViewModel.cs";
    private const string BrowserPath = WorkspaceUiFolder + "WorkspaceBrowserViewModel.cs";
    private const string CodeBehindPath = WorkspaceUiFolder + "WorkspacePanelView.xaml.cs";
    private const string XamlPath = WorkspaceUiFolder + "WorkspacePanelView.xaml";
    private const string CorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string NavigationHostPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs";

    // The text of the real file with the line endings of the repository normalized, so a self-test can name a line break.
    private static string Real(string path) => Src.Code(path).Replace("\r\n", "\n");

    private static string RealText(string path) => Src.Text(path).Replace("\r\n", "\n");

    // One Replace of the real text: it asserts that the replaced text is there, and that the result is another text.
    private static string Mutate(string real, string from, string to)
    {
        Assert.Contains(from, real, StringComparison.Ordinal);
        var mutated = real.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(real, mutated);
        return mutated;
    }

    // Two names exchanged in a text, through a placeholder that is not in it.
    private static string Swap(string real, string a, string b)
    {
        Assert.Contains(a, real, StringComparison.Ordinal);
        Assert.Contains(b, real, StringComparison.Ordinal);
        const string placeholder = "\u0001swap\u0001";
        Assert.DoesNotContain(placeholder, real, StringComparison.Ordinal);
        return real.Replace(a, placeholder, StringComparison.Ordinal).Replace(b, a, StringComparison.Ordinal).Replace(placeholder, b, StringComparison.Ordinal);
    }

    private static IReadOnlyList<(string Path, string Code)> UiFiles() =>
        Src.Files(UiFolder).Select(f => (f.Path, f.Code.Replace("\r\n", "\n"))).ToList();

    // The same files with ONE of them replaced by another text.
    private static IReadOnlyList<(string Path, string Code)> With(IReadOnlyList<(string Path, string Code)> files, string path, string code) =>
        files.Select(f => f.Path == path ? (f.Path, code) : f).ToList();

    private static string CodeOf(IReadOnlyList<(string Path, string Code)> files, string path) => files.Single(f => f.Path == path).Code;

    // ================================================================== rule 1: who reads, by token, in every .cs of the UI

    private static readonly (string Token, string Body)[] TheFourReads =
    {
        ("TryConfirmBase", "CheckNow"),
        ("TryCheckNow", "CheckNow"),
        ("TryKeepMine", "ConfirmChoice"),
        ("TryDiscardAndReload", "ConfirmChoice"),
    };

    private static IEnumerable<string> ReaderViolations(IReadOnlyList<(string Path, string Code)> files)
    {
        foreach (var (token, body) in TheFourReads)
        {
            var pattern = @"\b" + token + @"\b";
            foreach (var (path, code) in files)
                if (path != FieldPath && Src.Count(code, pattern) > 0)
                    yield return path + " names " + token;

            var field = files.Where(f => f.Path == FieldPath).ToList();
            if (field.Count != 1)
            {
                yield return "the field view-model is not among the files";
                continue;
            }

            var fieldCode = field[0].Code;
            if (Src.Count(fieldCode, pattern) != 1) yield return "the field view-model does not name " + token + " exactly once";
            if (!Regex.IsMatch(fieldCode, @"public\s+void\s+" + body + @"\s*\(\s*\)")) yield return "the field view-model has no " + body;
            else if (Src.Count(Src.BodyOf(fieldCode, @"public\s+void\s+" + body + @"\s*\(\s*\)"), pattern) != 1)
                yield return token + " is not inside " + body;
        }
    }

    [Fact]
    public void TheFourOperationsOfTheCheckAndOfTheReconciliationAreNamedOnlyByTheFieldViewModelOnceEachInsideTheirGesture()
    {
        Assert.Empty(ReaderViolations(UiFiles()));
    }

    [Fact]
    public void TheGuardOfWhoReadsSeesAKeepInTheRequestACheckInThePresentationAnyOfTheFourInTheBrowserAndANameof()
    {
        var files = UiFiles();
        var field = CodeOf(files, FieldPath);
        var browser = CodeOf(files, BrowserPath);

        Assert.NotEmpty(ReaderViolations(With(files, FieldPath, Mutate(field, "public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);", "public void RequestKeepMine() { _port.TryKeepMine(null); }"))));
        Assert.NotEmpty(ReaderViolations(With(files, FieldPath, Mutate(field, "PilotFieldView view;", "PilotFieldView view; _port.TryConfirmBase(null);"))));
        Assert.NotEmpty(ReaderViolations(With(files, FieldPath, Mutate(field, "PilotFieldView view;", "PilotFieldView view; _port.TryDiscardAndReload(null);"))));
        Assert.NotEmpty(ReaderViolations(With(files, FieldPath, Mutate(field, "PilotFieldView view;", "PilotFieldView view; _port.TryCheckNow(null);"))));
        Assert.NotEmpty(ReaderViolations(With(files, FieldPath, Mutate(field, "private const string SurfaceChangedText", "private static readonly string Aliased = nameof(IWorkspacePilotEditPort.TryKeepMine); private const string SurfaceChangedText"))));
        foreach (var token in TheFourReads.Select(r => r.Token))
            Assert.NotEmpty(ReaderViolations(With(files, BrowserPath, Mutate(browser, "public void ReloadField() => Field.Rematerialize();", "public void ReloadField() { _pilotEdit." + token + "(null); }"))));
    }

    [Fact]
    public void TheGuardOfWhoReadsDoesNotReadTheFileIfTheFieldViewModelIsMissing()
    {
        Assert.NotEmpty(ReaderViolations(UiFiles().Where(f => f.Path != FieldPath).ToList()));
    }

    // ================================================================== rule 2: who calls the five gestures

    private static readonly (string Gesture, string Handler)[] TheFiveGestures =
    {
        ("CheckNow", "OnBrowserFieldCheckClick"),
        ("RequestDiscardAndReload", "OnBrowserFieldDiscardAndReloadClick"),
        ("RequestKeepMine", "OnBrowserFieldKeepMineClick"),
        ("ConfirmChoice", "OnBrowserFieldChoiceYesClick"),
        ("CancelChoice", "OnBrowserFieldChoiceNoClick"),
    };

    private static IEnumerable<string> GestureViolations(IReadOnlyList<(string Path, string Code)> files)
    {
        foreach (var (gesture, handler) in TheFiveGestures)
        {
            var token = @"\b" + gesture + @"\b";
            var declaration = @"public\s+void\s+" + gesture + @"\s*\(\s*\)";
            var call = @"private\s+void\s+" + handler + @"\s*\(\s*object\s+sender\s*,\s*RoutedEventArgs\s+e\s*\)\s*=>\s*Contained\s*\(\s*\(\s*\)\s*=>\s*_viewModel\s*\.\s*Browser\s*\.\s*Field\s*\.\s*"
                       + gesture + @"\s*\(\s*\)\s*\)\s*;";

            foreach (var (path, code) in files)
            {
                var named = Src.Count(code, token);
                if (named == 0) continue;

                if (path == FieldPath)
                {
                    if (named != 1 || Src.Count(code, declaration) != 1) yield return gesture + " is named in the field view-model other than by its declaration";
                }
                else if (path == CodeBehindPath)
                {
                    if (named != 1 || Src.Count(code, call) != 1) yield return gesture + " is named in the code-behind other than by " + handler;
                }
                else
                {
                    yield return path + " names " + gesture;
                }
            }

            if (Src.Count(CodeOf(files, FieldPath), declaration) != 1) yield return gesture + " is not declared once in the field view-model";
            if (Src.Count(CodeOf(files, CodeBehindPath), call) != 1) yield return handler + " is not the exact handler of " + gesture;
        }
    }

    [Fact]
    public void TheFiveGesturesAreDeclaredOnceInTheFieldAndCalledOnlyByTheirExactHandlerOfTheCodeBehind()
    {
        Assert.Empty(GestureViolations(UiFiles()));
    }

    [Fact]
    public void TheGuardOfTheGesturesSeesACallInThePresentationInASetterAsAMethodGroupFromTheBrowserAndTheSwapOfYesAndNo()
    {
        var files = UiFiles();
        var field = CodeOf(files, FieldPath);
        var browser = CodeOf(files, BrowserPath);
        var codeBehind = CodeOf(files, CodeBehindPath);

        Assert.NotEmpty(GestureViolations(With(files, FieldPath, Mutate(field, "_isPreview = isPreview;", "_isPreview = isPreview; CheckNow();"))));
        Assert.NotEmpty(GestureViolations(With(files, FieldPath, Mutate(field, "if (string.Equals(value, _text, StringComparison.Ordinal)) return;", "if (string.Equals(value, _text, StringComparison.Ordinal)) return; this.ConfirmChoice();"))));
        Assert.NotEmpty(GestureViolations(With(files, FieldPath, Mutate(field, "_isPreview = isPreview;", "_isPreview = isPreview; Action a = CheckNow;"))));
        Assert.NotEmpty(GestureViolations(With(files, BrowserPath, Mutate(browser, "public void ReloadField() => Field.Rematerialize();", "public void ReloadField() { Field.CheckNow(); }"))));
        Assert.NotEmpty(GestureViolations(With(files, CodeBehindPath, Swap(codeBehind, "Field.ConfirmChoice()", "Field.CancelChoice()"))));
        Assert.NotEmpty(GestureViolations(With(files, CodeBehindPath, Swap(codeBehind, "Field.CheckNow()", "Field.RequestKeepMine()"))));
        Assert.NotEmpty(GestureViolations(With(files, CodeBehindPath, Mutate(codeBehind, "=> Contained(() => _viewModel.Browser.Field.CheckNow());", "=> _viewModel.Browser.Field.CheckNow();"))));
    }

    [Fact]
    public void ThePatternOfTheGesturesIsNotConfusedWithTheNamesOfTheReads()
    {
        // \bCheckNow does not match TryCheckNow, which has another word boundary: the two rules count different tokens.
        Assert.Equal(0, Src.Count("_port.TryCheckNow(r);", @"\bCheckNow\b"));
        Assert.Equal(1, Src.Count("Field.CheckNow();", @"\bCheckNow\b"));
    }

    // ================================================================== rule 3: the XAML of the five buttons

    private static string XamlWithoutComments() => Regex.Replace(RealText(XamlPath), @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

    // The start tag of the element with that x:Name, and the whole element up to its matching end tag.
    private static (int Start, int End, string StartTag, string Whole)? ElementNamed(string xaml, string name)
    {
        var match = Regex.Match(xaml, @"<(?<tag>\w+)\b[^>]*x:Name=""" + Regex.Escape(name) + @"""[^>]*>", RegexOptions.Singleline);
        if (!match.Success) return null;

        var tag = match.Groups["tag"].Value;
        if (match.Value.EndsWith("/>", StringComparison.Ordinal)) return (match.Index, match.Index + match.Length, match.Value, match.Value);

        var depth = 0;
        foreach (Match part in Regex.Matches(xaml.Substring(match.Index), @"<(?<close>/?)" + tag + @"\b[^>]*?(?<self>/?)>", RegexOptions.Singleline))
        {
            if (part.Groups["close"].Value == "/") depth--;
            else if (part.Groups["self"].Value != "/") depth++;
            if (depth == 0)
            {
                var end = match.Index + part.Index + part.Length;
                return (match.Index, end, match.Value, xaml.Substring(match.Index, end - match.Index));
            }
        }

        return null;
    }

    private static readonly (string Name, string Content, string Automation, string Click, string? Binding)[] TheFiveButtons =
    {
        ("BrowserFieldCheckButton", "Comprobar ahora", "Comprobar el borrador ahora", "OnBrowserFieldCheckClick", "Browser.Field.ShowCheckButton"),
        ("BrowserFieldDiscardAndReloadButton", "Descartar y recargar", "Descartar el borrador y recargarlo", "OnBrowserFieldDiscardAndReloadClick", "Browser.Field.ShowDiscardAndReloadButton"),
        ("BrowserFieldKeepMineButton", "Conservar mi borrador", "Conservar el borrador sobre la versión actual", "OnBrowserFieldKeepMineClick", "Browser.Field.ShowKeepMineButton"),
        ("BrowserFieldChoiceYesButton", "Sí", "Confirmar la elección", "OnBrowserFieldChoiceYesClick", null),
        ("BrowserFieldChoiceNoButton", "No", "Cancelar la elección", "OnBrowserFieldChoiceNoClick", null),
    };

    private static IEnumerable<string> ButtonViolations(string xaml)
    {
        var panel = ElementNamed(xaml, "BrowserFieldPanel");
        var choicePanel = ElementNamed(xaml, "BrowserFieldChoicePanel");
        if (panel == null) { yield return "the field panel is missing"; yield break; }
        if (choicePanel == null) { yield return "the choice panel is missing"; yield break; }

        var choiceStart = choicePanel.Value.StartTag;
        if (!Regex.IsMatch(choiceStart, @"Visibility=""\{Binding\s+Browser\.Field\.HasChoice\s*,\s*Converter=\{StaticResource\s+FlagToVisibility\}\s*\}"""))
            yield return "the choice panel is not visible with Browser.Field.HasChoice";
        if (choicePanel.Value.Start < panel.Value.Start || choicePanel.Value.End > panel.Value.End) yield return "the choice panel is not inside the field panel";

        var order = new List<(string Name, int Index)>();
        foreach (var (name, content, automation, click, binding) in TheFiveButtons)
        {
            if (Src.Count(xaml, @"x:Name=""" + name + @"""") != 1) { yield return name + " does not appear exactly once"; continue; }
            var element = ElementNamed(xaml, name);
            if (element == null) { yield return name + " is not an element"; continue; }
            if (element.Value.Start < panel.Value.Start || element.Value.End > panel.Value.End) yield return name + " is not inside the field panel";

            var tag = element.Value.StartTag;
            if (!tag.StartsWith("<Button", StringComparison.Ordinal)) yield return name + " is not a Button";
            if (!Regex.IsMatch(tag, @"\bContent=""" + Regex.Escape(content) + @"""")) yield return name + " does not have its Content";
            if (!Regex.IsMatch(tag, @"AutomationProperties\.Name=""" + Regex.Escape(automation) + @"""")) yield return name + " does not have its automation name";
            if (!Regex.IsMatch(tag, @"\bClick=""" + Regex.Escape(click) + @"""")) yield return name + " does not have its Click";
            if (Regex.IsMatch(tag, @"\b(?:IsDefault|IsCancel)\s*=")) yield return name + " is a default or cancel button";

            if (binding != null)
            {
                if (!Regex.IsMatch(tag, @"Visibility=""\{Binding\s+" + Regex.Escape(binding) + @"\s*,\s*Converter=\{StaticResource\s+FlagToVisibility\}\s*\}""")) yield return name + " has not its own Visibility";
            }
            else
            {
                if (Regex.IsMatch(tag, @"\bVisibility\s*=")) yield return name + " has a Visibility of its own";
                if (choicePanel.Value.Start >= element.Value.Start || element.Value.End > choicePanel.Value.End) yield return name + " is not inside the choice panel";
            }

            order.Add((name, element.Value.Start));
        }

        var yes = order.FirstOrDefault(o => o.Name == "BrowserFieldChoiceYesButton");
        var no = order.FirstOrDefault(o => o.Name == "BrowserFieldChoiceNoButton");
        if (yes.Name != null && no.Name != null && yes.Index >= no.Index) yield return "Yes does not come before No";

        var text = ElementNamed(xaml, "BrowserFieldChoiceText");
        if (text == null || !Regex.IsMatch(text.Value.StartTag, @"Text=""\{Binding\s+Browser\.Field\.ChoiceText\s*\}""")) yield return "the question is not bound to ChoiceText";
        var reconcile = ElementNamed(xaml, "BrowserFieldReconcileText");
        if (reconcile == null || !Regex.IsMatch(reconcile.Value.StartTag, @"Text=""\{Binding\s+Browser\.Field\.ReconcileText\s*\}""")) yield return "the line is not bound to ReconcileText";
    }

    [Fact]
    public void TheXamlHoldsTheFiveButtonsOfTheTableInsideTheFieldPanelWithTheirOwnNamesTextsHandlersAndVisibility()
    {
        Assert.Empty(ButtonViolations(XamlWithoutComments()));
    }

    [Fact]
    public void TheGuardOfTheButtonsSeesTheSwapOfYesAndNoOfTheirNamesAndOfTheClicksOfTheCheckAndTheKeep()
    {
        var xaml = XamlWithoutComments();

        Assert.NotEmpty(ButtonViolations(Swap(xaml, "Click=\"OnBrowserFieldChoiceYesClick\"", "Click=\"OnBrowserFieldChoiceNoClick\"")));
        Assert.NotEmpty(ButtonViolations(Swap(xaml, "AutomationProperties.Name=\"Confirmar la elección\"", "AutomationProperties.Name=\"Cancelar la elección\"")));
        Assert.NotEmpty(ButtonViolations(Swap(xaml, "Click=\"OnBrowserFieldCheckClick\"", "Click=\"OnBrowserFieldKeepMineClick\"")));
        Assert.NotEmpty(ButtonViolations(Swap(xaml, "Content=\"Sí\"", "Content=\"No\"")));
        Assert.NotEmpty(ButtonViolations(Swap(xaml, "x:Name=\"BrowserFieldChoiceYesButton\"", "x:Name=\"BrowserFieldChoiceNoButton\"")));
    }

    [Fact]
    public void TheGuardOfTheButtonsSeesAChangedBindingAVisibilityOnYesAnIsDefaultAndTheChoicePanelWithoutItsBinding()
    {
        var xaml = XamlWithoutComments();

        Assert.NotEmpty(ButtonViolations(Mutate(xaml, "{Binding Browser.Field.ShowCheckButton,", "{Binding Browser.Field.ShowKeepMineButton,")));
        Assert.NotEmpty(ButtonViolations(Mutate(xaml, "Click=\"OnBrowserFieldChoiceYesClick\"", "Visibility=\"Visible\" Click=\"OnBrowserFieldChoiceYesClick\"")));
        Assert.NotEmpty(ButtonViolations(Mutate(xaml, "Content=\"No\"", "Content=\"No\" IsCancel=\"True\"")));
        Assert.NotEmpty(ButtonViolations(Mutate(xaml, "{Binding Browser.Field.HasChoice,", "{Binding Browser.Field.ShowCheckButton,")));
        Assert.NotEmpty(ButtonViolations(Mutate(xaml, "Text=\"{Binding Browser.Field.ChoiceText}\"", "Text=\"{Binding Browser.Field.ReconcileText}\"")));
        Assert.NotEmpty(ButtonViolations(Mutate(xaml, "Content=\"Comprobar ahora\"", "Content=\"Comprobar\"")));
    }

    // ================================================================== rule 4: no false clean

    private static string FolderText() =>
        string.Concat(Src.Files(WorkspaceUiFolder).Select(f => Src.Text(f.Path).Replace("\r\n", "\n")))
        + string.Concat(new[] { XamlPath }.Select(RealText));

    private static int CountOrdinal(string text, string needle)
    {
        var count = 0;
        for (var index = text.IndexOf(needle, StringComparison.Ordinal); index >= 0; index = text.IndexOf(needle, index + needle.Length, StringComparison.Ordinal)) count++;
        return count;
    }

    // The text of an answer that says "nothing to do" lives in ONE private constant of the field view-model, referenced once, inside the case of the
    // one answer that says it; nothing else of the folder repeats the text or one of its halves, whole or in two pieces.
    private static IEnumerable<string> FalseCleanViolations(string field, string folder, string literal, string firstHalf, string secondHalf, string splitPattern, string caseLabel)
    {
        if (CountOrdinal(folder, literal) != 1) yield return "the text is not exactly once in the folder";
        if (CountOrdinal(folder, firstHalf) != 1) yield return "the first half is not exactly once in the folder";
        if (CountOrdinal(folder, secondHalf) != 1) yield return "the second half is not exactly once in the folder";
        // The folder is the text of the files as they are on disk; the field view-model is also given apart (without its comments), so a piece
        // built in the field is seen even when only that text was changed.
        if (Regex.IsMatch(folder + "\n" + field, splitPattern)) yield return "the text is built in two pieces";

        var declaration = Regex.Match(field, @"private\s+const\s+string\s+(?<name>\w+)\s*=\s*""" + Regex.Escape(literal) + @"""\s*;");
        if (!declaration.Success) { yield return "the text is not a private constant of the field view-model"; yield break; }

        var name = declaration.Groups["name"].Value;
        if (Src.Count(field, @"\b" + name + @"\b") != 2) yield return name + " is not referenced exactly once";

        var caseStart = field.IndexOf(caseLabel, StringComparison.Ordinal);
        if (caseStart < 0) { yield return "there is no " + caseLabel; yield break; }

        var rest = field.Substring(caseStart + caseLabel.Length);
        var next = Regex.Match(rest, @"\bcase\s+\w+\.\w+\s*:|\bdefault\s*:");
        var section = next.Success ? rest.Substring(0, next.Index) : rest;
        if (Src.Count(section, @"\b" + name + @"\b") != 1) yield return name + " is not referenced inside " + caseLabel;
    }

    private static IEnumerable<string> NoChangesViolations(string field, string folder) => FalseCleanViolations(
        field, folder,
        "Sin cambios: el dibujo coincide con la base del borrador.",
        "Sin cambios",
        "el dibujo coincide con la base del borrador",
        @"""Sin\s*""\s*\+\s*""\s*cambios",
        "case EditConfirmStatus.Confirmed:");

    private static IEnumerable<string> NotAppliedViolations(string field, string folder) => FalseCleanViolations(
        field, folder,
        "El cambio no llegó a aplicarse: el borrador recupera el estado que tenía antes.",
        "El cambio no llegó a aplicarse",
        "el borrador recupera el estado que tenía antes",
        @"""El\s*""\s*\+\s*""\s*cambio",
        "case EditCheckStatus.Recovered:");

    [Fact]
    public void TheTextOfNoChangesAndTheTextOfNotAppliedLiveOnlyInTheirConstantReferencedOnceInTheCaseOfTheAnswerThatSaysThem()
    {
        var field = Real(FieldPath);
        var folder = FolderText();

        Assert.Empty(NoChangesViolations(field, folder));
        Assert.Empty(NotAppliedViolations(field, folder));
    }

    [Fact]
    public void TheGuardOfNoFalseCleanSeesASecondReferenceTheConstantInAnotherCaseAndTheTextBuiltInTwoPieces()
    {
        var field = Real(FieldPath);
        var folder = FolderText();

        Assert.NotEmpty(NoChangesViolations(Mutate(field, "return ChangedDuringCheckText;", "return UnchangedText;"), folder));
        Assert.NotEmpty(NoChangesViolations(Mutate(field, "case EditConfirmStatus.Confirmed:", "case EditConfirmStatus.Failed:"), folder));
        Assert.NotEmpty(NoChangesViolations(Mutate(field, "return ChangedDuringCheckText;", "return \"Sin \" + \"cambios\";"), folder));
        Assert.NotEmpty(NotAppliedViolations(Mutate(field, "return UnknownConflictText;", "return RecoveredText;"), folder));
        Assert.NotEmpty(NotAppliedViolations(Mutate(field, "case EditCheckStatus.Recovered:", "case EditCheckStatus.Failed:"), folder));
        Assert.NotEmpty(NotAppliedViolations(Mutate(field, "return UnknownConflictText;", "return \"El \" + \"cambio no llegó\";"), folder));
    }

    [Fact]
    public void TheGuardOfNoFalseCleanSeesTheTextOrItsHalvesAnywhereElseInTheFolderOfTheUi()
    {
        var field = Real(FieldPath);
        var folder = FolderText();

        Assert.NotEmpty(NoChangesViolations(field, folder + "\nSin cambios."));
        Assert.NotEmpty(NoChangesViolations(field, folder + "\n<TextBlock Text=\"Sin cambios: el dibujo coincide con la base del borrador.\" />"));
        Assert.NotEmpty(NoChangesViolations(field, folder + "\nel dibujo coincide con la base del borrador"));
        Assert.NotEmpty(NotAppliedViolations(field, folder + "\nEl cambio no llegó a aplicarse."));
        Assert.NotEmpty(NotAppliedViolations(field, folder + "\nel borrador recupera el estado que tenía antes"));
        // The comparison is ordinal: the same words in another case are another text and are not flagged.
        Assert.Empty(NoChangesViolations(field, folder + "\nsin cambios\nCabecera sin cambios."));
    }

    // ================================================================== rule 5: Authorize by exact form

    private const string PassGateForm =
        @"if\s*\(\s*Authorize\s*\(\s*new\s+WorkspaceRequest\s*\(\s*session\s*\)\s*\)\s*!=\s*RequestDecision\.Accepted\s*\)\s*\{\s*reason\s*=\s*""La petición no es del documento activo\.""\s*;\s*return\s+EditGate\.Rejected\s*;\s*\}";

    private static IEnumerable<string> PassGateViolations(string core)
    {
        var body = Src.BodyOf(core, @"private\s+EditGate\s+PassGate\s*\(");
        if (Src.Count(body, @"\bAuthorize\s*\(") != 1) yield return "PassGate does not call Authorize exactly once";
        if (Src.Count(body, PassGateForm) != 1) yield return "the authorization of PassGate is not of the exact form";
    }

    [Fact]
    public void PassGateAuthorizesTheSessionAgainstTheActiveDocumentByTheExactFormAndOnlyOnce()
    {
        Assert.Empty(PassGateViolations(Real(CorePath)));
    }

    [Fact]
    public void TheGuardOfPassGateSeesTheComparisonTheDecisionTheSessionTheIgnoredResultAndTheAlwaysTrue()
    {
        var core = Real(CorePath);
        const string head = "Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted)\n            {\n                reason";

        Assert.NotEmpty(PassGateViolations(Mutate(core, head, "Authorize(new WorkspaceRequest(session)) == RequestDecision.Accepted)\n            {\n                reason")));
        Assert.NotEmpty(PassGateViolations(Mutate(core, head, "Authorize(new WorkspaceRequest(session)) != RequestDecision.RejectedSessionMismatch)\n            {\n                reason")));
        Assert.NotEmpty(PassGateViolations(Mutate(core, head, "Authorize(new WorkspaceRequest(default)) != RequestDecision.Accepted)\n            {\n                reason")));
        Assert.NotEmpty(PassGateViolations(Mutate(core, head, "Authorize(new WorkspaceRequest(new SessionId(1))) != RequestDecision.Accepted)\n            {\n                reason")));
        Assert.NotEmpty(PassGateViolations(Mutate(core, head, "false)\n            {\n                reason")));
        Assert.NotEmpty(PassGateViolations(Mutate(core, head, "Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted || true)\n            {\n                reason")));
        Assert.NotEmpty(PassGateViolations(Mutate(core, "reason = \"La petición no es del documento activo.\";\n                return EditGate.Rejected;", "reason = \"La petición no es del documento activo.\";\n                return EditGate.Open;")));
    }

    // The reader of the Plugin: every call of the authorization inside the nested class has the exact form, without asking for a unique one, so a
    // later read can be added to the class. The two other calls of the file (the summary and the navigation) are outside the rule.
    private const string ReaderAuthorizeForm =
        @"var\s+decision\s*=\s*_authorize\s*\(\s*new\s+WorkspaceRequest\s*\(\s*session\s*\)\s*,\s*document\s*\.\s*UnmanagedObject\s*\.\s*ToInt64\s*\(\s*\)\s*\)\s*;\s*if\s*\(\s*decision\s*!=\s*RequestDecision\.Accepted\s*\)\s*return\s+EditIntentReadResult\.Rejected\s*\(";

    private static IEnumerable<string> ReaderAuthorizeViolations(string host)
    {
        var reader = Src.BodyOf(host, @"private\s+sealed\s+class\s+PilotIntentReader\s*:\s*IWorkspaceEditIntentReader");
        var calls = Src.Count(reader, @"\b_authorize\s*\(");
        if (calls == 0) yield return "the reader does not authorize";
        if (Src.Count(reader, ReaderAuthorizeForm) != calls) yield return "a call of _authorize in the reader is not of the exact form";
    }

    [Fact]
    public void TheReaderOfThePluginAuthorizesTheSessionAgainstTheDocumentThatExecutesItByTheExactFormAndTheOtherCallsOfTheFileAreOutsideTheRule()
    {
        var host = Real(NavigationHostPath);

        Assert.Empty(ReaderAuthorizeViolations(host));
        Assert.Equal(1, Src.Count(Src.BodyOf(host, @"private\s+sealed\s+class\s+PilotIntentReader\s*:\s*IWorkspaceEditIntentReader"), @"\b_authorize\s*\("));
        Assert.Equal(3, Src.Count(host, @"\b_authorize\s*\("));
    }

    [Fact]
    public void TheGuardOfTheReaderSeesTheComparisonTheDecisionTheSessionTheDocumentTheIgnoredResultAndTheAlwaysTrue()
    {
        var host = Real(NavigationHostPath);
        const string call = "var decision = _authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64());";
        const string check = "if (decision != RequestDecision.Accepted)\n                        return EditIntentReadResult.Rejected(";

        Assert.NotEmpty(ReaderAuthorizeViolations(Mutate(host, check, "if (decision == RequestDecision.Accepted)\n                        return EditIntentReadResult.Rejected(")));
        Assert.NotEmpty(ReaderAuthorizeViolations(Mutate(host, check, "if (decision != RequestDecision.RejectedSessionMismatch)\n                        return EditIntentReadResult.Rejected(")));
        Assert.NotEmpty(ReaderAuthorizeViolations(Mutate(host, call, "var decision = _authorize(new WorkspaceRequest(default), document.UnmanagedObject.ToInt64());")));
        Assert.NotEmpty(ReaderAuthorizeViolations(Mutate(host, call, "var decision = _authorize(new WorkspaceRequest(new SessionId(1)), document.UnmanagedObject.ToInt64());")));
        Assert.NotEmpty(ReaderAuthorizeViolations(Mutate(host, call, "var decision = _authorize(new WorkspaceRequest(session), 0L);")));
        Assert.NotEmpty(ReaderAuthorizeViolations(Mutate(host, call, "_authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64()); var decision = RequestDecision.Accepted;")));
        Assert.NotEmpty(ReaderAuthorizeViolations(Mutate(host, check, "if (decision != RequestDecision.Accepted || true)\n                        return EditIntentReadResult.Rejected(")));
    }

    [Fact]
    public void TheGuardOfTheReaderDoesNotAskForAUniqueCallSoALaterReadCanBeAddedToTheClassButASecondCallOfAnotherFormIsSeen()
    {
        var host = Real(NavigationHostPath);
        const string call = "var decision = _authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64());";
        const string check = "if (decision != RequestDecision.Accepted)\n                        return EditIntentReadResult.Rejected(\"La petición no es del documento activo.\");";

        Assert.Contains(check, host, StringComparison.Ordinal);
        Assert.Empty(ReaderAuthorizeViolations(Mutate(host, call + "\n                    " + check, call + "\n                    " + check + "\n                    " + call + "\n                    " + check)));
        Assert.NotEmpty(ReaderAuthorizeViolations(Mutate(host, check, check + "\n                    _authorize(new WorkspaceRequest(session), 0L);")));
    }
}
