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
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.RackFrames;
// I-20: the selective editor's state and its Cell/FondoMatrix/Scope models moved to RackCad.Application.Systems;
// these aliases keep the window's render/edit code reading the same short names while the state lives in Application.
using Cell = RackCad.Application.Systems.Selective.SelectiveEditorCell;
using FondoMatrix = RackCad.Application.Systems.Selective.SelectiveEditorFondoMatrix;
using Scope = RackCad.Application.Systems.Selective.SelectiveApplyScope;

namespace RackCad.UI.Systems.Selective
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
        /// <summary>Stroke of a cell that is part of the multi-selection but is NOT the primary (I-43): the same
        /// softer-outline distinction the dinamico draws, so one glance separates "the editor is bound to this"
        /// from "this will also be written".</summary>
        private static readonly Brush CellMultiStroke = UiSupport.FrozenBrush(Color.FromRgb(0x93, 0xB4, 0xF5));

        private readonly RackCatalog catalog;
        private readonly SelectiveFrontalBuilder builder = new SelectiveFrontalBuilder();
        private readonly SelectiveGeometryResolver resolver = new SelectiveGeometryResolver();
        private readonly bool canInsertInAutoCad;

        /// <summary>The pure editor state (initiative I-20): the working matrix, the per-fondo matrices, the selection
        /// and the per-post cabeceras/peraltes, together with their operations (snapshot/restore, save/load fondo,
        /// resize, add/remove level, ApplyScope, BuildDesign…). The window OBSERVES it through the accessors below and
        /// DELEGATES the operations to it; the painting (matrix + previews), the cell editor and the events stay here.</summary>
        private readonly SelectiveEditorState state = new SelectiveEditorState();

        /// <summary>The working design matrix: <c>bays[bay][level]</c>, level 0 = ground; each bay has its own length.
        /// A view over <see cref="SelectiveEditorState.Bays"/> so the existing render/edit code reads and mutates the
        /// state in place (I-20).</summary>
        private List<List<Cell>> bays => state.Bays;

        /// <summary>Per-bay "larguero a piso" flag, parallel to <see cref="bays"/> (view over the state).</summary>
        private List<bool> floorBeams => state.FloorBeams;

        /// <summary>Per-bay manual height override (in); null = auto. Parallel to <see cref="bays"/> (view over the state).</summary>
        private List<double?> bayHeights => state.BayHeights;

        /// <summary>Per-bay "medio frente" tramos (N tramos, the last calculated); empty = normal full-width bay. Parallel to <see cref="bays"/> (view over the state).</summary>
        private List<List<SelectiveSegment>> baySegments => state.BaySegments;

        /// <summary>Safety accessories chosen for this rack (id + quantity), for the BOM. Edited via the "Elementos de
        /// seguridad" dialog; drawing them is a future phase (needs their AutoCAD blocks). Stays in the window — safety
        /// ownership is I-22.</summary>
        private readonly List<SelectiveSafetySelection> safetySelections = new List<SelectiveSafetySelection>();

        /// <summary>Optional per-post cabecera (frame); one entry per post (N frentes → N+1 posts), null = run default (view over the state).</summary>
        private List<RackFrameConfiguration> postCabeceras => state.PostCabeceras;
        private List<double> postPeraltes => state.PostPeraltes; // per-post PERALTE override; 0 = inherit the global (view over the state)

        /// <summary>
        /// One saved level matrix per fondo (doble profundidad: each back-to-back side edits its OWN levels). Entry
        /// <see cref="selectedFondo"/> is stale WHILE editing — the live <see cref="bays"/>/<see cref="floorBeams"/>/
        /// <see cref="bayHeights"/> are that fondo's working copy; <see cref="SaveWorkingToSelected"/> commits them
        /// back before switching, building or resizing. Fondo 0 defines the shared frente count (view over the state).
        /// </summary>
        private List<FondoMatrix> fondoMatrices => state.FondoMatrices;
        private int selectedFondo { get => state.SelectedFondo; set => state.SelectedFondo = value; }
        private bool switchingFondo; // guards FondoSelector_Changed while the combo is repopulated

        /// <summary>The dynamic per-gap separator textboxes (one per hueco between consecutive fondos).</summary>
        private readonly List<TextBox> separatorBoxes = new List<TextBox>();

        private int selBay { get => state.SelBay; set => state.SelBay = value; }
        private int selLevel { get => state.SelLevel; set => state.SelLevel = value; }
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

        /// <summary>The shared editor session (I-15): the catalog, the rack identity (GUID + name), the coalesced
        /// recompute (its <see cref="RecomputeGate"/> replaces the old defer-depth/pending fields, running
        /// <see cref="RunRecompute"/>) and the insert/update contract. The editor-specific state — the fondo matrices and
        /// the design assembly (<c>BuildDesign</c>) — now lives in <see cref="state"/> (<see cref="SelectiveEditorState"/>,
        /// extracted in I-20); the window observes it and keeps the catalog-bound resolve/preview (<c>BuildSystem</c>).</summary>
        private readonly RackEditorSession<SelectivePalletDesign, SelectiveRackSystem> session;

        /// <summary>The library project this design was opened from, if any, so a re-save preserves the WRAPPER
        /// RackProjectDocument's unknown JSON metadata + non-downgraded schema version (I-11). Null for a brand-new design
        /// or the in-place drawing edit (whose inner design is a SelectivePalletDesignDocument, not a wrapper boundary).</summary>
        private RackProject sourceProject;

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

        /// <summary>Insert/update contract, backed by the shared session (I-15).</summary>
        public bool InsertRequested => session.InsertRequested;

        public SelectiveRackSystem SystemToInsert => (session.InsertionRequest as SelectiveInsertionRequest)?.System;

        /// <summary>The design that produced <see cref="SystemToInsert"/> — embedded in the drawing for round-trip editing.</summary>
        public SelectivePalletDesign DesignToInsert => (session.InsertionRequest as SelectiveInsertionRequest)?.Design;

        /// <summary>Stable id of the inserted rack (fresh GUID for a new rack, preserved when re-editing).</summary>
        public string RackId => session.Identity.Id;

        /// <summary>Client-facing name of the inserted rack (may be empty).</summary>
        public string RackName => session.Identity.Name;

        /// <summary>Which view the user asked to insert ("frontal"/"lateral"/"planta"); null when only updating.</summary>
        public string InsertView => session.InsertView;

        /// <summary>True when the user chose "Actualizar" (redraw existing views in place, insert nothing).</summary>
        public bool UpdateOnly => session.UpdateOnly;

        /// <summary>Test seam (I-15): confirms the window carries identity, coalesced recompute and insert through the session.</summary>
        internal RackEditorSession<SelectivePalletDesign, SelectiveRackSystem> Session => session;

        /// <summary>Test seam (I-20): builds the pallet design from the current editor state exactly as the insert/update
        /// path does (same <see cref="BuildDesign"/>), so a characterization test can lock the resolved geometry across the
        /// state extraction. Not used in production — the window builds through <see cref="BuildSystem(out string)"/>.</summary>
        internal SelectivePalletDesign BuildDesignForTest(out string error) => BuildDesign(out error);

        public RackSelectiveWindow()
            : this(false)
        {
        }

        public RackSelectiveWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            // The shared session owns the catalog, the identity, the coalesced recompute (its gate runs RunRecompute) and
            // the insert contract (I-15). Created before InitializeComponent so the catalog is ready for the combos below.
            session = new RackEditorSession<SelectivePalletDesign, SelectiveRackSystem>(recompute: RunRecompute);
            InitializeComponent();
            updateButtonTip = UpdateButton.ToolTip;
            insertLateralTip = InsertLateralButton.ToolTip;
            insertPlantaTip = InsertPlantaButton.ToolTip;
            catalog = session.Catalog;

            PostBox.ItemsSource = UiSupport.ToOptions(catalog?.PostProfiles);
            PostBox.SelectedValue = catalog?.Defaults?.Post;
            if (PostBox.SelectedItem == null && PostBox.Items.Count > 0) PostBox.SelectedIndex = 0;

            CellBeamBox.ItemsSource = UiSupport.ToOptions(catalog?.BeamProfiles);
            if (CellBeamBox.Items.Count > 0) CellBeamBox.SelectedIndex = 0;
            state.DefaultBeamId = CellBeamBox.SelectedValue as string; // the beam a fresh cell adopts (I-20)
            CellBeamBox.SelectionChanged += (s, e) => OnBeamChanged();

            state.InitMatrix(2, 4);
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

        // ---- Matrix model (state + operations extracted to SelectiveEditorState, I-20) ----
        //
        // Cell / FondoMatrix / Scope are now RackCad.Application.Systems types (aliased at the top of the file); the
        // matrix, the per-fondo matrices, the selection and the operations live in `state`. The window keeps the
        // WPF-bound wrappers below (they read the fondo/cabecera boxes with the keep-previous fallback and sync them
        // back) plus the render/edit code that observes and mutates the state through the accessors above.

        /// <summary>Read the working fondo's depth + cabecera override from the boxes, with the editor's keep-previous
        /// fallback: invalid text keeps the fondo's PREVIOUSLY SAVED value (not the global default) and latches a
        /// warning, so a typo while switching fondos doesn't silently reset this line; blank cabecera stays auto (0).</summary>
        private (double Depth, double CabeceraOverride) ReadWorkingDepthCabecera()
        {
            var previous = selectedFondo >= 0 && selectedFondo < fondoMatrices.Count ? fondoMatrices[selectedFondo] : null;

            double depth;
            if (UiSupport.TryNum(FondoBox.Text, out var d) && d > 0.0) depth = d;
            else
            {
                depth = previous != null && previous.Depth > 0.0 ? previous.Depth : SelectiveRackDefaults.DefaultPalletDepth;
                if (!string.IsNullOrWhiteSpace(FondoBox.Text)) pendingWarning = "Fondo de tarima inválido; se conserva el anterior.";
            }

            double cabecera;
            if (string.IsNullOrWhiteSpace(CabeceraFondoBox.Text)) cabecera = 0.0; // blank = auto (rule tarima − 6)
            else if (UiSupport.TryNum(CabeceraFondoBox.Text, out var co) && co > 0.0) cabecera = co;
            else
            {
                cabecera = previous?.CabeceraOverride ?? 0.0;
                pendingWarning = "Fondo de cabecera inválido (vacío = auto); se conserva el anterior.";
            }

            return (depth, cabecera);
        }

        // ---- Per-fondo matrices (doble profundidad: each fondo edits its own levels) ----

        /// <summary>Snapshot the live working matrix (the selected fondo) into a saveable copy, reading its fondo (depth)/cabecera boxes.</summary>
        private FondoMatrix SnapshotWorking()
        {
            var (depth, cabecera) = ReadWorkingDepthCabecera();
            return state.SnapshotWorking(depth, cabecera);
        }

        /// <summary>Load a saved fondo matrix into the live working matrix (state deep-clones it), and sync its fondo/cabecera boxes.</summary>
        private void RestoreWorkingFrom(FondoMatrix snap)
        {
            state.RestoreWorkingFrom(snap);
            FondoBox.Text = (snap.Depth > 0.0 ? snap.Depth : SelectiveRackDefaults.DefaultPalletDepth).ToString("0.###", CultureInfo.InvariantCulture);
            CabeceraFondoBox.Text = snap.CabeceraOverride > 0.0 ? snap.CabeceraOverride.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>Commit the live working matrix back into its fondo slot (reading its boxes) before switching/building/resizing.</summary>
        private void SaveWorkingToSelected()
        {
            var (depth, cabecera) = ReadWorkingDepthCabecera();
            state.SaveWorkingToSelected(depth, cabecera);
        }

        /// <summary>
        /// Commit the live MATRIX of the visible fondo while KEEPING the depths its slot already holds.
        /// <para>
        /// The ordinary commit reads <c>FondoBox</c>/<c>CabeceraFondoBox</c>, which is right when the boxes describe
        /// the fondo they are about to be written into. It is wrong just before a fondo-wide edit aimed elsewhere: the
        /// text the user just typed would become the VISIBLE fondo's value even though that fondo is not a target.
        /// The matrix still has to be committed — the state writes the OTHER fondos through their slots — so this
        /// commits it with the depths already stored (I-43, gate 7).
        /// </para>
        /// </summary>
        private void SaveWorkingMatrixKeepingDepths()
        {
            var slot = selectedFondo >= 0 && selectedFondo < fondoMatrices.Count ? fondoMatrices[selectedFondo] : null;
            if (slot == null)
            {
                SaveWorkingToSelected();
                return;
            }

            state.SaveWorkingToSelected(slot.Depth, slot.CabeceraOverride);
        }

        /// <summary>Re-show the two fondo boxes from the VISIBLE fondo's real slot. The boxes always describe the fondo
        /// on screen, whatever fondos an edit actually landed on; this only touches the two texts, never the matrix.</summary>
        private void SyncFondoDepthBoxes()
        {
            var slot = selectedFondo >= 0 && selectedFondo < fondoMatrices.Count ? fondoMatrices[selectedFondo] : null;
            if (slot == null) return;

            FondoBox.Text = (slot.Depth > 0.0 ? slot.Depth : SelectiveRackDefaults.DefaultPalletDepth).ToString("0.###", CultureInfo.InvariantCulture);
            CabeceraFondoBox.Text = slot.CabeceraOverride > 0.0 ? slot.CabeceraOverride.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>A copy of <paramref name="source"/> resized to <paramref name="bayCount"/> frentes (delegates to the state:
        /// a new frente clones <paramref name="widthSeed"/>'s column at that index; extra bays are dropped).</summary>
        private FondoMatrix CloneAligned(FondoMatrix source, int bayCount, FondoMatrix widthSeed)
            => state.CloneAligned(source, bayCount, widthSeed);

        /// <summary>Load fondo <paramref name="k"/> into the working matrix (via <see cref="RestoreWorkingFrom"/>, which
        /// syncs its boxes). Each fondo keeps its OWN frente count (a corner layout); the resolver aligns the overlapping
        /// widths to the longest fondo, so nothing is forced here.</summary>
        private void LoadFondo(int k) => RestoreWorkingFrom(fondoMatrices[k]);

        /// <summary>Turn saved design bays into a fondo matrix (state), accumulating the padded-empty-frente count so the load warns.</summary>
        private FondoMatrix FondoMatrixFromDesignBays(IList<SelectiveBayDesign> designBays)
        {
            var m = state.FondoMatrixFromDesignBays(designBays, out var padded);
            paddedEmptyFrentesOnLoad += padded;
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
            // The fondo count just changed: drop targets that no longer exist (never leaving the set empty) and show
            // the surviving set (I-43).
            state.SyncTargetFondos();
            RefreshTargetFondos();
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
                // SelectFondo, not the raw setter: when the targets were exactly the fondo being left they follow to
                // the new one, so an editor nobody has retargeted keeps applying to the fondo on screen (I-43).
                state.SelectFondo(target);
                LoadFondo(selectedFondo);
                RefreshTargetFondos();
                UpdateFrenteEditingEnabled();
                BayCountBox.Text = bays.Count.ToString(CultureInfo.InvariantCulture);
                LoadCellEditor();
                RenderMatrix();
                UpdatePostStatus(); // "Personalizada/Por defecto" is about the VISIBLE fondo (I-43)
                Recompute();
            }
        }

        /// <summary>
        /// "Fondo de tarima" and "Fondo de cabecera" are authorities of a FONDO, and they now land on every target
        /// fondo (I-43, gate 7). There is no inner scope for them: a depth belongs to a whole fondo.
        /// <para>
        /// The boxes keep showing the VISIBLE fondo; <c>TargetFondos</c> only decides where the edit lands. The custom
        /// cabeceras of each touched fondo adopt their new effective depth immediately, through the gate-4 authority.
        /// </para>
        /// </summary>
        private void FondoDepth_LostFocus(object sender, RoutedEventArgs e)
        {
            if (catalog == null || !initialized) return; // ignore the initial values set during InitializeComponent

            using (DeferRecompute())
            {
                // Commit the live MATRIX keeping the slot's depths: the typed text is an edit aimed at TargetFondos,
                // not a statement about the fondo on screen (which may not even be a target).
                SaveWorkingMatrixKeepingDepths();

                var isCabecera = ReferenceEquals(sender, CabeceraFondoBox);
                SelectiveFondoApplyResult result;
                if (isCabecera)
                {
                    var text = CabeceraFondoBox.Text;
                    double? over = null;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (!UiSupport.TryNum(text, out var typed) || typed <= 0.0)
                        {
                            SetStatus("Fondo de cabecera inválido (vacío = derivado de la tarima).", true);
                            return;
                        }

                        over = typed;
                    }

                    result = state.ApplyCabeceraDepthToTargets(over);
                    if (state.TargetFondos.Count > 1) pendingWarning = result.Describe("el fondo de cabecera", restore: !over.HasValue);
                }
                else
                {
                    if (!UiSupport.TryNum(FondoBox.Text, out var depth) || depth <= 0.0)
                    {
                        SetStatus("Fondo de tarima inválido.", true);
                        return;
                    }

                    result = state.ApplyPalletDepthToTargets(depth);
                    if (state.TargetFondos.Count > 1) pendingWarning = result.Describe("el fondo de tarima", restore: false);
                }

                // The boxes describe the VISIBLE fondo, so they go back to ITS values — otherwise the next commit (or
                // BuildDesign, which reads them again) would re-introduce the typed value into a fondo that was never
                // a target.
                SyncFondoDepthBoxes();
                Recompute();
            }
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

        /// <summary>Grow/shrink the number of bays (delegates to the state: a new bay clones the last — cells + floor flag + height + tramos).</summary>
        private void ResizeBays(int bayCount) => state.ResizeBays(bayCount);

        private void AddLevel(int bay)
        {
            using (DeferRecompute()) // the rebuild can fire a height box LostFocus → coalesce its Recompute with ours
            {
                state.AddLevel(bay);
                RenderMatrix();
                Recompute();
            }
        }

        private void RemoveLevel(int bay)
        {
            if (!state.CanRemoveLevel(bay))
            {
                SetStatus("Cada frente necesita al menos un nivel.", true);
                return;
            }

            using (DeferRecompute()) // the rebuild can fire a height box LostFocus → coalesce its Recompute with ours
            {
                state.RemoveLevel(bay); // drops the top level and clamps the selection
                LoadCellEditor();
                RenderMatrix();
                Recompute();
            }
        }

        /// <summary>"Larguero a piso" of a frente, over the SAME target fondos as everything else (I-43, gate 7).
        /// The flag has no inheritance, so it is written explicitly; the state omits and reports a target fondo that
        /// does not have this frente, and the window recomputes ONCE.</summary>
        private void SetFloor(int bay, bool value, SelectiveFrontApplyScope scope = SelectiveFrontApplyScope.Front)
        {
            if (bay < 0 || bay >= floorBeams.Count) return;

            using (DeferRecompute())
            {
                SaveWorkingToSelected(); // the state writes the OTHER fondos' stored matrices
                FrontApplyLog.Add("Piso:" + scope);
                var result = state.ApplyFloorBeamToTargets(scope, bay, value);
                if (scope == SelectiveFrontApplyScope.All) RenderMatrix(); // every checkbox of the fondo may have moved
                Recompute();
                if (state.TargetFondos.Count > 1 || result.OmittedFondos.Count > 0)
                {
                    pendingWarning = result.Describe(restore: false);
                }
            }
        }

        private void ClampSelection() => state.ClampSelection();

        private bool TryGetSelected(out Cell cell) => state.TryGetSelected(out cell);

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

            // The checkbox is Front x TargetFondos; the button applies its CURRENT value to every frente of those
            // fondos in ONE operation (I-43, gate 7) — never Front first and All after.
            var floorRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            floorRow.Children.Add(floor);
            var floorAll = SmallButton("⇊");
            floorAll.ToolTip = "Aplicar este «Piso» a TODOS los frentes de los fondos destino.";
            floorAll.Margin = new Thickness(4, 3, 0, 0);
            WireScopeButton(floorAll, () => SetFloor(bay, floor.IsChecked == true, SelectiveFrontApplyScope.All));
            floorRow.Children.Add(floorAll);
            panel.Children.Add(floorRow);

            // "Elevacion de larguero a piso" of THIS frente (I-43, ID14): a number overrides the global, empty
            // inherits it. It sits under "Piso" because that is the only flag it acts on — and it is NOT cleared when
            // that box is unchecked, so re-checking brings the value back.
            var riseRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 3, 0, 0) };
            riseRow.Children.Add(new TextBlock { Text = "Elev.", Foreground = LabelStroke, FontSize = 10.5, Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            var stored = state.FloorBeamRiseOverrideAt(selectedFondo, bay);
            var riseBox = new TextBox
            {
                Width = 44,
                FontSize = 10.5,
                Text = stored.HasValue ? stored.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty,
                ToolTip = "Elevación del larguero a piso de ESTE frente (in). Vacío = usa el valor global; 0 = elevación cero, que no es lo mismo. "
                    + "Solo actúa cuando 'Piso' está marcado, pero el valor se conserva aunque lo desmarques. "
                    + "Se aplica a este frente en TODOS los fondos destino de «Aplicar en fondos»."
            };
            riseBox.LostFocus += (s, e) => SetFloorBeamRise(bay, riseBox.Text);
            // e.Handled for the same reason as the height box: Enter must not also fire the window's default button.
            riseBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) { SetFloorBeamRise(bay, riseBox.Text); e.Handled = true; } };
            riseRow.Children.Add(riseBox);

            var riseAll = SmallButton("⇊");
            riseAll.ToolTip = "Aplicar esta elevación a TODOS los frentes de los fondos destino.";
            riseAll.Margin = new Thickness(4, 0, 0, 0);
            WireScopeButton(riseAll, () => SetFloorBeamRise(bay, riseBox.Text, SelectiveFrontApplyScope.All));
            riseRow.Children.Add(riseAll);
            panel.Children.Add(riseRow);

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

            var heightAll = SmallButton("⇊");
            heightAll.ToolTip = "Aplicar esta altura a TODOS los frentes de los fondos destino (vacío = restablecer a automático).";
            heightAll.Margin = new Thickness(4, 0, 0, 0);
            WireScopeButton(heightAll, () => SetBayHeight(bay, heightBox.Text, SelectiveFrontApplyScope.All));
            heightRow.Children.Add(heightAll);
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

        /// <summary>
        /// Commit the per-frente "elevacion de larguero a piso". The value goes to <see cref="SelectiveEditorState"/>,
        /// which writes it across every target fondo that has that frente — the window never loops over fondos, and
        /// recomputes ONCE for the whole operation (I-43, ID14).
        /// <para>
        /// An empty box is the RESTORE (null: inherit the global); <c>0</c> is an explicit zero. Scope <c>All</c>
        /// writes every frente of those fondos instead of just this one.
        /// </para>
        /// </summary>
        private void SetFloorBeamRise(int bay, string text, SelectiveFrontApplyScope scope = SelectiveFrontApplyScope.Front)
        {
            if (bay < 0 || bay >= bays.Count) return;

            // Parsed here rather than with UiSupport.TryOptionalNum: that helper rejects 0, which is right for a
            // height ("auto or a real height") and wrong here, where 0 is an explicit "no rise at all" and must be
            // distinguishable from empty (I-43, ID14).
            double? value = null;
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!UiSupport.TryNum(text, out var typed) || typed < 0.0)
                {
                    SetStatus("Elevación de larguero a piso inválida (vacío = usa el valor global; 0 = sin elevación).", true);
                    return;
                }

                value = typed;
            }

            if (scope == SelectiveFrontApplyScope.Front
                && Nullable.Equals(state.FloorBeamRiseOverrideAt(selectedFondo, bay), value)
                && state.TargetFondos.Count == 1
                && state.TargetFondos.Fondos[0] == selectedFondo)
            {
                return; // nothing to do: the only target already holds this value
            }

            using (DeferRecompute())
            {
                // The fondo boxes must be committed first: the state writes the OTHER fondos' stored matrices.
                SaveWorkingToSelected();
                FrontApplyLog.Add("Elevacion:" + scope);
                var result = state.ApplyFloorBeamRiseToTargets(scope, bay, value);
                RenderMatrix();
                Recompute();
                pendingWarning = result.Describe(restore: !value.HasValue);
            }
        }

        /// <summary>A frente's manual height, over the target fondos (I-43, gate 7). Null is the RESTORE to the
        /// derived height — there is no run-wide default here — and the legacy parse is kept verbatim, so 0 stays an
        /// invalid height rather than becoming a second way to say "auto".</summary>
        private void SetBayHeight(int bay, string text, SelectiveFrontApplyScope scope = SelectiveFrontApplyScope.Front)
        {
            if (bay < 0 || bay >= bayHeights.Count) return;
            if (!UiSupport.TryOptionalNum(text, out var value)) { SetStatus("Altura de frente inválida (vacío = auto).", true); return; }
            if (scope == SelectiveFrontApplyScope.Front
                && Nullable.Equals(bayHeights[bay], value)
                && state.TargetFondos.Count == 1
                && state.TargetFondos.Fondos[0] == selectedFondo)
            {
                return; // nothing to do: the only target already holds this height
            }

            using (DeferRecompute())
            {
                SaveWorkingToSelected();
                FrontApplyLog.Add("Alto:" + scope);
                var result = state.ApplyBayHeightToTargets(scope, bay, value);
                RenderMatrix();
                Recompute();
                if (state.TargetFondos.Count > 1 || result.OmittedFondos.Count > 0)
                {
                    pendingWarning = result.Describe(restore: !value.HasValue);
                }
            }

        }

        /// <summary>Open the tramos ("medio frente" generalizado) editor for a frente and apply the result.</summary>
        private void EditTramos(int bay)
        {
            if (bay < 0 || bay >= baySegments.Count) return;

            // Best-effort full bay width (shared across fondos) so the dialog can show the calculated last tramo + warn.
            var fullWidth = lastSystem != null && bay < lastSystem.Bays.Count ? lastSystem.Bays[bay].BeamLength : 0.0;

            // The dialog opens ONCE, seeded from the VISIBLE frente. Cancelling changes nothing anywhere (I-39).
            var dialog = new SelectiveSegmentsWindow(bay + 1, baySegments[bay], fullWidth) { Owner = this };
            if (dialog.ShowDialog() != true) return;

            using (DeferRecompute())
            {
                SaveWorkingToSelected();
                // Application projects the accepted tramos onto the SAME frente of every valid target, each with its
                // own copy of the segments. Scope is Front only, and that is a domain limit, not an omission: the
                // width of a frente is shared by FrontIndex across fondos but NOT between different frentes.
                var result = state.ApplySegmentsToTargets(bay, dialog.Result.Select(s => new SelectiveSegment { Length = s.Length, Loaded = s.Loaded }));
                RenderMatrix(); // refresh the button label (tramo count)
                Recompute();
                if (state.TargetFondos.Count > 1 || result.OmittedFondos.Count > 0)
                {
                    pendingWarning = result.Describe(restore: false);
                }
            }
        }

        // ---- Per-post cabeceras ----

        /// <summary>The largest frente count across all fondos (the master grid), delegating to the state (live working
        /// matrix for the selected fondo, saved slots for the rest).</summary>
        private int MaxFrenteCount() => state.MaxFrenteCount();

        /// <summary>Keep the per-post cabecera + peralte lists sized to the MASTER grid's posts (delegates to the state);
        /// sizing to the LONGEST fondo means switching to a shorter fondo never truncates fondo 0's custom cabeceras.</summary>
        private void SyncPostCabeceras() => state.SyncPostCabeceras();

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

        /// <summary>"Personalizada / Por defecto" always describes the pair <c>(fondo visible, poste seleccionado)</c>
        /// (I-43): with several fondos the same post can be custom on one and standard on another, so a status that
        /// only ever read fondo 0 would be wrong on every other fondo.</summary>
        private void UpdatePostStatus()
        {
            if (PostCabeceraStatus == null) return;
            var i = PostSelectBox.SelectedIndex;
            if (i < 0)
            {
                PostCabeceraStatus.Text = string.Empty;
                return;
            }

            var custom = state.CabeceraAt(selectedFondo, i) != null;
            var label = custom ? "Personalizada" : "Por defecto (del tramo)";
            PostCabeceraStatus.Text = state.FondoCount > 1
                ? label + " · fondo " + (selectedFondo + 1).ToString(CultureInfo.InvariantCulture)
                : label;
        }

        private void CustomizePost_Click(object sender, RoutedEventArgs e)
        {
            var i = PostSelectBox.SelectedIndex;
            if (i < 0 || i >= postCabeceras.Count) return;

            // Seed from the fondo the user is LOOKING AT (I-43): its custom cabecera at this post if it has one, else
            // the standard cabecera resolved with THAT fondo's height and depth. Seeding from fondo 0 would open a
            // cabecera that is not the one on screen.
            var resolvedHeight = ResolvedPostHeight(i);
            var fondo = ResolvedFondo();

            // Work on a CLONE and compare before/after: closing the configurator without editing is a real
            // CANCEL (before, the seed was mutated up-front and any close marked the post "Personalizada").
            var visibleCustom = state.CabeceraAt(selectedFondo, i);
            var seed = visibleCustom != null ? CloneCabecera(visibleCustom) : BuildStandardPostCabecera(resolvedHeight, fondo);
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
            // PostPeraltes stays GLOBAL by post (I-43): it is not part of the per-fondo write below.
            if (i < postPeraltes.Count)
            {
                var edited = cfg.PostPeralte;
                postPeraltes[i] = (edited > 0.0 && Math.Abs(edited - globalPeralte) > 1e-6) ? edited : 0.0;
                ShowPostPeralteOverride();
            }

            // ONE Application call writes this post in every target fondo, each with its own deep copy, and reports the
            // fondos that do not reach that post. The window never loops over fondos (I-43).
            using (DeferRecompute())
            {
                // Commit the depth/cabecera boxes into their fondo slot first: ApplyCabeceraToTargets resolves EACH
                // target's cabecera depth from its slot, so an uncommitted box would stamp a stale depth (I-43).
                SaveWorkingToSelected();
                var result = state.ApplyCabeceraToTargets(i, cfg, CloneCabecera);
                UpdatePostStatus();
                Recompute();
                pendingWarning = result.Describe(reset: false);
            }
        }

        /// <summary>Deep-clone a cabecera via the single canonical clone (initiative I-17),
        /// <see cref="RackFrameProjectStore.DeepCopy"/>. For the persisted+derived model this bare-header round-trip
        /// matches the previous wrapper round-trip (<see cref="RackProjectStore"/> + <see cref="RackProject.ForSelective"/>):
        /// both copy the same <c>RackFrameProjectDocument</c> source of truth, rebuild the derived members and reject an
        /// unusable header. DeepCopy additionally re-attaches the runtime-only overrides
        /// (<see cref="RackFrameConfiguration.Exceptions"/>) that the wrapper round-trip dropped; they are metadata only
        /// (they do not drive geometry or BOM), so state is preserved without any visible change.</summary>
        private static RackFrameConfiguration CloneCabecera(RackFrameConfiguration configuration)
            => new RackFrameProjectStore().DeepCopy(configuration);

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
            // The safety grid needs the resolved matrix dimensions and the fondo count so the tope picker can offer the
            // real fondos (doble/triple profundidad).
            var depthCount = UiSupport.TryNum(FondosBox.Text, out var fondosNum) && fondosNum >= 1.0
                ? Math.Min(SelectiveRackDefaults.MaxDepthCount, Math.Max(1, (int)Math.Round(fondosNum)))
                : 1;
            // The parrilla grid shows a live deck count per cell, which needs the RESOLVED claros (and the medio-frente
            // tramos). Resolve once here; if the design doesn't resolve yet (BuildSystem returns null), the dialog still
            // configures — it just omits the counts.
            var resolved = BuildSystem(out _);
            // Drawing/BOM index RESOLVED beam levels. The design's first row may be a floor pallet with no beam; using
            // design row counts made the top checkbox dead and shifted every visible safety choice by one level.
            var levelsPerFrente = resolved != null
                ? SelectiveSafetyGrid.LevelCounts(resolved)
                : bays.Select((bay, i) => Math.Max(0, bay.Count - (i < floorBeams.Count && floorBeams[i] ? 0 : 1))).ToList();
            var parrillaPlan = resolved != null ? SelectiveParrillaPlan.Cells(resolved, catalog) : null;

            var dialog = new SelectiveSafetyWindow(catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>(), safetySelections, MaxFrenteCount() + 1, levelsPerFrente, depthCount, parrillaPlan, catalog, resolved) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            safetySelections.Clear();
            safetySelections.AddRange(dialog.Result.Select(selection => selection.DeepCopy()));
            UpdateSafetyButton();
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

        /// <summary>
        /// Restore the standard cabecera at the selected post, over the SAME target fondos the rest of the editor uses.
        /// <para>
        /// The per-post PERALTE is a separate, GLOBAL authority and is only cleared when the reset covers EVERY fondo
        /// (I-43). A reset aimed at some fondos is a statement about those cabeceras; wiping a rack-wide peralte
        /// override because of it would destroy something the user never pointed at. With a single fondo, or with all
        /// of them targeted, the behaviour is exactly the legacy one.
        /// </para>
        /// </summary>
        private void ResetPost_Click(object sender, RoutedEventArgs e)
        {
            var i = PostSelectBox.SelectedIndex;
            if (i < 0 || i >= postCabeceras.Count) return;

            using (DeferRecompute())
            {
                var result = state.ApplyCabeceraToTargets(i, null, CloneCabecera);

                var allFondos = state.TargetFondos.Count >= state.FondoCount;
                if (allFondos && i < postPeraltes.Count)
                {
                    postPeraltes[i] = 0.0; // whole-post reset: back to the global peralte too, as it always did
                    ShowPostPeralteOverride();
                }

                UpdatePostStatus();
                Recompute();
                pendingWarning = result.Describe(reset: true);
            }
        }

        /// <summary>
        /// Wire a compact "apply to every frente" button so a REAL mouse click performs exactly ONE operation.
        /// <para>
        /// WPF moves focus to a Button on <c>MouseLeftButtonDown</c>, which makes the TextBox next to it raise
        /// <c>LostFocus</c> BEFORE the button's <c>Click</c>. Left alone, one gesture would run the Front commit and
        /// then the All one — two mutations of the model, and the first of them rebuilds the matrix and destroys the
        /// very button the click is travelling to. So the gesture is consumed one step earlier, on
        /// <c>PreviewMouseLeftButtonDown</c>: the handler runs the All operation and marks the event handled, which
        /// stops <c>ButtonBase</c> from taking focus at all. No focus change, no LostFocus, no Front commit.
        /// </para>
        /// <para>
        /// There is no flag anywhere: the decision is the event itself, so a cancelled or redirected gesture leaves
        /// nothing behind. The <c>Click</c> handler stays for keyboard and programmatic activation, where focus has
        /// already moved deliberately and the ordinary commit is the correct behaviour.
        /// </para>
        /// </summary>
        private static void WireScopeButton(Button button, Action apply)
        {
            button.PreviewMouseLeftButtonDown += (s, e) =>
            {
                e.Handled = true; // consume the gesture before ButtonBase focuses the button
                apply();
            };
            button.Click += (s, e) => apply();
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

            StyleCellBorder(border, bay == selBay && level == selLevel, state.IsSelected(bay, level));
            RefreshCellVisual(border, cell);

            // Ctrl+clic agrega/quita de la seleccion; el clic normal deja UNA sola celda — el mismo gesto del dinamico.
            border.MouseLeftButtonUp += (s, e) =>
                SelectCell(bay, level, (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
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

        /// <summary>
        /// Selection styling of a matrix cell — single source for <see cref="CellUi"/> and <see cref="SelectCell"/>.
        /// Three states, the same grammar Dinamico/Push Back already use: the PRIMARY cell (the one the editor is
        /// bound to) keeps the strong stroke, a cell that is merely part of the multi-selection gets the same fill
        /// with a softer stroke, and an unselected one stays white and thin (I-43).
        /// </summary>
        private static void StyleCellBorder(Border border, bool primary, bool included)
        {
            border.Background = primary || included ? CellSelFill : Brushes.White;
            border.BorderBrush = primary ? CellSelStroke : included ? CellMultiStroke : CellStroke;
            border.BorderThickness = new Thickness(primary || included ? 2 : 1);
        }

        /// <summary>Restyle EVERY cached cell to the current selection. A plain click can deselect an arbitrary number
        /// of cells, so tracking a delta would be wrong; this only sets three properties per cell and creates no
        /// elements, which is what the border cache exists to make cheap.</summary>
        private void RefreshSelectionVisuals()
        {
            foreach (var entry in cellBorders)
            {
                StyleCellBorder(entry.Value, entry.Key.Bay == selBay && entry.Key.Level == selLevel, state.IsSelected(entry.Key.Bay, entry.Key.Level));
            }
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

        private void SelectCell(int bay, int level) => SelectCell(bay, level, extend: false);

        /// <summary>
        /// A matrix click. <paramref name="extend"/> (Ctrl) toggles the cell in the multi-selection; without it the
        /// cell becomes the only selection. The state refuses to empty the selection and keeps the primary coherent,
        /// so the cell editor always has a cell to bind to (I-43).
        /// </summary>
        private void SelectCell(int bay, int level, bool extend)
        {
            using (DeferRecompute())
            {
                // Don't silently discard what was typed in the current cell: apply it if valid+changed, or ask before
                // discarding if it is invalid (returning false keeps the user on the current cell).
                if (!TryCommitEditedCell(out var applied)) return;

                var prevBay = selBay;
                var prevLevel = selLevel;
                state.SelectCell(bay, level, extend);
                LoadCellEditor();

                // Same effect the full rebuild had here: a focused bay-height box commits (LostFocus) before the
                // visuals change; its Recompute coalesces with the one below.
                CommitFocusedMatrixTextBox();

                // A click only changes the selection visuals (and, when applied, the OLD cell's committed text):
                // restyle in place instead of rebuilding ~400 elements. Structure never changes here.
                if (cellBorders.Count > 0)
                {
                    RefreshSelectionVisuals();
                    if (applied && prevBay >= 0 && prevBay < bays.Count && prevLevel >= 0 && prevLevel < bays[prevBay].Count
                        && cellBorders.TryGetValue((prevBay, prevLevel), out var previous))
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
            // The header names the PRIMARY cell and, when a multi-selection is live, how many cells "Seleccionadas"
            // would write — so the count is readable without counting outlines in the matrix (I-43).
            CellHeader.Text = state.SelectedCount > 1
                ? string.Format(CultureInfo.InvariantCulture, "Celda: Frente {0} · Nivel {1} ({2} seleccionadas)", selBay + 1, selLevel + 1, state.SelectedCount)
                : string.Format(CultureInfo.InvariantCulture, "Celda: Frente {0} · Nivel {1}", selBay + 1, selLevel + 1);
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
        private void ApplySelected_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Selected);
        private void ApplyRow_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Row);
        private void ApplyColumn_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Column);
        private void ApplyAll_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.All);

        // ---- Fondos destino (I-43): the second axis, edited next to "Editando fondo" ----

        private void TargetFondos_LostFocus(object sender, RoutedEventArgs e) => CommitTargetFondos();

        private void TargetFondos_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            CommitTargetFondos();
            e.Handled = true; // Enter edits this field; it must not reach the window's default-button policy (I-39)
        }

        private void TargetFondosAll_Click(object sender, RoutedEventArgs e)
        {
            state.SetTargetFondos(Enumerable.Range(0, state.FondoCount));
            RefreshTargetFondos();
        }

        private void TargetFondosCurrent_Click(object sender, RoutedEventArgs e)
        {
            state.SetTargetFondos(new[] { selectedFondo });
            RefreshTargetFondos();
        }

        /// <summary>Read the typed subset. An unreadable or out-of-range entry is REJECTED with its reason and the box
        /// reverts to the set still in force — narrowing an operation silently is the failure this axis exists to
        /// avoid. Choosing targets changes no geometry, so nothing recomputes here.</summary>
        private void CommitTargetFondos()
        {
            if (!initialized) return;

            if (string.IsNullOrWhiteSpace(TargetFondosBox.Text))
            {
                state.SetTargetFondos(new[] { selectedFondo }); // blank = the fondo being edited (the default)
                RefreshTargetFondos();
                return;
            }

            if (!SelectiveFondoTargets.TryParse(TargetFondosBox.Text, state.FondoCount, out var parsed, out var error))
            {
                SetStatus(error, true);
                RefreshTargetFondos();
                return;
            }

            state.SetTargetFondos(parsed.Fondos);
            RefreshTargetFondos();
        }

        /// <summary>Show the target set the state actually holds (one-based, like the fondo combo), and hide the whole
        /// row when there is a single fondo — there the two axes always coincide.</summary>
        private void RefreshTargetFondos()
        {
            var fondos = state.TargetFondos.Fondos;
            TargetFondosPanel.Visibility = state.FondoCount > 1 ? Visibility.Visible : Visibility.Collapsed;
            TargetFondosBox.Text = string.Join(", ", fondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture)));
            TargetFondosHint.Text = fondos.Count == 1
                ? "(solo el fondo " + (fondos[0] + 1).ToString(CultureInfo.InvariantCulture) + ")"
                : "(" + fondos.Count.ToString(CultureInfo.InvariantCulture) + " fondos)";
        }
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

        /// <summary>
        /// Apply the cell editor over <c>alcance x fondos destino</c>. The window does NOT loop over fondos: it hands
        /// the scope to <see cref="SelectiveEditorState.ApplyToTargets"/>, which snapshots the topology, resolves the
        /// whole plan and writes every target — the active fondo's live matrix and the other fondos' stored ones —
        /// before returning. One plan, then ONE recompute, whether it touched one fondo or four (I-43).
        /// </summary>
        private void ApplyScope(Scope scope)
        {
            if (!ReadCellEditor(out var values, out var error))
            {
                SetStatus(error, true);
                return;
            }

            SelectiveTargetPlan plan;
            using (DeferRecompute())
            {
                // The scope rewrites cell VALUES, never the matrix shape, so we refresh in place instead of
                // rebuilding ~400 elements. Only the fondo on screen has borders to refresh; the rest were written
                // into their stored matrices and will show when the user navigates to them.
                plan = state.ApplyToTargets(scope, values);

                var stale = false;
                foreach (var target in plan.Targets)
                {
                    if (target.FondoIndex != selectedFondo) continue;
                    if (cellBorders.TryGetValue((target.FrontIndex, target.LevelIndex), out var border)) RefreshCellVisual(border, bays[target.FrontIndex][target.LevelIndex]);
                    else stale = true;
                }

                if (stale) RenderMatrix(); // defensive fallback: cache out of sync → old full-rebuild behavior
                else RefreshSelectionVisuals(); // the state may have re-seated the primary
                CommitFocusedMatrixTextBox(); // the rebuild used to commit a focused bay-height box here; keep that
                Recompute();
            }

            // Recompute says a generic "Vista actualizada."; tell the user HOW MANY cells the scope touched
            // (only on success — an error status from Recompute must stay visible).
            if (lastSystem != null)
            {
                SetStatus(DescribeApplied(plan), false);
            }
        }

        /// <summary>What an apply reached, in the user's own numbering (fondos are shown one-based, as the "Editando
        /// fondo" combo does). The single-fondo wording is the one the editor has always shown.</summary>
        private static string DescribeApplied(SelectiveTargetPlan plan)
        {
            if (plan.AnchorMissing) return "La celda de origen ya no existe: no se aplicó nada.";
            if (plan.IsEmpty) return "Ningún fondo destino tiene celdas en el alcance: no se aplicó nada.";

            var text = string.Format(CultureInfo.InvariantCulture, "Aplicado a {0} celda(s)", plan.Count);
            if (plan.Fondos.Count > 1)
            {
                text += " en los fondos " + string.Join(", ", plan.Fondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture)));
            }

            return text + ".";
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

            // Route the insert/update through the shared session (I-15): it captures the (trimmed) name, ensures the GUID,
            // normalizes the view (updateOnly ? null : view) and builds the typed payload — the same values as before.
            session.Identity.SetName(NameBox.Text?.Trim());
            session.SetModel(design, system);
            if (updateOnly)
            {
                session.RequestUpdate(ctx => new SelectiveInsertionRequest(system, design, ctx.Id, ctx.Name, ctx.View));
            }
            else
            {
                session.RequestInsert(view, section: -1, ctx => new SelectiveInsertionRequest(system, design, ctx.Id, ctx.Name, ctx.View));
            }

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
        private IDisposable DeferRecompute() => session.Recompute.Defer();

        private void Recompute() => session.Recompute.Request();

        /// <summary>How many times the REAL recompute pipeline ran. A test seam (I-43, InternalsVisibleTo): it is how
        /// the promise "one bulk apply, one recompute, however many fondos" is checked rather than asserted.</summary>
        internal int RecomputeCount { get; private set; }

        /// <summary>Every frente-wide Application operation this window issued, as <c>"property:scope"</c>. A test seam
        /// (I-43, InternalsVisibleTo): a recompute counter alone cannot tell one operation from two that coalesced, and
        /// what the gesture contract forbids is the second MUTATION, not the second redraw.</summary>
        internal List<string> FrontApplyLog { get; } = new List<string>();

        /// <summary>The editor state — a test seam (I-43, InternalsVisibleTo), matching the one the safety grids expose.</summary>
        internal SelectiveEditorState EditorState => state;

        /// <summary>The Border painted for a matrix cell, or null when it is not on screen — a test seam (I-43) so the
        /// multi-selection can be verified as PIXELS-worth-of-brush, not just as model state.</summary>
        internal Border MatrixCell(int bay, int level) => cellBorders.TryGetValue((bay, level), out var border) ? border : null;

        /// <summary>The brush a cell that is selected but not primary is outlined with (I-43 test seam).</summary>
        internal static Brush MultiSelectionStroke => CellMultiStroke;

        /// <summary>The brush the PRIMARY cell is outlined with (I-43 test seam).</summary>
        internal static Brush PrimarySelectionStroke => CellSelStroke;

        /// <summary>Repaint the selection — a test seam (I-43) for the gesture a real Ctrl+click drives, since a test
        /// cannot forge <c>Keyboard.Modifiers</c>.</summary>
        internal void RefreshSelectionVisualsForTest() => RefreshSelectionVisuals();

        /// <summary>The actual rebuild the session's coalescing gate runs; <see cref="DeferRecompute"/> collapses a burst
        /// of <see cref="Recompute"/> calls within a gesture into one pass here (same behavior as the old inline deferral).</summary>
        private void RunRecompute()
        {
            RecomputeCount++; // test seam (I-43): the coalescing gate must collapse a bulk apply into ONE run
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

        /// <summary>Reads and validates the editor's scalar/toggle controls into <see cref="SelectiveDesignInputs"/>, then
        /// delegates the pure assembly of the pallet-driven design (matrices + inputs) to
        /// <see cref="SelectiveEditorState.BuildDesign"/> (I-20). Returns null + a Spanish error for an invalid control or
        /// an empty fondo 0.</summary>
        private SelectivePalletDesign BuildDesign(out string error)
        {
            error = null;
            if (!(PostBox.SelectedValue is string postId) || string.IsNullOrWhiteSpace(postId)) { error = "Selecciona un poste."; return null; }
            if (!UiSupport.TryNum(PostPeralteBox.Text, out var postPeralte) || postPeralte <= 0.0) { error = "Peralte de poste inválido."; return null; }
            if (!UiSupport.TryNum(ToleranceBox.Text, out var tolerance) || tolerance < 0.0) { error = "Tolerancia horizontal inválida."; return null; }
            if (!UiSupport.TryNum(ClearanceBox.Text, out var clearance) || clearance < 0.0) { error = "Holgura vertical inválida."; return null; }
            if (!UiSupport.TryNum(FloorRiseBox.Text, out var floorRise) || floorRise < 0.0) { error = "Elevación de larguero a piso (global) inválida."; return null; }
            if (!UiSupport.TryNum(FondoBox.Text, out var fondo) || fondo <= 0.0) { error = "Fondo de tarima inválido."; return null; }
            if (!UiSupport.TryNum(FondosBox.Text, out var fondosNum) || fondosNum < 1.0) { error = "Número de fondos inválido (mínimo 1)."; return null; }
            var depthCount = Math.Min(SelectiveRackDefaults.MaxDepthCount, Math.Max(1, (int)Math.Round(fondosNum)));

            // The working fondo's depth/cabecera come from their boxes (with the keep-previous fallback); the state commits
            // the live matrix into its fondo slot before reading fondo 0. Safety is filtered + deep-copied here so its
            // ownership stays in the editor (I-22).
            var (workingDepth, workingCabecera) = ReadWorkingDepthCabecera();
            var inputs = new SelectiveDesignInputs
            {
                PostId = postId,
                PostPeralte = postPeralte,
                PalletTolerance = tolerance,
                VerticalClearance = clearance,
                FloorBeamRise = floorRise,
                Fondo = fondo,
                DepthCount = depthCount,
                WorkingDepth = workingDepth,
                WorkingCabeceraOverride = workingCabecera,
                Separators = ReadSeparators(),
                DrawBasePlate = DrawBasePlateCheck.IsChecked == true,
                NumberFronts = NumberFrontsCheck.IsChecked == true,
                NumberLevels = NumberLevelsCheck.IsChecked == true,
                DrawRackName = DrawRackNameCheck.IsChecked == true,
                DrawPallets = DrawPalletsCheck.IsChecked == true,
                AnnotationScale = UiSupport.TryNum(AnnotationScaleBox.Text, out var annScale) && annScale > 0.0 ? annScale : 1.0,
                Dimensions = (DimensionDetail)Math.Min((int)DimensionDetail.Detailed, Math.Max(0, DimensionsBox.SelectedIndex)),
                DimensionStyle = SelectedDimStyle(),
                SafetySelections = safetySelections
                    .Where(s => SafetyDraws(s) && !string.IsNullOrWhiteSpace(s.ElementId))
                    .Select(s => s.DeepCopy())
                    .ToList()
            };

            var design = state.BuildDesign(inputs);
            if (design == null) { error = "Define frentes y niveles."; return null; } // fondo 0 has no frentes/levels
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

            // The other fondos' cabecera rows (I-43). A design written before this axis existed carries none, so those
            // fondos stay standard — the drawing it described is reproduced exactly, with no migration.
            // No depth is repaired here: SelectiveCabeceraAuthority imposes each fondo's cabecera depth wherever a
            // custom is read, so load is not a place where a wrong value gets fixed.
            state.ExtraFondoPostCabeceras.Clear();
            for (var k = 1; k < fondoMatrices.Count; k++)
            {
                var stored = (k - 1) < design.ExtraFondoPostCabeceras.Count ? design.ExtraFondoPostCabeceras[k - 1] : null;
                state.ExtraFondoPostCabeceras.Add(stored == null
                    ? new List<RackFrameConfiguration>()
                    : new List<RackFrameConfiguration>(stored));
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
                    safetySelections.Add(safety.DeepCopy());
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
            session.Identity.Adopt(document.Id, document.Name); // keep the drawn rack's GUID + name (I-15)
            isEditingExisting = true; // opened on an existing rack → "Actualizar" + linked lateral/planta become available
            UpdateInsertButtons();
            NameBox.Text = document.Name ?? string.Empty;
            LoadDesign(document.ToDomain());
        }

        /// <summary>Open pre-loaded from a LIBRARY template as a NEW rack — a fresh GUID on insert (not an in-place update),
        /// mirroring the dynamic editor's library open. Keeps the "Insertar" flow.</summary>
        public void LoadForNew(SelectivePalletDesignDocument document, RackProject sourceProject = null)
        {
            if (document == null) return;
            this.sourceProject = sourceProject;
            session.Identity.Adopt(null, document.Name); // a library template inserts as a NEW rack: no id yet, fresh GUID on insert (I-15)
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

            // A throwaway file id when the rack has no GUID yet (a library template re-opens with LoadForNew, which nulls
            // it anyway): does NOT mint into the session identity, so a later Insert still gets its own fresh GUID (I-15).
            var id = session.Identity.HasId ? session.Identity.Id : Guid.NewGuid().ToString();
            var name = string.IsNullOrWhiteSpace(NameBox.Text) ? session.Identity.Name : NameBox.Text.Trim();
            var document = SelectivePalletDesignDocument.From(design, id, name);

            var path = UiSupport.PromptSaveToLibrary(this, name, "selectivo");
            if (path == null) return;

            try
            {
                new RackProjectStore().Save(RackProject.ForSelectiveRack(document).WithSourceMetadataFrom(sourceProject), path);
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
                        // Visual reference only: box whose bottom-CENTRE is Insertion (the block's bottom-centre origin);
                        // item.Size = frente, item.Peralte = alto. Painted after the beams so a tarima sits on its larguero.
                        var palTop = Map(item.X - item.Size / 2.0, item.Y + item.Peralte);
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
            => UiSupport.TryInt(text, out value);
    }
}
