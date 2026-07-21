using System.Linq;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The pure selection/placeholder logic behind <see cref="CatalogCombo"/>. No WPF.</summary>
    public sealed class CatalogComboSelectionTests
    {
        private static CatalogOption[] Options() => new[]
        {
            new CatalogOption("POST_A", "Poste A"),
            new CatalogOption("POST_B", "Poste B"),
        };

        [Fact]
        public void Resolve_FindsById()
        {
            var match = CatalogComboSelection.Resolve(Options(), "POST_B");

            Assert.NotNull(match);
            Assert.Equal("POST_B", match.Id);
        }

        [Fact]
        public void Resolve_IsCaseInsensitiveAndTrims()
        {
            var match = CatalogComboSelection.Resolve(Options(), "  post_a ");

            Assert.NotNull(match);
            Assert.Equal("POST_A", match.Id);
        }

        [Theory]
        [InlineData("MISSING")]
        [InlineData("")]
        [InlineData(null)]
        public void Resolve_ReturnsNull_WhenAbsentOrBlank(string id)
        {
            Assert.Null(CatalogComboSelection.Resolve(Options(), id));
        }

        [Fact]
        public void WithPlaceholder_PrependsSentinel()
        {
            var placeholder = new CatalogOption(null, "(auto)");

            var list = CatalogComboSelection.WithPlaceholder(placeholder, Options()).ToList();

            Assert.Equal(3, list.Count);
            Assert.Same(placeholder, list[0]);
            Assert.Equal("POST_A", list[1].Id);
        }

        [Fact]
        public void WithPlaceholder_NullPlaceholder_ReturnsOptionsOnly()
        {
            var list = CatalogComboSelection.WithPlaceholder(null, Options()).ToList();

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void WithPlaceholder_NullOptions_ReturnsPlaceholderOnly()
        {
            var placeholder = new CatalogOption(null, "(none)");

            var list = CatalogComboSelection.WithPlaceholder(placeholder, null).ToList();

            Assert.Single(list);
            Assert.Same(placeholder, list[0]);
        }
    }
}
