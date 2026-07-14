using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;

namespace RackCad.Application.Layout
{
    /// <summary>
    /// A rectangular obstacle on the warehouse floor a rack must keep clear of — a column, a dock, a no-go zone. Its
    /// footprint is the rectangle [X, X+Width] × [Y, Y+Depth]; a rack must stay <see cref="Clearance"/> away from it.
    /// Pure data (same coordinate frame as a <see cref="WarehouseGridPlan"/>). No AutoCAD.
    /// </summary>
    public sealed class SiteObstacle
    {
        public SiteObstacle(double x, double y, double width, double depth, double clearance = 0.0, string label = null)
        {
            X = x;
            Y = y;
            Width = width;
            Depth = depth;
            Clearance = clearance;
            Label = label;
        }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Depth { get; }

        /// <summary>Minimum gap a rack must keep from this obstacle (added around its rectangle).</summary>
        public double Clearance { get; }

        /// <summary>Human-readable name for messages (e.g. "Columna A3"); optional.</summary>
        public string Label { get; }
    }

    /// <summary>
    /// The warehouse floor as pure data: a rectangular usable envelope [OriginX, OriginX+Width] × [OriginY, OriginY+Depth],
    /// a required gap from the walls (<see cref="WallClearance"/>), an optional minimum aisle width, and the obstacles
    /// (columns, docks…) racks must avoid. Same coordinate frame as a <see cref="WarehouseGridPlan"/>; feeds
    /// <see cref="WarehouseFitChecker"/> to answer "does this layout physically fit?". No AutoCAD, no scoring.
    /// </summary>
    public sealed class WarehouseSite
    {
        public WarehouseSite(
            double width, double depth,
            double originX = 0.0, double originY = 0.0,
            double wallClearance = 0.0, double minAisle = 0.0,
            IReadOnlyList<SiteObstacle> obstacles = null,
            IReadOnlyList<Point2D> boundary = null)
        {
            Width = width;
            Depth = depth;
            OriginX = originX;
            OriginY = originY;
            WallClearance = wallClearance;
            MinAisle = minAisle;
            Obstacles = obstacles ?? Array.Empty<SiteObstacle>();
            Boundary = boundary;
        }

        /// <summary>Build a site from an IRREGULAR envelope polygon (an L-shaped nave, notches): the rectangular
        /// Origin/Width/Depth become the polygon's bounding box (used to seed the fill grid) and the polygon itself
        /// is kept for the exact inside test. Vertices in any winding order; at least 3.</summary>
        public static WarehouseSite FromBoundary(
            IReadOnlyList<Point2D> boundary,
            double wallClearance = 0.0, double minAisle = 0.0,
            IReadOnlyList<SiteObstacle> obstacles = null)
        {
            if (boundary == null || boundary.Count < 3)
            {
                throw new ArgumentException("El contorno debe tener al menos 3 vértices.", nameof(boundary));
            }

            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            foreach (var point in boundary)
            {
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            }

            return new WarehouseSite(maxX - minX, maxY - minY, minX, minY, wallClearance, minAisle, obstacles, boundary);
        }

        /// <summary>Usable envelope extent along X (the rows/depth axis).</summary>
        public double Width { get; }

        /// <summary>Usable envelope extent along Y (the columns/run axis).</summary>
        public double Depth { get; }

        public double OriginX { get; }
        public double OriginY { get; }

        /// <summary>Minimum gap every rack must keep from the envelope boundary (0 = racks may touch the walls).</summary>
        public double WallClearance { get; }

        /// <summary>Minimum aisle width between adjacent racks; 0 = don't check aisles here.</summary>
        public double MinAisle { get; }

        public IReadOnlyList<SiteObstacle> Obstacles { get; }

        /// <summary>The exact envelope polygon when the site is irregular (null = the rectangular envelope). When set,
        /// Origin/Width/Depth hold its bounding box and the inside test uses the polygon.</summary>
        public IReadOnlyList<Point2D> Boundary { get; }
    }
}
