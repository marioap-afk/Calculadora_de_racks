using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// One row of a section picker: the exact id, the designation a human recognises, and the family.
    ///
    /// It is a VIEW of a catalogue row and not a second catalogue: it carries no dimension, no property and no
    /// weight, so nothing downstream can start resolving geometry from a picker row.
    /// </summary>
    public sealed class StructuralSectionChoice
    {
        internal StructuralSectionChoice(StructuralSectionDefinition section)
        {
            Id = section.SectionId;
            Designation = section.DisplayName ?? string.Empty;
            Family = section.Family;
            IsEnabled = section.IsEnabled;
        }

        /// <summary>The EXACT id. This is what a design stores and what a resolver looks up (ADR-0024, D1).</summary>
        public StructuralSectionId Id { get; }

        /// <summary>The label printed in the AISC manual: what the user reads and types.</summary>
        public string Designation { get; }

        public StructuralSectionFamily Family { get; }

        public bool IsEnabled { get; }

        public override string ToString() => Designation + " (" + Id.Value + ")";
    }

    /// <summary>
    /// The search a section picker runs. PURE: no WPF, no catalogue loading, no policy, no Cantilever.
    ///
    /// <para><b>Searching is not resolving.</b> This matches text so a human can FIND a row; the row then hands
    /// back an EXACT <see cref="StructuralSectionId"/>. Nothing here ever turns a typed fragment into a section
    /// on its own — that is the substring resolution ADR-0024 D6 forbids, and the reason the two operations live
    /// in different places with different names.</para>
    ///
    /// <para>It matches the designation and the id, case-insensitively, ignoring the separators a user does not
    /// type: <c>W10X33</c>, <c>w10x33</c>, <c>AISC-W-W10X33</c>, <c>10X33</c> and <c>10x33</c> all find the same
    /// row, and <c>C4</c> finds every four-inch channel.</para>
    /// </summary>
    public static class StructuralSectionSearch
    {
        /// <summary>The families a picker offers as filters, in the order the AISC manual lists them.</summary>
        public static IReadOnlyList<StructuralSectionFamily> Families { get; } = new[]
        {
            StructuralSectionFamily.W,
            StructuralSectionFamily.S,
            StructuralSectionFamily.Channel,
            StructuralSectionFamily.Angle,
            StructuralSectionFamily.HssRectangular
        };

        /// <summary>The Spanish label of a family filter.</summary>
        public static string Label(StructuralSectionFamily family)
        {
            switch (family)
            {
                case StructuralSectionFamily.W: return "W";
                case StructuralSectionFamily.S: return "S";
                case StructuralSectionFamily.Channel: return "C";
                case StructuralSectionFamily.Angle: return "L";
                case StructuralSectionFamily.HssRectangular: return "HSS rectangular";
                default: return family.ToString();
            }
        }

        /// <summary>
        /// The rows of <paramref name="catalogue"/> a consumer may offer, restricted to
        /// <paramref name="eligibleFamilies"/> when it declares any.
        ///
        /// The eligible families come from the CONSUMER — a column picker offers W, a separator picker offers
        /// channels — because which families a system admits is a decision of that system and never of a
        /// control.
        /// </summary>
        public static IReadOnlyList<StructuralSectionChoice> Choices(
            StructuralSectionCatalog catalogue,
            IEnumerable<StructuralSectionFamily> eligibleFamilies = null)
        {
            if (catalogue == null)
            {
                throw new ArgumentNullException(nameof(catalogue));
            }

            var eligible = eligibleFamilies?.Distinct().ToArray();

            return catalogue.All
                .Where(s => s.IsEnabled)
                .Where(s => eligible == null || eligible.Length == 0 || eligible.Contains(s.Family))
                .OrderBy(s => s.Family)
                .ThenBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(s => new StructuralSectionChoice(s))
                .ToList();
        }

        /// <summary>
        /// The rows that match <paramref name="text"/>, optionally narrowed to one <paramref name="family"/>.
        ///
        /// Blank text matches everything: an empty box means «no filter», not «no results».
        /// </summary>
        public static IReadOnlyList<StructuralSectionChoice> Filter(
            IReadOnlyList<StructuralSectionChoice> choices,
            string text,
            StructuralSectionFamily? family = null)
        {
            if (choices == null)
            {
                throw new ArgumentNullException(nameof(choices));
            }

            var needle = Normalize(text);

            return choices
                .Where(c => family == null || c.Family == family.Value)
                .Where(c => needle.Length == 0 || Matches(c, needle))
                .ToList();
        }

        private static bool Matches(StructuralSectionChoice choice, string needle) =>
            Normalize(choice.Designation).Contains(needle, StringComparison.Ordinal) ||
            Normalize(choice.Id.Value).Contains(needle, StringComparison.Ordinal);

        /// <summary>
        /// Upper case without the separators a user does not type.
        ///
        /// A designation is written <c>W10X33</c> and an id <c>AISC-W-W10X33</c>; somebody typing <c>w 10x33</c>
        /// means the same thing. Dropping spaces, hyphens and underscores is what makes the three forms one
        /// search, and it happens ONLY here — the id that comes out the other side is untouched.
        /// </summary>
        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new System.Text.StringBuilder(value.Length);

            foreach (var ch in value)
            {
                if (ch == ' ' || ch == '-' || ch == '_' || ch == '.')
                {
                    continue;
                }

                builder.Append(char.ToUpperInvariant(ch));
            }

            return builder.ToString();
        }
    }
}
