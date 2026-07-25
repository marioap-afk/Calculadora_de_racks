using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>The two Push Back frontal cuts. Push Back is LIFO, so both are at the SAME (low) aisle, but they show
    /// opposite ends of the lane.</summary>
    public enum PushBackFrontalEnd
    {
        /// <summary>The entrance/exit (low) cut: complete IN/OUT beams + the applicable safety (never a guide).</summary>
        EntradaSalida,

        /// <summary>The rear (high) cut: LARGUERO_ESCALON_TROQUEL_REDONDO beams + rear topes, no normal dynamic safety.</summary>
        Posterior
    }

    /// <summary>
    /// Frontal Push Back cuts by BLACK-BOX composition of <see cref="DynamicSystemFrontalBuilder"/>. It reuses the dynamic
    /// posts/plates/transverse structure but substitutes the beams and safety: <see cref="PushBackFrontalEnd.EntradaSalida"/>
    /// keeps the dynamic exit cut (its safety is already GUIA-free); <see cref="PushBackFrontalEnd.Posterior"/> takes the
    /// dynamic entrance cut, removes every IN/OUT beam and all dynamic safety, and adds one TROQUEL_REDONDO per cell plus a
    /// rear tope per active cell. Instances are identified by Role/PieceId/position, never by group name.
    /// </summary>
    public sealed class PushBackSystemFrontalBuilder
    {
        private const string View = "FRONTAL";
        private readonly DynamicSystemFrontalBuilder dynamicBuilder = new DynamicSystemFrontalBuilder();

        public DynamicSystemPlan BuildPlan(PushBackSystem system, RackCatalog catalog, PushBackFrontalEnd end)
        {
            offsetsByFront.Clear();
            var structure = system?.Structure;
            if (structure == null)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            if (end == PushBackFrontalEnd.EntradaSalida)
            {
                // Low cut: the dynamic exit frontal (structure is GUIA-free) already IS "IN/OUT + applicable safety".
                return HeaderInstanceGrouper.Group(
                    dynamicBuilder.Build(structure, catalog, DynamicRackEnd.Exit),
                    "PB_FRONTAL_ENTRADA_SALIDA");
            }

            var entrance = dynamicBuilder.Build(structure, catalog, DynamicRackEnd.Entrance);
            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            var redondoId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var redondoBlock = CatalogLookup.Block(catalog, redondoId, View);
            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var topePieceId = PushBackRearTopeBuilder.ResolvePieceId(catalog, rearTope);   // PB-005
            var topeBlock = CatalogLookup.Block(catalog, topePieceId, View);
            var saque = rearTope.Saque > 0.0 ? rearTope.Saque : PushBackDefaults.RearTopeSaque;

            // Owner decision (2026-07-24, final): in the REAR FRONTAL the stop is anchored by the post's own TROQUEL_TOPE
            // in this view — never TROQUEL_LARGUERO, which places a beam. Both its X (which follows the post PERALTE) and
            // its Y (the elevation grid base) come from that single measured row; a missing row means no stop is emitted,
            // never a silent fallback to the insertion point.
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(structure, catalog, postId);
            var topeAnchor = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, postPeralte, View);
            var troquelMateY = topeAnchor?.Y ?? 0.0;

            var result = new List<HeaderBlockInstance>();
            foreach (var instance in entrance)
            {
                if (PushBackPlanComposer.IsSafetyPiece(instance))
                {
                    continue; // no normal dynamic safety on the rear cut
                }

                if (PushBackPlanComposer.IsDynamicEndBeam(instance))
                {
                    var (frontIndex, level) = LocateCell(structure, catalog, layout, instance);

                    // Swap the IN/OUT for the rear TROQUEL_REDONDO, keeping the transverse LONGITUD, at the same column.
                    // PB-004 (I-32): its ELEVATION is the lateral one — the beam is tangent to the bed line there, and the
                    // same physical piece cannot sit at two heights in two views (D14 of the Owner's matrix). The offset
                    // is resolved once in the lateral frame by the single authority and applied verbatim here, because an
                    // elevation does not depend on the view. The stop below measures from this SAME shifted elevation, so
                    // the canonical rule ("rise above the rear larguero, snap to the post's grid, +4\"") is preserved.
                    var shift = level >= 0
                        ? RearBeamOffset(system, catalog, structure, frontIndex, level + 1) ?? 0.0
                        : 0.0;
                    var redondo = CloneAt(instance, redondoId, redondoBlock);
                    redondo.DynamicParameters[SelectiveRackDefaults.PeralteParam] = level >= 0
                        ? system.HighEndBeamPeralteAt(frontIndex, level)
                        : PushBackDefaults.HighEndBeamDefaultPeralte;
                    if (shift != 0.0)
                    {
                        redondo.Insertion = new Point2D(redondo.Insertion.X, redondo.Insertion.Y + shift);
                        redondo.ConnectionAnchor = redondo.Insertion;
                    }

                    result.Add(redondo);

                    // Rear tope only for a MATCHED, active cell, placed by the canonical Selective rule (rise + snap).
                    if (!string.IsNullOrWhiteSpace(topeBlock) && level >= 0 && rearTope.At(frontIndex, level)
                        && topeAnchor.HasValue)
                    {
                        // Owner clarification (2026-07-25): the tope block mates by its ORIGIN, so its insertion sits on
                        // the POST's TROQUEL_TOPE — resolved from the post instance of this very plan, not from the
                        // beam's insertion (which is what left it on the wrong troquel). The post carries a COLUMN of
                        // stop holes every 2", so the X coincides exactly while the Y keeps the approved rise-and-snap
                        // (+4") measured from that same TROQUEL_TOPE.
                        var mate = PushBackRearTopeBuilder.PostMateWorld(
                            catalog, postId, postPeralte, View, entrance, instance.Insertion);
                        if (!mate.HasValue)
                        {
                            continue;   // no measured post point: no stop, never a raw fallback
                        }

                        var topeY = PushBackRearTopeBuilder.ElevationY(troquelMateY, instance.Insertion.Y + shift);
                        var topeX = mate.Value.X;
                        double? longitud = instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var beamLength)
                            ? beamLength + SelectiveTopePlacement.LengthAllowance
                            : (double?)null;
                        result.Add(SelectiveTopePlacement.Tope(
                            topePieceId, topeBlock, View,
                            topeX, topeY, saque, longitud,
                            mirroredX: PushBackRearTopeBuilder.Mirrored(View, instance.MirroredX)));
                    }

                    continue;
                }

                result.Add(instance); // keep posts/plates/decorations
            }

            return HeaderInstanceGrouper.Group(result, "PB_FRONTAL_POSTERIOR");
        }

        /// <summary>
        /// The lateral tangency offset of one cell's rear beam, or null when the cell has no resolved bed axis. Cached
        /// per front so a frontal cut resolves each front's axes once, not once per level.
        /// </summary>
        private double? RearBeamOffset(PushBackSystem system, RackCatalog catalog, DynamicRackSystem structure, int frontIndex, int levelNumber)
        {
            if (frontIndex < 0 || frontIndex >= structure.Fronts.Count)
            {
                return null;
            }

            if (!offsetsByFront.TryGetValue(frontIndex, out var offsets))
            {
                offsets = PushBackLoadBeamGeometry.RearBeamElevationOffsets(system, catalog, structure.Fronts[frontIndex]);
                offsetsByFront[frontIndex] = offsets;
            }

            return offsets.TryGetValue(levelNumber, out var offset) ? offset : (double?)null;
        }

        private readonly Dictionary<int, IReadOnlyDictionary<int, double>> offsetsByFront =
            new Dictionary<int, IReadOnlyDictionary<int, double>>();

        private static HeaderBlockInstance CloneAt(HeaderBlockInstance source, string pieceId, string block)
        {
            var clone = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = pieceId,
                BlockName = block,
                View = View,
                Insertion = source.Insertion,
                ConnectionAnchor = source.ConnectionAnchor,
                RotationRadians = source.RotationRadians,
                MirroredX = source.MirroredX,
                MirroredY = source.MirroredY
            };
            if (source.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var length))
            {
                clone.DynamicParameters[SelectiveRackDefaults.LengthParam] = length;
            }

            return clone;
        }

        /// <summary>Explicit tolerance (in) for matching a frontal beam's Y to a level's entrance elevation.</summary>
        private const double LevelMatchTolerance = 0.05;

        /// <summary>
        /// Recover (frontIndex, 0-based level) of a frontal IN/OUT beam. The front comes from its X column
        /// (post+troquel); the level comes from THAT FRONT'S OWN load-beam levels (never the global projection), matched
        /// by entrance elevation within <see cref="LevelMatchTolerance"/>. Returns level = -1 (no silent front-0/level-0
        /// fallback) when nothing matches, so the caller neither mislabels the peralte nor draws a wrong-cell tope.
        /// </summary>
        private static (int FrontIndex, int Level) LocateCell(DynamicRackSystem system, RackCatalog catalog, DynamicFrontLayout layout, HeaderBlockInstance beam)
        {
            var frontIndex = -1;
            var bestX = double.MaxValue;
            for (var index = 0; index < Math.Min(layout.PostPositions.Count, layout.TroquelPositions.Count) && index < system.Fronts.Count; index++)
            {
                var columnX = layout.PostPositions[index] + layout.TroquelPositions[index];
                var distance = Math.Abs(columnX - beam.Insertion.X);
                if (distance < bestX)
                {
                    bestX = distance;
                    frontIndex = index;
                }
            }

            if (frontIndex < 0)
            {
                return (-1, -1);
            }

            // Use the identified FRONT'S OWN levels — a front may have a different FirstLevelHeight, level count or
            // vertical configuration than the global projection.
            var frontLevels = DynamicFrontGeometry.LoadBeamLevels(system, system.Fronts[frontIndex]);
            var level = -1;
            var bestY = double.MaxValue;
            for (var index = 0; index < frontLevels.Count; index++)
            {
                var distance = Math.Abs(frontLevels[index].EntranceElevation - beam.Insertion.Y);
                if (distance < bestY)
                {
                    bestY = distance;
                    level = index;
                }
            }

            return level >= 0 && bestY <= LevelMatchTolerance ? (frontIndex, level) : (frontIndex, -1);
        }
    }
}
