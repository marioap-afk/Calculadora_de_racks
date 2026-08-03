using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>Which plate of the sub-assembly a plan is.</summary>
    public enum CantileverPlateKind
    {
        /// <summary>Cap of the base's free end. Carries no punches.</summary>
        BaseFront = 0,

        /// <summary>The plate that meets the column. Its punch columns are governed by the column.</summary>
        BaseRear = 1,

        /// <summary>The plate at the foot of the column, perpendicular to its axis.</summary>
        ColumnBottom = 2,

        /// <summary>
        /// The plate that bolts an arm to the column (I-37B). Its outer face is where the arm's profile
        /// STARTS — the ORIGIN of the cut plane sits on that face.
        ///
        /// With slope that is not the same as flush. The profile is cut SQUARE while this plate is a vertical
        /// plane, so the part of the section above its own origin penetrates the plate and the part below
        /// leaves clearance. It is a declared visual approximation; a mitred cut or any end preparation stays
        /// out of scope, and the resolver reports both magnitudes.
        /// </summary>
        ArmMounting = 3,

        /// <summary>
        /// The plate that closes an arm's free end (I-37B) — a cap, or a stop when it reaches above the body.
        /// Perpendicular to the arm's sloped axis, not to the world.
        /// </summary>
        ArmEnd = 4,

        /// <summary>
        /// The plate on a column's bracing face that receives one end of a separator (I-37D). Its normal is
        /// ±Y: the separator's end LAPS it and is bolted transversely, which is why it is a plate on a side
        /// face and not a cap on the column's X face.
        ///
        /// Its single centred hole is a DATUM, not a decoration: the separator's cut length is derived from the
        /// distance between the two plates' holes, so a plate that moved would move the separator with it
        /// rather than leaving the two disagreeing (ADR-0027, D5).
        /// </summary>
        SeparatorColumn = 5
    }

    /// <summary>
    /// A flat plate: a rectangular outline on a reference face, a normal, and a thickness.
    ///
    /// It is NOT a solid and there is no boolean anywhere: ADR-0022 keeps the representation flat, and a
    /// plate that carried a body would need a 3D modeller that I-37A neither has nor may create. The
    /// outline is enough to draw it, to measure it and to prove a punch falls inside it.
    ///
    /// The outline is given in WORLD coordinates on the reference face — the face at
    /// <see cref="NearOffset"/> along <see cref="Normal"/> — because every consumer of this plan works in
    /// the sub-assembly frame, and a plate that reported local coordinates would push the same transform
    /// into each of them.
    /// </summary>
    public sealed class CantileverPlatePlan
    {
        private CantileverPlatePlan(
            CantileverPieceId id,
            CantileverPlateKind kind,
            double thickness,
            Vector3D normal,
            double nearOffset,
            IReadOnlyList<Point3D> outline)
        {
            Id = id;
            Kind = kind;
            Thickness = thickness;
            Normal = normal;
            NearOffset = nearOffset;
            Outline = outline;
        }

        public CantileverPieceId Id { get; }

        public CantileverPlateKind Kind { get; }

        /// <summary>Thickness, inches. Its own value; four plates, four independent thicknesses.</summary>
        public double Thickness { get; }

        /// <summary>Unit normal: the direction the thickness grows along, from the reference face.</summary>
        public Vector3D Normal { get; }

        /// <summary>World coordinate, along <see cref="Normal"/>, of the reference face.</summary>
        public double NearOffset { get; }

        /// <summary>World coordinate of the far face: <see cref="NearOffset"/> + <see cref="Thickness"/>.</summary>
        public double FarOffset => NearOffset + Thickness;

        /// <summary>The four corners of the reference face, in world coordinates and in order.</summary>
        public IReadOnlyList<Point3D> Outline { get; }

        public static CantileverPlatePlan Create(
            CantileverPieceId id,
            CantileverPlateKind kind,
            double thickness,
            Vector3D normal,
            double nearOffset,
            IEnumerable<Point3D> outline)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException("Una placa necesita su id.", nameof(id));
            }

            GeometryTolerance.RequirePositive(thickness, nameof(thickness));
            GeometryTolerance.RequireFinite(nearOffset, nameof(nearOffset));

            var corners = (outline ?? Enumerable.Empty<Point3D>()).ToList();

            if (corners.Count != 4)
            {
                throw new ArgumentException(
                    "El contorno de una placa son cuatro esquinas; se recibieron " + corners.Count + ".",
                    nameof(outline));
            }

            return new CantileverPlatePlan(id, kind, thickness, normal.Normalized(), nearOffset, corners);
        }

        /// <summary>The plate's own extent, as a box over its reference face and its thickness.</summary>
        public CantileverEnvelope3D Envelope()
        {
            var near = CantileverEnvelope3D.FromPoints(Outline);
            var offset = new Vector3D(Normal.X * Thickness, Normal.Y * Thickness, Normal.Z * Thickness);
            var far = CantileverEnvelope3D.FromPoints(Outline.Select(p => p + offset));
            return near.Union(far);
        }

        public override string ToString() => Id.Value + " " + Kind + " t=" + Thickness;
    }

    /// <summary>
    /// The gusset: a right triangle in the central vertical plane, joining the rear plate to the top of the
    /// base.
    ///
    /// Its legs are DERIVED and equal, so the hypotenuse is at 45 degrees. The vertical leg is
    /// <c>RearPlateTopZ - BaseTopZ</c> — how far the rear plate reaches above the base section — and it is
    /// NOT three times the connection pitch: how far the plate reaches depends on where the last punch
    /// INSIDE the base fell, which depends on the base section's height. Hard-coding three pitches would be
    /// right only when the base height happens to be an exact multiple of the pitch.
    /// </summary>
    public sealed class CantileverGussetPlan
    {
        private CantileverGussetPlan(
            CantileverPieceId id,
            double thickness,
            Vector3D normal,
            double nearOffset,
            IReadOnlyList<Point3D> vertices,
            double verticalLeg,
            double horizontalLeg)
        {
            Id = id;
            Thickness = thickness;
            Normal = normal;
            NearOffset = nearOffset;
            Vertices = vertices;
            VerticalLeg = verticalLeg;
            HorizontalLeg = horizontalLeg;
        }

        public CantileverPieceId Id { get; }

        public double Thickness { get; }

        /// <summary>Unit normal of the gusset plane. The gusset lives in the central vertical plane, so this is X.</summary>
        public Vector3D Normal { get; }

        /// <summary>World coordinate, along <see cref="Normal"/>, of the reference face.</summary>
        public double NearOffset { get; }

        /// <summary>
        /// The three corners in world coordinates: the right-angle corner first (at the rear plate face, on
        /// top of the base), then the top of the vertical leg, then the far end of the horizontal leg.
        /// </summary>
        public IReadOnlyList<Point3D> Vertices { get; }

        public double VerticalLeg { get; }

        public double HorizontalLeg { get; }

        /// <summary>The hypotenuse angle from the horizontal, degrees. Equal legs make it exactly 45.</summary>
        public double HypotenuseAngleDegrees =>
            Math.Atan2(VerticalLeg, HorizontalLeg) * 180.0 / Math.PI;

        public static CantileverGussetPlan Create(
            CantileverPieceId id,
            double thickness,
            Vector3D normal,
            double nearOffset,
            Point3D rightAngleCorner,
            Point3D verticalEnd,
            Point3D horizontalEnd,
            double verticalLeg,
            double horizontalLeg)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException("Un cartabon necesita su id.", nameof(id));
            }

            GeometryTolerance.RequirePositive(thickness, nameof(thickness));
            GeometryTolerance.RequirePositive(verticalLeg, nameof(verticalLeg));
            GeometryTolerance.RequirePositive(horizontalLeg, nameof(horizontalLeg));
            GeometryTolerance.RequireFinite(nearOffset, nameof(nearOffset));

            return new CantileverGussetPlan(
                id, thickness, normal.Normalized(), nearOffset,
                new[] { rightAngleCorner, verticalEnd, horizontalEnd },
                verticalLeg, horizontalLeg);
        }

        public CantileverEnvelope3D Envelope()
        {
            var near = CantileverEnvelope3D.FromPoints(Vertices);
            var offset = new Vector3D(Normal.X * Thickness, Normal.Y * Thickness, Normal.Z * Thickness);
            var far = CantileverEnvelope3D.FromPoints(Vertices.Select(p => p + offset));
            return near.Union(far);
        }

        public override string ToString() =>
            Id.Value + " cateto=" + VerticalLeg.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
    }
}
