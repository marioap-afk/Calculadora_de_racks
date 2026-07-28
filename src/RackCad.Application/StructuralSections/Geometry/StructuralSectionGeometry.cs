using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>How much detail the CALLER asked for.</summary>
    public enum SectionDetailLevel
    {
        /// <summary>Straight corners only. Always available, always cheap, never wrong about what it shows.</summary>
        Simplified = 0,

        /// <summary>Every detail the source lets us derive in a documented way — root fillets, rounded HSS corners.</summary>
        Tabulated = 1
    }

    /// <summary>
    /// How faithful the result actually IS, which is not the same as what was asked for.
    ///
    /// Keeping the two apart is the whole point: a caller asks for <see cref="SectionDetailLevel.Tabulated"/>
    /// and gets back a contour that says whether it managed it. A single "detail" field would force the
    /// builder either to lie or to fail, and the owner's decision 14 rules out both.
    /// </summary>
    public enum SectionFidelity
    {
        /// <summary>Straight corners, by request. Not a degradation — this is what <c>Simplified</c> means.</summary>
        Simplified = 0,

        /// <summary>Every detail the source publishes or lets us derive is present. Today only W reaches this.</summary>
        TabulatedComplete = 1,

        /// <summary>
        /// Derived detail is present, but the source does not publish everything the real shape has — the toe
        /// fillets of C and L, the flange taper of a channel. The contour is closer to reality than the
        /// simplified one and still not the whole truth, and it says so instead of implying exactness.
        /// </summary>
        TabulatedDerived = 2,

        /// <summary>
        /// <c>Tabulated</c> was requested and a required datum was missing, incoherent or non-physical, so the
        /// result fell back to straight corners. Always accompanied by a diagnostic naming what was missing.
        /// </summary>
        DegradedToSimplified = 3
    }

    public enum SectionDiagnosticSeverity
    {
        /// <summary>Worth knowing; the geometry is what it claims to be.</summary>
        Info = 0,

        /// <summary>A detail could not be produced and the result degraded. Never silent.</summary>
        Degraded = 1
    }

    /// <summary>One thing the builder wants the caller to know. The <see cref="Code"/> is the stable token.</summary>
    public sealed class SectionGeometryDiagnostic
    {
        public SectionGeometryDiagnostic(SectionDiagnosticSeverity severity, string code, string message)
        {
            Severity = severity;
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public SectionDiagnosticSeverity Severity { get; }

        /// <summary>Stable machine token, e.g. <c>SG_FILLET_NOT_DERIVABLE</c>. Tests key on this, never on the text.</summary>
        public string Code { get; }

        /// <summary>Spanish description for the user.</summary>
        public string Message { get; }

        public override string ToString() => "[" + Severity + "] " + Code + " — " + Message;
    }

    /// <summary>What a labelled point on the section MEANS. Only points with clear semantics get one.</summary>
    public enum SectionReferencePointKind
    {
        /// <summary>The area centroid, which after centring is the origin.</summary>
        Centroid = 0,

        /// <summary>The shear centre, where the source publishes it (channels).</summary>
        ShearCenter = 1,

        /// <summary>The heel of an angle: the outer corner where both legs meet.</summary>
        AngleHeel = 2,

        /// <summary>The back of a channel's web, the face its <c>x</c> is measured from.</summary>
        ChannelWebBack = 3
    }

    /// <summary>A named point of the section, in section coordinates.</summary>
    public sealed class SectionReferencePoint
    {
        public SectionReferencePoint(SectionReferencePointKind kind, Point2D location)
        {
            Kind = kind;
            Location = location;
        }

        public SectionReferencePointKind Kind { get; }

        public Point2D Location { get; }

        public override string ToString() => Kind + " " + Location;
    }

    /// <summary>
    /// The cross-section of one catalogued shape, as geometry.
    ///
    /// Coordinates are INCHES in the section plane, with the LOCAL axes of ADR-0022: X and Y span the section
    /// and Z — absent here on purpose — is the longitudinal axis of a prismatic instance. There is no length
    /// in this type and there never will be: a cross-section does not have one.
    ///
    /// The outer boundary is counter-clockwise and holes are clockwise, so the area of the whole thing is the
    /// plain sum of the signed areas. Everything is validated at construction.
    /// </summary>
    public sealed class StructuralSectionGeometry
    {
        private static readonly ClosedContour2D[] NoHoles = new ClosedContour2D[0];
        private static readonly SectionGeometryDiagnostic[] NoDiagnostics = new SectionGeometryDiagnostic[0];

        private StructuralSectionGeometry(
            StructuralSectionId sectionId,
            StructuralSectionFamily family,
            SectionDetailLevel requestedDetail,
            SectionFidelity fidelity,
            ClosedContour2D outer,
            ClosedContour2D[] holes,
            SectionReferencePoint[] referencePoints,
            SectionGeometryDiagnostic[] diagnostics)
        {
            SectionId = sectionId;
            Family = family;
            RequestedDetail = requestedDetail;
            Fidelity = fidelity;
            OuterContour = outer;
            Holes = holes;
            ReferencePoints = referencePoints;
            Diagnostics = diagnostics;

            Area = outer.Area - holes.Sum(hole => hole.Area);
            Bounds = outer.Bounds;
            Centroid = ComputeNetCentroid(outer, holes);
        }

        public StructuralSectionId SectionId { get; }

        public StructuralSectionFamily Family { get; }

        public SectionDetailLevel RequestedDetail { get; }

        public SectionFidelity Fidelity { get; }

        /// <summary>The outer boundary, always counter-clockwise.</summary>
        public ClosedContour2D OuterContour { get; }

        /// <summary>Interior voids, always clockwise. Only rectangular HSS has one today.</summary>
        public IReadOnlyList<ClosedContour2D> Holes { get; }

        /// <summary>Net area: the outer boundary minus the holes. Geometric, not the tabulated <c>A</c>.</summary>
        public double Area { get; }

        /// <summary>Bounding box of the outer boundary, arc bulges included.</summary>
        public Bounds2D Bounds { get; }

        /// <summary>
        /// The GEOMETRIC centroid of the material, computed from the contours.
        ///
        /// It is not assumed to be the origin: the contour is centred using the value the SOURCE tabulates, so
        /// for W and HSS this lands on (0,0) by symmetry, while for C and L it lands very close to it — the
        /// small residual is the detail the source does not publish, and reporting it is more useful than
        /// hiding it. <see cref="CentroidOffset"/> is that residual.
        /// </summary>
        public Point2D Centroid { get; }

        /// <summary>How far the geometric centroid sits from the origin, in inches.</summary>
        public double CentroidOffset => Math.Sqrt((Centroid.X * Centroid.X) + (Centroid.Y * Centroid.Y));

        public IReadOnlyList<SectionReferencePoint> ReferencePoints { get; }

        public IReadOnlyList<SectionGeometryDiagnostic> Diagnostics { get; }

        /// <summary>True when any diagnostic reports a degradation.</summary>
        public bool IsDegraded => Diagnostics.Any(d => d.Severity == SectionDiagnosticSeverity.Degraded);

        /// <summary>Outer boundary plus holes, in drawing order.</summary>
        public IEnumerable<ClosedContour2D> AllContours()
        {
            yield return OuterContour;

            foreach (var hole in Holes)
            {
                yield return hole;
            }
        }

        public static StructuralSectionGeometry Create(
            StructuralSectionId sectionId,
            StructuralSectionFamily family,
            SectionDetailLevel requestedDetail,
            SectionFidelity fidelity,
            ClosedContour2D outer,
            IEnumerable<ClosedContour2D> holes = null,
            IEnumerable<SectionReferencePoint> referencePoints = null,
            IEnumerable<SectionGeometryDiagnostic> diagnostics = null)
        {
            if (outer == null)
            {
                throw new ArgumentNullException(nameof(outer));
            }

            // Orientation is normalized here rather than trusted, so no builder can leak a hole that is
            // counter-clockwise and quietly add area instead of removing it.
            var normalizedOuter = outer.WithOrientation(ContourOrientation.CounterClockwise);
            var normalizedHoles = (holes ?? NoHoles)
                .Where(hole => hole != null)
                .Select(hole => hole.WithOrientation(ContourOrientation.Clockwise))
                .ToArray();

            var diagnosticList = (diagnostics ?? NoDiagnostics).Where(d => d != null).ToArray();

            if (fidelity == SectionFidelity.DegradedToSimplified &&
                !diagnosticList.Any(d => d.Severity == SectionDiagnosticSeverity.Degraded))
            {
                throw new ArgumentException(
                    "Una geometria degradada debe declarar por que: falta el diagnostico de degradacion.",
                    nameof(diagnostics));
            }

            foreach (var hole in normalizedHoles)
            {
                if (hole.Area >= normalizedOuter.Area)
                {
                    throw new ArgumentException(
                        "Un hueco no puede ser mayor que el contorno exterior.", nameof(holes));
                }
            }

            return new StructuralSectionGeometry(
                sectionId, family, requestedDetail, fidelity,
                normalizedOuter, normalizedHoles,
                (referencePoints ?? Enumerable.Empty<SectionReferencePoint>()).Where(p => p != null).ToArray(),
                diagnosticList);
        }

        /// <summary>Area centroid of the material: the outer contour minus each hole, weighted by area.</summary>
        private static Point2D ComputeNetCentroid(ClosedContour2D outer, IReadOnlyList<ClosedContour2D> holes)
        {
            var area = outer.Area;
            var momentX = outer.Centroid.X * area;
            var momentY = outer.Centroid.Y * area;

            foreach (var hole in holes)
            {
                area -= hole.Area;
                momentX -= hole.Centroid.X * hole.Area;
                momentY -= hole.Centroid.Y * hole.Area;
            }

            return GeometryTolerance.IsZero(area)
                ? new Point2D(0.0, 0.0)
                : new Point2D(momentX / area, momentY / area);
        }

        public override string ToString() =>
            SectionId.Value + " " + Family + " " + RequestedDetail + " -> " + Fidelity;
    }
}
