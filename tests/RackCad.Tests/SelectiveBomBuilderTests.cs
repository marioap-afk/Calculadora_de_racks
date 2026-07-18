using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Aggregating a resolved selective rack into a COMPONENT bill of materials: cabeceras + largueros.</summary>
    public class SelectiveBomBuilderTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveRackSystem TwoBaySystem()
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0 };
            for (var b = 0; b < 2; b++)
            {
                var bay = new SelectiveBay { BeamLength = 100.0, Height = 240.0 };
                bay.Levels.Add(new SelectiveLevel { Y = 48.0, BeamId = BeamId, BeamPeralte = 4.0 });
                bay.Levels.Add(new SelectiveLevel { Y = 96.0, BeamId = BeamId, BeamPeralte = 4.0 });
                system.Bays.Add(bay);
            }

            return system;
        }

        // ---- Component BOM (Build(system)): cabeceras + largueros ----

        [Fact]
        public void BuildSystem_IsComponentBased_WithCabecerasAndLargueros()
        {
            var bom = SelectiveBomBuilder.Build(TwoBaySystem(), Catalog);

            Assert.True(bom.IsComponentBased);
            var cabeceras = bom.Components.Where(c => c.Category == BomBuilder.Cabecera).ToList();
            var largueros = bom.Components.Where(c => c.Category == SelectiveBomBuilder.Beam).ToList();

            Assert.Single(cabeceras);
            Assert.Equal(3, cabeceras[0].Quantity); // 2 bays -> 3 frame positions, all identical
            Assert.Single(largueros);
            Assert.Equal(8, largueros[0].Quantity); // 4 frontal beams × 2 (front/back)
        }

        [Fact]
        public void BuildSystem_CabeceraComponent_HasBothPostsBothPlatesAndCelosia()
        {
            var bom = SelectiveBomBuilder.Build(TwoBaySystem(), Catalog);

            var cabecera = bom.Components.First(c => c.Category == BomBuilder.Cabecera);
            Assert.Equal(2, cabecera.Pieces.Where(p => p.Category == SelectiveBomBuilder.Post).Sum(p => p.Quantity)); // front + back post
            Assert.Equal(2, cabecera.Pieces.Where(p => p.Category == SelectiveBomBuilder.BasePlate).Sum(p => p.Quantity));
            // The cabecera brings its celosía — the OLD post+plate BOM omitted it.
            Assert.Contains(cabecera.Pieces, p => p.Category == BomBuilder.Diagonal || p.Category == BomBuilder.Horizontal);
        }

        [Fact]
        public void BuildSystem_LargueroComponent_IsOneProfilePlusTwoMensulas()
        {
            var bom = SelectiveBomBuilder.Build(TwoBaySystem(), Catalog);

            var larguero = bom.Components.First(c => c.Category == SelectiveBomBuilder.Beam);
            Assert.Equal(1, larguero.Pieces.Where(p => p.Category == LargueroBomBuilder.Perfil).Sum(p => p.Quantity));
            Assert.Equal(2, larguero.Pieces.Where(p => p.Category == LargueroBomBuilder.Mensula).Sum(p => p.Quantity));
        }

        [Fact]
        public void BuildSystem_DrawPallets_DoesNotChangeTheBom()
        {
            // Tarimas are a VISUAL reference only ("Solo visual (no BOM)"): turning DrawPallets on must not add,
            // remove, or re-quantify a single BOM component. The BOM is component-driven (bays/levels/safety), so
            // the frontal pallet instances never reach it — this test guards that invariant against refactors.
            var withoutPallets = SelectiveBomBuilder.Build(TwoBaySystem(), Catalog);

            var withPallets = TwoBaySystem();
            withPallets.DrawPallets = true;
            foreach (var bay in withPallets.Bays)
            {
                foreach (var level in bay.Levels)
                {
                    level.PalletFrente = 40.0;
                    level.PalletAlto = 48.0;
                    level.PalletCount = 2;
                }
            }

            var bom = SelectiveBomBuilder.Build(withPallets, Catalog);

            Assert.Equal(withoutPallets.Components.Count, bom.Components.Count);
            Assert.Equal(
                withoutPallets.Components.Sum(c => c.Quantity),
                bom.Components.Sum(c => c.Quantity));
            Assert.DoesNotContain(bom.Components,
                c => c.Category == TestCatalogIds.BlockOnlyPieces.Pallet);
        }

        [Fact]
        public void BuildSystem_DecorationsAndFlagsRestored_DoNotChangeTheBom()
        {
            // The BOM strips the decoration flags on its counting views (BuildForCounting) purely for speed —
            // component identity, order and quantities must be identical, AND the caller's system must come back
            // with its flags untouched (the planta path mutates+restores the REAL system).
            var plain = SelectiveBomBuilder.Build(TwoBaySystem(), Catalog);

            var decorated = TwoBaySystem();
            decorated.DrawPallets = true;
            decorated.Dimensions = DimensionDetail.Detailed;
            decorated.NumberFronts = decorated.NumberLevels = decorated.DrawRackName = true;
            foreach (var bay in decorated.Bays)
            {
                foreach (var level in bay.Levels)
                {
                    level.PalletFrente = 40.0;
                    level.PalletAlto = 48.0;
                    level.PalletCount = 2;
                }
            }

            decorated.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TestCatalogIds.Safety.Boots.H3_16_18,
                Quantity = 1,
                Side = SafetySide.Left
            });

            var bom = SelectiveBomBuilder.Build(decorated, Catalog);

            var plainSignature = plain.Components.Select(c => (c.Category, c.ProfileId, c.Length, c.Quantity)).ToList();
            var decoratedSignature = bom.Components
                .Where(c => !string.Equals(
                    c.ProfileId,
                    TestCatalogIds.Safety.Boots.H3_16_18,
                    System.StringComparison.OrdinalIgnoreCase))
                .Select(c => (c.Category, c.ProfileId, c.Length, c.Quantity)).ToList();
            Assert.Equal(plainSignature, decoratedSignature);
            Assert.Contains(bom.Components, c => c.Quantity > 0 && string.Equals(
                c.ProfileId,
                TestCatalogIds.Safety.Boots.H3_16_18,
                System.StringComparison.OrdinalIgnoreCase)); // the safety path really ran

            // The caller's flags survived the counting pass.
            Assert.True(decorated.DrawPallets);
            Assert.Equal(DimensionDetail.Detailed, decorated.Dimensions);
            Assert.True(decorated.NumberFronts && decorated.NumberLevels && decorated.DrawRackName);
        }

        [Fact]
        public void BuildSystem_FlattenedLines_AreComponentQtyTimesPerUnit()
        {
            var bom = SelectiveBomBuilder.Build(TwoBaySystem(), Catalog);

            // Ménsulas: 2 per larguero × 8 largueros = 16; Postes: 2 per cabecera × 3 cabeceras = 6.
            Assert.Equal(16, bom.Lines.Where(l => l.Category == SelectiveBomBuilder.Mensula).Sum(l => l.Quantity));
            Assert.Equal(6, bom.Lines.Where(l => l.Category == SelectiveBomBuilder.Post).Sum(l => l.Quantity));
        }

        [Fact]
        public void BuildSystem_CustomCabeceraWithoutMaterializedMembers_StillCountsCelosia()
        {
            var system = TwoBaySystem();

            // A custom per-post cabecera as it arrives from a load / RACKEDITAR round-trip: Horizontals + BracingPanels
            // populated by the factory, but Members NOT materialized (derived data is regenerated on load).
            var custom = new RackFrameConfigurationFactory(Catalog)
                .Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 240.0, 42.0);
            Assert.Empty(custom.Members); // precondition: the celosía isn't materialized yet

            system.PostCabeceras.Clear();
            for (var i = 0; i < system.Bays.Count + 1; i++)
            {
                system.PostCabeceras.Add(i == 0 ? custom : null);
            }

            var bom = SelectiveBomBuilder.Build(system, Catalog);
            var cabeceras = bom.Components.Where(c => c.Category == BomBuilder.Cabecera).ToList();

            // EVERY cabecera (the custom one included) counts its celosía — the custom one used to drop it.
            Assert.All(cabeceras, c => Assert.Contains(c.Pieces, p => p.Category == BomBuilder.Diagonal || p.Category == BomBuilder.Horizontal));
        }
    }
}
