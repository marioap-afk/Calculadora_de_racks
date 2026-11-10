using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RackCad.Application.Bom;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>One written cell, read back from the raw SpreadsheetML, independently of the writer's own model.</summary>
    internal sealed class SheetCell
    {
        public SheetCell(string column, int row, bool isNumber, string raw, bool bold)
        {
            Column = column;
            Row = row;
            IsNumber = isNumber;
            Raw = raw;
            Bold = bold;
        }

        public string Column { get; }
        public int Row { get; }
        public bool IsNumber { get; }
        public string Raw { get; }
        public bool Bold { get; }
        public double Number => double.Parse(Raw, CultureInfo.InvariantCulture);
    }

    /// <summary>Opens an exported workbook as raw parts and builds the BOM inputs the I-68 tests share.</summary>
    internal static class BomExcelTestKit
    {
        public const string Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        public const string PackageRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static XName N(string localName) => XName.Get(localName, Ns);

        public static Dictionary<string, string> Unzip(byte[] bytes)
        {
            var parts = new Dictionary<string, string>(StringComparer.Ordinal);
            using (var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                    {
                        parts[entry.FullName] = reader.ReadToEnd();
                    }
                }
            }

            return parts;
        }

        public static List<List<SheetCell>> Rows(string sheetXml)
        {
            var rows = new List<List<SheetCell>>();
            foreach (var row in XDocument.Parse(sheetXml).Descendants(N("row")))
            {
                var cells = new List<SheetCell>();
                foreach (var c in row.Elements(N("c")))
                {
                    var reference = (string)c.Attribute("r");
                    var column = new string(reference.TakeWhile(char.IsLetter).ToArray());
                    var rowNumber = int.Parse(reference.Substring(column.Length), CultureInfo.InvariantCulture);
                    var isInline = (string)c.Attribute("t") == "inlineStr";
                    var raw = isInline ? c.Descendants(N("t")).Single().Value : c.Element(N("v"))?.Value ?? string.Empty;
                    cells.Add(new SheetCell(column, rowNumber, !isInline, raw, (string)c.Attribute("s") == "1"));
                }

                rows.Add(cells);
            }

            return rows;
        }

        public static SheetCell Col(IEnumerable<SheetCell> row, string column) => Assert.Single(row, c => c.Column == column);

        public static BomLine Line(string category, string id, string description, double length, int quantity)
            => new BomLine { Category = category, ProfileId = id, Description = description, Length = length, Quantity = quantity };

        public static BomComponent Comp(string category, string id, string description, double length, int quantity, params BomLine[] pieces)
            => new BomComponent { Category = category, ProfileId = id, Description = description, Length = length, Quantity = quantity, Pieces = pieces.ToList() };

        public static ConsolidatedRackBom Tree(string name, string kind, int copies, params BomComponent[] components)
            => new ConsolidatedRackBom { Name = name, Kind = kind, Copies = copies, Bom = new BillOfMaterials(components.ToList()) };

        public static ConsolidatedRackBom Flat(string name, string kind, int copies, params BomLine[] lines)
            => new ConsolidatedRackBom { Name = name, Kind = kind, Copies = copies, Bom = new BillOfMaterials(lines.ToList()) };

        public static ConsolidatedBom Drawing(params ConsolidatedRackBom[] racks) => ConsolidatedBomBuilder.Build(racks);

        public static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontro la raiz del repositorio (RackCad.sln).");
            return dir.FullName;
        }
    }
}
