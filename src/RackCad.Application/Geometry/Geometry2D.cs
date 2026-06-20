using System;

namespace RackCad.Application.Geometry
{
    /// <summary>A point in the abstract model plane (inches). Not tied to AutoCAD.</summary>
    public readonly struct Point2D
    {
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public bool ApproxEquals(Point2D other, double tolerance)
        {
            return Math.Abs(X - other.X) <= tolerance && Math.Abs(Y - other.Y) <= tolerance;
        }

        public override string ToString()
        {
            return "(" + X.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", " +
                   Y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")";
        }
    }

    /// <summary>
    /// Where a piece's local origin sits in model space. A piece defines its connection points
    /// in its own local frame; a placement maps those local points to world coordinates.
    /// </summary>
    public readonly struct Placement2D
    {
        public Placement2D(double offsetX, double offsetY)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        public double OffsetX { get; }
        public double OffsetY { get; }

        public Point2D Apply(Point2D local)
        {
            return new Point2D(OffsetX + local.X, OffsetY + local.Y);
        }
    }
}
