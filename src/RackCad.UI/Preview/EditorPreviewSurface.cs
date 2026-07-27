using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.Preview
{
    /// <summary>
    /// The shared preview SURFACE: the world→canvas transform plus the primitive vocabulary (lines, rectangles, labels,
    /// dashes) every editor paints with. Extracted from the dynamic editor's renderer, which now consumes it, so the two
    /// editors cannot drift into separate pipelines.
    ///
    /// Neutral by contract (ADR-0019 §3): no <c>RackSystemKind</c>, no branch per system. What varies — which plans to
    /// draw, which labels, which selection — is supplied by each editor.
    ///
    /// The transform is the dynamic renderer's own: <c>x → offsetX + x·scale</c> and <c>y → bottomY − y·scale</c>
    /// (Y grows upward in world space and downward on the canvas), so a migrated editor produces identical coordinates.
    /// </summary>
    internal sealed class EditorPreviewSurface
    {
        private readonly Canvas canvas;
        private readonly PreviewCanvasPainter painter;

        public EditorPreviewSurface(Canvas canvas)
        {
            this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            painter = new PreviewCanvasPainter(canvas);
        }

        /// <summary>World units per canvas unit. Zero until <see cref="Fit"/> succeeds — nothing should be drawn before.</summary>
        public double Scale { get; private set; }

        public double OffsetX { get; private set; }

        public double BottomY { get; private set; }

        public double CanvasWidth => canvas.ActualWidth;

        public double CanvasHeight => canvas.ActualHeight;

        /// <summary>True once <see cref="Fit"/> produced a usable transform.</summary>
        public bool IsReady => Scale > 0.0;

        public void Clear() => canvas.Children.Clear();

        /// <summary>
        /// Fit a world box into the canvas, honouring the given margins, and return whether a usable transform resulted.
        /// This is the dynamic renderer's own fit: uniform scale (the smaller of the two ratios), horizontally centred on
        /// the canvas and vertically centred inside the usable band.
        /// </summary>
        public bool Fit(
            double worldStartX, double worldLength, double worldHeight,
            double horizontalMargin, double topMargin, double bottomMargin)
        {
            Scale = 0.0;
            var availableWidth = canvas.ActualWidth;
            var availableHeight = canvas.ActualHeight;
            if (worldLength <= 0.0 || worldHeight <= 0.0 || availableWidth < 20.0 || availableHeight < 20.0)
            {
                return false;
            }

            var usableWidth = Math.Max(1.0, availableWidth - 2 * horizontalMargin);
            var usableHeight = Math.Max(1.0, availableHeight - topMargin - bottomMargin);
            var scale = Math.Min(usableWidth / worldLength, usableHeight / worldHeight);
            if (scale <= 0.0)
            {
                return false;
            }

            var drawHeight = worldHeight * scale;
            Scale = scale;
            OffsetX = (availableWidth - worldLength * scale) / 2.0 - worldStartX * scale;
            BottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;
            return true;
        }

        /// <summary>Adopt an already-computed transform (used while an editor migrates its own fit onto the surface).</summary>
        public void UseTransform(double scale, double offsetX, double bottomY)
        {
            Scale = scale;
            OffsetX = offsetX;
            BottomY = bottomY;
        }

        /// <summary>World → canvas.</summary>
        public Point Map(double x, double y) => new Point(OffsetX + x * Scale, BottomY - y * Scale);

        /// <summary>Canvas → world (for hit-testing a click).</summary>
        public Point Unmap(Point canvasPoint)
            => Scale <= 0.0
                ? new Point(0.0, 0.0)
                : new Point((canvasPoint.X - OffsetX) / Scale, (BottomY - canvasPoint.Y) / Scale);

        public void Line(Point a, Point b, Brush stroke, double thickness, DoubleCollection dash = null)
            => painter.AddLine(a, b, stroke, thickness, dash);

        /// <summary>A line given in WORLD coordinates.</summary>
        public void WorldLine(double x1, double y1, double x2, double y2, Brush stroke, double thickness, DoubleCollection dash = null)
            => painter.AddLine(Map(x1, y1), Map(x2, y2), stroke, thickness, dash);

        public System.Windows.Shapes.Rectangle Rectangle(
            double left, double top, double width, double height,
            Brush stroke, double thickness, DoubleCollection dash = null, Brush fill = null)
            => painter.AddRectangle(left, top, width, height, stroke, thickness, dash, fill);

        /// <summary>A rectangle given by two WORLD corners (drawn in canvas space, so it stays axis-aligned).</summary>
        public void WorldRectangle(
            double x1, double y1, double x2, double y2,
            Brush stroke, double thickness, DoubleCollection dash = null, Brush fill = null)
        {
            var a = Map(x1, y1);
            var b = Map(x2, y2);
            var left = Math.Min(a.X, b.X);
            var top = Math.Min(a.Y, b.Y);
            painter.AddRectangle(left, top, Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y), stroke, thickness, dash, fill);
        }

        public void Label(double left, double top, string text, Brush brush, double size, double maxWidth = 0.0, FontWeight? fontWeight = null)
        {
            var label = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = size,
                FontWeight = fontWeight ?? FontWeights.Normal,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            if (maxWidth > 0.0)
            {
                label.Width = maxWidth;
                label.TextAlignment = TextAlignment.Left;
            }

            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }

        public void CenteredLabel(double centerX, double top, string text, double maxWidth, Brush brush, double size, FontWeight? fontWeight = null)
        {
            var width = Math.Max(24.0, maxWidth);
            var label = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = size,
                FontWeight = fontWeight ?? FontWeights.Normal,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Width = width
            };

            Canvas.SetLeft(label, centerX - width / 2.0);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }

        /// <summary>The shared dash pattern.</summary>
        public static DoubleCollection Dash() => new DoubleCollection { 5, 3 };

        /// <summary>
        /// A block instance's LOCAL point in world coordinates (mirror, then rotation, then insertion) — the same
        /// transform the drawing pipeline uses, so a preview never invents its own placement maths.
        /// </summary>
        public static Point2D LocalToWorld(HeaderBlockInstance instance, Point2D local)
        {
            var localX = instance.MirroredX ? -local.X : local.X;
            var localY = instance.MirroredY ? -local.Y : local.Y;
            var cos = Math.Cos(instance.RotationRadians);
            var sin = Math.Sin(instance.RotationRadians);
            return new Point2D(
                instance.Insertion.X + localX * cos - localY * sin,
                instance.Insertion.Y + localX * sin + localY * cos);
        }
    }
}
