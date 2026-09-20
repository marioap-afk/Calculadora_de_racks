using System;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Owner-validation regression contract for OV-01.</summary>
    public sealed class G12A1CreateExpressionHotfixTests
    {
        private const string AlturaId = "11111111-2222-4333-8444-555555555555";
        private const string OtraId = "66666666-7777-4888-8999-aaaaaaaaaaaa";
        private const string MissingId = "bbbbbbbb-cccc-4ddd-8eee-ffffffffffff";

        [Fact, Trait("Gate", "G12-A1")]
        public void CREATE_LITERAL_PRESERVES_HISTORICAL_BEHAVIOR()
        {
            var registry = ProjectVariablesDocument.CreateNew();
            var result = Preflight(registry, "Altura", "10");

            Assert.True(result.IsSuccess, result.Error);
            Assert.Equal(RegistryMutationKind.Add, result.Plan.RegistryMutation.Kind);
            Assert.Equal(VariableDefinitionKind.Literal, result.Plan.RegistryMutation.Definition.Kind);
        }

        [Fact, Trait("Gate", "G12-A1")]
        public void CREATE_NUMERIC_EXPRESSION_EVALUATES_IN_ATTEMPTED_REGISTRY()
        {
            var registry = ProjectVariablesDocument.CreateNew();
            var result = Preflight(registry, "Altura", "=10 + 3");
            var commit = RegistryCommit.Prepare(result.Plan, registry, Array.Empty<ProjectVariableScanEntry>());

            Assert.True(result.IsSuccess, result.Error);
            Assert.True(commit.IsReady, commit.Error);
            var created = Workspace(commit.Changed).Variables.Single();
            Assert.Equal(VariableDefinitionKind.Expression, created.Definition.Kind);
            Assert.Equal(13, created.EvaluatedValue);
        }

        [Fact, Trait("Gate", "G12-A1")]
        public void OV_01_CREATE_EXPRESSION_USES_EXISTING_REGISTRY_THROUGH_REAL_APPLICATION_PATH()
        {
            var registry = Registry(10);
            var workspace = ProjectVariablesWorkspace.Build(
                ProjectVariablesReadResult.Readable(registry),
                Array.Empty<ProjectVariableScanEntry>());

            Assert.True(ProjectVariableDefinitionAuthoring.TryCreate(
                "=Altura + 1", workspace.Variables, out var definition, out var authoringError),
                authoringError);

            var intent = ProjectVariableIntent.Create("AlturaFinal", definition);
            var preflight = ProjectVariableIntentPreflight.Run(
                intent, registry, Array.Empty<ProjectVariableScanEntry>());

            Assert.True(preflight.IsSuccess, preflight.Error);
            var commit = RegistryCommit.Prepare(
                preflight.Plan, registry, Array.Empty<ProjectVariableScanEntry>());

            Assert.True(commit.IsReady, commit.Error);
            var created = ProjectVariablesWorkspace.Build(
                    ProjectVariablesReadResult.Readable(commit.Changed),
                    Array.Empty<ProjectVariableScanEntry>())
                .Variables.Single(row => row.Name == "AlturaFinal");
            Assert.Equal(VariableDefinitionKind.Expression, created.Definition.Kind);
            Assert.Equal("=Altura + 1", created.DefinitionText);
            Assert.True(created.EvaluationSucceeded);
            Assert.Equal(11, created.EvaluatedValue);

            var observation = Assert.Single(preflight.Plan.PlanReadSet.SymbolResultObservations);
            Assert.Equal(SymbolObservationPhase.After, observation.Phase);
            Assert.Equal(preflight.Plan.RegistryMutation.VariableId.Value, observation.SymbolId);
        }

        [Fact, Trait("Gate", "G12-A1")]
        public void UNKNOWN_REFERENCE_IS_REJECTED_BY_AUTHORING_BEFORE_PREFLIGHT()
        {
            var workspace = Workspace(Registry(10));

            Assert.False(ProjectVariableDefinitionAuthoring.TryCreate(
                "=NoExiste + 1", workspace.Variables, out var definition, out var error));
            Assert.Null(definition);
            Assert.Contains("UnknownSymbol", error);
        }

        [Fact, Trait("Gate", "G12-A1")]
        public void ROOT_INVALID_RESULT_IS_A_STRUCTURED_PREFLIGHT_FAILURE()
        {
            var result = Preflight(Registry(10), "AlturaFinal", "=Altura - 20");

            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
            Assert.NotNull(result.AttemptedStateFailure);
            Assert.Equal("Domain", result.AttemptedStateFailure.Category);
            Assert.Contains("Domain", result.AttemptedStateFailure.DiagnosticCodes);
            Assert.False(string.IsNullOrWhiteSpace(result.AttemptedStateFailure.AttemptedSymbolId));
            Assert.Null(result.AttemptedStateFailure.RackId);
            Assert.NotNull(result.PresentationFailure);
        }

        [Fact, Trait("Gate", "G12-A1")]
        public void EXISTING_FAILED_DEPENDENCY_FAILS_CLOSED_WITH_ROOT_CAUSE()
        {
            var registry = RegistryWithExpression(
                AlturaId, "Altura", BoundExpression.Reference(SymbolId.ProjectVariable(MissingId)));

            var result = Preflight(registry, "AlturaFinal", "=Altura + 1");

            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
            Assert.NotNull(result.AttemptedStateFailure);
            Assert.Contains("DependencyFailed", result.AttemptedStateFailure.DiagnosticCodes);
            Assert.Contains("BrokenReference", result.AttemptedStateFailure.DiagnosticCodes);
        }

        [Fact, Trait("Gate", "G12-A1")]
        public void EXISTING_CYCLE_REACHED_BY_CREATE_REMAINS_REJECTED()
        {
            var registry = ProjectVariablesDocument.CreateNew();
            registry.Variables.Add(ExpressionVariable(
                AlturaId, "Altura", BoundExpression.Reference(SymbolId.ProjectVariable(OtraId))));
            registry.Variables.Add(ExpressionVariable(
                OtraId, "Otra", BoundExpression.Reference(SymbolId.ProjectVariable(AlturaId))));

            var result = Preflight(registry, "AlturaFinal", "=Altura + 1");

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.AttemptedStateFailure);
            Assert.Contains("Cycle", result.AttemptedStateFailure.DiagnosticCodes);
        }

        [Fact, Trait("Gate", "G12-A1")]
        public void CREATE_DEPENDENCY_CHANGE_BETWEEN_PREFLIGHT_AND_COMMIT_MISMATCHES_AFTER_X()
        {
            var plannedRegistry = Registry(10);
            var preflight = Preflight(plannedRegistry, "AlturaFinal", "=Altura + 1");

            Assert.True(preflight.IsSuccess, preflight.Error);
            var commit = RegistryCommit.Prepare(
                preflight.Plan, Registry(12), Array.Empty<ProjectVariableScanEntry>());

            Assert.True(commit.IsBlocked);
            Assert.Contains("estado semantico observado cambio", commit.Error, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, commit.RegistryWrites);
            Assert.Null(commit.Changed);
        }

        private static ProjectVariablesDocument Registry(double altura)
        {
            var registry = ProjectVariablesDocument.CreateNew();
            registry.Variables.Add(new ProjectVariableDocument
            {
                VariableId = AlturaId,
                Name = "Altura",
                Type = "Length",
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = altura },
            });
            return registry;
        }

        private static ProjectVariablesDocument RegistryWithExpression(
            string id, string name, BoundExpression expression)
        {
            var registry = ProjectVariablesDocument.CreateNew();
            registry.Variables.Add(ExpressionVariable(id, name, expression));
            return registry;
        }

        private static ProjectVariableDocument ExpressionVariable(
            string id, string name, BoundExpression expression)
            => new ProjectVariableDocument
            {
                VariableId = id,
                Name = name,
                Type = "Length",
                Definition = new ProjectVariableDefinitionDocument
                {
                    Kind = "expression",
                    Expression = expression,
                },
            };

        private static ProjectVariablesWorkspace Workspace(ProjectVariablesDocument registry)
            => ProjectVariablesWorkspace.Build(
                ProjectVariablesReadResult.Readable(registry),
                Array.Empty<ProjectVariableScanEntry>());

        private static VariableMutationPreflightResult Preflight(
            ProjectVariablesDocument registry, string name, string authoredText)
        {
            var workspace = Workspace(registry);
            Assert.True(workspace.IsEditable, workspace.Error);
            Assert.True(ProjectVariableDefinitionAuthoring.TryCreate(
                authoredText, workspace.Variables, out var definition, out var authoringError),
                authoringError);
            return ProjectVariableIntentPreflight.Run(
                ProjectVariableIntent.Create(name, definition),
                registry,
                Array.Empty<ProjectVariableScanEntry>());
        }
    }
}
