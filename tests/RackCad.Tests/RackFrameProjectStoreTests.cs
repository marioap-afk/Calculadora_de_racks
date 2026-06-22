using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class RackFrameProjectStoreTests
    {
        private static RackFrameConfiguration SampleConfig()
        {
            var configuration = new RackFrameConfigurationFactory(JsonRackCatalogProvider.FromBaseDirectory().Load())
                .Build(RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0, 42.0);
            // Introduce a manual tweak so the round-trip must preserve non-standard state.
            configuration.BracingPanels[1].Arrangement = BracingPattern.XBracing;
            configuration.Horizontals[2].State = FrameComponentState.Manual;
            return configuration;
        }

        [Fact]
        public void RoundTrip_PreservesSourceOfTruth()
        {
            var store = new RackFrameProjectStore();
            var original = SampleConfig();

            var loaded = store.Deserialize(store.Serialize(original));

            Assert.Equal(original.Height, loaded.Height);
            Assert.Equal(original.Depth, loaded.Depth);
            Assert.Equal(original.LeftPost.PostCatalogId, loaded.LeftPost.PostCatalogId);
            Assert.Equal(original.LeftBasePlate.ConnectionPointId, loaded.LeftBasePlate.ConnectionPointId);
            Assert.Equal(
                original.Horizontals.Select(h => (h.Id, h.Elevation, h.ProfileId, h.State)),
                loaded.Horizontals.Select(h => (h.Id, h.Elevation, h.ProfileId, h.State)));
            Assert.Equal(
                original.BracingPanels.Select(p => (p.PanelId, p.LowerHorizontalId, p.UpperHorizontalId, p.Arrangement)),
                loaded.BracingPanels.Select(p => (p.PanelId, p.LowerHorizontalId, p.UpperHorizontalId, p.Arrangement)));
        }

        [Fact]
        public void RoundTrip_PreservesCelosiaAndDiagonalParameters()
        {
            var store = new RackFrameProjectStore();
            var original = SampleConfig();
            original.CelosiaStartTroquel = 5;
            original.DiagonalStartOffsetTroqueles = 1;
            original.DiagonalEndOffsetTroqueles = 3;

            var loaded = store.Deserialize(store.Serialize(original));

            Assert.Equal(5, loaded.CelosiaStartTroquel);
            Assert.Equal(1, loaded.DiagonalStartOffsetTroqueles);
            Assert.Equal(3, loaded.DiagonalEndOffsetTroqueles);
        }

        [Fact]
        public void RoundTrip_PreservesReinforcement()
        {
            var store = new RackFrameProjectStore();
            var original = SampleConfig();
            original.LeftPost.HasReinforcement = true;
            original.LeftPost.ReinforcementCatalogId = "POSTE_OMEGA_3X3";
            original.LeftPost.ReinforcementHeight = 60.0;

            var loaded = store.Deserialize(store.Serialize(original));

            Assert.True(loaded.LeftPost.HasReinforcement);
            Assert.Equal("POSTE_OMEGA_3X3", loaded.LeftPost.ReinforcementCatalogId);
            Assert.Equal(60.0, loaded.LeftPost.ReinforcementHeight, 4);
        }

        [Fact]
        public void LegacyDocument_WithoutCelosiaParameters_FallsBackToDefaults()
        {
            // A project that predates these fields leaves them null -> built-in defaults.
            var configuration = new RackFrameProjectDocument().ToConfiguration();

            Assert.Equal(3, configuration.CelosiaStartTroquel);
            Assert.Equal(2, configuration.DiagonalStartOffsetTroqueles);
            Assert.Equal(2, configuration.DiagonalEndOffsetTroqueles);
        }

        [Fact]
        public void Deserialize_RebuildsDerivedMembersAndPanelElevations()
        {
            var store = new RackFrameProjectStore();

            var loaded = store.Deserialize(store.Serialize(SampleConfig()));

            Assert.NotEmpty(loaded.Members);
            Assert.Equal(4.0, loaded.BracingPanels.First().StartElevation);
            Assert.Equal(48.0, loaded.BracingPanels.First().EndElevation);
        }

        [Fact]
        public void Deserialize_InvalidJson_ThrowsDescriptiveError()
        {
            var store = new RackFrameProjectStore();

            var error = Assert.Throws<System.InvalidOperationException>(() => store.Deserialize("{ not valid"));
            Assert.Contains("JSON", error.Message);
        }

        [Fact]
        public void SaveLoad_RoundTripsThroughDisk()
        {
            var store = new RackFrameProjectStore();
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "rackcad-" + System.Guid.NewGuid().ToString("N") + RackFrameProjectStore.FileExtension);

            try
            {
                store.Save(SampleConfig(), path);
                var loaded = store.Load(path);
                Assert.Equal(5, loaded.Horizontals.Count);
                Assert.Equal(4, loaded.BracingPanels.Count);
            }
            finally
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }
    }
}
