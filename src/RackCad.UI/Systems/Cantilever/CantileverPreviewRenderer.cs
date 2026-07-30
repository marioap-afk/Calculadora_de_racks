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

        private static readonly Brush ColumnBrush = UiSupport.FrozenBrush(Color.FromRgb(0x3D, 0xC9, 0x86));
        private static readonly Brush BaseBrush = UiSupport.FrozenBrush(Color.FromRgb(0x5B, 0x8D, 0xEF));
        private static readonly Brush ArmBrush = UiSupport.FrozenBrush(Color.FromRgb(0xE0, 0x8A, 0x2B));
        private static readonly Brush PlateBrush = UiSupport.FrozenBrush(Color.FromRgb(0xB7, 0xC3, 0xCF));
        private static readonly Brush GussetBrush = UiSupport.FrozenBrush(Color.FromRgb(0x8A, 0x97, 0xA4));
        private static readonly Brush SeparatorBrush = UiSupport.FrozenBrush(Color.FromRgb(0xC0, 0x5B, 0xC0));
        private static readonly Brush BraceBrush = UiSupport.FrozenBrush(Color.FromRgb(0xFF, 0xD1, 0x66));
        private static readonly Brush AdapterBrush = UiSupport.FrozenBrush(Color.FromRgb(0xC0, 0x5B, 0x5B));
        private static readonly Brush PunchBrush = UiSupport.FrozenBrush(Color.FromRgb(0x61, 0x70, 0x80));
        private static readonly Brush EmptyBrush = UiSupport.FrozenBrush(Color.FromRgb(0x8A, 0x97, 0xA4));

        /// <summary>The colour a piece kind reads in, so the legend and the drawing agree by construction.</summary>
        internal static Brush BrushFor(CantileverViewPieceKind kind)
        {
            switch (kind)
            {
                case CantileverViewPieceKind.Column: return ColumnBrush;
                case CantileverViewPieceKind.Base: return BaseBrush;
                case CantileverViewPieceKind.Arm: return ArmBrush;
                case CantileverViewPieceKind.Plate: return PlateBrush;
                case CantileverViewPieceKind.Gusset: return GussetBrush;
                case CantileverViewPieceKind.Separator: return SeparatorBrush;
                case CantileverViewPieceKind.Brace: return BraceBrush;
                case CantileverViewPieceKind.ColdRolledAdapter: return AdapterBrush;
                case CantileverViewPieceKind.Punch: return PunchBrush;

                // A kind added without a colour would otherwise be invisible, which is worse than being ugly.
                default: return EmptyBrush;
            }
        }

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

            var brush = BrushFor(curve.Kind);

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
