using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using S = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T3 — D-11: the payload of the dump script (ASCII, one compact line, decoded by the interpreter), the capabilities its gate
    /// demands (all among the 207 functions measured in P1) and the factory that renders the script for a folder that obeys the rule of the
    /// package folder.
    /// </summary>
    public class StructuralAnalysisRamElementsDumpPayloadTests
    {
        private const string P1Report = "docs/automation/evidence/I-65-f0/p1-run/p1-report.json";

        // what the dump script calls on the core, by area; the 6 marked with a star were NOT executed in P1 (read from the installed source)
        private static readonly string[] ExpectedFunctions =
        {
            "lc_count", "lc_get_case_category", "lc_get_id", "lc_get_self_weight", "lc_is_combo", "lc_term_count" /* * */, "lc_term_factor" /* * */,
            "lc_term_lc_index" /* * */, "members_count", "members_get_axial_behavior", "members_get_cardinal_point", "members_get_description",
            "members_get_id", "members_get_local_axes", "members_get_material", "members_get_nodes" /* * */, "members_get_point_load",
            "members_get_point_load_count", "members_get_releases", "members_get_rigid_end_offset", "members_get_rotation",
            "members_get_rotational_stiffness", "members_get_section", "members_get_stiffness_factors", "nodes_count", "nodes_get_coordinates",
            "nodes_get_description" /* * */, "nodes_get_id" /* * */, "nodes_get_restraints", "units_get", "units_set",
        };

        private static JsonDocument PayloadDocument() => JsonDocument.Parse(RamDumpPayload.Create(S.Version, S.Folder));

        // ---------------------------------------------------------------- the payload

        [Fact]
        public void LaCargaUtilLlevaEsquemaVersionCarpetaNombreDelArchivoYCapacidades()
        {
            using (var document = PayloadDocument())
            {
                var root = document.RootElement;

                Assert.Equal(
                    new[] { "adapterVersion", "capabilities", "dumpFileName", "dumpFolder", "schema" },
                    root.EnumerateObject().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
                Assert.Equal("rackcad-ram-dump-payload/v1", root.GetProperty("schema").GetString());
                Assert.Equal(S.Version, root.GetProperty("adapterVersion").GetString());
                Assert.Equal(S.Folder, root.GetProperty("dumpFolder").GetString());
                Assert.Equal("ram-dump.json", root.GetProperty("dumpFileName").GetString());
                Assert.Equal("0.1.0", root.GetProperty("capabilities").GetProperty("version").GetString());
                Assert.Equal(
                    RamDumpPayload.CoreFunctions,
                    root.GetProperty("capabilities").GetProperty("coreFunctions").EnumerateArray().Select(e => e.GetString()).ToList());
            }
        }

        [Fact]
        public void LaCargaUtilEsUnaSolaLineaCompactaDeAsciiYEsDeterminista()
        {
            var payload = RamDumpPayload.Create(S.Version, S.Folder);

            Assert.NotEmpty(payload);
            Assert.All(payload, c => Assert.True(c >= 0x20 && c <= 0x7E, "Caracter fuera de una linea ASCII: " + (int)c));
            Assert.DoesNotContain("\n  ", payload);
            Assert.Equal(payload, RamDumpPayload.Create(S.Version, S.Folder));
            Assert.NotEqual(payload, RamDumpPayload.Create(S.Version, "C:\\RackCadDumps\\Other-Folder"));
        }

        [Fact]
        public void LaCargaUtilEscapaLaCarpetaDeWindowsYSeDecodificaIgual()
        {
            using (var document = PayloadDocument())
            {
                Assert.Contains("\\\\", RamDumpPayload.Create(S.Version, S.Folder));
                Assert.Equal(S.Folder, document.RootElement.GetProperty("dumpFolder").GetString());
            }
        }

        // ---------------------------------------------------------------- the capabilities: contained in the 207 of P1

        [Fact]
        public void LasFuncionesDelNucleoSonLasQueLlamaElScriptEnOrdenOrdinalSinRepetir()
        {
            Assert.NotEmpty(RamDumpPayload.CoreFunctions);
            Assert.Equal(ExpectedFunctions.OrderBy(n => n, StringComparer.Ordinal).ToList(), RamDumpPayload.CoreFunctions);
            Assert.Equal(RamDumpPayload.CoreFunctions.Distinct(StringComparer.Ordinal).Count(), RamDumpPayload.CoreFunctions.Count);
        }

        [Fact]
        public void LasFuncionesDelNucleoQueExigeLaPuertaDelVolcadoEstanContenidasEnLas207DeP1()
        {
            var measured = new HashSet<string>(P1CoreFunctions(), StringComparer.Ordinal);

            Assert.Equal(207, measured.Count);
            Assert.NotEmpty(RamDumpPayload.CoreFunctions);
            Assert.All(RamDumpPayload.CoreFunctions, name => Assert.True(measured.Contains(name), "Fuera de las 207 de P1: " + name));

            using (var document = PayloadDocument())
            {
                var inPayload = document.RootElement.GetProperty("capabilities").GetProperty("coreFunctions").EnumerateArray().Select(e => e.GetString()).ToList();

                Assert.NotEmpty(inPayload);
                Assert.All(inPayload, name => Assert.True(measured.Contains(name), "Fuera de las 207 de P1: " + name));
            }
        }

        [Fact]
        public void ElVolcadoNoExigeLaFuncionDeNingunaEscrituraNiAnalisis()
        {
            Assert.NotEmpty(RamDumpPayload.CoreFunctions);
            Assert.All(
                RamDumpPayload.CoreFunctions.Where(name => name != "units_set"),
                name => Assert.DoesNotMatch(@"_(?:add|set|delete|enable|run|save|new)(?:_|$)|^(?:analysis|design|results|model)_", name));
            Assert.Equal(new[] { "units_set" }, RamDumpPayload.CoreFunctions.Where(name => name.Contains("_set", StringComparison.Ordinal)).ToArray());
        }

        private static List<string> P1CoreFunctions()
        {
            var path = Path.Combine(C.RepoRoot(), P1Report.Replace('/', Path.DirectorySeparatorChar));

            using (var document = JsonDocument.Parse(File.ReadAllText(path)))
            {
                return document.RootElement.GetProperty("steps").GetProperty("E1_environment").GetProperty("observed")
                    .GetProperty("core_functions").EnumerateArray().Select(e => e.GetString()).ToList();
            }
        }

        // ---------------------------------------------------------------- the factory

        [Fact]
        public void LaFabricaDaElScriptRenderizadoDeLaCargaUtilYLaCarpetaEmbebida()
        {
            var result = RamDumpScriptFactory.Create(S.Version, S.Folder);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Diagnostics);
            Assert.NotNull(result.Script);
            Assert.Equal(S.Folder, result.Script.DumpFolder);
            Assert.Equal(RamDumpScriptTemplate.Render(RamDumpPayload.Create(S.Version, S.Folder)), result.Script.Text);
        }

        [Fact]
        public void ElScriptSePublicaComoUnArchivoUtf8SinBomConSuNombreFijo()
        {
            var script = RamDumpScriptFactory.Create(S.Version, S.Folder).Script;
            var file = script.ToPackageFile();

            Assert.Equal("rackcad-ram-dump.py", RamDumpScript.ScriptFileName);
            Assert.Equal(RamDumpScript.ScriptFileName, file.Name);
            Assert.Equal(new UTF8Encoding(false).GetBytes(script.Text), file.GetContent());
        }

        [Theory]
        [InlineData("")]
        [InlineData("relative\\folder")]
        [InlineData("C:/forward/slash")]
        [InlineData("C:\\trailing\\")]
        [InlineData("C:\\dot\\..\\up")]
        [InlineData("C:\\empty\\\\segment")]
        [InlineData("C:\\reserved\\CON")]
        [InlineData("C:\\ends.with.dot.")]
        [InlineData("C:\\ends with space ")]
        [InlineData("\\\\server")]
        public void LaFabricaRechazaUnaCarpetaQueNoCumpleLaReglaDeLaCarpetaDelPaquete(string folder)
        {
            var result = RamDumpScriptFactory.Create(S.Version, folder);

            Assert.False(RamBuildPackageFactory.IsAbsoluteNormalisedFolder(folder));
            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Script);
            Assert.Contains(result.Diagnostics, d => d.Code == RamPackageDiagnosticCodes.FolderInvalid && d.IsBlocking);
        }

        [Theory]
        [InlineData("C:\\RackCadDumps\\One")]
        [InlineData("d:\\lower\\case")]
        [InlineData("\\\\server\\share\\dump")]
        [InlineData("C:\\With Space\\And-Dash_1")]
        public void LaFabricaAceptaExactamenteLasCarpetasQueAceptaLaReglaDelPaquete(string folder)
        {
            var result = RamDumpScriptFactory.Create(S.Version, folder);

            Assert.True(RamBuildPackageFactory.IsAbsoluteNormalisedFolder(folder));
            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Equal(folder, result.Script.DumpFolder);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void LaFabricaRechazaUnaVersionDelAdaptadorEnBlanco(string version)
        {
            var result = RamDumpScriptFactory.Create(version, S.Folder);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Script);
            Assert.Contains(result.Diagnostics, d => d.Code == RamPackageDiagnosticCodes.AdapterVersionInvalid && d.IsBlocking);
        }

        [Fact]
        public void LaFabricaInformaTodosLosHallazgosYNuncaDaUnScriptParcial()
        {
            var result = RamDumpScriptFactory.Create(" ", "relative");

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Script);
            Assert.Contains(result.Diagnostics, d => d.Code == RamPackageDiagnosticCodes.AdapterVersionInvalid);
            Assert.Contains(result.Diagnostics, d => d.Code == RamPackageDiagnosticCodes.FolderInvalid);
        }
    }
}
