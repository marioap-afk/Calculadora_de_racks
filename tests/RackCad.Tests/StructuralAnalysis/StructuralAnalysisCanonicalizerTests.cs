using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using RackCad.Application.StructuralAnalysis;
using Xunit;
using F = RackCad.Tests.StructuralAnalysisFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T1 — D-12 and CD-I65-F1-10: the canonical form of the model is deterministic (ordinal order by id, invariant
    /// culture, round-trip doubles, no timestamps, UTF-8 without BOM, LF).
    /// </summary>
    public class StructuralAnalysisCanonicalizerTests
    {
        [Fact]
        public void ElMismoDisenoYLasMismasEntradasDanElMismoJson()
        {
            var first = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel());
            var second = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel());

            Assert.Equal(first, second);
            Assert.NotEmpty(first);
        }

        [Fact]
        public void LosBytesSonUtf8SinBomYConSaltosLf()
        {
            var model = F.ValidModel();
            var bytes = StructuralModelCanonicalizer.ToCanonicalBytes(model);
            var text = Encoding.UTF8.GetString(bytes);

            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, "el JSON canónico no lleva BOM");
            Assert.DoesNotContain("\r", text, StringComparison.Ordinal);
            Assert.Contains("\n", text, StringComparison.Ordinal);
            Assert.Equal(StructuralModelCanonicalizer.ToCanonicalJson(model), text);
            Assert.Equal(bytes, Encoding.UTF8.GetBytes(text));
        }

        [Fact]
        public void ElOrdenDeEntradaNoCambiaLaFormaCanonica_OrdenOrdinalPorId()
        {
            var forward = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel(new StructuralAnalysisFixtureSource { Reverse = false }));
            var reversed = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel(new StructuralAnalysisFixtureSource { Reverse = true }));

            Assert.Equal(forward, reversed);

            Assert.True(forward.IndexOf("\"ARM.1.PositiveY.J\"", StringComparison.Ordinal) < forward.IndexOf("\"COL.FOOT\"", StringComparison.Ordinal),
                "ARM.* debe preceder a COL.* en orden ordinal");
        }

        [Theory]
        [InlineData("es-MX")]
        [InlineData("de-DE")]
        [InlineData("fr-FR")]
        [InlineData("tr-TR")]
        public void LaCulturaDelHiloNoCambiaLaFormaCanonica(string culture)
        {
            var model = F.ValidModel(new StructuralAnalysisFixtureSource { ArmLengthInches = 47.625 });
            var expected = StructuralModelCanonicalizer.ToCanonicalJson(model);
            var original = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Assert.Equal(expected, StructuralModelCanonicalizer.ToCanonicalJson(model));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }

            Assert.Contains("47.625", expected, StringComparison.Ordinal);
        }

        [Fact]
        public void LosDoblesVanEnFormatoDeIdaYVueltaYElCeroNegativoSeNormaliza()
        {
            var arm = 0.1 + 0.2;
            var model = F.ValidModel(new StructuralAnalysisFixtureSource { ArmLengthInches = arm, NegativeZeroColumnX = true });
            var json = StructuralModelCanonicalizer.ToCanonicalJson(model);

            Assert.Contains("0.30000000000000004", json, StringComparison.Ordinal);
            Assert.DoesNotMatch(new Regex(@"(?<![\w.])-0(?![.\d])"), json);
            Assert.Equal(arm, double.Parse("0.30000000000000004", CultureInfo.InvariantCulture));
        }

        [Fact]
        public void NoHayMarcasDeTiempoEnLaFormaCanonica()
        {
            var json = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel());

            Assert.DoesNotMatch(new Regex(@"\d{4}-\d{2}-\d{2}"), json);
            Assert.DoesNotMatch(new Regex(@"\d{2}:\d{2}:\d{2}"), json);
        }

        [Fact]
        public void LaFormaCanonicaEsJsonValidoConElCasoREF1LasPartesYLasPropiedadesAusentesComoNull()
        {
            var source = new StructuralAnalysisFixtureSource { ColumnSectionId = F.BareSection };
            var json = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel(source));

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            foreach (var part in new[] { "metadata", "loadCase", "nodes", "members", "sections", "materials", "supports", "loads", "assumptions", "diagnostics" })
            {
                Assert.True(root.TryGetProperty(part, out _), "falta la parte '" + part + "'");
            }

            Assert.Equal("REF1", root.GetProperty("loadCase").GetProperty("id").GetString());
            var bare = root.GetProperty("sections").EnumerateArray().Single(s => s.GetProperty("id").GetString() == F.BareSection);
            Assert.Equal(JsonValueKind.Null, bare.GetProperty("area").ValueKind);
            Assert.Equal(JsonValueKind.Null, bare.GetProperty("j").ValueKind);
            Assert.Equal(JsonValueKind.Number, bare.GetProperty("iy").ValueKind);
        }

        [Fact]
        public void LaFormaCanonicaCambiaCuandoCambiaLaGeometriaPeroNoCuandoSoloCambiaElOrden()
        {
            var baseline = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel());
            var moved = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel(new StructuralAnalysisFixtureSource { ArmLengthInches = 49 }));

            Assert.NotEqual(baseline, moved);
        }

        [Fact]
        public void LaFormaCanonicaNoContieneTextoQueAfirmeValidacionEstructural()
        {
            var json = StructuralModelCanonicalizer.ToCanonicalJson(F.ValidModel());

            foreach (var word in new[] { "validated", "certified", "approved" })
            {
                Assert.DoesNotContain(word, json, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
