using System;
using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Single source of truth for the dynamic rack's forklift-defense grid. A selected transverse post represents the
    /// physical pair at the exit and entrance ends; drawings and BOM consume that same resolved length.
    /// </summary>
    public static class DynamicForkliftDefensePlan
    {
        public const string PostOriginPoint = "ORIGEN_POSTE";
        public const double EdgeLength = 12.0;
        public const double IntermediateLength = 36.0;

        public static DynamicForkliftDefenseSetting At(
            IEnumerable<SafetyPostDefense> overrides,
            int postIndex,
            int postCount)
            => At(overrides, postIndex, postCount, lowEndOnly: false);

        /// <summary>
        /// The resolved lengths of one post's pair, honouring PB-009 and PB-010 (I-32).
        ///
        /// <para><b>PB-010 — Auto per end.</b> The 12"/36" rule depends on whether the post is an EDGE or an
        /// INTERMEDIATE one, and that changes as the rack gains or loses fronts. A stored record used to freeze both
        /// lengths forever, so a post that had been an edge kept its 12" after becoming an intermediate. An end marked
        /// <see cref="SafetyPostDefense.ExitAuto"/>/<see cref="SafetyPostDefense.EntranceAuto"/> is recomputed from the
        /// CURRENT post count instead; an end the user typed keeps its number. Records with neither flag — every
        /// document written before this existed — behave exactly as they always did.</para>
        ///
        /// <para><b>PB-009 — low end only.</b> When <paramref name="lowEndOnly"/> is set (Push Back), the far end has
        /// no automatic length at all: with no explicit override it resolves to 0 and nothing is drawn there. An
        /// explicit, non-auto override is still honoured, so "off by default" stays a DEFAULT, not a prohibition.</para>
        /// </summary>
        public static DynamicForkliftDefenseSetting At(
            IEnumerable<SafetyPostDefense> overrides,
            int postIndex,
            int postCount,
            bool lowEndOnly)
        {
            postCount = Math.Max(1, postCount);
            if (postIndex < 0 || postIndex >= postCount)
            {
                return new DynamicForkliftDefenseSetting(0.0, 0.0);
            }

            var automatic = postIndex == 0 || postIndex == postCount - 1
                ? EdgeLength
                : IntermediateLength;
            var automaticEntrance = lowEndOnly ? 0.0 : automatic;

            foreach (var over in overrides ?? Array.Empty<SafetyPostDefense>())
            {
                if (over != null && over.PostIndex == postIndex)
                {
                    return new DynamicForkliftDefenseSetting(
                        over.ExitAuto ? automatic : Math.Max(0.0, over.ExitLength),
                        over.EntranceAuto ? automaticEntrance : Math.Max(0.0, over.EntranceLength));
                }
            }

            return new DynamicForkliftDefenseSetting(automatic, automaticEntrance);
        }

        /// <summary>
        /// The resolved pair for a SELECTION — the entry point every builder uses, because the selection is what
        /// carries the low-end-only rule (PB-009). A distinct name, not an overload: <c>At(null, …)</c> would be
        /// ambiguous and the compiler would pick one silently.
        /// </summary>
        public static DynamicForkliftDefenseSetting ForSelection(
            SelectiveSafetySelection selection,
            int postIndex,
            int postCount)
            => At(selection?.DefensaPosts, postIndex, postCount, selection?.LowEndOnly ?? false);
    }

    public readonly struct DynamicForkliftDefenseSetting
    {
        public DynamicForkliftDefenseSetting(double exitLength, double entranceLength)
        {
            ExitLength = exitLength;
            EntranceLength = entranceLength;
        }

        public double ExitLength { get; }
        public double EntranceLength { get; }
        public bool DrawsExit => ExitLength > 0.0;
        public bool DrawsEntrance => EntranceLength > 0.0;
    }
}
