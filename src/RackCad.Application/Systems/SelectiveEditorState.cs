using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
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

        /// <summary>Per-post PERALTE override; 0 = inherit the global. One entry per post, parallel to <see cref="PostCabeceras"/>.</summary>
        public List<double> PostPeraltes { get; } = new List<double>();

        /// <summary>Selected bay (frente) index in the working matrix.</summary>
        public int SelBay { get; set; }

        /// <summary>Selected level index in the working matrix.</summary>
        public int SelLevel { get; set; }

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

            SelBay = 0;
            SelLevel = 0;
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

        /// <summary>Keep the selection inside the working matrix after a structural change.</summary>
        public void ClampSelection()
        {
            SelBay = Math.Min(Math.Max(0, SelBay), Bays.Count - 1);
            var levelCount = SelBay >= 0 && SelBay < Bays.Count ? Bays[SelBay].Count : 1;
            SelLevel = Math.Min(Math.Max(0, SelLevel), levelCount - 1);
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
        /// touched (bay, level) coordinates (bay-outer, level-inner order) so the window can refresh just those cells.</summary>
        public IReadOnlyList<(int Bay, int Level)> ApplyScope(SelectiveApplyScope scope, SelectiveEditorCell values)
        {
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

        /// <summary>Keep the per-post cabecera + peralte lists sized to the MASTER grid's posts (masterFrentes+1),
        /// preserving existing entries. Sizing to the LONGEST fondo (not the working one) means switching to a shorter
        /// fondo never truncates and loses fondo 0's custom cabeceras / per-post peraltes.</summary>
        public void SyncPostCabeceras()
        {
            var posts = MaxFrenteCount() + 1;
            while (PostCabeceras.Count < posts) PostCabeceras.Add(null);
            while (PostCabeceras.Count > posts) PostCabeceras.RemoveAt(PostCabeceras.Count - 1);
            while (PostPeraltes.Count < posts) PostPeraltes.Add(0.0);
            while (PostPeraltes.Count > posts) PostPeraltes.RemoveAt(PostPeraltes.Count - 1);
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
