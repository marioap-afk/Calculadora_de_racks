using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The two Push Back end beams per front and level, in the LATERAL view. Push Back is LIFO: the LOW (left) end
    /// carries the same complete IN/OUT beam as the dynamic system (reused verbatim from the already-snapped exit
    /// placement); the HIGH (right, rear) end carries <c>LARGUERO_ESCALON_TROQUEL_REDONDO</c> with the cell's own PERALTE
    /// (<see cref="PushBackSystem.HighEndBeamPeralteAt"/>) and the same transverse LONGITUD as the corresponding IN/OUT.
    /// Both origins come from <see cref="DynamicLoadBeamGeometry.Placements"/>, whose Y is already snapped to the 2" troquel.
    /// </summary>
    public static class PushBackLoadBeamGeometry
    {
        /// <summary>
        /// The transverse LONGITUD of one cell's beams, PER FRONT AND LEVEL — the same rule the resolved cell carries:
        /// the resolved cell's <see cref="DynamicRackLevel.BeamLength"/> when valid, else the front's. Both the low IN/OUT
        /// and the high TROQUEL_REDONDO of a cell share this length, and the drawing (lateral) and the BOM consume it, so
        /// they cannot drift by level. With no per-level override the cell length equals the front length (so the golden
        /// views are unchanged); a per-level <c>BeamLengthOverride</c> makes them differ by level.
        /// </summary>
        public static double CellBeamLength(DynamicRackSystem structure, DynamicRackFront front, int levelNumber)
        {
            if (structure == null || front == null)
            {
                return 0.0;
            }

            var cell = DynamicRackLevelGeometry.At(structure, front, levelNumber);
            return cell.BeamLength > 0.0 ? cell.BeamLength : front.BeamLength;
        }

        /// <summary>
        /// PB-VAL-05 — the CANONICAL tangency point of the low IN/OUT beam: the catalog's <c>TROQUEL_CAMA</c>
        /// (<see cref="DynamicRackDefaults.InOutBeamBedMatePoint"/>) in the beam's own view, in BLOCK-LOCAL coordinates.
        ///
        /// This is a named, measured point of the real block, not the insertion point and not an offset: the Owner
        /// confirmed on the DWG that TROQUEL_CAMA IS the beam's physical contact face with the bed, so consuming it as
        /// the tangency point is DEMONSTRATED, not assumed. Returns null when the catalog carries no such row — a
        /// missing mate is a missing physical contract and must never fall back to the block origin.
        /// </summary>
        public static Point2D? BedTangencyPointLocal(RackCatalog catalog, string beamId)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(
                string.IsNullOrWhiteSpace(beamId) ? DynamicRackDefaults.InOutBeamCatalogId : beamId,
                DynamicRackDefaults.InOutBeamBedMatePoint,
                DynamicRackDefaults.InOutBeamView);
            return entry == null ? (Point2D?)null : new Point2D(entry.LocalX, entry.LocalY);
        }

        /// <summary>
        /// PB-VAL-05 — the world position of a placed beam's tangency point: its local <c>TROQUEL_CAMA</c> transformed by
        /// the placement (mirror included), exactly like <see cref="PushBackFlowBedGeometry"/> transforms the bed mates.
        /// </summary>
        public static Point2D BedTangencyPointWorld(Point2D localTangency, double placementX, double placementY, bool mirroredX)
            => new Point2D(placementX + (mirroredX ? -localTangency.X : localTangency.X), placementY + localTangency.Y);

        /// <summary>
        /// Owner decision (2026-07-24) — the REAR beam's tangency: of its two measured bed-contact edges
        /// (<see cref="PushBackDefaults.HighEndBeamLeftBedMatePoint"/> / <see cref="PushBackDefaults.HighEndBeamRightBedMatePoint"/>),
        /// the one the bed actually lands on, transformed by the placement's mirror.
        ///
        /// Both edges share the block's contact-face Y, and the bed line RISES toward +X, so lowering the beam onto that
        /// line makes contact at the edge where the line is HIGHEST — the one at the greater WORLD X. Picking it by
        /// geometry (not by a fixed left/right constant) is what keeps the rule correct when the beam is mirrored.
        /// Null when the catalog carries neither edge: a missing contact face is a missing physical contract and must
        /// never fall back to the insertion point.
        /// </summary>
        public static Point2D? RearBeamTangencyPointWorld(RackCatalog catalog, string beamId, double placementX, double placementY, bool mirroredX)
        {
            Point2D? best = null;
            foreach (var pointId in new[] { PushBackDefaults.HighEndBeamLeftBedMatePoint, PushBackDefaults.HighEndBeamRightBedMatePoint })
            {
                var entry = catalog?.ConnectionLayout.FindConnectionLayout(beamId, pointId, PushBackDefaults.HighEndBeamView);
                if (entry == null)
                {
                    continue;
                }

                var world = new Point2D(placementX + (mirroredX ? -entry.LocalX : entry.LocalX), placementY + entry.LocalY);
                if (!best.HasValue || world.X > best.Value.X)
                {
                    best = world;
                }
            }

            return best;
        }

        /// <summary>
        /// Owner decision (2026-07-24) — the vertical shift that lands the REAR beam's contact edge on the line through
        /// the bed's PHYSICAL origin (<see cref="PushBackFlowBedAxis.RailOrigin"/>), evaluated at that edge's own X.
        ///
        /// The bed is resolved from the RAW placements (<see cref="PushBackFlowBedGeometry.Resolve"/> never sees this
        /// shift), so its origin, slope, axis and full length are untouched: the bed does not move to meet the beam, the
        /// REAR beam moves to meet the bed. The LOW IN/OUT beam is NOT shifted at all — it stays bolted where its
        /// TROQUEL_CAMA meets the rail's TROQUEL_IN, which is the mate the bed itself is resolved from.
        ///
        /// Returns 0 for a level with no resolved bed axis or a beam with no measured contact edge: with no bed line
        /// there is nothing to be tangent to, so the beam keeps its troquel-snapped elevation rather than being moved
        /// by an unrelated level's line.
        /// </summary>
        public static double RearBeamTangencyOffset(
            IReadOnlyList<PushBackFlowBedAxis> axes, int levelNumber,
            RackCatalog catalog, string beamId, double placementX, double placementY, bool mirroredX)
        {
            if (axes == null)
            {
                return 0.0;
            }

            foreach (var axis in axes)
            {
                if (axis.LevelNumber != levelNumber)
                {
                    continue;
                }

                var contact = RearBeamTangencyPointWorld(catalog, beamId, placementX, placementY, mirroredX);
                return contact.HasValue ? axis.RailOriginYAt(contact.Value.X) - contact.Value.Y : 0.0;
            }

            return 0.0;
        }

        /// <summary>
        /// PB-004 (I-32, Owner rule after round 1) — THE elevation of the ENTRANCE/EXIT beam for one cell.
        ///
        /// The rear beam is the anchor: <paramref name="highContact"/> is its REAL contact point, already on a valid
        /// troquel. From there the theoretical low contact is the nominal slope applied over the run between the two
        /// contacts; converting it to an INSERTION elevation subtracts the beam's own <c>TROQUEL_CAMA</c> datum
        /// (<paramref name="lowMateLocalY"/>); and that insertion is finally snapped to its own nearest troquel, so
        /// this beam is bolted too. The rise that comes out of it is the RESULTING one, not the nominal target.
        /// </summary>
        public static double LowBeamElevation(Point2D highContact, double lowContactX, double lowMateLocalY, double gridBase)
        {
            var run = highContact.X - lowContactX;
            var theoreticalContact = highContact.Y - PushBackBedSlope.Rise(run);
            return PushBackTroquelGrid.Snap(theoreticalContact - lowMateLocalY, gridBase);
        }

        /// <summary>
        /// The derived-and-snapped elevation of the ENTRANCE/EXIT beam, PER LEVEL, for one front. It is the single
        /// authority every view consumes, so the lateral, the low frontal cut, the bed and the BOM cannot disagree.
        /// </summary>
        public static IReadOnlyDictionary<int, double> LowBeamElevations(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
        {
            var result = new Dictionary<int, double>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var highBeamId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var highColumn = catalog?.ConnectionLayout.FindConnectionLayout(
                highBeamId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);
            if (highColumn == null)
            {
                return result;   // sin fila medida no hay ancla: no se inventa una
            }

            var gridBase = PushBackTroquelGrid.Base(structure, catalog);
            var placements = DynamicLoadBeamGeometry.Placements(structure, front);
            foreach (var level in placements.Select(p => p.LevelNumber).Distinct())
            {
                var low = placements.FirstOrDefault(p => p.LevelNumber == level && !p.IsEntrance);
                var high = placements.FirstOrDefault(p => p.LevelNumber == level && p.IsEntrance);
                if (low == null || high == null)
                {
                    continue;
                }

                var lowBeamId = string.IsNullOrWhiteSpace(low.BeamCatalogId) ? DynamicRackDefaults.InOutBeamCatalogId : low.BeamCatalogId;
                var lowMateLocal = CatalogLookup.Local(catalog, lowBeamId, DynamicRackDefaults.InOutBeamBedMatePoint, DynamicRackDefaults.InOutBeamView);
                var highContact = new Point2D(
                    high.X + (high.MirroredX ? -highColumn.LocalX : highColumn.LocalX),
                    high.Y + highColumn.LocalY);
                var lowContactX = low.X + (low.MirroredX ? -lowMateLocal.X : lowMateLocal.X);
                if (highContact.X - lowContactX <= 0.0)
                {
                    continue;
                }

                result[level] = LowBeamElevation(highContact, lowContactX, lowMateLocal.Y, gridBase);
            }

            return result;
        }

        /// <summary>
        /// Low-end IN/OUT beams: one per front x level, taken from the dynamic exit placements VERBATIM.
        ///
        /// Owner decision (2026-07-24): the low beam is placed so that its <c>TROQUEL_CAMA</c> coincides EXACTLY with the
        /// rail's <c>TROQUEL_IN</c> — the very mate the bed is resolved from — so it carries NO shift of its own. The
        /// earlier bed-origin displacement of this beam is gone: it is the REAR beam that moves onto the bed-origin line
        /// (<see cref="RearBeamTangencyOffset"/>).
        /// </summary>
        public static IReadOnlyList<HeaderBlockInstance> LowBeams(PushBackSystem system, RackCatalog catalog, DynamicRackFront front = null)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var elevations = LowBeamElevations(system, catalog, front);
            foreach (var placement in DynamicLoadBeamGeometry.Placements(structure, front).Where(placement => !placement.IsEntrance))
            {
                var beamId = string.IsNullOrWhiteSpace(placement.BeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : placement.BeamCatalogId;
                var block = CatalogLookup.Block(catalog, beamId, DynamicRackDefaults.InOutBeamView);
                if (string.IsNullOrWhiteSpace(block))
                {
                    continue;
                }

                var origin = new Point2D(
                    placement.X,
                    elevations.TryGetValue(placement.LevelNumber, out var derived) ? derived : placement.Y);
                result.Add(new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Beam,
                    PieceId = beamId,
                    BlockName = block,
                    View = DynamicRackDefaults.InOutBeamView,
                    Insertion = origin,
                    ConnectionAnchor = origin,
                    MirroredX = placement.MirroredX
                });
            }

            return result;
        }

        /// <summary>
        /// High-end (rear) TROQUEL_REDONDO beams: one per front x level, PERALTE from the cell, LONGITUD = the IN/OUT's,
        /// and lowered so that its measured contact edge is TANGENT to the bed-origin line
        /// (<see cref="RearBeamTangencyOffset"/>, Owner decision 2026-07-24). The shift is vertical only, so the bed's
        /// slope, full length, per-cell peralte and the intermediates are untouched.
        /// </summary>
        public static IReadOnlyList<HeaderBlockInstance> HighBeams(PushBackSystem system, RackCatalog catalog, int frontIndex, DynamicRackFront front = null)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var beamId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var block = CatalogLookup.Block(catalog, beamId, PushBackDefaults.HighEndBeamView);
            if (string.IsNullOrWhiteSpace(block))
            {
                return result;
            }

            foreach (var placement in DynamicLoadBeamGeometry.Placements(structure, front).Where(placement => placement.IsEntrance))
            {
                // PB-004: el posterior ES el ancla — se queda en el troquel que le dio el resolver, sin desplazamiento.
                var origin = new Point2D(placement.X, placement.Y);
                var instance = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Beam,
                    PieceId = beamId,
                    BlockName = block,
                    View = PushBackDefaults.HighEndBeamView,
                    Insertion = origin,
                    ConnectionAnchor = origin,
                    MirroredX = placement.MirroredX
                };
                instance.DynamicParameters[SelectiveRackDefaults.PeralteParam] =
                    system.HighEndBeamPeralteAt(frontIndex, placement.LevelNumber - 1);
                // The high beam's LONGITUD is the cell's transverse length (same as its low IN/OUT), per front and level.
                var length = front != null ? CellBeamLength(structure, front, placement.LevelNumber) : placement.BeamLength;
                if (length > 0.0)
                {
                    instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = length;
                }

                result.Add(instance);
            }

            return result;
        }
    }
}
