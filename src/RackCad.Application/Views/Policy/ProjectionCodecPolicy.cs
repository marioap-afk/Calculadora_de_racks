using System;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Policy
{
    public enum ProjectionPolicyDecisionKind
    {
        Reject,
        Accept,
        AcceptCanonicalized
    }

    public enum ProjectionPolicyReasonCode
    {
        None,
        InvalidSyntax,
        CoercedLegacyFormNotAllowed,
        UnavailableForSystem,
        NotExposedForOperation,
        UnsupportedVariant,
        UnknownKind
    }

    public enum ProjectionPolicyRemedyCategory
    {
        None,
        UpdateRack,
        ChooseAnotherView,
        RepairLegacyRepresentation,
        Unsupported
    }

    public readonly struct ProjectionPolicyDecision
    {
        internal ProjectionPolicyDecision(
            ProjectionPolicyDecisionKind decision,
            RackViewAddress? canonicalAddress,
            RackViewSyntax? canonicalSyntax,
            ProjectionPolicyReasonCode reasonCode,
            ProjectionPolicyRemedyCategory remedyCategory)
        {
            Decision = decision;
            CanonicalAddress = canonicalAddress;
            CanonicalSyntax = canonicalSyntax;
            ReasonCode = reasonCode;
            RemedyCategory = remedyCategory;
        }

        public ProjectionPolicyDecisionKind Decision { get; }
        public RackViewAddress? CanonicalAddress { get; }
        public RackViewSyntax? CanonicalSyntax { get; }
        public ProjectionPolicyReasonCode ReasonCode { get; }
        public ProjectionPolicyRemedyCategory RemedyCategory { get; }
    }

    public static class ProjectionCodecPolicy
    {
        public static ProjectionPolicyDecision Evaluate(DecodedRackView decoded)
        {
            switch (decoded.Disposition)
            {
                case RackViewSyntaxDisposition.Canonical:
                    return decoded.HasAddress
                        ? Accepted(decoded, ProjectionPolicyDecisionKind.Accept)
                        : Reject(ProjectionPolicyReasonCode.InvalidSyntax,
                            ProjectionPolicyRemedyCategory.RepairLegacyRepresentation);
                case RackViewSyntaxDisposition.Canonicalizable:
                    return decoded.HasAddress
                        ? Accepted(decoded, ProjectionPolicyDecisionKind.AcceptCanonicalized)
                        : Reject(ProjectionPolicyReasonCode.InvalidSyntax,
                            ProjectionPolicyRemedyCategory.RepairLegacyRepresentation);
                case RackViewSyntaxDisposition.Coerced:
                    return Reject(
                        ProjectionPolicyReasonCode.CoercedLegacyFormNotAllowed,
                        ProjectionPolicyRemedyCategory.UpdateRack);
                case RackViewSyntaxDisposition.Invalid:
                    var kind = RackViewCodec.DecodeKind(decoded.OriginalSyntax.Kind);
                    return kind.Disposition == RackViewSyntaxDisposition.Invalid
                        ? Reject(ProjectionPolicyReasonCode.UnknownKind, ProjectionPolicyRemedyCategory.Unsupported)
                        : Reject(ProjectionPolicyReasonCode.InvalidSyntax,
                            ProjectionPolicyRemedyCategory.RepairLegacyRepresentation);
                default:
                    return Reject(ProjectionPolicyReasonCode.InvalidSyntax,
                        ProjectionPolicyRemedyCategory.RepairLegacyRepresentation);
            }
        }

        public static ProjectionPolicyDecision ForNewView(
            RackSystemKind systemKind,
            RackViewAddress address)
        {
            try
            {
                var syntax = RackViewCodec.Encode(systemKind, address);
                return Evaluate(RackViewCodec.Decode(syntax.Kind, syntax.View, syntax.Section));
            }
            catch (ArgumentException)
            {
                return Reject(
                    ProjectionPolicyReasonCode.UnsupportedVariant,
                    ProjectionPolicyRemedyCategory.Unsupported);
            }
        }

        private static ProjectionPolicyDecision Accepted(
            DecodedRackView decoded,
            ProjectionPolicyDecisionKind decision) => new ProjectionPolicyDecision(
                decision,
                decoded.Address,
                RackViewCodec.Encode(decoded.SystemKind, decoded.Address),
                ProjectionPolicyReasonCode.None,
                ProjectionPolicyRemedyCategory.None);

        internal static ProjectionPolicyDecision Reject(
            ProjectionPolicyReasonCode reasonCode,
            ProjectionPolicyRemedyCategory remedyCategory) => new ProjectionPolicyDecision(
            ProjectionPolicyDecisionKind.Reject,
            null,
            null,
            reasonCode,
            remedyCategory);
    }
}
