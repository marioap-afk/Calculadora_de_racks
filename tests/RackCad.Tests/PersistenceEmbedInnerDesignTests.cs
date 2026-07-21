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
    /// I-11 (changes-required, 3ª ronda): the ENVELOPE and its inner Design string are TWO INDEPENDENT boundaries. For a
    /// dynamic / cabecera block the inner Design is itself a <see cref="RackProjectDocument"/> — so preserving the block's
    /// unknown metadata means re-serializing the inner design WITH the source project it was read from
    /// (<c>RackProject.ForX(model).WithSourceMetadataFrom(innerSource)</c>), in addition to composing the envelope with
    /// <see cref="RackEmbedComposer"/>. These tests lock the store-level MECHANISM that the Plugin edit-redraw calls
    /// per view-block; the actual Plugin/WPF wiring (that each block's own Embed.Design is threaded in) is covered by the
    /// AutoCAD matrix, not by these store tests.
    /// </summary>
    public class PersistenceEmbedInnerDesignTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem DynamicSystem()
            => new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0);

        private static RackFrameConfiguration Cabecera()
            => new HardcodedStandardRackFrameService(Catalog).CreateDefault();

        /// <summary>Build a block's inner Design (a wrapper JSON) with an injected version + unknown field, as a newer
        /// build would have written it, then read it back as the source RackProject the redraw would deserialize.</summary>
        private static RackProject InnerSource(string designJson, string schemaVersion, string unknownKey, JsonNode unknownValue)
        {
            var node = JsonNode.Parse(designJson).AsObject();
            node["SchemaVersion"] = schemaVersion;
            node[unknownKey] = unknownValue;
            return new RackProjectStore().Deserialize(node.ToJsonString());
        }

        [Fact]
        public void DynamicEmbed_InnerWrapperFutureMinorAndUnknown_SurvivesEditRedraw()
        {
            var store = new RackProjectStore();
            var baseJson = store.Serialize(RackProject.ForDynamic(DynamicSystem()));
            var source = InnerSource(baseJson, "2.5", "futureInner", JsonValue.Create(7));

            // The redraw re-serializes the reopened design WITH that block's own inner source.
            var back = store.Serialize(RackProject.ForDynamic(source.DynamicDesign, source.DynamicSystem).WithSourceMetadataFrom(source));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.5", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal(7, doc.RootElement.GetProperty("futureInner").GetInt32());
        }

        [Fact]
        public void CabeceraEmbed_InnerWrapperFutureMinorAndUnknown_SurvivesEditRedraw()
        {
            var store = new RackProjectStore();
            var baseJson = store.Serialize(RackProject.ForSelective(Cabecera()));
            var source = InnerSource(baseJson, "2.5", "futureInner", JsonValue.Create("keep"));

            var back = store.Serialize(RackProject.ForSelective(source.Header).WithSourceMetadataFrom(source));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.5", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("keep", doc.RootElement.GetProperty("futureInner").GetString());
        }

        [Fact]
        public void TwoViews_WithDistinctInnerWrappers_EachKeepsItsOwnMetadata()
        {
            var store = new RackProjectStore();
            var baseJson = store.Serialize(RackProject.ForDynamic(DynamicSystem()));
            var viewA = InnerSource(baseJson, "2.1", "futureInner", JsonValue.Create("A"));
            var viewB = InnerSource(baseJson, "2.3", "futureInner", JsonValue.Create("B"));

            var backA = store.Serialize(RackProject.ForDynamic(viewA.DynamicDesign, viewA.DynamicSystem).WithSourceMetadataFrom(viewA));
            var backB = store.Serialize(RackProject.ForDynamic(viewB.DynamicDesign, viewB.DynamicSystem).WithSourceMetadataFrom(viewB));

            using var docA = JsonDocument.Parse(backA);
            using var docB = JsonDocument.Parse(backB);
            Assert.Equal("2.1", docA.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("A", docA.RootElement.GetProperty("futureInner").GetString());
            Assert.Equal("2.3", docB.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("B", docB.RootElement.GetProperty("futureInner").GetString());
        }

        [Fact]
        public void NewView_InheritsBothInnerWrapperAndEnvelopeFromInitiator()
        {
            // A NEW view inserted during an edit inherits the initiating block's inner wrapper AND its envelope.
            var store = new RackProjectStore();
            var embedStore = new RackEmbedStore();

            var baseJson = store.Serialize(RackProject.ForDynamic(DynamicSystem()));
            var initiatingInner = InnerSource(baseJson, "2.4", "futureInner", JsonValue.Create("inner"));

            var envNode = JsonNode.Parse(embedStore.Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindDynamic,
                Id = "id",
                Name = "n",
                View = RackEmbedDocument.ViewLateral,
                Section = 0,
                Design = "{}"
            })).AsObject();
            envNode["SchemaVersion"] = "1.5";
            envNode["futureEnv"] = 9;
            var initiatingEnvelope = embedStore.Deserialize(envNode.ToJsonString());

            // Inner design of the new view = reopened design + initiating inner source.
            var innerJson = store.Serialize(
                RackProject.ForDynamic(initiatingInner.DynamicDesign, initiatingInner.DynamicSystem).WithSourceMetadataFrom(initiatingInner));
            // Envelope of the new view = composed from the initiating envelope.
            var newViewEmbedJson = embedStore.Serialize(RackEmbedComposer.Compose(
                initiatingEnvelope, RackEmbedDocument.KindDynamic, "id", "n", RackEmbedDocument.ViewFrontal, 1, innerJson));

            using var envelope = JsonDocument.Parse(newViewEmbedJson);
            Assert.Equal("1.5", envelope.RootElement.GetProperty("SchemaVersion").GetString());     // envelope inherited
            Assert.Equal(9, envelope.RootElement.GetProperty("futureEnv").GetInt32());
            using var inner = JsonDocument.Parse(envelope.RootElement.GetProperty("Design").GetString());
            Assert.Equal("2.4", inner.RootElement.GetProperty("SchemaVersion").GetString());          // inner wrapper inherited
            Assert.Equal("inner", inner.RootElement.GetProperty("futureInner").GetString());
        }
    }
}
