using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Units;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G8 contract RED. Fixtures use the exact V6/ADR-0043 wire tokens and exercise the real stores before the
    /// Project Variables adapter, G7 RegistryEvaluation, effective resolver, restamp, export and BOM paths.
    /// </summary>
    public sealed class G8PersistenceIntegrationContractTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string PropertyToken = ProjectPropertyIds.SelectiveVerticalClearanceToken;
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        public static IEnumerable<object[]> AcceptedIdentityTexts()
        {
            yield return new object[] { "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221" };
            yield return new object[] { "8a1d4e772c934b608f156e0b93a7c221" };
            yield return new object[] { "{8a1d4e77-2c93-4b60-8f15-6e0b93a7c221}" };
            yield return new object[] { "(8a1d4e77-2c93-4b60-8f15-6e0b93a7c221)" };
            yield return new object[] { "{0x8a1d4e77,0x2c93,0x4b60,{0x8f,0x15,0x6e,0x0b,0x93,0xa7,0xc2,0x21}}" };
        }

        public static IEnumerable<object[]> ValidBoundaryTrees()
        {
            yield return new object[] { Deep(ExpressionLimits.MaxBoundExpressionDepth) };
            yield return new object[] { G8ContractTestSupport.Call(
                "MAX",
                Enumerable.Range(0, ExpressionLimits.MaxArgumentCount)
                    .Select(index => G8ContractTestSupport.Number(index + 1.0))
                    .ToArray()) };
            yield return new object[] { Balanced(128) };
        }

        public static IEnumerable<object[]> InvalidLimitTrees()
        {
            yield return new object[] { Deep(ExpressionLimits.MaxBoundExpressionDepth + 1) };
            yield return new object[] { G8ContractTestSupport.Call(
                "MAX",
                Enumerable.Range(0, ExpressionLimits.MaxArgumentCount + 1)
                    .Select(index => G8ContractTestSupport.Number(index + 1.0))
                    .ToArray()) };
            yield return new object[] { Balanced(129) };
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void VARIABLE_DEFINITION_EXPRESSION_IS_A_DISTINCT_CLOSED_UNION_CASE()
        {
            var tree = BoundExpression.Number(4.0, LengthUnit.Inch);
            var definition = G8ContractTestSupport.CreateExpressionDefinition(tree);

            Assert.Equal("Expression", definition.Kind.ToString());
            Assert.Equal(tree, G8ContractTestSupport.DefinitionExpression(definition));
            Assert.Throws<InvalidOperationException>(() => definition.LiteralValue);
            Assert.NotEqual(VariableDefinition.Literal(4.0), definition);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void PROPERTY_VALUE_EXPRESSION_COEXISTS_WITH_LITERAL_AND_DIRECT_REFERENCE()
        {
            var tree = BoundExpression.Number(100.0, LengthUnit.Millimeter);
            var expression = G8ContractTestSupport.CreateExpressionProperty(tree);

            Assert.Equal("Expression", expression.Kind.ToString());
            Assert.False(expression.IsLiteral);
            Assert.False(expression.IsProjectVariableReference);
            Assert.Throws<InvalidOperationException>(() => expression.LiteralValue);
            Assert.Throws<InvalidOperationException>(() => expression.VariableId);
            Assert.NotEqual(PropertyValue<double>.Literal(100.0), expression);
            Assert.NotEqual(PropertyValue<double>.Reference(VariableId.Parse(G8ContractTestSupport.IdA)), expression);
        }

        [Theory]
        [InlineData("number")]
        [InlineData("ref")]
        [InlineData("neg")]
        [InlineData("add")]
        [InlineData("sub")]
        [InlineData("mul")]
        [InlineData("div")]
        [InlineData("call")]
        [Trait("Gate", "G8-Contract")]
        public void COMPLETE_BOUND_AST_ROUND_TRIPS_THROUGH_THE_REAL_REGISTRY_STORE(string nodeKind)
        {
            var node = NodeOfKind(nodeKind);
            var json = RegistryWithExpression(G8ContractTestSupport.IdA, "A", node);
            var store = new ProjectVariablesStore();
            var first = G8ContractTestSupport.ReadSingleVariable(json);
            var second = Assert.Single(store.Deserialize(store.Serialize(G8ContractTestSupport.ReadRegistry(json)))
                .Document.ToProjectVariables());

            Assert.Equal(nodeKind, BoundExpressionTokens.NodeToken(G8ContractTestSupport.DefinitionExpression(first.Definition)));
            Assert.Equal(first.Definition, second.Definition);
        }

        [Theory]
        [MemberData(nameof(AcceptedIdentityTexts))]
        [Trait("Gate", "G8-Contract")]
        public void A2_ADAPTER_PRESERVES_VARIABLE_ID_EXACT_TEXT(string identity)
        {
            var document = G8ContractTestSupport.ReadRegistry(G8ContractTestSupport.RegistryJson(
                G8ContractTestSupport.VariableJson(identity, "A", G8ContractTestSupport.LiteralDefinition(6.0))));
            var context = G8ContractTestSupport.Adapt(document);

            Assert.Equal(identity, Assert.Single(context.Symbols.Entries).Id.Key);
            Assert.Equal("A", Assert.Single(context.Symbols.Entries).DisplayName);
            Assert.Equal(SymbolScope.Project, Assert.Single(context.Symbols.Entries).Scope);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void A2_ADAPTER_DOES_NOT_COLLAPSE_TWO_TEXTS_WITH_THE_SAME_GUID_VALUE()
        {
            const string d = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
            const string n = "8a1d4e772c934b608f156e0b93a7c221";
            var document = G8ContractTestSupport.ReadRegistry(G8ContractTestSupport.RegistryJson(
                G8ContractTestSupport.VariableJson(d, "D", G8ContractTestSupport.LiteralDefinition(1.0)),
                G8ContractTestSupport.VariableJson(n, "N", G8ContractTestSupport.LiteralDefinition(2.0))));
            var context = G8ContractTestSupport.Adapt(document);

            Assert.Equal(new[] { n, d }.OrderBy(value => value, StringComparer.OrdinalIgnoreCase),
                context.Symbols.Entries.Select(entry => entry.Id.Key));
            Assert.Equal(2, context.Symbols.Entries.Count);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void PERSISTED_REFERENCE_REMAINS_BOUND_TO_ID_AFTER_RENAME()
        {
            var json = G8ContractTestSupport.RegistryJson(
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdA, "Nombre viejo", G8ContractTestSupport.LiteralDefinition(6.0)),
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdB, "B",
                    G8ContractTestSupport.ExpressionDefinition(G8ContractTestSupport.Reference(G8ContractTestSupport.IdA))));
            var document = G8ContractTestSupport.ReadRegistry(json);
            document.Variables.Single(item => item.VariableId == G8ContractTestSupport.IdA).Name = "Nombre nuevo";
            var reloaded = G8ContractTestSupport.ReadRegistry(new ProjectVariablesStore().Serialize(document));
            var reference = Assert.IsType<BoundReference>(G8ContractTestSupport.DefinitionExpression(
                reloaded.ToProjectVariables().Single(item => item.Id.Value == G8ContractTestSupport.IdB).Definition));

            Assert.Equal(G8ContractTestSupport.IdA, reference.Symbol.Key);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void UNKNOWN_VARIABLE_DEFINITION_KIND_REMAINS_STRUCTURALLY_UNREADABLE()
        {
            var json = G8ContractTestSupport.RegistryJson(G8ContractTestSupport.VariableJson(
                G8ContractTestSupport.IdA, "A", "{\"Kind\":\"futureKind\",\"Value\":6}"));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable,
                new ProjectVariablesStore().Deserialize(json).Outcome);
        }

        [Theory]
        [InlineData("{\"Node\":\"future\"}")]
        [InlineData("{\"Node\":\"ref\",\"Namespace\":\"rack\",\"Id\":\"8a1d4e77-2c93-4b60-8f15-6e0b93a7c221\"}")]
        [InlineData("{\"Node\":\"call\",\"Function\":\"ROUND\",\"Arguments\":[]}")]
        [InlineData("{\"Node\":\"number\",\"Value\":1,\"Unit\":\"cm\"}")]
        [InlineData("{\"Node\":\"number\",\"Value\":1,\"Future\":2}")]
        [InlineData("{\"Node\":\"number\",\"Node\":\"number\",\"Value\":1}")]
        [InlineData("{\"Node\":\"ref\",\"Namespace\":\"projectVariable\"}")]
        [InlineData("{\"Node\":\"ref\",\"Namespace\":\"projectVariable\",\"Id\":\"not-a-guid\"}")]
        [Trait("Gate", "G8-Contract")]
        public void UNKNOWN_OR_MALFORMED_EXPRESSION_PAYLOAD_IS_STRUCTURALLY_UNREADABLE(string node)
        {
            var result = new ProjectVariablesStore().Deserialize(
                RegistryWithExpression(G8ContractTestSupport.IdA, "A", node));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, result.Outcome);
            Assert.Null(result.Document);
        }

        [Theory]
        [MemberData(nameof(ValidBoundaryTrees))]
        [Trait("Gate", "G8-Contract")]
        public void PERSISTED_BOUND_TREE_AT_EACH_NORMATIVE_LIMIT_IS_READABLE(string node)
        {
            Assert.Equal(ProjectVariablesReadOutcome.Readable,
                new ProjectVariablesStore().Deserialize(RegistryWithExpression(G8ContractTestSupport.IdA, "A", node)).Outcome);
        }

        [Theory]
        [MemberData(nameof(InvalidLimitTrees))]
        [Trait("Gate", "G8-Contract")]
        public void PERSISTED_BOUND_TREE_BEYOND_EACH_NORMATIVE_LIMIT_IS_UNREADABLE(string node)
        {
            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable,
                new ProjectVariablesStore().Deserialize(RegistryWithExpression(G8ContractTestSupport.IdA, "A", node)).Outcome);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void PERSISTED_DEPENDENCY_CHAIN_USES_G7_REGISTRY_EVALUATION()
        {
            var document = G8ContractTestSupport.ReadRegistry(G8ContractTestSupport.RegistryJson(
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdA, "A", G8ContractTestSupport.LiteralDefinition(1.0)),
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdB, "B", G8ContractTestSupport.ExpressionDefinition(
                    G8ContractTestSupport.Binary("add", G8ContractTestSupport.Reference(G8ContractTestSupport.IdA), G8ContractTestSupport.Number(2.0)))),
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdC, "C", G8ContractTestSupport.ExpressionDefinition(
                    G8ContractTestSupport.Binary("mul", G8ContractTestSupport.Reference(G8ContractTestSupport.IdB), G8ContractTestSupport.Number(3.0))))));
            var evaluation = G8ContractTestSupport.Evaluate(document);

            Assert.Equal(1.0, evaluation.Result(SymbolId.ProjectVariable(G8ContractTestSupport.IdA)).Value);
            Assert.Equal(3.0, evaluation.Result(SymbolId.ProjectVariable(G8ContractTestSupport.IdB)).Value);
            Assert.Equal(9.0, evaluation.Result(SymbolId.ProjectVariable(G8ContractTestSupport.IdC)).Value);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void MULTI_CAUSE_ROOTS_SURVIVE_REAL_PERSISTENCE_AND_ADAPTER()
        {
            const string missing = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
            var document = G8ContractTestSupport.ReadRegistry(G8ContractTestSupport.RegistryJson(
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdA, "A", G8ContractTestSupport.ExpressionDefinition(
                    G8ContractTestSupport.Reference(missing))),
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdB, "B", G8ContractTestSupport.ExpressionDefinition(
                    G8ContractTestSupport.Binary("div", G8ContractTestSupport.Number(1.0), G8ContractTestSupport.Number(0.0)))),
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdD, "D", G8ContractTestSupport.ExpressionDefinition(
                    G8ContractTestSupport.Binary("add", G8ContractTestSupport.Reference(G8ContractTestSupport.IdA),
                        G8ContractTestSupport.Reference(G8ContractTestSupport.IdB))))));
            var result = G8ContractTestSupport.Evaluate(document).Result(SymbolId.ProjectVariable(G8ContractTestSupport.IdD));

            Assert.False(result.Succeeded);
            Assert.Equal(new[] { ExpressionDiagnosticCode.BrokenReference, ExpressionDiagnosticCode.DivisionByZero },
                result.RootCauses.Select(root => root.Code));
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void INVALID_ARGUMENTS_REMAINS_SEMANTIC_THROUGH_REAL_PERSISTENCE()
        {
            var document = G8ContractTestSupport.ReadRegistry(RegistryWithExpression(
                G8ContractTestSupport.IdA, "A", G8ContractTestSupport.Call("MAX", G8ContractTestSupport.Number(1.0))));
            var result = G8ContractTestSupport.Evaluate(document).Result(SymbolId.ProjectVariable(G8ContractTestSupport.IdA));

            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(ExpressionDiagnosticCode.InvalidArguments, diagnostic.Code);
            Assert.Equal("MAX", diagnostic.FunctionToken);
            Assert.Equal(1, diagnostic.ArgumentCount);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void NON_CANONICAL_PERSISTED_DEFINITION_IS_READABLE_BUT_FAILS_SEMANTICALLY()
        {
            var document = G8ContractTestSupport.ReadRegistry(
                RegistryWithExpression(G8ContractTestSupport.IdA, "A", G8ContractTestSupport.Number(6.0)));
            var result = G8ContractTestSupport.Evaluate(document).Result(SymbolId.ProjectVariable(G8ContractTestSupport.IdA));

            Assert.Equal(ExpressionDiagnosticCode.NonCanonicalForm, Assert.Single(result.Diagnostics).Code);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void RENAME_PREFLIGHT_PRESERVES_AN_EXPRESSION_DEFINITION()
        {
            var registry = G8ContractTestSupport.ReadRegistry(RegistryWithExpression(
                G8ContractTestSupport.IdA, "Antes", G8ContractTestSupport.Number(100.0, "mm")));
            var renamed = ProjectVariableMutationPreflight.Rename(
                registry, VariableId.Parse(G8ContractTestSupport.IdA), "Despues");

            Assert.True(renamed.IsSuccess);
            var result = renamed.Plan.RegistryMutation.ApplyTo(registry).ToProjectVariables().Single();
            Assert.Equal("Despues", result.Name);
            Assert.Equal(VariableDefinitionKind.Expression, result.Definition.Kind);
            Assert.Equal(BoundExpression.Number(100.0, LengthUnit.Millimeter), result.Definition.ExpressionValue);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void LINK_OPTIONS_USE_THE_EVALUATED_EXPRESSION_VALUE()
        {
            var read = ProjectVariablesReadResult.Readable(G8ContractTestSupport.ReadRegistry(
                RegistryWithExpression(
                    G8ContractTestSupport.IdA, "Calculada", G8ContractTestSupport.Number(100.0, "mm"))));

            var result = LinkedPropertyOptions.ForProperty(
                ProjectPropertyIds.SelectiveVerticalClearance, read);

            Assert.True(result.IsUsable);
            Assert.Equal(100.0 / 25.4, Assert.Single(result.Options).LiteralValue, 12);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void WORKSPACE_PROJECTS_THE_EVALUATED_EXPRESSION_VALUE()
        {
            var read = ProjectVariablesReadResult.Readable(G8ContractTestSupport.ReadRegistry(
                RegistryWithExpression(
                    G8ContractTestSupport.IdA, "Calculada", G8ContractTestSupport.Number(100.0, "mm"))));

            var workspace = ProjectVariablesWorkspace.Build(read, new ProjectVariableScanEntry[0]);

            Assert.True(workspace.IsEditable);
            Assert.Equal(100.0 / 25.4, Assert.Single(workspace.Variables).LiteralValue, 12);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void SCHEMA_V0_PRESERVES_1_0_WHEN_EXPRESSION_EXISTS_AND_AFTER_IT_IS_REMOVED()
        {
            var store = new ProjectVariablesStore();
            var document = G8ContractTestSupport.ReadRegistry(
                RegistryWithExpression(G8ContractTestSupport.IdA, "A", G8ContractTestSupport.Number(100.0, "mm")));
            Assert.Contains("\"SchemaVersion\":\"1.0\"", store.Serialize(document));

            document.Variables[0].Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 6.0 };
            var withoutExpression = store.Serialize(document);
            Assert.Contains("\"SchemaVersion\":\"1.0\"", withoutExpression);
            Assert.DoesNotContain("expression", withoutExpression, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void SCHEMA_V0_NEVER_DOWNGRADES_A_COMPATIBLE_HIGHER_MINOR_WITH_EXPRESSION()
        {
            var store = new ProjectVariablesStore();
            var json = RegistryWithExpression(G8ContractTestSupport.IdA, "A", G8ContractTestSupport.Number(100.0, "mm"))
                .Replace("\"1.0\"", "\"1.7\"");
            var document = G8ContractTestSupport.ReadRegistry(json);

            Assert.Contains("\"SchemaVersion\":\"1.7\"", store.Serialize(document));
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void PROPERTY_EXPRESSION_ROUND_TRIPS_AND_RESOLVES_AGAINST_THE_SHARED_SNAPSHOT()
        {
            var document = Reload(DesignWithExpression(G8ContractTestSupport.Binary(
                "add", G8ContractTestSupport.Reference(G8ContractTestSupport.IdA), G8ContractTestSupport.Number(4.0))));
            var result = new SelectiveEffectiveDesignResolver().Resolve(document, LiteralRegistry(6.0));

            Assert.True(result.IsSuccess);
            Assert.Equal(10.0, result.Design.VerticalClearance);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void FAILED_PROPERTY_EXPRESSION_HAS_NO_FROZEN_LITERAL_FALLBACK()
        {
            var document = Reload(DesignWithExpression(G8ContractTestSupport.Binary(
                "div", G8ContractTestSupport.Number(1.0), G8ContractTestSupport.Number(0.0))));
            var result = new SelectiveEffectiveDesignResolver().Resolve(document, LiteralRegistry(6.0));

            Assert.False(result.IsSuccess);
            Assert.Contains("DivisionByZero", result.Error, StringComparison.Ordinal);
            Assert.NotEqual(6.0, result.Design?.VerticalClearance);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void DIRECT_REFERENCE_TO_FAILED_EXPRESSION_VARIABLE_HAS_NO_LITERAL_FALLBACK()
        {
            var registry = G8ContractTestSupport.ReadRegistry(RegistryWithExpression(
                G8ContractTestSupport.IdA,
                "A",
                G8ContractTestSupport.Binary("div", G8ContractTestSupport.Number(1.0), G8ContractTestSupport.Number(0.0))));
            var authored = SelectivePalletDesignDocument.From(Design(6.0), RackId, "Rack A");
            authored.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [PropertyToken] = SelectivePropertyValueDocument.ToProjectVariable(G8ContractTestSupport.IdA),
            };

            var result = new SelectiveEffectiveDesignResolver().Resolve(Reload(authored), registry);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Design);
            Assert.Contains("DivisionByZero", result.Error, StringComparison.Ordinal);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void FAILED_UNUSED_VARIABLE_DOES_NOT_BLOCK_EXPRESSION_BACKED_BOM()
        {
            var registry = G8ContractTestSupport.ReadRegistry(G8ContractTestSupport.RegistryJson(
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdA, "A", G8ContractTestSupport.LiteralDefinition(6.0)),
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdB, "Unused", G8ContractTestSupport.ExpressionDefinition(
                    G8ContractTestSupport.Binary("div", G8ContractTestSupport.Number(1.0), G8ContractTestSupport.Number(0.0))))));
            var authored = Reload(DesignWithExpression(G8ContractTestSupport.Binary(
                "add", G8ContractTestSupport.Reference(G8ContractTestSupport.IdA), G8ContractTestSupport.Number(24.0))));
            var effective = new SelectiveEffectiveDesignResolver().Resolve(authored, registry);

            Assert.True(effective.IsSuccess);
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var bom = SelectiveBomBuilder.Build(new SelectiveGeometryResolver().Resolve(effective.Design, catalog), catalog);
            Assert.NotEmpty(bom.Lines);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void UNKNOWN_PROPERTY_SOURCE_KIND_IS_STRUCTURALLY_UNREADABLE_AT_THE_STORE()
        {
            var document = DesignWithExpression(G8ContractTestSupport.Number(100.0, "mm"));
            document.PropertyValues[PropertyToken].Kind = "futureKind";
            var json = new SelectivePalletDesignStore().Serialize(document);

            Assert.Throws<InvalidOperationException>(() => new SelectivePalletDesignStore().Deserialize(json));
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void DUPLICATE_FIELD_INSIDE_PROPERTY_EXPRESSION_IS_STRUCTURALLY_UNREADABLE()
        {
            var store = new SelectivePalletDesignStore();
            var json = store.Serialize(DesignWithExpression(G8ContractTestSupport.Number(100.0, "mm")))
                .Replace("\"Node\":\"number\"", "\"Node\":\"number\",\"Node\":\"number\"");

            Assert.Throws<InvalidOperationException>(() => store.Deserialize(json));
        }

        [Theory]
        [InlineData("{\"Node\":\"future\"}")]
        [InlineData("{\"Node\":\"number\",\"Value\":1,\"Unit\":\"cm\"}")]
        [InlineData("{\"Node\":\"ref\",\"Namespace\":\"rack\",\"Id\":\"8a1d4e77-2c93-4b60-8f15-6e0b93a7c221\"}")]
        [Trait("Gate", "G8-Contract")]
        public void MALFORMED_PROPERTY_EXPRESSION_IS_STRUCTURALLY_UNREADABLE_AT_THE_STORE(string node)
        {
            var store = new SelectivePalletDesignStore();
            var json = store.Serialize(DesignWithExpression(node));

            Assert.Throws<InvalidOperationException>(() => store.Deserialize(json));
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void RESTAMP_PRESERVES_EXPRESSION_SOURCE_WITH_THE_SAME_BOUND_REFERENCE()
        {
            var store = new SelectivePalletDesignStore();
            var result = SelectiveAuthoredRestamp.Restamp(
                store.Serialize(DesignWithExpression(G8ContractTestSupport.Reference(G8ContractTestSupport.IdA))),
                "44444444-5555-6666-7777-888888888888",
                "Rack copia");

            Assert.True(result.IsSuccess);
            Assert.Contains("\"Kind\":\"expression\"", result.DesignJson);
            Assert.Contains(G8ContractTestSupport.IdA, result.DesignJson);
            Assert.Equal("expression", ReloadJson(result.DesignJson).PropertyValues[PropertyToken].Kind);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void EXPORT_MATERIALIZES_EXPRESSION_EFFECTIVE_AND_WRITES_LITERAL_ONLY()
        {
            var export = SelectiveLibraryExport.Materialize(
                Reload(DesignWithExpression(G8ContractTestSupport.Binary(
                    "add", G8ContractTestSupport.Reference(G8ContractTestSupport.IdA), G8ContractTestSupport.Number(4.0)))),
                LiteralRegistry(6.0));

            Assert.True(export.IsSuccess);
            Assert.Equal(10.0, export.Document.VerticalClearance);
            Assert.Null(export.Document.PropertyValues);
        }

        [Fact]
        [Trait("Gate", "G8-Contract")]
        public void BOM_USES_EXPRESSION_EFFECTIVE_AND_NEVER_THE_FROZEN_LITERAL()
        {
            var authored = Reload(DesignWithExpression(G8ContractTestSupport.Binary(
                "add", G8ContractTestSupport.Reference(G8ContractTestSupport.IdA), G8ContractTestSupport.Number(24.0))));
            var effective = new SelectiveEffectiveDesignResolver().Resolve(authored, LiteralRegistry(6.0));
            Assert.True(effective.IsSuccess);
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var expressionBom = SelectiveBomBuilder.Build(new SelectiveGeometryResolver().Resolve(effective.Design, catalog), catalog);
            var literalBom = SelectiveBomBuilder.Build(new SelectiveGeometryResolver().Resolve(Design(30.0), catalog), catalog);
            var frozenBom = SelectiveBomBuilder.Build(new SelectiveGeometryResolver().Resolve(Design(6.0), catalog), catalog);

            Assert.Equal(BomSignature(literalBom), BomSignature(expressionBom));
            Assert.NotEqual(BomSignature(frozenBom), BomSignature(expressionBom));
        }

        private static string RegistryWithExpression(string id, string name, string node)
            => G8ContractTestSupport.RegistryJson(G8ContractTestSupport.VariableJson(
                id, name, G8ContractTestSupport.ExpressionDefinition(node)));

        private static string NodeOfKind(string kind)
        {
            switch (kind)
            {
                case "number": return G8ContractTestSupport.Number(100.0, "mm");
                case "ref": return G8ContractTestSupport.Reference(G8ContractTestSupport.IdB);
                case "neg": return G8ContractTestSupport.Negate(G8ContractTestSupport.Number(1.0));
                case "add": return G8ContractTestSupport.Binary("add", G8ContractTestSupport.Number(1.0), G8ContractTestSupport.Number(2.0));
                case "sub": return G8ContractTestSupport.Binary("sub", G8ContractTestSupport.Number(3.0), G8ContractTestSupport.Number(2.0));
                case "mul": return G8ContractTestSupport.Binary("mul", G8ContractTestSupport.Number(2.0), G8ContractTestSupport.Number(3.0));
                case "div": return G8ContractTestSupport.Binary("div", G8ContractTestSupport.Number(6.0), G8ContractTestSupport.Number(2.0));
                case "call": return G8ContractTestSupport.Call("ABS", G8ContractTestSupport.Number(1.0));
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static string Deep(int depth)
        {
            var node = G8ContractTestSupport.Number(1.0, "mm");
            for (var index = 1; index < depth; index++)
            {
                node = G8ContractTestSupport.Negate(node);
            }

            return node;
        }

        private static string Balanced(int leaves)
        {
            var level = Enumerable.Range(0, leaves).Select(_ => G8ContractTestSupport.Number(1.0, "mm")).ToList();
            while (level.Count > 1)
            {
                var next = new List<string>();
                for (var index = 0; index < level.Count; index += 2)
                {
                    next.Add(index + 1 < level.Count
                        ? G8ContractTestSupport.Binary("add", level[index], level[index + 1])
                        : level[index]);
                }

                level = next;
            }

            return level[0];
        }

        private static ProjectVariablesDocument LiteralRegistry(double value)
            => G8ContractTestSupport.ReadRegistry(G8ContractTestSupport.RegistryJson(
                G8ContractTestSupport.VariableJson(G8ContractTestSupport.IdA, "A", G8ContractTestSupport.LiteralDefinition(value))));

        private static SelectivePalletDesignDocument DesignWithExpression(string node)
        {
            var document = SelectivePalletDesignDocument.From(Design(6.0), RackId, "Rack A");
            document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [PropertyToken] = G8ContractTestSupport.ExpressionPropertyDocument(node),
            };
            return document;
        }

        private static SelectivePalletDesignDocument Reload(SelectivePalletDesignDocument document)
        {
            var store = new SelectivePalletDesignStore();
            return store.Deserialize(store.Serialize(document));
        }

        private static SelectivePalletDesignDocument ReloadJson(string json)
            => new SelectivePalletDesignStore().Deserialize(json);

        private static SelectivePalletDesign Design(double clearance)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = clearance,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1,
                DrawBasePlate = true,
            };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            for (var index = 0; index < 2; index++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0,
                });
            }

            design.Bays.Add(bay);
            return design;
        }

        private static string BomSignature(BillOfMaterials bom)
            => string.Join("|", bom.Lines.Select(line =>
                line.Category + ":" + line.ProfileId + ":" + line.Length.ToString("R") + "x" + line.Quantity));
    }
}
