using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using RackCad.UI;
using RackCad.UI.Shell;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-18b — visual adoption of RackEditorVisualShell. The Push Back editor consumes the COMMON visual contract like the
    /// dynamic (I-30) and selective (I-31) editors: action-button styles, shell surface/border/preview tokens, and the
    /// status band (summary + messages) in the shell's StatusHost — never a second status inside the sidebar — while
    /// keeping every x:Name, handler, matrix, preview and action. Structural/semantic STA assertions only; no pixels.
    /// </summary>
    public sealed class PushBackShellAdoptionTests
    {
        private static T Named<T>(DependencyObject root, string name) where T : FrameworkElement
            => Descendants(root).OfType<T>().FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.Ordinal));

        private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            if (root == null) yield break;
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                foreach (var nested in Descendants(child)) yield return nested;
            }
        }

        private static bool IsUnder(DependencyObject ancestor, DependencyObject node)
        {
            for (var p = node; p != null; p = VisualTreeHelper.GetParent(p))
            {
                if (ReferenceEquals(p, ancestor)) return true;
            }
            return false;
        }

        private static Color ColorOf(Brush brush) => ((SolidColorBrush)brush).Color;

        private static RackPushBackSystemWindow Shown()
        {
            var w = new RackPushBackSystemWindow
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                Left = -10000,
                Top = -10000,
            };
            w.Show();
            w.UpdateLayout();
            return w;
        }

        [Fact]
        public void ActionButtons_UseTheCommonStyles_AndShowReasonsWhenDisabled()
        {
            StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow();
                try
                {
                    var primary = w.FindResource("PrimaryButtonStyle");
                    var secondary = w.FindResource("SecondaryButtonStyle");
                    Button B(string n) => (Button)w.FindName(n);

                    // Actualizar + the primary insertion action = PrimaryButtonStyle.
                    Assert.Same(primary, B("UpdateButton").Style);
                    Assert.Same(primary, B("InsertButton").Style);
                    // Restaurar, BOM, biblioteca, the secondary view inserts and Cerrar = SecondaryButtonStyle.
                    foreach (var n in new[] { "RestoreButton", "BomButton", "SaveLibraryButton", "InsertLateralButton",
                        "InsertFrontalEntradaButton", "InsertFrontalPosteriorButton", "InsertPlantaButton", "CloseButton" })
                    {
                        Assert.Same(secondary, B(n).Style);
                    }

                    // ToolTipService.ShowOnDisabled wherever the code-behind provides an unavailability reason (every gated
                    // data/draw action; Cerrar is always enabled and has no reason).
                    foreach (var n in new[] { "RestoreButton", "BomButton", "SaveLibraryButton", "UpdateButton", "InsertLateralButton",
                        "InsertFrontalEntradaButton", "InsertFrontalPosteriorButton", "InsertPlantaButton", "InsertButton" })
                    {
                        Assert.True(ToolTipService.GetShowOnDisabled(B(n)), $"{n} must show its reason when disabled");
                    }
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Surfaces_AndTheDarkPreview_AreFedByShellTokens()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var surface = ColorOf((Brush)w.FindResource("ShellSurfaceBrush"));
                    var border = ColorOf((Brush)w.FindResource("ShellBorderBrush"));
                    var previewBg = ColorOf((Brush)w.FindResource("ShellPreviewBackgroundBrush"));

                    var shell = (RackEditorVisualShell)w.Content;
                    var sidebar = (Border)shell.SidePanelContent;
                    Assert.Equal(surface, ColorOf(sidebar.Background));
                    Assert.Equal(border, ColorOf(sidebar.BorderBrush));

                    Assert.Equal(surface, ColorOf(Named<Border>(w, "MatrixZone").Background));
                    Assert.Equal(surface, ColorOf(Named<Border>(w, "PreviewZone").Background));

                    // The technical preview now sits on the shared DARK surface — the hardcoded light #F7FAFC is gone.
                    var canvasBg = ColorOf(Named<Canvas>(w, "PreviewCanvas").Background);
                    Assert.Equal(previewBg, canvasBg);
                    Assert.NotEqual((Color)ColorConverter.ConvertFromString("#F7FAFC"), canvasBg);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void SummaryHintAndStatus_LiveInStatusHost_NotInSidebar()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var shell = (RackEditorVisualShell)w.Content;
                    Assert.NotNull(shell.StatusHost);
                    Assert.NotNull(shell.SidebarScroll);

                    foreach (var n in new[] { "PreviewSummary", "PreviewHint", "StatusText" })
                    {
                        var tb = Named<TextBlock>(w, n);
                        Assert.True(tb != null, $"missing status TextBlock {n}");
                        Assert.True(IsUnder(shell.StatusHost, tb), $"{n} must live in the shell StatusHost");
                        Assert.False(IsUnder(shell.SidebarScroll, tb), $"{n} must NOT be a second status inside the sidebar");
                    }
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void SizeContract_MatchesTheMigratedEditors_SidebarScrolls_ActionBarPresent()
        {
            StaTestRunner.Run(() =>
            {
                var tokens = new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) };
                var minW = (double)tokens["ShellMinWidth"];
                var minH = (double)tokens["ShellMinHeight"];

                var w = new RackPushBackSystemWindow();
                try
                {
                    // Same size contract as the dynamic/selective editors: initial + minimum from the shared tokens.
                    Assert.Equal(minW, w.MinWidth);
                    Assert.Equal(minH, w.MinHeight);
                    Assert.Equal((double)tokens["ShellInitialWidth"], w.Width);
                    Assert.Equal((double)tokens["ShellInitialHeight"], w.Height);

                    var shell = (RackEditorVisualShell)w.Content;
                    shell.Style = (Style)new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/Generic.xaml", UriKind.Relative) }[typeof(RackEditorVisualShell)];
                    shell.Measure(new Size(minW, minH));
                    shell.Arrange(new Rect(0, 0, minW, minH));
                    shell.UpdateLayout();

                    var scroll = shell.SidebarScroll;
                    Assert.NotNull(scroll);
                    Assert.Equal(ScrollBarVisibility.Auto, scroll.VerticalScrollBarVisibility);
                    Assert.Equal(ScrollBarVisibility.Disabled, scroll.HorizontalScrollBarVisibility);
                    Assert.True(scroll.ScrollableHeight > 0.0, "the sidebar must scroll at the minimum size, not clip");

                    var bar = shell.ActionBar;
                    Assert.NotNull(bar);
                    Assert.True(bar.ActualHeight > 0.0);
                    var buttons = Descendants(bar).OfType<Button>().ToList();
                    Assert.True(buttons.Count >= 10, $"the whole action bar must be present; got {buttons.Count}");
                    Assert.All(buttons, b => Assert.True(b.ActualWidth > 0.0, $"action '{b.Content}' is clipped"));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void VisualAdoption_PreservesXNames_Matrix_Preview_AndActions()
        {
            StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow();
                try
                {
                    // Controls, matrix, preview and actions survive the visual adoption (window name scope intact).
                    foreach (var n in new[] { "NameBox", "GuidText", "FrontCountBox", "PostBox", "PositionsBox", "RearPeralteBox",
                        "RearTopeActiveCheck", "SafetyButton", "PushBackMatrixGrid", "CellSelectionMatrix", "TopeMatrix",
                        "PreviewCanvas", "ViewBox", "LateralSectionBox", "PreviewSummary", "PreviewHint", "StatusText",
                        "RestoreButton", "BomButton", "SaveLibraryButton", "UpdateButton", "InsertLateralButton",
                        "InsertFrontalEntradaButton", "InsertFrontalPosteriorButton", "InsertPlantaButton", "InsertButton",
                        "CloseButton", "ApplyCellButton", "ApplyAllButton" })
                    {
                        Assert.True(w.FindName(n) != null, $"missing {n} after the visual adoption");
                    }
                }
                finally { w.Close(); }
            });
        }
    }
}
