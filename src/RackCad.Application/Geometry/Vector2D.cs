using System;
using System.Globalization;

namespace RackCad.Application.Geometry
{
    /// <summary>
    /// A free vector in the model plane (inches). The DIRECTION-and-magnitude counterpart of
    /// <see cref="Point2D"/>, which is a position: adding two positions is meaningless, adding a vector to a
    /// position is not, and keeping them as separate types is what stops that confusion from compiling.
    /// </summary>
    public readonly struct Vector2D : IEquatable<Vector2D>
    {
        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }

        public double Y { get; }

        public static Vector2D Zero => new Vector2D(0.0, 0.0);

        public static Vector2D UnitX => new Vector2D(1.0, 0.0);

        public static Vector2D UnitY => new Vector2D(0.0, 1.0);

        public double LengthSquared => (X * X) + (Y * Y);

        public double Length => Math.Sqrt(LengthSquared);

        public bool IsFinite => GeometryTolerance.IsFinite(X) && GeometryTolerance.IsFinite(Y);

        /// <summary>The vector from <paramref name="from"/> to <paramref name="to"/>.</summary>
        public static Vector2D Between(Point2D from, Point2D to) => new Vector2D(to.X - from.X, to.Y - from.Y);

        /// <summary>Unit vector in the same direction. Throws on a zero-length vector: normalizing one is a bug,
        /// and returning zero would let it travel silently into a contour.</summary>
        public Vector2D Normalized()
        {
            var length = Length;

            if (!GeometryTolerance.IsFinite(length) || length <= GeometryTolerance.Length)
            {
                throw new InvalidOperationException("No se puede normalizar un vector de longitud nula o no finita.");
            }

            return new Vector2D(X / length, Y / length);
        }

        /// <summary>Rotated a quarter turn counter-clockwise. The 2D "normal" of a direction.</summary>
        public Vector2D Perpendicular() => new Vector2D(-Y, X);

        public double Dot(Vector2D other) => (X * other.X) + (Y * other.Y);

        /// <summary>The z component of the 3D cross product. Positive when <paramref name="other"/> turns left.</summary>
        public double Cross(Vector2D other) => (X * other.Y) - (Y * other.X);

        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);

        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);

        public static Vector2D operator -(Vector2D v) => new Vector2D(-v.X, -v.Y);

        public static Vector2D operator *(Vector2D v, double scale) => new Vector2D(v.X * scale, v.Y * scale);

        public static Vector2D operator *(double scale, Vector2D v) => v * scale;

        public bool Equals(Vector2D other) => X.Equals(other.X) && Y.Equals(other.Y);

        public override bool Equals(object obj) => obj is Vector2D other && Equals(other);

        public override int GetHashCode() => (X, Y).GetHashCode();

        public bool ApproxEquals(Vector2D other, double tolerance) =>
            Math.Abs(X - other.X) <= tolerance && Math.Abs(Y - other.Y) <= tolerance;

        public override string ToString() =>
            "<" + X.ToString(CultureInfo.InvariantCulture) + ", " + Y.ToString(CultureInfo.InvariantCulture) + ">";
    }

    /// <summary>
    /// The tolerances the neutral geometry compares with, in one place.
    ///
    /// They are absolute and expressed in inches because that is RackCad's internal unit (ADR-0005). A
    /// relative tolerance would be wrong here: the sections span from a 1/8 in. wall to a 44 in. depth, and a
    /// relative epsilon would be either useless at the small end or blind at the large one.
    /// </summary>
    public static class GeometryTolerance
    {
        /// <summary>Two lengths closer than this are the same point (1e-9 in.). Well below any tabulated digit.</summary>
        public const double Length = 1e-9;

        /// <summary>Tolerance for chaining segments end to start. Looser than <see cref="Length"/> because the
        /// endpoints are computed from angles, and a round trip through sin/cos costs a few ulps.</summary>
        public const double Continuity = 1e-7;

        /// <summary>Two angles closer than this are the same angle (radians).</summary>
        public const double Angle = 1e-9;

        public static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        public static bool IsZero(double value, double tolerance = Length) => Math.Abs(value) <= tolerance;

        public static bool AreClose(double a, double b, double tolerance = Length) => Math.Abs(a - b) <= tolerance;

        /// <summary>Guards a value that must be a real number before it reaches a contour.</summary>
        public static double RequireFinite(double value, string name)
        {
            if (!IsFinite(value))
            {
                throw new ArgumentException("El valor '" + name + "' no es finito.", name);
            }

            return value;
        }

        /// <summary>Guards a magnitude that must be strictly positive (a width, a thickness, a radius).</summary>
        public static double RequirePositive(double value, string name)
        {
            RequireFinite(value, name);

            if (value <= 0.0)
            {
                throw new ArgumentException(
                    "El valor '" + name + "' debe ser positivo y vale " +
                    value.ToString(CultureInfo.InvariantCulture) + ".",
                    name);
            }

            return value;
        }
    }
}
