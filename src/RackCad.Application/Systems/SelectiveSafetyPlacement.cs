using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Places "protector de bota" safety elements at post bases, identically across views (frontal/lateral/planta).
    /// A bota's origin coincides with the base plate's origin — post origin minus the plate's MONTAJE_POSTE mate for
    /// that view (the user's rule; the bota has no mate of its own). The side chooses the X mirror: Left = as-is,
    /// Right = mirrored (X scale −1), Both = one of each. The side is per-post: a post uses its
    /// <see cref="SelectiveSafetySelection.SideForPost"/> override, else the selection default. Shared so the rule
    /// stays identical in every view.
    /// </summary>
    internal static class SelectiveSafetyPlacement
    {
        /// <summary>The enabled botas for a view: catalog Type == BOTA, a drawn side (default OR any per-post override),
        /// and a block defined for the view (else it can't be drawn). Returns (pieceId, block, selection) so the caller
        /// resolves the per-post side at each post.</summary>
        public static List<(string PieceId, string Block, SelectiveSafetySelection Selection)> EnabledBotas(SelectiveRackSystem system, RackCatalog catalog, string view)
        {
            var result = new List<(string, string, SelectiveSafetySelection)>();
            if (system?.SafetySelections == null || catalog?.SafetyElements == null)
            {
                return result;
            }

            foreach (var selection in system.SafetySelections)
            {
                if (selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
                {
                    continue;
                }

                // Drawn if the default side draws OR some post overrides to a drawn side.
                var drawsSomewhere = selection.Side != SafetySide.None
                    || selection.PostSides.Any(p => p != null && p.Side != SafetySide.None);
                if (!drawsSomewhere)
                {
                    continue;
                }

                var element = catalog.SafetyElements.FirstOrDefault(s => string.Equals(s?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
                if (element == null || !string.Equals(element.Type, "BOTA", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var block = CatalogLookup.Block(catalog, selection.ElementId, view);
                if (!string.IsNullOrWhiteSpace(block))
                {
                    result.Add((selection.ElementId, block, selection));
                }
            }

            return result;
        }

        /// <summary>Append the botas for ONE post (index <paramref name="postIndex"/>) at <paramref name="postOrigin"/>:
        /// each sits at the base plate origin (postOrigin − the plate's <paramref name="view"/> mate), on the side this
        /// post resolves to. <paramref name="plateId"/> may be blank (no plate) → the bota sits on the post origin.</summary>
        public static void AppendAtPost(
            ICollection<HeaderBlockInstance> target, RackCatalog catalog, string view,
            IReadOnlyList<(string PieceId, string Block, SelectiveSafetySelection Selection)> botas,
            Point2D postOrigin, string plateId, int postIndex)
        {
            if (botas == null || botas.Count == 0)
            {
                return;
            }

            var plateMate = string.IsNullOrWhiteSpace(plateId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);
            var at = new Point2D(postOrigin.X - plateMate.X, postOrigin.Y - plateMate.Y);

            foreach (var (pieceId, block, selection) in botas)
            {
                var side = selection.SideForPost(postIndex);
                if (side == SafetySide.Left || side == SafetySide.Both)
                {
                    target.Add(Bota(pieceId, block, view, at, mirrored: false));
                }

                if (side == SafetySide.Right || side == SafetySide.Both)
                {
                    target.Add(Bota(pieceId, block, view, at, mirrored: true));
                }
            }
        }

        private static HeaderBlockInstance Bota(string pieceId, string block, string view, Point2D at, bool mirrored)
            => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = pieceId,
                BlockName = block,
                View = view,
                MirroredX = mirrored,
                Insertion = at,
                ConnectionAnchor = at
            };
    }
}
