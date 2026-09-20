using System;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.ProjectVariables;
using Xunit;
using static RackCad.Tests.G9ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>I-49 G9-A RED: definition-aware mutation, affected closure, discovery, repair and commit contract.</summary>
    public sealed class G9MutationPropagationContractTests
    {
        [Fact, Trait("Gate", "G9-Contract")]
        public void CHANGEDEFINITION_IS_THE_DEFINITION_AWARE_OPERATION_FOR_LITERAL_AND_EXPRESSION()
        {
            var literal = G9BehavioralContractSupport.ChangeDefinition(
                Registry(Literal(IdA, "A", 1)), Id(IdA), VariableDefinition.Literal(2),
                Array.Empty<ProjectVariableScanEntry>());
            var expression = G9BehavioralContractSupport.ChangeDefinition(
                Registry(Literal(IdA, "A", 1), Literal(IdB, "B", 2)), Id(IdA),
                ExpressionDefinition(BoundExpression.Reference(SymbolId.ProjectVariable(IdB))),
                Array.Empty<ProjectVariableScanEntry>());
            Assert.True(literal.IsSuccess); Assert.True(expression.IsSuccess);
            Assert.Equal(RegistryMutationKind.ChangeValue, literal.Plan.RegistryMutation.Kind);
            Assert.NotNull(expression.Plan.RegistryMutation.Definition);
        }

        [Theory]
        [InlineData("self", IdA, IdA)]
        [InlineData("two-cycle", IdA, IdB)]
        [InlineData("larger-cycle", IdA, IdC)]
        [Trait("Gate", "G9-Contract")]
        public void R2_REJECTS_A_NEW_CYCLE_WITH_AN_EMPTY_PLAN(string _, string changed, string dependency)
        {
            var registry = Registry(
                Literal(IdA, "A", 1),
                Expression(IdB, "B", G8ContractTestSupport.Reference(IdA)),
                Expression(IdC, "C", G8ContractTestSupport.Reference(IdB)));
            var result = ProjectVariableMutationPreflight.ChangeValue(
                registry, Id(changed), ExpressionDefinition(BoundExpression.Reference(SymbolId.ProjectVariable(dependency))),
                Array.Empty<ProjectVariableScanEntry>());
            AssertEmpty(result);
            Assert.Contains("Cycle", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void R2_DOES_NOT_REJECT_AN_UNCHANGED_PREEXISTING_FAILURE_MERELY_FOR_REMAINING_FAILED()
        {
            var registry = Registry(
                Literal(IdA, "A", 1),
                Expression(IdB, "B", G8ContractTestSupport.Reference(MissingId)));
            var result = ProjectVariableMutationPreflight.ChangeValue(
                registry, Id(IdA), VariableDefinition.Literal(2), Array.Empty<ProjectVariableScanEntry>());
            Assert.True(result.IsSuccess);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void AFFECTED_CLOSURE_INCLUDES_TRANSITIVE_VARIABLE_DEPENDENTS_BUT_NOT_RACKS_AS_GRAPH_NODES()
        {
            var result = ProjectVariableMutationPreflight.ChangeValue(
                Registry(
                    Literal(IdA, "A", 1),
                    Expression(IdB, "B", G8ContractTestSupport.Reference(IdA)),
                    Expression(IdC, "C", G8ContractTestSupport.Reference(IdB))),
                Id(IdA), VariableDefinition.Literal(2),
                new[] { View(Design(directVariable: IdC)) });
            Assert.True(result.IsSuccess);
            Assert.Equal(RackA, Assert.Single(result.Plan.RackMutations).RackId);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void R3_REJECTS_WHEN_A_HEALTHY_DEPENDENT_BECOMES_FAILED_AFTER_CHANGE()
        {
            var result = ProjectVariableMutationPreflight.ChangeValue(
                Registry(
                    Literal(IdA, "A", 2),
                    Expression(IdB, "B", G8ContractTestSupport.Binary(
                        "div", G8ContractTestSupport.Number(1), G8ContractTestSupport.Reference(IdA)))),
                Id(IdA), VariableDefinition.Literal(0), Array.Empty<ProjectVariableScanEntry>());
            AssertEmpty(result);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void EXPRESSION_CONSUMER_PROBE_IS_POSITIVE_IFF_BOUND_DEPENDENCIES_INTERSECT_THE_CLOSURE()
        {
            var expression = BoundExpression.Reference(SymbolId.ProjectVariable(IdB));
            var document = Design(expression: expression);
            Assert.Equal(ConsumerProbeOutcome.Positive, ProjectVariableConsumerProbe.Probe(document, Id(IdB)));
            Assert.Equal(ConsumerProbeOutcome.Negative, ProjectVariableConsumerProbe.Probe(document, Id(IdA)));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void DISCOVERY_FINDS_A_RACK_THROUGH_A_PROPERTY_EXPRESSION_DEPENDENCY()
        {
            var expression = BoundExpression.Reference(SymbolId.ProjectVariable(IdB));
            var result = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[] { View(Design(expression: expression)) }, Id(IdB));
            Assert.True(result.IsSuccess);
            Assert.Equal(RackA, Assert.Single(result.Consumers).RackId);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void DISCOVERY_NEVER_TREATS_AN_INDETERMINATE_RACK_AS_UNRELATED()
        {
            var result = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[] { ProjectVariableScanEntry.SelectiveUnreadableDesign("D1", RackA) }, Id(IdA));
            Assert.Equal(ConsumerDiscoveryOutcome.Abort, result.Outcome);
            Assert.Empty(result.Consumers);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void DELETE_IS_BLOCKED_BY_A_VARIABLE_DEFINITION_DEPENDENT_WITHOUT_A_RACK_CONSUMER()
        {
            var result = ProjectVariableMutationPreflight.Delete(
                Registry(
                    Literal(IdA, "A", 1),
                    Expression(IdB, "B", G8ContractTestSupport.Reference(IdA))),
                Id(IdA), Array.Empty<ProjectVariableScanEntry>());
            Assert.Equal(VariableMutationOutcome.BlockedByConsumers, result.Outcome);
            AssertEmpty(result);
            Assert.Contains(IdB, result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void UNLINKALLANDDELETE_BLOCKS_INSTEAD_OF_SUBSTITUTING_INSIDE_A_PROPERTY_EXPRESSION()
        {
            var result = ProjectVariableMutationPreflight.UnlinkAllAndDelete(
                Registry(Literal(IdA, "A", 11)), Id(IdA),
                new[] { View(Design(expression: BoundExpression.Reference(SymbolId.ProjectVariable(IdA)))) });
            AssertEmpty(result);
            RequireNamedCapability("structured property-expression dependent block", "ExpressionDependent", "FormulaDependent");
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void UNLINKALLANDDELETE_BLOCKS_A_VARIABLE_DEFINITION_DEPENDENT_WITH_NO_PARTIAL_UNLINK()
        {
            var result = ProjectVariableMutationPreflight.UnlinkAllAndDelete(
                Registry(
                    Literal(IdA, "A", 11),
                    Expression(IdB, "B", G8ContractTestSupport.Reference(IdA))),
                Id(IdA), Array.Empty<ProjectVariableScanEntry>());
            AssertEmpty(result);
        }

        [Fact, Trait("Gate", "G9-Api-Smoke")]
        public void INSPECTBINDING_IS_THE_SINGLE_STRUCTURAL_MISSING_INCOMPATIBLE_INTRINSIC_UPSTREAM_DOMAIN_AUTHORITY()
            => RequireSemanticInspection();

        [Fact, Trait("Gate", "G9-Api-Smoke")]
        public void MUTATION_PLAN_CARRIES_SYMBOL_AND_REPAIR_OBSERVATIONS_WITH_EXPLICIT_PHASES()
            => RequirePlanReadSetSurface();

        [Fact, Trait("Gate", "G9-Contract")]
        public void COMMIT_REREADS_AND_COMPARES_PLAN_OBSERVATIONS_BEFORE_FIRST_WRITE()
        {
            var planned = ProjectVariableMutationPreflight.ChangeValue(
                Registry(Literal(IdA, "A", 1)), Id(IdA), VariableDefinition.Literal(2),
                new[] { View(Design(directVariable: IdA)) });
            Assert.True(planned.IsSuccess);
            var commit = G9BehavioralContractSupport.Commit(
                planned.Plan, Registry(Literal(IdA, "A", 3)),
                new[] { View(Design(directVariable: IdA)) }, G9BehavioralContractSupport.NewWriteSpy());
            Assert.False(commit.Succeeded);
            Assert.Equal(0, commit.RegistryWrites);
            Assert.Equal(0, commit.RackWrites);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void CREATE_OBSERVES_AFTER_THE_NEW_SYMBOL()
        {
            RequirePlanReadSetSurface();
            var create = ProjectVariableMutationPreflight.Create("A", VariableType.Length, VariableDefinition.Literal(1));
            Assert.True(create.IsSuccess);
            var observation = Assert.Single(SymbolObservations(create.Plan));
            Assert.True(IsObservation(observation, "After", create.Plan.RegistryMutation.VariableId.Value));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void RENAME_AND_SAFE_DELETE_HAVE_ZERO_VALUE_OBSERVATIONS()
        {
            RequirePlanReadSetSurface();
            var rename = ProjectVariableMutationPreflight.Rename(Registry(Literal(IdA, "A", 1)), Id(IdA), "B");
            var delete = ProjectVariableMutationPreflight.Delete(
                Registry(Literal(IdA, "A", 1)), Id(IdA), Array.Empty<ProjectVariableScanEntry>());
            Assert.True(rename.IsSuccess && delete.IsSuccess);
            Assert.All(new[] { rename.Plan, delete.Plan }, AssertZeroObservations);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void CHANGEDEFINITION_OBSERVES_BEFORE_AFFECTED_EXCEPT_X_AND_AFTER_AFFECTED_PLUS_FINAL_RACK_READS()
        {
            var unrelated = "44444444-5555-6666-7777-888888888888";
            var result = G9BehavioralContractSupport.ChangeDefinition(
                Registry(Literal(IdA, "X", 2),
                    Expression(IdB, "A", G8ContractTestSupport.Reference(IdA)),
                    Expression(IdC, "B", G8ContractTestSupport.Reference(IdB)),
                    Literal(unrelated, "U", 9)),
                Id(IdA), VariableDefinition.Literal(3),
                new[]
                {
                    View(Design(expression: BoundExpression.Reference(SymbolId.ProjectVariable(IdC)))),
                    View(Design(rackId: RackB, expression: BoundExpression.Reference(SymbolId.ProjectVariable(unrelated))), "D2", RackB),
                });
            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { IdB, IdC }, ObservationIds(result.Plan, "Before"));
            Assert.Equal(new[] { IdA, IdB, IdC }.OrderBy(value => value, StringComparer.Ordinal), ObservationIds(result.Plan, "After"));
            Assert.DoesNotContain(unrelated, ObservationIds(result.Plan, "Before").Concat(ObservationIds(result.Plan, "After")));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void UNLINKALLANDDELETE_OBSERVES_BEFORE_X_BUT_NEVER_AFTER_X()
        {
            RequirePlanReadSetSurface();
            var result = ProjectVariableMutationPreflight.UnlinkAllAndDelete(
                Registry(Literal(IdA, "A", 11)), Id(IdA), new[] { View(Design(directVariable: IdA)) });
            Assert.True(result.IsSuccess);
            AssertObservation(result.Plan, "Before", IdA);
            AssertNoObservation(result.Plan, "After", IdA);
            Assert.Equal(new[] { IdA }, ObservationIds(result.Plan, "Before"));
            Assert.Empty(ObservationIds(result.Plan, "After"));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void REPAIRBROKENRACK_ALWAYS_HAS_ONE_REPAIR_DECISION_OBSERVATION_PER_REMOVED_SOURCE()
        {
            RequirePlanReadSetSurface();
            var result = ProjectVariableMutationPreflight.RepairBrokenRack(
                Registry(), RackA, new[] { View(Design(directVariable: MissingId)) }, confirmed: true);
            Assert.True(result.IsSuccess);
            var readSet = PlanReadSet(result.Plan);
            var decisions = Items(Member(readSet, "RepairDecisionObservations", "RepairDecisions"));
            Assert.Single(decisions);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void UNRELATED_REGISTRY_CHANGE_OUTSIDE_OBSERVATIONS_DOES_NOT_ABORT_COMMIT()
        {
            var unrelated = "44444444-5555-6666-7777-888888888888";
            var planned = ProjectVariableMutationPreflight.ChangeValue(
                Registry(Literal(IdA, "A", 1), Literal(unrelated, "Z", 10)),
                Id(IdA), VariableDefinition.Literal(2), Array.Empty<ProjectVariableScanEntry>());
            var commit = G9BehavioralContractSupport.Commit(
                planned.Plan, Registry(Literal(IdA, "A", 1), Literal(unrelated, "Z", 99)),
                Array.Empty<ProjectVariableScanEntry>(), G9BehavioralContractSupport.NewWriteSpy());
            Assert.True(commit.Succeeded); Assert.Equal(1, commit.RegistryWrites);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void ZERO_OBSERVATION_RENAME_DOES_NOT_TRIGGER_SEMANTIC_REREAD()
        {
            var plan = ProjectVariableMutationPreflight.Rename(Registry(Literal(IdA, "A", 1)), Id(IdA), "B").Plan;
            var commit = G9BehavioralContractSupport.Commit(plan, Registry(Literal(IdA, "A", 1)),
                Array.Empty<ProjectVariableScanEntry>(), G9BehavioralContractSupport.NewWriteSpy());
            Assert.True(commit.Succeeded); Assert.Equal(0, commit.SemanticReads);
        }

        private static void AssertZeroObservations(MutationPlan plan)
        {
            var readSet = PlanReadSet(plan);
            Assert.Empty(Items(Member(readSet, "SymbolResultObservations", "SymbolObservations")));
            Assert.Empty(Items(Member(readSet, "RepairDecisionObservations", "RepairDecisions")));
        }

        private static void AssertObservation(MutationPlan plan, string phase, string id)
            => Assert.Contains(SymbolObservations(plan), item => IsObservation(item, phase, id));

        private static void AssertNoObservation(MutationPlan plan, string phase, string id)
            => Assert.DoesNotContain(SymbolObservations(plan), item => IsObservation(item, phase, id));

        private static System.Collections.Generic.IReadOnlyList<object> SymbolObservations(MutationPlan plan)
            => Items(Member(PlanReadSet(plan), "SymbolResultObservations", "SymbolObservations"));

        private static string[] ObservationIds(MutationPlan plan, string phase)
            => SymbolObservations(plan)
                .Where(item => string.Equals(Member(item, "Phase").ToString(), phase, StringComparison.OrdinalIgnoreCase))
                .Select(item => Member(item, "SymbolId", "Symbol").ToString())
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

        private static bool IsObservation(object item, string phase, string id)
            => string.Equals(Member(item, "Phase").ToString(), phase, StringComparison.OrdinalIgnoreCase)
                && Member(item, "SymbolId", "Symbol").ToString().IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0;

        private static object PlanReadSet(MutationPlan plan) => Member(plan, "PlanReadSet", "ReadSet");

        private static object Member(object target, params string[] names)
        {
            foreach (var name in names)
            {
                var property = target.GetType().GetProperty(name,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (property != null) return property.GetValue(target);
            }
            throw new Xunit.Sdk.XunitException("G9 projection: missing member " + string.Join("/", names) + ".");
        }

        private static System.Collections.Generic.IReadOnlyList<object> Items(object value)
            => ((System.Collections.IEnumerable)value).Cast<object>().ToArray();
    }
}
