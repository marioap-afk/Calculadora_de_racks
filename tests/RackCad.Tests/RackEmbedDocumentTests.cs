using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The uniform embed envelope (kind + id + name + design) round-trips and tolerates junk.</summary>
    public class RackEmbedDocumentTests
    {
        [Fact]
        public void RoundTrips_KindIdNameAndDesign()
        {
            var store = new RackEmbedStore();
            var document = new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindDynamic,
                Id = "id-1",
                Name = "Rack A",
                Design = "{\"any\":1}"
            };

            var back = store.Deserialize(store.Serialize(document));

            Assert.Equal(RackEmbedDocument.KindDynamic, back.Kind);
            Assert.Equal("id-1", back.Id);
            Assert.Equal("Rack A", back.Name);
            Assert.Equal("{\"any\":1}", back.Design);
        }

        [Fact]
        public void Deserialize_JunkOrEmpty_ReturnsNull()
        {
            var store = new RackEmbedStore();
            Assert.Null(store.Deserialize("no soy json"));
            Assert.Null(store.Deserialize(""));
            Assert.Null(store.Deserialize(null));
        }

        // ---- I-18b increment 4a: the Push Back embed kind ----

        [Fact]
        public void KindPushBack_IsExactlyPushback_AndTheFourPriorConstantsAreUnchanged_NoVersionBump()
        {
            Assert.Equal("pushback", RackEmbedDocument.KindPushBack);
            Assert.Equal("selective", RackEmbedDocument.KindSelective);
            Assert.Equal("dynamic", RackEmbedDocument.KindDynamic);
            Assert.Equal("cabecera", RackEmbedDocument.KindCabecera);
            Assert.Equal("cama", RackEmbedDocument.KindCama);
            Assert.Equal("1.0", RackEmbedDocument.CurrentSchemaVersion); // the new kind does NOT bump the envelope version
        }

        [Fact]
        public void PushBackEnvelope_RoundTrips_KindViewSectionAndDesign()
        {
            var store = new RackEmbedStore();
            var document = new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindPushBack,
                Id = "pb-1",
                Name = "Rack PB",
                View = RackEmbedDocument.ViewLateral,
                Section = 2,
                Design = "{\"pb\":1}"
            };

            var back = store.Deserialize(store.Serialize(document));

            Assert.Equal(RackEmbedDocument.KindPushBack, back.Kind);
            Assert.Equal("pb-1", back.Id);
            Assert.Equal("Rack PB", back.Name);
            Assert.Equal(RackEmbedDocument.ViewLateral, back.View);
            Assert.Equal(2, back.Section);
            Assert.Equal("{\"pb\":1}", back.Design);
        }

        [Fact]
        public void PushBackEnvelope_PreservesUnknownFields_AndDoesNotDowngradeAHigherMinor()
        {
            var store = new RackEmbedStore();
            var initial = store.Serialize(new RackEmbedDocument { Kind = RackEmbedDocument.KindPushBack, Id = "pb-2", Design = "{}" });
            var node = System.Text.Json.Nodes.JsonNode.Parse(initial).AsObject();
            node["futureField"] = "keep";           // an unknown envelope field a newer build wrote
            node["SchemaVersion"] = "1.7";          // a future SAME-major minor

            var resaved = store.Serialize(store.Deserialize(node.ToJsonString()));

            Assert.Contains("futureField", resaved);
            Assert.Contains("keep", resaved);
            using var parsed = System.Text.Json.JsonDocument.Parse(resaved);
            Assert.Equal("1.7", parsed.RootElement.GetProperty("SchemaVersion").GetString()); // not downgraded to 1.0
        }

        [Fact]
        public void PushBackEnvelope_SameMajorIsReadable_IncompatibleMajorIsSkipped()
        {
            var store = new RackEmbedStore();
            var pb = new RackEmbedDocument { Kind = RackEmbedDocument.KindPushBack, Id = "pb-3", Design = "{}" };

            var sameMajor = System.Text.Json.Nodes.JsonNode.Parse(store.Serialize(pb)).AsObject();
            sameMajor["SchemaVersion"] = "1.5";
            Assert.NotNull(store.Deserialize(sameMajor.ToJsonString())); // same-major stays readable (tolerant policy)

            var higherMajor = System.Text.Json.Nodes.JsonNode.Parse(store.Serialize(pb)).AsObject();
            higherMajor["SchemaVersion"] = "9.0";
            Assert.Null(store.Deserialize(higherMajor.ToJsonString())); // incompatible MAJOR skipped (null), never thrown
        }

        [Fact]
        public void Composer_ForPushBack_TakesFreshIdentity_AndPreservesSourceEnvelopeMetadata()
        {
            var store = new RackEmbedStore();
            var sourceJson = System.Text.Json.Nodes.JsonNode.Parse(
                store.Serialize(new RackEmbedDocument { Kind = RackEmbedDocument.KindPushBack, Design = "{}" })).AsObject();
            sourceJson["futureEnvField"] = "keep-env";
            sourceJson["SchemaVersion"] = "1.9";
            var source = store.Deserialize(sourceJson.ToJsonString());

            var composed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindPushBack, "pb-4", "Rack PB", RackEmbedDocument.ViewFrontal, 1, "{\"d\":1}");
            var resaved = store.Serialize(composed);

            Assert.Equal(RackEmbedDocument.KindPushBack, composed.Kind);
            Assert.Equal("pb-4", composed.Id);
            Assert.Equal("Rack PB", composed.Name);
            Assert.Equal(RackEmbedDocument.ViewFrontal, composed.View);
            Assert.Equal(1, composed.Section);
            Assert.Contains("futureEnvField", resaved); // source envelope metadata inherited
            Assert.Contains("keep-env", resaved);
            using var parsed = System.Text.Json.JsonDocument.Parse(resaved);
            Assert.Equal("1.9", parsed.RootElement.GetProperty("SchemaVersion").GetString()); // not downgraded
        }
    }
}
