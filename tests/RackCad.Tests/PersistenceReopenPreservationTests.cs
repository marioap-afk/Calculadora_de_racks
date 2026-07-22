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
    /// I-11 (changes-required round): the reusable mechanisms that make preservation REAL when a document is
    /// RECONSTRUCTED (not just round-tripped) — the pure <see cref="SchemaVersionPolicy"/>, the pure
    /// <see cref="RackEmbedComposer"/> (envelope factory that inherits metadata + a non-downgraded version), and the
    /// library <see cref="RackProject.WithSourceMetadataFrom"/> sidecar the UI editors use so an edited design keeps the
    /// unknown fields and schema version of the project it was opened from (cama, larguero, and the dynamic / cabecera
    /// wrappers). The Plugin edit-redraw wiring (per-block source embeds, new-view inheritance) composes these same
    /// helpers and is covered by owner-validation, not by this AutoCAD-free suite.
    /// </summary>
    public class PersistenceReopenPreservationTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem DynamicSystem()
            => new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0);

        private static RackFrameConfiguration SelectiveHeader()
            => new HardcodedStandardRackFrameService(Catalog).CreateDefault();

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

        // ---------------------------------------------------------------------------------------------
        // SchemaVersionPolicy — the shared, tested version rules
        // ---------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(null, "1.0", true)]        // missing => legacy => readable
        [InlineData("garbage", "1.0", true)]   // unparseable => legacy => readable
        [InlineData("1.0", "1.0", true)]       // equal
        [InlineData("1.9", "1.0", true)]       // same major, newer minor => readable
        [InlineData("2.0", "1.0", false)]      // higher major => NOT readable
        [InlineData("0.5", "1.0", true)]       // major 0 => legacy 1 => readable
        public void IsReadable_OnlyHigherMajorIsUnreadable(string stored, string current, bool expected)
            => Assert.Equal(expected, SchemaVersionPolicy.IsReadable(stored, current));

        [Theory]
        [InlineData(null, "1.0", "1.0")]       // missing => current
        [InlineData("garbage", "1.0", "1.0")]  // unparseable => current (never copy junk verbatim)
        [InlineData("1.0", "2.0", "2.0")]      // older major => current (upgrade)
        [InlineData("1.5", "1.0", "1.5")]      // same major, newer minor => keep stored (no downgrade)
        [InlineData("1.0", "1.5", "1.5")]      // same major, older minor => current
        [InlineData("1.0", "1.0", "1.0")]      // equal => current
        public void ResolveWriteVersion_NeverDowngrades_NeverCopiesJunk(string stored, string current, string expected)
            => Assert.Equal(expected, SchemaVersionPolicy.ResolveWriteVersion(stored, current));

        // ---------------------------------------------------------------------------------------------
        // RackEmbedComposer — updates identity/view/design while inheriting metadata + version
        // ---------------------------------------------------------------------------------------------

        private static RackEmbedDocument SourceEnvelope(RackEmbedStore store, string schemaVersion, string view, int unknownValue)
        {
            var node = JsonNode.Parse(store.Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindSelective,
                Id = "old-id",
                Name = "Old",
                View = view,
                Section = 0,
                Design = "{\"old\":1}"
            })).AsObject();
            node["SchemaVersion"] = schemaVersion;
            node["futureEnv"] = unknownValue;
            return store.Deserialize(node.ToJsonString());
        }

        [Fact]
        public void Composer_UpdatesIdentityViewSectionDesign_WithoutLosingMetadataOrDowngradingVersion()
        {
            var store = new RackEmbedStore();
            var source = SourceEnvelope(store, "1.7", RackEmbedDocument.ViewFrontal, 9); // same major, newer minor

            var composed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindSelective, "new-id", "New", RackEmbedDocument.ViewLateral, 3, "{\"d\":2}");

            Assert.Equal("new-id", composed.Id);
            Assert.Equal("New", composed.Name);
            Assert.Equal(RackEmbedDocument.ViewLateral, composed.View);
            Assert.Equal(3, composed.Section);
            Assert.Equal("{\"d\":2}", composed.Design);
            Assert.Equal("1.7", composed.SchemaVersion); // not downgraded to 1.0

            using var doc = JsonDocument.Parse(store.Serialize(composed));
            Assert.Equal("new-id", doc.RootElement.GetProperty("Id").GetString());
            Assert.Equal("1.7", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal(9, doc.RootElement.GetProperty("futureEnv").GetInt32());
        }

        [Fact]
        public void Composer_TwoViews_KeepTheirOwnExtensionData()
        {
            var store = new RackEmbedStore();
            var frontalSource = SourceEnvelope(store, "1.0", RackEmbedDocument.ViewFrontal, 1);
            var lateralSource = SourceEnvelope(store, "1.0", RackEmbedDocument.ViewLateral, 2);

            var frontal = RackEmbedComposer.Compose(frontalSource, RackEmbedDocument.KindSelective, "id", "n", RackEmbedDocument.ViewFrontal, 0, "{}");
            var lateral = RackEmbedComposer.Compose(lateralSource, RackEmbedDocument.KindSelective, "id", "n", RackEmbedDocument.ViewLateral, 1, "{}");

            using var frontalDoc = JsonDocument.Parse(store.Serialize(frontal));
            using var lateralDoc = JsonDocument.Parse(store.Serialize(lateral));
            Assert.Equal(1, frontalDoc.RootElement.GetProperty("futureEnv").GetInt32()); // each view keeps its own value
            Assert.Equal(2, lateralDoc.RootElement.GetProperty("futureEnv").GetInt32());
        }

        [Fact]
        public void Composer_NullSource_WritesCurrentVersion_NoExtensionData()
        {
            var store = new RackEmbedStore();
            var composed = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindCama, "id", "n", RackEmbedDocument.ViewFrontal, -1, "{}");

            Assert.Equal(RackEmbedDocument.CurrentSchemaVersion, composed.SchemaVersion);
            Assert.Null(composed.ExtensionData);
            using var doc = JsonDocument.Parse(store.Serialize(composed));
            Assert.False(doc.RootElement.TryGetProperty("futureEnv", out _));
        }

        // ---------------------------------------------------------------------------------------------
        // Library sidecar — an edited design serialized WITH the source project's metadata
        // ---------------------------------------------------------------------------------------------

        [Fact]
        public void Library_CamaSave_WithSource_PreservesWrapperAndPayloadUnknowns_AndWritesEdit()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForCama(Cama()))).AsObject();
            node["SchemaVersion"] = "2.3";
            node["futureWrapper"] = "w";
            node["FlowBed"].AsObject()["SchemaVersion"] = "1.4";
            node["FlowBed"].AsObject()["futureBed"] = "b";
            var loaded = store.Deserialize(node.ToJsonString());

            // Simulate the UI edit: a fresh known model + the source project's metadata.
            var edited = new FlowBedConfiguration { BedType = FlowBedType.Pushback, LaneDepth = 200.0, PalletDepth = 0.0, RollerId = "ROLLER_NEW" };
            var back = store.Serialize(RackProject.ForCama(edited).WithSourceMetadataFrom(loaded));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.3", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.True(doc.RootElement.TryGetProperty("futureWrapper", out _));
            var flowBed = doc.RootElement.GetProperty("FlowBed");
            Assert.Equal("1.4", flowBed.GetProperty("SchemaVersion").GetString());
            Assert.True(flowBed.TryGetProperty("futureBed", out _));
            Assert.Equal(200.0, flowBed.GetProperty("LaneDepth").GetDouble(), 4); // the edit is written
            Assert.Equal("ROLLER_NEW", flowBed.GetProperty("RollerId").GetString());
        }

        [Fact]
        public void Library_LargueroSave_WithSource_PreservesWrapperAndPayloadUnknowns_AndWritesEdit()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForLarguero(Larguero()))).AsObject();
            node["SchemaVersion"] = "2.2";
            node["futureWrapper"] = 1;
            node["Larguero"].AsObject()["SchemaVersion"] = "1.6";
            node["Larguero"].AsObject()["futureLarguero"] = true;
            var loaded = store.Deserialize(node.ToJsonString());

            var edited = new LargueroDesign { Name = "L2", BeamProfileId = "BEAM_Z", Peralte = 5.0, Length = 120.0 };
            var back = store.Serialize(RackProject.ForLarguero(edited).WithSourceMetadataFrom(loaded));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.2", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.True(doc.RootElement.TryGetProperty("futureWrapper", out _));
            var larguero = doc.RootElement.GetProperty("Larguero");
            Assert.Equal("1.6", larguero.GetProperty("SchemaVersion").GetString());
            Assert.True(larguero.TryGetProperty("futureLarguero", out _));
            Assert.Equal("BEAM_Z", larguero.GetProperty("BeamProfileId").GetString());
            Assert.Equal(120.0, larguero.GetProperty("Length").GetDouble(), 4);
        }

        [Fact]
        public void Library_DynamicWrapper_WithSource_PreservesWrapperUnknownAndVersion()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForDynamic(DynamicSystem()))).AsObject();
            node["SchemaVersion"] = "2.4";
            node["futureWrapper"] = 42;
            var loaded = store.Deserialize(node.ToJsonString());

            var back = store.Serialize(
                RackProject.ForDynamic(loaded.DynamicDesign, loaded.DynamicSystem).WithSourceMetadataFrom(loaded));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.4", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal(42, doc.RootElement.GetProperty("futureWrapper").GetInt32());
        }

        [Fact]
        public void Library_CabeceraWrapper_WithSource_PreservesWrapperUnknownAndVersion()
        {
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForSelective(SelectiveHeader()))).AsObject();
            node["SchemaVersion"] = "2.6";
            node["futureWrapper"] = "keep";
            var loaded = store.Deserialize(node.ToJsonString());

            var back = store.Serialize(RackProject.ForSelective(loaded.Header).WithSourceMetadataFrom(loaded));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.6", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("keep", doc.RootElement.GetProperty("futureWrapper").GetString());
        }

        [Fact]
        public void Library_CabeceraWrapper_WithClonedHeader_PreservesWrapperUnknownAndVersion()
        {
            // I-17 x I-11 regression: the header configurator edits a DEEP CLONE of the loaded header
            // (RackFrameProjectStore.DeepCopy, the single canonical clone) and re-saves it via WithSourceMetadataFrom.
            // Cloning the header must NOT interfere with wrapper-level metadata preservation: the unknown wrapper field
            // and the non-downgraded schema version must still survive the round-trip (they ride on the RackProject's
            // SourceDocument, independent of the header). I-11 policy and the DTOs are untouched.
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForSelective(SelectiveHeader()))).AsObject();
            node["SchemaVersion"] = "2.6";
            node["futureWrapper"] = "keep";
            var loaded = store.Deserialize(node.ToJsonString());

            // The editor never edits the loaded object in place — it clones it first (I-17's canonical DeepCopy).
            var editedHeader = new RackFrameProjectStore().DeepCopy(loaded.Header);
            Assert.NotSame(loaded.Header, editedHeader);
            var back = store.Serialize(RackProject.ForSelective(editedHeader).WithSourceMetadataFrom(loaded));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.6", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("keep", doc.RootElement.GetProperty("futureWrapper").GetString());
        }

        [Fact]
        public void Library_SelectiveRackWrapper_WithSource_PreservesWrapperUnknownAndVersion()
        {
            // Opening a SelectiveRack from the library reconstructs its WRAPPER RackProjectDocument on save — a boundary,
            // even though the inner SelectivePalletDesignDocument is not one of the four. The wrapper metadata must survive.
            var store = new RackProjectStore();
            var design = new SelectivePalletDesign { PostId = "POST_P", PalletDepth = 48.0 };
            design.Bays.Add(new SelectiveBayDesign());
            var document = SelectivePalletDesignDocument.From(design, "id-1", "Rack A");

            var node = JsonNode.Parse(store.Serialize(RackProject.ForSelectiveRack(document))).AsObject();
            node["SchemaVersion"] = "2.7";
            node["futureWrapper"] = "keep";
            var loaded = store.Deserialize(node.ToJsonString());

            var back = store.Serialize(RackProject.ForSelectiveRack(loaded.SelectiveRack).WithSourceMetadataFrom(loaded));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal("2.7", doc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("keep", doc.RootElement.GetProperty("futureWrapper").GetString());
        }

        [Fact]
        public void Library_Save_WithSource_DoesNotResurrectInactiveKnownPayload()
        {
            // Source carries a stray Header alongside the cama; a with-source re-save must NOT resurrect it.
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForCama(Cama()))).AsObject();
            node["Header"] = new JsonObject { ["Name"] = "stray", ["Height"] = 10.0 };
            node["futureWrapper"] = 1;
            var loaded = store.Deserialize(node.ToJsonString());

            var back = store.Serialize(RackProject.ForCama(Cama()).WithSourceMetadataFrom(loaded));

            using var doc = JsonDocument.Parse(back);
            Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("Header").ValueKind);
            Assert.True(doc.RootElement.TryGetProperty("futureWrapper", out _));
        }
    }
}
