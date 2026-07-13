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
    /// Places post-base safety elements (BOTA = "protector de bota"; LATERAL = "protector lateral", an end-of-row guard)
    /// identically across views (frontal/lateral/planta). An element's origin coincides with the base plate's origin —
    /// post origin minus the plate's MONTAJE_POSTE mate for that view (the user's rule; the element has no mate of its
    /// own). The side chooses the mirror: Left = as-is, Right = mirrored, Both = one of each. The side is per-post: a
    /// post uses its <see cref="SelectiveSafetySelection.SideForPost"/> override, else the selection default.
    ///
    /// A LATERAL is placed like a BOTA but carries a LONGITUD = the frame fondo (depth) and, where present, REPLACES the
    /// botas at that frente (an end-guard covers the uprights, so no botas there).
    ///
    /// The mirror reference differs by view. In the FRONTAL a post is its own symmetric unit, so the mirrored copy flips
    /// about the block's own origin (X scale −1), in place. In the depth views (PLANTA/LATERAL) the whole system is
    /// symmetric about the CENTER of its total fondo (depth) span — a rack can have several fondos — so the mirrored copy
    /// is a true reflection about that vertical line: it flips AND moves to the reflected X. Callers pass that center as
    /// <c>mirrorAxisX</c> (null = flip about the origin). Shared so the rule stays identical per view.
    /// </summary>
    internal static class SelectiveSafetyPlacement
    {
        public const string BotaType = "BOTA";
        public const string LateralType = "LATERAL";

        /// <summary>A protector lateral's manufactured length exceeds its drawn LONGITUD (= the fondo) by this much (the
        /// guide/flanges overhang the posts). The BOM reports drawnLongitud + this.</summary>
        public const double LateralLengthAllowance = 4.0;

        public sealed class SafetyElement
        {
            public string PieceId;
            public string Block;
            public SelectiveSafetySelection Selection;
        }

        /// <summary>The enabled safety elements of a catalog <paramref name="type"/> for a view: a drawn side (default OR
        /// any per-post override) and a block defined for the view (else it can't be drawn). The caller resolves the
        /// per-post side at each post.</summary>
        public static List<SafetyElement> EnabledOfType(SelectiveRackSystem system, RackCatalog catalog, string view, string type)
        {
            var result = new List<SafetyElement>();
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
                if (element == null || !string.Equals(element.Type, type, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var block = CatalogLookup.Block(catalog, selection.ElementId, view);
                if (!string.IsNullOrWhiteSpace(block))
                {
                    result.Add(new SafetyElement { PieceId = selection.ElementId, Block = block, Selection = selection });
                }
            }

            return result;
        }

        /// <summary>True if any of these elements draws at post <paramref name="postIndex"/> — used so a LATERAL frente
        /// suppresses its botas.</summary>
        public static bool DrawsAt(IReadOnlyList<SafetyElement> elements, int postIndex)
            => elements != null && elements.Any(e => e.Selection.SideForPost(postIndex) != SafetySide.None);

        /// <summary>Append the elements for ONE post (index <paramref name="postIndex"/>) at <paramref name="postOrigin"/>:
        /// each sits at the base plate origin (postOrigin − the plate's <paramref name="view"/> mate), on the side this
        /// post resolves to. <paramref name="plateId"/> may be blank (no plate) → it sits on the post origin.
        /// <paramref name="mirrorAxisX"/> is the reflection line for the mirrored (Right) copy: null flips about the
        /// block origin in place (frontal); a value reflects position + orientation about that X (planta/lateral).
        /// <paramref name="longitud"/>, when set, becomes the piece's LONGITUD dynamic param (the LATERAL spans the fondo).</summary>
        public static void AppendAtPost(
            ICollection<HeaderBlockInstance> target, RackCatalog catalog, string view,
            IReadOnlyList<SafetyElement> elements,
            Point2D postOrigin, string plateId, int postIndex, double? mirrorAxisX = null, double? longitud = null, bool mirrorYInPlace = false)
        {
            if (elements == null || elements.Count == 0)
            {
                return;
            }

            var plateMate = string.IsNullOrWhiteSpace(plateId)
                ? new Point2D(0.0, 0.0)
                : CatalogLookup.Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);
            var at = new Point2D(postOrigin.X - plateMate.X, postOrigin.Y - plateMate.Y);

            // The mirrored (Right) copy. A LATERAL block already spans the fondo, so it stays IN PLACE and only its guide
            // flips — a Y-flip in the depth views (mirrorYInPlace). A point element (bota) instead reflects about the
            // mirror axis (moves across it), or flips about the block origin in the frontal (mirrorAxisX null).
            var reflectedAt = mirrorAxisX.HasValue ? new Point2D(2.0 * mirrorAxisX.Value - at.X, at.Y) : at;
            var mirroredAt = mirrorYInPlace ? at : reflectedAt;

            foreach (var element in elements)
            {
                var side = element.Selection.SideForPost(postIndex);
                if (side == SafetySide.Left || side == SafetySide.Both)
                {
                    target.Add(Piece(element.PieceId, element.Block, view, at, mirroredX: false, mirroredY: false, longitud));
                }

                if (side == SafetySide.Right || side == SafetySide.Both)
                {
                    target.Add(Piece(element.PieceId, element.Block, view, mirroredAt, mirroredX: !mirrorYInPlace, mirroredY: mirrorYInPlace, longitud));
                }
            }
        }

        private static HeaderBlockInstance Piece(string pieceId, string block, string view, Point2D at, bool mirroredX, bool mirroredY, double? longitud)
        {
            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = pieceId,
                BlockName = block,
                View = view,
                MirroredX = mirroredX,
                MirroredY = mirroredY,
                Insertion = at,
                ConnectionAnchor = at
            };

            if (longitud.HasValue && longitud.Value > 0.0)
            {
                instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = longitud.Value;
            }

            return instance;
        }
    }
}
