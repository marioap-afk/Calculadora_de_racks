using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI.Controls;

namespace RackCad.UI
{
    /// <summary>Configures the DESVIADOR post × load-level grid and its two even dimensions. The grid is the shared
    /// <see cref="SelectionMatrix"/> control (I-22) with absent cells for the jagged posts; toggling a cell still
    /// recomputes the live clearance note through the model's granular events.</summary>
    public sealed class SafetyDesviadorGridWindow : Window
    {
        private readonly SelectionMatrixModel model;
        private readonly IReadOnlyList<int> levelsPerPost;
        private readonly ComboBox side;
        private readonly TextBox longitud;
        private readonly TextBox firstHeight;
        private readonly TextBlock note;
        private readonly TextBlock error;
        private readonly SelectiveRackSystem system;
        private readonly RackCatalog catalog;
        private readonly string elementId;

        public sealed class DesviadorResult
        {
            public SafetySide Side;
            public double Longitud;
            public double FirstLevelHeight;
            public IReadOnlyList<int> LevelCounts;
            public List<SelectiveGridCell> OffCells = new List<SelectiveGridCell>();
        }

        public DesviadorResult Result { get; private set; }

        public SafetyDesviadorGridWindow(
            string elementId,
            string label,
            SelectiveRackSystem system,
            RackCatalog catalog,
            double longitud,
            double firstHeight,
            SafetySide side,
            IEnumerable<SelectiveGridCell> offCells,
            int fallbackPostCount,
            IReadOnlyList<int> fallbackLevelsPerFrente,
            bool fallbackLevelsArePerPost = false)
        {
            this.elementId = elementId;
            this.system = system;
            this.catalog = catalog;

            var initial = WorkingSelection(
                Effective(longitud, SelectiveSafetyDefaults.DesviadorLongitud),
                Effective(firstHeight, SelectiveSafetyDefaults.DesviadorPrimerNivelAltura),
                EffectiveSide(side),
                offCells);
            // The grid shape must not shrink when a one-sided legacy selection is opened. Build its post/level
            // union with Both; the selected side only filters the physical drawing and BOM.
            var gridSelection = WorkingSelection(
                initial.DesviadorLongitud,
                initial.DesviadorPrimerNivelAltura,
                SafetySide.Both,
                offCells);
            var plan = SelectiveDesviadorPlan.Build(system, catalog, gridSelection);
            levelsPerPost = plan.LevelCounts.Count > 0
                ? plan.LevelCounts
                : FallbackCounts(fallbackPostCount, fallbackLevelsPerFrente, fallbackLevelsArePerPost);

            var posts = levelsPerPost.Count;
            var maxLevels = posts > 0 ? levelsPerPost.Max() : 0;
            model = SelectionMatrixModel.WithJaggedColumns(
                levelsPerPost,
                (offCells ?? Enumerable.Empty<SelectiveGridCell>())
                    .Where(cell => cell != null)
                    .Select(cell => new SelectionMatrixCell(cell.Frente, cell.Level)));

            Title = string.IsNullOrWhiteSpace(label) ? "Desviador" : label;
            Width = Math.Max(560, Math.Min(1000, 270 + posts * 46));
            Height = Math.Min(680, 330 + maxLevels * 30);
            MinWidth = 540;
            MinHeight = 330;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };
            var intro = new TextBlock
            {
                Text = "Marca los postes y niveles que llevan desviador (todos por defecto). El nivel 1 siempre se mide "
                     + "desde el primer TROQUEL_LARGUERO, aunque no haya larguero a piso; los niveles superiores van 6\" debajo del larguero. "
                     + "Elige si se coloca en la cara exterior izquierda, derecha espejeada o en ambas.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11.5,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            var options = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            options.Children.Add(new TextBlock { Text = "Lado:", VerticalAlignment = VerticalAlignment.Center });
            this.side = new ComboBox
            {
                Width = 92,
                Margin = new Thickness(5, 0, 16, 0),
                ToolTip = "Izquierdo = cara exterior frontal; Derecho = cara exterior posterior espejeada."
            };
            this.side.Items.Add("Izquierdo");
            this.side.Items.Add("Derecho");
            this.side.Items.Add("Ambas");
            this.side.SelectedIndex = SideIndex(initial.Side);
            options.Children.Add(this.side);
            options.Children.Add(new TextBlock { Text = "Longitud (in):", VerticalAlignment = VerticalAlignment.Center });
            this.longitud = new TextBox
            {
                Width = 62,
                Margin = new Thickness(5, 0, 16, 0),
                Text = initial.DesviadorLongitud.ToString(CultureInfo.InvariantCulture),
                ToolTip = "LONGITUD del bloque: entero par mayor de 8\"."
            };
            options.Children.Add(this.longitud);
            options.Children.Add(new TextBlock { Text = "Primer nivel sobre troquel (in):", VerticalAlignment = VerticalAlignment.Center });
            this.firstHeight = new TextBox
            {
                Width = 62,
                Margin = new Thickness(5, 0, 0, 0),
                Text = initial.DesviadorPrimerNivelAltura.ToString(CultureInfo.InvariantCulture),
                ToolTip = "Altura sobre el primer TROQUEL_LARGUERO: entero par mayor de 8\"."
            };
            options.Children.Add(this.firstHeight);
            DockPanel.SetDock(options, Dock.Bottom);
            root.Children.Add(options);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
            var all = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Todos", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
            all.Click += (s, e) => model.SetAll(true);
            var none = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Ninguno", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
            none.Click += (s, e) => model.SetAll(false);
            var ok = new Button { Style = TryFindResource("PrimaryButtonStyle") as Style, Content = "Aceptar", Padding = new Thickness(16, 3, 16, 3), IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            ok.Click += (s, e) => OnOk();
            var cancel = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Cancelar", Padding = new Thickness(10, 3, 10, 3), IsCancel = true };
            buttons.Children.Add(all);
            buttons.Children.Add(none);
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            DockPanel.SetDock(buttons, Dock.Bottom);
            root.Children.Add(buttons);

            note = new TextBlock { FontSize = 11, Foreground = Brushes.DarkOrange, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 7, 0, 0) };
            DockPanel.SetDock(note, Dock.Bottom);
            root.Children.Add(note);
            error = new TextBlock { FontSize = 11, Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) };
            DockPanel.SetDock(error, Dock.Bottom);
            root.Children.Add(error);

            // ---- The grid: columns = posts, rows = load levels (highest at top), via the shared control. Toggling a
            // cell or changing a dimension recomputes the clearance note through the model's granular events. ----
            var matrix = new SelectionMatrix
            {
                Model = model,
                InvertRows = true,
                ColumnHeaders = Enumerable.Range(0, posts)
                    .Select(p => "P" + (p + 1).ToString(CultureInfo.InvariantCulture)).ToArray(),
                RowHeaders = Enumerable.Range(0, maxLevels)
                    .Select(level => level == 0 ? "Nivel 1 (piso)" : "Nivel " + (level + 1).ToString(CultureInfo.InvariantCulture)).ToArray()
            };
            model.CellChanged += (s, e) => RefreshNote();
            model.BulkChanged += (s, e) => RefreshNote();

            this.longitud.TextChanged += (s, e) => RefreshNote();
            this.firstHeight.TextChanged += (s, e) => RefreshNote();
            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, Content = matrix });
            Content = root;
            RefreshNote();
        }

        private void RefreshNote()
        {
            note.Text = string.Empty;
            if (!TryDimensions(out var length, out var first, showError: false) || system == null || catalog == null)
            {
                return;
            }

            var working = WorkingSelection(length, first, SelectedSide(), CurrentOffCells());
            var plan = SelectiveDesviadorPlan.Build(system, catalog, working);
            if (plan.ClearanceIssues.Count == 0)
            {
                return;
            }

            var issue = plan.ClearanceIssues.OrderBy(i => i.Clear).First();
            var recommended = Math.Floor(issue.Clear / 2.0) * 2.0;
            var advice = recommended > 8.0
                ? "Prueba una LONGITUD de " + recommended.ToString("0", CultureInfo.InvariantCulture) + "\" o menor."
                : "Ese claro no admite una LONGITUD par mayor de 8\"; revisa la geometría o desactiva esa celda.";
            note.Text = "Nota: el claro mínimo entre niveles seleccionados es "
                      + issue.Clear.ToString("0.##", CultureInfo.InvariantCulture) + "\" en P"
                      + (issue.PostIndex + 1).ToString(CultureInfo.InvariantCulture)
                      + ", menor que la LONGITUD de " + plan.Longitud.ToString("0.##", CultureInfo.InvariantCulture)
                      + "\". " + advice;
        }

        /// <summary>Builds the config from the current controls and grid, or null when a dimension is invalid (the
        /// error is shown). Shared by OK and the tests, which cannot set <see cref="Window.DialogResult"/>.</summary>
        internal DesviadorResult BuildResult()
        {
            if (!TryDimensions(out var length, out var first, showError: true))
            {
                return null;
            }

            return new DesviadorResult
            {
                Side = SelectedSide(),
                Longitud = length,
                FirstLevelHeight = first,
                LevelCounts = levelsPerPost.ToList(),
                OffCells = CurrentOffCells()
            };
        }

        private void OnOk()
        {
            var result = BuildResult();
            if (result == null)
            {
                return;
            }

            Result = result;
            DialogResult = true;
        }

        private bool TryDimensions(out double length, out double first, bool showError)
        {
            length = 0.0;
            first = 0.0;
            if (!UiSupport.TryNum((longitud.Text ?? string.Empty).Trim(), out length)
                || !SelectiveDesviadorPlan.IsValidEvenAbove8(length))
            {
                if (showError) error.Text = "Longitud inválida: usa un número entero par mayor de 8\".";
                return false;
            }

            if (!UiSupport.TryNum((firstHeight.Text ?? string.Empty).Trim(), out first)
                || !SelectiveDesviadorPlan.IsValidEvenAbove8(first))
            {
                if (showError) error.Text = "Altura del primer nivel inválida: usa un número entero par mayor de 8\".";
                return false;
            }

            if (showError) error.Text = string.Empty;
            return true;
        }

        /// <summary>The disabled (post, level) cells for the current grid state. In a desviador off-cell the
        /// <see cref="SelectiveGridCell.Frente"/> holds the post index. A test seam too (I-22, InternalsVisibleTo).</summary>
        internal List<SelectiveGridCell> CurrentOffCells()
            => model.UnselectedCells()
                .Select(cell => new SelectiveGridCell { Frente = cell.Column, Level = cell.Row })
                .ToList();

        /// <summary>The working matrix state — a test seam (I-22, InternalsVisibleTo).</summary>
        internal SelectionMatrixModel Model => model;

        private SelectiveSafetySelection WorkingSelection(
            double length,
            double first,
            SafetySide selectedSide,
            IEnumerable<SelectiveGridCell> offCells)
        {
            var selection = new SelectiveSafetySelection
            {
                ElementId = elementId,
                Quantity = 1,
                Side = selectedSide,
                DesviadorLongitud = length,
                DesviadorPrimerNivelAltura = first
            };
            foreach (var cell in offCells ?? Enumerable.Empty<SelectiveGridCell>())
            {
                if (cell != null) selection.DesviadorOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            }

            return selection;
        }

        private static double Effective(double value, double fallback)
            => SelectiveDesviadorPlan.IsValidEvenAbove8(value) ? value : fallback;

        private SafetySide SelectedSide()
        {
            switch (side.SelectedIndex)
            {
                case 0: return SafetySide.Left;
                case 1: return SafetySide.Right;
                default: return SafetySide.Both;
            }
        }

        private static int SideIndex(SafetySide value)
        {
            switch (value)
            {
                case SafetySide.Left: return 0;
                case SafetySide.Right: return 1;
                default: return 2;
            }
        }

        private static SafetySide EffectiveSide(SafetySide value)
            => value == SafetySide.Left || value == SafetySide.Right ? value : SafetySide.Both;

        private static IReadOnlyList<int> FallbackCounts(int postCount, IReadOnlyList<int> levelsPerFrente, bool levelsArePerPost)
        {
            var count = Math.Max(1, postCount);
            var result = new int[count];
            for (var post = 0; post < count; post++)
            {
                if (levelsArePerPost)
                {
                    result[post] = levelsPerFrente != null && post < levelsPerFrente.Count
                        ? Math.Max(1, levelsPerFrente[post])
                        : 1;
                    continue;
                }

                var left = post > 0 && levelsPerFrente != null && post - 1 < levelsPerFrente.Count ? levelsPerFrente[post - 1] : 0;
                var right = levelsPerFrente != null && post < levelsPerFrente.Count ? levelsPerFrente[post] : 0;
                result[post] = Math.Max(1, Math.Max(left, right) + 1);
            }

            return result;
        }
    }
}
