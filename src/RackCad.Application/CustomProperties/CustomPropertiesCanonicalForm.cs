using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using RackCad.Application.Persistence;

namespace RackCad.Application.CustomProperties
{
    /// <summary>
    /// Canonical equality of writable collections (I-54 D-09.7 / ADR-0039 §8), expressed as one canonical text: two
    /// collections are equal if and only if their texts are equal.
    ///
    /// <para>
    /// The text is built from the document, never from its stored bytes. It keeps the major and drops the minor, which
    /// carries no data and would break the transitivity of «absent ≡ empty with any minor». Root and entry extension data
    /// are written in depth: object members sorted by ordinal name, arrays in order, strings by value, numbers by their
    /// raw JSON text. Each entry keeps its position, its <see cref="CustomPropertyId"/> by GUID value, and its name and
    /// value exactly. Duplicate member names cannot reach this point: the store already reads them as unreadable.
    /// </para>
    /// </summary>
    public static class CustomPropertiesCanonicalForm
    {
        /// <summary>The canonical text of an Absent or Readable collection. Any other outcome has no canonical form.</summary>
        public static string Of(CustomPropertiesReadResult collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (!collection.CanWrite)
            {
                throw new InvalidOperationException(
                    "Solo una colección Absent o Readable tiene forma canónica; " + collection.Outcome + " es de solo lectura.");
            }

            var document = collection.Document;
            var buffer = new ArrayBufferWriter<byte>();

            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                writer.WriteNumber("major", MajorOf(document.SchemaVersion));
                writer.WritePropertyName("root");
                WriteExtension(writer, document.ExtensionData);
                writer.WriteStartArray("entries");

                foreach (var entry in document.Entries)
                {
                    writer.WriteStartObject();
                    writer.WriteString("id", IdOf(entry.Id));
                    writer.WriteString("name", entry.Name);
                    writer.WriteString("value", entry.Value);
                    writer.WritePropertyName("extension");
                    WriteExtension(writer, entry.ExtensionData);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }

        public static bool AreEqual(CustomPropertiesReadResult left, CustomPropertiesReadResult right)
            => string.Equals(Of(left), Of(right), StringComparison.Ordinal);

        /// <summary>The minor of a writable collection; an Absent one has the minor of the current version (D-09.10).</summary>
        internal static int MinorOf(CustomPropertiesReadResult collection)
            => VersionPart(
                collection.Outcome == CustomPropertiesReadOutcome.Absent
                    ? CustomPropertiesDocument.CurrentSchemaVersion
                    : collection.Document.SchemaVersion,
                1);

        /// <summary>True when the document carries content this build only keeps: root or entry extension data.</summary>
        internal static bool HasExtensionData(CustomPropertiesDocument document)
            => HasAny(document.ExtensionData) || document.Entries.Any(entry => HasAny(entry.ExtensionData));

        private static int MajorOf(string version) => VersionPart(version, 0);

        /// <summary>A writable collection always has an exact <c>major.minor</c> version (D-05.5): the store guarantees it.</summary>
        private static int VersionPart(string version, int index)
            => int.Parse(version.Split('.')[index], NumberStyles.None, CultureInfo.InvariantCulture);

        private static string IdOf(string text)
        {
            if (!CustomPropertyId.TryParse(text, out var id))
            {
                throw new InvalidOperationException("Una entrada de una colección legible siempre tiene un id válido.");
            }

            return id.ToString();
        }

        private static bool HasAny(IDictionary<string, JsonElement> extension) => extension != null && extension.Count > 0;

        private static void WriteExtension(Utf8JsonWriter writer, IDictionary<string, JsonElement> extension)
        {
            writer.WriteStartObject();

            if (extension != null)
            {
                foreach (var pair in extension.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(pair.Key);
                    WriteElement(writer, pair.Value);
                }
            }

            writer.WriteEndObject();
        }

        private static void WriteElement(Utf8JsonWriter writer, JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();

                    foreach (var property in element.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal))
                    {
                        writer.WritePropertyName(property.Name);
                        WriteElement(writer, property.Value);
                    }

                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();

                    foreach (var item in element.EnumerateArray())
                    {
                        WriteElement(writer, item);
                    }

                    writer.WriteEndArray();
                    break;

                case JsonValueKind.String:
                    writer.WriteStringValue(element.GetString());
                    break;

                case JsonValueKind.Number:
                    writer.WriteRawValue(element.GetRawText());
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    writer.WriteBooleanValue(element.GetBoolean());
                    break;

                case JsonValueKind.Null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new InvalidOperationException("Un documento legible no contiene elementos JSON indefinidos.");
            }
        }
    }
}
