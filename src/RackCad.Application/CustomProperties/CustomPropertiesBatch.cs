using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;

namespace RackCad.Application.CustomProperties
{
    /// <summary>
    /// One property of a draft of custom properties (I-64 A-14, cuarta tanda, va): of the collection when <see cref="Id"/> is its id,
    /// new when it is empty. The order of the draft is the order of the collection that results.
    /// </summary>
    public sealed record CustomPropertyDraftEntry(string Id, string Name, string Value)
    {
        public bool IsNew => string.IsNullOrWhiteSpace(Id);
    }

    /// <summary>The field of a property a problem of the draft belongs to.</summary>
    public enum CustomPropertyDraftField
    {
        Name = 1,
        Value = 2,
        Collection = 3,
    }

    /// <summary>One problem of a draft of custom properties: the index of its property (-1 for the collection), the field and the text.</summary>
    public sealed record CustomPropertyDraftProblem(int Index, CustomPropertyDraftField Field, string Text);

    /// <summary>
    /// A whole draft of custom properties as ONE change (cuarta tanda, va; VA-02, VA-06 and VA-07 of the Owner): the page «Variables» and
    /// the tab «Propiedades» of a rack edit the collection freely, and their «Actualizar» writes it at once. PURE.
    ///
    /// <para>
    /// It reuses the single mutations (<see cref="CustomPropertiesMutations.Apply"/>), with their rules, in an order that never judges a
    /// state the user did not ask for: deletes, then every rename through a unique temporary name (so two properties can swap names),
    /// then the values, then the creates in the order of the draft. A property the draft does not touch keeps its exact name, value and
    /// extension data, and only what the draft introduces is validated (D-05.6).
    /// </para>
    /// </summary>
    public static class CustomPropertiesBatch
    {
        /// <summary>Why a draft is not written over a collection that changed since it was read.</summary>
        public const string ChangedSinceReadText = "las propiedades del proyecto cambiaron en el dibujo desde que se leyeron.";

        /// <summary>What a page read of a collection, to compare with what the write reads again: its outcome and its persisted shape.</summary>
        public static string Fingerprint(CustomPropertiesReadResult read)
            => read == null
                ? "none"
                : read.Outcome + ":" + (read.Document == null ? string.Empty : new CustomPropertiesStore().Serialize(read.Document));

        /// <summary>The collection the draft leaves over <paramref name="current"/>, or the first refusal of its mutations.</summary>
        public static CustomPropertiesMutationResult Apply(CustomPropertiesReadResult current, IReadOnlyList<CustomPropertyDraftEntry> desired)
        {
            if (current == null || !current.CanWrite)
            {
                return CustomPropertiesMutationResult.Rejected(CustomPropertiesRejection.NotWritable,
                    current?.Error ?? "Las propiedades personalizadas no se pueden escribir.");
            }

            desired ??= Array.Empty<CustomPropertyDraftEntry>();
            var document = current.Document;
            var byId = new Dictionary<CustomPropertyId, CustomPropertyEntryDocument>();
            foreach (var entry in document.Entries)
                if (CustomPropertyId.TryParse(entry.Id, out var id)) byId[id] = entry;

            var kept = new HashSet<CustomPropertyId>();
            foreach (var draft in desired.Where(d => !d.IsNew))
            {
                if (!CustomPropertyId.TryParse(draft.Id, out var id) || !byId.ContainsKey(id))
                    return CustomPropertiesMutationResult.Rejected(CustomPropertiesRejection.EntryNotFound,
                        "Una propiedad del borrador ya no está en el dibujo: vuelve a leer.");
                kept.Add(id);
            }

            var result = CustomPropertiesMutationResult.Success(document);
            CustomPropertiesMutationResult Step(CustomPropertiesIntent intent)
            {
                result = CustomPropertiesMutations.Apply(Read(result.Document), intent);
                return result;
            }

            foreach (var id in byId.Keys.Where(id => !kept.Contains(id)).ToList())
                if (!Step(CustomPropertiesIntent.Delete(id)).Succeeded) return result;

            var renames = desired.Where(d => !d.IsNew).Select(d => (Id: Parse(d.Id), d.Name))
                .Where(r => !string.Equals(byId[r.Id].Name, r.Name, StringComparison.Ordinal)
                            && !string.Equals(byId[r.Id].Name, (r.Name ?? string.Empty).Trim(), StringComparison.Ordinal))
                .ToList();
            foreach (var rename in renames)
                if (!Step(CustomPropertiesIntent.Rename(rename.Id, "~" + Guid.NewGuid().ToString("N"))).Succeeded) return result;
            foreach (var rename in renames)
                if (!Step(CustomPropertiesIntent.Rename(rename.Id, rename.Name)).Succeeded) return result;

            foreach (var draft in desired.Where(d => !d.IsNew))
            {
                var id = Parse(draft.Id);
                if (!string.Equals(byId[id].Value, draft.Value ?? string.Empty, StringComparison.Ordinal))
                    if (!Step(CustomPropertiesIntent.ChangeValue(id, draft.Value ?? string.Empty)).Succeeded) return result;
            }

            foreach (var draft in desired.Where(d => d.IsNew))
                if (!Step(CustomPropertiesIntent.Create(draft.Name, draft.Value ?? string.Empty)).Succeeded) return result;

            return result;
        }

        /// <summary>
        /// Every problem of the draft at once, for the page to show by its fields: what each property introduces (a new or renamed name,
        /// a new or changed value) is validated with the rules of its mutation, names are compared as the collection compares them, and
        /// a draft that would create past the limit says so.
        /// </summary>
        public static IReadOnlyList<CustomPropertyDraftProblem> Check(CustomPropertiesReadResult current, IReadOnlyList<CustomPropertyDraftEntry> desired)
        {
            var problems = new List<CustomPropertyDraftProblem>();
            desired ??= Array.Empty<CustomPropertyDraftEntry>();
            var byId = new Dictionary<CustomPropertyId, CustomPropertyEntryDocument>();
            foreach (var entry in current?.Document?.Entries ?? new List<CustomPropertyEntryDocument>())
                if (CustomPropertyId.TryParse(entry.Id, out var id)) byId[id] = entry;

            var empty = CustomPropertiesReadResult.Absent();
            var names = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < desired.Count; i++)
            {
                var draft = desired[i];
                byId.TryGetValue(draft.IsNew ? default : Parse(draft.Id), out var before);
                var nameChanged = before == null || !string.Equals(before.Name, (draft.Name ?? string.Empty).Trim(), StringComparison.Ordinal);
                var valueChanged = before == null || !string.Equals(before.Value, draft.Value ?? string.Empty, StringComparison.Ordinal);

                // A create over an empty collection judges a name and a value exactly as the mutation does, without the collisions.
                if (nameChanged)
                {
                    var probe = CustomPropertiesMutations.Apply(empty, CustomPropertiesIntent.Create(draft.Name, string.Empty));
                    if (!probe.Succeeded) problems.Add(new CustomPropertyDraftProblem(i, CustomPropertyDraftField.Name, probe.Error));
                }

                if (valueChanged)
                {
                    var probe = CustomPropertiesMutations.Apply(empty, CustomPropertiesIntent.Create("v", draft.Value ?? string.Empty));
                    if (!probe.Succeeded) problems.Add(new CustomPropertyDraftProblem(i, CustomPropertyDraftField.Value, probe.Error));
                }

                var comparable = Comparable(draft.Name);
                if (comparable.Length == 0) continue;
                if (names.TryGetValue(comparable, out var other))
                {
                    if (problems.All(p => !(p.Index == i && p.Field == CustomPropertyDraftField.Name)))
                        problems.Add(new CustomPropertyDraftProblem(i, CustomPropertyDraftField.Name,
                            "Ya existe otra propiedad llamada «" + (draft.Name ?? string.Empty).Trim() + "»."));
                }
                else
                {
                    names[comparable] = i;
                }
            }

            var creates = desired.Count(d => d.IsNew);
            if (creates > 0 && desired.Count > CustomPropertiesMutations.MaxEntriesOnCreate)
                problems.Add(new CustomPropertyDraftProblem(-1, CustomPropertyDraftField.Collection,
                    "Una colección admite como máximo " + CustomPropertiesMutations.MaxEntriesOnCreate + " propiedades al crear."));
            return problems;
        }

        /// <summary>The draft of a collection as it is: its properties, in order, with their ids.</summary>
        public static IReadOnlyList<CustomPropertyDraftEntry> DraftOf(CustomPropertiesDocument document)
            => (document?.Entries ?? new List<CustomPropertyEntryDocument>())
                .Select(entry => new CustomPropertyDraftEntry(entry.Id, entry.Name, entry.Value ?? string.Empty)).ToList();

        /// <summary>Whether a draft writes nothing over a collection: same properties, same order, same names and values.</summary>
        public static bool IsUnchanged(CustomPropertiesDocument document, IReadOnlyList<CustomPropertyDraftEntry> desired)
        {
            var before = DraftOf(document);
            desired ??= Array.Empty<CustomPropertyDraftEntry>();
            if (before.Count != desired.Count) return false;
            for (var i = 0; i < before.Count; i++)
            {
                if (desired[i].IsNew || !string.Equals(before[i].Id, desired[i].Id, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(before[i].Name, (desired[i].Name ?? string.Empty).Trim(), StringComparison.Ordinal)
                    || !string.Equals(before[i].Value, desired[i].Value ?? string.Empty, StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        private static CustomPropertiesReadResult Read(CustomPropertiesDocument document)
            => CustomPropertiesReadResult.Readable(document, Array.Empty<CustomPropertyId>());

        private static CustomPropertyId Parse(string text) => CustomPropertyId.TryParse(text, out var id) ? id : default;

        private static string Comparable(string name)
        {
            var trimmed = (name ?? string.Empty).Trim();
            if (trimmed.Length == 0) return string.Empty;
            return CustomPropertyText.TryNormalizeAccredited(trimmed, out var normalized) ? normalized.Trim() : trimmed;
        }
    }
}
