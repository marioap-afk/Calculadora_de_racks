using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class HardcodedStandardRackFrameServiceTests
    {
        private static RackFrameConfiguration CreateStandard()
        {
            return new HardcodedStandardRackFrameService().CreateDefault();
        }

        [Fact]
        public void CreateDefault_ProducesFourHorizontalsAtExpectedElevations()
        {
            var configuration = CreateStandard();

            Assert.Equal(4, configuration.Horizontals.Count);
            Assert.Equal(new[] { "H1", "H2", "H3", "H4" }, configuration.Horizontals.Select(h => h.Id));
            Assert.Equal(new[] { 0.0, 44.0, 88.0, 132.0 }, configuration.Horizontals.Select(h => h.Elevation));
            Assert.All(configuration.Horizontals, h => Assert.True(h.IsStandard));
        }

        [Fact]
        public void CreateDefault_ProducesThreeConsecutivePanels()
        {
            var configuration = CreateStandard();

            Assert.Equal(3, configuration.BracingPanels.Count);

            // Invariant: each panel spans two consecutive horizontals (no skips).
            Assert.Equal(new[] { "H1", "H2", "H3" }, configuration.BracingPanels.Select(p => p.LowerHorizontalId));
            Assert.Equal(new[] { "H2", "H3", "H4" }, configuration.BracingPanels.Select(p => p.UpperHorizontalId));
            Assert.All(configuration.BracingPanels, p => Assert.Equal(BracingPattern.SingleDiagonal, p.Arrangement));
        }

        [Fact]
        public void CreateDefault_TargetHeightMatchesTopHorizontal()
        {
            var configuration = CreateStandard();

            Assert.Equal(132.0, configuration.Height);
            Assert.Equal(configuration.Height, configuration.Horizontals.Max(h => h.Elevation));
        }
    }
}
