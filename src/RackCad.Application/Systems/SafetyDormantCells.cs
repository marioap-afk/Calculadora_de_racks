using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The rule that keeps a BLANK front's safety configuration DORMANT across a safety dialog (I-33/PB-014).
    /// <para>
    /// A safety grid renders a front en blanco as a column with NO cells, so the user cannot see, select or apply
    /// anything there. But the grid reports only the cells it actually shows: without this merge, accepting the dialog
    /// would silently DROP every off-cell stored on that column and the user would lose the configuration they had —
    /// exactly what "dormant" must prevent. Reactivating the front has to bring the cells back intact.
    /// </para>
    /// <para>
    /// Only a column with ZERO levels is treated as dormant. A merely SHORTER column (a jagged grid, where a front has
    /// fewer levels than the tallest) keeps its historical behaviour: cells past its own level count are dropped, as
    /// they always were. That is what makes this a no-op for the Selectivo, which never has a zero-level column.
    /// </para>
    /// </summary>
    public static class SafetyDormantCells
    {
        /// <summary>True when <paramref name="column"/> exists in the grid but carries no level at all — a front en
        /// blanco. A column outside the supplied counts is NOT dormant: it never had cells to preserve.</summary>
        public static bool IsDormantColumn(IReadOnlyList<int> levelsPerColumn, int column)
            => levelsPerColumn != null
               && column >= 0
               && column < levelsPerColumn.Count
               && levelsPerColumn[column] <= 0;

        /// <summary>
        /// The off-cells a safety grid must persist: the ones it currently shows as OFF (<paramref name="live"/>) plus
        /// the ones <paramref name="stored"/> holds on dormant columns, which the grid never rendered and therefore
        /// could not report. Live cells win; a stored dormant cell is added once, and the result keeps the live order
        /// followed by the dormant ones so an unchanged dialog round trip is stable.
        /// </summary>
        public static IReadOnlyList<SelectiveGridCell> Merge(
            IEnumerable<SelectiveGridCell> live,
            IEnumerable<SelectiveGridCell> stored,
            IReadOnlyList<int> levelsPerColumn)
        {
            var result = (live ?? Enumerable.Empty<SelectiveGridCell>())
                .Where(cell => cell != null)
                .ToList();
            var seen = new HashSet<(int Frente, int Level)>(result.Select(cell => (cell.Frente, cell.Level)));

            foreach (var cell in stored ?? Enumerable.Empty<SelectiveGridCell>())
            {
                if (cell == null || !IsDormantColumn(levelsPerColumn, cell.Frente))
                {
                    continue;
                }

                if (seen.Add((cell.Frente, cell.Level)))
                {
                    result.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
                }
            }

            return result;
        }
    }
}
