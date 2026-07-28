using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Geometry
{
    /// <summary>Which way a closed contour is traversed.</summary>
    public enum ContourOrientation
    {
        /// <summary>Counter-clockwise. Positive signed area. The convention for an OUTER boundary.</summary>
        CounterClockwise = 0,

        /// <summary>Clockwise. Negative signed area. The convention for a HOLE.</summary>
        Clockwise = 1
    }

    /// <summary>
    /// A closed, continuous chain of lines and arcs.
    ///
    /// Everything that could be wrong is rejected at CONSTRUCTION — a gap between consecutive segments, a
    /// contour that does not come back to its start, a non-finite coordinate, a degenerate segment — because
    /// a broken contour that survives construction surfaces much later as a drawing that looks almost right.
    ///
    /// Orientation is not stored as a flag: it is DERIVED from the signed area, so it cannot disagree with the
    /// geometry. The convention is the usual one: outer boundaries counter-clockwise, holes clockwise, which
    /// makes the area of a section with holes a plain sum.
    /// </summary>
    public sealed class ClosedContour2D
    {
        private readonly PathSegment2D[] _segments;

        private ClosedContour2D(PathSegment2D[] segments, double signedArea, Point2D centroid, Bounds2D bounds)
        {
            _segments = segments;
            SignedArea = signedArea;
            Centroid = centroid;
            Bounds = bounds;
        }

        public IReadOnlyList<PathSegment2D> Segments => _segments;

        /// <summary>Positive when counter-clockwise, negative when clockwise. Exact for lines and arcs.</summary>
        public double SignedArea { get; }

        public double Area => Math.Abs(SignedArea);

        public ContourOrientation Orientation =>
            SignedArea >= 0.0 ? ContourOrientation.CounterClockwise : ContourOrientation.Clockwise;

        /// <summary>Area centroid of the region the contour encloses. Exact for lines and arcs.</summary>
        public Point2D Centroid { get; }

        public Bounds2D Bounds { get; }

        public Point2D Start => _segments[0].Start;

        public static ClosedContour2D Create(IEnumerable<PathSegment2D> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            var list = segments.ToArray();

            if (list.Length < 2)
            {
                throw new ArgumentException(
                    "Un contorno cerrado necesita al menos dos segmentos; se recibieron " + list.Length + ".",
                    nameof(segments));
            }

            for (var i = 0; i < list.Length; i++)
            {
                var next = list[(i + 1) % list.Length];

                if (!list[i].End.ApproxEquals(next.Start, GeometryTolerance.Continuity))
                {
                    throw new ArgumentException(
                        "El contorno no es continuo: el segmento " + i + " termina en " + list[i].End +
                        " y el siguiente empieza en " + next.Start + ".",
                        nameof(segments));
                }
            }

            ComputeAreaAndCentroid(list, out var signedArea, out var centroid);

            if (GeometryTolerance.IsZero(signedArea))
            {
                throw new ArgumentException("El contorno encierra area nula.", nameof(segments));
            }

            return new ClosedContour2D(list, signedArea, centroid, ComputeBounds(list));
        }

        /// <summary>Builds a contour from straight segments between consecutive points, closing the loop.</summary>
        public static ClosedContour2D FromPolygon(IReadOnlyList<Point2D> vertices)
        {
            if (vertices == null)
            {
                throw new ArgumentNullException(nameof(vertices));
            }

            if (vertices.Count < 3)
            {
                throw new ArgumentException("Un poligono necesita al menos tres vertices.", nameof(vertices));
            }

            var segments = new List<PathSegment2D>(vertices.Count);

            for (var i = 0; i < vertices.Count; i++)
            {
                segments.Add(PathSegment2D.Line(vertices[i], vertices[(i + 1) % vertices.Count]));
            }

            return Create(segments);
        }

        /// <summary>The same contour traversed the other way. Flips <see cref="Orientation"/>.</summary>
        public ClosedContour2D Reversed() =>
            Create(_segments.Reverse().Select(segment => segment.Reversed()));

        /// <summary>Forces a given orientation, reversing only when needed.</summary>
        public ClosedContour2D WithOrientation(ContourOrientation orientation) =>
            Orientation == orientation ? this : Reversed();

        /// <summary>
        /// The contour under an affine transformation. A mirroring transformation flips the winding, so the
        /// result is reversed to keep the caller's orientation contract — otherwise every mirrored section
        /// would silently turn its outer boundary into a hole.
        /// </summary>
        public ClosedContour2D Transformed(Transform2D transform)
        {
            var moved = Create(_segments.Select(segment => segment.Transformed(transform)));
            return transform.ReversesOrientation ? moved.Reversed() : moved;
        }

        /// <summary>The contour as a point sequence, closed, with no duplicated vertices.</summary>
        public IReadOnlyList<Point2D> Flatten(double chordTolerance)
        {
            var points = new List<Point2D> { _segments[0].Start };

            foreach (var segment in _segments)
            {
                foreach (var point in segment.FlattenAfterStart(chordTolerance))
                {
                    points.Add(point);
                }
            }

            // The last point is the first one again: drop it so the polyline is stored closed, not duplicated.
            if (points.Count > 1 && points[points.Count - 1].ApproxEquals(points[0], GeometryTolerance.Continuity))
            {
                points.RemoveAt(points.Count - 1);
            }

            return points;
        }

        /// <summary>
        /// Signed area and area centroid, EXACTLY, for a boundary of lines and arcs.
        ///
        /// The chord polygon through the segment endpoints gives the bulk; each arc then contributes its
        /// circular segment — the sliver between the chord and the arc — with area r²/2·(Δθ − sin Δθ), signed
        /// by the sweep. The centroid of that sliver sits on the bisector at 4r·sin³(Δθ/2) / (3(Δθ − sin Δθ))
        /// from the centre. Tessellating instead would have been shorter and would have made the area depend
        /// on a tolerance, which is exactly what the area regression must not do.
        /// </summary>
        private static void ComputeAreaAndCentroid(
            IReadOnlyList<PathSegment2D> segments, out double signedArea, out Point2D centroid)
        {
            double polygonArea2 = 0.0;   // twice the polygon's signed area
            double momentX = 0.0;        // ∫x dA · 6  for the polygon part
            double momentY = 0.0;

            foreach (var segment in segments)
            {
                var a = segment.Start;
                var b = segment.End;
                var cross = (a.X * b.Y) - (b.X * a.Y);
                polygonArea2 += cross;
                momentX += (a.X + b.X) * cross;
                momentY += (a.Y + b.Y) * cross;
            }

            var area = polygonArea2 / 2.0;
            var cx = momentX / 6.0;      // still to be divided by the total area
            var cy = momentY / 6.0;

            foreach (var segment in segments)
            {
                if (!segment.IsArc)
                {
                    continue;
                }

                var sweep = segment.SweepAngle;
                var absSweep = Math.Abs(sweep);
                var r = segment.Radius;

                // Area of the circular segment, signed like the sweep.
                var sliver = (r * r / 2.0) * (absSweep - Math.Sin(absSweep));
                var signedSliver = sweep >= 0.0 ? sliver : -sliver;
                area += signedSliver;

                if (GeometryTolerance.IsZero(sliver))
                {
                    continue;
                }

                // Centroid of the sliver: on the bisector of the sweep, measured from the arc centre.
                var half = absSweep / 2.0;
                var sinHalf = Math.Sin(half);
                var distance = (4.0 * r * sinHalf * sinHalf * sinHalf) / (3.0 * (absSweep - Math.Sin(absSweep)));
                var bisector = segment.StartAngle + (sweep / 2.0);
                var px = segment.Center.X + (distance * Math.Cos(bisector));
                var py = segment.Center.Y + (distance * Math.Sin(bisector));

                cx += signedSliver * px;
                cy += signedSliver * py;
            }

            signedArea = area;
            centroid = GeometryTolerance.IsZero(area)
                ? new Point2D(0.0, 0.0)
                : new Point2D(cx / area, cy / area);
        }

        /// <summary>
        /// Bounds that account for the BULGE of arcs, not just their endpoints.
        ///
        /// An arc crossing an axis direction reaches further than either endpoint; using only endpoints would
        /// under-report the box of every rounded corner, and the preview would clip the drawing.
        /// </summary>
        private static Bounds2D ComputeBounds(IReadOnlyList<PathSegment2D> segments)
        {
            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;

            void Include(Point2D point)
            {
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            }

            foreach (var segment in segments)
            {
                Include(segment.Start);
                Include(segment.End);

                if (!segment.IsArc)
                {
                    continue;
                }

                foreach (var extreme in ArcAxisExtremes(segment))
                {
                    Include(extreme);
                }
            }

            return new Bounds2D(minX, minY, maxX, maxY);
        }

        /// <summary>The cardinal points (0°, 90°, 180°, 270°) the arc actually passes through.</summary>
        private static IEnumerable<Point2D> ArcAxisExtremes(PathSegment2D arc)
        {
            for (var quadrant = 0; quadrant < 4; quadrant++)
            {
                var angle = quadrant * Math.PI / 2.0;

                if (ArcContainsAngle(arc, angle))
                {
                    yield return PathSegment2D.PointOnCircle(arc.Center, arc.Radius, angle);
                }
            }
        }

        private static bool ArcContainsAngle(PathSegment2D arc, double angle)
        {
            // Measure the target relative to the start, in the direction of travel, and see whether it lands
            // before the end. Working modulo 2π keeps this correct for arcs that cross the 0° seam.
            var sweep = arc.SweepAngle;
            var delta = sweep >= 0.0 ? angle - arc.StartAngle : arc.StartAngle - angle;
            delta -= Math.Floor(delta / (2.0 * Math.PI)) * 2.0 * Math.PI;

            return delta <= Math.Abs(sweep) + GeometryTolerance.Angle;
        }
    }
}
