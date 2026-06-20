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
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static RackFrameConfiguration StandardWithMembers()
        {
            var configuration = new RackFrameConfigurationFactory(Catalog)
                .Build(RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0, 42.0);
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);
            return configuration;
        }

        [Fact]
        public void Build_GroupsPostsToQuantityTwoWithCatalogDescription()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            var post = bom.Lines.Single(l => l.Category == BomBuilder.Post && l.ProfileId == "POSTE_OMEGA_3X3");
            Assert.Equal(2, post.Quantity);
            Assert.Equal(132.0, post.Length);
            Assert.Equal("Poste omega 3x3", post.Description);
        }

        [Fact]
        public void Build_AggregatesHorizontalsAndDiagonals()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            // H1 has quantity 2 -> the lower horizontal line is quantity 2.
            Assert.Equal(2, bom.Lines.Single(l => l.ProfileId == "HORIZONTAL_INFERIOR").Quantity);
            // H2 + H3 share the intermediate profile -> quantity 2.
            Assert.Equal(2, bom.Lines.Single(l => l.ProfileId == "HORIZONTAL_INTERMEDIA").Quantity);
            // Three SingleDiagonal panels of equal height collapse to one line of quantity 3.
            Assert.Equal(3, bom.Lines.Single(l => l.Category == BomBuilder.Diagonal).Quantity);
            // Two base plates.
            Assert.Equal(2, bom.Lines.Single(l => l.Category == BomBuilder.BasePlate).Quantity);
        }

        [Fact]
        public void Build_TotalPieces_CountsEverything()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            // 2 posts + 2 plates + (2+2+1) horizontals + 3 diagonals
            Assert.Equal(12, bom.TotalPieces);
        }

        [Fact]
        public void Build_IncludesReinforcementWhenEnabled()
        {
            var configuration = StandardWithMembers();
            configuration.LeftPost.HasReinforcement = true;
            configuration.LeftPost.ReinforcementCatalogId = "REFUERZO_OMEGA_3X3";

            var bom = BomBuilder.Build(configuration, Catalog);

            var reinforcement = bom.Lines.Single(l => l.Category == BomBuilder.Reinforcement);
            Assert.Equal("REFUERZO_OMEGA_3X3", reinforcement.ProfileId);
            Assert.Equal("Refuerzo omega 3x3", reinforcement.Description);
        }

        [Fact]
        public void ToCsv_HasHeaderAndRows()
        {
            var csv = BomCsvExporter.ToCsv(BomBuilder.Build(StandardWithMembers(), Catalog));

            Assert.StartsWith("Categoria,Perfil,Descripcion,Longitud_in,Cantidad", csv);
            Assert.Contains("POSTE_OMEGA_3X3", csv);
            Assert.Contains("Poste omega 3x3", csv);
        }
    }
}
