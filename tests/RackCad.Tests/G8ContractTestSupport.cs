using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;
using Xunit.Sdk;

namespace RackCad.Tests
{
    /// <summary>
    /// Test-only projection for the missing G8 seams. It keeps the RED suite compilable without supplying any product
    /// implementation and names a coherent adapter contract instead of embedding a second evaluator in the tests.
    /// </summary>
    internal static class G8ContractTestSupport
    {
        internal const string IdA = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        internal const string IdB = "11111111-2222-3333-4444-555555555555";
        internal const string IdC = "22222222-3333-4444-5555-666666666666";
        internal const string IdD = "33333333-4444-5555-6666-777777777777";

        internal static string RegistryJson(params string[] variables)
            => "{\"SchemaVersion\":\"1.0\",\"Variables\":[" + string.Join(",", variables) + "]}";

        internal static string VariableJson(string id, string name, string definition)
            => "{\"VariableId\":" + JsonSerializer.Serialize(id) +
               ",\"Name\":" + JsonSerializer.Serialize(name) +
               ",\"Type\":\"Length\",\"Definition\":" + definition + "}";

        internal static string LiteralDefinition(double value)
            => "{\"Kind\":\"literal\",\"Value\":" +
               value.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "}";

        internal static string ExpressionDefinition(string node)
            => "{\"Kind\":\"expression\",\"Expression\":" + node + "}";

        internal static string Number(double value, string unit = null)
            => "{\"Node\":\"number\",\"Value\":" +
               value.ToString("R", System.Globalization.CultureInfo.InvariantCulture) +
               (unit == null ? string.Empty : ",\"Unit\":" + JsonSerializer.Serialize(unit)) + "}";

        internal static string Reference(string id, string symbolNamespace = "projectVariable")
            => "{\"Node\":\"ref\",\"Namespace\":" + JsonSerializer.Serialize(symbolNamespace) +
               ",\"Id\":" + JsonSerializer.Serialize(id) + "}";

        internal static string Negate(string operand)
            => "{\"Node\":\"neg\",\"Operand\":" + operand + "}";

        internal static string Binary(string node, string left, string right)
            => "{\"Node\":" + JsonSerializer.Serialize(node) +
               ",\"Left\":" + left + ",\"Right\":" + right + "}";

        internal static string Call(string function, params string[] arguments)
            => "{\"Node\":\"call\",\"Function\":" + JsonSerializer.Serialize(function) +
               ",\"Arguments\":[" + string.Join(",", arguments) + "]}";

        internal static ProjectVariablesDocument ReadRegistry(string json)
        {
            var result = new ProjectVariablesStore().Deserialize(json);
            Assert.Equal(ProjectVariablesReadOutcome.Readable, result.Outcome);
            Assert.NotNull(result.Document);
            return result.Document;
        }

        internal static ProjectVariable ReadSingleVariable(string json)
            => Assert.Single(ReadRegistry(json).ToProjectVariables());

        internal static BoundExpression DefinitionExpression(VariableDefinition definition)
        {
            var property = typeof(VariableDefinition).GetProperty("ExpressionValue", BindingFlags.Public | BindingFlags.Instance)
                ?? typeof(VariableDefinition).GetProperty("Expression", BindingFlags.Public | BindingFlags.Instance)
                ?? throw Missing("VariableDefinition.ExpressionValue");
            return property.GetValue(definition) as BoundExpression
                ?? throw Missing("a non-null bound expression in VariableDefinition");
        }

        internal static VariableDefinition CreateExpressionDefinition(BoundExpression expression)
        {
            var method = typeof(VariableDefinition).GetMethod(
                "Expression",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(BoundExpression) },
                null) ?? throw Missing("VariableDefinition.Expression(BoundExpression)");
            return method.Invoke(null, new object[] { expression }) as VariableDefinition
                ?? throw Missing("VariableDefinition.Expression result");
        }

        internal static PropertyValue<double> CreateExpressionProperty(BoundExpression expression)
        {
            var method = typeof(PropertyValue<double>).GetMethod(
                "Expression",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(BoundExpression) },
                null) ?? throw Missing("PropertyValue<T>.Expression(BoundExpression)");
            return method.Invoke(null, new object[] { expression }) as PropertyValue<double>
                ?? throw Missing("PropertyValue<T>.Expression result");
        }

        internal static ExpressionContext Adapt(ProjectVariablesDocument document)
        {
            var assembly = typeof(ProjectVariable).Assembly;
            var adapterType = assembly.GetType("RackCad.Application.ProjectVariables.ProjectVariablesExpressionAdapter")
                ?? assembly.GetType("RackCad.Application.ProjectVariables.ProjectVariableSymbolAdapter")
                ?? throw Missing("one Project Variables to ExpressionContext adapter");
            var methods = adapterType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.ReturnType == typeof(ExpressionContext))
                .Where(method => method.GetParameters().Length == 1)
                .ToArray();
            var method = methods.SingleOrDefault()
                ?? throw Missing("one public adapter method returning ExpressionContext");
            var parameter = method.GetParameters()[0].ParameterType;
            object input = parameter == typeof(ProjectVariablesDocument)
                ? document
                : parameter.IsAssignableFrom(typeof(List<ProjectVariable>))
                    ? document.ToProjectVariables().ToList()
                    : (object)document.ToProjectVariables();
            return method.Invoke(null, new[] { input }) as ExpressionContext
                ?? throw Missing("a non-null ExpressionContext from the Project Variables adapter");
        }

        internal static RegistryEvaluation Evaluate(ProjectVariablesDocument document)
            => RegistryEvaluation.Evaluate(Adapt(document));

        internal static SelectivePropertyValueDocument ExpressionPropertyDocument(string node)
            => new SelectivePropertyValueDocument
            {
                Kind = "expression",
                ExtensionData = new Dictionary<string, JsonElement>
                {
                    ["Expression"] = JsonDocument.Parse(node).RootElement.Clone(),
                },
            };

        internal static string RootCode(RegistrySymbolResult result)
            => string.Join(",", result.RootCauses.Select(root => root.Code.ToString()));

        private static XunitException Missing(string capability)
            => new XunitException("G8 contract RED: product is missing " + capability + ".");
    }
}
