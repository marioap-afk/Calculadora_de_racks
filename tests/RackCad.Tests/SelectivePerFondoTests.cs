using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Fase 1 of doble profundidad: each fondo (back-to-back side) carries its OWN levels/heights while sharing the
    /// horizontal grid (bay widths) with fondo 0, so the posts stay aligned. A fondo's bay with no levels is an empty
    /// frente (a building column). Frontal is fondo 0; lateral/planta/BOM reflect every fondo's own content.
    /// </summary>
    public class SelectivePerFondoTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>A bay with <paramref name="cellCount"/> levels (FloorBeam so cells == resolved levels, predictable).</summary>
        private static SelectiveBayDesign Bay(int cellCount, double frente = 40.0)
        {
            var bay = new SelectiveBayDesign { FloorBeam = cellCount > 0 };
            for (var c = 0; c < cellCount; c++)
            {
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = frente, Alto = 45.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            }

            return bay;
        }

        /// <summary>2 fondos, 2 bays: fondo 0 taller (3 levels/bay), fondo 1 shorter (2 levels/bay).</summary>
        private static SelectivePalletDesign PerFondoDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.SeparatorLengths.Add(8.0);
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(2) });
            return design;
        }

        [Fact]
        public void Lateral_DobleProfundidad_DrawsSeparadoresInTheGap()
        {
            var system = new SelectiveGeometryResolver().Resolve(PerFondoDesign(), Catalog);
            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog);

            Assert.NotEmpty(cortes);
            var separadores = cortes[0].Largueros.Where(x => x.Role == HeaderBlockRole.Separator).ToList();
            Assert.True(separadores.Count >= 2); // stacked vertically, at least MinCount
            Assert.All(separadores, s => Assert.Equal("SEPARADOR_DE_CABECERA_FORMADA_DE_CINTA_CALIBRE_12_FRONTAL", s.BlockName));
            Assert.All(separadores, s => Assert.True(s.DynamicParameters[SelectiveRackDefaults.LengthParam] > 0.0)); // span the gap
        }

        [Fact]
        public void Lateral_SingleFondo_HasNoSeparadores()
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0 };
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3));
            var cortes = new SelectiveLateralBuilder().Cortes(new SelectiveGeometryResolver().Resolve(design, Catalog), Catalog);

            Assert.All(cortes, c => Assert.DoesNotContain(c.Largueros, x => x.Role == HeaderBlockRole.Separator));
        }

        [Fact]
        public void SeparatorLevelCalculator_100_CountsOneEvery100()
        {
            Assert.Equal(2, SeparatorLevelCalculator.Count(100, 100.0)); // floor(100/100)+1
            Assert.Equal(3, SeparatorLevelCalculator.Count(200, 100.0)); // floor(200/100)+1
            Assert.Equal(2, SeparatorLevelCalculator.Count(80, 100.0));  // min 2
            Assert.Equal(3, SeparatorLevelCalculator.Count(120));        // the dynamic default (60) is unchanged
        }

        [Fact]
        public void Resolve_PopulatesFondoBays_SharesWidths_KeepsOwnLevelsAndHeights()
        {
            var system = new SelectiveGeometryResolver().Resolve(PerFondoDesign(), Catalog);

            Assert.Equal(2, system.FondoBays.Count);
            Assert.Equal(3, system.FondoBays[0][0].Levels.Count); // fondo 0: 3 levels
            Assert.Equal(2, system.FondoBays[1][0].Levels.Count); // fondo 1: 2 levels

            // Shared horizontal grid: every fondo's bay adopts fondo 0's width, so the posts align.
            for (var i = 0; i < system.Bays.Count; i++)
            {
                Assert.Equal(system.FondoBays[0][i].BeamLength, system.FondoBays[1][i].BeamLength, 4);
            }

            // Distinct heights: fondo 0 (taller) governs the overall height; fondo 1 is shorter.
            var h0 = system.FondoBays[0].Max(b => b.Height);
            var h1 = system.FondoBays[1].Max(b => b.Height);
            Assert.True(h0 > h1, $"fondo 0 ({h0}) should be taller than fondo 1 ({h1})");
            Assert.Equal(h0, system.Height, 4);
        }

        [Fact]
        public void Lateral_EachFondoDrawsItsOwnLevelsAndCabecera()
        {
            var system = new SelectiveGeometryResolver().Resolve(PerFondoDesign(), Catalog);
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0); // frame depth = tarima 48 − 6 = 42
            var offsets = SelectiveDepthLayout.Offsets(system); // [0, 50]

            var corte = new SelectiveLateralBuilder().Cortes(system, Catalog).First(c => c.PostIndex == 0);

            int BeamsAt(double front, double back) => corte.Largueros.Count(b => b.Role == HeaderBlockRole.Beam
                && (System.Math.Abs(b.Insertion.X - front) < 1e-6 || System.Math.Abs(b.Insertion.X - back) < 1e-6));

            var f0 = BeamsAt(offsets[0], offsets[0] + depth);
            var f1 = BeamsAt(offsets[1], offsets[1] + depth);

            // Post 0 touches bay 0 only: fondo 0 has 3 levels, fondo 1 has 2 — each ×2 (front+back).
            Assert.Equal(system.FondoBays[0][0].Levels.Count * 2, f0);
            Assert.Equal(system.FondoBays[1][0].Levels.Count * 2, f1);
            Assert.True(f0 > f1);

            // fondo 1's own cabecera is present (its posts at the fondo-1 offset).
            Assert.Contains(corte.Largueros, i => i.Role == HeaderBlockRole.Post && System.Math.Abs(i.Insertion.X - offsets[1]) < 1e-6);
        }

        [Fact]
        public void Planta_EmptyBayInAFondo_IsAColumn_NoLargueroThere()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.SeparatorLengths.Add(8.0);
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3));
            // fondo 1: bay 0 has levels, bay 1 is EMPTY (a building column blocks that frente).
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(0) });

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var offsets = SelectiveDepthLayout.Offsets(system); // [0, 56]

            var beams = new SelectivePlantaBuilder().Build(system, Catalog).Where(i => i.Role == HeaderBlockRole.Beam).ToList();

            // fondo 0: 2 bays × (front+back) = 4; fondo 1: only bay 0 (bay 1 is a column) × 2 = 2. Total 6, not 8.
            Assert.Equal(6, beams.Count);
            Assert.Equal(2, beams.Count(b => b.Insertion.X >= offsets[1])); // fondo 1 band (X past the separator)
            Assert.Equal(4, beams.Count(b => b.Insertion.X < offsets[1]));  // fondo 0 band
        }

        [Fact]
        public void Bom_SumsEachFondosRealContent_NotAFlatMultiplier()
        {
            var system = new SelectiveGeometryResolver().Resolve(PerFondoDesign(), Catalog);

            var bom = SelectiveBomBuilder.Build(system, Catalog);
            // Largueros are COMPONENTS now (perfil + 2 ménsulas); their total quantity is the beam count.
            var beamQty = bom.Components.Where(c => c.Category == SelectiveBomBuilder.Beam).Sum(c => c.Quantity);

            var f0Beams = system.FondoBays[0].Sum(b => b.Levels.Count);
            var f1Beams = system.FondoBays[1].Sum(b => b.Levels.Count);

            // Every fondo's real content, doubled for front + back cabeceras.
            Assert.Equal(2 * (f0Beams + f1Beams), beamQty);
            // NOT the naive "fondo 0 × 2 fondos" (which would over-count the shorter fondo 1).
            Assert.NotEqual(2 * 2 * f0Beams, beamQty);

            // Posts (flattened piece total): each cabecera component = 2 posts; (frentes+1) cabeceras per fondo × 2 fondos.
            var postQty = bom.Lines.Where(l => l.Category == SelectiveBomBuilder.Post).Sum(l => l.Quantity);
            Assert.Equal((system.Bays.Count + 1) * 2 * 2, postQty);
        }

        [Fact]
        public void Resolve_WiderPalletsInFondo1_StillAdoptFondo0Width_PostsStayAligned()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.SeparatorLengths.Add(8.0);
            design.Bays.Add(Bay(3, frente: 40.0));
            // fondo 1's pallets are wider (48) — but the shared grid comes from fondo 0, so the width is fondo 0's.
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2, frente: 48.0) });

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            Assert.Equal(system.FondoBays[0][0].BeamLength, system.FondoBays[1][0].BeamLength, 4);
        }

        [Fact]
        public void Resolve_PerFondoDepth_DrivesOffsetsAndFondoDepths()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 3
            };
            design.SeparatorLengths.Add(8.0);
            design.SeparatorLengths.Add(8.0);
            design.ExtraFondoDepths.Add(40.0); // fondo 1 = 40; fondo 0 = 48 (PalletDepth); fondo 2 inherits 48
            design.Bays.Add(Bay(2));
            design.Bays.Add(Bay(2));

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            Assert.Equal(new[] { 48.0, 40.0, 48.0 }, system.FondoDepths); // pallet depths (unchanged by the rule)
            Assert.Equal(40.0, SelectiveDepthLayout.DepthOfFondo(system, 1), 4);       // pallet
            Assert.Equal(34.0, SelectiveDepthLayout.CabeceraDepthOfFondo(system, 1), 4); // cabecera = 40 − 6

            var offsets = SelectiveDepthLayout.Offsets(system);
            Assert.Equal(0.0, offsets[0], 4);
            Assert.Equal(50.0, offsets[1], 4); // 0 + cabecera(fondo 0)=42 + sep=8
            Assert.Equal(92.0, offsets[2], 4); // 50 + cabecera(fondo 1)=34 + sep=8
        }

        [Fact]
        public void CabeceraDepth_UsesTheOverrideWhenSet_ElseTheRule()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.SeparatorLengths.Add(8.0);
            design.CabeceraFondoOverrides.Add(30.0); // fondo 0: custom cabecera fondo (ignores the rule)
            design.CabeceraFondoOverrides.Add(0.0);  // fondo 1: blank -> derived (48 − 6 = 42)
            design.Bays.Add(Bay(2));

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            Assert.Equal(30.0, SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0), 4); // override wins
            Assert.Equal(42.0, SelectiveDepthLayout.CabeceraDepthOfFondo(system, 1), 4); // rule: tarima 48 − 6
            Assert.Equal(48.0, SelectiveDepthLayout.DepthOfFondo(system, 0), 4);          // the pallet is untouched
        }

        [Fact]
        public void Lateral_PerFondoDepth_BackAtEachFondosOwnDepth()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.SeparatorLengths.Add(8.0);
            design.ExtraFondoDepths.Add(40.0); // fondo 1 is shallower (its own fondo)
            design.Bays.Add(Bay(2));

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var offsets = SelectiveDepthLayout.Offsets(system); // [0, 56]

            var corte = new SelectiveLateralBuilder().Cortes(system, Catalog).First(c => c.PostIndex == 0);

            // Cabecera depths: fondo 0 = 48−6 = 42, fondo 1 = 40−6 = 34. Back at 0+42 and offsets[1]+34 — beams AND fondo 1's cabecera post.
            var cab0 = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0); // 42
            var cab1 = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 1); // 34
            Assert.Contains(corte.Largueros, b => b.Role == HeaderBlockRole.Beam && System.Math.Abs(b.Insertion.X - cab0) < 1e-6);
            Assert.Contains(corte.Largueros, b => b.Role == HeaderBlockRole.Beam && System.Math.Abs(b.Insertion.X - (offsets[1] + cab1)) < 1e-6);
            Assert.Contains(corte.Largueros, p => p.Role == HeaderBlockRole.Post && System.Math.Abs(p.Insertion.X - (offsets[1] + cab1)) < 1e-6);
        }

        [Fact]
        public void Resolve_ShorterFondo_KeepsOwnCount_SharesMasterPrefixWidths()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3)); // fondo 0: 3 frentes (the longest → master)
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2) }); // fondo 1: only 1 frente

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            // Each fondo keeps its OWN frente count now (no padding to fondo 0). Fondo 0 is the longest (the master).
            Assert.Equal(3, system.FondoBays[0].Count);
            Assert.Single(system.FondoBays[1]);
            Assert.Equal(0, SelectiveDepthLayout.MasterFondoIndex(system));
            // Its one overlapping frente shares the master width so the posts coincide.
            Assert.Equal(system.FondoBays[0][0].BeamLength, system.FondoBays[1][0].BeamLength, 4);
        }

        [Fact]
        public void Resolve_LongerExtraFondo_IsMaster_ShorterFondoAdoptsItsWidths()
        {
            // A corner layout: fondo 0 has 3 frentes @ 40", fondo 1 has 6 frentes @ 48" (the longest → the width master).
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.Bays.Add(Bay(2, 40.0));
            design.Bays.Add(Bay(2, 40.0));
            design.Bays.Add(Bay(2, 40.0));
            var fondo1 = new List<SelectiveBayDesign>();
            for (var i = 0; i < 6; i++) fondo1.Add(Bay(2, 48.0));
            design.ExtraFondoBays.Add(fondo1);

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            Assert.Equal(3, system.FondoBays[0].Count); // fondo 0 keeps its 3 frentes
            Assert.Equal(6, system.FondoBays[1].Count); // fondo 1 keeps its 6 — no truncation
            Assert.Equal(1, SelectiveDepthLayout.MasterFondoIndex(system)); // fondo 1 is the longest

            // Fondo 0's overlapping frentes adopt the MASTER (fondo 1, 48") widths so the shared posts coincide.
            for (var i = 0; i < 3; i++)
            {
                Assert.Equal(system.FondoBays[1][i].BeamLength, system.FondoBays[0][i].BeamLength, 4);
            }

            // The master width (48" pallets ×2) is wider than fondo 0's own 40" frente would have been.
            Assert.True(system.FondoBays[0][0].BeamLength > 40.0 * 2);
        }

        [Fact]
        public void Resolve_EmptyColumnInMasterFondo_DoesNotZeroOtherFondosWidth()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.Bays.Add(Bay(3)); // fondo 0: 1 loaded frente
            // fondo 1: 3 frentes → the longest (master); its FIRST frente is an empty column (no levels → BeamLength 0).
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(0), Bay(2), Bay(2) });

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            Assert.Equal(1, SelectiveDepthLayout.MasterFondoIndex(system)); // fondo 1 is the longest
            // The master's empty column must NOT collapse fondo 0's real frente to zero width.
            Assert.True(system.FondoBays[0][0].BeamLength > 0.0);
            // Both fondos share that (real) width at the overlapping index so their posts still coincide.
            Assert.Equal(system.FondoBays[0][0].BeamLength, system.FondoBays[1][0].BeamLength, 4);
        }

        [Fact]
        public void Planta_CornerLayout_PlacesEachFondosOwnFrames_OnMasterGrid()
        {
            // fondo 0: 3 frentes (4 posts); fondo 1: 6 frentes (7 posts). Master grid = fondo 1's.
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            for (var i = 0; i < 3; i++) design.Bays.Add(Bay(2));
            var fondo1 = new List<SelectiveBayDesign>();
            for (var i = 0; i < 6; i++) fondo1.Add(Bay(2));
            design.ExtraFondoBays.Add(fondo1);

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var instances = new SelectivePlantaBuilder().Build(system, Catalog);

            // Each planta cabecera-frame is 2 posts (front + back along depth). fondo 0's 4 frames + fondo 1's 7 frames
            // = 11 frames × 2 = 22 posts.
            Assert.Equal((4 + 7) * 2, instances.Count(i => i.Role == HeaderBlockRole.Post));
        }

        [Fact]
        public void Lateral_CornerLayout_HasOneCortePerMasterPost()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            for (var i = 0; i < 3; i++) design.Bays.Add(Bay(2));
            var fondo1 = new List<SelectiveBayDesign>();
            for (var i = 0; i < 6; i++) fondo1.Add(Bay(2));
            design.ExtraFondoBays.Add(fondo1);

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog);

            // The master (fondo 1) has 6 frentes → 7 posts → 7 cortes.
            Assert.Equal(7, cortes.Count);
            // The far cortes (beyond fondo 0's 3 frentes) exist and are anchored on fondo 1 (fondo 0 doesn't reach them).
            Assert.All(cortes, c => Assert.NotNull(c.Cabecera));
        }
    }
}
