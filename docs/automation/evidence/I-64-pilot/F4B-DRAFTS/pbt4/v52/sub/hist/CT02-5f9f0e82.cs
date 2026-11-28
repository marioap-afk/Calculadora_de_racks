#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4B PB-T4d — CT-PBT4D-02 (D-S11): no file of tests/ names what the retirement of the pilot removes, as code. The text of every
/// .cs file of the two test projects is read without comments, without string and character literals; a use of a retired name there would
/// stop compiling on the GREEN, and the GREEN can no longer touch tests/ (AUTOMATION_PLAN 16.8). It passes on the RED and on the GREEN; its
/// self-tests inject each name, as code, in the text of one file in memory.
/// </summary>
public sealed class PushBackRetirePilotCensusGuardTests
{
    private const string Sample = "tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs";

    private static readonly (string Name, string Pattern, string Injected)[] RetiredUses =
    {
        ("PushBackPilotStateComparer", @"\bPushBackPilotStateComparer\b", "var x = new PushBackPilotStateComparer();"),
        ("PushBackPilotFieldAdapter", @"\bPushBackPilotFieldAdapter\b", "var x = new PushBackPilotFieldAdapter();"),
        ("ComposeForWrite", @"\bComposeForWrite\b", "var x = PushBackPilotMaterializer.ComposeForWrite(a, b, c);"),
        ("KeepPilotEditsOn", @"\bKeepPilotEditsOn\b", "var x = PushBackPilotMaterializer.KeepPilotEditsOn(a, b);"),
        ("PushBackPilotPrepareInput", @"\bPushBackPilotPrepareInput\b", "var x = PushBackPilotPrepareInput.From(d);"),
        ("PushBackPilotComposition", @"\bPushBackPilotComposition\b", "PushBackPilotComposition x = null;"),
        ("ComparePost", @"\bComparePost\b", "var x = PushBackPilotUpdateRun.ComparePost(p, r);"),
        ("PushBackPilotUpdatePlanner.Prepare", @"\bPushBackPilotUpdatePlanner\s*\.\s*Prepare\s*\(", "var x = PushBackPilotUpdatePlanner.Prepare(i, r, c);"),
        ("PushBackPilotUpdateRun.Execute", @"\bPushBackPilotUpdateRun\s*\.\s*Execute\s*\(", "var x = PushBackPilotUpdateRun.Execute(d, r, p);"),
    };

    /// <summary>The code of a C# text without comments and without the text of string and character literals (regular, verbatim,
    /// interpolated and raw), each replaced by a space.</summary>
    private static string Mask(string text)
    {
        var s = new StringBuilder(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
            {
                while (i < text.Length && text[i] != '\n') i++;
                continue;
            }

            if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
            {
                var end = text.IndexOf("*/", i + 2, StringComparison.Ordinal);
                i = end < 0 ? text.Length : end + 2;
                s.Append(' ');
                continue;
            }

            if (c == '"' && i + 2 < text.Length && text[i + 1] == '"' && text[i + 2] == '"')
            {
                var run = 0;
                while (i + run < text.Length && text[i + run] == '"') run++;
                var close = text.IndexOf(new string('"', run), i + run, StringComparison.Ordinal);
                i = close < 0 ? text.Length : close + run;
                s.Append(' ');
                continue;
            }

            var verbatim = (c == '@' || c == '$') && i + 1 < text.Length
                && (text[i + 1] == '"' || ((text[i + 1] == '@' || text[i + 1] == '$') && i + 2 < text.Length && text[i + 2] == '"'))
                && (c == '@' || text[i + 1] == '@');
            if (verbatim)
            {
                i = text.IndexOf('"', i) + 1;
                while (i < text.Length)
                {
                    if (text[i] == '"' && i + 1 < text.Length && text[i + 1] == '"') { i += 2; continue; }
                    if (text[i] == '"') { i++; break; }
                    i++;
                }

                s.Append(' ');
                continue;
            }

            if (c == '"' || c == '\'')
            {
                i++;
                while (i < text.Length && text[i] != c && text[i] != '\n')
                {
                    if (text[i] == '\\') i++;
                    i++;
                }

                i++;
                s.Append(' ');
                continue;
            }

            s.Append(c);
            i++;
        }

        return s.ToString();
    }

    private static IReadOnlyList<(string Path, string Code)> TestFiles()
    {
        var root = Src.RepoRoot();
        return new[] { "tests/RackCad.Tests", "tests/RackCad.UI.Tests" }
            .SelectMany(folder => Directory.GetFiles(Path.Combine(root, folder.Replace('/', Path.DirectorySeparatorChar)), "*.cs", SearchOption.AllDirectories))
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) && !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
            .Select(f => (Path.GetRelativePath(root, f).Replace(Path.DirectorySeparatorChar, '/'), Mask(File.ReadAllText(f))))
            .OrderBy(f => f.Item1, StringComparer.Ordinal)
            .ToList();
    }

    private static string Naming(IEnumerable<(string Path, string Code)> files)
    {
        var hits = files.SelectMany(f => RetiredUses.Where(r => Regex.IsMatch(f.Code, r.Pattern)).Select(r => f.Path.Substring(f.Path.LastIndexOf('/') + 1) + ":" + r.Name));
        return "[" + string.Join(",", hits) + "]";
    }

    [Fact]
    public void CT_PBT4D_02_NoFileOfTheTestsNamesWhatTheRetirementOfThePilotRemoves()
    {
        var log = new List<string>();
        var files = TestFiles();
        log.Add("tests archivos-revisados>0=" + (files.Count > 0) + " nombran-lo-retirado=" + Naming(files));

        var real = Naming(files);
        foreach (var (name, _, injected) in RetiredUses)
        {
            var mutated = files.Select(f => f.Path == Sample ? (f.Path, f.Code + "\n" + Mask(injected) + "\n") : f).ToList();
            log.Add("autoprueba-" + name + "-en-WorkspacePilotPrepareFull.cs=" + (Naming(mutated) != real ? "falla" : "pasa"));
        }

        var expected = new List<string>
        {
            "tests archivos-revisados>0=True nombran-lo-retirado=[]",
            "autoprueba-PushBackPilotStateComparer-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotFieldAdapter-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-ComposeForWrite-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-KeepPilotEditsOn-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotPrepareInput-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotComposition-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-ComparePost-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotUpdatePlanner.Prepare-en-WorkspacePilotPrepareFull.cs=falla",
            "autoprueba-PushBackPilotUpdateRun.Execute-en-WorkspacePilotPrepareFull.cs=falla",
        };
        Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
    }
}
