using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// How RACKPROYECTAR orients the placed block references (G16 C16-06). Only the BlockReference rotates: the block
    /// definition, its geometry, texts and dimensions never change. The product default is <see cref="Projected"/>.
    /// </summary>
    public enum RackProjectionOrientationMode
    {
        /// <summary>«Proyectada»: the projected view keeps, in the drawing, the physical axis it shares with its source.</summary>
        Projected,

        /// <summary>«Predeterminada»: the canonical RackCad presentation, whatever the rotation of the source.</summary>
        Canonical
    }

    /// <summary>The group frames of one orthographic operation, or why no single projected orientation exists.</summary>
    public readonly struct RackProjectionGroupFramesResult
    {
        private RackProjectionGroupFramesResult(SourceGroupFrame source, TargetGroupFrame target, RackProjectionFailureCode failure)
        {
            Source = source;
            Target = target;
            Failure = failure;
        }

        public SourceGroupFrame Source { get; }
        public TargetGroupFrame Target { get; }
        public RackProjectionFailureCode Failure { get; }
        public bool IsAvailable => Failure == RackProjectionFailureCode.None;

        internal static RackProjectionGroupFramesResult Available(SourceGroupFrame source, TargetGroupFrame target) =>
            new RackProjectionGroupFramesResult(source, target, RackProjectionFailureCode.None);

        internal static RackProjectionGroupFramesResult Unavailable(RackProjectionFailureCode failure) =>
            new RackProjectionGroupFramesResult(
                SourceGroupFrame.Universal(Origin), TargetGroupFrame.Universal(Origin), failure);

        private static Point3D Origin => new Point3D(0.0, 0.0, 0.0);
    }

    /// <summary>
    /// Pure orientation authority of ID19 (G16 C16-06). It reads AUTH-05 axis maps and AUTH-08 V2 accepted placements only,
    /// and returns the group frames G14 already consumes; no AutoCAD type, no angle table.
    /// </summary>
    public static class RackProjectionOrientationResolver
    {
        /// <summary>
        /// The source and target group frames of an orthographic operation (their orientations only; the points arrive later).
        /// </summary>
        public static RackProjectionGroupFramesResult ResolveOrthographic(
            IReadOnlyList<RackProjectionSourceView> views,
            RackPhysicalAxis conservedAxis,
            RackProjectionOrientationMode mode)
        {
            if (views == null) throw new ArgumentNullException(nameof(views));
            var origin = new Point3D(0.0, 0.0, 0.0);

            // Canonical: the universal frames frozen by G14 (OD-6.b A, OD-6.c A). An empty operation is left to the policy.
            if (mode == RackProjectionOrientationMode.Canonical || views.Count == 0)
            {
                return RackProjectionGroupFramesResult.Available(
                    SourceGroupFrame.Universal(origin), TargetGroupFrame.Universal(origin));
            }

            // Projected: d_i = L_i · s_i is where the source +K axis points in the drawing (AUTH-05 axis map, AUTH-08 V2 linear part).
            Vector2D firstTarget = Vector2D.Zero;
            var worlds = new List<Vector2D>(views.Count);
            foreach (var view in views)
            {
                if (!RackViewFrameSemantics.TryAxisDirection(view.SourceFrame, conservedAxis, out var source)
                    || !RackViewFrameSemantics.TryAxisDirection(view.TargetFrame, conservedAxis, out var target))
                {
                    return RackProjectionGroupFramesResult.Unavailable(RackProjectionFailureCode.FrameAxisMissing);
                }

                if (worlds.Count == 0)
                {
                    firstTarget = target;
                }

                worlds.Add(view.Placement.Linear.Apply(source).Normalized());
            }

            // Frozen order (contract E): parallelism first (tolerance on the cross product), then the sense (sign of the dot product).
            var d = worlds[0];
            for (var index = 1; index < worlds.Count; index++)
            {
                if (Math.Abs(worlds[index].Cross(d)) > GeometryTolerance.Angle)
                {
                    return RackProjectionGroupFramesResult.Unavailable(RackProjectionFailureCode.NonParallelSources);
                }
            }

            for (var index = 1; index < worlds.Count; index++)
            {
                if (worlds[index].Dot(d) < 0.0)
                {
                    return RackProjectionGroupFramesResult.Unavailable(RackProjectionFailureCode.SourceOrientationDivergent);
                }
            }

            // phi_s is the rotation of the first accepted placement (R(phi_s)·s_0 = d); phi_t turns the target +K onto d.
            return RackProjectionGroupFramesResult.Available(
                new SourceGroupFrame(origin, views[0].Placement.RotationRadians),
                new TargetGroupFrame(origin, RackOrthographicPlacementPolicy.AngleFrom(firstTarget, d)));
        }

        /// <summary>
        /// The group frames of a same-class (Rigid) operation: alpha = phi_t - phi_s is the angle of its single common transform.
        /// </summary>
        public static RackProjectionGroupFramesResult ResolveRigid(
            IReadOnlyList<RackProjectionSourceView> views,
            RackProjectionOrientationMode mode)
        {
            if (views == null) throw new ArgumentNullException(nameof(views));
            var origin = new Point3D(0.0, 0.0, 0.0);

            // Projected: the frozen G14 rigid copy, alpha = 0 (every view keeps its own rotation).
            if (mode == RackProjectionOrientationMode.Projected || views.Count == 0)
            {
                return RackProjectionGroupFramesResult.Available(
                    SourceGroupFrame.Universal(origin), TargetGroupFrame.Universal(origin));
            }

            // Canonical: one rigid transform with alpha = -theta_0 leaves every view unrotated only if all share one rotation.
            var first = views[0].Placement.Linear.Apply(new Vector2D(1.0, 0.0)).Normalized();
            for (var index = 1; index < views.Count; index++)
            {
                var other = views[index].Placement.Linear.Apply(new Vector2D(1.0, 0.0)).Normalized();
                if (Math.Abs(other.Cross(first)) > GeometryTolerance.Angle || other.Dot(first) < 0.0)
                {
                    return RackProjectionGroupFramesResult.Unavailable(RackProjectionFailureCode.SourceRotationsDiffer);
                }
            }

            return RackProjectionGroupFramesResult.Available(
                new SourceGroupFrame(origin, views[0].Placement.RotationRadians),
                TargetGroupFrame.Universal(origin));
        }
    }
}
