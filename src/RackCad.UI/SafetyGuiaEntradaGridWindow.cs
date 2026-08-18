using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;

namespace RackCad.UI
{
    /// <summary>Chooses the dynamic-rack frente/level cells that receive the mirrored entrance-guide pair. The grid is
    /// the shared <see cref="SelectionMatrix"/> control (I-22): column = frente, row = level, with absent cells for the
    /// jagged fronts. The captions (F1.., Nivel n), the high-to-low order, the tolerant loading of persisted off-cells
    /// and the exact off-cell set the dialog returns are unchanged. I-34 adds the shared
    /// <see cref="SelectionMatrixBulkBar"/> with the FRENTE axis.</summary>
    public sealed class SafetyGuiaEntradaGridWindow : Window
    {
        private readonly SelectionMatrixModel model;
        private readonly SelectionMatrixBulkEditor bulkEditor;
        private readonly SelectionMatrixBulkBar bulkBar;
        private readonly IReadOnlyList<int> levelsPerFront;

        /// <summary>The off-cells this dialog was opened with; the ones on absent (blank-front) columns are DORMANT
        /// and must survive the round trip untouched (I-33).</summary>
        private readonly IReadOnlyList<SelectiveGridCell> storedOffCells;

        public IReadOnlyList<SelectiveGridCell> Result { get; private set; } = new List<SelectiveGridCell>();

        /// <param name="allowBlankColumns">
        /// I-33, opt-in: honour a supplied count of ZERO instead of flooring it to one, so a front EN BLANCO renders as
        /// a column with no cells. Default false keeps the historical flooring for every existing caller.
        /// </param>
        public SafetyGuiaEntradaGridWindow(
            string elementLabel,
            IReadOnlyList<int> levelsPerFront,
            IEnumerable<SelectiveGridCell> offCells,
            bool allowBlankColumns = false)
        {
            var levels = (levelsPerFront ?? Array.Empty<int>())
                .Select(count => allowBlankColumns ? Math.Max(0, count) : Math.Max(1, count))
                .ToList();
            if (levels.Count == 0)
            {
                levels = new List<int> { 1 };
            }

            this.levelsPerFront = levels;
            this.storedOffCells = (offCells ?? Enumerable.Empty<SelectiveGridCell>())
                .Where(cell => cell != null)
                .ToList();

            var maxLevels = levels.Max();
            model = SelectionMatrixModel.WithJaggedColumns(
                levels,
                (offCells ?? Enumerable.Empty<SelectiveGridCell>())
                    .Where(cell => cell != null)
                    .Select(cell => new SelectionMatrixCell(cell.Frente, cell.Level)));

            Title = string.IsNullOrWhiteSpace(elementLabel) ? "Guía de entrada" : elementLabel;
            Width = Math.Max(470, Math.Min(1000, 220 + levels.Count * 54));
            // +36: the shared "Aplicar a:" row of I-34 sits under the grid.
            Height = Math.Min(716, 281 + maxLevels * 30);
            MinWidth = 450;
            MinHeight = 336;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            // I-39D: el chrome del arquetipo C en una sola fuente. Sustituye las cuatro sentencias que las
            // diez ventanas repetian a mano; los valores son los MISMOS, asi que no cambia un pixel.
            DialogWindowChrome.Apply(this);

            var root = new DockPanel { Margin = new Thickness(14) };
            var intro = new TextBlock
            {
                Text = "Marca los niveles de cada frente que llevan guía de entrada (todos por defecto). Cada celda "
                     + "coloca una pareja espejeada, 8\" arriba del larguero IN/OUT de entrada; LONGITUD se resuelve "
                     + "automáticamente con el tramo de cabecera correspondiente.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DockPanel.SetDock(intro, Dock.Top);
            root.Children.Add(intro);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var all = Button("Todos", false);
            all.Click += (_, __) => model.SetAll(true);
            var none = Button("Ninguno", false);
            none.Click += (_, __) => model.SetAll(false);
            var ok = Button("Aceptar", true);
            ok.Click += (_, __) => Accept();
            var cancel = Button("Cancelar", false);
            cancel.IsCancel = true;
            buttons.Children.Add(all);
            buttons.Children.Add(none);
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            DockPanel.SetDock(buttons, Dock.Bottom);
            root.Children.Add(buttons);

            var matrix = new SelectionMatrix
            {
                Model = model,
                InvertRows = true, // level 0 at the bottom, highest on top (as the hand-built grid drew it)
                ColumnHeaders = Enumerable.Range(0, levels.Count)
                    .Select(front => "F" + (front + 1).ToString(CultureInfo.InvariantCulture)).ToArray(),
                RowHeaders = Enumerable.Range(0, maxLevels)
                    .Select(level => "Nivel " + (level + 1).ToString(CultureInfo.InvariantCulture)).ToArray()
            };

            // I-34 — the shared bulk-edit row, declared with the FRENTE axis.
            bulkEditor = new SelectionMatrixBulkEditor(model, SelectionMatrixScopeLabels.ByFrente);
            bulkBar = new SelectionMatrixBulkBar(bulkEditor);
            bulkBar.Attach(matrix);
            DockPanel.SetDock(bulkBar, Dock.Bottom);
            root.Children.Add(bulkBar);

            root.Children.Add(new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = matrix
            });
            Content = root;
        }

        private Button Button(string content, bool primary)
            => new Button
            {
                Content = content,
                Style = TryFindResource(primary ? "PrimaryButtonStyle" : "SecondaryButtonStyle") as Style,
                Padding = new Thickness(primary ? 16 : 10, 3, primary ? 16 : 10, 3),
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = primary
            };

        /// <summary>The working matrix state — a test seam (I-22, InternalsVisibleTo).</summary>
        internal SelectionMatrixModel Model => model;

        /// <summary>The shared bulk-edit row and its editor — test seams (I-34).</summary>
        internal SelectionMatrixBulkBar BulkBar => bulkBar;

        internal SelectionMatrixBulkEditor BulkEditor => bulkEditor;

        /// <summary>The off-cells the dialog would return for the current state, without needing ShowDialog.</summary>
        internal IReadOnlyList<SelectiveGridCell> CurrentOffCells()
            => model.UnselectedCells()
                .Select(cell => new SelectiveGridCell { Frente = cell.Column, Level = cell.Row })
                .ToList();

        /// <summary>What the dialog PERSISTS: the cells it shows as OFF plus the DORMANT ones stored on columns it
        /// rendered as absent (a front en blanco, I-33), through the shared <see cref="SafetyDormantCells"/> rule.</summary>
        internal IReadOnlyList<SelectiveGridCell> PersistedOffCells()
            => SafetyDormantCells.Merge(CurrentOffCells(), storedOffCells, levelsPerFront);

        private void Accept()
        {
            Result = PersistedOffCells();
            DialogResult = true;
        }
    }
}
