using System;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Preparation
{
    public readonly struct RackProductIntentAcceptanceResult<TIntent>
        where TIntent : class
    {
        internal RackProductIntentAcceptanceResult(
            TIntent intent,
            RackProductPrepareFailure failure,
            string causeCode,
            string diagnostic,
            ProjectionPolicyDecision policy)
        {
            Intent = intent;
            Failure = failure;
            CauseCode = causeCode;
            Diagnostic = diagnostic;
            Policy = policy;
        }

        public bool IsAccepted => Intent != null && Failure == RackProductPrepareFailure.None;
        public TIntent Intent { get; }
        public RackProductPrepareFailure Failure { get; }
        public string CauseCode { get; }
        public string Diagnostic { get; }
        public ProjectionPolicyDecision Policy { get; }
    }

    public static class RackProductIntentAcceptance
    {
        public static RackProductIntentAcceptanceResult<AcceptedNewRackViewIntent<TAuthored>> Accept<TAuthored>(
            NewRackViewIntent<TAuthored> intent)
        {
            if (intent?.Creation == null || !RackProductIntentValidation.Valid(intent.View))
                return Failed<AcceptedNewRackViewIntent<TAuthored>>(
                    RackProductPrepareFailure.UnsupportedIntent, "INTENT_NOT_SUPPORTED", null, default);

            var policy = Evaluate(intent.View);
            if (policy.Decision == ProjectionPolicyDecisionKind.Reject)
                return PolicyRejected<AcceptedNewRackViewIntent<TAuthored>>(policy);

            string rackId;
            try
            {
                rackId = intent.Creation.AcceptRackIdentity();
            }
            catch (Exception ex)
            {
                return Failed<AcceptedNewRackViewIntent<TAuthored>>(
                    RackProductPrepareFailure.InvalidIdentity, "RACK_ID_INVALID", ex.Message, policy);
            }

            return Accepted(new AcceptedNewRackViewIntent<TAuthored>(
                intent.Creation, intent.View, rackId, policy), policy);
        }

        public static RackProductIntentAcceptanceResult<AcceptedExistingRackViewIntent<TComparisonInput>> Accept<TComparisonInput>(
            ExistingRackViewIntent<TComparisonInput> intent)
        {
            if (intent?.Existing?.SourceEnvelope == null || !RackProductIntentValidation.Valid(intent.View))
                return Failed<AcceptedExistingRackViewIntent<TComparisonInput>>(
                    RackProductPrepareFailure.UnsupportedIntent, "INTENT_NOT_SUPPORTED", null, default);

            var policy = Evaluate(intent.View);
            if (policy.Decision == ProjectionPolicyDecisionKind.Reject)
                return PolicyRejected<AcceptedExistingRackViewIntent<TComparisonInput>>(policy);

            var source = intent.Existing.SourceEnvelope;
            if (string.IsNullOrWhiteSpace(source.Id)
                || !string.Equals(source.Kind, policy.CanonicalSyntax?.Kind, StringComparison.OrdinalIgnoreCase))
                return Failed<AcceptedExistingRackViewIntent<TComparisonInput>>(
                    RackProductPrepareFailure.InvalidIdentity, "RACK_ID_INVALID", null, policy);

            return Accepted(new AcceptedExistingRackViewIntent<TComparisonInput>(
                intent.Existing, intent.View, policy), policy);
        }

        private static ProjectionPolicyDecision Evaluate(RackProductViewRequest view)
            => ProjectionAvailabilityPolicy.EvaluateNewView(
                view.SystemKind,
                view.RequestedAddress,
                view.Availability,
                view.Operation);

        private static RackProductIntentAcceptanceResult<TIntent> Accepted<TIntent>(
            TIntent intent,
            ProjectionPolicyDecision policy)
            where TIntent : class
            => new RackProductIntentAcceptanceResult<TIntent>(
                intent, RackProductPrepareFailure.None, null, null, policy);

        private static RackProductIntentAcceptanceResult<TIntent> PolicyRejected<TIntent>(
            ProjectionPolicyDecision policy)
            where TIntent : class
            => Failed<TIntent>(
                RackProductPrepareFailure.PolicyRejected,
                policy.ReasonCode == ProjectionPolicyReasonCode.None
                    ? "INTENT_NOT_SUPPORTED"
                    : policy.ReasonCode.ToString(),
                null,
                policy);

        private static RackProductIntentAcceptanceResult<TIntent> Failed<TIntent>(
            RackProductPrepareFailure failure,
            string causeCode,
            string diagnostic,
            ProjectionPolicyDecision policy)
            where TIntent : class
            => new RackProductIntentAcceptanceResult<TIntent>(null, failure, causeCode, diagnostic, policy);
    }

    internal static class RackProductIntentValidation
    {
        internal static bool Valid(RackProductViewRequest view)
            => view != null
                && Enum.IsDefined(typeof(RackViewProductOperation), view.Operation)
                && (view.SystemKind == RackSystemKind.SelectiveRack
                    || view.SystemKind == RackSystemKind.PalletFlow
                    || view.SystemKind == RackSystemKind.PushBack
                    || view.SystemKind == RackSystemKind.Cantilever
                    || view.SystemKind == RackSystemKind.Selective
                    || view.SystemKind == RackSystemKind.Cama);
    }
}
