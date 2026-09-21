using Xunit;

namespace RackCad.Tests
{
    /// <summary>I-55 G3 baseline for IC-10. These different predicates and outcomes are current behavior.</summary>
    public sealed class InsertBlankIdFlowCharacterizationTests
    {
        [Theory]
        [InlineData("RackSelectivoCommands.cs", "string.IsNullOrEmpty(embed.Id) ? window.RackId : embed.Id")]
        [InlineData("RackDinamicoCommands.cs", "string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id")]
        [InlineData("RackCabeceraCommands.cs", "string.IsNullOrEmpty(embed.Id) ? System.Guid.NewGuid().ToString() : embed.Id")]
        public void CurrentReadersCureBlankIdentityByKind(string file, string expression)
        {
            Assert.Contains(expression, I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", file));
        }

        [Theory]
        [InlineData("RackPushBackCommands.cs", "string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id", "viewBlock.Embed.Id")]
        [InlineData("RackCantileverCommands.cs", "string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id", "viewBlock.Embed.Id")]
        public void StrictReadersMintFallbackThenRejectSiblingsThatDoNotCarryIt(string file, string fallback, string siblingId)
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", file);
            Assert.Contains(fallback, source);
            Assert.Contains(siblingId, source);
            Assert.Contains("return;", source);
        }

        [Fact]
        public void SelectiveFallbackCanAdoptExistingDefinitionsWithTheCuredId()
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", "RackSelectivoCommands.cs");
            var id = source.IndexOf("var id = string.IsNullOrEmpty(embed.Id)", System.StringComparison.Ordinal);
            var scan = source.IndexOf("FindRackBlocks(document, id)", System.StringComparison.Ordinal);
            Assert.True(id >= 0 && scan > id);
        }

        [Fact]
        public void FlowBedHasNoSiblingScanAndKeepsItsWindowIdentity()
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", "RackCamaCommands.cs");
            Assert.DoesNotContain("FindRackBlocks", source);
            Assert.Contains("window.RackId", source);
            Assert.Contains("RedrawInPlace(", source);
        }
    }
}
