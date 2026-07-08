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

        private static SelectiveRackSystem System(double postPeralte = 3.0)
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = postPeralte };
            system.Bays.Add(new SelectiveBay
            {
                BeamId = BeamId,
                BeamPeralte = 4.0,
                BeamLength = 100.0,
                Levels = 4,
                FirstLevel = 48.0,
                Separation = 48.0
            });
            return system;
        }

        /// <summary>The post's larguero-troquel X resolved from the catalog (base + slope*peralte); never hardcoded.</summary>
        private static double TroquelX(double peralte)
        {
            var entry = Catalog.ConnectionLayout.FindConnectionLayout(PostId, "TROQUEL_LARGUERO", "FRONTAL");
            return entry.LocalX + entry.LocalXPorParam * peralte;
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
        public void Build_PostSpacing_IsBeamLengthPlusTwoTroquelX()
        {
            var posts = new SelectiveFrontalBuilder().Build(System(), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Post)
                .OrderBy(i => i.Insertion.X)
                .ToList();

            // Post-to-post = larguero length (100) + 2*troquelX, read from the catalog.
            Assert.Equal(0.0, posts[0].Insertion.X, 4);
            Assert.Equal(100.0 + 2.0 * TroquelX(3.0), posts[1].Insertion.X, 4);
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
        public void Build_Levels_SnapToTheTroquelGrid()
        {
            var ys = new SelectiveFrontalBuilder().Build(System(), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Beam)
                .Select(i => Math.Round(i.Insertion.Y, 3))
                .OrderBy(y => y)
                .ToList();

            Assert.Equal(new[] { 48.0, 96.0, 144.0, 192.0 }, ys);
        }
    }
}
