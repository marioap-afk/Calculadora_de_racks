using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Configures a larguero tope: a grid of (frente × nivel) checkboxes (which largueros carry a tope, all on by
    /// default), whether it is one shared central tope or one per fondo (+ the side), and the SAQUE. Built in code
    /// (no XAML), like the other safety dialogs. On OK, <see cref="Result"/> holds the config.
    /// </summary>
    public sealed class SafetyTopeGridWindow : Window
    {
        private static readonly string[] SideLabels = { "Izquierda", "Derecha", "Ambos" };

        private readonly CheckBox[][] cells; // [frente][level]
        private readonly IReadOnlyList<int> levelsPerFrente;
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

        public TopeResult Result { get; private set; }

        public SafetyTopeGridWindow(string label, IReadOnlyList<int> levelsPerFrente, bool shared, SafetySide side, double saque, bool frontal, IEnumerable<SelectiveGridCell> offCells, int fondoCount = 1, int fondo = -1)
        {
            this.levelsPerFrente = levelsPerFrente ?? new List<int>();
            var off = new HashSet<(int, int)>();
            foreach (var c in offCells ?? Enumerable.Empty<SelectiveGridCell>())
            {
                if (c != null) off.Add((c.Frente, c.Level));
            }

            var frentes = this.levelsPerFrente.Count;
            var maxLevels = frentes > 0 ? this.levelsPerFrente.Max() : 0;

            Title = string.IsNullOrWhiteSpace(label) ? "Larguero tope" : label;
            Width = Math.Max(560, Math.Min(900, 260 + frentes * 46));
            Height = Math.Min(640, 260 + maxLevels * 30);
            MinWidth = 540; // the options row (compartido + lado + saque + frontal) must fit without clipping
            MinHeight = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };

            var intro = new TextBlock
            {
                Text = "Marca en qué frente y nivel de larguero va el tope (todos por defecto; la tarima de piso sin larguero no aparece). Va en el fondo central; puedes compartir "
                     + "uno central o uno por fondo (con el lado), y fijar el SAQUE.",
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            // ---- Bottom: options (shared, side, saque, frontal) — a WrapPanel so nothing clips on a narrow window ----
            var options = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            this.shared = new CheckBox { Content = "Compartido (uno central)", IsChecked = shared, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 0, 4), ToolTip = "Un solo tope central para ambos fondos; desmarcado = uno por fondo (según el lado)." };
            options.Children.Add(this.shared);

            options.Children.Add(new TextBlock { Text = "Lado:", Margin = new Thickness(16, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            this.side = new ComboBox { Width = 100, VerticalAlignment = VerticalAlignment.Center, ToolTip = "Cuando NO es compartido: qué fondo(s) del par central llevan tope." };
            foreach (var s in SideLabels) this.side.Items.Add(s);
            this.side.SelectedIndex = SideIndex(side);
            options.Children.Add(this.side);

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
            all.Click += (s, e) => SetAll(true);
            var none = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Ninguno", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
            none.Click += (s, e) => SetAll(false);
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

            // ---- The grid: columns = frentes, rows = levels (highest at top). ----
            cells = new CheckBox[frentes][];
            for (var f = 0; f < frentes; f++) cells[f] = new CheckBox[this.levelsPerFrente[f]];

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // level labels
            for (var f = 0; f < frentes; f++) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // header
            for (var l = 0; l < maxLevels; l++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (var f = 0; f < frentes; f++)
            {
                var head = new TextBlock { Text = "F" + (f + 1).ToString(CultureInfo.InvariantCulture), FontWeight = FontWeights.SemiBold, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 4) };
                Grid.SetRow(head, 0);
                Grid.SetColumn(head, f + 1);
                grid.Children.Add(head);
            }

            for (var l = maxLevels - 1; l >= 0; l--)
            {
                var rowIndex = maxLevels - l; // level 0 at the bottom
                var lbl = new TextBlock { Text = "Larg. " + (l + 1).ToString(CultureInfo.InvariantCulture), FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetRow(lbl, rowIndex);
                Grid.SetColumn(lbl, 0);
                grid.Children.Add(lbl);

                for (var f = 0; f < frentes; f++)
                {
                    if (l >= this.levelsPerFrente[f]) continue; // this frente has fewer levels
                    var cb = new CheckBox { IsChecked = !off.Contains((f, l)), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 2, 0, 2) };
                    Grid.SetRow(cb, rowIndex);
                    Grid.SetColumn(cb, f + 1);
                    grid.Children.Add(cb);
                    cells[f][l] = cb;
                }
            }

            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, Content = grid });
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

        private void SetAll(bool on)
        {
            foreach (var col in cells)
            {
                foreach (var cb in col)
                {
                    if (cb != null) cb.IsChecked = on;
                }
            }
        }

        private void OnOk()
        {
            var text = (saque.Text ?? string.Empty).Trim();
            if (!UiSupport.TryNum(text, out var saqueValue) || saqueValue <= 0.0)
            {
                error.Text = "Saque inválido: usa un número > 0.";
                return;
            }

            var result = new TopeResult
            {
                Shared = shared.IsChecked == true,
                Side = SideFromIndex(side.SelectedIndex),
                Saque = saqueValue,
                Frontal = frontal.IsChecked == true,
                Fondo = fondoBox == null || fondoBox.SelectedIndex <= 0 ? -1 : fondoBox.SelectedIndex - 1
            };

            for (var f = 0; f < cells.Length; f++)
            {
                for (var l = 0; l < cells[f].Length; l++)
                {
                    if (cells[f][l] != null && cells[f][l].IsChecked != true)
                    {
                        result.OffCells.Add(new SelectiveGridCell { Frente = f, Level = l }); // an off cell
                    }
                }
            }

            Result = result;
            DialogResult = true;
        }
    }
}
