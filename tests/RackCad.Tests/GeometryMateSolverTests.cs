using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class GeometryMateSolverTests
    {
        private const double Tolerance = 1e-9;

        [Fact]
        public void SolveCoincident_PlacesPieceSoMateLocalLandsOnAnchor()
        {
            var anchor = new Point2D(10.0, 5.0);
            var mateLocal = new Point2D(2.0, 0.25);

            var placement = MateSolver.SolveCoincident(anchor, mateLocal);

            Assert.True(MateSolver.ToWorld(placement, mateLocal).ApproxEquals(anchor, Tolerance));
            Assert.Equal(8.0, placement.OffsetX);
            Assert.Equal(4.75, placement.OffsetY);
        }

        [Fact]
        public void ResolveBasePlates_AnchorsLeftAtZeroAndRightAtDepth()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var configuration = new RackFrameConfigurationFactory(catalog)
                .Build(RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0, 42.0);

            var plates = FramePlacementResolver.ResolveBasePlates(configuration, catalog);

            Assert.Equal(2, plates.Count);
            var left = plates.Single(p => p.Side == PostSide.Left);
            var right = plates.Single(p => p.Side == PostSide.Right);
            Assert.True(left.Anchor.ApproxEquals(new Point2D(0.0, 0.0), Tolerance));
            Assert.True(right.Anchor.ApproxEquals(new Point2D(42.0, 0.0), Tolerance));
        }

        [Fact]
        public void ResolveBasePlates_PlateConnectionPointMatesOntoTheAnchor()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var configuration = new RackFrameConfigurationFactory(catalog)
                .Build(RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0, 42.0);

            var left = FramePlacementResolver.ResolveBasePlates(configuration, catalog)
                .Single(p => p.Side == PostSide.Left);

            var layout = catalog.ConnectionLayout.FindConnectionLayout(left.PlateCatalogId, left.ConnectionPointId, "FRONTAL");
            var local = new Point2D(layout.LocalX, layout.LocalY);

            // Once placed, the plate's named point must coincide with the post-base anchor.
            Assert.True(MateSolver.ToWorld(left.Placement, local).ApproxEquals(left.Anchor, Tolerance));
        }
    }
}
