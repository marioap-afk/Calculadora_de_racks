using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private bool hasValidModel;
        private RackProject sourceProject;
        private PushBackEditorComputation lastComputation;
        private SelectionMatrixModel topeModel;
        private List<int> topeShape = new List<int>();

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
        internal PushBackEditorComputation LastComputation => lastComputation;
        internal bool HasValidModel => hasValidModel;
        internal IReadOnlyList<SelectiveSafetySelection> SafetySelections => safetySelections;

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
                FrontBox.Text = Num(pallet.Front);
                DepthBox.Text = Num(pallet.Depth);
                PalletHeightBox.Text = Num(pallet.Height);
                WeightBox.Text = Num(pallet.Weight);
                WeightUnitBox.SelectedItem = string.IsNullOrWhiteSpace(pallet.WeightUnit) ? "kg" : pallet.WeightUnit;
                PalletsDeepBox.Text = Math.Max(2, inputs.PalletsDeep).ToString(CultureInfo.InvariantCulture);
                ToleranceBox.Text = Num(inputs.PalletTolerance > 0.0 ? inputs.PalletTolerance : DynamicRackDefaults.DefaultPalletTolerance);
                PostBox.SelectedId = string.IsNullOrWhiteSpace(inputs.PostCatalogId) ? catalog?.Defaults?.Post : inputs.PostCatalogId;
                PostPeralteBox.Text = Num(inputs.PostPeralte);
                BeamDepthBox.Text = Num(inputs.BeamDepth > 0.0 ? inputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth);
                SaqueBox.Text = Num(state.RearTopeSaque);

                var options = inputs.Annotations ?? new DynamicAnnotationOptions();
                NumberFrontsCheck.IsChecked = options.NumberFronts;
                NumberLevelsCheck.IsChecked = options.NumberLevels;
                DrawRackNameCheck.IsChecked = options.DrawRackName;
                AnnotationScaleBox.Text = Num(options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0);
                DimensionsBox.SelectedIndex = Math.Min((int)DimensionDetail.Detailed, Math.Max(0, (int)options.Dimensions));

                safetySelections.Clear();
                foreach (var safety in inputs.SafetySelections ?? Enumerable.Empty<SelectiveSafetySelection>())
                {
                    if (safety != null) safetySelections.Add(safety.DeepCopy());
                }

                RefreshFrontSelector();
                SyncTopeMatrix();
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
                FrontCountBox.Text = state.Structure.Count.ToString(CultureInfo.InvariantCulture);
                if (SelectedFrontBox.Items.Count == state.Structure.Count) SelectedFrontBox.SelectedIndex = frontIndex;
                RefreshLevelSelector();
                if (SelectedLevelBox.Items.Count == Math.Max(1, front.LoadLevels)) SelectedLevelBox.SelectedIndex = levelIndex;

                PositionsBox.Text = front.PalletCount.ToString(CultureInfo.InvariantCulture);
                LevelsBox.Text = front.LoadLevels.ToString(CultureInfo.InvariantCulture);
                FondosBox.Text = front.PalletsDeep.ToString(CultureInfo.InvariantCulture);
                DepthStartBox.Text = front.DepthStartPosition.ToString(CultureInfo.InvariantCulture);
                FirstLevelHeightBox.Text = Num(front.FirstLevelHeight);

                CellPalletFrontBox.Text = Num(cell.PalletFront);
                CellPalletHeightBox.Text = Num(cell.PalletHeight);
                CellPalletWeightBox.Text = Num(cell.PalletWeight);
                CellClearBox.Text = Num(cell.ClearHeight);
                CellInOutBeamBox.SelectedValue = cell.InOutBeamCatalogId;
                SetPeralteOptions(CellInOutPeralteBox, cell.InOutBeamCatalogId, cell.InOutBeamDepth);
                CellBeamLengthOverrideBox.Text = cell.BeamLengthOverride.HasValue ? Num(cell.BeamLengthOverride.Value) : string.Empty;
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

            CommitCurrentCell();
            state.RearTopeSaque = ReadDouble(SaqueBox, state.RearTopeSaque);

            var computation = assembler.Build(state, ReadInputs());
            if (computation.IsValid)
            {
                session.SetModel(computation.Design, computation.System);
                assembler.AcceptComputation(state, computation); // advance the opaque baseline (never mutated by the window)
                lastComputation = computation;
                hasValidModel = true;
                SyncTopeMatrixIfShapeChanged();
                UpdateViewSelector();
                RenderPreview();
                SetStatus("Vista recalculada.", false);
            }
            else
            {
                SetStatus("No se pudo generar el sistema: " + computation.Error, true); // keep the last valid model
            }

            GuidText.Text = session.Identity.HasId ? session.Identity.Id : "(se asigna al insertar)";
            UpdateButtons();
        }

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
                    PalletCount = Math.Max(1, ReadInt(PositionsBox, front.PalletCount)),
                    LoadLevels = Math.Max(1, ReadInt(LevelsBox, front.LoadLevels)),
                    PalletsDeep = Math.Max(2, ReadInt(FondosBox, front.PalletsDeep)),
                    DepthStartPosition = Math.Max(1, ReadInt(DepthStartBox, front.DepthStartPosition)),
                    FirstLevelHeight = ReadDouble(FirstLevelHeightBox, front.FirstLevelHeight),
                    PalletFront = ReadDouble(CellPalletFrontBox, cell.PalletFront),
                    PalletHeight = ReadDouble(CellPalletHeightBox, cell.PalletHeight),
                    PalletWeight = ReadDouble(CellPalletWeightBox, cell.PalletWeight),
                    ClearHeight = ReadDouble(CellClearBox, cell.ClearHeight),
                    InOutBeamCatalogId = CellInOutBeamBox.SelectedValue as string ?? cell.InOutBeamCatalogId,
                    InOutBeamDepth = SelectedPeralte(CellInOutPeralteBox, cell.InOutBeamDepth),
                    BeamLengthOverride = ReadOptionalDouble(CellBeamLengthOverrideBox),
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
                    ReadDouble(FrontBox, 42.0), ReadDouble(DepthBox, 48.0), ReadDouble(PalletHeightBox, 60.0),
                    ReadDouble(WeightBox, 1000.0), WeightUnitBox.SelectedItem as string ?? "kg"),
                PalletsDeep = Math.Max(2, ReadInt(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep)),
                PostCatalogId = PostBox.SelectedId,
                PostPeralte = ReadDouble(PostPeralteBox, 0.0),
                PalletTolerance = ReadDouble(ToleranceBox, DynamicRackDefaults.DefaultPalletTolerance),
                BeamDepth = ReadDouble(BeamDepthBox, DynamicRackDefaults.DefaultBeamDepth),
                Annotations = new DynamicAnnotationOptions
                {
                    NumberFronts = NumberFrontsCheck.IsChecked == true,
                    NumberLevels = NumberLevelsCheck.IsChecked == true,
                    DrawRackName = DrawRackNameCheck.IsChecked == true,
                    AnnotationScale = ReadDouble(AnnotationScaleBox, 1.0),
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

            RequestRecompute();
        }

        // ---- Structure handlers ----------------------------------------------------------------------------------

        private void FrontCount_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync) return;
            var requested = ReadInt(FrontCountBox, state.Structure.Count);
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
            CommitCurrentCell();
            state.ToggleCell(SelectedFrontBox.SelectedIndex, state.Structure.SelectedLevelIndex, false);
            LoadSelectedFront();
            RequestRecompute();
        }

        private void SelectedLevel_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync || SelectedLevelBox.SelectedIndex < 0) return;
            CommitCurrentCell();
            state.ToggleCell(state.Structure.SelectedFrontIndex, SelectedLevelBox.SelectedIndex, false);
            LoadSelectedFront();
            RequestRecompute();
        }

        /// <summary>Commit the panel, run a structural mutation on the state, re-sync the matrix + panel, recompute once.</summary>
        private void MutateStructure(Action mutate)
        {
            if (suppressSync) return;
            using (session.Recompute.Defer())
            {
                CommitCurrentCell();
                mutate();
                suppressSync = true;
                try
                {
                    RefreshFrontSelector();
                    SyncTopeMatrix();
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
            using (session.Recompute.Defer())
            {
                state.ApplyScope(ReadCellValues(), scope);
                suppressSync = true;
                try
                {
                    SyncTopeMatrix();
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
            var elements = (catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>())
                .Where(element => element != null
                    && !SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.GuiaType))
                .ToList();
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
            var count = lastComputation?.System != null
                ? new PushBackSystemLateralBuilder().Cortes(lastComputation.System, catalog).Count
                : 0;
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

        // ---- Preview (a stable, view-scoped textual representation; the canvas is enriched in a later step) -------

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
            PreviewHint.Text = "Vista previa esquemática de la vista seleccionada.";
            DrawPlan(plan);
        }

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderPreview();

        /// <summary>A stable, generic schematic of a resolved plan: one marker per instance at its scaled insertion point,
        /// colour-coded by role. It renders the geometry Application already produced — no recomputation, no re-projection.</summary>
        private void DrawPlan(DynamicSystemPlan plan)
        {
            PreviewCanvas.Children.Clear();
            if (plan == null) return;
            var instances = plan.Headers.SelectMany(group => group.Instances).Concat(plan.LooseInstances).ToList();
            if (instances.Count == 0) return;

            var width = PreviewCanvas.ActualWidth;
            var height = PreviewCanvas.ActualHeight;
            if (width < 20.0 || height < 20.0) return;

            var minX = instances.Min(i => i.Insertion.X);
            var maxX = instances.Max(i => i.Insertion.X);
            var minY = instances.Min(i => i.Insertion.Y);
            var maxY = instances.Max(i => i.Insertion.Y);
            const double margin = 12.0;
            var scale = Math.Min((width - 2 * margin) / Math.Max(1e-6, maxX - minX), (height - 2 * margin) / Math.Max(1e-6, maxY - minY));
            if (double.IsInfinity(scale) || double.IsNaN(scale) || scale <= 0.0) scale = 1.0;

            foreach (var instance in instances)
            {
                var px = margin + (instance.Insertion.X - minX) * scale;
                var py = height - margin - (instance.Insertion.Y - minY) * scale; // flip Y (world up -> screen down)
                var marker = new Rectangle { Width = 6.0, Height = 6.0, Fill = RoleBrush(instance.Role) };
                Canvas.SetLeft(marker, px - 3.0);
                Canvas.SetTop(marker, py - 3.0);
                PreviewCanvas.Children.Add(marker);
            }
        }

        private static Brush RoleBrush(HeaderBlockRole role)
        {
            switch (role)
            {
                case HeaderBlockRole.Beam: return Brushes.SteelBlue;
                case HeaderBlockRole.Tope: return Brushes.OrangeRed;
                case HeaderBlockRole.Safety: return Brushes.Goldenrod;
                case HeaderBlockRole.Rail:
                case HeaderBlockRole.Roller:
                case HeaderBlockRole.Stop: return Brushes.MediumSeaGreen;
                default: return Brushes.SlateGray;
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

            RequestRecompute();
            if (!hasValidModel || session.System == null)
            {
                SetStatus("Genera un sistema válido antes de insertar en AutoCAD.", true);
                return;
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
            if (!hasValidModel || lastComputation?.Bom == null)
            {
                SetStatus("Genera un sistema válido antes de ver el BOM.", true);
                return;
            }

            new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();
        }

        private void SaveLibrary_Click(object sender, RoutedEventArgs e)
        {
            RequestRecompute();
            if (!hasValidModel || lastComputation?.Design == null)
            {
                SetStatus("Genera un sistema válido antes de guardar.", true);
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
            InsertButton.IsEnabled = canInsertInAutoCad && hasValidModel;
            UpdateButton.IsEnabled = canInsertInAutoCad && hasValidModel && isEditingExisting;
            if (!canInsertInAutoCad)
            {
                InsertButton.ToolTip = "Disponible solo cuando la ventana se abre desde AutoCAD.";
                UpdateButton.ToolTip = InsertButton.ToolTip;
            }
            else
            {
                InsertButton.ToolTip = "Inserta la vista seleccionada enlazada al sistema.";
                UpdateButton.ToolTip = isEditingExisting
                    ? "Redibuja en sitio todas las vistas del sistema."
                    : "Disponible solo para un sistema abierto con RACKEDITAR.";
            }
        }

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

        private static string Num(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

        private static double ReadDouble(TextBox box, double fallback) => UiSupport.TryNum(box?.Text, out var value) ? value : fallback;

        private static int ReadInt(TextBox box, int fallback) => UiSupport.TryInt(box?.Text, out var value) ? value : fallback;

        private static double? ReadOptionalDouble(TextBox box)
            => UiSupport.TryOptionalNum(box?.Text, out var value) ? value : null;
    }
}
