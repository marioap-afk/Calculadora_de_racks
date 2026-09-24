using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;

namespace RackCad.Plugin.Views
{
    /// <summary>Concrete I-55 compositions. They invoke existing builders and Foundation ports; no geometry lives here.</summary>
    internal static class RackViewBatchProducts
    {
        internal static RackViewBatchProductSession<SelectivePalletDesignDocument, SelectiveRackSystem, HeaderRunPlan> SelectiveNew(
            Document document, SelectiveRackSystem system, SelectivePalletDesign design, string rackId, string rackName)
        {
            var catalog = LateralHeaderDrawService.LoadCatalog();
            var facts = new SelectiveViewAvailabilityFacts(SelectiveDepthLayout.Count(system),
                new SelectiveLateralBuilder().Cortes(system, catalog).Select(c => c.PostIndex));
            var baseName = string.IsNullOrWhiteSpace(rackName) ? null : rackName.Trim();
            var authored = SelectivePalletDesignDocument.From(design, rackId, rackName);
            var port = RackViewPreparationPorts.Selective<SelectiveRackSystem, HeaderRunPlan>(
                (resolved, address) => SelectivePlan(resolved, address, catalog, rackName),
                address => RackViewAvailability.Evaluate(Decoded(RackSystemKind.SelectiveRack, address), facts).Status
                    == RackViewAvailabilityStatus.Available,
                RackBlockRequirementExtractors.HeaderRun);
            var preparer = new RackProductPreparer<SelectivePalletDesignDocument, SelectiveRackSystem, HeaderRunPlan>(
                Cached(RackResolvePorts.Selective<SelectivePalletDesignDocument, SelectiveRackSystem>(_ => system)), port,
                (resolved, address) => SelectiveViewFrameAdapter.Resolve(resolved, catalog, address),
                (resolved, address) => address.Kind == DimensionViewKind.Planta
                    ? RackViewBaseName.SelectivePlanta(resolved, rackName)
                    : address.Kind == DimensionViewKind.Lateral
                        ? RackViewBaseName.LinkedLateral(baseName, address.Variant.Index)
                        : RackViewBaseName.LinkedSelectiveFrontal(baseName, address.Variant.Index,
                            SelectiveDepthLayout.Count(resolved)) ?? RackViewBaseName.SelectiveFrontal(resolved, rackName));
            return new RackViewBatchProductSession<SelectivePalletDesignDocument, SelectiveRackSystem, HeaderRunPlan>(
                RackSystemKind.SelectiveRack, facts, preparer, null, null, null, authored, rackId, rackName,
                new SelectivePalletDesignStore().Serialize(authored),
                product => RackViewPlacement.PlaceSelective(document, product, regen: false));
        }

        internal static RackViewBatchProductSession<DynamicRackDesign, DynamicRackSystem, HeaderRunPlan> Dynamic(
            Document document, DynamicRackSystem system, DynamicRackDesign design, string rackId, string rackName,
            RackEmbedDocument source, RackProject innerSource, RackAuthoredInput comparison)
        {
            var catalog = LateralHeaderDrawService.LoadCatalog();
            var cuts = new DynamicSystemLateralBuilder().Cortes(system, catalog);
            var facts = new DynamicViewAvailabilityFacts(cuts.Select(c => c.PostIndex));
            var baseName = string.IsNullOrWhiteSpace(rackName) ? null : rackName.Trim();
            var port = RackViewPreparationPorts.Dynamic<DynamicRackSystem, HeaderRunPlan>(
                (resolved, address) => DynamicPlan(resolved, address, catalog),
                address => RackViewAvailability.Evaluate(Decoded(RackSystemKind.PalletFlow, address), facts).Status
                    == RackViewAvailabilityStatus.Available,
                RackBlockRequirementExtractors.HeaderRun);
            var preparer = new RackProductPreparer<DynamicRackDesign, DynamicRackSystem, HeaderRunPlan>(
                Cached(RackResolvePorts.Dynamic<DynamicRackDesign, DynamicRackSystem>(_ => system)), port,
                (resolved, address) => DynamicViewFrameAdapter.Resolve(resolved, catalog, address),
                (resolved, address) => DynamicName(resolved, address, rackName, baseName));
            return new RackViewBatchProductSession<DynamicRackDesign, DynamicRackSystem, HeaderRunPlan>(
                RackSystemKind.PalletFlow, facts, preparer, RackAuthoredComparatorPorts.Dynamic(), comparison,
                source, design, rackId, rackName, Project(RackProject.ForDynamic(design), innerSource),
                product => RackViewPlacement.PlaceDynamic(document, product, regen: false));
        }

        internal static RackViewBatchProductSession<PushBackDesign, PushBackSystem, HeaderRunPlan> PushBack(
            Document document, PushBackSystem system, PushBackDesign design, string rackId, string rackName,
            RackEmbedDocument source, RackProject innerSource, RackAuthoredInput comparison)
        {
            var catalog = LateralHeaderDrawService.LoadCatalog();
            var facts = new PushBackViewAvailabilityFacts(
                new PushBackSystemLateralBuilder().Cortes(system, catalog).Select(c => c.PostIndex), system.IsComposite);
            var baseName = string.IsNullOrWhiteSpace(rackName) ? null : rackName.Trim();
            var port = RackViewPreparationPorts.PushBack<PushBackSystem, HeaderRunPlan>(
                (resolved, address) => PushBackPlan(resolved, address, catalog),
                address => RackViewAvailability.Evaluate(Decoded(RackSystemKind.PushBack, address), facts).Status
                    == RackViewAvailabilityStatus.Available,
                RackBlockRequirementExtractors.HeaderRun);
            var preparer = new RackProductPreparer<PushBackDesign, PushBackSystem, HeaderRunPlan>(
                Cached(RackResolvePorts.PushBack<PushBackDesign, PushBackSystem>(_ => system)), port,
                (resolved, address) => PushBackViewFrameAdapter.Resolve(resolved, catalog, address),
                (resolved, address) => PushBackName(resolved, address, rackName, baseName));
            return new RackViewBatchProductSession<PushBackDesign, PushBackSystem, HeaderRunPlan>(
                RackSystemKind.PushBack, facts, preparer, RackAuthoredComparatorPorts.PushBack(), comparison,
                source, design, rackId, rackName, Project(RackProject.ForPushBack(design), innerSource),
                product => RackViewPlacement.PlacePushBack(document, product, regen: false));
        }

        internal static RackViewBatchProductSession<RackFrameConfiguration, RackFrameConfiguration, HeaderRunPlan> Header(
            Document document, RackFrameConfiguration configuration, string rackId, string rackName,
            RackEmbedDocument source, RackProject innerSource, RackAuthoredInput comparison)
        {
            var catalog = LateralHeaderDrawService.LoadCatalog();
            var facts = new CabeceraViewAvailabilityFacts();
            var port = RackViewPreparationPorts.Cabecera<RackFrameConfiguration, HeaderRunPlan>(
                (resolved, address) => HeaderPlan(resolved, address, catalog),
                address => RackViewAvailability.Evaluate(Decoded(RackSystemKind.Selective, address), facts).Status
                    == RackViewAvailabilityStatus.Available,
                RackBlockRequirementExtractors.HeaderRun);
            var preparer = new RackProductPreparer<RackFrameConfiguration, RackFrameConfiguration, HeaderRunPlan>(
                Cached(RackResolvePorts.Cabecera<RackFrameConfiguration, RackFrameConfiguration>(_ => configuration)), port,
                CabeceraViewFrameAdapter.Resolve,
                (resolved, address) => address.Kind == DimensionViewKind.Planta
                    ? RackViewBaseName.CabeceraPlanta(rackName)
                    : RackViewBaseName.CabeceraLateral(catalog, resolved, rackName));
            return new RackViewBatchProductSession<RackFrameConfiguration, RackFrameConfiguration, HeaderRunPlan>(
                RackSystemKind.Selective, facts, preparer, RackAuthoredComparatorPorts.Cabecera(), comparison,
                source, configuration, rackId, rackName, Project(RackProject.ForSelective(configuration), innerSource),
                product => RackViewPlacement.PlaceHeader(document, product, regen: false));
        }

        internal static RackViewBatchProductSession<CantileverLineDesign, CantileverLineAssembly, CantileverViewPlan> Cantilever(
            Document document, CantileverLineAssembly line, CantileverLineDesign design,
            StructuralSectionGeometryFactory geometry, string rackId, string rackName,
            RackEmbedDocument source, RackProject innerSource, RackAuthoredInput comparison)
        {
            var facts = new CantileverViewAvailabilityFacts(line.Stations.Count);
            var port = RackViewPreparationPorts.Cantilever<CantileverLineAssembly, CantileverViewPlan>(
                (resolved, address) => CantileverPlan(resolved, design, geometry, address),
                address => RackViewAvailability.Evaluate(Decoded(RackSystemKind.Cantilever, address), facts).Status
                    == RackViewAvailabilityStatus.Available,
                RackBlockRequirementExtractors.Cantilever);
            var preparer = new RackProductPreparer<CantileverLineDesign, CantileverLineAssembly, CantileverViewPlan>(
                Cached(RackResolvePorts.Cantilever<CantileverLineDesign, CantileverLineAssembly>(_ => line)), port,
                (resolved, address) =>
                {
                    var plan = CantileverPlan(resolved, design, geometry, address);
                    return CantileverViewFrameAdapter.Resolve(resolved, plan, address);
                },
                (resolved, address) => RackViewBaseName.CantileverGenerated(
                    CantileverKind(address), address.Variant.Kind == RackViewVariantKind.Station ? address.Variant.Index : -1,
                    rackName));
            return new RackViewBatchProductSession<CantileverLineDesign, CantileverLineAssembly, CantileverViewPlan>(
                RackSystemKind.Cantilever, facts, preparer, RackAuthoredComparatorPorts.Cantilever(), comparison,
                source, design, rackId, rackName, Project(RackProject.ForCantilever(design), innerSource),
                product => RackViewPlacement.PlaceCantilever(document, product, regen: false));
        }

        private static HeaderRunPlan DynamicPlan(DynamicRackSystem system, RackViewAddress address, RackCatalog catalog)
        {
            if (address.Kind == DimensionViewKind.Planta) return new DynamicSystemPlantaBuilder().BuildPlan(system, catalog);
            if (address.Kind == DimensionViewKind.Frontal)
                return new DynamicSystemFrontalBuilder().BuildPlan(system, catalog,
                    address.Variant.FlowEnd == RackFlowEnd.Entrance ? DynamicRackEnd.Entrance : DynamicRackEnd.Exit);
            return new DynamicSystemLateralBuilder().Cortes(system, catalog)
                .First(c => c.PostIndex == address.Variant.Index).Plan;
        }

        private static HeaderRunPlan SelectivePlan(
            SelectiveRackSystem system, RackViewAddress address, RackCatalog catalog, string rackName)
        {
            if (address.Kind == DimensionViewKind.Planta) return new SelectivePlantaBuilder().BuildPlan(system, catalog);
            if (address.Kind == DimensionViewKind.Frontal)
                return new SelectiveFrontalBuilder().BuildPlan(
                    SelectiveDepthLayout.FondoSystemView(system, address.Variant.Index), catalog);
            var cut = new SelectiveLateralBuilder().Cortes(system, catalog)
                .First(c => c.PostIndex == address.Variant.Index);
            var parameters = LateralHeaderParametersFactory.FromConfiguration(cut.Cabecera);
            var layout = new LateralHeaderLayoutBuilder().Build(cut.Cabecera, parameters, catalog);
            return HeaderInstanceGrouper.Group(layout.Instances.Concat(cut.Largueros).ToList(), rackName);
        }

        private static HeaderRunPlan PushBackPlan(PushBackSystem system, RackViewAddress address, RackCatalog catalog)
        {
            if (address.Kind == DimensionViewKind.Planta) return new PushBackSystemPlantaBuilder().BuildPlan(system, catalog);
            if (address.Kind == DimensionViewKind.Frontal)
                return new PushBackSystemFrontalBuilder().BuildPlan(system, catalog,
                    address.Variant.PushBackEnd == RackPushBackEnd.Posterior
                        ? PushBackFrontalEnd.Posterior : PushBackFrontalEnd.EntradaSalida,
                    address.Variant.PushBackSide == RackPushBackSide.B ? PushBackSide.B : PushBackSide.A);
            return new PushBackSystemLateralBuilder().Build(system, catalog, address.Variant.Index);
        }

        private static HeaderRunPlan HeaderPlan(RackFrameConfiguration configuration, RackViewAddress address, RackCatalog catalog)
        {
            if (address.Kind == DimensionViewKind.Planta)
                return HeaderInstanceGrouper.Group(new PlantaHeaderLayoutBuilder().Build(configuration, catalog),
                    RackViewBaseName.CabeceraPlanta(configuration.Name));
            var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);
            var layout = new LateralHeaderLayoutBuilder().Build(configuration, parameters, catalog);
            return HeaderInstanceGrouper.Group(layout.Instances,
                RackViewBaseName.CabeceraLateral(catalog, configuration, configuration.Name));
        }

        private static CantileverViewPlan CantileverPlan(CantileverLineAssembly line, CantileverLineDesign design,
            StructuralSectionGeometryFactory geometry, RackViewAddress address)
            => CantileverViewPlanBuilder.Build(line, CantileverKind(address), geometry,
                address.Variant.Kind == RackViewVariantKind.Station ? address.Variant.Index : 0,
                design.PlantaVisibility);

        private static CantileverViewKind CantileverKind(RackViewAddress address)
            => address.Kind == DimensionViewKind.Planta ? CantileverViewKind.Planta
                : address.Kind == DimensionViewKind.Lateral ? CantileverViewKind.Lateral
                : CantileverViewKind.Frontal;

        private static string DynamicName(DynamicRackSystem system, RackViewAddress address, string name, string baseName)
            => address.Kind == DimensionViewKind.Planta ? RackViewBaseName.DynamicPlanta(system, name)
                : address.Kind == DimensionViewKind.Frontal ? RackViewBaseName.DynamicFrontal(system, name, address.Variant.FlowEnd)
                : RackViewBaseName.LinkedLateral(baseName, address.Variant.Index)
                    ?? RackViewBaseName.DynamicLateral(system, name);

        private static string PushBackName(PushBackSystem system, RackViewAddress address, string name, string baseName)
            => address.Kind == DimensionViewKind.Planta ? RackViewBaseName.PushBackPlanta(system, name)
                : address.Kind == DimensionViewKind.Frontal
                    ? RackViewBaseName.PushBackFrontal(system, name, address.Variant.PushBackEnd, address.Variant.PushBackSide)
                    : RackViewBaseName.LinkedLateral(baseName, address.Variant.Index)
                        ?? RackViewBaseName.PushBackLateral(system, name, address.Variant.Index);

        private static DecodedRackView Decoded(RackSystemKind kind, RackViewAddress address)
        {
            var syntax = RackViewCodec.Encode(kind, address);
            return RackViewCodec.Decode(syntax.Kind, syntax.View, syntax.Section);
        }

        private static string Project(RackProject project, RackProject source)
            => new RackProjectStore().Serialize(project.WithSourceMetadataFrom(source));

        private static IRackResolvePort<TInput, TResolved> Cached<TInput, TResolved>(
            IRackResolvePort<TInput, TResolved> inner) => new CachedResolve<TInput, TResolved>(inner);

        private sealed class CachedResolve<TInput, TResolved> : IRackResolvePort<TInput, TResolved>
        {
            private readonly IRackResolvePort<TInput, TResolved> inner;
            private RackResolveResult<TResolved> result;
            private bool hasResult;
            internal CachedResolve(IRackResolvePort<TInput, TResolved> inner) => this.inner = inner;
            public string Kind => inner.Kind;
            public RackResolveResult<TResolved> Resolve(TInput input)
            {
                if (!hasResult) { result = inner.Resolve(input); hasResult = true; }
                return result;
            }
        }
    }
}
