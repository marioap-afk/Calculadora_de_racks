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
    /// Configures the parrilla (deck): a grid of (frente × nivel) checkboxes picking which load positions carry a deck
    /// (all on by default), plus per-view "Dibujar en frontal / lateral" toggles (never in planta). Built in code
    /// (no XAML), like the other safety dialogs. On OK, <see cref="Result"/> holds the config.
    /// </summary>
    public sealed class SafetyParrillaGridWindow : Window
    {
        private readonly CheckBox[][] cells; // [frente][level]
        private readonly IReadOnlyList<int> levelsPerFrente;
        private readonly CheckBox frontal;
        private readonly CheckBox lateral;
        private readonly TextBox frenteBox;

        public sealed class ParrillaResult
        {
            public bool Frontal;
            public bool Lateral;
            public double Frente; // 0 = one deck per tarima at the tarima's own frente
            public List<SelectiveGridCell> OffCells = new List<SelectiveGridCell>();
        }

        public ParrillaResult Result { get; private set; }

        public SafetyParrillaGridWindow(string label, IReadOnlyList<int> levelsPerFrente, bool frontal, bool lateral, double frente, IEnumerable<SelectiveGridCell> offCells)
        {
            this.levelsPerFrente = levelsPerFrente ?? new List<int>();
            var off = new HashSet<(int, int)>();
            foreach (var c in offCells ?? Enumerable.Empty<SelectiveGridCell>())
            {
                if (c != null) off.Add((c.Frente, c.Level));
            }

            var frentes = this.levelsPerFrente.Count;
            var maxLevels = frentes > 0 ? this.levelsPerFrente.Max() : 0;

            Title = string.IsNullOrWhiteSpace(label) ? "Parrilla" : label;
            Width = Math.Max(520, Math.Min(900, 260 + frentes * 46));
            Height = Math.Min(640, 260 + maxLevels * 30);
            MinWidth = 460;
            MinHeight = 280;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };

            var intro = new TextBlock
            {
                Text = "Marca en qué frente y nivel va la parrilla (todas por defecto). La parrilla se apoya sobre los "
                     + "largueros de cada nivel. Elige en qué vistas dibujarla (en planta no se dibuja).",
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            // ---- Bottom: the two view toggles ----
            var options = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            this.frontal = new CheckBox { Content = "Dibujar en frontal", IsChecked = frontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 16, 4) };
            this.lateral = new CheckBox { Content = "Dibujar en lateral", IsChecked = lateral, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 16, 4) };
            options.Children.Add(this.frontal);
            options.Children.Add(this.lateral);

            options.Children.Add(new TextBlock { Text = "Frente parrilla (in):", Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            this.frenteBox = new TextBox { Width = 64, VerticalAlignment = VerticalAlignment.Center, Text = frente > 0.0 ? frente.ToString(CultureInfo.InvariantCulture) : string.Empty, ToolTip = "Vacío = una parrilla por tarima (mismo frente que la tarima). Un valor fija ese ancho y ajusta cuántas caben (p.ej. 2 parrillas bajo 3 tarimas)." };
            options.Children.Add(this.frenteBox);
            DockPanel.SetDock(options, Dock.Bottom);
            root.Children.Add(options);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
            var all = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Todas", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
            all.Click += (s, e) => SetAll(true);
            var none = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Ninguna", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
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
                var lbl = new TextBlock { Text = "Nivel " + (l + 1).ToString(CultureInfo.InvariantCulture), FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
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
            var frenteText = (frenteBox.Text ?? string.Empty).Trim();
            var frente = 0.0;
            if (frenteText.Length > 0 && (!double.TryParse(frenteText, NumberStyles.Float, CultureInfo.InvariantCulture, out frente) || frente <= 0.0))
            {
                MessageBox.Show(this, "Frente de parrilla inválido: deja vacío (una por tarima) o usa un número > 0.", "Parrilla", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = new ParrillaResult
            {
                Frontal = frontal.IsChecked == true,
                Lateral = lateral.IsChecked == true,
                Frente = frente
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
