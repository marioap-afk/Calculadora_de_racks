using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Advanced editor for a selective rack (FRONTAL view). The user edits a bays × levels MATRIX where each
    /// bay has its OWN number of levels and its own "larguero a piso" flag, and each cell carries its own pallet
    /// (frente/alto), count and larguero. <see cref="SelectiveGeometryResolver"/> derives the larguero lengths,
    /// the floor-referenced level Ys and the post height (tallest bay governs); <see cref="SelectiveFrontalBuilder"/>
    /// lays out the blocks. Click a cell to edit it, then apply the values to the cell / row / column / all.
    /// </summary>
    public partial class RackSelectiveWindow : Window
    {
        private static readonly Brush PostBrush = UiSupport.FrozenBrush(Color.FromRgb(0x3D, 0xC9, 0x86));
        private static readonly Brush PostFill = UiSupport.FrozenBrush(Color.FromArgb(0x30, 0x3D, 0xC9, 0x86));
        private static readonly Brush PostHiBrush = UiSupport.FrozenBrush(Color.FromRgb(0xFF, 0xC5, 0x3D));
        private static readonly Brush PostHiFill = UiSupport.FrozenBrush(Color.FromArgb(0x55, 0xFF, 0xC5, 0x3D));
        private static readonly Brush CelosiaBrush = UiSupport.FrozenBrush(Color.FromRgb(0x2E, 0x9C, 0x66));
        private static readonly Brush BeamBrush = UiSupport.FrozenBrush(Color.FromRgb(0xE0, 0x8A, 0x2B));
        private static readonly Brush BeamFill = UiSupport.FrozenBrush(Color.FromArgb(0x66, 0xE0, 0x8A, 0x2B));
        private static readonly Brush PlateFill = UiSupport.FrozenBrush(Color.FromRgb(0xB7, 0xC3, 0xCF));
        private static readonly Brush PalletBrush = UiSupport.FrozenBrush(Color.FromRgb(0xB0, 0x8D, 0x57));
        private static readonly Brush PalletFill = UiSupport.FrozenBrush(Color.FromArgb(0x33, 0xB0, 0x8D, 0x57));
        private static readonly Brush FloorStroke = UiSupport.FrozenBrush(Color.FromRgb(0x6A, 0x7B, 0x8A));
        private static readonly Brush LabelStroke = UiSupport.FrozenBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));

        private static readonly Brush CellStroke = UiSupport.FrozenBrush(Color.FromRgb(0xD8, 0xDE, 0xE6));
        private static readonly Brush CellText = UiSupport.FrozenBrush(Color.FromRgb(0x1F, 0x29, 0x33));
        private static readonly Brush CellSelStroke = UiSupport.FrozenBrush(Color.FromRgb(0x2F, 0x6F, 0xED));
        private static readonly Brush CellSelFill = UiSupport.FrozenBrush(Color.FromRgb(0xDB, 0xEA, 0xFE));

        private readonly RackCatalog catalog;
        private readonly SelectiveFrontalBuilder builder = new SelectiveFrontalBuilder();
        private readonly SelectiveGeometryResolver resolver = new SelectiveGeometryResolver();
        private readonly bool canInsertInAutoCad;

        /// <summary>The design matrix: <c>bays[bay][level]</c>, level 0 = ground; each bay has its own length.</summary>
        private readonly List<List<Cell>> bays = new List<List<Cell>>();

        /// <summary>Per-bay "larguero a piso" flag, parallel to <see cref="bays"/>.</summary>
        private readonly List<bool> floorBeams = new List<bool>();

        /// <summary>Per-bay manual height override (in); null = auto. Parallel to <see cref="bays"/>.</summary>
        private readonly List<double?> bayHeights = new List<double?>();

        /// <summary>Per-bay "medio frente" tramos (N tramos, the last calculated); empty = normal full-width bay. Parallel to <see cref="bays"/>.</summary>
        private readonly List<List<SelectiveSegment>> baySegments = new List<List<SelectiveSegment>>();

        /// <summary>Safety accessories chosen for this rack (id + quantity), for the BOM. Edited via the "Elementos de
        /// seguridad" dialog; drawing them is a future phase (needs their AutoCAD blocks).</summary>
        private readonly List<SelectiveSafetySelection> safetySelections = new List<SelectiveSafetySelection>();

        /// <summary>Optional per-post cabecera (frame); one entry per post (N frentes → N+1 posts), null = run default.</summary>
        private readonly List<RackFrameConfiguration> postCabeceras = new List<RackFrameConfiguration>();
        private readonly List<double> postPeraltes = new List<double>(); // per-post PERALTE override; 0 = inherit the global

        /// <summary>
        /// One saved level matrix per fondo (doble profundidad: each back-to-back side edits its OWN levels). Entry
        /// <see cref="selectedFondo"/> is stale WHILE editing — the live <see cref="bays"/>/<see cref="floorBeams"/>/
        /// <see cref="bayHeights"/> are that fondo's working copy; <see cref="SaveWorkingToSelected"/> commits them
        /// back before switching, building or resizing. Fondo 0 defines the shared frente count.
        /// </summary>
        private readonly List<FondoMatrix> fondoMatrices = new List<FondoMatrix>();
        private int selectedFondo;
        private bool switchingFondo; // guards FondoSelector_Changed while the combo is repopulated

        /// <summary>The dynamic per-gap separator textboxes (one per hueco between consecutive fondos).</summary>
        private readonly List<TextBox> separatorBoxes = new List<TextBox>();

        /// <summary>A saved copy of one fondo's level matrix (bays/floor flags/height overrides) + its own fondo (depth).</summary>
        private sealed class FondoMatrix
        {
            public List<List<Cell>> Bays { get; } = new List<List<Cell>>();
            public List<bool> FloorBeams { get; } = new List<bool>();
            public List<double?> BayHeights { get; } = new List<double?>();
            public List<List<SelectiveSegment>> BaySegments { get; } = new List<List<SelectiveSegment>>();
            public double Depth { get; set; } = SelectiveRackDefaults.DefaultPalletDepth;
            public double CabeceraOverride { get; set; } // custom cabecera fondo; 0 = auto (tarima − allowance)
        }

        private string defaultBeamId;
        private int selBay;
        private int selLevel;
        private bool loadingCell;

        /// <summary>False until the constructor finished wiring the UI. The live-apply handlers (poste, tolerancias,
        /// frentes…) check this so the ItemsSource/SelectedValue assignments during construction don't fire a
        /// premature Recompute on a half-built matrix.</summary>
        private bool initialized;

        /// <summary>Cell Border by (bay, level). Repopulated ONLY inside <see cref="RenderMatrix"/> (the single
        /// structural source, so it can never go stale) and used by <see cref="SelectCell"/>/<see cref="ApplyScope"/>
        /// to restyle/retext just the affected cells instead of rebuilding the whole matrix per click.</summary>
        private readonly Dictionary<(int Bay, int Level), Border> cellBorders = new Dictionary<(int Bay, int Level), Border>();

        /// <summary>The preview's post Rectangle + number TextBlock per post index. Repopulated ONLY inside
        /// <see cref="DrawPreview"/> so <see cref="UpdatePostHighlight"/> can move the picked-post highlight
        /// without destroying and recreating the whole canvas.</summary>
        private readonly List<Rectangle> postRects = new List<Rectangle>();
        private readonly List<TextBlock> postLabels = new List<TextBlock>();

        /// <summary>Coalescing state for <see cref="DeferRecompute"/>: while depth &gt; 0, <see cref="Recompute"/>
        /// only latches <see cref="recomputePending"/>; the outermost scope runs the single pending pass on close.</summary>
        private int recomputeDeferDepth;
        private bool recomputePending;

        /// <summary>Identity of the rack currently edited: stable id (GUID) + client name. Empty for a brand-new rack.</summary>
        private string currentId;
        private string currentName;

        /// <summary>True when the window was opened on an EXISTING rack (RACKEDITAR). The lateral view can only be
        /// inserted then — it links to that rack's frontal; inserting it on a brand-new rack would orphan it.</summary>
        private bool isEditingExisting;

        /// <summary>Descriptive XAML tooltips of Actualizar/Insertar lateral/Insertar planta, captured before
        /// <see cref="UpdateInsertButtons"/> swaps them for the disabled reason, so enabling restores them.</summary>
        private readonly object updateButtonTip;
        private readonly object insertLateralTip;
        private readonly object insertPlantaTip;

        private IReadOnlyList<HeaderBlockInstance> lastInstances;
        private SelectiveRackSystem lastSystem;

        private double mapScale;
        private double mapOffsetX;
        private double mapBottomY;
        private double mapMinX;

        public bool InsertRequested { get; private set; }

        public SelectiveRackSystem SystemToInsert { get; private set; }

        /// <summary>The design that produced <see cref="SystemToInsert"/> — embedded in the drawing for round-trip editing.</summary>
        public SelectivePalletDesign DesignToInsert { get; private set; }

        /// <summary>Stable id of the inserted rack (fresh GUID for a new rack, preserved when re-editing).</summary>
        public string RackId { get; private set; }

        /// <summary>Client-facing name of the inserted rack (may be empty).</summary>
        public string RackName { get; private set; }

        /// <summary>Which view the user asked to insert ("frontal"/"lateral"/"planta"); null when only updating.</summary>
        public string InsertView { get; private set; }

        /// <summary>True when the user chose "Actualizar" (redraw existing views in place, insert nothing).</summary>
        public bool UpdateOnly { get; private set; }

        public RackSelectiveWindow()
            : this(false)
        {
        }

        public RackSelectiveWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();
            updateButtonTip = UpdateButton.ToolTip;
            insertLateralTip = InsertLateralButton.ToolTip;
            insertPlantaTip = InsertPlantaButton.ToolTip;
            catalog = UiSupport.LoadCatalogSafe();

            PostBox.ItemsSource = UiSupport.ToOptions(catalog?.PostProfiles);
            PostBox.SelectedValue = catalog?.Defaults?.Post;
            if (PostBox.SelectedItem == null && PostBox.Items.Count > 0) PostBox.SelectedIndex = 0;

            CellBeamBox.ItemsSource = UiSupport.ToOptions(catalog?.BeamProfiles);
            if (CellBeamBox.Items.Count > 0) CellBeamBox.SelectedIndex = 0;
            defaultBeamId = CellBeamBox.SelectedValue as string;
            CellBeamBox.SelectionChanged += (s, e) => OnBeamChanged();

            InitMatrix(2, 4);
            fondoMatrices.Clear();
            fondoMatrices.Add(SnapshotWorking());
            selectedFondo = 0;
            RebuildFondoSelector();
            RebuildSeparatorFields(1);
            LoadCellEditor();
            RenderMatrix();
            RefreshPostSelect();
            UpdateInsertButtons();
            DimensionsBox.SelectedIndex = 0; // "Ninguna" — cotas off by default
            DimStyleBox.Items.Add(AutoDimStyle); // populated with the drawing's styles later via SetDimensionStyles
            DimStyleBox.SelectedIndex = 0;
            initialized = true; // from here on, field edits live-apply (see GlobalScalar_* / Post_Changed / BayCount_*)
            Recompute();
        }

        /// <summary>
        /// Lateral/planta are views OF an existing system: enabled only when editing one via RACKEDITAR (and
        /// inside AutoCAD). A disabled button with the reason in its tooltip beats a rejection MessageBox.
        /// </summary>
        private void UpdateInsertButtons()
        {
            // "Actualizar" (redraw existing views in place) and adding a linked lateral/planta only make sense on an
            // existing rack, so they light up only when editing via RACKEDITAR (and inside AutoCAD). A new rack starts
            // with "Insertar frontal", which creates the first block.
            var enabled = isEditingExisting && canInsertInAutoCad;
            UpdateButton.IsEnabled = enabled;
            InsertLateralButton.IsEnabled = enabled;
            InsertPlantaButton.IsEnabled = enabled;

            if (!enabled)
            {
                var reason = !canInsertInAutoCad
                    ? "Disponible solo cuando la ventana se abre desde AutoCAD."
                    : "Primero inserta la vista frontal; luego selecciónala con RACKEDITAR y actualiza o agrega vistas desde ahí.";
                UpdateButton.ToolTip = reason;
                InsertLateralButton.ToolTip = reason;
                InsertPlantaButton.ToolTip = reason;
            }
            else
            {
                // Re-enabled (RACKEDITAR): put back the descriptive tooltips, or the disabled reason would linger.
                UpdateButton.ToolTip = updateButtonTip;
                InsertLateralButton.ToolTip = insertLateralTip;
                InsertPlantaButton.ToolTip = insertPlantaTip;
            }
        }

        // ---- Matrix model ----

        /// <summary>One editable matrix cell (a bay's level): its pallet, count and beam.</summary>
        private sealed class Cell
        {
            public double Frente = 42.0;
            public double Alto = 60.0;
            public int PalletCount = 2;
            public string BeamId;
            public double BeamPeralte = SelectiveRackDefaults.DefaultBeamPeralte;

            /// <summary>Optional manual overrides (null = auto): larguero length and the clear below this level.</summary>
            public double? BeamLength;
            public double? Clear;

            public bool HasOverride => BeamLength.HasValue || Clear.HasValue;

            public Cell Clone() => (Cell)MemberwiseClone();

            public void CopyFrom(Cell other)
            {
                Frente = other.Frente;
                Alto = other.Alto;
                PalletCount = other.PalletCount;
                BeamId = other.BeamId;
                BeamPeralte = other.BeamPeralte;
                BeamLength = other.BeamLength;
                Clear = other.Clear;
            }
        }

        private enum Scope { Cell, Row, Column, All }

        private Cell NewCell() => new Cell { BeamId = defaultBeamId, BeamPeralte = SelectiveRackDefaults.DefaultBeamPeralte };

        private void InitMatrix(int bayCount, int levelCount)
        {
            bays.Clear();
            floorBeams.Clear();
            bayHeights.Clear();
            baySegments.Clear();
            for (var b = 0; b < bayCount; b++)
            {
                var column = new List<Cell>();
                for (var l = 0; l < levelCount; l++) column.Add(NewCell());
                bays.Add(column);
                floorBeams.Add(false);
                bayHeights.Add(null);
                baySegments.Add(new List<SelectiveSegment>());
            }

            selBay = 0;
            selLevel = 0;
        }

        // ---- Per-fondo matrices (doble profundidad: each fondo edits its own levels) ----

        private static List<Cell> CloneColumn(List<Cell> column) => column.Select(c => c.Clone()).ToList();

        /// <summary>Deep-clone a bay's medio-frente tramos so edits stay isolated per fondo/snapshot.</summary>
        private static List<SelectiveSegment> CloneSegments(IEnumerable<SelectiveSegment> segments)
            => segments?.Select(s => new SelectiveSegment { Length = s.Length, Loaded = s.Loaded }).ToList() ?? new List<SelectiveSegment>();

        /// <summary>Snapshot the live working matrix (the selected fondo) — including its fondo (depth) box — into a saveable copy.</summary>
        private FondoMatrix SnapshotWorking()
        {
            var snap = new FondoMatrix();
            foreach (var column in bays) snap.Bays.Add(CloneColumn(column));
            snap.FloorBeams.AddRange(floorBeams);
            snap.BayHeights.AddRange(bayHeights);
            foreach (var segments in baySegments) snap.BaySegments.Add(CloneSegments(segments));

            // Invalid text falls back to the fondo's PREVIOUSLY SAVED value (not the global default) so a typo while
            // switching fondos doesn't silently reset this line's depth/override; blank cabecera stays auto (0).
            var previous = selectedFondo >= 0 && selectedFondo < fondoMatrices.Count ? fondoMatrices[selectedFondo] : null;
            if (UiSupport.TryNum(FondoBox.Text, out var d) && d > 0.0) snap.Depth = d;
            else
            {
                snap.Depth = previous != null && previous.Depth > 0.0 ? previous.Depth : SelectiveRackDefaults.DefaultPalletDepth;
                if (!string.IsNullOrWhiteSpace(FondoBox.Text)) pendingWarning = "Fondo de tarima inválido; se conserva el anterior.";
            }

            if (string.IsNullOrWhiteSpace(CabeceraFondoBox.Text)) snap.CabeceraOverride = 0.0; // blank = auto (rule tarima − 6)
            else if (UiSupport.TryNum(CabeceraFondoBox.Text, out var co) && co > 0.0) snap.CabeceraOverride = co;
            else
            {
                snap.CabeceraOverride = previous?.CabeceraOverride ?? 0.0;
                pendingWarning = "Fondo de cabecera inválido (vacío = auto); se conserva el anterior.";
            }

            return snap;
        }

        /// <summary>Load a saved fondo matrix into the live working matrix (deep-cloned so edits stay isolated), incl. its fondo box.</summary>
        private void RestoreWorkingFrom(FondoMatrix snap)
        {
            bays.Clear();
            floorBeams.Clear();
            bayHeights.Clear();
            baySegments.Clear();
            foreach (var column in snap.Bays) bays.Add(CloneColumn(column));
            floorBeams.AddRange(snap.FloorBeams);
            bayHeights.AddRange(snap.BayHeights);
            foreach (var segments in snap.BaySegments) baySegments.Add(CloneSegments(segments));
            if (bays.Count == 0) { bays.Add(new List<Cell> { NewCell() }); floorBeams.Add(false); bayHeights.Add(null); baySegments.Add(new List<SelectiveSegment>()); }
            while (baySegments.Count < bays.Count) baySegments.Add(new List<SelectiveSegment>()); // defensive: keep parallel to bays (legacy snapshots)
            FondoBox.Text = (snap.Depth > 0.0 ? snap.Depth : SelectiveRackDefaults.DefaultPalletDepth).ToString("0.###", CultureInfo.InvariantCulture);
            CabeceraFondoBox.Text = snap.CabeceraOverride > 0.0 ? snap.CabeceraOverride.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
            ClampSelection();
        }

        /// <summary>Commit the live working matrix back into its fondo slot before switching/building/resizing.</summary>
        private void SaveWorkingToSelected()
        {
            if (fondoMatrices.Count == 0) { fondoMatrices.Add(SnapshotWorking()); return; }
            if (selectedFondo >= 0 && selectedFondo < fondoMatrices.Count) fondoMatrices[selectedFondo] = SnapshotWorking();
        }

        /// <summary>A copy of <paramref name="source"/> resized to <paramref name="bayCount"/> frentes: a new frente
        /// clones <paramref name="widthSeed"/>'s column at that index (fondo 0 defines the frente count/width), extra
        /// bays are dropped. Keeps every fondo's posts aligned on the shared grid.</summary>
        private FondoMatrix CloneAligned(FondoMatrix source, int bayCount, FondoMatrix widthSeed)
        {
            var m = new FondoMatrix { Depth = source.Depth, CabeceraOverride = source.CabeceraOverride };
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
                    m.Bays.Add(widthSeed != null && b < widthSeed.Bays.Count ? CloneColumn(widthSeed.Bays[b]) : new List<Cell> { NewCell() });
                    m.FloorBeams.Add(false);
                    m.BayHeights.Add(null);
                    m.BaySegments.Add(new List<SelectiveSegment>());
                }
            }

            return m;
        }

        /// <summary>Load fondo <paramref name="k"/> into the working matrix. Each fondo keeps its OWN frente count (a
        /// corner layout); the resolver aligns the overlapping widths to the longest fondo, so nothing is forced here.</summary>
        private void LoadFondo(int k)
        {
            RestoreWorkingFrom(fondoMatrices[k]);
        }

        /// <summary>Turn a fondo matrix into design bays (the shape the resolver consumes).</summary>
        private static List<SelectiveBayDesign> BuildBayDesigns(FondoMatrix m)
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

        /// <summary>Turn saved design bays into a fondo matrix (for load).</summary>
        private FondoMatrix FondoMatrixFromDesignBays(IList<SelectiveBayDesign> designBays)
        {
            var m = new FondoMatrix();
            foreach (var bayDesign in designBays)
            {
                var column = new List<Cell>();
                foreach (var cell in bayDesign.Levels)
                {
                    column.Add(new Cell
                    {
                        Frente = cell.Pallet?.Frente ?? 42.0,
                        Alto = cell.Pallet?.Alto ?? 60.0,
                        PalletCount = cell.PalletCount,
                        BeamId = cell.BeamId ?? defaultBeamId,
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
                    paddedEmptyFrentesOnLoad++;
                }
                m.Bays.Add(column);
                m.FloorBeams.Add(bayDesign.FloorBeam);
                m.BayHeights.Add(bayDesign.HeightOverride);
                m.BaySegments.Add(CloneSegments(bayDesign.Segments));
            }

            if (m.Bays.Count == 0) { m.Bays.Add(new List<Cell> { NewCell() }); m.FloorBeams.Add(false); m.BayHeights.Add(null); m.BaySegments.Add(new List<SelectiveSegment>()); }
            return m;
        }

        /// <summary>Repopulate the "Editando fondo" combo to match the fondo count; hidden for a single fondo.</summary>
        private void RebuildFondoSelector()
        {
            switchingFondo = true;
            FondoSelectorBox.Items.Clear();
            for (var k = 0; k < fondoMatrices.Count; k++) FondoSelectorBox.Items.Add("Fondo " + (k + 1).ToString(CultureInfo.InvariantCulture));
            if (selectedFondo >= fondoMatrices.Count) selectedFondo = 0;
            FondoSelectorBox.SelectedIndex = fondoMatrices.Count > 0 ? selectedFondo : -1;
            FondoSelectorPanel.Visibility = fondoMatrices.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            switchingFondo = false;
            UpdateFrenteEditingEnabled();
        }

        /// <summary>Rebuild the per-gap separator textboxes (fondoCount-1 of them), preserving current values.</summary>
        private void RebuildSeparatorFields(int fondoCount)
        {
            var current = ReadSeparators();
            SeparatorsHost.Children.Clear();
            separatorBoxes.Clear();

            var gaps = Math.Max(0, fondoCount - 1);
            SeparatorsSection.Visibility = gaps > 0 ? Visibility.Visible : Visibility.Collapsed;
            for (var g = 0; g < gaps; g++)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
                row.Children.Add(new TextBlock
                {
                    Text = string.Format(CultureInfo.InvariantCulture, "Fondo {0}–{1}", g + 1, g + 2),
                    Width = 78,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                });
                var box = new TextBox { Width = 70, Height = 24, VerticalContentAlignment = VerticalAlignment.Center };
                row.Children.Add(box);
                separatorBoxes.Add(box);
                SeparatorsHost.Children.Add(row);
            }

            SetSeparatorValues(current);
        }

        /// <summary>Fill the separator textboxes from a value list, padding missing gaps with the SAME rule the drawing
        /// uses (<see cref="SelectiveDepthLayout.Separator"/>: reuse the last positive value, else the default) so a
        /// reopened rack shows — and re-saves — the gaps it was actually drawn with.</summary>
        private void SetSeparatorValues(IList<double> values)
        {
            for (var g = 0; g < separatorBoxes.Count; g++)
            {
                separatorBoxes[g].Text = SelectiveDepthLayout.Separator(values, g).ToString("0.###", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Read the per-gap separator textboxes (invalid/blank → the default).</summary>
        private List<double> ReadSeparators()
        {
            var result = new List<double>();
            for (var g = 0; g < separatorBoxes.Count; g++)
            {
                var box = separatorBoxes[g];
                if (UiSupport.TryNum(box.Text, out var v) && v > 0.0)
                {
                    result.Add(v);
                }
                else
                {
                    // Don't silently swallow a typo: fall back to the default, SAY so, and resync the box so what the
                    // user sees is what the drawing will use.
                    result.Add(SelectiveRackDefaults.DefaultSeparator);
                    if (!string.IsNullOrWhiteSpace(box.Text))
                    {
                        pendingWarning = "Separación " + (g + 1).ToString(CultureInfo.InvariantCulture) + " inválida; se usa la default.";
                        box.Text = SelectiveRackDefaults.DefaultSeparator.ToString("0.###", CultureInfo.InvariantCulture);
                    }
                }
            }

            return result;
        }

        /// <summary>Read "Número de fondos", resize the fondo list (new fondos clone fondo 0), rebuild the combo + separators.</summary>
        private void ApplyFondoCountFromBox()
        {
            // Commit the working matrix to its slot FIRST — the callers reload from the slots afterwards
            // (LoadFondo), so bailing out before this save would silently revert uncommitted matrix edits.
            SaveWorkingToSelected();
            if (fondoMatrices.Count == 0) fondoMatrices.Add(SnapshotWorking());

            // An invalid/blank count must NOT shrink the list — the old fallback to 1 silently DELETED the extra
            // fondos' level matrices before any validation could run. Keep the current count and say why.
            if (!UiSupport.TryNum(FondosBox.Text, out var f) || f < 1.0)
            {
                pendingWarning = "Número de fondos inválido (mínimo 1); se conserva el actual.";
                FondosBox.Text = Math.Max(1, fondoMatrices.Count).ToString(CultureInfo.InvariantCulture);
                return;
            }

            var n = Math.Min(SelectiveRackDefaults.MaxDepthCount, (int)Math.Round(f));

            while (fondoMatrices.Count < n) fondoMatrices.Add(CloneAligned(fondoMatrices[0], fondoMatrices[0].Bays.Count, fondoMatrices[0]));
            while (fondoMatrices.Count > n) fondoMatrices.RemoveAt(fondoMatrices.Count - 1);
            if (selectedFondo >= fondoMatrices.Count) selectedFondo = 0;

            RebuildFondoSelector();
            RebuildSeparatorFields(n);
        }

        /// <summary>Frentes (bay count) are edited PER FONDO now: each line can have its own count (a corner layout).
        /// The longest fondo defines the shared grid, so overlapping frentes still align at their posts.</summary>
        private void UpdateFrenteEditingEnabled()
        {
            BayCountBox.IsEnabled = true;
            BayCountBox.ToolTip = "Número de frentes (bahías) de ESTE fondo. Cada fondo puede tener su propio número (p. ej. esquina); "
                + "el fondo más largo define la rejilla y los frentes que se traslapan alinean sus postes. Se aplica al salir del campo.";
        }

        private void FondoSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (switchingFondo || catalog == null) return;
            var target = FondoSelectorBox.SelectedIndex;
            if (target < 0 || target >= fondoMatrices.Count || target == selectedFondo) return;

            // Commit what's typed in the cell editor first (like SelectCell) — don't silently discard it.
            if (!TryCommitEditedCell(out _))
            {
                switchingFondo = true; // user kept an invalid value: revert the combo and stay on this fondo
                FondoSelectorBox.SelectedIndex = selectedFondo;
                switchingFondo = false;
                return;
            }

            using (DeferRecompute())
            {
                SaveWorkingToSelected();
                selectedFondo = target;
                LoadFondo(selectedFondo);
                UpdateFrenteEditingEnabled();
                BayCountBox.Text = bays.Count.ToString(CultureInfo.InvariantCulture);
                LoadCellEditor();
                RenderMatrix();
                Recompute();
            }
        }

        /// <summary>The "Fondo de tarima" is per-fondo now (it belongs to the selected fondo); recompute so the change lands.</summary>
        private void FondoDepth_LostFocus(object sender, RoutedEventArgs e)
        {
            if (catalog == null) return; // ignore the initial value set during InitializeComponent
            Recompute(); // BuildDesign -> SaveWorkingToSelected captures this fondo's depth into its slot
        }

        private void Fondos_LostFocus(object sender, RoutedEventArgs e)
        {
            if (catalog == null) return; // ignore the initial value set during InitializeComponent
            if (!TryCommitEditedCell(out _)) return; // don't discard typed cell input on a fondo-count change
            using (DeferRecompute())
            {
                ApplyFondoCountFromBox();  // may reset selectedFondo when the count shrinks
                LoadFondo(selectedFondo);  // always reload the working matrix for the (possibly new) selection
                // Resync the frente-count box + post selector to the reloaded fondo — a shrink can switch fondos, and a
                // stale BayCountBox would make the next 'Recalcular' resize the wrong fondo (matches the other handlers).
                BayCountBox.Text = bays.Count.ToString(CultureInfo.InvariantCulture);
                UpdateFrenteEditingEnabled();
                LoadCellEditor();
                RenderMatrix();
                RefreshPostSelect();
                Recompute();
            }
        }

        /// <summary>Grow/shrink the number of bays, preserving existing ones; a new bay clones the last (cells + floor flag).</summary>
        private void ResizeBays(int bayCount)
        {
            while (bays.Count < bayCount)
            {
                if (bays.Count > 0)
                {
                    bays.Add(bays[bays.Count - 1].Select(c => c.Clone()).ToList());
                    floorBeams.Add(floorBeams[floorBeams.Count - 1]);
                    bayHeights.Add(bayHeights[bayHeights.Count - 1]);
                    baySegments.Add(CloneSegments(baySegments[baySegments.Count - 1]));
                }
                else
                {
                    bays.Add(new List<Cell> { NewCell() });
                    floorBeams.Add(false);
                    bayHeights.Add(null);
                    baySegments.Add(new List<SelectiveSegment>());
                }
            }

            while (bays.Count > bayCount)
            {
                bays.RemoveAt(bays.Count - 1);
                baySegments.RemoveAt(baySegments.Count - 1);
                floorBeams.RemoveAt(floorBeams.Count - 1);
                bayHeights.RemoveAt(bayHeights.Count - 1);
            }

            ClampSelection();
        }

        private void AddLevel(int bay)
        {
            using (DeferRecompute()) // the rebuild can fire a height box LostFocus → coalesce its Recompute with ours
            {
                var column = bays[bay];
                column.Add(column.Count > 0 ? column[column.Count - 1].Clone() : NewCell());
                RenderMatrix();
                Recompute();
            }
        }

        private void RemoveLevel(int bay)
        {
            var column = bays[bay];
            if (column.Count <= 1)
            {
                SetStatus("Cada frente necesita al menos un nivel.", true);
                return;
            }

            using (DeferRecompute()) // the rebuild can fire a height box LostFocus → coalesce its Recompute with ours
            {
                column.RemoveAt(column.Count - 1);
                ClampSelection();
                LoadCellEditor();
                RenderMatrix();
                Recompute();
            }
        }

        private void SetFloor(int bay, bool value)
        {
            if (bay < 0 || bay >= floorBeams.Count) return;
            floorBeams[bay] = value;
            Recompute();
        }

        private void ClampSelection()
        {
            selBay = Math.Min(Math.Max(0, selBay), bays.Count - 1);
            var levelCount = selBay >= 0 && selBay < bays.Count ? bays[selBay].Count : 1;
            selLevel = Math.Min(Math.Max(0, selLevel), levelCount - 1);
        }

        private bool TryGetSelected(out Cell cell)
        {
            cell = null;
            if (selBay < 0 || selBay >= bays.Count) return false;
            var column = bays[selBay];
            if (selLevel < 0 || selLevel >= column.Count) return false;
            cell = column[selLevel];
            return true;
        }

        // ---- Matrix rendering ----

        private void RenderMatrix()
        {
            MatrixGrid.Children.Clear();
            MatrixGrid.RowDefinitions.Clear();
            MatrixGrid.ColumnDefinitions.Clear();
            cellBorders.Clear(); // repopulated below by CellUi — cleared FIRST so every early return stays consistent

            var bayCount = bays.Count;
            if (bayCount == 0) return;
            var maxLevels = bays.Max(c => c.Count);
            if (maxLevels == 0) return;

            MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
            for (var b = 0; b < bayCount; b++)
                MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });

            MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var r = 0; r < maxLevels; r++)
                MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddToGrid(HeaderCell(string.Empty), 0, 0);
            for (var b = 0; b < bayCount; b++)
                AddToGrid(BayHeader(b), 0, b + 1);

            // Top display row = highest level; shorter bays leave the upper rows empty (aligned to the floor).
            for (var displayRow = 0; displayRow < maxLevels; displayRow++)
            {
                var level = maxLevels - 1 - displayRow;
                var gridRow = displayRow + 1;
                AddToGrid(HeaderCell("Nivel " + (level + 1)), gridRow, 0);

                for (var b = 0; b < bayCount; b++)
                {
                    if (level < bays[b].Count)
                    {
                        AddToGrid(CellUi(bays[b][level], b, level), gridRow, b + 1);
                    }
                }
            }
        }

        private void AddToGrid(UIElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            MatrixGrid.Children.Add(element);
        }

        private static TextBlock HeaderCell(string text) => new TextBlock
        {
            Text = text,
            Foreground = LabelStroke,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2)
        };

        private UIElement BayHeader(int bay)
        {
            var panel = new StackPanel { Margin = new Thickness(2, 2, 2, 6) };
            panel.Children.Add(new TextBlock
            {
                Text = "Frente " + (bay + 1),
                Foreground = LabelStroke,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            var levelRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 0) };
            var minus = SmallButton("−");
            minus.ToolTip = "Quitar el nivel superior de este frente.";
            minus.Click += (s, e) => RemoveLevel(bay);
            var count = new TextBlock
            {
                Text = bays[bay].Count.ToString(CultureInfo.InvariantCulture),
                Foreground = CellText,
                FontSize = 11,
                MinWidth = 16,
                Margin = new Thickness(5, 0, 5, 0),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var plus = SmallButton("+");
            plus.ToolTip = "Agregar un nivel arriba (clona el último).";
            plus.Click += (s, e) => AddLevel(bay);
            levelRow.Children.Add(minus);
            levelRow.Children.Add(count);
            levelRow.Children.Add(plus);
            panel.Children.Add(levelRow);

            var floor = new CheckBox
            {
                Content = "Piso",
                FontSize = 10.5,
                Foreground = LabelStroke,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 3, 0, 0),
                IsChecked = floorBeams[bay],
                ToolTip = "Larguero a piso: el nivel de piso lleva larguero."
            };
            floor.Checked += (s, e) => SetFloor(bay, true);
            floor.Unchecked += (s, e) => SetFloor(bay, false);
            panel.Children.Add(floor);

            var heightRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 3, 0, 0) };
            heightRow.Children.Add(new TextBlock { Text = "Alto", Foreground = LabelStroke, FontSize = 10.5, Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            var heightBox = new TextBox
            {
                Width = 44,
                FontSize = 10.5,
                Text = bayHeights[bay].HasValue ? bayHeights[bay].Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty,
                ToolTip = "Altura del frente (in). Vacío = auto. El poste toma el frente más alto que toca."
            };
            heightBox.LostFocus += (s, e) => SetBayHeight(bay, heightBox.Text);
            // e.Handled: without it Enter ALSO fires the window's default button (double Recompute; the matrix
            // rebuild steals focus and any validation message is wiped instantly).
            heightBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) { SetBayHeight(bay, heightBox.Text); e.Handled = true; } };
            heightRow.Children.Add(heightBox);
            panel.Children.Add(heightRow);

            // "Medio frente" (N tramos): a button opens the tramos dialog. No tramos = normal full-width bay.
            var segCount = bay < baySegments.Count ? baySegments[bay].Count : 0;
            var tramosBtn = new Button
            {
                Content = segCount >= 2 ? "½fr: " + segCount + " tramos" : "Medio frente…",
                FontSize = 10.5,
                Margin = new Thickness(0, 3, 0, 0),
                Padding = new Thickness(6, 1, 6, 1),
                HorizontalAlignment = HorizontalAlignment.Center,
                ToolTip = "Medio frente: parte el frente en tramos con postes intermedios (el último se calcula). Sin tramos = frente completo."
            };
            tramosBtn.Click += (s, e) => EditTramos(bay);
            panel.Children.Add(tramosBtn);

            return panel;
        }

        private void SetBayHeight(int bay, string text)
        {
            if (bay < 0 || bay >= bayHeights.Count) return;
            if (!UiSupport.TryOptionalNum(text, out var value)) { SetStatus("Altura de frente inválida (vacío = auto).", true); return; }
            if (Nullable.Equals(bayHeights[bay], value)) return;
            bayHeights[bay] = value;
            Recompute();
        }

        /// <summary>Open the tramos ("medio frente" generalizado) editor for a frente and apply the result.</summary>
        private void EditTramos(int bay)
        {
            if (bay < 0 || bay >= baySegments.Count) return;

            // Best-effort full bay width (shared across fondos) so the dialog can show the calculated last tramo + warn.
            var fullWidth = lastSystem != null && bay < lastSystem.Bays.Count ? lastSystem.Bays[bay].BeamLength : 0.0;

            var dialog = new SelectiveSegmentsWindow(bay + 1, baySegments[bay], fullWidth) { Owner = this };
            if (dialog.ShowDialog() != true) return;

            baySegments[bay] = dialog.Result.Select(s => new SelectiveSegment { Length = s.Length, Loaded = s.Loaded }).ToList();
            RenderMatrix(); // refresh the button label (tramo count)
            Recompute();
        }

        // ---- Per-post cabeceras ----

        /// <summary>The largest frente count across all fondos (the master grid). Uses the LIVE working matrix for the
        /// selected fondo (its slot is stale mid-edit) and the saved slots for the rest.</summary>
        private int MaxFrenteCount()
        {
            var max = bays.Count;
            for (var k = 0; k < fondoMatrices.Count; k++)
            {
                if (k == selectedFondo) continue; // the working copy is live in `bays`; the slot is stale
                if (fondoMatrices[k].Bays.Count > max) max = fondoMatrices[k].Bays.Count;
            }

            return max;
        }

        /// <summary>Keep the per-post cabecera + peralte lists sized to the MASTER grid's posts (masterFrentes+1),
        /// preserving existing entries. Sizing to the LONGEST fondo (not the working one) means switching to a shorter
        /// fondo never truncates and loses fondo 0's custom cabeceras / per-post peraltes.</summary>
        private void SyncPostCabeceras()
        {
            var posts = MaxFrenteCount() + 1;
            while (postCabeceras.Count < posts) postCabeceras.Add(null);
            while (postCabeceras.Count > posts) postCabeceras.RemoveAt(postCabeceras.Count - 1);
            while (postPeraltes.Count < posts) postPeraltes.Add(0.0);
            while (postPeraltes.Count > posts) postPeraltes.RemoveAt(postPeraltes.Count - 1);
        }

        /// <summary>Fill the post selector with "Poste 1..N+1", preserving the selection, then refresh its status.</summary>
        private void RefreshPostSelect()
        {
            SyncPostCabeceras();
            var previous = PostSelectBox.SelectedIndex;
            var items = new List<string>();
            for (var i = 0; i < postCabeceras.Count; i++) items.Add("Poste " + (i + 1).ToString(CultureInfo.InvariantCulture));
            PostSelectBox.ItemsSource = items;
            PostSelectBox.SelectedIndex = previous >= 0 && previous < items.Count ? previous : (items.Count > 0 ? 0 : -1);
            UpdatePostStatus();
        }

        private void PostSelect_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdatePostStatus();
            ShowPostPeralteOverride();
            UpdatePostHighlight(); // re-highlight the picked post (in place — geometry did not change)
        }

        /// <summary>
        /// Move the picked-post highlight by restyling the cached post shapes instead of redrawing the whole
        /// canvas. Restyles EVERY post from the current SelectedIndex (≤21 brush writes), so it can never go
        /// stale after the combo's ItemsSource resets; falls back to the full <see cref="DrawPreview"/> (the old
        /// behavior) whenever the cache is empty or inconsistent. Geometry changes still redraw via Recompute.
        /// </summary>
        private void UpdatePostHighlight()
        {
            if (postRects.Count == 0 || postRects.Count != postLabels.Count)
            {
                DrawPreview();
                return;
            }

            var selected = PostSelectBox?.SelectedIndex ?? -1; // same source DrawPreview reads
            for (var i = 0; i < postRects.Count; i++)
            {
                StylePost(postRects[i], postLabels[i], i == selected);
            }
        }

        /// <summary>Highlight styling of a preview post (rectangle + number) — single source of truth shared by
        /// <see cref="DrawPreview"/> and <see cref="UpdatePostHighlight"/>, so the two paths cannot diverge.</summary>
        private static void StylePost(Rectangle rect, TextBlock number, bool highlighted)
        {
            if (rect != null)
            {
                rect.Stroke = highlighted ? PostHiBrush : PostBrush;
                rect.StrokeThickness = highlighted ? 3.0 : 1.6;
                rect.Fill = highlighted ? PostHiFill : PostFill;
            }

            if (number != null)
            {
                number.Foreground = highlighted ? PostHiBrush : LabelStroke;
            }
        }

        /// <summary>Show the selected post's peralte override in its box (empty when the post inherits the global).</summary>
        private void ShowPostPeralteOverride()
        {
            if (PostPeralteOverrideBox == null) return;
            var i = PostSelectBox.SelectedIndex;
            var over = i >= 0 && i < postPeraltes.Count ? postPeraltes[i] : 0.0;
            PostPeralteOverrideBox.Text = over > 0.0 ? over.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>"Dibujar placa base" changes the drawn geometry, so recompute the preview. (The numbering/name
        /// toggles only persist for now — their text drawing is a future pipeline — so they need no handler.)</summary>
        private void DrawToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (catalog == null) return; // ignore the initial IsChecked set during InitializeComponent
            Recompute();
        }

        /// <summary>Store the per-post peralte override for the selected post; empty (or = global) means inherit.</summary>
        private void PostPeralteOverride_LostFocus(object sender, RoutedEventArgs e) => CommitPostPeralteOverride();

        /// <summary>Enter commits the override too (same as the per-bay height box); e.Handled keeps the key
        /// from bubbling further up the window.</summary>
        private void PostPeralteOverride_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            CommitPostPeralteOverride();
            e.Handled = true;
        }

        private void CommitPostPeralteOverride()
        {
            var i = PostSelectBox.SelectedIndex;
            if (i < 0 || i >= postPeraltes.Count) return;

            var text = PostPeralteOverrideBox.Text;
            double value;
            if (string.IsNullOrWhiteSpace(text))
            {
                value = 0.0; // empty → inherit the global peralte
            }
            else if (!UiSupport.TryNum(text, out value) || value <= 0.0)
            {
                SetStatus("Peralte de poste inválido (deja vacío para usar el peralte global).", true);
                ShowPostPeralteOverride();
                return;
            }

            // A value equal to the global is just the default → store as inherit (0) so it tracks the global if it changes.
            if (value > 0.0 && UiSupport.TryNum(PostPeralteBox.Text, out var global) && Math.Abs(value - global) < 1e-6)
            {
                value = 0.0;
            }

            postPeraltes[i] = value;
            ShowPostPeralteOverride();
            Recompute();
        }

        private void UpdatePostStatus()
        {
            if (PostCabeceraStatus == null) return;
            var i = PostSelectBox.SelectedIndex;
            var custom = i >= 0 && i < postCabeceras.Count && postCabeceras[i] != null;
            PostCabeceraStatus.Text = i < 0 ? string.Empty : (custom ? "Personalizada" : "Por defecto (del tramo)");
        }

        private void CustomizePost_Click(object sender, RoutedEventArgs e)
        {
            var i = PostSelectBox.SelectedIndex;
            if (i < 0 || i >= postCabeceras.Count) return;

            // Seed with the RESOLVED cabecera for THIS post: height = the post's resolved height (tallest adjacent
            // frente), fondo = the tramo's fondo (shared by every cabecera). So "Personalizar" opens the cabecera that
            // is actually in use (e.g. 312 in / 48 in), not a generic 132/42. A custom one keeps its structural edits.
            var resolvedHeight = ResolvedPostHeight(i);
            var fondo = ResolvedFondo();

            // Work on a CLONE and compare before/after: closing the configurator without editing is a real
            // CANCEL (before, the seed was mutated up-front and any close marked the post "Personalizada").
            var seed = postCabeceras[i] != null ? CloneCabecera(postCabeceras[i]) : BuildStandardPostCabecera(resolvedHeight, fondo);
            if (seed == null) return;
            if (resolvedHeight > 0.0) seed.Height = resolvedHeight;
            if (fondo > 0.0) seed.Depth = fondo;

            // Seed the cabecera's post peralte with THIS post's effective value (its override, else the global) so the
            // configurator shows/edits it; the write-back below keeps the selective's PostPeraltes the source of truth.
            var globalPeralte = UiSupport.TryNum(PostPeralteBox.Text, out var gp) && gp > 0.0 ? gp : 0.0;
            seed.PostPeralte = (i < postPeraltes.Count && postPeraltes[i] > 0.0) ? postPeraltes[i] : globalPeralte;

            var store = new RackProjectStore();
            var before = store.Serialize(RackProject.ForSelective(seed));

            var window = new RackFrameConfiguratorWindow(seed, canInsertInAutoCad: false) { Owner = this };
            window.ShowDialog();

            var cfg = window.Configuration;
            if (cfg == null || store.Serialize(RackProject.ForSelective(cfg)) == before)
            {
                // Nothing was edited: leave the post exactly as it was (default stays default).
                UpdatePostStatus();
                return;
            }

            // Fondo is locked to the tramo — every cabecera of the rack shares it.
            if (fondo > 0.0) cfg.Depth = fondo;

            // Height comes from the system; the user MAY override it, but warn it can desynchronize the rack
            // (the frontal largueros are placed for the resolved height). The SEVERE case is when the cabecera ends
            // up BELOW the top load level: the top larguero/pallet would stick out above the post — flag it specially.
            var topLevelY = TopLevelYAtPost(i);
            if (topLevelY > 0.0 && cfg.Height < topLevelY - 0.5)
            {
                MessageBox.Show(
                    this,
                    "La cabecera del poste (" + cfg.Height.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in) queda MÁS BAJA que el nivel de carga superior (" + topLevelY.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in).\n\nEl larguero/tarima superior sobresaldría por encima del poste. Sube la altura de la cabecera "
                        + "o revisa los niveles de las bahías vecinas.",
                    "Cabecera demasiado baja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else if (resolvedHeight > 0.0 && Math.Abs(cfg.Height - resolvedHeight) > 0.5)
            {
                MessageBox.Show(
                    this,
                    "La altura de la cabecera (" + cfg.Height.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in) difiere del alto resuelto del poste (" + resolvedHeight.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in).\n\nEl sistema se puede desconfigurar: el frontal coloca los largueros para el alto resuelto, "
                        + "así que el corte lateral y el frontal pueden dejar de coincidir.",
                    "Altura de cabecera",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // Sync the post peralte edited in the cabecera back to the selective's per-post source of truth (0 = global,
            // so it keeps tracking the global peralte). The frontal/planta read PostPeraltes, so this avoids divergence.
            if (i < postPeraltes.Count)
            {
                var edited = cfg.PostPeralte;
                postPeraltes[i] = (edited > 0.0 && Math.Abs(edited - globalPeralte) > 1e-6) ? edited : 0.0;
                ShowPostPeralteOverride();
            }

            postCabeceras[i] = cfg;
            UpdatePostStatus();
            Recompute();
        }

        /// <summary>Deep-clone a cabecera via the project store round-trip (the same serialization RACKEDITAR uses).</summary>
        private static RackFrameConfiguration CloneCabecera(RackFrameConfiguration configuration)
        {
            var store = new RackProjectStore();
            return store.Deserialize(store.Serialize(RackProject.ForSelective(configuration)))?.Header;
        }

        /// <summary>The resolved height of post <paramref name="i"/> (tallest adjacent frente); falls back to the run height.</summary>
        private double ResolvedPostHeight(int i)
        {
            if (lastSystem == null) return 0.0;
            var height = SelectivePostGeometry.PostHeight(lastSystem, i);
            return height > 0.0 ? height : lastSystem.Height;
        }

        /// <summary>The Y of the topmost load level touching post <paramref name="i"/> (max over its adjacent bays); 0 if none.</summary>
        private double TopLevelYAtPost(int i)
        {
            if (lastSystem == null) return 0.0;

            var top = 0.0;
            void Consider(int bayIndex)
            {
                if (bayIndex < 0 || bayIndex >= lastSystem.Bays.Count) return;
                foreach (var level in lastSystem.Bays[bayIndex].Levels)
                {
                    if (level.Y > top) top = level.Y;
                }
            }

            Consider(i - 1); // bay to the left of the post
            Consider(i);     // bay to the right
            return top;
        }

        /// <summary>The CABECERA fondo of fondo 0: the per-line "Fondo de cabecera" override when set, else the rule
        /// (cabecera = tarima − 6"). This is what a per-post custom cabecera is drawn at; its fondo is not set
        /// independently, so we coerce it to this value. Delegates to <see cref="SelectiveDepthLayout.CabeceraDepthOfFondo"/>
        /// (the single home of override→rule→fallback) so "Personalizar" matches the drawn geometry.</summary>
        private double ResolvedFondo()
        {
            if (lastSystem != null)
            {
                var cabecera = SelectiveDepthLayout.CabeceraDepthOfFondo(lastSystem, 0);
                if (cabecera > 0.0) return cabecera;
            }

            var pallet = SelectiveRackDefaults.DefaultPalletDepth;
            var derived = pallet - SelectiveRackDefaults.CabeceraFondoAllowance;
            return derived > 0.0 ? derived : pallet;
        }

        /// <summary>Build a standard cabecera at the given height/fondo using the run's post; the seed when a post has no custom one.</summary>
        private RackFrameConfiguration BuildStandardPostCabecera(double height, double fondo)
        {
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();
            var postId = PostBox.SelectedValue as string;
            if (string.IsNullOrWhiteSpace(postId)) postId = lastSystem?.PostId;
            return new RackFrameConfigurationFactory(catalog).Build(
                template, postId,
                height > 0.0 ? height : template.DefaultHeight,
                fondo > 0.0 ? fondo : SelectiveRackDefaults.DefaultPalletDepth);
        }

        /// <summary>Open the safety-accessories dialog (catalog elements × quantity); store the selection for the BOM.</summary>
        private void Safety_Click(object sender, RoutedEventArgs e)
        {
            // The tope grid needs the matrix dimensions (levels per frente) of fondo 0 (the main matrix).
            var levelsPerFrente = bays.Select(b => b.Count).ToList();
            var dialog = new SelectiveSafetyWindow(catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>(), safetySelections, MaxFrenteCount() + 1, levelsPerFrente) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            safetySelections.Clear();
            safetySelections.AddRange(dialog.Result.Select(CopySafety));
            UpdateSafetyButton();
        }

        /// <summary>A deep copy of a safety selection, carrying its per-post side overrides + the tope grid config.</summary>
        private static SelectiveSafetySelection CopySafety(SelectiveSafetySelection s)
        {
            var copy = new SelectiveSafetySelection { ElementId = s.ElementId, Quantity = s.Quantity, Side = s.Side, TopeShared = s.TopeShared, TopeSaque = s.TopeSaque, TopeFrontal = s.TopeFrontal };
            foreach (var post in s.PostSides)
            {
                if (post != null) copy.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = post.Side });
            }

            foreach (var cell in s.TopeOffCells)
            {
                if (cell != null) copy.TopeOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            }

            return copy;
        }

        /// <summary>A selection contributes to the drawing/BOM: it has a quantity, a default drawn side, or any post override that draws.</summary>
        private static bool SafetyDraws(SelectiveSafetySelection s)
            => s != null && (s.Quantity > 0 || s.Side != SafetySide.None || s.PostSides.Any(p => p != null && p.Side != SafetySide.None));

        /// <summary>Reflect the number of chosen safety accessories on the button label.</summary>
        private void UpdateSafetyButton()
        {
            var count = safetySelections.Count(SafetyDraws);
            SafetyButton.Content = count > 0
                ? "Elementos de seguridad (" + count.ToString(CultureInfo.InvariantCulture) + ")…"
                : "Elementos de seguridad…";
        }

        private void ResetPost_Click(object sender, RoutedEventArgs e)
        {
            var i = PostSelectBox.SelectedIndex;
            if (i < 0 || i >= postCabeceras.Count) return;
            postCabeceras[i] = null;
            if (i < postPeraltes.Count) postPeraltes[i] = 0.0; // back to the global peralte too
            UpdatePostStatus();
            ShowPostPeralteOverride();
            Recompute();
        }

        private static Button SmallButton(string text) => new Button
        {
            Content = text,
            Width = 20,
            Height = 18,
            Padding = new Thickness(0),
            FontSize = 12,
            Cursor = Cursors.Hand
        };

        private UIElement CellUi(Cell cell, int bay, int level)
        {
            var border = new Border
            {
                Margin = new Thickness(2),
                Padding = new Thickness(6, 4, 6, 4),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    FontSize = 11,
                    Foreground = CellText,
                    TextAlignment = TextAlignment.Center
                }
            };

            StyleCellBorder(border, bay == selBay && level == selLevel);
            RefreshCellVisual(border, cell);

            border.MouseLeftButtonUp += (s, e) => SelectCell(bay, level);
            cellBorders[(bay, level)] = border;
            return border;
        }

        /// <summary>The terse matrix-cell text ("40×60 / ×2 · P3 ✎"). SINGLE source of truth shared by the full
        /// rebuild (<see cref="CellUi"/>) and the partial refresh paths, so they cannot drift apart.</summary>
        private static string CellLabel(Cell cell)
            => string.Format(CultureInfo.InvariantCulture, "{0:0.#}×{1:0.#}\n×{2} · P{3:0.#}{4}",
                cell.Frente, cell.Alto, cell.PalletCount, cell.BeamPeralte, cell.HasOverride ? " ✎" : string.Empty);

        /// <summary>Long-form reading of the terse cell text for new users — same single-source rule as <see cref="CellLabel"/>.</summary>
        private static string CellToolTip(Cell cell)
            => string.Format(
                CultureInfo.InvariantCulture,
                "Tarima {0:0.#}×{1:0.#} in · {2} tarima(s) por nivel · peralte de larguero {3:0.#} in{4}",
                cell.Frente, cell.Alto, cell.PalletCount, cell.BeamPeralte,
                cell.HasOverride ? " · con ajustes manuales (✎)" : string.Empty);

        /// <summary>Selection styling of a matrix cell — single source for <see cref="CellUi"/> and <see cref="SelectCell"/>.</summary>
        private static void StyleCellBorder(Border border, bool selected)
        {
            border.Background = selected ? CellSelFill : Brushes.White;
            border.BorderBrush = selected ? CellSelStroke : CellStroke;
            border.BorderThickness = new Thickness(selected ? 2 : 1);
        }

        /// <summary>Refresh a cell's text AND tooltip from its data (the tooltip reads the same values, so a
        /// partial refresh must update both or the hover text would go stale).</summary>
        private static void RefreshCellVisual(Border border, Cell cell)
        {
            if (border.Child is TextBlock text) text.Text = CellLabel(cell);
            border.ToolTip = CellToolTip(cell);
        }

        /// <summary>
        /// The full matrix rebuild used to DESTROY a focused bay-height box, firing its LostFocus commit
        /// (SetBayHeight → Recompute) within the same gesture. The partial-refresh paths keep the box alive, so
        /// they call this where RenderMatrix used to run: moving focus to the window fires the exact same commit.
        /// </summary>
        private void CommitFocusedMatrixTextBox()
        {
            if (Keyboard.FocusedElement is TextBox box && MatrixGrid.IsAncestorOf(box))
            {
                Focus();
            }
        }

        private void SelectCell(int bay, int level)
        {
            using (DeferRecompute())
            {
                // Don't silently discard what was typed in the current cell: apply it if valid+changed, or ask before
                // discarding if it is invalid (returning false keeps the user on the current cell).
                if (!TryCommitEditedCell(out var applied)) return;

                var prevBay = selBay;
                var prevLevel = selLevel;
                selBay = bay;
                selLevel = level;
                LoadCellEditor();

                // Same effect the full rebuild had here: a focused bay-height box commits (LostFocus) before the
                // visuals change; its Recompute coalesces with the one below.
                CommitFocusedMatrixTextBox();

                // A click only changes the selection visuals (and, when applied, the OLD cell's committed text):
                // restyle those two cells in place instead of rebuilding ~400 elements. Structure never changes here.
                if (cellBorders.TryGetValue((prevBay, prevLevel), out var previous)
                    && cellBorders.TryGetValue((bay, level), out var current))
                {
                    StyleCellBorder(previous, selected: false);
                    StyleCellBorder(current, selected: true);
                    if (applied && prevBay < bays.Count && prevLevel < bays[prevBay].Count)
                    {
                        RefreshCellVisual(previous, bays[prevBay][prevLevel]); // the commit wrote into the OLD cell
                    }
                }
                else
                {
                    RenderMatrix(); // defensive fallback: cache out of sync → old full-rebuild behavior
                }

                if (applied) Recompute();
            }
        }

        /// <summary>Commit the cell editor into the currently-selected matrix cell before moving away. Returns false only
        /// when the editor is invalid and the user chooses to stay (cancel the switch); <paramref name="applied"/> is true
        /// when a real change was written (so the caller recomputes).</summary>
        private bool TryCommitEditedCell(out bool applied)
        {
            applied = false;
            if (loadingCell || !TryGetSelected(out var current)) return true;

            if (!ReadCellEditor(out var edited, out var error))
            {
                var choice = MessageBox.Show(
                    this,
                    "La celda actual tiene un valor inválido:\n" + error + "\n\n¿Descartar lo tecleado y continuar?",
                    "Cambios sin aplicar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                return choice == MessageBoxResult.Yes; // Yes = discard & continue; No = stay on this cell
            }

            if (!CellEquals(current, edited))
            {
                current.CopyFrom(edited);
                applied = true;
            }

            return true;
        }

        private static bool CellEquals(Cell a, Cell b)
            => NearEq(a.Frente, b.Frente)
               && NearEq(a.Alto, b.Alto)
               && a.PalletCount == b.PalletCount
               && string.Equals(a.BeamId, b.BeamId, StringComparison.OrdinalIgnoreCase)
               && NearEq(a.BeamPeralte, b.BeamPeralte)
               && NearEq(a.BeamLength, b.BeamLength)
               && NearEq(a.Clear, b.Clear);

        private static bool NearEq(double a, double b) => Math.Abs(a - b) < 1e-6;

        private static bool NearEq(double? a, double? b)
            => a.HasValue == b.HasValue && (!a.HasValue || NearEq(a.Value, b.Value));

        private void LoadCellEditor()
        {
            if (!TryGetSelected(out var cell)) return;

            loadingCell = true;
            CellHeader.Text = string.Format(CultureInfo.InvariantCulture, "Celda: Frente {0} · Nivel {1}", selBay + 1, selLevel + 1);
            FrenteBox.Text = cell.Frente.ToString("0.###", CultureInfo.InvariantCulture);
            AltoBox.Text = cell.Alto.ToString("0.###", CultureInfo.InvariantCulture);
            PalletCountBox.Text = cell.PalletCount.ToString(CultureInfo.InvariantCulture);
            BeamLenBox.Text = cell.BeamLength.HasValue ? cell.BeamLength.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
            ClearBox.Text = cell.Clear.HasValue ? cell.Clear.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
            CellBeamBox.SelectedValue = cell.BeamId;
            RefreshPeralteCombo(cell.BeamPeralte);
            loadingCell = false;
        }

        /// <summary>The beam changed in the cell editor: repopulate the allowed peraltes, keeping the current one if it still fits.</summary>
        private void OnBeamChanged()
        {
            if (loadingCell) return;
            var current = BeamPeralteCombo.SelectedItem as string;
            RefreshPeralteCombo(UiSupport.TryNum(current, out var v) ? v : (double?)null);
        }

        /// <summary>Fill the peralte combo with the selected larguero's allowed values; select <paramref name="keep"/> if present, else the first.</summary>
        private void RefreshPeralteCombo(double? keep)
        {
            var options = PeralteOptions(CellBeamBox.SelectedValue as string);
            BeamPeralteCombo.ItemsSource = options;

            var target = keep.HasValue ? keep.Value.ToString("0.###", CultureInfo.InvariantCulture) : null;
            if (target != null && options.Contains(target)) BeamPeralteCombo.SelectedItem = target;
            else if (options.Count > 0) BeamPeralteCombo.SelectedIndex = 0;
        }

        /// <summary>Allowed PERALTE values declared by a larguero (parsed by <see cref="PeralteList"/>), formatted for display.</summary>
        private List<string> PeralteOptions(string beamId)
        {
            var raw = catalog?.BeamProfiles.FirstOrDefault(b => string.Equals(b?.Id, beamId, StringComparison.OrdinalIgnoreCase))?.Peraltes;
            return PeralteList.Parse(raw)
                .Select(value => value.ToString("0.###", CultureInfo.InvariantCulture))
                .ToList();
        }

        // ---- Events ----

        private void Update_Click(object sender, RoutedEventArgs e) => ApplyBayCount();

        /// <summary>Apply the "Frentes" count to THIS fondo's matrix (resize bays), then reconcile fondos/posts and
        /// recompute. Shared by the explicit "Recalcular tramo" button and the live BayCountBox commit (LostFocus/Enter)
        /// so both paths behave identically. Frentes are per-fondo (a corner layout); the resolver aligns overlapping
        /// widths to the longest fondo.</summary>
        private void ApplyBayCount()
        {
            if (!initialized) return;

            if (!TryInt(BayCountBox.Text, out var bayCount) || bayCount < 1)
            {
                // Keep the current count (don't wipe the matrix on a typo) and say why, latched so Recompute shows it.
                pendingWarning = "Cantidad de frentes inválida (mínimo 1); se conserva la actual.";
                BayCountBox.Text = bays.Count.ToString(CultureInfo.InvariantCulture);
                Recompute();
                return;
            }

            if (bayCount == bays.Count && !TryCellEditorDiffersFromSelected())
            {
                return; // no structural change and nothing typed pending — avoid a redundant rebuild on tab-out
            }

            if (!TryCommitEditedCell(out _)) return; // don't discard typed cell input on a frente-count change

            using (DeferRecompute()) // the rebuild can fire a height box LostFocus → coalesce its Recompute with ours
            {
                ResizeBays(bayCount);
                ApplyFondoCountFromBox();      // apply "Número de fondos" (rebuild combo + separators; may reset selectedFondo)
                LoadFondo(selectedFondo);      // reload the working matrix for the (possibly new) selected fondo
                BayCountBox.Text = bays.Count.ToString(CultureInfo.InvariantCulture);
                LoadCellEditor();
                RenderMatrix();
                RefreshPostSelect();
                Recompute();
            }
        }

        /// <summary>The global tramo scalars (poste peralte, tolerancia, holgura, elevación) live-apply on leave-field
        /// now — same pattern the "Fondo de tarima"/height boxes already used — so the preview never lags behind a typed
        /// value and the user isn't left guessing which field needs "Recalcular tramo".</summary>
        private void GlobalScalar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;
            Recompute();
        }

        /// <summary>Enter also commits a global scalar (it doesn't move focus, so LostFocus wouldn't fire); e.Handled
        /// keeps the key from bubbling to the window.</summary>
        private void GlobalScalar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || !initialized) return;
            Recompute();
            e.Handled = true;
        }

        /// <summary>Changing the poste profile re-scales the frontal/planta (post width), so live-apply it too.</summary>
        private void Post_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!initialized) return;
            Recompute();
        }

        /// <summary>The dimension-detail combo. Cotas don't render in the schematic WPF preview (only in AutoCAD), but
        /// recompute so <c>lastSystem.Dimensions</c> is fresh for the next draw and the status reflects the change.</summary>
        private void Dimensions_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!initialized) return;
            Recompute();
        }

        /// <summary>The "(Automático)" entry: the current DIMSTYLE sized to the annotation scale (no named style).</summary>
        private const string AutoDimStyle = "(Automático)";

        /// <summary>Fill the dimension-style combo with the drawing's styles (called by the plugin, which has the
        /// document). Keeps "(Automático)" first and preserves the current selection when it still exists.</summary>
        public void SetDimensionStyles(IEnumerable<string> styleNames)
        {
            var previous = DimStyleBox.SelectedItem as string;
            DimStyleBox.Items.Clear();
            DimStyleBox.Items.Add(AutoDimStyle);
            if (styleNames != null)
            {
                foreach (var name in styleNames)
                {
                    if (!string.IsNullOrWhiteSpace(name) && !DimStyleBox.Items.Contains(name.Trim()))
                    {
                        DimStyleBox.Items.Add(name.Trim());
                    }
                }
            }

            DimStyleBox.SelectedItem = previous != null && DimStyleBox.Items.Contains(previous) ? previous : AutoDimStyle;
        }

        /// <summary>The chosen dimension style name, or null when "(Automático)".</summary>
        private string SelectedDimStyle()
        {
            var name = DimStyleBox.SelectedItem as string;
            return string.IsNullOrEmpty(name) || name == AutoDimStyle ? null : name;
        }

        /// <summary>Select a saved style in the combo; add it if the current drawing doesn't have it (so it round-trips),
        /// and fall back to "(Automático)" when none.</summary>
        private void SelectDimStyle(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                DimStyleBox.SelectedItem = AutoDimStyle;
                return;
            }

            var trimmed = name.Trim();
            if (!DimStyleBox.Items.Contains(trimmed)) DimStyleBox.Items.Add(trimmed);
            DimStyleBox.SelectedItem = trimmed;
        }

        private void BayCount_LostFocus(object sender, RoutedEventArgs e) => ApplyBayCount();

        private void BayCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            ApplyBayCount();
            e.Handled = true;
        }

        /// <summary>True when the cell editor holds a valid value that differs from the selected cell (a pending, unapplied
        /// edit). Used to avoid a redundant rebuild when nothing changed, and by the pre-draw guard.</summary>
        private bool TryCellEditorDiffersFromSelected()
        {
            if (loadingCell || !TryGetSelected(out var current)) return false;
            return ReadCellEditor(out var edited, out _) && !CellEquals(current, edited);
        }

        private void ApplyCell_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Cell);
        private void ApplyRow_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Row);
        private void ApplyColumn_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Column);
        private void ApplyAll_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.All);
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowBom_Click(object sender, RoutedEventArgs e)
        {
            if (lastInstances == null || lastSystem == null)
            {
                SetStatus("Genera primero la geometría (revisa tarima/niveles).", true);
                return;
            }

            var bom = SelectiveBomBuilder.Build(lastSystem, catalog);
            new RackBomWindow(bom) { Owner = this }.ShowDialog();
        }

        private void ApplyScope(Scope scope)
        {
            if (!ReadCellEditor(out var values, out var error))
            {
                SetStatus(error, true);
                return;
            }

            var applied = 0;
            using (DeferRecompute())
            {
                var stale = false;
                for (var b = 0; b < bays.Count; b++)
                {
                    for (var l = 0; l < bays[b].Count; l++)
                    {
                        var inScope =
                            scope == Scope.All ||
                            (scope == Scope.Cell && b == selBay && l == selLevel) ||
                            (scope == Scope.Row && l == selLevel) ||
                            (scope == Scope.Column && b == selBay);

                        if (inScope)
                        {
                            bays[b][l].CopyFrom(values);
                            applied++;

                            // The scope rewrites cell VALUES, never the matrix shape: refresh the touched cells in place.
                            if (cellBorders.TryGetValue((b, l), out var border)) RefreshCellVisual(border, bays[b][l]);
                            else stale = true;
                        }
                    }
                }

                if (stale) RenderMatrix(); // defensive fallback: cache out of sync → old full-rebuild behavior
                CommitFocusedMatrixTextBox(); // the rebuild used to commit a focused bay-height box here; keep that
                Recompute();
            }

            // Recompute says a generic "Vista actualizada."; tell the user HOW MANY cells the scope touched
            // (only on success — an error status from Recompute must stay visible).
            if (lastSystem != null)
            {
                SetStatus(string.Format(CultureInfo.InvariantCulture, "Aplicado a {0} celda(s).", applied), false);
            }
        }

        private void InsertFrontal_Click(object sender, RoutedEventArgs e) => RequestDraw(RackEmbedDocument.ViewFrontal, updateOnly: false);

        private void InsertLateral_Click(object sender, RoutedEventArgs e) => RequestDraw(RackEmbedDocument.ViewLateral, updateOnly: false);

        private void InsertPlanta_Click(object sender, RoutedEventArgs e) => RequestDraw(RackEmbedDocument.ViewPlanta, updateOnly: false);

        /// <summary>"Actualizar": redraw the rack's already-drawn views in place with the current edits, inserting nothing.</summary>
        private void UpdateExisting_Click(object sender, RoutedEventArgs e) => RequestDraw(view: null, updateOnly: true);

        /// <summary>
        /// Close the window asking AutoCAD to draw. <paramref name="updateOnly"/> = redraw existing views only (Actualizar);
        /// otherwise insert a new linked view-block of <paramref name="view"/> AND refresh the existing ones.
        /// </summary>
        private void RequestDraw(string view, bool updateOnly)
        {
            if (!canInsertInAutoCad)
            {
                SetStatus("El dibujo en AutoCAD solo está disponible cuando el selectivo se abre desde AutoCAD.", true);
                return;
            }

            // Updating, and adding a linked lateral/planta, only make sense on an existing system (a new rack has no GUID
            // to link to yet): insert the frontal first, then add the rest via RACKEDITAR.
            if (!isEditingExisting && (updateOnly || view == RackEmbedDocument.ViewLateral || view == RackEmbedDocument.ViewPlanta))
            {
                MessageBox.Show(
                    this,
                    "Primero inserta la vista frontal. Luego selecciónala con RACKEDITAR y desde ahí actualiza o agrega "
                        + "las demás vistas: así quedan ligadas al sistema (mismo GUID).",
                    updateOnly ? "Actualizar" : "Vista " + view,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // The cell editor's "Aplicar a:" is manual (it carries a scope choice), so the user can type new cell
            // values and hit Actualizar/Insertar without applying them. Ask before drawing the OLD values instead of
            // silently losing the edit.
            if (!ConfirmPendingCellEdits()) return;

            var system = BuildSystem(out var design, out var error);
            if (system == null)
            {
                SetStatus(error, true);
                return;
            }

            currentName = NameBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(currentId)) currentId = Guid.NewGuid().ToString();

            InsertRequested = true;
            UpdateOnly = updateOnly;
            InsertView = updateOnly ? null : view;
            SystemToInsert = system;
            DesignToInsert = design;
            RackId = currentId;
            RackName = currentName;
            Close();
        }

        /// <summary>
        /// Before drawing (Actualizar/Insertar), catch cell-editor values the user typed but never applied with
        /// "Aplicar a:". Returns true to proceed, false to cancel and stay in the editor. On a valid pending edit it
        /// offers Aplicar (Sí) / Actualizar sin aplicar (No) / Cancelar; on an invalid one, ignore-and-draw or stay.
        /// (Global scalars already committed via their LostFocus when the button took focus, so only the manual cell
        /// editor can be pending here.)
        /// </summary>
        private bool ConfirmPendingCellEdits()
        {
            if (loadingCell || !TryGetSelected(out var current)) return true;

            var cellRef = string.Format(CultureInfo.InvariantCulture, "Frente {0} · Nivel {1}", selBay + 1, selLevel + 1);

            if (!ReadCellEditor(out var edited, out var error))
            {
                var invalid = MessageBox.Show(
                    this,
                    "El editor de celda (" + cellRef + ") tiene un valor sin aplicar que además es inválido:\n" + error
                        + "\n\n¿Dibujar de todos modos, ignorando ese cambio?\n\n"
                        + "«Sí»: actualiza el dibujo y descarta lo tecleado.\n"
                        + "«No»: vuelve al editor para corregirlo.",
                    "Cambios sin aplicar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                return invalid == MessageBoxResult.Yes;
            }

            if (CellEquals(current, edited)) return true; // nothing pending — proceed

            var choice = MessageBox.Show(
                this,
                "Tienes cambios sin aplicar en la celda seleccionada (" + cellRef + ").\n\n"
                    + "«Sí»: aplícalos a esa celda y actualiza el dibujo.\n"
                    + "«No»: actualiza el dibujo SIN esos cambios (se descartan).\n"
                    + "«Cancelar»: vuelve al editor sin dibujar.",
                "Cambios sin aplicar",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (choice == MessageBoxResult.Cancel) return false;
            if (choice == MessageBoxResult.Yes)
            {
                current.CopyFrom(edited);
                if (cellBorders.TryGetValue((selBay, selLevel), out var border)) RefreshCellVisual(border, current);
                Recompute(); // so the just-applied value is what BuildSystem reads next
            }

            return true; // Yes (applied) or No (discard) → proceed to draw
        }

        /// <summary>
        /// Coalesce every <see cref="Recompute"/> issued while the returned scope is open into AT MOST ONE run when
        /// the outermost scope closes — still synchronous, inside the same gesture, so lastSystem/lastInstances and
        /// the status are already fresh for any follow-up reader (Ver BOM, Personalizar poste, RequestDraw). This
        /// collapses the double pipeline of composite gestures (e.g. a matrix click whose focus move first commits a
        /// pending bay height via LostFocus). ALWAYS dispose via <c>using</c>: TryCommitEditedCell has early returns
        /// and can pump a nested message loop (MessageBox), and a leaked depth would freeze the preview forever.
        /// </summary>
        private IDisposable DeferRecompute() => new RecomputeDeferral(this);

        private sealed class RecomputeDeferral : IDisposable
        {
            private RackSelectiveWindow owner;

            public RecomputeDeferral(RackSelectiveWindow owner)
            {
                this.owner = owner;
                owner.recomputeDeferDepth++;
            }

            public void Dispose()
            {
                var window = owner;
                if (window == null) return; // tolerate double-dispose
                owner = null;
                window.recomputeDeferDepth--;
                if (window.recomputeDeferDepth > 0 || !window.recomputePending) return;
                window.recomputePending = false;
                window.Recompute();
            }
        }

        private void Recompute()
        {
            if (recomputeDeferDepth > 0)
            {
                recomputePending = true; // latched: the enclosing DeferRecompute scope runs one pass on close
                return;
            }

            var system = BuildSystem(out var error);
            if (system == null)
            {
                lastSystem = null;
                lastInstances = null;
                SummaryText.Text = string.Empty;
                PreviewCanvas.Children.Clear();
                postRects.Clear();
                postLabels.Clear();
                pendingWarning = null; // the validation error supersedes any latched input warning
                SetStatus(error, true);
                return;
            }

            lastSystem = system;
            // The frontal preview shows the fondo being edited (each fondo has its own levels); fondo 0 is the default.
            lastInstances = builder.Build(SelectiveDepthLayout.FondoSystemView(system, selectedFondo), catalog);
            UpdateSummary();
            if (pendingWarning != null)
            {
                SetStatus(pendingWarning, true); // the view DID update, but with a kept-previous/default fallback — say so
                pendingWarning = null;
            }
            else
            {
                SetStatus(selectedFondo == 0 ? "Vista actualizada." : "Vista actualizada (Fondo " + (selectedFondo + 1).ToString(CultureInfo.InvariantCulture) + ").", false);
            }

            DrawPreview();
        }

        // ---- Reading inputs ----

        private bool ReadCellEditor(out Cell values, out string error)
        {
            values = null;
            error = null;
            if (!(CellBeamBox.SelectedValue is string beamId) || string.IsNullOrWhiteSpace(beamId)) { error = "Selecciona un larguero."; return false; }
            if (!UiSupport.TryNum(FrenteBox.Text, out var frente) || frente <= 0.0) { error = "Frente de tarima inválido."; return false; }
            if (!UiSupport.TryNum(AltoBox.Text, out var alto) || alto <= 0.0) { error = "Alto de tarima inválido."; return false; }
            if (!TryInt(PalletCountBox.Text, out var count) || count < 1) { error = "Tarimas por nivel inválido."; return false; }
            if (!(BeamPeralteCombo.SelectedItem is string peralteText) || !UiSupport.TryNum(peralteText, out var peralte) || peralte <= 0.0) { error = "Selecciona un peralte de larguero."; return false; }
            if (!UiSupport.TryOptionalNum(BeamLenBox.Text, out var beamLen)) { error = "Longitud de larguero inválida (deja vacío para auto)."; return false; }
            if (!UiSupport.TryOptionalNum(ClearBox.Text, out var clear)) { error = "Claro inválido (deja vacío para auto)."; return false; }

            values = new Cell { Frente = frente, Alto = alto, PalletCount = count, BeamId = beamId, BeamPeralte = peralte, BeamLength = beamLen, Clear = clear };
            return true;
        }

        /// <summary>Builds the pallet-driven design from the current editor state (globals + matrix), or null + error.</summary>
        private SelectivePalletDesign BuildDesign(out string error)
        {
            error = null;
            if (!(PostBox.SelectedValue is string postId) || string.IsNullOrWhiteSpace(postId)) { error = "Selecciona un poste."; return null; }
            if (!UiSupport.TryNum(PostPeralteBox.Text, out var postPeralte) || postPeralte <= 0.0) { error = "Peralte de poste inválido."; return null; }
            if (!UiSupport.TryNum(ToleranceBox.Text, out var tolerance) || tolerance < 0.0) { error = "Tolerancia horizontal inválida."; return null; }
            if (!UiSupport.TryNum(ClearanceBox.Text, out var clearance) || clearance < 0.0) { error = "Holgura vertical inválida."; return null; }
            if (!UiSupport.TryNum(FloorRiseBox.Text, out var floorRise) || floorRise < 0.0) { error = "Elevación de larguero a piso inválida."; return null; }
            if (!UiSupport.TryNum(FondoBox.Text, out var fondo) || fondo <= 0.0) { error = "Fondo de tarima inválido."; return null; }
            if (!UiSupport.TryNum(FondosBox.Text, out var fondosNum) || fondosNum < 1.0) { error = "Número de fondos inválido (mínimo 1)."; return null; }
            var depthCount = Math.Min(SelectiveRackDefaults.MaxDepthCount, Math.Max(1, (int)Math.Round(fondosNum)));

            // Commit the live matrix into its fondo slot, then read fondo 0 (the master frente grid) + the extra fondos.
            SaveWorkingToSelected();
            if (fondoMatrices.Count == 0) fondoMatrices.Add(SnapshotWorking());
            var fondo0 = fondoMatrices[0];
            if (fondo0.Bays.Count == 0 || fondo0.Bays[0].Count == 0) { error = "Define frentes y niveles."; return null; }

            var design = new SelectivePalletDesign
            {
                PostId = postId,
                PostPeralte = postPeralte,
                PalletTolerance = tolerance,
                VerticalClearance = clearance,
                FloorBeamRise = floorRise,
                PalletDepth = fondo0.Depth > 0.0 ? fondo0.Depth : fondo, // fondo 0's own depth
                DepthCount = depthCount
            };

            foreach (var separator in ReadSeparators())
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
            for (var k = 1; k < depthCount; k++)
            {
                var m = k < fondoMatrices.Count ? fondoMatrices[k] : fondo0;
                design.ExtraFondoBays.Add(BuildBayDesigns(m));
                design.ExtraFondoDepths.Add(m.Depth);
                design.CabeceraFondoOverrides.Add(m.CabeceraOverride);
            }

            SyncPostCabeceras();
            foreach (var cabecera in postCabeceras)
            {
                design.PostCabeceras.Add(cabecera);
            }

            foreach (var peralte in postPeraltes)
            {
                design.PostPeraltes.Add(peralte);
            }

            design.DrawBasePlate = DrawBasePlateCheck.IsChecked == true;
            design.NumberFronts = NumberFrontsCheck.IsChecked == true;
            design.NumberLevels = NumberLevelsCheck.IsChecked == true;
            design.DrawRackName = DrawRackNameCheck.IsChecked == true;
            design.DrawPallets = DrawPalletsCheck.IsChecked == true;
            design.AnnotationScale = UiSupport.TryNum(AnnotationScaleBox.Text, out var annScale) && annScale > 0.0 ? annScale : 1.0;
            design.Dimensions = (DimensionDetail)Math.Min((int)DimensionDetail.Detailed, Math.Max(0, DimensionsBox.SelectedIndex));
            design.DimensionStyle = SelectedDimStyle();
            foreach (var safety in safetySelections)
            {
                if (SafetyDraws(safety) && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    design.SafetySelections.Add(CopySafety(safety));
                }
            }

            return design;
        }

        private SelectiveRackSystem BuildSystem(out string error) => BuildSystem(out _, out error);

        /// <summary>Design + resolved system in one pass — RequestInsert and Recompute share this (no duplicated resolve/validation).</summary>
        private SelectiveRackSystem BuildSystem(out SelectivePalletDesign design, out string error)
        {
            design = BuildDesign(out error);
            if (design == null) return null;

            var system = resolver.Resolve(design, catalog);
            if (system.Height <= 0.0)
            {
                error = "No se pudo derivar la geometría (revisa tarima/niveles).";
                design = null;
                return null;
            }

            return system;
        }

        /// <summary>Empty frentes (columns) that the current load had to pad with a default cell (the matrix can't edit
        /// zero-level columns yet); &gt; 0 makes <see cref="LoadDesign"/> warn instead of silently converting them.</summary>
        private int paddedEmptyFrentesOnLoad;

        /// <summary>A warning latched by input-normalizing code (invalid fondo/cabecera/separador/conteo kept-previous
        /// fallbacks): the pipeline always ends in <see cref="Recompute"/>, whose final status would overwrite a direct
        /// SetStatus, so Recompute emits THIS instead of the generic success message when set.</summary>
        private string pendingWarning;

        /// <summary>Restore the whole editor (globals + matrix) from a saved design, then recompute.</summary>
        private void LoadDesign(SelectivePalletDesign design)
        {
            if (design == null || design.Bays.Count == 0) return;

            paddedEmptyFrentesOnLoad = 0;

            // Assigning the toggles below (e.g. DrawBasePlateCheck) fires DrawToggle_Changed → Recompute on the
            // half-loaded state; defer so the whole load runs exactly ONE pipeline (the explicit call at the end).
            using var deferral = DeferRecompute();

            PostBox.SelectedValue = design.PostId;
            if (PostBox.SelectedItem == null && PostBox.Items.Count > 0) PostBox.SelectedIndex = 0;
            PostPeralteBox.Text = design.PostPeralte.ToString("0.###", CultureInfo.InvariantCulture);
            ToleranceBox.Text = design.PalletTolerance.ToString("0.###", CultureInfo.InvariantCulture);
            ClearanceBox.Text = design.VerticalClearance.ToString("0.###", CultureInfo.InvariantCulture);
            FloorRiseBox.Text = design.FloorBeamRise.ToString("0.###", CultureInfo.InvariantCulture);
            FondoBox.Text = (design.PalletDepth > 0.0 ? design.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth).ToString("0.###", CultureInfo.InvariantCulture);
            FondosBox.Text = Math.Max(1, design.DepthCount).ToString(CultureInfo.InvariantCulture);

            // Rebuild every fondo's matrix: fondo 0 from Bays, the rest from ExtraFondoBays (or a clone of fondo 0).
            fondoMatrices.Clear();
            fondoMatrices.Add(FondoMatrixFromDesignBays(design.Bays));
            var depthCount = Math.Max(1, design.DepthCount);
            for (var k = 1; k < depthCount; k++)
            {
                var hasExtra = (k - 1) < design.ExtraFondoBays.Count && design.ExtraFondoBays[k - 1] != null && design.ExtraFondoBays[k - 1].Count > 0;
                fondoMatrices.Add(hasExtra
                    ? FondoMatrixFromDesignBays(design.ExtraFondoBays[k - 1])
                    : CloneAligned(fondoMatrices[0], fondoMatrices[0].Bays.Count, fondoMatrices[0]));
            }

            // Per-fondo depth: fondo 0 = PalletDepth; each extra fondo its own override, else fondo 0's.
            var baseDepth = design.PalletDepth > 0.0 ? design.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth;
            fondoMatrices[0].Depth = baseDepth;
            for (var k = 1; k < fondoMatrices.Count; k++)
            {
                var over = (k - 1) < design.ExtraFondoDepths.Count ? design.ExtraFondoDepths[k - 1] : 0.0;
                fondoMatrices[k].Depth = over > 0.0 ? over : baseDepth;
            }

            // Per-fondo custom cabecera fondo (0 = auto/derived).
            for (var k = 0; k < fondoMatrices.Count; k++)
            {
                fondoMatrices[k].CabeceraOverride = k < design.CabeceraFondoOverrides.Count ? design.CabeceraFondoOverrides[k] : 0.0;
            }

            selectedFondo = 0;
            RestoreWorkingFrom(fondoMatrices[0]);
            RebuildFondoSelector();
            RebuildSeparatorFields(depthCount);
            SetSeparatorValues(design.SeparatorLengths);

            postCabeceras.Clear();
            var loadedPallet = design.PalletDepth > 0.0 ? design.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth;
            // Fondo 0's custom "Fondo de cabecera" override wins over the rule (tarima − 6") — same precedence as
            // SelectiveDepthLayout.CabeceraDepthOfFondo, so a persisted custom cabecera keeps the override's depth.
            var loadedOverride = design.CabeceraFondoOverrides.Count > 0 ? design.CabeceraFondoOverrides[0] : 0.0;
            var loadedCabeceraFondo = loadedOverride > 0.0 ? loadedOverride : loadedPallet - SelectiveRackDefaults.CabeceraFondoAllowance;
            if (loadedCabeceraFondo <= 0.0) loadedCabeceraFondo = loadedPallet;
            foreach (var cabecera in design.PostCabeceras)
            {
                // A per-post cabecera's fondo obeys the rule (cabecera = tarima − 6"): coerce it on load so a
                // legacy/round-tripped design can't carry a stale/independently-set depth.
                if (cabecera != null) cabecera.Depth = loadedCabeceraFondo;
                postCabeceras.Add(cabecera);
            }

            postPeraltes.Clear();
            foreach (var peralte in design.PostPeraltes)
            {
                postPeraltes.Add(peralte);
            }

            DrawBasePlateCheck.IsChecked = design.DrawBasePlate;
            NumberFrontsCheck.IsChecked = design.NumberFronts;
            NumberLevelsCheck.IsChecked = design.NumberLevels;
            DrawRackNameCheck.IsChecked = design.DrawRackName;
            DrawPalletsCheck.IsChecked = design.DrawPallets;
            AnnotationScaleBox.Text = (design.AnnotationScale > 0.0 ? design.AnnotationScale : 1.0).ToString(CultureInfo.InvariantCulture);
            DimensionsBox.SelectedIndex = (int)design.Dimensions;
            SelectDimStyle(design.DimensionStyle);

            safetySelections.Clear();
            foreach (var safety in design.SafetySelections)
            {
                if (SafetyDraws(safety) && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    safetySelections.Add(CopySafety(safety));
                }
            }

            UpdateSafetyButton();

            BayCountBox.Text = bays.Count.ToString(CultureInfo.InvariantCulture);
            selBay = 0;
            selLevel = 0;
            ClampSelection();
            LoadCellEditor();
            RenderMatrix();
            RefreshPostSelect();

            if (paddedEmptyFrentesOnLoad > 0)
            {
                // Latched (not SetStatus): the method runs under DeferRecompute, so the REAL Recompute fires at the
                // using-scope exit and would overwrite a direct status; the latch makes it the FINAL message instead.
                pendingWarning = paddedEmptyFrentesOnLoad.ToString(CultureInfo.InvariantCulture)
                    + " frente(s) vacío(s) (columna) del diseño se cargaron con un nivel default — el editor aún no maneja columnas; revisa antes de redibujar.";
            }

            Recompute();
        }

        /// <summary>Open the editor pre-loaded with an existing rack (from an embedded/saved document), keeping its Id/Name.</summary>
        public void LoadExisting(SelectivePalletDesignDocument document)
        {
            if (document == null) return;
            currentId = document.Id;
            currentName = document.Name;
            isEditingExisting = true; // opened on an existing rack → "Actualizar" + linked lateral/planta become available
            UpdateInsertButtons();
            NameBox.Text = document.Name ?? string.Empty;
            LoadDesign(document.ToDomain());
        }

        /// <summary>Open pre-loaded from a LIBRARY template as a NEW rack — a fresh GUID on insert (not an in-place update),
        /// mirroring the dynamic editor's library open. Keeps the "Insertar" flow.</summary>
        public void LoadForNew(SelectivePalletDesignDocument document)
        {
            if (document == null) return;
            currentId = null;
            currentName = document.Name;
            isEditingExisting = false; // a library template inserts as its own rack, not an update of one in the drawing
            UpdateInsertButtons();
            NameBox.Text = document.Name ?? string.Empty;
            LoadDesign(document.ToDomain());
        }

        /// <summary>Save this selective design to the on-disk design library (a reusable <c>.rackcad.json</c>).</summary>
        private void SaveToLibrary_Click(object sender, RoutedEventArgs e)
        {
            var design = BuildDesign(out var error);
            if (design == null)
            {
                SetStatus(error ?? "Define frentes y niveles.", true);
                return;
            }

            var id = string.IsNullOrWhiteSpace(currentId) ? Guid.NewGuid().ToString() : currentId;
            var name = string.IsNullOrWhiteSpace(NameBox.Text) ? currentName : NameBox.Text.Trim();
            var document = SelectivePalletDesignDocument.From(design, id, name);

            var path = UiSupport.PromptSaveToLibrary(this, name, "selectivo");
            if (path == null) return;

            try
            {
                new RackProjectStore().Save(RackProject.ForSelectiveRack(document), path);
                SetStatus("Selectivo guardado en la biblioteca: " + System.IO.Path.GetFileName(path), false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo guardar: " + ex.Message, true);
            }
        }

        private void UpdateSummary()
        {
            var posts = lastInstances.Count(i => i.Role == HeaderBlockRole.Post);
            var beams = lastInstances.Count(i => i.Role == HeaderBlockRole.Beam);

            var bay0 = lastSystem.Bays.Count > 0 ? lastSystem.Bays[0] : null;
            var beamLength = bay0?.BeamLength ?? 0.0;
            var separation = bay0 != null && bay0.Levels.Count > 1 ? bay0.Levels[1].Y - bay0.Levels[0].Y : 0.0;

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0} frentes · {1} cabeceras · {2} largueros\nDerivado (frente 1): larguero {3:0.##}\" · sep. {4:0.##}\" · altura {5:0.##}\"",
                lastSystem.Bays.Count, posts, beams, beamLength, separation, lastSystem.Height);
        }

        // ---- Preview ----

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawPreview();

        private bool previewLateral;

        /// <summary>Toggle the preview between the frontal face and a schematic lateral (side) view.</summary>
        private void PreviewView_Changed(object sender, RoutedEventArgs e)
        {
            previewLateral = PreviewLateralRadio != null && PreviewLateralRadio.IsChecked == true;
            if (PreviewHint != null)
            {
                PreviewHint.Text = previewLateral
                    ? "Vista lateral (X = fondo, Y = altura). Cada fondo con su cabecera (tarima − 6) y sus largueros."
                    : "Vista frontal (X = ancho del tramo, Y = altura). Postes (cabeceras) + largueros por nivel.";
            }

            if (catalog != null) DrawPreview();
        }

        /// <summary>
        /// Schematic LATERAL preview: each fondo drawn as its cabecera (front + back post at its OWN depth and height)
        /// with a larguero mark at every level, stepped along X by the fondo offsets (separadores as gaps). Reuses the
        /// frontal preview's <see cref="Map"/> mapping so both share the same canvas helpers.
        /// </summary>
        private void DrawLateralPreview()
        {
            PreviewCanvas.Children.Clear();
            postRects.Clear();
            postLabels.Clear();

            if (lastSystem == null || lastSystem.Height <= 0.0)
            {
                return;
            }

            var offsets = SelectiveDepthLayout.Offsets(lastSystem);
            var fondoCount = offsets.Count;
            var postWidth = ProfileWidth(lastSystem.PostId);

            var depths = new double[fondoCount];
            var heights = new double[fondoCount];
            var levelYs = new List<double>[fondoCount];
            for (var k = 0; k < fondoCount; k++)
            {
                depths[k] = SelectiveDepthLayout.CabeceraDepthOfFondo(lastSystem, k);
                var bays = SelectiveDepthLayout.BaysOfFondo(lastSystem, k);
                var maxH = 0.0;
                var ys = new List<double>();
                foreach (var bay in bays)
                {
                    if (bay.Height > maxH) maxH = bay.Height;
                    foreach (var level in bay.Levels)
                    {
                        var y = Math.Round(level.Y, 4);
                        if (!ys.Contains(y)) ys.Add(y);
                    }
                }

                heights[k] = maxH > 0.0 ? maxH : lastSystem.Height;
                levelYs[k] = ys;
            }

            var xMin = -postWidth / 2.0;
            var xMax = offsets[fondoCount - 1] + depths[fondoCount - 1] + postWidth / 2.0;
            var totalWidth = Math.Max(1.0, xMax - xMin);
            var height = lastSystem.Height;

            var availableWidth = PreviewCanvas.ActualWidth;
            var availableHeight = PreviewCanvas.ActualHeight;
            if (availableWidth < 20 || availableHeight < 20)
            {
                return;
            }

            const double horizontalMargin = 46.0;
            const double topMargin = 26.0;
            const double bottomMargin = 40.0;
            var usableWidth = Math.Max(1.0, availableWidth - 2 * horizontalMargin);
            var usableHeight = Math.Max(1.0, availableHeight - topMargin - bottomMargin);
            mapScale = Math.Min(usableWidth / totalWidth, usableHeight / height);
            if (mapScale <= 0.0)
            {
                return;
            }

            mapMinX = xMin;
            var drawWidth = totalWidth * mapScale;
            var drawHeight = height * mapScale;
            mapOffsetX = (availableWidth - drawWidth) / 2.0;
            mapBottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;

            AddCanvasLabel(mapOffsetX, Math.Max(4.0, mapBottomY - drawHeight - 22.0),
                "Vista lateral · " + fondoCount.ToString(CultureInfo.InvariantCulture) + (fondoCount == 1 ? " fondo" : " fondos")
                    + " · fondo total " + totalWidth.ToString("0.##", CultureInfo.InvariantCulture) + " in",
                LabelStroke, 12, 360.0);

            AddLine(Map(xMin, 0), Map(xMax, 0), FloorStroke, 1.5); // floor

            for (var k = 0; k < fondoCount; k++)
            {
                var frontX = offsets[k];
                var backX = offsets[k] + depths[k];
                var h = heights[k];

                // Celosía (schematic): a top travesaño + a diagonal zigzag between the front and back posts, tied to the
                // level Ys (floor → level 1 → level 2 … → top, alternating sides). Drawn first so the posts sit on top.
                var sortedYs = new List<double>(levelYs[k]);
                sortedYs.Sort();
                AddLine(Map(frontX, h), Map(backX, h), CelosiaBrush, 1.3); // top travesaño
                var verts = new List<double> { 0.0 };
                verts.AddRange(sortedYs);
                if (verts[verts.Count - 1] < h - 1e-6) verts.Add(h);
                var prevPt = Map(frontX, verts[0]);
                for (var s = 1; s < verts.Count; s++)
                {
                    var pt = Map((s % 2) == 1 ? backX : frontX, verts[s]);
                    AddLine(prevPt, pt, CelosiaBrush, 1.0);
                    prevPt = pt;
                }

                var f = Map(frontX - postWidth / 2.0, h);
                AddRectangle(f.X, f.Y, postWidth * mapScale, h * mapScale, PostBrush, 1.6, PostFill);
                var b = Map(backX - postWidth / 2.0, h);
                AddRectangle(b.X, b.Y, postWidth * mapScale, h * mapScale, PostBrush, 1.6, PostFill);

                var beamW = Math.Max(4.0, postWidth * 1.4 * mapScale);
                var beamH = Math.Max(2.0, 3.0 * mapScale);
                foreach (var y in levelYs[k])
                {
                    var lf = Map(frontX - postWidth * 0.7, y + 1.5);
                    AddRectangle(lf.X, lf.Y, beamW, beamH, BeamBrush, 1.2, BeamFill);
                    var lb = Map(backX - postWidth * 0.7, y + 1.5);
                    AddRectangle(lb.X, lb.Y, beamW, beamH, BeamBrush, 1.2, BeamFill);
                }

                var mid = Map((frontX + backX) / 2.0, 0.0);
                AddCanvasLabel(mid.X - 12.0, mapBottomY + 8.0, "F" + (k + 1).ToString(CultureInfo.InvariantCulture), LabelStroke, 11, 40.0);
            }
        }

        private void DrawPreview()
        {
            if (previewLateral) { DrawLateralPreview(); return; }

            PreviewCanvas.Children.Clear();
            postRects.Clear();  // right after the canvas clear, so EVERY early return leaves cache+canvas consistent
            postLabels.Clear();

            if (lastInstances == null || lastSystem == null || lastSystem.Height <= 0.0)
            {
                return;
            }

            var postWidth = ProfileWidth(lastSystem.PostId);
            var height = lastSystem.Height;

            // ONE pass over the instances gathers the extents AND the per-instance draw data that the paint loop
            // below consumes (the extents and paint passes used to iterate + re-read the parameters twice).
            var xMin = -postWidth / 2.0;
            var xMax = xMin;
            var items = new List<(HeaderBlockRole Role, double X, double Y, double Size, double Peralte)>(lastInstances.Count);
            foreach (var instance in lastInstances)
            {
                switch (instance.Role)
                {
                    case HeaderBlockRole.Post:
                        var postH = Param(instance, "LONGITUD");
                        if (postH <= 0.0) postH = height;
                        xMax = Math.Max(xMax, instance.Insertion.X + postWidth / 2.0);
                        items.Add((instance.Role, instance.Insertion.X, instance.Insertion.Y, postH, 0.0));
                        break;
                    case HeaderBlockRole.Beam:
                        var length = Param(instance, "LONGITUD");
                        xMax = Math.Max(xMax, instance.Insertion.X + length);
                        items.Add((instance.Role, instance.Insertion.X, instance.Insertion.Y, length, Param(instance, "PERALTE")));
                        break;
                    case HeaderBlockRole.BasePlate:
                        items.Add((instance.Role, instance.ConnectionAnchor.X, 0.0, 0.0, 0.0));
                        break;
                    case HeaderBlockRole.Pallet:
                        var palFrente = Param(instance, SelectiveRackDefaults.PalletFrenteParam);
                        var palAlto = Param(instance, SelectiveRackDefaults.PalletAltoParam);
                        xMax = Math.Max(xMax, instance.Insertion.X + palFrente / 2.0); // centre origin
                        items.Add((instance.Role, instance.Insertion.X, instance.Insertion.Y, palFrente, palAlto));
                        break;
                }
            }

            var totalWidth = Math.Max(1.0, xMax - xMin);
            var availableWidth = PreviewCanvas.ActualWidth;
            var availableHeight = PreviewCanvas.ActualHeight;
            if (availableWidth < 20 || availableHeight < 20)
            {
                return;
            }

            const double horizontalMargin = 46.0;
            const double topMargin = 26.0;
            const double bottomMargin = 40.0;
            var usableWidth = Math.Max(1.0, availableWidth - 2 * horizontalMargin);
            var usableHeight = Math.Max(1.0, availableHeight - topMargin - bottomMargin);
            mapScale = Math.Min(usableWidth / totalWidth, usableHeight / height);
            if (mapScale <= 0.0)
            {
                return;
            }

            mapMinX = xMin;
            var drawWidth = totalWidth * mapScale;
            var drawHeight = height * mapScale;
            mapOffsetX = (availableWidth - drawWidth) / 2.0;
            mapBottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;

            AddCanvasLabel(mapOffsetX, Math.Max(4.0, mapBottomY - drawHeight - 22.0),
                "Ancho " + totalWidth.ToString("0.##", CultureInfo.InvariantCulture) + " in  ·  altura " + height.ToString("0.##", CultureInfo.InvariantCulture) + " in",
                LabelStroke, 12, 320.0);

            // Floor.
            AddLine(Map(xMin, 0), Map(xMax, 0), FloorStroke, 1.5);

            var selectedPost = PostSelectBox?.SelectedIndex ?? -1;
            var postIndex = 0;
            // The frontal builder emits the SHARED grid posts first and appends medio-frente INTERMEDIATE posts after
            // them. Only the shared ones get a number/cache entry — numbering intermediates would desync the preview
            // from "Cabecera por poste" and the "insertar lateral" prompt (which only know shared posts).
            var sharedPosts = SelectiveDepthLayout.BaysOfFondo(lastSystem, selectedFondo).Count + 1;

            foreach (var item in items)
            {
                switch (item.Role)
                {
                    case HeaderBlockRole.Post:
                        // Drawn with the base style; StylePost applies the highlight, so this path and
                        // UpdatePostHighlight share one styling source and cannot diverge.
                        var pTop = Map(item.X - postWidth / 2.0, item.Size);
                        var rect = AddRectangle(pTop.X, pTop.Y, postWidth * mapScale, item.Size * mapScale, PostBrush, 1.6, PostFill);
                        if (postIndex >= sharedPosts)
                        {
                            break; // intermediate (medio frente): visible, but unnumbered and outside the post cache
                        }

                        // Post number under the base (1-based) — matches "Cabecera por poste" and the "insertar lateral" prompt.
                        var numAt = Map(item.X, 0.0);
                        var number = AddPostNumber(numAt.X, mapBottomY + 8.0, (postIndex + 1).ToString(CultureInfo.InvariantCulture));
                        postRects.Add(rect); // even a null rect is added so indexes stay aligned with the post numbers
                        postLabels.Add(number);
                        StylePost(rect, number, postIndex == selectedPost);
                        postIndex++;
                        break;
                    case HeaderBlockRole.Beam:
                        var bTop = Map(item.X, item.Y + item.Peralte / 2.0);
                        AddRectangle(bTop.X, bTop.Y, item.Size * mapScale, Math.Max(2.0, item.Peralte * mapScale), BeamBrush, 1.2, BeamFill);
                        break;
                    case HeaderBlockRole.BasePlate:
                        var plate = Map(item.X - postWidth * 0.7, 0);
                        AddRectangle(plate.X, plate.Y, postWidth * 1.4 * mapScale, Math.Max(3.0, 0.3 * mapScale + 4.0), PlateFill, 1.0, PlateFill);
                        break;
                    case HeaderBlockRole.Pallet:
                        // Visual reference only: outlined box centred on Insertion (the block's CENTRE origin); item.Size =
                        // frente, item.Peralte = alto. Painted after the beams so a tarima sits on its larguero.
                        var palTop = Map(item.X - item.Size / 2.0, item.Y + item.Peralte / 2.0);
                        AddRectangle(palTop.X, palTop.Y, item.Size * mapScale, Math.Max(2.0, item.Peralte * mapScale), PalletBrush, 1.0, PalletFill);
                        break;
                }
            }
        }

        private double ProfileWidth(string postId)
        {
            var width = catalog?.PostProfiles.FirstOrDefault(p => string.Equals(p?.Id, postId, StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : 3.0;
        }

        private static double Param(HeaderBlockInstance instance, string name)
            => instance.DynamicParameters.TryGetValue(name, out var value) ? value : 0.0;

        private Point Map(double x, double y) => new Point(mapOffsetX + (x - mapMinX) * mapScale, mapBottomY - y * mapScale);

        private PreviewCanvasPainter previewPainter;
        private PreviewCanvasPainter Painter => previewPainter ??= new PreviewCanvasPainter(PreviewCanvas);

        private void AddLine(Point a, Point b, Brush stroke, double thickness)
            => Painter.AddLine(a, b, stroke, thickness);

        private Rectangle AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, Brush fill)
            => Painter.AddRectangle(left, top, width, height, stroke, thickness, dash: null, fill: fill);

        /// <summary>A post's 1-based number, centered under its base; <see cref="StylePost"/> applies the highlight.</summary>
        private TextBlock AddPostNumber(double centerX, double top, string text)
        {
            var label = new TextBlock
            {
                Text = text,
                Foreground = LabelStroke,
                FontSize = 12.5,
                FontWeight = FontWeights.Bold,
                Width = 24.0,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(label, centerX - 12.0);
            Canvas.SetTop(label, top);
            PreviewCanvas.Children.Add(label);
            return label;
        }

        private void AddCanvasLabel(double left, double top, string text, Brush brush, double size, double maxWidth)
        {
            var label = new TextBlock
            {
                Text = text, Foreground = brush, FontSize = size,
                FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis
            };
            if (maxWidth > 0.0) label.Width = maxWidth;
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            PreviewCanvas.Children.Add(label);
        }

        private void SetStatus(string message, bool isError)
        {
            UiSupport.SetStatus(StatusText, message, isError);

            // StatusText lives at the BOTTOM of the left panel's ScrollViewer: scroll an error into view or the
            // user (positioned further up, e.g. clicking the bottom action row) never sees the red message.
            if (isError) StatusText.BringIntoView();
        }

        private static bool TryInt(string text, out int value)
            => int.TryParse((text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
}
