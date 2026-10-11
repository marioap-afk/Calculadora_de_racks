using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Application.Units;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The inputs of the F1-T1 tests. Everything here is a FIXTURE: the material rows and the sections are declared test
    /// data and are NOT an engineering verification (CD-I65-F1-06); the source is a test double, because the Cantilever
    /// source belongs to F1-T2.
    /// </summary>
    internal static class StructuralAnalysisFixtures
    {
        public const string ColumnSection = "AISC-W-W12X26";
        public const string ArmSection = "AISC-HSS-RECT-HSS4X4X_250";
        public const string OtherArmSection = "AISC-HSS-RECT-HSS6X4X_250";
        public const string BareSection = "AISC-W-W8X10";
        public const string ChannelSection = "AISC-C-C12X20_7";
        public const string AngleSection = "AISC-L-L4X4X1_4";
        public const string UnknownSection = "AISC-W-W99X999";

        public static readonly DateTime FixtureDate = new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc);

        public static StructuralSectionProperties Properties(double area, double ix, double iy, double j) =>
            new StructuralSectionProperties { Area = area, Ix = ix, Iy = iy, J = j };

        public static StructuralSectionDefinition Section(
            string id,
            StructuralSectionFamily family,
            string edi,
            StructuralSectionProperties properties,
            StructuralSectionUnitSystem system = StructuralSectionUnitSystem.UsCustomary,
            string grade = null)
        {
            return new StructuralSectionDefinition
            {
                Identity = new StructuralSectionIdentity
                {
                    SectionId = StructuralSectionId.Parse(id),
                    Family = family,
                    EdiDesignation = edi,
                    ManualLabel = edi,
                    SourceId = "FIXTURE",
                    SourceRevision = "0",
                },
                WeightPerLength = 10,
                NativeUnitSystem = system,
                Properties = properties,
                MaterialGrade = grade,
            };
        }

        public static StructuralSectionCatalog Catalog(string columnGrade = "FIXTURE-GRADE-X", StructuralSectionUnitSystem armSystem = StructuralSectionUnitSystem.UsCustomary)
        {
            return StructuralSectionCatalog.Create(
                new[]
                {
                    Section(ColumnSection, StructuralSectionFamily.W, "W12X26", Properties(7.65, 204, 17.3, 0.3), grade: columnGrade),
                    Section(ArmSection, StructuralSectionFamily.HssRectangular, "HSS4X4X.250", Properties(3.37, 7.8, 7.8, 12.8), armSystem),
                    Section(OtherArmSection, StructuralSectionFamily.HssRectangular, "HSS6X4X.250", Properties(4.30, 20.9, 11.1, 24.0)),
                    Section(BareSection, StructuralSectionFamily.W, "W8X10", new StructuralSectionProperties { Area = null, Ix = null, Iy = 2.09, J = null }),
                    Section(ChannelSection, StructuralSectionFamily.Channel, "C12X20.7", Properties(6.09, 129, 3.88, 0.369)),
                    Section(AngleSection, StructuralSectionFamily.Angle, "L4X4X1/4", Properties(1.94, 3.04, 3.04, 0.04)),
                },
                null);
        }

        public static StructuralMaterialRow Row(
            string sectionId, string materialId, string verifier = "TEST-FIXTURE", DateTime? verifiedOn = null, bool unverified = false)
        {
            return new StructuralMaterialRow(
                StructuralSectionId.Parse(sectionId),
                StructuralMaterialId.Create(materialId),
                unverified ? null : verifier,
                unverified ? (DateTime?)null : (verifiedOn ?? FixtureDate));
        }

        public static StructuralMaterialProfile Profile(
            string columnMaterial = "FIXTURE-MATERIAL-COLUMN", string armMaterial = "FIXTURE-MATERIAL-ARM", string verifier = "TEST-FIXTURE")
        {
            return new StructuralMaterialProfile(new[]
            {
                Row(ColumnSection, columnMaterial, verifier),
                Row(ArmSection, armMaterial, verifier),
                Row(OtherArmSection, armMaterial, verifier),
                Row(BareSection, columnMaterial, verifier),
            });
        }

        public static StructuralScope Scope(string rackId = "RACK-1", string kind = "Cantilever", int station = 0, string design = "{\"design\":1}") =>
            new StructuralScope(rackId, kind, station, Encoding.UTF8.GetBytes(design));

        // ---- the loads file ----------------------------------------------------------------------------

        public static string Load(
            string id = "\"L1\"",
            string level = "1",
            string side = "\"PositiveY\"",
            string basis = "\"physicalArmFraction\"",
            string position = "0.5",
            string direction = "\"-Z\"",
            string magnitude = "100",
            string unit = "\"N\"",
            string target = null,
            string extra = null)
        {
            var parts = new List<string>();
            Add(parts, "id", id);
            Add(parts, "target", target ?? "{\"level\":" + level + ",\"side\":" + side + "}");
            Add(parts, "positionBasis", basis);
            Add(parts, "position", position);
            Add(parts, "direction", direction);
            Add(parts, "magnitude", magnitude);
            Add(parts, "unit", unit);

            if (extra != null)
            {
                parts.Add(extra);
            }

            return "{" + string.Join(",", parts) + "}";
        }

        public static string File(IEnumerable<string> loads, string schema = "\"rackcad-structural-loads/v1\"", string extra = null)
        {
            var parts = new List<string>();
            Add(parts, "schema", schema);
            parts.Add("\"loads\":[" + string.Join(",", loads) + "]");

            if (extra != null)
            {
                parts.Add(extra);
            }

            return "{" + string.Join(",", parts) + "}";
        }

        public static string File(params string[] loads) => File((IEnumerable<string>)loads);

        public static string ManyLoads(int count) =>
            File(Enumerable.Range(1, count).Select(i => Load(id: "\"L" + i + "\"", position: (i / (double)(count + 1)).ToString("R", System.Globalization.CultureInfo.InvariantCulture))));

        public static byte[] Bytes(string json) => new UTF8Encoding(false).GetBytes(json);

        public static StructuralLoadFileReadResult Read(string json) => StructuralLoadFileReader.Read(Bytes(json));

        public static StructuralLoadFileReadResult TwoLoads() =>
            Read(File(Load(), Load(id: "\"L2\"", level: "2", position: "1", magnitude: "2.5", unit: "\"kip\"")));

        public static StructuralModelResult Build(
            StructuralAnalysisFixtureSource source = null,
            StructuralLoadFileReadResult loads = null,
            IStructuralMaterialProfile profile = null,
            StructuralSectionCatalog catalog = null,
            StructuralScope scope = null)
        {
            return StructuralModelBuilder.BuildStructuralModel(
                source ?? new StructuralAnalysisFixtureSource(),
                scope ?? Scope(),
                loads ?? TwoLoads(),
                profile ?? Profile(),
                catalog ?? Catalog());
        }

        public static StructuralModel ValidModel(
            StructuralAnalysisFixtureSource source = null,
            StructuralLoadFileReadResult loads = null,
            IStructuralMaterialProfile profile = null,
            StructuralScope scope = null)
        {
            var result = Build(source, loads, profile, null, scope);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Model);
            return result.Model;
        }

        private static void Add(List<string> parts, string key, string rawValue)
        {
            if (rawValue != null)
            {
                parts.Add("\"" + key + "\":" + rawValue);
            }
        }
    }

    /// <summary>A column with <see cref="Levels"/> arms on its positive face. A test double of the neutral source.</summary>
    internal sealed class StructuralAnalysisFixtureSource : IStructuralModelSource
    {
        public const double LevelPitch = 48;

        public int Levels = 2;
        public double ArmLengthInches = 48;
        public string ColumnSectionId = StructuralAnalysisFixtures.ColumnSection;
        public string ArmSectionId = StructuralAnalysisFixtures.ArmSection;
        public bool ReleaseFirstArm;
        public bool Reverse;
        public bool PeralteParallelToArm;
        public double DistanceScale = 1.0;
        public bool NegativeZeroColumnX;
        public string SupportStatement = "COL.FOOT is fixed: a hypothesis for transfer equivalence only.";
        public int CallCount;

        public StructuralSourceDescription Describe(StructuralScope scope, StructuralLoadFile loads)
        {
            CallCount++;

            var nodes = new List<StructuralNode> { Node("COL.FOOT", 0, 0, 0) };
            var members = new List<StructuralSourceMember>();
            var previous = "COL.FOOT";

            for (var level = 1; level <= Levels; level++)
            {
                var joint = "ARM." + level + ".PositiveY.J";
                var tip = "ARM." + level + ".PositiveY.TIP";

                nodes.Add(Node(joint, 0, 0, LevelPitch * level));
                nodes.Add(Node(tip, 0, ArmLengthInches, LevelPitch * level));
                members.Add(Column("COL." + level, previous, joint));
                members.Add(Arm("ARM." + level + ".PositiveY", joint, tip, level == 1 && ReleaseFirstArm));
                previous = joint;
            }

            nodes.Add(Node("COL.TOP", 0, 0, LevelPitch * (Levels + 1)));
            members.Add(Column("COL." + (Levels + 1), previous, "COL.TOP"));

            var diagnostics = new List<StructuralDiagnostic>();
            var placed = new List<StructuralMemberPointLoad>();

            foreach (var load in loads.Loads)
            {
                if (load.Side != StructuralLoadSide.PositiveY || load.Level < 1 || load.Level > Levels)
                {
                    diagnostics.Add(StructuralDiagnostic.Blocking(
                        StructuralDiagnosticCodes.LoadTargetMember, StructuralExportOutcome.Invalid, load.Id));
                    continue;
                }

                placed.Add(new StructuralMemberPointLoad(
                    load.Id,
                    "ARM." + load.Level + ".PositiveY",
                    load.Magnitude,
                    StructuralLength.FromInches(load.PositionFraction * ArmLengthInches * DistanceScale)));
            }

            var assumptions = new List<AnalysisAssumption>
            {
                new AnalysisAssumption("SUP-01", SupportStatement),
                new AnalysisAssumption("CON-01", "Arm and column are continuous: a hypothesis for transfer equivalence only."),
                new AnalysisAssumption("LOD-01", "One reference case REF1 with the loads of the file."),
            };

            var supports = new List<StructuralSupport>
            {
                new StructuralSupport("COL.FOOT", Enumerable.Repeat(true, StructuralSupport.FlagCount), "SUP-01"),
            };

            if (Reverse)
            {
                nodes.Reverse();
                members.Reverse();
                placed.Reverse();
                assumptions.Reverse();
            }

            return new StructuralSourceDescription(nodes, members, supports, assumptions, placed, diagnostics);
        }

        private StructuralNode Node(string id, double x, double y, double z) =>
            new StructuralNode(id, new StructuralPosition(
                StructuralLength.FromInches(NegativeZeroColumnX && x == 0 ? -0.0 : x), StructuralLength.FromInches(y), StructuralLength.FromInches(z)));

        private StructuralSourceMember Column(string id, string i, string j) =>
            new StructuralSourceMember(
                id, i, j, StructuralSectionId.Parse(ColumnSectionId), new StructuralDirection(1, 0, 0),
                StructuralMemberRole.Column, "COLUMN", StructuralMemberReleases.None("CON-01"));

        private StructuralSourceMember Arm(string id, string i, string j, bool released)
        {
            var flags = Enumerable.Repeat(false, StructuralMemberReleases.FlagCount).ToArray();
            flags[0] = released;

            return new StructuralSourceMember(
                id, i, j, StructuralSectionId.Parse(ArmSectionId),
                PeralteParallelToArm ? new StructuralDirection(0, 1, 0) : new StructuralDirection(0, 0, 1),
                StructuralMemberRole.Arm, "ARM", new StructuralMemberReleases(flags, "CON-01"));
        }
    }

    /// <summary>
    /// I-65 F1-T1 — D-13, D-06, D-07, INV-05: the pure construction of the neutral model from a neutral source (a test
    /// double here), the catalogue, the loads file and the explicit material profile.
    /// </summary>
    public class StructuralAnalysisModelBuilderTests
    {
        private static IEnumerable<string> Codes(StructuralModelResult result) => result.Diagnostics.Select(d => d.Code);

        [Fact]
        public void UnDisenoValidoProduceUnModeloConTodasSusPartes()
        {
            var result = StructuralAnalysisFixtures.Build();
            var model = result.Model;

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(model);
            Assert.Equal(6, model.Nodes.Count);
            Assert.Equal(5, model.Members.Count);
            Assert.Equal(3, model.Members.Count(m => m.Role == StructuralMemberRole.Column));
            Assert.Equal(2, model.Members.Count(m => m.Role == StructuralMemberRole.Arm));
            Assert.Equal(2, model.Sections.Count);
            Assert.Equal(2, model.Materials.Count);
            Assert.Single(model.Supports);
            Assert.Equal(2, model.Loads.Count);
            Assert.Equal(3, model.Assumptions.Count);
            Assert.Equal(StructuralLoadCase.ReferenceCaseId, model.LoadCase.Id);
            Assert.All(model.Members, member => Assert.False(member.Releases.AnyReleased));
        }

        [Fact]
        public void LaMetadataLlevaLaFuenteLasHuellasYLaVersionDelGenerador()
        {
            var json = StructuralAnalysisFixtures.File(StructuralAnalysisFixtures.Load());
            var bytes = StructuralAnalysisFixtures.Bytes(json);
            var scope = StructuralAnalysisFixtures.Scope("RACK-9", "Cantilever", 3, "{\"design\":42}");
            var profile = StructuralAnalysisFixtures.Profile();
            var model = StructuralAnalysisFixtures.ValidModel(loads: StructuralLoadFileReader.Read(bytes), profile: profile, scope: scope);

            Assert.Equal("RACK-9", model.Metadata.SourceRef.RackId);
            Assert.Equal("Cantilever", model.Metadata.SourceRef.Kind);
            Assert.Equal(3, model.Metadata.SourceRef.StationIndex);
            Assert.Equal(StructuralFingerprints.Source(scope.GetDesignBytes(), "RACK-9", "Cantilever", 3), model.Metadata.SourceRef.SourceFingerprint);
            Assert.Equal(StructuralModelBuilder.GeneratorVersion, model.Metadata.GeneratorVersion);
            Assert.Equal(StructuralFingerprints.Sha256Hex(bytes), model.Metadata.LoadFileSha256);

            var usedRows = new[] { StructuralAnalysisFixtures.ColumnSection, StructuralAnalysisFixtures.ArmSection }
                .Select(id => { profile.TryGetRow(StructuralSectionId.Parse(id), out var row); return row; });

            Assert.Equal(StructuralFingerprints.MappingRows(usedRows), model.Metadata.MappingRowsSha256);
            Assert.Equal(StructuralFingerprints.Assumptions(model.Assumptions), model.Metadata.AssumptionsSha256);
        }

        [Fact]
        public void LaPosicionDeLaCargaEsLaDistanciaAbsolutaDesdeElNudoIYLaMagnitudeSeConvierteALbf()
        {
            var loads = StructuralAnalysisFixtures.Read(StructuralAnalysisFixtures.File(
                StructuralAnalysisFixtures.Load(position: "0.25", magnitude: "2", unit: "\"kip\"")));
            var model = StructuralAnalysisFixtures.ValidModel(loads: loads);
            var load = Assert.Single(model.Loads);

            Assert.Equal("ARM.1.PositiveY", load.MemberId);
            Assert.Equal(12.0, load.DistanceFromNodeI.Inches);
            Assert.Equal(2000.0, load.Magnitude.PoundsForce);
            Assert.Equal(StructuralLoadDirection.GlobalNegativeZ, load.Direction);
        }

        [Fact]
        public void UnaSeccionAusenteDelCatalogoEsInvalidYNoExponeModelo()
        {
            var result = StructuralAnalysisFixtures.Build(new StructuralAnalysisFixtureSource { ArmSectionId = StructuralAnalysisFixtures.UnknownSection });

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.SectionUnknown, Codes(result));
        }

        [Theory]
        [InlineData("AISC-C-C12X20_7")]
        [InlineData("AISC-L-L4X4X1_4")]
        public void LasFamiliasCYLSonUnsupportedYNoExponenModelo(string sectionId)
        {
            var profile = new StructuralMaterialProfile(new[]
            {
                StructuralAnalysisFixtures.Row(StructuralAnalysisFixtures.ColumnSection, "FIXTURE-MATERIAL-COLUMN"),
                StructuralAnalysisFixtures.Row(sectionId, "FIXTURE-MATERIAL-ARM"),
            });
            var result = StructuralAnalysisFixtures.Build(new StructuralAnalysisFixtureSource { ArmSectionId = sectionId }, profile: profile);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.SectionFamilyUnsupported, Codes(result));
        }

        [Fact]
        public void UnaFilaConSistemaNativoDistintoDeUsCustomaryEsUnsupported()
        {
            var result = StructuralAnalysisFixtures.Build(catalog: StructuralAnalysisFixtures.Catalog(armSystem: StructuralSectionUnitSystem.Metric));

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.SectionUnitSystem, Codes(result));
        }

        [Fact]
        public void LasPropiedadesAusentesSonAusenciaExplicitaConDiagnosticoNoBloqueante()
        {
            var source = new StructuralAnalysisFixtureSource { ColumnSectionId = StructuralAnalysisFixtures.BareSection };
            var result = StructuralAnalysisFixtures.Build(source);
            var section = result.Model.Sections.Single(s => s.Id == StructuralSectionId.Parse(StructuralAnalysisFixtures.BareSection));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(section.Area);
            Assert.Null(section.Ix);
            Assert.Null(section.J);
            Assert.Equal(2.09, section.Iy.Value.InchesToTheFourth);

            var absent = result.Diagnostics.Where(d => d.Code == StructuralDiagnosticCodes.SectionPropertyAbsent).ToList();
            Assert.Equal(3, absent.Count);
            Assert.All(absent, d => Assert.False(d.IsBlocking));
            Assert.All(absent, d => Assert.Contains(StructuralAnalysisFixtures.BareSection, d.Subject, StringComparison.Ordinal));
        }

        [Fact]
        public void UnaFilaDeMaterialAusenteEsUnsupportedAunqueElCatalogoTengaMaterialGrade()
        {
            var profile = new StructuralMaterialProfile(new[]
            {
                StructuralAnalysisFixtures.Row(StructuralAnalysisFixtures.ArmSection, "FIXTURE-MATERIAL-ARM"),
            });
            var result = StructuralAnalysisFixtures.Build(profile: profile, catalog: StructuralAnalysisFixtures.Catalog(columnGrade: "A992 GR50"));

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.MaterialRowMissing, Codes(result));
        }

        [Fact]
        public void ConFilaElMaterialEsElDeLaFilaAunqueMaterialGradeDigaOtro()
        {
            var model = StructuralAnalysisFixtures.ValidModel(
                profile: StructuralAnalysisFixtures.Profile(columnMaterial: "FIXTURE-MATERIAL-FROM-ROW"));
            var column = model.Members.First(m => m.Role == StructuralMemberRole.Column);

            Assert.Equal("FIXTURE-MATERIAL-FROM-ROW", column.MaterialRef.Value);
            Assert.Contains(model.Materials, material => material.Id.Value == "FIXTURE-MATERIAL-FROM-ROW");
            Assert.DoesNotContain(model.Materials, material => material.Id.Value.Contains("GRADE", StringComparison.Ordinal));
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void UnaFilaSinVerificadorOSinFechaEsUnsupported(bool verifierBlank, bool dateMissing)
        {
            var profile = new StructuralMaterialProfile(new[]
            {
                StructuralAnalysisFixtures.Row(StructuralAnalysisFixtures.ColumnSection, "FIXTURE-MATERIAL-COLUMN"),
                new StructuralMaterialRow(
                    StructuralSectionId.Parse(StructuralAnalysisFixtures.ArmSection),
                    StructuralMaterialId.Create("FIXTURE-MATERIAL-ARM"),
                    verifierBlank ? "  " : (dateMissing ? "TEST-FIXTURE" : null),
                    dateMissing ? (DateTime?)null : StructuralAnalysisFixtures.FixtureDate),
            });
            var result = StructuralAnalysisFixtures.Build(profile: profile);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.MaterialRowUnverified, Codes(result));
        }

        [Fact]
        public void ElPerfilDeProductoNoTieneFilasAsiQueTodoDisenoEsUnsupported()
        {
            var result = StructuralAnalysisFixtures.Build(profile: StructuralMaterialProfile.Empty);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.MaterialRowMissing, Codes(result));
        }

        [Fact]
        public void CualquierLiberacionVerdaderaEsUnsupported()
        {
            var result = StructuralAnalysisFixtures.Build(new StructuralAnalysisFixtureSource { ReleaseFirstArm = true });

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.ReleaseUnsupported, Codes(result));
        }

        [Fact]
        public void VeinticuatroCargasEnUnMiembroSonValidasYVeinticincoSonUnsupported()
        {
            var valid = StructuralAnalysisFixtures.Build(loads: StructuralAnalysisFixtures.Read(StructuralAnalysisFixtures.ManyLoads(24)));
            var tooMany = StructuralAnalysisFixtures.Build(loads: StructuralAnalysisFixtures.Read(StructuralAnalysisFixtures.ManyLoads(25)));

            Assert.Equal(24, StructuralModelBuilder.MaxPointLoadsPerMember);
            Assert.Equal(StructuralExportOutcome.Valid, valid.Outcome);
            Assert.Equal(24, valid.Model.Loads.Count);
            Assert.Equal(StructuralExportOutcome.Unsupported, tooMany.Outcome);
            Assert.Null(tooMany.Model);
            Assert.Contains(StructuralDiagnosticCodes.LoadCountExceeded, Codes(tooMany));
        }

        [Fact]
        public void UnaCargaSinUnidadEsInvalidYLaFuenteNiSiquieraSeConsulta()
        {
            var source = new StructuralAnalysisFixtureSource();
            var loads = StructuralAnalysisFixtures.Read(StructuralAnalysisFixtures.File(StructuralAnalysisFixtures.Load(unit: null)));
            var result = StructuralAnalysisFixtures.Build(source, loads);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.LoadsUnitMissing, Codes(result));
            Assert.Equal(0, source.CallCount);
        }

        [Fact]
        public void UnDestinoQueLaFuenteNoResuelveEsInvalid()
        {
            var loads = StructuralAnalysisFixtures.Read(StructuralAnalysisFixtures.File(
                StructuralAnalysisFixtures.Load(), StructuralAnalysisFixtures.Load(id: "\"L2\"", level: "9")));
            var result = StructuralAnalysisFixtures.Build(loads: loads);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.LoadTargetMember, Codes(result));
        }

        [Fact]
        public void UnaCargaFueraDelMiembroEsInvalid()
        {
            var result = StructuralAnalysisFixtures.Build(new StructuralAnalysisFixtureSource { DistanceScale = 1.5 }, StructuralAnalysisFixtures.Read(
                StructuralAnalysisFixtures.File(StructuralAnalysisFixtures.Load(position: "0.9"))));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.LoadPositionRange, Codes(result));
        }

        [Fact]
        public void UnPeralteParaleloAlEjeDelMiembroEsInvalid()
        {
            var result = StructuralAnalysisFixtures.Build(new StructuralAnalysisFixtureSource { PeralteParallelToArm = true });

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Model);
            Assert.Contains(StructuralDiagnosticCodes.MemberGeometry, Codes(result));
        }

        [Fact]
        public void LaPrecedenciaDelResultadoAgregadoSeAplicaALosHallazgosDelConstructor()
        {
            var result = StructuralAnalysisFixtures.Build(
                new StructuralAnalysisFixtureSource { ReleaseFirstArm = true, ArmSectionId = StructuralAnalysisFixtures.UnknownSection });

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Contains(StructuralDiagnosticCodes.ReleaseUnsupported, Codes(result));
            Assert.Contains(StructuralDiagnosticCodes.SectionUnknown, Codes(result));
            Assert.Null(result.Model);
        }

        [Fact]
        public void NoExistenAnalyzeNiReadResultsNiEscrituraDeArchivosEnLaCapaNeutral()
        {
            var types = typeof(StructuralModelBuilder).Assembly.GetTypes()
                .Where(t => t.Namespace == "RackCad.Application.StructuralAnalysis")
                .ToList();
            var members = types
                .SelectMany(t => t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Select(m => m.Name)
                .ToList();

            Assert.NotEmpty(types);
            Assert.DoesNotContain(members, name => name.Equals("Analyze", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(members, name => name.Equals("ReadResults", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(types, t => t.Name.Contains("Writer", StringComparison.Ordinal));
        }

        [Fact]
        public void LaConstruccionEsPuraYRepetibleSobreLasMismasEntradas()
        {
            var first = StructuralAnalysisFixtures.ValidModel();
            var second = StructuralAnalysisFixtures.ValidModel();

            Assert.NotSame(first, second);
            Assert.Equal(StructuralModelCanonicalizer.ToCanonicalJson(first), StructuralModelCanonicalizer.ToCanonicalJson(second));
        }
    }
}
