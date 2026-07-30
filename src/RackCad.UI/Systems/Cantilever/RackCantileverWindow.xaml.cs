using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Controls;
using RackCad.UI.Editor;

namespace RackCad.UI.Systems.Cantilever
{
    /// <summary>One catalogued section as a picker row: the exact id, and a label a human recognises.</summary>
    internal sealed class CantileverSectionOption
    {
        internal CantileverSectionOption(string id, string label)
        {
            Id = id;
            Label = label;
        }

        /// <summary>The EXACT <c>StructuralSectionId</c> text. Null is the "no section chosen" row.</summary>
        public string Id { get; }

        public string Label { get; }
    }

    /// <summary>
    /// The Cantilever LINE editor (I-37D).
    ///
    /// It is a thin coordinator over the pure model: the editable authority is a <see cref="CantileverLineDesign"/>,
    /// the per-cell writes go through <see cref="CantileverLineArmMatrix"/>, the recompute is
    /// <see cref="CantileverLineEditorAssembler"/>, and identity plus the insert/update contract live on the shared
    /// <see cref="RackEditorSession{TDesign,TSystem}"/>. It resolves no geometry, builds no BOM, decides no panel
    /// count and knows no AutoCAD type: the Plugin host draws the payload it produces.
    ///
    /// Numeric entry uses <see cref="NumericField"/>; a field in error blocks the recompute, so a stale line is
    /// never inserted or saved behind the user's back.
    /// </summary>
    public partial class RackCantileverWindow : Window
    {
        private static readonly string[] FaceModes = { "Sencilla", "Doble" };
        private static readonly string[] Sides = { "Lado +Y", "Lado −Y" };
        private static readonly string[] HeightModes = { "Automática", "Manual" };
        private static readonly string[] PanelModes = { "Automático", "Manual" };
        private static readonly string[] BraceKinds = { "Cold rolled (varilla)", "Estructural (perfil)" };
        private static readonly string[] Arrangements = { "Sencillo", "Canal doble enfrentada", "Canal doble espalda" };
        private static readonly string[] EndPlateModes = { "Ninguna", "Placa", "Placa con tope" };
        private static readonly string[] Scopes = { "Celda", "Estación", "Nivel (toda la línea)", "Lado (toda la línea)", "Toda la línea" };

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
        private bool currentInputsAreValid;

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
            // that failed validation. It is not an exception, because a window that cannot open leaves the user
            // with nothing to read.
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
            BraceKindBox.ItemsSource = BraceKinds;
            ArmArrangementBox.ItemsSource = Arrangements;
            CellArrangementBox.ItemsSource = Arrangements;
            ArmEndPlateModeBox.ItemsSource = EndPlateModes;
            CellEndPlateModeBox.ItemsSource = EndPlateModes;
            ScopeBox.ItemsSource = Scopes;
            ScopeBox.SelectedIndex = 0;

            LoadSectionPickers();
            LoadNew();
        }

        // ---- Test seams (internal) --------------------------------------------------------------------------

        internal RackEditorSession<CantileverLineDesign, CantileverLineAssembly> Session => session;

        internal CantileverLineEditorAssembler Assembler => assembler;

        /// <summary>The LIVE design the controls write into. Tests read it to assert what an edit produced.</summary>
        internal CantileverLineDesign Design => design;

        internal CantileverLineEditorComputation LastComputation => lastComputation;

        internal bool CurrentInputsAreValid => currentInputsAreValid;

        internal CantileverLineCell? SelectedCell => selectedCell;

        internal CantileverLineArmMatrix Matrix => new CantileverLineArmMatrix(design);

        /// <summary>The plan the preview is currently drawing, so a test asserts CONTENT and never pixels.</summary>
        internal CantileverViewPlan CurrentPreviewPlan =>
            lastComputation != null && lastComputation.IsValid
                ? assembler.View(lastComputation.Line, SelectedViewKind(), LateralStationIndex())
                : null;

        // ---- The editor→host contract (mirrors the Push Back editor) -----------------------------------------

        public bool InsertRequested => session.InsertRequested;

        public bool UpdateOnly => session.UpdateOnly;

        public string RackId => session.Identity.Id;

        public string RackName => session.Identity.Name;

        public string InsertView => session.InsertView;

        public int InsertSection => session.InsertSection;

        public RackInsertionRequest InsertionRequest => session.InsertionRequest;

        public CantileverLineAssembly LineToInsert => (session.InsertionRequest as CantileverInsertionRequest)?.Line;

        public CantileverLineDesign DesignToInsert => (session.InsertionRequest as CantileverInsertionRequest)?.Design;

        // ---- Loading -----------------------------------------------------------------------------------------

        /// <summary>A brand-new line: the domain defaults, no identity, insert only.</summary>
        public void LoadNew()
        {
            // No section id is invented here. The three sections and the two mandatory punch margins have no
            // approved default, so the line opens BLOCKED and says which value it is waiting for — inventing one
            // would be indistinguishable from a value the owner approved.
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
            session.Identity.Adopt(null, rackName); // no id -> a fresh GUID is minted on insert
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
                SpacingBox.SetNumber(design.ColumnCentreSpacing);

                var topology = design.StationTopology ?? new CantileverLineStationTopologyDesign();
                FaceModeBox.SelectedIndex = topology.FaceMode == CantileverStationFaceMode.Double ? 1 : 0;
                SingleSideBox.SelectedIndex = topology.SingleSide == CantileverArmSide.NegativeY ? 1 : 0;
                LevelCountBox.SetNumber(topology.LevelCount, "0");
                FirstPunchBox.SetNumber(topology.FirstLevelPunchIndex, "0");
                ClearHeightBox.SetNumber(topology.RequestedClearHeight);
                TopClearFactorBox.SetNumber(topology.TopClearFactor);
                HeightModeBox.SelectedIndex =
                    (topology.ColumnHeight?.Mode ?? CantileverStationColumnHeightMode.Automatic)
                        == CantileverStationColumnHeightMode.Manual ? 1 : 0;
                ManualHeightBox.SetNumber(topology.ColumnHeight?.ManualHeight);

                var template = topology.ColumnBaseTemplate ?? new CantileverStationColumnBaseTemplateDesign();
                ColumnSectionBox.SelectedValue = template.ColumnSectionId;
                ColumnPlateThicknessBox.SetNumber(template.ColumnBottomPlate?.Thickness);
                BaseSectionBox.SelectedValue = template.Base?.SectionId;
                BaseLengthBox.SetNumber(template.Base?.Length);
                BaseFrontPlateBox.SetNumber(template.Base?.FrontPlate?.Thickness);
                BaseRearPlateBox.SetNumber(template.Base?.RearPlate?.Thickness);
                BaseGussetBox.SetNumber(template.Base?.Gusset?.Thickness);

                var punches = template.Connection?.Punches ?? new CantileverPunchParameters();
                PunchDiameterBox.SetNumber(punches.Diameter);
                PunchHorizontalOffsetBox.SetNumber(punches.HorizontalEndOffset);
                ConnectionPitchBox.SetNumber(punches.ConnectionPitch);
                ConnectionPunchesAboveBaseBox.SetNumber(punches.ConnectionPunchesAboveBase, "0");
                RearPlateOffsetBox.SetNumber(punches.RearPlateVerticalEndOffset);
                RegularPitchBox.SetNumber(punches.RegularColumnPitch);
                BottomPlatePitchBox.SetNumber(punches.ColumnBottomPlatePitch);
                BottomPlateEndOffsetBox.SetNumber(punches.ColumnBottomPlateEndOffset);
                TopPunchOffsetBox.SetNumber(punches.ColumnTopPunchOffset);

                WriteArm(
                    design.DefaultArmTemplate ?? new CantileverArmTemplateDesign(),
                    ArmArrangementBox, ArmSectionBox, ArmCutLengthBox, ArmSlopeBox, ArmPlateThicknessBox,
                    ArmPunchCountBox, ArmVerticalOffsetBox, ArmEndPlateModeBox, ArmEndPlateThicknessBox, ArmExtraStopBox);

                var bracing = design.Bracing ?? new CantileverBracingDesign();
                SeparatorSectionBox.SelectedValue = bracing.SeparatorSectionId;
                PanelModeBox.SelectedIndex = bracing.PanelCountMode == CantileverBracedPanelCountMode.Manual ? 1 : 0;
                ManualPanelCountBox.SetNumber(bracing.ManualPanelCount, "0");
                PanelHeightBox.SetNumber(bracing.BracedPanelHeight);
                CentralSpaceBox.SetNumber(bracing.CentralEmptySpaceHeight);
                BraceKindBox.SelectedIndex = bracing.BraceKind == CantileverBraceBodyKind.StructuralSection ? 1 : 0;
                BraceSectionBox.SelectedValue = bracing.BraceSectionId;
                RodDiameterBox.SetNumber(bracing.ColdRolled?.Diameter);

                LateralStationBox.SetNumber(1, "0");
            }
            finally
            {
                suppressSync = false;
            }

            Recompute();
        }

        private void LoadSectionPickers()
        {
            if (assembler == null)
            {
                return; // no catalogue: the pickers stay empty and every action is disabled
            }

            var w = Options(StructuralSectionFamily.W);
            var channels = Options(StructuralSectionFamily.Channel);
            var braces = Options(StructuralSectionFamily.Channel, StructuralSectionFamily.Angle);
            var all = Options();

            ColumnSectionBox.ItemsSource = w;
            BaseSectionBox.ItemsSource = w;
            SeparatorSectionBox.ItemsSource = channels;
            BraceSectionBox.ItemsSource = braces;
            ArmSectionBox.ItemsSource = all;
            CellSectionBox.ItemsSource = Options();
        }

        /// <summary>The picker rows of one or more families; no family means the whole catalogue.</summary>
        private List<CantileverSectionOption> Options(params StructuralSectionFamily[] families)
        {
            var sections = families == null || families.Length == 0
                ? assembler.Catalogue.Enabled
                : families.SelectMany(f => assembler.SectionsOf(f)).Where(s => s.IsEnabled).ToList();

            var options = new List<CantileverSectionOption> { new CantileverSectionOption(null, "(sin sección)") };

            options.AddRange(sections
                .OrderBy(s => s.Family)
                .ThenBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(s => new CantileverSectionOption(s.SectionId.Value, s.DisplayName + "  ·  " + s.SectionId.Value)));

            return options;
        }

        // ---- Reading the controls ----------------------------------------------------------------------------

        /// <summary>
        /// Writes every control into the live design. Returns false — with the reason — when a numeric field is in
        /// error, so a half-parsed design never reaches the resolver.
        /// </summary>
        private bool ReadInputs(out string error)
        {
            error = null;

            var fields = new[]
            {
                StationCountBox, SpacingBox, LevelCountBox, FirstPunchBox, ClearHeightBox, TopClearFactorBox,
                ManualHeightBox, ColumnPlateThicknessBox, BaseLengthBox, BaseFrontPlateBox, BaseRearPlateBox,
                BaseGussetBox, PunchDiameterBox, PunchHorizontalOffsetBox, ConnectionPitchBox,
                ConnectionPunchesAboveBaseBox, RearPlateOffsetBox, RegularPitchBox, BottomPlatePitchBox,
                BottomPlateEndOffsetBox, TopPunchOffsetBox, ArmCutLengthBox, ArmSlopeBox, ArmPlateThicknessBox,
                ArmPunchCountBox, ArmVerticalOffsetBox, ArmEndPlateThicknessBox, ArmExtraStopBox,
                ManualPanelCountBox, PanelHeightBox, CentralSpaceBox, RodDiameterBox
            };

            var broken = fields.FirstOrDefault(f => f.HasError);

            if (broken != null)
            {
                error = broken.ErrorMessage ?? "Hay un campo numérico inválido.";
                return false;
            }

            design.StationCount = (int)Math.Round(StationCountBox.Value ?? design.StationCount);
            design.ColumnCentreSpacing = SpacingBox.Value ?? design.ColumnCentreSpacing;

            var topology = design.StationTopology ??= new CantileverLineStationTopologyDesign();
            topology.FaceMode = FaceModeBox.SelectedIndex == 1
                ? CantileverStationFaceMode.Double
                : CantileverStationFaceMode.Single;
            topology.SingleSide = SingleSideBox.SelectedIndex == 1
                ? CantileverArmSide.NegativeY
                : CantileverArmSide.PositiveY;
            topology.LevelCount = (int)Math.Round(LevelCountBox.Value ?? topology.LevelCount);
            topology.FirstLevelPunchIndex = (int)Math.Round(FirstPunchBox.Value ?? topology.FirstLevelPunchIndex);
            topology.RequestedClearHeight = ClearHeightBox.Value ?? topology.RequestedClearHeight;
            topology.TopClearFactor = TopClearFactorBox.Value ?? topology.TopClearFactor;

            var height = topology.ColumnHeight ??= new CantileverStationColumnHeightDesign();
            height.Mode = HeightModeBox.SelectedIndex == 1
                ? CantileverStationColumnHeightMode.Manual
                : CantileverStationColumnHeightMode.Automatic;
            height.ManualHeight = ManualHeightBox.Value;

            var template = topology.ColumnBaseTemplate ??= new CantileverStationColumnBaseTemplateDesign();
            template.ColumnSectionId = SelectedSection(ColumnSectionBox);
            (template.ColumnBottomPlate ??= new CantileverPlateDesign()).Thickness =
                ColumnPlateThicknessBox.Value ?? template.ColumnBottomPlate.Thickness;

            var baseDesign = template.Base ??= new CantileverBaseDesign();
            baseDesign.SectionId = SelectedSection(BaseSectionBox);
            baseDesign.Length = BaseLengthBox.Value ?? baseDesign.Length;
            (baseDesign.FrontPlate ??= new CantileverPlateDesign()).Thickness =
                BaseFrontPlateBox.Value ?? baseDesign.FrontPlate.Thickness;
            (baseDesign.RearPlate ??= new CantileverPlateDesign()).Thickness =
                BaseRearPlateBox.Value ?? baseDesign.RearPlate.Thickness;
            (baseDesign.Gusset ??= new CantileverGussetDesign()).Thickness =
                BaseGussetBox.Value ?? baseDesign.Gusset.Thickness;

            var connection = template.Connection ??= new CantileverColumnBaseConnectionDesign();
            var punches = connection.Punches ??= new CantileverPunchParameters();
            punches.Diameter = PunchDiameterBox.Value ?? punches.Diameter;
            punches.HorizontalEndOffset = PunchHorizontalOffsetBox.Value ?? punches.HorizontalEndOffset;
            punches.ConnectionPitch = ConnectionPitchBox.Value ?? punches.ConnectionPitch;
            punches.ConnectionPunchesAboveBase =
                (int)Math.Round(ConnectionPunchesAboveBaseBox.Value ?? punches.ConnectionPunchesAboveBase);
            punches.RearPlateVerticalEndOffset = RearPlateOffsetBox.Value ?? punches.RearPlateVerticalEndOffset;
            punches.RegularColumnPitch = RegularPitchBox.Value ?? punches.RegularColumnPitch;
            punches.ColumnBottomPlatePitch = BottomPlatePitchBox.Value ?? punches.ColumnBottomPlatePitch;

            // The two mandatory margins are nullable ON PURPOSE: an empty box stays null and the resolver rejects
            // the line by name. Substituting a number here is exactly the invention I-37A refused.
            punches.ColumnBottomPlateEndOffset = BottomPlateEndOffsetBox.Value;
            punches.ColumnTopPunchOffset = TopPunchOffsetBox.Value;

            design.DefaultArmTemplate = ReadArm(
                design.DefaultArmTemplate ?? new CantileverArmTemplateDesign(),
                ArmArrangementBox, ArmSectionBox, ArmCutLengthBox, ArmSlopeBox, ArmPlateThicknessBox,
                ArmPunchCountBox, ArmVerticalOffsetBox, ArmEndPlateModeBox, ArmEndPlateThicknessBox, ArmExtraStopBox);

            var bracing = design.Bracing ??= new CantileverBracingDesign();
            bracing.SeparatorSectionId = SelectedSection(SeparatorSectionBox);
            bracing.PanelCountMode = PanelModeBox.SelectedIndex == 1
                ? CantileverBracedPanelCountMode.Manual
                : CantileverBracedPanelCountMode.Automatic;
            bracing.ManualPanelCount = ManualPanelCountBox.Value.HasValue
                ? (int?)Math.Round(ManualPanelCountBox.Value.Value)
                : null;
            bracing.BracedPanelHeight = PanelHeightBox.Value ?? bracing.BracedPanelHeight;
            bracing.CentralEmptySpaceHeight = CentralSpaceBox.Value ?? bracing.CentralEmptySpaceHeight;
            bracing.BraceKind = BraceKindBox.SelectedIndex == 1
                ? CantileverBraceBodyKind.StructuralSection
                : CantileverBraceBodyKind.ColdRolledRound;
            bracing.BraceSectionId = SelectedSection(BraceSectionBox);
            (bracing.ColdRolled ??= new CantileverColdRolledBraceDesign()).Diameter =
                RodDiameterBox.Value ?? bracing.ColdRolled.Diameter;

            return true;
        }

        private static string SelectedSection(Selector box) => (box.SelectedValue as string)?.Trim();

        /// <summary>Reads an arm out of one set of controls, onto a COPY of <paramref name="current"/>.</summary>
        private static CantileverArmTemplateDesign ReadArm(
            CantileverArmTemplateDesign current,
            Selector arrangement, Selector section, NumericField cutLength, NumericField slope,
            NumericField plateThickness, NumericField punchCount, NumericField verticalOffset,
            Selector endPlateMode, NumericField endPlateThickness, NumericField extraStop)
        {
            var arm = current.DeepCopy();

            var body = arm.Body ??= new CantileverArmBodyDesign();
            body.Arrangement = ArrangementOf(arrangement.SelectedIndex);
            body.SectionId = (section.SelectedValue as string)?.Trim();
            body.CutLength = cutLength.Value ?? body.CutLength;
            body.SlopeRisePer12 = slope.Value ?? body.SlopeRisePer12;

            var plate = arm.MountingPlate ??= new CantileverArmMountingPlateTemplateDesign();
            plate.Thickness = plateThickness.Value ?? plate.Thickness;
            plate.VerticalPunchCount = (int)Math.Round(punchCount.Value ?? plate.VerticalPunchCount);
            plate.VerticalEndOffset = verticalOffset.Value; // mandatory and nullable, like the two punch margins

            var end = arm.EndPlate ??= new CantileverArmEndPlateDesign();
            end.Mode = EndPlateModeOf(endPlateMode.SelectedIndex);
            end.Thickness = endPlateThickness.Value ?? end.Thickness;
            end.ExtraStopHeight = extraStop.Value ?? end.ExtraStopHeight;

            return arm;
        }

        /// <summary>Writes an arm into one set of controls. The exact mirror of <see cref="ReadArm"/>.</summary>
        private static void WriteArm(
            CantileverArmTemplateDesign arm,
            Selector arrangement, Selector section, NumericField cutLength, NumericField slope,
            NumericField plateThickness, NumericField punchCount, NumericField verticalOffset,
            Selector endPlateMode, NumericField endPlateThickness, NumericField extraStop)
        {
            var body = arm.Body ?? new CantileverArmBodyDesign();
            arrangement.SelectedIndex = (int)body.Arrangement;
            section.SelectedValue = body.SectionId;
            cutLength.SetNumber(body.CutLength);
            slope.SetNumber(body.SlopeRisePer12);

            var plate = arm.MountingPlate ?? new CantileverArmMountingPlateTemplateDesign();
            plateThickness.SetNumber(plate.Thickness);
            punchCount.SetNumber(plate.VerticalPunchCount, "0");
            verticalOffset.SetNumber(plate.VerticalEndOffset);

            var end = arm.EndPlate ?? new CantileverArmEndPlateDesign();
            endPlateMode.SelectedIndex = (int)end.Mode;
            endPlateThickness.SetNumber(end.Thickness);
            extraStop.SetNumber(end.ExtraStopHeight);
        }

        private static CantileverArmBodyArrangement ArrangementOf(int index) =>
            Enum.IsDefined(typeof(CantileverArmBodyArrangement), index)
                ? (CantileverArmBodyArrangement)index
                : CantileverArmBodyArrangement.Single;

        private static CantileverArmEndPlateMode EndPlateModeOf(int index) =>
            Enum.IsDefined(typeof(CantileverArmEndPlateMode), index)
                ? (CantileverArmEndPlateMode)index
                : CantileverArmEndPlateMode.None;

        // ---- Recompute ---------------------------------------------------------------------------------------

        private void RequestRecompute() => session.Recompute.Request();

        private void Recompute()
        {
            if (suppressSync)
            {
                return;
            }

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
            SetStatus(Warnings(computation) ?? "Línea recalculada.", Warnings(computation) != null);
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
            var separators = line.Separators.Count;
            var braces = line.Braces.Count;
            var panels = line.BracedPanels.Count;

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} estaciones · {1} intervalos · altura común {2} in · {3} paneles arriostrados · {4} separadores · {5} tensores · {6} componentes en el BOM",
                line.StationCount,
                line.IntervalCount,
                line.ColumnHeight.ToString("0.##", CultureInfo.InvariantCulture),
                panels,
                separators,
                braces,
                computation.Bom?.Components.Count ?? 0);
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
                ? "Sin excepciones: todas las celdas siguen el brazo por omisión."
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

            var section = string.IsNullOrWhiteSpace(arm.Body.SectionId) ? "(sin sección)" : arm.Body.SectionId;

            return section + "\n" + arm.Body.CutLength.ToString("0.##", CultureInfo.InvariantCulture) + " in";
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button)?.Tag is CantileverLineCell cell))
            {
                return;
            }

            selectedCell = cell;

            var arm = Matrix.Effective(cell) ?? new CantileverArmTemplateDesign();
            WriteArm(
                arm, CellArrangementBox, CellSectionBox, CellCutLengthBox, CellSlopeBox, CellPlateThicknessBox,
                CellPunchCountBox, CellVerticalOffsetBox, CellEndPlateModeBox, CellEndPlateThicknessBox, CellExtraStopBox);

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
                CellHeaderText.Text = "Ninguna celda seleccionada — elige una para editar su brazo.";
                ApplyCellButton.IsEnabled = false;
                RestoreCellButton.IsEnabled = false;
                return;
            }

            var cell = selectedCell.Value;
            CellHeaderText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Celda seleccionada: estación {0}, nivel {1}, lado {2}{3}",
                cell.StationIndex + 1, cell.LevelIndex + 1, SideLabel(cell.Side),
                Matrix.HasOverride(cell) ? " (con excepción)" : " (sigue el brazo por omisión)");

            ApplyCellButton.IsEnabled = true;
            RestoreCellButton.IsEnabled = true;
        }

        private CantileverLineApplyScope SelectedScope() =>
            Enum.IsDefined(typeof(CantileverLineApplyScope), ScopeBox.SelectedIndex)
                ? (CantileverLineApplyScope)ScopeBox.SelectedIndex
                : CantileverLineApplyScope.Cell;

        private void ApplyCell_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedCell.HasValue)
            {
                SetStatus("Elige primero una celda de la matriz.", true);
                return;
            }

            var cellFields = new[]
            {
                CellCutLengthBox, CellSlopeBox, CellPlateThicknessBox, CellPunchCountBox, CellVerticalOffsetBox,
                CellEndPlateThicknessBox, CellExtraStopBox
            };

            var broken = cellFields.FirstOrDefault(f => f.HasError);

            if (broken != null)
            {
                SetStatus(broken.ErrorMessage ?? "Hay un campo numérico inválido en la celda.", true);
                return;
            }

            var arm = ReadArm(
                Matrix.Effective(selectedCell.Value) ?? new CantileverArmTemplateDesign(),
                CellArrangementBox, CellSectionBox, CellCutLengthBox, CellSlopeBox, CellPlateThicknessBox,
                CellPunchCountBox, CellVerticalOffsetBox, CellEndPlateModeBox, CellEndPlateThicknessBox, CellExtraStopBox);

            // ONE write and ONE recompute per operation: the matrix reports whether anything actually moved, so a
            // scope that changed nothing does not redraw the preview or claim it did something.
            var change = Matrix.Apply(SelectedScope(), selectedCell.Value, arm);
            Recompute();

            SetStatus(change.IsNoOp
                ? "Ninguna celda cambió: ese brazo ya estaba en vigor en las " + change.Count + " celdas del alcance."
                : change.Changed.Count + " de " + change.Count + " celdas actualizadas.", false);
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
                    ? "Vista en planta de la línea completa."
                    : "Vista frontal de la línea completa, con sus paneles arriostrados.";

            var plan = lastComputation != null && lastComputation.IsValid
                ? assembler.View(lastComputation.Line, view, LateralStationIndex())
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

        // ---- Input handlers ----------------------------------------------------------------------------------

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

                // WithSourceMetadataFrom preserves the opened project's unknown JSON fields + non-downgraded
                // schema version (I-11). Saving never flags an insert.
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
