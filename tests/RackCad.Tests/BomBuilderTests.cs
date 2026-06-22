using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class BomBuilderTests
    {
        private const string PostDescription = "Poste Omega 3x3 calibre 14";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static RackFrameConfiguration StandardWithMembers()
        {
            var configuration = new RackFrameConfigurationFactory(Catalog)
                .Build(RackFrameTemplateCatalog.Default, CatalogIds.StandardPost, 132.0, 42.0);
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);
            return configuration;
        }

        [Fact]
        public void Build_GroupsPostsToQuantityTwoWithCatalogDescription()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            var post = bom.Lines.Single(l => l.Category == BomBuilder.Post && l.ProfileId == CatalogIds.StandardPost);
            Assert.Equal(2, post.Quantity);
            Assert.Equal(132.0, post.Length);
            Assert.Equal(PostDescription, post.Description);
        }

        [Fact]
        public void Build_AggregatesHorizontalsAndDiagonals()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            // Horizontals and diagonals are all one truss profile now, so they aggregate by category.
            Assert.Equal(5, bom.Lines.Where(l => l.Category == BomBuilder.Horizontal).Sum(l => l.Quantity));
            Assert.Equal(3, bom.Lines.Where(l => l.Category == BomBuilder.Diagonal).Sum(l => l.Quantity));
            Assert.Equal(2, bom.Lines.Where(l => l.Category == BomBuilder.BasePlate).Sum(l => l.Quantity));
        }

        [Fact]
        public void Build_TotalPieces_CountsEverything()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            // 2 posts + 2 plates + (2+1+1+1) horizontals + 3 diagonals
            Assert.Equal(12, bom.TotalPieces);
        }

        [Fact]
        public void Build_IncludesReinforcementWhenEnabled()
        {
            var configuration = StandardWithMembers();
            configuration.LeftPost.HasReinforcement = true;
            configuration.LeftPost.ReinforcementCatalogId = CatalogIds.StandardPost; // reinforcements are posts

            var bom = BomBuilder.Build(configuration, Catalog);

            var reinforcement = bom.Lines.Single(l => l.Category == BomBuilder.Reinforcement);
            Assert.Equal(CatalogIds.StandardPost, reinforcement.ProfileId);
            Assert.Equal(PostDescription, reinforcement.Description);
        }

        [Fact]
        public void ToCsv_HasHeaderAndRows()
        {
            var csv = BomCsvExporter.ToCsv(BomBuilder.Build(StandardWithMembers(), Catalog));

            Assert.StartsWith("Categoria,Perfil,Descripcion,Longitud_in,Cantidad", csv);
            Assert.Contains(CatalogIds.StandardPost, csv);
            Assert.Contains(PostDescription, csv);
        }
    }
}
