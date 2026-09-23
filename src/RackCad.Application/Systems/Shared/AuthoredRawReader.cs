using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared
{
    // Local to AUTH-13: no store policy, serializers, or public persistence contracts are changed.
    internal static partial class AuthoredRawReader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        internal static (string Name, JsonElement Payload) Read(RackAuthoredSibling sibling, string rackId, string kind)
        {
            if (sibling == null || string.IsNullOrWhiteSpace(sibling.SourceIdentity))
                throw Invalid("source", "missing sibling or source identity");
            if (!string.Equals(sibling.Kind, kind, StringComparison.OrdinalIgnoreCase))
                throw Invalid("source.Kind", "mixed kind");
            if (string.IsNullOrWhiteSpace(sibling.RawEnvelope) || string.IsNullOrWhiteSpace(sibling.RawDesign))
                throw Invalid("source", "missing original raw");
            using var envelope = JsonDocument.Parse(sibling.RawEnvelope);
            string id = null, envelopeKind = null, name = null, design = null;
            foreach (var member in Members(envelope.RootElement, "envelope"))
            {
                var v = member.Value;
                string path = "envelope." + member.Name;
                switch (member.Name.ToUpperInvariant())
                {
                    case "SCHEMAVERSION": Schema(v, path, false); break;
                    case "ID": String(v, path); id = v.GetString(); break;
                    case "KIND": String(v, path); envelopeKind = v.GetString(); break;
                    case "NAME": String(v, path); name = v.GetString(); break;
                    case "DESIGN": String(v, path); design = v.GetString(); break;
                    case "VIEW": String(v, path); break;
                    case "SECTION": Integer(v, path); break;
                    // ADR-0039 owns this opaque value, including its independent readability gate.
                    case "CUSTOMPROPERTIES": break;
                    default: throw Invalid(path, "unknown member");
                }
            }
            if (!string.Equals(id, rackId, StringComparison.Ordinal) ||
                !string.Equals(envelopeKind, kind, StringComparison.OrdinalIgnoreCase))
                throw Invalid("envelope", "membership or kind mismatch");
            // This is provenance of the supplied original bytes, never authored equality.
            if (!string.Equals(design, sibling.RawDesign, StringComparison.Ordinal))
                throw Invalid("envelope.Design", "raw evidence mismatch");
            using var project = JsonDocument.Parse(sibling.RawDesign);
            var root = project.RootElement;
            bool wrapper = Has(root, "Kind");
            if (!wrapper)
            {
                if (kind != RackEmbedDocument.KindCabecera) throw Invalid("project.Kind", "missing wrapper");
                RackFrameProjectDocument(root, "Header");
                return (name, root.Clone());
            }
            var expected = kind switch
            {
                RackEmbedDocument.KindDynamic => RackSystemKind.PalletFlow,
                RackEmbedDocument.KindPushBack => RackSystemKind.PushBack,
                RackEmbedDocument.KindCantilever => RackSystemKind.Cantilever,
                RackEmbedDocument.KindCabecera => RackSystemKind.Selective,
                _ => throw Invalid("Kind", "unsupported")
            };
            string slot = kind switch
            {
                RackEmbedDocument.KindDynamic => "DYNAMICSYSTEM",
                RackEmbedDocument.KindPushBack => "PUSHBACK",
                RackEmbedDocument.KindCantilever => "CANTILEVER",
                _ => "HEADER"
            };
            JsonElement payload = default;
            foreach (var member in Members(root, "project"))
            {
                var v = member.Value;
                string key = member.Name.ToUpperInvariant();
                string path = "project." + member.Name;
                if (key == "SCHEMAVERSION") { Schema(v, path, true); continue; }
                if (key == "KIND")
                {
                    if (EnumValue<RackSystemKind>(v, path) != expected) throw Invalid(path, "wrong payload kind");
                    continue;
                }
                if (key == slot) { payload = v; continue; }
                if (key is "HEADER" or "DYNAMICSYSTEM" or "SELECTIVERACK" or "FLOWBED" or "LARGUERO" or "PUSHBACK" or "CANTILEVER")
                {
                    if (v.ValueKind != JsonValueKind.Null) throw Invalid(path, "ambiguous inactive payload");
                    continue;
                }
                throw Invalid(path, "unknown member");
            }
            switch (kind)
            {
                case RackEmbedDocument.KindDynamic: DynamicRackSystemDocument(payload, slot); break;
                case RackEmbedDocument.KindPushBack: PushBackDesignDocument(payload, slot); break;
                case RackEmbedDocument.KindCantilever:
                    CantileverLineDocument(payload, slot);
                    var line = Property(payload, "Line");
                    Identity(Property(line, "Id"), "Cantilever.Line.Id");
                    break;
                case RackEmbedDocument.KindCabecera: RackFrameProjectDocument(payload, slot); break;
            }
            return (name, payload.Clone());
        }

        internal static T Decode<T>(JsonElement payload) => payload.Deserialize<T>(Options);
        internal static JsonException Invalid(string path, string reason) => new JsonException("AUTH-13 " + path + ": " + reason);
        private static IEnumerable<JsonProperty> Members(JsonElement node, string path)
        {
            if (node.ValueKind != JsonValueKind.Object) throw Invalid(path, "object required");
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var member in node.EnumerateObject())
            {
                if (!seen.Add(member.Name)) throw Invalid(path + "." + member.Name, "duplicate or case collision");
                yield return member;
            }
        }
        private static bool Has(JsonElement node, string name)
        {
            foreach (var p in Members(node, "project")) if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        private static JsonElement Property(JsonElement node, string name)
        {
            foreach (var p in Members(node, name)) if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) return p.Value;
            return default;
        }
        private static void Schema(JsonElement value, string path, bool wrapper)
        {
            if (value.ValueKind == JsonValueKind.Null) return;
            if (value.ValueKind != JsonValueKind.String)
                throw Invalid(path, "unrecognized schema");
            var version = value.GetString().Trim();
            if (!(version == "1.0" || (wrapper && version == "2.0")))
                throw Invalid(path, "unrecognized schema");
        }
        private static void Nullable(JsonElement value, string path, Action<JsonElement, string> check)
        {
            if (value.ValueKind != JsonValueKind.Null) check(value, path);
        }
        private static void Array(JsonElement value, string path, Action<JsonElement, string> check)
        {
            if (value.ValueKind == JsonValueKind.Null) return;
            if (value.ValueKind != JsonValueKind.Array) throw Invalid(path, "array required");
            int i = 0;
            foreach (var item in value.EnumerateArray()) check(item, path + "[" + i++ + "]");
        }
        private static void String(JsonElement value, string path)
        {
            if (value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)) throw Invalid(path, "string required");
        }
        private static void Integer(JsonElement value, string path)
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out _)) throw Invalid(path, "Int32 required");
        }
        private static void Number(JsonElement value, string path)
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out double n) || !double.IsFinite(n))
                throw Invalid(path, "finite number required");
        }
        private static void Boolean(JsonElement value, string path)
        {
            if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw Invalid(path, "boolean required");
        }
        private static void Identity(JsonElement value, string path)
        {
            if (value.ValueKind != JsonValueKind.String || !Guid.TryParse(value.GetString(), out var id) || id == Guid.Empty)
                throw Invalid(path, "nonempty identity required");
        }
        private static T EnumValue<T>(JsonElement value, string path) where T : struct, Enum
        {
            if (value.ValueKind is not (JsonValueKind.String or JsonValueKind.Number) ||
                !Enum.TryParse<T>(value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText(), true, out var e) ||
                !Enum.IsDefined(e)) throw Invalid(path, "unknown enum");
            return e;
        }
    }
}
