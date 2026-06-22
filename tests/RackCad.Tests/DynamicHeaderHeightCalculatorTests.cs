using RackCad.Application.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicHeaderHeightCalculatorTests
    {
        [Theory]
        [InlineData(134.0, 144.0)]
        [InlineData(144.0, 144.0)]
        [InlineData(145.0, 156.0)]
        [InlineData(181.0, 192.0)]
        public void RoundUpToCommercialFoot_RoundsToNextWholeFoot(double theoretical, double expected)
        {
            Assert.Equal(expected, DynamicHeaderHeightCalculator.RoundUpToCommercialFoot(theoretical), 4);
        }

        [Fact]
        public void Calculate_StacksEveryTermAndRoundsUp()
        {
            // load 48, 3 levels, first level at 6", 3" beams, 192" lane:
            //   clearSpace = 48 + 6 = 54
            //   between    = (3-1) * 54 = 108
            //   top finish = 54 / 3 = 18
            //   beams      = 3 * 3 = 9
            //   slope      = (192/12) * (7/16) = 16 * 0.4375 = 7
            //   theoretical= 6 + 9 + 108 + 18 + 7 = 148  → ceil(148/12)*12 = 156
            var result = DynamicHeaderHeightCalculator.Calculate(
                loadHeight: 48.0, levels: 3, firstLevelHeight: 6.0, beamDepth: 3.0, totalDepth: 192.0);

            Assert.Equal(54.0, result.ClearSpace, 4);
            Assert.Equal(7.0, result.Slope, 4);
            Assert.Equal(148.0, result.TheoreticalHeight, 4);
            Assert.Equal(156.0, result.HeaderHeight, 4);
        }

        [Fact]
        public void Calculate_ClearSpace_IsLoadPlusSix()
        {
            var result = DynamicHeaderHeightCalculator.Calculate(40.0, 2, 4.0, 3.0, 96.0);

            Assert.Equal(46.0, result.ClearSpace, 4);
        }

        [Fact]
        public void Calculate_LevelsBelowOne_AreClampedToOne()
        {
            var clamped = DynamicHeaderHeightCalculator.Calculate(48.0, 0, 6.0, 3.0, 192.0);
            var one = DynamicHeaderHeightCalculator.Calculate(48.0, 1, 6.0, 3.0, 192.0);

            Assert.Equal(one.TheoreticalHeight, clamped.TheoreticalHeight, 4);
        }
    }
}
