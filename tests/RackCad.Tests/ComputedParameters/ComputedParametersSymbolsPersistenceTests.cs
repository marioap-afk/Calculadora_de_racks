using System;
using System.Text.Json;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.ComputedParametersSymbolsKit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G3-T1 (RED) - INV-24: persistencia cerrada en los dos sentidos (D-18, D-19, ADR-0043 D9). La tabla de
    /// namespaces EN MEMORIA conoce <c>rack</c>; la de tokens PERSISTIDOS solo conoce <c>projectVariable</c>. Con una sola
    /// tabla, el lector aceptaria <c>rack</c> y el escritor lo escribiria (<c>PersistedBoundExpressionJson.cs:95,170</c>).
    /// El caso de lectura es el de <c>G8PersistenceIntegrationContractTests</c>, que sigue en verde sin modificarse.
    /// </summary>
    public class ComputedParametersSymbolsPersistenceTests
    {
        private const string RackNodeKey = "frentes";

        private static JsonSerializerOptions PersistedOptions()
        {
            var options = new JsonSerializerOptions();
            PersistedBoundExpressionJson.AddConverter(options);
            return options;
        }

        /// <summary>El MISMO helper para el objetivo y para el control: escribe un arbol con el escritor persistido.</summary>
        private static string Write(BoundExpression tree) => JsonSerializer.Serialize<BoundExpression>(tree, PersistedOptions());

        /// <summary>Lee un nodo dentro del registro de variables con el lector real del almacen.</summary>
        private static ProjectVariablesReadOutcome ReadInRegistry(string node)
        {
            var json = G8ContractTestSupport.RegistryJson(G8ContractTestSupport.VariableJson(
                G8ContractTestSupport.IdA, "A", G8ContractTestSupport.ExpressionDefinition(node)));

            return new ProjectVariablesStore().Deserialize(json).Outcome;
        }

        // ================================================================ control positivo: la tabla en memoria SI conoce rack

        [Fact]
        public void INV24_TheInMemoryNamespaceTable_KnowsRack_WithItsOwnToken()
        {
            Assert.True(SymbolNamespaces.TryParseToken("rack", out var parsed));
            Assert.Equal(RackNamespace, parsed);
            Assert.Equal("rack", SymbolNamespaces.Token(RackNamespace));
            Assert.Contains(RackNamespace, SymbolNamespaces.All);
        }

        // ================================================================ lectura

        [Fact]
        public void INV24_Read_ARackNamespaceIsStructurallyUnreadable_WhileTheMemoryTableKnowsRack()
        {
            // Control en la MISMA prueba: la tabla en memoria conoce rack, asi que el rechazo es del lector persistido.
            Assert.True(SymbolNamespaces.TryParseToken("rack", out _));

            // El caso G8 existente (GUID) y el token rack real del catalogo.
            Assert.Equal(
                ProjectVariablesReadOutcome.PresentButUnreadable,
                ReadInRegistry(G8ContractTestSupport.Reference("8a1d4e77-2c93-4b60-8f15-6e0b93a7c221", "rack")));
            Assert.Equal(
                ProjectVariablesReadOutcome.PresentButUnreadable,
                ReadInRegistry(G8ContractTestSupport.Reference(RackNodeKey, "rack")));

            // Control del lector: el namespace persistible sigue legible.
            Assert.Equal(
                ProjectVariablesReadOutcome.Readable,
                ReadInRegistry(G8ContractTestSupport.Reference(G8ContractTestSupport.IdB)));
        }

        [Fact]
        public void INV24_Read_TheConverterRejectsRack_WithAJsonException_NeverBuildingATree()
        {
            Assert.True(SymbolNamespaces.TryParseToken("rack", out _));

            var node = G8ContractTestSupport.Reference(RackNodeKey, "rack");

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BoundExpression>(node, PersistedOptions()));

            // Control: el mismo lector acepta el namespace persistible.
            var tree = JsonSerializer.Deserialize<BoundExpression>(
                G8ContractTestSupport.Reference(G8ContractTestSupport.IdB), PersistedOptions());
            Assert.Equal(SymbolId.ProjectVariable(G8ContractTestSupport.IdB), Assert.IsType<BoundReference>(tree).Symbol);
        }

        // ================================================================ escritura

        [Fact]
        public void INV24_Write_ATreeWithARackReference_IsAProgrammingError_AndIsNeverWritten()
        {
            // Control con el MISMO helper: un arbol projectVariable se escribe.
            var control = Write(Add(Ref(1), Num(2)));
            Assert.Contains("\"Namespace\":\"projectVariable\"", control, StringComparison.Ordinal);

            var rackTree = BoundExpression.Reference(FrentesId);

            Assert.Throws<InvalidOperationException>(() => Write(rackTree));
            Assert.Throws<InvalidOperationException>(() => Write(Add(Num(2), rackTree)));
            Assert.Throws<InvalidOperationException>(() => Write(Call(FunctionId.Max, Ref(1), Mul(Num(2), rackTree))));
        }
    }
}
