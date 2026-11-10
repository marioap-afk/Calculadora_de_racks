using System;
using System.IO;
using System.Linq;
using RackCad.Application.Bom;
using Xunit;
using static RackCad.Tests.BomExcelTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-68: escribe los libros que usa el humo en Excel real (docs/automation/evidence/I-68-smoke), con el exportador del
    /// SHA en curso. Solo actua si RACKCAD_I68_SMOKE_DIR esta definida; sin ella no hace nada, y las suites ordinarias no
    /// dependen de Excel.
    /// </summary>
    public class BomExcelSmokeFixtures
    {
        private static ConsolidatedBom SmokeDrawing(int rollers = 30) => Drawing(
            Tree("R-A", "Selectivo", 3,
                Comp("Larguero", "L1", "Larguero 96", 96, 8, Line("Perfil", "L1", "Larguero 96", 96, 1), Line("Ménsula", "M", "Ménsula", 0, 2)),
                Comp("Cabecera", "C", "Cabecera 144", 144, 2, Line("Poste", "C3", "Poste C3", 144, 2), Line("Poste", "C3", "Poste C3", 9, 1))),
            Tree("R-B", "Selectivo", 2,
                Comp("Poste suelto", "C3", "Poste C3", 10, 1),
                Comp("Poste suelto", "C3", "Poste C3", 100, 1),
                Comp("Poste suelto", "C3", "Poste C3", 144, 5)),
            Flat("R-C", "Cama", 1, Line("Rodillo", "RD", "Rodillo", 40.5, rollers)),
            Flat("R-D", "Cama", 2, Line("Riel", "RL", string.Empty, 12, 5)));

        [Fact]
        public void WritesSmokeWorkbooks_WhenRequested()
        {
            var target = Environment.GetEnvironmentVariable("RACKCAD_I68_SMOKE_DIR");
            if (string.IsNullOrWhiteSpace(target))
            {
                return;
            }

            Directory.CreateDirectory(target);
            void Write(string name, ConsolidatedBom bom) => File.WriteAllBytes(Path.Combine(target, name + ".xlsx"), ConsolidatedBomXlsxExporter.ToXlsx(bom));

            Write("smoke-main", SmokeDrawing());
            Write("smoke-empty", Drawing());
            Write("smoke-rack-vacio", Drawing(Tree("V", "Selectivo", 2)));
            for (var i = 1; i <= 6; i++)
            {
                Write("smoke-c" + i, SmokeDrawing(30 + i));
            }

            var systems = BomExcelRealSystems.AllTogether(2);
            Write("smoke-sistemas", systems);

            // Caracterizacion de rendimiento: sin umbrales, solo se registra.
            var report = new System.Text.StringBuilder();
            void Measure(string name, ConsolidatedBom bom)
            {
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var bytes = ConsolidatedBomXlsxExporter.ToXlsx(bom);
                clock.Stop();
                File.WriteAllBytes(Path.Combine(target, name + ".xlsx"), bytes);
                var rows = bom.Racks.Sum(r => r.Bom.IsComponentBased ? r.Bom.Components.Sum(c => 1 + c.Pieces.Count) : r.Bom.Lines.Count);
                report.AppendLine(name + " filas_detalle=" + rows + " generar_ms=" + clock.ElapsedMilliseconds + " bytes=" + bytes.Length);
            }

            Measure("perf-tipico", systems);
            Measure("perf-5000", LargeDrawing(50));
            Measure("perf-20000", LargeDrawing(200));
            File.WriteAllText(Path.Combine(target, "perf-generacion.txt"), report.ToString());
        }

        /// <summary>racks × 25 componentes × (1 + 3 piezas) filas, con 500 descripciones distintas como maximo.</summary>
        private static ConsolidatedBom LargeDrawing(int racks)
        {
            var list = new System.Collections.Generic.List<ConsolidatedRackBom>();
            var description = 0;
            for (var r = 0; r < racks; r++)
            {
                var components = new BomComponent[25];
                for (var c = 0; c < components.Length; c++)
                {
                    var text = "Componente " + (description++ % 500);
                    components[c] = Comp("Cat", "C" + c, text, 10 + c, 2, Line("P1", "A", text + " a", 10 + c, 1), Line("P2", "B", text + " b", 20 + c, 2), Line("P3", "C", text + " c", 30 + c, 3));
                }

                list.Add(Tree("Rack " + r, "Selectivo", 1 + (r % 3), components));
            }

            return ConsolidatedBomBuilder.Build(list);
        }
    }
}
