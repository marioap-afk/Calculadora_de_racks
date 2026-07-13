using System;
using System.Collections.Generic;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure rules for the vertical layout of the separators that connect adjacent headers in a dynamic
    /// system. The count grows with the header height (one more separator per ~60", minimum 2). The
    /// positions distribute the header height as half-full-…-full-half (floor-to-first and last-to-top are
    /// half of the between-separator spacing), snapped to the post's separator troquel grid
    /// (<c>TROQUEL_SEPARADOR</c> + k·paso); the bottom space absorbs the remainder so the rest stay even.
    /// </summary>
    public static class SeparatorLevelCalculator
    {
        /// <summary>Maximum nominal spacing between separators; drives how many are needed.</summary>
        public const double MaxSpacing = 60.0;

        /// <summary>Minimum number of separators per header.</summary>
        public const int MinCount = 2;

        /// <summary>Separators needed for a header of the given height: max(2, floor(height/spacing) + 1). The nominal
        /// <paramref name="maxSpacing"/> defaults to the dynamic rack's 60"; the selectivo passes its own (100").</summary>
        public static int Count(double headerHeight, double maxSpacing = MaxSpacing)
        {
            if (headerHeight <= 0.0)
            {
                return MinCount;
            }

            var spacing = maxSpacing > 0.0 ? maxSpacing : MaxSpacing;
            return Math.Max(MinCount, (int)Math.Floor(headerHeight / spacing) + 1);
        }

        /// <summary>
        /// Separator heights (Y, ascending) for a header of <paramref name="headerHeight"/>. They land on the
        /// troquel grid <paramref name="troquelSeparadorY"/> + k·<paramref name="paso"/>, are spaced an even
        /// "full" apart, with the top space ≈ half; the bottom space absorbs the remainder.
        /// </summary>
        public static IReadOnlyList<double> Levels(
            double headerHeight, double troquelSeparadorY, double paso,
            int? countOverride = null, double? spacingOverride = null, double maxSpacing = MaxSpacing)
        {
            var levels = new List<double>();

            if (headerHeight <= 0.0)
            {
                return levels;
            }

            if (paso <= 0.0)
            {
                paso = 2.0;
            }

            // Defaults follow the standard rule (one per maxSpacing); an explicit count and/or spacing override them.
            var count = countOverride.HasValue && countOverride.Value >= 1 ? countOverride.Value : Count(headerHeight, maxSpacing);
            var full = spacingOverride.HasValue && spacingOverride.Value > 0.0 ? spacingOverride.Value : headerHeight / count;
            var half = full / 2.0;

            // Spacing between separators, rounded to a whole number of troqueles (so it is even on a 2" pitch).
            var fullGrid = Math.Max(paso, Math.Round(full / paso, MidpointRounding.AwayFromZero) * paso);

            // Anchor the top separator about "half" below the header top, snapped to the troquel grid; step
            // down by the even full spacing for the rest. The bottom space (to TROQUEL_SEPARADOR) takes the slack.
            var topLevel = Snap(headerHeight - half, troquelSeparadorY, paso);

            for (var index = 0; index < count; index++)
            {
                var y = topLevel - (count - 1 - index) * fullGrid;

                // User overrides (a large count and/or spacing) can push lower levels below the first physical
                // troquel — even below the floor. There is no post to mount them on: drop them instead of
                // emitting impossible (negative) heights.
                if (y >= troquelSeparadorY - 1e-9)
                {
                    levels.Add(y);
                }
            }

            return levels;
        }

        private static double Snap(double value, double baseY, double paso)
        {
            return baseY + Math.Round((value - baseY) / paso, MidpointRounding.AwayFromZero) * paso;
        }
    }
}
