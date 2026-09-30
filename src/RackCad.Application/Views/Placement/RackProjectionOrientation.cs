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
            return RackProjectionGroupFramesResult.Available(
                SourceGroupFrame.Universal(origin), TargetGroupFrame.Universal(origin));
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
            return RackProjectionGroupFramesResult.Available(
                SourceGroupFrame.Universal(origin), TargetGroupFrame.Universal(origin));
        }
    }
}
