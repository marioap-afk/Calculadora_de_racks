using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Safety accessories (Fase 0): the catalog loads them, the selection round-trips, and they enter the BOM.</summary>
    public class SelectiveSafetyTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign Design(params (string Id, int Qty)[] safety)
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0 };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            design.Bays.Add(bay);
            foreach (var (id, qty) in safety) design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = id, Quantity = qty });
            return design;
        }

        [Fact]
        public void Catalog_LoadsSafetyElements_FromCsv()
        {
            var elements = Catalog.SafetyElements;
            Assert.Contains(elements, e => e.Id == "PROTECTOR_BOTA_H");
            Assert.Contains(elements, e => e.Id == "PARRILLA");
        }

        [Fact]
        public void SafetySelections_RoundTripThroughDocument()
        {
            var store = new SelectivePalletDesignStore();
            var document = SelectivePalletDesignDocument.From(Design(("PROTECTOR_BOTA_H", 4), ("PARRILLA", 3)), "id-1", "Rack A");

            var restored = store.Deserialize(store.Serialize(document)).ToDomain();

            Assert.Equal(2, restored.SafetySelections.Count);
            Assert.Contains(restored.SafetySelections, s => s.ElementId == "PROTECTOR_BOTA_H" && s.Quantity == 4);
        }

        [Fact]
        public void Bom_IncludesSafetyComponent_WithSelectedQuantities()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H", 4), ("PARRILLA", 3)), Catalog);
            var bom = SelectiveBomBuilder.Build(system, Catalog);

            var safety = bom.Components.Single(c => c.Category == SelectiveBomBuilder.Safety);
            Assert.Equal(2, safety.Pieces.Count);
            Assert.Contains(safety.Pieces, p => p.ProfileId == "PROTECTOR_BOTA_H" && p.Quantity == 4);
            // The flattened piece total = piece qty × component qty (1).
            Assert.Contains(bom.Lines, l => l.ProfileId == "PARRILLA" && l.Quantity == 3);
        }

        [Fact]
        public void Resolver_SkipsNonPositiveSafetyQuantity()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H", 0)), Catalog);
            Assert.Empty(system.SafetySelections);
        }

        [Fact]
        public void Bom_NoSafetySelections_HasNoSafetyComponent()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(), Catalog);
            Assert.DoesNotContain(SelectiveBomBuilder.Build(system, Catalog).Components, c => c.Category == SelectiveBomBuilder.Safety);
        }
    }
}
