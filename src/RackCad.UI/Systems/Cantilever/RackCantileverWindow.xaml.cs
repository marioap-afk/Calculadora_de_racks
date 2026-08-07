using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.UI.Shell;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Controls;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Cantilever.Components;

namespace RackCad.UI.Systems.Cantilever
{
    /// <summary>
    /// The Cantilever LINE editor (I-37D, reestructurado en la ronda 2).
    ///
    /// It edits the AGGREGATE and only the aggregate: the line, the topology its stations share and the bracing
    /// distribution. Every component — column–base, arm, separator, brace — is edited in its own window, with its
    /// own parameters, preview, diagnostics and recipe; here they appear as a compact summary and a button. That
    /// is motivos 1, 2 y 6 del rechazo de la ronda 1: the window was saturated, it mixed line properties with the
    /// internals of components, and its architecture did not reflect the real configuration flow.
    ///
    /// It still computes nothing: it reads controls into a <see cref="CantileverLineDesign"/> and hands it to
    /// <see cref="CantileverLineEditorAssembler"/>.
    /// </summary>
    public partial class RackCantileverWindow : Window
    {
        /// <summary>The ONE display format of every decimal box, so writing and reading a field agree.</summary>
        private const string NumberFormat = "0.###";

        private static readonly string[] FaceModes = { "Sencilla", "Doble" };
        private static readonly string[] Sides = { "Lado +Y", "Lado −Y" };
        private static readonly string[] HeightModes = { "Automática", "Manual" };
        private static readonly string[] PanelModes = { "Automático", "Manual" };
        private static readonly string[] PanelLayoutModes = { "Automática", "Avanzada" };
        private static readonly string[] Scopes =
            { "Celda", "Estación", "Nivel (toda la línea)", "Lado (toda la línea)", "Toda la línea" };

        private readonly bool canInsertInAutoCad;
        private readonly RackEditorSession<CantileverLineDesign, CantileverLineAssembly> session;
        private readonly CantileverLineEditorAssembler assembler;
        private readonly string catalogueError;

        private readonly Dictionary<CantileverLineCell, Button> cellButtons = new Dictionary<CantileverLineCell, Button>();

        private CantileverLineDesign design = new CantileverLineDesign();
        private CantileverLineEditorComputation lastComputation;
        private CantileverLineCell? selectedCell;
        private RackProject sourceProject;
        private bool isEditingExisting;
        private bool suppressSync;

        /// <summary>
        /// El estado del editor avanzado de paneles. La ventana COORDINA; quien decide es Application.
        /// </summary>
        /// <summary>
        /// Cómo se pregunta antes de descartar la lista manual.
        ///
        /// Es una COSTURA y no un <c>MessageBox</c> escrito en el sitio, por dos razones. La primera es que un
        /// diálogo modal dentro de un manejador cuelga cualquier prueba que toque este camino, y este camino
        /// —volver a automático— es justo el que hay que probar. La segunda es que la decisión de PREGUNTAR ya
        /// la tomó Application al marcar <c>ReplacesManualWork</c>; lo único que queda aquí es CÓMO.
        /// </summary>
        internal Func<string, bool> ConfirmDiscardingManualPanels { get; set; } = reason =>
            MessageBox.Show(
                reason + Environment.NewLine + Environment.NewLine + "¿Volver a la secuencia automática?",
                "Secuencia de paneles",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;

        private CantileverPanelLayoutEditorState panelEditor =
            new CantileverPanelLayoutEditorState(
                CantileverPanelLayoutMode.Automatic, Array.Empty<CantileverPanelSegmentDesign>());

        private readonly System.Collections.ObjectModel.ObservableCollection<CantileverPanelSegmentRow>
            panelRows = new System.Collections.ObjectModel.ObservableCollection<CantileverPanelSegmentRow>();

        private bool currentInputsAreValid;
        private int recomputeCount;

        public RackCantileverWindow()
            : this(false)
        {
        }

        public RackCantileverWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();

            session = new RackEditorSession<CantileverLineDesign, CantileverLineAssembly>(
                recompute: Recompute, newIdFactory: newIdFactory);

            // FAIL CLOSED, like RACKSECCION: an invalid section catalogue means the dimensions are not
            // trustworthy, so the window opens read-only with the reason on screen rather than drawing from data
            // that failed validation.
            try
            {
                assembler = new CantileverLineEditorAssembler(
                    new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load());
            }
            catch (StructuralSectionCatalogException ex)
            {
                catalogueError = ex.Message;
            }
            catch (System.IO.IOException ex)
            {
                catalogueError = ex.Message;
            }

            FaceModeBox.ItemsSource = FaceModes;
            SingleSideBox.ItemsSource = Sides;
            HeightModeBox.ItemsSource = HeightModes;
            PanelModeBox.ItemsSource = PanelModes;
            PanelLayoutModeBox.ItemsSource = PanelLayoutModes;
            PanelSegmentGrid.ItemsSource = panelRows;
            ScopeBox.ItemsSource = Scopes;
            ScopeBox.SelectedIndex = 0;

            LoadNew();
        }

        // ---- Test seams (internal) --------------------------------------------------------------------------

        internal RackEditorSession<CantileverLineDesign, CantileverLineAssembly> Session => session;

        internal CantileverLineEditorAssembler Assembler => assembler;

        /// <summary>The LIVE design the controls write into. Tests read it to assert what an edit produced.</summary>
        internal CantileverLineDesign Design => design;

        internal CantileverLineEditorComputation LastComputation => lastComputation;

        internal bool CurrentInputsAreValid => currentInputsAreValid;

        /// <summary>How many times the editor rebuilt. One matrix operation must add exactly ONE.</summary>
        internal int RecomputeCount => recomputeCount;

        internal CantileverLineCell? SelectedCell => selectedCell;

        internal CantileverLineArmMatrix Matrix => new CantileverLineArmMatrix(design);

        /// <summary>The plan the preview is currently drawing, so a test asserts CONTENT and never pixels.</summary>
        internal CantileverViewPlan CurrentPreviewPlan =>
            lastComputation != null && lastComputation.IsValid
                ? assembler.View(
                    lastComputation.Line, SelectedViewKind(), LateralStationIndex(), design.PlantaVisibility)
                : null;

        // ---- The editor→host contract ------------------------------------------------------------------------

        public bool InsertRequested => session.InsertRequested;

        public bool UpdateOnly => session.UpdateOnly;

        public string RackId => session.Identity.Id;

        public string RackName => session.Identity.Name;

        public string InsertView => session.InsertView;

        public int InsertSection => session.InsertSection;

        public RackInsertionRequest InsertionRequest => session.InsertionRequest;

        public CantileverLineAssembly LineToInsert => (session.InsertionRequest as CantileverInsertionRequest)?.Line;

        public CantileverLineDesign DesignToInsert => (session.InsertionRequest as CantileverInsertionRequest)?.Design;

        /// <summary>
        /// The stand-alone component insertion a configurator asked for, or null.
        ///
        /// It travels through the SAME seam as a line insertion — the host draws it after every modal has closed,
        /// because the point prompt needs the editor free. It does not touch the line being edited: whatever the
        /// configurator accepted was already applied to the design before the window closed.
        /// </summary>
        public CantileverComponentInsertionRequest ComponentInsertion { get; private set; }

        /// <summary>Bubbles a configurator's stand-alone insertion and closes, so the host can prompt for a point.</summary>
        private void BubbleComponentInsertion(CantileverComponentInsertionRequest request)
        {
            if (request == null)
            {
                return;
            }

            ComponentInsertion = request;
            Close();
        }

        // ---- Loading -----------------------------------------------------------------------------------------

        /// <summary>A brand-new line: the domain defaults, no identity, insert only.</summary>
        public void LoadNew()
        {
            // No section id is invented here. The sections and the mandatory punch margins have no approved
            // default, so the line opens BLOCKED and its component cards say what they are waiting for.
            design = new CantileverLineDesign();
            sourceProject = null;
            isEditingExisting = false;
            selectedCell = null;
            session.Identity.Adopt(null, null);
            LoadFromDesign(string.Empty);
        }

        /// <summary>A line opened from the library, edited as a NEW insert: a fresh GUID is minted on insert.</summary>
        public void LoadDesignForNew(CantileverLineDesign line, string rackName, RackProject sourceProject = null)
        {
            if (line == null)
            {
                return;
            }

            design = line.DeepCopy();
            this.sourceProject = sourceProject;
            isEditingExisting = false;
            selectedCell = null;
            session.Identity.Adopt(null, rackName);
            LoadFromDesign(rackName);
        }

        /// <summary>A line opened from the DWG (RACKEDITAR): keeps its GUID + name and enables "Actualizar".</summary>
        public void LoadExisting(CantileverLineDesign line, string rackId, string rackName, RackProject sourceProject = null)
        {
            if (line == null)
            {
                return;
            }

            design = line.DeepCopy();
            this.sourceProject = sourceProject;
            isEditingExisting = true;
            selectedCell = null;
            session.Identity.Adopt(rackId, rackName);
            LoadFromDesign(rackName);
        }

        private void LoadFromDesign(string rackName)
        {
            suppressSync = true;

            try
            {
                NameBox.Text = rackName ?? string.Empty;
                StationCountBox.SetNumber(design.StationCount, "0");
                SpacingBox.SetNumber(design.ColumnCentreSpacing, NumberFormat);

                var topology = design.StationTopology ?? new CantileverLineStationTopologyDesign();
                FaceModeBox.SelectedIndex = topology.FaceMode == CantileverStationFaceMode.Double ? 1 : 0;
                SingleSideBox.SelectedIndex = topology.SingleSide == CantileverArmSide.NegativeY ? 1 : 0;
                LevelCountBox.SetNumber(topology.LevelCount, "0");
                FirstPunchBox.SetNumber(topology.FirstLevelPunchIndex, "0");
                ClearHeightBox.SetNumber(topology.RequestedClearHeight, NumberFormat);
                TopClearFactorBox.SetNumber(topology.TopClearFactor, NumberFormat);
                HeightModeBox.SelectedIndex =
                    (topology.ColumnHeight?.Mode ?? CantileverStationColumnHeightMode.Automatic)
                        == CantileverStationColumnHeightMode.Manual ? 1 : 0;
                ManualHeightBox.SetNumber(topology.ColumnHeight?.ManualHeight, NumberFormat);

                var bracing = design.Bracing ?? new CantileverBracingDesign();
                PanelModeBox.SelectedIndex = bracing.PanelCountMode == CantileverBracedPanelCountMode.Manual ? 1 : 0;
                ManualPanelCountBox.SetNumber(bracing.ManualPanelCount, "0");
                PanelHeightBox.SetNumber(bracing.BracedPanelHeight, NumberFormat);
                CentralSpaceBox.SetNumber(bracing.CentralEmptySpaceHeight, NumberFormat);

                PanelLayoutModeBox.SelectedIndex =
                    bracing.PanelLayoutMode == CantileverPanelLayoutMode.Advanced ? 1 : 0;

                panelEditor = new CantileverPanelLayoutEditorState(
                    bracing.PanelLayoutMode, bracing.AdvancedPanelSegments);

                RefreshPanelRows();

                LateralStationBox.SetNumber(1, "0");

                var planta = design.PlantaVisibility ?? new CantileverPlantaVisibilityDesign();
                PlantaShowArmsCheck.IsChecked = planta.ShowArms;
                PlantaShowBracesCheck.IsChecked = planta.ShowBraces;
            }
            finally
            {
                suppressSync = false;
            }

            Recompute();
        }

        // ---- Reading the controls: ONLY the aggregate ---------------------------------------------------------

        /// <summary>
        /// Writes the LINE controls into the live design. The components' own fields are not here — they are
        /// written by their own windows, which is the whole point of this round.
        /// </summary>
        private bool ReadInputs(out string error)
        {
            error = null;

            var fields = new (NumericField Field, string Label)[]
            {
                (StationCountBox, "Estaciones"),
                (SpacingBox, "Separación entre centros"),
                (LevelCountBox, "Niveles"),
                (FirstPunchBox, "Troquel del primer nivel"),
                (ClearHeightBox, "Claro solicitado"),
                (TopClearFactorBox, "Factor de claro superior"),
                (ManualHeightBox, "Altura manual"),
                (ManualPanelCountBox, "Cantidad manual de paneles"),
                (PanelHeightBox, "Altura de panel"),
                (CentralSpaceBox, "Altura de espacio central")
            };

            var broken = fields.Where(f => f.Field.HasError).ToList();

            if (broken.Count > 0)
            {
                error = broken.Count == 1
                    ? broken[0].Label + ": " + (broken[0].Field.ErrorMessage ?? "valor inválido.")
                    : "Faltan o son inválidos " + broken.Count + " campos: "
                      + string.Join(", ", broken.Select(f => f.Label)) + ".";

                return false;
            }

            design.StationCount = (int)Math.Round(StationCountBox.Value ?? design.StationCount);
            design.ColumnCentreSpacing = Keep(SpacingBox, design.ColumnCentreSpacing);

            var topology = design.StationTopology ??= new CantileverLineStationTopologyDesign();
            topology.FaceMode = FaceModeBox.SelectedIndex == 1
                ? CantileverStationFaceMode.Double
                : CantileverStationFaceMode.Single;
            topology.SingleSide = SingleSideBox.SelectedIndex == 1
                ? CantileverArmSide.NegativeY
                : CantileverArmSide.PositiveY;
            topology.LevelCount = (int)Math.Round(LevelCountBox.Value ?? topology.LevelCount);
            topology.FirstLevelPunchIndex = (int)Math.Round(FirstPunchBox.Value ?? topology.FirstLevelPunchIndex);
            topology.RequestedClearHeight = Keep(ClearHeightBox, topology.RequestedClearHeight);
            topology.TopClearFactor = Keep(TopClearFactorBox, topology.TopClearFactor);

            var height = topology.ColumnHeight ??= new CantileverStationColumnHeightDesign();
            height.Mode = HeightModeBox.SelectedIndex == 1
                ? CantileverStationColumnHeightMode.Manual
                : CantileverStationColumnHeightMode.Automatic;
            height.ManualHeight = ManualHeightBox.Value;

            var bracing = design.Bracing ??= new CantileverBracingDesign();
            bracing.PanelCountMode = PanelModeBox.SelectedIndex == 1
                ? CantileverBracedPanelCountMode.Manual
                : CantileverBracedPanelCountMode.Automatic;
            bracing.ManualPanelCount = ManualPanelCountBox.Value.HasValue
                ? (int?)Math.Round(ManualPanelCountBox.Value.Value)
                : null;
            bracing.BracedPanelHeight = Keep(PanelHeightBox, bracing.BracedPanelHeight);
            bracing.CentralEmptySpaceHeight = Keep(CentralSpaceBox, bracing.CentralEmptySpaceHeight);

            panelEditor.ApplyTo(bracing);

            return true;
        }

        /// <summary>
        /// The number a field carries — or the value it was WRITTEN from, when the two are the same to display
        /// precision. A default like one third does not survive a round trip through a formatted box, and a field
        /// the user did not touch must not change the design.
        /// </summary>
        private static double Keep(NumericField field, double current)
        {
            if (!field.Value.HasValue)
            {
                return current;
            }

            var shown = field.Value.Value.ToString(NumberFormat, CultureInfo.InvariantCulture);
            var stored = current.ToString(NumberFormat, CultureInfo.InvariantCulture);

            return string.Equals(shown, stored, StringComparison.Ordinal) ? current : field.Value.Value;
        }

        // ---- Recompute ---------------------------------------------------------------------------------------

        private void RequestRecompute() => session.Recompute.Request();

        private void Recompute()
        {
            if (suppressSync)
            {
                return;
            }

            // Counted so a test can prove that ONE matrix operation produces ONE rebuild — the alternative,
            // a regeneration per cell, is invisible to the eye and ruinous on a line of twelve stations.
            recomputeCount++;

            if (assembler == null)
            {
                currentInputsAreValid = false;
                SetStatus("No se pudo cargar el catálogo de secciones estructurales, así que no se dibujará nada. "
                          + catalogueError, true);
                UpdateButtons();
                return;
            }

            if (!ReadInputs(out var fieldError))
            {
                currentInputsAreValid = false;
                SetStatus(fieldError, true);
                UpdateButtons();
                return;
            }

            var computation = assembler.Build(design, LateralStationIndex());

            BuildMatrix();
            UpdateGuid();
            UpdateComponentCards();

            if (!computation.IsValid)
            {
                currentInputsAreValid = false;
                lastComputation = null;
                SummaryText.Text = "La línea no se resolvió.";
                SetStatus(computation.Error ?? "La línea no se resolvió.", true);
                RenderPreview();
                UpdateButtons();
                return;
            }

            currentInputsAreValid = true;
            lastComputation = computation;
            session.SetModel(computation.Design, computation.Line);

            SummaryText.Text = Summarize(computation);

            // ADR-0029: la severidad se representa como es, no como el booleano permitía. `Warnings` son los
            // diagnósticos NO bloqueantes —la línea SÍ resolvió—, y hasta I-39B se pintaban con `isError: true`,
            // es decir en el mismo rojo que un fallo real e indistinguibles de él. El dominio ya distinguía
            // Info/Warning/Blocking; lo que faltaba era no perder esa distinción al presentarla.
            var warnings = Warnings(computation);
            UiSupport.SetStatus(
                StatusText,
                warnings ?? "Línea recalculada.",
                warnings != null ? EditorStatusSeverity.Warning : EditorStatusSeverity.Success);

            RenderPreview();
            UpdateButtons();
        }

        private static string Warnings(CantileverLineEditorComputation computation)
        {
            var warnings = computation.Warnings;

            return warnings.Count == 0
                ? null
                : string.Join(" · ", warnings.Select(w => w.Message));
        }

        private string Summarize(CantileverLineEditorComputation computation)
        {
            var line = computation.Line;

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} estaciones · {1} intervalos · altura común {2} in · {3} paneles arriostrados · {4} separadores · {5} tensores · {6} componentes en el BOM",
                line.StationCount,
                line.IntervalCount,
                line.ColumnHeight.ToString("0.##", CultureInfo.InvariantCulture),
                line.BracedPanels.Count,
                line.Separators.Count,
                line.Braces.Count,
                computation.Bom?.Components.Count ?? 0);
        }

        // ---- The component cards -------------------------------------------------------------------------------

        /// <summary>
        /// The four summaries. COMPACT and deterministic: a card says what the component is, not how it is built.
        ///
        /// Long ids stay out of the card and live in its tooltip — showing `AISC-HSS-RECT-HSS4X4X_250` four times
        /// is how the sidebar became unreadable in the first place.
        /// </summary>
        private void UpdateComponentCards()
        {
            var template = design.StationTopology?.ColumnBaseTemplate ?? new CantileverStationColumnBaseTemplateDesign();
            var arm = design.DefaultArmTemplate ?? new CantileverArmTemplateDesign();
            var bracing = design.Bracing ?? new CantileverBracingDesign();

            ColumnBaseSummary.Text = Short(template.ColumnSectionId) == null
                ? "(elige una sección de columna)"
                : Short(template.ColumnSectionId) + " · base " + (Short(template.Base?.SectionId) ?? "(sin elegir)") +
                  " · " + (template.BaseFollowsColumn ? "sigue a la columna" : "base manual");
            ConfigureColumnBaseButton.ToolTip = template.ColumnSectionId;

            ArmSummary.Text = Short(arm.Body?.SectionId) == null
                ? "(elige una sección de brazo)"
                : Short(arm.Body.SectionId) + " · " +
                  arm.Body.CutLength.ToString("0.##", CultureInfo.InvariantCulture) + " in · " +
                  arm.Body.SlopeRisePer12.ToString("0.##", CultureInfo.InvariantCulture) + "/12";
            ConfigureArmButton.ToolTip = arm.Body?.SectionId;

            var separatorPunches = lastComputation?.Line?.Separators.FirstOrDefault()?.Punches.Count ?? 4;
            SeparatorSummary.Text = (Short(bracing.SeparatorSectionId) ?? "(sin elegir)") +
                                    " · " + separatorPunches + " troqueles";
            ConfigureSeparatorButton.ToolTip = bracing.SeparatorSectionId;

            BraceSummary.Text = bracing.BraceKind == CantileverBraceBodyKind.ColdRolledRound
                ? "Cold rolled Ø" + (bracing.ColdRolled?.Diameter ?? 0.0).ToString("0.###", CultureInfo.InvariantCulture) + " in"
                : "Estructural " + (Short(bracing.BraceSectionId) ?? "(sin sección)");
            ConfigureBraceButton.ToolTip = bracing.BraceSectionId;

            BracingSummaryText.Text = lastComputation?.Line == null
                ? string.Empty
                : "Derivado: " + lastComputation.Line.BracedPanels.Count + " paneles · " +
                  lastComputation.Line.Separators.Count + " separadores · " +
                  lastComputation.Line.Braces.Count + " tensores.";
        }

        /// <summary>The designation part of an id, for a card that must stay compact.</summary>
        private static string Short(string sectionId) => CantileverColumnBaseEditorState.Designation(sectionId);

        private void ConfigureColumnBase_Click(object sender, RoutedEventArgs e)
        {
            if (assembler == null)
            {
                return;
            }

            var topology = design.StationTopology ??= new CantileverLineStationTopologyDesign();
            var window = new CantileverColumnBaseWindow(
                topology.ColumnBaseTemplate, assembler.Catalogue, canInsertInAutoCad) { Owner = this };

            window.ShowDialog();

            if (window.Result == null)
            {
                return; // cancelled: nothing was mutated, the window edited a copy
            }

            topology.ColumnBaseTemplate = window.Result;
            Recompute();
            SetStatus("Columna y base actualizadas.", false);
            BubbleComponentInsertion(window.ComponentInsertion);
        }

        private void ConfigureArm_Click(object sender, RoutedEventArgs e) => EditArm(null);

        private void ConfigureSeparator_Click(object sender, RoutedEventArgs e)
        {
            if (assembler == null)
            {
                return;
            }

            var window = new CantileverSeparatorWindow(
                design.Bracing, assembler.Catalogue,
                lastComputation?.Line?.Separators.FirstOrDefault()) { Owner = this };
            window.ShowDialog();

            if (window.Result == null)
            {
                return;
            }

            design.Bracing = window.Result;
            Recompute();
            SetStatus("Separador actualizado.", false);
            BubbleComponentInsertion(window.ComponentInsertion);
        }

        private void ConfigureBrace_Click(object sender, RoutedEventArgs e)
        {
            if (assembler == null)
            {
                return;
            }

            var window = new CantileverBraceWindow(
                design.Bracing, assembler.Catalogue,
                lastComputation?.Line?.Braces.FirstOrDefault()) { Owner = this };
            window.ShowDialog();

            if (window.Result == null)
            {
                return;
            }

            design.Bracing = window.Result;
            Recompute();
            SetStatus("Tensor actualizado.", false);
            BubbleComponentInsertion(window.ComponentInsertion);
        }

        // ---- The matrix --------------------------------------------------------------------------------------

        /// <summary>
        /// Rebuilds the estación × nivel × lado grid.
        ///
        /// It is rebuilt from the DESIGN and holds no state of its own: a cell knows whether it carries an
        /// override by asking the matrix, never by remembering what a previous click did.
        /// </summary>
        private void BuildMatrix()
        {
            MatrixGrid.Children.Clear();
            MatrixGrid.ColumnDefinitions.Clear();
            MatrixGrid.RowDefinitions.Clear();
            cellButtons.Clear();

            var matrix = Matrix;
            var sides = matrix.ActiveSides;

            if (matrix.StationCount <= 0 || matrix.LevelCount <= 0 || sides.Count == 0)
            {
                OverrideCountText.Text = string.Empty;
                UpdateCellHeader();
                return;
            }

            MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            for (var station = 0; station < matrix.StationCount; station++)
            {
                MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            var rows = matrix.LevelCount * sides.Count;

            for (var row = 0; row <= rows; row++)
            {
                MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (var station = 0; station < matrix.StationCount; station++)
            {
                MatrixGrid.Children.Add(Header(
                    "Estación " + (station + 1).ToString(CultureInfo.InvariantCulture), 0, station + 1));
            }

            // Level 1 at the BOTTOM, like every other editor of this product.
            for (var level = matrix.LevelCount - 1; level >= 0; level--)
            {
                foreach (var side in sides)
                {
                    var row = RowOf(level, side, sides, matrix.LevelCount);

                    MatrixGrid.Children.Add(Header(
                        "N" + (level + 1).ToString(CultureInfo.InvariantCulture) + " " + SideLabel(side), row, 0));

                    for (var station = 0; station < matrix.StationCount; station++)
                    {
                        var cell = new CantileverLineCell(station, level, side);
                        var button = CellButton(matrix, cell);
                        Grid.SetRow(button, row);
                        Grid.SetColumn(button, station + 1);
                        MatrixGrid.Children.Add(button);
                        cellButtons[cell] = button;
                    }
                }
            }

            OverrideCountText.Text = matrix.OverrideCount == 0
                ? "Sin excepciones."
                : matrix.OverrideCount.ToString(CultureInfo.InvariantCulture) + " celdas con excepción.";

            if (selectedCell.HasValue && !matrix.IsActive(selectedCell.Value))
            {
                selectedCell = null; // the line shrank under the selection
            }

            HighlightSelection();
            UpdateCellHeader();
        }

        private static int RowOf(int level, CantileverArmSide side, IReadOnlyList<CantileverArmSide> sides, int levelCount)
        {
            var levelRow = levelCount - 1 - level;
            var sideRow = 0;

            for (var index = 0; index < sides.Count; index++)
            {
                if (sides[index] == side)
                {
                    sideRow = index;
                    break;
                }
            }

            return 1 + levelRow * sides.Count + sideRow;
        }

        private static string SideLabel(CantileverArmSide side) =>
            side == CantileverArmSide.NegativeY ? "−Y" : "+Y";

        private static TextBlock Header(string text, int row, int column)
        {
            var block = new TextBlock
            {
                Text = text,
                FontSize = 11,
                Margin = new Thickness(4, 3, 8, 3),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = UiSupport.FrozenBrush(Color.FromRgb(0x61, 0x70, 0x80))
            };

            Grid.SetRow(block, row);
            Grid.SetColumn(block, column);
            return block;
        }

        private Button CellButton(CantileverLineArmMatrix matrix, CantileverLineCell cell)
        {
            var arm = matrix.Effective(cell);
            var hasOverride = matrix.HasOverride(cell);

            var button = new Button
            {
                Content = Describe(arm),
                Tag = cell,
                MinWidth = 84,
                FontSize = 10.5,
                Margin = new Thickness(2),
                Padding = new Thickness(6, 3, 6, 3),
                ToolTip = (hasOverride ? "Excepción · " : "Por omisión · ") + Describe(arm)
            };

            button.Click += Cell_Click;

            if (hasOverride)
            {
                button.FontWeight = FontWeights.Bold;
            }

            return button;
        }

        private static string Describe(CantileverArmTemplateDesign arm)
        {
            if (arm?.Body == null)
            {
                return "—";
            }

            var section = Short(arm.Body.SectionId) ?? "(sin sección)";

            return section + "\n" + arm.Body.CutLength.ToString("0.##", CultureInfo.InvariantCulture) + " in";
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button)?.Tag is CantileverLineCell cell))
            {
                return;
            }

            selectedCell = cell;
            HighlightSelection();
            UpdateCellHeader();
        }

        private void HighlightSelection()
        {
            foreach (var pair in cellButtons)
            {
                pair.Value.BorderThickness = selectedCell.HasValue && pair.Key.Equals(selectedCell.Value)
                    ? new Thickness(2)
                    : new Thickness(1);
            }
        }

        private void UpdateCellHeader()
        {
            if (!selectedCell.HasValue)
            {
                CellHeaderText.Text = "Ninguna celda seleccionada — elige una para ver su brazo.";
                CellSummaryText.Text = string.Empty;
                EditCellArmButton.IsEnabled = false;
                RestoreCellButton.IsEnabled = false;
                return;
            }

            var cell = selectedCell.Value;
            var matrix = Matrix;

            CellHeaderText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Celda seleccionada: estación {0}, nivel {1}, lado {2}{3}",
                cell.StationIndex + 1, cell.LevelIndex + 1, SideLabel(cell.Side),
                matrix.HasOverride(cell) ? " (con excepción)" : " (sigue el brazo por omisión)");

            var arm = matrix.Effective(cell);
            CellSummaryText.Text = arm?.Body == null
                ? string.Empty
                : (Short(arm.Body.SectionId) ?? "(sin sección)") + " · corte " +
                  arm.Body.CutLength.ToString("0.##", CultureInfo.InvariantCulture) + " in · pendiente " +
                  arm.Body.SlopeRisePer12.ToString("0.##", CultureInfo.InvariantCulture) + "/12 · " +
                  (arm.MountingPlate?.VerticalPunchCount ?? 0) + " troqueles";

            EditCellArmButton.IsEnabled = true;
            RestoreCellButton.IsEnabled = true;
        }

        private CantileverLineApplyScope SelectedScope() =>
            Enum.IsDefined(typeof(CantileverLineApplyScope), ScopeBox.SelectedIndex)
                ? (CantileverLineApplyScope)ScopeBox.SelectedIndex
                : CantileverLineApplyScope.Cell;

        /// <summary>
        /// Opens the arm configurator for the selected scope, and applies what it returns as ONE operation.
        ///
        /// The full arm form used to live under the matrix; moving it out is half of the saturation the owner
        /// rejected. The scope, the single notification and the single regeneration are unchanged.
        /// </summary>
        private void EditCellArm_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedCell.HasValue)
            {
                SetStatus("Elige primero una celda de la matriz.", true);
                return;
            }

            EditArm(selectedCell.Value);
        }

        private void EditArm(CantileverLineCell? cell)
        {
            if (assembler == null)
            {
                return;
            }

            var matrix = Matrix;
            var current = cell.HasValue
                ? matrix.Effective(cell.Value) ?? design.DefaultArmTemplate
                : design.DefaultArmTemplate;

            var scope = cell.HasValue
                ? DescribeScope(SelectedScope(), cell.Value)
                : null;

            var window = new CantileverArmWindow(
                current, design.StationTopology?.ColumnBaseTemplate, assembler.Catalogue, scope) { Owner = this };

            window.ShowDialog();

            if (window.Result == null)
            {
                return; // cancelled: the design was not touched
            }

            if (!cell.HasValue)
            {
                design.DefaultArmTemplate = window.Result;
                Recompute();
                SetStatus("Brazo por omisión actualizado.", false);
                BubbleComponentInsertion(window.ComponentInsertion);
                return;
            }

            // ONE write and ONE recompute per operation: the matrix reports whether anything actually moved.
            var change = matrix.Apply(SelectedScope(), cell.Value, window.Result);
            Recompute();

            SetStatus(change.IsNoOp
                ? "Ninguna celda cambió: ese brazo ya estaba en vigor en las " + change.Count + " celdas del alcance."
                : change.Changed.Count + " de " + change.Count + " celdas actualizadas.", false);

            BubbleComponentInsertion(window.ComponentInsertion);
        }

        private static string DescribeScope(CantileverLineApplyScope scope, CantileverLineCell cell)
        {
            switch (scope)
            {
                case CantileverLineApplyScope.Station:
                    return "estación " + (cell.StationIndex + 1);
                case CantileverLineApplyScope.Level:
                    return "nivel " + (cell.LevelIndex + 1) + " de toda la línea";
                case CantileverLineApplyScope.Side:
                    return "lado " + SideLabel(cell.Side) + " de toda la línea";
                case CantileverLineApplyScope.Line:
                    return "toda la línea";
                default:
                    return "estación " + (cell.StationIndex + 1) + ", nivel " + (cell.LevelIndex + 1) +
                           ", lado " + SideLabel(cell.Side);
            }
        }

        private void RestoreCell_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedCell.HasValue)
            {
                SetStatus("Elige primero una celda de la matriz.", true);
                return;
            }

            var change = Matrix.Restore(SelectedScope(), selectedCell.Value);
            Recompute();

            SetStatus(change.IsNoOp
                ? "Ninguna celda cambió: esas celdas ya seguían el brazo por omisión."
                : change.Changed.Count + " de " + change.Count + " celdas volvieron al brazo por omisión.", false);
        }

        // ---- Preview -----------------------------------------------------------------------------------------

        private CantileverViewKind SelectedViewKind()
        {
            if (PreviewLateralRadio.IsChecked == true)
            {
                return CantileverViewKind.Lateral;
            }

            return PreviewPlantaRadio.IsChecked == true
                ? CantileverViewKind.Planta
                : CantileverViewKind.Frontal;
        }

        /// <summary>The base-zero station the lateral shows. The box is base-ONE, because a user counts from one.</summary>
        private int LateralStationIndex()
        {
            var value = LateralStationBox.Value;
            var index = value.HasValue ? (int)Math.Round(value.Value) - 1 : 0;
            return index < 0 ? 0 : index;
        }

        private void RenderPreview()
        {
            var view = SelectedViewKind();

            PreviewHint.Text = view == CantileverViewKind.Lateral
                ? "Vista lateral de la estación " + (LateralStationIndex() + 1).ToString(CultureInfo.InvariantCulture)
                  + ". El arriostramiento vive ENTRE estaciones, así que no aparece aquí."
                : view == CantileverViewKind.Planta
                    ? PlantaHint()
                    : "Vista frontal de la línea completa, con sus paneles arriostrados.";

            var plan = lastComputation != null && lastComputation.IsValid
                ? assembler.View(
                    lastComputation.Line, view, LateralStationIndex(), design.PlantaVisibility)
                : null;

            CantileverPreviewRenderer.Render(
                PreviewCanvas, plan,
                lastComputation == null
                    ? "Corrige los datos: la línea no se ha resuelto todavía."
                    : "Esta vista no dibuja nada.");

            BuildLegend(plan);
        }

        private void BuildLegend(CantileverViewPlan plan)
        {
            LegendPanel.Children.Clear();

            foreach (var kind in CantileverPreviewRenderer.KindsIn(plan))
            {
                var entry = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 12, 0) };

                entry.Children.Add(new Border
                {
                    Width = 12,
                    Height = 6,
                    Background = CantileverPreviewRenderer.BrushFor(kind),
                    Margin = new Thickness(0, 4, 4, 0)
                });

                entry.Children.Add(new TextBlock
                {
                    Text = PieceLabel(kind),
                    FontSize = 10.5,
                    Foreground = UiSupport.FrozenBrush(Color.FromRgb(0x61, 0x70, 0x80))
                });

                LegendPanel.Children.Add(entry);
            }
        }

        private static string PieceLabel(CantileverViewPieceKind kind)
        {
            switch (kind)
            {
                case CantileverViewPieceKind.Column: return "columna";
                case CantileverViewPieceKind.Base: return "base";
                case CantileverViewPieceKind.Arm: return "brazo";
                case CantileverViewPieceKind.Plate: return "placa";
                case CantileverViewPieceKind.Gusset: return "cartabón";
                case CantileverViewPieceKind.Separator: return "separador";
                case CantileverViewPieceKind.Brace: return "tensor";
                case CantileverViewPieceKind.ColdRolledAdapter: return "adaptador";
                case CantileverViewPieceKind.Punch: return "troquel";
                default: return kind.ToString();
            }
        }

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderPreview();

        private void PreviewView_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync || !IsLoaded)
            {
                return;
            }

            RenderPreview();
        }

        private void LateralStation_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            RenderPreview();
        }

        /// <summary>
        /// Los dos interruptores de la PLANTA.
        ///
        /// Sólo redibujan: no llaman a `RequestRecompute`, y eso es lo que dice que son de dibujo y no de
        /// producto. La línea no se vuelve a resolver, el BOM no se recalcula y la firma física no se mueve —
        /// lo único que cambia es qué entra en una vista.
        /// </summary>
        private void PlantaVisibility_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            design.PlantaVisibility = new CantileverPlantaVisibilityDesign
            {
                ShowArms = PlantaShowArmsCheck.IsChecked == true,
                ShowBraces = PlantaShowBracesCheck.IsChecked == true
            };

            RenderPreview();
        }

        /// <summary>El texto de ayuda de la planta, que dice lo que NO se está dibujando.</summary>
        private string PlantaHint()
        {
            var planta = design.PlantaVisibility ?? new CantileverPlantaVisibilityDesign();

            var hidden = new List<string>();

            if (!planta.ShowArms)
            {
                hidden.Add("brazos");
            }

            if (!planta.ShowBraces)
            {
                hidden.Add("tensores");
            }

            // Se dice lo que falta en vez de callarlo: una planta a la que le faltan piezas y no lo avisa es
            // indistinguible de una que se resolvio mal.
            return hidden.Count == 0
                ? "Vista en planta de la línea completa."
                : "Vista en planta de la línea completa, SIN " + string.Join(" ni ", hidden) +
                  ". No cambia el BOM ni la firma física.";
        }

        // ---- Input handlers ----------------------------------------------------------------------------------

        // ---- Secuencia de paneles: la tabla avanzada -------------------------------------------------------

        /// <summary>
        /// Vuelca los tramos del editor en la tabla.
        ///
        /// La tabla es una VISTA del estado, no su dueña: se reconstruye desde <c>panelEditor</c> despues de
        /// cada accion, asi que nunca puede quedar contando una historia distinta de la que se va a resolver.
        /// </summary>
        private void RefreshPanelRows()
        {
            var wasSuppressed = suppressSync;
            suppressSync = true;

            try
            {
                var selected = PanelSegmentGrid.SelectedIndex;

                panelRows.Clear();

                for (var i = 0; i < panelEditor.Segments.Count; i++)
                {
                    panelRows.Add(CantileverPanelSegmentRow.From(i, panelEditor.Segments[i], NumberFormat));
                }

                AdvancedPanelArea.Visibility = panelEditor.IsAdvanced
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if (selected >= 0 && selected < panelRows.Count)
                {
                    PanelSegmentGrid.SelectedIndex = selected;
                }
                else if (panelRows.Count > 0)
                {
                    PanelSegmentGrid.SelectedIndex = 0;
                }
            }
            finally
            {
                suppressSync = wasSuppressed;
            }
        }

        /// <summary>Muestra —o retira— el motivo por el que una accion no se pudo hacer.</summary>
        private void ShowPanelLayoutMessage(string message)
        {
            PanelLayoutErrorText.Text = message ?? string.Empty;
            PanelLayoutErrorText.Visibility = string.IsNullOrEmpty(message)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void ApplyPanelEdit(CantileverPanelEditResult result)
        {
            ShowPanelLayoutMessage(result.Applied ? null : result.Reason);
            RefreshPanelRows();

            if (result.Applied)
            {
                RequestRecompute();
            }
        }

        private int SelectedPanelIndex => PanelSegmentGrid.SelectedIndex;

        private void PanelLayoutMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            var wantsAdvanced = PanelLayoutModeBox.SelectedIndex == 1;

            if (wantsAdvanced == panelEditor.IsAdvanced)
            {
                return;
            }

            if (wantsAdvanced)
            {
                // MATERIALIZA la secuencia que se esta viendo: el usuario empieza a editar lo que ya tenia, no
                // una lista en blanco.
                ApplyPanelEdit(panelEditor.MaterializeAutomatic(CurrentAutomaticSegments()));
                return;
            }

            var restored = panelEditor.RestoreAutomatic();

            // AVISA antes de devolver la autoridad a la regla, porque la lista manual deja de mandar. No se
            // borra —se conserva por si vuelve— pero el dibujo y el BOM pasan a salir de la regla, y eso el
            // usuario tiene que saberlo ANTES.
            if (restored.ReplacesManualWork && !ConfirmDiscardingManualPanels(restored.Reason))
            {
                PanelLayoutModeBox.SelectedIndex = 1;
                panelEditor.MaterializeAutomatic(panelEditor.Segments.ToList());
                RefreshPanelRows();
                return;
            }

            ApplyPanelEdit(restored);
        }

        /// <summary>
        /// La lista que la REGLA produce ahora mismo, o vacia si la secuencia no resuelve.
        ///
        /// Sale de la resolucion vigente y no de un calculo aparte: materializar tiene que dar exactamente lo
        /// que se esta dibujando, o el cambio de modo movería el dibujo por su cuenta.
        /// </summary>
        private IReadOnlyList<CantileverPanelSegmentDesign> CurrentAutomaticSegments()
        {
            var interval = lastComputation?.Line?.Intervals?.FirstOrDefault();

            return interval?.Layout?.EffectiveSegments
                ?? (IReadOnlyList<CantileverPanelSegmentDesign>)Array.Empty<CantileverPanelSegmentDesign>();
        }

        private void PanelAdd_Click(object sender, RoutedEventArgs e) =>
            ApplyPanelEdit(panelEditor.Add(
                PanelHeightBox.Value ?? CantileverLineDefaults.BracedPanelHeight,
                CantileverPanelBracingMode.CrossBraced));

        private void PanelRemove_Click(object sender, RoutedEventArgs e) =>
            ApplyPanelEdit(panelEditor.Remove(SelectedPanelIndex));

        private void PanelUp_Click(object sender, RoutedEventArgs e) =>
            ApplyPanelEdit(panelEditor.Move(SelectedPanelIndex, 1));

        private void PanelDown_Click(object sender, RoutedEventArgs e) =>
            ApplyPanelEdit(panelEditor.Move(SelectedPanelIndex, -1));

        private void PanelSplit_Click(object sender, RoutedEventArgs e) =>
            ApplyPanelEdit(panelEditor.Split(SelectedPanelIndex));

        private void PanelMerge_Click(object sender, RoutedEventArgs e) =>
            ApplyPanelEdit(panelEditor.MergeWithNext(SelectedPanelIndex));

        private void PanelToggle_Click(object sender, RoutedEventArgs e) =>
            ApplyPanelEdit(panelEditor.ToggleBracing(SelectedPanelIndex));

        private void PanelMaterialize_Click(object sender, RoutedEventArgs e) =>
            ApplyPanelEdit(panelEditor.MaterializeAutomatic(CurrentAutomaticSegments()));

        /// <summary>
        /// Una cota o una casilla editada a mano vuelve al estado.
        ///
        /// Se lee la fila ENTERA y se reescribe el tramo, en vez de aplicar solo la celda que cambio: una cota
        /// escrita a mano puede dejar la lista incompleta, y quien decide si eso es legal es la validacion al
        /// resolver, no esta casilla.
        /// </summary>
        private void PanelSegment_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (suppressSync || e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            // La celda aun no ha escrito su valor en el objeto cuando llega este evento, asi que se aplica
            // despues de que el enlace lo haya hecho.
            Dispatcher.BeginInvoke(new Action(CommitPanelRows), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void CommitPanelRows()
        {
            var edited = new List<CantileverPanelSegmentDesign>();

            foreach (var row in panelRows)
            {
                if (!row.TryToDesign(out var segment, out var reason))
                {
                    ShowPanelLayoutMessage(reason);
                    RefreshPanelRows();
                    return;
                }

                edited.Add(segment);
            }

            panelEditor = new CantileverPanelLayoutEditorState(CantileverPanelLayoutMode.Advanced, edited);

            ShowPanelLayoutMessage(null);
            RefreshPanelRows();
            RequestRecompute();
        }

        private void Input_Changed(object sender, RoutedEventArgs e) => RequestRecompute();

        private void Combo_Changed(object sender, SelectionChangedEventArgs e) => RequestRecompute();

        private void Name_Changed(object sender, TextChangedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            session.Identity.SetName(NameBox.Text?.Trim());
            design.Name = NameBox.Text?.Trim();
        }

        // ---- Draw, BOM and library ---------------------------------------------------------------------------

        private void InsertFrontal_Click(object sender, RoutedEventArgs e) =>
            RequestDraw(RackEmbedDocument.ViewFrontal, -1, updateOnly: false);

        private void InsertPlanta_Click(object sender, RoutedEventArgs e) =>
            RequestDraw(RackEmbedDocument.ViewPlanta, -1, updateOnly: false);

        private void InsertLateral_Click(object sender, RoutedEventArgs e) =>
            RequestDraw(RackEmbedDocument.ViewLateral, LateralStationIndex(), updateOnly: false);

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
                SetStatus("Solo una línea abierta con RACKEDITAR puede actualizarse en sitio.", true);
                return;
            }

            Recompute(); // synchronous validate + build

            if (!currentInputsAreValid || lastComputation == null)
            {
                SetStatus("Corrige los datos: no se puede insertar una línea que no se resolvió.", true);
                return; // never fall back to the previous valid line
            }

            if (!updateOnly &&
                string.Equals(view, RackEmbedDocument.ViewLateral, StringComparison.OrdinalIgnoreCase) &&
                (section < 0 || section >= lastComputation.Line.Stations.Count))
            {
                SetStatus("La línea no tiene esa estación; elige una entre 1 y "
                          + lastComputation.Line.Stations.Count.ToString(CultureInfo.InvariantCulture) + ".", true);
                return;
            }

            session.Identity.SetName(NameBox.Text?.Trim());
            session.SetModel(lastComputation.Design, lastComputation.Line);

            if (updateOnly)
            {
                session.RequestUpdate(ctx => new CantileverInsertionRequest(
                    lastComputation.Line, lastComputation.Design, ctx.Id, ctx.Name, ctx.View, ctx.Section, sourceProject));
            }
            else
            {
                session.RequestInsert(view, section, ctx => new CantileverInsertionRequest(
                    lastComputation.Line, lastComputation.Design, ctx.Id, ctx.Name, ctx.View, ctx.Section, sourceProject));
            }

            Close();
        }

        private void Bom_Click(object sender, RoutedEventArgs e)
        {
            Recompute();

            if (!currentInputsAreValid || lastComputation?.Bom == null)
            {
                SetStatus("Corrige los datos: no se puede mostrar el BOM de una línea que no se resolvió.", true);
                return;
            }

            new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();
        }

        private void SaveLibrary_Click(object sender, RoutedEventArgs e)
        {
            Recompute();

            if (!currentInputsAreValid || lastComputation?.Design == null)
            {
                SetStatus("Corrige los datos: no se puede guardar una línea que no se resolvió.", true);
                return;
            }

            var path = UiSupport.PromptSaveToLibrary(this, NameBox.Text, "linea");

            if (path == null)
            {
                return;
            }

            try
            {
                session.Identity.EnsureId();
                session.Identity.SetName(NameBox.Text?.Trim());

                var project = RackProject.ForCantilever(lastComputation.Design).WithSourceMetadataFrom(sourceProject);
                new RackProjectStore().Save(project, path);
                SetStatus("Línea guardada: " + System.IO.Path.GetFileName(path), false);
            }
            catch (System.IO.IOException ex)
            {
                SetStatus("No se pudo guardar: " + ex.Message, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                SetStatus("No se pudo guardar: " + ex.Message, true);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        // ---- Small helpers -----------------------------------------------------------------------------------

        private void UpdateGuid() =>
            GuidText.Text = string.IsNullOrWhiteSpace(session.Identity.Id)
                ? "(se asigna al insertar)"
                : session.Identity.Id;

        private void UpdateButtons()
        {
            var canDraw = canInsertInAutoCad && currentInputsAreValid;

            InsertFrontalButton.IsEnabled = canDraw;
            InsertLateralButton.IsEnabled = canDraw;
            InsertPlantaButton.IsEnabled = canDraw;
            UpdateButton.IsEnabled = canDraw && isEditingExisting;
            BomButton.IsEnabled = currentInputsAreValid;
            SaveLibraryButton.IsEnabled = currentInputsAreValid;

            if (!canInsertInAutoCad)
            {
                InsertFrontalButton.ToolTip = "Disponible solo cuando la ventana se abre desde AutoCAD.";
                InsertLateralButton.ToolTip = InsertFrontalButton.ToolTip;
                InsertPlantaButton.ToolTip = InsertFrontalButton.ToolTip;
                UpdateButton.ToolTip = InsertFrontalButton.ToolTip;
            }
            else if (!isEditingExisting)
            {
                UpdateButton.ToolTip = "Disponible solo para una línea abierta con RACKEDITAR.";
            }
        }

        private void SetStatus(string message, bool isError) => UiSupport.SetStatus(StatusText, message, isError);
    }
}
