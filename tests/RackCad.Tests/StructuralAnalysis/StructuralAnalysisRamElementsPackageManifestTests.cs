using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T2 — D-12 and CD-I65-F3-09: the manifest of provenance (schema <c>rackcad-ram-build-manifest/v1</c>) and the sensitivity of the
    /// fingerprint of the input to the version of the adapter, to a mapping row and to the load file.
    /// </summary>
    public class StructuralAnalysisRamElementsPackageManifestTests
    {
        private const string Warning = "These assumptions are hypotheses for transfer equivalence only; they are not a structural validation.";

        private static JsonElement Manifest(RamBuildPackage package) =>
            JsonDocument.Parse(I.Text(package, RamBuildPackage.ManifestFileName)).RootElement;

        private static RamBuildPackage Ref() => I.Package();

        // ---------------------------------------------------------------- what it says

        [Fact]
        public void ElManifiestoDeclaraSuEsquemaYElAdaptadorConSuVersion()
        {
            var root = Manifest(Ref());

            Assert.Equal("rackcad-ram-build-manifest/v1", root.GetProperty("schema").GetString());
            Assert.Equal("rackcad-ram-elements-adapter", root.GetProperty("adapter").GetProperty("name").GetString());
            Assert.Equal(I.AdapterVersion, root.GetProperty("adapter").GetProperty("version").GetString());
        }

        [Fact]
        public void ElDestinoDeclaraLaVersionDelProductoSinVerificarElPaqueteEnLaVersion010YElParLbIn()
        {
            var target = Manifest(Ref()).GetProperty("target");

            Assert.Equal("26.00.01.015", target.GetProperty("productVersion").GetString());
            Assert.Equal("declared, not verified", target.GetProperty("productVersionStatus").GetString());
            Assert.Equal("0.1.0", target.GetProperty("packageVersion").GetString());
            Assert.Equal("LB_IN", target.GetProperty("units").GetString());
        }

        [Fact]
        public void ElOrigenRepiteElSourceRefYElSourceFingerprintDelModelo()
        {
            var metadata = F.ValidRefModel(F.RefTable()).Metadata;
            var source = Manifest(Ref()).GetProperty("source");

            Assert.Equal(metadata.SourceRef.RackId, source.GetProperty("rackId").GetString());
            Assert.Equal(metadata.SourceRef.Kind, source.GetProperty("kind").GetString());
            Assert.Equal(metadata.SourceRef.StationIndex, source.GetProperty("stationIndex").GetInt32());
            Assert.Equal(metadata.SourceRef.SourceFingerprint, source.GetProperty("sourceFingerprint").GetString());
            Assert.Equal(metadata.GeneratorVersion, source.GetProperty("generatorVersion").GetString());
        }

        [Fact]
        public void LaHuellaDeEntradaDelManifiestoEsLaDelPaqueteYLaDeLaCargaUtil()
        {
            var package = Ref();
            var fingerprint = Manifest(package).GetProperty("analysisInputFingerprint").GetString();

            Assert.Equal(package.AnalysisInputFingerprint, fingerprint);
            Assert.Equal(fingerprint, I.PayloadOf(package).RootElement.GetProperty("analysisInputFingerprint").GetString());
        }

        [Fact]
        public void ElManifiestoNombraElArchivoDeCargasConSuSha256Real()
        {
            var package = Ref();
            var loads = Manifest(package).GetProperty("loadsFile");

            Assert.Equal("structural-loads.json", loads.GetProperty("name").GetString());
            Assert.Equal(I.Sha256(C.LoadsBytes()), loads.GetProperty("sha256").GetString());
            Assert.Equal(I.Sha256(I.Bytes(package, "structural-loads.json")), loads.GetProperty("sha256").GetString());
        }

        [Fact]
        public void LasFilasDeMapeoUsadasVanCompletasYConSuHashDelModelo()
        {
            var table = F.RefTable();
            var model = F.ValidRefModel(table);
            var mapping = Manifest(I.Package(table)).GetProperty("mapping");
            var rows = mapping.GetProperty("rows").EnumerateArray().ToList();

            Assert.Equal(model.Metadata.MappingRowsSha256, mapping.GetProperty("rowsSha256").GetString());
            Assert.Equal(new[] { F.ArmSection, F.ColumnSection }, rows.Select(r => r.GetProperty("sectionId").GetString()));

            var column = rows.Single(r => r.GetProperty("sectionId").GetString() == F.ColumnSection);
            table.TryGetRamRow(RackCad.Application.StructuralSections.StructuralSectionId.Parse(F.ColumnSection), out var expected);

            Assert.Equal(F.ColumnRamSection, column.GetProperty("ramSection").GetProperty("name").GetString());
            Assert.Equal("TEST-FIXTURE-TABLE", column.GetProperty("ramSection").GetProperty("table").GetString());
            Assert.Equal("TEST-FIXTURE-COUNTRY", column.GetProperty("ramSection").GetProperty("country").GetString());
            Assert.Equal(F.ColumnMaterial, column.GetProperty("materialId").GetString());
            Assert.Equal(F.ColumnRamMaterial, column.GetProperty("ramMaterial").GetString());
            Assert.Equal("verified", column.GetProperty("status").GetString());
            Assert.Equal(F.Verifier, column.GetProperty("verifiedBy").GetString());
            Assert.Equal("2026-10-11", column.GetProperty("verifiedOn").GetString());
            Assert.Equal(expected.CanonicalText, column.GetProperty("canonicalText").GetString());

            var material = column.GetProperty("expectedMaterial");

            Assert.Equal(111.5, material.GetProperty("elasticModulus").GetProperty("value").GetDouble());
            Assert.Equal("ksi", material.GetProperty("elasticModulus").GetProperty("unit").GetString());
            Assert.Equal(0.25, material.GetProperty("poissonRatio").GetDouble());
            Assert.Equal(JsonValueKind.Null, material.GetProperty("shearModulus").ValueKind);
            Assert.Equal(222.5, material.GetProperty("unitWeight").GetProperty("value").GetDouble());
            Assert.Equal("pcf", material.GetProperty("unitWeight").GetProperty("unit").GetString());
            Assert.Equal(F.Source, material.GetProperty("source").GetString());
        }

        [Fact]
        public void LosSieteSupuestosVanConSuEnunciadoYLaAdvertenciaLiteral()
        {
            var model = F.ValidRefModel(F.RefTable());
            var assumptions = Manifest(Ref()).GetProperty("assumptions");
            var items = assumptions.GetProperty("items").EnumerateArray().ToList();

            Assert.Equal(7, model.Assumptions.Count);
            Assert.Equal(7, items.Count);
            Assert.Equal(Warning, assumptions.GetProperty("warning").GetString());
            Assert.Equal(model.Metadata.AssumptionsSha256, assumptions.GetProperty("sha256").GetString());

            foreach (var assumption in model.Assumptions)
            {
                var item = items.Single(i => i.GetProperty("id").GetString() == assumption.Id);

                Assert.Equal(assumption.Statement, item.GetProperty("statement").GetString());
            }

            Assert.Equal(model.Assumptions.Select(a => a.Id).OrderBy(i => i, StringComparer.Ordinal), items.Select(i => i.GetProperty("id").GetString()));
        }

        [Fact]
        public void LaAdvertenciaEstaLiteralEnElTextoDelManifiesto()
        {
            Assert.Contains(Warning, I.Text(Ref(), RamBuildPackage.ManifestFileName));
            Assert.Equal(Warning, RamBuildManifest.AssumptionsWarning);
        }

        [Theory]
        [InlineData("base")]
        [InlineData("plates")]
        [InlineData("gussets")]
        [InlineData("spacers")]
        [InlineData("bracing")]
        [InlineData("round bars")]
        [InlineData("self weight")]
        [InlineData("load combinations")]
        [InlineData("nodal loads")]
        [InlineData("distributed loads")]
        [InlineData("local loads")]
        [InlineData("springs")]
        [InlineData("rigid end offsets")]
        [InlineData("releases")]
        public void LasExclusionesDeV1EstanEnElManifiesto(string phrase)
        {
            var exclusions = Manifest(Ref()).GetProperty("exclusions").EnumerateArray().Select(e => e.GetString()).ToList();

            Assert.Contains(exclusions, text => text.Contains(phrase, StringComparison.Ordinal));
        }

        [Fact]
        public void LosArchivosDelPaqueteLlevanSuSha256SalvoElPropioManifiesto()
        {
            var package = Ref();
            var files = Manifest(package).GetProperty("files").EnumerateArray().ToList();

            Assert.Equal(new[] { "rackcad-ram-build.py", "structural-loads.json" }, files.Select(f => f.GetProperty("name").GetString()));

            foreach (var file in files)
            {
                var name = file.GetProperty("name").GetString();

                Assert.Equal(I.Sha256(I.Bytes(package, name)), file.GetProperty("sha256").GetString());
            }
        }

        [Fact]
        public void LaLecturaDeVueltaRepiteLaHuellaDeEntradaYElScriptLaEscribe()
        {
            var package = Ref();
            var readback = Manifest(package).GetProperty("readback");

            Assert.Equal("ram-readback.json", readback.GetProperty("name").GetString());
            Assert.True(readback.GetProperty("repeatsAnalysisInputFingerprint").GetBoolean());
            Assert.Contains("data[\"analysisInputFingerprint\"]", I.Text(package, RamBuildPackage.ScriptFileName));
        }

        [Fact]
        public void LaRutaDeLaCarpetaEsLaUnicaRutaDelManifiesto()
        {
            var text = I.Text(Ref(), RamBuildPackage.ManifestFileName);
            var root = JsonDocument.Parse(text).RootElement;

            Assert.Equal(I.Folder, root.GetProperty("packageFolder").GetString());
            Assert.Equal(1, text.Split(new[] { "C:\\\\" }, StringSplitOptions.None).Length - 1);
        }

        [Fact]
        public void ElManifiestoSeEscribeConSangriaDeDosEspaciosYSaltosLf()
        {
            var text = I.Text(Ref(), RamBuildPackage.ManifestFileName);

            Assert.StartsWith("{\n  \"schema\": \"rackcad-ram-build-manifest/v1\",\n", text);
            Assert.DoesNotContain("\r", text);
            Assert.EndsWith("}\n", text);
        }

        [Fact]
        public void ElManifiestoNoDeclaraNingunaValidacionEstructuralNiResultados()
        {
            var text = I.Text(Ref(), RamBuildPackage.ManifestFileName).ToLowerInvariant();

            Assert.DoesNotContain("validated", text.Replace("not a structural validation", string.Empty));
            Assert.DoesNotContain("verdict", text);
            Assert.DoesNotContain("passed", text);
        }

        // ---------------------------------------------------------------- sensitivity (INV-04, D-12)

        [Fact]
        public void CambiarLaVersionDelAdaptadorCambiaLaHuellaYElManifiestoPeroNoLaCopiaDeCargas()
        {
            var first = Ref();
            var second = I.Package(version: "1.0.1-fixture");

            Assert.NotEqual(first.AnalysisInputFingerprint, second.AnalysisInputFingerprint);
            Assert.NotEqual(I.Bytes(first, "manifest.json"), I.Bytes(second, "manifest.json"));
            Assert.NotEqual(I.Bytes(first, "rackcad-ram-build.py"), I.Bytes(second, "rackcad-ram-build.py"));
            Assert.Equal(I.Bytes(first, "structural-loads.json"), I.Bytes(second, "structural-loads.json"));
        }

        [Fact]
        public void CambiarUnaFilaDeMapeoCambiaElHashDeFilasLaHuellaYElManifiesto()
        {
            var first = Ref();
            var second = I.Package(I.ChangedTable());

            Assert.NotEqual(Manifest(first).GetProperty("mapping").GetProperty("rowsSha256").GetString(), Manifest(second).GetProperty("mapping").GetProperty("rowsSha256").GetString());
            Assert.NotEqual(first.AnalysisInputFingerprint, second.AnalysisInputFingerprint);
            Assert.NotEqual(I.Bytes(first, "manifest.json"), I.Bytes(second, "manifest.json"));
        }

        [Fact]
        public void CambiarElArchivoDeCargasCambiaSuSha256LaHuellaYElManifiesto()
        {
            var changed = I.ChangedLoads();
            var table = F.RefTable();
            var model = C.Build(C.Design(), loadsBytes: changed, profile: table);

            Assert.Equal(StructuralExportOutcome.Valid, model.Outcome);

            var second = RamBuildPackageFactory.CreateRamBuildPackage(model.Model, I.Options(table, loads: changed));
            var first = Ref();

            Assert.Equal(StructuralExportOutcome.Valid, second.Outcome);
            Assert.NotEqual(first.AnalysisInputFingerprint, second.Package.AnalysisInputFingerprint);
            Assert.NotEqual(I.Bytes(first, "manifest.json"), I.Bytes(second.Package, "manifest.json"));
            Assert.NotEqual(
                Manifest(first).GetProperty("loadsFile").GetProperty("sha256").GetString(),
                Manifest(second.Package).GetProperty("loadsFile").GetProperty("sha256").GetString());
            Assert.Equal(changed, I.Bytes(second.Package, "structural-loads.json"));
        }

        [Fact]
        public void LaFilaDeMapeoCambiadaApareceEnElManifiestoYLaOriginalDesaparece()
        {
            var text = I.Text(I.Package(I.ChangedTable()), RamBuildPackage.ManifestFileName);

            Assert.Contains("W 12X27", text);
            Assert.DoesNotContain(F.ColumnRamSection, text);
        }
    }
}
