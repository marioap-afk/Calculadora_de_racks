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
    /// Plugin: while every row is proposed, the model is Unsupported and no package exists, so nothing is ever published.
    /// </summary>
    public class StructuralAnalysisPluginCommandProductTableTests
    {
        private static RamSectionMapReadResult ReadProductTable()
        {
            var path = Path.Combine(C.RepoRoot(), "assets", "structural-analysis", "ram-elements", "section-material-map.json");

            return RamSectionMapReader.Read(File.ReadAllText(path, new UTF8Encoding(false)));
        }

        private static CantileverLineDesign Decode(byte[] bytes) =>
            new RackProjectStore().Deserialize(Encoding.UTF8.GetString(bytes))?.CantileverLineDesign;

        [Fact]
        public void LaTablaDeProductoSeLeeYTodasSusFilasSiguenProposed()
        {
            var read = ReadProductTable();

            Assert.Equal(StructuralExportOutcome.Valid, read.Outcome);
            Assert.NotNull(read.Table);
            Assert.NotEmpty(read.Table.Rows);
            Assert.All(read.Table.Rows, row => Assert.Equal(RamSectionMapStatus.Proposed, row.Status));
        }

        [Fact]
        public void ConLaTablaDeProducto_LaEstacionEsUnsupported_ConElDiagnosticoDeFilaSinVerificar_YSinModelo()
        {
            var table = ReadProductTable().Table;

            var result = CantileverStructuralExport.Build(
                C.RackId, "Cantilever", 0, C.DesignBytes(), Decode, C.LoadsBytes(), table, C.Catalog);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == StructuralDiagnosticCodes.MaterialRowUnverified);
        }

        [Fact]
        public void ConLaTablaDeProducto_LaFabricaNoDaPaquete_AsiQueElComandoNoEscribeNada()
        {
            var table = ReadProductTable().Table;
            var loads = C.LoadsBytes();

            var model = CantileverStructuralExport.Build(C.RackId, "cantilever", 0, C.DesignBytes(), Decode, loads, table, C.Catalog);
            var package = RamBuildPackageFactory.CreateRamBuildPackage(
                model, new RamBuildPackageOptions(table, "1.0.0", @"C:\Paquetes\estacion-0", loads));

            Assert.NotEqual(StructuralExportOutcome.Valid, package.Outcome);
            Assert.Null(package.Package);
            Assert.NotEmpty(package.Diagnostics);
            Assert.Contains(package.Diagnostics, diagnostic => diagnostic.IsBlocking);
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
