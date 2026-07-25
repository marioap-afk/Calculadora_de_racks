using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-VAL-06 through the REAL persistence stores. An OLDER Push Back project — serialized with a PARRILLA safety
    /// selection, unknown I-11 metadata and a schema version, wrapped in an envelope with a GUID — is read back through
    /// the current stores WITHOUT error; loading + resolving strips the PARRILLA so it reaches no system, view or BOM; and
    /// the canonical re-serialization (the snapshot of the resolved system) neither re-emits the PARRILLA nor loses the
    /// GUID, the unknown metadata or the (readable) schema version. No destructive migration — exactly the GUIA treatment.
    /// </summary>
    public class PushBackNoParrillaPersistenceTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign DesignWithParrilla()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            // A walk grid a NEWER build never seeds, but an OLDER document could carry (the dialog used to not hide it).
            design.Structure.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "PARRILLA_GENERICA", Quantity = 1, Side = SafetySide.Both,
                ParrillaFrontal = true, ParrillaLateral = true
            });
            return design;
        }

        // Serialize the inner Push Back project, then inject a schema version + an unknown field (as an older/newer build
        // would have written it) and read it back as the RackProject the reopen deserializes (version + unknown captured).
        private static RackProject OldInnerProject(string schemaVersion, string unknownKey, JsonNode unknownValue)
        {
            var baseJson = new RackProjectStore().Serialize(RackProject.ForPushBack(DesignWithParrilla()));
            var node = JsonNode.Parse(baseJson).AsObject();
            node["SchemaVersion"] = schemaVersion;
            node[unknownKey] = unknownValue;
            return new RackProjectStore().Deserialize(node.ToJsonString());
        }

        [Fact]
        public void OldProjectWithParrilla_ReadsResolvesAndRewrites_WithoutParrilla_PreservingIdentityAndMetadata_PbVal06()
        {
            var catalog = Catalog;
            var store = new RackProjectStore();
            var embedStore = new RackEmbedStore();
            const string guid = "11111111-1111-1111-1111-111111111111";

            // ---- an OLD payload: envelope(GUID) wrapping an inner project with PARRILLA + unknown metadata + version ----
            var oldInner = OldInnerProject("2.5", "futureInner", JsonValue.Create("keep"));
            var oldInnerJson = store.Serialize(RackProject.ForPushBack(oldInner.PushBackDesign).WithSourceMetadataFrom(oldInner));
            var oldPayload = embedStore.Serialize(
                RackEmbedComposer.Compose(null, RackEmbedDocument.KindPushBack, guid, "PB-OLD", RackEmbedDocument.ViewLateral, 0, oldInnerJson));

            // ---- read back through the CURRENT stores (no error) ----
            var embed = embedStore.Deserialize(oldPayload);
            Assert.Equal(RackEmbedDocument.KindPushBack, embed.Kind);
            Assert.Equal(guid, embed.Id);
            var project = store.Deserialize(embed.Design);
            Assert.NotNull(project?.PushBackDesign);
            // The design as LOADED still carries the legacy parrilla — the read is non-destructive.
            Assert.Contains(project.PushBackDesign.Structure.SafetySelections, s => (s.ElementId ?? string.Empty).Contains("PARRILLA"));

            // ---- load + resolve: the parrilla reaches NO system, view or BOM ----
            var resolver = new PushBackResolver(catalog);
            var system = resolver.Resolve(project.PushBackDesign);
            Assert.DoesNotContain(system.SafetySelections, s => (s.ElementId ?? string.Empty).Contains("PARRILLA"));

            var frontal = new PushBackSystemFrontalBuilder();
            bool NoParrilla(IEnumerable<HeaderBlockInstance> ins) => !ins.Any(i => (i.PieceId ?? string.Empty).Contains("PARRILLA"));
            Assert.True(NoParrilla(new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances));
            Assert.True(NoParrilla(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances));
            Assert.True(NoParrilla(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances));
            Assert.True(NoParrilla(new PushBackSystemPlantaBuilder().Build(system, catalog)));
            Assert.DoesNotContain(PushBackBomBuilder.Build(system, catalog).Lines, l => (l.ProfileId ?? string.Empty).Contains("PARRILLA"));

            // ---- canonical re-serialization: the snapshot of the resolved (parrilla-free) system, carrying the metadata ----
            var snapshot = resolver.Snapshot(system);
            var canonicalInnerJson = store.Serialize(RackProject.ForPushBack(snapshot).WithSourceMetadataFrom(project));
            var canonicalPayload = embedStore.Serialize(
                RackEmbedComposer.Compose(embed, RackEmbedDocument.KindPushBack, embed.Id, embed.Name, RackEmbedDocument.ViewLateral, 0, canonicalInnerJson));

            // PARRILLA does NOT reappear on the explicit write (case-insensitive: no id, no Parrilla* property emitted).
            Assert.False(canonicalInnerJson.Contains("parrilla", StringComparison.OrdinalIgnoreCase), "PARRILLA reappeared in the canonical write");

            // GUID, unknown metadata and the readable schema version ARE preserved.
            var rewritten = embedStore.Deserialize(canonicalPayload);
            Assert.Equal(guid, rewritten.Id);
            using var innerDoc = JsonDocument.Parse(rewritten.Design);
            Assert.Equal("2.5", innerDoc.RootElement.GetProperty("SchemaVersion").GetString());
            Assert.Equal("keep", innerDoc.RootElement.GetProperty("futureInner").GetString());
        }
    }
}
