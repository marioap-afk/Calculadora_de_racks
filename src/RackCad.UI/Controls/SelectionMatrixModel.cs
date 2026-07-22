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
        private readonly HashSet<SelectionMatrixCell> absent;

        /// <summary>Creates a <paramref name="columns"/> × <paramref name="rows"/> rectangular grid.</summary>
        /// <param name="initialSelected">The starting state of every cell (safety grids start all-on).</param>
        public SelectionMatrixModel(int columns, int rows, bool initialSelected = true)
            : this(columns, rows, null, initialSelected)
        {
        }

        private SelectionMatrixModel(int columns, int rows, IEnumerable<SelectionMatrixCell> absentCells, bool initialSelected)
        {
            if (columns < 0) throw new ArgumentOutOfRangeException(nameof(columns));
            if (rows < 0) throw new ArgumentOutOfRangeException(nameof(rows));

            Columns = columns;
            Rows = rows;
            selected = new bool[columns, rows];
            absent = new HashSet<SelectionMatrixCell>();
            if (absentCells != null)
            {
                foreach (var cell in absentCells)
                {
                    if (cell.Column >= 0 && cell.Column < columns && cell.Row >= 0 && cell.Row < rows)
                    {
                        absent.Add(cell);
                    }
                }
            }

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

        /// <summary>The number of PRESENT (selectable) cells: the full rectangle minus the absent cells of a jagged
        /// grid. Equal to <see cref="Columns"/> × <see cref="Rows"/> for a rectangular grid (no absent cells).</summary>
        public int CellCount => Columns * Rows - absent.Count;

        /// <summary>The number of absent (non-existent) cells; 0 for a rectangular grid.</summary>
        public int AbsentCount => absent.Count;

        /// <summary>True when (column, row) does not exist in a jagged grid (a column shorter than the tallest): it is
        /// never drawn, never selectable, and excluded from the on/off queries. Always false for a rectangular grid.</summary>
        public bool IsAbsent(int column, int row)
            => absent.Count != 0 && absent.Contains(new SelectionMatrixCell(column, row));

        /// <summary>True when the grid is jagged (has at least one absent cell).</summary>
        public bool HasAbsentCells => absent.Count != 0;

        public bool this[int column, int row]
        {
            get => IsSelected(column, row);
            set => SetSelected(column, row, value);
        }

        public bool IsSelected(int column, int row)
        {
            EnsureInRange(column, row);
            return !IsAbsent(column, row) && selected[column, row]; // an absent cell is not selectable -> not selected
        }

        public void SetSelected(int column, int row, bool value)
        {
            EnsureInRange(column, row);
            if (IsAbsent(column, row) || selected[column, row] == value)
            {
                return;
            }

            selected[column, row] = value;
            CellChanged?.Invoke(this, new SelectionMatrixCellChangedEventArgs(new SelectionMatrixCell(column, row), value));
        }

        public bool Toggle(int column, int row)
        {
            EnsureInRange(column, row);
            if (IsAbsent(column, row))
            {
                return false; // not selectable: no toggle, no CellChanged, no phantom change reported
            }

            var next = !selected[column, row];
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

        /// <summary>Builds a JAGGED grid from a per-column row count: cells at (column, row) with
        /// row &gt;= <paramref name="rowsPerColumn"/>[column] are ABSENT — the safety grids where each frente/post has
        /// its own level count, so the taller columns leave empty slots at the top. The listed
        /// <paramref name="unselected"/> cells start OFF; coordinates outside the grid (or absent) are ignored, matching
        /// the tolerant legacy handling of the safety grids.</summary>
        public static SelectionMatrixModel WithJaggedColumns(
            IReadOnlyList<int> rowsPerColumn, IEnumerable<SelectionMatrixCell> unselected, bool initialSelected = true)
        {
            var columns = rowsPerColumn?.Count ?? 0;
            var rows = 0;
            for (var c = 0; c < columns; c++)
            {
                rows = Math.Max(rows, Math.Max(0, rowsPerColumn[c]));
            }

            var absentCells = new List<SelectionMatrixCell>();
            for (var c = 0; c < columns; c++)
            {
                for (var r = Math.Max(0, rowsPerColumn[c]); r < rows; r++)
                {
                    absentCells.Add(new SelectionMatrixCell(c, r));
                }
            }

            var model = new SelectionMatrixModel(columns, rows, absentCells, initialSelected);
            if (unselected != null)
            {
                foreach (var cell in unselected)
                {
                    if (cell.Column >= 0 && cell.Column < columns && cell.Row >= 0 && cell.Row < rows
                        && !model.IsAbsent(cell.Column, cell.Row))
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
                    if (!IsAbsent(c, r) && selected[c, r] == state)
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
                    if (!IsAbsent(c, r) && selected[c, r] == state)
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
