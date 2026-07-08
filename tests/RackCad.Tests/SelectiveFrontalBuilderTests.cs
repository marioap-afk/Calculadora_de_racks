using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
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
    }
}
