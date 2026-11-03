using System;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using Xunit;
using F = RackCad.Tests.StructuralAnalysisFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T1 — D-07 and CD-I65-F1-06: the material reaches the model ONLY through the explicit profile of verified
    /// rows. The rows used here are FIXTURES declared as such: they are not an engineering verification.
    /// </summary>
    public class StructuralAnalysisMaterialProfileTests
    {
        private static StructuralSectionId Section(string id) => StructuralSectionId.Parse(id);

        [Fact]
        public void ElProductoNoTrajoFilasEnF1()
        {
            Assert.Empty(StructuralMaterialProfile.Empty.Rows);
            Assert.False(StructuralMaterialProfile.Empty.TryGetRow(Section(F.ColumnSection), out var row));
            Assert.Null(row);
        }

        [Fact]
        public void UnaFilaVerificadaTieneVerificadorYFecha()
        {
            var row = F.Row(F.ColumnSection, "FIXTURE-MATERIAL-COLUMN", "TEST-FIXTURE");

            Assert.True(row.IsVerified);
            Assert.Equal("TEST-FIXTURE", row.Verifier);
            Assert.Equal(F.FixtureDate, row.VerifiedOn);
            Assert.Equal(Section(F.ColumnSection), row.SectionId);
            Assert.Equal("FIXTURE-MATERIAL-COLUMN", row.MaterialId.Value);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("TEST-FIXTURE", false)]
        public void UnaFilaSinVerificadorOSinFechaNoEstaVerificada(string verifier, bool expectUnverified)
        {
            var withDate = new StructuralMaterialRow(Section(F.ColumnSection), StructuralMaterialId.Create("M"), verifier, F.FixtureDate);
            var withoutDate = new StructuralMaterialRow(Section(F.ColumnSection), StructuralMaterialId.Create("M"), "TEST-FIXTURE", null);

            Assert.Equal(!expectUnverified, withDate.IsVerified);
            Assert.False(withoutDate.IsVerified);
        }

        [Fact]
        public void ElTextoCanonicoDeLaFilaEsDeterministaYCambiaConCadaCampo()
        {
            var baseline = F.Row(F.ColumnSection, "FIXTURE-MATERIAL-COLUMN", "TEST-FIXTURE").CanonicalText;

            Assert.Equal(baseline, F.Row(F.ColumnSection, "FIXTURE-MATERIAL-COLUMN", "TEST-FIXTURE").CanonicalText);
            Assert.NotEqual(baseline, F.Row(F.ArmSection, "FIXTURE-MATERIAL-COLUMN", "TEST-FIXTURE").CanonicalText);
            Assert.NotEqual(baseline, F.Row(F.ColumnSection, "FIXTURE-MATERIAL-OTHER", "TEST-FIXTURE").CanonicalText);
            Assert.NotEqual(baseline, F.Row(F.ColumnSection, "FIXTURE-MATERIAL-COLUMN", "OTHER-VERIFIER").CanonicalText);
            Assert.NotEqual(baseline, F.Row(F.ColumnSection, "FIXTURE-MATERIAL-COLUMN", "TEST-FIXTURE", F.FixtureDate.AddDays(1)).CanonicalText);
            Assert.DoesNotContain("\r", baseline, StringComparison.Ordinal);
            Assert.DoesNotContain("\n", baseline, StringComparison.Ordinal);
        }

        [Fact]
        public void ElPerfilBuscaPorIdDeSeccionExactoYRechazaDuplicados()
        {
            var profile = F.Profile();

            Assert.True(profile.TryGetRow(Section(F.ColumnSection), out var row));
            Assert.Equal("FIXTURE-MATERIAL-COLUMN", row.MaterialId.Value);
            Assert.False(profile.TryGetRow(Section(F.UnknownSection), out _));
            Assert.Equal(4, profile.Rows.Count);

            Assert.Throws<ArgumentException>(() => new StructuralMaterialProfile(new[]
            {
                F.Row(F.ColumnSection, "A"), F.Row(F.ColumnSection, "B"),
            }));
        }

        [Fact]
        public void ElIdDeMaterialEsOrdinalYNoAdmiteVacios()
        {
            Assert.False(StructuralMaterialId.TryCreate(null, out _));
            Assert.False(StructuralMaterialId.TryCreate("  ", out _));
            Assert.True(StructuralMaterialId.TryCreate("FIXTURE-MATERIAL", out var id));
            Assert.Equal("FIXTURE-MATERIAL", id.Value);
            Assert.False(id.IsEmpty);
            Assert.True(default(StructuralMaterialId).IsEmpty);
            Assert.NotEqual(StructuralMaterialId.Create("A992"), StructuralMaterialId.Create("a992"));
            Assert.Equal(StructuralMaterialId.Create("A992"), StructuralMaterialId.Create("A992"));
            Assert.Throws<ArgumentException>(() => StructuralMaterialId.Create(""));
        }

        [Fact]
        public void ElMaterialNoSeInfierePorFamiliaDesignacionNiNombreDeLaSeccion()
        {
            // Two sections of the same family, one with a row and one without: only the row decides.
            var profile = new StructuralMaterialProfile(new[] { F.Row(F.ArmSection, "FIXTURE-MATERIAL-ARM"), F.Row(F.ColumnSection, "FIXTURE-MATERIAL-COLUMN") });
            var result = F.Build(
                new StructuralAnalysisFixtureSource { ArmSectionId = F.OtherArmSection },
                profile: profile);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.MaterialRowMissing && d.Subject.Contains(F.OtherArmSection, StringComparison.Ordinal));
        }

        [Fact]
        public void ElMaterialDelModeloEsElDeLaFilaParaCadaSeccionUsada()
        {
            var model = F.ValidModel(profile: F.Profile(columnMaterial: "MAT-C", armMaterial: "MAT-A"));

            Assert.All(model.Members.Where(m => m.Role == StructuralMemberRole.Column), m => Assert.Equal("MAT-C", m.MaterialRef.Value));
            Assert.All(model.Members.Where(m => m.Role == StructuralMemberRole.Arm), m => Assert.Equal("MAT-A", m.MaterialRef.Value));
            Assert.Equal(new[] { "MAT-A", "MAT-C" }, model.Materials.Select(m => m.Id.Value).OrderBy(v => v, StringComparer.Ordinal));
        }
    }
}
