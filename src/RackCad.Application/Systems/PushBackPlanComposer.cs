using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure helpers to transform a BLACK-BOX dynamic <see cref="DynamicSystemPlan"/> into a Push Back plan: they identify
    /// the dynamic-specific pieces to remove by <see cref="HeaderBlockInstance.Role"/> and <see cref="HeaderBlockInstance.PieceId"/>
    /// (never by generated group name), while preserving the common structure (cabeceras, separators, derived posts,
    /// plates, annotations, dimensions). The dynamic plan itself is never mutated — callers build NEW lists.
    /// </summary>
    public static class PushBackPlanComposer
    {
        private static readonly System.StringComparison Ic = System.StringComparison.OrdinalIgnoreCase;

        /// <summary>The dynamic complete IN/OUT beam (both dynamic ends use it) — removed; Push Back re-adds its own low IN/OUT.</summary>
        public static bool IsDynamicEndBeam(HeaderBlockInstance instance)
            => instance != null && string.Equals(instance.PieceId, DynamicRackDefaults.InOutBeamCatalogId, Ic);

        /// <summary>The dynamic intermediate beam — removed; Push Back re-adds intermediates tangent to its own axis.</summary>
        public static bool IsDynamicIntermediate(HeaderBlockInstance instance)
            => instance != null && string.Equals(instance.PieceId, DynamicRackDefaults.IntermediateBeamCatalogId, Ic);

        /// <summary>Any roller-bed piece (rail/roller/brake/stop) — removed; Push Back re-adds its own bed.</summary>
        public static bool IsBedPiece(HeaderBlockInstance instance)
            => instance != null && (instance.Role == HeaderBlockRole.Rail
                || instance.Role == HeaderBlockRole.Roller
                || instance.Role == HeaderBlockRole.Brake
                || instance.Role == HeaderBlockRole.Stop);

        /// <summary>Any dynamic safety/tope — removed; Push Back re-adds only authorized safety + rear topes.</summary>
        public static bool IsSafetyPiece(HeaderBlockInstance instance)
            => instance != null && (instance.Role == HeaderBlockRole.Safety || instance.Role == HeaderBlockRole.Tope);

        public static bool IsDynamicSpecific(HeaderBlockInstance instance)
            => IsBedPiece(instance) || IsSafetyPiece(instance) || IsDynamicEndBeam(instance) || IsDynamicIntermediate(instance);

        /// <summary>A loose instance to KEEP: common structure (separators, derived posts, plates) + annotations/dimensions.</summary>
        public static bool KeepLoose(HeaderBlockInstance instance)
            => instance != null && !IsDynamicSpecific(instance);

        /// <summary>A <see cref="HeaderGroup"/> to KEEP: a structural cabecera group — none of its instances is dynamic-specific.</summary>
        public static bool KeepHeaderGroup(HeaderGroup group)
            => group?.Instances != null && !group.Instances.Any(IsDynamicSpecific);

        /// <summary>The structural cabecera groups of <paramref name="plan"/> (bed and intermediate groups removed).</summary>
        public static List<HeaderGroup> StructuralHeaderGroups(DynamicSystemPlan plan)
            => (plan?.Headers ?? new List<HeaderGroup>()).Where(KeepHeaderGroup).ToList();

        /// <summary>The structural loose instances of <paramref name="plan"/> (dynamic ends/bed/safety/intermediates removed).</summary>
        public static List<HeaderBlockInstance> StructuralLoose(DynamicSystemPlan plan)
            => (plan?.LooseInstances ?? new List<HeaderBlockInstance>()).Where(KeepLoose).ToList();
    }
}
