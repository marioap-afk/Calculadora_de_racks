using System;
using System.Globalization;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>The world axis a punch is drilled along. Only the two the sub-assembly uses exist.</summary>
    public enum CantileverPunchAxis
    {
        /// <summary>Along the base direction: every connection punch, on the column face and on the rear plate.</summary>
        AlongY = 0,

        /// <summary>Along the column axis: every punch of the column's bottom plate.</summary>
        AlongZ = 1
    }

    /// <summary>
    /// Which piece's surface a punch was recorded on. It explains why two coincident punches have different
    /// 3D centres, and it is NOT part of their identity.
    /// </summary>
    public enum CantileverPunchSurface
    {
        /// <summary>The connecting face of the column, on the connection plane.</summary>
        ColumnFace = 0,

        /// <summary>The mid-thickness of the base's rear plate.</summary>
        BaseRearPlate = 1,

        /// <summary>The mid-thickness of the column's bottom plate.</summary>
        ColumnBottomPlate = 2
    }

    /// <summary>
    /// The LOGICAL identity of a punch: the axis it is drilled along, the two world coordinates that axis
    /// does not consume, and its diameter.
    ///
    /// This type is the reason the initiative can prove that the rear plate and the column carry the same
    /// holes. Two punches of the SAME bolt sit on surfaces separated by the thickness of a plate, so their
    /// 3D centres are legitimately different and comparing them would fail — or, worse, would only pass
    /// while somebody kept subtracting thicknesses in the test. The datum is what is actually shared
    /// (ADR-0024, D3).
    ///
    /// For <see cref="CantileverPunchAxis.AlongY"/>: <see cref="U"/> is world X and <see cref="V"/> is
    /// world Z. For <see cref="CantileverPunchAxis.AlongZ"/>: <see cref="U"/> is world X and
    /// <see cref="V"/> is world Y.
    /// </summary>
    public readonly struct CantileverPunchDatum : IEquatable<CantileverPunchDatum>
    {
        /// <summary>Default tolerance for datum comparison, inches.</summary>
        public const double Tolerance = 1e-9;

        public CantileverPunchDatum(CantileverPunchAxis axis, double u, double v, double diameter)
        {
            // A cast from an int can produce an axis that no case handles. Rejecting it here means every
            // datum in existence has an axis somebody wrote down, so the consumers may switch on it without
            // an "everything else" arm that would quietly pick a direction.
            if (axis != CantileverPunchAxis.AlongY && axis != CantileverPunchAxis.AlongZ)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(axis), axis, "El eje de troquel '" + axis + "' no esta definido.");
            }

            GeometryTolerance.RequireFinite(u, nameof(u));
            GeometryTolerance.RequireFinite(v, nameof(v));
            GeometryTolerance.RequireFinite(diameter, nameof(diameter));

            if (diameter <= 0.0)
            {
                throw new ArgumentException("El diametro de un troquel debe ser positivo.", nameof(diameter));
            }

            Axis = axis;
            U = u;
            V = v;
            Diameter = diameter;
        }

        public CantileverPunchAxis Axis { get; }

        /// <summary>World X, for both axes used here.</summary>
        public double U { get; }

        /// <summary>World Z when the axis is <see cref="CantileverPunchAxis.AlongY"/>; world Y when it is <see cref="CantileverPunchAxis.AlongZ"/>.</summary>
        public double V { get; }

        public double Diameter { get; }

        /// <summary>
        /// Whether two datums describe the same hole WITHIN A TOLERANCE. This is the geometric question,
        /// and it is deliberately NOT <see cref="Equals(CantileverPunchDatum)"/>.
        ///
        /// Tolerant comparison is not an equivalence relation: it is not transitive, so a value type whose
        /// <c>Equals</c> delegated here would be broken in every dictionary and every <c>Distinct()</c> —
        /// two "equal" values could hash to different buckets, and a hash coarse enough to agree with the
        /// tolerance would put values that are NOT equal in the same one. Two questions, two methods, and
        /// the caller who means the geometric one says so.
        /// </summary>
        public bool ApproxEquals(CantileverPunchDatum other, double tolerance = Tolerance) =>
            Axis == other.Axis &&
            Math.Abs(U - other.U) <= tolerance &&
            Math.Abs(V - other.V) <= tolerance &&
            Math.Abs(Diameter - other.Diameter) <= tolerance;

        /// <summary>
        /// Exact value equality over the same four fields the hash uses: reflexive, symmetric, transitive
        /// and consistent with <see cref="GetHashCode"/>.
        /// </summary>
        public bool Equals(CantileverPunchDatum other) =>
            Axis == other.Axis &&
            U.Equals(other.U) &&
            V.Equals(other.V) &&
            Diameter.Equals(other.Diameter);

        public override bool Equals(object obj) => obj is CantileverPunchDatum other && Equals(other);

        /// <summary>Exactly the four fields <see cref="Equals(CantileverPunchDatum)"/> compares, unrounded.</summary>
        public override int GetHashCode() => (Axis, U, V, Diameter).GetHashCode();

        public static bool operator ==(CantileverPunchDatum left, CantileverPunchDatum right) => left.Equals(right);

        public static bool operator !=(CantileverPunchDatum left, CantileverPunchDatum right) => !left.Equals(right);

        public override string ToString() =>
            Axis + " u=" + U.ToString("0.####", CultureInfo.InvariantCulture) +
            " v=" + V.ToString("0.####", CultureInfo.InvariantCulture) +
            " d=" + Diameter.ToString("0.####", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// One punch, as a parametrised circular opening associated with a surface and an axis.
    ///
    /// It does NOT subtract material from anything. There is no solid to subtract from — ADR-0022 keeps the
    /// representation flat — and modelling a hole as a boolean would need a 3D body that I-37A neither has
    /// nor is allowed to create.
    /// </summary>
    public sealed class CantileverPunchPlan
    {
        public CantileverPunchPlan(
            CantileverPieceId id,
            CantileverPunchSurface surface,
            Point3D centre,
            CantileverPunchDatum datum)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException("Un troquel necesita su id.", nameof(id));
            }

            Id = id;
            Surface = surface;
            Centre = centre;
            Datum = datum;
        }

        public CantileverPieceId Id { get; }

        /// <summary>The surface this punch was recorded on. Explains the 3D centre; not part of the identity.</summary>
        public CantileverPunchSurface Surface { get; }

        /// <summary>
        /// A point on the punch axis, lying on <see cref="Surface"/>. Two coincident punches have DIFFERENT
        /// centres by the thickness that separates their surfaces; compare <see cref="Datum"/>.
        /// </summary>
        public Point3D Centre { get; }

        public CantileverPunchDatum Datum { get; }

        public CantileverPunchAxis Axis => Datum.Axis;

        public double Diameter => Datum.Diameter;

        /// <summary>
        /// The world direction of the drilling axis.
        ///
        /// It FAILS CLOSED. The previous form was a conditional whose else branch returned Z, so an axis
        /// nobody had written a rule for would have been drilled vertically without a word — the kind of
        /// default that is only discovered when a hole comes out in the wrong face.
        /// </summary>
        public Vector3D Direction
        {
            get
            {
                switch (Datum.Axis)
                {
                    case CantileverPunchAxis.AlongY:
                        return Vector3D.UnitY;
                    case CantileverPunchAxis.AlongZ:
                        return Vector3D.UnitZ;
                    default:
                        throw new InvalidOperationException(
                            "El eje de troquel '" + Datum.Axis + "' no tiene direccion definida.");
                }
            }
        }

        public override string ToString() => Id.Value + " " + Datum;
    }
}
