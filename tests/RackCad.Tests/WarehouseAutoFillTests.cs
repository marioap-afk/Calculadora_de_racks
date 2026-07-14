using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Layout;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Polygon tests + the auto-fill: max racks in a site, dropping cells outside the polygon or on columns,
    /// and picking the orientation that fits more.</summary>
    public class WarehouseAutoFillTests
    {
        private static Point2D[] Rect(double w, double d) => new[]
        {
            new Point2D(0, 0), new Point2D(w, 0), new Point2D(w, d), new Point2D(0, d)
        };

        /// <summary>An L: 500 wide × 400 deep, but for y > 200 only x ≤ 300 exists (a 200×200 notch at the far corner).</summary>
        private static Point2D[] LShape() => new[]
        {
            new Point2D(0, 0), new Point2D(500, 0), new Point2D(500, 200),
            new Point2D(300, 200), new Point2D(300, 400), new Point2D(0, 400)
        };

        // ---- PolygonGeometry ----

        [Fact]
        public void Polygon_Contains_InsideOutsideAndBoundary()
        {
            var l = LShape();

            Assert.True(PolygonGeometry.Contains(l, 100, 100));   // in the main body
            Assert.True(PolygonGeometry.Contains(l, 100, 300));   // in the leg
            Assert.False(PolygonGeometry.Contains(l, 400, 300));  // in the notch (outside)
            Assert.True(PolygonGeometry.Contains(l, 0, 0));       // corner counts as inside
            Assert.True(PolygonGeometry.Contains(l, 250, 0));     // edge counts as inside
            Assert.False(PolygonGeometry.Contains(l, -1, 100));
        }

        [Fact]
        public void Polygon_ContainsRect_HandlesTheNotch()
        {
            var l = LShape();

            Assert.True(PolygonGeometry.ContainsRect(l, 10, 10, 200, 190));    // main body
            Assert.True(PolygonGeometry.ContainsRect(l, 10, 210, 290, 390));   // leg
            Assert.False(PolygonGeometry.ContainsRect(l, 250, 150, 350, 250)); // straddles the notch corner
            Assert.False(PolygonGeometry.ContainsRect(l, 350, 250, 450, 350)); // fully in the notch
            Assert.True(PolygonGeometry.ContainsRect(l, 0, 0, 500, 200));      // flush against every wall
        }

        [Fact]
        public void Polygon_ContainsRect_CatchesASpike_WithAllCornersInside()
        {
            // A V-notch stabbing down to (5,2): the rect (1,1)-(9,3) has all 4 corners inside, but the notch's edges
            // cross its interior — the edge test must reject it.
            var spiked = new[]
            {
                new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10),
                new Point2D(6, 10), new Point2D(5, 2), new Point2D(4, 10), new Point2D(0, 10)
            };

            Assert.True(PolygonGeometry.Contains(spiked, 1, 3));
            Assert.True(PolygonGeometry.Contains(spiked, 9, 3));
            Assert.False(PolygonGeometry.ContainsRect(spiked, 1, 1, 9, 3));
        }

        // ---- Fit checker with a polygon boundary ----

        [Fact]
        public void FitChecker_PolygonSite_FlagsRacksInTheNotch()
        {
            var site = WarehouseSite.FromBoundary(LShape());
            // 4 rows × 3 cols of 40×100 racks, aisles 96/12: X rows at 0/136/272/408, Y cols at 0/112/224.
            var plan = WarehouseGridPlanner.Plan(40, 100, 4, 3, 96, 12);

            var result = WarehouseFitChecker.Check(plan, 40, 100, site);

            Assert.False(result.Fits);
            Assert.Contains(result.Violations, v => v.Kind == FitViolationKind.OutOfBounds);
        }

        // ---- Auto-fill ----

        [Fact]
        public void Fill_RectangularSite_FillsTheFullGrid()
        {
            var site = new WarehouseSite(width: 500, depth: 400);

            var result = WarehouseAutoFill.Fill(site, rackDepth: 40, rackWidth: 100, aisleRows: 96, aisleCols: 12, tryRotated: false);

            // rows: offsets 0/136/272/408 (+40 ≤ 500) → 4; cols: 0/112/224 (+100 ≤ 400) → 3.
            Assert.Equal(4, result.RowsTried);
            Assert.Equal(3, result.ColsTried);
            Assert.Equal(12, result.Cells.Count);
            Assert.Equal(0, result.OmittedOutside);
            Assert.Equal(0, result.OmittedByObstacle);
        }

        [Fact]
        public void Fill_LShapedSite_DropsTheCellsInTheNotch()
        {
            var site = WarehouseSite.FromBoundary(LShape());

            var result = WarehouseAutoFill.Fill(site, 40, 100, 96, 12, tryRotated: false);

            // Grid 4×3 over the bbox; cells whose rack crosses into the notch (x > 300 with y > 200) drop out:
            // col y=112..212 loses rows x=272/408; col y=224..324 loses the same two → 12 − 4 = 8.
            Assert.Equal(8, result.Cells.Count);
            Assert.Equal(4, result.OmittedOutside);
            Assert.Equal(0, result.OmittedByObstacle);
            // The kept cells keep their grid labels (gaps where cells dropped).
            Assert.Contains(result.Cells, c => c.Label == "A1");
            Assert.DoesNotContain(result.Cells, c => c.Label == "C2"); // row x=272, col y=112 → dropped
        }

        [Fact]
        public void Fill_ColumnObstacle_DropsOnlyItsCell()
        {
            var column = new SiteObstacle(x: 150, y: 50, width: 12, depth: 12, clearance: 4, label: "Columna");
            var site = new WarehouseSite(width: 500, depth: 400, obstacles: new[] { column });

            var result = WarehouseAutoFill.Fill(site, 40, 100, 96, 12, tryRotated: false);

            // The column (146..166 with clearance) hits row x=136..176 at col y=0..100 → exactly one cell drops.
            Assert.Equal(11, result.Cells.Count);
            Assert.Equal(1, result.OmittedByObstacle);
            Assert.DoesNotContain(result.Cells, c => c.Row == 1 && c.Col == 0);
        }

        [Fact]
        public void Fill_TryRotated_PicksTheOrientationWithMoreRacks()
        {
            // Site 110 (X) × 500 (Y); rack 40 deep × 100 wide; aisles 10/10.
            // AlongDepth: rows(110, fp40, pitch50) = 2; cols(500, fp100, pitch110) = 4 → 8 racks.
            // Rotated:   rows(110, fp100) = 1;         cols(500, fp40, pitch50) = 10 → 10 racks. Rotated wins.
            var site = new WarehouseSite(width: 110, depth: 500);

            var result = WarehouseAutoFill.Fill(site, 40, 100, 10, 10, tryRotated: true);

            Assert.Equal(RackOrientation.Rotated, result.Orientation);
            Assert.Equal(10, result.Cells.Count);
        }

        [Fact]
        public void Fill_Rotated_SwapsAisleAxes_SoThePickAisleFollowsTheRackDepth()
        {
            // Review-workflow regression: with world-bound aisles, the rotated try packed racks face-to-face with NO
            // pick aisle and "won" the count. Site 500×480, rack 40 deep × 100 wide, pick aisle 96, end gap 0:
            // AlongDepth = 4×4 = 16; rotated (ends along X pitch 100 → 5, faces along Y pitch 136 → 4) = 20 — a
            // GENUINELY better, feasible layout (pick aisle preserved along the rack depth).
            var site = new WarehouseSite(width: 500, depth: 480);

            var result = WarehouseAutoFill.Fill(site, rackDepth: 40, rackWidth: 100, aisleRows: 96, aisleCols: 0, tryRotated: true);

            Assert.Equal(RackOrientation.Rotated, result.Orientation);
            Assert.Equal(20, result.Cells.Count);
            // The rotated grid keeps the 96" pick aisle along the depth axis (world Y): col pitch = 40 + 96.
            var byCol = result.Cells.Where(c => c.Row == 0).OrderBy(c => c.Col).ToList();
            Assert.Equal(136.0, byCol[1].Y - byCol[0].Y, 6);
        }

        [Fact]
        public void Fill_BackToBack_DoesNotTryTheRotatedOrientation()
        {
            // Pairing is only a true back-to-back along the rack depth (the planner pairs on X), so the rotated try is
            // skipped: same 500×480 site where rotated would win by raw count, but paired rotated would be mis-paired.
            var site = new WarehouseSite(width: 500, depth: 480);

            var result = WarehouseAutoFill.Fill(site, 40, 100, 96, 0, RowPairing.BackToBack, 6, tryRotated: true);

            Assert.Equal(RackOrientation.AlongDepth, result.Orientation);
            // b2b rows on 500: offsets 0/46/182/228/364/410 (+40 ≤ 500) = 6; cols pitch 100 on 480 = 4 → 24.
            Assert.Equal(24, result.Cells.Count);
        }

        [Fact]
        public void Fill_BackToBack_FitsMoreRowsThanSingle()
        {
            // Span 300, rack 40 deep, aisle 96, flue 6. Single: 0/136/272 (+40>300 at 272? 272+40=312 → 2 rows: 0,136).
            // BackToBack: 0/46/182/228 (+40 ≤ 300) → 4 rows.
            var site = new WarehouseSite(width: 300, depth: 100);

            var single = WarehouseAutoFill.Fill(site, 40, 100, 96, 0, RowPairing.Single, 0, tryRotated: false);
            var paired = WarehouseAutoFill.Fill(site, 40, 100, 96, 0, RowPairing.BackToBack, 6, tryRotated: false);

            Assert.Equal(2, single.Cells.Count);
            Assert.Equal(4, paired.Cells.Count);
        }

        [Fact]
        public void Fill_RackBiggerThanSite_ReturnsEmpty()
        {
            var site = new WarehouseSite(width: 30, depth: 50);

            var result = WarehouseAutoFill.Fill(site, 40, 100, 96, 12);

            Assert.Empty(result.Cells);
        }

        [Fact]
        public void Fill_WallClearance_ShrinksTheUsableArea()
        {
            // 500×400 fits 4×3 without clearance; a 50" wall clearance leaves 400×300 → rows 0/136/272 (+40 ≤ 400) = 3,
            // cols 0/112 (+100 ≤ 300) = 2 → 6 racks, anchored at (50,50).
            var site = new WarehouseSite(width: 500, depth: 400, wallClearance: 50);

            var result = WarehouseAutoFill.Fill(site, 40, 100, 96, 12, tryRotated: false);

            Assert.Equal(6, result.Cells.Count);
            var seedCell = result.Cells.Single(c => c.Row == 0 && c.Col == 0);
            Assert.Equal(50.0, seedCell.X, 6);
            Assert.Equal(50.0, seedCell.Y, 6);
        }
    }
}
