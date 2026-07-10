using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;

namespace RackCad.UI
{
    /// <summary>
    /// Small helpers shared by every window/view-model so they are not re-implemented per file:
    /// resilient catalog loading, invariant numeric parsing of user input, and building the
    /// DisplayName/Id options the combos bind to.
    /// </summary>
    internal static class UiSupport
    {
        /// <summary>Loads the catalog, or an empty one if the files are missing/corrupt (UI keeps working).</summary>
        public static RackCatalog LoadCatalogSafe()
        {
            try
            {
                return JsonRackCatalogProvider.FromBaseDirectory().Load();
            }
            catch
            {
                return new RackCatalog();
            }
        }

        /// <summary>Parses a user-typed number: invariant first, then the user's culture (so "96.5" and "96,5" both work).</summary>
        public static bool TryNum(string text, out double value)
        {
            text = (text ?? string.Empty).Trim();
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value)
                || double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
        }

        /// <summary>Parses an OPTIONAL positive number: empty/whitespace → null (auto); a valid &gt; 0 value → that value;
        /// anything else (non-numeric or &lt;= 0) → false, so the caller can report an error instead of silently defaulting.</summary>
        public static bool TryOptionalNum(string text, out double? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            if (TryNum(text, out var v) && v > 0.0)
            {
                value = v;
                return true;
            }

            return false;
        }

        /// <summary>Distinct, ordered DisplayName/Id options for a combo (skips blank ids).</summary>
        public static List<CatalogOption> ToOptions<T>(IEnumerable<T> entries) where T : CatalogEntryBase
        {
            return (entries ?? Enumerable.Empty<T>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Id))
                .GroupBy(entry => entry.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(entry => entry.Label, StringComparer.CurrentCultureIgnoreCase)
                .Select(entry => new CatalogOption(entry.Id.Trim(), entry.Label))
                .ToList();
        }
    }
}
