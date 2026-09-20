using System;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Shared
{
    public enum RackTransformFailure
    {
        None,
        InvalidTolerance,
        Degenerate,
        NonUniformScale,
        NonOrthogonal
    }

    /// <summary>Mathematical facts from a supported affine decomposition. It carries no mirror or placement policy.</summary>
    public readonly struct RackTransformDescription
    {
        internal RackTransformDescription(
            Vector2D translation,
            double rotationRadians,
            double determinant,
            bool isReflection,
            double uniformScale)
        {
            Translation = translation;
            RotationRadians = rotationRadians;
            Determinant = determinant;
            IsReflection = isReflection;
            UniformScale = uniformScale;
        }

        public Vector2D Translation { get; }
        public double RotationRadians { get; }
        public double Determinant { get; }
        public bool IsReflection { get; }
        public double UniformScale { get; }
    }

    public readonly struct RackTransformFactResult
    {
        private RackTransformFactResult(
            bool isSupported, RackTransformDescription facts, RackTransformFailure failure, double tolerance)
        {
            IsSupported = isSupported;
            Facts = facts;
            Failure = failure;
            Tolerance = tolerance;
        }

        public bool IsSupported { get; }
        public RackTransformDescription Facts { get; }
        public RackTransformFailure Failure { get; }
        public double Tolerance { get; }

        internal static RackTransformFactResult Supported(RackTransformDescription facts, double tolerance)
            => new RackTransformFactResult(true, facts, RackTransformFailure.None, tolerance);

        internal static RackTransformFactResult Unsupported(RackTransformFailure failure, double tolerance)
            => new RackTransformFactResult(false, default, failure, tolerance);
    }

    /// <summary>
    /// Decomposes the existing <see cref="Transform2D"/> into neutral facts. The caller supplies the tolerance;
    /// non-uniform, sheared and degenerate matrices remain representable by Transform2D but fail closed here.
    /// </summary>
    public static class RackTransformFacts
    {
        public static RackTransformFactResult Describe(Transform2D transform, double tolerance)
        {
            if (!GeometryTolerance.IsFinite(tolerance) || tolerance < 0.0)
            {
                return RackTransformFactResult.Unsupported(RackTransformFailure.InvalidTolerance, tolerance);
            }

            var first = new Vector2D(transform.M11, transform.M21);
            var second = new Vector2D(transform.M12, transform.M22);
            var firstLength = first.Length;
            var secondLength = second.Length;

            if (firstLength <= tolerance || secondLength <= tolerance)
            {
                return RackTransformFactResult.Unsupported(RackTransformFailure.Degenerate, tolerance);
            }

            var orthogonalScale = Math.Max(1.0, firstLength * secondLength);
            if (Math.Abs(first.Dot(second)) > tolerance * orthogonalScale)
            {
                return RackTransformFactResult.Unsupported(RackTransformFailure.NonOrthogonal, tolerance);
            }

            var lengthScale = Math.Max(1.0, Math.Max(firstLength, secondLength));
            if (Math.Abs(firstLength - secondLength) > tolerance * lengthScale)
            {
                return RackTransformFactResult.Unsupported(RackTransformFailure.NonUniformScale, tolerance);
            }

            var determinant = transform.Determinant;
            if (Math.Abs(determinant) <= tolerance * Math.Max(1.0, firstLength * secondLength))
            {
                return RackTransformFactResult.Unsupported(RackTransformFailure.Degenerate, tolerance);
            }

            return RackTransformFactResult.Supported(
                new RackTransformDescription(
                    new Vector2D(transform.Dx, transform.Dy),
                    Math.Atan2(transform.M21, transform.M11),
                    determinant,
                    determinant < 0.0,
                    (firstLength + secondLength) / 2.0),
                tolerance);
        }
    }
}
