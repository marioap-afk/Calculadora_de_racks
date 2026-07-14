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
