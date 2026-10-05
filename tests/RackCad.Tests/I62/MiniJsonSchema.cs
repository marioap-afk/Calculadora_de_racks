#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>
    /// Instance validation for the closed subset of JSON Schema (draft 2020-12) that <c>rackcad-clause-map/v1</c> uses: <c>type</c> (with
    /// <c>null</c>), <c>const</c>, <c>enum</c>, <c>pattern</c>, <c>minLength</c>, <c>properties</c>, <c>required</c>, <c>additionalProperties</c> =
    /// false, <c>items</c>, <c>minItems</c>, <c>uniqueItems</c>, <c>minimum</c> and <c>maximum</c>. Any other keyword fails closed, so the schema can
    /// never rely on a rule this validator would ignore. No dependency is added (decisions §43, YAML/JSON: no new dependency).
    /// </summary>
    public static class MiniJsonSchema
    {
        private static readonly HashSet<string> Annotations = new HashSet<string>(StringComparer.Ordinal) { "$schema", "$id", "title", "description", "$comment" };

        private static readonly HashSet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
        {
            "type", "const", "enum", "pattern", "minLength", "properties", "required", "additionalProperties", "items", "minItems", "uniqueItems", "minimum",
            "maximum",
        };

        public static List<string> Validate(JsonNode schema, JsonNode? instance)
        {
            var problems = new List<string>();
            Check(schema, instance, "$", problems);
            return problems;
        }

        private static void Check(JsonNode? schemaNode, JsonNode? x, string at, List<string> problems)
        {
            if (schemaNode is not JsonObject s)
            {
                problems.Add(at + ": the schema is not an object");
                return;
            }

            foreach (var key in s.Select(kv => kv.Key).Where(k => !Annotations.Contains(k) && !Supported.Contains(k)))
            {
                problems.Add(at + ": unsupported keyword " + key + " (fails closed)");
            }

            if (s["type"] is JsonNode type)
            {
                var types = type is JsonArray a ? a.Select(t => (string)t!).ToList() : new List<string> { (string)type! };
                if (!types.Any(t => Is(t, x)))
                {
                    problems.Add(at + ": not of type " + string.Join("|", types));
                    return;
                }
            }

            if (s["const"] is JsonNode c && !JsonNode.DeepEquals(c, x))
            {
                problems.Add(at + ": not the constant " + c.ToJsonString());
            }

            if (s["enum"] is JsonArray e && !e.Any(v => JsonNode.DeepEquals(v, x)))
            {
                problems.Add(at + ": not in the enumeration");
            }

            if (x is JsonValue v && v.TryGetValue<string>(out var str))
            {
                if (s["pattern"] is JsonNode p && !Regex.IsMatch(str, (string)p!))
                {
                    problems.Add(at + ": does not match " + (string)p!);
                }

                if (s["minLength"] is JsonNode ml && str.Length < (int)ml)
                {
                    problems.Add(at + ": shorter than " + (int)ml);
                }
            }

            if (TryNumber(x, out var d))
            {
                if (TryNumber(s["minimum"], out var min) && d < min)
                {
                    problems.Add(at + ": below the minimum");
                }

                if (TryNumber(s["maximum"], out var max) && d > max)
                {
                    problems.Add(at + ": above the maximum");
                }
            }

            if (x is JsonObject o)
            {
                var properties = s["properties"] as JsonObject ?? new JsonObject();
                foreach (var name in (s["required"] as JsonArray)?.Select(r => (string)r!) ?? Enumerable.Empty<string>())
                {
                    if (!o.ContainsKey(name))
                    {
                        problems.Add(at + ": missing " + name);
                    }
                }

                foreach (var (name, child) in o)
                {
                    if (properties[name] is JsonNode sub)
                    {
                        Check(sub, child, at + "." + name, problems);
                    }
                    else if (s["additionalProperties"] is JsonValue ap && ap.TryGetValue<bool>(out var allowed) && !allowed)
                    {
                        problems.Add(at + ": property " + name + " is not allowed");
                    }
                }
            }

            if (x is JsonArray arr)
            {
                if (s["minItems"] is JsonNode mi && arr.Count < (int)mi)
                {
                    problems.Add(at + ": fewer than " + (int)mi + " items");
                }

                if (s["uniqueItems"] is JsonValue u && u.TryGetValue<bool>(out var unique) && unique
                    && arr.Select(i => i?.ToJsonString()).Distinct(StringComparer.Ordinal).Count() != arr.Count)
                {
                    problems.Add(at + ": items are not unique");
                }

                if (s["items"] is JsonNode items)
                {
                    for (var i = 0; i < arr.Count; i++)
                    {
                        Check(items, arr[i], at + "[" + i + "]", problems);
                    }
                }
            }
        }

        /// <summary>A JSON number whatever its backing (parsed text, or a value created from a long, an int or a double).</summary>
        private static bool TryNumber(JsonNode? x, out double d)
        {
            d = 0;
            if (x is not JsonValue v)
            {
                return false;
            }

            if (v.TryGetValue<long>(out var l))
            {
                d = l;
                return true;
            }

            if (v.TryGetValue<int>(out var i))
            {
                d = i;
                return true;
            }

            return v.TryGetValue<double>(out d);
        }

        private static bool Is(string type, JsonNode? x) => type switch
        {
            "null" => x == null,
            "object" => x is JsonObject,
            "array" => x is JsonArray,
            "string" => x is JsonValue v && v.TryGetValue<string>(out _),
            "boolean" => x is JsonValue b && b.TryGetValue<bool>(out _),
            "integer" => TryNumber(x, out var d) && Math.Floor(d) == d,
            "number" => TryNumber(x, out _),
            _ => false,
        };
    }
}
