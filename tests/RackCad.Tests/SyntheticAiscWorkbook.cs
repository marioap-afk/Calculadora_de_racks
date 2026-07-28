using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace RackCad.Tests
{
    /// <summary>
    /// Builds a small, real .xlsx in memory so the importer can be tested WITHOUT versioning a binary
    /// fixture and without the official AISC workbook (which is an external source and is deliberately not
    /// committed).
    ///
    /// It writes genuine OOXML — container, workbook part, relationships, shared strings and a worksheet with
    /// <c>r</c>-referenced cells — so what the tests exercise is the same code path the real 2 MB workbook
    /// goes through, not a stub.
    ///
    /// The header layout mirrors the published one, including the METRIC MIRROR: the column resolver finds the
    /// boundary by looking for the second <c>EDI_Std_Nomenclature</c>, so a fixture without the mirror would
    /// not test the resolver at all.
    /// </summary>
    internal static class SyntheticAiscWorkbook
    {
        public const string SheetName = "Database v16.0";
        public const string ReadmeSheetName = "Readme";

        /// <summary>
        /// A Readme that carries the three markers the verifier demands. It is a faithful abbreviation of the
        /// real one: the product line, the manual edition and the sentence that introduces the EDI convention.
        /// </summary>
        public static readonly string[] ValidReadme =
        {
            "AISC Shapes Database v16.0",
            "Readme File",
            "August 2023",
            "AISC Shapes Database v16.0 is an update to Shapes Database v15.0. This version is consistent " +
            "with shape properties and dimensions tabulated in the AISC Steel Construction Manual, " +
            "16th Edition, 1st Printing.",
            "The shape designation according to the AISC Naming Convention for Structural Steel Products " +
            "for Use in Electronic Data Interchange (EDI), June 25, 2001."
        };

        /// <summary>A Readme of a DIFFERENT revision: same shape, wrong product.</summary>
        public static readonly string[] PreviousRevisionReadme =
        {
            "AISC Shapes Database v15.0",
            "Readme File",
            "AISC Shapes Database v15.0 is consistent with the AISC Steel Construction Manual, " +
            "15th Edition.",
            "Naming Convention for Structural Steel Products for Use in Electronic Data Interchange (EDI)."
        };

        /// <summary>The value columns, in published order. Shared by the US block and the metric mirror.</summary>
        public static readonly string[] ValueColumns =
        {
            "W", "A", "d", "ddet", "Ht", "h", "OD", "bf", "bfdet", "B", "b",
            "tw", "twdet", "twdet/2", "tf", "tfdet", "t", "tnom", "tdes",
            "kdes", "kdet", "k1", "x", "y", "eo", "xp", "yp",
            "Ix", "Zx", "Sx", "rx", "Iy", "Zy", "Sy", "ry", "Iz", "rz", "Sz",
            "J", "Cw", "C", "Wno", "Sw1", "Sw2", "Sw3", "Qf", "Qw", "ro", "H", "tan(α)", "Iw",
            "zA", "zB", "zC", "wA", "wB", "wC",
            "SwA", "SwB", "SwC", "SzA", "SzB", "SzC",
            "rts", "ho", "PA", "PA2", "PB", "PC", "PD", "T", "WGi", "WGo"
        };

        /// <summary>The "not applicable" marker AISC prints: an EN DASH, not an empty cell.</summary>
        public const string NotApplicable = "–";

        public static string[] Headers()
        {
            var headers = new List<string> { "Type", "EDI_Std_Nomenclature", "AISC_Manual_Label", "T_F" };
            headers.AddRange(ValueColumns);
            headers.Add("EDI_Std_Nomenclature");
            headers.Add("AISC_Manual_Label");
            headers.AddRange(ValueColumns);
            return headers.ToArray();
        }

        /// <summary>
        /// One source row under construction, addressed by published variable name. Not sealed: a test that
        /// needs to plant a hand-crafted inconsistency overrides <see cref="Cells"/>.
        /// </summary>
        public class RowBuilder
        {
            private readonly Dictionary<string, string> _us = new Dictionary<string, string>(StringComparer.Ordinal);

            public RowBuilder(string type, string edi, string label)
            {
                Type = type;
                Edi = edi;
                Label = label;
            }

            public string Type { get; }

            public string Edi { get; }

            public string Label { get; }

            public string SpecialNoteFlag { get; set; }

            public RowBuilder With(string column, double value)
            {
                _us[column] = value.ToString("R", CultureInfo.InvariantCulture);
                return this;
            }

            /// <summary>Writes raw text, so a test can plant an unparseable value on purpose.</summary>
            public RowBuilder WithRaw(string column, string value)
            {
                _us[column] = value;
                return this;
            }

            internal virtual string[] Cells()
            {
                var cells = new List<string> { Type, Edi, Label, SpecialNoteFlag ?? NotApplicable };

                foreach (var column in ValueColumns)
                {
                    cells.Add(_us.TryGetValue(column, out var value) ? value : NotApplicable);
                }

                cells.Add(Edi);
                cells.Add(Label);

                // The metric mirror: only the magnitudes the coherence check compares are filled, with the
                // exact conversion. Everything else stays "not applicable", which the check skips.
                foreach (var column in ValueColumns)
                {
                    cells.Add(_us.TryGetValue(column, out var value) && double.TryParse(
                        value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                        ? Metric(column, number)
                        : NotApplicable);
                }

                return cells.ToArray();
            }

            private static string Metric(string column, double value)
            {
                double factor;

                switch (column)
                {
                    case "W": factor = 0.45359237d / 0.3048d; break;
                    case "A": factor = 645.16; break;
                    default: factor = 25.4; break;
                }

                return (value * factor).ToString("R", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>A complete W row with every value its family requires.</summary>
        public static RowBuilder W(string designation, double weight)
        {
            var row = new RowBuilder("W", designation, designation) { SpecialNoteFlag = "F" };
            row.With("W", weight).With("A", 7.65)
                .With("d", 12.2).With("ddet", 12.25).With("bf", 6.49).With("bfdet", 6.5)
                .With("tw", 0.23).With("twdet", 0.25).With("twdet/2", 0.125)
                .With("tf", 0.38).With("tfdet", 0.375)
                .With("kdes", 0.68).With("kdet", 1.0625).With("k1", 0.75)
                .With("Ix", 204).With("Zx", 37.2).With("Sx", 33.4).With("rx", 5.17)
                .With("Iy", 17.3).With("Zy", 8.17).With("Sy", 5.34).With("ry", 1.51)
                .With("J", 0.3).With("Cw", 607).With("T", 10.5).With("WGi", 3.5);
            return row;
        }

        /// <summary>A rectangular or square HSS: walls present, no outside diameter.</summary>
        public static RowBuilder HssRectangular(string edi, string label, double ht, double b, double thickness)
        {
            var row = new RowBuilder("HSS", edi, label);
            row.With("W", 12.21).With("A", 3.37)
                .With("Ht", ht).With("h", ht - 0.9).With("B", b).With("b", b - 0.9)
                .With("tnom", thickness).With("tdes", thickness * 0.93)
                .With("Ix", 7.8).With("Zx", 4.69).With("Sx", 3.9).With("rx", 1.52)
                .With("Iy", 7.8).With("Zy", 4.69).With("Sy", 3.9).With("ry", 1.52)
                .With("J", 12.8).With("C", 6.56);
            return row;
        }

        /// <summary>A round HSS: outside diameter present, no walls. Must be excluded.</summary>
        public static RowBuilder HssRound(string designation, double outsideDiameter)
        {
            var row = new RowBuilder("HSS", designation, designation);
            row.With("W", 9.63).With("A", 2.66).With("OD", outsideDiameter)
                .With("tnom", 0.25).With("tdes", 0.233)
                .With("Ix", 4.79).With("Iy", 4.79).With("J", 9.58);
            return row;
        }

        public static RowBuilder Channel(string designation, double weight)
        {
            var row = new RowBuilder("C", designation, designation);
            row.With("W", weight).With("A", 4.48)
                .With("d", 10).With("ddet", 10).With("bf", 2.6).With("bfdet", 2.625)
                .With("tw", 0.24).With("twdet", 0.25).With("twdet/2", 0.125)
                .With("tf", 0.436).With("tfdet", 0.4375)
                .With("kdes", 0.87).With("kdet", 1).With("x", 0.634).With("eo", 0.796).With("xp", 0.239)
                .With("Ix", 67.3).With("Zx", 15.8).With("Sx", 13.5).With("rx", 3.87)
                .With("Iy", 2.27).With("Zy", 2.34).With("Sy", 1.15).With("ry", 0.711)
                .With("J", 0.209).With("Cw", 39.4).With("ro", 4.32).With("H", 0.796);
            return row;
        }

        public static RowBuilder Angle(string edi, string label, double longLeg, double shortLeg, double thickness)
        {
            var row = new RowBuilder("L", edi, label);
            row.With("W", 6.6).With("A", 1.93)
                .With("d", shortLeg).With("b", longLeg).With("t", thickness)
                .With("kdes", 0.5).With("kdet", 0.75)
                .With("x", 1.08).With("y", 1.08).With("xp", 0.741).With("yp", 0.741)
                .With("Ix", 3).With("Zx", 1.29).With("Sx", 1.03).With("rx", 1.25)
                .With("Iy", 3).With("Zy", 1.29).With("Sy", 1.03).With("ry", 1.25)
                .With("Iz", 1.22).With("rz", 0.795).With("Sz", 1.14)
                .With("J", 0.0438).With("Cw", 0.0438).With("ro", 1.94).With("H", 0.716)
                .With("tan(α)", 1).With("Iw", 4.79);
            return row;
        }

        /// <summary>A row of a type the MVP does not import. Only the columns the classifier reads matter.</summary>
        public static RowBuilder OtherType(string type, string designation)
        {
            var row = new RowBuilder(type, designation, designation);
            row.With("W", 10).With("A", 3);
            return row;
        }

        public static string WriteToTempFile(
            IEnumerable<RowBuilder> rows,
            string[] headers = null,
            string[] readme = null,
            bool includeReadme = true,
            string dataSheetName = SheetName)
        {
            var path = Path.Combine(Path.GetTempPath(), "rackcad-i36a-" + Guid.NewGuid().ToString("N") + ".xlsx");
            File.WriteAllBytes(path, Build(rows, headers, readme, includeReadme, dataSheetName));
            return path;
        }

        /// <summary>
        /// Builds the workbook. By default it carries a valid <c>Readme</c> and the <c>Database v16.0</c>
        /// sheet, so the fixtures go through the SAME verified path production uses; the parameters exist so a
        /// test can take one of those away on purpose.
        /// </summary>
        public static byte[] Build(
            IEnumerable<RowBuilder> rows,
            string[] headers = null,
            string[] readme = null,
            bool includeReadme = true,
            string dataSheetName = SheetName)
        {
            var readmeLines = readme ?? ValidReadme;
            var allRows = new List<string[]> { headers ?? Headers() };
            allRows.AddRange(rows.Select(row => row.Cells()));

            // Excel keeps every distinct string once and references it by index; the reader has to follow that
            // indirection, so the fixture uses it too.
            var readmeRows = readmeLines.Select(line => new[] { line }).ToList();
            var sharedStrings = new List<string>();
            var sharedStringIndex = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var value in allRows.Concat(readmeRows)
                .SelectMany(row => row)
                .Where(value => !IsNumber(value)))
            {
                if (!sharedStringIndex.ContainsKey(value))
                {
                    sharedStringIndex.Add(value, sharedStrings.Count);
                    sharedStrings.Add(value);
                }
            }

            using (var buffer = new MemoryStream())
            {
                using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
                {
                    Write(archive, "[Content_Types].xml", ContentTypes(includeReadme));
                    Write(archive, "_rels/.rels", PackageRelationships);
                    Write(archive, "xl/workbook.xml", WorkbookXml(includeReadme, dataSheetName));
                    Write(archive, "xl/_rels/workbook.xml.rels", WorkbookRelationships(includeReadme));
                    Write(archive, "xl/sharedStrings.xml", SharedStringsXml(sharedStrings));
                    Write(archive, "xl/worksheets/sheet1.xml", SheetXml(allRows, sharedStringIndex));

                    if (includeReadme)
                    {
                        Write(archive, "xl/worksheets/sheet2.xml", SheetXml(readmeRows, sharedStringIndex));
                    }
                }

                return buffer.ToArray();
            }
        }

        private static bool IsNumber(string value) =>
            !string.IsNullOrEmpty(value) &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _);

        private static void Write(ZipArchive archive, string name, string content)
        {
            var entry = archive.CreateEntry(name);

            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string SheetXml(
            IReadOnlyList<string[]> rows,
            IReadOnlyDictionary<string, int> sharedStringIndex)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");

            for (var r = 0; r < rows.Count; r++)
            {
                builder.Append("<row r=\"").Append(r + 1).Append("\">");

                for (var c = 0; c < rows[r].Length; c++)
                {
                    var value = rows[r][c];

                    if (string.IsNullOrEmpty(value))
                    {
                        // A genuinely absent cell: Excel omits it, and the reader must cope with the gap.
                        continue;
                    }

                    var reference = ColumnName(c) + (r + 1);

                    if (IsNumber(value))
                    {
                        builder.Append("<c r=\"").Append(reference).Append("\"><v>")
                            .Append(Escape(value)).Append("</v></c>");
                    }
                    else
                    {
                        builder.Append("<c r=\"").Append(reference).Append("\" t=\"s\"><v>")
                            .Append(sharedStringIndex[value]).Append("</v></c>");
                    }
                }

                builder.Append("</row>");
            }

            builder.Append("</sheetData></worksheet>");
            return builder.ToString();
        }

        private static string SharedStringsXml(IReadOnlyList<string> strings)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" count=\"")
                .Append(strings.Count).Append("\" uniqueCount=\"").Append(strings.Count).Append("\">");

            foreach (var value in strings)
            {
                builder.Append("<si><t xml:space=\"preserve\">").Append(Escape(value)).Append("</t></si>");
            }

            builder.Append("</sst>");
            return builder.ToString();
        }

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

        private static string Escape(string value) =>
            value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        private static string ContentTypes(bool includeReadme) =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
            "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
            "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
            "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
            "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
            (includeReadme
                ? "<Override PartName=\"/xl/worksheets/sheet2.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                : string.Empty) +
            "<Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/>" +
            "</Types>";

        private const string PackageRelationships =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
            "</Relationships>";

        private static string WorkbookXml(bool includeReadme, string dataSheetName) =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
            "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>" +
            (includeReadme
                ? "<sheet name=\"" + ReadmeSheetName + "\" sheetId=\"2\" r:id=\"rId3\"/>"
                : string.Empty) +
            "<sheet name=\"" + Escape(dataSheetName) + "\" sheetId=\"1\" r:id=\"rId1\"/>" +
            "</sheets></workbook>";

        private static string WorkbookRelationships(bool includeReadme) =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
            (includeReadme
                ? "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet2.xml\"/>"
                : string.Empty) +
            "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>" +
            "</Relationships>";
    }
}
