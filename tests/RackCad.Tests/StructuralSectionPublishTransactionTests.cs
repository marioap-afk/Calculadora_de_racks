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
    /// La garantía que estas pruebas fijan, dicha con precisión y ni una palabra más:
    ///
    /// - ante una excepción durante la publicación, el rollback **intenta** todas las restauraciones y
    ///   todas las eliminaciones, sin detenerse en la primera que falle;
    /// - cuando todos esos intentos funcionan, el directorio de salida vuelve **byte por byte** a como
    ///   estaba y no quedan carpetas de trabajo;
    /// - el sistema de archivos puede **impedir** alguna restauración —un archivo bloqueado por otro
    ///   proceso, un atributo de solo lectura, un disco lleno— y entonces el directorio NO vuelve a su
    ///   estado anterior;
    /// - esos fallos secundarios se adjuntan a la excepción **original**, que sigue siendo la que se
    ///   propaga, y se leen con <c>ImportOutputWriter.RollbackFailuresOf</c>;
    /// - un estado incompleto —parcialmente aplicado o parcialmente restaurado— **no puede cargarse**,
    ///   porque el manifiesto se publica el último y la carga validada falla cerrada.
    ///
    /// Lo que NO se promete: atomicidad frente a un crash, un corte eléctrico o una terminación abrupta
    /// del proceso. Eso exigiría escrituras con journal del sistema de archivos, no se puede demostrar
    /// con una prueba y por lo tanto tampoco se afirma.
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

        // ---- Micro-ronda 3: el rollback es honesto cuando el sistema de archivos no coopera --------------

        [Fact]
        public void WhenARestoreIsImpossible_TheOriginalFailureSurvivesAndTheRestIsStillAttempted()
        {
            // Orden de publicacion (ordinal, con el manifiesto al final):
            //   1 sources · 2 c · 3 hss-rect · 4 l · 5 w · 6 manifest
            // Se deja reemplazar sources y c, se BLOQUEA c —que ya fue reemplazado— y se falla en hss-rect.
            // El rollback no podra restaurar c, pero tiene que intentar los demas igualmente.
            var directory = NewDirectory();
            Publish(FirstCatalog(), directory);
            var before = Snapshot(directory);

            var blocked = Path.Combine(directory, StructuralSectionCsvSchema.ChannelFile);
            FileStream lockedHandle = null;

            try
            {
                ImportOutputWriter.AfterReplaceForTests = (_, count) =>
                {
                    if (count == 2)
                    {
                        lockedHandle = File.Open(blocked, FileMode.Open, FileAccess.Read, FileShare.None);
                    }

                    if (count == 3)
                    {
                        throw new InvalidOperationException("fallo inyectado tras 3 reemplazos.");
                    }
                };

                var error = Assert.Throws<InvalidOperationException>(
                    () => ImportOutputWriter.Publish(SecondCatalog(), directory));

                // 1. La excepcion ORIGINAL sobrevive: no la sustituye el fallo del rollback.
                Assert.Contains("fallo inyectado", error.Message, StringComparison.OrdinalIgnoreCase);

                // 2. El fallo secundario se informa de forma inspeccionable.
                var failures = ImportOutputWriter.RollbackFailuresOf(error);
                Assert.NotEmpty(failures);
                Assert.Contains(
                    failures,
                    failure => failure.Contains(StructuralSectionCsvSchema.ChannelFile, StringComparison.Ordinal));
            }
            finally
            {
                ImportOutputWriter.AfterReplaceForTests = null;
                lockedHandle?.Dispose();
            }

            // 3. Las DEMAS restauraciones se intentaron y salieron bien.
            var after = Snapshot(directory);

            Assert.True(
                after[StructuralSectionCsvSchema.HssRectangularFile]
                    .SequenceEqual(before[StructuralSectionCsvSchema.HssRectangularFile]),
                "el archivo posterior al bloqueado no se restauro.");
            Assert.True(
                after[StructuralSectionCsvSchema.SourcesFile]
                    .SequenceEqual(before[StructuralSectionCsvSchema.SourcesFile]),
                "el archivo anterior al bloqueado no se restauro.");

            // El bloqueado, por definicion, NO pudo restaurarse.
            Assert.False(
                after[StructuralSectionCsvSchema.ChannelFile]
                    .SequenceEqual(before[StructuralSectionCsvSchema.ChannelFile]));

            AssertNoWorkingFolders(directory);

            // 4. Y el estado resultante NO se puede cargar como valido: fail-closed.
            Assert.ThrowsAny<Exception>(() => new CsvStructuralSectionCatalogProvider(directory).Load());
        }

        [Fact]
        public void AFailureWhileSeedingTheOverlayDoesNotLeaveItBehind()
        {
            // El overlay se siembra solo cuando falta. Si la publicacion falla despues, la promesa de
            // "el directorio queda exactamente como estaba" obliga a retirarlo: antes no existia.
            var directory = NewDirectory();

            Assert.False(File.Exists(Path.Combine(directory, StructuralSectionCsvSchema.StatusFile)));

            Assert.Throws<InvalidOperationException>(
                () => PublishFailingAfter(FirstCatalog(), directory, failAfter: 2));

            Assert.False(
                File.Exists(Path.Combine(directory, StructuralSectionCsvSchema.StatusFile)),
                "quedo un overlay sembrado por una publicacion que fallo.");
            Assert.Empty(Directory.EnumerateFiles(directory));
            AssertNoWorkingFolders(directory);
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
