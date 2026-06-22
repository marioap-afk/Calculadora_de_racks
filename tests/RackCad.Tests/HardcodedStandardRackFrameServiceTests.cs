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
        public void CreateDefault_ProducesParametricHorizontals_StartingAtTheCelosiaTroquel()
        {
            var configuration = CreateStandard();

            // First travesaño at the start troquel (troquel 3 -> 4"), then panels of 44", then two closings.
            Assert.Equal(new[] { "H1", "H2", "H3", "H4", "H5" }, configuration.Horizontals.Select(h => h.Id));
            Assert.Equal(new[] { 4.0, 48.0, 92.0, 110.0, 128.0 }, configuration.Horizontals.Select(h => h.Elevation));
            Assert.All(configuration.Horizontals, h => Assert.True(h.IsStandard));
        }

        [Fact]
        public void CreateDefault_StandardPanelsAreDiagonal_ClosingPanelsAreNot()
        {
            var configuration = CreateStandard();
            var panels = configuration.BracingPanels.OrderBy(p => p.Number).ToList();

            Assert.Equal(4, panels.Count);
            Assert.Equal(new[] { "H1", "H2", "H3", "H4" }, panels.Select(p => p.LowerHorizontalId));
            Assert.Equal(new[] { "H2", "H3", "H4", "H5" }, panels.Select(p => p.UpperHorizontalId));

            // Two standard diagonal panels, then two closing panels with no bracing.
            Assert.Equal(BracingPattern.SingleDiagonal, panels[0].Arrangement);
            Assert.Equal(BracingPattern.SingleDiagonal, panels[1].Arrangement);
            Assert.Equal(BracingPattern.NoBracing, panels[2].Arrangement);
            Assert.Equal(BracingPattern.NoBracing, panels[3].Arrangement);
        }

        [Fact]
        public void CreateDefault_ClosingTravesanosClearThePostTop()
        {
            var configuration = CreateStandard();

            Assert.Equal(132.0, configuration.Height);
            // The top travesaño deliberately sits below the post top (the closings clear it).
            Assert.True(configuration.Horizontals.Max(h => h.Elevation) < configuration.Height);
        }
    }
}
