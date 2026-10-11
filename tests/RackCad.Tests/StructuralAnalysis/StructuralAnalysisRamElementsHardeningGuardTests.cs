using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The scanners of the I-65 F4-T1 guard fixes: pure functions over source text, proven on synthetic code before they sweep the tree.</summary>
    internal static class StructuralAnalysisRamElementsHardeningScans
    {
        private static readonly Regex FileIo = new Regex(
            @"\b(?:File|Directory)\s*\.|\b(?:FileStream|FileInfo|DirectoryInfo)\b", RegexOptions.CultureInvariant);

        // two or more string literals joined with '+'
        private static readonly Regex Chain = new Regex(
            @"""(?:[^""\\\n]|\\.)*""(?:\s*\+\s*""(?:[^""\\\n]|\\.)*"")+", RegexOptions.CultureInvariant);

        private static readonly Regex Literal = new Regex(@"""((?:[^""\\\n]|\\.)*)""", RegexOptions.CultureInvariant);

        private const string ProbePath = "tests/RackCad.Tests/Probe.cs";

        /// <summary>The file I/O of a source file: <c>File.</c>, <c>Directory.</c>, FileStream, FileInfo and DirectoryInfo, without comments.</summary>
        public static IReadOnlyList<string> NeutralFileIo(string source) =>
            FileIo.Matches(StructuralAnalysisGuards.Code(source)).Select(m => "E/S de archivos '" + m.Value + "'").ToList();

        /// <summary>
        /// The literals split to dodge INV-14: a chain of literals whose JOINED text is flagged by the INV-14 guard while no single piece is.
        /// The guard itself is the oracle, so this file needs to name none of the words it forbids.
        /// </summary>
        public static IReadOnlyList<string> SplitLiterals(string source)
        {
            var offenders = new List<string>();

            foreach (Match chain in Chain.Matches(StructuralAnalysisGuards.Code(source)))
            {
                var pieces = Literal.Matches(chain.Value).Select(m => m.Groups[1].Value).ToList();

                if (StructuralAnalysisGuards.Inv14(ProbePath, "\"" + string.Concat(pieces) + "\"").Count > 0
                    && pieces.All(piece => StructuralAnalysisGuards.Inv14(ProbePath, "\"" + piece + "\"").Count == 0))
                {
                    offenders.Add("literal partido: " + chain.Value);
                }
            }

            return offenders;
        }

        /// <summary>The name of the module of the analysis program, read from the generated script so that no test names it.</summary>
        public static string ToolName()
        {
            var match = Regex.Match(RamBuildScriptTemplate.Text, @"import (\w+) as rm");

            Assert.True(match.Success, "No se encontro el import de la plantilla.");
            return match.Groups[1].Value;
        }
    }

    /// <summary>
    /// I-65 F4-T1 — A65-F3-05 and A65-F3-06: no literal is split to dodge INV-14, the exemption of INV-14 is exactly four exact paths, and the
    /// neutral layer does file I/O only in the package writer (read from the source, not from the type names).
    /// </summary>
    public class StructuralAnalysisRamElementsHardeningGuardTests
    {
        private const string NeutralRoot = "src/RackCad.Application/StructuralAnalysis";
        private const string WriterFile = NeutralRoot + "/StructuralPackageWriter.cs";
        private const string GuardTestsFile = "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs";
        private const string AssumptionsFile = "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsAssumptionsTests.cs";

        private static readonly string[] Exempt =
        {
            "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsAssumptionsTests.cs",
            "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs",
            "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageTemplateTests.cs",
            "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs",
        };

        private static string SourceOf(string folder, string path) =>
            StructuralAnalysisGuards.Files(folder).Single(f => f.Path == path).Source;

        // ---------------------------------------------------------------- A65-F3-05

        [Fact]
        public void NingunArchivoDePruebasCoreParteUnaPalabraProhibidaEnVariosLiterales()
        {
            var offenders = StructuralAnalysisGuards.Files("tests/RackCad.Tests")
                .SelectMany(file => StructuralAnalysisRamElementsHardeningScans.SplitLiterals(file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Literales partidos:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void LosDosLiteralesQueEstabanPartidosSeEscribenEnteros()
        {
            Assert.Matches(@"""r[a]melements""", SourceOf("tests/RackCad.Tests", GuardTestsFile));
            Assert.Matches(@"""B[e]ntley""", SourceOf("tests/RackCad.Tests", AssumptionsFile));
        }

        [Fact]
        public void ElDetectorDeLiteralesPartidosVeLaPalabraPartidaYNoLaEnteraNiLosComentarios()
        {
            var tool = StructuralAnalysisRamElementsHardeningScans.ToolName();
            var left = tool.Substring(0, 5);
            var right = tool.Substring(5);

            var split = "var m = \"" + left + "\" + \"" + right + "\";";
            var chained = "var m = \"" + left + "\" + \"" + right.Substring(0, 3) + "\" + \"" + right.Substring(3) + "\";";
            var whole = "var m = \"" + tool + "\";";
            var comment = "// \"" + left + "\" + \"" + right + "\"\nvar n = \"a\" + \"b\";";

            Assert.Single(StructuralAnalysisRamElementsHardeningScans.SplitLiterals(split));
            Assert.Single(StructuralAnalysisRamElementsHardeningScans.SplitLiterals(chained));
            Assert.Empty(StructuralAnalysisRamElementsHardeningScans.SplitLiterals(whole));
            Assert.Empty(StructuralAnalysisRamElementsHardeningScans.SplitLiterals(comment));
        }

        [Fact]
        public void ElInv14EximeExactamenteCuatroRutasExactas()
        {
            var probe = "var m = \"" + StructuralAnalysisRamElementsHardeningScans.ToolName() + "\";";
            var exempt = StructuralAnalysisGuards.Files("tests/RackCad.Tests")
                .Where(file => StructuralAnalysisGuards.Inv14(file.Path, probe).Count == 0)
                .Select(file => file.Path)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();

            Assert.Equal(Exempt, exempt);
        }

        [Theory]
        [InlineData("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsAssumptionsTests.cs")]
        [InlineData("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs")]
        [InlineData("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageTemplateTests.cs")]
        [InlineData("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs")]
        public void CadaUnaDeLasCuatroRutasEximidasQuedaEximidaPorRutaExacta(string path)
        {
            var probe = "var m = \"" + StructuralAnalysisRamElementsHardeningScans.ToolName() + "\";";

            Assert.Empty(StructuralAnalysisGuards.Inv14(path, probe));
        }

        [Theory]
        [InlineData("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsAssumptionsTests.cs.bak")]
        [InlineData("tests/RackCad.Tests/Other/StructuralAnalysisRamElementsAssumptionsTests.cs")]
        [InlineData("tests/RackCad.Tests/Other/StructuralAnalysisRamElementsGuardTests.cs")]
        [InlineData("tests/RackCad.Tests/Other/StructuralAnalysisSourceGuardTests.cs")]
        [InlineData("tests/RackCad.Tests/StructuralAnalysis/STRUCTURALANALYSISRAMELEMENTSGUARDTESTS.CS")]
        [InlineData("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs")]
        [InlineData("tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsMapReaderTests.cs")]
        public void CualquierOtraRutaNoQuedaEximida(string path)
        {
            var probe = "var m = \"" + StructuralAnalysisRamElementsHardeningScans.ToolName() + "\";";

            Assert.NotEmpty(StructuralAnalysisGuards.Inv14(path, probe));
        }

        // ---------------------------------------------------------------- A65-F3-06

        [Theory]
        [InlineData("var text = File.ReadAllText(path);")]
        [InlineData("if (Directory.Exists(path)) { }")]
        [InlineData("using (var s = new FileStream(path, FileMode.Open)) { }")]
        [InlineData("var info = new FileInfo(path);")]
        [InlineData("var info = new DirectoryInfo(path);")]
        [InlineData("var ok = System.IO.File.Exists(path);")]
        public void ElDetectorDeEntradaYSalidaDeArchivosVeCadaApi(string code)
        {
            Assert.NotEmpty(StructuralAnalysisRamElementsHardeningScans.NeutralFileIo(code));
        }

        [Theory]
        [InlineData("// File.ReadAllText and Directory.Exists are never called here")]
        [InlineData("var fileName = 1; var Filesystem = 2; var MyFile = 3; var Directories = 4;")]
        [InlineData("var path = Path.Combine(a, b); var f = new StructuralPackageFile(name, bytes);")]
        [InlineData("public sealed class StructuralPackagePublisher { private readonly IStructuralPackageFileSystem _fileSystem; }")]
        public void ElDetectorDeEntradaYSalidaDeArchivosNoMarcaComentariosNiNombresParecidos(string code)
        {
            Assert.Empty(StructuralAnalysisRamElementsHardeningScans.NeutralFileIo(code));
        }

        [Fact]
        public void EnLaCapaNeutralLaEntradaYSalidaDeArchivosSoloApareceEnElEscritorDePaquetes()
        {
            var neutral = StructuralAnalysisGuards.Files(NeutralRoot)
                .Where(file => !file.Path.StartsWith(NeutralRoot + "/Providers/", StringComparison.Ordinal))
                .ToList();

            Assert.True(neutral.Count >= 10, "La barrida apenas mira la capa neutral: " + neutral.Count);
            Assert.Contains(neutral, file => file.Path == WriterFile);

            var offenders = neutral
                .Where(file => file.Path != WriterFile)
                .SelectMany(file => StructuralAnalysisRamElementsHardeningScans.NeutralFileIo(file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "E/S de archivos fuera del escritor:\n" + string.Join("\n", offenders));

            var inWriter = StructuralAnalysisRamElementsHardeningScans.NeutralFileIo(neutral.Single(file => file.Path == WriterFile).Source);

            Assert.Contains(inWriter, v => v.Contains("Directory", StringComparison.Ordinal));
            Assert.Contains(inWriter, v => v.Contains("FileStream", StringComparison.Ordinal));
        }

        [Fact]
        public void LaPruebaDeLaCapaNeutralYaNoBuscaWriterEnLosNombresDeTipoYBarreElCodigoFuente()
        {
            var source = SourceOf("tests/RackCad.Tests", "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisModelBuilderTests.cs");
            var code = StructuralAnalysisGuards.Code(source);

            Assert.DoesNotMatch(@"Contains\(""Writer""", code);
            Assert.Contains("NeutralFileIo", code);
        }
    }
}
