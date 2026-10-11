using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T1 — D-08 and CD-I65-F1-04: the aggregate outcome with its precedence
    /// (Unavailable &gt; Invalid &gt; Unsupported &gt; Valid), the diagnostics and the rule that only Valid exposes a model.
    /// </summary>
    public class StructuralAnalysisOutcomeTests
    {
        private static StructuralDiagnostic Blocking(StructuralExportOutcome outcome) =>
            StructuralDiagnostic.Blocking("TEST-CODE", outcome, "subject");

        [Theory]
        [InlineData(StructuralExportOutcome.Valid, StructuralExportOutcome.Valid)]
        [InlineData(StructuralExportOutcome.Unsupported, StructuralExportOutcome.Unsupported)]
        [InlineData(StructuralExportOutcome.Invalid, StructuralExportOutcome.Invalid)]
        [InlineData(StructuralExportOutcome.Unavailable, StructuralExportOutcome.Unavailable)]
        public void UnSoloDiagnosticoBloqueanteImponeSuResultado(StructuralExportOutcome implied, StructuralExportOutcome expected)
        {
            var diagnostics = implied == StructuralExportOutcome.Valid
                ? new StructuralDiagnostic[0]
                : new[] { Blocking(implied) };

            Assert.Equal(expected, StructuralModelResult.Aggregate(diagnostics));
        }

        [Fact]
        public void LaPrecedenciaEsUnavailableInvalidUnsupportedValid()
        {
            var unsupported = Blocking(StructuralExportOutcome.Unsupported);
            var invalid = Blocking(StructuralExportOutcome.Invalid);
            var unavailable = Blocking(StructuralExportOutcome.Unavailable);

            Assert.Equal(StructuralExportOutcome.Invalid, StructuralModelResult.Aggregate(new[] { unsupported, invalid }));
            Assert.Equal(StructuralExportOutcome.Invalid, StructuralModelResult.Aggregate(new[] { invalid, unsupported }));
            Assert.Equal(StructuralExportOutcome.Unavailable, StructuralModelResult.Aggregate(new[] { invalid, unavailable, unsupported }));
            Assert.Equal(StructuralExportOutcome.Unavailable, StructuralModelResult.Aggregate(new[] { unavailable, unsupported }));
            Assert.Equal(StructuralExportOutcome.Unsupported, StructuralModelResult.Aggregate(new[] { unsupported, unsupported }));
        }

        [Fact]
        public void SinDiagnosticosOSoloNoBloqueantesElResultadoEsValid()
        {
            Assert.Equal(StructuralExportOutcome.Valid, StructuralModelResult.Aggregate(new StructuralDiagnostic[0]));
            Assert.Equal(
                StructuralExportOutcome.Valid,
                StructuralModelResult.Aggregate(new[] { StructuralDiagnostic.NonBlocking("TEST-INFO", "subject") }));
        }

        [Fact]
        public void CualquierDiagnosticoBloqueanteDejaElModeloFueraAunqueSeLeEntregue()
        {
            var model = StructuralAnalysisFixtures.ValidModel();

            foreach (var outcome in new[] { StructuralExportOutcome.Unsupported, StructuralExportOutcome.Invalid, StructuralExportOutcome.Unavailable })
            {
                var result = StructuralModelResult.Create(new[] { Blocking(outcome) }, model);

                Assert.Equal(outcome, result.Outcome);
                Assert.Null(result.Model);
                Assert.Single(result.Diagnostics);
            }
        }

        [Fact]
        public void SoloValidExponeElModeloYConservaLosDiagnosticosNoBloqueantes()
        {
            var model = StructuralAnalysisFixtures.ValidModel();
            var result = StructuralModelResult.Create(new[] { StructuralDiagnostic.NonBlocking("TEST-INFO", "subject") }, model);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Same(model, result.Model);
            Assert.Single(result.Diagnostics);
            Assert.Equal(StructuralExportOutcome.Valid, StructuralModelResult.Create(null, model).Outcome);
        }

        [Fact]
        public void UnDiagnosticoLlevaCodigoSeveridadResultadoImplicadoYSujeto()
        {
            var blocking = new StructuralDiagnostic("SECTION-UNKNOWN", StructuralDiagnosticSeverity.Blocking, StructuralExportOutcome.Invalid, "AISC-W-W99");
            var info = StructuralDiagnostic.NonBlocking("SECTION-PROPERTY-ABSENT", "AISC-W-W8X10#Area");

            Assert.Equal("SECTION-UNKNOWN", blocking.Code);
            Assert.Equal(StructuralDiagnosticSeverity.Blocking, blocking.Severity);
            Assert.Equal(StructuralExportOutcome.Invalid, blocking.ImpliedOutcome);
            Assert.Equal("AISC-W-W99", blocking.Subject);
            Assert.True(blocking.IsBlocking);
            Assert.False(info.IsBlocking);
            Assert.Equal(StructuralDiagnosticSeverity.NonBlocking, info.Severity);
            Assert.Equal(StructuralExportOutcome.Valid, info.ImpliedOutcome);
        }

        [Fact]
        public void UnDiagnosticoIncoherenteSeRechaza()
        {
            Assert.Throws<ArgumentException>(() => new StructuralDiagnostic("X", StructuralDiagnosticSeverity.Blocking, StructuralExportOutcome.Valid, "s"));
            Assert.Throws<ArgumentException>(() => new StructuralDiagnostic("X", StructuralDiagnosticSeverity.NonBlocking, StructuralExportOutcome.Invalid, "s"));
            Assert.Throws<ArgumentException>(() => StructuralDiagnostic.Blocking(" ", StructuralExportOutcome.Invalid, "s"));
            Assert.Throws<ArgumentException>(() => StructuralDiagnostic.Blocking("X", StructuralExportOutcome.Invalid, ""));
            Assert.Throws<ArgumentException>(() => StructuralDiagnostic.NonBlocking("", "s"));
        }

        [Fact]
        public void LosCodigosDeDiagnosticoSonEstablesUnicosYConFormaDeCodigo()
        {
            var codes = typeof(StructuralDiagnosticCodes).GetFields()
                .Where(f => f.IsLiteral)
                .Select(f => (string)f.GetRawConstantValue())
                .ToList();

            Assert.True(codes.Count >= 25, "se esperaban al menos 25 códigos, hay " + codes.Count);
            Assert.Equal(codes.Count, codes.Distinct(StringComparer.Ordinal).Count());
            Assert.All(codes, code => Assert.Matches(new Regex("^[A-Z]+(-[A-Z]+)+$"), code));
            Assert.Equal("LOADS-UNIT-MISSING", StructuralDiagnosticCodes.LoadsUnitMissing);
            Assert.Equal("MATERIAL-ROW-MISSING", StructuralDiagnosticCodes.MaterialRowMissing);
            Assert.Equal("SECTION-UNKNOWN", StructuralDiagnosticCodes.SectionUnknown);
        }

        [Fact]
        public void ElResultadoEnumeraLosCuatroValoresEnOrdenDePrecedenciaAscendente()
        {
            var values = Enum.GetValues(typeof(StructuralExportOutcome)).Cast<StructuralExportOutcome>().ToList();

            Assert.Equal(
                new[] { StructuralExportOutcome.Valid, StructuralExportOutcome.Unsupported, StructuralExportOutcome.Invalid, StructuralExportOutcome.Unavailable },
                values.OrderBy(v => (int)v));
            Assert.Equal(4, values.Count);
        }
    }
}
