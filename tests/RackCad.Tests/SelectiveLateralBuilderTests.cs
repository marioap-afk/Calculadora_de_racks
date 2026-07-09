using System.Linq;
using RackCad.Application.Catalogs;
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
    }
}
