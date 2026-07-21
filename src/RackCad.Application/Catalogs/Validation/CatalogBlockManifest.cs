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
        /// Dynamic parameter names the block is expected to expose, DERIVED from the connection layout
        /// (the <c>paramX</c>/<c>paramY</c> that drive a piece's grips) — data, never invented.
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
        /// pieces/views, and the parameters its pieces' layout rows name. Deterministic (everything sorted) so
        /// the fingerprint is stable across loads. Blank block names are skipped (the validator reports those
        /// as <c>MISSING_BLOCK_NAME</c> — they are not a library expectation).
        /// </summary>
        public static CatalogBlockManifest BuildExpected(RackCatalog catalog)
        {
            var manifest = new CatalogBlockManifest();

            if (catalog == null)
            {
                manifest.Fingerprint = manifest.ComputeFingerprint();
                return manifest;
            }

            var parametersByPiece = BuildParametersByPiece(catalog.ConnectionLayout);

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

                var parameters = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var piece in pieces)
                {
                    if (parametersByPiece.TryGetValue(piece, out var pieceParameters))
                    {
                        foreach (var parameter in pieceParameters)
                        {
                            parameters.Add(parameter);
                        }
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
        /// Diff THIS (expected, from the catalog) against the library's <paramref name="actual"/> manifest:
        /// an expected block absent from the library is an ERROR (partial drawing), an expected parameter
        /// absent is a WARNING, and a library block the catalog never references is INFO (harmless).
        /// </summary>
        public IReadOnlyList<CatalogValidationIssue> Compare(CatalogBlockManifest actual)
        {
            var issues = new List<CatalogValidationIssue>();

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

        private static Dictionary<string, SortedSet<string>> BuildParametersByPiece(
            IReadOnlyList<ConnectionLayoutEntry> layout)
        {
            var parametersByPiece = new Dictionary<string, SortedSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in layout ?? (IReadOnlyList<ConnectionLayoutEntry>)Array.Empty<ConnectionLayoutEntry>())
            {
                if (row == null || string.IsNullOrWhiteSpace(row.PieceId))
                {
                    continue;
                }

                var piece = row.PieceId.Trim();
                if (!parametersByPiece.TryGetValue(piece, out var parameters))
                {
                    parameters = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                    parametersByPiece[piece] = parameters;
                }

                if (!string.IsNullOrWhiteSpace(row.ParamX))
                {
                    parameters.Add(row.ParamX.Trim());
                }

                if (!string.IsNullOrWhiteSpace(row.ParamY))
                {
                    parameters.Add(row.ParamY.Trim());
                }
            }

            return parametersByPiece;
        }

        private static string Join(IEnumerable<string> values) =>
            string.Join(",", (values ?? Enumerable.Empty<string>()).OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
    }
}
