using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using RackCad.Application.Systems.Larguero;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 lg (I-64, sexta tanda; the Owner, point 3: «un alzado esquemático con el peralte y la ménsula a escala»): the view of the page
    /// «Larguero». It draws the sketch of the plan of the piece (<see cref="LargueroPieceSketch"/>: the same plan AutoCAD inserts) to ONE scale
    /// that fits the element, with a long piece broken in the middle and the break marked. The profile in the color of the text, the ménsulas
    /// in the color of «correcto», the notes in the secondary color: the tokens of the shell, read from the resources of the view, with plain
    /// colors when it is shown alone. No dimensions: the caption of the page says the measures.
    /// </summary>
    public sealed class LargueroSketchView : FrameworkElement
    {
        public static readonly DependencyProperty SketchProperty = DependencyProperty.Register(
            nameof(Sketch), typeof(LargueroPieceSketch), typeof(LargueroSketchView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        private const double Pad = 10.0;

        public LargueroSketchView()
        {
            Height = 150;
            SnapsToDevicePixels = true;
        }

        public LargueroPieceSketch Sketch
        {
            get => (LargueroPieceSketch)GetValue(SketchProperty);
            set => SetValue(SketchProperty, value);
        }

        /// <summary>The scale of the last drawing (pixels per inch), one for both axes; 0 when nothing was drawn.</summary>
        internal double Scale { get; private set; }

        /// <summary>Where a point of the sketch falls in this element, with the scale that fits it (Y up in the sketch, down on screen).</summary>
        internal Point ToScreen(double x, double y)
        {
            var sketch = Sketch;
            if (sketch == null) return new Point(Pad, Pad);
            var scale = ScaleFor(sketch);
            var bounds = sketch.Bounds;
            var left = Pad + (Math.Max(0.0, ActualWidth - 2 * Pad) - bounds.Width * scale) / 2.0;
            var top = Pad + (Math.Max(0.0, ActualHeight - 2 * Pad) - bounds.Height * scale) / 2.0;
            return new Point(left + (x - bounds.MinX) * scale, top + (bounds.MaxY - y) * scale);
        }

        private double ScaleFor(LargueroPieceSketch sketch)
        {
            var width = Math.Max(1.0, ActualWidth - 2 * Pad);
            var height = Math.Max(1.0, ActualHeight - 2 * Pad);
            var bounds = sketch.Bounds;
            return Math.Min(width / Math.Max(bounds.Width, 1e-6), height / Math.Max(bounds.Height, 1e-6));
        }

        protected override AutomationPeer OnCreateAutomationPeer() => new LargueroSketchViewPeer(this);

        protected override void OnRender(DrawingContext drawing)
        {
            base.OnRender(drawing);
            Scale = 0.0;
            var sketch = Sketch;
            if (sketch == null || sketch.Lines.Count == 0 || ActualWidth < 2 * Pad + 10 || ActualHeight < 2 * Pad + 10) return;

            var profile = new Pen(Brush("ShellTextPrimaryBrush", Color.FromRgb(0x20, 0x28, 0x30)), 1.2);
            var mensula = new Pen(Brush("ShellStatusSuccessBrush", Color.FromRgb(0x2F, 0x85, 0x5A)), 1.6);
            var note = new Pen(Brush("ShellTextSecondaryBrush", Color.FromRgb(0x60, 0x70, 0x80)), 1.0);
            profile.Freeze();
            mensula.Freeze();
            note.Freeze();
            Scale = ScaleFor(sketch);

            foreach (var line in sketch.Lines)
            {
                var pen = line.Role == LargueroCurveRole.Mensula ? mensula : line.Role == LargueroCurveRole.Annotation ? note : profile;
                drawing.DrawLine(pen, ToScreen(line.From.X, line.From.Y), ToScreen(line.To.X, line.To.Y));
            }

            if (sketch.BreakAt is double at)
            {
                // The break: two short zigzags across the gap, above and below the piece.
                var gap = LargueroPieceSketch.BreakGap / 2.0;
                var bounds = sketch.Bounds;
                foreach (var x in new[] { at - gap, at + gap })
                {
                    var top = ToScreen(x, bounds.MaxY);
                    var bottom = ToScreen(x, bounds.MinY);
                    var middle = (top.Y + bottom.Y) / 2.0;
                    var figure = new StreamGeometry();
                    using (var context = figure.Open())
                    {
                        context.BeginFigure(new Point(top.X, top.Y - 6), false, false);
                        context.LineTo(new Point(top.X, middle - 4), true, false);
                        context.LineTo(new Point(top.X + 4, middle - 1), true, false);
                        context.LineTo(new Point(top.X - 4, middle + 1), true, false);
                        context.LineTo(new Point(top.X, middle + 4), true, false);
                        context.LineTo(new Point(bottom.X, bottom.Y + 6), true, false);
                    }

                    figure.Freeze();
                    drawing.DrawGeometry(null, note, figure);
                }
            }
        }

        private Brush Brush(string key, Color fallback) => TryFindResource(key) as Brush ?? new SolidColorBrush(fallback);

        private sealed class LargueroSketchViewPeer : FrameworkElementAutomationPeer
        {
            public LargueroSketchViewPeer(LargueroSketchView owner)
                : base(owner)
            {
            }

            protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Image;

            protected override string GetClassNameCore() => nameof(LargueroSketchView);
        }
    }
}
