using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// What the distributed structural catalog claims about itself: which document it came from, how many rows
    /// each family should have, and the SHA-256 of every generated CSV.
    ///
    /// It exists so the claim can be CHECKED instead of trusted — a test recomputes the hashes of the shipped
    /// files and compares. Two consequences follow directly:
    /// - it carries NO import timestamp; a field that changes on every run would make the file differ from
    ///   itself and destroy the reproducibility it is supposed to prove;
    /// - it does NOT hash itself, which would be circular.
    /// </summary>
    public sealed class StructuralSectionsManifest
    {
        public const string CurrentSchemaVersion = "1.0";
        public const string StructuralSectionsCatalogId = "structural-sections";

        public string SchemaVersion { get; init; }

        public string CatalogId { get; init; }

        public string SourceId { get; init; }

        public string SourceRevision { get; init; }

        /// <summary>File name of the workbook the import read. The workbook itself is never versioned.</summary>
        public string SourceFileName { get; init; }

        /// <summary>SHA-256 of that workbook, upper-case hex. The anchor of the whole provenance chain.</summary>
        public string SourceSha256 { get; init; }

        /// <summary>Name of the worksheet the rows were read from, e.g. <c>Database v16.0</c>.</summary>
        public string SourceWorksheet { get; init; }

        /// <summary>Version of the column mapping; bumped whenever the importer changes what it writes.</summary>
        public string MapperVersion { get; init; }

        /// <summary>Section count per family token, e.g. <c>W</c> → 289.</summary>
        public IReadOnlyDictionary<string, int> CountsByFamily { get; init; }

        public int TotalCount { get; init; }

        /// <summary>Rows of a SELECTED family that could not be imported. Must be zero; a non-zero value is a defect.</summary>
        public int RejectedSelectedRows { get; init; }

        /// <summary>Rows skipped on purpose, per source type. Reported, never an error.</summary>
        public IReadOnlyDictionary<string, int> ExcludedTypeCounts { get; init; }

        /// <summary>Every generated file with its SHA-256, ordered by name.</summary>
        public IReadOnlyList<ManifestFile> Files { get; init; }

        public sealed class ManifestFile
        {
            public string Name { get; init; }

            public string Sha256 { get; init; }
        }

        /// <summary>
        /// Serializes deterministically: fixed key order, sorted dictionaries, two-space indent and an explicit
        /// <c>\n</c>. Written by hand rather than with <c>Utf8JsonWriter</c> because that writer's indented mode
        /// emits <see cref="Environment.NewLine"/>, so the very same manifest would differ between the Windows
        /// and Linux CI jobs — which is precisely the byte-identity this file has to guarantee.
        /// </summary>
        public string ToJson()
        {
            var builder = new StringBuilder();
            builder.Append("{\n");
            AppendString(builder, 1, "schemaVersion", SchemaVersion, true);
            AppendString(builder, 1, "catalogId", CatalogId, true);
            AppendString(builder, 1, "sourceId", SourceId, true);
            AppendString(builder, 1, "sourceRevision", SourceRevision, true);
            AppendString(builder, 1, "sourceFileName", SourceFileName, true);
            AppendString(builder, 1, "sourceSha256", SourceSha256, true);
            AppendString(builder, 1, "sourceWorksheet", SourceWorksheet, true);
            AppendString(builder, 1, "mapperVersion", MapperVersion, true);
            AppendCounts(builder, 1, "countsByFamily", CountsByFamily, true);
            AppendNumber(builder, 1, "totalCount", TotalCount, true);
            AppendNumber(builder, 1, "rejectedSelectedRows", RejectedSelectedRows, true);
            AppendCounts(builder, 1, "excludedTypeCounts", ExcludedTypeCounts, true);

            builder.Append("  \"files\": [\n");
            var files = (Files ?? new List<ManifestFile>())
                .OrderBy(file => file.Name, StringComparer.Ordinal)
                .ToArray();

            for (var i = 0; i < files.Length; i++)
            {
                builder.Append("    {\n");
                AppendString(builder, 3, "name", files[i].Name, true);
                AppendString(builder, 3, "sha256", files[i].Sha256, false);
                builder.Append("    }");
                builder.Append(i == files.Length - 1 ? "\n" : ",\n");
            }

            builder.Append("  ]\n");
            builder.Append("}\n");
            return builder.ToString();
        }

        public static StructuralSectionsManifest FromJson(string json)
        {
            using (var document = JsonDocument.Parse(json))
            {
                var root = document.RootElement;

                return new StructuralSectionsManifest
                {
                    SchemaVersion = Text(root, "schemaVersion"),
                    CatalogId = Text(root, "catalogId"),
                    SourceId = Text(root, "sourceId"),
                    SourceRevision = Text(root, "sourceRevision"),
                    SourceFileName = Text(root, "sourceFileName"),
                    SourceSha256 = Text(root, "sourceSha256"),
                    SourceWorksheet = Text(root, "sourceWorksheet"),
                    MapperVersion = Text(root, "mapperVersion"),
                    CountsByFamily = Counts(root, "countsByFamily"),
                    TotalCount = Number(root, "totalCount"),
                    RejectedSelectedRows = Number(root, "rejectedSelectedRows"),
                    ExcludedTypeCounts = Counts(root, "excludedTypeCounts"),
                    Files = root.TryGetProperty("files", out var files)
                        ? files.EnumerateArray()
                            .Select(file => new ManifestFile
                            {
                                Name = Text(file, "name"),
                                Sha256 = Text(file, "sha256")
                            })
                            .ToArray()
                        : new ManifestFile[0]
                };
            }
        }

        private static string Text(JsonElement element, string name) =>
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;

        private static int Number(JsonElement element, string name) =>
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
                ? value.GetInt32()
                : 0;

        private static IReadOnlyDictionary<string, int> Counts(JsonElement element, string name)
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);

            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in value.EnumerateObject())
                {
                    result[property.Name] = property.Value.GetInt32();
                }
            }

            return result;
        }

        private static void AppendString(StringBuilder builder, int depth, string name, string value, bool comma)
        {
            builder.Append(new string(' ', depth * 2));
            builder.Append('"').Append(name).Append("\": ");

            if (value == null)
            {
                builder.Append("null");
            }
            else
            {
                builder.Append('"').Append(Escape(value)).Append('"');
            }

            builder.Append(comma ? ",\n" : "\n");
        }

        private static void AppendNumber(StringBuilder builder, int depth, string name, int value, bool comma)
        {
            builder.Append(new string(' ', depth * 2));
            builder.Append('"').Append(name).Append("\": ");
            builder.Append(value.ToString(CultureInfo.InvariantCulture));
            builder.Append(comma ? ",\n" : "\n");
        }

        private static void AppendCounts(
            StringBuilder builder,
            int depth,
            string name,
            IReadOnlyDictionary<string, int> counts,
            bool comma)
        {
            var indent = new string(' ', depth * 2);
            builder.Append(indent).Append('"').Append(name).Append("\": {");

            var ordered = (counts ?? new Dictionary<string, int>())
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .ToArray();

            if (ordered.Length == 0)
            {
                builder.Append('}').Append(comma ? ",\n" : "\n");
                return;
            }

            builder.Append('\n');

            for (var i = 0; i < ordered.Length; i++)
            {
                builder.Append(indent).Append("  ");
                builder.Append('"').Append(Escape(ordered[i].Key)).Append("\": ");
                builder.Append(ordered[i].Value.ToString(CultureInfo.InvariantCulture));
                builder.Append(i == ordered.Length - 1 ? "\n" : ",\n");
            }

            builder.Append(indent).Append('}').Append(comma ? ",\n" : "\n");
        }

        private static string Escape(string value) =>
            value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
