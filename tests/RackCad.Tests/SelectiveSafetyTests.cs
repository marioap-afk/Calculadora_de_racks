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

        private static SelectivePalletDesign Design(params (string Id, int Qty, SafetySide Side)[] safety)
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0 };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            design.Bays.Add(bay);
            foreach (var (id, qty, side) in safety) design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = id, Quantity = qty, Side = side });
            return design;
        }

        /// <summary>A design of N frentes (N+1 posts) with one bota selection carrying per-post side overrides.</summary>
        private static SelectivePalletDesign DesignWithPostSides(int frentes, SafetySide defaultSide, params (int Post, SafetySide Side)[] overrides)
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0 };
            for (var i = 0; i < frentes; i++)
            {
                var bay = new SelectiveBayDesign();
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                design.Bays.Add(bay);
            }

            var selection = new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Quantity = 1, Side = defaultSide };
            foreach (var (post, side) in overrides) selection.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side });
            design.SafetySelections.Add(selection);
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
            var document = SelectivePalletDesignDocument.From(Design(("PROTECTOR_BOTA_H_3_16_18", 1, SafetySide.Right)), "id-1", "Rack A");

            var restored = store.Deserialize(store.Serialize(document)).ToDomain();

            var selection = Assert.Single(restored.SafetySelections);
            Assert.Equal("PROTECTOR_BOTA_H_3_16_18", selection.ElementId);
            Assert.Equal(SafetySide.Right, selection.Side); // the chosen side survives the round-trip
        }

        [Fact]
        public void Bom_NonDrawableElement_UsesManualQuantity()
        {
            // An id the catalog doesn't map to a drawable rule falls back to its manual quantity.
            var system = new SelectiveGeometryResolver().Resolve(Design(("PARRILLA", 3, SafetySide.None)), Catalog);
            var bom = SelectiveBomBuilder.Build(system, Catalog);

            Assert.Contains(bom.Lines, l => l.ProfileId == "PARRILLA" && l.Quantity == 3);
        }

        [Fact]
        public void Frontal_Left_DrawsOneBotaPerPost_CoincidentWithBasePlate()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 1, SafetySide.Left)), Catalog);
            var instances = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog).ToList();

            var botas = instances.Where(i => i.Role == HeaderBlockRole.Safety).ToList();
            var posts = instances.Count(i => i.Role == HeaderBlockRole.Post);
            Assert.Equal(posts, botas.Count);                       // one bota per post (single side)
            Assert.All(botas, b => Assert.False(b.MirroredX));      // Left = not mirrored
            Assert.All(botas, b => Assert.Equal("PROTECTOR_BOTA_H_3_16_18_FRONTAL", b.BlockName));

            var plates = instances.Where(i => i.Role == HeaderBlockRole.BasePlate).ToList();
            Assert.All(botas, b => Assert.Contains(plates, p => Math.Abs(p.Insertion.X - b.Insertion.X) < 1e-6 && Math.Abs(p.Insertion.Y - b.Insertion.Y) < 1e-6));
        }

        [Fact]
        public void Frontal_Both_DrawsTwoBotasPerPost_OneMirrored()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 1, SafetySide.Both)), Catalog);
            var instances = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog).ToList();

            var botas = instances.Where(i => i.Role == HeaderBlockRole.Safety).ToList();
            var posts = instances.Count(i => i.Role == HeaderBlockRole.Post);
            Assert.Equal(posts * 2, botas.Count);                   // both sides
            Assert.Equal(posts, botas.Count(b => b.MirroredX));     // half mirrored (the right side)
        }

        [Fact]
        public void Bom_DrawableBota_CountsFromDrawing_NotManualQuantity()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 99, SafetySide.Left)), Catalog);
            var posts = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog).Count(i => i.Role == HeaderBlockRole.Post);

            var line = SelectiveBomBuilder.Build(system, Catalog).Lines.Single(l => l.ProfileId == "PROTECTOR_BOTA_H_3_16_18");

            Assert.Equal(posts, line.Quantity); // the drawn count (one side), NOT the manual 99
            Assert.NotEqual(99, line.Quantity);
        }

        [Fact]
        public void Resolver_SkipsSafetyWithNoSideAndNoQuantity()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 0, SafetySide.None)), Catalog);
            Assert.Empty(system.SafetySelections);
        }

        [Fact]
        public void Bom_NoSafetySelections_HasNoSafetyComponent()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(), Catalog);
            Assert.DoesNotContain(SelectiveBomBuilder.Build(system, Catalog).Components, c => c.Category == SelectiveBomBuilder.Safety);
        }

        [Fact]
        public void Frontal_PerPostSide_OverridesOnlyTheNamedPosts()
        {
            // 3 frentes = 4 posts. Default Left (1 bota/post, unmirrored); post 1 → Right (mirrored), post 2 → None (off).
            var system = new SelectiveGeometryResolver().Resolve(
                DesignWithPostSides(3, SafetySide.Left, (1, SafetySide.Right), (2, SafetySide.None)), Catalog);
            var instances = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog).ToList();

            var botas = instances.Where(i => i.Role == HeaderBlockRole.Safety).ToList();
            Assert.Equal(3, botas.Count);                          // posts 0 & 3 (Left) + post 1 (Right); post 2 off
            Assert.Equal(1, botas.Count(b => b.MirroredX));        // only post 1 (Right) is mirrored
            Assert.Equal(2, botas.Count(b => !b.MirroredX));       // posts 0 & 3 (Left)
        }

        [Fact]
        public void PostSides_RoundTripThroughDocument()
        {
            var store = new SelectivePalletDesignStore();
            var design = DesignWithPostSides(2, SafetySide.Left, (0, SafetySide.None), (1, SafetySide.Right));
            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id-1", "Rack A"))).ToDomain();

            var selection = Assert.Single(restored.SafetySelections);
            Assert.Equal(SafetySide.Left, selection.Side);
            Assert.Equal(SafetySide.None, selection.SideForPost(0));   // override survives
            Assert.Equal(SafetySide.Right, selection.SideForPost(1));  // override survives
            Assert.Equal(SafetySide.Left, selection.SideForPost(5));   // unlisted post → default
        }

        [Fact]
        public void Lateral_DrawsBotaAtCorteBase_WithLateralBlock()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 1, SafetySide.Both)), Catalog);
            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog);

            Assert.NotEmpty(cortes);
            var botas = cortes[0].Largueros.Where(i => i.Role == HeaderBlockRole.Safety).ToList();
            Assert.Equal(2, botas.Count); // Both = one per side at the corte's front post
            Assert.All(botas, b => Assert.Equal("PROTECTOR_BOTA_H_3_16_18_LATERAL", b.BlockName));
        }

        [Fact]
        public void Planta_DrawsOneBotaPerFrame_WithPlantaBlock()
        {
            // 2 frentes = 3 posts, single fondo, default Left → one bota per frame post.
            var system = new SelectiveGeometryResolver().Resolve(DesignWithPostSides(2, SafetySide.Left), Catalog);
            var instances = new SelectivePlantaBuilder().Build(system, Catalog).ToList();

            var botas = instances.Where(i => i.Role == HeaderBlockRole.Safety).ToList();
            Assert.Equal(3, botas.Count);
            Assert.All(botas, b => Assert.False(b.MirroredX)); // Left
            Assert.All(botas, b => Assert.Equal("PROTECTOR_BOTA_H_3_16_18_PLANTA", b.BlockName));
        }

        [Fact]
        public void Planta_PerPostNone_SkipsThatFramesBota()
        {
            // Default Left, but post 0 turned off → only posts 1 & 2 carry a bota.
            var system = new SelectiveGeometryResolver().Resolve(
                DesignWithPostSides(2, SafetySide.Left, (0, SafetySide.None)), Catalog);
            var botas = new SelectivePlantaBuilder().Build(system, Catalog).Count(i => i.Role == HeaderBlockRole.Safety);
            Assert.Equal(2, botas);
        }
    }
}
