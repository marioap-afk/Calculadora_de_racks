using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using F = RackCad.Tests.StructuralAnalysisRamElementsFixtures;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;
using P = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamPackageDiagnosticCodes;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T1 — A65-F3-07 and A65-F3-08: the package exposes the folder that its script embeds (the one of the options, the one of the
    /// payload), and the folder rule rejects what Windows cannot create or silently changes: a name that ends in a dot or a space and the
    /// reserved device names, with or without extension, whatever the case. The valid folders of F3 stay valid.
    /// </summary>
    public class StructuralAnalysisRamElementsHardeningPackageTests
    {
        private static RamBuildPackageResult Result(string folder)
        {
            var table = F.RefTable();
            return RamBuildPackageFactory.CreateRamBuildPackage(F.ValidRefModel(table), I.Options(table, folder: folder));
        }

        // ---------------------------------------------------------------- A65-F3-07

        [Theory]
        [InlineData("C:\\RackCadPackages\\REF-CANT-01-S0")]
        [InlineData("D:\\Other Folder\\REF-CANT-01-S0")]
        [InlineData("\\\\server\\share\\pkg")]
        public void ElPaqueteExponeLaCarpetaEmbebidaIgualALaDeLasOpciones(string folder)
        {
            var result = Result(folder);

            Assert.NotNull(result.Package);
            Assert.Equal(folder, result.Package.PackageFolder);
        }

        [Theory]
        [InlineData("C:\\RackCadPackages\\REF-CANT-01-S0")]
        [InlineData("D:\\Other Folder\\REF-CANT-01-S0")]
        [InlineData("\\\\server\\share\\pkg")]
        public void ElCampoPackageFolderDeLaCargaUtilDelScriptEsLaCarpetaDeLasOpciones(string folder)
        {
            var result = Result(folder);

            Assert.NotNull(result.Package);

            using (var payload = I.PayloadOf(result.Package))
            {
                Assert.Equal(folder, payload.RootElement.GetProperty("packageFolder").GetString());
            }
        }

        [Fact]
        public void LaCarpetaDeLasOpcionesLaDelPaqueteYLaDeLaCargaUtilSonLaMismaEnLaReferencia()
        {
            var table = F.RefTable();
            var options = I.Options(table);
            var result = RamBuildPackageFactory.CreateRamBuildPackage(F.ValidRefModel(table), options);

            Assert.NotNull(result.Package);

            using (var payload = I.PayloadOf(result.Package))
            {
                var embedded = payload.RootElement.GetProperty("packageFolder").GetString();

                Assert.Equal(I.Folder, options.PackageFolder);
                Assert.Equal(options.PackageFolder, result.Package.PackageFolder);
                Assert.Equal(embedded, result.Package.PackageFolder);
            }
        }

        // ---------------------------------------------------------------- A65-F3-08

        public static IEnumerable<object[]> RejectedFolders()
        {
            foreach (var folder in new[]
            {
                "C:\\pkg.",
                "C:\\pkg ",
                "C:\\pkg..",
                "C:\\a.\\b",
                "C:\\a \\b",
                "C:\\a\\b.",
                "C:\\a\\b ",
                "C:\\CON",
                "C:\\con",
                "C:\\Prn",
                "C:\\AUX",
                "C:\\NUL",
                "C:\\a\\COM1",
                "C:\\a\\com9",
                "C:\\a\\Com5",
                "C:\\a\\LPT1",
                "C:\\a\\lpt9",
                "C:\\a\\Lpt3",
                "C:\\a\\CON.txt",
                "C:\\a\\nul.tar.gz",
                "C:\\a\\aux.x\\b",
                "C:\\a\\COM1.x",
                "C:\\a\\lpt2.log\\pkg",
                "\\\\server\\share\\AUX",
                "\\\\CON\\share\\pkg",
                "\\\\server\\prn\\pkg",
            })
            {
                yield return new object[] { folder };
            }
        }

        [Theory]
        [MemberData(nameof(RejectedFolders))]
        public void UnaCarpetaConUnSegmentoQueTerminaEnPuntoOEspacioOQueEsUnNombreReservadoEsInvalidaYSinContenido(string folder)
        {
            var result = Result(folder);

            Assert.False(RamBuildPackageFactory.IsAbsoluteNormalisedFolder(folder));
            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Package);
            Assert.Contains(result.Diagnostics, d => d.Code == P.FolderInvalid);
        }

        [Theory]
        [InlineData("C:\\RackCadPackages\\REF-CANT-01-S0")]
        [InlineData("D:\\Other Folder\\REF-CANT-01-S0")]
        [InlineData("c:\\RackCad\\REF 1")]
        [InlineData("D:\\a b\\c-d_e.f\\g")]
        [InlineData("\\\\server\\share\\a\\b c")]
        [InlineData("C:\\a\\COM10")]
        [InlineData("C:\\a\\CONSOLE")]
        [InlineData("C:\\a\\AUX1")]
        [InlineData("C:\\a\\LPT10")]
        [InlineData("C:\\a\\x.CON")]
        [InlineData("C:\\a\\b.c")]
        [InlineData("C:\\a\\.hidden")]
        public void LasCarpetasValidasDeF3SiguenSiendoValidasYDanContenido(string folder)
        {
            var result = Result(folder);

            Assert.True(RamBuildPackageFactory.IsAbsoluteNormalisedFolder(folder));
            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Package);
        }
    }
}
