using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T2 — D-09 rule 2 and CD-I65-F3-08: the payload is compact JSON in plain ASCII, embedded in the script as ONE string literal,
    /// and its capabilities are contained in the 207 functions measured in P1.
    /// </summary>
    public class StructuralAnalysisRamElementsPackagePayloadTests
    {
        private const string P1Report = "docs/automation/evidence/I-65-f0/p1-run/p1-report.json";

        private static string RefPayloadText() => I.ExtractPayload(I.Text(I.Package(), RamBuildPackage.ScriptFileName));

        private static JsonElement Root(JsonDocument document) => document.RootElement;

        private static RamBuildPlan Plan(string nodeId, double x)
        {
            return new RamBuildPlan(
                "LB_IN",
                "REF1",
                "OTHER",
                new[]
                {
                    new RamNodePlan(nodeId, new RamVector(x, 0.0, 2.5), new RamRestraints(true, false, false, false, true, true)),
                    new RamNodePlan("N2", new RamVector(x, 1.0, 2.5), default(RamRestraints)),
                },
                new[]
                {
                    new RamMemberPlan(
                        "M1",
                        nodeId,
                        "N2",
                        "W 12X26",
                        "A992 Gr50",
                        -90.0,
                        new RamAxesFrame(new RamVector(0, 1, 0), new RamVector(0, 0, -1), new RamVector(-1, 0, 0), true),
                        new bool[10]),
                },
                new[] { new RamLoadPlan("P1", "M1", "GLOBAL_Y", -1000.0, 50.0) },
                new RamSectionMapRow[0]);
        }

        // ---------------------------------------------------------------- the shape

        [Fact]
        public void LaCargaUtilEsJsonCompactoDeUnaLineaYSoloAscii()
        {
            var text = RefPayloadText();

            Assert.StartsWith("{\"schema\":\"rackcad-ram-build-payload/v1\"", text);
            Assert.DoesNotContain("\n", text);
            Assert.DoesNotContain("\r", text);
            Assert.DoesNotContain("\":  ", text);
            Assert.All(text, c => Assert.True(c >= 0x20 && c <= 0x7E, "caracter no ASCII o de control: " + (int)c));
        }

        [Fact]
        public void LaCargaUtilLlevaEsquemaVersionDelAdaptadorParDeUnidadesRutaYHuella()
        {
            var package = I.Package();
            var root = I.PayloadOf(package).RootElement;

            Assert.Equal("rackcad-ram-build-payload/v1", root.GetProperty("schema").GetString());
            Assert.Equal(I.AdapterVersion, root.GetProperty("adapterVersion").GetString());
            Assert.Equal("LB_IN", root.GetProperty("units").GetString());
            Assert.Equal(I.Folder, root.GetProperty("packageFolder").GetString());
            Assert.Equal(package.AnalysisInputFingerprint, root.GetProperty("analysisInputFingerprint").GetString());
        }

        [Fact]
        public void ElCasoEsRef1ConLaCategoriaOtherComoTexto()
        {
            var root = I.PayloadOf(I.Package()).RootElement;
            var loadCase = root.GetProperty("loadCase");

            Assert.Equal("REF1", loadCase.GetProperty("id").GetString());
            Assert.Equal(JsonValueKind.String, loadCase.GetProperty("category").ValueKind);
            Assert.Equal("OTHER", loadCase.GetProperty("category").GetString());
        }

        [Fact]
        public void LosNudosLlevanIdCoordenadasDeRamYSeisBanderas()
        {
            var nodes = I.PayloadOf(I.Package()).RootElement.GetProperty("nodes");

            Assert.Equal(8, nodes.GetArrayLength());

            foreach (var node in nodes.EnumerateArray())
            {
                Assert.False(string.IsNullOrEmpty(node.GetProperty("id").GetString()));
                Assert.Equal(JsonValueKind.Number, node.GetProperty("x").ValueKind);
                Assert.Equal(JsonValueKind.Number, node.GetProperty("y").ValueKind);
                Assert.Equal(JsonValueKind.Number, node.GetProperty("z").ValueKind);
                Assert.Equal(6, node.GetProperty("restraints").GetArrayLength());
                Assert.All(node.GetProperty("restraints").EnumerateArray(), flag => Assert.True(flag.ValueKind == JsonValueKind.True || flag.ValueKind == JsonValueKind.False));
            }
        }

        [Fact]
        public void LosMiembrosLlevanNudosSeccionYMaterialDeRamNoVaciosGiroYEjesEsperados()
        {
            var members = I.PayloadOf(I.Package()).RootElement.GetProperty("members");

            Assert.Equal(7, members.GetArrayLength());

            foreach (var member in members.EnumerateArray())
            {
                Assert.False(string.IsNullOrEmpty(member.GetProperty("id").GetString()));
                Assert.False(string.IsNullOrEmpty(member.GetProperty("nodeI").GetString()));
                Assert.False(string.IsNullOrEmpty(member.GetProperty("nodeJ").GetString()));
                Assert.False(string.IsNullOrWhiteSpace(member.GetProperty("section").GetString()));
                Assert.False(string.IsNullOrWhiteSpace(member.GetProperty("material").GetString()));
                Assert.Equal(JsonValueKind.Number, member.GetProperty("rotation").ValueKind);

                foreach (var axis in new[] { "l1", "l2", "l3" })
                {
                    Assert.Equal(3, member.GetProperty(axis).GetArrayLength());
                }
            }
        }

        [Fact]
        public void LasCargasLlevanMiembroDireccionGlobalYValorYPorcentaje()
        {
            var loads = I.PayloadOf(I.Package()).RootElement.GetProperty("loads");

            Assert.Equal(3, loads.GetArrayLength());

            foreach (var load in loads.EnumerateArray())
            {
                Assert.False(string.IsNullOrEmpty(load.GetProperty("member").GetString()));
                Assert.Equal("GLOBAL_Y", load.GetProperty("direction").GetString());
                Assert.True(load.GetProperty("value").GetDouble() < 0);
                var percent = load.GetProperty("percent").GetDouble();
                Assert.InRange(percent, 0.0, 100.0);
            }
        }

        [Fact]
        public void LasToleranciasSonLasDeD09Regla9()
        {
            var tolerances = I.PayloadOf(I.Package()).RootElement.GetProperty("tolerances");

            Assert.Equal(1e-9, tolerances.GetProperty("relative").GetDouble());
            Assert.Equal(1e-9, tolerances.GetProperty("percent").GetDouble());
            Assert.Equal(1e-9, tolerances.GetProperty("axes").GetDouble());
        }

        [Fact]
        public void LasCapacidadesPidenLaVersion010YLasFuncionesDelNucleoSinRepetirlasNiDesordenarlas()
        {
            var capabilities = I.PayloadOf(I.Package()).RootElement.GetProperty("capabilities");
            var functions = capabilities.GetProperty("coreFunctions").EnumerateArray().Select(e => e.GetString()).ToList();

            Assert.Equal("0.1.0", capabilities.GetProperty("version").GetString());
            Assert.NotEmpty(functions);
            Assert.Equal(functions.Distinct(StringComparer.Ordinal).Count(), functions.Count);
            Assert.Equal(functions.OrderBy(f => f, StringComparer.Ordinal), functions);
            Assert.Equal(RamBuildPayload.CoreFunctions, functions);
        }

        // ---------------------------------------------------------------- CD-I65-F3-08: the capabilities are among the 207 of P1

        [Fact]
        public void LaListaDeP1TieneLas207FuncionesDelNucleo()
        {
            Assert.Equal(207, P1CoreFunctions().Count);
        }

        [Fact]
        public void LasFuncionesDelNucleoQueExigeLaPuertaEstanContenidasEnLasMedidasEnP1()
        {
            var measured = new HashSet<string>(P1CoreFunctions(), StringComparer.Ordinal);
            var payload = I.PayloadOf(I.Package()).RootElement.GetProperty("capabilities").GetProperty("coreFunctions")
                .EnumerateArray().Select(e => e.GetString()).ToList();

            Assert.All(payload, name => Assert.True(measured.Contains(name), "Fuera de las 207 de P1: " + name));
            Assert.All(RamBuildPayload.CoreFunctions, name => Assert.True(measured.Contains(name), "Fuera de las 207 de P1: " + name));
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

        // ---------------------------------------------------------------- embedded as ONE literal, no base64

        [Fact]
        public void ElScriptIncrustaLaCargaUtilComoUnSoloLiteralDeCadenaYSuEscapeSeDeshaceSinPerdidas()
        {
            var package = I.Package();
            var script = I.Text(package, RamBuildPackage.ScriptFileName);
            var literal = I.ExtractPayload(script);

            Assert.Equal(RamBuildPayload.Create(PlanOf(), I.AdapterVersion, I.Folder, package.AnalysisInputFingerprint), literal);
            Assert.Equal(1, script.Split('\n').Count(line => line.StartsWith(RamBuildScriptTemplate.PayloadLinePrefix, StringComparison.Ordinal)));
            Assert.DoesNotContain(RamBuildScriptTemplate.PayloadMarker, script);
        }

        private static RamBuildPlan PlanOf()
        {
            var table = StructuralAnalysisRamElementsFixtures.RefTable();
            var model = StructuralAnalysisRamElementsFixtures.ValidRefModel(table);
            var result = RamElementsTranslator.Translate(model, table);

            Assert.NotNull(result.Plan);
            return result.Plan;
        }

        [Fact]
        public void LaCargaUtilNoSeCodificaEnBase64()
        {
            var text = RefPayloadText();

            Assert.StartsWith("{", text);
            Assert.DoesNotContain("base64", I.Text(I.Package(), RamBuildPackage.ScriptFileName), StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("a\\b", "a\\\\b")]
        [InlineData("say \"x\"", "say \\\"x\\\"")]
        [InlineData("\\\"", "\\\\\\\"")]
        [InlineData("C:\\Folder\\\"q\"", "C:\\\\Folder\\\\\\\"q\\\"")]
        public void ElEscapeDelLiteralDuplicaLaBarraInvertidaYAnteponeLaBarraALaComilla(string text, string expected)
        {
            Assert.Equal(expected, RamBuildScriptTemplate.Escape(text));
        }

        [Fact]
        public void LaRutaConBarrasSobreviveAlJsonYAlLiteral()
        {
            var package = I.Package(folder: "\\\\server\\share\\a b\\REF 1");
            var text = I.ExtractPayload(I.Text(package, RamBuildPackage.ScriptFileName));

            Assert.Contains(
                new string('\\', 8) + "server" + new string('\\', 4) + "share",
                I.Text(package, RamBuildPackage.ScriptFileName));
            Assert.Equal("\\\\server\\share\\a b\\REF 1", JsonDocument.Parse(text).RootElement.GetProperty("packageFolder").GetString());
        }

        [Fact]
        public void RenderRechazaUnaCargaUtilNoAsciiOConSaltoDeLinea()
        {
            Assert.Throws<ArgumentException>(() => RamBuildScriptTemplate.Render("{\"a\":\"\u00f1\"}"));
            Assert.Throws<ArgumentException>(() => RamBuildScriptTemplate.Render("{\n}"));
            Assert.Throws<ArgumentNullException>(() => RamBuildScriptTemplate.Render(null));
        }

        // ---------------------------------------------------------------- ASCII, numbers and determinism

        [Fact]
        public void UnIdNoAsciiSeEscribeComoSecuenciaUnicodeYVuelveIgualAlDecodificar()
        {
            var text = RamBuildPayload.Create(Plan("N\u00d1\u00fc", 0.5), "1.0.0", I.Folder, "fp");

            Assert.All(text, c => Assert.True(c >= 0x20 && c <= 0x7E));
            Assert.Contains("\\u00d1", text);
            Assert.Equal("N\u00d1\u00fc", JsonDocument.Parse(text).RootElement.GetProperty("nodes")[0].GetProperty("id").GetString());
        }

        [Fact]
        public void UnCaracterFueraDelPlanoBasicoSeEscribeComoParSustituto()
        {
            var text = RamBuildPayload.Create(Plan("N\U0001F600", 0.5), "1.0.0", I.Folder, "fp");

            Assert.All(text, c => Assert.True(c >= 0x20 && c <= 0x7E));
            Assert.Equal("N\U0001F600", JsonDocument.Parse(text).RootElement.GetProperty("nodes")[0].GetProperty("id").GetString());
        }

        [Fact]
        public void UnCeroNegativoSeEscribeComoCero()
        {
            var text = RamBuildPayload.Create(Plan("N1", -0.0), "1.0.0", I.Folder, "fp");

            Assert.DoesNotContain("-0,", text);
            Assert.DoesNotContain("-0}", text);
            Assert.Contains("\"x\":0,", text);
        }

        [Fact]
        public void LosNumerosSeEscribenConElFormatoDeIdaYVueltaDeLaCulturaInvariante()
        {
            var original = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                var invariant = RamBuildPayload.Create(Plan("N1", 1.0 / 3.0), "1.0.0", I.Folder, "fp");
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
                var german = RamBuildPayload.Create(Plan("N1", 1.0 / 3.0), "1.0.0", I.Folder, "fp");

                Assert.Equal(invariant, german);
                Assert.Contains("0.3333333333333333", invariant);
                Assert.Equal(1.0 / 3.0, JsonDocument.Parse(invariant).RootElement.GetProperty("nodes")[0].GetProperty("x").GetDouble());
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = original;
            }
        }

        [Fact]
        public void UnValorNoFinitoNoSeEscribe()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RamBuildPayload.Create(Plan("N1", double.NaN), "1.0.0", I.Folder, "fp"));
            Assert.Throws<ArgumentOutOfRangeException>(() => RamBuildPayload.Create(Plan("N1", double.PositiveInfinity), "1.0.0", I.Folder, "fp"));
        }

        [Fact]
        public void LaMismaEntradaDaLaMismaCargaUtil()
        {
            var first = RamBuildPayload.Create(Plan("N1", 2.25), "1.0.0", I.Folder, "fp");
            var second = RamBuildPayload.Create(Plan("N1", 2.25), "1.0.0", I.Folder, "fp");

            Assert.Equal(first, second);
            Assert.NotEqual(first, RamBuildPayload.Create(Plan("N1", 2.5), "1.0.0", I.Folder, "fp"));
            Assert.NotEqual(first, RamBuildPayload.Create(Plan("N1", 2.25), "1.0.1", I.Folder, "fp"));
        }

        [Fact]
        public void LaCargaUtilSinPlanNoSeConstruye()
        {
            Assert.Throws<ArgumentNullException>(() => RamBuildPayload.Create(null, "1.0.0", I.Folder, "fp"));
        }
    }
}
