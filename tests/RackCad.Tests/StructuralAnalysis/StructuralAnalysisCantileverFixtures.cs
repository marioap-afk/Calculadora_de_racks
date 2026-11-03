using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The inputs of the I-65 F1-T2 tests (REF-CANT-01). The material rows are FIXTURES declared as such: they are not an
    /// engineering verification (CD-I65-F1-06). The design and the load file are read from the fixtures directory, from the
    /// repository root, and never from a hash of their bytes.
    /// </summary>
    internal static class StructuralAnalysisCantileverFixtures
    {
        public const string RackId = "3b0b1b64-1c5e-4a38-9c6f-0c3a5d2e7f10";
        public const string ColumnSection = "AISC-W-W12X26";
        public const string ArmSection = "AISC-HSS-RECT-HSS4X4X_250";
        public const string OtherColumnSection = "AISC-W-W10X33";
        public const string ChannelSection = "AISC-C-C10X15_3";
        public const string AngleSection = "AISC-L-L2X2X1_4";
        public const string FixtureDirectory = "tests/RackCad.Tests/StructuralAnalysis/Fixtures/";

        public const string OneLoadFile =
            "{ \"schema\": \"rackcad-structural-loads/v1\", \"loads\": [ { \"id\": \"Q1\", \"target\": { \"level\": 1, \"side\": \"PositiveY\" },"
            + " \"positionBasis\": \"physicalArmFraction\", \"position\": 0.5, \"direction\": \"-Z\", \"magnitude\": 100, \"unit\": \"lbf\" } ] }";

        public static readonly DateTime FixtureDate = new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc);

        public static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        public static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontro la raiz del repositorio (RackCad.sln).");
            return dir.FullName;
        }

        public static byte[] FixtureBytes(string name)
        {
            var path = Path.Combine(RepoRoot(), FixtureDirectory + name);
            Assert.True(File.Exists(path), "No existe el fixture " + path);
            return File.ReadAllBytes(path);
        }

        public static byte[] DesignBytes() => FixtureBytes("ref-cant-01.design.json");

        public static byte[] LoadsBytes() => FixtureBytes("ref-cant-01.loads.json");

        /// <summary>A fresh deserialised design every call, so a test can change it without touching another.</summary>
        public static CantileverLineDesign Design()
        {
            var json = Encoding.UTF8.GetString(DesignBytes());
            var project = new RackProjectStore().Deserialize(json);
            Assert.NotNull(project.CantileverLineDesign);
            return project.CantileverLineDesign;
        }

        public static byte[] Serialized(CantileverLineDesign design) =>
            new UTF8Encoding(false).GetBytes(new RackProjectStore().Serialize(RackProject.ForCantilever(design)));

        public static IStructuralMaterialProfile Profile() =>
            new StructuralMaterialProfile(new[]
            {
                Row(ColumnSection, "FIXTURE-MATERIAL-A"),
                Row(ArmSection, "FIXTURE-MATERIAL-B"),
            });

        public static IStructuralMaterialProfile ProfileWith(params string[] extraSections) =>
            new StructuralMaterialProfile(new[]
            {
                Row(ColumnSection, "FIXTURE-MATERIAL-A"),
                Row(ArmSection, "FIXTURE-MATERIAL-B"),
            }.Concat(extraSections.Select(id => Row(id, "FIXTURE-MATERIAL-C"))));

        public static StructuralMaterialRow Row(string sectionId, string materialId) =>
            new StructuralMaterialRow(
                StructuralSectionId.Parse(sectionId), StructuralMaterialId.Create(materialId), "TEST-FIXTURE", FixtureDate);

        public static StructuralScope Scope(byte[] designBytes, int stationIndex = 0, string kind = "Cantilever") =>
            new StructuralScope(RackId, kind, stationIndex, designBytes);

        public static StructuralLoadFileReadResult Loads(byte[] bytes)
        {
            var result = StructuralLoadFileReader.Read(bytes);
            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            return result;
        }

        public static StructuralLoadFileReadResult Loads(string json) => Loads(new UTF8Encoding(false).GetBytes(json));

        /// <summary>The whole chain with the production source: design in, result out.</summary>
        public static StructuralModelResult Build(
            CantileverLineDesign design,
            byte[] loadsBytes = null,
            int stationIndex = 0,
            string kind = "Cantilever",
            IStructuralMaterialProfile profile = null)
        {
            var source = new CantileverStructuralModelSource(design, Catalog);

            return StructuralModelBuilder.BuildStructuralModel(
                source,
                Scope(Serialized(design), stationIndex, kind),
                Loads(loadsBytes ?? LoadsBytes()),
                profile ?? Profile(),
                Catalog);
        }

        public static StructuralModel BuildValid(CantileverLineDesign design = null, byte[] loadsBytes = null, int stationIndex = 0)
        {
            var result = Build(design ?? Design(), loadsBytes, stationIndex);
            Assert.True(
                result.Outcome == StructuralExportOutcome.Valid && result.Model != null,
                "Se esperaba Valid; resultado " + result.Outcome + ": " + Describe(result));
            return result.Model;
        }

        public static string Describe(StructuralModelResult result) =>
            string.Join(", ", result.Diagnostics.Select(d => d.Code + "(" + d.Subject + ")"));

        public static CantileverLineAssembly ResolveLine(CantileverLineDesign design) =>
            CantileverLineResolver.Resolve(
                design,
                Catalog,
                new StructuralSectionGeometryFactory(Catalog),
                CantileverCataloguePolicies.ColumnBase(Catalog),
                CantileverCataloguePolicies.Arm(Catalog));

        public static CantileverArmTemplateDesign ArmTemplate(
            string section, CantileverArmBodyArrangement arrangement, double cutLength = 36.0) =>
            new CantileverArmTemplateDesign
            {
                Body = new CantileverArmBodyDesign
                {
                    Arrangement = arrangement,
                    SectionId = section,
                    CutLength = cutLength,
                },
                // Deeper sections (the channels) need a taller mounting plate than the default two punches give.
                MountingPlate = new CantileverArmMountingPlateTemplateDesign { VerticalPunchCount = 4 },
            };

        public static bool Near(double expected, double actual, double tolerance) =>
            Math.Abs(expected - actual) <= tolerance;

        // ---------------------------------------------------------------- synthetic placements (IDL-08, INV-07)

        public const double ColumnStartZ = 0.25;
        public const double ColumnLength = 90.5;
        public const double ColumnAxisY = -6.1;
        public const double ArmOriginY = 0.25;

        public static CantileverStructuralModelSource.StationPiece Column(
            double rotationDegrees = 0.0, bool mirrored = false, double length = ColumnLength, double axisX = 0.0)
        {
            var frame = LocalFrame3D.Create(new Point3D(axisX, ColumnAxisY, ColumnStartZ), Vector3D.UnitZ, Vector3D.UnitX);

            return new CantileverStructuralModelSource.StationPiece(
                "CANT-S0-STN-COL",
                PrismaticSectionInstance.Create(StructuralSectionId.Parse(ColumnSection), length, frame, rotationDegrees, mirrored));
        }

        /// <summary>
        /// An arm whose axis, prolonged, meets the column axis at height <paramref name="zAtColumnAxis"/>. The slope, a
        /// shift of the whole arm along X and a tilt out of the station plane are all in degrees or inches.
        /// </summary>
        public static CantileverStructuralModelSource.StationPiece Arm(
            int level,
            double zAtColumnAxis,
            double slopeDegrees = 2.0,
            double rotationDegrees = 0.0,
            bool mirrored = false,
            double shiftX = 0.0,
            double outOfPlaneDegrees = 0.0,
            string section = ArmSection)
        {
            var slope = slopeDegrees * Math.PI / 180.0;
            var tilt = outOfPlaneDegrees * Math.PI / 180.0;
            var direction = new Vector3D(Math.Sin(tilt), Math.Cos(slope), Math.Sin(slope));
            var originZ = zAtColumnAxis + ((ArmOriginY - ColumnAxisY) * Math.Tan(slope));
            var frame = LocalFrame3D.Create(new Point3D(shiftX, ArmOriginY, originZ), direction, new Vector3D(-1.0, 0.0, 0.0));

            return new CantileverStructuralModelSource.StationPiece(
                "CANT-S0-STN-L" + level + "-PY-BODY-1",
                PrismaticSectionInstance.Create(StructuralSectionId.Parse(section), 36.0, frame, rotationDegrees, mirrored));
        }

        public static StructuralModelResult BuildFrom(
            CantileverStructuralModelSource.StationPiece column,
            IReadOnlyList<CantileverStructuralModelSource.StationPiece> arms,
            string loadsJson = OneLoadFile)
        {
            var loads = Loads(loadsJson);
            var description = CantileverStructuralModelSource.Idealise(column, arms, loads.LoadFile);

            return StructuralModelBuilder.BuildStructuralModel(
                new FixedSource(description),
                Scope(new byte[] { 1 }),
                loads,
                Profile(),
                Catalog);
        }

        private sealed class FixedSource : IStructuralModelSource
        {
            private readonly StructuralSourceDescription _description;

            public FixedSource(StructuralSourceDescription description)
            {
                _description = description;
            }

            public StructuralSourceDescription Describe(StructuralScope scope, StructuralLoadFile loads) => _description;
        }
    }
}
