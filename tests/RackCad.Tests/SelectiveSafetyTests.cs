using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
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
            Assert.NotEmpty(elements);
            Assert.Contains(elements, e => string.Equals(e.Type, "BOTA", System.StringComparison.OrdinalIgnoreCase));
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
        public void Frontal_DrawsBotaAtEveryPost_CoincidentWithBasePlate()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 99)), Catalog);
            var instances = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog).ToList();

            var botas = instances.Where(i => i.Role == HeaderBlockRole.Safety).ToList();
            var posts = instances.Count(i => i.Role == HeaderBlockRole.Post);
            Assert.Equal(posts, botas.Count); // one bota per post
            Assert.All(botas, b => Assert.Equal("PROTECTOR_BOTA_H_3_16_18_FRONTAL", b.BlockName));

            // Its insertion coincides with a base plate's (same point as the plate at that post).
            var plates = instances.Where(i => i.Role == HeaderBlockRole.BasePlate).ToList();
            Assert.All(botas, b => Assert.Contains(plates, p => Math.Abs(p.Insertion.X - b.Insertion.X) < 1e-6 && Math.Abs(p.Insertion.Y - b.Insertion.Y) < 1e-6));
        }

        [Fact]
        public void Bom_DrawableBota_CountsFromDrawing_NotManualQuantity()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 99)), Catalog);
            var posts = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog).Count(i => i.Role == HeaderBlockRole.Post);

            var line = SelectiveBomBuilder.Build(system, Catalog).Lines.Single(l => l.ProfileId == "PROTECTOR_BOTA_H_3_16_18");

            Assert.Equal(posts, line.Quantity); // the drawn count, NOT the manual 99
            Assert.NotEqual(99, line.Quantity);
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
