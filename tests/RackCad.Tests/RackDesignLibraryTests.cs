using System;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The design library lists .rackcad.json files, inferring type (cabecera vs dinámico) and display name.</summary>
    public class RackDesignLibraryTests
    {
        [Fact]
        public void List_InfersTypeAndName_AndSkipsForeignFiles()
        {
            var dir = Path.Combine(Path.GetTempPath(), "RackCadLibTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var store = new RackProjectStore();

                // A cabecera WITH a name -> the entry uses the name.
                var header = new RackFrameConfigurationFactory(JsonRackCatalogProvider.FromBaseDirectory().Load())
                    .Build(RackFrameTemplateCatalog.Default, CatalogIds.StandardPost, 132.0, 42.0);
                header.Name = "Cabecera A";
                store.Save(RackProject.ForSelective(header), Path.Combine(dir, "cab" + RackProjectStore.FileExtension));

                // A (valid) dynamic system -> inferred as Dinamico.
                var dynamic = new DynamicRackSystemBuilder(JsonRackCatalogProvider.FromBaseDirectory().Load())
                    .BuildDefault(new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"), palletsDeep: 2,
                        headerTemplate: RackFrameTemplateCatalog.Default, headerPostCatalogId: CatalogIds.StandardPost, headerHeight: 132.0);
                store.Save(RackProject.ForDynamic(dynamic), Path.Combine(dir, "sistema" + RackProjectStore.FileExtension));

                // A foreign file -> skipped, not thrown.
                File.WriteAllText(Path.Combine(dir, "ruido.txt"), "no soy un diseño");

                var entries = RackDesignLibrary.List(dir);

                Assert.Equal(2, entries.Count);
                Assert.Contains(entries, e => e.Kind == RackDesignKind.Cabecera && e.Name == "Cabecera A");
                Assert.Contains(entries, e => e.Kind == RackDesignKind.Dinamico);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void List_MissingFolder_ReturnsEmpty()
        {
            var missing = Path.Combine(Path.GetTempPath(), "RackCadLibMissing_" + Guid.NewGuid().ToString("N"));
            Assert.Empty(RackDesignLibrary.List(missing));
        }
    }
}
