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
            // T-A3-37: a non-simple SCC whose only source root is CycleRoot[A,B].
            var result = Assess(Registry(Expression(IdA, "A", Add(Ref(IdA), Ref(IdB))), Expression(IdB, "B", Ref(IdA))), Ref(IdA));
            AssertBlocked(result, "NonSimpleCycle");
            Assert.Equal(new[] { "NonSimpleCycle" }, result.Reasons);
            Assert.Single(result.SourceRoots);
            Assert.Contains(IdA, result.SourceRoots[0]); Assert.Contains(IdB, result.SourceRoots[0]);
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
            // T-A3-39, pre-state: A is the known root, so D's DivisionByZero is latent.
            var registry = Registry(Expression(IdA, "A", Ref(M1)), Expression(IdD, "D", Add(Ref(IdA), Div(1, 0))));
            var result = G8ContractTestSupport.Evaluate(registry).Results[SymbolId.ProjectVariable(IdD)];
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.DivisionByZero);
            Assert.True(Assess(registry, Add(Ref(IdD), Num(2))).IsCandidate);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_39_CORRECTION_MAKES_LATENT_DIVISION_REAL_AND_R1_REPORTS_IT()
        {
            // T-A3-39 attempted state: repairing A permits D to evaluate and exposes DivisionByZero.
            var registry = Registry(Expression(IdA, "A", Ref(M1)), Expression(IdD, "D", Add(Ref(IdA), Div(1, 0))));
            var attempt = ChangeDefinitionAttempt(registry, Id(IdA), VariableDefinition.Literal(1),
                new[] { View(Document(RackA, Add(Ref(IdD), Num(2)), null, false)) });
            Assert.False(attempt.Result.IsSuccess); Assert.True(attempt.Result.Plan.IsEmpty);
            Assert.Equal(RackA, attempt.RackId); Assert.Equal("DivisionByZero", attempt.Category);
            Assert.Equal(new[] { "DivisionByZero" }, attempt.DiagnosticCodes);
            Assert.False(attempt.HasRecoveryCandidate); Assert.Empty(attempt.PriorBlockingReasons);
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
        public void T_A3_45_DOMAIN_SIBLING_BLOCKS_WITHOUT_DISCOVERY_ABORT()
        {
            // T-A3-45 A: Domain is a semantic sibling blocker, not an indeterminate probe.
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Add(Ref(IdA), Num(1)), Num(-1));
            AssertBlocked(result, "OtherInvalidSources"); Assert.DoesNotContain("DiscoveryIndeterminate", result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_45_STRUCTURAL_SIBLING_BLOCKS_AND_ABORTS_DISCOVERY()
        {
            // T-A3-45 B: FatalUnknownProperty makes the rack Blocked and the probe Indeterminate.
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Add(Ref(IdA), Num(1)), unknown: true);
            AssertBlocked(result, "OtherInvalidSources"); Assert.Contains("DiscoveryIndeterminate", result.Reasons);
            Assert.Equal("Blocked", result.RackRepairability); Assert.False(result.HasRepairPlan);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_45_OTHER_RACK_MISSINGTARGET_BLOCKS_WITHOUT_DISCOVERY_ABORT()
        {
            // T-A3-45 C: K2 consumes H but has a repairable MissingTarget source.
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Add(Ref(IdA), Num(1)),
                other: Add(Ref(IdA), Ref(M2)), otherRackId: "K2");
            AssertBlocked(result, "OtherInvalidSources"); Assert.DoesNotContain("DiscoveryIndeterminate", result.Reasons);
            Assert.Contains(result.ReasonData, value => value.Contains("K2") && value.Contains(M2));
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
        public void T_A3_49_F_PERSPECTIVE_HAS_ONLY_SEVERAL_UNITS()
        {
            // T-A3-49, f perspective: s3's {A1} is contained in f's {A1,A2}.
            var result = Assess(Registry(Expression(IdA, "A1", Ref(M1)), Expression(IdB, "A2", Ref(M2))), Add(Ref(IdA), Ref(IdB)), other: Ref(IdA));
            Assert.False(result.IsCandidate); Assert.Equal(new[] { "SeveralRecoveryUnits" }, result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_49_S3_PERSPECTIVE_IS_BLOCKED_BY_F_IN_OTHER_RACK()
        {
            // T-A3-49, s3 perspective: f adds A2 outside s3's candidate set {A1}.
            var registry = Registry(Expression(IdA, "A1", Ref(M1)), Expression(IdB, "A2", Ref(M2)));
            var entries = new[]
            {
                View(Document(RackA, Add(Ref(IdA), Ref(IdB)), null, false)),
                View(Document(RackB, Ref(IdA), null, false), "D2", RackB),
            };
            var result = AssessEntries(registry, entries, RackB);
            AssertBlocked(result, "OtherInvalidSources"); Assert.DoesNotContain("SeveralRecoveryUnits", result.Reasons);
            Assert.Contains(result.ReasonData, value => value.Contains(RackA) && value.Contains(IdB));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_50_R1_REJECTION_EXPOSES_EXACT_ATTEMPTED_OUTOFRANGE()
        {
            // T-A3-50: attempted-state failure is structured and separate from prior-state context.
            var attempt = ChangeDefinitionAttempt(
                Registry(Literal(IdA, "A", 1)), Id(IdA), VariableDefinition.Literal(-1),
                new[] { View(Document(RackA, Ref(IdA), null, false)) });
            Assert.False(attempt.Result.IsSuccess); Assert.True(attempt.Result.Plan.IsEmpty);
            Assert.Equal(RackA, attempt.RackId); Assert.Equal("OutOfRange", attempt.Category);
            Assert.Equal(new[] { "OutOfRange" }, attempt.DiagnosticCodes);
            Assert.False(attempt.HasRecoveryCandidate);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_50_PRIOR_BLOCKING_CONTEXT_IS_SEPARATE_FROM_ATTEMPT_FAILURE()
        {
            // T-A3-50/T-A3-44 variant: previous blockers remain context, never the attempted failure itself.
            var registry = Registry(Expression(IdA, "H", Ref(M1)), Expression(IdB, "G", Div(1, 0)));
            var attempt = ChangeDefinitionAttempt(registry, Id(IdA), VariableDefinition.Literal(1),
                new[] { View(Document(RackA, Add(Ref(IdA), Ref(IdB)), Add(Ref(IdA), Num(1)), false)) });
            Assert.False(attempt.Result.IsSuccess); Assert.True(attempt.Result.Plan.IsEmpty);
            Assert.Equal(RackA, attempt.RackId); Assert.Contains("OtherInvalidSources", attempt.PriorBlockingReasons);
            Assert.DoesNotContain("RecoveryCandidate", attempt.DiagnosticCodes); Assert.False(attempt.HasRecoveryCandidate);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_51_INDETERMINATE_DISCOVERY_BLOCKS_WITH_RACK_CAUSE()
        {
            // T-A3-51 A: rack probe failure is retained when no global envelope failure exists.
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Ref(IdA), unreadable: true);
            AssertBlocked(result, "DiscoveryIndeterminate"); Assert.Contains(result.ReasonData, value => value.Contains(RackB));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_51_GLOBAL_ENVELOPE_CAUSE_PRECEDES_AND_REPLACES_RACK_CAUSE()
        {
            // T-A3-51 envelope variant: the global precondition prevents a rack probe diagnosis.
            var entries = new[]
            {
                View(Document(RackA, Ref(IdA), null, false)),
                ProjectVariableScanEntry.SelectiveUnreadableDesign("K9-view", "K9"),
                ProjectVariableScanEntry.UnreadableEnvelope("D1"),
            };
            var result = AssessEntries(Registry(Expression(IdA, "H", Ref(M1))), entries, RackA);
            AssertBlocked(result, "DiscoveryIndeterminate");
            Assert.Single(result.AbortCauses); Assert.Equal("EnvelopeUnclassifiable(D1)", result.AbortCauses[0]);
            Assert.DoesNotContain(result.AbortCauses, cause => cause.Contains("K9"));
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
            // T-A3-55: the owner root and the simple-cycle root are distinct recovery units.
            var result = Assess(Registry(Expression(IdA, "A", Add(Ref(IdB), Call("ABS", Num(1), Num(2)))), Expression(IdB, "B", Ref(IdA))), Ref(IdB));
            Assert.False(result.IsCandidate);
            Assert.Equal(new[] { IdA, "CycleRoot[" + IdA + "," + IdB + "]" }, result.Units);
            Assert.Equal(2, result.SourceRoots.Length);
            Assert.Contains(result.SourceRoots, root => root.Contains(IdA) && root.Contains("InvalidArguments"));
            Assert.Contains(result.SourceRoots, root => root.Contains("CycleRoot") && root.Contains(IdA) && root.Contains(IdB));
            Assert.Contains("SeveralRecoveryUnits", result.Reasons); Assert.DoesNotContain("NonSimpleCycle", result.Reasons);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_56_BLOCKED_RACK_HAS_BOTH_STRUCTURED_REASONS()
        {
            // T-A3-56: structural sibling leaves no destructive repair plan.
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Ref(IdA), unknown: true);
            Assert.False(result.IsCandidate); Assert.Equal(new[] { "OtherInvalidSources", "DiscoveryIndeterminate" }, result.Reasons);
            Assert.Equal("Blocked", result.RackRepairability); Assert.False(result.HasRepairPlan);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_57_POSITIVELY_UNRELATED_FAILED_RACK_IS_IGNORED()
        {
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1)), Expression(IdB, "Z", Ref(M2))), Ref(IdA), other: Add(Ref(IdB), Ref(M3)));
            Assert.True(result.IsCandidate); Assert.DoesNotContain(result.ReasonData, value => value.Contains(RackB));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_58_REAL_NON_SINGLE_BUT_ZERO_POSITIVE_RACK_IS_UNRELATED()
        {
            // T-A3-58 positive contrast: K6 views differ structurally, but neither touches I(H).
            var v1 = Document("K6", Ref(IdB), Ref(M3), false);
            var v2 = Document("K6", Ref(IdB), null, false);
            Assert.False(EquivalentAuthored(v1, v2));
            var entries = new[] { View(Document(RackA, Ref(IdA), null, false)), View(v1, "K6-V1", "K6"), View(v2, "K6-V2", "K6") };
            var result = AssessEntries(Registry(Expression(IdA, "H", Ref(M1)), Expression(IdB, "Z", Ref(M2))), entries, RackA);
            Assert.True(result.IsCandidate); Assert.DoesNotContain("DiscoveryIndeterminate", result.Reasons);
            Assert.DoesNotContain("OtherInvalidSources", result.Reasons);
            Assert.DoesNotContain(result.ReasonData, value => value.Contains("K6"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_58_STRUCTURAL_ZERO_POSITIVE_RACK_IS_INDETERMINATE_NOT_UNRELATED()
        {
            // T-A3-58 negative contrast: an unknown source is unclassifiable even though it does not read H.
            var entries = new[]
            {
                View(Document(RackA, Ref(IdA), null, false)),
                View(Document("K6", Ref(IdB), null, true), "K6-V1", "K6"),
            };
            var result = AssessEntries(Registry(Expression(IdA, "H", Ref(M1)), Expression(IdB, "Z", Ref(M2))), entries, RackA);
            Assert.False(result.IsCandidate); Assert.Contains("DiscoveryIndeterminate", result.Reasons);
            Assert.Contains(result.AbortCauses, cause => cause.Contains("K6"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_59_A_STATIC_INVALID_ARGUMENTS_PRECEDES_LATENT_DIVISION()
        {
            // T-A3-59 A: 1/0 + ABS(1,2) is Intrinsic; numeric evaluation never starts.
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Add(Div(1, 0), Call("ABS", Num(1), Num(2))));
            Assert.Equal("Intrinsic", result.Classification);
            Assert.Contains(result.ReasonData, value => value.Contains("InvalidArguments") && value.Contains("ABS") && value.Contains("2"));
            Assert.DoesNotContain(result.ReasonData, value => value.Contains("DivisionByZero"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_59_B_STATIC_INVALID_ARGUMENTS_PRECEDES_UPSTREAM_AND_DIVISION()
        {
            // T-A3-59 B: H + 1/0 + ABS(1,2) remains Intrinsic, never Upstream.
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Add(Add(Ref(IdA), Div(1, 0)), Call("ABS", Num(1), Num(2))));
            Assert.Equal("Intrinsic", result.Classification);
            Assert.DoesNotContain(result.ReasonData, value => value.Contains("DivisionByZero") || value.Contains("Upstream"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_59_C_UPSTREAM_PRECEDES_LATENT_DIVISION_WITHOUT_STATIC_FAILURE()
        {
            // T-A3-59 C: H + 1/0 is Upstream(H); DivisionByZero is still unknown.
            var result = Assess(Registry(Expression(IdA, "H", Ref(M1))), Add(Ref(IdA), Div(1, 0)));
            Assert.Equal("Upstream", result.Classification); Assert.Equal(new[] { IdA }, result.Units);
            Assert.DoesNotContain(result.ReasonData, value => value.Contains("DivisionByZero"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_59_D_REGISTRY_SENTINEL_STATIC_ARITY_BLOCKS_NUMERIC_EVALUATION()
        {
            // T-A3-59 D mirrors G7 STATIC_ARITY_COEXISTS_WITH_KNOWN_FAILURES_AND_BLOCKS_NUMERIC_EVALUATION.
            var evaluation = G8ContractTestSupport.Evaluate(Registry(Expression(IdC, "S", Add(Div(1, 0), Call("ABS", Num(1), Num(2))))));
            var result = evaluation.Results[SymbolId.ProjectVariable(IdC)];
            Assert.Equal(ExpressionDiagnosticCode.InvalidArguments, Assert.Single(result.Diagnostics).Code);
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
        public void T_A3_64_A_ENVELOPES_ARE_DEDUPLICATED_ORDERED_AND_HIDE_RACK_CAUSE()
        {
            // T-A3-64(a), envelope branch: global causes precede and replace rack probe causes.
            var registry = Registry(Expression(IdA, "H", Ref(M1)));
            var entries = new[]
            {
                ProjectVariableScanEntry.UnreadableEnvelope("D2"),
                View(Document(RackA, Ref(IdA), null, false)),
                View(Document("k9", Ref(IdA), null, true), "k9-v", "k9"),
                View(Document("K9", Ref(IdA), null, true), "K9-v", "K9"),
                ProjectVariableScanEntry.UnreadableEnvelope("D1"),
                ProjectVariableScanEntry.UnreadableEnvelope("D2"),
            };
            var result = AssessEntries(registry, entries, RackA);
            AssertBlocked(result, "DiscoveryIndeterminate");
            Assert.Equal(new[]
            {
                "EnvelopeUnclassifiable(D1){" + IdA + "}",
                "EnvelopeUnclassifiable(D2){" + IdA + "}",
            }, result.StructuredAbortCauses);
            Assert.DoesNotContain(result.StructuredAbortCauses, cause => cause.Contains("ProbeIndeterminate"));
            AssertPermutationInvariant(registry, entries, RackA);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_64_A_NO_ENVELOPE_USES_MINIMUM_ORDINAL_RACK_SPELLING_ONCE()
        {
            // T-A3-64(a), no-envelope branch: K9 is Ordinal-minimum over K9/k9.
            var registry = Registry(Expression(IdA, "H", Ref(M1)));
            var entries = new[]
            {
                View(Document(RackA, Ref(IdA), null, false)),
                View(Document("k9", Ref(IdB), null, true), "k9-v", "k9"),
                View(Document("K9", Ref(IdB), null, true), "K9-v", "K9"),
            };
            var result = AssessEntries(registry, entries, RackA);
            Assert.Equal(new[] { "ProbeIndeterminate(K9){" + IdA + "}" }, result.StructuredAbortCauses);
            Assert.DoesNotContain(result.StructuredAbortCauses, cause => cause.Contains("EnvelopeUnclassifiable"));
            AssertPermutationInvariant(registry, entries, RackA);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_64_B_PARTIAL_G_PRECEDES_NON_SINGLE_H_WITH_EXACT_UNITS()
        {
            // T-A3-64(b): K7 V1 reads H/G; structurally different V2 reads H only.
            var registry = Registry(
                Expression(IdA, "H", Ref(M1)),
                Expression(IdB, "G", Div(1, 0)),
                Expression(IdC, "S", Add(Ref(IdA), Ref(IdB))));
            var v1 = Document("K7", Ref(IdA), Ref(IdB), false);
            var v2 = Document("K7", Ref(IdA), null, false);
            Assert.False(EquivalentAuthored(v1, v2));
            var entries = new[]
            {
                View(Document(RackA, Add(Ref(IdC), Num(1)), null, false)),
                View(v1, "K7-V1", "K7"), View(v2, "K7-V2", "K7"),
            };
            var result = AssessEntries(registry, entries, RackA);
            Assert.Equal(new[] { "SeveralRecoveryUnits", "DiscoveryIndeterminate" }, result.Reasons);
            Assert.Equal(new[]
            {
                "PartialPositives(K7){" + IdB + "}",
                "NonSingleAuthority(K7){" + IdA + "}",
            }, result.StructuredAbortCauses);
            AssertPermutationInvariant(registry, entries, RackA);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void T_A3_64_C_TWO_POSITIVE_DIFFERENT_VIEWS_ARE_NON_SINGLE_NOT_PARTIAL()
        {
            // T-A3-64(c): both K8 views touch I(H), so the only cause is non-Single authority.
            var registry = Registry(Expression(IdA, "H", Ref(M1)), Expression(IdC, "S", Add(Ref(IdA), Num(1))));
            var v1 = Document("K8", Ref(IdA), null, false);
            var v2 = Document("K8", Ref(IdC), null, false);
            Assert.False(EquivalentAuthored(v1, v2));
            var entries = new[]
            {
                View(Document(RackA, Ref(IdA), null, false)),
                View(v1, "K8-V1", "K8"), View(v2, "K8-V2", "K8"),
            };
            var result = AssessEntries(registry, entries, RackA);
            Assert.Equal(new[] { "NonSingleAuthority(K8){" + IdA + "}" }, result.StructuredAbortCauses);
            Assert.DoesNotContain(result.StructuredAbortCauses, cause => cause.Contains("PartialPositives"));
            AssertPermutationInvariant(registry, entries, RackA);
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
                .Select(name => int.Parse(name.Split('_')[2])).Distinct().OrderBy(value => value).ToArray();
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
            string other = null, bool unreadable = false, bool duplicateOther = false, bool unknown = false,
            bool envelopes = false, string otherRackId = null, string other2 = null)
        {
            var entries = new List<ProjectVariableScanEntry> { View(Document(RackA, primary, sibling, unknown)) };
            var otherId = otherRackId ?? RackB;
            if (other != null)
            {
                var doc = Document(otherId, other, null, false); entries.Add(View(doc, "D2", otherId));
                if (duplicateOther) entries.Add(View(Document(otherId, other2 ?? other, null, false), "D3", otherId));
            }
            if (unreadable) entries.Add(ProjectVariableScanEntry.SelectiveUnreadableDesign("D4", RackB));
            if (envelopes) { entries.Add(ProjectVariableScanEntry.UnreadableEnvelope("D2")); entries.Add(ProjectVariableScanEntry.UnreadableEnvelope("D1")); }
            return AssessEntries(registry, entries, RackA);
        }

        private static RecoveryView AssessEntries(ProjectVariablesDocument registry, IReadOnlyList<ProjectVariableScanEntry> entries, string rackId)
            => AssessRecovery(registry, entries, rackId, ProjectPropertyIds.SelectivePalletToleranceToken);

        private static bool EquivalentAuthored(SelectivePalletDesignDocument left, SelectivePalletDesignDocument right)
            => left.PropertyValues.Count == right.PropertyValues.Count
                && left.PropertyValues.Keys.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(right.PropertyValues.Keys.OrderBy(value => value, StringComparer.Ordinal));

        private static void AssertPermutationInvariant(
            ProjectVariablesDocument registry,
            IReadOnlyList<ProjectVariableScanEntry> entries,
            string rackId)
        {
            var baseline = AssessEntries(registry, entries, rackId).StableSignature;
            var permutations = new[]
            {
                entries.Reverse().ToArray(),
                entries.OrderBy(entry => entry.DefinitionId, StringComparer.Ordinal).ToArray(),
                entries.Skip(1).Concat(entries.Take(1)).ToArray(),
            };
            Assert.All(permutations, permutation =>
                Assert.Equal(baseline, AssessEntries(registry, permutation, rackId).StableSignature));
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
