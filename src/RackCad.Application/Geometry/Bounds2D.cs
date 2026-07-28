using System;
using System.Collections.Generic;
using System.Globalization;

namespace RackCad.Application.Geometry
{
    /// <summary>
    /// An axis-aligned bounding box in the model plane (inches).
    ///
    /// It has no "empty" state on purpose: a box that might be empty forces every consumer to check, and the
    /// checks get forgotten. Callers build one from actual points through <see cref="FromPoints"/>, which
    /// fails loudly on an empty input rather than returning something that draws as a dot at the origin.
    /// </summary>
    public readonly struct Bounds2D
    {
        public Bounds2D(double minX, double minY, double maxX, double maxY)
        {
            if (!GeometryTolerance.IsFinite(minX) || !GeometryTolerance.IsFinite(minY) ||
                !GeometryTolerance.IsFinite(maxX) || !GeometryTolerance.IsFinite(maxY))
            {
                throw new ArgumentException("Los limites deben ser finitos.");
            }

            if (maxX < minX || maxY < minY)
            {
                throw new ArgumentException("Los limites estan invertidos: el maximo es menor que el minimo.");
            }

            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public double MinX { get; }

        public double MinY { get; }

        public double MaxX { get; }

        public double MaxY { get; }

        public double Width => MaxX - MinX;

        public double Height => MaxY - MinY;

        public Point2D Center => new Point2D((MinX + MaxX) / 2.0, (MinY + MaxY) / 2.0);

        public Point2D Min => new Point2D(MinX, MinY);

        public Point2D Max => new Point2D(MaxX, MaxY);

        /// <summary>True when the box has a real extent on both axes.</summary>
        public bool HasArea => Width > GeometryTolerance.Length && Height > GeometryTolerance.Length;

        public static Bounds2D FromPoints(IEnumerable<Point2D> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;
            var any = false;

            foreach (var point in points)
            {
                any = true;
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            }

            if (!any)
            {
                throw new ArgumentException("No se pueden calcular limites de un conjunto vacio.", nameof(points));
            }

            return new Bounds2D(minX, minY, maxX, maxY);
        }

        public Bounds2D Union(Bounds2D other) =>
            new Bounds2D(
                Math.Min(MinX, other.MinX),
                Math.Min(MinY, other.MinY),
                Math.Max(MaxX, other.MaxX),
                Math.Max(MaxY, other.MaxY));

        /// <summary>Grows the box by <paramref name="margin"/> on every side. A negative margin is rejected.</summary>
        public Bounds2D Inflate(double margin)
        {
            GeometryTolerance.RequireFinite(margin, nameof(margin));

            if (margin < 0.0)
            {
                throw new ArgumentException("El margen no puede ser negativo.", nameof(margin));
            }

            return new Bounds2D(MinX - margin, MinY - margin, MaxX + margin, MaxY + margin);
        }

        public bool Contains(Point2D point, double tolerance = GeometryTolerance.Length) =>
            point.X >= MinX - tolerance && point.X <= MaxX + tolerance &&
            point.Y >= MinY - tolerance && point.Y <= MaxY + tolerance;

        public bool ApproxEquals(Bounds2D other, double tolerance) =>
            GeometryTolerance.AreClose(MinX, other.MinX, tolerance) &&
            GeometryTolerance.AreClose(MinY, other.MinY, tolerance) &&
            GeometryTolerance.AreClose(MaxX, other.MaxX, tolerance) &&
            GeometryTolerance.AreClose(MaxY, other.MaxY, tolerance);

        public override string ToString() =>
            "[" + MinX.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            MinY.ToString("0.####", CultureInfo.InvariantCulture) + " .. " +
            MaxX.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            MaxY.ToString("0.####", CultureInfo.InvariantCulture) + "]";
    }
}
