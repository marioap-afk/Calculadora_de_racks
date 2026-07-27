using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI.Controls;

namespace RackCad.UI
{
    /// <summary>
    /// Configures a larguero tope: a grid of (frente × nivel) checkboxes (which largueros carry a tope, all on by
    /// default), whether it is one shared central tope or one per fondo (+ the side), and the SAQUE. Built in code
    /// (no XAML), like the other safety dialogs; the grid is the shared <see cref="SelectionMatrix"/> control (I-22)
    /// with absent cells for the jagged frentes. On OK, <see cref="Result"/> holds the config.
    /// <para>
    /// I-34 adds the shared <see cref="SelectionMatrixBulkBar"/>, whose column axis here is a FRENTE. This one dialog
    /// serves TWO matrices — the Selectivo's tope and Push Back's REAR tope — so adopting it once covers both, and it
    /// stays free of any per-system branch: what differs between them already travels through its parameters.
    /// </para>
    /// </summary>
    public sealed class SafetyTopeGridWindow : Window
    {
        private static readonly string[] SideLabels = { "Izquierda", "Derecha", "Ambos" };

        private readonly SelectionMatrixModel model;
        private readonly SelectionMatrixBulkEditor bulkEditor;
        private readonly SelectionMatrixBulkBar bulkBar;
        private readonly CheckBox shared;
        private readonly ComboBox side;
        private readonly ComboBox fondoBox; // null when there is a single fondo (no choice)
        private readonly TextBox saque;
        private readonly TextBlock error;

        public sealed class TopeResult
        {
            public bool Shared;
            public SafetySide Side;
            public double Saque;
            public bool Frontal;
            public int Fondo = -1; // -1 = automatic central fondo
            public List<SelectiveGridCell> OffCells = new List<SelectiveGridCell>();
        }

        private readonly CheckBox frontal;
        private readonly IReadOnlyList<int> levelsPerColumn;

        /// <summary>The off-cells this dialog was opened with; the ones on absent (blank-front) columns are DORMANT
        /// and must survive the round trip untouched (I-33).</summary>
        private readonly IReadOnlyList<SelectiveGridCell> storedOffCells;

        public TopeResult Result { get; private set; }

        /// <param name="showSharedAndSide">
        /// PB-006 (I-32) — whether the "Compartido (uno central)" checkbox and the "Lado" selector are offered. True
        /// (Selectivo) is the historical dialog, unchanged. Push Back passes false: it has a single depth line, so
        /// there is no central-vs-per-fondo choice and no side to pick — both controls were inert there (its adapter
        /// only ever reads SAQUE and the off-cells). The controls are still CONSTRUCTED so the result keeps reading a
        /// real value with no null checks; only their placement in the options row and the sentence describing them
        /// are skipped.
        /// </param>
        public SafetyTopeGridWindow(string label, IReadOnlyList<int> levelsPerFrente, bool shared, SafetySide side, double saque, bool frontal, IEnumerable<SelectiveGridCell> offCells, int fondoCount = 1, int fondo = -1, bool showSharedAndSide = true)
        {
            var levels = levelsPerFrente ?? new List<int>();
            // A supplied count of ZERO already renders as an absent column here (no flooring), which is what a front EN
            // BLANCO needs (I-33); its stored cells are kept dormant by PersistedOffCells.
            levelsPerColumn = levels;
            storedOffCells = (offCells ?? Enumerable.Empty<SelectiveGridCell>())
                .Where(cell => cell != null)
                .ToList();
            model = SelectionMatrixModel.WithJaggedColumns(
                levels,
                storedOffCells.Select(cell => new SelectionMatrixCell(cell.Frente, cell.Level)));

            var frentes = levels.Count;
            var maxLevels = frentes > 0 ? levels.Max() : 0;

            Title = string.IsNullOrWhiteSpace(label) ? "Larguero tope" : label;
            Width = Math.Max(560, Math.Min(900, 260 + frentes * 46));
            // +36: the shared "Aplicar a:" row of I-34 sits under the grid and must not push the options out of view.
            Height = Math.Min(676, 296 + maxLevels * 30);
            MinWidth = 540; // the options row (compartido + lado + saque + frontal) must fit without clipping
            MinHeight = 336;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };

            var intro = new TextBlock
            {
                Text = "Marca en qué frente y nivel de larguero va el tope (todos por defecto; la tarima de piso sin larguero no aparece). "
                     + (showSharedAndSide
                        ? "Va en el fondo central; puedes compartir uno central o uno por fondo (con el lado), y fijar el SAQUE."
                        : "Fija el SAQUE."),
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            // ---- Bottom: options (shared, side, saque, frontal) — a WrapPanel so nothing clips on a narrow window ----
            var options = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            // PB-006: build both controls ALWAYS (BuildResult reads them without null checks) and only add them to the
            // row when the system has a real choice to make. Collapsing instead of skipping would keep them in the
            // visual tree and make "the control is not offered" unverifiable.
            this.shared = new CheckBox { Content = "Compartido (uno central)", IsChecked = shared, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 0, 4), ToolTip = "Un solo tope central para ambos fondos; desmarcado = uno por fondo (según el lado)." };
            var sideLabel = new TextBlock { Text = "Lado:", Margin = new Thickness(16, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center };
            this.side = new ComboBox { Width = 100, VerticalAlignment = VerticalAlignment.Center, ToolTip = "Cuando NO es compartido: qué fondo(s) del par central llevan tope." };
            foreach (var s in SideLabels) this.side.Items.Add(s);
            this.side.SelectedIndex = SideIndex(side);
            if (showSharedAndSide)
            {
                options.Children.Add(this.shared);
                options.Children.Add(sideLabel);
                options.Children.Add(this.side);
            }

            options.Children.Add(new TextBlock { Text = "Saque (in):", Margin = new Thickness(16, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            this.saque = new TextBox { Width = 56, VerticalAlignment = VerticalAlignment.Center, Text = (saque > 0 ? saque : SelectiveSafetyDefaults.TopeSaque).ToString(CultureInfo.InvariantCulture) };
            options.Children.Add(this.saque);

            // Fondo picker only when there is a real choice (2+ fondos); "Central (auto)" keeps the automatic middle.
            if (fondoCount >= 2)
            {
                options.Children.Add(new TextBlock { Text = "Fondo:", Margin = new Thickness(16, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
                this.fondoBox = new ComboBox { Width = 120, VerticalAlignment = VerticalAlignment.Center, ToolTip = "En qué fondo va el tope. 'Central (auto)' elige el fondo del medio." };
                this.fondoBox.Items.Add("Central (auto)");
                for (var k = 0; k < fondoCount; k++) this.fondoBox.Items.Add("Fondo " + (k + 1).ToString(CultureInfo.InvariantCulture));
                this.fondoBox.SelectedIndex = fondo >= 0 && fondo < fondoCount ? fondo + 1 : 0;
                options.Children.Add(this.fondoBox);
            }

            this.frontal = new CheckBox { Content = "Dibujar en frontal", IsChecked = frontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 4, 0, 4), ToolTip = "Además de lateral y planta, dibujarlo también en la vista frontal." };
            options.Children.Add(this.frontal);
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

            error = new TextBlock { FontSize = 11, Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) };
            DockPanel.SetDock(error, Dock.Bottom);
            root.Children.Add(error);

            // ---- The grid: columns = frentes, rows = levels (highest at top), via the shared control. ----
            var matrix = new SelectionMatrix
            {
                Model = model,
                InvertRows = true, // level 0 at the bottom
                ColumnHeaders = Enumerable.Range(0, frentes)
                    .Select(f => "F" + (f + 1).ToString(CultureInfo.InvariantCulture)).ToArray(),
                RowHeaders = Enumerable.Range(0, maxLevels)
                    .Select(l => "Larg. " + (l + 1).ToString(CultureInfo.InvariantCulture)).ToArray()
            };

            // I-34 — the shared bulk-edit row, declared with the FRENTE axis (both consumers of this dialog index the
            // grid by frente; only their persistence destination differs).
            bulkEditor = new SelectionMatrixBulkEditor(model, SelectionMatrixScopeLabels.ByFrente);
            bulkBar = new SelectionMatrixBulkBar(bulkEditor);
            bulkBar.Attach(matrix);
            DockPanel.SetDock(bulkBar, Dock.Bottom);
            root.Children.Add(bulkBar);

            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, Content = matrix });
            Content = root;
        }

        private static int SideIndex(SafetySide side)
        {
            switch (side)
            {
                case SafetySide.Left: return 0;
                case SafetySide.Right: return 1;
                default: return 2; // Both
            }
        }

        private static SafetySide SideFromIndex(int index)
        {
            switch (index)
            {
                case 0: return SafetySide.Left;
                case 1: return SafetySide.Right;
                default: return SafetySide.Both;
            }
        }

        /// <summary>The working matrix state — a test seam (I-22, InternalsVisibleTo).</summary>
        internal SelectionMatrixModel Model => model;

        /// <summary>The shared bulk-edit row and its editor — test seams (I-34).</summary>
        internal SelectionMatrixBulkBar BulkBar => bulkBar;

        internal SelectionMatrixBulkEditor BulkEditor => bulkEditor;

        /// <summary>Builds the config from the current controls and grid, or null when the SAQUE is invalid (the error
        /// is shown). Shared by OK and the tests, which cannot set <see cref="Window.DialogResult"/> without ShowDialog.</summary>
        internal TopeResult BuildResult()
        {
            var text = (saque.Text ?? string.Empty).Trim();
            if (!UiSupport.TryNum(text, out var saqueValue) || saqueValue <= 0.0)
            {
                error.Text = "Saque inválido: usa un número > 0.";
                return null;
            }

            var result = new TopeResult
            {
                Shared = shared.IsChecked == true,
                Side = SideFromIndex(side.SelectedIndex),
                Saque = saqueValue,
                Frontal = frontal.IsChecked == true,
                Fondo = fondoBox == null || fondoBox.SelectedIndex <= 0 ? -1 : fondoBox.SelectedIndex - 1
            };

            // The cells shown as OFF plus the DORMANT ones stored on absent (blank-front) columns, which this grid never
            // rendered and therefore could not report; dropping them would erase a blank front's configuration (I-33).
            var live = model.UnselectedCells()
                .Select(cell => new SelectiveGridCell { Frente = cell.Column, Level = cell.Row });
            result.OffCells.AddRange(SafetyDormantCells.Merge(live, storedOffCells, levelsPerColumn));

            return result;
        }

        private void OnOk()
        {
            var result = BuildResult();
            if (result == null)
            {
                return; // invalid saque; the error is already shown
            }

            Result = result;
            DialogResult = true;
        }
    }
}
