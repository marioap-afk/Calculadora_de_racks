using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Planta (top view) Push Back plan by BLACK-BOX composition of <see cref="DynamicSystemPlantaBuilder"/>. It keeps the
    /// common structure (cabeceras, separators, derived posts, plates, intermediate beams, GUIA-free safety and
    /// decorations), keeps the LOW IN/OUT beam, and swaps every HIGH (rear) IN/OUT beam for a TROQUEL_REDONDO plus a rear
    /// tope when the front has an active rear tope. Levels collapse onto one plan line, so the rear tope is one per front.
    /// Instances are identified by Role/PieceId/mirror, never by group name.
    /// </summary>
    public sealed class PushBackSystemPlantaBuilder
    {
        private const string View = "PLANTA";
        private readonly DynamicSystemPlantaBuilder dynamicBuilder = new DynamicSystemPlantaBuilder();

        public DynamicSystemPlan BuildPlan(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            if (structure == null)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var instances = dynamicBuilder.BuildPlan(structure, catalog).Flatten().Instances;
            var redondoId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var redondoBlock = CatalogLookup.Block(catalog, redondoId, View);
            var topeBlock = CatalogLookup.Block(catalog, PushBackRearTopeBuilder.TopePieceId, View);
            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var saque = rearTope.Saque > 0.0 ? rearTope.Saque : PushBackDefaults.RearTopeSaque;

            var result = new List<HeaderBlockInstance>();
            foreach (var instance in instances)
            {
                if (!PushBackPlanComposer.IsDynamicEndBeam(instance))
                {
                    result.Add(instance); // keep structure, intermediates, safety (GUIA-free) and decorations
                    continue;
                }

                if (!instance.MirroredX)
                {
                    result.Add(instance); // the LOW (entrance/exit) IN/OUT beam stays
                    continue;
                }

                // The HIGH (rear) IN/OUT beam becomes a TROQUEL_REDONDO with the ENVELOPING rear peralte (planta collapses
                // the levels); add a rear tope if that front has any active cell.
                var frontIndex = NearestHighFront(structure, instance.Insertion.X);
                var redondo = CloneAt(instance, redondoId, redondoBlock);
                redondo.DynamicParameters[SelectiveRackDefaults.PeralteParam] = PushBackHighEndBeamGeometry.PlantaPeralte(system, frontIndex);
                result.Add(redondo);

                var front = frontIndex >= 0 && frontIndex < structure.Fronts.Count ? structure.Fronts[frontIndex] : null;
                var anyActive = front != null && Enumerable.Range(0, Math.Max(1, front.LoadLevels)).Any(level => rearTope.At(frontIndex, level));
                if (!string.IsNullOrWhiteSpace(topeBlock) && anyActive)
                {
                    // Planta draws top-down and keeps the frente Y (no rise-and-snap); LONGITUD + SAQUE via the canonical helper.
                    double? longitud = instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var beamLength)
                        ? beamLength + SelectiveTopePlacement.LengthAllowance
                        : (double?)null;
                    result.Add(SelectiveTopePlacement.Tope(
                        PushBackRearTopeBuilder.TopePieceId, topeBlock, View,
                        instance.Insertion.X, instance.Insertion.Y, saque, longitud,
                        mirroredX: PushBackRearTopeBuilder.Mirrored(View, instance.MirroredX)));
                }
            }

            return HeaderInstanceGrouper.Group(result, "PB_PLANTA_PIEZA");
        }

        public IReadOnlyList<HeaderBlockInstance> Build(PushBackSystem system, RackCatalog catalog)
            => BuildPlan(system, catalog).Flatten().Instances;

        private static HeaderBlockInstance CloneAt(HeaderBlockInstance source, string pieceId, string block)
        {
            var clone = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = pieceId,
                BlockName = block,
                View = View,
                MirroredX = source.MirroredX,
                MirroredY = source.MirroredY,
                RotationRadians = source.RotationRadians,
                Insertion = source.Insertion,
                ConnectionAnchor = source.ConnectionAnchor
            };
            if (source.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var length))
            {
                clone.DynamicParameters[SelectiveRackDefaults.LengthParam] = length;
            }

            return clone;
        }

        /// <summary>The front whose EndX is nearest to a high (rear) planta beam's X (it sits at front.EndX − troquel).</summary>
        private static int NearestHighFront(DynamicRackSystem system, double x)
        {
            var best = 0;
            var bestDistance = double.MaxValue;
            for (var index = 0; index < system.Fronts.Count; index++)
            {
                var distance = Math.Abs(system.Fronts[index].EndX - x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = index;
                }
            }

            return best;
        }
    }
}
