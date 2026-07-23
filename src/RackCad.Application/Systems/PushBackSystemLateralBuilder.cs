using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Lateral Push Back plan by BLACK-BOX composition of the dynamic lateral plan. It invokes
    /// <see cref="DynamicSystemLateralBuilder"/> (whole system and per-post section modes), keeps the common structure
    /// (cabeceras, separators, derived posts, plates, annotations/dimensions), removes the dynamic-specific pieces by
    /// Role/PieceId (both dynamic end beams, the roller bed, all dynamic safety/tope, the dynamic intermediate beams),
    /// and adds the Push Back pieces: the low IN/OUT beam, the high TROQUEL_REDONDO beam per cell, intermediates tangent
    /// to the Push Back axis, the pushback bed, and the rear topes. The dynamic plan is never mutated.
    /// </summary>
    public sealed class PushBackSystemLateralBuilder
    {
        private readonly DynamicSystemLateralBuilder dynamicBuilder = new DynamicSystemLateralBuilder();
        private readonly PushBackFlowBedLateralBuilder bedBuilder = new PushBackFlowBedLateralBuilder();
        private readonly PushBackIntermediateBeamLateralBuilder intermediateBuilder = new PushBackIntermediateBeamLateralBuilder();
        private readonly PushBackRearTopeBuilder rearTopeBuilder = new PushBackRearTopeBuilder();

        public DynamicSystemPlan Build(PushBackSystem system, RackCatalog catalog) => BuildCore(system, catalog, -1);

        public DynamicSystemPlan Build(PushBackSystem system, RackCatalog catalog, int postIndex) => BuildCore(system, catalog, postIndex);

        /// <summary>The lateral section at each transverse post, following the dynamic Cortes contract.</summary>
        public IReadOnlyList<DynamicLateralCorte> Cortes(PushBackSystem system, RackCatalog catalog)
        {
            var result = new List<DynamicLateralCorte>();
            var structure = system?.Structure;
            if (structure == null)
            {
                return result;
            }

            var layout = DynamicFrontGeometry.Compute(structure, catalog);
            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                result.Add(new DynamicLateralCorte(postIndex, layout.PostPositions[postIndex], Build(system, catalog, postIndex)));
            }

            return result;
        }

        private DynamicSystemPlan BuildCore(PushBackSystem system, RackCatalog catalog, int postIndex)
        {
            var structure = system?.Structure;
            if (structure == null)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var sectioned = postIndex >= 0;
            var basePlan = sectioned ? dynamicBuilder.Build(structure, catalog, postIndex) : dynamicBuilder.Build(structure, catalog);

            // Keep the common structure; drop every dynamic-specific piece by Role/PieceId.
            var headers = PushBackPlanComposer.StructuralHeaderGroups(basePlan);
            var loose = PushBackPlanComposer.StructuralLoose(basePlan);

            var levelCount = sectioned
                ? DynamicFrontGeometry.LoadLevelsAtPost(structure, postIndex)
                : structure.LoadBeamLevels.Count;
            IReadOnlyList<DynamicRackFront> fronts = sectioned
                ? DynamicFrontGeometry.AdjacentFronts(structure, postIndex)
                    .GroupBy(front => string.Join("|", front.StartX, front.EndX, front.LoadLevels))
                    .Select(group => group.First())
                    .ToList()
                : new List<DynamicRackFront> { null };

            foreach (var front in fronts)
            {
                var frontIndex = front?.Index ?? 0;
                loose.AddRange(PushBackLoadBeamGeometry.LowBeams(system, catalog, front));
                loose.AddRange(PushBackLoadBeamGeometry.HighBeams(system, catalog, frontIndex, front));
                loose.AddRange(rearTopeBuilder.BuildLateral(system, catalog, frontIndex, front));

                var bedLevels = sectioned ? Math.Min(levelCount, front.LoadLevels) : levelCount;
                var bed = bedBuilder.BuildLateral(system, catalog, front, bedLevels);
                if (bed != null)
                {
                    headers.Add(bed);
                }
            }

            var intermediates = intermediateBuilder.Build(system, catalog, postIndex, levelCount);
            headers.AddRange(intermediates.Headers);
            loose.AddRange(intermediates.LooseInstances);

            return new DynamicSystemPlan(headers, loose);
        }
    }
}
