using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Independent module: the dynamic (pallet flow) system as an editable sequence of modules. It
    /// builds its own headers through the header FACTORY (reusing that logic without coupling to the
    /// header configurator window), draws a richer side view, lets each module be selected/edited,
    /// and opens the existing header configurator to edit a header module's configuration.
    /// </summary>
    public partial class RackDynamicSystemWindow : Window
    {
        private const string KindHeader = "Cabecera";
        private const string KindSeparator = "Separador";

        private static readonly Brush UprightStroke = new SolidColorBrush(Color.FromRgb(0xCF, 0xDB, 0xE8));
        private static readonly Brush HorizontalStroke = new SolidColorBrush(Color.FromRgb(0x3D, 0xC9, 0x86));
        private static readonly Brush DiagonalStroke = new SolidColorBrush(Color.FromRgb(0x5B, 0x8D, 0xEF));
        private static readonly Brush SeparatorStroke = new SolidColorBrush(Color.FromRgb(0x3A, 0x50, 0x68));
        private static readonly Brush PostStroke = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
        private static readonly Brush FloorStroke = new SolidColorBrush(Color.FromRgb(0x6A, 0x7B, 0x8A));
        private static readonly Brush LabelStroke = new SolidColorBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));
        private static readonly Brush SelectionStroke = new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x66));
        private static readonly Brush HeaderFill = new SolidColorBrush(Color.FromArgb(0x36, 0x2A, 0x5B, 0x78));
        private static readonly Brush SeparatorFill = new SolidColorBrush(Color.FromArgb(0x18, 0x8A, 0xA0, 0xB4));
        private static readonly Brush ModuleBoundaryStroke = new SolidColorBrush(Color.FromArgb(0x95, 0x74, 0x86, 0x99));
        private static readonly Brush PostFill = new SolidColorBrush(Color.FromRgb(0x20, 0x34, 0x48));
        private static readonly Brush PlateFill = new SolidColorBrush(Color.FromRgb(0xB7, 0xC3, 0xCF));
        private static readonly Brush ReinforcementStroke = new SolidColorBrush(Color.FromRgb(0xF2, 0xA6, 0x3B));
        private static readonly Brush FlowBedStroke = new SolidColorBrush(Color.FromRgb(0x59, 0xC3, 0xE6));
        private static readonly Brush SafetyStroke = new SolidColorBrush(Color.FromRgb(0xFF, 0xC8, 0x57));
        private static readonly Brush LoadBeamStroke = new SolidColorBrush(Color.FromRgb(0xE0, 0x8A, 0x2B));

        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemBuilder builder;
        private readonly DynamicRackSystemResolver resolver;
        private readonly string defaultPostCatalogId;
        private readonly double defaultHeaderHeight;
        private double computedHeaderHeight;
        private DynamicRackSystem system;
        private DynamicRackDesign design;
        private DynamicRackModule selectedModule;
        private readonly List<SelectiveSafetySelection> safetySelections = new List<SelectiveSafetySelection>();
        private readonly List<DynamicFrontRow> frontRows = new List<DynamicFrontRow>
        {
            new DynamicFrontRow
            {
                Index = 1,
                PalletCount = DynamicRackDefaults.DefaultPalletsWide,
                LoadLevels = DynamicRackDefaults.DefaultLoadLevels,
                PalletsDeep = DynamicRackDefaults.DefaultPalletsDeep,
                DepthStartPosition = 1
            }
        };
        private int selectedFrontIndex;
        private int selectedLevelIndex;
        private readonly HashSet<(int FrontIndex, int LevelIndex)> selectedCells =
            new HashSet<(int FrontIndex, int LevelIndex)> { (0, 0) };
        private int selectedLateralPostIndex;
        private bool suppressLateralPostSelection;
        private DynamicPreviewMode previewMode = DynamicPreviewMode.Lateral;
        private const string AutoDimStyle = "(Automático)";

        private const string ConfigCalculated = "Calculada";
        private readonly List<HeaderPreset> headerPresets = new List<HeaderPreset>();
        private bool suppressConfigSelection;
        private bool suppressRecompose;

        private double mapScale;
        private double mapOffsetX;
        private double mapBottomY;
        private readonly bool canInsertInAutoCad;

        /// <summary>Set when the user asks to draw the system in AutoCAD; the host command draws it after this
        /// window (and the menu) close, so the placement jig has the editor free.</summary>
        public bool InsertRequested { get; private set; }

        public DynamicRackSystem SystemToInsert { get; private set; }

        /// <summary>The editable inputs that produced <see cref="SystemToInsert"/>; this is what the DWG persists.</summary>
        public DynamicRackDesign DesignToInsert { get; private set; }

        /// <summary>Stable id + client name of the system for the drawing round-trip (embed / reopen / edit).</summary>
        public string RackId { get; private set; }
        public string RackName { get; private set; }

        /// <summary>Requested linked view; null when the user only updates existing blocks.</summary>
        public string InsertView { get; private set; }

        /// <summary>Frontal section: 0 = exit, 1 = entrance; -1 for lateral/planta/update.</summary>
        public int InsertSection { get; private set; } = -1;

        public bool UpdateOnly { get; private set; }

        /// <summary>The project this system was opened from (library), exposed so the host command can carry its wrapper
        /// metadata into a library→drawing insert (I-11). Null for a brand-new system.</summary>
        public RackProject SourceProjectToInsert => sourceProject;

        private string currentId;
        private string currentName;
        private bool isEditingExisting;

        /// <summary>The project this system was opened from (library or drawing), if any, so a re-save preserves its
        /// wrapper's unknown JSON metadata + non-downgraded schema version. SaveSystem_Click re-snapshots the design, so
        /// this retained field is the ONLY source of that metadata (it cannot be recovered from the recomputed design). (I-11)</summary>
        private RackProject sourceProject;

        public RackDynamicSystemWindow()
            : this(false)
        {
        }

        public RackDynamicSystemWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();

            catalog = UiSupport.LoadCatalogSafe();
            builder = new DynamicRackSystemBuilder(catalog);
            resolver = new DynamicRackSystemResolver(catalog);
            safetySelections.AddRange(DynamicSafetyDefaults.Build(catalog).Select(selection => selection.DeepCopy()));
            defaultPostCatalogId = catalog.Defaults.Post;
            defaultHeaderHeight = catalog.Defaults.DefaultHeaderHeight;
            computedHeaderHeight = defaultHeaderHeight;
            PostBox.ItemsSource = BuildPostOptions();
            PostBox.SelectedValue = defaultPostCatalogId;
            PostPeralteBox.Text = Num(SelectedPostCatalogPeralte());
            SelectedInOutBeamBox.ItemsSource = InOutBeamOptions();
            SelectedIntermediateBeamBox.ItemsSource = IntermediateBeamOptions();
            EnsureCellCount(frontRows[0], frontRows[0].LoadLevels);
            KindBox.ItemsSource = new[] { KindHeader, KindSeparator };
            if (DimensionsBox != null) DimensionsBox.SelectedIndex = 0;
            if (DimStyleBox != null)
            {
                DimStyleBox.Items.Add(AutoDimStyle);
                DimStyleBox.SelectedIndex = 0;
            }
            LoadSelectedFrontEditor();
            RenderFrontMatrix();
            RefreshConfigBox();
            UpdateDrawButtons();
            Recompose();
        }

        private void UpdateDrawButtons()
        {
            InsertLateralButton.IsEnabled = canInsertInAutoCad;
            var linked = canInsertInAutoCad && isEditingExisting;
            UpdateButton.IsEnabled = linked;
            InsertExitButton.IsEnabled = linked;
            InsertEntranceButton.IsEnabled = linked;
            InsertPlantaButton.IsEnabled = linked;

            if (!canInsertInAutoCad)
            {
                const string reason = "Disponible solo cuando la ventana se abre desde AutoCAD.";
                UpdateButton.ToolTip = reason;
                InsertLateralButton.ToolTip = reason;
                InsertExitButton.ToolTip = reason;
                InsertEntranceButton.ToolTip = reason;
                InsertPlantaButton.ToolTip = reason;
            }
            else if (!isEditingExisting)
            {
                const string reason = "Primero inserta la vista lateral; después usa RACKEDITAR para agregar las vistas enlazadas.";
                UpdateButton.ToolTip = reason;
                InsertExitButton.ToolTip = reason;
                InsertEntranceButton.ToolTip = reason;
                InsertPlantaButton.ToolTip = reason;
            }
            else
            {
                UpdateButton.ToolTip = "Redibuja en sitio todas las vistas existentes del sistema sin insertar otra.";
                InsertLateralButton.ToolTip = "Pide el número de poste e inserta ese corte lateral enlazado al sistema.";
                InsertExitButton.ToolTip = "Inserta el corte frontal de salida con los largueros IN/OUT en su elevación baja.";
                InsertEntranceButton.ToolTip = "Inserta el corte frontal de entrada con los largueros IN/OUT elevados por la pendiente.";
                InsertPlantaButton.ToolTip = "Inserta la vista planta de la estructura, sin camas.";
            }
        }

        public void SetDimensionStyles(IEnumerable<string> styleNames)
        {
            if (DimStyleBox == null)
            {
                return;
            }

            var selected = SelectedDimensionStyle();
            DimStyleBox.Items.Clear();
            DimStyleBox.Items.Add(AutoDimStyle);
            foreach (var name in (styleNames ?? Enumerable.Empty<string>())
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
            {
                DimStyleBox.Items.Add(name);
            }

            SelectDimensionStyle(selected);
        }

        private string SelectedDimensionStyle()
        {
            var value = DimStyleBox?.SelectedItem as string;
            return string.IsNullOrWhiteSpace(value) || string.Equals(value, AutoDimStyle, StringComparison.Ordinal)
                ? null
                : value;
        }

        private void SelectDimensionStyle(string name)
        {
            if (DimStyleBox == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                foreach (var item in DimStyleBox.Items)
                {
                    if (item is string candidate && string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase))
                    {
                        DimStyleBox.SelectedItem = item;
                        return;
                    }
                }
            }

            DimStyleBox.SelectedIndex = 0;
        }

        private void Annotation_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressRecompose || system == null)
            {
                return;
            }

            if (!TryNum(AnnotationScaleBox?.Text, out var scale) || scale <= 0.0)
            {
                SetStatus("La escala de texto debe ser mayor que cero.", true);
                return;
            }

            Recompose();
        }

        /// <summary>Post-type options (DisplayName shown, Id stored) for the basic "Tipo de poste" combo.</summary>
        private List<CatalogOption> BuildPostOptions() => UiSupport.ToOptions(catalog?.PostProfiles);

        /// <summary>The selected post id, or the catalog default if the combo has no selection yet.</summary>
        private string SelectedPostId() => PostBox?.SelectedValue as string ?? defaultPostCatalogId;

        private double SelectedPostCatalogPeralte()
        {
            var width = catalog?.PostProfiles?.FirstOrDefault(profile => string.Equals(
                profile?.Id,
                SelectedPostId(),
                StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : DynamicRackDefaults.DefaultPostPeralte;
        }

        private void PostBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignore the initial selection set during construction and changes applied while syncing a load.
            if (system == null || suppressRecompose)
            {
                return;
            }

            Recompose();
        }

        private void AdvancedToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (AdvancedPanel != null)
            {
                AdvancedPanel.Visibility = AdvancedToggle.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // ---- Build / edit ----

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Recompose();
        }

        private bool Recompose(bool forceRebuild = false)
        {
            if (!TryReadFrontInputs(out var palletTolerance, out var error))
            {
                SetStatus(error, true);
                return false;
            }

            if (!TryReadInputs(out var pallet, out var palletsDeep, out error))
            {
                SetStatus(error, true);
                return false;
            }

            DynamicDepthLayout depthLayout;
            try
            {
                depthLayout = DynamicDepthGeometry.Resolve(
                    frontRows.Select(row => new DynamicRackFrontDesign
                    {
                        PalletsDeep = row.PalletsDeep,
                        DepthStartPosition = row.DepthStartPosition
                    }),
                    palletsDeep);
                palletsDeep = depthLayout.TotalPositions;
                PalletsDeepBox.Text = palletsDeep.ToString(CultureInfo.InvariantCulture);
            }
            catch (ArgumentException ex)
            {
                SetStatus(ex.Message, true);
                return false;
            }

            if (!TryReadHeightInputs(out var levels, out var firstLevel, out var beamDepth, out error))
            {
                SetStatus(error, true);
                return false;
            }

            if (!TryReadPostPeralte(out var postPeralte, out error))
            {
                SetStatus(error, true);
                return false;
            }

            try
            {
                beamDepth = DynamicLoadBeamGeometry.ResolveBeamDepth(
                    catalog,
                    DynamicRackDefaults.InOutBeamCatalogId,
                    beamDepth);
                BeamDepthBox.Text = Num(beamDepth);
                computedHeaderHeight = ComputeHeaderHeight(pallet, palletsDeep, levels, firstLevel, beamDepth);

                // A full rebuild (BuildDefault, from scratch) is only needed when the pallet or the number of fondos
                // changes — that changes the module SEQUENCE. When only the height inputs change (niveles / peralte /
                // 1er nivel / altura manual, or the post), update the header height IN PLACE so the per-module edits
                // (custom fondo/length, cabeceras) survive instead of reverting to the calculated defaults.
                var mustRebuild = forceRebuild || system == null || system.Modules.Count == 0
                    || !SamePallet(system.Pallet, pallet)
                    || !DynamicDepthGeometry.Matches(system, depthLayout);

                // A pallet/fondos change forces a full rebuild that would discard the custom fondos the user set on
                // headers. Snapshot them (in header order) so they survive — UNLESS this is an explicit "restaurar
                // estandar" (forceRebuild), which is meant to reset everything.
                var savedFondos = mustRebuild && !forceRebuild ? SnapshotHeaderFondos() : null;
                var restoredFondos = 0;

                if (mustRebuild)
                {
                    system = builder.BuildDefault(
                        pallet,
                        depthLayout,
                        RackFrameTemplateCatalog.Default,
                        SelectedPostId(),
                        computedHeaderHeight,
                        postPeralte);
                    restoredFondos = RestoreHeaderFondos(savedFondos, computedHeaderHeight);
                }
                else
                {
                    UpdateHeaderHeightInPlace(computedHeaderHeight);
                }

                builder.ApplyPostPeralte(system, postPeralte);

                ApplySeparatorOverrides();
                ApplyDerivedPostOptions();
                ApplyHeightOverride();
                design = resolver.Snapshot(system, levels, firstLevel, beamDepth, SelectedPostId());
                design.PalletsDeep = depthLayout.TotalPositions;
                design.PostPeralte = postPeralte;
                design.PalletTolerance = palletTolerance;
                design.IntermediateBeamDepths.Clear();
                design.Fronts.Clear();
                foreach (var row in frontRows)
                {
                    EnsureCellCount(row, row.LoadLevels);
                    var frontDesign = new DynamicRackFrontDesign
                    {
                        PalletCount = row.PalletCount,
                        LoadLevels = row.LoadLevels,
                        PalletsDeep = row.PalletsDeep,
                        DepthStartPosition = row.DepthStartPosition,
                        FirstLevelHeight = row.FirstLevelHeight
                    };
                    foreach (var cell in row.Cells.Take(row.LoadLevels))
                    {
                        frontDesign.Levels.Add(cell.ToDesign());
                        frontDesign.IntermediateBeamDepths.Add(cell.IntermediateBeamDepth);
                    }
                    design.Fronts.Add(frontDesign);
                }
                design.NumberFronts = NumberFrontsCheck?.IsChecked == true;
                design.NumberLevels = NumberLevelsCheck?.IsChecked == true;
                design.DrawRackName = DrawRackNameCheck?.IsChecked == true;
                design.AnnotationScale = TryNum(AnnotationScaleBox?.Text, out var annotationScale) && annotationScale > 0.0
                    ? annotationScale
                    : 1.0;
                design.Dimensions = (DimensionDetail)Math.Min(
                    (int)DimensionDetail.Detailed,
                    Math.Max(0, DimensionsBox?.SelectedIndex ?? 0));
                design.DimensionStyle = SelectedDimensionStyle();
                ReplaceSafetySelections(design.SafetySelections, safetySelections);
                var resolution = resolver.Resolve(design);
                system = resolution.System;
                system.Name = NameBox?.Text?.Trim();
                RefreshFrontRows(system.Fronts);
                computedHeaderHeight = system.ManualHeaderHeightOverride ?? resolution.Height.HeaderHeight;
                selectedModule = null;
                BindModules();
                UpdateSelectedPanel();
                UpdateSummary();
                DrawSideView();
                SetStatus(
                    !mustRebuild ? "Altura actualizada; se conservaron los módulos (fondos y cabeceras)."
                    : restoredFondos > 0 ? "Vista recalculada; se conservaron los fondos personalizados de las cabeceras."
                    : "Vista recalculada (layout estándar).", false);
                return true;
            }
            catch (Exception ex)
            {
                system = null;
                ModulesGrid.ItemsSource = null;
                PreviewCanvas.Children.Clear();
                SetStatus("No se pudo generar el sistema: " + ex.Message, true);
                return false;
            }
        }

        private void Safety_Click(object sender, RoutedEventArgs e)
        {
            var elements = (catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>())
                .Where(element => element != null
                                  && (SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.BotaType)
                                      || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.LateralType)
                                      || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DesviadorType)
                                      || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType)
                                      || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.GuiaType)))
                .ToList();
            var levelCount = Math.Max(1, system?.LoadBeamLevels.Count ?? 1);
            var postCount = Math.Max(2, (system?.Fronts.Count ?? frontRows.Count) + 1);
            var levels = system?.Fronts.Count > 0
                ? system.Fronts.Select(front => Math.Max(1, front.LoadLevels)).ToList()
                : frontRows.Select(front => Math.Max(1, front.LoadLevels)).ToList();
            if (levels.Count == 0) levels.Add(levelCount);
            var intro = "Izquierda es la salida y derecha la entrada. La selección se proyecta en lateral, frontal y "
                        + "planta: el protector lateral reemplaza las botas del mismo poste y los desviadores respetan "
                        + "la matriz frente por nivel. La defensa de montacargas permite longitud independiente en "
                        + "salida y entrada por poste. La guía se coloca por frente y nivel únicamente en la entrada. "
                        + "El BOM conserva el conteo físico sin duplicar vistas.";
            var dialog = new SelectiveSafetyWindow(
                elements,
                safetySelections,
                postCount: postCount,
                levelsPerFrente: levels,
                fondoCount: 1,
                catalog: catalog,
                fallbackLevelsArePerPost: true,
                introduction: intro,
                includeDefensa: true,
                includeGuia: true,
                useDynamicSafetyDefaults: true) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            safetySelections.Clear();
            safetySelections.AddRange(dialog.Result.Select(selection => selection.DeepCopy()));
            UpdateSafetyButton();
            Recompose();
        }

        private void PostPeralte_Changed(object sender, RoutedEventArgs e)
        {
            if (system == null || suppressRecompose)
            {
                return;
            }

            Recompose();
        }

        private static bool SafetyDraws(SelectiveSafetySelection selection)
            => selection != null
               && (selection.Quantity > 0
                   || selection.Side != SafetySide.None
                   || selection.PostSides.Any(post => post != null && post.Side != SafetySide.None));

        private void UpdateSafetyButton()
        {
            var count = safetySelections.Count(SafetyDraws);
            SafetyButton.Content = count > 0
                ? "Elementos de seguridad (" + count.ToString(CultureInfo.InvariantCulture) + ")…"
                : "Elementos de seguridad…";
        }

        private static void ReplaceSafetySelections(
            ICollection<SelectiveSafetySelection> target,
            IEnumerable<SelectiveSafetySelection> source)
        {
            target.Clear();
            foreach (var safety in source ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (SafetyDraws(safety) && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    target.Add(safety.DeepCopy());
                }
            }
        }

        /// <summary>
        /// Update calculated cabeceras on the EXISTING modules. Custom cabeceras remain untouched, matching the
        /// selective editor's contract: calculated inputs may regenerate defaults but never overwrite explicit edits.
        /// </summary>
        private void UpdateHeaderHeightInPlace(double newHeight)
        {
            var factory = new RackFrameConfigurationFactory(catalog);
            var postId = SelectedPostId();

            foreach (var module in system.Modules)
            {
                if (!module.IsHeader || !module.UseCalculatedHeaderConfiguration)
                {
                    continue;
                }

                var fondo = module.Length > 0.0 ? module.Length : system.DefaultHeaderLength;
                module.AssociatedFrameConfiguration = factory.Build(RackFrameTemplateCatalog.Default, postId, newHeight, fondo);
            }

            builder.Refresh(system);
        }

        /// <summary>Snapshot the custom fondo of each header module, in header order (null = that header used the default),
        /// so a full rebuild (pallet/fondos change) can restore the user's per-header fondos afterwards.</summary>
        private List<double?> SnapshotHeaderFondos()
        {
            var fondos = new List<double?>();
            if (system == null)
            {
                return fondos;
            }

            foreach (var module in system.Modules.Where(m => m.IsHeader))
            {
                fondos.Add(module.IsManualOverride && module.Length > 0.0 ? module.Length : (double?)null);
            }

            return fondos;
        }

        /// <summary>Re-apply the snapshot fondos to the freshly-rebuilt header modules by header order (only where the
        /// header still exists), rebuilding each restored cabecera at the NEW height. Returns how many were restored.
        /// Deep structural cabecera edits are not preserved (they are rebuilt to the standard for the new mesh).</summary>
        private int RestoreHeaderFondos(List<double?> savedFondos, double newHeight)
        {
            if (savedFondos == null || savedFondos.Count == 0 || system == null)
            {
                return 0;
            }

            var factory = new RackFrameConfigurationFactory(catalog);
            var postId = SelectedPostId();
            var ordinal = 0;
            var restored = 0;

            foreach (var module in system.Modules.Where(m => m.IsHeader))
            {
                if (ordinal < savedFondos.Count && savedFondos[ordinal].HasValue)
                {
                    var fondo = savedFondos[ordinal].Value;
                    module.Length = fondo;
                    module.IsManualOverride = true;
                    module.IsCalculated = false;
                    module.UseCalculatedHeaderConfiguration = true;
                    module.AssociatedFrameConfiguration = factory.Build(RackFrameTemplateCatalog.Default, postId, newHeight, fondo);
                    restored++;
                }

                ordinal++;
            }

            if (restored > 0)
            {
                system.RecalculatePositions();
                builder.Refresh(system);
            }

            return restored;
        }

        private static bool SamePallet(PalletSpecification a, PalletSpecification b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return Math.Abs(a.Front - b.Front) < 1e-6
                && Math.Abs(a.Depth - b.Depth) < 1e-6
                && Math.Abs(a.Height - b.Height) < 1e-6
                && Math.Abs(a.Weight - b.Weight) < 1e-6;
        }

        private void ModulesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedModule = ModulesGrid.SelectedItem as DynamicRackModule;
            UpdateSelectedPanel();
            DrawSideView();
        }

        private void ApplyModule_Click(object sender, RoutedEventArgs e)
        {
            if (selectedModule == null || system == null)
            {
                return;
            }

            if (!TryNum(ModuleLengthBox.Text, out var length) || length <= 0)
            {
                SetStatus("Longitud inválida (debe ser mayor que cero).", true);
                return;
            }

            var label = KindBox.SelectedItem as string ?? (selectedModule.IsHeader ? KindHeader : KindSeparator);
            var wantsHeader = label == KindHeader;

            selectedModule.Length = length;
            selectedModule.IsManualOverride = true;
            selectedModule.IsCalculated = false;

            if (wantsHeader)
            {
                var index = system.Modules.IndexOf(selectedModule);
                selectedModule.Kind = index == 0
                    ? DynamicRackModuleKind.HeaderStart
                    : index == system.Modules.Count - 1
                        ? DynamicRackModuleKind.HeaderEnd
                        : DynamicRackModuleKind.HeaderIntermediate;

                if (selectedModule.AssociatedFrameConfiguration == null)
                {
                    selectedModule.AssociatedFrameConfiguration = BuildHeaderConfig(Math.Max(length, 1.0));
                    selectedModule.UseCalculatedHeaderConfiguration = true;
                }
            }
            else
            {
                selectedModule.Kind = DynamicRackModuleKind.Separator;
                selectedModule.AssociatedFrameConfiguration = null;
                selectedModule.UseCalculatedHeaderConfiguration = true;
            }

            builder.Refresh(system);
            BindModules();
            UpdateSelectedPanel();
            UpdateSummary();
            DrawSideView();
            SetStatus("Módulo actualizado.", false);
        }

        private void EditHeader_Click(object sender, RoutedEventArgs e)
        {
            if (selectedModule == null || !selectedModule.IsHeader)
            {
                SetStatus("Selecciona un módulo de cabecera para editarlo.", true);
                return;
            }

            if (selectedModule.AssociatedFrameConfiguration == null)
            {
                selectedModule.AssociatedFrameConfiguration = BuildHeaderConfig(Math.Max(selectedModule.Length, 1.0));
            }

            // Snapshot before the dialog: closing it without editing must NOT accumulate a duplicate
            // "Personalizada N" preset on every open.
            var beforeEdit = new RackProjectStore().Serialize(RackProject.ForSelective(selectedModule.AssociatedFrameConfiguration));

            var window = new RackFrameConfiguratorWindow(selectedModule.AssociatedFrameConfiguration) { Owner = this };
            window.ShowDialog();

            // The dynamic editor owns one rack-wide post PERALTE. The individual header editor may display it, but
            // cannot create a conflicting value for just one module.
            if (TryNum(PostPeralteBox?.Text, out var globalPostPeralte) && globalPostPeralte > 0.0)
            {
                selectedModule.AssociatedFrameConfiguration.PostPeralte = globalPostPeralte;
            }

            var afterEdit = new RackProjectStore().Serialize(RackProject.ForSelective(selectedModule.AssociatedFrameConfiguration));
            if (afterEdit == beforeEdit)
            {
                SetStatus("Cabecera sin cambios.", false);
                return;
            }

            selectedModule.UseCalculatedHeaderConfiguration = false;

            // The header's depth (fondo) edited in the configurator becomes the module length.
            var editedDepth = selectedModule.AssociatedFrameConfiguration.Depth;
            if (editedDepth > 0 && Math.Abs(editedDepth - selectedModule.Length) > 0.0001)
            {
                selectedModule.Length = editedDepth;
                selectedModule.IsManualOverride = true;
                selectedModule.IsCalculated = false;
            }

            // Save the edited header as a reusable preset ("Personalizada N") for the configuration dropdown.
            headerPresets.Add(new HeaderPreset(
                "Personalizada " + (headerPresets.Count + 1).ToString(CultureInfo.InvariantCulture),
                Clone(selectedModule.AssociatedFrameConfiguration)));
            RefreshConfigBox();

            builder.Refresh(system);
            BindModules();
            UpdateSelectedPanel();
            UpdateSummary();
            DrawSideView();
            SetStatus("Cabecera del módulo actualizada (fondo " + editedDepth.ToString("0.##", CultureInfo.InvariantCulture) + " in).", false);
        }

        private void RestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            // Explicit "restore standard": a full rebuild that DOES discard the per-module overrides.
            Recompose(forceRebuild: true);
        }

        private RackFrameConfiguration BuildHeaderConfig(double depth)
        {
            var postPeralte = TryNum(PostPeralteBox?.Text, out var value) && value > 0.0
                ? value
                : SelectedPostCatalogPeralte();
            return builder.BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default,
                SelectedPostId(),
                computedHeaderHeight,
                depth,
                postPeralte);
        }

        /// <summary>
        /// Computes the header height from the load inputs (DynamicHeaderHeightCalculator) and shows it.
        /// Load height = the pallet height; total depth (the slope run) = the full run length we already
        /// derive (tarimas x fondo + 12"). Levels/first-level/beam-depth come from the new fields.
        /// </summary>
        private double ComputeHeaderHeight(
            PalletSpecification pallet,
            int palletsDeep,
            int levels,
            double firstLevel,
            double beamDepth)
        {
            var totalDepth = palletsDeep * pallet.Depth + 2.0 * DynamicRackDefaults.HeaderEndAllowance;

            var result = DynamicHeaderHeightCalculator.Calculate(pallet.Height, levels, firstLevel, beamDepth, totalDepth);

            // Manual override wins over the derived height, but we still show what the levels would give.
            var manual = ManualHeightToggle?.IsChecked == true && TryNum(ManualHeightBox.Text, out var m) && m > 0.0
                ? m
                : (double?)null;

            ComputedHeightText.Text = manual.HasValue
                ? string.Format(CultureInfo.InvariantCulture, "Altura manual: {0:0.#}\"  (derivada sería {1:0}\")", manual.Value, result.HeaderHeight)
                : string.Format(CultureInfo.InvariantCulture, "Altura calculada: {0:0.#}\" → {1:0}\"  (pendiente {2:0.#}\")", result.TheoreticalHeight, result.HeaderHeight, result.Slope);

            return manual ?? result.HeaderHeight;
        }

        private bool TryReadHeightInputs(out int levels, out double firstLevel, out double beamDepth, out string error)
        {
            levels = 0;
            firstLevel = 0.0;
            beamDepth = 0.0;
            error = null;

            levels = frontRows.Count > 0
                ? frontRows.Max(front => Math.Max(1, front.LoadLevels))
                : DynamicRackDefaults.DefaultLoadLevels;
            if (LoadLevelsBox != null)
            {
                LoadLevelsBox.Text = levels.ToString(CultureInfo.InvariantCulture);
            }

            var firstRow = frontRows.FirstOrDefault();
            if (firstRow == null)
            {
                error = "Se requiere al menos un frente.";
                return false;
            }
            EnsureCellCount(firstRow, firstRow.LoadLevels);
            firstLevel = firstRow.FirstLevelHeight;
            beamDepth = firstRow.Cells[0].InOutBeamDepth;
            BeamDepthBox.Text = Num(beamDepth);

            return true;
        }

        private bool TryReadPostPeralte(out double postPeralte, out string error)
        {
            error = null;
            if (!TryNum(PostPeralteBox?.Text, out postPeralte) || postPeralte <= 0.0)
            {
                error = "El peralte de poste debe ser un número mayor que cero.";
                return false;
            }

            return true;
        }

        private static bool TryOptionalNonNegative(string text, out double value)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                value = 0.0;
                return true;
            }

            return TryNum(text, out value) && value >= 0.0;
        }

        /// <summary>Read the manual-height toggle/box onto the system (null = derived from levels). Non-destructive.</summary>
        private void ApplyHeightOverride()
        {
            if (system == null)
            {
                return;
            }

            system.ManualHeaderHeightOverride =
                ManualHeightToggle?.IsChecked == true && TryNum(ManualHeightBox.Text, out var h) && h > 0.0 ? h : (double?)null;
        }

        private void ManualHeight_Changed(object sender, RoutedEventArgs e)
        {
            // Changing the header height requires rebuilding the headers at the new height.
            if (system == null || suppressRecompose)
            {
                return;
            }

            Recompose();
        }

        /// <summary>Flush the advanced separator/derived-post override boxes onto the model WITHOUT a destructive rebuild.</summary>
        private void AdvancedOverride_LostFocus(object sender, RoutedEventArgs e)
        {
            if (system == null || suppressRecompose)
            {
                return;
            }

            ApplySeparatorOverrides();
            ApplyDerivedPostOptions();
            UpdateSummary();
            DrawSideView();
        }

        private bool TryReadInputs(out PalletSpecification pallet, out int palletsDeep, out string error)
        {
            pallet = null;
            palletsDeep = 0;
            error = null;

            if (!TryNum(DepthBox.Text, out var depth) || depth <= 0) { error = "Fondo inválido."; return false; }
            if (!UiSupport.TryInt(PalletsDeepBox.Text, out palletsDeep) || palletsDeep < 2)
            {
                error = "Las tarimas de fondo deben ser un entero >= 2.";
                return false;
            }

            var firstRow = frontRows.FirstOrDefault();
            if (firstRow == null)
            {
                error = "Se requiere al menos un frente.";
                return false;
            }
            EnsureCellCount(firstRow, firstRow.LoadLevels);
            var firstCell = firstRow.Cells[0];
            pallet = new PalletSpecification(
                firstCell.PalletFront,
                depth,
                firstCell.PalletHeight,
                firstCell.PalletWeight,
                "kg");
            return true;
        }

        private bool TryReadFrontInputs(out double palletTolerance, out string error)
        {
            // Kept on the persisted model only for legacy compatibility. The current physical rule is fixed:
            // BFR = pallet front + 2; IN/OUT = BFR x positions + 6.
            palletTolerance = DynamicRackDefaults.DefaultPalletTolerance;
            error = null;

            if (!CommitSelectedFrontEditor(out error))
            {
                return false;
            }

            if (frontRows.Count == 0)
            {
                error = "Se requiere al menos un frente.";
                return false;
            }

            foreach (var row in frontRows)
            {
                if (row.PalletCount < 1)
                {
                    error = "Cada frente requiere al menos una posición de tarima.";
                    return false;
                }

                if (row.LoadLevels < 1)
                {
                    error = "Cada frente requiere al menos un nivel de carga.";
                    return false;
                }

                if (row.PalletsDeep < 2 || row.DepthStartPosition < 1)
                {
                    error = "Cada frente requiere al menos 2 fondos y una posición inicial >= 1.";
                    return false;
                }

                if (row.FirstLevelHeight < 0.0)
                {
                    error = "El inicio del primer larguero debe ser mayor o igual que cero.";
                    return false;
                }

                EnsureCellCount(row, row.LoadLevels);
                if (row.Cells.Take(row.LoadLevels).Any(cell => !cell.IsValid))
                {
                    error = "Revisa los valores de tarima, claro y largueros de cada celda.";
                    return false;
                }
            }

            return true;
        }

        private void FrontCount_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFrontCount();
        }

        private void FrontCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            ApplyFrontCount();
            e.Handled = true;
        }

        private void ApplyFrontCount()
        {
            if (suppressRecompose || FrontCountBox == null)
            {
                return;
            }

            if (!UiSupport.TryInt(FrontCountBox.Text, out var requestedCount) || requestedCount < 1)
            {
                FrontCountBox.Text = frontRows.Count.ToString(CultureInfo.InvariantCulture);
                SetStatus("La cantidad de frentes debe ser un entero >= 1.", true);
                return;
            }

            if (!CommitSelectedFrontEditor(out var error))
            {
                FrontCountBox.Text = frontRows.Count.ToString(CultureInfo.InvariantCulture);
                SetStatus(error, true);
                return;
            }

            if (requestedCount == frontRows.Count)
            {
                FrontCountBox.Text = requestedCount.ToString(CultureInfo.InvariantCulture);
                return;
            }

            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, frontRows.Count - 1));
            var template = frontRows.Count > 0 ? frontRows[selectedFrontIndex] : null;
            while (frontRows.Count < requestedCount)
            {
                var newFront = new DynamicFrontRow
                {
                    Index = frontRows.Count + 1,
                    PalletCount = template?.PalletCount ?? DynamicRackDefaults.DefaultPalletsWide,
                    LoadLevels = template?.LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels,
                    PalletsDeep = template?.PalletsDeep ?? DynamicRackDefaults.DefaultPalletsDeep,
                    DepthStartPosition = template?.DepthStartPosition ?? 1,
                    FirstLevelHeight = template?.FirstLevelHeight ?? DynamicRackDefaults.DefaultFirstLevelHeight
                };
                foreach (var cell in template?.Cells ?? Enumerable.Empty<DynamicCellRow>())
                {
                    newFront.Cells.Add(cell.Clone());
                }
                EnsureCellCount(newFront, newFront.LoadLevels);
                frontRows.Add(newFront);
            }

            if (frontRows.Count > requestedCount)
            {
                frontRows.RemoveRange(requestedCount, frontRows.Count - requestedCount);
            }

            for (var index = 0; index < frontRows.Count; index++)
            {
                frontRows[index].Index = index + 1;
            }

            selectedFrontIndex = Math.Min(selectedFrontIndex, frontRows.Count - 1);
            FrontCountBox.Text = frontRows.Count.ToString(CultureInfo.InvariantCulture);
            LoadSelectedFrontEditor();
            RenderFrontMatrix();
            Recompose();
        }

        private void RefreshFrontRows(IList<DynamicRackFront> resolved)
        {
            if (resolved == null || resolved.Count != frontRows.Count)
            {
                return;
            }

            for (var index = 0; index < resolved.Count; index++)
            {
                frontRows[index].Index = index + 1;
                frontRows[index].Bfr = resolved[index].Bfr;
                frontRows[index].BeamLength = resolved[index].BeamLength;
                frontRows[index].LoadLevels = Math.Max(1, resolved[index].LoadLevels);
                frontRows[index].PalletsDeep = Math.Max(2, resolved[index].PalletsDeep);
                frontRows[index].DepthStartPosition = Math.Max(1, resolved[index].DepthStartPosition);
                frontRows[index].FirstLevelHeight = resolved[index].FirstLevelHeight;
                frontRows[index].Cells.Clear();
                foreach (var level in resolved[index].Levels)
                {
                    frontRows[index].Cells.Add(DynamicCellRow.From(level));
                }
                EnsureCellCount(frontRows[index], frontRows[index].LoadLevels);
            }

            LoadSelectedFrontEditor();
            RenderFrontMatrix();
        }

        private void RestoreFrontRows(IEnumerable<DynamicRackFront> resolved)
        {
            frontRows.Clear();
            foreach (var front in resolved ?? Enumerable.Empty<DynamicRackFront>())
            {
                var row = new DynamicFrontRow
                {
                    Index = frontRows.Count + 1,
                    PalletCount = Math.Max(1, front.PalletCount),
                    LoadLevels = Math.Max(1, front.LoadLevels),
                    PalletsDeep = Math.Max(2, front.PalletsDeep),
                    DepthStartPosition = Math.Max(1, front.DepthStartPosition),
                    FirstLevelHeight = front.FirstLevelHeight,
                    Bfr = front.Bfr,
                    BeamLength = front.BeamLength
                };
                foreach (var level in front.Levels)
                {
                    row.Cells.Add(DynamicCellRow.From(level));
                }
                EnsureCellCount(row, row.LoadLevels);
                frontRows.Add(row);
            }

            if (frontRows.Count == 0)
            {
                frontRows.Add(new DynamicFrontRow
                {
                    Index = 1,
                    PalletCount = DynamicRackDefaults.DefaultPalletsWide,
                    LoadLevels = DynamicRackDefaults.DefaultLoadLevels,
                    PalletsDeep = DynamicRackDefaults.DefaultPalletsDeep,
                    DepthStartPosition = 1
                });
            }

            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, frontRows.Count - 1));
            if (FrontCountBox != null)
            {
                FrontCountBox.Text = frontRows.Count.ToString(CultureInfo.InvariantCulture);
            }
            LoadSelectedFrontEditor();
            RenderFrontMatrix();
        }

        private bool CommitSelectedFrontEditor(out string error)
        {
            if (!TryReadSelectedEditor(out var values, out error))
            {
                return false;
            }

            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, frontRows.Count - 1));
            selectedLevelIndex = Math.Max(0, Math.Min(selectedLevelIndex, values.LoadLevels - 1));
            var row = frontRows[selectedFrontIndex];
            ApplyFrontValues(row, values);
            EnsureCellCount(row, row.LoadLevels);
            ApplyCellValues(row.Cells[selectedLevelIndex], values);
            UpdateMaximumLevelText();
            return true;
        }

        private bool TryReadSelectedEditor(out DynamicEditorValues values, out string error)
        {
            values = null;
            error = null;
            if (frontRows.Count == 0 || SelectedPositionsBox == null || SelectedLevelsBox == null
                || SelectedPalletsDeepBox == null || SelectedDepthStartBox == null || SelectedBeamLengthBox == null)
            {
                return true;
            }

            if (!UiSupport.TryInt(SelectedPositionsBox.Text, out var positions) || positions < 1)
            {
                error = "Las posiciones del frente deben ser un entero >= 1.";
                return false;
            }

            if (!UiSupport.TryInt(SelectedLevelsBox.Text, out var levels) || levels < 1)
            {
                error = "Los niveles del frente deben ser un entero >= 1.";
                return false;
            }

            if (!UiSupport.TryOptionalNum(SelectedBeamLengthBox.Text, out var manual)
                || manual.HasValue && manual.Value <= 0.0)
            {
                error = "El largo manual debe ser un número mayor que cero o quedar vacío.";
                return false;
            }

            if (!UiSupport.TryInt(SelectedPalletsDeepBox.Text, out var palletsDeep) || palletsDeep < 2)
            {
                error = "Los fondos del frente deben ser un entero >= 2.";
                return false;
            }

            if (!UiSupport.TryInt(SelectedDepthStartBox.Text, out var depthStart) || depthStart < 1)
            {
                error = "La posición inicial de fondo debe ser un entero >= 1.";
                return false;
            }

            if (!TryNum(FirstLevelHeightBox?.Text, out var firstLevelHeight) || firstLevelHeight < 0.0)
            {
                error = "El inicio del primer larguero debe ser un número >= 0.";
                return false;
            }

            if (!TryNum(FrontBox?.Text, out var palletFront) || palletFront <= 0.0)
            {
                error = "El frente de tarima debe ser mayor que cero.";
                return false;
            }
            if (!TryNum(PalletHeightBox?.Text, out var palletHeight) || palletHeight <= 0.0)
            {
                error = "El alto de tarima debe ser mayor que cero.";
                return false;
            }
            if (!TryNum(WeightBox?.Text, out var palletWeight) || palletWeight < 0.0)
            {
                error = "El peso de tarima debe ser mayor o igual que cero.";
                return false;
            }
            if (!TryNum(SelectedClearHeightBox?.Text, out var clearHeight) || clearHeight < 0.0)
            {
                error = "El claro libre debe ser mayor o igual que cero.";
                return false;
            }
            if (!(SelectedInOutPeralteBox?.SelectedItem is double inOutBeamDepth) || inOutBeamDepth <= 0.0)
            {
                error = "Selecciona un peralte de larguero IN/OUT.";
                return false;
            }
            if (!(SelectedIntermediatePeralteBox?.SelectedItem is double intermediateBeamDepth)
                || intermediateBeamDepth <= 0.0)
            {
                error = "Selecciona un peralte de larguero intermedio.";
                return false;
            }

            values = new DynamicEditorValues
            {
                PalletCount = positions,
                LoadLevels = levels,
                PalletsDeep = palletsDeep,
                DepthStartPosition = depthStart,
                BeamLengthOverride = manual,
                FirstLevelHeight = firstLevelHeight,
                PalletFront = palletFront,
                PalletHeight = palletHeight,
                PalletWeight = palletWeight,
                ClearHeight = clearHeight,
                InOutBeamCatalogId = SelectedInOutBeamBox?.SelectedValue as string
                                     ?? DynamicRackDefaults.InOutBeamCatalogId,
                InOutBeamDepth = inOutBeamDepth,
                IntermediateBeamCatalogId = SelectedIntermediateBeamBox?.SelectedValue as string
                                             ?? DynamicRackDefaults.IntermediateBeamCatalogId,
                IntermediateBeamDepth = intermediateBeamDepth
            };
            return true;
        }

        private static void ApplyFrontValues(DynamicFrontRow row, DynamicEditorValues values)
        {
            row.PalletCount = values.PalletCount;
            row.LoadLevels = values.LoadLevels;
            row.PalletsDeep = values.PalletsDeep;
            row.DepthStartPosition = values.DepthStartPosition;
            row.FirstLevelHeight = values.FirstLevelHeight;
        }

        private static void ApplyCellValues(DynamicCellRow cell, DynamicEditorValues values)
        {
            cell.PalletFront = values.PalletFront;
            cell.PalletHeight = values.PalletHeight;
            cell.PalletWeight = values.PalletWeight;
            cell.ClearHeight = values.ClearHeight;
            cell.InOutBeamCatalogId = values.InOutBeamCatalogId;
            cell.InOutBeamDepth = values.InOutBeamDepth;
            cell.BeamLengthOverride = values.BeamLengthOverride;
            cell.IntermediateBeamCatalogId = values.IntermediateBeamCatalogId;
            cell.IntermediateBeamDepth = values.IntermediateBeamDepth;
        }

        private void UpdateMaximumLevelText()
        {
            if (LoadLevelsBox != null && frontRows.Count > 0)
            {
                LoadLevelsBox.Text = frontRows.Max(front => Math.Max(1, front.LoadLevels))
                    .ToString(CultureInfo.InvariantCulture);
            }
        }

        private void LoadSelectedFrontEditor()
        {
            if (frontRows.Count == 0 || SelectedPositionsBox == null)
            {
                return;
            }

            NormalizeSelectedCells();
            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, frontRows.Count - 1));
            var row = frontRows[selectedFrontIndex];
            selectedLevelIndex = Math.Max(0, Math.Min(selectedLevelIndex, Math.Max(1, row.LoadLevels) - 1));
            EnsureCellCount(row, row.LoadLevels);
            var cell = row.Cells[selectedLevelIndex];
            SelectedFrontText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Celda: Frente {0} · Nivel {1}{2}",
                selectedFrontIndex + 1,
                selectedLevelIndex + 1,
                selectedCells.Count > 1
                    ? " · " + selectedCells.Count.ToString(CultureInfo.InvariantCulture) + " seleccionadas"
                    : string.Empty);
            SelectedPositionsBox.Text = row.PalletCount.ToString(CultureInfo.InvariantCulture);
            SelectedLevelsBox.Text = Math.Max(1, row.LoadLevels).ToString(CultureInfo.InvariantCulture);
            SelectedPalletsDeepBox.Text = Math.Max(2, row.PalletsDeep).ToString(CultureInfo.InvariantCulture);
            SelectedDepthStartBox.Text = Math.Max(1, row.DepthStartPosition).ToString(CultureInfo.InvariantCulture);
            FirstLevelHeightBox.Text = Num(row.FirstLevelHeight);
            FrontBox.Text = Num(cell.PalletFront);
            PalletHeightBox.Text = Num(cell.PalletHeight);
            WeightBox.Text = Num(cell.PalletWeight);
            SelectedClearHeightBox.Text = Num(cell.ClearHeight);
            SelectedBeamLengthBox.Text = cell.BeamLengthOverride.HasValue ? Num(cell.BeamLengthOverride.Value) : string.Empty;
            SelectedInOutBeamBox.ItemsSource = InOutBeamOptions();
            SelectedInOutBeamBox.SelectedValue = cell.InOutBeamCatalogId;
            SetPeralteOptions(SelectedInOutPeralteBox, cell.InOutBeamCatalogId, cell.InOutBeamDepth);
            SelectedIntermediateBeamBox.ItemsSource = IntermediateBeamOptions();
            SelectedIntermediateBeamBox.SelectedValue = cell.IntermediateBeamCatalogId;
            SetPeralteOptions(SelectedIntermediatePeralteBox, cell.IntermediateBeamCatalogId, cell.IntermediateBeamDepth);
            var bfr = DynamicFrontGeometry.Bfr(cell.PalletFront);
            var requestedBeam = cell.BeamLengthOverride ?? DynamicFrontGeometry.AutoBeamLength(
                cell.PalletFront, row.PalletCount, DynamicRackDefaults.DefaultPalletTolerance);
            var beam = row.BeamLength > 0.0 ? row.BeamLength : requestedBeam;
            SelectedFrontInfo.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Fondos {0}–{1} · BFR {2:0.##}\" · Solicitud nivel {3:0.##}\" · Largo físico frente {4:0.##}\"",
                row.DepthStartPosition,
                row.DepthStartPosition + row.PalletsDeep - 1,
                bfr,
                requestedBeam,
                beam);
        }

        private void NormalizeSelectedCells()
        {
            if (frontRows.Count == 0)
            {
                selectedCells.Clear();
                return;
            }

            selectedFrontIndex = Math.Max(0, Math.Min(selectedFrontIndex, frontRows.Count - 1));
            selectedLevelIndex = Math.Max(0, Math.Min(
                selectedLevelIndex,
                Math.Max(1, frontRows[selectedFrontIndex].LoadLevels) - 1));
            selectedCells.RemoveWhere(cell => cell.FrontIndex < 0
                                              || cell.FrontIndex >= frontRows.Count
                                              || cell.LevelIndex < 0
                                              || cell.LevelIndex >= Math.Max(1, frontRows[cell.FrontIndex].LoadLevels));
            var primary = (FrontIndex: selectedFrontIndex, LevelIndex: selectedLevelIndex);
            if (!selectedCells.Contains(primary))
            {
                if (selectedCells.Count > 0)
                {
                    var next = selectedCells.First();
                    selectedFrontIndex = next.FrontIndex;
                    selectedLevelIndex = next.LevelIndex;
                }
                else
                {
                    selectedCells.Add(primary);
                }
            }
        }

        private void SelectedFront_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressRecompose || system == null)
            {
                return;
            }

            if (!CommitSelectedFrontEditor(out var error))
            {
                SetStatus(error, true);
                return;
            }

            Recompose();
        }

        private void ApplyCell_Click(object sender, RoutedEventArgs e) => ApplyEditorScope(DynamicRackCellScope.Cell);

        private void ApplySelectedCells_Click(object sender, RoutedEventArgs e) => ApplyEditorScope(DynamicRackCellScope.Selected);

        private void ApplyLevel_Click(object sender, RoutedEventArgs e) => ApplyEditorScope(DynamicRackCellScope.Level);

        private void ApplyFront_Click(object sender, RoutedEventArgs e) => ApplyEditorScope(DynamicRackCellScope.Front);

        private void ApplyAll_Click(object sender, RoutedEventArgs e) => ApplyEditorScope(DynamicRackCellScope.All);

        private void ApplyFrontData_Click(object sender, RoutedEventArgs e)
            => ApplyFrontDataTo(new[] { selectedFrontIndex }, "este frente");

        private void ApplySelectedFrontData_Click(object sender, RoutedEventArgs e)
            => ApplyFrontDataTo(
                selectedCells.Select(cell => cell.FrontIndex),
                "los frentes seleccionados");

        private void ApplyAllFrontData_Click(object sender, RoutedEventArgs e)
            => ApplyFrontDataTo(Enumerable.Range(0, frontRows.Count), "todos los frentes");

        private void ApplyFrontDataTo(IEnumerable<int> requestedTargets, string description)
        {
            if (!TryReadSelectedEditor(out var values, out var error))
            {
                SetStatus(error, true);
                return;
            }

            var targets = (requestedTargets ?? Enumerable.Empty<int>())
                .Where(index => index >= 0 && index < frontRows.Count)
                .Distinct()
                .ToList();
            if (targets.Count == 0)
            {
                SetStatus("Selecciona al menos una celda para identificar sus frentes.", true);
                return;
            }

            var saved = frontRows.Select(CloneFrontRow).ToList();
            foreach (var target in targets)
            {
                ApplyFrontValues(frontRows[target], values);
                EnsureCellCount(frontRows[target], frontRows[target].LoadLevels);
            }
            NormalizeSelectedCells();
            if (!Recompose())
            {
                frontRows.Clear();
                frontRows.AddRange(saved);
                NormalizeSelectedCells();
                LoadSelectedFrontEditor();
                RenderFrontMatrix();
                return;
            }

            SetStatus(string.Format(
                CultureInfo.InvariantCulture,
                "Datos estructurales aplicados a {0} ({1} frente(s)).",
                description,
                targets.Count), false);
        }

        private void ApplyEditorScope(DynamicRackCellScope scope)
        {
            if (suppressRecompose || system == null)
            {
                return;
            }

            if (!TryReadSelectedEditor(out var values, out var error))
            {
                SetStatus(error, true);
                return;
            }

            var saved = frontRows.Select(CloneFrontRow).ToList();
            var sourceIndex = Math.Max(0, Math.Min(selectedFrontIndex, frontRows.Count - 1));
            var levelIndex = Math.Max(0, Math.Min(selectedLevelIndex, values.LoadLevels - 1));
            ApplyFrontValues(frontRows[sourceIndex], values);
            EnsureCellCount(frontRows[sourceIndex], frontRows[sourceIndex].LoadLevels);

            var targets = DynamicRackCellScopeResolver.Targets(
                frontRows.Select(row => row.LoadLevels).ToList(),
                sourceIndex,
                levelIndex,
                scope,
                selectedCells.Select(cell => new DynamicRackCellAddress(cell.FrontIndex, cell.LevelIndex)));
            foreach (var target in targets)
            {
                var row = frontRows[target.FrontIndex];
                EnsureCellCount(row, row.LoadLevels);
                ApplyCellValues(row.Cells[target.LevelIndex], values);
            }

            selectedFrontIndex = sourceIndex;
            selectedLevelIndex = levelIndex;
            UpdateMaximumLevelText();
            if (!Recompose())
            {
                frontRows.Clear();
                frontRows.AddRange(saved);
                LoadSelectedFrontEditor();
                RenderFrontMatrix();
                return;
            }

            SetStatus(string.Format(
                CultureInfo.InvariantCulture,
                "Datos aplicados a {0} celda(s).",
                targets.Count), false);
        }

        private static DynamicFrontRow CloneFrontRow(DynamicFrontRow source)
        {
            var clone = new DynamicFrontRow
            {
                Index = source.Index,
                PalletCount = source.PalletCount,
                LoadLevels = source.LoadLevels,
                PalletsDeep = source.PalletsDeep,
                DepthStartPosition = source.DepthStartPosition,
                FirstLevelHeight = source.FirstLevelHeight,
                Bfr = source.Bfr,
                BeamLength = source.BeamLength
            };
            foreach (var cell in source.Cells)
            {
                clone.Cells.Add(cell.Clone());
            }
            return clone;
        }

        private void RenderFrontMatrix()
        {
            if (DynamicMatrixGrid == null)
            {
                return;
            }

            DynamicMatrixGrid.Children.Clear();
            DynamicMatrixGrid.RowDefinitions.Clear();
            DynamicMatrixGrid.ColumnDefinitions.Clear();
            DynamicMatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64.0) });
            foreach (var _ in frontRows)
            {
                DynamicMatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(118.0) });
            }

            var levels = frontRows.Count > 0
                ? frontRows.Max(front => Math.Max(1, front.LoadLevels))
                : DynamicRackDefaults.DefaultLoadLevels;
            DynamicMatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var level = 0; level < levels; level++)
            {
                DynamicMatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            AddMatrixElement(new TextBlock
            {
                Text = "",
                Margin = new Thickness(2.0)
            }, 0, 0);

            for (var frontIndex = 0; frontIndex < frontRows.Count; frontIndex++)
            {
                var captured = frontIndex;
                var row = frontRows[frontIndex];
                var header = new StackPanel { Margin = new Thickness(3.0, 0.0, 3.0, 5.0) };
                header.Children.Add(new TextBlock
                {
                    Text = "Frente " + (frontIndex + 1).ToString(CultureInfo.InvariantCulture),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = frontIndex == selectedFrontIndex ? SelectionStroke : LabelStroke,
                    FontWeight = FontWeights.SemiBold
                });
                header.Children.Add(new TextBlock
                {
                    Text = "Posiciones",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = LabelStroke,
                    FontSize = 9.5
                });
                var controls = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                var minus = new Button { Content = "−", Width = 24.0, Height = 22.0, Padding = new Thickness(0.0), Margin = new Thickness(0.0, 2.0, 5.0, 2.0) };
                minus.Click += (_, __) => AdjustFrontPositions(captured, -1);
                controls.Children.Add(minus);
                controls.Children.Add(new TextBlock
                {
                    Text = row.PalletCount.ToString(CultureInfo.InvariantCulture),
                    Width = 24.0,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = LabelStroke
                });
                var plus = new Button { Content = "+", Width = 24.0, Height = 22.0, Padding = new Thickness(0.0), Margin = new Thickness(5.0, 2.0, 0.0, 2.0) };
                plus.Click += (_, __) => AdjustFrontPositions(captured, 1);
                controls.Children.Add(plus);
                header.Children.Add(controls);
                header.Children.Add(new TextBlock
                {
                    Text = "Niveles",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = LabelStroke,
                    FontSize = 9.5
                });
                var levelControls = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                var lessLevel = new Button { Content = "−", Width = 24.0, Height = 22.0, Padding = new Thickness(0.0), Margin = new Thickness(0.0, 2.0, 5.0, 2.0) };
                lessLevel.Click += (_, __) => AdjustFrontLevels(captured, -1);
                levelControls.Children.Add(lessLevel);
                levelControls.Children.Add(new TextBlock
                {
                    Text = row.LoadLevels.ToString(CultureInfo.InvariantCulture),
                    Width = 24.0,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = LabelStroke
                });
                var moreLevel = new Button { Content = "+", Width = 24.0, Height = 22.0, Padding = new Thickness(0.0), Margin = new Thickness(5.0, 2.0, 0.0, 2.0) };
                moreLevel.Click += (_, __) => AdjustFrontLevels(captured, 1);
                levelControls.Children.Add(moreLevel);
                header.Children.Add(levelControls);
                header.MouseLeftButtonDown += (_, __) => SelectFront(captured);
                AddMatrixElement(header, 0, frontIndex + 1);
            }

            for (var displayRow = 0; displayRow < levels; displayRow++)
            {
                var level = levels - displayRow;
                AddMatrixElement(new TextBlock
                {
                    Text = "Nivel " + level.ToString(CultureInfo.InvariantCulture),
                    Foreground = LabelStroke,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2.0, 7.0, 6.0, 7.0)
                }, displayRow + 1, 0);

                for (var frontIndex = 0; frontIndex < frontRows.Count; frontIndex++)
                {
                    var captured = frontIndex;
                    var capturedLevel = level - 1;
                    var row = frontRows[frontIndex];
                    EnsureCellCount(row, row.LoadLevels);
                    var cellValues = row.Cells[Math.Max(0, Math.Min(capturedLevel, row.Cells.Count - 1))];
                    var bfr = DynamicFrontGeometry.Bfr(cellValues.PalletFront);
                    var beam = row.BeamLength > 0.0
                        ? row.BeamLength
                        : cellValues.BeamLengthOverride ?? DynamicFrontGeometry.AutoBeamLength(
                            cellValues.PalletFront, row.PalletCount, DynamicRackDefaults.DefaultPalletTolerance);
                    var active = level <= Math.Max(1, row.LoadLevels);
                    var selected = active
                                   && frontIndex == selectedFrontIndex
                                   && capturedLevel == selectedLevelIndex;
                    var included = active && selectedCells.Contains((frontIndex, capturedLevel));
                    var cell = new Border
                    {
                        BorderBrush = selected
                            ? SelectionStroke
                            : included ? DiagonalStroke : new SolidColorBrush(Color.FromRgb(0xD8, 0xDE, 0xE6)),
                        BorderThickness = new Thickness(selected || included ? 2.0 : 1.0),
                        Background = !active
                            ? new SolidColorBrush(Color.FromRgb(0xF1, 0xF4, 0xF8))
                            : selected || included
                                ? new SolidColorBrush(Color.FromRgb(0xF3, 0xF8, 0xFD))
                                : Brushes.White,
                        Margin = new Thickness(2.0),
                        Padding = new Thickness(5.0),
                        Cursor = active ? Cursors.Hand : Cursors.Arrow,
                        Child = new TextBlock
                        {
                            Text = active
                                ? string.Format(
                                    CultureInfo.InvariantCulture,
                                    "×{0} · BFR {1:0.##}\nL {2:0.##} · P {3:0.##}",
                                     row.PalletCount,
                                     bfr,
                                     beam,
                                     cellValues.IntermediateBeamDepth)
                                : "—",
                            TextAlignment = TextAlignment.Center,
                            Foreground = active
                                ? new SolidColorBrush(Color.FromRgb(0x41, 0x51, 0x61))
                                : new SolidColorBrush(Color.FromRgb(0x9A, 0xA7, 0xB4)),
                            FontSize = 10.5
                        }
                    };
                    cell.MouseLeftButtonDown += (_, __) =>
                    {
                        if (active)
                        {
                            SelectCell(
                                captured,
                                capturedLevel,
                                (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
                        }
                        else
                        {
                            SelectFront(captured);
                        }
                    };
                    AddMatrixElement(cell, displayRow + 1, frontIndex + 1);
                }
            }
        }

        private void AddMatrixElement(UIElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            DynamicMatrixGrid.Children.Add(element);
        }

        private IReadOnlyList<BeamProfileCatalogEntry> InOutBeamOptions()
            => DynamicRackLevelGeometry.CompatibleInOutBeams(catalog);

        private IReadOnlyList<BeamProfileCatalogEntry> IntermediateBeamOptions()
            => DynamicRackLevelGeometry.CompatibleIntermediateBeams(catalog);

        private IReadOnlyList<double> BeamPeraltes(string beamId, double fallback)
        {
            var allowed = DynamicRackLevelGeometry.AllowedPeraltes(catalog, beamId);
            return allowed.Count > 0
                ? allowed
                : new[] { fallback };
        }

        private void SetPeralteOptions(ComboBox combo, string beamId, double selected)
        {
            var fallback = string.Equals(beamId, DynamicRackDefaults.InOutBeamCatalogId, StringComparison.OrdinalIgnoreCase)
                ? DynamicRackDefaults.DefaultBeamDepth
                : DynamicRackDefaults.DefaultIntermediateBeamDepth;
            var allowed = BeamPeraltes(beamId, fallback);
            combo.ItemsSource = allowed;
            combo.SelectedItem = allowed.FirstOrDefault(value => Math.Abs(value - selected) < 1e-6);
            if (combo.SelectedIndex < 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void SelectedInOutBeam_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedInOutBeamBox?.SelectedValue is string id)
            {
                SetPeralteOptions(SelectedInOutPeralteBox, id, SelectedInOutPeralteBox?.SelectedItem is double value
                    ? value
                    : DynamicRackDefaults.DefaultBeamDepth);
            }
        }

        private void SelectedIntermediateBeam_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedIntermediateBeamBox?.SelectedValue is string id)
            {
                SetPeralteOptions(SelectedIntermediatePeralteBox, id, SelectedIntermediatePeralteBox?.SelectedItem is double value
                    ? value
                    : DynamicRackDefaults.DefaultIntermediateBeamDepth);
            }
        }

        private void EnsureCellCount(DynamicFrontRow row, int levelCount)
        {
            if (row == null)
            {
                return;
            }

            while (row.Cells.Count < Math.Max(1, levelCount))
            {
                row.Cells.Add(row.Cells.LastOrDefault()?.Clone() ?? DynamicCellRow.Default());
            }
        }

        private void EnsureIntermediateBeamDepthCount(DynamicFrontRow row, int levelCount)
        {
            EnsureCellCount(row, levelCount);
        }

        private bool SelectedEditorDiffers()
        {
            if (!TryReadSelectedEditor(out var values, out _) || frontRows.Count == 0)
            {
                return false;
            }

            var index = Math.Max(0, Math.Min(selectedFrontIndex, frontRows.Count - 1));
            var row = frontRows[index];
            EnsureCellCount(row, row.LoadLevels);
            var levelIndex = Math.Max(0, Math.Min(selectedLevelIndex, row.LoadLevels - 1));
            var cell = row.Cells[levelIndex];
            return row.PalletCount != values.PalletCount
                   || row.LoadLevels != values.LoadLevels
                   || row.PalletsDeep != values.PalletsDeep
                   || row.DepthStartPosition != values.DepthStartPosition
                   || Math.Abs(row.FirstLevelHeight - values.FirstLevelHeight) > 1e-6
                   || Math.Abs(cell.PalletFront - values.PalletFront) > 1e-6
                   || Math.Abs(cell.PalletHeight - values.PalletHeight) > 1e-6
                   || Math.Abs(cell.PalletWeight - values.PalletWeight) > 1e-6
                   || Math.Abs(cell.ClearHeight - values.ClearHeight) > 1e-6
                   || !string.Equals(cell.InOutBeamCatalogId, values.InOutBeamCatalogId, StringComparison.OrdinalIgnoreCase)
                   || Math.Abs(cell.InOutBeamDepth - values.InOutBeamDepth) > 1e-6
                   || !NullableDoubleEquals(cell.BeamLengthOverride, values.BeamLengthOverride)
                   || !string.Equals(cell.IntermediateBeamCatalogId, values.IntermediateBeamCatalogId, StringComparison.OrdinalIgnoreCase)
                   || Math.Abs(cell.IntermediateBeamDepth - values.IntermediateBeamDepth) > 1e-6;
        }

        private static bool NullableDoubleEquals(double? left, double? right)
        {
            if (!left.HasValue || !right.HasValue)
            {
                return left.HasValue == right.HasValue;
            }

            return Math.Abs(left.Value - right.Value) < 1e-6;
        }

        private void SelectFront(int index)
            => SelectCell(index, Math.Min(selectedLevelIndex, Math.Max(1, frontRows.ElementAtOrDefault(index)?.LoadLevels ?? 1) - 1));

        private void SelectCell(int frontIndex, int levelIndex, bool extendSelection = false)
        {
            if (frontIndex < 0 || frontIndex >= frontRows.Count)
            {
                return;
            }

            levelIndex = Math.Max(0, Math.Min(levelIndex, Math.Max(1, frontRows[frontIndex].LoadLevels) - 1));
            if (!extendSelection && frontIndex == selectedFrontIndex && levelIndex == selectedLevelIndex)
            {
                return;
            }

            var changed = SelectedEditorDiffers();
            if (!CommitSelectedFrontEditor(out var error))
            {
                SetStatus(error, true);
                return;
            }

            if (changed && !Recompose())
            {
                return;
            }

            var key = (FrontIndex: frontIndex, LevelIndex: levelIndex);
            if (extendSelection)
            {
                if (selectedCells.Contains(key) && selectedCells.Count > 1)
                {
                    selectedCells.Remove(key);
                    if (selectedFrontIndex == frontIndex && selectedLevelIndex == levelIndex)
                    {
                        var next = selectedCells.First();
                        selectedFrontIndex = next.FrontIndex;
                        selectedLevelIndex = next.LevelIndex;
                    }
                }
                else
                {
                    selectedCells.Add(key);
                    selectedFrontIndex = frontIndex;
                    selectedLevelIndex = levelIndex;
                }
            }
            else
            {
                selectedCells.Clear();
                selectedCells.Add(key);
                selectedFrontIndex = frontIndex;
                selectedLevelIndex = levelIndex;
            }
            LoadSelectedFrontEditor();
            RenderFrontMatrix();
            DrawSideView();
        }

        private void AdjustFrontPositions(int index, int delta)
        {
            if (index < 0 || index >= frontRows.Count)
            {
                return;
            }

            if (!CommitSelectedFrontEditor(out var error))
            {
                SetStatus(error, true);
                return;
            }

            selectedFrontIndex = index;
            frontRows[index].PalletCount = Math.Max(1, frontRows[index].PalletCount + delta);
            LoadSelectedFrontEditor();
            RenderFrontMatrix();
            Recompose();
        }

        private void AdjustFrontLevels(int index, int delta)
        {
            if (index < 0 || index >= frontRows.Count)
            {
                return;
            }

            if (!CommitSelectedFrontEditor(out var error))
            {
                SetStatus(error, true);
                return;
            }

            selectedFrontIndex = index;
            frontRows[index].LoadLevels = Math.Max(1, frontRows[index].LoadLevels + delta);
            LoadSelectedFrontEditor();
            RenderFrontMatrix();
            Recompose();
        }

        // ---- Table + selected panel ----

        private void BindModules()
        {
            var selectedId = selectedModule?.ModuleId;
            ModulesGrid.ItemsSource = system?.Modules.ToList();

            if (selectedId != null && system != null)
            {
                selectedModule = system.Modules.FirstOrDefault(m => m.ModuleId == selectedId);
                ModulesGrid.SelectedItem = selectedModule;
            }
        }

        private void UpdateSelectedPanel()
        {
            if (selectedModule == null)
            {
                SelectedInfoText.Text = "Ninguno.";
                KindBox.SelectedItem = null;
                ModuleLengthBox.Text = string.Empty;
                ApplyModuleButton.IsEnabled = false;
                EditHeaderButton.IsEnabled = false;
                ConfigBox.IsEnabled = false;
                SelectConfigCalculated();
                return;
            }

            SelectedInfoText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0}  -  X {1:0.##} a {2:0.##}  (long {3:0.##}){4}",
                selectedModule.ModuleId,
                selectedModule.StartX,
                selectedModule.EndX,
                selectedModule.Length,
                selectedModule.IsManualOverride ? "  [override]" : string.Empty);

            KindBox.SelectedItem = selectedModule.IsHeader ? KindHeader : KindSeparator;
            ModuleLengthBox.Text = selectedModule.Length.ToString("0.##", CultureInfo.InvariantCulture);
            ApplyModuleButton.IsEnabled = true;
            EditHeaderButton.IsEnabled = selectedModule.IsHeader;
            ConfigBox.IsEnabled = selectedModule.IsHeader;
            SelectConfigCalculated();
        }

        // ---- Header configuration presets ----

        private void RefreshConfigBox()
        {
            suppressConfigSelection = true;
            try
            {
                var items = new List<string> { ConfigCalculated };
                items.AddRange(headerPresets.Select(preset => preset.Name));
                ConfigBox.ItemsSource = items;
                ConfigBox.SelectedIndex = 0;
            }
            finally
            {
                suppressConfigSelection = false;
            }
        }

        private void SelectConfigCalculated()
        {
            if (ConfigBox.Items.Count == 0)
            {
                return;
            }

            suppressConfigSelection = true;
            try
            {
                ConfigBox.SelectedIndex = 0;
            }
            finally
            {
                suppressConfigSelection = false;
            }
        }

        private void ConfigBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressConfigSelection || selectedModule == null || !selectedModule.IsHeader || system == null)
            {
                return;
            }

            if (!(ConfigBox.SelectedItem is string name))
            {
                return;
            }

            RackFrameConfiguration applied;

            if (name == ConfigCalculated)
            {
                applied = BuildHeaderConfig(Math.Max(selectedModule.Length, 1.0));
                selectedModule.IsManualOverride = false;
                selectedModule.UseCalculatedHeaderConfiguration = true;
            }
            else
            {
                var preset = headerPresets.FirstOrDefault(p => p.Name == name);
                if (preset == null)
                {
                    return;
                }

                // Copy the configuration only — keep the module's own length (Refresh sets Depth = Length).
                applied = Clone(preset.Config);
                selectedModule.IsManualOverride = true;
                selectedModule.UseCalculatedHeaderConfiguration = false;
            }

            if (TryNum(PostPeralteBox?.Text, out var globalPostPeralte) && globalPostPeralte > 0.0)
            {
                applied.PostPeralte = globalPostPeralte;
            }

            selectedModule.AssociatedFrameConfiguration = applied;
            builder.Refresh(system);
            BindModules();
            UpdateSelectedPanel();
            UpdateSummary();
            DrawSideView();
            SetStatus("Configuración '" + name + "' aplicada al módulo.", false);
        }

        private static RackFrameConfiguration Clone(RackFrameConfiguration configuration)
        {
            var store = new RackFrameProjectStore();
            return store.Deserialize(store.Serialize(configuration));
        }

        private sealed class HeaderPreset
        {
            public HeaderPreset(string name, RackFrameConfiguration config)
            {
                Name = name;
                Config = config;
            }

            public string Name { get; }
            public RackFrameConfiguration Config { get; }
        }

        private void UpdateSummary()
        {
            if (system == null)
            {
                SummaryText.Text = string.Empty;
                return;
            }

            var headers = system.Modules.Count(m => m.IsHeader);
            var separators = system.Modules.Count(m => m.Kind == DynamicRackModuleKind.Separator);
            var posts = system.GetDerivedPostOffsets().Count;
            var positions = system.Fronts.Sum(front => Math.Max(1, front.PalletCount));
            var totalWidth = DynamicFrontGeometry.Compute(system, catalog).TotalWidth;

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Longitud total: {0:0.##} in   (regla: N x fondo + 12)\nMódulos: {1}   |   {2} cabeceras, {3} separadores   |   postes derivados: {4}\nFrentes: {5}   |   posiciones: {6}   |   ancho total: {7:0.##} in",
                system.TotalLength,
                system.Modules.Count,
                headers,
                separators,
                posts,
                system.Fronts.Count,
                positions,
                totalWidth);
        }

        // ---- Drawing ----

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSideView();
        }

        private void PreviewView_Changed(object sender, RoutedEventArgs e)
        {
            if (PreviewExitRadio?.IsChecked == true)
            {
                previewMode = DynamicPreviewMode.FrontalExit;
                if (PreviewHint != null) PreviewHint.Text = "Vista frontal de salida (X = frentes, Y = altura). Las divisiones interiores representan posiciones/BFR.";
            }
            else if (PreviewEntranceRadio?.IsChecked == true)
            {
                previewMode = DynamicPreviewMode.FrontalEntrance;
                if (PreviewHint != null) PreviewHint.Text = "Vista frontal de entrada (X = frentes, Y = altura). Los largueros aparecen 6\" arriba por la pendiente.";
            }
            else
            {
                previewMode = DynamicPreviewMode.Lateral;
                if (PreviewHint != null) PreviewHint.Text = "Vista lateral por poste (X = flujo, Y = altura). Clic en un módulo para seleccionarlo.";
            }

            UpdateLateralPostSelector();

            if (PreviewCanvas != null)
            {
                DrawSideView();
            }
        }

        private void PreviewLateralPost_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressLateralPostSelection || PreviewLateralPostBox == null)
            {
                return;
            }

            selectedLateralPostIndex = Math.Max(0, PreviewLateralPostBox.SelectedIndex);
            if (previewMode == DynamicPreviewMode.Lateral && PreviewCanvas != null)
            {
                DrawSideView();
            }
        }

        private void UpdateLateralPostSelector()
        {
            if (PreviewLateralPostBox == null || PreviewLateralPostLabel == null)
            {
                return;
            }

            var visible = previewMode == DynamicPreviewMode.Lateral
                ? Visibility.Visible
                : Visibility.Collapsed;
            PreviewLateralPostBox.Visibility = visible;
            PreviewLateralPostLabel.Visibility = visible;
            var count = Math.Max(1, (system?.Fronts.Count ?? frontRows.Count) + 1);
            selectedLateralPostIndex = Math.Min(selectedLateralPostIndex, count - 1);
            if (PreviewLateralPostBox.Items.Count == count
                && PreviewLateralPostBox.SelectedIndex == selectedLateralPostIndex)
            {
                return;
            }

            suppressLateralPostSelection = true;
            try
            {
                PreviewLateralPostBox.ItemsSource = Enumerable.Range(1, count)
                    .Select(index => "Poste " + index.ToString(CultureInfo.InvariantCulture))
                    .ToList();
                PreviewLateralPostBox.SelectedIndex = selectedLateralPostIndex;
            }
            finally
            {
                suppressLateralPostSelection = false;
            }
        }

        private void PreviewCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (system == null || mapScale <= 0)
            {
                return;
            }

            var worldX = (e.GetPosition(PreviewCanvas).X - mapOffsetX) / mapScale;
            if (previewMode != DynamicPreviewMode.Lateral)
            {
                var preview = DynamicSystemPreviewGeometry.Frontal(system, catalog);
                var layout = preview.Layout;
                var worldY = (mapBottomY - e.GetPosition(PreviewCanvas).Y) / mapScale;
                for (var i = 0; i + 1 < layout.PostPositions.Count; i++)
                {
                    if (worldX >= layout.PostPositions[i] && worldX <= layout.PostPositions[i + 1])
                    {
                        var levels = DynamicFrontGeometry.LoadBeamLevels(system, system.Fronts[i]);
                        var levelIndex = levels.Count == 0
                            ? 0
                            : Enumerable.Range(0, levels.Count)
                                .OrderBy(index => Math.Abs(
                                    (previewMode == DynamicPreviewMode.FrontalEntrance
                                        ? levels[index].EntranceElevation
                                        : levels[index].ExitElevation) - worldY))
                                .First();
                        SelectCell(i, levelIndex);
                        return;
                    }
                }

                return;
            }

            var hit = system.Modules.FirstOrDefault(m => m.Length > 0 && worldX >= m.StartX && worldX <= m.EndX)
                ?? system.Modules.OrderBy(m => Math.Abs((m.StartX + m.EndX) / 2.0 - worldX)).FirstOrDefault();

            if (hit != null)
            {
                ModulesGrid.SelectedItem = hit;
                ModulesGrid.ScrollIntoView(hit);
            }
        }

        private void DrawSideView()
        {
            UpdateLateralPostSelector();
            if (previewMode == DynamicPreviewMode.FrontalExit)
            {
                DrawFrontalPreview(DynamicRackEnd.Exit);
                return;
            }

            if (previewMode == DynamicPreviewMode.FrontalEntrance)
            {
                DrawFrontalPreview(DynamicRackEnd.Entrance);
                return;
            }

            DrawLateralPreview();
        }

        private void DrawLateralPreview()
        {
            PreviewCanvas.Children.Clear();

            if (system == null)
            {
                return;
            }

            var postIndex = Math.Min(selectedLateralPostIndex, system.Fronts.Count);
            var section = DynamicSystemPreviewGeometry.Lateral(system, catalog, postIndex);
            var total = section.Length;
            var height = section.Height;
            var availableWidth = PreviewCanvas.ActualWidth;
            var availableHeight = PreviewCanvas.ActualHeight;

            if (total <= 0 || height <= 0 || availableWidth < 20 || availableHeight < 20)
            {
                return;
            }

            const double horizontalMargin = 52.0;
            const double topMargin = 28.0;
            const double bottomMargin = 76.0;
            var usableWidth = Math.Max(1.0, availableWidth - 2 * horizontalMargin);
            var usableHeight = Math.Max(1.0, availableHeight - topMargin - bottomMargin);
            mapScale = Math.Min(usableWidth / total, usableHeight / height);
            if (mapScale <= 0)
            {
                return;
            }

            var drawHeight = height * mapScale;
            mapOffsetX = (availableWidth - total * mapScale) / 2.0 - section.StartX * mapScale;
            mapBottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;

            AddCanvasLabel(
                Map(section.StartX, height).X,
                Math.Max(4.0, mapBottomY - drawHeight - 24.0),
                "Poste " + (postIndex + 1).ToString(CultureInfo.InvariantCulture)
                    + " · longitud total: " + total.ToString("0.##", CultureInfo.InvariantCulture) + " in",
                LabelStroke,
                12,
                320.0,
                FontWeights.SemiBold);
            AddLine(Map(section.StartX, 0), Map(section.EndX, 0), FloorStroke, 1.5);

            var separatorLevels = DynamicSeparatorGeometry.Levels(system, catalog, height);
            var headerOrdinal = 0;

            foreach (var module in section.Modules.Where(m => m.Length > 0.0))
            {
                if (module.IsHeader)
                {
                    // Every other header is mirrored so the celosía alternates along the line (matches AutoCAD).
                    DrawHeader(
                        module,
                        height,
                        headerOrdinal % 2 == 1,
                        DynamicFrontGeometry.HeaderConfigurationAtPost(system, module, catalog, postIndex));
                    headerOrdinal++;
                }
                else
                {
                    DrawSeparator(module, height, drawHeight, separatorLevels);
                }

                AddLengthLabel(module);
            }

            DrawTotalDimension(section.StartX, section.EndX);

            // Derived intermediate posts: markers only, never primary editable modules.
            foreach (var postX in system.GetDerivedPostOffsets()
                         .Where(offset => offset > section.StartX && offset < section.EndX))
            {
                DrawDerivedPost(postX, height, system.DerivedPostReinforced);
            }
            foreach (var postX in DynamicDepthGeometry.BoundaryPostOffsets(system, section.Range))
            {
                DrawDerivedPost(postX, height, reinforced: false);
            }

            var lateralPlan = section.Plan.Flatten().Instances;
            DrawIntermediateBeams(lateralPlan);
            DrawFlowBeds(lateralPlan);
            DrawLoadBeams(lateralPlan);
            DrawSafety(lateralPlan);

            DrawSelectionHighlight(height, drawHeight, section.StartX, section.EndX);
        }

        private void DrawFrontalPreview(DynamicRackEnd end)
        {
            PreviewCanvas.Children.Clear();
            if (system == null || system.Fronts.Count == 0)
            {
                return;
            }

            var preview = DynamicSystemPreviewGeometry.Frontal(system, catalog);
            var layout = preview.Layout;
            var total = layout.TotalWidth;
            var height = preview.Height;
            var availableWidth = PreviewCanvas.ActualWidth;
            var availableHeight = PreviewCanvas.ActualHeight;
            if (total <= 0.0 || height <= 0.0 || availableWidth < 20.0 || availableHeight < 20.0)
            {
                return;
            }

            const double horizontalMargin = 64.0;
            const double topMargin = 34.0;
            const double bottomMargin = 88.0;
            var usableWidth = Math.Max(1.0, availableWidth - 2.0 * horizontalMargin);
            var usableHeight = Math.Max(1.0, availableHeight - topMargin - bottomMargin);
            mapScale = Math.Min(usableWidth / total, usableHeight / height);
            var drawHeight = height * mapScale;
            mapOffsetX = (availableWidth - total * mapScale) / 2.0;
            mapBottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;

            var cut = end == DynamicRackEnd.Entrance ? "entrada" : "salida";
            AddCanvasLabel(
                mapOffsetX,
                Math.Max(4.0, mapBottomY - drawHeight - 28.0),
                "Frontal " + cut + " · " + system.Fronts.Count.ToString(CultureInfo.InvariantCulture) + " frentes",
                LabelStroke,
                12.0,
                320.0,
                FontWeights.SemiBold);
            AddLine(Map(0.0, 0.0), Map(total, 0.0), FloorStroke, 1.5);

            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                var x = layout.PostPositions[postIndex];
                var postHeight = preview.PostHeights[postIndex];
                AddLine(Map(x, 0.0), Map(x, postHeight), UprightStroke, 4.0);
                var floor = Map(x, 0.0);
                AddRectangle(floor.X - 6.0, floor.Y - 3.0, 12.0, 5.0, PlateFill, 1.0, null, PlateFill);
            }

            for (var frontIndex = 0; frontIndex < system.Fronts.Count; frontIndex++)
            {
                var front = system.Fronts[frontIndex];
                var left = layout.PostPositions[frontIndex];
                var right = layout.PostPositions[frontIndex + 1];
                var beamStart = left + layout.TroquelPositions[frontIndex];
                if (frontIndex == selectedFrontIndex)
                {
                    var selectedLevels = DynamicFrontGeometry.LoadBeamLevels(system, front);
                    if (selectedLevels.Count > 0)
                    {
                        var levelIndex = Math.Max(0, Math.Min(selectedLevelIndex, selectedLevels.Count - 1));
                        double Elevation(DynamicLoadBeamLevel level) => end == DynamicRackEnd.Entrance
                            ? level.EntranceElevation
                            : level.ExitElevation;
                        var bottom = Elevation(selectedLevels[levelIndex]);
                        var top = levelIndex + 1 < selectedLevels.Count
                            ? Elevation(selectedLevels[levelIndex + 1])
                            : front.Height;
                        top = Math.Max(bottom + 1.0, Math.Min(front.Height, top));
                        var topLeft = Map(left, top);
                        AddRectangle(
                            topLeft.X,
                            topLeft.Y,
                            Math.Max(1.0, (right - left) * mapScale),
                            Math.Max(1.0, (top - bottom) * mapScale),
                            SelectionStroke,
                            2.0,
                            Dash(),
                            new SolidColorBrush(Color.FromArgb(0x12, 0xFF, 0xD1, 0x66)));
                    }
                }

                foreach (var level in DynamicFrontGeometry.LoadBeamLevels(system, front))
                {
                    var y = end == DynamicRackEnd.Entrance ? level.EntranceElevation : level.ExitElevation;
                    AddLine(Map(beamStart, y), Map(beamStart + front.BeamLength, y), LoadBeamStroke, 4.0);
                }

                // One BFR band per pallet lane. The beam has three inches of allowance at each end.
                var laneStart = beamStart + DynamicRackDefaults.InOutBeamLengthAllowance / 2.0;
                for (var lane = 1; lane < front.PalletCount; lane++)
                {
                    var x = laneStart + lane * front.Bfr;
                    AddLine(Map(x, 0.0), Map(x, front.Height), SeparatorStroke, 1.0, Dash());
                }

                var center = (left + right) / 2.0;
                AddCenteredLabel(
                    Map(center, 0.0).X,
                    mapBottomY + 10.0,
                    string.Format(CultureInfo.InvariantCulture, "Frente {0} · ×{1}\nBFR {2:0.##} · L {3:0.##}",
                        frontIndex + 1, front.PalletCount, front.Bfr, front.BeamLength),
                    Math.Max(88.0, (right - left) * mapScale),
                    frontIndex == selectedFrontIndex ? SelectionStroke : LabelStroke,
                    10.5,
                    frontIndex == selectedFrontIndex ? FontWeights.SemiBold : FontWeights.Normal);
            }

            var safety = new DynamicSystemFrontalBuilder()
                .Build(system, catalog, end)
                .Where(instance => instance.Role == HeaderBlockRole.Safety);
            foreach (var piece in safety)
            {
                var at = Map(piece.Insertion.X, piece.Insertion.Y);
                var element = catalog?.SafetyElements?.FirstOrDefault(entry => string.Equals(
                    entry?.Id, piece.PieceId, StringComparison.OrdinalIgnoreCase));
                if (SelectiveSafetyDefaults.IsType(element?.Type, SelectiveSafetyDefaults.DesviadorType)
                    && piece.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var length))
                {
                    AddLine(at, Map(piece.Insertion.X, piece.Insertion.Y + length), SafetyStroke, 2.6);
                }
                else
                {
                    AddRectangle(at.X - 4.0, at.Y - 8.0, 8.0, 8.0, SafetyStroke, 1.8, null);
                }
            }
        }

        private void DrawLoadBeams(IReadOnlyList<HeaderBlockInstance> lateralPlan)
        {
            var depth = system?.InOutBeamDepth > 0.0
                ? system.InOutBeamDepth
                : DynamicRackDefaults.DefaultBeamDepth;
            var beamId = string.IsNullOrWhiteSpace(system?.InOutBeamCatalogId)
                ? DynamicRackDefaults.InOutBeamCatalogId
                : system.InOutBeamCatalogId;

            foreach (var placement in lateralPlan.Where(instance =>
                         instance.Role == HeaderBlockRole.Beam
                         && string.Equals(
                             instance.PieceId,
                             beamId,
                             StringComparison.OrdinalIgnoreCase)))
            {
                AddLine(
                    Map(placement.Insertion.X, placement.Insertion.Y),
                    Map(placement.Insertion.X, placement.Insertion.Y + depth),
                    HorizontalStroke,
                    5.0);
            }
        }

        private void DrawIntermediateBeams(IReadOnlyList<HeaderBlockInstance> lateralPlan)
        {
            var leftMate = catalog.ConnectionLayout.FindConnectionLayout(
                DynamicRackDefaults.IntermediateBeamCatalogId,
                DynamicRackDefaults.IntermediateBeamLeftBedMatePoint,
                DynamicRackDefaults.IntermediateBeamView);
            var rightMate = catalog.ConnectionLayout.FindConnectionLayout(
                DynamicRackDefaults.IntermediateBeamCatalogId,
                DynamicRackDefaults.IntermediateBeamRightBedMatePoint,
                DynamicRackDefaults.IntermediateBeamView);
            if (leftMate == null || rightMate == null)
            {
                return;
            }

            var supports = lateralPlan
                .Where(instance => instance.Role == HeaderBlockRole.Beam
                                   && instance.PieceId == DynamicRackDefaults.IntermediateBeamCatalogId);

            foreach (var support in supports)
            {
                var mate = support.MirroredX
                    ? new Point2D(rightMate.LocalX, rightMate.LocalY)
                    : new Point2D(leftMate.LocalX, leftMate.LocalY);
                var contact = LocalToWorld(support, mate);
                var topOnPostAxis = new Point2D(support.Insertion.X, contact.Y);

                // Simplified preview glyph: vertical slotted bracket on the post axis plus its short bed contact.
                // The actual block geometry remains exclusively in AutoCAD.
                AddLine(Map(support.Insertion.X, support.Insertion.Y), Map(topOnPostAxis.X, topOnPostAxis.Y), HorizontalStroke, 3.0);
                AddLine(Map(topOnPostAxis.X, topOnPostAxis.Y), Map(contact.X, contact.Y), HorizontalStroke, 2.0);
            }
        }

        private void DrawSafety(IReadOnlyList<HeaderBlockInstance> lateralPlan)
        {
            foreach (var piece in lateralPlan.Where(instance => instance.Role == HeaderBlockRole.Safety))
            {
                var element = catalog?.SafetyElements?.FirstOrDefault(entry => string.Equals(
                    entry?.Id,
                    piece.PieceId,
                    StringComparison.OrdinalIgnoreCase));
                var at = Map(piece.Insertion.X, piece.Insertion.Y);

                if (SelectiveSafetyDefaults.IsType(element?.Type, SelectiveSafetyDefaults.LateralType)
                    && piece.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var guardLength))
                {
                    var endX = piece.MirroredX ? piece.Insertion.X - guardLength : piece.Insertion.X + guardLength;
                    AddLine(at, Map(endX, piece.Insertion.Y), SafetyStroke, 4.2);
                }
                else if (SelectiveSafetyDefaults.IsType(element?.Type, SelectiveSafetyDefaults.DesviadorType)
                         && piece.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var deviatorLength))
                {
                    AddLine(at, Map(piece.Insertion.X, piece.Insertion.Y + deviatorLength), SafetyStroke, 3.2);
                }
                else
                {
                    const double size = 9.0;
                    AddRectangle(at.X - size / 2.0, at.Y - size, size, size, SafetyStroke, 2.0, null);
                }
            }
        }

        private void DrawFlowBeds(IReadOnlyList<HeaderBlockInstance> instances)
        {
            var railMate = catalog.ConnectionLayout.FindConnectionLayout(
                FlowBedDefaults.RailId,
                FlowBedDefaults.RailInOutMatePoint,
                FlowBedDefaults.View);
            if (railMate == null)
            {
                return;
            }

            foreach (var rail in instances.Where(instance => instance.Role == HeaderBlockRole.Rail))
            {
                if (!rail.DynamicParameters.TryGetValue("LONGITUD", out var length))
                {
                    continue;
                }

                var start = LocalToWorld(rail, new Point2D(railMate.LocalX, railMate.LocalY));
                var end = LocalToWorld(rail, new Point2D(length, railMate.LocalY));
                AddLine(Map(start.X, start.Y), Map(end.X, end.Y), FlowBedStroke, 3.2);
            }

            foreach (var piece in instances.Where(instance =>
                         instance.Role == HeaderBlockRole.Roller
                         || instance.Role == HeaderBlockRole.Brake
                         || instance.Role == HeaderBlockRole.Stop))
            {
                var center = Map(piece.Insertion.X, piece.Insertion.Y);
                var half = piece.Role == HeaderBlockRole.Brake ? 4.5 : 3.0;
                var dx = -Math.Sin(piece.RotationRadians) * half;
                var dy = -Math.Cos(piece.RotationRadians) * half;
                var stroke = piece.Role == HeaderBlockRole.Brake
                    ? ReinforcementStroke
                    : piece.Role == HeaderBlockRole.Stop ? SelectionStroke : FlowBedStroke;
                AddLine(
                    new Point(center.X - dx, center.Y - dy),
                    new Point(center.X + dx, center.Y + dy),
                    stroke,
                    piece.Role == HeaderBlockRole.Brake ? 2.2 : 1.2);
            }
        }

        private static Point2D LocalToWorld(HeaderBlockInstance instance, Point2D local)
        {
            var localX = instance.MirroredX ? -local.X : local.X;
            var localY = instance.MirroredY ? -local.Y : local.Y;
            var cos = Math.Cos(instance.RotationRadians);
            var sin = Math.Sin(instance.RotationRadians);
            return new Point2D(
                instance.Insertion.X + localX * cos - localY * sin,
                instance.Insertion.Y + localX * sin + localY * cos);
        }

        private void DrawHeader(
            DynamicRackModule module,
            double height,
            bool mirrored,
            RackFrameConfiguration config)
        {
            var topLeft = Map(module.StartX, height);
            var moduleWidth = module.Length * mapScale;
            var moduleHeight = height * mapScale;
            AddRectangle(topLeft.X, topLeft.Y, moduleWidth, moduleHeight, ModuleBoundaryStroke, 1.0, null, HeaderFill);
            AddLine(Map(module.StartX, height), Map(module.EndX, height), ModuleBoundaryStroke, 1.0);
            AddLine(Map(module.StartX, 0), Map(module.EndX, 0), ModuleBoundaryStroke, 1.0);
            DrawHeaderPost(module.StartX, height, moduleWidth, config?.LeftPost, true);
            DrawHeaderPost(module.EndX, height, moduleWidth, config?.RightPost, false);
            DrawBasePlate(module.StartX);
            DrawBasePlate(module.EndX);

            if (config == null)
            {
                DrawFallbackHeaderMembers(module, height, mirrored);
                return;
            }

            var depth = config.Depth <= 0 ? module.Length : config.Depth;
            var drewMembers = false;

            // Mirror a header by flipping member X within the module span (the celosía direction flips).
            double MemberX(double ratio) => mirrored ? module.EndX - ratio * depth : module.StartX + ratio * depth;

            foreach (var member in config.Members ?? Enumerable.Empty<FrameMember>())
            {
                if (member?.Start == null || member.End == null)
                {
                    continue;
                }

                var sx = MemberX(member.Start.HorizontalPositionRatio);
                var ex = MemberX(member.End.HorizontalPositionRatio);
                var thickness = member.MemberType == FrameMemberType.DiagonalBrace ? 3.0 : 3.6;
                AddLine(Map(sx, member.Start.Elevation), Map(ex, member.End.Elevation), MemberBrush(member.MemberType), thickness);
                drewMembers = true;
            }

            if (!drewMembers)
            {
                DrawFallbackHeaderMembers(module, height, mirrored);
            }
        }

        private void DrawSeparator(DynamicRackModule module, double height, double drawHeight, IReadOnlyList<double> levels)
        {
            var topLeft = Map(module.StartX, height);
            var width = Math.Max(1.0, module.Length * mapScale);
            AddRectangle(topLeft.X, topLeft.Y, width, drawHeight, SeparatorStroke, 0.9, Dash(), SeparatorFill);

            // The actual separator beams at each vertical level (so a custom count/spacing is visible).
            foreach (var level in levels)
            {
                AddLine(Map(module.StartX, level), Map(module.EndX, level), SeparatorStroke, 2.2);
            }
        }

        /// <summary>Read the optional separator count/spacing fields onto the system (empty = standard).</summary>
        private void ApplySeparatorOverrides()
        {
            if (system == null)
            {
                return;
            }

            system.SeparatorCountOverride =
                UiSupport.TryInt(SeparatorCountBox.Text, out var count) && count >= 1 ? count : (int?)null;
            system.SeparatorSpacingOverride =
                TryNum(SeparatorSpacingBox.Text, out var spacing) && spacing > 0.0 ? spacing : (double?)null;
        }

        /// <summary>Read the derived-post reinforcement option/length onto the system (empty length = full height).</summary>
        private void ApplyDerivedPostOptions()
        {
            if (system == null)
            {
                return;
            }

            system.DerivedPostReinforced = DerivedReinforceBox.IsChecked == true;
            system.DerivedPostReinforcementHeight =
                TryNum(DerivedReinforcementBox.Text, out var length) && length > 0.0 ? length : (double?)null;
        }

        /// <summary>Validate the OPTIONAL numeric fields so invalid text is reported instead of silently defaulting (the
        /// selective validates the same way). Empty stays valid (= the field's default); only garbage/out-of-range fails.</summary>
        private bool TryValidateOptionalInputs(out string error)
        {
            error = null;

            if (!TryReadHeightInputs(out _, out _, out _, out error))
            {
                return false;
            }

            if (!TryReadFrontInputs(out _, out error))
            {
                return false;
            }

            if (!TryNum(AnnotationScaleBox?.Text, out var annotationScale) || annotationScale <= 0.0)
            {
                error = "La escala de texto debe ser un número mayor que cero.";
                return false;
            }

            var count = SeparatorCountBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(count) && !(UiSupport.TryInt(count, out var n) && n >= 1))
            {
                error = "Cantidad de separadores inválida (entero >= 1, o vacío para estándar).";
                return false;
            }

            if (!UiSupport.TryOptionalNum(SeparatorSpacingBox.Text, out _))
            {
                error = "Separación de separadores inválida (deja vacío para estándar).";
                return false;
            }

            if (!UiSupport.TryOptionalNum(DerivedReinforcementBox.Text, out _))
            {
                error = "Altura de refuerzo del poste derivado inválida (deja vacío para altura completa).";
                return false;
            }

            if (ManualHeightToggle?.IsChecked == true && !(UiSupport.TryNum(ManualHeightBox.Text, out var m) && m > 0.0))
            {
                error = "Altura manual inválida (debe ser mayor que cero).";
                return false;
            }

            return true;
        }

        private void DerivedReinforce_Changed(object sender, RoutedEventArgs e)
        {
            // Ignore the initial state set during construction and changes applied while syncing a load.
            if (system == null || suppressRecompose)
            {
                return;
            }

            // Just a scalar option — apply it without a rebuild so per-module overrides survive.
            ApplyDerivedPostOptions();
            builder.Refresh(system);
            UpdateSummary();
            DrawSideView();
        }

        /// <summary>Y of the post's first separator troquel (TROQUEL_SEPARADOR), the base of the separator grid.</summary>
        private double SeparatorBaseY()
        {
            var postId = system?.Modules
                .FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration != null)?
                .AssociatedFrameConfiguration.LeftPost?.PostCatalogId ?? defaultPostCatalogId;

            var entry = catalog.ConnectionLayout.FindConnectionLayout(postId, DynamicRackDefaults.SeparatorPostPoint, "LATERAL");
            return entry?.LocalY ?? 0.0;
        }

        private void DrawHeaderPost(double x, double height, double moduleWidth, PostAssembly post, bool reinforcementOnRightSide)
        {
            var postCenter = Map(x, height / 2.0);
            var top = Map(x, height).Y;
            var bottom = Map(x, 0).Y;
            // Real post thickness from the catalog profile (its width, in inches), scaled to screen.
            var profileWidth = catalog.PostProfiles.FindProfile(post?.PostCatalogId)?.Width ?? 0.0;
            var postWidth = profileWidth > 0.0
                ? Math.Max(5.0, profileWidth * mapScale)
                : Math.Max(7.0, Math.Min(14.0, moduleWidth * 0.18));
            var left = postCenter.X - postWidth / 2.0;

            AddRectangle(left, top, postWidth, bottom - top, UprightStroke, 1.7, null, PostFill);
            AddLine(new Point(left + postWidth * 0.28, top + 6.0), new Point(left + postWidth * 0.28, bottom - 6.0), ModuleBoundaryStroke, 0.8);
            AddLine(new Point(left + postWidth * 0.72, top + 6.0), new Point(left + postWidth * 0.72, bottom - 6.0), ModuleBoundaryStroke, 0.8);

            if (post?.HasReinforcement == true)
            {
                var offset = reinforcementOnRightSide
                    ? postWidth / 2.0 + 5.0
                    : -(postWidth / 2.0 + 5.0);
                var reinforcementX = postCenter.X + offset;
                AddLine(new Point(reinforcementX, top + 3.0), new Point(reinforcementX, bottom - 3.0), ReinforcementStroke, 3.4);
                AddLine(new Point(reinforcementX - 3.0, top + 10.0), new Point(reinforcementX + 3.0, top + 10.0), ReinforcementStroke, 1.0);
                AddLine(new Point(reinforcementX - 3.0, bottom - 10.0), new Point(reinforcementX + 3.0, bottom - 10.0), ReinforcementStroke, 1.0);
            }
        }

        private void DrawBasePlate(double x)
        {
            var bottom = Map(x, 0).Y;
            var plateWidth = Math.Max(16.0, Math.Min(34.0, 18.0 + 4.0 * mapScale));
            AddRectangle(Map(x, 0).X - plateWidth / 2.0, bottom + 4.0, plateWidth, 5.5, UprightStroke, 1.0, null, PlateFill);
        }

        private void DrawFallbackHeaderMembers(DynamicRackModule module, double height, bool mirrored)
        {
            AddLine(Map(module.StartX, height * 0.12), Map(module.EndX, height * 0.12), HorizontalStroke, 3.6);
            AddLine(Map(module.StartX, height * 0.50), Map(module.EndX, height * 0.50), HorizontalStroke, 3.2);
            AddLine(Map(module.StartX, height * 0.88), Map(module.EndX, height * 0.88), HorizontalStroke, 3.2);

            // Flip the diagonals for a mirrored header so the celosía alternates along the line.
            var lowX = mirrored ? module.EndX : module.StartX;
            var highX = mirrored ? module.StartX : module.EndX;
            AddLine(Map(lowX, height * 0.12), Map(highX, height * 0.50), DiagonalStroke, 3.0);
            AddLine(Map(highX, height * 0.50), Map(lowX, height * 0.88), DiagonalStroke, 3.0);
        }

        private void DrawDerivedPost(double x, double height, bool reinforced)
        {
            var postId = system?.Modules
                .FirstOrDefault(module => module.IsHeader && module.AssociatedFrameConfiguration?.LeftPost != null)?
                .AssociatedFrameConfiguration.LeftPost.PostCatalogId ?? defaultPostCatalogId;
            var finPoste = CatalogLookup.Local(catalog, postId, "FIN_POSTE", "LATERAL");
            var placement = DynamicDerivedPostGeometry.Resolve(
                x,
                reinforced,
                finPoste);
            var primaryX = placement.PrimaryOrigin.X;
            var top = Map(primaryX, height).Y;
            var bottom = Map(primaryX, 0).Y;
            var canvasX = Map(primaryX, 0).X;

            AddLine(new Point(canvasX, top), new Point(canvasX, bottom), PostStroke, 2.4, Dash());
            AddLine(new Point(canvasX - 8.0, top + 8.0), new Point(canvasX + 8.0, top + 8.0), PostStroke, 1.3);
            AddLine(new Point(canvasX - 8.0, bottom - 8.0), new Point(canvasX + 8.0, bottom - 8.0), PostStroke, 1.3);

            if (placement.HasReinforcement)
            {
                var reinforcementHeight = system.DerivedPostReinforcementHeight is double rh && rh > 0.0 ? rh : height;
                var reinforcementTop = Map(placement.ReinforcementOrigin.X, Math.Min(reinforcementHeight, height)).Y;
                var reinforcementX = Map(placement.ReinforcementOrigin.X, 0.0).X;
                AddLine(new Point(reinforcementX, reinforcementTop + 1.0), new Point(reinforcementX, bottom - 1.0), ReinforcementStroke, 3.4);
                AddLine(new Point(reinforcementX - 3.0, bottom - 10.0), new Point(reinforcementX + 3.0, bottom - 10.0), ReinforcementStroke, 1.0);
            }

            AddCenteredLabel(Map(x, 0.0).X, mapBottomY + 34.0, "poste", 52.0, PostStroke, 9.5, null);
        }

        private void DrawSelectionHighlight(
            double height,
            double drawHeight,
            double sectionStartX,
            double sectionEndX)
        {
            if (selectedModule == null || selectedModule.Length <= 0.0)
            {
                return;
            }

            if (selectedModule.EndX <= sectionStartX || selectedModule.StartX >= sectionEndX)
            {
                return;
            }

            var topLeft = Map(selectedModule.StartX, height);
            AddRectangle(topLeft.X - 1, topLeft.Y - 1, Math.Max(selectedModule.Length * mapScale, 2) + 2, drawHeight + 2, SelectionStroke, 2.2, null);
        }

        private void AddLengthLabel(DynamicRackModule module)
        {
            if (module.Length <= 0.0)
            {
                return;
            }

            var centerX = (module.StartX + module.EndX) / 2.0;
            var canvasX = Map(centerX, 0).X;
            var maxWidth = Math.Max(28.0, module.Length * mapScale - 6.0);
            AddCenteredLabel(canvasX, mapBottomY + 7.0, module.Length.ToString("0.##", CultureInfo.InvariantCulture) + " in", maxWidth, LabelStroke, 10.5, FontWeights.SemiBold);

            if (maxWidth >= 54.0)
            {
                AddCenteredLabel(canvasX, mapBottomY + 22.0, ModuleLabel(module), maxWidth, LabelStroke, 9.2, null);
            }
        }

        private void DrawTotalDimension(double startX, double endX)
        {
            var total = Math.Max(0.0, endX - startX);
            var y = mapBottomY + 56.0;
            var start = Map(startX, 0).X;
            var end = Map(endX, 0).X;
            AddLine(new Point(start, y), new Point(end, y), FloorStroke, 1.0);
            AddLine(new Point(start, y - 5.0), new Point(start, y + 5.0), FloorStroke, 1.0);
            AddLine(new Point(end, y - 5.0), new Point(end, y + 5.0), FloorStroke, 1.0);
            AddCenteredLabel((start + end) / 2.0, y + 4.0, total.ToString("0.##", CultureInfo.InvariantCulture) + " in total", Math.Max(80.0, end - start), LabelStroke, 10.5, null);
        }

        private static string ModuleLabel(DynamicRackModule module)
        {
            if (module == null)
            {
                return string.Empty;
            }

            if (module.Kind == DynamicRackModuleKind.Separator)
            {
                return "separador";
            }

            if (module.Kind == DynamicRackModuleKind.HeaderStart)
            {
                return "cab. inicial";
            }

            if (module.Kind == DynamicRackModuleKind.HeaderEnd)
            {
                return "cab. final";
            }

            return "cabecera";
        }

        private static Brush MemberBrush(FrameMemberType type)
        {
            return type == FrameMemberType.DiagonalBrace ? DiagonalStroke : HorizontalStroke;
        }

        private double HeaderHeight()
        {
            var header = system?.Modules.FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration != null);
            return header?.AssociatedFrameConfiguration.Height ?? computedHeaderHeight;
        }

        /// <summary>Troquel pitch from the first header's configuration (falls back to the standard 2").</summary>
        private double PasoTroquel()
        {
            var header = system?.Modules.FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration != null);
            var paso = header?.AssociatedFrameConfiguration.PasoTroquel ?? 0.0;
            return paso > 0.0 ? paso : 2.0;
        }

        private Point Map(double x, double y)
        {
            return new Point(mapOffsetX + x * mapScale, mapBottomY - y * mapScale);
        }

        private PreviewCanvasPainter previewPainter;
        private PreviewCanvasPainter Painter => previewPainter ??= new PreviewCanvasPainter(PreviewCanvas);

        private void AddLine(Point a, Point b, Brush stroke, double thickness, DoubleCollection dash = null)
            => Painter.AddLine(a, b, stroke, thickness, dash);

        private void AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, DoubleCollection dash, Brush fill = null)
            => Painter.AddRectangle(left, top, width, height, stroke, thickness, dash, fill);

        private void AddCanvasLabel(double left, double top, string text, Brush brush, double size, double maxWidth = 0.0, FontWeight? fontWeight = null)
        {
            var label = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = size,
                FontWeight = fontWeight ?? FontWeights.Normal,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            if (maxWidth > 0.0)
            {
                label.Width = maxWidth;
                label.TextAlignment = TextAlignment.Left;
            }

            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            PreviewCanvas.Children.Add(label);
        }

        private void AddCenteredLabel(double centerX, double top, string text, double maxWidth, Brush brush, double size, FontWeight? fontWeight)
        {
            var width = Math.Max(24.0, maxWidth);
            var label = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = size,
                FontWeight = fontWeight ?? FontWeights.Normal,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Width = width
            };

            Canvas.SetLeft(label, centerX - width / 2.0);
            Canvas.SetTop(label, top);
            PreviewCanvas.Children.Add(label);
        }

        private static DoubleCollection Dash()
        {
            return new DoubleCollection { 5, 3 };
        }

        // ---- BOM / persistence ----

        private void ExportBom_Click(object sender, RoutedEventArgs e)
        {
            if (system == null) { SetStatus("Genera la vista antes de exportar el BOM.", true); return; }

            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "CSV (*.csv)|*.csv|Todos (*.*)|*.*", FileName = "bom-sistema.csv" };
            if (dialog.ShowDialog(this) != true) { return; }

            try
            {
                System.IO.File.WriteAllText(dialog.FileName, BomCsvExporter.ToCsv(SystemBomBuilder.Build(system, catalog)));
                SetStatus("BOM del sistema exportado.", false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo exportar el BOM: " + ex.Message, true);
            }
        }

        private void ViewBom_Click(object sender, RoutedEventArgs e)
        {
            if (system == null) { SetStatus("Genera la vista antes de ver el BOM.", true); return; }
            new RackBomWindow(SystemBomBuilder.Build(system, catalog)) { Owner = this }.ShowDialog();
        }

        private void SaveSystem_Click(object sender, RoutedEventArgs e)
        {
            if (system == null) { SetStatus("Genera la vista antes de guardar.", true); return; }
            if (!TryValidateOptionalInputs(out var invalid)) { SetStatus(invalid, true); return; }
            if (!Recompose()) { return; }

            var path = UiSupport.PromptSaveToLibrary(this, NameBox?.Text, "sistema");
            if (path == null) { return; }

            try
            {
                new RackProjectStore().Save(RackProject.ForDynamic(design, system).WithSourceMetadataFrom(sourceProject), path);
                SetStatus("Sistema guardado: " + System.IO.Path.GetFileName(path), false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo guardar: " + ex.Message, true);
            }
        }

        private void OpenSystem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Proyecto RackCad (*.rackcad.json)|*.rackcad.json|JSON (*.json)|*.json|Todos (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != true) { return; }

            try
            {
                var project = new RackProjectStore().Load(dialog.FileName);
                if (project.Kind != RackSystemKind.PalletFlow || project.DynamicDesign == null)
                {
                    SetStatus("El archivo no es un sistema dinámico.", true);
                    return;
                }

                sourceProject = project; // keep the loaded project so a re-save preserves its wrapper metadata (I-11)
                RestoreFrom(project.DynamicDesign);
                SetStatus("Sistema abierto: " + System.IO.Path.GetFileName(dialog.FileName), false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo abrir: " + ex.Message, true);
            }
        }

        /// <summary>Restore the whole editor from persisted editable inputs, then resolve one fresh system.</summary>
        private void RestoreFrom(DynamicRackDesign loaded)
        {
            if (loaded == null)
            {
                return;
            }

            safetySelections.Clear();
            safetySelections.AddRange(loaded.SafetySelections
                .Where(SafetyDraws)
                .Select(selection => selection.DeepCopy()));
            var resolution = resolver.Resolve(loaded);
            design = loaded;
            system = resolution.System;
            system.Name = NameBox?.Text?.Trim();
            selectedModule = null;
            suppressRecompose = true;
            try
            {
                FrontBox.Text = Num(system.Pallet.Front);
                DepthBox.Text = Num(system.Pallet.Depth);
                PalletHeightBox.Text = Num(system.Pallet.Height);
                WeightBox.Text = Num(system.Pallet.Weight);
                PalletsDeepBox.Text = system.PalletsDeep.ToString(CultureInfo.InvariantCulture);
                PostPeralteBox.Text = Num(system.PostPeralte > 0.0
                    ? system.PostPeralte
                    : DynamicFrontGeometry.PostPeralte(system, catalog));
                LoadLevelsBox.Text = loaded.LoadLevels.ToString(CultureInfo.InvariantCulture);
                FirstLevelHeightBox.Text = Num(loaded.FirstLevelHeight);
                BeamDepthBox.Text = Num(system.InOutBeamDepth);
                RestoreFrontRows(system.Fronts);
                NumberFrontsCheck.IsChecked = loaded.NumberFronts;
                NumberLevelsCheck.IsChecked = loaded.NumberLevels;
                DrawRackNameCheck.IsChecked = loaded.DrawRackName;
                AnnotationScaleBox.Text = Num(loaded.AnnotationScale > 0.0 ? loaded.AnnotationScale : 1.0);
                DimensionsBox.SelectedIndex = Math.Min(
                    (int)DimensionDetail.Detailed,
                    Math.Max(0, (int)loaded.Dimensions));
                SelectDimensionStyle(loaded.DimensionStyle);

                SeparatorCountBox.Text = system.SeparatorCountOverride?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                SeparatorSpacingBox.Text = system.SeparatorSpacingOverride.HasValue ? Num(system.SeparatorSpacingOverride.Value) : string.Empty;
                DerivedReinforceBox.IsChecked = system.DerivedPostReinforced;
                DerivedReinforcementBox.Text = system.DerivedPostReinforcementHeight.HasValue ? Num(system.DerivedPostReinforcementHeight.Value) : string.Empty;

                ManualHeightToggle.IsChecked = system.ManualHeaderHeightOverride.HasValue;
                ManualHeightBox.Text = system.ManualHeaderHeightOverride.HasValue ? Num(system.ManualHeaderHeightOverride.Value) : string.Empty;
                computedHeaderHeight = system.ManualHeaderHeightOverride ?? resolution.Height.HeaderHeight;
                ComputedHeightText.Text = system.ManualHeaderHeightOverride.HasValue
                    ? string.Format(CultureInfo.InvariantCulture, "Altura manual: {0:0.#}\"  (derivada sería {1:0}\")", system.ManualHeaderHeightOverride.Value, resolution.Height.HeaderHeight)
                    : string.Format(CultureInfo.InvariantCulture, "Altura calculada: {0:0.#}\" → {1:0}\"  (pendiente {2:0.#}\")", resolution.Height.TheoreticalHeight, resolution.Height.HeaderHeight, resolution.Height.Slope);

                var loadedPostId = loaded.HeaderPostCatalogId;
                if (string.IsNullOrWhiteSpace(loadedPostId))
                {
                    loadedPostId = system.Modules
                        .FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration?.LeftPost != null)?
                        .AssociatedFrameConfiguration.LeftPost.PostCatalogId;
                }
                if (!string.IsNullOrWhiteSpace(loadedPostId))
                {
                    PostBox.SelectedValue = loadedPostId;
                }
            }
            finally
            {
                suppressRecompose = false;
            }

            BindModules();
            UpdateSelectedPanel();
            UpdateSummary();
            UpdateSafetyButton();
            DrawSideView();
        }

        /// <summary>Open the editor pre-loaded with an existing drawn system (from its embedded payload), keeping Id/Name.
        /// <paramref name="sourceProject"/> is the project the system was read from; passing it lets a re-save preserve its
        /// wrapper metadata (I-11).</summary>
        public void LoadExisting(DynamicRackDesign loaded, string id, string name, RackProject sourceProject = null)
        {
            if (loaded == null)
            {
                return;
            }

            this.sourceProject = sourceProject;
            currentId = id;
            currentName = name;
            if (NameBox != null)
            {
                NameBox.Text = name ?? string.Empty;
            }

            isEditingExisting = true;
            UpdateDrawButtons();

            RestoreFrom(loaded);
        }

        /// <summary>Load a design opened from the library as a NEW insert: keeps the "Insertar" button and mints a fresh
        /// GUID on insert (unlike <see cref="LoadExisting"/>, which is the in-place round-trip edit).</summary>
        public void LoadDesignForNew(DynamicRackDesign loaded, string name, RackProject sourceProject = null)
        {
            if (loaded == null)
            {
                return;
            }

            this.sourceProject = sourceProject;
            if (NameBox != null && !string.IsNullOrWhiteSpace(name))
            {
                NameBox.Text = name.Trim();
            }

            RestoreFrom(loaded);
        }

        private void InsertLateral_Click(object sender, RoutedEventArgs e)
            => RequestDraw(RackEmbedDocument.ViewLateral, -1, updateOnly: false);

        private void InsertExit_Click(object sender, RoutedEventArgs e)
            => RequestDraw(RackEmbedDocument.ViewFrontal, (int)DynamicRackEnd.Exit, updateOnly: false);

        private void InsertEntrance_Click(object sender, RoutedEventArgs e)
            => RequestDraw(RackEmbedDocument.ViewFrontal, (int)DynamicRackEnd.Entrance, updateOnly: false);

        private void InsertPlanta_Click(object sender, RoutedEventArgs e)
            => RequestDraw(RackEmbedDocument.ViewPlanta, -1, updateOnly: false);

        private void UpdateExisting_Click(object sender, RoutedEventArgs e)
            => RequestDraw(null, -1, updateOnly: true);

        private void RequestDraw(string view, int section, bool updateOnly)
        {
            if (!canInsertInAutoCad)
            {
                SetStatus("El dibujo en AutoCAD solo está disponible cuando el sistema se abre desde AutoCAD.", true);
                return;
            }

            if (!isEditingExisting && (updateOnly || view != RackEmbedDocument.ViewLateral))
            {
                MessageBox.Show(
                    this,
                    "Primero inserta la vista lateral. Luego selecciónala con RACKEDITAR para actualizar el sistema o agregar las vistas frontal y planta enlazadas.",
                    "Vistas del sistema dinámico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (system == null)
            {
                SetStatus("Genera la vista antes de insertar en AutoCAD.", true);
                return;
            }

            if (!TryValidateOptionalInputs(out var invalid)) { SetStatus(invalid, true); return; }
            if (!Recompose()) { return; }

            // The placement jig needs the editor free, so only flag the request and close; the host command
            // draws the system once every modal window is gone.
            currentName = NameBox?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(currentId)) currentId = Guid.NewGuid().ToString();

            InsertRequested = true;
            UpdateOnly = updateOnly;
            InsertView = updateOnly ? null : view;
            InsertSection = updateOnly ? -1 : section;
            SystemToInsert = system;
            DesignToInsert = design;
            RackId = currentId;
            RackName = currentName;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private sealed class DynamicFrontRow
        {
            public int Index { get; set; }
            public int PalletCount { get; set; }
            public int LoadLevels { get; set; }
            public int PalletsDeep { get; set; }
            public int DepthStartPosition { get; set; } = 1;
            public double FirstLevelHeight { get; set; } = DynamicRackDefaults.DefaultFirstLevelHeight;
            public double Bfr { get; set; }
            public double BeamLength { get; set; }
            public List<DynamicCellRow> Cells { get; } = new List<DynamicCellRow>();
        }

        private sealed class DynamicCellRow
        {
            public double PalletFront { get; set; } = 42.0;
            public double PalletHeight { get; set; } = 60.0;
            public double PalletWeight { get; set; } = 1000.0;
            public double ClearHeight { get; set; } = DynamicRackDefaults.DefaultClearHeight;
            public string InOutBeamCatalogId { get; set; } = DynamicRackDefaults.InOutBeamCatalogId;
            public double InOutBeamDepth { get; set; } = DynamicRackDefaults.DefaultBeamDepth;
            public double? BeamLengthOverride { get; set; }
            public string IntermediateBeamCatalogId { get; set; } = DynamicRackDefaults.IntermediateBeamCatalogId;
            public double IntermediateBeamDepth { get; set; } = DynamicRackDefaults.DefaultIntermediateBeamDepth;

            public bool IsValid => PalletFront > 0.0 && PalletHeight > 0.0 && PalletWeight >= 0.0
                                   && ClearHeight >= 0.0 && InOutBeamDepth > 0.0
                                   && IntermediateBeamDepth > 0.0
                                   && (!BeamLengthOverride.HasValue || BeamLengthOverride.Value > 0.0);

            public DynamicRackLevelDesign ToDesign()
                => new DynamicRackLevelDesign
                {
                    PalletFront = PalletFront,
                    PalletHeight = PalletHeight,
                    PalletWeight = PalletWeight,
                    ClearHeight = ClearHeight,
                    InOutBeamCatalogId = InOutBeamCatalogId,
                    InOutBeamDepth = InOutBeamDepth,
                    BeamLengthOverride = BeamLengthOverride,
                    IntermediateBeamCatalogId = IntermediateBeamCatalogId,
                    IntermediateBeamDepth = IntermediateBeamDepth
                };

            public DynamicCellRow Clone()
                => new DynamicCellRow
                {
                    PalletFront = PalletFront,
                    PalletHeight = PalletHeight,
                    PalletWeight = PalletWeight,
                    ClearHeight = ClearHeight,
                    InOutBeamCatalogId = InOutBeamCatalogId,
                    InOutBeamDepth = InOutBeamDepth,
                    BeamLengthOverride = BeamLengthOverride,
                    IntermediateBeamCatalogId = IntermediateBeamCatalogId,
                    IntermediateBeamDepth = IntermediateBeamDepth
                };

            public static DynamicCellRow Default() => new DynamicCellRow();

            public static DynamicCellRow From(DynamicRackLevel level)
                => level == null
                    ? Default()
                    : new DynamicCellRow
                    {
                        PalletFront = level.Pallet?.Front ?? 42.0,
                        PalletHeight = level.Pallet?.Height ?? 60.0,
                        PalletWeight = level.Pallet?.Weight ?? 0.0,
                        ClearHeight = level.ClearHeight,
                        InOutBeamCatalogId = level.InOutBeamCatalogId,
                        InOutBeamDepth = level.InOutBeamDepth,
                        BeamLengthOverride = level.BeamLengthOverride,
                        IntermediateBeamCatalogId = level.IntermediateBeamCatalogId,
                        IntermediateBeamDepth = level.IntermediateBeamDepth
                    };
        }

        private sealed class DynamicEditorValues
        {
            public int PalletCount { get; set; }
            public int LoadLevels { get; set; }
            public int PalletsDeep { get; set; }
            public int DepthStartPosition { get; set; }
            public double? BeamLengthOverride { get; set; }
            public double FirstLevelHeight { get; set; }
            public double PalletFront { get; set; }
            public double PalletHeight { get; set; }
            public double PalletWeight { get; set; }
            public double ClearHeight { get; set; }
            public string InOutBeamCatalogId { get; set; }
            public double InOutBeamDepth { get; set; }
            public string IntermediateBeamCatalogId { get; set; }
            public double IntermediateBeamDepth { get; set; }
        }

        private enum DynamicPreviewMode
        {
            Lateral,
            FrontalExit,
            FrontalEntrance
        }

        // ---- Helpers ----

        private void SetStatus(string message, bool isError) => UiSupport.SetStatus(StatusText, message, isError);

        private static string Num(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static bool TryNum(string text, out double value) => UiSupport.TryNum(text, out value);
    }
}
