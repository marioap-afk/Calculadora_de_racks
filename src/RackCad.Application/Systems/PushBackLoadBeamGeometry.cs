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

        /// <summary>Low-end IN/OUT beams (the dynamic exit placements, unchanged): one per front x level.</summary>
        public static IReadOnlyList<HeaderBlockInstance> LowBeams(PushBackSystem system, RackCatalog catalog, DynamicRackFront front = null)
        {
            var result = new List<HeaderBlockInstance>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

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

                var origin = new Point2D(placement.X, placement.Y);
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
