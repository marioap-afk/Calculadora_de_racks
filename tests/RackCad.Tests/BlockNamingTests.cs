using RackCad.Application;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The pure block-naming helpers moved out of the Plugin so they could be tested here.</summary>
    public class BlockNamingTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SanitizeBlockName_EmptyOrBlank_FallsBackToCabecera(string input)
        {
            Assert.Equal("Cabecera", BlockNaming.SanitizeBlockName(input));
        }

        [Fact]
        public void SanitizeBlockName_ReplacesInvalidCharsWithSpacesAndTrims()
        {
            // AutoCAD forbids < > / \ " : ; ? * | , = ` — each becomes a space, then the result is trimmed.
            Assert.Equal("a b c", BlockNaming.SanitizeBlockName("a<b>c"));      // '<' and '>' → spaces
            Assert.Equal("Sistema  dinamico", BlockNaming.SanitizeBlockName("Sistema; dinamico")); // ';' + its space → 2
            Assert.Equal("Rack A", BlockNaming.SanitizeBlockName("  Rack A:  "));  // ':' → space, then trailing trim
        }

        [Fact]
        public void SanitizeBlockName_LeavesAValidNameUntouched()
        {
            Assert.Equal("Rack A", BlockNaming.SanitizeBlockName("Rack A"));
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void NormalizeWhitespace_NullOrEmpty_PassesThrough(string input, string expected)
        {
            Assert.Equal(expected, BlockNaming.NormalizeWhitespace(input));
        }

        [Fact]
        public void NormalizeWhitespace_CollapsesNewlinesTabsAndRepeatedSpacesToOneAndTrims()
        {
            var multiline = "Poste omega 3\" x 3\"\r\n  troquel gota de agua \tcalibre 14 ";
            Assert.Equal("Poste omega 3\" x 3\" troquel gota de agua calibre 14", BlockNaming.NormalizeWhitespace(multiline));
        }
    }
}
