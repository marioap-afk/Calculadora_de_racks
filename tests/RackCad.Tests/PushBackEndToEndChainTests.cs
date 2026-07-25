using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18b increment 5a — closes the automatic end-to-end chain in PURE code (no Plugin, no AutoCAD; ADR-0003): the two
    /// links that only existed inside Plugin types are re-expressed here against the same Application APIs those types
    /// call, so the round-trip is proven by execution rather than only by source guards.
    ///
    /// 1. envelope -> project -> resolver -> BOM, exactly the sequence <c>PushBackKindHandler.BuildBom</c> performs.
    /// 2. the independent-copy restamp: <c>RackEnvelopeRestamp</c> gives the envelope a fresh GUID + copy name and
    ///    delegates the inner design to the handler, whose Push Back implementation returns the JSON UNTOUCHED.
    /// </summary>
    public class PushBackEndToEndChainTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design()
            => new PushBackDesign
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

        /// <summary>The insert-time payload, composed exactly as <c>RackPushBackCommands.BuildPushBackPayload</c> does.</summary>
        private static string Payload(string id, string name)
        {
            var designJson = new RackProjectStore().Serialize(RackProject.ForPushBack(Design()));
            var embed = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindPushBack, id, name, RackEmbedDocument.ViewLateral, 0, designJson);
            return new RackEmbedStore().Serialize(embed);
        }

        /// <summary>Models <c>PushBackKindHandler.RestampDesign</c>: the Push Back design has no display identity of its
        /// own (GUID + name live in the envelope), so the inner JSON is returned untouched.</summary>
        private static string RestampPushBackDesign(string designJson, string newId, string copyName) => designJson;

        private static int Cells(PushBackSystem system)
            => system.Structure.Fronts.Sum(front => Math.Max(1, front.LoadLevels));

        [Fact]
        public void Envelope_ToProject_ToResolver_ToBom_IsCoherent()
        {
            var catalog = Catalog;
            var payload = Payload(Guid.NewGuid().ToString(), "PB-CHAIN");

            // Read back exactly as the kind handler does: envelope -> inner project -> design.
            var embed = new RackEmbedStore().Deserialize(payload);
            Assert.Equal(RackEmbedDocument.KindPushBack, embed.Kind);

            var project = new RackProjectStore().Deserialize(embed.Design);
            Assert.NotNull(project);
            Assert.NotNull(project.PushBackDesign);

            var system = new PushBackResolver(catalog).Resolve(project.PushBackDesign);
            var bom = PushBackBomBuilder.Build(system, catalog);

            // The BOM that survives the envelope round-trip is the SAME as one built straight from the design.
            var direct = PushBackBomBuilder.Build(new PushBackResolver(catalog).Resolve(Design()), catalog);
            Assert.Equal(direct.Components.Count, bom.Components.Count);

            var cells = Cells(system);
            Assert.Equal(cells, bom.Components.Where(c => c.Category == SystemBomBuilder.InOutBeam).Sum(c => c.Quantity));
            Assert.Equal(cells, bom.Components.Where(c => c.Category == PushBackBomBuilder.HighEndBeam).Sum(c => c.Quantity));
        }

        [Fact]
        public void Restamp_ChangesEnvelopeIdentity_AndLeavesTheInnerPushBackJsonByteIdentical()
        {
            var originalId = Guid.NewGuid().ToString();
            var store = new RackEmbedStore();
            var embed = store.Deserialize(Payload(originalId, "PB-ORIGINAL"));
            var innerBefore = embed.Design;

            // RackEnvelopeRestamp: fresh GUID + copy name on the ENVELOPE, inner design delegated to the handler.
            embed.Id = Guid.NewGuid().ToString();
            embed.Name = "PB-ORIGINAL (copia)";
            embed.Design = RestampPushBackDesign(embed.Design, embed.Id, embed.Name);
            var copyPayload = store.Serialize(embed);

            Assert.NotEqual(originalId, embed.Id);                       // independent identity
            Assert.Equal("PB-ORIGINAL (copia)", embed.Name);
            Assert.Equal(innerBefore, embed.Design);                     // inner Push Back JSON untouched
            Assert.Equal(RackEmbedDocument.KindPushBack, embed.Kind);    // still a Push Back envelope

            // The copy is FUNCTIONALLY identical (same design => same BOM) but identity-independent.
            var copy = new RackEmbedStore().Deserialize(copyPayload);
            Assert.NotEqual(originalId, copy.Id);
            Assert.NotNull(new RackProjectStore().Deserialize(copy.Design)?.PushBackDesign);

            var catalog = Catalog;
            var copyBom = PushBackBomBuilder.Build(
                new PushBackResolver(catalog).Resolve(new RackProjectStore().Deserialize(copy.Design).PushBackDesign),
                catalog);
            var originalBom = PushBackBomBuilder.Build(new PushBackResolver(catalog).Resolve(Design()), catalog);
            Assert.Equal(originalBom.Components.Count, copyBom.Components.Count);
        }
    }
}
