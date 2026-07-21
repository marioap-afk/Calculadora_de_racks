using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// Selection helpers shared by <see cref="CatalogCombo"/>: resolving the option that matches a stored id and
    /// prepending an "(auto)"/"(none)" sentinel. Windows repeat the <c>combo.SelectedValue = id</c> idiom (with a
    /// hand-injected "(auto)" row in the larguero window); keeping the resolution here makes the fallback behavior
    /// one testable rule. Pure: <see cref="CatalogOption"/> is a plain pair, so no dispatcher is involved.
    /// </summary>
    public static class CatalogComboSelection
    {
        /// <summary>The option whose <see cref="CatalogOption.Id"/> equals <paramref name="id"/> (ordinal,
        /// case-insensitive to match the catalog id comparison), or null when none matches or the id is blank.</summary>
        public static CatalogOption Resolve(IEnumerable<CatalogOption> options, string id)
        {
            if (options == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var trimmed = id.Trim();
            return options.FirstOrDefault(option =>
                option != null && string.Equals(option.Id, trimmed, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Returns <paramref name="options"/> with a leading sentinel (e.g. <c>new CatalogOption(null,
        /// "(auto)")</c>) so a blank/auto choice is selectable. The sentinel is only added when supplied.</summary>
        public static IReadOnlyList<CatalogOption> WithPlaceholder(CatalogOption placeholder, IEnumerable<CatalogOption> options)
        {
            var list = new List<CatalogOption>();
            if (placeholder != null)
            {
                list.Add(placeholder);
            }

            if (options != null)
            {
                list.AddRange(options.Where(option => option != null));
            }

            return list;
        }
    }
}
