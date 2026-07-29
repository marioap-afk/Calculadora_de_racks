using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// What a drawn curve MEANS, so a consumer can style it without guessing.
    ///
    /// Outside and inside are separate roles rather than one role plus a flag. A consumer that styles the
    /// bore of a tube differently from its outside — or that wants to leave it out of a silhouette — must be
    /// able to tell them apart without re-deriving which contour a curve came from, which is exactly the
    /// re-derivation ADR-0022 §7 forbids.
    /// </summary>
    public enum SectionCurveRole
    {
        /// <summary>The outer boundary of the section.</summary>
        OuterContour = 0,

        /// <summary>An interior void, such as the bore of an HSS.</summary>
        Hole = 1,

        /// <summary>The OUTER boundary drawn at one end of the run.</summary>
        EndProfile = 2,

        /// <summary>A longitudinal line joining the two ends, coming from the OUTER boundary.</summary>
        Generatrix = 3,

        /// <summary>The longitudinal centroidal axis.</summary>
        Axis = 4,

        /// <summary>The bounding envelope, drawn on request.</summary>
        Envelope = 5,

        /// <summary>An interior void drawn at one end of the run: the mouth of the bore.</summary>
        EndProfileHole = 6,

        /// <summary>
        /// A longitudinal line coming from an interior void.
        ///
        /// This is what makes a tube read as a tube instead of a solid bar: seen from the side, the two lines
        /// it adds sit exactly one nominal wall thickness inside the outer ones.
        /// </summary>
        InteriorGeneratrix = 7
    }

    /// <summary>How much of the prism to draw. Orthogonal to the section's detail level.</summary>
    public enum SectionRepresentationMode
    {
        /// <summary>End profiles plus the generatrices that join them. No hidden-line removal.</summary>
        Wireframe = 0,

        /// <summary>Only the bounding box of the projection.</summary>
        Envelope = 1,

        /// <summary>Only the longitudinal axis.</summary>
        Axis = 2
    }

    /// <summary>
    /// One polyline of the plan: an ordered, already-projected point list with a role.
    ///
    /// Curves arrive here FLATTENED. In the cross-section view an arc really is an arc, but in an isometric
    /// or any oblique view the projection of a circle is an ELLIPSE, and emitting it as an arc would be
    /// quietly wrong (ADR-0022 §15). Rather than carry two representations and have every consumer handle
    /// both, everything is tessellated with a stated chord tolerance — the same geometry the preview and
    /// AutoCAD receive.
    /// </summary>
    public sealed class SectionPlanCurve
    {
        public SectionPlanCurve(SectionCurveRole role, IReadOnlyList<Point2D> points, bool isClosed)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            if (points.Count < 2)
            {
                throw new ArgumentException("Una curva del plan necesita al menos dos puntos.", nameof(points));
            }

            // A closed curve must enclose something. Looking at a section exactly edge-on collapses its
            // contour onto a line, and emitting THAT as closed makes every consumer retrace each edge in
            // reverse — in AutoCAD, a zero-area polyline drawn twice over itself. The check lives in the type
            // rather than in the builder so no future code path can reintroduce it.
            if (isClosed && ProjectedDimensionality(points) < 2)
            {
                throw new ArgumentException(
                    "Una curva cerrada tiene que encerrar area: esta proyeccion colapsa a una recta o a un " +
                    "punto y debe emitirse abierta.", nameof(isClosed));
            }

            Role = role;
            Points = points;
            IsClosed = isClosed;
        }

        public SectionCurveRole Role { get; }

        public IReadOnlyList<Point2D> Points { get; }

        /// <summary>True when the last point joins back to the first (which is NOT repeated in the list).</summary>
        public bool IsClosed { get; }

        /// <summary>
        /// How many dimensions the point set actually spans: 0 for a point, 1 for a line, 2 otherwise.
        ///
        /// The tolerance is relative to the extent so the answer does not depend on whether the piece is 4
        /// inches or 400.
        /// </summary>
        public static int ProjectedDimensionality(IReadOnlyList<Point2D> points)
        {
            if (points == null || points.Count == 0)
            {
                return 0;
            }

            var bounds = Bounds2D.FromPoints(points);
            var extent = Math.Max(bounds.Width, bounds.Height);
            var tolerance = Math.Max(GeometryTolerance.Continuity, extent * 1e-9);

            if (extent <= tolerance)
            {
                return 0;
            }

            var origin = points[0];
            var direction = Vector2D.Zero;

            foreach (var point in points)
            {
                var candidate = Vector2D.Between(origin, point);

                if (candidate.Length > tolerance)
                {
                    direction = candidate.Normalized();
                    break;
                }
            }

            foreach (var point in points)
            {
                if (Math.Abs(direction.Cross(Vector2D.Between(origin, point))) > tolerance)
                {
                    return 2;
                }
            }

            return 1;
        }

        public override string ToString() => Role + " (" + Points.Count + " puntos)";
    }

    /// <summary>
    /// The single neutral result that the preview and the AutoCAD adapter both consume.
    ///
    /// This is the boundary ADR-0022 §7 draws: everything above it is pure geometry in Application, everything
    /// below is a renderer. Neither the UI nor the plugin recomputes a section dimension — they receive points
    /// and roles. Two generators would be two truths, and the repository already paid for that once with the
    /// preview projections I-30 had to unify.
    ///
    /// It is a WIREFRAME. There is no hidden-line removal, so in an isometric the far end profile is drawn
    /// too. That is stated rather than fixed: an HLR pass is expensive, fragile, and when it fails it produces
    /// drawings that are subtly wrong rather than obviously broken.
    /// </summary>
    public sealed class StructuralSectionRepresentationPlan
    {
        public StructuralSectionRepresentationPlan(
            StructuralSectionId sectionId,
            StructuralSectionFamily family,
            double length,
            SectionViewKind view,
            SectionRepresentationMode mode,
            SectionDetailLevel requestedDetail,
            SectionFidelity fidelity,
            double chordTolerance,
            IReadOnlyList<SectionPlanCurve> curves,
            IReadOnlyList<SectionGeometryDiagnostic> diagnostics,
            SectionGeometryAuthority authority = SectionGeometryAuthority.TabulatedConstrained)
        {
            Authority = authority;
            SectionId = sectionId;
            Family = family;
            Length = length;
            View = view;
            Mode = mode;
            RequestedDetail = requestedDetail;
            Fidelity = fidelity;
            ChordTolerance = chordTolerance;
            Curves = curves ?? throw new ArgumentNullException(nameof(curves));
            Diagnostics = diagnostics ?? new SectionGeometryDiagnostic[0];

            if (curves.Count == 0)
            {
                throw new ArgumentException("Un plan sin curvas no representa nada.", nameof(curves));
            }

            Bounds = Bounds2D.FromPoints(curves.SelectMany(curve => curve.Points));
        }

        public StructuralSectionId SectionId { get; }

        public StructuralSectionFamily Family { get; }

        /// <summary>Length of the run this plan represents, in inches.</summary>
        public double Length { get; }

        public SectionViewKind View { get; }

        public SectionRepresentationMode Mode { get; }

        public SectionDetailLevel RequestedDetail { get; }

        /// <summary>The fidelity the underlying section geometry achieved. Travels with the plan on purpose.</summary>
        public SectionFidelity Fidelity { get; }

        /// <summary>
        /// Whose geometry this plan draws (ADR-0023). Travels with the plan for the same reason the fidelity
        /// does: the consumer that renders it is the one that has to warn about it, and it must not have to
        /// look at the family to find out.
        /// </summary>
        public SectionGeometryAuthority Authority { get; }

        /// <summary>True when the plan carries a declared RackCad convention and needs a visible warning.</summary>
        public bool IsVisualDerived => Authority == SectionGeometryAuthority.VisualDerived;

        /// <summary>Chord tolerance used to flatten arcs, in inches.</summary>
        public double ChordTolerance { get; }

        public IReadOnlyList<SectionPlanCurve> Curves { get; }

        public Bounds2D Bounds { get; }

        public IReadOnlyList<SectionGeometryDiagnostic> Diagnostics { get; }

        public IEnumerable<SectionPlanCurve> CurvesOf(SectionCurveRole role) =>
            Curves.Where(curve => curve.Role == role);

        /// <summary>
        /// A deterministic fingerprint of what the plan draws.
        ///
        /// Two plans built from the same inputs produce the same string, which is what lets a test assert
        /// "nothing moved" without listing thousands of coordinates. Rounded to six decimals — a millionth of
        /// an inch — so it is stable against the last bits of floating point while still catching any real
        /// change.
        /// </summary>
        public string Signature()
        {
            var builder = new StringBuilder();
            builder.Append(SectionId.Value).Append('|').Append(Family).Append('|')
                .Append(Length.ToString("0.######", CultureInfo.InvariantCulture)).Append('|')
                .Append(View).Append('|').Append(Mode).Append('|')
                .Append(RequestedDetail).Append('|').Append(Fidelity).Append('|').Append(Authority).Append('|')
                .Append(ChordTolerance.ToString("0.########", CultureInfo.InvariantCulture));

            foreach (var curve in Curves)
            {
                builder.Append("\n").Append(curve.Role).Append(curve.IsClosed ? "*" : "-");

                foreach (var point in curve.Points)
                {
                    builder.Append(';')
                        .Append(point.X.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                        .Append(point.Y.ToString("0.######", CultureInfo.InvariantCulture));
                }
            }

            return builder.ToString();
        }

        public override string ToString() =>
            SectionId.Value + " " + View + " " + Mode + " L=" +
            Length.ToString("0.###", CultureInfo.InvariantCulture) + " (" + Curves.Count + " curvas)";
    }
}
