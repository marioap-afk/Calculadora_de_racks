using System.Windows;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The shared world→canvas projection. Uses only value types, so it runs on the normal thread.</summary>
    public sealed class PreviewProjectionTests
    {
        [Fact]
        public void Fit_UsesUniformScale_AndCentersWithoutMargin()
        {
            var projection = PreviewProjection.Fit(new Rect(0, 0, 100, 50), new Size(200, 100), new Thickness(0));

            Assert.True(projection.IsDrawable);
            Assert.Equal(2.0, projection.Scale, 6); // min(200/100, 100/50) = 2
        }

        [Fact]
        public void Project_FlipsYAxis()
        {
            var projection = PreviewProjection.Fit(new Rect(0, 0, 100, 50), new Size(200, 100), new Thickness(0));

            var bottom = projection.Project(0, 0);
            var top = projection.Project(0, 50);

            Assert.Equal(0.0, bottom.X, 6);
            Assert.Equal(100.0, bottom.Y, 6); // world min-Y sits at the baseline (largest canvas Y)
            Assert.Equal(0.0, top.Y, 6);       // world max-Y sits at the top (smallest canvas Y)
            Assert.True(top.Y < bottom.Y);
        }

        [Fact]
        public void Fit_CentersTheShorterAxis()
        {
            // world 100x50 into a square 100x100 canvas → scale 1, 50-tall content centered vertically.
            var projection = PreviewProjection.Fit(new Rect(0, 0, 100, 50), new Size(100, 100), new Thickness(0));

            Assert.Equal(1.0, projection.Scale, 6);
            var bottom = projection.Project(0, 0);
            var top = projection.Project(0, 50);
            Assert.Equal(75.0, bottom.Y, 6); // (usableH + drawnH)/2 = (100+50)/2
            Assert.Equal(25.0, top.Y, 6);
        }

        [Fact]
        public void Unproject_RoundTripsProject()
        {
            var projection = PreviewProjection.Fit(new Rect(10, 20, 80, 40), new Size(300, 200), new Thickness(12));
            var world = new Point(42, 33);

            var back = projection.Unproject(projection.Project(world));

            Assert.Equal(world.X, back.X, 6);
            Assert.Equal(world.Y, back.Y, 6);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]     // no world extent
        [InlineData(0, 0, 100, 50)]  // valid world, but zero canvas below
        public void Fit_Degenerate_IsNotDrawable(double x, double y, double w, double h)
        {
            var canvas = (w > 0 && h > 0) ? new Size(0, 0) : new Size(200, 100);
            var projection = PreviewProjection.Fit(new Rect(x, y, w, h), canvas, new Thickness(0));

            Assert.False(projection.IsDrawable);
            Assert.Equal(0.0, projection.Scale, 6);
        }

        [Fact]
        public void Fit_RespectsMargins()
        {
            var projection = PreviewProjection.Fit(new Rect(0, 0, 100, 100), new Size(140, 140), new Thickness(20));

            // usable 100x100 into world 100x100 → scale 1, content spans the inner box exactly.
            Assert.Equal(1.0, projection.Scale, 6);
            var bottomLeft = projection.Project(0, 0);
            Assert.Equal(20.0, bottomLeft.X, 6);
            Assert.Equal(120.0, bottomLeft.Y, 6);
        }
    }
}
