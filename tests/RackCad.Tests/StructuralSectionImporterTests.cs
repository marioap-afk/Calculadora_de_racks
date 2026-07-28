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
    /// The importer, driven over a real .xlsx that <see cref="SyntheticAiscWorkbook"/> builds at RUNTIME.
    ///
    /// No binary fixture is versioned and the official AISC workbook is never needed here: what these tests
    /// have to prove is the importer's CONTRACT — which families enter, that nothing selected is dropped in
    /// silence, that a layout or value change is fatal, that two runs are byte-identical and that an id
    /// collision stops everything.
    ///
    /// Serializada con las demas clases que publican: la costura de fallo de ImportOutputWriter es
    /// estatica, y xUnit ejecuta CLASES distintas en paralelo, asi que sin esta coleccion un test
    /// podria armar el fallo que otro sufre.
    /// </summary>
    [Collection(StructuralSectionPublishCollection.Name)]
    public class StructuralSectionImporterTests : IDisposable
    {
        private readonly List<string> _temporaryFiles = new List<string>();
        private readonly List<string> _temporaryDirectories = new List<string>();

        // ---- Classification ------------------------------------------------------------------------------

        [Fact]
        public void OnlyTheFourAuthorizedFamiliesAreImported()
        {
            var result = Import(
                SyntheticAiscWorkbook.W("W12X26", 26),
                SyntheticAiscWorkbook.HssRectangular("HSS6X4X.250", "HSS6X4X1/4", 6, 4, 0.25),
                SyntheticAiscWorkbook.HssRectangular("HSS4X4X.250", "HSS4X4X1/4", 4, 4, 0.25),
                SyntheticAiscWorkbook.HssRound("HSS5.000X0.250", 5),
                SyntheticAiscWorkbook.Channel("C10X15.3", 15.3),
                SyntheticAiscWorkbook.OtherType("MC", "MC12X50"),
                SyntheticAiscWorkbook.Angle("L4X4X1/4", "L4X4X1/4", 4, 4, 0.25),
                SyntheticAiscWorkbook.OtherType("2L", "2L4X4X1/4X3/8"));

            Assert.Equal(5, result.Sections.Count);
            Assert.Equal(1, result.Sections.Count(s => s.Family == StructuralSectionFamily.W));
            Assert.Equal(2, result.Sections.Count(s => s.Family == StructuralSectionFamily.HssRectangular));
            Assert.Equal(1, result.Sections.Count(s => s.Family == StructuralSectionFamily.Channel));
            Assert.Equal(1, result.Sections.Count(s => s.Family == StructuralSectionFamily.Angle));
        }

        [Fact]
        public void ExcludedTypesAreReportedByType_AndAreNotErrors()
        {
            var result = Import(
                SyntheticAiscWorkbook.W("W12X26", 26),
                SyntheticAiscWorkbook.HssRound("HSS5.000X0.250", 5),
                SyntheticAiscWorkbook.OtherType("MC", "MC12X50"),
                SyntheticAiscWorkbook.OtherType("2L", "2L4X4X1/4X3/8"),
                SyntheticAiscWorkbook.OtherType("WT", "WT6X13"),
                SyntheticAiscWorkbook.OtherType("PIPE", "PIPE4STD"));

            Assert.Equal(1, result.ExcludedTypeCounts[AiscFamilyClassifier.RoundHssToken]);
            Assert.Equal(1, result.ExcludedTypeCounts["MC"]);
            Assert.Equal(1, result.ExcludedTypeCounts["2L"]);
            Assert.Equal(1, result.ExcludedTypeCounts["WT"]);
            Assert.Equal(1, result.ExcludedTypeCounts["PIPE"]);
            Assert.Single(result.Sections);
        }

        [Fact]
        public void RoundAndRectangularHss_AreSplitByTheSourcesOwnFields_NotByTheDesignationText()
        {
            // Both designations start with "HSS" and a regular expression over the text could not separate
            // them reliably. The outside diameter and the walls can.
            var result = Import(
                SyntheticAiscWorkbook.HssRectangular("HSS4X4X.250", "HSS4X4X1/4", 4, 4, 0.25),
                SyntheticAiscWorkbook.HssRound("HSS4.000X0.250", 4));

            Assert.Single(result.Sections);
            Assert.Equal("AISC-HSS-RECT-HSS4X4X_250", result.Sections[0].SectionId.Value);
            Assert.Equal(1, result.ExcludedTypeCounts[AiscFamilyClassifier.RoundHssToken]);
        }

        [Fact]
        public void AnHssThatIsNeitherRoundNorRectangular_IsAmbiguousAndStopsTheImport()
        {
            var ambiguous = SyntheticAiscWorkbook.HssRectangular("HSS4X4X.250", "HSS4X4X1/4", 4, 4, 0.25);
            ambiguous.With("OD", 4);

            var error = Assert.Throws<XlsxFormatException>(() => Import(ambiguous));
            Assert.Contains("ambiguo", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void NoSelectedRowIsEverDroppedSilently()
        {
            var rows = new[]
            {
                SyntheticAiscWorkbook.W("W12X26", 26),
                SyntheticAiscWorkbook.W("W12X30", 30),
                SyntheticAiscWorkbook.Channel("C10X15.3", 15.3),
                SyntheticAiscWorkbook.Angle("L4X4X1/4", "L4X4X1/4", 4, 4, 0.25),
                SyntheticAiscWorkbook.HssRectangular("HSS4X4X.250", "HSS4X4X1/4", 4, 4, 0.25)
            };

            var result = Import(rows);

            Assert.Equal(rows.Length, result.SelectedRowCount);
            Assert.Equal(result.SelectedRowCount, result.Sections.Count);
        }

        // ---- Layout and value integrity -------------------------------------------------------------------

        [Fact]
        public void ARenamedHeader_StopsTheImport()
        {
            var headers = SyntheticAiscWorkbook.Headers();
            headers[Array.IndexOf(headers, "kdes")] = "k_des";

            var path = WriteWorkbook(new[] { SyntheticAiscWorkbook.W("W12X26", 26) }, headers);
            var error = Assert.Throws<XlsxFormatException>(() => new AiscShapesImporter().Import(path));

            Assert.Contains("kdes", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void ARemovedMetricMirror_StopsTheImport()
        {
            // Without the mirror the resolver cannot tell where the US block ends, and reading a metric value
            // as if it were an inch would be catastrophic and silent.
            var headers = SyntheticAiscWorkbook.Headers()
                .Take(4 + SyntheticAiscWorkbook.ValueColumns.Length)
                .ToArray();

            var path = WriteWorkbook(new[] { SyntheticAiscWorkbook.W("W12X26", 26) }, headers);
            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(path));

            Assert.Contains("EDI_Std_Nomenclature", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AnUnparseableNumber_StopsTheImport()
        {
            var row = SyntheticAiscWorkbook.W("W12X26", 26);
            row.WithRaw("A", "no-es-un-numero");

            var error = Assert.Throws<XlsxFormatException>(() => Import(row));
            Assert.Contains("no es un numero valido", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AMissingRequiredDimension_RejectsTheRowWithItsNumberAndReason()
        {
            var row = SyntheticAiscWorkbook.W("W12X26", 26);
            row.WithRaw("tw", SyntheticAiscWorkbook.NotApplicable);

            var error = Assert.Throws<AiscRowRejectedException>(() => Import(row));

            Assert.Equal("W12X26", error.Designation);
            Assert.Equal(2, error.RowNumber);
            Assert.Contains("tw", error.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void AnInvalidSpecialNoteFlag_RejectsTheRow()
        {
            var row = SyntheticAiscWorkbook.W("W12X26", 26);
            row.SpecialNoteFlag = "quiza";

            var error = Assert.Throws<AiscRowRejectedException>(() => Import(row));
            Assert.Contains("T_F", error.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void ANonPositiveWeight_RejectsTheRow()
        {
            var row = SyntheticAiscWorkbook.W("W12X26", 26);
            row.With("W", 0);

            var error = Assert.Throws<AiscRowRejectedException>(() => Import(row));
            Assert.Contains("peso", error.Reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AnIdCollision_StopsTheImport()
        {
            // Two different published designations that normalize to the SAME id. The importer must refuse
            // rather than let one overwrite the other.
            var error = Assert.Throws<XlsxFormatException>(() => Import(
                SyntheticAiscWorkbook.Angle("L4X4X1/4", "L4X4X1/4", 4, 4, 0.25),
                SyntheticAiscWorkbook.Angle("L4X4X1.4", "L4X4X1.4", 4, 4, 0.25)));

            Assert.Contains("Colision de ids", error.Message, StringComparison.Ordinal);
            Assert.Contains("AISC-L-L4X4X1_4", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void AMetricMirrorThatContradictsTheUsBlock_StopsTheImport()
        {
            var headers = SyntheticAiscWorkbook.Headers();
            var row = SyntheticAiscWorkbook.W("W12X26", 26);
            var cells = InvokeToCells(row);

            // Corrupt the metric depth by an order of magnitude, keeping everything else consistent.
            var metricDepthIndex = 4 + SyntheticAiscWorkbook.ValueColumns.Length + 2 +
                                   Array.IndexOf(SyntheticAiscWorkbook.ValueColumns, "d");
            cells[metricDepthIndex] = "3100";

            var path = WriteRawWorkbook(headers, new[] { cells });
            var error = Assert.Throws<XlsxFormatException>(
                () => new AiscShapesImporter().Import(path));

            Assert.Contains("Discrepancia", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ---- Reproducibility -------------------------------------------------------------------------------

        [Fact]
        public void TwoRunsOverTheSameWorkbookProduceByteIdenticalFiles()
        {
            var path = WriteWorkbook(new[]
            {
                SyntheticAiscWorkbook.W("W12X26", 26),
                SyntheticAiscWorkbook.HssRectangular("HSS4X4X.250", "HSS4X4X1/4", 4, 4, 0.25),
                SyntheticAiscWorkbook.Channel("C10X15.3", 15.3),
                SyntheticAiscWorkbook.Angle("L4X4X1/4", "L4X4X1/4", 4, 4, 0.25)
            });

            var first = ImportOutputWriter.Build(
                new AiscShapesImporter().Import(path));
            var second = ImportOutputWriter.Build(
                new AiscShapesImporter().Import(path));

            Assert.Equal(first.Files.Count, second.Files.Count);

            foreach (var file in first.Files)
            {
                Assert.Equal(file.Value, second.Content(file.Key));
            }
        }

        [Fact]
        public void TheManifestCarriesNoTimestamp_AndNeverHashesItself()
        {
            var output = BuildOutput(SyntheticAiscWorkbook.W("W12X26", 26));
            var json = output.Content(StructuralSectionCsvSchema.ManifestFile);

            Assert.DoesNotContain("timestamp", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("importedAt", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(DateTime.UtcNow.Year.ToString(), json, StringComparison.Ordinal);
            Assert.DoesNotContain(
                output.Manifest.Files,
                file => string.Equals(file.Name, StructuralSectionCsvSchema.ManifestFile, StringComparison.Ordinal));
        }

        [Fact]
        public void TheManifestDeclaresTheWorkbookHashAndTheCountsItProduced()
        {
            var output = BuildOutput(
                SyntheticAiscWorkbook.W("W12X26", 26),
                SyntheticAiscWorkbook.W("W12X30", 30),
                SyntheticAiscWorkbook.Channel("C10X15.3", 15.3));

            Assert.Equal(StructuralSectionsManifest.CurrentSchemaVersion, output.Manifest.SchemaVersion);
            Assert.Equal(3, output.Manifest.TotalCount);
            Assert.Equal(2, output.Manifest.CountsByFamily["W"]);
            Assert.Equal(1, output.Manifest.CountsByFamily["C"]);
            Assert.Equal(0, output.Manifest.CountsByFamily["HSS-RECT"]);
            Assert.Equal(0, output.Manifest.RejectedSelectedRows);
            Assert.Matches("^[0-9A-F]{64}$", output.Manifest.SourceSha256);
        }

        [Fact]
        public void PublishWritesEveryFile_AndLeavesNoStagingBehind()
        {
            var output = BuildOutput(SyntheticAiscWorkbook.W("W12X26", 26));
            var directory = NewTemporaryDirectory();

            ImportOutputWriter.Publish(output, directory);

            foreach (var file in output.Files)
            {
                Assert.True(File.Exists(Path.Combine(directory, file.Key)), file.Key);
            }

            Assert.False(Directory.Exists(Path.Combine(directory, ImportOutputWriter.StagingFolderName)));
        }

        [Fact]
        public void PublishedFilesAreReadableByTheStrictProvider_AndValidateClean()
        {
            var output = BuildOutput(
                SyntheticAiscWorkbook.W("W12X26", 26),
                SyntheticAiscWorkbook.HssRectangular("HSS4X4X.250", "HSS4X4X1/4", 4, 4, 0.25),
                SyntheticAiscWorkbook.Channel("C10X15.3", 15.3),
                SyntheticAiscWorkbook.Angle("L4X4X1/4", "L4X4X1/4", 4, 4, 0.25));

            var directory = NewTemporaryDirectory();
            ImportOutputWriter.Publish(output, directory);

            var provider = new CsvStructuralSectionCatalogProvider(directory);
            var catalog = provider.LoadUnvalidated();
            var report = new StructuralSectionCatalogValidator()
                .Validate(catalog, provider.ReadManifest(), provider.ComputeSha256);

            Assert.Equal(4, catalog.Count);
            Assert.True(report.IsValid(), report.Format());
        }

        [Fact]
        public void AReimportLeavesTheStatusOverlayUntouchedByteForByte()
        {
            // The overlay is the operator's file, not the importer's output: a re-import must not rewrite it,
            // reorder it or normalise it.
            var directory = NewTemporaryDirectory();
            ImportOutputWriter.Publish(
                BuildOutput(SyntheticAiscWorkbook.W("W12X26", 26), SyntheticAiscWorkbook.W("W12X30", 30)),
                directory);

            var overlayPath = Path.Combine(directory, StructuralSectionCsvSchema.StatusFile);
            File.WriteAllText(
                overlayPath,
                StructuralSectionCsvWriter.WriteStatus(new[]
                {
                    new StructuralSectionStatusOverride
                    {
                        SectionId = StructuralSectionId.Parse("AISC-W-W12X26"),
                        IsEnabled = false,
                        Notes = "retirada"
                    }
                }));

            var before = File.ReadAllBytes(overlayPath);

            ImportOutputWriter.Publish(
                BuildOutput(SyntheticAiscWorkbook.W("W12X26", 26), SyntheticAiscWorkbook.W("W12X30", 30)),
                directory);

            Assert.Equal(before, File.ReadAllBytes(overlayPath));

            var catalog = new CsvStructuralSectionCatalogProvider(directory).LoadUnvalidated();

            Assert.True(catalog.TryGetById("AISC-W-W12X26", out var disabled));
            Assert.False(disabled.IsEnabled);
            Assert.Single(catalog.Enabled);
        }

        [Fact]
        public void AFirstPublishSeedsAnEmptyOverlaySoTheSchemaIsThere()
        {
            var directory = NewTemporaryDirectory();
            ImportOutputWriter.Publish(BuildOutput(SyntheticAiscWorkbook.W("W12X26", 26)), directory);

            var overlay = Path.Combine(directory, StructuralSectionCsvSchema.StatusFile);

            Assert.True(File.Exists(overlay));
            Assert.Empty(ImportOutputWriter.ReadExistingStatus(directory));
            Assert.Equal(
                string.Join(",", StructuralSectionCsvSchema.StatusColumns) + "\n",
                File.ReadAllText(overlay));
        }

        // ---- Helpers ----------------------------------------------------------------------------------------

        private AiscImportResult Import(params SyntheticAiscWorkbook.RowBuilder[] rows) => ImportResult(rows);

        private AiscImportResult ImportResult(params SyntheticAiscWorkbook.RowBuilder[] rows)
        {
            var path = WriteWorkbook(rows);
            return new AiscShapesImporter().Import(path);
        }

        private ImportOutput BuildOutput(params SyntheticAiscWorkbook.RowBuilder[] rows) =>
            ImportOutputWriter.Build(ImportResult(rows));

        private string WriteWorkbook(IEnumerable<SyntheticAiscWorkbook.RowBuilder> rows, string[] headers = null)
        {
            var path = SyntheticAiscWorkbook.WriteToTempFile(rows, headers);
            _temporaryFiles.Add(path);
            return path;
        }

        private string WriteRawWorkbook(string[] headers, IReadOnlyList<string[]> rows)
        {
            var path = Path.Combine(Path.GetTempPath(), "rackcad-i36a-" + Guid.NewGuid().ToString("N") + ".xlsx");
            File.WriteAllBytes(path, RawWorkbookBytes(headers, rows));
            _temporaryFiles.Add(path);
            return path;
        }

        /// <summary>Builds a workbook from literal cells, so a test can plant an inconsistency by hand.</summary>
        private static byte[] RawWorkbookBytes(string[] headers, IReadOnlyList<string[]> rows)
        {
            var builders = rows.Select(cells => new LiteralRow(cells)).ToArray();
            return SyntheticAiscWorkbook.Build(builders, headers);
        }

        /// <summary>A row whose cells are given verbatim.</summary>
        private sealed class LiteralRow : SyntheticAiscWorkbook.RowBuilder
        {
            private readonly string[] _cells;

            public LiteralRow(string[] cells) : base(cells[0], cells[1], cells[2]) => _cells = cells;

            internal override string[] Cells() => _cells;
        }

        private static string[] InvokeToCells(SyntheticAiscWorkbook.RowBuilder row) => row.Cells();

        private string NewTemporaryDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-i36a-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            _temporaryDirectories.Add(directory);
            return directory;
        }

        public void Dispose()
        {
            foreach (var path in _temporaryFiles.Where(File.Exists))
            {
                try
                {
                    File.Delete(path);
                }
                catch (IOException)
                {
                    // A leftover temp file must never fail a test run.
                }
            }

            foreach (var directory in _temporaryDirectories.Where(Directory.Exists))
            {
                try
                {
                    Directory.Delete(directory, recursive: true);
                }
                catch (IOException)
                {
                }
            }
        }
    }
}
