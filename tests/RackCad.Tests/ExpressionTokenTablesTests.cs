using System;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — tablas de tokens cerradas y explícitas, en los dos sentidos (V6 P2.4, P17.10; ADR-0040 D9).
    ///
    /// <para>
    /// Nodos, operadores, namespaces, unidades y funciones tienen su token declarado a mano y comparado en
    /// <c>Ordinal</c>. Ninguno sale de <c>enum.ToString()</c>, <c>nameof</c>, <c>Type.ToString()</c> ni del nombre de una
    /// clase: renombrar un miembro no puede cambiar lo que se escribe ni lo que se lee. G8 persistirá con estas mismas
    /// tablas; G6 no persiste nada.
    /// </para>
    /// </summary>
    public class ExpressionTokenTablesTests
    {
        [Fact]
        public void LOS_NODOS_TIENEN_TOKENS_EXPLICITOS_EN_LOS_DOS_SENTIDOS()
        {
            var casos = new (BoundExpression Node, string Token, BoundExpressionKind Kind, BoundBinaryOperator? Operator)[]
            {
                (Num(1), "number", BoundExpressionKind.Number, null),
                (Ref(1), "ref", BoundExpressionKind.Reference, null),
                (Neg(Num(1)), "neg", BoundExpressionKind.Negate, null),
                (Add(Num(1), Num(2)), "add", BoundExpressionKind.Binary, BoundBinaryOperator.Add),
                (Sub(Num(1), Num(2)), "sub", BoundExpressionKind.Binary, BoundBinaryOperator.Subtract),
                (Mul(Num(1), Num(2)), "mul", BoundExpressionKind.Binary, BoundBinaryOperator.Multiply),
                (Div(Num(1), Num(2)), "div", BoundExpressionKind.Binary, BoundBinaryOperator.Divide),
                (Call(FunctionId.Abs, Num(1)), "call", BoundExpressionKind.Call, null),
            };

            foreach (var caso in casos)
            {
                Assert.Equal(caso.Token, BoundExpressionTokens.NodeToken(caso.Node));
                Assert.True(BoundExpressionTokens.TryParseNodeToken(caso.Token, out var kind, out var binaryOperator), caso.Token);
                Assert.Equal(caso.Kind, kind);
                Assert.Equal(caso.Operator, binaryOperator);
            }

            Assert.Equal(new[] { "number", "ref", "neg", "add", "sub", "mul", "div", "call" }, BoundExpressionTokens.NodeTokens);

            foreach (var token in new[] { "Number", "REF", "Neg", "Add", "negate", "reference", "binary", "expression", string.Empty, "number " })
            {
                Assert.False(BoundExpressionTokens.TryParseNodeToken(token, out _, out _), "«" + token + "» no es un nodo");
            }

            Assert.False(BoundExpressionTokens.TryParseNodeToken(null, out _, out _));
            Assert.Throws<ArgumentNullException>(() => BoundExpressionTokens.NodeToken(null));
        }

        [Fact]
        public void LOS_OPERADORES_TIENEN_TOKENS_EXPLICITOS_EN_LOS_DOS_SENTIDOS()
        {
            foreach (var (op, token) in new[]
                     {
                         (BoundBinaryOperator.Add, "add"), (BoundBinaryOperator.Subtract, "sub"),
                         (BoundBinaryOperator.Multiply, "mul"), (BoundBinaryOperator.Divide, "div"),
                     })
            {
                Assert.Equal(token, BoundExpressionTokens.OperatorToken(op));
                Assert.True(BoundExpressionTokens.TryParseOperatorToken(token, out var parsed));
                Assert.Equal(op, parsed);
            }

            foreach (var token in new[] { "Add", "+", "-", "subtract", "MUL", "neg", string.Empty })
            {
                Assert.False(BoundExpressionTokens.TryParseOperatorToken(token, out _), "«" + token + "» no es un operador");
            }

            Assert.False(BoundExpressionTokens.TryParseOperatorToken(null, out _));
            Assert.Equal(new[] { 1, 2, 3, 4 }, Enum.GetValues<BoundBinaryOperator>().Select(value => (int)value));
            Assert.Throws<ArgumentOutOfRangeException>(() => BoundExpressionTokens.OperatorToken((BoundBinaryOperator)5));
        }

        /// <summary>
        /// La prueba de que las tablas son explícitas: cada token difiere del nombre de su miembro C# (mayúsculas,
        /// abreviatura o grafía), así que ninguno puede estar saliendo de <c>ToString()</c> o de <c>nameof</c>.
        /// </summary>
        [Fact]
        public void NINGUN_TOKEN_SALE_DEL_NOMBRE_DE_SU_MIEMBRO()
        {
            foreach (BoundBinaryOperator op in Enum.GetValues(typeof(BoundBinaryOperator)))
            {
                Assert.NotEqual(op.ToString(), BoundExpressionTokens.OperatorToken(op));
            }

            foreach (BoundExpressionKind kind in Enum.GetValues(typeof(BoundExpressionKind)))
            {
                Assert.DoesNotContain(kind.ToString(), BoundExpressionTokens.NodeTokens);
            }

            foreach (LengthUnit unit in Enum.GetValues(typeof(LengthUnit)))
            {
                Assert.NotEqual(unit.ToString(), LengthUnits.Authority.Token(unit));
            }

            foreach (FunctionId function in Enum.GetValues(typeof(FunctionId)))
            {
                Assert.NotEqual(function.ToString(), FunctionRegistry.Productive.Token(function));
            }

            Assert.NotEqual(SymbolNamespace.ProjectVariable.ToString(), SymbolNamespaces.Token(SymbolNamespace.ProjectVariable));
        }
    }
}
