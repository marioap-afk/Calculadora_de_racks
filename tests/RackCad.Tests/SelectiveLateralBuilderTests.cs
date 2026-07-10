using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The selective lateral draws one cabecera per post at the frontal post Xs (so the views line up).</summary>
    public class SelectiveLateralBuilderTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign TwoBayDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                PalletDepth = 48.0
            };

            for (var b = 0; b < 2; b++)
            {
                var bay = new SelectiveBayDesign();
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                design.Bays.Add(bay);
            }

            return design;
        }

        [Fact]
        public void Cortes_OnePerPost_AtTheFrontalPostXs_EachIsACabecera()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);

            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog);

            Assert.Equal(3, cortes.Count); // 2 frentes -> 3 postes

            var expected = SelectivePostGeometry.Compute(system, Catalog).PostXs.OrderBy(x => x).ToList();
            var placed = cortes.Select(c => c.X).OrderBy(x => x).ToList();
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], placed[i], 4);
            }

            Assert.All(cortes, c => Assert.NotNull(c.Cabecera));       // each corte is its own cabecera
            Assert.All(cortes, c => Assert.True(c.Cabecera.Height > 0));
        }

        [Fact]
        public void Cortes_UseThePostsCustomCabecera_WhenOneIsSet()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);

            // The user customized post 0's cabecera to a distinct height: the corte IS that cabecera.
            var template = RackFrameTemplateCatalog.FindById("STD-3P") ?? RackFrameTemplateCatalog.Default;
            var custom = new RackFrameConfigurationFactory(Catalog).Build(template, PostId, height: 500.0, depth: 48.0);
            system.PostCabeceras[0] = custom;

            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog);

            var first = cortes.First(c => c.PostIndex == 0);
            Assert.Same(custom, first.Cabecera);
            Assert.Equal(500.0, first.Cabecera.Height, 4);
        }

        [Fact]
        public void Cortes_DistinctBeamsAtTheSameY_EachGetTheirLateralSection()
        {
            // An interior frame joins two bays. If both carry a level at the SAME height but with DIFFERENT
            // beams (here: same beam id, different peralte), the corte must show BOTH sections — deduping by Y
            // alone would silently drop the right bay's beam and disagree with the frontal.
            var system = new SelectiveRackSystem { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0, Height = 96.0 };
            var bayA = new SelectiveBay { BeamLength = 92.0, Height = 96.0 };
            bayA.Levels.Add(new SelectiveLevel { Y = 50.0, BeamId = BeamId, BeamPeralte = 4.0 });
            var bayB = new SelectiveBay { BeamLength = 92.0, Height = 96.0 };
            bayB.Levels.Add(new SelectiveLevel { Y = 50.0, BeamId = BeamId, BeamPeralte = 6.0 });
            system.Bays.Add(bayA);
            system.Bays.Add(bayB);

            var interior = new SelectiveLateralBuilder().Cortes(system, Catalog).First(c => c.PostIndex == 1);

            Assert.Equal(4, interior.Largueros.Count); // 2 distinct beams x (front + back)
            Assert.Contains(interior.Largueros, b => System.Math.Abs(b.DynamicParameters["PERALTE"] - 4.0) < 1e-6);
            Assert.Contains(interior.Largueros, b => System.Math.Abs(b.DynamicParameters["PERALTE"] - 6.0) < 1e-6);
        }

        [Fact]
        public void Planta_OneFramePerPost_PlusFrontAndBackLargueroPerBay()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);

            var instances = new SelectivePlantaBuilder().Build(system, Catalog);

            var posts = instances.Count(i => i.Role == HeaderBlockRole.Post);
            var beams = instances.Count(i => i.Role == HeaderBlockRole.Beam);

            Assert.Equal(2 * (system.Bays.Count + 1), posts); // 2 posts (front+back) per frame, N+1 frames
            Assert.Equal(2 * system.Bays.Count, beams);       // a front + a back larguero per bay

            // Frames are stacked along Y at the frente positions; each larguero carries the beam LONGITUD.
            var frenteYs = SelectivePostGeometry.Compute(system, Catalog).PostXs;
            var framePosts = instances.Where(i => i.Role == HeaderBlockRole.Post).Select(i => i.Insertion.Y).Distinct().ToList();
            Assert.All(frenteYs, y => Assert.Contains(framePosts, py => System.Math.Abs(py - y) < 1e-6));
        }

        [Fact]
        public void Planta_LargueroPlacement_TroquelSlideAndBeamLength()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);

            var beams = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == RackCad.Application.Headers.HeaderBlockRole.Beam)
                .ToList();

            var troquelEntry = Catalog.ConnectionLayout.FindConnectionLayout(PostId, "TROQUEL_LARGUERO", "PLANTA");
            var troquel = SelectivePostGeometry.Resolve(
                troquelEntry, new System.Collections.Generic.Dictionary<string, double> { ["PERALTE"] = system.PostPeralte });
            var frenteYs = SelectivePostGeometry.Compute(system, Catalog).PostXs;
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0); // the DRAWN frame depth = tarima − 6

            // Front beam at the troquel X (slides with the post peralte via the PLANTA Y-slope), back mirrored
            // at fondo - troquel.X; mate Y = frame position + troquel Y; LONGITUD = the bay's beam length.
            Assert.Contains(beams, b => !b.MirroredX
                && System.Math.Abs(b.Insertion.X - troquel.X) < 1e-6
                && System.Math.Abs(b.Insertion.Y - (frenteYs[0] + troquel.Y)) < 1e-6);
            Assert.Contains(beams, b => b.MirroredX && System.Math.Abs(b.Insertion.X - (depth - troquel.X)) < 1e-6);
            Assert.All(beams, b => Assert.Equal(system.Bays[0].BeamLength, b.DynamicParameters["LONGITUD"], 3));
        }

        [Fact]
        public void Planta_PostPeralte_GrowsWithTheDesignPeralte_NotTheProfileWidth()
        {
            // The design peralte (5) differs from the POSTE_OMEGA catalog width (3): the planta must draw the design
            // value, like the frontal — the bug was it used the fixed profile width, so a grown peralte never showed.
            var design = TwoBayDesign();
            design.PostPeralte = 5.0;
            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            var frontalPeralte = new SelectiveFrontalBuilder().Build(system, Catalog)
                .First(i => i.Role == HeaderBlockRole.Post).DynamicParameters[SelectiveRackDefaults.PeralteParam];

            var plantaPosts = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Post)
                .ToList();

            Assert.NotEmpty(plantaPosts);
            Assert.Equal(5.0, frontalPeralte, 4); // sanity: frontal already grows
            Assert.All(plantaPosts, p => Assert.Equal(5.0, p.DynamicParameters[SelectiveRackDefaults.PeralteParam], 4));
        }

        [Fact]
        public void Planta_CustomPostCabecera_DrivesItsFramesPlate()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);

            var template = RackFrameTemplateCatalog.FindStandardOrDefault();
            var custom = new RackFrameConfigurationFactory(Catalog).Build(template, PostId, 300.0, 48.0);
            custom.LeftBasePlate.PeralteOverride = 9.0;
            system.PostCabeceras[0] = custom;

            var frenteYs = SelectivePostGeometry.Compute(system, Catalog).PostXs;
            var plates = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == RackCad.Application.Headers.HeaderBlockRole.BasePlate)
                .ToList();

            // The frame stacked at post 0's frente draws the CUSTOM cabecera, so its front plate carries the override.
            Assert.Contains(plates, p => System.Math.Abs(p.ConnectionAnchor.Y - frenteYs[0]) < 1e-6
                && p.DynamicParameters.TryGetValue("PERALTE", out var v) && System.Math.Abs(v - 9.0) < 1e-6);
        }

        [Fact]
        public void Cortes_IncludeLateralLargueros_FrontAndBack_AtEachLevelY()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);

            var cortes = new SelectiveLateralBuilder().Cortes(system, Catalog);

            // End frame (post 0) touches one bay: a FRONT (X=0) and a BACK (X=fondo) larguero per level, at level.Y.
            var end = cortes.First(c => c.PostIndex == 0);
            var bay0 = system.Bays[0];
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0); // the DRAWN frame depth = tarima − 6

            Assert.Equal(bay0.Levels.Count * 2, end.Largueros.Count);
            Assert.All(end.Largueros, b => Assert.Equal(HeaderBlockRole.Beam, b.Role));

            foreach (var level in bay0.Levels)
            {
                Assert.Contains(end.Largueros, b => Math.Abs(b.Insertion.X - 0.0) < 1e-6 && Math.Abs(b.Insertion.Y - level.Y) < 1e-6);
                Assert.Contains(end.Largueros, b => Math.Abs(b.Insertion.X - depth) < 1e-6 && Math.Abs(b.Insertion.Y - level.Y) < 1e-6);
            }
        }

        [Fact]
        public void Planta_NumberFronts_EmitsANumberPerBay()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);
            system.NumberFronts = true;

            var labels = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Annotation)
                .ToList();

            Assert.Equal(2, labels.Count); // 2 bays
            Assert.Contains(labels, l => l.Text == "1");
            Assert.Contains(labels, l => l.Text == "2");
        }

        [Fact]
        public void Lateral_NumberLevels_EmitsOneNumberPerDistinctLevelInTheCorte()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);
            system.NumberLevels = true;

            var corte = new SelectiveLateralBuilder().Cortes(system, Catalog).First(c => c.PostIndex == 0);
            var labels = corte.Largueros.Where(i => i.Role == HeaderBlockRole.Annotation).ToList();
            var distinctLevelYs = corte.Largueros.Where(i => i.Role == HeaderBlockRole.Beam)
                .Select(i => Math.Round(i.Insertion.Y, 4)).Distinct().Count();

            Assert.NotEqual(0, distinctLevelYs);
            Assert.Equal(distinctLevelYs, labels.Count); // one level number per distinct larguero height
            Assert.Equal("1", labels.OrderBy(l => l.Text).First().Text);
        }
    }
}
