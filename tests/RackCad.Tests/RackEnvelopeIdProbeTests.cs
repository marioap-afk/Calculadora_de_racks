using RackCad.Application.Views.Insertion;
using Xunit;

namespace RackCad.Tests
{
    public class RackEnvelopeIdProbeTests
    {
        [Theory]
        [InlineData("{\"Id\":\"rack\",\"Design\":{\"Id\":\"nested\"}}", "rack")]
        [InlineData("{\"iD\":\"rack\"}", "rack")]
        [InlineData("{\"Design\":{\"Id\":\"nested\"}}", null)]
        [InlineData("{\"Id\":\"a\",\"ID\":\"b\"}", null)]
        [InlineData("{broken", null)]
        public void Only_one_readable_top_level_id_is_attributable(string json, string expected)
            => Assert.Equal(expected, RackEnvelopeIdProbe.Probe(json));
    }
}
