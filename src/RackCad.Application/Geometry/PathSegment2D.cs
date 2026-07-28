using System;
using System.Collections.Generic;
using System.Globalization;

namespace RackCad.Application.Geometry
{
    /// <summary>What a <see cref="PathSegment2D"/> actually is.</summary>
    public enum PathSegmentKind
    {
        Line = 0,
        Arc = 1
    }

    /// <summary>
    /// One piece of a path: either a straight segment or a circular arc.
    ///
    /// It is a single struct with a discriminator instead of a base class with two subclasses. The reason is
    /// concrete, not stylistic: a section contour is a short ordered list that gets copied, transformed and
    /// compared constantly, and a polymorphic hierarchy would put every one of those on the heap and make
    /// value equality — which the deterministic plan signature depends on — something you have to remember to
    /// implement. Two kinds is also the whole universe here: nothing in AISC needs a spline.
    ///
    /// An arc is stored by centre, radius, start angle and SIGNED sweep. Positive sweep is counter-clockwise.
    /// Endpoints are derived, never stored, so they cannot drift out of agreement with the angles.
    /// </summary>
    public readonly struct PathSegment2D
    {
        private PathSegment2D(
            PathSegmentKind kind, Point2D start, Point2D end,
            Point2D center, double radius, double startAngle, double sweepAngle)
        {
            Kind = kind;
            Start = start;
            End = end;
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }

        public PathSegmentKind Kind { get; }

        public Point2D Start { get; }

        public Point2D End { get; }

        /// <summary>Centre of the arc. Meaningless for a line.</summary>
        public Point2D Center { get; }

        /// <summary>Radius of the arc, strictly positive. Zero for a line.</summary>
        public double Radius { get; }

        /// <summary>Angle of <see cref="Start"/> about <see cref="Center"/>, radians. Zero for a line.</summary>
        public double StartAngle { get; }

        /// <summary>Signed sweep in radians; positive is counter-clockwise. Zero for a line.</summary>
        public double SweepAngle { get; }

        public bool IsLine => Kind == PathSegmentKind.Line;

        public bool IsArc => Kind == PathSegmentKind.Arc;

        public double EndAngle => StartAngle + SweepAngle;

        public static PathSegment2D Line(Point2D start, Point2D end)
        {
            RequireFinite(start, nameof(start));
            RequireFinite(end, nameof(end));

            if (Math.Abs(end.X - start.X) <= GeometryTolerance.Length &&
                Math.Abs(end.Y - start.Y) <= GeometryTolerance.Length)
            {
                throw new ArgumentException(
                    "Un segmento de longitud cero no es geometria: " + start + " -> " + end + ".");
            }

            return new PathSegment2D(PathSegmentKind.Line, start, end, default, 0.0, 0.0, 0.0);
        }

        /// <summary>
        /// An arc from its centre, radius, start angle and signed sweep. This is the only way to build one:
        /// three-point or endpoint-plus-bulge constructions look friendlier and are ambiguous about direction,
        /// which is precisely the thing a contour cannot afford to get wrong.
        /// </summary>
        public static PathSegment2D Arc(Point2D center, double radius, double startAngle, double sweepAngle)
        {
            RequireFinite(center, nameof(center));
            GeometryTolerance.RequirePositive(radius, nameof(radius));
            GeometryTolerance.RequireFinite(startAngle, nameof(startAngle));
            GeometryTolerance.RequireFinite(sweepAngle, nameof(sweepAngle));

            if (Math.Abs(sweepAngle) <= GeometryTolerance.Angle)
            {
                throw new ArgumentException("Un arco de barrido nulo no es geometria.", nameof(sweepAngle));
            }

            if (Math.Abs(sweepAngle) > (2.0 * Math.PI) + GeometryTolerance.Angle)
            {
                throw new ArgumentException(
                    "Un arco no puede barrer mas de una vuelta completa.", nameof(sweepAngle));
            }

            return new PathSegment2D(
                PathSegmentKind.Arc,
                PointOnCircle(center, radius, startAngle),
                PointOnCircle(center, radius, startAngle + sweepAngle),
                center, radius, startAngle, sweepAngle);
        }

        /// <summary>A quarter-turn arc, which is every fillet and every rounded corner in these four families.</summary>
        public static PathSegment2D QuarterArc(Point2D center, double radius, double startAngle, bool counterClockwise) =>
            Arc(center, radius, startAngle, counterClockwise ? Math.PI / 2.0 : -Math.PI / 2.0);

        public static Point2D PointOnCircle(Point2D center, double radius, double angle) =>
            new Point2D(center.X + (radius * Math.Cos(angle)), center.Y + (radius * Math.Sin(angle)));

        /// <summary>Arc length for an arc; straight distance for a line.</summary>
        public double Length =>
            IsLine
                ? Math.Sqrt(((End.X - Start.X) * (End.X - Start.X)) + ((End.Y - Start.Y) * (End.Y - Start.Y)))
                : Radius * Math.Abs(SweepAngle);

        /// <summary>The same segment traversed backwards. An arc keeps its centre and flips its sweep.</summary>
        public PathSegment2D Reversed() =>
            IsLine
                ? Line(End, Start)
                : Arc(Center, Radius, StartAngle + SweepAngle, -SweepAngle);

        /// <summary>
        /// The segment under an affine transformation. A uniform-scale-plus-rotation-plus-mirror keeps an arc
        /// circular, which is why <see cref="Transform2D"/> refuses non-uniform scaling: the alternative would
        /// be silently returning an ellipse pretending to be an arc.
        /// </summary>
        public PathSegment2D Transformed(Transform2D transform)
        {
            if (IsLine)
            {
                return Line(transform.Apply(Start), transform.Apply(End));
            }

            var center = transform.Apply(Center);
            var radius = Radius * transform.ScaleFactor;
            var rotation = transform.RotationAngle();

            if (!transform.ReversesOrientation)
            {
                return Arc(center, radius, StartAngle + rotation, SweepAngle);
            }

            // A mirror reflects the start angle about the transformed X direction and flips the sweep. Taking
            // the image of the start point and re-deriving the angle keeps this exact even for oblique mirrors.
            var start = transform.Apply(Start);
            var startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            return Arc(center, radius, startAngle, -SweepAngle);
        }

        /// <summary>
        /// Every point of the segment approximated within <paramref name="chordTolerance"/> inches, INCLUDING
        /// both endpoints and excluding the start (so consecutive segments chain without duplicates).
        ///
        /// A line yields just its end. An arc is split into equal steps chosen so the sagitta stays under the
        /// tolerance, which makes the result deterministic and monotone: a tighter tolerance never produces a
        /// coarser result.
        /// </summary>
        public IEnumerable<Point2D> FlattenAfterStart(double chordTolerance)
        {
            GeometryTolerance.RequirePositive(chordTolerance, nameof(chordTolerance));

            if (IsLine)
            {
                yield return End;
                yield break;
            }

            var steps = ArcStepCount(Radius, SweepAngle, chordTolerance);
            var step = SweepAngle / steps;

            for (var i = 1; i <= steps; i++)
            {
                // The last point is the stored endpoint, not a recomputed one: flattening must never move an
                // endpoint, or a closed contour would stop closing.
                yield return i == steps ? End : PointOnCircle(Center, Radius, StartAngle + (step * i));
            }
        }

        /// <summary>
        /// How many chords an arc needs so the sagitta stays within <paramref name="chordTolerance"/>.
        ///
        /// The sagitta of a chord spanning θ is r·(1 − cos(θ/2)); solving for θ gives the step. When the
        /// tolerance is larger than the radius the formula degenerates, so the count is clamped to a minimum
        /// of two — a single chord across an arc would collapse a fillet into a straight cut.
        /// </summary>
        public static int ArcStepCount(double radius, double sweepAngle, double chordTolerance)
        {
            GeometryTolerance.RequirePositive(radius, nameof(radius));
            GeometryTolerance.RequirePositive(chordTolerance, nameof(chordTolerance));

            var sweep = Math.Abs(sweepAngle);

            if (chordTolerance >= radius)
            {
                return Math.Max(2, (int)Math.Ceiling(sweep / (Math.PI / 2.0)));
            }

            var maxStep = 2.0 * Math.Acos(1.0 - (chordTolerance / radius));

            if (!GeometryTolerance.IsFinite(maxStep) || maxStep <= GeometryTolerance.Angle)
            {
                return 2;
            }

            return Math.Max(2, (int)Math.Ceiling(sweep / maxStep));
        }

        private static void RequireFinite(Point2D point, string name)
        {
            GeometryTolerance.RequireFinite(point.X, name + ".X");
            GeometryTolerance.RequireFinite(point.Y, name + ".Y");
        }

        public override string ToString() =>
            IsLine
                ? "Line " + Start + " -> " + End
                : "Arc r=" + Radius.ToString("0.####", CultureInfo.InvariantCulture) + " " + Start + " -> " + End;
    }
}
