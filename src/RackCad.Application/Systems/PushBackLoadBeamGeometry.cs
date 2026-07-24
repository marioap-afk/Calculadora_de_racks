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
        /// PB-VAL-05 — the vertical shift that lands the beam's TANGENCY POINT (above) on the line through the bed's
        /// PHYSICAL origin (<see cref="PushBackFlowBedAxis.RailOrigin"/>), evaluated AT THAT POINT'S OWN X, instead of on
        /// the TROQUEL line through <see cref="PushBackFlowBedAxis.ExitMate"/>. Both lines are PARALLEL (same axis angle),
        /// so this is a pure vertical constant per level: it cannot change the bed's slope, axis, origin or length — the
        /// bed is resolved from the RAW placements and never sees this shift (see <see cref="LowBeams"/>).
        ///
        /// The axis's <see cref="PushBackFlowBedAxis.ExitMate"/> IS that transformed tangency point (the bed resolver
        /// mates the very same catalog point), which is why the shift can be read off the axis alone.
        /// Returns 0 for a level with no resolved bed axis: with no bed there is no bed-origin line to be tangent to,
        /// so the beam keeps its troquel-snapped elevation rather than being moved by an unrelated level's line.
        /// </summary>
        public static double BedOriginOffset(IReadOnlyList<PushBackFlowBedAxis> axes, int levelNumber)
        {
            if (axes == null)
            {
                return 0.0;
            }

            foreach (var axis in axes)
            {
                if (axis.LevelNumber == levelNumber)
                {
                    return axis.RailOriginYAt(axis.ExitMate.X) - axis.ExitMate.Y;
                }
            }

            return 0.0;
        }

        /// <summary>
        /// Low-end IN/OUT beams: one per front x level, taken from the dynamic exit placements and then lowered onto the
        /// BED-ORIGIN line (PB-VAL-05). The bed is the geometric authority: its axis, origin, slope and full length are
        /// resolved from the RAW placements — <see cref="PushBackFlowBedGeometry.Resolve"/> never sees this shift — so the
        /// bed is never displaced to accommodate the beam; the beam is what moves. The rear beam and the intermediates,
        /// which resolve against the troquel snap and the bed axis respectively, are untouched.
        /// </summary>
        public static IReadOnlyList<HeaderBlockInstance> LowBeams(PushBackSystem system, RackCatalog catalog, DynamicRackFront front = null)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
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

                var origin = new Point2D(placement.X, placement.Y + BedOriginOffset(axes, placement.LevelNumber));
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

        /// <summary>High-end (rear) TROQUEL_REDONDO beams: one per front x level, PERALTE from the cell, LONGITUD = the IN/OUT's.</summary>
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
