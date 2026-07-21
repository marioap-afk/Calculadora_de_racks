using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// A preview <see cref="System.Windows.Controls.Canvas"/> that owns the shared world→canvas
    /// <see cref="PreviewProjection"/> and draws through the existing <see cref="PreviewCanvasPainter"/>, so callers
    /// work in world coordinates and the projection stops being re-derived per window. Combined with
    /// <see cref="PreviewPalette"/> it gives every preview the same projection AND the same colors. Being a
    /// <see cref="System.Windows.Controls.Canvas"/>, adopters can still add raw children when they need to.
    /// </summary>
    public class PreviewCanvas : System.Windows.Controls.Canvas
    {
        private readonly PreviewCanvasPainter painter;

        public PreviewCanvas()
        {
            painter = new PreviewCanvasPainter(this);
        }

        /// <summary>The active projection. Defaults to a non-drawable identity until <see cref="FitToWorld(Rect,Size)"/>
        /// (or <see cref="Projection"/>'s setter) supplies one.</summary>
        public PreviewProjection Projection { get; set; }

        /// <summary>Blank margin (canvas units) kept around the fitted world content. Adopters tune it per window.</summary>
        public Thickness ContentMargins { get; set; } = new Thickness(24);

        /// <summary>Clears every drawn shape.</summary>
        public void Clear() => Children.Clear();

        /// <summary>Recomputes and stores the projection that fits <paramref name="worldBounds"/> into an explicit
        /// canvas size (used before a layout pass, e.g. in tests). Returns the new projection.</summary>
        public PreviewProjection FitToWorld(Rect worldBounds, Size canvasSize)
        {
            Projection = PreviewProjection.Fit(worldBounds, canvasSize, ContentMargins);
            return Projection;
        }

        /// <summary>Recomputes the projection using the canvas's current rendered size (runtime, after layout).</summary>
        public PreviewProjection FitToWorld(Rect worldBounds)
            => FitToWorld(worldBounds, new Size(ActualWidth, ActualHeight));

        /// <summary>Draws a line given in world coordinates (projected, then delegated to the shared painter).</summary>
        public void DrawWorldLine(Point worldA, Point worldB, Brush stroke, double thickness, DoubleCollection dash = null)
            => painter.AddLine(Projection.Project(worldA), Projection.Project(worldB), stroke, thickness, dash);

        /// <summary>Draws an axis-aligned rectangle whose bottom-left corner is <paramref name="worldBottomLeft"/> and
        /// whose size is in world units. The Y flip is handled here (world up → canvas down). Returns the created
        /// rectangle, or null for a degenerate size.</summary>
        public Rectangle DrawWorldRectangle(Point worldBottomLeft, double worldWidth, double worldHeight, Brush stroke, double thickness, DoubleCollection dash = null, Brush fill = null)
        {
            var topLeft = Projection.Project(worldBottomLeft.X, worldBottomLeft.Y + worldHeight);
            return painter.AddRectangle(topLeft.X, topLeft.Y, worldWidth * Projection.Scale, worldHeight * Projection.Scale, stroke, thickness, dash, fill);
        }
    }
}
