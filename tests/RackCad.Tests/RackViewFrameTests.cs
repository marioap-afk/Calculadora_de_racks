using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    public sealed class RackViewFrameTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Frame_DerivesExactCenter_AndKeepsDrawnBoundsSeparate()
        {
            var result = RackViewFrame.TryCreate(
                new RackViewAxisMap(RackPhysicalAxis.Run, RackPhysicalAxis.Height),
                new RackPhysicalPoint(10.0, 20.0, 30.0),
                -12.0,
                36.0,
                RackFrameEndpointConvention.PhysicalStationSpan,
                new RackPhysicalVector(4.0, 5.0, 6.0),
                new Bounds2D(-99.0, -88.0, 77.0, 66.0));

            Assert.True(result.IsAvailable);
            Assert.Equal(12.0, result.Frame.Center);
            Assert.Equal(-12.0, result.Frame.KMin);
            Assert.Equal(36.0, result.Frame.KMax);
            Assert.Equal(RackAxisOrientation.Positive, result.Frame.AxisMap.LocalXOrientation);
            Assert.Equal(-99.0, result.Frame.DrawnBounds.Value.MinX);
            Assert.NotEqual(result.Frame.DrawnBounds.Value.MinX, result.Frame.KMin);
        }

        [Fact]
        public void Frame_FailsTyped_ForInvertedNonFiniteOrRepeatedAxes()
        {
            Assert.Equal(RackViewFrameFailure.InvertedSpan, RackViewFrame.TryCreate(
                RackViewAxisMap.RunHeight, RackPhysicalPoint.Zero, 5.0, 4.0,
                RackFrameEndpointConvention.PhysicalRunAxes, RackPhysicalVector.Zero).Failure);
            Assert.Equal(RackViewFrameFailure.NonFinite, RackViewFrame.TryCreate(
                RackViewAxisMap.RunHeight, RackPhysicalPoint.Zero, 0.0, double.NaN,
                RackFrameEndpointConvention.PhysicalRunAxes, RackPhysicalVector.Zero).Failure);
            Assert.Equal(RackViewFrameFailure.RepeatedAxis, RackViewFrame.TryCreate(
                new RackViewAxisMap(RackPhysicalAxis.Depth, RackPhysicalAxis.Depth),
                RackPhysicalPoint.Zero, 0.0, 10.0,
                RackFrameEndpointConvention.PhysicalDepthFaces, RackPhysicalVector.Zero).Failure);
            Assert.Equal(RackViewFrameFailure.EmptyGeometry, RackViewFrame.TryCreate(
                RackViewAxisMap.DepthHeight, RackPhysicalPoint.Zero, 4.0, 4.0,
                RackFrameEndpointConvention.PhysicalDepthFaces, RackPhysicalVector.Zero).Failure);
            Assert.Equal(RackViewFrameFailure.InvalidOrientation, RackViewFrame.TryCreate(
                new RackViewAxisMap(RackPhysicalAxis.Depth, RackPhysicalAxis.Height, (RackAxisOrientation)0),
                RackPhysicalPoint.Zero, 0.0, 4.0,
                RackFrameEndpointConvention.PhysicalDepthFaces, RackPhysicalVector.Zero).Failure);
        }

        [Fact]
        public void SixKindAdapters_ProjectExistingAuthorities_WithoutConsumerPolicy()
        {
            var selective = SelectiveSystem();
            var dynamic = DynamicSystem();
            var pushBack = new PushBackSystem { Structure = dynamic };
            var header = new RackFrameConfiguration { Depth = 48.0 };
            var bed = new FlowBedConfiguration { LaneDepth = 100.0 };
            var selectivePosts = SelectivePostGeometry.Compute(selective, Catalog).PostXs;
            var runWidth = DynamicFrontGeometry.Compute(dynamic, Catalog).TotalWidth;

            AssertFrame(SelectiveViewFrameAdapter.Resolve(selective, Catalog, RackViewAddress.Fondo(0)),
                RackPhysicalAxis.Run, RackPhysicalAxis.Height, 0.0,
                selectivePosts[selectivePosts.Count - 1] - selectivePosts[0]);
            AssertFrame(DynamicViewFrameAdapter.Resolve(dynamic, Catalog, RackViewAddress.FlowEnd(RackFlowEnd.Exit)),
                RackPhysicalAxis.Run, RackPhysicalAxis.Height, 0.0, runWidth);
            AssertFrame(PushBackViewFrameAdapter.Resolve(pushBack, Catalog,
                    RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A)),
                RackPhysicalAxis.Run, RackPhysicalAxis.Height, 0.0, runWidth);
            AssertFrame(CabeceraViewFrameAdapter.Resolve(header, RackViewAddress.Whole(DimensionViewKind.Planta)),
                RackPhysicalAxis.Depth, RackPhysicalAxis.Run, 0.0, 48.0);
            AssertFrame(FlowBedViewFrameAdapter.Resolve(bed, RackViewAddress.Whole(DimensionViewKind.Lateral)),
                RackPhysicalAxis.Depth, RackPhysicalAxis.Height, 0.0, 100.0);

            var (line, plan) = Cantilever();
            var cantilever = CantileverViewFrameAdapter.Resolve(
                line, plan, RackViewAddress.Whole(DimensionViewKind.Frontal));
            Assert.True(cantilever.IsAvailable);
            Assert.Equal(RackPhysicalAxis.Run, cantilever.Frame.AxisMap.LocalX);
            Assert.Equal(RackPhysicalAxis.Height, cantilever.Frame.AxisMap.LocalY);
            Assert.True(cantilever.Frame.DrawnBounds.HasValue);
            Assert.Equal(RackFrameEndpointConvention.PhysicalStationSpan, cantilever.Frame.EndpointConvention);
        }

        [Fact]
        public void AdaptersFailClosed_ForUnsupportedMissingAndUnavailableVariants()
        {
            Assert.Equal(RackViewFrameFailure.MissingSource,
                SelectiveViewFrameAdapter.Resolve(null, Catalog, RackViewAddress.Fondo(0)).Failure);
            Assert.Equal(RackViewFrameFailure.UnsupportedAddress,
                CabeceraViewFrameAdapter.Resolve(new RackFrameConfiguration { Depth = 48.0 },
                    RackViewAddress.Whole(DimensionViewKind.Frontal)).Failure);
            Assert.Equal(RackViewFrameFailure.VariantUnavailable,
                PushBackViewFrameAdapter.Resolve(new PushBackSystem { Structure = DynamicSystem() }, Catalog,
                    RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.B)).Failure);
            Assert.Equal(RackViewFrameFailure.VariantUnavailable,
                DynamicViewFrameAdapter.Resolve(DynamicSystem(), Catalog, RackViewAddress.Post(999)).Failure);
        }

        [Fact]
        public void IndexedSectionsUseRealPostAuthority_NotTheOrdinalAsDistance()
        {
            var selective = SelectiveSystem();
            var selectiveCut = new SelectiveLateralBuilder().Cortes(selective, Catalog).Single(item => item.PostIndex == 1);
            var selectiveFrame = SelectiveViewFrameAdapter.Resolve(selective, Catalog, RackViewAddress.Post(1));
            Assert.True(selectiveFrame.IsAvailable);
            Assert.Equal(selectiveCut.X, selectiveFrame.Frame.PhysicalOrigin.Run, 6);
            Assert.Equal(selectiveCut.X, selectiveFrame.Frame.VariantOffset.Run, 6);
            Assert.NotEqual(1.0, selectiveFrame.Frame.VariantOffset.Run);

            var dynamic = DynamicSystem();
            var dynamicCut = new DynamicSystemLateralBuilder().Cortes(dynamic, Catalog).Single(item => item.PostIndex == 1);
            var dynamicFrame = DynamicViewFrameAdapter.Resolve(dynamic, Catalog, RackViewAddress.Post(1));
            Assert.True(dynamicFrame.IsAvailable);
            Assert.Equal(dynamicCut.PostX, dynamicFrame.Frame.PhysicalOrigin.Run, 6);
            Assert.Equal(dynamicCut.PostX, dynamicFrame.Frame.VariantOffset.Run, 6);
            Assert.NotEqual(1.0, dynamicFrame.Frame.VariantOffset.Run);
        }

        [Fact]
        public void DynamicEndsKeepTheirDistinctPhysicalOrigins()
        {
            var system = DynamicSystem();
            var exit = DynamicViewFrameAdapter.Resolve(system, Catalog, RackViewAddress.FlowEnd(RackFlowEnd.Exit));
            var entrance = DynamicViewFrameAdapter.Resolve(system, Catalog, RackViewAddress.FlowEnd(RackFlowEnd.Entrance));

            Assert.True(exit.IsAvailable);
            Assert.True(entrance.IsAvailable);
            Assert.Equal(0.0, exit.Frame.PhysicalOrigin.Depth);
            Assert.Equal(system.TotalLength, entrance.Frame.PhysicalOrigin.Depth);
            Assert.Equal(exit.Frame.KMin, entrance.Frame.KMin);
            Assert.Equal(exit.Frame.KMax, entrance.Frame.KMax);
        }

        [Fact]
        public void CantileverPhysicalSpan_DoesNotComeFromProjectedBounds()
        {
            var (line, plan) = Cantilever();
            var result = CantileverViewFrameAdapter.Resolve(
                line, plan, RackViewAddress.Whole(DimensionViewKind.Frontal));

            Assert.True(result.IsAvailable);
            var envelope = line.Envelope().Value;
            Assert.Equal(envelope.MinX, result.Frame.KMin);
            Assert.Equal(envelope.MaxX, result.Frame.KMax);
            Assert.Equal(plan.Bounds, result.Frame.DrawnBounds.Value);
        }

        private static void AssertFrame(
            RackViewFrameResult result, RackPhysicalAxis x, RackPhysicalAxis y, double min, double max)
        {
            Assert.True(result.IsAvailable, result.Failure.ToString());
            Assert.Equal(x, result.Frame.AxisMap.LocalX);
            Assert.Equal(y, result.Frame.AxisMap.LocalY);
            Assert.Equal(min, result.Frame.KMin, 6);
            Assert.Equal(max, result.Frame.KMax, 6);
            Assert.Equal((min + max) / 2.0, result.Frame.Center, 6);
        }

        private static SelectiveRackSystem SelectiveSystem()
        {
            var system = new SelectiveRackSystem
            {
                Height = 240.0,
                PostId = TestCatalogIds.Profiles.Posts.Standard,
                PostPeralte = 3.0,
                PalletDepth = 48.0
            };
            system.Bays.Add(new SelectiveBay { BeamLength = 100.0, Height = 240.0 });
            return system;
        }

        private static DynamicRackSystem DynamicSystem()
        {
            var design = DynamicFrontGeometryTests.Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3 });
            return new DynamicRackSystemResolver(Catalog).Resolve(design).System;
        }

        private static (CantileverLineAssembly Line, CantileverViewPlan Plan) Cantilever()
        {
            var catalog = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();
            var factory = new StructuralSectionGeometryFactory(catalog);
            var topology = new CantileverLineStationTopologyDesign
            {
                FaceMode = CantileverStationFaceMode.Single,
                SingleSide = CantileverArmSide.PositiveY,
                LevelCount = 2,
                RequestedClearHeight = 24.0,
                ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                {
                    ColumnSectionId = "AISC-W-W10X33",
                    Base = new CantileverBaseDesign { SectionId = "AISC-W-W12X26", Length = 48.0 }
                }
            };
            topology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            topology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 4.0;
            var design = new CantileverLineDesign
            {
                Name = "CT-05",
                StationCount = 3,
                ColumnCentreSpacing = 96.0,
                StationTopology = topology,
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign
                    {
                        Arrangement = CantileverArmBodyArrangement.Single,
                        SectionId = "AISC-HSS-RECT-HSS4X4X_250",
                        CutLength = 36.0
                    },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5
                    }
                },
                Bracing = new CantileverBracingDesign()
            };
            var line = CantileverLineResolver.Resolve(
                design, catalog, factory,
                CantileverCataloguePolicies.ColumnBase(catalog),
                CantileverCataloguePolicies.Arm(catalog));
            Assert.False(line.IsBlocked, string.Join(" | ", line.Diagnostics.Select(item => item.Message)));
            return (line, CantileverViewPlanBuilder.Build(line, CantileverViewKind.Frontal, factory));
        }
    }
}
