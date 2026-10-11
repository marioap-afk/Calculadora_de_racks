using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;
using K = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamElementsDiagnosticCodes;
using P = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamPackageDiagnosticCodes;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T2 — D-08, D-13 and INV-05: <c>CreateRamBuildPackage(model, options)</c> validates EVERYTHING before it returns content. A
    /// blocking diagnostic gives the outcome of CD-I65-F1-04 and NO content, never a partial one. The adapter does not touch files (D-01).
    /// </summary>
    public class StructuralAnalysisRamElementsPackageValidationTests
    {
        private static void AssertRefused(RamBuildPackageResult result, StructuralExportOutcome outcome, params string[] codes)
        {
            Assert.Equal(outcome, result.Outcome);
            Assert.Null(result.Package);
            Assert.Contains(result.Diagnostics, d => d.IsBlocking);

            foreach (var code in codes)
            {
                Assert.True(result.Diagnostics.Any(d => d.Code == code), "Falta el codigo " + code + "; hay: " + I.Describe(result));
            }
        }

        private static RamBuildPackageResult WithFolder(string folder)
        {
            var table = F.RefTable();
            return RamBuildPackageFactory.CreateRamBuildPackage(F.ValidRefModel(table), I.Options(table, folder: folder));
        }

        // ---------------------------------------------------------------- the table as it is delivered

        [Fact]
        public void ConLaTablaDelProductoTalComoSeEntregaElResultadoEsUnsupportedYSinContenido()
        {
            var shipped = RamSectionMapReader.Read(F.DataFileText());
            var fixtureTable = F.RefTable();
            var model = F.ValidRefModel(fixtureTable);

            Assert.Equal(StructuralExportOutcome.Valid, shipped.Outcome);

            var result = RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(shipped.Table));

            AssertRefused(result, StructuralExportOutcome.Unsupported, K.SectionRowUnverified);
        }

        [Fact]
        public void UnaFilaProposedNuncaSeTrataComoVerified()
        {
            var proposed = F.Table(F.Document(F.ColumnRow(), ProposedArmRow()));
            var model = F.ValidRefModel(F.RefTable());

            var result = RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(proposed));

            AssertRefused(result, StructuralExportOutcome.Unsupported, K.SectionRowUnverified);
        }

        private static System.Text.Json.Nodes.JsonObject ProposedArmRow()
        {
            var row = F.ArmRow();
            row["status"] = "proposed";
            row["verifiedBy"] = null;
            row["verifiedOn"] = null;
            return row;
        }

        [Fact]
        public void LaTablaSinLaSeccionDelModeloEsUnsupported()
        {
            var model = F.ValidRefModel(F.RefTable());
            var result = RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(F.ColumnTable()));

            AssertRefused(result, StructuralExportOutcome.Unsupported, K.SectionRowMissing);
        }

        [Fact]
        public void UnaFilaProposedQueElModeloNoUsaNoImpideElPaquete()
        {
            var table = F.Table(F.Document(F.ColumnRow(), F.ArmRow(), ProposedRow("AISC-W-W10X33", "W 10X33", "ASTM-A992-GR50", "A992 Gr50")));
            var result = RamBuildPackageFactory.CreateRamBuildPackage(F.ValidRefModel(table), I.Options(table));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Package);
        }

        private static System.Text.Json.Nodes.JsonObject ProposedRow(string sectionId, string ramName, string materialId, string ramMaterial)
        {
            var row = F.Row(sectionId, ramName, materialId, ramMaterial);
            row["status"] = "proposed";
            row["verifiedBy"] = null;
            row["verifiedOn"] = null;
            return row;
        }

        [Fact]
        public void UnaTablaConOtroMaterialParaLaSeccionEsInvalid()
        {
            var table = F.Table(F.Document(F.ColumnRow(), F.Row(F.ArmSection, F.ArmRamSection, "ASTM-OTHER", F.ArmRamMaterial)));
            var model = F.ValidRefModel(F.RefTable());

            AssertRefused(RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(table)), StructuralExportOutcome.Invalid, K.MaterialMismatch);
        }

        [Fact]
        public void UnaTablaConOtrasFilasQueLasDelModeloEsInvalidPorMapeoObsoletoYSinContenido()
        {
            var model = F.ValidRefModel(F.RefTable());

            AssertRefused(
                RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(I.ChangedTable())),
                StructuralExportOutcome.Invalid,
                P.MappingStale);
        }

        // ---------------------------------------------------------------- the model

        [Fact]
        public void UnResultadoDeModeloUnsupportedNoDaContenido()
        {
            var table = F.RefTable();
            var result = C.Build(C.Design(), profile: StructuralMaterialProfile.Empty);

            Assert.Equal(StructuralExportOutcome.Unsupported, result.Outcome);

            AssertRefused(
                RamBuildPackageFactory.CreateRamBuildPackage(result, I.Options(table)),
                StructuralExportOutcome.Unsupported,
                K.ModelNotValid);
        }

        [Fact]
        public void UnResultadoDeModeloValidoDaElMismoPaqueteQueElModelo()
        {
            var table = F.RefTable();
            var result = F.BuildRef(table);
            var direct = RamBuildPackageFactory.CreateRamBuildPackage(result.Model, I.Options(table));
            var viaResult = RamBuildPackageFactory.CreateRamBuildPackage(result, I.Options(table));

            Assert.Equal(StructuralExportOutcome.Valid, viaResult.Outcome);
            Assert.Equal(direct.Package.Files.Select(f => I.Sha256(f.GetContent())), viaResult.Package.Files.Select(f => I.Sha256(f.GetContent())));
        }

        [Fact]
        public void UnModeloConUnDiagnosticoBloqueanteEsInvalidYSinContenido()
        {
            var blocked = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 0, 100) },
                new[] { new F.SyntheticMember() },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0],
                new[] { StructuralDiagnostic.Blocking("TEST-BLOCK", StructuralExportOutcome.Invalid, "model") });

            AssertRefused(RamBuildPackageFactory.CreateRamBuildPackage(blocked, I.Options(F.ColumnTable())), StructuralExportOutcome.Invalid, K.ModelNotValid);
        }

        [Fact]
        public void UnaLiberacionEnElModeloEsUnsupportedYSinContenido()
        {
            var member = new F.SyntheticMember();
            member.Releases[3] = true;
            var model = F.Synthetic(
                new[] { F.Node("N1", 0, 0, 0), F.Node("N2", 0, 0, 100) },
                new[] { member },
                new[] { F.Support("N1", true, true, true, true, true, true) },
                new StructuralMemberPointLoad[0]);

            var result = RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(F.ColumnTable()));

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Package);
            Assert.Contains(result.Diagnostics, d => d.Code == K.ReleaseUnsupported && d.ImpliedOutcome == StructuralExportOutcome.Unsupported);
        }

        // ---------------------------------------------------------------- the options

        public static IEnumerable<object[]> InvalidFolders()
        {
            foreach (var folder in new[]
            {
                null,
                string.Empty,
                " ",
                "pkg",
                "RackCadPackages\\pkg",
                "\\pkg",
                "C:pkg",
                "C:",
                "C:\\",
                "C:\\pkg\\",
                "C:/pkg",
                "C:\\a/b",
                "/tmp/pkg",
                "C:\\a\\..\\b",
                "C:\\a\\.\\b",
                "C:\\a\\.",
                "C:\\..",
                "C:\\a\\\\b",
                "1:\\pkg",
                "C:\\pkg|x",
                "C:\\pk?g",
                "C:\\pk*g",
                "C:\\pk\"g",
                "C:\\pk<g",
                "C:\\a:b",
                "C:\\a\tb",
                "\\\\server",
                "\\\\server\\share",
                "\\\\server\\share\\",
                "\\\\\\server\\share\\pkg",
                "\\\\server\\\\share\\pkg",
                "//server/share/pkg",
            })
            {
                yield return new object[] { folder };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidFolders))]
        public void UnaCarpetaQueNoEsAbsolutaDeWindowsNiNormalizadaEsInvalidYSinContenido(string folder)
        {
            AssertRefused(WithFolder(folder), StructuralExportOutcome.Invalid, P.FolderInvalid);
            Assert.False(RamBuildPackageFactory.IsAbsoluteNormalisedFolder(folder));
        }

        [Theory]
        [InlineData("C:\\pkg")]
        [InlineData("c:\\RackCad\\REF 1")]
        [InlineData("D:\\a b\\c-d_e.f\\g")]
        [InlineData("C:\\Users\\\u00d1and\u00fa\\pkg")]
        [InlineData("\\\\server\\share\\pkg")]
        [InlineData("\\\\server\\share\\a\\b c")]
        public void UnaCarpetaAbsolutaDeWindowsNormalizadaDaContenido(string folder)
        {
            var result = WithFolder(folder);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Package);
            Assert.True(RamBuildPackageFactory.IsAbsoluteNormalisedFolder(folder));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public void UnaVersionDeAdaptadorEnBlancoEsInvalidYSinContenido(string version)
        {
            var table = F.RefTable();
            var result = RamBuildPackageFactory.CreateRamBuildPackage(F.ValidRefModel(table), I.Options(table, version: version));

            AssertRefused(result, StructuralExportOutcome.Invalid, P.AdapterVersionInvalid);
        }

        [Fact]
        public void SinBytesDelArchivoDeCargasEsInvalidYSinContenido()
        {
            var table = F.RefTable();
            var model = F.ValidRefModel(table);

            AssertRefused(
                RamBuildPackageFactory.CreateRamBuildPackage(model, new RamBuildPackageOptions(table, I.AdapterVersion, I.Folder, null)),
                StructuralExportOutcome.Invalid,
                P.LoadsMissing);
            AssertRefused(
                RamBuildPackageFactory.CreateRamBuildPackage(model, new RamBuildPackageOptions(table, I.AdapterVersion, I.Folder, new byte[0])),
                StructuralExportOutcome.Invalid,
                P.LoadsMissing);
        }

        [Fact]
        public void UnosBytesCuyoSha256NoEsElDelModeloSonInvalidYSinContenido()
        {
            AssertRefused(
                RamBuildPackageFactory.CreateRamBuildPackage(F.ValidRefModel(F.RefTable()), I.Options(F.RefTable(), loads: I.ChangedLoads())),
                StructuralExportOutcome.Invalid,
                P.LoadsHashMismatch);
        }

        [Fact]
        public void UnSoloByteDistintoEnLasCargasBastaParaRechazarlas()
        {
            var bytes = C.LoadsBytes();
            bytes[bytes.Length - 1] ^= 0x01;

            AssertRefused(
                RamBuildPackageFactory.CreateRamBuildPackage(F.ValidRefModel(F.RefTable()), I.Options(F.RefTable(), loads: bytes)),
                StructuralExportOutcome.Invalid,
                P.LoadsHashMismatch);
        }

        // ---------------------------------------------------------------- precedence and no partial content

        [Fact]
        public void LaPrecedenciaEsLaDeCdI65F104InvalidGanaAUnsupported()
        {
            var shipped = RamSectionMapReader.Read(F.DataFileText()).Table;
            var model = F.ValidRefModel(F.RefTable());

            var result = RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(shipped, folder: "relative"));

            AssertRefused(result, StructuralExportOutcome.Invalid, P.FolderInvalid, K.SectionRowUnverified);
            Assert.Contains(result.Diagnostics, d => d.ImpliedOutcome == StructuralExportOutcome.Unsupported);
        }

        [Fact]
        public void UnResultadoDeModeloNoValidoConCarpetaInvalidaSumaAmbosDiagnosticosYElResultadoMasGrave()
        {
            var table = F.RefTable();
            var unavailable = C.Build(C.Design(), kind: "Selective");

            Assert.NotEqual(StructuralExportOutcome.Valid, unavailable.Outcome);

            var result = RamBuildPackageFactory.CreateRamBuildPackage(unavailable, I.Options(table, folder: "relative"));

            Assert.Null(result.Package);
            Assert.True(result.Outcome >= unavailable.Outcome);
            Assert.Contains(result.Diagnostics, d => d.Code == P.FolderInvalid);
        }

        [Fact]
        public void NingunCasoDeRechazoDevuelveContenidoParcial()
        {
            var table = F.RefTable();
            var model = F.ValidRefModel(table);
            var refused = new[]
            {
                RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(table, folder: "relative")),
                RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(table, version: " ")),
                RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(table, loads: I.ChangedLoads())),
                RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(I.ChangedTable())),
                RamBuildPackageFactory.CreateRamBuildPackage(model, I.Options(F.ColumnTable())),
            };

            Assert.All(refused, result =>
            {
                Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
                Assert.Null(result.Package);
            });
        }

        [Fact]
        public void UnResultadoValidoNoTrajoDiagnosticosBloqueantesYLosTresArchivosVienenJuntos()
        {
            var result = I.Create();

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Diagnostics.Where(d => d.IsBlocking));
            Assert.Equal(3, result.Package.Files.Count);
            Assert.All(result.Package.Files, file => Assert.NotEmpty(file.GetContent()));
        }

        [Fact]
        public void ElResultadoNoSeDejaModificarDesdeFuera()
        {
            var result = I.Create();

            Assert.IsNotType<List<StructuralPackageFile>>(result.Package.Files);
            Assert.IsNotType<List<StructuralDiagnostic>>(result.Diagnostics);
        }

        [Fact]
        public void LosArgumentosNulosSonUnErrorDeProgramacion()
        {
            var table = F.RefTable();
            var model = F.ValidRefModel(table);

            Assert.Throws<ArgumentNullException>(() => RamBuildPackageFactory.CreateRamBuildPackage((StructuralModel)null, I.Options(table)));
            Assert.Throws<ArgumentNullException>(() => RamBuildPackageFactory.CreateRamBuildPackage((StructuralModelResult)null, I.Options(table)));
            Assert.Throws<ArgumentNullException>(() => RamBuildPackageFactory.CreateRamBuildPackage(model, null));
            Assert.Throws<ArgumentNullException>(() => new RamBuildPackageOptions(null, I.AdapterVersion, I.Folder, C.LoadsBytes()));
        }

        [Fact]
        public void LosCodigosDeDiagnosticoDelPaqueteSonEstablesYNoSeRepiten()
        {
            var codes = typeof(RamPackageDiagnosticCodes).GetFields().Where(f => f.IsLiteral).Select(f => (string)f.GetRawConstantValue()).ToList();

            Assert.Equal(6, codes.Count);
            Assert.Equal(codes.Count, codes.Distinct(StringComparer.Ordinal).Count());
            Assert.All(codes, code => Assert.StartsWith("RAM-PACKAGE-", code));
        }

        // ---------------------------------------------------------------- D-01: the adapter returns content, it does not write

        [Fact]
        public void LaCreacionDelPaqueteNoCreaNingunaCarpetaNiArchivoEnElDiscoNiEnLaRutaDeDestino()
        {
                var folder = "Z:\\RackCadNeverWritten\\" + Guid.NewGuid().ToString("N");
            var package = I.Package(folder: folder);

            Assert.NotNull(package);
            Assert.False(Directory.Exists(folder));
            Assert.False(File.Exists(folder));
        }
    }
}
