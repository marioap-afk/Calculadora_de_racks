using System;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Shared
{
    public enum RackPhysicalAxis
    {
        Run,
        Depth,
        Height
    }

    public enum RackAxisOrientation
    {
        Positive = 1,
        Negative = -1
    }

    public readonly struct RackViewAxisMap
    {
        public RackViewAxisMap(
            RackPhysicalAxis localX,
            RackPhysicalAxis localY,
            RackAxisOrientation localXOrientation = RackAxisOrientation.Positive,
            RackAxisOrientation localYOrientation = RackAxisOrientation.Positive)
        {
            LocalX = localX;
            LocalY = localY;
            LocalXOrientation = localXOrientation;
            LocalYOrientation = localYOrientation;
        }

        public RackPhysicalAxis LocalX { get; }
        public RackPhysicalAxis LocalY { get; }
        public RackAxisOrientation LocalXOrientation { get; }
        public RackAxisOrientation LocalYOrientation { get; }

        public static RackViewAxisMap RunHeight => new RackViewAxisMap(RackPhysicalAxis.Run, RackPhysicalAxis.Height);
        public static RackViewAxisMap DepthRun => new RackViewAxisMap(RackPhysicalAxis.Depth, RackPhysicalAxis.Run);
        public static RackViewAxisMap DepthHeight => new RackViewAxisMap(RackPhysicalAxis.Depth, RackPhysicalAxis.Height);
        public static RackViewAxisMap RunDepth => new RackViewAxisMap(RackPhysicalAxis.Run, RackPhysicalAxis.Depth);
    }

    public readonly struct RackPhysicalPoint
    {
        public RackPhysicalPoint(double run, double depth, double height)
        {
            Run = run;
            Depth = depth;
            Height = height;
        }

        public double Run { get; }
        public double Depth { get; }
        public double Height { get; }
        public static RackPhysicalPoint Zero => new RackPhysicalPoint(0.0, 0.0, 0.0);
    }

    public readonly struct RackPhysicalVector
    {
        public RackPhysicalVector(double run, double depth, double height)
        {
            Run = run;
            Depth = depth;
            Height = height;
        }

        public double Run { get; }
        public double Depth { get; }
        public double Height { get; }
        public static RackPhysicalVector Zero => new RackPhysicalVector(0.0, 0.0, 0.0);
    }

    public enum RackFrameEndpointConvention
    {
        PostAxisClosedInterval,
        PhysicalDepthFaces,
        PhysicalRunAxes,
        StorageDepthFaces,
        StructuralCoverage,
        LayoutFaces,
        PhysicalStationSpan,
        BedLengthFaces
    }

    public enum RackViewFrameFailure
    {
        None,
        MissingSource,
        UnsupportedAddress,
        VariantUnavailable,
        EmptyGeometry,
        RepeatedAxis,
        InvalidOrientation,
        NonFinite,
        InvertedSpan
    }

    /// <summary>
    /// Physical frame of a semantic view. K describes physical extent on local X. Drawn bounds are optional output
    /// facts and never participate in K, origin or center.
    /// </summary>
    public readonly struct RackViewFrame
    {
        private RackViewFrame(
            RackViewAxisMap axisMap,
            RackPhysicalPoint physicalOrigin,
            double kMin,
            double kMax,
            RackFrameEndpointConvention endpointConvention,
            RackPhysicalVector variantOffset,
            Bounds2D? drawnBounds)
        {
            AxisMap = axisMap;
            PhysicalOrigin = physicalOrigin;
            KMin = kMin;
            KMax = kMax;
            EndpointConvention = endpointConvention;
            VariantOffset = variantOffset;
            DrawnBounds = drawnBounds;
        }

        public RackViewAxisMap AxisMap { get; }
        public RackPhysicalPoint PhysicalOrigin { get; }
        public double KMin { get; }
        public double KMax { get; }
        public double Center => (KMin + KMax) / 2.0;
        public RackFrameEndpointConvention EndpointConvention { get; }
        public RackPhysicalVector VariantOffset { get; }
        public Bounds2D? DrawnBounds { get; }

        public static RackViewFrameResult TryCreate(
            RackViewAxisMap axisMap,
            RackPhysicalPoint physicalOrigin,
            double kMin,
            double kMax,
            RackFrameEndpointConvention endpointConvention,
            RackPhysicalVector variantOffset,
            Bounds2D? drawnBounds = null)
        {
            if (axisMap.LocalX == axisMap.LocalY)
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.RepeatedAxis);
            }


            if (axisMap.LocalXOrientation != RackAxisOrientation.Positive
                && axisMap.LocalXOrientation != RackAxisOrientation.Negative
                || axisMap.LocalYOrientation != RackAxisOrientation.Positive
                && axisMap.LocalYOrientation != RackAxisOrientation.Negative)
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.InvalidOrientation);
            }

            if (!Finite(physicalOrigin.Run) || !Finite(physicalOrigin.Depth) || !Finite(physicalOrigin.Height)
                || !Finite(variantOffset.Run) || !Finite(variantOffset.Depth) || !Finite(variantOffset.Height)
                || !Finite(kMin) || !Finite(kMax)
                || drawnBounds.HasValue && (!Finite(drawnBounds.Value.MinX) || !Finite(drawnBounds.Value.MinY)
                    || !Finite(drawnBounds.Value.MaxX) || !Finite(drawnBounds.Value.MaxY)))
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.NonFinite);
            }

            if (kMax < kMin)
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.InvertedSpan);
            }


            if (kMax == kMin)
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.EmptyGeometry);
            }

            return RackViewFrameResult.Available(new RackViewFrame(
                axisMap, physicalOrigin, kMin, kMax, endpointConvention, variantOffset, drawnBounds));
        }

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }

    public readonly struct RackViewFrameResult
    {
        private RackViewFrameResult(bool isAvailable, RackViewFrame frame, RackViewFrameFailure failure)
        {
            IsAvailable = isAvailable;
            Frame = frame;
            Failure = failure;
        }

        public bool IsAvailable { get; }
        public RackViewFrame Frame { get; }
        public RackViewFrameFailure Failure { get; }

        public static RackViewFrameResult Available(RackViewFrame frame) =>
            new RackViewFrameResult(true, frame, RackViewFrameFailure.None);

        public static RackViewFrameResult Unavailable(RackViewFrameFailure failure)
        {
            if (failure == RackViewFrameFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new RackViewFrameResult(false, default, failure);
        }
    }
}
