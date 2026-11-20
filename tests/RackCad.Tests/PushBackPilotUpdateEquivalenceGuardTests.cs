using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    using Src = RackCad.Tests.WorkspaceHostSource;

    /// <summary>
    /// I-64 F4B PB-T3b — the EQUIVALENTS of the six guards of F4 that D-1 substitutes (F4B-E29, variant (A)): E-1 to E-6, one [Fact] each.
    /// The guards of F4 read the plan and the run of the pilot as they were written for ONE model; with the generalization of PB-T3b
    /// (a single code for the two models) six of them describe a text that no longer exists. Each equivalent keeps what its
    /// predecessor protected and says it on the new form, and it STARTS by asserting that new form, so a text that does not have it
    /// fails here too. The registry of the substitution is <c>superseding-characterization.md</c>; the self-tests run over text mutated in
    /// memory, never written.
    /// </summary>
    public sealed class PushBackPilotUpdateEquivalenceGuardTests
    {
        private const string PilotFolder = "src/RackCad.Application/Systems/PushBack/Pilot/";
        private const string PlanPath = PilotFolder + "PushBackPilotUpdatePlan.cs";
        private const string RunPath = PilotFolder + "PushBackPilotUpdateRun.cs";
        private const string DispositionPath = "src/RackCad.Application/Systems/PushBack/Draft/PushBackViewDisposition.cs";

        private static string Plan() => Src.Code(PlanPath);

        private static string Run() => Src.Code(RunPath);

        private static string Core(string plan) => Src.BodyOf(plan, @"private\s+static\s+PushBackPilotPrepareResult\s+BuildCore\s*\(");

        private static string Join(IEnumerable<string> lines) => string.Join("\n", lines);

        // ------------------------------------------------------------------ E-1 and E-2: the plan composes ONCE, over the envelope that was re-read

        /// <summary>The one call to Compose, in a member of its own with a parameter source and a parameter name, and the name of every call of
        /// the plan coming from one expression of BuildCore (E-1; S-1 of F4).</summary>
        private static IEnumerable<string> E1Violations(string code)
        {
            var calls = Regex.Matches(code, @"\bRackEmbedComposer\s*\.\s*Compose\s*\(").Count;
            if (calls != 1) yield return "the file composes " + calls + " times; it must compose exactly once";

            var member = Regex.Match(code, @"private\s+static\s+string\s+ComposeMemberEnvelope\s*\(\s*RackEmbedDocument\s+source\s*,\s*string\s+name\s*,\s*string\s+view\s*,\s*int\s+section\s*,\s*string\s+designJson\s*\)");
            if (!member.Success)
            {
                yield return "the member ComposeMemberEnvelope with a parameter RackEmbedDocument source (without ?) and a parameter string name is missing";
                yield break;
            }

            var body = Src.BodyOf(code, @"private\s+static\s+string\s+ComposeMemberEnvelope\s*\(");
            if (!Regex.IsMatch(body, @"\bRackEmbedComposer\s*\.\s*Compose\s*\(\s*source\s*,")) yield return "the composition does not start from the parameter source";
            if (Regex.IsMatch(body, @"\bnull\b")) yield return "the composition names null";
            // The identity comes from the envelope that was re-read and the name is the parameter: never a value of the draft inside the member.
            if (!Regex.IsMatch(body, @"source\.Id\s*,\s*name\s*,")) yield return "the identity does not come from the source or the name is not the parameter";

            // The name of each call comes from ONE expression of BuildCore: the member name that BuildCore computes once.
            var core = Core(code);
            var composeCalls = Regex.Matches(core, @"\bComposeMemberEnvelope\s*\(\s*embed\s*,\s*([^,]+?)\s*,");
            if (composeCalls.Count != 1) yield return "BuildCore calls ComposeMemberEnvelope " + composeCalls.Count + " times; it must call it exactly once";
            else if (composeCalls[0].Groups[1].Value != "memberName") yield return "the name of the call is not the member name of BuildCore";
            if (Regex.Matches(core, @"\bvar\s+memberName\s*=").Count != 1) yield return "the member name is not computed by exactly one expression of BuildCore";
            if (Regex.Matches(code, @"\bComposeMemberEnvelope\s*\(").Count != 2) yield return "ComposeMemberEnvelope is declared or called more times than the declaration and the one call";
        }

        [Fact]
        public void E_1_ThePlanComposesExactlyOnceInAMemberOfItsOwnWithAParameterSourceAndAParameterNameThatComesFromOneExpression()
        {
            var plan = Plan();
            Assert.Contains("ComposeMemberEnvelope(RackEmbedDocument source, string name, string view, int section, string designJson)", plan);

            Assert.Equal(string.Empty, Join(E1Violations(plan)));
        }

        [Fact]
        public void E_2_TheComposeGuardTripsOnAnotherCallAnUnsourcedCompositionANullableSourceAnotherNameOrANameThatIsNotTheParameter()
        {
            var plan = Plan();
            var changes = new (string From, string To)[]
            {
                ("source.Id, name,", "source.Id, \"Otro\","),
                ("RackEmbedComposer.Compose(source,", "RackEmbedComposer.Compose(null,"),
                ("RackEmbedDocument source, string name", "RackEmbedDocument? source, string name"),
                ("ComposeMemberEnvelope(RackEmbedDocument source", "ComposeEnvelope(RackEmbedDocument source"),
                ("private static string ComposeMemberEnvelope(", "private static string Another(RackEmbedDocument source) => RackEmbedComposer.Compose(source, \"k\", \"i\", \"n\", \"v\", 1, \"d\");\n        private static string ComposeMemberEnvelope("),
            };

            // The new form first: every text that a self-test mutates is in the plan, and the plan has no violation.
            foreach (var change in changes) Assert.Contains(change.From, plan);
            Assert.Empty(E1Violations(plan));

            var lines = new List<string>();
            foreach (var change in changes)
            {
                var mutated = plan.Replace(change.From, change.To, StringComparison.Ordinal);
                lines.Add(change.From.Substring(0, Math.Min(24, change.From.Length)) + " cambia-el-texto=" + (mutated != plan) + " violaciones>0=" + E1Violations(mutated).Any());
            }

            Assert.Equal(
                Join(changes.Select(change => change.From.Substring(0, Math.Min(24, change.From.Length)) + " cambia-el-texto=True violaciones>0=True")),
                Join(lines));
        }

        // ------------------------------------------------------------------ E-3: the plans of the members, from BuildCore and from the disposition of the views

        /// <summary>What S-3 of F4 protected, over BuildCore and over the disposition of the views (V2-03): the members that the plan redraws are
        /// the mutable ones of the re-read, in ordinal order; the availability comes from the outgoing system; a view that is not available is never
        /// erased by the pilot; and the erasures are only filled when the model admits them and only with an obsolete lateral.</summary>
        private static IEnumerable<string> E3Violations(string plan, string disposition)
        {
            var core = Core(plan);
            var build = Src.BodyOf(plan, @"private\s+static\s+PushBackPilotPrepareResult\s+Build\s*\(");
            var full = Src.BodyOf(plan, @"private\s+static\s+PushBackPilotPrepareResult\s+BuildFull\s*\(");
            var of = Src.BodyOf(disposition, @"public\s+static\s+PushBackViewDispositionPlan\s+Of\s*\(");

            if (!Regex.IsMatch(core, @"rereadBase\.Entries\.Values\.Where\(\s*e\s*=>\s*e\.Membership\s*==\s*DraftMembership\.Mutable\s*\)\.OrderBy\(\s*e\s*=>\s*e\.Key\s*,\s*StringComparer\.Ordinal\s*\)"))
                yield return "BuildCore does not take the mutable members of the re-read in ordinal order";
            if (!Regex.IsMatch(core, @"staleBase\.Entries\.Values\.OrderBy\(\s*e\s*=>\s*e\.Key\s*,\s*StringComparer\.Ordinal\s*\)"))
                yield return "BuildCore does not take the base of the draft in ordinal order";
            foreach (var needle in new[] { "PreflightInnerSources(", "RackSystemKind.PushBack", "PushBackViewDispositions.Of(" })
                if (!core.Contains(needle, StringComparison.Ordinal)) yield return "BuildCore lacks " + needle;
            foreach (var needle in new[] { "PushBackViewAvailabilityFacts(", "RackViewAvailability.Evaluate(", "RackViewAvailabilityStatus.Available", "RackViewCodec.Decode(", "PushBackSystemLateralBuilder().Cortes(", ".IsComposite" })
                if (!of.Contains(needle, StringComparison.Ordinal)) yield return "the disposition of the views lacks " + needle;

            // The erasures: only with a model that admits them, only with an obsolete lateral, and in one place.
            if (Regex.Matches(core, @"\berasures\.Add\s*\(").Count != 1) yield return "the erasures are not filled in exactly one place";
            if (!core.Contains("PushBackViewDispositionKind.ObsoleteLateral && allowErase", StringComparison.Ordinal)) yield return "the erasures are not conditioned to an obsolete lateral and to a model that admits erasing";
            // The pilot keeps its text for a view that is not available.
            if (!core.Contains("ViewUnavailablePrefix + entry.Key + ViewUnavailableSuffix", StringComparison.Ordinal)) yield return "the pilot does not keep ViewUnavailablePrefix";
            if (!Regex.IsMatch(build, @"\bBuildCore\s*\([^;]*\bfalse\b")) yield return "the pilot does not call BuildCore without the capacity to erase";
            if (!Regex.IsMatch(full, @"\bBuildCore\s*\([^;]*\btrue\b")) yield return "the full model does not call BuildCore with the capacity to erase";
            // The scale of the plan is the one of the current state of the model.
            if (!build.Contains("input.CurrentDraftState.Inputs.Annotations.AnnotationScale", StringComparison.Ordinal)) yield return "the pilot does not take the scale of its current state";
            if (!full.Contains("input.CurrentDraftState.Editable.AnnotationScale", StringComparison.Ordinal)) yield return "the full model does not take the scale of its current state";
        }

        [Fact]
        public void E_3_TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndErasuresOnlyComeFromAnObsoleteLateralOfAModelThatErases()
        {
            var plan = Plan();
            var disposition = Src.Code(DispositionPath);

            Assert.Contains("PushBackViewDispositionKind.ObsoleteLateral && allowErase", plan);
            Assert.Contains("ViewUnavailablePrefix + entry.Key + ViewUnavailableSuffix", plan);
            Assert.Equal(string.Empty, Join(E3Violations(plan, disposition)));
        }

        // ------------------------------------------------------------------ E-4: the two scopes are released in finally blocks of their own, on Execution<TState>

        private static IEnumerable<string> E4Violations(string code)
        {
            if (Regex.IsMatch(code, @"\busing\s*\(|\busing\s+var\b")) yield return "the run releases a scope with a using";
            if (!Regex.IsMatch(code, @"finally\s*\{\s*try\s*\{\s*mutation\.Dispose\s*\(\s*\)\s*;\s*\}\s*catch\s*\(\s*Exception\s+ex\s*\)")) yield return "the scope of MUTATE is not released in a finally of its own";
            if (!Regex.IsMatch(code, @"finally\s*\{\s*if\s*\(\s*scope\s*!=\s*null\s*\)\s*\{\s*try\s*\{\s*scope\.Dispose\s*\(\s*\)\s*;\s*\}\s*catch\s*\(\s*Exception\s+ex\s*\)")) yield return "the scope of the run is not released in a finally of its own";
            if (Src.Count(code, @"\bmutation\.Dispose\s*\(") != 1) yield return "the scope of MUTATE is not released exactly once";
            if (Src.Count(code, @"\bscope\.Dispose\s*\(") != 1) yield return "the scope of the run is not released exactly once";

            // POST is reached only after MUTATE was released, and its signature is the one of the generic run.
            var release = code.IndexOf("mutation.Dispose()", StringComparison.Ordinal);
            var goesToPost = release < 0 ? -1 : code.IndexOf("return null;", release, StringComparison.Ordinal);
            if (release <= 0 || goesToPost <= release) yield return "the scope of MUTATE is not released before the run says that POST follows";
            else if (code.IndexOf("private DraftApplyOutcome<TState> Post(IPushBackPilotUpdateScope scope)", goesToPost, StringComparison.Ordinal) <= goesToPost)
                yield return "POST does not follow the release of MUTATE";
        }

        [Fact]
        public void E_4_TheGenericRunReleasesBothScopesInFinallyBlocksOfItsOwnAndNeverWithAUsing()
        {
            var run = Run();
            Assert.Contains("private sealed class Execution<TState>", run);
            Assert.Contains("private DraftApplyOutcome<TState> Post(IPushBackPilotUpdateScope scope)", run);

            Assert.Equal(string.Empty, Join(E4Violations(run)));
        }

        // ------------------------------------------------------------------ E-5 and E-6: what escapes the run is classified by what the run already did

        private static IEnumerable<string> EscapeOf(string label, string body)
        {
            var created = body.IndexOf("var execution = new Execution<", StringComparison.Ordinal);
            var tryAt = Regex.Match(body, @"\btry\b").Index;
            if (created < 0 || created > tryAt) yield return label + ": the state of the run is not created before the try";
            if (!Regex.IsMatch(body, @"catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*return\s+execution\.OutcomeOfAnEscape\s*\(\s*ex\s*\)\s*;"))
                yield return label + ": the catch does not classify by the state of the run";
        }

        /// <summary>The rules of M-01 on the two runs that must exist, <c>Execute</c> (the pilot) and <c>ExecuteFull</c> (the full model), and on the
        /// <c>OutcomeOfAnEscape</c> of <c>Execution&lt;TState&gt;</c> (E-5; S-5 of F4). A run that does not exist is a violation.</summary>
        private static IEnumerable<string> E5Violations(string run)
        {
            foreach (var (label, signature) in new[]
            {
                ("Execute", @"public\s+static\s+DraftApplyOutcome<PushBackPilotDraftState>\s+Execute\s*\("),
                ("ExecuteFull", @"public\s+static\s+DraftApplyOutcome<PushBackFullDraftState>\s+ExecuteFull\s*\("),
            })
            {
                if (!Regex.IsMatch(run, signature))
                {
                    yield return label + " does not exist";
                    continue;
                }

                foreach (var violation in EscapeOf(label, Src.BodyOf(run, signature))) yield return violation;
            }

            var escapeSignature = @"public\s+DraftApplyOutcome<TState>\s+OutcomeOfAnEscape\s*\(";
            if (!Regex.IsMatch(run, escapeSignature))
            {
                yield return "OutcomeOfAnEscape does not exist on the generic run";
                yield break;
            }

            var escape = Src.BodyOf(run, escapeSignature);
            var seal = escape.IndexOf("_sealReturned", StringComparison.Ordinal);
            var write = escape.IndexOf("_writeStarted", StringComparison.Ordinal);
            var plain = escape.IndexOf("NotAppliedFailure(", StringComparison.Ordinal);
            if (seal < 0 || write < seal || plain < write) yield return "the escape does not consult the seal, then the write, before a plain failure";
            if (Regex.IsMatch(escape, @"DraftApplyOutcomeKind\.Applied\b|\.Applied\s*\(")) yield return "the escape can answer an applied";
        }

        [Fact]
        public void E_5_TheEscapeOfEachRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal()
        {
            var run = Run();
            Assert.Contains("public static DraftApplyOutcome<PushBackFullDraftState> ExecuteFull(", run);
            Assert.Contains("public DraftApplyOutcome<TState> OutcomeOfAnEscape(", run);

            Assert.Equal(string.Empty, Join(E5Violations(run)));
        }

        [Fact]
        public void E_6_TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst()
        {
            var run = Run();
            foreach (var form in new[] { "var execution = new Execution<", "return execution.OutcomeOfAnEscape(ex);", "if (_sealReturned) return PostFailed(PostFailurePrefix + ex.Message);" })
                Assert.Contains(form, run);
            Assert.Empty(E5Violations(run));

            var mutations = new[]
            {
                run.Replace("return execution.OutcomeOfAnEscape(ex);", "return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(RunFailurePrefix + ex.Message);", StringComparison.Ordinal),
                run.Replace("var execution = new Execution<", "var another = new Execution<", StringComparison.Ordinal),
                run.Replace("if (_sealReturned) return PostFailed(PostFailurePrefix + ex.Message);", string.Empty, StringComparison.Ordinal),
            };

            var lines = mutations.Select((mutated, index) => "autoprueba-" + (index + 1) + " cambia-el-texto=" + (mutated != run) + " violaciones>0=" + E5Violations(mutated).Any());
            Assert.Equal(
                Join(Enumerable.Range(1, 3).Select(index => "autoprueba-" + index + " cambia-el-texto=True violaciones>0=True")),
                Join(lines));
        }
    }
}
