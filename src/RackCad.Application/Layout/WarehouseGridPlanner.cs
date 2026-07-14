using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RackCad.Application.Layout
{
    /// <summary>One placed rack in a warehouse grid: its row/column indices, the world point where the rack's origin
    /// goes, and its label (e.g. "B3"). Pure data — no AutoCAD types.</summary>
    public readonly struct WarehouseCell
    {
        public WarehouseCell(int row, int col, double x, double y, string label)
        {
            Row = row;
            Col = col;
            X = x;
            Y = y;
            Label = label;
        }

        public int Row { get; }
        public int Col { get; }
        public double X { get; }
        public double Y { get; }
        public string Label { get; }

        /// <summary>True for the seed rack — the one already drawn that the grid is arrayed from. Never re-drawn.</summary>
        public bool IsSeed => Row == 0 && Col == 0;
    }

    /// <summary>The computed grid: every cell (seed included) plus the overall footprint the grid occupies.</summary>
    public sealed class WarehouseGridPlan
    {
        public WarehouseGridPlan(IReadOnlyList<WarehouseCell> cells, double totalX, double totalY)
        {
            Cells = cells;
            TotalX = totalX;
            TotalY = totalY;
        }

        public IReadOnlyList<WarehouseCell> Cells { get; }

        /// <summary>Total span along X (the rows/depth axis), including the last rack's own footprint.</summary>
        public double TotalX { get; }

        /// <summary>Total span along Y (the columns/width axis).</summary>
        public double TotalY { get; }
    }

    /// <summary>
    /// Lays a single rack out into a warehouse grid: <paramref name="rows"/> racks along the X axis (the rack depth /
    /// pick-aisle direction) separated by <paramref name="aisleX"/>, and <paramref name="cols"/> racks along the Y axis
    /// (the rack length / cross-aisle direction) separated by <paramref name="aisleY"/>. Cell (0,0) is the seed rack the
    /// caller already drew; its world origin is (<paramref name="seedX"/>, <paramref name="seedY"/>). Rows are labeled
    /// A, B, C… and columns 1, 2, 3…, so a cell's label is e.g. "B3". Pure geometry — the caller (Plugin) reads the
    /// footprint from the drawn block's extents and materializes each cell as a block reference.
    /// </summary>
    public static class WarehouseGridPlanner
    {
        public static WarehouseGridPlan Plan(
            double footprintX, double footprintY,
            int rows, int cols,
            double aisleX, double aisleY,
            double seedX = 0.0, double seedY = 0.0)
        {
            if (rows < 1) throw new ArgumentOutOfRangeException(nameof(rows), "Debe haber al menos 1 fila.");
            if (cols < 1) throw new ArgumentOutOfRangeException(nameof(cols), "Debe haber al menos 1 columna.");
            if (footprintX <= 0.0) throw new ArgumentOutOfRangeException(nameof(footprintX), "El fondo del rack debe ser > 0.");
            if (footprintY <= 0.0) throw new ArgumentOutOfRangeException(nameof(footprintY), "El ancho del rack debe ser > 0.");
            if (aisleX < 0.0) throw new ArgumentOutOfRangeException(nameof(aisleX), "El pasillo no puede ser negativo.");
            if (aisleY < 0.0) throw new ArgumentOutOfRangeException(nameof(aisleY), "El pasillo no puede ser negativo.");

            var pitchX = footprintX + aisleX; // origin-to-origin distance between consecutive rows
            var pitchY = footprintY + aisleY;

            var cells = new List<WarehouseCell>(rows * cols);
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var x = seedX + r * pitchX;
                    var y = seedY + c * pitchY;
                    cells.Add(new WarehouseCell(r, c, x, y, Label(r, c)));
                }
            }

            // Total span = last origin offset + one footprint (no trailing aisle).
            var totalX = (rows - 1) * pitchX + footprintX;
            var totalY = (cols - 1) * pitchY + footprintY;
            return new WarehouseGridPlan(cells, totalX, totalY);
        }

        /// <summary>Cell label: row as a bijective base-26 letter (A..Z, AA..) + column as a 1-based number, e.g. "B3".</summary>
        public static string Label(int row, int col)
            => RowLetters(row) + (col + 1).ToString(CultureInfo.InvariantCulture);

        /// <summary>Bijective base-26: 0→A, 25→Z, 26→AA, 27→AB… (spreadsheet-column style, no "A0").</summary>
        public static string RowLetters(int row)
        {
            if (row < 0) throw new ArgumentOutOfRangeException(nameof(row));

            var builder = new StringBuilder();
            var n = row + 1; // shift to 1-based for the bijective mapping
            while (n > 0)
            {
                var rem = (n - 1) % 26;
                builder.Insert(0, (char)('A' + rem));
                n = (n - 1) / 26;
            }

            return builder.ToString();
        }
    }
}
