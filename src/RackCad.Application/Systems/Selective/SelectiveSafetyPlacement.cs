using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
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
        public const string BotaType = SelectiveSafetyDefaults.BotaType;
        public const string LateralType = SelectiveSafetyDefaults.LateralType;
        public const string TopeType = SelectiveSafetyDefaults.TopeType;

        /// <summary>Deck / grating safety family (the catalog types it as this).</summary>
        public const string ParrillaType = SelectiveSafetyDefaults.ParrillaType;

        /// <summary>PARRILLA block param that stretches its width (the frente span, used in FRONTAL).</summary>
        public const string ParrillaFrenteParam = SelectiveSafetyDefaults.ParrillaFrenteParam;

        /// <summary>PARRILLA block param that stretches its depth (the fondo span, used in LATERAL).</summary>
        public const string ParrillaFondoParam = SelectiveSafetyDefaults.ParrillaFondoParam;

        /// <summary>The post connection point the larguero tope mates on (its own troquel, distinct from the separador's).</summary>
        public const string TopePostPoint = "TROQUEL_TOPE";

        /// <summary>The "larguero tope" (rear pallet stop) block parameter for its stick-out ("saque").</summary>
        public const string SaqueParam = SelectiveSafetyDefaults.SaqueParam;

        /// <summary>Default SAQUE (stick-out) of a larguero tope, inches.</summary>
        public const double DefaultSaque = SelectiveSafetyDefaults.TopeSaque;

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

        /// <summary>
        /// The tope position(s). The side is read on the rack's own DEPTH axis — never a world coordinate — where
        /// <c>SelectiveDepthLayout.Offsets</c> puts the frontmost post at 0, so LOW/HIGH are the two ends of the
        /// tope's reference span ordered along it (I-46, ID12):
        /// <list type="number">
        /// <item><c>Side = None</c> → NO spot, decided BEFORE the shared/per-fondo split, so the plan agrees with the
        /// drawable gate its consumers already applied;</item>
        /// <item>with a fondo AFTER c the span is the CENTRAL GAP — <c>LOW = c's back</c>, <c>HIGH = c+1's front</c>
        /// (back-to-back, same gap, not two depths). Shared keeps ONE bar at LOW and the side is DORMANT there:
        /// that is the historic shared tope and it does not change;</item>
        /// <item>with no fondo after c — the last fondo, and the only case a SINGLE-fondo rack ever has — the span
        /// degenerates to c's own frame (<c>LOW = c's front</c>, <c>HIGH = c's back</c>) and there is nothing to
        /// share, so <c>TopeShared</c> is inert.</item>
        /// </list>
        /// </summary>
        public static IEnumerable<TopeSpot> TopeSpots(SelectiveSafetySelection selection, int fondoCount)
        {
            if (selection == null)
            {
                // No selection to read a side from: the historic single central spot, unchanged.
                yield return new TopeSpot { Fondo = CentralFondo(fondoCount), AtFront = false, Mirror = false };
                yield break;
            }

            if (selection.Side == SafetySide.None)
            {
                yield break; // "Ninguno" is zero spots, in both modes and at any fondo
            }

            // The user's chosen fondo (0-based) if valid, else the automatic central one.
            var c = selection.TopeFondo >= 0 && selection.TopeFondo < fondoCount
                ? selection.TopeFondo
                : CentralFondo(fondoCount);

            // The two ends of the reference span, ordered LOW → HIGH on the local depth axis. A front post's tope
            // faces back into the span, which is the mirrored orientation the gap's far cabecera already used.
            var gap = c + 1 < fondoCount;
            var low = gap
                ? new TopeSpot { Fondo = c, AtFront = false, Mirror = false }
                : new TopeSpot { Fondo = c, AtFront = true, Mirror = true };
            var high = gap
                ? new TopeSpot { Fondo = c + 1, AtFront = true, Mirror = true }
                : new TopeSpot { Fondo = c, AtFront = false, Mirror = false };

            if (selection.TopeShared && gap)
            {
                yield return low; // one shared bar in the gap; the side is dormant (historic)
                yield break;
            }

            if (selection.Side == SafetySide.Left || selection.Side == SafetySide.Both)
            {
                yield return low;
            }

            if (selection.Side == SafetySide.Right || selection.Side == SafetySide.Both)
            {
                yield return high;
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
            => EnabledOfType(system?.SafetySelections, catalog, view, type);

        /// <summary>System-neutral overload for rack families that compose the same catalog-driven safety subsystem.</summary>
        public static List<SafetyElement> EnabledOfType(
            IEnumerable<SelectiveSafetySelection> selections,
            RackCatalog catalog,
            string view,
            string type,
            bool allowEmptySide = false)
        {
            var result = new List<SafetyElement>();
            if (selections == null || catalog?.SafetyElements == null)
            {
                return result;
            }

            // A catalog type is one family with one active ElementId. Resolve it once so malformed legacy documents
            // containing two variants cannot draw both (or make the lateral disagree with frontal/planta/BOM).
            var selection = SelectiveSafetyFamilies.SelectedOfType(selections, catalog.SafetyElements, type);
            if (selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
            {
                return result;
            }

            // Drawn if the default side draws OR some post overrides to a drawn side.
            // I-42 (S1D, contrato del dueño) — «algun poste» incluye la decision de BOTA de ese poste, que vive en
            // su propia configuracion. Sin eso la general «Ninguno» actuaba como interruptor de la familia y se
            // llevaba por delante los postes que el usuario SI habia configurado: la general es un DEFECTO.
            var drawsSomewhere = selection.DrawsSomewhere();
            if (!drawsSomewhere && !allowEmptySide)
            {
                return result;
            }

            var block = CatalogLookup.Block(catalog, selection.ElementId, view);
            if (!string.IsNullOrWhiteSpace(block))
            {
                result.Add(new SafetyElement { PieceId = selection.ElementId, Block = block, Selection = selection });
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
        /// <param name="physicalFaces">
        /// I-42 (S1E) — true para la familia BOTA, cuya pertenencia se resuelve por UBICACION FISICA (y por lado en
        /// un rack compuesto) y no por el lado historico. La resolucion ya viene hecha del dominio: esta vista no
        /// vuelve a decidir quien lleva pieza, solo donde se ancla y como se orienta.
        /// </param>
        public static void AppendAtPost(
            ICollection<HeaderBlockInstance> target, RackCatalog catalog, string view,
            IReadOnlyList<SafetyElement> elements,
            Point2D postOrigin, string plateId, int postIndex, double? mirrorAxisX = null, double? longitud = null,
            bool mirrorYInPlace = false, SafetySide? sideOverride = null,
            bool physicalFaces = false)
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
                // Owner-validation round 1 (I-32): PERTENENCIA, ORIENTACIÓN y EXTREMO son tres ejes distintos y una
                // sola autoridad los separa. Cada copia llega con los dos últimos ya resueltos:
                //
                //   · AtHighEnd decide DÓNDE va. Con mirrorYInPlace el bloque LATERAL ya cubre el fondo, así que las
                //     dos copias comparten sitio y solo cambia la cara; sin él, la copia alta se refleja sobre el eje
                //     y aterriza en la otra punta de la línea del poste.
                //   · Mirrored decide CÓMO va, y sobrevive aunque el sistema solo tenga extremo bajo: un Right en
                //     Push Back se queda delante, orientado a la derecha en su propio sitio, nunca atrás.
                //
                // La versión anterior colapsaba los dos ejes en un solo SafetySide, y al imponer el extremo bajo
                // perdía la orientación: un Right acababa dibujado como un Left, o desaparecía del corte.
                foreach (var copy in Copies(
                    element.Selection, postIndex, sideOverride, mirrorYInPlace, physicalFaces))
                {
                    target.Add(Piece(
                        element.PieceId, element.Block, view,
                        copy.AtHighEnd ? mirroredAt : at,
                        mirroredX: !mirrorYInPlace && copy.Mirrored,
                        mirroredY: mirrorYInPlace && copy.Mirrored,
                        longitud));
                }
            }
        }

        /// <summary>
        /// Las copias físicas de una pieza en un poste, con la distinción que importa:
        ///
        /// <para>En una vista de PROFUNDIDAD (<paramref name="orientationOnly"/>) las dos copias comparten sitio y solo
        /// se diferencian en la cara: ahí Left/Right es ORIENTACIÓN pura y se lee literal, sin que el extremo tenga
        /// nada que decir. Restringir el extremo en esa vista borraría una de las dos caras de un Both.</para>
        ///
        /// <para>En el resto, la copia espejada además se muda a la otra punta: ahí interviene el EXTREMO y decide
        /// <see cref="SelectiveSafetyEnds"/>, que conserva la orientación elegida.</para>
        ///
        /// <paramref name="sideOverride"/> lo impone el llamador que ya resolvió la pertenencia por su cuenta (la
        /// regla adaptativa de los protectores), y se lee literal como orientación + extremo.
        /// </summary>
        private static IReadOnlyList<SafetyEndCopy> Copies(
            SelectiveSafetySelection selection, int postIndex, SafetySide? sideOverride, bool orientationOnly,
            bool physicalFaces = false)
        {
            if (sideOverride.HasValue)
            {
                return Literal(sideOverride.Value);
            }

            // I-42 (S1): la BOTA elige UBICACIONES FISICAS —que cara de ataque proteger—, tambien en las vistas de
            // profundidad, donde no hay una segunda cara del mismo sitio que orientar. El resto de las familias
            // conserva su lectura de siempre.
            if (physicalFaces)
            {
                return SelectiveSafetyEnds.BootCopiesForPost(selection, postIndex);
            }

            return orientationOnly
                ? Literal(selection?.SideForPost(postIndex) ?? SafetySide.None)
                : SelectiveSafetyEnds.CopiesForPost(selection, postIndex);
        }

        private static IReadOnlyList<SafetyEndCopy> Literal(SafetySide side)
        {
            switch (side)
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
