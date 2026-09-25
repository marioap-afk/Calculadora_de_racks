using System;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    public enum RackSiblingAuthoredGateOutcome
    {
        Proceed,
        Divergent,
        Unreadable
    }

    public sealed class RackSiblingAuthoredGateResult
    {
        internal RackSiblingAuthoredGateResult(
            RackSiblingAuthoredGateOutcome outcome,
            RackProjectionFailureCode failure,
            string diagnostic)
        {
            Outcome = outcome;
            Failure = failure;
            Diagnostic = diagnostic;
        }

        public RackSiblingAuthoredGateOutcome Outcome { get; }
        public RackProjectionFailureCode Failure { get; }
        public string Diagnostic { get; }
        public bool Proceeds => Outcome == RackSiblingAuthoredGateOutcome.Proceed;
    }

    /// <summary>
    /// Authored authority gate for a projected group. It only reads the AUTH-13 comparison of the Foundation and
    /// turns it into product policy; it never compares authored designs itself.
    /// </summary>
    public static class RackSiblingAuthoredGate
    {
        public static RackSiblingAuthoredGateResult Evaluate<TInput, TAuthored>(
            IRackAuthoredComparatorPort<TInput, TAuthored> comparator,
            TInput input)
        {
            if (comparator == null) throw new ArgumentNullException(nameof(comparator));

            var comparison = comparator.Compare(input);
            return From(comparison.Outcome, comparison.Diagnostic);
        }

        public static RackSiblingAuthoredGateResult From(RackAuthoredComparisonOutcome outcome, string diagnostic = null)
        {
            switch (outcome)
            {
                case RackAuthoredComparisonOutcome.Single:
                    return new RackSiblingAuthoredGateResult(
                        RackSiblingAuthoredGateOutcome.Proceed, RackProjectionFailureCode.None, diagnostic);
                case RackAuthoredComparisonOutcome.Divergent:
                    return new RackSiblingAuthoredGateResult(
                        RackSiblingAuthoredGateOutcome.Divergent,
                        RackProjectionFailureCode.AuthoredDivergent,
                        diagnostic);
                default:
                    return new RackSiblingAuthoredGateResult(
                        RackSiblingAuthoredGateOutcome.Unreadable,
                        RackProjectionFailureCode.AuthoredUnreadable,
                        diagnostic);
            }
        }
    }
}
