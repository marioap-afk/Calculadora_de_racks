using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;
using static RackCad.Tests.G9ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>I-49 G9-A sentinels for inherited I-47/I-48 mutation behavior. Every test starts GREEN.</summary>
    public sealed class G9LegacyMutationCharacterizationTests
    {
        [Fact, Trait("Gate", "G9-Legacy")]
        public void LITERAL_CREATE_REMAINS_REGISTRY_ONLY()
        {
            var result = ProjectVariableMutationPreflight.Create("A", VariableType.Length, VariableDefinition.Literal(4));
            Assert.True(result.IsSuccess);
            Assert.Equal(RegistryMutationKind.Add, result.Plan.RegistryMutation.Kind);
            Assert.Empty(result.Plan.RackMutations);
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void LITERAL_CHANGEVALUE_REDRAWS_DIRECT_CONSUMER_WITHOUT_REWRITING_AUTHORED_LITERAL()
        {
            var result = ProjectVariableMutationPreflight.ChangeValue(
                Registry(Literal(IdA, "A", 4)), Id(IdA), VariableDefinition.Literal(12),
                new[] { View(Design(directVariable: IdA)) });
            Assert.True(result.IsSuccess);
            var rack = Assert.Single(result.Plan.RackMutations);
            Assert.Equal(6, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(12, rack.EffectiveOutput.VerticalClearance);
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void RENAME_PRESERVES_ID_AND_DEFINITION_AND_HAS_NO_RACK_MUTATION()
        {
            var result = ProjectVariableMutationPreflight.Rename(Registry(Literal(IdA, "A", 4)), Id(IdA), "Renamed");
            Assert.True(result.IsSuccess);
            Assert.Equal(Id(IdA), result.Plan.RegistryMutation.VariableId);
            Assert.Equal(RegistryMutationKind.Rename, result.Plan.RegistryMutation.Kind);
            Assert.Empty(result.Plan.RackMutations);
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void DELETE_WITHOUT_DIRECT_CONSUMERS_REMAINS_ALLOWED()
        {
            var result = ProjectVariableMutationPreflight.Delete(
                Registry(Literal(IdA, "A", 4)), Id(IdA), new[] { View(Design()) });
            Assert.True(result.IsSuccess);
            Assert.Equal(RegistryMutationKind.Remove, result.Plan.RegistryMutation.Kind);
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void DELETE_WITH_DIRECT_CONSUMER_REMAINS_BLOCKED_WITH_EMPTY_PLAN()
        {
            var result = ProjectVariableMutationPreflight.Delete(
                Registry(Literal(IdA, "A", 4)), Id(IdA), new[] { View(Design(directVariable: IdA)) });
            Assert.Equal(VariableMutationOutcome.BlockedByConsumers, result.Outcome);
            AssertEmpty(result);
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void UNLINK_ALL_AND_DELETE_MATERIALIZES_DIRECT_REFERENCE_AT_CURRENT_VALUE()
        {
            var result = ProjectVariableMutationPreflight.UnlinkAllAndDelete(
                Registry(Literal(IdA, "A", 14)), Id(IdA), new[] { View(Design(directVariable: IdA)) });
            Assert.True(result.IsSuccess);
            var rack = Assert.Single(result.Plan.RackMutations);
            Assert.False(rack.AuthoredOutput.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance));
            Assert.Equal(14, rack.AuthoredOutput.VerticalClearance);
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void REPAIR_BROKEN_RACK_REMOVES_CONFIRMED_MISSING_DIRECT_REFERENCE()
        {
            var result = ProjectVariableMutationPreflight.RepairBrokenRack(
                Registry(), RackA, new[] { View(Design(directVariable: MissingId)) }, confirmed: true);
            Assert.True(result.IsSuccess);
            Assert.False(Assert.Single(result.Plan.RackMutations).AuthoredOutput
                .HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance));
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void ONE_BAD_RACK_LEAVES_THE_WHOLE_CHANGE_PLAN_EMPTY()
        {
            var entries = new[]
            {
                View(Design(directVariable: IdA)),
                ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", RackB),
            };
            AssertEmpty(ProjectVariableMutationPreflight.ChangeValue(
                Registry(Literal(IdA, "A", 4)), Id(IdA), VariableDefinition.Literal(12), entries));
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void REGISTRY_COMMIT_REACCREDITS_AND_BLOCKS_DUPLICATE_ID_BEFORE_APPLY()
        {
            var duplicate = Registry(Literal(IdA, "A", 4), Literal(IdA, "Again", 9));
            var prepared = RegistryCommit.Prepare(
                RegistryMutation.ChangeValue(Id(IdA), VariableDefinition.Literal(12)),
                ProjectVariablesReadResult.Readable(duplicate));
            Assert.True(prepared.IsBlocked);
            Assert.Null(prepared.Changed);
        }

        [Fact, Trait("Gate", "G9-Legacy")]
        public void RACK_ONLY_COMMIT_DOES_NOT_READ_A_REGISTRY_IT_DOES_NOT_USE()
        {
            var prepared = RegistryCommit.Prepare(RegistryMutation.None, null);
            Assert.Equal(RegistryCommitOutcome.Unchanged, prepared.Outcome);
        }
    }
}
