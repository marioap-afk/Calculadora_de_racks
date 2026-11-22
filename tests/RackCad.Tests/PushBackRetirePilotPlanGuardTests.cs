#nullable enable
using System;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4B PB-T4d (owner-pbt4d-list-decisions.md, pregunta 1): the equivalents of two guards of WorkspacePilotPrepareGuardTests (F4-T4A1)
// that name what the retirement of the pilot removes. Each one is its original with one declared change and nothing else:
// (1) the types of the package lose the snapshot of the pilot (PushBackPilotPrepareInput) and keep the one of the full model
//     (PushBackFullPrepareInput), with its private constructor, its one factory From over a draft of the full model and its one construction;
// (2) the order of PREPARE is read in PrepareFull, the PREPARE that stays, whose last step is BuildFull.
public class PushBackRetirePilotPlanGuardTests
{
    private const string PlanPath = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs";
    private const string PilotFolder = "src/RackCad.Application/Systems/PushBack/Pilot/";

    private static string Plan() => Src.Code(PlanPath);

    [Fact]
    public void TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage()
    {
        var text = Src.Text(PlanPath);

        Assert.StartsWith("#nullable enable", text.TrimStart((char)0xFEFF));
        var namespaces = Regex.Matches(Src.CodeOnly(text), @"^\s*namespace\s+([\w\.]+)\s*(\{|;)", RegexOptions.Multiline);
        var declared = Assert.Single(namespaces);
        Assert.Equal("RackCad.Application.Systems.PushBack.Pilot", declared.Groups[1].Value);
        Assert.Equal("{", declared.Groups[2].Value);
        Assert.Equal(PilotFolder + "PushBackPilotUpdatePlan.cs", PlanPath);
        foreach (var type in new[] { "PushBackFullPrepareInput", "PushBackPilotUpdatePlanner", "PushBackPilotUpdatePlan", "PushBackPilotPrepareResult", "PushBackPilotPrepareKind" })
            Assert.Matches(@"\b(?:class|enum)\s+" + type + @"\b", Plan());
        // The one construction of the snapshot is its factory: the constructor is private.
        Assert.Matches(@"private\s+PushBackFullPrepareInput\s*\(", Plan());
        Assert.Matches(@"public\s+static\s+PushBackFullPrepareInput\s+From\s*\(\s*DraftSession<PushBackFullDraftState>\s+draft\s*\)", Plan());
        Assert.Equal(1, Src.Count(Plan(), @"\bnew\s+PushBackFullPrepareInput\s*\("));
    }

    [Fact]
    public void TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread()
    {
        var plan = Plan();
        var prepare = Src.BodyOf(plan, @"public\s+static\s+PushBackPilotPrepareResult\s+PrepareFull\s*\(");

        var order = new[]
        {
            "PushBackPilotPrepareKind.NotApplicable",
            "PushBackPilotPrepareKind.PendingText",
            "PushBackPilotPrepareKind.NothingToApply",
            ".CompareWith(",
            "WithConflict(",
            "reread.Read.IsAdmitted",
            "WithNotAdmitted(",
            "BuildFull(",
        };
        var previous = -1;
        foreach (var needle in order)
        {
            var at = prepare.IndexOf(needle, StringComparison.Ordinal);
            Assert.True(at > previous, "out of order or missing: " + needle);
            previous = at;
        }

        // The re-read is not named before NothingToApply: it is not looked at for a clean draft.
        var nothing = prepare.IndexOf("PushBackPilotPrepareKind.NothingToApply", StringComparison.Ordinal);
        Assert.DoesNotContain("reread.", prepare.Substring(0, nothing), StringComparison.Ordinal);
        // The conflict comes before the admission.
        Assert.True(prepare.IndexOf("WithConflict(", StringComparison.Ordinal) < prepare.IndexOf("reread.Read.IsAdmitted", StringComparison.Ordinal));
        // Any exception is an Invalid with its reason, never an exception to the caller.
        Assert.Matches(@"catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*return\s+PushBackPilotPrepareResult\.WithInvalid\s*\(", prepare);
        Assert.DoesNotMatch(new Regex(@"\bthrow\b"), prepare);
    }
}
