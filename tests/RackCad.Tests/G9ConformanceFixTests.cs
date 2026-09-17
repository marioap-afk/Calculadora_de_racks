using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;
using static RackCad.Tests.G9ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>Focused regressions for the G9-B1 conformance corrections.</summary>
    public sealed class G9ConformanceFixTests
    {
        private const string Missing2 = "bbbbbbbb-cccc-dddd-eeee-ffffffffffff";

        [Fact, Trait("Gate", "G9-Contract")]
        public void REPAIR_DECISION_MISMATCH_PRECEDES_RETAINED_SYMBOL_MISMATCH_WITH_ZERO_WRITES()
        {
            var plannedRack = RackWithRemovedAndRetainedSources(MissingId, IdB);
            var planned = ProjectVariableMutationPreflight.RepairBrokenRack(
                Registry(Literal(IdB, "B", 1)), RackA, new[] { View(plannedRack) }, confirmed: true);

            Assert.True(planned.IsSuccess);
            Assert.Single(planned.Plan.PlanReadSet.RepairDecisionObservations);
            Assert.Single(planned.Plan.PlanReadSet.SymbolResultObservations);

            var currentRack = RackWithRemovedAndRetainedSources(Missing2, IdB);
            var commit = RegistryCommit.Prepare(
                planned.Plan,
                Registry(Literal(IdB, "B", 2)),
                new[] { View(currentRack) });

            Assert.True(commit.IsBlocked);
            Assert.Contains("razon de reparacion", commit.Error, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, commit.RegistryWrites);
            Assert.Equal(0, commit.RackWrites);
            Assert.Null(commit.Changed);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void REPAIR_DECISION_MISMATCH_PRECEDES_BEFORE_SYMBOL_MISMATCH_WITH_ZERO_WRITES()
        {
            var expectedRegistry = Registry(Literal(IdB, "B", 1));
            var expectedEvaluation = RegistryEvaluation.Evaluate(
                ProjectVariablesExpressionAdapter.From(
                    UsableProjectVariablesRegistry.Accredit(ProjectVariablesReadResult.Readable(expectedRegistry)).Registry));
            var source = SelectivePropertyValueDocument.ToProjectVariable(MissingId);
            var readSet = new PlanReadSet(
                new[]
                {
                    SymbolResultObservation.Capture(
                        Symbol(IdB), SymbolObservationPhase.Before, expectedEvaluation.Result(Symbol(IdB))),
                },
                new[]
                {
                    RepairDecisionObservation.Capture(
                        RackA,
                        ProjectPropertyIds.SelectiveVerticalClearance,
                        source,
                        RepairDecisionReason.MissingTarget(new[] { Symbol(MissingId) })),
                });
            var plan = MutationPlan.Of(RegistryMutation.None, Array.Empty<RackMutation>(), readSet);

            var commit = RegistryCommit.Prepare(
                plan,
                Registry(Literal(IdB, "B", 2)),
                new[] { View(Design(directVariable: Missing2)) });

            Assert.True(commit.IsBlocked);
            Assert.Contains("razon de reparacion", commit.Error, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, commit.RegistryWrites);
            Assert.Equal(0, commit.RackWrites);
            Assert.Null(commit.Changed);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void MISSINGTARGET_USES_P11_ORDER_AND_IS_PERMUTATION_INVARIANT_WITH_MIXED_CASE()
        {
            const string lowerA = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
            const string upperB = "BBBBBBBB-CCCC-DDDD-EEEE-FFFFFFFFFFFF";
            Assert.True(string.CompareOrdinal(upperB, lowerA) < 0);

            var forward = RepairDecisionReason.MissingTarget(new[] { Symbol(lowerA), Symbol(upperB) });
            var reverse = RepairDecisionReason.MissingTarget(new[] { Symbol(upperB), Symbol(lowerA) });

            Assert.Equal(new[] { lowerA, upperB }, forward.StableData);
            Assert.Equal(forward.StableData, reverse.StableData);
            Assert.Equal(lowerA, forward.StableData[0]);
            Assert.Equal(upperB, forward.StableData[1]);
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void MISSINGTARGET_PRESERVES_EXACT_A2_D_N_B_P_X_TEXT()
        {
            var value = new Guid("12345678-9abc-def0-1234-56789abcdef0");
            var keys = new[] { value.ToString("D"), value.ToString("N"), value.ToString("B"), value.ToString("P"), value.ToString("X") };
            var reason = RepairDecisionReason.MissingTarget(keys.Select(Symbol));
            var expected = keys.Select(Symbol).OrderBy(id => id).Select(id => id.Key).ToArray();

            Assert.Equal(expected, reason.StableData);
            Assert.All(keys, key => Assert.Contains(key, reason.StableData));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void INTRINSIC_INVALID_ARGUMENTS_ORDER_ARGUMENT_COUNT_NUMERICALLY_AND_COMPARE_STRUCTURALLY()
        {
            var forward = RepairDecisionReason.Intrinsic(new[]
            {
                IntrinsicDiagnosticSignature.InvalidArguments("ABS", 10),
                IntrinsicDiagnosticSignature.InvalidArguments("ABS", 2),
                IntrinsicDiagnosticSignature.InvalidArguments("ABS", 2),
            });
            var reverse = RepairDecisionReason.Intrinsic(new[]
            {
                IntrinsicDiagnosticSignature.InvalidArguments("ABS", 2),
                IntrinsicDiagnosticSignature.InvalidArguments("ABS", 10),
            });

            Assert.Equal(new[] { "InvalidArguments:ABS:2", "InvalidArguments:ABS:10" }, forward.StableData);
            Assert.Equal(new[] { 2, 10 }, forward.IntrinsicSignatures.Select(item => item.ArgumentCount.Value));
            Assert.Equal(forward, reverse);
            Assert.NotEqual(forward, RepairDecisionReason.Intrinsic(new[]
            {
                IntrinsicDiagnosticSignature.InvalidArguments("ABS", 3),
                IntrinsicDiagnosticSignature.InvalidArguments("ABS", 10),
            }));
            Assert.NotEqual(forward, RepairDecisionReason.Intrinsic(new[]
            {
                IntrinsicDiagnosticSignature.InvalidArguments("MAX", 2),
                IntrinsicDiagnosticSignature.InvalidArguments("ABS", 10),
            }));
            Assert.NotEqual(forward, RepairDecisionReason.Intrinsic(new[]
            {
                IntrinsicDiagnosticSignature.ForCode(ExpressionDiagnosticCode.DivisionByZero),
            }));
        }

        [Fact, Trait("Gate", "G9-Contract")]
        public void INSPECTBINDING_ADAPTS_STATIC_AND_NUMERIC_FAILURES_TO_THE_STRUCTURED_MODEL()
        {
            var abs2 = BoundExpression.Call(FunctionId.Abs, new[] { BoundExpression.Number(1), BoundExpression.Number(2) });
            var abs10 = BoundExpression.Call(FunctionId.Abs, Enumerable.Range(0, 10).Select(index => BoundExpression.Number(index)));
            var staticInspection = Inspect(BoundExpression.Binary(BoundBinaryOperator.Add, abs10, abs2));

            Assert.Equal(new[] { 2, 10 }, staticInspection.RepairReason.IntrinsicSignatures
                .Select(item => item.ArgumentCount.Value));
            Assert.All(staticInspection.RepairReason.IntrinsicSignatures,
                item => Assert.Equal(ExpressionDiagnosticCode.InvalidArguments, item.Code));

            var numericInspection = Inspect(BoundExpression.Binary(
                BoundBinaryOperator.Divide, BoundExpression.Number(1), BoundExpression.Number(0)));
            var numeric = Assert.Single(numericInspection.RepairReason.IntrinsicSignatures);
            Assert.Equal(ExpressionDiagnosticCode.DivisionByZero, numeric.Code);
            Assert.Null(numeric.FunctionToken);
            Assert.Null(numeric.ArgumentCount);
        }

        private static SelectivePalletDesignDocument RackWithRemovedAndRetainedSources(string missing, string retained)
        {
            var document = Design();
            document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [ProjectPropertyIds.SelectiveVerticalClearanceToken] = SelectivePropertyValueDocument.ToProjectVariable(missing),
                [ProjectPropertyIds.SelectivePalletToleranceToken] = SelectivePropertyValueDocument.ToProjectVariable(retained),
            };
            document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            return document;
        }

        private static BindingInspection Inspect(BoundExpression expression)
        {
            var accredited = UsableProjectVariablesRegistry.Accredit(
                ProjectVariablesReadResult.Readable(Registry()));
            Assert.True(accredited.IsUsable);
            var evaluation = RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(accredited.Registry));
            return LinkedPropertyInspection.InspectBinding(
                ProjectPropertyIds.SelectiveVerticalClearanceToken,
                SelectivePropertyValueDocument.FromExpression(expression),
                SelectiveLinkedProperties.All,
                accredited.Registry,
                evaluation);
        }

        private static SymbolId Symbol(string key) => SymbolId.ProjectVariable(key);
    }
}
