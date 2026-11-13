#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Sdk;
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

    // A declaration has a type right before the name: a word, with generic arguments closed by '>', array brackets or a nullable mark. A call has
    // an operator, a bracket, a semicolon or a statement keyword. The token must carry at least one character of a word: the '>' of "=>" or of
    // "a > Name(" is none (I-64 F4-T3C1, M-4).
    private static bool IsDeclaration(string code, int index)
    {
        var before = code.Substring(0, index).TrimEnd();
        var match = Regex.Match(before, @"([\w>]+(?:\[\])*\??)$");
        return match.Success && Regex.IsMatch(match.Groups[1].Value, @"\w") && !NotATypeBeforeAName.Contains(match.Groups[1].Value);
    }

    // The seven cases of M-4: four declarations that carry a type of every shape, and the three calls that an operator precedes.
    [Fact]
    public void TheGuardOfTheCallsTellsADeclarationFromACallInTheSevenCasesOfTheTypesAndTheOperators()
    {
        Assert.Equal(0, CallCount("class C { string[] Name(int a) { } }", "Name"));
        Assert.Equal(0, CallCount("class C { List<int> Name(int a) { } }", "Name"));
        Assert.Equal(0, CallCount("class C { int? Name(int a) { } }", "Name"));
        Assert.Equal(0, CallCount("class C { private void Name(int a) { } }", "Name"));
        Assert.Equal(1, CallCount("class C { void M() { var f = x => Name(x); } }", "Name"));
        Assert.Equal(1, CallCount("class C { void M() { var v = c ? Name(1) : 2; } }", "Name"));
        Assert.Equal(1, CallCount("class C { void M() { var v = a > Name(1); } }", "Name"));

        // The keywords and the brackets of a statement keep being calls, and a qualified call is one whatever precedes the dot.
        Assert.Equal(1, CallCount("class C { void M() { return Name(1); } }", "Name"));
        Assert.Equal(1, CallCount("class C { void M() { if (a) Name(1); } }", "Name"));
        Assert.Equal(1, CallCount("class C { void M() { x.Name(1); } }", "Name"));
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

    // Each guard of this section is a function of a text that asserts, run on the real source and, with its self-test, on a synthetic text that
    // it accepts and on the mutations that it must reject (the Assert of xUnit throws a XunitException).
    private static void Rejects(Action check) => Assert.ThrowsAny<XunitException>(check);

    private static void CheckKeepActivePassed(string code)
    {
        Assert.Matches(@"\.ApplySelectionContext\s*\(\s*\w+\s*,\s*keepActive\s*\)", code);
        Assert.Equal(1, CallCount(code, "ApplySelectionContext"));
    }

    [Fact]
    public void TheBrowserViewModelPassesKeepActiveToApplySelectionContext()
    {
        CheckKeepActivePassed(Browser());
    }

    [Fact]
    public void TheGuardOfKeepActiveSeesAMissingArgumentAnotherArgumentAndASecondCallOrNone()
    {
        const string good = "class C { void M() { var change = _current.ApplySelectionContext(context, keepActive); } }";

        CheckKeepActivePassed(good);
        Rejects(() => CheckKeepActivePassed("class C { void M() { var change = _current.ApplySelectionContext(context); } }"));
        Rejects(() => CheckKeepActivePassed("class C { void M() { var change = _current.ApplySelectionContext(context, false); } }"));
        Rejects(() => CheckKeepActivePassed("class C { void M() { var change = _current.ApplySelectionContext(context, keepActive); _current.ApplySelectionContext(context, keepActive); } }"));
        Rejects(() => CheckKeepActivePassed("class C { void M() { } }"));
    }

    private static void CheckSessionSignature(string code)
    {
        Assert.Matches(@"public\s+BrowserChange\s+ApplySelectionContext\s*\(\s*SelectionContext\s+context\s*,\s*bool\s+keepActive\s*=\s*false\s*\)", code);
        Assert.Equal(1, Src.Count(code, @"\bApplySelectionContext\s*\("));
        Assert.DoesNotMatch(new Regex(@"\bApplySelectionContext\s*\(\s*SelectionContext\s+context\s*\)"), code);
    }

    [Fact]
    public void ApplySelectionContextHasTheExactSignatureWithTheOneOptionalParameterAndNoOverload()
    {
        CheckSessionSignature(Src.Code(SessionPath));
    }

    [Fact]
    public void TheGuardOfTheSignatureSeesTheMissingDefaultTheOverloadAndAnotherReturnType()
    {
        const string good = "public sealed class S { public BrowserChange ApplySelectionContext(SelectionContext context, bool keepActive = false) { return BrowserChange.None; } }";

        CheckSessionSignature(good);
        Rejects(() => CheckSessionSignature(good.Replace("bool keepActive = false", "bool keepActive", StringComparison.Ordinal)));
        Rejects(() => CheckSessionSignature(good.Replace("bool keepActive = false", "bool keepActive = true", StringComparison.Ordinal)));
        Rejects(() => CheckSessionSignature(good.Replace("public BrowserChange", "public void", StringComparison.Ordinal)));
        Rejects(() => CheckSessionSignature(good.Replace("} }", "} public BrowserChange ApplySelectionContext(SelectionContext context) { return BrowserChange.None; } }", StringComparison.Ordinal)));
        Rejects(() => CheckSessionSignature("public sealed class S { }"));
    }

    // The words that the guards of the session forbid in its code; the text of this task uses neutral words only.
    private static readonly Regex SessionVocabulary = new(@"Draft|\bDirty\b|\bEditor\b|Subscription|Commit\(|Transaction|Provider|Metric", RegexOptions.IgnoreCase);

    private static readonly Regex SessionStatic = new(@"\bstatic\b|\block\b|\bAction\b|\bFunc<|\bDateTime\b");

    private static void CheckSessionVocabulary(string code)
    {
        Assert.DoesNotMatch(SessionVocabulary, code);
        Assert.DoesNotMatch(SessionStatic, code);
        // No type is added: the file declares the same four types as before (two enums, one record and the session).
        Assert.Equal(4, Src.Count(code, @"\b(?:enum|record|class)\s+\w+"));
    }

    [Fact]
    public void BrowserSessionKeepsTheVocabularyOfItsGuardsAndAddsNoTypeNorValue()
    {
        CheckSessionVocabulary(Src.Code(SessionPath));
        Assert.Equal(new[] { "None", "Activated", "Created", "Replaced", "Pinned", "Closed", "SummaryApplied" }, Enum.GetNames<RackCad.Application.Workspace.BrowserChange>());
    }

    [Fact]
    public void TheGuardOfTheVocabularySeesAForbiddenWordAStaticMemberAndAFifthType()
    {
        const string good = "enum A { } enum B { } record C(int X); class D { }";

        CheckSessionVocabulary(good);
        Rejects(() => CheckSessionVocabulary(good.Replace("class D { }", "class D { int DraftCount; }", StringComparison.Ordinal)));
        Rejects(() => CheckSessionVocabulary(good.Replace("class D { }", "class D { int Dirty; }", StringComparison.Ordinal)));
        Rejects(() => CheckSessionVocabulary(good.Replace("class D { }", "class D { Commit(); }", StringComparison.Ordinal)));
        Rejects(() => CheckSessionVocabulary(good.Replace("class D { }", "class D { static int x; }", StringComparison.Ordinal)));
        Rejects(() => CheckSessionVocabulary(good.Replace("class D { }", "class D { object gate; void M() { lock (gate) { } } }", StringComparison.Ordinal)));
        Rejects(() => CheckSessionVocabulary(good.Replace("class D { }", "class D { Action a; }", StringComparison.Ordinal)));
        Rejects(() => CheckSessionVocabulary(good.Replace("class D { }", "class D { DateTime t; }", StringComparison.Ordinal)));
        Rejects(() => CheckSessionVocabulary(good + " class E { }"));
    }

    // ------------------------------------------------------------------ the shape of the UI: the view-model and the record

    private static readonly string[] PublicSignatures =
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
    };

    private static void CheckPublicMembers(string code, string fieldCode)
    {
        Assert.Matches(@"public\s+sealed\s+record\s+WorkspaceBrowserNoticeItem\s*\(\s*string\s+Key\s*,\s*string\s+Text\s*,\s*bool\s+CanDiscardDocument\s*\)\s*;", code);
        foreach (var signature in PublicSignatures)
            Assert.Equal(1, Src.Count(code, signature));

        // The record of the tab is the one of F3 and the field gains only the discard button.
        Assert.Matches(@"public\s+sealed\s+record\s+WorkspaceBrowserTabItem\s*\(\s*string\s+RackId\s*,\s*string\s+Title\s*,\s*bool\s+IsActive\s*,\s*bool\s+IsPreview\s*\)\s*;", code);
        Assert.Equal(1, Src.Count(fieldCode, @"public\s+bool\s+ShowDiscardButton\s*=>"));
    }

    private const string GoodMembers =
        "public sealed record WorkspaceBrowserNoticeItem(string Key, string Text, bool CanDiscardDocument); "
        + "public sealed record WorkspaceBrowserTabItem(string RackId, string Title, bool IsActive, bool IsPreview); "
        + "public sealed class WorkspaceBrowserViewModel { public void CloseTab(string rackId) { } public void DiscardField() { } public void RequestDocumentDiscard() { } "
        + "public void ConfirmDiscard() { } public void CancelDiscard() { } public void DismissNotice(string key) { } public bool CanDiscardDocument => true; "
        + "public bool HasConfirmation => true; public string ConfirmationText => \"\"; public string NoticeText => \"\"; "
        + "public IReadOnlyList<WorkspaceBrowserNoticeItem> Notices => null; public bool HasNotices => true; }";

    private const string GoodFieldMembers = "public sealed class WorkspacePilotFieldViewModel { public bool ShowDiscardButton => true; }";

    [Fact]
    public void TheBrowserViewModelGainsExactlyThePublicMembersOfTheContractAndTheNoticeRecord()
    {
        CheckPublicMembers(Browser(), Src.Code(UiWorkspace + "WorkspacePilotFieldViewModel.cs"));
    }

    [Fact]
    public void TheGuardOfThePublicMembersSeesAMissingMemberAChangedSignatureARepeatedOneAndAChangedRecord()
    {
        CheckPublicMembers(GoodMembers, GoodFieldMembers);

        foreach (var (before, after) in new[]
                 {
                     ("public void CancelDiscard() { }", "internal void CancelDiscard() { }"),
                     ("public void DismissNotice(string key) { }", "public void DismissNotice(int key) { }"),
                     ("public void CloseTab(string rackId) { }", "public void CloseTab(string rackId) { } public void CloseTab(string rackId) { }"),
                     ("public bool HasNotices => true;", "public int HasNotices => 1;"),
                     ("public string NoticeText => \"\";", "public string NoticeText { get; } = \"\";"),
                     ("bool CanDiscardDocument); ", "bool CanDiscard); "),
                     ("string Title, bool IsActive", "string Title, bool IsOn"),
                 })
        {
            Assert.Contains(before, GoodMembers);
            Rejects(() => CheckPublicMembers(GoodMembers.Replace(before, after, StringComparison.Ordinal), GoodFieldMembers));
        }

        Rejects(() => CheckPublicMembers(GoodMembers, "public sealed class WorkspacePilotFieldViewModel { }"));
        Rejects(() => CheckPublicMembers(GoodMembers, GoodFieldMembers + " " + GoodFieldMembers));
    }

    // The public shape of the browser view-model, fixed by the rule of extraction of the package (I-64 F4-T3C1, M-5): the lines that start with
    // public in the body of the class, without the body of StoredNotice and without the class line (the two records are outside the class),
    // each one cut at the first => or {, with the spaces collapsed in one and trimmed. A later change of this surface will be declared.
    private static readonly string[] PublicShape =
    {
        "public WorkspaceBrowserViewModel(IWorkspaceBrowserSummaryPort port, Action tabChanged = null, IWorkspacePilotEditPort pilotEdit = null)",
        "public WorkspacePilotFieldViewModel Field",
        "public bool IsAvailable",
        "public IReadOnlyList<WorkspaceBrowserTabItem> Tabs",
        "public bool HasTabs",
        "public string ActiveRackId",
        "public int LiveSurfaceCount",
        "public bool HasSurface",
        "public string EmptyText",
        "public string SurfaceTitle",
        "public string SurfaceKindText",
        "public string SurfaceViewsText",
        "public string SurfaceStatusText",
        "public bool CanDiscardDocument",
        "public bool HasConfirmation",
        "public string ConfirmationText",
        "public string NoticeText",
        "public IReadOnlyList<WorkspaceBrowserNoticeItem> Notices",
        "public bool HasNotices",
        "public void Present(SessionId? session, SelectionContext context, bool canReadDrawing, WorkspaceActionBlock block, string blockText)",
        "public void ActivateTab(string rackId)",
        "public void PinTab(string rackId)",
        "public void CloseTab(string rackId)",
        "public void DiscardField()",
        "public void RequestDocumentDiscard()",
        "public void ConfirmDiscard()",
        "public void CancelDiscard()",
        "public void DismissNotice(string key)",
        "public void BeginFieldEdit()",
        "public void BeginFieldEditFromBox()",
        "public void BeginFieldEditFromTab(string rackId)",
        "public void ReloadField()",
        "public void ReloadSummary()",
    };

    private static IReadOnlyList<string> ExtractPublicShape(string code)
    {
        var classSignature = @"public\s+sealed\s+class\s+WorkspaceBrowserViewModel\b";
        Assert.Matches(classSignature, code);
        var body = Src.BodyOf(code, classSignature);

        var stored = @"private\s+sealed\s+class\s+StoredNotice\b";
        if (Regex.IsMatch(body, stored)) body = body.Replace(Src.BodyOf(body, stored), string.Empty, StringComparison.Ordinal);

        var shape = new List<string>();
        foreach (var line in body.Split('\n'))
        {
            if (!Regex.IsMatch(line, @"^\s*public\b") || Regex.IsMatch(line, @"^\s*public\s+sealed\s+class\s+WorkspaceBrowserViewModel\b")) continue;

            var cut = Regex.Match(line, @"=>|\{");
            var text = cut.Success ? line.Substring(0, cut.Index) : line;
            shape.Add(Regex.Replace(text, @"\s+", " ").Trim());
        }

        return shape;
    }

    private static void CheckPublicShape(string code)
    {
        var shape = ExtractPublicShape(code);

        Assert.Equal(33, PublicShape.Length);
        Assert.Equal(33, PublicShape.Distinct().Count());
        Assert.Equal(33, shape.Count);
        Assert.True(new HashSet<string>(shape, StringComparer.Ordinal).SetEquals(PublicShape), string.Join(" | ", shape.Except(PublicShape).Concat(PublicShape.Except(shape))));
    }

    [Fact]
    public void ThePublicShapeOfTheBrowserViewModelIsExactlyTheOneOf33Signatures()
    {
        CheckPublicShape(Browser());
    }

    [Fact]
    public void TheGuardOfThePublicShapeSeesAnAddedAChangedAndARemovedMemberAndIgnoresTheStoredNotice()
    {
        var shape = string.Join("\n", PublicShape.Select(s => "    " + s + (s.Contains('(') ? " { }" : " { get; }")));
        var good = "public sealed record R(int X);\npublic sealed class WorkspaceBrowserViewModel : ObservableObject\n{\n"
            + "    private sealed class StoredNotice\n    {\n        public StoredNotice(string key) { }\n        public string Key { get; }\n    }\n"
            + shape + "\n}\n";

        CheckPublicShape(good);
        Rejects(() => CheckPublicShape(good.Replace("    public void ReloadField() { }", "    public void ReloadField() { }\n    public void Extra() { }", StringComparison.Ordinal)));
        Rejects(() => CheckPublicShape(good.Replace("    public void ReloadField() { }", "    internal void ReloadField() { }", StringComparison.Ordinal)));
        Rejects(() => CheckPublicShape(good.Replace("    public void PinTab(string rackId) { }", "    public void PinTab(int rackId) { }", StringComparison.Ordinal)));
        Rejects(() => CheckPublicShape(good.Replace("    public string NoticeText { get; }", "    public string NoticeText { get; }\n    public string Another { get; }", StringComparison.Ordinal)));
        Rejects(() => CheckPublicShape(good.Replace("    public bool HasNotices { get; }\n", string.Empty, StringComparison.Ordinal)));
        Rejects(() => CheckPublicShape(good.Replace("public string Key { get; }", "public string Key { get; }\n        public string Other { get; }", StringComparison.Ordinal).Replace("private sealed class StoredNotice", "public sealed class StoredNotice", StringComparison.Ordinal)));
        Rejects(() => CheckPublicShape("public sealed class Another { }"));
    }

    // Texts are composed with a = a + b: no += and no -= anywhere, and nothing that schedules.
    private static readonly Regex Subscriptions = new(@"\+=|-=|\bevent\b|\bdelegate\b|\bstatic\s+(?!readonly\s+string\b)\w+\s+\w+\s*;");

    private static readonly Regex Scheduling = new(@"\bTask\b|\basync\b|\bawait\b|\bDispatcher\b|\w*Timer\b|\bThread\b|\.Result\b|\.Wait\s*\(");

    private static void CheckNoSubscriptions(string code)
    {
        Assert.DoesNotMatch(Subscriptions, code);
        Assert.DoesNotMatch(Scheduling, code);
    }

    // The browser still asks the port for the liveness of a session and for a summary exactly once, and holds one registry.
    private static void CheckSingleQueries(string browser)
    {
        Assert.Equal(1, Src.Count(browser, @"\.IsSessionAlive\s*\("));
        Assert.Equal(1, Src.Count(browser, @"\.ReadSummary\s*\("));
        Assert.Equal(1, Src.Count(browser, @"\bBrowserSessionRegistry\b"));
        // The purge of the notices of veto is in the same loop of PurgeDeadSessions.
        var purge = RequiredBody(browser, "PurgeDeadSessions");
        Assert.Equal(1, Src.Count(purge, @"\.IsSessionAlive\s*\("));
        Assert.Matches(@"RemoveAll", purge);
    }

    [Fact]
    public void TheBrowserAndTheFieldViewModelsAddNoSubscriptionNoEventAndNoStaticState()
    {
        foreach (var code in new[] { Browser(), Src.Code(UiWorkspace + "WorkspacePilotFieldViewModel.cs") })
            CheckNoSubscriptions(code);

        CheckSingleQueries(Browser());
    }

    private const string GoodQueries =
        "class C { private BrowserSessionRegistry _registry; void PurgeDeadSessions() { if (_port.IsSessionAlive(s)) continue; _stored.RemoveAll(n => n.IsLoss); } "
        + "void Materialize() { _port.ReadSummary(r, k, u); } }";

    [Fact]
    public void TheGuardOfTheQueriesSeesASecondLivenessCheckASecondSummaryAndAPurgeWithoutItsLoop()
    {
        CheckSingleQueries(GoodQueries);
        Rejects(() => CheckSingleQueries(GoodQueries.Replace("void Materialize() {", "void Materialize() { _port.IsSessionAlive(s);", StringComparison.Ordinal)));
        Rejects(() => CheckSingleQueries(GoodQueries.Replace("void Materialize() {", "void Materialize() { _port.ReadSummary(r, k, u);", StringComparison.Ordinal)));
        Rejects(() => CheckSingleQueries(GoodQueries + " class D { private BrowserSessionRegistry _other; }"));
        Rejects(() => CheckSingleQueries(GoodQueries.Replace("if (_port.IsSessionAlive(s)) continue; ", string.Empty, StringComparison.Ordinal)));
        Rejects(() => CheckSingleQueries(GoodQueries.Replace("_stored.RemoveAll(n => n.IsLoss);", string.Empty, StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("var a = x; a += y;")]
    [InlineData("text -= y;")]
    [InlineData("public event Action Changed;")]
    [InlineData("Action a = delegate { };")]
    [InlineData("private static int _count;")]
    [InlineData("public static string Name;")]
    [InlineData("Task.Run(() => 1);")]
    [InlineData("public async void Go() { }")]
    [InlineData("await Go();")]
    [InlineData("Dispatcher.Invoke(a);")]
    [InlineData("var t = new DispatcherTimer();")]
    [InlineData("var t = new System.Threading.Timer(a);")]
    [InlineData("var t = new Thread(a);")]
    [InlineData("var v = task.Result;")]
    [InlineData("task.Wait();")]
    public void TheGuardOfTheSubscriptionsSeesTheComposedTextTheEventsTheStaticStateAndWhateverSchedules(string code)
    {
        Rejects(() => CheckNoSubscriptions(code));
    }

    [Theory]
    [InlineData("var a = x; a = a + y;")]
    [InlineData("private static readonly string Key = \"k\";")]
    [InlineData("var done = Completed(a);")]
    [InlineData("var results = list.ToArray();")]
    public void TheGuardOfTheSubscriptionsAcceptsTheComposedTextWithAPlusAndAConstantText(string code)
    {
        CheckNoSubscriptions(code);
    }

    private const string PanelSignature =
        @"public\s+WorkspacePanelViewModel\s*\(\s*IWorkspaceHostPort\s+\w+\s*,\s*Action<bool>\s+\w+\s*,\s*IWorkspaceNavigationPort\s+\w+\s*=\s*null\s*,\s*IWorkspaceBrowserSummaryPort\s+browserSummary\s*=\s*null\s*,\s*IWorkspacePilotEditPort\s+pilotEdit\s*=\s*null\s*\)";

    private static void CheckPanelViewModel(string code, string text)
    {
        Assert.Matches(PanelSignature, code);
        Assert.Matches(@"new\s+WorkspaceBrowserViewModel\s*\(\s*browserSummary\s*,\s*RepresentNavigation\s*,\s*pilotEdit\s*\)", code);
        Assert.DoesNotMatch(new Regex(@"\.TakeCloseNotices|\.TakeLossNotices|\.DiscardRack|\.DiscardDocument|\.GetRacksAtRisk"), code);
        Assert.DoesNotContain("Building the panel never calls it", text);
    }

    [Fact]
    public void ThePanelViewModelOnlyChangesTheDocumentationOfPilotEdit()
    {
        CheckPanelViewModel(Src.Code(UiWorkspace + "WorkspacePanelViewModel.cs"), Src.Text(UiWorkspace + "WorkspacePanelViewModel.cs"));
    }

    private const string GoodPanelCode =
        "public sealed class P { public WorkspacePanelViewModel(IWorkspaceHostPort port, Action<bool> set, IWorkspaceNavigationPort navigation = null, "
        + "IWorkspaceBrowserSummaryPort browserSummary = null, IWorkspacePilotEditPort pilotEdit = null) { Browser = new WorkspaceBrowserViewModel(browserSummary, RepresentNavigation, pilotEdit); } }";

    [Fact]
    public void TheGuardOfThePanelViewModelSeesAChangedSignatureAChangedConstructionACallOfTheQueuesAndTheOldPhrase()
    {
        CheckPanelViewModel(GoodPanelCode, "/// it is asked only when the user clicks");
        Rejects(() => CheckPanelViewModel(GoodPanelCode.Replace("IWorkspacePilotEditPort pilotEdit = null", "IWorkspacePilotEditPort pilotEdit", StringComparison.Ordinal), string.Empty));
        Rejects(() => CheckPanelViewModel(GoodPanelCode.Replace("Action<bool> set", "Action<int> set", StringComparison.Ordinal), string.Empty));
        Rejects(() => CheckPanelViewModel(GoodPanelCode.Replace("RepresentNavigation, pilotEdit", "null, pilotEdit", StringComparison.Ordinal), string.Empty));
        Rejects(() => CheckPanelViewModel(GoodPanelCode.Replace("{ Browser =", "{ pilotEdit.TakeCloseNotices(); Browser =", StringComparison.Ordinal), string.Empty));
        Rejects(() => CheckPanelViewModel(GoodPanelCode.Replace("{ Browser =", "{ pilotEdit.TakeLossNotices(); Browser =", StringComparison.Ordinal), string.Empty));
        Rejects(() => CheckPanelViewModel(GoodPanelCode.Replace("{ Browser =", "{ pilotEdit.DiscardRack(o, r); Browser =", StringComparison.Ordinal), string.Empty));
        Rejects(() => CheckPanelViewModel(GoodPanelCode.Replace("{ Browser =", "{ pilotEdit.DiscardDocument(o); Browser =", StringComparison.Ordinal), string.Empty));
        Rejects(() => CheckPanelViewModel(GoodPanelCode.Replace("{ Browser =", "{ pilotEdit.GetRacksAtRisk(o); Browser =", StringComparison.Ordinal), string.Empty));
        Rejects(() => CheckPanelViewModel(GoodPanelCode, "/// Building the panel never calls it; the user does."));
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
        CheckNewXamlElements(Xaml());
    }

    private static void CheckNewXamlElements(string xaml)
    {
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

    // The mutations of the real XAML that the guard must see: a handler, an automation name, a content, a binding, a name and a placement.
    [Theory]
    [InlineData("Click=\"OnBrowserConfirmDiscardClick\"", "Click=\"OnSomethingElseClick\"")]
    [InlineData("Click=\"OnBrowserCancelDiscardClick\"", "Click=\"OnSomethingElseClick\"")]
    [InlineData("Click=\"OnBrowserDiscardDocumentClick\"", "Click=\"OnSomethingElseClick\"")]
    [InlineData("Click=\"OnBrowserNoticeDismissClick\"", "Click=\"OnSomethingElseClick\"")]
    [InlineData("Click=\"OnBrowserNoticeDiscardDocumentClick\"", "Click=\"OnSomethingElseClick\"")]
    [InlineData("Click=\"OnBrowserFieldDiscardClick\"", "Click=\"OnSomethingElseClick\"")]
    [InlineData("AutomationProperties.Name=\"Cancelar el descarte\"", "AutomationProperties.Name=\"Cancelar\"")]
    [InlineData("AutomationProperties.Name=\"Confirmar el descarte\"", "AutomationProperties.Name=\"Confirmar\"")]
    [InlineData("AutomationProperties.Name=\"Quitar el aviso\"", "AutomationProperties.Name=\"Quitar\"")]
    [InlineData("AutomationProperties.Name=\"Descartar el borrador\"", "AutomationProperties.Name=\"Descartar\"")]
    [InlineData("Content=\"Cancelar\"", "Content=\"Salir\"")]
    [InlineData("Content=\"Entendido\"", "Content=\"Vale\"")]
    [InlineData("{Binding Browser.CanDiscardDocument", "{Binding Browser.Another")]
    [InlineData("{Binding Browser.HasConfirmation", "{Binding Browser.Another")]
    [InlineData("{Binding Browser.ConfirmationText}", "{Binding Browser.Another}")]
    [InlineData("{Binding Browser.Field.ShowDiscardButton", "{Binding Browser.Field.Another")]
    [InlineData("x:Name=\"BrowserNoticeList\"", "x:Name=\"BrowserNoticeList2\"")]
    [InlineData("x:Name=\"BrowserConfirmationPanel\"", "x:Name=\"BrowserConfirmationPanel2\"")]
    [InlineData("x:Name=\"BrowserFieldDiscardButton\"", "x:Name=\"BrowserFieldDiscardButton2\"")]
    public void TheGuardOfTheNewXamlElementsSeesAChangedHandlerAutomationNameContentBindingOrName(string from, string to)
    {
        var xaml = Xaml();
        Assert.Contains(from, xaml);

        Rejects(() => CheckNewXamlElements(xaml.Replace(from, to, StringComparison.Ordinal)));
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
        CheckSurfaceButtons(Xaml());
    }

    private static void CheckSurfaceButtons(string xaml)
    {
        var name = xaml.IndexOf("x:Name=\"BrowserSurface\"", StringComparison.Ordinal);
        Assert.True(name >= 0, "the XAML has no surface");
        var start = xaml.LastIndexOf("<Border", name, StringComparison.Ordinal);
        var end = xaml.IndexOf("Text=\"Navegación\"", name, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "the surface is not where it should be");
        var surface = xaml.Substring(start, end - start);

        Assert.Equal(1, Src.Count(surface, "Content=\"Editar\""));
        Assert.Equal(1, Src.Count(surface, "Content=\"Recargar\""));
        Assert.Equal(1, Src.Count(surface, "Content=\"Volver a leer\""));
        Assert.Equal(1, Src.Count(surface, "Content=\"Descartar\""));
    }

    [Theory]
    [InlineData("Content=\"Editar\"", "Content=\"Editar\" /><Button Content=\"Editar\"")]
    [InlineData("Content=\"Recargar\"", "Content=\"Recargar2\"")]
    [InlineData("Content=\"Volver a leer\"", "Content=\"Descartar\"")]
    [InlineData("Content=\"Volver a leer\"", "Content=\"Recargar\"")]
    [InlineData("Content=\"Descartar\"", "Content=\"Borrar\"")]
    [InlineData("x:Name=\"BrowserSurface\"", "x:Name=\"BrowserSurface2\"")]
    public void TheGuardOfTheButtonsOfTheSurfaceSeesARepeatedAMissingAndARenamedButton(string from, string to)
    {
        var xaml = Xaml();
        Assert.Contains(from, xaml);

        Rejects(() => CheckSurfaceButtons(xaml.Replace(from, to, StringComparison.Ordinal)));
    }

    // ------------------------------------------------------------------ the code-behind: handlers inside Contained, N-3 with Key and IsRepeat only

    [Fact]
    public void EveryNewHandlerDelegatesInsideContainedToItsMemberOfTheBrowser()
    {
        CheckHandlers(Src.Code(CodeBehindPath));
    }

    [Theory]
    [InlineData("Contained(() => _viewModel.Browser.CancelDiscard())", "_viewModel.Browser.CancelDiscard()")]
    [InlineData("_viewModel.Browser.ConfirmDiscard()", "_viewModel.Browser.ConfirmDiscard(1)")]
    [InlineData("_viewModel.Browser.DiscardField()", "_viewModel.Browser.ReloadField()")]
    [InlineData("_viewModel.Browser.DismissNotice(NoticeKeyOf(sender))", "_viewModel.Browser.DismissNotice(null)")]
    [InlineData("private void OnBrowserDiscardDocumentClick(object sender, RoutedEventArgs e)", "private void OnBrowserDiscardDocumentClick(object sender, EventArgs e)")]
    [InlineData("private void OnBrowserNoticeDiscardDocumentClick(", "private void OnBrowserNoticeDiscardDocumentClick2(")]
    public void TheGuardOfTheHandlersSeesACallOutsideContainedAnotherMemberAndAChangedSignature(string from, string to)
    {
        var code = Src.Code(CodeBehindPath);
        Assert.Contains(from, code);

        Rejects(() => CheckHandlers(code.Replace(from, to, StringComparison.Ordinal)));
    }

    private static void CheckHandlers(string code)
    {
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
        Assert.DoesNotMatch(KeyHandlerBad, code);
        Assert.DoesNotMatch(new Regex(@"\bKeyUp\b"), xaml);
        Assert.Equal(1, Src.Count(xaml, @"\bPreviewKeyDown\s*="));
    }

    // What the code-behind of the box must never use: the keyboard device, the key up, the focus and the keys of the system, the method of
    // input and the state of the key. The self-test below sees each alternative of the expression, so none can be lost without notice.
    private static readonly Regex KeyHandlerBad = new(@"e\.KeyboardDevice|\bKeyboard\b|\bKeyUp\b|\.Focus\s*\(|FocusManager|e\.SystemKey|e\.ImeProcessedKey|e\.IsDown|e\.IsToggled");

    [Theory]
    [InlineData("if (e.KeyboardDevice.IsKeyDown(Key.A)) { }")]
    [InlineData("Keyboard.Focus(box);")]
    [InlineData("box.KeyUp += OnUp;")]
    [InlineData("box.Focus();")]
    [InlineData("box.Focus ();")]
    [InlineData("FocusManager.SetFocusedElement(scope, box);")]
    [InlineData("if (e.Key == Key.System) { var k = e.SystemKey; }")]
    [InlineData("var k = e.ImeProcessedKey;")]
    [InlineData("if (e.IsDown) { }")]
    [InlineData("if (e.IsToggled) { }")]
    public void TheGuardOfTheKeyHandlerSeesEveryAlternativeOfItsExpression(string code)
    {
        Assert.Matches(KeyHandlerBad, code);
    }

    [Theory]
    [InlineData("if (e.IsRepeat && e.Key == _launchKey.Value) { e.Handled = true; }")]
    [InlineData("private Key? _launchKey;")]
    [InlineData("var focused = FocusedText(); var down = e.IsRepeat;")]
    public void TheGuardOfTheKeyHandlerAcceptsTheKeyAndTheRepeatFlag(string code)
    {
        Assert.DoesNotMatch(KeyHandlerBad, code);
    }

    // ------------------------------------------------------------------ the limits of the task: nothing of the host, the port, the palette or a command changes

    [Fact]
    public void TheUiOfTheTaskReachesNeitherTheIntentionNorTheDrawingNorTheFocusNorADialog()
    {
        CheckUiReach(Browser(), new[] { Browser(), Src.Code(CodeBehindPath) });
    }

    private static readonly Regex IntentionReads = new(@"\b(?:TryBeginEdit|TryReload|TryCheckNow)\b");

    private static readonly Regex DrawingReach = new(@"\bTransaction|\bDatabase\b|\.Commit\s*\(|SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync|Autodesk");

    private static readonly Regex FocusReach = new(@"\bKeyboard|FocusManager|SetFocus|\.Focus\s*\(|InputManager");

    private static readonly Regex DialogReach = new(@"EnterPanelModal|ShowModalWindow|ShowDialog|MessageBox|\bRegistry\b|\bRackLog\b");

    private static void CheckUiReach(string browser, IEnumerable<string> codes)
    {
        // The browser view-model names none of the three reads; the one that gestures of the field own stays in the field.
        Assert.DoesNotMatch(IntentionReads, browser);
        foreach (var code in codes)
        {
            Assert.DoesNotMatch(DrawingReach, code);
            Assert.DoesNotMatch(FocusReach, code);
            Assert.DoesNotMatch(DialogReach, code);
        }
    }

    [Theory]
    [InlineData("_p.TryBeginEdit(r);", "intention")]
    [InlineData("_p.TryReload(r);", "intention")]
    [InlineData("_p.TryCheckNow(r);", "intention")]
    [InlineData("var t = new Transaction();", "drawing")]
    [InlineData("var d = HostApplicationServices.WorkingDatabase; Database db;", "drawing")]
    [InlineData("tr.Commit();", "drawing")]
    [InlineData("doc.SendStringToExecute(\"X\", true, false, false);", "drawing")]
    [InlineData("[CommandMethod(\"X\")]", "drawing")]
    [InlineData("using Autodesk.AutoCAD.Runtime;", "drawing")]
    [InlineData("Keyboard.Focus(box);", "focus")]
    [InlineData("FocusManager.SetFocusedElement(a, b);", "focus")]
    [InlineData("InputManager.Current.ProcessInput(a);", "focus")]
    [InlineData("box.Focus();", "focus")]
    [InlineData("EnterPanelModal();", "dialog")]
    [InlineData("window.ShowDialog();", "dialog")]
    [InlineData("MessageBox.Show(\"x\");", "dialog")]
    [InlineData("var r = new Registry();", "dialog")]
    [InlineData("RackLog.Write(\"x\");", "dialog")]
    public void TheGuardOfTheReachOfTheUiSeesTheReadsTheDrawingTheFocusAndTheDialogs(string added, string kind)
    {
        const string good = "class C { void M() { var x = 1; } }";
        CheckUiReach(good, new[] { good, good });

        if (kind == "intention")
        {
            Rejects(() => CheckUiReach(good + added, new[] { good }));
            return;
        }

        // In the browser view-model and, apart, in the code-behind: both are looked at.
        Rejects(() => CheckUiReach(good, new[] { good + added, good }));
        Rejects(() => CheckUiReach(good, new[] { good, good + added }));
    }

    // ------------------------------------------------------------------ M-7: the six gestures never call ContainFailure

    private static readonly string[] SixGestures =
    {
        "CloseTab", "DiscardField", "RequestDocumentDiscard", "ConfirmDiscard", "CancelDiscard", "DismissNotice",
    };

    private static IEnumerable<string> GestureViolations(string code)
    {
        // Not vacuous: the helper that the gestures must not call exists and something calls it (Interact and ReloadSummary do).
        if (CallCount(code, "ContainFailure") == 0) yield return "nothing calls ContainFailure, so the guard would be empty";

        foreach (var gesture in SixGestures)
        {
            var body = BodyOfMember(code, gesture);
            if (body == null)
            {
                yield return "the gesture " + gesture + " does not exist";
                continue;
            }

            var calls = CallCount(body, "ContainFailure");
            if (calls != 0) yield return "the gesture " + gesture + " calls ContainFailure " + calls + " times";
        }
    }

    private const string GoodGestures =
        "class C { public void CloseTab(string r) { try { Ask(); } catch (Exception ex) { FailGesture(ex, \"a\"); } } public void DiscardField() { FailGesture(e, \"b\"); } "
        + "public void RequestDocumentDiscard() { FailGesture(e, \"c\"); } public void ConfirmDiscard() { FailGesture(e, \"d\"); } public void CancelDiscard() { RemoveConfirmation(); } "
        + "public void DismissNotice(string key) { Publish(); } private void Interact() { try { } catch (Exception ex) { ContainFailure(ex); } } private void ContainFailure(Exception e) { } }";

    [Fact]
    public void NoneOfTheSixGesturesOfTheClosingCallsContainFailure()
    {
        Assert.Empty(GestureViolations(Browser()));
        Assert.Empty(GestureViolations(GoodGestures));
    }

    [Theory]
    [InlineData("FailGesture(ex, \"a\");", "ContainFailure(ex);")]
    [InlineData("FailGesture(e, \"b\");", "this.ContainFailure(e);")]
    [InlineData("FailGesture(e, \"c\");", "if (x) ContainFailure(e);")]
    [InlineData("FailGesture(e, \"d\");", "var v = x ? ContainFailure(e) : 0;")]
    [InlineData("RemoveConfirmation();", "return ContainFailure(e);")]
    [InlineData("Publish();", "ContainFailure(e); Publish();")]
    public void TheGuardOfTheSixGesturesSeesACallToContainFailureInAnyOfThem(string from, string to)
    {
        Assert.Contains(from, GoodGestures);

        Assert.NotEmpty(GestureViolations(GoodGestures.Replace(from, to, StringComparison.Ordinal)));
    }

    [Fact]
    public void TheGuardOfTheSixGesturesIsNotVacuousAndNeedsEveryGesture()
    {
        Assert.NotEmpty(GestureViolations(GoodGestures.Replace("ContainFailure(ex);", "Nothing();", StringComparison.Ordinal).Replace("private void ContainFailure(Exception e) { }", string.Empty, StringComparison.Ordinal)));
        Assert.NotEmpty(GestureViolations(GoodGestures.Replace("public void DismissNotice(string key)", "public void Dismiss(string key)", StringComparison.Ordinal)));
        Assert.NotEmpty(GestureViolations("class C { }"));
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
