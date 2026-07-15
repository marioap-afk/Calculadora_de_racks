using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Configures the parrilla (deck): a grid of (frente × nivel) checkboxes picking which load positions carry decks
    /// (all on by default), per-view "Dibujar en frontal / lateral" toggles (never in planta), and the manual FRENTE /
    /// CANTIDAD pair. Each cell shows how many decks it actually gets and the footer the rack total — both computed
    /// through <see cref="SelectiveParrillaPlan"/>, the same rule the draw and the BOM use, so what is read here is what
    /// gets drawn and quoted. Built in code (no XAML), like the other safety dialogs. On OK, <see cref="Result"/> holds
    /// the config.
    /// </summary>
    public sealed class SafetyParrillaGridWindow : Window
    {
        private readonly CheckBox[][] cells; // [frente][level]
        private readonly TextBlock[][] counts; // [frente][level] — that cell's live deck count
        private readonly IReadOnlyList<int> levelsPerFrente;
        private readonly IReadOnlyList<SelectiveParrillaPlan.Cell> plan; // null = geometry unavailable (no live count)
        private readonly CheckBox frontal;
        private readonly CheckBox lateral;
        private readonly TextBox frenteBox;
        private readonly TextBox cantidadBox;
        private readonly TextBlock summary;
        private bool ready; // suppresses recounting while the dialog is still being built

        public sealed class ParrillaResult
        {
            public bool Frontal;
            public bool Lateral;
            public double Frente; // 0 = one deck per tarima at the tarima's own frente
            public int Cantidad;  // 0 = derived from the frente (how many fit)
            public List<SelectiveGridCell> OffCells = new List<SelectiveGridCell>();
        }

        public ParrillaResult Result { get; private set; }

        public SafetyParrillaGridWindow(
            string label, IReadOnlyList<int> levelsPerFrente, bool frontal, bool lateral, double frente, int cantidad,
            IEnumerable<SelectiveGridCell> offCells, IReadOnlyList<SelectiveParrillaPlan.Cell> plan = null)
        {
            this.levelsPerFrente = levelsPerFrente ?? new List<int>();
            this.plan = plan;
            var off = new HashSet<(int, int)>();
            foreach (var c in offCells ?? Enumerable.Empty<SelectiveGridCell>())
            {
                if (c != null) off.Add((c.Frente, c.Level));
            }

            var frentes = this.levelsPerFrente.Count;
            var maxLevels = frentes > 0 ? this.levelsPerFrente.Max() : 0;

            Title = string.IsNullOrWhiteSpace(label) ? "Parrilla" : label;
            Width = Math.Max(560, Math.Min(920, 300 + frentes * 52));
            Height = Math.Min(680, 320 + maxLevels * 30);
            MinWidth = 520;
            MinHeight = 340;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Segoe UI");
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) });
            if (TryFindResource("WindowBackgroundBrush") is Brush background) Background = background;

            var root = new DockPanel { Margin = new Thickness(14) };

            var intro = new TextBlock
            {
                Text = "Va UNA PARRILLA POR TARIMA. Marca en qué frente y nivel de larguero van (la tarima de piso sin larguero no aparece); el número junto a "
                     + "cada casilla es cuántas se dibujan ahí. Elige en qué vistas dibujarlas (en planta no se dibuja).",
                TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            // ---- Bottom: view toggles + the manual frente/cantidad pair, then the live total ----
            var options = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            this.frontal = new CheckBox { Content = "Dibujar en frontal", IsChecked = frontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 16, 4) };
            this.lateral = new CheckBox { Content = "Dibujar en lateral", IsChecked = lateral, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 16, 4) };
            options.Children.Add(this.frontal);
            options.Children.Add(this.lateral);

            options.Children.Add(new TextBlock { Text = "Frente (in):", Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            this.frenteBox = new TextBox
            {
                Width = 60, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0),
                Text = frente > 0.0 ? frente.ToString(CultureInfo.InvariantCulture) : string.Empty,
                ToolTip = "Ancho de cada parrilla. Vacío = el mismo frente que la tarima."
            };
            this.frenteBox.TextChanged += (s, e) => Recount();
            options.Children.Add(this.frenteBox);

            options.Children.Add(new TextBlock { Text = "Cantidad:", Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            this.cantidadBox = new TextBox
            {
                Width = 46, VerticalAlignment = VerticalAlignment.Center,
                Text = cantidad > 0 ? cantidad.ToString(CultureInfo.InvariantCulture) : string.Empty,
                ToolTip = "Cuántas parrillas por posición de carga. Vacío = las que quepan (una por tarima). "
                        + "En un medio frente la cantidad es POR TRAMO: cada tramo es su propia posición de carga."
            };
            this.cantidadBox.TextChanged += (s, e) => Recount();
            options.Children.Add(this.cantidadBox);
            DockPanel.SetDock(options, Dock.Bottom);
            root.Children.Add(options);

            this.summary = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 11.5, Margin = new Thickness(0, 8, 0, 0) };
            DockPanel.SetDock(this.summary, Dock.Bottom);
            root.Children.Add(this.summary);

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

            // ---- The grid: columns = frentes, rows = levels (highest at top). Each cell = a checkbox + its deck count. ----
            cells = new CheckBox[frentes][];
            counts = new TextBlock[frentes][];
            for (var f = 0; f < frentes; f++)
            {
                cells[f] = new CheckBox[this.levelsPerFrente[f]];
                counts[f] = new TextBlock[this.levelsPerFrente[f]];
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // level labels
            for (var f = 0; f < frentes; f++) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
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
                    var box = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 2) };
                    var cb = new CheckBox { IsChecked = !off.Contains((f, l)), VerticalAlignment = VerticalAlignment.Center };
                    cb.Checked += (s, e) => Recount();
                    cb.Unchecked += (s, e) => Recount();
                    var n = new TextBlock { FontSize = 10.5, MinWidth = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(3, 0, 0, 0) };
                    box.Children.Add(cb);
                    box.Children.Add(n);
                    Grid.SetRow(box, rowIndex);
                    Grid.SetColumn(box, f + 1);
                    grid.Children.Add(box);
                    cells[f][l] = cb;
                    counts[f][l] = n;
                }
            }

            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, Content = grid });
            Content = root;

            ready = true;
            Recount();
        }

        private void SetAll(bool on)
        {
            ready = false; // one recount at the end, not one per checkbox
            foreach (var col in cells)
            {
                foreach (var cb in col)
                {
                    if (cb != null) cb.IsChecked = on;
                }
            }

            ready = true;
            Recount();
        }

        /// <summary>Reads both boxes (0 = blank). Returns the complaint to show, or null when both are usable.</summary>
        private string ReadInputs(out double frente, out int cantidad)
        {
            frente = 0.0;
            cantidad = 0;

            var frenteText = (frenteBox.Text ?? string.Empty).Trim();
            if (frenteText.Length > 0 && (!UiSupport.TryNum(frenteText, out frente) || frente <= 0.0))
            {
                frente = 0.0;
                return "Frente inválido: déjalo vacío (el frente de la tarima) o usa un número > 0.";
            }

            var cantidadText = (cantidadBox.Text ?? string.Empty).Trim();
            if (cantidadText.Length > 0 && (!UiSupport.TryInt(cantidadText, out cantidad) || cantidad <= 0))
            {
                cantidad = 0;
                return "Cantidad inválida: déjala vacía (las que quepan) o usa un entero > 0.";
            }

            return null;
        }

        /// <summary>Whether that cell carries decks. A cell the grid does not show (a fondo with more levels than the main
        /// matrix) counts as ON, exactly as <c>ParrillaAt</c> reads it — so the total matches the BOM.</summary>
        private bool IsOn(int frente, int level)
            => frente < 0 || frente >= cells.Length || level < 0 || level >= cells[frente].Length
               || cells[frente][level] == null || cells[frente][level].IsChecked == true;

        /// <summary>Repaints every cell's deck count and the footer total for what is typed right now. Sets the existing
        /// TextBlocks in place — the grid is never rebuilt.</summary>
        private void Recount()
        {
            if (!ready)
            {
                return;
            }

            var complaint = ReadInputs(out var frente, out var cantidad);
            if (plan == null)
            {
                summary.Text = complaint ?? "La geometría aún no es válida; puedes guardar la selección, pero el conteo se mostrará al resolver el rack.";
                summary.Foreground = WarnBrush(true);
                return;
            }

            foreach (var col in counts)
            {
                foreach (var t in col)
                {
                    if (t != null) t.Text = string.Empty;
                }
            }

            var total = 0;
            var tooMany = new List<string>();
            var empty = new List<string>();
            foreach (var cell in plan)
            {
                if (!IsOn(cell.Frente, cell.Level))
                {
                    continue;
                }

                var n = SelectiveParrillaPlan.CountIn(cell, frente, cantidad);
                total += n;
                if (cell.Frente < counts.Length && cell.Level < counts[cell.Frente].Length && counts[cell.Frente][cell.Level] != null)
                {
                    counts[cell.Frente][cell.Level].Text = n.ToString(CultureInfo.InvariantCulture);
                }

                // "Draws nothing" is answered by the number just painted, NOT by MaxCountIn: that one is a MIN across the
                // cell's load rows, so a medio frente with one inherently-empty tramo would claim the whole cell is empty
                // while the cell shows the decks the other tramos really get.
                var name = CellName(cell.Frente, cell.Level);
                if (n <= 0)
                {
                    empty.Add(name);
                }
                else if (cantidad > 0)
                {
                    var max = SelectiveParrillaPlan.MaxCountIn(cell, frente);
                    if (max > 0 && cantidad > max) tooMany.Add(name + " (caben " + max.ToString(CultureInfo.InvariantCulture) + ")");
                }
            }

            if (complaint != null)
            {
                summary.Text = complaint;
                summary.Foreground = WarnBrush(true);
                return;
            }

            var text = "Total: " + total.ToString(CultureInfo.InvariantCulture) + (total == 1 ? " parrilla." : " parrillas.");
            if (tooMany.Count > 0) text += " No caben " + cantidad.ToString(CultureInfo.InvariantCulture) + " en " + Join(tooMany) + ".";
            if (empty.Count > 0) text += " No cabe ninguna en " + Join(empty) + ".";
            summary.Text = text;
            summary.Foreground = WarnBrush(tooMany.Count > 0 || empty.Count > 0);
        }

        private static string CellName(int frente, int level)
            => "F" + (frente + 1).ToString(CultureInfo.InvariantCulture) + "/N" + (level + 1).ToString(CultureInfo.InvariantCulture);

        private static string Join(IReadOnlyList<string> names)
            => string.Join(", ", names.Take(4)) + (names.Count > 4 ? " y " + (names.Count - 4).ToString(CultureInfo.InvariantCulture) + " más" : string.Empty);

        private Brush WarnBrush(bool warn)
            => warn
                ? UiSupport.FrozenBrush(Color.FromRgb(0xB0, 0x00, 0x20))
                : (TryFindResource("MutedTextBrush") as Brush ?? UiSupport.FrozenBrush(Color.FromRgb(0x70, 0x70, 0x70)));

        private void OnOk()
        {
            var complaint = ReadInputs(out var frente, out var cantidad);
            if (complaint != null)
            {
                MessageBox.Show(this, complaint, "Parrilla", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // A forced cantidad that does not fit is refused HERE rather than silently trimmed at draw time.
            if (cantidad > 0 && plan != null)
            {
                var offenders = plan
                    .Where(c => IsOn(c.Frente, c.Level))
                    .Select(c => new { Cell = c, Max = SelectiveParrillaPlan.MaxCountIn(c, frente) })
                    .Where(x => x.Max > 0 && cantidad > x.Max)
                    .Select(x => CellName(x.Cell.Frente, x.Cell.Level) + ": caben " + x.Max.ToString(CultureInfo.InvariantCulture))
                    .ToList();
                if (offenders.Count > 0)
                {
                    MessageBox.Show(
                        this,
                        "No caben " + cantidad.ToString(CultureInfo.InvariantCulture) + " parrillas de ese frente en:\n\n"
                        + string.Join("\n", offenders.Take(10)) + (offenders.Count > 10 ? "\n…" : string.Empty)
                        + "\n\nBaja la cantidad, reduce el frente, o apaga esas casillas.",
                        "Parrilla", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var result = new ParrillaResult
            {
                Frontal = frontal.IsChecked == true,
                Lateral = lateral.IsChecked == true,
                Frente = frente,
                Cantidad = cantidad
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
