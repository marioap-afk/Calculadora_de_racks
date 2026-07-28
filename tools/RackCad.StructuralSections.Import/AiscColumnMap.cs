using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.StructuralSections.Import
{
    /// <summary>
    /// Resolves AISC's variable names to column indices from the REAL header row of the workbook.
    ///
    /// The sheet publishes every magnitude twice — a US customary block and a metric mirror that repeats the
    /// same header names — so a naive name lookup would be ambiguous. The boundary is found from the data
    /// itself: the metric block starts at the SECOND occurrence of <c>EDI_Std_Nomenclature</c>. Inside the US
    /// range every required name must appear EXACTLY ONCE; anything else means the layout changed and the
    /// import stops instead of reading a neighbouring column.
    /// </summary>
    public sealed class AiscColumnMap
    {
        public const string Type = "Type";
        public const string EdiNomenclature = "EDI_Std_Nomenclature";
        public const string ManualLabel = "AISC_Manual_Label";
        public const string SpecialNoteFlag = "T_F";
        public const string Weight = "W";
        public const string Area = "A";
        public const string OutsideDiameter = "OD";

        /// <summary>The tangent of the principal-axis angle. The alpha is U+03B1 in the published header.</summary>
        public const string TanAlpha = "tan(α)";

        /// <summary>
        /// Every header the importer needs to exist. Listing them explicitly is the layout contract: if AISC
        /// renames or drops one, the import fails loudly on the header row instead of producing 983 rows with
        /// a quietly missing field.
        /// </summary>
        public static readonly string[] RequiredHeaders =
        {
            Type, EdiNomenclature, ManualLabel, SpecialNoteFlag, Weight, Area,
            "d", "ddet", "Ht", "h", OutsideDiameter, "bf", "bfdet", "B", "b",
            "tw", "twdet", "twdet/2", "tf", "tfdet", "t", "tnom", "tdes",
            "kdes", "kdet", "k1", "x", "y", "eo", "xp", "yp",
            "Ix", "Zx", "Sx", "rx", "Iy", "Zy", "Sy", "ry", "Iz", "rz", "Sz",
            "J", "Cw", "C", "Wno", "Sw1", "Sw2", "Sw3", "Qf", "Qw", "ro", "H", TanAlpha, "Iw",
            "zA", "zB", "zC", "wA", "wB", "wC",
            "SwA", "SwB", "SwC", "SzA", "SzB", "SzC",
            "rts", "ho", "PA", "PA2", "PB", "PC", "PD", "T", "WGi", "WGo"
        };

        private readonly Dictionary<string, int> _usColumns;
        private readonly Dictionary<string, int> _metricColumns;

        private AiscColumnMap(Dictionary<string, int> usColumns, Dictionary<string, int> metricColumns)
        {
            _usColumns = usColumns;
            _metricColumns = metricColumns;
        }

        /// <summary>0-based index where the metric mirror begins.</summary>
        public int MetricBlockStart { get; private init; }

        public static AiscColumnMap Resolve(IReadOnlyDictionary<int, string> headerRow)
        {
            if (headerRow == null || headerRow.Count == 0)
            {
                throw new XlsxFormatException("La hoja de datos no tiene fila de encabezados.");
            }

            var maxColumn = headerRow.Keys.Max();
            var headers = new string[maxColumn + 1];

            foreach (var pair in headerRow)
            {
                headers[pair.Key] = (pair.Value ?? string.Empty).Trim();
            }

            var ediOccurrences = Enumerable.Range(0, headers.Length)
                .Where(index => string.Equals(headers[index], EdiNomenclature, StringComparison.Ordinal))
                .ToArray();

            if (ediOccurrences.Length != 2)
            {
                throw new XlsxFormatException(
                    "Se esperaban DOS columnas '" + EdiNomenclature +
                    "' (bloque estadounidense y espejo metrico) y se encontraron " + ediOccurrences.Length + ".");
            }

            var metricStart = ediOccurrences[1];

            var usColumns = Index(headers, 0, metricStart, "estadounidense");
            var metricColumns = Index(headers, metricStart, headers.Length, "metrico");

            foreach (var required in RequiredHeaders)
            {
                if (!usColumns.ContainsKey(required))
                {
                    throw new XlsxFormatException(
                        "Falta la columna obligatoria '" + required + "' en el bloque estadounidense.");
                }
            }

            return new AiscColumnMap(usColumns, metricColumns) { MetricBlockStart = metricStart };
        }

        private static Dictionary<string, int> Index(string[] headers, int from, int toExclusive, string blockName)
        {
            var columns = new Dictionary<string, int>(StringComparer.Ordinal);

            for (var index = from; index < toExclusive; index++)
            {
                var header = headers[index];

                if (string.IsNullOrEmpty(header))
                {
                    continue;
                }

                if (columns.ContainsKey(header))
                {
                    throw new XlsxFormatException(
                        "El encabezado '" + header + "' aparece dos veces dentro del bloque " + blockName +
                        " (columnas " + XlsxWorkbook.ColumnName(columns[header]) + " y " +
                        XlsxWorkbook.ColumnName(index) + ").");
                }

                columns.Add(header, index);
            }

            return columns;
        }

        public int UsColumn(string header)
        {
            if (!_usColumns.TryGetValue(header, out var index))
            {
                throw new XlsxFormatException("No existe la columna estadounidense '" + header + "'.");
            }

            return index;
        }

        public bool TryMetricColumn(string header, out int index) => _metricColumns.TryGetValue(header, out index);

        /// <summary>
        /// The trimmed text of a US-block cell, or null when it is empty or holds the source's
        /// "not applicable" marker. AISC uses an EN DASH (U+2013) for that, and treating it as text would turn
        /// every inapplicable magnitude into an unparseable number.
        /// </summary>
        public string Text(IReadOnlyDictionary<int, string> row, string header)
        {
            var index = UsColumn(header);
            return Clean(row != null && row.TryGetValue(index, out var value) ? value : null);
        }

        public string MetricText(IReadOnlyDictionary<int, string> row, string header)
        {
            if (!TryMetricColumn(header, out var index))
            {
                return null;
            }

            return Clean(row != null && row.TryGetValue(index, out var value) ? value : null);
        }

        /// <summary>The "not applicable" marker AISC prints instead of leaving a cell blank.</summary>
        public const string NotApplicableMarker = "–";

        private static string Clean(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed == NotApplicableMarker ? null : trimmed;
        }

        public double? Number(IReadOnlyDictionary<int, string> row, string header, int rowNumber)
        {
            return Parse(Text(row, header), header, rowNumber);
        }

        public double? MetricNumber(IReadOnlyDictionary<int, string> row, string header, int rowNumber)
        {
            return Parse(MetricText(row, header), header + " (metrico)", rowNumber);
        }

        private static double? Parse(string text, string header, int rowNumber)
        {
            if (text == null)
            {
                return null;
            }

            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw new XlsxFormatException(
                    "Fila " + rowNumber + ", columna '" + header + "': '" + text + "' no es un numero valido.");
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new XlsxFormatException(
                    "Fila " + rowNumber + ", columna '" + header + "': el valor no es finito.");
            }

            return value;
        }
    }
}
