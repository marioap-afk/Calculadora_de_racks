using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.Application.StructuralSections;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T1 — A65-F1-01 and A65-F1-02: the provider table is the neutral material profile, every projected row carries the
    /// COMPLETE provider row as length-prefixed text, and a change in any field of a used verified row changes the fingerprint.
    /// </summary>
    public class StructuralAnalysisRamElementsProjectionTests
    {
        private static StructuralSectionId Section(string id) => StructuralSectionId.Parse(id);

        private static string Fingerprint(StructuralModel model) => StructuralFingerprints.AnalysisInput(model, "adapter-test");

        // ---------------------------------------------------------------- the neutral default text (A65-F1-02)

        [Fact]
        public void ElTextoCanonicoNeutralPorDefectoLlevaPrefijosDeLongitud()
        {
            var date = new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc);
            var first = new StructuralMaterialRow(Section(F.ColumnSection), StructuralMaterialId.Create("M|N"), "V", date);
            var second = new StructuralMaterialRow(Section(F.ColumnSection), StructuralMaterialId.Create("M"), "N|V", date);

            // Joined with a bare separator both would read "...|M|N|V|2026-10-11".
            Assert.NotEqual(first.CanonicalText, second.CanonicalText);
            Assert.Contains("3:M|N", first.CanonicalText, StringComparison.Ordinal);
        }

        [Fact]
        public void ElTextoCanonicoNeutralSigueSiendoDeterministaYSinSaltosDeLinea()
        {
            var date = new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc);
            var one = new StructuralMaterialRow(Section(F.ColumnSection), StructuralMaterialId.Create("M"), "V", date).CanonicalText;
            var two = new StructuralMaterialRow(Section(F.ColumnSection), StructuralMaterialId.Create("M"), "V", date).CanonicalText;

            Assert.Equal(one, two);
            Assert.DoesNotContain("\n", one, StringComparison.Ordinal);
            Assert.Contains("2026-10-11", one, StringComparison.Ordinal);
        }

        // ---------------------------------------------------------------- the projection

        [Fact]
        public void LaTablaEsUnPerfilDeMaterialNeutral()
        {
            Assert.IsAssignableFrom<IStructuralMaterialProfile>(F.RefTable());
        }

        [Fact]
        public void UnaFilaVerifiedSeProyectaConSuIdDeMaterialVerificadorYFecha()
        {
            IStructuralMaterialProfile profile = F.RefTable();

            Assert.True(profile.TryGetRow(Section(F.ColumnSection), out var row));
            Assert.True(row.IsVerified);
            Assert.Equal(Section(F.ColumnSection), row.SectionId);
            Assert.Equal(F.ColumnMaterial, row.MaterialId.Value);
            Assert.Equal(F.Verifier, row.Verifier);
            Assert.Equal(new DateTime(2026, 10, 11), row.VerifiedOn.Value.Date);
        }

        [Fact]
        public void UnaFilaProposedSeProyectaSinVerificadorNiFechaYNoAsignaNada()
        {
            var row = F.ColumnRow();
            row["status"] = "proposed";
            row["verifiedBy"] = null;
            row["verifiedOn"] = null;
            IStructuralMaterialProfile profile = F.Table(F.Document(row));

            Assert.True(profile.TryGetRow(Section(F.ColumnSection), out var neutral));
            Assert.False(neutral.IsVerified);
            Assert.Null(neutral.Verifier);
            Assert.Null(neutral.VerifiedOn);
        }

        [Fact]
        public void UnaFilaProposedHaceQueElModeloSeaUnsupportedYNoSeExponeNingunModelo()
        {
            var proposed = F.ArmRow();
            proposed["status"] = "proposed";
            proposed["verifiedBy"] = null;
            proposed["verifiedOn"] = null;
            var table = F.Table(F.Document(F.ColumnRow(), proposed));

            var result = F.BuildRef(table);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.MaterialRowUnverified && d.Subject == F.ArmSection);
            Assert.DoesNotContain(result.Diagnostics, d => d.Subject == F.ColumnSection && d.IsBlocking);
        }

        [Fact]
        public void UnaSeccionSinFilaHaceQueElModeloSeaUnsupported()
        {
            var table = F.Table(F.Document(F.ColumnRow()));

            Assert.False(((IStructuralMaterialProfile)table).TryGetRow(Section(F.ArmSection), out _));

            var result = F.BuildRef(table);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.MaterialRowMissing && d.Subject == F.ArmSection);
        }

        [Fact]
        public void ElModeloUsaElIdDeMaterialDeLaFilaDeCadaSeccion()
        {
            var model = F.ValidRefModel(F.RefTable());

            Assert.All(model.Members.Where(m => m.Role == StructuralMemberRole.Column), m => Assert.Equal(F.ColumnMaterial, m.MaterialRef.Value));
            Assert.All(model.Members.Where(m => m.Role == StructuralMemberRole.Arm), m => Assert.Equal(F.ArmMaterial, m.MaterialRef.Value));
        }

        [Fact]
        public void ElMaterialNoSeInfierePorFamiliaDeLaSeccionNiPorSuGrado()
        {
            // A table with the arm row only: the column section has the same catalogue grade family but no row, so it assigns nothing.
            var table = F.Table(F.Document(F.ArmRow()));
            var result = F.BuildRef(table);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.MaterialRowMissing && d.Subject == F.ColumnSection);
        }

        // ---------------------------------------------------------------- the canonical text carries the whole row

        [Fact]
        public void LaFilaNeutralProyectadaLlevaLaFilaCompletaDelProveedorComoTextoCanonico()
        {
            var table = F.RefTable();
            Assert.True(table.TryGetRamRow(Section(F.ColumnSection), out var provider));
            Assert.True(((IStructuralMaterialProfile)table).TryGetRow(Section(F.ColumnSection), out var neutral));

            Assert.False(string.IsNullOrEmpty(provider.CanonicalText));
            Assert.Contains(provider.CanonicalText, neutral.CanonicalText, StringComparison.Ordinal);

            foreach (var field in new[]
            {
                F.ColumnSection, F.ColumnRamSection, "TEST-FIXTURE-TABLE", "TEST-FIXTURE-COUNTRY", F.ColumnMaterial, F.ColumnRamMaterial,
                F.Source, F.Verifier, "2026-10-11", "ksi", "pcf", "verified",
            })
            {
                Assert.Contains(field, provider.CanonicalText, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ElTextoCanonicoDeLaFilaDelProveedorUsaPrefijosDeLongitud()
        {
            var one = F.ColumnRow();
            one["ramSection"]["table"] = "A|B";
            one["ramSection"]["country"] = "C";
            var two = F.ColumnRow();
            two["ramSection"]["table"] = "A";
            two["ramSection"]["country"] = "B|C";

            var first = F.Table(F.Document(one)).Rows.Single().CanonicalText;
            var second = F.Table(F.Document(two)).Rows.Single().CanonicalText;

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void ElTextoCanonicoEsDeterministaSinSaltosDeLineaYNoDependeDeLaCultura()
        {
            var baseline = F.RefTable().Rows.Select(r => r.CanonicalText).ToArray();
            var original = Thread.CurrentThread.CurrentCulture;

            try
            {
                foreach (var name in new[] { "de-DE", "tr-TR", "fr-FR" })
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(name);
                    Assert.Equal(baseline, F.RefTable().Rows.Select(r => r.CanonicalText).ToArray());
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }

            Assert.All(baseline, text => Assert.DoesNotContain("\n", text, StringComparison.Ordinal));
            Assert.All(baseline, text => Assert.DoesNotContain("\r", text, StringComparison.Ordinal));
            Assert.Contains("111.5", baseline[0], StringComparison.Ordinal);
        }

        // ---------------------------------------------------------------- the fingerprint follows every field of a used verified row

        public static IEnumerable<object[]> FieldChanges() => new[]
        {
            "ramSection.name", "ramSection.table", "ramSection.country", "materialId", "ramMaterial",
            "elasticModulus.value", "elasticModulus.unit", "poissonRatio", "shearModulus", "unitWeight.value", "unitWeight.unit",
            "source", "verifiedBy", "verifiedOn",
        }.Select(name => new object[] { name });

        private static void Change(JsonObject row, string field)
        {
            var expected = (JsonObject)row["expectedMaterial"];

            switch (field)
            {
                case "ramSection.name": row["ramSection"]["name"] = "W 12X27"; break;
                case "ramSection.table": row["ramSection"]["table"] = "TEST-FIXTURE-TABLE-2"; break;
                case "ramSection.country": row["ramSection"]["country"] = "TEST-FIXTURE-COUNTRY-2"; break;
                case "materialId": row["materialId"] = "ASTM-A992-GR65"; break;
                case "ramMaterial": row["ramMaterial"] = "A992 Gr65"; break;
                case "elasticModulus.value": expected["elasticModulus"]["value"] = 111.75; break;
                case "elasticModulus.unit": expected["elasticModulus"]["unit"] = "psi"; break;
                case "poissonRatio": expected["poissonRatio"] = 0.26; break;
                case "shearModulus": expected["shearModulus"] = new JsonObject { ["value"] = 4.5, ["unit"] = "ksi" }; break;
                case "unitWeight.value": expected["unitWeight"]["value"] = 222.75; break;
                case "unitWeight.unit": expected["unitWeight"]["unit"] = "lbf/in3"; break;
                case "source": expected["source"] = "TEST-FIXTURE-SOURCE-2"; break;
                case "verifiedBy": row["verifiedBy"] = "TEST-FIXTURE-2"; break;
                case "verifiedOn": row["verifiedOn"] = "2026-10-12"; break;
                default: throw new ArgumentException(field);
            }
        }

        [Theory]
        [MemberData(nameof(FieldChanges))]
        public void CambiarUnCampoDeUnaFilaVerifiedUsadaCambiaLaHuellaDeEntradaDelAnalisis(string field)
        {
            var baseline = Fingerprint(F.ValidRefModel(F.RefTable()));

            var row = F.ColumnRow();
            Change(row, field);
            var changed = Fingerprint(F.ValidRefModel(F.Table(F.Document(row, F.ArmRow()))));

            Assert.NotEqual(baseline, changed);
        }

        [Fact]
        public void LaHuellaEsLaMismaConLaMismaTabla()
        {
            Assert.Equal(Fingerprint(F.ValidRefModel(F.RefTable())), Fingerprint(F.ValidRefModel(F.RefTable())));
        }

        [Fact]
        public void LaHuellaDeLasFilasUsadasCambiaConLaFilaDelBrazoTambien()
        {
            var baseline = F.ValidRefModel(F.RefTable()).Metadata.MappingRowsSha256;

            var arm = F.ArmRow();
            arm["verifiedBy"] = "TEST-FIXTURE-2";
            var changed = F.ValidRefModel(F.Table(F.Document(F.ColumnRow(), arm))).Metadata.MappingRowsSha256;

            Assert.NotEqual(baseline, changed);
        }
    }
}
