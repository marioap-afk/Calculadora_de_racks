using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// Product acceptance of AUTH-08 V2 facts. I-55 decides which classified transforms ID19 admits; it never
    /// parses placement values, decomposes transforms, extracts normals or canonicalises angles on its own.
    /// </summary>
    public sealed class RackSourcePlacementAcceptance
    {
        private RackSourcePlacementAcceptance(
            bool isAccepted,
            RackProjectionFailureCode failure,
            RackSourceTransformFactsV2 facts,
            Transform2D linear)
        {
            IsAccepted = isAccepted;
            Failure = failure;
            Facts = facts;
            Linear = linear;
        }

        public bool IsAccepted { get; }
        public RackProjectionFailureCode Failure { get; }
        public RackSourceTransformFactsV2 Facts { get; }

        /// <summary>Planar linear part of the accepted source placement, built from foundation facts only.</summary>
        public Transform2D Linear { get; }

        public double RotationRadians => Linear.RotationAngle();

        internal static RackSourcePlacementAcceptance Accepted(RackSourceTransformFactsV2 facts, Transform2D linear) =>
            new RackSourcePlacementAcceptance(true, RackProjectionFailureCode.None, facts, linear);

        internal static RackSourcePlacementAcceptance Rejected(RackProjectionFailureCode failure) =>
            new RackSourcePlacementAcceptance(false, failure, null, Transform2D.Identity);
    }

    public static class RackProjectionSourceTransform
    {
        public static RackSourcePlacementAcceptance Accept(RackSourceTransformFactsResult factsResult)
        {
            if (!factsResult.HasFacts)
            {
                switch (factsResult.Outcome)
                {
                    case RackSourceTransformOutcome.NonFinite:
                        return RackSourcePlacementAcceptance.Rejected(RackProjectionFailureCode.SourceFactsNonFinite);
                    case RackSourceTransformOutcome.Degenerate:
                        return RackSourcePlacementAcceptance.Rejected(RackProjectionFailureCode.SourceFactsDegenerate);
                    default:
                        return RackSourcePlacementAcceptance.Rejected(
                            RackProjectionFailureCode.SourceFactsInvalidTolerance);
                }
            }

            var facts = factsResult.Facts;

            if (facts.ReferenceBasisIsReflectionXY)
            {
                return RackSourcePlacementAcceptance.Rejected(RackProjectionFailureCode.ReflectionNotAllowed);
            }

            if (facts.IsNegativeZ)
            {
                return RackSourcePlacementAcceptance.Rejected(RackProjectionFailureCode.NegativeZScale);
            }

            if (!facts.IsUniformScale)
            {
                return RackSourcePlacementAcceptance.Rejected(RackProjectionFailureCode.NonUniformScale);
            }

            if (!facts.IsUnitScale)
            {
                return RackSourcePlacementAcceptance.Rejected(RackProjectionFailureCode.NonUnitScale);
            }

            if (!facts.NormalIsWorldZ)
            {
                return RackSourcePlacementAcceptance.Rejected(RackProjectionFailureCode.NormalNotWorldZ);
            }

            // A planar half turn is the only admitted sign combination, and the Foundation already reports it.
            // It composes as one more rotation through the shared Transform2D, so no angle is canonicalised here.
            var rotation = Transform2D.Rotation(facts.RotationRadians);
            var linear = facts.HasPlanarHalfTurnSign
                ? rotation.Then(Transform2D.RotationDegrees(180.0))
                : rotation;

            return RackSourcePlacementAcceptance.Accepted(facts, linear);
        }

        /// <summary>World point of a local point of the source definition, from AUTH-08 V2 facts.</summary>
        public static Point2D ToWorld(RackSourcePlacementAcceptance accepted, Point2D localPoint)
        {
            var facts = accepted.Facts;
            var offset = accepted.Linear.Apply(new Vector2D(
                localPoint.X - facts.DefinitionOrigin.X,
                localPoint.Y - facts.DefinitionOrigin.Y));

            return new Point2D(
                facts.ReferencePosition.X + offset.X,
                facts.ReferencePosition.Y + offset.Y);
        }
    }
}
