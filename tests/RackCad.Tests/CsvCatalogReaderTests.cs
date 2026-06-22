using System.Linq;
using RackCad.Application.Catalogs;
using Xunit;

namespace RackCad.Tests
{
    public class CsvCatalogReaderTests
    {
        [Fact]
        public void Read_MapsKnownColumnsToTypedFields_AndExtraColumnsToProperties()
        {
            var csv =
                "id,displayName,width,thickness,material,Ix,Iy\n" +
                "POSTE_X,Poste X,3,0.105,Acero A36,2.5,1.8\n";

            var entry = Assert.Single(CsvCatalogReader.Read<ProfileCatalogEntry>(csv));

            Assert.Equal("POSTE_X", entry.Id);
            Assert.Equal("Poste X", entry.DisplayName);
            Assert.Equal(3.0, entry.Width);
            Assert.Equal(0.105, entry.Thickness);
            Assert.Equal("Acero A36", entry.Material);

            // Unknown engineering columns land in the open properties bag.
            Assert.Equal("2.5", entry.Properties["Ix"]);
            Assert.Equal("1.8", entry.Properties["Iy"]);
        }

        [Fact]
        public void Read_HandlesQuotedFieldsWithCommas()
        {
            var csv =
                "id,description,material\n" +
                "P1,\"Poste 3x3, reforzado\",\"Acero, A36\"\n";

            var entry = Assert.Single(CsvCatalogReader.Read<ProfileCatalogEntry>(csv));

            Assert.Equal("Poste 3x3, reforzado", entry.Description);
            Assert.Equal("Acero, A36", entry.Material);
        }

        [Fact]
        public void Read_EmptyCells_LeaveDefaults()
        {
            var csv =
                "id,width,weightPerMeter\n" +
                "P1,,\n";

            var entry = Assert.Single(CsvCatalogReader.Read<ProfileCatalogEntry>(csv));

            Assert.Equal("P1", entry.Id);
            Assert.Equal(0.0, entry.Width);
            Assert.Equal(0.0, entry.WeightPerMeter);
        }

        [Fact]
        public void ShippedPostProfile_LoadsFromCsv_WithStructuralColumnsInProperties()
        {
            var post = JsonRackCatalogProvider.FromBaseDirectory().Load().PostProfiles.FindProfile("POSTE_OMEGA_3X3");

            Assert.NotNull(post);
            Assert.Equal("Poste Omega 3x3 cal.14", post.DisplayName); // came from the CSV
            Assert.True(post.Properties.ContainsKey("Ix"));           // extra column -> properties bag
        }

        [Fact]
        public void ShippedViews_LoadFromCsv()
        {
            var views = JsonRackCatalogProvider.FromBaseDirectory().Load().Views;

            Assert.NotEmpty(views);
            Assert.Equal("Frontal", views.FindView("FRONTAL")?.DisplayName);
        }

        [Fact]
        public void ShippedBlocks_RelatePieceAndView()
        {
            var blocks = JsonRackCatalogProvider.FromBaseDirectory().Load().Blocks;

            // A single piece carries several view-specific blocks (the whole point of a separate table).
            Assert.Equal(4, blocks.BlocksFor("POSTE_OMEGA_3X3").Count());

            var frontal = blocks.FindBlock("POSTE_OMEGA_3X3", "FRONTAL");
            Assert.Equal("POSTE_OMEGA_3X3_FRONT", frontal?.BlockName);
            Assert.Equal("RACK-POSTES", frontal?.Layer);
            Assert.Equal(1.0, frontal?.Scale);

            Assert.Null(blocks.FindBlock("POSTE_OMEGA_3X3", "VISTA_INEXISTENTE"));
        }
    }
}
