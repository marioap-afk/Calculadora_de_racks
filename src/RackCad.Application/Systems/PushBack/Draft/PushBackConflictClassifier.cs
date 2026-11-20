using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>The group of a changed leaf (D-PBT3-07).</summary>
    public enum PushBackChangeGroup { Name = 0, Rack = 1, Annotations = 2, Safety = 3, Fronts = 4, Modules = 5, SideB = 6, Composite = 7, OutsideEditor = 8 }

    /// <summary>Who changed a leaf with respect to the base.</summary>
    public enum PushBackChangeClass { ChangedExternally = 0, ChangedByUser = 1, Overlapping = 2 }

    /// <summary>One leaf that differs from the base in the current rack, in the draft or in both.</summary>
    public sealed class PushBackChangeEntry
    {
        internal PushBackChangeEntry(
            PushBackDocumentLevel level, string path, PushBackChangeGroup group,
            PushBackChangeClass changeClass, bool isStructural, string baseValue, string externalValue, string draftValue)
        {
            Level = level;
            Path = path;
            Group = group;
            Class = changeClass;
            IsStructural = isStructural;
            BaseValue = baseValue;
            ExternalValue = externalValue;
            DraftValue = draftValue;
        }

        public PushBackDocumentLevel Level { get; }

        /// <summary>The path <c>$.A.B[3].C</c>, with the real indexes.</summary>
        public string Path { get; }

        public PushBackChangeGroup Group { get; }

        public PushBackChangeClass Class { get; }

        public bool IsStructural { get; }

        /// <summary>The JSON text of the leaf, or the marker of an absent one.</summary>
        public string BaseValue { get; }

        public string ExternalValue { get; }

        public string DraftValue { get; }
    }

    /// <summary>The classification of the differences of a draft and of the current rack with respect to the base.</summary>
    public sealed class PushBackConflictClassification
    {
        internal PushBackConflictClassification(
            IEnumerable<PushBackChangeEntry> entries, IEnumerable<string> viewsToErase, IEnumerable<string> viewsRejected, bool allViewsObsolete)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            if (viewsToErase == null) throw new ArgumentNullException(nameof(viewsToErase));
            if (viewsRejected == null) throw new ArgumentNullException(nameof(viewsRejected));
            Entries = entries
                .OrderBy(entry => entry.Level)
                .ThenBy(entry => entry.Path, StringComparer.Ordinal)
                .ToList()
                .AsReadOnly();
            ChangedExternally = OfClass(PushBackChangeClass.ChangedExternally);
            ChangedByUser = OfClass(PushBackChangeClass.ChangedByUser);
            Overlapping = OfClass(PushBackChangeClass.Overlapping);
            Structural = Entries.Where(entry => entry.IsStructural).ToList().AsReadOnly();
            ViewsToErase = viewsToErase.OrderBy(key => key, StringComparer.Ordinal).ToList().AsReadOnly();
            ViewsRejected = viewsRejected.OrderBy(key => key, StringComparer.Ordinal).ToList().AsReadOnly();
            AllViewsObsolete = allViewsObsolete;
        }

        /// <summary>Every entry, by level and by path (ordinal).</summary>
        public IReadOnlyList<PushBackChangeEntry> Entries { get; }

        public IReadOnlyList<PushBackChangeEntry> ChangedExternally { get; }

        public IReadOnlyList<PushBackChangeEntry> ChangedByUser { get; }

        public IReadOnlyList<PushBackChangeEntry> Overlapping { get; }

        public IReadOnlyList<PushBackChangeEntry> Structural { get; }

        public IReadOnlyList<string> ViewsToErase { get; }

        public IReadOnlyList<string> ViewsRejected { get; }

        public bool AllViewsObsolete { get; }

        private IReadOnlyList<PushBackChangeEntry> OfClass(PushBackChangeClass changeClass)
            => Entries.Where(entry => entry.Class == changeClass).ToList().AsReadOnly();
    }

    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-07; OB-05.3) — the classifier of the conflicts of D-PB-05, not connected to the panel. It
    /// compares three states leaf by leaf: the base (b), the current rack (c) and the draft (d). It is an internal
    /// classification with named groups and classes, not a user-facing JSON diff. It does not tell what is derived from what
    /// is edited: it reports every leaf that changes, with the group of its path.
    /// </summary>
    public static class PushBackConflictClassifier
    {
        private static readonly string Absent = "<ausente>";

        public static PushBackConflictClassification Classify(
            PushBackFullDraftState baseState, PushBackFullDraftState currentRack, PushBackFullDraftState draft,
            IEnumerable<KeyValuePair<string, RackEmbedDocument>> currentMembers, RackCatalog catalog)
        {
            if (baseState == null) throw new ArgumentNullException(nameof(baseState));
            if (currentRack == null) throw new ArgumentNullException(nameof(currentRack));
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (currentMembers == null) throw new ArgumentNullException(nameof(currentMembers));

            // The three documents of each state, by level: the envelope (with the name), the project and the Push Back object.
            var baseDocs = DocumentsOf(baseState, catalog, out _);
            var rackDocs = DocumentsOf(currentRack, catalog, out _);
            var draftDocs = DocumentsOf(draft, catalog, out var draftDesign);

            var entries = new List<PushBackChangeEntry>();
            foreach (var level in new[] { PushBackDocumentLevel.Envelope, PushBackDocumentLevel.Project, PushBackDocumentLevel.Payload })
            {
                var index = (int)level;
                var baseLeaves = new Dictionary<string, string>(StringComparer.Ordinal);
                var rackLeaves = new Dictionary<string, string>(StringComparer.Ordinal);
                var draftLeaves = new Dictionary<string, string>(StringComparer.Ordinal);
                AddLeaves(baseLeaves, "$", baseDocs[index]);
                AddLeaves(rackLeaves, "$", rackDocs[index]);
                AddLeaves(draftLeaves, "$", draftDocs[index]);

                // The leaves that differ from the base on each side, by their path.
                var rackChanges = new HashSet<string>(StringComparer.Ordinal);
                var draftChanges = new HashSet<string>(StringComparer.Ordinal);
                DiffPaths(baseDocs[index], rackDocs[index], "$", rackChanges);
                DiffPaths(baseDocs[index], draftDocs[index], "$", draftChanges);

                foreach (var path in rackChanges.Union(draftChanges, StringComparer.Ordinal))
                {
                    var b = baseLeaves.TryGetValue(path, out var baseText) ? baseText : Absent;
                    var c = rackLeaves.TryGetValue(path, out var rackText) ? rackText : Absent;
                    var d = draftLeaves.TryGetValue(path, out var draftText) ? draftText : Absent;
                    var changeClass = rackChanges.Contains(path) && draftChanges.Contains(path)
                        ? PushBackChangeClass.Overlapping
                        : rackChanges.Contains(path) ? PushBackChangeClass.ChangedExternally : PushBackChangeClass.ChangedByUser;
                    var group = GroupOf(level, path);
                    var structural = group == PushBackChangeGroup.Modules
                                     || group == PushBackChangeGroup.SideB
                                     || group == PushBackChangeGroup.Composite
                                     || (group == PushBackChangeGroup.Fronts
                                         && (string.Equals(b, Absent, StringComparison.Ordinal)
                                             || string.Equals(c, Absent, StringComparison.Ordinal)
                                             || string.Equals(d, Absent, StringComparison.Ordinal)));
                    entries.Add(new PushBackChangeEntry(level, path, group, changeClass, structural, b, c, d));
                }
            }

            // The views: the disposition of the current members against the system of the DRAFT design. F4B creates none.
            var draftSystem = new PushBackResolver(catalog).Resolve(draftDesign);
            var plan = PushBackViewDispositions.Of(currentMembers, draftSystem, catalog);
            var rejected = plan.FrontalBOfOneWay.Concat(plan.NoAddress).Distinct(StringComparer.Ordinal);
            return new PushBackConflictClassification(entries, plan.ObsoleteLaterals, rejected, plan.AllObsolete);
        }

        /// <summary>
        /// The documents of a state, indexed by <see cref="PushBackDocumentLevel"/>: the envelope of its origin without
        /// <c>Design</c> and with the name of the state as <c>Name</c>; the project of its origin without <c>PushBack</c>; and the
        /// persisted <c>PushBack</c> object of its design, built over a COPY of the state.
        /// </summary>
        private static JsonNode[] DocumentsOf(PushBackFullDraftState state, RackCatalog catalog, out PushBackDesign design)
        {
            var copy = PushBackEditableStateComparer.Copy(state.Editable);
            design = new PushBackCompositeEditorAssembler(catalog).BuildDesign(copy.Editor, copy.BuildInputs(new PushBackEditorDesignAssembler(catalog)));
            var payload = JsonNode.Parse(new RackProjectStore().Serialize(RackProject.ForPushBack(design)))?["PushBack"];

            var envelope = new JsonObject { ["Name"] = state.Editable.Name == null ? null : JsonValue.Create(state.Editable.Name) };
            var project = new JsonObject();
            var origin = ParseObject(state.Origin);
            if (origin != null)
            {
                foreach (var property in origin)
                {
                    if (string.Equals(property.Key, "Name", StringComparison.Ordinal)) continue;
                    if (!string.Equals(property.Key, "Design", StringComparison.Ordinal))
                    {
                        envelope[property.Key] = property.Value?.DeepClone();
                        continue;
                    }

                    var inner = property.Value is JsonValue value && value.TryGetValue<string>(out var text) ? ParseObject(text) : null;
                    if (inner == null) continue;
                    foreach (var innerProperty in inner)
                    {
                        if (!string.Equals(innerProperty.Key, "PushBack", StringComparison.Ordinal)) project[innerProperty.Key] = innerProperty.Value?.DeepClone();
                    }
                }
            }

            var documents = new JsonNode[3];
            documents[(int)PushBackDocumentLevel.Envelope] = envelope;
            documents[(int)PushBackDocumentLevel.Project] = project;
            documents[(int)PushBackDocumentLevel.Payload] = payload;
            return documents;
        }

        private static JsonObject ParseObject(string text)
        {
            try
            {
                return JsonNode.Parse(text) as JsonObject;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>A leaf is a value, an empty object, an empty array or a null; the arrays go by index.</summary>
        private static void AddLeaves(Dictionary<string, string> leaves, string path, JsonNode node)
        {
            if (node is JsonObject obj && obj.Count > 0)
            {
                foreach (var property in obj) AddLeaves(leaves, path + "." + property.Key, property.Value);
                return;
            }

            if (node is JsonArray array && array.Count > 0)
            {
                for (var index = 0; index < array.Count; index++)
                    AddLeaves(leaves, path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", array[index]);
                return;
            }

            leaves[path] = node == null ? "null" : node.ToJsonString();
        }

        /// <summary>
        /// The paths of the leaves that differ between two documents. Objects go by member and arrays by index; a member or an
        /// element on one side only gives every leaf it holds; a container against a value gives the leaves of the container;
        /// two values differ when their kind or their text differs, and two numbers when they are not the same number.
        /// </summary>
        private static void DiffPaths(JsonNode left, JsonNode right, string path, HashSet<string> into)
        {
            if (left is JsonObject objectLeft && right is JsonObject objectRight)
            {
                foreach (var name in objectLeft.Select(pair => pair.Key).Union(objectRight.Select(pair => pair.Key), StringComparer.Ordinal))
                {
                    var inLeft = objectLeft.ContainsKey(name);
                    var inRight = objectRight.ContainsKey(name);
                    if (inLeft && inRight) DiffPaths(objectLeft[name], objectRight[name], path + "." + name, into);
                    else if (inLeft) LeafPaths(objectLeft[name], path + "." + name, into);
                    else LeafPaths(objectRight[name], path + "." + name, into);
                }

                return;
            }

            if (left is JsonArray arrayLeft && right is JsonArray arrayRight)
            {
                var count = Math.Max(arrayLeft.Count, arrayRight.Count);
                for (var index = 0; index < count; index++)
                {
                    var itemPath = path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
                    if (index < arrayLeft.Count && index < arrayRight.Count) DiffPaths(arrayLeft[index], arrayRight[index], itemPath, into);
                    else if (index < arrayLeft.Count) LeafPaths(arrayLeft[index], itemPath, into);
                    else LeafPaths(arrayRight[index], itemPath, into);
                }

                return;
            }

            var leftIsContainer = left is JsonObject || left is JsonArray;
            var rightIsContainer = right is JsonObject || right is JsonArray;
            if (leftIsContainer && !rightIsContainer) { LeafPaths(left, path, into); return; }
            if (!leftIsContainer && rightIsContainer) { LeafPaths(right, path, into); return; }
            if (!SameValue(left, right)) into.Add(path);
        }

        private static void LeafPaths(JsonNode node, string path, HashSet<string> into)
        {
            if (node is JsonObject obj && obj.Count > 0)
            {
                foreach (var property in obj) LeafPaths(property.Value, path + "." + property.Key, into);
                return;
            }

            if (node is JsonArray array && array.Count > 0)
            {
                for (var index = 0; index < array.Count; index++)
                    LeafPaths(array[index], path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", into);
                return;
            }

            into.Add(path);
        }

        private static bool SameValue(JsonNode left, JsonNode right)
        {
            if (left == null || right == null) return left == null && right == null;
            if (left is JsonValue valueLeft && right is JsonValue valueRight
                && valueLeft.TryGetValue<double>(out var numberLeft) && valueRight.TryGetValue<double>(out var numberRight))
            {
                return numberLeft == numberRight;
            }

            return string.Equals(left.ToJsonString(), right.ToJsonString(), StringComparison.Ordinal);
        }

        /// <summary>The group of a path, by the longest prefix of the table of D-PBT3-07.</summary>
        private static PushBackChangeGroup GroupOf(PushBackDocumentLevel level, string path)
        {
            if (level == PushBackDocumentLevel.Envelope) return Covers("$.Name", path) ? PushBackChangeGroup.Name : PushBackChangeGroup.OutsideEditor;
            if (level == PushBackDocumentLevel.Project) return PushBackChangeGroup.OutsideEditor;

            if (Covers("$.SchemaVersion", path)) return PushBackChangeGroup.OutsideEditor;
            if (Covers("$.Structure.Fronts", path) || Covers("$.Fronts", path)) return PushBackChangeGroup.Fronts;
            if (Covers("$.Structure.Modules", path) || Covers("$.Structure.HeaderLineOverrides", path) || Covers("$.Structure.DerivedPostLineOverrides", path))
                return PushBackChangeGroup.Modules;
            if (Covers("$.SideB", path)) return PushBackChangeGroup.SideB;
            if (Covers("$.Composite", path)) return PushBackChangeGroup.Composite;
            if (Covers("$.Structure.SafetySelections", path) || Covers("$.RearTopeSaque", path) || Covers("$.RearTopePieceId", path)
                || Covers("$.RearTopeOffCells", path) || Covers("$.DefensePieceId", path))
                return PushBackChangeGroup.Safety;
            if (Covers("$.Structure.NumberFronts", path) || Covers("$.Structure.NumberLevels", path) || Covers("$.Structure.DrawRackName", path)
                || Covers("$.Structure.AnnotationScale", path) || Covers("$.Structure.Dimensions", path) || Covers("$.Structure.DimensionViews", path)
                || Covers("$.Structure.DimensionStyle", path))
                return PushBackChangeGroup.Annotations;
            if (Covers("$.Structure", path) || Covers("$.LegacyHighEndBeamPeralte", path)) return PushBackChangeGroup.Rack;

            // The default row (V42-03): a path that no prefix covers goes by the ownership map.
            return PushBackEditableStateOwnership.Of(PushBackDocumentLevel.Payload, path) == PushBackFieldOwner.Editor
                ? PushBackChangeGroup.Rack
                : PushBackChangeGroup.OutsideEditor;
        }

        private static bool Covers(string prefix, string path)
            => string.Equals(path, prefix, StringComparison.Ordinal)
               || path.StartsWith(prefix + ".", StringComparison.Ordinal)
               || path.StartsWith(prefix + "[", StringComparison.Ordinal);
    }
}
