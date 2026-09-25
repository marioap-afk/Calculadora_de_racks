using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14 (I, J, K): the authored authority gate consumes the AUTH-13 comparison of I-58 and turns it into
    /// product policy. It never compares authored designs itself.
    /// </summary>
    public class RackSiblingAuthoredGateTests
    {
        [Fact]
        public void G14_I_A_SINGLE_AUTHORED_AUTHORITY_PROCEEDS()
        {
            var result = RackSiblingAuthoredGate.Evaluate(new StubComparator(RackAuthoredComparisonOutcome.Single), "input");

            Assert.True(result.Proceeds);
            Assert.Equal(RackProjectionFailureCode.None, result.Failure);
        }

        [Fact]
        public void G14_J_A_DIVERGENT_AUTHORED_AUTHORITY_FAILS_CLOSED()
        {
            var result = RackSiblingAuthoredGate.Evaluate(
                new StubComparator(RackAuthoredComparisonOutcome.Divergent, "two designs"), "input");

            Assert.False(result.Proceeds);
            Assert.Equal(RackSiblingAuthoredGateOutcome.Divergent, result.Outcome);
            Assert.Equal(RackProjectionFailureCode.AuthoredDivergent, result.Failure);
            Assert.Equal("two designs", result.Diagnostic);
        }

        [Fact]
        public void G14_K_AN_UNREADABLE_AUTHORED_AUTHORITY_FAILS_CLOSED()
        {
            var result = RackSiblingAuthoredGate.Evaluate(
                new StubComparator(RackAuthoredComparisonOutcome.Unreadable, "payload"), "input");

            Assert.False(result.Proceeds);
            Assert.Equal(RackSiblingAuthoredGateOutcome.Unreadable, result.Outcome);
            Assert.Equal(RackProjectionFailureCode.AuthoredUnreadable, result.Failure);
        }

        [Fact]
        public void G14_THE_GATE_MAPS_EVERY_FOUNDATION_OUTCOME_WITHOUT_INVENTING_ONE()
        {
            Assert.True(RackSiblingAuthoredGate.From(RackAuthoredComparisonOutcome.Single).Proceeds);
            Assert.Equal(
                RackProjectionFailureCode.AuthoredDivergent,
                RackSiblingAuthoredGate.From(RackAuthoredComparisonOutcome.Divergent).Failure);
            Assert.Equal(
                RackProjectionFailureCode.AuthoredUnreadable,
                RackSiblingAuthoredGate.From(RackAuthoredComparisonOutcome.Unreadable).Failure);
        }

        private sealed class StubComparator : IRackAuthoredComparatorPort<string, string>
        {
            private readonly RackAuthoredComparisonOutcome outcome;
            private readonly string diagnostic;

            internal StubComparator(RackAuthoredComparisonOutcome outcome, string diagnostic = null)
            {
                this.outcome = outcome;
                this.diagnostic = diagnostic;
            }

            public string Kind => "selective";

            public RackAuthoredComparisonResult<string> Compare(string input)
            {
                switch (outcome)
                {
                    case RackAuthoredComparisonOutcome.Single:
                        return RackAuthoredComparisonResult<string>.Single("authored");
                    case RackAuthoredComparisonOutcome.Divergent:
                        return RackAuthoredComparisonResult<string>.Divergent(diagnostic);
                    default:
                        return RackAuthoredComparisonResult<string>.Unreadable(diagnostic);
                }
            }
        }
    }
}
