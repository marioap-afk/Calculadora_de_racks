using System;
using System.Collections.Generic;
using System.Text.Json;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>Whether the views of one rack agree on a single authored state.</summary>
    public enum AuthoredAuthorityOutcome
    {
        /// <summary>Every sibling was readable and they are the same authority.</summary>
        Single = 1,

        /// <summary>Every sibling was readable and they are NOT the same authority.</summary>
        Divergent = 2,

        /// <summary>At least one sibling could not be read, so no authority can be established.</summary>
        UnreadableSibling = 3,
    }

    /// <summary>The authority of a rack, or the reason there is none.</summary>
    public sealed class AuthoredAuthorityResult
    {
        private AuthoredAuthorityResult(
            AuthoredAuthorityOutcome outcome,
            SelectivePalletDesignDocument authored,
            string error)
        {
            Outcome = outcome;
            Authored = authored;
            Error = error;
        }

        public AuthoredAuthorityOutcome Outcome { get; }

        /// <summary>The single logical authored state. Null unless the outcome is <see cref="AuthoredAuthorityOutcome.Single"/>.</summary>
        public SelectivePalletDesignDocument Authored { get; }

        public string Error { get; }

        public bool IsSingle => Outcome == AuthoredAuthorityOutcome.Single;

        public static AuthoredAuthorityResult Single(SelectivePalletDesignDocument authored)
            => new AuthoredAuthorityResult(AuthoredAuthorityOutcome.Single, authored, null);

        public static AuthoredAuthorityResult Divergent(string error)
            => new AuthoredAuthorityResult(AuthoredAuthorityOutcome.Divergent, null, error);

        public static AuthoredAuthorityResult Unreadable(string error)
            => new AuthoredAuthorityResult(AuthoredAuthorityOutcome.UnreadableSibling, null, error);
    }

    /// <summary>
    /// Decides whether the sibling views of one rack carry the SAME authored state.
    ///
    /// <para>
    /// A rack has a frontal, a plan and one lateral per cut, and each of them stores the whole design. They
    /// are supposed to agree, and they can fail to — historical partial commits reach that state — so the
    /// question has to be asked before any operation reads "the" design of a rack. There is no safe way to
    /// pick one: not the frontal, not the first, not the majority. Divergence aborts.
    /// </para>
    /// <para>
    /// <b>The comparison is structural over the persisted state, not over JSON bytes and not field by field.</b>
    /// That choice is the whole design: the contract requires INCLUDE-BY-DEFAULT, so any field a later gate
    /// adds must participate without anybody remembering to add it. A hand-written field comparison is
    /// exclude-by-default in practice — the day someone adds a property and forgets the comparator, two
    /// divergent siblings start reading as equal. Comparing the persisted tree is order-insensitive and
    /// formatting-insensitive, so a document that went through disk equals one built in memory, and yet
    /// nothing can silently drop out of the comparison.
    /// </para>
    /// <para>
    /// Consequences worth stating because they look severe and are deliberate. <c>SchemaVersion</c>
    /// participates: a view on the promoted line beside one on the legacy line is the trace of an interrupted
    /// link, and it IS divergence. <c>ExtensionData</c> participates too: if a later version wrote authority
    /// into one view and not the others, this build cannot know which one governs — and that is exactly the
    /// situation that must not be resolved blindly.
    /// </para>
    /// <para>
    /// What is excluded is only what legitimately belongs to the VIEW rather than to the rack — the
    /// envelope's view and section — and neither of them lives in this document, so nothing has to be
    /// filtered out here.
    /// </para>
    /// </summary>
    public static class SelectiveAuthoredAuthority
    {
        private static readonly JsonSerializerOptions ComparisonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>
        /// True when every document describes the same authored state. A single document is trivially its own
        /// authority; a null one never is.
        /// </summary>
        public static bool IsSameAuthority(IReadOnlyList<SelectivePalletDesignDocument> documents)
        {
            if (documents == null || documents.Count == 0)
            {
                return false;
            }

            using var first = Canonical(documents[0]);

            if (first == null)
            {
                return false;
            }

            for (var i = 1; i < documents.Count; i++)
            {
                using var other = Canonical(documents[i]);

                if (other == null || !AreEquivalent(first.RootElement, other.RootElement))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The single authored authority of a rack, or why it has none. An unreadable sibling is NEVER
        /// filtered out to carry on with the readable ones: that shortcut is what makes a partial answer look
        /// like a complete one.
        /// </summary>
        public static AuthoredAuthorityResult Resolve(
            string rackId,
            IReadOnlyList<ProjectVariableScanEntry> siblings)
        {
            if (siblings == null || siblings.Count == 0)
            {
                return AuthoredAuthorityResult.Unreadable(
                    "El rack " + rackId + " no tiene ninguna vista presente de la que leer su diseño.");
            }

            var documents = new List<SelectivePalletDesignDocument>();

            foreach (var sibling in siblings)
            {
                if (sibling == null || !sibling.AuthoredReadable || sibling.Authored == null)
                {
                    return AuthoredAuthorityResult.Unreadable(
                        "El rack " + rackId + " tiene una vista cuyo diseño no se puede interpretar (definición '" +
                        (sibling?.DefinitionId ?? "<desconocida>") + "'). No se elige otra hermana.");
                }

                documents.Add(sibling.Authored);
            }

            return IsSameAuthority(documents)
                ? AuthoredAuthorityResult.Single(documents[0])
                : AuthoredAuthorityResult.Divergent(
                    "Las vistas del rack " + rackId + " tienen diseños divergentes. " +
                    "No se cotiza ni se propaga desde una hermana elegida a dedo: reconcilia el rack primero.");
        }

        private static JsonDocument Canonical(SelectivePalletDesignDocument document)
            => document == null ? null : JsonDocument.Parse(JsonSerializer.Serialize(document, ComparisonOptions));

        /// <summary>
        /// Structural equivalence of two persisted trees. Objects compare by member regardless of order;
        /// arrays compare in order, because order is meaningful in every list this document persists (bays,
        /// levels, separations); numbers compare numerically, so a value that round-tripped through disk
        /// equals the same value built in memory.
        /// </summary>
        private static bool AreEquivalent(JsonElement left, JsonElement right)
        {
            if (left.ValueKind != right.ValueKind)
            {
                return false;
            }

            switch (left.ValueKind)
            {
                case JsonValueKind.Object:
                    var leftMembers = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

                    foreach (var member in left.EnumerateObject())
                    {
                        leftMembers[member.Name] = member.Value;
                    }

                    var seen = 0;

                    foreach (var member in right.EnumerateObject())
                    {
                        if (!leftMembers.TryGetValue(member.Name, out var counterpart) ||
                            !AreEquivalent(counterpart, member.Value))
                        {
                            return false;
                        }

                        seen++;
                    }

                    return seen == leftMembers.Count;

                case JsonValueKind.Array:
                    var leftItems = left.EnumerateArray();
                    var rightItems = right.EnumerateArray();

                    while (true)
                    {
                        var hasLeft = leftItems.MoveNext();
                        var hasRight = rightItems.MoveNext();

                        if (hasLeft != hasRight)
                        {
                            return false;
                        }

                        if (!hasLeft)
                        {
                            return true;
                        }

                        if (!AreEquivalent(leftItems.Current, rightItems.Current))
                        {
                            return false;
                        }
                    }

                case JsonValueKind.String:
                    return string.Equals(left.GetString(), right.GetString(), StringComparison.Ordinal);

                case JsonValueKind.Number:
                    return left.GetDouble().Equals(right.GetDouble());

                default:
                    // true, false, null and undefined carry no payload: the kind already decided it.
                    return true;
            }
        }
    }
}
