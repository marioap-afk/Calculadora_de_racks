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
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI.Controls;
using RackCad.UI.Editor;

namespace RackCad.UI
{
    /// <summary>
    /// The Push Back system editor (initiative I-18b, increment 3a). It is a THIN shell over the pure Application model: the
    /// only editable authority is <see cref="PushBackEditorState"/>, the only recompute path is
    /// <see cref="PushBackEditorDesignAssembler"/>, and identity + the insert/update contract live on a shared
    /// <see cref="RackEditorSession{TDesign,TSystem}"/>. The window does nothing but read controls, delegate mutations to the
    /// state, ask the assembler to recompute, render the result and produce a <see cref="PushBackInsertionRequest"/>. It never
    /// computes geometry, BOM, slope, topes or persistence itself, and it references no AutoCAD type — the Plugin host (a later
    /// increment) draws the payload. It never reuses <see cref="RackDynamicSystemWindow"/> as its editor.
    /// Numeric entry uses <see cref="NumericField"/> (localized parse + range + visible error); a control in error blocks the
    /// recompute, so a stale model is never inserted/saved silently — <see cref="CurrentInputsAreValid"/> gates every action.
    /// </summary>
    public partial class RackPushBackSystemWindow : Window
    {
        private static readonly double[] RearPeraltes = { 3.0, 3.5, 4.0, 4.5, 5.0, 5.5, 6.0 };
        private static readonly string[] ViewOptions = { "Lateral", "Frontal entrada/salida", "Frontal posterior", "Planta" };

        private readonly RackCatalog catalog;
        private readonly PushBackEditorState state = new PushBackEditorState();
        private readonly PushBackEditorDesignAssembler assembler;
        private readonly RackEditorSession<PushBackDesign, PushBackSystem> session;
        private readonly List<SelectiveSafetySelection> safetySelections = new List<SelectiveSafetySelection>();
        private readonly bool canInsertInAutoCad;

        private bool isEditingExisting;
        private bool suppressSync;
        private bool hasValidModel;          // ever produced a valid model (a preview reference exists)
        private bool currentInputsAreValid;  // the CURRENT controls recomputed to a valid model
        private RackProject sourceProject;
        private PushBackEditorComputation lastComputation; // the LAST VALID computation (only replaced on a valid build)
        private SelectionMatrixModel topeModel;
        private List<int> topeShape = new List<int>();
        private SelectionMatrixModel cellSelectionModel; // the visible multi-selection; the DynamicFrontMatrix is the authority
        private List<int> cellSelectionShape = new List<int>();

        // The informative card matrix (PB-VAL-01 round 3). Cards are looked up by (front, level) for in-place updates;
        // the grid stores NO state of its own — every card derives from PushBackMatrixCardModel over the state.
        private readonly Dictionary<(int Front, int Level), (Border Border, TextBlock Text)> matrixCards
            = new Dictionary<(int, int), (Border, TextBlock)>();

        private static readonly Brush CardPrimaryStroke = UiSupport.FrozenBrush(Color.FromRgb(0xFF, 0xD1, 0x66));   // primary cell
        private static readonly Brush CardIncludedStroke = UiSupport.FrozenBrush(Color.FromRgb(0x5B, 0x8D, 0xEF)); // in multi-selection
        private static readonly Brush CardNormalStroke = UiSupport.FrozenBrush(Color.FromRgb(0xD8, 0xDE, 0xE6));
        private static readonly Brush CardLabelBrush = UiSupport.FrozenBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));
        private static readonly Brush CardTextBrush = UiSupport.FrozenBrush(Color.FromRgb(0x41, 0x51, 0x61));
        private static readonly Brush CardGhostBrush = UiSupport.FrozenBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));
        private static readonly Brush CardSelectedFill = UiSupport.FrozenBrush(Color.FromRgb(0xF3, 0xF8, 0xFD));
        private static readonly Brush CardGhostFill = UiSupport.FrozenBrush(Color.FromRgb(0xF1, 0xF4, 0xF8));
        private static readonly Brush CardTopeOffBrush = UiSupport.FrozenBrush(Color.FromRgb(0xC0, 0x5B, 0x5B));

        public RackPushBackSystemWindow()
            : this(false)
        {
        }

        public RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();

            // The session owns the catalog (loaded once), the identity (GUID + name) and the insert/update contract; its
            // coalescing gate drives THIS window's Recompute so programmatic bursts collapse to one pass.
            session = new RackEditorSession<PushBackDesign, PushBackSystem>(recompute: Recompute, newIdFactory: newIdFactory);
            catalog = session.Catalog;
            assembler = new PushBackEditorDesignAssembler(catalog);

            WeightUnitBox.ItemsSource = new[] { "kg", "lb" };
            WeightUnitBox.SelectedIndex = 0;
            DimensionsBox.ItemsSource = new[] { "Ninguna", "Mínima", "Estándar", "Detallada" };
            DimensionsBox.SelectedIndex = 0;
            RearPeralteBox.ItemsSource = RearPeraltes;
            ViewBox.ItemsSource = ViewOptions;
            ViewBox.SelectedIndex = 0;
            PostBox.SetCatalogEntries(catalog?.PostProfiles, catalog?.Defaults?.Post);
            CellInOutBeamBox.ItemsSource = InOutBeamOptions();
            CellIntermediateBeamBox.ItemsSource = IntermediateBeamOptions();

            LoadNew();
        }

        // ---- Test seams (internal) --------------------------------------------------------------------------------

        internal RackEditorSession<PushBackDesign, PushBackSystem> Session => session;
        internal PushBackEditorState State => state;
        internal PushBackEditorDesignAssembler Assembler => assembler;
        internal SelectionMatrixModel TopeModel => topeModel;
        internal SelectionMatrixModel CellSelectionModel => cellSelectionModel;

        /// <summary>The plan currently drawn in the preview (the selected view/corte). For the lateral view this is the
        /// SELECTED corte's plan, so changing "Corte 1"→"Corte 2" changes what the preview shows.</summary>
        internal DynamicSystemPlan CurrentPreviewPlan
        {
            get
            {
                var (view, section) = SelectedView();
                return PlanFor(view, section);
            }
        }
        /// <summary>The SEMANTIC primitives the preview is currently drawing (role/piece/kind/coordinates) — the
        /// round-3 seam: tests assert content, never pixels.</summary>
        internal PushBackPreviewModel CurrentPreviewModel
        {
            get
            {
                var (view, section) = SelectedView();
                return BuildPreviewModel(PlanFor(view, section), view);
            }
        }

        internal PushBackEditorComputation LastComputation => lastComputation;
        internal bool HasValidModel => hasValidModel;
        internal bool CurrentInputsAreValid => currentInputsAreValid;
        internal IReadOnlyList<SelectiveSafetySelection> SafetySelections => safetySelections;

        /// <summary>The safety families offered by the dialog: every applicable family EXCEPT entrance guides (GUIA), which
        /// Push Back never admits — so GUIA is not even a visible option.</summary>
        internal IReadOnlyList<SafetyElementCatalogEntry> SafetyElementsForDialog()
            => (catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>())
                .Where(element => element != null && !SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.GuiaType))
                .ToList();

        /// <summary>The library project a "Guardar en biblioteca" would write (the active Push Back payload + the opened
        /// project's I-11 metadata), or NULL when the CURRENT controls are invalid — a stale model is never saved.</summary>
        internal RackProject BuildLibraryProjectForTest()
            => !currentInputsAreValid || lastComputation?.Design == null
                ? null
                : RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject);

        // ---- Public contract (derived from the session) -----------------------------------------------------------

        public bool InsertRequested => session.InsertRequested;
        public bool UpdateOnly => session.UpdateOnly;
        public string RackId => session.Identity.Id;
        public string RackName => session.Identity.Name;
        public string InsertView => session.InsertView;
        public int InsertSection => session.InsertSection;
        public RackProject SourceProjectToInsert => sourceProject;
        public RackInsertionRequest InsertionRequest => session.InsertionRequest;
        public PushBackSystem SystemToInsert => (session.InsertionRequest as PushBackInsertionRequest)?.System;
        public PushBackDesign DesignToInsert => (session.InsertionRequest as PushBackInsertionRequest)?.Design;

        // ---- Public load paths ------------------------------------------------------------------------------------

        /// <summary>A brand-new Push Back system: one dynamic-default front, rear peralte 3.5, topes active, selection (0,0),
        /// standard modular baseline. No identity is forced until insert; only "Insertar" is offered (no "Actualizar").</summary>
        public void LoadNew()
        {
            var inputs = state.LoadNew();
            sourceProject = null;
            isEditingExisting = false;
            session.Identity.Adopt(null, null);
            LoadFromModel(inputs, string.Empty);

            // PB-VAL-04: a new rack opens with the catalog-driven safety defaults, exactly like the dynamic editor does.
            // The Push Back authority is the single filter (drops GUIA, deep-copies, restricts every family to the LOW end),
            // so no hard-coded list lives here and the shared defaults are never mutated. Seeded AFTER LoadFromModel,
            // which resets the selections for the loaded design.
            safetySelections.Clear();
            safetySelections.AddRange(new PushBackSafetyAuthority(catalog).Defaults());
            Recompute();
        }

        /// <summary>A design opened from the library, edited as a NEW insert: keeps the suggested name, carries the source
        /// project for I-11 metadata, mints a FRESH GUID on insert (its library GUID is never reused as identity). Insert
        /// only, no update.</summary>
        public void LoadDesignForNew(PushBackDesign design, string rackName, RackProject sourceProject = null)
        {
            if (design == null) return;
            var inputs = state.LoadFromDesign(design, assembler.Resolver);
            this.sourceProject = sourceProject;
            isEditingExisting = false;
            session.Identity.Adopt(null, rackName); // no id -> a fresh GUID is minted on insert
            LoadFromModel(inputs, rackName);
        }

        /// <summary>A system opened from the DWG (RACKEDITAR): keeps its GUID + name, carries the source project, enables
        /// "Actualizar" and also allows requesting an additional linked view.</summary>
        public void LoadExisting(PushBackDesign design, string rackId, string rackName, RackProject sourceProject = null)
        {
            if (design == null) return;
            var inputs = state.LoadFromDesign(design, assembler.Resolver);
            this.sourceProject = sourceProject;
            isEditingExisting = true;
            session.Identity.Adopt(rackId, rackName);
            LoadFromModel(inputs, rackName);
        }

        private void LoadFromModel(PushBackEditorInputs inputs, string rackName)
        {
            suppressSync = true;
            try
            {
                NameBox.Text = rackName ?? string.Empty;
                var pallet = inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
                FrontBox.SetNumber(pallet.Front);
                DepthBox.SetNumber(pallet.Depth);
                PalletHeightBox.SetNumber(pallet.Height);
                WeightBox.SetNumber(pallet.Weight);
                WeightUnitBox.SelectedItem = string.IsNullOrWhiteSpace(pallet.WeightUnit) ? "kg" : pallet.WeightUnit;
                PalletsDeepBox.SetNumber(Math.Max(2, inputs.PalletsDeep));
                ToleranceBox.SetNumber(inputs.PalletTolerance > 0.0 ? inputs.PalletTolerance : DynamicRackDefaults.DefaultPalletTolerance);
                PostBox.SelectedId = string.IsNullOrWhiteSpace(inputs.PostCatalogId) ? catalog?.Defaults?.Post : inputs.PostCatalogId;
                PostPeralteBox.SetNumber(inputs.PostPeralte);
                BeamDepthBox.SetNumber(inputs.BeamDepth > 0.0 ? inputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth);
                SaqueBox.SetNumber(state.RearTopeSaque);

                var options = inputs.Annotations ?? new DynamicAnnotationOptions();
                NumberFrontsCheck.IsChecked = options.NumberFronts;
                NumberLevelsCheck.IsChecked = options.NumberLevels;
                DrawRackNameCheck.IsChecked = options.DrawRackName;
                AnnotationScaleBox.SetNumber(options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0);
                DimensionsBox.SelectedIndex = Math.Min((int)DimensionDetail.Detailed, Math.Max(0, (int)options.Dimensions));

                safetySelections.Clear();
                foreach (var safety in inputs.SafetySelections ?? Enumerable.Empty<SelectiveSafetySelection>())
                {
                    if (safety != null) safetySelections.Add(safety.DeepCopy());
                }

                RefreshFrontSelector();
                SyncTopeMatrix();
                SyncCellSelectionMatrix();
                RenderPushBackMatrix();
                LoadSelectedFront();
            }
            finally
            {
                suppressSync = false;
            }

            Recompute();
        }

        // ---- Load the per-front / per-cell panel from the selected cell -------------------------------------------

        private void LoadSelectedFront()
        {
            state.NormalizeSelection();
            var frontIndex = state.Structure.SelectedFrontIndex;
            var levelIndex = state.Structure.SelectedLevelIndex;
            if (frontIndex < 0 || frontIndex >= state.Structure.Count) return;
            var front = state.Structure.Fronts[frontIndex];
            var cell = front.Cells.Count > 0 ? front.Cells[Math.Max(0, Math.Min(levelIndex, front.Cells.Count - 1))] : new DynamicEditorCell();
            var push = state.Cell(frontIndex, levelIndex);

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                FrontCountBox.SetNumber(state.Structure.Count);
                if (SelectedFrontBox.Items.Count == state.Structure.Count) SelectedFrontBox.SelectedIndex = frontIndex;
                RefreshLevelSelector();
                if (SelectedLevelBox.Items.Count == Math.Max(1, front.LoadLevels)) SelectedLevelBox.SelectedIndex = levelIndex;

                PositionsBox.SetNumber(front.PalletCount);
                LevelsBox.SetNumber(front.LoadLevels);
                FondosBox.SetNumber(front.PalletsDeep);
                DepthStartBox.SetNumber(front.DepthStartPosition);
                FirstLevelHeightBox.SetNumber(front.FirstLevelHeight);

                CellPalletFrontBox.SetNumber(cell.PalletFront);
                CellPalletHeightBox.SetNumber(cell.PalletHeight);
                CellPalletWeightBox.SetNumber(cell.PalletWeight);
                CellClearBox.SetNumber(cell.ClearHeight);
                CellInOutBeamBox.SelectedValue = cell.InOutBeamCatalogId;
                SetPeralteOptions(CellInOutPeralteBox, cell.InOutBeamCatalogId, cell.InOutBeamDepth);
                CellBeamLengthOverrideBox.SetNumber(cell.BeamLengthOverride); // null -> blank (optional field)
                CellIntermediateBeamBox.SelectedValue = cell.IntermediateBeamCatalogId;
                SetPeralteOptions(CellIntermediatePeralteBox, cell.IntermediateBeamCatalogId, cell.IntermediateBeamDepth);

                RearPeralteBox.SelectedItem = RearPeraltes.FirstOrDefault(p => Math.Abs(p - push.HighEndBeamPeralte) < 1e-6);
                if (RearPeralteBox.SelectedItem == null) RearPeralteBox.SelectedItem = PushBackDefaults.HighEndBeamDefaultPeralte;
                RearTopeActiveCheck.IsChecked = push.RearTopeEnabled;
            }
            finally
            {
                suppressSync = wasSuppressed;
            }
        }

        private void RefreshFrontSelector()
        {
            SelectedFrontBox.ItemsSource = Enumerable.Range(1, Math.Max(1, state.Structure.Count))
                .Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();
            SelectedFrontBox.SelectedIndex = Math.Max(0, Math.Min(state.Structure.SelectedFrontIndex, state.Structure.Count - 1));
        }

        private void RefreshLevelSelector()
        {
            var frontIndex = state.Structure.SelectedFrontIndex;
            var levels = frontIndex >= 0 && frontIndex < state.Structure.Count ? Math.Max(1, state.Structure.Fronts[frontIndex].LoadLevels) : 1;
            SelectedLevelBox.ItemsSource = Enumerable.Range(1, levels).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();
            SelectedLevelBox.SelectedIndex = Math.Max(0, Math.Min(state.Structure.SelectedLevelIndex, levels - 1));
        }

        // ---- Recompute (the session's coalesced action) ----------------------------------------------------------

        private void RequestRecompute() => session.Recompute.Request();

        private void Recompute()
        {
            if (suppressSync) return;

            // A control in error blocks the model rebuild: do NOT touch state/session/computation/baseline, keep the last
            // valid model as a preview reference only, and disable every action.
            if (!AllFieldsValid(out var fieldError))
            {
                currentInputsAreValid = false;
                SetStatus(fieldError, true);
                RenderPreview();
                UpdateGuid();
                UpdateButtons();
                return;
            }

            CommitCurrentCell();
            state.RearTopeSaque = SaqueBox.Value ?? PushBackDefaults.RearTopeSaque;

            var computation = assembler.Build(state, ReadInputs());
            if (computation.IsValid)
            {
                session.SetModel(computation.Design, computation.System);
                assembler.AcceptComputation(state, computation); // advance the opaque baseline (never mutated by the window)
                lastComputation = computation;
                hasValidModel = true;
                currentInputsAreValid = true;
                SyncTopeMatrixIfShapeChanged();
                SyncCellSelectionMatrixIfShapeChanged();
                RenderPushBackMatrix();
                UpdateViewSelector();
                RenderPreview();
                SetStatus("Vista recalculada.", false);
            }
            else
            {
                currentInputsAreValid = false; // keep the last valid model; it is only a reference now
                SetStatus("No se pudo generar el sistema: " + computation.Error, true);
                RenderPreview();
            }

            UpdateGuid();
            UpdateButtons();
        }

        /// <summary>True when EVERY numeric control currently parses within its range; otherwise the first offending field's
        /// localized message is returned so the recompute can be blocked and the field marked.</summary>
        private bool AllFieldsValid(out string error)
        {
            error = null;
            NumericField firstError = null;
            var errorCount = 0;
            foreach (var field in AllNumericFields())
            {
                if (field.HasError)
                {
                    errorCount++;
                    if (firstError == null) firstError = field;
                }
            }

            if (firstError == null) return true;
            error = "Corrige los campos numéricos marcados: " + (firstError.ErrorMessage ?? "valor inválido")
                + (errorCount > 1 ? string.Format(CultureInfo.InvariantCulture, " (+{0} más)", errorCount - 1) : string.Empty);
            return false;
        }

        private NumericField[] AllNumericFields() => new[]
        {
            FrontBox, DepthBox, PalletHeightBox, WeightBox, PalletsDeepBox, ToleranceBox, PostPeralteBox,
            BeamDepthBox, SaqueBox, AnnotationScaleBox, FrontCountBox, PositionsBox, LevelsBox, FondosBox,
            DepthStartBox, FirstLevelHeightBox, CellPalletFrontBox, CellPalletHeightBox, CellPalletWeightBox,
            CellClearBox, CellBeamLengthOverrideBox
        };

        /// <summary>Read the cell panel (shared + Push Back values) and apply it to the primary selected cell + its front.</summary>
        private void CommitCurrentCell()
        {
            if (state.Structure.Count == 0) return;
            state.CommitEditorValues(ReadCellValues());
        }

        private PushBackEditorValues ReadCellValues()
        {
            var frontIndex = Math.Max(0, Math.Min(state.Structure.SelectedFrontIndex, state.Structure.Count - 1));
            var front = state.Structure.Fronts[frontIndex];
            var levelIndex = Math.Max(0, Math.Min(state.Structure.SelectedLevelIndex, front.Cells.Count - 1));
            var cell = front.Cells.Count > 0 ? front.Cells[levelIndex] : new DynamicEditorCell();
            var push = state.Cell(frontIndex, levelIndex);

            return new PushBackEditorValues
            {
                Dynamic = new DynamicEditorValues
                {
                    PalletCount = IntVal(PositionsBox, front.PalletCount),
                    LoadLevels = IntVal(LevelsBox, front.LoadLevels),
                    PalletsDeep = IntVal(FondosBox, front.PalletsDeep),
                    DepthStartPosition = IntVal(DepthStartBox, front.DepthStartPosition),
                    FirstLevelHeight = Val(FirstLevelHeightBox, front.FirstLevelHeight),
                    PalletFront = Val(CellPalletFrontBox, cell.PalletFront),
                    PalletHeight = Val(CellPalletHeightBox, cell.PalletHeight),
                    PalletWeight = Val(CellPalletWeightBox, cell.PalletWeight),
                    ClearHeight = Val(CellClearBox, cell.ClearHeight),
                    InOutBeamCatalogId = CellInOutBeamBox.SelectedValue as string ?? cell.InOutBeamCatalogId,
                    InOutBeamDepth = SelectedPeralte(CellInOutPeralteBox, cell.InOutBeamDepth),
                    BeamLengthOverride = CellBeamLengthOverrideBox.Value, // null when blank (optional override)
                    IntermediateBeamCatalogId = CellIntermediateBeamBox.SelectedValue as string ?? cell.IntermediateBeamCatalogId,
                    IntermediateBeamDepth = SelectedPeralte(CellIntermediatePeralteBox, cell.IntermediateBeamDepth)
                },
                HighEndBeamPeralte = RearPeralteBox.SelectedItem is double p ? p : push.HighEndBeamPeralte,
                RearTopeEnabled = RearTopeActiveCheck.IsChecked == true
            };
        }

        private PushBackEditorInputs ReadInputs()
        {
            var inputs = new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(
                    Val(FrontBox, 42.0), Val(DepthBox, 48.0), Val(PalletHeightBox, 60.0),
                    Val(WeightBox, 1000.0), WeightUnitBox.SelectedItem as string ?? "kg"),
                PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep),
                PostCatalogId = PostBox.SelectedId,
                PostPeralte = Val(PostPeralteBox, 0.0),
                PalletTolerance = Val(ToleranceBox, DynamicRackDefaults.DefaultPalletTolerance),
                BeamDepth = Val(BeamDepthBox, DynamicRackDefaults.DefaultBeamDepth),
                Annotations = new DynamicAnnotationOptions
                {
                    NumberFronts = NumberFrontsCheck.IsChecked == true,
                    NumberLevels = NumberLevelsCheck.IsChecked == true,
                    DrawRackName = DrawRackNameCheck.IsChecked == true,
                    AnnotationScale = Val(AnnotationScaleBox, 1.0),
                    Dimensions = (DimensionDetail)Math.Min((int)DimensionDetail.Detailed, Math.Max(0, DimensionsBox.SelectedIndex))
                }
            };
            // Only the authorized (GUIA-free) safety reaches the design; the assembler filters again, so a GUIA can never persist.
            foreach (var safety in assembler.AuthorizedSafety(safetySelections))
            {
                inputs.SafetySelections.Add(safety);
            }

            return inputs;
        }

        // ---- Rear topes matrix -----------------------------------------------------------------------------------

        private List<int> CurrentShape()
        {
            var shape = new List<int>();
            for (var f = 0; f < state.Structure.Count; f++)
            {
                shape.Add(Math.Max(1, state.Structure.Fronts[f].LoadLevels));
            }

            return shape;
        }

        private void SyncTopeMatrix()
        {
            if (topeModel != null) topeModel.CellChanged -= TopeCell_Changed;
            var shape = CurrentShape();
            var unselected = new List<SelectionMatrixCell>();
            for (var f = 0; f < shape.Count; f++)
            {
                for (var l = 0; l < shape[f]; l++)
                {
                    if (!state.Cell(f, l).RearTopeEnabled) unselected.Add(new SelectionMatrixCell(f, l));
                }
            }

            topeModel = SelectionMatrixModel.WithJaggedColumns(shape, unselected);
            topeModel.CellChanged += TopeCell_Changed;
            TopeMatrix.ColumnHeaders = Enumerable.Range(1, Math.Max(1, shape.Count)).Select(i => "F" + i).ToList();
            TopeMatrix.RowHeaders = Enumerable.Range(1, Math.Max(1, shape.DefaultIfEmpty(1).Max())).Select(i => "N" + i).ToList();
            TopeMatrix.Model = topeModel;
            topeShape = shape;
        }

        private void SyncTopeMatrixIfShapeChanged()
        {
            if (!CurrentShape().SequenceEqual(topeShape)) SyncTopeMatrix();
        }

        // ---- Cell selection matrix (the visible multi-selection; DynamicFrontMatrix stays the authority) ---------

        /// <summary>Rebuild the visible cell-selection matrix from the DynamicFrontMatrix selection: a checked cell is one in
        /// the edit selection. Only a structural change rebuilds it; a click updates a single cell in place.</summary>
        private void SyncCellSelectionMatrix()
        {
            if (cellSelectionModel != null) cellSelectionModel.CellChanged -= CellSelection_Changed;
            var shape = CurrentShape();
            var unselected = new List<SelectionMatrixCell>();
            for (var f = 0; f < shape.Count; f++)
            {
                for (var l = 0; l < shape[f]; l++)
                {
                    if (!state.Structure.IsSelected(f, l)) unselected.Add(new SelectionMatrixCell(f, l));
                }
            }

            cellSelectionModel = SelectionMatrixModel.WithJaggedColumns(shape, unselected);
            cellSelectionModel.CellChanged += CellSelection_Changed;
            CellSelectionMatrix.ColumnHeaders = Enumerable.Range(1, Math.Max(1, shape.Count)).Select(i => "F" + i).ToList();
            CellSelectionMatrix.RowHeaders = Enumerable.Range(1, Math.Max(1, shape.DefaultIfEmpty(1).Max())).Select(i => "N" + i).ToList();
            CellSelectionMatrix.Model = cellSelectionModel;
            cellSelectionShape = shape;
            UpdatePrimaryIndicator();
        }

        private void SyncCellSelectionMatrixIfShapeChanged()
        {
            if (!CurrentShape().SequenceEqual(cellSelectionShape)) SyncCellSelectionMatrix();
        }

        /// <summary>Update the model's checks (not its structure) to match the DynamicFrontMatrix selection — used when a combo
        /// replaces the selection with a single cell without changing the shape.</summary>
        private void RefreshCellSelectionChecks()
        {
            if (cellSelectionModel == null) return;
            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                for (var f = 0; f < cellSelectionModel.Columns; f++)
                {
                    for (var l = 0; l < cellSelectionModel.Rows; l++)
                    {
                        if (!cellSelectionModel.IsAbsent(f, l))
                        {
                            cellSelectionModel.SetSelected(f, l, state.Structure.IsSelected(f, l));
                        }
                    }
                }
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            UpdatePrimaryIndicator();
        }

        private void UpdatePrimaryIndicator()
            => CellSelectionPrimaryText.Text = state.Structure.Count > 0
                ? string.Format(CultureInfo.InvariantCulture, "Primaria: F{0} N{1} · {2} celda(s) seleccionada(s)",
                    state.Structure.SelectedFrontIndex + 1, state.Structure.SelectedLevelIndex + 1, state.Structure.SelectedCellCount)
                : string.Empty;

        /// <summary>A click on the cell-selection matrix: check → add the cell (it becomes primary); uncheck → remove it while
        /// others remain (never empty). The DynamicFrontMatrix is the single authority; the panel reloads the new primary.</summary>
        private void CellSelection_Changed(object sender, SelectionMatrixCellChangedEventArgs e)
        {
            if (suppressSync) return;
            var f = e.Cell.Column;
            var l = e.Cell.Row;

            if (!e.IsSelected && state.Structure.SelectedCellCount <= 1)
            {
                // Cannot leave the selection empty: revert the visual uncheck.
                var wasSuppressed = suppressSync;
                suppressSync = true;
                try { cellSelectionModel?.SetSelected(f, l, true); }
                finally { suppressSync = wasSuppressed; }
                SetStatus("Debe quedar al menos una celda seleccionada.", true);
                return;
            }

            CommitCurrentCell();
            state.ToggleCell(f, l, true); // add (checked) or remove (unchecked, others remain); the last touched is primary
            LoadSelectedFront();          // sync the front/level combos + load the (new) primary's values
            RefreshMatrixSelectionVisuals();
            UpdatePrimaryIndicator();
            RequestRecompute();
        }

        // ---- Informative card matrix (PB-VAL-01 round 3): THE central editing surface --------------------------

        /// <summary>Full imperative rebuild of the card matrix (structure or selection shape changed). One card per slot
        /// of the jagged grid padded to the tallest front; "Nivel 1" renders at the BOTTOM like the dynamic editor.</summary>
        private void RenderPushBackMatrix()
        {
            if (PushBackMatrixGrid == null)
            {
                return;
            }

            PushBackMatrixGrid.Children.Clear();
            PushBackMatrixGrid.RowDefinitions.Clear();
            PushBackMatrixGrid.ColumnDefinitions.Clear();
            matrixCards.Clear();

            var cards = PushBackMatrixCardModel.Build(state);
            if (cards.Count == 0)
            {
                return;
            }

            var fronts = state.Structure.Count;
            var levels = state.Structure.MaxLoadLevels();
            PushBackMatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56.0) });
            for (var f = 0; f < fronts; f++)
            {
                PushBackMatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(128.0) });
            }

            PushBackMatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var l = 0; l < levels; l++)
            {
                PushBackMatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Column headers: "Frente N" (click selects that front's primary cell).
            for (var f = 0; f < fronts; f++)
            {
                var captured = f;
                var header = new TextBlock
                {
                    Text = "Frente " + (f + 1).ToString(CultureInfo.InvariantCulture),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(2.0, 0.0, 2.0, 3.0),
                    Cursor = Cursors.Hand,
                    Foreground = f == state.Structure.SelectedFrontIndex ? CardPrimaryStroke : CardLabelBrush
                };
                header.MouseLeftButtonDown += (_, __) => SelectMatrixCell(captured, state.Structure.SelectedLevelIndex, false);
                AddMatrixElement(header, 0, f + 1);
            }

            // Row labels + cards, level 1 at the bottom.
            foreach (var card in cards)
            {
                var displayRow = levels - card.LevelIndex; // grid row 1..levels (row 0 = headers)
                if (card.FrontIndex == 0)
                {
                    AddMatrixElement(new TextBlock
                    {
                        Text = "Nivel " + (card.LevelIndex + 1).ToString(CultureInfo.InvariantCulture),
                        Foreground = CardLabelBrush,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(2.0, 4.0, 6.0, 4.0)
                    }, displayRow, 0);
                }

                var text = new TextBlock
                {
                    Text = card.Text,
                    TextAlignment = TextAlignment.Center,
                    FontSize = 10.5,
                    Foreground = card.IsActive ? CardTextBrush : CardGhostBrush
                };
                var border = new Border
                {
                    Margin = new Thickness(2.0),
                    Padding = new Thickness(5.0, 4.0, 5.0, 4.0),
                    Cursor = card.IsActive ? Cursors.Hand : Cursors.Arrow,
                    Child = text
                };
                StyleMatrixCard(border, text, card);

                var capturedFront = card.FrontIndex;
                var capturedLevel = card.LevelIndex;
                var isActive = card.IsActive;
                border.MouseLeftButtonDown += (_, __) =>
                {
                    // Plain click replaces the selection (the card becomes primary and loads the editor);
                    // Ctrl+click extends/shrinks the multi-selection. A ghost slot selects its front instead.
                    var extend = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                    SelectMatrixCell(capturedFront, isActive ? capturedLevel : state.Structure.SelectedLevelIndex, isActive && extend);
                };

                matrixCards[(card.FrontIndex, card.LevelIndex)] = (border, text);
                AddMatrixElement(border, displayRow, card.FrontIndex + 1);
            }
        }

        private void AddMatrixElement(UIElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            PushBackMatrixGrid.Children.Add(element);
        }

        private static void StyleMatrixCard(Border border, TextBlock text, PushBackMatrixCard card)
        {
            border.BorderBrush = card.IsPrimary ? CardPrimaryStroke : card.IsIncluded ? CardIncludedStroke : CardNormalStroke;
            border.BorderThickness = new Thickness(card.IsPrimary || card.IsIncluded ? 2.0 : 1.0);
            border.Background = !card.IsActive ? CardGhostFill : card.IsPrimary || card.IsIncluded ? CardSelectedFill : Brushes.White;
            text.Foreground = !card.IsActive ? CardGhostBrush : card.TopeActive ? CardTextBrush : CardTopeOffBrush;
        }

        /// <summary>Update ONE card's content + style in place (a value/tope change; the structure did not change).</summary>
        private void UpdateMatrixCard(int frontIndex, int levelIndex)
        {
            if (!matrixCards.TryGetValue((frontIndex, levelIndex), out var slot))
            {
                return;
            }

            var levels = Math.Max(1, state.Structure.Fronts[frontIndex].LoadLevels);
            var card = new PushBackMatrixCard
            {
                FrontIndex = frontIndex,
                LevelIndex = levelIndex,
                IsActive = levelIndex < levels,
                IsPrimary = frontIndex == state.Structure.SelectedFrontIndex && levelIndex == state.Structure.SelectedLevelIndex,
                IsIncluded = state.Structure.IsSelected(frontIndex, levelIndex),
                TopeActive = state.Cell(frontIndex, levelIndex).RearTopeEnabled,
                Text = levelIndex < levels ? PushBackMatrixCardModel.CardText(state, frontIndex, levelIndex) : "—"
            };
            slot.Text.Text = card.Text;
            StyleMatrixCard(slot.Border, slot.Text, card);
        }

        /// <summary>Restyle every card's selection border/background (the selection changed; content did not).</summary>
        private void RefreshMatrixSelectionVisuals()
        {
            foreach (var entry in matrixCards)
            {
                UpdateMatrixCard(entry.Key.Front, entry.Key.Level);
            }
        }

        /// <summary>The single card-click entry point (also the test seam): plain click replaces the selection with this
        /// cell and loads its editor; <paramref name="extend"/> toggles it in the multi-selection (the authority —
        /// <see cref="DynamicFrontMatrix.ToggleCell"/> — guarantees the selection can never become empty).</summary>
        internal void SelectMatrixCell(int frontIndex, int levelIndex, bool extend)
        {
            if (suppressSync || frontIndex < 0 || frontIndex >= state.Structure.Count)
            {
                return;
            }

            if (!AllFieldsValid(out var error))
            {
                SetStatus(error, true);
                return;
            }

            levelIndex = state.Structure.ClampLevel(frontIndex, levelIndex);
            CommitCurrentCell();
            state.ToggleCell(frontIndex, levelIndex, extend);
            LoadSelectedFront();
            RefreshCellSelectionChecks();      // keep the bulk check-matrix mirrored
            RefreshMatrixSelectionVisuals();
            UpdatePrimaryIndicator();
            RequestRecompute();
        }

        private void ApplyTope(int frontIndex, int levelIndex, bool active)
        {
            if (frontIndex < 0 || frontIndex >= state.Structure.Count) return;
            var levels = Math.Max(1, state.Structure.Fronts[frontIndex].LoadLevels);
            if (levelIndex < 0 || levelIndex >= levels) return;

            state.Cell(frontIndex, levelIndex).RearTopeEnabled = active;
            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                topeModel?.SetSelected(frontIndex, levelIndex, active);
                if (frontIndex == state.Structure.SelectedFrontIndex && levelIndex == state.Structure.SelectedLevelIndex)
                {
                    RearTopeActiveCheck.IsChecked = active;
                }
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            UpdateMatrixCard(frontIndex, levelIndex);
            RequestRecompute();
        }

        private void TopeCell_Changed(object sender, SelectionMatrixCellChangedEventArgs e)
        {
            if (suppressSync) return;
            ApplyTope(e.Cell.Column, e.Cell.Row, e.IsSelected);
        }

        private void RearTopeActive_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync) return;
            ApplyTope(state.Structure.SelectedFrontIndex, state.Structure.SelectedLevelIndex, RearTopeActiveCheck.IsChecked == true);
        }

        private void TopesAll_Click(object sender, RoutedEventArgs e) => SetAllTopes(true);

        private void TopesNone_Click(object sender, RoutedEventArgs e) => SetAllTopes(false);

        private void SetAllTopes(bool active)
        {
            for (var f = 0; f < state.Structure.Count; f++)
            {
                for (var l = 0; l < Math.Max(1, state.Structure.Fronts[f].LoadLevels); l++)
                {
                    state.Cell(f, l).RearTopeEnabled = active;
                }
            }

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                topeModel?.SetAll(active);
                RearTopeActiveCheck.IsChecked = active;
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            RefreshMatrixSelectionVisuals();
            RequestRecompute();
        }

        // ---- Structure handlers ----------------------------------------------------------------------------------

        private void FrontCount_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync || FrontCountBox.HasError) return;
            var requested = IntVal(FrontCountBox, state.Structure.Count);
            if (requested >= 1 && requested != state.Structure.Count)
            {
                MutateStructure(() => state.SetFrontCount(requested));
            }
        }

        private void AddFront_Click(object sender, RoutedEventArgs e) => MutateStructure(() => state.SetFrontCount(state.Structure.Count + 1));

        private void RemoveFront_Click(object sender, RoutedEventArgs e) => MutateStructure(() => state.SetFrontCount(Math.Max(1, state.Structure.Count - 1)));

        private void AddLevel_Click(object sender, RoutedEventArgs e) => MutateStructure(() => state.AdjustLevels(state.Structure.SelectedFrontIndex, 1));

        private void RemoveLevel_Click(object sender, RoutedEventArgs e) => MutateStructure(() => state.AdjustLevels(state.Structure.SelectedFrontIndex, -1));

        private void SelectedFront_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync || SelectedFrontBox.SelectedIndex < 0) return;
            SelectSingleCell(SelectedFrontBox.SelectedIndex, state.Structure.SelectedLevelIndex);
        }

        private void SelectedLevel_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync || SelectedLevelBox.SelectedIndex < 0) return;
            SelectSingleCell(state.Structure.SelectedFrontIndex, SelectedLevelBox.SelectedIndex);
        }

        /// <summary>Replace the selection with a single cell (the front/level combos), reload the panel, recompute.</summary>
        private void SelectSingleCell(int frontIndex, int levelIndex)
        {
            if (!AllFieldsValid(out var error)) { SetStatus(error, true); return; }
            CommitCurrentCell();
            state.ToggleCell(frontIndex, levelIndex, false);
            LoadSelectedFront();
            RefreshCellSelectionChecks();
            RefreshMatrixSelectionVisuals();
            RequestRecompute();
        }

        /// <summary>Commit the panel, run a structural mutation on the state, re-sync the matrix + panel, recompute once.
        /// Blocked while any numeric field is in error (a structural change must not consume invalid inputs).</summary>
        private void MutateStructure(Action mutate)
        {
            if (suppressSync) return;
            if (!AllFieldsValid(out var error)) { SetStatus(error, true); return; }
            using (session.Recompute.Defer())
            {
                CommitCurrentCell();
                mutate();
                suppressSync = true;
                try
                {
                    RefreshFrontSelector();
                    SyncTopeMatrix();
                    SyncCellSelectionMatrix();
                    RenderPushBackMatrix();
                    LoadSelectedFront();
                }
                finally
                {
                    suppressSync = false;
                }

                RequestRecompute();
            }
        }

        // ---- Cell / input handlers -------------------------------------------------------------------------------

        private void Name_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync) return;
            session.Identity.SetName(NameBox.Text?.Trim());
        }

        private void Input_Changed(object sender, RoutedEventArgs e) => RequestRecompute();

        private void Combo_Changed(object sender, SelectionChangedEventArgs e) => RequestRecompute();

        private void InOutBeam_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CellInOutBeamBox.SelectedValue is string id)
            {
                SetPeralteOptions(CellInOutPeralteBox, id, SelectedPeralte(CellInOutPeralteBox, DynamicRackDefaults.DefaultBeamDepth));
            }

            RequestRecompute();
        }

        private void IntermediateBeam_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CellIntermediateBeamBox.SelectedValue is string id)
            {
                SetPeralteOptions(CellIntermediatePeralteBox, id, SelectedPeralte(CellIntermediatePeralteBox, DynamicRackDefaults.DefaultIntermediateBeamDepth));
            }

            RequestRecompute();
        }

        // ---- Scope apply -----------------------------------------------------------------------------------------

        private void ApplyCell_Click(object sender, RoutedEventArgs e) => ApplyScope(DynamicRackCellScope.Cell);

        private void ApplySelected_Click(object sender, RoutedEventArgs e) => ApplyScope(DynamicRackCellScope.Selected);

        private void ApplyLevel_Click(object sender, RoutedEventArgs e) => ApplyScope(DynamicRackCellScope.Level);

        private void ApplyFront_Click(object sender, RoutedEventArgs e) => ApplyScope(DynamicRackCellScope.Front);

        private void ApplyAll_Click(object sender, RoutedEventArgs e) => ApplyScope(DynamicRackCellScope.All);

        private void ApplyScope(DynamicRackCellScope scope)
        {
            if (!AllFieldsValid(out var error)) { SetStatus(error, true); return; }
            using (session.Recompute.Defer())
            {
                state.ApplyScope(ReadCellValues(), scope);
                suppressSync = true;
                try
                {
                    SyncTopeMatrix();
                    SyncCellSelectionMatrix();
                    RenderPushBackMatrix();
                    LoadSelectedFront();
                }
                finally
                {
                    suppressSync = false;
                }

                RequestRecompute();
            }
        }

        // ---- Safety --------------------------------------------------------------------------------------------

        private void Safety_Click(object sender, RoutedEventArgs e)
        {
            // Push Back admits every applicable family EXCEPT entrance guides: the dialog is opened with includeGuia:false, so
            // GUIA is never offered; the selections are already low-end (the resolver normalized them on load). The assembler's
            // AuthorizedSafety filters again at build, so a GUIA can never reach the design/system/BOM/plan.
            var elements = SafetyElementsForDialog();
            var levels = state.Structure.Fronts.Select(front => Math.Max(1, front.LoadLevels)).ToList();
            if (levels.Count == 0) levels.Add(Math.Max(1, DynamicRackDefaults.DefaultLoadLevels));
            var postCount = Math.Max(2, state.Structure.Count + 1);

            var dialog = new SelectiveSafetyWindow(
                elements, safetySelections, postCount,
                levelsPerFrente: levels, fondoCount: 1, parrillaPlan: null, catalog: catalog, resolvedSystem: null,
                fallbackLevelsArePerPost: true,
                introduction: "Push Back admite botas, protectores laterales, desviadores y defensa de montacargas en el extremo bajo (entrada/salida). No usa guías.",
                includeDefensa: true, includeGuia: false, useDynamicSafetyDefaults: true)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                RequestRecompute();
            }
        }

        // ---- View selector -------------------------------------------------------------------------------------

        private void View_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync) return;
            LateralSectionBox.Visibility = ViewBox.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            LateralSectionLabel.Visibility = LateralSectionBox.Visibility;
            RenderPreview();
        }

        private void UpdateViewSelector()
        {
            // Populate from the cortes the assembler already computed; never re-invoke a builder to recompute geometry.
            var count = lastComputation?.LateralCortes?.Count ?? 0;
            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                var previous = LateralSectionBox.SelectedIndex;
                LateralSectionBox.ItemsSource = Enumerable.Range(1, Math.Max(1, count)).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();
                LateralSectionBox.SelectedIndex = count > 0 ? Math.Max(0, Math.Min(previous, count - 1)) : 0;
                LateralSectionBox.Visibility = ViewBox.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
                LateralSectionLabel.Visibility = LateralSectionBox.Visibility;
            }
            finally
            {
                suppressSync = wasSuppressed;
            }
        }

        /// <summary>The (view id, section) the view selector currently points at, normalized to the embed contract.</summary>
        private (string View, int Section) SelectedView()
        {
            switch (ViewBox.SelectedIndex)
            {
                case 1: return (RackEmbedDocument.ViewFrontal, (int)PushBackFrontalEnd.EntradaSalida);
                case 2: return (RackEmbedDocument.ViewFrontal, (int)PushBackFrontalEnd.Posterior);
                case 3: return (RackEmbedDocument.ViewPlanta, -1);
                default: return (RackEmbedDocument.ViewLateral, Math.Max(0, LateralSectionBox.SelectedIndex));
            }
        }

        // ---- Preview -------------------------------------------------------------------------------------------

        private void RenderPreview()
        {
            PreviewCanvas.Children.Clear();
            if (!hasValidModel || lastComputation == null)
            {
                PreviewSummary.Text = string.Empty;
                PreviewHint.Text = "Genera un sistema válido para ver la vista previa.";
                return;
            }

            var (view, section) = SelectedView();
            var plan = PlanFor(view, section);
            var pieces = plan == null ? 0 : plan.Headers.SelectMany(g => g.Instances).Count() + plan.LooseInstances.Count;
            PreviewSummary.Text = string.Format(CultureInfo.InvariantCulture, "{0} · {1} pieza(s)", ViewLabel(view, section), pieces);
            PreviewHint.Text = currentInputsAreValid
                ? "Vista previa técnica de la vista seleccionada."
                : "⚠ La vista previa corresponde al ÚLTIMO cálculo válido; corrige los campos marcados.";
            PreviewLegend.Text = ViewLabel(view, section)
                + "  ·  estructura verde · largueros azul · cama gris · topes rojo · seguridad ámbar";
            DrawModel(BuildPreviewModel(plan, view));
        }

        /// <summary>The semantic preview of <paramref name="plan"/>: interpreted primitives only — the plan is the
        /// geometry authority. The low lateral IN/OUT beam (no PERALTE of its own) falls back to the resolved system's
        /// beam depth, like the dynamic editor's preview.</summary>
        private PushBackPreviewModel BuildPreviewModel(DynamicSystemPlan plan, string view)
            => PushBackPreviewModel.Build(
                plan,
                catalog,
                view,
                lastComputation?.System?.Structure?.InOutBeamDepth > 0.0
                    ? lastComputation.System.Structure.InOutBeamDepth
                    : DynamicRackDefaults.DefaultBeamDepth);

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderPreview();

        /// <summary>Paint the semantic preview: real segments (position/orientation/length from the plan) per role,
        /// boxes for reference pallets, and small markers only for pieces whose drawable size the plan does not carry.
        /// Projection via the shared PreviewProjection; shapes via the shared PreviewCanvasPainter.</summary>
        private void DrawModel(PushBackPreviewModel model)
        {
            PreviewCanvas.Children.Clear();
            if (model == null || model.IsEmpty) return;

            var width = PreviewCanvas.ActualWidth;
            var height = PreviewCanvas.ActualHeight;
            if (width < 20.0 || height < 20.0) return;

            var projection = PreviewProjection.Fit(
                new Rect(model.MinX, model.MinY, Math.Max(1e-6, model.MaxX - model.MinX), Math.Max(1e-6, model.MaxY - model.MinY)),
                new Size(width, height),
                new Thickness(14.0));
            if (!projection.IsDrawable) return;

            var painter = new PreviewCanvasPainter(PreviewCanvas);
            var dash = new DoubleCollection { 4.0, 3.0 };
            foreach (var primitive in model.Primitives)
            {
                var brush = RoleBrush(primitive.Role);
                switch (primitive.Kind)
                {
                    case PushBackPreviewKind.Line:
                        painter.AddLine(
                            projection.Project(primitive.X1, primitive.Y1),
                            projection.Project(primitive.X2, primitive.Y2),
                            brush,
                            primitive.Thick ? 3.0 : 1.8);
                        break;

                    case PushBackPreviewKind.Box:
                    {
                        var topLeft = projection.Project(Math.Min(primitive.X1, primitive.X2), Math.Max(primitive.Y1, primitive.Y2));
                        painter.AddRectangle(
                            topLeft.X,
                            topLeft.Y,
                            Math.Abs(primitive.X2 - primitive.X1) * projection.Scale,
                            Math.Abs(primitive.Y2 - primitive.Y1) * projection.Scale,
                            brush,
                            1.2,
                            dash);
                        break;
                    }

                    default:
                    {
                        var at = projection.Project(primitive.X1, primitive.Y1);
                        painter.AddRectangle(at.X - 2.5, at.Y - 2.5, 5.0, 5.0, brush, 1.4, null, brush);
                        break;
                    }
                }
            }
        }

        private static Brush RoleBrush(HeaderBlockRole role)
        {
            switch (role)
            {
                case HeaderBlockRole.Beam: return PreviewPalette.Beam;                 // largueros (bajo, posterior, interm.)
                case HeaderBlockRole.Tope: return PreviewPalette.Warning;              // topes posteriores
                case HeaderBlockRole.Safety: return PreviewPalette.Accent;             // seguridad
                case HeaderBlockRole.Rail:
                case HeaderBlockRole.Roller:
                case HeaderBlockRole.Brake:
                case HeaderBlockRole.Stop: return PreviewPalette.Floor;                // cama / rieles
                case HeaderBlockRole.Pallet: return PreviewPalette.Guide;              // tarimas de referencia
                case HeaderBlockRole.Post:
                case HeaderBlockRole.BasePlate:
                case HeaderBlockRole.Horizontal:
                case HeaderBlockRole.Diagonal:
                case HeaderBlockRole.ClosingHorizontal:
                case HeaderBlockRole.Separator: return PreviewPalette.Structure;       // postes / cabeceras
                default: return PreviewPalette.Muted;
            }
        }

        private DynamicSystemPlan PlanFor(string view, int section)
        {
            if (lastComputation == null) return null;
            if (string.Equals(view, RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase)) return lastComputation.PlantaPlan;
            if (string.Equals(view, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase))
            {
                return section == (int)PushBackFrontalEnd.Posterior ? lastComputation.FrontalPosterior : lastComputation.FrontalEntradaSalida;
            }

            // Lateral: the SELECTED corte's plan (the assembler already computed every corte), not the full lateral.
            var cortes = lastComputation.LateralCortes;
            if (cortes != null && cortes.Count > 0)
            {
                return cortes[Math.Max(0, Math.Min(section, cortes.Count - 1))].Plan;
            }

            return lastComputation.LateralPlan;
        }

        private static string ViewLabel(string view, int section)
        {
            if (string.Equals(view, RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase)) return "Planta";
            if (string.Equals(view, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase))
            {
                return section == (int)PushBackFrontalEnd.Posterior ? "Frontal posterior" : "Frontal entrada/salida";
            }

            return "Lateral (corte " + (section + 1).ToString(CultureInfo.InvariantCulture) + ")";
        }

        // ---- Insert / Update -----------------------------------------------------------------------------------

        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var (view, section) = SelectedView();
            RequestDraw(view, section, updateOnly: false);
        }

        // PB-VAL-01: the four linked views are FIRST-CLASS buttons in the action bar (the dynamic editor offers the same
        // flow), not one hidden combo. Each selects its view — so the preview follows along and the embed View/Section
        // contract stays the single source — and then inserts through the very same path as "Insertar vista actual".
        private void InsertLateral_Click(object sender, RoutedEventArgs e) => InsertViewAt(0);

        private void InsertFrontalEntrada_Click(object sender, RoutedEventArgs e) => InsertViewAt(1);

        private void InsertFrontalPosterior_Click(object sender, RoutedEventArgs e) => InsertViewAt(2);

        private void InsertPlanta_Click(object sender, RoutedEventArgs e) => InsertViewAt(3);

        private void InsertViewAt(int viewIndex)
        {
            if (ViewBox.SelectedIndex != viewIndex)
            {
                ViewBox.SelectedIndex = viewIndex;   // View_Changed re-renders the preview and shows/hides the corte box
            }

            var (view, section) = SelectedView();
            RequestDraw(view, section, updateOnly: false);
        }

        /// <summary>REAL restore (round-3 review): reload the state and EVERY control from the last VALID design —
        /// not merely a recompute of the current, possibly edited, controls. Identity, source project and edit mode
        /// stay untouched; nothing is drawn.</summary>
        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (lastComputation?.Design == null)
            {
                SetStatus("Todavía no hay un sistema válido que restaurar.", true);
                return;
            }

            var inputs = state.LoadFromDesign(lastComputation.Design, assembler.Resolver);
            LoadFromModel(inputs, NameBox.Text);
            SetStatus("Valores restaurados al último sistema válido.", false);
        }

        private void Update_Click(object sender, RoutedEventArgs e) => RequestDraw(null, -1, updateOnly: true);

        private void RequestDraw(string view, int section, bool updateOnly)
        {
            if (!canInsertInAutoCad)
            {
                SetStatus("El dibujo en AutoCAD solo está disponible cuando la ventana se abre desde AutoCAD.", true);
                return;
            }

            if (updateOnly && !isEditingExisting)
            {
                SetStatus("Solo un sistema abierto con RACKEDITAR puede actualizarse en sitio.", true);
                return;
            }

            RequestRecompute(); // synchronous validate + build
            if (!currentInputsAreValid || session.System == null)
            {
                SetStatus("Corrige los datos: no se puede insertar un modelo inválido.", true);
                return; // never fall back to the previous valid model
            }

            session.Identity.SetName(NameBox.Text?.Trim());
            session.SetModel(lastComputation.Design, lastComputation.System);
            if (updateOnly)
            {
                session.RequestUpdate(ctx => new PushBackInsertionRequest(
                    lastComputation.System, lastComputation.Design, ctx.Id, ctx.Name, ctx.View, ctx.Section, sourceProject));
            }
            else
            {
                session.RequestInsert(view, section, ctx => new PushBackInsertionRequest(
                    lastComputation.System, lastComputation.Design, ctx.Id, ctx.Name, ctx.View, ctx.Section, sourceProject));
            }

            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        // ---- BOM + library ---------------------------------------------------------------------------------------

        private void Bom_Click(object sender, RoutedEventArgs e)
        {
            RequestRecompute();
            if (!currentInputsAreValid || lastComputation?.Bom == null)
            {
                SetStatus("Corrige los datos: no se puede mostrar el BOM de un modelo inválido.", true);
                return;
            }

            new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();
        }

        private void SaveLibrary_Click(object sender, RoutedEventArgs e)
        {
            RequestRecompute();
            if (!currentInputsAreValid || lastComputation?.Design == null)
            {
                SetStatus("Corrige los datos: no se puede guardar un modelo inválido.", true);
                return;
            }

            var path = UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema");
            if (path == null) return;

            try
            {
                session.Identity.EnsureId();
                session.Identity.SetName(NameBox.Text?.Trim());
                // Save ONLY the active Push Back payload; WithSourceMetadataFrom preserves the opened project's unknown JSON
                // fields + non-downgraded schema version (I-11). Saving never flags an insert.
                var project = RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject);
                new RackProjectStore().Save(project, path);
                SetStatus("Sistema guardado: " + System.IO.Path.GetFileName(path), false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo guardar: " + ex.Message, true);
            }
        }

        // ---- Small helpers -------------------------------------------------------------------------------------

        private void UpdateButtons()
        {
            InsertButton.IsEnabled = canInsertInAutoCad && currentInputsAreValid;
            UpdateButton.IsEnabled = canInsertInAutoCad && currentInputsAreValid && isEditingExisting;
            BomButton.IsEnabled = currentInputsAreValid;
            SaveLibraryButton.IsEnabled = currentInputsAreValid;
            if (!canInsertInAutoCad)
            {
                InsertButton.ToolTip = "Disponible solo cuando la ventana se abre desde AutoCAD.";
                UpdateButton.ToolTip = InsertButton.ToolTip;
            }
            else
            {
                InsertButton.ToolTip = currentInputsAreValid ? "Inserta la vista seleccionada enlazada al sistema." : "Corrige los campos numéricos marcados.";
                UpdateButton.ToolTip = isEditingExisting
                    ? "Redibuja en sitio todas las vistas del sistema."
                    : "Disponible solo para un sistema abierto con RACKEDITAR.";
            }
        }

        private void UpdateGuid() => GuidText.Text = session.Identity.HasId ? session.Identity.Id : "(se asigna al insertar)";

        private void SetStatus(string message, bool isError) => UiSupport.SetStatus(StatusText, message, isError);

        private IReadOnlyList<BeamProfileCatalogEntry> InOutBeamOptions() => DynamicRackLevelGeometry.CompatibleInOutBeams(catalog);

        private IReadOnlyList<BeamProfileCatalogEntry> IntermediateBeamOptions() => DynamicRackLevelGeometry.CompatibleIntermediateBeams(catalog);

        private void SetPeralteOptions(ComboBox combo, string beamId, double selected)
        {
            var fallback = string.Equals(beamId, DynamicRackDefaults.InOutBeamCatalogId, StringComparison.OrdinalIgnoreCase)
                ? DynamicRackDefaults.DefaultBeamDepth
                : DynamicRackDefaults.DefaultIntermediateBeamDepth;
            var allowed = DynamicRackLevelGeometry.AllowedPeraltes(catalog, beamId);
            var options = allowed.Count > 0 ? allowed : new[] { fallback };
            combo.ItemsSource = options;
            combo.SelectedItem = options.FirstOrDefault(value => Math.Abs(value - selected) < 1e-6);
            if (combo.SelectedIndex < 0) combo.SelectedIndex = 0;
        }

        private static double SelectedPeralte(ComboBox combo, double fallback) => combo?.SelectedItem is double value ? value : fallback;

        private static double Val(NumericField field, double fallback) => field.Value ?? fallback;

        private static int IntVal(NumericField field, int fallback) => field.Value.HasValue ? (int)Math.Round(field.Value.Value) : fallback;
    }
}
