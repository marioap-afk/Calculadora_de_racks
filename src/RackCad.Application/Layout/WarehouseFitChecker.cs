using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Layout
{
    public enum FitViolationKind
    {
        OutOfBounds,
        HitsObstacle,
        AisleTooNarrow
    }

    /// <summary>One reason a layout does not fit its site, with a human-readable message.</summary>
    public sealed class FitViolation
    {
        public FitViolation(FitViolationKind kind, string message)
        {
            Kind = kind;
            Message = message;
        }

        public FitViolationKind Kind { get; }
        public string Message { get; }
    }

    /// <summary>The feasibility verdict: whether the layout fits, and every violation found (empty = fits).</summary>
    public sealed class WarehouseFitResult
    {
        public WarehouseFitResult(IReadOnlyList<FitViolation> violations)
        {
            Violations = violations ?? Array.Empty<FitViolation>();
        }

        public IReadOnlyList<FitViolation> Violations { get; }
        public bool Fits => Violations.Count == 0;
    }

    /// <summary>
    /// Checks whether a laid-out grid of racks physically FITS a warehouse site: every rack stays inside the envelope
    /// (minus the wall clearance), clears every obstacle (plus its clearance), and the aisles meet the site minimum.
    /// Pure rectangle geometry — each rack occupies [cell.X+offsetX, +footprintX] × [cell.Y+offsetY, +footprintY].
    /// <paramref name="offsetX"/>/<paramref name="offsetY"/> is the (constant) gap between a cell's origin and the rack's
    /// min corner — pass 0 when cells already ARE min corners (the pure planner), or the block's origin→bbox-min offset
    /// when the caller seeds the plan with an insertion point (the AutoCAD plugin). Returns the violations (empty = fits).
    /// No AutoCAD, no scoring — this answers "is this candidate legal?", the feasibility gate the future optimizer needs.
    /// Assumes the racks don't overlap EACH OTHER (the planner's invariant); this validates fit against the SITE only.
    /// </summary>
    public static class WarehouseFitChecker
    {
        private const double Eps = 1e-6;

        public static WarehouseFitResult Check(
            WarehouseGridPlan plan, double footprintX, double footprintY, WarehouseSite site,
            double offsetX = 0.0, double offsetY = 0.0)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (site == null) throw new ArgumentNullException(nameof(site));

            var violations = new List<FitViolation>();

            foreach (var cell in plan.Cells)
            {
                var rx0 = cell.X + offsetX;
                var ry0 = cell.Y + offsetY;
                var rx1 = rx0 + footprintX;
                var ry1 = ry0 + footprintY;

                if (IsOutOfBounds(rx0, ry0, rx1, ry1, site))
                {
                    violations.Add(new FitViolation(FitViolationKind.OutOfBounds,
                        "El rack " + cell.Label + " se sale de la envolvente del almacén."));
                }

                foreach (var obstacle in site.Obstacles)
                {
                    if (OverlapsObstacle(rx0, ry0, rx1, ry1, obstacle))
                    {
                        var name = string.IsNullOrWhiteSpace(obstacle.Label) ? "un obstáculo" : "'" + obstacle.Label.Trim() + "'";
                        violations.Add(new FitViolation(FitViolationKind.HitsObstacle,
                            "El rack " + cell.Label + " choca con " + name + "."));
                    }
                }
            }

            if (site.MinAisle > 0.0)
            {
                ReportNarrowestAisle(Distinct(plan.Cells.Select(c => c.X)), footprintX, site.MinAisle, "entre filas", violations);
                ReportNarrowestAisle(Distinct(plan.Cells.Select(c => c.Y)), footprintY, site.MinAisle, "entre columnas", violations);
            }

            return new WarehouseFitResult(violations);
        }

        /// <summary>Verdict for ONE rack rectangle against the site (bounds/polygon + obstacles), used by the auto-fill
        /// to keep/drop cells one by one. Null = fits. Bounds violations win over obstacle hits.</summary>
        public static FitViolationKind? ClassifyRect(double rx0, double ry0, double rx1, double ry1, WarehouseSite site)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));

            if (IsOutOfBounds(rx0, ry0, rx1, ry1, site))
            {
                return FitViolationKind.OutOfBounds;
            }

            foreach (var obstacle in site.Obstacles)
            {
                if (OverlapsObstacle(rx0, ry0, rx1, ry1, obstacle))
                {
                    return FitViolationKind.HitsObstacle;
                }
            }

            return null;
        }

        /// <summary>Bounds test: inside the rectangular envelope inset by the wall clearance, or — for an irregular
        /// site — inside the polygon with the rect EXPANDED by the wall clearance (Chebyshev distance to the walls;
        /// equivalent to the inset for axis-aligned walls, slightly conservative for diagonal ones).</summary>
        private static bool IsOutOfBounds(double rx0, double ry0, double rx1, double ry1, WarehouseSite site)
        {
            if (site.Boundary != null && site.Boundary.Count >= 3)
            {
                var wc = site.WallClearance;
                return !PolygonGeometry.ContainsRect(site.Boundary, rx0 - wc + Eps, ry0 - wc + Eps, rx1 + wc - Eps, ry1 + wc - Eps);
            }

            var minX = site.OriginX + site.WallClearance;
            var minY = site.OriginY + site.WallClearance;
            var maxX = site.OriginX + site.Width - site.WallClearance;
            var maxY = site.OriginY + site.Depth - site.WallClearance;
            return rx0 < minX - Eps || ry0 < minY - Eps || rx1 > maxX + Eps || ry1 > maxY + Eps;
        }

        private static bool OverlapsObstacle(double rx0, double ry0, double rx1, double ry1, SiteObstacle obstacle)
            => Overlaps(rx0, ry0, rx1, ry1,
                obstacle.X - obstacle.Clearance, obstacle.Y - obstacle.Clearance,
                obstacle.X + obstacle.Width + obstacle.Clearance, obstacle.Y + obstacle.Depth + obstacle.Clearance);

        /// <summary>Two rectangles overlap when they overlap on BOTH axes (touching edges do not count as overlap).</summary>
        private static bool Overlaps(double ax0, double ay0, double ax1, double ay1, double bx0, double by0, double bx1, double by1)
            => ax0 < bx1 - Eps && ax1 > bx0 + Eps && ay0 < by1 - Eps && ay1 > by0 + Eps;

        private static List<double> Distinct(IEnumerable<double> values)
            => values.Distinct().OrderBy(v => v).ToList();

        /// <summary>Report ONE violation per axis: the narrowest gap between consecutive rack rows/columns, if below the
        /// minimum (a regular grid has a uniform gap, but this stays correct if a future planner varies it).</summary>
        private static void ReportNarrowestAisle(List<double> origins, double footprint, double minAisle, string where, List<FitViolation> violations)
        {
            if (origins.Count < 2)
            {
                return;
            }

            var narrowest = double.MaxValue;
            for (var i = 1; i < origins.Count; i++)
            {
                narrowest = Math.Min(narrowest, origins[i] - origins[i - 1] - footprint);
            }

            if (narrowest < minAisle - Eps)
            {
                violations.Add(new FitViolation(FitViolationKind.AisleTooNarrow,
                    string.Format(CultureInfo.InvariantCulture,
                        "El pasillo {0} más angosto ({1:0.##}\") es menor al mínimo ({2:0.##}\").", where, narrowest, minAisle)));
            }
        }
    }
}
