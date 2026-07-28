using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.StructuralSections;

namespace RackCad.StructuralSections.Import
{
    /// <summary>
    /// The status overlay on disk contradicts what this import produces. Fatal on purpose: an operator's
    /// decision is never withdrawn by the importer.
    /// </summary>
    public sealed class StructuralSectionOverlayException : Exception
    {
        public StructuralSectionOverlayException(string message) : base(message)
        {
        }
    }

    /// <summary>A selected row that could not be imported. Always fatal — there are no silent drops.</summary>
    public sealed class AiscRowRejectedException : Exception
    {
        public AiscRowRejectedException(int rowNumber, string designation, string reason)
            : base("Fila " + rowNumber + (designation == null ? string.Empty : " ('" + designation + "')") +
                   ": " + reason)
        {
            RowNumber = rowNumber;
            Designation = designation;
            Reason = reason;
        }

        public int RowNumber { get; }

        public string Designation { get; }

        public string Reason { get; }
    }

    /// <summary>Everything the import produced, so the caller can write it and report it.</summary>
    public sealed class AiscImportResult
    {
        public IReadOnlyList<StructuralSectionDefinition> Sections { get; init; }

        public StructuralSectionSource Source { get; init; }

        public IReadOnlyDictionary<string, int> ExcludedTypeCounts { get; init; }

        public int SelectedRowCount { get; init; }

        public int TotalDataRows { get; init; }

        public string Worksheet { get; init; }

        public string SourceFileName { get; init; }

        public string SourceSha256 { get; init; }

        /// <summary>Worst relative deviation found between a US value and its metric mirror, per magnitude.</summary>
        public IReadOnlyList<MetricCoherenceResult> MetricCoherence { get; init; }
    }

    public sealed class MetricCoherenceResult
    {
        public string Header { get; init; }

        public int Compared { get; init; }

        public double MaxRelativeDeviation { get; init; }

        public double Tolerance { get; init; }

        public string WorstDesignation { get; init; }

        public bool WithinTolerance => MaxRelativeDeviation <= Tolerance;
    }

    /// <summary>
    /// Reads the AISC Shapes Database workbook and produces the neutral catalog.
    ///
    /// Three properties it must have, and how each is obtained:
    /// - COMPLETE: every row of the four authorized families is imported or rejected with its row number and
    ///   reason. Excluded types are counted separately and are NOT errors.
    /// - REPRODUCIBLE: the output text comes from <see cref="StructuralSectionCsvWriter"/>, which sorts and
    ///   formats deterministically and carries no timestamp, so two runs over the same workbook produce
    ///   byte-identical files.
    /// - HONEST ABOUT UNITS: the US block is the canonical value; the metric mirror is only used as a CONTRAST
    ///   (see <see cref="CheckMetricCoherence"/>), never as a second row and never as a replacement.
    /// </summary>
    public sealed class AiscShapesImporter
    {
        public const string DefaultRevision = AiscWorkbookVerifier.SupportedRevision;

        /// <summary>Geometry and area: AISC rounds to three significant figures, so 1 % is generous but tight.</summary>
        public const double GeometryTolerance = 0.01;

        /// <summary>
        /// Nominal weight: the metric column is an independently rounded DESIGNATION value, not a conversion
        /// (C6X6.7 is published as 10.4 kg/m while the exact conversion is 9.97). 5 % catches a mis-mapped
        /// column, which is what this contrast is for; it cannot and must not police precision.
        /// </summary>
        public const double WeightTolerance = 0.05;

        private static readonly (string Header, double Factor, double Tolerance)[] CoherenceChecks =
        {
            (AiscColumnMap.Weight, 0.45359237d / 0.3048d, WeightTolerance),
            (AiscColumnMap.Area, StructuralSectionUnits.SquareInchesToSquareMillimeters, GeometryTolerance),
            ("d", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("bf", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("tw", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("tf", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("Ht", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("B", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("tnom", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("tdes", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("t", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance),
            ("b", StructuralSectionUnits.InchesToMillimeters, GeometryTolerance)
        };

        /// <summary>
        /// Imports a workbook, VERIFYING first that it really is the AISC Shapes Database v16.0.
        ///
        /// There is no way to point this at another sheet or to label another revision as v16.0: the data
        /// worksheet is derived from what the workbook's own Readme proves, never from a caller's argument.
        /// That is why the CLI has no <c>--worksheet</c> flag.
        /// </summary>
        public AiscImportResult Import(string workbookPath)
        {
            var fileName = Path.GetFileName(workbookPath);
            var sha256 = Sha256OfFile(workbookPath);

            using (var workbook = XlsxWorkbook.Open(workbookPath))
            {
                var identity = AiscWorkbookVerifier.Verify(workbook);
                var worksheetName = identity.DataWorksheet;
                var rows = workbook.ReadRows(worksheetName).ToArray();

                if (rows.Length == 0)
                {
                    throw new XlsxFormatException("La hoja '" + worksheetName + "' esta vacia.");
                }

                var columns = AiscColumnMap.Resolve(rows[0].Value);
                var source = BuildSource(identity.Revision);
                AiscWorkbookVerifier.RequireCoherentMetadata(identity, worksheetName, source.Revision);

                var sections = new List<StructuralSectionDefinition>();
                var excluded = new Dictionary<string, int>(StringComparer.Ordinal);
                var dataRows = 0;
                var selectedRows = new List<(int RowNumber, IReadOnlyDictionary<int, string> Cells)>();

                foreach (var row in rows.Skip(1))
                {
                    if (row.Value.Count == 0 || row.Value.Values.All(string.IsNullOrWhiteSpace))
                    {
                        continue;
                    }

                    dataRows++;

                    var classification = AiscFamilyClassifier.Classify(columns, row.Value, row.Key);

                    if (classification.Disposition == AiscRowDisposition.Excluded)
                    {
                        excluded.TryGetValue(classification.ExclusionToken, out var count);
                        excluded[classification.ExclusionToken] = count + 1;
                        continue;
                    }

                    selectedRows.Add((row.Key, row.Value));
                    sections.Add(AiscRowMapper.Map(
                        columns, row.Value, row.Key, classification.Family, source));
                }

                GuardAgainstIdCollisions(sections);

                return new AiscImportResult
                {
                    Sections = sections,
                    Source = source,
                    ExcludedTypeCounts = excluded,
                    SelectedRowCount = selectedRows.Count,
                    TotalDataRows = dataRows,
                    Worksheet = worksheetName,
                    SourceFileName = fileName,
                    SourceSha256 = sha256,
                    MetricCoherence = CheckMetricCoherence(columns, selectedRows)
                };
            }
        }

        /// <summary>
        /// The identity guard. <see cref="StructuralSectionCatalog.Create"/> would also refuse a duplicate, but
        /// failing HERE names the offending designations, which is what a stop condition needs to be actionable.
        /// </summary>
        private static void GuardAgainstIdCollisions(IReadOnlyList<StructuralSectionDefinition> sections)
        {
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var section in sections)
            {
                var id = section.SectionId.Value;

                if (seen.TryGetValue(id, out var previous))
                {
                    throw new XlsxFormatException(
                        "Colision de ids: '" + previous + "' y '" + section.Identity.EdiDesignation +
                        "' normalizan al mismo id '" + id + "'.");
                }

                seen.Add(id, section.Identity.EdiDesignation);
            }
        }

        /// <summary>
        /// Contrasts every US value against the source's own metric mirror. It does NOT import the metric
        /// block: it uses it as an independent witness that each magnitude was read from the column the header
        /// claims. A deviation beyond tolerance is an unexplained US/metric discrepancy and stops the import.
        /// </summary>
        public static IReadOnlyList<MetricCoherenceResult> CheckMetricCoherence(
            AiscColumnMap columns,
            IReadOnlyList<(int RowNumber, IReadOnlyDictionary<int, string> Cells)> selectedRows)
        {
            var results = new List<MetricCoherenceResult>();

            foreach (var check in CoherenceChecks)
            {
                if (!columns.TryMetricColumn(check.Header, out _))
                {
                    continue;
                }

                var compared = 0;
                var worst = 0d;
                string worstDesignation = null;

                foreach (var row in selectedRows)
                {
                    var us = columns.Number(row.Cells, check.Header, row.RowNumber);
                    var metric = columns.MetricNumber(row.Cells, check.Header, row.RowNumber);

                    if (!us.HasValue || !metric.HasValue || metric.Value == 0)
                    {
                        continue;
                    }

                    compared++;
                    var deviation = Math.Abs((us.Value * check.Factor) - metric.Value) / Math.Abs(metric.Value);

                    if (deviation > worst)
                    {
                        worst = deviation;
                        worstDesignation = columns.Text(row.Cells, AiscColumnMap.ManualLabel);
                    }
                }

                results.Add(new MetricCoherenceResult
                {
                    Header = check.Header,
                    Compared = compared,
                    MaxRelativeDeviation = worst,
                    Tolerance = check.Tolerance,
                    WorstDesignation = worstDesignation
                });
            }

            var breached = results.FirstOrDefault(result => !result.WithinTolerance);

            if (breached != null)
            {
                throw new XlsxFormatException(
                    "Discrepancia no explicable entre el bloque estadounidense y el metrico en '" +
                    breached.Header + "': desviacion relativa maxima " +
                    breached.MaxRelativeDeviation.ToString("P3", CultureInfo.InvariantCulture) +
                    " (tolerancia " + breached.Tolerance.ToString("P3", CultureInfo.InvariantCulture) +
                    ") en '" + breached.WorstDesignation + "'.");
            }

            return results;
        }

        /// <summary>
        /// The logical source. The revision comes from what the workbook PROVED, not from a constant, so the
        /// metadata cannot claim a revision the document does not support.
        /// </summary>
        private static StructuralSectionSource BuildSource(string revision) => new StructuralSectionSource
        {
            SourceId = StructuralSectionSource.AiscShapesId,
            Revision = revision,
            IdNamespace = StructuralSectionSource.AiscIdNamespace,
            Publisher = "American Institute of Steel Construction",
            SourceType = "official technical database",
            NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
            Title = "AISC Shapes Database v" + revision,
            Url = "https://www.aisc.org/aisc/publications/steel-construction-manual/aisc-shapes-database-v160/"
        };

        public static string Sha256OfFile(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                return Convert.ToHexString(sha.ComputeHash(stream));
            }
        }

        public static string Sha256OfText(string text)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToHexString(sha.ComputeHash(new UTF8Encoding(false).GetBytes(text)));
            }
        }
    }
}
