using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RackCad.Application.StructuralSections;

namespace RackCad.StructuralSections.Import
{
    /// <summary>The exact text of every file an import produces, before anything touches the output directory.</summary>
    public sealed class ImportOutput
    {
        /// <summary>File name to content, in a stable order.</summary>
        public IReadOnlyList<KeyValuePair<string, string>> Files { get; init; }

        public StructuralSectionsManifest Manifest { get; init; }

        public string Content(string fileName) =>
            Files.First(file => string.Equals(file.Key, fileName, StringComparison.Ordinal)).Value;
    }

    /// <summary>
    /// Renders and then publishes an import.
    ///
    /// <see cref="Build"/> is pure: the same result always yields the same text, which is what makes
    /// byte-identity between two runs testable without touching a disk.
    ///
    /// <see cref="Publish"/> is FAIL-CLOSED, and the exact guarantee is worth stating precisely because an
    /// imprecise one would be a lie:
    ///
    ///   if any step throws, the rollback ATTEMPTS every restoration — each replaced file back byte for byte
    ///   from a backup, each file that did not exist before removed — and then deletes both working folders.
    ///   When every attempt succeeds, the output directory ends exactly as it started.
    ///
    /// What it does NOT claim, and why:
    ///
    /// - **Restoration is attempted, not guaranteed.** The file system can refuse: a file locked by another
    ///   process, a read-only attribute, a disk that fills up. The rollback does not stop at the first
    ///   refusal — it tries the rest — and it never replaces the exception that caused it. The failures it
    ///   could not resolve are attached to that original exception under
    ///   <see cref="RollbackFailuresKey"/>, readable with <see cref="RollbackFailuresOf"/>. In that case the
    ///   directory is NOT back to its previous state, and saying otherwise would be false.
    ///
    /// - **Atomicity against a power cut or a killed process.** That would require journalled writes from the
    ///   file system and cannot be demonstrated by a test.
    ///
    /// In both of those cases the consumer is protected by a different mechanism, not by a promise: the
    /// manifest is published LAST, so any partially applied or partially restored set no longer matches its
    /// declared hashes and
    /// <see cref="RackCad.Application.StructuralSections.CsvStructuralSectionCatalogProvider.Load"/> refuses
    /// to load it.
    /// </summary>
    public static class ImportOutputWriter
    {
        public const string StagingFolderName = ".structural-sections-staging";

        /// <summary>Where the previous bytes live while the replacement runs, so a rollback can restore them.</summary>
        public const string BackupFolderName = ".structural-sections-backup";

        /// <summary>How many files a publication replaces: the immutable set plus the manifest.</summary>
        public static int PublishedFileCount =>
            StructuralSectionCsvSchema.ImmutableFiles().Length + 1;

        /// <summary>
        /// Test seam. Invoked after each successful replacement with the file name and the 1-based count, so a
        /// test can throw at a chosen point and prove the rollback. Null in every real run.
        ///
        /// It is deliberately a hook and not an injected file-system abstraction: the only thing a test needs
        /// is to fail at a known moment, and a public interface for that would be a whole seam nobody else
        /// would ever implement.
        /// </summary>
        internal static Action<string, int> AfterReplaceForTests { get; set; }

        /// <summary>
        /// Key under which a failed publication records the rollback's OWN failures in
        /// <see cref="Exception.Data"/>.
        /// </summary>
        public const string RollbackFailuresKey = "RackCad.StructuralSections.RollbackFailures";

        /// <summary>
        /// The files the rollback could not restore or remove, if any. Empty when the rollback was complete
        /// or when the exception did not come from a publication.
        /// </summary>
        public static IReadOnlyList<string> RollbackFailuresOf(Exception exception)
        {
            return exception?.Data[RollbackFailuresKey] as string[] ?? new string[0];
        }

        /// <summary>UTF-8 without BOM. The generated content is pure ASCII, so the bytes are unambiguous.</summary>
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// Renders the REPRODUCIBLE output of an import: one file per family, the source sheet and the
        /// manifest. The status overlay is NOT here — it is a local decision, not a function of the workbook,
        /// so the importer neither produces it nor hashes it (see <see cref="SeedStatusOverlayIfMissing"/>).
        /// </summary>
        public static ImportOutput Build(AiscImportResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var files = new List<KeyValuePair<string, string>>();

            foreach (var family in StructuralSectionFamilies.All)
            {
                files.Add(new KeyValuePair<string, string>(
                    StructuralSectionCsvSchema.FileFor(family),
                    StructuralSectionCsvWriter.WriteFamily(family, result.Sections)));
            }

            files.Add(new KeyValuePair<string, string>(
                StructuralSectionCsvSchema.SourcesFile,
                StructuralSectionCsvWriter.WriteSources(new[] { result.Source })));

            var manifest = new StructuralSectionsManifest
            {
                SchemaVersion = StructuralSectionsManifest.CurrentSchemaVersion,
                CatalogId = StructuralSectionsManifest.StructuralSectionsCatalogId,
                SourceId = result.Source.SourceId,
                SourceRevision = result.Source.Revision,
                IdNamespace = result.Source.IdNamespace,
                SourceFileName = result.SourceFileName,
                SourceSha256 = result.SourceSha256,
                SourceWorksheet = result.Worksheet,
                MapperVersion = StructuralSectionsManifest.SupportedMapperVersion,
                CountsByFamily = StructuralSectionFamilies.All.ToDictionary(
                    StructuralSectionFamilies.ToToken,
                    family => result.Sections.Count(section => section.Family == family),
                    StringComparer.Ordinal),
                TotalCount = result.Sections.Count,

                // Zero by construction: a selected row that cannot be mapped throws
                // AiscRowRejectedException and aborts the whole import, so an output can only ever exist
                // when nothing was rejected. The field stays in the manifest because a reader must be able to
                // CHECK that, not take it on faith.
                RejectedSelectedRows = 0,
                ExcludedTypeCounts = result.ExcludedTypeCounts,
                Files = files
                    .Select(file => new StructuralSectionsManifest.ManifestFile
                    {
                        Name = file.Key,
                        Sha256 = AiscShapesImporter.Sha256OfText(file.Value)
                    })
                    .OrderBy(file => file.Name, StringComparer.Ordinal)
                    .ToArray()
            };

            files.Add(new KeyValuePair<string, string>(
                StructuralSectionCsvSchema.ManifestFile, manifest.ToJson()));

            return new ImportOutput { Files = files, Manifest = manifest };
        }

        public static void Publish(ImportOutput output, string outputDirectory)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            Directory.CreateDirectory(outputDirectory);
            var staging = Path.Combine(outputDirectory, StagingFolderName);
            var backup = Path.Combine(outputDirectory, BackupFolderName);

            DeleteDirectory(staging);
            DeleteDirectory(backup);
            Directory.CreateDirectory(staging);
            Directory.CreateDirectory(backup);

            // The manifest goes LAST. While the replacement runs, the data files are already new and the
            // manifest is still the old one, so its hashes do not match and a validated load refuses the
            // folder — which is exactly what should happen to a half-published state.
            var ordered = output.Files
                .OrderBy(file => string.Equals(
                    file.Key, StructuralSectionCsvSchema.ManifestFile, StringComparison.Ordinal) ? 1 : 0)
                .ThenBy(file => file.Key, StringComparer.Ordinal)
                .ToArray();

            var replaced = new List<string>();
            var created = new List<string>();

            try
            {
                foreach (var file in ordered)
                {
                    File.WriteAllBytes(Path.Combine(staging, file.Key), Utf8NoBom.GetBytes(file.Value));
                }

                // The overlay is seeded BEFORE the replacements and recorded as created, so a later failure
                // can remove it. Seeding it at the end would leave a file the rollback never knew about, and
                // "the directory ends exactly as it started" would be false.
                SeedStatusOverlayIfMissing(outputDirectory, created);

                var count = 0;

                foreach (var file in ordered)
                {
                    var target = Path.Combine(outputDirectory, file.Key);

                    if (File.Exists(target))
                    {
                        // Copy BEFORE replacing. If this throws, the file has not been touched yet and the
                        // rollback list stays truthful.
                        File.Copy(target, Path.Combine(backup, file.Key), overwrite: true);
                        replaced.Add(file.Key);
                    }
                    else
                    {
                        created.Add(file.Key);
                    }

                    File.Move(Path.Combine(staging, file.Key), target, overwrite: true);
                    AfterReplaceForTests?.Invoke(file.Key, ++count);
                }
            }
            catch (Exception publishFailure)
            {
                // The rollback must never replace the failure that caused it. It collects its OWN failures
                // and attaches them to the original exception, which is what propagates.
                var failures = Rollback(outputDirectory, backup, replaced, created);
                failures.AddRange(RemoveWorkingFolders(staging, backup));

                if (failures.Count > 0)
                {
                    publishFailure.Data[RollbackFailuresKey] = failures.ToArray();
                }

                throw;
            }

            var cleanupFailures = RemoveWorkingFolders(staging, backup);

            if (cleanupFailures.Count > 0)
            {
                // The data landed, but leaving the working folders behind would make the next publication
                // start from a dirty state. Fail loudly rather than pretend.
                throw new IOException(
                    "La publicacion escribio los archivos pero no pudo retirar sus carpetas de trabajo: " +
                    string.Join(" | ", cleanupFailures));
            }
        }

        /// <summary>
        /// Tries to put the output directory back as it was: every replaced file returns byte for byte from
        /// the backup, and every file that did not exist before is removed.
        ///
        /// It ATTEMPTS all of them and never throws. A file the operating system will not let it write —
        /// locked by another process, read-only, gone — must not stop the remaining restorations, and above
        /// all must not replace the exception that caused the rollback: the caller needs to see WHY the
        /// publication failed, not why the cleanup did. Each secondary failure is returned so the caller can
        /// attach it to the original exception.
        ///
        /// Consequence to be honest about: if a restoration fails, the directory is NOT back to its previous
        /// state. That is why the failures are reported and why the validated load will refuse the folder —
        /// the manifest is published last, so a partially restored set no longer matches its hashes.
        /// </summary>
        private static List<string> Rollback(
            string outputDirectory,
            string backup,
            IEnumerable<string> replaced,
            IEnumerable<string> created)
        {
            var failures = new List<string>();

            foreach (var fileName in replaced)
            {
                try
                {
                    var source = Path.Combine(backup, fileName);

                    if (File.Exists(source))
                    {
                        File.Copy(source, Path.Combine(outputDirectory, fileName), overwrite: true);
                    }
                    else
                    {
                        failures.Add(fileName + ": no se conservo respaldo, no se pudo restaurar.");
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(
                        fileName + ": no se pudo restaurar (" + ex.GetType().Name + ": " + ex.Message + ").");
                }
            }

            foreach (var fileName in created)
            {
                try
                {
                    var target = Path.Combine(outputDirectory, fileName);

                    if (File.Exists(target))
                    {
                        File.Delete(target);
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(
                        fileName + ": no se pudo eliminar (" + ex.GetType().Name + ": " + ex.Message + ").");
                }
            }

            return failures;
        }

        /// <summary>Removes staging and backup, reporting rather than throwing if one of them survives.</summary>
        private static List<string> RemoveWorkingFolders(params string[] directories)
        {
            var failures = new List<string>();

            foreach (var directory in directories)
            {
                try
                {
                    DeleteDirectory(directory);
                }
                catch (Exception ex)
                {
                    failures.Add(
                        Path.GetFileName(directory) + ": no se pudo retirar (" + ex.GetType().Name + ": " +
                        ex.Message + ").");
                }
            }

            return failures;
        }

        /// <summary>
        /// Writes an EMPTY status overlay when the folder does not have one yet, so a fresh installation gets
        /// the file with its header. An overlay that already exists is never touched: it holds the operator's
        /// decisions and is not the importer's to rewrite.
        ///
        /// It records the file as CREATED before writing it, so a rollback removes it even if the write itself
        /// failed halfway. Otherwise a failed publication could leave behind a file that did not exist before.
        /// </summary>
        private static void SeedStatusOverlayIfMissing(string outputDirectory, ICollection<string> created)
        {
            var path = Path.Combine(outputDirectory, StructuralSectionCsvSchema.StatusFile);

            if (File.Exists(path))
            {
                return;
            }

            created.Add(StructuralSectionCsvSchema.StatusFile);

            File.WriteAllBytes(
                path,
                Utf8NoBom.GetBytes(
                    StructuralSectionCsvWriter.WriteStatus(new StructuralSectionStatusOverride[0])));
        }

        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        /// <summary>
        /// Reads the existing status overlay. A re-import never rewrites it and never discards an entry: if an
        /// id no longer exists, the import STOPS (see <c>Program</c>), because withdrawing a decision the
        /// operator took is the operator's call, not the importer's.
        /// </summary>
        public static IReadOnlyList<StructuralSectionStatusOverride> ReadExistingStatus(string outputDirectory)
        {
            var path = Path.Combine(outputDirectory, StructuralSectionCsvSchema.StatusFile);

            if (!File.Exists(path))
            {
                return new StructuralSectionStatusOverride[0];
            }

            var table = StrictCsvTable.Parse(
                StructuralSectionCsvSchema.StatusFile,
                File.ReadAllText(path),
                StructuralSectionCsvSchema.StatusColumns);

            return table.Rows
                .Select(row => new StructuralSectionStatusOverride
                {
                    SectionId = row.RequiredSectionId(StructuralSectionCsvSchema.SectionId),
                    IsEnabled = row.RequiredBool(StructuralSectionCsvSchema.IsEnabled),
                    Notes = row.OptionalText(StructuralSectionCsvSchema.Notes)
                })
                .ToArray();
        }
    }
}
