using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The <see cref="PreviewCanvas"/> control (STA): fits a projection and draws in world coordinates.</summary>
    public sealed class PreviewCanvasTests
    {
        private static PreviewCanvas NewCanvas() => new PreviewCanvas { ContentMargins = new Thickness(0) };

        [Fact]
        public void FitToWorld_StoresProjection()
        {
            var scale = StaTestRunner.Run(() =>
            {
                var canvas = NewCanvas();
                var projection = canvas.FitToWorld(new Rect(0, 0, 100, 50), new Size(200, 100));
                return projection.Scale;
            });

            Assert.Equal(2.0, scale, 6);
        }

        [Fact]
        public void DrawWorldLine_ProjectsThenAddsAShape()
        {
            var (count, y1) = StaTestRunner.Run(() =>
            {
                var canvas = NewCanvas();
                canvas.FitToWorld(new Rect(0, 0, 100, 50), new Size(200, 100));
                canvas.DrawWorldLine(new Point(0, 0), new Point(100, 0), Brushes.Black, 1);
                var line = canvas.Children.OfType<Line>().Single();
                return (canvas.Children.Count, line.Y1);
            });

            Assert.Equal(1, count);
            Assert.Equal(100.0, y1, 6); // world Y=0 sits at the baseline (canvas Y=100)
        }

        [Fact]
        public void DrawWorldRectangle_UsesProjectedTopLeftAndSize()
        {
            var (left, top, width, height) = StaTestRunner.Run(() =>
            {
                var canvas = NewCanvas();
                canvas.FitToWorld(new Rect(0, 0, 100, 50), new Size(200, 100));
                var rect = canvas.DrawWorldRectangle(new Point(0, 0), 50, 25, Brushes.Black, 1);
                return (Canvas.GetLeft(rect), Canvas.GetTop(rect), rect.Width, rect.Height);
            });

            Assert.Equal(0.0, left, 6);
            Assert.Equal(50.0, top, 6);   // project(0, 25) = 100 - 25*2 = 50
            Assert.Equal(100.0, width, 6); // 50 * scale(2)
            Assert.Equal(50.0, height, 6); // 25 * scale(2)
        }

        [Fact]
        public void Clear_RemovesShapes()
        {
            var count = StaTestRunner.Run(() =>
            {
                var canvas = NewCanvas();
                canvas.FitToWorld(new Rect(0, 0, 100, 50), new Size(200, 100));
                canvas.DrawWorldLine(new Point(0, 0), new Point(100, 0), Brushes.Black, 1);
                canvas.Clear();
                return canvas.Children.Count;
            });

            Assert.Equal(0, count);
        }
    }
}
