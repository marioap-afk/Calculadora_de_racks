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

    /// <summary>How rows are spaced along the depth axis: each on its own pick aisle, or paired back-to-back (two rows
    /// sharing a flue gap, with the pick aisle only on the OUTER side of each pair — the denser warehouse pattern).</summary>
    public enum RowPairing
    {
        Single,
        BackToBack
    }

    /// <summary>Which way each rack faces: its depth along the rows axis (the natural arrangement) or turned 90° so its
    /// depth runs along the columns axis. Carried on the plan so the drawer can rotate the blocks. The math is
    /// axis-agnostic — orientation just maps the rack's (depth, width) to the (rows, columns) extents.</summary>
    public enum RackOrientation
    {
        AlongDepth,
        Rotated
    }

    /// <summary>The computed grid: every cell (seed included) plus the overall footprint the grid occupies.</summary>
    public sealed class WarehouseGridPlan
    {
        public WarehouseGridPlan(IReadOnlyList<WarehouseCell> cells, double totalX, double totalY,
            RackOrientation orientation = RackOrientation.AlongDepth)
        {
            Cells = cells;
            TotalX = totalX;
            TotalY = totalY;
            Orientation = orientation;
        }

        public IReadOnlyList<WarehouseCell> Cells { get; }

        /// <summary>Total span along X (the rows/depth axis), including the last rack's own footprint.</summary>
        public double TotalX { get; }

        /// <summary>Total span along Y (the columns/width axis).</summary>
        public double TotalY { get; }

        /// <summary>How the racks are turned (the drawer rotates 90° when <see cref="RackOrientation.Rotated"/>).</summary>
        public RackOrientation Orientation { get; }
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
            double seedX = 0.0, double seedY = 0.0,
            RowPairing pairing = RowPairing.Single, double backGap = 0.0,
            RackOrientation orientation = RackOrientation.AlongDepth)
        {
            if (rows < 1) throw new ArgumentOutOfRangeException(nameof(rows), "Debe haber al menos 1 fila.");
            if (cols < 1) throw new ArgumentOutOfRangeException(nameof(cols), "Debe haber al menos 1 columna.");
            if (footprintX <= 0.0) throw new ArgumentOutOfRangeException(nameof(footprintX), "El fondo del rack debe ser > 0.");
            if (footprintY <= 0.0) throw new ArgumentOutOfRangeException(nameof(footprintY), "El ancho del rack debe ser > 0.");
            if (aisleX < 0.0) throw new ArgumentOutOfRangeException(nameof(aisleX), "El pasillo no puede ser negativo.");
            if (aisleY < 0.0) throw new ArgumentOutOfRangeException(nameof(aisleY), "El pasillo no puede ser negativo.");
            if (backGap < 0.0) throw new ArgumentOutOfRangeException(nameof(backGap), "El hueco entre espaldas no puede ser negativo.");

            var pitchY = footprintY + aisleY; // columns: always a uniform pitch

            var cells = new List<WarehouseCell>(rows * cols);
            var maxX = seedX;
            var maxY = seedY;
            for (var r = 0; r < rows; r++)
            {
                var x = seedX + RowOffset(r, footprintX, aisleX, pairing, backGap);
                for (var c = 0; c < cols; c++)
                {
                    var y = seedY + c * pitchY;
                    cells.Add(new WarehouseCell(r, c, x, y, Label(r, c)));
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            // Total span = furthest origin (from the seed) + one footprint (no trailing aisle). Derived from the cells
            // so it is correct for both the uniform Single pitch and the paired BackToBack one.
            var totalX = (maxX - seedX) + footprintX;
            var totalY = (maxY - seedY) + footprintY;
            return new WarehouseGridPlan(cells, totalX, totalY, orientation);
        }

        /// <summary>The X offset (from the seed) of row <paramref name="r"/> along the depth axis: a uniform pitch when
        /// Single; paired when BackToBack — the two rows of a pair share a <paramref name="backGap"/> flue and the pick
        /// aisle sits only BETWEEN pairs (an odd last row is a lone rack). Pairing is along the rows/depth axis, so it is
        /// a true back-to-back only in the AlongDepth orientation.</summary>
        private static double RowOffset(int r, double footprintX, double aisleX, RowPairing pairing, double backGap)
        {
            if (pairing == RowPairing.BackToBack)
            {
                var pair = r / 2;
                var within = r % 2;
                return pair * (2.0 * footprintX + backGap + aisleX) + within * (footprintX + backGap);
            }

            return r * (footprintX + aisleX);
        }

        /// <summary>
        /// Orientation-aware convenience: lay out racks of size <paramref name="rackDepth"/> × <paramref name="rackWidth"/>
        /// in the chosen <paramref name="orientation"/>. AlongDepth stacks rows across the rack's DEPTH (the natural
        /// warehouse arrangement); Rotated turns each rack 90° so its DEPTH runs along the columns axis. Maps the rack
        /// dimensions onto the (rows, columns) extents and records the orientation on the plan for the drawer.
        /// </summary>
        public static WarehouseGridPlan PlanForRack(
            double rackDepth, double rackWidth, RackOrientation orientation,
            int rows, int cols, double aisleBetweenRows, double aisleBetweenCols,
            RowPairing pairing = RowPairing.Single, double backGap = 0.0,
            double seedX = 0.0, double seedY = 0.0)
        {
            var footprintX = orientation == RackOrientation.AlongDepth ? rackDepth : rackWidth;
            var footprintY = orientation == RackOrientation.AlongDepth ? rackWidth : rackDepth;
            return Plan(footprintX, footprintY, rows, cols, aisleBetweenRows, aisleBetweenCols, seedX, seedY, pairing, backGap, orientation);
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
