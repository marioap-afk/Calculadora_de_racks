using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;
using static RackCad.Tests.G9BehavioralContractSupport;
using static RackCad.Tests.G9ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>Behavioral coverage of every G9 row in exact A3-R2 §11.2. T-A3-48 is G10-only.</summary>
    public sealed class G9A3ContractMatrixTests
    {
        private const string M1 = "aaaaaaaa-0000-0000-0000-000000000001";
        private const string M2 = "aaaaaaaa-0000-0000-0000-000000000002";
        private const string M3 = "aaaaaaaa-0000-0000-0000-000000000003";

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_17_ROOT_SHRINK_ABORTS_BOTH_OBSERVATIONS() => AssertRootChange(false);

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_18_ROOT_GROWTH_ABORTS_BOTH_OBSERVATIONS() => AssertRootChange(true);

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_19_CHAIN_ONLY_RACE_ABORTS_SYMBOL_BUT_UPSTREAM_MATCHES()
        {
            var root = Broken(IdC, M1);
            var before = Failed(new[] { Dependency(IdB, root, IdD, IdB, IdA, IdC) }, root);
            var commit = Failed(new[] { Dependency(IdB, root, IdD, IdB, IdC) }, root);
            Assert.False(SymbolMatches(before, commit));
            Assert.True(UpstreamMatches(Read(IdD, before), Read(IdD, commit)));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_20_REMOVED_SOURCE_ROOT_CHANGE_MISMATCHES()
            => Assert.False(UpstreamMatches(Read(IdD, Failure(IdD, Broken(IdA, M1), Broken(IdB, M2))), Read(IdD, Failure(IdD, Broken(IdA, M1)))));

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_21_ONE_UNIT_IS_ELIGIBLE_AND_TWO_UNITS_ARE_NOT()
        {
            var one = Assess(Registry(Expression(IdA, "A", Ref(M1))), Ref(IdA));
            Assert.True(one.IsCandidate); Assert.Equal(new[] { IdA }, one.Units);
            var two = Assess(Registry(Expression(IdA, "A", Ref(M1)), Expression(IdB, "B", Ref(M2))), Add(Ref(IdA), Ref(IdB)));
            Assert.False(two.IsCandidate); Assert.Equal(new[] { IdA, IdB }, two.Units);
            Assert.Equal(new[] { "SeveralRecoveryUnits" }, two.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_35_RECOVERED_ROOT_MISMATCHES_UPSTREAM()
            => Assert.False(UpstreamMatches(Read(IdA, Failure(IdA, Broken(IdA, M1))), Read(IdA, Success(1))));

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_36_MISSINGTARGET_SIBLING_IS_OTHER_INVALID_SOURCE()
            => AssertBlocked(Assess(Registry(Expression(IdA, "H", Ref(M1))), Ref(IdA), Ref(M2)), "OtherInvalidSources");

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_37_NON_SIMPLE_SCC_REPORTS_EXACT_MEMBERS()
        {
            var result = Assess(Registry(Expression(IdA, "A", Add(Ref(IdA), Ref(IdB))), Expression(IdB, "B", Ref(IdA))), Ref(IdA));
            AssertBlocked(result, "NonSimpleCycle"); Assert.Contains(result.ReasonData, value => value.Contains(IdA) && value.Contains(IdB));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_38_ENTRY_MEMBER_CHANGE_ONLY_MISMATCHES_SYMBOL()
        {
            var cycle = Cycle(IdA, IdB);
            var before = Failed(new[] { Dependency(IdA, cycle, IdD, IdA) }, cycle);
            var after = Failed(new[] { Dependency(IdB, cycle, IdD, IdB) }, cycle);
            Assert.False(SymbolMatches(before, after)); Assert.True(UpstreamMatches(Read(IdD, before), Read(IdD, after)));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_39_LATENT_DIVISION_STILL_ALLOWS_CURRENT_CANDIDATE()
        {
            var registry = Registry(Expression(IdA, "A", Ref(M1)), Expression(IdD, "D", Add(Ref(IdA), Div(1, 0))));
            var result = G8ContractTestSupport.Evaluate(registry).Results[SymbolId.ProjectVariable(IdD)];
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.DivisionByZero);
            Assert.True(Assess(registry, Add(Ref(IdD), Num(2))).IsCandidate);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_40_SIMPLE_CYCLE_IS_A_CANDIDATE()
        {
            var result = Assess(Registry(Expression(IdA, "A", Ref(IdB)), Expression(IdB, "B", Ref(IdA))), Add(Ref(IdA), Num(1)));
            Assert.True(result.IsCandidate); Assert.Contains(IdA, result.Candidate); Assert.Contains(IdB, result.Candidate);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_44_ONLY_UNITS_OUTSIDE_THE_SOURCE_SET_BLOCK()
        {
            var registry = Registry(Expression(IdA, "H", Ref(M1)), Expression(IdB, "G", Div(1, 0)));
            AssertBlocked(Assess(registry, Add(Ref(IdA), Num(1)), Add(Ref(IdA), Ref(IdB))), "OtherInvalidSources");
            var wider = Assess(registry, Add(Ref(IdA), Ref(IdB)), Add(Ref(IdA), Num(1)));
            Assert.Contains("SeveralRecoveryUnits", wider.Reasons); Assert.DoesNotContain("OtherInvalidSources", wider.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_45_DOMAIN_STRUCTURAL_AND_OTHER_RACK_SOURCES_BLOCK()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Add(Ref(IdA), Num(1)), Div(1, 0), Add(Ref(IdA), Ref(M2)), unknown: true);
            AssertBlocked(result, "OtherInvalidSources"); Assert.Contains("DiscoveryIndeterminate", result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_46_STATIC_OWN_FAILURE_PRECEDES_UPSTREAM()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Call("ABS", Ref(IdA), Num(2)));
            Assert.False(result.IsCandidate); Assert.Contains(result.ReasonData, value => value.Contains("InvalidArguments"));
            Assert.DoesNotContain(result.ReasonData, value => value.Contains("Upstream"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_47_SELF_EDGE_INSIDE_SCC_IS_NON_SIMPLE()
            => AssertBlocked(Assess(Registry(Expression(IdA, "A", Add(Ref(IdA), Ref(IdB))), Expression(IdB, "B", Ref(IdA))), Ref(IdA)), "NonSimpleCycle");

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_49_CONTAINED_OTHER_RACK_UNIT_DOES_NOT_ADD_BLOCKER()
        {
            var result = Assess(Registry(Expression(IdA, "A1", Ref(M1)), Expression(IdB, "A2", Ref(M2))), Add(Ref(IdA), Ref(IdB)), other: Ref(IdA));
            Assert.False(result.IsCandidate); Assert.Equal(new[] { "SeveralRecoveryUnits" }, result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_50_R1_REJECTION_RETURNS_TYPED_ATTEMPT_AND_EMPTY_PLAN()
        {
            var result = ProjectVariableMutationPreflight.ChangeValue(
                Registry(Literal(IdA, "A", 1), Expression(IdB, "B", Div(1, Ref(IdA)))), Id(IdA), VariableDefinition.Literal(0),
                new[] { View(Design(directVariable: IdB)) });
            Assert.False(result.IsSuccess); Assert.True(result.Plan.IsEmpty); Assert.NotNull(result.Error);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_51_INDETERMINATE_DISCOVERY_BLOCKS_WITH_RACK_CAUSE()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Ref(IdA), unreadable: true);
            AssertBlocked(result, "DiscoveryIndeterminate"); Assert.Contains(result.ReasonData, value => value.Contains(RackB));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_52_SAME_OWNER_TWO_SIGNATURES_IS_ONE_ELIGIBLE_UNIT()
        {
            var result = Assess(Registry(Expression(IdA, "H", Call("ABS", Ref(M1), Num(2)))), Ref(IdA));
            Assert.True(result.IsCandidate); Assert.Equal(new[] { IdA }, result.Units);
            Assert.Contains(result.ReasonData, value => value.Contains("BrokenReference"));
            Assert.Contains(result.ReasonData, value => value.Contains("InvalidArguments"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_53_SAME_UNIT_DIFFERENT_SIGNATURE_MISMATCHES()
            => Assert.False(UpstreamMatches(Read(IdA, Failure(IdA, Broken(IdA, M1))), Read(IdA, Failure(IdA, Invalid(IdA, "ABS", 2)))));

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_54_TWO_OWNERS_ARE_TWO_ORDERED_UNITS()
        {
            var result = Assess(Registry(Expression(IdA, "P", Ref(M1)), Expression(IdB, "Q", Call("ABS", Num(1), Num(2))), Expression(IdC, "S", Add(Ref(IdA), Ref(IdB)))), Ref(IdC));
            Assert.False(result.IsCandidate); Assert.Equal(new[] { IdA, IdB }, result.Units); Assert.Equal(new[] { "SeveralRecoveryUnits" }, result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_55_CYCLE_PLUS_MEMBER_FAILURE_HAS_TWO_UNITS_NOT_NON_SIMPLE()
        {
            var result = Assess(Registry(Expression(IdA, "A", Add(Ref(IdB), Call("ABS", Num(1), Num(2)))), Expression(IdB, "B", Ref(IdA))), Ref(IdB));
            Assert.False(result.IsCandidate); Assert.Equal(2, result.Units.Length); Assert.Contains("SeveralRecoveryUnits", result.Reasons); Assert.DoesNotContain("NonSimpleCycle", result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_56_BLOCKED_RACK_HAS_BOTH_STRUCTURED_REASONS()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Ref(IdA), unknown: true);
            Assert.False(result.IsCandidate); Assert.Equal(new[] { "OtherInvalidSources", "DiscoveryIndeterminate" }, result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_57_POSITIVELY_UNRELATED_FAILED_RACK_IS_IGNORED()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1)), Expression(IdB, "Z", Ref(M2))), Ref(IdA), other: Add(Ref(IdB), Ref(M3)));
            Assert.True(result.IsCandidate); Assert.DoesNotContain(result.ReasonData, value => value.Contains(RackB));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_58_POSITIVELY_UNRELATED_NON_SINGLE_RACK_IS_IGNORED()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1)), Expression(IdB, "Z", Ref(M2))), Ref(IdA), other: Ref(IdB), duplicateOther: true);
            Assert.True(result.IsCandidate); Assert.DoesNotContain("DiscoveryIndeterminate", result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_59_STATIC_INVALID_ARGUMENTS_KEEP_NUMERIC_FAILURE_LATENT()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Add(Add(Ref(IdA), Div(1, 0)), Call("ABS", Num(1), Num(2))));
            Assert.Contains(result.ReasonData, value => value.Contains("InvalidArguments"));
            Assert.DoesNotContain(result.ReasonData, value => value.Contains("DivisionByZero") || value.Contains("Upstream"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_60_ROOT_SWAP_IS_COMPARED_PER_VARIABLE_NOT_BY_UNION()
        {
            var x = Broken(IdC, M1); var y = Broken(IdD, M2);
            var before = new Dictionary<SymbolId, RegistrySymbolResult> { [SymbolId.ProjectVariable(IdA)] = Failure(IdA, x), [SymbolId.ProjectVariable(IdB)] = Failure(IdB, y) };
            var after = new Dictionary<SymbolId, RegistrySymbolResult> { [SymbolId.ProjectVariable(IdA)] = Failure(IdA, y), [SymbolId.ProjectVariable(IdB)] = Failure(IdB, x) };
            Assert.False(UpstreamMatches(before, after));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_64_MULTI_ABORT_DATA_IS_COMPLETE_DEDUPLICATED_AND_ORDERED()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Ref(IdA), envelopes: true);
            AssertBlocked(result, "DiscoveryIndeterminate");
            Assert.Equal(new[] { "EnvelopeUnclassifiable(D1)", "EnvelopeUnclassifiable(D2)" }, result.ReasonData.Where(value => value.Contains("EnvelopeUnclassifiable")).ToArray());
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void SYMBOL_RESULTS_COMPARE_VALUES_DIAGNOSTICS_CAUSE_CHAIN_ROOT_AND_ALL_ROOTS()
        {
            Assert.True(SymbolMatches(Success(10), Success(10))); Assert.False(SymbolMatches(Success(10), Success(11)));
            var root = Broken(IdA, M1); var same = Failure(IdD, root);
            Assert.True(SymbolMatches(same, Failure(IdD, root)));
            Assert.False(SymbolMatches(same, Failed(new[] { Dependency(IdB, root, IdD, IdB) }, root)));
            Assert.False(SymbolMatches(Failed(new[] { Dependency(IdA, root, IdD, IdA) }, root), Failed(new[] { Dependency(IdB, root, IdD, IdA) }, root)));
            Assert.False(SymbolMatches(Failed(new[] { Dependency(IdA, root, IdD, IdA) }, root), Failed(new[] { Dependency(IdA, root, IdD, IdB) }, root)));
            Assert.False(SymbolMatches(Failure(IdD, root), Failure(IdD, root, Broken(IdB, M2))));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void REPAIR_REASONS_COMPARE_STABLE_MISSING_INTRINSIC_AND_DOMAIN_DATA()
        {
            Assert.True(RepairReasonMatches(MissingTarget(M1, M2), MissingTarget(M2, M1), "MissingTarget"));
            Assert.False(RepairReasonMatches(MissingTarget(M1, M2), MissingTarget(M1), "MissingTarget"));
            Assert.False(RepairReasonMatches(MissingTarget(M1, M2), MissingTarget(M1, M2, M3), "MissingTarget"));
            Assert.True(RepairReasonMatches(IntrinsicReason("InvalidArguments", "ABS", 2), IntrinsicReason("InvalidArguments", "ABS", 2), "Intrinsic"));
            Assert.False(RepairReasonMatches(IntrinsicReason("InvalidArguments", "ABS", 2), IntrinsicReason("InvalidArguments", "ABS", 3), "Intrinsic"));
            Assert.True(RepairReasonMatches(IntrinsicReason("NonCanonicalForm"), IntrinsicReason("NonCanonicalForm"), "Intrinsic"));
            Assert.False(RepairReasonMatches(IntrinsicReason("NonCanonicalForm"), IntrinsicReason("InvalidArguments", "ABS", 2), "Intrinsic"));
            Assert.True(RepairReasonMatches(DomainReason(1, "uno"), DomainReason(999, "different"), "Domain"));
            Assert.False(RepairReasonMatches(DomainReason(1, "uno"), HealthyReason(), "Domain"));
        }

        [Fact, Trait("Gate", "G9-Contract-Mapping")]
        public void A3_R2_MAPPING_IS_EXACT_AND_EXCLUDES_T_A3_48()
        {
            var actual = GetType().GetMethods().Select(method => method.Name).Where(name => name.StartsWith("T_A3_"))
                .Select(name => int.Parse(name.Split('_')[2])).OrderBy(value => value).ToArray();
            Assert.Equal(new[] {17,18,19,20,21,35,36,37,38,39,40,44,45,46,47,49,50,51,52,53,54,55,56,57,58,59,60,64}, actual);
            Assert.DoesNotContain(48, actual);
        }

        private static void AssertRootChange(bool grow)
        {
            var a = Broken(IdA, M1); var b = Broken(IdB, M2);
            var before = Failure(IdD, a, grow ? Array.Empty<RootSignature>() : new[] { b });
            var after = Failure(IdD, a, grow ? new[] { b } : Array.Empty<RootSignature>());
            Assert.False(SymbolMatches(before, after)); Assert.False(UpstreamMatches(Read(IdD, before), Read(IdD, after)));
        }

        private static RegistrySymbolResult Failure(string symbol, RootSignature first, params RootSignature[] rest)
            => Failed(new[] { Dependency(symbol, first, symbol) }, new[] { first }.Concat(rest).ToArray());
        private static Dictionary<SymbolId, RegistrySymbolResult> Read(string id, RegistrySymbolResult value)
            => new Dictionary<SymbolId, RegistrySymbolResult> { [SymbolId.ProjectVariable(id)] = value };
        private static void AssertBlocked(RecoveryView result, string reason) { Assert.False(result.IsCandidate); Assert.Contains(reason, result.Reasons); }
        private static string Ref(string id) => G8ContractTestSupport.Reference(id);
        private static string Num(double value) => G8ContractTestSupport.Number(value);
        private static string Add(string left, string right) => G8ContractTestSupport.Binary("add", left, right);
        private static string Div(double left, double right) => G8ContractTestSupport.Binary("div", Num(left), Num(right));
        private static string Div(double left, string right) => G8ContractTestSupport.Binary("div", Num(left), right);
        private static string Call(string token, params string[] args) => G8ContractTestSupport.Call(token, args);

        private static RecoveryView Assess(ProjectVariablesDocument registry, string primary, string sibling = null,
            string other = null, bool unreadable = false, bool duplicateOther = false, bool unknown = false, bool envelopes = false)
        {
            var entries = new List<ProjectVariableScanEntry> { View(Document(RackA, primary, sibling, unknown)) };
            if (other != null) { var doc = Document(RackB, other, null, false); entries.Add(View(doc, "D2", RackB)); if (duplicateOther) entries.Add(View(doc, "D3", RackB)); }
            if (unreadable) entries.Add(ProjectVariableScanEntry.SelectiveUnreadableDesign("D4", RackB));
            if (envelopes) { entries.Add(ProjectVariableScanEntry.UnreadableEnvelope("D2")); entries.Add(ProjectVariableScanEntry.UnreadableEnvelope("D1")); }
            return AssessRecovery(registry, entries, RackA, ProjectPropertyIds.SelectivePalletToleranceToken);
        }

        private static SelectivePalletDesignDocument Document(string rackId, string primary, string sibling, bool unknown)
        {
            var document = Design(rackId: rackId); document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument> { [ProjectPropertyIds.SelectivePalletToleranceToken] = G8ContractTestSupport.ExpressionPropertyDocument(primary) };
            if (sibling != null) document.PropertyValues[ProjectPropertyIds.SelectiveVerticalClearanceToken] = G8ContractTestSupport.ExpressionPropertyDocument(sibling);
            if (unknown) document.PropertyValues["unknown.property"] = G8ContractTestSupport.ExpressionPropertyDocument(Num(1));
            return document;
        }
    }
}
