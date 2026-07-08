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
    /// Independent module: a selective rack in the FRONTAL view. Inputs a cabecera (height + post + peralte)
    /// and a bay template (larguero, peralte, length, levels), builds N bays and previews the run — posts as
    /// vertical members, largueros per level. Geometry comes from the pure <see cref="SelectiveFrontalBuilder"/>.
    /// </summary>
    public partial class RackSelectiveWindow : Window
    {
        private static readonly Brush PostBrush = new SolidColorBrush(Color.FromRgb(0x3D, 0xC9, 0x86));
        private static readonly Brush PostFill = new SolidColorBrush(Color.FromArgb(0x30, 0x3D, 0xC9, 0x86));
        private static readonly Brush BeamBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0x8A, 0x2B));
        private static readonly Brush BeamFill = new SolidColorBrush(Color.FromArgb(0x66, 0xE0, 0x8A, 0x2B));
        private static readonly Brush PlateFill = new SolidColorBrush(Color.FromRgb(0xB7, 0xC3, 0xCF));
        private static readonly Brush FloorStroke = new SolidColorBrush(Color.FromRgb(0x6A, 0x7B, 0x8A));
        private static readonly Brush LabelStroke = new SolidColorBrush(Color.FromRgb(0x9A, 0xA7, 0xB4));

        private readonly RackCatalog catalog;
        private readonly SelectiveFrontalBuilder builder = new SelectiveFrontalBuilder();
        private readonly bool canInsertInAutoCad;

        private IReadOnlyList<HeaderBlockInstance> lastInstances;
        private SelectiveRackSystem lastSystem;

        private double mapScale;
        private double mapOffsetX;
        private double mapBottomY;
        private double mapMinX;

        public bool InsertRequested { get; private set; }

        public SelectiveRackSystem SystemToInsert { get; private set; }

        public RackSelectiveWindow()
            : this(false)
        {
        }

        public RackSelectiveWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();
            catalog = UiSupport.LoadCatalogSafe();

            PostBox.ItemsSource = UiSupport.ToOptions(catalog?.PostProfiles);
            PostBox.SelectedValue = catalog?.Defaults?.Post;
            if (PostBox.SelectedItem == null && PostBox.Items.Count > 0) PostBox.SelectedIndex = 0;

            BeamBox.ItemsSource = UiSupport.ToOptions(catalog?.BeamProfiles);
            if (BeamBox.Items.Count > 0) BeamBox.SelectedIndex = 0;

            Recompute();
        }

        private void Update_Click(object sender, RoutedEventArgs e) => Recompute();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void InsertInAutoCad_Click(object sender, RoutedEventArgs e)
        {
            if (!canInsertInAutoCad)
            {
                SetStatus("El dibujo en AutoCAD solo esta disponible cuando el selectivo se abre desde AutoCAD.", true);
                return;
            }

            var system = ReadSystem(out var error);
            if (system == null)
            {
                SetStatus(error, true);
                return;
            }

            InsertRequested = true;
            SystemToInsert = system;
            Close();
        }

        private void Recompute()
        {
            var system = ReadSystem(out var error);
            if (system == null)
            {
                lastSystem = null;
                lastInstances = null;
                SummaryText.Text = string.Empty;
                PreviewCanvas.Children.Clear();
                SetStatus(error, true);
                return;
            }

            lastSystem = system;
            lastInstances = builder.Build(system, catalog);
            UpdateSummary();
            SetStatus("Vista actualizada.", false);
            DrawPreview();
        }

        private SelectiveRackSystem ReadSystem(out string error)
        {
            error = null;

            if (!UiSupport.TryNum(HeightBox.Text, out var height) || height <= 0.0) { error = "Altura invalida."; return null; }
            if (!UiSupport.TryNum(PostPeralteBox.Text, out var postPeralte) || postPeralte <= 0.0) { error = "Peralte de poste invalido."; return null; }
            if (!(PostBox.SelectedValue is string postId) || string.IsNullOrWhiteSpace(postId)) { error = "Selecciona un poste."; return null; }
            if (!(BeamBox.SelectedValue is string beamId) || string.IsNullOrWhiteSpace(beamId)) { error = "Selecciona un larguero."; return null; }
            if (!TryInt(BayCountBox.Text, out var bayCount) || bayCount < 1) { error = "Cantidad de bahias invalida."; return null; }
            if (!UiSupport.TryNum(BeamLengthBox.Text, out var beamLength) || beamLength <= 0.0) { error = "Longitud de larguero invalida."; return null; }
            if (!UiSupport.TryNum(BeamPeralteBox.Text, out var beamPeralte) || beamPeralte <= 0.0) { error = "Peralte de larguero invalido."; return null; }
            if (!TryInt(LevelsBox.Text, out var levels) || levels < 1) { error = "Niveles invalidos."; return null; }
            if (!UiSupport.TryNum(FirstLevelBox.Text, out var firstLevel) || firstLevel <= 0.0) { error = "1er nivel invalido."; return null; }
            if (!UiSupport.TryNum(SeparationBox.Text, out var separation) || separation <= 0.0) { error = "Separacion invalida."; return null; }

            var system = new SelectiveRackSystem { Height = height, PostId = postId, PostPeralte = postPeralte };
            for (var i = 0; i < bayCount; i++)
            {
                system.Bays.Add(new SelectiveBay
                {
                    BeamId = beamId,
                    BeamPeralte = beamPeralte,
                    BeamLength = beamLength,
                    Levels = levels,
                    FirstLevel = firstLevel,
                    Separation = separation
                });
            }

            return system;
        }

        private void UpdateSummary()
        {
            var posts = lastInstances.Count(i => i.Role == HeaderBlockRole.Post);
            var beams = lastInstances.Count(i => i.Role == HeaderBlockRole.Beam);

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0} bahías   ·   {1} cabeceras   ·   {2} largueros   ·   altura {3:0.##}\"",
                lastSystem.Bays.Count, posts, beams, lastSystem.Height);
        }

        // ---- Preview ----

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawPreview();

        private void DrawPreview()
        {
            PreviewCanvas.Children.Clear();

            if (lastInstances == null || lastSystem == null || lastSystem.Height <= 0.0)
            {
                return;
            }

            var postWidth = ProfileWidth(lastSystem.PostId);
            var height = lastSystem.Height;

            var xMin = -postWidth / 2.0;
            var xMax = xMin;
            foreach (var instance in lastInstances)
            {
                if (instance.Role == HeaderBlockRole.Post)
                {
                    xMax = Math.Max(xMax, instance.Insertion.X + postWidth / 2.0);
                }
                else if (instance.Role == HeaderBlockRole.Beam)
                {
                    var length = Param(instance, "LONGITUD");
                    xMax = Math.Max(xMax, instance.Insertion.X + length);
                }
            }

            var totalWidth = Math.Max(1.0, xMax - xMin);
            var availableWidth = PreviewCanvas.ActualWidth;
            var availableHeight = PreviewCanvas.ActualHeight;
            if (availableWidth < 20 || availableHeight < 20)
            {
                return;
            }

            const double horizontalMargin = 46.0;
            const double topMargin = 26.0;
            const double bottomMargin = 40.0;
            var usableWidth = Math.Max(1.0, availableWidth - 2 * horizontalMargin);
            var usableHeight = Math.Max(1.0, availableHeight - topMargin - bottomMargin);
            mapScale = Math.Min(usableWidth / totalWidth, usableHeight / height);
            if (mapScale <= 0.0)
            {
                return;
            }

            mapMinX = xMin;
            var drawWidth = totalWidth * mapScale;
            var drawHeight = height * mapScale;
            mapOffsetX = (availableWidth - drawWidth) / 2.0;
            mapBottomY = topMargin + (usableHeight - drawHeight) / 2.0 + drawHeight;

            AddCanvasLabel(mapOffsetX, Math.Max(4.0, mapBottomY - drawHeight - 22.0),
                "Ancho " + totalWidth.ToString("0.##", CultureInfo.InvariantCulture) + " in  ·  altura " + height.ToString("0.##", CultureInfo.InvariantCulture) + " in",
                LabelStroke, 12, 320.0);

            // Floor.
            AddLine(Map(xMin, 0), Map(xMax, 0), FloorStroke, 1.5);

            foreach (var instance in lastInstances)
            {
                switch (instance.Role)
                {
                    case HeaderBlockRole.Post:
                        var pTop = Map(instance.Insertion.X - postWidth / 2.0, height);
                        AddRectangle(pTop.X, pTop.Y, postWidth * mapScale, height * mapScale, PostBrush, 1.6, PostFill);
                        break;
                    case HeaderBlockRole.Beam:
                        var length = Param(instance, "LONGITUD");
                        var peralte = Param(instance, "PERALTE");
                        var bTop = Map(instance.Insertion.X, instance.Insertion.Y + peralte / 2.0);
                        AddRectangle(bTop.X, bTop.Y, length * mapScale, Math.Max(2.0, peralte * mapScale), BeamBrush, 1.2, BeamFill);
                        break;
                    case HeaderBlockRole.BasePlate:
                        var plate = Map(instance.ConnectionAnchor.X - postWidth * 0.7, 0);
                        AddRectangle(plate.X, plate.Y, postWidth * 1.4 * mapScale, Math.Max(3.0, 0.3 * mapScale + 4.0), PlateFill, 1.0, PlateFill);
                        break;
                }
            }
        }

        private double ProfileWidth(string postId)
        {
            var width = catalog?.PostProfiles.FirstOrDefault(p => string.Equals(p?.Id, postId, StringComparison.OrdinalIgnoreCase))?.Width ?? 0.0;
            return width > 0.0 ? width : 3.0;
        }

        private static double Param(HeaderBlockInstance instance, string name)
            => instance.DynamicParameters.TryGetValue(name, out var value) ? value : 0.0;

        private Point Map(double x, double y) => new Point(mapOffsetX + (x - mapMinX) * mapScale, mapBottomY - y * mapScale);

        private void AddLine(Point a, Point b, Brush stroke, double thickness)
        {
            PreviewCanvas.Children.Add(new Line
            {
                X1 = a.X, Y1 = a.Y, X2 = b.X, Y2 = b.Y,
                Stroke = stroke, StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
            });
        }

        private void AddRectangle(double left, double top, double width, double height, Brush stroke, double thickness, Brush fill)
        {
            if (width <= 0.0 || height <= 0.0)
            {
                return;
            }

            var rectangle = new Rectangle
            {
                Width = width, Height = height,
                Stroke = stroke, StrokeThickness = thickness,
                Fill = fill ?? Brushes.Transparent
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            PreviewCanvas.Children.Add(rectangle);
        }

        private void AddCanvasLabel(double left, double top, string text, Brush brush, double size, double maxWidth)
        {
            var label = new TextBlock
            {
                Text = text, Foreground = brush, FontSize = size,
                FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis
            };
            if (maxWidth > 0.0) label.Width = maxWidth;
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            PreviewCanvas.Children.Add(label);
        }

        private void SetStatus(string message, bool isError)
        {
            StatusText.Text = message ?? string.Empty;
            StatusText.Foreground = isError
                ? new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B))
                : new SolidColorBrush(Color.FromRgb(0x61, 0x70, 0x80));
        }

        private static bool TryInt(string text, out int value)
            => int.TryParse((text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
}
