using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.ExpressionSyntaxTestSupport;

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

        // ================================================================ identidades textuales de Amendment A2

        /// <summary>
        /// R1 de A2 §2.3 y §4: A con clave N y B con otra clave D, los dos «Holgura». A se escribe <c>Holgura#{n}</c> y B
        /// <c>Holgura#d</c>; cada texto enlaza a SU id —sin primera coincidencia— y los dos hacen round-trip.
        /// </summary>
        [Fact]
        public void R1_UNA_CLAVE_N_Y_UN_HOMONIMO_CON_OTRA_CLAVE_D_SE_DISTINGUEN_Y_HACEN_ROUND_TRIP()
        {
            const string otraD = "0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d";
            var a = VariableKey(ClaveN, "Holgura", 1);
            var b = VariableKey(otraD, "Holgura", 2);
            var table = Table(a, b);
            var ctx = ExpressionContext.Create(table);

            Assert.Equal("Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}", ExpressionFormatter.Format(BoundExpression.Reference(a.Id), table));
            Assert.Equal("Holgura#0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d", ExpressionFormatter.Format(BoundExpression.Reference(b.Id), table));

            Assert.Equal(BoundExpression.Reference(a.Id), BindOk("Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}", ctx));
            Assert.Equal(BoundExpression.Reference(b.Id), BindOk("Holgura#0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d", ctx));
            Assert.Equal(ExpressionDiagnosticCode.AmbiguousName, Assert.Single(BindFails("Holgura", ctx)).Code);

            AssertRoundTrip(BoundExpression.Reference(a.Id), table);
            AssertRoundTrip(BoundExpression.Reference(b.Id), table);
            AssertRoundTrip(Sub(BoundExpression.Reference(a.Id), BoundExpression.Reference(b.Id)), table);
        }

        /// <summary>
        /// R2 de A2 §2.4 y §4: el MISMO valor <c>System.Guid</c> en clave D y en clave N, los dos «Holgura», son DOS
        /// identidades en la tabla —el núcleo no da <c>AmbiguousIdentity</c> porque los valores coincidan—; se escriben
        /// <c>Holgura#d</c> y <c>Holgura#{n}</c> y cada una vuelve a su propio <c>SymbolId</c>.
        /// </summary>
        [Fact]
        public void R2_LA_MISMA_GUID_EN_CLAVE_D_Y_EN_CLAVE_N_SON_DOS_IDENTIDADES_CON_SU_PROPIO_TEXTO()
        {
            var a = VariableKey(ClaveD, "Holgura", 1);
            var b = VariableKey(ClaveN, "Holgura", 2);
            var table = Table(a, b);
            var ctx = ExpressionContext.Create(table);

            Assert.Equal(Guid.Parse(ClaveD), Guid.Parse(ClaveN));
            Assert.Equal(2, table.Entries.Count);
            Assert.NotEqual(a.Id, b.Id);

            Assert.Equal("Holgura#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b", ExpressionFormatter.Format(BoundExpression.Reference(a.Id), table));
            Assert.Equal("Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}", ExpressionFormatter.Format(BoundExpression.Reference(b.Id), table));

            Assert.Equal(a.Id, Assert.IsType<BoundReference>(BindOk("Holgura#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b", ctx)).Symbol);
            Assert.Equal(b.Id, Assert.IsType<BoundReference>(BindOk("Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}", ctx)).Symbol);

            AssertRoundTrip(BoundExpression.Reference(a.Id), table);
            AssertRoundTrip(BoundExpression.Reference(b.Id), table);
            AssertRoundTrip(Mul(BoundExpression.Reference(a.Id), Add(BoundExpression.Reference(b.Id), Num(1))), table);
        }

        /// <summary>
        /// A2 §4 y pruebas E y G de A2 §9.3: la familia de seis grafías del mismo GUID, B, P, las disposiciones de
        /// compatibilidad y X con espacio en blanco Unicode, todas con el mismo nombre: cualificadores distintos —los de la
        /// familia, literales de A2 §4— y round-trip de cada una sola y de todas en un mismo árbol.
        /// </summary>
        [Fact]
        public void LAS_GRAFIAS_DEL_MISMO_GUID_TIENEN_CUALIFICADORES_DISTINTOS_Y_HACEN_ROUND_TRIP()
        {
            var entries = ClavesDistintasA2.Select((key, index) => VariableKey(key, "Holgura", index)).ToArray();
            var table = Table(entries);

            var texts = entries.Select(entry => ExpressionFormatter.Format(BoundExpression.Reference(entry.Id), table)).ToList();
            Assert.Equal(entries.Length, texts.Distinct(StringComparer.Ordinal).Count());

            Assert.Equal(
                new[]
                {
                    "Holgura#3f2b1c9e-8a4d-4e6f-9b0a-1c2d3e4f5a6b",
                    "Holgura#{3f2b1c9e8a4d4e6f9b0a1c2d3e4f5a6b}",
                    "Holgura#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}",
                    "Holgura#{{0x3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0xa,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}",
                    "Holgura#{{0x00003f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}}}}",
                    "Holgura#{{0x3f2b1c9e, 0x8a4d, 0x4e6f, {0x9b, 0x0a, 0x1c, 0x2d, 0x3e, 0x4f, 0x5a, 0x6b}}}}}",
                },
                FamiliaA2.Select(key => ExpressionFormatter.Format(BoundExpression.Reference(SymbolId.ProjectVariable(key)), table)));

            foreach (var entry in entries)
            {
                AssertRoundTrip(BoundExpression.Reference(entry.Id), table);
                AssertRoundTrip(Neg(BoundExpression.Reference(entry.Id)), table);
            }

            AssertRoundTrip(Call(FunctionId.Max, entries.Select(entry => BoundExpression.Reference(entry.Id)).ToArray()), table);
        }

        /// <summary>
        /// La cota de A1 §4 con cualificadores de clave exacta (prueba M de A2 §9.3): 256 nodos con homónimos cuyas claves
        /// son N y X con mil ceros; cada cualificador es UN token, así que <c>tokens(Format(b)) ≤ 6n - 3</c> se sigue
        /// cumpliendo y el texto hace round-trip.
        /// </summary>
        [Fact]
        public void CASO_A_256_NODOS_CON_HOMONIMOS_DE_CLAVE_EXACTA()
        {
            var larga = "{0x" + new string('0', 1000) + "3f2b1c9e,0x8a4d,0x4e6f,{0x9b,0x0a,0x1c,0x2d,0x3e,0x4f,0x5a,0x6b}}";
            var n = VariableKey(ClaveN, "Holgura", 1);
            var x = VariableKey(larga, "Holgura", 2);
            var table = Table(n, x);

            var interior = Call(FunctionId.Min, Enumerable.Range(0, 16).Select(i => BoundExpression.Reference(i % 2 == 0 ? n.Id : x.Id)).ToArray());
            var tree = Call(FunctionId.Min, Repeat(interior, 15));

            Assert.Equal(256, tree.NodeCount);
            AssertRoundTrip(tree, table);
        }

        /// <summary>
        /// P2.5 sobre un corpus GENERADO con claves de todas las disposiciones (prueba L de A2 §9.3 y el corpus obligatorio
        /// de A2 §5): D, N, B, P y X de dos GUID, las de compatibilidad, el mismo GUID en varias identidades textuales,
        /// homónimos entre ellas, grafías en mayúsculas y en minúsculas, nombres largos, llaves, reservados y un ámbito Rack;
        /// 1500 árboles, con los límites de nodos, profundidad y argumentos cerca. El corpus de siempre sigue aparte, sin
        /// cambios.
        /// </summary>
        [Fact]
        public void P2_5_SE_CUMPLE_SOBRE_UN_CORPUS_GENERADO_CON_CLAVES_DE_TODAS_LAS_DISPOSICIONES()
        {
            var keys = new[]
            {
                ClaveDMayusculas, ClaveN, ClaveB.ToUpperInvariant(), ClaveP, ClaveXMayusculas, ClaveXGrupoCorto, ClaveXConCeros,
                ClaveXConEspacios, ClaveXConEspacioDuro, ClaveXConSeparadorDeLinea, ClaveDCompatSigno, ClaveDCompatHex,
                ClaveBCompatHex, ClavePCompatSigno, Key(1), Key(2).ToUpperInvariant(), "8C1D7E204B5A4C6D9E7F102132435465",
                "{8c1d7e20-4b5a-4c6d-9e7f-102132435465}", "(8C1D7E20-4B5A-4C6D-9E7F-102132435465)",
                "{0x8c1d7e20,0x4b5a,0x4c6d,{0x9e,0x7f,0x10,0x21,0x32,0x43,0x54,0x65}}", Guid2,
            };

            var names = new[]
            {
                "Holgura", "holgura", "HOLGURA", "Base", "a}b", "}", "{", "MIN", "Rack", "Holgura-Base", "Alto (m)", " lead",
                "Holgura General", new string('A', 300), new string('}', 150), string.Empty,
            };

            var table = SymbolTable.Create(keys.Select((key, index) => new SymbolEntry(
                SymbolId.ProjectVariable(key),
                index % 7 == 6 ? SymbolScope.Rack : SymbolScope.Project,
                names[index % names.Length],
                SymbolDefinition.FromLiteral(index))));

            Assert.Equal(keys.Length, table.Entries.Count);

            var random = new Random(20260914);
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
