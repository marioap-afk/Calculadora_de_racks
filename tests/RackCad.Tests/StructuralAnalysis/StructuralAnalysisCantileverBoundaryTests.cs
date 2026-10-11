using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Units;
using RackCad.Domain.Systems.Cantilever;
using Xunit;
using static RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T2 — what the Cantilever source refuses (IDL-09, IDL-10, CD-I65-F1-14), how it places the loads (INV-09, D-10b),
    /// and the identity and fingerprints of its model (INV-04, D-12), through the production source and the real resolver.
    /// </summary>
    public class StructuralAnalysisCantileverBoundaryTests
    {
        private static void AssertNoModel(StructuralModelResult result, StructuralExportOutcome outcome)
        {
            Assert.Equal(outcome, result.Outcome);
            Assert.Null(result.Model);
            Assert.NotEmpty(result.Diagnostics);
        }

        // ---------------------------------------------------------------- IDL-10: outside V1 -> Unsupported, no model

        [Fact]
        public void UnaEstacionDeDobleCaraEsUnsupportedSinModelo()
        {
            var design = Design();
            design.StationTopology.FaceMode = CantileverStationFaceMode.Double;

            var result = Build(design);

            AssertNoModel(result, StructuralExportOutcome.Unsupported);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.StationFaceUnsupported);
        }

        [Fact]
        public void UnaEstacionDeUnaCaraNegativeYEsUnsupportedSinModelo()
        {
            var design = Design();
            design.StationTopology.SingleSide = CantileverArmSide.NegativeY;

            var result = Build(design);

            AssertNoModel(result, StructuralExportOutcome.Unsupported);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.StationSideUnsupported);
        }

        [Theory]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelFacing)]
        [InlineData(CantileverArmBodyArrangement.DoubleChannelBackToBack)]
        public void UnBrazoDeCanalesPareadosPorPlantillaEsUnsupportedSinModelo(CantileverArmBodyArrangement arrangement)
        {
            var design = Design();
            design.DefaultArmTemplate = ArmTemplate(ChannelSection, arrangement);

            var result = Build(design, profile: ProfileWith(ChannelSection));

            AssertNoModel(result, StructuralExportOutcome.Unsupported);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.ArmArrangementUnsupported);
        }

        [Fact]
        public void UnBrazoDeCanalesPareadosPorExcepcionDeCeldaEsUnsupportedSinModelo()
        {
            var design = Design();
            design.ArmCellOverrides.Add(new CantileverArmCellOverride
            {
                StationIndex = 0,
                LevelIndex = 1,
                Side = CantileverArmSide.PositiveY,
                Arm = ArmTemplate(ChannelSection, CantileverArmBodyArrangement.DoubleChannelFacing),
            });

            var result = Build(design, profile: ProfileWith(ChannelSection));

            AssertNoModel(result, StructuralExportOutcome.Unsupported);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.ArmArrangementUnsupported);
            Assert.Contains(result.Diagnostics, d => d.Subject == "ARM.2.PositiveY");
        }

        [Fact]
        public void UnaInclinacionDeBrazoMayorQue10GradosEsUnsupportedSinModelo()
        {
            var design = Design();
            design.DefaultArmTemplate.Body.SlopeRisePer12 = 3.0; // atan(3 / 12) = 14 degrees

            var result = Build(design);

            AssertNoModel(result, StructuralExportOutcome.Unsupported);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.ArmSlopeUnsupported);
        }

        [Fact]
        public void UnaLineaBloqueadaPorElResolverEsUnsupportedSinModelo()
        {
            var design = Design();
            design.StationCount = 1; // a line needs two stations: the resolver blocks it

            Assert.True(ResolveLine(design).IsBlocked);

            var result = Build(design);

            // F4-T2 (A65-F1-11): a block of the whole line has its own code and a subject that names the line.
            AssertNoModel(result, StructuralExportOutcome.Unsupported);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceLineBlocked && d.Subject == "line");
        }

        [Fact]
        public void UnaEstacionBloqueadaPorElResolverEsUnsupportedSinModelo()
        {
            var design = Design();
            design.StationTopology.ColumnBaseTemplate.ColumnSectionId = "AISC-W-W99X999"; // not in the catalogue

            Assert.True(ResolveLine(design).IsBlocked);

            var result = Build(design);

            // F4-T2 (A65-F1-11): the resolver blocks the whole line when one station cannot be resolved, so the source reports
            // the line, not "station-N".
            AssertNoModel(result, StructuralExportOutcome.Unsupported);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceLineBlocked && d.Subject == "line");
        }

        [Theory]
        [InlineData(ChannelSection)]
        [InlineData(AngleSection)]
        public void UnaSeccionDeFamiliaCOLEnUnBrazoSencilloEsUnsupportedSinModelo(string section)
        {
            var design = Design();
            design.DefaultArmTemplate = ArmTemplate(section, CantileverArmBodyArrangement.Single);

            var result = Build(design, profile: ProfileWith(section));

            // The resolver admits the arm; the box-centred placement of a C or an L then leaves its axis off the plane of the
            // station, which the idealisation refuses before the model builder can name the family (IDL-09).
            Assert.False(ResolveLine(design).IsBlocked, "el resolver admite el brazo");
            AssertNoModel(result, StructuralExportOutcome.Unsupported);
        }

        [Theory]
        [InlineData(ChannelSection)]
        [InlineData(AngleSection)]
        public void UnaSeccionDeFamiliaCOLEnElPlanoDeLaEstacionEsUnsupportedPorSuFamilia(string section)
        {
            var result = BuildFrom(Column(), new[] { Arm(1, 30.0, section: section) });

            AssertNoModel(result, StructuralExportOutcome.Unsupported);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SectionFamilyUnsupported && d.Subject == section);
        }

        // ---------------------------------------------------------------- loads (INV-09, D-10b)

        [Fact]
        public void LasPosicionesDeCargaSumanLaDistanciaRigidaDeIdl06ALaFraccionDeLaLongitudFisica()
        {
            var model = BuildValid();

            var expected = new[]
            {
                ("P1", "ARM.1.PositiveY", 24.35421884050472),
                ("P2", "ARM.2.PositiveY", 15.354218840504721),
                ("P3", "ARM.3.PositiveY", 33.35421884050472),
            };

            Assert.Equal(3, model.Loads.Count);

            foreach (var (id, member, position) in expected)
            {
                var load = model.Loads.Single(l => l.Id == id);
                Assert.Equal(member, load.MemberId);
                Assert.True(Near(position, load.DistanceFromNodeI.Inches, 1e-9), id + ": " + load.DistanceFromNodeI.Inches);
            }
        }

        [Fact]
        public void LasMagnitudesSonLasDelLectorEnLibrasFuerzaYLaDireccionEsMenosZ()
        {
            var model = BuildValid();

            Assert.Equal(1000.0, model.Loads.Single(l => l.Id == "P1").Magnitude.PoundsForce);
            Assert.Equal(2000.0, model.Loads.Single(l => l.Id == "P2").Magnitude.PoundsForce);
            Assert.Equal(ForceUnits.ToPoundsForce(500.0, ForceUnit.KilogramForce), model.Loads.Single(l => l.Id == "P3").Magnitude.PoundsForce);
            Assert.All(model.Loads, load => Assert.Equal(StructuralLoadDirection.GlobalNegativeZ, load.Direction));
        }

        [Fact]
        public void LasCargasSalenSoloDelArchivoLeidoPorElLector()
        {
            var one = BuildValid(null, new UTF8Encoding(false).GetBytes(OneLoadFile));

            var load = Assert.Single(one.Loads);
            Assert.Equal("Q1", load.Id);
            Assert.Equal("ARM.1.PositiveY", load.MemberId);
            Assert.Equal(100.0, load.Magnitude.PoundsForce);
            Assert.Equal(one.Metadata.LoadFileSha256, StructuralFingerprints.Sha256Hex(new UTF8Encoding(false).GetBytes(OneLoadFile)));
        }

        [Fact]
        public void UnNivelDeCargaFueraDeRangoEsInvalidoSinModelo()
        {
            var text = Encoding.UTF8.GetString(LoadsBytes()).Replace("\"level\": 3", "\"level\": 4");

            var result = Build(Design(), Encoding.UTF8.GetBytes(text));

            AssertNoModel(result, StructuralExportOutcome.Invalid);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.LoadTargetUnknown && d.Subject == "P3");
        }

        [Fact]
        public void UnLadoDeCargaDistintoDelDeLaEstacionEsInvalidoSinModelo()
        {
            var text = Encoding.UTF8.GetString(LoadsBytes()).Replace("\"level\": 1, \"side\": \"PositiveY\"", "\"level\": 1, \"side\": \"NegativeY\"");

            var result = Build(Design(), Encoding.UTF8.GetBytes(text));

            AssertNoModel(result, StructuralExportOutcome.Invalid);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.LoadTargetUnknown && d.Subject == "P1");
        }

        [Theory]
        [InlineData(24, StructuralExportOutcome.Valid)]
        [InlineData(25, StructuralExportOutcome.Unsupported)]
        public void MasDe24CargasPorMiembroEsUnsupportedSinModelo(int count, StructuralExportOutcome outcome)
        {
            var entries = Enumerable.Range(1, count).Select(i =>
                "{ \"id\": \"L" + i + "\", \"target\": { \"level\": 2, \"side\": \"PositiveY\" }, \"positionBasis\": \"physicalArmFraction\","
                + " \"position\": 0.5, \"direction\": \"-Z\", \"magnitude\": 10, \"unit\": \"lbf\" }");
            var json = "{ \"schema\": \"rackcad-structural-loads/v1\", \"loads\": [ " + string.Join(", ", entries) + " ] }";

            var result = Build(Design(), Encoding.UTF8.GetBytes(json));

            Assert.Equal(outcome, result.Outcome);
            Assert.Equal(outcome == StructuralExportOutcome.Valid, result.Model != null);

            if (outcome == StructuralExportOutcome.Unsupported)
            {
                Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.LoadCountExceeded);
            }
        }

        // ---------------------------------------------------------------- D-12 / INV-04: scope, identity and fingerprints

        [Fact]
        public void UnKindDistintoDeCantileverEsInvalidoSinModelo()
        {
            foreach (var kind in new[] { "Selective", "cantilever", "PushBack" })
            {
                var result = Build(Design(), kind: kind);

                AssertNoModel(result, StructuralExportOutcome.Invalid);
                Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceKindMismatch);
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(7)]
        public void UnIndiceDeEstacionFueraDeRangoEsInvalidoSinModelo(int stationIndex)
        {
            var result = Build(Design(), stationIndex: stationIndex);

            AssertNoModel(result, StructuralExportOutcome.Invalid);
            Assert.Contains(result.Diagnostics, d => d.Code == StructuralDiagnosticCodes.SourceStationRange);
        }

        [Fact]
        public void ElRackIdDelModeloEsElDelAlcanceYNoElIdDeLaLinea()
        {
            var design = Design();
            var model = BuildValid(design);

            Assert.Equal(RackId, model.Metadata.SourceRef.RackId);
            Assert.NotEqual(design.Id.ToString(), model.Metadata.SourceRef.RackId);
            Assert.NotEqual(design.Id.ToString("D"), RackId);
            Assert.Equal("Cantilever", model.Metadata.SourceRef.Kind);
            Assert.Equal(0, model.Metadata.SourceRef.StationIndex);
        }

        [Fact]
        public void ElSourceFingerprintSalaDeLosBytesDelDisenoYDelAlcance()
        {
            var design = Design();
            var bytes = Serialized(design);
            var source = new CantileverStructuralModelSource(design, Catalog);
            var loads = Loads(LoadsBytes());

            StructuralModel Make(byte[] designBytes, int station = 0) =>
                StructuralModelBuilder.BuildStructuralModel(source, Scope(designBytes, station), loads, Profile(), Catalog).Model;

            var model = Make(bytes);
            Assert.Equal(StructuralFingerprints.Source(bytes, RackId, "Cantilever", 0), model.Metadata.SourceRef.SourceFingerprint);

            var changed = bytes.Concat(new byte[] { 32 }).ToArray();
            Assert.NotEqual(model.Metadata.SourceRef.SourceFingerprint, Make(changed).Metadata.SourceRef.SourceFingerprint);
            Assert.NotEqual(model.Metadata.SourceRef.SourceFingerprint, Make(bytes, 1).Metadata.SourceRef.SourceFingerprint);
        }

        [Fact]
        public void MismoDisenoYMismasEntradasDanElMismoJsonCanonicoYLaMismaHuella()
        {
            var first = BuildValid();
            var second = BuildValid();

            Assert.Equal(StructuralModelCanonicalizer.ToCanonicalJson(first), StructuralModelCanonicalizer.ToCanonicalJson(second));
            Assert.Equal(StructuralFingerprints.AnalysisInput(first, "adapter-test/1"), StructuralFingerprints.AnalysisInput(second, "adapter-test/1"));
        }

        [Fact]
        public void CambiarLaLongitudDeCorteDelBrazoCambiaElModeloYLaHuella()
        {
            var baseline = BuildValid();
            var design = Design();
            design.DefaultArmTemplate.Body.CutLength = 40.0;

            var changed = BuildValid(design);

            Assert.NotEqual(StructuralModelCanonicalizer.ToCanonicalJson(baseline), StructuralModelCanonicalizer.ToCanonicalJson(changed));
            Assert.NotEqual(StructuralFingerprints.AnalysisInput(baseline, "adapter-test/1"), StructuralFingerprints.AnalysisInput(changed, "adapter-test/1"));
        }

        [Fact]
        public void CambiarLaSeccionDeLaColumnaCambiaElModeloYLaHuella()
        {
            var baseline = BuildValid();
            var design = Design();
            design.StationTopology.ColumnBaseTemplate.ColumnSectionId = OtherColumnSection;

            var result = Build(design, profile: ProfileWith(OtherColumnSection));
            Assert.True(result.Outcome == StructuralExportOutcome.Valid, Describe(result));

            Assert.NotEqual(StructuralModelCanonicalizer.ToCanonicalJson(baseline), StructuralModelCanonicalizer.ToCanonicalJson(result.Model));
            Assert.NotEqual(StructuralFingerprints.AnalysisInput(baseline, "adapter-test/1"), StructuralFingerprints.AnalysisInput(result.Model, "adapter-test/1"));
            Assert.Equal(OtherColumnSection, result.Model.Members.First(m => m.Id == "COL.1").SectionRef.Value);
        }

        [Fact]
        public void CambiarElArchivoDeCargasCambiaElModeloYLaHuella()
        {
            var baseline = BuildValid();
            var text = Encoding.UTF8.GetString(LoadsBytes()).Replace("\"magnitude\": 1000", "\"magnitude\": 1001");

            var changed = BuildValid(null, Encoding.UTF8.GetBytes(text));

            Assert.NotEqual(StructuralModelCanonicalizer.ToCanonicalJson(baseline), StructuralModelCanonicalizer.ToCanonicalJson(changed));
            Assert.NotEqual(StructuralFingerprints.AnalysisInput(baseline, "adapter-test/1"), StructuralFingerprints.AnalysisInput(changed, "adapter-test/1"));
            Assert.NotEqual(baseline.Metadata.LoadFileSha256, changed.Metadata.LoadFileSha256);
        }

        // ---------------------------------------------------------------- CD-I65-F1-03 / CD-I65-F1-16: the shape of the source

        [Fact]
        public void LaFuenteRecibeElDisenoYElCatalogoYImplementaLaAbstraccionNeutral()
        {
            var constructor = Assert.Single(typeof(CantileverStructuralModelSource).GetConstructors());

            Assert.Equal(
                new[] { typeof(CantileverLineDesign), typeof(StructuralSectionCatalog) },
                constructor.GetParameters().Select(p => p.ParameterType));
            Assert.True(typeof(IStructuralModelSource).IsAssignableFrom(typeof(CantileverStructuralModelSource)));
        }

        [Fact]
        public void LaFuenteNoLeeElSobreNiDeserializaBytesYResuelveConElResolverDelProducto()
        {
            var path = Path.Combine(RepoRoot(), "src", "RackCad.Application", "Systems", "Cantilever", "CantileverStructuralModelSource.cs");
            Assert.True(File.Exists(path));

            var withoutBlocks = Regex.Replace(File.ReadAllText(path), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            var code = Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);

            Assert.DoesNotContain("Deserialize", code, StringComparison.Ordinal);
            Assert.DoesNotContain("GetDesignBytes", code, StringComparison.Ordinal);
            Assert.DoesNotContain("JsonSerializer", code, StringComparison.Ordinal);
            Assert.Contains("CantileverLineResolver.Resolve", code, StringComparison.Ordinal);
            Assert.Contains("CantileverCataloguePolicies.ColumnBase", code, StringComparison.Ordinal);
            Assert.Contains("CantileverCataloguePolicies.Arm", code, StringComparison.Ordinal);
            Assert.Contains("ScopedId", code, StringComparison.Ordinal);
        }
    }
}
