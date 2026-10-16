using System;
using System.IO;
using System.Linq;
using System.Text;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-D — the table of the product ships the two rows of REF-CANT-01 as verified, with the values approved by the Owner
    /// (D-09 and D-10). Everything below is observed on the table EXACTLY as it ships (the data file), never on a fixture table, except
    /// where a fixture table is named as the reference for the expected diagnostics.
    /// </summary>
    public class StructuralAnalysisSectionMapVerifiedTests
    {
        private const string ColumnId = "AISC-W-W12X26";
        private const string ArmId = "AISC-HSS-RECT-HSS4X4X_250";

        private const string VerifiedBy = "Mario Pérez — Owner / Engineering Validator";
        private const string VerifiedOn = "2026-10-16";

        private const string ColumnSource =
            "Geometry: AISC Shapes Database v16.0 (Aug 2023; file SHA-256 82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496), row W12X26 as published: A 7.65 in2, Ix 204 in4, Iy 17.3 in4, J 0.300 in4; independent derivation within 0.24 %. Material: ASTM A992/A992M Grade 50 (current ASTM edition A992/A992M-22). E 29000 ksi and nu 0.30: modeling conventions for structural steel accepted at verification; primary citation not read in I-65. Unit weight 490 pcf: steel density implied by the AISC v16.0 HSS nominal weights (median 490.00 pcf). RAM: United States/W/W 12X26, material A992 Gr50 (E 29000 ksi, nu 0.3, 486.94 pcf). DIF-01: RAM unit weight -0.62 %, informative only for the pilot transfer test without self weight; RAM values are not equivalent for later analyses. RAM catalog formal edition not verified. Ref: I-65 F4 engineering approval package.";

        private const string ArmSource =
            "Geometry: AISC Shapes Database v16.0 (Aug 2023; file SHA-256 82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496), row HSS4X4X1/4 as published: tnom 0.25 in, tdes 0.233 in (0.93 tnom), A 3.37 in2, Ix = Iy 7.80 in4, J 12.8 in4; independent derivation with outside corner radius 2 tdes within 0.15 %. Material: ASTM A500/A500M Grade C, square and rectangular (current ASTM edition A500/A500M-23). E 29000 ksi and nu 0.30: modeling conventions for structural steel accepted at verification; primary citation not read in I-65. Unit weight 490 pcf: steel density implied by the AISC v16.0 HSS nominal weights (median 490.00 pcf). RAM: United States/HSS_SQR/HSS_SQR 4X4X1_4, material A500 GrC rectangular, not A500 GrC rounded (E 29000 ksi, nu 0.3, 486.94 pcf). DIF-01: RAM unit weight -0.62 %, informative only for the pilot transfer test without self weight; RAM values are not equivalent for later analyses. RAM catalog formal edition not verified. Ref: I-65 F4 engineering approval package.";

        private static string TableText() => F.DataFileText();

        private static RamSectionMaterialTable ShippedTable()
        {
            var read = RamSectionMapReader.Read(TableText());
            Assert.Equal(StructuralExportOutcome.Valid, read.Outcome);
            Assert.NotNull(read.Table);
            return read.Table;
        }

        private static CantileverLineDesign Decode(byte[] bytes) =>
            new RackProjectStore().Deserialize(Encoding.UTF8.GetString(bytes))?.CantileverLineDesign;

        private static StructuralModelResult BuildRefCant01(RamSectionMaterialTable table) =>
            CantileverStructuralExport.Build(C.RackId, "Cantilever", 0, C.DesignBytes(), Decode, C.LoadsBytes(), table, C.Catalog);

        private static string[] Codes(StructuralModelResult result) =>
            result.Diagnostics.Select(d => d.Code + "|" + d.Subject + "|" + d.Severity).OrderBy(x => x, StringComparer.Ordinal).ToArray();

        private static RamSectionMapRow Row(RamSectionMaterialTable table, string id) =>
            table.Rows.Single(r => r.SectionId.Value == id);

        [Fact]
        public void ElLectorLeLaTablaDistribuidaValidaSinDiagnosticosConLasDosFilasVerified()
        {
            var read = RamSectionMapReader.Read(TableText());

            Assert.Equal(StructuralExportOutcome.Valid, read.Outcome);
            Assert.Empty(read.Diagnostics);
            Assert.NotNull(read.Table);
            Assert.Equal(new[] { ArmId, ColumnId }, read.Table.Rows.Select(r => r.SectionId.Value).ToArray());
            Assert.All(read.Table.Rows, row => Assert.Equal(RamSectionMapStatus.Verified, row.Status));
            Assert.All(read.Table.Rows, row => Assert.True(row.IsVerified));
        }

        [Fact]
        public void LaFilaDeLaColumnaLlevaLosValoresAprobadosPorElOwner()
        {
            var row = Row(ShippedTable(), ColumnId);

            Assert.Equal("W 12X26", row.RamSectionName);
            Assert.Equal("W", row.RamSectionTable);
            Assert.Equal("United States", row.RamSectionCountry);
            Assert.Equal("ASTM-A992-GR50", row.MaterialId.Value);
            Assert.Equal("A992 Gr50", row.RamMaterial);
            AssertApprovedMaterial(row, ColumnSource);
        }

        [Fact]
        public void LaFilaDelBrazoLlevaLosValoresAprobadosPorElOwner()
        {
            var row = Row(ShippedTable(), ArmId);

            Assert.Equal("HSS_SQR 4X4X1_4", row.RamSectionName);
            Assert.Equal("HSS_SQR", row.RamSectionTable);
            Assert.Equal("United States", row.RamSectionCountry);
            Assert.Equal("ASTM-A500-GRC", row.MaterialId.Value);
            Assert.Equal("A500 GrC rectangular", row.RamMaterial);
            AssertApprovedMaterial(row, ArmSource);
        }

        private static void AssertApprovedMaterial(RamSectionMapRow row, string source)
        {
            Assert.NotNull(row.ExpectedMaterial.ElasticModulus);
            Assert.Equal(29000d, row.ExpectedMaterial.ElasticModulus.Value);
            Assert.Equal("ksi", row.ExpectedMaterial.ElasticModulus.Unit);
            Assert.Equal(0.3, row.ExpectedMaterial.PoissonRatio);
            Assert.Null(row.ExpectedMaterial.ShearModulus);
            Assert.NotNull(row.ExpectedMaterial.UnitWeight);
            Assert.Equal(490d, row.ExpectedMaterial.UnitWeight.Value);
            Assert.Equal("pcf", row.ExpectedMaterial.UnitWeight.Unit);
            Assert.Equal(source, row.ExpectedMaterial.Source);
            Assert.Equal(43, row.VerifiedBy.Length);
            Assert.Equal(VerifiedBy, row.VerifiedBy);
            Assert.Equal(VerifiedOn, row.VerifiedOn);
        }

        [Fact]
        public void RefCant01ConLaTablaDistribuidaEsValidoConLosMismosDiagnosticosQueConLaTablaVerifiedDeFixture()
        {
            var shipped = BuildRefCant01(ShippedTable());
            var reference = BuildRefCant01(F.RefTable());

            Assert.Equal(StructuralExportOutcome.Valid, reference.Outcome);
            Assert.Equal(StructuralExportOutcome.Valid, shipped.Outcome);
            Assert.NotNull(shipped.Model);
            Assert.DoesNotContain(shipped.Diagnostics, d => d.IsBlocking);
            Assert.Equal(Codes(reference), Codes(shipped));
        }

        [Fact]
        public void LaFabricaDaUnPaqueteValidoEnMemoriaConLaTablaDistribuidaSinEscribirLaCarpeta()
        {
            var table = ShippedTable();
            var folder = Path.Combine(Path.GetTempPath(), "rackcad-f4d-" + Guid.NewGuid().ToString("N"));
            var model = BuildRefCant01(table);
            Assert.Equal(StructuralExportOutcome.Valid, model.Outcome);
            Assert.False(Directory.Exists(folder));

            var package = RamBuildPackageFactory.CreateRamBuildPackage(model, new RamBuildPackageOptions(table, "1.0.0", folder, C.LoadsBytes()));

            Assert.Equal(StructuralExportOutcome.Valid, package.Outcome);
            Assert.NotNull(package.Package);
            Assert.False(Directory.Exists(folder));
            Assert.False(File.Exists(folder));
        }

        [Fact]
        public void CambiarUnCampoDeUnaFilaVerifiedCambiaLaHuellaDelPaquete()
        {
            var text = TableText();
            var marker = "\"verifiedOn\": \"" + VerifiedOn + "\"";
            var index = text.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(index >= 0, "La tabla distribuida no trae el campo verifiedOn esperado.");
            var changedText = text.Substring(0, index) + "\"verifiedOn\": \"2026-10-17\"" + text.Substring(index + marker.Length);

            var changedRead = RamSectionMapReader.Read(changedText);
            Assert.Equal(StructuralExportOutcome.Valid, changedRead.Outcome);
            var shipped = ShippedTable();
            var folder = Path.Combine(Path.GetTempPath(), "rackcad-f4d-" + Guid.NewGuid().ToString("N"));

            var original = RamBuildPackageFactory.CreateRamBuildPackage(
                BuildRefCant01(shipped), new RamBuildPackageOptions(shipped, "1.0.0", folder, C.LoadsBytes()));
            var changed = RamBuildPackageFactory.CreateRamBuildPackage(
                BuildRefCant01(changedRead.Table), new RamBuildPackageOptions(changedRead.Table, "1.0.0", folder, C.LoadsBytes()));

            Assert.Equal(StructuralExportOutcome.Valid, original.Outcome);
            Assert.Equal(StructuralExportOutcome.Valid, changed.Outcome);
            Assert.NotNull(original.Package);
            Assert.NotNull(changed.Package);
            Assert.False(string.IsNullOrEmpty(original.Package.AnalysisInputFingerprint));
            Assert.NotEqual(original.Package.AnalysisInputFingerprint, changed.Package.AnalysisInputFingerprint);
        }
    }
}
