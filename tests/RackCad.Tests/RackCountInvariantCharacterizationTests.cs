using System.Linq;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>I-55 G3: the list counts physical views while rack identity remains grouped by RackId.</summary>
    public sealed class RackCountInvariantCharacterizationTests
    {
        [Fact]
        public void ThreeViewDefinitionsRemainOneRackWithThreeViews()
        {
            var entries = RackListBuilder.Build(new[]
            {
                View("rack-a", RackEmbedDocument.ViewFrontal, 0),
                View("rack-a", RackEmbedDocument.ViewLateral, 2),
                View("rack-a", RackEmbedDocument.ViewPlanta, -1)
            });
            var rack = Assert.Single(entries);
            Assert.Equal("rack-a", rack.Id);
            Assert.Equal(3, rack.ViewCount);
        }

        [Fact]
        public void CaseInsensitiveRackIdsRemainOneRackAndDistinctIdsRemainTwo()
        {
            var entries = RackListBuilder.Build(new[]
            {
                View("Rack-A", RackEmbedDocument.ViewFrontal, 0),
                View("rack-a", RackEmbedDocument.ViewLateral, 1),
                View("rack-b", RackEmbedDocument.ViewFrontal, 0)
            });
            Assert.Equal(2, entries.Count);
            Assert.Equal(2, entries.Single(e => e.Id.Equals("Rack-A", System.StringComparison.OrdinalIgnoreCase)).ViewCount);
        }

        [Fact]
        public void BomRepresentativeStillUsesOneAuthoredDocumentPerRack()
        {
            var source = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Application", "Bom", "BomAuthoredAuthority.cs");
            Assert.Contains("var representative = siblings[0].DefinitionId", source);
            Assert.Contains("SelectiveAuthoredAuthority.Resolve(rackId, siblings)", source);
            Assert.Contains("BomAuthoredAuthority", source);
        }

        private static RackEmbedDocument View(string id, string view, int section) => new RackEmbedDocument
        {
            Id = id, Kind = RackEmbedDocument.KindSelective, Name = "Rack", View = view, Section = section
        };
    }
}
