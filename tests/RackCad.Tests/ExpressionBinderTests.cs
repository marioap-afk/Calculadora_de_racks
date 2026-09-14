using System;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — <c>SymbolResolver</c> y binder de V6 P7 con la sintaxis de §4 (OPEN A; ADR-0040 D5, D6, D7).
    ///
    /// <para>
    /// Cada fila de la tabla de P7.1 es una prueba de comportamiento: un nombre exacto y único enlaza; con cualificador
    /// manda el id y el nombre solo se valida; un id ausente es <c>BrokenReference</c>; <c>#id</c> sin nombre nunca
    /// enlaza; un homónimo sin cualificar es <c>AmbiguousName</c> con cada candidato; un reservado sin llaves es
    /// <c>ReservedName</c>; <c>palabra.</c> es <c>UnknownNamespace</c>; y un tramo contiguo con operadores que coincide
    /// con un nombre es <c>OperatorInName</c>, nunca una resta. Nada de esto recorta, normaliza, completa ni elige.
    /// </para>
    /// <para>
    /// El binder es total y fail-closed (P7.4): devuelve un árbol solo con cero diagnósticos; si no, todos los
    /// diagnósticos en orden determinista y con un tope de 20.
    /// </para>
    /// </summary>
    public class ExpressionBinderTests
    {
        private static readonly SymbolEntry Holgura = Variable(1, "Holgura", 6);
        private static readonly SymbolEntry Base = Variable(2, "Base", 10);
        private static readonly SymbolEntry HolguraGeneralA = Variable(3, "Holgura General");
        private static readonly SymbolEntry HolguraGeneralB = Variable(4, "Holgura General");
        private static readonly SymbolEntry Min = Variable(5, "MIN");
        private static readonly SymbolEntry Rack = Variable(6, "Rack");
        private static readonly SymbolEntry HolguraBase = Variable(7, "Holgura-Base");
        private static readonly SymbolEntry LlaveEnMedio = Variable(8, "a}b");
        private static readonly SymbolEntry SoloRack = Variable(9, "SoloRack", scope: SymbolScope.Rack);
        private static readonly SymbolEntry NivelUno = Variable(10, "Nivel Uno");

        private static ExpressionContext Ctx()
            => Context(Holgura, Base, HolguraGeneralA, HolguraGeneralB, Min, Rack, HolguraBase, LlaveEnMedio, SoloRack, NivelUno);

        private static SymbolId RefId(BoundExpression expression) => Assert.IsType<BoundReference>(expression).Symbol;

        // ================================================================ enlaza (P7.1, P7.2)

        [Theory]
        [InlineData("Holgura", 1)]
        [InlineData("holgura", 1)]
        [InlineData("HOLGURA", 1)]
        [InlineData("Nivel Uno", 10)]
        [InlineData("nivel uno", 10)]
        [InlineData("{Holgura}", 1)]
        [InlineData("{Holgura-Base}", 7)]
        [InlineData("{a}}b}", 8)]
        [InlineData("{MIN}", 5)]
        [InlineData("{min}", 5)]
        [InlineData("{Rack}", 6)]
        public void UN_NOMBRE_EXACTO_Y_UNICO_ENLAZA_A_SU_ID(string text, int id)
        {
            Assert.Equal(Id(id), RefId(BindOk(text, Ctx())));
        }

        [Fact]
        public void CON_CUALIFICADOR_MANDA_EL_ID_Y_EL_NOMBRE_SOLO_SE_VALIDA()
        {
            var ctx = Ctx();

            Assert.Equal(Id(1), RefId(BindOk("Holgura#" + Key(1), ctx)));
            Assert.Equal(Id(1), RefId(BindOk("holgura#" + Key(1).ToUpperInvariant(), ctx)));
            Assert.Equal(Id(3), RefId(BindOk("Holgura General#" + Key(3), ctx)));
            Assert.Equal(Id(4), RefId(BindOk("{Holgura General}#" + Key(4), ctx)));
            Assert.Equal(Id(7), RefId(BindOk("{Holgura-Base}#" + Key(7), ctx)));

            // La identidad enlazada es la de la tabla, no la grafía tecleada del GUID.
            Assert.Equal(Key(1), RefId(BindOk("Holgura#" + Key(1).ToUpperInvariant(), ctx)).Key);
        }

        // ================================================================ no enlaza

        [Fact]
        public void UN_ID_AUSENTE_DA_BROKENREFERENCE_Y_NUNCA_SE_COMPROMETE()
        {
            var ausente = Key(99);

            var conNombre = Assert.Single(BindFails("Holgura#" + ausente, Ctx()));
            Assert.Equal(ExpressionDiagnosticCode.BrokenReference, conNombre.Code);
            Assert.Equal(ExpressionDiagnosticClass.Semantic, conNombre.Class);
            Assert.Equal(new SourceSpan(0, 44), conNombre.Span);
            Assert.Equal(new[] { Id(99) }, conNombre.RelatedSymbols);

            var sinNombre = Assert.Single(BindFails("#" + ausente + " + 2", Ctx()));
            Assert.Equal(ExpressionDiagnosticCode.BrokenReference, sinNombre.Code);
            Assert.Equal(new SourceSpan(0, 37), sinNombre.Span);
            Assert.Equal(new[] { Id(99) }, sinNombre.RelatedSymbols);
        }

        [Fact]
        public void UN_NOMBRE_DISTINTO_DEL_ACTUAL_CON_CUALIFICADOR_DA_QUALIFIEDNAMEMISMATCH()
        {
            var distinto = Assert.Single(BindFails("Base#" + Key(1), Ctx()));
            Assert.Equal(ExpressionDiagnosticCode.QualifiedNameMismatch, distinto.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, distinto.Class);
            Assert.Equal(new SourceSpan(0, 41), distinto.Span);
            Assert.Equal(new[] { Id(1) }, distinto.RelatedSymbols);

            // Un nombre parcial tampoco valida: la comparación es exacta.
            Assert.Equal(
                ExpressionDiagnosticCode.QualifiedNameMismatch,
                Assert.Single(BindFails("Holg#" + Key(1), Ctx())).Code);
        }

        [Fact]
        public void ALMOHADILLA_SIN_NOMBRE_CON_ID_PRESENTE_DA_NAMEREQUIRED()
        {
            var diagnostic = Assert.Single(BindFails("#" + Key(1), Ctx()));

            Assert.Equal(ExpressionDiagnosticCode.NameRequired, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            Assert.Equal(new SourceSpan(0, 37), diagnostic.Span);
            Assert.Equal(new[] { Id(1) }, diagnostic.RelatedSymbols);
        }

        [Theory]
        [InlineData("Holg")]
        [InlineData("Holgura Gen")]
        [InlineData("Nivel")]
        [InlineData("{Holgura }")]
        [InlineData("{ Holgura}")]
        [InlineData("Min Value")]
        [InlineData("Inexistente")]
        public void UN_NOMBRE_SIN_COINCIDENCIA_EXACTA_DA_UNKNOWNSYMBOL(string text)
        {
            var diagnostic = Assert.Single(BindFails(text, Ctx()));

            Assert.Equal(ExpressionDiagnosticCode.UnknownSymbol, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            Assert.Equal(new SourceSpan(0, text.Length), diagnostic.Span);
            Assert.Empty(diagnostic.RelatedSymbols);
        }

        [Fact]
        public void UN_HOMONIMO_SIN_CUALIFICAR_DA_AMBIGUOUSNAME_CON_CADA_CANDIDATO_CUALIFICADO()
        {
            var ctx = Ctx();
            var diagnostic = Assert.Single(BindFails("Holgura General + 1", ctx));

            Assert.Equal(ExpressionDiagnosticCode.AmbiguousName, diagnostic.Code);
            Assert.Equal(new SourceSpan(0, 15), diagnostic.Span);
            Assert.Equal(new[] { Id(3), Id(4) }, diagnostic.RelatedSymbols);

            Assert.Equal(
                new[] { "Holgura General#" + Key(3), "Holgura General#" + Key(4) },
                diagnostic.RelatedSymbols.Select(id =>
                {
                    Assert.True(ctx.Symbols.TryGet(id, out var entry));
                    return ExpressionFormatter.FormatQualifiedReference(entry);
                }));
        }

        [Theory]
        [InlineData("MIN")]
        [InlineData("max")]
        [InlineData("Abs")]
        [InlineData("Rack")]
        [InlineData("PROJECT")]
        public void UN_RESERVADO_SIN_LLAVES_DA_RESERVEDNAME_AUNQUE_EXISTA_UNA_VARIABLE_ASI(string text)
        {
            var diagnostic = Assert.Single(BindFails(text, Ctx()));

            Assert.Equal(ExpressionDiagnosticCode.ReservedName, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            Assert.Equal(new SourceSpan(0, text.Length), diagnostic.Span);
        }

        [Fact]
        public void UN_RESERVADO_CUALIFICADO_SIN_LLAVES_TAMBIEN_DA_RESERVEDNAME()
        {
            var diagnostic = Assert.Single(BindFails("MIN#" + Key(5), Ctx()));

            Assert.Equal(ExpressionDiagnosticCode.ReservedName, diagnostic.Code);
            Assert.Equal(new SourceSpan(0, 3), diagnostic.Span);
        }

        [Theory]
        [InlineData("Rack.Frentes * 2", 0, 12)]
        [InlineData("Project.TotalRacks", 0, 18)]
        [InlineData("2 * Holgura.X", 4, 9)]
        [InlineData("MIN.x", 0, 5)]
        public void LA_SINTAXIS_DE_NAMESPACE_DA_UNKNOWNNAMESPACE(string text, int start, int length)
        {
            var diagnostic = Assert.Single(BindFails(text, Ctx()));

            Assert.Equal(ExpressionDiagnosticCode.UnknownNamespace, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            Assert.Equal(new SourceSpan(start, length), diagnostic.Span);
        }

        // ================================================================ OperatorInName (P7.1, §4.2 regla 12)

        [Theory]
        [InlineData("Holgura-Base", 0, 12)]
        [InlineData("holgura-base", 0, 12)]
        [InlineData("Holgura-Base + 2", 0, 12)]
        [InlineData("2 * (Holgura-Base)", 5, 12)]
        [InlineData("MAX(1, Holgura-Base)", 7, 12)]
        public void UN_TRAMO_CONTIGUO_CON_OPERADORES_IGUAL_A_UN_NOMBRE_DA_OPERATORINNAME(string text, int start, int length)
        {
            var diagnostic = Assert.Single(BindFails(text, Ctx()));

            Assert.Equal(ExpressionDiagnosticCode.OperatorInName, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            Assert.Equal(new SourceSpan(start, length), diagnostic.Span);
            Assert.Equal(new[] { Id(7) }, diagnostic.RelatedSymbols);
        }

        [Fact]
        public void CON_ESPACIOS_ES_UNA_RESTA_Y_SIN_UNA_VARIABLE_ASI_TAMBIEN()
        {
            var resta = Assert.IsType<BoundBinary>(BindOk("Holgura - Base", Ctx()));
            Assert.Equal(BoundBinaryOperator.Subtract, resta.Operator);
            Assert.Equal(Id(1), RefId(resta.Left));
            Assert.Equal(Id(2), RefId(resta.Right));

            var contigua = Assert.IsType<BoundBinary>(BindOk("Holgura-Base", Context(Holgura, Base)));
            Assert.Equal(BoundBinaryOperator.Subtract, contigua.Operator);
        }

        [Fact]
        public void OPERATORINNAME_CUBRE_TRAMOS_CON_VARIOS_OPERADORES_UNARIOS_Y_NUMEROS()
        {
            var ctx = Context(Variable(1, "a"), Variable(2, "b"), Variable(3, "a*-b"), Variable(4, "Nivel-1"), Variable(5, "Nivel"));

            var unario = Assert.Single(BindFails("a*-b", ctx));
            Assert.Equal(ExpressionDiagnosticCode.OperatorInName, unario.Code);
            Assert.Equal(new SourceSpan(0, 4), unario.Span);
            Assert.Equal(new[] { Id(3) }, unario.RelatedSymbols);

            var numero = Assert.Single(BindFails("2 + Nivel-1", ctx));
            Assert.Equal(ExpressionDiagnosticCode.OperatorInName, numero.Code);
            Assert.Equal(new SourceSpan(4, 7), numero.Span);
            Assert.Equal(new[] { Id(4) }, numero.RelatedSymbols);

            // Un tramo que EMPIEZA por un operador no es un nombre usado como operación: -a es la negación de a, y el
            // formatter escribe así toda negación (P2.5).
            Assert.Equal(Neg(Ref(1)), BindOk("-a", Context(Variable(1, "a"), Variable(2, "-a"))));
        }

        [Fact]
        public void OPERATORINNAME_NO_ARRASTRA_LOS_ERRORES_DE_SUS_PARTES()
        {
            var diagnostic = Assert.Single(BindFails("Foo-Bar * 2", Context(Variable(1, "Foo-Bar"))));

            Assert.Equal(ExpressionDiagnosticCode.OperatorInName, diagnostic.Code);
            Assert.Equal(new SourceSpan(0, 7), diagnostic.Span);
        }

        /// <summary>
        /// A1 §5: sin guarda de caracteres, un nombre también puede ser muy largo al enlazar, y la comprobación de
        /// <c>OperatorInName</c> no puede reintroducir un coste cuadrático en su longitud.
        /// </summary>
        [Fact]
        public void OPERATORINNAME_CON_NOMBRES_Y_TRAMOS_LARGOS()
        {
            var parte = new string('x', 200000);
            var nombre = parte + "-" + parte;

            var largo = Assert.Single(BindFails(nombre, Context(Variable(1, nombre), Variable(2, parte))));
            Assert.Equal(ExpressionDiagnosticCode.OperatorInName, largo.Code);
            Assert.Equal(new SourceSpan(0, nombre.Length), largo.Span);

            var tramo = string.Join("-", Enumerable.Repeat("v", 1900));
            var patron = string.Join("-", Enumerable.Repeat("v", 900));
            var diagnostics = BindFails(tramo, Context(Variable(1, "v"), Variable(2, patron)));

            Assert.Equal(ExpressionLimits.MaxBinderDiagnostics, diagnostics.Count);
            Assert.Contains(
                diagnostics,
                diagnostic => diagnostic.Code == ExpressionDiagnosticCode.OperatorInName
                              && diagnostic.Span == new SourceSpan(0, patron.Length));
        }

        // ================================================================ ámbito (P5.3, P25.4)

        [Fact]
        public void UNA_DEFINICION_PROJECT_NO_VE_UN_SIMBOLO_RACK_SINTETICO()
        {
            var diagnostic = Assert.Single(BindFails("SoloRack + 1", Ctx(), SymbolScope.Project));

            Assert.Equal(ExpressionDiagnosticCode.ScopeViolation, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            Assert.Equal(new SourceSpan(0, 8), diagnostic.Span);
            Assert.Equal(new[] { Id(9) }, diagnostic.RelatedSymbols);

            Assert.Equal(Id(9), RefId(BindOk("SoloRack", Ctx(), SymbolScope.Rack)));
            Assert.Equal(Id(1), RefId(BindOk("Holgura", Ctx(), SymbolScope.Rack)));
        }

        // ================================================================ funciones (P10)

        [Theory]
        [InlineData("MIN(1, 2)", FunctionId.Min)]
        [InlineData("min(1, 2)", FunctionId.Min)]
        [InlineData("Max(1, 2)", FunctionId.Max)]
        [InlineData("abs(-1)", FunctionId.Abs)]
        public void LAS_FUNCIONES_NO_DISTINGUEN_MAYUSCULAS_A_LA_ENTRADA(string text, FunctionId function)
        {
            Assert.Equal(function, Assert.IsType<BoundCall>(BindOk(text, Ctx())).Function);
        }

        [Theory]
        [InlineData("FOO(1)", 0, 3)]
        [InlineData("Rack(1)", 0, 4)]
        [InlineData("ROUND(1.5)", 0, 5)]
        [InlineData("2 * CEILING(1)", 4, 7)]
        public void UNA_FUNCION_DESCONOCIDA_DA_UNKNOWNFUNCTION(string text, int start, int length)
        {
            var diagnostic = Assert.Single(BindFails(text, Ctx()));

            Assert.Equal(ExpressionDiagnosticCode.UnknownFunction, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            Assert.Equal(new SourceSpan(start, length), diagnostic.Span);
        }

        [Theory]
        [InlineData("MIN(1)", 0, 6)]
        [InlineData("MAX()", 0, 5)]
        [InlineData("ABS(1, 2)", 0, 9)]
        [InlineData("ABS()", 0, 5)]
        [InlineData("1 + min(Holgura)", 4, 12)]
        public void UNA_ARIDAD_ERRONEA_DA_INVALIDARGUMENTS(string text, int start, int length)
        {
            var diagnostic = Assert.Single(BindFails(text, Ctx()));

            Assert.Equal(ExpressionDiagnosticCode.InvalidArguments, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Semantic, diagnostic.Class);
            Assert.Equal(new SourceSpan(start, length), diagnostic.Span);
        }

        // ================================================================ forma enlazada (P2.1)

        [Fact]
        public void EL_ENLACE_QUITA_EL_MAS_UNARIO_Y_LOS_PARENTESIS_REDUNDANTES()
        {
            var ctx = Ctx();

            Assert.Equal(Ref(1), BindOk("+(((Holgura)))", ctx));
            Assert.Equal(Num(1), BindOk("+1", ctx));
            Assert.Equal(Neg(Num(1)), BindOk("-(1)", ctx));
            Assert.Equal(Neg(Neg(Num(1))), BindOk("--1", ctx));
            Assert.Equal(Mul(Add(Num(1), Num(2)), Num(3)), BindOk("(1 + 2) * 3", ctx));
            Assert.Equal(Add(Num(1), Mul(Num(2), Num(3))), BindOk("1 + (2 * 3)", ctx));
            Assert.Equal(Sub(Sub(Num(1), Num(2)), Num(3)), BindOk("1 - 2 - 3", ctx));
            Assert.Equal(Sub(Num(1), Sub(Num(2), Num(3))), BindOk("1 - (2 - 3)", ctx));
            Assert.Equal(Div(Neg(Ref(1)), Ref(2)), BindOk("-Holgura / Base", ctx));
        }

        [Fact]
        public void LAS_UNIDADES_SE_ENLAZAN_CON_SU_TOKEN_SIN_CONVERTIR()
        {
            var ctx = Ctx();

            Assert.Equal(Num(100, LengthUnit.Millimeter), BindOk("100[mm]", ctx));
            Assert.Equal(Num(4, LengthUnit.Inch), BindOk("4 [ in ]", ctx));
            Assert.Equal(Num(20, LengthUnit.Foot), BindOk("20[ft]", ctx));
            Assert.Equal(Num(100), BindOk("100", ctx));

            // P2.3: la unidad forma parte de la igualdad.
            Assert.NotEqual(Num(100, LengthUnit.Millimeter), Num(100));
            Assert.NotEqual(Num(100, LengthUnit.Millimeter), Num(100 / 25.4));
        }

        [Fact]
        public void EL_BINDER_DEVUELVE_UN_ARBOL_SOLO_CON_CERO_DIAGNOSTICOS()
        {
            var fallo = Bind("Inexistente", Ctx());
            Assert.False(fallo.Succeeded);
            Assert.Throws<InvalidOperationException>(() => fallo.Expression);

            var exito = Bind("Holgura", Ctx());
            Assert.True(exito.Succeeded);
            Assert.Empty(exito.Diagnostics);
            Assert.Equal(Ref(1), exito.Expression);

            var parsed = ExpressionParser.Parse("1");
            Assert.ThrowsAny<ArgumentException>(() => ExpressionBinder.Bind(null, Ctx(), SymbolScope.Project));
            Assert.ThrowsAny<ArgumentException>(() => ExpressionBinder.Bind(parsed.Syntax, null, SymbolScope.Project));
            Assert.ThrowsAny<ArgumentException>(() => ExpressionBinder.Bind(parsed.Syntax, Ctx(), (SymbolScope)0));
        }

        // ================================================================ total, ordenado y con tope (P7.4, P15.7)

        [Fact]
        public void EL_BINDER_ES_TOTAL_Y_ENTREGA_LOS_DIAGNOSTICOS_POR_POSICION()
        {
            var text = "SoloRack + Inexistente * #" + Key(99) + " + MIN + FOO(1) + Rack.X + ABS(1, 2) - Holgura General";

            Assert.Equal(
                new[]
                {
                    ExpressionDiagnosticCode.ScopeViolation,
                    ExpressionDiagnosticCode.UnknownSymbol,
                    ExpressionDiagnosticCode.BrokenReference,
                    ExpressionDiagnosticCode.ReservedName,
                    ExpressionDiagnosticCode.UnknownFunction,
                    ExpressionDiagnosticCode.UnknownNamespace,
                    ExpressionDiagnosticCode.InvalidArguments,
                    ExpressionDiagnosticCode.AmbiguousName,
                },
                BindFails(text, Ctx()).Select(diagnostic => diagnostic.Code));
        }

        [Fact]
        public void COMO_MUCHO_VEINTE_DIAGNOSTICOS_Y_SIEMPRE_LOS_PRIMEROS()
        {
            // 24 operandos: dentro de la profundidad normativa, así que solo hay símbolos desconocidos.
            var text = string.Join(" + ", Enumerable.Range(1, 24).Select(i => "U" + i));

            var diagnostics = BindFails(text, Ctx());

            Assert.Equal(20, diagnostics.Count);
            Assert.All(diagnostics, diagnostic => Assert.Equal(ExpressionDiagnosticCode.UnknownSymbol, diagnostic.Code));
            Assert.Equal(
                Enumerable.Range(1, 20).Select(i => text.IndexOf("U" + i + " ", StringComparison.Ordinal)),
                diagnostics.Select(diagnostic => diagnostic.Span.Value.Start));
        }

        [Fact]
        public void EL_ENLACE_NO_DEPENDE_DE_LA_CULTURA()
        {
            var esperado = BindOk("MIN(Holgura, 4.5[in]) / 2", Ctx());

            WithCulture("tr-TR", () => Assert.Equal(esperado, BindOk("min(holgura, 4.5[in]) / 2", Ctx())));
            WithCulture("de-DE", () => Assert.Equal(esperado, BindOk("MIN(Holgura, 4.5[in]) / 2", Ctx())));
        }
    }
}
