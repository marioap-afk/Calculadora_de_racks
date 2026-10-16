using System.IO;
using System.Linq;
using System.Text;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T5 — what the command of D-13 answers with the table of the product exactly as it ships, observed with the very
    /// pieces the command composes (the reader of the table, the entry of the station and the package factory), without the
    /// Plugin: with every row verified (F4-D), the model is Valid and the factory gives a package in memory; nothing is published.
    /// </summary>
    public class StructuralAnalysisPluginCommandProductTableTests
    {
        // Windows absolute path string: the factory only checks its syntax and writes nothing, so it is valid on Windows and Linux.
        private const string PackageFolder = @"C:\RackCad-F4D-NoExiste\paquete-estacion-0";

        private static RamSectionMapReadResult ReadProductTable()
        {
            var path = Path.Combine(C.RepoRoot(), "assets", "structural-analysis", "ram-elements", "section-material-map.json");

            return RamSectionMapReader.Read(File.ReadAllText(path, new UTF8Encoding(false)));
        }

        private static CantileverLineDesign Decode(byte[] bytes) =>
            new RackProjectStore().Deserialize(Encoding.UTF8.GetString(bytes))?.CantileverLineDesign;

        [Fact]
        public void LaTablaDeProductoSeLeeYTodasSusFilasEstanVerified()
        {
            var read = ReadProductTable();

            Assert.Equal(StructuralExportOutcome.Valid, read.Outcome);
            Assert.NotNull(read.Table);
            Assert.NotEmpty(read.Table.Rows);
            Assert.All(read.Table.Rows, row => Assert.Equal(RamSectionMapStatus.Verified, row.Status));
        }

        [Fact]
        public void ConLaTablaDeProducto_LaEstacionEsValida_ConModelo_YSinElDiagnosticoDeFilaSinVerificar()
        {
            var table = ReadProductTable().Table;

            var result = CantileverStructuralExport.Build(
                C.RackId, "Cantilever", 0, C.DesignBytes(), Decode, C.LoadsBytes(), table, C.Catalog);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Model);
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == StructuralDiagnosticCodes.MaterialRowUnverified);
        }

        [Fact]
        public void ConLaTablaDeProducto_LaFabricaDaUnPaqueteValidoEnMemoria_SinCrearLaCarpeta()
        {
            var table = ReadProductTable().Table;
            var loads = C.LoadsBytes();
            var folder = PackageFolder;

            var model = CantileverStructuralExport.Build(C.RackId, "cantilever", 0, C.DesignBytes(), Decode, loads, table, C.Catalog);
            var package = RamBuildPackageFactory.CreateRamBuildPackage(
                model, new RamBuildPackageOptions(table, "1.0.0", folder, loads));

            Assert.Equal(StructuralExportOutcome.Valid, package.Outcome);
            Assert.NotNull(package.Package);
            Assert.False(Directory.Exists(folder));
        }

        [Fact]
        public void UnKindDistintoDeCantilever_NoEsElDelSobreCantilever()
        {
            var result = CantileverStructuralExport.Build(
                C.RackId, "selective", 0, C.DesignBytes(), Decode, C.LoadsBytes(), ReadProductTable().Table, C.Catalog);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Model);
        }
    }
}
