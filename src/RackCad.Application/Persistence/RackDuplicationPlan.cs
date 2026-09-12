using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.ProjectVariables;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// One entity the user selected for RACKDUPLICAR, as the Plugin measured it (I-51). Plain data: the keys are
    /// opaque strings (AutoCAD handles in production) and nothing here is a live AutoCAD object.
    /// </summary>
    public sealed class RackDuplicationSelectedReference
    {
        public RackDuplicationSelectedReference(string referenceKey, bool isBlockReference, bool isInModelSpace, string definitionKey)
        {
            ReferenceKey = referenceKey;
            IsBlockReference = isBlockReference;
            IsInModelSpace = isInModelSpace;
            DefinitionKey = definitionKey;
        }

        /// <summary>Identity of the selected entity. A key selected twice counts once.</summary>
        public string ReferenceKey { get; }

        /// <summary>False for anything that is not a block reference: it is ignored with a notice.</summary>
        public bool IsBlockReference { get; }

        /// <summary>False for a reference outside Model Space: it is filtered with a notice BEFORE classification.</summary>
        public bool IsInModelSpace { get; }

        /// <summary>The block definition the reference points at. Required for a block reference in Model Space.</summary>
        public string DefinitionKey { get; }
    }

    /// <summary>One distinct block definition behind the selection, with its raw RackCad payload (or none).</summary>
    public sealed class RackDuplicationDefinitionSnapshot
    {
        public RackDuplicationDefinitionSnapshot(string definitionKey, string rawPayload, string definitionName)
        {
            DefinitionKey = definitionKey;
            RawPayload = rawPayload;
            DefinitionName = definitionName;
        }

        public string DefinitionKey { get; }

        /// <summary>The envelope text as read from the definition; null or empty when the block carries no RackCad data.</summary>
        public string RawPayload { get; }

        /// <summary>The block definition's name: the only human handle on a definition whose envelope cannot be read.</summary>
        public string DefinitionName { get; }
    }

    /// <summary>Which of the two variants a <see cref="RackDuplicationSourceKey"/> is.</summary>
    public enum RackDuplicationSourceKeyKind
    {
        /// <summary>The envelope carries a usable rack identity.</summary>
        RackId = 1,

        /// <summary>Legacy envelope without identity: the definition is its own source (PD-6).</summary>
        Definition = 2,
    }

    /// <summary>
    /// The logical source of a group of copies: a RackId, or — for a legacy envelope without identity — the
    /// definition itself (I-51 PD-6, RC-2).
    ///
    /// <para>
    /// The two variants NEVER compare equal, even when a RackId happens to read like a definition handle: the
    /// kind is part of the identity. A RackId compares case-insensitively, as every GUID lookup in the Plugin
    /// does; a definition key compares ordinally. A definition key exists only inside one duplication batch:
    /// it is not a RackId, it is never persisted and it is never shown as one (I-47 C4.6-3).
    /// </para>
    /// </summary>
    public sealed class RackDuplicationSourceKey : IEquatable<RackDuplicationSourceKey>
    {
        private RackDuplicationSourceKey(RackDuplicationSourceKeyKind kind, string value)
        {
            Kind = kind;
            Value = value ?? string.Empty;
        }

        public RackDuplicationSourceKeyKind Kind { get; }

        public string Value { get; }

        public static RackDuplicationSourceKey ForRackId(string rackId)
            => new RackDuplicationSourceKey(RackDuplicationSourceKeyKind.RackId, rackId);

        public static RackDuplicationSourceKey ForDefinition(string definitionKey)
            => new RackDuplicationSourceKey(RackDuplicationSourceKeyKind.Definition, definitionKey);

        public bool Equals(RackDuplicationSourceKey other)
        {
            if (other is null || other.Kind != Kind)
            {
                return false;
            }

            return Comparer(Kind).Equals(Value, other.Value);
        }

        public override bool Equals(object obj) => Equals(obj as RackDuplicationSourceKey);

        public override int GetHashCode() => ((int)Kind * 397) ^ Comparer(Kind).GetHashCode(Value);

        public override string ToString()
            => Kind == RackDuplicationSourceKeyKind.RackId ? "rack " + Value : "definicion " + Value;

        private static StringComparer Comparer(RackDuplicationSourceKeyKind kind)
            => kind == RackDuplicationSourceKeyKind.RackId ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }

    /// <summary>
    /// One logical rack to copy: its unique definitions and ALL the physical references selected from them
    /// (I-51 INV-06). Cloning each definition once and pointing every reference at the clone of its own
    /// definition is what keeps linked references linked and the copy count of RACKLISTA/RACKBOMTOTAL — the
    /// maximum of direct references per RackId — equal to the source.
    /// </summary>
    public sealed class RackDuplicationGroup
    {
        internal RackDuplicationGroup(
            RackDuplicationSourceKey key,
            string kind,
            string baseName,
            IReadOnlyList<RackDuplicationDefinitionSnapshot> definitions,
            IReadOnlyList<RackDuplicationSelectedReference> references)
        {
            Key = key;
            Kind = kind;
            BaseName = baseName;
            Definitions = definitions;
            References = references;
        }

        public RackDuplicationSourceKey Key { get; }

        /// <summary>The envelope kind, as stored by the first selected definition of the group.</summary>
        public string Kind { get; }

        /// <summary>The single logical name of the group, trimmed; "Rack" when no definition carries one.</summary>
        public string BaseName { get; }

        /// <summary>The distinct definitions to clone, in order of first selection.</summary>
        public IReadOnlyList<RackDuplicationDefinitionSnapshot> Definitions { get; }

        /// <summary>Every selected reference of the group, in selection order.</summary>
        public IReadOnlyList<RackDuplicationSelectedReference> References { get; }
    }

    /// <summary>
    /// I-51 (ID15) — the PURE planner of RACKDUPLICAR with several sources: classify → group → deduplicate, and
    /// then assign identity and name per destination.
    ///
    /// <para>
    /// It lives here and not in the Plugin because every rule it applies decides identity and the BOM copy
    /// count, and the Plugin cannot be loaded by any test suite (ADR-0003): a rule kept only there could be
    /// watched by text guards alone. It knows nothing of AutoCAD — no positions, no transforms, no transactions —
    /// and it neither re-stamps nor clones: the Plugin does both, with this plan as its only authority on WHICH
    /// definitions and references form each copy.
    /// </para>
    /// <para>
    /// Classification never lets an unreadable RackCad payload vanish: a block that carries RackCad data this
    /// build cannot use fails the whole plan (PD-3; I-47 C4.6-1, C4.7-1). It deliberately does not reuse the
    /// ID22A scan projection, which reads a missing identity as an unreadable envelope and would reject the
    /// legacy sources PD-6 includes.
    /// </para>
    /// </summary>
    public sealed class RackDuplicationPlan
    {
        private const string FallbackBaseName = "Rack";

        private static readonly IReadOnlyList<RackDuplicationGroup> NoGroups = new RackDuplicationGroup[0];

        private readonly IReadOnlyCollection<Guid> sourceRackIds;

        private RackDuplicationPlan(
            IReadOnlyList<RackDuplicationGroup> groups,
            IReadOnlyList<string> errors,
            IReadOnlyCollection<Guid> sourceRackIds,
            int ignoredNonBlockReferences,
            int ignoredOutsideModelSpace,
            int ignoredWithoutRackData)
        {
            this.sourceRackIds = sourceRackIds;
            Groups = groups;
            Errors = errors;
            IgnoredNonBlockReferences = ignoredNonBlockReferences;
            IgnoredOutsideModelSpace = ignoredOutsideModelSpace;
            IgnoredWithoutRackData = ignoredWithoutRackData;
            Notices = DescribeNotices(ignoredNonBlockReferences, ignoredOutsideModelSpace, ignoredWithoutRackData);
        }

        /// <summary>True when there is at least one group and nothing failed. A failed plan has NO groups: nothing is partial.</summary>
        public bool IsSuccess => Errors.Count == 0;

        public IReadOnlyList<RackDuplicationGroup> Groups { get; }

        /// <summary>The visible reasons the plan failed, attributable to a definition or a RackId. Empty on success.</summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>Aggregated notices for what was ignored. Present on success and on failure.</summary>
        public IReadOnlyList<string> Notices { get; }

        public int IgnoredNonBlockReferences { get; }

        public int IgnoredOutsideModelSpace { get; }

        public int IgnoredWithoutRackData { get; }

        /// <summary>
        /// Classifies the physical selection and folds it into logical sources.
        ///
        /// <para>
        /// Order, per the contract (INV-01..INV-05): not a block reference → ignored; outside Model Space →
        /// filtered; no RackCad payload → ignored; payload present but unusable (envelope unreadable or of a future
        /// major, blank or unknown kind, blank design) → FAIL; otherwise a valid source. Only the definitions the
        /// selection references are ever read: a sibling view nobody selected is never added.
        /// </para>
        /// </summary>
        /// <param name="selection">The selected entities, in selection order.</param>
        /// <param name="definitions">One snapshot per distinct definition referenced from Model Space.</param>
        /// <param name="isKnownKind">Whether this build has a handler for an envelope kind (looked up case-insensitively).</param>
        public static RackDuplicationPlan Build(
            IReadOnlyList<RackDuplicationSelectedReference> selection,
            IReadOnlyList<RackDuplicationDefinitionSnapshot> definitions,
            Func<string, bool> isKnownKind)
        {
            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            if (isKnownKind == null)
            {
                throw new ArgumentNullException(nameof(isKnownKind));
            }

            var snapshots = IndexDefinitions(definitions);
            var classified = new Dictionary<string, Classification>(StringComparer.Ordinal);
            var seenReferences = new HashSet<string>(StringComparer.Ordinal);
            var builders = new List<GroupBuilder>();
            var byKey = new Dictionary<RackDuplicationSourceKey, GroupBuilder>();
            var sourceRackIds = new HashSet<Guid>();
            var errors = new List<string>();
            int ignoredNonBlock = 0, ignoredOutside = 0, ignoredWithoutData = 0;

            foreach (var reference in selection)
            {
                if (reference == null || string.IsNullOrEmpty(reference.ReferenceKey))
                {
                    throw new ArgumentException("Every selected entity needs a reference key.", nameof(selection));
                }

                if (!seenReferences.Add(reference.ReferenceKey))
                {
                    continue; // the same physical entity selected twice counts once
                }

                if (!reference.IsBlockReference)
                {
                    ignoredNonBlock++;
                    continue;
                }

                if (!reference.IsInModelSpace)
                {
                    ignoredOutside++; // PD-7: filtered before its definition is even looked at
                    continue;
                }

                if (string.IsNullOrEmpty(reference.DefinitionKey) ||
                    !snapshots.TryGetValue(reference.DefinitionKey, out var snapshot))
                {
                    throw new ArgumentException(
                        "The block reference '" + reference.ReferenceKey + "' has no definition snapshot.", nameof(definitions));
                }

                if (!classified.TryGetValue(snapshot.DefinitionKey, out var classification))
                {
                    classification = Classify(snapshot, isKnownKind);
                    classified.Add(snapshot.DefinitionKey, classification);

                    if (classification.Error != null)
                    {
                        errors.Add(classification.Error);
                    }
                }

                if (classification.Error != null)
                {
                    continue; // already reported once for this definition
                }

                if (classification.Envelope == null)
                {
                    ignoredWithoutData++;
                    continue;
                }

                var key = KeyOf(snapshot, classification.Envelope);

                if (Guid.TryParse(classification.Envelope.Id, out var sourceRackId))
                {
                    sourceRackIds.Add(sourceRackId); // a copy may never reuse the identity of any selected source
                }

                if (!byKey.TryGetValue(key, out var builder))
                {
                    builder = new GroupBuilder(key);
                    byKey.Add(key, builder);
                    builders.Add(builder);
                }

                builder.Add(snapshot, classification.Envelope, reference);
            }

            if (errors.Count == 0 && builders.Count == 0)
            {
                errors.Add("No hay racks validos de RackCad en la seleccion: no se duplico nada.");
            }

            var groups = new List<RackDuplicationGroup>(builders.Count);

            if (errors.Count == 0)
            {
                foreach (var builder in builders)
                {
                    var group = builder.Build(out var groupErrors);
                    errors.AddRange(groupErrors);
                    groups.Add(group);
                }
            }

            return new RackDuplicationPlan(
                errors.Count == 0 ? groups : NoGroups,
                errors,
                sourceRackIds,
                ignoredNonBlock,
                ignoredOutside,
                ignoredWithoutData);
        }

        /// <summary>
        /// The identity/name sequencer for the destination points of this plan. Only a successful plan has one:
        /// a failed plan has nothing to copy.
        /// </summary>
        /// <param name="newRackId">The id generator. Production passes <c>Guid.NewGuid</c>; it is called exactly once per group per destination.</param>
        public RackDuplicationDestinationAssigner CreateDestinationAssigner(Func<Guid> newRackId)
        {
            if (newRackId == null)
            {
                throw new ArgumentNullException(nameof(newRackId));
            }

            if (!IsSuccess)
            {
                throw new InvalidOperationException("A failed duplication plan has no destinations to assign.");
            }

            return new RackDuplicationDestinationAssigner(Groups, sourceRackIds, newRackId);
        }

        private static Dictionary<string, RackDuplicationDefinitionSnapshot> IndexDefinitions(
            IReadOnlyList<RackDuplicationDefinitionSnapshot> definitions)
        {
            var index = new Dictionary<string, RackDuplicationDefinitionSnapshot>(StringComparer.Ordinal);

            foreach (var definition in definitions)
            {
                if (definition == null || string.IsNullOrEmpty(definition.DefinitionKey))
                {
                    throw new ArgumentException("Every definition snapshot needs a definition key.", nameof(definitions));
                }

                if (index.ContainsKey(definition.DefinitionKey))
                {
                    throw new ArgumentException(
                        "The definition '" + definition.DefinitionKey + "' was snapshotted twice.", nameof(definitions));
                }

                index.Add(definition.DefinitionKey, definition);
            }

            return index;
        }

        private static Classification Classify(RackDuplicationDefinitionSnapshot snapshot, Func<string, bool> isKnownKind)
        {
            // No RackCad text at all: the same convention as the drawing-wide envelope scan.
            if (string.IsNullOrEmpty(snapshot.RawPayload))
            {
                return Classification.WithoutRackData;
            }

            var envelope = new RackEmbedStore().Deserialize(snapshot.RawPayload);

            if (envelope == null)
            {
                return Classification.Unusable(Unusable(snapshot, null,
                    "el sobre no se puede leer (JSON invalido o version posterior a esta)"));
            }

            var rackId = string.IsNullOrWhiteSpace(envelope.Id) ? null : envelope.Id;

            if (string.IsNullOrWhiteSpace(envelope.Kind))
            {
                return Classification.Unusable(Unusable(snapshot, rackId, "no declara tipo de rack"));
            }

            if (!isKnownKind(envelope.Kind))
            {
                return Classification.Unusable(Unusable(snapshot, rackId,
                    "tipo de rack no reconocido (" + envelope.Kind + ")"));
            }

            if (string.IsNullOrWhiteSpace(envelope.Design))
            {
                return Classification.Unusable(Unusable(snapshot, rackId, "no lleva diseno"));
            }

            return Classification.Valid(envelope);
        }

        private static string Unusable(RackDuplicationDefinitionSnapshot snapshot, string rackId, string reason)
            => "La definicion de bloque '" + DisplayName(snapshot) + "'" +
               (rackId == null ? string.Empty : " del rack " + rackId) +
               " lleva datos de RackCad que esta version no puede usar: " + reason + ". No se duplico nada.";

        private static string DisplayName(RackDuplicationDefinitionSnapshot snapshot)
            => string.IsNullOrWhiteSpace(snapshot.DefinitionName) ? snapshot.DefinitionKey : snapshot.DefinitionName;

        private static RackDuplicationSourceKey KeyOf(RackDuplicationDefinitionSnapshot snapshot, RackEmbedDocument envelope)
            => string.IsNullOrWhiteSpace(envelope.Id)
                ? RackDuplicationSourceKey.ForDefinition(snapshot.DefinitionKey)
                : RackDuplicationSourceKey.ForRackId(envelope.Id);

        private static IReadOnlyList<string> DescribeNotices(int nonBlock, int outside, int withoutData)
        {
            var notices = new List<string>();

            if (nonBlock > 0)
            {
                notices.Add(Count(nonBlock) + " objeto(s) seleccionado(s) no son referencias de bloque: se ignoran.");
            }

            if (outside > 0)
            {
                notices.Add(Count(outside) + " referencia(s) fuera del espacio modelo: se ignoran (solo se duplica desde el espacio modelo).");
            }

            if (withoutData > 0)
            {
                notices.Add(Count(withoutData) + " referencia(s) a bloques sin datos de RackCad: se ignoran.");
            }

            return notices;
        }

        private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);

        private sealed class Classification
        {
            private Classification(RackEmbedDocument envelope, string error)
            {
                Envelope = envelope;
                Error = error;
            }

            public RackEmbedDocument Envelope { get; }

            public string Error { get; }

            public static Classification WithoutRackData { get; } = new Classification(null, null);

            public static Classification Unusable(string error) => new Classification(null, error);

            public static Classification Valid(RackEmbedDocument envelope) => new Classification(envelope, null);
        }

        private sealed class GroupBuilder
        {
            private readonly List<RackDuplicationDefinitionSnapshot> definitions = new List<RackDuplicationDefinitionSnapshot>();
            private readonly List<RackEmbedDocument> envelopes = new List<RackEmbedDocument>();
            private readonly HashSet<string> definitionKeys = new HashSet<string>(StringComparer.Ordinal);
            private readonly List<RackDuplicationSelectedReference> references = new List<RackDuplicationSelectedReference>();

            public GroupBuilder(RackDuplicationSourceKey key) => Key = key;

            public RackDuplicationSourceKey Key { get; }

            public void Add(RackDuplicationDefinitionSnapshot snapshot, RackEmbedDocument envelope, RackDuplicationSelectedReference reference)
            {
                if (definitionKeys.Add(snapshot.DefinitionKey))
                {
                    definitions.Add(snapshot);
                    envelopes.Add(envelope);
                }

                references.Add(reference);
            }

            /// <summary>The group, and the reasons it is not a single consistent logical rack (INV-05).</summary>
            public RackDuplicationGroup Build(out List<string> errors)
            {
                errors = new List<string>();

                var kinds = envelopes.Select(envelope => envelope.Kind).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (kinds.Count > 1)
                {
                    errors.Add("Las vistas seleccionadas del " + Key + " mezclan tipos de rack distintos (" +
                               string.Join(", ", kinds) + "). No se duplico nada.");
                }

                var names = envelopes
                    .Select(envelope => envelope.Name?.Trim())
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                if (names.Count > 1)
                {
                    errors.Add("Las vistas seleccionadas del " + Key + " tienen nombres distintos (" +
                               string.Join(", ", names.Select(name => "'" + name + "'")) +
                               "). Reconcilia el rack con RACKEDITAR antes de duplicarlo.");
                }

                if (kinds.Count == 1 && definitions.Count > 1 &&
                    string.Equals(kinds[0], RackEmbedDocument.KindSelective, StringComparison.OrdinalIgnoreCase))
                {
                    var authority = SelectiveAuthority();

                    if (authority != null)
                    {
                        errors.Add(authority);
                    }
                }

                return new RackDuplicationGroup(
                    Key,
                    envelopes[0].Kind,
                    names.Count == 1 ? names[0] : FallbackBaseName,
                    definitions,
                    references);
            }

            /// <summary>
            /// A Selective copy with several views must not be born divergent: every selected view has to carry the
            /// SAME authored document, compared with the one total comparator of I-47 (ADR-0034 §9). An unreadable
            /// view fails the group; the readable one is never picked instead.
            /// </summary>
            private string SelectiveAuthority()
            {
                var store = new SelectivePalletDesignStore();
                var documents = new List<SelectivePalletDesignDocument>(envelopes.Count);

                for (var i = 0; i < envelopes.Count; i++)
                {
                    try
                    {
                        documents.Add(store.Deserialize(envelopes[i].Design));
                    }
                    catch (InvalidOperationException ex)
                    {
                        // The store SIGNALS by throwing; here it becomes the visible reason this group cannot be copied.
                        return "Una vista seleccionada del " + Key + " (definicion '" + DisplayName(definitions[i]) +
                               "') tiene un diseno selectivo ilegible: " + ex.Message + " No se elige otra vista.";
                    }
                }

                return SelectiveAuthoredAuthority.IsSameAuthority(documents)
                    ? null
                    : "Las vistas seleccionadas del " + Key + " tienen disenos distintos. " +
                      "Reconcilia el rack con RACKEDITAR antes de duplicarlo.";
            }
        }
    }

    /// <summary>
    /// Hands out the identity and name of each destination point of one plan (I-51 INV-07, INV-08).
    ///
    /// <para>
    /// Every call to <see cref="Next"/> is one destination. It asks the generator for exactly ONE id per group
    /// and names every group after the destination ordinal with the historic policy ("… - copia", "… - copia 2").
    /// An id that is empty, repeated — within the destination or across destinations of the same command — or
    /// equal to a source RackId fails the destination with nothing assigned. Only a successful destination
    /// consumes an ordinal; there is no notion of command keywords here.
    /// </para>
    /// </summary>
    public sealed class RackDuplicationDestinationAssigner
    {
        private readonly IReadOnlyList<RackDuplicationGroup> groups;
        private readonly HashSet<Guid> sourceRackIds;
        private readonly Func<Guid> newRackId;
        private readonly HashSet<Guid> issued = new HashSet<Guid>();
        private int assignedDestinations;

        internal RackDuplicationDestinationAssigner(
            IReadOnlyList<RackDuplicationGroup> groups, IReadOnlyCollection<Guid> sourceRackIds, Func<Guid> newRackId)
        {
            this.groups = groups;
            this.sourceRackIds = new HashSet<Guid>(sourceRackIds);
            this.newRackId = newRackId;
        }

        public RackDuplicationDestinationAssignment Next()
        {
            var ordinal = assignedDestinations + 1;
            var assignments = new List<RackDuplicationGroupAssignment>(groups.Count);
            var thisDestination = new HashSet<Guid>();

            foreach (var group in groups)
            {
                var id = newRackId();

                if (id == Guid.Empty)
                {
                    return RackDuplicationDestinationAssignment.Failure(ordinal,
                        "El generador de identidades devolvio un GUID vacio: no se asigno el destino.");
                }

                if (sourceRackIds.Contains(id))
                {
                    return RackDuplicationDestinationAssignment.Failure(ordinal,
                        "El generador de identidades devolvio la identidad de un rack de origen: no se asigno el destino.");
                }

                if (issued.Contains(id) || !thisDestination.Add(id))
                {
                    return RackDuplicationDestinationAssignment.Failure(ordinal,
                        "El generador de identidades repitio un GUID: no se asigno el destino.");
                }

                assignments.Add(new RackDuplicationGroupAssignment(group.Key, id, CopyName(group.BaseName, ordinal)));
            }

            foreach (var assignment in assignments)
            {
                issued.Add(assignment.NewRackId);
            }

            assignedDestinations = ordinal;
            return RackDuplicationDestinationAssignment.Success(ordinal, assignments);
        }

        /// <summary>The historic RACKDUPLICAR name: "&lt;base&gt; - copia" for the first destination, "&lt;base&gt; - copia N" after.</summary>
        private static string CopyName(string baseName, int ordinal)
            => ordinal == 1
                ? baseName + " - copia"
                : baseName + " - copia " + ordinal.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>The identity and name of every group for one destination point, or why none was assigned.</summary>
    public sealed class RackDuplicationDestinationAssignment
    {
        private static readonly IReadOnlyList<RackDuplicationGroupAssignment> None = new RackDuplicationGroupAssignment[0];

        private RackDuplicationDestinationAssignment(int ordinal, IReadOnlyList<RackDuplicationGroupAssignment> groups, string error)
        {
            Ordinal = ordinal;
            Groups = groups;
            Error = error;
        }

        public bool IsSuccess => Error == null;

        /// <summary>The 1-based destination ordinal this assignment was made for.</summary>
        public int Ordinal { get; }

        /// <summary>One assignment per group, in group order. Empty on failure: nothing is partial.</summary>
        public IReadOnlyList<RackDuplicationGroupAssignment> Groups { get; }

        public string Error { get; }

        internal static RackDuplicationDestinationAssignment Success(int ordinal, IReadOnlyList<RackDuplicationGroupAssignment> groups)
            => new RackDuplicationDestinationAssignment(ordinal, groups, null);

        internal static RackDuplicationDestinationAssignment Failure(int ordinal, string error)
            => new RackDuplicationDestinationAssignment(ordinal, None, error);
    }

    /// <summary>{LogicalSourceKey, NewRackId, CopyName}: what every definition of one group carries at one destination.</summary>
    public sealed class RackDuplicationGroupAssignment
    {
        internal RackDuplicationGroupAssignment(RackDuplicationSourceKey key, Guid newRackId, string copyName)
        {
            Key = key;
            NewRackId = newRackId;
            CopyName = copyName;
        }

        public RackDuplicationSourceKey Key { get; }

        public Guid NewRackId { get; }

        public string CopyName { get; }
    }
}
