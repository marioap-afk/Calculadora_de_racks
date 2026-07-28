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

    /// <summary>
    /// WHOSE the contour is, which is a different question from how much detail it has.
    ///
    /// <see cref="SectionFidelity"/> answers "how complete is this?". This answers "who authored it?". They
    /// are orthogonal and ADR-0023 keeps them apart on purpose: collapsing the authority into the fidelity
    /// enum would force a single value to mean both, and the first consumer that needed one without the other
    /// would have to re-derive it from the family — the re-derivation ADR-0022 already forbids elsewhere.
    /// </summary>
    public enum SectionGeometryAuthority
    {
        /// <summary>
        /// Every point is traceable to a published datum or to a derivation whose rule lives in ADR-0022.
        /// This is W, HSS, C and L, and it is the default: a new family does not get to be creative silently.
        /// </summary>
        TabulatedConstrained = 0,

        /// <summary>
        /// The contour embeds a RackCad convention the source does not publish (ADR-0023). Today only S, whose
        /// flange slope AISC does not tabulate. A consumer showing this geometry must say so.
        /// </summary>
        VisualDerived = 1
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

    /// <summary>
    /// How the transverse origin of a section was resolved.
    ///
    /// It is recorded rather than assumed because the two answers have different standing. A doubly
    /// symmetric shape is centred by CONSTRUCTION and the origin is exact; an asymmetric one is centred on
    /// the value AISC publishes, rounded to three figures, and the origin is exact only in the sense that it
    /// is where the source says the centroid is.
    /// </summary>
    public enum SectionOriginBasis
    {
        /// <summary>The shape is symmetric about both axes, so the origin falls out of the construction.</summary>
        Symmetry = 0,

        /// <summary>The contour was translated so the origin sits at the centroid the SOURCE tabulates.</summary>
        TabulatedCentroid = 1
    }

    /// <summary>What a labelled point on the section MEANS. Only points with clear semantics get one.</summary>
    public enum SectionReferencePointKind
    {
        /// <summary>
        /// The centroid AISC tabulates, which after centring is the origin.
        ///
        /// Named for its SOURCE, not just "centroid", so it can never be confused with
        /// <see cref="StructuralSectionGeometry.GeometricContourCentroid"/> — the centroid of the
        /// approximated contour, which is a diagnostic and not a placement authority.
        /// </summary>
        TabulatedCentroid = 0,

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
            SectionOriginBasis originBasis,
            SectionReferencePoint[] referencePoints,
            SectionGeometryDiagnostic[] diagnostics,
            SectionGeometryAuthority authority)
        {
            Authority = authority;
            SectionId = sectionId;
            Family = family;
            RequestedDetail = requestedDetail;
            Fidelity = fidelity;
            OuterContour = outer;
            Holes = holes;
            OriginBasis = originBasis;
            ReferencePoints = referencePoints;
            Diagnostics = diagnostics;

            Area = outer.Area - holes.Sum(hole => hole.Area);
            Bounds = outer.Bounds;
            GeometricContourCentroid = ComputeNetCentroid(outer, holes);
        }

        public StructuralSectionId SectionId { get; }

        public StructuralSectionFamily Family { get; }

        public SectionDetailLevel RequestedDetail { get; }

        public SectionFidelity Fidelity { get; }

        /// <summary>
        /// Whose contour this is: tabulated-constrained, or carrying a declared RackCad convention.
        ///
        /// Travels with the geometry and with the plan so no consumer has to look at the family to find out.
        /// </summary>
        public SectionGeometryAuthority Authority { get; }

        /// <summary>True when the contour embeds a RackCad convention and must be shown with a warning.</summary>
        public bool IsVisualDerived => Authority == SectionGeometryAuthority.VisualDerived;

        /// <summary>The outer boundary, always counter-clockwise.</summary>
        public ClosedContour2D OuterContour { get; }

        /// <summary>Interior voids, always clockwise. Only rectangular HSS has one today.</summary>
        public IReadOnlyList<ClosedContour2D> Holes { get; }

        /// <summary>Net area: the outer boundary minus the holes. Geometric, not the tabulated <c>A</c>.</summary>
        public double Area { get; }

        /// <summary>Bounding box of the outer boundary, arc bulges included.</summary>
        public Bounds2D Bounds { get; }

        /// <summary>
        /// The transverse origin of the section: always (0,0), by definition of these coordinates.
        ///
        /// It exists as a named property rather than as an implicit convention because the whole point of
        /// ADR-0022 §5 is that a section has ONE origin and everything composes against it. Read
        /// <see cref="OriginBasis"/> to know how it was resolved.
        /// </summary>
        public Point2D Origin => new Point2D(0.0, 0.0);

        /// <summary>How <see cref="Origin"/> was resolved: by symmetry, or on the tabulated centroid.</summary>
        public SectionOriginBasis OriginBasis { get; }

        /// <summary>
        /// The centroid of the APPROXIMATED contour, computed from its area.
        ///
        /// This is a DIAGNOSTIC and never a placement authority. The section is positioned on the centroid
        /// the source tabulates; this one is what the contour we could derive happens to have, and the two
        /// differ by whatever the source does not publish — the toe rounding of a channel, the taper of its
        /// flange. Comparing them measures the approximation, which is useful; moving the geometry to make
        /// them agree would silently overrule the source with our own incomplete contour, which is not.
        ///
        /// For W and HSS it lands on the origin by symmetry. For C and L it lands near it, and
        /// <see cref="GeometricCentroidResidual"/> says how near.
        /// </summary>
        public Point2D GeometricContourCentroid { get; }

        /// <summary>
        /// Distance from <see cref="Origin"/> to <see cref="GeometricContourCentroid"/>, in inches.
        ///
        /// A measure of how much the derivable contour differs from the real shape. It is reported, not
        /// corrected: see <see cref="GeometricContourCentroid"/>.
        /// </summary>
        public double GeometricCentroidResidual =>
            Math.Sqrt((GeometricContourCentroid.X * GeometricContourCentroid.X) +
                      (GeometricContourCentroid.Y * GeometricContourCentroid.Y));

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
            SectionOriginBasis originBasis = SectionOriginBasis.Symmetry,
            IEnumerable<SectionReferencePoint> referencePoints = null,
            IEnumerable<SectionGeometryDiagnostic> diagnostics = null,
            SectionGeometryAuthority authority = SectionGeometryAuthority.TabulatedConstrained)
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

            // A visually derived contour without its warning would be exactly the silent authorship ADR-0023
            // exists to prevent, so the type refuses to build one.
            if (authority == SectionGeometryAuthority.VisualDerived &&
                !diagnosticList.Any(d => d.Code == SectionGeometryDiagnostics.VisualConventionApplied))
            {
                throw new ArgumentException(
                    "Una geometria de autoridad visual derivada debe declararlo: falta el diagnostico " +
                    SectionGeometryDiagnostics.VisualConventionApplied + ".",
                    nameof(diagnostics));
            }

            return new StructuralSectionGeometry(
                sectionId, family, requestedDetail, fidelity,
                normalizedOuter, normalizedHoles, originBasis,
                (referencePoints ?? Enumerable.Empty<SectionReferencePoint>()).Where(p => p != null).ToArray(),
                diagnosticList,
                authority);
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

        /// <summary>The tabulated reference point that resolved the origin, when the source published one.</summary>
        public SectionReferencePoint TabulatedCentroidReference =>
            ReferencePoints.FirstOrDefault(p => p.Kind == SectionReferencePointKind.TabulatedCentroid);
    }
}
