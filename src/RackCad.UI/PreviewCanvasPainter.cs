using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RackCad.UI
{
    /// <summary>
    /// The one place that turns already-projected canvas coordinates into WPF shapes on a preview Canvas.
    /// The dynamic/cama/selectivo windows each kept their own byte-identical copies of these primitives; now
    /// they hold a painter and delegate. World→canvas projection (Map) stays per-window — it uses each
    /// window's own scale/offset — so only the drawing is shared, not the projection.
    /// </summary>
    internal sealed class PreviewCanvasPainter
    {
        private readonly Canvas canvas;

        public PreviewCanvasPainter(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void AddLine(Point a, Point b, Brush stroke, double thickness, DoubleCollection dash = null)
        {
            canvas.Children.Add(new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeDashArray = dash,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            });
        }

        /// <summary>Returns the created Rectangle (or null for a degenerate size) so callers that later restyle a
        /// shape in place — e.g. the selectivo's post highlight — can keep a reference; most callers ignore it.</summary>
        public Rectangle AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, DoubleCollection dash = null, Brush fill = null)
        {
            if (width <= 0.0 || height <= 0.0)
            {
                return null;
            }

            var rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeDashArray = dash,
                Fill = fill ?? Brushes.Transparent
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            canvas.Children.Add(rectangle);
            return rectangle;
        }
    }
}
