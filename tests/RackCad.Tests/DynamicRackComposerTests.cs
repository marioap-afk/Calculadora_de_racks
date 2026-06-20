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

        private static DynamicRackSystem CreateSystem(int palletsDeep = 4, double? headerDepthOverride = null)
        {
            return new DynamicRackSystemFactory(Catalog).Create(
                Pallet48(),
                palletsDeep,
                RackFrameTemplateCatalog.Default,
                "POSTE_OMEGA_3X3",
                headerHeight: 132.0,
                headerDepthOverride: headerDepthOverride);
        }

        // ---- Generator overload (header length / depth override) ----

        [Fact]
        public void Generate_WithHeaderLengthOverride_UsesItForHeadersAndSumsToTotal()
        {
            var layout = new DynamicRackLayoutGenerator().Generate(Pallet48(), 4, headerLength: 60.0);

            Assert.Equal(2 * 60.0 + 2 * 48.0, layout.TotalLength); // 216
            Assert.Equal(60.0, layout.Modules.First().Length);
            Assert.Equal(60.0, layout.Modules.Last().Length);
            Assert.Equal(layout.TotalLength, layout.SumOfModuleLengths);
            Assert.Equal(156.0, layout.Modules.Single(m => m.Kind == RackModuleKind.HeaderEnd).StartOffset);
        }

        // ---- Factory builds a real header at the system-driven depth ----

        [Fact]
        public void Factory_BuildsHeaderAtPalletDepthPlusSix()
        {
            var system = CreateSystem();

            Assert.NotNull(system.Header);
            Assert.Equal(54.0, system.EffectiveHeaderDepth);
            Assert.Equal(54.0, system.Header.Depth);
            Assert.Equal(4, system.Header.Horizontals.Count); // STD-3P header is real, not a stub
        }

        [Fact]
        public void Factory_WithDepthOverride_UsesOverrideForTheHeader()
        {
            var system = CreateSystem(headerDepthOverride: 60.0);

            Assert.Equal(60.0, system.EffectiveHeaderDepth);
            Assert.Equal(60.0, system.Header.Depth);
        }

        // ---- Composition: positioning + symmetry + real members ----

        [Fact]
        public void Compose_PlacesEveryModuleAndKeepsCounts()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4));

            Assert.Equal(composed.Layout.Modules.Count, composed.PlacedModules.Count);
            Assert.Equal(204.0, composed.Layout.TotalLength);
            Assert.Equal(0.0, composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderStart).Placement.OffsetX);
            Assert.Equal(150.0, composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderEnd).Placement.OffsetX);
        }

        [Fact]
        public void Compose_BothHeadersShareTheSameConfiguration()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4));

            var start = composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderStart).Header;
            var end = composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderEnd).Header;

            Assert.NotNull(start);
            Assert.Same(start, end); // symmetric: same instance, different placement
            Assert.NotEmpty(start.Members); // real members were generated
        }

        [Fact]
        public void Compose_SeparatorsAndPosts_HaveNoHeader()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4));

            Assert.All(
                composed.PlacedModules.Where(p => p.Module.Kind == RackModuleKind.Separator || p.Module.Kind == RackModuleKind.IntermediatePost),
                p => Assert.False(p.IsHeader));
        }

        [Fact]
        public void Compose_WithOverride_ShiftsPlacementsAndTotal()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4, headerDepthOverride: 60.0));

            Assert.Equal(216.0, composed.Layout.TotalLength);
            Assert.Equal(60.0, composed.System.Header.Depth);
            Assert.Equal(156.0, composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderEnd).Placement.OffsetX);
        }

        [Fact]
        public void Compose_EndHeaderRightUpright_LandsAtTotalLength()
        {
            var composed = new DynamicRackComposer().Compose(CreateSystem(4));
            var depth = composed.System.EffectiveHeaderDepth;
            var endHeader = composed.PlacedModules.Single(p => p.Module.Kind == RackModuleKind.HeaderEnd);

            // A horizontal member's far end sits on the right upright (local X = depth).
            var rightUpright = composed.System.Header.Members.First(m => Math.Abs(m.End.HorizontalPositionRatio - 1.0) < 1e-9);
            var worldEnd = endHeader.Placement.Apply(new Point2D(rightUpright.End.HorizontalPositionRatio * depth, rightUpright.End.Elevation));

            Assert.Equal(composed.Layout.TotalLength, worldEnd.X, 6);
        }

        // ---- Validation ----

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
