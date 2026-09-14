using System.Linq;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — límites NORMATIVOS del árbol enlazado (V6 P1.9, P7.5; ADR-0040 D8): nodos ≤ 256, profundidad del
    /// <c>BoundExpression</c> ≤ 24 y argumentos por llamada ≤ 16.
    ///
    /// <para>
    /// La profundidad es el número máximo de nodos en un camino raíz→hoja, contando los dos extremos, y la asociatividad
    /// cuenta: una suma plana de n operandos tiene profundidad n (T-V4-04). Los límites se validan tras el enlace, nunca
    /// en el parser, y superar uno nunca trunca. La aridad de una función (MIN y MAX 2..16, ABS 1) es otra regla.
    /// </para>
    /// </summary>
    public class ExpressionBoundLimitsTests
    {
        [Fact]
        public void LOS_LIMITES_NORMATIVOS_TIENEN_LOS_VALORES_DE_V6()
        {
            Assert.Equal(256, ExpressionLimits.MaxNodeCount);
            Assert.Equal(24, ExpressionLimits.MaxBoundExpressionDepth);
            Assert.Equal(16, ExpressionLimits.MaxArgumentCount);
            Assert.Equal(20, ExpressionLimits.MaxBinderDiagnostics);

            var normativos = ExpressionLimits.Normative;
            Assert.Equal(256, normativos.NodeCount);
            Assert.Equal(24, normativos.BoundExpressionDepth);
            Assert.Equal(16, normativos.ArgumentCount);
            Assert.Equal(20, normativos.BinderDiagnostics);

            // A1.2: la guarda de tokens del parser nunca baja de 6 × el máximo normativo de nodos.
            Assert.Equal(1536, 6 * ExpressionLimits.MaxNodeCount);
            Assert.InRange(ExpressionParser.MaxSyntacticTokens, 6 * ExpressionLimits.MaxNodeCount, int.MaxValue);
            Assert.InRange(ExpressionParser.MaxSyntacticNesting, ExpressionLimits.MaxBoundExpressionDepth, int.MaxValue);
        }

        /// <summary>Los ejemplos normativos de P1.9, más dos que muestran que paréntesis y más unario no son nodos.</summary>
        [Theory]
        [InlineData("1", 1, 1)]
        [InlineData("A", 1, 1)]
        [InlineData("-1", 2, 2)]
        [InlineData("1 + 2", 2, 3)]
        [InlineData("(1 + 2) + 3", 3, 5)]
        [InlineData("ABS(A)", 2, 2)]
        [InlineData("MIN(A, B + C)", 3, 5)]
        [InlineData("((((1))))", 1, 1)]
        [InlineData("+(+(+1))", 1, 1)]
        public void PROFUNDIDAD_Y_NODOS_SIGUEN_LA_DEFINICION_NORMATIVA(string text, int depth, int nodes)
        {
            var bound = BindOk(text, Context(Variable(1, "A"), Variable(2, "B"), Variable(3, "C")));

            Assert.Equal(depth, bound.Depth);
            Assert.Equal(nodes, bound.NodeCount);
        }

        /// <summary>T-V4-04, parte de G6.</summary>
        [Fact]
        public void LA_SUMA_PLANA_DE_24_OPERANDOS_PASA_Y_LA_DE_25_DA_LIMITEXCEEDED_TRAS_EL_ENLACE()
        {
            var veinticuatro = string.Join(" + ", Enumerable.Repeat("1", 24));
            Assert.Equal(24, BindOk(veinticuatro, Context()).Depth);

            var veinticinco = string.Join(" + ", Enumerable.Repeat("1", 25));
            Assert.True(ExpressionParser.Parse(veinticinco).Succeeded, "las guardas del parser no saltan");

            var diagnostic = Assert.Single(BindFails(veinticinco, Context()));
            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.SyntaxAndLimits, diagnostic.Class);
            Assert.Equal(ExpressionLimitKind.BoundExpressionDepth, diagnostic.Limit);
            Assert.Equal(24, diagnostic.LimitMaximum);
            Assert.Equal(new SourceSpan(0, veinticinco.Length), diagnostic.Span);
        }

        /// <summary>T-V4-02 y T-V4-03, parte de G6: profundidad exacta 24 con llamadas y con negaciones, y 25.</summary>
        [Fact]
        public void LA_PROFUNDIDAD_EXACTA_24_SE_COMPROMETE_Y_LA_25_NO()
        {
            Assert.Equal(24, BindOk(Repeat("ABS(", 23) + "1" + new string(')', 23), Context()).Depth);
            Assert.Equal(24, BindOk(new string('-', 23) + "1", Context()).Depth);

            foreach (var text in new[] { Repeat("ABS(", 24) + "1" + new string(')', 24), new string('-', 24) + "1" })
            {
                var diagnostic = Assert.Single(BindFails(text, Context()));
                Assert.Equal(ExpressionLimitKind.BoundExpressionDepth, diagnostic.Limit);
                Assert.Equal(24, diagnostic.LimitMaximum);
            }
        }

        [Fact]
        public void DOSCIENTOS_CINCUENTA_Y_SEIS_NODOS_PASAN_Y_257_DAN_LIMITEXCEEDED()
        {
            var interior = "MIN(" + string.Join(", ", Enumerable.Repeat("1", 16)) + ")";

            var justo = "MIN(" + string.Join(", ", Enumerable.Repeat(interior, 15)) + ")";
            var bound = BindOk(justo, Context());
            Assert.Equal(256, bound.NodeCount);
            Assert.Equal(3, bound.Depth);

            var sobra = "MIN(" + string.Join(", ", Enumerable.Repeat(interior, 15).Concat(new[] { "1" })) + ")";
            var diagnostic = Assert.Single(BindFails(sobra, Context()));
            Assert.Equal(ExpressionDiagnosticCode.LimitExceeded, diagnostic.Code);
            Assert.Equal(ExpressionLimitKind.NodeCount, diagnostic.Limit);
            Assert.Equal(256, diagnostic.LimitMaximum);
            Assert.Equal(new SourceSpan(0, sobra.Length), diagnostic.Span);
        }

        [Fact]
        public void DIECISEIS_ARGUMENTOS_PASAN_Y_DIECISIETE_DAN_LIMITEXCEEDED_EN_LA_LLAMADA()
        {
            var dieciseis = "MAX(" + string.Join(", ", Enumerable.Repeat("1", 16)) + ")";
            Assert.Equal(16, Assert.IsType<BoundCall>(BindOk(dieciseis, Context())).Arguments.Count);

            var diecisiete = "2 * MAX(" + string.Join(", ", Enumerable.Repeat("1", 17)) + ")";
            var diagnostics = BindFails(diecisiete, Context());

            var limite = Assert.Single(diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.LimitExceeded);
            Assert.Equal(ExpressionLimitKind.ArgumentCount, limite.Limit);
            Assert.Equal(16, limite.LimitMaximum);
            Assert.Equal(new SourceSpan(4, diecisiete.Length - 4), limite.Span);

            // La aridad de MAX (2..16) es una regla distinta y también se informa (P10.4).
            Assert.Contains(diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.InvalidArguments);

            // El límite es de la llamada, conozca o no el registro la función.
            var desconocida = BindFails("F(" + string.Join(", ", Enumerable.Repeat("1", 17)) + ")", Context());
            Assert.Contains(desconocida, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.UnknownFunction);
            Assert.Contains(desconocida, diagnostic => diagnostic.Limit == ExpressionLimitKind.ArgumentCount);
        }

        [Fact]
        public void LA_ARIDAD_DE_UNA_FUNCION_NO_ES_EL_LIMITE_NORMATIVO()
        {
            var diagnostic = Assert.Single(BindFails("MIN(1)", Context()));

            Assert.Equal(ExpressionDiagnosticCode.InvalidArguments, diagnostic.Code);
            Assert.Null(diagnostic.Limit);
        }

        [Fact]
        public void LOS_LIMITES_SE_INFORMAN_AUNQUE_HAYA_OTROS_ERRORES()
        {
            var text = "Inexistente + " + string.Join(" + ", Enumerable.Repeat("1", 24));

            var diagnostics = BindFails(text, Context());

            Assert.Contains(diagnostics, diagnostic => diagnostic.Limit == ExpressionLimitKind.BoundExpressionDepth);
            Assert.Contains(diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.UnknownSymbol);
        }
    }
}
