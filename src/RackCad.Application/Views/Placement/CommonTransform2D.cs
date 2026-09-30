using RackCad.Application.Geometry;

namespace RackCad.Application.Views.Placement
{
    public enum CommonTransformFailure
    {
        None,
        NonFinite,
        NotRigid
    }

    /// <summary>
    /// The single rigid transform of one projection operation, shared by every RackId group:
    /// Translation(-B).Then(Rotation(alpha)).Then(Translation(T)).
    /// </summary>
    public sealed class CommonTransform2D
    {
        private CommonTransform2D(Point3D basePoint, Point3D targetPoint, double alphaRadians, Transform2D planar)
        {
            BasePoint = basePoint;
            TargetPoint = targetPoint;
            AlphaRadians = alphaRadians;
            Planar = planar;
        }

        public Point3D BasePoint { get; }
        public Point3D TargetPoint { get; }
        public double AlphaRadians { get; }
        public Transform2D Planar { get; }
        public double Determinant => Planar.Determinant;
        public double ScaleFactor => Planar.ScaleFactor;

        public Point2D Apply(Point2D point) => Planar.Apply(point);

        public static CommonTransform2DResult TryCreate(Point3D basePoint, Point3D targetPoint, double alphaRadians)
        {
            if (!GeometryTolerance.IsFinite(basePoint.X) || !GeometryTolerance.IsFinite(basePoint.Y)
                || !GeometryTolerance.IsFinite(basePoint.Z) || !GeometryTolerance.IsFinite(targetPoint.X)
                || !GeometryTolerance.IsFinite(targetPoint.Y) || !GeometryTolerance.IsFinite(targetPoint.Z)
                || !GeometryTolerance.IsFinite(alphaRadians))
            {
                return CommonTransform2DResult.Unavailable(CommonTransformFailure.NonFinite);
            }

            var planar = Transform2D
                .Translation(new Vector2D(-basePoint.X, -basePoint.Y))
                .Then(Transform2D.Rotation(alphaRadians))
                .Then(Transform2D.Translation(new Vector2D(targetPoint.X, targetPoint.Y)));

            if (!GeometryTolerance.AreClose(planar.Determinant, 1.0)
                || !GeometryTolerance.AreClose(planar.ScaleFactor, 1.0))
            {
                return CommonTransform2DResult.Unavailable(CommonTransformFailure.NotRigid);
            }

            return CommonTransform2DResult.Available(
                new CommonTransform2D(basePoint, targetPoint, alphaRadians, planar));
        }
    }

    public readonly struct CommonTransform2DResult
    {
        private CommonTransform2DResult(CommonTransform2D transform, CommonTransformFailure failure)
        {
            Transform = transform;
            Failure = failure;
        }

        public CommonTransform2D Transform { get; }
        public CommonTransformFailure Failure { get; }
        public bool IsAvailable => Failure == CommonTransformFailure.None;

        internal static CommonTransform2DResult Available(CommonTransform2D transform) =>
            new CommonTransform2DResult(transform, CommonTransformFailure.None);

        internal static CommonTransform2DResult Unavailable(CommonTransformFailure failure) =>
            new CommonTransform2DResult(null, failure);
    }
}
