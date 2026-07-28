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
    /// F1 — la publicación de una importación tiene que ser fail-closed.
    ///
    /// La garantía que estas pruebas fijan es exactamente ésta y ni una palabra más: **ante una excepción
    /// durante la publicación, el directorio de salida vuelve byte por byte a como estaba**, no quedan
    /// carpetas de trabajo, y un estado a medias jamás se puede cargar como válido. NO se afirma atomicidad
    /// frente a un corte de energía o un `kill -9`: eso exigiría escrituras con journal del sistema de
    /// archivos y no se puede demostrar con una prueba, así que tampoco se promete.
    ///
    /// Serializada con las demas clases que publican: la costura de fallo de ImportOutputWriter es
    /// estatica, y xUnit ejecuta CLASES distintas en paralelo, asi que sin esta coleccion un test
    /// podria armar el fallo que otro sufre.
    /// </summary>
    [Collection(StructuralSectionPublishCollection.Name)]
    public class StructuralSectionPublishTransactionTests : IDisposable
    {
        private readonly List<string> _directories = new List<string>();
        private readonly List<string> _files = new List<string>();

        // ---- El defecto original, sin costura: un archivo bloqueado a mitad de la publicación -------------

        [Fact]
        public void WhenAReplacementFails_EveryOriginalByteSurvives()
        {
            var directory = NewDirectory();
            Publish(FirstCatalog(), directory);
            var before = Snapshot(directory);

            var second = SecondCatalog();
            var blocked = Path.Combine(directory, StructuralSectionCsvSchema.ChannelFile);

            // Bloquear un archivo de destino hace fallar su reemplazo a mitad de la operación, que es
            // justamente el escenario que la versión anterior dejaba a medias.
            using (File.Open(blocked, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Assert.ThrowsAny<IOException>(() => ImportOutputWriter.Publish(second, directory));
            }

            AssertUnchanged(before, directory);
            AssertNoWorkingFolders(directory);
        }

        // ---- Los tres puntos de fallo que exige el contrato, con la costura interna ------------------------

        [Fact]
        public void FailureAfterTheFirstReplacement_RollsEverythingBack()
        {
            AssertRollback(failAfter: 1);
        }

        [Fact]
        public void FailureHalfway_RollsEverythingBack()
        {
            AssertRollback(failAfter: 3);
        }

        [Fact]
        public void FailureJustBeforeTheManifest_RollsEverythingBack()
        {
            // El manifiesto se publica el ÚLTIMO, así que "justo antes" es tras el penúltimo archivo.
            AssertRollback(failAfter: ImportOutputWriter.PublishedFileCount - 1);
        }

        private void AssertRollback(int failAfter)
        {
            var directory = NewDirectory();
            Publish(FirstCatalog(), directory);
            var before = Snapshot(directory);

            var error = Assert.Throws<InvalidOperationException>(
                () => PublishFailingAfter(SecondCatalog(), directory, failAfter));

            Assert.Contains("fallo inyectado", error.Message, StringComparison.OrdinalIgnoreCase);
            AssertUnchanged(before, directory);
            AssertNoWorkingFolders(directory);
        }

        [Fact]
        public void TheManifestIsAlwaysPublishedLast()
        {
            var published = new List<string>();
            var directory = NewDirectory();

            ImportOutputWriter.AfterReplaceForTests = (fileName, _) => published.Add(fileName);

            try
            {
                Publish(FirstCatalog(), directory);
            }
            finally
            {
                ImportOutputWriter.AfterReplaceForTests = null;
            }

            Assert.Equal(StructuralSectionCsvSchema.ManifestFile, published.Last());
        }

        [Fact]
        public void APartiallyPublishedStateIsRejectedByTheValidatedLoad()
        {
            // Se fabrica a mano el estado que un reemplazo a medias produciría —CSV nuevos con el manifiesto
            // viejo— y se comprueba que la ruta pública de carga se niega a devolverlo.
            var directory = NewDirectory();
            Publish(FirstCatalog(), directory);

            var second = SecondCatalog();
            File.WriteAllBytes(
                Path.Combine(directory, StructuralSectionCsvSchema.WFile),
                System.Text.Encoding.UTF8.GetBytes(second.Content(StructuralSectionCsvSchema.WFile)));

            var provider = new CsvStructuralSectionCatalogProvider(directory);

            Assert.ThrowsAny<Exception>(() => provider.Load());
        }

        [Fact]
        public void ASuccessfulPublishLeavesNoWorkingFolders()
        {
            var directory = NewDirectory();
            Publish(FirstCatalog(), directory);

            AssertNoWorkingFolders(directory);
        }

        // ---- Utilidades -------------------------------------------------------------------------------------

        private void PublishFailingAfter(ImportOutput output, string directory, int failAfter)
        {
            var replaced = 0;
            ImportOutputWriter.AfterReplaceForTests = (_, __) =>
            {
                if (++replaced == failAfter)
                {
                    throw new InvalidOperationException("fallo inyectado tras " + failAfter + " reemplazos.");
                }
            };

            try
            {
                ImportOutputWriter.Publish(output, directory);
            }
            finally
            {
                ImportOutputWriter.AfterReplaceForTests = null;
            }
        }

        private static void AssertUnchanged(IReadOnlyDictionary<string, byte[]> before, string directory)
        {
            var after = Snapshot(directory);

            Assert.Equal(
                before.Keys.OrderBy(name => name, StringComparer.Ordinal),
                after.Keys.OrderBy(name => name, StringComparer.Ordinal));

            foreach (var entry in before)
            {
                Assert.True(after[entry.Key].SequenceEqual(entry.Value), entry.Key + " cambió tras el rollback.");
            }
        }

        private static void AssertNoWorkingFolders(string directory)
        {
            Assert.False(
                Directory.Exists(Path.Combine(directory, ImportOutputWriter.StagingFolderName)),
                "quedó la carpeta de staging.");
            Assert.False(
                Directory.Exists(Path.Combine(directory, ImportOutputWriter.BackupFolderName)),
                "quedó la carpeta de respaldo.");
        }

        private static Dictionary<string, byte[]> Snapshot(string directory) =>
            Directory.EnumerateFiles(directory)
                .ToDictionary(Path.GetFileName, File.ReadAllBytes, StringComparer.Ordinal);

        private void Publish(ImportOutput output, string directory) =>
            ImportOutputWriter.Publish(output, directory);

        private ImportOutput FirstCatalog() => Build(
            SyntheticAiscWorkbook.W("W12X26", 26),
            SyntheticAiscWorkbook.HssRectangular("HSS4X4X.250", "HSS4X4X1/4", 4, 4, 0.25),
            SyntheticAiscWorkbook.Channel("C10X15.3", 15.3),
            SyntheticAiscWorkbook.Angle("L4X4X1/4", "L4X4X1/4", 4, 4, 0.25));

        private ImportOutput SecondCatalog() => Build(
            SyntheticAiscWorkbook.W("W12X30", 30),
            SyntheticAiscWorkbook.HssRectangular("HSS6X4X.250", "HSS6X4X1/4", 6, 4, 0.25),
            SyntheticAiscWorkbook.Channel("C15X50", 50),
            SyntheticAiscWorkbook.Angle("L8X8X1", "L8X8X1", 8, 8, 1));

        private ImportOutput Build(params SyntheticAiscWorkbook.RowBuilder[] rows)
        {
            var path = SyntheticAiscWorkbook.WriteToTempFile(rows);
            _files.Add(path);
            return ImportOutputWriter.Build(new AiscShapesImporter().Import(path));
        }

        private string NewDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-i36a-f1-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            _directories.Add(directory);
            return directory;
        }

        public void Dispose()
        {
            ImportOutputWriter.AfterReplaceForTests = null;

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
