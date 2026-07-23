using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure editor state of a Push Back system. It OWNS two authorities and nothing else:
    /// <list type="bullet">
    /// <item><see cref="Structure"/> — the shared <see cref="DynamicFrontMatrix"/>: fronts, levels, the primary cell and
    /// the multi-cell selection, per-cell pallet/beam values, fondos, DepthStartPosition, heights, IN/OUT and intermediate
    /// beams and per-front length overrides. Push Back reuses the dynamic structure verbatim, so this is the SAME matrix the
    /// dynamic editor drives — never a renamed copy.</item>
    /// <item>a parallel per-front Push Back configuration (<see cref="PushFronts"/>) — authority ONLY for the high-end (rear)
    /// beam PERALTE per front x level and whether the rear pallet-stop tope is active per front x level.</item>
    /// </list>
    /// After every structural mutation the parallel configuration is re-synced to <see cref="Structure"/> so the two never
    /// drift: growing fronts clones the selected front's config, growing levels clones the last cell, shrinking drops only
    /// the cells a reduction left behind, and the surviving intersection is conserved. The selection is the matrix's alone;
    /// there is no parallel selection. No WPF, AutoCAD, geometry or catalog lives here — the assembler resolves the model.
    /// </summary>
    public sealed partial class PushBackEditorState
    {
        private readonly DynamicFrontMatrix structure;
        private readonly List<PushBackEditorFront> pushFronts = new List<PushBackEditorFront>();

        /// <summary>A brand-new Push Back design: one dynamic-default front, rear peralte 3.5 and an active tope on every
        /// cell, with a valid primary selection on the first cell (the dynamic matrix's own default).</summary>
        public PushBackEditorState()
        {
            structure = new DynamicFrontMatrix();
            SyncPushConfig();
        }

        /// <summary>The shared transverse structure (fronts/levels/selection/cells/fondos/...). The authority Push Back reuses.</summary>
        public DynamicFrontMatrix Structure => structure;

        /// <summary>The parallel per-front Push Back configuration, aligned by index with <see cref="DynamicFrontMatrix.Fronts"/>.</summary>
        public IReadOnlyList<PushBackEditorFront> PushFronts => pushFronts;

        /// <summary>The rear pallet-stop stick-out (SAQUE) applied to every rear tope (a single rack-wide Push Back scalar).</summary>
        public double RearTopeSaque { get; set; } = PushBackDefaults.RearTopeSaque;

        private PushBackSystem workingBaseline;

        /// <summary>The last accepted resolved system whose MODULAR structure — custom cabeceras and manual fondos — the next
        /// recompute preserves. Null for a brand-new design, which rebuilds from a standard structure. Set on load and by the
        /// assembler's AcceptComputation; it is always a fresh resolve, so the editor never mutates the source design/system
        /// through it.</summary>
        public PushBackSystem WorkingBaseline => workingBaseline;

        /// <summary>Replace the working baseline (used by load and by the assembler's AcceptComputation). A null clears it so
        /// the next recompute rebuilds from a standard structure.</summary>
        public void SetWorkingBaseline(PushBackSystem baseline) => workingBaseline = baseline;

        /// <summary>The Push Back cell at (<paramref name="frontIndex"/>, <paramref name="levelIndex"/>), or a default when
        /// out of range — never throws and never returns a shared/orphan cell the caller could mutate into the state.</summary>
        public PushBackEditorCell Cell(int frontIndex, int levelIndex)
        {
            if (frontIndex < 0 || frontIndex >= pushFronts.Count)
            {
                return PushBackEditorCell.Default();
            }

            var cells = pushFronts[frontIndex].Cells;
            return levelIndex >= 0 && levelIndex < cells.Count ? cells[levelIndex] : PushBackEditorCell.Default();
        }

        // ---- Coordinating mutations: delegate structure to the matrix, then re-sync the parallel config ----------

        /// <summary>Grow/shrink the front count on the matrix (cloning the selected front), then re-sync the parallel config.</summary>
        public void SetFrontCount(int requested)
        {
            structure.SetFrontCount(requested);
            SyncPushConfig();
        }

        /// <summary>Change a front's pallet-position count on the matrix (levels unchanged; re-sync is defensive).</summary>
        public void AdjustPositions(int index, int delta)
        {
            structure.AdjustPositions(index, delta);
            SyncPushConfig();
        }

        /// <summary>Change a front's load-level count on the matrix, then re-sync the parallel config's per-front cells.</summary>
        public void AdjustLevels(int index, int delta)
        {
            structure.AdjustLevels(index, delta);
            SyncPushConfig();
        }

        /// <summary>Set or (extend) toggle the matrix selection at a cell. The selection is the matrix's alone.</summary>
        public void ToggleCell(int frontIndex, int levelIndex, bool extendSelection)
            => structure.ToggleCell(frontIndex, levelIndex, extendSelection);

        /// <summary>Prune out-of-range matrix selections and re-seat the primary cell.</summary>
        public void NormalizeSelection() => structure.NormalizeSelection();

        /// <summary>Normalize every existing cell's rear peralte against the catalog-allowed high-end values (Push Back's
        /// canonical rule). After this, each cell holds a value the resolver will accept unchanged, so the state, the
        /// assembled design and the resolved system agree. A null/empty list resolves every cell to the explicit 3.5 default.</summary>
        public void NormalizePeraltes(IReadOnlyList<double> allowed)
        {
            foreach (var front in pushFronts)
            {
                foreach (var cell in front.Cells)
                {
                    cell.NormalizePeralte(allowed);
                }
            }
        }

        /// <summary>Apply the buffer to the primary cell: the shared values via the matrix, the Push Back values to the
        /// parallel primary cell. The matrix may grow the front's levels, so re-sync first.</summary>
        public void CommitEditorValues(PushBackEditorValues values)
        {
            structure.CommitEditorValues(values.Dynamic);
            SyncPushConfig();
            var frontIndex = structure.SelectedFrontIndex;
            var levelIndex = structure.SelectedLevelIndex;
            if (frontIndex >= 0 && frontIndex < pushFronts.Count)
            {
                var front = pushFronts[frontIndex];
                if (levelIndex >= 0 && levelIndex < front.Cells.Count)
                {
                    front.Cells[levelIndex].Apply(values);
                }
            }
        }

        /// <summary>Apply the buffer across a scope: the shared values via <see cref="DynamicFrontMatrix.ApplyScope"/>, and
        /// the Push Back values (rear peralte + tope) to the SAME cell addresses resolved by the SAME
        /// <see cref="DynamicRackCellScopeResolver"/> — never a second, independently-built target list. Returns the count
        /// of cells the shared apply wrote.</summary>
        public int ApplyScope(PushBackEditorValues values, DynamicRackCellScope scope)
        {
            var written = structure.ApplyScope(values.Dynamic, scope);
            SyncPushConfig();
            var targets = DynamicRackCellScopeResolver.Targets(
                structure.LevelCounts(),
                structure.SelectedFrontIndex,
                structure.SelectedLevelIndex,
                scope,
                structure.SelectedCells());
            foreach (var target in targets)
            {
                if (target.FrontIndex < 0 || target.FrontIndex >= pushFronts.Count)
                {
                    continue;
                }

                var front = pushFronts[target.FrontIndex];
                if (target.LevelIndex >= 0 && target.LevelIndex < front.Cells.Count)
                {
                    front.Cells[target.LevelIndex].Apply(values);
                }
            }

            return written;
        }

        // ---- Snapshot / rollback --------------------------------------------------------------------------------

        /// <summary>Deep-snapshot both authorities plus the FULL selection for rollback: the matrix fronts, the parallel Push
        /// Back configuration (both independent copies, no shared cell) and the primary cell + every multi-selection address.</summary>
        public PushBackEditorSnapshot Snapshot()
            => new PushBackEditorSnapshot(
                structure.Snapshot(),
                pushFronts.Select(front => front.Clone()).ToList(),
                RearTopeSaque,
                structure.SelectedFrontIndex,
                structure.SelectedLevelIndex,
                structure.SelectedCells());

        /// <summary>Restore both authorities from a snapshot (taking fresh clones so the snapshot stays reusable), re-sync
        /// defensively, and rebuild the exact selection (primary + multi-selection) through the matrix's own toggles.</summary>
        public void Restore(PushBackEditorSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            structure.Restore(snapshot.Structure.Select(front => front.Clone()).ToList());
            pushFronts.Clear();
            pushFronts.AddRange(snapshot.PushFronts.Select(front => front.Clone()));
            RearTopeSaque = snapshot.RearTopeSaque;
            SyncPushConfig();
            RestoreSelection(snapshot.SelectedFrontIndex, snapshot.SelectedLevelIndex, snapshot.SelectedCells);
        }

        /// <summary>
        /// Rebuild an exact selection through <see cref="DynamicFrontMatrix.ToggleCell"/> alone (the matrix is never modified):
        /// out-of-range addresses are discarded, the non-primary cells are toggled first and the primary LAST so it becomes the
        /// matrix's primary (a toggle re-seats the primary on the last cell touched), then the selection is normalized.
        /// </summary>
        private void RestoreSelection(int primaryFront, int primaryLevel, IReadOnlyList<DynamicRackCellAddress> cells)
        {
            bool InRange(int front, int level)
                => front >= 0 && front < structure.Count && level >= 0 && level < Math.Max(1, structure.Fronts[front].LoadLevels);

            var primaryInRange = InRange(primaryFront, primaryLevel);
            var others = (cells ?? new List<DynamicRackCellAddress>())
                .Where(address => InRange(address.FrontIndex, address.LevelIndex)
                                  && !(address.FrontIndex == primaryFront && address.LevelIndex == primaryLevel))
                .ToList();

            if (others.Count > 0)
            {
                structure.ToggleCell(others[0].FrontIndex, others[0].LevelIndex, false);
                for (var index = 1; index < others.Count; index++)
                {
                    structure.ToggleCell(others[index].FrontIndex, others[index].LevelIndex, true);
                }

                if (primaryInRange)
                {
                    structure.ToggleCell(primaryFront, primaryLevel, true); // primary last -> it becomes the primary
                }
            }
            else if (primaryInRange)
            {
                structure.ToggleCell(primaryFront, primaryLevel, false);
            }

            structure.NormalizeSelection();
        }

        // ---- Shape sync -----------------------------------------------------------------------------------------

        /// <summary>
        /// Re-align the parallel Push Back configuration to <see cref="Structure"/> after any structural mutation. Fronts and
        /// levels are matched by index: a new front clones the selected front's config (the SAME template the matrix clones),
        /// a new level clones the front's last cell, and a removed front/level drops only the trailing entries it left behind.
        /// The surviving intersection keeps its edited peralte/tope; no cell is ever orphaned or shared.
        /// </summary>
        private void SyncPushConfig()
        {
            var frontCount = structure.Count;
            if (frontCount == 0)
            {
                pushFronts.Clear();
                return;
            }

            // Grow/shrink the FRONT count, cloning the selected front's config as the template (the matrix's own rule).
            if (pushFronts.Count > frontCount)
            {
                pushFronts.RemoveRange(frontCount, pushFronts.Count - frontCount);
            }
            else if (pushFronts.Count < frontCount)
            {
                var template = pushFronts.Count > 0
                    ? pushFronts[Math.Max(0, Math.Min(structure.SelectedFrontIndex, pushFronts.Count - 1))]
                    : null;
                while (pushFronts.Count < frontCount)
                {
                    pushFronts.Add(template?.Clone() ?? new PushBackEditorFront());
                }
            }

            // Align each front's LEVEL count to the matrix front (grow clones the last cell, shrink drops trailing cells).
            for (var index = 0; index < frontCount; index++)
            {
                var levels = Math.Max(1, structure.Fronts[index].LoadLevels);
                pushFronts[index].EnsureCellCount(levels);
                pushFronts[index].TrimToLevelCount(levels);
            }
        }
    }

    /// <summary>An immutable deep snapshot of a <see cref="PushBackEditorState"/> for rollback: the matrix fronts, the
    /// parallel Push Back configuration (both independent copies) and the full selection (primary cell + every address).</summary>
    public sealed class PushBackEditorSnapshot
    {
        public PushBackEditorSnapshot(
            IReadOnlyList<DynamicEditorFront> structure,
            IReadOnlyList<PushBackEditorFront> pushFronts,
            double rearTopeSaque,
            int selectedFrontIndex,
            int selectedLevelIndex,
            IReadOnlyList<DynamicRackCellAddress> selectedCells)
        {
            Structure = structure ?? new List<DynamicEditorFront>();
            PushFronts = pushFronts ?? new List<PushBackEditorFront>();
            RearTopeSaque = rearTopeSaque;
            SelectedFrontIndex = selectedFrontIndex;
            SelectedLevelIndex = selectedLevelIndex;
            SelectedCells = selectedCells ?? new List<DynamicRackCellAddress>();
        }

        public IReadOnlyList<DynamicEditorFront> Structure { get; }
        public IReadOnlyList<PushBackEditorFront> PushFronts { get; }
        public double RearTopeSaque { get; }
        public int SelectedFrontIndex { get; }
        public int SelectedLevelIndex { get; }
        public IReadOnlyList<DynamicRackCellAddress> SelectedCells { get; }
    }
}
