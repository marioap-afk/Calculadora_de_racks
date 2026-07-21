using System;
using System.Collections.Generic;

namespace RackCad.UI.Controls
{
    /// <summary>A (column, row) coordinate in a <see cref="SelectionMatrixModel"/>. The adopter decides the meaning
    /// of the axes — e.g. column = frente/poste, row = level — so the model stays reusable across systems.</summary>
    public readonly struct SelectionMatrixCell : IEquatable<SelectionMatrixCell>
    {
        public SelectionMatrixCell(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }

        public int Row { get; }

        public bool Equals(SelectionMatrixCell other) => Column == other.Column && Row == other.Row;

        public override bool Equals(object obj) => obj is SelectionMatrixCell other && Equals(other);

        public override int GetHashCode() => unchecked((Column * 397) ^ Row);

        public override string ToString() => $"(col {Column}, row {Row})";
    }

    /// <summary>Payload for <see cref="SelectionMatrixModel.CellChanged"/>: the cell and its new selection state.</summary>
    public sealed class SelectionMatrixCellChangedEventArgs : EventArgs
    {
        public SelectionMatrixCellChangedEventArgs(SelectionMatrixCell cell, bool isSelected)
        {
            Cell = cell;
            IsSelected = isSelected;
        }

        public SelectionMatrixCell Cell { get; }

        public bool IsSelected { get; }
    }

    /// <summary>
    /// The reusable state behind a <see cref="SelectionMatrix"/>: a rectangular grid of on/off cells, defaulting to
    /// all-on, that reports OFF cells the way the safety grids persist them (only what the user disabled). This
    /// concentrates the near-identical <c>CheckBox[][]</c> bookkeeping that the tope/parrilla/desviador/guía grids
    /// each re-implement today. Pure: no WPF — the control observes it and updates only the cell that changed
    /// (granular <see cref="CellChanged"/>) so a click never rebuilds the whole grid.
    /// </summary>
    public sealed class SelectionMatrixModel
    {
        private readonly bool[,] selected;

        /// <summary>Creates a <paramref name="columns"/> × <paramref name="rows"/> grid.</summary>
        /// <param name="initialSelected">The starting state of every cell (safety grids start all-on).</param>
        public SelectionMatrixModel(int columns, int rows, bool initialSelected = true)
        {
            if (columns < 0) throw new ArgumentOutOfRangeException(nameof(columns));
            if (rows < 0) throw new ArgumentOutOfRangeException(nameof(rows));

            Columns = columns;
            Rows = rows;
            selected = new bool[columns, rows];
            if (initialSelected)
            {
                SetAllInternal(true);
            }
        }

        /// <summary>Raised for a single cell whose state changed (one click). Not raised by <see cref="SetAll"/>.</summary>
        public event EventHandler<SelectionMatrixCellChangedEventArgs> CellChanged;

        /// <summary>Raised after a bulk change (<see cref="SetAll"/>): observers repaint every cell once.</summary>
        public event EventHandler BulkChanged;

        public int Columns { get; }

        public int Rows { get; }

        public int CellCount => Columns * Rows;

        public bool this[int column, int row]
        {
            get => IsSelected(column, row);
            set => SetSelected(column, row, value);
        }

        public bool IsSelected(int column, int row)
        {
            EnsureInRange(column, row);
            return selected[column, row];
        }

        public void SetSelected(int column, int row, bool value)
        {
            EnsureInRange(column, row);
            if (selected[column, row] == value)
            {
                return;
            }

            selected[column, row] = value;
            CellChanged?.Invoke(this, new SelectionMatrixCellChangedEventArgs(new SelectionMatrixCell(column, row), value));
        }

        public bool Toggle(int column, int row)
        {
            var next = !IsSelected(column, row);
            SetSelected(column, row, next);
            return next;
        }

        /// <summary>Sets every cell to <paramref name="value"/> (the "Todos"/"Ninguno" buttons) and raises
        /// <see cref="BulkChanged"/> once.</summary>
        public void SetAll(bool value)
        {
            SetAllInternal(value);
            BulkChanged?.Invoke(this, EventArgs.Empty);
        }

        public int SelectedCount => CountSelected(true);

        public int UnselectedCount => CountSelected(false);

        /// <summary>The cells the user turned OFF, in column-major then row order — the set the safety grids persist.</summary>
        public IReadOnlyList<SelectionMatrixCell> UnselectedCells() => Collect(false);

        /// <summary>The cells that remain ON.</summary>
        public IReadOnlyList<SelectionMatrixCell> SelectedCells() => Collect(true);

        /// <summary>Builds a grid whose only OFF cells are <paramref name="unselected"/> (loading persisted state).
        /// Coordinates outside the grid are ignored, matching the tolerant legacy handling of the safety grids.</summary>
        public static SelectionMatrixModel WithUnselected(int columns, int rows, IEnumerable<SelectionMatrixCell> unselected)
        {
            var model = new SelectionMatrixModel(columns, rows, initialSelected: true);
            if (unselected != null)
            {
                foreach (var cell in unselected)
                {
                    if (cell.Column >= 0 && cell.Column < columns && cell.Row >= 0 && cell.Row < rows)
                    {
                        model.selected[cell.Column, cell.Row] = false;
                    }
                }
            }

            return model;
        }

        private void SetAllInternal(bool value)
        {
            for (var c = 0; c < Columns; c++)
            {
                for (var r = 0; r < Rows; r++)
                {
                    selected[c, r] = value;
                }
            }
        }

        private int CountSelected(bool state)
        {
            var count = 0;
            for (var c = 0; c < Columns; c++)
            {
                for (var r = 0; r < Rows; r++)
                {
                    if (selected[c, r] == state)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private IReadOnlyList<SelectionMatrixCell> Collect(bool state)
        {
            var cells = new List<SelectionMatrixCell>();
            for (var c = 0; c < Columns; c++)
            {
                for (var r = 0; r < Rows; r++)
                {
                    if (selected[c, r] == state)
                    {
                        cells.Add(new SelectionMatrixCell(c, r));
                    }
                }
            }

            return cells;
        }

        private void EnsureInRange(int column, int row)
        {
            if (column < 0 || column >= Columns) throw new ArgumentOutOfRangeException(nameof(column));
            if (row < 0 || row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
        }
    }
}
