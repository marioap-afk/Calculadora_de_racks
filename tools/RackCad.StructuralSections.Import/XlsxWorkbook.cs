using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace RackCad.StructuralSections.Import
{
    /// <summary>
    /// Reads an .xlsx with the BCL only: a ZIP container plus SpreadsheetML parts (ADR-0012 — no NuGet, no
    /// Office Interop, no Excel installed).
    ///
    /// It resolves everything from the workbook ITSELF instead of assuming positions: the sheet is found by
    /// name through <c>xl/workbook.xml</c> and its relationship, and every cell carries its own <c>r</c>
    /// reference so sparse rows (a row that simply omits an empty cell) land in the right column. Assuming
    /// "the data sheet is sheet2.xml" or "column 5 is the weight" is how an importer silently reads the wrong
    /// column after a publisher reorders a file.
    ///
    /// Streaming with <see cref="XmlReader"/> matters: the AISC data sheet is a 14 MB part.
    /// </summary>
    public sealed class XlsxWorkbook : IDisposable
    {
        private const string SpreadsheetMlNamespace =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        private const string RelationshipsNamespace =
            "http://schemas.openxmlformats.org/package/2006/relationships";

        private const string OfficeDocumentRelationshipsNamespace =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        private readonly ZipArchive _archive;
        private readonly Dictionary<string, string> _sheetPartByName;
        private readonly string[] _sharedStrings;

        private XlsxWorkbook(ZipArchive archive, Dictionary<string, string> sheetPartByName, string[] sharedStrings)
        {
            _archive = archive;
            _sheetPartByName = sheetPartByName;
            _sharedStrings = sharedStrings;
        }

        /// <summary>Sheet names in workbook order.</summary>
        public IReadOnlyList<string> SheetNames => _sheetPartByName.Keys.ToArray();

        public static XlsxWorkbook Open(string path)
        {
            if (!File.Exists(path))
            {
                throw new XlsxFormatException("No existe el libro '" + path + "'.");
            }

            var archive = ZipFile.OpenRead(path);

            try
            {
                var relationships = ReadRelationships(archive);
                var sheets = ReadSheets(archive, relationships);
                var sharedStrings = ReadSharedStrings(archive);
                return new XlsxWorkbook(archive, sheets, sharedStrings);
            }
            catch
            {
                archive.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Enumerates the rows of a sheet as (1-based row number, cells by 0-based column index). Only cells
        /// with content appear in the dictionary — an absent key means an empty cell, which is exactly how the
        /// AISC sheet represents most of its data.
        /// </summary>
        public IEnumerable<KeyValuePair<int, IReadOnlyDictionary<int, string>>> ReadRows(string sheetName)
        {
            if (!_sheetPartByName.TryGetValue(sheetName, out var partName))
            {
                throw new XlsxFormatException(
                    "El libro no tiene la hoja '" + sheetName + "'. Hojas presentes: " +
                    string.Join(", ", _sheetPartByName.Keys) + ".");
            }

            var entry = _archive.GetEntry(partName);

            if (entry == null)
            {
                throw new XlsxFormatException("El libro declara la hoja '" + sheetName +
                                              "' pero le falta la parte '" + partName + "'.");
            }

            using (var stream = entry.Open())
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                var cells = new Dictionary<int, string>();
                var rowNumber = 0;

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    if (reader.LocalName == "row" && reader.NamespaceURI == SpreadsheetMlNamespace)
                    {
                        if (rowNumber > 0)
                        {
                            yield return Row(rowNumber, cells);
                            cells = new Dictionary<int, string>();
                        }

                        var reference = reader.GetAttribute("r");
                        rowNumber = reference == null ? rowNumber + 1 : int.Parse(reference);

                        if (reader.IsEmptyElement)
                        {
                            yield return Row(rowNumber, cells);
                            cells = new Dictionary<int, string>();
                            rowNumber = 0;
                        }

                        continue;
                    }

                    if (reader.LocalName == "c" && reader.NamespaceURI == SpreadsheetMlNamespace && rowNumber > 0)
                    {
                        var cellReference = reader.GetAttribute("r");
                        var cellType = reader.GetAttribute("t");
                        var column = ColumnIndex(cellReference);

                        if (reader.IsEmptyElement)
                        {
                            continue;
                        }

                        var value = ReadCellValue(reader.ReadSubtree(), cellType);

                        if (value != null && column >= 0)
                        {
                            cells[column] = value;
                        }
                    }
                }

                if (rowNumber > 0)
                {
                    yield return Row(rowNumber, cells);
                }
            }
        }

        private static KeyValuePair<int, IReadOnlyDictionary<int, string>> Row(
            int number,
            Dictionary<int, string> cells) =>
            new KeyValuePair<int, IReadOnlyDictionary<int, string>>(number, cells);

        private string ReadCellValue(XmlReader cell, string cellType)
        {
            string raw = null;
            var inlineText = new StringBuilder();
            var readingInline = false;

            using (cell)
            {
                while (cell.Read())
                {
                    if (cell.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    if (cell.LocalName == "v" && cell.NamespaceURI == SpreadsheetMlNamespace)
                    {
                        raw = cell.ReadElementContentAsString();
                    }
                    else if (cell.LocalName == "is" && cell.NamespaceURI == SpreadsheetMlNamespace)
                    {
                        readingInline = true;
                    }
                    else if (readingInline && cell.LocalName == "t" && cell.NamespaceURI == SpreadsheetMlNamespace)
                    {
                        inlineText.Append(cell.ReadElementContentAsString());
                    }
                }
            }

            if (cellType == "inlineStr")
            {
                return inlineText.Length == 0 ? null : inlineText.ToString();
            }

            if (raw == null)
            {
                return null;
            }

            if (cellType == "s")
            {
                if (!int.TryParse(raw, out var index) || index < 0 || index >= _sharedStrings.Length)
                {
                    throw new XlsxFormatException(
                        "Una celda referencia la cadena compartida " + raw + ", fuera de rango.");
                }

                return _sharedStrings[index];
            }

            if (cellType == "e")
            {
                throw new XlsxFormatException("Una celda contiene un error de Excel: '" + raw + "'.");
            }

            return raw;
        }

        /// <summary>0-based column index from a cell reference such as <c>AB12</c>. -1 when unreadable.</summary>
        public static int ColumnIndex(string cellReference)
        {
            if (string.IsNullOrEmpty(cellReference))
            {
                return -1;
            }

            var index = 0;
            var any = false;

            foreach (var ch in cellReference)
            {
                if (ch >= 'A' && ch <= 'Z')
                {
                    index = (index * 26) + (ch - 'A' + 1);
                    any = true;
                    continue;
                }

                if (ch >= '0' && ch <= '9')
                {
                    break;
                }

                return -1;
            }

            return any ? index - 1 : -1;
        }

        /// <summary>Spreadsheet column letters for a 0-based index. Used only in diagnostics.</summary>
        public static string ColumnName(int index)
        {
            var name = string.Empty;
            var value = index + 1;

            while (value > 0)
            {
                var remainder = (value - 1) % 26;
                name = (char)('A' + remainder) + name;
                value = (value - 1) / 26;
            }

            return name;
        }

        private static Dictionary<string, string> ReadRelationships(ZipArchive archive)
        {
            var entry = archive.GetEntry("xl/_rels/workbook.xml.rels");

            if (entry == null)
            {
                throw new XlsxFormatException("El libro no tiene 'xl/_rels/workbook.xml.rels'.");
            }

            var relationships = new Dictionary<string, string>(StringComparer.Ordinal);

            using (var stream = entry.Open())
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element &&
                        reader.LocalName == "Relationship" &&
                        reader.NamespaceURI == RelationshipsNamespace)
                    {
                        var id = reader.GetAttribute("Id");
                        var target = reader.GetAttribute("Target");

                        if (id != null && target != null)
                        {
                            relationships[id] = target.StartsWith("/", StringComparison.Ordinal)
                                ? target.TrimStart('/')
                                : "xl/" + target;
                        }
                    }
                }
            }

            return relationships;
        }

        private static Dictionary<string, string> ReadSheets(
            ZipArchive archive,
            IReadOnlyDictionary<string, string> relationships)
        {
            var entry = archive.GetEntry("xl/workbook.xml");

            if (entry == null)
            {
                throw new XlsxFormatException("El libro no tiene 'xl/workbook.xml'.");
            }

            var sheets = new Dictionary<string, string>(StringComparer.Ordinal);

            using (var stream = entry.Open())
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element &&
                        reader.LocalName == "sheet" &&
                        reader.NamespaceURI == SpreadsheetMlNamespace)
                    {
                        var name = reader.GetAttribute("name");
                        var relationshipId = reader.GetAttribute("id", OfficeDocumentRelationshipsNamespace);

                        if (name == null || relationshipId == null ||
                            !relationships.TryGetValue(relationshipId, out var part))
                        {
                            continue;
                        }

                        sheets[name] = part;
                    }
                }
            }

            if (sheets.Count == 0)
            {
                throw new XlsxFormatException("El libro no declara ninguna hoja.");
            }

            return sheets;
        }

        private static string[] ReadSharedStrings(ZipArchive archive)
        {
            var entry = archive.GetEntry("xl/sharedStrings.xml");

            if (entry == null)
            {
                return new string[0];
            }

            var strings = new List<string>();

            using (var stream = entry.Open())
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element ||
                        reader.LocalName != "si" ||
                        reader.NamespaceURI != SpreadsheetMlNamespace)
                    {
                        continue;
                    }

                    var text = new StringBuilder();

                    using (var item = reader.ReadSubtree())
                    {
                        while (item.Read())
                        {
                            if (item.NodeType == XmlNodeType.Element &&
                                item.LocalName == "t" &&
                                item.NamespaceURI == SpreadsheetMlNamespace)
                            {
                                text.Append(item.ReadElementContentAsString());
                            }
                        }
                    }

                    strings.Add(text.ToString());
                }
            }

            return strings.ToArray();
        }

        public void Dispose() => _archive.Dispose();
    }

    /// <summary>The workbook is not what the importer requires. Always fatal — never a silent fallback.</summary>
    public sealed class XlsxFormatException : Exception
    {
        public XlsxFormatException(string message) : base(message)
        {
        }
    }
}
