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
                geometry.Diagnostics);
        }

        /// <summary>
        /// End profiles at both ends plus the generatrices joining them.
        ///
        /// In the cross-section view the two ends project on top of each other, so only one is emitted and no
        /// generatrix is drawn: duplicating them would double every entity in the drawing for nothing.
        /// </summary>
        private static void AddWireframe(
            ICollection<SectionPlanCurve> curves,
            StructuralSectionGeometry geometry,
            PrismaticSectionInstance instance,
            SectionViewpoint viewpoint,
            Transform2D sectionTransform,
            SectionRepresentationOptions options)
        {
            var alongZ = viewpoint.PreservesSectionShape;
            var ends = alongZ ? new[] { 0.0 } : new[] { 0.0, instance.Length };
            var role = alongZ ? SectionCurveRole.OuterContour : SectionCurveRole.EndProfile;

            var contours = geometry.AllContours().ToArray();

            foreach (var z in ends)
            {
                for (var i = 0; i < contours.Length; i++)
                {
                    var flattened = contours[i].Flatten(options.ChordTolerance);
                    var projected = flattened
                        .Select(p => Project(p, z, instance, viewpoint, sectionTransform))
                        .ToArray();

                    var curveRole = alongZ
                        ? (i == 0 ? SectionCurveRole.OuterContour : SectionCurveRole.Hole)
                        : role;

                    curves.Add(new SectionPlanCurve(curveRole, Dedupe(projected), isClosed: true));
                }
            }

            if (alongZ)
            {
                return;
            }

            // Generatrices: the silhouette of the prism. Every vertex of the OUTER contour would produce a
            // line, which for a tessellated arc is hundreds of nearly-coincident ones; only the corners of
            // the un-flattened contour are used, which is exactly the silhouette a draughtsman would draw.
            foreach (var vertex in SilhouetteVertices(contours[0]))
            {
                var start = Project(vertex, 0.0, instance, viewpoint, sectionTransform);
                var end = Project(vertex, instance.Length, instance, viewpoint, sectionTransform);

                if (!start.ApproxEquals(end, GeometryTolerance.Continuity))
                {
                    curves.Add(new SectionPlanCurve(
                        SectionCurveRole.Generatrix, new[] { start, end }, isClosed: false));
                }
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
            var alongZ = viewpoint.PreservesSectionShape;
            var ends = alongZ ? new[] { 0.0 } : new[] { 0.0, instance.Length };

            var points = ends
                .SelectMany(z => geometry.OuterContour.Flatten(options.ChordTolerance)
                    .Select(p => Project(p, z, instance, viewpoint, sectionTransform)))
                .ToArray();

            AddEnvelope(curves, points);
        }

        private static void AddEnvelope(ICollection<SectionPlanCurve> curves, IReadOnlyList<Point2D> points)
        {
            if (points.Count == 0)
            {
                return;
            }

            var bounds = Bounds2D.FromPoints(points);

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
