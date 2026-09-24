using System;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>Raw primitive values copied at the AutoCAD boundary. Construction does not imply validity.</summary>
    public readonly struct RackSourcePlacementInput
    {
        public RackSourcePlacementInput(
            double positionX,
            double positionY,
            double positionZ,
            double rotationRadians,
            double scaleX,
            double scaleY,
            double scaleZ,
            double normalX,
            double normalY,
            double normalZ,
            double definitionOriginX,
            double definitionOriginY,
            double definitionOriginZ)
        {
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            RotationRadians = rotationRadians;
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleZ = scaleZ;
            NormalX = normalX;
            NormalY = normalY;
            NormalZ = normalZ;
            DefinitionOriginX = definitionOriginX;
            DefinitionOriginY = definitionOriginY;
            DefinitionOriginZ = definitionOriginZ;
        }

        public double PositionX { get; }
        public double PositionY { get; }
        public double PositionZ { get; }
        public double RotationRadians { get; }
        public double ScaleX { get; }
        public double ScaleY { get; }
        public double ScaleZ { get; }
        public double NormalX { get; }
        public double NormalY { get; }
        public double NormalZ { get; }
        public double DefinitionOriginX { get; }
        public double DefinitionOriginY { get; }
        public double DefinitionOriginZ { get; }
    }

    public readonly struct RackSourceTransformTolerance
    {
        public RackSourceTransformTolerance(double scale, double angleRadians, double normal)
        {
            Scale = scale;
            AngleRadians = angleRadians;
            Normal = normal;
        }

        public double Scale { get; }
        public double AngleRadians { get; }
        public double Normal { get; }
    }

    public sealed class RackSourcePlacementSnapshot
    {
        internal RackSourcePlacementSnapshot(
            Point3D referencePosition,
            double sourceRotationRadians,
            Vector3D scaleFactors,
            Vector3D normal,
            Point3D definitionOrigin)
        {
            ReferencePosition = referencePosition;
            SourceRotationRadians = sourceRotationRadians;
            ScaleFactors = scaleFactors;
            Normal = normal;
            DefinitionOrigin = definitionOrigin;
        }

        public Point3D ReferencePosition { get; }
        public double SourceRotationRadians { get; }
        public Vector3D ScaleFactors { get; }
        public Vector3D Normal { get; }
        public Point3D DefinitionOrigin { get; }
    }

    public enum RackScaleSign
    {
        Negative,
        ZeroWithinTolerance,
        Positive
    }

    public enum RackSourceTransformOutcome
    {
        Facts,
        NonFinite,
        Degenerate,
        InvalidTolerance
    }

    /// <summary>Neutral source-placement facts. It deliberately exposes no composite planar transform.</summary>
    public sealed class RackSourceTransformFactsV2
    {
        internal RackSourceTransformFactsV2(
            RackSourcePlacementSnapshot snapshot,
            double rotationRadians,
            RackScaleSign signX,
            RackScaleSign signY,
            RackScaleSign signZ,
            double referenceBasisDeterminantXY,
            bool referenceBasisIsReflectionXY,
            bool isUniformScale,
            bool isUnitScale,
            bool hasPlanarHalfTurnSign,
            bool isNegativeZ,
            bool normalIsWorldZ,
            RackSourceTransformTolerance tolerance)
        {
            Snapshot = snapshot;
            RotationRadians = rotationRadians;
            SignX = signX;
            SignY = signY;
            SignZ = signZ;
            ReferenceBasisDeterminantXY = referenceBasisDeterminantXY;
            ReferenceBasisIsReflectionXY = referenceBasisIsReflectionXY;
            IsUniformScale = isUniformScale;
            IsUnitScale = isUnitScale;
            HasPlanarHalfTurnSign = hasPlanarHalfTurnSign;
            IsNegativeZ = isNegativeZ;
            NormalIsWorldZ = normalIsWorldZ;
            Tolerance = tolerance;
        }

        public RackSourcePlacementSnapshot Snapshot { get; }
        public Point3D ReferencePosition => Snapshot.ReferencePosition;
        public Point3D DefinitionOrigin => Snapshot.DefinitionOrigin;
        public Vector3D Normal => Snapshot.Normal;
        public double ScaleX => Snapshot.ScaleFactors.X;
        public double ScaleY => Snapshot.ScaleFactors.Y;
        public double ScaleZ => Snapshot.ScaleFactors.Z;
        public double RotationRadians { get; }
        public RackScaleSign SignX { get; }
        public RackScaleSign SignY { get; }
        public RackScaleSign SignZ { get; }
        public double ReferenceBasisDeterminantXY { get; }
        public bool ReferenceBasisIsReflectionXY { get; }
        public bool IsUniformScale { get; }
        public bool IsUnitScale { get; }
        public bool HasPlanarHalfTurnSign { get; }
        public bool IsNegativeZ { get; }
        public bool NormalIsWorldZ { get; }
        public RackSourceTransformTolerance Tolerance { get; }
    }

    public readonly struct RackSourceTransformFactsResult
    {
        private RackSourceTransformFactsResult(
            RackSourceTransformOutcome outcome,
            RackSourceTransformFactsV2 facts)
        {
            Outcome = outcome;
            Facts = facts;
        }

        public RackSourceTransformOutcome Outcome { get; }
        public bool HasFacts => Outcome == RackSourceTransformOutcome.Facts;
        public RackSourceTransformFactsV2 Facts { get; }

        internal static RackSourceTransformFactsResult Success(RackSourceTransformFactsV2 facts)
            => new RackSourceTransformFactsResult(RackSourceTransformOutcome.Facts, facts);

        internal static RackSourceTransformFactsResult Failed(RackSourceTransformOutcome outcome)
            => new RackSourceTransformFactsResult(outcome, null);
    }

    public static class RackSourceTransformClassifier
    {
        public static RackSourceTransformFactsResult Classify(
            RackSourcePlacementInput input,
            RackSourceTransformTolerance tolerance)
        {
            if (!Finite(tolerance.Scale) || !Finite(tolerance.AngleRadians) || !Finite(tolerance.Normal)
                || tolerance.Scale < 0.0 || tolerance.AngleRadians < 0.0 || tolerance.Normal < 0.0)
            {
                return RackSourceTransformFactsResult.Failed(RackSourceTransformOutcome.InvalidTolerance);
            }

            if (!Finite(input.PositionX) || !Finite(input.PositionY) || !Finite(input.PositionZ)
                || !Finite(input.RotationRadians)
                || !Finite(input.ScaleX) || !Finite(input.ScaleY) || !Finite(input.ScaleZ)
                || !Finite(input.NormalX) || !Finite(input.NormalY) || !Finite(input.NormalZ)
                || !Finite(input.DefinitionOriginX) || !Finite(input.DefinitionOriginY)
                || !Finite(input.DefinitionOriginZ))
            {
                return RackSourceTransformFactsResult.Failed(RackSourceTransformOutcome.NonFinite);
            }

            var snapshot = new RackSourcePlacementSnapshot(
                new Point3D(input.PositionX, input.PositionY, input.PositionZ),
                input.RotationRadians,
                new Vector3D(input.ScaleX, input.ScaleY, input.ScaleZ),
                new Vector3D(input.NormalX, input.NormalY, input.NormalZ),
                new Point3D(input.DefinitionOriginX, input.DefinitionOriginY, input.DefinitionOriginZ));
            var signX = Sign(input.ScaleX, tolerance.Scale);
            var signY = Sign(input.ScaleY, tolerance.Scale);
            var signZ = Sign(input.ScaleZ, tolerance.Scale);
            var normalLength = snapshot.Normal.Length;

            if (signX == RackScaleSign.ZeroWithinTolerance
                || signY == RackScaleSign.ZeroWithinTolerance
                || signZ == RackScaleSign.ZeroWithinTolerance
                || normalLength <= tolerance.Normal)
            {
                return RackSourceTransformFactsResult.Failed(RackSourceTransformOutcome.Degenerate);
            }

            var absX = Math.Abs(input.ScaleX);
            var absY = Math.Abs(input.ScaleY);
            var absZ = Math.Abs(input.ScaleZ);
            var minScale = Math.Min(absX, Math.Min(absY, absZ));
            var maxScale = Math.Max(absX, Math.Max(absY, absZ));
            var reflection = signX != signY;
            var facts = new RackSourceTransformFactsV2(
                snapshot,
                NormalizePi(input.RotationRadians),
                signX,
                signY,
                signZ,
                input.ScaleX * input.ScaleY,
                reflection,
                maxScale - minScale <= tolerance.Scale,
                Math.Abs(absX - 1.0) <= tolerance.Scale
                    && Math.Abs(absY - 1.0) <= tolerance.Scale
                    && Math.Abs(absZ - 1.0) <= tolerance.Scale,
                signX == RackScaleSign.Negative && signY == RackScaleSign.Negative,
                signZ == RackScaleSign.Negative,
                Math.Abs(input.NormalX) <= tolerance.Normal
                    && Math.Abs(input.NormalY) <= tolerance.Normal
                    && Math.Abs(input.NormalZ - 1.0) <= tolerance.Normal,
                tolerance);
            return RackSourceTransformFactsResult.Success(facts);
        }

        private static RackScaleSign Sign(double value, double tolerance)
        {
            if (Math.Abs(value) <= tolerance) return RackScaleSign.ZeroWithinTolerance;
            return value > tolerance ? RackScaleSign.Positive : RackScaleSign.Negative;
        }

        private static double NormalizePi(double angle)
        {
            var normalized = angle % (2.0 * Math.PI);
            if (normalized <= -Math.PI) normalized += 2.0 * Math.PI;
            if (normalized > Math.PI) normalized -= 2.0 * Math.PI;
            return normalized;
        }

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
