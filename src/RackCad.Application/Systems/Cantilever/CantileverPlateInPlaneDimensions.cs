using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The two dimensions of a plate measured IN ITS OWN PLANE, plus which world direction each one runs along.
    /// </summary>
    public readonly struct CantileverPlateInPlaneSize
    {
        internal CantileverPlateInPlaneSize(
            double width, double height, Vector3D widthAxis, Vector3D heightAxis)
        {
            Width = width;
            Height = height;
            WidthAxis = widthAxis;
            HeightAxis = heightAxis;
        }

        /// <summary>The dimension across the plate: the arm's transverse direction for an end plate.</summary>
        public double Width { get; }

        /// <summary>The dimension up the plate: the arm's depth/up direction for an end plate.</summary>
        public double Height { get; }

        /// <summary>Unit direction <see cref="Width"/> is measured along, in world terms.</summary>
        public Vector3D WidthAxis { get; }

        /// <summary>Unit direction <see cref="Height"/> is measured along, in world terms.</summary>
        public Vector3D HeightAxis { get; }

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture, "{0:0.######}x{1:0.######}", Width, Height);

        public override string ToString() => "InPlane " + Signature();
    }

    /// <summary>
    /// THE authority that measures a plate inside its own plane.
    ///
    /// It replaced a world-axis bounding box, and the difference is not cosmetic. The earlier code took the
    /// three world spans of the outline and used the largest two — which is correct only while the plate is
    /// parallel to a world plane. An arm's end plate is perpendicular to a SLOPED axis, so its world spans
    /// are projections: tilt the arm and a 10 in tall cap starts reporting 9.8 in, then 9.4 in, and the BOM
    /// splits one physical plate into several as the slope changes. The plate never changed size.
    ///
    /// Measuring in-plane also fixes which dimension is which. World spans come back sorted, so a section
    /// taller than it is wide and one wider than it is tall describe themselves identically; here the width is
    /// the plate's first edge and the height the second, so the two stay distinguishable.
    ///
    /// It VALIDATES rather than assumes: enough corners, non-degenerate edges, a planar outline and finite
    /// positive dimensions. A plate that fails any of those has no in-plane size, and saying so is better than
    /// returning a number derived from a degenerate contour.
    /// </summary>
    public static class CantileverPlateInPlaneDimensions
    {
        /// <summary>
        /// How far from perpendicular two edges may be, and how far off-plane a corner may sit.
        ///
        /// It is the same geometric tolerance the rest of Cantilever uses for a fit. A looser value would let a
        /// visibly skewed outline pass as a rectangle; a tighter one would reject plates that only differ from
        /// rectangular by floating-point noise.
        /// </summary>
        public const double PlanarTolerance = 1e-6;

        /// <summary>
        /// Measures a plate, or reports why it cannot be measured.
        /// </summary>
        public static bool TryMeasure(
            IReadOnlyList<Point3D> outline, out CantileverPlateInPlaneSize size, out string reason)
        {
            size = default;
            reason = null;

            if (outline == null || outline.Count < 3)
            {
                reason = "el contorno necesita al menos tres vertices";
                return false;
            }

            // The first two edges of the outline ARE the plate's own directions: every Cantilever plate is
            // built as a rectangle in a known order, so reading the edges is reading the plate's intent rather
            // than guessing it from a box.
            var across = outline[1] - outline[0];
            var up = outline[2] - outline[1];

            if (!across.IsFinite || !up.IsFinite)
            {
                reason = "una arista no es finita";
                return false;
            }

            var width = across.Length;
            var height = up.Length;

            if (width <= PlanarTolerance || height <= PlanarTolerance)
            {
                reason = "una arista es degenerada (" +
                         width.ToString("0.######", CultureInfo.InvariantCulture) + " x " +
                         height.ToString("0.######", CultureInfo.InvariantCulture) + ")";
                return false;
            }

            var widthAxis = across.Normalized();
            var heightAxis = up.Normalized();

            if (Math.Abs(widthAxis.Dot(heightAxis)) > PlanarTolerance)
            {
                reason = "las dos aristas no son perpendiculares";
                return false;
            }

            // Planarity: every remaining corner must lie in the plane the first three define. Without it a
            // folded outline would report the size of one of its halves.
            var normal = widthAxis.Cross(heightAxis);

            if (normal.Length <= PlanarTolerance)
            {
                reason = "el contorno no define un plano";
                return false;
            }

            normal = normal.Normalized();

            foreach (var corner in outline)
            {
                if (Math.Abs((corner - outline[0]).Dot(normal)) > PlanarTolerance)
                {
                    reason = "el contorno no es plano";
                    return false;
                }
            }

            size = new CantileverPlateInPlaneSize(width, height, widthAxis, heightAxis);
            return true;
        }

        /// <summary>
        /// The same measurement, throwing when the outline cannot be measured.
        ///
        /// For callers that hold a plate a resolver already built: those outlines are rectangles by
        /// construction, so a failure there is a programming error rather than bad input.
        /// </summary>
        public static CantileverPlateInPlaneSize Measure(IReadOnlyList<Point3D> outline)
        {
            if (!TryMeasure(outline, out var size, out var reason))
            {
                throw new ArgumentException(
                    "El contorno no se puede medir en su plano: " + reason + ".", nameof(outline));
            }

            return size;
        }

        /// <summary>Measures a plate plan.</summary>
        public static CantileverPlateInPlaneSize Measure(CantileverPlatePlan plate) =>
            Measure((plate ?? throw new ArgumentNullException(nameof(plate))).Outline);

        /// <summary>
        /// The plate's extent along a GIVEN world direction.
        ///
        /// It exists for the stop extension, which is the plate's height along the arm's own UP — not the
        /// second-largest of its world spans, and not its in-plane height when the plate's own edge order
        /// happens to run the other way.
        /// </summary>
        public static double ExtentAlong(IReadOnlyList<Point3D> outline, Vector3D direction)
        {
            if (outline == null || outline.Count == 0)
            {
                throw new ArgumentException("El contorno esta vacio.", nameof(outline));
            }

            if (!direction.IsFinite || direction.Length <= PlanarTolerance)
            {
                throw new ArgumentException("La direccion no es utilizable.", nameof(direction));
            }

            var axis = direction.Normalized();
            var projections = outline.Select(p => (p - outline[0]).Dot(axis)).ToList();

            return projections.Max() - projections.Min();
        }
    }
}
