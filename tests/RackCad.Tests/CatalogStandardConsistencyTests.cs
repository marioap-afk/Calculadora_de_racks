using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Guards the migration away from hardcoded ids: every catalog id referenced by
    /// the standard frame must exist in the shipped JSON catalogs. When the standard
    /// service is changed to read from catalogs, these references must already resolve.
    /// </summary>
    public class CatalogStandardConsistencyTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void StandardPostAndPlateIds_ExistInCatalog()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var catalog = Catalog;

            Assert.NotNull(catalog.PostProfiles.FindProfile(configuration.LeftPost.PostCatalogId));
            Assert.NotNull(catalog.PostProfiles.FindProfile(configuration.RightPost.PostCatalogId));
            Assert.NotNull(catalog.BasePlates.FindBasePlate(configuration.LeftBasePlate.PlateCatalogId));
            Assert.NotNull(catalog.BasePlates.FindBasePlate(configuration.RightBasePlate.PlateCatalogId));
        }

        [Fact]
        public void StandardHorizontalProfileIds_ExistInCatalog()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var catalog = Catalog;

            foreach (var horizontal in configuration.Horizontals)
            {
                Assert.NotNull(catalog.TrussProfiles.FindProfile(horizontal.ProfileId));
            }
        }

        [Fact]
        public void StandardDiagonalAndConnectionPointIds_ExistInCatalog()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var catalog = Catalog;

            foreach (var panel in configuration.BracingPanels)
            {
                Assert.NotNull(catalog.TrussProfiles.FindProfile(panel.DiagonalProfileId));
                Assert.NotNull(catalog.ConnectionPoints.FindConnectionPoint(panel.StartConnectionPointId));
                Assert.NotNull(catalog.ConnectionPoints.FindConnectionPoint(panel.EndConnectionPointId));
            }
        }

        [Fact]
        public void DynamicInOutC6_HasProfileCompleteBlockAndConnectorReferences()
        {
            var catalog = Catalog;
            var beam = catalog.BeamProfiles.Single(entry => string.Equals(
                entry.Id, TestCatalogIds.Profiles.Beams.DynamicInOut, StringComparison.OrdinalIgnoreCase));

            Assert.Equal(new[] { 6.0 }, PeralteList.Parse(beam.Peraltes));
            Assert.Contains(catalog.Mensulas, entry => string.Equals(entry.Id, beam.Mensula, StringComparison.OrdinalIgnoreCase));
            Assert.Equal(
                "LARGUERO_IN_OUT_C6_LATERAL",
                catalog.Blocks.FindBlock(beam.Id, TestCatalogIds.Views.Lateral)?.BlockName);

            Assert.NotNull(catalog.ConnectionPoints.FindConnectionPoint(TestCatalogIds.ConnectionPoints.BedMate));
            Assert.NotNull(catalog.ConnectionPoints.FindConnectionPoint(TestCatalogIds.ConnectionPoints.RailInOut));

            var beamMate = catalog.ConnectionLayout.FindConnectionLayout(
                beam.Id,
                TestCatalogIds.ConnectionPoints.BedMate,
                TestCatalogIds.Views.Lateral);
            var railMate = catalog.ConnectionLayout.FindConnectionLayout(
                TestCatalogIds.FlowBed.Rail,
                TestCatalogIds.ConnectionPoints.RailInOut,
                TestCatalogIds.Views.Lateral);
            Assert.NotNull(beamMate);
            Assert.Equal(3.7544, beamMate.LocalX, 4);
            Assert.Equal(1.8783, beamMate.LocalY, 4);
            Assert.NotNull(railMate);
            Assert.Equal(1.5, railMate.LocalX, 4);
            Assert.Equal(1.25, railMate.LocalY, 4);
        }

        [Fact]
        public void DynamicIntermediateBeam_HasProfileBlockAndBothBedMates()
        {
            var catalog = Catalog;
            var beam = catalog.BeamProfiles.Single(entry => string.Equals(
                entry.Id, TestCatalogIds.Profiles.Beams.DynamicIntermediate, StringComparison.OrdinalIgnoreCase));

            Assert.Equal(new[] { 3.0, 3.5, 4.0, 4.5, 5.0, 5.5, 6.0 }, PeralteList.Parse(beam.Peraltes));
            Assert.Contains(catalog.Mensulas, entry => string.Equals(
                entry.Id, beam.Mensula, StringComparison.OrdinalIgnoreCase));
            Assert.Equal(
                "LARGUERO_ESCALON_INFINITO_LATERAL",
                catalog.Blocks.FindBlock(beam.Id, TestCatalogIds.Views.Lateral)?.BlockName);
            Assert.NotNull(catalog.ConnectionPoints.FindConnectionPoint(TestCatalogIds.ConnectionPoints.LeftStart));
            Assert.NotNull(catalog.ConnectionPoints.FindConnectionPoint(TestCatalogIds.ConnectionPoints.RightStart));

            var left = catalog.ConnectionLayout.FindConnectionLayout(
                beam.Id,
                TestCatalogIds.ConnectionPoints.LeftStart,
                TestCatalogIds.Views.Lateral);
            var right = catalog.ConnectionLayout.FindConnectionLayout(
                beam.Id,
                TestCatalogIds.ConnectionPoints.RightStart,
                TestCatalogIds.Views.Lateral);
            Assert.NotNull(left);
            Assert.Equal(0.2214, left.LocalX, 4);
            Assert.Equal(6.1972, left.LocalY, 4);
            Assert.NotNull(right);
            Assert.Equal(1.5095, right.LocalX, 4);
            Assert.Equal(6.1972, right.LocalY, 4);
        }
    }
}
