using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The STRICT CSV reader of the neutral catalog (ADR-0020 §3).
    ///
    /// <see cref="CsvCatalogReader"/> is deliberately tolerant — a malformed cell keeps the field's default
    /// and the load continues — which is right for a sheet an engineer edits in Excel and wrong for 983 rows
    /// imported from an official source, where a silent <c>0</c> is a FALSE datum indistinguishable from a
    /// real one. This reader inverts that contract: anything it cannot read exactly becomes a
    /// <see cref="StructuralSectionCsvException"/> naming file, line, column and section.
    ///
    /// It shares only the tokenizer (<see cref="CsvLexer"/>), so the tolerant reader's behaviour is untouched.
    ///
    /// Rules, all of them errors: missing required header; duplicate header; unknown header; empty id; invalid
    /// bool, enum or number; NaN or infinity; missing required value. An EMPTY optional cell is <c>null</c> —
    /// never zero, never <c>false</c>.
    /// </summary>
    public sealed class StrictCsvTable
    {
        private readonly string _fileName;
        private readonly string[] _headers;
        private readonly Dictionary<string, int> _headerIndex;
        private readonly List<Row> _rows;

        private StrictCsvTable(string fileName, string[] headers, Dictionary<string, int> headerIndex, List<Row> rows)
        {
            _fileName = fileName;
            _headers = headers;
            _headerIndex = headerIndex;
            _rows = rows;
        }

        public string FileName => _fileName;

        public IReadOnlyList<string> Headers => _headers;

        public IReadOnlyList<Row> Rows => _rows;

        /// <summary>
        /// Parses <paramref name="text"/> demanding EXACTLY the given headers, in any order.
        ///
        /// "Exactly" is both directions on purpose. A missing column would silently null a field the schema
        /// promises; an unknown column means the file was written by a version this build does not understand,
        /// and guessing which is which is how a schema drifts. A future additive column is therefore an
        /// explicit, versioned schema change, not a surprise.
        /// </summary>
        public static StrictCsvTable Parse(string fileName, string text, IReadOnlyList<string> requiredHeaders)
        {
            if (requiredHeaders == null)
            {
                throw new ArgumentNullException(nameof(requiredHeaders));
            }

            var rawRows = CsvLexer.ParseRows(text);

            if (rawRows.Count == 0)
            {
                throw new StructuralSectionCsvException(fileName, 0, null, null, "el archivo esta vacio.");
            }

            var headers = rawRows[0].Select(header => header.Trim()).ToArray();
            var headerIndex = new Dictionary<string, int>(StringComparer.Ordinal);

            for (var column = 0; column < headers.Length; column++)
            {
                var header = headers[column];

                if (string.IsNullOrEmpty(header))
                {
                    throw new StructuralSectionCsvException(
                        fileName, 1, null, null, "hay un encabezado vacio en la posicion " + (column + 1) + ".");
                }

                if (headerIndex.ContainsKey(header))
                {
                    throw new StructuralSectionCsvException(
                        fileName, 1, header, null, "el encabezado esta duplicado.");
                }

                headerIndex.Add(header, column);
            }

            foreach (var required in requiredHeaders)
            {
                if (!headerIndex.ContainsKey(required))
                {
                    throw new StructuralSectionCsvException(
                        fileName, 1, required, null, "falta el encabezado obligatorio.");
                }
            }

            var allowed = new HashSet<string>(requiredHeaders, StringComparer.Ordinal);

            foreach (var header in headers)
            {
                if (!allowed.Contains(header))
                {
                    throw new StructuralSectionCsvException(
                        fileName, 1, header, null,
                        "encabezado desconocido; el esquema estricto no admite columnas extra sin versionarlas.");
                }
            }

            var rows = new List<Row>();

            for (var index = 1; index < rawRows.Count; index++)
            {
                var cells = rawRows[index];

                if (cells.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                if (cells.Length != headers.Length)
                {
                    throw new StructuralSectionCsvException(
                        fileName, index + 1, null, null,
                        "la fila tiene " + cells.Length + " celdas y el encabezado declara " + headers.Length + ".");
                }

                rows.Add(new Row(fileName, index + 1, headerIndex, cells));
            }

            return new StrictCsvTable(fileName, headers, headerIndex, rows);
        }

        /// <summary>One data row, addressed by header name. Every accessor fails loudly.</summary>
        public sealed class Row
        {
            private readonly string _fileName;
            private readonly Dictionary<string, int> _headerIndex;
            private readonly string[] _cells;

            internal Row(string fileName, int line, Dictionary<string, int> headerIndex, string[] cells)
            {
                _fileName = fileName;
                Line = line;
                _headerIndex = headerIndex;
                _cells = cells;
            }

            /// <summary>1-based line in the file, so an error message points at what the editor shows.</summary>
            public int Line { get; }

            /// <summary>Set by the caller once the row's id is known, so later errors can name it.</summary>
            public string SectionId { get; set; }

            public string Raw(string column)
            {
                if (!_headerIndex.TryGetValue(column, out var index))
                {
                    throw Fail(column, "la columna no existe en este archivo.");
                }

                return _cells[index];
            }

            /// <summary>Trimmed text, or null when the cell is empty. Optional string fields use this.</summary>
            public string OptionalText(string column)
            {
                var value = Raw(column);
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }

            public string RequiredText(string column)
            {
                var value = OptionalText(column);

                if (value == null)
                {
                    throw Fail(column, "el valor es obligatorio y esta vacio.");
                }

                return value;
            }

            /// <summary>A finite double, or null when empty. Never zero-by-failure.</summary>
            public double? OptionalDouble(string column)
            {
                var text = OptionalText(column);

                if (text == null)
                {
                    return null;
                }

                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    throw Fail(column, "'" + text + "' no es un numero valido en cultura invariante.");
                }

                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    throw Fail(column, "'" + text + "' no es un numero finito.");
                }

                return value;
            }

            public double RequiredDouble(string column)
            {
                var value = OptionalDouble(column);

                if (!value.HasValue)
                {
                    throw Fail(column, "el numero es obligatorio y la celda esta vacia.");
                }

                return value.Value;
            }

            /// <summary>Strict boolean: only <c>true</c> or <c>false</c>, ordinal. No 1/0, no SI/NO, no blank.</summary>
            public bool RequiredBool(string column)
            {
                var text = RequiredText(column);

                switch (text)
                {
                    case "true": return true;
                    case "false": return false;
                    default:
                        throw Fail(column, "'" + text + "' no es un booleano valido; se admite 'true' o 'false'.");
                }
            }

            public bool? OptionalBool(string column)
            {
                var text = OptionalText(column);

                if (text == null)
                {
                    return null;
                }

                return RequiredBool(column);
            }

            public StructuralSectionFamily RequiredFamily(string column)
            {
                var text = RequiredText(column);

                if (!StructuralSectionFamilies.TryParseToken(text, out var family))
                {
                    throw Fail(column, "'" + text + "' no es una familia valida.");
                }

                return family;
            }

            public StructuralSectionUnitSystem RequiredUnitSystem(string column)
            {
                var text = RequiredText(column);

                if (!StructuralSectionUnitSystems.TryParseToken(text, out var system))
                {
                    throw Fail(column, "'" + text + "' no es un sistema de unidades valido.");
                }

                return system;
            }

            public StructuralSectionId RequiredSectionId(string column)
            {
                var text = RequiredText(column);

                if (!StructuralSectionId.TryParse(text, out var id))
                {
                    throw Fail(column, "'" + text + "' no es un id de seccion valido.");
                }

                return id;
            }

            public StructuralSectionCsvException Fail(string column, string reason) =>
                new StructuralSectionCsvException(_fileName, Line, column, SectionId, reason);
        }
    }
}
