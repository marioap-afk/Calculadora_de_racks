using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;

namespace RackCad.Application.Layout
{
    /// <summary>
    /// Pure 2D polygon tests for an irregular warehouse envelope (an L-shaped nave, notches): point-in-polygon
    /// (ray casting; the boundary counts as inside, so a rack flush against a wall is legal) and axis-aligned
    /// rectangle-inside-polygon (all four corners inside AND no polygon edge crossing the rectangle's interior —
    /// the edge test catches a notch stabbing into the rect even when every corner is inside). Simple polygons,
    /// any winding order, no holes (columns are separate obstacles).
    /// </summary>
    public static class PolygonGeometry
    {
        private const double Eps = 1e-6;

        public static bool Contains(IReadOnlyList<Point2D> polygon, double x, double y)
        {
            if (polygon == null || polygon.Count < 3)
            {
                return false;
            }

            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var xi = polygon[i].X;
                var yi = polygon[i].Y;
                var xj = polygon[j].X;
                var yj = polygon[j].Y;

                if (OnSegment(xj, yj, xi, yi, x, y))
                {
                    return true; // on the boundary counts as inside
                }

                if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public static bool ContainsRect(IReadOnlyList<Point2D> polygon, double x0, double y0, double x1, double y1)
        {
            if (polygon == null || polygon.Count < 3)
            {
                return false;
            }

            if (!Contains(polygon, x0, y0) || !Contains(polygon, x1, y0) ||
                !Contains(polygon, x1, y1) || !Contains(polygon, x0, y1))
            {
                return false;
            }

            // No polygon edge may cross the rect's INTERIOR (shrunk by Eps so a wall flush against the rect is legal).
            var ix0 = x0 + Eps;
            var iy0 = y0 + Eps;
            var ix1 = x1 - Eps;
            var iy1 = y1 - Eps;
            if (ix0 >= ix1 || iy0 >= iy1)
            {
                return true; // degenerate-thin rect: the corner tests suffice
            }

            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (SegmentIntersectsRect(polygon[j].X, polygon[j].Y, polygon[i].X, polygon[i].Y, ix0, iy0, ix1, iy1))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>True when (px,py) lies on segment a→b (distance to the line ≤ Eps and within the span).</summary>
        private static bool OnSegment(double ax, double ay, double bx, double by, double px, double py)
        {
            var abx = bx - ax;
            var aby = by - ay;
            var apx = px - ax;
            var apy = py - ay;

            var lengthSquared = abx * abx + aby * aby;
            if (lengthSquared <= Eps * Eps)
            {
                return Math.Abs(apx) <= Eps && Math.Abs(apy) <= Eps; // degenerate (repeated vertex)
            }

            var cross = abx * apy - aby * apx;
            if (cross * cross > Eps * Eps * lengthSquared)
            {
                return false; // farther than Eps from the segment's line
            }

            var dot = apx * abx + apy * aby;
            var slack = Eps * Math.Sqrt(lengthSquared);
            return dot >= -slack && dot <= lengthSquared + slack;
        }

        /// <summary>Liang-Barsky: does segment a→b have a positive-length portion inside the (open) rect?</summary>
        private static bool SegmentIntersectsRect(
            double ax, double ay, double bx, double by,
            double rx0, double ry0, double rx1, double ry1)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var t0 = 0.0;
            var t1 = 1.0;

            return Clip(-dx, ax - rx0, ref t0, ref t1)
                && Clip(dx, rx1 - ax, ref t0, ref t1)
                && Clip(-dy, ay - ry0, ref t0, ref t1)
                && Clip(dy, ry1 - ay, ref t0, ref t1)
                && t1 > t0; // strictly: a grazing touch is not an interior crossing
        }

        private static bool Clip(double p, double q, ref double t0, ref double t1)
        {
            if (p == 0.0)
            {
                return q >= 0.0; // parallel to this boundary: fully outside when q < 0
            }

            var t = q / p;
            if (p < 0.0)
            {
                if (t > t1) return false;
                if (t > t0) t0 = t;
            }
            else
            {
                if (t < t0) return false;
                if (t < t1) t1 = t;
            }

            return true;
        }
    }
}
