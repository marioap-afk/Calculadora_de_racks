using System;
using System.Collections.Generic;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;
using static RackCad.Tests.G10ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>Behavioral contract for D21/P23 over the existing pure editor session.</summary>
    public sealed class G10EditorContractTests
    {
        private static readonly VariableId A = VariableId.Parse("3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44");
        private static readonly VariableId B = VariableId.Parse("5d9e2a10-77b4-4c31-8e06-2f9a4b7c1d38");
        private const string Missing = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";

        [Fact, Trait("Gate", "G10-Editor")]
        public void FORMULA_TEXT_IS_A_DRAFT_EXPRESSION_AND_DOES_NOT_MUTATE_COMMITTED_SOURCE()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=A + 2");
            Assert.Equal("DraftExpression", session.Draft.ToString());
            Assert.Equal("Literal", SourceKind(session));
            Assert.Equal(6, session.Committed.CommittedLiteral);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void ENTER_COMMITS_EXACT_UNIQUE_NAME_AS_HISTORICAL_DIRECT_REFERENCE()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=A");
            Assert.True(session.TryCommitByEnter(out var error), error);
            Assert.Equal("ProjectVariableReference", SourceKind(session));
            Assert.Equal(A, session.Committed.Source.VariableId);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void ENTER_COMMITS_NONTRIVIAL_FORMULA_AS_BOUND_EXPRESSION()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=MAX(A, B)");
            Assert.True(session.TryCommitByEnter(out var error), error);
            Assert.Equal("Expression", SourceKind(session));
            Assert.NotNull(Expression(session));
            Assert.Equal(6, session.Committed.CommittedLiteral);
        }

        [Theory, Trait("Gate", "G10-Editor")]
        [InlineData("=Unknown + 2", "UnknownSymbol")]
        [InlineData("=Same + 2", "AmbiguousName")]
        [InlineData("=A#{bad}", "InvalidQualifier")]
        [InlineData("={A", "UnterminatedName")]
        public void INVALID_EXPRESSION_REJECTS_WITH_TYPED_DIAGNOSTIC_AND_ZERO_SOURCE_MUTATION(string draft, string code)
        {
            var session = Session(LinkedPropertyEditState.Literal(6), homonyms: true);
            session.Type(draft);
            Assert.False(session.TryCommitByEnter(out _));
            Assert.Equal(code, DiagnosticCode(session));
            Assert.Equal("Literal", SourceKind(session));
            Assert.Equal(6, session.Committed.CommittedLiteral);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void PARSER_RESOURCE_GUARD_REJECTS_WITH_TYPED_DIAGNOSTIC_AND_ZERO_SOURCE_MUTATION()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=" + new string('(', 600) + "A" + new string(')', 600));
            Assert.False(session.TryCommitByEnter(out _));
            Assert.Equal("ResourceLimit", DiagnosticCode(session));
            Assert.Equal("Literal", SourceKind(session));
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void SUCCESSFUL_EXPRESSION_COMMIT_DISPLAYS_FORMATTER_CANONICAL_TEXT()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type(" = max( A ,B ) ");
            Assert.True(session.TryCommitByEnter(out var error), error);
            Assert.Equal("=MAX(A, B)", session.Text);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void RENAME_REFRESHES_PRESENTATION_WITHOUT_REBINDING_THE_BOUND_SYMBOL()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=A + 2");
            Assert.True(session.TryCommitByEnter(out var error), error);
            var expression = Expression(session);
            var renamed = new LinkedPropertyEditSession(session.Committed, new[]
            {
                new LinkedPropertyOption(A, "Renamed", VariableType.Length, 10),
                new LinkedPropertyOption(B, "B", VariableType.Length, 12),
            });
            Assert.Same(expression, Expression(renamed));
            Assert.Equal("=Renamed + 2", renamed.Text);
        }

        [Theory, Trait("Gate", "G10-Editor")]
        [InlineData("3F2B1C9E-6D4A-4F38-9B71-0C2A5E8D1F44", "#3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44")]
        [InlineData("3F2B1C9E6D4A4F389B710C2A5E8D1F44", "#{3f2b1c9e6d4a4f389b710c2a5e8d1f44}")]
        [InlineData("{3F2B1C9E-6D4A-4F38-9B71-0C2A5E8D1F44}", "#{{3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44}}}")]
        public void AMBIGUOUS_EDITOR_DISPLAY_USES_A2_EXACT_KEY_QUALIFIER(string key, string qualifier)
        {
            var id = VariableId.Parse(key);
            var session = new LinkedPropertyEditSession(LinkedPropertyEditState.Reference(6, id), new[]
            {
                new LinkedPropertyOption(id, "Same", VariableType.Length, 10),
                new LinkedPropertyOption(B, "Same", VariableType.Length, 12),
            });
            Assert.Equal("=Same" + qualifier, session.Text);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void AUTOCOMPLETE_SELECTION_INSIDE_FORMULA_ONLY_INSERTS_AUTHORING_TEXT()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=A + 2");
            Assert.Single(session.Candidates, option => option.VariableId.Equals(A));
            Assert.Equal("Literal", SourceKind(session));
            Assert.Equal("=A + 2", session.Text);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void ESCAPE_FROM_FORMULA_RESTORES_CANONICAL_COMMITTED_EXPRESSION()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=A + 2");
            Assert.True(session.TryCommitByEnter(out var error), error);
            session.Type("=A +");
            session.Cancel();
            Assert.Equal("=A + 2", session.Text);
            Assert.Equal("Expression", SourceKind(session));
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void LOSTFOCUS_NEVER_CONVERTS_LITERAL_OR_REFERENCE_TO_EXPRESSION()
        {
            var literal = Session(LinkedPropertyEditState.Literal(6));
            literal.Type("=A + 2");
            Assert.False(literal.TryCommitByLostFocus(out _));
            Assert.Equal("Literal", SourceKind(literal));

            var reference = Session(LinkedPropertyEditState.Reference(6, A));
            reference.Type("=A + 2");
            Assert.False(reference.TryCommitByLostFocus(out _));
            Assert.Equal("ProjectVariableReference", SourceKind(reference));
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void DOMAIN_FAILURE_PRESERVES_DRAFT_AND_COMMITTED_EFFECTIVE_STATE_WITHOUT_CLAMP_OR_FALLBACK()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=A - 20");
            Assert.False(session.TryCommitByEnter(out _));
            Assert.Equal("Domain", DiagnosticCode(session));
            Assert.Equal("=A - 20", session.Text);
            Assert.Equal("Literal", SourceKind(session));
            Assert.Equal(6, session.Committed.CommittedLiteral);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void FAILED_PROJECT_VARIABLE_READ_REJECTS_PROPERTY_FORMULA_WITH_STRUCTURED_ROOT_CAUSE()
        {
            var projection = PropertyProjection(
                VariableDefinition.Expression(BoundExpression.Reference(SymbolId.ProjectVariable(Missing))));
            var session = new LinkedPropertyEditSession(
                LinkedPropertyEditState.Literal(6), projection.Options, projection.AuthoringContext);
            session.Type("=A + 2");
            Assert.False(session.TryCommitByEnter(out _));
            Assert.Equal("DependencyFailed", DiagnosticCode(session));
            Assert.Contains(Assert.Single(session.Diagnostics).RegistryResult.RootCauses,
                root => root.Code == ExpressionDiagnosticCode.BrokenReference);
            Assert.Equal("Literal", SourceKind(session));
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void PRODUCTIVE_PROPERTY_AUTHORING_PRESERVES_EXISTING_FAILED_SYMBOL_IDENTITY()
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = A.Value,
                    Name = "A",
                    Type = "Length",
                    Definition = new ProjectVariableDefinitionDocument
                    {
                        Kind = "expression",
                        Expression = BoundExpression.Reference(SymbolId.ProjectVariable(Missing)),
                    },
                },
            };
            var projection = LinkedPropertyOptions.ForProperty(
                ProjectPropertyIds.SelectiveVerticalClearance,
                ProjectVariablesReadResult.Readable(document));
            Assert.True(projection.IsUsable, projection.Error);

            var session = new LinkedPropertyEditSession(
                LinkedPropertyEditState.Literal(6), projection.Options, projection.AuthoringContext);
            session.Type("=A + 2");

            Assert.False(session.TryCommitByEnter(out _));
            Assert.Equal("DependencyFailed", DiagnosticCode(session));
            var diagnostic = Assert.Single(session.Diagnostics);
            Assert.Equal(SymbolId.ProjectVariable(A.Value), diagnostic.FailedSymbol);
            Assert.Contains(diagnostic.RegistryResult.RootCauses,
                root => root.Code == ExpressionDiagnosticCode.BrokenReference);
            Assert.Equal("Literal", SourceKind(session));
            Assert.Equal(6, session.Committed.CommittedLiteral);
            Assert.Empty(projection.Options);

            session.Type("=A");
            Assert.False(session.TryCommitByEnter(out _));
            Assert.Equal("DependencyFailed", DiagnosticCode(session));

            session.Type("=Unknown");
            Assert.False(session.TryCommitByEnter(out _));
            Assert.Equal("UnknownSymbol", DiagnosticCode(session));
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void FAILED_EXISTING_EXPRESSION_REOPENS_WITH_IDENTITY_AND_RECOVERS_AFTER_VARIABLE_FIX()
        {
            var expression = BoundExpression.Binary(
                BoundBinaryOperator.Add,
                BoundExpression.Reference(SymbolId.ProjectVariable(A.Value)),
                BoundExpression.Number(2));
            var failed = PropertyProjection(
                VariableDefinition.Expression(BoundExpression.Reference(SymbolId.ProjectVariable(Missing))));
            var reopened = new LinkedPropertyEditSession(
                LinkedPropertyEditState.Expression(6, expression),
                failed.Options,
                failed.AuthoringContext);

            Assert.Same(expression, Expression(reopened));
            Assert.Equal("=A + 2", reopened.Text);
            Assert.False(reopened.TryGetEffectiveValue(out _));
            reopened.Type("=Unknown + 1");
            reopened.Cancel();
            Assert.Equal("=A + 2", reopened.Text);
            Assert.Same(expression, Expression(reopened));

            var healthy = PropertyProjection(VariableDefinition.Literal(10));
            var recovered = new LinkedPropertyEditSession(
                reopened.Committed,
                healthy.Options,
                healthy.AuthoringContext);
            Assert.Same(expression, Expression(recovered));
            Assert.Equal("=A + 2", recovered.Text);
            Assert.True(recovered.TryGetEffectiveValue(out var value));
            Assert.Equal(12, value);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void FAILED_COMMIT_PRESERVES_DIRECT_REFERENCE_ID_AND_FROZEN_LITERAL()
        {
            var session = Session(LinkedPropertyEditState.Reference(6, A));
            session.Type("=Unknown + 2");
            Assert.False(session.TryCommitByEnter(out _));
            Assert.Equal("ProjectVariableReference", SourceKind(session));
            Assert.Equal(A, session.Committed.Source.VariableId);
            Assert.Equal(6, session.Committed.CommittedLiteral);
        }

        [Fact, Trait("Gate", "G10-Editor")]
        public void FAILED_COMMIT_PRESERVES_EXISTING_EXPRESSION_TREE_AND_EFFECTIVE_STATE()
        {
            var session = Session(LinkedPropertyEditState.Literal(6));
            session.Type("=A + 2");
            Assert.True(session.TryCommitByEnter(out var error), error);
            var committedTree = Expression(session);
            Assert.True(session.TryGetEffectiveValue(out var committedEffective));

            session.Type("=Unknown + 2");
            Assert.False(session.TryCommitByEnter(out _));
            Assert.Same(committedTree, Expression(session));
            Assert.True(session.TryGetEffectiveValue(out var effective));
            Assert.Equal(committedEffective, effective);
        }

        private static LinkedPropertyEditSession Session(
            LinkedPropertyEditState state, bool homonyms = false)
        {
            var options = new[]
            {
                new LinkedPropertyOption(A, homonyms ? "Same" : "A", VariableType.Length, 10),
                new LinkedPropertyOption(B, homonyms ? "Same" : "B", VariableType.Length, 12),
            };
            return new LinkedPropertyEditSession(state, options);
        }

        private static LinkedPropertyOptionsResult PropertyProjection(VariableDefinition definition)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = A.Value,
                    Name = "A",
                    Type = "Length",
                    Definition = ProjectVariableDefinitionDocument.From(definition),
                },
            };

            var projection = LinkedPropertyOptions.ForProperty(
                ProjectPropertyIds.SelectiveVerticalClearance,
                ProjectVariablesReadResult.Readable(document));
            Assert.True(projection.IsUsable, projection.Error);
            return projection;
        }
    }
}
