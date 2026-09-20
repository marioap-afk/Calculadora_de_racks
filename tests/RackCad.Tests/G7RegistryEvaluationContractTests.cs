using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using static RackCad.Tests.G7ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>I-49 G7-A RED — A3-R2 multi-cause registry evaluation and structural root contracts.</summary>
    public class G7RegistryEvaluationContractTests
    {
        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "01")]
        [Trait("T-A3", "03")]
        [Trait("T-A3", "05")]
        [Trait("T-A3", "06")]
        public void LOCAL_FAILURES_COEXIST_IN_CATALOGUE_AND_P11_ORDER_WITH_LATENT_NUMERIC_FAILURES()
        {
            var u1 = Evaluate(
                Expression(1, Add(Ref(101), Num(1))),
                Expression(2, Div(Num(1), Num(0))),
                Expression(4, Add(Ref(2), Ref(1))));

            AssertDiagnostics(u1, 4,
                Df(1, new[] { 1 }, RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101)))),
                Df(2, new[] { 2 }, RootKey(2, ExpressionDiagnosticCode.DivisionByZero)));
            Assert.Equal(new[]
            {
                RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101))),
                RootKey(2, ExpressionDiagnosticCode.DivisionByZero),
            }, u1.Result(4).RootCauses);

            var u3 = Evaluate(
                Expression(1, Div(Num(1), Num(0))),
                Expression(4, Add(Add(Ref(1), Ref(103)), Div(Num(1), Num(0)))));

            AssertDiagnostics(u3, 4,
                Broken(103),
                Df(1, new[] { 1 }, RootKey(1, ExpressionDiagnosticCode.DivisionByZero)));
            Assert.Equal(new[]
            {
                RootKey(4, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(103))),
                RootKey(1, ExpressionDiagnosticCode.DivisionByZero),
            }, u3.Result(4).RootCauses);
            Assert.DoesNotContain(u3.Result(4).Diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.DivisionByZero);

            var brokenOrder = Evaluate(Expression(5, Add(Ref(105), Ref(104))));
            AssertDiagnostics(brokenOrder, 5, Broken(104), Broken(105));
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "02")]
        [Trait("T-A3", "09")]
        [Trait("T-A3", "11")]
        public void REPRESENTATIVE_CHAIN_STAYS_SINGLE_WHILE_ROOTCAUSES_RETAIN_ALL_REACHABLE_ROOTS()
        {
            var evaluation = Evaluate(
                Expression(1, Ref(101)),
                Expression(2, Ref(102)),
                Expression(3, Add(Ref(2), Ref(1))),
                Expression(4, Mul(Ref(3), Num(2))),
                Expression(5, Add(Ref(3), Ref(4))));

            AssertDiagnostics(evaluation, 3,
                Df(1, new[] { 1 }, RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101)))),
                Df(2, new[] { 2 }, RootKey(2, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(102)))));
            AssertDiagnostics(evaluation, 4,
                Df(3, new[] { 3, 1 }, RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101)))));
            Assert.Equal(new[]
            {
                RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101))),
                RootKey(2, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(102))),
            }, evaluation.Result(4).RootCauses);
            Assert.Equal(evaluation.Result(4).RootCauses, evaluation.Result(5).RootCauses);
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "04")]
        [Trait("T-A3", "10")]
        [Trait("T-A3", "30")]
        public void U4_AND_CYCLE_ENTRY_KEEP_LOCAL_DIAGNOSTICS_AND_SCC_WIDE_ROOTS()
        {
            var u4 = Evaluate(
                Expression(1, Add(Ref(2), Ref(5))),
                Expression(2, Ref(1)),
                Expression(5, Div(Num(1), Num(0))),
                Expression(6, Add(Ref(2), Ref(1))));
            var u4Roots = new[] { CycleKey(1, 2), RootKey(5, ExpressionDiagnosticCode.DivisionByZero) };

            AssertDiagnostics(u4, 1,
                Cycle(1, 2),
                Df(5, new[] { 5 }, RootKey(5, ExpressionDiagnosticCode.DivisionByZero)));
            AssertDiagnostics(u4, 2, Cycle(1, 2));
            AssertDiagnostics(u4, 6,
                Df(1, new[] { 1 }, CycleKey(1, 2)),
                Df(2, new[] { 2 }, CycleKey(1, 2)));
            Assert.Equal(u4Roots, u4.Result(1).RootCauses);
            Assert.Equal(u4Roots, u4.Result(2).RootCauses);
            Assert.Equal(u4Roots, u4.Result(6).RootCauses);

            var cycleWithOwnBroken = Evaluate(
                Expression(1, Add(Ref(2), Ref(101))),
                Expression(2, Add(Ref(1), Ref(5))),
                Expression(5, Div(Num(1), Num(0))),
                Expression(7, Add(Ref(1), Num(1))));
            AssertDiagnostics(cycleWithOwnBroken, 7,
                Df(1, new[] { 1 }, RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101)))));
            Assert.Equal(new[]
            {
                RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101))),
                CycleKey(1, 2),
                RootKey(5, ExpressionDiagnosticCode.DivisionByZero),
            }, cycleWithOwnBroken.Result(7).RootCauses);
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "07")]
        public void ALL_STATIC_AND_GRAPH_DIAGNOSTICS_USE_THE_FROZEN_CATALOGUE_ORDER()
        {
            var evaluation = Evaluate(
                Expression(1, Add(Add(Add(Ref(1), Ref(101)), Ref(5)), Call(FunctionId.Abs, Num(1), Num(2)))),
                Expression(5, Div(Num(1), Num(0))));

            Assert.Equal(new[]
            {
                ExpressionDiagnosticCode.BrokenReference,
                ExpressionDiagnosticCode.Cycle,
                ExpressionDiagnosticCode.DependencyFailed,
                ExpressionDiagnosticCode.InvalidArguments,
            }, evaluation.Result(1).Diagnostics.Select(diagnostic => diagnostic.Code));
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "22")]
        [Trait("T-A3", "23")]
        [Trait("T-A3", "31")]
        public void U5_PROPAGATES_EXTERNAL_ROOTS_ACROSS_THE_SCC_WITHOUT_PROPAGATING_LOCAL_DIAGNOSTICS()
        {
            var evaluation = Evaluate(
                Expression(1, Add(Ref(2), Ref(5))),
                Expression(2, Ref(3)),
                Expression(3, Add(Ref(4), Ref(6))),
                Expression(4, Ref(1)),
                Expression(5, Div(Num(1), Num(0))),
                Expression(6, Div(Num(1), Num(0))));
            var roots = new[]
            {
                CycleKey(1, 2, 3, 4),
                RootKey(5, ExpressionDiagnosticCode.DivisionByZero),
                RootKey(6, ExpressionDiagnosticCode.DivisionByZero),
            };

            foreach (var member in new[] { 1, 2, 3, 4 })
            {
                Assert.Equal(roots, evaluation.Result(member).RootCauses);
            }

            AssertDiagnostics(evaluation, 1,
                Cycle(1, 2, 3, 4),
                Df(5, new[] { 5 }, RootKey(5, ExpressionDiagnosticCode.DivisionByZero)));
            AssertDiagnostics(evaluation, 2, Cycle(1, 2, 3, 4));
            AssertDiagnostics(evaluation, 3,
                Cycle(1, 2, 3, 4),
                Df(6, new[] { 6 }, RootKey(6, ExpressionDiagnosticCode.DivisionByZero)));
            AssertDiagnostics(evaluation, 4, Cycle(1, 2, 3, 4));
            Assert.True(evaluation.IsSimpleCycle(1, 2, 3, 4));
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "24")]
        [Trait("T-A3", "25")]
        [Trait("T-A3", "27")]
        [Trait("T-A3", "42")]
        public void STATIC_ARITY_COEXISTS_WITH_KNOWN_FAILURES_AND_BLOCKS_NUMERIC_EVALUATION()
        {
            var evaluation = Evaluate(
                Expression(1, Ref(101)),
                Expression(2, Call(FunctionId.Abs, Ref(1), Num(2))),
                Expression(3, Add(Call(FunctionId.Abs, Ref(103), Num(2)), Div(Num(1), Num(0)))),
                Expression(4, Add(Ref(2), Num(1))),
                Expression(9, Add(Num(2), Num(3))));

            AssertDiagnostics(evaluation, 2,
                Df(1, new[] { 1 }, RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101)))),
                Invalid("ABS", 2));
            Assert.Equal(new[]
            {
                RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101))),
                RootKey(2, ExpressionDiagnosticCode.InvalidArguments, "ABS/2"),
            }, evaluation.Result(2).RootCauses);

            AssertDiagnostics(evaluation, 3, Broken(103), Invalid("ABS", 2));
            Assert.DoesNotContain(evaluation.Result(3).Diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.DivisionByZero);
            AssertDiagnostics(evaluation, 4,
                Df(2, new[] { 2, 1 }, RootKey(1, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101)))));
            Assert.Equal(evaluation.Result(2).RootCauses, evaluation.Result(4).RootCauses);
            Assert.True(evaluation.Result(9).Succeeded);
            Assert.Equal(5, evaluation.Result(9).Value);
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "26")]
        public void INVALID_ARGUMENT_PAIRS_ARE_DISTINCT_SORTED_DEDUPED_AND_HAVE_ONE_AGGREGATED_ROOT()
        {
            var evaluation = Evaluate(Expression(4,
                Add(
                    Add(Call(FunctionId.Min, Num(1)), Call(FunctionId.Abs, Num(1), Num(2))),
                    Add(Call(FunctionId.Max, Num(3)), Add(Call(FunctionId.Abs, Num(4), Num(5)), Call(FunctionId.Abs))))));

            AssertDiagnostics(evaluation, 4,
                Invalid("ABS", 0),
                Invalid("ABS", 2),
                Invalid("MAX", 1),
                Invalid("MIN", 1));
            Assert.Equal(new[]
            {
                RootKey(4, ExpressionDiagnosticCode.InvalidArguments, "ABS/0+ABS/2+MAX/1+MIN/1"),
            }, evaluation.Result(4).RootCauses);
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "28")]
        [Trait("T-A3", "29")]
        [Trait("T-A3", "62")]
        public void ROOT_SIGNATURES_AGGREGATE_OWNER_DATA_AND_SORT_BY_CODE_BEFORE_OWNER()
        {
            var evaluation = Evaluate(
                Expression(1, Div(Num(1), Num(0))),
                Expression(2, Add(Ref(102), Ref(101))),
                Expression(4, Add(Ref(1), Ref(2))),
                Expression(5, Add(Ref(2), Num(1))));
            var brokenRoot = RootKey(2, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(101)) + "+" + IdKey(Id(102)));

            AssertDiagnostics(evaluation, 2, Broken(101), Broken(102));
            Assert.Equal(new[] { brokenRoot }, evaluation.Result(2).RootCauses);
            Assert.Equal(new[] { brokenRoot, RootKey(1, ExpressionDiagnosticCode.DivisionByZero) }, evaluation.Result(4).RootCauses);
            AssertDiagnostics(evaluation, 5, Df(2, new[] { 2 }, brokenRoot));
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "33")]
        public void ENTERING_THE_SAME_CYCLE_THROUGH_ANOTHER_MEMBER_CHANGES_DIAGNOSTICS_BUT_NOT_ROOTS()
        {
            var throughA = Evaluate(Expression(1, Ref(2)), Expression(2, Ref(1)), Expression(8, Add(Ref(1), Num(1))));
            var throughB = Evaluate(Expression(1, Ref(2)), Expression(2, Ref(1)), Expression(8, Add(Ref(2), Num(1))));

            AssertDiagnostics(throughA, 8, Df(1, new[] { 1 }, CycleKey(1, 2)));
            AssertDiagnostics(throughB, 8, Df(2, new[] { 2 }, CycleKey(1, 2)));
            Assert.Equal(throughA.Result(8).RootCauses, throughB.Result(8).RootCauses);
            Assert.Equal(new[] { CycleKey(1, 2) }, throughA.Result(8).RootCauses);
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "34")]
        public void SYNTHETIC_NON_CANONICAL_EXPRESSION_IS_AN_INTRINSIC_ROOT_AND_IS_NOT_EVALUATED()
        {
            var evaluation = Evaluate(Expression(1, Num(6)), Expression(2, Add(Ref(1), Num(1))));

            AssertDiagnostics(evaluation, 1, Own(ExpressionDiagnosticCode.NonCanonicalForm));
            Assert.False(evaluation.Result(1).Succeeded);
            Assert.Equal(new[] { RootKey(1, ExpressionDiagnosticCode.NonCanonicalForm) }, evaluation.Result(1).RootCauses);
            AssertDiagnostics(evaluation, 2,
                Df(1, new[] { 1 }, RootKey(1, ExpressionDiagnosticCode.NonCanonicalForm)));
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "41")]
        [Trait("T-A3", "61")]
        public void EVERY_REPRESENTATIVE_DEPENDENCY_ROOT_BELONGS_TO_ITS_OWNER_ROOTCAUSES()
        {
            var evaluation = Evaluate(
                Expression(1, Div(Num(1), Num(0))),
                Expression(2, Add(Ref(102), Ref(1))),
                Expression(3, Add(Ref(2), Num(1))),
                Expression(4, Add(Ref(3), Ref(2))));

            AssertDiagnostics(evaluation, 2,
                Broken(102),
                Df(1, new[] { 1 }, RootKey(1, ExpressionDiagnosticCode.DivisionByZero)));
            AssertDiagnostics(evaluation, 3,
                Df(2, new[] { 2 }, RootKey(2, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(102)))));
            Assert.Equal(new[]
            {
                RootKey(2, ExpressionDiagnosticCode.BrokenReference, IdKey(Id(102))),
                RootKey(1, ExpressionDiagnosticCode.DivisionByZero),
            }, evaluation.Result(3).RootCauses);

            foreach (var owner in new[] { 2, 3, 4 })
            {
                var result = evaluation.Result(owner);
                foreach (var diagnostic in result.Diagnostics.Where(diagnostic => diagnostic.Code == ExpressionDiagnosticCode.DependencyFailed))
                {
                    Assert.Contains(diagnostic.Root, result.RootCauses);
                }
            }
        }

        [Fact]
        [Trait("Gate", "G7")]
        [Trait("T-A3", "43")]
        public void LINKED_SCCS_CONTRIBUTE_EACH_CYCLE_ROOT_EXACTLY_ONCE()
        {
            var evaluation = Evaluate(
                Expression(1, Ref(2)),
                Expression(2, Ref(1)),
                Expression(3, Add(Ref(4), Ref(1))),
                Expression(4, Ref(3)));

            Assert.Equal(new[] { CycleKey(1, 2), CycleKey(3, 4) }, evaluation.Result(3).RootCauses);
            Assert.Equal(new[] { CycleKey(1, 2), CycleKey(3, 4) }, evaluation.Result(4).RootCauses);
        }

        private static ExpectedDiagnostic Broken(int missing)
            => new ExpectedDiagnostic(ExpressionDiagnosticCode.BrokenReference, null, Array.Empty<int>(), null, new[] { missing }, null, null);

        private static ExpectedDiagnostic Cycle(params int[] members)
            => new ExpectedDiagnostic(ExpressionDiagnosticCode.Cycle, null, Array.Empty<int>(), null, members, null, null);

        private static ExpectedDiagnostic Df(int cause, int[] chain, string root)
            => new ExpectedDiagnostic(ExpressionDiagnosticCode.DependencyFailed, cause, chain, root, Array.Empty<int>(), null, null);

        private static ExpectedDiagnostic Invalid(string token, int count)
            => new ExpectedDiagnostic(ExpressionDiagnosticCode.InvalidArguments, null, Array.Empty<int>(), null, Array.Empty<int>(), token, count);

        private static ExpectedDiagnostic Own(ExpressionDiagnosticCode code)
            => new ExpectedDiagnostic(code, null, Array.Empty<int>(), null, Array.Empty<int>(), null, null);

        private static void AssertDiagnostics(G7EvaluationView evaluation, int owner, params ExpectedDiagnostic[] expected)
        {
            var actual = evaluation.Result(owner).Diagnostics;
            Assert.Equal(expected.Length, actual.Count);
            for (var index = 0; index < expected.Length; index++)
            {
                Assert.Equal(expected[index].Code, actual[index].Code);
                Assert.Equal(expected[index].Cause.HasValue ? Id(expected[index].Cause.Value) : null, actual[index].Cause);
                Assert.Equal(expected[index].Chain.Select(Id), actual[index].Chain);
                Assert.Equal(expected[index].Root, actual[index].Root);
                Assert.Equal(expected[index].Related.Select(Id), actual[index].RelatedSymbols);
                Assert.Equal(expected[index].Token, actual[index].FunctionToken);
                Assert.Equal(expected[index].Count, actual[index].ArgumentCount);
            }
        }

        private sealed class ExpectedDiagnostic
        {
            internal ExpectedDiagnostic(
                ExpressionDiagnosticCode code,
                int? cause,
                IReadOnlyList<int> chain,
                string root,
                IReadOnlyList<int> related,
                string token,
                int? count)
            {
                Code = code;
                Cause = cause;
                Chain = chain;
                Root = root;
                Related = related;
                Token = token;
                Count = count;
            }

            internal ExpressionDiagnosticCode Code { get; }
            internal int? Cause { get; }
            internal IReadOnlyList<int> Chain { get; }
            internal string Root { get; }
            internal IReadOnlyList<int> Related { get; }
            internal string Token { get; }
            internal int? Count { get; }
        }
    }
}
