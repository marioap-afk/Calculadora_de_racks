#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    /// <summary>
    /// The facts captured by one logical read that serve ONLY to detect external changes (D-12, INV-07). Immutable and in
    /// memory. No public member builds an editable state, a draft or any write data from it.
    /// </summary>
    public sealed class DraftStaleBase
    {
        private static readonly DraftStaleBaseEntry NoEntry = new(string.Empty, DraftMembership.NotMember, null, null, null);

        private readonly Dictionary<string, DraftStaleBaseEntry> _entries;
        private readonly ReadOnlyDictionary<string, DraftStaleBaseEntry> _view;

        private DraftStaleBase(Dictionary<string, DraftStaleBaseEntry> entries, string? variableRegistryText)
        {
            _entries = entries;
            _view = new ReadOnlyDictionary<string, DraftStaleBaseEntry>(entries);
            VariableRegistryText = variableRegistryText;
        }

        /// <summary>
        /// Builds a base. A NotMember entry is dropped (absent equals NotMember and its text is never kept); the envelope
        /// text is kept exactly as given for the other memberships; a repeated key keeps the last entry.
        /// </summary>
        public static DraftStaleBase Create(IEnumerable<DraftStaleBaseEntry> entries, string? variableRegistryText = null)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            var kept = new Dictionary<string, DraftStaleBaseEntry>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Key)) throw new ArgumentException("A definition key is required.", nameof(entries));
                if (entry.Membership == DraftMembership.NotMember) kept.Remove(entry.Key);
                else kept[entry.Key] = entry;
            }

            return new DraftStaleBase(kept, variableRegistryText);
        }

        /// <summary>The recorded entries by definition key (ordinal). Keys of NotMember definitions are not present.</summary>
        public IReadOnlyDictionary<string, DraftStaleBaseEntry> Entries => _view;

        /// <summary>The exact raw text of the variable registry when the adapter supplied it; otherwise null.</summary>
        public string? VariableRegistryText { get; }

        /// <summary>The membership recorded for a key; a key that is not recorded is NotMember.</summary>
        public DraftMembership MembershipOf(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("A definition key is required.", nameof(key));
            return _entries.TryGetValue(key, out var entry) ? entry.Membership : DraftMembership.NotMember;
        }

        /// <summary>
        /// Compares this base with a re-read over the UNION of their keys; an absent key is NotMember. Every comparison is
        /// ordinal and exact, with no normalization. Differences come ordered by key (ordinal), then by kind; the registry
        /// difference, which has no key, comes last.
        /// </summary>
        public DraftStaleComparison CompareWith(DraftStaleBase reread)
        {
            if (reread == null) throw new ArgumentNullException(nameof(reread));
            var differences = new List<DraftDifference>();
            var keys = new SortedSet<string>(_entries.Keys, StringComparer.Ordinal);
            keys.UnionWith(reread._entries.Keys);
            foreach (var key in keys)
            {
                var before = _entries.TryGetValue(key, out var b) ? b : NoEntry;
                var after = reread._entries.TryGetValue(key, out var a) ? a : NoEntry;
                if (before.Membership != after.Membership) differences.Add(new DraftDifference(key, DraftDifferenceKind.Membership));
                if (!string.Equals(before.View, after.View, StringComparison.Ordinal)) differences.Add(new DraftDifference(key, DraftDifferenceKind.View));
                if (!string.Equals(before.Section, after.Section, StringComparison.Ordinal)) differences.Add(new DraftDifference(key, DraftDifferenceKind.Section));
                if (!string.Equals(before.EnvelopeText, after.EnvelopeText, StringComparison.Ordinal)) differences.Add(new DraftDifference(key, DraftDifferenceKind.EnvelopeText));
            }

            if (!string.Equals(VariableRegistryText, reread.VariableRegistryText, StringComparison.Ordinal))
                differences.Add(new DraftDifference(null, DraftDifferenceKind.VariableRegistry));

            return new DraftStaleComparison(differences.ToArray());
        }
    }
}
