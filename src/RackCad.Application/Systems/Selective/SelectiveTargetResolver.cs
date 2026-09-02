using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The pure rule of I-43: <c>fondos objetivo x alcance interno -&gt; celdas del Selectivo</c>.
    /// <para>
    /// The two axes are INDEPENDENT and that independence is the whole contract. The set of target fondos decides
    /// WHERE the operation lands; the <see cref="SelectiveApplyScope"/> decides WHICH frente/nivel coordinates it
    /// covers inside each of them. The anchor's own fondo is never compared against anything: it only lends its
    /// frente and nivel coordinates, so an operation anchored in fondo 0 and aimed at fondos 1 and 3 writes in 1 and
    /// 3 and nowhere else. There is no scope value that means "el fondo actual" and no fondo that is implicitly
    /// included — a fondo is reached because the caller named it.
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
            IEnumerable<SelectiveCellAddress> selected = null)
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
                    SelectiveTargetPlan.NoTargets, omittedFondos, SelectiveTargetPlan.NoOmittedCells);
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
                targets, omittedFondos, SelectiveTargetPlan.NoOmittedCells);
        }

        /// <summary>
        /// <c>Selected</c> is the only scope whose addresses are given rather than derived, and it obeys the SAME
        /// product rule as the other four: a named cell is a target only if its fondo was targeted and the cell
        /// exists. Exempting it would make the fondo axis mean one thing for four scopes and another for the fifth,
        /// which is precisely the divergence the future multi-selection must not inherit. Each rejection is reported
        /// with its reason; the survivors come out sorted, because the given set has no order of its own.
        /// </summary>
        private static SelectiveTargetPlan ResolveSelected(
            SelectiveTopology topology,
            IReadOnlyList<int> targetFondos,
            SelectiveCellAddress anchor,
            IEnumerable<SelectiveCellAddress> selected,
            IReadOnlyList<int> omittedFondos)
        {
            var targets = new List<SelectiveCellAddress>();
            var omittedCells = new List<SelectiveTargetOmission>();
            var seen = new HashSet<SelectiveCellAddress>();

            foreach (var address in selected ?? Enumerable.Empty<SelectiveCellAddress>())
            {
                if (!seen.Add(address))
                {
                    continue;
                }

                if (!topology.HasFondo(address.FondoIndex))
                {
                    omittedCells.Add(new SelectiveTargetOmission(address, SelectiveTargetOmissionReason.FondoOutOfRange));
                }
                else if (!targetFondos.Contains(address.FondoIndex))
                {
                    omittedCells.Add(new SelectiveTargetOmission(address, SelectiveTargetOmissionReason.FondoNotTargeted));
                }
                else if (!topology.HasCell(address))
                {
                    omittedCells.Add(new SelectiveTargetOmission(address, SelectiveTargetOmissionReason.CellOutOfRange));
                }
                else
                {
                    targets.Add(address);
                }
            }

            targets.Sort();
            omittedCells.Sort((left, right) => left.Address.CompareTo(right.Address));
            return new SelectiveTargetPlan(
                SelectiveApplyScope.Selected, anchor, anchorMissing: false,
                targets, omittedFondos, omittedCells);
        }
    }
}
