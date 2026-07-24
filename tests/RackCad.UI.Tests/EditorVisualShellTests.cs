using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.UI.Shell;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-30 foundation (ADR-0019 accepted): STA + structural/semantic tests for <see cref="RackEditorVisualShell"/>,
    /// <see cref="EditorStatusPresenter"/>, <see cref="EditorActionBar"/> and the AppStyles tokens. No window is
    /// migrated here. Assertions are structural/semantic — never screenshots or pixel comparisons.
    /// </summary>
    public sealed class EditorVisualShellTests
    {
        // ---- helpers ----

        private static RackEditorVisualShell Measured(double w, double h, Action<RackEditorVisualShell> setup = null)
        {
            var shell = new RackEditorVisualShell();
            setup?.Invoke(shell);
            shell.Measure(new Size(w, h));
            shell.Arrange(new Rect(0, 0, w, h));
            shell.UpdateLayout();
            return shell;
        }

        private static ContentPresenter Host(RackEditorVisualShell shell, string name) => (ContentPresenter)shell.FindName(name);

        private static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject root)
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

        private static string ShellSource(string relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            var path = Path.Combine(dir.FullName, "src", "RackCad.UI", "Shell", relative);
            Assert.True(File.Exists(path), $"missing shell source {path}");
            return File.ReadAllText(path);
        }

        private static ResourceDictionary AppStyles()
            => new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) };

        // ---- 1. instantiation ----

        [Fact]
        public void Shell_Instantiates()
            => StaTestRunner.Run(() => Assert.NotNull(new RackEditorVisualShell()));

        // ---- 2. each slot receives a single content ----

        [Fact]
        public void EachSlot_HostsExactlyTheInjectedContent()
        {
            StaTestRunner.Run(() =>
            {
                var header = new TextBlock { Text = "id" };
                var side = new StackPanel();
                var matrix = new Border();
                var preview = new Border();
                var status = new EditorStatusPresenter();
                var shell = Measured(1280, 720, s =>
                {
                    s.SidebarHeader = header;
                    s.SidePanelContent = side;
                    s.MatrixContent = matrix;
                    s.PreviewContent = preview;
                    s.StatusContent = status;
                });

                Assert.Same(header, Host(shell, "SidebarHeaderHost").Content);
                Assert.Same(side, Host(shell, "SidePanelHost").Content);
                Assert.Same(matrix, Host(shell, "MatrixHost").Content);
                Assert.Same(preview, Host(shell, "PreviewHost").Content);
                Assert.Same(status, Host(shell, "StatusHost").Content);
            });
        }

        // ---- 3. empty SidebarHeader collapses ----

        [Fact]
        public void EmptySidebarHeader_Collapses()
        {
            StaTestRunner.Run(() =>
            {
                var shell = Measured(1280, 720); // no header set
                Assert.Equal(Visibility.Collapsed, Host(shell, "SidebarHeaderHost").Visibility);
            });
        }

        // ---- 4. empty MatrixContent collapses and the preview fills ----

        [Fact]
        public void EmptyMatrix_Collapses_AndPreviewTakesTheArea()
        {
            StaTestRunner.Run(() =>
            {
                var shell = Measured(1280, 720, s => s.PreviewContent = new Border());
                var matrix = Host(shell, "MatrixHost");
                var preview = Host(shell, "PreviewHost");

                Assert.Equal(Visibility.Collapsed, matrix.Visibility);
                Assert.Equal(0.0, matrix.ActualHeight);
                Assert.True(preview.ActualHeight > 200.0, $"preview should fill; got {preview.ActualHeight}");
            });
        }

        // ---- 5. jagged matrix content is hosted without the shell assuming a rectangular shape ----

        [Fact]
        public void JaggedMatrixContent_IsHostedAsIs()
        {
            StaTestRunner.Run(() =>
            {
                // A deliberately non-rectangular content (rows of different lengths). The shell must host it verbatim.
                var jagged = new StackPanel();
                jagged.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Children = { new Border { Width = 20, Height = 20 }, new Border { Width = 20, Height = 20 }, new Border { Width = 20, Height = 20 } } });
                jagged.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Children = { new Border { Width = 20, Height = 20 } } });

                var shell = Measured(1280, 720, s => s.MatrixContent = jagged);
                var host = Host(shell, "MatrixHost");
                Assert.Same(jagged, host.Content);
                Assert.Equal(Visibility.Visible, host.Visibility);
                Assert.True(host.ActualHeight > 0.0);   // measured to the jagged content's own size, not forced
            });
        }

        // ---- 6. empty preview does not throw ----

        [Fact]
        public void EmptyPreview_DoesNotThrow()
        {
            StaTestRunner.Run(() =>
            {
                var shell = Measured(1280, 720); // no preview set
                Assert.Equal(Visibility.Collapsed, Host(shell, "PreviewHost").Visibility);
            });
        }

        // ---- 7. empty status collapses ----

        [Fact]
        public void EmptyStatus_Collapses()
        {
            StaTestRunner.Run(() =>
            {
                var shell = Measured(1280, 720);
                Assert.Equal(Visibility.Collapsed, Host(shell, "StatusHost").Visibility);
            });
        }

        // ---- 8. the four severities are distinguishable ----

        [Fact]
        public void StatusPresenter_RendersTheFourSeveritiesDistinguishably()
        {
            StaTestRunner.Run(() =>
            {
                var presenter = new EditorStatusPresenter();
                Color ColorFor(EditorStatusSeverity sev)
                {
                    presenter.Message = new EditorStatusMessage("x", sev);
                    return ((SolidColorBrush)((TextBlock)presenter.Content).Foreground).Color;
                }

                var colors = new[]
                {
                    ColorFor(EditorStatusSeverity.Info),
                    ColorFor(EditorStatusSeverity.Success),
                    ColorFor(EditorStatusSeverity.Warning),
                    ColorFor(EditorStatusSeverity.Error)
                };
                Assert.Equal(4, colors.Distinct().Count());   // all four distinct

                // And an empty message collapses the presenter.
                presenter.Message = EditorStatusMessage.None;
                Assert.Equal(Visibility.Collapsed, presenter.Visibility);
            });
        }

        // ---- 9. the four action categories keep order and content ----

        [Fact]
        public void ActionBar_KeepsTheFourCategoriesInOrderWithTheirContent()
        {
            StaTestRunner.Run(() =>
            {
                var bar = new EditorActionBar();
                var leading = new Button { Content = "L" };
                var secondary = new Button { Content = "S" };
                var primary = new Button { Content = "P" };
                var trailing = new Button { Content = "T" };
                bar.LeadingActions = leading;
                bar.SecondaryActions = secondary;
                bar.PrimaryActions = primary;
                bar.TrailingActions = trailing;

                var presenters = bar.CategoryPresenters;
                Assert.Equal(4, presenters.Count);
                Assert.Same(leading, presenters[0].Content);
                Assert.Same(secondary, presenters[1].Content);
                Assert.Same(primary, presenters[2].Content);
                Assert.Same(trailing, presenters[3].Content);
                Assert.All(presenters, p => Assert.Equal(Visibility.Visible, p.Visibility));
            });
        }

        // ---- 10. optional categories leave no gap ----

        [Fact]
        public void ActionBar_OptionalCategoriesCollapse_NoGap()
        {
            StaTestRunner.Run(() =>
            {
                var bar = new EditorActionBar { PrimaryActions = new Button { Content = "P" } };
                var presenters = bar.CategoryPresenters;
                Assert.Equal(Visibility.Collapsed, presenters[0].Visibility); // leading
                Assert.Equal(Visibility.Collapsed, presenters[1].Visibility); // secondary
                Assert.Equal(Visibility.Visible, presenters[2].Visibility);   // primary
                Assert.Equal(Visibility.Collapsed, presenters[3].Visibility); // trailing
            });
        }

        // ---- 11. a disabled action keeps a visible tooltip ----

        [Fact]
        public void DisabledAction_KeepsAVisibleTooltip()
        {
            StaTestRunner.Run(() =>
            {
                var button = EditorActions.Button(new EditorAction("Insertar", isEnabled: false, disabledReason: "Corrige los campos."));
                Assert.False(button.IsEnabled);
                Assert.True(ToolTipService.GetShowOnDisabled(button));
                Assert.Equal("Corrige los campos.", button.ToolTip);
            });
        }

        [Fact]
        public void PrimaryAndSecondaryActions_GetDifferentStyles()
        {
            StaTestRunner.Run(() =>
            {
                var primary = EditorActions.Button(new EditorAction("Insertar", isPrimary: true));
                var secondary = EditorActions.Button(new EditorAction("Cerrar", isPrimary: false));
                Assert.NotNull(primary.Style);
                Assert.NotNull(secondary.Style);
                Assert.NotSame(primary.Style, secondary.Style);   // differentiated
            });
        }

        // ---- 12. Measure/Arrange at initial and minimum size ----

        [Fact]
        public void Shell_MeasuresAndArranges_AtInitialAndMinimumSize()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (w, h) in new[] { (1280.0, 720.0), (1120.0, 640.0) })
                {
                    var shell = Measured(w, h, s => { s.SidePanelContent = new StackPanel(); s.PreviewContent = new Border(); });
                    Assert.True(shell.ActualWidth > 0 && shell.ActualHeight > 0, $"failed at {w}x{h}");
                }
            });
        }

        // ---- 13. the sidebar keeps its scroll ----

        [Fact]
        public void Sidebar_HasAVerticalScroll()
        {
            StaTestRunner.Run(() =>
            {
                var shell = Measured(1120, 640, s => s.SidePanelContent = new StackPanel());
                var scroll = Descendants(shell).OfType<ScrollViewer>().Single();
                Assert.Equal(ScrollBarVisibility.Auto, scroll.VerticalScrollBarVisibility);
                Assert.Equal(ScrollBarVisibility.Disabled, scroll.HorizontalScrollBarVisibility);
            });
        }

        // ---- 14. the action bar does not clip its actions at the minimum ----

        [Fact]
        public void ActionBar_DoesNotClipItsActionsAtMinimum()
        {
            StaTestRunner.Run(() =>
            {
                var bar = new EditorActionBar
                {
                    LeadingActions = new Button { Content = "Restaurar", Width = 120, Height = 30 },
                    SecondaryActions = new Button { Content = "BOM", Width = 120, Height = 30 },
                    PrimaryActions = new Button { Content = "Actualizar", Width = 120, Height = 30 },
                    TrailingActions = new Button { Content = "Cerrar", Width = 120, Height = 30 }
                };

                bar.Measure(new Size(200, double.PositiveInfinity)); // narrower than the four buttons in a row
                bar.Arrange(new Rect(0, 0, 200, bar.DesiredSize.Height));
                bar.UpdateLayout();

                // WrapPanel flows to new lines instead of clipping: each category keeps its full desired width and the
                // bar grew taller than a single row.
                Assert.All(bar.CategoryPresenters, p => Assert.True(p.DesiredSize.Width >= 120.0, $"category clipped: {p.DesiredSize.Width}"));
                Assert.True(bar.DesiredSize.Height > 30.0, "the bar should have wrapped to more than one row");
            });
        }

        // ---- 15. the required tokens resolve from AppStyles.xaml ----

        [Fact]
        public void RequiredTokens_ResolveFromAppStyles()
        {
            StaTestRunner.Run(() =>
            {
                var d = AppStyles();
                foreach (var key in new[]
                {
                    "ShellWindowBackgroundBrush", "ShellSurfaceBrush", "ShellBorderBrush",
                    "ShellTextPrimaryBrush", "ShellTextSecondaryBrush", "ShellPreviewBackgroundBrush",
                    "ShellSelectionPrimaryBrush", "ShellSelectionIncludedBrush",
                    "ShellStatusInfoBrush", "ShellStatusSuccessBrush", "ShellStatusWarningBrush", "ShellStatusErrorBrush"
                })
                {
                    Assert.True(d.Contains(key), $"missing token {key}");
                    Assert.IsAssignableFrom<Brush>(d[key]);
                }

                foreach (var key in new[]
                {
                    "ShellSidebarWidth", "ShellPreviewMinHeight", "ShellFontSizeBase",
                    "ShellInitialWidth", "ShellInitialHeight", "ShellMinWidth", "ShellMinHeight", "ShellActionBarSpacing"
                })
                {
                    Assert.True(d.Contains(key), $"missing token {key}");
                    Assert.IsType<double>(d[key]);
                }

                foreach (var key in new[] { "ShellZoneSpacing", "ShellPanelPadding", "ShellVerticalRhythm", "ShellActionBarPadding" })
                {
                    Assert.True(d.Contains(key), $"missing token {key}");
                    Assert.IsType<Thickness>(d[key]);
                }

                // The existing keys survive.
                Assert.True(d.Contains("WindowBackgroundBrush"));
                Assert.True(d.Contains("PrimaryButtonStyle"));
                Assert.True(d.Contains("SecondaryButtonStyle"));
            });
        }

        // ---- 16. zero RackSystemKind in the shell files ----

        [Fact]
        public void ShellFiles_ReferenceNoRackSystemKind()
        {
            foreach (var file in new[]
            {
                "RackEditorVisualShell.xaml", "RackEditorVisualShell.xaml.cs", "EditorActionBar.cs",
                "EditorAction.cs", "EditorStatus.cs", "EditorStatusPresenter.cs", "ShellResources.cs",
                "ShellSlotVisibilityConverter.cs"
            })
            {
                Assert.DoesNotContain("RackSystemKind", ShellSource(file));
            }
        }

        // ---- 17. zero per-system coupling in the shell files ----

        [Fact]
        public void ShellFiles_HaveNoPerSystemCoupling()
        {
            // "DynamicResource"/"DependencyProperty" legitimately contain "Dynamic"; target the actual system couplings.
            var forbidden = new[]
            {
                "Selective", "PushBack", "FlowBed",
                "RackDynamic", "DynamicSystem", "DynamicFront", "DynamicEditor", "DynamicRack"
            };
            foreach (var file in new[]
            {
                "RackEditorVisualShell.xaml", "RackEditorVisualShell.xaml.cs", "EditorActionBar.cs",
                "EditorAction.cs", "EditorStatus.cs", "EditorStatusPresenter.cs", "ShellResources.cs",
                "ShellSlotVisibilityConverter.cs"
            })
            {
                var src = ShellSource(file);
                foreach (var token in forbidden)
                {
                    Assert.DoesNotContain(token, src);
                }
            }
        }

        // ---- 18. the foundation does not depend on AutoCAD ----

        [Fact]
        public void Foundation_DoesNotDependOnAutoCad()
        {
            var referenced = typeof(RackEditorVisualShell).Assembly.GetReferencedAssemblies().Select(a => a.Name);
            Assert.DoesNotContain("AcMgd", referenced);
            Assert.DoesNotContain("AcDbMgd", referenced);
            Assert.DoesNotContain("AcCoreMgd", referenced);
            Assert.All(referenced, name => Assert.DoesNotContain("Autodesk", name ?? string.Empty));
        }

        // ---- the shell forwards its action DPs into the inner action bar ----

        [Fact]
        public void Shell_ForwardsActionCategoriesToItsActionBar()
        {
            StaTestRunner.Run(() =>
            {
                var primary = new Button { Content = "Insertar" };
                var shell = Measured(1280, 720, s => s.PrimaryActions = primary);
                var bar = (EditorActionBar)shell.FindName("ActionBar");
                Assert.Same(primary, bar.PrimaryActions);
                Assert.Same(primary, bar.CategoryPresenters[2].Content);
            });
        }
    }
}
