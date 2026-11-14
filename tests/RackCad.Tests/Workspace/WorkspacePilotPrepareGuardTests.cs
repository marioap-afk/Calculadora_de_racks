#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace.Drafts;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T4A1-PREPARE-CORE (INV-F4T4A1-01, 07, 08, 10, 12 and 14): the source guards of the read-only PREPARE. The Core test project cannot
// reference the Plugin (it needs AutoCAD), so the sources are read as text, like the rest of the guards of the workspace. Each guard is a
// function of a text that returns its violations; it is run on the real source and, on a mutation of that text or on a synthetic text, it must
// reject what it forbids. The guards of the bodies of the host and of the port (the oracle of zero writes) are the extension of the guards of
// WorkspacePilotEditGuardTests; these are the ones of the files that this task creates or that only it constrains.
public class WorkspacePilotPrepareGuardTests
{
    private const string PlanPath = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs";
    private const string SessionPath = "src/RackCad.Application/Workspace/Drafts/DraftSession.cs";
    private const string PilotFolder = "src/RackCad.Application/Systems/PushBack/Pilot/";

    private static string Plan() => Src.Code(PlanPath);

    private static string Session() => Src.Code(SessionPath);

    // ------------------------------------------------------------------ the file of the plan is pure

    // What a read-only PREPARE never names: a transaction, a lock, a commit, the writing of a block, the host, a thread or a reflection.
    private static readonly (string Name, Regex Pattern, string Injected)[] Forbidden =
    {
        ("OpenMode.ForWrite", new Regex(@"OpenMode\s*\.\s*(?:ForWrite|ForNotify)"), "var o = OpenMode.ForWrite;"),
        ("UpgradeOpen", new Regex(@"\bUpgradeOpen\b"), "entity.UpgradeOpen();"),
        ("AppendEntity", new Regex(@"\bAppendEntity\b"), "btr.AppendEntity(e);"),
        ("Erase", new Regex(@"\.\s*Erase\s*\("), "entity.Erase();"),
        ("Transaction", new Regex(@"Transaction"), "var t = db.TransactionManager.StartTransaction();"),
        ("OpenCloseTransaction", new Regex(@"StartOpenCloseTransaction"), "var t = db.TransactionManager.StartOpenCloseTransaction();"),
        ("LockDocument", new Regex(@"LockDocument"), "using (doc.LockDocument()) { }"),
        ("Commit", new Regex(@"\.\s*Commit\s*\("), "tr.Commit();"),
        ("RackBlockData.Write", new Regex(@"RackBlockData\s*\.\s*Write"), "RackBlockData.Write(id, text);"),
        ("Database", new Regex(@"\bDatabase\b"), "var d = Database.Open();"),
        ("Autodesk", new Regex(@"\bAutodesk\b"), "using Autodesk.AutoCAD.DatabaseServices;"),
        ("RackCad.Plugin", new Regex(@"RackCad\s*\.\s*Plugin"), "using RackCad.Plugin.Views;"),
        ("RackCad.UI", new Regex(@"RackCad\s*\.\s*UI"), "using RackCad.UI.Workspace;"),
        ("the host", new Regex(@"\bIWorkspace\w*|\bWorkspaceHost\w*|\bEditIntentRequest\b|\bEditIntentOutcome\b"), "private IWorkspacePilotEditPort _port;"),
        ("SetCurrentDraftState", new Regex(@"\.\s*SetCurrentDraftState\s*\("), "draft.SetCurrentDraftState(state);"),
        ("a mutation of a draft", new Regex(@"\.\s*(?:SetPendingText|RemovePendingText|MarkPossiblyStale|ConfirmBase|CheckNow|TryBeginApply|CompleteApply|DiscardAndReload|KeepMineOnCurrentVersion|EnterError|CrossBoundary)\s*\("), "draft.CompleteApply(outcome);"),
        ("async", new Regex(@"\basync\b|\bawait\b|\bTask\b|\bThread\b|\bTimer\b|\bDispatcher\b|\bParallel\b"), "await Task.Delay(1);"),
        ("File", new Regex(@"\bFile\s*\.|System\.IO|\bDirectory\s*\."), "File.WriteAllText(path, text);"),
        ("reflection", new Regex(@"System\.Reflection|\bBindingFlags\b|\bActivator\b|\bMethodInfo\b|\bFieldInfo\b|\bPropertyInfo\b"), "var f = typeof(X).GetField(\"a\", BindingFlags.NonPublic);"),
        ("Guid", new Regex(@"\bGuid\s*\.\s*NewGuid|\bDateTime\s*\.\s*(?:Now|UtcNow)|\bRandom\b"), "var g = Guid.NewGuid();"),
    };

    private static IEnumerable<string> PurityViolations(string code)
    {
        foreach (var (name, pattern, _) in Forbidden)
        {
            var match = pattern.Match(code);
            if (match.Success) yield return "the file of the plan names " + name + ": " + match.Value;
        }
    }

    [Fact]
    public void TheFileOfThePlanNamesNoWriteOfTheDrawingNoTransactionNoLockNoHostNoMutationOfADraftAndNoClockOrThread()
    {
        Assert.Empty(PurityViolations(Plan()));
    }

    [Fact]
    public void ThePurityGuardTripsOnEachNameThatItForbidsInTheRealTextOfTheFile()
    {
        var plan = Plan();

        foreach (var (name, pattern, injected) in Forbidden)
        {
            Assert.DoesNotMatch(pattern, plan);
            Assert.True(PurityViolations(plan + "\n" + injected).Any(), name + " is not detected");
        }
    }

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
        foreach (var type in new[] { "PushBackPilotPrepareInput", "PushBackPilotUpdatePlanner", "PushBackPilotUpdatePlan", "PushBackPilotPrepareResult", "PushBackPilotPrepareKind" })
            Assert.Matches(@"\b(?:class|enum)\s+" + type + @"\b", Plan());
        // The one construction of the snapshot is its factory: the constructor is private.
        Assert.Matches(@"private\s+PushBackPilotPrepareInput\s*\(", Plan());
        Assert.Matches(@"public\s+static\s+PushBackPilotPrepareInput\s+From\s*\(\s*DraftSession<PushBackPilotDraftState>\s+draft\s*\)", Plan());
        Assert.Equal(1, Src.Count(Plan(), @"\bnew\s+PushBackPilotPrepareInput\s*\("));
    }

    // ------------------------------------------------------------------ the one call to Compose (INV-F4T4A1-10)

    private static IEnumerable<string> ComposeViolations(string code)
    {
        var calls = Regex.Matches(code, @"\bRackEmbedComposer\s*\.\s*Compose\s*\(").Count;
        if (calls != 1) yield return "the file composes " + calls + " times; it must compose exactly once";

        var member = Regex.Match(code, @"private\s+static\s+string\s+ComposeMemberEnvelope\s*\(\s*RackEmbedDocument\s+source\s*,\s*string\s+view\s*,\s*int\s+section\s*,\s*string\s+designJson\s*\)");
        if (!member.Success)
        {
            yield return "the member ComposeMemberEnvelope with a parameter RackEmbedDocument source (without ?) is missing";
            yield break;
        }

        var body = Src.BodyOf(code, @"private\s+static\s+string\s+ComposeMemberEnvelope\s*\(");
        if (!Regex.IsMatch(body, @"\bRackEmbedComposer\s*\.\s*Compose\s*\(\s*source\s*,")) yield return "the composition does not start from the parameter source";
        if (Regex.IsMatch(body, @"\bnull\b")) yield return "the composition names null";
        // The name is the one of the envelope that was re-read: never a value of the draft.
        if (!Regex.IsMatch(body, @"source\.Id\s*,\s*source\.Name\s*,")) yield return "the identity and the name do not come from the source";
    }

    [Fact]
    public void TheFileOfThePlanComposesExactlyOnceInAMemberOfItsOwnWithAParameterSourceAndTheNameOfTheRereadEnvelope()
    {
        Assert.Empty(ComposeViolations(Plan()));
    }

    [Theory]
    [InlineData("source.Id, source.Name,", "source.Id, \"Otro\",")]
    [InlineData("RackEmbedComposer.Compose(source,", "RackEmbedComposer.Compose(null,")]
    [InlineData("RackEmbedDocument source, string view", "RackEmbedDocument? source, string view")]
    [InlineData("ComposeMemberEnvelope(RackEmbedDocument source", "ComposeEnvelope(RackEmbedDocument source")]
    [InlineData("private static string ComposeMemberEnvelope(", "private static string Another(RackEmbedDocument source) => RackEmbedComposer.Compose(source, \"k\", \"i\", \"n\", \"v\", 1, \"d\");\n        private static string ComposeMemberEnvelope(")]
    public void TheComposeGuardTripsOnAnotherCallAnUnsourcedCompositionANullableSourceOrAnotherName(string from, string to)
    {
        var plan = Plan();
        Assert.Contains(from, plan);

        Assert.NotEmpty(ComposeViolations(plan.Replace(from, to, StringComparison.Ordinal)));
    }

    [Fact]
    public void NobodyComposesTheEnvelopeOfTheWritingOfThePilotOutsideThePlanAndNoPilotFileBeforeThisTaskDoes()
    {
        var composers = Src.AllProduct()
            .Where(f => Regex.IsMatch(f.Code, @"\bRackEmbedComposer\s*\.\s*Compose\s*\(") && f.Path.StartsWith(PilotFolder, StringComparison.Ordinal))
            .Select(f => f.Path)
            .ToArray();

        Assert.Equal(new[] { PlanPath }, composers);
        // The folder of the workspace never composes (the guard of the host is older; this one is the same fact seen from the pilot).
        Assert.DoesNotContain(Src.ApplicationFiles(), f => Regex.IsMatch(f.Code, @"\bRackEmbedComposer\b"));
    }

    // ------------------------------------------------------------------ the composition is used only after a match of the base (H-06)

    private static IEnumerable<string> AfterMatchViolations(IEnumerable<(string Path, string Code)> product)
    {
        var callers = product
            .Where(f => !f.Path.EndsWith("/PushBackPilotMaterializer.cs", StringComparison.Ordinal)
                        && Regex.IsMatch(f.Code, @"\bPushBackPilotMaterializer\s*\.\s*ComposeForWrite\s*\("))
            .ToArray();
        if (callers.Length != 1 || callers[0].Path != PlanPath) yield return "ComposeForWrite has callers other than the planner: " + string.Join(", ", callers.Select(c => c.Path));
        foreach (var (_, code) in callers.Where(c => c.Path == PlanPath))
        {
            var compare = code.IndexOf(".CompareWith(", StringComparison.Ordinal);
            var compose = code.IndexOf("PushBackPilotMaterializer.ComposeForWrite(", StringComparison.Ordinal);
            if (compare < 0 || compose < compare) yield return "ComposeForWrite is called before the comparison of the base";
            if (Regex.Matches(code, @"\bPushBackPilotMaterializer\s*\.\s*ComposeForWrite\s*\(").Count != 1) yield return "ComposeForWrite is called more than once";
        }
    }

    [Fact]
    public void ComposeForWriteIsCalledOnlyByThePlannerAndOnlyAfterTheComparisonOfTheBase()
    {
        Assert.Empty(AfterMatchViolations(Src.AllProduct()));
    }

    [Fact]
    public void TheAfterMatchGuardTripsOnAnotherCallerAndOnACallBeforeTheComparison()
    {
        var product = Src.AllProduct().ToList();

        Assert.NotEmpty(AfterMatchViolations(product.Append(("src/RackCad.Application/Workspace/Other.cs", "var c = PushBackPilotMaterializer.ComposeForWrite(a, b, catalog);"))));

        var plan = Src.Code(PlanPath);
        var before = plan.Replace("var comparison = input.StaleBase.CompareWith(reread.StaleBase);", "var early = PushBackPilotMaterializer.ComposeForWrite(i, c, catalog); var comparison = input.StaleBase.CompareWith(reread.StaleBase);", StringComparison.Ordinal);
        Assert.NotEqual(plan, before);
        var mutated = product.Select(f => f.Path == PlanPath ? (f.Path, before) : f).ToList();
        Assert.NotEmpty(AfterMatchViolations(mutated));
    }

    // ------------------------------------------------------------------ Prepare changes nothing and follows the order of the package

    [Fact]
    public void TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread()
    {
        var plan = Plan();
        var prepare = Src.BodyOf(plan, @"public\s+static\s+PushBackPilotPrepareResult\s+Prepare\s*\(");

        var order = new[]
        {
            "PushBackPilotPrepareKind.NotApplicable",
            "PushBackPilotPrepareKind.PendingText",
            "PushBackPilotPrepareKind.NothingToApply",
            ".CompareWith(",
            "WithConflict(",
            "reread.Read.IsAdmitted",
            "WithNotAdmitted(",
            "Build(",
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

    [Fact]
    public void TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndNeverErase()
    {
        var plan = Plan();
        var build = Src.BodyOf(plan, @"private\s+static\s+PushBackPilotPrepareResult\s+Build\s*\(");

        Assert.Matches(@"reread\.StaleBase\.Entries\.Values\.Where\(\s*e\s*=>\s*e\.Membership\s*==\s*DraftMembership\.Mutable\s*\)\.OrderBy\(\s*e\s*=>\s*e\.Key\s*,\s*StringComparer\.Ordinal\s*\)", build);
        Assert.Matches(@"input\.StaleBase\.Entries\.Values\.OrderBy\(\s*e\s*=>\s*e\.Key\s*,\s*StringComparer\.Ordinal\s*\)", build);
        // The availability is evaluated over the outgoing system, and a view that is not available is an Invalid, never a deletion.
        Assert.Contains("PushBackViewAvailabilityFacts(", build);
        Assert.Contains("RackViewAvailability.Evaluate(", build);
        Assert.Contains("RackViewAvailabilityStatus.Available", build);
        Assert.Contains("RackViewCodec.Decode(", build);
        Assert.Contains("PreflightInnerSources(", build);
        Assert.Contains("RackSystemKind.PushBack", build);
        Assert.Contains("PushBackSystemLateralBuilder().Cortes(", build);
        Assert.Contains("system.IsComposite", build);
        Assert.DoesNotMatch(new Regex(@"\bErase\b|\bDelete\b|\bRemove\b|Phantom|Obsolete"), build);
        // The name of the outgoing envelope comes from the composition over the re-read one; the draft has no name.
        Assert.DoesNotMatch(new Regex(@"\.Name\b"), build);
        // The plan expects the base of the draft and the scale of its current state, nothing of the re-read as a value to write.
        Assert.Contains("input.CurrentDraftState.Inputs.Annotations.AnnotationScale", build);
    }

    // ------------------------------------------------------------------ R-3, the invariant of the core (D-T4A1-06)

    // ErrorReason is only assigned together with the explicit status Error, or to null: the statement that precedes the assignment, inside
    // its block, sets _explicitStatus to DraftStatus.Error.
    // M-4 (I-64 F4-T4A2): the forms of an assignment that the guard recognizes. Besides the plain one, the assignment through this, the compound
    // forms (the null-coalescing assignment and the addition) and a deconstruction that has ErrorReason among its targets. Group 1 is the value.
    private static readonly string[] ErrorReasonAssignmentForms =
    {
        @"(?<![\w.])ErrorReason\s*=(?![=>])\s*([^;]+);",
        @"\bthis\s*\.\s*ErrorReason\s*=(?![=>])\s*([^;]+);",
        @"(?<![\w.])ErrorReason\s*\?\?=\s*([^;]+);",
        @"(?<![\w.])ErrorReason\s*\+=\s*([^;]+);",
        @"\(\s*[^()]*\bErrorReason\b[^()]*\)\s*=(?![=>])\s*([^;]+);",
    };

    private static IEnumerable<string> ErrorReasonCoreViolations(string code)
    {
        var assignments = ErrorReasonAssignmentForms.SelectMany(form => Regex.Matches(code, form).Cast<Match>()).ToList();
        if (assignments.Count == 0) yield return "no assignment of ErrorReason: the guard would be empty";

        foreach (Match assignment in assignments)
        {
            var value = assignment.Groups[1].Value.Trim();
            if (value == "null") continue;

            var before = code.Substring(0, assignment.Index);
            var blockStart = Math.Max(before.LastIndexOf('{'), before.LastIndexOf('}'));
            var block = before.Substring(blockStart + 1);
            if (!Regex.IsMatch(block, @"_explicitStatus\s*=\s*DraftStatus\.Error\s*;"))
                yield return "ErrorReason = " + value + " is not assigned together with _explicitStatus = DraftStatus.Error";
        }

        // The status Error is only entered by the two places that also say why.
        var entries = Regex.Matches(code, @"_explicitStatus\s*=\s*DraftStatus\.Error\s*;");
        foreach (Match entry in entries)
        {
            var after = code.Substring(entry.Index + entry.Length);
            if (!Regex.IsMatch(after.TrimStart(), @"^(?:UnknownOrigin\s*=\s*null\s*;\s*)?ErrorReason\s*=|^ErrorReason\s*=")
                && !Regex.IsMatch(after.TrimStart(), @"^ErrorReason\s*=\s*\w+\s*;"))
                yield return "_explicitStatus = DraftStatus.Error is not followed by the reason";
        }
    }

    [Fact]
    public void TheReasonOfTheErrorIsAssignedInTheCoreOnlyTogetherWithTheStatusErrorOrToNull()
    {
        Assert.Empty(ErrorReasonCoreViolations(Session()));
    }

    [Theory]
    [InlineData("_explicitStatus = DraftStatus.Error;\n            ErrorReason = reason;", "_explicitStatus = DraftStatus.Unknown;\n            ErrorReason = reason;")]
    [InlineData("_explicitStatus = DraftStatus.Error;\n            ErrorReason = reason;", "ErrorReason = reason;")]
    [InlineData("_explicitStatus = DraftStatus.Error;\n            ErrorReason = deferred;", "_explicitStatus = null;\n            ErrorReason = deferred;")]
    [InlineData("LastApplyCause = null;\n            Warnings = Array.Empty<string>();\n            ErrorReason = null;", "LastApplyCause = null;\n            Warnings = Array.Empty<string>();\n            ErrorReason = \"left over\";")]
    public void TheGuardOfTheReasonOfTheErrorTripsOnAReasonWithoutTheStatusErrorAndOnAnAssignmentOutsideTheStatus(string from, string to)
    {
        var session = Session().Replace("\r\n", "\n");
        Assert.Contains(from, session);

        Assert.NotEmpty(ErrorReasonCoreViolations(session.Replace(from, to, StringComparison.Ordinal)));
    }

    // M-4: the same guard trips on the forms of assignment that it did not see before, each one outside the status Error. The real block that
    // resets the reason to null is the place of the mutation, so each text is the real file with ONE Replace.
    [Theory]
    [InlineData("this.ErrorReason = \"left over\";")]
    [InlineData("this . ErrorReason = \"left over\";")]
    [InlineData("ErrorReason ??= \"left over\";")]
    [InlineData("ErrorReason += \"left over\";")]
    [InlineData("(ErrorReason, _) = (\"left over\", 0);")]
    [InlineData("(var other, ErrorReason) = (0, \"left over\");")]
    public void TheGuardOfTheReasonOfTheErrorTripsOnTheFormsThatAssignItThroughThisCompoundlyOrByDeconstructionOutsideTheStatusError(string assignment)
    {
        const string block = "LastApplyCause = null;\n            Warnings = Array.Empty<string>();\n            ErrorReason = null;";
        var session = Session().Replace("\r\n", "\n");
        Assert.Contains(block, session);
        Assert.Empty(ErrorReasonCoreViolations(session));

        var mutated = session.Replace(block, "LastApplyCause = null;\n            Warnings = Array.Empty<string>();\n            " + assignment, StringComparison.Ordinal);

        Assert.NotEqual(session, mutated);
        Assert.NotEmpty(ErrorReasonCoreViolations(mutated));
    }

    [Fact]
    public void TheGuardOfTheReasonOfTheErrorTripsOnANewMethodThatAssignsItAlone()
    {
        var session = Session().Replace("\r\n", "\n");
        var mutated = session.Replace(
            "        void IDraftEntry.Detach()",
            "        public void Fail(string why)\n        {\n            ErrorReason = why;\n        }\n\n        void IDraftEntry.Detach()",
            StringComparison.Ordinal);

        Assert.NotEqual(session, mutated);
        Assert.NotEmpty(ErrorReasonCoreViolations(mutated));
        Assert.Empty(ErrorReasonCoreViolations(session));
        Assert.NotEmpty(ErrorReasonCoreViolations("class C { }"));
    }

    [Fact]
    public void TheReasonOfTheErrorIsNeverLeftBehindOutsideTheStatusErrorInTheRealDraftSession()
    {
        // The same invariant in behavior: the reason exists only while the draft is in Error, and every way out of Error removes it.
        var draft = DraftKit.Dirty();
        Assert.Null(draft.ErrorReason);

        draft.EnterError("the surface failed");
        Assert.Equal(DraftStatus.Error, draft.Status);
        Assert.Equal("the surface failed", draft.ErrorReason);
        Assert.Equal(DraftReloadStatus.Reloaded, draft.DiscardAndReload(DraftKit.Read()).Status);
        Assert.NotEqual(DraftStatus.Error, draft.Status);
        Assert.Null(draft.ErrorReason);

        // An Error that arrives while an update is in flight is applied when the update closes, and the reason goes with it.
        var applying = DraftKit.ToApplying(DraftKit.Dirty());
        applying.EnterError("late failure");
        Assert.Null(applying.ErrorReason);
        Assert.Equal("late failure", applying.DeferredErrorReason);
        applying.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedFailure("not applied"));
        Assert.Equal(DraftStatus.Error, applying.Status);
        Assert.Equal("late failure", applying.ErrorReason);
        Assert.Equal(DraftReloadStatus.Reloaded, applying.DiscardAndReload(DraftKit.Read()).Status);
        Assert.Null(applying.ErrorReason);
        Assert.NotEqual(DraftStatus.Error, applying.Status);

        // The new operations never leave a reason behind: the general check does not reach an Error, and the keep needs Dirty or Conflict.
        var errored = DraftKit.ToError(DraftKit.Dirty());
        Assert.Equal(DraftConfirmStatus.NotApplicable, errored.ConfirmBase(DraftKit.DefaultBase()).Status);
        Assert.Equal(DraftReloadStatus.NotAllowed, errored.KeepMineOnCurrentVersion(DraftKit.Read(), (initial, current) => current, confirmed: true).Status);
        Assert.Equal("surface failed", errored.ErrorReason);
        Assert.Equal(DraftStatus.Error, errored.Status);
    }

    // ------------------------------------------------------------------ the limits of the task

    [Fact]
    public void TheBuildersThatThePlanUsesNeedNeitherTheDrawingNorThePlugin()
    {
        foreach (var path in new[]
                 {
                     "src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorAssembler.cs",
                     "src/RackCad.Application/Systems/PushBack/PushBackSystemLateralBuilder.cs",
                     "src/RackCad.Application/Systems/PushBack/PushBackResolver.cs",
                     "src/RackCad.Application/Systems/Shared/RackViewAvailability.cs",
                     "src/RackCad.Application/Systems/Shared/RackViewCodec.cs",
                     "src/RackCad.Application/Persistence/RackEmbedComposer.cs",
                     "src/RackCad.Application/Persistence/RackProjectStore.cs",
                 })
        {
            var code = Src.Code(path);
            Assert.DoesNotMatch(new Regex(@"\bAutodesk\b|RackCad\s*\.\s*Plugin|RackCad\s*\.\s*UI|\bTransactionManager\b"), code);
        }
    }

    [Fact]
    public void NoFileOfTheTaskUsesTheCommandContextNorAsynchronyNorTimersNorADispatcher()
    {
        var pure = new[]
        {
            PlanPath,
            SessionPath,
            "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs",
            "src/RackCad.Application/Workspace/WorkspacePilotSupport.cs",
            "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs",
            "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs",
        };
        // The host core and the controller read the result of a drain (outcome.Result), which is not a task: they only exclude the rest.
        var shared = new[]
        {
            "src/RackCad.Application/Workspace/WorkspaceHostCore.cs",
            "src/RackCad.Plugin/Workspace/WorkspaceController.cs",
        };

        foreach (var path in pure)
            Assert.DoesNotMatch(new Regex(@"ExecuteInCommandContextAsync|\basync\b|\bawait\b|\.Result\b|\bTask\b|\bDispatcher\b|\bTimer\b"), Src.Code(path));
        foreach (var path in shared)
            Assert.DoesNotMatch(new Regex(@"ExecuteInCommandContextAsync|\basync\b|\bawait\b|\bTask\b|\bDispatcher\b|\bTimer\b"), Src.Code(path));
    }
}
