using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicRackComposerTests
    {
        private static PalletSpecification Pallet48()
        {
            return new PalletSpecification(front: 42.0, depth: 48.0, height: 60.0, weight: 1000.0, weightUnit: "kg");
        }

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem CreateSystem(int palletsDeep = 4)
        {
            return new DynamicRackSystemFactory(Catalog).Create(
                Pallet48(),
                palletsDeep,
                RackFrameTemplateCatalog.Default,
                "POSTE_OMEGA_3X3",
                headerHeight: 132.0);
        }

        [Fact]
        public void Factory_BuildsRealHeaderAtPalletDepthPlusSix()
        {
            var system = CreateSystem();

            Assert.NotNull(system.Header);
            Assert.Equal(54.0, system.EffectiveHeaderDepth);
            Assert.Equal(54.0, system.Header.Depth);
            Assert.Equal(4, system.Header.Horizontals.Count); // STD-3P header is real, not a stub
        }

        [Fact]
        public void Compose_PlacesExactlyNModules()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4));

            Assert.Equal(4, composed.Layout.Modules.Count);          // module count == pallets deep
            Assert.Equal(composed.Layout.Modules.Count, composed.PlacedModules.Count);
            Assert.Equal(204.0, composed.Layout.TotalLength);
            Assert.Equal(0.0, composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderStart).Placement.OffsetX);
            Assert.Equal(150.0, composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderEnd).Placement.OffsetX);
        }

        [Fact]
        public void Compose_IntermediatePostsAreMarkersNotPlacedModules()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4));

            Assert.Equal(new[] { 102.0 }, composed.Layout.IntermediatePosts); // center marker
            Assert.DoesNotContain(composed.PlacedModules, p => p.Module.Kind == RackModuleKind.Separator && p.IsHeader);
        }

        [Fact]
        public void Compose_BothHeadersShareTheSameConfiguration()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4));

            var start = composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderStart).Header;
            var end = composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderEnd).Header;

            Assert.NotNull(start);
            Assert.Same(start, end);
            Assert.NotEmpty(start.Members);
        }

        [Fact]
        public void Compose_SeparatorsHaveNoHeader()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(5));

            Assert.All(
                composed.PlacedModules.Where(p => p.Module.Kind == RackModuleKind.Separator),
                p => Assert.False(p.IsHeader));
        }

        [Fact]
        public void Compose_EndHeaderRightUpright_LandsAtTotalLength()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4));
            var depth = composed.System.EffectiveHeaderDepth;
            var endHeader = composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderEnd);

            var rightUpright = composed.System.Header.Members.First(m => Math.Abs(m.End.HorizontalPositionRatio - 1.0) < 1e-9);
            var worldEnd = endHeader.Placement.Apply(new Point2D(rightUpright.End.HorizontalPositionRatio * depth, rightUpright.End.Elevation));

            Assert.Equal(composed.Layout.TotalLength, worldEnd.X, 6);
        }

        [Fact]
        public void Compose_NullSystem_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicRackComposer().Compose(null));
        }

        [Fact]
        public void Compose_SystemWithoutHeader_Throws()
        {
            var system = new DynamicRackSystem { Pallet = Pallet48(), PalletsDeep = 4, Header = null };
            Assert.Throws<InvalidOperationException>(() => new DynamicRackComposer().Compose(system));
        }
    }
}
