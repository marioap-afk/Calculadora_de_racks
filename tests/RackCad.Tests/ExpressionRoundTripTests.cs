using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — P2.5, la prueba central: para todo <c>BoundExpression</c> canónico <c>b</c> sin referencias rotas y un
    /// mismo snapshot, <c>Canonicalize(Bind(Parse(Format(b)))) == b</c> (V6 P2.5; Amendment A1 §7; ADR-0040 D4, D8).
    ///
    /// <para>
    /// Sin condiciones: ni «si cabe», ni «nombres razonables», ni guarda de caracteres, ni bypass privado, ni parser
    /// alternativo. Se recorre la tubería pública: el formatter único, <c>ExpressionParser.Parse</c> y
    /// <c>ExpressionBinder.Bind</c>. Las reproducciones A–D de A1 §2.2 son casos obligatorios, con B probado con nombres
    /// de 4001 y de 1 000 001 caracteres, y un corpus generado cubre precedencia, asociatividad, negación, funciones,
    /// unidades, homónimos, llaves, <c>}}</c>, reservados, cualificadores y doubles extremos cerca de los límites. En cada
    /// caso se comprueba además la cota del formatter de A1 §4: <c>tokens(Format(b)) ≤ 6n - 3</c>.
    /// </para>
    /// </summary>
    public class ExpressionRoundTripTests
    {
        /// <summary><c>Canonicalize</c> es la identidad sobre un árbol enlazado; la clasificación de P2.8 también se conserva.</summary>
        private static void AssertRoundTrip(BoundExpression tree, SymbolTable table)
        {
            var text = ExpressionFormatter.Format(tree, table);

            var parsed = ExpressionParser.Parse(text);
            Assert.True(
                parsed.Succeeded,
                "P2.5: el texto canónico no parsea («" + Short(text) + "»): " + ExpressionSyntaxTestSupport.Describe(parsed));

            var bound = ExpressionBinder.Bind(parsed.Syntax, ExpressionContext.Create(table), SymbolScope.Rack);
            Assert.True(bound.Succeeded, "P2.5: el texto canónico no enlaza («" + Short(text) + "»): " + Describe(bound.Diagnostics));

            Assert.Equal(tree, bound.Expression);
            Assert.Equal(CanonicalShape.Classify(tree).Kind, CanonicalShape.Classify(bound.Expression).Kind);
            Assert.InRange(TokenCount(text), 1, 6 * tree.NodeCount - 3);
        }

        private static BoundExpression[] Repeat(BoundExpression node, int count) => Enumerable.Repeat(node, count).ToArray();

        // ================================================================ reproducciones A–D de A1 §2.2

        [Fact]
        public void CASO_A_256_NODOS_CON_HOMONIMOS_CUALIFICADOS()
        {
            var table = Table(Variable(1, "Holgura"), Variable(2, "Holgura"));
            var interior = Call(FunctionId.Min, Repeat(Ref(1), 16));
            var tree = Call(FunctionId.Min, Repeat(interior, 15));

            Assert.Equal(256, tree.NodeCount);
            Assert.Equal(3, tree.Depth);
            AssertRoundTrip(tree, table);
        }

        [Theory]
        [InlineData(4000)]
        [InlineData(1000000)]
        public void CASO_B_NOMBRES_DE_L_MAS_UNO_CARACTERES(int limite)
        {
            var seguro = Table(Variable(1, new string('A', limite + 1)));
            Assert.Equal(limite + 1, ExpressionFormatter.Format(Ref(1), seguro).Length);
            AssertRoundTrip(Ref(1), seguro);

            var llaves = Table(Variable(1, new string('}', limite / 2 + 1)));
            Assert.Equal(limite + 4, ExpressionFormatter.Format(Ref(1), llaves).Length);
            AssertRoundTrip(Ref(1), llaves);

            var homonimo = new string('A', limite - 36);
            var homonimos = Table(Variable(1, homonimo), Variable(2, homonimo));
            Assert.Equal(limite + 1, ExpressionFormatter.Format(Ref(1), homonimos).Length);
            AssertRoundTrip(Ref(1), homonimos);

            // Variante de A1 §2.1: NombreLargo + 1 es Expression en las dos superficies.
            AssertRoundTrip(Add(Ref(1), Num(1)), seguro);
        }

        [Fact]
        public void CASO_C_EPSILON_EN_UNA_SUMA_DE_PROFUNDIDAD_24_Y_EN_UN_MIN_DE_256_NODOS()
        {
            var suma = Num(double.Epsilon);
            for (var i = 1; i < 24; i++)
            {
                suma = Add(suma, Num(double.Epsilon));
            }

            Assert.Equal(47, suma.NodeCount);
            Assert.Equal(24, suma.Depth);
            AssertRoundTrip(suma, SymbolTable.Empty);

            var interior = Call(FunctionId.Min, Repeat(Num(double.Epsilon), 16));
            var minimo = Call(FunctionId.Min, Repeat(interior, 15));
            Assert.Equal(256, minimo.NodeCount);
            AssertRoundTrip(minimo, SymbolTable.Empty);
        }

        [Fact]
        public void CASO_D_256_NODOS_PROFUNDIDAD_24_Y_16_ARGUMENTOS()
        {
            var table = Table(Variable(1, "a}b"), Variable(2, "a}b"));

            var cadena = Ref(1);
            for (var i = 1; i < 23; i++)
            {
                cadena = Add(cadena, Ref(1));
            }

            var minimo = Call(FunctionId.Min, Repeat(Num(double.Epsilon, LengthUnit.Millimeter), 13));
            var tree = Call(FunctionId.Max, new[] { cadena }.Concat(Repeat(minimo, 15)).ToArray());

            Assert.Equal(256, tree.NodeCount);
            Assert.Equal(24, tree.Depth);
            Assert.Equal(16, Assert.IsType<BoundCall>(tree).Arguments.Count);
            AssertRoundTrip(tree, table);
        }

        // ================================================================ corpus generado

        private static readonly double[] NumberPool =
        {
            0, 1, 2, 10, 0.5, 0.1, 1.0 / 3, 99.99, 123456.789, 0.000123, 7E-07, 1E-05, 1E-300, 1E+15, 1E+16, 1E+20,
            4503599627370496, 2.2250738585072014E-308, double.Epsilon, double.MaxValue,
        };

        private static readonly LengthUnit?[] UnitPool = { null, null, LengthUnit.Millimeter, LengthUnit.Inch, LengthUnit.Foot };

        private static SymbolTable CorpusTable()
        {
            var names = new[]
            {
                "Holgura", "Holgura", "holgura", "Base", "Holgura General", "a}b", "}", "{", "MIN", "max", "Abs", "Rack",
                "project", "Holgura-Base", "a-b", "a", "b", "A*-B", "x+1", "Nivel-1", "Nivel", "1", "Alto (m)", "Nivel  2",
                " lead", "trail ", "v1.2", "2x", "Caf" + (char)0x00E9, "\U0001D49Cstral", string.Empty, "mm", "in", "ft", "e",
                "_x1", "Rack.Frentes", "#", "Holgura#1", new string('A', 300), new string('}', 150),
            };

            var entries = names
                .Select((name, index) => new SymbolEntry(
                    SymbolId.ProjectVariable(index % 3 == 0 ? Key(index + 1).ToUpperInvariant() : Key(index + 1)),
                    index % 7 == 6 ? SymbolScope.Rack : SymbolScope.Project,
                    name,
                    SymbolDefinition.FromLiteral(index)))
                .ToList();

            return SymbolTable.Create(entries);
        }

        private static BoundExpression Generate(Random random, IReadOnlyList<SymbolEntry> symbols, ref int budget, int depth)
        {
            budget--;

            if (depth <= 1 || budget <= 0 || random.Next(5) == 0)
            {
                if (random.Next(2) == 0)
                {
                    return Num(NumberPool[random.Next(NumberPool.Length)], UnitPool[random.Next(UnitPool.Length)]);
                }

                return BoundExpression.Reference(symbols[random.Next(symbols.Count)].Id);
            }

            var choice = random.Next(10);

            if (choice < 2)
            {
                return Neg(Generate(random, symbols, ref budget, depth - 1));
            }

            if (choice < 7)
            {
                var left = Generate(random, symbols, ref budget, depth - 1);
                var right = Generate(random, symbols, ref budget, depth - 1);
                return BoundExpression.Binary((BoundBinaryOperator)(1 + random.Next(4)), left, right);
            }

            if (choice < 9)
            {
                var count = 2 + random.Next(15);
                var arguments = new BoundExpression[count];
                for (var i = 0; i < count; i++)
                {
                    arguments[i] = Generate(random, symbols, ref budget, depth - 1);
                }

                return BoundExpression.Call(random.Next(2) == 0 ? FunctionId.Min : FunctionId.Max, arguments);
            }

            return Call(FunctionId.Abs, Generate(random, symbols, ref budget, depth - 1));
        }

        [Fact]
        public void P2_5_SE_CUMPLE_SOBRE_UN_CORPUS_GENERADO_CERCA_DE_LOS_LIMITES()
        {
            var table = CorpusTable();
            var random = new Random(20260913);
            var accepted = 0;
            var nearLimits = 0;

            for (var attempt = 0; attempt < 20000 && accepted < 1500; attempt++)
            {
                var budget = 1 + random.Next(256);
                var tree = Generate(random, table.Entries, ref budget, 1 + random.Next(24));

                if (tree.NodeCount > ExpressionLimits.MaxNodeCount || tree.Depth > ExpressionLimits.MaxBoundExpressionDepth)
                {
                    continue;
                }

                AssertRoundTrip(tree, table);
                accepted++;

                if (tree.NodeCount >= 200 || tree.Depth >= 20)
                {
                    nearLimits++;
                }
            }

            Assert.Equal(1500, accepted);
            Assert.True(nearLimits >= 50, "El corpus apenas se acercó a los límites: " + nearLimits);
        }

        [Fact]
        public void P2_5_CON_NOMBRES_LARGOS_Y_OPERADORES_DENTRO_DE_EXPRESIONES()
        {
            var y = new string('y', 2000);
            var z = new string('z', 2000);
            var table = Table(
                Variable(1, new string('x', 4001)),
                Variable(2, y + "-" + z),
                Variable(3, y),
                Variable(4, z),
                Variable(5, new string('w', 1000001)));

            AssertRoundTrip(Call(FunctionId.Max, Add(Ref(1), Num(1)), Sub(Ref(3), Ref(4)), Ref(2)), table);
            AssertRoundTrip(Mul(Neg(Ref(5)), Ref(2)), table);
        }
    }
}
