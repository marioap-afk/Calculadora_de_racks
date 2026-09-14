using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — evaluador de UN árbol enlazado sobre valores puros suministrados (V6 P5.6, P9.3, P14.4, P14.5, P16;
    /// ADR-0040 D2).
    ///
    /// <para>
    /// El motor es adimensional: acepta cualquier <c>double</c> finito, no rechaza ninguna operación por dimensiones ni
    /// por signo, convierte cada unidad al evaluar su literal y falla cerrado con <c>DivisionByZero</c> o
    /// <c>NonFiniteResult</c>, sin valor parcial ni fallback. G6 NO evalúa definiciones de la tabla, ni grafo, ni orden, ni
    /// <c>RegistryEvaluation</c> (G7): lee los valores que le da quien llama.
    /// </para>
    /// </summary>
    public class ExpressionEvaluatorTests
    {
        private static readonly SymbolEntry A = Variable(1, "A");
        private static readonly SymbolEntry B = Variable(2, "B");
        private static readonly SymbolEntry Base = Variable(3, "Base");

        private static ExpressionContext Ctx() => Context(A, B, Base);

        private static IReadOnlyDictionary<SymbolId, double> V(double a = 7, double b = 2, double @base = 10)
            => Values((Id(1), a), (Id(2), b), (Id(3), @base));

        private static double Eval(string text, IReadOnlyDictionary<SymbolId, double> values = null)
            => EvaluateOk(BindOk(text, Ctx()), Ctx(), values ?? V());

        private static ExpressionDiagnostic Fails(string text, IReadOnlyDictionary<SymbolId, double> values = null)
        {
            var result = ExpressionEvaluator.Evaluate(BindOk(text, Ctx()), Ctx(), values ?? V());

            Assert.Equal(EvaluationOutcome.Failed, result.Outcome);
            Assert.False(result.Succeeded);
            Assert.Throws<InvalidOperationException>(() => result.Value);
            return Assert.Single(result.Diagnostics);
        }

        // ================================================================ adimensional (P5.6, P28.3)

        [Fact]
        public void EL_MOTOR_ES_ADIMENSIONAL_Y_DA_VALORES_EXACTOS()
        {
            AssertBits(8, Eval("6 + 2"));
            AssertBits(9, Eval("A + 2"));
            AssertBits(14, Eval("A * B"));
            AssertBits(3.5, Eval("A / B"));
            AssertBits(100 / 25.4 + 2, Eval("100[mm] + 2"));
            AssertBits(7, Eval("MAX(A, 4[in])"));
            AssertBits(15, Eval("Base + (A - B)"));

            // Un intermedio negativo es legal: 10 + (2 - 7) = 5.
            AssertBits(5, Eval("Base + (B - A)"));

            // Ni el signo ni el cero son fronteras del motor: son del adaptador y del consumidor (P5.7).
            AssertBits(-5, Eval("B - A"));
            AssertBits(0, Eval("A - A"));
        }

        [Fact]
        public void LAS_UNIDADES_SE_CONVIERTEN_AL_EVALUAR_SU_LITERAL_BIT_A_BIT()
        {
            AssertBits(4, Eval("4[in]"));
            AssertBits(240, Eval("20[ft]"));
            AssertBits(100 / 25.4, Eval("100[mm]"));
            AssertBits(6, Eval("0.5[ft]"));
            AssertBits(1, Eval("25.4[mm]"));
            AssertBits(240 + 100 / 25.4, Eval("20[ft] + 100[mm]"));
        }

        // ================================================================ fail-closed (P14.4, P15.5)

        [Theory]
        [InlineData("1 / 0")]
        [InlineData("A / (B - 2)")]
        [InlineData("A / -0")]
        [InlineData("0 / 0")]
        [InlineData("MAX(1, 2 / (A - A))")]
        public void DIVIDIR_ENTRE_CERO_DA_DIVISIONBYZERO_SIN_VALOR(string text)
        {
            var diagnostic = Fails(text);

            Assert.Equal(ExpressionDiagnosticCode.DivisionByZero, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Semantic, diagnostic.Class);
            Assert.Null(diagnostic.Span);
        }

        [Fact]
        public void UN_RESULTADO_NO_FINITO_INTERMEDIO_O_FINAL_DA_NONFINITERESULT()
        {
            var maximo = V(a: double.MaxValue);

            foreach (var text in new[] { "A * 2", "A + A", "A * 2 - A * 2", "A * 10 / 20", "MAX(A * 2, 1)", "0 - A - A" })
            {
                var diagnostic = Fails(text, maximo);
                Assert.Equal(ExpressionDiagnosticCode.NonFiniteResult, diagnostic.Code);
                Assert.Equal(ExpressionDiagnosticClass.Semantic, diagnostic.Class);
            }

            var enorme = "17976931348623157" + new string('0', 292);
            Assert.Equal(ExpressionDiagnosticCode.NonFiniteResult, Fails(enorme + "[ft]").Code);
            Assert.Equal(ExpressionDiagnosticCode.NonFiniteResult, Fails(enorme + " * 10").Code);
        }

        [Fact]
        public void UNA_REFERENCIA_A_UN_ID_AUSENTE_DA_BROKENREFERENCE_SIN_EVALUAR_NADA()
        {
            var tree = Add(Ref(1), Add(Ref(99), Mul(Ref(98), Div(Ref(99), Num(0)))));

            var result = ExpressionEvaluator.Evaluate(tree, Ctx(), V());

            Assert.False(result.Succeeded);
            Assert.Equal(
                new[] { ExpressionDiagnosticCode.BrokenReference, ExpressionDiagnosticCode.BrokenReference },
                result.Diagnostics.Select(diagnostic => diagnostic.Code));
            Assert.Equal(new[] { Id(99), Id(98) }, result.Diagnostics.SelectMany(diagnostic => diagnostic.RelatedSymbols));
            Assert.All(result.Diagnostics, diagnostic => Assert.Equal(ExpressionDiagnosticClass.Semantic, diagnostic.Class));
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void UN_ARBOL_CON_ARIDAD_ERRONEA_DA_INVALIDARGUMENTS_AL_EVALUAR()
        {
            var result = ExpressionEvaluator.Evaluate(Call(FunctionId.Abs, Num(1), Num(2)), Context(), Values());

            Assert.False(result.Succeeded);
            Assert.Equal(ExpressionDiagnosticCode.InvalidArguments, Assert.Single(result.Diagnostics).Code);
        }

        [Fact]
        public void EL_EXITO_TIENE_VALOR_Y_NINGUN_DIAGNOSTICO()
        {
            var result = ExpressionEvaluator.Evaluate(BindOk("A + 1", Ctx()), Ctx(), V());

            Assert.Equal(EvaluationOutcome.Success, result.Outcome);
            Assert.True(result.Succeeded);
            Assert.Empty(result.Diagnostics);
            AssertBits(8, result.Value);
        }

        [Fact]
        public void LAS_ENTRADAS_INVALIDAS_SON_ERRORES_DE_PROGRAMACION()
        {
            var tree = BindOk("A + B", Ctx());

            Assert.Throws<ArgumentException>(() => ExpressionEvaluator.Evaluate(tree, Ctx(), Values((Id(1), 7))));
            Assert.Throws<ArgumentException>(() => ExpressionEvaluator.Evaluate(tree, Ctx(), Values((Id(1), double.NaN), (Id(2), 1))));
            Assert.Throws<ArgumentException>(() => ExpressionEvaluator.Evaluate(tree, Ctx(), Values((Id(1), 1), (Id(2), double.NegativeInfinity))));
            Assert.Throws<ArgumentNullException>(() => ExpressionEvaluator.Evaluate(null, Ctx(), V()));
            Assert.Throws<ArgumentNullException>(() => ExpressionEvaluator.Evaluate(tree, null, V()));
            Assert.Throws<ArgumentNullException>(() => ExpressionEvaluator.Evaluate(tree, Ctx(), null));
        }

        // ================================================================ determinismo y traza (P14.5, P16.3)

        [Fact]
        public void LA_EVALUACION_ES_DETERMINISTA_BIT_A_BIT_E_INDEPENDIENTE_DE_LA_CULTURA()
        {
            const string text = "MAX(A / 3, 100[mm]) * 1.1 - ABS(B - 20[ft]) / 7";
            var primero = Eval(text);

            for (var i = 0; i < 3; i++)
            {
                AssertBits(primero, Eval(text));
            }

            WithCulture("de-DE", () => AssertBits(primero, Eval(text)));
            WithCulture("ar-SA", () => AssertBits(primero, Eval(text)));
        }

        [Fact]
        public void LA_TRAZA_ES_OPCIONAL_Y_NO_CAMBIA_NINGUN_RESULTADO()
        {
            var tree = BindOk("A + B * A", Ctx());

            var sin = ExpressionEvaluator.Evaluate(tree, Ctx(), V());
            var con = ExpressionEvaluator.Evaluate(tree, Ctx(), V(), includeTrace: true);

            Assert.True(sin.Succeeded);
            Assert.True(con.Succeeded);
            AssertBits(sin.Value, con.Value);
            Assert.Null(sin.Trace);
            Assert.NotNull(con.Trace);
            Assert.Same(tree, con.Trace.Expression);
            Assert.Equal(
                new[] { new KeyValuePair<SymbolId, double>(Id(1), 7), new KeyValuePair<SymbolId, double>(Id(2), 2) },
                con.Trace.Inputs);

            var falloSin = ExpressionEvaluator.Evaluate(BindOk("A / 0", Ctx()), Ctx(), V());
            var falloCon = ExpressionEvaluator.Evaluate(BindOk("A / 0", Ctx()), Ctx(), V(), includeTrace: true);
            Assert.Equal(falloSin.Diagnostics.Select(d => d.Code), falloCon.Diagnostics.Select(d => d.Code));
            Assert.Equal(ExpressionDiagnosticCode.DivisionByZero, Assert.Single(falloCon.Diagnostics).Code);
        }

        /// <summary>
        /// G6 no es <c>RegistryEvaluation</c>: no evalúa la definición de un símbolo, ni detecta ciclos, ni propaga fallos
        /// de dependencias. Lee el valor suministrado aunque la definición de la tabla no fuera evaluable.
        /// </summary>
        [Fact]
        public void EL_EVALUADOR_USA_LOS_VALORES_SUMINISTRADOS_Y_NO_EVALUA_DEFINICIONES()
        {
            var rota = new SymbolEntry(Id(1), SymbolScope.Project, "A", SymbolDefinition.FromExpression(Div(Num(1), Num(0))));
            var ctx = ExpressionContext.Create(Table(rota));

            AssertBits(8, EvaluateOk(BindOk("A + 1", ctx), ctx, Values((Id(1), 7))));
        }
    }
}
