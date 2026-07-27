using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.Dynamic
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

        /// <summary>
        /// The same rule over an EDITOR row, so the editors and the safety dialogs they open size their grids from the
        /// authority instead of restating <c>Math.Max(1, LoadLevels)</c> — the very predicate this class replaces.
        /// </summary>
        public static int EffectiveLoadLevels(DynamicEditorFront front)
            => front == null || !front.IsActive ? 0 : Math.Max(1, front.LoadLevels);

        /// <summary>Effective load levels of every resolved front, in front order (feeds the per-post rules).</summary>
        public static IReadOnlyList<int> EffectiveLevelsPerFront(DynamicRackSystem system)
            => (system?.Fronts ?? (IList<DynamicRackFront>)Array.Empty<DynamicRackFront>())
                .Select(EffectiveLoadLevels)
                .ToList();

        /// <summary>
        /// The level count of EVERY post from a set of EFFECTIVE per-front counts, by the same "tallest adjacent front
        /// owns the cut" rule the drawing uses (<see cref="DynamicFrontGeometry.LoadLevelsAtPost(IReadOnlyList{int},int)"/>
        /// — the rule is reused, not restated). Unlike <see cref="DynamicFrontGeometry.LoadLevelsPerPost"/> this does
        /// NOT floor the answer to one: a post whose only neighbours are blank fronts carries ZERO levels, which is what
        /// lets a safety grid render its column as absent (I-33).
        /// </summary>
        public static IReadOnlyList<int> EffectiveLevelsPerPost(IReadOnlyList<int> effectiveLevelsPerFront)
        {
            var fronts = effectiveLevelsPerFront ?? Array.Empty<int>();
            var result = new List<int>(fronts.Count + 1);
            for (var post = 0; post <= fronts.Count; post++)
            {
                result.Add(DynamicFrontGeometry.LoadLevelsAtPost(fronts, post));
            }

            return result;
        }

        /// <summary>The resolved fronts that actually carry load; blank fronts are dropped.</summary>
        public static IEnumerable<DynamicRackFront> Active(IEnumerable<DynamicRackFront> fronts)
            => (fronts ?? Enumerable.Empty<DynamicRackFront>()).Where(front => front != null && front.IsActive);

        /// <summary>
        /// Whether the physical BOUNDARY at <paramref name="postIndex"/> exists — the post line, and with it its plate,
        /// its share of the cabecera/separator assembly, its derived posts and reinforcements, its section cut and its
        /// per-post safety (Owner, I-33).
        /// <para>
        /// A rack of N fronts has N+1 boundaries. The two EXTERIOR ones (0 and N) always exist. An INTERIOR one exists
        /// unless BOTH of its adjacent fronts are en blanco: there is nothing on either side to hold up, so no post is
        /// built. A run of N blank fronts therefore keeps only its two outer boundaries and loses its N−1 interior
        /// ones; a SINGLE blank front loses none, and alternating blank fronts lose none either.
        /// </para>
        /// <para>
        /// This is about the ASSEMBLY only. The logical fronts are untouched: their indices, claros, dormant
        /// configuration, persistence and the rack's total length stay exactly as they were, so the X coordinates of
        /// every boundary — including the suppressed ones — are still the ones the layout computed.
        /// </para>
        /// </summary>
        public static bool BoundaryExists(IReadOnlyList<bool> frontIsActive, int postIndex)
        {
            var fronts = frontIsActive ?? Array.Empty<bool>();
            if (postIndex < 0 || postIndex > fronts.Count)
            {
                return false;
            }

            if (postIndex == 0 || postIndex == fronts.Count)
            {
                return true;   // the rack's outer edges always exist
            }

            return fronts[postIndex - 1] || fronts[postIndex];
        }

        /// <summary>The same rule over a resolved system, which both the dynamic builders and Push Back consume.</summary>
        public static bool BoundaryExists(DynamicRackSystem system, int postIndex)
            => BoundaryExists(FrontActivation(system), postIndex);

        /// <summary>The Activo/En blanco state of every resolved front, in front order.</summary>
        public static IReadOnlyList<bool> FrontActivation(DynamicRackSystem system)
            => (system?.Fronts ?? (IList<DynamicRackFront>)Array.Empty<DynamicRackFront>())
                .Select(front => front != null && front.IsActive)
                .ToList();

        /// <summary>
        /// The indices of the boundaries that DO exist, in order. Callers that must both skip the suppressed ones and
        /// keep the original post index — every view, the BOM and the safety families do — iterate this.
        /// </summary>
        public static IReadOnlyList<int> PresentBoundaries(DynamicRackSystem system)
        {
            var fronts = FrontActivation(system);
            var result = new List<int>(fronts.Count + 1);
            for (var post = 0; post <= fronts.Count; post++)
            {
                if (BoundaryExists(fronts, post))
                {
                    result.Add(post);
                }
            }

            return result;
        }

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
