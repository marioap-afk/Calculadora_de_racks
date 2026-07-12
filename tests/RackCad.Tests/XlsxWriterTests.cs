using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RackCad.Application.Bom;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The hand-written .xlsx must be a valid OOXML package Excel opens without repair: the right parts, well-formed
    /// XML, escaped text, correct cell references and values. These tests unzip the output and parse every part.
    /// </summary>
    public class XlsxWriterTests
    {
        private const string SheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        private static Dictionary<string, string> Unzip(byte[] bytes)
        {
            var parts = new Dictionary<string, string>();
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

        [Fact]
        public void Build_ProducesAllRequiredParts_AllWellFormedXml()
        {
            var sheet = new XlsxSheet("Datos", new IReadOnlyList<XlsxCell>[]
            {
                new[] { XlsxCell.Str("A", bold: true), XlsxCell.Str("B", bold: true) },
                new[] { XlsxCell.Str("x"), XlsxCell.Num(42.5) }
            });

            var parts = Unzip(XlsxWriter.Build(new[] { sheet }));

            foreach (var required in new[] { "[Content_Types].xml", "_rels/.rels", "xl/workbook.xml", "xl/_rels/workbook.xml.rels", "xl/styles.xml", "xl/worksheets/sheet1.xml" })
            {
                Assert.True(parts.ContainsKey(required), "falta la parte " + required);
                XDocument.Parse(parts[required]); // throws if not well-formed
            }
        }

        [Fact]
        public void Build_WritesTextAndNumberCells_WithCorrectReferences()
        {
            var sheet = new XlsxSheet("Datos", new IReadOnlyList<XlsxCell>[]
            {
                new[] { XlsxCell.Str("Nombre"), XlsxCell.Num(3) },
                new[] { XlsxCell.Str("poste"), XlsxCell.Num(96.5) }
            });

            var doc = XDocument.Parse(Unzip(XlsxWriter.Build(new[] { sheet }))["xl/worksheets/sheet1.xml"]);
            XName C(string n) => XName.Get(n, SheetNs);
            var cells = doc.Descendants(C("c")).ToList();

            var a1 = cells.Single(c => (string)c.Attribute("r") == "A1");
            Assert.Equal("inlineStr", (string)a1.Attribute("t"));
            Assert.Equal("Nombre", a1.Descendants(C("t")).Single().Value);

            var b2 = cells.Single(c => (string)c.Attribute("r") == "B2");
            Assert.Equal("n", (string)b2.Attribute("t"));
            Assert.Equal("96.5", b2.Element(C("v")).Value);
        }

        [Fact]
        public void Build_EscapesXmlSpecialCharactersInText()
        {
            var sheet = new XlsxSheet("Datos", new IReadOnlyList<XlsxCell>[]
            {
                new[] { XlsxCell.Str("A & B <C> \"D\"") }
            });

            var raw = Unzip(XlsxWriter.Build(new[] { sheet }))["xl/worksheets/sheet1.xml"];
            Assert.DoesNotContain("A & B", raw);             // the bare & must have been escaped
            var doc = XDocument.Parse(raw);                  // and it must still parse
            Assert.Equal("A & B <C> \"D\"", doc.Descendants(XName.Get("t", SheetNs)).Single().Value);
        }

        [Fact]
        public void Build_StripsXmlIllegalCharacters_StaysWellFormed()
        {
            // A C0 control char and the noncharacter U+FFFF are illegal in XML 1.0; they must be dropped, not emitted raw.
            var input = "ab" + (char)0x07 + (char)0xFFFF + "c";
            var sheet = new XlsxSheet("Datos", new IReadOnlyList<XlsxCell>[] { new[] { XlsxCell.Str(input) } });

            var raw = Unzip(XlsxWriter.Build(new[] { sheet }))["xl/worksheets/sheet1.xml"];
            var text = XDocument.Parse(raw).Descendants(XName.Get("t", SheetNs)).Single().Value; // parses ⇒ well-formed

            Assert.DoesNotContain((char)0x07, text);
            Assert.DoesNotContain((char)0xFFFF, text);
            Assert.Contains("ab", text); // the real characters survive
        }

        [Fact]
        public void Build_MultipleSheets_AreDeclaredAndDeduped()
        {
            var parts = Unzip(XlsxWriter.Build(new[]
            {
                new XlsxSheet("Hoja", System.Array.Empty<IReadOnlyList<XlsxCell>>()),
                new XlsxSheet("Hoja", System.Array.Empty<IReadOnlyList<XlsxCell>>())
            }));

            Assert.True(parts.ContainsKey("xl/worksheets/sheet2.xml"));
            var names = XDocument.Parse(parts["xl/workbook.xml"]).Descendants(XName.Get("sheet", SheetNs))
                .Select(s => (string)s.Attribute("name")).ToList();
            Assert.Equal(2, names.Count);
            Assert.Equal(names.Count, names.Distinct().Count()); // duplicate sheet names were made unique
        }

        [Theory]
        [InlineData(0, "A")]
        [InlineData(25, "Z")]
        [InlineData(26, "AA")]
        [InlineData(27, "AB")]
        [InlineData(701, "ZZ")]
        public void ColumnName_MapsIndexToLetters(int index, string expected)
            => Assert.Equal(expected, XlsxWriter.ColumnName(index));

        private static List<List<string>> ReadRows(string sheetXml)
        {
            XName N(string n) => XName.Get(n, SheetNs);
            var rows = new List<List<string>>();
            foreach (var row in XDocument.Parse(sheetXml).Descendants(N("row")))
            {
                var cells = new List<string>();
                foreach (var c in row.Elements(N("c")))
                {
                    cells.Add((string)c.Attribute("t") == "inlineStr"
                        ? c.Descendants(N("t")).Single().Value
                        : c.Element(N("v"))?.Value ?? string.Empty);
                }

                rows.Add(cells);
            }

            return rows;
        }

        [Fact]
        public void BomXlsxExporter_FlatBom_WritesHeaderAndLine()
        {
            var bom = new BillOfMaterials(new List<BomLine>
            {
                new BomLine { Category = "Poste", ProfileId = "P1", Description = "Poste 132", Length = 132, Quantity = 4 }
            });

            var rows = ReadRows(Unzip(BomXlsxExporter.ToXlsx(bom))["xl/worksheets/sheet1.xml"]);

            Assert.Equal(new[] { "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad" }, rows[0]);
            Assert.Equal(new[] { "Poste", "P1", "Poste 132", "132", "4" }, rows[1]);
        }

        [Fact]
        public void BomXlsxExporter_ComponentBom_FlattensPieceQuantityByComponentQty()
        {
            var component = new BomComponent
            {
                Category = "Cabecera", ProfileId = "CAB", Description = "Cabecera A", Length = 0, Quantity = 2,
                Pieces = new List<BomLine> { new BomLine { Category = "Poste", ProfileId = "P1", Description = "poste", Length = 132, Quantity = 2 } }
            };

            var rows = ReadRows(Unzip(BomXlsxExporter.ToXlsx(new BillOfMaterials(new List<BomComponent> { component })))["xl/worksheets/sheet1.xml"]);

            Assert.Equal("Componente", rows[1][0]);
            Assert.Equal("2", rows[1][5]);   // component quantity
            Assert.Equal("Pieza", rows[2][0]);
            Assert.Equal("4", rows[2][5]);   // 2 pieces/unit × 2 components
        }
    }
}
