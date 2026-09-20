using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.G7ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>I-49 G7-A RED — P11, P12, P13 and deterministic graph-derived sets.</summary>
    public class G7DependencyGraphContractTests
    {
        [Fact]
        [Trait("Gate", "G7")]
        public void DEPENDENCY_EXTRACTION_IS_PURE_DISTINCT_AND_IN_P11_ORDER()
        {
            Assert.Empty(Dependencies(Num(7)));
            AssertIds(new[] { 2 }, Dependencies(Ref(2)));

            var nested = Add(Ref(3), Call(FunctionId.Max, Ref(1), Mul(Ref(3), Ref(2))));
            AssertIds(new[] { 1, 2, 3 }, Dependencies(nested));

            // Same display name would not merge identities: extraction reads only bound ids and performs no evaluation.
            AssertIds(new[] { 1, 2 }, Dependencies(Add(Ref(2), Ref(1))));
        }

        [Fact]
        [Trait("Gate", "G7")]
        public void GRAPH_DIRECTION_AND_INVERSE_INDEX_ARE_OWNER_TO_DIRECT_DEPENDENCY()
        {
            var evaluation = Evaluate(
                Expression(1, Add(Ref(2), Ref(3))),
                new SymbolEntry(Id(2), SymbolScope.Project, "same-display-name", SymbolDefinition.FromExpression(Ref(4))),
                new SymbolEntry(Id(3), SymbolScope.Project, "same-display-name", SymbolDefinition.FromLiteral(3)),
                Literal(4, 4));

            AssertIds(new[] { 2, 3 }, evaluation.DirectDependencies(1));
            AssertIds(new[] { 1 }, evaluation.DirectDependents(2));
            AssertIds(new[] { 1 }, evaluation.DirectDependents(3));
            AssertIds(new[] { 2, 3, 4 }, evaluation.TransitiveDependencies(1));
            AssertIds(new[] { 1, 2 }, evaluation.TransitiveDependents(4));
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "15")]
        public void MISSING_IDS_ARE_DEPENDENCY_INFORMATION_BUT_NEVER_GRAPH_NODES()
        {
            AssertIds(new[] { 99 }, Dependencies(Ref(99)));

            var evaluation = Evaluate(Expression(1, Ref(99)), Literal(2));

            AssertIds(new[] { 99 }, evaluation.DirectDependencies(1));
            Assert.Empty(evaluation.TransitiveDependencies(1));
            Assert.DoesNotContain(evaluation.Cycles.SelectMany(cycle => cycle), id => id == Id(99));
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "08")]
        [Trait("T-A3", "31")]
        [Trait("T-A3", "32")]
        [Trait("T-A3", "63")]
        public void SCCS_COVER_SELF_LOOPS_SIMPLE_AND_NON_SIMPLE_CYCLES_WITHOUT_DUPLICATE_EDGES()
        {
            var self = Evaluate(Expression(1, Ref(1)), Literal(9));
            AssertCycles(self, new[] { new[] { 1 } });
            Assert.True(self.IsSimpleCycle(1));

            var two = Evaluate(Expression(1, Add(Ref(2), Ref(2))), Expression(2, Ref(1)), Literal(9));
            AssertCycles(two, new[] { new[] { 1, 2 } });
            AssertIds(new[] { 2 }, two.DirectDependencies(1));
            Assert.True(two.IsSimpleCycle(1, 2));
            Assert.Equal(new[] { ExpressionDiagnosticCode.Cycle }, two.Result(1).Diagnostics.Select(diagnostic => diagnostic.Code));
            Assert.Equal(new[] { ExpressionDiagnosticCode.Cycle }, two.Result(2).Diagnostics.Select(diagnostic => diagnostic.Code));
            AssertIds(new[] { 1, 2 }, two.TransitiveDependencies(1));
            AssertIds(new[] { 1, 2 }, two.TransitiveDependents(1));
            AssertIds(new[] { 1, 2 }, two.TransitiveDependencies(2));
            AssertIds(new[] { 1, 2 }, two.TransitiveDependents(2));

            var three = Evaluate(Expression(1, Ref(2)), Expression(2, Ref(3)), Expression(3, Ref(1)));
            AssertCycles(three, new[] { new[] { 1, 2, 3 } });
            Assert.True(three.IsSimpleCycle(1, 2, 3));
            Assert.All(three.Cycles.Single(), member =>
                Assert.Equal(new[] { ExpressionDiagnosticCode.Cycle }, three.Result(ParseTestId(member)).Diagnostics.Select(diagnostic => diagnostic.Code)));

            var chord = Evaluate(Expression(1, Add(Ref(1), Ref(2))), Expression(2, Ref(1)));
            Assert.False(chord.IsSimpleCycle(1, 2));
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "10")]
        [Trait("T-A3", "15")]
        public void DISCONNECTED_CYCLES_ARE_ORDERED_BY_MINIMUM_MEMBER_AND_DEDUPED_ON_ENTRY()
        {
            var evaluation = Evaluate(
                Expression(7, Ref(8)),
                Expression(8, Ref(7)),
                Expression(1, Ref(2)),
                Expression(2, Ref(1)),
                Expression(9, Add(Ref(1), Ref(2))),
                Literal(5));

            AssertCycles(evaluation, new[] { new[] { 1, 2 }, new[] { 7, 8 } });
            Assert.Equal(new[] { CycleKey(1, 2) }, evaluation.Result(9).RootCauses);
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "16")]
        public void TOPOLOGICAL_ORDER_USES_SMALLEST_READY_SYMBOL_AND_IGNORES_INSERTION_ORDER()
        {
            var entries = new[]
            {
                Expression(5, Ref(3)),
                Expression(4, Ref(2)),
                Literal(3, 3),
                Literal(2, 2),
                Literal(1, 1),
            };

            var forward = Evaluate(entries);
            var reverse = Evaluate(entries.Reverse().ToArray());

            AssertIds(new[] { 1, 2, 3, 4, 5 }, forward.EvaluationOrder);
            Assert.Equal(forward.EvaluationOrder, reverse.EvaluationOrder);

            var withCycle = Evaluate(
                Expression(1, Ref(2)),
                Expression(2, Ref(1)),
                Expression(3, Ref(1)),
                Literal(4));
            AssertIds(new[] { 3, 4 }, withCycle.EvaluationOrder);
            Assert.False(withCycle.Result(3).Succeeded);
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "12")]
        [Trait("T-A3", "13")]
        public void GRAPH_AND_RESULTS_ARE_DETERMINISTIC_ACROSS_INPUT_ORDER_REPETITION_AND_CULTURE()
        {
            var entries = new[]
            {
                Expression(1, Add(Ref(2), Ref(4))),
                Expression(2, Ref(3)),
                Expression(3, Ref(2)),
                Expression(4, Div(Num(1), Num(0))),
                Expression(5, Add(Ref(1), Ref(2))),
            };

            var expected = Evaluate(entries).StableSnapshot(1, 2, 3, 4, 5);
            Assert.Equal(expected, Evaluate(entries.Reverse().ToArray()).StableSnapshot(1, 2, 3, 4, 5));
            Assert.Equal(expected, Evaluate(entries).StableSnapshot(1, 2, 3, 4, 5));

            using (WithCulture("tr-TR"))
            {
                Assert.Equal(expected, Evaluate(entries.Reverse().ToArray()).StableSnapshot(1, 2, 3, 4, 5));
            }
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "14")]
        public void DEEP_CHAIN_AND_STACKED_DIAMONDS_REQUIRE_LINEAR_DERIVATION_WITHOUT_PATH_ENUMERATION()
        {
            const int chainLength = 2048;
            var chain = new List<SymbolEntry> { Expression(1, Ref(9000)) };
            for (var id = 2; id <= chainLength; id++)
            {
                chain.Add(Expression(id, Ref(id - 1)));
            }

            var deep = Evaluate(chain.AsEnumerable().Reverse().ToArray());
            Assert.Single(deep.Result(chainLength).RootCauses);
            Assert.Equal(RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(9000))), deep.Result(chainLength).RootCauses[0]);

            var diamonds = new List<SymbolEntry> { Expression(1, Ref(9001)) };
            var prior = 1;
            var next = 2;
            for (var layer = 0; layer < 24; layer++)
            {
                var left = next++;
                var right = next++;
                var join = next++;
                diamonds.Add(Expression(left, Ref(prior)));
                diamonds.Add(Expression(right, Ref(prior)));
                diamonds.Add(Expression(join, Add(Ref(left), Ref(right))));
                prior = join;
            }

            var fan = Evaluate(diamonds.AsEnumerable().Reverse().ToArray());
            Assert.Single(fan.Result(prior).RootCauses);
            Assert.Equal(RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(9001))), fan.Result(prior).RootCauses[0]);
        }

        private static void AssertIds(IEnumerable<int> expected, IEnumerable<SymbolId> actual)
            => Assert.Equal(expected.Select(Id), actual);

        private static void AssertCycles(G7EvaluationView evaluation, IEnumerable<int[]> expected)
            => Assert.Equal(
                expected.Select(cycle => cycle.Select(Id).ToArray()).ToArray(),
                evaluation.Cycles.Select(cycle => cycle.ToArray()).ToArray());

        private static int ParseTestId(SymbolId id)
            => int.Parse(id.Key.Substring(id.Key.Length - 12), System.Globalization.NumberStyles.HexNumber);
    }
}
