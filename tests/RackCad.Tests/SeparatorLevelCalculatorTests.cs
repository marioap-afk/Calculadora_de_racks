using System.Linq;
using RackCad.Application.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class SeparatorLevelCalculatorTests
    {
        [Theory]
        [InlineData(40.0, 2)]
        [InlineData(60.0, 2)]
        [InlineData(119.0, 2)]
        [InlineData(120.0, 3)]
        [InlineData(179.0, 3)]
        [InlineData(180.0, 4)]
        [InlineData(0.0, 2)]
        public void Count_FollowsTheSixtyInchRuleWithMinimumTwo(double height, int expected)
        {
            Assert.Equal(expected, SeparatorLevelCalculator.Count(height));
        }

        [Fact]
        public void Levels_StandardCase_DistributesHalfFullFullHalf()
        {
            // 120" header, 3 separators, troquel grid at 0 → ideal 20-40-40-20 → separators at 20/60/100.
            var levels = SeparatorLevelCalculator.Levels(120.0, troquelSeparadorY: 0.0, paso: 2.0);

            Assert.Equal(new[] { 20.0, 60.0, 100.0 }, levels.Select(y => System.Math.Round(y, 4)));
        }

        [Fact]
        public void Levels_TwoSeparators_AreHalfFromEachEnd()
        {
            // 40" header, 2 separators → ideal 10-20-10 → separators at 10/30.
            var levels = SeparatorLevelCalculator.Levels(40.0, troquelSeparadorY: 0.0, paso: 2.0);

            Assert.Equal(new[] { 10.0, 30.0 }, levels.Select(y => System.Math.Round(y, 4)));
        }

        [Fact]
        public void Levels_LandOnTheTroquelGrid_EvenSpacing()
        {
            const double baseY = 2.1563; // real TROQUEL_SEPARADOR.Y
            var levels = SeparatorLevelCalculator.Levels(120.0, baseY, paso: 2.0).ToList();

            Assert.Equal(3, levels.Count);

            // Every separator sits on the grid baseY + k*2.
            foreach (var y in levels)
            {
                var k = (y - baseY) / 2.0;
                Assert.Equal(System.Math.Round(k), k, 6);
            }

            // Between-separator spacing is an even 40".
            Assert.Equal(40.0, levels[1] - levels[0], 4);
            Assert.Equal(40.0, levels[2] - levels[1], 4);

            // Bottom space (from TROQUEL_SEPARADOR) absorbs the slack; top stays near "half" (20").
            var bottomSpace = levels[0] - baseY;
            var topSpace = 120.0 - levels[2];
            Assert.True(bottomSpace > 0.0 && bottomSpace <= 20.0);
            Assert.True(topSpace > 18.0 && topSpace <= 20.0);
        }

        [Fact]
        public void Levels_CountOverride_ForcesThatManyLevels()
        {
            var levels = SeparatorLevelCalculator.Levels(120.0, 0.0, 2.0, countOverride: 4);

            Assert.Equal(4, levels.Count);
        }

        [Fact]
        public void Levels_SpacingOverride_UsesThatSpacing()
        {
            var levels = SeparatorLevelCalculator.Levels(200.0, 0.0, 2.0, spacingOverride: 50.0).ToList();

            for (var i = 1; i < levels.Count; i++)
            {
                Assert.Equal(50.0, levels[i] - levels[i - 1], 4);
            }
        }

        [Fact]
        public void Levels_AreAscending()
        {
            var levels = SeparatorLevelCalculator.Levels(200.0, 2.1563, 2.0).ToList();

            for (var i = 1; i < levels.Count; i++)
            {
                Assert.True(levels[i] > levels[i - 1]);
            }
        }
    }
}
