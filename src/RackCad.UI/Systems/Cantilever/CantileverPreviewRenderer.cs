using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Cantilever;

namespace RackCad.UI.Systems.Cantilever
{
    /// <summary>
    /// Draws a <see cref="CantileverViewPlan"/> on a WPF canvas.
    ///
    /// It renders the SAME plan object the Plugin materialises, so the picture in the editor and the entities in
    /// the drawing cannot disagree: there is one projection, computed in Application, and two adapters that turn
    /// its points into a <c>Polyline</c> — one WPF, one AutoCAD. It computes no geometry, asks no section for a
    /// dimension and rounds nothing except the fit-to-canvas transform, which is a viewing concern and never
    /// travels back into the model.
    /// </summary>
    internal static class CantileverPreviewRenderer
    {
        /// <summary>Padding, in device pixels, between the plan's extent and the canvas edge.</summary>
        private const double Margin = 14.0;

        /// <summary>The smallest a hole may be drawn in the PREVIEW, in device pixels. See <see cref="Hole"/>.</summary>
        private const double MinimumHolePixels = 3.0;

        /// <summary>
        /// One brush per visual ROLE, built from the colour Application declares for it.
        ///
        /// The palette is not chosen here. Application owns it, because the block in the drawing has to read the
        /// same as the picture the user approved — a green column that arrives yellow is the wrong picture — and
        /// the only way to guarantee that is one authority and two adapters. This adapter's whole job is turning
        /// an AutoCAD Color Index into a WPF brush.
        /// </summary>
        /// <summary>The colour of the «nothing to draw» message. Not a piece, so not a role.</summary>
        private static readonly Brush EmptyBrush = UiSupport.FrozenBrush(Color.FromRgb(0x8A, 0x97, 0xA4));

        private static readonly IReadOnlyDictionary<CantileverVisualRole, Brush> RoleBrushes =
            Enum.GetValues(typeof(CantileverVisualRole))
                .Cast<CantileverVisualRole>()
                .ToDictionary(
                    role => role,
                    role => UiSupport.FrozenBrush(AciPalette.ColorOf(CantileverVisualRoles.ColorIndexOf(role))));

        /// <summary>
        /// El color de un TIPO de pieza, para una leyenda que no tiene una curva delante.
        ///
        /// Una leyenda lista tipos, no curvas, así que aquí sí hay que deducir el rol. Lo que dibuja una curva
        /// usa el rol que la curva TRAE —ver la sobrecarga de abajo—, porque hay naturalezas que el tipo no
        /// distingue.
        /// </summary>
        internal static Brush BrushFor(CantileverViewPieceKind kind) =>
            RoleBrushes[CantileverVisualRoles.Of(kind)];

        /// <summary>The colour a role reads in, for a legend that lists natures rather than pieces.</summary>
        internal static Brush BrushFor(CantileverVisualRole role) => RoleBrushes[role];

        /// <summary>
        /// Clears <paramref name="canvas"/> and draws <paramref name="plan"/> scaled to fit, preserving aspect.
        ///
        /// An empty or blocked plan leaves a message rather than an empty box: "nothing is drawn" and "nothing
        /// could be drawn" look identical on a canvas and are not the same thing.
        /// </summary>
        internal static void Render(Canvas canvas, CantileverViewPlan plan, string emptyMessage)
        {
            if (canvas == null)
            {
                return;
            }

            canvas.Children.Clear();

            if (plan == null || plan.IsEmpty)
            {
                canvas.Children.Add(Message(emptyMessage ?? "Sin vista que mostrar."));
                return;
            }

            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;

            if (width <= 2.0 * Margin || height <= 2.0 * Margin)
            {
                return; // not laid out yet; the SizeChanged handler renders again once it is
            }

            var bounds = plan.Bounds;
            var scale = Scale(bounds, width, height);

            foreach (var curve in plan.Curves)
            {
                var shape = Build(curve, bounds, scale, width, height);

                if (shape != null)
                {
                    canvas.Children.Add(shape);
                }
            }
        }

        /// <summary>The uniform scale that fits the plan's extent inside the canvas, with a margin.</summary>
        private static double Scale(Bounds2D bounds, double width, double height)
        {
            var usableWidth = Math.Max(1.0, width - 2.0 * Margin);
            var usableHeight = Math.Max(1.0, height - 2.0 * Margin);

            // A degenerate extent — a line seen exactly end-on — would divide by zero. It gets a scale that keeps
            // it on screen instead of an exception.
            var scaleX = bounds.Width > 1e-9 ? usableWidth / bounds.Width : usableWidth;
            var scaleY = bounds.Height > 1e-9 ? usableHeight / bounds.Height : usableHeight;

            return Math.Min(scaleX, scaleY);
        }

        private static Shape Build(
            CantileverViewCurve curve, Bounds2D bounds, double scale, double width, double height)
        {
            if (curve.Points == null || curve.Points.Count == 0)
            {
                return null;
            }

            var offsetX = (width - bounds.Width * scale) / 2.0;
            var offsetY = (height - bounds.Height * scale) / 2.0;

            var points = new PointCollection(curve.Points.Count);

            foreach (var point in curve.Points)
            {
                // Y is flipped: the plan is in model space (Y up) and a canvas grows downwards.
                points.Add(new Point(
                    offsetX + (point.X - bounds.MinX) * scale,
                    height - offsetY - (point.Y - bounds.MinY) * scale));
            }

            var brush = BrushFor(curve.Role);

            if (curve.IsCircle)
            {
                return Hole(points[0], curve.CircleDiameter.Value * scale, brush);
            }

            if (!curve.IsClosed)
            {
                return new Polyline { Points = points, Stroke = brush, StrokeThickness = 1.4 };
            }

            return new Polygon
            {
                Points = points,
                Stroke = brush,
                StrokeThickness = 1.0,

                // A translucent fill so overlapping pieces stay readable: a column behind an arm must not
                // disappear under it.
                Fill = Translucent(brush)
            };
        }

        /// <summary>
        /// A hole, as a circle of its real diameter — with a FLOOR so it stays visible.
        ///
        /// A 9/16 in hole on a line 20 ft long is a fraction of a pixel at any scale that shows the whole rack,
        /// and a hole nobody can see is indistinguishable from the hole that was missing. The floor is a
        /// VIEWING aid of the preview and travels nowhere: the drawing gets the real diameter.
        /// </summary>
        private static Shape Hole(Point centre, double diameter, Brush brush)
        {
            var size = Math.Max(diameter, MinimumHolePixels);

            var ellipse = new System.Windows.Shapes.Ellipse
            {
                Width = size,
                Height = size,
                Stroke = brush,
                StrokeThickness = 1.0,
                Fill = Translucent(brush)
            };

            Canvas.SetLeft(ellipse, centre.X - size / 2.0);
            Canvas.SetTop(ellipse, centre.Y - size / 2.0);
            return ellipse;
        }

        private static Brush Translucent(Brush brush)
        {
            if (!(brush is SolidColorBrush solid))
            {
                return Brushes.Transparent;
            }

            var colour = solid.Color;
            var faded = new SolidColorBrush(Color.FromArgb(0x40, colour.R, colour.G, colour.B));
            faded.Freeze();
            return faded;
        }

        private static TextBlock Message(string text)
        {
            var block = new TextBlock
            {
                Text = text,
                Foreground = EmptyBrush,
                FontSize = 11.5,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 320.0
            };

            Canvas.SetLeft(block, Margin);
            Canvas.SetTop(block, Margin);
            return block;
        }

        /// <summary>The legend entries a view needs: the kinds it actually drew, in enum order.</summary>
        internal static IReadOnlyList<CantileverViewPieceKind> KindsIn(CantileverViewPlan plan) =>
            plan == null
                ? Array.Empty<CantileverViewPieceKind>()
                : plan.Curves.Select(c => c.Kind).Distinct().OrderBy(k => (int)k).ToList();
    }
}
