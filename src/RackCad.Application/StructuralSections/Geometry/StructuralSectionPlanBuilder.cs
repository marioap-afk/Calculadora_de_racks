using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>What to draw and how finely. Everything the plan builder needs beyond the instance itself.</summary>
    public sealed class SectionRepresentationOptions
    {
        /// <summary>
        /// Default chord tolerance for flattening arcs: one thousandth of an inch.
        ///
        /// Chosen against what the drawing is for. The finest real feature in the catalogue is a 0.1 in.
        /// inner corner radius, and 0.001 in. splits its quarter-turn into about seven chords — visually
        /// smooth at any preview zoom and far below what a fabricator would ever measure. Ten times coarser
        /// starts to show facets on small tubes; ten times finer multiplies the point count for nothing.
        /// </summary>
        public const double DefaultChordTolerance = 0.001;

        public SectionViewpoint Viewpoint { get; init; } = SectionViewpoint.CrossSection;

        public SectionRepresentationMode Mode { get; init; } = SectionRepresentationMode.Wireframe;

        public SectionDetailLevel Detail { get; init; } = SectionDetailLevel.Tabulated;

        public double ChordTolerance { get; init; } = DefaultChordTolerance;

        /// <summary>Draw the longitudinal centroidal axis alongside the wireframe.</summary>
        public bool IncludeAxis { get; init; }

        /// <summary>Draw the bounding envelope of the projection.</summary>
        public bool IncludeEnvelope { get; init; }
    }

    /// <summary>
    /// Turns a section plus a length plus a viewpoint into the neutral plan.
    ///
    /// The whole pipeline lives here and nowhere else: section geometry → mirror and rotation → lift to the
    /// two ends of the run → place through the instance frame → project → flatten. A consumer receives points
    /// and roles, and cannot reach back into the section's dimensions even if it wanted to.
    /// </summary>
    public static class StructuralSectionPlanBuilder
    {
        public static StructuralSectionRepresentationPlan Build(
            StructuralSectionGeometry geometry,
            PrismaticSectionInstance instance,
            SectionRepresentationOptions options = null)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (geometry.SectionId != instance.SectionId)
            {
                throw new ArgumentException(
                    "La geometria es de '" + geometry.SectionId + "' y la instancia de '" +
                    instance.SectionId + "'.", nameof(instance));
            }

            options = options ?? new SectionRepresentationOptions();
            GeometryTolerance.RequirePositive(options.ChordTolerance, nameof(options.ChordTolerance));

            var viewpoint = options.Viewpoint ?? SectionViewpoint.CrossSection;
            var sectionTransform = instance.SectionTransform();
            var curves = new List<SectionPlanCurve>();

            switch (options.Mode)
            {
                case SectionRepresentationMode.Wireframe:
                    AddWireframe(curves, geometry, instance, viewpoint, sectionTransform, options);
                    break;

                case SectionRepresentationMode.Envelope:
                    AddEnvelopeOnly(curves, geometry, instance, viewpoint, sectionTransform, options);
                    break;

                case SectionRepresentationMode.Axis:
                    AddAxis(curves, instance, viewpoint);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(options), options.Mode, "Modo desconocido.");
            }

            // The piece is canonicalized BEFORE the annotations are added, so the axis and the envelope are
            // computed over the ink that will actually be drawn and are never eaten by it.
            curves = SectionProjectionCanonicalizer.Canonicalize(curves).ToList();

            if (options.IncludeAxis && options.Mode != SectionRepresentationMode.Axis)
            {
                AddAxis(curves, instance, viewpoint);
            }

            if (options.IncludeEnvelope && options.Mode != SectionRepresentationMode.Envelope)
            {
                AddEnvelope(curves, curves.SelectMany(c => c.Points).ToArray());
            }

            return new StructuralSectionRepresentationPlan(
                geometry.SectionId,
                geometry.Family,
                instance.Length,
                viewpoint.Kind,
                options.Mode,
                geometry.RequestedDetail,
                geometry.Fidelity,
                options.ChordTolerance,
                curves,
                geometry.Diagnostics,
                geometry.Authority);
        }

        /// <summary>
        /// End profiles at both ends plus the generatrices joining them.
        ///
        /// In the cross-section view the two ends project on top of each other, so only one is emitted and no
        /// generatrix is drawn: duplicating them would double every entity in the drawing for nothing.
        ///
        /// EVERY contour takes part, holes included. A tube whose bore stops at the end faces would be
        /// drawing a solid bar: the two extra lines its bore adds, one nominal wall inside the outer ones,
        /// are the whole reason a side view of a tube reads as hollow.
        /// </summary>
        private static void AddWireframe(
            ICollection<SectionPlanCurve> curves,
            StructuralSectionGeometry geometry,
            PrismaticSectionInstance instance,
            SectionViewpoint viewpoint,
            Transform2D sectionTransform,
            SectionRepresentationOptions options)
        {
            // The MEMBER's own axis, not world Z: a camera looking down preserves a standing column's shape and
            // not a base lying on the floor, and only the placement can tell them apart.
            var alongZ = viewpoint.PreservesShapeOf(instance.Frame.AxisZ);
            var ends = alongZ ? new[] { 0.0 } : new[] { 0.0, instance.Length };

            var contours = geometry.AllContours().ToArray();

            foreach (var z in ends)
            {
                for (var i = 0; i < contours.Length; i++)
                {
                    var projected = contours[i].Flatten(options.ChordTolerance)
                        .Select(p => Project(p, z, instance, viewpoint, sectionTransform))
                        .ToArray();

                    AddProfile(curves, ProfileRole(alongZ, isHole: i > 0), Dedupe(projected));
                }
            }

            if (alongZ)
            {
                return;
            }

            // Generatrices: the silhouette of the prism. Every vertex of a contour would produce a line,
            // which for a tessellated arc is hundreds of nearly-coincident ones; only the corners of the
            // un-flattened contour are used, which is exactly the silhouette a draughtsman would draw.
            for (var i = 0; i < contours.Length; i++)
            {
                var role = i == 0 ? SectionCurveRole.Generatrix : SectionCurveRole.InteriorGeneratrix;

                foreach (var vertex in SilhouetteVertices(contours[i]))
                {
                    var start = Project(vertex, 0.0, instance, viewpoint, sectionTransform);
                    var end = Project(vertex, instance.Length, instance, viewpoint, sectionTransform);

                    if (!start.ApproxEquals(end, GeometryTolerance.Continuity))
                    {
                        curves.Add(new SectionPlanCurve(role, new[] { start, end }, isClosed: false));
                    }
                }
            }
        }

        /// <summary>Which role a projected contour carries: outside or bore, cross-section or end face.</summary>
        private static SectionCurveRole ProfileRole(bool alongZ, bool isHole)
        {
            if (alongZ)
            {
                return isHole ? SectionCurveRole.Hole : SectionCurveRole.OuterContour;
            }

            return isHole ? SectionCurveRole.EndProfileHole : SectionCurveRole.EndProfile;
        }

        /// <summary>
        /// Emits a projected contour, closed when it still encloses area and open when the view flattened it.
        ///
        /// Looking exactly along X or Y squashes the whole cross-section onto a line. What the drawing wants
        /// there is the single edge a person sees, not a closed polyline that walks the edge and walks back.
        /// </summary>
        private static void AddProfile(
            ICollection<SectionPlanCurve> curves, SectionCurveRole role, IReadOnlyList<Point2D> points)
        {
            switch (SectionPlanCurve.ProjectedDimensionality(points))
            {
                case 2:
                    curves.Add(new SectionPlanCurve(role, points, isClosed: true));
                    return;

                case 1:
                    foreach (var piece in SectionProjectionCanonicalizer.Flatten(role, points))
                    {
                        curves.Add(piece);
                    }

                    return;

                default:
                    // A contour that projects to a single point draws nothing at all.
                    return;
            }
        }

        private static void AddEnvelopeOnly(
            ICollection<SectionPlanCurve> curves,
            StructuralSectionGeometry geometry,
            PrismaticSectionInstance instance,
            SectionViewpoint viewpoint,
            Transform2D sectionTransform,
            SectionRepresentationOptions options)
        {
            // The MEMBER's own axis, not world Z: a camera looking down preserves a standing column's shape and
            // not a base lying on the floor, and only the placement can tell them apart.
            var alongZ = viewpoint.PreservesShapeOf(instance.Frame.AxisZ);
            var ends = alongZ ? new[] { 0.0 } : new[] { 0.0, instance.Length };

            var points = ends
                .SelectMany(z => geometry.OuterContour.Flatten(options.ChordTolerance)
                    .Select(p => Project(p, z, instance, viewpoint, sectionTransform)))
                .ToArray();

            AddEnvelope(curves, points);
        }

        /// <summary>
        /// The bounding box of what is drawn.
        ///
        /// When the drawing is itself flat — asking for the axis alone in a longitudinal view leaves a single
        /// straight line — the box has no area, and a closed rectangle of zero area is the same defect the
        /// end profiles had. It degrades to the segment it really is.
        /// </summary>
        private static void AddEnvelope(ICollection<SectionPlanCurve> curves, IReadOnlyList<Point2D> points)
        {
            if (points.Count == 0)
            {
                return;
            }

            var bounds = Bounds2D.FromPoints(points);

            if (!bounds.HasArea)
            {
                if (bounds.Width > GeometryTolerance.Length || bounds.Height > GeometryTolerance.Length)
                {
                    curves.Add(new SectionPlanCurve(
                        SectionCurveRole.Envelope,
                        new[] { bounds.Min, bounds.Max },
                        isClosed: false));
                }

                return;
            }

            curves.Add(new SectionPlanCurve(
                SectionCurveRole.Envelope,
                new[]
                {
                    new Point2D(bounds.MinX, bounds.MinY),
                    new Point2D(bounds.MaxX, bounds.MinY),
                    new Point2D(bounds.MaxX, bounds.MaxY),
                    new Point2D(bounds.MinX, bounds.MaxY)
                },
                isClosed: true));
        }

        /// <summary>
        /// The longitudinal centroidal axis. In the cross-section view it projects to a single point, so a
        /// short cross marker is drawn instead of a zero-length line.
        /// </summary>
        private static void AddAxis(
            ICollection<SectionPlanCurve> curves,
            PrismaticSectionInstance instance,
            SectionViewpoint viewpoint)
        {
            var start = viewpoint.Project(instance.Frame.ToWorld(new Point2D(0.0, 0.0), 0.0));
            var end = viewpoint.Project(instance.Frame.ToWorld(new Point2D(0.0, 0.0), instance.Length));

            if (start.ApproxEquals(end, GeometryTolerance.Continuity))
            {
                var size = Math.Max(instance.Length * 0.02, 0.25);
                curves.Add(new SectionPlanCurve(
                    SectionCurveRole.Axis,
                    new[] { new Point2D(start.X - size, start.Y), new Point2D(start.X + size, start.Y) },
                    isClosed: false));
                curves.Add(new SectionPlanCurve(
                    SectionCurveRole.Axis,
                    new[] { new Point2D(start.X, start.Y - size), new Point2D(start.X, start.Y + size) },
                    isClosed: false));
                return;
            }

            curves.Add(new SectionPlanCurve(SectionCurveRole.Axis, new[] { start, end }, isClosed: false));
        }

        /// <summary>
        /// Section point → mirrored and rotated → lifted to z → placed by the instance frame → projected.
        /// The one path every coordinate in a plan travels.
        /// </summary>
        private static Point2D Project(
            Point2D sectionPoint,
            double z,
            PrismaticSectionInstance instance,
            SectionViewpoint viewpoint,
            Transform2D sectionTransform)
        {
            var oriented = sectionTransform.Apply(sectionPoint);
            return viewpoint.Project(instance.Frame.ToWorld(oriented, z));
        }

        /// <summary>The corner vertices of a contour, before flattening: where a generatrix belongs.</summary>
        private static IEnumerable<Point2D> SilhouetteVertices(ClosedContour2D contour) =>
            contour.Segments.Select(segment => segment.Start);

        /// <summary>Drops consecutive duplicates a projection can create when two points collapse.</summary>
        private static IReadOnlyList<Point2D> Dedupe(IReadOnlyList<Point2D> points)
        {
            var result = new List<Point2D>(points.Count);

            foreach (var point in points)
            {
                if (result.Count == 0 || !result[result.Count - 1].ApproxEquals(point, GeometryTolerance.Continuity))
                {
                    result.Add(point);
                }
            }

            // A closed curve must not repeat its first point at the end either.
            while (result.Count > 2 && result[result.Count - 1].ApproxEquals(result[0], GeometryTolerance.Continuity))
            {
                result.RemoveAt(result.Count - 1);
            }

            return result;
        }
    }
}
