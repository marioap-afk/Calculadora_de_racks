using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>Source frame of one projection operation: the picked base point and its orientation.</summary>
    public readonly struct SourceGroupFrame
    {
        public SourceGroupFrame(Point3D basePoint, double orientationRadians)
        {
            BasePoint = basePoint;
            OrientationRadians = orientationRadians;
        }

        public Point3D BasePoint { get; }
        public double OrientationRadians { get; }

        /// <summary>Owner decision OD-6.c A: the source frame keeps universal X, so alpha is 0 for Rigid.</summary>
        public static SourceGroupFrame Universal(Point3D basePoint) => new SourceGroupFrame(basePoint, 0.0);
    }

    /// <summary>Target frame of one projection operation. Owner decision OD-6.b A froze phi_t = 0 (universal X).</summary>
    public readonly struct TargetGroupFrame
    {
        public TargetGroupFrame(Point3D targetPoint, double orientationRadians)
        {
            TargetPoint = targetPoint;
            OrientationRadians = orientationRadians;
        }

        public Point3D TargetPoint { get; }
        public double OrientationRadians { get; }

        public static TargetGroupFrame Universal(Point3D targetPoint) => new TargetGroupFrame(targetPoint, 0.0);
    }

    /// <summary>
    /// Semantic reading of an AUTH-05 frame: local points of physical coordinates. Drawn bounds never participate.
    /// </summary>
    public static class RackViewFrameSemantics
    {
        /// <summary>Physical coordinate of the frame origin on one axis.</summary>
        public static double OriginOn(RackViewFrame frame, RackPhysicalAxis axis)
        {
            switch (axis)
            {
                case RackPhysicalAxis.Run:
                    return frame.PhysicalOrigin.Run;
                case RackPhysicalAxis.Depth:
                    return frame.PhysicalOrigin.Depth;
                default:
                    return frame.PhysicalOrigin.Height;
            }
        }

        /// <summary>Local unit vector of the positive physical axis, when the view represents it.</summary>
        public static bool TryAxisDirection(RackViewFrame frame, RackPhysicalAxis axis, out Vector2D direction)
        {
            if (frame.AxisMap.LocalX == axis)
            {
                direction = new Vector2D((int)frame.AxisMap.LocalXOrientation, 0.0);
                return true;
            }

            if (frame.AxisMap.LocalY == axis)
            {
                direction = new Vector2D(0.0, (int)frame.AxisMap.LocalYOrientation);
                return true;
            }

            direction = Vector2D.Zero;
            return false;
        }

        /// <summary>Local point of the frame origin.</summary>
        public static Point2D Anchor(RackViewFrame frame)
            => new Point2D(
                (int)frame.AxisMap.LocalXOrientation * OriginOn(frame, frame.AxisMap.LocalX),
                (int)frame.AxisMap.LocalYOrientation * OriginOn(frame, frame.AxisMap.LocalY));

        /// <summary>
        /// Local point whose conserved physical coordinate is <paramref name="physicalK"/>; every other physical
        /// coordinate stays on the frame origin.
        /// </summary>
        public static bool TryLocalPointAt(
            RackViewFrame frame,
            RackPhysicalAxis axis,
            double physicalK,
            out Point2D local)
        {
            var anchor = Anchor(frame);
            if (frame.AxisMap.LocalX == axis)
            {
                local = new Point2D((int)frame.AxisMap.LocalXOrientation * physicalK, anchor.Y);
                return true;
            }

            if (frame.AxisMap.LocalY == axis)
            {
                local = new Point2D(anchor.X, (int)frame.AxisMap.LocalYOrientation * physicalK);
                return true;
            }

            local = anchor;
            return false;
        }

        /// <summary>Absolute physical span of the frame on the axis it measures (its local X).</summary>
        public static bool TrySpanOn(RackViewFrame frame, RackPhysicalAxis axis, out double min, out double max)
        {
            if (frame.AxisMap.LocalX != axis)
            {
                min = 0.0;
                max = 0.0;
                return false;
            }

            var origin = OriginOn(frame, axis);
            min = origin + frame.KMin;
            max = origin + frame.KMax;
            return true;
        }
    }
}
