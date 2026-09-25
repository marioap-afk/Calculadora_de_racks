using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14 (X): semantic frames. Origin, axes, span, center and variant offsets come from AUTH-05; nothing is
    /// derived from a bounding box and the target frame is frozen to phi_t = 0 (Owner decision OD-6.b A).
    /// </summary>
    public class RackGroupFrameTests
    {
        [Fact]
        public void G14_TARGET_FRAME_IS_UNIVERSAL_X_AND_SOURCE_FRAME_KEEPS_ALPHA_ZERO()
        {
            Assert.Equal(0.0, TargetGroupFrame.Universal(new Point3D(10, 20, 3)).OrientationRadians, 12);
            Assert.Equal(0.0, SourceGroupFrame.Universal(new Point3D(-4, 8, 1)).OrientationRadians, 12);
            Assert.Equal(3.0, TargetGroupFrame.Universal(new Point3D(10, 20, 3)).TargetPoint.Z, 12);
        }

        [Fact]
        public void G14_FRONTAL_FRAME_EXPOSES_RUN_ON_LOCAL_X_WITH_ITS_PHYSICAL_ORIGIN()
        {
            var frame = G14.FrontalFrame(runOrigin: 12.0, length: 90.0);

            Assert.True(RackViewFrameSemantics.TryAxisDirection(frame, RackPhysicalAxis.Run, out var direction));
            Assert.Equal(new Vector2D(1, 0), direction);
            Assert.Equal(12.0, RackViewFrameSemantics.OriginOn(frame, RackPhysicalAxis.Run), 9);

            Assert.True(RackViewFrameSemantics.TrySpanOn(frame, RackPhysicalAxis.Run, out var min, out var max));
            Assert.Equal(12.0, min, 9);
            Assert.Equal(102.0, max, 9);
        }

        [Fact]
        public void G14_PLANTA_FRAME_EXPOSES_RUN_ON_LOCAL_Y_AND_DEPTH_ON_LOCAL_X()
        {
            var frame = G14.PlantaFrame(depth: 48.0, runOrigin: 7.0);

            Assert.True(RackViewFrameSemantics.TryAxisDirection(frame, RackPhysicalAxis.Run, out var run));
            Assert.Equal(new Vector2D(0, 1), run);
            Assert.True(RackViewFrameSemantics.TryAxisDirection(frame, RackPhysicalAxis.Depth, out var depth));
            Assert.Equal(new Vector2D(1, 0), depth);

            Assert.True(RackViewFrameSemantics.TrySpanOn(frame, RackPhysicalAxis.Depth, out var min, out var max));
            Assert.Equal(0.0, min, 9);
            Assert.Equal(48.0, max, 9);

            Assert.False(RackViewFrameSemantics.TrySpanOn(frame, RackPhysicalAxis.Height, out _, out _));
        }

        [Fact]
        public void G14_LOCAL_POINT_KEEPS_EVERY_OTHER_PHYSICAL_COORDINATE_ON_THE_FRAME_ORIGIN()
        {
            var frontal = G14.FrontalFrame(runOrigin: 12.0, length: 90.0);

            Assert.True(RackViewFrameSemantics.TryLocalPointAt(frontal, RackPhysicalAxis.Run, 102.0, out var local));
            Assert.Equal(102.0, local.X, 9);
            Assert.Equal(0.0, local.Y, 9);

            var planta = G14.PlantaFrame(depth: 48.0, runOrigin: 7.0);
            Assert.True(RackViewFrameSemantics.TryLocalPointAt(planta, RackPhysicalAxis.Run, 97.0, out var plantaLocal));
            Assert.Equal(0.0, plantaLocal.X, 9);
            Assert.Equal(97.0, plantaLocal.Y, 9);
        }

        [Fact]
        public void G14_ANCHOR_IS_THE_LOCAL_POINT_OF_THE_PHYSICAL_ORIGIN()
        {
            var frame = G14.LateralFrame(runOrigin: 33.0, depth: 48.0);
            var anchor = RackViewFrameSemantics.Anchor(frame);

            Assert.Equal(0.0, anchor.X, 9);
            Assert.Equal(0.0, anchor.Y, 9);
            Assert.Equal(33.0, RackViewFrameSemantics.OriginOn(frame, RackPhysicalAxis.Run), 9);
        }

        [Fact]
        public void G14_X_DIFFERENT_SPANS_KEEP_THEIR_OWN_KMIN_AND_LENGTH()
        {
            var shortFrame = G14.FrontalFrame(runOrigin: 0.0, length: 90.0);
            var longFrame = G14.FrontalFrame(runOrigin: 250.0, length: 150.0);

            RackViewFrameSemantics.TrySpanOn(shortFrame, RackPhysicalAxis.Run, out var shortMin, out var shortMax);
            RackViewFrameSemantics.TrySpanOn(longFrame, RackPhysicalAxis.Run, out var longMin, out var longMax);

            Assert.Equal(0.0, shortMin, 9);
            Assert.Equal(90.0, shortMax, 9);
            Assert.Equal(250.0, longMin, 9);
            Assert.Equal(400.0, longMax, 9);
        }

        [Fact]
        public void G14_FRAME_CENTER_COMES_FROM_THE_SEMANTIC_SPAN_NOT_FROM_DRAWN_BOUNDS()
        {
            var frame = G14.FrontalFrame(runOrigin: 0.0, length: 90.0);
            Assert.Equal(45.0, frame.Center, 9);
            Assert.Null(frame.DrawnBounds);
        }
    }
}
