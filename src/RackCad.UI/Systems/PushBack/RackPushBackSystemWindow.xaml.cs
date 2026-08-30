using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Controls;
using RackCad.UI.Editor;
using RackCad.UI.Preview;
using RackCad.UI.RackFrames;
using RackCad.UI.Shell;
using RackCad.UI.Systems.Dynamic;

namespace RackCad.UI.Systems.PushBack
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

        /// <summary>
        /// I-42 — el estado COMPUESTO. Contiene los dos lados; el de la izquierda (A) es exactamente el mismo
        /// <see cref="PushBackEditorState"/> que esta ventana conducia antes de la iniciativa. Toda la ventana lee
        /// <see cref="state"/>, que es el lado ACTIVO, asi que el selector de lado hace que la matriz, la celda
        /// seleccionada y los cinco alcances trabajen sobre el lado que el usuario esta mirando, sin duplicar ni un
        /// control.
        /// </summary>
        private readonly PushBackCompositeEditorState composite =
            new PushBackCompositeEditorState(new PushBackEditorState(), new PushBackEditorState());

        private readonly PushBackCompositeEditorAssembler compositeAssembler;
        private readonly PushBackEditorDesignAssembler assembler;

        /// <summary>El estado del lado ACTIVO. Un rack de un solo sentido responde siempre el lado A, el de siempre.</summary>
        private PushBackEditorState state => composite.Active;
        private readonly RackEditorSession<PushBackDesign, PushBackSystem> session;
        private readonly List<SelectiveSafetySelection> safetySelections = new List<SelectiveSafetySelection>();
        private readonly bool canInsertInAutoCad;

        private bool isEditingExisting;
        private bool suppressSync;
        private bool hasValidModel;          // ever produced a valid model (a preview reference exists)
        private bool currentInputsAreValid;  // the CURRENT controls recomputed to a valid model
        private RackProject sourceProject;
        private PushBackEditorComputation lastComputation; // the LAST VALID computation (only replaced on a valid build)

        /// <summary>
        /// PB-013 (I-32): the RACK-WIDE pallet. Only its Fondo and Unidad are edited in this window (the two boxes that
        /// stay enabled); Frente, Alto and Peso belong to the cell, so they are kept exactly as the design was loaded
        /// with and the general panel only MIRRORS the selected cell. Keeping them here — instead of re-reading the
        /// mirrored boxes — is what guarantees that selecting or editing a cell never rewrites the rack-wide pallet, and
        /// therefore never changes the drawing through a panel the Owner reported as inert.
        /// </summary>
        private PalletSpecification generalPallet = PushBackEditorInputs.NewDesign().Pallet;

        /// <summary>Carrier of the four advanced RACK-WIDE scopes between recomputes (I-35, Owner round 2). It is the
        /// SAME transport type the assembler consumes, so there is no parallel field and no second authority: only
        /// its four advanced properties are used, and ReadInputs copies them across verbatim.</summary>
        private readonly PushBackEditorInputs advanced = PushBackEditorInputs.NewDesign();

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
            compositeAssembler = new PushBackCompositeEditorAssembler(catalog);

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
            InitializeCompositeSection();

            LoadNew();
        }

        // ---- Test seams (internal) --------------------------------------------------------------------------------

        internal RackEditorSession<PushBackDesign, PushBackSystem> Session => session;
        internal PushBackEditorState State => state;
        internal PushBackEditorDesignAssembler Assembler => assembler;

        /// <summary>The plan currently drawn in the preview (the selected view/corte). For the lateral view this is the
        /// SELECTED corte's plan, so changing "Corte 1"→"Corte 2" changes what the preview shows.</summary>
        internal HeaderRunPlan CurrentPreviewPlan
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

        /// <summary>The safety families offered by the dialog: every applicable family EXCEPT entrance guides (GUIA) and walk
        /// grids (PARRILLA), which Push Back never admits (PB-VAL-06) — so neither is even a visible option. The exclusion is
        /// authoritative in <see cref="PushBackSafetyAuthority"/>; hiding them here keeps the UI from offering what the build
        /// would strip anyway.</summary>
        internal IReadOnlyList<SafetyElementCatalogEntry> SafetyElementsForDialog()
            => (catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>())
                .Where(element => element != null
                    && !SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.GuiaType)
                    && !SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.ParrillaType)
                    // Owner decision (2026-07-24): the TOPE belongs to the HIGH end and is owned by the rear-tope
                    // config, so it is never offered as ordinary low-end safety (PushBackSafetyAuthority refuses it too).
                    && !SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.TopeType)
                    // I-42 (ronda 7E, decision del dueño): la DEFENSA se elige en su seccion, con su «Ninguno» y
                    // por lado. Dejarla tambien en la lista general seria dos sitios para la misma decision, que es
                    // exactamente lo que la ronda 7B corrigio para los topes.
                    && !SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType))
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
            // I-42: un rack nuevo nace de UN solo sentido, como siempre. El lado B se declara despues, si se quiere.
            composite.SetActiveSide(RackCad.Domain.Systems.PushBack.PushBackSide.A);
            composite.SetSideBPresent(false);
            composite.LoadComposite(null);
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
            // I-42: primero el lado A, por el camino de siempre, y luego la parte compuesta. Un documento
            // anterior a I-42 no trae ninguna, asi que el rack se abre como el de un solo sentido que es.
            composite.SetActiveSide(RackCad.Domain.Systems.PushBack.PushBackSide.A);
            composite.SetSideBPresent(false);
            var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);
            LoadCompositeFromDesign(design);
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
            // I-42: primero el lado A, por el camino de siempre, y luego la parte compuesta. Un documento
            // anterior a I-42 no trae ninguna, asi que el rack se abre como el de un solo sentido que es.
            composite.SetActiveSide(RackCad.Domain.Systems.PushBack.PushBackSide.A);
            composite.SetSideBPresent(false);
            var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);
            LoadCompositeFromDesign(design);
            this.sourceProject = sourceProject;
            isEditingExisting = true;
            session.Identity.Adopt(rackId, rackName);
            LoadFromModel(inputs, rackName);
        }

        /// <summary>
        /// El diseño del lado A SOLO, sin la parte compuesta.
        ///
        /// <para>
        /// El estado del editor se reconstruye desde el sistema RESUELTO, y el de un rack compuesto es la estructura
        /// COMPARTIDA —A + hueco + B—: cargar el lado A contra ella le metia en su matriz los rangos del rack entero,
        /// que no estan anidados, y el siguiente recalculo se caia con «los frentes con el menor numero de fondos
        /// deben compartir la misma posicion inicial». El lado A se carga contra SU propio diseño, exactamente como
        /// ya se hacia con el lado B; la parte compuesta se recupera despues, en su propio estado.
        /// </para>
        /// <para>
        /// Un documento anterior a I-42 no trae parte compuesta, asi que devuelve el MISMO objeto y su carga es
        /// literalmente la de siempre. La copia comparte las referencias del diseño: solo se retiran SideB y
        /// Composite, y nada de lo que se comparte se muta en este camino.
        /// </para>
        /// </summary>
        private static PushBackDesign SideAOnly(PushBackDesign design)
        {
            if (design == null || !design.IsComposite)
            {
                return design;
            }

            var copy = new PushBackDesign
            {
                Structure = design.Structure,
                LegacyHighEndBeamPeralte = design.LegacyHighEndBeamPeralte,
                RearTope = design.RearTope
            };

            foreach (var front in design.Fronts)
            {
                copy.Fronts.Add(front);
            }

            return copy;
        }

        /// <summary>
        /// I-42 — desde donde se mide «Alto 1er nivel» en el rack ABIERTO. Un rack nuevo nace con el datum del
        /// producto; uno cargado conserva el que la carga recupero —y, si venia sin marcador, el que esa unica
        /// frontera le asigno al re-expresarlo sin moverlo. La ventana solo lo guarda y lo devuelve.
        /// </summary>
        private int? firstLevelDatum = (int)RackCad.Application.Systems.Shared.RackFirstLevelDatumMode.LowestUsablePunch;

        /// <summary>El datum vigente (seam de prueba).</summary>
        internal int? FirstLevelDatumForTest => firstLevelDatum;

        private void LoadFromModel(PushBackEditorInputs inputs, string rackName)
        {
            suppressSync = true;
            try
            {
                NameBox.Text = rackName ?? string.Empty;
                firstLevelDatum = inputs.FirstLevelDatum;
                var pallet = inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
                generalPallet = pallet;
                DepthBox.SetNumber(pallet.Depth);
                WeightUnitBox.SelectedItem = string.IsNullOrWhiteSpace(pallet.WeightUnit) ? "kg" : pallet.WeightUnit;
                PalletsDeepBox.SetNumber(Math.Max(2, inputs.PalletsDeep));
                ToleranceBox.SetNumber(inputs.PalletTolerance > 0.0 ? inputs.PalletTolerance : DynamicRackDefaults.DefaultPalletTolerance);
                PostBox.SelectedId = string.IsNullOrWhiteSpace(inputs.PostCatalogId) ? catalog?.Defaults?.Post : inputs.PostCatalogId;
                PostPeralteBox.SetNumber(inputs.PostPeralte);
                BeamDepthBox.SetNumber(inputs.BeamDepth > 0.0 ? inputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth);

                // I-35 (Owner round 2): adopt the four advanced scopes the load recovered, then paint them.
                advanced.ManualHeaderHeightOverride = inputs.ManualHeaderHeightOverride;
                advanced.DerivedPostReinforced = inputs.DerivedPostReinforced;
                advanced.DerivedPostReinforcementHeight = inputs.DerivedPostReinforcementHeight;
                advanced.DerivedPostHeight = inputs.DerivedPostHeight;
                advanced.SeparatorCountOverride = inputs.SeparatorCountOverride;
                advanced.SeparatorSpacingOverride = inputs.SeparatorSpacingOverride;

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
                RenderPushBackMatrix();
                LoadSelectedFront();
                LoadAdvancedRackParameters();
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
                MirrorSelectedCellPallet(cell);
                CellClearBox.SetNumber(cell.ClearHeight);
                CellInOutBeamBox.SelectedValue = cell.InOutBeamCatalogId;
                SetPeralteOptions(CellInOutPeralteBox, cell.InOutBeamCatalogId, cell.InOutBeamDepth);
                CellBeamLengthOverrideBox.SetNumber(cell.BeamLengthOverride); // null -> blank (optional field)
                CellIntermediateBeamBox.SelectedValue = cell.IntermediateBeamCatalogId;
                SetPeralteOptions(CellIntermediatePeralteBox, cell.IntermediateBeamCatalogId, cell.IntermediateBeamDepth);

                RearPeralteBox.SelectedItem = RearPeraltes.FirstOrDefault(p => Math.Abs(p - push.HighEndBeamPeralte) < 1e-6);
                if (RearPeralteBox.SelectedItem == null) RearPeralteBox.SelectedItem = PushBackDefaults.HighEndBeamDefaultPeralte;

                // I-41: el fondo propio de la celda (vacio = hereda «Fondos frente») y su tarima.
                // I-42: si la celda es una cama CORRIDA, el campo edita el fondo propio de esa cama.
                LoadCellFondoField(push);
                CellDrawPalletCheck.IsChecked = push.DrawPallet;

                // I-42: presencia de la ranura y los topes de los DOS lados, para la MISMA celda seleccionada.
                LoadCompositeCellPanel();

                ApplyBlankFrontEditability(front.IsActive);
            }
            finally
            {
                suppressSync = wasSuppressed;
            }
        }

        /// <summary>
        /// I-33: un frente EN BLANCO conserva una seleccion valida, pero sus niveles y celdas no existen, asi que todo
        /// control que los edite —incluidos los alcances ligados a celda— se deshabilita mientras dure ese estado. Los
        /// controles ESTRUCTURALES del frente (posiciones, fondos e inicio en fondo) siguen siendo validos y quedan
        /// disponibles. Reactivar el frente vuelve a llamar aqui y restaura la edicion de inmediato.
        /// </summary>
        private void ApplyBlankFrontEditability(bool isActive)
        {
            const string reason = "El frente esta en blanco: no tiene niveles ni celdas que editar. "
                                  + "Desmarca «En blanco» para volver a editarlo.";

            // Nivel y elevacion del primer larguero: editan NIVELES.
            foreach (var control in new Control[] { LevelsBox, FirstLevelHeightBox, SelectedLevelBox })
            {
                SetBlankSensitive(control, isActive, reason);
            }

            // Celda seleccionada y su peralte posterior: editan CELDAS inexistentes.
            foreach (var control in new Control[]
                     {
                         CellPalletFrontBox, CellPalletHeightBox, CellPalletWeightBox, CellClearBox,
                         CellBeamLengthOverrideBox, CellInOutBeamBox, CellInOutPeralteBox,
                         CellIntermediateBeamBox, CellIntermediatePeralteBox, RearPeralteBox,
                         // I-41: el fondo propio y la tarima son propiedades de una CELDA, asi que siguen la misma
                         // regla que las demas. La configuracion queda DORMIDA, no borrada: reactivar el frente la
                         // devuelve intacta.
                         CellFondoOverrideBox, CellDrawPalletCheck
                     })
            {
                SetBlankSensitive(control, isActive, reason);
            }

            // Alcances/aplicaciones ligados a CELDA. Los tres botones de datos del FRENTE quedan disponibles: copian
            // valores estructurales, que siguen siendo validos en un frente en blanco.
            foreach (var control in new Control[]
                     {
                         ApplyCellButton, ApplySelectedButton, ApplyLevelButton, ApplyFrontButton, ApplyAllButton,
                         CellPropertyScopeBox, ApplyCellFondoButton, RestoreCellFondoButton,
                         ApplyCellPalletButton, RestoreCellPalletButton
                     })
            {
                SetBlankSensitive(control, isActive, reason);
            }
        }

        /// <summary>Original tooltips, so explaining WHY a control is disabled never destroys the tooltip the control
        /// already had (several carry real usage notes).</summary>
        private readonly Dictionary<Control, object> blankToolTips = new Dictionary<Control, object>();

        private void SetBlankSensitive(Control control, bool isActive, string reason)
        {
            if (control == null)
            {
                return;
            }

            if (!blankToolTips.ContainsKey(control))
            {
                blankToolTips[control] = control.ToolTip;
            }

            control.IsEnabled = isActive;
            ToolTipService.SetShowOnDisabled(control, true);
            control.ToolTip = isActive ? blankToolTips[control] : reason;
        }

        /// <summary>
        /// PB-013 (I-32): show the SELECTED CELL's pallet in the general panel's frozen Frente/Alto/Peso. Called both
        /// when the selection changes and after a valid recompute, because the ordinary edit path (type + leave the
        /// control) commits the cell and recomputes WITHOUT reloading the front panel — which is what used to leave
        /// these three showing a stale number.
        /// </summary>
        private void MirrorSelectedCellPallet(DynamicEditorCell cell)
        {
            if (cell == null) return;
            FrontBox.SetNumber(cell.PalletFront);
            PalletHeightBox.SetNumber(cell.PalletHeight);
            WeightBox.SetNumber(cell.PalletWeight);
        }

        /// <summary>The primary selected cell, or null when the matrix has no front/cell yet.</summary>
        private DynamicEditorCell SelectedCell()
        {
            var frontIndex = state.Structure.SelectedFrontIndex;
            if (frontIndex < 0 || frontIndex >= state.Structure.Count) return null;
            var front = state.Structure.Fronts[frontIndex];
            if (front.Cells.Count == 0) return null;
            var levelIndex = Math.Max(0, Math.Min(state.Structure.SelectedLevelIndex, front.Cells.Count - 1));
            return front.Cells[levelIndex];
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

        /// <summary>
        /// Cuantas veces ha corrido efectivamente el recalculo. Es el unico modo observable de comprobar que una
        /// operacion masiva de I-41 produce UNA sola pasada y no una por celda; la puerta de coalescencia
        /// (<see cref="RecomputeGate"/>) no publica ningun evento. Solo lectura, y no participa de ninguna regla.
        /// </summary>
        internal int RecomputePassesForTest { get; private set; }

        private void Recompute()
        {
            if (suppressSync) return;

            RecomputePassesForTest++;

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

            // I-42: con lado B el diseno lo arma el ensamblador COMPUESTO, que COMPONE al de un sentido; sin lado B
            // el camino es literalmente el de antes de la iniciativa. Las vistas se construyen SIEMPRE por el mismo
            // BuildFrom, asi que no puede aparecer un segundo constructor de vistas que diverja.
            var computation = composite.SideBPresent
                // I-42: los dos cortes frontales se construyen para el lado que el SELECTOR DE VISTA pide. El lado
                // ACTIVO es el que se esta editando y no tiene por que ser el que se mira: con «Editando A» y
                // «Frontal de B» el panel construia el corte de A y lo rotulaba como de B.
                ? assembler.BuildFrom(compositeAssembler.BuildDesign(composite, ReadInputs()), frontalSide)
                : assembler.Build(state, ReadInputs());
            if (computation.IsValid)
            {
                session.SetModel(computation.Design, computation.System);
                assembler.AcceptComputation(state, computation); // advance the opaque baseline (never mutated by the window)
                lastComputation = computation;
                hasValidModel = true;
                currentInputsAreValid = true;
                RenderPushBackMatrix();
                UpdateViewSelector();
                RenderPreview();
                // PB-013: the general panel mirrors the cell that was just committed. SetNumber only re-validates the
                // field, it never raises LostFocus, so this cannot re-enter the edit path.
                var wasSuppressed = suppressSync;
                suppressSync = true;
                try { MirrorSelectedCellPallet(SelectedCell()); }
                finally { suppressSync = wasSuppressed; }

                // I-35: the accepted module edit has landed on the new baseline, so it must not be re-applied by an
                // unrelated recompute (an individual restore would otherwise fire again and again). The module list is
                // rebuilt from the new structure and the reconciliation report reaches the panel.
                state.ClearModuleCommit();
                RefreshModuleSelector();
                RefreshHeaderDestinations();

                RefreshCompositePanel(computation.System);
                SetStatus(CompositeStatusOr("Vista recalculada."), CompositeHasBlocking(computation.System));
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

        // PB-013: FrontBox/PalletHeightBox/WeightBox are mirrors, not inputs — they are still validated so a mirrored
        // value out of range is reported rather than silently drawn.
        private NumericField[] AllNumericFields() => new[]
        {
            FrontBox, DepthBox, PalletHeightBox, WeightBox, PalletsDeepBox, ToleranceBox, PostPeralteBox,
            BeamDepthBox, AnnotationScaleBox, FrontCountBox, PositionsBox, LevelsBox, FondosBox,
            DepthStartBox, FirstLevelHeightBox, CellPalletFrontBox, CellPalletHeightBox, CellPalletWeightBox,
            CellClearBox, CellBeamLengthOverrideBox, CellFondoOverrideBox
        };

        /// <summary>Read the cell panel (shared + Push Back values) and apply it to the primary selected cell + its front.</summary>
        private void CommitCurrentCell()
        {
            if (state.Structure.Count == 0) return;
            ForEachEditedSide(side => side.CommitEditorValues(ReadCellValues(side)));
        }

        /// <summary>
        /// Ejecuta una escritura sobre los lados que la seleccion de edicion alcanza: uno, o los DOS cuando el
        /// usuario eligio «Ambos». Es el UNICO sitio donde «Ambos» se materializa, asi que ningun campo puede
        /// quedarse a medias ni inventarse una regla propia.
        ///
        /// <para>
        /// Antes de escribir en el otro lado se le lleva la MISMA seleccion de celda: si no, «esta celda» significaria
        /// dos cosas distintas y un alcance escribiria donde nadie pidio.
        /// </para>
        /// </summary>
        private void ForEachEditedSide(Action<PushBackEditorState> write)
        {
            if (composite.ActiveSelection == PushBackSideSelection.Both)
            {
                composite.MirrorSelection();
            }

            foreach (var side in composite.EditTargets())
            {
                if (side.Structure.Count > 0)
                {
                    write(side);
                }
            }
        }

        private PushBackEditorValues ReadCellValues() => ReadCellValues(state);

        /// <summary>
        /// Lee el panel de la celda contra UN lado concreto.
        ///
        /// <para>
        /// El lado importa porque un campo VACIO no significa «cero»: significa «deja este valor como esta», y ese
        /// valor es el del lado que se escribe. Con «Ambos» y un campo en estado MIXTO —A y B tienen valores
        /// distintos— es exactamente lo que hace falta: si el usuario no escribe nada, cada lado conserva el suyo,
        /// en vez de que el de A pise silenciosamente al de B.
        /// </para>
        /// </summary>
        private PushBackEditorValues ReadCellValues(PushBackEditorState state)
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
            };
        }

        private PushBackEditorInputs ReadInputs()
        {
            var inputs = new PushBackEditorInputs
            {
                // PB-013: only Fondo and Unidad come from this panel. Frente/Alto/Peso keep the rack-wide values the
                // design was loaded with — the boxes showing them are a mirror of the cell, not an input.
                Pallet = new PalletSpecification(
                    generalPallet.Front, Val(DepthBox, generalPallet.Depth), generalPallet.Height,
                    generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),
                PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep),

                // I-42 (correccion aislada 3) — el datum de «Alto 1er nivel» se TRANSPORTA, no se fabrica. Este
                // metodo construye unos inputs NUEVOS en cada recalculo, y sus valores por defecto imponian el datum
                // del PRODUCTO a un documento que se guardo con la semantica historica: medido, un rack legacy con
                // Alto = 5 saltaba de 4.6053 a 6.6053 al recalcular. La ventana no es autoridad de datum.
                FirstLevelDatum = firstLevelDatum,

                // I-35 (Owner round 2): the four advanced RACK-WIDE scopes travel verbatim from their carrier.
                ManualHeaderHeightOverride = advanced.ManualHeaderHeightOverride,
                DerivedPostReinforced = advanced.DerivedPostReinforced,
                DerivedPostReinforcementHeight = advanced.DerivedPostReinforcementHeight,
                DerivedPostHeight = advanced.DerivedPostHeight,
                SeparatorCountOverride = advanced.SeparatorCountOverride,
                SeparatorSpacingOverride = advanced.SeparatorSpacingOverride,
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

        // ---- Cell selection matrix (the visible multi-selection; DynamicFrontMatrix stays the authority) ---------

        private void UpdatePrimaryIndicator()
            => CellSelectionPrimaryText.Text = state.Structure.Count > 0
                ? string.Format(CultureInfo.InvariantCulture, "Primaria: F{0} N{1} · {2} celda(s) seleccionada(s)",
                    state.Structure.SelectedFrontIndex + 1, state.Structure.SelectedLevelIndex + 1, state.Structure.SelectedCellCount)
                : string.Empty;

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

            // Column headers: "Frente N" (click selects that front's primary cell) + the Activo/En blanco toggle (I-33).
            for (var f = 0; f < fronts; f++)
            {
                var captured = f;
                var column = new StackPanel { Margin = new Thickness(2.0, 0.0, 2.0, 3.0) };
                var header = new TextBlock
                {
                    Text = "Frente " + (f + 1).ToString(CultureInfo.InvariantCulture),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                    Cursor = Cursors.Hand,
                    Foreground = f == state.Structure.SelectedFrontIndex ? CardPrimaryStroke : CardLabelBrush
                };
                header.MouseLeftButtonDown += (_, __) => SelectMatrixCell(captured, state.Structure.SelectedLevelIndex, false);
                column.Children.Add(header);

                // El frente en blanco conserva su claro y su estructura, desplaza los frentes posteriores y deja de
                // llevar niveles; su configuracion queda dormida y regresa al reactivarlo.
                var blank = new CheckBox
                {
                    Content = "En blanco",
                    IsChecked = !state.Structure.IsActive(captured),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0.0, 3.0, 0.0, 0.0),
                    FontSize = 10.5,
                    Foreground = CardLabelBrush,
                    ToolTip = "Conserva el claro y la estructura del frente, desplaza los frentes posteriores y no lleva "
                              + "niveles ni componentes de carga. Su configuracion se conserva para reactivarlo."
                };
                blank.Checked += (_, __) => SetFrontActive(captured, false);
                blank.Unchecked += (_, __) => SetFrontActive(captured, true);
                column.Children.Add(blank);
                AddMatrixElement(column, 0, f + 1);
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
            text.Foreground = card.IsActive ? CardTextBrush : CardGhostBrush;
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
            RefreshMatrixSelectionVisuals();
            UpdatePrimaryIndicator();
            RequestRecompute();
        }

        // ---- Structure handlers ----------------------------------------------------------------------------------

        private void FrontCount_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync || FrontCountBox.HasError) return;
            var requested = IntVal(FrontCountBox, state.Structure.Count);
            if (requested >= 1 && requested != state.Structure.Count)
            {
                // I-42: la retícula transversal es UNA. El numero de frentes es del RACK, no del lado activo, asi
                // que crece y decrece en los dos a la vez; la asimetria A/B se expresa con PRESENCIA por ranura.
                // Cuando el rack es de un solo sentido esto es literalmente lo de siempre.
                MutateStructure(() => composite.SetSlotCount(requested));
            }
        }

        /// <summary>
        /// Switches one front between Activo and En blanco (I-33). A blank front keeps its claro and its structure and
        /// still displaces the fronts behind it, but carries no level and therefore no larguero, cama, larguero
        /// posterior ni tope. Its configuration stays dormant, so the same box brings it back exactly as it was.
        /// </summary>
        private void SetFrontActive(int index, bool isActive)
        {
            if (suppressSync || index < 0 || index >= state.Structure.Count
                || state.Structure.IsActive(index) == isActive)
            {
                return;
            }

            if (!AllFieldsValid(out var error))
            {
                SetStatus(error, true);
                RenderPushBackMatrix();   // re-checks the box the refused click had flipped
                return;
            }

            var applied = false;
            // «En blanco» (I-33) es del frente de UN lado; con «Ambos» se aplica a los dos, que es lo que el usuario
            // acaba de pedir al elegir esa seleccion.
            //
            // I-42 (ronda post-82e918b): pasa por el estado COMPUESTO porque la guarda «al menos un frente activo»
            // es del RACK, no de cada lado. En un rack compuesto el lado B puede quedarse entero en blanco —es la
            // capacidad declarada y todavia sin usar— mientras el lado A lo sostenga; solo quien conoce los dos
            // lados puede conceder esa excepcion. Y como la presencia por lado se DERIVA de esta misma casilla,
            // esta es la unica autoridad visible para el caso.
            MutateStructure(() => ForEachEditedSide(side =>
                applied |= composite.SetSlotPresent(
                    ReferenceEquals(side, composite.SideB) ? PushBackSide.B : PushBackSide.A, index, isActive)));
            if (!applied)
            {
                SetStatus(
                    "Al menos un frente del rack debe permanecer activo.",
                    true);
                RenderPushBackMatrix();
                return;
            }

            SetStatus(
                isActive
                    ? string.Format(CultureInfo.InvariantCulture, "Frente {0} activo.", index + 1)
                    : string.Format(
                        CultureInfo.InvariantCulture,
                        "Frente {0} en blanco: conserva claro y estructura, sin niveles de carga.",
                        index + 1),
                false);
        }

        // I-42: añadir o quitar un frente cambia la RETICULA, que es del rack: los dos lados crecen y decrecen a la
        // vez, igual que al escribir el numero en el campo. La asimetria se declara con la presencia por frente.
        private void AddFront_Click(object sender, RoutedEventArgs e)
            => MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 1));

        private void RemoveFront_Click(object sender, RoutedEventArgs e)
            => MutateStructure(() => composite.SetSlotCount(Math.Max(1, state.Structure.Count - 1)));

        // Los NIVELES son de un lado, asi que siguen la seleccion de edicion: con «Ambos» suben o bajan en los dos.
        private void AddLevel_Click(object sender, RoutedEventArgs e)
            => MutateStructure(() => ForEachEditedSide(side => side.AdjustLevels(side.Structure.SelectedFrontIndex, 1)));

        private void RemoveLevel_Click(object sender, RoutedEventArgs e)
            => MutateStructure(() => ForEachEditedSide(side => side.AdjustLevels(side.Structure.SelectedFrontIndex, -1)));

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
                ForEachEditedSide(side => side.ApplyScope(ReadCellValues(side), scope));
                suppressSync = true;
                try
                {
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

        // ---- I-41: fondo y tarima por celda (PB-015 / PB-016) ----------------------------------------------------

        /// <summary>
        /// El alcance elegido en el desplegable propio de I-41. Son los CINCO de siempre
        /// (<see cref="DynamicRackCellScope"/>); no hay un sexto ni un modelo de seleccion paralelo.
        /// </summary>
        private DynamicRackCellScope SelectedPropertyScope()
            => (DynamicRackCellScope)Math.Min(
                (int)DynamicRackCellScope.All,
                Math.Max(0, CellPropertyScopeBox.SelectedIndex));

        /// <summary>
        /// El camino ordinario de edicion de las dos propiedades de I-41: escribir en el control y salir de el escribe
        /// la CELDA primaria, igual que hace cualquier otro campo de la celda. Los botones de alcance son para ir mas
        /// alla de esa celda; no son la unica forma de aplicar.
        /// </summary>
        private void CellFondo_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync) return;
            WriteCellProperties(DynamicRackCellScope.Cell, depth: true, pallet: false);
        }

        private void CellDrawPallet_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync) return;
            WriteCellProperties(DynamicRackCellScope.Cell, depth: false, pallet: true);
        }

        private void ApplyCellFondo_Click(object sender, RoutedEventArgs e)
            => WriteCellProperties(SelectedPropertyScope(), depth: true, pallet: false);

        private void ApplyCellPallet_Click(object sender, RoutedEventArgs e)
            => WriteCellProperties(SelectedPropertyScope(), depth: false, pallet: true);

        /// <summary>Restaurar el fondo = quitar el override; las celdas del alcance vuelven al default del frente.</summary>
        private void RestoreCellFondo_Click(object sender, RoutedEventArgs e)
        {
            WithSuppressedSync(() => CellFondoOverrideBox.SetNumber(null));
            WriteCellProperties(SelectedPropertyScope(), depth: true, pallet: false);
        }

        /// <summary>Restaurar la tarima = volver al default LEGACY, que es no dibujarla.</summary>
        private void RestoreCellPallet_Click(object sender, RoutedEventArgs e)
        {
            WithSuppressedSync(() => CellDrawPalletCheck.IsChecked = false);
            WriteCellProperties(SelectedPropertyScope(), depth: false, pallet: true);
        }

        /// <summary>
        /// Escribe en el estado SOLO la(s) propiedad(es) pedida(s) sobre el alcance, y recalcula UNA sola vez — el
        /// <see cref="RackEditorSession{TDesign,TSystem}.Recompute"/> queda diferido durante toda la operacion, asi que
        /// una aplicacion masiva a 300 celdas produce un unico paso de recalculo, no 300.
        /// </summary>
        private void WriteCellProperties(DynamicRackCellScope scope, bool depth, bool pallet)
        {
            // Un campo en error NO escribe nada en el estado, pero SI pide recalculo: es esa pasada la que marca los
            // controles, publica el motivo del bloqueo y deshabilita las acciones (contrato I-39). Salir en silencio
            // dejaria la ventana diciendo que sus entradas siguen siendo validas con un campo marcado en rojo.
            if (!AllFieldsValid(out _))
            {
                RequestRecompute();
                return;
            }

            using (session.Recompute.Defer())
            {
                var written = 0;
                if (depth)
                {
                    // Vacio = restaurar: sin override, la celda hereda su default.
                    var requested = CellFondoOverrideBox.Value;
                    var value = requested.HasValue ? (int?)(int)Math.Round(requested.Value) : null;

                    // I-42: el campo escribe la autoridad de la celda SELECCIONADA. En una cama corrida esa
                    // autoridad es el fondo propio de la corrida, no el de A ni el de B: escribir ahi los fondos de
                    // un lado seria editar una configuracion dormante y dejar la cama como estaba.
                    if (EditsCorridaDepth())
                    {
                        // El fondo de una corrida es de la CELDA compuesta, no de un lado: «Ambos» no lo duplica.
                        written = composite.ApplyCorridaDepth(value, scope);
                        ReportCorridaScope(scope, written);
                    }
                    else
                    {
                        var count = 0;
                        ForEachEditedSide(side => count = side.ApplyPalletsDeep(value, scope));
                        written = count;
                    }
                }

                if (pallet)
                {
                    var draw = CellDrawPalletCheck.IsChecked == true;
                    var count = 0;
                    ForEachEditedSide(side => count = side.ApplyDrawPallet(draw, scope));
                    written = count;
                }

                suppressSync = true;
                try
                {
                    RenderPushBackMatrix();
                    LoadSelectedFront();
                }
                finally
                {
                    suppressSync = false;
                }

                if (scope != DynamicRackCellScope.Cell && !(depth && EditsCorridaDepth()))
                {
                    SetStatus(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} aplicado a {1} celda(s).",
                        depth ? "Fondo" : "Tarima",
                        written), false);
                }

                RequestRecompute();
            }
        }

        // ---- Front data apply (structural values of the WHOLE frente, never the cell scopes) ----------------------

        private void ApplyFrontData_Click(object sender, RoutedEventArgs e)
            => ApplyFrontData(new[] { state.Structure.SelectedFrontIndex });

        private void ApplySelectedFrontData_Click(object sender, RoutedEventArgs e)
            => ApplyFrontData(state.Structure.SelectedCells()
                .Select(cell => cell.FrontIndex)
                .DefaultIfEmpty(state.Structure.SelectedFrontIndex)
                .Distinct());

        private void ApplyAllFrontData_Click(object sender, RoutedEventArgs e)
            => ApplyFrontData(Enumerable.Range(0, state.Structure.Count));

        /// <summary>
        /// Owner decision (2026-07-24): the FRENTE's structural data (positions, levels, fondos, start, first-level
        /// height) are applied by their own scopes — this frente / selected frentes / all — through the EXISTING
        /// authority <see cref="DynamicFrontMatrix.ApplyFrontValuesTo"/>. No second authority and no cell scope: the
        /// per-cell buffer keeps carrying only the cell's own values.
        /// </summary>
        private void ApplyFrontData(IEnumerable<int> targets)
        {
            if (!AllFieldsValid(out var error)) { SetStatus(error, true); return; }
            using (session.Recompute.Defer())
            {
                var frontTargets = targets.ToList();
                ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues(side).Dynamic, frontTargets));
                suppressSync = true;
                try
                {
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

        /// <summary>
        /// Test seam for the SAFETY dialog: given the current selections, returns what the user chose, or NULL when the
        /// dialog was cancelled. Default = show the real <see cref="SelectiveSafetyWindow"/>. Overriding it lets a test
        /// drive the REAL <c>Safety_Click</c> path (button click, authority, recompute, BOM) without a modal window.
        /// </summary>
        internal Func<IReadOnlyList<SelectiveSafetySelection>, IReadOnlyList<SelectiveSafetySelection>> SafetyDialog;

        /// <summary>
        /// Test seam for the REAR-TOPE dialog: given the projected config, returns the dialog's result, or NULL when it
        /// was cancelled. Default = show the real shared <see cref="SafetyTopeGridWindow"/>.
        /// </summary>
        internal Func<PushBackRearTopeConfig, IReadOnlyList<int>, SafetyTopeGridWindow.TopeResult> RearTopeDialog;

        /// <summary>
        /// I-42 (ronda 7D) — seam de prueba de la rejilla POR POSTE de un lado. Recibe la seccion, que es quien
        /// declara el lado y la aplicabilidad; devolver NULL es cancelar.
        /// </summary>
        internal Func<PushBackDefenseSection, IReadOnlyList<SafetyPostDefense>> DefenseDialog;

        /// <summary>
        /// PB-002 (I-32) — the desviador grid's level count PER POST: the canonical "tallest adjacent front owns the
        /// cut" rule of <see cref="DynamicFrontGeometry"/>, the very rule the drawing uses. The dialog used to receive a
        /// per-FRONT list and index it by post, so the last post (and every interior one next to a taller front) offered
        /// fewer levels than the drawing places — cells the user could see drawn but not switch off.
        /// </summary>
        /// <remarks>
        /// I-33: los conteos salen de la AUTORIDAD compartida. Un frente en blanco aporta CERO, y un poste cuyos unicos
        /// vecinos estan en blanco tambien, de modo que su columna del desviador se dibuja SIN celdas.
        /// </remarks>
        internal IReadOnlyList<int> DesviadorLevelsPerPost()
            => DynamicFrontActivation.EffectiveLevelsPerPost(state.Structure.EffectiveLevelCounts());

        // ---- Advanced: per-module editing (I-35 / PB-011) ---------------------------------------------------------

        /// <summary>Test seam: how the header configurator is shown. Production opens the shared window on a COPY and
        /// returns the edited copy; a test replaces the dialog without a real window. Returning null means "cancelled",
        /// and NOTHING is staged.</summary>
        internal Func<RackFrameConfiguration, RackFrameConfiguration> HeaderConfiguratorDialog { get; set; }

        /// <summary>Test seam: the pure editor state the window drives, so an STA test can assert on the session, the
        /// baseline and the reconciliation report without reaching through private fields.</summary>
        internal PushBackEditorState EditorStateForTest => state;

        private void AdvancedModules_Changed(object sender, RoutedEventArgs e)
        {
            if (AdvancedModulesPanel == null)
            {
                return;
            }

            AdvancedModulesPanel.Visibility = AdvancedModulesToggle.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (AdvancedModulesToggle.IsChecked == true)
            {
                LoadAdvancedRackParameters();
                RefreshModuleSelector();
            }
        }

        // ---- Advanced RACK-WIDE parameters (I-35, Owner round 2) --------------------------------------------------
        // Height, derived-post reinforcement and the two separator overrides belong to the WHOLE rack, never to the
        // selected Separator module: they have their own section and their own handler, and they are carried to the
        // authorities that already own them by PushBackAdvancedRackParameters. Nothing here restates a rule.

        /// <summary>Show the four scopes as the loaded inputs have them; empty means the standing calculation.</summary>
        private void LoadAdvancedRackParameters()
        {
            if (RackHeaderHeightBox == null)
            {
                return;
            }

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                SetOptional(RackHeaderHeightBox, advanced.ManualHeaderHeightOverride);
                DerivedPostReinforcedCheck.IsChecked = advanced.DerivedPostReinforced;
                SetOptional(DerivedPostReinforcementHeightBox, advanced.DerivedPostReinforcementHeight);
                SetOptional(DerivedPostHeightBox, advanced.DerivedPostHeight);
                SetOptional(RackSeparatorCountBox, advanced.SeparatorCountOverride);
                SetOptional(RackSeparatorSpacingBox, advanced.SeparatorSpacingOverride);
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            UpdateReinforcementHeightSensitivity();
        }

        /// <summary>An optional numeric field: null clears it, which is how the user expresses "use the calculation".</summary>
        private static void SetOptional(NumericField field, double? value)
        {
            if (field == null) return;
            field.SetNumber(value);   // null clears the field: that is how "use the calculation" is expressed
        }

        private static void SetOptional(NumericField field, int? value)
            => SetOptional(field, value.HasValue ? (double?)value.Value : null);

        /// <summary>Read the four scopes back from the panel. An EMPTY field is null — the calculation — and never zero.</summary>
        private void ReadAdvancedRackParameters()
        {
            if (RackHeaderHeightBox == null)
            {
                return;
            }

            advanced.ManualHeaderHeightOverride = RackHeaderHeightBox.Value;
            advanced.DerivedPostReinforced = DerivedPostReinforcedCheck.IsChecked == true;
            advanced.DerivedPostReinforcementHeight = DerivedPostReinforcementHeightBox.Value;
            advanced.DerivedPostHeight = DerivedPostHeightBox.Value;
            advanced.SeparatorCountOverride = RackSeparatorCountBox.Value.HasValue
                ? (int?)(int)Math.Round(RackSeparatorCountBox.Value.Value)
                : null;
            advanced.SeparatorSpacingOverride = RackSeparatorSpacingBox.Value;
        }

        /// <summary>The reinforcement length is meaningless with no reinforcement: disabled, with the reason visible.</summary>
        private void UpdateReinforcementHeightSensitivity()
            => SetBlankSensitive(
                DerivedPostReinforcementHeightBox,
                DerivedPostReinforcedCheck.IsChecked == true,
                "El poste derivado no lleva refuerzo: activa «Reforzar poste derivado» para fijar su altura.");

        private void AdvancedRackParameter_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            ReadAdvancedRackParameters();
            UpdateReinforcementHeightSensitivity();
            RequestRecompute();
        }

        private void RestoreRackParameters_Click(object sender, RoutedEventArgs e)
        {
            state.RestoreAdvancedRackParameters(advanced);
            LoadAdvancedRackParameters();
            RequestRecompute();
            SetModuleStatus("Parametros globales del rack restaurados.");
        }

        /// <summary>
        /// Rebuild the module list from the session, preserving the selected module id when it survives. Called after
        /// every valid recompute, so the list always names the modules the rack actually has.
        /// </summary>
        private void RefreshModuleSelector()
        {
            if (ModuleBox == null)
            {
                return;
            }

            var selectedId = SelectedModuleId();
            var descriptors = state.ModuleSession.Modules;

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                var ordinals = KindOrdinals();
                ModuleBox.ItemsSource = descriptors
                    .Select(descriptor => ModuleLabel(descriptor, ordinals[descriptor.ModuleId]))
                    .ToList();
                moduleIds = descriptors.Select(descriptor => descriptor.ModuleId).ToList();

                var index = selectedId == null ? -1 : moduleIds.IndexOf(selectedId);
                ModuleBox.SelectedIndex = index >= 0 ? index : (moduleIds.Count > 0 ? 0 : -1);
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            LoadSelectedModule();
        }

        private List<string> moduleIds = new List<string>();

        private string SelectedModuleId()
            => ModuleBox != null && ModuleBox.SelectedIndex >= 0 && ModuleBox.SelectedIndex < moduleIds.Count
                ? moduleIds[ModuleBox.SelectedIndex]
                : null;

        private RackModuleDescriptor SelectedModule()
        {
            var id = SelectedModuleId();
            return id == null
                ? null
                : state.ModuleSession.Modules.FirstOrDefault(module => module.ModuleId == id)
                  ?? DescribeFromLastComputation(id);
        }

        /// <summary>The physically-present flag only exists on a RESOLVED system, so it is read from the last valid
        /// computation; the session's own descriptors describe intents and cannot know it.</summary>
        private RackModuleDescriptor DescribeFromLastComputation(string moduleId)
            => lastComputation?.System?.Structure == null
                ? null
                : RackModuleDescriptor.Describe(lastComputation.System.Structure)
                    .FirstOrDefault(module => module.ModuleId == moduleId);

        /// <summary>
        /// The ordinal of each module WITHIN ITS KIND — cabeceras 1..n and separadores 1..m — which is how a user
        /// counts them. The MODULE ordinal interleaves both, so the cabeceras of a real rack read 1, 3, 6, 8 and
        /// «Cabecera 2» names nothing the user can find (Owner, ronda 2).
        /// </summary>
        private Dictionary<string, int> KindOrdinals()
        {
            var ordinals = new Dictionary<string, int>(StringComparer.Ordinal);
            var headers = 0;
            var separators = 0;
            foreach (var module in state.ModuleSession.Modules)
            {
                ordinals[module.ModuleId] = module.IsHeader ? ++headers : ++separators;
            }

            return ordinals;
        }

        /// <summary>«Cabecera 2» / «Separador 1» — the module named as the user counts it.</summary>
        private string ModuleDisplayName(string moduleId)
        {
            var module = state.ModuleSession.Modules.FirstOrDefault(candidate => candidate.ModuleId == moduleId);
            if (module == null)
            {
                return moduleId;
            }

            KindOrdinals().TryGetValue(moduleId, out var ordinal);
            return ModuleDisplayName(module, ordinal);
        }

        private static string ModuleDisplayName(RackModuleDescriptor module, int kindOrdinal)
            => string.Format(
                CultureInfo.CurrentCulture,
                "{0} {1}",
                module.IsHeader ? "Cabecera" : "Separador",
                kindOrdinal);

        private static string ModuleLabel(RackModuleDescriptor module, int kindOrdinal)
        {
            var marks = new List<string>();
            if (module.IsManualOverride) marks.Add("medida propia");
            if (module.HasCustomHeaderConfiguration) marks.Add("cabecera propia");
            if (!module.IsLengthBearing) marks.Add("poste derivado");

            return string.Format(
                CultureInfo.CurrentCulture,
                "{0} · {1:0.##} in{2}",
                ModuleDisplayName(module, kindOrdinal),
                module.Length,
                marks.Count == 0 ? string.Empty : " · " + string.Join(", ", marks));
        }

        private void Module_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            LoadSelectedModule();
        }

        /// <summary>
        /// Load the selected module into the panel and decide, WITH A REASON, what may be edited. Three gates:
        /// a module with no longitudinal run (a derived post) has no length to change; a module whose physical assembly
        /// I-33 suppressed is drawn nowhere and must not be edited; and an invalid rack blocks everything.
        /// </summary>
        private void LoadSelectedModule()
        {
            var module = SelectedModule();
            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                if (module == null)
                {
                    ModuleInfoText.Text = "Sin modulos todavia: recalcula el sistema para poder editarlos.";
                    ModuleLengthBox.SetNumber(0.0);
                    SetModuleSensitive(false, "Todavia no hay una estructura valida que editar.");
                    RefreshCopySources();
                    ModuleStatusText.Text = string.Empty;
                    return;
                }

                var resolved = DescribeFromLastComputation(module.ModuleId);
                var physicallyPresent = resolved?.IsPhysicallyPresent ?? true;

                ModuleInfoText.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} · X {1:0.##} a {2:0.##} · {3}",
                    module.IsHeader ? "Cabecera" : "Separador",
                    module.StartX,
                    module.EndX,
                    module.IsHeader
                        ? (module.HasCustomHeaderConfiguration ? "cabecera personalizada" : "cabecera calculada")
                        : "solo consume longitud");

                ModuleLengthBox.SetNumber(module.Length);
                ModuleHeaderPanel.Visibility = module.IsHeader ? Visibility.Visible : Visibility.Collapsed;
                ModuleCustomRadio.IsChecked = module.HasCustomHeaderConfiguration;
                ModuleCalculatedRadio.IsChecked = !module.HasCustomHeaderConfiguration;

                if (!currentInputsAreValid)
                {
                    SetModuleSensitive(false, "Corrige los campos numericos marcados antes de editar modulos.");
                }
                else if (!physicallyPresent)
                {
                    SetModuleSensitive(
                        false,
                        "Este modulo no se dibuja en ningun corte: los frentes en blanco suprimieron los postes donde aparecia (I-33). "
                        + "Reactiva un frente para poder editarlo.");
                }
                else if (!module.IsLengthBearing)
                {
                    SetModuleSensitive(false, "Es un poste derivado: no consume longitud, asi que no hay medida que editar.");
                }
                else
                {
                    SetModuleSensitive(true, null);
                }

                RetargetSelectedHeader();
                RefreshCopySources();
                RefreshModuleStatus();
            }
            finally
            {
                suppressSync = wasSuppressed;
            }
        }

        /// <summary>Enable or disable the whole per-module surface with the SAME reason on every control, reusing the
        /// I-33 helper so the original tooltip comes back when it is re-enabled.</summary>
        private void SetModuleSensitive(bool enabled, string reason)
        {
            foreach (var control in new Control[]
                     {
                         ModuleLengthBox, ModuleCalculatedRadio, ModuleCustomRadio,
                         ConfigureModuleHeaderButton, RestoreModuleButton,
                         CopyHeaderFromBox, CopyHeaderFromButton,
                         HeaderTargetsList, HeaderTargetsThisButton, HeaderTargetsAllButton,
                         HeaderLinesList, HeaderLinesThisButton, HeaderLinesAllButton,
                         ApplyHeaderSelectionButton, DerivedPostLineHeightBox, ApplyDerivedPostLinesButton
                     })
            {
                SetBlankSensitive(control, enabled, reason);
            }

            // "Personalizada" is never clickable: it is a READOUT of provenance that only "Configurar cabecera…" sets.
            if (enabled)
            {
                ModuleCustomRadio.IsEnabled = false;
            }
        }

        private void ModuleLength_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            var module = SelectedModule();
            if (module == null || ModuleLengthBox.HasError)
            {
                return;
            }

            var length = Val(ModuleLengthBox, module.Length);
            if (length <= 0.0 || Math.Abs(length - module.Length) < 0.0001)
            {
                return;
            }

            state.ModuleSession.SetLength(module.ModuleId, length);
            RefreshModuleStatus();
        }

        private void ModuleCalculated_Checked(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            var module = SelectedModule();
            if (module == null || !module.IsHeader)
            {
                return;
            }

            state.ModuleSession.ResetHeaderToCalculated(module.ModuleId);
            RefreshModuleStatus();
        }

        /// <summary>
        /// Open the SHARED header configurator on an independent canonical COPY. The configurator is not modified in
        /// any way (Owner decision 5): it mutates the copy it was handed, and the copy only becomes a staged edit here.
        /// Cancelling the whole module edit later still throws it away, which is the confirm/cancel the base lacks.
        /// </summary>
        private void ConfigureModuleHeader_Click(object sender, RoutedEventArgs e)
        {
            var module = SelectedModule();
            if (module == null || !module.IsHeader)
            {
                SetModuleStatus("Selecciona una cabecera para configurarla.");
                return;
            }

            // I-40 (ronda 3): para una cabecera declarada PERSONALIZADA la unica fuente valida es su propia
            // configuracion. El fallback al ultimo sistema resuelto solo puede servir a una cabecera CALCULADA —ahi
            // la calculada ES lo correcto—; servirlo bajo la etiqueta «Personalizada» era entregar los datos
            // predeterminados y, al copiarla, propagarlos. El invariante de la sesion hace ese caso imposible, y si
            // aun asi ocurriera se bloquea con diagnostico en vez de degradar en silencio.
            // I-40: con alcance por LINEA se edita la cabecera FISICA de esa linea, asi que se abre sobre lo que esa
            // linea dibuja hoy (su override si lo tiene, y si no la del modulo).
            // Se abre sobre lo que esa cabecera FISICA dibuja hoy —su configuracion de linea si la tiene, y si no
            // la del modulo—, sea cual sea el alcance: el ORIGEN es siempre la instancia que el usuario ve; lo que
            // el alcance decide es el DESTINO.
            var copy = state.ModuleSession.HeaderConfigurationCopy(module.ModuleId, SourceLine());
            if (copy == null && module.HasCustomHeaderConfiguration)
            {
                SetModuleStatus(
                    ModuleDisplayName(module.ModuleId) + " figura como personalizada pero su configuracion no esta "
                    + "disponible: no se abre el configurador para no reemplazarla por la predeterminada. "
                    + "Usa «Restaurar modulo» para devolverla al calculo y configurarla de nuevo.");
                return;
            }

            copy = copy ?? HeaderConfigurationFromLastComputation(module.ModuleId);
            if (copy == null)
            {
                SetModuleStatus("Esta cabecera todavia no tiene una configuracion que editar.");
                return;
            }

            // Ya personalizada = esta INSTANCIA fisica lo esta: por su propia configuracion de linea o por la del
            // modulo. De eso depende que el configurador se abra para EDITAR en vez de para generar (ronda 2).
            var alreadyCustom = state.ModuleSession.HasLineOverride(module.ModuleId, SourceLine())
                                || module.HasCustomHeaderConfiguration;

            var edited = HeaderConfiguratorDialog != null
                ? HeaderConfiguratorDialog(copy)
                : ShowHeaderConfigurator(copy, alreadyCustom);
            if (edited == null)
            {
                return;
            }

            StageHeaderConfiguration(module, edited);
        }

        /// <summary>
        /// PBH-03: reuse the configuration of ANOTHER cabecera of this session on the current targets, as an
        /// INDEPENDENT copy. Nothing is stored: the source is a module of this rack, not a library entry, and the
        /// copy lands in the module intent the design already carries.
        /// </summary>
        private void CopyHeaderFrom_Click(object sender, RoutedEventArgs e)
        {
            var module = SelectedModule();
            if (module == null || !module.IsHeader)
            {
                SetModuleStatus("Selecciona una cabecera de destino.");
                return;
            }

            var sourceId = SelectedCopySourceId();
            if (sourceId == null)
            {
                SetModuleStatus("Elige la cabecera de origen que quieres copiar.");
                return;
            }

            // The source's REAL personalization, read from the session and from nowhere else. There is deliberately
            // NO fallback to the last resolved system here (Owner, ronda 2): that fallback would let «copiar mi
            // configuracion» hand out a recalculated cabecera. Only custom cabeceras are offered as origin, so a
            // null here means the list and the session disagree, and that is reported instead of guessed.
            // El origen es la configuracion REAL de esa cabecera en la linea seleccionada: si esa instancia tiene
            // la suya, es esa; si no, la del modulo. Nunca una recalculada.
            var source = state.ModuleSession.SourceConfigurationCopy(sourceId, SourceLine());
            if (source == null)
            {
                SetModuleStatus(
                    ModuleDisplayName(sourceId) + " ya no tiene una configuracion personalizada que copiar.");
                RefreshCopySources();
                return;
            }

            StageHeaderConfiguration(module, source);
        }

        /// <summary>
        /// I-40 (Owner, ronda 5) — la configuracion ORIGEN, independiente de los destinos.
        /// <para>
        /// Este era el defecto UX central: el alcance solo se evaluaba cuando esta funcion recibia una configuracion
        /// NUEVA, asi que cambiarlo despues no constituia una operacion — habia que volver a abrir «Configurar
        /// cabecera...» para que el nuevo alcance tuviera efecto. Ahora la configuracion se RECUERDA como origen y
        /// «Aplicar configuracion a la seleccion» la reparte tantas veces como haga falta, sobre las selecciones que
        /// haga falta, sin volver a configurar nada.
        /// </para>
        /// </summary>
        private RackFrameConfiguration pendingHeaderConfiguration;
        private string pendingHeaderSourceName = string.Empty;

        /// <summary>Test seam: the configuration currently held as ORIGIN.</summary>
        internal RackFrameConfiguration PendingHeaderConfigurationForTest => pendingHeaderConfiguration;

        /// <summary>
        /// Adopt a configuration as ORIGIN and apply it to the current selection. Configuring a cabecera edits THAT
        /// cabecera, so the operation runs once here; from then on the same configuration can be re-applied to any
        /// other selection without touching the configurator again.
        /// </summary>
        private void StageHeaderConfiguration(RackModuleDescriptor module, RackFrameConfiguration configuration)
        {
            pendingHeaderConfiguration = configuration;
            pendingHeaderSourceName = string.Format(
                CultureInfo.CurrentCulture,
                "{0} · Linea {1}",
                ModuleDisplayName(module.ModuleId),
                SourceLine() + 1);

            ApplyPendingConfiguration();
        }

        private void ApplyHeaderSelection_Click(object sender, RoutedEventArgs e)
        {
            if (pendingHeaderConfiguration == null)
            {
                SetModuleStatus(
                    "Todavia no hay configuracion de origen: usa «Configurar cabecera...» o toma la de otra cabecera.");
                return;
            }

            ApplyPendingConfiguration();
        }

        /// <summary>
        /// THE operation: the origin configuration onto the CARTESIAN PRODUCT of the selected cabeceras and the
        /// selected lines. Atomic in the session — every module and every line is validated and the whole product
        /// resolved before anything moves — and each destination receives its OWN deep copy.
        /// </summary>
        private void ApplyPendingConfiguration()
        {
            var modules = SelectedTargetModuleIds();
            var lines = SelectedTargetLines();
            if (modules.Count == 0 || lines.Count == 0)
            {
                SetModuleStatus("Selecciona al menos una cabecera y al menos una linea de destino.");
                return;
            }

            var result = state.ModuleSession.ApplyHeaderConfigurationToInstances(
                pendingHeaderConfiguration, modules, lines);
            if (!result.Applied)
            {
                SetModuleStatus(result.RejectionReason);
                return;
            }

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                ModuleCustomRadio.IsChecked = true;
                ModuleCalculatedRadio.IsChecked = false;
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            var message = DescribeApplied(modules, lines);
            RefreshModuleStatus();
            SetModuleStatus(message);
        }

        /// <summary>
        /// I-40 — la altura del poste derivado en las LINEAS seleccionadas. El poste derivado nace entre dos
        /// separadores consecutivos, asi que pertenece a la LINEA y no a ninguna cabecera: usa el mismo eje de
        /// lineas y la misma transaccion, pero no el de cabeceras.
        /// </summary>
        private void ApplyDerivedPostLines_Click(object sender, RoutedEventArgs e)
        {
            var lines = SelectedTargetLines();
            if (lines.Count == 0)
            {
                SetModuleStatus("Selecciona al menos una linea de destino.");
                return;
            }

            if (DerivedPostLineHeightBox.HasError)
            {
                SetModuleStatus("Corrige la altura del poste derivado antes de aplicarla.");
                return;
            }

            var height = DerivedPostLineHeightBox.Value;
            var result = state.ModuleSession.ApplyDerivedPostHeightToLines(height, lines);
            if (!result.Applied)
            {
                SetModuleStatus(result.RejectionReason);
                return;
            }

            RefreshModuleStatus();
            SetModuleStatus(string.Format(
                CultureInfo.CurrentCulture,
                height.HasValue
                    ? "Altura del poste derivado aplicada a {0} linea(s). Queda pendiente de Confirmar."
                    : "Las {0} linea(s) vuelven a la altura de poste derivado del rack. Queda pendiente de Confirmar.",
                lines.Count));
        }

        /// <summary>What the operation just did, in the user's words: how many PHYSICAL cabeceras, which ones and on
        /// which lines — named as the user counts them, never by module id.</summary>
        private string DescribeApplied(IReadOnlyList<string> moduleIds, IReadOnlyList<int> lines)
        {
            var names = moduleIds.Select(ModuleDisplayName).ToList();
            var lineNames = lines
                .Select(line => string.Format(CultureInfo.CurrentCulture, "Linea {0}", line + 1))
                .ToList();

            return string.Format(
                CultureInfo.CurrentCulture,
                "Configuracion aplicada a {0} cabecera(s) fisica(s): {1} en {2}. Queda pendiente de Confirmar.",
                names.Count * lineNames.Count,
                string.Join(", ", names),
                string.Join(", ", lineNames));
        }

        /// <summary>
        /// I-40 (Owner, ronda 5) — LOS DOS EJES. Los destinos de una operacion son el PRODUCTO CARTESIANO de las
        /// cabeceras seleccionadas por las lineas seleccionadas. Ya no hay «alcance»: hay dos selecciones
        /// independientes, y elegirlas no modifica nada — solo «Aplicar configuracion a la seleccion» lo hace.
        /// </summary>
        private IReadOnlyList<string> SelectedTargetModuleIds()
            => HeaderTargetsList == null
                ? Array.Empty<string>()
                : HeaderTargetsList.SelectedItems.Cast<string>()
                    .Select(label => headerTargetLabels.IndexOf(label))
                    .Where(index => index >= 0 && index < headerTargetIds.Count)
                    .Select(index => headerTargetIds[index])
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

        private IReadOnlyList<int> SelectedTargetLines()
            => HeaderLinesList == null
                ? Array.Empty<int>()
                : HeaderLinesList.SelectedItems.Cast<string>()
                    .Select(label => headerLineLabels.IndexOf(label))
                    .Where(index => index >= 0 && index < headerLineIndexes.Count)
                    .Select(index => headerLineIndexes[index])
                    .Distinct()
                    .OrderBy(line => line)
                    .ToList();

        /// <summary>The line the ORIGIN instance sits on: the first selected line, or the rack's first.</summary>
        private int SourceLine()
        {
            var selected = SelectedTargetLines();
            if (selected.Count > 0)
            {
                return selected[0];
            }

            return headerLineIndexes.Count > 0 ? headerLineIndexes[0] : 0;
        }

        private List<string> headerTargetIds = new List<string>();
        private List<string> headerTargetLabels = new List<string>();
        private List<int> headerLineIndexes = new List<int>();
        private List<string> headerLineLabels = new List<string>();

        /// <summary>
        /// Rebuild both destination lists from the rack the last valid recompute produced, preserving what the user
        /// had selected. The lines offered are the boundaries I-33 says exist — the same ones the lateral draws.
        /// </summary>
        private void RefreshHeaderDestinations()
        {
            if (HeaderTargetsList == null || HeaderLinesList == null)
            {
                return;
            }

            var previousModules = SelectedTargetModuleIds().ToList();
            var previousLines = SelectedTargetLines().ToList();

            var ordinals = KindOrdinals();
            var headers = state.ModuleSession.Modules.Where(module => module.IsHeader).ToList();
            var structure = lastComputation?.System?.Structure;
            var lines = structure == null
                ? new List<int>()
                : DynamicFrontActivation.PresentBoundaries(structure).ToList();

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                headerTargetIds = headers.Select(module => module.ModuleId).ToList();
                headerTargetLabels = headers
                    .Select(module => ModuleDisplayName(module, ordinals[module.ModuleId]))
                    .ToList();
                HeaderTargetsList.ItemsSource = headerTargetLabels;

                headerLineIndexes = lines;
                headerLineLabels = lines
                    .Select(line => string.Format(CultureInfo.CurrentCulture, "Linea {0}", line + 1))
                    .ToList();
                HeaderLinesList.ItemsSource = headerLineLabels;

                RestoreSelection(
                    HeaderTargetsList,
                    headerTargetLabels,
                    previousModules.Select(id => headerTargetIds.IndexOf(id)).ToList());
                RestoreSelection(
                    HeaderLinesList,
                    headerLineLabels,
                    previousLines.Select(line => headerLineIndexes.IndexOf(line)).ToList());

                // Sin seleccion previa, la operacion arranca apuntando a la cabecera y la linea que el usuario tiene
                // delante: el caso mas frecuente listo, y los demas a un clic.
                if (HeaderTargetsList.SelectedItems.Count == 0)
                {
                    SelectThisHeaderTarget();
                }

                if (HeaderLinesList.SelectedItems.Count == 0 && headerLineLabels.Count > 0)
                {
                    HeaderLinesList.SelectedItems.Add(headerLineLabels[0]);
                }
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            RefreshHeaderSourceText();
        }

        private static void RestoreSelection(ListBox list, List<string> labels, List<int> indexes)
        {
            list.SelectedItems.Clear();
            foreach (var index in indexes.Where(index => index >= 0 && index < labels.Count))
            {
                list.SelectedItems.Add(labels[index]);
            }
        }

        private void SelectThisHeaderTarget()
        {
            var selected = SelectedModule();
            var index = selected == null ? -1 : headerTargetIds.IndexOf(selected.ModuleId);
            HeaderTargetsList.SelectedItems.Clear();
            if (index >= 0)
            {
                HeaderTargetsList.SelectedItems.Add(headerTargetLabels[index]);
            }
        }

        /// <summary>
        /// Cambiar de cabecera en el selector de modulo MUEVE el destino cuando el usuario tiene UNA sola elegida:
        /// es lo que espera al ir de una cabecera a otra. Si ha elegido varias a proposito, no se le deshace.
        /// </summary>
        private void RetargetSelectedHeader()
        {
            if (HeaderTargetsList == null || HeaderTargetsList.SelectedItems.Count > 1)
            {
                return;
            }

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try { SelectThisHeaderTarget(); }
            finally { suppressSync = wasSuppressed; }
            RefreshHeaderSourceText();
        }

        private void HeaderTargetsThis_Click(object sender, RoutedEventArgs e)
            => WithSuppressedSync(SelectThisHeaderTarget);

        private void HeaderTargetsAll_Click(object sender, RoutedEventArgs e)
            => WithSuppressedSync(() => HeaderTargetsList.SelectAll());

        private void HeaderLinesThis_Click(object sender, RoutedEventArgs e)
            => WithSuppressedSync(() =>
            {
                HeaderLinesList.SelectedItems.Clear();
                if (headerLineLabels.Count > 0)
                {
                    HeaderLinesList.SelectedItems.Add(headerLineLabels[0]);
                }
            });

        private void HeaderLinesAll_Click(object sender, RoutedEventArgs e)
            => WithSuppressedSync(() => HeaderLinesList.SelectAll());

        /// <summary>Selecting destinations changes NOTHING by itself (Owner): it only updates what the panel says
        /// the next «Aplicar» would reach.</summary>
        private void HeaderDestinations_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            RefreshHeaderSourceText();
        }

        private void WithSuppressedSync(Action action)
        {
            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                action();
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            RefreshHeaderSourceText();
        }

        /// <summary>Say WHICH configuration is the origin and HOW MANY physical cabeceras the selection covers.</summary>
        private void RefreshHeaderSourceText()
        {
            if (HeaderSourceText == null)
            {
                return;
            }

            var modules = SelectedTargetModuleIds();
            var lines = SelectedTargetLines();
            var origin = pendingHeaderConfiguration == null
                ? "Sin configuracion de origen todavia: usa «Configurar cabecera...» o toma una de otra cabecera."
                : "Origen: " + pendingHeaderSourceName + ".";

            HeaderSourceText.Text = string.Format(
                CultureInfo.CurrentCulture,
                "{0} Destino: {1} cabecera(s) x {2} linea(s) = {3} cabecera(s) fisica(s).",
                origin,
                modules.Count,
                lines.Count,
                modules.Count * lines.Count);
        }

        private List<string> copySourceIds = new List<string>();

        private string SelectedCopySourceId()
            => CopyHeaderFromBox != null
               && CopyHeaderFromBox.SelectedIndex >= 0
               && CopyHeaderFromBox.SelectedIndex < copySourceIds.Count
                ? copySourceIds[CopyHeaderFromBox.SelectedIndex]
                : null;


        /// <summary>
        /// Rebuild the «Copiar configuracion de:» list: the other cabeceras that carry a REAL personalization.
        /// <para>
        /// Owner, ronda 2 (defecto 2), opcion A. Every cabecera holds a configuration, calculated ones included, so
        /// listing all of them let an action the user reads as «copiar mi configuracion» hand out a STANDARD
        /// cabecera without saying so — and applying that to every cabecera is exactly how the whole rack came back
        /// predetermined. Only a personalization that exists can be an origin; with none, the control is disabled
        /// and says why instead of quietly offering a surprise.
        /// </para>
        /// </summary>
        private void RefreshCopySources()
        {
            if (CopyHeaderFromBox == null)
            {
                return;
            }

            var selected = SelectedModule();
            var previous = SelectedCopySourceId();
            var ordinals = KindOrdinals();
            // Una cabecera es origen valido cuando tiene una personalizacion REAL: la del modulo, o la propia de
            // ESTA linea. Con la unidad de edicion en la instancia fisica, lo segundo es lo habitual.
            var sources = state.ModuleSession.Modules
                .Where(module => module.IsHeader
                                 && state.ModuleSession.HasAnyPersonalization(module.ModuleId))
                .Where(module => selected == null || module.ModuleId != selected.ModuleId)
                .ToList();

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                CopyHeaderFromBox.ItemsSource = sources
                    .Select(module => ModuleLabel(module, ordinals[module.ModuleId]))
                    .ToList();
                copySourceIds = sources.Select(module => module.ModuleId).ToList();

                var index = previous == null ? -1 : copySourceIds.IndexOf(previous);
                CopyHeaderFromBox.SelectedIndex = index >= 0 ? index : (copySourceIds.Count > 0 ? 0 : -1);
            }
            finally
            {
                suppressSync = wasSuppressed;
            }

            const string reason = "Todavia no hay otra cabecera personalizada de la que copiar: "
                                  + "configura una primero con «Configurar cabecera...».";
            SetBlankSensitive(CopyHeaderFromBox, copySourceIds.Count > 0, reason);
            SetBlankSensitive(CopyHeaderFromButton, copySourceIds.Count > 0, reason);
        }

        /// <summary>
        /// Test seam: how the configurator window is SHOWN. Production shows it modally; a test drives the REAL
        /// window and its REAL ViewModel without a modal loop. The read-back below is NOT part of the seam, so a
        /// test exercises the same statement production does — that is the whole point (I-40, PBH-01).
        /// </summary>
        internal Action<RackFrameConfiguratorWindow> HeaderConfiguratorPresenter { get; set; }

        /// <summary>
        /// Open the shared configurator on <paramref name="copy"/> and return the configuration that is ACTUALLY
        /// effective when it closes.
        /// <para>
        /// REGRESION I-40 (PBH-01): it must be read off the window, never assumed to be the instance handed in.
        /// <c>RackFrameConfiguratorViewModel</c> REPLACES its <c>Configuration</c> with a fresh clone on three
        /// paths — «Aplicar» de la configuracion rapida (<c>ApplySimpleConfiguration</c>, que es exactamente donde
        /// el usuario fija una ALTURA propia), «Restaurar estandar» y abrir un proyecto— so after any of them the
        /// instance we handed in is a STALE object and every edit of that session lives in another one. Returning
        /// it discarded the user's cabecera in silence.
        /// </para>
        /// </summary>
        private RackFrameConfiguration ShowHeaderConfigurator(RackFrameConfiguration copy, bool alreadyCustom)
        {
            var window = new RackFrameConfiguratorWindow(copy) { Owner = this };

            // REGRESION I-40 (ronda 2 del Owner): una cabecera YA personalizada se abre en el editor AVANZADO.
            // El configurador siempre arranca en «Configuracion rapida», y en ese modo la unica forma de cambiar el
            // alto es «Aplicar», que NO edita: RECONSTRUYE la cabecera desde la plantilla y conserva solo
            // alto/fondo/poste/peralte/nombre. Sobre una cabecera calculada eso es exactamente lo que se quiere
            // —generarla—, pero sobre una ya personalizada borra en silencio todo lo demas que el usuario habia
            // confirmado, y por eso «la altura funcionaba» mientras la cabecera volvia a la predeterminada.
            // Se elige el MODO desde aqui, con una propiedad publica del ViewModel: la ventana compartida no se toca.
            window.ViewModel.IsAdvancedEditor = alreadyCustom;

            if (HeaderConfiguratorPresenter != null)
            {
                HeaderConfiguratorPresenter(window);
            }
            else
            {
                window.ShowDialog();
            }

            return window.Configuration ?? copy;
        }

        private RackFrameConfiguration HeaderConfigurationFromLastComputation(string moduleId)
            => lastComputation?.System?.Structure?.Modules
                .FirstOrDefault(module => module.ModuleId == moduleId)?
                .AssociatedFrameConfiguration;

        private void ConfirmModule_Click(object sender, RoutedEventArgs e)
        {
            if (!state.ModuleSession.HasPendingChanges)
            {
                SetModuleStatus("No hay cambios de modulo pendientes.");
                return;
            }

            state.CommitModuleEdits();
            RequestRecompute();
        }

        private void CancelModule_Click(object sender, RoutedEventArgs e)
        {
            if (!state.ModuleSession.HasPendingChanges)
            {
                SetModuleStatus("No hay cambios de modulo pendientes.");
                return;
            }

            state.CancelModuleEdits();
            LoadSelectedModule();
            SetModuleStatus("Cambios de modulo descartados.");
        }

        private void RestoreModule_Click(object sender, RoutedEventArgs e)
        {
            var module = SelectedModule();
            if (module == null)
            {
                return;
            }

            state.ModuleSession.RestoreModule(module.ModuleId);
            LoadSelectedModule();
            SetModuleStatus("Modulo " + module.ModuleId + " marcado para restaurar. Confirma para aplicarlo.");
        }

        /// <summary>
        /// Rack-wide "restaurar estándar": every module customization goes, and the structure comes back from the
        /// standard build. It is the ONE explicit way to lose everything at once — the assembler turns it into the
        /// <c>forceRebuild</c> the base already accepted but no Push Back surface ever requested.
        /// </summary>
        private void RestoreAllModules_Click(object sender, RoutedEventArgs e)
        {
            state.ModuleSession.RequestStandardRestore();
            SetModuleStatus("Estructura estandar marcada para restaurar. Confirma para aplicarla.");
        }

        /// <summary>Pending-changes state plus what the LAST recompute did with the customizations — preserved,
        /// adapted, restored, removed or incompatible. A loss is never silent (Owner decision 3).</summary>
        private void RefreshModuleStatus()
        {
            if (ModuleStatusText == null)
            {
                return;
            }

            var pending = state.ModuleSession.HasPendingChanges;
            ConfirmModuleButton.IsEnabled = pending;
            CancelModuleButton.IsEnabled = pending;

            var parts = new List<string>();
            if (pending) parts.Add("Cambios pendientes: confirma o cancela.");

            var reconciliation = state.LastModuleReconciliation?.Describe();
            if (!string.IsNullOrEmpty(reconciliation)) parts.Add("Ultimo recalculo — " + reconciliation + ".");

            ModuleStatusText.Text = string.Join(" ", parts);
        }

        private void SetModuleStatus(string message)
        {
            RefreshModuleStatus();
            if (!string.IsNullOrEmpty(message))
            {
                ModuleStatusText.Text = message + (string.IsNullOrEmpty(ModuleStatusText.Text) ? string.Empty : " " + ModuleStatusText.Text);
            }
        }

        private void Safety_Click(object sender, RoutedEventArgs e)
        {
            var elements = SafetyElementsForDialog();
            var levels = PushBackRearTopeDialogAdapter.LevelsPerFrente(
                state.Structure.EffectiveLevelCounts(), allowBlankFronts: true);
            var postCount = Math.Max(2, state.Structure.Count + 1);

            // The rear stop is edited INSIDE this same dialog, as its own visible section (Owner decision 2026-07-24).
            // The section works on a COPY, so nothing is committed until the main dialog is accepted, and its grid opens
            // ONLY from its own "Configurar…" button — never automatically afterwards.
            // I-42 (ronda 7B) — un rack COMPUESTO tiene un tope por lado, y los DOS se editan aqui. Antes esta
            // ventana solo llegaba al lado activo, y por eso existia una segunda superficie en la ventana principal:
            // ahora la intencion de seguridad se decide en un solo sitio. Un rack de un solo sentido construye
            // exactamente una seccion sin etiqueta, como siempre.
            var composed = new StackPanel();

            // I-42 (ronda 7D, decision del dueño) — LA DEFENSA se organiza como los topes: una seccion POR LADO,
            // dentro de esta misma ventana. Antes la unica superficie hablaba de «entrada/salida» y «posterior»,
            // el vocabulario de un rack de un solo sentido, y el lado B no tenia donde editarse: toda decision
            // acababa aplicandose al lado A. La ventana principal no participa — abrir Seguridad con el lado
            // activo en A o en B ofrece exactamente lo mismo.
            // La familia DEFENSA se lee ANTES de abrir: al aceptar, la ventana reemplaza la lista de selecciones y
            // esta familia ya no viaja en ella —la deciden las secciones—, asi que su id se conserva aqui. Es el
            // portador de la intencion POR POSTE cuando los dos lados quedan en «Ninguno».
            var defenseCarrierId = DefenseSelection(safetySelections)?.ElementId;

            // I-42 (S1E, decision del dueño) — LAS BOTAS se organizan como los topes y la defensa: una seccion por
            // lado. En un compuesto «Entrada/Salida» no significa lo mismo para A que para B —cada uno tiene SU
            // pasillo—, asi que una fila global no podia expresar la intencion y cada vista acababa leyendola en su
            // propio marco. Un rack de un solo sentido conserva una sola seccion, sin etiqueta.
            var bootSectionA = BuildBootSection(PushBackSide.A, composite.SideBPresent ? "A" : null, postCount);
            BootSectionForTest = bootSectionA;
            composed.Children.Add(bootSectionA.View);

            PushBackBootSection bootSectionB = null;
            if (composite.SideBPresent)
            {
                bootSectionB = BuildBootSection(PushBackSide.B, "B", postCount);
                composed.Children.Add(bootSectionB.View);
            }

            BootSectionBForTest = bootSectionB;
            var defenseSectionA = BuildDefenseSection(PushBackSide.A, composite.SideBPresent ? "A" : null, postCount);
            DefenseSectionForTest = defenseSectionA;
            composed.Children.Add(defenseSectionA.View);

            PushBackDefenseSection defenseSectionB = null;
            if (composite.SideBPresent)
            {
                defenseSectionB = BuildDefenseSection(PushBackSide.B, "B", postCount);
                composed.Children.Add(defenseSectionB.View);
            }

            DefenseSectionBForTest = defenseSectionB;

            var topeSection = new PushBackRearTopeSection(
                composite.SideBPresent ? composite.Of(PushBackSide.A).RearTopeConfig() : state.RearTopeConfig(),
                // I-42 (ronda 7C, defecto del dueño) — CADA seccion abre su rejilla con los niveles de SU lado. Antes
                // las dos leian el lado ACTIVO, asi que un frente en blanco en A salia tambien en blanco en B aunque
                // B existiera: el blanco de un lado es del lado, no del rack.
                config => OpenRearTopeDialog(PushBackSide.A, config),
                catalog,
                composite.SideBPresent ? "A" : null);
            RearTopeSectionForTest = topeSection;
            composed.Children.Add(topeSection.View);

            PushBackRearTopeSection topeSectionB = null;
            if (composite.SideBPresent)
            {
                topeSectionB = new PushBackRearTopeSection(
                    composite.Of(PushBackSide.B).RearTopeConfig(),
                    config => OpenRearTopeDialog(PushBackSide.B, config),
                    catalog,
                    "B");
                composed.Children.Add(topeSectionB.View);
            }

            RearTopeSectionBForTest = topeSectionB;

            // CANCELLING the safety dialog abandons the WHOLE Seguridad step: neither the safety list nor the rear-tope
            // config is touched, and nothing is recomputed.
            var chosen = SafetyDialog != null
                ? SafetyDialog(safetySelections)
                : ShowSafetyDialog(elements, levels, postCount, composed);
            if (chosen == null)
            {
                return;
            }

            // ACCEPTED: apply the authorized safety AND the rear stop in ONE operation. Authorize deep-copies every
            // selection, restricts it to the low end and refuses GUIA, PARRILLA and TOPE, so the stop can never travel
            // as ordinary safety and the stored selections stay independent of the dialog's objects.
            var authorized = new PushBackSafetyAuthority(catalog).Authorize(chosen);
            using (session.Recompute.Defer())
            {
                safetySelections.Clear();
                foreach (var selection in authorized)
                {
                    safetySelections.Add(selection);
                }

                // I-42 (ronda 7D): cada seccion de defensa funde SU cara sobre los registros por poste. A/Pn y
                // B/Pn comparten la linea y son dos intenciones distintas, asi que la fusion escribe un extremo y
                // deja el otro exactamente como estaba.
                ApplyDefenseSections(defenseSectionA, defenseSectionB, defenseCarrierId);

                // I-42 (S1E): y cada seccion de botas escribe la configuracion de SU lado, entera. Editar A no
                // puede tocar B ni al reves, que es lo que las dos secciones vienen a garantizar.
                ApplyBootSections(bootSectionA, bootSectionB);

                // Cada seccion aplica a SU lado: editar el tope de A no toca el de B, que es el contrato de
                // StopA/StopB que las rondas anteriores cerraron.
                if (composite.SideBPresent)
                {
                    composite.Of(PushBackSide.A).LoadRearTopeConfig(topeSection.Config);
                    composite.Of(PushBackSide.B).LoadRearTopeConfig(topeSectionB.Config);
                }
                else
                {
                    state.LoadRearTopeConfig(topeSection.Config);
                }

                RequestRecompute();
            }
        }

        /// <summary>I-42 (S1E) — la seccion de botas del lado A del ultimo Seguridad abierto (seam de prueba).</summary>
        internal PushBackBootSection BootSectionForTest { get; private set; }

        /// <summary>La del lado B, o null en un rack de un solo sentido (seam de prueba).</summary>
        internal PushBackBootSection BootSectionBForTest { get; private set; }

        /// <summary>
        /// La seccion de botas de un lado, sembrada con la configuracion que ESE lado tiene ahora. Un rack de un
        /// solo sentido usa la configuracion de siempre (la del lado A), sin etiqueta.
        /// </summary>
        private PushBackBootSection BuildBootSection(PushBackSide side, string sideLabel, int postCount)
        {
            // Se siembra con la seleccion RESUELTA cuando la hay: es la que trae el automatico del sistema, y por
            // tanto la que sabe que hace hoy este lado. La del diseño solo guarda lo que alguien eligio.
            var selection = BootSelection(LastComputation?.System?.Structure?.SafetySelections)
                            ?? BootSelection(safetySelections);
            var config = selection == null
                ? null
                : (side == PushBackSide.B ? selection.BotaB : selection.Bota);
            return new PushBackBootSection(
                side, sideLabel, config, postCount, OpenBootPerPostDialog,
                side == PushBackSide.A ? selection?.PostSides : null);
        }

        /// <summary>La seleccion de la familia BOTA dentro de <paramref name="selections"/>, o null.</summary>
        private SelectiveSafetySelection BootSelection(IEnumerable<SelectiveSafetySelection> selections)
            => RackCad.Application.Systems.Selective.SelectiveSafetyFamilies.SelectedOfType(
                selections, catalog?.SafetyElements, SelectiveSafetyDefaults.BotaType);

        /// <summary>Seam de prueba: sustituye SOLO el ShowDialog de la rejilla por poste de botas.</summary>
        internal Func<SafetyPerPostWindow, bool?> BootPerPostWindowDialog;

        /// <summary>
        /// La rejilla por poste real. Es la MISMA superficie por poste que ya existia —cinco opciones, con «(por
        /// defecto)» como herencia—, abierta para el lado que la seccion declara. Los ordinales de la colocacion y
        /// del lado historico coinciden, asi que la traduccion de ida y vuelta es exacta.
        /// </summary>
        private IReadOnlyList<BootPostPlacement> OpenBootPerPostDialog(PushBackBootSection section)
        {
            var current = section.Posts
                .Select(post => new SafetyPostSide
                {
                    PostIndex = post.PostIndex,
                    Side = BootPlacements.To(post.Placement),
                })
                .ToList();
            var dialog = new SafetyPerPostWindow(
                PushBackBootSection.Heading(section.SideLabel),
                section.PostCount,
                BootPlacements.To(section.Placement),
                current,
                PushBackBootSection.ModeLabels) { Owner = this };
            var accepted = BootPerPostWindowDialog != null ? BootPerPostWindowDialog(dialog) : dialog.ShowDialog();
            if (accepted != true)
            {
                return null;
            }

            return dialog.Result
                .Select(post => new BootPostPlacement
                {
                    PostIndex = post.PostIndex,
                    Placement = BootPlacements.From(post.Side),
                })
                .ToList();
        }

        /// <summary>
        /// Escribe lo que decidio cada seccion sobre la seleccion de botas. Cada lado sustituye SU configuracion
        /// entera —general y postes—, y no toca la del otro. Un rack de un solo sentido escribe solo la del lado A
        /// y deja la de B vacia, que es lo que significa «B no pide nada».
        /// </summary>
        private void ApplyBootSections(PushBackBootSection sideA, PushBackBootSection sideB)
        {
            var selection = BootSelection(safetySelections);
            if (selection == null)
            {
                return;   // el usuario no eligio tipo de bota: no hay nada que configurar
            }

            if (sideA != null)
            {
                selection.Bota = sideA.ToConfig();
            }

            selection.BotaB = sideB != null ? sideB.ToConfig() : new SelectiveBotaConfig();

            // Desde aqui el documento declara su intencion POR LADO: una general vacia significa «este lado hereda
            // su automatico», no «este lado no pide nada», que es lo que significaba antes de S1E.
            selection.BootSidesDeclared = true;
        }

        /// <summary>I-42 (ronda 7D) — la seccion de defensa del lado A del ultimo Seguridad abierto (seam de prueba).</summary>
        internal PushBackDefenseSection DefenseSectionForTest { get; private set; }

        /// <summary>La del lado B, o null en un rack de un solo sentido (seam de prueba).</summary>
        internal PushBackDefenseSection DefenseSectionBForTest { get; private set; }

        /// <summary>
        /// La seccion de defensa de un lado, con la aplicabilidad REAL de ese lado y los registros por poste que el
        /// rack tiene ahora. La aplicabilidad la decide la fisica (<see cref="PushBackDefenseSides.FacesOf"/>), no
        /// la seccion: un frente en blanco quita la cara de ataque de SU lado y no toca la del otro.
        /// </summary>
        private PushBackDefenseSection BuildDefenseSection(PushBackSide side, string sideLabel, int postCount)
            => new PushBackDefenseSection(
                side,
                sideLabel,
                DefensePostsNow(),
                postCount,
                PushBackDefenseSides.FacesOf(LastComputation?.System?.Structure, postCount, side),
                OpenDefenseDialog,
                DefenseVariants(),
                DefensePieceIdOf(side));

        /// <summary>Las variantes de DEFENSA que el catalogo declara. Hoy una; el contrato no supone que siga siendo asi.</summary>
        private IReadOnlyList<SafetyElementCatalogEntry> DefenseVariants()
            => RackCad.Application.Systems.Selective.SelectiveSafetyFamilies.VariantsOfType(
                catalog?.SafetyElements, SelectiveSafetyDefaults.DefensaType);

        /// <summary>
        /// El tipo de defensa de un lado: el que ese lado guarda, o —si nunca se eligio— la pieza que la seleccion
        /// de seguridad del rack ya traia. Un documento anterior a esta ronda abre asi en la pieza que dibujaba, y
        /// no en «Ninguno»: la ausencia de eleccion no es una eleccion.
        /// </summary>
        private string DefensePieceIdOf(PushBackSide side)
        {
            var stored = composite.Of(side).DefensePieceId;
            return string.IsNullOrWhiteSpace(stored) ? DefenseSelection(safetySelections)?.ElementId : stored;
        }

        /// <summary>Los registros POR POSTE que la familia DEFENSA tiene ahora, o una lista vacia si no hay ninguna.</summary>
        private IReadOnlyList<SafetyPostDefense> DefensePostsNow()
            => DefenseSelection(safetySelections)?.DefensaPosts?.ToList() ?? new List<SafetyPostDefense>();

        /// <summary>La seleccion de la familia DEFENSA dentro de <paramref name="selections"/>, o null.</summary>
        private SelectiveSafetySelection DefenseSelection(IEnumerable<SelectiveSafetySelection> selections)
            => RackCad.Application.Systems.Selective.SelectiveSafetyFamilies.SelectedOfType(
                selections, catalog?.SafetyElements, SelectiveSafetyDefaults.DefensaType);

        /// <summary>Abre la rejilla POR POSTE de la cara que declara <paramref name="section"/>; NULL si se cancela.</summary>
        private IReadOnlyList<SafetyPostDefense> OpenDefenseDialog(PushBackDefenseSection section)
            => DefenseDialog != null ? DefenseDialog(section) : ShowDefenseDialog(section);

        /// <summary>Muestra la rejilla real por poste, con la CARA declarada explicitamente.</summary>
        private IReadOnlyList<SafetyPostDefense> ShowDefenseDialog(PushBackDefenseSection section)
        {
            var element = DefenseSelection(safetySelections);
            var label = catalog?.SafetyElements?.FirstOrDefault(entry =>
                string.Equals(entry?.Id, element?.ElementId, StringComparison.OrdinalIgnoreCase))?.Label;
            var dialog = new SafetyDefensaGridWindow(
                label ?? PushBackDefenseSection.HeadingText,
                section.PostCount,
                section.Posts,
                lowEndOnly: true,
                autoPerEnd: true,
                face: section.Face())
            {
                Owner = this
            };
            return dialog.ShowDialog() == true ? dialog.Result : null;
        }

        /// <summary>
        /// Funde lo que cada seccion decidio sobre los registros por poste de la familia DEFENSA. Si la familia no
        /// esta seleccionada no hay donde escribir, y las decisiones se descartan con ella.
        /// </summary>
        private void ApplyDefenseSections(
            PushBackDefenseSection sideA, PushBackDefenseSection sideB, string carrierId)
        {
            // I-42 (ronda 7E): el TIPO de cada lado se guarda en SU estado. Es un eje distinto de la intencion por
            // poste, que sigue en la seleccion de seguridad: cambiar el tipo —incluso a «Ninguno»— no la destruye,
            // asi que volver a una pieza recupera exactamente los postes que habia.
            if (sideA != null)
            {
                composite.Of(PushBackSide.A).DefensePieceId = sideA.PieceId;
            }

            if (sideB != null)
            {
                composite.Of(PushBackSide.B).DefensePieceId = sideB.PieceId;
            }

            var selection = EnsureDefenseSelection(sideA, sideB, carrierId);
            if (selection == null)
            {
                return;
            }

            // Un rack de un solo sentido tiene UNA seccion que decide los dos extremos: su lista sustituye. Un
            // compuesto tiene dos, y cada una funde SOLO su cara sobre la del otro lado.
            if (sideA != null && sideA.OwnsBothEnds && sideB == null)
            {
                Write(selection, PushBackDefenseSides.Copy(sideA.Posts));
                return;
            }

            var merged = PushBackDefenseSides.Copy(selection.DefensaPosts);
            if (sideA != null)
            {
                merged = PushBackDefenseSides.Merge(merged, sideA.Posts, PushBackSide.A);
            }

            if (sideB != null)
            {
                merged = PushBackDefenseSides.Merge(merged, sideB.Posts, PushBackSide.B);
            }

            // Reconciliacion: una linea que el rack ya no tiene no deja intencion fantasma. Las que conservan su
            // identidad conservan su intencion, de cada lado; una nueva nace con el automatico.
            var lines = sideA?.PostCount ?? sideB?.PostCount ?? 0;
            merged.RemoveAll(record => record.PostIndex < 0 || record.PostIndex >= lines);
            Write(selection, merged);
        }

        /// <summary>
        /// La seleccion de la familia DEFENSA, creandola si hace falta. Con la fila general retirada, las secciones
        /// son quienes la traen a existencia: mientras algun lado tenga una pieza, la familia existe; si los dos
        /// dicen «Ninguno», no hay familia y no hay ni bloque ni linea de BOM —«Ninguno» nunca es una pieza.
        /// </summary>
        private SelectiveSafetySelection EnsureDefenseSelection(
            PushBackDefenseSection sideA, PushBackDefenseSection sideB, string carrierId)
        {
            var existing = DefenseSelection(safetySelections);
            var chosen = new[] { sideA, sideB }
                .Where(section => section != null && !section.IsNone)
                .Select(section => section.PieceId)
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

            if (string.IsNullOrWhiteSpace(chosen))
            {
                // Los DOS lados en «Ninguno». La familia se conserva SOLO como portadora de la intencion POR POSTE,
                // que es un eje distinto del tipo y no se destruye al apagarlo: sus dos caras resuelven a «ninguna
                // pieza», asi que no se dibuja ni se cuenta nada, y volver a elegir una devuelve la rejilla entera.
                // Sin postes decididos no hay nada que conservar y no se inventa ninguna familia.
                var hasIntent = (sideA?.Posts?.Count ?? 0) > 0 || (sideB?.Posts?.Count ?? 0) > 0;
                if (!hasIntent || string.IsNullOrWhiteSpace(carrierId))
                {
                    return null;
                }

                chosen = carrierId;
            }

            if (existing == null)
            {
                existing = new SelectiveSafetySelection { Quantity = 1, Side = SafetySide.None };
                safetySelections.Add(existing);
            }

            existing.ElementId = chosen;
            PushBackSafetyAuthority.RestrictToLowEnd(existing);
            return existing;
        }

        private static void Write(SelectiveSafetySelection selection, IEnumerable<SafetyPostDefense> records)
        {
            selection.DefensaPosts.Clear();
            foreach (var record in records)
            {
                selection.DefensaPosts.Add(record);
            }
        }

        /// <summary>The tope section of the last opened Seguridad dialog (test seam). In a composite, side A's.</summary>
        internal PushBackRearTopeSection RearTopeSectionForTest { get; private set; }

        /// <summary>I-42 (ronda 7B) — side B's tope section, or null in a single-sided rack (test seam).</summary>
        internal PushBackRearTopeSection RearTopeSectionBForTest { get; private set; }

        /// <summary>Opens the shared tope grid for <paramref name="config"/>; NULL when cancelled.</summary>
        private SafetyTopeGridWindow.TopeResult OpenRearTopeDialog(PushBackSide side, PushBackRearTopeConfig config)
        {
            var levels = RearTopeLevels(side);
            return RearTopeDialog != null ? RearTopeDialog(config, levels) : ShowRearTopeDialog(config, levels);
        }

        /// <summary>
        /// Los niveles por frente con los que se abre la rejilla del tope de <paramref name="side"/> — los de ESE
        /// lado, incluidos sus ceros. Un frente en blanco en A no tiene por que estarlo en B, y al reves.
        /// </summary>
        internal IReadOnlyList<int> RearTopeLevels(PushBackSide side)
            => PushBackRearTopeDialogAdapter.LevelsPerFrente(
                composite.Of(side).Structure.EffectiveLevelCounts(), allowBlankFronts: true);

        /// <summary>Shows the real shared safety dialog; NULL when the user cancelled.</summary>
        private IReadOnlyList<SelectiveSafetySelection> ShowSafetyDialog(
            IReadOnlyList<SafetyElementCatalogEntry> elements, IReadOnlyList<int> levels, int postCount,
            System.Windows.UIElement extraSection)
        {
            var dialog = new SelectiveSafetyWindow(
                elements, safetySelections, postCount,
                levelsPerFrente: levels, fondoCount: 1, parrillaPlan: null, catalog: catalog, resolvedSystem: null,
                fallbackLevelsArePerPost: true,
                introduction: "Push Back admite botas, protectores laterales, desviadores y defensa de montacargas en el extremo de entrada/salida (el extremo bajo). Las botas, la defensa y los topes se configuran en las secciones de arriba, cada lado por separado. El lado posterior viene apagado y no usa guías.",
                includeDefensa: false, includeGuia: false, useDynamicSafetyDefaults: true,
                extraSection: extraSection,
                desviadorLevelsPerPost: DesviadorLevelsPerPost(),
                allowBlankFrontColumns: true,
                // PB-003: Push Back carga por un solo extremo, así que el selector de cara se OCULTA. Ahora es una
                // decisión explícita e independiente de la lista por poste, no un efecto secundario de entregarla.
                showDesviadorSide: false,
                // PB-008/009/010: the two ends of the defence are named for what Push Back really has, the rear one is
                // off by default, and each end can follow the automatic 12"/36" that recomputes with the front count.
                defensaLowEndOnly: true,
                // I-42 (S1E): la UBICACION de la bota se decide en las secciones por lado de arriba; la fila
                // conserva solo el TIPO.
                bootModeInSections: true)
            {
                Owner = this
            };
            var accepted = SafetyWindowDialog != null ? SafetyWindowDialog(dialog) : dialog.ShowDialog();
            return accepted == true ? dialog.Result : null;
        }

        /// <summary>
        /// I-42 (ronda 7C) — seam de prueba de «Elementos de seguridad». Sustituye UNICAMENTE el <c>ShowDialog</c>:
        /// la ventana que recibe es la REAL, construida con los mismos argumentos que ve el usuario. Con el, una
        /// prueba recorre la ruta entera —esta ventana, la rejilla por poste, el commit, el resolve y el dibujo—
        /// en vez de sustituir la ventana por un delegado, que es justo lo que ocultaba el defecto.
        /// </summary>
        internal Func<SelectiveSafetyWindow, bool?> SafetyWindowDialog;

        /// <summary>Shows the real shared tope-grid dialog; NULL when the user cancelled.</summary>
        private SafetyTopeGridWindow.TopeResult ShowRearTopeDialog(PushBackRearTopeConfig config, IReadOnlyList<int> levels)
        {
            var dialog = new SafetyTopeGridWindow(
                PushBackRearTopeDialogAdapter.Label,
                levels,
                shared: false,
                side: SafetySide.Left,
                saque: PushBackRearTopeDialogAdapter.Saque(config),
                frontal: false,
                offCells: PushBackRearTopeDialogAdapter.OffCells(config),
                fondoCount: 1,
                fondo: -1,
                // PB-006: Push Back has a single depth line — there is no central-vs-per-fondo stop and no side to
                // pick, so neither control is offered. The adapter never read them either.
                showSharedAndSide: false)
            {
                Owner = this
            };
            return dialog.ShowDialog() == true ? dialog.Result : null;
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
                // I-42: un corte frontal es de UN lado, asi que su SECCION lleva tambien el lado activo. Un rack de
                // un solo sentido codifica 0 y 1, exactamente lo que escribieron todas las versiones anteriores.
                case 1: return (RackEmbedDocument.ViewFrontal, PushBackSystemFrontalBuilder.EncodeSection(
                    PushBackFrontalEnd.EntradaSalida, frontalSide));
                case 2: return (RackEmbedDocument.ViewFrontal, PushBackSystemFrontalBuilder.EncodeSection(
                    PushBackFrontalEnd.Posterior, frontalSide));
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
            DrawSharedPreview(plan, view);
        }

        /// <summary>
        /// I-18b decision 6: the Push Back preview is painted by the SHARED infrastructure
        /// (<see cref="PushBackPreviewRenderer"/> over <see cref="EditorPreviewSurface"/> and
        /// <see cref="EditorPreviewParts"/>) — the same parts the dynamic editor draws with. There is no second painter
        /// and no simplified renderer left in the drawing path.
        /// </summary>
        private void DrawSharedPreview(HeaderRunPlan plan, string view)
        {
            previewSurface ??= new EditorPreviewSurface(PreviewCanvas);
            PushBackPreviewRenderer.Draw(
                previewSurface,
                plan,
                catalog,
                view,
                LowBeamDepth(),
                DynamicRackDefaults.InOutBeamCatalogId,
                string.IsNullOrWhiteSpace(lastComputation?.System?.HighEndBeamCatalogId)
                    ? PushBackDefaults.HighEndBeamCatalogId
                    : lastComputation.System.HighEndBeamCatalogId);
        }

        private EditorPreviewSurface previewSurface;

        /// <summary>The resolved IN/OUT beam depth (the low beam has no PERALTE of its own), as the dynamic preview uses.</summary>
        private double LowBeamDepth()
            => lastComputation?.System?.Structure?.InOutBeamDepth > 0.0
                ? lastComputation.System.Structure.InOutBeamDepth
                : DynamicRackDefaults.DefaultBeamDepth;

        /// <summary>The semantic preview of <paramref name="plan"/>: interpreted primitives only — the plan is the
        /// geometry authority. The low lateral IN/OUT beam (no PERALTE of its own) falls back to the resolved system's
        /// beam depth, like the dynamic editor's preview.</summary>
        private PushBackPreviewModel BuildPreviewModel(HeaderRunPlan plan, string view)
            => PushBackPreviewModel.Build(
                plan,
                catalog,
                view,
                lastComputation?.System?.Structure?.InOutBeamDepth > 0.0
                    ? lastComputation.System.Structure.InOutBeamDepth
                    : DynamicRackDefaults.DefaultBeamDepth);

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderPreview();

        private HeaderRunPlan PlanFor(string view, int section)
        {
            if (lastComputation == null) return null;
            if (string.Equals(view, RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase)) return lastComputation.PlantaPlan;
            if (string.Equals(view, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase))
            {
                // I-42: la seccion frontal lleva EXTREMO y LADO, asi que hay que DECODIFICARLA. Compararla contra
                // (int)Posterior solo acertaba en el lado A: la seccion 3 —posterior de B— caia en el corte de
                // entrada/salida y el panel mostraba el pasillo de carga donde el usuario pidio el fondo.
                return PushBackSystemFrontalBuilder.DecodeSection(section).End == PushBackFrontalEnd.Posterior
                    ? lastComputation.FrontalPosterior
                    : lastComputation.FrontalEntradaSalida;
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
                var frontal = PushBackSystemFrontalBuilder.DecodeSection(section);
                var label = frontal.End == PushBackFrontalEnd.Posterior ? "Frontal posterior" : "Frontal entrada/salida";
                return section >= 2 ? label + " (lado " + (frontal.Side == PushBackSide.B ? "B" : "A") + ")" : label;
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

        /// <summary>
        /// I-42 — el lado que el CORTE FRONTAL muestra. Es una eleccion del boton que se pulsa, no el «lado activo»
        /// de la edicion: el usuario tiene que poder pedir el corte de B mientras edita A, y el dibujo insertado no
        /// puede depender de un modo que no se ve en la barra de vistas.
        /// </summary>
        private PushBackSide frontalSide = PushBackSide.A;

        /// <summary>El lado del ultimo corte frontal pedido (seam de prueba).</summary>
        internal PushBackSide FrontalSideForTest => frontalSide;

        private void InsertFrontalEntrada_Click(object sender, RoutedEventArgs e)
            => InsertViewAt(1);

        private void InsertFrontalPosterior_Click(object sender, RoutedEventArgs e)
            => InsertViewAt(2);

        /// <summary>
        /// El lado de los CORTES FRONTALES lo declara su propio selector, no el lado que se este editando: el usuario
        /// tiene que poder pedir el corte de B mientras configura A, y el dibujo insertado no puede depender de un
        /// modo que no se ve en la barra de vistas.
        /// </summary>
        private void FrontalSide_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            frontalSide = FrontalSideBox.SelectedIndex == 1 ? PushBackSide.B : PushBackSide.A;
            // Los cortes frontales YA construidos son los del lado anterior: repintar sin reconstruir dejaba el
            // panel mostrando el otro pasillo hasta que cualquier otra edicion forzara un recalculo.
            RequestRecompute();
            RenderPreview();
        }

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

            // I-40 (ronda 3) — LA PERDIDA REAL. Dibujar recalculaba SIN confirmar la sesion de modulos, asi que
            // toda edicion escenificada —una cabecera personalizada, una copia a otras cabeceras— se descartaba en
            // silencio y el rack se dibujaba Y SE EMBEBIA con la cabecera estandar. Medido: la sesion tenia 187/41.5
            // y el diseno dibujado salia con 192/44 y procedencia calculada, de modo que el siguiente RACKEDITAR ya
            // no podia recuperar nada.
            //
            // Pulsar Actualizar o Insertar es aplicar lo que el panel muestra: se confirma primero. La transaccion
            // de I-35 no se debilita —«Cancelar» sigue siendo el UNICO descarte—, deja de haber un camino que
            // descarta sin decirlo.
            if (state.ModuleSession.HasPendingChanges)
            {
                state.CommitModuleEdits();
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

        /// <summary>
        /// El unico punto por el que pasan los CUATRO caminos de cierre (ADR-0029 D7): el boton Cerrar, Escape —que
        /// llega aqui desde I-39B, porque ese boton pasa a ser <c>IsCancel</c>—, el boton de sistema y <c>Alt+F4</c>.
        ///
        /// <para>Hasta I-39B esta ventana era la unica de las seis SIN <c>IsCancel</c>, de modo que Escape no la
        /// cerraba. Anadirselo sin politica habria convertido esa tecla en un descarte instantaneo justo en la unica
        /// ventana con un ambito transaccional declarado, y mientras la propia ventana muestra «Cambios pendientes:
        /// confirma o cancela». Por eso la politica va primero y el <c>IsCancel</c> despues.</para>
        ///
        /// <para>Insertar y Actualizar no son un descarte: <c>RequestDraw</c> marca la peticion en la sesion antes
        /// de cerrar, y ese cierre no pregunta nada.</para>
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!InsertRequested && !EditorClosePolicy.MayClose(PendingWork()))
            {
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }

        /// <summary>
        /// El ambito transaccional que esta ventana DECLARA: la edicion de modulo escenificada y todavia sin
        /// confirmar, que es la que la ventana ya sabe confirmar y cancelar por si misma. Incluye la cabecera
        /// personalizada, porque configurarla escenifica sobre esa misma sesion.
        ///
        /// <para>Lo demas que el usuario haya tocado —matriz, seguridad, topes— no es un ambito declarado: se aplica
        /// al estado en el acto y se recomputa, y su perdida al cerrar es la del ciclo de vida normal de un editor,
        /// identica en las seis ventanas. Convertir eso en dirty exigiria comparar la sesion entera contra su
        /// apertura, que es el dirty global artificial que ADR-0029 D8 no quiere: el contrato dice que dirty
        /// pertenece a un AMBITO y que «no aplicable» es un valor legitimo.</para>
        /// </summary>
        internal EditorPendingWork PendingWork() => EditorPendingWork.When(
            state.ModuleSession != null && state.ModuleSession.HasPendingChanges,
            "Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?");

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

            // The four per-view insert actions follow the SAME gate and explain themselves when disabled (round-3 §6);
            // BOM/save get the shared invalid-fields reason, and Restaurar states why it has nothing to restore yet.
            foreach (var viewButton in new[] { InsertLateralButton, InsertFrontalEntradaButton, InsertFrontalPosteriorButton, InsertPlantaButton })
            {
                viewButton.IsEnabled = InsertButton.IsEnabled;
                viewButton.ToolTip = InsertButton.ToolTip;
            }

            if (!currentInputsAreValid)
            {
                BomButton.ToolTip = "Corrige los campos numéricos marcados.";
                SaveLibraryButton.ToolTip = BomButton.ToolTip;
            }
            else
            {
                BomButton.ToolTip = "Lista de materiales del sistema actual.";
                SaveLibraryButton.ToolTip = "Guarda este diseño Push Back en la biblioteca.";
            }

            RestoreButton.IsEnabled = lastComputation != null;
            RestoreButton.ToolTip = lastComputation != null
                ? "Vuelve todos los valores al último sistema válido."
                : "Aún no hay un sistema válido que restaurar.";
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
