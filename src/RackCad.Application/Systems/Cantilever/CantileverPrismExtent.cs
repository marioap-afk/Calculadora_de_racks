using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The world extent of a PLACED prism's cross-section, constant along the extrusion.
    ///
    /// A line needs to know where a column's faces are: the separators butt against the X faces and the
    /// bracing plane sits on a Y face. Two ways of getting there were rejected. Reading <c>d</c> and
    /// <c>bf</c> would report a nominal number instead of the contour that gets drawn, which ADR-0024 D5
    /// forbids for exactly this reason. Re-deriving it from the frame at each call site would put the same
    /// mirror-then-rotate-then-transform chain in the separator resolver, the plate resolver and the brace
    /// resolver, where the three could drift apart.
    ///
    /// It maps the four corners of <c>Bounds</c> through the placement's own section transform and frame, so
    /// a mirrored or rotated member reports the extent it really occupies.
    /// </summary>
    public static class CantileverPrismExtent
    {
        /// <summary>
        /// The world axis-aligned box of the cross-section at the START of the run.
        ///
        /// For a prism extruded along a world axis this is also its extent everywhere along the run, in the
        /// two directions the extrusion does not consume. The caller reads the coordinates it needs.
        /// </summary>
        public static CantileverEnvelope3D CrossSection(
            PrismaticSectionInstance placement, StructuralSectionGeometry geometry)
        {
            if (placement == null)
            {
                throw new ArgumentNullException(nameof(placement));
            }

            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            return CantileverEnvelope3D.FromPoints(WorldCorners(placement, geometry));
        }

        /// <summary>The whole run: the cross-section at both ends, unioned.</summary>
        public static CantileverEnvelope3D Run(
            PrismaticSectionInstance placement, StructuralSectionGeometry geometry)
        {
            var start = CrossSection(placement, geometry);
            var along = placement.Frame.AxisZ * placement.Length;
            var end = CantileverEnvelope3D.FromPoints(
                WorldCorners(placement, geometry).Select(p => p + along));
            return start.Union(end);
        }

        private static IEnumerable<Point3D> WorldCorners(
            PrismaticSectionInstance placement, StructuralSectionGeometry geometry)
        {
            var bounds = geometry.Bounds;
            var transform = placement.SectionTransform();

            var corners = new[]
            {
                new Point2D(bounds.MinX, bounds.MinY),
                new Point2D(bounds.MaxX, bounds.MinY),
                new Point2D(bounds.MaxX, bounds.MaxY),
                new Point2D(bounds.MinX, bounds.MaxY)
            };

            // The transform is applied to the corners of the BOX, not to the contour. For a mirror and for a
            // rotation by a multiple of a quarter turn the two agree exactly; for any other rotation the box
            // of the transformed corners is a superset of the transformed contour's box. Every placement in
            // this system is axis-aligned, and a rotated one would report a slightly generous extent rather
            // than a wrong one.
            foreach (var corner in corners)
            {
                var placed = transform.Apply(corner);
                yield return placement.Frame.ToWorld(new Point3D(placed.X, placed.Y, 0.0));
            }
        }
    }
}
