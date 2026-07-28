using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.StructuralSections;
using RackCad.StructuralSections.Import;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// F5 — la ruta pública de carga falla CERRADA.
    ///
    /// Un consumidor futuro no puede recibir un catálogo que sólo se parseó. Una carpeta de catálogos puede
    /// reemplazarse en una instalación desplegada, y una publicación interrumpida deja CSV nuevos junto a un
    /// manifiesto viejo: en ambos casos los archivos son individualmente correctos y colectivamente mentira.
    ///
    /// Cada prueba corrompe UNA cosa y comprueba que <see cref="CsvStructuralSectionCatalogProvider.Load"/>
    /// lanza. El overlay se valida aparte y no participa de ningún hash.
    /// </summary>
    [Collection(StructuralSectionPublishCollection.Name)]
    public class StructuralSectionLoadValidationTests : IDisposable
    {
        private readonly List<string> _directories = new List<string>();
        private readonly List<string> _files = new List<string>();

        [Fact]
        public void AWellFormedCatalogLoads()
        {
            var directory = Published();
            var catalog = new CsvStructuralSectionCatalogProvider(directory).Load();

            Assert.Equal(4, catalog.Count);
        }

        [Fact]
        public void ATamperedDataFileIsRejected_BecauseItsHashNoLongerMatches()
        {
            var directory = Published();
            Append(directory, StructuralSectionCsvSchema.WFile, " ");

            AssertRejected(directory, "SHA-256");
        }

        [Fact]
        public void AManifestThatMisstatesTheCountIsRejected()
        {
            var directory = Published();
            Patch(directory, "\"totalCount\": 4", "\"totalCount\": 99");

            AssertRejected(directory, "en total");
        }

        [Fact]
        public void AManifestOfAnotherCatalogIsRejected()
        {
            var directory = Published();
            Patch(directory, "\"catalogId\": \"structural-sections\"", "\"catalogId\": \"otro\"");

            AssertRejected(directory, "catalogo");
        }

        [Fact]
        public void AManifestWhoseSourceDoesNotMatchTheRowsIsRejected()
        {
            var directory = Published();
            Patch(directory, "\"sourceId\": \"AISC-SHAPES\"", "\"sourceId\": \"OTRA-FUENTE\"");

            AssertRejected(directory, "fuente");
        }

        [Fact]
        public void AManifestWhoseRevisionDoesNotMatchTheSourceIsRejected()
        {
            var directory = Published();
            Patch(directory, "\"sourceRevision\": \"16.0\"", "\"sourceRevision\": \"15.0\"");

            AssertRejected(directory, "revision");
        }

        [Fact]
        public void AManifestWhoseIdNamespaceDoesNotMatchTheSourceIsRejected()
        {
            var directory = Published();
            Patch(directory, "\"idNamespace\": \"AISC\"", "\"idNamespace\": \"OTRA\"");

            AssertRejected(directory, "namespace");
        }

        [Fact]
        public void AManifestWithoutItsWorksheetIsRejected()
        {
            var directory = Published();
            Patch(directory, "\"sourceWorksheet\": \"Database v16.0\"", "\"sourceWorksheet\": \"\"");

            AssertRejected(directory, "sourceWorksheet");
        }

        [Fact]
        public void AManifestWithoutItsMapperVersionIsRejected()
        {
            var directory = Published();
            Patch(directory, "\"mapperVersion\": \"" + StructuralSectionsManifest.SupportedMapperVersion + "\"", "\"mapperVersion\": \"\"");

            AssertRejected(directory, "mapperVersion");
        }

        [Fact]
        public void AWorkbookHashThatIsNotSixtyFourHexIsRejected()
        {
            var directory = Published();
            Patch(directory, "\"sourceSha256\": \"", "\"sourceSha256\": \"XX");

            AssertRejected(directory, "hexadecimales");
        }

        [Fact]
        public void AMissingImmutableFileIsRejected()
        {
            var directory = Published();
            File.Delete(Path.Combine(directory, StructuralSectionCsvSchema.SourcesFile));

            // Falta un archivo obligatorio: el lector estricto lo dice antes incluso de llegar al manifiesto.
            Assert.ThrowsAny<Exception>(() => new CsvStructuralSectionCatalogProvider(directory).Load());
        }

        [Fact]
        public void AMissingManifestIsRejected()
        {
            var directory = Published();
            File.Delete(Path.Combine(directory, StructuralSectionCsvSchema.ManifestFile));

            AssertRejected(directory, "manifiesto");
        }

        [Fact]
        public void AnOverlayPointingAtASectionThatDoesNotExistIsRejected()
        {
            var directory = Published();
            File.WriteAllText(
                Path.Combine(directory, StructuralSectionCsvSchema.StatusFile),
                StructuralSectionCsvWriter.WriteStatus(new[]
                {
                    new StructuralSectionStatusOverride
                    {
                        SectionId = StructuralSectionId.Parse("AISC-W-W99X999"),
                        IsEnabled = false
                    }
                }));

            Assert.ThrowsAny<Exception>(() => new CsvStructuralSectionCatalogProvider(directory).Load());
        }

        [Fact]
        public void ALegitimateOverlayEditDoesNotInvalidateTheImportedData()
        {
            // Ésta es la mitad positiva de F4: deshabilitar una sección es una decisión local legítima y NO
            // puede parecer corrupción de los datos AISC.
            var directory = Published();
            File.WriteAllText(
                Path.Combine(directory, StructuralSectionCsvSchema.StatusFile),
                StructuralSectionCsvWriter.WriteStatus(new[]
                {
                    new StructuralSectionStatusOverride
                    {
                        SectionId = StructuralSectionId.Parse("AISC-W-W12X26"),
                        IsEnabled = false,
                        Notes = "sin existencias"
                    }
                }));

            var catalog = new CsvStructuralSectionCatalogProvider(directory).Load();

            Assert.True(catalog.TryGetById("AISC-W-W12X26", out var disabled));
            Assert.False(disabled.IsEnabled);
            Assert.Equal(3, catalog.Enabled.Count);
            Assert.Equal(4, catalog.Count);
        }

        [Fact]
        public void ThereIsNoPublicWayToObtainAnUnvalidatedCatalog()
        {
            var publicMethods = typeof(CsvStructuralSectionCatalogProvider)
                .GetMethods()
                .Where(method => method.ReturnType == typeof(StructuralSectionCatalog))
                .Select(method => method.Name)
                .ToArray();

            Assert.Equal(new[] { nameof(CsvStructuralSectionCatalogProvider.Load) }, publicMethods);
        }

        // ---- Utilidades ---------------------------------------------------------------------------------

        private void AssertRejected(string directory, string expectedFragment)
        {
            var error = Assert.Throws<StructuralSectionCatalogException>(
                () => new CsvStructuralSectionCatalogProvider(directory).Load());

            Assert.Contains(expectedFragment, error.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static void Patch(string directory, string from, string to)
        {
            var path = Path.Combine(directory, StructuralSectionCsvSchema.ManifestFile);
            var text = File.ReadAllText(path);

            Assert.Contains(from, text, StringComparison.Ordinal);
            File.WriteAllText(path, text.Replace(from, to));
        }

        private static void Append(string directory, string fileName, string suffix)
        {
            var path = Path.Combine(directory, fileName);
            File.WriteAllText(path, File.ReadAllText(path) + suffix);
        }

        private string Published()
        {
            var workbook = SyntheticAiscWorkbook.WriteToTempFile(new[]
            {
                SyntheticAiscWorkbook.W("W12X26", 26),
                SyntheticAiscWorkbook.HssRectangular("HSS4X4X.250", "HSS4X4X1/4", 4, 4, 0.25),
                SyntheticAiscWorkbook.Channel("C10X15.3", 15.3),
                SyntheticAiscWorkbook.Angle("L4X4X1/4", "L4X4X1/4", 4, 4, 0.25)
            });

            _files.Add(workbook);

            var directory = Path.Combine(Path.GetTempPath(), "rackcad-i36a-f5-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            _directories.Add(directory);

            ImportOutputWriter.Publish(
                ImportOutputWriter.Build(new AiscShapesImporter().Import(workbook)), directory);

            return directory;
        }

        public void Dispose()
        {
            foreach (var path in _files.Where(File.Exists))
            {
                try { File.Delete(path); } catch (IOException) { }
            }

            foreach (var directory in _directories.Where(Directory.Exists))
            {
                try { Directory.Delete(directory, recursive: true); } catch (IOException) { }
            }
        }
    }
}
