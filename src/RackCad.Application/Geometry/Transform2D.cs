using System;

namespace RackCad.Application.Geometry
{
    /// <summary>
    /// An affine transformation of the model plane: rotation, mirroring, uniform scaling and translation.
    ///
    /// <see cref="Placement2D"/> already existed and only TRANSLATES; it stays untouched because the header
    /// placement code depends on it. This is the general case the section geometry needs, added alongside.
    ///
    /// Stored as the 2×3 matrix
    /// <code>
    ///   | M11 M12 Dx |        x' = M11·x + M12·y + Dx
    ///   | M21 M22 Dy |        y' = M21·x + M22·y + Dy
    /// </code>
    /// The sign of <see cref="Determinant"/> is what tells a contour whether its winding survived: a mirror
    /// makes it negative, and a counter-clockwise outline comes out clockwise. Anything that mirrors a
    /// contour has to reverse it, and this is where that fact is discoverable.
    /// </summary>
    public readonly struct Transform2D
    {
        public Transform2D(double m11, double m12, double m21, double m22, double dx, double dy)
        {
            M11 = GeometryTolerance.RequireFinite(m11, nameof(m11));
            M12 = GeometryTolerance.RequireFinite(m12, nameof(m12));
            M21 = GeometryTolerance.RequireFinite(m21, nameof(m21));
            M22 = GeometryTolerance.RequireFinite(m22, nameof(m22));
            Dx = GeometryTolerance.RequireFinite(dx, nameof(dx));
            Dy = GeometryTolerance.RequireFinite(dy, nameof(dy));
        }

        public double M11 { get; }

        public double M12 { get; }

        public double M21 { get; }

        public double M22 { get; }

        public double Dx { get; }

        public double Dy { get; }

        public static Transform2D Identity => new Transform2D(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

        public static Transform2D Translation(double dx, double dy) => new Transform2D(1.0, 0.0, 0.0, 1.0, dx, dy);

        public static Transform2D Translation(Vector2D offset) => Translation(offset.X, offset.Y);

        /// <summary>Rotation about the origin, counter-clockwise, in RADIANS.</summary>
        public static Transform2D Rotation(double radians)
        {
            GeometryTolerance.RequireFinite(radians, nameof(radians));
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            return new Transform2D(cos, -sin, sin, cos, 0.0, 0.0);
        }

        /// <summary>Rotation about the origin, counter-clockwise, in DEGREES. The unit a user types.</summary>
        public static Transform2D RotationDegrees(double degrees) => Rotation(degrees * Math.PI / 180.0);

        /// <summary>Mirror about the Y axis: X flips. Reverses winding.</summary>
        public static Transform2D MirrorAboutY => new Transform2D(-1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

        /// <summary>Mirror about the X axis: Y flips. Reverses winding.</summary>
        public static Transform2D MirrorAboutX => new Transform2D(1.0, 0.0, 0.0, -1.0, 0.0, 0.0);

        /// <summary>Uniform scaling about the origin. Non-uniform scaling is deliberately absent: it would turn
        /// a circular arc into an elliptical one, and this type promises to preserve arcs.</summary>
        public static Transform2D Scale(double factor)
        {
            GeometryTolerance.RequirePositive(factor, nameof(factor));
            return new Transform2D(factor, 0.0, 0.0, factor, 0.0, 0.0);
        }

        /// <summary>Negative when the transformation mirrors; its magnitude is the area scale factor.</summary>
        public double Determinant => (M11 * M22) - (M12 * M21);

        /// <summary>True when the transformation flips orientation, so contours must be reversed.</summary>
        public bool ReversesOrientation => Determinant < 0.0;

        /// <summary>The uniform scale factor. Valid because only uniform scaling can be built.</summary>
        public double ScaleFactor => Math.Sqrt(Math.Abs(Determinant));

        /// <summary>This transformation followed by <paramref name="next"/> — read left to right.</summary>
        public Transform2D Then(Transform2D next) =>
            new Transform2D(
                (next.M11 * M11) + (next.M12 * M21),
                (next.M11 * M12) + (next.M12 * M22),
                (next.M21 * M11) + (next.M22 * M21),
                (next.M21 * M12) + (next.M22 * M22),
                (next.M11 * Dx) + (next.M12 * Dy) + next.Dx,
                (next.M21 * Dx) + (next.M22 * Dy) + next.Dy);

        public Point2D Apply(Point2D point) =>
            new Point2D(
                (M11 * point.X) + (M12 * point.Y) + Dx,
                (M21 * point.X) + (M22 * point.Y) + Dy);

        /// <summary>Transforms a free vector: the translation does NOT apply to a direction.</summary>
        public Vector2D Apply(Vector2D vector) =>
            new Vector2D(
                (M11 * vector.X) + (M12 * vector.Y),
                (M21 * vector.X) + (M22 * vector.Y));

        /// <summary>
        /// How much the transformation rotates a direction, in radians, and whether it mirrors. An arc needs
        /// both to survive: its centre and radius transform directly, but its angles need the rotation, and a
        /// mirror reverses its sweep.
        /// </summary>
        public double RotationAngle()
        {
            var image = Apply(Vector2D.UnitX);
            return Math.Atan2(image.Y, image.X);
        }
    }
}
