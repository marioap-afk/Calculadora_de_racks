using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicFlowBedLateralBuilderTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem ResolvedSystem()
        {
            return new DynamicRackSystemResolver(Catalog).Resolve(new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                InOutBeamCatalogId = DynamicRackDefaults.InOutBeamCatalogId
            }).System;
        }

        [Fact]
        public void Build_ReusesOneCompleteDynamicBedAtEveryLoadLevel()
        {
            var system = ResolvedSystem();

            var group = new DynamicFlowBedLateralBuilder().Build(system, Catalog);

            Assert.NotNull(group);
            Assert.Equal(system.LoadBeamLevels.Count, group.Placements.Count);
            Assert.Single(group.Instances, instance => instance.Role == HeaderBlockRole.Rail);
            Assert.Contains(group.Instances, instance => instance.Role == HeaderBlockRole.Stop);
            Assert.Contains(group.Instances, instance => instance.Role == HeaderBlockRole.Roller);
            Assert.Contains(group.Instances, instance => instance.Role == HeaderBlockRole.Brake);
            Assert.All(group.Placements, placement => Assert.False(placement.Mirrored));
        }

        [Fact]
        public void Build_UsesTotalLengthMinusFourForTheCompleteBed()
        {
            var system = ResolvedSystem();

            var group = new DynamicFlowBedLateralBuilder().Build(system, Catalog);
            var rail = Assert.Single(group.Instances, instance => instance.Role == HeaderBlockRole.Rail);
            var bed = Assert.Single(
                SystemBomBuilder.Build(system, Catalog).Components,
                component => component.Category == SystemBomBuilder.Cama);

            Assert.Equal(system.TotalLength - 4.0, rail.DynamicParameters["LONGITUD"], 4);
            Assert.Equal(system.TotalLength - 4.0, bed.Length, 4);
        }

        [Fact]
        public void Build_MatesTroquelInToLowLeftBeam_AndKeepsFourInchesTotalClearance()
        {
            var catalog = Catalog;
            var system = ResolvedSystem();
            var group = new DynamicFlowBedLateralBuilder().Build(system, catalog);
            var plan = new DynamicSystemPlan(new[] { group }, Array.Empty<HeaderBlockInstance>());
            var firstRail = plan.Flatten().OfRole(HeaderBlockRole.Rail)
                .OrderBy(instance => instance.Insertion.Y)
                .First();

            var beamMate = catalog.ConnectionLayout.FindConnectionLayout(
                DynamicRackDefaults.InOutBeamCatalogId,
                DynamicRackDefaults.InOutBeamBedMatePoint,
                DynamicRackDefaults.InOutBeamView);
            var railMate = catalog.ConnectionLayout.FindConnectionLayout(
                FlowBedDefaults.RailId,
                FlowBedDefaults.RailInOutMatePoint,
                FlowBedDefaults.View);
            var level = system.LoadBeamLevels[0];
            var expectedExit = new Point2D(beamMate.LocalX, level.ExitElevation + beamMate.LocalY);

            var actualExit = LocalToWorld(
                firstRail,
                new Point2D(railMate.LocalX, railMate.LocalY));

            Assert.True(actualExit.ApproxEquals(expectedExit, 1e-4), actualExit + " != " + expectedExit);
            Assert.Equal(system.TotalLength - 4.0, firstRail.DynamicParameters["LONGITUD"], 4);
            Assert.True(firstRail.RotationRadians > 0.0);
        }

        [Fact]
        public void Build_PreservesTheResolvedSlopeMagnitude_InTheReversedDirection()
        {
            var system = ResolvedSystem();
            var group = new DynamicFlowBedLateralBuilder().Build(system, Catalog);
            var firstRail = new DynamicSystemPlan(new[] { group }, Array.Empty<HeaderBlockInstance>())
                .Flatten().OfRole(HeaderBlockRole.Rail).OrderBy(instance => instance.Insertion.Y).First();
            var expectedAngle = DynamicFlowBedGeometry.Resolve(system, Catalog).First().AngleRadians;

            Assert.Equal(expectedAngle, firstRail.RotationRadians, 6);
            Assert.True(firstRail.RotationRadians > 0.0);
        }

        private static Point2D LocalToWorld(HeaderBlockInstance instance, Point2D local)
        {
            var cos = Math.Cos(instance.RotationRadians);
            var sin = Math.Sin(instance.RotationRadians);
            return new Point2D(
                instance.Insertion.X + local.X * cos - local.Y * sin,
                instance.Insertion.Y + local.X * sin + local.Y * cos);
        }
    }
}
