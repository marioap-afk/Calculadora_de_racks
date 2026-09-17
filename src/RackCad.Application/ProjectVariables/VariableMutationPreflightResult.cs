using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>How the semantic preflight of a variable operation turned out.</summary>
    public enum VariableMutationOutcome
    {
        /// <summary>The operation can proceed, and the plan says exactly what it must do.</summary>
        Success = 1,

        /// <summary>It cannot proceed. The plan is empty.</summary>
        Error = 2,

        /// <summary>
        /// A delete that consumers prevent. Distinct from a plain error because the user can ACT on it: the
        /// racks in the way are listed.
        /// </summary>
        BlockedByConsumers = 3,
    }

    /// <summary>
    /// The output of the semantic half of a preflight: pure, and either a complete plan or nothing.
    ///
    /// <para>
    /// The physical half — resolving object ids, checking block definitions exist, opening the lock — runs
    /// afterwards and only on a success. Splitting them is not cosmetic: this half is verifiable in the Core
    /// suite and the other one is not, so everything that CAN be decided here is decided here.
    /// </para>
    /// </summary>
    public sealed class VariableMutationPreflightResult
    {
        private static readonly VariableConsumerSummary[] NoConsumers = new VariableConsumerSummary[0];

        private VariableMutationPreflightResult(
            VariableMutationOutcome outcome,
            MutationPlan plan,
            string error,
            IReadOnlyList<VariableConsumerSummary> blockingConsumers,
            AttemptedStateFailure attemptedStateFailure = null,
            IReadOnlyList<string> priorBlockingReasons = null)
        {
            Outcome = outcome;
            Plan = plan;
            Error = error;
            BlockingConsumers = blockingConsumers;
            AttemptedStateFailure = attemptedStateFailure;
            PriorBlockingReasons = priorBlockingReasons ?? new string[0];
            PresentationFailure = outcome == VariableMutationOutcome.Success
                ? null
                : new VariableMutationPresentationFailure(
                    attemptedStateFailure?.Category ?? outcome.ToString(),
                    attemptedStateFailure?.DiagnosticCodes ?? new[] { error ?? outcome.ToString() });
        }

        public VariableMutationOutcome Outcome { get; }

        /// <summary>What must change. <see cref="MutationPlan.Empty"/> on any failure, always.</summary>
        public MutationPlan Plan { get; }

        /// <summary>The visible reason. Null on success.</summary>
        public string Error { get; }

        /// <summary>The racks preventing a delete. Empty unless the outcome is <see cref="VariableMutationOutcome.BlockedByConsumers"/>.</summary>
        public IReadOnlyList<VariableConsumerSummary> BlockingConsumers { get; }

        public AttemptedStateFailure AttemptedStateFailure { get; }

        public IReadOnlyList<string> PriorBlockingReasons { get; }

        public VariableMutationPresentationFailure PresentationFailure { get; }

        public bool IsSuccess => Outcome == VariableMutationOutcome.Success;

        public static VariableMutationPreflightResult Success(MutationPlan plan)
            => new VariableMutationPreflightResult(VariableMutationOutcome.Success, plan, null, NoConsumers);

        public static VariableMutationPreflightResult Failed(string error)
            => new VariableMutationPreflightResult(VariableMutationOutcome.Error, MutationPlan.Empty, error, NoConsumers);

        internal static VariableMutationPreflightResult Failed(
            string error, AttemptedStateFailure attempted, IEnumerable<string> priorReasons)
            => new VariableMutationPreflightResult(
                VariableMutationOutcome.Error, MutationPlan.Empty, error, NoConsumers, attempted,
                (priorReasons ?? Enumerable.Empty<string>()).Distinct().OrderBy(value => value).ToArray());

        public static VariableMutationPreflightResult Blocked(
            string error,
            IReadOnlyList<VariableConsumerSummary> consumers)
            => new VariableMutationPreflightResult(
                VariableMutationOutcome.BlockedByConsumers,
                MutationPlan.Empty,
                error,
                consumers ?? NoConsumers);
    }

    /// <summary>The structured reason every rejected variable intent can pass to a presentation layer.</summary>
    public sealed class VariableMutationPresentationFailure
    {
        internal VariableMutationPresentationFailure(string category, IEnumerable<string> diagnosticCodes)
        {
            Category = category;
            DiagnosticCodes = (diagnosticCodes ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct()
                .ToArray();
        }

        public string Category { get; }
        public IReadOnlyList<string> DiagnosticCodes { get; }
    }

    /// <summary>The exact attempted rack state that made a definition change inadmissible.</summary>
    public sealed class AttemptedStateFailure
    {
        internal AttemptedStateFailure(string rackId, string category, IEnumerable<string> diagnosticCodes)
        {
            RackId = rackId;
            Category = category;
            DiagnosticCodes = (diagnosticCodes ?? Enumerable.Empty<string>()).Distinct().OrderBy(value => value).ToArray();
        }

        public string RackId { get; }
        public string Category { get; }
        public IReadOnlyList<string> DiagnosticCodes { get; }
        public bool HasRecoveryCandidate => false;
    }
}
