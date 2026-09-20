using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;

namespace RackCad.Application.Systems.Shared
{
    public enum RackPhysicalSpace
    {
        Unknown,
        ModelSpace,
        PaperSpace,
        ReferenceSpace
    }

    public enum RackPhysicalKindDisposition
    {
        Known,
        Foreign,
        Unknown
    }

    public enum RackPhysicalIdentityFact
    {
        Present,
        Missing
    }

    public enum RackPhysicalMemberDisposition
    {
        Selected,
        NotBlock,
        WrongSpace,
        Xref,
        MissingDefinition,
        MissingRackData,
        Unreadable,
        Foreign,
        UnknownKind
    }

    public enum RackPhysicalFailure
    {
        None,
        EnvelopeUnreadable,
        MissingKind,
        MissingDesign
    }

    /// <summary>
    /// Physical facts measured for one selected entity. Keys are stable descriptive values supplied by the probe;
    /// they are never live AutoCAD handles or objects. Space, xref and origin are observed facts, not selection policy.
    /// </summary>
    public sealed class RackPhysicalReferenceSnapshot
    {
        public RackPhysicalReferenceSnapshot(
            string physicalKey,
            bool isBlockReference,
            RackPhysicalSpace space,
            bool isExternalReference,
            string definitionKey,
            Point2D origin)
        {
            PhysicalKey = physicalKey;
            IsBlockReference = isBlockReference;
            Space = space;
            IsExternalReference = isExternalReference;
            DefinitionKey = definitionKey;
            Origin = origin;
        }

        /// <summary>Compatibility shape for probes that only distinguish Model Space from every other space.</summary>
        public RackPhysicalReferenceSnapshot(
            string physicalKey, bool isBlockReference, bool isInModelSpace, string definitionKey)
            : this(
                physicalKey,
                isBlockReference,
                isInModelSpace ? RackPhysicalSpace.ModelSpace : RackPhysicalSpace.PaperSpace,
                false,
                definitionKey,
                new Point2D(0.0, 0.0))
        {
        }

        public string PhysicalKey { get; }
        public string ReferenceKey => PhysicalKey;
        public bool IsBlockReference { get; }
        public RackPhysicalSpace Space { get; }
        public bool IsExternalReference { get; }
        public string DefinitionKey { get; }
        public Point2D Origin { get; }
        public bool IsInModelSpace => Space == RackPhysicalSpace.ModelSpace;
    }

    /// <summary>Physical definition and raw envelope facts measured by a Plugin probe.</summary>
    public sealed class RackPhysicalDefinitionSnapshot
    {
        public RackPhysicalDefinitionSnapshot(
            string definitionKey,
            string rawPayload,
            string definitionName,
            int directReferenceCount = 0)
        {
            if (directReferenceCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(directReferenceCount));
            }

            DefinitionKey = definitionKey;
            RawPayload = rawPayload;
            DefinitionName = definitionName;
            DirectReferenceCount = directReferenceCount;
        }

        public string DefinitionKey { get; }
        public string RawPayload { get; }
        public string DefinitionName { get; }
        public int DirectReferenceCount { get; }
    }

    /// <summary>
    /// Neutral interpretation of one physical member. It reports what was observed and why a member is unavailable;
    /// it never assigns identities, copy counts, names or consumer intent.
    /// </summary>
    public sealed class RackPhysicalMemberFacts
    {
        internal RackPhysicalMemberFacts(
            RackPhysicalReferenceSnapshot reference,
            RackPhysicalDefinitionSnapshot definition,
            RackEmbedDocument envelope,
            RackPhysicalMemberDisposition disposition,
            RackPhysicalFailure failure,
            RackPhysicalIdentityFact identity,
            string rackId,
            string kind,
            string rackName,
            string design,
            bool hasViewSyntax,
            RackViewSyntax viewSyntax,
            bool hasAddress,
            RackViewAddress address)
        {
            Reference = reference;
            Definition = definition;
            Envelope = envelope;
            Disposition = disposition;
            Failure = failure;
            Identity = identity;
            RackId = rackId;
            Kind = kind;
            RackName = rackName;
            Design = design;
            HasViewSyntax = hasViewSyntax;
            ViewSyntax = viewSyntax;
            HasAddress = hasAddress;
            Address = address;
        }

        internal RackPhysicalReferenceSnapshot Reference { get; }
        internal RackPhysicalDefinitionSnapshot Definition { get; }
        internal RackEmbedDocument Envelope { get; }
        public string PhysicalKey => Reference.PhysicalKey;
        public string DefinitionKey => Reference.DefinitionKey;
        public Point2D Origin => Reference.Origin;
        public RackPhysicalSpace Space => Reference.Space;
        public bool IsExternalReference => Reference.IsExternalReference;
        public int DirectReferenceCount => Definition?.DirectReferenceCount ?? 0;
        public RackPhysicalMemberDisposition Disposition { get; }
        public RackPhysicalFailure Failure { get; }
        public RackPhysicalIdentityFact Identity { get; }
        public string RackId { get; }
        public string Kind { get; }
        public string RackName { get; }
        public string Design { get; }
        public bool HasViewSyntax { get; }
        public RackViewSyntax ViewSyntax { get; }
        public bool HasAddress { get; }
        public RackViewAddress Address { get; }
        public bool IsSelected => Disposition == RackPhysicalMemberDisposition.Selected;
    }

    public sealed class RackPhysicalRackGroup
    {
        internal RackPhysicalRackGroup(string rackId, IReadOnlyList<RackPhysicalMemberFacts> members)
        {
            RackId = rackId;
            Members = members;
        }

        public string RackId { get; }
        public IReadOnlyList<RackPhysicalMemberFacts> Members { get; }
    }

    /// <summary>
    /// Stable, deduplicated selection facts. Grouping exists only where a RackId was observed; a missing identity is
    /// kept as missing and is never promoted to a new or generated identity.
    /// </summary>
    public sealed class RackPhysicalSelection
    {
        private RackPhysicalSelection(
            IReadOnlyList<RackPhysicalMemberFacts> members,
            IReadOnlyList<RackPhysicalMemberFacts> selectedMembers,
            IReadOnlyList<RackPhysicalRackGroup> rackGroups,
            int duplicatePhysicalMembers)
        {
            Members = members;
            SelectedMembers = selectedMembers;
            RackGroups = rackGroups;
            DuplicatePhysicalMembers = duplicatePhysicalMembers;
        }

        public IReadOnlyList<RackPhysicalMemberFacts> Members { get; }
        public IReadOnlyList<RackPhysicalMemberFacts> SelectedMembers { get; }
        public IReadOnlyList<RackPhysicalRackGroup> RackGroups { get; }
        public int DuplicatePhysicalMembers { get; }

        public static RackPhysicalSelection Classify(
            IReadOnlyList<RackPhysicalReferenceSnapshot> references,
            IReadOnlyList<RackPhysicalDefinitionSnapshot> definitions,
            Func<string, RackPhysicalKindDisposition> classifyKind)
        {
            if (references == null) throw new ArgumentNullException(nameof(references));
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            if (classifyKind == null) throw new ArgumentNullException(nameof(classifyKind));

            var definitionIndex = IndexDefinitions(definitions);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var members = new List<RackPhysicalMemberFacts>();
            var selected = new List<RackPhysicalMemberFacts>();
            var groupBuilders = new List<GroupBuilder>();
            var groupsById = new Dictionary<string, GroupBuilder>(StringComparer.OrdinalIgnoreCase);
            var duplicates = 0;

            foreach (var reference in references)
            {
                if (reference == null || string.IsNullOrEmpty(reference.PhysicalKey))
                {
                    throw new ArgumentException("Every physical member needs a stable key.", nameof(references));
                }

                if (!seen.Add(reference.PhysicalKey))
                {
                    duplicates++;
                    continue;
                }

                var facts = Classify(reference, definitionIndex, classifyKind);
                members.Add(facts);
                if (!facts.IsSelected)
                {
                    continue;
                }

                selected.Add(facts);
                if (facts.Identity != RackPhysicalIdentityFact.Present)
                {
                    continue;
                }

                if (!groupsById.TryGetValue(facts.RackId, out var builder))
                {
                    builder = new GroupBuilder(facts.RackId);
                    groupsById.Add(facts.RackId, builder);
                    groupBuilders.Add(builder);
                }

                builder.Members.Add(facts);
            }

            var groups = new List<RackPhysicalRackGroup>(groupBuilders.Count);
            foreach (var builder in groupBuilders)
            {
                groups.Add(new RackPhysicalRackGroup(builder.RackId, builder.Members));
            }

            return new RackPhysicalSelection(members, selected, groups, duplicates);
        }

        private static RackPhysicalMemberFacts Classify(
            RackPhysicalReferenceSnapshot reference,
            IReadOnlyDictionary<string, RackPhysicalDefinitionSnapshot> definitions,
            Func<string, RackPhysicalKindDisposition> classifyKind)
        {
            if (!reference.IsBlockReference)
            {
                return Unavailable(reference, null, RackPhysicalMemberDisposition.NotBlock);
            }

            if (reference.Space != RackPhysicalSpace.ModelSpace)
            {
                return Unavailable(reference, null, RackPhysicalMemberDisposition.WrongSpace);
            }

            if (reference.IsExternalReference)
            {
                return Unavailable(reference, null, RackPhysicalMemberDisposition.Xref);
            }

            if (string.IsNullOrEmpty(reference.DefinitionKey)
                || !definitions.TryGetValue(reference.DefinitionKey, out var definition))
            {
                return Unavailable(reference, null, RackPhysicalMemberDisposition.MissingDefinition);
            }

            if (string.IsNullOrEmpty(definition.RawPayload))
            {
                return Unavailable(reference, definition, RackPhysicalMemberDisposition.MissingRackData);
            }

            var envelope = new RackEmbedStore().Deserialize(definition.RawPayload);
            if (envelope == null)
            {
                return Unavailable(
                    reference, definition, RackPhysicalMemberDisposition.Unreadable, RackPhysicalFailure.EnvelopeUnreadable);
            }

            if (string.IsNullOrWhiteSpace(envelope.Kind))
            {
                return EnvelopeFacts(
                    reference,
                    definition,
                    envelope,
                    RackPhysicalMemberDisposition.UnknownKind,
                    RackPhysicalFailure.MissingKind);
            }

            var kindDisposition = classifyKind(envelope.Kind);
            if (kindDisposition == RackPhysicalKindDisposition.Foreign)
            {
                return EnvelopeFacts(reference, definition, envelope, RackPhysicalMemberDisposition.Foreign);
            }

            if (kindDisposition == RackPhysicalKindDisposition.Unknown)
            {
                return EnvelopeFacts(reference, definition, envelope, RackPhysicalMemberDisposition.UnknownKind);
            }

            if (string.IsNullOrWhiteSpace(envelope.Design))
            {
                return EnvelopeFacts(
                    reference,
                    definition,
                    envelope,
                    RackPhysicalMemberDisposition.Unreadable,
                    RackPhysicalFailure.MissingDesign);
            }

            return EnvelopeFacts(reference, definition, envelope, RackPhysicalMemberDisposition.Selected);
        }

        private static RackPhysicalMemberFacts EnvelopeFacts(
            RackPhysicalReferenceSnapshot reference,
            RackPhysicalDefinitionSnapshot definition,
            RackEmbedDocument envelope,
            RackPhysicalMemberDisposition disposition,
            RackPhysicalFailure failure = RackPhysicalFailure.None)
        {
            var syntax = new RackViewSyntax(envelope.Kind, envelope.View, envelope.Section);
            var decoded = RackViewCodec.Decode(envelope.Kind, envelope.View, envelope.Section);
            var rackId = string.IsNullOrWhiteSpace(envelope.Id) ? null : envelope.Id;
            return new RackPhysicalMemberFacts(
                reference,
                definition,
                envelope,
                disposition,
                failure,
                rackId == null ? RackPhysicalIdentityFact.Missing : RackPhysicalIdentityFact.Present,
                rackId,
                envelope.Kind,
                envelope.Name,
                envelope.Design,
                true,
                syntax,
                decoded.HasAddress,
                decoded.Address);
        }

        private static RackPhysicalMemberFacts Unavailable(
            RackPhysicalReferenceSnapshot reference,
            RackPhysicalDefinitionSnapshot definition,
            RackPhysicalMemberDisposition disposition,
            RackPhysicalFailure failure = RackPhysicalFailure.None)
            => new RackPhysicalMemberFacts(
                reference,
                definition,
                null,
                disposition,
                failure,
                RackPhysicalIdentityFact.Missing,
                null,
                null,
                null,
                null,
                false,
                default,
                false,
                default);

        private static Dictionary<string, RackPhysicalDefinitionSnapshot> IndexDefinitions(
            IReadOnlyList<RackPhysicalDefinitionSnapshot> definitions)
        {
            var index = new Dictionary<string, RackPhysicalDefinitionSnapshot>(StringComparer.Ordinal);
            foreach (var definition in definitions)
            {
                if (definition == null || string.IsNullOrEmpty(definition.DefinitionKey))
                {
                    throw new ArgumentException("Every definition snapshot needs a stable key.", nameof(definitions));
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

        private sealed class GroupBuilder
        {
            internal GroupBuilder(string rackId)
            {
                RackId = rackId;
            }

            internal string RackId { get; }
            internal List<RackPhysicalMemberFacts> Members { get; } = new List<RackPhysicalMemberFacts>();
        }
    }
}
