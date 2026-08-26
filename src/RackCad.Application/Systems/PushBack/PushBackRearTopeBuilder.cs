using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The rear pallet-stop ("larguero tope") of a Push Back system: one stop per front and load level at the HIGH
    /// (rear) end, active by default and deactivable through <see cref="PushBackRearTopeConfig.OffCells"/>. Which
    /// catalog TOPE variant is placed is chosen by the user and resolved by <see cref="ResolvePieceId"/> (PB-005);
    /// <see cref="TopePieceId"/> is only the default. It uses the CANONICAL Selective tope rule
    /// (<see cref="SelectiveTopePlacement"/>): it rises above the rear larguero and snaps to the post's TROQUEL grid, with
    /// SAQUE and LONGITUD. Planta draws top-down and keeps the frente Y (no rise-and-snap). Counted on its own
    /// (<see cref="HeaderBlockRole.Tope"/>), one physical piece per active cell across the lateral/rear-frontal/planta.
    /// </summary>
    public sealed class PushBackRearTopeBuilder
    {
        /// <summary>The DEFAULT rear-stop piece. Selectable variants are resolved by <see cref="ResolvePieceId"/>.</summary>
        public const string TopePieceId = "LARGUERO_ESCALON_TOPE_DE_3";

        /// <summary>
        /// PB-005 (I-32) — THE rule that picks the rear-stop piece, so the three views and the BOM cannot disagree.
        /// The configured id wins when the CURRENT catalog lists it as a TOPE; anything else — blank (every legacy
        /// document), an unknown id, a renamed row, a piece of another family — falls back to <see cref="TopePieceId"/>.
        ///
        /// Falling back rather than honouring a blank is deliberate: a blank id would resolve to no block at all and
        /// the rack would silently lose every stop in the drawing AND an empty ProfileId in the BOM.
        /// </summary>
        public static string ResolvePieceId(RackCatalog catalog, PushBackRearTopeConfig config)
        {
            var requested = config?.PieceId;
            if (string.IsNullOrWhiteSpace(requested))
            {
                return TopePieceId;
            }

            var known = catalog?.SafetyElements?.Any(entry => entry != null
                && string.Equals(entry.Id, requested, StringComparison.OrdinalIgnoreCase)
                && SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.TopeType)) ?? false;
            return known ? requested.Trim() : TopePieceId;
        }

        /// <summary>
        /// Extra rise of the rear tope ABOVE the canonical Selective rise-and-snap (PB-VAL-03: the Owner measured the tope
        /// sitting exactly 4" too low in AutoCAD). It is exactly TWO <see cref="SelectiveRackDefaults.TroquelPaso"/> steps,
        /// so the tope stays on the very same TROQUEL grid the Selective snap lands on — the snap rule is preserved by
        /// construction, not bypassed. Elevation views only; PLANTA has no elevation.
        /// </summary>
        public const double ExtraRise = 2.0 * SelectiveRackDefaults.TroquelPaso;

        /// <summary>True when <paramref name="view"/> is the top view (which keeps the frente Y, no rise-and-snap).</summary>
        public static bool IsPlanta(string view) => string.Equals(view, "PLANTA", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Owner decision (2026-07-24) — the rear tope's facing, INVERTED with respect to 10d8eeb, where the Owner
        /// measured it upside down on the drawing. In the elevation views it is drawn UNMIRRORED; PLANTA is a top view
        /// where the tope lies ALONG the beam, so there it keeps the beam's plan orientation (unchanged and approved).
        /// The rule never reads the rear BEAM's mirror for the elevations: a beam's mirror orients a BEAM profile, not
        /// the tope's step.
        /// </summary>
        public static bool Mirrored(string view, bool beamMirroredX)
            => IsPlanta(view) ? !beamMirroredX : ElevationMirrored;

        /// <summary>The rear tope's mirror in the elevation views. Owner decision (2026-07-24): the inverse of 10d8eeb.</summary>
        public const bool ElevationMirrored = false;

        /// <summary>
        /// Owner decision (2026-07-24) — the rear tope's world ANCHOR: the vertical axis of the POST's
        /// <c>TROQUEL_SEPARADOR</c> (<see cref="DynamicRackDefaults.SeparatorPostPoint"/>), resolved for the post's own
        /// PERALTE and transformed by the mirror, exactly as the Selective lateral anchors its own topes and separators.
        ///
        /// It is explicitly NOT the rear beam's contact points and NOT the raw placement X: the stop belongs to the post,
        /// not to the beam. PLANTA takes the point measured in the PLANTA view (whose Y runs with the depth and depends
        /// on the peralte); the elevations take the LATERAL/FRONTAL one. Returns null when the catalog carries no such
        /// row for the post — a missing mate is a missing physical contract and must never degrade to the insertion point.
        /// </summary>
        public static Point2D? SeparatorAnchorLocal(RackCatalog catalog, string postId, double postPeralte, string view)
            => PostAnchorLocal(catalog, postId, postPeralte, view);

        /// <summary>
        /// Owner decision (2026-07-24, final) — WHICH post point anchors the rear tope in each view, audited against the
        /// real rows of <c>connection-layout.csv</c>:
        /// <list type="bullet">
        /// <item>LATERAL → <c>TROQUEL_SEPARADOR</c>: the stop sits on the separator's vertical axis, and its elevation
        /// grid is measured from that same point (never from <c>TROQUEL_LARGUERO</c>, which the post does not even
        /// publish in this view).</item>
        /// <item>FRONTAL → <c>TROQUEL_TOPE</c>: the post's own stop hole, whose X follows the post PERALTE.</item>
        /// <item>PLANTA → <c>TROQUEL_TOPE</c> measured in PLANTA, whose depth offset follows the post PERALTE — not the
        /// separator's, which points the other way.</item>
        /// </list>
        /// Null when the catalog has no such row: a missing mate is a missing physical contract and never degrades to
        /// the insertion point.
        /// </summary>
        public static Point2D? PostAnchorLocal(RackCatalog catalog, string postId, double postPeralte, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(postId, AnchorPoint(view), view);
            return entry == null
                ? (Point2D?)null
                : SelectivePostGeometry.Resolve(
                    entry,
                    new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = postPeralte });
        }

        /// <summary>The post connection point that anchors the tope in <paramref name="view"/>.</summary>
        public static string AnchorPoint(string view)
            => string.Equals(view, "LATERAL", StringComparison.OrdinalIgnoreCase)
                ? DynamicRackDefaults.SeparatorPostPoint
                : TopePostPoint;

        /// <summary>The post's own stop hole. Never <c>TROQUEL_LARGUERO</c>: that one places a BEAM, not a stop.</summary>
        public const string TopePostPoint = "TROQUEL_TOPE";

        /// <summary>
        /// Owner clarification (2026-07-25) — the <c>LARGUERO_ESCALON_TOPE_DE_3</c> block mates by its ORIGIN: its
        /// insertion point IS its mate point, so the block publishes no connection point of its own. Placing the stop is
        /// therefore "put its origin exactly on the POST's TROQUEL_TOPE, in world coordinates".
        ///
        /// This resolves that world point from the plan itself: it picks the POST instance nearest to
        /// <paramref name="near"/> (the rear beam being served) and transforms the post's measured local point by that
        /// post's own placement, mirror included. Using the post from the plan — instead of the beam's insertion, which
        /// is what placed the stop on the larguero troquel — is the whole correction.
        ///
        /// Null when the catalog has no such row or the plan carries no post: never a fallback to an insertion point.
        /// </summary>
        public static Point2D? PostMateWorld(
            RackCatalog catalog, string postId, double postPeralte, string view,
            IEnumerable<HeaderBlockInstance> planInstances, Point2D near)
        {
            var local = PostAnchorLocal(catalog, postId, postPeralte, view);
            if (!local.HasValue || planInstances == null)
            {
                return null;
            }

            HeaderBlockInstance post = null;
            var best = double.MaxValue;
            foreach (var instance in planInstances)
            {
                if (instance == null || instance.Role != HeaderBlockRole.Post)
                {
                    continue;
                }

                var dx = instance.Insertion.X - near.X;
                var dy = instance.Insertion.Y - near.Y;
                var distance = dx * dx + dy * dy;
                if (distance < best)
                {
                    best = distance;
                    post = instance;
                }
            }

            if (post == null)
            {
                return null;
            }

            return new Point2D(
                post.Insertion.X + (post.MirroredX ? -local.Value.X : local.Value.X),
                post.Insertion.Y + local.Value.Y);
        }

        /// <summary>The rear tope Y in an ELEVATION view: the canonical Selective rise-and-snap plus <see cref="ExtraRise"/>.</summary>
        public static double ElevationY(double troquelMateY, double largueroY)
            => SelectiveTopePlacement.SnapY(troquelMateY, largueroY, SelectiveRackDefaults.TroquelPaso) + ExtraRise;

        /// <summary>Rear topes in the LATERAL view (rise-and-snap above the rear beam of each active cell of the front).</summary>
        /// <param name="levels">
        /// I-42 — los NIVELES a materializar, o null para todos (que es lo que hace cualquier llamador anterior a la
        /// iniciativa). Un rack compuesto lo necesita porque un nivel puede pertenecer a una cama corrida y no a la de
        /// este lado: sin el filtro, la celda emitiria dos veces la misma pieza fisica.
        /// </param>
        public IReadOnlyList<HeaderBlockInstance> BuildLateral(
            PushBackSystem system, RackCatalog catalog, int frontIndex, DynamicRackFront front = null,
            IReadOnlyCollection<int> levels = null)
            => Build(system, catalog, frontIndex, front, "LATERAL", levels);

        /// <summary>Rear topes in <paramref name="view"/>. LATERAL/FRONTAL rise-and-snap; PLANTA keeps the frente Y.</summary>
        /// <param name="levels">
        /// I-42 — los NIVELES a materializar, o null para todos (que es lo que hace cualquier llamador anterior a la
        /// iniciativa). Un rack compuesto lo necesita porque un nivel puede pertenecer a una cama corrida y no a la de
        /// este lado: sin el filtro, la celda emitiria dos veces la misma pieza fisica.
        /// </param>
        public IReadOnlyList<HeaderBlockInstance> Build(
            PushBackSystem system, RackCatalog catalog, int frontIndex, DynamicRackFront front, string view,
            IReadOnlyCollection<int> levels = null)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var pieceId = ResolvePieceId(catalog, rearTope);   // PB-005: one rule for the piece, shared by every view
            var block = CatalogLookup.Block(catalog, pieceId, view);
            if (string.IsNullOrWhiteSpace(block))
            {
                return result;
            }

            var saque = rearTope.Saque > 0.0 ? rearTope.Saque : PushBackDefaults.RearTopeSaque;
            var keepFrenteY = IsPlanta(view);
            var troquelMateY = keepFrenteY ? 0.0 : PostTroquelGridBase(structure, catalog, view);
            // Owner decision (2026-07-24): the anchor is the POST's TROQUEL_SEPARADOR axis, in the view's own measured
            // point (PLANTA has its own row, whose Y runs with the depth and depends on the peralte). Never the rear
            // beam's points, never the bare placement.
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var separator = PostAnchorLocal(
                catalog, postId, DynamicFrontGeometry.PostPeralte(structure, catalog, postId), view);

            // El tope cuelga de SU larguero alto, asi que su referencia es la elevacion DERIVADA de ese larguero —
            // la que publica PushBackElevations— y no la que el resolver compartido le dio al nivel. Desde la
            // inversion vertical de I-42 las dos ya no coinciden: medir desde la del resolver dejaba el tope a la
            // altura de un larguero que no esta ahi, y ademas discrepando del corte frontal, que si consume la
            // derivada. Sin entrada para el nivel se conserva la colocacion, que es lo que hace un rack sin camas.
            var highInsertions = PushBackElevations.HighInsertions(system, catalog, front);

            foreach (var placement in PushBackPlacements.Resolve(system, front)
                         .Where(placement => placement.IsEntrance)
                         .Where(placement => levels == null || levels.Contains(placement.LevelNumber)))
            {
                var levelIndex = placement.LevelNumber - 1;
                if (!rearTope.At(frontIndex, levelIndex))
                {
                    continue; // this cell's rear tope is deactivated
                }

                if (!separator.HasValue)
                {
                    continue;   // no measured TROQUEL_SEPARADOR: no anchor, and never a raw-placement fallback
                }

                // The stop sits on the POST's separator axis. The post shares the rear beam's column, so the mirror that
                // transforms the local point is the placement's; PLANTA also takes the separator's own depth offset.
                var x = placement.X + (placement.MirroredX ? -separator.Value.X : separator.Value.X);
                var beamY = highInsertions.TryGetValue(placement.LevelNumber, out var resolved)
                    ? resolved
                    : placement.Y;
                var y = keepFrenteY
                    ? placement.Y + separator.Value.Y
                    : ElevationY(troquelMateY, beamY);
                // Commercial LONGITUD = the corresponding transverse beam length (per front x level) + the allowance.
                var baseLength = front != null
                    ? PushBackLoadBeamGeometry.CellBeamLength(structure, front, placement.LevelNumber)
                    : placement.BeamLength;
                double? longitud = baseLength > 0.0
                    ? baseLength + SelectiveTopePlacement.LengthAllowance
                    : (double?)null;
                result.Add(SelectiveTopePlacement.Tope(
                    pieceId, block, view, x, y, saque, longitud,
                    mirroredX: Mirrored(view, placement.MirroredX)));
            }

            return result;
        }

        /// <summary>
        /// The elevation snap grid's BASE: the Y of the view's own anchor point (see <see cref="AnchorPoint"/>), resolved
        /// with the post's real peralte. Owner decision (2026-07-24): the lateral measures from TROQUEL_SEPARADOR, the
        /// frontal from TROQUEL_TOPE — never from TROQUEL_LARGUERO, and never from another view's row.
        /// </summary>
        /// <summary>
        /// La retícula de troqueles del POSTE en una vista de elevacion: la base sobre la que el tope se ajusta.
        /// Publica porque es parte del contrato vertical del tope y las pruebas lo comprueban contra ella, no contra
        /// una copia — una segunda base seria una segunda autoridad.
        /// </summary>
        public static double PostTroquelGridBase(DynamicRackSystem structure, RackCatalog catalog, string view)
        {
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(structure, catalog, postId);
            return PostAnchorLocal(catalog, postId, postPeralte, view)?.Y ?? 0.0;
        }
    }
}
