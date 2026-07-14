using System;
using System.Collections.Generic;

namespace RackCad.Application.Layout
{
    /// <summary>The computed fill: the orientation chosen, the KEPT cells (min-corner space: each rack occupies
    /// [cell, cell + footprint]), the oriented footprint, and how many candidate cells were dropped and why.</summary>
    public sealed class WarehouseAutoFillResult
    {
        public WarehouseAutoFillResult(
            RackOrientation orientation, IReadOnlyList<WarehouseCell> cells,
            double footprintX, double footprintY,
            int omittedOutside, int omittedByObstacle, int rowsTried, int colsTried)
        {
            Orientation = orientation;
            Cells = cells;
            FootprintX = footprintX;
            FootprintY = footprintY;
            OmittedOutside = omittedOutside;
            OmittedByObstacle = omittedByObstacle;
            RowsTried = rowsTried;
            ColsTried = colsTried;
        }

        public RackOrientation Orientation { get; }

        /// <summary>The cells that FIT (grid labels keep their row/column identity, so omitted cells leave gaps).</summary>
        public IReadOnlyList<WarehouseCell> Cells { get; }

        /// <summary>Oriented footprint: X along the rows axis, Y along the columns axis.</summary>
        public double FootprintX { get; }
        public double FootprintY { get; }

        /// <summary>Candidate cells dropped because they poke outside the envelope/polygon.</summary>
        public int OmittedOutside { get; }

        /// <summary>Candidate cells dropped because they hit a column/obstacle (+ its clearance).</summary>
        public int OmittedByObstacle { get; }

        public int RowsTried { get; }
        public int ColsTried { get; }
    }

    /// <summary>
    /// Fills a warehouse site with as many racks as fit: seeds a regular grid over the site's bounding box (anchored at
    /// the bbox min corner + wall clearance), drops every cell that pokes outside the envelope/polygon or hits an
    /// obstacle, and — when allowed — tries both orientations and keeps the one that fits MORE racks. Deterministic and
    /// pure: this is the first working "brain" of the layout optimizer (count-maximizing; benefit/cost scoring can
    /// replace the count later). The caller places the returned cells (RACKRELLENAR).
    /// </summary>
    public static class WarehouseAutoFill
    {
        private const double Eps = 1e-6;
        private const int MaxCells = 5000;

        public static WarehouseAutoFillResult Fill(
            WarehouseSite site, double rackDepth, double rackWidth,
            double aisleRows, double aisleCols,
            RowPairing pairing = RowPairing.Single, double backGap = 0.0,
            bool tryRotated = true)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));
            if (rackDepth <= 0.0) throw new ArgumentOutOfRangeException(nameof(rackDepth), "El fondo del rack debe ser > 0.");
            if (rackWidth <= 0.0) throw new ArgumentOutOfRangeException(nameof(rackWidth), "El ancho del rack debe ser > 0.");

            var best = FillOriented(site, rackDepth, rackWidth, RackOrientation.AlongDepth, aisleRows, aisleCols, pairing, backGap);

            // The rotated candidate SWAPS which world axis each aisle lives on: the pick aisle (aisleRows) always
            // separates rack FACES — the depth axis — which after the 90° turn runs along Y; the end gap (aisleCols)
            // moves to X. Without the swap a rotated pack with NO pick aisle could win the count. Back-to-back also
            // pairs along the depth axis, which the planner only supports on X, so with pairing on the rotated try is
            // SKIPPED rather than mis-paired (pre-rotate the seed to fill the other way).
            if (tryRotated && pairing == RowPairing.Single)
            {
                var rotated = FillOriented(site, rackDepth, rackWidth, RackOrientation.Rotated, aisleCols, aisleRows, pairing, backGap);
                if (rotated.Cells.Count > best.Cells.Count)
                {
                    best = rotated; // strictly more racks wins; ties keep the natural orientation
                }
            }

            return best;
        }

        /// <summary>One orientation candidate. <paramref name="aisleX"/>/<paramref name="aisleY"/> are WORLD-axis gaps —
        /// the caller maps the user's pick/end aisles onto them per orientation (see <see cref="Fill"/>).</summary>
        private static WarehouseAutoFillResult FillOriented(
            WarehouseSite site, double rackDepth, double rackWidth, RackOrientation orientation,
            double aisleX, double aisleY, RowPairing pairing, double backGap)
        {
            var footprintX = orientation == RackOrientation.AlongDepth ? rackDepth : rackWidth;
            var footprintY = orientation == RackOrientation.AlongDepth ? rackWidth : rackDepth;

            // Candidate grid anchored at the (clearance-inset) bbox min corner; cells ARE rack min corners.
            var anchorX = site.OriginX + site.WallClearance;
            var anchorY = site.OriginY + site.WallClearance;
            var spanX = site.Width - 2.0 * site.WallClearance;
            var spanY = site.Depth - 2.0 * site.WallClearance;

            var rows = CountFitting(spanX, footprintX, aisleX, pairing, backGap);
            var cols = CountFitting(spanY, footprintY, aisleY, RowPairing.Single, 0.0);
            if (rows == 0 || cols == 0)
            {
                return Empty(orientation, footprintX, footprintY);
            }

            if ((long)rows * cols > MaxCells)
            {
                throw new InvalidOperationException(
                    "El relleno calculado excede " + MaxCells + " racks; revisa las unidades del contorno o los pasillos.");
            }

            var plan = WarehouseGridPlanner.Plan(
                footprintX, footprintY, rows, cols, aisleX, aisleY,
                anchorX, anchorY, pairing, backGap, orientation);

            var kept = new List<WarehouseCell>();
            var outside = 0;
            var blocked = 0;
            foreach (var cell in plan.Cells)
            {
                var verdict = WarehouseFitChecker.ClassifyRect(cell.X, cell.Y, cell.X + footprintX, cell.Y + footprintY, site);
                if (verdict == null)
                {
                    kept.Add(cell);
                }
                else if (verdict == FitViolationKind.OutOfBounds)
                {
                    outside++;
                }
                else
                {
                    blocked++;
                }
            }

            return new WarehouseAutoFillResult(orientation, kept, footprintX, footprintY, outside, blocked, rows, cols);
        }

        /// <summary>How many rows/columns fit a span, using the SAME spacing rule as the planner (RowOffset).</summary>
        private static int CountFitting(double span, double footprint, double aisle, RowPairing pairing, double backGap)
        {
            if (footprint > span + Eps)
            {
                return 0;
            }

            var count = 0;
            while (count < MaxCells)
            {
                var offset = WarehouseGridPlanner.RowOffset(count, footprint, aisle, pairing, backGap);
                if (offset + footprint > span + Eps)
                {
                    break;
                }

                count++;
            }

            return count;
        }

        private static WarehouseAutoFillResult Empty(RackOrientation orientation, double footprintX, double footprintY)
            => new WarehouseAutoFillResult(orientation, Array.Empty<WarehouseCell>(), footprintX, footprintY, 0, 0, 0, 0);
    }
}
