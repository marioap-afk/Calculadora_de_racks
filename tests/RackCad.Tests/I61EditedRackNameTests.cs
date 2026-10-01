#nullable enable
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-61 OBL-P1 (D-1a): the pure rule that EditCama applies to the edited name. Blank or white space resolves to the envelope
    /// name (null included); any other edited name wins.
    /// </summary>
    public class I61EditedRackNameTests
    {
        [Fact]
        public void BlankEditedName_ResolvesToEnvelopeName()
        {
            Assert.Equal("Cama 1", EditedRackNameResolver.Resolve(string.Empty, "Cama 1"));
        }

        [Fact]
        public void WhiteSpaceOnlyEditedName_ResolvesToEnvelopeName()
        {
            Assert.Equal("Cama 1", EditedRackNameResolver.Resolve("   ", "Cama 1"));
            Assert.Equal("Cama 1", EditedRackNameResolver.Resolve(" \t ", "Cama 1"));
        }

        [Fact]
        public void BlankEditedName_WithNullEnvelope_ResolvesToNull()
        {
            Assert.Null(EditedRackNameResolver.Resolve(string.Empty, null!));
            Assert.Null(EditedRackNameResolver.Resolve("  ", null!));
        }

        [Fact]
        public void NullEditedName_ResolvesToEnvelopeName()
        {
            Assert.Equal("Cama 1", EditedRackNameResolver.Resolve(null!, "Cama 1"));
            Assert.Null(EditedRackNameResolver.Resolve(null!, null!));
        }

        [Fact]
        public void OwnEditedName_IsKept()
        {
            Assert.Equal("Mi cama", EditedRackNameResolver.Resolve("Mi cama", "Cama 1"));
            Assert.Equal("Mi cama", EditedRackNameResolver.Resolve("Mi cama", null!));
        }

        [Fact]
        public void OwnEditedName_IsReturnedAsIs_WithoutTrimming()
        {
            Assert.Equal("  Mi cama ", EditedRackNameResolver.Resolve("  Mi cama ", "Cama 1"));
        }
    }
}
