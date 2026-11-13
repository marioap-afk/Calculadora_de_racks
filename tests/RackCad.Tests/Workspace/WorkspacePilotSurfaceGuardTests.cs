#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T3B1-EDIT-SURFACE (INV-F4T3B1-01, 03, 04, 09, 10, 14; D-T3B1-12): source guards of the edit surface of the pilot in the panel. The Core
// test project cannot reference the Plugin (it needs AutoCAD) nor load WPF, so those files are read as text, like the rest of the guards of the
// workspace. These guards are ADDITIVE and live in a new file. By the rule I-1 they are limited to this task, they are written in the negative
// and by the body of a member, and none of them fixes an exact set of a whole file or forbids what belongs to F4-T3B2 (the discard of a tab or
// of the document, the notices of veto and loss, the retention by selection), to F4-T3C or to F4-T4. Each one has a self-test that mutates a
// synthetic source and proves that the guard sees the violation, and that it leaves the legitimate text alone.
public class WorkspacePilotSurfaceGuardTests
{
    private const string UiFolder = "src/RackCad.UI/Workspace/";
    private const string FieldModelPath = UiFolder + "WorkspacePilotFieldViewModel.cs";
    private const string BrowserModelPath = UiFolder + "WorkspaceBrowserViewModel.cs";
    private const string PanelModelPath = UiFolder + "WorkspacePanelViewModel.cs";
    private const string XamlPath = UiFolder + "WorkspacePanelView.xaml";
    private const string CodeBehindPath = UiFolder + "WorkspacePanelView.xaml.cs";
    private const string PaletteHostPath = "src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs";

    private static readonly Regex TheThreeReads = new(@"\b(?:TryBeginEdit|TryReload|TryCheckNow)\b");

    // ------------------------------------------------------------------ helpers: the body of a member, by its name

    // The text between the braces that open at or after 'from' (or up to the semicolon of an expression body).
    private static string BodyFrom(string code, int from)
    {
        var open = code.IndexOf('{', from);
        var arrow = code.IndexOf("=>", from, StringComparison.Ordinal);
        if (arrow >= 0 && (open < 0 || arrow < open))
        {
            var end = code.IndexOf(';', arrow);
            return end < 0 ? code.Substring(arrow) : code.Substring(arrow, end - arrow + 1);
        }

        if (open < 0) return string.Empty;
        var depth = 0;
        for (var i = open; i < code.Length; i++)
        {
            if (code[i] == '{') depth++;
            else if (code[i] == '}' && --depth == 0) return code.Substring(open, i - open + 1);
        }

        return code.Substring(open);
    }

    // Every body of a method with one of the names ('void' or 'bool' members), of a constructor of one of the classes and of every property setter.
    private static IEnumerable<(string Name, string Body)> NonGestureMembers(string code)
    {
        var names = @"Present|Refresh|Show|Clear|RebuildTabs|SyncSurface|StartSurface|ClearSurface|Materialize";
        foreach (Match m in Regex.Matches(code, @"\b(?:void|bool)\s+(?<name>" + names + @")\s*\("))
            yield return (m.Groups["name"].Value, BodyFrom(code, m.Index + m.Length));

        foreach (Match m in Regex.Matches(code, @"\bpublic\s+(?<name>WorkspaceBrowserViewModel|WorkspacePilotFieldViewModel|WorkspacePanelViewModel)\s*\("))
            yield return (m.Groups["name"].Value + " (constructor)", BodyFrom(code, m.Index + m.Length));

        foreach (Match m in Regex.Matches(code, @"\bset\s*(?=\{|=>)"))
            yield return ("a setter", BodyFrom(code, m.Index + m.Length));
    }

    // ------------------------------------------------------------------ INV-F4T3B1-09: only a gesture reads

    // The three reads of the port are named only by the gesture methods of the field view-model; the presentation, the rebuilds, the
    // constructors and the setters never name them, and the check of the draft is not used at all in this task.
    private static IEnumerable<string> ReadViolations(IEnumerable<(string Path, string Code)> files)
    {
        foreach (var (path, code) in files)
        {
            foreach (var (name, body) in NonGestureMembers(code))
                if (TheThreeReads.IsMatch(body))
                    yield return path + ": " + name + " names " + TheThreeReads.Match(body).Value;

            if (!path.EndsWith("WorkspacePilotFieldViewModel.cs", StringComparison.Ordinal) && TheThreeReads.IsMatch(code))
                yield return path + " names a read of the port outside the field view-model";
            if (Regex.IsMatch(code, @"\bTryCheckNow\b"))
                yield return path + " names TryCheckNow";
        }
    }

    [Fact]
    public void OnlyTheGestureMethodsOfTheFieldNameTheThreeReadsAndThePresentationNeverDoes()
    {
        Assert.Empty(ReadViolations(Src.Files(UiFolder)));
        Assert.Empty(ReadViolations(Src.Files("src/RackCad.Plugin/Workspace/").Where(f => f.Path == PaletteHostPath)));
    }

    [Theory]
    [InlineData("public void Present(int a) { _port.TryReload(r); }")]
    [InlineData("private void Refresh() => _port.TryBeginEdit(r);")]
    [InlineData("public void Show(int a) { x = 1; _port.TryBeginEdit(r); }")]
    [InlineData("private void RebuildTabs() { _port.TryReload(r); }")]
    [InlineData("private void SyncSurface(bool a) { _port.TryBeginEdit(r); }")]
    [InlineData("private void StartSurface(string a) { _port.TryBeginEdit(r); }")]
    [InlineData("private void ClearSurface() { _port.TryReload(r); }")]
    [InlineData("private void Materialize(bool a) { _port.TryReload(r); }")]
    [InlineData("public WorkspacePilotFieldViewModel(object p) { p.TryBeginEdit(r); }")]
    [InlineData("public string Text { get => x; set { _port.TryReload(r); } }")]
    public void TheGuardOfTheReadsSeesAReadInAMemberThatIsNotAGesture(string member)
    {
        var violations = ReadViolations(new[] { (FieldModelPath, "class C { " + member + " }") }).ToList();

        Assert.NotEmpty(violations);
    }

    [Fact]
    public void TheGuardOfTheReadsLeavesAGestureAloneAndSeesAReadOutsideTheFieldAndTheCheckAnywhere()
    {
        Assert.Empty(ReadViolations(new[] { (FieldModelPath, "class C { public void BeginEdit() { var o = _port.TryBeginEdit(r); var p = _port.TryReload(r); } public void Show(int a) { x = 1; } }") }));
        Assert.NotEmpty(ReadViolations(new[] { (BrowserModelPath, "class C { public void BeginFieldEdit() { _port.TryBeginEdit(r); } }") }));
        Assert.NotEmpty(ReadViolations(new[] { (FieldModelPath, "class C { public void BeginEdit() { _port.TryCheckNow(r); } }") }));
    }

    // ------------------------------------------------------------------ INV-F4T3B1-04: SetPendingText only in the setter of the text

    // The text typed by the user reaches the draft only through the public setter of Text of the field view-model: nobody else in the UI
    // names SetPendingText, and inside the field view-model it is named only within that setter.
    private static IEnumerable<string> SetPendingViolations(IEnumerable<(string Path, string Code)> files)
    {
        foreach (var (path, code) in files)
        {
            var total = Src.Count(code, @"\bSetPendingText\b");
            if (total == 0) continue;
            if (!path.EndsWith("WorkspacePilotFieldViewModel.cs", StringComparison.Ordinal))
            {
                yield return path + " names SetPendingText";
                continue;
            }

            var property = Regex.Match(code, @"\bpublic\s+string\s+Text\b");
            var inside = 0;
            if (property.Success)
            {
                var propertyBody = BodyFrom(code, property.Index + property.Length);
                var setter = Regex.Match(propertyBody, @"\bset\s*(?=\{)");
                if (setter.Success) inside = Src.Count(BodyFrom(propertyBody, setter.Index + setter.Length), @"\bSetPendingText\b");
            }

            if (inside != total) yield return path + " names SetPendingText outside the setter of Text";
        }
    }

    [Fact]
    public void SetPendingTextIsNamedOnlyInTheSetterOfTheTextOfTheFieldViewModel()
    {
        Assert.Empty(SetPendingViolations(Src.Files(UiFolder)));
    }

    [Fact]
    public void TheGuardOfSetPendingTextSeesItOutsideTheSetterAndLeavesTheSetterAlone()
    {
        const string good = "class C { public string Text { get => _t; set { _port.SetPendingText(o, r, value); } } public void Show() { } }";
        const string elsewhere = "class C { public string Text { get => _t; set { _port.SetPendingText(o, r, value); } } public void Show() { _port.SetPendingText(o, r, \"x\"); } }";
        const string getter = "class C { public string Text { get { _port.SetPendingText(o, r, \"x\"); return _t; } set { } } }";

        Assert.Empty(SetPendingViolations(new[] { (FieldModelPath, good) }));
        Assert.NotEmpty(SetPendingViolations(new[] { (FieldModelPath, elsewhere) }));
        Assert.NotEmpty(SetPendingViolations(new[] { (FieldModelPath, getter) }));
        Assert.NotEmpty(SetPendingViolations(new[] { (BrowserModelPath, good) }));
    }

    // ------------------------------------------------------------------ INV-F4T3B1-04: the box is bound, never captured

    // The XAML and the code-behind do not capture the text of the box: no TextChanged (a handler with RoutedEventArgs would be valid for a
    // TextChangedEventHandler by contravariance), no SourceUpdated, no TargetUpdated, and the code-behind assigns no Text.
    private static IEnumerable<string> BoxCaptureViolations(string xaml, string codeBehind)
    {
        if (Regex.IsMatch(xaml, @"\bTextChanged\s*=")) yield return "the XAML captures TextChanged";
        if (Regex.IsMatch(xaml, @"\b(?:SourceUpdated|TargetUpdated)\b")) yield return "the XAML captures SourceUpdated or TargetUpdated";
        if (Regex.IsMatch(codeBehind, @"\b(?:TextChanged|SourceUpdated|TargetUpdated)\b")) yield return "the code-behind captures the text";
        if (Regex.IsMatch(codeBehind, @"\.Text\s*=(?!=)")) yield return "the code-behind assigns Text";
    }

    [Fact]
    public void NeitherTheXamlNorTheCodeBehindCaptureTheTextOfTheBoxAndTheCodeBehindAssignsNoText()
    {
        var xaml = Regex.Replace(Src.Text(XamlPath), @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

        Assert.Empty(BoxCaptureViolations(xaml, Src.Code(CodeBehindPath)));
    }

    [Theory]
    [InlineData("<TextBox TextChanged=\"OnChanged\" />", "")]
    [InlineData("<TextBox TextChanged = \"OnChanged\" />", "")]
    [InlineData("<TextBox SourceUpdated=\"OnX\" />", "")]
    [InlineData("<TextBox TargetUpdated=\"OnX\" />", "")]
    [InlineData("<TextBox />", "box.TextChanged += OnChanged;")]
    [InlineData("<TextBox />", "box.Text = \"x\";")]
    [InlineData("<TextBox />", "FieldBox.Text=value;")]
    public void TheGuardOfTheBoxSeesTheCaptureOfTheText(string xaml, string codeBehind)
    {
        Assert.NotEmpty(BoxCaptureViolations(xaml, codeBehind));
    }

    [Fact]
    public void TheGuardOfTheBoxLeavesTheBindingAndTheComparisonsAlone()
    {
        Assert.Empty(BoxCaptureViolations(
            "<TextBox Text=\"{Binding Browser.Field.Text, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}\" />",
            "if (box.Text == \"x\") { } var t = box.Text;"));
    }

    [Fact]
    public void TheBoxIsBoundTwoWayToTheTextOfTheFieldAndItsGesturesAreDeclaredInTheXaml()
    {
        var xaml = Regex.Replace(Src.Text(XamlPath), @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
        var box = Regex.Match(xaml, @"<TextBox\b[^>]*>", RegexOptions.Singleline).Value;

        Assert.Equal(1, Src.Count(xaml, @"<TextBox\b"));
        Assert.Contains("x:Name=\"BrowserFieldBox\"", box);
        Assert.Matches(@"Text=""\{Binding\s+Browser\.Field\.Text\s*,\s*Mode=TwoWay\s*,\s*UpdateSourceTrigger=PropertyChanged\}""", box);
        foreach (var handler in new[] { "PreviewMouseLeftButtonDown", "PreviewKeyDown", "KeyDown", "LostFocus" })
            Assert.Matches(@"\b" + handler + @"=""OnBrowserFieldBox\w+""", box);
    }

    // ------------------------------------------------------------------ INV-F4T3B1-01: the palette only passes the port of the controller

    private static IEnumerable<string> PaletteViolations(string host)
    {
        if (Src.Count(host, @"_controller\.PilotEdit\b") != 1) yield return "the palette does not pass _controller.PilotEdit exactly once";
        if (Regex.IsMatch(host, @"IWorkspacePilotEditPort|PilotField|PilotBoundary|EditIntent|WorkspaceBrowser"))
            yield return "the palette names a type of the port";

        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "Port", "SetInstrumentationEnabled", "Navigation", "BrowserSummary", "PilotEdit", "StateChanged", "ShowPanel", "HidePanel", "PanelVisible",
        };
        foreach (Match used in Regex.Matches(host, @"_controller\.(?<member>\w+)"))
            if (!allowed.Contains(used.Groups["member"].Value))
                yield return "the palette uses _controller." + used.Groups["member"].Value;
    }

    [Fact]
    public void ThePaletteHostPassesTheEditPortOfTheControllerAndNothingNewAndNamesNoTypeOfIt()
    {
        Assert.Empty(PaletteViolations(Src.Code(PaletteHostPath)));
    }

    [Fact]
    public void TheGuardOfThePaletteSeesAnotherMemberAMissingPortAndATypeName()
    {
        const string good = "var vm = new WorkspacePanelViewModel(_controller.Port, _controller.SetInstrumentationEnabled, _controller.Navigation, _controller.BrowserSummary, _controller.PilotEdit); _controller.StateChanged += OnX;";

        Assert.Empty(PaletteViolations(good));
        Assert.NotEmpty(PaletteViolations(good.Replace(", _controller.PilotEdit", string.Empty)));
        Assert.NotEmpty(PaletteViolations(good + " var x = _controller.TryGetSession;"));
        Assert.NotEmpty(PaletteViolations(good + " IWorkspacePilotEditPort p;"));
        Assert.NotEmpty(PaletteViolations(good + " var y = _controller.PilotEdit;"));
    }

    // ------------------------------------------------------------------ INV-F4T3B1-10 and 14: no write, no focus, no subdialog, no scheduling

    // What the UI of the workspace never names in this task: the drawing, the commands, the focus and the dialogs. It does not forbid the
    // operations of the port that belong to the later tasks (the discard, the notices and the risks).
    private static IEnumerable<string> LimitViolations(IEnumerable<(string Path, string Code)> files)
    {
        var drawing = new Regex(@"\bTransaction|\bDatabase\b|\.Commit\s*\(|SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync|Autodesk");
        var focus = new Regex(@"\bKeyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager");
        var dialogs = new Regex(@"EnterPanelModal|ShowModalWindow|ShowDialog|MessageBox");
        var scheduling = new Regex(@"\bDispatcher\b|\w*Timer\b|\bTask\b|\basync\b|\bawait\b|\bThread\b|\.Result\b|\.Wait\s*\(");
        foreach (var (path, code) in files)
        {
            foreach (var (kind, pattern) in new[] { ("writes or reaches the drawing", drawing), ("uses the focus", focus), ("opens a dialog", dialogs) })
                if (pattern.IsMatch(code)) yield return path + " " + kind + ": " + pattern.Match(code).Value;
            if (path != "src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs" && scheduling.IsMatch(code))
                yield return path + " schedules: " + scheduling.Match(code).Value;
        }
    }

    [Fact]
    public void TheUiOfTheWorkspaceNamesNoApiOfWritingTheDrawingNorOfTheFocusNorADialogAndSchedulesNothing()
    {
        Assert.Empty(LimitViolations(Src.Files(UiFolder)));
    }

    [Theory]
    [InlineData("var t = db.TransactionManager;")]
    [InlineData("tr.Commit();")]
    [InlineData("doc.SendStringToExecute(\"X\");")]
    [InlineData("Keyboard.Focus(box);")]
    [InlineData("box.Focus();")]
    [InlineData("FocusManager.SetFocusedElement(a, b);")]
    [InlineData("using (_c.EnterPanelModal()) { }")]
    [InlineData("MessageBox.Show(\"x\");")]
    [InlineData("Dispatcher.BeginInvoke(a);")]
    [InlineData("var t = new DispatcherTimer();")]
    public void TheGuardOfTheLimitsSeesEachOfTheForbiddenNames(string statement)
    {
        Assert.NotEmpty(LimitViolations(new[] { (FieldModelPath, "class C { void M() { " + statement + " } }") }));
    }

    [Fact]
    public void TheGuardOfTheLimitsLeavesTheOperationsOfTheLaterTasksAndTheEventArgsAlone()
    {
        const string legitimate = "void M(KeyEventArgs e, TextCompositionEventArgs t) { _port.DiscardRack(o, r); _port.GetRacksAtRisk(o); _port.TakeCloseNotices(); e.Handled = true; } void LostFocusX() { }";

        Assert.Empty(LimitViolations(new[] { (FieldModelPath, "class C { " + legitimate + " }") }));
    }

    // The field view-model has no subscription, no event, no delegate type of its own and no static state: it tells the browser by the
    // method that its constructor receives.
    [Fact]
    public void TheFieldViewModelIsSynchronousHasNoSubscriptionNoEventAndNoStaticState()
    {
        var code = Src.Code(FieldModelPath);

        Assert.DoesNotMatch(new Regex(@"\+=|-=|\bevent\b|\bdelegate\b|\bstatic\s+(?!readonly\s+string\b)\w+\s+\w+\s*;"), code);
        Assert.DoesNotMatch(new Regex(@"Autodesk|RackCad\.Plugin|WorkspaceHostCore|WorkspaceController|IWorkspaceHostPort"), code);
        Assert.Matches(@"public\s+sealed\s+class\s+WorkspacePilotFieldViewModel\s*:\s*ObservableObject\b", code);
        Assert.Equal(1, Src.Count(code, @"\bpublic\s+WorkspacePilotFieldViewModel\s*\("));
        Assert.Matches(@"WorkspacePilotFieldViewModel\s*\(\s*IWorkspacePilotEditPort\s+\w+\s*,\s*Action\s+\w+", code);
    }

    [Fact]
    public void TheBrowserOwnsTheFieldAndTheOnlyPublicConstructorTakesThePilotPortAsItsThirdOptionalParameter()
    {
        var code = Src.Code(BrowserModelPath);

        Assert.Equal(1, Src.Count(code, @"\bpublic\s+WorkspaceBrowserViewModel\s*\("));
        Assert.Matches(
            @"public\s+WorkspaceBrowserViewModel\s*\(\s*IWorkspaceBrowserSummaryPort\s+\w+\s*,\s*Action\s+\w+\s*=\s*null\s*,\s*IWorkspacePilotEditPort\s+\w+\s*=\s*null\s*\)",
            code);
        Assert.Matches(@"public\s+WorkspacePilotFieldViewModel\s+Field\b", code);
        Assert.Equal(1, Src.Count(code, @"\bnew\s+WorkspacePilotFieldViewModel\s*\("));
        Assert.DoesNotMatch(new Regex(@"\bnew\s+WorkspacePilotFieldViewModel\b"), Src.Code(PanelModelPath));
    }
}
