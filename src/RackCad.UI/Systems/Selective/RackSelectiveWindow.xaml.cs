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
using RackCad.Application.Settings;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Controls;
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

        // I-43 (gate 8.6C): las cuatro cajas son editores de un valor PENDIENTE; la autoridad son los slots y las
        // matrices. Se comprometen en dos fases y SIEMPRE en este orden: estructura antes que valores.
        private PendingTextField<int> pendingFondos;
        private PendingTextField<int> pendingBayCount;
        private PendingTextField<double> pendingDepth;
        private PendingTextField<double?> pendingCabecera;
        private IPendingTextField[] pendingAll;
        private readonly IUserSettingsGateway settingsGateway;
        private readonly UserSettings settings;

        /// <summary>The remembered "Fondos destino" INTENT (gate 8 correction). Kept for the session because it must be
        /// re-resolved against each rack that gets OPENED: a preference of <c>{1,3}</c> means nothing against the empty
        /// one-fondo matrix a fresh window starts with, and would be thrown away if it were only read there.</summary>
        private SelectiveTargetPreference targetPreference = SelectiveTargetPreference.All;

        /// <summary>The dynamic per-gap separator textboxes (one per hueco between consecutive fondos).</summary>
        private readonly List<TextBox> separatorBoxes = new List<TextBox>();

        private int selBay { get => state.SelBay; set => state.SelBay = value; }
        private int selLevel { get => state.SelLevel; set => state.SelLevel = value; }
        private bool loadingCell;

        /// <summary>
        /// The run-wide "elevacion de larguero a piso" a loaded document carried, kept ONLY so the design keeps
        /// writing the legacy field and an older reader still finds it (I-43, gate 8A). It is no longer an operative
        /// concept: the authority is each frente's own value, and this is never shown or edited.
        /// </summary>
        private double legacyFloorBeamRise = SelectiveRackDefaults.DefaultFloorBeamRise;

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
            : this(canInsertInAutoCad, new UserSettingsGateway())
        {
        }

        /// <summary>Same window against an explicit settings gateway, so a test can drive the remembered "Fondos
        /// destino" preference without reading or writing the developer's real <c>%APPDATA%</c>.</summary>
        internal RackSelectiveWindow(bool canInsertInAutoCad, IUserSettingsGateway settingsGateway)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            this.settingsGateway = settingsGateway ?? new UserSettingsGateway();
            settings = this.settingsGateway.Load();
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

            BuildPendingEditors();

            state.InitMatrix(2, 4);
            fondoMatrices.Clear();
            fondoMatrices.Add(SnapshotWorking());
            selectedFondo = 0;
            // No-op con la siembra de InitMatrix, pero deja escrito que una ventana recien abierta ya cumple INV-12:
            // ningun frente espera a que alguien coalesque el global.
            state.MaterializeFloorBeamRises(legacyFloorBeamRise);
            targetPreference = SelectiveTargetPreference.Decode(settings?.SelectiveTargetFondos);
            ApplyStoredTargetPreference();
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
        /// <summary>
        /// Las profundidades COMPROMETIDAS del fondo seleccionado, leídas de su slot (I-43, gate 8.6C). Antes esto
        /// leía <c>FondoBox</c>/<c>CabeceraFondoBox</c>, que es de donde se filtraba el texto pendiente al slot y al
        /// diseño (S7). Sin slot todavía —el constructor, antes del primer snapshot— vale el valor por defecto.
        /// </summary>
        private (double Depth, double CabeceraOverride) CommittedDepthCabecera()
        {
            var slot = selectedFondo >= 0 && selectedFondo < fondoMatrices.Count ? fondoMatrices[selectedFondo] : null;
            var depth = slot != null && slot.Depth > 0.0 ? slot.Depth : SelectiveRackDefaults.DefaultPalletDepth;
            return (depth, slot?.CabeceraOverride ?? 0.0);
        }

        // ---- Per-fondo matrices (doble profundidad: each fondo edits its own levels) ----

        /// <summary>Snapshot the live working matrix (the selected fondo) into a saveable copy, reading its fondo (depth)/cabecera boxes.</summary>
        private FondoMatrix SnapshotWorking()
        {
            var (depth, cabecera) = CommittedDepthCabecera();
            return state.SnapshotWorking(depth, cabecera);
        }

        /// <summary>Load a saved fondo matrix into the live working matrix (state deep-clones it), and sync its fondo/cabecera boxes.</summary>
        private void RestoreWorkingFrom(FondoMatrix snap)
        {
            state.RestoreWorkingFrom(snap);
            ShowDepthBoxes();
        }

        /// <summary>
        /// Commit the live working MATRIX back into its fondo slot, conservando las profundidades COMPROMETIDAS de ese
        /// slot (I-43, gate 8.6C).
        /// <para>
        /// Sigue guardando exactamente lo mismo que antes —<c>Bays</c>, <c>FloorBeams</c>, <c>BayHeights</c>,
        /// <c>FloorBeamRiseOverrides</c> y <c>BaySegments</c>—; lo único que cambia es de dónde salen <c>Depth</c> y
        /// <c>CabeceraOverride</c>: del propio slot, no de las cajas. Así una matriz se puede comprometer sin que el
        /// texto pendiente de la caja se cuele en el fondo visible aunque no sea destino.
        /// </para>
        /// </summary>
        private void SaveWorkingToSelected()
        {
            var (depth, cabecera) = CommittedDepthCabecera();
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
        private void SaveWorkingMatrixKeepingDepths() => SaveWorkingToSelected();

        // ---- I-43, gate 8.6C: frontera pendiente / comprometido -------------------------------------------

        /// <summary>Texto comprometido de cada caja: lo que el estado dice AHORA, nunca lo tecleado.</summary>
        private string CommittedFondosText() => Math.Max(1, fondoMatrices.Count).ToString(CultureInfo.InvariantCulture);

        private string CommittedBayCountText() => bays.Count.ToString(CultureInfo.InvariantCulture);

        private string CommittedDepthText()
            => CommittedDepthCabecera().Depth.ToString("0.###", CultureInfo.InvariantCulture);

        private string CommittedCabeceraText()
        {
            var over = CommittedDepthCabecera().CabeceraOverride;
            return over > 0.0 ? over.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>
        /// Re-mostrar las dos cajas de profundidad desde el slot del fondo VISIBLE.
        /// <para>
        /// Un campo con una edición SIN RESOLVER se deja intacto: puede llegarse aquí desde un commit parcial —el
        /// gesto propio de «Fondo de tarima» solo compromete su campo— y entonces lo tecleado en el hermano sigue
        /// siendo del usuario. Pisarlo sería justo el descarte silencioso que INV-14 prohíbe; el hermano se resolverá
        /// en su propio gesto o en la siguiente frontera.
        /// </para>
        /// </summary>
        private void ShowDepthBoxes()
        {
            if (pendingDepth != null && !pendingDepth.IsDirty) pendingDepth.Show(CommittedDepthText());
            if (pendingCabecera != null && !pendingCabecera.IsDirty) pendingCabecera.Show(CommittedCabeceraText());
        }

        /// <summary>
        /// Ata cada caja a su parseo (fase 1) y a su escritura productiva (fase 2). El orden del arreglo ES el orden
        /// contractual del commit (INV-17): la estructura define la topología contra la que se resuelven los valores,
        /// y <c>TargetFondos</c> se re-resuelve entre medias.
        /// </summary>
        private void BuildPendingEditors()
        {
            pendingFondos = new PendingTextField<int>(
                FondosBox,
                "Número de fondos",
                text => UiSupport.TryNum(text, out var f) && f >= 1.0
                    ? PendingParse<int>.Valid(Math.Min(SelectiveRackDefaults.MaxDepthCount, (int)Math.Round(f)))
                    : PendingParse<int>.Invalid("Número de fondos inválido (mínimo 1)."),
                ApplyFondoCount,
                CommittedFondosText);

            pendingBayCount = new PendingTextField<int>(
                BayCountBox,
                "Frentes",
                text => TryInt(text, out var n) && n >= 1
                    ? PendingParse<int>.Valid(n)
                    : PendingParse<int>.Invalid("Cantidad de frentes inválida (mínimo 1)."),
                ApplyBayCountToTargets,
                CommittedBayCountText);

            pendingDepth = new PendingTextField<double>(
                FondoBox,
                "Fondo de tarima",
                text => UiSupport.TryNum(text, out var d) && d > 0.0
                    ? PendingParse<double>.Valid(d)
                    : PendingParse<double>.Invalid("Fondo de tarima inválido."),
                ApplyPalletDepthToTargets,
                CommittedDepthText);

            // Vacío es un valor legítimo: "derivado de la tarima". Por eso el tipo es anulable y no un double con
            // centinela: un RESTORE explícito tiene que poder distinguirse de "no hay nada que aplicar".
            pendingCabecera = new PendingTextField<double?>(
                CabeceraFondoBox,
                "Fondo de cabecera",
                text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return PendingParse<double?>.Valid(null);
                    return UiSupport.TryNum(text, out var over) && over > 0.0
                        ? PendingParse<double?>.Valid(over)
                        : PendingParse<double?>.Invalid("Fondo de cabecera inválido (vacío = derivado de la tarima).");
                },
                ApplyCabeceraDepthToTargets,
                CommittedCabeceraText);

            pendingAll = new IPendingTextField[] { pendingFondos, pendingBayCount, pendingDepth, pendingCabecera };
        }

        /// <summary>Fase 2 de "Número de fondos": crece o encoge la lista de slots y re-resuelve los destinos.</summary>
        private void ApplyFondoCount(int n)
        {
            SaveWorkingToSelected();
            if (fondoMatrices.Count == 0) fondoMatrices.Add(SnapshotWorking());

            while (fondoMatrices.Count < n) fondoMatrices.Add(CloneAligned(fondoMatrices[0], fondoMatrices[0].Bays.Count, fondoMatrices[0]));
            while (fondoMatrices.Count > n) fondoMatrices.RemoveAt(fondoMatrices.Count - 1);
            if (selectedFondo >= fondoMatrices.Count) selectedFondo = 0;

            // RebuildFondoSelector re-resuelve TargetFondos (SyncTargetFondos): "Todos" se expande al fondo nuevo,
            // "Explicit" poda y "Actual" sigue al visible. Los valores pendientes que se apliquen después ven ya ese
            // conjunto final, que es justo lo que exige INV-17.
            RebuildFondoSelector();
            RebuildSeparatorFields(fondoMatrices.Count);
            structuralCommit = true;
        }

        /// <summary>Fase 2 de "Frentes": redimensiona cada fondo destino por separado.</summary>
        private void ApplyBayCountToTargets(int bayCount)
        {
            SaveWorkingToSelected();
            var resize = state.ApplyBayCountToTargets(bayCount);
            state.MaterializeFloorBeamRises(legacyFloorBeamRise); // frentes que acaban de aparecer reciben un valor directo
            if (state.TargetFondos.Count > 1 || resize.OmittedFondos.Count > 0)
            {
                pendingWarning = resize.Describe("el número de frentes", restore: false);
            }

            structuralCommit = true;
        }

        /// <summary>Fase 2 de "Fondo de tarima".</summary>
        private void ApplyPalletDepthToTargets(double depth)
        {
            SaveWorkingToSelected();
            var result = state.ApplyPalletDepthToTargets(depth);
            if (state.TargetFondos.Count > 1) pendingWarning = result.Describe("el fondo de tarima", restore: false);
        }

        /// <summary>Fase 2 de "Fondo de cabecera"; <c>null</c> es el RESTORE al valor derivado.</summary>
        private void ApplyCabeceraDepthToTargets(double? over)
        {
            SaveWorkingToSelected();
            var result = state.ApplyCabeceraDepthToTargets(over);
            if (state.TargetFondos.Count > 1) pendingWarning = result.Describe("el fondo de cabecera", restore: !over.HasValue);
        }

        /// <summary>Marca que un apply cambió la TOPOLOGÍA, para reconciliar la UI una sola vez al cerrar el commit.</summary>
        private bool structuralCommit;

        /// <summary>
        /// Compromete los campos pendientes indicados, en DOS FASES y de forma atómica (INV-16).
        /// <para>
        /// Fase 1: se valida cada campo sucio en el orden recibido, sin mutar nada. Si alguno es inválido la operación
        /// se ABORTA: no se ejecuta ningún apply, no cambia ningún slot, ninguna matriz, <c>FondoMatrices.Count</c> ni
        /// <c>TargetFondos</c>, y el texto inválido se queda en su caja (una frontera no auto-repara). Fase 2: solo si
        /// todos son válidos, dentro de un ÚNICO <c>DeferRecompute</c>, se aplican en orden, se reconcilia la UI si la
        /// topología cambió y cada caja vuelve a mostrar el estado comprometido — un solo recompute (INV-07).
        /// </para>
        /// <para>
        /// La fase 2 no puede fallar por construcción: los apply son operaciones de estado sobre valores ya validados
        /// en sintaxis y rango, y las que pueden no alcanzar a un fondo (un destino sin ese poste) lo OMITEN y lo
        /// reportan en vez de reventar. La atomicidad se consigue validando antes, no revirtiendo después.
        /// </para>
        /// </summary>
        private bool CommitPendingEditors(params IPendingTextField[] fields)
        {
            var targets = fields == null || fields.Length == 0 ? pendingAll : fields;
            if (targets == null) return true;

            // --- fase 1: preparar TODO sin mutar nada ---
            var invalid = new List<string>();
            var dirty = false;
            foreach (var field in targets)
            {
                if (field == null) continue;
                if (field.IsDirty) dirty = true;
                if (!field.TryStage(out var error)) invalid.Add(error);
            }

            if (invalid.Count > 0)
            {
                SetStatus(string.Join(" ", invalid.Distinct()), true);
                return false;
            }

            if (!dirty) return true; // idempotente: tras un commit por LostFocus no queda nada que hacer

            // --- fase 2: aplicar todo, un solo recompute ---
            structuralCommit = false;
            using (DeferRecompute())
            {
                foreach (var field in targets) field?.ApplyStaged();

                if (structuralCommit)
                {
                    // La matriz VIVA acaba de cambiar de forma; su slot todavia tiene la anterior. Comprometerla
                    // antes de recargar es obligatorio: LoadFondo lee del slot y, sin esto, revertiria en silencio
                    // el redimensionado del fondo visible.
                    SaveWorkingToSelected();
                    LoadFondo(selectedFondo);
                    LoadCellEditor();
                    RenderMatrix();
                    RefreshPostSelect();
                    UpdateFrenteEditingEnabled();
                }

                foreach (var field in targets) field?.ShowCommitted();
                ShowDepthBoxes(); // las dos cajas describen el fondo VISIBLE, aunque la edición fuera a otros
                Recompute();
            }

            return true;
        }

        /// <summary>Compromete los cuatro campos. Es lo que hace toda frontera transaccional antes de consumir estado.</summary>
        private bool CommitPendingEditors() => CommitPendingEditors(pendingAll);

        /// <summary>
        /// El gesto propio de un campo ESTRUCTURAL (<c>Frentes</c>, <c>Número de fondos</c>): su reconciliación hace
        /// <c>Show</c> sobre los hermanos, así que estos tienen que estar resueltos.
        /// <para>
        /// Si un hermano está sucio e inválido, se ABORTA nombrándolo (C5): descartarlo con un <c>Show</c> sería
        /// perder en silencio lo que el usuario tecleó. Si el propio campo es inválido y NINGÚN hermano está sucio, se
        /// conserva la auto-reparación de siempre — es un descarte explícito de un texto que no significa nada.
        /// </para>
        /// </summary>
        private void CommitStructuralGesture(IPendingTextField own)
        {
            var siblingDirty = pendingAll.Any(f => !ReferenceEquals(f, own) && f.IsDirty);
            if (!siblingDirty && own.IsDirty && !own.TryStage(out var ownError))
            {
                pendingWarning = ownError;
                own.ResetToCommitted();
                Recompute();
                return;
            }

            CommitPendingEditors();
        }


        /// <summary>Re-show the two fondo boxes from the VISIBLE fondo's real slot. The boxes always describe the fondo
        /// on screen, whatever fondos an edit actually landed on; this only touches the two texts, never the matrix.</summary>
        private void SyncFondoDepthBoxes()
        {
            var slot = selectedFondo >= 0 && selectedFondo < fondoMatrices.Count ? fondoMatrices[selectedFondo] : null;
            if (slot == null) return;

            ShowDepthBoxes();
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
        /// <summary>Gesto propio de «Número de fondos»: <c>LoadFondo</c> hace <c>Show</c> de las otras tres cajas,
        /// así que el commit las incluye (C4).</summary>
        private void Fondos_LostFocus(object sender, RoutedEventArgs e)
        {
            if (catalog == null || !initialized) return; // ignore the initial value set during InitializeComponent
            if (!TryCommitEditedCell(out _)) return;      // don't discard typed cell input on a fondo-count change
            CommitStructuralGesture(pendingFondos);
        }

        /// <summary>Frentes (bay count) are edited PER FONDO now: each line can have its own count (a corner layout).
        /// The longest fondo defines the shared grid, so overlapping frentes still align at their posts.</summary>
        private void UpdateFrenteEditingEnabled()
        {
            BayCountBox.IsEnabled = true;
            BayCountBox.ToolTip = "Número de frentes (bahías). Se aplica a los «Fondos destino»: cada uno se redimensiona por separado "
                + "y los demás quedan intactos. Cada fondo puede tener su propio número (p. ej. esquina); "
                + "el fondo más largo define la rejilla y los frentes que se traslapan alinean sus postes. Se aplica al salir del campo.";
        }

        private void FondoSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (switchingFondo || catalog == null) return;
            var target = FondoSelectorBox.SelectedIndex;
            if (target < 0 || target >= fondoMatrices.Count || target == selectedFondo) return;

            // Los cuatro pendientes se comprometen ANTES de LoadFondo, que hace Show sobre sus cajas (C4). Un
            // pendiente inválido revierte el combo, igual que ya hace la celda inválida más abajo.
            if (!CommitPendingEditors())
            {
                switchingFondo = true;
                FondoSelectorBox.SelectedIndex = selectedFondo;
                switchingFondo = false;
                return;
            }

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
                pendingBayCount.Show(CommittedBayCountText());
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
        /// <summary>
        /// Gesto propio de «Fondo de tarima» / «Fondo de cabecera»: comprometen SOLO su campo (no muestran hermanos).
        /// Un texto inválido se queda en la caja con el error en status, como siempre (C5).
        /// </summary>
        private void FondoDepth_LostFocus(object sender, RoutedEventArgs e)
        {
            if (catalog == null || !initialized) return; // ignore the initial values set during InitializeComponent
            CommitPendingEditors(ReferenceEquals(sender, CabeceraFondoBox) ? pendingCabecera : (IPendingTextField)pendingDepth);
        }

        /// <summary>Enter compromete igual que salir del campo: O-43-02 lo exige y antes no hacía nada.</summary>
        private void FondoDepth_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            FondoDepth_LostFocus(sender, e);
            e.Handled = true;
        }



        /// <summary>Grow/shrink the number of bays (delegates to the state: a new bay clones the last — cells + floor flag + height + tramos).</summary>
        private void ResizeBays(int bayCount) => state.ResizeBays(bayCount);

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

        /// <summary>Open the tramos ("medio frente" generalizado) editor for a frente and apply the result.</summary>
        private void EditTramos(int bay)
        {
            if (bay < 0 || bay >= baySegments.Count) return;
            if (!CommitPendingEditors()) return; // frontera: fullWidth y la proyección leen estado comprometido (C4)

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
                    pendingWarning = result.Describe("el medio frente", restore: false);
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
            if (!CommitPendingEditors()) return; // frontera: la semilla lee slots (C4)

            // Seed from the fondo the user is LOOKING AT (I-43): its custom cabecera at this post if it has one, else
            // the standard cabecera resolved with THAT fondo's height and depth. Seeding from fondo 0 would open a
            // cabecera that is not the one on screen.
            var resolvedHeight = ResolvedPostHeight(i);

            // CabeceraDepthOfFondo reads the fondo's SLOT, and the fondo being edited lives in the working matrix
            // until it is committed: without this the seed would use the depth from before the user's last edit.
            SaveWorkingToSelected();

            // The depth of the fondo ON SCREEN. Reading fondo 0 here (what ResolvedFondo used to do) opened the
            // configurator at another fondo's cabecera depth — 42" while editing a 72" tarima, say.
            var fondo = ResolvedCabeceraFondo(selectedFondo);

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

            ApplyCustomizedCabecera(i, cfg, fondo, resolvedHeight, globalPeralte);
        }

        /// <summary>
        /// La mitad de "Personalizar" que ocurre DESPUÉS del configurador: validar la altura, avisar y escribir.
        /// Extraída para que sea comprobable — el configurador es modal y bloquea el hilo STA, así que sin esto el
        /// contrato de validación y de cancelación no se podría probar (I-43, gate 8.6E).
        /// </summary>
        private void ApplyCustomizedCabecera(int i, RackFrameConfiguration cfg, double fondo, double resolvedHeight, double globalPeralte)
        {
            // The depth is NOT the configurator's to choose: it belongs to the fondo (gate 4). Stamp the visible
            // fondo's depth here so what the user accepted matches what they saw; every TARGET fondo then has its own
            // depth imposed by ApplyCabeceraToTargets, which is the single authority.
            if (fondo > 0.0) cfg.Depth = fondo;

            // Height comes from the system; the user MAY override it, but warn it can desynchronize the rack
            // (the frontal largueros are placed for the resolved height). The SEVERE case is when the cabecera ends
            // up BELOW the top load level: the top larguero/pallet would stick out above the post — flag it specially.
            var topLevelY = TopLevelYAtPost(i);
            if (topLevelY > 0.0 && cfg.Height < topLevelY - 0.5)
            {
                SelectiveCabeceraHeightPrompt.ConfirmSevere(
                    "La cabecera del poste (" + cfg.Height.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in) queda MÁS BAJA que el nivel de carga superior (" + topLevelY.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in).\n\nEl larguero/tarima superior sobresaldría por encima del poste. Sube la altura de la cabecera "
                        + "o revisa los niveles de las bahías vecinas.",
                    this);
            }
            else if (resolvedHeight > 0.0 && Math.Abs(cfg.Height - resolvedHeight) > 0.5)
            {
                SelectiveCabeceraHeightPrompt.Inform(
                    "La altura de la cabecera (" + cfg.Height.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in) difiere del alto resuelto del poste (" + resolvedHeight.ToString("0.##", CultureInfo.InvariantCulture)
                        + " in).\n\nEl sistema se puede desconfigurar: el frontal coloca los largueros para el alto resuelto, "
                        + "así que el corte lateral y el frontal pueden dejar de coincidir.",
                    this);
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

        /// <summary>
        /// The CABECERA depth of fondo <paramref name="fondoIndex"/>: its own "Fondo de cabecera" override when set,
        /// else the rule (cabecera = tarima − 6"). This is what a per-post custom cabecera is drawn at, so it is what
        /// "Personalizar" must open with.
        /// <para>
        /// It reads the LIVE editor state (<see cref="SelectiveEditorState.CabeceraDepthOfFondo"/>), which is the
        /// gate-4 authority <c>FondoIndex → CabeceraDepth</c> — no new authority, and no dependence on
        /// <c>lastSystem</c>, whose fondo 0 is what made the configurator open at the wrong depth. The final depth of
        /// what gets stored is still imposed per TARGET fondo by <c>ApplyCabeceraToTargets</c>; this only decides what
        /// the user is shown.
        /// </para>
        /// </summary>
        private double ResolvedCabeceraFondo(int fondoIndex)
        {
            var cabecera = state.CabeceraDepthOfFondo(fondoIndex);
            if (cabecera > 0.0) return cabecera;

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
            if (!CommitPendingEditors()) return; // frontera: siempre consume BuildSystem (C4)

            // The safety grid needs the resolved matrix dimensions and the fondo count so the tope picker can offer the
            // real fondos (doble/triple profundidad).
            var depthCount = Math.Max(1, fondoMatrices.Count); // comprometido, no el texto de la caja (INV-13)
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
            if (!CommitPendingEditors()) return; // frontera: comando explícito de escritura (C4)

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
            LoadFrontEditor(); // the frente panel describes the frente of the selected cell
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
        /// <summary>
        /// Gesto propio de «Frentes» y acción de «Recalcular tramo».
        /// <para>
        /// Sin edición pendiente NO redimensiona nada: es la diferencia que exige O-43-02 entre tabular por el campo y
        /// editarlo. Comprometer aquí los cuatro campos es obligatorio porque la reconciliación posterior hace
        /// <c>Show</c> sobre los hermanos.
        /// </para>
        /// </summary>
        private void ApplyBayCount()
        {
            if (!initialized) return;
            if (!TryCommitEditedCell(out _)) return; // don't discard typed cell input on a frente-count change
            CommitStructuralGesture(pendingBayCount);
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

        /// <summary>The reach chosen in the frente section: it governs the three "Aplicar" buttons alike, so there is
        /// ONE scope idea in the editor even though each property is written on its own.</summary>
        private SelectiveFrontApplyScope FrontScope()
            => FrontScopeBox.SelectedIndex == 2 ? SelectiveFrontApplyScope.All
                : FrontScopeBox.SelectedIndex == 1 ? SelectiveFrontApplyScope.Selected
                : SelectiveFrontApplyScope.Front;

        /// <summary>
        /// Run ONE frente-wide operation over the chosen reach and the target fondos: commit the live matrix, let
        /// Application write every destination, repaint and recompute ONCE (I-43, gate 8A).
        /// </summary>
        private void ApplyFrontOperation(string logName, string describeAs, Func<SelectiveFrontApplyScope, SelectiveFrontApplyResult> apply)
        {
            if (!CommitPendingEditors()) return; // frontera: comando explícito de escritura (C4)

            var scope = FrontScope();
            using (DeferRecompute())
            {
                SaveWorkingToSelected();
                FrontApplyLog.Add(logName + ":" + scope);
                var result = apply(scope);
                RenderMatrix();
                LoadCellEditor();
                Recompute();
                pendingWarning = result.Describe(describeAs, restore: false);
            }
        }

        /// <summary>Write ONLY "larguero a piso". The elevations of those frentes are left exactly as they were —
        /// turning the beam on across a run must not flatten elevations the user set one by one.</summary>
        private void ApplyFrontFloorBeam_Click(object sender, RoutedEventArgs e)
        {
            var value = FrontFloorBeamCheck.IsChecked == true;
            ApplyFrontOperation("Piso", "el larguero a piso", scope => state.ApplyFloorBeamToTargets(scope, selBay, value));
        }

        /// <summary>Write ONLY the elevation. The "larguero a piso" flags of those frentes are left as they were.</summary>
        private void ApplyFrontRise_Click(object sender, RoutedEventArgs e)
        {
            if (!UiSupport.TryNum(FrontRiseBox.Text, out var rise) || rise < 0.0)
            {
                SetStatus("Elevación del larguero a piso inválida (número ≥ 0).", true);
                return;
            }

            ApplyFrontOperation("Elevacion", "la elevación", scope => state.ApplyFloorBeamRiseToTargets(scope, selBay, rise));
        }

        /// <summary>Write ONLY the level count — an exact number, so every frente in reach ends with the same one.</summary>
        private void ApplyFrontLevels_Click(object sender, RoutedEventArgs e)
        {
            if (!TryInt(FrontLevelsBox.Text, out var levels) || levels < 1)
            {
                SetStatus("Número de niveles inválido (mínimo 1).", true);
                return;
            }

            ApplyFrontOperation("Niveles", "los niveles", scope => state.ApplyLevelCountToTargets(scope, selBay, levels));
        }

        /// <summary>Show the frente properties of the cell currently selected — the frente on screen is what the panel
        /// describes, whatever fondos an edit will land on.</summary>
        private void LoadFrontEditor()
        {
            if (FrontFloorBeamCheck == null || selBay < 0 || selBay >= bays.Count) return;

            FrontHeader.Text = string.Format(CultureInfo.InvariantCulture, "Frente {0}", selBay + 1);
            FrontFloorBeamCheck.IsChecked = selBay < floorBeams.Count && floorBeams[selBay];
            var rise = state.FloorBeamRiseOverrideAt(selectedFondo, selBay) ?? legacyFloorBeamRise;
            FrontRiseBox.Text = rise.ToString("0.###", CultureInfo.InvariantCulture);
            FrontLevelsBox.Text = bays[selBay].Count.ToString(CultureInfo.InvariantCulture);
        }

        private void ApplyCell_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Cell);
        private void ApplySelected_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Selected);
        private void ApplyRow_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Row);
        private void ApplyColumn_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Column);
        private void ApplyAll_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.All);

        // ---- Fondos destino (I-43): the second axis, edited next to "Editando fondo" ----

        /// <summary>True while the dropdown is being rebuilt, so repopulating its checkboxes does not look like clicks.</summary>
        private bool buildingTargetFondos;

        /// <summary>
        /// Rebuild the "Fondos destino" dropdown and its closed caption (I-43, gate 8A).
        /// <para>
        /// It is a popup of check boxes rather than a text field: the Owner should never have to learn a syntax like
        /// <c>1,3-4</c> to say "fondos 1 and 3". "Actual" is a live choice, not a snapshot — it keeps following the
        /// fondo on screen — while a deliberate set of fondos is preserved when navigating, which is the gate-3 rule.
        /// With a single fondo the whole thing is hidden: the two axes would always coincide.
        /// </para>
        /// </summary>
        private void RefreshTargetFondos()
        {
            if (TargetFondosList == null) return;

            var count = state.FondoCount;
            TargetFondosPanel.Visibility = count > 1 ? Visibility.Visible : Visibility.Collapsed;

            buildingTargetFondos = true;
            TargetFondosList.Children.Clear();

            var fondos = state.TargetFondos.Fondos;
            // "Actual" is a MODE, not a set that happens to match: an explicit "Fondo 2" reads as itself even while
            // fondo 2 is the one on screen (I-43, gate 8A correction).
            var isCurrentOnly = state.TargetMode == SelectiveTargetMode.FollowCurrent;
            var isAll = state.TargetMode == SelectiveTargetMode.All;

            // "Actual" and "Todos" are ACTIONS, not ticks. A check box can be un-ticked, and neither "no mode" nor
            // "un-select all" exists in the model, so a tick that can be turned off would let the UI show a state the
            // editor cannot hold. Which one is in force is read from the closed caption, not from a mark here.
            var actual = new Button
            {
                Content = isCurrentOnly ? "✓ Actual" : "Actual",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 4),
                Padding = new Thickness(6, 2, 6, 2),
                ToolTip = "Aplicar solo en el fondo que estás editando; sigue al fondo visible."
            };
            actual.Click += (s, e) => { if (!buildingTargetFondos) { state.FollowCurrentFondo(); RefreshTargetFondos(); RememberTargetFondos(); } };
            TargetFondosList.Children.Add(actual);

            var todos = new Button
            {
                Content = isAll ? "✓ Todos" : "Todos",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(6, 2, 6, 2),
                ToolTip = "Aplicar en todos los fondos."
            };
            // FollowAllFondos, not a snapshot of today's indices: "Todos" must keep meaning every fondo when the rack
            // grows, and it is the intent that gets remembered between openings.
            todos.Click += (s, e) => { if (!buildingTargetFondos) { state.FollowAllFondos(); RefreshTargetFondos(); RememberTargetFondos(); } };
            TargetFondosList.Children.Add(todos);

            for (var k = 0; k < count; k++)
            {
                var index = k;
                var item = new CheckBox
                {
                    Content = "Fondo " + (k + 1).ToString(CultureInfo.InvariantCulture),
                    // While "Actual" is the mode the individual boxes read UNCHECKED, even though the set matches the
                    // fondo on screen: otherwise the one that matches looks already chosen and clicking it — the very
                    // gesture that means "I want THIS fondo, explicitly" — would appear to do nothing.
                    IsChecked = !isCurrentOnly && state.TargetFondos.Contains(k),
                    Margin = new Thickness(0, 0, 0, 2)
                };
                void Toggle(object s, RoutedEventArgs e)
                {
                    if (buildingTargetFondos) return;

                    // The boxes are the VISIBLE truth, so a toggle starts from what they show, never from the mode's
                    // internal set: under "Actual" they read EMPTY (ticking the first one begins a fresh explicit set,
                    // instead of silently adding a second target next to the followed fondo), while under "Todos" they
                    // read ALL TICKED (un-ticking one must mean "every fondo except this", not "nothing").
                    var chosen = state.TargetMode == SelectiveTargetMode.FollowCurrent
                        ? new List<int>()
                        : state.TargetFondos.Fondos.ToList();

                    if (item.IsChecked == true) { if (!chosen.Contains(index)) chosen.Add(index); }
                    else chosen.Remove(index);

                    // Un-ticking the last one leaves no explicit target: fall back to "Actual", which is a real mode,
                    // instead of inventing a singleton the user never chose.
                    if (chosen.Count == 0) { state.FollowCurrentFondo(); RefreshTargetFondos(); RememberTargetFondos(); return; }
                    SetTargets(chosen);
                }

                item.Checked += Toggle;
                item.Unchecked += Toggle;
                TargetFondosList.Children.Add(item);
            }

            TargetFondosButton.Content = isCurrentOnly
                ? "Actual"
                : isAll
                    ? "Todos"
                    : fondos.Count == 1
                        ? "Fondo " + (fondos[0] + 1).ToString(CultureInfo.InvariantCulture)
                        : "Fondos " + string.Join(", ", fondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture)));
            buildingTargetFondos = false;
        }

        /// <summary>Choose the target fondos. Picking targets changes no geometry, so nothing recomputes here.</summary>
        private void SetTargets(IEnumerable<int> fondos)
        {
            state.SetTargetFondos(fondos);
            RefreshTargetFondos();
            RememberTargetFondos();
        }

        /// <summary>
        /// Store the "Fondos destino" INTENT as an editor preference so the next opening starts where the user left
        /// off (gate 8 correction). Best-effort and atomic through the existing per-user settings; it never touches
        /// the design, so no recompute and no unsaved-changes state is involved.
        /// </summary>
        private void RememberTargetFondos()
        {
            targetPreference = SelectiveTargetPreference.Capture(state);
            if (settingsGateway == null) return;

            // Re-read before writing: the settings file holds OTHER preferences (block library, design library) that
            // another window may have changed since this editor opened, and saving our stale snapshot would undo them.
            var current = settingsGateway.Load() ?? new UserSettings();
            current.SelectiveTargetFondos = targetPreference.Encode();
            settingsGateway.Save(current);
        }

        /// <summary>
        /// Aim the editor at the remembered choice, resolved against the fondos the rack ACTUALLY has. Called only
        /// when a rack is opened — a fresh editor, or a design loaded into it. It is deliberately NOT called on every
        /// fondo-count change: resizing drops targets destructively (gate 4), and re-resolving there would resurrect
        /// indices the user had already reshaped away.
        /// </summary>
        private void ApplyStoredTargetPreference() => targetPreference.ApplyTo(state);
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowBom_Click(object sender, RoutedEventArgs e)
        {
            // El commit abre y CIERRA su propio Defer, así que lastSystem ya está recalculado cuando se lee aquí
            // abajo; con el commit diferido el BOM describiría el estado anterior (C4).
            if (!CommitPendingEditors()) return;

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
            if (!CommitPendingEditors()) return; // frontera: comando explícito de escritura (C4)
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
            if (!CommitPendingEditors()) return; // frontera: lo pendiente se compromete ANTES de construir (C4)
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
        /// what a bulk gesture must never do is mutate twice.</summary>
        internal List<string> FrontApplyLog { get; } = new List<string>();

        /// <summary>The editor state — a test seam (I-43, InternalsVisibleTo), matching the one the safety grids expose.</summary>
        internal SelectiveEditorState EditorState => state;

        /// <summary>Test seam: las instancias que la preview acaba de construir. La preview dibuja el fondo VISIBLE,
        /// asi que esto es lo que permite comprobar que la cabecera que pinta es la de ESE fondo (I-43, O-43-03).</summary>
        internal IReadOnlyList<HeaderBlockInstance> PreviewInstancesForTest => lastInstances;

        /// <summary>Refresh the cell + frente panels from the current selection — a test seam (I-43, gate 8A) for the
        /// repaint a real click performs after moving the selection.</summary>
        internal void LoadCellEditorForTest() => LoadCellEditor();

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

        /// <summary>Test seam: recompute the "Personalizada / Por defecto" legend, exactly as every handler that
        /// changes the visible fondo or the selected post does. Needed only when a test writes a cabecera straight
        /// into the state instead of going through the configurator dialog (which cannot run headless).</summary>
        internal void RefreshPostStatusForTest() => UpdatePostStatus();

        /// <summary>Test seam: the cabecera depth "Personalizar" would open with right now — the VISIBLE fondo's,
        /// committed first, exactly as <see cref="CustomizePost_Click"/> computes it. The click itself cannot be
        /// driven from a test because the configurator is modal (ShowDialog blocks the STA thread).</summary>
        /// <summary>Test seam: la ALTURA con la que "Personalizar" abriria ahora mismo, para el poste dado.</summary>
        internal double CustomizeSeedHeightForTest(int postIndex) => ResolvedPostHeight(postIndex);

        /// <summary>Test seam: el ancho completo del frente que "Medio frente" pasaria al dialogo.</summary>
        internal double TramosFullWidthForTest(int bay)
            => lastSystem != null && bay >= 0 && bay < lastSystem.Bays.Count ? lastSystem.Bays[bay].BeamLength : 0.0;

        /// <summary>Test seam: la mitad post-configurador de "Personalizar", que un modal impide ejecutar.</summary>
        internal void ApplyCustomizedCabeceraForTest(int postIndex, RackFrameConfiguration cfg, double globalPeralte)
            => ApplyCustomizedCabecera(
                postIndex, cfg, ResolvedCabeceraFondo(selectedFondo), ResolvedPostHeight(postIndex), globalPeralte);

        internal double CustomizeSeedDepthForTest()
        {
            SaveWorkingToSelected();
            return ResolvedCabeceraFondo(selectedFondo);
        }

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
            // INV-13: NADA de esto sale de una caja de texto. Las cuatro son editores de un valor pendiente; la
            // autoridad son los slots y la lista de fondos, así que un texto tecleado y no comprometido no puede
            // llegar al documento ni redimensionar el rack (I-43, gate 8.6C). Safety se filtra + copia aquí para que
            // su propiedad siga siendo del editor (I-22).
            var depthCount = Math.Max(1, fondoMatrices.Count);
            var (workingDepth, workingCabecera) = CommittedDepthCabecera();
            var fondo = workingDepth;
            var inputs = new SelectiveDesignInputs
            {
                PostId = postId,
                PostPeralte = postPeralte,
                PalletTolerance = tolerance,
                VerticalClearance = clearance,
                FloorBeamRise = legacyFloorBeamRise, // legacy field only: every frente carries its own value now
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
            // 0 is a legitimate rise ("no elevation at all"), so only a NEGATIVE value falls back to the default.
            legacyFloorBeamRise = design.FloorBeamRise >= 0.0 ? design.FloorBeamRise : SelectiveRackDefaults.DefaultFloorBeamRise;
            // Una carga REEMPLAZA todo el estado: lo que hubiera pendiente se descarta explícitamente (Reset), que
            // es distinto de Show — aquí no hay nada que preservar (C4, fila "Carga").
            pendingDepth.Reset((design.PalletDepth > 0.0 ? design.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth).ToString("0.###", CultureInfo.InvariantCulture));
            pendingFondos.Reset(Math.Max(1, design.DepthCount).ToString(CultureInfo.InvariantCulture));
            pendingCabecera.Reset(string.Empty);

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
            // Every frente gets a DIRECT elevation. A document written before gate 8A carries only the run-wide value,
            // and the drawing it described used it everywhere, so each frente materializes it as its own; from here on
            // the frente is the authority (I-43, gate 8A).
            RestoreWorkingFrom(fondoMatrices[0]);
            // DESPUES de restaurar: asi la fila viva ya es la del fondo 0 y la materializacion opera sobre el estado
            // definitivo. Materialize cubre ademas todos los slots, de modo que un documento legacy queda con valor
            // directo en cada frente de cada fondo (I-43, gate 8.6D, INV-12).
            state.MaterializeFloorBeamRises(legacyFloorBeamRise);
            // NOW the fondos of this rack are known, so the remembered choice can be resolved against them: an
            // explicit set keeps the fondos this rack has, and "Todos"/"Actual" re-aim at it (gate 8 correction).
            ApplyStoredTargetPreference();
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

            pendingBayCount.Reset(CommittedBayCountText());
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
            if (!CommitPendingEditors()) return; // frontera: persiste el diseño (C4)
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
