using System;
using System.Collections.Generic;

namespace RackCad.Application.Views.Redraw
{
    public enum RackSiblingMembershipKind
    {
        Redraw,
        ReadOnly,
        Erase,
        NotMember,
        BlockingUnreadable
    }

    /// <summary>Product projection of one definition from the single AUTH-06 scan.</summary>
    public sealed class RackSiblingScanFact
    {
        public RackSiblingScanFact(
            string definitionKey,
            string view,
            bool isSelectedSource,
            bool isXrefDependent,
            bool envelopeInterpretable,
            string rackId,
            string probeId,
            int layoutReferenceCount,
            int nestedReferenceCount,
            bool eraseByKind)
        {
            DefinitionKey = definitionKey;
            View = view;
            IsSelectedSource = isSelectedSource;
            IsXrefDependent = isXrefDependent;
            EnvelopeInterpretable = envelopeInterpretable;
            RackId = rackId;
            ProbeId = probeId;
            LayoutReferenceCount = layoutReferenceCount;
            NestedReferenceCount = nestedReferenceCount;
            EraseByKind = eraseByKind;
        }

        public string DefinitionKey { get; }
        public string View { get; }
        public bool IsSelectedSource { get; }
        public bool IsXrefDependent { get; }
        public bool EnvelopeInterpretable { get; }
        public string RackId { get; }
        public string ProbeId { get; }
        public int LayoutReferenceCount { get; }
        public int NestedReferenceCount { get; }
        public bool EraseByKind { get; }
    }

    public sealed class RackSiblingMember
    {
        internal RackSiblingMember(RackSiblingScanFact fact, RackSiblingMembershipKind kind)
        {
            Fact = fact;
            Kind = kind;
        }

        public RackSiblingScanFact Fact { get; }
        public RackSiblingMembershipKind Kind { get; }
        public bool IsSurvivor => Kind == RackSiblingMembershipKind.Redraw && Fact.LayoutReferenceCount >= 1;
    }

    public sealed class RackSiblingMembershipSnapshot
    {
        internal RackSiblingMembershipSnapshot(IReadOnlyList<RackSiblingMember> members)
        {
            Members = members;
            var mutable = new List<RackSiblingMember>();
            foreach (var member in members)
            {
                if (member.Kind == RackSiblingMembershipKind.Redraw || member.Kind == RackSiblingMembershipKind.Erase)
                {
                    mutable.Add(member);
                }
            }

            MutableMembers = mutable;
        }

        public IReadOnlyList<RackSiblingMember> Members { get; }
        /// <summary>The one membership set consumed by redraw, CustomProperties and authored gates.</summary>
        public IReadOnlyList<RackSiblingMember> MutableMembers { get; }
        public IReadOnlyList<RackSiblingMember> CustomPropertiesGateMembers => MutableMembers;
        public IReadOnlyList<RackSiblingMember> AuthoredGateMembers => MutableMembers;
    }

    public static class RackSiblingMembership
    {
        public static RackSiblingMembershipSnapshot Classify(
            IReadOnlyList<RackSiblingScanFact> scan,
            string rackId,
            string originalSourceId,
            bool originalSourceIdIsAttributable)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            if (string.IsNullOrWhiteSpace(rackId)) throw new ArgumentException("A cured RackId is required.", nameof(rackId));

            var attributable = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rackId };
            if (originalSourceIdIsAttributable && !string.IsNullOrEmpty(originalSourceId))
            {
                attributable.Add(originalSourceId);
            }

            var result = new List<RackSiblingMember>(scan.Count);
            var definitionKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var fact in scan)
            {
                if (fact == null || string.IsNullOrWhiteSpace(fact.DefinitionKey))
                {
                    throw new ArgumentException("Every scan fact needs a stable definition key.", nameof(scan));
                }

                if (!definitionKeys.Add(fact.DefinitionKey))
                {
                    throw new ArgumentException("A definition cannot occur twice in one scan: " + fact.DefinitionKey, nameof(scan));
                }

                if (fact.LayoutReferenceCount < 0 || fact.NestedReferenceCount < 0)
                {
                    throw new ArgumentException("Reference counts cannot be negative.", nameof(scan));
                }

                RackSiblingMembershipKind kind;
                if (fact.IsSelectedSource)
                {
                    kind = fact.EraseByKind ? RackSiblingMembershipKind.Erase : RackSiblingMembershipKind.Redraw;
                }
                else if (!fact.EnvelopeInterpretable)
                {
                    kind = !fact.IsXrefDependent && !string.IsNullOrEmpty(fact.ProbeId) && attributable.Contains(fact.ProbeId)
                        ? RackSiblingMembershipKind.BlockingUnreadable
                        : RackSiblingMembershipKind.NotMember;
                }
                else if (string.IsNullOrEmpty(fact.RackId) || !attributable.Contains(fact.RackId))
                {
                    kind = RackSiblingMembershipKind.NotMember;
                }
                else if (fact.IsXrefDependent)
                {
                    kind = RackSiblingMembershipKind.ReadOnly;
                }
                else
                {
                    kind = fact.EraseByKind ? RackSiblingMembershipKind.Erase : RackSiblingMembershipKind.Redraw;
                }

                result.Add(new RackSiblingMember(fact, kind));
            }

            return new RackSiblingMembershipSnapshot(result);
        }
    }
}
