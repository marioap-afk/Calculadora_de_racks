using System;
using System.IO;
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
        }
    }
}
