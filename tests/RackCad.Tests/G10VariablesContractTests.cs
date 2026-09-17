using System;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;
using static RackCad.Tests.G10ContractTestSupport;
using static RackCad.Tests.G8ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>Behavioral contract for the D22 variable authoring and workspace projection.</summary>
    public sealed class G10VariablesContractTests
    {
        [Fact, Trait("Gate", "G10-Variables")]
        public void RACKVARIABLES_ROW_EXPOSES_NAME_EVALUATED_VALUE_AND_LITERAL_DEFINITION()
        {
            var row = Row(RegistryJson(VariableJson(IdA, "A", LiteralDefinition(10))));
            Assert.Equal("A", row.Name);
            Assert.True(EvaluationSucceeded(row));
            Assert.Equal(10, EvaluatedValue(row));
            Assert.Equal("10", DefinitionText(row));
        }

        [Fact, Trait("Gate", "G10-Variables")]
        public void EXPRESSION_ROW_USES_FORMATTER_DEFINITION_AND_SEPARATE_EVALUATED_VALUE()
        {
            var document = RegistryJson(
                VariableJson(IdA, "A", LiteralDefinition(10)),
                VariableJson(IdB, "B", ExpressionDefinition(Binary("add", Reference(IdA), Number(2)))));
            var workspace = Workspace(document);
            var row = workspace.Variables.Single(item => item.Id.Equals(VariableId.Parse(IdB)));
            Assert.Equal("=A + 2", DefinitionText(row));
            Assert.True(EvaluationSucceeded(row));
            Assert.Equal(12, EvaluatedValue(row));
        }

        [Fact, Trait("Gate", "G10-Variables")]
        public void FAILED_VARIABLE_HAS_NO_FABRICATED_EVALUATED_NUMBER_AND_EXPOSES_CAUSE()
        {
            var missing = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
            var workspace = Workspace(RegistryJson(
                VariableJson(IdA, "A", ExpressionDefinition(Reference(missing)))));
            Assert.True(workspace.IsEditable);
            var row = Assert.Single(workspace.Variables);
            Assert.False(EvaluationSucceeded(row));
            Assert.Contains("BrokenReference", EvaluationDiagnostic(row));
            Assert.ThrowsAny<Exception>(() => EvaluatedValue(row));
        }

        [Fact, Trait("Gate", "G10-Variables")]
        public void FORMULA_AUTHORING_INTENT_CARRIES_BOUND_EXPRESSION_BY_IDENTITY()
        {
            var document = ReadRegistry(RegistryJson(
                VariableJson(IdA, "A", LiteralDefinition(10)),
                VariableJson(IdB, "B", ExpressionDefinition(Binary("add", Reference(IdA), Number(2))))));
            var definition = document.ToProjectVariables()
                .Single(item => item.Id.Equals(VariableId.Parse(IdB))).Definition;
            var intent = ChangeDefinitionIntent(VariableId.Parse(IdB), definition);
            Assert.Equal("ChangeDefinition", intent.Kind.ToString());
            Assert.Same(definition, IntentDefinition(intent));
            Assert.Equal(VariableId.Parse(IdB), intent.VariableId);
        }

        [Theory, Trait("Gate", "G10-Variables")]
        [InlineData("Cycle")]
        [InlineData("R1")]
        [InlineData("R2")]
        [InlineData("R3")]
        public void REJECTED_CHANGE_DEFINITION_REACHES_PRESENTATION_AS_STRUCTURED_FAILURE(string expected)
        {
            var result = RejectedChange(expected);
            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
            var presentation = PresentationFailure(result);
            Assert.False(string.IsNullOrWhiteSpace(presentation));
            Assert.DoesNotContain("success", presentation, StringComparison.OrdinalIgnoreCase);
        }

        [Fact, Trait("Gate", "G10-Variables")]
        public void T_A3_39_PRESENTATION_LEADS_WITH_ATTEMPTED_DIVISION_BY_ZERO_NOT_PRIOR_RECOVERY()
        {
            var missing = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
            var registry = G9ContractTestSupport.Registry(
                G9ContractTestSupport.Expression(G9ContractTestSupport.IdA, "A", Reference(missing)),
                G9ContractTestSupport.Expression(G9ContractTestSupport.IdD, "D",
                    Binary("add", Reference(G9ContractTestSupport.IdA), Binary("div", Number(1), Number(0)))));
            var result = G9BehavioralContractSupport.ChangeDefinition(
                registry,
                G9ContractTestSupport.Id(G9ContractTestSupport.IdA),
                VariableDefinition.Literal(1),
                new[]
                {
                    G9ContractTestSupport.View(G9ContractTestSupport.Design(
                        expression: BoundExpressionFor(G9ContractTestSupport.IdD))),
                });
            Assert.False(result.IsSuccess);
            var presentation = PresentationFailure(result);
            Assert.Contains("DivisionByZero", presentation);
            Assert.DoesNotContain("Corregir", presentation, StringComparison.OrdinalIgnoreCase);
        }

        private static VariableMutationPreflightResult RejectedChange(string kind)
        {
            switch (kind)
            {
                case "Cycle":
                    return G9BehavioralContractSupport.ChangeDefinition(
                        G9ContractTestSupport.Registry(G9ContractTestSupport.Literal(G9ContractTestSupport.IdA, "A", 1)),
                        G9ContractTestSupport.Id(G9ContractTestSupport.IdA),
                        G9ContractTestSupport.ExpressionDefinition(BoundExpressionFor(G9ContractTestSupport.IdA)),
                        Array.Empty<ProjectVariableScanEntry>());

                case "R1":
                    return G9BehavioralContractSupport.ChangeDefinition(
                        G9ContractTestSupport.Registry(G9ContractTestSupport.Literal(G9ContractTestSupport.IdA, "A", 1)),
                        G9ContractTestSupport.Id(G9ContractTestSupport.IdA), VariableDefinition.Literal(-1),
                        new[] { G9ContractTestSupport.View(G9ContractTestSupport.Design(directVariable: G9ContractTestSupport.IdA)) });

                case "R2":
                    return G9BehavioralContractSupport.ChangeDefinition(
                        G9ContractTestSupport.Registry(
                            G9ContractTestSupport.Literal(G9ContractTestSupport.IdA, "A", 1),
                            G9ContractTestSupport.Expression(G9ContractTestSupport.IdB, "B", Reference(G9ContractTestSupport.IdA))),
                        G9ContractTestSupport.Id(G9ContractTestSupport.IdA),
                        G9ContractTestSupport.ExpressionDefinition(BoundExpressionFor(G9ContractTestSupport.IdB)),
                        Array.Empty<ProjectVariableScanEntry>());

                case "R3":
                    return G9BehavioralContractSupport.ChangeDefinition(
                        G9ContractTestSupport.Registry(
                            G9ContractTestSupport.Literal(G9ContractTestSupport.IdA, "A", 2),
                            G9ContractTestSupport.Expression(G9ContractTestSupport.IdB, "B",
                                Binary("div", Number(1), Reference(G9ContractTestSupport.IdA)))),
                        G9ContractTestSupport.Id(G9ContractTestSupport.IdA), VariableDefinition.Literal(0),
                        Array.Empty<ProjectVariableScanEntry>());

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static RackCad.Application.Expressions.BoundExpression BoundExpressionFor(string id)
            => RackCad.Application.Expressions.BoundExpression.Reference(
                RackCad.Application.Expressions.SymbolId.ProjectVariable(id));

        private static ProjectVariablesWorkspace Workspace(string json)
            => ProjectVariablesWorkspace.Build(
                ProjectVariablesReadResult.Readable(ReadRegistry(json)),
                Array.Empty<ProjectVariableScanEntry>());

        private static ProjectVariableRow Row(string json) => Assert.Single(Workspace(json).Variables);
    }
}
