using System.IO;
using RackCad.Application.Catalogs;
using Xunit;

namespace RackCad.Tests
{
    public class JsonRackCatalogProviderTests
    {
        [Fact]
        public void Load_ShippedCatalogs_ContainsExpectedEntries()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

            Assert.NotEmpty(catalog.PostProfiles);
            Assert.NotEmpty(catalog.HorizontalProfiles);
            Assert.NotEmpty(catalog.DiagonalProfiles);
            Assert.NotEmpty(catalog.BasePlates);
            Assert.NotEmpty(catalog.ConnectionPoints);
            Assert.NotEmpty(catalog.ReinforcementProfiles);

            Assert.NotNull(catalog.PostProfiles.FindProfile("POSTE_OMEGA_3X3"));
            Assert.NotNull(catalog.BasePlates.FindBasePlate("PLACA_BASE_ATORNILLABLE"));
        }

        [Fact]
        public void Load_ShippedDefaults_AreReadFromJson()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

            Assert.NotNull(catalog.Defaults);
            Assert.Equal("POSTE_OMEGA_3X3", catalog.Defaults.Post);
            Assert.Equal("PLACA_BASE_ATORNILLABLE", catalog.Defaults.BasePlate);
            Assert.Equal(132.0, catalog.Defaults.DefaultHeaderHeight);
            Assert.Equal(6.0, catalog.Defaults.HeaderEndAllowance);
        }

        [Fact]
        public void Load_MissingDefaults_FallsBackToBuiltInValues()
        {
            var provider = new JsonRackCatalogProvider(Path.Combine(Path.GetTempPath(), "rackcad-no-defaults-xyz"));

            var defaults = provider.Load().Defaults;

            Assert.NotNull(defaults);
            Assert.Equal("POSTE_OMEGA_3X3", defaults.Post); // CatalogIds fallback
        }

        [Fact]
        public void FindProfile_IsCaseInsensitive()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

            Assert.NotNull(catalog.PostProfiles.FindProfile("poste_omega_3x3"));
        }

        [Fact]
        public void Load_MissingDirectory_ReturnsEmptyCatalogWithoutThrowing()
        {
            var provider = new JsonRackCatalogProvider(Path.Combine(Path.GetTempPath(), "rackcad-does-not-exist-xyz"));

            var catalog = provider.Load();

            Assert.Empty(catalog.PostProfiles);
            Assert.Empty(catalog.HorizontalProfiles);
            Assert.Empty(catalog.ConnectionPoints);
        }

        [Fact]
        public void Load_InvalidJson_ThrowsDescriptiveError()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-invalid-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, JsonRackCatalogProvider.PostProfilesFile), "{ not valid json");

            try
            {
                var provider = new JsonRackCatalogProvider(directory);
                var error = Assert.Throws<System.InvalidOperationException>(() => provider.Load());
                Assert.Contains(JsonRackCatalogProvider.PostProfilesFile, error.Message);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
