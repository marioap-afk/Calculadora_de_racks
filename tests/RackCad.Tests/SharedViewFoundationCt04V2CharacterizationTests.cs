using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-57 CT-04 V2 correction evidence. This remains characterization: it audits the existing readers and fixes the
    /// matrix that blocked F2, without introducing a production codec or changing any reader.
    /// </summary>
    public sealed class SharedViewFoundationCt04V2CharacterizationTests
    {
        private static readonly string Root = FindRepositoryRoot();

        [Fact]
        public void MatrixCoversAllKindsReadersAndDispositionClasses()
        {
            var matrix = Matrix();

            Assert.Equal("i57-ct04-v2/v1", matrix.Schema);
            Assert.Equal(
                new[] { "cabecera", "cama", "cantilever", "dynamic", "pushback", "selective" },
                matrix.DecodeRows.Select(row => row.Kind).Distinct().OrderBy(value => value).ToArray());
            Assert.Equal(
                new[] { "Canonical", "Canonicalizable", "Coerced", "Invalid" },
                matrix.KindRows.Select(row => row.Disposition)
                    .Concat(matrix.DecodeRows.Select(row => row.Disposition))
                    .Distinct().OrderBy(value => value).ToArray());
            Assert.Equal(
                new[]
                {
                    "ProjectVariableMutationExecutor", "RackCabeceraCommands", "RackCamaCommands",
                    "RackCantileverCommands", "RackCommandSupport", "RackDinamicoCommands",
                    "RackPushBackCommands", "RackSelectivoCommands",
                },
                matrix.ReaderInventory.Select(reader => reader.Consumer).OrderBy(value => value).ToArray());
        }

        [Fact]
        public void DecodeRowsAreTotalAndCoercionsAreFalsifiable()
        {
            var rows = Matrix().DecodeRows;
            Assert.NotEmpty(rows);
            Assert.Equal(rows.Count, rows.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count());

            Assert.All(rows, row =>
            {
                Assert.False(string.IsNullOrWhiteSpace(row.Id));
                Assert.False(string.IsNullOrWhiteSpace(row.Kind));
                Assert.NotEmpty(row.Consumers);
                Assert.False(string.IsNullOrWhiteSpace(row.Observed));
                Assert.False(string.IsNullOrWhiteSpace(row.Source));
                Assert.False(string.IsNullOrWhiteSpace(row.Symbol));

                if (row.Disposition == "Invalid")
                {
                    Assert.Null(row.SemanticAddress);
                    Assert.Null(row.CanonicalEncoding);
                    Assert.Null(row.CoercionCode);
                }
                else
                {
                    Assert.False(string.IsNullOrWhiteSpace(row.SemanticAddress));
                    Assert.False(string.IsNullOrWhiteSpace(row.CanonicalEncoding));
                }

                if (row.Disposition == "Coerced")
                {
                    Assert.False(string.IsNullOrWhiteSpace(row.CoercionCode));
                }
                else
                {
                    Assert.Null(row.CoercionCode);
                }
            });
        }

        [Fact]
        public void EveryKindCoversNullBlankWhitespaceCaseAndUnknownViewCategories()
        {
            foreach (var group in Matrix().DecodeRows.GroupBy(row => row.Kind))
            {
                Assert.Contains(group, row => row.ViewInput == null);
                Assert.Contains(group, row => row.ViewInput == string.Empty);
                Assert.Contains(group, row => row.ViewInput != null
                    && row.ViewInput.Length > 0
                    && (char.IsWhiteSpace(row.ViewInput[0]) || char.IsWhiteSpace(row.ViewInput[row.ViewInput.Length - 1])));
                Assert.Contains(group, row => row.ViewInput == "future-view");
                Assert.Contains(group, row => row.ViewInput != null
                    && row.ViewInput.Any(char.IsUpper));
            }
        }

        [Fact]
        public void AdversarialCasesRemainExplicit()
        {
            var matrix = Matrix();
            var ids = matrix.KindRows.Select(row => row.Id)
                .Concat(matrix.DecodeRows.Select(row => row.Id))
                .Concat(matrix.ConsumerFallbacks.Select(row => row.Id))
                .Concat(matrix.AvailabilityBoundaries.Select(row => row.Id))
                .ToHashSet(StringComparer.Ordinal);

            AssertRequired(ids,
                "SEL-UNKNOWN-VIEW",
                "SEL-NULL-VIEW",
                "SEL-FRONTAL-MINUS-1",
                "AVL-SEL-FONDO-999",
                "DYN-UNKNOWN-VIEW",
                "DYN-MISSING-VIEW",
                "DYN-FRONTAL-ENTRANCE",
                "DYN-FRONTAL-UNKNOWN-SECTION",
                "POL-DYN-INSERT-LATERAL-PROMPT",
                "CAB-UNKNOWN-VIEW",
                "CAMA-HISTORIC-NO-VIEW",
                "PB-FRONTAL-B-REAR",
                "CANT-STATION-0",
                "AVL-CANT-STATION-999",
                "KIND-CASE",
                "KIND-WHITESPACE",
                "KIND-UNKNOWN");

            Assert.Contains(matrix.ConsumerFallbacks,
                row => row.Id == "POL-DYN-INSERT-UNKNOWN-NEGATIVE"
                    && row.CodecAddress == null
                    && row.WhyNotCodec.Contains("differs from persisted redraw", StringComparison.Ordinal));
            Assert.Contains(matrix.DecodeRows,
                row => row.Id == "PB-FRONTAL-INVALID-SECTION"
                    && row.Disposition == "Invalid"
                    && row.Observed.Contains("DecodeSection fallback is unreachable", StringComparison.Ordinal));
            Assert.Contains(matrix.DecodeRows,
                row => row.Id == "CANT-ADAPTER-SECTION" && row.Disposition == "Invalid");
        }

        [Fact]
        public void AvailabilityOccursAfterSuccessfulSyntaxDecode()
        {
            var boundaries = Matrix().AvailabilityBoundaries;
            Assert.NotEmpty(boundaries);
            Assert.All(boundaries, boundary =>
            {
                Assert.Equal("Canonical", boundary.SyntaxDisposition);
                Assert.False(string.IsNullOrWhiteSpace(boundary.DecodedAddress));
                Assert.False(string.IsNullOrWhiteSpace(boundary.Availability));
            });

            Assert.Contains(boundaries, row => row.Availability.Contains("VariantNotPresent", StringComparison.Ordinal));
            Assert.Contains(boundaries, row => row.Availability == "SystemDoesNotSupportKind");
        }

        [Fact]
        public void EveryCharacterizedEvidenceSymbolStillExists()
        {
            var matrix = Matrix();
            var evidence = matrix.KindRows.Select(row => (row.Source, row.Symbol))
                .Concat(matrix.DecodeRows.Select(row => (row.Source, row.Symbol)))
                .Concat(matrix.ConsumerFallbacks.Select(row => (row.Source, row.Symbol)))
                .Concat(matrix.AvailabilityBoundaries.Select(row => (row.Source, row.Symbol)))
                .Distinct()
                .ToList();

            Assert.NotEmpty(evidence);
            Assert.All(evidence, item =>
            {
                var path = Path.Combine(Root, item.Source.Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(path), item.Source);
                Assert.Contains(item.Symbol, File.ReadAllText(path), StringComparison.Ordinal);
            });
        }

        [Fact]
        public void CorrectionIsAdditionalEvidenceAndDoesNotRewriteF1History()
        {
            var original = File.ReadAllText(Path.Combine(
                Root, "tests", "RackCad.Tests", "Fixtures", "I57", "f1-characterizations.json"));
            Assert.Contains("unknown View token -> Invalid", original, StringComparison.Ordinal);

            var correction = File.ReadAllText(Path.Combine(
                Root, "docs", "initiatives", "I-57-ct04-v2-correction-evidence.md"));
            Assert.Contains("MATERIAL CONTRADICTION", correction, StringComparison.Ordinal);
            Assert.Contains("F2 = BLOCKED", correction, StringComparison.Ordinal);
            Assert.Contains("ct04-v2-coercion-matrix.json", correction, StringComparison.Ordinal);
        }

        private static void AssertRequired(ISet<string> actual, params string[] expected)
            => Assert.All(expected, id => Assert.Contains(id, actual));

        private static Ct04Matrix Matrix()
        {
            var path = Path.Combine(
                Root, "tests", "RackCad.Tests", "Fixtures", "I57", "ct04-v2-coercion-matrix.json");
            return JsonSerializer.Deserialize<Ct04Matrix>(File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName
                ?? throw new InvalidOperationException("RackCad.sln was not found above the test output directory.");
        }

        private sealed class Ct04Matrix
        {
            public string Schema { get; set; }
            public List<KindRow> KindRows { get; set; } = new List<KindRow>();
            public List<DecodeRow> DecodeRows { get; set; } = new List<DecodeRow>();
            public List<ConsumerFallbackRow> ConsumerFallbacks { get; set; } = new List<ConsumerFallbackRow>();
            public List<AvailabilityBoundaryRow> AvailabilityBoundaries { get; set; } = new List<AvailabilityBoundaryRow>();
            public List<ReaderInventoryRow> ReaderInventory { get; set; } = new List<ReaderInventoryRow>();
        }

        private sealed class KindRow
        {
            public string Id { get; set; }
            public string Disposition { get; set; }
            public string Source { get; set; }
            public string Symbol { get; set; }
        }

        private sealed class DecodeRow
        {
            public string Id { get; set; }
            public string Kind { get; set; }
            public string ViewInput { get; set; }
            public List<string> Consumers { get; set; } = new List<string>();
            public string SemanticAddress { get; set; }
            public string Disposition { get; set; }
            public string CoercionCode { get; set; }
            public string Observed { get; set; }
            public string CanonicalEncoding { get; set; }
            public string Source { get; set; }
            public string Symbol { get; set; }
        }

        private sealed class ConsumerFallbackRow
        {
            public string Id { get; set; }
            public string CodecAddress { get; set; }
            public string WhyNotCodec { get; set; }
            public string Source { get; set; }
            public string Symbol { get; set; }
        }

        private sealed class AvailabilityBoundaryRow
        {
            public string Id { get; set; }
            public string DecodedAddress { get; set; }
            public string SyntaxDisposition { get; set; }
            public string Availability { get; set; }
            public string Source { get; set; }
            public string Symbol { get; set; }
        }

        private sealed class ReaderInventoryRow
        {
            public string Consumer { get; set; }
        }
    }
}
