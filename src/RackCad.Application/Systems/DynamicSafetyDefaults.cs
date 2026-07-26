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

            // PB-009 (I-32): with no explicit side the rule is ADAPTIVE — it puts a guard on the far face of the last
            // post. A low-end-only system (Push Back) has no far face to guard, so that branch resolves to nothing
            // there; otherwise the historical behaviour is untouched.
            if (postCount <= 1)
            {
                return selection.LowEndOnly ? SafetySide.Left : SafetySide.Both;
            }

            if (postIndex == 0)
            {
                return SafetySide.Left;
            }

            if (postIndex == postCount - 1)
            {
                return selection.LowEndOnly ? SafetySide.None : SafetySide.Right;
            }

            return SafetySide.None;
        }

        /// <summary>
        /// Las copias físicas del protector en un poste. Owner-validation round 1 (I-32): este es el camino REAL del
        /// protector lateral, y también aquí hay que separar los tres ejes.
        ///
        /// <para>Una elección EXPLÍCITA del usuario la resuelve <see cref="SelectiveSafetyEnds"/>, la misma autoridad
        /// que el resto de la seguridad: conserva la pertenencia, conserva la orientación y, en un sistema de extremo
        /// bajo, lleva la pieza delante. Antes el lado se traducía directamente a un extremo, y un <c>Right</c> en
        /// Push Back acababa dibujado ATRÁS, donde no hay pasillo que proteger.</para>
        ///
        /// <para>La regla ADAPTATIVA (sin elección) queda intacta: ahí Left/Right nombran una posición y no una
        /// orientación, y <see cref="SideAt"/> ya contempla el extremo bajo.</para>
        /// </summary>
        public static IReadOnlyList<SafetyEndCopy> CopiesAt(
            SelectiveSafetySelection selection, int postIndex, int postCount)
        {
            if (selection == null || postIndex < 0 || postIndex >= Math.Max(1, postCount))
            {
                return new SafetyEndCopy[0];
            }

            if (selection.Side != SafetySide.None || selection.PostSides.Any(post => post != null))
            {
                return SelectiveSafetyEnds.CopiesForPost(selection, postIndex);
            }

            switch (SideAt(selection, postIndex, postCount))
            {
                case SafetySide.Left:
                    return new[] { new SafetyEndCopy(atHighEnd: false, mirrored: false) };
                case SafetySide.Right:
                    return new[] { new SafetyEndCopy(atHighEnd: true, mirrored: true) };
                case SafetySide.Both:
                    return new[]
                    {
                        new SafetyEndCopy(atHighEnd: false, mirrored: false),
                        new SafetyEndCopy(atHighEnd: true, mirrored: true),
                    };
                default:
                    return new SafetyEndCopy[0];
            }
        }

        /// <summary>La copia que le toca a un extremo, o null si ese extremo no lleva protector en ese poste.</summary>
        public static SafetyEndCopy? CopyAtEnd(
            SelectiveSafetySelection selection, int postIndex, int postCount, DynamicRackEnd end)
        {
            var highEnd = end == DynamicRackEnd.Entrance;
            foreach (var copy in CopiesAt(selection, postIndex, postCount))
            {
                if (copy.AtHighEnd == highEnd)
                {
                    return copy;
                }
            }

            return null;
        }

        public static bool DrawsAtEnd(
            SelectiveSafetySelection selection,
            int postIndex,
            int postCount,
            DynamicRackEnd end)
            => CopyAtEnd(selection, postIndex, postCount, end).HasValue;
    }
}
