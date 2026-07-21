using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-11 (changes-required round): schema-version discipline. Two guarantees that the first implementation missed:
    /// (1) the drawing envelope rejects a newer MAJOR tolerantly (null, never aborting the scan); (2) a re-save never
    /// DOWNGRADES a newer same-major minor version. The version-downgrade tests are written against the existing store
    /// APIs so they compile and FAIL before the fix (the store re-stamped CurrentSchemaVersion); they pass after it.
    /// </summary>
    public class PersistenceVersioningTests
    {
        private static FlowBedConfiguration Cama() => new FlowBedConfiguration
        {
            BedType = FlowBedType.Dynamic,
            LaneDepth = 96.0,
            PalletDepth = 48.0,
            RollerId = "ROLLER_X"
        };

        private static LargueroDesign Larguero() => new LargueroDesign
        {
            Name = "L1",
            BeamProfileId = "BEAM_X",
            Peralte = 4.0,
            Length = 96.0
        };

        private static RackEmbedDocument Envelope(string schemaVersion) => new RackEmbedDocument
        {
            SchemaVersion = schemaVersion,
            Kind = RackEmbedDocument.KindCama,
            Id = "id-1",
            Name = "Rack A",
            Design = "{\"any\":1}"
        };

        // --- Envelope readability gate (tolerant: null, never throws) ---

        [Fact]
        public void Envelope_FutureMajor_DeserializesToNull()
        {
            var store = new RackEmbedStore();
            var json = store.Serialize(Envelope("2.0")); // a newer MAJOR than the current envelope schema (1.x)

            Assert.Null(store.Deserialize(json)); // fails before fix: the store had no version gate and returned the doc
        }

        [Fact]
        public void Envelope_SameMajorFuture_RemainsReadable()
        {
            // A newer MINOR of the SAME major is forward-compatible: it must still load (additive fields ignored).
            var store = new RackEmbedStore();
            var back = store.Deserialize(store.Serialize(Envelope("1.9")));

            Assert.NotNull(back);
            Assert.Equal("1.9", back.SchemaVersion);
            Assert.Equal(RackEmbedDocument.KindCama, back.Kind);
        }

        [Fact]
        public void Envelope_InvalidJson_StillNull_WithoutThrowing()
        {
            var store = new RackEmbedStore();
            Assert.Null(store.Deserialize("no soy json"));
            Assert.Null(store.Deserialize(""));
            Assert.Null(store.Deserialize(null));
        }

        // --- No schema-version DOWNGRADE on re-save ---

        [Fact]
        public void Wrapper_FutureMinorSameMajor_VersionAndUnknownPreservedOnResave()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForCama(Cama()))).AsObject();
            node["SchemaVersion"] = "2.5"; // newer MINOR of the same wrapper major (2.x)
            node["futureWrapperField"] = 7;

            var back = store.Serialize(store.Deserialize(node.ToJsonString()));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.5", doc.RootElement.GetProperty("SchemaVersion").GetString()); // fails before fix: re-stamped "2.0"
            Assert.True(doc.RootElement.TryGetProperty("futureWrapperField", out _));
        }

        [Fact]
        public void FlowBed_FutureMinorSameMajor_VersionPreservedOnResave()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForCama(Cama()))).AsObject();
            node["FlowBed"].AsObject()["SchemaVersion"] = "1.5";
            node["FlowBed"].AsObject()["futureBedField"] = "x";

            var back = store.Serialize(store.Deserialize(node.ToJsonString()));

            using var doc = JsonDocument.Parse(back);
            var flowBed = doc.RootElement.GetProperty("FlowBed");
            Assert.Equal("1.5", flowBed.GetProperty("SchemaVersion").GetString()); // fails before fix: re-stamped "1.0"
            Assert.True(flowBed.TryGetProperty("futureBedField", out _));
        }

        [Fact]
        public void Larguero_FutureMinorSameMajor_VersionPreservedOnResave()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForLarguero(Larguero()))).AsObject();
            node["Larguero"].AsObject()["SchemaVersion"] = "1.5";
            node["Larguero"].AsObject()["futureLargueroField"] = true;

            var back = store.Serialize(store.Deserialize(node.ToJsonString()));

            using var doc = JsonDocument.Parse(back);
            var larguero = doc.RootElement.GetProperty("Larguero");
            Assert.Equal("1.5", larguero.GetProperty("SchemaVersion").GetString()); // fails before fix: re-stamped "1.0"
            Assert.True(larguero.TryGetProperty("futureLargueroField", out _));
        }

        [Fact]
        public void FreshProject_WritesCurrentPayloadVersions_NoSource()
        {
            // With no source document, a fresh save stamps the current versions (characterization; stays green).
            var store = new RackProjectStore();
            using (var camaDoc = JsonDocument.Parse(store.Serialize(RackProject.ForCama(Cama()))))
            {
                Assert.Equal("1.0", camaDoc.RootElement.GetProperty("FlowBed").GetProperty("SchemaVersion").GetString());
                Assert.Equal("2.0", camaDoc.RootElement.GetProperty("SchemaVersion").GetString());
            }
        }
    }
}
