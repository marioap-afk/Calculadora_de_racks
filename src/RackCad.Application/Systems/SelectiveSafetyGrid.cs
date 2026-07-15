using System;
using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Shared safety-grid utilities. The classic frente × level grid uses resolved BEAM levels: a design's floor-pallet
    /// row is intentionally absent when it has no floor beam. DESVIADOR supplies its own post × load-level counts through
    /// <see cref="SelectiveDesviadorPlan"/>, but reuses the same validated off-cell helpers.
    /// </summary>
    public static class SelectiveSafetyGrid
    {
        public static IReadOnlyList<int> LevelCounts(SelectiveRackSystem system)
        {
            var frenteCount = 0;
            var fondoCount = SelectiveDepthLayout.Count(system);
            for (var k = 0; k < fondoCount; k++)
            {
                frenteCount = Math.Max(frenteCount, SelectiveDepthLayout.BaysOfFondo(system, k)?.Count ?? 0);
            }

            var counts = new int[frenteCount];
            for (var k = 0; k < fondoCount; k++)
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(system, k);
                if (bays == null) continue;
                for (var frente = 0; frente < bays.Count; frente++)
                {
                    counts[frente] = Math.Max(counts[frente], bays[frente]?.Levels?.Count ?? 0);
                }
            }

            return counts;
        }

        /// <summary>
        /// True only when every valid grid cell is disabled. Duplicate, negative and out-of-range legacy references do
        /// not inflate the disabled count; an empty grid remains configurable instead of being silently discarded.
        /// </summary>
        public static bool AllCellsOff(IReadOnlyList<int> levelsPerFrente, IEnumerable<SelectiveGridCell> offCells)
        {
            if (levelsPerFrente == null || levelsPerFrente.Count == 0)
            {
                return false;
            }

            var total = 0;
            foreach (var count in levelsPerFrente)
            {
                total += Math.Max(0, count);
            }

            if (total == 0)
            {
                return false;
            }

            var valid = new HashSet<(int Frente, int Level)>();
            foreach (var cell in OffCellKeys(offCells))
            {
                if (cell.Frente >= levelsPerFrente.Count) continue;
                if (cell.Level >= Math.Max(0, levelsPerFrente[cell.Frente])) continue;
                valid.Add(cell);
            }

            return valid.Count == total;
        }

        /// <summary>
        /// Hash lookup for hot builder/BOM loops. Negative and duplicate legacy entries are discarded once instead of
        /// scanning the mutable DTO-style list for every generated block.
        /// </summary>
        public static HashSet<(int Frente, int Level)> OffCellKeys(IEnumerable<SelectiveGridCell> offCells)
        {
            var result = new HashSet<(int Frente, int Level)>();
            foreach (var cell in offCells ?? Array.Empty<SelectiveGridCell>())
            {
                if (cell != null && cell.Frente >= 0 && cell.Level >= 0)
                {
                    result.Add((cell.Frente, cell.Level));
                }
            }

            return result;
        }
    }
}
