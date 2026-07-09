using RackCad.Application.Catalogs;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The user-maintained "Peraltes" column parser (was private UI code; the combos feed from this).</summary>
    public class PeralteListTests
    {
        [Fact]
        public void Parse_SemicolonList_KeepsOrderAndValues()
        {
            Assert.Equal(new[] { 3.0, 3.5, 4.0, 4.5, 5.0 }, PeralteList.Parse("3;3.5;4;4.5;5"));
        }

        [Fact]
        public void Parse_AcceptsCommasAsSeparatorToo()
        {
            Assert.Equal(new[] { 4.0, 6.0 }, PeralteList.Parse("4,6"));
        }

        [Fact]
        public void Parse_SkipsJunkNonPositivesAndDuplicates()
        {
            Assert.Equal(new[] { 3.0, 5.0 }, PeralteList.Parse("3; abc; -2; 0; 3; 5"));
        }

        [Fact]
        public void Parse_NullOrBlank_YieldsEmpty()
        {
            Assert.Empty(PeralteList.Parse(null));
            Assert.Empty(PeralteList.Parse("   "));
        }
    }
}
