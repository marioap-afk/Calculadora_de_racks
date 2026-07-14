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
        public const string TopeType = "TOPE";

        /// <summary>Deck / grating safety family (the catalog types it as this).</summary>
        public const string ParrillaType = "PARRILLA";

        /// <summary>PARRILLA block param that stretches its width (the frente span, used in FRONTAL).</summary>
        public const string ParrillaFrenteParam = "FRENTE";

        /// <summary>PARRILLA block param that stretches its depth (the fondo span, used in LATERAL).</summary>
        public const string ParrillaFondoParam = "FONDO";

        /// <summary>The post connection point the larguero tope mates on (its own troquel, distinct from the separador's).</summary>
        public const string TopePostPoint = "TROQUEL_TOPE";

        /// <summary>The "larguero tope" (rear pallet stop) block parameter for its stick-out ("saque").</summary>
        public const string SaqueParam = "SAQUE";

        /// <summary>Default SAQUE (stick-out) of a larguero tope, inches.</summary>
        public const double DefaultSaque = 3.0;

        /// <summary>A larguero tope's nominal rise ABOVE its larguero level (then snapped to the TROQUEL_SEPARADOR grid).</summary>
        public const double TopeYOffset = 8.0;

        /// <summary>A larguero tope's LONGITUD = its larguero's length + this (inches).</summary>
        public const double TopeLengthAllowance = 0.25;

        /// <summary>The fondo whose back carries the tope: the central one. 1 fondo → 0 (back); 2 → 0 (center); 4 → 1 (central pair).</summary>
        public static int CentralFondo(int fondoCount) => fondoCount > 0 ? (fondoCount - 1) / 2 : 0;

        /// <summary>One tope position: the fondo whose largueros it follows, whether it sits at that fondo's FRONT post
        /// (else its back), and whether the block is mirrored. Both spots of a per-fondo pair sit in the SAME central gap.</summary>
        public struct TopeSpot
        {
            public int Fondo;
            public bool AtFront;
            public bool Mirror;
        }

        /// <summary>The tope position(s): shared → one at the central fondo's back (facing the gap). Per-fondo → the two
        /// posts flanking the CENTRAL GAP — fondo c's back and fondo c+1's FRONT (back-to-back) — filtered by side
        /// (Left = c's back, Right = c+1's front, Both = both). So a per-fondo pair lands in the same gap, not two depths.</summary>
        public static IEnumerable<TopeSpot> TopeSpots(SelectiveSafetySelection selection, int fondoCount)
        {
            // The user's chosen fondo (0-based) if valid, else the automatic central one.
            var c = selection != null && selection.TopeFondo >= 0 && selection.TopeFondo < fondoCount
                ? selection.TopeFondo
                : CentralFondo(fondoCount);
            if (selection == null || selection.TopeShared)
            {
                yield return new TopeSpot { Fondo = c, AtFront = false, Mirror = false };
                yield break;
            }

            if (selection.Side == SafetySide.Left || selection.Side == SafetySide.Both)
            {
                yield return new TopeSpot { Fondo = c, AtFront = false, Mirror = false };
            }

            if ((selection.Side == SafetySide.Right || selection.Side == SafetySide.Both) && c + 1 < fondoCount)
            {
                yield return new TopeSpot { Fondo = c + 1, AtFront = true, Mirror = true }; // the other cabecera facing the same gap
            }
        }

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
