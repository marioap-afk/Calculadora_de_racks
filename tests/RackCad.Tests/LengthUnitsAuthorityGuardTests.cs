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
    /// I-49 G6 — guarda NUEVA de V6 P28.4 y P9.7: autoridad de unidades acotada (CR-1; T-V4-10).
    ///
    /// <para>
    /// Es defensa SECUNDARIA: el criterio son las pruebas de comportamiento de <see cref="LengthUnitsTests"/> y de la
    /// evaluación. Prohíbe ÚNICAMENTE, fuera de la autoridad:
    /// <list type="number">
    /// <item><description>tablas de tokens o parsers de <c>[mm]</c>, <c>[in]</c> o <c>[ft]</c>;</description></item>
    /// <item><description>un literal semántico de conversión <c>25.4</c> duplicado;</description></item>
    /// <item><description>una segunda autoridad genérica pies↔pulgadas (o milímetros↔pulgadas): una constante o un
    /// conversor con ese papel declarado, salvo los alias de <c>StructuralSectionUnits</c> inicializados desde
    /// <c>LengthUnits</c>.</description></item>
    /// </list>
    /// Se apoya en nombres y papeles declarados; NO busca el número 12, así que las constantes de dominio de P9.6 quedan
    /// fuera por diseño. La guarda se prueba a sí misma: detecta cada violación en código sintético y no marca las
    /// constantes de dominio.
    /// </para>
    /// </summary>
    public class LengthUnitsAuthorityGuardTests
    {
        private const string AuthorityFile = "src/RackCad.Application/Units/LengthUnits.cs";
        private const string AliasFile = "src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs";

        private static IReadOnlyList<(string Path, string Source)> ProductSources()
        {
            var root = RepoRoot().FullName;
            var src = Path.Combine(root, "src");

            return Directory
                .GetFiles(src, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                            && !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .Select(path => (Path.GetRelativePath(root, path).Replace('\\', '/'), File.ReadAllText(path)))
                .ToList();
        }

        // ================================================================ la barrida mira de verdad

        [Fact]
        public void LA_BARRIDA_MIRA_DE_VERDAD()
        {
            var sources = ProductSources();

            Assert.True(sources.Count > 200, "La barrida apenas encontró archivos de producto: " + sources.Count);
            Assert.Contains(sources, source => source.Path == AuthorityFile && source.Source.Contains("class LengthUnits", StringComparison.Ordinal));
            Assert.Contains(sources, source => source.Path == AliasFile && source.Source.Contains("LengthUnits.", StringComparison.Ordinal));
            Assert.Contains(sources, source => source.Path.EndsWith("/SelectiveGeometryResolver.cs", StringComparison.Ordinal));
        }

        // ================================================================ el árbol real

        [Fact]
        public void NINGUN_ARCHIVO_DE_PRODUCTO_VIOLA_LA_AUTORIDAD_DE_UNIDADES()
        {
            var offenders = ProductSources()
                .SelectMany(source => UnitsAuthorityGuard.Violations(source.Path, source.Source).Select(violation => source.Path + ": " + violation))
                .ToList();

            Assert.True(offenders.Count == 0, "Violaciones de la autoridad de unidades:\n" + string.Join("\n", offenders));
        }

        // ================================================================ la guarda se prueba a sí misma (T-V4-10)

        [Theory]
        [InlineData("src/RackCad.UI/X.cs", "private static readonly string[] Units = { \"mm\", \"in\", \"ft\" };")]
        [InlineData("src/RackCad.Plugin/X.cs", "if (text.EndsWith(\"[mm]\")) { return value; }")]
        [InlineData("src/RackCad.Application/Expressions/X.cs", "var suffix = \"[ ft ]\";")]
        [InlineData("src/RackCad.Application/X.cs", "return value * 25.4;")]
        [InlineData("src/RackCad.Application/X.cs", "return value / 25.40d;")]
        [InlineData("src/RackCad.Application/Systems/X.cs", "public const double InchesPerFoot = 12;")]
        [InlineData("src/RackCad.Application/Systems/X.cs", "private static readonly double FeetToInches = 12d;")]
        [InlineData("src/RackCad.UI/X.cs", "public static double InchesToFeet(double inches) => inches / 12;")]
        [InlineData("src/RackCad.Domain/X.cs", "internal static double FeetToInches(double feet) { return feet * 12; }")]
        [InlineData("src/RackCad.Application/X.cs", "public const double InchesToMillimeters = LengthUnits.MillimetersPerInch;")]
        [InlineData(AliasFile, "private const double InchesPerFoot = 12d;")]
        [InlineData(AliasFile, "public const double InchesToMillimeters = 25.4;")]
        public void LA_GUARDA_DETECTA_CADA_VIOLACION(string path, string code)
        {
            Assert.NotEmpty(UnitsAuthorityGuard.Violations(path, code));
        }

        [Theory]
        [InlineData("src/RackCad.Application/Systems/Selective/X.cs", "public const double FootInches = 12.0;")]
        [InlineData("src/RackCad.Application/Systems/Dynamic/X.cs", "public const double CommercialFoot = 12.0;")]
        [InlineData("src/RackCad.Application/Systems/Cantilever/X.cs", "return Math.Atan(slopeRisePer12 / 12.0);")]
        [InlineData("src/RackCad.Application/RackFrames/X.cs", "Units = \"in\",")]
        [InlineData("src/RackCad.Application/X.cs", "// 1 in = 25.4 mm, y [mm] se lee en la autoridad")]
        [InlineData("src/RackCad.Application/X.cs", "/// <summary>Exact: 1 in = 25.4 mm.</summary>")]
        [InlineData("src/RackCad.Application/X.cs", "var ratio = 125.45; var t = 25.41; var u = 1025.4;")]
        [InlineData("src/RackCad.Application/X.cs", "var inches = feet * LengthUnits.InchesPerFoot;")]
        [InlineData(AliasFile, "public const double InchesToMillimeters = LengthUnits.MillimetersPerInch;")]
        [InlineData(AliasFile, "private const double InchesPerFoot = LengthUnits.InchesPerFoot;")]
        [InlineData(AuthorityFile, "public const double MillimetersPerInch = 25.4; case \"mm\": case \"in\": case \"ft\":")]
        public void LA_GUARDA_NO_MARCA_CONSTANTES_DE_DOMINIO_NI_USOS_NI_LA_PROPIA_AUTORIDAD(string path, string code)
        {
            Assert.Empty(UnitsAuthorityGuard.Violations(path, code));
        }

        /// <summary>El escáner de la guarda: sin estado y sin E/S, para poder probarlo sobre código sintético.</summary>
        internal static class UnitsAuthorityGuard
        {
            private static readonly Regex UnitTokenLiteral =
                new Regex("\"(mm|in|ft)\"", RegexOptions.CultureInvariant);

            private static readonly Regex BracketedUnitInString =
                new Regex("\"[^\"\\r\\n]*\\[\\s*(mm|in|ft)\\s*\\][^\"\\r\\n]*\"", RegexOptions.CultureInvariant);

            private static readonly Regex MillimetersPerInchLiteral =
                new Regex(@"(?<![0-9.])25\.40*(?![0-9])", RegexOptions.CultureInvariant);

            private const string RoleNames =
                "InchesPerFoot|InchPerFoot|FeetPerInch|FootPerInch|FeetToInches|FootToInches|InchesToFeet|InchesToFoot"
                + "|MillimetersPerInch|MillimeterPerInch|InchesToMillimeters|InchToMillimeters|MillimetersToInches|MillimeterToInches";

            private static readonly Regex RoleField = new Regex(
                @"\b(?:const|readonly|static)\b[^;=\r\n(]*\b(?<name>" + RoleNames + @")\b\s*(?<tail>=\s*[^;\r\n]*|;|\{)",
                RegexOptions.CultureInvariant);

            private static readonly Regex RoleMethod = new Regex(
                @"\b(?:double|float|decimal|int|long)\s+(?<name>" + RoleNames + @")\s*\(",
                RegexOptions.CultureInvariant);

            private static readonly Regex AliasInitializer = new Regex(
                @"^=\s*LengthUnits\.(?<target>MillimetersPerInch|InchesPerFoot)\s*$",
                RegexOptions.CultureInvariant);

            internal static IReadOnlyList<string> Violations(string path, string source)
            {
                var normalized = path.Replace('\\', '/');

                if (string.Equals(normalized, AuthorityFile, StringComparison.Ordinal))
                {
                    return Array.Empty<string>();
                }

                var code = WithoutComments(source);
                var violations = new List<string>();

                if (UnitTokenLiteral.Matches(code).Select(match => match.Groups[1].Value).Distinct(StringComparer.Ordinal).Count() == 3)
                {
                    violations.Add("tabla de tokens de unidad mm/in/ft fuera de la autoridad");
                }

                if (BracketedUnitInString.IsMatch(code))
                {
                    violations.Add("token de unidad entre corchetes fuera de la autoridad");
                }

                if (MillimetersPerInchLiteral.IsMatch(code))
                {
                    violations.Add("literal de conversión 25.4 duplicado");
                }

                foreach (Match match in RoleField.Matches(code))
                {
                    var name = match.Groups["name"].Value;
                    var alias = AliasInitializer.Match(match.Groups["tail"].Value.Trim());
                    var isAllowedAlias = string.Equals(normalized, AliasFile, StringComparison.Ordinal)
                                         && alias.Success
                                         && ((name == "InchesToMillimeters" && alias.Groups["target"].Value == "MillimetersPerInch")
                                             || (name == "InchesPerFoot" && alias.Groups["target"].Value == "InchesPerFoot"));

                    if (!isAllowedAlias)
                    {
                        violations.Add("segunda autoridad genérica de conversión: " + name);
                    }
                }

                foreach (Match match in RoleMethod.Matches(code))
                {
                    violations.Add("conversor genérico fuera de la autoridad: " + match.Groups["name"].Value);
                }

                return violations;
            }

            /// <summary>
            /// El CÓDIGO: sin comentarios de línea ni de bloque, respetando los literales de cadena y de carácter, porque la
            /// prohibición es sobre lo que se ejecuta y documentar la regla («1 in = 25.4 mm») no la viola.
            /// </summary>
            private static string WithoutComments(string source)
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
}
