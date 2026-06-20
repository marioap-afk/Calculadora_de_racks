using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class RackProjectStoreTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem DynamicSystem(double? headerDepthOverride = null)
        {
            return new DynamicRackSystemFactory(Catalog).Create(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0,
                headerDepthOverride: headerDepthOverride);
        }

        private static RackFrameConfiguration SelectiveHeader()
        {
            return new HardcodedStandardRackFrameService(Catalog).CreateDefault();
        }

        [Fact]
        public void RoundTrip_DynamicSystem_PreservesPalletHeaderAndKind()
        {
            var store = new RackProjectStore();

            var loaded = store.Deserialize(store.Serialize(RackProject.ForDynamic(DynamicSystem())));

            Assert.Equal(RackSystemKind.PalletFlow, loaded.Kind);
            var system = loaded.DynamicSystem;
            Assert.NotNull(system);
            Assert.Equal(48.0, system.Pallet.Depth);
            Assert.Equal(42.0, system.Pallet.Front);
            Assert.Equal(1000.0, system.Pallet.Weight);
            Assert.Equal("kg", system.Pallet.WeightUnit);
            Assert.Equal(4, system.PalletsDeep);
            Assert.Null(system.HeaderDepthOverride);
            Assert.Equal(54.0, system.Header.Depth);
            Assert.Equal(4, system.Header.Horizontals.Count);
            Assert.Equal(3, system.Header.BracingPanels.Count);
            Assert.NotEmpty(system.Header.Members); // members rebuilt on load
        }

        [Fact]
        public void RoundTrip_DynamicSystem_PreservesDepthOverride()
        {
            var store = new RackProjectStore();

            var loaded = store.Deserialize(store.Serialize(RackProject.ForDynamic(DynamicSystem(headerDepthOverride: 60.0))));

            Assert.Equal(60.0, loaded.DynamicSystem.HeaderDepthOverride);
            Assert.Equal(60.0, loaded.DynamicSystem.Header.Depth);
        }

        [Fact]
        public void RoundTrip_SelectiveHeader_PreservesHeader()
        {
            var store = new RackProjectStore();

            var loaded = store.Deserialize(store.Serialize(RackProject.ForSelective(SelectiveHeader())));

            Assert.Equal(RackSystemKind.Selective, loaded.Kind);
            Assert.NotNull(loaded.Header);
            Assert.Equal(4, loaded.Header.Horizontals.Count);
            Assert.NotEmpty(loaded.Header.Members);
        }

        [Fact]
        public void Load_LegacyBareHeaderFile_IsTreatedAsSelective()
        {
            // A file produced by the old single-header store has no "kind" property.
            var legacyJson = new RackFrameProjectStore().Serialize(SelectiveHeader());

            var loaded = new RackProjectStore().Deserialize(legacyJson);

            Assert.Equal(RackSystemKind.Selective, loaded.Kind);
            Assert.NotNull(loaded.Header);
            Assert.Equal(4, loaded.Header.Horizontals.Count);
        }

        [Fact]
        public void Deserialize_InvalidJson_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(() => new RackProjectStore().Deserialize("{ not json"));
        }

        [Fact]
        public void SaveLoad_Dynamic_RoundTripsThroughDisk()
        {
            var store = new RackProjectStore();
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "rackcad-sys-" + System.Guid.NewGuid().ToString("N") + RackProjectStore.FileExtension);

            try
            {
                store.Save(RackProject.ForDynamic(DynamicSystem()), path);
                var loaded = store.Load(path);
                Assert.Equal(RackSystemKind.PalletFlow, loaded.Kind);
                Assert.Equal(4, loaded.DynamicSystem.PalletsDeep);
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
