using System;
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
        private const string DefaultPostCatalogId = "POSTE_OMEGA_3X3";
        private const double DefaultHeaderHeight = 132.0;

        private static readonly Brush UprightStroke = new SolidColorBrush(Color.FromRgb(0xCF, 0xDB, 0xE8));
        private static readonly Brush HorizontalStroke = new SolidColorBrush(Color.FromRgb(0x3D, 0xC9, 0x86));
        private static readonly Brush DiagonalStroke = new SolidColorBrush(Color.FromRgb(0x5B, 0x8D, 0xEF));
        private static readonly Brush SeparatorStroke = new SolidColorBrush(Color.FromRgb(0x3A, 0x50, 0x68));
        private static readonly Brush PostStroke = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
        private static readonly Brush FloorStroke = new SolidColorBrush(Color.FromRgb(0x6A, 0x7B, 0x8A));
        private static readonly Brush LabelStroke = new SolidColorBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));
        private static readonly Brush SelectionStroke = new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x66));

        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemBuilder builder;
        private DynamicRackSystem system;
        private DynamicRackModule selectedModule;

        private double mapScale;
        private double mapOffsetX;
        private double mapBottomY;

        public RackDynamicSystemWindow()
        {
            InitializeComponent();
            catalog = LoadCatalogSafe();
            builder = new DynamicRackSystemBuilder(catalog);
            KindBox.ItemsSource = Enum.GetValues(typeof(DynamicRackModuleKind));
            Recompose();
        }

        // ---- Build / edit ----

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Recompose();
        }

        private void Recompose()
        {
            if (!TryReadInputs(out var pallet, out var palletsDeep, out var error))
            {
                SetStatus(error, true);
                return;
            }

            try
            {
                system = builder.BuildDefault(pallet, palletsDeep, RackFrameTemplateCatalog.Default, DefaultPostCatalogId, DefaultHeaderHeight);
                selectedModule = null;
                BindModules();
                UpdateSelectedPanel();
                UpdateSummary();
                DrawSideView();
                SetStatus("Vista recalculada (layout estandar).", false);
            }
            catch (Exception ex)
            {
                system = null;
                ModulesGrid.ItemsSource = null;
                PreviewCanvas.Children.Clear();
                SetStatus("No se pudo generar el sistema: " + ex.Message, true);
            }
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

            if (!TryNum(ModuleLengthBox.Text, out var length) || length < 0)
            {
                SetStatus("Longitud invalida.", true);
                return;
            }

            var kind = KindBox.SelectedItem is DynamicRackModuleKind k ? k : selectedModule.Kind;

            selectedModule.Kind = kind;
            selectedModule.Length = length;
            selectedModule.IsManualOverride = true;
            selectedModule.IsCalculated = false;

            if (selectedModule.IsHeader && selectedModule.AssociatedFrameConfiguration == null)
            {
                selectedModule.AssociatedFrameConfiguration = BuildHeaderConfig(Math.Max(length, 1.0));
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

            var window = new RackFrameConfiguratorWindow(selectedModule.AssociatedFrameConfiguration) { Owner = this };
            window.ShowDialog();

            builder.Refresh(system);
            BindModules();
            UpdateSummary();
            DrawSideView();
            SetStatus("Cabecera del modulo actualizada.", false);
        }

        private RackFrameConfiguration BuildHeaderConfig(double depth)
        {
            return new RackFrameConfigurationFactory(catalog)
                .Build(RackFrameTemplateCatalog.Default, DefaultPostCatalogId, DefaultHeaderHeight, depth);
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

            KindBox.SelectedItem = selectedModule.Kind;
            ModuleLengthBox.Text = selectedModule.Length.ToString("0.##", CultureInfo.InvariantCulture);
            ApplyModuleButton.IsEnabled = true;
            EditHeaderButton.IsEnabled = selectedModule.IsHeader;
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
            var posts = system.Modules.Count(m => m.Kind == DynamicRackModuleKind.IntermediatePost);

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Longitud total: {0:0.##} in   (regla: N x fondo + 12)\nModulos con longitud: {1}   |   {2} cabeceras, {3} separadores, {4} postes",
                system.TotalLength,
                system.LengthBearingModuleCount,
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

            const double margin = 40.0;
            mapScale = Math.Min((availableWidth - 2 * margin) / total, (availableHeight - 2 * margin) / height);
            if (mapScale <= 0)
            {
                return;
            }

            var drawHeight = height * mapScale;
            mapOffsetX = (availableWidth - total * mapScale) / 2.0;
            mapBottomY = (availableHeight - drawHeight) / 2.0 + drawHeight;

            AddLine(Map(0, 0), Map(total, 0), FloorStroke, 1.5);

            foreach (var module in system.Modules)
            {
                if (module.Kind == DynamicRackModuleKind.IntermediatePost)
                {
                    AddLine(Map(module.StartX, 0), Map(module.StartX, height), PostStroke, 1.8, Dash());
                    continue;
                }

                if (module.IsHeader)
                {
                    DrawHeader(module, height);
                }
                else
                {
                    var topLeft = Map(module.StartX, height);
                    AddRectangle(topLeft.X, topLeft.Y, module.Length * mapScale, drawHeight, SeparatorStroke, 1.0, Dash());
                }

                AddLengthLabel(module);
            }

            DrawSelectionHighlight(height, drawHeight);
            AddCanvasLabel(mapOffsetX, mapBottomY + 14, "Longitud total: " + total.ToString("0.##", CultureInfo.InvariantCulture) + " in", LabelStroke, 12);
        }

        private void DrawHeader(DynamicRackModule module, double height)
        {
            // Two uprights (posts) at the module edges.
            AddLine(Map(module.StartX, 0), Map(module.StartX, height), UprightStroke, 2.2);
            AddLine(Map(module.EndX, 0), Map(module.EndX, height), UprightStroke, 2.2);

            var config = module.AssociatedFrameConfiguration;
            if (config == null)
            {
                return;
            }

            var depth = config.Depth <= 0 ? module.Length : config.Depth;

            foreach (var member in config.Members)
            {
                if (member?.Start == null || member.End == null)
                {
                    continue;
                }

                var sx = module.StartX + member.Start.HorizontalPositionRatio * depth;
                var ex = module.StartX + member.End.HorizontalPositionRatio * depth;
                AddLine(Map(sx, member.Start.Elevation), Map(ex, member.End.Elevation), MemberBrush(member.MemberType), 1.4);
            }
        }

        private void DrawSelectionHighlight(double height, double drawHeight)
        {
            if (selectedModule == null)
            {
                return;
            }

            if (selectedModule.Kind == DynamicRackModuleKind.IntermediatePost)
            {
                AddLine(Map(selectedModule.StartX, 0), Map(selectedModule.StartX, height), SelectionStroke, 2.6, Dash());
                return;
            }

            var topLeft = Map(selectedModule.StartX, height);
            AddRectangle(topLeft.X - 1, topLeft.Y - 1, Math.Max(selectedModule.Length * mapScale, 2) + 2, drawHeight + 2, SelectionStroke, 2.2, null);
        }

        private void AddLengthLabel(DynamicRackModule module)
        {
            var centerX = (module.StartX + module.EndX) / 2.0;
            var text = module.Kind == DynamicRackModuleKind.IntermediatePost
                ? "poste"
                : module.Length.ToString("0.##", CultureInfo.InvariantCulture);
            AddCanvasLabel(Map(centerX, 0).X - 12, mapBottomY + 2, text, LabelStroke, 11);
        }

        private static Brush MemberBrush(FrameMemberType type)
        {
            return type == FrameMemberType.DiagonalBrace ? DiagonalStroke : HorizontalStroke;
        }

        private double HeaderHeight()
        {
            var header = system?.Modules.FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration != null);
            return header?.AssociatedFrameConfiguration.Height ?? DefaultHeaderHeight;
        }

        private Point Map(double x, double y)
        {
            return new Point(mapOffsetX + x * mapScale, mapBottomY - y * mapScale);
        }

        private void AddLine(Point a, Point b, Brush stroke, double thickness, DoubleCollection dash = null)
        {
            PreviewCanvas.Children.Add(new Line { X1 = a.X, Y1 = a.Y, X2 = b.X, Y2 = b.Y, Stroke = stroke, StrokeThickness = thickness, StrokeDashArray = dash });
        }

        private void AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, DoubleCollection dash)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var rectangle = new Rectangle { Width = width, Height = height, Stroke = stroke, StrokeThickness = thickness, StrokeDashArray = dash, Fill = Brushes.Transparent };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            PreviewCanvas.Children.Add(rectangle);
        }

        private void AddCanvasLabel(double left, double top, string text, Brush brush, double size)
        {
            var label = new TextBlock { Text = text, Foreground = brush, FontSize = size };
            Canvas.SetLeft(label, left);
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

                system = project.DynamicSystem;
                selectedModule = null;
                FrontBox.Text = Num(system.Pallet.Front);
                DepthBox.Text = Num(system.Pallet.Depth);
                PalletHeightBox.Text = Num(system.Pallet.Height);
                WeightBox.Text = Num(system.Pallet.Weight);
                PalletsDeepBox.Text = system.PalletsDeep.ToString(CultureInfo.InvariantCulture);
                BindModules();
                UpdateSelectedPanel();
                UpdateSummary();
                DrawSideView();
                SetStatus("Sistema abierto: " + System.IO.Path.GetFileName(dialog.FileName), false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo abrir: " + ex.Message, true);
            }
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

        private static bool TryNum(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value)
                || double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
        }

        private static RackCatalog LoadCatalogSafe()
        {
            try
            {
                return JsonRackCatalogProvider.FromBaseDirectory().Load();
            }
            catch
            {
                return new RackCatalog();
            }
        }
    }
}
