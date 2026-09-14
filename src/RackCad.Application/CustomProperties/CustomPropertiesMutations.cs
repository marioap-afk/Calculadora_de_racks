using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;

namespace RackCad.Application.CustomProperties
{
    /// <summary>Why a mutation was refused. <see cref="None"/> only on success.</summary>
    public enum CustomPropertiesRejection
    {
        None = 0,

        /// <summary>The last read was not Absent or Readable, or there was no read at all.</summary>
        NotWritable = 1,

        /// <summary>No entry has the requested id. The name is never tried instead.</summary>
        EntryNotFound = 2,

        NameMissing = 3,

        /// <summary>The name has an unpaired surrogate. Rejected before anything else looks at it.</summary>
        NameMalformedUtf16 = 4,

        /// <summary>The name contains a Unicode noncharacter. Rejected before NFC (C-F1).</summary>
        NameNoncharacter = 5,

        /// <summary>The platform normalizer refused an accredited name. The agreed defence; no executed case reaches it.</summary>
        NameNotNormalizable = 6,

        /// <summary>The name is empty after NFC and trimming.</summary>
        NameEmpty = 7,

        NameTooLong = 8,

        NameControlCharacter = 9,

        /// <summary>Another entry already has this name, compared in NFC without case.</summary>
        NameCollision = 10,

        ValueMissing = 11,

        /// <summary>The value has an unpaired surrogate: accepting it would persist U+FFFD instead of what the user typed.</summary>
        ValueMalformedUtf16 = 12,

        ValueTooLong = 13,

        ValueControlCharacter = 14,

        /// <summary>Creating would take the collection above the entries admitted on create.</summary>
        TooManyEntries = 15,
    }

    /// <summary>The outcome of one pure mutation: a new document, or a typed refusal and no document at all.</summary>
    public sealed class CustomPropertiesMutationResult
    {
        private CustomPropertiesMutationResult(
            CustomPropertiesDocument document,
            CustomPropertyId createdId,
            CustomPropertiesRejection rejection,
            string error)
        {
            Document = document;
            CreatedId = createdId;
            Rejection = rejection;
            Error = error;
        }

        public bool Succeeded => Rejection == CustomPropertiesRejection.None;

        /// <summary>The resulting collection. Null on every refusal: a refused operation has nothing to write.</summary>
        public CustomPropertiesDocument Document { get; }

        /// <summary>The identity minted by a successful create. Empty otherwise.</summary>
        public CustomPropertyId CreatedId { get; }

        public CustomPropertiesRejection Rejection { get; }

        /// <summary>The visible reason of a refusal. Null on success.</summary>
        public string Error { get; }

        internal static CustomPropertiesMutationResult Success(CustomPropertiesDocument document, CustomPropertyId createdId = default)
            => new CustomPropertiesMutationResult(document, createdId, CustomPropertiesRejection.None, null);

        internal static CustomPropertiesMutationResult Rejected(CustomPropertiesRejection rejection, string error)
            => new CustomPropertiesMutationResult(null, default, rejection, error);
    }

    /// <summary>
    /// The pure mutations of a collection of custom properties (I-54 D-05, D-10 / ADR-0039 §1, §9, §10): create, rename,
    /// change value and delete — emptying is deleting the last entry.
    ///
    /// <para>
    /// A mutation takes the READ, not a loose document. Its precondition is therefore structural: only an
    /// <see cref="CustomPropertiesReadOutcome.Absent"/> or <see cref="CustomPropertiesReadOutcome.Readable"/> read yields a
    /// document to change, and no operation — delete included — runs on any other outcome. The document read is never
    /// modified: every success returns a copy.
    /// </para>
    /// <para>
    /// What an operation does NOT touch is as much the contract as what it does: renaming changes only the name and
    /// changing a value only the value, both keeping the entry's extension data; untouched entries keep their exact names;
    /// the root extension data and the stored version travel unchanged, and the store resolves the version without
    /// downgrading when it writes.
    /// </para>
    /// <para>
    /// The write limits validate only what the operation INTRODUCES (D-05.6): a create checks its name, its value and the
    /// resulting count; a rename, its new name; a value change, its new value; a delete, nothing. Entries that came from a
    /// build with other limits are never re-judged. The authority across sibling views and the commit are G5.
    /// </para>
    /// </summary>
    public static class CustomPropertiesMutations
    {
        /// <summary>The most entries a create may leave in a collection. A write limit, not a format rule: reading never applies it.</summary>
        public const int MaxEntriesOnCreate = 50;

        /// <summary>The longest name, in UTF-16 units, after NFC and trimming.</summary>
        public const int MaxNameLength = 80;

        /// <summary>The longest value, in UTF-16 units.</summary>
        public const int MaxValueLength = 1000;

        /// <exception cref="ArgumentNullException"><paramref name="intent"/> is null.</exception>
        public static CustomPropertiesMutationResult Apply(CustomPropertiesReadResult current, CustomPropertiesIntent intent)
        {
            if (intent == null)
            {
                throw new ArgumentNullException(nameof(intent));
            }

            if (current == null)
            {
                return CustomPropertiesMutationResult.Rejected(
                    CustomPropertiesRejection.NotWritable,
                    "No se pueden modificar las propiedades personalizadas sin haberlas leído antes.");
            }

            if (!current.CanWrite)
            {
                return CustomPropertiesMutationResult.Rejected(
                    CustomPropertiesRejection.NotWritable,
                    current.Error ?? "Las propiedades personalizadas están en solo lectura.");
            }

            var source = current.Document;

            switch (intent.Kind)
            {
                case CustomPropertiesIntentKind.Create:
                    return Create(source, intent.Name, intent.Value);

                case CustomPropertiesIntentKind.Rename:
                    return Rename(source, intent.Id, intent.Name);

                case CustomPropertiesIntentKind.ChangeValue:
                    return ChangeValue(source, intent.Id, intent.Value);

                case CustomPropertiesIntentKind.Delete:
                    return Delete(source, intent.Id);

                default:
                    throw new InvalidOperationException("Operación de propiedades personalizadas no reconocida: " + intent.Kind + ".");
            }
        }

        private static CustomPropertiesMutationResult Create(CustomPropertiesDocument source, string name, string value)
        {
            if (!TryValidateName(name, source.Entries, -1, out var normalizedName, out var refusal) ||
                !TryValidateValue(value, out refusal))
            {
                return refusal;
            }

            if (source.Entries.Count + 1 > MaxEntriesOnCreate)
            {
                return CustomPropertiesMutationResult.Rejected(
                    CustomPropertiesRejection.TooManyEntries,
                    "Una colección admite como máximo " + MaxEntriesOnCreate + " propiedades al crear.");
            }

            var id = NewIdOutside(source.Entries);
            var copy = Copy(source);

            copy.Entries.Add(new CustomPropertyEntryDocument
            {
                Id = id.ToString(),
                Name = normalizedName,
                Value = value,
            });

            return CustomPropertiesMutationResult.Success(copy, id);
        }

        private static CustomPropertiesMutationResult Rename(CustomPropertiesDocument source, CustomPropertyId id, string name)
        {
            var index = IndexOf(source.Entries, id);

            if (index < 0)
            {
                return NotFound(id);
            }

            if (!TryValidateName(name, source.Entries, index, out var normalizedName, out var refusal))
            {
                return refusal;
            }

            var copy = Copy(source);
            copy.Entries[index].Name = normalizedName;
            return CustomPropertiesMutationResult.Success(copy);
        }

        private static CustomPropertiesMutationResult ChangeValue(CustomPropertiesDocument source, CustomPropertyId id, string value)
        {
            var index = IndexOf(source.Entries, id);

            if (index < 0)
            {
                return NotFound(id);
            }

            if (!TryValidateValue(value, out var refusal))
            {
                return refusal;
            }

            var copy = Copy(source);
            copy.Entries[index].Value = value;
            return CustomPropertiesMutationResult.Success(copy);
        }

        private static CustomPropertiesMutationResult Delete(CustomPropertiesDocument source, CustomPropertyId id)
        {
            var index = IndexOf(source.Entries, id);

            if (index < 0)
            {
                return NotFound(id);
            }

            var copy = Copy(source);
            copy.Entries.RemoveAt(index);
            return CustomPropertiesMutationResult.Success(copy);
        }

        /// <summary>
        /// The name rules, in the contract's order (D-05.1): well-formed UTF-16, no noncharacter, NFC, trim, length and
        /// control characters, and uniqueness against the OTHER entries in NFC without case.
        /// </summary>
        private static bool TryValidateName(
            string name,
            IReadOnlyList<CustomPropertyEntryDocument> entries,
            int selfIndex,
            out string normalizedName,
            out CustomPropertiesMutationResult refusal)
        {
            normalizedName = null;
            refusal = null;

            if (name == null)
            {
                refusal = Refuse(CustomPropertiesRejection.NameMissing, "El nombre de la propiedad es obligatorio.");
                return false;
            }

            switch (CustomPropertyText.Scan(name))
            {
                case CustomPropertyTextScan.MalformedUtf16:
                    refusal = Refuse(
                        CustomPropertiesRejection.NameMalformedUtf16,
                        "El nombre contiene texto UTF-16 mal formado (un surrogate suelto).");
                    return false;

                case CustomPropertyTextScan.ContainsNoncharacter:
                    refusal = Refuse(
                        CustomPropertiesRejection.NameNoncharacter,
                        "El nombre contiene un no-carácter Unicode, que los nombres no admiten.");
                    return false;
            }

            if (!CustomPropertyText.TryNormalizeAccredited(name, out var normalized))
            {
                refusal = Refuse(
                    CustomPropertiesRejection.NameNotNormalizable,
                    "El nombre no se puede normalizar a NFC en esta plataforma.");
                return false;
            }

            var trimmed = normalized.Trim();

            if (trimmed.Length == 0)
            {
                refusal = Refuse(CustomPropertiesRejection.NameEmpty, "El nombre no puede quedar vacío.");
                return false;
            }

            if (trimmed.Length > MaxNameLength)
            {
                refusal = Refuse(
                    CustomPropertiesRejection.NameTooLong,
                    "El nombre admite como máximo " + MaxNameLength + " caracteres.");
                return false;
            }

            if (CustomPropertyText.ContainsControlCharacter(trimmed))
            {
                refusal = Refuse(CustomPropertiesRejection.NameControlCharacter, "El nombre no admite caracteres de control.");
                return false;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                if (i != selfIndex &&
                    string.Equals(CustomPropertyText.ComparableName(entries[i].Name), trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    refusal = Refuse(
                        CustomPropertiesRejection.NameCollision,
                        "Ya existe otra propiedad llamada '" + trimmed + "'.");
                    return false;
                }
            }

            normalizedName = trimmed;
            return true;
        }

        /// <summary>The value rules (D-05.4): well-formed UTF-16 first, then length, then only tab, LF and CR as controls. Never normalized.</summary>
        private static bool TryValidateValue(string value, out CustomPropertiesMutationResult refusal)
        {
            refusal = null;

            if (value == null)
            {
                refusal = Refuse(
                    CustomPropertiesRejection.ValueMissing,
                    "El valor de la propiedad es obligatorio; puede estar vacío, pero no ausente.");
                return false;
            }

            if (CustomPropertyText.Scan(value) == CustomPropertyTextScan.MalformedUtf16)
            {
                refusal = Refuse(
                    CustomPropertiesRejection.ValueMalformedUtf16,
                    "El valor contiene texto UTF-16 mal formado (un surrogate suelto).");
                return false;
            }

            if (value.Length > MaxValueLength)
            {
                refusal = Refuse(
                    CustomPropertiesRejection.ValueTooLong,
                    "El valor admite como máximo " + MaxValueLength + " caracteres.");
                return false;
            }

            if (CustomPropertyText.ContainsControlCharacterOtherThanLineBreakOrTab(value))
            {
                refusal = Refuse(
                    CustomPropertiesRejection.ValueControlCharacter,
                    "El valor solo admite como caracteres de control el tabulador, el salto de línea y el retorno de carro.");
                return false;
            }

            return true;
        }

        /// <summary>The position of the entry with this id, compared by GUID VALUE; -1 when there is none.</summary>
        private static int IndexOf(IReadOnlyList<CustomPropertyEntryDocument> entries, CustomPropertyId id)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (CustomPropertyId.TryParse(entries[i].Id, out var candidate) && candidate == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private static CustomPropertyId NewIdOutside(IReadOnlyList<CustomPropertyEntryDocument> entries)
        {
            var id = CustomPropertyId.New();

            while (IndexOf(entries, id) >= 0)
            {
                id = CustomPropertyId.New();
            }

            return id;
        }

        private static CustomPropertiesDocument Copy(CustomPropertiesDocument source)
            => new CustomPropertiesDocument
            {
                SchemaVersion = source.SchemaVersion,
                Entries = source.Entries.Select(CopyEntry).ToList(),
                ExtensionData = CopyExtension(source.ExtensionData),
            };

        private static CustomPropertyEntryDocument CopyEntry(CustomPropertyEntryDocument entry)
            => new CustomPropertyEntryDocument
            {
                Id = entry.Id,
                Name = entry.Name,
                Value = entry.Value,
                ExtensionData = CopyExtension(entry.ExtensionData),
            };

        /// <summary>The elements are immutable once read, so a new dictionary over the same values is a full copy.</summary>
        private static IDictionary<string, JsonElement> CopyExtension(IDictionary<string, JsonElement> extension)
            => extension == null ? null : new Dictionary<string, JsonElement>(extension);

        private static CustomPropertiesMutationResult NotFound(CustomPropertyId id)
            => Refuse(CustomPropertiesRejection.EntryNotFound, "No existe ninguna propiedad personalizada con el id " + id + ".");

        private static CustomPropertiesMutationResult Refuse(CustomPropertiesRejection rejection, string error)
            => CustomPropertiesMutationResult.Rejected(rejection, error);
    }
}
