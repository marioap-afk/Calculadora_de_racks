using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
    /// Independent module: preliminary side view of a dynamic (pallet flow) system. It builds its
    /// own header through the header FACTORY (reusing that logic without coupling to the header
    /// configurator window). UI only; layout/BOM logic lives in the Application layer.
    /// </summary>
    public partial class RackDynamicSystemWindow : Window
    {
        private const string DefaultPostCatalogId = "POSTE_OMEGA_3X3";
        private const double DefaultHeaderHeight = 132.0;

        private static readonly Brush HeaderMember = new SolidColorBrush(Color.FromRgb(0x7F, 0xB3, 0xFF));
        private static readonly Brush HeaderOutline = new SolidColorBrush(Color.FromRgb(0x3A, 0x55, 0x7A));
        private static readonly Brush SeparatorOutline = new SolidColorBrush(Color.FromRgb(0x3A, 0x50, 0x68));
        private static readonly Brush PostStroke = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
        private static readonly Brush FloorStroke = new SolidColorBrush(Color.FromRgb(0x6A, 0x7B, 0x8A));

        private readonly RackCatalog catalog;
        private RackFrameConfiguration header;
        private ComposedDynamicRack composed;

        public RackDynamicSystemWindow()
        {
            InitializeComponent();
            catalog = LoadCatalogSafe();
            header = BuildStandardHeader();
            UpdateHeaderInfo();
            Recompose();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Recompose();
        }

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSideView();
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
                var system = new DynamicRackSystem
                {
                    Kind = RackSystemKind.PalletFlow,
                    Pallet = pallet,
                    PalletsDeep = palletsDeep,
                    Header = header
                };

                composed = new DynamicRackComposer().Compose(system);
                UpdateSummary();
                DrawSideView();
                SetStatus("Vista actualizada.", false);
            }
            catch (Exception ex)
            {
                composed = null;
                PreviewCanvas.Children.Clear();
                SetStatus("No se pudo generar el sistema: " + ex.Message, true);
            }
        }

        private bool TryReadInputs(out PalletSpecification pallet, out int palletsDeep, out string error)
        {
            pallet = null;
            palletsDeep = 0;
            error = null;

            if (!TryNum(FrontBox.Text, out var front) || front <= 0)
            {
                error = "Frente invalido.";
                return false;
            }

            if (!TryNum(DepthBox.Text, out var depth) || depth <= 0)
            {
                error = "Fondo invalido.";
                return false;
            }

            if (!TryNum(PalletHeightBox.Text, out var palletHeight) || palletHeight <= 0)
            {
                error = "Altura de tarima invalida.";
                return false;
            }

            if (!TryNum(WeightBox.Text, out var weight) || weight < 0)
            {
                error = "Peso invalido.";
                return false;
            }

            if (!int.TryParse(PalletsDeepBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out palletsDeep) || palletsDeep < 2)
            {
                error = "Las tarimas de fondo deben ser un entero >= 2.";
                return false;
            }

            pallet = new PalletSpecification(front, depth, palletHeight, weight, "kg");
            return true;
        }

        private void UpdateSummary()
        {
            if (composed == null)
            {
                SummaryText.Text = string.Empty;
                return;
            }

            var modules = composed.Layout.Modules;
            var separators = modules.Count(m => m.Kind == RackModuleKind.Separator);

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Longitud total: {0:0.##} in\nProfundidad de cabecera: {1:0.##} in\nModulos: {2}  (2 cabeceras, {3} separadores)\nPostes intermedios: {4}",
                composed.Layout.TotalLength,
                composed.System.EffectiveHeaderDepth,
                modules.Count,
                separators,
                composed.Layout.IntermediatePosts.Count);
        }

        private void UpdateHeaderInfo()
        {
            if (header == null)
            {
                HeaderInfoText.Text = string.Empty;
                return;
            }

            HeaderInfoText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0}\nAlto {1:0.##} in - {2} horizontales - {3} paneles",
                header.Name,
                header.Height,
                header.Horizontals.Count,
                header.BracingPanels.Count);
        }

        private void DrawSideView()
        {
            PreviewCanvas.Children.Clear();

            if (composed == null)
            {
                return;
            }

            var total = composed.Layout.TotalLength;
            var height = composed.System.Header?.Height ?? 0.0;
            var availableWidth = PreviewCanvas.ActualWidth;
            var availableHeight = PreviewCanvas.ActualHeight;

            if (total <= 0 || height <= 0 || availableWidth < 20 || availableHeight < 20)
            {
                return;
            }

            const double margin = 28.0;
            var scale = Math.Min((availableWidth - 2 * margin) / total, (availableHeight - 2 * margin) / height);
            if (scale <= 0)
            {
                return;
            }

            var drawWidth = total * scale;
            var drawHeight = height * scale;
            var offsetX = (availableWidth - drawWidth) / 2.0;
            var bottomY = (availableHeight - drawHeight) / 2.0 + drawHeight;

            Point Map(double x, double y) => new Point(offsetX + x * scale, bottomY - y * scale);

            // Floor baseline.
            AddLine(Map(0, 0), Map(total, 0), FloorStroke, 1.5);

            var depth = composed.System.EffectiveHeaderDepth;

            foreach (var placed in composed.PlacedModules)
            {
                var module = placed.Module;
                var topLeft = Map(module.StartOffset, height);
                AddRectangle(topLeft.X, topLeft.Y, module.Length * scale, drawHeight,
                    placed.IsHeader ? HeaderOutline : SeparatorOutline, 1.0, placed.IsHeader ? null : Dash());

                if (!placed.IsHeader || placed.Header == null)
                {
                    continue;
                }

                foreach (var member in placed.Header.Members)
                {
                    if (member?.Start == null || member.End == null)
                    {
                        continue;
                    }

                    var sx = placed.Placement.OffsetX + member.Start.HorizontalPositionRatio * depth;
                    var ex = placed.Placement.OffsetX + member.End.HorizontalPositionRatio * depth;
                    AddLine(Map(sx, member.Start.Elevation), Map(ex, member.End.Elevation), HeaderMember, 1.4);
                }
            }

            // Intermediate posts are markers (a vertical line), not modules.
            foreach (var postX in composed.Layout.IntermediatePosts)
            {
                AddLine(Map(postX, 0), Map(postX, height), PostStroke, 1.6, Dash());
            }
        }

        private void AddLine(Point a, Point b, Brush stroke, double thickness, DoubleCollection dash = null)
        {
            PreviewCanvas.Children.Add(new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeDashArray = dash
            });
        }

        private void AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, DoubleCollection dash)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeDashArray = dash,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            PreviewCanvas.Children.Add(rectangle);
        }

        private static DoubleCollection Dash()
        {
            return new DoubleCollection { 5, 3 };
        }

        private void ExportBom_Click(object sender, RoutedEventArgs e)
        {
            if (composed == null)
            {
                SetStatus("Genera la vista antes de exportar el BOM.", true);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv|Todos (*.*)|*.*",
                FileName = "bom-sistema.csv"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                System.IO.File.WriteAllText(dialog.FileName, BomCsvExporter.ToCsv(SystemBomBuilder.Build(composed, catalog)));
                SetStatus("BOM del sistema exportado.", false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo exportar el BOM: " + ex.Message, true);
            }
        }

        private void ViewBom_Click(object sender, RoutedEventArgs e)
        {
            if (composed == null)
            {
                SetStatus("Genera la vista antes de ver el BOM.", true);
                return;
            }

            var window = new RackBomWindow(SystemBomBuilder.Build(composed, catalog)) { Owner = this };
            window.ShowDialog();
        }

        private void SaveSystem_Click(object sender, RoutedEventArgs e)
        {
            if (composed == null)
            {
                SetStatus("Genera la vista antes de guardar.", true);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Proyecto RackCad (*.rackcad.json)|*.rackcad.json|JSON (*.json)|*.json",
                FileName = "sistema" + RackProjectStore.FileExtension
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                new RackProjectStore().Save(RackProject.ForDynamic(composed.System), dialog.FileName);
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

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                var project = new RackProjectStore().Load(dialog.FileName);

                if (project.Kind != RackSystemKind.PalletFlow || project.DynamicSystem == null)
                {
                    SetStatus("El archivo no es un sistema dinamico.", true);
                    return;
                }

                var system = project.DynamicSystem;
                if (system.Header != null)
                {
                    header = system.Header;
                    UpdateHeaderInfo();
                }

                FrontBox.Text = Num(system.Pallet.Front);
                DepthBox.Text = Num(system.Pallet.Depth);
                PalletHeightBox.Text = Num(system.Pallet.Height);
                WeightBox.Text = Num(system.Pallet.Weight);
                PalletsDeepBox.Text = system.PalletsDeep.ToString(CultureInfo.InvariantCulture);
                Recompose();
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

        private void SetStatus(string message, bool isError)
        {
            StatusText.Text = message;
            StatusText.Foreground = new SolidColorBrush(isError ? Color.FromRgb(0xB0, 0x00, 0x20) : Color.FromRgb(0x2F, 0x85, 0x5A));
        }

        private RackFrameConfiguration BuildStandardHeader()
        {
            // Reuse the header factory; depth here is a placeholder (the composer re-imposes pallet depth + 6).
            return new RackFrameConfigurationFactory(catalog).Build(
                RackFrameTemplateCatalog.Default,
                DefaultPostCatalogId,
                DefaultHeaderHeight,
                depth: 54.0);
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
