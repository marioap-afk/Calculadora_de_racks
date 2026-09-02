using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The pure, testable STATE of the selective advanced editor (initiative I-20): the working matrix (bays × levels),
    /// the saved matrix per fondo (doble profundidad), the selection, and the per-post cabeceras/peraltes — plus the
    /// operations the editor performs on them (init, snapshot/restore of the working fondo, save/load fondo, resize,
    /// add/remove level, apply-by-scope, and <see cref="BuildDesign"/>). Extracted verbatim from
    /// <c>RackSelectiveWindow</c> so this logic runs without WPF and is covered by <c>RackCad.Tests</c>; the window keeps
    /// the painting (matrix + previews), the cell editor, the events and the coalesced recompute (shell I-15). No AutoCAD,
    /// no WPF, no catalog: the resolver/builder stay in the window and consume the <see cref="SelectivePalletDesign"/>
    /// this produces.
    ///
    /// Invariants preserved from the window: <see cref="FloorBeams"/>/<see cref="BayHeights"/>/<see cref="BaySegments"/>
    /// stay parallel to the working <see cref="Bays"/> by bay; each <see cref="SelectiveEditorFondoMatrix"/> is a deep
    /// copy so per-fondo edits stay isolated; fondo 0 defines the master frente grid; the selected fondo's slot is stale
    /// WHILE editing (the live working matrix is that fondo's copy) until <see cref="SaveWorkingToSelected"/> commits it.
    /// </summary>
    public sealed class SelectiveEditorState
    {
        /// <summary>The working matrix: <c>Bays[bay][level]</c>, level 0 = ground; each bay has its own length.</summary>
        public List<List<SelectiveEditorCell>> Bays { get; } = new List<List<SelectiveEditorCell>>();

        /// <summary>Per-bay "larguero a piso" flag, parallel to <see cref="Bays"/>.</summary>
        public List<bool> FloorBeams { get; } = new List<bool>();

        /// <summary>Per-bay manual height override (in); null = auto. Parallel to <see cref="Bays"/>.</summary>
        public List<double?> BayHeights { get; } = new List<double?>();

        /// <summary>Per-bay "medio frente" tramos (N tramos, the last calculated); empty = normal full-width bay. Parallel to <see cref="Bays"/>.</summary>
        public List<List<SelectiveSegment>> BaySegments { get; } = new List<List<SelectiveSegment>>();

        /// <summary>One saved level matrix per fondo. Entry <see cref="SelectedFondo"/> is stale WHILE editing — the live
        /// working matrix (<see cref="Bays"/> etc.) is that fondo's copy until <see cref="SaveWorkingToSelected"/> commits.
        /// Fondo 0 defines the shared frente count.</summary>
        public List<SelectiveEditorFondoMatrix> FondoMatrices { get; } = new List<SelectiveEditorFondoMatrix>();

        /// <summary>The fondo currently being edited (its slot is stale; the working matrix is its copy).</summary>
        public int SelectedFondo { get; set; }

        /// <summary>Optional per-post cabecera (frame); one entry per post (N frentes → N+1 posts), null = run default.</summary>
        public List<RackFrameConfiguration> PostCabeceras { get; } = new List<RackFrameConfiguration>();

        /// <summary>
        /// Per-post custom cabeceras of the fondos AFTER fondo 0 (I-43): entry <c>k-1</c> is fondo <c>k</c>, each a row
        /// by post of THAT fondo. Missing row, short row or null entry all mean "standard". Fondo 0 keeps using
        /// <see cref="PostCabeceras"/>, so the legacy shape and its meaning are untouched.
        /// </summary>
        public List<List<RackFrameConfiguration>> ExtraFondoPostCabeceras { get; } = new List<List<RackFrameConfiguration>>();

        /// <summary>
        /// Per-post PERALTE override; 0 = inherit the global. One entry per post of the MASTER grid.
        /// <para>
        /// This axis is deliberately NOT per fondo (I-43). A post is one physical column shared by every fondo of its
        /// frente, so its peralte is a property of the post, not of a depth line. Only the CABECERA gained the fondo
        /// axis.
        /// </para>
        /// </summary>
        public List<double> PostPeraltes { get; } = new List<double>();

        private int selBay;
        private int selLevel;

        /// <summary>
        /// True when the primary was moved through the LEGACY imperative path (<see cref="SelBay"/>/<see cref="SelLevel"/>
        /// assigned directly) and <see cref="NormalizeSelection"/> has not reconciled it yet.
        /// <para>
        /// This flag is how the two ways of moving the primary coexist. <see cref="SelectCell"/> maintains the
        /// invariant "the primary belongs to the selection" itself, so normalization may treat the selection as the
        /// authority. A direct assignment cannot maintain it — it knows nothing about the set — and it MEANS something
        /// different: "the primary is now this cell", which is a single-cell selection. Without the distinction one of
        /// the two has to lose: either the historical clamp stops holding, or normalization invents a selected
        /// position the user never marked.
        /// </para>
        /// </summary>
        private bool primaryAssignedDirectly;

        /// <summary>Selected bay (frente) index in the working matrix. Assigning it is the LEGACY imperative move of
        /// the primary: the next <see cref="ClampSelection"/> clamps it as it always has, and the selection collapses
        /// onto it.</summary>
        public int SelBay
        {
            get => selBay;
            set
            {
                selBay = value;
                primaryAssignedDirectly = true;
            }
        }

        /// <summary>Selected level index in the working matrix. Same legacy semantics as <see cref="SelBay"/>.</summary>
        public int SelLevel
        {
            get => selLevel;
            set
            {
                selLevel = value;
                primaryAssignedDirectly = true;
            }
        }

        /// <summary>
        /// The multi-selection of the VISIBLE matrix (I-43), as positions — never cells. It is ONE set for the editor,
        /// not one per fondo: switching fondo prunes the positions that no longer exist and keeps the rest, so a
        /// selection is never accumulated per fondo. <see cref="SelBay"/>/<see cref="SelLevel"/> remain the PRIMARY
        /// cell and are always inside this set, which is never empty. Runtime only: nothing here is persisted.
        /// </summary>
        private readonly HashSet<SelectiveMatrixPosition> selectedPositions = new HashSet<SelectiveMatrixPosition> { new SelectiveMatrixPosition(0, 0) };

        /// <summary>The fondos an operation is aimed at (I-43). Default and legacy behaviour: only the fondo being
        /// edited. Runtime only: never persisted.</summary>
        private SelectiveFondoTargets targetFondos = SelectiveFondoTargets.Single(0);

        /// <summary>The beam id a fresh cell adopts (the editor's default larguero); set once by the window at startup.</summary>
        public string DefaultBeamId { get; set; }

        /// <summary>A fresh cell with the default larguero + peralte (matches the editor's <c>NewCell</c>).</summary>
        public SelectiveEditorCell NewCell()
            => new SelectiveEditorCell { BeamId = DefaultBeamId, BeamPeralte = SelectiveRackDefaults.DefaultBeamPeralte };

        /// <summary>Reset the working matrix to <paramref name="bayCount"/> bays × <paramref name="levelCount"/> levels of fresh cells.</summary>
        public void InitMatrix(int bayCount, int levelCount)
        {
            Bays.Clear();
            FloorBeams.Clear();
            BayHeights.Clear();
            BaySegments.Clear();
            for (var b = 0; b < bayCount; b++)
            {
                var column = new List<SelectiveEditorCell>();
                for (var l = 0; l < levelCount; l++) column.Add(NewCell());
                Bays.Add(column);
                FloorBeams.Add(false);
                BayHeights.Add(null);
                BaySegments.Add(new List<SelectiveSegment>());
            }

            // A full reset of the matrix resets the selection too: keeping positions from the matrix that just
            // disappeared would leave stale coordinates that no ClampSelection call is coming to prune (I-43). It
            // writes the fields, not the legacy setters: this path establishes the invariant by itself.
            selBay = 0;
            selLevel = 0;
            primaryAssignedDirectly = false;
            selectedPositions.Clear();
            if (Bays.Count > 0 && Bays[0].Count > 0) selectedPositions.Add(new SelectiveMatrixPosition(0, 0));
        }

        // ---- Per-fondo matrices (doble profundidad: each fondo edits its own levels) ----

        private static List<SelectiveEditorCell> CloneColumn(List<SelectiveEditorCell> column)
            => column.Select(c => c.Clone()).ToList();

        /// <summary>Deep-clone a bay's medio-frente tramos so edits stay isolated per fondo/snapshot.</summary>
        private static List<SelectiveSegment> CloneSegments(IEnumerable<SelectiveSegment> segments)
            => segments?.Select(s => new SelectiveSegment { Length = s.Length, Loaded = s.Loaded }).ToList() ?? new List<SelectiveSegment>();

        /// <summary>Snapshot the live working matrix into a saveable copy, tagging it with the given fondo (depth) and
        /// cabecera override (the window reads those two from its boxes, with its keep-previous fallback).</summary>
        public SelectiveEditorFondoMatrix SnapshotWorking(double depth, double cabeceraOverride)
        {
            var snap = new SelectiveEditorFondoMatrix { Depth = depth, CabeceraOverride = cabeceraOverride };
            foreach (var column in Bays) snap.Bays.Add(CloneColumn(column));
            snap.FloorBeams.AddRange(FloorBeams);
            snap.BayHeights.AddRange(BayHeights);
            foreach (var segments in BaySegments) snap.BaySegments.Add(CloneSegments(segments));
            return snap;
        }

        /// <summary>Load a saved fondo matrix into the live working matrix (deep-cloned so edits stay isolated), clamping
        /// the selection. The window syncs its fondo/cabecera boxes from <paramref name="snap"/> afterwards.</summary>
        public void RestoreWorkingFrom(SelectiveEditorFondoMatrix snap)
        {
            Bays.Clear();
            FloorBeams.Clear();
            BayHeights.Clear();
            BaySegments.Clear();
            foreach (var column in snap.Bays) Bays.Add(CloneColumn(column));
            FloorBeams.AddRange(snap.FloorBeams);
            BayHeights.AddRange(snap.BayHeights);
            foreach (var segments in snap.BaySegments) BaySegments.Add(CloneSegments(segments));
            if (Bays.Count == 0) { Bays.Add(new List<SelectiveEditorCell> { NewCell() }); FloorBeams.Add(false); BayHeights.Add(null); BaySegments.Add(new List<SelectiveSegment>()); }
            while (BaySegments.Count < Bays.Count) BaySegments.Add(new List<SelectiveSegment>()); // defensive: keep parallel to bays (legacy snapshots)
            ClampSelection();
        }

        /// <summary>Commit the live working matrix back into its fondo slot (with the given depth/cabecera) before switching/building/resizing.</summary>
        public void SaveWorkingToSelected(double depth, double cabeceraOverride)
        {
            if (FondoMatrices.Count == 0) { FondoMatrices.Add(SnapshotWorking(depth, cabeceraOverride)); return; }
            if (SelectedFondo >= 0 && SelectedFondo < FondoMatrices.Count) FondoMatrices[SelectedFondo] = SnapshotWorking(depth, cabeceraOverride);
        }

        /// <summary>A copy of <paramref name="source"/> resized to <paramref name="bayCount"/> frentes: a new frente clones
        /// <paramref name="widthSeed"/>'s column at that index (fondo 0 defines the frente count/width), extra bays are
        /// dropped. Keeps every fondo's posts aligned on the shared grid.</summary>
        public SelectiveEditorFondoMatrix CloneAligned(SelectiveEditorFondoMatrix source, int bayCount, SelectiveEditorFondoMatrix widthSeed)
        {
            var m = new SelectiveEditorFondoMatrix { Depth = source.Depth, CabeceraOverride = source.CabeceraOverride };
            for (var b = 0; b < bayCount; b++)
            {
                if (b < source.Bays.Count)
                {
                    m.Bays.Add(CloneColumn(source.Bays[b]));
                    m.FloorBeams.Add(source.FloorBeams[b]);
                    m.BayHeights.Add(source.BayHeights[b]);
                    m.BaySegments.Add(b < source.BaySegments.Count ? CloneSegments(source.BaySegments[b]) : new List<SelectiveSegment>());
                }
                else
                {
                    m.Bays.Add(widthSeed != null && b < widthSeed.Bays.Count ? CloneColumn(widthSeed.Bays[b]) : new List<SelectiveEditorCell> { NewCell() });
                    m.FloorBeams.Add(false);
                    m.BayHeights.Add(null);
                    m.BaySegments.Add(new List<SelectiveSegment>());
                }
            }

            return m;
        }

        /// <summary>Load fondo <paramref name="k"/> into the working matrix. Each fondo keeps its OWN frente count (a
        /// corner layout); the resolver aligns overlapping widths to the longest fondo, so nothing is forced here.</summary>
        public void LoadFondo(int k) => RestoreWorkingFrom(FondoMatrices[k]);

        /// <summary>Turn a fondo matrix into design bays (the shape the resolver consumes).</summary>
        public static List<SelectiveBayDesign> BuildBayDesigns(SelectiveEditorFondoMatrix m)
        {
            var result = new List<SelectiveBayDesign>();
            for (var b = 0; b < m.Bays.Count; b++)
            {
                var bay = new SelectiveBayDesign
                {
                    FloorBeam = m.FloorBeams[b],
                    HeightOverride = m.BayHeights[b]
                };
                if (b < m.BaySegments.Count)
                {
                    foreach (var segment in m.BaySegments[b])
                    {
                        bay.Segments.Add(new SelectiveSegment { Length = segment.Length, Loaded = segment.Loaded });
                    }
                }

                foreach (var cell in m.Bays[b])
                {
                    bay.Levels.Add(new SelectiveCell
                    {
                        Pallet = new Tarima { Frente = cell.Frente, Alto = cell.Alto },
                        PalletCount = cell.PalletCount,
                        BeamId = cell.BeamId,
                        BeamPeralte = cell.BeamPeralte,
                        BeamLengthOverride = cell.BeamLength,
                        ClearOverride = cell.Clear
                    });
                }

                result.Add(bay);
            }

            return result;
        }

        /// <summary>Turn saved design bays into a fondo matrix (for load). <paramref name="paddedEmptyFrentes"/> counts the
        /// empty (zero-level) frentes padded with a default cell so the matrix editor can hold them — the window warns on it.</summary>
        public SelectiveEditorFondoMatrix FondoMatrixFromDesignBays(IList<SelectiveBayDesign> designBays, out int paddedEmptyFrentes)
        {
            paddedEmptyFrentes = 0;
            var m = new SelectiveEditorFondoMatrix();
            foreach (var bayDesign in designBays)
            {
                var column = new List<SelectiveEditorCell>();
                foreach (var cell in bayDesign.Levels)
                {
                    column.Add(new SelectiveEditorCell
                    {
                        Frente = cell.Pallet?.Frente ?? 42.0,
                        Alto = cell.Pallet?.Alto ?? 60.0,
                        PalletCount = cell.PalletCount,
                        BeamId = cell.BeamId ?? DefaultBeamId,
                        BeamPeralte = cell.BeamPeralte,
                        BeamLength = cell.BeamLengthOverride,
                        Clear = cell.ClearOverride
                    });
                }

                if (column.Count == 0)
                {
                    // The matrix editor needs >=1 cell per frente, but a persisted design CAN carry an empty frente
                    // (a building column, honored by resolver/planta/BOM). Pad it so the editor works, and COUNT it so
                    // the load warns instead of silently converting the column into a loaded frente.
                    column.Add(NewCell());
                    paddedEmptyFrentes++;
                }
                m.Bays.Add(column);
                m.FloorBeams.Add(bayDesign.FloorBeam);
                m.BayHeights.Add(bayDesign.HeightOverride);
                m.BaySegments.Add(CloneSegments(bayDesign.Segments));
            }

            if (m.Bays.Count == 0) { m.Bays.Add(new List<SelectiveEditorCell> { NewCell() }); m.FloorBeams.Add(false); m.BayHeights.Add(null); m.BaySegments.Add(new List<SelectiveSegment>()); }
            return m;
        }

        /// <summary>Grow/shrink the number of bays, preserving existing ones; a new bay clones the last (cells + floor flag + height + tramos).</summary>
        public void ResizeBays(int bayCount)
        {
            while (Bays.Count < bayCount)
            {
                if (Bays.Count > 0)
                {
                    Bays.Add(Bays[Bays.Count - 1].Select(c => c.Clone()).ToList());
                    FloorBeams.Add(FloorBeams[FloorBeams.Count - 1]);
                    BayHeights.Add(BayHeights[BayHeights.Count - 1]);
                    BaySegments.Add(CloneSegments(BaySegments[BaySegments.Count - 1]));
                }
                else
                {
                    Bays.Add(new List<SelectiveEditorCell> { NewCell() });
                    FloorBeams.Add(false);
                    BayHeights.Add(null);
                    BaySegments.Add(new List<SelectiveSegment>());
                }
            }

            while (Bays.Count > bayCount)
            {
                Bays.RemoveAt(Bays.Count - 1);
                BaySegments.RemoveAt(BaySegments.Count - 1);
                FloorBeams.RemoveAt(FloorBeams.Count - 1);
                BayHeights.RemoveAt(BayHeights.Count - 1);
            }

            ClampSelection();
        }

        /// <summary>Append a level to bay <paramref name="bay"/> (clones the top level, or a fresh cell when empty).</summary>
        public void AddLevel(int bay)
        {
            var column = Bays[bay];
            column.Add(column.Count > 0 ? column[column.Count - 1].Clone() : NewCell());
        }

        /// <summary>True when bay <paramref name="bay"/> can drop its top level (it has more than one).</summary>
        public bool CanRemoveLevel(int bay) => bay >= 0 && bay < Bays.Count && Bays[bay].Count > 1;

        /// <summary>Drop the top level of bay <paramref name="bay"/> and clamp the selection; false (no change) when it has only one.</summary>
        public bool RemoveLevel(int bay)
        {
            if (!CanRemoveLevel(bay)) return false;
            Bays[bay].RemoveAt(Bays[bay].Count - 1);
            ClampSelection();
            return true;
        }

        /// <summary>Keep the selection inside the working matrix after a structural change — the primary cell AND the
        /// multi-selection, which is pruned and re-seated by <see cref="NormalizeSelection"/>.</summary>
        public void ClampSelection()
        {
            // The fields, not the setters: clamping is the state repairing itself, not a caller moving the primary,
            // so it must not look like the legacy imperative path to NormalizeSelection.
            selBay = Math.Min(Math.Max(0, selBay), Bays.Count - 1);
            var levelCount = selBay >= 0 && selBay < Bays.Count ? Bays[selBay].Count : 1;
            selLevel = Math.Min(Math.Max(0, selLevel), levelCount - 1);
            NormalizeSelection();
        }

        // ---- Multi-selection of the visible matrix (I-43) ----

        /// <summary>How many positions are selected (at least one, once the matrix has a cell).</summary>
        public int SelectedCount => selectedPositions.Count;

        /// <summary>Whether that matrix position is part of the multi-selection.</summary>
        public bool IsSelected(int bay, int level) => selectedPositions.Contains(new SelectiveMatrixPosition(bay, level));

        /// <summary>The selection in canonical order (frente, then nivel), so the same set always resolves the same
        /// plan. Sorting here — rather than handing out the hash set's iteration order, as the dinamico does — is what
        /// makes <see cref="SelectiveApplyScope.Selected"/> deterministic end to end.</summary>
        public IReadOnlyList<SelectiveMatrixPosition> SelectedPositions()
            => selectedPositions.OrderBy(position => position).ToList();

        /// <summary>True when that position exists in the working matrix.</summary>
        private bool PositionExists(SelectiveMatrixPosition position)
            => position.FrontIndex >= 0 && position.FrontIndex < Bays.Count
               && position.LevelIndex >= 0 && position.LevelIndex < Bays[position.FrontIndex].Count;

        /// <summary>
        /// Select a matrix cell: a plain click (<paramref name="extend"/> false) makes it the ONLY selection, and a
        /// Ctrl+click toggles it. Removing is refused when it would empty the selection — an editor with nothing
        /// selected has no cell editor to show — so the set is never empty and the primary always belongs to it.
        /// Out-of-range coordinates change nothing (the same guard <c>DynamicFrontMatrix.ToggleCell</c> applies).
        /// </summary>
        public void SelectCell(int bay, int level, bool extend)
        {
            var position = new SelectiveMatrixPosition(bay, level);
            if (!PositionExists(position)) return;

            // Every branch writes the fields and clears the legacy flag: this path keeps "the primary belongs to the
            // selection" true on its own, so normalization must apply the shared (dinamico) rule to it.
            primaryAssignedDirectly = false;
            if (!extend)
            {
                selectedPositions.Clear();
                selectedPositions.Add(position);
                selBay = bay;
                selLevel = level;
                return;
            }

            if (selectedPositions.Contains(position))
            {
                if (selectedPositions.Count == 1) return; // never leave the selection empty
                selectedPositions.Remove(position);
                if (selBay == bay && selLevel == level) SeatPrimaryOnFirstSelected();
                return;
            }

            selectedPositions.Add(position);
            selBay = bay;
            selLevel = level;
        }

        /// <summary>
        /// Prune the positions the working matrix no longer has and leave a coherent, non-empty selection with the
        /// primary inside it. Called on every structural change (resize, add/remove level, fondo switch) through
        /// <see cref="ClampSelection"/>.
        /// <para>
        /// For a selection built with <see cref="SelectCell"/> this is the rule <c>DynamicFrontMatrix</c> follows:
        /// every SURVIVING position is kept, the primary is kept if it is still one of them, and otherwise the primary
        /// re-seats onto a surviving position. It never ADDS a position: the clamped primary is not a cell the user
        /// marked, and turning it into a selected one would silently widen the next bulk edit. Only when nothing
        /// survives does the clamped primary become the selection, so the set is never empty.
        /// </para>
        /// <para>
        /// A primary moved through the LEGACY imperative path is the exception, and it is not one: assigning
        /// <see cref="SelBay"/>/<see cref="SelLevel"/> directly says "the primary is now this cell", which is a
        /// single-cell selection. That statement wins over the surviving set — otherwise the historical clamp would
        /// stop holding — and the flag is consumed here, so the state returns to the shared rule immediately after.
        /// </para>
        /// </summary>
        public void NormalizeSelection()
        {
            selectedPositions.RemoveWhere(position => !PositionExists(position));
            if (Bays.Count == 0) return; // no cell to select; ClampSelection already parked the primary

            var primary = new SelectiveMatrixPosition(selBay, selLevel);
            if (primaryAssignedDirectly)
            {
                primaryAssignedDirectly = false;
                selectedPositions.Clear();
                if (PositionExists(primary)) selectedPositions.Add(primary);
                return;
            }

            if (selectedPositions.Contains(primary)) return;                 // the primary survived: nothing to do
            if (selectedPositions.Count > 0) { SeatPrimaryOnFirstSelected(); return; } // re-seat, never add
            if (PositionExists(primary)) selectedPositions.Add(primary);     // nothing survived: the clamped primary
        }

        /// <summary>Move the primary onto the FIRST selected position in canonical order — deterministic, unlike
        /// taking whatever the hash set yields first.</summary>
        private void SeatPrimaryOnFirstSelected()
        {
            var first = selectedPositions.OrderBy(position => position).First();
            selBay = first.FrontIndex;
            selLevel = first.LevelIndex;
        }

        // ---- Target fondos (I-43) ----

        /// <summary>The fondos the next operation writes to. Never empty.</summary>
        public SelectiveFondoTargets TargetFondos => targetFondos;

        /// <summary>How many fondos this state has, counting the uncommitted working matrix as fondo 0 when no slot
        /// exists yet — the same rule <see cref="SelectiveTopology.From"/> applies.</summary>
        public int FondoCount => FondoMatrices.Count > 0 ? FondoMatrices.Count : (Bays.Count > 0 ? 1 : 0);

        /// <summary>
        /// Choose the target fondos. Indices this rack does not have are dropped, and a request that leaves nothing
        /// falls back to the fondo being edited: the set is never empty, because an operation with no destination
        /// would silently do nothing.
        /// </summary>
        public void SetTargetFondos(IEnumerable<int> fondos)
        {
            var count = FondoCount;
            var valid = (fondos ?? Enumerable.Empty<int>())
                .Where(index => index >= 0 && index < count)
                .Distinct()
                .ToList();
            targetFondos = valid.Count > 0 ? SelectiveFondoTargets.Of(valid) : CurrentFondoOnly();
        }

        /// <summary>
        /// Switch the fondo being edited, preserving the legacy feel: when the targets were EXACTLY the fondo we are
        /// leaving, they follow to the new one, so an editor nobody has touched keeps behaving as it always did —
        /// edits land on the fondo on screen. A deliberate multi-fondo choice is left alone, because following it
        /// would silently rewrite an intent the user expressed.
        /// </summary>
        public void SelectFondo(int fondoIndex)
        {
            var previous = SelectedFondo;
            SelectedFondo = fondoIndex;
            if (targetFondos.Count == 1 && targetFondos.Fondos[0] == previous) targetFondos = CurrentFondoOnly();
        }

        /// <summary>Drop targets the rack no longer has after a fondo-count change, never leaving the set empty.
        /// Call it whenever the number of fondos changes.</summary>
        public void SyncTargetFondos() => SetTargetFondos(targetFondos.Fondos);

        private SelectiveFondoTargets CurrentFondoOnly()
        {
            var count = FondoCount;
            if (count <= 0) return SelectiveFondoTargets.Single(0);
            return SelectiveFondoTargets.Single(Math.Min(Math.Max(0, SelectedFondo), count - 1));
        }

        /// <summary>The currently selected cell, or false when the selection is out of range.</summary>
        public bool TryGetSelected(out SelectiveEditorCell cell)
        {
            cell = null;
            if (SelBay < 0 || SelBay >= Bays.Count) return false;
            var column = Bays[SelBay];
            if (SelLevel < 0 || SelLevel >= column.Count) return false;
            cell = column[SelLevel];
            return true;
        }

        /// <summary>Copy <paramref name="values"/> into every cell in scope of the current selection, returning the
        /// touched (bay, level) coordinates (bay-outer, level-inner order) so the window can refresh just those cells.
        /// <para>
        /// This is the SINGLE-FONDO legacy path: it walks the live working matrix, so it can only reach the fondo
        /// being edited. It rejects <see cref="SelectiveApplyScope.Selected"/> out loud rather than matching no cell
        /// and returning an empty list, because a bulk edit that quietly applies to nothing is the worst outcome of
        /// the three. The multi-selection contract lives in <see cref="SelectiveTargetResolver"/> (I-43), which
        /// resolves across fondos; no editor produces <c>Selected</c> yet.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The scope is <see cref="SelectiveApplyScope.Selected"/>.</exception>
        public IReadOnlyList<(int Bay, int Level)> ApplyScope(SelectiveApplyScope scope, SelectiveEditorCell values)
        {
            if (scope == SelectiveApplyScope.Selected)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(scope),
                    scope,
                    "El alcance 'Selected' no lo resuelve el estado de un solo fondo; usa SelectiveTargetResolver.");
            }

            var touched = new List<(int Bay, int Level)>();
            for (var b = 0; b < Bays.Count; b++)
            {
                for (var l = 0; l < Bays[b].Count; l++)
                {
                    var inScope =
                        scope == SelectiveApplyScope.All ||
                        (scope == SelectiveApplyScope.Cell && b == SelBay && l == SelLevel) ||
                        (scope == SelectiveApplyScope.Row && l == SelLevel) ||
                        (scope == SelectiveApplyScope.Column && b == SelBay);

                    if (inScope)
                    {
                        Bays[b][l].CopyFrom(values);
                        touched.Add((b, l));
                    }
                }
            }

            return touched;
        }

        // ---- The multi-fondo write authority (I-43) ----

        /// <summary>
        /// The cell at a three-axis address, or null when it does not exist. It is the ONE place that knows a fondo's
        /// cells live in two different containers: the fondo being edited is the LIVE working matrix (its slot is
        /// stale), every other fondo is its slot in <see cref="FondoMatrices"/>. Getting this backwards would write
        /// the active fondo's edits into a copy that the next <c>SaveWorkingToSelected</c> overwrites.
        /// </summary>
        public SelectiveEditorCell CellAt(SelectiveCellAddress address)
        {
            var columns = ColumnsOf(address.FondoIndex);
            if (columns == null || address.FrontIndex < 0 || address.FrontIndex >= columns.Count) return null;
            var column = columns[address.FrontIndex];
            return address.LevelIndex >= 0 && address.LevelIndex < column.Count ? column[address.LevelIndex] : null;
        }

        private List<List<SelectiveEditorCell>> ColumnsOf(int fondoIndex)
        {
            if (FondoMatrices.Count == 0) return fondoIndex == 0 ? Bays : null;
            if (fondoIndex < 0 || fondoIndex >= FondoMatrices.Count) return null;
            return fondoIndex == SelectedFondo ? Bays : FondoMatrices[fondoIndex].Bays;
        }

        /// <summary>
        /// Apply <paramref name="values"/> to <c>fondos objetivo x alcance</c> — the whole multi-fondo operation, in
        /// Application, so no window ever loops over fondos.
        /// <para>
        /// The order is the contract: snapshot the topology, resolve the COMPLETE plan, and only then write. Nothing
        /// mutates while targets are still being decided, so the plan can be inspected or refused first, and the
        /// caller recomputes ONCE for the whole plan however many fondos it touched. Cells are reached through
        /// <see cref="CellAt"/>, which routes the active fondo to the live matrix and the rest to their slots.
        /// </para>
        /// <para>
        /// Only the seven value fields of <see cref="SelectiveEditorCell"/> are written (<c>CopyFrom</c>). The matrix
        /// SHAPE and everything that hangs off a bay — floor beam, manual height, tramos — is untouched, so a scope
        /// can never restructure the rack. Writing the same values twice is therefore idempotent.
        /// </para>
        /// </summary>
        public SelectiveTargetPlan ApplyToTargets(SelectiveApplyScope scope, SelectiveEditorCell values)
        {
            var plan = ResolveTargets(scope);
            if (values != null)
            {
                foreach (var target in plan.Targets)
                {
                    CellAt(target)?.CopyFrom(values);
                }
            }

            NormalizeSelection();
            return plan;
        }

        /// <summary>Resolve what an operation WOULD touch, without touching it: the same snapshot-then-resolve the
        /// write path uses, exposed so a caller can preview or report a plan first.</summary>
        public SelectiveTargetPlan ResolveTargets(SelectiveApplyScope scope)
            => SelectiveTargetResolver.Resolve(
                SelectiveTopology.From(this),
                targetFondos,
                scope,
                new SelectiveCellAddress(SelectedFondo, SelBay, SelLevel),
                SelectedPositions());

        /// <summary>The largest frente count across all fondos (the master grid). Uses the LIVE working matrix for the
        /// selected fondo (its slot is stale mid-edit) and the saved slots for the rest.</summary>
        public int MaxFrenteCount()
        {
            var max = Bays.Count;
            for (var k = 0; k < FondoMatrices.Count; k++)
            {
                if (k == SelectedFondo) continue; // the working copy is live in Bays; the slot is stale
                if (FondoMatrices[k].Bays.Count > max) max = FondoMatrices[k].Bays.Count;
            }

            return max;
        }

        /// <summary>
        /// Keep the per-post cabecera + peralte lists sized to the MASTER grid's posts (masterFrentes+1), preserving
        /// existing entries, and keep every fondo's cabecera row consistent with the posts that fondo actually has.
        /// Sizing to the LONGEST fondo (not the working one) means switching to a shorter fondo never truncates and
        /// loses fondo 0's custom cabeceras / per-post peraltes.
        /// <para>
        /// The pruning is DESTRUCTIVE and there is no resurrection (I-43): an override at a post its fondo no longer
        /// reaches is dropped, and growing back creates a standard (null) slot instead of restoring what used to be
        /// there. A configuration that survived invisibly would come back on a rack the user had already reshaped.
        /// </para>
        /// </summary>
        public void SyncPostCabeceras()
        {
            var posts = MaxFrenteCount() + 1;
            while (PostCabeceras.Count < posts) PostCabeceras.Add(null);
            while (PostCabeceras.Count > posts) PostCabeceras.RemoveAt(PostCabeceras.Count - 1);
            while (PostPeraltes.Count < posts) PostPeraltes.Add(0.0);
            while (PostPeraltes.Count > posts) PostPeraltes.RemoveAt(PostPeraltes.Count - 1);

            var topology = SelectiveTopology.From(this);
            var extras = Math.Max(0, topology.FondoCount - 1);
            while (ExtraFondoPostCabeceras.Count < extras) ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration>());
            while (ExtraFondoPostCabeceras.Count > extras) ExtraFondoPostCabeceras.RemoveAt(ExtraFondoPostCabeceras.Count - 1);

            // Fondo 0's row keeps the master length (nothing else may shrink it), but any entry beyond the posts fondo 0
            // really has is cleared: it could never be drawn, and leaving it would resurrect it on a regrow.
            for (var i = topology.FrontCount(0) + 1; i < PostCabeceras.Count; i++) PostCabeceras[i] = null;

            for (var k = 1; k < topology.FondoCount; k++)
            {
                var row = ExtraFondoPostCabeceras[k - 1];
                var own = topology.FrontCount(k) + 1;
                while (row.Count > own) row.RemoveAt(row.Count - 1);
            }
        }

        // ---- Custom cabeceras by (fondo, post) - I-43 ----

        /// <summary>The custom cabecera stored at <c>(fondoIndex, postIndex)</c>, or null for the standard one.</summary>
        public RackFrameConfiguration CabeceraAt(int fondoIndex, int postIndex)
        {
            var row = CabeceraRow(fondoIndex);
            return row != null && postIndex >= 0 && postIndex < row.Count ? row[postIndex] : null;
        }

        /// <summary>Whether that post exists in that fondo: a fondo with C frentes has posts 0..C.</summary>
        public bool PostExistsIn(int fondoIndex, int postIndex)
        {
            var topology = SelectiveTopology.From(this);
            return topology.HasFondo(fondoIndex) && postIndex >= 0 && postIndex <= topology.FrontCount(fondoIndex);
        }

        private List<RackFrameConfiguration> CabeceraRow(int fondoIndex)
        {
            if (fondoIndex == 0) return PostCabeceras;
            if (fondoIndex < 0 || fondoIndex - 1 >= ExtraFondoPostCabeceras.Count) return null;
            return ExtraFondoPostCabeceras[fondoIndex - 1];
        }

        /// <summary>
        /// Apply one cabecera configuration to <paramref name="postIndex"/> of every TARGET fondo, using the same
        /// <see cref="TargetFondos"/> the cell editor uses so the editor teaches ONE grammar of fondos (I-43).
        /// <para>
        /// Each target receives an INDEPENDENT deep copy, never the same instance: editing one cabecera afterwards must
        /// not silently edit the others, which is exactly what sharing a reference would do. A target where that post
        /// does not exist is OMITTED and reported, never padded and never clamped onto a neighbouring post.
        /// </para>
        /// <para>
        /// Passing null is the RESET: that post returns to the standard cabecera in the targeted fondos. It does not
        /// touch <see cref="PostPeraltes"/>, which is a GLOBAL per-post authority: a reset aimed at some fondos must not
        /// clear an override that belongs to the whole rack.
        /// </para>
        /// </summary>
        public SelectiveCabeceraApplyResult ApplyCabeceraToTargets(
            int postIndex, RackFrameConfiguration configuration, Func<RackFrameConfiguration, RackFrameConfiguration> deepCopy)
        {
            SyncPostCabeceras();
            var applied = new List<int>();
            var omitted = new List<int>();
            foreach (var fondo in targetFondos.Fondos)
            {
                if (!PostExistsIn(fondo, postIndex))
                {
                    omitted.Add(fondo);
                    continue;
                }

                var row = EnsureCabeceraRow(fondo, postIndex);
                if (row == null)
                {
                    omitted.Add(fondo);
                    continue;
                }

                row[postIndex] = configuration == null
                    ? null
                    : (deepCopy != null ? deepCopy(configuration) : configuration);
                applied.Add(fondo);
            }

            return new SelectiveCabeceraApplyResult(postIndex, applied, omitted);
        }

        /// <summary>The row of <paramref name="fondoIndex"/>, grown with nulls so <paramref name="postIndex"/> fits.
        /// Padding INSIDE a fondo that really has that post is not invention: the row is a sparse list by post.</summary>
        private List<RackFrameConfiguration> EnsureCabeceraRow(int fondoIndex, int postIndex)
        {
            if (fondoIndex < 0) return null;
            if (fondoIndex > 0)
            {
                while (ExtraFondoPostCabeceras.Count < fondoIndex) ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration>());
            }

            var row = CabeceraRow(fondoIndex);
            if (row == null) return null;
            while (row.Count <= postIndex) row.Add(null);
            return row;
        }

        /// <summary>
        /// Build the pallet-driven design from the current editor state (matrices + the already-validated
        /// <paramref name="inputs"/>). Commits the live matrix into its fondo slot first (so fondo 0 = the master grid),
        /// then assembles fondo 0 + the extra fondos + per-post cabeceras/peraltes + the toggles/annotation/safety. Returns
        /// null ONLY when fondo 0 has no frentes/levels (the window maps that to "Define frentes y niveles.").
        /// </summary>
        public SelectivePalletDesign BuildDesign(SelectiveDesignInputs inputs)
        {
            // Commit the live matrix into its fondo slot, then read fondo 0 (the master frente grid) + the extra fondos.
            SaveWorkingToSelected(inputs.WorkingDepth, inputs.WorkingCabeceraOverride);
            if (FondoMatrices.Count == 0) FondoMatrices.Add(SnapshotWorking(inputs.WorkingDepth, inputs.WorkingCabeceraOverride));
            var fondo0 = FondoMatrices[0];
            if (fondo0.Bays.Count == 0 || fondo0.Bays[0].Count == 0) return null;

            var design = new SelectivePalletDesign
            {
                PostId = inputs.PostId,
                PostPeralte = inputs.PostPeralte,
                PalletTolerance = inputs.PalletTolerance,
                VerticalClearance = inputs.VerticalClearance,
                FloorBeamRise = inputs.FloorBeamRise,
                PalletDepth = fondo0.Depth > 0.0 ? fondo0.Depth : inputs.Fondo, // fondo 0's own depth
                DepthCount = inputs.DepthCount
            };

            foreach (var separator in inputs.Separators)
            {
                design.SeparatorLengths.Add(separator);
            }

            foreach (var bay in BuildBayDesigns(fondo0))
            {
                design.Bays.Add(bay);
            }

            design.CabeceraFondoOverrides.Add(fondo0.CabeceraOverride); // fondo 0's custom cabecera fondo (0 = auto)

            // Extra fondos: each carries its OWN levels + its OWN fondo (depth) + its OWN cabecera override AND its OWN
            // frente count (a corner layout). The resolver aligns the overlapping widths to the longest fondo.
            for (var k = 1; k < inputs.DepthCount; k++)
            {
                var m = k < FondoMatrices.Count ? FondoMatrices[k] : fondo0;
                design.ExtraFondoBays.Add(BuildBayDesigns(m));
                design.ExtraFondoDepths.Add(m.Depth);
                design.CabeceraFondoOverrides.Add(m.CabeceraOverride);
            }

            SyncPostCabeceras();
            foreach (var cabecera in PostCabeceras)
            {
                design.PostCabeceras.Add(cabecera);
            }

            foreach (var peralte in PostPeraltes)
            {
                design.PostPeraltes.Add(peralte);
            }

            // The other fondos' cabecera rows travel as they are; fondo 0 stays in PostCabeceras above, which is what
            // keeps the legacy projection (and the frontal master representation) intact (I-43).
            foreach (var row in ExtraFondoPostCabeceras)
            {
                design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration>(row));
            }

            design.DrawBasePlate = inputs.DrawBasePlate;
            design.NumberFronts = inputs.NumberFronts;
            design.NumberLevels = inputs.NumberLevels;
            design.DrawRackName = inputs.DrawRackName;
            design.DrawPallets = inputs.DrawPallets;
            design.AnnotationScale = inputs.AnnotationScale;
            design.Dimensions = inputs.Dimensions;
            design.DimensionStyle = inputs.DimensionStyle;
            foreach (var safety in inputs.SafetySelections)
            {
                design.SafetySelections.Add(safety);
            }

            return design;
        }
    }
}
