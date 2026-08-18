using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;

namespace RackCad.UI
{
    /// <summary>
    /// Configures the parrilla (deck): a grid of (frente × nivel) checkboxes picking which load positions carry decks
    /// (all on by default), per-view "Dibujar en frontal / lateral" toggles (never in planta), and the manual FRENTE /
    /// CANTIDAD pair. Each cell shows how many decks it actually gets and the footer the rack total — both computed
    /// through <see cref="SelectiveParrillaPlan"/>, the same rule the draw and the BOM use, so what is read here is what
    /// gets drawn and quoted. Built in code (no XAML), like the other safety dialogs. On OK, <see cref="Result"/> holds
    /// the config.
    /// <para>
    /// I-34 addendum (Owner, 2026-07-27): this grid adopts the shared <see cref="SelectionMatrix"/> and the shared
    /// <see cref="SelectionMatrixBulkBar"/> (column axis = FRENTE) — the adoption I-22 had deferred. The Owner's
    /// condition is that the LIVE DECK COUNT survives, so it is not reduced to a bare check box: it rides on the
    /// control's neutral <see cref="SelectionMatrix.CellAdornment"/>, and the recount runs ONCE per operation because
    /// the model publishes one aggregated <see cref="SelectionMatrixModel.ScopeApplied"/>.
    /// </para>
    /// </summary>
    public sealed class SafetyParrillaGridWindow : Window
    {
        private readonly SelectionMatrixModel model;
        private readonly SelectionMatrix matrix;
        private readonly SelectionMatrixBulkEditor bulkEditor;
        private readonly SelectionMatrixBulkBar bulkBar;
        private readonly IReadOnlyList<int> levelsPerFrente;
        private readonly IReadOnlyList<SelectiveParrillaPlan.Cell> plan; // null = geometry unavailable (no live count)

        /// <summary>Each cell's live deck count, as text. The control reads it through the adornment provider; the
        /// dictionary is rewritten on every recount and never rebuilds a visual.</summary>
        private readonly Dictionary<SelectionMatrixCell, string> countText =
            new Dictionary<SelectionMatrixCell, string>();

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

            var frentes = this.levelsPerFrente.Count;
            var maxLevels = frentes > 0 ? this.levelsPerFrente.Max() : 0;

            // The jagged shape the hand-built grid produced (cells[f] sized by that frente's level count) is exactly
            // what WithJaggedColumns yields; off-cells outside the grid are ignored, as the HashSet lookup ignored them.
            model = SelectionMatrixModel.WithJaggedColumns(
                this.levelsPerFrente,
                (offCells ?? Enumerable.Empty<SelectiveGridCell>())
                    .Where(cell => cell != null)
                    .Select(cell => new SelectionMatrixCell(cell.Frente, cell.Level)));

            Title = string.IsNullOrWhiteSpace(label) ? "Parrilla" : label;
            Width = Math.Max(560, Math.Min(920, 300 + frentes * 52));
            // +36: the shared "Aplicar a:" row of I-34 sits under the grid and must not push the options out of view.
            Height = Math.Min(716, 356 + maxLevels * 30);
            MinWidth = 520;
            MinHeight = 376;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            // I-39D: el chrome del arquetipo C en una sola fuente. Sustituye las cuatro sentencias que las
            // diez ventanas repetian a mano; los valores son los MISMOS, asi que no cambia un pixel.
            DialogWindowChrome.Apply(this);

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
            all.Click += (s, e) => model.SetAll(true);
            var none = new Button { Style = TryFindResource("SecondaryButtonStyle") as Style, Content = "Ninguna", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
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

            // ---- The grid: columns = frentes, rows = levels (highest at top), via the shared control. The per-cell
            // deck count rides on the control's NEUTRAL adornment, so the count survives the adoption (Owner §0.4). ----
            matrix = new SelectionMatrix
            {
                InvertRows = true, // level 0 at the bottom
                ColumnHeaders = Enumerable.Range(0, frentes)
                    .Select(f => "F" + (f + 1).ToString(CultureInfo.InvariantCulture)).ToArray(),
                RowHeaders = Enumerable.Range(0, maxLevels)
                    .Select(l => "Larg. " + (l + 1).ToString(CultureInfo.InvariantCulture)).ToArray(),
                CellAdornment = cell => countText.TryGetValue(cell, out var text) ? text : string.Empty
            };
            matrix.Model = model;

            // One recount per OPERATION: a click, a Todas/Ninguna, or a whole scoped bulk edit.
            model.CellChanged += (s, e) => Recount();
            model.BulkChanged += (s, e) => Recount();
            model.ScopeApplied += (s, e) => Recount();

            bulkEditor = new SelectionMatrixBulkEditor(model, SelectionMatrixScopeLabels.ByFrente);
            bulkBar = new SelectionMatrixBulkBar(bulkEditor);
            bulkBar.Attach(matrix);
            DockPanel.SetDock(bulkBar, Dock.Bottom);
            root.Children.Add(bulkBar);

            root.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, Content = matrix });
            Content = root;

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
        /// matrix) counts as ON, exactly as <c>ParrillaAt</c> reads it — so the total matches the BOM. An ABSENT cell of
        /// the jagged grid is that same case, so it keeps answering ON and the count is unchanged by the adoption.</summary>
        private bool IsOn(int frente, int level)
            => frente < 0 || frente >= model.Columns || level < 0 || level >= model.Rows
               || model.IsAbsent(frente, level) || model.IsSelected(frente, level);

        /// <summary>Repaints every cell's deck count and the footer total for what is typed right now. Sets the existing
        /// TextBlocks in place — the grid is never rebuilt.</summary>
        private void Recount()
        {
            if (!ready)
            {
                return;
            }

            RecountCount++;
            var complaint = ReadInputs(out var frente, out var cantidad);
            if (plan == null)
            {
                countText.Clear();
                matrix.RefreshAdornments();
                summary.Text = complaint ?? "La geometría aún no es válida; puedes guardar la selección, pero el conteo se mostrará al resolver el rack.";
                summary.Foreground = WarnBrush(true);
                return;
            }

            countText.Clear();

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
                countText[new SelectionMatrixCell(cell.Frente, cell.Level)] =
                    n.ToString(CultureInfo.InvariantCulture);

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

            matrix.RefreshAdornments();

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

        /// <summary>I-39D: <c>MutedTextBrush</c> no esta definido en ningun diccionario del repositorio, asi que el
        /// <c>TryFindResource</c> devolvia siempre null y ganaba el respaldo. Se conserva el MISMO gris; desaparece
        /// solo la busqueda muerta.</summary>
        private static Brush WarnBrush(bool warn)
            => warn
                ? UiSupport.FrozenBrush(Color.FromRgb(0xB0, 0x00, 0x20))
                : UiSupport.FrozenBrush(Color.FromRgb(0x70, 0x70, 0x70));

        /// <summary>The working matrix state — a test seam (I-34).</summary>
        internal SelectionMatrixModel Model => model;

        /// <summary>The shared bulk-edit row and its editor — test seams (I-34).</summary>
        internal SelectionMatrixBulkBar BulkBar => bulkBar;

        internal SelectionMatrixBulkEditor BulkEditor => bulkEditor;

        /// <summary>How many times the live count has been recomputed (I-34 test seam): a bulk operation must add
        /// exactly ONE, not one per cell.</summary>
        internal int RecountCount { get; private set; }

        /// <summary>The live deck count shown at a cell, as the user reads it (test seam, I-34).</summary>
        internal string CountTextFor(int frente, int level)
            => matrix.AdornmentFor(frente, level)?.Text ?? string.Empty;

        /// <summary>The footer total, as the user reads it (test seam, I-34).</summary>
        internal string SummaryText => summary.Text;

        /// <summary>Builds the config from the current controls and grid, without the modal OK path (test seam, I-34).
        /// Shared with <see cref="OnOk"/> so what the tests read is what the dialog returns.</summary>
        internal ParrillaResult BuildResultForTest()
        {
            ReadInputs(out var frente, out var cantidad);
            return BuildResult(frente, cantidad);
        }

        private ParrillaResult BuildResult(double frente, int cantidad)
        {
            var result = new ParrillaResult
            {
                Frontal = frontal.IsChecked == true,
                Lateral = lateral.IsChecked == true,
                Frente = frente,
                Cantidad = cantidad
            };

            // Only the deactivations, as before: an absent cell is never "unselected", so the set is unchanged.
            foreach (var cell in model.UnselectedCells())
            {
                result.OffCells.Add(new SelectiveGridCell { Frente = cell.Column, Level = cell.Row });
            }

            return result;
        }

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

            Result = BuildResult(frente, cantidad);
            DialogResult = true;
        }
    }
}
