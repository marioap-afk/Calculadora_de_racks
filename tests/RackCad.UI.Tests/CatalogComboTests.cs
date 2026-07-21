using RackCad.Application.Catalogs;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The <see cref="CatalogCombo"/> control (STA): DisplayName shown, Id kept, placeholder honored.</summary>
    public sealed class CatalogComboTests
    {
        private sealed class FakeEntry : CatalogEntryBase
        {
        }

        private static CatalogOption[] Options() => new[]
        {
            new CatalogOption("POST_A", "Poste A"),
            new CatalogOption("POST_B", "Poste B"),
        };

        [Fact]
        public void Constructor_WiresDisplayAndValuePaths()
        {
            var (display, value) = StaTestRunner.Run(() =>
            {
                var combo = new CatalogCombo();
                return (combo.DisplayMemberPath, combo.SelectedValuePath);
            });

            Assert.Equal("DisplayName", display);
            Assert.Equal("Id", value);
        }

        [Fact]
        public void SetOptions_SelectsById()
        {
            var (selectedId, displayName) = StaTestRunner.Run(() =>
            {
                var combo = new CatalogCombo();
                combo.SetOptions(Options(), "POST_B");
                return (combo.SelectedId, ((CatalogOption)combo.SelectedItem).DisplayName);
            });

            Assert.Equal("POST_B", selectedId);
            Assert.Equal("Poste B", displayName);
        }

        [Fact]
        public void SetOptions_UnknownId_FallsBackToPlaceholder()
        {
            var (selectedId, isPlaceholder) = StaTestRunner.Run(() =>
            {
                var combo = new CatalogCombo();
                var placeholder = new CatalogOption(null, "(auto)");
                combo.SetOptions(Options(), "MISSING", placeholder);
                return (combo.SelectedId, ReferenceEquals(combo.SelectedItem, placeholder));
            });

            Assert.Null(selectedId);
            Assert.True(isPlaceholder);
        }

        [Fact]
        public void SetCatalogEntries_BuildsOptionsFromEntries()
        {
            var (count, selectedId, displayName) = StaTestRunner.Run(() =>
            {
                var combo = new CatalogCombo();
                var entries = new[]
                {
                    new FakeEntry { Id = "X", DisplayName = "Equis" },
                    new FakeEntry { Id = "Y", DisplayName = "Ye" },
                };
                combo.SetCatalogEntries(entries, "X");
                return (combo.Items.Count, combo.SelectedId, ((CatalogOption)combo.SelectedItem).DisplayName);
            });

            Assert.Equal(2, count);
            Assert.Equal("X", selectedId);
            Assert.Equal("Equis", displayName);
        }

        [Fact]
        public void SelectedId_Setter_SelectsMatchingItem()
        {
            var displayName = StaTestRunner.Run(() =>
            {
                var combo = new CatalogCombo();
                combo.SetOptions(Options());
                combo.SelectedId = "POST_A";
                return ((CatalogOption)combo.SelectedItem).DisplayName;
            });

            Assert.Equal("Poste A", displayName);
        }
    }
}
