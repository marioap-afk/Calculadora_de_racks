using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Layout;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The site-fit checker: racks inside the envelope, clear of obstacles, aisles above the minimum.</summary>
    public class WarehouseFitCheckerTests
    {
        // A 2x2 grid: rack 40 (X/depth) x 100 (Y/width); row aisle 96, column aisle 12. Cells occupy
        // X in {[0,40], [136,176]} and Y in {[0,100], [112,212]}. Total extent 176 x 212.
        private static WarehouseGridPlan Grid()
            => WarehouseGridPlanner.Plan(footprintX: 40, footprintY: 100, rows: 2, cols: 2, aisleX: 96, aisleY: 12);

        private const double FootX = 40;
        private const double FootY = 100;

        [Fact]
        public void Check_GridInsideEnvelope_Fits()
        {
            var site = new WarehouseSite(width: 200, depth: 250);

            var result = WarehouseFitChecker.Check(Grid(), FootX, FootY, site);

            Assert.True(result.Fits);
            Assert.Empty(result.Violations);
        }

        [Fact]
        public void Check_RackPastEnvelope_ReportsOutOfBounds()
        {
            var site = new WarehouseSite(width: 150, depth: 250); // the far row (rx1 = 176) pokes past 150

            var result = WarehouseFitChecker.Check(Grid(), FootX, FootY, site);

            Assert.False(result.Fits);
            Assert.Contains(result.Violations, v => v.Kind == FitViolationKind.OutOfBounds);
        }

        [Fact]
        public void Check_WallClearance_PushesRacksOffTheWall()
        {
            // Fits without clearance, but a 10" wall gap is violated by the racks sitting at X=0 / Y=0.
            var site = new WarehouseSite(width: 200, depth: 250, wallClearance: 10);

            var result = WarehouseFitChecker.Check(Grid(), FootX, FootY, site);

            Assert.False(result.Fits);
            Assert.Contains(result.Violations, v => v.Kind == FitViolationKind.OutOfBounds);
        }

        [Fact]
        public void Check_ObstacleInsideARack_ReportsCollision()
        {
            var column = new SiteObstacle(x: 20, y: 20, width: 10, depth: 10, label: "Columna A3"); // inside rack A1 [0,40]x[0,100]
            var site = new WarehouseSite(width: 200, depth: 250, obstacles: new[] { column });

            var result = WarehouseFitChecker.Check(Grid(), FootX, FootY, site);

            Assert.False(result.Fits);
            var hit = Assert.Single(result.Violations, v => v.Kind == FitViolationKind.HitsObstacle);
            Assert.Contains("Columna A3", hit.Message);
        }

        [Fact]
        public void Check_ObstacleInTheAisle_Clears()
        {
            // A column at X~88 sits in the row aisle (between X=40 and X=136), touching no rack.
            var column = new SiteObstacle(x: 86, y: 40, width: 4, depth: 4, label: "Columna");
            var site = new WarehouseSite(width: 200, depth: 250, obstacles: new[] { column });

            var result = WarehouseFitChecker.Check(Grid(), FootX, FootY, site);

            Assert.True(result.Fits);
        }

        [Fact]
        public void Check_ObstacleClearance_CanReachIntoARack()
        {
            // The column body (X 44..48) is in the aisle, but a 6" clearance (X 38..54) reaches into rack A1 (ends at 40).
            var column = new SiteObstacle(x: 44, y: 20, width: 4, depth: 4, clearance: 6, label: "Columna");
            var site = new WarehouseSite(width: 200, depth: 250, obstacles: new[] { column });

            var result = WarehouseFitChecker.Check(Grid(), FootX, FootY, site);

            Assert.Contains(result.Violations, v => v.Kind == FitViolationKind.HitsObstacle);
        }

        [Fact]
        public void Check_AisleBelowMinimum_ReportsNarrowAisle()
        {
            // Column aisle is 12"; require 96". Row aisle (96") is fine, so exactly one aisle violation.
            var site = new WarehouseSite(width: 300, depth: 300, minAisle: 96);

            var result = WarehouseFitChecker.Check(Grid(), FootX, FootY, site);

            var narrow = Assert.Single(result.Violations, v => v.Kind == FitViolationKind.AisleTooNarrow);
            Assert.Contains("entre columnas", narrow.Message);
        }

        [Fact]
        public void Check_AislesAboveMinimum_Fit()
        {
            var site = new WarehouseSite(width: 300, depth: 300, minAisle: 10); // both aisles (96 and 12) clear 10

            var result = WarehouseFitChecker.Check(Grid(), FootX, FootY, site);

            Assert.True(result.Fits);
        }

        [Fact]
        public void Check_AppliesFootprintOffset_ToTheRackRectangle()
        {
            // The rack's real min corner is +20 in X from the cell origin (e.g. a center/bottom-anchored planta block).
            // The envelope fits the un-offset rects (far rack ends at 176 < 190) but not the offset ones (196 > 190).
            var site = new WarehouseSite(width: 190, depth: 250);

            Assert.True(WarehouseFitChecker.Check(Grid(), FootX, FootY, site).Fits);
            Assert.Contains(
                WarehouseFitChecker.Check(Grid(), FootX, FootY, site, offsetX: 20, offsetY: 0).Violations,
                v => v.Kind == FitViolationKind.OutOfBounds);
        }

        [Fact]
        public void Check_SingleRack_HasNoAisleViolations()
        {
            var plan = WarehouseGridPlanner.Plan(FootX, FootY, rows: 1, cols: 1, aisleX: 0, aisleY: 0);
            var site = new WarehouseSite(width: 200, depth: 200, minAisle: 96);

            var result = WarehouseFitChecker.Check(plan, FootX, FootY, site);

            Assert.DoesNotContain(result.Violations, v => v.Kind == FitViolationKind.AisleTooNarrow);
        }
    }
}
