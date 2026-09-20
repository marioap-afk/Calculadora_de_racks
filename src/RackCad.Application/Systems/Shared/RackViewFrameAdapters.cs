using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Shared
{
    public static class SelectiveViewFrameAdapter
    {
        public static RackViewFrameResult Resolve(
            SelectiveRackSystem system, RackCatalog catalog, RackViewAddress address)
        {
            if (system == null || catalog == null)
            {
                return Missing();
            }

            if (address.Kind == DimensionViewKind.Frontal && address.Variant.Kind == RackViewVariantKind.Fondo)
            {
                var fondo = address.Variant.Index;
                var offsets = SelectiveDepthLayout.Offsets(system);
                if (fondo >= offsets.Count) return Variant();
                var posts = SelectivePostGeometry.Compute(SelectiveDepthLayout.FondoSystemView(system, fondo), catalog).PostXs;
                if (posts.Count == 0) return Empty();
                var run0 = posts[0];
                var depth = offsets[fondo];
                return RackViewFrame.TryCreate(
                    RackViewAxisMap.RunHeight, new RackPhysicalPoint(run0, depth, 0.0),
                    0.0, posts[posts.Count - 1] - run0,
                    RackFrameEndpointConvention.PostAxisClosedInterval,
                    new RackPhysicalVector(0.0, depth, 0.0));
            }

            if (address.Kind == DimensionViewKind.Planta && address.Variant.Kind == RackViewVariantKind.Whole)
            {
                var depth = SelectiveDepthLayout.TotalFondoDepth(system);
                return depth > 0.0
                    ? RackViewFrame.TryCreate(RackViewAxisMap.DepthRun, RackPhysicalPoint.Zero, 0.0, depth,
                        RackFrameEndpointConvention.PhysicalDepthFaces, RackPhysicalVector.Zero)
                    : Empty();
            }

            if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Post)
            {
                var cut = new SelectiveLateralBuilder().Cortes(system, catalog)
                    .FirstOrDefault(item => item.PostIndex == address.Variant.Index);
                if (cut == null) return Variant();
                var depth = SelectiveDepthLayout.TotalFondoDepth(system);
                return depth > 0.0
                    ? RackViewFrame.TryCreate(RackViewAxisMap.DepthHeight,
                        new RackPhysicalPoint(cut.X, 0.0, 0.0), 0.0, depth,
                        RackFrameEndpointConvention.PhysicalDepthFaces,
                        new RackPhysicalVector(cut.X, 0.0, 0.0))
                    : Empty();
            }

            return Unsupported();
        }

        private static RackViewFrameResult Missing() => RackViewFrameResult.Unavailable(RackViewFrameFailure.MissingSource);
        private static RackViewFrameResult Unsupported() => RackViewFrameResult.Unavailable(RackViewFrameFailure.UnsupportedAddress);
        private static RackViewFrameResult Variant() => RackViewFrameResult.Unavailable(RackViewFrameFailure.VariantUnavailable);
        private static RackViewFrameResult Empty() => RackViewFrameResult.Unavailable(RackViewFrameFailure.EmptyGeometry);
    }

    public static class DynamicViewFrameAdapter
    {
        public static RackViewFrameResult Resolve(
            DynamicRackSystem system, RackCatalog catalog, RackViewAddress address)
        {
            if (system == null || catalog == null) return Unavailable(RackViewFrameFailure.MissingSource);

            if (address.Kind == DimensionViewKind.Frontal && address.Variant.Kind == RackViewVariantKind.FlowEnd)
            {
                var width = DynamicFrontGeometry.Compute(system, catalog).TotalWidth;
                if (width <= 0.0) return Unavailable(RackViewFrameFailure.EmptyGeometry);
                var depth = address.Variant.FlowEnd == RackFlowEnd.Entrance ? system.TotalLength : 0.0;
                return RackViewFrame.TryCreate(RackViewAxisMap.RunHeight,
                    new RackPhysicalPoint(0.0, depth, 0.0), 0.0, width,
                    RackFrameEndpointConvention.PhysicalRunAxes,
                    new RackPhysicalVector(0.0, depth, 0.0));
            }

            if (address.Kind == DimensionViewKind.Planta && address.Variant.Kind == RackViewVariantKind.Whole)
            {
                return system.TotalLength > 0.0
                    ? RackViewFrame.TryCreate(RackViewAxisMap.DepthRun, RackPhysicalPoint.Zero, 0.0, system.TotalLength,
                        RackFrameEndpointConvention.StorageDepthFaces, RackPhysicalVector.Zero)
                    : Unavailable(RackViewFrameFailure.EmptyGeometry);
            }

            if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Post)
            {
                var cut = new DynamicSystemLateralBuilder().Cortes(system, catalog)
                    .FirstOrDefault(item => item.PostIndex == address.Variant.Index);
                if (cut == null) return Unavailable(RackViewFrameFailure.VariantUnavailable);
                var modules = DynamicDepthGeometry.ModulesInCoverage(
                    system, DynamicDepthGeometry.CoverageAtPost(system, cut.PostIndex));
                if (modules.Count == 0) return Unavailable(RackViewFrameFailure.EmptyGeometry);
                var min = modules.Min(item => item.StartX);
                var max = modules.Max(item => item.EndX);
                return RackViewFrame.TryCreate(RackViewAxisMap.DepthHeight,
                    new RackPhysicalPoint(cut.PostX, min, 0.0), 0.0, max - min,
                    RackFrameEndpointConvention.StructuralCoverage,
                    new RackPhysicalVector(cut.PostX, min, 0.0));
            }

            return Unavailable(RackViewFrameFailure.UnsupportedAddress);
        }

        private static RackViewFrameResult Unavailable(RackViewFrameFailure failure) =>
            RackViewFrameResult.Unavailable(failure);
    }

    public static class PushBackViewFrameAdapter
    {
        public static RackViewFrameResult Resolve(
            PushBackSystem system, RackCatalog catalog, RackViewAddress address)
        {
            if (system?.Structure == null || catalog == null)
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.MissingSource);
            }

            if (address.Kind == DimensionViewKind.Frontal && address.Variant.Kind == RackViewVariantKind.PushBackCut)
            {
                var width = DynamicFrontGeometry.Compute(system.Structure, catalog).TotalWidth;
                if (width <= 0.0) return Empty();
                var side = address.Variant.PushBackSide == RackPushBackSide.A ? PushBackSide.A : PushBackSide.B;
                double depth;
                if (system.Composite == null)
                {
                    if (side == PushBackSide.B) return Variant();
                    depth = address.Variant.PushBackEnd == RackPushBackEnd.EntradaSalida ? 0.0 : system.TotalLength;
                }
                else
                {
                    var sideView = system.Composite.Of(side);
                    if (sideView == null || !sideView.IsPresent) return Variant();
                    depth = address.Variant.PushBackEnd == RackPushBackEnd.EntradaSalida
                        ? sideView.OuterX
                        : sideView.InnerX;
                }

                return RackViewFrame.TryCreate(RackViewAxisMap.RunHeight,
                    new RackPhysicalPoint(0.0, depth, 0.0), 0.0, width,
                    RackFrameEndpointConvention.PhysicalRunAxes,
                    new RackPhysicalVector(0.0, depth, 0.0));
            }

            if (address.Kind == DimensionViewKind.Planta && address.Variant.Kind == RackViewVariantKind.Whole)
            {
                return system.TotalLength > 0.0
                    ? RackViewFrame.TryCreate(RackViewAxisMap.DepthRun, RackPhysicalPoint.Zero, 0.0, system.TotalLength,
                        RackFrameEndpointConvention.PhysicalDepthFaces, RackPhysicalVector.Zero)
                    : Empty();
            }

            if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Post)
            {
                var cut = new PushBackSystemLateralBuilder().Cortes(system, catalog)
                    .FirstOrDefault(item => item.PostIndex == address.Variant.Index);
                if (cut == null) return Variant();
                var modules = DynamicDepthGeometry.ModulesInCoverage(
                    system.Structure, DynamicDepthGeometry.CoverageAtPost(system.Structure, cut.PostIndex));
                if (modules.Count == 0) return Empty();
                var min = modules.Min(item => item.StartX);
                var max = modules.Max(item => item.EndX);
                return RackViewFrame.TryCreate(RackViewAxisMap.DepthHeight,
                    new RackPhysicalPoint(cut.PostX, min, 0.0), 0.0, max - min,
                    RackFrameEndpointConvention.StructuralCoverage,
                    new RackPhysicalVector(cut.PostX, min, 0.0));
            }

            return RackViewFrameResult.Unavailable(RackViewFrameFailure.UnsupportedAddress);
        }

        private static RackViewFrameResult Variant() => RackViewFrameResult.Unavailable(RackViewFrameFailure.VariantUnavailable);
        private static RackViewFrameResult Empty() => RackViewFrameResult.Unavailable(RackViewFrameFailure.EmptyGeometry);
    }

    public static class CabeceraViewFrameAdapter
    {
        public static RackViewFrameResult Resolve(RackFrameConfiguration configuration, RackViewAddress address)
        {
            if (configuration == null) return RackViewFrameResult.Unavailable(RackViewFrameFailure.MissingSource);
            if (address.Variant.Kind != RackViewVariantKind.Whole
                || address.Kind != DimensionViewKind.Planta && address.Kind != DimensionViewKind.Lateral)
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.UnsupportedAddress);
            }

            if (configuration.Depth <= 0.0) return RackViewFrameResult.Unavailable(RackViewFrameFailure.EmptyGeometry);
            var axes = address.Kind == DimensionViewKind.Planta ? RackViewAxisMap.DepthRun : RackViewAxisMap.DepthHeight;
            return RackViewFrame.TryCreate(axes, RackPhysicalPoint.Zero, 0.0, configuration.Depth,
                RackFrameEndpointConvention.LayoutFaces, RackPhysicalVector.Zero);
        }
    }

    public static class FlowBedViewFrameAdapter
    {
        public static RackViewFrameResult Resolve(FlowBedConfiguration configuration, RackViewAddress address)
        {
            if (configuration == null) return RackViewFrameResult.Unavailable(RackViewFrameFailure.MissingSource);
            if (address.Kind != DimensionViewKind.Lateral || address.Variant.Kind != RackViewVariantKind.Whole)
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.UnsupportedAddress);
            }

            return configuration.LaneDepth > 0.0
                ? RackViewFrame.TryCreate(RackViewAxisMap.DepthHeight, RackPhysicalPoint.Zero, 0.0,
                    configuration.LaneDepth, RackFrameEndpointConvention.BedLengthFaces, RackPhysicalVector.Zero)
                : RackViewFrameResult.Unavailable(RackViewFrameFailure.EmptyGeometry);
        }
    }

    public static class CantileverViewFrameAdapter
    {
        public static RackViewFrameResult Resolve(
            CantileverLineAssembly line, CantileverViewPlan plan, RackViewAddress address)
        {
            if (line == null || plan == null) return RackViewFrameResult.Unavailable(RackViewFrameFailure.MissingSource);
            if (line.IsBlocked || plan.IsEmpty) return RackViewFrameResult.Unavailable(RackViewFrameFailure.EmptyGeometry);
            var drawn = plan.Bounds;

            if (address.Variant.Kind == RackViewVariantKind.Whole
                && (address.Kind == DimensionViewKind.Frontal || address.Kind == DimensionViewKind.Planta))
            {
                var expected = address.Kind == DimensionViewKind.Frontal
                    ? CantileverViewKind.Frontal : CantileverViewKind.Planta;
                var envelope = line.Envelope();
                if (plan.View != expected || plan.StationIndex != -1 || !envelope.HasValue)
                {
                    return RackViewFrameResult.Unavailable(RackViewFrameFailure.VariantUnavailable);
                }

                var axes = address.Kind == DimensionViewKind.Frontal ? RackViewAxisMap.RunHeight : RackViewAxisMap.RunDepth;
                return RackViewFrame.TryCreate(axes, RackPhysicalPoint.Zero,
                    envelope.Value.MinX, envelope.Value.MaxX,
                    RackFrameEndpointConvention.PhysicalStationSpan, RackPhysicalVector.Zero, drawn);
            }

            if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Station)
            {
                var placement = line.Stations.FirstOrDefault(item => item.Index == address.Variant.Index);
                if (placement == null || plan.View != CantileverViewKind.Lateral
                    || plan.StationIndex != address.Variant.Index || !placement.Station.Envelope.HasValue)
                {
                    return RackViewFrameResult.Unavailable(RackViewFrameFailure.VariantUnavailable);
                }

                var envelope = placement.Station.Envelope.Value;
                return RackViewFrame.TryCreate(RackViewAxisMap.DepthHeight,
                    new RackPhysicalPoint(placement.OriginX, 0.0, 0.0),
                    envelope.MinY, envelope.MaxY,
                    RackFrameEndpointConvention.PhysicalStationSpan,
                    new RackPhysicalVector(placement.OriginX, 0.0, 0.0), drawn);
            }

            return RackViewFrameResult.Unavailable(RackViewFrameFailure.UnsupportedAddress);
        }
    }
}
