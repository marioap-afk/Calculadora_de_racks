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
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Independent module: one roller bed ("cama de rodamiento") in the lateral view. Mirrors the rest of the
    /// app's style — basic inputs + "Avanzado" toggle, a live preview and measurements. The geometry comes
    /// from the pure <see cref="FlowBedLateralBuilder"/>; the host command draws it in AutoCAD on request.
    /// </summary>
    public partial class RackFlowBedWindow : Window
    {
        private static readonly Brush RollerStroke = new SolidColorBrush(Color.FromRgb(0x3D, 0xC9, 0x86));
        private static readonly Brush BrakeStroke = new SolidColorBrush(Color.FromRgb(0x5B, 0x8D, 0xEF));
        private static readonly Brush StopStroke = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
        private static readonly Brush StopFill = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0x6B, 0x6B));
        private static readonly Brush RailStroke = new SolidColorBrush(Color.FromRgb(0xCF, 0xDB, 0xE8));
        private static readonly Brush TroquelStroke = new SolidColorBrush(Color.FromRgb(0x45, 0x59, 0x70));
        private static readonly Brush FloorStroke = new SolidColorBrush(Color.FromRgb(0x6A, 0x7B, 0x8A));
        private static readonly Brush LabelStroke = new SolidColorBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));

        private readonly RackCatalog catalog;
        private readonly FlowBedLateralBuilder builder = new FlowBedLateralBuilder();
        private readonly bool canInsertInAutoCad;

        private IReadOnlyList<HeaderBlockInstance> lastInstances;
        private FlowBedConfiguration lastConfig;

        private double mapScale;
        private double mapOffsetX;
        private double mapBottomY;

        /// <summary>Set when the user asks to draw the bed; the host command draws it after the windows close.</summary>
        public bool InsertRequested { get; private set; }

        public FlowBedConfiguration FlowBedToInsert { get; private set; }

        public RackFlowBedWindow()
            : this(false)
        {
        }

        public RackFlowBedWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();
            catalog = UiSupport.LoadCatalogSafe();
            RollerBox.ItemsSource = BuildRollerOptions();
            RollerBox.SelectedValue = FlowBedDefaults.RollerId;
            if (RollerBox.SelectedItem == null && RollerBox.Items.Count > 0)
            {
                RollerBox.SelectedIndex = 0;
            }

            Recompute();
        }

        // ---- Inputs ----

        private void Update_Click(object sender, RoutedEventArgs e) => Recompute();

        private void Input_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Recompute();
            }
        }

        private void AdvancedToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (AdvancedPanel != null)
            {
                AdvancedPanel.Visibility = AdvancedToggle.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Recompute()
        {
            var isDynamic = !IsPushback();
            PalletDepthBox.IsEnabled = isDynamic;
            PalletDepthLabel.Foreground = isDynamic ? LabelDefault : LabelMuted;

            var config = ReadConfig(out var error);
            if (config == null)
            {
                lastConfig = null;
                lastInstances = null;
                SummaryText.Text = string.Empty;
                PreviewCanvas.Children.Clear();
                SetStatus(error, true);
                return;
            }

            lastConfig = config;
            lastInstances = builder.Build(config, catalog);
            UpdateSummary();
            SetStatus("Vista actualizada.", false);
            DrawPreview();
        }

        private bool IsPushback() => ((BedTypeBox.SelectedItem as ComboBoxItem)?.Tag as string) == "Pushback";

        private FlowBedConfiguration ReadConfig(out string error)
        {
            error = null;

            var bedType = IsPushback() ? FlowBedType.Pushback : FlowBedType.Dynamic;
            var rollerId = RollerBox.SelectedValue as string ?? FlowBedDefaults.RollerId;

            if (!TryNum(LaneDepthBox.Text, out var laneDepth) || laneDepth <= 0.0)
            {
                error = "Fondo del carril invalido.";
                return null;
            }

            var palletDepth = 0.0;
            if (bedType == FlowBedType.Dynamic && (!TryNum(PalletDepthBox.Text, out palletDepth) || palletDepth <= 0.0))
            {
                error = "Fondo de tarima invalido.";
                return null;
            }

            double? pitch = null;
            if (TryNum(RollerPitchBox.Text, out var p) && p > 0.0)
            {
                pitch = p;
            }

            return new FlowBedConfiguration
            {
                BedType = bedType,
                LaneDepth = laneDepth,
                PalletDepth = palletDepth,
                RollerId = rollerId,
                RollerPitchOverride = pitch
            };
        }

        private void UpdateSummary()
        {
            var rollers = lastInstances.Where(i => i.Role == HeaderBlockRole.Roller).OrderBy(i => i.Insertion.X).ToList();
            var brakes = lastInstances.Count(i => i.Role == HeaderBlockRole.Brake);
            var pitch = rollers.Count >= 2 ? rollers[1].Insertion.X - rollers[0].Insertion.X : 0.0;

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "Riel L = {0:0.##}\"   ·   {1} rodillos   ·   {2} frenos   ·   paso {3:0.##}\"",
                lastConfig.LaneDepth, rollers.Count, brakes, pitch);
        }

        // ---- Insert / close ----

        private void InsertInAutoCad_Click(object sender, RoutedEventArgs e)
        {
            if (!canInsertInAutoCad)
            {
                SetStatus("El dibujo en AutoCAD solo esta disponible cuando la cama se abre desde AutoCAD.", true);
                return;
            }

            var config = ReadConfig(out var error);
            if (config == null)
            {
                SetStatus(error, true);
                return;
            }

            InsertRequested = true;
            FlowBedToInsert = config;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        // ---- Preview ----

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawPreview();

        private void DrawPreview()
        {
            PreviewCanvas.Children.Clear();

            if (lastInstances == null || lastConfig == null)
            {
                return;
            }

            var total = lastConfig.LaneDepth;
            var availableWidth = PreviewCanvas.ActualWidth;
            var availableHeight = PreviewCanvas.ActualHeight;

            if (total <= 0.0 || availableWidth < 20 || availableHeight < 20)
            {
                return;
            }

            var railHeight = ProfileValue(FlowBedDefaults.RailId, p => p.Height, 3.5);
            var troquelY = lastInstances.FirstOrDefault(i => i.Role == HeaderBlockRole.Roller || i.Role == HeaderBlockRole.Stop)?.Insertion.Y ?? 2.75;
            var maxRadius = lastInstances
                .Where(i => i.Role == HeaderBlockRole.Roller || i.Role == HeaderBlockRole.Brake)
                .Select(i => Diameter(i.PieceId) / 2.0)
                .DefaultIfEmpty(0.0)
                .Max();
            var topeHeight = Math.Max(railHeight, 5.0);
            var heightExtent = Math.Max(topeHeight, troquelY + maxRadius + 0.5);

            const double horizontalMargin = 46.0;
            const double topMargin = 24.0;
            const double bottomMargin = 54.0;
            var usableWidth = Math.Max(1.0, availableWidth - 2 * horizontalMargin);
            var usableHeight = Math.Max(1.0, availableHeight - topMargin - bottomMargin);
            mapScale = Math.Min(usableWidth / total, usableHeight / heightExtent);
            if (mapScale <= 0.0)
            {
                return;
            }

            var drawHeight = heightExtent * mapScale;
            mapOffsetX = (availableWidth - total * mapScale) / 2.0;
            mapBottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;

            AddCanvasLabel(mapOffsetX, Math.Max(4.0, mapBottomY - drawHeight - 22.0),
                "Fondo del carril: " + total.ToString("0.##", CultureInfo.InvariantCulture) + " in", LabelStroke, 12, 280.0, FontWeights.SemiBold);

            // Floor, rail body and troquel reference line.
            AddLine(Map(0, 0), Map(total, 0), FloorStroke, 1.5);
            var railTopLeft = Map(0, railHeight);
            AddRectangle(railTopLeft.X, railTopLeft.Y, total * mapScale, railHeight * mapScale, RailStroke, 1.2, null);
            AddLine(Map(0, troquelY), Map(total, troquelY), TroquelStroke, 0.8, Dash());

            foreach (var instance in lastInstances)
            {
                switch (instance.Role)
                {
                    case HeaderBlockRole.Stop:
                        // Tope zone at the discharge end (from the rail start up to the first roller).
                        var stopTop = Map(0, topeHeight);
                        AddRectangle(stopTop.X, stopTop.Y, (instance.Insertion.X + FlowBedDefaults.TopeOccupiedLength) * mapScale,
                            topeHeight * mapScale, StopStroke, 1.4, null, StopFill);
                        break;
                    case HeaderBlockRole.Roller:
                        AddEllipse(instance.Insertion.X, troquelY, Diameter(instance.PieceId), RollerStroke, 1.6);
                        break;
                    case HeaderBlockRole.Brake:
                        AddEllipse(instance.Insertion.X, troquelY, Diameter(instance.PieceId), BrakeStroke, 1.8);
                        break;
                }
            }
        }

        private double Diameter(string pieceId) => ProfileValue(pieceId, p => p.Diameter, 0.0);

        private double ProfileValue(string pieceId, Func<FlowBedComponentCatalogEntry, double> selector, double fallback)
        {
            var entry = catalog?.FlowBedProfiles.FirstOrDefault(c => string.Equals(c.Id, pieceId, StringComparison.OrdinalIgnoreCase));
            return entry == null ? fallback : selector(entry);
        }

        // ---- Drawing primitives (world Y up) ----

        private Point Map(double x, double y) => new Point(mapOffsetX + x * mapScale, mapBottomY - y * mapScale);

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
                StrokeDashArray = dash,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            });
        }

        private void AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, DoubleCollection dash, Brush fill = null)
        {
            if (width <= 0.0 || height <= 0.0)
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
                Fill = fill ?? Brushes.Transparent
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            PreviewCanvas.Children.Add(rectangle);
        }

        private void AddEllipse(double worldX, double worldY, double worldDiameter, Brush stroke, double thickness)
        {
            var d = worldDiameter * mapScale;
            if (d <= 0.0)
            {
                return;
            }

            var center = Map(worldX, worldY);
            var ellipse = new Ellipse
            {
                Width = d,
                Height = d,
                Stroke = stroke,
                StrokeThickness = thickness,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(ellipse, center.X - d / 2.0);
            Canvas.SetTop(ellipse, center.Y - d / 2.0);
            PreviewCanvas.Children.Add(ellipse);
        }

        private void AddCanvasLabel(double left, double top, string text, Brush brush, double size, double maxWidth, FontWeight? fontWeight)
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
            }

            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            PreviewCanvas.Children.Add(label);
        }

        private static DoubleCollection Dash() => new DoubleCollection { 5, 3 };

        // ---- Helpers ----

        private static readonly Brush LabelDefault = new SolidColorBrush(Color.FromRgb(0x41, 0x51, 0x61));
        private static readonly Brush LabelMuted = new SolidColorBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));

        private List<CatalogOption> BuildRollerOptions()
        {
            return UiSupport.ToOptions((catalog?.FlowBedProfiles ?? Enumerable.Empty<FlowBedComponentCatalogEntry>())
                .Where(c => string.Equals(c?.Role, "RODILLO", StringComparison.OrdinalIgnoreCase)));
        }

        private void SetStatus(string message, bool isError)
        {
            StatusText.Text = message ?? string.Empty;
            StatusText.Foreground = isError
                ? new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B))
                : new SolidColorBrush(Color.FromRgb(0x61, 0x70, 0x80));
        }

        private static bool TryNum(string text, out double value) => UiSupport.TryNum(text, out value);
    }
}
