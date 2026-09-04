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
    /// numero exacto de niveles por frente, apply-by-scope, and <see cref="BuildDesign"/>). Extracted verbatim from
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

        /// <summary>Per-bay "elevacion de larguero a piso" override (in); null = inherit the global, <c>0.0</c> = an
        /// explicit zero (I-43, ID14). Parallel to <see cref="Bays"/> by bay.</summary>
        public List<double?> FloorBeamRiseOverrides { get; } = new List<double?>();

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
            // EVERY per-bay list is cleared, or a leftover entry survives a full reset by index and the lists stop
            // being parallel — a stale override would reappear on a frente the user never touched (I-43, ID14).
            Bays.Clear();
            FloorBeams.Clear();
            BayHeights.Clear();
            BaySegments.Clear();
            FloorBeamRiseOverrides.Clear();
            for (var b = 0; b < bayCount; b++)
            {
                var column = new List<SelectiveEditorCell>();
                for (var l = 0; l < levelCount; l++) column.Add(NewCell());
                Bays.Add(column);
                FloorBeams.Add(false);
                BayHeights.Add(null);
                BaySegments.Add(new List<SelectiveSegment>());
                // Un frente NUEVO sin origen nace con valor DIRECTO (INV-12); solo la carga legacy produce null.
                FloorBeamRiseOverrides.Add(SelectiveRackDefaults.DefaultFloorBeamRise);
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
            snap.FloorBeamRiseOverrides.AddRange(FloorBeamRiseOverrides);
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
            FloorBeamRiseOverrides.Clear();
            foreach (var column in snap.Bays) Bays.Add(CloneColumn(column));
            FloorBeams.AddRange(snap.FloorBeams);
            BayHeights.AddRange(snap.BayHeights);
            FloorBeamRiseOverrides.AddRange(snap.FloorBeamRiseOverrides);
            foreach (var segments in snap.BaySegments) BaySegments.Add(CloneSegments(segments));
            if (Bays.Count == 0) { Bays.Add(new List<SelectiveEditorCell> { NewCell() }); FloorBeams.Add(false); BayHeights.Add(null); BaySegments.Add(new List<SelectiveSegment>()); FloorBeamRiseOverrides.Add(null); }
            while (BaySegments.Count < Bays.Count) BaySegments.Add(new List<SelectiveSegment>()); // defensive: keep parallel to bays (legacy snapshots)
            while (FloorBeamRiseOverrides.Count < Bays.Count) FloorBeamRiseOverrides.Add(null);   // same, for a snapshot taken before ID14
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
                    m.FloorBeamRiseOverrides.Add(b < source.FloorBeamRiseOverrides.Count ? source.FloorBeamRiseOverrides[b] : null);
                    m.BaySegments.Add(b < source.BaySegments.Count ? CloneSegments(source.BaySegments[b]) : new List<SelectiveSegment>());
                }
                else
                {
                    m.Bays.Add(widthSeed != null && b < widthSeed.Bays.Count ? CloneColumn(widthSeed.Bays[b]) : new List<SelectiveEditorCell> { NewCell() });
                    m.FloorBeams.Add(false);
                    m.BayHeights.Add(null);
                    m.FloorBeamRiseOverrides.Add(SelectiveRackDefaults.DefaultFloorBeamRise); // frente nuevo sin origen
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
                    HeightOverride = m.BayHeights[b],
                    FloorBeamRiseOverride = b < m.FloorBeamRiseOverrides.Count ? m.FloorBeamRiseOverrides[b] : null
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
                m.FloorBeamRiseOverrides.Add(bayDesign.FloorBeamRiseOverride);
                m.BaySegments.Add(CloneSegments(bayDesign.Segments));
            }

            if (m.Bays.Count == 0) { m.Bays.Add(new List<SelectiveEditorCell> { NewCell() }); m.FloorBeams.Add(false); m.BayHeights.Add(null); m.FloorBeamRiseOverrides.Add(null); m.BaySegments.Add(new List<SelectiveSegment>()); }
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
                    // A new frente clones the last one, exactly like every other per-bay field. It does NOT resurrect a
                    // value a previous shrink deleted: what it copies is whatever the CURRENT last frente holds.
                    FloorBeamRiseOverrides.Add(FloorBeamRiseOverrides[FloorBeamRiseOverrides.Count - 1]);
                    BaySegments.Add(CloneSegments(BaySegments[BaySegments.Count - 1]));
                }
                else
                {
                    Bays.Add(new List<SelectiveEditorCell> { NewCell() });
                    FloorBeams.Add(false);
                    BayHeights.Add(null);
                    FloorBeamRiseOverrides.Add(SelectiveRackDefaults.DefaultFloorBeamRise); // frente nuevo sin origen
                    BaySegments.Add(new List<SelectiveSegment>());
                }
            }

            while (Bays.Count > bayCount)
            {
                Bays.RemoveAt(Bays.Count - 1);
                BaySegments.RemoveAt(BaySegments.Count - 1);
                FloorBeams.RemoveAt(FloorBeams.Count - 1);
                BayHeights.RemoveAt(BayHeights.Count - 1);
                FloorBeamRiseOverrides.RemoveAt(FloorBeamRiseOverrides.Count - 1);
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

        /// <summary>The DISTINCT frentes the cell selection touches, ascending — the selection read at the frente
        /// axis, for properties whose authority is the frente rather than the cell (I-43, gate 8A).</summary>
        public IReadOnlyList<int> SelectedFrontIndices()
            => selectedPositions.Select(position => position.FrontIndex).Distinct().OrderBy(index => index).ToList();

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

        /// <summary>
        /// Whether the target set FOLLOWS the fondo on screen or was chosen deliberately (I-43, gate 8A correction).
        /// <para>
        /// Without it the two are indistinguishable whenever an explicit choice happens to be a single fondo that
        /// matches the one being edited: navigating away would silently re-aim it. The mode records the user's
        /// INTENT, which no comparison of the sets can recover. Runtime only, like the set itself.
        /// </para>
        /// </summary>
        public SelectiveTargetMode TargetMode { get; private set; } = SelectiveTargetMode.FollowCurrent;

        /// <summary>Choose "Actual": the targets are the fondo being edited, and they keep following it.</summary>
        public void FollowCurrentFondo()
        {
            TargetMode = SelectiveTargetMode.FollowCurrent;
            targetFondos = CurrentFondoOnly();
        }

        /// <summary>
        /// Choose "Todos": every fondo, and it KEEPS meaning every fondo (gate 8 correction). Storing the indices
        /// instead would freeze the answer: a fondo added afterwards would silently fall outside the set the user
        /// believes is "todos", and the remembered preference could not open a rack with a different fondo count.
        /// </summary>
        public void FollowAllFondos()
        {
            TargetMode = SelectiveTargetMode.All;
            targetFondos = AllFondos();
        }

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
            TargetMode = SelectiveTargetMode.Explicit; // a deliberate choice, even when it is a single fondo
            SetTargetFondosCore(fondos);
        }

        /// <summary>Re-normalize the set WITHOUT touching the mode — for the internal reconciliations (a fondo count
        /// change), which are not the user choosing anything.</summary>
        private void SetTargetFondosCore(IEnumerable<int> fondos)
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
            SelectedFondo = fondoIndex;
            if (TargetMode == SelectiveTargetMode.FollowCurrent) targetFondos = CurrentFondoOnly();
            // "Todos" does not depend on the fondo on screen, so navigating leaves it exactly as it is.
        }

        /// <summary>Drop targets the rack no longer has after a fondo-count change, never leaving the set empty.
        /// Call it whenever the number of fondos changes.</summary>
        public void SyncTargetFondos()
        {
            if (TargetMode == SelectiveTargetMode.FollowCurrent) FollowCurrentFondo();
            else if (TargetMode == SelectiveTargetMode.All) FollowAllFondos(); // re-expands: a new fondo joins "todos"
            else SetTargetFondosCore(targetFondos.Fondos);
        }

        /// <summary>Every fondo this rack has; a rack with none still yields fondo 0, so the set is never empty.</summary>
        private SelectiveFondoTargets AllFondos()
        {
            var count = FondoCount;
            return count > 0 ? SelectiveFondoTargets.Of(Enumerable.Range(0, count)) : SelectiveFondoTargets.Single(0);
        }

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

        // ---- Floor-beam rise by (fondo, frente) - I-43, ID14 ----

        /// <summary>The "elevacion de larguero a piso" override of <c>(fondoIndex, frontIndex)</c>; null = inherit the
        /// global. Reads the LIVE working matrix for the fondo being edited and the stored slot for the rest.</summary>
        public double? FloorBeamRiseOverrideAt(int fondoIndex, int frontIndex)
        {
            var row = FloorBeamRiseRow(fondoIndex);
            return row != null && frontIndex >= 0 && frontIndex < row.Count ? row[frontIndex] : null;
        }

        private List<double?> FloorBeamRiseRow(int fondoIndex)
        {
            if (FondoMatrices.Count == 0) return fondoIndex == 0 ? FloorBeamRiseOverrides : null;
            if (fondoIndex < 0 || fondoIndex >= FondoMatrices.Count) return null;
            return fondoIndex == SelectedFondo ? FloorBeamRiseOverrides : FondoMatrices[fondoIndex].FloorBeamRiseOverrides;
        }

        /// <summary>
        /// Write a frente-wide "elevacion de larguero a piso" over <see cref="TargetFondos"/> - the same fondo axis the
        /// rest of the editor uses, with the two scopes a FRENTE really has (I-43, ID14).
        /// <para>
        /// <paramref name="rise"/> null is the RESTORE: those frentes go back to inheriting the global. A value of
        /// <c>0.0</c> is written as an explicit zero, which is a different thing. A target fondo that does not have
        /// the requested frente is OMITTED and reported - never padded, never clamped onto a neighbouring frente.
        /// </para>
        /// <para>
        /// It writes the LIVE matrix for the fondo being edited and the stored matrices for the others, so the caller
        /// mutates everything in one call and recomputes ONCE.
        /// </para>
        /// </summary>
        public SelectiveFrontApplyResult ApplyFloorBeamRiseToTargets(
            SelectiveFrontApplyScope scope, int frontIndex, double? rise)
            => ApplyToFrontTargets(scope, frontIndex, (fondo, front) => FloorBeamRiseRow(fondo)[front] = rise);

        /// <summary>
        /// Set "larguero a piso" on a frente across the target fondos (I-43, gate 7). The flag has no inheritance:
        /// it is written true or false explicitly, so there is no restore value for it.
        /// </summary>
        public SelectiveFrontApplyResult ApplyFloorBeamToTargets(
            SelectiveFrontApplyScope scope, int frontIndex, bool floorBeam)
            => ApplyToFrontTargets(scope, frontIndex, (fondo, front) => FloorBeamRow(fondo)[front] = floorBeam);

        /// <summary>
        /// Set a frente's manual height across the target fondos (I-43, gate 7). Null is the RESTORE: that frente goes
        /// back to the derived height. There is no run-wide default for this one, so null means "auto", exactly the
        /// semantics the editor has always had.
        /// </summary>
        public SelectiveFrontApplyResult ApplyBayHeightToTargets(
            SelectiveFrontApplyScope scope, int frontIndex, double? height)
            => ApplyToFrontTargets(scope, frontIndex, (fondo, front) => BayHeightRow(fondo)[front] = height);

        /// <summary>
        /// Project one "medio frente" tramo configuration onto the SAME frente of every target fondo (I-43, gate 7).
        /// Each target receives an independent copy of the segments, so editing one later cannot move another.
        /// <para>
        /// Only <see cref="SelectiveFrontApplyScope.Front"/> exists here, and that is a DOMAIN limit rather than an
        /// omission. A tramo list is a set of absolute lengths that must fit the frente's width; the same
        /// <c>FrontIndex</c> has the same width in every fondo — the horizontal grid comes from fondo 0 so the posts
        /// align — which is what makes projecting across fondos meaningful. Different frentes do NOT share a width,
        /// so an "All" would push lengths onto frentes they may not fit, and the resolver would silently fall back to
        /// a full-width frente on each one that fails. Rather than invent a scope that quietly does nothing on half
        /// the rack, medio frente stays <c>Front x TargetFondos</c>.
        /// </para>
        /// </summary>
        public SelectiveFrontApplyResult ApplySegmentsToTargets(int frontIndex, IEnumerable<SelectiveSegment> segments)
        {
            var source = CloneSegments(segments);
            return ApplyToFrontTargets(
                SelectiveFrontApplyScope.Front,
                frontIndex,
                (fondo, front) => SegmentRow(fondo)[front] = CloneSegments(source));
        }

        /// <summary>
        /// Write BOTH properties a frente owns — "larguero a piso" and its elevation — in ONE pass (I-43, gate 8A).
        /// <para>
        /// They travel together because the editor edits them together: the left panel shows the pair for the frente
        /// on screen and the scope buttons apply that pair. Splitting them into two calls would mean two mutations and
        /// two chances for the scopes to disagree about which frentes they reached.
        /// </para>
        /// <para>
        /// The elevation is a DIRECT value of the frente, not an override of anything: there is no global to inherit
        /// from and no "empty means inherit". It keeps its value while "piso" is off, so switching the beam back on
        /// restores the elevation the user had chosen.
        /// </para>
        /// </summary>
        /// <summary>
        /// Set the LEVEL COUNT of the resolved frentes of every target fondo (I-43, gate 8A correction).
        /// <para>
        /// An exact count rather than a delta, because a delta means different things on frentes that start with
        /// different numbers of levels: "+1" over a selection leaves them as uneven as it found them, while "5" says
        /// what the user actually wants. Growing clones the top level of the column it grows; shrinking drops the tail
        /// and never resurrects it; one level is the floor, so a frente is never emptied.
        /// </para>
        /// </summary>
        public SelectiveFrontApplyResult ApplyLevelCountToTargets(
            SelectiveFrontApplyScope scope, int frontIndex, int levels)
        {
            var wanted = Math.Max(1, levels);
            var result = ApplyToFrontTargets(scope, frontIndex, (fondo, front) =>
            {
                var columns = ColumnsOf(fondo);
                if (columns == null || front >= columns.Count) return;
                var column = columns[front];
                while (column.Count < wanted) column.Add(column.Count > 0 ? column[column.Count - 1].Clone() : NewCell());
                while (column.Count > wanted) column.RemoveAt(column.Count - 1);
            });

            ClampSelection(); // levels vanished: prune the selection to what survived
            return result;
        }

        /// <summary>
        /// Set the number of frentes of every TARGET fondo (I-43, gate 8A). This is TOPOLOGY, so it is the one
        /// operation that changes the shape of the rack rather than a value inside it.
        /// <para>
        /// Each fondo is resized INDEPENDENTLY — they do not have to share a frente count — and every parallel list
        /// travels with it: cells, "piso", elevations, manual heights and tramos. Shrinking drops the tail and
        /// growing clones the last surviving frente, so nothing a shrink deleted comes back. It never creates a fondo:
        /// a target index the rack does not have is omitted.
        /// </para>
        /// </summary>
        public SelectiveFondoApplyResult ApplyBayCountToTargets(int bayCount)
        {
            if (bayCount < 1) return new SelectiveFondoApplyResult(new List<int>(), targetFondos.Fondos.ToList());

            var applied = new List<int>();
            var omitted = new List<int>();
            foreach (var fondo in targetFondos.Fondos)
            {
                if (fondo < 0 || fondo >= Math.Max(FondoMatrices.Count, 1))
                {
                    omitted.Add(fondo);
                    continue;
                }

                if (FondoMatrices.Count == 0 || fondo == SelectedFondo) ResizeBays(bayCount);
                else ResizeStoredMatrix(FondoMatrices[fondo], bayCount);
                applied.Add(fondo);
            }

            SyncPostCabeceras(); // trims the cabeceras of posts the resized fondos no longer have
            ClampSelection();    // and prunes the selection to what survived
            return new SelectiveFondoApplyResult(applied, omitted);
        }

        /// <summary>Resize a STORED fondo matrix with exactly the semantics <see cref="ResizeBays"/> applies to the
        /// live one, keeping its five per-bay lists parallel.</summary>
        private void ResizeStoredMatrix(SelectiveEditorFondoMatrix matrix, int bayCount)
        {
            while (matrix.Bays.Count < bayCount)
            {
                var last = matrix.Bays.Count - 1;
                if (last >= 0)
                {
                    matrix.Bays.Add(matrix.Bays[last].Select(cell => cell.Clone()).ToList());
                    matrix.FloorBeams.Add(matrix.FloorBeams[last]);
                    matrix.BayHeights.Add(matrix.BayHeights[last]);
                    matrix.FloorBeamRiseOverrides.Add(last < matrix.FloorBeamRiseOverrides.Count ? matrix.FloorBeamRiseOverrides[last] : null);
                    matrix.BaySegments.Add(CloneSegments(last < matrix.BaySegments.Count ? matrix.BaySegments[last] : null));
                }
                else
                {
                    matrix.Bays.Add(new List<SelectiveEditorCell> { NewCell() });
                    matrix.FloorBeams.Add(false);
                    matrix.BayHeights.Add(null);
                    matrix.FloorBeamRiseOverrides.Add(SelectiveRackDefaults.DefaultFloorBeamRise); // frente nuevo sin origen
                    matrix.BaySegments.Add(new List<SelectiveSegment>());
                }
            }

            while (matrix.Bays.Count > bayCount)
            {
                var last = matrix.Bays.Count - 1;
                matrix.Bays.RemoveAt(last);
                if (last < matrix.FloorBeams.Count) matrix.FloorBeams.RemoveAt(last);
                if (last < matrix.BayHeights.Count) matrix.BayHeights.RemoveAt(last);
                if (last < matrix.FloorBeamRiseOverrides.Count) matrix.FloorBeamRiseOverrides.RemoveAt(last);
                if (last < matrix.BaySegments.Count) matrix.BaySegments.RemoveAt(last);
            }
        }

        /// <summary>
        /// Give every frente a DIRECT elevation, filling the ones that have none from <paramref name="legacyGlobal"/>
        /// (I-43, gate 8A).
        /// <para>
        /// This is the whole legacy contract in one place. A document written before the elevation became a property
        /// of the frente carries only the run-wide value, and the drawing it described used that value everywhere; so
        /// on load each frente materializes it as its own. From then on the frente is the authority and the old global
        /// is never consulted again — which is what removes the "which frentes still follow the global?" ambiguity.
        /// </para>
        /// </summary>
        public void MaterializeFloorBeamRises(double legacyGlobal)
        {
            // A NEGATIVE value is not a rise; 0 IS one. The old run-wide field accepted >= 0, so a document that
            // deliberately said "no rise at all" must materialize 0 and not the 4" default (I-43, gate 8A correction).
            var value = legacyGlobal >= 0.0 ? legacyGlobal : SelectiveRackDefaults.DefaultFloorBeamRise;
            void Fill(List<double?> row, int count)
            {
                while (row.Count < count) row.Add(null);
                for (var i = 0; i < count; i++)
                {
                    if (!row[i].HasValue) row[i] = value;
                }
            }

            Fill(FloorBeamRiseOverrides, Bays.Count);

            // TODOS los slots, incluido el del fondo seleccionado. Saltarlo daba por sentado que la fila viva y ese
            // slot son la misma cosa, y durante una carga NO lo son: se materializa antes de restaurar, asi que el
            // slot se quedaba con nulos y el RestoreWorkingFrom siguiente los devolvia a la fila viva (I-43,
            // gate 8.6D, INV-12). Cada contenedor se rellena hasta SU propio conteo de frentes.
            for (var k = 0; k < FondoMatrices.Count; k++)
            {
                Fill(FondoMatrices[k].FloorBeamRiseOverrides, FondoMatrices[k].Bays.Count);
            }
        }

        /// <summary>
        /// Resolve the frentes a frente-wide operation reaches and write every one of them (I-43, gate 7).
        /// <para>
        /// It is the single place that knows how a frente-wide edit lands: which fondos are targeted, which of them
        /// actually have the frente — a target that does not is OMITTED and reported, never padded and never clamped
        /// onto a neighbour — and that the fondo being edited is the LIVE matrix while the rest are their stored ones.
        /// Every property routes through it so none of them can grow its own interpretation.
        /// </para>
        /// </summary>
        private SelectiveFrontApplyResult ApplyToFrontTargets(
            SelectiveFrontApplyScope scope, int frontIndex, Action<int, int> write)
        {
            var topology = SelectiveTopology.From(this);
            var applied = new List<(int FondoIndex, int FrontIndex)>();
            var omitted = new List<int>();

            foreach (var fondo in targetFondos.Fondos)
            {
                var frentes = topology.HasFondo(fondo) ? topology.FrontCount(fondo) : 0;
                if (frentes == 0 || !EnsureFrontRows(fondo, frentes))
                {
                    omitted.Add(fondo);
                    continue;
                }

                if (scope == SelectiveFrontApplyScope.All)
                {
                    for (var front = 0; front < frentes; front++)
                    {
                        write(fondo, front);
                        applied.Add((fondo, front));
                    }

                    continue;
                }

                if (scope == SelectiveFrontApplyScope.Selected)
                {
                    // The frentes the cell selection names. A fondo that reaches none of them is omitted whole, and a
                    // single frente the fondo lacks is simply skipped: never padded, never clamped onto a neighbour.
                    var reached = 0;
                    foreach (var front in SelectedFrontIndices())
                    {
                        if (front < 0 || front >= frentes) continue;
                        write(fondo, front);
                        applied.Add((fondo, front));
                        reached++;
                    }

                    if (reached == 0) omitted.Add(fondo);
                    continue;
                }

                if (frontIndex < 0 || frontIndex >= frentes)
                {
                    omitted.Add(fondo); // this fondo simply does not have that frente
                    continue;
                }

                write(fondo, frontIndex);
                applied.Add((fondo, frontIndex));
            }

            return new SelectiveFrontApplyResult(scope, applied, omitted);
        }

        /// <summary>Grow that fondo's per-bay lists so every frente it has is addressable; false when the fondo has no
        /// storage at all. Filling a list up to the frentes a fondo REALLY has is not padding the rack.</summary>
        private bool EnsureFrontRows(int fondoIndex, int frentes)
        {
            var floorBeams = FloorBeamRow(fondoIndex);
            var heights = BayHeightRow(fondoIndex);
            var segments = SegmentRow(fondoIndex);
            var rises = FloorBeamRiseRow(fondoIndex);
            if (floorBeams == null || heights == null || segments == null || rises == null) return false;

            while (floorBeams.Count < frentes) floorBeams.Add(false);
            while (heights.Count < frentes) heights.Add(null);
            while (segments.Count < frentes) segments.Add(new List<SelectiveSegment>());
            while (rises.Count < frentes) rises.Add(null);
            return true;
        }

        private List<bool> FloorBeamRow(int fondoIndex)
        {
            if (FondoMatrices.Count == 0) return fondoIndex == 0 ? FloorBeams : null;
            if (fondoIndex < 0 || fondoIndex >= FondoMatrices.Count) return null;
            return fondoIndex == SelectedFondo ? FloorBeams : FondoMatrices[fondoIndex].FloorBeams;
        }

        private List<double?> BayHeightRow(int fondoIndex)
        {
            if (FondoMatrices.Count == 0) return fondoIndex == 0 ? BayHeights : null;
            if (fondoIndex < 0 || fondoIndex >= FondoMatrices.Count) return null;
            return fondoIndex == SelectedFondo ? BayHeights : FondoMatrices[fondoIndex].BayHeights;
        }

        private List<List<SelectiveSegment>> SegmentRow(int fondoIndex)
        {
            if (FondoMatrices.Count == 0) return fondoIndex == 0 ? BaySegments : null;
            if (fondoIndex < 0 || fondoIndex >= FondoMatrices.Count) return null;
            return fondoIndex == SelectedFondo ? BaySegments : FondoMatrices[fondoIndex].BaySegments;
        }

        // ---- Fondo-wide properties over the target fondos (I-43, gate 7) ----

        /// <summary>
        /// Write the pallet depth of every TARGET fondo. This one has no inner scope at all: the depth belongs to a
        /// fondo as a whole, so inventing a "frente" or "celda" reach for it would be fiction.
        /// <para>
        /// The custom cabeceras of each touched fondo immediately adopt the depth their fondo now dictates, through
        /// the same authority that governs it everywhere else — so the editor never shows a cabecera at a depth the
        /// drawing would not use.
        /// </para>
        /// </summary>
        public SelectiveFondoApplyResult ApplyPalletDepthToTargets(double depth)
            => ApplyToFondoTargets(fondo => FondoMatrices[fondo].Depth = depth);

        /// <summary>
        /// Write the "fondo de cabecera" override of every TARGET fondo; null (or a non-positive value) is the
        /// RESTORE, which returns those fondos to the derived rule (<c>tarima − 6"</c>).
        /// </summary>
        public SelectiveFondoApplyResult ApplyCabeceraDepthToTargets(double? cabeceraOverride)
            => ApplyToFondoTargets(fondo => FondoMatrices[fondo].CabeceraOverride =
                cabeceraOverride.HasValue && cabeceraOverride.Value > 0.0 ? cabeceraOverride.Value : 0.0);

        private SelectiveFondoApplyResult ApplyToFondoTargets(Action<int> write)
        {
            var applied = new List<int>();
            var omitted = new List<int>();
            foreach (var fondo in targetFondos.Fondos)
            {
                if (fondo < 0 || fondo >= FondoMatrices.Count)
                {
                    omitted.Add(fondo);
                    continue;
                }

                write(fondo);
                applied.Add(fondo);
            }

            // A fondo's depth just moved, so the cabeceras stored there must follow it now rather than at the next
            // read: the depth of a cabecera is the fondo's authority (gate 4), never the configuration's own.
            foreach (var fondo in applied)
            {
                var depth = CabeceraDepthOfFondo(fondo);
                var row = CabeceraRow(fondo);
                for (var post = 0; row != null && post < row.Count; post++)
                {
                    SelectiveCabeceraAuthority.ImposeFondoDepth(row[post], depth);
                }
            }

            return new SelectiveFondoApplyResult(applied, omitted);
        }

        // ---- Custom cabeceras by (fondo, post) - I-43 ----

        /// <summary>The custom cabecera stored at <c>(fondoIndex, postIndex)</c>, or null for the standard one.</summary>
        public RackFrameConfiguration CabeceraAt(int fondoIndex, int postIndex)
        {
            var row = CabeceraRow(fondoIndex);
            return row != null && postIndex >= 0 && postIndex < row.Count ? row[postIndex] : null;
        }

        /// <summary>
        /// The CABECERA depth of a fondo: its <c>CabeceraFondoOverride</c> when set, else the rule
        /// (<c>tarima − 6"</c>). Delegates to <see cref="SelectiveDepthLayout.CabeceraDepthOfFondoValue"/>, the same
        /// precedence the resolved system uses, so the editor and the drawing can never disagree about it (I-43).
        /// <para>
        /// It reads the fondo's SLOT, so a caller editing the depth boxes must commit them
        /// (<see cref="SaveWorkingToSelected"/>) first — the same rule every other build path already follows.
        /// </para>
        /// </summary>
        public double CabeceraDepthOfFondo(int fondoIndex)
        {
            if (fondoIndex < 0 || fondoIndex >= FondoMatrices.Count) return 0.0;
            var matrix = FondoMatrices[fondoIndex];
            return SelectiveDepthLayout.CabeceraDepthOfFondoValue(matrix.Depth, matrix.CabeceraOverride);
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

                var copy = configuration == null
                    ? null
                    : (deepCopy != null ? deepCopy(configuration) : configuration);

                // Each target gets ITS OWN fondo's cabecera depth, resolved with the same rule the drawing uses, so the
                // state is already correct before anything recomputes. Without this the configuration would travel
                // carrying the depth of the fondo it was authored on (I-43).
                SelectiveCabeceraAuthority.ImposeFondoDepth(copy, CabeceraDepthOfFondo(fondo));

                row[postIndex] = copy;
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
