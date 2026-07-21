using System;
using System.Windows;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// The shared world→canvas projection for the preview panels. Every preview window today keeps its own
    /// <c>mapScale/mapOffsetX/mapBottomY</c> fields and a private <c>Map(x,y)</c>; this is the one place that math
    /// lives, so the drawing (already shared via <see cref="PreviewCanvasPainter"/>) and the projection stop
    /// diverging. World Y points up; canvas Y points down, so <see cref="Project"/> flips it. Pure and immutable:
    /// it only uses <see cref="Point"/>/<see cref="Size"/>/<see cref="Thickness"/> value types, so it is unit
    /// tested without a dispatcher.
    /// </summary>
    public readonly struct PreviewProjection
    {
        public PreviewProjection(double scale, double offsetX, double baselineY, double worldMinX, double worldMinY)
        {
            Scale = scale;
            OffsetX = offsetX;
            BaselineY = baselineY;
            WorldMinX = worldMinX;
            WorldMinY = worldMinY;
        }

        /// <summary>Canvas units per world unit (uniform on both axes so nothing is distorted).</summary>
        public double Scale { get; }

        /// <summary>Canvas X that <see cref="WorldMinX"/> maps to.</summary>
        public double OffsetX { get; }

        /// <summary>Canvas Y that <see cref="WorldMinY"/> maps to (the baseline; larger world Y is drawn above it).</summary>
        public double BaselineY { get; }

        public double WorldMinX { get; }

        public double WorldMinY { get; }

        /// <summary>True when the projection can draw (positive, finite scale).</summary>
        public bool IsDrawable => Scale > 0.0 && !double.IsNaN(Scale) && !double.IsInfinity(Scale);

        /// <summary>Projects a world point to canvas coordinates (Y flipped).</summary>
        public Point Project(double worldX, double worldY)
            => new Point(OffsetX + ((worldX - WorldMinX) * Scale), BaselineY - ((worldY - WorldMinY) * Scale));

        /// <summary>Projects a world point to canvas coordinates.</summary>
        public Point Project(Point world) => Project(world.X, world.Y);

        /// <summary>The inverse of <see cref="Project"/> for hit-testing (the dynamic front matrix needs it).
        /// Returns the world point unchanged through a round trip when <see cref="IsDrawable"/>.</summary>
        public Point Unproject(Point canvas)
        {
            if (!IsDrawable)
            {
                return new Point(WorldMinX, WorldMinY);
            }

            return new Point(
                WorldMinX + ((canvas.X - OffsetX) / Scale),
                WorldMinY + ((BaselineY - canvas.Y) / Scale));
        }

        /// <summary>
        /// Fits a world bounding box into a canvas, centered, with uniform scale and the given margins. Degenerate
        /// input (non-positive canvas or world size) yields a non-drawable projection centered on the canvas rather
        /// than throwing, so a preview with nothing to show simply stays blank.
        /// </summary>
        public static PreviewProjection Fit(Rect worldBounds, Size canvas, Thickness margins)
        {
            var usableWidth = canvas.Width - margins.Left - margins.Right;
            var usableHeight = canvas.Height - margins.Top - margins.Bottom;

            if (usableWidth <= 0.0 || usableHeight <= 0.0 || worldBounds.Width <= 0.0 || worldBounds.Height <= 0.0)
            {
                return new PreviewProjection(0.0, canvas.Width / 2.0, canvas.Height / 2.0, worldBounds.X, worldBounds.Y);
            }

            var scale = Math.Min(usableWidth / worldBounds.Width, usableHeight / worldBounds.Height);
            var drawnWidth = worldBounds.Width * scale;
            var drawnHeight = worldBounds.Height * scale;

            var offsetX = margins.Left + ((usableWidth - drawnWidth) / 2.0);
            var baselineY = margins.Top + ((usableHeight + drawnHeight) / 2.0);

            return new PreviewProjection(scale, offsetX, baselineY, worldBounds.X, worldBounds.Y);
        }

        /// <summary>Convenience overload for a world box given by explicit min/size and uniform margin.</summary>
        public static PreviewProjection Fit(double worldMinX, double worldMinY, double worldWidth, double worldHeight, Size canvas, double margin)
            => Fit(new Rect(worldMinX, worldMinY, worldWidth, worldHeight), canvas, new Thickness(margin));
    }
}
