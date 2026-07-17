using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Resolves catalog-driven safety variants by family. A family is mutually exclusive: the persisted
    /// <see cref="SelectiveSafetySelection.ElementId"/> identifies the chosen variant while the catalog type owns the
    /// placement rule. Malformed legacy documents with several variants of one family consistently keep the first
    /// selection in document order, so drawing, BOM and the editor cannot disagree about which piece is active.
    /// </summary>
    public static class SelectiveSafetyFamilies
    {
        /// <summary>Families whose catalog rows are variants of one mutually-exclusive selection.</summary>
        public static bool IsExclusive(string type)
            => SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.BotaType)
               || SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.LateralType)
               || SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.TopeType)
               || SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.DesviadorType)
               || SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.DefensaType)
               || SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.GuiaType);

        /// <summary>Catalog variants of <paramref name="type"/>, preserving their catalog order.</summary>
        public static IReadOnlyList<SafetyElementCatalogEntry> VariantsOfType(
            IEnumerable<SafetyElementCatalogEntry> elements,
            string type)
            => (elements ?? Enumerable.Empty<SafetyElementCatalogEntry>())
                .Where(e => e != null
                            && !string.IsNullOrWhiteSpace(e.Id)
                            && SelectiveSafetyDefaults.IsType(e.Type, type))
                .ToList();

        /// <summary>The active variant of <paramref name="type"/>; first persisted selection wins.</summary>
        public static SelectiveSafetySelection SelectedOfType(
            IEnumerable<SelectiveSafetySelection> selections,
            IEnumerable<SafetyElementCatalogEntry> elements,
            string type)
        {
            var variants = new HashSet<string>(
                VariantsOfType(elements, type).Select(e => e.Id),
                StringComparer.OrdinalIgnoreCase);
            if (variants.Count == 0)
            {
                return null;
            }

            foreach (var selection in selections ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (selection != null
                    && !string.IsNullOrWhiteSpace(selection.ElementId)
                    && variants.Contains(selection.ElementId))
                {
                    return selection;
                }
            }

            return null;
        }
    }
}
