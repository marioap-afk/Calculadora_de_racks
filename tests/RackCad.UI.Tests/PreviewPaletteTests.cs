using System.Windows.Media;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The shared preview palette: frozen brushes with the exact hex values already on screen, so adopters
    /// map to them without changing appearance. Frozen brushes are safe to read from any thread.</summary>
    public sealed class PreviewPaletteTests
    {
        [Fact]
        public void Structure_IsGreenAndFrozen()
        {
            var brush = Assert.IsType<SolidColorBrush>(PreviewPalette.Structure);

            Assert.True(brush.IsFrozen);
            Assert.Equal(Color.FromRgb(0x3D, 0xC9, 0x86), brush.Color);
        }

        [Theory]
        [InlineData("Structure", 0x3D, 0xC9, 0x86)]
        [InlineData("Beam", 0x5B, 0x8D, 0xEF)]
        [InlineData("Floor", 0x6A, 0x7B, 0x8A)]
        [InlineData("Label", 0x9A, 0xA7, 0xB4)]
        [InlineData("Warning", 0xFF, 0x6B, 0x6B)]
        [InlineData("Guide", 0xCF, 0xDB, 0xE8)]
        [InlineData("Accent", 0xE0, 0x8A, 0x2B)]
        [InlineData("Muted", 0xB7, 0xC3, 0xCF)]
        public void Named_ExposesEveryFrozenBrushByRole(string key, byte r, byte g, byte b)
        {
            Assert.True(PreviewPalette.Named.ContainsKey(key));
            var brush = Assert.IsType<SolidColorBrush>(PreviewPalette.Named[key]);

            Assert.True(brush.IsFrozen);
            Assert.Equal(Color.FromRgb(r, g, b), brush.Color);
        }

        [Fact]
        public void Named_HasEightRoles()
        {
            Assert.Equal(8, PreviewPalette.Named.Count);
        }
    }
}
