using System;
using System.Linq;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G5 — los diagnósticos de la clase «Sintaxis y límites» del catálogo cerrado de V6 (P15.3).
    ///
    /// <para>
    /// Cada código se fija con su posición exacta. Los de enlace (<c>UnknownSymbol</c>, <c>ReservedName</c>,
    /// <c>UnknownNamespace</c>, <c>UnknownFunction</c>, <c>OperatorInName</c>…) NO se producen aquí: son de G6, y un
    /// parser que los adelantara sería una segunda autoridad. Tampoco hay mensajes: el núcleo devuelve códigos y
    /// datos, y el texto en español vive en una capa de texto posterior (P15.4).
    /// </para>
    /// </summary>
    public class ExpressionSyntaxDiagnosticsTests
    {
        [Theory]
        // EmptyExpression
        [InlineData("", ExpressionDiagnosticCode.EmptyExpression, 0, 0)]
        [InlineData("   ", ExpressionDiagnosticCode.EmptyExpression, 0, 3)]
        [InlineData("\t\r\n", ExpressionDiagnosticCode.EmptyExpression, 0, 3)]
        // UnexpectedCharacter: el '=' es marca de superficie, no gramática (P1.8)
        [InlineData("=1+2", ExpressionDiagnosticCode.UnexpectedCharacter, 0, 1)]
        [InlineData("1 @ 2", ExpressionDiagnosticCode.UnexpectedCharacter, 2, 1)]
        [InlineData("1 % 2", ExpressionDiagnosticCode.UnexpectedCharacter, 2, 1)]
        [InlineData("1 ; 2", ExpressionDiagnosticCode.UnexpectedCharacter, 2, 1)]
        [InlineData("1 + ]", ExpressionDiagnosticCode.UnexpectedCharacter, 4, 1)]
        [InlineData("}", ExpressionDiagnosticCode.UnexpectedCharacter, 0, 1)]
        [InlineData("1 + \U0001F600", ExpressionDiagnosticCode.UnexpectedCharacter, 4, 2)]
        [InlineData("'", ExpressionDiagnosticCode.UnexpectedCharacter, 0, 1)]
        [InlineData("A'", ExpressionDiagnosticCode.UnexpectedCharacter, 1, 1)]
        [InlineData("\"", ExpressionDiagnosticCode.UnexpectedCharacter, 0, 1)]
        // UnexpectedToken
        [InlineData("1 +", ExpressionDiagnosticCode.UnexpectedToken, 3, 0)]
        [InlineData("1 2", ExpressionDiagnosticCode.UnexpectedToken, 2, 1)]
        [InlineData("1 1", ExpressionDiagnosticCode.UnexpectedToken, 2, 1)]
        [InlineData("(1)(2)", ExpressionDiagnosticCode.UnexpectedToken, 3, 1)]
        [InlineData("2(3)", ExpressionDiagnosticCode.UnexpectedToken, 1, 1)]
        [InlineData("*1", ExpressionDiagnosticCode.UnexpectedToken, 0, 1)]
        [InlineData("1 + * 2", ExpressionDiagnosticCode.UnexpectedToken, 4, 1)]
        [InlineData("1, 2", ExpressionDiagnosticCode.UnexpectedToken, 1, 1)]
        [InlineData(",", ExpressionDiagnosticCode.UnexpectedToken, 0, 1)]
        [InlineData(".", ExpressionDiagnosticCode.UnexpectedToken, 0, 1)]
        [InlineData("F(1,)", ExpressionDiagnosticCode.UnexpectedToken, 4, 1)]
        [InlineData("()", ExpressionDiagnosticCode.UnexpectedToken, 1, 1)]
        [InlineData("A {B}", ExpressionDiagnosticCode.UnexpectedToken, 2, 3)]
        [InlineData("{A}(1)", ExpressionDiagnosticCode.UnexpectedToken, 3, 1)]
        [InlineData("{A}.B", ExpressionDiagnosticCode.UnexpectedToken, 3, 1)]
        [InlineData("Holgura General(2)", ExpressionDiagnosticCode.UnexpectedToken, 15, 1)]
        [InlineData("100 inches", ExpressionDiagnosticCode.UnexpectedToken, 4, 6)]
        [InlineData("100[mm", ExpressionDiagnosticCode.UnexpectedToken, 6, 0)]
        [InlineData("Rack.5", ExpressionDiagnosticCode.UnexpectedToken, 5, 1)]
        [InlineData("Rack.Frentes.Total", ExpressionDiagnosticCode.UnexpectedToken, 12, 1)]
        [InlineData("Holgura General.Frentes", ExpressionDiagnosticCode.UnexpectedToken, 15, 1)]
        [InlineData("Holgura  General", ExpressionDiagnosticCode.UnexpectedToken, 9, 7)]
        [InlineData("F(", ExpressionDiagnosticCode.UnexpectedToken, 2, 0)]
        [InlineData("100[mm + 2", ExpressionDiagnosticCode.UnexpectedToken, 7, 1)]
        [InlineData("MAX(100[mm, 4[in])", ExpressionDiagnosticCode.UnexpectedToken, 10, 1)]
        [InlineData("#" + Guid1 + "#" + Guid2, ExpressionDiagnosticCode.UnexpectedToken, 37, 37)]
        // UnbalancedParenthesis
        [InlineData("(1 + 2", ExpressionDiagnosticCode.UnbalancedParenthesis, 0, 1)]
        [InlineData("1 + 2)", ExpressionDiagnosticCode.UnbalancedParenthesis, 5, 1)]
        [InlineData("F(1", ExpressionDiagnosticCode.UnbalancedParenthesis, 1, 1)]
        [InlineData("((1)", ExpressionDiagnosticCode.UnbalancedParenthesis, 0, 1)]
        [InlineData("1 + )", ExpressionDiagnosticCode.UnbalancedParenthesis, 4, 1)]
        [InlineData("(1))", ExpressionDiagnosticCode.UnbalancedParenthesis, 3, 1)]
        [InlineData("MAX(A, (B)", ExpressionDiagnosticCode.UnbalancedParenthesis, 3, 1)]
        // UnterminatedName
        [InlineData("{Holgura", ExpressionDiagnosticCode.UnterminatedName, 0, 8)]
        [InlineData("{A}}", ExpressionDiagnosticCode.UnterminatedName, 0, 4)]
        [InlineData("1 + {A", ExpressionDiagnosticCode.UnterminatedName, 4, 2)]
        // InvalidNumber (P1.4)
        [InlineData(".5", ExpressionDiagnosticCode.InvalidNumber, 0, 2)]
        [InlineData("5.", ExpressionDiagnosticCode.InvalidNumber, 0, 2)]
        [InlineData("5..5", ExpressionDiagnosticCode.InvalidNumber, 0, 4)]
        [InlineData("5.5.5", ExpressionDiagnosticCode.InvalidNumber, 0, 5)]
        [InlineData("1e3", ExpressionDiagnosticCode.InvalidNumber, 0, 3)]
        [InlineData("1E3", ExpressionDiagnosticCode.InvalidNumber, 0, 3)]
        [InlineData("1e-3", ExpressionDiagnosticCode.InvalidNumber, 0, 4)]
        [InlineData("1e+3", ExpressionDiagnosticCode.InvalidNumber, 0, 4)]
        [InlineData("1e3x", ExpressionDiagnosticCode.InvalidNumber, 0, 4)]
        [InlineData("12abc", ExpressionDiagnosticCode.InvalidNumber, 0, 5)]
        [InlineData("0x1F", ExpressionDiagnosticCode.InvalidNumber, 0, 4)]
        [InlineData("1_000", ExpressionDiagnosticCode.InvalidNumber, 0, 5)]
        [InlineData("2 + .5", ExpressionDiagnosticCode.InvalidNumber, 4, 2)]
        [InlineData("(.5)", ExpressionDiagnosticCode.InvalidNumber, 1, 2)]
        // AmbiguousDecimalComma (P1.5)
        [InlineData("1,5", ExpressionDiagnosticCode.AmbiguousDecimalComma, 0, 3)]
        [InlineData("MAX(A, 1,5)", ExpressionDiagnosticCode.AmbiguousDecimalComma, 7, 3)]
        [InlineData("MAX(Holgura, 1,5)", ExpressionDiagnosticCode.AmbiguousDecimalComma, 13, 3)]
        [InlineData("F(1,2)", ExpressionDiagnosticCode.AmbiguousDecimalComma, 2, 3)]
        [InlineData("MAX(1.5,2)", ExpressionDiagnosticCode.AmbiguousDecimalComma, 4, 5)]
        [InlineData("1,234.5", ExpressionDiagnosticCode.AmbiguousDecimalComma, 0, 7)]
        [InlineData("1.234,5", ExpressionDiagnosticCode.AmbiguousDecimalComma, 0, 7)]
        [InlineData("1e3,5", ExpressionDiagnosticCode.AmbiguousDecimalComma, 0, 5)]
        [InlineData(".5,5", ExpressionDiagnosticCode.AmbiguousDecimalComma, 0, 4)]
        [InlineData("MAX(A1,5)", ExpressionDiagnosticCode.AmbiguousDecimalComma, 5, 3)]
        // UnknownUnit (P1.6)
        [InlineData("100[MM]", ExpressionDiagnosticCode.UnknownUnit, 3, 4)]
        [InlineData("100[cm]", ExpressionDiagnosticCode.UnknownUnit, 3, 4)]
        [InlineData("100[m]", ExpressionDiagnosticCode.UnknownUnit, 3, 3)]
        [InlineData("100[]", ExpressionDiagnosticCode.UnknownUnit, 3, 2)]
        [InlineData("100[m m]", ExpressionDiagnosticCode.UnknownUnit, 3, 5)]
        [InlineData("100[mm2]", ExpressionDiagnosticCode.UnknownUnit, 3, 5)]
        [InlineData("4[IN]", ExpressionDiagnosticCode.UnknownUnit, 1, 4)]
        [InlineData("20[Ft]", ExpressionDiagnosticCode.UnknownUnit, 2, 4)]
        [InlineData("100[1/2]", ExpressionDiagnosticCode.UnknownUnit, 3, 5)]
        [InlineData("100[\"in\"]", ExpressionDiagnosticCode.UnknownUnit, 3, 6)]
        // UnitNotAllowedHere (P1.6, §4.2 regla 10)
        [InlineData("Holgura[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 7, 4)]
        [InlineData("{Holgura}[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 9, 4)]
        [InlineData("Holgura#" + Guid1 + "[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 44, 4)]
        [InlineData("(2 + 3)[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 7, 4)]
        [InlineData("(A + B)[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 7, 4)]
        [InlineData("ABS(A)[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 6, 4)]
        [InlineData("F(A)[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 4, 4)]
        [InlineData("[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 0, 4)]
        [InlineData("2 + [mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 4, 4)]
        [InlineData("100[mm][in]", ExpressionDiagnosticCode.UnitNotAllowedHere, 7, 4)]
        [InlineData("A[cm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 1, 4)]
        [InlineData("Rack.Frentes[mm]", ExpressionDiagnosticCode.UnitNotAllowedHere, 12, 4)]
        // UnitSyntaxNotSupported (P1.6)
        [InlineData("100 mm", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 6)]
        [InlineData("100 mm + 2", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 6)]
        [InlineData("100 MM", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 6)]
        [InlineData("4 in", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 4)]
        [InlineData("100mm", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 5)]
        [InlineData("20ft", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 4)]
        [InlineData("12\"", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 3)]
        [InlineData("5'", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 2)]
        [InlineData("10'6\"", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 5)]
        [InlineData("1 1/8", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 5)]
        [InlineData("1 1 / 8", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 7)]
        [InlineData("2 * 1 1/8", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 4, 5)]
        [InlineData("-1 1/8", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 1, 5)]
        [InlineData("10'-6\"", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 6)]
        [InlineData("100 in(2)", ExpressionDiagnosticCode.UnitSyntaxNotSupported, 0, 6)]
        // InvalidQualifier (P1.7, P3.9): un fragmento de GUID nunca se parsea
        [InlineData("#12345678", ExpressionDiagnosticCode.InvalidQualifier, 0, 9)]
        [InlineData("#3f2b1c9e", ExpressionDiagnosticCode.InvalidQualifier, 0, 9)]
        [InlineData("Holgura#3f2b1c9e", ExpressionDiagnosticCode.InvalidQualifier, 7, 9)]
        [InlineData("#", ExpressionDiagnosticCode.InvalidQualifier, 0, 1)]
        [InlineData("#3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f4", ExpressionDiagnosticCode.InvalidQualifier, 0, 36)]
        [InlineData("#3f2b1c9e6d4a4f389b710c2a5e8d1f44", ExpressionDiagnosticCode.InvalidQualifier, 0, 33)]
        [InlineData("#" + Guid1 + "x", ExpressionDiagnosticCode.InvalidQualifier, 0, 38)]
        [InlineData("#{" + Guid1 + "}", ExpressionDiagnosticCode.InvalidQualifier, 0, 39)]
        [InlineData("#(" + Guid1 + ")", ExpressionDiagnosticCode.InvalidQualifier, 0, 39)]
        [InlineData("Holgura # " + Guid1, ExpressionDiagnosticCode.InvalidQualifier, 8, 38)]
        public void CADA_ERROR_TIENE_SU_CODIGO_Y_SU_POSICION_EXACTA(
            string text, ExpressionDiagnosticCode code, int start, int length)
        {
            var diagnostic = Assert.Single(ParseFails(text));

            Assert.Equal(code, diagnostic.Code);
            Assert.Equal(new SourceSpan(start, length), diagnostic.Span);
        }

        /// <summary>
        /// P1.4 excluye <c>NaN</c> e <c>Infinity</c> de los números, y eso vale también para lo que un numeral denota:
        /// cuatrocientos nueves no caben en un <c>double</c> finito y no pueden llegar al árbol como infinito.
        /// </summary>
        [Fact]
        public void UN_NUMERAL_QUE_NO_CABE_EN_UN_DOUBLE_FINITO_ES_INVALIDO()
        {
            var numeral = new string('9', 400);

            var diagnostic = Assert.Single(ParseFails(numeral + " + 1"));

            Assert.Equal(ExpressionDiagnosticCode.InvalidNumber, diagnostic.Code);
            Assert.Equal(new SourceSpan(0, 400), diagnostic.Span);

            var finito = Assert.IsType<NumberSyntax>(ParseOk(new string('9', 308)));
            Assert.False(double.IsInfinity(finito.Value));
        }

        /// <summary>
        /// P1.5: <c>MAX(Holgura, 1,5)</c> no puede convertirse en silencio en una llamada de tres argumentos. No hay
        /// árbol que lo afirme: el resultado es el error léxico y nada más.
        /// </summary>
        [Fact]
        public void LA_COMA_AMBIGUA_NUNCA_SE_CONVIERTE_EN_ARGUMENTOS()
        {
            var result = ExpressionParser.Parse("MAX(Holgura, 1,5)");

            Assert.False(result.Succeeded);
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(ExpressionDiagnosticCode.AmbiguousDecimalComma, diagnostic.Code);
            Assert.Throws<InvalidOperationException>(() => result.Syntax);
        }

        /// <summary>
        /// P1.5 se aplica literalmente: «una coma entre dos dígitos», pertenezcan los dígitos a un número, a un nombre
        /// o a lo que sea. <c>MAX(A1,5)</c> también es ambigua; con un espacio tras la coma es una llamada válida, y
        /// esa es la forma que emite el formatter.
        /// </summary>
        [Fact]
        public void LA_COMA_ENTRE_DOS_DIGITOS_ES_AMBIGUA_AUNQUE_EL_PRIMERO_SEA_DE_UN_NOMBRE()
        {
            var diagnostic = Assert.Single(ParseFails("MAX(A1,5)"));

            Assert.Equal(ExpressionDiagnosticCode.AmbiguousDecimalComma, diagnostic.Code);
            Assert.Equal(new SourceSpan(5, 3), diagnostic.Span);
            Assert.Equal("(call \"MAX\" (ref bare \"A1\") (num 5))", Dump(ParseOk("MAX(A1, 5)")));
        }

        // ================================================================ orden determinista (P15.7)

        [Theory]
        [InlineData("1,5 + 2,5", "AmbiguousDecimalComma@0+3, AmbiguousDecimalComma@6+3")]
        [InlineData("A[mm] + .5", "UnitNotAllowedHere@1+4, InvalidNumber@8+2")]
        [InlineData(".5 + A[mm]", "InvalidNumber@0+2, UnitNotAllowedHere@6+4")]
        [InlineData("1 1/8 + @", "UnitSyntaxNotSupported@0+5, UnexpectedCharacter@8+1")]
        [InlineData("100 mm + #123", "UnitSyntaxNotSupported@0+6, InvalidQualifier@9+4")]
        public void LOS_DIAGNOSTICOS_SE_ENTREGAN_POR_POSICION_SEA_CUAL_SEA_EL_ORDEN_EN_QUE_SE_DETECTAN(
            string text, string expected)
        {
            var result = ExpressionParser.Parse(text);

            Assert.False(result.Succeeded);
            Assert.Equal(expected, Describe(result));
            Assert.Equal(expected, Describe(ExpressionParser.Parse(text)));
        }

        /// <summary>
        /// A igual posición manda el código, en el orden del catálogo (P15.3), y después la longitud. G5 no tiene
        /// dueño: el dueño variable o propiedad lo añade el adaptador en G8+.
        /// </summary>
        [Fact]
        public void A_IGUAL_POSICION_ORDENA_EL_CODIGO_Y_DESPUES_LA_LONGITUD()
        {
            var result = ExpressionParseResult.Failure(
                "0123456789",
                new[]
                {
                    ExpressionDiagnostic.LimitExceeded(ExpressionLimitKind.SyntacticNesting, 64, new SourceSpan(2, 1)),
                    new ExpressionDiagnostic(ExpressionDiagnosticCode.UnknownUnit, new SourceSpan(2, 3)),
                    new ExpressionDiagnostic(ExpressionDiagnosticCode.UnknownUnit, new SourceSpan(2, 1)),
                    new ExpressionDiagnostic(ExpressionDiagnosticCode.UnexpectedToken, new SourceSpan(2, 4)),
                    new ExpressionDiagnostic(ExpressionDiagnosticCode.EmptyExpression, new SourceSpan(0, 0)),
                });

            Assert.Equal(
                "EmptyExpression@0+0, UnexpectedToken@2+4, UnknownUnit@2+1, UnknownUnit@2+3, LimitExceeded@2+1",
                Describe(result));
        }

        /// <summary>
        /// Los doce códigos de sintaxis y límites, con su valor y su orden de catálogo. G6 amplía el catálogo de P15.3 con
        /// los códigos de enlace, semánticos y de frontera AL FINAL, sin renumerar estos (ver
        /// <see cref="ExpressionDiagnosticCatalogTests"/>).
        /// </summary>
        [Fact]
        public void EL_CATALOGO_SINTACTICO_TIENE_EL_ORDEN_Y_LOS_CODIGOS_DE_V6()
        {
            Assert.Equal(
                new[]
                {
                    "EmptyExpression", "UnexpectedCharacter", "UnexpectedToken", "UnbalancedParenthesis",
                    "UnterminatedName", "InvalidNumber", "AmbiguousDecimalComma", "UnknownUnit",
                    "UnitNotAllowedHere", "UnitSyntaxNotSupported", "InvalidQualifier", "LimitExceeded",
                },
                Enum.GetValues(typeof(ExpressionDiagnosticCode))
                    .Cast<ExpressionDiagnosticCode>()
                    .Where(code => (int)code <= 12)
                    .OrderBy(code => (int)code)
                    .Select(code => code.ToString()));

            Assert.Equal(1, (int)ExpressionDiagnosticCode.EmptyExpression);
            Assert.Equal(12, (int)ExpressionDiagnosticCode.LimitExceeded);
        }

        // ================================================================ sin cascadas

        /// <summary>
        /// Un error léxico impide el análisis sintáctico, y un token inválido no arrastra errores de contexto: el
        /// usuario ve lo que escribió mal, no las consecuencias de ese error.
        /// </summary>
        [Theory]
        [InlineData("{A + 1,5", "UnterminatedName@0+8")]
        [InlineData("#123[mm]", "InvalidQualifier@0+4")]
        [InlineData(".5[cm]", "InvalidNumber@0+2")]
        [InlineData("1,5 mm", "AmbiguousDecimalComma@0+3")]
        [InlineData("1,5 + (2", "AmbiguousDecimalComma@0+3")]
        [InlineData("(1 2) + (3", "UnexpectedToken@3+1")]
        [InlineData("1 1/8 in", "UnitSyntaxNotSupported@0+5")]
        [InlineData("A[mm][in]", "UnitNotAllowedHere@1+4")]
        [InlineData("Holgura # " + Guid1 + " + 2", "InvalidQualifier@8+38")]
        [InlineData("#(" + Guid1 + ") * 2", "InvalidQualifier@0+39")]
        [InlineData("10'-6\" + 1", "UnitSyntaxNotSupported@0+6")]
        public void UN_ERROR_NO_ARRASTRA_ERRORES_EN_CASCADA(string text, string expected)
        {
            Assert.Equal(expected, Describe(ExpressionParser.Parse(text)));
        }

        /// <summary>
        /// Caracteres que no se ven o que se confunden con otros: un espacio duro, un acento combinante (P7.2: sin
        /// normalización Unicode) y el signo de infinito. Se construyen por su punto de código para que ningún editor
        /// pueda normalizarlos en el fuente de la prueba sin que se note.
        /// </summary>
        [Fact]
        public void LOS_CARACTERES_INVISIBLES_O_COMBINANTES_SON_INESPERADOS()
        {
            var espacioDuro = "1 +" + (char)0x00A0 + "2";
            var acentoCombinante = "e" + (char)0x0301;
            var infinito = ((char)0x221E).ToString();

            foreach (var (text, start) in new[] { (espacioDuro, 3), (acentoCombinante, 1), (infinito, 0) })
            {
                var diagnostic = Assert.Single(ParseFails(text));

                Assert.Equal(ExpressionDiagnosticCode.UnexpectedCharacter, diagnostic.Code);
                Assert.Equal(new SourceSpan(start, 1), diagnostic.Span);
            }
        }

        // ================================================================ datos, no mensajes (P15.1, P15.2, P15.4)

        [Theory]
        [InlineData("")]
        [InlineData("1 @ 2")]
        [InlineData("(1")]
        [InlineData("100[cm] + A[mm]")]
        [InlineData("#123")]
        public void TODO_DIAGNOSTICO_ES_UN_ERROR_DE_SINTAXIS_Y_LIMITES_CON_POSICION(string text)
        {
            foreach (var diagnostic in ParseFails(text))
            {
                Assert.Equal(ExpressionDiagnosticSeverity.Error, diagnostic.Severity);
                Assert.Equal(ExpressionDiagnosticClass.SyntaxAndLimits, diagnostic.Class);
                Assert.True(diagnostic.Span.HasValue, Describe(diagnostic));
                Assert.Null(diagnostic.Limit);
                Assert.Null(diagnostic.LimitMaximum);
            }
        }

        /// <summary>El núcleo no produce texto para el usuario: un diagnóstico no tiene ninguna propiedad de texto.</summary>
        [Fact]
        public void UN_DIAGNOSTICO_NO_LLEVA_MENSAJE()
        {
            Assert.DoesNotContain(
                typeof(ExpressionDiagnostic).GetProperties(),
                property => property.PropertyType == typeof(string));
        }
    }
}
