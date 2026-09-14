using System;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — clasificación neutral de la forma canónica (V6 P2.8; ADR-0040 D4): una sola representación por
    /// significado. Solo una referencia es referencia directa; solo un número sin unidad (o su negación) es literal;
    /// cualquier otro árbol —operación, función o unidad explícita— es expresión. G6 solo clasifica: los casos de
    /// <c>VariableDefinition</c> y de <c>LinkedPropertySource</c> son de G8.
    /// </summary>
    public class ExpressionCanonicalShapeTests
    {
        private static ExpressionContext Ctx() => Context(Variable(1, "Holgura"));

        [Fact]
        public void SOLO_UNA_REFERENCIA_ES_REFERENCIA_DIRECTA()
        {
            var shape = CanonicalShape.Classify(BindOk("Holgura", Ctx()));

            Assert.Equal(CanonicalShapeKind.DirectReference, shape.Kind);
            Assert.Equal(Id(1), shape.Reference);
            Assert.Throws<InvalidOperationException>(() => shape.LiteralValue);
            Assert.Throws<InvalidOperationException>(() => shape.Expression);

            // =Holgura y seleccionar Holgura en la lista persisten lo mismo.
            Assert.Equal(CanonicalShapeKind.DirectReference, CanonicalShape.Classify(BindOk("(+Holgura)", Ctx())).Kind);
        }

        [Theory]
        [InlineData("6", 6)]
        [InlineData("-2", -2)]
        [InlineData("-(2)", -2)]
        [InlineData("0.5", 0.5)]
        [InlineData("+(6)", 6)]
        public void SOLO_UN_NUMERO_SIN_UNIDAD_O_SU_NEGACION_ES_LITERAL(string text, double value)
        {
            var shape = CanonicalShape.Classify(BindOk(text, Ctx()));

            Assert.Equal(CanonicalShapeKind.Literal, shape.Kind);
            AssertBits(value, shape.LiteralValue);
            Assert.Throws<InvalidOperationException>(() => shape.Reference);
            Assert.Throws<InvalidOperationException>(() => shape.Expression);
        }

        [Theory]
        [InlineData("4[in]")]
        [InlineData("-4[in]")]
        [InlineData("--2")]
        [InlineData("Holgura + 0")]
        [InlineData("MAX(Holgura, 1)")]
        [InlineData("-Holgura")]
        [InlineData("2 * 3")]
        [InlineData("ABS(2)")]
        public void CUALQUIER_OTRO_ARBOL_ES_EXPRESION(string text)
        {
            var bound = BindOk(text, Ctx());
            var shape = CanonicalShape.Classify(bound);

            Assert.Equal(CanonicalShapeKind.Expression, shape.Kind);
            Assert.Same(bound, shape.Expression);
            Assert.Throws<InvalidOperationException>(() => shape.LiteralValue);
            Assert.Throws<InvalidOperationException>(() => shape.Reference);
        }

        [Fact]
        public void UNA_UNIDAD_EXPLICITA_ES_EXPRESION_AUNQUE_VALGA_LO_MISMO()
        {
            Assert.Equal(CanonicalShapeKind.Expression, CanonicalShape.Classify(Num(4, LengthUnit.Inch)).Kind);
            Assert.Equal(CanonicalShapeKind.Literal, CanonicalShape.Classify(Num(4)).Kind);
            Assert.Throws<ArgumentNullException>(() => CanonicalShape.Classify(null));
        }
    }
}
