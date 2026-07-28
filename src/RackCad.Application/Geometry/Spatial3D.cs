using System;
using System.Globalization;

namespace RackCad.Application.Geometry
{
    /// <summary>A position in model space (inches). Used only for orientation and projection.</summary>
    public readonly struct Point3D
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public static Point3D Origin => new Point3D(0.0, 0.0, 0.0);

        /// <summary>Lifts a plane point onto the z = <paramref name="z"/> plane.</summary>
        public static Point3D From(Point2D point, double z) => new Point3D(point.X, point.Y, z);

        public static Point3D operator +(Point3D p, Vector3D v) => new Point3D(p.X + v.X, p.Y + v.Y, p.Z + v.Z);

        public static Vector3D operator -(Point3D a, Point3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public override string ToString() =>
            "(" + X.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            Y.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            Z.ToString("0.####", CultureInfo.InvariantCulture) + ")";
    }

    /// <summary>A direction and magnitude in model space.</summary>
    public readonly struct Vector3D
    {
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public static Vector3D UnitX => new Vector3D(1.0, 0.0, 0.0);

        public static Vector3D UnitY => new Vector3D(0.0, 1.0, 0.0);

        public static Vector3D UnitZ => new Vector3D(0.0, 0.0, 1.0);

        public double LengthSquared => (X * X) + (Y * Y) + (Z * Z);

        public double Length => Math.Sqrt(LengthSquared);

        public bool IsFinite =>
            GeometryTolerance.IsFinite(X) && GeometryTolerance.IsFinite(Y) && GeometryTolerance.IsFinite(Z);

        public Vector3D Normalized()
        {
            var length = Length;

            if (!GeometryTolerance.IsFinite(length) || length <= GeometryTolerance.Length)
            {
                throw new InvalidOperationException("No se puede normalizar un vector 3D de longitud nula o no finita.");
            }

            return new Vector3D(X / length, Y / length, Z / length);
        }

        public double Dot(Vector3D other) => (X * other.X) + (Y * other.Y) + (Z * other.Z);

        public Vector3D Cross(Vector3D other) =>
            new Vector3D(
                (Y * other.Z) - (Z * other.Y),
                (Z * other.X) - (X * other.Z),
                (X * other.Y) - (Y * other.X));

        public static Vector3D operator +(Vector3D a, Vector3D b) => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3D operator -(Vector3D v) => new Vector3D(-v.X, -v.Y, -v.Z);

        public static Vector3D operator *(Vector3D v, double scale) => new Vector3D(v.X * scale, v.Y * scale, v.Z * scale);

        public static Vector3D operator *(double scale, Vector3D v) => v * scale;

        public bool ApproxEquals(Vector3D other, double tolerance) =>
            Math.Abs(X - other.X) <= tolerance &&
            Math.Abs(Y - other.Y) <= tolerance &&
            Math.Abs(Z - other.Z) <= tolerance;

        public override string ToString() =>
            "<" + X.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            Y.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            Z.ToString("0.####", CultureInfo.InvariantCulture) + ">";
    }

    /// <summary>
    /// A right-handed orthonormal frame: an origin and three unit axes.
    ///
    /// It is what lets a section be placed and looked at without the section knowing anything about it. Two
    /// uses, both essential and both the same type: the LOCAL frame of a prismatic instance (its Z is the
    /// longitudinal axis) and the CAMERA frame of an orthographic view (its Z is the viewing direction).
    ///
    /// Orthonormality is enforced at construction. A frame that is nearly-but-not-quite orthonormal produces
    /// a projection that is subtly sheared, and nothing downstream would ever notice.
    /// </summary>
    public sealed class LocalFrame3D
    {
        /// <summary>How far from orthonormal a frame may be before it is rejected.</summary>
        public const double OrthonormalTolerance = 1e-9;

        private LocalFrame3D(Point3D origin, Vector3D axisX, Vector3D axisY, Vector3D axisZ)
        {
            Origin = origin;
            AxisX = axisX;
            AxisY = axisY;
            AxisZ = axisZ;
        }

        public Point3D Origin { get; }

        public Vector3D AxisX { get; }

        public Vector3D AxisY { get; }

        public Vector3D AxisZ { get; }

        /// <summary>The world frame: origin at zero, axes along X, Y and Z.</summary>
        public static LocalFrame3D World { get; } =
            new LocalFrame3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ);

        /// <summary>
        /// Builds a frame from an origin, a longitudinal direction (Z) and a reference for X. The reference
        /// only has to be non-parallel to Z: X is re-orthogonalized against it and Y completes the right-handed
        /// triple, so a caller can pass a rough "up" without having to do the algebra.
        /// </summary>
        public static LocalFrame3D Create(Point3D origin, Vector3D axisZ, Vector3D referenceX)
        {
            if (!axisZ.IsFinite || !referenceX.IsFinite)
            {
                throw new ArgumentException("Los ejes del marco deben ser finitos.");
            }

            var z = axisZ.Normalized();
            var projected = referenceX - (z * referenceX.Dot(z));

            if (projected.Length <= 1e-8)
            {
                throw new ArgumentException(
                    "La referencia de X es paralela al eje Z: no define un marco.", nameof(referenceX));
            }

            var x = projected.Normalized();
            var y = z.Cross(x).Normalized();

            return new LocalFrame3D(origin, x, y, z);
        }

        /// <summary>
        /// A CAMERA frame from a viewing direction and an up reference.
        ///
        /// It exists because building one by hand is deceptively easy to get wrong: picking three plausible
        /// axes and passing them to <see cref="FromAxes"/> produces a LEFT-handed triple more often than not,
        /// and the failure is total rather than subtle. Here the right axis is derived as up × forward and the
        /// true up as forward × right, so the result is right-handed by construction.
        ///
        /// The resulting X and Y span the picture plane and Z is the viewing direction.
        /// </summary>
        public static LocalFrame3D Camera(Vector3D viewDirection, Vector3D upReference)
        {
            if (!viewDirection.IsFinite || !upReference.IsFinite)
            {
                throw new ArgumentException("La direccion de vista y la referencia vertical deben ser finitas.");
            }

            var forward = viewDirection.Normalized();
            var right = upReference.Cross(forward);

            if (right.Length <= 1e-8)
            {
                throw new ArgumentException(
                    "La referencia vertical es paralela a la direccion de vista: no define una camara.",
                    nameof(upReference));
            }

            right = right.Normalized();
            return new LocalFrame3D(Point3D.Origin, right, forward.Cross(right).Normalized(), forward);
        }

        /// <summary>Accepts three axes that are ALREADY orthonormal, verifying it rather than trusting it.</summary>
        public static LocalFrame3D FromAxes(Point3D origin, Vector3D axisX, Vector3D axisY, Vector3D axisZ)
        {
            RequireUnit(axisX, nameof(axisX));
            RequireUnit(axisY, nameof(axisY));
            RequireUnit(axisZ, nameof(axisZ));
            RequireOrthogonal(axisX, axisY, nameof(axisX), nameof(axisY));
            RequireOrthogonal(axisY, axisZ, nameof(axisY), nameof(axisZ));
            RequireOrthogonal(axisZ, axisX, nameof(axisZ), nameof(axisX));

            if (!axisX.Cross(axisY).ApproxEquals(axisZ, 1e-7))
            {
                throw new ArgumentException("El marco no es dextrogiro: X × Y debe dar Z.");
            }

            return new LocalFrame3D(origin, axisX, axisY, axisZ);
        }

        /// <summary>Maps a point expressed in this frame to world coordinates.</summary>
        public Point3D ToWorld(Point3D local) =>
            Origin + (AxisX * local.X) + (AxisY * local.Y) + (AxisZ * local.Z);

        /// <summary>Maps a section point at longitudinal position <paramref name="z"/> to world coordinates.</summary>
        public Point3D ToWorld(Point2D sectionPoint, double z) =>
            ToWorld(Point3D.From(sectionPoint, z));

        /// <summary>Maps a world direction into this frame's coordinates.</summary>
        public Vector3D ToLocal(Vector3D world) =>
            new Vector3D(world.Dot(AxisX), world.Dot(AxisY), world.Dot(AxisZ));

        /// <summary>Maps a world point into this frame's coordinates.</summary>
        public Point3D ToLocal(Point3D world)
        {
            var delta = world - Origin;
            return new Point3D(delta.Dot(AxisX), delta.Dot(AxisY), delta.Dot(AxisZ));
        }

        private static void RequireUnit(Vector3D axis, string name)
        {
            if (!axis.IsFinite || Math.Abs(axis.Length - 1.0) > OrthonormalTolerance)
            {
                throw new ArgumentException("El eje '" + name + "' no es unitario.", name);
            }
        }

        private static void RequireOrthogonal(Vector3D a, Vector3D b, string nameA, string nameB)
        {
            if (Math.Abs(a.Dot(b)) > OrthonormalTolerance)
            {
                throw new ArgumentException(
                    "Los ejes '" + nameA + "' y '" + nameB + "' no son ortogonales.", nameA);
            }
        }
    }
}
