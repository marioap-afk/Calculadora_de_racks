using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T1 — the source guards of the structural-analysis interoperability foundation: G1 (provider identities stay
    /// inside their folder), G2 (neutral code does not reference systems or providers), G3 (Domain does not reference the
    /// analysis), G4 (no unit-conversion literals in the analysis), INV-11 (ADR-0017: no design ratios), INV-14 (Core tests
    /// do not name the external tool or an interpreter) and INV-16 (one owner of the pound mass and of standard gravity).
    ///
    /// <para>
    /// Same technique as <c>LengthUnitsAuthorityGuardTests</c>: they read the product as TEXT, WITHOUT comments, because the
    /// prohibition is on what is executed and documenting a rule does not violate it. Each guard is a pure function over
    /// (path, source) so it proves itself on synthetic code, and then sweeps the real tree. A text guard shows what the code
    /// does not say, never that what it says is right: the behaviour tests are the criterion.
    /// </para>
    /// <para>
    /// This file is the one exempt from the INV-14 sweep, because its synthetic samples must name what that guard forbids.
    /// </para>
    /// </summary>
    public class StructuralAnalysisSourceGuardTests
    {
        private const string GuardFile = "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs";
        private const string AnalysisRoot = "src/RackCad.Application/StructuralAnalysis/";
        private const string ProvidersRoot = AnalysisRoot + "Providers/";
        private const string RamProviderRoot = ProvidersRoot + "RamElements/";
        private const string SystemsRoot = "src/RackCad.Application/Systems/";
        private const string DomainRoot = "src/RackCad.Domain/";
        private const string MassFile = "src/RackCad.Application/Units/MassUnits.cs";
        private const string ForceFile = "src/RackCad.Application/Units/ForceUnits.cs";

        /// <summary>G1 allow-list of exact paths. EMPTY in F1: the command file of D-13 does not exist yet (F4 fixes it).</summary>
        private static readonly string[] G1ExactPathAllowList = new string[0];

        private static readonly string[] ProductFiles =
        {
            "StructuralLength.cs", "StructuralForce.cs", "StructuralArea.cs", "StructuralMomentOfInertia.cs",
            "StructuralModel.cs", "StructuralExportOutcome.cs", "StructuralDiagnostic.cs", "StructuralLoadFileReader.cs",
            "StructuralMaterialProfile.cs", "StructuralModelBuilder.cs", "StructuralModelCanonicalizer.cs",
            "StructuralFingerprints.cs",
        };

        // ================================================================ the sweeps look at the real tree

        [Fact]
        public void LA_BARRIDA_MIRA_DE_VERDAD_EL_ARBOL_DE_PRODUCTO_Y_DE_PRUEBAS()
        {
            var product = StructuralAnalysisGuards.Files("src");
            var tests = StructuralAnalysisGuards.Files("tests/RackCad.Tests");

            Assert.True(product.Count > 200, "La barrida de src apenas encontró archivos: " + product.Count);
            Assert.True(tests.Count > 100, "La barrida de tests apenas encontró archivos: " + tests.Count);
            Assert.Contains(tests, file => file.Path == GuardFile);

            foreach (var name in ProductFiles)
            {
                Assert.Contains(product, file => file.Path == AnalysisRoot + name);
            }

            Assert.Contains(product, file => file.Path == MassFile && file.Source.Contains("class MassUnits", StringComparison.Ordinal));
            Assert.Contains(product, file => file.Path == ForceFile && file.Source.Contains("class ForceUnits", StringComparison.Ordinal));
        }

        // ================================================================ G1 — provider identities

        [Fact]
        public void G1_ELARBOL_REAL_NO_NOMBRA_AL_PROVEEDOR_FUERA_DE_SU_CARPETA_Y_LA_LISTA_DE_PERMISO_ESTA_VACIA()
        {
            Assert.Empty(G1ExactPathAllowList);

            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => StructuralAnalysisGuards.G1(file.Path, file.Source, G1ExactPathAllowList).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de G1:\n" + string.Join("\n", offenders));
            Assert.False(Directory.Exists(Path.Combine(RepoRoot().FullName, ProvidersRoot.TrimEnd('/'))), "F1 no crea la carpeta Providers.");
        }

        [Theory]
        [InlineData(AnalysisRoot + "X.cs", "public sealed class RamElementsAdapter { }")]
        [InlineData(AnalysisRoot + "X.cs", "var name = \"ramelements\";")]
        [InlineData(AnalysisRoot + "X.cs", "var tool = \"RAM Elements\";")]
        [InlineData(AnalysisRoot + "X.cs", "public sealed class RamX { }")]
        [InlineData("src/RackCad.Domain/X.cs", "var v = RamNode.Create();")]
        [InlineData("src/RackCad.UI/X.cs", "using RamElements;")]
        [InlineData("src/RackCad.Plugin/X.cs", "Run(\"ramelements\");")]
        [InlineData(ProvidersRoot + "Other/X.cs", "class RamSection { }")]
        public void G1_LA_GUARDA_DETECTA_UN_ARCHIVO_NEUTRAL_CON_IDENTIDAD_DE_PROVEEDOR(string path, string code)
        {
            Assert.NotEmpty(StructuralAnalysisGuards.G1(path, code, G1ExactPathAllowList));
        }

        [Theory]
        [InlineData(RamProviderRoot + "X.cs", "public sealed class RamElementsAdapter { }")]
        [InlineData(AnalysisRoot + "X.cs", "// the adapter for RamElements lives elsewhere")]
        [InlineData(AnalysisRoot + "X.cs", "/// <summary>RAM Elements is only a consumer.</summary>")]
        [InlineData(AnalysisRoot + "X.cs", "var program = 1; var parameter = 2; var Ramp = 3; var framework = 4;")]
        [InlineData("tests/RackCad.Tests/X.cs", "var tool = \"RamElements\";")]
        [InlineData("tools/X/X.cs", "var tool = \"RamElements\";")]
        public void G1_LA_GUARDA_NO_MARCA_COMENTARIOS_LA_CARPETA_DEL_PROVEEDOR_NI_LO_QUE_QUEDA_FUERA_DE_ALCANCE(string path, string code)
        {
            Assert.Empty(StructuralAnalysisGuards.G1(path, code, G1ExactPathAllowList));
        }

        [Fact]
        public void G1_UNA_RUTA_EXACTA_DE_LA_LISTA_DE_PERMISO_QUEDA_EXENTA_PERO_SOLO_ELLA()
        {
            var allowed = new[] { "src/RackCad.Plugin/Commands/X.cs" };

            Assert.Empty(StructuralAnalysisGuards.G1("src/RackCad.Plugin/Commands/X.cs", "class RamElementsCommand { }", allowed));
            Assert.NotEmpty(StructuralAnalysisGuards.G1("src/RackCad.Plugin/Commands/Y.cs", "class RamElementsCommand { }", allowed));
        }

        // ================================================================ G2 — neutrality

        [Fact]
        public void G2_LO_NEUTRAL_NO_REFERENCIA_SYSTEMS_NI_PROVIDERS_Y_SYSTEMS_NO_REFERENCIA_PROVIDERS()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => StructuralAnalysisGuards.G2(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de G2:\n" + string.Join("\n", offenders));
        }

        [Theory]
        [InlineData(AnalysisRoot + "X.cs", "using RackCad.Application.Systems.Cantilever;")]
        [InlineData(AnalysisRoot + "X.cs", "var x = Systems.Cantilever.Foo;")]
        [InlineData(AnalysisRoot + "X.cs", "using RackCad.Application.StructuralAnalysis.Providers.RamElements;")]
        [InlineData(AnalysisRoot + "X.cs", "var p = Providers.Foo;")]
        [InlineData(SystemsRoot + "Cantilever/X.cs", "using RackCad.Application.StructuralAnalysis.Providers;")]
        [InlineData(SystemsRoot + "Selective/X.cs", "var p = Providers.Foo;")]
        public void G2_LA_GUARDA_DETECTA_CADA_REFERENCIA_PROHIBIDA(string path, string code)
        {
            Assert.NotEmpty(StructuralAnalysisGuards.G2(path, code));
        }

        [Theory]
        [InlineData(AnalysisRoot + "X.cs", "using System.Collections.Generic; // Systems and Providers are not referenced")]
        [InlineData(AnalysisRoot + "X.cs", "var systemsCount = 1; var filesystem = 2;")]
        [InlineData(ProvidersRoot + "RamElements/X.cs", "using RackCad.Application.Systems.Cantilever;")]
        [InlineData(SystemsRoot + "Cantilever/X.cs", "using RackCad.Application.StructuralAnalysis;")]
        public void G2_LA_GUARDA_NO_MARCA_LO_PERMITIDO(string path, string code)
        {
            Assert.Empty(StructuralAnalysisGuards.G2(path, code));
        }

        // ================================================================ G3 — Domain

        [Fact]
        public void G3_DOMAIN_NO_REFERENCIA_STRUCTURALANALYSIS()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => StructuralAnalysisGuards.G3(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de G3:\n" + string.Join("\n", offenders));
        }

        [Theory]
        [InlineData(DomainRoot + "Systems/X.cs", "using RackCad.Application.StructuralAnalysis;")]
        [InlineData(DomainRoot + "X.cs", "var m = StructuralAnalysis.Foo;")]
        public void G3_LA_GUARDA_DETECTA_LA_REFERENCIA_DESDE_DOMAIN(string path, string code)
        {
            Assert.NotEmpty(StructuralAnalysisGuards.G3(path, code));
        }

        [Theory]
        [InlineData(DomainRoot + "X.cs", "// StructuralAnalysis is not referenced from here")]
        [InlineData("src/RackCad.Application/X.cs", "using RackCad.Application.StructuralAnalysis;")]
        public void G3_LA_GUARDA_NO_MARCA_COMENTARIOS_NI_OTROS_PROYECTOS(string path, string code)
        {
            Assert.Empty(StructuralAnalysisGuards.G3(path, code));
        }

        // ================================================================ G4 — units

        [Fact]
        public void G4_EL_ANALISIS_NO_CONTIENE_LITERALES_NI_ROLES_DE_CONVERSION()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => StructuralAnalysisGuards.G4(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de G4:\n" + string.Join("\n", offenders));
        }

        [Theory]
        [InlineData(AnalysisRoot + "X.cs", "return value * 25.4;")]
        [InlineData(AnalysisRoot + "X.cs", "return value / 25.40d;")]
        [InlineData(AnalysisRoot + "X.cs", "var area = 645.16;")]
        [InlineData(AnalysisRoot + "X.cs", "var lb = 0.45359237;")]
        [InlineData(AnalysisRoot + "X.cs", "var g = 9.80665;")]
        [InlineData(AnalysisRoot + "X.cs", "var n = 4.4482216152605;")]
        [InlineData(AnalysisRoot + "X.cs", "var ft = 0.3048;")]
        [InlineData(AnalysisRoot + "X.cs", "public const double InchesToMillimeters = 1;")]
        [InlineData(AnalysisRoot + "X.cs", "var k = MillimetersPerInch;")]
        [InlineData(ProvidersRoot + "Other/X.cs", "return value * 25.4;")]
        public void G4_LA_GUARDA_DETECTA_CADA_LITERAL_Y_ROL_DE_CONVERSION(string path, string code)
        {
            Assert.NotEmpty(StructuralAnalysisGuards.G4(path, code));
        }

        [Theory]
        [InlineData(AnalysisRoot + "X.cs", "// 1 in = 25.4 mm, owned by the length authority")]
        [InlineData(AnalysisRoot + "X.cs", "var ratio = 125.45; var t = 25.41; var u = 1025.4; var v = 0.30481;")]
        [InlineData(AnalysisRoot + "X.cs", "var inches = LengthUnits.Authority.ToInches(value, unit);")]
        [InlineData("src/RackCad.Application/Units/ForceUnits.cs", "public const double StandardGravity = 9.80665;")]
        [InlineData("src/RackCad.Application/Systems/Cantilever/X.cs", "return value * 25.4;")]
        public void G4_LA_GUARDA_NO_MARCA_COMENTARIOS_NUMEROS_PARECIDOS_NI_LO_QUE_QUEDA_FUERA_DEL_ANALISIS(string path, string code)
        {
            Assert.Empty(StructuralAnalysisGuards.G4(path, code));
        }

        // ================================================================ INV-11 — ADR-0017

        [Fact]
        public void INV11_NINGUN_ARCHIVO_DE_PRODUCTO_CALCULA_UTILIZACION_RATIOS_NI_VEREDICTOS()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => StructuralAnalysisGuards.Inv11(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de INV-11:\n" + string.Join("\n", offenders));
        }

        [Theory]
        [InlineData("member_design")]
        [InlineData("MemberDesign")]
        [InlineData("Utilization")]
        [InlineData("InteractionRatio")]
        [InlineData("DesignRatio")]
        [InlineData("DemandCapacity")]
        [InlineData("CodeCheck")]
        [InlineData("UnityCheck")]
        public void INV11_LA_GUARDA_DETECTA_CADA_TOKEN_PROHIBIDO(string token)
        {
            Assert.NotEmpty(StructuralAnalysisGuards.Inv11("src/RackCad.Application/X.cs", "var r = Compute" + token + "(x);"));
            Assert.Empty(StructuralAnalysisGuards.Inv11("src/RackCad.Application/X.cs", "// " + token + " is out of scope\nvar r = 1;"));
        }

        [Fact]
        public void INV11_LA_GUARDA_ES_SENSIBLE_A_MAYUSCULAS()
        {
            Assert.Empty(StructuralAnalysisGuards.Inv11("src/RackCad.Application/X.cs", "var utilization = 1; var codecheck = 2;"));
        }

        // ================================================================ INV-14 — Core without the external tool or an interpreter

        [Fact]
        public void INV14_NINGUNA_PRUEBA_CORE_NOMBRA_LA_HERRAMIENTA_EXTERNA_NI_UN_INTERPRETE()
        {
            var offenders = StructuralAnalysisGuards.Files("tests/RackCad.Tests")
                .SelectMany(file => StructuralAnalysisGuards.Inv14(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de INV-14:\n" + string.Join("\n", offenders));
        }

        [Theory]
        [InlineData("tests/RackCad.Tests/X.cs", "var m = \"ramelements\";")]
        [InlineData("tests/RackCad.Tests/X.cs", "Process.Start(\"python\", args);")]
        [InlineData("tests/RackCad.Tests/X.cs", "Process.Start(\"PYTHON.exe\", args);")]
        [InlineData("tests/RackCad.Tests/Sub/X.cs", "var p = @\"C:\\Program Files\\Bentley\\RAM Elements\";")]
        [InlineData("tests/RackCad.Tests/X.cs", "var p = \"bentley\";")]
        public void INV14_LA_GUARDA_DETECTA_CADA_REFERENCIA_PROHIBIDA(string path, string code)
        {
            Assert.NotEmpty(StructuralAnalysisGuards.Inv14(path, code));
        }

        [Theory]
        [InlineData("tests/RackCad.Tests/X.cs", "// python and ramelements are never invoked here")]
        [InlineData("tests/RackCad.Tests/X.cs", "var m = \"RamElements\";")]
        [InlineData(GuardFile, "var m = \"ramelements\"; var p = \"python\"; var b = \"Bentley\";")]
        [InlineData("src/RackCad.Application/X.cs", "var p = \"python\";")]
        public void INV14_LA_GUARDA_NO_MARCA_COMENTARIOS_SU_PROPIO_ARCHIVO_NI_LO_QUE_QUEDA_FUERA_DE_ALCANCE(string path, string code)
        {
            Assert.Empty(StructuralAnalysisGuards.Inv14(path, code));
        }

        // ================================================================ INV-16 — one owner of the pound and of gn

        [Fact]
        public void INV16_LA_MASA_DE_LIBRA_Y_LA_GRAVEDAD_ESTANDAR_TIENEN_UN_SOLO_PROPIETARIO_EN_TODO_SRC()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => StructuralAnalysisGuards.Inv16(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de INV-16:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void INV16_LOS_PROPIETARIOS_DECLARAN_SU_LITERAL()
        {
            var files = StructuralAnalysisGuards.Files("src");

            Assert.Contains(files, file => file.Path == MassFile && StructuralAnalysisGuards.Code(file.Source).Contains("0.45359237", StringComparison.Ordinal));
            Assert.Contains(files, file => file.Path == ForceFile && StructuralAnalysisGuards.Code(file.Source).Contains("9.80665", StringComparison.Ordinal));
        }

        [Theory]
        [InlineData("src/RackCad.Application/StructuralSections/X.cs", "public static readonly double F = 0.45359237d / 0.3048d;")]
        [InlineData("src/RackCad.Application/Units/ForceUnits.cs", "public const double Lb = 0.45359237;")]
        [InlineData("src/RackCad.Application/StructuralAnalysis/X.cs", "var g = 9.80665;")]
        [InlineData("src/RackCad.Application/Units/MassUnits.cs", "public const double G = 9.80665;")]
        [InlineData("src/RackCad.Plugin/X.cs", "var lb = 0.45359237000;")]
        public void INV16_LA_GUARDA_DETECTA_UN_SEGUNDO_LITERAL_EN_CUALQUIER_ARCHIVO_DE_SRC(string path, string code)
        {
            Assert.NotEmpty(StructuralAnalysisGuards.Inv16(path, code));
        }

        [Theory]
        [InlineData("src/RackCad.Application/Units/MassUnits.cs", "public const double KilogramsPerPound = 0.45359237;")]
        [InlineData("src/RackCad.Application/Units/ForceUnits.cs", "public const double StandardGravity = 9.80665;")]
        [InlineData("src/RackCad.Application/StructuralSections/X.cs", "/// 1 lb = 0.45359237 kg\n// gn = 9.80665\nvar f = MassUnits.KilogramsPerPound;")]
        [InlineData("src/RackCad.Application/X.cs", "var a = 10.45359237; var b = 19.80665; var c = 0.453592371;")]
        [InlineData("tests/RackCad.Tests/X.cs", "var lb = 0.45359237;")]
        public void INV16_LA_GUARDA_NO_MARCA_AL_PROPIETARIO_COMENTARIOS_NI_LO_QUE_QUEDA_FUERA_DE_SRC(string path, string code)
        {
            Assert.Empty(StructuralAnalysisGuards.Inv16(path, code));
        }

        // ================================================================ D-07 — the material never comes from the catalogue grade

        [Fact]
        public void D07_EL_ANALISIS_NO_REFERENCIA_MATERIALGRADE()
        {
            var offenders = StructuralAnalysisGuards.Files("src")
                .SelectMany(file => StructuralAnalysisGuards.MaterialGrade(file.Path, file.Source).Select(v => file.Path + ": " + v))
                .ToList();

            Assert.True(offenders.Count == 0, "StructuralAnalysis no puede leer MaterialGrade:\n" + string.Join("\n", offenders));
        }

        [Theory]
        [InlineData(AnalysisRoot + "X.cs", "var grade = section.MaterialGrade;")]
        [InlineData(ProvidersRoot + "Other/X.cs", "var grade = definition.MaterialGrade;")]
        public void D07_LA_GUARDA_DETECTA_LA_LECTURA_DEL_GRADO(string path, string code)
        {
            Assert.NotEmpty(StructuralAnalysisGuards.MaterialGrade(path, code));
        }

        [Theory]
        [InlineData(AnalysisRoot + "X.cs", "// MaterialGrade is never read here")]
        [InlineData("src/RackCad.Application/StructuralSections/X.cs", "public string MaterialGrade { get; init; }")]
        public void D07_LA_GUARDA_NO_MARCA_COMENTARIOS_NI_EL_CATALOGO(string path, string code)
        {
            Assert.Empty(StructuralAnalysisGuards.MaterialGrade(path, code));
        }
    }

    /// <summary>The guard scanners: stateless and free of I/O (apart from the tree walk), so they can be proven on synthetic code.</summary>
    internal static class StructuralAnalysisGuards
    {
        private const string AnalysisRoot = "src/RackCad.Application/StructuralAnalysis/";
        private const string ProvidersRoot = AnalysisRoot + "Providers/";
        private const string RamProviderRoot = ProvidersRoot + "RamElements/";
        private const string SystemsRoot = "src/RackCad.Application/Systems/";
        private const string DomainRoot = "src/RackCad.Domain/";
        private const string MassFile = "src/RackCad.Application/Units/MassUnits.cs";
        private const string ForceFile = "src/RackCad.Application/Units/ForceUnits.cs";
        private const string GuardFile = "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs";

        private static readonly string[] G1Roots =
        {
            "src/RackCad.Domain/", "src/RackCad.Application/", "src/RackCad.UI/", "src/RackCad.Plugin/",
        };

        private static readonly Regex G1Tokens = new Regex(
            @"RamElements|ramelements|RAM Elements|\bRam[A-Z]", RegexOptions.CultureInvariant);

        private static readonly Regex G2Neutral = new Regex(@"\bSystems\b|\bProviders\b", RegexOptions.CultureInvariant);

        private static readonly Regex G2Systems = new Regex(@"\bProviders\b", RegexOptions.CultureInvariant);

        private static readonly Regex G3Tokens = new Regex(@"\bStructuralAnalysis\b", RegexOptions.CultureInvariant);

        private static readonly Regex G4Literals = new Regex(
            @"(?<![0-9.])(?:25\.40*|645\.160*|0\.453592370*|9\.806650*|4\.44822161526050*|0\.30480*)(?![0-9])",
            RegexOptions.CultureInvariant);

        private const string RoleNames =
            "InchesPerFoot|InchPerFoot|FeetPerInch|FootPerInch|FeetToInches|FootToInches|InchesToFeet|InchesToFoot"
            + "|MillimetersPerInch|MillimeterPerInch|InchesToMillimeters|InchToMillimeters|MillimetersToInches|MillimeterToInches";

        private static readonly Regex G4Roles = new Regex(@"\b(?:" + RoleNames + @")\b", RegexOptions.CultureInvariant);

        private static readonly Regex Inv11Tokens = new Regex(
            "member_design|MemberDesign|Utilization|InteractionRatio|DesignRatio|DemandCapacity|CodeCheck|UnityCheck",
            RegexOptions.CultureInvariant);

        private static readonly Regex Inv14Tool = new Regex("ramelements", RegexOptions.CultureInvariant);

        private static readonly Regex Inv14Other = new Regex(
            "python|bentley", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex PoundLiteral = new Regex(
            @"(?<![0-9.])0\.453592370*(?![0-9])", RegexOptions.CultureInvariant);

        private static readonly Regex GravityLiteral = new Regex(
            @"(?<![0-9.])9\.806650*(?![0-9])", RegexOptions.CultureInvariant);

        private static readonly Regex MaterialGradeToken = new Regex(@"\bMaterialGrade\b", RegexOptions.CultureInvariant);

        /// <summary>Every .cs file under a repository folder, as (path relative to the repo with '/', full text).</summary>
        internal static IReadOnlyList<(string Path, string Source)> Files(string folder)
        {
            var root = RepoRoot().FullName;
            var start = System.IO.Path.Combine(root, folder.Replace('/', System.IO.Path.DirectorySeparatorChar));
            var separator = System.IO.Path.DirectorySeparatorChar;

            return Directory
                .GetFiles(start, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(separator + "obj" + separator, StringComparison.Ordinal)
                            && !path.Contains(separator + "bin" + separator, StringComparison.Ordinal))
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => (System.IO.Path.GetRelativePath(root, path).Replace('\\', '/'), File.ReadAllText(path)))
                .ToList();
        }

        internal static IReadOnlyList<string> G1(string path, string source, IReadOnlyCollection<string> allowList)
        {
            var normalized = Normalize(path);

            if (!G1Roots.Any(root => normalized.StartsWith(root, StringComparison.Ordinal))
                || normalized.StartsWith(RamProviderRoot, StringComparison.Ordinal)
                || allowList.Contains(normalized, StringComparer.Ordinal))
            {
                return new string[0];
            }

            return G1Tokens.Matches(Code(source)).Select(m => "identidad de proveedor '" + m.Value + "' fuera de su carpeta").ToList();
        }

        internal static IReadOnlyList<string> G2(string path, string source)
        {
            var normalized = Normalize(path);
            Regex pattern = null;

            if (normalized.StartsWith(AnalysisRoot, StringComparison.Ordinal) && !normalized.StartsWith(ProvidersRoot, StringComparison.Ordinal))
            {
                pattern = G2Neutral;
            }
            else if (normalized.StartsWith(SystemsRoot, StringComparison.Ordinal))
            {
                pattern = G2Systems;
            }

            return pattern == null
                ? new string[0]
                : pattern.Matches(Code(source)).Select(m => "referencia prohibida '" + m.Value + "'").ToList();
        }

        internal static IReadOnlyList<string> G3(string path, string source)
        {
            return Normalize(path).StartsWith(DomainRoot, StringComparison.Ordinal)
                ? G3Tokens.Matches(Code(source)).Select(m => "Domain referencia '" + m.Value + "'").ToList()
                : new string[0];
        }

        internal static IReadOnlyList<string> G4(string path, string source)
        {
            if (!Normalize(path).StartsWith(AnalysisRoot, StringComparison.Ordinal))
            {
                return new string[0];
            }

            var code = Code(source);

            return G4Literals.Matches(code).Select(m => "literal de conversión '" + m.Value + "'")
                .Concat(G4Roles.Matches(code).Select(m => "rol de conversión de longitud '" + m.Value + "'"))
                .ToList();
        }

        internal static IReadOnlyList<string> Inv11(string path, string source)
        {
            return Normalize(path).StartsWith("src/", StringComparison.Ordinal)
                ? Inv11Tokens.Matches(Code(source)).Select(m => "vocabulario de diseño prohibido '" + m.Value + "'").ToList()
                : new string[0];
        }

        internal static IReadOnlyList<string> Inv14(string path, string source)
        {
            var normalized = Normalize(path);

            if (!normalized.StartsWith("tests/RackCad.Tests/", StringComparison.Ordinal)
                || string.Equals(normalized, GuardFile, StringComparison.Ordinal))
            {
                return new string[0];
            }

            var code = Code(source);

            return Inv14Tool.Matches(code).Concat(Inv14Other.Matches(code))
                .Select(m => "referencia prohibida en una prueba Core '" + m.Value + "'")
                .ToList();
        }

        internal static IReadOnlyList<string> Inv16(string path, string source)
        {
            var normalized = Normalize(path);

            if (!normalized.StartsWith("src/", StringComparison.Ordinal))
            {
                return new string[0];
            }

            var code = Code(source);
            var violations = new List<string>();

            if (!string.Equals(normalized, MassFile, StringComparison.Ordinal) && PoundLiteral.IsMatch(code))
            {
                violations.Add("la masa de libra solo se escribe en " + MassFile);
            }

            if (!string.Equals(normalized, ForceFile, StringComparison.Ordinal) && GravityLiteral.IsMatch(code))
            {
                violations.Add("la gravedad estándar solo se escribe en " + ForceFile);
            }

            return violations;
        }

        internal static IReadOnlyList<string> MaterialGrade(string path, string source)
        {
            return Normalize(path).StartsWith(AnalysisRoot, StringComparison.Ordinal)
                ? MaterialGradeToken.Matches(Code(source)).Select(m => "lee '" + m.Value + "'").ToList()
                : new string[0];
        }

        private static string Normalize(string path) => path.Replace('\\', '/');

        /// <summary>The CODE: without line or block comments, respecting string and character literals.</summary>
        internal static string Code(string source)
        {
            var builder = new StringBuilder(source.Length);
            var index = 0;

            while (index < source.Length)
            {
                var character = source[index];
                var next = index + 1 < source.Length ? source[index + 1] : '\0';

                if (character == '/' && next == '/')
                {
                    while (index < source.Length && source[index] != '\n')
                    {
                        index++;
                    }

                    continue;
                }

                if (character == '/' && next == '*')
                {
                    var end = source.IndexOf("*/", index + 2, StringComparison.Ordinal);
                    index = end < 0 ? source.Length : end + 2;
                    continue;
                }

                if (character == '"' || character == '\'')
                {
                    var verbatim = character == '"' && index > 0 && source[index - 1] == '@';
                    builder.Append(character);
                    index++;

                    while (index < source.Length)
                    {
                        var inner = source[index];
                        builder.Append(inner);
                        index++;

                        if (!verbatim && inner == '\\' && index < source.Length)
                        {
                            builder.Append(source[index]);
                            index++;
                            continue;
                        }

                        if (inner == character)
                        {
                            if (verbatim && index < source.Length && source[index] == '"')
                            {
                                builder.Append('"');
                                index++;
                                continue;
                            }

                            break;
                        }

                        if (!verbatim && inner == '\n')
                        {
                            break;
                        }
                    }

                    continue;
                }

                builder.Append(character);
                index++;
            }

            return builder.ToString();
        }
    }
}
