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
    /// carries the same complete IN/OUT beam as the dynamic system, pero su ELEVACIÓN ya no es la del resolver: la
    /// deriva <see cref="PushBackElevations"/> desde el larguero posterior y la ajusta a su propio troquel (PB-004,
    /// I-32). El HIGH (right, rear) end carries <c>LARGUERO_ESCALON_TROQUEL_REDONDO</c> with the cell's own PERALTE
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
        /// La elevación de inserción del larguero de ENTRADA/SALIDA, por nivel, para un frente. Delega en la autoridad
        /// única <see cref="PushBackElevations"/>: aquí no vive ninguna fórmula, solo la forma que consumen las vistas.
        /// </summary>
        public static IReadOnlyDictionary<int, double> LowBeamElevations(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
            => PushBackElevations.LowInsertions(system, catalog, front);

        /// <summary>
        /// Low-end IN/OUT beams: one per front x level, taken from the dynamic exit placements VERBATIM.
        ///
        /// PB-004 (I-32, tras el rechazo del round 1): el larguero POSTERIOR no se desplaza — es el ANCLA y conserva
        /// su troquel. El que se deriva es ESTE: su elevación sale de <see cref="PushBackElevations"/>, que aplica la
        /// pendiente nominal desde el contacto posterior y ajusta el resultado al troquel válido más cercano. Los dos
        /// extremos quedan atornillados y la cama se traza entre los dos contactos reales.
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
        /// en la elevación de troquel que ya les dio el resolver. NO se desplazan: son el ancla de la que cuelga todo
        /// lo demás (PB-004, I-32). Su contacto con la cama lo elige la geometría entre las dos aristas medidas
        /// (<see cref="RearBeamTangencyPointWorld"/>), no un lado fijo del catálogo.
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
