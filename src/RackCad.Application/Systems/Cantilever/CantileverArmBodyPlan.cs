using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// A resolved arm BODY: one or two catalogued profiles, placed, with their shared cut and slope.
    ///
    /// <see cref="Members"/> is a flat list and its length is what the arrangement decides — one for a single
    /// profile, two for a paired channel. There is deliberately no subclass per arrangement: a consumer walks
    /// the list without asking where it came from, and the arrangement only matters to whoever placed them
    /// (ADR-0025, D1).
    /// </summary>
    public sealed class CantileverArmBodyPlan
    {
        private CantileverArmBodyPlan(
            CantileverArmBodyArrangement arrangement,
            CantileverArmSide side,
            IReadOnlyList<CantileverStructuralMemberPlan> members,
            double cutLength,
            double slopeRisePer12,
            double angleRadians,
            Bounds2D sectionBounds,
            CantileverEnvelope3D envelope,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            Arrangement = arrangement;
            Side = side;
            Members = members;
            CutLength = cutLength;
            SlopeRisePer12 = slopeRisePer12;
            AngleRadians = angleRadians;
            SectionBounds = sectionBounds;
            Envelope = envelope;
            Diagnostics = diagnostics;
        }

        public CantileverArmBodyArrangement Arrangement { get; }

        public CantileverArmSide Side { get; }

        /// <summary>
        /// The profiles that make up this body, in deterministic order. One or two; never zero.
        /// </summary>
        public IReadOnlyList<CantileverStructuralMemberPlan> Members { get; }

        /// <summary>
        /// The cut length shared by every member, inches. In a paired body both profiles are cut EXACTLY the
        /// same, which is asserted at construction rather than trusted.
        /// </summary>
        public double CutLength { get; }

        /// <summary>Rise per 12 inches. The stored authority.</summary>
        public double SlopeRisePer12 { get; }

        /// <summary>The derived angle, radians.</summary>
        public double AngleRadians { get; }

        public double AngleDegrees => AngleRadians * 180.0 / Math.PI;

        /// <summary>
        /// The COMBINED section extent in the body's own section coordinates: X across the members, Y their
        /// shared depth. It is what the plates take their size from, and it comes from I-36's <c>Bounds</c>.
        /// </summary>
        public Bounds2D SectionBounds { get; }

        /// <summary>Conservative world envelope of the placed body.</summary>
        public CantileverEnvelope3D Envelope { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        /// <summary>The longitudinal direction, taken from the first member's frame.</summary>
        public Vector3D Direction => Members[0].Placement.Frame.AxisZ;

        /// <summary>The section's own height, inches — the extent along its local Y.</summary>
        public double SectionHeight => SectionBounds.Height;

        /// <summary>The section's own width, inches — the extent across every member.</summary>
        public double SectionWidth => SectionBounds.Width;

        internal static CantileverArmBodyPlan Create(
            CantileverArmBodyArrangement arrangement,
            CantileverArmSide side,
            IReadOnlyList<CantileverStructuralMemberPlan> members,
            double slopeRisePer12,
            double angleRadians,
            Bounds2D sectionBounds,
            LocalFrame3D bodyFrame,
            IEnumerable<CantileverDiagnostic> diagnostics = null)
        {
            if (members == null || members.Count == 0)
            {
                throw new ArgumentException("Un cuerpo de brazo necesita al menos un miembro.", nameof(members));
            }

            var expected = CantileverArmBodyArrangementResolver.MemberCount(arrangement);

            if (members.Count != expected)
            {
                // The arrangement decides the cardinality, so a mismatch is a programming error and not a
                // user's. Catching it here is what stops a paired body from silently resolving to one profile.
                throw new InvalidOperationException(
                    "El arreglo '" + arrangement + "' exige " + expected + " miembros y se recibieron " +
                    members.Count + ".");
            }

            var cut = members[0].GeometricLength;

            foreach (var member in members)
            {
                if (Math.Abs(member.GeometricLength - cut) > GeometryTolerance.Length)
                {
                    throw new InvalidOperationException(
                        "Los miembros de un cuerpo compuesto deben tener el MISMO corte; se recibieron " +
                        cut + " y " + member.GeometricLength + ".");
                }

                if (member.Role != CantileverMemberRole.Arm)
                {
                    throw new InvalidOperationException(
                        "Un miembro del cuerpo del brazo debe tener rol Arm; se recibio " + member.Role + ".");
                }
            }

            // The envelope is computed ONCE, from the body's own frame and the COMBINED section box. Mapping
            // the combined box through each member's frame instead would inflate it by that member's
            // transverse offset — the box is already expressed against the body's centre.
            var envelope = CantileverEnvelope3D.FromPoints(CornerPoints(bodyFrame, sectionBounds, cut));

            return new CantileverArmBodyPlan(
                arrangement, side, members, cut, slopeRisePer12, angleRadians, sectionBounds, envelope,
                diagnostics == null
                    ? (IReadOnlyList<CantileverDiagnostic>)Array.Empty<CantileverDiagnostic>()
                    : diagnostics.ToList());
        }

        /// <summary>
        /// The eight corners of the body's combined section box at both ends, in world coordinates.
        ///
        /// The section box is used rather than the contour because an envelope is conservative by definition
        /// and a box is the honest shape of one; the real silhouette per view is what a representation plan
        /// reports, and I-37B does not build one.
        /// </summary>
        private static IEnumerable<Point3D> CornerPoints(
            LocalFrame3D frame, Bounds2D combined, double length)
        {
            foreach (var z in new[] { 0.0, length })
            {
                foreach (var x in new[] { combined.MinX, combined.MaxX })
                {
                    foreach (var y in new[] { combined.MinY, combined.MaxY })
                    {
                        yield return frame.ToWorld(new Point3D(x, y, z));
                    }
                }
            }
        }

        /// <summary>Deterministic fingerprint, rounded like the plans of I-36B and I-37A.</summary>
        public string Signature()
        {
            var parts = new List<string>
            {
                "arr=" + Arrangement,
                "side=" + Side,
                "cut=" + Format(CutLength),
                "slope=" + Format(SlopeRisePer12),
                "sec=" + Format(SectionBounds.MinX) + ".." + Format(SectionBounds.MaxX) + "|" +
                    Format(SectionBounds.MinY) + ".." + Format(SectionBounds.MaxY)
            };

            parts.AddRange(Members.Select(m =>
                m.Id.Value + "@" + Format(m.Start.X) + "," + Format(m.Start.Y) + "," + Format(m.Start.Z) +
                (m.Placement.Mirrored ? "|esp" : string.Empty)));

            return string.Join(";", parts);
        }

        private static string Format(double value) =>
            Math.Round(value, 6, MidpointRounding.AwayFromZero).ToString("0.######", CultureInfo.InvariantCulture);

        public override string ToString() =>
            Arrangement + " " + Side + " x" + Members.Count + " L=" + Format(CutLength);
    }
}
