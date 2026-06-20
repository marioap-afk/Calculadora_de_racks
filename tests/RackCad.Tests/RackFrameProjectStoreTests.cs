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
        public void Deserialize_RebuildsDerivedMembersAndPanelElevations()
        {
            var store = new RackFrameProjectStore();

            var loaded = store.Deserialize(store.Serialize(SampleConfig()));

            Assert.NotEmpty(loaded.Members);
            Assert.Equal(0.0, loaded.BracingPanels.First().StartElevation);
            Assert.Equal(44.0, loaded.BracingPanels.First().EndElevation);
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
                Assert.Equal(4, loaded.Horizontals.Count);
                Assert.Equal(3, loaded.BracingPanels.Count);
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
