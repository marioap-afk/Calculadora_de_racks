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
                "id,displayName,width,thickness,blockName,Ix,Iy\n" +
                "POSTE_X,Poste X,3,0.105,BLK_X,2.5,1.8\n";

            var entry = Assert.Single(CsvCatalogReader.Read<ProfileCatalogEntry>(csv));

            Assert.Equal("POSTE_X", entry.Id);
            Assert.Equal("Poste X", entry.DisplayName);
            Assert.Equal(3.0, entry.Width);
            Assert.Equal(0.105, entry.Thickness);
            Assert.Equal("BLK_X", entry.BlockName);

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
    }
}
