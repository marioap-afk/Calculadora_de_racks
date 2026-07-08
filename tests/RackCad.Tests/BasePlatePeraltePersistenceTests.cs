using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The base plate's manual peralte override survives serialization; legacy/blank files stay derived (null).</summary>
    public class BasePlatePeraltePersistenceTests
    {
        [Fact]
        public void PlateDocument_RoundTripsPeralteOverride()
        {
            var plate = new BasePlatePlacement { PostSide = PostSide.Left, PlateCatalogId = "PL", PeralteOverride = 5.5 };

            var back = PlateDocument.From(plate).ToDomain(PostSide.Left);

            Assert.True(back.PeralteOverride.HasValue);
            Assert.Equal(5.5, back.PeralteOverride.Value, 4);
        }

        [Fact]
        public void PlateDocument_NullOverride_StaysNull()
        {
            var plate = new BasePlatePlacement { PostSide = PostSide.Right, PlateCatalogId = "PL" };

            var back = PlateDocument.From(plate).ToDomain(PostSide.Right);

            Assert.Null(back.PeralteOverride);
        }
    }
}
