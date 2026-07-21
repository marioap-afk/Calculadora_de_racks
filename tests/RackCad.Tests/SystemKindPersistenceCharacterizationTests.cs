using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// F1 characterization for I-08 (System Registry). Freezes the CURRENT observable persistence behavior of the five
    /// <see cref="RackSystemKind"/> values BEFORE the registry refactor, so F2 cannot drift the wire format, the legacy
    /// fallbacks, the unknown-kind behavior or the validation rules unnoticed. These tests assert only what
    /// <c>origin/main</c> actually does today; they do not change or "fix" behavior. Round-trip payload preservation, the
    /// legacy bare-header path, the empty-object rejection and the future-schema rejection are already covered by
    /// <see cref="RackProjectStoreTests"/> and <see cref="RackProjectStorePerKindTests"/> and are not duplicated here.
    /// </summary>
    public class SystemKindPersistenceCharacterizationTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem DynamicSystem()
        {
            return new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0);
        }

        private static RackFrameConfiguration SelectiveHeader()
        {
            return new HardcodedStandardRackFrameService(Catalog).CreateDefault();
        }

        private static SelectivePalletDesignDocument SelectiveRackDoc()
        {
            var design = new SelectivePalletDesign { PostId = "POST_P", PalletDepth = 48.0 };
            design.Bays.Add(new SelectiveBayDesign());
            return SelectivePalletDesignDocument.From(design, "id-1", "Rack A");
        }

        private static FlowBedConfiguration Cama() =>
            new FlowBedConfiguration { BedType = FlowBedType.Pushback, LaneDepth = 120.0, PalletDepth = 0.0, RollerId = "ROLLER_Z" };

        private static LargueroDesign Larguero() =>
            new LargueroDesign { Name = "L1", BeamProfileId = "BEAM_X", Peralte = 0.0, Length = 0.0 };

        private RackProject ProjectFor(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.Selective: return RackProject.ForSelective(SelectiveHeader());
                case RackSystemKind.PalletFlow: return RackProject.ForDynamic(DynamicSystem());
                case RackSystemKind.SelectiveRack: return RackProject.ForSelectiveRack(SelectiveRackDoc());
                case RackSystemKind.Cama: return RackProject.ForCama(Cama());
                case RackSystemKind.Larguero: return RackProject.ForLarguero(Larguero());
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        // --- On-disk wire format (schemaVersion, kind discriminator, payload keys, casing, structure) ---

        // The wrapper property NAMES, their exact casing, their order, the schema version and the kind discriminator
        // string are the file-format contract. There is no JsonNamingPolicy and no [JsonPropertyName] anywhere in
        // Persistence, so the on-disk names are the C# PascalCase names verbatim; the kind value is the enum MEMBER name
        // (JsonStringEnumConverter). Null payload slots are written (no DefaultIgnoreCondition). Freezing all of this
        // catches an accidental naming-policy, rename, reorder or discriminator change during the registry refactor.
        [Theory]
        [InlineData(RackSystemKind.Selective, "Selective", "Header")]
        [InlineData(RackSystemKind.PalletFlow, "PalletFlow", "DynamicSystem")]
        [InlineData(RackSystemKind.SelectiveRack, "SelectiveRack", "SelectiveRack")]
        [InlineData(RackSystemKind.Cama, "Cama", "FlowBed")]
        [InlineData(RackSystemKind.Larguero, "Larguero", "Larguero")]
        public void Serialize_EachKind_FreezesSchemaKindDiscriminatorAndPayloadShape(
            RackSystemKind kind, string expectedDiscriminator, string activePayloadKey)
        {
            var json = new RackProjectStore().Serialize(ProjectFor(kind));

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Exact wrapper keys, in declaration order, PascalCase.
            Assert.Equal(
                new[] { "SchemaVersion", "Kind", "Header", "DynamicSystem", "SelectiveRack", "FlowBed", "Larguero" },
                root.EnumerateObject().Select(p => p.Name).ToArray());

            Assert.Equal("2.0", root.GetProperty("SchemaVersion").GetString());
            Assert.Equal(expectedDiscriminator, root.GetProperty("Kind").GetString());

            // The active payload is an object; every other payload slot is written as null.
            Assert.Equal(JsonValueKind.Object, root.GetProperty(activePayloadKey).ValueKind);
            foreach (var key in new[] { "Header", "DynamicSystem", "SelectiveRack", "FlowBed", "Larguero" })
            {
                if (key != activePayloadKey)
                {
                    Assert.Equal(JsonValueKind.Null, root.GetProperty(key).ValueKind);
                }
            }
        }

        // Derived physical members are NOT persisted (the header DTO omits them) and are rebuilt on load. Freezing both
        // halves guards the "reconstrucción de miembros físicos derivados" contract.
        [Fact]
        public void Serialize_SelectiveHeader_OmitsDerivedMembers_AndLoadRebuildsThem()
        {
            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForSelective(SelectiveHeader()));

            using (var doc = JsonDocument.Parse(json))
            {
                var header = doc.RootElement.GetProperty("Header");
                Assert.Equal(JsonValueKind.Object, header.ValueKind);
                Assert.False(header.TryGetProperty("Members", out _)); // derived, intentionally not persisted
            }

            var loaded = store.Deserialize(json);
            Assert.Equal(RackSystemKind.Selective, loaded.Kind);
            Assert.NotEmpty(loaded.Header.Members); // rebuilt on load
        }

        // --- Wrapper that declares a kind but omits its payload: corrupt/truncated, fails clearly for EVERY kind ---

        [Theory]
        [InlineData("Selective")]
        [InlineData("PalletFlow")]
        [InlineData("SelectiveRack")]
        [InlineData("Cama")]
        [InlineData("Larguero")]
        public void Deserialize_KindWithoutPayload_ThrowsForEveryKind(string kind)
        {
            Assert.Throws<InvalidOperationException>(
                () => new RackProjectStore().Deserialize("{\"kind\":\"" + kind + "\"}"));
        }

        // --- Unknown / invalid kind: characterize the CURRENT behavior exactly (do not change it) ---

        // Current behavior: the value is deserialized into the RackSystemKind enum via JsonStringEnumConverter; an
        // unrecognized name makes System.Text.Json throw, which the store wraps as an InvalidOperationException whose
        // message reports it as invalid JSON. This is characterized as-is; whether that message should distinguish
        // "unknown kind" from "malformed JSON" is left for F2 (see the report's ambiguities section), NOT changed here.
        [Fact]
        public void Deserialize_UnknownKind_ThrowsInvalidOperation_ReportedAsInvalidJson()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new RackProjectStore().Deserialize("{\"kind\":\"NoSuchKind\"}"));

            Assert.Contains("no es un JSON valido", ex.Message);
        }

        // --- Undefined NUMERIC kind (e.g. 999): characterizes the CURRENT behavior, distinct from an unknown STRING ---

        // The store's JsonStringEnumConverter defaults to allowIntegerValues=true, so an integer that is not a defined
        // RackSystemKind member is ACCEPTED as (RackSystemKind)999 with no exception. BuildProject then matches no case
        // and falls to the DEFAULT branch, which treats the file as a header (Selective): with a valid Header it loads as
        // a cabecera and the numeric kind is normalized away (a re-save writes "Selective"). Frozen as-is, NOT fixed.
        [Fact]
        public void Deserialize_NumericUndefinedKind_WithValidHeader_IsTreatedAsSelectiveHeaderViaDefault()
        {
            var store = new RackProjectStore();

            var node = JsonNode.Parse(store.Serialize(RackProject.ForSelective(SelectiveHeader()))).AsObject();
            Assert.Equal("Selective", node["Kind"].GetValue<string>()); // sanity: we start from a header wrapper
            node["Kind"] = 999; // an undefined enum NUMBER
            var json999 = node.ToJsonString();

            var loaded = store.Deserialize(json999);

            // The unknown number is routed through the default/header path and normalized to Selective, not preserved.
            Assert.Equal(RackSystemKind.Selective, loaded.Kind);
            Assert.NotNull(loaded.Header);
            Assert.NotEmpty(loaded.Header.Members); // header members rebuilt on load

            // 999 does not survive a load/save cycle: re-serialization writes the Selective discriminator.
            using var reDoc = JsonDocument.Parse(store.Serialize(loaded));
            Assert.Equal("Selective", reDoc.RootElement.GetProperty("Kind").GetString());
        }

        // With NO usable payload, the same default/header route reaches IsUsableHeader(null) and fails with the DEGENERATE
        // "cabecera" message — NOT the "no es un JSON valido" message an unknown STRING kind produces.
        [Fact]
        public void Deserialize_NumericUndefinedKind_WithoutPayload_ThrowsDegenerateCabecera()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new RackProjectStore().Deserialize("{\"schemaVersion\":\"2.0\",\"kind\":999}"));

            Assert.Contains("cabecera", ex.Message);
            Assert.DoesNotContain("no es un JSON valido", ex.Message);
        }

        // Explicit contrast: an undefined NUMBER is accepted and routed to the header default, whereas an unknown STRING
        // name is rejected up front as invalid JSON. Same input shape (kind + header), opposite outcome.
        [Fact]
        public void UndefinedNumericKind_DiffersFromUnknownStringKind()
        {
            var store = new RackProjectStore();

            var node = JsonNode.Parse(store.Serialize(RackProject.ForSelective(SelectiveHeader()))).AsObject();
            node["Kind"] = 999;
            var numericLoaded = store.Deserialize(node.ToJsonString()); // no throw
            Assert.Equal(RackSystemKind.Selective, numericLoaded.Kind);

            var stringEx = Assert.Throws<InvalidOperationException>(
                () => store.Deserialize("{\"kind\":\"NoSuchKind\",\"header\":{}}"));
            Assert.Contains("no es un JSON valido", stringEx.Message);
        }

        // --- Current per-type validation rules (RackDesignValidation), including the two deliberately-loose ones ---

        [Fact]
        public void IsUsableHeader_RequiresPositiveHeightDepthAndBothPosts()
        {
            Assert.True(RackDesignValidation.IsUsableHeader(new RackFrameConfiguration
            {
                Height = 100.0,
                Depth = 54.0,
                LeftPost = new PostAssembly(),
                RightPost = new PostAssembly()
            }));

            Assert.False(RackDesignValidation.IsUsableHeader(null));
            Assert.False(RackDesignValidation.IsUsableHeader(new RackFrameConfiguration
            {
                Height = 0.0,
                Depth = 54.0,
                LeftPost = new PostAssembly(),
                RightPost = new PostAssembly()
            }));
            Assert.False(RackDesignValidation.IsUsableHeader(new RackFrameConfiguration
            {
                Height = 100.0,
                Depth = 54.0,
                LeftPost = null,
                RightPost = new PostAssembly()
            }));
        }

        [Fact]
        public void IsUsableSelective_RequiresAtLeastOneBay()
        {
            Assert.True(RackDesignValidation.IsUsableSelective(SelectiveRackDoc()));
            Assert.False(RackDesignValidation.IsUsableSelective(
                SelectivePalletDesignDocument.From(new SelectivePalletDesign { PostId = "P" }, "id", "n")));
            Assert.False(RackDesignValidation.IsUsableSelective(null));
        }

        [Fact]
        public void IsUsableFlowBed_OnlyLaneDepthRequired_PalletDepthZeroStaysValid()
        {
            // Pushback beds legitimately have PalletDepth 0; requiring it would wrongly reject a whole valid sub-type.
            Assert.True(RackDesignValidation.IsUsableFlowBed(new FlowBedConfiguration { LaneDepth = 120.0, PalletDepth = 0.0 }));
            Assert.False(RackDesignValidation.IsUsableFlowBed(new FlowBedConfiguration { LaneDepth = 0.0, PalletDepth = 48.0 }));
            Assert.False(RackDesignValidation.IsUsableFlowBed(null));
        }

        [Fact]
        public void IsUsableLarguero_OnlyProfileRequired_ZeroLengthAndPeralteStayValid()
        {
            // A real larguero may be mid-edit with a 0 length/peralte; only the profile id distinguishes it from "{}".
            Assert.True(RackDesignValidation.IsUsableLarguero(new LargueroDesign { BeamProfileId = "BEAM_X", Length = 0.0, Peralte = 0.0 }));
            Assert.False(RackDesignValidation.IsUsableLarguero(new LargueroDesign { BeamProfileId = "   " }));
            Assert.False(RackDesignValidation.IsUsableLarguero(new LargueroDesign { BeamProfileId = null, Length = 96.0, Peralte = 4.0 }));
            Assert.False(RackDesignValidation.IsUsableLarguero(null));
        }

        [Fact]
        public void IsUsableDynamic_RequiresModules_DepthAndAtLeastTwoDeep()
        {
            var system = DynamicSystem();
            Assert.True(RackDesignValidation.IsUsableDynamic(system));
            Assert.False(RackDesignValidation.IsUsableDynamic((DynamicRackSystem)null));

            var pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            Assert.True(RackDesignValidation.IsUsableDynamic(
                new DynamicRackDesign { Pallet = pallet, PalletsDeep = 4 }, system));

            // PalletsDeep < 2 is not a usable dynamic rack.
            Assert.False(RackDesignValidation.IsUsableDynamic(
                new DynamicRackDesign { Pallet = pallet, PalletsDeep = 1 }, system));
            // Pallet depth 0 is not usable.
            Assert.False(RackDesignValidation.IsUsableDynamic(
                new DynamicRackDesign { Pallet = new PalletSpecification(42.0, 0.0, 60.0, 1000.0, "kg"), PalletsDeep = 4 }, system));
            // Missing pallet is not usable.
            Assert.False(RackDesignValidation.IsUsableDynamic(
                new DynamicRackDesign { Pallet = null, PalletsDeep = 4 }, system));
        }

        // --- F3: descriptor operations + proof the store dispatches via the registry (no hard-coded Kind switch) ---

        [Theory]
        [InlineData(RackSystemKind.Selective, "la cabecera")]
        [InlineData(RackSystemKind.PalletFlow, "el sistema dinámico")]
        [InlineData(RackSystemKind.SelectiveRack, "el rack selectivo")]
        [InlineData(RackSystemKind.Cama, "la cama")]
        [InlineData(RackSystemKind.Larguero, "el larguero")]
        public void DefaultDescriptor_SupportsPersistence_WithExactValidationNoun(RackSystemKind kind, string expectedNoun)
        {
            var descriptor = SystemRegistry.Default.Get(kind);
            Assert.True(descriptor.SupportsPersistence);
            Assert.Equal(expectedNoun, descriptor.ValidationNoun); // distinct from LibraryLabel
        }

        // Each payload-carrying default descriptor can write its payload, build the project back, and validate it.
        [Theory]
        [InlineData(RackSystemKind.PalletFlow)]
        [InlineData(RackSystemKind.SelectiveRack)]
        [InlineData(RackSystemKind.Cama)]
        [InlineData(RackSystemKind.Larguero)]
        public void DefaultDescriptor_PayloadKinds_WriteBuildValidate(RackSystemKind kind)
        {
            var descriptor = SystemRegistry.Default.Get(kind);
            var project = ProjectFor(kind);

            var document = new RackProjectDocument { Kind = kind };
            Assert.True(descriptor.TryWritePayload(project, document)); // writes its own payload slot

            var rebuilt = descriptor.Build(document, new BracingPanelMemberBuilder());
            Assert.Equal(kind, rebuilt.Kind);
            Assert.True(descriptor.IsUsable(rebuilt));
        }

        // The Selective (cabecera) descriptor defers its write to the store's shared header fallback, but still builds and
        // validates the header (rebuilding derived members).
        [Fact]
        public void DefaultDescriptor_Selective_DefersWrite_ButBuildsAndValidatesTheHeader()
        {
            var descriptor = SystemRegistry.Default.Get(RackSystemKind.Selective);
            var project = ProjectFor(RackSystemKind.Selective);

            Assert.False(descriptor.TryWritePayload(project, new RackProjectDocument { Kind = RackSystemKind.Selective }));

            var document = new RackProjectDocument
            {
                Kind = RackSystemKind.Selective,
                Header = RackFrameProjectDocument.FromConfiguration(project.Header),
            };
            var rebuilt = descriptor.Build(document, new BracingPanelMemberBuilder());
            Assert.Equal(RackSystemKind.Selective, rebuilt.Kind);
            Assert.NotEmpty(rebuilt.Header.Members); // members rebuilt during Build
            Assert.True(descriptor.IsUsable(rebuilt));
        }

        // No multi-case RackSystemKind dispatch remains in RackProjectStore: removing a kind from the registry changes the
        // outcome, which a hard-coded switch could not do.
        [Fact]
        public void RackProjectStore_DispatchesViaRegistry_NotAHardCodedKindSwitch()
        {
            var larguero = new LargueroDesign { BeamProfileId = "BEAM_X", Name = "L" };

            // Default registry: the larguero payload is written and the discriminator is "Larguero".
            using (var defaultDoc = JsonDocument.Parse(new RackProjectStore().Serialize(RackProject.ForLarguero(larguero))))
            {
                Assert.Equal("Larguero", defaultDoc.RootElement.GetProperty("Kind").GetString());
            }

            // Same store code, a registry WITHOUT Larguero: it falls back to the header/Selective path instead of writing
            // a larguero payload — proving the dispatch is registry-driven.
            var withoutLarguero = new SystemRegistry(
                SystemRegistry.Default.Descriptors.Where(d => d.Kind != RackSystemKind.Larguero));
            using (var fallbackDoc = JsonDocument.Parse(new RackProjectStore(withoutLarguero).Serialize(RackProject.ForLarguero(larguero))))
            {
                Assert.Equal("Selective", fallbackDoc.RootElement.GetProperty("Kind").GetString());
                Assert.Equal(JsonValueKind.Null, fallbackDoc.RootElement.GetProperty("Larguero").ValueKind);
            }

            // Build side: a Larguero wrapper on that registry builds via the header fallback and fails with the degenerate
            // "cabecera" message, not the "larguero" payload message.
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new RackProjectStore(withoutLarguero).Deserialize("{\"schemaVersion\":\"2.0\",\"kind\":\"Larguero\"}"));
            Assert.Contains("cabecera", ex.Message);
        }
    }
}
