using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;

namespace RackCad.Application.CustomProperties
{
    /// <summary>
    /// The ONE order of results of the rack authority (I-54 D-09.8 / ADR-0039 §8). With several causes at once, the first
    /// wins. Every result but <see cref="Single"/> is read-only; the only other write is a safe unify from
    /// <see cref="Divergent"/> (D-09.10).
    /// </summary>
    public enum RackCustomPropertiesAuthorityOutcome
    {
        /// <summary>The picked definition comes from, or depends on, an external reference.</summary>
        XrefRejected = 1,

        /// <summary>The picked envelope is readable but its rack id is blank: it belongs to no rack.</summary>
        NoIdentity = 2,

        /// <summary>Some non-dependent scanned definition, placed or not, has a RackCad payload this build cannot interpret.</summary>
        IndeterminateMembership = 3,

        /// <summary>Some member has a blank kind, or the members declare more than one kind.</summary>
        MixedKind = 4,

        /// <summary>One common kind that this build does not know: read-only (C-1).</summary>
        UnknownKind = 5,

        /// <summary>Some member's collection is PresentButUnreadable, AmbiguousIdentity, IncompatibleMajor or DepthLimitExceeded.</summary>
        CustomPropertiesReadOnly = 6,

        /// <summary>All collections writable, but not all canonically equal.</summary>
        Divergent = 7,

        /// <summary>All collections writable and canonically equal: one collection for the whole rack.</summary>
        Single = 8,
    }

    /// <summary>One member of the rack: one view-block whose envelope carries the rack id, with its own collection.</summary>
    public sealed class RackCustomPropertiesMember
    {
        internal RackCustomPropertiesMember(RackCustomPropertiesDefinition definition, CustomPropertiesReadResult collection)
        {
            Definition = definition;
            Collection = collection;
            CanonicalForm = collection.CanWrite ? CustomPropertiesCanonicalForm.Of(collection) : null;
        }

        public RackCustomPropertiesDefinition Definition { get; }

        public string Handle => Definition.Handle;

        public string BlockName => Definition.BlockName;

        public bool IsPlaced => Definition.IsPlaced;

        public string Kind => Definition.Envelope.Kind;

        public string View => Definition.Envelope.View;

        public int Section => Definition.Envelope.Section;

        /// <summary>This view's own collection, as the store read it.</summary>
        public CustomPropertiesReadResult Collection { get; }

        /// <summary>The canonical text of the collection (D-09.7); null when the collection is not writable.</summary>
        public string CanonicalForm { get; }
    }

    /// <summary>
    /// A view that the user could pick as the source of a unify, and the views that would block it (D-09.10). A view
    /// blocks when it would be overwritten and carries root or entry extension data, or a minor higher than the source's.
    /// There is no default source: an option is only something the user may choose.
    /// </summary>
    public sealed class RackCustomPropertiesUnifyOption
    {
        internal RackCustomPropertiesUnifyOption(string sourceHandle, IReadOnlyList<string> blockingHandles)
        {
            SourceHandle = sourceHandle;
            BlockingHandles = blockingHandles;
        }

        public string SourceHandle { get; }

        /// <summary>The handles of the views that would be overwritten and must not be, in handle order.</summary>
        public IReadOnlyList<string> BlockingHandles { get; }

        public bool IsAvailable => BlockingHandles.Count == 0;
    }

    /// <summary>
    /// What the authority decided about the picked rack (D-09). Only <see cref="RackCustomPropertiesAuthorityOutcome.Single"/>
    /// carries <see cref="Collection"/> and <see cref="WriteVersion"/>: no other result ever exposes one view's values as the
    /// rack's (INV-11).
    /// </summary>
    public sealed class RackCustomPropertiesAuthorityResult
    {
        private static readonly IReadOnlyList<RackCustomPropertiesMember> NoMembers = Array.Empty<RackCustomPropertiesMember>();

        private static readonly IReadOnlyList<RackCustomPropertiesDefinition> NoDefinitions = Array.Empty<RackCustomPropertiesDefinition>();

        private static readonly IReadOnlyList<RackCustomPropertiesUnifyOption> NoOptions = Array.Empty<RackCustomPropertiesUnifyOption>();

        private RackCustomPropertiesAuthorityResult(
            RackCustomPropertiesAuthorityOutcome outcome,
            string selectedHandle,
            RackEmbedDocument selectedEnvelope,
            string kind,
            IReadOnlyList<RackCustomPropertiesMember> members,
            IReadOnlyList<RackCustomPropertiesDefinition> uninterpretable,
            IReadOnlyList<RackCustomPropertiesDefinition> blankKind,
            IReadOnlyList<RackCustomPropertiesUnifyOption> unifyOptions,
            CustomPropertiesReadResult collection,
            string writeVersion,
            string error)
        {
            Outcome = outcome;
            SelectedHandle = selectedHandle;
            RackId = selectedEnvelope == null || string.IsNullOrWhiteSpace(selectedEnvelope.Id) ? null : selectedEnvelope.Id;
            RackName = selectedEnvelope?.Name;
            Kind = kind;
            Members = members ?? NoMembers;
            UninterpretableDefinitions = uninterpretable ?? NoDefinitions;
            BlankKindDefinitions = blankKind ?? NoDefinitions;
            UnifyOptions = unifyOptions ?? NoOptions;
            Collection = collection;
            WriteVersion = writeVersion;
            Error = error;
        }

        public RackCustomPropertiesAuthorityOutcome Outcome { get; }

        public string SelectedHandle { get; }

        /// <summary>The rack id of the picked envelope; null when the pick is an xref, has no identity or cannot be read.</summary>
        public string RackId { get; }

        /// <summary>The name in the picked envelope, for labels; null when the pick cannot be read.</summary>
        public string RackName { get; }

        /// <summary>The common kind of the members, when there is one.</summary>
        public string Kind { get; }

        /// <summary>
        /// The members, in handle order, each with its own collection. Empty when the pick has no rack id. Under
        /// <see cref="RackCustomPropertiesAuthorityOutcome.IndeterminateMembership"/> these are only the interpretable members
        /// found: the complete set cannot be known.
        /// </summary>
        public IReadOnlyList<RackCustomPropertiesMember> Members { get; }

        /// <summary>The definitions that make membership indeterminate, in handle order, for the diagnostic (D-09.3).</summary>
        public IReadOnlyList<RackCustomPropertiesDefinition> UninterpretableDefinitions { get; }

        /// <summary>The members that declare no kind, for the diagnostic of <see cref="RackCustomPropertiesAuthorityOutcome.MixedKind"/>.</summary>
        public IReadOnlyList<RackCustomPropertiesDefinition> BlankKindDefinitions { get; }

        /// <summary>One option per member when the result is <see cref="RackCustomPropertiesAuthorityOutcome.Divergent"/>; empty otherwise.</summary>
        public IReadOnlyList<RackCustomPropertiesUnifyOption> UnifyOptions { get; }

        /// <summary>The rack's collection, with <see cref="WriteVersion"/> as its version. Only for Single.</summary>
        public CustomPropertiesReadResult Collection { get; }

        /// <summary>The highest minor among the members, never lower than the current version (D-09.8). Only for Single.</summary>
        public string WriteVersion { get; }

        /// <summary>The visible reason for any result other than Single.</summary>
        public string Error { get; }

        public bool IsWritable => Outcome == RackCustomPropertiesAuthorityOutcome.Single;

        internal static RackCustomPropertiesAuthorityResult Create(
            RackCustomPropertiesAuthorityOutcome outcome,
            string selectedHandle,
            RackEmbedDocument selectedEnvelope,
            string error,
            string kind = null,
            IReadOnlyList<RackCustomPropertiesMember> members = null,
            IReadOnlyList<RackCustomPropertiesDefinition> uninterpretable = null,
            IReadOnlyList<RackCustomPropertiesDefinition> blankKind = null,
            IReadOnlyList<RackCustomPropertiesUnifyOption> unifyOptions = null,
            CustomPropertiesReadResult collection = null,
            string writeVersion = null)
            => new RackCustomPropertiesAuthorityResult(
                outcome, selectedHandle, selectedEnvelope, kind, members, uninterpretable, blankKind, unifyOptions, collection, writeVersion, error);
    }

    /// <summary>
    /// The rack authority for custom properties (I-54 D-09 / ADR-0039 §8): pure Application over the flat projection of
    /// the scan (D-22), with the known-kind predicate injected from the edge. It never picks a sibling, never interprets a
    /// design and never changes an envelope; the scan order does not change its answer.
    /// </summary>
    public static class RackCustomPropertiesAuthority
    {
        public static RackCustomPropertiesAuthorityResult Evaluate(
            IEnumerable<RackCustomPropertiesDefinition> definitions,
            RackCustomPropertiesSelection selection,
            Func<string, bool> isKnownKind)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            if (isKnownKind == null)
            {
                throw new ArgumentNullException(nameof(isKnownKind));
            }

            var scanned = Validated(definitions);
            var handle = selection.DefinitionHandle;

            // 1. XrefRejected: decided on the pick, before looking at the rest of the drawing.
            if (selection.IsFromExternalReference)
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.XrefRejected, handle, null, XrefMessage);
            }

            var selected = scanned.FirstOrDefault(definition => string.Equals(definition.Handle, handle, StringComparison.Ordinal));

            if (selected == null)
            {
                throw new ArgumentException("La definición elegida no está en la proyección del barrido.", nameof(selection));
            }

            if (selected.IsDependent)
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.XrefRejected, handle, null, XrefMessage);
            }

            // 2. NoIdentity: a readable pick with a blank rack id belongs to no rack.
            if (selected.IsInterpretable && string.IsNullOrWhiteSpace(selected.Envelope.Id))
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.NoIdentity,
                    handle,
                    selected.Envelope,
                    "El bloque elegido tiene datos de RackCad pero no la identidad de un rack (Id en blanco): no pertenece a "
                    + "ningún rack y sus propiedades quedan en solo lectura.");
            }

            // 3. IndeterminateMembership: without its identity, no uninterpretable payload can be shown not to be a sibling.
            var uninterpretable = scanned
                .Where(definition => !definition.IsDependent && !definition.IsInterpretable)
                .OrderBy(definition => definition.Handle, StringComparer.Ordinal)
                .ToList();

            if (uninterpretable.Count > 0)
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.IndeterminateMembership,
                    handle,
                    selected.Envelope,
                    IndeterminateMessage(uninterpretable),
                    members: selected.IsInterpretable ? MembersOf(scanned, selected.Envelope.Id) : null,
                    uninterpretable: uninterpretable);
            }

            var members = MembersOf(scanned, selected.Envelope.Id);

            // 4. MixedKind: a blank kind, or more than one kind, known or not.
            var blankKind = members.Where(member => string.IsNullOrWhiteSpace(member.Kind)).Select(member => member.Definition).ToList();

            if (blankKind.Count > 0)
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.MixedKind,
                    handle,
                    selected.Envelope,
                    BlankKindMessage(blankKind),
                    members: members,
                    blankKind: blankKind);
            }

            var kinds = members.Select(member => member.Kind).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (kinds.Count > 1)
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.MixedKind,
                    handle,
                    selected.Envelope,
                    "Las vistas de este rack declaran tipos distintos (" + string.Join(", ", kinds) + "): sus propiedades quedan "
                    + "en solo lectura.",
                    members: members);
            }

            // 5. UnknownKind: never writable, whatever the collections say (C-1).
            var kind = members[0].Kind;

            if (!isKnownKind(kind))
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.UnknownKind,
                    handle,
                    selected.Envelope,
                    "Este build no conoce el tipo de rack «" + kind + "»: sus propiedades se muestran en solo lectura.",
                    kind: kind,
                    members: members);
            }

            // 6. CustomPropertiesReadOnly: an unwritable member is never skipped to carry on with its siblings.
            var unwritable = members.Where(member => !member.Collection.CanWrite).ToList();

            if (unwritable.Count > 0)
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.CustomPropertiesReadOnly,
                    handle,
                    selected.Envelope,
                    ReadOnlyMessage(unwritable),
                    kind: kind,
                    members: members);
            }

            // 7. Divergent: no collection of the rack; only a safe unify, chosen by the user, may write.
            if (members.Select(member => member.CanonicalForm).Distinct(StringComparer.Ordinal).Count() > 1)
            {
                return RackCustomPropertiesAuthorityResult.Create(
                    RackCustomPropertiesAuthorityOutcome.Divergent,
                    handle,
                    selected.Envelope,
                    "Las vistas de este rack tienen propiedades distintas: no hay una colección común que editar.",
                    kind: kind,
                    members: members,
                    unifyOptions: UnifyOptionsOf(members));
            }

            // 8. Single.
            var writeVersion = WriteVersionOf(members);

            return RackCustomPropertiesAuthorityResult.Create(
                RackCustomPropertiesAuthorityOutcome.Single,
                handle,
                selected.Envelope,
                null,
                kind: kind,
                members: members,
                collection: CollectionOf(members, writeVersion),
                writeVersion: writeVersion);
        }

        private const string XrefMessage =
            "El bloque elegido depende de una referencia externa: sus propiedades pertenecen a otro dibujo y no se editan aquí.";

        private static IReadOnlyList<RackCustomPropertiesDefinition> Validated(IEnumerable<RackCustomPropertiesDefinition> definitions)
        {
            var scanned = definitions.ToList();

            if (scanned.Any(definition => definition == null))
            {
                throw new ArgumentException("La proyección del barrido contiene una definición nula.", nameof(definitions));
            }

            var repeated = scanned
                .GroupBy(definition => definition.Handle, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);

            if (repeated != null)
            {
                throw new ArgumentException("La proyección del barrido repite el handle " + repeated.Key + ".", nameof(definitions));
            }

            return scanned;
        }

        /// <summary>
        /// Members of <paramref name="rackId"/> (D-09.2): readable envelope, non-blank id equal with OrdinalIgnoreCase, not
        /// dependent on an external reference. In handle order, each with the collection its own envelope carries.
        /// </summary>
        private static IReadOnlyList<RackCustomPropertiesMember> MembersOf(
            IReadOnlyList<RackCustomPropertiesDefinition> scanned, string rackId)
        {
            var store = new CustomPropertiesStore();

            return scanned
                .Where(definition => definition.IsInterpretable
                                     && !definition.IsDependent
                                     && !string.IsNullOrWhiteSpace(definition.Envelope.Id)
                                     && string.Equals(definition.Envelope.Id, rackId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(definition => definition.Handle, StringComparer.Ordinal)
                .Select(definition => new RackCustomPropertiesMember(definition, store.ReadElement(definition.Envelope.CustomProperties)))
                .ToList();
        }

        /// <summary>Every member's minor folds into the highest one, starting from the current version (D-09.8, PR-10).</summary>
        private static string WriteVersionOf(IReadOnlyList<RackCustomPropertiesMember> members)
        {
            var version = CustomPropertiesDocument.CurrentSchemaVersion;

            foreach (var member in members.Where(member => member.Collection.Outcome == CustomPropertiesReadOutcome.Readable))
            {
                version = SchemaVersionPolicy.ResolveWriteVersion(member.Collection.Document.SchemaVersion, version);
            }

            return version;
        }

        /// <summary>
        /// The rack's collection for Single. All members are canonically equal, so any readable one has the content; the first
        /// in handle order is read AGAIN, into a document nobody else holds, and gets the write version. With no readable
        /// member the collection is the empty one.
        /// </summary>
        private static CustomPropertiesReadResult CollectionOf(IReadOnlyList<RackCustomPropertiesMember> members, string writeVersion)
        {
            var readable = members.FirstOrDefault(member => member.Collection.Outcome == CustomPropertiesReadOutcome.Readable);

            if (readable == null)
            {
                return CustomPropertiesReadResult.Absent();
            }

            var collection = new CustomPropertiesStore().ReadElement(readable.Definition.Envelope.CustomProperties);
            collection.Document.SchemaVersion = writeVersion;
            return collection;
        }

        private static IReadOnlyList<RackCustomPropertiesUnifyOption> UnifyOptionsOf(IReadOnlyList<RackCustomPropertiesMember> members)
            => members
                .Select(source => new RackCustomPropertiesUnifyOption(
                    source.Handle,
                    members
                        .Where(target => !ReferenceEquals(target, source)
                                         && !string.Equals(target.CanonicalForm, source.CanonicalForm, StringComparison.Ordinal)
                                         && (CustomPropertiesCanonicalForm.HasExtensionData(target.Collection.Document)
                                             || CustomPropertiesCanonicalForm.MinorOf(target.Collection)
                                             > CustomPropertiesCanonicalForm.MinorOf(source.Collection)))
                        .Select(target => target.Handle)
                        .ToList()))
                .ToList();

        private static string Describe(RackCustomPropertiesDefinition definition)
            => "bloque «" + definition.BlockName + "» (handle " + definition.Handle + ", " + (definition.IsPlaced ? "colocado" : "sin colocar") + ")";

        private static string IndeterminateMessage(IReadOnlyList<RackCustomPropertiesDefinition> uninterpretable)
            => "Hay datos de RackCad que este build no puede interpretar en " + uninterpretable.Count + " definición(es): "
               + string.Join("; ", uninterpretable.Select(Describe))
               + ". Sin su identidad no se puede demostrar que no pertenezcan a este rack, así que las propiedades de rack de "
               + "este dibujo quedan en solo lectura.";

        private static string BlankKindMessage(IReadOnlyList<RackCustomPropertiesDefinition> blankKind)
            => "Alguna vista no declara el tipo de rack: " + string.Join("; ", blankKind.Select(Describe))
               + ". Mientras sea así, las propiedades de este rack quedan en solo lectura; ese bloque se puede revisar con "
               + "RACKEDITAR.";

        private static string ReadOnlyMessage(IReadOnlyList<RackCustomPropertiesMember> unwritable)
            => "Alguna vista tiene propiedades que no se pueden escribir, así que las del rack quedan en solo lectura: "
               + string.Join("; ", unwritable.Select(member => Describe(member.Definition) + ": " + member.Collection.Error)) + ".";
    }
}
