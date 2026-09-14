using System;
using System.Globalization;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G5 — la gramática de la Proposal V6 (P1.2) sobre el núcleo sintáctico, por tabla.
    ///
    /// <para>
    /// Cada caso fija la FORMA del árbol y nada más: ni enlace, ni evaluación, ni unidades numéricas, ni formatter.
    /// Que <c>MIN</c>, <c>Rack</c> o <c>NaN</c> sean referencias aquí no dice que enlacen: <c>ReservedName</c>,
    /// <c>UnknownSymbol</c>, <c>UnknownNamespace</c> y <c>UnknownFunction</c> son resultados de enlace de G6 (P7.1,
    /// P15.3), y el parser solo tiene que entregar la forma con la que G6 los decide.
    /// </para>
    /// </summary>
    public class ExpressionSyntaxGrammarTests
    {
        // ================================================================ números (P1.4)

        [Theory]
        [InlineData("0", "(num 0)")]
        [InlineData("7", "(num 7)")]
        [InlineData("123", "(num 123)")]
        [InlineData("123.45", "(num 123.45)")]
        [InlineData("007", "(num 7)")]
        [InlineData("1.50", "(num 1.5)")]
        [InlineData("0.001", "(num 0.001)")]
        public void LOS_LITERALES_NUMERICOS_VALIDOS_SE_RECONOCEN(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        [Fact]
        public void EL_LITERAL_CONSERVA_SU_TEXTO_SU_VALOR_Y_SU_POSICION()
        {
            var number = Assert.IsType<NumberSyntax>(ParseOk("123.45"));

            Assert.Equal(ExpressionSyntaxKind.Number, number.Kind);
            Assert.Equal("123.45", number.Literal);
            Assert.Equal(123.45, number.Value);
            Assert.Equal(new SourceSpan(0, 6), number.Span);
            Assert.Equal(new SourceSpan(0, 6), number.LiteralSpan);
            Assert.Null(number.UnitToken);
            Assert.Null(number.UnitSpan);
            Assert.False(number.HasUnit);
        }

        /// <summary>
        /// El signo no pertenece al número (P1.4): es el operador unario. <c>NaN</c> e <c>Infinity</c> son
        /// <c>word</c> de la gramática, así que nunca llegan a ser un literal numérico.
        /// </summary>
        [Theory]
        [InlineData("-5", "(neg (num 5))")]
        [InlineData("NaN", "(ref bare \"NaN\")")]
        [InlineData("Infinity", "(ref bare \"Infinity\")")]
        [InlineData("-Infinity", "(neg (ref bare \"Infinity\"))")]
        public void NI_EL_SIGNO_NI_NAN_NI_INFINITY_FORMAN_PARTE_DE_UN_NUMERO(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        // ================================================================ unidades: solo sintaxis (P1.6)

        [Theory]
        [InlineData("100[mm]", "(num 100 mm)")]
        [InlineData("100 [mm]", "(num 100 mm)")]
        [InlineData("100[ mm ]", "(num 100 mm)")]
        [InlineData("4[in]", "(num 4 in)")]
        [InlineData("20[ft]", "(num 20 ft)")]
        [InlineData("2.5[in]", "(num 2.5 in)")]
        [InlineData("-100[mm]", "(neg (num 100 mm))")]
        [InlineData("100[mm] + 2", "(+ (num 100 mm) (num 2))")]
        public void LAS_UNIDADES_SE_RECONOCEN_TRAS_UN_LITERAL_NUMERICO(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        /// <summary>G5 reconoce la unidad; convertirla es de G6 (P9). <c>100[mm]</c> sigue valiendo 100 aquí.</summary>
        [Fact]
        public void LA_UNIDAD_ES_SINTAXIS_Y_NO_CONVIERTE_EL_VALOR()
        {
            var pegada = Assert.IsType<NumberSyntax>(ParseOk("100[mm]"));

            Assert.Equal(100.0, pegada.Value);
            Assert.Equal("mm", pegada.UnitToken);
            Assert.True(pegada.HasUnit);
            Assert.Equal(new SourceSpan(0, 7), pegada.Span);
            Assert.Equal(new SourceSpan(0, 3), pegada.LiteralSpan);
            Assert.Equal(new SourceSpan(3, 4), pegada.UnitSpan);

            var separada = Assert.IsType<NumberSyntax>(ParseOk("100 [mm]"));

            Assert.Equal(new SourceSpan(0, 8), separada.Span);
            Assert.Equal(new SourceSpan(4, 4), separada.UnitSpan);
        }

        // ================================================================ precedencia y asociatividad (P1.3)

        [Theory]
        [InlineData("1 + 2 * 3", "(+ (num 1) (* (num 2) (num 3)))")]
        [InlineData("1 * 2 + 3", "(+ (* (num 1) (num 2)) (num 3))")]
        [InlineData("1 - 6 / 3", "(- (num 1) (/ (num 6) (num 3)))")]
        [InlineData("(1 + 2) * 3", "(* (paren (+ (num 1) (num 2))) (num 3))")]
        [InlineData("2 * (3 + 4) / 5", "(/ (* (num 2) (paren (+ (num 3) (num 4)))) (num 5))")]
        public void LA_MULTIPLICACION_Y_LA_DIVISION_LIGAN_MAS_QUE_LA_SUMA_Y_LA_RESTA(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        [Theory]
        [InlineData("1 - 2 - 3", "(- (- (num 1) (num 2)) (num 3))")]
        [InlineData("8 / 4 / 2", "(/ (/ (num 8) (num 4)) (num 2))")]
        [InlineData("1 + 2 - 3 + 4", "(+ (- (+ (num 1) (num 2)) (num 3)) (num 4))")]
        [InlineData("2 * 3 / 4 * 5", "(* (/ (* (num 2) (num 3)) (num 4)) (num 5))")]
        [InlineData("1 - (2 - 3)", "(- (num 1) (paren (- (num 2) (num 3))))")]
        public void LOS_BINARIOS_ASOCIAN_POR_LA_IZQUIERDA(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        [Theory]
        [InlineData("---1", "(neg (neg (neg (num 1))))")]
        [InlineData("+1", "(pos (num 1))")]
        [InlineData("-+1", "(neg (pos (num 1)))")]
        [InlineData("-2 * 3", "(* (neg (num 2)) (num 3))")]
        [InlineData("2 * -3", "(* (num 2) (neg (num 3)))")]
        [InlineData("1 - -1", "(- (num 1) (neg (num 1)))")]
        [InlineData("- 1", "(neg (num 1))")]
        [InlineData("-(1 + 2)", "(neg (paren (+ (num 1) (num 2))))")]
        [InlineData("-A", "(neg (ref bare \"A\"))")]
        public void EL_UNARIO_LIGA_MAS_QUE_CUALQUIER_BINARIO(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        /// <summary><c>1/2</c> no es una fracción: es una división válida (P1.6).</summary>
        [Fact]
        public void UNO_BARRA_DOS_ES_UNA_DIVISION()
        {
            Assert.Equal("(/ (num 1) (num 2))", Dump(ParseOk("1/2")));
        }

        /// <summary>La sintaxis conserva la intención: quitar paréntesis redundantes es del árbol enlazado (P2.1).</summary>
        [Fact]
        public void LOS_PARENTESIS_REDUNDANTES_SE_CONSERVAN_EN_LA_SINTAXIS()
        {
            var syntax = ParseOk("((1))");

            Assert.Equal("(paren (paren (num 1)))", Dump(syntax));
            Assert.Equal(ExpressionSyntaxKind.Parenthesized, syntax.Kind);
            Assert.Equal(new SourceSpan(0, 5), syntax.Span);
        }

        [Theory]
        [InlineData("1+2*3")]
        [InlineData(" 1 + 2 * 3 ")]
        [InlineData("1\t+\n2\r\n*3")]
        public void LOS_ESPACIOS_ENTRE_TOKENS_NO_SON_SIGNIFICATIVOS(string text)
        {
            Assert.Equal("(+ (num 1) (* (num 2) (num 3)))", Dump(ParseOk(text)));
        }

        // ================================================================ referencias y nombres (P1.2, P1.7, §4)

        [Theory]
        [InlineData("Holgura", "(ref bare \"Holgura\")")]
        [InlineData("Holgura General", "(ref bare \"Holgura General\")")]
        [InlineData("Holgura General + 2", "(+ (ref bare \"Holgura General\") (num 2))")]
        [InlineData("A B C", "(ref bare \"A B C\")")]
        [InlineData("_x1", "(ref bare \"_x1\")")]
        [InlineData("Año", "(ref bare \"Año\")")]
        [InlineData("Größe Máxima", "(ref bare \"Größe Máxima\")")]
        [InlineData("in", "(ref bare \"in\")")]
        [InlineData("{Holgura-Base}", "(ref braced \"Holgura-Base\")")]
        [InlineData("{A}}B}", "(ref braced \"A}B\")")]
        [InlineData("{Nombre con }} llave}", "(ref braced \"Nombre con } llave\")")]
        [InlineData("{Alto (m)}", "(ref braced \"Alto (m)\")")]
        [InlineData("{Nivel  2}", "(ref braced \"Nivel  2\")")]
        [InlineData("{v1.2}", "(ref braced \"v1.2\")")]
        [InlineData("{}", "(ref braced \"\")")]
        public void LOS_NOMBRES_SE_RECONOCEN_SIN_RESOLVERSE(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        /// <summary>
        /// Los nombres reservados de §4.2 regla 4 son, en sintaxis, nombres como cualquier otro: decidir
        /// <c>ReservedName</c> es del enlace (G6). Las llaves sí quedan registradas, porque es lo que G6 necesita.
        /// </summary>
        [Theory]
        [InlineData("MIN", "(ref bare \"MIN\")")]
        [InlineData("max", "(ref bare \"max\")")]
        [InlineData("Rack", "(ref bare \"Rack\")")]
        [InlineData("Project", "(ref bare \"Project\")")]
        [InlineData("{MIN}", "(ref braced \"MIN\")")]
        [InlineData("{Rack}", "(ref braced \"Rack\")")]
        public void LOS_RESERVADOS_SON_NOMBRES_EN_SINTAXIS_Y_LAS_LLAVES_SE_CONSERVAN(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        [Theory]
        [InlineData("Holgura#" + Guid1, "(ref bare \"Holgura\" #" + Guid1 + ")")]
        [InlineData("{Nombre complejo}#" + Guid2, "(ref braced \"Nombre complejo\" #" + Guid2 + ")")]
        [InlineData("#" + Guid1, "(ref #" + Guid1 + ")")]
        [InlineData("Holgura General#" + Guid1 + " + 2", "(+ (ref bare \"Holgura General\" #" + Guid1 + ") (num 2))")]
        [InlineData("Holgura #" + Guid1, "(ref bare \"Holgura\" #" + Guid1 + ")")]
        [InlineData("#" + Guid1 + "-2", "(- (ref #" + Guid1 + ") (num 2))")]
        public void LAS_REFERENCIAS_CUALIFICADAS_SE_REPRESENTAN_SINTACTICAMENTE(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        /// <summary>P3.9: forma D completa, hexadecimales sin distinguir mayúsculas; el texto tecleado se conserva.</summary>
        [Fact]
        public void EL_CUALIFICADOR_ACEPTA_HEXADECIMALES_EN_CUALQUIER_CASO_Y_CONSERVA_LO_TECLEADO()
        {
            const string mayusculas = "3F2B1C9E-6D4A-4F38-9B71-0C2A5E8D1F44";

            var reference = Assert.IsType<ReferenceSyntax>(ParseOk("#" + mayusculas));

            Assert.Equal(ExpressionSyntaxKind.Reference, reference.Kind);
            Assert.Null(reference.Name);
            Assert.Equal(mayusculas, reference.Qualifier.Key);
            Assert.Equal(new SourceSpan(0, 37), reference.Qualifier.Span);
            Assert.Equal(new SourceSpan(0, 37), reference.Span);
        }

        /// <summary>
        /// Amendment A2 §3.3 y §3.4 (ADR-0041 D7): <c>qualifier = "#" , ( guid-d | braced-key )</c>. Dentro de <c>#{…}</c>
        /// todo carácter es dato —comas, espacios, espacio en blanco Unicode, paréntesis, llaves anidadas, <c>0x</c> y
        /// <c>+</c>— salvo <c>}}</c>, que es una <c>}</c>; la llave sin duplicar cierra. La sintaxis lleva la clave
        /// DESESCAPADA tal como se tecleó, y la posición del lexema entero.
        /// </summary>
        public static TheoryData<string, string, string, int> CualificadoresDeClaveExacta
        {
            get
            {
                var data = new TheoryData<string, string, string, int>();

                void Fila(string name, string nameText, string key)
                    => data.Add(nameText + Llaves(key), name, key, nameText.Length);

                Fila(null, string.Empty, ClaveN);
                Fila("Holgura", "Holgura", ClaveN);
                Fila("Nombre complejo", "{Nombre complejo}", ClaveNMayusculas);
                Fila("Holgura", "Holgura", ClaveB);
                Fila(null, string.Empty, ClaveP);
                Fila("Holgura", "Holgura", ClaveX);
                Fila("Holgura", "Holgura", ClaveXMayusculas);
                Fila("Holgura", "Holgura", ClaveXConEspacios);
                Fila("Holgura", "Holgura", ClaveXConEspacioDuro);
                Fila("a}b", "{a}}b}", ClaveXConSeparadorDeLinea);
                Fila("Holgura", "Holgura", ClaveDCompatSigno);
                Fila("Holgura", "Holgura", ClaveBCompatHex);
                Fila("Holgura", "Holgura", ClaveDMayusculas);
                Fila(null, string.Empty, "{0x" + new string('0', 4000) + "3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}");
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(CualificadoresDeClaveExacta))]
        public void EL_CUALIFICADOR_DE_CLAVE_EXACTA_LLEVA_LA_CLAVE_DESESCAPADA(string text, string name, string key, int qualifierStart)
        {
            var reference = Assert.IsType<ReferenceSyntax>(ParseOk(text));

            Assert.Equal(name, reference.Name?.Text);
            Assert.Equal(key, reference.Qualifier.Key);
            Assert.Equal(new SourceSpan(qualifierStart, text.Length - qualifierStart), reference.Qualifier.Span);
            Assert.Equal(new SourceSpan(0, text.Length), reference.Span);
        }

        /// <summary>
        /// A2 §3.6 y prueba F de A2 §9.3: la clave B se teclea <c>#{{d}}}</c> y recupera <c>{d}</c>, mientras que
        /// <c>#{d}</c> recupera la clave D: dos claves distintas, sin colisión. Escapar y desescapar devuelve la misma clave
        /// también con llaves anidadas.
        /// </summary>
        [Fact]
        public void LA_CLAVE_B_ESCAPADA_NO_COLISIONA_CON_LA_CLAVE_D_ENTRE_LLAVES()
        {
            var b = Assert.IsType<ReferenceSyntax>(ParseOk("#{{" + ClaveD + "}}}"));
            var d = Assert.IsType<ReferenceSyntax>(ParseOk("#{" + ClaveD + "}"));

            Assert.Equal(ClaveB, b.Qualifier.Key);
            Assert.Equal(ClaveD, d.Qualifier.Key);
            Assert.NotEqual(SymbolId.ProjectVariable(b.Qualifier.Key), SymbolId.ProjectVariable(d.Qualifier.Key));

            foreach (var key in new[] { ClaveB, ClaveX, ClaveXMayusculas, ClaveXConEspacios, ClaveBCompatHex })
            {
                var qualified = Assert.IsType<ReferenceSyntax>(Assert.IsType<BinaryExpressionSyntax>(ParseOk("Holgura" + Llaves(key) + " + 1")).Left);

                Assert.Equal(key, qualified.Qualifier.Key);
                Assert.True(SymbolNamespaces.KeyComparer(SymbolNamespace.ProjectVariable).Equals(key, qualified.Qualifier.Key));
            }
        }

        /// <summary>
        /// Lo que G6 necesita para aplicar P7.1 sin volver a leer el texto con heurísticas: el nombre sin escapes, si
        /// iba entre llaves, el id y la posición exacta de cada pieza.
        /// </summary>
        [Fact]
        public void LA_REFERENCIA_CONSERVA_NOMBRE_FORMA_ID_Y_POSICIONES()
        {
            var suma = Assert.IsType<BinaryExpressionSyntax>(ParseOk("{A}}B} + Holgura General#" + Guid1));

            var braced = Assert.IsType<ReferenceSyntax>(suma.Left);
            Assert.Equal("A}B", braced.Name.Text);
            Assert.True(braced.Name.IsBraced);
            Assert.Equal(new SourceSpan(0, 6), braced.Name.Span);
            Assert.Null(braced.Qualifier);
            Assert.Equal(new SourceSpan(0, 6), braced.Span);

            var qualified = Assert.IsType<ReferenceSyntax>(suma.Right);
            Assert.Equal("Holgura General", qualified.Name.Text);
            Assert.False(qualified.Name.IsBraced);
            Assert.Equal(new SourceSpan(9, 15), qualified.Name.Span);
            Assert.Equal(Guid1, qualified.Qualifier.Key);
            Assert.Equal(new SourceSpan(24, 37), qualified.Qualifier.Span);
            Assert.Equal(new SourceSpan(9, 52), qualified.Span);
        }

        /// <summary>
        /// <c>Holgura-Base</c> es una resta en sintaxis. <c>OperatorInName</c> lo decide G6 contra los nombres del
        /// snapshot (P7.1, §4.2 regla 12); lo que G5 garantiza es que las posiciones delatan un tramo contiguo.
        /// </summary>
        [Fact]
        public void LA_RESTA_CONTIGUA_CONSERVA_LAS_POSICIONES_QUE_G6_NECESITA_PARA_OPERATORINNAME()
        {
            var contigua = Assert.IsType<BinaryExpressionSyntax>(ParseOk("Holgura-Base"));

            Assert.Equal("(- (ref bare \"Holgura\") (ref bare \"Base\"))", Dump(contigua));
            Assert.Equal(SyntaxBinaryOperator.Subtract, contigua.Operator);
            Assert.Equal(new SourceSpan(0, 7), contigua.Left.Span);
            Assert.Equal(new SourceSpan(7, 1), contigua.OperatorSpan);
            Assert.Equal(new SourceSpan(8, 4), contigua.Right.Span);
            Assert.Equal(contigua.Left.Span.End, contigua.OperatorSpan.Start);
            Assert.Equal(contigua.OperatorSpan.End, contigua.Right.Span.Start);

            var separada = Assert.IsType<BinaryExpressionSyntax>(ParseOk("Holgura - Base"));

            Assert.NotEqual(separada.Left.Span.End, separada.OperatorSpan.Start);
        }

        // ================================================================ namespace reservado (P1.7, P25.3)

        /// <summary>
        /// <c>palabra.</c> es sintaxis de namespace para ID20: G5 la reconoce y G6 da <c>UnknownNamespace</c>. Delante del
        /// punto va UNA palabra, igual que delante del paréntesis de una llamada; lo que sigue al punto es el nombre sin
        /// llaves que haya, si hay alguno.
        /// </summary>
        [Theory]
        [InlineData("Rack.Frentes", "(ns \"Rack\" \"Frentes\")")]
        [InlineData("Rack.Frentes * 2", "(* (ns \"Rack\" \"Frentes\") (num 2))")]
        [InlineData("Project.TotalRacks", "(ns \"Project\" \"TotalRacks\")")]
        [InlineData("Rack.", "(ns \"Rack\")")]
        [InlineData("Rack.Frentes Totales", "(ns \"Rack\" \"Frentes Totales\")")]
        public void LA_SINTAXIS_DE_NAMESPACE_SE_RECONOCE_PARA_QUE_G6_LA_DECIDA(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        [Fact]
        public void LA_SINTAXIS_DE_NAMESPACE_CONSERVA_SUS_PIEZAS_Y_POSICIONES()
        {
            var reserved = Assert.IsType<NamespaceReferenceSyntax>(ParseOk("Rack.Frentes"));

            Assert.Equal(ExpressionSyntaxKind.NamespaceReference, reserved.Kind);
            Assert.Equal("Rack", reserved.Namespace.Text);
            Assert.Equal(new SourceSpan(0, 4), reserved.Namespace.Span);
            Assert.Equal(new SourceSpan(4, 1), reserved.DotSpan);
            Assert.Equal("Frentes", reserved.Member.Text);
            Assert.Equal(new SourceSpan(5, 7), reserved.Member.Span);
            Assert.Equal(new SourceSpan(0, 12), reserved.Span);
        }

        // ================================================================ llamadas (P1.2, P1.7)

        /// <summary>Si la función existe, su aridad y su valor son de G6 (<c>UnknownFunction</c>, <c>InvalidArguments</c>).</summary>
        [Theory]
        [InlineData("F()", "(call \"F\")")]
        [InlineData("F(A)", "(call \"F\" (ref bare \"A\"))")]
        [InlineData("F(A, B)", "(call \"F\" (ref bare \"A\") (ref bare \"B\"))")]
        [InlineData("F(A,B)", "(call \"F\" (ref bare \"A\") (ref bare \"B\"))")]
        [InlineData("MIN(A, B)", "(call \"MIN\" (ref bare \"A\") (ref bare \"B\"))")]
        [InlineData("UNKNOWN(A)", "(call \"UNKNOWN\" (ref bare \"A\"))")]
        [InlineData("max(1, 2)", "(call \"max\" (num 1) (num 2))")]
        [InlineData("MIN (A, B)", "(call \"MIN\" (ref bare \"A\") (ref bare \"B\"))")]
        [InlineData("ABS(A) + 1", "(+ (call \"ABS\" (ref bare \"A\")) (num 1))")]
        [InlineData("MAX(Holgura, 1.5)", "(call \"MAX\" (ref bare \"Holgura\") (num 1.5))")]
        [InlineData("F(G(1), -2)", "(call \"F\" (call \"G\" (num 1)) (neg (num 2)))")]
        [InlineData("ABS(A, B, C)", "(call \"ABS\" (ref bare \"A\") (ref bare \"B\") (ref bare \"C\"))")]
        [InlineData("MAX(A, 4[in])", "(call \"MAX\" (ref bare \"A\") (num 4 in))")]
        public void LAS_LLAMADAS_SE_RECONOCEN_SIN_DECIDIR_FUNCION_NI_ARIDAD(string text, string expected)
        {
            Assert.Equal(expected, Dump(ParseOk(text)));
        }

        [Fact]
        public void LA_LLAMADA_CONSERVA_NOMBRE_ARGUMENTOS_Y_POSICIONES()
        {
            var call = Assert.IsType<CallExpressionSyntax>(ParseOk("MAX(A, 1)"));

            Assert.Equal(ExpressionSyntaxKind.Call, call.Kind);
            Assert.Equal("MAX", call.Function.Text);
            Assert.False(call.Function.IsBraced);
            Assert.Equal(new SourceSpan(0, 3), call.Function.Span);
            Assert.Equal(2, call.Arguments.Count);
            Assert.Equal(new SourceSpan(4, 1), call.Arguments[0].Span);
            Assert.Equal(new SourceSpan(7, 1), call.Arguments[1].Span);
            Assert.Equal(new SourceSpan(0, 9), call.Span);
        }

        // ================================================================ posiciones (P2.1)

        [Fact]
        public void LAS_POSICIONES_DE_NODOS_Y_OPERADORES_SON_EXACTAS()
        {
            var suma = Assert.IsType<BinaryExpressionSyntax>(ParseOk("1 + 2 * 3"));

            Assert.Equal(ExpressionSyntaxKind.Binary, suma.Kind);
            Assert.Equal(SyntaxBinaryOperator.Add, suma.Operator);
            Assert.Equal(new SourceSpan(0, 9), suma.Span);
            Assert.Equal(new SourceSpan(2, 1), suma.OperatorSpan);
            Assert.Equal(new SourceSpan(0, 1), suma.Left.Span);

            var producto = Assert.IsType<BinaryExpressionSyntax>(suma.Right);
            Assert.Equal(SyntaxBinaryOperator.Multiply, producto.Operator);
            Assert.Equal(new SourceSpan(4, 5), producto.Span);
            Assert.Equal(new SourceSpan(6, 1), producto.OperatorSpan);

            var negacion = Assert.IsType<UnaryExpressionSyntax>(ParseOk("-x"));
            Assert.Equal(ExpressionSyntaxKind.Unary, negacion.Kind);
            Assert.Equal(SyntaxUnaryOperator.Minus, negacion.Operator);
            Assert.Equal(new SourceSpan(0, 2), negacion.Span);
            Assert.Equal(new SourceSpan(0, 1), negacion.OperatorSpan);
            Assert.Equal(new SourceSpan(1, 1), negacion.Operand.Span);

            var grupo = Assert.IsType<ParenthesizedExpressionSyntax>(ParseOk(" (1) "));
            Assert.Equal(new SourceSpan(1, 3), grupo.Span);
            Assert.Equal(new SourceSpan(2, 1), grupo.Expression.Span);
        }

        /// <summary>Las posiciones son unidades UTF-16 del texto: una letra fuera del plano básico ocupa dos.</summary>
        [Fact]
        public void LAS_POSICIONES_SE_MIDEN_EN_UNIDADES_UTF16()
        {
            const string letraAstral = "\U0001D49C";

            var suma = Assert.IsType<BinaryExpressionSyntax>(ParseOk("{" + letraAstral + "} + 1"));
            Assert.Equal(new SourceSpan(0, 4), suma.Left.Span);
            Assert.Equal(new SourceSpan(5, 1), suma.OperatorSpan);
            Assert.Equal(new SourceSpan(7, 1), suma.Right.Span);

            var bare = Assert.IsType<ReferenceSyntax>(ParseOk(letraAstral + letraAstral));
            Assert.Equal(letraAstral + letraAstral, bare.Name.Text);
            Assert.False(bare.Name.IsBraced);
            Assert.Equal(new SourceSpan(0, 4), bare.Span);
        }

        // ================================================================ el resultado

        [Fact]
        public void EL_TEXTO_ORIGINAL_VIAJA_CON_EL_RESULTADO_EN_EXITO_Y_EN_FALLO()
        {
            Assert.Equal(" 1 + A ", ExpressionParser.Parse(" 1 + A ").Text);
            Assert.Equal("1 +", ExpressionParser.Parse("1 +").Text);
        }

        [Fact]
        public void UN_FALLO_NO_TIENE_ARBOL()
        {
            var result = ExpressionParser.Parse("1 +");

            Assert.False(result.Succeeded);
            Assert.Throws<InvalidOperationException>(() => result.Syntax);
        }

        [Fact]
        public void ANALIZAR_NULL_ES_UN_ERROR_DE_PROGRAMACION()
        {
            Assert.Throws<ArgumentNullException>(() => ExpressionParser.Parse(null));
        }

        [Fact]
        public void EL_MISMO_TEXTO_DA_SIEMPRE_EL_MISMO_ARBOL()
        {
            const string text = "MAX(Holgura General#" + Guid1 + ", 100[mm]) - -{A}}B} / Rack.Frentes";

            Assert.Equal(Dump(ParseOk(text)), Dump(ParseOk(text)));
        }

        // ================================================================ independencia de la cultura (P1.10)

        /// <summary>
        /// La cultura del proceso no cambia nada: ni el separador decimal, ni la coma ambigua, ni la comparación de
        /// unidades sin distinguir mayúsculas (tr-TR es la cultura donde <c>"IN".ToLower()</c> no da <c>"in"</c>).
        /// </summary>
        [Theory]
        [InlineData("en-US")]
        [InlineData("es-MX")]
        [InlineData("de-DE")]
        [InlineData("fr-FR")]
        [InlineData("tr-TR")]
        public void EL_RESULTADO_NO_DEPENDE_DE_LA_CULTURA_DEL_PROCESO(string culture)
        {
            WithCulture(culture, () =>
            {
                var syntax = ParseOk("123.45 + 100[in] * MAX(A, 2)");
                Assert.Equal("(+ (num 123.45) (* (num 100 in) (call \"MAX\" (ref bare \"A\") (num 2))))", Dump(syntax));
                Assert.Equal(123.45, Assert.IsType<NumberSyntax>(Assert.IsType<BinaryExpressionSyntax>(syntax).Left).Value);

                var coma = Assert.Single(ParseFails("1,5"));
                Assert.Equal(ExpressionDiagnosticCode.AmbiguousDecimalComma, coma.Code);
                Assert.Equal(new SourceSpan(0, 3), coma.Span);

                var unidad = Assert.Single(ParseFails("100 IN"));
                Assert.Equal(ExpressionDiagnosticCode.UnitSyntaxNotSupported, unidad.Code);
                Assert.Equal(new SourceSpan(0, 6), unidad.Span);

                Assert.Equal(CultureInfo.GetCultureInfo(culture), CultureInfo.CurrentCulture);
            });
        }

        // ================================================================ SourceSpan

        [Fact]
        public void SOURCESPAN_ES_UN_VALOR_CON_INICIO_LONGITUD_Y_FIN()
        {
            var span = new SourceSpan(3, 4);

            Assert.Equal(3, span.Start);
            Assert.Equal(4, span.Length);
            Assert.Equal(7, span.End);
            Assert.Equal(new SourceSpan(3, 4), span);
            Assert.NotEqual(new SourceSpan(3, 5), span);
            Assert.True(span == new SourceSpan(3, 4));
            Assert.True(span != new SourceSpan(4, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SourceSpan(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SourceSpan(0, -1));
        }
    }
}
