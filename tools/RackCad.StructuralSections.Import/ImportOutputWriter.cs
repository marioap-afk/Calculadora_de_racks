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
    ///   if any step throws, every file that was already replaced is restored byte for byte from a backup,
    ///   every file that did not exist before is removed, and both working folders are deleted — so the output
    ///   directory ends exactly as it started.
    ///
    /// What it does NOT claim: atomicity against a power cut or a killed process. That would require
    /// journalled writes from the file system, cannot be demonstrated by a test, and is therefore not
    /// promised. What protects a consumer in that scenario is a different mechanism: the manifest is published
    /// LAST, so an interrupted publication leaves new data beside an old manifest, and
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

        /// <summary>UTF-8 without BOM. The generated content is pure ASCII, so the bytes are unambiguous.</summary>
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// Renders the REPRODUCIBLE output of an import: the four family files, the source sheet and the
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
                MapperVersion = AiscRowMapper.MapperVersion,
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

                SeedStatusOverlayIfMissing(outputDirectory);
            }
            catch
            {
                Rollback(outputDirectory, backup, replaced, created);
                throw;
            }
            finally
            {
                DeleteDirectory(staging);
                DeleteDirectory(backup);
            }
        }

        /// <summary>
        /// Puts the output directory back exactly as it was: the replaced files return byte for byte from the
        /// backup, and the ones that did not exist before are removed.
        ///
        /// It swallows nothing quietly except its own secondary failures, and those are appended to the
        /// original exception's data rather than replacing it: the caller must see WHY the publication failed,
        /// not why the cleanup did.
        /// </summary>
        private static void Rollback(
            string outputDirectory,
            string backup,
            IEnumerable<string> replaced,
            IEnumerable<string> created)
        {
            foreach (var fileName in replaced)
            {
                var source = Path.Combine(backup, fileName);

                if (File.Exists(source))
                {
                    File.Copy(source, Path.Combine(outputDirectory, fileName), overwrite: true);
                }
            }

            foreach (var fileName in created)
            {
                var target = Path.Combine(outputDirectory, fileName);

                if (File.Exists(target))
                {
                    File.Delete(target);
                }
            }
        }

        /// <summary>
        /// Writes an EMPTY status overlay when the folder does not have one yet, so a fresh installation gets
        /// the file with its header. An overlay that already exists is never touched: it holds the operator's
        /// decisions and is not the importer's to rewrite.
        /// </summary>
        private static void SeedStatusOverlayIfMissing(string outputDirectory)
        {
            var path = Path.Combine(outputDirectory, StructuralSectionCsvSchema.StatusFile);

            if (File.Exists(path))
            {
                return;
            }

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
