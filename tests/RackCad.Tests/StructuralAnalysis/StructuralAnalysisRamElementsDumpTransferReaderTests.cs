using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using Codes = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamDumpDiagnosticCodes;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T3 — the strict readers of <c>rackcad-ram-readback/v1</c> and <c>rackcad-ram-build-manifest/v1</c> (D-11, D-12). They receive TEXT
    /// (no I/O). The read-back is approved ONLY when its status is exactly <c>READBACK OK</c>, with no difference; a failure, whatever its
    /// gate, is never approved. The manifest gives the fingerprint of the input and the version of the adapter. The manifest of the real
    /// package of REF-CANT-01 is the sample, so the reader is proven against what the writer really writes.
    /// </summary>
    public class StructuralAnalysisRamElementsDumpTransferReaderTests
    {
        private const string Fingerprint = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

        // ---------------------------------------------------------------- the read-back

        private static JsonObject Readback(string status = "READBACK OK", params string[] differences) =>
            new JsonObject
            {
                ["schema"] = RamReadbackReader.Schema,
                ["status"] = status,
                ["adapterVersion"] = I.AdapterVersion,
                ["analysisInputFingerprint"] = Fingerprint,
                ["differences"] = new JsonArray(differences.Select(d => (JsonNode)d).ToArray()),
                ["values"] = new JsonObject
                {
                    ["nodes"] = new JsonArray(),
                    ["members"] = new JsonArray(),
                    ["loads"] = new JsonArray(),
                    ["loadCase"] = null,
                },
            };

        private static JsonObject Backup() =>
            new JsonObject
            {
                ["schema"] = RamReadbackReader.Schema,
                ["status"] = RamReadbackReader.StatusFailed,
                ["analysisInputFingerprint"] = Fingerprint,
                ["error"] = "ValueError: Out of range float values are not JSON compliant",
            };

        private static RamReadbackReadResult Read(JsonObject json) => RamReadbackReader.Read(json.ToJsonString());

        [Fact]
        public void LaLecturaDeVueltaConElEstadoReadbackOkYSinDiferenciasEsAprobada()
        {
            var result = Read(Readback());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Diagnostics);
            Assert.NotNull(result.Transfer);
            Assert.True(result.Transfer.Passed);
            Assert.Equal(Fingerprint, result.Transfer.AnalysisInputFingerprint);
            Assert.Equal(I.AdapterVersion, result.Transfer.AdapterVersion);
            Assert.Empty(result.Transfer.Differences);
        }

        [Fact]
        public void LosEstadosYElEsquemaSonLosQueEscribeLaPlantillaDeConstruccion()
        {
            Assert.Equal("rackcad-ram-readback/v1", RamReadbackReader.Schema);
            Assert.Equal("READBACK OK", RamReadbackReader.StatusPassed);
            Assert.Equal("FAILED — DISCARD MODEL", RamReadbackReader.StatusFailed);
            Assert.Contains("PASSED = \"READBACK OK\"", RamBuildScriptTemplate.Text);
            Assert.Contains("FAILED = \"FAILED \\u2014 DISCARD MODEL\"", RamBuildScriptTemplate.Text);
        }

        [Fact]
        public void UnEstadoDeFalloNuncaEsAprobadoYConservaLasDiferencias()
        {
            var result = Read(Readback(RamReadbackReader.StatusFailed, "node N1: coordinates (0.0, 0.0, 1.0)", "member M1: section 'W 8X10'"));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Transfer);
            Assert.False(result.Transfer.Passed);
            Assert.Equal(2, result.Transfer.Differences.Count);
            Assert.Equal(Fingerprint, result.Transfer.AnalysisInputFingerprint);
        }

        [Theory]
        [InlineData("gate: package version is '0.0.1', expected '0.1.0'")]
        [InlineData("gate: core function missing: members_add")]
        [InlineData("gate: the model has 3 nodes")]
        [InlineData("gate: member M1 needs a section")]
        [InlineData("gate: the unit pair of the payload is not LB_IN")]
        public void UnRechazoDeCualquierPuertaNuncaEsAprobado(string difference)
        {
            var result = Read(Readback(RamReadbackReader.StatusFailed, difference));

            Assert.NotNull(result.Transfer);
            Assert.False(result.Transfer.Passed);
            Assert.Equal(new[] { difference }, result.Transfer.Differences.ToArray());
        }

        [Fact]
        public void UnRechazoDePuertaEscritoConElEstadoDeExitoEsInconsistenteYNoSeAprueba()
        {
            var result = Read(Readback("READBACK OK", "gate: the model has 3 nodes"));

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Transfer);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.ReadbackInconsistent && d.IsBlocking);
        }

        [Fact]
        public void ElRespaldoMinimoDeUnFalloSeLeeComoNoAprobadoConSuError()
        {
            var result = Read(Backup());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Transfer);
            Assert.False(result.Transfer.Passed);
            Assert.Equal(Fingerprint, result.Transfer.AnalysisInputFingerprint);
            Assert.Null(result.Transfer.AdapterVersion);
            Assert.Contains(result.Transfer.Differences, d => d.Contains("Out of range float values", StringComparison.Ordinal));
        }

        [Fact]
        public void UnaLecturaDeVueltaFallidaSinHuellaSeLeeComoNoAprobada()
        {
            var json = Readback(RamReadbackReader.StatusFailed, "exception: KeyError: 'nodes'");
            json["adapterVersion"] = null;
            json["analysisInputFingerprint"] = null;

            var result = Read(json);

            Assert.NotNull(result.Transfer);
            Assert.False(result.Transfer.Passed);
            Assert.Null(result.Transfer.AnalysisInputFingerprint);
        }

        [Theory]
        [InlineData("READBACK OK ")]
        [InlineData("readback ok")]
        [InlineData("OK")]
        [InlineData("FAILED")]
        [InlineData("")]
        public void CualquierEstadoDesconocidoEsInvalidoYNoSeAprueba(string status)
        {
            var result = Read(Readback(status));

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Transfer);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.ReadbackStatusUnknown && d.IsBlocking);
        }

        private static readonly Dictionary<string, (Func<JsonObject, string> Mutate, string Code)> ReadbackMutations = BuildReadbackMutations();

        public static IEnumerable<object[]> ReadbackVariants() => ReadbackMutations.Keys.Select(name => new object[] { name });

        private static (Func<JsonObject, string> Mutate, string Code) Json(Action<JsonObject> change, string code) =>
            (json => { change(json); return json.ToJsonString(); }, code);

        private static (Func<JsonObject, string> Mutate, string Code) Text(Func<string, string> change, string code) =>
            (json => change(json.ToJsonString()), code);

        private static Dictionary<string, (Func<JsonObject, string> Mutate, string Code)> BuildReadbackMutations()
        {
            var all = new Dictionary<string, (Func<JsonObject, string>, string)>(StringComparer.Ordinal);

            all["propiedad desconocida"] = Json(j => j["extra"] = 1, Codes.ReadbackUnknownProperty);
            all["propiedad duplicada"] = Text(t => "{\"status\":\"READBACK OK\"," + t.Substring(1), Codes.ReadbackDuplicateProperty);
            all["sin estado"] = Json(j => j.Remove("status"), Codes.ReadbackMissingProperty);
            all["sin huella"] = Json(j => j.Remove("analysisInputFingerprint"), Codes.ReadbackMissingProperty);
            all["sin diferencias"] = Json(j => j.Remove("differences"), Codes.ReadbackMissingProperty);
            all["sin valores"] = Json(j => j.Remove("values"), Codes.ReadbackMissingProperty);
            all["sin version del adaptador"] = Json(j => j.Remove("adapterVersion"), Codes.ReadbackMissingProperty);
            all["otro esquema"] = Json(j => j["schema"] = "rackcad-ram-readback/v2", Codes.ReadbackSchemaMismatch);
            all["esquema numerico"] = Json(j => j["schema"] = 1, Codes.ReadbackWrongType);
            all["estado numerico"] = Json(j => j["status"] = 1, Codes.ReadbackWrongType);
            all["diferencias como texto"] = Json(j => j["differences"] = "none", Codes.ReadbackWrongType);
            all["diferencia numerica"] = Json(j => j["differences"] = new JsonArray(1), Codes.ReadbackWrongType);
            all["valores como lista"] = Json(j => j["values"] = new JsonArray(), Codes.ReadbackWrongType);
            all["version numerica"] = Json(j => j["adapterVersion"] = 1, Codes.ReadbackWrongType);
            all["huella numerica"] = Json(j => j["analysisInputFingerprint"] = 1, Codes.ReadbackWrongType);
            all["huella que no es sha 256"] = Json(j => j["analysisInputFingerprint"] = "abc", Codes.ReadbackValueInvalid);
            all["huella en mayusculas"] = Json(j => j["analysisInputFingerprint"] = Fingerprint.ToUpperInvariant(), Codes.ReadbackValueInvalid);
            all["version del adaptador en blanco"] = Json(j => j["adapterVersion"] = " ", Codes.ReadbackValueInvalid);
            all["raiz que es una lista"] = Text(t => "[" + t + "]", Codes.ReadbackWrongType);
            all["texto vacio"] = Text(t => string.Empty, Codes.ReadbackJsonMalformed);
            all["texto que no es json"] = Text(t => "READBACK OK", Codes.ReadbackJsonMalformed);
            all["json truncado"] = Text(t => t.Substring(0, t.Length / 2), Codes.ReadbackJsonMalformed);

            return all.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }

        [Theory]
        [MemberData(nameof(ReadbackVariants))]
        public void ElLectorDeLaLecturaDeVueltaEsEstrictoYNuncaApruebaUnaVariante(string name)
        {
            var (mutate, code) = ReadbackMutations[name];
            var result = RamReadbackReader.Read(mutate(Readback()));

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Transfer);
            Assert.Contains(result.Diagnostics, d => d.Code == code && d.IsBlocking);
        }

        [Fact]
        public void UnRespaldoMinimoConUnaPropiedadDesconocidaEsInvalido()
        {
            var backup = Backup();
            backup["extra"] = 1;

            var result = Read(backup);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Transfer);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.ReadbackUnknownProperty);
        }

        [Fact]
        public void LaLecturaDeVueltaSoloEsAprobadaConLaFormaCompletaYElEstadoExacto()
        {
            // the only approved shape: full, READBACK OK, no differences; every other combination of the sample is not approved
            var approved = new List<string>();

            foreach (var json in new[]
            {
                Readback(), Readback(RamReadbackReader.StatusFailed), Readback(RamReadbackReader.StatusFailed, "x"), Backup(),
            })
            {
                var result = Read(json);

                if (result.Transfer != null && result.Transfer.Passed)
                {
                    approved.Add(json.ToJsonString());
                }
            }

            Assert.Single(approved);
            Assert.Equal(Readback().ToJsonString(), approved[0]);
        }

        [Fact]
        public void ElLectorNoLanzaConEntradasNulasOHostiles()
        {
            foreach (var text in new[] { null, string.Empty, "null", "{}", "[]", "1", new string('[', 5000) })
            {
                var result = RamReadbackReader.Read(text);

                Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
                Assert.Null(result.Transfer);
                Assert.NotEmpty(result.Diagnostics);
            }
        }

        // ---------------------------------------------------------------- the manifest

        private static string ManifestText() => I.Text(I.Package(), RamBuildPackage.ManifestFileName);

        private static JsonObject ManifestJson() => (JsonObject)JsonNode.Parse(ManifestText());

        [Fact]
        public void ElManifiestoRealDelPaqueteDaLaHuellaDeEntradaYLaVersionDelAdaptador()
        {
            var package = I.Package();
            var result = RamBuildManifestReader.Read(ManifestText());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Diagnostics);
            Assert.NotNull(result.Provenance);
            Assert.Equal(package.AnalysisInputFingerprint, result.Provenance.AnalysisInputFingerprint);
            Assert.Equal(I.AdapterVersion, result.Provenance.AdapterVersion);
        }

        [Fact]
        public void ElManifiestoDeOtraVersionDelAdaptadorDaOtraVersionYOtraHuella()
        {
            var other = I.Package(version: "2.0.0-fixture");
            var result = RamBuildManifestReader.Read(I.Text(other, RamBuildPackage.ManifestFileName));

            Assert.Equal("2.0.0-fixture", result.Provenance.AdapterVersion);
            Assert.Equal(other.AnalysisInputFingerprint, result.Provenance.AnalysisInputFingerprint);
            Assert.NotEqual(I.Package().AnalysisInputFingerprint, result.Provenance.AnalysisInputFingerprint);
        }

        [Fact]
        public void ElManifiestoGeneradoYLaLecturaDeVueltaDeUnaMismaConstruccionComparanLaMismaHuella()
        {
            var package = I.Package();
            var manifest = RamBuildManifestReader.Read(I.Text(package, RamBuildPackage.ManifestFileName)).Provenance;
            var json = Readback();
            json["analysisInputFingerprint"] = package.AnalysisInputFingerprint;
            json["adapterVersion"] = I.AdapterVersion;
            var transfer = Read(json).Transfer;

            Assert.True(transfer.Passed);
            Assert.Equal(manifest.AnalysisInputFingerprint, transfer.AnalysisInputFingerprint);
            Assert.Equal(manifest.AdapterVersion, transfer.AdapterVersion);
        }

        private static readonly Dictionary<string, (Func<JsonObject, string> Mutate, string Code)> ManifestMutations = BuildManifestMutations();

        public static IEnumerable<object[]> ManifestVariants() => ManifestMutations.Keys.Select(name => new object[] { name });

        private static Dictionary<string, (Func<JsonObject, string> Mutate, string Code)> BuildManifestMutations()
        {
            var all = new Dictionary<string, (Func<JsonObject, string>, string)>(StringComparer.Ordinal);

            all["propiedad desconocida en la raiz"] = Json(j => j["extra"] = 1, Codes.ManifestUnknownProperty);
            all["propiedad desconocida en el adaptador"] = Json(j => j["adapter"]["extra"] = 1, Codes.ManifestUnknownProperty);
            all["propiedad desconocida en el destino"] = Json(j => j["target"]["extra"] = 1, Codes.ManifestUnknownProperty);
            all["propiedad desconocida en la fuente"] = Json(j => j["source"]["extra"] = 1, Codes.ManifestUnknownProperty);
            all["propiedad desconocida en las cargas"] = Json(j => j["loadsFile"]["extra"] = 1, Codes.ManifestUnknownProperty);
            all["propiedad desconocida en un archivo"] = Json(j => ((JsonArray)j["files"])[0]["extra"] = 1, Codes.ManifestUnknownProperty);
            all["propiedad desconocida en la lectura de vuelta"] = Json(j => j["readback"]["extra"] = 1, Codes.ManifestUnknownProperty);
            all["propiedad duplicada en la raiz"] = Text(t => "{\"schema\":\"x\"," + t.Substring(1), Codes.ManifestDuplicateProperty);
            all["propiedad duplicada en el adaptador"] = Text(t => t.Replace("\"name\":\"rackcad-ram-elements-adapter\"", "\"name\":\"rackcad-ram-elements-adapter\",\"name\":\"x\"", StringComparison.Ordinal), Codes.ManifestDuplicateProperty);
            all["sin adaptador"] = Json(j => j.Remove("adapter"), Codes.ManifestMissingProperty);
            all["sin huella de entrada"] = Json(j => j.Remove("analysisInputFingerprint"), Codes.ManifestMissingProperty);
            all["sin version del adaptador"] = Json(j => ((JsonObject)j["adapter"]).Remove("version"), Codes.ManifestMissingProperty);
            all["sin archivos"] = Json(j => j.Remove("files"), Codes.ManifestMissingProperty);
            all["otro esquema"] = Json(j => j["schema"] = "rackcad-ram-build-manifest/v2", Codes.ManifestSchemaMismatch);
            all["esquema numerico"] = Json(j => j["schema"] = 1, Codes.ManifestWrongType);
            all["adaptador como texto"] = Json(j => j["adapter"] = "x", Codes.ManifestWrongType);
            all["version numerica"] = Json(j => j["adapter"]["version"] = 1, Codes.ManifestWrongType);
            all["huella numerica"] = Json(j => j["analysisInputFingerprint"] = 1, Codes.ManifestWrongType);
            all["archivos como objeto"] = Json(j => j["files"] = new JsonObject(), Codes.ManifestWrongType);
            all["archivo como texto"] = Json(j => j["files"] = new JsonArray("x"), Codes.ManifestWrongType);
            all["exclusiones como texto"] = Json(j => j["exclusions"] = "x", Codes.ManifestWrongType);
            all["raiz que es una lista"] = Text(t => "[" + t + "]", Codes.ManifestWrongType);
            all["huella que no es sha 256"] = Json(j => j["analysisInputFingerprint"] = "abc", Codes.ManifestValueInvalid);
            all["huella en mayusculas"] = Json(j => j["analysisInputFingerprint"] = ((string)j["analysisInputFingerprint"]).ToUpperInvariant(), Codes.ManifestValueInvalid);
            all["version del adaptador en blanco"] = Json(j => j["adapter"]["version"] = "  ", Codes.ManifestValueInvalid);
            all["texto vacio"] = Text(t => string.Empty, Codes.ManifestJsonMalformed);
            all["texto que no es json"] = Text(t => "not json", Codes.ManifestJsonMalformed);
            all["json truncado"] = Text(t => t.Substring(0, t.Length / 2), Codes.ManifestJsonMalformed);

            return all.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }

        [Theory]
        [MemberData(nameof(ManifestVariants))]
        public void ElLectorDelManifiestoEsEstrictoYNuncaDaProcedenciaDeUnaVariante(string name)
        {
            var (mutate, code) = ManifestMutations[name];
            var result = RamBuildManifestReader.Read(mutate(ManifestJson()));

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Provenance);
            Assert.Contains(result.Diagnostics, d => d.Code == code && d.IsBlocking);
        }

        [Fact]
        public void ElLectorDelManifiestoNoLanzaConEntradasNulasOHostiles()
        {
            foreach (var text in new[] { null, string.Empty, "null", "{}", "[]", "1", new string('[', 5000) })
            {
                var result = RamBuildManifestReader.Read(text);

                Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
                Assert.Null(result.Provenance);
                Assert.NotEmpty(result.Diagnostics);
            }
        }
    }
}
