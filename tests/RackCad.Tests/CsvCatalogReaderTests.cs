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
        public void ShippedViews_LoadFromCsv()
        {
            var views = JsonRackCatalogProvider.FromBaseDirectory().Load().Views;

            Assert.NotEmpty(views);
            Assert.Equal("Frontal", views.FindView("FRONTAL")?.DisplayName);
        }

        [Fact]
        public void Load_AnsiSavedCsv_DecodesSpanishAccents()
        {
            // Excel's plain "CSV" save is Windows-1252, not UTF-8: á/ñ read as UTF-8 become U+FFFD and the UI
            // shows "escal�n". The provider must fall back to Latin-1 so those files still load correctly.
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "rackcad-csv-ansi-" + System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            try
            {
                var csv = "id,displayName\nLARG_1,Larguero escalón con ménsula\n";
                System.IO.File.WriteAllBytes(
                    System.IO.Path.Combine(directory, "beam-profiles.csv"),
                    System.Text.Encoding.Latin1.GetBytes(csv));

                var catalog = new JsonRackCatalogProvider(directory).Load();

                Assert.Equal("Larguero escalón con ménsula", catalog.BeamProfiles.Single().DisplayName);
            }
            finally
            {
                System.IO.Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public void Load_Utf8CsvWithBom_StillDecodes()
        {
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "rackcad-csv-bom-" + System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            try
            {
                var csv = "id,displayName\nLARG_1,Larguero escalón\n";
                var utf8Bom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                System.IO.File.WriteAllBytes(
                    System.IO.Path.Combine(directory, "beam-profiles.csv"),
                    utf8Bom.GetPreamble().Concat(utf8Bom.GetBytes(csv)).ToArray());

                var catalog = new JsonRackCatalogProvider(directory).Load();

                Assert.Equal("Larguero escalón", catalog.BeamProfiles.Single().DisplayName);
            }
            finally
            {
                System.IO.Directory.Delete(directory, recursive: true);
            }
        }
    }
}
