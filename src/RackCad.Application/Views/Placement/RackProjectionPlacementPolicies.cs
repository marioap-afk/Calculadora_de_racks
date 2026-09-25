using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>One accepted source view with its target address and both semantic frames.</summary>
    public sealed class RackProjectionSourceView
    {
        public RackProjectionSourceView(
            string physicalKey,
            string rackId,
            RackViewAddress sourceAddress,
            RackViewFrame sourceFrame,
            RackViewAddress targetAddress,
            RackViewFrame targetFrame,
            RackSourcePlacementAcceptance placement)
        {
            PhysicalKey = physicalKey;
            RackId = rackId;
            SourceAddress = sourceAddress;
            SourceFrame = sourceFrame;
            TargetAddress = targetAddress;
            TargetFrame = targetFrame;
            Placement = placement;
        }

        public string PhysicalKey { get; }
        public string RackId { get; }
        public RackViewAddress SourceAddress { get; }
        public RackViewFrame SourceFrame { get; }
        public RackViewAddress TargetAddress { get; }
        public RackViewFrame TargetFrame { get; }
        public RackSourcePlacementAcceptance Placement { get; }
    }

    /// <summary>A linked view placement: identity is preserved, so it carries no new RackId.</summary>
    public sealed class RackProjectedPlacement
    {
        public RackProjectedPlacement(
            string physicalKey,
            string rackId,
            RackViewAddress targetAddress,
            Point3D position,
            double rotationRadians)
        {
            PhysicalKey = physicalKey;
            RackId = rackId;
            TargetAddress = targetAddress;
            Position = position;
            RotationRadians = rotationRadians;
        }

        public string PhysicalKey { get; }
        public string RackId { get; }
        public RackViewAddress TargetAddress { get; }
        public Point3D Position { get; }
        public double RotationRadians { get; }
        public double Scale => 1.0;
    }

    /// <summary>Same class: the anchor layout is reproduced with the single common transform (alpha = 0).</summary>
    public static class RackRigidPlacementPolicy
    {
        public static RackProjectedPlacement Place(RackProjectionSourceView view, CommonTransform2D transform)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (transform == null) throw new ArgumentNullException(nameof(transform));

            var sourceAnchor = RackProjectionSourceTransform.ToWorld(
                view.Placement, RackViewFrameSemantics.Anchor(view.SourceFrame));
            var targetAnchor = transform.Apply(sourceAnchor);
            var rho = view.Placement.RotationRadians + transform.AlphaRadians;
            var targetLocal = RackViewFrameSemantics.Anchor(view.TargetFrame);
            var rotated = Transform2D.Rotation(rho).Apply(new Vector2D(targetLocal.X, targetLocal.Y));
            var elevation = transform.TargetPoint.Z
                + (view.Placement.Facts.ReferencePosition.Z - transform.BasePoint.Z);

            return new RackProjectedPlacement(
                view.PhysicalKey,
                view.RackId,
                view.TargetAddress,
                new Point3D(targetAnchor.X - rotated.X, targetAnchor.Y - rotated.Y, elevation),
                rho);
        }
    }

    /// <summary>Per reference facts of the orthographic stage, all independent of the picked points.</summary>
    public sealed class RackOrthographicReference
    {
        public RackOrthographicReference(
            RackProjectionSourceView view,
            double sourceCoordinate,
            double spanLength,
            int sourceSign,
            Point2D targetLocalAnchor)
        {
            View = view;
            SourceCoordinate = sourceCoordinate;
            SpanLength = spanLength;
            SourceSign = sourceSign;
            TargetLocalAnchor = targetLocalAnchor;
        }

        public RackProjectionSourceView View { get; }

        /// <summary>p_r: coordinate of the anchored span end over the common line, independent of the base point.</summary>
        public double SourceCoordinate { get; }
        public double SpanLength { get; }
        public int SourceSign { get; }
        public Point2D TargetLocalAnchor { get; }
    }

    public sealed class RackOrthographicProjection
    {
        public RackOrthographicProjection(
            Vector2D commonDirection,
            Vector2D targetDirection,
            double alphaRadians,
            double targetRotationRadians,
            bool nearWindowLimit,
            IReadOnlyList<RackOrthographicReference> references)
        {
            CommonDirection = commonDirection;
            TargetDirection = targetDirection;
            AlphaRadians = alphaRadians;
            TargetRotationRadians = targetRotationRadians;
            NearWindowLimit = nearWindowLimit;
            References = references;
        }

        /// <summary>e: oriented common line of the sources, chosen by the Relative Frame Window rule.</summary>
        public Vector2D CommonDirection { get; }

        /// <summary>w: direction of the conserved axis in the target views.</summary>
        public Vector2D TargetDirection { get; }

        public double AlphaRadians { get; }

        /// <summary>rho: orientation of every projected view, frozen to the target frame (OD-6.b A, OD-6.d A).</summary>
        public double TargetRotationRadians { get; }

        public bool NearWindowLimit { get; }
        public IReadOnlyList<RackOrthographicReference> References { get; }
    }

    public readonly struct RackOrthographicProjectionResult
    {
        private RackOrthographicProjectionResult(RackOrthographicProjection projection, RackProjectionFailureCode failure)
        {
            Projection = projection;
            Failure = failure;
        }

        public RackOrthographicProjection Projection { get; }
        public RackProjectionFailureCode Failure { get; }
        public bool IsAvailable => Failure == RackProjectionFailureCode.None;

        internal static RackOrthographicProjectionResult Available(RackOrthographicProjection projection) =>
            new RackOrthographicProjectionResult(projection, RackProjectionFailureCode.None);

        internal static RackOrthographicProjectionResult Unavailable(RackProjectionFailureCode failure) =>
            new RackOrthographicProjectionResult(null, failure);
    }

    /// <summary>
    /// Planta against elevations. Owner decision OD-7.e A froze the Relative Frame Window rule: the sense of the
    /// common line is read from the source frame of the family, never from a majority of references.
    /// </summary>
    public static class RackOrthographicPlacementPolicy
    {
        /// <summary>Half open window of exactly pi, so it always contains one of c and -c.</summary>
        private const double WindowLowerBound = -Math.PI / 4.0;
        private const double WindowUpperBound = 3.0 * Math.PI / 4.0;
        private const double NearLimitRadians = Math.PI / 180.0;

        public static RackOrthographicProjectionResult Project(
            IReadOnlyList<RackProjectionSourceView> views,
            RackPhysicalAxis conservedAxis,
            RackProjectionFamily family,
            SourceGroupFrame sourceFrame,
            TargetGroupFrame targetFrame)
        {
            if (views == null) throw new ArgumentNullException(nameof(views));
            if (views.Count == 0) return RackOrthographicProjectionResult.Unavailable(RackProjectionFailureCode.FrameUnavailable);

            var sourceAxes = new List<Vector2D>(views.Count);
            var worldAxes = new List<Vector2D>(views.Count);
            var targetAxes = new List<Vector2D>(views.Count);

            foreach (var view in views)
            {
                if (!RackViewFrameSemantics.TryAxisDirection(view.SourceFrame, conservedAxis, out var sourceAxis)
                    || !RackViewFrameSemantics.TryAxisDirection(view.TargetFrame, conservedAxis, out var targetAxis))
                {
                    return RackOrthographicProjectionResult.Unavailable(RackProjectionFailureCode.FrameAxisMissing);
                }

                sourceAxes.Add(sourceAxis);
                worldAxes.Add(view.Placement.Linear.Apply(sourceAxis).Normalized());
                targetAxes.Add(Transform2D.Rotation(targetFrame.OrientationRadians).Apply(targetAxis).Normalized());
            }

            for (var index = 1; index < worldAxes.Count; index++)
            {
                if (Math.Abs(worldAxes[index].Cross(worldAxes[0])) > GeometryTolerance.Angle)
                {
                    return RackOrthographicProjectionResult.Unavailable(RackProjectionFailureCode.NonParallelSources);
                }
            }

            for (var index = 1; index < targetAxes.Count; index++)
            {
                if (Math.Abs(targetAxes[index].Cross(targetAxes[0])) > GeometryTolerance.Angle
                    || targetAxes[index].Dot(targetAxes[0]) < 0.0)
                {
                    return RackOrthographicProjectionResult.Unavailable(RackProjectionFailureCode.MixedFamilies);
                }
            }

            var aligned = Vector2D.Zero;
            foreach (var axis in worldAxes)
            {
                aligned += axis.Dot(worldAxes[0]) >= 0.0 ? axis : -axis;
            }

            var candidate = aligned.Normalized();
            var familyAxis = Transform2D.Rotation(sourceFrame.OrientationRadians).Apply(sourceAxes[0]).Normalized();
            var relative = AngleFrom(familyAxis, candidate);
            var insideWindow = relative >= WindowLowerBound - GeometryTolerance.Angle
                && relative < WindowUpperBound - GeometryTolerance.Angle;
            var commonDirection = insideWindow ? candidate : -candidate;
            var targetDirection = targetAxes[0];
            var alpha = AngleFrom(commonDirection, targetDirection);
            var nearLimit = Math.Abs(relative - (WindowUpperBound - GeometryTolerance.Angle)) < NearLimitRadians;

            var references = new List<RackOrthographicReference>(views.Count);
            for (var index = 0; index < views.Count; index++)
            {
                var view = views[index];
                if (!TrySpan(view, conservedAxis, out var spanMin, out var spanMax))
                {
                    return RackOrthographicProjectionResult.Unavailable(RackProjectionFailureCode.FrameAxisMissing);
                }

                var sign = worldAxes[index].Dot(commonDirection) >= 0.0 ? 1 : -1;
                var sourceKappa = sign > 0 ? spanMin : spanMax;
                if (!RackViewFrameSemantics.TryLocalPointAt(view.SourceFrame, conservedAxis, sourceKappa, out var sourceLocal)
                    || !RackViewFrameSemantics.TryLocalPointAt(view.TargetFrame, conservedAxis, spanMin, out var targetLocal))
                {
                    return RackOrthographicProjectionResult.Unavailable(RackProjectionFailureCode.FrameAxisMissing);
                }

                var world = RackProjectionSourceTransform.ToWorld(view.Placement, sourceLocal);
                var coordinate = (world.X * commonDirection.X) + (world.Y * commonDirection.Y);
                references.Add(new RackOrthographicReference(
                    view, coordinate, spanMax - spanMin, sign, targetLocal));
            }

            return RackOrthographicProjectionResult.Available(new RackOrthographicProjection(
                commonDirection,
                targetDirection,
                alpha,
                targetFrame.OrientationRadians,
                nearLimit,
                references));
        }

        public static RackProjectedPlacement Place(
            RackOrthographicProjection projection,
            RackOrthographicReference reference,
            Point3D basePoint,
            Point3D targetPoint)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            if (reference == null) throw new ArgumentNullException(nameof(reference));

            var baseCoordinate = (basePoint.X * projection.CommonDirection.X)
                + (basePoint.Y * projection.CommonDirection.Y);
            var offset = reference.SourceCoordinate - baseCoordinate;
            var anchorX = targetPoint.X + (offset * projection.TargetDirection.X);
            var anchorY = targetPoint.Y + (offset * projection.TargetDirection.Y);
            var rotated = Transform2D
                .Rotation(projection.TargetRotationRadians)
                .Apply(new Vector2D(reference.TargetLocalAnchor.X, reference.TargetLocalAnchor.Y));

            return new RackProjectedPlacement(
                reference.View.PhysicalKey,
                reference.View.RackId,
                reference.View.TargetAddress,
                new Point3D(anchorX - rotated.X, anchorY - rotated.Y, targetPoint.Z),
                projection.TargetRotationRadians);
        }

        /// <summary>The conserved span is measured by whichever frame maps K onto its own local X.</summary>
        private static bool TrySpan(
            RackProjectionSourceView view, RackPhysicalAxis axis, out double min, out double max)
            => RackViewFrameSemantics.TrySpanOn(view.TargetFrame, axis, out min, out max)
                || RackViewFrameSemantics.TrySpanOn(view.SourceFrame, axis, out min, out max);

        /// <summary>Signed angle from one unit direction to another, canonicalised by the shared Transform2D.</summary>
        private static double AngleFrom(Vector2D from, Vector2D to)
        {
            var cos = from.Dot(to);
            var sin = from.Cross(to);
            return new Transform2D(cos, -sin, sin, cos, 0.0, 0.0).RotationAngle();
        }
    }
}
