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
                .Build(RackFrameTemplateCatalog.Default, TestCatalogIds.Profiles.Posts.Standard, 132.0, 42.0);
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);
            return configuration;
        }

        [Fact]
        public void ToCsv_UsesCrlfForHeaderAndEveryRow()
        {
            var bom = new BillOfMaterials(new[]
            {
                new BomLine { Category = "Poste", ProfileId = "P1", Description = "Poste", Length = 132.0, Quantity = 2 },
                new BomLine { Category = "Larguero", ProfileId = "L1", Description = "Larguero", Length = 92.0, Quantity = 4 }
            });

            var csv = BomCsvExporter.ToCsv(bom);

            // RFC-4180: CRLF everywhere — no lone \n (header and data rows must agree).
            var lines = csv.Split("\r\n");
            Assert.Equal(4, lines.Length); // header + 2 rows + trailing empty after the final CRLF
            Assert.DoesNotMatch("[^\r]\n", csv);
            Assert.EndsWith("\r\n", csv);
        }

        [Fact]
        public void Build_GroupsPostsToQuantityTwoWithCatalogDescription()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            var post = bom.Lines.Single(l => l.Category == BomBuilder.Post
                && l.ProfileId == TestCatalogIds.Profiles.Posts.Standard);
            Assert.Equal(2, post.Quantity);
            Assert.Equal(132.0, post.Length);
            Assert.Equal(PostDescription, post.Description);
        }

        [Fact]
        public void Build_AggregatesHorizontalsAndDiagonals()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            // Standard now: 5 single travesaños (3 standard + 2 closings) and 2 diagonals.
            Assert.Equal(5, bom.Lines.Where(l => l.Category == BomBuilder.Horizontal).Sum(l => l.Quantity));
            Assert.Equal(2, bom.Lines.Where(l => l.Category == BomBuilder.Diagonal).Sum(l => l.Quantity));
            Assert.Equal(2, bom.Lines.Where(l => l.Category == BomBuilder.BasePlate).Sum(l => l.Quantity));
        }

        [Fact]
        public void Build_TotalPieces_CountsEverything()
        {
            var bom = BomBuilder.Build(StandardWithMembers(), Catalog);

            // 2 posts + 2 plates + 5 single travesaños + 2 diagonals
            Assert.Equal(11, bom.TotalPieces);
        }

        [Fact]
        public void Build_IncludesReinforcementWhenEnabled()
        {
            var configuration = StandardWithMembers();
            configuration.LeftPost.HasReinforcement = true;
            configuration.LeftPost.ReinforcementCatalogId = TestCatalogIds.Profiles.Posts.Standard; // reinforcements are posts

            var bom = BomBuilder.Build(configuration, Catalog);

            var reinforcement = bom.Lines.Single(l => l.Category == BomBuilder.Reinforcement);
            Assert.Equal(TestCatalogIds.Profiles.Posts.Standard, reinforcement.ProfileId);
            Assert.Equal(PostDescription, reinforcement.Description);
        }

        [Fact]
        public void ToCsv_HasHeaderAndRows()
        {
            var csv = BomCsvExporter.ToCsv(BomBuilder.Build(StandardWithMembers(), Catalog));

            Assert.StartsWith("Categoria,Perfil,Descripcion,Longitud_in,Cantidad", csv);
            Assert.Contains(TestCatalogIds.Profiles.Posts.Standard, csv);
            Assert.Contains(PostDescription, csv);
        }
    }
}
