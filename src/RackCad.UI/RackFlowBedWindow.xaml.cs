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

        /// <summary>Stable id + client name for the drawing round-trip (embed / reopen / edit).</summary>
        public string RackId { get; private set; }
        public string RackName { get; private set; }

        private string currentId;
        private string currentName;

        /// <summary>The library project this bed was opened from, if any, so a re-save preserves its unknown JSON
        /// metadata and schema version instead of stamping a fresh document (I-11). Null for a brand-new design.</summary>
        private RackCad.Application.Persistence.RackProject sourceProject;

        /// <summary>The FlowBed document this bed was opened from in the DRAWING (RACKEDITAR), if any, so a SAVE-TO-LIBRARY
        /// preserves its version + unknown fields even though there is no source project (I-11, item 4). Null otherwise.</summary>
        private RackCad.Application.Persistence.FlowBedDocument sourceFlowBed;

        public RackFlowBedWindow()
            : this(false)
        {
        }

        public RackFlowBedWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();
            if (!canInsertInAutoCad)
            {
                // Same pattern as the sibling windows: disabled CTA with the reason on the tooltip.
                InsertButton.IsEnabled = false;
                InsertButton.ToolTip = "Disponible solo cuando la cama se abre desde AutoCAD.";
            }

            catalog = UiSupport.LoadCatalogSafe();
            RollerBox.ItemsSource = BuildRollerOptions();
            RollerBox.SelectedValue = FlowBedDefaults.RollerId;
            if (RollerBox.SelectedItem == null && RollerBox.Items.Count > 0)
            {
                RollerBox.SelectedIndex = 0;
            }

            Recompute();
            if (RollerBox.Items.Count == 0)
            {
                SetStatus("No se encontró el catálogo de rodillos; revisa los CSV del catálogo.", true);
            }
        }

        // ---- Inputs ----

        private void Update_Click(object sender, RoutedEventArgs e) => Recompute();

        private void ShowBom_Click(object sender, RoutedEventArgs e)
        {
            // The BOM must reflect the fields on screen, like the AutoCAD insert (which re-reads the config).
            Recompute();
            if (lastInstances == null || lastConfig == null)
            {
                return; // Recompute already reported the specific input error in the status.
            }

            var bom = FlowBedBomBuilder.Build(lastInstances, catalog);
            new RackBomWindow(bom) { Owner = this }.ShowDialog();
        }

        private void Input_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Recompute();
            }
        }

        private void NumericBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Keep the preview live for the numeric fields too (the combos already recompute on change).
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
                error = "Fondo de cama inválido.";
                return null;
            }

            var palletDepth = 0.0;
            if (bedType == FlowBedType.Dynamic && (!TryNum(PalletDepthBox.Text, out palletDepth) || palletDepth <= 0.0))
            {
                error = "Fondo de tarima inválido.";
                return null;
            }

            // Optional roller pitch: empty = automatic; report garbage instead of silently falling back to auto.
            if (!UiSupport.TryOptionalNum(RollerPitchBox.Text, out var pitch))
            {
                error = "Paso de rodillo inválido (deja vacío para el paso automático).";
                return null;
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
                SetStatus("El dibujo en AutoCAD solo está disponible cuando la cama se abre desde AutoCAD.", true);
                return;
            }

            var config = ReadConfig(out var error);
            if (config == null)
            {
                SetStatus(error, true);
                return;
            }

            currentName = NameBox?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(currentId)) currentId = Guid.NewGuid().ToString();

            InsertRequested = true;
            FlowBedToInsert = config;
            RackId = currentId;
            RackName = currentName;
            Close();
        }

        /// <summary>Open pre-loaded with an existing drawn bed (from its embedded payload), keeping Id/Name.
        /// <paramref name="sourceFlowBed"/> is the FlowBed document read from the embed; passing it lets a SAVE-TO-LIBRARY
        /// preserve its unknown fields + schema version (I-11).</summary>
        public void LoadExisting(FlowBedConfiguration config, string id, string name,
            RackCad.Application.Persistence.FlowBedDocument sourceFlowBed = null)
        {
            if (config == null)
            {
                return;
            }

            this.sourceFlowBed = sourceFlowBed;
            currentId = id;
            currentName = name;
            if (NameBox != null)
            {
                NameBox.Text = name ?? string.Empty;
            }

            // Editing an existing bed: the draw button redraws it in place (all copies share the definition+GUID and
            // update), so it reads "Actualizar". A brand-new bed still "Insertar".
            if (InsertButton != null)
            {
                InsertButton.Content = "Actualizar en AutoCAD";
                InsertButton.ToolTip = "Redibuja la cama en sitio con tus cambios; todas sus copias en el dibujo se actualizan.";
            }

            foreach (var item in BedTypeBox.Items)
            {
                if (item is ComboBoxItem option && (option.Tag as string) == (config.BedType == FlowBedType.Pushback ? "Pushback" : "Dynamic"))
                {
                    BedTypeBox.SelectedItem = option;
                    break;
                }
            }

            RollerBox.SelectedValue = config.RollerId;
            LaneDepthBox.Text = config.LaneDepth.ToString("0.###", CultureInfo.InvariantCulture);
            PalletDepthBox.Text = config.PalletDepth > 0.0 ? config.PalletDepth.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
            RollerPitchBox.Text = config.RollerPitchOverride.HasValue ? config.RollerPitchOverride.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;

            Recompute();
        }

        /// <summary>Open pre-loaded from a LIBRARY template as a NEW bed — a fresh GUID on insert (keeps "Insertar"),
        /// unlike <see cref="LoadExisting"/> which edits a drawn bed in place. <paramref name="sourceProject"/> is the
        /// loaded library project; passing it lets a re-save preserve its unknown JSON metadata (I-11).</summary>
        public void LoadForNew(FlowBedConfiguration config, string name, RackCad.Application.Persistence.RackProject sourceProject = null)
        {
            if (config == null)
            {
                return;
            }

            this.sourceProject = sourceProject;
            currentId = null;
            currentName = name;
            if (NameBox != null)
            {
                NameBox.Text = name ?? string.Empty;
            }

            foreach (var item in BedTypeBox.Items)
            {
                if (item is ComboBoxItem option && (option.Tag as string) == (config.BedType == FlowBedType.Pushback ? "Pushback" : "Dynamic"))
                {
                    BedTypeBox.SelectedItem = option;
                    break;
                }
            }

            RollerBox.SelectedValue = config.RollerId;
            LaneDepthBox.Text = config.LaneDepth.ToString("0.###", CultureInfo.InvariantCulture);
            PalletDepthBox.Text = config.PalletDepth > 0.0 ? config.PalletDepth.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
            RollerPitchBox.Text = config.RollerPitchOverride.HasValue ? config.RollerPitchOverride.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;

            Recompute();
        }

        /// <summary>Save this cama to the on-disk design library (a reusable <c>.rackcad.json</c>).</summary>
        private void SaveToLibrary_Click(object sender, RoutedEventArgs e)
        {
            var config = ReadConfig(out var error);
            if (config == null)
            {
                SetStatus(error, true);
                return;
            }

            var name = string.IsNullOrWhiteSpace(NameBox?.Text) ? currentName : NameBox.Text.Trim();
            var path = UiSupport.PromptSaveToLibrary(this, name, "cama");
            if (path == null) return;

            try
            {
                // Preserve source metadata from whichever origin this bed came from: a library project (open-from-library)
                // or a standalone FlowBed document (open-from-drawing). Both no-op when null (a brand-new bed) (I-11).
                var project = RackCad.Application.Persistence.RackProject.ForCama(config);
                project = sourceProject != null ? project.WithSourceMetadataFrom(sourceProject) : project.WithSourceFlowBed(sourceFlowBed);
                new RackCad.Application.Persistence.RackProjectStore().Save(project, path);
                SetStatus("Cama guardada en la biblioteca: " + System.IO.Path.GetFileName(path), false);
            }
            catch (Exception ex)
            {
                SetStatus("No se pudo guardar: " + ex.Message, true);
            }
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
                "Fondo de cama: " + total.ToString("0.##", CultureInfo.InvariantCulture) + " in", LabelStroke, 12, 280.0, FontWeights.SemiBold);

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

        private PreviewCanvasPainter previewPainter;
        private PreviewCanvasPainter Painter => previewPainter ??= new PreviewCanvasPainter(PreviewCanvas);

        private void AddLine(Point a, Point b, Brush stroke, double thickness, DoubleCollection dash = null)
            => Painter.AddLine(a, b, stroke, thickness, dash);

        private void AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, DoubleCollection dash, Brush fill = null)
            => Painter.AddRectangle(left, top, width, height, stroke, thickness, dash, fill);

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
                .Where(c => string.Equals(c?.Role, FlowBedDefaults.RollerRole, StringComparison.OrdinalIgnoreCase)));
        }

        private void SetStatus(string message, bool isError) => UiSupport.SetStatus(StatusText, message, isError);

        private static bool TryNum(string text, out double value) => UiSupport.TryNum(text, out value);
    }
}
