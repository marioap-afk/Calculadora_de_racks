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
        {
            postCount = Math.Max(1, postCount);
            if (postIndex < 0 || postIndex >= postCount)
            {
                return new DynamicForkliftDefenseSetting(0.0, 0.0);
            }

            foreach (var over in overrides ?? Array.Empty<SafetyPostDefense>())
            {
                if (over != null && over.PostIndex == postIndex)
                {
                    return new DynamicForkliftDefenseSetting(
                        Math.Max(0.0, over.ExitLength),
                        Math.Max(0.0, over.EntranceLength));
                }
            }

            var length = postIndex == 0 || postIndex == postCount - 1
                ? EdgeLength
                : IntermediateLength;
            return new DynamicForkliftDefenseSetting(length, length);
        }
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
