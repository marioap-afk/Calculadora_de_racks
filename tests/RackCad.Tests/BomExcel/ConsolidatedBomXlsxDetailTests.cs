using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RackCad.Application.Bom;
using Xunit;
using static RackCad.Tests.BomExcelTestKit;

namespace RackCad.Tests
{
    /// <summary>I-68 G1: la columna Cantidad total y la tabla nativa de «Por rack».</summary>
    public class ConsolidatedBomXlsxDetailTests
    {
        private const string Details = "xl/worksheets/sheet1.xml";
        private const string TablePart = "xl/tables/table1.xml";

        private static readonly string[] Headers =
        {
            "Rack", "Tipo", "Copias", "Nivel", "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad", "Cantidad total"
        };

        private static List<List<SheetCell>> PorRack(ConsolidatedBom bom) => Rows(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(bom))[Details]);

        /// <summary>Mezcla real de formas: varios racks, copias, Camas sin piezas, racks planos y longitudes bajo 0.01 de diferencia.</summary>
        private static ConsolidatedBom Mixed() => Drawing(
            Tree("A", "Selectivo", 3,
                Comp("Larguero", "L", "Larguero 96", 96, 8, Line("Perfil", "L", "Larguero 96", 96, 1), Line("Ménsula", "M", "Ménsula", 0, 2)),
                Comp("Cabecera", "C", "Cabecera 144", 144, 2, Line("Poste", "P", "Poste C3", 144, 2))),
            Tree("B", "Selectivo", 2,
                Comp("Larguero", "L", "Larguero 96", 96, 4, Line("Perfil", "L", "Larguero 96", 96, 1), Line("Ménsula", "M", "Ménsula", 0, 2))),
            Tree("D", "Dinámico", 2,
                Comp("Cama", string.Empty, "Cama · BFR 2\"", 96, 1),
                Comp("Poste derivado", "PD", "Poste derivado", 100, 1, Line("Poste", "PD", "poste", 50.001, 2))),
            Tree("E", "Dinámico", 1,
                Comp("Poste derivado", "PD", "Poste derivado", 100, 1, Line("Poste", "PD", "poste", 50.004, 2))),
            Tree("P", "Push Back", 1, Comp("Cama", "Cama", "Cama", 96, 3)),
            Flat("F", "Cama", 2, Line("Rodillo", "R", "Rodillo", 40.5, 10), Line("Riel", "RL", "Riel", 240, 1)),
            Flat("G", "Cabecera", 1, Line("Placa base", "PB", "Placa", 0, 2)));

        [Fact]
        public void I01_CantidadTotal_EsCantidadPorCopias_UnaSolaVez()
        {
            var row = PorRack(Drawing(Flat("R", "Cama", 3, Line("Rodillo", "RD", "Rodillo", 40, 4))))[1];

            Assert.Equal(4, Col(row, "I").Number);
            Assert.Equal(3, Col(row, "C").Number);
            Assert.Equal(12, Col(row, "J").Number);
        }

        [Fact]
        public void I01_Encabezados_ConservanLosProductivosYAgreganCantidadTotalAlFinal()
            => Assert.Equal(Headers, PorRack(Drawing(Flat("R", "Cama", 1, Line("X", "X", "X", 1, 1))))[0].Select(c => c.Raw));

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void I02_CopiasMenorQueUno_SeMuestraYMultiplicaComoUno(int stored)
        {
            var row = PorRack(new ConsolidatedBom(new[] { Flat("R", "Cama", stored, Line("X", "X", "X", 1, 7)) }, null))[1];

            Assert.Equal(1, Col(row, "C").Number);
            Assert.Equal(7, Col(row, "J").Number);
        }

        [Fact]
        public void I03_Pieza_RecetaPorComponentePorCopias()
        {
            var rows = PorRack(Drawing(Tree("R", "Selectivo", 3, Comp("Cabecera", "C", "Cabecera", 144, 2, Line("Poste", "P", "Poste", 144, 3)))));

            Assert.Equal(new[] { 2d, 6d }, new[] { Col(rows[1], "I").Number, Col(rows[1], "J").Number });
            Assert.Equal(new[] { 6d, 18d }, new[] { Col(rows[2], "I").Number, Col(rows[2], "J").Number });
        }

        private static Dictionary<string, double> SumBy(IEnumerable<(string Key, double Value)> rows)
            => rows.GroupBy(r => r.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Sum(r => r.Value), StringComparer.Ordinal);

        private static string TwoDecimals(double value) => Math.Round(value, 2).ToString("0.00", CultureInfo.InvariantCulture);

        [Fact]
        public void I04_Pieza_PorRackConCantidadTotal_IgualaTotalDelDibujo()
        {
            var parts = Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Mixed()));
            var detail = Rows(parts[Details]).Skip(1).Where(r => Col(r, "D").Raw == "Pieza")
                .Select(r => (Col(r, "E").Raw + "|" + Col(r, "F").Raw + "|" + TwoDecimals(Col(r, "H").Number), Col(r, "J").Number));
            var total = Rows(parts["xl/worksheets/sheet2.xml"]).Skip(1).Where(r => Col(r, "A").Raw == "Pieza")
                .Select(r => (Col(r, "B").Raw + "|" + Col(r, "C").Raw + "|" + TwoDecimals(Col(r, "E").Number), Col(r, "F").Number));

            Assert.Equal(SumBy(total).OrderBy(k => k.Key, StringComparer.Ordinal), SumBy(detail).OrderBy(k => k.Key, StringComparer.Ordinal));
        }

        [Fact]
        public void I04b_Componente_PorRackConCantidadTotal_IgualaTotalDelDibujo_SinFilasSinteticas()
        {
            var bom = Mixed();
            var synthetic = new HashSet<string>(bom.Racks.Where(r => !r.Bom.IsComponentBased).Select(r => r.Kind + "|" + r.Name), StringComparer.Ordinal);
            var parts = Unzip(ConsolidatedBomXlsxExporter.ToXlsx(bom));
            var detail = Rows(parts[Details]).Skip(1).Where(r => Col(r, "D").Raw == "Componente")
                .Select(r => (Col(r, "E").Raw + "|" + TwoDecimals(Col(r, "H").Number), Col(r, "J").Number));
            var total = Rows(parts["xl/worksheets/sheet2.xml"]).Skip(1)
                .Where(r => Col(r, "A").Raw == "Componente" && !synthetic.Contains(Col(r, "B").Raw + "|" + Col(r, "D").Raw))
                .Select(r => (Col(r, "B").Raw + "|" + TwoDecimals(Col(r, "E").Number), Col(r, "F").Number));

            Assert.Equal(SumBy(total).OrderBy(k => k.Key, StringComparer.Ordinal), SumBy(detail).OrderBy(k => k.Key, StringComparer.Ordinal));
        }

        [Fact]
        public void I06_ColumnasNumericas_SonCeldasNumericas_YLasDeTextoNo()
        {
            foreach (var row in PorRack(Mixed()).Skip(1))
            {
                foreach (var column in new[] { "C", "H", "I", "J" })
                {
                    Assert.True(Col(row, column).IsNumber, column + " debe ser numerica");
                }

                foreach (var column in new[] { "A", "B", "D", "E", "F", "G" })
                {
                    Assert.False(Col(row, column).IsNumber, column + " debe ser texto");
                }
            }
        }

        [Fact]
        public void I07_PorRack_EsTablaNativaEstable_ConRangoExactoYSoloDetalle()
        {
            var bom = Mixed();
            var parts = Unzip(ConsolidatedBomXlsxExporter.ToXlsx(bom));
            var expectedRows = 1 + bom.Racks.Sum(r => r.Bom.IsComponentBased ? r.Bom.Components.Sum(c => 1 + c.Pieces.Count) : r.Bom.Lines.Count);

            Assert.True(parts.ContainsKey(TablePart), "falta la parte de la tabla");
            var table = XDocument.Parse(parts[TablePart]).Root;
            var range = "A1:J" + expectedRows.ToString(CultureInfo.InvariantCulture);
            Assert.Equal("RackCadBOM", (string)table.Attribute("name"));
            Assert.Equal("RackCadBOM", (string)table.Attribute("displayName"));
            Assert.Equal(range, (string)table.Attribute("ref"));
            Assert.Equal("0", (string)table.Attribute("totalsRowShown"));
            Assert.Equal(range, (string)table.Element(N("autoFilter")).Attribute("ref"));

            var sheetRows = Rows(parts[Details]);
            Assert.Equal(expectedRows, sheetRows.Count);
            Assert.Equal(sheetRows[0].Select(c => c.Raw), table.Element(N("tableColumns")).Elements(N("tableColumn")).Select(c => (string)c.Attribute("name")));

            var style = table.Element(N("tableStyleInfo"));
            Assert.Equal("TableStyleMedium2", (string)style.Attribute("name"));
            Assert.Equal("1", (string)style.Attribute("showRowStripes"));

            var sheet = XDocument.Parse(parts[Details]).Root;
            Assert.Equal("1", (string)sheet.Element(N("tableParts")).Attribute("count"));
            Assert.Contains("../tables/table1.xml", parts["xl/worksheets/_rels/sheet1.xml.rels"]);
            Assert.Contains("/xl/tables/table1.xml", parts["[Content_Types].xml"]);
        }

        [Fact]
        public void I07_TotalDelDibujo_NoEsTabla()
            => Assert.False(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Mixed())).ContainsKey("xl/worksheets/_rels/sheet2.xml.rels"));

        [Fact]
        public void I12_BomVacio_NuncaEsTablaSoloConEncabezado()
        {
            foreach (var bom in new[] { Drawing(), Drawing(Tree("V", "Selectivo", 2)) })
            {
                var parts = Unzip(ConsolidatedBomXlsxExporter.ToXlsx(bom));
                Assert.Equal("A1:J2", (string)XDocument.Parse(parts[TablePart]).Root.Attribute("ref"));
                Assert.Single(Rows(parts[Details]));
            }
        }

        [Fact]
        public void I13_DescripcionVacia_SeConservaYSeCuenta()
        {
            var row = PorRack(Drawing(Flat("R", "Cabecera", 2, Line("Poste", "P", string.Empty, 96, 5))))[1];

            Assert.Equal(string.Empty, Col(row, "G").Raw);
            Assert.False(Col(row, "G").IsNumber);
            Assert.Equal(10, Col(row, "J").Number);
        }

        [Fact]
        public void I16_ModeloBomYDominio_NoConocenExcel_YElProductoNoAgregaPaquetes()
        {
            var root = RepoRoot();
            var sources = Directory.GetFiles(Path.Combine(root, "src", "RackCad.Domain"), "*.cs", SearchOption.AllDirectories).ToList();
            sources.Add(Path.Combine(root, "src", "RackCad.Application", "Bom", "BillOfMaterials.cs"));
            sources.Add(Path.Combine(root, "src", "RackCad.Application", "Bom", "ConsolidatedBom.cs"));
            sources.Add(Path.Combine(root, "src", "RackCad.Application", "Bom", "BomBuilder.cs"));
            sources.AddRange(Directory.GetFiles(Path.Combine(root, "src", "RackCad.Application", "Systems"), "*BomBuilder.cs", SearchOption.AllDirectories));

            Assert.True(sources.Count >= 50, "la busqueda de fuentes no selecciono lo esperado: " + sources.Count);
            foreach (var path in sources)
            {
                var text = File.ReadAllText(path);
                foreach (var word in new[] { "Xlsx", "PivotTable", "OpenXml", "ClosedXML" })
                {
                    Assert.DoesNotContain(word, text, StringComparison.Ordinal);
                }
            }

            foreach (var project in new[] { "RackCad.Domain", "RackCad.Application", "RackCad.UI", "RackCad.Plugin" })
            {
                var packages = XDocument.Load(Path.Combine(root, "src", project, project + ".csproj")).Descendants("PackageReference")
                    .Select(p => (string)p.Attribute("Include"));
                Assert.All(packages, p => Assert.Equal("AutoCAD.NET", p));
            }
        }
    }
}
