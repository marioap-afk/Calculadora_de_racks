using System.Linq;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G5 — las dos guardas del parser de V6 P1.9: longitud del texto y anidamiento sintáctico.
    ///
    /// <para>
    /// Solo protegen al parser y se comprueban ANTES de descender. NO son los límites normativos del árbol
    /// —nodos ≤ 256, profundidad del <c>BoundExpression</c> ≤ 24, argumentos ≤ 16—, que se validan tras el enlace en
    /// G6. Por eso una suma plana de 25 operandos PARSEA aquí aunque en G6 vaya a dar <c>LimitExceeded</c> por
    /// profundidad (T-V4-04): confundir las dos cosas es exactamente lo que estas pruebas impiden.
    /// </para>
    /// </summary>
    public class ExpressionParserGuardTests
    {
        private const string LetraAstral = "\U0001D49C";

        [Fact]
        public void LAS_GUARDAS_TIENEN_LOS_VALORES_DE_V6()
        {
            var texto = ExpressionParser.MaxTextLength;
            var anidamiento = ExpressionParser.MaxSyntacticNesting;

            Assert.Equal(4000, texto);
            Assert.Equal(64, anidamiento);

            // «Valor de implementación: inicial 64, nunca menor que 24» (P1.9): el texto canónico de cualquier árbol
            // legal tiene que volver a parsear.
            Assert.InRange(anidamiento, 24, int.MaxValue);
        }

        // ================================================================ texto

        [Fact]
        public void UN_TEXTO_VALIDO_DE_4000_CARACTERES_PASA_LA_GUARDA()
        {
            var text = "11" + Repeat("+1", 1999);
            Assert.Equal(4000, text.Length);

            Assert.Equal(ExpressionSyntaxKind.Binary, ParseOk(text).Kind);
        }

        [Fact]
        public void UN_NOMBRE_ENTRE_LLAVES_DE_4000_CARACTERES_PASA_LA_GUARDA()
        {
            var text = "{" + new string('a', 3998) + "}";

            var reference = Assert.IsType<ReferenceSyntax>(ParseOk(text));
            Assert.Equal(3998, reference.Name.Text.Length);
        }

        /// <summary>«Caracteres» son caracteres Unicode: una letra fuera del plano básico cuenta uno, aunque ocupe dos unidades UTF-16.</summary>
        [Fact]
        public void LA_GUARDA_CUENTA_CARACTERES_UNICODE_Y_NO_UNIDADES_UTF16()
        {
            var text = "{" + Repeat(LetraAstral, 3998) + "}";
            Assert.Equal(7998, text.Length);

            Assert.Equal(ExpressionSyntaxKind.Reference, ParseOk(text).Kind);
        }

        [Fact]
        public void UN_TEXTO_DE_4001_CARACTERES_DA_LIMITEXCEEDED()
        {
            var text = "11" + Repeat("+1", 1999) + "1";
            Assert.Equal(4001, text.Length);

            var diagnostic = Assert.Single(ParseFails(text));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.TextLength, diagnostic.Limit);
            Assert.Equal(4000, diagnostic.LimitMaximum);
            Assert.Equal(new SourceSpan(4000, 1), diagnostic.Span);
        }

        /// <summary>
        /// La guarda de texto va antes del lexer: 5000 caracteres inválidos dan UN diagnóstico de límite, no cinco mil
        /// caracteres inesperados, y la posición señala todo el exceso.
        /// </summary>
        [Fact]
        public void LA_GUARDA_DE_TEXTO_CORTA_ANTES_DE_LEER_EL_TEXTO()
        {
            var diagnostic = Assert.Single(ParseFails(new string('@', 5000)));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.TextLength, diagnostic.Limit);
            Assert.Equal(new SourceSpan(4000, 1000), diagnostic.Span);
        }

        [Fact]
        public void EL_EXCESO_UNICODE_SE_SENALA_DESDE_EL_CARACTER_4001()
        {
            var text = "{" + Repeat(LetraAstral, 3999) + "}";
            Assert.Equal(8000, text.Length);

            var diagnostic = Assert.Single(ParseFails(text));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(new SourceSpan(7999, 1), diagnostic.Span);
        }

        // ================================================================ anidamiento sintáctico

        public static TheoryData<string> Construcciones => new TheoryData<string> { "parentesis", "unario", "llamada" };

        private static string Anidar(string construccion, int niveles)
        {
            switch (construccion)
            {
                case "parentesis": return new string('(', niveles) + "1" + new string(')', niveles);
                case "unario": return new string('-', niveles) + "1";
                default: return Repeat("F(", niveles) + "1" + new string(')', niveles);
            }
        }

        [Theory]
        [MemberData(nameof(Construcciones))]
        public void SESENTA_Y_CUATRO_NIVELES_PASAN(string construccion)
        {
            ParseOk(Anidar(construccion, 64));
        }

        [Theory]
        [InlineData("parentesis", 64, 1)]
        [InlineData("unario", 64, 1)]
        [InlineData("llamada", 128, 2)]
        public void SESENTA_Y_CINCO_NIVELES_DAN_LIMITEXCEEDED_EN_EL_NIVEL_QUE_SOBRA(string construccion, int start, int length)
        {
            var diagnostic = Assert.Single(ParseFails(Anidar(construccion, 65)));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.SyntacticNesting, diagnostic.Limit);
            Assert.Equal(64, diagnostic.LimitMaximum);
            Assert.Equal(new SourceSpan(start, length), diagnostic.Span);
        }

        /// <summary>Paréntesis, unarios y llamadas suman al mismo contador.</summary>
        [Fact]
        public void LAS_TRES_CONSTRUCCIONES_SUMAN_AL_MISMO_ANIDAMIENTO()
        {
            ParseOk(Repeat("-(", 32) + "1" + new string(')', 32));

            var diagnostic = Assert.Single(ParseFails(Repeat("-(", 32) + "-1" + new string(')', 32)));
            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(new SourceSpan(64, 1), diagnostic.Span);

            // 21 × (llamada + unario + paréntesis) = 63 niveles, más el paréntesis final = 64.
            ParseOk(Repeat("F(-(", 21) + "(1)" + Repeat("))", 21));

            var llamadas = Assert.Single(ParseFails(Repeat("F(-(", 21) + "((1))" + Repeat("))", 21)));
            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, llamadas.Code);
            Assert.Equal(new SourceSpan(85, 1), llamadas.Span);
        }

        /// <summary>
        /// «Antes de descender»: un anidamiento de casi dos mil niveles dentro de la guarda de texto se corta en el
        /// nivel 65, sin recorrer el resto y sin desbordar la pila.
        /// </summary>
        [Theory]
        [InlineData("parentesis", 1999, 64, 1)]
        [InlineData("unario", 3999, 64, 1)]
        [InlineData("llamada", 1333, 128, 2)]
        public void LA_GUARDA_DE_ANIDAMIENTO_CORTA_ANTES_DE_DESCENDER(string construccion, int niveles, int start, int length)
        {
            var text = Anidar(construccion, niveles);
            Assert.InRange(text.Length, 3000, ExpressionParser.MaxTextLength);

            var diagnostic = Assert.Single(ParseFails(text));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.SyntacticNesting, diagnostic.Limit);
            Assert.Equal(new SourceSpan(start, length), diagnostic.Span);
        }

        // ================================================================ lo que la guarda NO es

        /// <summary>
        /// T-V4-04, parte de G5: la cadena plana de 25 operandos no tiene ningún anidamiento sintáctico y PARSEA. Su
        /// árbol asocia por la izquierda y tiene profundidad 25; en G6, tras el enlace, dará <c>LimitExceeded</c> por
        /// <c>MaxBoundExpressionDepth = 24</c>.
        /// </summary>
        [Fact]
        public void LA_SUMA_PLANA_DE_25_OPERANDOS_PARSEA_EN_G5()
        {
            var text = string.Join(" + ", Enumerable.Repeat("1", 25));

            var syntax = ParseOk(text);

            Assert.Equal(25, CaminoIzquierdo(syntax));
            Assert.Equal(24, CaminoIzquierdo(ParseOk(string.Join(" + ", Enumerable.Repeat("1", 24)))));
        }

        /// <summary>Tampoco dos mil operandos: la cadena binaria se construye sin recursión y no cuenta como anidamiento.</summary>
        [Fact]
        public void UNA_CADENA_BINARIA_LARGA_NO_CUENTA_COMO_ANIDAMIENTO()
        {
            var text = "1" + Repeat("+1", 1999);

            Assert.Equal(2000, CaminoIzquierdo(ParseOk(text)));
        }

        /// <summary>Argumentos ≤ 16 es un límite normativo de G6, no una guarda del parser.</summary>
        [Fact]
        public void EL_NUMERO_DE_ARGUMENTOS_NO_ES_UNA_GUARDA_DEL_PARSER()
        {
            var call = Assert.IsType<CallExpressionSyntax>(ParseOk("F(" + string.Join(", ", Enumerable.Repeat("1", 17)) + ")"));

            Assert.Equal(17, call.Arguments.Count);
        }

        /// <summary>Nodos en el camino raíz→hoja más a la izquierda, contados sin recursión (P1.9).</summary>
        private static int CaminoIzquierdo(ExpressionSyntax syntax)
        {
            var nodos = 1;

            while (syntax is BinaryExpressionSyntax binary)
            {
                syntax = binary.Left;
                nodos++;
            }

            Assert.Equal(ExpressionSyntaxKind.Number, syntax.Kind);
            return nodos;
        }
    }
}
