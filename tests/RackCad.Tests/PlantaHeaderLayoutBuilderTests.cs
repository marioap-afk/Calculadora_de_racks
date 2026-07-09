using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The cabecera PLANTA: two post footprints (front at 0, back at fondo) + plates + one celosía spanning.</summary>
    public class PlantaHeaderLayoutBuilderTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static RackFrameConfiguration Config(double depth)
        {
            var template = RackFrameTemplateCatalog.FindById("STD-3P") ?? RackFrameTemplateCatalog.Default;
            return new RackFrameConfigurationFactory(Catalog).Build(template, "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA", 132.0, depth);
        }

        [Fact]
        public void Build_TwoPostFootprints_AtOriginAndFondo_PlusPlatesAndOneCelosia()
        {
            var instances = new PlantaHeaderLayoutBuilder().Build(Config(48.0), Catalog);

            var posts = instances.Where(i => i.Role == HeaderBlockRole.Post).ToList();
            var plates = instances.Where(i => i.Role == HeaderBlockRole.BasePlate).ToList();
            var celosias = instances.Where(i => i.Role == HeaderBlockRole.Horizontal).ToList();

            Assert.Equal(2, posts.Count);
            Assert.Equal(2, plates.Count);
            Assert.Single(celosias);

            // Front post at X=0, back post at X=fondo.
            Assert.Contains(posts, p => System.Math.Abs(p.Insertion.X - 0.0) < 1e-6);
            Assert.Contains(posts, p => System.Math.Abs(p.Insertion.X - 48.0) < 1e-6);

            // The celosía is the TRAVESAÑO A-corte = fondo - 2 × (post celosía-troquel inset - ménsula overhang),
            // the SAME length it has in the lateral view; carries peralte = post peralte (3) - 1 = 2.
            var celosia = celosias[0];
            var troquelInset = Catalog.ConnectionLayout
                .FindConnectionLayout("POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA", "TROQUEL_CELOSIA", "LATERAL").LocalX;
            var mensula = Catalog.ConnectionLayout
                .FindConnectionLayout(Catalog.Defaults.HorizontalProfile, "CELOSIA", "PLANTA").LocalX;
            Assert.Equal(48.0 - 2.0 * (troquelInset - mensula), celosia.DynamicParameters["LONGITUD"], 3);
            Assert.True(celosia.DynamicParameters["LONGITUD"] < 48.0); // shorter than the fondo
            Assert.Equal(2.0, celosia.DynamicParameters["PERALTE"], 3);
        }

        [Fact]
        public void Build_PostsCarryThePostPeralte()
        {
            var instances = new PlantaHeaderLayoutBuilder().Build(Config(42.0), Catalog);

            var post = instances.First(i => i.Role == HeaderBlockRole.Post);
            Assert.Equal(3.0, post.DynamicParameters["PERALTE"], 3); // Omega 3x3 post width
        }
    }
}
