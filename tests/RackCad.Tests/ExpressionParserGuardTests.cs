using System;
using System.Linq;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 — las dos guardas del parser: complejidad sintáctica (tokens) y anidamiento sintáctico.
    ///
    /// <para>
    /// G5 implementó la guarda de recurso de V6 P1.9 como una longitud de texto de 4000 caracteres. El Amendment A1
    /// (ADR-0040 D8) la sustituyó por una guarda de tokens, porque con nombres legales sin longitud máxima ninguna guarda
    /// finita de caracteres es compatible con P2.5. G6 reanudado empieza aquí: sin límite de validez por caracteres
    /// (A1.1); <c>MaxSyntacticTokens</c> inicial 4096 y nunca menor que 6 × 256 (A1.2); la granularidad normativa de A1.3;
    /// y el corte durante el lexing con un único diagnóstico (A1.4).
    /// </para>
    /// <para>
    /// Ninguna de las dos guardas es un límite normativo del árbol —nodos ≤ 256, profundidad del <c>BoundExpression</c>
    /// ≤ 24, argumentos ≤ 16—, que se validan tras el enlace (A1.5). Por eso una suma plana de 25 operandos PARSEA aunque
    /// después dé <c>LimitExceeded</c> por profundidad (T-V4-04): confundir las dos cosas es exactamente lo que estas
    /// pruebas impiden.
    /// </para>
    /// </summary>
    public class ExpressionParserGuardTests
    {
        private const string LetraAstral = "\U0001D49C";

        [Fact]
        public void LAS_GUARDAS_TIENEN_LOS_VALORES_DE_V6_CON_A1()
        {
            var tokens = ExpressionParser.MaxSyntacticTokens;
            var anidamiento = ExpressionParser.MaxSyntacticNesting;

            Assert.Equal(4096, tokens);
            Assert.Equal(64, anidamiento);

            // A1.2: «nunca puede bajar de 6 × máximo normativo de nodos», que con nodos ≤ 256 vale 1536.
            Assert.InRange(tokens, 6 * 256, int.MaxValue);

            // «Valor de implementación: inicial 64, nunca menor que 24» (P1.9): el texto canónico de cualquier árbol
            // legal tiene que volver a parsear.
            Assert.InRange(anidamiento, 24, int.MaxValue);
        }

        /// <summary>
        /// A1.1 y A1 §6, opción A: <c>TextLength</c> sale del contrato productivo, no se emite y su valor numérico no se
        /// reutiliza para otro tipo de límite.
        /// </summary>
        [Fact]
        public void TEXTLENGTH_Y_LA_GUARDA_DE_CARACTERES_YA_NO_EXISTEN()
        {
            Assert.DoesNotContain("TextLength", Enum.GetNames(typeof(ExpressionLimitKind)));
            Assert.False(Enum.IsDefined(typeof(ExpressionLimitKind), 1));
            Assert.Null(typeof(ExpressionParser).GetField("MaxTextLength"));

            Assert.Equal(2, (int)ExpressionLimitKind.SyntacticNesting);
            Assert.Equal(3, (int)ExpressionLimitKind.SyntacticTokenCount);
        }

        // ================================================================ sin límite por caracteres (A1.1)

        /// <summary>
        /// Las formas B de A1 §2.2 con L = 4000: un nombre seguro de 4001 letras, 2001 llaves escapadas y un homónimo de
        /// 4001 caracteres ya no fallan por su longitud. Una letra fuera del plano básico tampoco cambia nada.
        /// </summary>
        [Fact]
        public void UN_NOMBRE_LEGAL_DE_MAS_DE_4000_CARACTERES_YA_NO_FALLA_POR_LONGITUD()
        {
            var seguro = Assert.IsType<ReferenceSyntax>(ParseOk(new string('A', 4001)));
            Assert.Equal(4001, seguro.Name.Text.Length);

            var llaves = "{" + Repeat("}}", 2001) + "}";
            Assert.Equal(4004, llaves.Length);
            Assert.Equal(new string('}', 2001), Assert.IsType<ReferenceSyntax>(ParseOk(llaves)).Name.Text);

            var homonimo = new string('A', 3964) + "#" + Guid1;
            Assert.Equal(4001, homonimo.Length);
            var cualificado = Assert.IsType<ReferenceSyntax>(ParseOk(homonimo));
            Assert.Equal(Guid.Parse(Guid1), cualificado.Qualifier.Id);

            var astral = "{" + Repeat(LetraAstral, 4001) + "}";
            Assert.Equal(8004, astral.Length);
            Assert.Equal(8002, Assert.IsType<ReferenceSyntax>(ParseOk(astral)).Name.Text.Length);
        }

        /// <summary>A1.3: un nombre arbitrariamente largo es UN token, con todas sus palabras o entre llaves.</summary>
        [Fact]
        public void UN_NOMBRE_DE_UN_MILLON_DE_CARACTERES_ES_UN_SOLO_TOKEN()
        {
            var nombre = new string('A', 1000001);

            Assert.Equal(1, TokenCount(nombre));
            Assert.Equal(1000001, Assert.IsType<ReferenceSyntax>(ParseOk(nombre)).Name.Text.Length);

            Assert.Equal(1, TokenCount("A" + Repeat(" A", 99999)));
            Assert.Equal(1, TokenCount("{" + new string('x', 999998) + "}"));
        }

        /// <summary>
        /// Lo mismo, visto desde la guarda: con el nombre de 1 000 001 caracteres, 4096 tokens pasan, y el token 4097
        /// —un operador suelto al final— da el límite antes de que el parser vea el texto.
        /// </summary>
        [Fact]
        public void EL_NOMBRE_DE_UN_MILLON_DE_CARACTERES_CUENTA_UNO_EN_LA_GUARDA()
        {
            var justo = "-" + new string('A', 1000001) + Repeat("+1", 2047);
            Assert.Equal(4096, TokenCount(justo));
            ParseOk(justo);

            var sobra = justo + "+";
            var diagnostic = Assert.Single(ParseFails(sobra));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.SyntacticTokenCount, diagnostic.Limit);
            Assert.Equal(4096, diagnostic.LimitMaximum);
            Assert.Equal(new SourceSpan(justo.Length, 1), diagnostic.Span);
        }

        // ================================================================ tokens (A1.2–A1.4)

        [Fact]
        public void EXACTAMENTE_MAXSYNTACTICTOKENS_TOKENS_PASAN_LA_GUARDA()
        {
            var text = "-1" + Repeat("+1", 2047);

            Assert.Equal(4096, TokenCount(text));
            Assert.Equal(ExpressionSyntaxKind.Binary, ParseOk(text).Kind);
        }

        /// <summary>A1.4: un solo diagnóstico, con el máximo y desde el inicio del token 4097 hasta el final del texto.</summary>
        [Fact]
        public void EL_TOKEN_4097_DA_LIMITEXCEEDED_DESDE_SU_INICIO_HASTA_EL_FINAL()
        {
            var text = "-1" + Repeat("+1", 2048);
            Assert.Equal(4098, text.Length);

            var diagnostic = Assert.Single(ParseFails(text));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.SyntacticTokenCount, diagnostic.Limit);
            Assert.Equal(4096, diagnostic.LimitMaximum);
            Assert.Equal(new SourceSpan(4096, 2), diagnostic.Span);
        }

        /// <summary>
        /// La guarda corta mientras se lexea: 5000 caracteres inesperados dan UN diagnóstico de límite, no cinco mil
        /// caracteres inesperados, y los errores léxicos anteriores al corte no se informan.
        /// </summary>
        [Fact]
        public void LA_GUARDA_CORTA_DURANTE_EL_LEXING_CON_UN_SOLO_DIAGNOSTICO()
        {
            var diagnostic = Assert.Single(ParseFails(new string('@', 5000)));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.SyntacticTokenCount, diagnostic.Limit);
            Assert.Equal(new SourceSpan(4096, 904), diagnostic.Span);
        }

        /// <summary>A1 §5, requisito 4: nunca se materializan más de <c>MaxSyntacticTokens + 1</c> tokens.</summary>
        [Fact]
        public void LA_COLECCION_DE_TOKENS_QUEDA_ACOTADA_POR_LA_GUARDA()
        {
            var lexed = ExpressionLexer.Tokenize(Repeat("1+", 50000) + "1");

            var diagnostic = Assert.Single(lexed.Diagnostics);
            Assert.Equal(ExpressionLimitKind.SyntacticTokenCount, diagnostic.Limit);
            Assert.InRange(lexed.Tokens.Count, 0, ExpressionParser.MaxSyntacticTokens + 1);
        }

        /// <summary>
        /// A1.3, granularidad normativa: número con todos sus dígitos, sufijo de unidad, nombre desnudo completo, nombre
        /// entre llaves con sus escapes, cualificador, cada operador, paréntesis, coma y punto de namespace, y cada lexema
        /// mal formado. Los espacios no cuentan, ni la marca interna de fin de texto.
        /// </summary>
        [Theory]
        [InlineData("123.456", 1)]
        [InlineData("100[mm]", 2)]
        [InlineData("100 [ mm ]", 2)]
        [InlineData("Holgura General", 1)]
        [InlineData("{a}}b}", 1)]
        [InlineData("Holgura#" + Guid1, 2)]
        [InlineData("#" + Guid1, 1)]
        [InlineData("MAX(A, 1)", 6)]
        [InlineData("Rack.Frentes", 3)]
        [InlineData("-(1 + 2) * 3 / 4", 10)]
        [InlineData("  1  +\t2 \r\n", 3)]
        [InlineData("1,5 + @ + .5", 5)]
        public void CADA_LEXEMA_CUENTA_UNO_SEGUN_A1_3(string text, int tokens)
        {
            Assert.Equal(tokens, ExpressionLexer.Tokenize(text).Tokens.Count - 1);
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
        /// «Antes de descender»: un anidamiento de miles de niveles, dentro de la guarda de tokens, se corta en el nivel
        /// 65, sin recorrer el resto y sin desbordar la pila.
        /// </summary>
        [Theory]
        [InlineData("parentesis", 1999, 64, 1)]
        [InlineData("unario", 3999, 64, 1)]
        [InlineData("llamada", 1333, 128, 2)]
        public void LA_GUARDA_DE_ANIDAMIENTO_CORTA_ANTES_DE_DESCENDER(string construccion, int niveles, int start, int length)
        {
            var text = Anidar(construccion, niveles);
            Assert.InRange(TokenCount(text), 3000, ExpressionParser.MaxSyntacticTokens);

            var diagnostic = Assert.Single(ParseFails(text));

            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.SyntacticNesting, diagnostic.Limit);
            Assert.Equal(new SourceSpan(start, length), diagnostic.Span);
        }

        // ================================================================ lo que la guarda NO es

        /// <summary>
        /// T-V4-04, parte del parser: la cadena plana de 25 operandos no tiene ningún anidamiento sintáctico y PARSEA. Su
        /// árbol asocia por la izquierda y tiene profundidad 25; tras el enlace da <c>LimitExceeded</c> por
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

        /// <summary>Argumentos ≤ 16 es un límite normativo del árbol, no una guarda del parser.</summary>
        [Fact]
        public void EL_NUMERO_DE_ARGUMENTOS_NO_ES_UNA_GUARDA_DEL_PARSER()
        {
            var call = Assert.IsType<CallExpressionSyntax>(ParseOk("F(" + string.Join(", ", Enumerable.Repeat("1", 17)) + ")"));

            Assert.Equal(17, call.Arguments.Count);
        }

        /// <summary>Tokens de un texto sin errores léxicos, sin la marca interna de fin de texto.</summary>
        private static int TokenCount(string text)
        {
            var lexed = ExpressionLexer.Tokenize(text);

            Assert.Empty(lexed.Diagnostics);
            return lexed.Tokens.Count - 1;
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
