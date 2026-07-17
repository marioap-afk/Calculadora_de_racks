using System;
using System.Collections.Generic;

namespace RackCad.Application.Systems
{
    public enum DynamicRackCellScope
    {
        Cell,
        Selected,
        Level,
        Front,
        All
    }

    public readonly struct DynamicRackCellAddress
    {
        public DynamicRackCellAddress(int frontIndex, int levelIndex)
        {
            FrontIndex = frontIndex;
            LevelIndex = levelIndex;
        }

        public int FrontIndex { get; }
        public int LevelIndex { get; }
    }

    /// <summary>
    /// Pure matrix-scope rule shared by the dynamic editor. Front-wide structural fields are deliberately absent:
    /// this helper can only return front x level cells, preventing a cell scope from propagating front ownership.
    /// </summary>
    public static class DynamicRackCellScopeResolver
    {
        public static IReadOnlyList<DynamicRackCellAddress> Targets(
            IReadOnlyList<int> levelCounts,
            int sourceFrontIndex,
            int sourceLevelIndex,
            DynamicRackCellScope scope,
            IEnumerable<DynamicRackCellAddress> selected = null)
        {
            var result = new List<DynamicRackCellAddress>();
            if (levelCounts == null || sourceFrontIndex < 0 || sourceFrontIndex >= levelCounts.Count)
            {
                return result;
            }

            var sourceLevels = Math.Max(1, levelCounts[sourceFrontIndex]);
            sourceLevelIndex = Math.Max(0, Math.Min(sourceLevelIndex, sourceLevels - 1));
            if (scope == DynamicRackCellScope.Selected)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var address in selected ?? Array.Empty<DynamicRackCellAddress>())
                {
                    if (address.FrontIndex < 0 || address.FrontIndex >= levelCounts.Count
                        || address.LevelIndex < 0
                        || address.LevelIndex >= Math.Max(1, levelCounts[address.FrontIndex]))
                    {
                        continue;
                    }

                    var key = address.FrontIndex + "|" + address.LevelIndex;
                    if (seen.Add(key))
                    {
                        result.Add(address);
                    }
                }

                return result;
            }

            for (var frontIndex = 0; frontIndex < levelCounts.Count; frontIndex++)
            {
                var levels = Math.Max(1, levelCounts[frontIndex]);
                for (var levelIndex = 0; levelIndex < levels; levelIndex++)
                {
                    var included = scope == DynamicRackCellScope.All
                                   || scope == DynamicRackCellScope.Cell
                                   && frontIndex == sourceFrontIndex && levelIndex == sourceLevelIndex
                                   || scope == DynamicRackCellScope.Level && levelIndex == sourceLevelIndex
                                   || scope == DynamicRackCellScope.Front && frontIndex == sourceFrontIndex;
                    if (included)
                    {
                        result.Add(new DynamicRackCellAddress(frontIndex, levelIndex));
                    }
                }
            }

            return result;
        }
    }
}
