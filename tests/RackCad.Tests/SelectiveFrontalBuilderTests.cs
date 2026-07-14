using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class SelectiveFrontalBuilderTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static readonly double[] LevelYs = { 48.0, 96.0, 144.0, 192.0 };

        /// <summary>A resolved system (levels already placed) so the builder tests exercise placement only.</summary>
        private static SelectiveRackSystem System(double postPeralte = 3.0)
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = postPeralte };
            var bay = new SelectiveBay { BeamLength = 100.0, Height = 240.0 };
            foreach (var y in LevelYs)
            {
                bay.Levels.Add(new SelectiveLevel { Y = y, BeamId = BeamId, BeamPeralte = 4.0 });
            }

            system.Bays.Add(bay);
            return system;
        }

        /// <summary>A resolved TWO-bay system (3 posts) with an explicit per-post peralte for each post.</summary>
        private static SelectiveRackSystem TwoBaySystem(double p0, double p1, double p2)
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = p0 };
            for (var b = 0; b < 2; b++)
            {
                var bay = new SelectiveBay { BeamLength = 100.0, Height = 240.0 };
                bay.Levels.Add(new SelectiveLevel { Y = 48.0, BeamId = BeamId, BeamPeralte = 4.0 });
                system.Bays.Add(bay);
            }

            system.PostPeraltes.Add(p0);
            system.PostPeraltes.Add(p1);
            system.PostPeraltes.Add(p2);
            return system;
        }

        private static System.Collections.Generic.List<double> PostXs(SelectiveRackSystem system)
            => new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Post)
                .Select(i => i.Insertion.X)
                .OrderBy(x => x)
                .ToList();

        /// <summary>The post's larguero-troquel X resolved from the catalog (base + slope*peralte); never hardcoded.</summary>
        private static double TroquelX(double peralte)
        {
            var entry = Catalog.ConnectionLayout.FindConnectionLayout(PostId, "TROQUEL_LARGUERO", "FRONTAL");
            return entry.LocalX + entry.LocalXPorParam * peralte;
        }

        /// <summary>The larguero's INICIO_PERFIL X (ménsula overhang from the hook to the profile start); from the catalog.</summary>
        private static double InicioPerfilX(double beamPeralte = 4.0)
        {
            var entry = Catalog.ConnectionLayout.FindConnectionLayout(BeamId, "INICIO_PERFIL", "FRONTAL");
            return entry == null ? 0.0 : entry.LocalX + entry.LocalXPorParam * beamPeralte;
        }

        private static double FirstBeamX(double postPeralte)
            => new SelectiveFrontalBuilder().Build(System(postPeralte), Catalog)
                .First(i => i.Role == HeaderBlockRole.Beam).Insertion.X;

        [Fact]
        public void Build_ProducesOnePostAndPlatePerCabecera_PlusOneBeamPerLevel()
        {
            var instances = new SelectiveFrontalBuilder().Build(System(), Catalog);

            Assert.Equal(2, instances.Count(i => i.Role == HeaderBlockRole.Post));       // N+1 cabeceras
            Assert.Equal(2, instances.Count(i => i.Role == HeaderBlockRole.BasePlate));
            Assert.Equal(4, instances.Count(i => i.Role == HeaderBlockRole.Beam));       // 4 levels
        }

        [Fact]
        public void Build_PostSpacing_IsBeamLengthPlusTwoTroquelXPlusTwoInicioPerfilX()
        {
            var posts = new SelectiveFrontalBuilder().Build(System(), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Post)
                .OrderBy(i => i.Insertion.X)
                .ToList();

            // LONGITUD is the profile "A corte": post-to-post = length (100) + 2*(troquelX + inicioPerfilX),
            // both offsets read from the catalog. The ménsula overhang (inicioPerfilX) is added on each end.
            Assert.Equal(0.0, posts[0].Insertion.X, 4);
            Assert.Equal(100.0 + 2.0 * (TroquelX(3.0) + InicioPerfilX()), posts[1].Insertion.X, 4);
        }

        [Fact]
        public void Build_BeamX_FollowsPostPeralte_PerTheCatalog()
        {
            var x3 = FirstBeamX(3.0);
            var x5 = FirstBeamX(5.0);

            // Resolved from the catalog (X = localX + localXPorParam * peralte), and the slope is applied.
            Assert.Equal(TroquelX(3.0), x3, 4);
            Assert.Equal(TroquelX(5.0), x5, 4);
            Assert.True(x5 > x3);
        }

        [Fact]
        public void Build_PerPostPeralte_EachPostCarriesItsOwn()
        {
            var posts = new SelectiveFrontalBuilder().Build(TwoBaySystem(3.0, 6.0, 4.0), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Post)
                .OrderBy(i => i.Insertion.X)
                .ToList();

            Assert.Equal(3, posts.Count);
            Assert.Equal(3.0, posts[0].DynamicParameters["PERALTE"], 4);
            Assert.Equal(6.0, posts[1].DynamicParameters["PERALTE"], 4);
            Assert.Equal(4.0, posts[2].DynamicParameters["PERALTE"], 4);
        }

        [Fact]
        public void Build_PerPostPeralte_BiggerMiddlePostWidensBothAdjacentBays()
        {
            var uniform = PostXs(TwoBaySystem(3.0, 3.0, 3.0));
            var varied = PostXs(TwoBaySystem(3.0, 6.0, 3.0)); // only the middle post is bigger

            // post 0 anchored at 0 in both. The middle post's LEFT troquel grew → bay 0 wider; its RIGHT troquel grew
            // → bay 1 wider too. Each bay adapts to the peralte of the posts that bound it.
            Assert.Equal(uniform[0], varied[0], 4);
            Assert.True(varied[1] > uniform[1]);                                   // bay 0 widened
            Assert.True(varied[2] - varied[1] > uniform[2] - uniform[1]);          // bay 1 widened
        }

        [Fact]
        public void Build_NoAnnotationFlags_EmitsNoText()
        {
            var labels = new SelectiveFrontalBuilder().Build(System(), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Annotation);
            Assert.Empty(labels);
        }

        [Fact]
        public void Build_NumberFronts_EmitsANumberPerBay()
        {
            var system = TwoBaySystem(3.0, 3.0, 3.0);
            system.NumberFronts = true;

            var labels = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Annotation)
                .ToList();

            Assert.Equal(2, labels.Count); // 2 bays → "1", "2"
            Assert.Contains(labels, l => l.Text == "1");
            Assert.Contains(labels, l => l.Text == "2");
        }

        [Fact]
        public void Build_DrawRackName_EmitsTheNameAboveTheRack()
        {
            var system = System();
            system.DrawRackName = true;
            system.Name = "Rack A";

            var label = new SelectiveFrontalBuilder().Build(system, Catalog)
                .First(i => i.Role == HeaderBlockRole.Annotation);

            Assert.Equal("Rack A", label.Text);
            Assert.True(label.Insertion.Y > system.Height); // drawn above the rack
        }

        [Fact]
        public void Build_DrawBasePlateOff_OmitsThePlates()
        {
            var system = System();
            system.DrawBasePlate = false;

            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);

            Assert.DoesNotContain(instances, i => i.Role == HeaderBlockRole.BasePlate);
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Post); // posts still there
        }

        [Fact]
        public void Build_Beam_CarriesLengthAndPeralte()
        {
            var beam = new SelectiveFrontalBuilder().Build(System(), Catalog)
                .First(i => i.Role == HeaderBlockRole.Beam);

            Assert.Equal(100.0, beam.DynamicParameters["LONGITUD"], 4);
            Assert.Equal(4.0, beam.DynamicParameters["PERALTE"], 4);
        }

        [Fact]
        public void Build_Plate_PeralteIsTheStandardDerivedFromThePost()
        {
            var plateEntry = Catalog.BasePlates.FindBasePlate(Catalog.Defaults.BasePlate);

            var plate = new SelectiveFrontalBuilder().Build(System(3.0), Catalog)
                .First(i => i.Role == HeaderBlockRole.BasePlate);

            // Standard plate peralte = peralteBase + peraltePorPeraltePoste * postPeralte (read from base-plates.csv).
            Assert.Equal(plateEntry.StandardPeralte(3.0), plate.DynamicParameters["PERALTE"], 4);
        }

        [Fact]
        public void Build_PostHeight_IsTallestAdjacentBay()
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = 3.0 };
            var tallBay = new SelectiveBay { BeamLength = 100.0, Height = 240.0 };
            tallBay.Levels.Add(new SelectiveLevel { Y = 48.0, BeamId = BeamId, BeamPeralte = 4.0 });
            var shortBay = new SelectiveBay { BeamLength = 100.0, Height = 120.0 };
            shortBay.Levels.Add(new SelectiveLevel { Y = 48.0, BeamId = BeamId, BeamPeralte = 4.0 });
            system.Bays.Add(tallBay);
            system.Bays.Add(shortBay);

            var heights = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Post)
                .OrderBy(i => i.Insertion.X)
                .Select(i => i.DynamicParameters["LONGITUD"])
                .ToList();

            // 3 postes: post0 solo toca la bahía alta (240); post1 toca ambas → máx (240); post2 solo la baja (120).
            Assert.Equal(new[] { 240.0, 240.0, 120.0 }, heights);
        }

        [Fact]
        public void Build_PostPlatePeralte_ComesFromTheCabeceraOverrideElseDerived()
        {
            var system = System(); // 1 bay -> 2 posts; postPeralte 3 -> default plate peralte 4 (base 1 + 1*3)
            system.PostCabeceras.Add(new RackFrameConfiguration { LeftBasePlate = new BasePlatePlacement { PeralteOverride = 7.0 } });
            system.PostCabeceras.Add(null); // post 1 -> run default

            var plates = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.BasePlate)
                .OrderBy(i => i.Insertion.X)
                .ToList();

            Assert.Equal(2, plates.Count);
            Assert.Equal(7.0, plates[0].DynamicParameters["PERALTE"], 4); // from the cabecera
            Assert.Equal(4.0, plates[1].DynamicParameters["PERALTE"], 4); // derived (StandardPeralte)
        }

        [Fact]
        public void Build_Beams_SitAtTheResolvedLevelYs()
        {
            var ys = new SelectiveFrontalBuilder().Build(System(), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Beam)
                .Select(i => Math.Round(i.Insertion.Y, 3))
                .OrderBy(y => y)
                .ToList();

            // The builder places each larguero at the level's already-resolved Y (no snapping here).
            Assert.Equal(LevelYs, ys);
        }

        // ---- Tarimas (pallet visual reference; DrawPallets) ----

        /// <summary>A resolved system whose levels each carry a pallet (frente/alto/count), for the tarima tests.</summary>
        private static SelectiveRackSystem PalletSystem(int perLevelCount = 2, double frente = 40.0, double alto = 48.0)
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = 3.0 };
            var bay = new SelectiveBay { BeamLength = 100.0, Height = 240.0 };
            foreach (var y in LevelYs)
            {
                bay.Levels.Add(new SelectiveLevel { Y = y, BeamId = BeamId, BeamPeralte = 4.0, PalletFrente = frente, PalletAlto = alto, PalletCount = perLevelCount });
            }

            system.Bays.Add(bay);
            return system;
        }

        [Fact]
        public void Build_DrawPalletsOff_EmitsNoPallets()
        {
            var pallets = new SelectiveFrontalBuilder().Build(PalletSystem(), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Pallet);
            Assert.Empty(pallets); // default (DrawPallets false) draws nothing
        }

        [Fact]
        public void Build_DrawPallets_EmitsCountPalletsPerLevel()
        {
            var system = PalletSystem(perLevelCount: 2);
            system.DrawPallets = true;

            var pallets = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Pallet)
                .ToList();

            Assert.Equal(2 * LevelYs.Length, pallets.Count); // 2 pallets × 4 levels
        }

        [Fact]
        public void Build_DrawPallets_CarryFrenteAndAlto()
        {
            var system = PalletSystem(perLevelCount: 1, frente: 42.0, alto: 50.0);
            system.DrawPallets = true;

            var pallet = new SelectiveFrontalBuilder().Build(system, Catalog)
                .First(i => i.Role == HeaderBlockRole.Pallet);

            Assert.Equal(42.0, pallet.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam], 4);
            Assert.Equal(50.0, pallet.DynamicParameters[SelectiveRackDefaults.PalletAltoParam], 4);
        }

        [Fact]
        public void Build_DrawPallets_SitOnTheLoadSurfaceAboveEachLevel()
        {
            var system = PalletSystem(perLevelCount: 1);
            system.DrawPallets = true;

            var surface = SelectivePostGeometry.BeamProfileStartY(Catalog, BeamId, 4.0, "FRONTAL");
            // Bottom-centre origin: Insertion.Y IS the pallet bottom, which rests on the level's load surface.
            var ys = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Pallet)
                .Select(i => Math.Round(i.Insertion.Y, 3))
                .OrderBy(y => y)
                .ToList();

            // Each pallet's bottom sits at its level's Y plus the beam's escalón (INICIO_PERFIL Y).
            Assert.Equal(LevelYs.Select(y => Math.Round(y + surface, 3)), ys);
        }

        [Fact]
        public void Build_DrawPallets_DistributedEvenlyWithinTheBay()
        {
            var system = PalletSystem(perLevelCount: 2, frente: 40.0);
            system.DrawPallets = true;

            var layout = SelectivePostGeometry.Compute(system, Catalog);
            // The pallet rests on the larguero PROFILE, which starts at the ménsula overhang (INICIO_PERFIL X) past the troquel.
            var anchorX = layout.PostXs[0] + layout.TroquelXs[0] + InicioPerfilX();

            // Level-0 pallets: bottom Y = level.Y + surface (bottom-centre origin, Insertion.Y is the bottom).
            var bottomY0 = LevelYs[0] + SelectivePostGeometry.BeamProfileStartY(Catalog, BeamId, 4.0, "FRONTAL");
            var xs = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Pallet)
                .Where(i => Math.Abs(i.Insertion.Y - bottomY0) < 0.001)
                .Select(i => i.Insertion.X)
                .OrderBy(x => x)
                .ToList();

            // 2 pallets of 40 across span 100 → gap = (100 - 80)/3 = 6.667. Insertion is the CENTRE = footprint-left + 20.
            var gap = (100.0 - 2 * 40.0) / 3.0;
            Assert.Equal(2, xs.Count);
            Assert.Equal(anchorX + gap + 20.0, xs[0], 3);
            Assert.Equal(anchorX + 2 * gap + 40.0 + 20.0, xs[1], 3);
        }

        [Fact]
        public void Build_DrawPallets_FloorPalletSitsOnTheFloor()
        {
            var system = new SelectiveRackSystem { Height = 60.0, PostId = PostId, PostPeralte = 3.0, DrawPallets = true };
            var bay = new SelectiveBay { BeamLength = 100.0, Height = 60.0, FloorPalletFrente = 40.0, FloorPalletAlto = 48.0, FloorPalletCount = 2 };
            system.Bays.Add(bay); // a floor-only bay: no larguero levels, just a ground pallet

            var pallets = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Pallet)
                .ToList();

            Assert.Equal(2, pallets.Count);
            // Bottom-centre origin: Insertion.Y is the pallet bottom, which rests on the floor (Y=0).
            Assert.All(pallets, p => Assert.Equal(0.0, p.Insertion.Y, 4));
        }

        [Fact]
        public void Build_DrawPallets_RestOnTheLargueroProfile_NotTheBareTroquel()
        {
            var system = PalletSystem(perLevelCount: 1, frente: 40.0);
            system.DrawPallets = true;

            var layout = SelectivePostGeometry.Compute(system, Catalog);
            var pallet = new SelectiveFrontalBuilder().Build(system, Catalog)
                .First(i => i.Role == HeaderBlockRole.Pallet);

            // 1 pallet of 40 across span 100 → gap = (100-40)/2 = 30. Anchor is the PROFILE start (troquel + INICIO_PERFIL),
            // the same datum the larguero rests on — NOT the bare troquel. Guards the X-offset fix. Insertion is the
            // pallet CENTRE = profile start + gap(30) + frente/2(20) = the bay centre.
            var profileX = layout.PostXs[0] + layout.TroquelXs[0] + InicioPerfilX();
            Assert.Equal(profileX + 50.0, pallet.Insertion.X, 3);
        }

        [Fact]
        public void Build_DrawPallets_MedioFrente_DrawsPalletsOnEachLoadedTramo()
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = 3.0, DrawPallets = true };
            var bay = new SelectiveBay { BeamLength = 200.0, Height = 240.0 };
            bay.Levels.Add(new SelectiveLevel { Y = 48.0, BeamId = BeamId, BeamPeralte = 4.0, PalletFrente = 40.0, PalletAlto = 48.0, PalletCount = 3 });
            bay.Segments.Add(new SelectiveSegment { Length = 60.0, Loaded = true });  // tramo 1: loaded
            bay.Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });   // tramo 2: remainder, loaded
            system.Bays.Add(bay);

            var pallets = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Pallet)
                .ToList();

            // Both tramos are loaded and each is wider than one 40" pallet, so a medio-frente bay now DRAWS pallets
            // (regression: it used to skip them entirely while still drawing largueros).
            Assert.True(pallets.Count >= 2, $"expected at least one pallet per loaded tramo, got {pallets.Count}");
        }

        [Fact]
        public void Build_DrawPallets_MedioFrente_SkipsUnloadedTramos()
        {
            var loadedOnly = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = 3.0, DrawPallets = true };
            var bay = new SelectiveBay { BeamLength = 200.0, Height = 240.0 };
            bay.Levels.Add(new SelectiveLevel { Y = 48.0, BeamId = BeamId, BeamPeralte = 4.0, PalletFrente = 40.0, PalletAlto = 48.0, PalletCount = 3 });
            bay.Segments.Add(new SelectiveSegment { Length = 60.0, Loaded = true });   // loaded
            bay.Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = false });   // remainder, NOT loaded
            loadedOnly.Bays.Add(bay);

            var pallets = new SelectiveFrontalBuilder().Build(loadedOnly, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Pallet)
                .ToList();

            // Only the 60" tramo is loaded → floor(60/40) = 1 pallet; the unloaded remainder draws none.
            Assert.Single(pallets);
        }

        [Fact]
        public void Build_DrawPallets_SurvivesTheFondoSystemViewProjection()
        {
            // The insert/redraw path draws through SelectiveDepthLayout.FondoSystemView, NOT the whole system. Exercise
            // that SAME projection so a future edit that drops `DrawPallets = system.DrawPallets` (SelectiveDepthLayout)
            // is caught here — otherwise every inserted rack would silently lose its tarimas with the suite still green.
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0, DrawPallets = true };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            design.Bays.Add(bay);

            var resolved = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var fondoView = SelectiveDepthLayout.FondoSystemView(resolved, 0);

            Assert.True(fondoView.DrawPallets); // the projection must carry the flag
            var pallets = new SelectiveFrontalBuilder().Build(fondoView, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Pallet);
            Assert.NotEmpty(pallets);
        }
    }
}
