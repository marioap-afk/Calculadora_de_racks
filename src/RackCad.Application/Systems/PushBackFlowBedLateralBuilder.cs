using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Composes the existing roller-bed assembly into a Push Back lane. It REUSES <see cref="FlowBedLateralBuilder"/>
    /// verbatim (the rail/roller recipe is never duplicated) with two Push-Back-specific choices:
    /// <list type="bullet">
    /// <item><see cref="FlowBedType.Pushback"/> — rollers + the bed's own tope, NO brakes (frenos).</item>
    /// <item>the commercial bed length is the front's COMPLETE structural span (no 4" clearance) — unlike the dynamic
    /// bed, which subtracts <see cref="DynamicRackDefaults.FlowBedLengthClearance"/>.</item>
    /// </list>
    /// The dynamic system is NOT touched; the divergent rule (full-span length) lives here, in one place.
    /// </summary>
    public sealed class PushBackFlowBedLateralBuilder
    {
        private readonly FlowBedLateralBuilder flowBedBuilder = new FlowBedLateralBuilder();

        /// <summary>
        /// Push Back commercial bed length = the front's COMPLETE longitudinal span (its resolved cabeceras + separators),
        /// i.e. <c>front.EndX - front.StartX</c> (or the whole system when no front is given). NO 4" clearance is
        /// subtracted — that discount is a dynamic-only rule.
        /// </summary>
        public static double ResolveBedLength(PushBackSystem system, DynamicRackFront front = null)
        {
            if (system == null)
            {
                return 0.0;
            }

            var span = front != null && front.EndX > front.StartX
                ? front.EndX - front.StartX
                : system.TotalLength;

            return span > 0.0 ? span : 0.0;
        }

        /// <summary>
        /// The local roller-bed assembly for one Push Back lane, at the tope's origin (before the lateral builder rotates
        /// it onto the bed axis). Rail LONGITUD = the full structural span; <see cref="FlowBedType.Pushback"/> omits brakes.
        /// </summary>
        public IReadOnlyList<HeaderBlockInstance> BuildLocalAssembly(
            PushBackSystem system,
            RackCatalog catalog,
            DynamicRackFront front = null)
        {
            var laneDepth = ResolveBedLength(system, front);
            if (laneDepth <= 0.0)
            {
                return new List<HeaderBlockInstance>();
            }

            return flowBedBuilder.Build(
                new FlowBedConfiguration
                {
                    BedType = FlowBedType.Pushback,
                    LaneDepth = laneDepth,
                    PalletDepth = system.Structure?.Pallet?.Depth ?? 0.0,
                    RollerId = FlowBedDefaults.RollerId
                },
                catalog);
        }
    }
}
