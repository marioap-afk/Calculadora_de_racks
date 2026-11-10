#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace.Drafts
{
    /// <summary>
    /// Membership of one definition key with respect to the rack a draft belongs to (I-64 D-12). Four values only:
    /// the Redraw/Erase split is NOT part of the base, it is recomputed from the current draft state when writing.
    /// </summary>
    public enum DraftMembership
    {
        Mutable,
        ReadOnly,
        Blocking,
        NotMember,
    }

    /// <summary>
    /// What the caller read for ONE definition key: its membership with identity, View and Section, and the exact raw
    /// text of its envelope when it is an interpretable member. There is deliberately no reference count here (INV-29).
    /// </summary>
    public readonly record struct DraftStaleBaseEntry(string Key, DraftMembership Membership, string? View, string? Section, string? EnvelopeText);

    /// <summary>The kind of one difference found by <see cref="DraftStaleBase.CompareWith"/>.</summary>
    public enum DraftDifferenceKind
    {
        Membership,
        View,
        Section,
        EnvelopeText,
        VariableRegistry,
    }

    /// <summary>One difference: the definition key (null for the variable registry) and what kind of fact changed.</summary>
    public readonly record struct DraftDifference(string? Key, DraftDifferenceKind Kind);

    /// <summary>Result of comparing a stale base with a re-read: a MATCH, or the list of what changed.</summary>
    public sealed class DraftStaleComparison
    {
        internal DraftStaleComparison(IReadOnlyList<DraftDifference> differences)
        {
            Differences = differences;
        }

        public bool IsMatch => Differences.Count == 0;

        public IReadOnlyList<DraftDifference> Differences { get; }
    }

    /// <summary>RED skeleton: every operation is inert.</summary>
    public sealed class DraftStaleBase
    {
        private DraftStaleBase()
        {
        }

        public static DraftStaleBase Create(IEnumerable<DraftStaleBaseEntry> entries, string? variableRegistryText = null) => new();

        public IReadOnlyDictionary<string, DraftStaleBaseEntry> Entries { get; } = new Dictionary<string, DraftStaleBaseEntry>();

        public string? VariableRegistryText => null;

        public DraftMembership MembershipOf(string key) => DraftMembership.NotMember;

        public DraftStaleComparison CompareWith(DraftStaleBase reread) => new(Array.Empty<DraftDifference>());
    }
}
