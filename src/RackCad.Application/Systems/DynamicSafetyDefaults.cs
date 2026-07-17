using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>Catalog-driven safety selections applied to a new dynamic rack.</summary>
    public static class DynamicSafetyDefaults
    {
        private static readonly string[] Families =
        {
            SelectiveSafetyDefaults.BotaType,
            SelectiveSafetyDefaults.LateralType,
            SelectiveSafetyDefaults.DesviadorType,
            SelectiveSafetyDefaults.DefensaType,
            SelectiveSafetyDefaults.GuiaType
        };

        public static IReadOnlyList<SelectiveSafetySelection> Build(RackCatalog catalog)
        {
            var result = new List<SelectiveSafetySelection>();
            foreach (var family in Families)
            {
                var element = catalog?.SafetyElements?.FirstOrDefault(entry => entry != null
                    && !string.IsNullOrWhiteSpace(entry.Id)
                    && SelectiveSafetyDefaults.IsType(entry.Type, family));
                if (element == null)
                {
                    continue;
                }

                result.Add(new SelectiveSafetySelection
                {
                    ElementId = element.Id,
                    Quantity = 1,
                    Side = SelectiveSafetyDefaults.IsType(family, SelectiveSafetyDefaults.BotaType)
                           || SelectiveSafetyDefaults.IsType(family, SelectiveSafetyDefaults.DesviadorType)
                        ? SafetySide.Both
                        : SafetySide.None
                });
            }

            return result;
        }
    }

    /// <summary>
    /// Dynamic LATERAL default: the first post faces the exit and the last faces the entrance. An empty override list
    /// remains adaptive when fronts are added or removed; any explicit per-post entry switches to the authored grid.
    /// </summary>
    public static class DynamicLateralGuardPlan
    {
        public static SafetySide SideAt(SelectiveSafetySelection selection, int postIndex, int postCount)
        {
            if (selection == null || postIndex < 0 || postIndex >= Math.Max(1, postCount))
            {
                return SafetySide.None;
            }

            if (selection.Side != SafetySide.None || selection.PostSides.Any(post => post != null))
            {
                return selection.SideForPost(postIndex);
            }

            if (postCount <= 1)
            {
                return SafetySide.Both;
            }

            if (postIndex == 0)
            {
                return SafetySide.Left;
            }

            return postIndex == postCount - 1 ? SafetySide.Right : SafetySide.None;
        }

        public static bool DrawsAtEnd(
            SelectiveSafetySelection selection,
            int postIndex,
            int postCount,
            DynamicRackEnd end)
        {
            var side = SideAt(selection, postIndex, postCount);
            return end == DynamicRackEnd.Exit
                ? side == SafetySide.Left || side == SafetySide.Both
                : side == SafetySide.Right || side == SafetySide.Both;
        }
    }
}
