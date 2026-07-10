using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Doble profundidad (espalda con espalda): N fondos = N cabecera-lines repeated along the depth axis, each with
    /// its own front/back largueros, separated by the per-gap separadores. Frontal is unchanged; only lateral, planta
    /// and BOM grow. A single fondo (DepthCount = 1) must stay geometrically identical to the classic selective.
    /// </summary>
    public class SelectiveDepthTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign DepthDesign(int fondos, params double[] separators)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                PalletDepth = 48.0,
                DepthCount = fondos
            };

            foreach (var separator in separators) design.SeparatorLengths.Add(separator);

            for (var b = 0; b < 2; b++)
            {
                var bay = new SelectiveBayDesign();
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                design.Bays.Add(bay);
            }

            return design;
        }

        // ---- SelectiveDepthLayout (the offset/separator rules) ----

        [Fact]
        public void Offsets_AreCumulative_WithPerGapSeparators()
        {
            var system = new SelectiveRackSystem { DepthCount = 3, PalletDepth = 48.0 };
            system.SeparatorLengths.Add(8.0);
            system.SeparatorLengths.Add(12.0);

            var offsets = SelectiveDepthLayout.Offsets(system, 48.0);

            // 0, 0+48+8, 56+48+12
            Assert.Equal(new[] { 0.0, 56.0, 116.0 }, offsets);
        }

        [Fact]
        public void Offsets_SingleSeparator_FillsEveryGap()
        {
            var system = new SelectiveRackSystem { DepthCount = 3, PalletDepth = 48.0 };
            system.SeparatorLengths.Add(8.0); // only one value given -> reused for both gaps

            Assert.Equal(new[] { 0.0, 56.0, 112.0 }, SelectiveDepthLayout.Offsets(system, 48.0));
        }

        [Fact]
        public void Offsets_NoSeparators_UseTheDefault()
        {
            var system = new SelectiveRackSystem { DepthCount = 4, PalletDepth = 48.0 };

            var step = 48.0 + SelectiveRackDefaults.DefaultSeparator;
            Assert.Equal(new[] { 0.0, step, 2 * step, 3 * step }, SelectiveDepthLayout.Offsets(system, 48.0));
        }

        [Fact]
        public void Offsets_SingleFondo_IsJustTheOrigin()
        {
            var system = new SelectiveRackSystem { DepthCount = 1, PalletDepth = 48.0 };
            Assert.Equal(new[] { 0.0 }, SelectiveDepthLayout.Offsets(system, 48.0));
        }

        // ---- Resolver pass-through ----

        [Fact]
        public void Resolve_PassesDepthCountAndSeparators()
        {
            var system = new SelectiveGeometryResolver().Resolve(DepthDesign(3, 8.0, 12.0), Catalog);

            Assert.Equal(3, system.DepthCount);
            Assert.Equal(new[] { 8.0, 12.0 }, system.SeparatorLengths);
        }

        [Fact]
        public void Resolve_ClampsDepthCountToAtLeastOne()
        {
            var design = DepthDesign(2, 8.0);
            design.DepthCount = 0;

            Assert.Equal(1, new SelectiveGeometryResolver().Resolve(design, Catalog).DepthCount);
        }

        // ---- Lateral ----

        [Fact]
        public void Lateral_SingleFondo_HasNoExtraCabeceraPieces()
        {
            var system = new SelectiveGeometryResolver().Resolve(DepthDesign(1), Catalog);

            var end = new SelectiveLateralBuilder().Cortes(system, Catalog).First(c => c.PostIndex == 0);

            // The base cabecera is `corte.Cabecera`; a single fondo adds NO extra cabecera pieces to the extras list.
            Assert.DoesNotContain(end.Largueros, i => i.Role == HeaderBlockRole.Post);
            Assert.DoesNotContain(end.Largueros, i => i.Role == HeaderBlockRole.BasePlate);
        }

        [Fact]
        public void Lateral_DoubleDepth_RepeatsTheCabeceraAndLargueros_AtEachFondo()
        {
            var system = new SelectiveGeometryResolver().Resolve(DepthDesign(2, 8.0), Catalog);
            var depth = system.PalletDepth;
            var offsets = SelectiveDepthLayout.Offsets(system, depth); // [0, 56]

            var end = new SelectiveLateralBuilder().Cortes(system, Catalog).First(c => c.PostIndex == 0);

            // Fondo 1's cabecera (fondo 0 is `corte.Cabecera`) shows up translated: its front post at X=56, back at 104.
            var extraPosts = end.Largueros.Where(i => i.Role == HeaderBlockRole.Post).ToList();
            Assert.Contains(extraPosts, p => Math.Abs(p.Insertion.X - offsets[1]) < 1e-6);
            Assert.Contains(extraPosts, p => Math.Abs(p.Insertion.X - (offsets[1] + depth)) < 1e-6);

            // Largueros: front + back at EACH fondo, per resolved level -> X in {0, 48, 56, 104}.
            var beams = end.Largueros.Where(i => i.Role == HeaderBlockRole.Beam).ToList();
            var levels = system.Bays[0].Levels;
            Assert.Equal(levels.Count * 2 * offsets.Count, beams.Count);

            foreach (var level in levels)
            {
                foreach (var offset in offsets)
                {
                    Assert.Contains(beams, b => Math.Abs(b.Insertion.X - offset) < 1e-6 && Math.Abs(b.Insertion.Y - level.Y) < 1e-6);
                    Assert.Contains(beams, b => Math.Abs(b.Insertion.X - (offset + depth)) < 1e-6 && Math.Abs(b.Insertion.Y - level.Y) < 1e-6);
                }
            }
        }

        // ---- Planta ----

        [Fact]
        public void Planta_DoubleDepth_RepeatsFramesAndLargueros_OffsetInX()
        {
            var system = new SelectiveGeometryResolver().Resolve(DepthDesign(2, 8.0), Catalog);
            var depth = system.PalletDepth;
            var offsets = SelectiveDepthLayout.Offsets(system, depth); // [0, 56]

            var instances = new SelectivePlantaBuilder().Build(system, Catalog);
            var posts = instances.Count(i => i.Role == HeaderBlockRole.Post);
            var beams = instances.Count(i => i.Role == HeaderBlockRole.Beam);

            // 2 posts/frame x (bays+1) frames x fondos; front+back larguero per bay x fondos.
            Assert.Equal(2 * (system.Bays.Count + 1) * offsets.Count, posts);
            Assert.Equal(2 * system.Bays.Count * offsets.Count, beams);

            // The second fondo's frame is offset along X (the depth axis) by depth + separator = 56 (front) / 104 (back).
            var postXs = instances.Where(i => i.Role == HeaderBlockRole.Post).Select(i => Math.Round(i.Insertion.X, 4)).Distinct().ToList();
            Assert.Contains(postXs, x => Math.Abs(x - offsets[1]) < 1e-6);
            Assert.Contains(postXs, x => Math.Abs(x - (offsets[1] + depth)) < 1e-6);
        }

        [Fact]
        public void Planta_SingleFondo_MatchesTheClassicCounts()
        {
            var system = new SelectiveGeometryResolver().Resolve(DepthDesign(1), Catalog);

            var instances = new SelectivePlantaBuilder().Build(system, Catalog);

            Assert.Equal(2 * (system.Bays.Count + 1), instances.Count(i => i.Role == HeaderBlockRole.Post));
            Assert.Equal(2 * system.Bays.Count, instances.Count(i => i.Role == HeaderBlockRole.Beam));
        }
    }
}
