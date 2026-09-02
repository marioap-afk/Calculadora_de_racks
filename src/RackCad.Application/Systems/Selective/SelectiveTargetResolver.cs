using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The pure rule of I-43: <c>fondos objetivo x alcance interno -&gt; celdas del Selectivo</c>.
    /// <para>
    /// The two axes are INDEPENDENT and that independence is the whole contract. The set of target fondos decides
    /// WHERE the operation lands; the <see cref="SelectiveApplyScope"/> decides WHICH frente/nivel coordinates it
    /// covers inside each of them. Neither the anchor nor the selection carries a fondo of its own into the answer:
    /// both are read as coordinates of the VISIBLE matrix and projected onto every target fondo, so an operation set
    /// up while looking at fondo 0 and aimed at fondos 1 and 3 writes in 1 and 3 and nowhere else. There is no scope
    /// value that means "el fondo actual" and no fondo that is implicitly included — a fondo is reached because the
    /// caller named it.
    /// </para>
    /// <para>
    /// Four properties hold for every call, and are what the tests pin:
    /// <list type="number">
    /// <item>PURE — no state is read or written; the same inputs always give the same plan.</item>
    /// <item>DETERMINISTIC — targets come out in canonical order (fondo, frente, nivel, ascending) and distinct,
    /// including for <c>Selected</c>, whose input is an unordered set.</item>
    /// <item>OMIT, NEVER CLAMP — an index that does not exist is left out. It is never moved to the nearest valid
    /// one and never causes a cell to be created. This is the rule <c>SelectiveEditorState.ApplyScope</c> already
    /// applies to ragged columns, extended to the fondo axis; it is also where the Dinamico resolver could NOT be
    /// reused (it clamps the source level and treats a zero-level front as having one).</item>
    /// <item>COMPLETE BEFORE MUTATING — the answer is a whole <see cref="SelectiveTargetPlan"/>, so a caller writes
    /// only after knowing every address, and then recomputes ONCE per fondo of the plan.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class SelectiveTargetResolver
    {
        /// <summary>
        /// Resolve one operation. <paramref name="selected"/> is read only by <see cref="SelectiveApplyScope.Selected"/>
        /// and is where the future multi-selection plugs in; every other scope ignores it. Null arguments are treated
        /// as "nothing", never as an error: an operation that reaches no cell is a legitimate outcome that the plan
        /// reports instead of throwing.
        /// </summary>
        public static SelectiveTargetPlan Resolve(
            SelectiveTopology topology,
            SelectiveFondoTargets fondos,
            SelectiveApplyScope scope,
            SelectiveCellAddress anchor,
            IEnumerable<SelectiveMatrixPosition> selected = null)
        {
            topology = topology ?? SelectiveTopology.Empty;
            fondos = fondos ?? SelectiveFondoTargets.None;

            // The target fondos split into the ones this rack has and the ones it does not. An absent fondo is
            // omitted and REPORTED, so a stale target set can never quietly shrink an operation.
            var targetFondos = new List<int>();
            var omittedFondos = new List<int>();
            foreach (var fondo in fondos.Fondos)
            {
                (topology.HasFondo(fondo) ? targetFondos : omittedFondos).Add(fondo);
            }

            if (scope == SelectiveApplyScope.Selected)
            {
                return ResolveSelected(topology, targetFondos, anchor, selected, omittedFondos);
            }

            // Cell/Row/Column read coordinates from the anchor, so the anchor must be a cell that EXISTS in its own
            // fondo. When it does not, the operation is refused whole: re-aiming it at a neighbour would apply the
            // values to a cell nobody selected.
            var needsAnchor = scope == SelectiveApplyScope.Cell
                              || scope == SelectiveApplyScope.Row
                              || scope == SelectiveApplyScope.Column;
            if (needsAnchor && !topology.HasCell(anchor))
            {
                return new SelectiveTargetPlan(
                    scope, anchor, anchorMissing: true,
                    SelectiveTargetPlan.NoCells, omittedFondos, SelectiveTargetPlan.NoCells);
            }

            // Walking fondo -> frente -> nivel ascending is what makes the result canonical without a sort, and the
            // bounds come from the topology, so a shorter fondo or a shorter frente simply contributes fewer cells.
            var targets = new List<SelectiveCellAddress>();
            foreach (var fondo in targetFondos)
            {
                var frontCount = topology.FrontCount(fondo);
                for (var front = 0; front < frontCount; front++)
                {
                    var levelCount = topology.LevelCount(fondo, front);
                    for (var level = 0; level < levelCount; level++)
                    {
                        var included =
                            scope == SelectiveApplyScope.All ||
                            (scope == SelectiveApplyScope.Cell && front == anchor.FrontIndex && level == anchor.LevelIndex) ||
                            (scope == SelectiveApplyScope.Row && level == anchor.LevelIndex) ||
                            (scope == SelectiveApplyScope.Column && front == anchor.FrontIndex);

                        if (included)
                        {
                            targets.Add(new SelectiveCellAddress(fondo, front, level));
                        }
                    }
                }
            }

            return new SelectiveTargetPlan(
                scope, anchor, anchorMissing: false,
                targets, omittedFondos, SelectiveTargetPlan.NoCells);
        }

        /// <summary>
        /// <c>Selected</c> is the only scope whose coordinates are given rather than derived, and it obeys the SAME
        /// product rule as the other four: the caller picks positions of the VISIBLE matrix and this projects each of
        /// them onto EVERY target fondo. A selection has no fondo of its own to honour — it is a
        /// <see cref="SelectiveMatrixPosition"/>, not a cell — so nothing here compares a selection against
        /// <paramref name="targetFondos"/> and nothing accumulates a selection per fondo. That is the same shape
        /// Dinamico/Push Back already give their multi-selection.
        /// <para>
        /// A position that does not exist in one target fondo omits ONLY that instance: the same position still
        /// applies in every other fondo where it does exist, and the missing one is reported. Sorting the
        /// deduplicated positions once and then walking the (already ascending) fondos outermost yields the canonical
        /// order without a final sort — the given set has no order of its own, so this is what makes the answer
        /// reproducible.
        /// </para>
        /// </summary>
        private static SelectiveTargetPlan ResolveSelected(
            SelectiveTopology topology,
            IReadOnlyList<int> targetFondos,
            SelectiveCellAddress anchor,
            IEnumerable<SelectiveMatrixPosition> selected,
            IReadOnlyList<int> omittedFondos)
        {
            var positions = (selected ?? Enumerable.Empty<SelectiveMatrixPosition>())
                .Distinct()
                .OrderBy(position => position)
                .ToList();

            var targets = new List<SelectiveCellAddress>();
            var omittedCells = new List<SelectiveCellAddress>();
            foreach (var fondo in targetFondos)
            {
                foreach (var position in positions)
                {
                    var address = position.InFondo(fondo);
                    (topology.HasCell(address) ? targets : omittedCells).Add(address);
                }
            }

            return new SelectiveTargetPlan(
                SelectiveApplyScope.Selected, anchor, anchorMissing: false,
                targets, omittedFondos, omittedCells);
        }
    }
}
