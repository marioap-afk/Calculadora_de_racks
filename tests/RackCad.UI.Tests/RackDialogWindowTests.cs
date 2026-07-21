using System.Windows;
using System.Windows.Media;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The <see cref="RackDialogWindow"/> base (STA): shared chrome and the standard action bar. A tiny
    /// subclass exposes the protected <see cref="RackDialogWindow.CreateActionBar"/> for assertion.</summary>
    public sealed class RackDialogWindowTests
    {
        private sealed class TestDialogWindow : RackDialogWindow
        {
            public DialogActionBar BuildActionBar(string accept = "Aceptar", string cancel = "Cancelar")
                => CreateActionBar(accept, cancel);
        }

        [Fact]
        public void Constructor_AppliesSharedChrome()
        {
            var (font, startup) = StaTestRunner.Run(() =>
            {
                var window = new TestDialogWindow();
                return (window.FontFamily.Source, window.WindowStartupLocation);
            });

            Assert.Equal("Segoe UI", font);
            Assert.Equal(WindowStartupLocation.CenterOwner, startup);
        }

        [Fact]
        public void Constructor_MergesSharedStyles_AndSetsWindowBackground()
        {
            // The window's Background brush is a resource owned by the STA dispatcher and is not frozen, so its
            // Color must be read on that thread; returning the struct keeps the assertion thread-safe.
            var (hasBrush, color) = StaTestRunner.Run(() =>
            {
                var window = new TestDialogWindow();
                // The shared brush is only resolvable if AppStyles.xaml merged from the pack URI.
                var brush = window.Background as SolidColorBrush;
                return (brush != null, brush?.Color ?? default);
            });

            Assert.True(hasBrush);
            Assert.Equal(Color.FromRgb(0xF4, 0xF6, 0xF9), color);
        }

        [Fact]
        public void CreateActionBar_ProducesDefaultAndCancelButtons()
        {
            var (acceptText, isDefault, cancelText, isCancel, hasPanel) = StaTestRunner.Run(() =>
            {
                var window = new TestDialogWindow();
                var bar = window.BuildActionBar();
                return (bar.AcceptButton.Content as string, bar.AcceptButton.IsDefault,
                        bar.CancelButton.Content as string, bar.CancelButton.IsCancel,
                        bar.Panel != null);
            });

            Assert.Equal("Aceptar", acceptText);
            Assert.True(isDefault);
            Assert.Equal("Cancelar", cancelText);
            Assert.True(isCancel);
            Assert.True(hasPanel);
        }

        [Fact]
        public void CreateActionBar_HonorsCustomLabels()
        {
            var (acceptText, cancelText) = StaTestRunner.Run(() =>
            {
                var window = new TestDialogWindow();
                var bar = window.BuildActionBar("Guardar", "Descartar");
                return (bar.AcceptButton.Content as string, bar.CancelButton.Content as string);
            });

            Assert.Equal("Guardar", acceptText);
            Assert.Equal("Descartar", cancelText);
        }
    }
}
