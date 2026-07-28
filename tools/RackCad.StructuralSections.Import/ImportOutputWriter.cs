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
    /// <see cref="Publish"/> writes to a STAGING folder first and only moves files into place once every one
    /// of them is written. A crash halfway therefore leaves the distributed catalog exactly as it was, instead
    /// of a half-updated set whose manifest no longer matches its files.
    /// </summary>
    public static class ImportOutputWriter
    {
        public const string StagingFolderName = ".structural-sections-staging";

        /// <summary>UTF-8 without BOM. The generated content is pure ASCII, so the bytes are unambiguous.</summary>
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public static ImportOutput Build(
            AiscImportResult result,
            IReadOnlyList<StructuralSectionStatusOverride> statusOverrides)
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

            files.Add(new KeyValuePair<string, string>(
                StructuralSectionCsvSchema.StatusFile,
                StructuralSectionCsvWriter.WriteStatus(
                    statusOverrides ?? new StructuralSectionStatusOverride[0])));

            var manifest = new StructuralSectionsManifest
            {
                SchemaVersion = StructuralSectionsManifest.CurrentSchemaVersion,
                CatalogId = StructuralSectionsManifest.StructuralSectionsCatalogId,
                SourceId = result.Source.SourceId,
                SourceRevision = result.Source.Revision,
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

            if (Directory.Exists(staging))
            {
                Directory.Delete(staging, recursive: true);
            }

            Directory.CreateDirectory(staging);

            try
            {
                foreach (var file in output.Files)
                {
                    File.WriteAllBytes(Path.Combine(staging, file.Key), Utf8NoBom.GetBytes(file.Value));
                }

                foreach (var file in output.Files)
                {
                    File.Move(
                        Path.Combine(staging, file.Key),
                        Path.Combine(outputDirectory, file.Key),
                        overwrite: true);
                }
            }
            finally
            {
                if (Directory.Exists(staging))
                {
                    Directory.Delete(staging, recursive: true);
                }
            }
        }

        /// <summary>Reads the existing status overlay so a re-import never discards a decision already taken.</summary>
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
