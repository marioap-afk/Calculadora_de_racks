using System.Collections.Generic;
using System.Linq;
using Xunit;
using static RackCad.Tests.G9ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// Exact A3-R2 §11.2 allocation to G9. G10-only T-A3-48 is deliberately absent; G7 rows remain in Gate=G7.
    /// Each row names the product datum that must make the scenario distinguishable at preflight or commit.
    /// </summary>
    public sealed class G9A3ContractMatrixTests
    {
        private static readonly string[] ComparableSymbolCases =
        {
            "T-A3-17", "T-A3-18", "T-A3-19", "T-A3-35", "T-A3-38",
        };

        private static readonly string[] RepairDecisionCases =
        {
            "T-A3-20", "T-A3-53", "T-A3-60",
        };

        private static readonly string[] RecoveryCases =
        {
            "T-A3-21", "T-A3-36", "T-A3-37", "T-A3-39", "T-A3-40", "T-A3-44", "T-A3-45",
            "T-A3-46", "T-A3-47", "T-A3-49", "T-A3-50", "T-A3-51", "T-A3-52", "T-A3-54",
            "T-A3-55", "T-A3-56", "T-A3-57", "T-A3-58", "T-A3-59", "T-A3-64",
        };

        public static IEnumerable<object[]> ComparableSymbols()
            => ComparableSymbolCases.Select(value => new object[] { value });

        public static IEnumerable<object[]> RepairDecisions()
            => RepairDecisionCases.Select(value => new object[] { value });

        public static IEnumerable<object[]> RecoveryEligibility()
            => RecoveryCases.Select(value => new object[] { value });

        [Theory]
        [MemberData(nameof(ComparableSymbols))]
        [Trait("Gate", "G9-Contract")]
        public void A3_SYMBOL_RESULT_COMPARISON_CASE(string caseId)
            => RequireComparableObservationSurface(caseId);

        [Theory]
        [MemberData(nameof(RepairDecisions))]
        [Trait("Gate", "G9-Contract")]
        public void A3_REPAIR_DECISION_COMPARISON_CASE(string caseId)
            => RequireRepairDecisionSurface(caseId);

        [Theory]
        [MemberData(nameof(RecoveryEligibility))]
        [Trait("Gate", "G9-Contract")]
        public void A3_RECOVERY_ELIGIBILITY_AND_BLOCKING_DATA_CASE(string caseId)
            => RequireRecoverySurface(caseId);

        [Fact, Trait("Gate", "G9-Contract-Mapping")]
        public void A3_R2_G9_MAPPING_IS_COMPLETE_EXACT_AND_EXCLUDES_G10_ONLY_T_A3_48()
        {
            var actual = ComparableSymbolCases.Concat(RepairDecisionCases).Concat(RecoveryCases)
                .OrderBy(value => int.Parse(value.Substring("T-A3-".Length)))
                .ToArray();
            var expected = new[]
            {
                "T-A3-17", "T-A3-18", "T-A3-19", "T-A3-20", "T-A3-21", "T-A3-35", "T-A3-36",
                "T-A3-37", "T-A3-38", "T-A3-39", "T-A3-40", "T-A3-44", "T-A3-45", "T-A3-46",
                "T-A3-47", "T-A3-49", "T-A3-50", "T-A3-51", "T-A3-52", "T-A3-53", "T-A3-54",
                "T-A3-55", "T-A3-56", "T-A3-57", "T-A3-58", "T-A3-59", "T-A3-60", "T-A3-64",
            };
            Assert.Equal(expected, actual);
            Assert.DoesNotContain("T-A3-48", actual);
        }
    }
}
