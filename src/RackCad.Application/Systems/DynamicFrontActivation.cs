using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The single authority over a front's <c>Activo</c>/<c>En blanco</c> state (I-33/PB-014), shared by the dynamic
    /// system and by Push Back — which composes the very same <see cref="DynamicRackDesign"/> as its structure, so the
    /// rule is stated once and neither system can drift from the other.
    /// <para>
    /// A BLANK front keeps its claro and its structure and still displaces the fronts behind it, but carries ZERO
    /// effective load levels: no IN/OUT beam, no intermediate beam, no bed, no rear beam, no rear tope and no
    /// level-indexed safety. Its own configuration is left DORMANT (levels, cells, peraltes, pallet count), so
    /// reactivating the front restores exactly what it had — a blank front is never modelled as a fake cell.
    /// </para>
    /// <para>
    /// Drawing and BOM must size load work with <see cref="EffectiveLoadLevels(DynamicRackFront)"/> instead of reading
    /// <see cref="DynamicRackFront.LoadLevels"/>: for an ACTIVE front it answers the historical
    /// <c>Math.Max(1, LoadLevels)</c> verbatim, so nothing changes for a rack without blank fronts.
    /// </para>
    /// </summary>
    public static class DynamicFrontActivation
    {
        /// <summary>True when the resolved front is blank (structure only, no load).</summary>
        public static bool IsBlank(DynamicRackFront front) => front != null && !front.IsActive;

        /// <summary>True when the editable front intent is blank (structure only, no load).</summary>
        public static bool IsBlank(DynamicRackFrontDesign design) => design != null && !design.IsActive;

        /// <summary>
        /// Load levels this front effectively carries: zero when blank, otherwise the historical
        /// <c>Math.Max(1, LoadLevels)</c>. This is the ONLY sanctioned way to size per-front load work.
        /// </summary>
        public static int EffectiveLoadLevels(DynamicRackFront front)
            => front == null || !front.IsActive ? 0 : Math.Max(1, front.LoadLevels);

        /// <summary>The same rule over the editable intent, for editors and assemblers that resolve nothing yet.</summary>
        public static int EffectiveLoadLevels(DynamicRackFrontDesign design)
            => design == null || !design.IsActive
                ? 0
                : Math.Max(1, design.LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels);

        /// <summary>Effective load levels of every resolved front, in front order (feeds the per-post rules).</summary>
        public static IReadOnlyList<int> EffectiveLevelsPerFront(DynamicRackSystem system)
            => (system?.Fronts ?? (IList<DynamicRackFront>)Array.Empty<DynamicRackFront>())
                .Select(EffectiveLoadLevels)
                .ToList();

        /// <summary>The resolved fronts that actually carry load; blank fronts are dropped.</summary>
        public static IEnumerable<DynamicRackFront> Active(IEnumerable<DynamicRackFront> fronts)
            => (fronts ?? Enumerable.Empty<DynamicRackFront>()).Where(front => front != null && front.IsActive);

        /// <summary>True when at least one front carries load. An all-blank rack is not a valid design.</summary>
        public static bool HasActiveFront(IEnumerable<DynamicRackFrontDesign> designs)
            => (designs ?? Enumerable.Empty<DynamicRackFrontDesign>()).Any(design => design != null && design.IsActive);

        /// <summary>
        /// Enforces "at least one active front" on an editable design set, reactivating the FIRST front when every
        /// front came back blank. Legacy documents never reach this path with an all-blank set — they have no blank
        /// fronts at all — so it only guards an editor or a hand-written document that blanked everything.
        /// </summary>
        public static void EnsureActiveFront(IList<DynamicRackFrontDesign> designs)
        {
            if (designs == null || designs.Count == 0 || HasActiveFront(designs))
            {
                return;
            }

            var first = designs.FirstOrDefault(design => design != null);
            if (first != null)
            {
                first.IsActive = true;
            }
        }

        /// <summary>The same guard over already resolved fronts, for the document boundary that rebuilds a system
        /// directly instead of going through a design.</summary>
        public static void EnsureActiveFront(IList<DynamicRackFront> fronts)
        {
            if (fronts == null || fronts.Count == 0 || fronts.Any(front => front != null && front.IsActive))
            {
                return;
            }

            var first = fronts.FirstOrDefault(front => front != null);
            if (first != null)
            {
                first.IsActive = true;
            }
        }
    }
}
