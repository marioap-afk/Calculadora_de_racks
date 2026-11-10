using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using RackCad.Application.Bom;
using Xunit;
using static RackCad.Tests.BomExcelTestKit;

namespace RackCad.Tests
{
    /// <summary>I-68 G3: el contrato del libro con el BOM real de cada sistema, no con formas sinteticas.</summary>
    public class BomExcelSystemMatrixTests
    {
        private const string Details = "xl/worksheets/sheet1.xml";
        private const string Totals = "xl/worksheets/sheet2.xml";

        public static IEnumerable<object[]> Systems => BomExcelRealSystems.Names.Select(name => new object[] { name });

        private static List<List<SheetCell>> Data(Dictionary<string, string> parts) => Rows(parts[Details]).Skip(1).ToList();

        private static string TwoDecimals(double value) => Math.Round(value, 2).ToString("0.00", CultureInfo.InvariantCulture);

        private static Dictionary<string, double> SumBy(IEnumerable<(string Key, double Value)> rows)
            => rows.GroupBy(r => r.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Sum(r => r.Value), StringComparer.Ordinal);

        private static void AssertSameTotals(Dictionary<string, double> expected, Dictionary<string, double> actual)
        {
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Keys.OrderBy(k => k, StringComparer.Ordinal), actual.Keys.OrderBy(k => k, StringComparer.Ordinal));
            foreach (var pair in expected)
            {
                Assert.Equal(pair.Value, actual[pair.Key], 6);
            }
        }

        private static void AssertWorkbookContract(ConsolidatedBom bom)
        {
            var bytes = ConsolidatedBomXlsxExporter.ToXlsx(bom);
            var parts = Unzip(bytes);
            var data = Data(parts);
            Assert.NotEmpty(data);

            foreach (var row in data)
            {
                Assert.Equal(Col(row, "I").Number * Col(row, "C").Number, Col(row, "J").Number);
                foreach (var numeric in new[] { "C", "H", "I", "J" })
                {
                    Assert.True(Col(row, numeric).IsNumber, numeric);
                }
            }

            var table = XDocument.Parse(parts["xl/tables/table1.xml"]).Root;
            Assert.Equal("A1:J" + (data.Count + 1).ToString(CultureInfo.InvariantCulture), (string)table.Attribute("ref"));

            var pivot = XDocument.Parse(parts["xl/pivotTables/pivotTable1.xml"]).Root;
            Assert.Equal(new[] { 0, 3 }, pivot.Element(N("pageFields")).Elements(N("pageField")).Select(f => (int)f.Attribute("fld")));
            Assert.Equal(new[] { 6, 7 }, pivot.Element(N("rowFields")).Elements(N("field")).Select(f => (int)f.Attribute("x")));

            var grand = Rows(parts["xl/worksheets/sheet3.xml"]).Last(r => r.Any(c => c.Column == "A"));
            Assert.Equal("Total general", Col(grand, "A").Raw);
            Assert.Equal(data.Sum(r => Col(r, "J").Number), Col(grand, "B").Number, 6);

            var totals = Rows(parts[Totals]).Skip(1).ToList();
            AssertSameTotals(
                SumBy(totals.Where(r => Col(r, "A").Raw == "Pieza").Select(r => (Col(r, "B").Raw + "|" + Col(r, "C").Raw + "|" + TwoDecimals(Col(r, "E").Number), Col(r, "F").Number))),
                SumBy(data.Where(r => Col(r, "D").Raw == "Pieza").Select(r => (Col(r, "E").Raw + "|" + Col(r, "F").Raw + "|" + TwoDecimals(Col(r, "H").Number), Col(r, "J").Number))));

            var synthetic = new HashSet<string>(bom.Racks.Where(r => !r.Bom.IsComponentBased).Select(r => r.Kind + "|" + r.Name), StringComparer.Ordinal);
            var componentDetail = SumBy(data.Where(r => Col(r, "D").Raw == "Componente").Select(r => (Col(r, "E").Raw + "|" + TwoDecimals(Col(r, "H").Number), Col(r, "J").Number)));
            var componentTotals = SumBy(totals.Where(r => Col(r, "A").Raw == "Componente" && !synthetic.Contains(Col(r, "B").Raw + "|" + Col(r, "D").Raw))
                .Select(r => (Col(r, "B").Raw + "|" + TwoDecimals(Col(r, "E").Number), Col(r, "F").Number)));
            if (bom.Racks.Any(r => r.Bom.IsComponentBased))
            {
                AssertSameTotals(componentTotals, componentDetail);
            }

            using (var stream = new MemoryStream(bytes))
            using (var document = SpreadsheetDocument.Open(stream, false))
            {
                var errors = new OpenXmlValidator(FileFormatVersions.Office2016).Validate(document).ToList();
                Assert.True(errors.Count == 0, string.Join("\n", errors.Select(e => e.Id + " " + e.Description)));
            }
        }

        [Theory]
        [MemberData(nameof(Systems))]
        public void I19_CadaSistema_CumpleElContratoDelLibro(string system) => AssertWorkbookContract(Drawing(BomExcelRealSystems.Rack(system, 3)));

        [Fact]
        public void I19_LosSeisSistemasJuntos_CumplenElContratoDelLibro() => AssertWorkbookContract(BomExcelRealSystems.AllTogether(2));

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        public void I14_ConteoIndependiente_CVanosDanCMasUnaCabeceras_SinDuplicarFilas(int bays)
        {
            const int Copies = 2;
            var data = Data(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Drawing(new ConsolidatedRackBom { Name = "S", Kind = "Selectivo", Copies = Copies, Bom = BomExcelRealSystems.Selective(bays) }))));

            var cabecera = Assert.Single(data, r => Col(r, "D").Raw == "Componente" && Col(r, "E").Raw == "Cabecera");
            Assert.Equal(bays + 1, Col(cabecera, "I").Number);
            Assert.Equal((bays + 1) * Copies, Col(cabecera, "J").Number);
        }

        [Fact]
        public void Selectivo_LargueroYSuPerfil_SonFilasDistintasConIgualDescripcionYLongitud()
        {
            var data = Data(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Drawing(BomExcelRealSystems.Rack("Selectivo", 1)))));
            var larguero = data.First(r => Col(r, "D").Raw == "Componente" && Col(r, "E").Raw == "Larguero");

            Assert.Contains(data, r => Col(r, "D").Raw == "Pieza" && Col(r, "G").Raw == Col(larguero, "G").Raw && Col(r, "H").Number == Col(larguero, "H").Number);
        }

        [Fact]
        public void Dinamico_CamaSinPiezas_YPiezasIdenticasNoSeFunden()
        {
            var bom = BomExcelRealSystems.Dynamic();
            var data = Data(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Drawing(new ConsolidatedRackBom { Name = "D", Kind = "Dinámico", Copies = 1, Bom = bom }))));

            Assert.Contains(data, r => Col(r, "D").Raw == "Componente" && Col(r, "E").Raw == RackCad.Application.Systems.Dynamic.SystemBomBuilder.Cama);
            Assert.DoesNotContain(data, r => Col(r, "D").Raw == "Pieza" && Col(r, "E").Raw == RackCad.Application.Systems.Dynamic.SystemBomBuilder.Cama);
            Assert.Contains(bom.Components, c => c.Pieces.GroupBy(p => p.Category + "|" + p.ProfileId + "|" + p.Length).Any(g => g.Count() > 1));
            Assert.Equal(bom.Components.Sum(c => 1 + c.Pieces.Count), data.Count);
        }

        [Fact]
        public void PushBack_CamaSinPiezas_YBotasConLongitudCero()
        {
            var data = Data(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Drawing(BomExcelRealSystems.Rack("Push Back", 1)))));

            Assert.Contains(data, r => Col(r, "D").Raw == "Componente" && Col(r, "E").Raw == "Cama");
            Assert.DoesNotContain(data, r => Col(r, "D").Raw == "Pieza" && Col(r, "E").Raw == "Cama");
            Assert.Contains(data, r => Col(r, "F").Raw == "PROTECTOR_BOTA_H_3_16_18" && Col(r, "H").Number == 0);
        }

        [Fact]
        public void Cantilever_RecetasDistintasConIgualesColumnas_SeSumanEnElResumen()
        {
            var parts = Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Drawing(
                new ConsolidatedRackBom { Name = "C2", Kind = "Cantilever", Copies = 1, Bom = BomExcelRealSystems.Cantilever(2) },
                new ConsolidatedRackBom { Name = "C3", Kind = "Cantilever", Copies = 1, Bom = BomExcelRealSystems.Cantilever(3) })));
            var arms = Data(parts).Where(r => Col(r, "D").Raw == "Componente" && Col(r, "E").Raw == "Brazo").ToList();

            Assert.Equal(2, arms.Count);
            Assert.Equal(Col(arms[0], "G").Raw, Col(arms[1], "G").Raw);
            Assert.Equal(Col(arms[0], "H").Number, Col(arms[1], "H").Number);

            var summary = Rows(parts["xl/worksheets/sheet3.xml"]).Where(r => r.Any(c => c.Column == "A") && r[0].Row >= 5).ToList();
            var group = summary.Single(r => Col(r, "A").Raw == Col(arms[0], "G").Raw);
            Assert.Equal(arms.Sum(r => Col(r, "J").Number), Col(group, "B").Number);
        }

        [Fact]
        public void Cabecera_SoloFilasPieza_ConPlacaDeLongitudCero()
        {
            var data = Data(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Drawing(BomExcelRealSystems.Rack("Cabecera", 1)))));

            Assert.All(data, r => Assert.Equal("Pieza", Col(r, "D").Raw));
            Assert.Contains(data, r => Col(r, "E").Raw == BomBuilder.BasePlate && Col(r, "H").Number == 0);
        }

        [Fact]
        public void Cama_SoloFilasPieza_ConTopeRodilloYFrenoDeLongitudCero()
        {
            var data = Data(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Drawing(BomExcelRealSystems.Rack("Cama", 1)))));

            Assert.All(data, r => Assert.Equal("Pieza", Col(r, "D").Raw));
            Assert.Equal(3, data.Count(r => Col(r, "H").Number == 0));
        }
    }
}
