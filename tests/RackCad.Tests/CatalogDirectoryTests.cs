using System.IO;
using RackCad.Application.Catalogs;
using Xunit;

namespace RackCad.Tests
{
    public class CatalogDirectoryTests
    {
        [Fact]
        public void Resolve_ReturnsACatalogsFolder()
        {
            var resolved = CatalogDirectory.Resolve();

            Assert.EndsWith(CatalogDirectory.FolderName, resolved);
        }

        [Fact]
        public void Resolve_FindsTheCatalogsCopiedNextToTheAssembly()
        {
            // The test project copies the catalogs next to its output, so resolution must locate them even
            // though it is run from a path where AppContext.BaseDirectory would also work. This guards the
            // AutoCAD case where AppContext.BaseDirectory points at acad.exe instead of the plugin.
            var resolved = CatalogDirectory.Resolve();

            Assert.True(Directory.Exists(resolved), "Resolved catalogs directory should exist: " + resolved);
            Assert.True(File.Exists(Path.Combine(resolved, "blocks.csv")), "blocks.csv should be present in the resolved catalogs folder.");
        }
    }
}
