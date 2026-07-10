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

        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemBuilder builder;
        private readonly string defaultPostCatalogId;
        private readonly double defaultHeaderHeight;
        private double computedHeaderHeight;
        private DynamicRackSystem system;
        private DynamicRackModule selectedModule;

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

        /// <summary>Stable id + client name of the system for the drawing round-trip (embed / reopen / edit).</summary>
        public string RackId { get; private set; }
        public string RackName { get; private set; }

        private string currentId;
        private string currentName;

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
            defaultPostCatalogId = catalog.Defaults.Post;
            defaultHeaderHeight = catalog.Defaults.DefaultHeaderHeight;
            computedHeaderHeight = defaultHeaderHeight;
            PostBox.ItemsSource = BuildPostOptions();
            PostBox.SelectedValue = defaultPostCatalogId;
            KindBox.ItemsSource = new[] { KindHeader, KindSeparator };
            RefreshConfigBox();
            Recompose();
        }

        /// <summary>Post-type options (DisplayName shown, Id stored) for the basic "Tipo de poste" combo.</summary>
        private List<CatalogOption> BuildPostOptions() => UiSupport.ToOptions(catalog?.PostProfiles);

        /// <summary>The selected post id, or the catalog default if the combo has no selection yet.</summary>
        private string SelectedPostId() => PostBox?.SelectedValue as string ?? defaultPostCatalogId;

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

        private void Recompose(bool forceRebuild = false)
        {
            if (!TryReadInputs(out var pallet, out var palletsDeep, out var error))
            {
                SetStatus(error, true);
                return;
            }

            try
            {
                computedHeaderHeight = ComputeHeaderHeight(pallet, palletsDeep);

                // A full rebuild (BuildDefault, from scratch) is only needed when the pallet or the number of fondos
                // changes — that changes the module SEQUENCE. When only the height inputs change (niveles / peralte /
                // 1er nivel / altura manual, or the post), update the header height IN PLACE so the per-module edits
                // (custom fondo/length, cabeceras) survive instead of reverting to the calculated defaults.
                var mustRebuild = forceRebuild || system == null || system.Modules.Count == 0
                    || !SamePallet(system.Pallet, pallet) || system.PalletsDeep != palletsDeep;

                if (mustRebuild)
                {
                    system = builder.BuildDefault(pallet, palletsDeep, RackFrameTemplateCatalog.Default, SelectedPostId(), computedHeaderHeight);
                }
                else
                {
                    UpdateHeaderHeightInPlace(computedHeaderHeight);
                }

                ApplySeparatorOverrides();
                ApplyDerivedPostOptions();
                ApplyHeightOverride();
                selectedModule = null;
                BindModules();
                UpdateSelectedPanel();
                UpdateSummary();
                DrawSideView();
                SetStatus(mustRebuild
                    ? "Vista recalculada (layout estandar)."
                    : "Altura actualizada; se conservaron los modulos (fondos y cabeceras).", false);
            }
            catch (Exception ex)
            {
                system = null;
                ModulesGrid.ItemsSource = null;
                PreviewCanvas.Children.Clear();
                SetStatus("No se pudo generar el sistema: " + ex.Message, true);
            }
        }

        /// <summary>
        /// Update the header height on the EXISTING modules (rebuild each header config at the new height but its
        /// CURRENT fondo), so a levels/height change keeps the module lengths and sequence — the fondo the user set on
        /// a cabecera survives. Deep structural cabecera edits are rebuilt to the standard for the new height.
        /// </summary>
        private void UpdateHeaderHeightInPlace(double newHeight)
        {
            var factory = new RackFrameConfigurationFactory(catalog);
            var postId = SelectedPostId();

            foreach (var module in system.Modules)
            {
                if (!module.IsHeader)
                {
                    continue;
                }

                var fondo = module.Length > 0.0 ? module.Length : system.DefaultHeaderLength;
                module.AssociatedFrameConfiguration = factory.Build(RackFrameTemplateCatalog.Default, postId, newHeight, fondo);
            }

            builder.Refresh(system);
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
                SetStatus("Longitud invalida (debe ser mayor que cero).", true);
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
                }
            }
            else
            {
                selectedModule.Kind = DynamicRackModuleKind.Separator;
                selectedModule.AssociatedFrameConfiguration = null;
            }

            builder.Refresh(system);
            BindModules();
            UpdateSelectedPanel();
            UpdateSummary();
            DrawSideView();
            SetStatus("Modulo actualizado.", false);
        }

        private void EditHeader_Click(object sender, RoutedEventArgs e)
        {
            if (selectedModule == null || !selectedModule.IsHeader)
            {
                SetStatus("Selecciona un modulo de cabecera para editarlo.", true);
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

            var afterEdit = new RackProjectStore().Serialize(RackProject.ForSelective(selectedModule.AssociatedFrameConfiguration));
            if (afterEdit == beforeEdit)
            {
                SetStatus("Cabecera sin cambios.", false);
                return;
            }

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
            SetStatus("Cabecera del modulo actualizada (fondo " + editedDepth.ToString("0.##", CultureInfo.InvariantCulture) + " in).", false);
        }

        private void RestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            // Explicit "restore standard": a full rebuild that DOES discard the per-module overrides.
            Recompose(forceRebuild: true);
        }

        private RackFrameConfiguration BuildHeaderConfig(double depth)
        {
            return new RackFrameConfigurationFactory(catalog)
                .Build(RackFrameTemplateCatalog.Default, SelectedPostId(), computedHeaderHeight, depth);
        }

        /// <summary>
        /// Computes the header height from the load inputs (DynamicHeaderHeightCalculator) and shows it.
        /// Load height = the pallet height; total depth (the slope run) = the full run length we already
        /// derive (tarimas x fondo + 12"). Levels/first-level/beam-depth come from the new fields.
        /// </summary>
        private double ComputeHeaderHeight(PalletSpecification pallet, int palletsDeep)
        {
            var levels = int.TryParse(LoadLevelsBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n >= 1 ? n : 1;
            var firstLevel = TryNum(FirstLevelHeightBox.Text, out var f) && f >= 0.0 ? f : 0.0;
            var beamDepth = TryNum(BeamDepthBox.Text, out var b) && b >= 0.0 ? b : 0.0;
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

            if (!TryNum(FrontBox.Text, out var front) || front <= 0) { error = "Frente invalido."; return false; }
            if (!TryNum(DepthBox.Text, out var depth) || depth <= 0) { error = "Fondo invalido."; return false; }
            if (!TryNum(PalletHeightBox.Text, out var palletHeight) || palletHeight <= 0) { error = "Altura de tarima invalida."; return false; }
            if (!TryNum(WeightBox.Text, out var weight) || weight < 0) { error = "Peso invalido."; return false; }
            if (!int.TryParse(PalletsDeepBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out palletsDeep) || palletsDeep < 2)
            {
                error = "Las tarimas de fondo deben ser un entero >= 2.";
                return false;
            }

            pallet = new PalletSpecification(front, depth, palletHeight, weight, "kg");
            return true;
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

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Longitud total: {0:0.##} in   (regla: N x fondo + 12)\nModulos: {1}   |   {2} cabeceras, {3} separadores   |   postes derivados: {4}",
                system.TotalLength,
                system.Modules.Count,
                headers,
                separators,
                posts);
        }

        // ---- Drawing ----

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSideView();
        }

        private void PreviewCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (system == null || mapScale <= 0)
            {
                return;
            }

            var worldX = (e.GetPosition(PreviewCanvas).X - mapOffsetX) / mapScale;
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
            PreviewCanvas.Children.Clear();

            if (system == null)
            {
                return;
            }

            var total = system.TotalLength;
            var height = HeaderHeight();
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
            mapOffsetX = (availableWidth - total * mapScale) / 2.0;
            mapBottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;

            AddCanvasLabel(mapOffsetX, Math.Max(4.0, mapBottomY - drawHeight - 24.0), "Longitud total: " + total.ToString("0.##", CultureInfo.InvariantCulture) + " in", LabelStroke, 12, 260.0, FontWeights.SemiBold);
            AddLine(Map(0, 0), Map(total, 0), FloorStroke, 1.5);

            var separatorLevels = SeparatorLevelCalculator.Levels(
                height, SeparatorBaseY(), PasoTroquel(), system.SeparatorCountOverride, system.SeparatorSpacingOverride);
            var headerOrdinal = 0;

            foreach (var module in system.Modules.Where(m => m.Length > 0.0))
            {
                if (module.IsHeader)
                {
                    // Every other header is mirrored so the celosía alternates along the line (matches AutoCAD).
                    DrawHeader(module, height, headerOrdinal % 2 == 1);
                    headerOrdinal++;
                }
                else
                {
                    DrawSeparator(module, height, drawHeight, separatorLevels);
                }

                AddLengthLabel(module);
            }

            DrawTotalDimension(total);

            // Derived intermediate posts: markers only, never primary editable modules.
            foreach (var postX in system.GetDerivedPostOffsets())
            {
                DrawDerivedPost(postX, height);
            }

            DrawSelectionHighlight(height, drawHeight);
        }

        private void DrawHeader(DynamicRackModule module, double height, bool mirrored)
        {
            var topLeft = Map(module.StartX, height);
            var moduleWidth = module.Length * mapScale;
            var moduleHeight = height * mapScale;
            AddRectangle(topLeft.X, topLeft.Y, moduleWidth, moduleHeight, ModuleBoundaryStroke, 1.0, null, HeaderFill);
            AddLine(Map(module.StartX, height), Map(module.EndX, height), ModuleBoundaryStroke, 1.0);
            AddLine(Map(module.StartX, 0), Map(module.EndX, 0), ModuleBoundaryStroke, 1.0);
            var config = module.AssociatedFrameConfiguration;
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
                int.TryParse(SeparatorCountBox.Text?.Trim(), out var count) && count >= 1 ? count : (int?)null;
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

            var count = SeparatorCountBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(count) && !(int.TryParse(count, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n >= 1))
            {
                error = "Cantidad de separadores invalida (entero >= 1, o vacio para estandar).";
                return false;
            }

            if (!UiSupport.TryOptionalNum(SeparatorSpacingBox.Text, out _))
            {
                error = "Separacion de separadores invalida (deja vacio para estandar).";
                return false;
            }

            if (!UiSupport.TryOptionalNum(DerivedReinforcementBox.Text, out _))
            {
                error = "Altura de refuerzo del poste derivado invalida (deja vacio para altura completa).";
                return false;
            }

            if (!IsBlankOrNumber(FirstLevelHeightBox.Text))
            {
                error = "Altura del primer nivel invalida (deja vacio para 0).";
                return false;
            }

            if (!IsBlankOrNumber(BeamDepthBox.Text))
            {
                error = "Peralte de viga invalido (deja vacio para 0).";
                return false;
            }

            if (ManualHeightToggle?.IsChecked == true && !(UiSupport.TryNum(ManualHeightBox.Text, out var m) && m > 0.0))
            {
                error = "Altura manual invalida (debe ser mayor que cero).";
                return false;
            }

            return true;
        }

        private static bool IsBlankOrNumber(string text) => string.IsNullOrWhiteSpace(text) || UiSupport.TryNum(text, out _);

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

        private void DrawDerivedPost(double x, double height)
        {
            var top = Map(x, height).Y;
            var bottom = Map(x, 0).Y;
            var canvasX = Map(x, 0).X;

            AddLine(new Point(canvasX, top), new Point(canvasX, bottom), PostStroke, 2.4, Dash());
            AddLine(new Point(canvasX - 8.0, top + 8.0), new Point(canvasX + 8.0, top + 8.0), PostStroke, 1.3);
            AddLine(new Point(canvasX - 8.0, bottom - 8.0), new Point(canvasX + 8.0, bottom - 8.0), PostStroke, 1.3);

            if (system?.DerivedPostReinforced == true)
            {
                var reinforcementHeight = system.DerivedPostReinforcementHeight is double rh && rh > 0.0 ? rh : height;
                var reinforcementTop = Map(x, Math.Min(reinforcementHeight, height)).Y;
                var reinforcementX = canvasX + 6.0;
                AddLine(new Point(reinforcementX, reinforcementTop + 1.0), new Point(reinforcementX, bottom - 1.0), ReinforcementStroke, 3.4);
                AddLine(new Point(reinforcementX - 3.0, bottom - 10.0), new Point(reinforcementX + 3.0, bottom - 10.0), ReinforcementStroke, 1.0);
            }

            AddCenteredLabel(canvasX, mapBottomY + 34.0, "poste", 52.0, PostStroke, 9.5, null);
        }

        private void DrawSelectionHighlight(double height, double drawHeight)
        {
            if (selectedModule == null || selectedModule.Length <= 0.0)
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

        private void DrawTotalDimension(double total)
        {
            var y = mapBottomY + 56.0;
            var start = Map(0, 0).X;
            var end = Map(total, 0).X;
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

            // Commit the advanced fields onto the model first: they only reach `system` on Apply*, so saving without
            // a prior recompose used to persist stale defaults and lose the user's customizations on reopen.
            ApplySeparatorOverrides();
            ApplyDerivedPostOptions();
            ApplyHeightOverride();

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Proyecto RackCad (*.rackcad.json)|*.rackcad.json|JSON (*.json)|*.json",
                FileName = "sistema" + RackProjectStore.FileExtension
            };
            if (dialog.ShowDialog(this) != true) { return; }

            try
            {
                new RackProjectStore().Save(RackProject.ForDynamic(system), dialog.FileName);
                SetStatus("Sistema guardado: " + System.IO.Path.GetFileName(dialog.FileName), false);
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
                if (project.Kind != RackSystemKind.PalletFlow || project.DynamicSystem == null)
                {
                    SetStatus("El archivo no es un sistema dinamico.", true);
                    return;
                }

                RestoreFrom(project.DynamicSystem);
                SetStatus("Sistema abierto: " + System.IO.Path.GetFileName(dialog.FileName), false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo abrir: " + ex.Message, true);
            }
        }

        /// <summary>Restore the whole editor state from a loaded dynamic system (shared by open-file and round-trip edit).</summary>
        private void RestoreFrom(DynamicRackSystem loaded)
        {
            system = loaded;
            selectedModule = null;
            suppressRecompose = true;
            try
            {
                FrontBox.Text = Num(system.Pallet.Front);
                DepthBox.Text = Num(system.Pallet.Depth);
                PalletHeightBox.Text = Num(system.Pallet.Height);
                WeightBox.Text = Num(system.Pallet.Weight);
                PalletsDeepBox.Text = system.PalletsDeep.ToString(CultureInfo.InvariantCulture);

                SeparatorCountBox.Text = system.SeparatorCountOverride?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                SeparatorSpacingBox.Text = system.SeparatorSpacingOverride.HasValue ? Num(system.SeparatorSpacingOverride.Value) : string.Empty;
                DerivedReinforceBox.IsChecked = system.DerivedPostReinforced;
                DerivedReinforcementBox.Text = system.DerivedPostReinforcementHeight.HasValue ? Num(system.DerivedPostReinforcementHeight.Value) : string.Empty;

                ManualHeightToggle.IsChecked = system.ManualHeaderHeightOverride.HasValue;
                ManualHeightBox.Text = system.ManualHeaderHeightOverride.HasValue ? Num(system.ManualHeaderHeightOverride.Value) : string.Empty;
                if (system.ManualHeaderHeightOverride.HasValue) computedHeaderHeight = system.ManualHeaderHeightOverride.Value;

                var loadedPostId = system.Modules
                    .FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration?.LeftPost != null)?
                    .AssociatedFrameConfiguration.LeftPost.PostCatalogId;
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
            DrawSideView();
        }

        /// <summary>Open the editor pre-loaded with an existing drawn system (from its embedded payload), keeping Id/Name.</summary>
        public void LoadExisting(DynamicRackSystem loaded, string id, string name)
        {
            if (loaded == null)
            {
                return;
            }

            currentId = id;
            currentName = name;
            if (NameBox != null)
            {
                NameBox.Text = name ?? string.Empty;
            }

            // Editing an existing system: the draw button redraws it in place (all copies share the definition+GUID and
            // update), so it reads "Actualizar". A brand-new system still "Insertar".
            if (InsertButton != null)
            {
                InsertButton.Content = "Actualizar en AutoCAD";
                InsertButton.ToolTip = "Redibuja el sistema en sitio con tus cambios; todas sus copias en el dibujo se actualizan.";
            }

            RestoreFrom(loaded);
        }

        private void InsertInAutoCad_Click(object sender, RoutedEventArgs e)
        {
            if (!canInsertInAutoCad)
            {
                SetStatus("El dibujo en AutoCAD solo esta disponible cuando el sistema se abre desde AutoCAD.", true);
                return;
            }

            if (system == null)
            {
                SetStatus("Genera la vista antes de insertar en AutoCAD.", true);
                return;
            }

            if (!TryValidateOptionalInputs(out var invalid)) { SetStatus(invalid, true); return; }

            // Commit the advanced override fields onto the model before embedding it (same reason as SaveSystem):
            // otherwise a customization not followed by "Actualizar vista" is lost in the .dwg round-trip.
            ApplySeparatorOverrides();
            ApplyDerivedPostOptions();
            ApplyHeightOverride();

            // The placement jig needs the editor free, so only flag the request and close; the host command
            // draws the system once every modal window is gone.
            currentName = NameBox?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(currentId)) currentId = Guid.NewGuid().ToString();

            InsertRequested = true;
            SystemToInsert = system;
            RackId = currentId;
            RackName = currentName;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ---- Helpers ----

        private void SetStatus(string message, bool isError)
        {
            StatusText.Text = message;
            StatusText.Foreground = new SolidColorBrush(isError ? Color.FromRgb(0xB0, 0x00, 0x20) : Color.FromRgb(0x2F, 0x85, 0x5A));
        }

        private static string Num(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static bool TryNum(string text, out double value) => UiSupport.TryNum(text, out value);
    }
}
