using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-11 (persistencia uniforme). Two groups of tests:
    ///
    /// (A) CHARACTERIZATION — freeze the CURRENT observable behavior that I-11 must preserve (legacy flat reads,
    ///     tolerant junk, round-trip of known fields). These pass today and must stay green after F3.
    ///
    /// (B) PRESERVATION / VERSIONING — the NEW guarantees I-11 adds: every payload writes a flat <c>SchemaVersion</c>,
    ///     a higher MAJOR is rejected, and UNKNOWN JSON fields survive a load/save at the four I-11 boundaries
    ///     (RackEmbedDocument, RackProjectDocument, FlowBedDocument, LargueroDocument). These are written against the
    ///     EXISTING public store APIs (injecting unknown fields via <see cref="JsonNode"/> and inspecting the
    ///     re-serialized output) so the suite compiles today; they FAIL before F3 (the current code drops the unknown
    ///     fields / writes no version / does not reject) and PASS after it.
    /// </summary>
    public class PersistenceUniformityTests
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
            Length = 96.0,
            MensulaOverride = "MENSULA_Y"
        };

        // ----------------------------------------------------------------------------------------------------
        // (A) Characterization — current behavior I-11 must keep
        // ----------------------------------------------------------------------------------------------------

        [Fact]
        public void FlowBed_LegacyFlatJson_WithoutSchemaVersion_LoadsKnownFields()
        {
            // The current on-disk shape is a flat POCO with NO SchemaVersion; it must remain loadable after versioning.
            var config = new FlowBedConfigurationStore()
                .Deserialize("{\"BedType\":\"Pushback\",\"LaneDepth\":120,\"PalletDepth\":0,\"RollerId\":\"ROLLER_Z\"}");

            Assert.NotNull(config);
            Assert.Equal(FlowBedType.Pushback, config.BedType);
            Assert.Equal(120.0, config.LaneDepth, 4);
            Assert.Equal(0.0, config.PalletDepth, 4);
            Assert.Equal("ROLLER_Z", config.RollerId);
        }

        [Fact]
        public void Larguero_LegacyFlatPayload_WithoutInnerSchemaVersion_LoadsKnownFields()
        {
            // A larguero wrapper whose Larguero payload has no inner SchemaVersion (today's shape) still loads.
            var json = "{\"schemaVersion\":\"2.0\",\"kind\":\"Larguero\",\"larguero\":{" +
                       "\"name\":\"L1\",\"beamProfileId\":\"BEAM_X\",\"peralte\":4,\"length\":96,\"mensulaOverride\":\"M\"}}";

            var project = new RackProjectStore().Deserialize(json);

            Assert.Equal(RackSystemKind.Larguero, project.Kind);
            Assert.NotNull(project.Larguero);
            Assert.Equal("BEAM_X", project.Larguero.BeamProfileId);
            Assert.Equal(96.0, project.Larguero.Length, 4);
            Assert.Equal("M", project.Larguero.MensulaOverride);
        }

        [Fact]
        public void Envelope_LegacyWithoutHigherVersion_RemainsReadable()
        {
            var embed = new RackEmbedStore()
                .Deserialize("{\"schemaVersion\":\"1.0\",\"kind\":\"cama\",\"id\":\"g\",\"name\":\"n\",\"design\":\"{}\"}");

            Assert.NotNull(embed);
            Assert.Equal(RackEmbedDocument.KindCama, embed.Kind);
            Assert.Equal("g", embed.Id);
            Assert.Equal("n", embed.Name);
        }

        [Fact]
        public void FlowBed_EmptyObject_StaysAbsent_And_PushbackZeroPalletDepth_StaysValid()
        {
            // {} = absent cama (tolerant); Pushback with PalletDepth 0 is a real bed. Both are current contracts.
            Assert.Null(new FlowBedConfigurationStore().Deserialize("{}"));
            Assert.NotNull(new FlowBedConfigurationStore().Deserialize("{\"laneDepth\":120,\"palletDepth\":0}"));
        }

        // ----------------------------------------------------------------------------------------------------
        // (B) Versioning — every payload writes a flat SchemaVersion (FAILS before F3)
        // ----------------------------------------------------------------------------------------------------

        [Fact]
        public void FlowBed_Serialized_InProjectWrapper_WritesFlatSchemaVersion()
        {
            var json = new RackProjectStore().Serialize(RackProject.ForCama(Cama()));

            using var doc = JsonDocument.Parse(json);
            var flowBed = doc.RootElement.GetProperty("FlowBed");
            Assert.True(flowBed.TryGetProperty("SchemaVersion", out var version),
                "FlowBed payload must carry an explicit SchemaVersion (I-11).");
            Assert.Equal("1.0", version.GetString());
            // Still flat: the known field names/casing are unchanged (no Configuration node).
            Assert.True(flowBed.TryGetProperty("LaneDepth", out _));
            Assert.True(flowBed.TryGetProperty("BedType", out _));
        }

        [Fact]
        public void FlowBed_Serialized_ByEmbedStore_WritesFlatSchemaVersion()
        {
            var json = new FlowBedConfigurationStore().Serialize(Cama());

            using var doc = JsonDocument.Parse(json);
            Assert.True(doc.RootElement.TryGetProperty("SchemaVersion", out var version),
                "The embed (DWG) FlowBed JSON must carry an explicit SchemaVersion (I-11).");
            Assert.Equal("1.0", version.GetString());
            Assert.True(doc.RootElement.TryGetProperty("LaneDepth", out _));
        }

        [Fact]
        public void Larguero_Serialized_InProjectWrapper_WritesFlatSchemaVersion()
        {
            var json = new RackProjectStore().Serialize(RackProject.ForLarguero(Larguero()));

            using var doc = JsonDocument.Parse(json);
            var larguero = doc.RootElement.GetProperty("Larguero");
            Assert.True(larguero.TryGetProperty("SchemaVersion", out var version),
                "Larguero payload must carry an explicit SchemaVersion (I-11).");
            Assert.Equal("1.0", version.GetString());
            Assert.True(larguero.TryGetProperty("BeamProfileId", out _));
        }

        // ----------------------------------------------------------------------------------------------------
        // (B) SchemaGuard — a higher MAJOR payload version is rejected (FAILS before F3)
        // ----------------------------------------------------------------------------------------------------

        [Fact]
        public void FlowBed_FutureMajorSchemaVersion_IsRejected()
        {
            var node = JsonNode.Parse(new RackProjectStore().Serialize(RackProject.ForCama(Cama()))).AsObject();
            node["FlowBed"].AsObject()["SchemaVersion"] = "99.0";

            var ex = Assert.Throws<InvalidOperationException>(
                () => new RackProjectStore().Deserialize(node.ToJsonString()));
            Assert.Contains("más nueva", ex.Message);
        }

        [Fact]
        public void Larguero_FutureMajorSchemaVersion_IsRejected()
        {
            var node = JsonNode.Parse(new RackProjectStore().Serialize(RackProject.ForLarguero(Larguero()))).AsObject();
            node["Larguero"].AsObject()["SchemaVersion"] = "99.0";

            var ex = Assert.Throws<InvalidOperationException>(
                () => new RackProjectStore().Deserialize(node.ToJsonString()));
            Assert.Contains("más nueva", ex.Message);
        }

        // ----------------------------------------------------------------------------------------------------
        // (B) Unknown-field preservation at the four I-11 boundaries (FAILS before F3)
        // ----------------------------------------------------------------------------------------------------

        [Fact]
        public void Envelope_UnknownField_SurvivesDeserializeSerialize()
        {
            var store = new RackEmbedStore();
            var node = JsonNode.Parse(store.Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindCama,
                Id = "id-1",
                Name = "Rack A",
                Design = "{\"any\":1}"
            })).AsObject();
            node["futureEnvField"] = 5;

            var back = store.Serialize(store.Deserialize(node.ToJsonString()));

            using var doc = JsonDocument.Parse(back);
            Assert.True(doc.RootElement.TryGetProperty("futureEnvField", out var value),
                "An unknown envelope field must survive a round-trip (I-11).");
            Assert.Equal(5, value.GetInt32());
        }

        [Fact]
        public void Envelope_UnknownField_SurvivesNameAndIdChange()
        {
            var store = new RackEmbedStore();
            var node = JsonNode.Parse(store.Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindCama,
                Id = "id-1",
                Name = "Rack A",
                Design = "{\"any\":1}"
            })).AsObject();
            node["futureEnvField"] = "keep-me";

            var embed = store.Deserialize(node.ToJsonString());
            embed.Id = "id-2";
            embed.Name = "Rack B";
            var back = store.Serialize(embed);

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("id-2", doc.RootElement.GetProperty("Id").GetString());
            Assert.Equal("Rack B", doc.RootElement.GetProperty("Name").GetString());
            Assert.True(doc.RootElement.TryGetProperty("futureEnvField", out var value),
                "The unknown envelope field must survive an identity re-stamp (I-11 / restamp path).");
            Assert.Equal("keep-me", value.GetString());
        }

        [Fact]
        public void ProjectWrapper_TopLevelUnknownField_SurvivesLoadSave()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForCama(Cama()))).AsObject();
            node["futureWrapperField"] = 123;

            var back = store.Serialize(store.Deserialize(node.ToJsonString()));

            using var doc = JsonDocument.Parse(back);
            Assert.True(doc.RootElement.TryGetProperty("futureWrapperField", out var value),
                "An unknown wrapper-level field must survive a library load/save (I-11).");
            Assert.Equal(123, value.GetInt32());
        }

        [Fact]
        public void FlowBedDocument_UnknownField_SurvivesLoadSave()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForCama(Cama()))).AsObject();
            node["FlowBed"].AsObject()["futureBedField"] = "keep-me";

            var back = store.Serialize(store.Deserialize(node.ToJsonString()));

            using var doc = JsonDocument.Parse(back);
            var flowBed = doc.RootElement.GetProperty("FlowBed");
            Assert.True(flowBed.TryGetProperty("futureBedField", out var value),
                "An unknown FlowBedDocument field must survive a load/save (I-11).");
            Assert.Equal("keep-me", value.GetString());
            // The edited/known field is still written correctly alongside the preserved unknown.
            Assert.Equal(96.0, flowBed.GetProperty("LaneDepth").GetDouble(), 4);
        }

        [Fact]
        public void LargueroDocument_UnknownField_SurvivesLoadSave()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForLarguero(Larguero()))).AsObject();
            node["Larguero"].AsObject()["futureLargueroField"] = true;

            var back = store.Serialize(store.Deserialize(node.ToJsonString()));

            using var doc = JsonDocument.Parse(back);
            var larguero = doc.RootElement.GetProperty("Larguero");
            Assert.True(larguero.TryGetProperty("futureLargueroField", out var value),
                "An unknown LargueroDocument field must survive a load/save (I-11).");
            Assert.True(value.GetBoolean());
            Assert.Equal("BEAM_X", larguero.GetProperty("BeamProfileId").GetString());
        }

        [Fact]
        public void ProjectWrapper_LoadSave_DoesNotResurrectInactiveKnownPayloads()
        {
            // Preserving unknown metadata must NOT drag along an inactive known payload slot from the source: a cama
            // wrapper carrying a stray Header must still re-save with Header = null (only the active payload is written).
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForCama(Cama()))).AsObject();
            node["Header"] = new JsonObject { ["Name"] = "stray", ["Height"] = 10.0 };
            node["futureWrapperField"] = 1; // an unknown that SHOULD survive

            var back = store.Serialize(store.Deserialize(node.ToJsonString()));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("Header").ValueKind);
            Assert.True(doc.RootElement.TryGetProperty("futureWrapperField", out _));
        }
    }
}
