using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.Expressions;
using RackCad.Application.Units;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// The single closed-world wire authority for persisted bound expressions. It reconstructs semantic trees directly:
    /// references are identities, so reading never binds a name or parses expression source text.
    /// </summary>
    internal static class PersistedBoundExpressionJson
    {
        internal static void AddConverter(JsonSerializerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Converters.Add(new Converter());
        }

        private sealed class Converter : JsonConverter<BoundExpression>
        {
            public override BoundExpression Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                using var document = JsonDocument.ParseValue(ref reader);
                var nodeCount = 0;
                return ReadNode(document.RootElement, 1, ref nodeCount);
            }

            public override void Write(Utf8JsonWriter writer, BoundExpression value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.NodeCount > ExpressionLimits.MaxNodeCount ||
                    value.Depth > ExpressionLimits.MaxBoundExpressionDepth)
                {
                    throw new JsonException("The bound expression exceeds its normative persistence limits.");
                }

                WriteNode(writer, value);
            }

            private static BoundExpression ReadNode(JsonElement element, int depth, ref int nodeCount)
            {
                nodeCount++;
                if (nodeCount > ExpressionLimits.MaxNodeCount ||
                    depth > ExpressionLimits.MaxBoundExpressionDepth)
                {
                    throw new JsonException("The persisted bound expression exceeds its normative limits.");
                }

                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("A persisted bound-expression node must be an object.");
                }

                var fields = Fields(element);
                var nodeToken = RequiredString(fields, "Node");

                if (!BoundExpressionTokens.TryParseNodeToken(nodeToken, out var kind, out var binaryOperator))
                {
                    throw new JsonException("Unknown persisted bound-expression node '" + nodeToken + "'.");
                }

                switch (kind)
                {
                    case BoundExpressionKind.Number:
                        RequireOnly(fields, "Node", "Value", "Unit");
                        var value = RequiredFiniteDouble(fields, "Value");
                        LengthUnit? unit = null;
                        if (fields.TryGetValue("Unit", out var unitElement))
                        {
                            var token = String(unitElement, "Unit");
                            if (!LengthUnits.Authority.TryParseToken(token, out var parsedUnit))
                            {
                                throw new JsonException("Unknown persisted length unit '" + token + "'.");
                            }

                            unit = parsedUnit;
                        }

                        return BoundExpression.Number(value, unit);

                    case BoundExpressionKind.Reference:
                        RequireOnly(fields, "Node", "Namespace", "Id");
                        var namespaceToken = RequiredString(fields, "Namespace");
                        if (!SymbolNamespaces.TryParseToken(namespaceToken, out var symbolNamespace))
                        {
                            throw new JsonException("Unknown persisted symbol namespace '" + namespaceToken + "'.");
                        }

                        var key = RequiredString(fields, "Id");
                        try
                        {
                            return BoundExpression.Reference(new SymbolId(symbolNamespace, key));
                        }
                        catch (ArgumentException ex)
                        {
                            throw new JsonException("Malformed persisted symbol identity.", ex);
                        }

                    case BoundExpressionKind.Negate:
                        RequireOnly(fields, "Node", "Operand");
                        return BoundExpression.Negate(ReadNode(Required(fields, "Operand"), depth + 1, ref nodeCount));

                    case BoundExpressionKind.Binary:
                        RequireOnly(fields, "Node", "Left", "Right");
                        return BoundExpression.Binary(
                            binaryOperator.Value,
                            ReadNode(Required(fields, "Left"), depth + 1, ref nodeCount),
                            ReadNode(Required(fields, "Right"), depth + 1, ref nodeCount));

                    case BoundExpressionKind.Call:
                        RequireOnly(fields, "Node", "Function", "Arguments");
                        var functionToken = RequiredString(fields, "Function");
                        if (!FunctionRegistry.Productive.TryParseToken(functionToken, out var function))
                        {
                            throw new JsonException("Unknown persisted function '" + functionToken + "'.");
                        }

                        var argumentsElement = Required(fields, "Arguments");
                        if (argumentsElement.ValueKind != JsonValueKind.Array)
                        {
                            throw new JsonException("Persisted call arguments must be an array.");
                        }

                        if (argumentsElement.GetArrayLength() > ExpressionLimits.MaxArgumentCount)
                        {
                            throw new JsonException("The persisted call exceeds the normative argument limit.");
                        }

                        var arguments = new List<BoundExpression>();
                        foreach (var argument in argumentsElement.EnumerateArray())
                        {
                            arguments.Add(ReadNode(argument, depth + 1, ref nodeCount));
                        }

                        return BoundExpression.Call(function, arguments);

                    default:
                        throw new JsonException("Unknown persisted bound-expression kind.");
                }
            }

            private static void WriteNode(Utf8JsonWriter writer, BoundExpression expression)
            {
                writer.WriteStartObject();
                writer.WriteString("Node", BoundExpressionTokens.NodeToken(expression));

                switch (expression)
                {
                    case BoundNumber number:
                        writer.WriteNumber("Value", number.Value);
                        if (number.Unit.HasValue)
                        {
                            writer.WriteString("Unit", LengthUnits.Authority.Token(number.Unit.Value));
                        }

                        break;

                    case BoundReference reference:
                        writer.WriteString("Namespace", SymbolNamespaces.Token(reference.Symbol.Namespace));
                        writer.WriteString("Id", reference.Symbol.Key);
                        break;

                    case BoundNegate negate:
                        writer.WritePropertyName("Operand");
                        WriteNode(writer, negate.Operand);
                        break;

                    case BoundBinary binary:
                        writer.WritePropertyName("Left");
                        WriteNode(writer, binary.Left);
                        writer.WritePropertyName("Right");
                        WriteNode(writer, binary.Right);
                        break;

                    case BoundCall call:
                        writer.WriteString("Function", FunctionRegistry.Productive.Token(call.Function));
                        writer.WritePropertyName("Arguments");
                        writer.WriteStartArray();
                        foreach (var argument in call.Arguments)
                        {
                            WriteNode(writer, argument);
                        }

                        writer.WriteEndArray();
                        break;

                    default:
                        throw new JsonException("Unknown bound-expression node.");
                }

                writer.WriteEndObject();
            }

            private static Dictionary<string, JsonElement> Fields(JsonElement element)
            {
                var fields = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject())
                {
                    if (!fields.TryAdd(property.Name, property.Value))
                    {
                        throw new JsonException("Duplicate persisted expression field '" + property.Name + "'.");
                    }
                }

                return fields;
            }

            private static void RequireOnly(Dictionary<string, JsonElement> fields, params string[] allowed)
            {
                var names = new HashSet<string>(allowed, StringComparer.Ordinal);
                foreach (var name in fields.Keys)
                {
                    if (!names.Contains(name))
                    {
                        throw new JsonException("Unknown persisted expression field '" + name + "'.");
                    }
                }
            }

            private static JsonElement Required(Dictionary<string, JsonElement> fields, string name)
                => fields.TryGetValue(name, out var value)
                    ? value
                    : throw new JsonException("Missing persisted expression field '" + name + "'.");

            private static string RequiredString(Dictionary<string, JsonElement> fields, string name)
                => String(Required(fields, name), name);

            private static string String(JsonElement element, string name)
                => element.ValueKind == JsonValueKind.String
                    ? element.GetString()
                    : throw new JsonException("Persisted expression field '" + name + "' must be a string.");

            private static double RequiredFiniteDouble(Dictionary<string, JsonElement> fields, string name)
            {
                var element = Required(fields, name);
                if (element.ValueKind != JsonValueKind.Number || !element.TryGetDouble(out var value) ||
                    double.IsNaN(value) || double.IsInfinity(value))
                {
                    throw new JsonException("Persisted expression field '" + name + "' must be a finite number.");
                }

                return value;
            }
        }
    }
}
