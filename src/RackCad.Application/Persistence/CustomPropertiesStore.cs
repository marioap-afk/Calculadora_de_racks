using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RackCad.Application.CustomProperties;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Reads and writes collections of custom properties (I-54 D-01, D-02, D-04, D-05, D-07.3, D-07.4, D-08.2 /
    /// ADR-0039 §1-§5, §10). PURE: the Plugin contributes only the physical access, so every judgement about a collection
    /// is made here, where the Core suite reaches it.
    ///
    /// <para>
    /// <b>No external content makes this store throw.</b> Every input ends in a <see cref="CustomPropertiesReadResult"/>.
    /// There is no <c>catch (Exception)</c>: each call that external content can break catches exactly the classes that
    /// content provokes in this pipeline, and nothing else —
    /// </para>
    /// <list type="bullet">
    /// <item><c>JsonDocument.Parse</c> with the constant <see cref="ParseOptions"/>: <see cref="JsonException"/> for invalid
    /// JSON or more than 64 levels, and <see cref="ArgumentException"/> for text that cannot be transcoded (a raw lone
    /// surrogate). The options are valid constants, so an <see cref="ArgumentException"/> there can only come from the text.</item>
    /// <item>Decoding a member name or a string: <see cref="InvalidOperationException"/> for an ESCAPED lone surrogate. Each
    /// call is made only after checking the value kind, so the exception can only come from the content.</item>
    /// </list>
    /// <para>
    /// The classification order is fixed (D-07.4): whole tree and version → major → structure → depth → identity →
    /// readable. The whole tree is validated in one walk BEFORE anything is mapped, so no dictionary ever chooses between
    /// two members with the same name, and after that walk no name or string of the document can throw when decoded.
    /// </para>
    /// </summary>
    public sealed class CustomPropertiesStore
    {
        private const string SchemaVersionMember = "SchemaVersion";
        private const string EntriesMember = "Entries";
        private const string IdMember = "Id";
        private const string NameMember = "Name";
        private const string ValueMember = "Value";

        private static readonly string[] RootMembers = { SchemaVersionMember, EntriesMember };
        private static readonly string[] EntryMembers = { IdMember, NameMember, ValueMember };

        /// <summary>
        /// The only options the Project text is ever parsed with: strict JSON (no comments, no trailing commas) and the
        /// platform's 64-level limit, stated rather than defaulted. A test pins them.
        /// </summary>
        internal static readonly JsonDocumentOptions ParseOptions = new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 64,
        };

        // ================================================================== read

        /// <summary>
        /// The Project collection, from what the drawing dictionary physically held (D-07). A null payload is NOT absent:
        /// "I was not told" is no evidence that the drawing has no collection, and treating it as empty would let the next
        /// write replace one.
        /// </summary>
        public CustomPropertiesReadResult Read(CustomPropertiesPayload payload)
        {
            if (payload == null)
            {
                return Unreadable("no se pudo determinar su estado en el dibujo");
            }

            switch (payload.State)
            {
                case CustomPropertiesPayloadState.Absent:
                    return CustomPropertiesReadResult.Absent();

                case CustomPropertiesPayloadState.PresentButUnreadable:
                    return Unreadable(payload.Error ?? "la entrada del dibujo existe pero no contiene texto utilizable");

                case CustomPropertiesPayloadState.Present:
                    return ReadText(payload.Text);

                default:
                    return Unreadable("el estado físico de la entrada no es reconocible");
            }
        }

        /// <summary>
        /// The Rack collection, from the envelope member already parsed (D-08.2). No member, <c>null</c>, or an undefined
        /// element are ABSENT: that is what an envelope without properties looks like. A value that is not an object is
        /// present and unreadable.
        /// </summary>
        public CustomPropertiesReadResult ReadElement(JsonElement? element)
        {
            if (!element.HasValue)
            {
                return CustomPropertiesReadResult.Absent();
            }

            switch (element.Value.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return CustomPropertiesReadResult.Absent();

                case JsonValueKind.Object:
                    return Classify(element.Value);

                default:
                    return Unreadable("el contenido no es un objeto JSON");
            }
        }

        private static CustomPropertiesReadResult ReadText(string text)
        {
            if (text == null)
            {
                return Unreadable("la entrada del dibujo existe pero no contiene texto");
            }

            JsonDocument document;

            try
            {
                document = JsonDocument.Parse(text, ParseOptions);
            }
            catch (JsonException ex)
            {
                return Unreadable("el texto no es un JSON válido (" + ex.Message + ")");
            }
            catch (ArgumentException)
            {
                return Unreadable("el texto contiene UTF-16 que no se puede transcodificar");
            }

            using (document)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return Unreadable("la raíz no es un objeto JSON");
                }

                return Classify(document.RootElement);
            }
        }

        private static CustomPropertiesReadResult Classify(JsonElement root)
        {
            // 1. The whole tree, before anything is mapped: repeated member names and undecodable strings at any depth
            //    (C-3, C-4). The same walk measures the depth that step 4 judges.
            var depth = 0;
            var treeProblem = ValidateTree(root, 1, ref depth);

            if (treeProblem != null)
            {
                return Unreadable(treeProblem);
            }

            if (!TryGetMember(root, SchemaVersionMember, out var versionElement))
            {
                return Unreadable("no declaran SchemaVersion");
            }

            if (versionElement.ValueKind != JsonValueKind.String)
            {
                return Unreadable("SchemaVersion no es un texto");
            }

            var version = versionElement.GetString();

            if (!TryParseVersion(version, out var major))
            {
                return Unreadable("SchemaVersion no tiene la forma exacta major.minor ('" + version + "')");
            }

            // 2. Version first. The depth limit belongs to major 1, so a newer major is judged before it.
            if (major > CustomPropertiesDocument.SupportedMajor)
            {
                return CustomPropertiesReadResult.IncompatibleMajor(
                    "Las propiedades personalizadas se escribieron con una versión más nueva de RackCad (esquema " +
                    version + "); se conservan intactas y quedan en solo lectura.");
            }

            // 3. Structure.
            if (!TryGetMember(root, EntriesMember, out var entriesElement) || entriesElement.ValueKind != JsonValueKind.Array)
            {
                return Unreadable("Entries falta, es nulo o no es un array");
            }

            var entries = new List<CustomPropertyEntryDocument>();
            var ids = new List<CustomPropertyId>();

            foreach (var item in entriesElement.EnumerateArray())
            {
                var entryProblem = ReadEntry(item, out var entry, out var id);

                if (entryProblem != null)
                {
                    return Unreadable(entryProblem);
                }

                entries.Add(entry);
                ids.Add(id);
            }

            // 4. Depth: a constant of the 1.x format, and a read-only state of its own (D-05.7).
            if (depth > CustomPropertiesDocument.MaxDepth)
            {
                return CustomPropertiesReadResult.DepthLimitExceeded(
                    "Las propiedades personalizadas tienen " + depth + " niveles de profundidad y el formato 1.x admite " +
                    CustomPropertiesDocument.MaxDepth + "; se conservan intactas y quedan en solo lectura.");
            }

            // 5. Identity by GUID value (D-02.6).
            var seen = new HashSet<CustomPropertyId>();

            foreach (var id in ids)
            {
                if (!seen.Add(id))
                {
                    return CustomPropertiesReadResult.AmbiguousIdentity(
                        "Dos propiedades personalizadas comparten el id " + id +
                        "; la identidad es ambigua y la colección queda en solo lectura.");
                }
            }

            // 6. Readable, with the diagnostic of repeated names.
            var document = new CustomPropertiesDocument
            {
                SchemaVersion = version,
                Entries = entries,
                ExtensionData = CollectExtension(root, RootMembers),
            };

            return CustomPropertiesReadResult.Readable(document, RepeatedNameEntryIds(entries, ids));
        }

        /// <summary>
        /// One walk over objects and arrays. Returns the first reason the tree is unreadable, or null, and records the
        /// deepest container level (the root counts 1).
        /// </summary>
        private static string ValidateTree(JsonElement element, int level, ref int depth)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                {
                    depth = Math.Max(depth, level);

                    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var property in element.EnumerateObject())
                    {
                        string name;

                        try
                        {
                            name = property.Name;
                        }
                        catch (InvalidOperationException)
                        {
                            return "un nombre de miembro no se puede decodificar como UTF-16";
                        }

                        if (!names.Add(name))
                        {
                            return "un objeto repite el miembro '" + name + "' (sin distinguir mayúsculas)";
                        }

                        var nested = ValidateTree(property.Value, level + 1, ref depth);

                        if (nested != null)
                        {
                            return nested;
                        }
                    }

                    return null;
                }

                case JsonValueKind.Array:
                {
                    depth = Math.Max(depth, level);

                    foreach (var item in element.EnumerateArray())
                    {
                        var nested = ValidateTree(item, level + 1, ref depth);

                        if (nested != null)
                        {
                            return nested;
                        }
                    }

                    return null;
                }

                case JsonValueKind.String:
                {
                    try
                    {
                        element.GetString();
                    }
                    catch (InvalidOperationException)
                    {
                        return "un texto no se puede decodificar como UTF-16";
                    }

                    return null;
                }

                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads one entry of a tree that <see cref="ValidateTree"/> already accepted. Declared members match without case;
        /// every other member goes, cloned, to the entry's extension data.
        /// </summary>
        private static string ReadEntry(JsonElement item, out CustomPropertyEntryDocument entry, out CustomPropertyId id)
        {
            entry = null;
            id = default;

            if (item.ValueKind != JsonValueKind.Object)
            {
                return "una entrada de Entries no es un objeto";
            }

            JsonElement? idElement = null;
            JsonElement? nameElement = null;
            JsonElement? valueElement = null;
            Dictionary<string, JsonElement> extension = null;

            foreach (var property in item.EnumerateObject())
            {
                if (IsMember(property.Name, IdMember))
                {
                    idElement = property.Value;
                }
                else if (IsMember(property.Name, NameMember))
                {
                    nameElement = property.Value;
                }
                else if (IsMember(property.Name, ValueMember))
                {
                    valueElement = property.Value;
                }
                else
                {
                    extension = extension ?? new Dictionary<string, JsonElement>();
                    extension.Add(property.Name, property.Value.Clone());
                }
            }

            if (idElement == null || idElement.Value.ValueKind != JsonValueKind.String)
            {
                return "una entrada no declara Id como texto";
            }

            var idText = idElement.Value.GetString();

            if (!CustomPropertyId.TryParse(idText, out id))
            {
                return "una entrada declara un Id que no es un GUID en forma D exacta ('" + idText + "')";
            }

            if (nameElement == null || nameElement.Value.ValueKind != JsonValueKind.String)
            {
                return "la propiedad " + id + " no declara Name como texto";
            }

            if (valueElement == null || valueElement.Value.ValueKind != JsonValueKind.String)
            {
                return "la propiedad " + id + " no declara Value como texto";
            }

            var name = nameElement.Value.GetString();

            // Accreditation of a read name (D-05.2, C-F1): before any normalization, and never normalized here.
            if (CustomPropertyText.Scan(name) != CustomPropertyTextScan.WellFormed)
            {
                return "la propiedad " + id + " tiene un Name con un no-carácter Unicode o con UTF-16 mal formado";
            }

            if (name.Trim().Length == 0)
            {
                return "la propiedad " + id + " tiene un Name vacío";
            }

            entry = new CustomPropertyEntryDocument
            {
                Id = idText,
                Name = name,
                Value = valueElement.Value.GetString(),
                ExtensionData = extension,
            };

            return null;
        }

        private static Dictionary<string, JsonElement> CollectExtension(JsonElement owner, string[] declared)
        {
            Dictionary<string, JsonElement> extension = null;

            foreach (var property in owner.EnumerateObject())
            {
                if (Array.Exists(declared, member => IsMember(property.Name, member)))
                {
                    continue;
                }

                extension = extension ?? new Dictionary<string, JsonElement>();
                extension.Add(property.Name, property.Value.Clone());
            }

            return extension;
        }

        /// <summary>The ids of the entries whose accredited names repeat, in NFC and without case, in document order.</summary>
        private static IReadOnlyList<CustomPropertyId> RepeatedNameEntryIds(
            IReadOnlyList<CustomPropertyEntryDocument> entries,
            IReadOnlyList<CustomPropertyId> ids)
        {
            var keys = new string[entries.Count];
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < entries.Count; i++)
            {
                keys[i] = CustomPropertyText.ComparableName(entries[i].Name);
                counts[keys[i]] = counts.TryGetValue(keys[i], out var count) ? count + 1 : 1;
            }

            var repeated = new List<CustomPropertyId>();

            for (var i = 0; i < entries.Count; i++)
            {
                if (counts[keys[i]] > 1)
                {
                    repeated.Add(ids[i]);
                }
            }

            return repeated;
        }

        private static bool TryGetMember(JsonElement owner, string member, out JsonElement value)
        {
            foreach (var property in owner.EnumerateObject())
            {
                if (IsMember(property.Name, member))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool IsMember(string name, string member) => string.Equals(name, member, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// The exact version shape (D-01.1): two runs of ASCII digits separated by one dot, no spaces, no sign, and a major
        /// of at least 1. Deliberately stricter than <see cref="SchemaVersionPolicy"/>, whose tolerance of legacy versions
        /// is right for the historical documents and wrong for a format that always writes its version.
        /// </summary>
        private static bool TryParseVersion(string version, out int major)
        {
            major = 0;

            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            var dot = version.IndexOf('.');

            if (dot <= 0 || dot == version.Length - 1 || version.IndexOf('.', dot + 1) >= 0)
            {
                return false;
            }

            return TryParseDigits(version, 0, dot, out major) &&
                   TryParseDigits(version, dot + 1, version.Length - dot - 1, out _) &&
                   major >= 1;
        }

        private static bool TryParseDigits(string text, int start, int length, out int value)
        {
            value = 0;

            for (var i = start; i < start + length; i++)
            {
                var digit = text[i] - '0';

                if (digit < 0 || digit > 9 || value > (int.MaxValue - digit) / 10)
                {
                    value = 0;
                    return false;
                }

                value = (value * 10) + digit;
            }

            return length > 0;
        }

        private static CustomPropertiesReadResult Unreadable(string reason)
            => CustomPropertiesReadResult.PresentButUnreadable(
                "Las propiedades personalizadas no se pueden leer: " + reason +
                ". Se conservan intactas y quedan en solo lectura.");

        // ================================================================== write

        /// <summary>
        /// Writes a collection. <b>Every 1.x writer always emits <c>SchemaVersion</c> and <c>Entries</c></b> (D-01.4); ids go
        /// out in lower-case <c>D</c> form (D-02.3); the version is resolved without ever downgrading a newer minor; and the
        /// output is ASCII-only, because the default encoder escapes everything else — a chunked Xrecord then never splits a
        /// surrogate pair.
        ///
        /// <para>
        /// It refuses, as a programming error, any document it would not read back as writable (INV-20): an unreadable
        /// version or a newer major, missing entries, an invalid or repeated id, a name that is empty, malformed or has a
        /// noncharacter, a malformed value, extension data that collides with a declared member or repeats a key, and more
        /// than <see cref="CustomPropertiesDocument.MaxDepth"/> levels. Refusing a malformed string matters in particular:
        /// the writer would otherwise replace a lone surrogate with U+FFFD, silently.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The document is not one this store could read back as writable.</exception>
        public string Serialize(CustomPropertiesDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (!TryParseVersion(document.SchemaVersion, out var major))
            {
                throw Invalid("SchemaVersion no tiene la forma exacta major.minor ('" + (document.SchemaVersion ?? "<null>") + "')");
            }

            if (major > CustomPropertiesDocument.SupportedMajor)
            {
                throw Invalid("fueron escritas con una versión más nueva de RackCad (esquema " + document.SchemaVersion + ") y no se sobrescriben");
            }

            if (document.Entries == null)
            {
                throw Invalid("Entries es obligatorio");
            }

            var depth = 2;
            ValidateExtensionForWrite(document.ExtensionData, RootMembers, 1, ref depth);

            var ids = new string[document.Entries.Count];
            var seen = new HashSet<CustomPropertyId>();

            for (var i = 0; i < document.Entries.Count; i++)
            {
                var entry = document.Entries[i];

                if (entry == null)
                {
                    throw Invalid("una entrada es nula");
                }

                if (!CustomPropertyId.TryParse(entry.Id, out var id))
                {
                    throw Invalid("una entrada tiene un Id que no es un GUID en forma D exacta");
                }

                if (!seen.Add(id))
                {
                    throw Invalid("dos entradas comparten el id " + id);
                }

                if (entry.Name == null ||
                    CustomPropertyText.Scan(entry.Name) != CustomPropertyTextScan.WellFormed ||
                    entry.Name.Trim().Length == 0)
                {
                    throw Invalid("la propiedad " + id + " tiene un Name nulo, vacío, con un no-carácter o con UTF-16 mal formado");
                }

                if (entry.Value == null || CustomPropertyText.Scan(entry.Value) == CustomPropertyTextScan.MalformedUtf16)
                {
                    throw Invalid("la propiedad " + id + " tiene un Value nulo o con UTF-16 mal formado");
                }

                depth = Math.Max(depth, 3);
                ValidateExtensionForWrite(entry.ExtensionData, EntryMembers, 3, ref depth);
                ids[i] = id.ToString();
            }

            if (depth > CustomPropertiesDocument.MaxDepth)
            {
                throw Invalid("tendrían " + depth + " niveles de profundidad y el formato 1.x admite " + CustomPropertiesDocument.MaxDepth);
            }

            var version = SchemaVersionPolicy.ResolveWriteVersion(document.SchemaVersion, CustomPropertiesDocument.CurrentSchemaVersion);
            var buffer = new ArrayBufferWriter<byte>();

            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                writer.WriteString(SchemaVersionMember, version);
                writer.WritePropertyName(EntriesMember);
                writer.WriteStartArray();

                for (var i = 0; i < document.Entries.Count; i++)
                {
                    var entry = document.Entries[i];

                    writer.WriteStartObject();
                    writer.WriteString(IdMember, ids[i]);
                    writer.WriteString(NameMember, entry.Name);
                    writer.WriteString(ValueMember, entry.Value);
                    WriteExtension(writer, entry.ExtensionData);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                WriteExtension(writer, document.ExtensionData);
                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }

        private static void ValidateExtensionForWrite(
            IDictionary<string, JsonElement> extension,
            string[] declared,
            int ownerLevel,
            ref int depth)
        {
            if (extension == null)
            {
                return;
            }

            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in extension)
            {
                if (Array.Exists(declared, member => IsMember(pair.Key, member)))
                {
                    throw Invalid("ExtensionData colisiona con el miembro declarado '" + pair.Key + "'");
                }

                if (!keys.Add(pair.Key) || CustomPropertyText.Scan(pair.Key) == CustomPropertyTextScan.MalformedUtf16)
                {
                    throw Invalid("ExtensionData repite la clave '" + pair.Key + "' sin distinguir mayúsculas o la tiene mal formada");
                }

                if (pair.Value.ValueKind == JsonValueKind.Undefined)
                {
                    throw Invalid("ExtensionData contiene un elemento sin valor en '" + pair.Key + "'");
                }

                var problem = ValidateTree(pair.Value, ownerLevel + 1, ref depth);

                if (problem != null)
                {
                    throw Invalid("ExtensionData no es releíble: " + problem);
                }
            }
        }

        private static void WriteExtension(Utf8JsonWriter writer, IDictionary<string, JsonElement> extension)
        {
            if (extension == null)
            {
                return;
            }

            foreach (var pair in extension)
            {
                writer.WritePropertyName(pair.Key);
                pair.Value.WriteTo(writer);
            }
        }

        private static InvalidOperationException Invalid(string reason)
            => new InvalidOperationException("Las propiedades personalizadas no se pueden escribir: " + reason + ".");
    }
}
