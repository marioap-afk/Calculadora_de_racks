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

        /// <summary>
        /// The ONE message for an all-blank rack, shared by every rejection point so the user reads the same sentence
        /// whichever boundary caught it.
        /// </summary>
        public const string AllBlankMessage =
            "Al menos un frente debe permanecer activo: un rack con todos los frentes en blanco no lleva carga.";

        /// <summary>
        /// True when the front set is acceptable: at least one front carries load. An EMPTY set is not judged here —
        /// it means "no fronts declared", which the legacy fallbacks resolve elsewhere — so this answers only the
        /// all-blank question.
        /// <para>
        /// This predicate is the SINGLE canonical check (I-33). Nothing normalizes an all-blank payload by silently
        /// reactivating a front: the editor PREVENTS reaching that state non-destructively
        /// (<c>DynamicFrontMatrix.SetActive</c> refuses to blank the last active front), and an explicitly all-blank
        /// payload that arrives anyway is REJECTED with a visible error by the resolver and by
        /// <c>RackDesignValidation</c>. Legacy documents carry no flag at all and therefore load every front active.
        /// </para>
        /// </summary>
        public static bool HasActiveFront(IEnumerable<DynamicRackFrontDesign> designs)
        {
            var list = (designs ?? Enumerable.Empty<DynamicRackFrontDesign>()).Where(design => design != null).ToList();
            return list.Count == 0 || list.Any(design => design.IsActive);
        }

        /// <summary>The same canonical check over already resolved fronts, for the boundaries that rebuild a system
        /// directly instead of going through a design.</summary>
        public static bool HasActiveFront(IEnumerable<DynamicRackFront> fronts)
        {
            var list = (fronts ?? Enumerable.Empty<DynamicRackFront>()).Where(front => front != null).ToList();
            return list.Count == 0 || list.Any(front => front.IsActive);
        }
    }
}
