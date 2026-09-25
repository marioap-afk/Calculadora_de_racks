using System;
using RackCad.Application.Geometry;
using RackCad.Application.Views.Placement;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14 (B, C, D): the single rigid transform of one projection operation. Owner decision OD-6.c A froze
    /// alpha = 0, so the common transform behaves like COPY while keeping the formal contract.
    /// </summary>
    public class CommonTransform2DTests
    {
        [Fact]
        public void G14_B_COMMON_TRANSFORM_TAKES_THE_BASE_POINT_TO_THE_TARGET_POINT()
        {
            var result = CommonTransform2D.TryCreate(new Point3D(10, -4, 2), new Point3D(500, 25, 7), 0.0);

            Assert.True(result.IsAvailable);
            var image = result.Transform.Apply(new Point2D(10, -4));
            Assert.Equal(500.0, image.X, 9);
            Assert.Equal(25.0, image.Y, 9);
        }

        [Fact]
        public void G14_B_COMMON_TRANSFORM_TAKES_B_TO_T_ALSO_WITH_A_ROTATION()
        {
            var result = CommonTransform2D.TryCreate(new Point3D(3, 5, 0), new Point3D(-20, 11, 0), Math.PI / 3.0);

            Assert.True(result.IsAvailable);
            var image = result.Transform.Apply(new Point2D(3, 5));
            Assert.Equal(-20.0, image.X, 9);
            Assert.Equal(11.0, image.Y, 9);
        }

        [Fact]
        public void G14_C_D_COMMON_TRANSFORM_IS_RIGID_WITH_DETERMINANT_ONE_AND_UNIT_SCALE()
        {
            var result = CommonTransform2D.TryCreate(new Point3D(0, 0, 0), new Point3D(120, 0, 0), Math.PI / 4.0);

            Assert.True(result.IsAvailable);
            Assert.Equal(1.0, result.Transform.Determinant, 9);
            Assert.Equal(1.0, result.Transform.ScaleFactor, 9);
        }

        [Fact]
        public void G14_COMMON_TRANSFORM_PRESERVES_DISTANCES_BETWEEN_ANY_TWO_POINTS()
        {
            var transform = CommonTransform2D
                .TryCreate(new Point3D(4, 4, 0), new Point3D(90, -12, 0), Math.PI / 6.0).Transform;

            var a = new Point2D(17, 3);
            var b = new Point2D(-6, 41);
            var imageA = transform.Apply(a);
            var imageB = transform.Apply(b);

            Assert.Equal(
                Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)),
                Math.Sqrt(Math.Pow(imageA.X - imageB.X, 2) + Math.Pow(imageA.Y - imageB.Y, 2)),
                9);
        }

        [Fact]
        public void G14_O_FROZEN_RIGID_ALPHA_IS_ZERO_AND_BEHAVES_AS_A_COMMON_TRANSLATION()
        {
            var transform = CommonTransform2D
                .TryCreate(new Point3D(2, 3, 0), new Point3D(202, 53, 0), 0.0).Transform;

            Assert.Equal(0.0, transform.AlphaRadians, 12);
            var image = transform.Apply(new Point2D(40, 10));
            Assert.Equal(240.0, image.X, 9);
            Assert.Equal(60.0, image.Y, 9);
        }

        [Fact]
        public void G14_COMMON_TRANSFORM_REJECTS_NON_FINITE_POINTS_AND_ANGLES()
        {
            Assert.Equal(
                CommonTransformFailure.NonFinite,
                CommonTransform2D.TryCreate(new Point3D(double.NaN, 0, 0), new Point3D(1, 1, 0), 0.0).Failure);
            Assert.Equal(
                CommonTransformFailure.NonFinite,
                CommonTransform2D.TryCreate(new Point3D(0, 0, 0), new Point3D(double.PositiveInfinity, 1, 0), 0.0).Failure);
            Assert.Equal(
                CommonTransformFailure.NonFinite,
                CommonTransform2D.TryCreate(new Point3D(0, 0, 0), new Point3D(1, 1, 0), double.NaN).Failure);
        }
    }
}
