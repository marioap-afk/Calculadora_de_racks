using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using Xunit;
using static RackCad.Tests.BomExcelTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-68, I-05 e I-17: lo que la iniciativa NO debe cambiar. Fijado antes del primer cambio del escritor; pasa antes y despues.
    /// </summary>
    public class BomExcelCharacterizationTests
    {
        private static ConsolidatedBom Sample() => Drawing(
            Tree("A", "Selectivo", 2, Comp("Larguero", "L", "Larguero 96", 96, 3, Line("Perfil", "L", "Larguero 96", 96, 1), Line("Ménsula", "M", "Ménsula", 0, 2))),
            Flat("B", "Cama", 1, Line("Rodillo", "R", "Rodillo", 40.5, 10)));

        [Fact]
        public void I05_TotalDelDibujo_ConservaColumnasYValores()
        {
            var rows = Rows(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Sample()))["xl/worksheets/sheet2.xml"]);

            Assert.Equal(new[] { "Nivel", "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad" }, rows[0].Select(c => c.Raw));
            Assert.Equal(
                new[]
                {
                    "Componente|Larguero|L|Larguero 96|96|6",
                    "Pieza|Perfil|L|Larguero 96|96|6",
                    "Pieza|Ménsula|M|Ménsula|0|12",
                    "Componente|Cama||B|0|1",
                    "Pieza|Rodillo|R|Rodillo|40.5|10",
                },
                rows.Skip(1).Select(r => string.Join("|", r.Select(c => c.Raw))));
        }

        [Fact]
        public void I05_TotalDelDibujo_UneComponentesPorCategoriaLongitudYReceta_IgnorandoPerfilYDescripcion()
        {
            // Camas de distinto BFR y de distinto sistema: misma categoria y longitud, sin piezas. Colapsan bajo la primera.
            var rows = Rows(Unzip(ConsolidatedBomXlsxExporter.ToXlsx(Drawing(
                Tree("D", "Dinámico", 2, Comp("Cama", string.Empty, "Cama · BFR 2\"", 96, 1)),
                Tree("P", "Push Back", 1, Comp("Cama", "Cama", "Cama", 96, 3)))))["xl/worksheets/sheet2.xml"]);

            var row = Assert.Single(rows.Skip(1));
            Assert.Equal("Componente||Cama · BFR 2\"|96|5", string.Join("|", new[] { row[0].Raw, row[2].Raw, row[3].Raw, row[4].Raw, row[5].Raw }));
        }

        [Fact]
        public void I17_ConsolidatedCsv_SinCambios()
        {
            var expected = string.Join("\r\n",
                "Rack,Tipo,Copias,Nivel,Categoria,Perfil,Descripcion,Longitud_in,Cantidad",
                "A,Selectivo,2,Componente,Larguero,L,Larguero 96,96,3",
                "A,Selectivo,2,Pieza,Perfil,L,Larguero 96,96,3",
                "A,Selectivo,2,Pieza,Ménsula,M,Ménsula,0,6",
                "B,Cama,1,Pieza,Rodillo,R,Rodillo,40.5,10",
                "TOTAL DEL DIBUJO,,,Componente,Larguero,L,Larguero 96,96,6",
                "TOTAL DEL DIBUJO,,,Pieza,Perfil,L,Larguero 96,96,6",
                "TOTAL DEL DIBUJO,,,Pieza,Ménsula,M,Ménsula,0,12",
                "TOTAL DEL DIBUJO,,,Componente,Cama,,B,0,1",
                "TOTAL DEL DIBUJO,,,Pieza,Rodillo,R,Rodillo,40.5,10",
                string.Empty);

            Assert.Equal(expected, ConsolidatedBomCsvExporter.ToCsv(Sample()));
        }

        [Fact]
        public void I17_CsvDeUnRack_SinCambios()
        {
            var csv = BomCsvExporter.ToCsv(new BillOfMaterials(new List<BomComponent>
            {
                Comp("Larguero", "L", "Larguero 96", 96, 3, Line("Perfil", "L", "Larguero 96", 96, 2))
            }));

            Assert.Equal(
                "Nivel,Categoria,Perfil,Descripcion,Longitud_in,Cantidad\r\nComponente,Larguero,L,Larguero 96,96,3\r\nPieza,Perfil,L,Larguero 96,96,6\r\n",
                csv);
        }

        [Fact]
        public void I17_XlsxDeUnRack_SinTablaNiDinamicaYMismoContenido()
        {
            var parts = Unzip(BomXlsxExporter.ToXlsx(new BillOfMaterials(new List<BomLine> { Line("Poste", "P1", "Poste 132", 132, 4) })));

            Assert.Equal(
                new[] { "[Content_Types].xml", "_rels/.rels", "xl/_rels/workbook.xml.rels", "xl/styles.xml", "xl/workbook.xml", "xl/worksheets/sheet1.xml" },
                parts.Keys.OrderBy(k => k, System.StringComparer.Ordinal));

            static string T(string reference, string text, bool bold) =>
                "<c r=\"" + reference + "\"" + (bold ? " s=\"1\"" : string.Empty) + " t=\"inlineStr\"><is><t xml:space=\"preserve\">" + text + "</t></is></c>";

            var expected = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"" + Ns + "\"><sheetData>"
                + "<row r=\"1\">" + T("A1", "Categoria", true) + T("B1", "Perfil", true) + T("C1", "Descripcion", true) + T("D1", "Longitud (in)", true) + T("E1", "Cantidad", true) + "</row>"
                + "<row r=\"2\">" + T("A2", "Poste", false) + T("B2", "P1", false) + T("C2", "Poste 132", false)
                + "<c r=\"D2\" t=\"n\"><v>132</v></c><c r=\"E2\" t=\"n\"><v>4</v></c></row></sheetData></worksheet>";

            Assert.Equal(expected, parts["xl/worksheets/sheet1.xml"]);
        }
    }
}
