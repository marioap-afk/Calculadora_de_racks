using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;

namespace RackCad.UI
{
    /// <summary>Configures the DESVIADOR post × load-level grid and its two even dimensions. The grid is the shared
    /// <see cref="SelectionMatrix"/> control (I-22) with absent cells for the jagged posts; toggling a cell still
    /// recomputes the live clearance note through the model's granular events. I-34 adds the shared
    /// <see cref="SelectionMatrixBulkBar"/>: this grid's column axis is a POSTE, and the note recomputes ONCE per bulk
    /// operation because it also listens to the aggregated <see cref="SelectionMatrixModel.ScopeApplied"/>.</summary>
    public sealed class SafetyDesviadorGridWindow : Window
    {
        private readonly SelectionMatrixModel model;
        private readonly SelectionMatrixBulkEditor bulkEditor;
        private readonly SelectionMatrixBulkBar bulkBar;
        private readonly IReadOnlyList<int> levelsPerPost;

        /// <summary>The off-cells this dialog was opened with. Those sitting on a column the grid renders as ABSENT —
        /// a front en blanco (I-33) — are DORMANT: invisible here, but they must survive the round trip untouched.</summary>
        private readonly IReadOnlyList<SelectiveGridCell> storedOffCells;
        private readonly ComboBox side;
        private readonly TextBox longitud;
        private readonly TextBox firstHeight;
        private readonly TextBlock note;
        private readonly TextBlock error;
        private readonly SelectiveRackSystem system;
        private readonly RackCatalog catalog;
        private readonly string elementId;

        /// <summary>
        /// PB-003 (I-32) — whether the aisle-face selector is offered. True (Selectivo, Dinámico) is the historical
        /// dialog, unchanged. Push Back passes false: its safety only ever lives at the LOW (entrance/exit) end — its
        /// own authority collapses every side to Left before anything is drawn — so the selector could only mislead.
        /// The control is still CONSTRUCTED (only its placement in the options row is skipped) so the result keeps
        /// reading a real value with no null checks.
        /// </summary>
        private readonly bool showSide;

        /// <summary>The side the dialog reports when the selector is hidden: the low (entrance/exit) aisle face.</summary>
        public const SafetySide LowEndSide = SafetySide.Left;

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
            bool fallbackLevelsArePerPost = false,
            bool showSide = true,
            bool allowBlankColumns = false)
        {
            this.elementId = elementId;
            this.system = system;
            this.catalog = catalog;
            this.showSide = showSide;
            this.storedOffCells = (offCells ?? Enumerable.Empty<SelectiveGridCell>())
                .Where(cell => cell != null)
                .ToList();

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
                : FallbackCounts(fallbackPostCount, fallbackLevelsPerFrente, fallbackLevelsArePerPost, allowBlankColumns);

            var posts = levelsPerPost.Count;
            var maxLevels = posts > 0 ? levelsPerPost.Max() : 0;
            model = SelectionMatrixModel.WithJaggedColumns(
                levelsPerPost,
                (offCells ?? Enumerable.Empty<SelectiveGridCell>())
                    .Where(cell => cell != null)
                    .Select(cell => new SelectionMatrixCell(cell.Frente, cell.Level)));

            Title = string.IsNullOrWhiteSpace(label) ? "Desviador" : label;
            Width = Math.Max(560, Math.Min(1000, 270 + posts * 46));
            // +36: the shared "Aplicar a:" row of I-34 sits under the grid and must not push the options out of view.
            Height = Math.Min(716, 366 + maxLevels * 30);
            MinWidth = 540;
            MinHeight = 366;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };
            var intro = new TextBlock
            {
                Text = "Marca los postes y niveles que llevan desviador (todos por defecto). El nivel 1 siempre se mide "
                     + "desde el primer TROQUEL_LARGUERO, aunque no haya larguero a piso; los niveles superiores van 6\" debajo del larguero."
                     + (showSide
                        ? " Elige si se coloca en la cara exterior izquierda, derecha espejeada o en ambas."
                        : " Se coloca en la cara de entrada/salida."),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11.5,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            var options = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            var sideLabel = new TextBlock { Text = "Lado:", VerticalAlignment = VerticalAlignment.Center };
            this.side = new ComboBox
            {
                Width = 92,
                Margin = new Thickness(5, 0, 16, 0),
                ToolTip = "Izquierdo = cara exterior frontal; Derecho = cara exterior posterior espejeada."
            };
            this.side.Items.Add("Izquierdo");
            this.side.Items.Add("Derecho");
            this.side.Items.Add("Ambas");
            this.side.SelectedIndex = showSide ? SideIndex(initial.Side) : SideIndex(LowEndSide);
            if (showSide)
            {
                options.Children.Add(sideLabel);
                options.Children.Add(this.side);
            }
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
            // I-34: ONE recomputation per bulk operation. The aggregated event is why the note does not run N times
            // when a whole level or a whole post is switched at once.
            model.ScopeApplied += (s, e) => RefreshNote();

            // I-34 — the shared bulk-edit row. The column axis of THIS grid is a POSTE (N frentes ⇒ N+1 postes), which
            // is the only thing that makes it different from the other two; it is DECLARED here, never derived.
            bulkEditor = new SelectionMatrixBulkEditor(model, SelectionMatrixScopeLabels.ByPoste);
            bulkBar = new SelectionMatrixBulkBar(bulkEditor);
            bulkBar.Attach(matrix);
            DockPanel.SetDock(bulkBar, Dock.Bottom);
            root.Children.Add(bulkBar);

            this.longitud.TextChanged += (s, e) => RefreshNote();
            this.firstHeight.TextChanged += (s, e) => RefreshNote();
            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, Content = matrix });
            Content = root;
            RefreshNote();
        }

        private void RefreshNote()
        {
            NoteRefreshCount++;
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
                OffCells = PersistedOffCells()
            };
        }

        /// <summary>
        /// What the dialog PERSISTS: the cells it shows as OFF plus the DORMANT ones stored on columns it rendered as
        /// absent (a front en blanco, I-33). Without the merge, accepting would silently erase a blank front's stored
        /// configuration, which must instead come back intact when the front is reactivated. The rule is the shared
        /// <see cref="SafetyDormantCells"/>, so the three level-indexed grids cannot drift.
        /// </summary>
        internal List<SelectiveGridCell> PersistedOffCells()
            => SafetyDormantCells.Merge(CurrentOffCells(), storedOffCells, levelsPerPost).ToList();

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

        /// <summary>The shared bulk-edit row and its editor — test seams (I-34).</summary>
        internal SelectionMatrixBulkBar BulkBar => bulkBar;

        internal SelectionMatrixBulkEditor BulkEditor => bulkEditor;

        /// <summary>How many times the live clearance note has been recomputed (I-34 test seam): a bulk operation must
        /// add exactly ONE, not one per cell.</summary>
        internal int NoteRefreshCount { get; private set; }

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

        /// <param name="allowBlankColumns">
        /// I-33, opt-in: when true a supplied count of ZERO is honoured instead of floored to one, so a front EN BLANCO
        /// — or a post whose only neighbours are blank — renders as a column with no cells. Default false keeps the
        /// historical flooring verbatim; the Selectivo never supplies a zero, so its grid is unchanged.
        /// </param>
        private static IReadOnlyList<int> FallbackCounts(
            int postCount, IReadOnlyList<int> levelsPerFrente, bool levelsArePerPost, bool allowBlankColumns)
        {
            var count = Math.Max(1, postCount);
            var result = new int[count];
            for (var post = 0; post < count; post++)
            {
                if (levelsArePerPost)
                {
                    var supplied = levelsPerFrente != null && post < levelsPerFrente.Count
                        ? levelsPerFrente[post]
                        : 1;
                    result[post] = allowBlankColumns ? Math.Max(0, supplied) : Math.Max(1, supplied);
                    continue;
                }

                var left = post > 0 && levelsPerFrente != null && post - 1 < levelsPerFrente.Count ? levelsPerFrente[post - 1] : 0;
                var right = levelsPerFrente != null && post < levelsPerFrente.Count ? levelsPerFrente[post] : 0;
                var adjacent = Math.Max(left, right);
                result[post] = allowBlankColumns && adjacent <= 0 ? 0 : Math.Max(1, adjacent + 1);
            }

            return result;
        }
    }
}
