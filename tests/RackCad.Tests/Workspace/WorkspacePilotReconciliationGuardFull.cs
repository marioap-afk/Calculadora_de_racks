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
public class FullPanelCopyWorkspacePilotReconciliationGuardGreenTests
{
    private const string UiFolder = "src/RackCad.UI/";
    private const string WorkspaceUiFolder = "src/RackCad.UI/Workspace/";
    private const string FieldPath = WorkspaceUiFolder + "WorkspacePilotFieldViewModel.cs";
    private const string BrowserPath = WorkspaceUiFolder + "WorkspaceBrowserViewModel.cs";

    // One Replace of the real text: it asserts that the replaced text is there, and that the result is another text.
    private static string Mutate(string real, string from, string to)
    {
        Assert.Contains(from, real, StringComparison.Ordinal);
        var mutated = real.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(real, mutated);
        return mutated;
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

            // M-4 (correction): the form of the invocation, and not only the token. A capture as a method group (for example into a field)
            // names the token once and never invokes it in this form, so it is seen.
            var invocation = @"\b_port\s*\.\s*" + token + @"\s*\(\s*request\s*\)";

            var fieldCode = field[0].Code;
            if (Src.Count(fieldCode, pattern) != 1) yield return "the field view-model does not name " + token + " exactly once";
            if (Src.Count(fieldCode, invocation) != 1) yield return "the field view-model does not invoke _port." + token + "(request) exactly once";
            if (!Regex.IsMatch(fieldCode, @"public\s+void\s+" + body + @"\s*\(\s*\)")) yield return "the field view-model has no " + body;
            else
            {
                var gesture = Src.BodyOf(fieldCode, @"public\s+void\s+" + body + @"\s*\(\s*\)");
                if (Src.Count(gesture, pattern) != 1) yield return token + " is not inside " + body;
                if (Src.Count(gesture, invocation) != 1) yield return "_port." + token + "(request) is not invoked inside " + body;
            }
        }
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
}
