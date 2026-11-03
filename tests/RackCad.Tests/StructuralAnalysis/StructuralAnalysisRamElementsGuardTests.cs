using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T1 — the new source guards of the provider folder (CD-I65-F3-05) and the proof of the three authorised edits of the
    /// F1 guard. D-01: the adapter returns content and never touches files. D-13: its input is the neutral model, so it does not
    /// reference the systems. Same technique as <c>StructuralAnalysisSourceGuardTests</c>: pure scanners over comment-free code,
    /// proven on synthetic code before they sweep the real tree.
    /// </summary>
    public class StructuralAnalysisRamElementsGuardTests
    {
        private const string ProviderRoot = "src/RackCad.Application/StructuralAnalysis/Providers/RamElements/";
        private const string ProvidersRoot = "src/RackCad.Application/StructuralAnalysis/Providers/";
        private const string PackageTemplateTestsFile = "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageTemplateTests.cs";
        private const string GuardFile = "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs";
        private const string GuardTestsFile = "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs";
        private const string AssumptionsTestsFile = "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsAssumptionsTests.cs";

        private static readonly Regex FileApis = new Regex(
            @"\b(?:File|Directory|FileStream|StreamReader|StreamWriter)\b", RegexOptions.CultureInvariant);

        private static readonly Regex SystemsReference = new Regex(
            @"RackCad\.Application\.Systems|\bSystems\b", RegexOptions.CultureInvariant);

        private static string Normalize(string path) => path.Replace('\\', '/');

        /// <summary>D-01: the adapter returns content; it uses none of the file APIs.</summary>
        private static IReadOnlyList<string> NoFileApis(string path, string source)
        {
            if (!Normalize(path).StartsWith(ProviderRoot, StringComparison.Ordinal))
            {
                return new string[0];
            }

            return FileApis.Matches(StructuralAnalysisGuards.Code(source)).Select(m => "API de archivos '" + m.Value + "'").ToList();
        }

        /// <summary>D-13: the input of the adapter is the neutral model, not the systems.</summary>
        private static IReadOnlyList<string> NoSystems(string path, string source)
        {
            if (!Normalize(path).StartsWith(ProviderRoot, StringComparison.Ordinal))
            {
                return new string[0];
            }

            return SystemsReference.Matches(StructuralAnalysisGuards.Code(source)).Select(m => "referencia a Systems '" + m.Value + "'").ToList();
        }

        // ---------------------------------------------------------------- the sweeps look at the real provider folder

        [Fact]
        public void LaBarridaMiraDeVerdadLosArchivosDelProveedor()
        {
            var files = StructuralAnalysisGuards.Files("src").Where(f => f.Path.StartsWith(ProviderRoot, StringComparison.Ordinal)).ToList();

            Assert.True(files.Count >= 6, "La carpeta del proveedor apenas tiene archivos: " + files.Count);
            Assert.Contains(files, f => f.Path == ProviderRoot + "RamElementsTranslator.cs");
            Assert.Contains(files, f => f.Path == ProviderRoot + "RamSectionMaterialTable.cs");
            Assert.Contains(files, f => f.Path == ProviderRoot + "RamLocalAxes.cs");
        }

        [Fact]
        public void D01_EL_PROVEEDOR_NO_USA_FILE_DIRECTORY_FILESTREAM_STREAMREADER_NI_STREAMWRITER()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => NoFileApis(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de D-01:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void D13_EL_PROVEEDOR_NO_REFERENCIA_RACKCAD_APPLICATION_SYSTEMS()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => NoSystems(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de D-13:\n" + string.Join("\n", offenders));
        }

        // ---------------------------------------------------------------- the guards prove themselves

        [Theory]
        [InlineData("var text = File.ReadAllText(path);")]
        [InlineData("if (Directory.Exists(path)) { }")]
        [InlineData("using (var s = new FileStream(path, FileMode.Open)) { }")]
        [InlineData("using (var r = new StreamReader(path)) { }")]
        [InlineData("using (var w = new StreamWriter(path)) { }")]
        [InlineData("var ok = System.IO.File.Exists(path);")]
        public void D01_LA_GUARDA_DETECTA_CADA_API_DE_ARCHIVOS(string code)
        {
            Assert.NotEmpty(NoFileApis(ProviderRoot + "X.cs", code));
        }

        [Theory]
        [InlineData("// File.ReadAllText is never called here")]
        [InlineData("/// <summary>It uses no Directory and no StreamWriter.</summary>")]
        [InlineData("var profile = 1; var fileName = 2; var Filesystem = 3; var MyFile = 4; var Directories = 5;")]
        public void D01_LA_GUARDA_NO_MARCA_COMENTARIOS_NI_NOMBRES_PARECIDOS(string code)
        {
            Assert.Empty(NoFileApis(ProviderRoot + "X.cs", code));
        }

        [Fact]
        public void D01_LA_GUARDA_NO_MARCA_LO_QUE_QUEDA_FUERA_DE_LA_CARPETA_DEL_PROVEEDOR()
        {
            Assert.Empty(NoFileApis("src/RackCad.Application/StructuralAnalysis/StructuralLoadFileReader.cs", "var text = File.ReadAllText(path);"));
            Assert.Empty(NoFileApis(ProvidersRoot + "Other/X.cs", "var text = File.ReadAllText(path);"));
            Assert.Empty(NoFileApis("tests/RackCad.Tests/X.cs", "var text = File.ReadAllText(path);"));
        }

        [Theory]
        [InlineData("using RackCad.Application.Systems.Cantilever;")]
        [InlineData("using RackCad.Application.Systems;")]
        [InlineData("var source = Systems.Cantilever.CantileverStructuralModelSource.Idealise();")]
        public void D13_LA_GUARDA_DETECTA_CADA_REFERENCIA_A_SYSTEMS(string code)
        {
            Assert.NotEmpty(NoSystems(ProviderRoot + "X.cs", code));
        }

        [Theory]
        [InlineData("// RackCad.Application.Systems is never referenced here")]
        [InlineData("using RackCad.Application.StructuralAnalysis;")]
        [InlineData("var systemsCount = 1; var filesystem = 2; var Subsystems = 3;")]
        public void D13_LA_GUARDA_NO_MARCA_COMENTARIOS_NI_LO_PERMITIDO(string code)
        {
            Assert.Empty(NoSystems(ProviderRoot + "X.cs", code));
        }

        [Fact]
        public void D13_LA_GUARDA_NO_MARCA_LO_QUE_QUEDA_FUERA_DE_LA_CARPETA_DEL_PROVEEDOR()
        {
            Assert.Empty(NoSystems("src/RackCad.Application/Systems/Cantilever/X.cs", "using RackCad.Application.Systems.Cantilever;"));
        }

        // ---------------------------------------------------------------- the authorised edits of the F1 guard

        [Fact]
        public void BajoProvidersSoloExisteRamElements()
        {
            var providers = Path.Combine(C.RepoRoot(), ProvidersRoot.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar));

            Assert.True(Directory.Exists(providers));
            Assert.Equal(
                new[] { "RamElements" },
                Directory.GetFileSystemEntries(providers).Select(entry => Path.GetFileName(entry)).OrderBy(n => n, StringComparer.Ordinal).ToArray());
        }

        [Fact]
        public void INV14_EXIME_EXACTAMENTE_CUATRO_RUTAS_EXACTAS()
        {
            var code = "var m = \"ramelements\";";

            // the four exempt files: the guard, the package template tests, and these two (their synthetic samples name what the guard forbids)
            Assert.Empty(StructuralAnalysisGuards.Inv14(GuardFile, code));
            Assert.Empty(StructuralAnalysisGuards.Inv14(PackageTemplateTestsFile, code));
            Assert.Empty(StructuralAnalysisGuards.Inv14(GuardTestsFile, code));
            Assert.Empty(StructuralAnalysisGuards.Inv14(AssumptionsTestsFile, code));

            // nothing else is exempt: not another file, not another folder, not another case, not a backup
            Assert.NotEmpty(StructuralAnalysisGuards.Inv14("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsOtherTests.cs", code));
            Assert.NotEmpty(StructuralAnalysisGuards.Inv14("tests/RackCad.Tests/Other/StructuralAnalysisRamElementsPackageTemplateTests.cs", code));
            Assert.NotEmpty(StructuralAnalysisGuards.Inv14("tests/RackCad.Tests/Other/StructuralAnalysisRamElementsGuardTests.cs", code));
            Assert.NotEmpty(StructuralAnalysisGuards.Inv14(PackageTemplateTestsFile + ".bak", code));
            Assert.NotEmpty(StructuralAnalysisGuards.Inv14(GuardTestsFile + ".bak", code));
            Assert.NotEmpty(StructuralAnalysisGuards.Inv14("tests/RackCad.Tests/StructuralAnalysis/STRUCTURALANALYSISRAMELEMENTSASSUMPTIONSTESTS.CS", code));
            Assert.NotEmpty(StructuralAnalysisGuards.Inv14("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsMapReaderTests.cs", code));
        }

        [Fact]
        public void G1_LA_LISTA_DE_PERMISO_ES_LA_UNICA_RUTA_DEL_COMANDO_Y_EL_PROVEEDOR_NO_APARECE_FUERA_DE_SU_CARPETA()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => StructuralAnalysisGuards.G1(file.Path, file.Source, StructuralAnalysisGuards.G1ExactPathAllowList).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de G1 con la lista de permiso:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void G1_CON_LA_LISTA_VACIA_EL_UNICO_INFRACTOR_EN_SRC_ES_EL_ARCHIVO_DEL_COMANDO()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .Where(file => StructuralAnalysisGuards.G1(file.Path, file.Source, new string[0]).Count > 0)
                .Select(file => file.Path)
                .ToList();

            Assert.Equal(StructuralAnalysisGuards.G1ExactPathAllowList, offenders);
        }
    }
}
