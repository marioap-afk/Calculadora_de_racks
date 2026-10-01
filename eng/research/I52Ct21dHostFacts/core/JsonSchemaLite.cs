using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace I52Ct21d.HostFacts.Core;

public sealed class SchemaUnsupportedException : Exception
{
    public SchemaUnsupportedException(string message) : base(message) { }
}

/// <summary>
/// A small, closed validator for the JSON Schema (2020-12) SUBSET that the schemas of this folder use. It is deliberately not a
/// general validator: a keyword outside <see cref="Supported"/> makes the schema unusable (fail closed) instead of being ignored, so a
/// schema can never silently validate less than it says.
/// Supported: type, const, enum, required, properties, additionalProperties (false or true), items, minItems, maxItems, minimum,
/// maximum, minLength, maxLength, pattern, oneOf, allOf, if/then/else, $ref (to "#/$defs/name" only), $defs, and the annotations
/// $schema, $id, title, description, format (annotation only; no format is asserted).
/// </summary>
public sealed class JsonSchemaLite
{
    private static readonly HashSet<string> Supported = new(StringComparer.Ordinal)
    {
        "$schema", "$id", "$defs", "$ref", "title", "description", "format", "type", "const", "enum", "required", "properties",
        "additionalProperties", "items", "minItems", "maxItems", "minimum", "maximum", "minLength", "maxLength", "pattern",
        "oneOf", "allOf", "if", "then", "else",
    };

    private readonly JsonObject _root;

    public JsonSchemaLite(JsonObject schema)
    {
        _root = schema;
        CheckKeywords(schema, "#");
    }

    /// <summary>Loads an embedded schema of this assembly by its file name (for example <c>ct21d.hostfact.v1.json</c>).</summary>
    public static JsonSchemaLite LoadEmbedded(string fileName)
    {
        var asm = typeof(JsonSchemaLite).Assembly;
        using var stream = asm.GetManifestResourceStream("schemas/" + fileName)
            ?? throw new FileNotFoundException("embedded schema not found: " + fileName);
        using var reader = new StreamReader(stream, new System.Text.UTF8Encoding(false, true));
        return new JsonSchemaLite((JsonObject)Jcs.ParseStrict(reader.ReadToEnd()));
    }

    public IReadOnlyList<string> Validate(JsonNode? instance)
    {
        var errors = new List<string>();
        ValidateNode(_root, instance, "", errors);
        return errors;
    }

    private static void CheckKeywords(JsonNode? node, string path)
    {
        if (node is not JsonObject obj) return;
        foreach (var (key, value) in obj)
        {
            if (!Supported.Contains(key))
                throw new SchemaUnsupportedException("unsupported schema keyword '" + key + "' at " + path);
            switch (key)
            {
                case "properties":
                case "$defs":
                    if (value is JsonObject members)
                        foreach (var (name, sub) in members) CheckKeywords(sub, path + "/" + key + "/" + name);
                    break;
                case "items":
                case "if":
                case "then":
                case "else":
                case "additionalProperties":
                    CheckKeywords(value, path + "/" + key);
                    break;
                case "oneOf":
                case "allOf":
                    if (value is JsonArray arr)
                        for (var i = 0; i < arr.Count; i++) CheckKeywords(arr[i], path + "/" + key + "/" + i);
                    break;
            }
        }
    }

    private JsonObject Resolve(string reference)
    {
        const string prefix = "#/$defs/";
        if (!reference.StartsWith(prefix, StringComparison.Ordinal))
            throw new SchemaUnsupportedException("only #/$defs/ references are supported: " + reference);
        var defs = _root["$defs"] as JsonObject ?? throw new SchemaUnsupportedException("no $defs");
        return defs[reference[prefix.Length..]] as JsonObject ?? throw new SchemaUnsupportedException("unresolved reference " + reference);
    }

    private void ValidateNode(JsonNode schemaNode, JsonNode? instance, string path, List<string> errors)
    {
        if (schemaNode is JsonValue sv && sv.TryGetValue<bool>(out var allow))
        {
            if (!allow) errors.Add(path + ": not allowed");
            return;
        }
        var schema = (JsonObject)schemaNode;
        if (schema.TryGetPropertyValue("$ref", out var r) && r is not null)
            ValidateNode(Resolve(r.GetValue<string>()), instance, path, errors);

        if (schema["type"] is JsonNode t && !TypeMatches(t.GetValue<string>(), instance))
        {
            errors.Add($"{path}: expected type {t.GetValue<string>()}");
            return; // the remaining keywords assume the type
        }
        if (schema.TryGetPropertyValue("const", out var c) && !JsonEquals(c, instance))
            errors.Add($"{path}: expected const {Jcs.Serialize(c)}");
        if (schema["enum"] is JsonArray en && !en.Any(e => JsonEquals(e, instance)))
            errors.Add($"{path}: not one of {Jcs.Serialize(en)}");

        if (instance is JsonValue iv)
        {
            if (iv.TryGetValue<string>(out var str))
            {
                if (schema["pattern"] is JsonNode pat && !Regex.IsMatch(str, pat.GetValue<string>(), RegexOptions.CultureInvariant))
                    errors.Add($"{path}: does not match pattern {pat.GetValue<string>()}");
                if (schema["minLength"] is JsonNode ml && str.Length < ml.GetValue<int>()) errors.Add($"{path}: shorter than {ml.GetValue<int>()}");
                if (schema["maxLength"] is JsonNode xl && str.Length > xl.GetValue<int>()) errors.Add($"{path}: longer than {xl.GetValue<int>()}");
            }
            if (TryGetInteger(iv, out var num))
            {
                if (schema["minimum"] is JsonNode mn && num < mn.GetValue<long>()) errors.Add($"{path}: below minimum {mn.GetValue<long>()}");
                if (schema["maximum"] is JsonNode mx && num > mx.GetValue<long>()) errors.Add($"{path}: above maximum {mx.GetValue<long>()}");
            }
        }

        if (instance is JsonObject obj)
        {
            if (schema["required"] is JsonArray req)
                foreach (var name in req)
                    if (!obj.ContainsKey(name!.GetValue<string>())) errors.Add($"{path}: missing required '{name.GetValue<string>()}'");
            var props = schema["properties"] as JsonObject;
            foreach (var (key, value) in obj)
            {
                if (props is not null && props.TryGetPropertyValue(key, out var sub) && sub is not null)
                    ValidateNode(sub, value, path + "/" + key, errors);
                else if (schema.TryGetPropertyValue("additionalProperties", out var ap) && ap is not null)
                    ValidateNode(ap, value, path + "/" + key, errors);
            }
        }

        if (instance is JsonArray arr)
        {
            if (schema["minItems"] is JsonNode mi && arr.Count < mi.GetValue<int>()) errors.Add($"{path}: fewer than {mi.GetValue<int>()} items");
            if (schema["maxItems"] is JsonNode ma && arr.Count > ma.GetValue<int>()) errors.Add($"{path}: more than {ma.GetValue<int>()} items");
            if (schema["items"] is JsonNode items)
                for (var i = 0; i < arr.Count; i++) ValidateNode(items, arr[i], path + "/" + i, errors);
        }

        if (schema["oneOf"] is JsonArray one)
        {
            var matches = 0;
            foreach (var alt in one)
            {
                var sub = new List<string>();
                ValidateNode(alt!, instance, path, sub);
                if (sub.Count == 0) matches++;
            }
            if (matches != 1) errors.Add($"{path}: matches {matches} alternatives of oneOf (exactly 1 required)");
        }
        if (schema["allOf"] is JsonArray all)
            foreach (var part in all) ValidateNode(part!, instance, path, errors);
        if (schema["if"] is JsonNode cond)
        {
            var probe = new List<string>();
            ValidateNode(cond, instance, path, probe);
            if (probe.Count == 0)
            {
                if (schema["then"] is JsonNode then) ValidateNode(then, instance, path, errors);
            }
            else if (schema["else"] is JsonNode els) ValidateNode(els, instance, path, errors);
        }
    }

    private static bool TryGetInteger(JsonValue v, out long value)
    {
        value = 0;
        if (v.TryGetValue<JsonElement>(out var el)) return el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out value);
        if (v.TryGetValue<long>(out value)) return true;
        if (v.TryGetValue<int>(out var i)) { value = i; return true; }
        value = 0;
        return false;
    }

    private static bool TypeMatches(string type, JsonNode? n)
    {
        return type switch
        {
            "object" => n is JsonObject,
            "array" => n is JsonArray,
            "string" => n is JsonValue v && v.GetValueKind() == JsonValueKind.String,
            "boolean" => n is JsonValue b && b.GetValueKind() is JsonValueKind.True or JsonValueKind.False,
            "null" => n is null,
            "integer" => n is JsonValue iv && iv.GetValueKind() == JsonValueKind.Number && TryGetInteger(iv, out _),
            "number" => n is JsonValue nv && nv.GetValueKind() == JsonValueKind.Number,
            _ => throw new SchemaUnsupportedException("unsupported type " + type),
        };
    }

    private static bool JsonEquals(JsonNode? a, JsonNode? b)
    {
        if (a is null || b is null) return a is null && b is null;
        return Jcs.Serialize(a) == Jcs.Serialize(b);
    }
}
