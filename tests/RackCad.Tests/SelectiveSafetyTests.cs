using System;
using System.Collections.Generic;
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

        private static SelectiveBayDesign Bay()
        {
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            return bay;
        }

        private const string LateralId = "PROTECTOR_LATERAL_BOTA_H_3_16_18";

        /// <summary>A design of N frentes with a bota (general side, all frentes) + a protector lateral on chosen posts.</summary>
        private static SelectivePalletDesign DesignWithLateral(int frentes, SafetySide botaSide, params (int Post, SafetySide Side)[] lateralPosts)
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0 };
            for (var i = 0; i < frentes; i++) design.Bays.Add(Bay());
            if (botaSide != SafetySide.None)
            {
                design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Side = botaSide });
            }

            var lateral = new SelectiveSafetySelection { ElementId = LateralId, Side = SafetySide.None };
            foreach (var (post, side) in lateralPosts) lateral.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side });
            design.SafetySelections.Add(lateral);
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
        public void Frontal_Lateral_IsOneBlock_ReplacesBotasAtItsFrente()
        {
            // 2 frentes = 3 posts. Bota Both on all; a protector lateral (Left = ONE block, the block already spans the
            // fondo) on frente 0 → frente 0 has 1 lateral, NO botas.
            var system = new SelectiveGeometryResolver().Resolve(
                DesignWithLateral(2, SafetySide.Both, (0, SafetySide.Left)), Catalog);
            var instances = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog).ToList();
            var safety = instances.Where(i => i.Role == HeaderBlockRole.Safety).ToList();

            Assert.Equal(1, safety.Count(s => s.BlockName == "PROTECTOR_LATERAL_BOTA_H_3_16_18_FRONTAL")); // ONE block, not two
            Assert.Equal(4, safety.Count(s => s.BlockName == "PROTECTOR_BOTA_H_3_16_18_FRONTAL"));         // frentes 1 & 2 only
        }

        [Fact]
        public void Planta_Lateral_SpansTheFondoDepth_ViaLongitudParam()
        {
            var system = new SelectiveGeometryResolver().Resolve(
                DesignWithLateral(2, SafetySide.None, (0, SafetySide.Left)), Catalog);
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0);
            var laterales = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Safety).ToList();

            var lateral = Assert.Single(laterales); // ONE block spanning the fondo
            Assert.Equal(depth, lateral.DynamicParameters[SelectiveRackDefaults.LengthParam], 3);
        }

        [Fact]
        public void Planta_Lateral_Orillas_OneBlockPerEnd_GuideFlippedByY()
        {
            // Default orillas: first frente Left (as-is), last frente Right. ONE block each; in planta the guide flips
            // via a Y-flip IN PLACE (the block already spans the fondo — no X-reflection to the back post).
            var system = new SelectiveGeometryResolver().Resolve(
                DesignWithLateral(3, SafetySide.None, (0, SafetySide.Left), (3, SafetySide.Right)), Catalog);
            var laterales = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Safety).ToList();

            Assert.Equal(2, laterales.Count);                    // one per orilla, not doubled
            Assert.All(laterales, l => Assert.False(l.MirroredX)); // planta flips the guide in Y, never X
            Assert.Single(laterales, l => !l.MirroredY);         // first orilla (Left)
            Assert.Single(laterales, l => l.MirroredY);          // last orilla (Right, guide flipped)
            // Both sit at the front post (same depth X) — the Right one is NOT reflected to the back.
            Assert.Single(laterales.Select(l => Math.Round(l.Insertion.X, 3)).Distinct());
        }

        [Fact]
        public void Bom_Lateral_ReportsLongitudPlusAllowance()
        {
            var system = new SelectiveGeometryResolver().Resolve(
                DesignWithLateral(2, SafetySide.None, (0, SafetySide.Left)), Catalog);
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0);
            var bom = SelectiveBomBuilder.Build(system, Catalog);

            var lateral = bom.Components.Single(c => c.ProfileId == LateralId);
            Assert.Equal(depth + 4.0, lateral.Length, 3);       // the given LONGITUD (= fondo) + 4"
            Assert.Contains(bom.Lines, l => l.ProfileId == LateralId && Math.Abs(l.Length - (depth + 4.0)) < 1e-3);
        }

        [Fact]
        public void Bom_Lateral_IsOwnComponent_AndSuppressesBotasThere()
        {
            // Bota Both on all 3 frentes (posts 0..2) + a lateral on frente 0 (Left). Botas count = frentes 1 & 2 only.
            var system = new SelectiveGeometryResolver().Resolve(
                DesignWithLateral(2, SafetySide.Both, (0, SafetySide.Left)), Catalog);
            var bom = SelectiveBomBuilder.Build(system, Catalog);

            var lateral = bom.Components.Single(c => c.ProfileId == LateralId);
            Assert.Equal(SelectiveBomBuilder.Safety, lateral.Category);
            Assert.Equal(1, lateral.Quantity); // frente 0, one block

            var bota = bom.Components.Single(c => c.ProfileId == "PROTECTOR_BOTA_H_3_16_18");
            Assert.Equal(4, bota.Quantity);    // frentes 1 & 2 (Both) — NOT 6; frente 0's botas are suppressed
        }

        [Fact]
        public void Bom_Bota_FullySuppressedByLaterales_NotListed()
        {
            // 1 bay = 2 posts. Bota Both + laterales on BOTH posts → every frente has a lateral → NO botas drawn.
            // The bota must NOT appear in the BOM (no phantom from the manual quantity).
            var system = new SelectiveGeometryResolver().Resolve(
                DesignWithLateral(1, SafetySide.Both, (0, SafetySide.Left), (1, SafetySide.Right)), Catalog);
            var bom = SelectiveBomBuilder.Build(system, Catalog);

            Assert.DoesNotContain(bom.Components, c => c.ProfileId == "PROTECTOR_BOTA_H_3_16_18"); // no phantom bota
            Assert.Contains(bom.Components, c => c.ProfileId == LateralId);                         // laterales are listed
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
        public void Lateral_Both_MirrorsAboutFondoDepthCenter()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 1, SafetySide.Both)), Catalog);
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0);
            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog);

            Assert.NotEmpty(cortes);
            var botas = cortes[0].Largueros.Where(i => i.Role == HeaderBlockRole.Safety).ToList();
            Assert.Equal(2, botas.Count); // Both = front upright + its mirror at the back
            Assert.All(botas, b => Assert.Equal("PROTECTOR_BOTA_H_3_16_18_LATERAL", b.BlockName));
            Assert.Single(botas, b => b.MirroredX);
            Assert.Single(botas, b => !b.MirroredX);
            // Reflection about the depth center (front at X=0): the two X's sum to the total fondo depth.
            Assert.Equal(depth, botas[0].Insertion.X + botas[1].Insertion.X, 3);
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

        [Fact]
        public void Planta_CornerLayout_MirrorsAboutReachingFondosOnly()
        {
            // Corner: fondo 0 = 3 frentes, fondo 1 = 1 frente. At the far frente only fondo 0 reaches, so the mirror
            // must center over fondo 0 ALONE — not over the deeper global span (which would float the bota behind
            // a post that doesn't exist there, disagreeing with the lateral corte).
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.Bays.Add(Bay());
            design.Bays.Add(Bay());
            design.Bays.Add(Bay());                                             // fondo 0: 3 frentes (posts 0..3)
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay() });  // fondo 1: 1 frente (posts 0..1)
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Side = SafetySide.Both });

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var depth0 = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0);
            var offsets = SelectiveDepthLayout.Offsets(system);
            var globalBackmost = offsets[1] + SelectiveDepthLayout.CabeceraDepthOfFondo(system, 1);

            var botas = new SelectivePlantaBuilder().Build(system, Catalog).Where(i => i.Role == HeaderBlockRole.Safety).ToList();

            // Farthest frente (largest Y): only fondo 0 reaches → Both = a front + its mirror, reflected about depth0/2.
            var farFrente = botas.GroupBy(b => Math.Round(b.Insertion.Y, 3)).OrderBy(g => g.Key).Last().ToList();
            Assert.Equal(2, farFrente.Count);
            Assert.Equal(depth0, farFrente[0].Insertion.X + farFrente[1].Insertion.X, 3);   // reflected about fondo 0 only
            Assert.All(farFrente, b => Assert.True(b.Insertion.X < globalBackmost - 1.0));   // never floats to the global backmost
        }

        [Fact]
        public void Planta_MultiFondo_TwoBotasPerFrente_NotPerFondo()
        {
            // 2 fondos reaching every frente, Both: the bota is a SYSTEM element → exactly 2 per frente (system front +
            // back), NOT one per fondo (which drew 4). This is the user's "en 4 fondos deberían ser 2" bug.
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.Bays.Add(Bay());
            design.Bays.Add(Bay());                                                    // fondo 0: 2 frentes → 3 posts
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(), Bay() });   // fondo 1: 2 frentes
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Side = SafetySide.Both });

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var offsets = SelectiveDepthLayout.Offsets(system);
            var systemBack = offsets[1] + SelectiveDepthLayout.CabeceraDepthOfFondo(system, 1);
            var botas = new SelectivePlantaBuilder().Build(system, Catalog).Where(i => i.Role == HeaderBlockRole.Safety).ToList();

            Assert.Equal(6, botas.Count); // 3 frentes × 2 (system front + back), NOT × 2 fondos
            // Every bota sits at the system front (X≈0) or system back (X≈systemBack), never on an interior fondo post.
            Assert.All(botas, b => Assert.True(b.Insertion.X < 1.0 || b.Insertion.X > systemBack - 1.0));
        }

        [Fact]
        public void Bom_BotaIsItsOwnComponent_NamedAfterThePiece()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 1, SafetySide.Both)), Catalog);
            var bom = SelectiveBomBuilder.Build(system, Catalog);

            var safety = bom.Components.Single(c => c.Category == SelectiveBomBuilder.Safety);
            Assert.Equal("PROTECTOR_BOTA_H_3_16_18", safety.ProfileId);           // the component IS the bota
            Assert.NotEqual("Elementos de seguridad", safety.Description);         // not the old generic wrapper
            Assert.True(safety.Quantity > 0);                                     // counted from the drawing
        }

        [Fact]
        public void Planta_Both_MirrorsAboutFondoDepthCenter()
        {
            // Single fondo, Both: each frame gets a front bota + its mirror; the pair reflects about the depth center.
            var system = new SelectiveGeometryResolver().Resolve(Design(("PROTECTOR_BOTA_H_3_16_18", 1, SafetySide.Both)), Catalog);
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0);
            var botas = new SelectivePlantaBuilder().Build(system, Catalog).Where(i => i.Role == HeaderBlockRole.Safety).ToList();

            Assert.NotEmpty(botas);
            Assert.Equal(0, botas.Count % 2); // a front + a mirrored per frame
            foreach (var frame in botas.GroupBy(b => Math.Round(b.Insertion.Y, 3)))
            {
                var pair = frame.ToList();
                Assert.Equal(2, pair.Count);
                Assert.Single(pair, b => b.MirroredX);
                Assert.Single(pair, b => !b.MirroredX);
                Assert.Equal(depth, pair[0].Insertion.X + pair[1].Insertion.X, 3); // reflected about depth/2
            }
        }

        // ---- Parrillas (decks): grid per (frente, level), drawn frontal/lateral, counted in the BOM ----

        private const string ParrillaId = "PARRILLA_GENERICA";

        private static SelectivePalletDesign ParrillaDesign(int frentes, int levelsPerBay, bool frontal, bool lateral, params (int f, int l)[] offCells)
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0 };
            for (var i = 0; i < frentes; i++)
            {
                var bay = new SelectiveBayDesign { FloorBeam = true }; // FloorBeam so cells == resolved levels
                for (var l = 0; l < levelsPerBay; l++)
                {
                    bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 45 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                }

                design.Bays.Add(bay);
            }

            var sel = new SelectiveSafetySelection { ElementId = ParrillaId, Side = SafetySide.Both, Quantity = 1, ParrillaFrontal = frontal, ParrillaLateral = lateral };
            foreach (var (f, l) in offCells) sel.ParrillaOffCells.Add(new SelectiveGridCell { Frente = f, Level = l });
            design.SafetySelections.Add(sel);
            return design;
        }

        private static List<HeaderBlockInstance> FrontalParrillas(SelectivePalletDesign design)
        {
            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            return new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId).ToList();
        }

        [Fact]
        public void Parrilla_Frontal_OnePerTarima_AtTarimaFrente()
        {
            var design = ParrillaDesign(2, 2, frontal: true, lateral: true);
            var parrillas = FrontalParrillas(design);

            // One deck PER TARIMA: 2 frentes × 2 levels × PalletCount(2) = 8, each at the tarima's own frente (40),
            // NOT one wide deck at the larguero length.
            Assert.Equal(8, parrillas.Count);
            Assert.All(parrillas, p => Assert.True(p.DynamicParameters.ContainsKey("FRENTE")));
            Assert.All(parrillas, p => Assert.Equal(40.0, p.DynamicParameters["FRENTE"], 4));
        }

        [Fact]
        public void Parrilla_Frontal_SkipsOffGridCells()
        {
            // 3 ON cells × PalletCount(2) = 6 (the (0,0) cell drops its 2 decks).
            Assert.Equal(6, FrontalParrillas(ParrillaDesign(2, 2, true, true, (0, 0))).Count);
        }

        [Fact]
        public void Parrilla_Frontal_ManualFrente_OverridesCountPerBay()
        {
            // 3 tarimas per bay, but a manual 60" deck fits only floor(136/60)=2 per bay → "2 parrillas, 3 tarimas".
            var design = ParrillaDesign(2, 1, frontal: true, lateral: true);
            foreach (var bay in design.Bays)
                foreach (var level in bay.Levels) { level.PalletCount = 3; }
            var sel = design.SafetySelections.Single(s => s.ElementId == ParrillaId);
            sel.ParrillaFrente = 60.0;

            var parrillas = FrontalParrillas(design);
            Assert.Equal(4, parrillas.Count); // 2 frentes × 1 level × 2 decks-that-fit
            Assert.All(parrillas, p => Assert.Equal(60.0, p.DynamicParameters["FRENTE"], 4));
        }

        [Fact]
        public void Parrilla_FrontalToggleOff_DrawsNoneInFrontal()
        {
            Assert.Empty(FrontalParrillas(ParrillaDesign(2, 2, frontal: false, lateral: true)));
        }

        [Fact]
        public void Parrilla_Lateral_OnePerFondoLevel_WithFondoSpan()
        {
            var system = new SelectiveGeometryResolver().Resolve(ParrillaDesign(2, 2, true, true), Catalog);
            var parrillas = new SelectiveLateralBuilder().Cortes(system, Catalog)
                .SelectMany(c => c.Largueros).Where(i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId).ToList();

            Assert.NotEmpty(parrillas);
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0);
            Assert.All(parrillas, p => Assert.Equal(depth, p.DynamicParameters["FONDO"], 4)); // FONDO = the deck span
        }

        /// <summary>A one-frente design with <paramref name="tarimas"/> pallets of 40" (tolerance 4" → claro 136").</summary>
        private static SelectivePalletDesign ParrillaCantidadDesign(int tarimas, double frente, int cantidad)
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0 };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 45 }, PalletCount = tarimas, BeamId = BeamId, BeamPeralte = 4.0 });
            design.Bays.Add(bay);
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = ParrillaId, Side = SafetySide.Both, Quantity = 1,
                ParrillaFrontal = true, ParrillaLateral = true, ParrillaFrente = frente, ParrillaCantidad = cantidad
            });
            return design;
        }

        [Fact]
        public void Parrilla_ManualCantidad_KeepsTheTarimaFrente()
        {
            // "2 parrillas bajo 3 tarimas" said as a COUNT: the decks stay the standard tarima width (40"), just fewer.
            var parrillas = FrontalParrillas(ParrillaCantidadDesign(tarimas: 3, frente: 0.0, cantidad: 2));

            Assert.Equal(2, parrillas.Count);
            Assert.All(parrillas, p => Assert.Equal(40.0, p.DynamicParameters["FRENTE"], 4));
        }

        [Fact]
        public void Parrilla_ManualCantidad_AndFrente_UsesBoth()
        {
            var parrillas = FrontalParrillas(ParrillaCantidadDesign(tarimas: 3, frente: 50.0, cantidad: 2));

            Assert.Equal(2, parrillas.Count); // 2 × 50" = 100" fits the 136" claro
            Assert.All(parrillas, p => Assert.Equal(50.0, p.DynamicParameters["FRENTE"], 4));
        }

        [Fact]
        public void Parrilla_ManualCantidad_ThatDoesNotFit_IsClampedNotOverflowed()
        {
            // The editor refuses this, but the bay can be narrowed AFTER configuring: 3 × 60" = 180" > 136" claro.
            // Drawing decks past the frame (or with a negative gap) is never right — clamp to what fits, and the BOM
            // must report the clamped truth, not the wish.
            var design = ParrillaCantidadDesign(tarimas: 3, frente: 60.0, cantidad: 3);
            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            var drawn = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog)
                .Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId);
            var bom = SelectiveBomBuilder.Build(system, Catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Parrilla).Sum(c => c.Quantity);

            Assert.Equal(2, drawn); // floor(136/60) = 2, not the forced 3
            Assert.Equal(drawn, bom);
        }

        [Fact]
        public void Parrilla_Plan_CountAndMaxMatchTheDraw()
        {
            // The editor's live count and its "no cabe" guard must agree with the builder, cell by cell.
            var design = ParrillaCantidadDesign(tarimas: 3, frente: 0.0, cantidad: 0);
            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var plan = SelectiveParrillaPlan.Cells(system, Catalog);

            var cell = Assert.Single(plan);
            Assert.Equal(3, SelectiveParrillaPlan.CountIn(cell, 0.0, 0));   // una por tarima
            Assert.Equal(2, SelectiveParrillaPlan.CountIn(cell, 60.0, 0));  // 60" → floor(136/60)
            Assert.Equal(2, SelectiveParrillaPlan.CountIn(cell, 0.0, 2));   // cantidad forzada
            Assert.Equal(3, SelectiveParrillaPlan.MaxCountIn(cell, 40.0));  // caben 3 de 40"
            Assert.Equal(2, SelectiveParrillaPlan.MaxCountIn(cell, 60.0));  // solo 2 de 60"
            Assert.Equal(0, SelectiveParrillaPlan.MaxCountIn(cell, 150.0)); // ninguna de 150"

            // And the count the plan reports IS what the frontal draws.
            Assert.Equal(
                SelectiveParrillaPlan.CountIn(cell, 60.0, 0),
                FrontalParrillas(ParrillaCantidadDesign(3, 60.0, 0)).Count);
        }

        [Fact]
        public void Parrilla_Plan_NarrowTramo_DoesNotVetoTheWholeCell()
        {
            // Regression: a medio frente whose 30" tramo holds no 40" tarima (so no deck either) while the remainder
            // holds several. The cell is NOT empty — the count is the SUM over the tramos. MaxCountIn must skip the
            // inherently-empty tramo instead of collapsing to 0, or the editor paints a real count in the cell and
            // simultaneously warns in red that none fits, and a forced cantidad the wide tramo could take gets vetoed.
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0 };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            bay.Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true }); // no 40" tarima fits here
            bay.Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });  // the remainder does
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 45 }, PalletCount = 3, BeamId = BeamId, BeamPeralte = 4.0 });
            design.Bays.Add(bay);
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = ParrillaId, Side = SafetySide.Both, Quantity = 1, ParrillaFrontal = true, ParrillaLateral = true });
            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            var cell = Assert.Single(SelectiveParrillaPlan.Cells(system, Catalog));
            Assert.True(cell.Rows.Count > 1, "expected the medio frente to resolve into several load rows");

            var drawn = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog)
                .Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId);
            var count = SelectiveParrillaPlan.CountIn(cell, 0.0, 0);

            Assert.True(count > 0, "the wide tramo carries decks, so the cell is not empty");
            Assert.Equal(drawn, count);                                     // the number shown IS the number drawn
            Assert.True(SelectiveParrillaPlan.MaxCountIn(cell, 0.0) > 0,    // the 30" tramo must not veto the cell
                "the narrow tramo holds no tarima either — it must not drag the forceable max to 0");
        }

        [Fact]
        public void Parrilla_Lateral_DrawsNothingWhereTheFrontalAndBomHaveNone()
        {
            // Regression: a manual frente WIDER than the claro (92") fits zero decks, so the frontal draws none and the
            // BOM counts none. The lateral collapses the row end-on, but zero must stay zero — it used to gate only on the
            // grid cell and emit a phantom deck per level that appeared in no frontal view and on no BOM line.
            var design = ParrillaDesign(2, 2, frontal: true, lateral: true);
            design.SafetySelections.Single(s => s.ElementId == ParrillaId).ParrillaFrente = 150.0;
            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            var frontal = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog)
                .Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId);
            var lateral = new SelectiveLateralBuilder().Cortes(system, Catalog)
                .SelectMany(c => c.Largueros).Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId);
            var bom = SelectiveBomBuilder.Build(system, Catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Parrilla).Sum(c => c.Quantity);

            Assert.Equal(0, frontal);
            Assert.Equal(0, bom);
            Assert.Equal(0, lateral); // was 6 phantom decks before the fix
        }

        [Fact]
        public void Parrilla_Lateral_NarrowBayThatFitsNoDeck_DrawsNoneInItsOwnCorte()
        {
            // The manual frente is ONE value for the whole run: 60" fits the 92" frente (1 deck) but not the 48" one, so
            // the frontal and the BOM give the narrow frente zero. Its far corte bounds ONLY that frente — it must stay
            // empty rather than invent a deck. This needs no absurd input, just two frentes of different widths.
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0 };
            foreach (var count in new[] { 2, 1 }) // frente 0 -> BeamLength 92", frente 1 -> 48"
            {
                var bay = new SelectiveBayDesign { FloorBeam = true };
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 45 }, PalletCount = count, BeamId = BeamId, BeamPeralte = 4.0 });
                design.Bays.Add(bay);
            }

            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = ParrillaId, Side = SafetySide.Both, Quantity = 1,
                ParrillaFrontal = true, ParrillaLateral = true, ParrillaFrente = 60.0
            });
            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            var frontal = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog)
                .Count(i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId);
            var bom = SelectiveBomBuilder.Build(system, Catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Parrilla).Sum(c => c.Quantity);
            Assert.Equal(1, frontal); // only the 92" frente holds a 60" deck
            Assert.Equal(1, bom);

            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog).ToList();
            var lastCorte = cortes[cortes.Count - 1]; // the far post bounds ONLY the narrow 48" frente
            Assert.DoesNotContain(lastCorte.Largueros, i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId);
        }

        [Fact]
        public void Parrilla_LateralToggleOff_DrawsNoneInLateral()
        {
            var system = new SelectiveGeometryResolver().Resolve(ParrillaDesign(2, 2, frontal: true, lateral: false), Catalog);
            Assert.DoesNotContain(
                new SelectiveLateralBuilder().Cortes(system, Catalog).SelectMany(c => c.Largueros),
                i => i.Role == HeaderBlockRole.Safety && i.PieceId == ParrillaId);
        }

        [Fact]
        public void Parrilla_Bom_CountsOnCells_AndNotUnderSeguridad()
        {
            var system = new SelectiveGeometryResolver().Resolve(ParrillaDesign(2, 2, true, true, (1, 1)), Catalog);
            var bom = SelectiveBomBuilder.Build(system, Catalog);

            var parrilla = bom.Components.Where(c => c.Category == SelectiveBomBuilder.Parrilla).ToList();
            Assert.Equal(6, parrilla.Sum(c => c.Quantity)); // (2×2 − 1 off) ON cells × PalletCount(2), 1 fondo
            // Not double-counted under the generic "Seguridad" (manual-quantity fallback).
            Assert.DoesNotContain(bom.Components, c => c.Category == SelectiveBomBuilder.Safety && c.ProfileId == ParrillaId);
        }

        [Fact]
        public void Parrilla_Config_RoundTrips()
        {
            var design = ParrillaDesign(2, 2, frontal: true, lateral: false, (0, 1));
            var sel0 = design.SafetySelections.Single(s => s.ElementId == ParrillaId);
            sel0.ParrillaFrente = 36.0;
            sel0.ParrillaCantidad = 2;
            var store = new SelectivePalletDesignStore();
            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id", "R"))).ToDomain();

            var sel = restored.SafetySelections.Single(s => s.ElementId == ParrillaId);
            Assert.True(sel.ParrillaFrontal);
            Assert.False(sel.ParrillaLateral);
            Assert.Equal(36.0, sel.ParrillaFrente, 4); // the manual frente override round-trips
            Assert.Equal(2, sel.ParrillaCantidad);      // and so does the manual cantidad
            Assert.False(sel.ParrillaAt(0, 1)); // the off cell survives
            Assert.True(sel.ParrillaAt(0, 0));
        }
    }
}
