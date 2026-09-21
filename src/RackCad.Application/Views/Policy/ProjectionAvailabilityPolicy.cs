using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Policy
{
    public static class ProjectionAvailabilityPolicy
    {
        /// <summary>
        /// Evaluates a new-view intent from its typed address. Encoding is delegated to the Foundation codec, so a
        /// successful result always carries canonical persisted syntax.
        /// </summary>
        public static ProjectionPolicyDecision EvaluateNewView(
            RackSystemKind systemKind,
            RackViewAddress address,
            RackViewAvailabilityFacts facts,
            RackViewProductOperation operation)
        {
            var codecDecision = ProjectionCodecPolicy.ForNewView(systemKind, address);
            if (codecDecision.Decision == ProjectionPolicyDecisionKind.Reject)
            {
                return codecDecision;
            }

            var syntax = codecDecision.CanonicalSyntax.Value;
            return Evaluate(
                RackViewCodec.Decode(syntax.Kind, syntax.View, syntax.Section),
                facts,
                operation);
        }

        public static ProjectionPolicyDecision Evaluate(
            DecodedRackView decoded,
            RackViewAvailabilityFacts facts,
            RackViewProductOperation operation)
        {
            var codecDecision = ProjectionCodecPolicy.Evaluate(decoded);
            if (codecDecision.Decision == ProjectionPolicyDecisionKind.Reject)
            {
                return codecDecision;
            }

            var availability = RackViewAvailability.Evaluate(decoded, facts);
            switch (availability.Status)
            {
                case RackViewAvailabilityStatus.Available:
                    break;
                case RackViewAvailabilityStatus.VariantNotPresent:
                    return ProjectionCodecPolicy.Reject(
                        ProjectionPolicyReasonCode.UnsupportedVariant,
                        ProjectionPolicyRemedyCategory.ChooseAnotherView);
                case RackViewAvailabilityStatus.SystemDoesNotSupportKind:
                case RackViewAvailabilityStatus.Unavailable:
                default:
                    return ProjectionCodecPolicy.Reject(
                        ProjectionPolicyReasonCode.UnavailableForSystem,
                        ProjectionPolicyRemedyCategory.Unsupported);
            }

            if (!RackViewExposure.IsExposed(
                    decoded.SystemKind,
                    codecDecision.CanonicalAddress.Value,
                    operation))
            {
                return ProjectionCodecPolicy.Reject(
                    ProjectionPolicyReasonCode.NotExposedForOperation,
                    ProjectionPolicyRemedyCategory.Unsupported);
            }

            return codecDecision;
        }
    }
}
