using System;
using System.Linq;
using System.Reflection;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — <c>FunctionRegistry</c> cerrado e inmutable (V6 P10; ADR-0040 D7, D9): <c>MIN</c>, <c>MAX</c> y
    /// <c>ABS</c>, con una sola instancia productiva, tokens canónicos en mayúsculas comparados en <c>Ordinal</c>, entrada
    /// sin distinguir mayúsculas y la misma tabla como fuente de las palabras reservadas. <c>ROUND</c>, <c>CEILING</c> y
    /// <c>FLOOR</c> siguen diferidas, y <c>IF</c>, <c>AND</c>, <c>OR</c> y demás, excluidas.
    /// </summary>
    public class ExpressionFunctionRegistryTests
    {
        [Fact]
        public void EL_REGISTRO_PRODUCTIVO_ES_UNICO_Y_CERRADO_CON_MIN_MAX_Y_ABS()
        {
            var registry = FunctionRegistry.Productive;

            Assert.Same(registry, FunctionRegistry.Productive);
            Assert.Equal(new[] { FunctionId.Min, FunctionId.Max, FunctionId.Abs }, registry.Functions);
            Assert.Equal(new[] { 1, 2, 3 }, Enum.GetValues<FunctionId>().Select(value => (int)value));
            Assert.Empty(typeof(FunctionRegistry).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        }

        [Theory]
        [InlineData(FunctionId.Min, "MIN", 2, 16)]
        [InlineData(FunctionId.Max, "MAX", 2, 16)]
        [InlineData(FunctionId.Abs, "ABS", 1, 1)]
        public void CADA_FUNCION_TIENE_SU_TOKEN_CANONICO_Y_SU_ARIDAD(FunctionId function, string token, int minimum, int maximum)
        {
            var registry = FunctionRegistry.Productive;

            Assert.Equal(token, registry.Token(function));
            Assert.True(registry.TryParseToken(token, out var parsed));
            Assert.Equal(function, parsed);
            Assert.Equal(minimum, registry.MinimumArity(function));
            Assert.Equal(maximum, registry.MaximumArity(function));
        }

        [Theory]
        [InlineData("min", FunctionId.Min)]
        [InlineData("Min", FunctionId.Min)]
        [InlineData("MIN", FunctionId.Min)]
        [InlineData("mAx", FunctionId.Max)]
        [InlineData("abs", FunctionId.Abs)]
        public void EL_NOMBRE_DE_ENTRADA_NO_DISTINGUE_MAYUSCULAS(string name, FunctionId function)
        {
            Assert.True(FunctionRegistry.Productive.TryResolveName(name, out var resolved));
            Assert.Equal(function, resolved);
        }

        [Theory]
        [InlineData("ROUND")]
        [InlineData("CEILING")]
        [InlineData("FLOOR")]
        [InlineData("IF")]
        [InlineData("AND")]
        [InlineData("OR")]
        [InlineData("SIN")]
        [InlineData("LOOKUP")]
        [InlineData("SUM")]
        [InlineData("MINIMUM")]
        [InlineData("MIN ")]
        [InlineData("")]
        public void NINGUNA_OTRA_FUNCION_EXISTE(string name)
        {
            var registry = FunctionRegistry.Productive;

            Assert.False(registry.TryResolveName(name, out _), name);
            Assert.False(registry.TryParseToken(name, out _), name);
        }

        [Fact]
        public void EL_TOKEN_PERSISTIDO_COMPARA_ORDINAL_Y_LOS_DESCONOCIDOS_LANZAN()
        {
            var registry = FunctionRegistry.Productive;

            Assert.False(registry.TryParseToken("min", out _));
            Assert.False(registry.TryParseToken("Max", out _));
            Assert.False(registry.TryParseToken(null, out _));
            Assert.False(registry.TryResolveName(null, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => registry.Token((FunctionId)4));
            Assert.Throws<ArgumentOutOfRangeException>(() => registry.MinimumArity((FunctionId)0));
        }

        [Theory]
        [InlineData("MIN", true)]
        [InlineData("max", true)]
        [InlineData("Abs", true)]
        [InlineData("Rack", true)]
        [InlineData("rack", true)]
        [InlineData("PROJECT", true)]
        [InlineData("Minimum", false)]
        [InlineData("Racks", false)]
        [InlineData("Rack Frentes", false)]
        [InlineData("ROUND", false)]
        [InlineData("", false)]
        public void LAS_PALABRAS_RESERVADAS_SON_LAS_FUNCIONES_MAS_RACK_Y_PROJECT(string name, bool reserved)
        {
            Assert.Equal(reserved, ExpressionReservedNames.IsReserved(name));
        }

        [Fact]
        public void MIN_MAX_Y_ABS_DAN_SUS_VALORES()
        {
            var ctx = Context(Variable(1, "A"));

            AssertBits(-1, EvaluateOk(BindOk("MIN(3, 0 - 1, 2)", ctx), ctx, Values((Id(1), 0))));
            AssertBits(3, EvaluateOk(BindOk("MAX(3, 0 - 1, 2)", ctx), ctx, Values((Id(1), 0))));
            AssertBits(2.5, EvaluateOk(BindOk("ABS(0 - 2.5)", ctx), ctx, Values((Id(1), 0))));
            AssertBits(0, EvaluateOk(BindOk("ABS(0)", ctx), ctx, Values((Id(1), 0))));

            var dieciseis = string.Join(", ", Enumerable.Range(1, 16).Select(i => (17 - i).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            AssertBits(1, EvaluateOk(BindOk("MIN(" + dieciseis + ")", ctx), ctx, Values((Id(1), 0))));
            AssertBits(16, EvaluateOk(BindOk("MAX(" + dieciseis + ")", ctx), ctx, Values((Id(1), 0))));

            AssertBits(4, EvaluateOk(BindOk("MAX(A, 4[in])", ctx), ctx, Values((Id(1), 3))));
            AssertBits(5, EvaluateOk(BindOk("MAX(A, 4[in])", ctx), ctx, Values((Id(1), 5))));
        }
    }
}
