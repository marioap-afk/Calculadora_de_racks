using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Renders the generated catalog files as TEXT, deterministically.
    ///
    /// Determinism is the contract, not a nicety: the manifest declares a SHA-256 per file and a test proves
    /// two runs over the same workbook produce byte-identical output. Everything that could vary is pinned —
    /// header order from <see cref="StructuralSectionCsvSchema"/>, rows sorted by <c>sectionId</c> with
    /// <see cref="StringComparer.Ordinal"/>, numbers in invariant culture, an explicit <c>\n</c> terminator
    /// (never <see cref="Environment.NewLine"/>) and no timestamp anywhere.
    ///
    /// Producing text rather than writing files keeps it pure, so the same function serves the importer and
    /// the tests.
    /// </summary>
    public static class StructuralSectionCsvWriter
    {
        /// <summary>Line terminator. Fixed on purpose: <c>Environment.NewLine</c> would differ per OS.</summary>
        public const string LineTerminator = "\n";

        public static string WriteFamily(
            StructuralSectionFamily family,
            IEnumerable<StructuralSectionDefinition> sections)
        {
            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            var columns = StructuralSectionCsvSchema.ColumnsFor(family);
            var ordered = sections
                .Where(section => section != null && section.Family == family)
                .OrderBy(section => section.SectionId.Value, StringComparer.Ordinal)
                .ToArray();

            var builder = new StringBuilder();
            AppendRow(builder, columns);

            foreach (var section in ordered)
            {
                AppendRow(builder, StructuralSectionCsvSerializer.ToCells(section));
            }

            return builder.ToString();
        }

        public static string WriteSources(IEnumerable<StructuralSectionSource> sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            var builder = new StringBuilder();
            AppendRow(builder, StructuralSectionCsvSchema.SourcesColumns);

            foreach (var source in sources.Where(s => s != null).OrderBy(s => s.SourceId, StringComparer.Ordinal))
            {
                AppendRow(builder, new[]
                {
                    source.SourceId,
                    source.Revision,
                    source.IdNamespace,
                    source.Publisher,
                    source.SourceType,
                    StructuralSectionUnitSystems.ToToken(source.NativeUnitSystem),
                    source.Title,
                    source.Url
                });
            }

            return builder.ToString();
        }

        /// <summary>
        /// The status overlay. It is the ONE hand-editable file of the neutral catalog and it holds ONLY the
        /// exceptions: a section absent from it is enabled. Writing it with the same deterministic rules means
        /// a regenerated overlay and a hand-edited one are comparable.
        /// </summary>
        public static string WriteStatus(IEnumerable<StructuralSectionStatusOverride> overrides)
        {
            if (overrides == null)
            {
                throw new ArgumentNullException(nameof(overrides));
            }

            var builder = new StringBuilder();
            AppendRow(builder, StructuralSectionCsvSchema.StatusColumns);

            foreach (var entry in overrides
                .Where(entry => entry != null)
                .OrderBy(entry => entry.SectionId.Value, StringComparer.Ordinal))
            {
                AppendRow(builder, new[]
                {
                    entry.SectionId.Value,
                    entry.IsEnabled ? "true" : "false",
                    entry.Notes ?? string.Empty
                });
            }

            return builder.ToString();
        }

        private static void AppendRow(StringBuilder builder, IReadOnlyList<string> cells)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }

                builder.Append(Quote(cells[i]));
            }

            builder.Append(LineTerminator);
        }

        /// <summary>
        /// Quotes only when the value forces it (comma, quote, CR or LF). Minimal quoting keeps the generated
        /// files diffable, and the rule is total so the output cannot depend on how the caller feels.
        /// </summary>
        private static string Quote(string value)
        {
            value = value ?? string.Empty;

            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>Round-trip precision in invariant culture, matching the serializer.</summary>
        public static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    }
}
