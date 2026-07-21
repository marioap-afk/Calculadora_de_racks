using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RackCad.Application.Catalogs.Validation
{
    /// <summary>One block the library is expected to contain, plus the dynamic parameters it should expose.</summary>
    public sealed class CatalogBlockManifestEntry
    {
        /// <summary>AutoCAD block name (the value of <c>blocks.csv</c>'s <c>blockName</c>).</summary>
        public string BlockName { get; set; }

        /// <summary>Catalog piece ids that draw with this block (sorted, distinct).</summary>
        public List<string> Pieces { get; set; } = new List<string>();

        /// <summary>Views this block is used in (sorted, distinct).</summary>
        public List<string> Views { get; set; } = new List<string>();

        /// <summary>
        /// Dynamic parameter names the block is expected to expose. They come from TWO sources, combined per
        /// exact piece+view by <see cref="CatalogBlockParameters"/>: the connection-layout slide params
        /// (<c>paramX</c>/<c>paramY</c>) AND the grips the production builders actually write to
        /// <see cref="Headers.HeaderBlockInstance.DynamicParameters"/> (LONGITUD of the rail/posts/separators,
        /// PERALTE, ALTURA of the pallet, SAQUE, FRENTE/FONDO). Data-derived, never invented; the names share the
        /// domain constants the builders use, and a builder→manifest guard proves the two cannot diverge.
        /// </summary>
        public List<string> Parameters { get; set; } = new List<string>();
    }

    /// <summary>
    /// The contract between the catalog and <c>blocks-library.dwg</c> (idea-futuras #15): the list of blocks
    /// (and expected parameters) the catalog references, with a stable fingerprint/version. Built from the
    /// catalog with <see cref="BuildExpected"/>, it is compared against the library's ACTUAL manifest so an
    /// incompatible library fails fast instead of producing a partial drawing.
    ///
    /// This type NEVER reads or writes the DWG: the actual manifest is supplied as data (a future plugin step,
    /// out of I-19 scope, would emit it by scanning the library). It only builds the expected side, hashes it,
    /// round-trips it as JSON, and diffs the two.
    /// </summary>
    public sealed class CatalogBlockManifest
    {
        public const int CurrentSchemaVersion = 1;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        /// <summary>SHA-256 of the canonical content; two manifests with the same blocks/params match here.</summary>
        public string Fingerprint { get; set; }

        public List<CatalogBlockManifestEntry> Blocks { get; set; } = new List<CatalogBlockManifestEntry>();

        /// <summary>
        /// The manifest the catalog implies: one entry per distinct <c>blockName</c> in <c>blocks.csv</c>, its
        /// pieces/views, and the dynamic parameters that block is expected to expose. The parameters come from
        /// <see cref="CatalogBlockParameters.ExpectedParameters"/> per EXACT piece+view — both the connection
        /// layout's <c>paramX</c>/<c>paramY</c> AND the grips the production builders actually write — so a
        /// LATERAL block never inherits a FRONTAL-only parameter. Deterministic (everything sorted) so the
        /// fingerprint is stable across loads. Blank block names are skipped (the validator reports those as
        /// <c>MISSING_BLOCK_NAME</c> — they are not a library expectation).
        /// </summary>
        public static CatalogBlockManifest BuildExpected(RackCatalog catalog)
        {
            var manifest = new CatalogBlockManifest();

            if (catalog == null)
            {
                manifest.Fingerprint = manifest.ComputeFingerprint();
                return manifest;
            }

            var blocks = (catalog.Blocks ?? Enumerable.Empty<BlockCatalogEntry>())
                .Where(block => block != null && !string.IsNullOrWhiteSpace(block.BlockName));

            foreach (var group in blocks
                .GroupBy(block => block.BlockName.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                var pieces = group
                    .Select(block => (block.PieceId ?? string.Empty).Trim())
                    .Where(piece => piece.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(piece => piece, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var views = group
                    .Select(block => (block.View ?? string.Empty).Trim())
                    .Where(view => view.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(view => view, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // EXACT by block usage: the params are those Application applies to each specific piece+view
                // that maps to this block name, never the union of every view of a piece (a LATERAL block must
                // not inherit a FRONTAL-only PERALTE). The shared source guarantees names match the producers.
                var parameters = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var block in group)
                {
                    foreach (var parameter in CatalogBlockParameters.ExpectedParameters(catalog, block.PieceId, block.View))
                    {
                        parameters.Add(parameter);
                    }
                }

                manifest.Blocks.Add(new CatalogBlockManifestEntry
                {
                    BlockName = group.Key,
                    Pieces = pieces,
                    Views = views,
                    Parameters = parameters.ToList()
                });
            }

            manifest.Fingerprint = manifest.ComputeFingerprint();
            return manifest;
        }

        /// <summary>SHA-256 (hex) over a canonical rendering of schema version + sorted blocks/pieces/views/params.</summary>
        public string ComputeFingerprint()
        {
            var builder = new StringBuilder();
            builder.Append('v').Append(SchemaVersion).Append('\n');

            foreach (var block in (Blocks ?? new List<CatalogBlockManifestEntry>())
                .Where(block => block != null)
                .OrderBy(block => block.BlockName, StringComparer.OrdinalIgnoreCase))
            {
                builder
                    .Append(block.BlockName).Append('|')
                    .Append(Join(block.Pieces)).Append('|')
                    .Append(Join(block.Views)).Append('|')
                    .Append(Join(block.Parameters))
                    .Append('\n');
            }

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
                var hex = new StringBuilder(hash.Length * 2);
                foreach (var octet in hash)
                {
                    hex.Append(octet.ToString("x2"));
                }

                return hex.ToString();
            }
        }

        public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

        public static CatalogBlockManifest FromJson(string json) =>
            string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<CatalogBlockManifest>(json, JsonOptions);

        /// <summary>
        /// Diff THIS (expected, from the catalog) against the library's <paramref name="actual"/> manifest.
        /// First the integrity of <paramref name="actual"/> is checked: an incompatible schema version aborts
        /// (a manifest we cannot read block-by-block), and a missing/tampered fingerprint is an ERROR (the file
        /// was hand-edited or truncated). Then the block diff: an expected block absent from the library is an
        /// ERROR (partial drawing), an expected parameter absent is a WARNING, and a library block the catalog
        /// never references is INFO (harmless).
        /// </summary>
        public IReadOnlyList<CatalogValidationIssue> Compare(CatalogBlockManifest actual)
        {
            var issues = new List<CatalogValidationIssue>();

            if (actual != null)
            {
                if (!IsSchemaCompatible(actual.SchemaVersion))
                {
                    // Cannot trust a block-by-block diff of a manifest whose schema we do not understand.
                    issues.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.Manifest,
                        "MANIFEST_SCHEMA_INCOMPATIBLE",
                        "La versión de esquema del manifiesto (" + actual.SchemaVersion + ") es incompatible con la esperada ("
                            + CurrentSchemaVersion + "); regenérelo.",
                        "SchemaVersion=" + actual.SchemaVersion));
                    return issues;
                }

                var recomputed = actual.ComputeFingerprint();
                if (string.IsNullOrWhiteSpace(actual.Fingerprint)
                    || !string.Equals(actual.Fingerprint.Trim(), recomputed, StringComparison.OrdinalIgnoreCase))
                {
                    // The stored huella does not match the content: the JSON was truncated or hand-edited.
                    issues.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.Manifest,
                        "MANIFEST_FINGERPRINT_MISMATCH",
                        "La huella del manifiesto no coincide con su contenido (esperada " + recomputed
                            + "); el archivo fue alterado o está incompleto.",
                        string.IsNullOrWhiteSpace(actual.Fingerprint) ? "(sin huella)" : actual.Fingerprint.Trim()));
                }
            }

            var actualBlocks = (actual?.Blocks ?? new List<CatalogBlockManifestEntry>())
                .Where(block => block != null && !string.IsNullOrWhiteSpace(block.BlockName))
                .GroupBy(block => block.BlockName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var expectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var expected in (Blocks ?? new List<CatalogBlockManifestEntry>())
                .Where(block => block != null && !string.IsNullOrWhiteSpace(block.BlockName)))
            {
                var name = expected.BlockName.Trim();
                expectedNames.Add(name);

                if (!actualBlocks.TryGetValue(name, out var actualBlock))
                {
                    issues.Add(new CatalogValidationIssue(
                        CatalogValidationSeverity.Error,
                        CatalogValidationCategory.Manifest,
                        "MANIFEST_MISSING_BLOCK",
                        "El catálogo requiere el bloque pero la biblioteca no lo declara; el dibujo saldría incompleto.",
                        name));
                    continue;
                }

                var actualParameters = new HashSet<string>(
                    actualBlock.Parameters ?? new List<string>(),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var parameter in (expected.Parameters ?? new List<string>())
                    .Where(parameter => !string.IsNullOrWhiteSpace(parameter)))
                {
                    if (!actualParameters.Contains(parameter.Trim()))
                    {
                        issues.Add(new CatalogValidationIssue(
                            CatalogValidationSeverity.Warning,
                            CatalogValidationCategory.Manifest,
                            "MANIFEST_MISSING_PARAMETER",
                            "El bloque de la biblioteca no declara el parámetro esperado '" + parameter.Trim() + "'.",
                            name));
                    }
                }
            }

            foreach (var name in actualBlocks.Keys
                .Where(name => !expectedNames.Contains(name))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
            {
                issues.Add(new CatalogValidationIssue(
                    CatalogValidationSeverity.Info,
                    CatalogValidationCategory.Manifest,
                    "MANIFEST_EXTRA_BLOCK",
                    "La biblioteca declara un bloque que el catálogo no referencia (inofensivo).",
                    name));
            }

            return issues;
        }

        /// <summary>A manifest version this validator can read block-by-block. Only v1 exists today; a future
        /// major bump adds its compatibility rule here (the ONE place that decides "can I diff this?").</summary>
        private static bool IsSchemaCompatible(int schemaVersion) => schemaVersion == CurrentSchemaVersion;

        private static string Join(IEnumerable<string> values) =>
            string.Join(",", (values ?? Enumerable.Empty<string>()).OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
    }
}
