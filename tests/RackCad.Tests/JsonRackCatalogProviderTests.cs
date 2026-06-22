using System.IO;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
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
            Assert.NotEmpty(catalog.TrussProfiles);
            Assert.NotEmpty(catalog.BasePlates);
            Assert.NotEmpty(catalog.ConnectionPoints);

            Assert.NotNull(catalog.PostProfiles.FindProfile(CatalogIds.StandardPost));
            Assert.NotNull(catalog.BasePlates.FindBasePlate(CatalogIds.BasePlate));
        }

        [Fact]
        public void Load_ShippedDefaults_AreReadFromJson()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

            Assert.NotNull(catalog.Defaults);
            Assert.Equal(CatalogIds.StandardPost, catalog.Defaults.Post);
            Assert.Equal(CatalogIds.BasePlate, catalog.Defaults.BasePlate);
            Assert.Equal(132.0, catalog.Defaults.DefaultHeaderHeight);
            Assert.Equal(6.0, catalog.Defaults.HeaderEndAllowance);
        }

        [Fact]
        public void Load_MissingDefaults_FallsBackToBuiltInValues()
        {
            var provider = new JsonRackCatalogProvider(Path.Combine(Path.GetTempPath(), "rackcad-no-defaults-xyz"));

            var defaults = provider.Load().Defaults;

            Assert.NotNull(defaults);
            Assert.Equal(CatalogIds.StandardPost, defaults.Post); // CatalogIds fallback
        }

        [Fact]
        public void FindProfile_IsCaseInsensitive()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

            Assert.NotNull(catalog.PostProfiles.FindProfile(CatalogIds.StandardPost.ToLowerInvariant()));
        }

        [Fact]
        public void Load_MissingDirectory_ReturnsEmptyCatalogWithoutThrowing()
        {
            var provider = new JsonRackCatalogProvider(Path.Combine(Path.GetTempPath(), "rackcad-does-not-exist-xyz"));

            var catalog = provider.Load();

            Assert.Empty(catalog.PostProfiles);
            Assert.Empty(catalog.TrussProfiles);
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
