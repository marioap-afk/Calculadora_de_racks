using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Advanced editor for a selective rack (FRONTAL view). The user edits a bays × levels MATRIX where each
    /// cell carries its own pallet (frente/alto), count and larguero; <see cref="SelectiveGeometryResolver"/>
    /// derives the larguero lengths, level separations and post height, and <see cref="SelectiveFrontalBuilder"/>
    /// lays out the blocks. Click a cell to edit it, then apply the values to the cell / row / column / all.
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

        private static readonly Brush CellStroke = new SolidColorBrush(Color.FromRgb(0xD8, 0xDE, 0xE6));
        private static readonly Brush CellText = new SolidColorBrush(Color.FromRgb(0x1F, 0x29, 0x33));
        private static readonly Brush CellSelStroke = new SolidColorBrush(Color.FromRgb(0x2F, 0x6F, 0xED));
        private static readonly Brush CellSelFill = new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE));

        private readonly RackCatalog catalog;
        private readonly SelectiveFrontalBuilder builder = new SelectiveFrontalBuilder();
        private readonly SelectiveGeometryResolver resolver = new SelectiveGeometryResolver();
        private readonly bool canInsertInAutoCad;

        /// <summary>The design matrix: <c>bays[bay][level]</c>, level 0 = bottom.</summary>
        private readonly List<List<Cell>> bays = new List<List<Cell>>();
        private string defaultBeamId;
        private int selBay;
        private int selLevel;

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

            CellBeamBox.ItemsSource = UiSupport.ToOptions(catalog?.BeamProfiles);
            if (CellBeamBox.Items.Count > 0) CellBeamBox.SelectedIndex = 0;
            defaultBeamId = CellBeamBox.SelectedValue as string;

            InitMatrix(2, 4);
            LoadCellEditor();
            RenderMatrix();
            Recompute();
        }

        // ---- Matrix model ----

        /// <summary>One editable matrix cell (a bay's level): its pallet, count and beam.</summary>
        private sealed class Cell
        {
            public double Frente = 40.0;
            public double Alto = 60.0;
            public int PalletCount = 2;
            public string BeamId;
            public double BeamPeralte = 4.0;

            public Cell Clone() => (Cell)MemberwiseClone();

            public void CopyFrom(Cell other)
            {
                Frente = other.Frente;
                Alto = other.Alto;
                PalletCount = other.PalletCount;
                BeamId = other.BeamId;
                BeamPeralte = other.BeamPeralte;
            }
        }

        private enum Scope { Cell, Row, Column, All }

        private Cell NewCell() => new Cell { BeamId = defaultBeamId, BeamPeralte = 4.0 };

        private void InitMatrix(int bayCount, int levelCount)
        {
            bays.Clear();
            for (var b = 0; b < bayCount; b++)
            {
                var column = new List<Cell>();
                for (var l = 0; l < levelCount; l++) column.Add(NewCell());
                bays.Add(column);
            }

            selBay = 0;
            selLevel = 0;
        }

        /// <summary>Grow/shrink the matrix, preserving existing cells; new bays/levels inherit the neighbour's config.</summary>
        private void ResizeMatrix(int bayCount, int levelCount)
        {
            while (bays.Count < bayCount)
            {
                var template = bays.Count > 0 ? bays[bays.Count - 1] : null;
                var column = new List<Cell>();
                for (var l = 0; l < levelCount; l++)
                {
                    column.Add(template != null && l < template.Count ? template[l].Clone() : NewCell());
                }

                bays.Add(column);
            }

            while (bays.Count > bayCount) bays.RemoveAt(bays.Count - 1);

            foreach (var column in bays)
            {
                while (column.Count < levelCount)
                {
                    column.Add(column.Count > 0 ? column[column.Count - 1].Clone() : NewCell());
                }

                while (column.Count > levelCount) column.RemoveAt(column.Count - 1);
            }

            selBay = Math.Min(Math.Max(0, selBay), bays.Count - 1);
            selLevel = Math.Min(Math.Max(0, selLevel), (bays.Count > 0 ? bays[0].Count : 1) - 1);
        }

        private bool TryGetSelected(out Cell cell)
        {
            cell = null;
            if (selBay < 0 || selBay >= bays.Count) return false;
            var column = bays[selBay];
            if (selLevel < 0 || selLevel >= column.Count) return false;
            cell = column[selLevel];
            return true;
        }

        // ---- Matrix rendering ----

        private void RenderMatrix()
        {
            MatrixGrid.Children.Clear();
            MatrixGrid.RowDefinitions.Clear();
            MatrixGrid.ColumnDefinitions.Clear();

            var bayCount = bays.Count;
            var levelCount = bayCount > 0 ? bays[0].Count : 0;
            if (bayCount == 0 || levelCount == 0) return;

            MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
            for (var b = 0; b < bayCount; b++)
                MatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });

            MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var r = 0; r < levelCount; r++)
                MatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddToGrid(HeaderCell(string.Empty), 0, 0);
            for (var b = 0; b < bayCount; b++)
                AddToGrid(HeaderCell("Bahía " + (b + 1)), 0, b + 1);

            // Top display row = highest level.
            for (var displayRow = 0; displayRow < levelCount; displayRow++)
            {
                var level = levelCount - 1 - displayRow;
                var gridRow = displayRow + 1;
                AddToGrid(HeaderCell("Nivel " + (level + 1)), gridRow, 0);

                for (var b = 0; b < bayCount; b++)
                {
                    AddToGrid(CellUi(bays[b][level], b, level), gridRow, b + 1);
                }
            }
        }

        private void AddToGrid(UIElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            MatrixGrid.Children.Add(element);
        }

        private static TextBlock HeaderCell(string text) => new TextBlock
        {
            Text = text,
            Foreground = LabelStroke,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2)
        };

        private UIElement CellUi(Cell cell, int bay, int level)
        {
            var selected = bay == selBay && level == selLevel;
            var border = new Border
            {
                Margin = new Thickness(2),
                Background = selected ? CellSelFill : Brushes.White,
                BorderBrush = selected ? CellSelStroke : CellStroke,
                BorderThickness = new Thickness(selected ? 2 : 1),
                Padding = new Thickness(6, 4, 6, 4),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = string.Format(CultureInfo.InvariantCulture, "{0:0.#}×{1:0.#}\n×{2} · P{3:0.#}",
                        cell.Frente, cell.Alto, cell.PalletCount, cell.BeamPeralte),
                    FontSize = 11,
                    Foreground = CellText,
                    TextAlignment = TextAlignment.Center
                }
            };

            border.MouseLeftButtonUp += (s, e) => SelectCell(bay, level);
            return border;
        }

        private void SelectCell(int bay, int level)
        {
            selBay = bay;
            selLevel = level;
            LoadCellEditor();
            RenderMatrix();
        }

        private void LoadCellEditor()
        {
            if (!TryGetSelected(out var cell)) return;

            CellHeader.Text = string.Format(CultureInfo.InvariantCulture, "Celda: Bahía {0} · Nivel {1}", selBay + 1, selLevel + 1);
            FrenteBox.Text = cell.Frente.ToString("0.###", CultureInfo.InvariantCulture);
            AltoBox.Text = cell.Alto.ToString("0.###", CultureInfo.InvariantCulture);
            PalletCountBox.Text = cell.PalletCount.ToString(CultureInfo.InvariantCulture);
            BeamPeralteBox.Text = cell.BeamPeralte.ToString("0.###", CultureInfo.InvariantCulture);
            CellBeamBox.SelectedValue = cell.BeamId;
        }

        // ---- Events ----

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadStructure(out var bayCount, out var levelCount, out var error))
            {
                SetStatus(error, true);
                return;
            }

            ResizeMatrix(bayCount, levelCount);
            LoadCellEditor();
            RenderMatrix();
            Recompute();
        }

        private void ApplyCell_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Cell);
        private void ApplyRow_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Row);
        private void ApplyColumn_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.Column);
        private void ApplyAll_Click(object sender, RoutedEventArgs e) => ApplyScope(Scope.All);
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void ApplyScope(Scope scope)
        {
            if (!ReadCellEditor(out var values, out var error))
            {
                SetStatus(error, true);
                return;
            }

            var levelCount = bays.Count > 0 ? bays[0].Count : 0;
            for (var b = 0; b < bays.Count; b++)
            {
                for (var l = 0; l < levelCount; l++)
                {
                    var inScope =
                        scope == Scope.All ||
                        (scope == Scope.Cell && b == selBay && l == selLevel) ||
                        (scope == Scope.Row && l == selLevel) ||
                        (scope == Scope.Column && b == selBay);

                    if (inScope) bays[b][l].CopyFrom(values);
                }
            }

            RenderMatrix();
            Recompute();
        }

        private void InsertInAutoCad_Click(object sender, RoutedEventArgs e)
        {
            if (!canInsertInAutoCad)
            {
                SetStatus("El dibujo en AutoCAD solo esta disponible cuando el selectivo se abre desde AutoCAD.", true);
                return;
            }

            var system = BuildSystem(out var error);
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
            var system = BuildSystem(out var error);
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

        // ---- Reading inputs ----

        private bool ReadStructure(out int bayCount, out int levelCount, out string error)
        {
            bayCount = 0;
            levelCount = 0;
            error = null;
            if (!TryInt(BayCountBox.Text, out bayCount) || bayCount < 1) { error = "Cantidad de bahias invalida."; return false; }
            if (!TryInt(LevelsBox.Text, out levelCount) || levelCount < 1) { error = "Niveles invalidos."; return false; }
            return true;
        }

        private bool ReadCellEditor(out Cell values, out string error)
        {
            values = null;
            error = null;
            if (!(CellBeamBox.SelectedValue is string beamId) || string.IsNullOrWhiteSpace(beamId)) { error = "Selecciona un larguero."; return false; }
            if (!UiSupport.TryNum(FrenteBox.Text, out var frente) || frente <= 0.0) { error = "Frente de tarima invalido."; return false; }
            if (!UiSupport.TryNum(AltoBox.Text, out var alto) || alto <= 0.0) { error = "Alto de tarima invalido."; return false; }
            if (!TryInt(PalletCountBox.Text, out var count) || count < 1) { error = "Tarimas por nivel invalido."; return false; }
            if (!UiSupport.TryNum(BeamPeralteBox.Text, out var peralte) || peralte <= 0.0) { error = "Peralte de larguero invalido."; return false; }

            values = new Cell { Frente = frente, Alto = alto, PalletCount = count, BeamId = beamId, BeamPeralte = peralte };
            return true;
        }

        private SelectiveRackSystem BuildSystem(out string error)
        {
            error = null;
            if (!(PostBox.SelectedValue is string postId) || string.IsNullOrWhiteSpace(postId)) { error = "Selecciona un poste."; return null; }
            if (!UiSupport.TryNum(PostPeralteBox.Text, out var postPeralte) || postPeralte <= 0.0) { error = "Peralte de poste invalido."; return null; }
            if (!UiSupport.TryNum(FirstLevelBox.Text, out var firstLevel) || firstLevel <= 0.0) { error = "1er nivel invalido."; return null; }
            if (!UiSupport.TryNum(ToleranceBox.Text, out var tolerance) || tolerance < 0.0) { error = "Tolerancia horizontal invalida."; return null; }
            if (!UiSupport.TryNum(ClearanceBox.Text, out var clearance) || clearance < 0.0) { error = "Holgura vertical invalida."; return null; }
            if (bays.Count == 0 || bays[0].Count == 0) { error = "Define bahias y niveles."; return null; }

            var design = new SelectivePalletDesign
            {
                PostId = postId,
                PostPeralte = postPeralte,
                PalletTolerance = tolerance,
                VerticalClearance = clearance,
                FirstLevel = firstLevel
            };

            foreach (var column in bays)
            {
                var bay = new SelectiveBayDesign();
                foreach (var cell in column)
                {
                    bay.Levels.Add(new SelectiveCell
                    {
                        Pallet = new Tarima { Frente = cell.Frente, Alto = cell.Alto },
                        PalletCount = cell.PalletCount,
                        BeamId = cell.BeamId,
                        BeamPeralte = cell.BeamPeralte
                    });
                }

                design.Bays.Add(bay);
            }

            var system = resolver.Resolve(design, catalog);
            if (system.Height <= 0.0) { error = "No se pudo derivar la geometria (revisa tarima/niveles)."; return null; }
            return system;
        }

        private void UpdateSummary()
        {
            var posts = lastInstances.Count(i => i.Role == HeaderBlockRole.Post);
            var beams = lastInstances.Count(i => i.Role == HeaderBlockRole.Beam);

            var bay0 = lastSystem.Bays.Count > 0 ? lastSystem.Bays[0] : null;
            var beamLength = bay0?.BeamLength ?? 0.0;
            var separation = bay0 != null && bay0.Levels.Count > 1 ? bay0.Levels[1].Y - bay0.Levels[0].Y : 0.0;

            SummaryText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0} bahías · {1} cabeceras · {2} largueros\nDerivado (bahía 1): larguero {3:0.##}\" · sep. {4:0.##}\" · altura {5:0.##}\"",
                lastSystem.Bays.Count, posts, beams, beamLength, separation, lastSystem.Height);
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
