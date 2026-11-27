using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Esquema de la cama» (question 2 of the prototype «camas»): ONE line at scale with the tope, the rollers and the brakes where the builder
    /// places them (<see cref="CamaLayout"/>), over the rail, with the marks of the scale. It is a scheme, not a view of the drawing: no
    /// dimensions. Its colors are the tokens of the shell (the tope in the color of the error, the rollers of «correcto», the brakes of
    /// information), read from the resources of the view, with plain colors when it is shown alone.
    /// </summary>
    public sealed class CamaSchemaStrip : FrameworkElement
    {
        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
            nameof(Layout), typeof(CamaLayout), typeof(CamaSchemaStrip), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TickStepProperty = DependencyProperty.Register(
            nameof(TickStep), typeof(double), typeof(CamaSchemaStrip), new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));

        private const double Pad = 8.0;

        public CamaSchemaStrip()
        {
            Height = 64;
            SnapsToDevicePixels = true;
        }

        /// <summary>Where the pieces are, measured from the troquel of the tope; nothing is drawn without it.</summary>
        public CamaLayout Layout
        {
            get => (CamaLayout)GetValue(LayoutProperty);
            set => SetValue(LayoutProperty, value);
        }

        /// <summary>How far apart the marks of the scale are (in).</summary>
        public double TickStep
        {
            get => (double)GetValue(TickStepProperty);
            set => SetValue(TickStepProperty, value);
        }

        /// <summary>The x of a position of the rail (from its start), in this element.</summary>
        internal double XOf(double railPosition)
        {
            var layout = Layout;
            if (layout == null || !(layout.RailLength > 0.0)) return Pad;
            var width = Math.Max(1.0, ActualWidth - 2 * Pad);
            return Pad + railPosition / layout.RailLength * width;
        }

        protected override AutomationPeer OnCreateAutomationPeer() => new CamaSchemaStripPeer(this);

        protected override void OnRender(DrawingContext drawing)
        {
            base.OnRender(drawing);
            var layout = Layout;
            if (layout == null || !(layout.RailLength > 0.0) || ActualWidth < 2 * Pad + 10) return;

            var rail = Brush("ShellTextSecondaryBrush", Color.FromRgb(0x60, 0x70, 0x80));
            var track = Brush("ShellBorderBrush", Color.FromRgb(0xD8, 0xDE, 0xE6));
            var tope = Brush("ShellStatusErrorBrush", Color.FromRgb(0xB0, 0x00, 0x20));
            var roller = Brush("ShellStatusSuccessBrush", Color.FromRgb(0x2F, 0x85, 0x5A));
            var brake = Brush("ShellStatusInfoBrush", Color.FromRgb(0x2B, 0x6C, 0xB0));

            const double top = 6.0;
            const double lineY = 22.0;
            const double railY = 32.0;
            var left = XOf(0.0);
            var right = XOf(layout.RailLength);

            // The rail and the line of the troqueles.
            drawing.DrawRectangle(null, new Pen(track, 1.0), new Rect(left, top, Math.Max(1.0, right - left), railY - top));
            drawing.DrawLine(new Pen(rail, 3.0), new Point(left, railY), new Point(right, railY));

            // The tope: from the start of the rail to the end of what it occupies past its troquel.
            var topeRight = XOf(Math.Min(layout.RailLength, layout.TopeOrigin + layout.TopeLength));
            var topeFill = tope.Clone();
            topeFill.Opacity = 0.25;
            drawing.DrawRectangle(topeFill, new Pen(tope, 1.0), new Rect(left, top, Math.Max(1.0, topeRight - left), railY - top));

            // The rollers and the brakes on the line of the troqueles, at their positions; a brake is bigger and filled.
            var spacing = layout.Pitch / layout.RailLength * (right - left);
            var radius = Math.Max(1.5, Math.Min(6.0, spacing / 2.0 - 0.5));
            foreach (var at in layout.Rollers)
                drawing.DrawEllipse(null, new Pen(roller, 1.5), new Point(XOf(layout.TopeOrigin + at), lineY), radius, radius);
            foreach (var at in layout.Brakes)
                drawing.DrawEllipse(brake, new Pen(brake, 1.5), new Point(XOf(layout.TopeOrigin + at), lineY), radius + 2.0, radius + 2.0);

            // The marks of the scale, with their inches.
            var step = TickStep > 0.0 ? TickStep : 12.0;
            var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            for (var x = step; x < layout.RailLength + 1e-9; x += step)
            {
                var px = XOf(x);
                drawing.DrawLine(new Pen(rail, 1.0), new Point(px, railY + 2.0), new Point(px, railY + 8.0));
                var text = new FormattedText(x.ToString("0.##", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 9.0, rail, dpi);
                drawing.DrawText(text, new Point(Math.Min(px - text.Width / 2.0, ActualWidth - text.Width), railY + 9.0));
            }
        }

        private Brush Brush(string key, Color fallback) => TryFindResource(key) as Brush ?? new SolidColorBrush(fallback);

        private sealed class CamaSchemaStripPeer : FrameworkElementAutomationPeer
        {
            public CamaSchemaStripPeer(CamaSchemaStrip owner)
                : base(owner)
            {
            }

            protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Image;

            protected override string GetClassNameCore() => nameof(CamaSchemaStrip);
        }
    }
}
