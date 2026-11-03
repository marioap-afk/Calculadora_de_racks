using System;
using System.Linq;
using System.Text;
using RackCad.Application.StructuralAnalysis;
using Xunit;
using F = RackCad.Tests.StructuralAnalysisFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T1 — INV-04, D-12 and CD-I65-F1-10: the fingerprints are DETERMINISTIC and SENSITIVE. A constant fingerprint
    /// passes every stability test and fails the sensitivity ones.
    /// </summary>
    public class StructuralAnalysisFingerprintTests
    {
        private static readonly byte[] Design = Encoding.UTF8.GetBytes("{\"design\":1}");

        private static string Input(StructuralModel model, string adapter = "adapter/1") =>
            StructuralFingerprints.AnalysisInput(model, adapter);

        [Fact]
        public void ElHashEsSha256EnHexadecimalMinusculaConUnVectorConocido()
        {
            Assert.Equal(
                "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
                StructuralFingerprints.Sha256Hex(Encoding.ASCII.GetBytes("abc")));
            Assert.Equal(
                "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                StructuralFingerprints.Sha256Hex(new byte[0]));
        }

        [Fact]
        public void LaHuellaDeFuenteEsEstableParaLosMismosValoresPrimitivos()
        {
            var first = StructuralFingerprints.Source(Design, "RACK-1", "Cantilever", 0);
            var second = StructuralFingerprints.Source((byte[])Design.Clone(), "RACK-1", "Cantilever", 0);

            Assert.Equal(first, second);
            Assert.Matches("^[0-9a-f]{64}$", first);
        }

        [Fact]
        public void LaHuellaDeFuenteEsSensibleACadaComponente()
        {
            var changedDesign = (byte[])Design.Clone();
            changedDesign[changedDesign.Length - 2] ^= 1;

            var values = new[]
            {
                StructuralFingerprints.Source(Design, "RACK-1", "Cantilever", 0),
                StructuralFingerprints.Source(changedDesign, "RACK-1", "Cantilever", 0),
                StructuralFingerprints.Source(Design, "RACK-2", "Cantilever", 0),
                StructuralFingerprints.Source(Design, "RACK-1", "Selective", 0),
                StructuralFingerprints.Source(Design, "RACK-1", "Cantilever", 1),
            };

            Assert.Equal(values.Length, values.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void LaHuellaDeFuenteNoConfundeLosLimitesEntreCampos()
        {
            Assert.NotEqual(
                StructuralFingerprints.Source(Design, "ab", "c", 0),
                StructuralFingerprints.Source(Design, "a", "bc", 0));
            Assert.Throws<ArgumentNullException>(() => StructuralFingerprints.Source(null, "RACK-1", "Cantilever", 0));
            Assert.Throws<ArgumentNullException>(() => StructuralFingerprints.Source(Design, null, "Cantilever", 0));
            Assert.Throws<ArgumentNullException>(() => StructuralFingerprints.Source(Design, "RACK-1", null, 0));
        }

        [Fact]
        public void LaHuellaDeEntradaDeAnalisisEsEstableParaLasMismasEntradas()
        {
            var first = Input(F.ValidModel());
            var second = Input(F.ValidModel());

            Assert.Equal(first, second);
            Assert.Matches("^[0-9a-f]{64}$", first);
            Assert.Equal(first, Input(F.ValidModel(new StructuralAnalysisFixtureSource { Reverse = true })));
        }

        [Fact]
        public void LaHuellaDeEntradaDeAnalisisEsSensibleACadaComponente()
        {
            var baseline = Input(F.ValidModel());

            var sensitive = new[]
            {
                ("geometria", Input(F.ValidModel(new StructuralAnalysisFixtureSource { ArmLengthInches = 49 }))),
                ("seccion", Input(F.ValidModel(new StructuralAnalysisFixtureSource { ArmSectionId = F.OtherArmSection }))),
                ("material", Input(F.ValidModel(profile: F.Profile(armMaterial: "FIXTURE-MATERIAL-OTHER")))),
                ("supuesto", Input(F.ValidModel(new StructuralAnalysisFixtureSource { SupportStatement = "COL.FOOT is pinned: a different hypothesis." }))),
                ("fila de mapeo", Input(F.ValidModel(profile: F.Profile(verifier: "OTHER-VERIFIER")))),
                ("archivo de cargas", Input(F.ValidModel(loads: F.Read(F.File(F.Load(magnitude: "101")))))),
                ("version del adaptador", Input(F.ValidModel(), "adapter/2")),
                ("diseno de origen", Input(F.ValidModel(scope: F.Scope(design: "{\"design\":2}")))),
            };

            foreach (var (name, value) in sensitive)
            {
                Assert.True(value != baseline, "la huella no cambia al cambiar: " + name);
            }

            Assert.Equal(sensitive.Length, sensitive.Select(s => s.Item2).Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void SoloLaVersionDelAdaptadorCambiaLaHuellaSiElModeloNoCambia()
        {
            var model = F.ValidModel();

            Assert.Equal(Input(model, "adapter/1"), Input(model, "adapter/1"));
            Assert.NotEqual(Input(model, "adapter/1"), Input(model, "adapter/1.1"));
            Assert.Throws<ArgumentException>(() => Input(model, " "));
            Assert.Throws<ArgumentException>(() => Input(model, null));
            Assert.Throws<ArgumentNullException>(() => StructuralFingerprints.AnalysisInput(null, "adapter/1"));
        }

        [Fact]
        public void LaHuellaDeFilasDeMapeoNoDependeDelOrdenYCambiaConElTexto()
        {
            var a = F.Row(F.ColumnSection, "MAT-1");
            var b = F.Row(F.ArmSection, "MAT-2");
            var changed = F.Row(F.ArmSection, "MAT-2", "OTHER");

            Assert.Equal(StructuralFingerprints.MappingRows(new[] { a, b }), StructuralFingerprints.MappingRows(new[] { b, a }));
            Assert.NotEqual(StructuralFingerprints.MappingRows(new[] { a, b }), StructuralFingerprints.MappingRows(new[] { a, changed }));
            Assert.NotEqual(StructuralFingerprints.MappingRows(new[] { a, b }), StructuralFingerprints.MappingRows(new[] { a }));
            Assert.Matches("^[0-9a-f]{64}$", StructuralFingerprints.MappingRows(new StructuralMaterialRow[0]));
        }

        [Fact]
        public void LaHuellaDeSupuestosNoDependeDelOrdenYCambiaConElEnunciado()
        {
            var a = new AnalysisAssumption("SUP-01", "one");
            var b = new AnalysisAssumption("CON-01", "two");
            var changed = new AnalysisAssumption("CON-01", "three");

            Assert.Equal(StructuralFingerprints.Assumptions(new[] { a, b }), StructuralFingerprints.Assumptions(new[] { b, a }));
            Assert.NotEqual(StructuralFingerprints.Assumptions(new[] { a, b }), StructuralFingerprints.Assumptions(new[] { a, changed }));
            Assert.NotEqual(StructuralFingerprints.Assumptions(new[] { a, b }), StructuralFingerprints.Assumptions(new[] { a }));
        }

        [Fact]
        public void LosHashesDeMetadataDelModeloSeRepitenAlRegenerarlo()
        {
            var first = F.ValidModel().Metadata;
            var second = F.ValidModel().Metadata;

            Assert.Equal(first.SourceRef.SourceFingerprint, second.SourceRef.SourceFingerprint);
            Assert.Equal(first.LoadFileSha256, second.LoadFileSha256);
            Assert.Equal(first.MappingRowsSha256, second.MappingRowsSha256);
            Assert.Equal(first.AssumptionsSha256, second.AssumptionsSha256);
            Assert.Equal(first.SourceRef.SourceFingerprint, StructuralFingerprints.Source(F.Scope().GetDesignBytes(), "RACK-1", "Cantilever", 0));
        }
    }
}
