using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using Xunit.Abstractions;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// The inputs of the I-65 F3-T2 tests. The material rows are FIXTURES (see <see cref="StructuralAnalysisRamElementsFixtures"/>): none is an
    /// engineering verification. The folder is a fixed Windows path: nothing is written to it, the content tests only build bytes.
    /// </summary>
    internal static class StructuralAnalysisRamElementsPackageInputs
    {
        public const string Folder = "C:\\RackCadPackages\\REF-CANT-01-S0";
        public const string OtherFolder = "D:\\Other Folder\\REF-CANT-01-S0";
        public const string AdapterVersion = "1.0.0-fixture";

        public static RamBuildPackageOptions Options(
            RamSectionMaterialTable table, string version = AdapterVersion, string folder = Folder, byte[] loads = null) =>
            new RamBuildPackageOptions(table, version, folder, loads ?? C.LoadsBytes());

        public static RamBuildPackageResult Create(
            RamSectionMaterialTable table = null, string version = AdapterVersion, string folder = Folder, byte[] loads = null)
        {
            table = table ?? F.RefTable();
            var model = F.ValidRefModel(table);

            return RamBuildPackageFactory.CreateRamBuildPackage(model, Options(table, version, folder, loads));
        }

        public static RamBuildPackage Package(
            RamSectionMaterialTable table = null, string version = AdapterVersion, string folder = Folder, byte[] loads = null)
        {
            var result = Create(table, version, folder, loads);

            Assert.True(
                result.Outcome == StructuralExportOutcome.Valid && result.Package != null,
                "Se esperaba Valid; resultado " + result.Outcome + ": " + Describe(result));
            return result.Package;
        }

        public static string Describe(RamBuildPackageResult result) => F.Codes(result.Diagnostics);

        public static byte[] Bytes(RamBuildPackage package, string name) => package.Files.Single(f => f.Name == name).GetContent();

        public static string Text(RamBuildPackage package, string name) => new UTF8Encoding(false, true).GetString(Bytes(package, name));

        public static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        /// <summary>The payload of a rendered script: the string literal of its payload line, with its escape undone.</summary>
        public static string ExtractPayload(string script)
        {
            var lines = script.Split('\n').Where(line => line.StartsWith(RamBuildScriptTemplate.PayloadLinePrefix, StringComparison.Ordinal)).ToList();
            Assert.Single(lines);

            var line = lines[0];
            Assert.EndsWith("\"", line);

            var literal = line.Substring(RamBuildScriptTemplate.PayloadLinePrefix.Length, line.Length - RamBuildScriptTemplate.PayloadLinePrefix.Length - 1);
            var text = new StringBuilder();

            for (var index = 0; index < literal.Length; index++)
            {
                if (literal[index] == '\\')
                {
                    index++;
                    Assert.True(index < literal.Length && (literal[index] == '\\' || literal[index] == '"'), "Escape desconocido en el literal.");
                }
                else
                {
                    Assert.NotEqual('"', literal[index]);
                }

                text.Append(literal[index]);
            }

            return text.ToString();
        }

        public static JsonDocument PayloadOf(RamBuildPackage package) =>
            JsonDocument.Parse(ExtractPayload(Text(package, RamBuildPackage.ScriptFileName)));

        /// <summary>The load file of the fixture with the first magnitude changed: another file with the same structure.</summary>
        public static byte[] ChangedLoads()
        {
            var original = Encoding.UTF8.GetString(C.LoadsBytes());
            var changed = original.Replace("\"magnitude\": 1000,", "\"magnitude\": 1001,", StringComparison.Ordinal);

            Assert.NotEqual(original, changed);
            return new UTF8Encoding(false).GetBytes(changed);
        }

        /// <summary>The two rows of REF-CANT-01 as fixtures, with the RAM name of the column section changed.</summary>
        public static RamSectionMaterialTable ChangedTable() =>
            F.Table(F.Document(F.Row(F.ColumnSection, "W 12X27", F.ColumnMaterial, F.ColumnRamMaterial), F.ArmRow()));
    }

    /// <summary>
    /// I-65 F3-T2 — D-09 (content of the package) and CD-I65-F3-08: exactly three files, UTF-8 without BOM, LF, ASCII script, a byte-for-byte
    /// copy of the load file, no clock and no machine, and the same bytes for the same inputs.
    /// </summary>
    public class StructuralAnalysisRamElementsPackageContentTests
    {
        private static readonly string[] Names = { "manifest.json", "rackcad-ram-build.py", "structural-loads.json" };

        private readonly ITestOutputHelper _output;

        public StructuralAnalysisRamElementsPackageContentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void RefCant01ConLaTablaDeFixtureProduceUnPaqueteValidoSinDiagnosticosBloqueantes()
        {
            var result = StructuralAnalysisRamElementsPackageInputs.Create();

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Package);
            Assert.DoesNotContain(result.Diagnostics, d => d.IsBlocking);
        }

        [Fact]
        public void ElPaqueteTieneExactamenteLosTresArchivosEnEseOrden()
        {
            var package = StructuralAnalysisRamElementsPackageInputs.Package();

            Assert.Equal(Names, package.Files.Select(f => f.Name));
            Assert.Equal(Names, new[] { RamBuildPackage.ManifestFileName, RamBuildPackage.ScriptFileName, RamBuildPackage.LoadFileName });
        }

        [Fact]
        public void ElManifiestoYElScriptSonUtf8SinBomYSinRetornosDeCarro()
        {
            var package = StructuralAnalysisRamElementsPackageInputs.Package();

            foreach (var name in new[] { "manifest.json", "rackcad-ram-build.py" })
            {
                var bytes = StructuralAnalysisRamElementsPackageInputs.Bytes(package, name);

                Assert.NotEmpty(bytes);
                Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, name + " lleva BOM");
                Assert.DoesNotContain((byte)'\r', bytes);
                Assert.EndsWith("\n", new UTF8Encoding(false, true).GetString(bytes));
            }
        }

        [Fact]
        public void ElScriptEsSoloAscii()
        {
            var bytes = StructuralAnalysisRamElementsPackageInputs.Bytes(StructuralAnalysisRamElementsPackageInputs.Package(), "rackcad-ram-build.py");

            Assert.All(bytes, b => Assert.True(b < 0x80, "byte no ASCII " + b));
        }

        [Fact]
        public void ElArchivoDeCargasEsUnaCopiaByteAByteYSuSha256EsElDelModelo()
        {
            var package = StructuralAnalysisRamElementsPackageInputs.Package();
            var copy = StructuralAnalysisRamElementsPackageInputs.Bytes(package, "structural-loads.json");

            Assert.Equal(C.LoadsBytes(), copy);
            Assert.Equal(F.ValidRefModel(F.RefTable()).Metadata.LoadFileSha256, StructuralAnalysisRamElementsPackageInputs.Sha256(copy));
        }

        [Fact]
        public void LaCopiaDelArchivoDeCargasNoCambiaAunqueQuienLlamaModifiqueSusBytes()
        {
            var table = F.RefTable();
            var model = F.ValidRefModel(table);
            var loads = C.LoadsBytes();
            var options = StructuralAnalysisRamElementsPackageInputs.Options(table, loads: loads);

            loads[0] = (byte)'#';
            var result = RamBuildPackageFactory.CreateRamBuildPackage(model, options);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Equal(C.LoadsBytes(), StructuralAnalysisRamElementsPackageInputs.Bytes(result.Package, "structural-loads.json"));
        }

        [Fact]
        public void MismoModeloMismasEntradasYMismaCarpetaDanBytesIdenticosEnDosEjecuciones()
        {
            var first = StructuralAnalysisRamElementsPackageInputs.Package();
            var second = StructuralAnalysisRamElementsPackageInputs.Package();

            foreach (var name in Names)
            {
                Assert.Equal(StructuralAnalysisRamElementsPackageInputs.Bytes(first, name), StructuralAnalysisRamElementsPackageInputs.Bytes(second, name));
            }

            Assert.Equal(first.AnalysisInputFingerprint, second.AnalysisInputFingerprint);
        }

        [Fact]
        public void LosBytesNoDependenDelRelojNiDeLaCultura()
        {
            var first = StructuralAnalysisRamElementsPackageInputs.Package();
            var original = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                System.Threading.Thread.Sleep(1100);
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
                var second = StructuralAnalysisRamElementsPackageInputs.Package();

                foreach (var name in Names)
                {
                    Assert.Equal(StructuralAnalysisRamElementsPackageInputs.Bytes(first, name), StructuralAnalysisRamElementsPackageInputs.Bytes(second, name));
                }
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = original;
            }
        }

        [Fact]
        public void NingunArchivoGeneradoLlevaMarcasDeTiempo()
        {
            var package = StructuralAnalysisRamElementsPackageInputs.Package();

            foreach (var name in new[] { "manifest.json", "rackcad-ram-build.py" })
            {
                var text = StructuralAnalysisRamElementsPackageInputs.Text(package, name);

                Assert.DoesNotMatch(new Regex(@"\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}"), text);
                Assert.DoesNotMatch(new Regex(@"\b\d{2}:\d{2}:\d{2}\b"), text);
                Assert.DoesNotMatch(new Regex(@"\d{4}-\d{2}-\d{2}T"), text);
            }
        }

        [Fact]
        public void NingunArchivoGeneradoLlevaNombreDeEquipoNiDeUsuario()
        {
            var package = StructuralAnalysisRamElementsPackageInputs.Package();
            var names = new[] { Environment.MachineName, Environment.UserName }.Where(n => !string.IsNullOrEmpty(n) && n.Length >= 4).ToList();

            foreach (var name in new[] { "manifest.json", "rackcad-ram-build.py" })
            {
                var text = StructuralAnalysisRamElementsPackageInputs.Text(package, name);

                Assert.All(names, n => Assert.DoesNotContain(n, text, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void LaCarpetaEmbebidaEsLaUnicaRutaYCambiarlaCambiaElScriptYElManifiestoPeroNoLaCopiaDeCargas()
        {
            var first = StructuralAnalysisRamElementsPackageInputs.Package();
            var second = StructuralAnalysisRamElementsPackageInputs.Package(folder: StructuralAnalysisRamElementsPackageInputs.OtherFolder);

            Assert.NotEqual(StructuralAnalysisRamElementsPackageInputs.Bytes(first, "rackcad-ram-build.py"), StructuralAnalysisRamElementsPackageInputs.Bytes(second, "rackcad-ram-build.py"));
            Assert.NotEqual(StructuralAnalysisRamElementsPackageInputs.Bytes(first, "manifest.json"), StructuralAnalysisRamElementsPackageInputs.Bytes(second, "manifest.json"));
            Assert.Equal(StructuralAnalysisRamElementsPackageInputs.Bytes(first, "structural-loads.json"), StructuralAnalysisRamElementsPackageInputs.Bytes(second, "structural-loads.json"));
        }

        [Fact]
        public void LaHuellaDelPaqueteEsLaDeEntradaDelModeloConLaVersionDelAdaptador()
        {
            var table = F.RefTable();
            var model = F.ValidRefModel(table);
            var package = StructuralAnalysisRamElementsPackageInputs.Package(table);

            Assert.Equal(StructuralFingerprints.AnalysisInput(model, StructuralAnalysisRamElementsPackageInputs.AdapterVersion), package.AnalysisInputFingerprint);
            Assert.Matches("^[0-9a-f]{64}$", package.AnalysisInputFingerprint);
        }

        [Fact]
        public void LosArchivosDelPaqueteSonCopiasQueNoSeAlteranDesdeFuera()
        {
            var package = StructuralAnalysisRamElementsPackageInputs.Package();
            var copy = package.Files[2].GetContent();

            copy[0] = (byte)'#';

            Assert.Equal(C.LoadsBytes(), package.Files[2].GetContent());
        }

        [Fact]
        public void RegistraLosSha256DeLosTresArchivosDeRefCant01ConLaTablaDeFixtureYLaRutaFija()
        {
            var package = StructuralAnalysisRamElementsPackageInputs.Package();

            _output.WriteLine("FOLDER=" + StructuralAnalysisRamElementsPackageInputs.Folder);
            _output.WriteLine("ADAPTER-VERSION=" + StructuralAnalysisRamElementsPackageInputs.AdapterVersion);

            foreach (var file in package.Files)
            {
                _output.WriteLine("SHA256 " + file.Name + " " + StructuralAnalysisRamElementsPackageInputs.Sha256(file.GetContent()));
            }

            Assert.Equal(3, package.Files.Count);
        }
    }
}
