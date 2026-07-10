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
            var depth = system.PalletDepth;
            var offsets = SelectiveDepthLayout.Offsets(system, depth); // [0, 56]

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
            var offsets = SelectiveDepthLayout.Offsets(system, system.PalletDepth); // [0, 56]

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
            var beamQty = bom.Lines.Where(l => l.Category == SelectiveBomBuilder.Beam).Sum(l => l.Quantity);

            var f0Beams = system.FondoBays[0].Sum(b => b.Levels.Count);
            var f1Beams = system.FondoBays[1].Sum(b => b.Levels.Count);

            // Every fondo's real content, doubled for front + back cabeceras.
            Assert.Equal(2 * (f0Beams + f1Beams), beamQty);
            // NOT the naive "fondo 0 × 2 fondos" (which would over-count the shorter fondo 1).
            Assert.NotEqual(2 * 2 * f0Beams, beamQty);

            // Posts: (frentes+1) per fondo × 2 (front/back) × 2 fondos — post count doesn't depend on levels.
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
        public void Resolve_ShortFondoBayList_PadsWithEmptyFrentes_KeepingFondo0Count()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2
            };
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3)); // fondo 0: 3 bays
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2) }); // fondo 1 only defines 1 bay

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            // fondo 1 is padded to fondo 0's frente count; the padded bays are empty frentes with the shared width.
            Assert.Equal(system.Bays.Count, system.FondoBays[1].Count);
            Assert.Empty(system.FondoBays[1][1].Levels);
            Assert.Empty(system.FondoBays[1][2].Levels);
            Assert.Equal(system.FondoBays[0][1].BeamLength, system.FondoBays[1][1].BeamLength, 4);
        }
    }
}
