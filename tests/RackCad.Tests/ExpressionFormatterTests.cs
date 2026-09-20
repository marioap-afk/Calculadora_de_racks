using System;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — el formatter ÚNICO (V6 P4.2–P4.7, §4.3; ADR-0040 D6): determinista e invariante, desde el árbol enlazado y
    /// los nombres actuales del snapshot.
    ///
    /// <para>
    /// Números en su forma más corta que hace round-trip, SIN exponente y nunca con <c>"0.###"</c>; unidad pegada; un
    /// espacio a cada lado de un operador binario; funciones en mayúsculas; paréntesis mínimos que conservan el árbol; y
    /// cada referencia en su forma mínima inequívoca: <c>Nombre</c>, <c>{Nombre}</c> con <c>}}</c>, la forma del nombre
    /// más <c>#guid</c> para un homónimo, o <c>#guid</c> para un id ausente. El GUID va siempre completo, en forma D y en
    /// minúsculas.
    /// </para>
    /// </summary>
    public class ExpressionFormatterTests
    {
        private static string Format(BoundExpression tree, SymbolTable table = null)
            => ExpressionFormatter.Format(tree, table ?? SymbolTable.Empty);

        // ================================================================ números (P4.2, P1.4)

        public static TheoryData<double, string> Numeros => new TheoryData<double, string>
        {
            { 0, "0" },
            { 6, "6" },
            { 2.5, "2.5" },
            { 0.1, "0.1" },
            { 1.0 / 3, "0.3333333333333333" },
            { 3.14159, "3.14159" },
            { 123456.789, "123456.789" },
            { 0.00001, "0.00001" },
            { 1.5E-05, "0.000015" },
            { 1E+15, "1000000000000000" },
            { 1E+20, "100000000000000000000" },
            { 4503599627370496, "4503599627370496" },
            { 1E-300, "0." + new string('0', 299) + "1" },
        };

        [Theory]
        [MemberData(nameof(Numeros))]
        public void LOS_NUMEROS_SALEN_EN_SU_FORMA_MAS_CORTA_SIN_EXPONENTE(double value, string expected)
        {
            Assert.Equal(expected, Format(Num(value)));
            AssertBits(value, Assert.IsType<NumberSyntax>(ParseOk(expected)).Value);
        }

        [Fact]
        public void LOS_DOUBLES_EXTREMOS_SALEN_SIN_EXPONENTE_Y_VUELVEN_CON_LOS_MISMOS_BITS()
        {
            var maximo = Format(Num(double.MaxValue));
            Assert.Equal("17976931348623157" + new string('0', 292), maximo);
            AssertBits(double.MaxValue, Assert.IsType<NumberSyntax>(ParseOk(maximo)).Value);

            var epsilon = Format(Num(double.Epsilon));
            Assert.Equal("0." + new string('0', 323) + "5", epsilon);
            AssertBits(double.Epsilon, Assert.IsType<NumberSyntax>(ParseOk(epsilon)).Value);

            var minimoNormal = Format(Num(2.2250738585072014E-308));
            Assert.Equal(326, minimoNormal.Length);
            AssertBits(2.2250738585072014E-308, Assert.IsType<NumberSyntax>(ParseOk(minimoNormal)).Value);
        }

        // ================================================================ unidades, operadores y funciones

        [Fact]
        public void LA_UNIDAD_VA_PEGADA_Y_LOS_OPERADORES_LLEVAN_UN_ESPACIO()
        {
            Assert.Equal("100[mm]", Format(Num(100, LengthUnit.Millimeter)));
            Assert.Equal("4[in]", Format(Num(4, LengthUnit.Inch)));
            Assert.Equal("20[ft]", Format(Num(20, LengthUnit.Foot)));

            Assert.Equal("1 + 2", Format(Add(Num(1), Num(2))));
            Assert.Equal("1 - 2", Format(Sub(Num(1), Num(2))));
            Assert.Equal("1 * 2", Format(Mul(Num(1), Num(2))));
            Assert.Equal("1 / 2", Format(Div(Num(1), Num(2))));

            Assert.Equal("MIN(1, 2)", Format(Call(FunctionId.Min, Num(1), Num(2))));
            Assert.Equal("MAX(1, 2, 3)", Format(Call(FunctionId.Max, Num(1), Num(2), Num(3))));
            Assert.Equal("ABS(-1)", Format(Call(FunctionId.Abs, Neg(Num(1)))));
        }

        public static TheoryData<BoundExpression, string> Parentesis => new TheoryData<BoundExpression, string>
        {
            { Add(Add(Num(1), Num(2)), Num(3)), "1 + 2 + 3" },
            { Add(Num(1), Add(Num(2), Num(3))), "1 + (2 + 3)" },
            { Sub(Sub(Num(1), Num(2)), Num(3)), "1 - 2 - 3" },
            { Sub(Num(1), Sub(Num(2), Num(3))), "1 - (2 - 3)" },
            { Sub(Num(1), Add(Num(2), Num(3))), "1 - (2 + 3)" },
            { Mul(Add(Num(1), Num(2)), Num(3)), "(1 + 2) * 3" },
            { Mul(Num(3), Add(Num(1), Num(2))), "3 * (1 + 2)" },
            { Add(Num(1), Mul(Num(2), Num(3))), "1 + 2 * 3" },
            { Add(Mul(Num(1), Num(2)), Num(3)), "1 * 2 + 3" },
            { Div(Num(1), Mul(Num(2), Num(3))), "1 / (2 * 3)" },
            { Mul(Num(1), Div(Num(2), Num(3))), "1 * (2 / 3)" },
            { Div(Div(Num(1), Num(2)), Num(3)), "1 / 2 / 3" },
            { Neg(Add(Num(1), Num(2))), "-(1 + 2)" },
            { Neg(Mul(Num(1), Num(2))), "-(1 * 2)" },
            { Neg(Neg(Num(1))), "--1" },
            { Sub(Num(1), Neg(Num(2))), "1 - -2" },
            { Mul(Neg(Num(1)), Num(2)), "-1 * 2" },
            { Neg(Call(FunctionId.Abs, Num(1))), "-ABS(1)" },
            { Call(FunctionId.Max, Add(Num(1), Num(2)), Neg(Num(3))), "MAX(1 + 2, -3)" },
        };

        [Theory]
        [MemberData(nameof(Parentesis))]
        public void LOS_PARENTESIS_SON_LOS_MINIMOS_QUE_CONSERVAN_EL_ARBOL(BoundExpression tree, string expected)
        {
            Assert.Equal(expected, Format(tree));
            Assert.Equal(tree, BindOk(expected, Context()));
        }

        // ================================================================ referencias (§4.3)

        [Fact]
        public void CADA_REFERENCIA_UNICA_SALE_EN_SU_FORMA_MINIMA_INEQUIVOCA()
        {
            var table = Table(
                Variable(1, "Holgura"),
                Variable(2, "Holgura General"),
                Variable(3, "Holgura-Base"),
                Variable(4, "Alto (m)"),
                Variable(5, "Nivel  2"),
                Variable(6, "v1.2"),
                Variable(7, "2x"),
                Variable(8, "a}b"),
                Variable(9, "MIN"),
                Variable(10, "Rack"),
                Variable(11, "max"),
                Variable(12, " lead"),
                Variable(13, "trail "),
                Variable(14, "Caf" + (char)0x00E9),
                Variable(15, string.Empty),
                Variable(16, "_x1"),
                Variable(17, "{"));

            Assert.Equal("Holgura", Format(Ref(1), table));
            Assert.Equal("Holgura General", Format(Ref(2), table));
            Assert.Equal("{Holgura-Base}", Format(Ref(3), table));
            Assert.Equal("{Alto (m)}", Format(Ref(4), table));
            Assert.Equal("{Nivel  2}", Format(Ref(5), table));
            Assert.Equal("{v1.2}", Format(Ref(6), table));
            Assert.Equal("{2x}", Format(Ref(7), table));
            Assert.Equal("{a}}b}", Format(Ref(8), table));
            Assert.Equal("{MIN}", Format(Ref(9), table));
            Assert.Equal("{Rack}", Format(Ref(10), table));
            Assert.Equal("{max}", Format(Ref(11), table));
            Assert.Equal("{ lead}", Format(Ref(12), table));
            Assert.Equal("{trail }", Format(Ref(13), table));
            Assert.Equal("Caf" + (char)0x00E9, Format(Ref(14), table));
            Assert.Equal("{}", Format(Ref(15), table));
            Assert.Equal("_x1", Format(Ref(16), table));
            Assert.Equal("{{}", Format(Ref(17), table));
        }

        [Fact]
        public void UN_HOMONIMO_SE_CUALIFICA_CON_EL_GUID_COMPLETO_EN_MINUSCULAS()
        {
            var mayusculas = new SymbolEntry(SymbolId.ProjectVariable(Guid1.ToUpperInvariant()), SymbolScope.Project, "Holgura", SymbolDefinition.FromLiteral(1));
            var table = Table(mayusculas, Variable(2, "holgura"));

            Assert.Equal("Holgura#" + Guid1, Format(BoundExpression.Reference(mayusculas.Id), table));
            Assert.Equal("holgura#" + Key(2), Format(Ref(2), table));
            Assert.Equal("Holgura#" + Guid1, ExpressionFormatter.FormatQualifiedReference(mayusculas));

            var llaves = Table(Variable(3, "Holgura-Base"), Variable(4, "HOLGURA-BASE"));
            Assert.Equal("{Holgura-Base}#" + Key(3), Format(Ref(3), llaves));
            Assert.Equal("{HOLGURA-BASE}#" + Key(4), Format(Ref(4), llaves));
        }

        /// <summary>T-V3-03 y P4.5: el id ausente se muestra como <c>#guid</c>, sin tomar un nombre de otro sitio, y ese texto no se compromete.</summary>
        [Fact]
        public void UNA_REFERENCIA_ROTA_SE_MUESTRA_COMO_GUID_Y_NO_SE_COMPROMETE()
        {
            var table = Table(Variable(1, "Holgura"));
            var tree = Add(BoundExpression.Reference(SymbolId.ProjectVariable(Guid2.ToUpperInvariant())), Num(2));

            var text = Format(tree, table);

            Assert.Equal("#" + Guid2 + " + 2", text);
            Assert.Equal(ExpressionDiagnosticCode.BrokenReference, Assert.Single(BindFails(text, ExpressionContext.Create(table))).Code);
        }

        // ================================================================ Q(clave) (Amendment A2 §3.6, §3.7, §3.8)

        /// <summary>
        /// A2 §3.6 (ADR-0041 D6): <c>Q(clave)</c>. Una clave con forma D exacta sale <c>#</c> + la clave en minúsculas
        /// ASCII; cualquier otra, <c>#{</c> + la clave en minúsculas ASCII con cada <c>}</c> duplicada + <c>}</c>. Las filas
        /// son las de la tabla de A2 §3.6, literales. El mismo <c>Q</c> vale para la forma cualificada y para el homónimo.
        /// </summary>
        public static TheoryData<string, string> CualificadoresDeA2 => new TheoryData<string, string>
        {
            { "3F2B1C9E-8A4D-4E6F-9B0A-1C2D3E4F5A6B", "#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b" },
            { "3F2B1C9E8A4D4E6F9B0A1C2D3E4F5A6B", "#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}" },
            { "{3F2B1C9E-8A4D-4E6F-9B0A-1C2D3E4F5A6B}", "#{{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}}}" },
            { "(3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b)", "#{(3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b)}" },
            {
                "{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}",
                "#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}"
            },
            { "+03f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b", "#{+03f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b}" },
        };

        [Theory]
        [MemberData(nameof(CualificadoresDeA2))]
        public void UN_HOMONIMO_SE_CUALIFICA_CON_Q_DE_SU_CLAVE_SEGUN_A2(string key, string qualifier)
        {
            var entry = VariableKey(key, "Holgura");
            var table = Table(entry, Variable(2, "HOLGURA"));

            Assert.Equal("Holgura" + qualifier, ExpressionFormatter.FormatQualifiedReference(entry));
            Assert.Equal("Holgura" + qualifier, Format(BoundExpression.Reference(entry.Id), table));
            Assert.Equal("HOLGURA#" + Key(2), Format(Ref(2), table));
            Assert.Equal("{Holgura-Base}" + qualifier, ExpressionFormatter.FormatQualifiedReference(VariableKey(key, "Holgura-Base")));
        }

        /// <summary>
        /// A2 §3.6: bajar a minúsculas es SOLO A–Z → a–z. No reescribe la disposición ni el valor del GUID —los ceros, los
        /// espacios, el grupo corto y el espacio en blanco Unicode se quedan—, no depende de la cultura y no cambia la clave
        /// guardada en la tabla.
        /// </summary>
        [Fact]
        public void Q_SOLO_BAJA_A_Z_Y_NO_REESCRIBE_LA_DISPOSICION()
        {
            var x = VariableKey(ClaveXMayusculas, "Holgura");
            var ceros = VariableKey(ClaveXConCeros.ToUpperInvariant(), "Holgura");
            var corto = VariableKey(ClaveXGrupoCorto, "Holgura");
            var espacioDuro = VariableKey(ClaveXConEspacioDuro.ToUpperInvariant(), "Holgura");
            var compat = VariableKey(ClaveDCompatHex.ToUpperInvariant(), "Holgura");

            const string esperadoX = "Holgura#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}";

            Assert.Equal(esperadoX, ExpressionFormatter.FormatQualifiedReference(x));
            Assert.Equal("Holgura#{{0x00003f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}", ExpressionFormatter.FormatQualifiedReference(ceros));
            Assert.Equal("Holgura#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0xa,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}", ExpressionFormatter.FormatQualifiedReference(corto));
            Assert.Equal("Holgura" + Llaves(ClaveXConEspacioDuro), ExpressionFormatter.FormatQualifiedReference(espacioDuro));
            Assert.Equal("Holgura#{3f2b1c9e-0x8a-4e6f-9b0a-1c2d3e4f5a6b}", ExpressionFormatter.FormatQualifiedReference(compat));

            Assert.Equal(ClaveXMayusculas, x.Id.Key);
            WithCulture("tr-TR", () => Assert.Equal(esperadoX, ExpressionFormatter.FormatQualifiedReference(x)));
        }

        /// <summary>
        /// A2 §3.7 (prueba J de A2 §9.3): una referencia rota se muestra SOLO con <c>Q(clave)</c>, sin tomar un nombre de
        /// ningún sitio; ese texto se lee de vuelta como la misma clave y no se compromete.
        /// </summary>
        public static TheoryData<string, string> ReferenciasRotas => new TheoryData<string, string>
        {
            { ClaveDMayusculas, "#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b" },
            { ClaveN, "#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}" },
            { ClaveB, "#{{3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b}}}" },
            { ClaveP, "#{(3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b)}" },
            { ClaveXMayusculas, "#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}" },
            { ClaveBCompatHex, "#{{0x3f2b1c-8a4d-4e6f-9b0a-1c2d3e4f5a6b}}}" },
        };

        [Theory]
        [MemberData(nameof(ReferenciasRotas))]
        public void UNA_REFERENCIA_ROTA_SE_MUESTRA_CON_Q_DE_SU_CLAVE_Y_NO_SE_COMPROMETE(string key, string expected)
        {
            var table = Table(Variable(1, "Holgura"));
            var ausente = SymbolId.ProjectVariable(key);

            var text = Format(Add(BoundExpression.Reference(ausente), Num(2)), table);

            Assert.Equal(expected + " + 2", text);

            var diagnostic = Assert.Single(BindFails(text, ExpressionContext.Create(table)));
            Assert.Equal(ExpressionDiagnosticCode.BrokenReference, diagnostic.Code);
            Assert.Equal(new SourceSpan(0, expected.Length), diagnostic.Span);
            Assert.Equal(new[] { ausente }, diagnostic.RelatedSymbols);
        }

        /// <summary>
        /// A2 §3.8 (prueba I de A2 §9.3): lo tecleado como <c>Nombre#{D}</c> se enlaza a la clave D y se muestra en la forma
        /// corta, <c>Nombre#d</c>; una clave D rota tecleada entre llaves se muestra <c>#d</c>. Es normalización de la
        /// entrada, no de la identidad.
        /// </summary>
        [Fact]
        public void EL_CUALIFICADOR_D_ENTRE_LLAVES_SE_MUESTRA_EN_FORMA_CORTA()
        {
            var d = VariableKey(ClaveDMayusculas, "Holgura", 1);
            var n = VariableKey(ClaveN, "Holgura", 2);
            var table = Table(d, n);

            var bound = BindOk("Holgura" + Llaves(ClaveD) + " + 1", ExpressionContext.Create(table));

            Assert.Equal(Add(BoundExpression.Reference(d.Id), Num(1)), bound);
            Assert.Equal("Holgura#" + ClaveD + " + 1", Format(bound, table));

            const string rotaD = "0A1B2C3D-4E5F-4A6B-8C7D-9E0F1A2B3C4D";
            var rota = Assert.Single(BindFails("#{" + rotaD + "}", ExpressionContext.Create(table)));

            Assert.Equal(ExpressionDiagnosticCode.BrokenReference, rota.Code);
            Assert.Equal(rotaD, Assert.Single(rota.RelatedSymbols).Key);
            Assert.Equal("#0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d", Format(BoundExpression.Reference(Assert.Single(rota.RelatedSymbols)), table));
        }

        /// <summary>P4.7: la forma mostrada depende del snapshot actual; lo persistido no cambia.</summary>
        [Fact]
        public void LA_FORMA_MOSTRADA_DEPENDE_DEL_SNAPSHOT()
        {
            var tree = Add(Ref(1), Num(2));

            Assert.Equal("Holgura + 2", Format(tree, Table(Variable(1, "Holgura"))));
            Assert.Equal("Holgura#" + Key(1) + " + 2", Format(tree, Table(Variable(1, "Holgura"), Variable(2, "HOLGURA"))));
            Assert.Equal("Holgura + 2", Format(tree, Table(Variable(1, "Holgura"), Variable(2, "Otra"))));
        }

        [Fact]
        public void EL_FORMATTER_ES_INVARIANTE_A_LA_CULTURA()
        {
            var tree = Mul(Add(Num(1.5), Num(100, LengthUnit.Millimeter)), Call(FunctionId.Max, Num(0.25), Num(1E-05)));
            const string expected = "(1.5 + 100[mm]) * MAX(0.25, 0.00001)";

            Assert.Equal(expected, Format(tree));
            WithCulture("de-DE", () => Assert.Equal(expected, Format(tree)));
            WithCulture("fr-FR", () => Assert.Equal(expected, Format(tree)));
            WithCulture("ar-SA", () => Assert.Equal(expected, Format(tree)));
        }

        [Fact]
        public void EL_FORMATTER_EXIGE_ARBOL_Y_TABLA()
        {
            Assert.Throws<ArgumentNullException>(() => ExpressionFormatter.Format(null, SymbolTable.Empty));
            Assert.Throws<ArgumentNullException>(() => ExpressionFormatter.Format(Num(1), null));
            Assert.Throws<ArgumentNullException>(() => ExpressionFormatter.FormatQualifiedReference(null));
        }
    }
}
