using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The row of the strip of the tramos of «medio frente» (I-64 A-14, line D, fourth batch, part «ins»; DG-11 of the prototype
    /// «dialogos»): its children side by side, each one as wide as its <see cref="WeightProperty"/> share of what is left once the children
    /// with no weight (the intermediate posts, of a fixed width) took theirs. Every child has the height of the row.
    /// </summary>
    public sealed class SegmentStripPanel : Panel
    {
        /// <summary>The share of the width of a child (a tramo: its length over the width of the front); 0 = its own width (a post).</summary>
        public static readonly DependencyProperty WeightProperty = DependencyProperty.RegisterAttached(
            "Weight", typeof(double), typeof(SegmentStripPanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static double GetWeight(DependencyObject element) => element == null ? 0.0 : (double)element.GetValue(WeightProperty);

        public static void SetWeight(DependencyObject element, double value) => element?.SetValue(WeightProperty, value);

        protected override Size MeasureOverride(Size availableSize)
        {
            var fixedWidth = 0.0;
            var height = 0.0;
            var weights = 0.0;
            foreach (UIElement child in InternalChildren)
            {
                var weight = Math.Max(0.0, GetWeight(child));
                if (weight > 0.0)
                {
                    weights += weight;
                    continue;
                }

                child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                fixedWidth += child.DesiredSize.Width;
                height = Math.Max(height, child.DesiredSize.Height);
            }

            var free = double.IsInfinity(availableSize.Width) ? 0.0 : Math.Max(0.0, availableSize.Width - fixedWidth);
            foreach (UIElement child in InternalChildren)
            {
                var weight = Math.Max(0.0, GetWeight(child));
                if (!(weight > 0.0)) continue;
                child.Measure(new Size(weights > 0.0 ? free * weight / weights : 0.0, availableSize.Height));
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Size(double.IsInfinity(availableSize.Width) ? fixedWidth : availableSize.Width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var fixedWidth = 0.0;
            var weights = 0.0;
            foreach (UIElement child in InternalChildren)
            {
                var weight = Math.Max(0.0, GetWeight(child));
                if (weight > 0.0) weights += weight;
                else fixedWidth += child.DesiredSize.Width;
            }

            var free = Math.Max(0.0, finalSize.Width - fixedWidth);
            var x = 0.0;
            foreach (UIElement child in InternalChildren)
            {
                var weight = Math.Max(0.0, GetWeight(child));
                var width = weight > 0.0 ? (weights > 0.0 ? free * weight / weights : 0.0) : child.DesiredSize.Width;
                child.Arrange(new Rect(x, 0.0, width, finalSize.Height));
                x += width;
            }

            return finalSize;
        }
    }
}
