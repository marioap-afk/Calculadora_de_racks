using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T3 — the neutral types of the readback (immutable, no identity of a provider) and the guards that keep them, and the new
    /// provider files, inside the rules of the foundation: G1, G2, G4, INV-11, INV-14, INV-16, D-01, D-13 and the one-owner rule of file I/O.
    /// </summary>
    public class StructuralAnalysisRamElementsDumpSourceTests
    {
        private const string AnalysisRoot = "src/RackCad.Application/StructuralAnalysis/";
        private const string ProviderRoot = AnalysisRoot + "Providers/RamElements/";
        private const string ReadbackFile = AnalysisRoot + "StructuralReadback.cs";
        private const string TestsRoot = "tests/RackCad.Tests/StructuralAnalysis/";

        private static readonly string[] NewProviderFiles =
        {
            "RamDumpDiagnosticCodes.cs", "RamDumpScriptTemplate.cs", "RamDumpPayload.cs", "RamDumpScriptFactory.cs", "RamDumpReader.cs", "RamTransferReaders.cs",
        };

        private static readonly Regex FileApis = new Regex(@"\b(?:File|Directory|FileStream|StreamReader|StreamWriter)\b", RegexOptions.CultureInvariant);

        private static readonly Regex SystemsReference = new Regex(@"RackCad\.Application\.Systems|\bSystems\b", RegexOptions.CultureInvariant);

        private static IReadOnlyList<(string Path, string Source)> Product() => StructuralAnalysisGuards.Files("src");

        private static string SourceOf(string path)
        {
            var matches = Product().Where(f => f.Path == path).ToList();

            Assert.True(matches.Count == 1, "No existe el archivo de producto " + path);
            return matches[0].Source;
        }

        // ---------------------------------------------------------------- the neutral file

        [Fact]
        public void ElArchivoDeTiposNeutralesDeLecturaExisteYDeclaraLosTiposDelContrato()
        {
            var code = StructuralAnalysisGuards.Code(SourceOf(ReadbackFile));

            foreach (var type in new[]
            {
                "StructuralReadbackNode", "StructuralReadbackMember", "StructuralReadbackPointLoad", "StructuralReadbackLoadCase",
                "StructuralReadbackCombination", "StructuralModelReadback", "StructuralTransferReadback", "StructuralPackageProvenance",
            })
            {
                Assert.Matches(@"public sealed class " + type + @"\b", code);
            }
        }

        [Fact]
        public void ElArchivoNeutralNoTieneIdentidadDeProveedorNiReferenciasASystemsOProvidersNiLiteralesDeConversion()
        {
            var source = SourceOf(ReadbackFile);

            Assert.Empty(StructuralAnalysisGuards.G1(ReadbackFile, source, new string[0]));
            Assert.Empty(StructuralAnalysisGuards.G2(ReadbackFile, source));
            Assert.Empty(StructuralAnalysisGuards.G3(ReadbackFile, source));
            Assert.Empty(StructuralAnalysisGuards.G4(ReadbackFile, source));
            Assert.Empty(StructuralAnalysisGuards.Inv11(ReadbackFile, source));
            Assert.Empty(StructuralAnalysisGuards.Inv16(ReadbackFile, source));
            Assert.Empty(StructuralAnalysisGuards.MaterialGrade(ReadbackFile, source));
        }

        [Fact]
        public void ElArchivoNeutralNoHaceEntradaYSalidaDeArchivos()
        {
            Assert.Empty(StructuralAnalysisRamElementsHardeningScans.NeutralFileIo(SourceOf(ReadbackFile)));
        }

        [Fact]
        public void ElArchivoNeutralSoloUsaBclYLosTiposNeutralesExistentes()
        {
            var usings = Regex.Matches(SourceOf(ReadbackFile), @"(?m)^using ([\w.]+);").Select(m => m.Groups[1].Value).ToList();

            Assert.Equal(new[] { "System", "System.Collections.Generic", "System.Collections.ObjectModel", "System.Linq" }.OrderBy(u => u, StringComparer.Ordinal).ToList(),
                usings.OrderBy(u => u, StringComparer.Ordinal).ToList());
        }

        // ---------------------------------------------------------------- the new provider files

        [Fact]
        public void LosArchivosNuevosDelProveedorExisten()
        {
            foreach (var name in NewProviderFiles)
            {
                Assert.NotEmpty(SourceOf(ProviderRoot + name));
            }
        }

        [Fact]
        public void LosArchivosNuevosDelProveedorNoUsanApisDeArchivosNiReferencianSystems()
        {
            foreach (var name in NewProviderFiles)
            {
                var code = StructuralAnalysisGuards.Code(SourceOf(ProviderRoot + name));

                Assert.False(FileApis.IsMatch(code), name + " usa una API de archivos");
                Assert.False(SystemsReference.IsMatch(code), name + " referencia Systems");
            }
        }

        [Fact]
        public void LosArchivosNuevosDelProveedorCumplenLasGuardasDeLaFundacion()
        {
            foreach (var name in NewProviderFiles)
            {
                var path = ProviderRoot + name;
                var source = SourceOf(path);

                Assert.Empty(StructuralAnalysisGuards.G4(path, source));
                Assert.Empty(StructuralAnalysisGuards.Inv11(path, source));
                Assert.Empty(StructuralAnalysisGuards.Inv16(path, source));
                Assert.Empty(StructuralAnalysisGuards.MaterialGrade(path, source));
                Assert.Empty(StructuralAnalysisGuards.G2(path, source));
            }
        }

        [Fact]
        public void LosArchivosNuevosDelProveedorViven_EnElEspacioDeNombresDeSuCarpeta()
        {
            foreach (var name in NewProviderFiles)
            {
                Assert.Contains("namespace RackCad.Application.StructuralAnalysis.Providers.RamElements", SourceOf(ProviderRoot + name));
            }

            Assert.Matches(@"(?m)^namespace RackCad\.Application\.StructuralAnalysis\s*$", SourceOf(ReadbackFile));
        }

        // ---------------------------------------------------------------- INV-14: the new tests do not name the module or the interpreter

        [Fact]
        public void NingunaPruebaNuevaNombraElModuloNiElInterpreteNiParteUnLiteral()
        {
            var mine = StructuralAnalysisGuards.Files("tests/RackCad.Tests")
                .Where(f => f.Path.StartsWith(TestsRoot + "StructuralAnalysisRamElementsDump", StringComparison.Ordinal))
                .ToList();

            Assert.True(mine.Count >= 7, "Se esperaban los archivos de pruebas nuevos de F4-T3: " + mine.Count);

            foreach (var file in mine)
            {
                Assert.Empty(StructuralAnalysisGuards.Inv14(file.Path, file.Source));
                Assert.Empty(StructuralAnalysisRamElementsHardeningScans.SplitLiterals(file.Source));
            }
        }

        [Fact]
        public void LaExencionDeInv14SigueSiendoExactamenteLasCuatroRutasExactas()
        {
            var probe = "var m = \"" + StructuralAnalysisRamElementsHardeningScans.ToolName() + "\";";
            var exempt = StructuralAnalysisGuards.Files("tests/RackCad.Tests")
                .Where(f => StructuralAnalysisGuards.Inv14(f.Path, probe).Count == 0)
                .Select(f => f.Path)
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToList();

            Assert.Equal(
                new[]
                {
                    TestsRoot + "StructuralAnalysisRamElementsAssumptionsTests.cs",
                    TestsRoot + "StructuralAnalysisRamElementsGuardTests.cs",
                    TestsRoot + "StructuralAnalysisRamElementsPackageTemplateTests.cs",
                    TestsRoot + "StructuralAnalysisSourceGuardTests.cs",
                },
                exempt);
        }

        // ---------------------------------------------------------------- immutability and validation of the neutral types

        private static IEnumerable<Type> ReadbackTypes() => typeof(StructuralModelReadback).Assembly.GetTypes()
            .Where(t => t.IsPublic && t.Namespace == "RackCad.Application.StructuralAnalysis"
                        && (t.Name.StartsWith("StructuralReadback", StringComparison.Ordinal) || t.Name == "StructuralModelReadback"
                            || t.Name == "StructuralTransferReadback" || t.Name == "StructuralPackageProvenance"))
            .Where(t => t.IsClass);

        [Fact]
        public void LosTiposNeutralesSonInmutablesSinSettersNiCamposPublicosEscribibles()
        {
            var types = ReadbackTypes().ToList();

            Assert.True(types.Count >= 8, "Se esperaban los tipos neutrales de lectura: " + types.Count);

            foreach (var type in types)
            {
                Assert.True(type.IsSealed, type.Name + " no es sealed");
                Assert.All(type.GetProperties(BindingFlags.Public | BindingFlags.Instance), p => Assert.True(p.SetMethod == null || !p.SetMethod.IsPublic, type.Name + "." + p.Name + " tiene setter publico"));
                Assert.All(type.GetFields(BindingFlags.Public | BindingFlags.Instance), f => Assert.True(f.IsInitOnly, type.Name + "." + f.Name + " es un campo escribible"));
            }
        }

        [Fact]
        public void LosTiposNeutralesNoExponenTiposDelProveedorEnSusMiembrosPublicos()
        {
            foreach (var type in ReadbackTypes())
            {
                var exposed = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.PropertyType)
                    .Concat(type.GetConstructors().SelectMany(c => c.GetParameters().Select(p => p.ParameterType)));

                Assert.All(exposed, t => Assert.DoesNotContain(".Providers.", t.FullName ?? string.Empty, StringComparison.Ordinal));
            }
        }

        private static StructuralPosition Origin() =>
            new StructuralPosition(StructuralLength.Zero, StructuralLength.Zero, StructuralLength.Zero);

        [Fact]
        public void UnNudoLeidoCopiaSusBanderasYLasExponeDeSoloLectura()
        {
            var flags = new List<bool> { true, false, false, false, true, true };
            var node = new StructuralReadbackNode("11", "N1", Origin(), flags);

            flags[0] = false;

            Assert.Equal(new[] { true, false, false, false, true, true }, node.SupportFlags.ToArray());
            Assert.False(node.SupportFlags is List<bool>);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void UnNudoLeidoExigeUnIdentificador(string id)
        {
            Assert.ThrowsAny<ArgumentException>(() => new StructuralReadbackNode(id, "N1", Origin(), new bool[6]));
        }

        [Fact]
        public void UnNudoLeidoExigeSeisBanderasYUnaPosicion()
        {
            Assert.Throws<ArgumentException>(() => new StructuralReadbackNode("11", "N1", Origin(), new bool[5]));
            Assert.Throws<ArgumentNullException>(() => new StructuralReadbackNode("11", "N1", null, new bool[6]));
            Assert.Throws<ArgumentNullException>(() => new StructuralReadbackNode("11", "N1", Origin(), null));
        }

        private static StructuralReadbackMember MemberWith(
            IEnumerable<double> factors = null, IEnumerable<bool> releases = null, int cardinal = 0, double twist = 0.0)
        {
            var direction = new StructuralDirection(1, 0, 0);

            return new StructuralReadbackMember(
                "21", "M1", "11", "12", "W 12X26", "A992 Gr50", twist, direction, direction, direction, cardinal, Origin(), Origin(),
                factors ?? new[] { 1.0, 1.0, 1.0, 1.0 }, StructuralReadbackAxialBehavior.Normal, 0, 0, releases ?? new bool[10]);
        }

        [Fact]
        public void UnMiembroLeidoExigeCuatroFactoresDiezLiberacionesYUnPuntoCardinalDeCeroANueve()
        {
            Assert.Equal(4, MemberWith().StiffnessFactors.Count);
            Assert.Equal(10, MemberWith().Releases.Count);
            Assert.Throws<ArgumentException>(() => MemberWith(factors: new[] { 1.0, 1.0, 1.0 }));
            Assert.Throws<ArgumentException>(() => MemberWith(releases: new bool[9]));
            Assert.Throws<ArgumentOutOfRangeException>(() => MemberWith(cardinal: 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => MemberWith(cardinal: -1));
        }

        [Fact]
        public void UnMiembroLeidoExigeValoresFinitos()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MemberWith(twist: double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => MemberWith(factors: new[] { 1.0, double.PositiveInfinity, 1.0, 1.0 }));
        }

        [Fact]
        public void UnaCargaLeidaLlevaSuDireccionSuFuerzaTipadaYSuDistanciaTipada()
        {
            var load = new StructuralReadbackPointLoad("21", StructuralReadbackLoadAxis.GlobalZ, StructuralForce.FromPoundsForce(-1000), StructuralLength.FromInches(25));

            Assert.Equal("21", load.MemberId);
            Assert.Equal(StructuralReadbackLoadAxis.GlobalZ, load.Axis);
            Assert.Equal(-1000, load.Value.PoundsForce);
            Assert.Equal(25, load.DistanceFromNodeI.Inches);
            Assert.ThrowsAny<ArgumentException>(() => new StructuralReadbackPointLoad(" ", StructuralReadbackLoadAxis.GlobalZ, load.Value, load.DistanceFromNodeI));
        }

        [Fact]
        public void UnCasoLeidoCopiaSusCargasYUnaCombinacionSusTerminos()
        {
            var load = new StructuralReadbackPointLoad("21", StructuralReadbackLoadAxis.GlobalZ, StructuralForce.FromPoundsForce(-1), StructuralLength.FromInches(1));
            var loads = new List<StructuralReadbackPointLoad> { load };
            var loadCase = new StructuralReadbackLoadCase("REF1", "OTHER", new StructuralDirection(0, 0, 0), loads);
            var terms = new List<StructuralReadbackCombinationTerm> { new StructuralReadbackCombinationTerm("REF1", 1.5) };
            var combination = new StructuralReadbackCombination("COMB1", terms);

            loads.Clear();
            terms.Clear();

            Assert.Single(loadCase.Loads);
            Assert.Single(combination.Terms);
            Assert.Throws<ArgumentException>(() => new StructuralReadbackLoadCase("REF1", "OTHER", new StructuralDirection(0, 0, 0), new StructuralReadbackPointLoad[] { null }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StructuralReadbackCombinationTerm("REF1", double.NaN));
        }

        [Fact]
        public void LaLecturaDeLaTransferenciaYLaProcedenciaDelManifiestoGuardanSusDatosYCopianLasDiferencias()
        {
            var differences = new List<string> { "node N1: coordinates" };
            var transfer = new StructuralTransferReadback(false, "abc", "1.0.0", differences);
            var provenance = new StructuralPackageProvenance("abc", "1.0.0");

            differences.Clear();

            Assert.False(transfer.Passed);
            Assert.Equal("abc", transfer.AnalysisInputFingerprint);
            Assert.Equal("1.0.0", transfer.AdapterVersion);
            Assert.Single(transfer.Differences);
            Assert.Equal("abc", provenance.AnalysisInputFingerprint);
            Assert.Equal("1.0.0", provenance.AdapterVersion);
            Assert.Throws<ArgumentNullException>(() => new StructuralTransferReadback(false, "abc", "1.0.0", null));
        }

        [Fact]
        public void UnaTransferenciaAprobadaNoPuedeLlevarDiferencias()
        {
            Assert.Throws<ArgumentException>(() => new StructuralTransferReadback(true, "abc", "1.0.0", new[] { "node N1: coordinates" }));
            Assert.True(new StructuralTransferReadback(true, "abc", "1.0.0", new string[0]).Passed);
        }
    }
}
