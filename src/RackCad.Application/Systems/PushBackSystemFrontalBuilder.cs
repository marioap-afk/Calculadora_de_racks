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
            var topeBlock = CatalogLookup.Block(catalog, PushBackRearTopeBuilder.TopePieceId, View);
            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var saque = rearTope.Saque > 0.0 ? rearTope.Saque : PushBackDefaults.RearTopeSaque;

            var result = new List<HeaderBlockInstance>();
            foreach (var instance in entrance)
            {
                if (PushBackPlanComposer.IsSafetyPiece(instance))
                {
                    continue; // no normal dynamic safety on the rear cut
                }

                if (PushBackPlanComposer.IsDynamicEndBeam(instance))
                {
                    var (frontIndex, level) = LocateCell(structure, layout, instance);

                    // Swap the IN/OUT for the rear TROQUEL_REDONDO, keeping the transverse LONGITUD, at the same spot.
                    var redondo = CloneAt(instance, redondoId, redondoBlock);
                    redondo.DynamicParameters[SelectiveRackDefaults.PeralteParam] = system.HighEndBeamPeralteAt(frontIndex, level);
                    result.Add(redondo);

                    if (!string.IsNullOrWhiteSpace(topeBlock) && rearTope.At(frontIndex, level))
                    {
                        var tope = new HeaderBlockInstance
                        {
                            Role = HeaderBlockRole.Tope,
                            PieceId = PushBackRearTopeBuilder.TopePieceId,
                            BlockName = topeBlock,
                            View = View,
                            Insertion = instance.Insertion,
                            ConnectionAnchor = instance.ConnectionAnchor
                        };
                        tope.DynamicParameters[SelectiveSafetyDefaults.SaqueParam] = saque;
                        if (instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var length))
                        {
                            tope.DynamicParameters[SelectiveRackDefaults.LengthParam] = length;
                        }

                        result.Add(tope);
                    }

                    continue;
                }

                result.Add(instance); // keep posts/plates/decorations
            }

            return HeaderInstanceGrouper.Group(result, "PB_FRONTAL_POSTERIOR");
        }

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

        /// <summary>Recover (frontIndex, 0-based level) of a frontal IN/OUT beam from its X (post+troquel column) and Y (entrance elevation).</summary>
        private static (int FrontIndex, int Level) LocateCell(DynamicRackSystem system, DynamicFrontLayout layout, HeaderBlockInstance beam)
        {
            var frontIndex = 0;
            var bestX = double.MaxValue;
            for (var index = 0; index < Math.Min(layout.PostPositions.Count, layout.TroquelPositions.Count); index++)
            {
                var columnX = layout.PostPositions[index] + layout.TroquelPositions[index];
                var distance = Math.Abs(columnX - beam.Insertion.X);
                if (distance < bestX)
                {
                    bestX = distance;
                    frontIndex = index;
                }
            }

            var level = 0;
            var bestY = double.MaxValue;
            for (var index = 0; index < system.LoadBeamLevels.Count; index++)
            {
                var distance = Math.Abs(system.LoadBeamLevels[index].EntranceElevation - beam.Insertion.Y);
                if (distance < bestY)
                {
                    bestY = distance;
                    level = index;
                }
            }

            return (frontIndex, level);
        }
    }
}
