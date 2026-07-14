using System;
using System.Linq;
using RackCad.Application.Layout;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The warehouse grid planner: cell positions (footprint + aisle pitch), labels (A1, B3…) and validation.</summary>
    public class WarehouseGridPlannerTests
    {
        [Fact]
        public void Plan_PlacesCells_AtFootprintPlusAislePitch()
        {
            // 2 rows x 3 cols; rack 40 deep x 100 wide; aisles 96 (X) and 12 (Y).
            var plan = WarehouseGridPlanner.Plan(footprintX: 40, footprintY: 100, rows: 2, cols: 3, aisleX: 96, aisleY: 12);

            Assert.Equal(6, plan.Cells.Count);

            // pitchX = 40 + 96 = 136 ; pitchY = 100 + 12 = 112
            var seed = plan.Cells.Single(c => c.Row == 0 && c.Col == 0);
            Assert.Equal(0.0, seed.X, 6);
            Assert.Equal(0.0, seed.Y, 6);
            Assert.True(seed.IsSeed);

            var r1c2 = plan.Cells.Single(c => c.Row == 1 && c.Col == 2);
            Assert.Equal(136.0, r1c2.X, 6);   // one row over
            Assert.Equal(224.0, r1c2.Y, 6);   // two cols over (2 * 112)
            Assert.False(r1c2.IsSeed);
        }

        [Fact]
        public void Plan_HonorsSeedOrigin()
        {
            var plan = WarehouseGridPlanner.Plan(40, 100, 2, 2, 96, 12, seedX: 1000, seedY: -500);

            var seed = plan.Cells.Single(c => c.IsSeed);
            Assert.Equal(1000.0, seed.X, 6);
            Assert.Equal(-500.0, seed.Y, 6);

            var r1c1 = plan.Cells.Single(c => c.Row == 1 && c.Col == 1);
            Assert.Equal(1000.0 + 136.0, r1c1.X, 6);
            Assert.Equal(-500.0 + 112.0, r1c1.Y, 6);
        }

        [Fact]
        public void Plan_TotalSpan_ExcludesTrailingAisle()
        {
            var plan = WarehouseGridPlanner.Plan(40, 100, 3, 2, 96, 12);

            // 3 rows: 2 pitches + 1 footprint = 2*136 + 40 = 312
            Assert.Equal(312.0, plan.TotalX, 6);
            // 2 cols: 1 pitch + 1 footprint = 112 + 100 = 212
            Assert.Equal(212.0, plan.TotalY, 6);
        }

        [Fact]
        public void Plan_SingleCell_HasOnlySeed_NoAisleInSpan()
        {
            var plan = WarehouseGridPlanner.Plan(40, 100, 1, 1, 96, 12);

            var cell = Assert.Single(plan.Cells);
            Assert.True(cell.IsSeed);
            Assert.Equal(40.0, plan.TotalX, 6);   // just the footprint, no aisle
            Assert.Equal(100.0, plan.TotalY, 6);
        }

        [Theory]
        [InlineData(0, 0, "A1")]
        [InlineData(1, 2, "B3")]
        [InlineData(25, 0, "Z1")]
        [InlineData(26, 0, "AA1")]
        [InlineData(27, 9, "AB10")]
        public void Label_UsesLetterRow_And1BasedColumn(int row, int col, string expected)
        {
            Assert.Equal(expected, WarehouseGridPlanner.Label(row, col));
        }

        [Fact]
        public void Plan_BackToBack_PairsRowsWithFlue_AndAisleBetweenPairs()
        {
            // rack 40 deep; pick aisle 96; back-to-back flue 6. Rows pair up: [0,1] share the flue, [2,3] the next pair.
            var plan = WarehouseGridPlanner.Plan(footprintX: 40, footprintY: 100, rows: 4, cols: 1,
                aisleX: 96, aisleY: 0, pairing: RowPairing.BackToBack, backGap: 6);

            double X(int r) => plan.Cells.Single(c => c.Row == r && c.Col == 0).X;
            Assert.Equal(0.0, X(0), 6);
            Assert.Equal(46.0, X(1), 6);    // 40 (depth) + 6 (flue)
            Assert.Equal(182.0, X(2), 6);   // 86 (end of pair 0) + 96 (pick aisle)
            Assert.Equal(228.0, X(3), 6);

            Assert.Equal(6.0, X(1) - (X(0) + 40), 6);    // within-pair gap = flue
            Assert.Equal(96.0, X(2) - (X(1) + 40), 6);   // between-pair gap = pick aisle
            Assert.Equal(268.0, plan.TotalX, 6);         // X(3) + footprint, no trailing aisle
            Assert.Equal(RackOrientation.AlongDepth, plan.Orientation);
        }

        [Fact]
        public void Plan_BackToBack_OddRows_LastRowStartsALonePair()
        {
            var plan = WarehouseGridPlanner.Plan(40, 100, rows: 3, cols: 1, aisleX: 96, aisleY: 0,
                pairing: RowPairing.BackToBack, backGap: 6);

            double X(int r) => plan.Cells.Single(c => c.Row == r).X;
            Assert.Equal(46.0, X(1), 6);
            Assert.Equal(182.0, X(2), 6); // the lone third row opens the next pair, after the pick aisle
        }

        [Fact]
        public void PlanForRack_Rotated_SwapsExtents_AndRecordsOrientation()
        {
            // rack 40 deep x 100 wide, 2 rows flush. AlongDepth spaces rows by depth (40); Rotated by width (100).
            var along = WarehouseGridPlanner.PlanForRack(rackDepth: 40, rackWidth: 100, RackOrientation.AlongDepth,
                rows: 2, cols: 1, aisleBetweenRows: 0, aisleBetweenCols: 0);
            var rotated = WarehouseGridPlanner.PlanForRack(rackDepth: 40, rackWidth: 100, RackOrientation.Rotated,
                rows: 2, cols: 1, aisleBetweenRows: 0, aisleBetweenCols: 0);

            Assert.Equal(40.0, along.Cells.Single(c => c.Row == 1).X, 6);
            Assert.Equal(100.0, rotated.Cells.Single(c => c.Row == 1).X, 6);
            Assert.Equal(RackOrientation.AlongDepth, along.Orientation);
            Assert.Equal(RackOrientation.Rotated, rotated.Orientation);
        }

        [Fact]
        public void Plan_RejectsNegativeBackGap()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                WarehouseGridPlanner.Plan(40, 100, 2, 2, 10, 10, pairing: RowPairing.BackToBack, backGap: -1));
        }

        [Theory]
        [InlineData(0, 100, 2, 2, 10, 10)] // footprintX <= 0
        [InlineData(40, 0, 2, 2, 10, 10)]  // footprintY <= 0
        [InlineData(40, 100, 0, 2, 10, 10)] // rows < 1
        [InlineData(40, 100, 2, 0, 10, 10)] // cols < 1
        [InlineData(40, 100, 2, 2, -1, 10)] // aisleX < 0
        [InlineData(40, 100, 2, 2, 10, -1)] // aisleY < 0
        public void Plan_RejectsInvalidInput(double fx, double fy, int rows, int cols, double ax, double ay)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => WarehouseGridPlanner.Plan(fx, fy, rows, cols, ax, ay));
        }
    }
}
