using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// Removes the ink a projection draws twice, and reopens the closed curves it flattened.
    ///
    /// Orthographic projection is not injective, and two things happen when the camera lines up with the
    /// geometry. A closed contour seen exactly edge-on collapses onto a LINE, so emitting it closed makes
    /// every consumer walk each edge and then walk it back. And two generatrices that come from different
    /// vertices — the two ends of a flat wall, or the tangent points of the concentric arcs of a tube —
    /// project onto the same line, so the same segment is emitted more than once.
    ///
    /// Neither is visible in bounds, area or the plan signature, which is why the earlier suites could not
    /// see them: they are defects of WHERE THE INK GOES. In AutoCAD they show up as overlapping polylines
    /// and as zero-area closed polylines, both of which a person notices immediately when they try to select,
    /// trim or measure.
    ///
    /// The pass is purely 2-D and runs AFTER projection. It never re-derives a section dimension — it only
    /// looks at points that <see cref="StructuralSectionPlanBuilder"/> already produced (ADR-0022 §7).
    /// </summary>
    internal static class SectionProjectionCanonicalizer
    {
        /// <summary>
        /// Which role keeps the ink when two curves land on the same line.
        ///
        /// Outside beats inside, and the piece beats its own subdivisions. The order matters because a
        /// tube's bore projects INSIDE its outer end profile: the outer face is what a person is looking at,
        /// so it is the one that survives, and the bore stops being a duplicate line drawn on top of it.
        /// </summary>
        private static readonly SectionCurveRole[] Priority =
        {
            SectionCurveRole.OuterContour,
            SectionCurveRole.EndProfile,
            SectionCurveRole.Generatrix,
            SectionCurveRole.Hole,
            SectionCurveRole.EndProfileHole,
            SectionCurveRole.InteriorGeneratrix
        };

        /// <summary>Lines closer than this are the same line; ink closer than this is the same ink.</summary>
        private const double Tolerance = GeometryTolerance.Continuity;

        /// <summary>
        /// Canonicalizes the curves of one projection.
        ///
        /// Closed curves that still enclose area pass through untouched, in their original order. Everything
        /// else — open curves and collapsed closed ones — is reduced to the maximal set of non-overlapping
        /// segments that draws exactly the same picture.
        ///
        /// Annotations (axis, envelope) are deliberately left out: they are a separate layer in the drawing,
        /// and letting the piece eat an annotation — or the other way round — would be a surprise.
        /// </summary>
        public static IReadOnlyList<SectionPlanCurve> Canonicalize(IEnumerable<SectionPlanCurve> curves)
        {
            var result = new List<SectionPlanCurve>();
            var flat = new List<Edge>();

            foreach (var curve in curves)
            {
                if (IsAnnotation(curve.Role))
                {
                    result.Add(curve);
                    continue;
                }

                if (curve.IsClosed)
                {
                    // Already guarded by SectionPlanCurve, so a closed curve here always has area.
                    result.Add(curve);
                    continue;
                }

                flat.AddRange(Explode(curve));
            }

            result.AddRange(Rebuild(flat));
            return result;
        }

        /// <summary>
        /// Splits a closed contour that projected onto a line into the open segments that actually draw it.
        ///
        /// The caller cannot simply keep the point list and set <c>isClosed: false</c>: the traversal walks
        /// the extent and comes back, so half the points retrace the other half.
        /// </summary>
        public static IEnumerable<SectionPlanCurve> Flatten(SectionCurveRole role, IReadOnlyList<Point2D> points)
        {
            var edges = new List<Edge>();

            for (var i = 1; i < points.Count; i++)
            {
                edges.Add(new Edge(role, points[i - 1], points[i]));
            }

            if (points.Count > 2)
            {
                edges.Add(new Edge(role, points[points.Count - 1], points[0]));
            }

            return Rebuild(edges);
        }

        private static bool IsAnnotation(SectionCurveRole role) =>
            role == SectionCurveRole.Axis || role == SectionCurveRole.Envelope;

        private static IEnumerable<Edge> Explode(SectionPlanCurve curve)
        {
            for (var i = 1; i < curve.Points.Count; i++)
            {
                yield return new Edge(curve.Role, curve.Points[i - 1], curve.Points[i]);
            }
        }

        /// <summary>
        /// The core: group the segments by the infinite line they lie on, merge what overlaps, and let the
        /// higher-priority role keep the ink where two roles claim the same stretch.
        /// </summary>
        private static IReadOnlyList<SectionPlanCurve> Rebuild(IReadOnlyList<Edge> edges)
        {
            var usable = edges.Where(e => e.Length > Tolerance).ToArray();

            if (usable.Length == 0)
            {
                return new SectionPlanCurve[0];
            }

            var result = new List<SectionPlanCurve>();

            foreach (var line in GroupByLine(usable))
            {
                // Ink already claimed on this line by a higher-priority role. Intervals are in the line's own
                // 1-D parameter, so containment and overlap are plain number comparisons.
                var claimed = new List<Interval>();

                foreach (var role in Priority)
                {
                    var mine = line.Edges
                        .Where(e => e.Role == role)
                        .Select(e => line.Parametrize(e))
                        .ToList();

                    if (mine.Count == 0)
                    {
                        continue;
                    }

                    foreach (var run in Subtract(Merge(mine), claimed))
                    {
                        result.Add(new SectionPlanCurve(
                            role,
                            new[] { line.PointAt(run.Start), line.PointAt(run.End) },
                            isClosed: false));
                    }

                    claimed = Merge(claimed.Concat(mine).ToList());
                }
            }

            // Deterministic order: the plan signature encodes it, so it must not depend on dictionary
            // iteration or on which contour happened to be visited first.
            return result
                .OrderBy(c => (int)c.Role)
                .ThenBy(c => Round(c.Points[0].X))
                .ThenBy(c => Round(c.Points[0].Y))
                .ThenBy(c => Round(c.Points[1].X))
                .ThenBy(c => Round(c.Points[1].Y))
                .ToArray();
        }

        private static double Round(double value) => Math.Round(value, 9, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Groups segments by their supporting line.
        ///
        /// Grouping is done by sorting on the line's canonical (normal, offset) and then walking runs that
        /// stay within tolerance, rather than by rounding to a key. Two segments that SHOULD share a line
        /// often reach it by different arithmetic — the tangent points of a tube's concentric arcs are
        /// <c>2 − 0.35</c> and <c>1.75 − 0.10</c> — and agree only to the last bits; a rounded key can split
        /// them across a boundary, a sorted sweep cannot.
        /// </summary>
        private static IEnumerable<LineGroup> GroupByLine(IReadOnlyList<Edge> edges)
        {
            var lines = edges
                .Select(e => new { Edge = e, Line = Line.Through(e) })
                .OrderBy(x => x.Line.NormalX)
                .ThenBy(x => x.Line.NormalY)
                .ThenBy(x => x.Line.Offset)
                .ToArray();

            var group = new LineGroup(lines[0].Line);
            group.Edges.Add(lines[0].Edge);

            for (var i = 1; i < lines.Length; i++)
            {
                if (group.Line.IsSameLineAs(lines[i].Line))
                {
                    group.Edges.Add(lines[i].Edge);
                    continue;
                }

                yield return group;
                group = new LineGroup(lines[i].Line);
                group.Edges.Add(lines[i].Edge);
            }

            yield return group;
        }

        /// <summary>Union of intervals: overlapping or merely touching runs become one.</summary>
        private static List<Interval> Merge(List<Interval> intervals)
        {
            if (intervals.Count == 0)
            {
                return intervals;
            }

            var sorted = intervals.OrderBy(i => i.Start).ToList();
            var merged = new List<Interval> { sorted[0] };

            foreach (var current in sorted.Skip(1))
            {
                var last = merged[merged.Count - 1];

                if (current.Start <= last.End + Tolerance)
                {
                    merged[merged.Count - 1] = new Interval(last.Start, Math.Max(last.End, current.End));
                }
                else
                {
                    merged.Add(current);
                }
            }

            return merged;
        }

        /// <summary>What is left of <paramref name="runs"/> once the already-claimed ink is taken out.</summary>
        private static IEnumerable<Interval> Subtract(List<Interval> runs, List<Interval> claimed)
        {
            foreach (var run in runs)
            {
                var start = run.Start;

                foreach (var taken in claimed.Where(c => c.End > run.Start && c.Start < run.End)
                             .OrderBy(c => c.Start))
                {
                    if (taken.Start - start > Tolerance)
                    {
                        yield return new Interval(start, taken.Start);
                    }

                    start = Math.Max(start, taken.End);
                }

                if (run.End - start > Tolerance)
                {
                    yield return new Interval(start, run.End);
                }
            }
        }

        private readonly struct Edge
        {
            public Edge(SectionCurveRole role, Point2D from, Point2D to)
            {
                Role = role;
                From = from;
                To = to;
            }

            public SectionCurveRole Role { get; }

            public Point2D From { get; }

            public Point2D To { get; }

            public double Length => Vector2D.Between(From, To).Length;
        }

        /// <summary>An infinite line in canonical form: a unit normal with a fixed sign, plus its offset.</summary>
        private readonly struct Line
        {
            private Line(double normalX, double normalY, double offset, Point2D origin, Vector2D direction)
            {
                NormalX = normalX;
                NormalY = normalY;
                Offset = offset;
                Origin = origin;
                Direction = direction;
            }

            public double NormalX { get; }

            public double NormalY { get; }

            public double Offset { get; }

            public Point2D Origin { get; }

            public Vector2D Direction { get; }

            public static Line Through(Edge edge)
            {
                var direction = Vector2D.Between(edge.From, edge.To).Normalized();

                // Both senses of a segment must give the SAME line, so the direction gets a canonical sign.
                if (direction.X < -Tolerance ||
                    (Math.Abs(direction.X) <= Tolerance && direction.Y < 0.0))
                {
                    direction = -direction;
                }

                var normal = direction.Perpendicular();
                var offset = (edge.From.X * normal.X) + (edge.From.Y * normal.Y);

                return new Line(normal.X, normal.Y, offset, edge.From, direction);
            }

            public bool IsSameLineAs(Line other) =>
                Math.Abs(NormalX - other.NormalX) <= Tolerance &&
                Math.Abs(NormalY - other.NormalY) <= Tolerance &&
                Math.Abs(Offset - other.Offset) <= Tolerance;

            public double Parameter(Point2D point) =>
                ((point.X - Origin.X) * Direction.X) + ((point.Y - Origin.Y) * Direction.Y);

            public Point2D At(double parameter) =>
                new Point2D(
                    Origin.X + (Direction.X * parameter),
                    Origin.Y + (Direction.Y * parameter));
        }

        private sealed class LineGroup
        {
            public LineGroup(Line line)
            {
                Line = line;
                Edges = new List<Edge>();
            }

            public Line Line { get; }

            public List<Edge> Edges { get; }

            /// <summary>A segment as an interval of the group's line, always with start &lt;= end.</summary>
            public Interval Parametrize(Edge edge)
            {
                var a = Line.Parameter(edge.From);
                var b = Line.Parameter(edge.To);
                return a <= b ? new Interval(a, b) : new Interval(b, a);
            }

            public Point2D PointAt(double parameter) => Line.At(parameter);
        }

        private readonly struct Interval
        {
            public Interval(double start, double end)
            {
                Start = start;
                End = end;
            }

            public double Start { get; }

            public double End { get; }
        }
    }
}
