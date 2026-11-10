using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
    /// <summary>I-68 G2: la tabla dinamica nativa de «Resumen» y la validez del paquete.</summary>
    public class BomExcelPivotTests
    {
        private const string Summary = "xl/worksheets/sheet3.xml";
        private const string Pivot = "xl/pivotTables/pivotTable1.xml";
        private const string Cache = "xl/pivotCache/pivotCacheDefinition1.xml";

        private static ConsolidatedBom Sample() => Drawing(
            Tree("R-A", "Selectivo", 3,
                Comp("Larguero", "L1", "Larguero 96", 96, 8, Line("Perfil", "L1", "Larguero 96", 96, 1)),
                Comp("Cabecera", "C", "Cabecera", 144, 2, Line("Poste", "C3", "Poste C3", 144, 2), Line("Poste", "C3", "Poste C3", 9, 1))),
            Tree("R-B", "Selectivo", 2,
                Comp("Poste suelto", "C3", "Poste C3", 10, 1),
                Comp("Poste suelto", "C3", "Poste C3", 100, 1),
                Comp("Poste suelto", "C3", "Poste C3", 144, 5)),
            Flat("R-C", "Cama", 1, Line("Rodillo", "RD", "Rodillo", 40.5, 30)),
            Flat("R-D", "Cama", 2, Line("Riel", "RL", string.Empty, 12, 5)));

        private static Dictionary<string, string> Parts(ConsolidatedBom bom) => Unzip(ConsolidatedBomXlsxExporter.ToXlsx(bom));

        private static double SumOfDetail(Dictionary<string, string> parts)
            => Rows(parts["xl/worksheets/sheet1.xml"]).Skip(1).Sum(r => Col(r, "J").Number);

        /// <summary>El cuerpo de la dinamica ya calculado: desde la fila siguiente al encabezado hasta antes del total.</summary>
        private static List<List<SheetCell>> Body(Dictionary<string, string> parts)
            => Rows(parts[Summary]).Where(r => r.Any(c => c.Column == "A") && r[0].Row >= 5).ToList();

        [Fact]
        public void I08_Resumen_EsTerceraHojaConDinamicaNativaEnlazadaALaTabla()
        {
            var parts = Parts(Sample());

            var workbook = XDocument.Parse(parts["xl/workbook.xml"]).Root;
            Assert.Equal(new[] { "Por rack", "Total del dibujo", "Resumen" }, workbook.Descendants(N("sheet")).Select(s => (string)s.Attribute("name")));
            Assert.Equal("1", (string)workbook.Element(N("pivotCaches")).Element(N("pivotCache")).Attribute("cacheId"));

            var definition = XDocument.Parse(parts[Pivot]).Root;
            Assert.Equal("ResumenBOM", (string)definition.Attribute("name"));
            Assert.Equal("1", (string)definition.Attribute("cacheId"));

            var source = XDocument.Parse(parts[Cache]).Root.Element(N("cacheSource")).Element(N("worksheetSource"));
            Assert.Equal("RackCadBOM", (string)source.Attribute("name"));
            Assert.Null(source.Attribute("ref"));
            Assert.Null(source.Attribute("sheet"));
        }

        [Fact]
        public void I08_Campos_FiltrosRackYNivel_FilasDescripcionLongitud_SumaCantidadTotal()
        {
            var parts = Parts(Sample());
            var definition = XDocument.Parse(parts[Pivot]).Root;
            var fields = definition.Element(N("pivotFields")).Elements(N("pivotField")).ToList();
            var cacheNames = XDocument.Parse(parts[Cache]).Root.Element(N("cacheFields")).Elements(N("cacheField")).Select(f => (string)f.Attribute("name")).ToList();
            int Index(string name) => cacheNames.IndexOf(name);

            Assert.Equal(new[] { "Rack", "Tipo", "Copias", "Nivel", "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad", "Cantidad total" }, cacheNames);
            Assert.Equal(
                new[] { Index("Rack"), Index("Nivel") },
                definition.Element(N("pageFields")).Elements(N("pageField")).Select(f => (int)f.Attribute("fld")));
            Assert.Equal(
                new[] { Index("Descripcion"), Index("Longitud (in)") },
                definition.Element(N("rowFields")).Elements(N("field")).Select(f => (int)f.Attribute("x")));

            var data = Assert.Single(definition.Element(N("dataFields")).Elements(N("dataField")));
            Assert.Equal(Index("Cantidad total"), (int)data.Attribute("fld"));
            Assert.Equal("Suma de Cantidad total", (string)data.Attribute("name"));
            Assert.True(data.Attribute("subtotal") == null || (string)data.Attribute("subtotal") == "sum", "la agregacion debe ser suma");

            Assert.Equal("axisPage", (string)fields[Index("Rack")].Attribute("axis"));
            Assert.Equal("axisPage", (string)fields[Index("Nivel")].Attribute("axis"));
            Assert.Equal("1", (string)fields[Index("Cantidad total")].Attribute("dataField"));
            foreach (var row in new[] { "Descripcion", "Longitud (in)" })
            {
                Assert.Equal("axisRow", (string)fields[Index(row)].Attribute("axis"));
                Assert.Equal("ascending", (string)fields[Index(row)].Attribute("sortType"));
            }
        }

        [Fact]
        public void I08_D3_FiltroNivelAbreSinSeleccion_EnTodas()
        {
            var parts = Parts(Sample());

            Assert.All(XDocument.Parse(parts[Pivot]).Root.Element(N("pageFields")).Elements(N("pageField")), f => Assert.Null(f.Attribute("item")));
            var rows = Rows(parts[Summary]);
            Assert.Equal("Rack", Col(rows[0], "A").Raw);
            Assert.Equal("(Todas)", Col(rows[0], "B").Raw);
            Assert.Equal("Nivel", Col(rows[1], "A").Raw);
            Assert.Equal("(Todas)", Col(rows[1], "B").Raw);
        }

        [Fact]
        public void I09_RefrescoAlAbrir_YCeldasPreviasIgualanElDetalle()
        {
            var parts = Parts(Sample());

            var cache = XDocument.Parse(parts[Cache]).Root;
            Assert.Equal("1", (string)cache.Attribute("refreshOnLoad"));
            Assert.Equal("0", (string)cache.Attribute("saveData"));

            var body = Body(parts);
            var grand = body.Last();
            Assert.Equal("Total general", Col(grand, "A").Raw);
            Assert.Equal(SumOfDetail(parts), Col(grand, "B").Number);

            // Las filas en negrita son los grupos de Descripcion y deben sumar lo mismo que el detalle.
            Assert.Equal(SumOfDetail(parts), body.Take(body.Count - 1).Where(r => Col(r, "A").Bold).Sum(r => Col(r, "B").Number));
        }

        [Fact]
        public void I10_CeldasPrevias_AgrupanYOrdenanNumericamente()
        {
            var body = Body(Parts(Sample()));
            var start = body.FindIndex(r => Col(r, "A").Raw == "Poste C3");
            var lengths = body.Skip(start + 1).TakeWhile(r => Col(r, "A").IsNumber).Select(r => (Col(r, "A").Number, Col(r, "B").Number)).ToList();

            Assert.Equal(new[] { 9d, 10d, 100d, 144d }, lengths.Select(l => l.Item1));
            Assert.Equal(new[] { 6d, 2d, 2d, 12d + 10d }, lengths.Select(l => l.Item2));
        }

        [Fact]
        public void I10_CeldasPrevias_IgnoranMayusculasYRespetanAcentos()
        {
            var body = Body(Parts(Drawing(Flat("R", "Cama", 1, Line("P", "P", "Poste", 10, 1), Line("P", "P", "POSTE", 10, 4), Line("P", "P", "Póste", 10, 2)))));
            var groups = body.Where(r => !Col(r, "A").IsNumber && Col(r, "A").Raw != "Total general").Select(r => (Col(r, "A").Raw, Col(r, "B").Number)).ToList();

            Assert.Equal(new[] { ("Poste", 5d), ("Póste", 2d) }, groups);
        }

        [Fact]
        public void I11_LaDefinicionNoListaValores()
        {
            var enteros = Parts(Drawing(Flat("R-A", "Cama", 1, Line("P", "P", "Poste", 10, 1), Line("P", "P", "POSTE", 20, 4)), Flat("r-a", "Cama", 1, Line("P", "P", "Zeta", 30, 2))));
            var otros = Parts(Drawing(Flat("Otro", "Cama", 7, Line("X", "Y", "Algo distinto", 40.5, 9))));

            Assert.Equal(otros[Cache], enteros[Cache]);
            var cache = XDocument.Parse(enteros[Cache]).Root;
            Assert.All(cache.Descendants(N("sharedItems")), s => Assert.Empty(s.Elements()));
            Assert.Equal("0", (string)cache.Attribute("recordCount"));
            Assert.All(XDocument.Parse(enteros[Pivot]).Root.Descendants(N("item")), i => Assert.Equal("default", (string)i.Attribute("t")));
        }

        [Fact]
        public void I12_BomVacio_DinamicaPresente_ConCeroRacksYConRackSinFilas()
        {
            foreach (var bom in new[] { Drawing(), Drawing(Tree("V", "Selectivo", 2)) })
            {
                var parts = Parts(bom);
                Assert.True(parts.ContainsKey(Pivot), "la dinamica debe seguir presente");
                var grand = Body(parts).Last();
                Assert.Equal("Total general", Col(grand, "A").Raw);
                Assert.Equal(0, Col(grand, "B").Number);
                ValidatePackage(ConsolidatedBomXlsxExporter.ToXlsx(bom));
            }
        }

        [Fact]
        public void I12_RackSinFilas_ConservaLaFilaSinteticaEnTotalDelDibujo()
        {
            var rows = Rows(Parts(Drawing(Tree("V", "Selectivo", 2)))["xl/worksheets/sheet2.xml"]);

            var row = Assert.Single(rows.Skip(1));
            Assert.Equal("Componente|Selectivo||V|0|2", string.Join("|", row.Select(c => c.Raw)));
        }

        [Fact]
        public void I15_MismaEntrada_BytesIdenticos_ConFechasDeEntradaConstantes()
        {
            var first = ConsolidatedBomXlsxExporter.ToXlsx(Sample());
            System.Threading.Thread.Sleep(2100); // la resolucion de fecha del ZIP es de 2 s
            var second = ConsolidatedBomXlsxExporter.ToXlsx(Sample());

            Assert.Equal(first, second);
            using (var archive = new ZipArchive(new MemoryStream(first), ZipArchiveMode.Read))
            {
                Assert.All(archive.Entries, e => Assert.Equal(2000, e.LastWriteTime.Year));
            }
        }

        private static void ValidatePackage(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var document = SpreadsheetDocument.Open(stream, false))
            {
                var errors = new OpenXmlValidator(FileFormatVersions.Office2016).Validate(document).ToList();
                Assert.True(errors.Count == 0, string.Join("\n", errors.Select(e => e.Path?.PartUri + " " + e.Id + " " + e.Description)));
            }
        }

        [Fact]
        public void I18_Paquete_ValidoSegunValidadorIndependiente() => ValidatePackage(ConsolidatedBomXlsxExporter.ToXlsx(Sample()));

        [Fact]
        public void I18_Paquete_XmlBienFormado_RelacionesResueltas_ContentTypesCompletos()
        {
            var parts = Parts(Sample());
            foreach (var part in parts)
            {
                XDocument.Parse(part.Value);
            }

            XName Rel(string name) => XName.Get(name, PackageRelNs);
            foreach (var rels in parts.Where(p => p.Key.EndsWith(".rels", StringComparison.Ordinal)))
            {
                var folder = rels.Key == "_rels/.rels" ? string.Empty : rels.Key.Substring(0, rels.Key.IndexOf("_rels/", StringComparison.Ordinal));
                foreach (var relation in XDocument.Parse(rels.Value).Root.Elements(Rel("Relationship")))
                {
                    var resolved = Path.GetFullPath(Path.Combine("/", folder, (string)relation.Attribute("Target"))).Replace('\\', '/');
                    var key = resolved.Substring(resolved.IndexOf('/') + 1);
                    Assert.True(parts.ContainsKey(key), rels.Key + " apunta a una parte inexistente: " + key);
                }
            }

            var overrides = XDocument.Parse(parts["[Content_Types].xml"]).Root.Elements(XName.Get("Override", "http://schemas.openxmlformats.org/package/2006/content-types"))
                .Select(o => (string)o.Attribute("PartName")).ToList();
            foreach (var key in parts.Keys.Where(k => !k.EndsWith(".rels", StringComparison.Ordinal) && k != "[Content_Types].xml"))
            {
                Assert.Contains("/" + key, overrides);
            }
        }

        [Fact]
        public void I20_AdvertenciaDeD3_VisibleYFueraDeLaDinamica()
        {
            var parts = Parts(Sample());
            var rows = Rows(parts[Summary]);

            Assert.Equal("Atención: con Nivel en (Todas) la suma no representa necesariamente un total físico.", Col(rows[0], "D").Raw);
            Assert.True(Col(rows[0], "D").Bold);
            Assert.Equal("Pueden coexistir componentes y piezas relacionadas; usa el filtro Nivel para verlos por separado.", Col(rows[1], "D").Raw);
            Assert.Equal("Las camas de Dinámico y Push Back solo aparecen como Componente. Si los filtros salen vacíos, usa Datos > Actualizar todo.", Col(rows[2], "D").Raw);

            var location = (string)XDocument.Parse(parts[Pivot]).Root.Element(N("location")).Attribute("ref");
            Assert.StartsWith("A", location);
            Assert.Contains(":B", location);
        }

        [Fact]
        public void G2_Escritor_RechazaDinamicaSobreTablaInexistente()
        {
            var sheets = new[]
            {
                new XlsxSheet("Datos", new IReadOnlyList<XlsxCell>[] { new[] { XlsxCell.Str("A"), XlsxCell.Str("B") } }),
                new XlsxSheet("Resumen", new XlsxPivotDefinition("P", "NoExiste", new[] { "A" }, new[] { "A" }, "B", "Suma", "Filas", "Total", "(Todas)", null))
            };

            Assert.Throws<InvalidOperationException>(() => XlsxWriter.Build(sheets));
        }
    }
}
