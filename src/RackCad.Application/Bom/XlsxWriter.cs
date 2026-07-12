using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RackCad.Application.Bom
{
    /// <summary>One cell of an <see cref="XlsxSheet"/>: either text or a number, optionally bold (used for headers).</summary>
    public readonly struct XlsxCell
    {
        private XlsxCell(string text, double? number, bool bold)
        {
            Text = text;
            Number = number;
            Bold = bold;
        }

        public string Text { get; }
        public double? Number { get; }
        public bool Bold { get; }

        public static XlsxCell Str(string text, bool bold = false) => new XlsxCell(text ?? string.Empty, null, bold);
        public static XlsxCell Num(double value, bool bold = false) => new XlsxCell(null, value, bold);
    }

    /// <summary>One worksheet: a name and its rows (each a list of cells). The first row is typically a bold header.</summary>
    public sealed class XlsxSheet
    {
        public XlsxSheet(string name, IReadOnlyList<IReadOnlyList<XlsxCell>> rows)
        {
            Name = name ?? "Hoja";
            Rows = rows ?? Array.Empty<IReadOnlyList<XlsxCell>>();
        }

        public string Name { get; }
        public IReadOnlyList<IReadOnlyList<XlsxCell>> Rows { get; }
    }

    /// <summary>
    /// Writes a minimal, valid <c>.xlsx</c> (OOXML SpreadsheetML) as a byte array with NO external dependency — just a
    /// ZIP (<see cref="System.IO.Compression"/>) of hand-written XML parts. Cells are inline strings or numbers; a bold
    /// cell style is available for headers. Excel opens the result without a repair prompt. Pure/testable (no AutoCAD).
    /// </summary>
    public static class XlsxWriter
    {
        private const string Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        public static byte[] Build(IReadOnlyList<XlsxSheet> sheets)
        {
            sheets = sheets ?? Array.Empty<XlsxSheet>();
            if (sheets.Count == 0)
            {
                sheets = new[] { new XlsxSheet("Hoja1", Array.Empty<IReadOnlyList<XlsxCell>>()) };
            }

            var names = UniqueSheetNames(sheets);

            using (var stream = new MemoryStream())
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    Write(zip, "[Content_Types].xml", ContentTypes(sheets.Count));
                    Write(zip, "_rels/.rels", RootRels());
                    Write(zip, "xl/workbook.xml", Workbook(names));
                    Write(zip, "xl/_rels/workbook.xml.rels", WorkbookRels(sheets.Count));
                    Write(zip, "xl/styles.xml", Styles());
                    for (var i = 0; i < sheets.Count; i++)
                    {
                        Write(zip, "xl/worksheets/sheet" + (i + 1).ToString(CultureInfo.InvariantCulture) + ".xml", Sheet(sheets[i]));
                    }
                }

                return stream.ToArray();
            }
        }

        private static void Write(ZipArchive zip, string path, string content)
        {
            var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string ContentTypes(int sheetCount)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
            builder.Append("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>");
            builder.Append("<Default Extension=\"xml\" ContentType=\"application/xml\"/>");
            builder.Append("<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");
            builder.Append("<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");
            for (var i = 0; i < sheetCount; i++)
            {
                builder.Append("<Override PartName=\"/xl/worksheets/sheet").Append((i + 1).ToString(CultureInfo.InvariantCulture))
                    .Append(".xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
            }

            builder.Append("</Types>");
            return builder.ToString();
        }

        private static string RootRels()
            => "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
               + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
               + "<Relationship Id=\"rId1\" Type=\"" + RelNs + "/officeDocument\" Target=\"xl/workbook.xml\"/>"
               + "</Relationships>";

        private static string Workbook(IReadOnlyList<string> names)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<workbook xmlns=\"").Append(Ns).Append("\" xmlns:r=\"").Append(RelNs).Append("\"><sheets>");
            for (var i = 0; i < names.Count; i++)
            {
                builder.Append("<sheet name=\"").Append(EscapeAttr(names[i]))
                    .Append("\" sheetId=\"").Append((i + 1).ToString(CultureInfo.InvariantCulture))
                    .Append("\" r:id=\"rId").Append((i + 1).ToString(CultureInfo.InvariantCulture)).Append("\"/>");
            }

            builder.Append("</sheets></workbook>");
            return builder.ToString();
        }

        private static string WorkbookRels(int sheetCount)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            for (var i = 0; i < sheetCount; i++)
            {
                builder.Append("<Relationship Id=\"rId").Append((i + 1).ToString(CultureInfo.InvariantCulture))
                    .Append("\" Type=\"").Append(RelNs).Append("/worksheet\" Target=\"worksheets/sheet")
                    .Append((i + 1).ToString(CultureInfo.InvariantCulture)).Append(".xml\"/>");
            }

            // Styles get an id AFTER the sheets so it can't collide with a sheet rId.
            builder.Append("<Relationship Id=\"rId").Append((sheetCount + 1).ToString(CultureInfo.InvariantCulture))
                .Append("\" Type=\"").Append(RelNs).Append("/styles\" Target=\"styles.xml\"/>");
            builder.Append("</Relationships>");
            return builder.ToString();
        }

        /// <summary>Minimal styles: cellXf 0 = normal, cellXf 1 = bold (headers). No theme/color part needed.</summary>
        private static string Styles()
            => "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
               + "<styleSheet xmlns=\"" + Ns + "\">"
               + "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font><font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font></fonts>"
               + "<fills count=\"2\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill></fills>"
               + "<borders count=\"1\"><border/></borders>"
               + "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>"
               + "<cellXfs count=\"2\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>"
               + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/></cellXfs>"
               + "</styleSheet>";

        private static string Sheet(XlsxSheet sheet)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<worksheet xmlns=\"").Append(Ns).Append("\"><sheetData>");

            for (var r = 0; r < sheet.Rows.Count; r++)
            {
                var row = sheet.Rows[r];
                var rowNumber = (r + 1).ToString(CultureInfo.InvariantCulture);
                builder.Append("<row r=\"").Append(rowNumber).Append("\">");
                for (var c = 0; c < row.Count; c++)
                {
                    var cell = row[c];
                    var reference = ColumnName(c) + rowNumber;
                    var style = cell.Bold ? " s=\"1\"" : string.Empty;
                    if (cell.Number.HasValue)
                    {
                        builder.Append("<c r=\"").Append(reference).Append('"').Append(style).Append(" t=\"n\"><v>")
                            .Append(cell.Number.Value.ToString("0.######", CultureInfo.InvariantCulture)).Append("</v></c>");
                    }
                    else
                    {
                        builder.Append("<c r=\"").Append(reference).Append('"').Append(style)
                            .Append(" t=\"inlineStr\"><is><t xml:space=\"preserve\">").Append(EscapeText(cell.Text)).Append("</t></is></c>");
                    }
                }

                builder.Append("</row>");
            }

            builder.Append("</sheetData></worksheet>");
            return builder.ToString();
        }

        /// <summary>Zero-based column index to its Excel letter(s): 0→A, 25→Z, 26→AA, …</summary>
        public static string ColumnName(int index)
        {
            var name = string.Empty;
            var n = index;
            do
            {
                name = (char)('A' + (n % 26)) + name;
                n = (n / 26) - 1;
            }
            while (n >= 0);

            return name;
        }

        /// <summary>Excel sheet names: ≤31 chars, no <c>[ ] : * ? / \</c>, non-empty and unique (deduped with a suffix).</summary>
        private static IReadOnlyList<string> UniqueSheetNames(IReadOnlyList<XlsxSheet> sheets)
        {
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<string>(sheets.Count);
            for (var i = 0; i < sheets.Count; i++)
            {
                var name = SanitizeSheetName(sheets[i].Name);
                var candidate = name;
                var suffix = 2;
                while (!used.Add(candidate))
                {
                    var tag = " (" + suffix.ToString(CultureInfo.InvariantCulture) + ")";
                    candidate = (name.Length + tag.Length > 31 ? name.Substring(0, 31 - tag.Length) : name) + tag;
                    suffix++;
                }

                result.Add(candidate);
            }

            return result;
        }

        private static string SanitizeSheetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Hoja";
            }

            var builder = new StringBuilder(name.Length);
            foreach (var ch in name)
            {
                builder.Append(ch == '[' || ch == ']' || ch == ':' || ch == '*' || ch == '?' || ch == '/' || ch == '\\' ? ' ' : ch);
            }

            var clean = builder.ToString().Trim();
            if (clean.Length == 0)
            {
                clean = "Hoja";
            }

            return clean.Length > 31 ? clean.Substring(0, 31) : clean;
        }

        private static string EscapeText(string value)
        {
            value ??= string.Empty;
            var builder = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '&': builder.Append("&amp;"); break;
                    case '<': builder.Append("&lt;"); break;
                    case '>': builder.Append("&gt;"); break;
                    default:
                        // Strip characters XML 1.0 forbids, which would corrupt the part: C0 controls (except tab/
                        // newline/CR) and the noncharacters U+FFFE / U+FFFF. (Unpaired surrogates are handled by the
                        // UTF-8 encoder's replacement fallback.)
                        if ((ch < 0x20 && ch != '\t' && ch != '\n' && ch != '\r') || ch >= (char)0xFFFE) builder.Append(' ');
                        else builder.Append(ch);
                        break;
                }
            }

            return builder.ToString();
        }

        private static string EscapeAttr(string value)
            => EscapeText(value).Replace("\"", "&quot;");
    }
}
