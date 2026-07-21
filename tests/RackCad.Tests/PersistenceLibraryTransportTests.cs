using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-11 (changes-required, 4ª ronda, item 2): metadata must survive the LIBRARY → DRAWING insert. The library editor's
    /// source project/document is exposed as an output sidecar and the host command threads it into the new embed. These
    /// pure tests lock the composition the DWG payload uses (the inner RackProjectDocument via WithSourceMetadataFrom for
    /// dynamic/cabecera, the FlowBedDocument via version-resolve + ExtensionData for cama); the Plugin/WPF transport wiring
    /// is exercised by the AutoCAD matrix (B5/B6).
    /// </summary>
    public class PersistenceLibraryTransportTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static RackProject DynamicProject()
            => RackProject.ForDynamic(new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0));

        private static RackFrameConfiguration Cabecera()
            => new HardcodedStandardRackFrameService(Catalog).CreateDefault();

        private static RackProject LoadWithWrapperMeta(RackProject seed, string schemaVersion, string unknownKey)
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(seed)).AsObject();
            node["SchemaVersion"] = schemaVersion;
            node[unknownKey] = "keep";
            return store.Deserialize(node.ToJsonString());
        }

        [Fact]
        public void DynamicLibraryToDrawing_InnerWrapperMetadataPreserved()
        {
            var store = new RackProjectStore();
            var librarySource = LoadWithWrapperMeta(DynamicProject(), "2.6", "futureInner");

            // The DWG embed's inner design = the reopened design + the library source project (as DrawDynamicView threads it).
            var innerDesignJson = store.Serialize(
                RackProject.ForDynamic(librarySource.DynamicDesign, librarySource.DynamicSystem).WithSourceMetadataFrom(librarySource));

            using var doc = JsonDocument.Parse(innerDesignJson);
            Assert.Equal("2.6", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("keep", doc.RootElement.GetProperty("futureInner").GetString());
        }

        [Fact]
        public void CamaLibraryToDrawing_FlowBedMetadataPreserved()
        {
            // The library cama's FlowBed document (1.6 + unknown), carried via the sidecar into the DWG embed payload as
            // BuildCamaPayload composes it: FromDomain(edited) + non-downgraded version + inherited ExtensionData.
            var sourceFlowBed = new FlowBedConfigurationStore().DeserializeDocument(
                "{\"SchemaVersion\":\"1.6\",\"BedType\":\"Dynamic\",\"LaneDepth\":96,\"PalletDepth\":48,\"RollerId\":\"R\",\"futureBed\":\"keep\"}");
            Assert.NotNull(sourceFlowBed);

            var edited = new FlowBedConfiguration { BedType = FlowBedType.Pushback, LaneDepth = 150.0, PalletDepth = 0.0, RollerId = "R2" };
            var designDocument = FlowBedDocument.FromDomain(edited);
            designDocument.SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(sourceFlowBed.SchemaVersion, FlowBedDocument.CurrentSchemaVersion);
            designDocument.ExtensionData = sourceFlowBed.ExtensionData;
            var payloadDesign = new FlowBedConfigurationStore().SerializeDocument(designDocument);

            using var doc = JsonDocument.Parse(payloadDesign);
            Assert.Equal("1.6", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("keep", doc.RootElement.GetProperty("futureBed").GetString());
            Assert.Equal(150.0, doc.RootElement.GetProperty("LaneDepth").GetDouble(), 4);
            Assert.Equal("R2", doc.RootElement.GetProperty("RollerId").GetString());
        }

        [Fact]
        public void FreshInsert_NoSource_WritesCurrentVersions()
        {
            var store = new RackProjectStore();
            using var dynamicDoc = JsonDocument.Parse(store.Serialize(DynamicProject())); // no WithSourceMetadataFrom
            Assert.Equal("2.0", dynamicDoc.RootElement.GetProperty("SchemaVersion").GetString());
        }

        [Fact]
        public void CabeceraFromWrapper_PreservesMetadata_BareHeaderFabricatesNone()
        {
            var store = new RackProjectStore();

            // Wrapper source (Kind=Selective) with wrapper metadata → preserved into the DWG cabecera embed's inner design.
            var wrapperSource = LoadWithWrapperMeta(RackProject.ForSelective(Cabecera()), "2.5", "futureInner");
            using (var wrapped = JsonDocument.Parse(store.Serialize(
                RackProject.ForSelective(wrapperSource.Header).WithSourceMetadataFrom(wrapperSource))))
            {
                Assert.Equal("2.5", wrapped.RootElement.GetProperty("SchemaVersion").GetString());
                Assert.Equal("keep", wrapped.RootElement.GetProperty("futureInner").GetString());
            }

            // A bare legacy header (no wrapper) has no SourceDocument → nothing is fabricated (current version, no unknowns).
            var bareHeaderProject = store.Deserialize(new RackFrameProjectStore().Serialize(Cabecera())); // legacy bare header
            using (var bare = JsonDocument.Parse(store.Serialize(
                RackProject.ForSelective(bareHeaderProject.Header).WithSourceMetadataFrom(bareHeaderProject))))
            {
                Assert.Equal("2.0", bare.RootElement.GetProperty("SchemaVersion").GetString());
                Assert.False(bare.RootElement.TryGetProperty("futureInner", out _));
            }
        }
    }
}
