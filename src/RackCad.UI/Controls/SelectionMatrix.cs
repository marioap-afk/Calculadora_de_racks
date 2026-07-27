using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// A grid of check-box cells backed by a <see cref="SelectionMatrixModel"/>: the one control behind the
    /// tope/parrilla/desviador/guía safety grids, which each rebuild a <c>CheckBox[][]</c> by hand today. Column and
    /// row headers are optional (default to 1..N); <see cref="InvertRows"/> draws the last model row on top so a
    /// level axis reads high-to-low like the safety grids. A single click updates only that cell — the model's
    /// granular <see cref="SelectionMatrixModel.CellChanged"/> event repaints one check box, never the whole grid
    /// (the performance invariant in AGENTS §6). Bulk changes (Todos/Ninguno) repaint once.
    /// </summary>
    public class SelectionMatrix : Grid
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model), typeof(SelectionMatrixModel), typeof(SelectionMatrix),
            new PropertyMetadata(null, OnModelChanged));

        private CheckBox[,] cells;
        private TextBlock[,] adornments; // parallel to cells; entries stay null when there is no adornment provider
        private Func<SelectionMatrixCell, string> cellAdornment;
        private bool suppress;
        private IReadOnlyList<string> columnHeaders;
        private IReadOnlyList<string> rowHeaders;
        private bool invertRows;
        private bool showHeaders = true;

        /// <summary>The grid state. Setting it rewires the cells to the new model.</summary>
        public SelectionMatrixModel Model
        {
            get => (SelectionMatrixModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        /// <summary>Column captions (index 0 = leftmost). Null → "1".."N".</summary>
        public IReadOnlyList<string> ColumnHeaders
        {
            get => columnHeaders;
            set { columnHeaders = value; Rebuild(); }
        }

        /// <summary>Row captions (index 0 = model row 0). Null → "1".."N".</summary>
        public IReadOnlyList<string> RowHeaders
        {
            get => rowHeaders;
            set { rowHeaders = value; Rebuild(); }
        }

        /// <summary>When true the highest model-row index is drawn at the top (level axis high-to-low).</summary>
        public bool InvertRows
        {
            get => invertRows;
            set { invertRows = value; Rebuild(); }
        }

        /// <summary>Whether the header row/column are drawn. Default true.</summary>
        public bool ShowHeaders
        {
            get => showHeaders;
            set { showHeaders = value; Rebuild(); }
        }

        /// <summary>
        /// Raised when the USER clicks a cell (never for a programmatic change). It is what makes a cell the PRIMARY
        /// one for a scoped bulk edit — "the last valid cell interacted with" (I-34). Only present cells have a check
        /// box at all, so the reported cell is always a valid anchor.
        /// </summary>
        public event EventHandler<SelectionMatrixCellChangedEventArgs> CellInteracted;

        /// <summary>
        /// OPTIONAL per-cell adornment (I-34 addendum): a short caption the ADOPTER supplies, drawn next to each
        /// present cell's check box. It exists so a grid that carries more than a boolean — the parrilla's live deck
        /// count — can adopt this control without losing that information; the Owner's condition for including it was
        /// precisely that it may not be reduced to a bare check box.
        /// <para>
        /// Deliberately NEUTRAL: the control renders a string and knows nothing about what it means, about safety
        /// families or about any rack system. It is also pure opt-in — leave it null and a cell is built exactly as
        /// before (the check box goes straight into the grid, with no wrapper), so the grids that do not use it are
        /// untouched.
        /// </para>
        /// </summary>
        public Func<SelectionMatrixCell, string> CellAdornment
        {
            get => cellAdornment;
            set { cellAdornment = value; Rebuild(); }
        }

        /// <summary>
        /// Re-reads <see cref="CellAdornment"/> for every present cell and rewrites the captions IN PLACE. No rebuild,
        /// no new visuals, no scroll or size disturbance — the same invariant a single click and a scoped bulk edit
        /// already honour (AGENTS §6). The adopter calls it ONCE per operation, never per cell.
        /// </summary>
        public void RefreshAdornments()
        {
            var model = Model;
            if (adornments == null || model == null || cellAdornment == null)
            {
                return;
            }

            for (var c = 0; c < model.Columns; c++)
            {
                for (var r = 0; r < model.Rows; r++)
                {
                    var block = adornments[c, r];
                    if (block != null)
                    {
                        block.Text = cellAdornment(new SelectionMatrixCell(c, r)) ?? string.Empty;
                    }
                }
            }
        }

        /// <summary>The adornment block of a cell, or null when there is none (test seam).</summary>
        internal TextBlock AdornmentFor(int column, int row)
        {
            if (adornments == null || column < 0 || row < 0
                || column >= adornments.GetLength(0) || row >= adornments.GetLength(1))
            {
                return null;
            }

            return adornments[column, row];
        }

        /// <summary>The check box for a model cell, or null when out of range (for tests/adopters that poke a cell).</summary>
        public CheckBox CellFor(int column, int row)
        {
            if (cells == null || column < 0 || row < 0 || column >= cells.GetLength(0) || row >= cells.GetLength(1))
            {
                return null;
            }

            return cells[column, row];
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var matrix = (SelectionMatrix)d;
            if (e.OldValue is SelectionMatrixModel oldModel)
            {
                oldModel.CellChanged -= matrix.OnModelCellChanged;
                oldModel.BulkChanged -= matrix.OnModelBulkChanged;
                oldModel.ScopeApplied -= matrix.OnModelScopeApplied;
            }

            if (e.NewValue is SelectionMatrixModel newModel)
            {
                newModel.CellChanged += matrix.OnModelCellChanged;
                newModel.BulkChanged += matrix.OnModelBulkChanged;
                newModel.ScopeApplied += matrix.OnModelScopeApplied;
            }

            matrix.Rebuild();
        }

        private void Rebuild()
        {
            Children.Clear();
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
            cells = null;
            adornments = null;

            var model = Model;
            if (model == null || model.Columns == 0 || model.Rows == 0)
            {
                return;
            }

            var headerRows = showHeaders ? 1 : 0;
            var headerCols = showHeaders ? 1 : 0;

            for (var i = 0; i < headerCols + model.Columns; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            for (var i = 0; i < headerRows + model.Rows; i++)
            {
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            if (showHeaders)
            {
                for (var c = 0; c < model.Columns; c++)
                {
                    var header = HeaderText(BuildText(columnHeaders, c));
                    SetRow(header, 0);
                    SetColumn(header, c + headerCols);
                    Children.Add(header);
                }
            }

            cells = new CheckBox[model.Columns, model.Rows];
            adornments = new TextBlock[model.Columns, model.Rows];
            for (var r = 0; r < model.Rows; r++)
            {
                var visualRow = invertRows ? (model.Rows - 1 - r) : r;
                var gridRow = visualRow + headerRows;

                if (showHeaders)
                {
                    var header = HeaderText(BuildText(rowHeaders, r));
                    SetRow(header, gridRow);
                    SetColumn(header, 0);
                    Children.Add(header);
                }

                for (var c = 0; c < model.Columns; c++)
                {
                    var column = c;
                    var row = r;
                    if (model.IsAbsent(column, row))
                    {
                        continue; // a jagged column's empty top slot: draw no check box (cells[.,.] stays null)
                    }

                    var checkbox = new CheckBox
                    {
                        IsChecked = model[column, row],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(6, 3, 6, 3),
                    };
                    checkbox.Click += (_, __) =>
                    {
                        if (suppress)
                        {
                            return;
                        }

                        var isSelected = checkbox.IsChecked == true;
                        model.SetSelected(column, row, isSelected);

                        // Raised even when the click did not change the model (it always does today), because what an
                        // adopter needs from this is WHERE the user is working, not whether the value moved.
                        CellInteracted?.Invoke(
                            this,
                            new SelectionMatrixCellChangedEventArgs(new SelectionMatrixCell(column, row), isSelected));
                    };

                    cells[column, row] = checkbox;
                    UIElement placed = checkbox;

                    if (cellAdornment != null)
                    {
                        // Opt-in only: with no provider the check box goes straight into the grid, exactly as before.
                        var caption = new TextBlock
                        {
                            Text = cellAdornment(new SelectionMatrixCell(column, row)) ?? string.Empty,
                            FontSize = 10.5,
                            MinWidth = 14,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(3, 0, 0, 0)
                        };
                        adornments[column, row] = caption;

                        var pair = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        checkbox.Margin = new Thickness(6, 3, 0, 3); // the caption supplies the right-hand gap
                        pair.Children.Add(checkbox);
                        pair.Children.Add(caption);
                        placed = pair;
                    }

                    SetRow(placed, gridRow);
                    SetColumn(placed, column + headerCols);
                    Children.Add(placed);
                }
            }
        }

        private void OnModelCellChanged(object sender, SelectionMatrixCellChangedEventArgs e)
        {
            var checkbox = CellFor(e.Cell.Column, e.Cell.Row);
            if (checkbox == null)
            {
                return;
            }

            suppress = true;
            checkbox.IsChecked = e.IsSelected;
            suppress = false;
        }

        /// <summary>A scoped bulk edit (I-34): repaint ONLY the cells the model reported, never the whole grid and
        /// never a rebuild — the same performance invariant a single click already honours (AGENTS §6).</summary>
        private void OnModelScopeApplied(object sender, SelectionMatrixScopeAppliedEventArgs e)
        {
            suppress = true;
            foreach (var cell in e.Cells)
            {
                var checkbox = CellFor(cell.Column, cell.Row);
                if (checkbox != null)
                {
                    checkbox.IsChecked = e.IsSelected;
                }
            }

            suppress = false;
        }

        private void OnModelBulkChanged(object sender, EventArgs e)
        {
            var model = Model;
            if (cells == null || model == null)
            {
                return;
            }

            suppress = true;
            for (var c = 0; c < model.Columns; c++)
            {
                for (var r = 0; r < model.Rows; r++)
                {
                    if (cells[c, r] != null)
                    {
                        cells[c, r].IsChecked = model[c, r];
                    }
                }
            }

            suppress = false;
        }

        private TextBlock HeaderText(string text)
        {
            var block = new TextBlock
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 3, 6, 3),
            };

            if (TryFindResource("FieldLabel") is Style labelStyle)
            {
                block.Style = labelStyle;
            }

            return block;
        }

        private static string BuildText(IReadOnlyList<string> headers, int index)
        {
            if (headers != null && index >= 0 && index < headers.Count && headers[index] != null)
            {
                return headers[index];
            }

            return (index + 1).ToString(System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}
