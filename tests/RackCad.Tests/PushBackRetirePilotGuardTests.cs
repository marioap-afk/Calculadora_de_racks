#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4B PB-T4d — CT-PBT4D-01 (D-PBT4-10; D-S11): the code of the pilot that the panel no longer uses is retired, and what the full
/// model uses stays. Over the text of src/ without comments: the two files that are deleted do not exist, none of the eleven retired
/// declarations is declared in its file (the last two, the mode of BuildCore that never erases and its text for a view that is not
/// available, by owner-pbt4d-deadcode-27-decisions.md, pregunta 1), no file of src/ names what is retired, and the seven pieces that stay
/// are there once. It fails
/// on the RED (src/ still has the pilot) and passes on the GREEN. Its self-tests run the same reading over text injected in memory.
/// </summary>
public sealed class PushBackRetirePilotGuardTests
{
    private const string Pilot = "src/RackCad.Application/Systems/PushBack/Pilot/";

    private static readonly (string Name, string File)[] RetiredFiles =
    {
        ("PushBackPilotStateComparer.cs", Pilot + "PushBackPilotStateComparer.cs"),
        ("PushBackPilotFieldAdapter.cs", Pilot + "PushBackPilotFieldAdapter.cs"),
    };

    // The retired declarations: a name, its file and the expression of its declaration.
    private static readonly (string Name, string File, string Pattern, string Injected)[] Retired =
    {
        ("PushBackPilotMaterializer.ComposeForWrite", Pilot + "PushBackPilotMaterializer.cs", @"\bComposeForWrite\s*\(",
            "public static object ComposeForWrite(object initial, object current, object catalog) => null;"),
        ("PushBackPilotMaterializer.KeepPilotEditsOn", Pilot + "PushBackPilotMaterializer.cs", @"\bKeepPilotEditsOn\s*\(",
            "public static object KeepPilotEditsOn(object newInitial, object current) => null;"),
        ("PushBackPilotComposition", Pilot + "PushBackPilotMaterializer.cs", @"\bclass\s+PushBackPilotComposition\b",
            "public sealed class PushBackPilotComposition { }"),
        ("PushBackPilotPrepareInput", Pilot + "PushBackPilotUpdatePlan.cs", @"\bclass\s+PushBackPilotPrepareInput\b",
            "public sealed class PushBackPilotPrepareInput { }"),
        ("PushBackPilotUpdatePlanner.Prepare", Pilot + "PushBackPilotUpdatePlan.cs", @"\bpublic\s+static\s+PushBackPilotPrepareResult\s+Prepare\s*\(",
            "public static PushBackPilotPrepareResult Prepare(object input, object reread, object catalog) => null;"),
        ("PushBackPilotUpdatePlanner.Build", Pilot + "PushBackPilotUpdatePlan.cs", @"\bprivate\s+static\s+PushBackPilotPrepareResult\s+Build\s*\(",
            "private static PushBackPilotPrepareResult Build(object input, object reread, object catalog) => null;"),
        ("PushBackPilotUpdateRun.Precheck(piloto)", Pilot + "PushBackPilotUpdateRun.cs", @"\bPrecheck\s*\(\s*DraftSession\s*<\s*PushBackPilotDraftState\s*>",
            "public static PushBackPilotPrecheck Precheck(DraftSession<PushBackPilotDraftState> draft) => null;"),
        ("PushBackPilotUpdateRun.Execute", Pilot + "PushBackPilotUpdateRun.cs", @"\bDraftApplyOutcome\s*<\s*PushBackPilotDraftState\s*>\s+Execute\s*\(",
            "public static DraftApplyOutcome<PushBackPilotDraftState> Execute(object draft, object request, object port) => null;"),
        ("PushBackPilotUpdateRun.ComparePost", Pilot + "PushBackPilotUpdateRun.cs", @"\bComparePost\s*\(",
            "public static string ComparePost(object plan, object postRead) => null;"),
        ("PushBackPilotUpdatePlanner.BuildCore(allowErase)", Pilot + "PushBackPilotUpdatePlan.cs", @"\ballowErase\b",
            "private static readonly bool allowErase = false;"),
        ("PushBackPilotUpdatePlanner.ViewUnavailable", Pilot + "PushBackPilotUpdatePlan.cs", @"\bViewUnavailable(?:Prefix|Suffix)\b",
            "public const string ViewUnavailablePrefix = \"\";"),
    };

    // A use of what is retired, as code, anywhere in src/.
    private static readonly Regex Uses = new Regex(
        @"\b(?:PushBackPilotStateComparer|PushBackPilotFieldAdapter|ComposeForWrite|KeepPilotEditsOn|PushBackPilotPrepareInput|PushBackPilotComposition|ComparePost)\b"
        + @"|\bPushBackPilotUpdatePlanner\s*\.\s*Prepare\s*\(|\bPushBackPilotUpdateRun\s*\.\s*Execute\s*\(");

    // What stays, because the full model uses it (D-PBT4-10, R-17): once each.
    private static readonly (string Name, string File, string Pattern)[] Kept =
    {
        ("PushBackPilotIntentReader.Read", Pilot + "PushBackPilotIntentReader.cs", @"\bpublic\s+static\s+PushBackPilotIntentResult\s+Read\s*\("),
        ("PushBackPilotMaterializer.Materialize", Pilot + "PushBackPilotMaterializer.cs", @"\bpublic\s+static\s+PushBackPilotMaterialization\s+Materialize\s*\("),
        ("PushBackPilotDraftState", Pilot + "PushBackPilotDraftState.cs", @"\bclass\s+PushBackPilotDraftState\b"),
        ("PushBackPilotUpdatePlanner.PrepareFull", Pilot + "PushBackPilotUpdatePlan.cs", @"\bpublic\s+static\s+PushBackPilotPrepareResult\s+PrepareFull\s*\("),
        ("PushBackPilotUpdateRun.ExecuteFull", Pilot + "PushBackPilotUpdateRun.cs", @"\bDraftApplyOutcome\s*<\s*PushBackFullDraftState\s*>\s+ExecuteFull\s*\("),
        ("PushBackPilotUpdateRun.Precheck(completo)", Pilot + "PushBackPilotUpdateRun.cs", @"\bPrecheck\s*\(\s*DraftSession\s*<\s*PushBackFullDraftState\s*>"),
        ("PushBackPilotUpdateRun.ComparePostFull", Pilot + "PushBackPilotUpdateRun.cs", @"\bpublic\s+static\s+string\?\s+ComparePostFull\s*\("),
    };

    /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
    private static T Paso<T>(List<string> log, Func<T> step)
    {
        try
        {
            return step();
        }
        catch (Exception error)
        {
            log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
            return default!;
        }
    }

    /// <summary>The lines of the reading, over a source given as a function: the code of a file without comments, or null when it does not exist.</summary>
    private static List<string> Lines(Func<string, string?> code, IReadOnlyList<(string Path, string Code)> product)
    {
        var lines = new List<string>();
        foreach (var (name, file) in RetiredFiles) lines.Add("retirado-archivo " + name + " existe=" + (code(file) != null));
        foreach (var (name, file, pattern, _) in Retired) lines.Add("retirado " + name + "=" + Src.Count(code(file) ?? string.Empty, pattern));
        var naming = product.Where(f => Uses.IsMatch(f.Code)).Select(f => f.Path.Substring(f.Path.LastIndexOf('/') + 1)).ToArray();
        lines.Add("src-nombra-lo-retirado archivos=[" + string.Join(",", naming) + "]");
        foreach (var (name, file, pattern) in Kept) lines.Add("se-queda " + name + "=" + Src.Count(code(file) ?? string.Empty, pattern));
        return lines;
    }

    private static string? Read(string file) => Src.Exists(file) ? Src.Code(file) : null;

    [Fact]
    public void CT_PBT4D_01_TheCodeOfThePilotThatThePanelNoLongerUsesIsRetiredAndWhatTheFullModelUsesStays()
    {
        var log = new List<string>();
        var product = Paso(log, () => Src.AllProduct()) ?? Array.Empty<(string, string)>();
        var real = Paso(log, () => Lines(Read, product)) ?? new List<string>();
        log.AddRange(real);

        // The self-tests: each one makes the reading see what is retired again, in memory, and the lines must differ from the real ones.
        foreach (var (name, file) in RetiredFiles)
        {
            var injected = Paso(log, () => Lines(f => f == file ? "public sealed class Injected { }" : Read(f), product));
            log.Add("autoprueba-archivo-" + name + "=" + (injected != null && !injected.SequenceEqual(real) ? "falla" : "pasa"));
        }

        foreach (var (name, file, _, text) in Retired)
        {
            var injected = Paso(log, () => Lines(f => f == file ? (Read(f) ?? string.Empty) + "\n" + text + "\n" : Read(f), product));
            log.Add("autoprueba-" + name + "=" + (injected != null && !injected.SequenceEqual(real) ? "falla" : "pasa"));
        }

        var withUse = product.Select(f => f.Path.EndsWith("/WorkspaceHostCore.cs", StringComparison.Ordinal) ? (f.Path, f.Code + "\nvar c = new PushBackPilotStateComparer();\n") : f).ToList();
        var used = Paso(log, () => Lines(Read, withUse));
        log.Add("autoprueba-uso-en-WorkspaceHostCore.cs=" + (used != null && !used.SequenceEqual(real) ? "falla" : "pasa"));

        var expected = new List<string>
        {
            "retirado-archivo PushBackPilotStateComparer.cs existe=False",
            "retirado-archivo PushBackPilotFieldAdapter.cs existe=False",
            "retirado PushBackPilotMaterializer.ComposeForWrite=0",
            "retirado PushBackPilotMaterializer.KeepPilotEditsOn=0",
            "retirado PushBackPilotComposition=0",
            "retirado PushBackPilotPrepareInput=0",
            "retirado PushBackPilotUpdatePlanner.Prepare=0",
            "retirado PushBackPilotUpdatePlanner.Build=0",
            "retirado PushBackPilotUpdateRun.Precheck(piloto)=0",
            "retirado PushBackPilotUpdateRun.Execute=0",
            "retirado PushBackPilotUpdateRun.ComparePost=0",
            "retirado PushBackPilotUpdatePlanner.BuildCore(allowErase)=0",
            "retirado PushBackPilotUpdatePlanner.ViewUnavailable=0",
            "src-nombra-lo-retirado archivos=[]",
            "se-queda PushBackPilotIntentReader.Read=1",
            "se-queda PushBackPilotMaterializer.Materialize=1",
            "se-queda PushBackPilotDraftState=1",
            "se-queda PushBackPilotUpdatePlanner.PrepareFull=1",
            "se-queda PushBackPilotUpdateRun.ExecuteFull=1",
            "se-queda PushBackPilotUpdateRun.Precheck(completo)=1",
            "se-queda PushBackPilotUpdateRun.ComparePostFull=1",
            "autoprueba-archivo-PushBackPilotStateComparer.cs=falla",
            "autoprueba-archivo-PushBackPilotFieldAdapter.cs=falla",
            "autoprueba-PushBackPilotMaterializer.ComposeForWrite=falla",
            "autoprueba-PushBackPilotMaterializer.KeepPilotEditsOn=falla",
            "autoprueba-PushBackPilotComposition=falla",
            "autoprueba-PushBackPilotPrepareInput=falla",
            "autoprueba-PushBackPilotUpdatePlanner.Prepare=falla",
            "autoprueba-PushBackPilotUpdatePlanner.Build=falla",
            "autoprueba-PushBackPilotUpdateRun.Precheck(piloto)=falla",
            "autoprueba-PushBackPilotUpdateRun.Execute=falla",
            "autoprueba-PushBackPilotUpdateRun.ComparePost=falla",
            "autoprueba-PushBackPilotUpdatePlanner.BuildCore(allowErase)=falla",
            "autoprueba-PushBackPilotUpdatePlanner.ViewUnavailable=falla",
            "autoprueba-uso-en-WorkspaceHostCore.cs=falla",
        };
        Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
    }
}
