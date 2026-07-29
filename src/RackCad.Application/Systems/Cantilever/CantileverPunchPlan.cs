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

        public bool ApproxEquals(CantileverPunchDatum other, double tolerance = Tolerance) =>
            Axis == other.Axis &&
            Math.Abs(U - other.U) <= tolerance &&
            Math.Abs(V - other.V) <= tolerance &&
            Math.Abs(Diameter - other.Diameter) <= tolerance;

        public bool Equals(CantileverPunchDatum other) => ApproxEquals(other);

        public override bool Equals(object obj) => obj is CantileverPunchDatum other && Equals(other);

        public override int GetHashCode() =>
            // Rounded so that two values that ApproxEquals also land in the same bucket. Six decimals is
            // the resolution the signatures use, and it is far coarser than the comparison tolerance.
            (Axis,
             Math.Round(U, 6, MidpointRounding.AwayFromZero),
             Math.Round(V, 6, MidpointRounding.AwayFromZero),
             Math.Round(Diameter, 6, MidpointRounding.AwayFromZero)).GetHashCode();

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

        /// <summary>The world direction of the drilling axis.</summary>
        public Vector3D Direction =>
            Datum.Axis == CantileverPunchAxis.AlongY ? Vector3D.UnitY : Vector3D.UnitZ;

        public override string ToString() => Id.Value + " " + Datum;
    }
}
