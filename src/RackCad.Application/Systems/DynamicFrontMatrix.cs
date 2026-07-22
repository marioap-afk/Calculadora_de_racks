using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure front x level grid of the dynamic editor: the editable fronts, the primary cell and the multi-cell selection,
    /// plus every structural mutation the window used to perform inline — add/remove fronts, adjust positions/levels,
    /// select/toggle cells, apply an edit buffer to a cell/scope/set of fronts, snapshot for rollback, and refresh/restore
    /// from a resolved system (I-21). The window keeps only WPF: reading fields into a <see cref="DynamicEditorValues"/>,
    /// rendering the grid, and orchestrating recompute. Selection indices are always kept clamped to the current fronts.
    /// </summary>
    public sealed class DynamicFrontMatrix
    {
        private readonly List<DynamicEditorFront> fronts = new List<DynamicEditorFront>();
        private readonly HashSet<(int FrontIndex, int LevelIndex)> selectedCells =
            new HashSet<(int FrontIndex, int LevelIndex)>();
        private int selectedFrontIndex;
        private int selectedLevelIndex;

        /// <summary>Start with the single default front the editor has always opened with, selecting its bottom cell.</summary>
        public DynamicFrontMatrix()
        {
            var front = DynamicEditorFront.CreateDefault(1);
            fronts.Add(front);
            front.EnsureCellCount(front.LoadLevels);
            selectedCells.Add((0, 0));
        }

        public IReadOnlyList<DynamicEditorFront> Fronts => fronts;
        public int Count => fronts.Count;
        public int SelectedFrontIndex => selectedFrontIndex;
        public int SelectedLevelIndex => selectedLevelIndex;
        public int SelectedCellCount => selectedCells.Count;

        public bool IsSelected(int frontIndex, int levelIndex) => selectedCells.Contains((frontIndex, levelIndex));

        /// <summary>The selected addresses, safe to enumerate while callers mutate the grid.</summary>
        public IReadOnlyList<DynamicRackCellAddress> SelectedCells()
            => selectedCells.Select(cell => new DynamicRackCellAddress(cell.FrontIndex, cell.LevelIndex)).ToList();

        /// <summary>Distinct front indices that own a selected cell (was <c>selectedCells.Select(c =&gt; c.FrontIndex)</c>).</summary>
        public IEnumerable<int> SelectedFrontIndices => selectedCells.Select(cell => cell.FrontIndex);

        /// <summary>Raw per-front load-level counts, in front order (fed to <see cref="DynamicRackCellScopeResolver"/>).</summary>
        public IReadOnlyList<int> LevelCounts() => fronts.Select(front => front.LoadLevels).ToList();

        /// <summary>Largest load-level count across fronts (the rack-wide level maximum), or the default when empty.</summary>
        public int MaxLoadLevels()
            => fronts.Count > 0
                ? fronts.Max(front => Math.Max(1, front.LoadLevels))
                : DynamicRackDefaults.DefaultLoadLevels;

        /// <summary>Clamp a level index to the valid range of the given front (matches the window's SelectCell clamp).</summary>
        public int ClampLevel(int frontIndex, int levelIndex)
        {
            if (frontIndex < 0 || frontIndex >= fronts.Count)
            {
                return Math.Max(0, levelIndex);
            }

            return Math.Max(0, Math.Min(levelIndex, Math.Max(1, fronts[frontIndex].LoadLevels) - 1));
        }

        /// <summary>Prune out-of-range selections and re-seat the primary cell (was NormalizeSelectedCells).</summary>
        public void NormalizeSelection()
        {
            if (fronts.Count == 0)
            {
                selectedCells.Clear();
                return;
            }

            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, fronts.Count - 1));
            selectedLevelIndex = Math.Max(0, Math.Min(
                selectedLevelIndex,
                Math.Max(1, fronts[selectedFrontIndex].LoadLevels) - 1));
            selectedCells.RemoveWhere(cell => cell.FrontIndex < 0
                                              || cell.FrontIndex >= fronts.Count
                                              || cell.LevelIndex < 0
                                              || cell.LevelIndex >= Math.Max(1, fronts[cell.FrontIndex].LoadLevels));
            var primary = (FrontIndex: selectedFrontIndex, LevelIndex: selectedLevelIndex);
            if (!selectedCells.Contains(primary))
            {
                if (selectedCells.Count > 0)
                {
                    var next = selectedCells.First();
                    selectedFrontIndex = next.FrontIndex;
                    selectedLevelIndex = next.LevelIndex;
                }
                else
                {
                    selectedCells.Add(primary);
                }
            }
        }

        /// <summary>Grow (cloning the selected front as template) or shrink to <paramref name="requested"/> fronts, renumber
        /// and re-clamp the primary front (was the structural core of ApplyFrontCount). Requests below 1 are ignored.</summary>
        public void SetFrontCount(int requested)
        {
            if (requested < 1)
            {
                return;
            }

            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, fronts.Count - 1));
            var template = fronts.Count > 0 ? fronts[selectedFrontIndex] : null;
            while (fronts.Count < requested)
            {
                var newFront = new DynamicEditorFront
                {
                    Index = fronts.Count + 1,
                    PalletCount = template?.PalletCount ?? DynamicRackDefaults.DefaultPalletsWide,
                    LoadLevels = template?.LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels,
                    PalletsDeep = template?.PalletsDeep ?? DynamicRackDefaults.DefaultPalletsDeep,
                    DepthStartPosition = template?.DepthStartPosition ?? 1,
                    FirstLevelHeight = template?.FirstLevelHeight ?? DynamicRackDefaults.DefaultFirstLevelHeight
                };
                foreach (var cell in template?.Cells ?? Enumerable.Empty<DynamicEditorCell>())
                {
                    newFront.Cells.Add(cell.Clone());
                }
                newFront.EnsureCellCount(newFront.LoadLevels);
                fronts.Add(newFront);
            }

            if (fronts.Count > requested)
            {
                fronts.RemoveRange(requested, fronts.Count - requested);
            }

            for (var index = 0; index < fronts.Count; index++)
            {
                fronts[index].Index = index + 1;
            }

            selectedFrontIndex = Math.Min(selectedFrontIndex, fronts.Count - 1);
        }

        /// <summary>Select the front and change its pallet-position count by <paramref name="delta"/> (min 1).</summary>
        public void AdjustPositions(int index, int delta)
        {
            if (index < 0 || index >= fronts.Count)
            {
                return;
            }

            selectedFrontIndex = index;
            fronts[index].PalletCount = Math.Max(1, fronts[index].PalletCount + delta);
        }

        /// <summary>Select the front and change its load-level count by <paramref name="delta"/> (min 1).</summary>
        public void AdjustLevels(int index, int delta)
        {
            if (index < 0 || index >= fronts.Count)
            {
                return;
            }

            selectedFrontIndex = index;
            fronts[index].LoadLevels = Math.Max(1, fronts[index].LoadLevels + delta);
        }

        /// <summary>Set or (Ctrl) toggle the selection at a cell, keeping the primary cell coherent (was SelectCell's core).
        /// <paramref name="levelIndex"/> is clamped defensively; callers pass the value from <see cref="ClampLevel"/>.</summary>
        public void ToggleCell(int frontIndex, int levelIndex, bool extendSelection)
        {
            if (frontIndex < 0 || frontIndex >= fronts.Count)
            {
                return;
            }

            levelIndex = ClampLevel(frontIndex, levelIndex);
            var key = (FrontIndex: frontIndex, LevelIndex: levelIndex);
            if (extendSelection)
            {
                if (selectedCells.Contains(key) && selectedCells.Count > 1)
                {
                    selectedCells.Remove(key);
                    if (selectedFrontIndex == frontIndex && selectedLevelIndex == levelIndex)
                    {
                        var next = selectedCells.First();
                        selectedFrontIndex = next.FrontIndex;
                        selectedLevelIndex = next.LevelIndex;
                    }
                }
                else
                {
                    selectedCells.Add(key);
                    selectedFrontIndex = frontIndex;
                    selectedLevelIndex = levelIndex;
                }
            }
            else
            {
                selectedCells.Clear();
                selectedCells.Add(key);
                selectedFrontIndex = frontIndex;
                selectedLevelIndex = levelIndex;
            }
        }

        /// <summary>Apply the edit buffer's structural front values and this cell's values to the primary cell, growing the
        /// cell list first (was CommitSelectedFrontEditor's apply step). Selection is re-clamped to the buffer's levels.</summary>
        public void CommitEditorValues(DynamicEditorValues values)
        {
            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, fronts.Count - 1));
            selectedLevelIndex = Math.Max(0, Math.Min(selectedLevelIndex, values.LoadLevels - 1));
            var row = fronts[selectedFrontIndex];
            row.Apply(values);
            row.EnsureCellCount(row.LoadLevels);
            row.Cells[selectedLevelIndex].Apply(values);
        }

        /// <summary>Whether the edit buffer differs from the primary cell (was SelectedEditorDiffers, minus the WPF parse).</summary>
        public bool SelectedEditorDiffers(DynamicEditorValues values)
        {
            if (values == null || fronts.Count == 0)
            {
                return false;
            }

            var index = Math.Max(0, Math.Min(selectedFrontIndex, fronts.Count - 1));
            var row = fronts[index];
            row.EnsureCellCount(row.LoadLevels);
            var levelIndex = Math.Max(0, Math.Min(selectedLevelIndex, row.LoadLevels - 1));
            var cell = row.Cells[levelIndex];
            return row.PalletCount != values.PalletCount
                   || row.LoadLevels != values.LoadLevels
                   || row.PalletsDeep != values.PalletsDeep
                   || row.DepthStartPosition != values.DepthStartPosition
                   || Math.Abs(row.FirstLevelHeight - values.FirstLevelHeight) > 1e-6
                   || Math.Abs(cell.PalletFront - values.PalletFront) > 1e-6
                   || Math.Abs(cell.PalletHeight - values.PalletHeight) > 1e-6
                   || Math.Abs(cell.PalletWeight - values.PalletWeight) > 1e-6
                   || Math.Abs(cell.ClearHeight - values.ClearHeight) > 1e-6
                   || !string.Equals(cell.InOutBeamCatalogId, values.InOutBeamCatalogId, StringComparison.OrdinalIgnoreCase)
                   || Math.Abs(cell.InOutBeamDepth - values.InOutBeamDepth) > 1e-6
                   || !NullableDoubleEquals(cell.BeamLengthOverride, values.BeamLengthOverride)
                   || !string.Equals(cell.IntermediateBeamCatalogId, values.IntermediateBeamCatalogId, StringComparison.OrdinalIgnoreCase)
                   || Math.Abs(cell.IntermediateBeamDepth - values.IntermediateBeamDepth) > 1e-6;
        }

        /// <summary>Apply the buffer's front values to the primary front, then its cell values to every cell in the resolved
        /// scope (was ApplyEditorScope's model step). Returns the number of cells written; selection follows the source.</summary>
        public int ApplyScope(DynamicEditorValues values, DynamicRackCellScope scope)
        {
            var sourceIndex = Math.Max(0, Math.Min(selectedFrontIndex, fronts.Count - 1));
            var levelIndex = Math.Max(0, Math.Min(selectedLevelIndex, values.LoadLevels - 1));
            fronts[sourceIndex].Apply(values);
            fronts[sourceIndex].EnsureCellCount(fronts[sourceIndex].LoadLevels);

            var targets = DynamicRackCellScopeResolver.Targets(
                LevelCounts(),
                sourceIndex,
                levelIndex,
                scope,
                SelectedCells());
            foreach (var target in targets)
            {
                var row = fronts[target.FrontIndex];
                row.EnsureCellCount(row.LoadLevels);
                row.Cells[target.LevelIndex].Apply(values);
            }

            selectedFrontIndex = sourceIndex;
            selectedLevelIndex = levelIndex;
            return targets.Count;
        }

        /// <summary>Valid, distinct target front indices out of a requested set (was ApplyFrontDataTo's filter).</summary>
        public IReadOnlyList<int> NormalizeFrontTargets(IEnumerable<int> requestedTargets)
            => (requestedTargets ?? Enumerable.Empty<int>())
                .Where(index => index >= 0 && index < fronts.Count)
                .Distinct()
                .ToList();

        /// <summary>Apply the buffer's structural front values to each target front, then normalize the selection (was the
        /// model step of ApplyFrontDataTo). Callers filter targets with <see cref="NormalizeFrontTargets"/> first.</summary>
        public void ApplyFrontValuesTo(DynamicEditorValues values, IEnumerable<int> targets)
        {
            foreach (var target in targets ?? Enumerable.Empty<int>())
            {
                if (target < 0 || target >= fronts.Count)
                {
                    continue;
                }

                fronts[target].Apply(values);
                fronts[target].EnsureCellCount(fronts[target].LoadLevels);
            }

            NormalizeSelection();
        }

        /// <summary>Deep-clone the fronts for rollback (was <c>frontRows.Select(CloneFrontRow).ToList()</c>).</summary>
        public IReadOnlyList<DynamicEditorFront> Snapshot() => fronts.Select(front => front.Clone()).ToList();

        /// <summary>Replace the fronts with a snapshot taken by <see cref="Snapshot"/> (rollback after a failed recompute).</summary>
        public void Restore(IReadOnlyList<DynamicEditorFront> snapshot)
        {
            fronts.Clear();
            if (snapshot != null)
            {
                fronts.AddRange(snapshot);
            }
        }

        /// <summary>Copy resolved per-front values back into the editable rows when the front count matches (was
        /// RefreshFrontRows). Returns false without touching anything when the resolved count differs.</summary>
        public bool RefreshFromResolved(IList<DynamicRackFront> resolved)
        {
            if (resolved == null || resolved.Count != fronts.Count)
            {
                return false;
            }

            for (var index = 0; index < resolved.Count; index++)
            {
                fronts[index].Index = index + 1;
                fronts[index].Bfr = resolved[index].Bfr;
                fronts[index].BeamLength = resolved[index].BeamLength;
                fronts[index].LoadLevels = Math.Max(1, resolved[index].LoadLevels);
                fronts[index].PalletsDeep = Math.Max(2, resolved[index].PalletsDeep);
                fronts[index].DepthStartPosition = Math.Max(1, resolved[index].DepthStartPosition);
                fronts[index].FirstLevelHeight = resolved[index].FirstLevelHeight;
                fronts[index].Cells.Clear();
                foreach (var level in resolved[index].Levels)
                {
                    fronts[index].Cells.Add(DynamicEditorCell.From(level));
                }
                fronts[index].EnsureCellCount(fronts[index].LoadLevels);
            }

            return true;
        }

        /// <summary>Rebuild every editable front from a resolved system, keeping at least one default front and a valid
        /// primary selection (was RestoreFrontRows). Used when opening/loading a persisted design.</summary>
        public void RestoreFromResolved(IEnumerable<DynamicRackFront> resolved)
        {
            fronts.Clear();
            foreach (var front in resolved ?? Enumerable.Empty<DynamicRackFront>())
            {
                var row = new DynamicEditorFront
                {
                    Index = fronts.Count + 1,
                    PalletCount = Math.Max(1, front.PalletCount),
                    LoadLevels = Math.Max(1, front.LoadLevels),
                    PalletsDeep = Math.Max(2, front.PalletsDeep),
                    DepthStartPosition = Math.Max(1, front.DepthStartPosition),
                    FirstLevelHeight = front.FirstLevelHeight,
                    Bfr = front.Bfr,
                    BeamLength = front.BeamLength
                };
                foreach (var level in front.Levels)
                {
                    row.Cells.Add(DynamicEditorCell.From(level));
                }
                row.EnsureCellCount(row.LoadLevels);
                fronts.Add(row);
            }

            if (fronts.Count == 0)
            {
                var fallback = DynamicEditorFront.CreateDefault(1);
                fallback.EnsureCellCount(fallback.LoadLevels);
                fronts.Add(fallback);
            }

            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, fronts.Count - 1));
        }

        /// <summary>Build the transverse design list from the editable rows (was the fronts loop inside Recompose).</summary>
        public IReadOnlyList<DynamicRackFrontDesign> BuildFrontDesigns()
        {
            var designs = new List<DynamicRackFrontDesign>();
            foreach (var row in fronts)
            {
                row.EnsureCellCount(row.LoadLevels);
                var frontDesign = new DynamicRackFrontDesign
                {
                    PalletCount = row.PalletCount,
                    LoadLevels = row.LoadLevels,
                    PalletsDeep = row.PalletsDeep,
                    DepthStartPosition = row.DepthStartPosition,
                    FirstLevelHeight = row.FirstLevelHeight
                };
                foreach (var cell in row.Cells.Take(row.LoadLevels))
                {
                    frontDesign.Levels.Add(cell.ToDesign());
                    frontDesign.IntermediateBeamDepths.Add(cell.IntermediateBeamDepth);
                }
                designs.Add(frontDesign);
            }

            return designs;
        }

        private static bool NullableDoubleEquals(double? left, double? right)
        {
            if (!left.HasValue || !right.HasValue)
            {
                return left.HasValue == right.HasValue;
            }

            return Math.Abs(left.Value - right.Value) < 1e-6;
        }
    }
}
