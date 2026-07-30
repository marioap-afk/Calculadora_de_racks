using System;
using System.Collections.Generic;

namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// STRUCTURAL equality for an arm template: same values, whatever the objects.
    ///
    /// It exists because the contract promises the station persists "only overrides that DIFFER from the
    /// default", and reference equality cannot tell whether they differ. Applying the default to a cell used to
    /// store a deep copy of it, which looks harmless and is not: from then on that cell no longer FOLLOWS the
    /// default, so changing the default leaves it behind — silently, and with no way for the user to see which
    /// cells were pinned and which were merely touched.
    ///
    /// Every editable field is compared, and the list is exhaustive on purpose: a field added to the template
    /// and forgotten here would make two different arms compare equal, and the override would be dropped.
    /// <see cref="Signature"/> exists so a test can see WHAT is compared rather than only whether two things
    /// matched.
    /// </summary>
    public static class CantileverArmTemplateComparer
    {
        /// <summary>
        /// How close two lengths must be to count as the same value.
        ///
        /// Exact equality would make a template that survived a round-trip through text differ from the one it
        /// came from. It is the same tolerance the geometry uses for a fit.
        /// </summary>
        public const double Tolerance = 1e-9;

        /// <summary>Whether two templates describe the same physical arm. Nulls compare equal to nulls.</summary>
        public static bool AreEqual(CantileverArmTemplateDesign left, CantileverArmTemplateDesign right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return string.Equals(Signature(left), Signature(right), StringComparison.Ordinal);
        }

        /// <summary>
        /// The template's whole editable content as one deterministic string.
        ///
        /// Comparing signatures rather than field-by-field keeps ONE list of what matters, so the comparison and
        /// anything that wants to explain it cannot drift apart. Lengths are rounded to the tolerance so the
        /// text carries the same equality the numbers do.
        /// </summary>
        public static string Signature(CantileverArmTemplateDesign template)
        {
            if (template == null)
            {
                return "<null>";
            }

            var body = template.Body ?? new CantileverArmBodyDesign();
            var mount = template.MountingPlate ?? new CantileverArmMountingPlateTemplateDesign();
            var end = template.EndPlate ?? new CantileverArmEndPlateDesign();

            return string.Join(";", new List<string>
            {
                "arr=" + body.Arrangement,
                "sec=" + (body.SectionId ?? string.Empty),
                "cut=" + Round(body.CutLength),
                "slope=" + Round(body.SlopeRisePer12),
                "mt=" + Round(mount.Thickness),
                "rows=" + mount.VerticalPunchCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                // The margin is NULLABLE and its absence is meaningful: a template that declares no margin is
                // not the same as one that declares zero, because the first is rejected and the second is not.
                "margin=" + (mount.VerticalEndOffset == null ? "-" : Round(mount.VerticalEndOffset.Value)),
                "end=" + end.Mode,
                "et=" + Round(end.Thickness),
                "stop=" + Round(end.ExtraStopHeight)
            });
        }

        private static string Round(double value)
        {
            if (double.IsNaN(value))
            {
                return "NaN";
            }

            if (double.IsPositiveInfinity(value))
            {
                return "+inf";
            }

            if (double.IsNegativeInfinity(value))
            {
                return "-inf";
            }

            return Math.Round(value, 9, MidpointRounding.AwayFromZero)
                .ToString("0.#########", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
