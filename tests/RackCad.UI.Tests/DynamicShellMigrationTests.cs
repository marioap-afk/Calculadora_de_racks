using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using RackCad.UI;
using RackCad.UI.Shell;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-30 migration (ADR-0019 accepted): STA + structural tests proving the REAL
    /// <see cref="RackDynamicSystemWindow"/> is now COMPOSED over <see cref="RackEditorVisualShell"/> and that the
    /// migration is behavior-preserving. These are the visual-adoption counterpart to <see cref="DynamicEditorWindowTests"/>
    /// (which already locks selection/multiselection/recompute/preview/insertion/GUID/persistence through the real
    /// handlers): here we assert the window's root IS the shell, every original control keeps its x:Name and lands in the
    /// correct neutral slot, no control was dropped, the disabled draw actions keep their reasons + show-on-disabled, the
    /// selection matrix and status render inside their slots, and the shell actually lays the window out (sidebar scroll,
    /// preview draw, resize). Assertions are structural/semantic — never screenshots or pixel comparisons.
    /// </summary>
    public sealed class DynamicShellMigrationTests
    {
        // Every x:Name defined in RackDynamicSystemWindow.xaml. The migration reparented these into the shell's slots;
        // none may be lost (the code-behind binds to all of them by name).
        private static readonly string[] AllNamedControls =
        {
            "Shell", "NameBox", "NumberFrontsCheck", "NumberLevelsCheck", "DrawRackNameCheck", "AnnotationScaleBox",
            "DimensionsBox", "DimStyleBox", "DepthBox", "PalletsDeepBox", "LoadLevelsBox", "PostPeralteBox", "PostBox",
            "FrontCountBox", "SelectedFrontText", "SelectedPositionsBox", "SelectedLevelsBox", "FirstLevelHeightBox",
            "SelectedPalletsDeepBox", "SelectedDepthStartBox", "FrontBox", "PalletHeightBox", "WeightBox",
            "SelectedClearHeightBox", "SelectedBeamLengthBox", "SelectedInOutBeamBox", "SelectedInOutPeralteBox",
            "SelectedIntermediateBeamBox", "SelectedIntermediatePeralteBox", "SelectedFrontInfo", "SafetyButton",
            "ComputedHeightText", "AdvancedToggle", "AdvancedPanel", "ManualHeightToggle", "ManualHeightBox",
            "BeamDepthBox", "SeparatorCountBox", "SeparatorSpacingBox", "DerivedReinforceBox", "DerivedReinforcementBox",
            "ModulesGrid", "SelectedInfoText", "KindBox", "ModuleLengthBox", "ConfigBox", "ApplyModuleButton",
            "EditHeaderButton", "SummaryText", "StatusText", "DynamicMatrixGrid", "PreviewLateralRadio",
            "PreviewExitRadio", "PreviewEntranceRadio", "PreviewLateralPostLabel", "PreviewLateralPostBox",
            "PreviewHint", "PreviewCanvas", "UpdateButton", "InsertLateralButton", "InsertExitButton",
            "InsertEntranceButton", "InsertPlantaButton"
        };

        // The five draw actions that carry a show-on-disabled reason (they gate on the AutoCAD origin / editing state).
        private static readonly string[] GatedDrawButtons =
        {
            "UpdateButton", "InsertLateralButton", "InsertExitButton", "InsertEntranceButton", "InsertPlantaButton"
        };

        // ---- 1. the window is composed over the shell ----

        [Fact]
        public void DynamicWindow_RootContentIsTheVisualShell()
        {
            var (contentIsShell, sameAsField) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                return (window.Content is RackEditorVisualShell, ReferenceEquals(window.Content, window.Shell));
            });

            Assert.True(contentIsShell);   // the window's root visual is the shared shell, not the old bespoke Grid
            Assert.True(sameAsField);       // and it is the x:Name="Shell" instance the code-behind/tests reference
        }

        // ---- 2. no control was dropped in the reparent ----

        [Fact]
        public void DynamicWindow_PreservesEveryNamedControl()
        {
            var missing = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                return AllNamedControls.Where(name => window.FindName(name) == null).ToArray();
            });

            Assert.Empty(missing); // all 63 named elements still resolve in the window's name scope after the migration
        }

        // ---- 3. each control lands in the correct neutral slot ----

        [Fact]
        public void DynamicWindow_PlacesEachControlInItsShellSlot()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var shell = window.Shell;

                // Sidebar: inputs + per-cell editor + module table live in the scrolling side panel.
                foreach (var name in new[] { "NameBox", "DepthBox", "PostBox", "FrontCountBox", "SelectedPositionsBox",
                    "FrontBox", "SafetyButton", "AdvancedPanel", "ModulesGrid", "ConfigBox", "SelectedFrontText" })
                {
                    AssertInSlot(shell.SidePanelContent, window, name, "SidePanelContent");
                }

                // Matrix: the front×level selection grid is the central editing surface.
                AssertInSlot(shell.MatrixContent, window, "DynamicMatrixGrid", "MatrixContent");

                // Preview: the linked-view canvas + its view selector.
                foreach (var name in new[] { "PreviewCanvas", "PreviewLateralRadio", "PreviewExitRadio",
                    "PreviewEntranceRadio", "PreviewLateralPostBox", "PreviewHint" })
                {
                    AssertInSlot(shell.PreviewContent, window, name, "PreviewContent");
                }

                // Status: summary + status line, in the always-visible band.
                AssertInSlot(shell.StatusContent, window, "SummaryText", "StatusContent");
                AssertInSlot(shell.StatusContent, window, "StatusText", "StatusContent");

                // Actions, by neutral category.
                Assert.Equal("Restaurar layout", ((Button)shell.LeadingActions).Content); // Leading = reset
                AssertContentButton(shell.SecondaryActions, "Exportar BOM (CSV)");         // Secondary = data/project
                AssertContentButton(shell.SecondaryActions, "Lista de materiales");
                AssertContentButton(shell.SecondaryActions, "Abrir proyecto");
                AssertContentButton(shell.SecondaryActions, "Guardar proyecto");
                foreach (var name in new[] { "UpdateButton", "InsertLateralButton", "InsertExitButton",
                    "InsertEntranceButton", "InsertPlantaButton" })
                {
                    AssertInSlot(shell.PrimaryActions, window, name, "PrimaryActions");     // Primary = draw/insert
                }
                Assert.Equal("Cerrar", ((Button)shell.TrailingActions).Content);           // Trailing = close
            });
        }

        // ---- 4. disabled draw actions keep their reason and show-on-disabled ----

        [Fact]
        public void DynamicWindow_DisabledDrawActions_KeepReasonAndShowOnDisabled()
        {
            StaTestRunner.Run(() =>
            {
                // Opened NOT from AutoCAD → every gated draw button is disabled, still shows its tooltip while disabled,
                // and carries the origin reason. The migration must not have dropped these (they moved into PrimaryActions).
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: false);
                foreach (var name in GatedDrawButtons)
                {
                    var button = (Button)window.FindName(name);
                    Assert.False(button.IsEnabled, $"{name} must be disabled when not opened from AutoCAD");
                    Assert.True(ToolTipService.GetShowOnDisabled(button), $"{name} must keep ShowOnDisabled");
                    Assert.Equal("Disponible solo cuando la ventana se abre desde AutoCAD.", button.ToolTip);
                }
            });
        }

        [Fact]
        public void DynamicWindow_InsertLateralEnabled_WhenOpenedFromAutoCad_WithItsRealTooltip()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var insert = (Button)window.FindName("InsertLateralButton");
                Assert.True(insert.IsEnabled); // lateral is the entry point: enabled as soon as the window comes from AutoCAD
                Assert.True(ToolTipService.GetShowOnDisabled(insert));
                Assert.Equal("Pide el número de poste e inserta ese corte lateral enlazado al sistema.", insert.ToolTip);
            });
        }

        // ---- 5. the selection matrix and status render inside their slots (recompute wiring intact) ----

        [Fact]
        public void DynamicWindow_SelectionMatrixAndStatus_RenderInsideTheShellSlots()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);

                // The ctor runs RenderFrontMatrix()+Recompose(): the matrix grid (in MatrixContent) is populated and the
                // status band (in StatusContent) shows the recomputed summary — proving the pipeline still targets the
                // shell-hosted controls.
                Assert.True(window.DynamicMatrixGrid.Children.Count > 0, "the selection matrix must render into the slot");
                Assert.True(window.DynamicMatrixGrid.ColumnDefinitions.Count >= 2, "matrix has a label column + a front column");
                Assert.False(string.IsNullOrWhiteSpace(window.SummaryText.Text), "recompute must fill the status summary");
            });
        }

        [Fact]
        public void DynamicWindow_MatrixCellClick_ChangesSelection_ThroughTheShellHostedGrid()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                // Grow to 2 fronts so there is a non-default cell to select, then click a cell Border in the shell-hosted
                // grid and confirm the selection label updates — the real SelectCell handler still fires post-migration.
                SetFrontCount(window, 2);
                var before = window.SelectedFrontText.Text;
                var cell = FirstSelectableCell(window.DynamicMatrixGrid);
                Assert.NotNull(cell);
                cell.RaiseEvent(new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                    Source = cell
                });
                Assert.False(string.IsNullOrWhiteSpace(window.SelectedFrontText.Text));
                Assert.Contains("Celda", window.SelectedFrontText.Text); // the label reflects a selected cell
            });
        }

        // ---- 6. the shell lays the window out: sidebar scroll, preview draw, resize ----

        [Fact]
        public void DynamicWindow_ShellLaysOut_SidebarScrolls_PreviewDraws_AndResizes()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var shell = window.Shell;

                // Give the shell the REAL default template (Themes/Generic.xaml) and lay it out, exactly as a shown window
                // would. (A control disconnected from a shown HWND does not auto-resolve its theme style.)
                var generic = new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/Generic.xaml", UriKind.Relative) };
                shell.Style = (Style)generic[typeof(RackEditorVisualShell)];
                shell.Measure(new Size(1300, 700));
                shell.Arrange(new Rect(0, 0, 1300, 700));
                shell.UpdateLayout();

                // Sidebar scroll: the dynamic side panel is wrapped in the shell's vertical-only scroll.
                var scroll = shell.SidebarScroll;
                Assert.NotNull(scroll);
                Assert.Equal(System.Windows.Controls.ScrollBarVisibility.Auto, scroll.VerticalScrollBarVisibility);
                Assert.Equal(System.Windows.Controls.ScrollBarVisibility.Disabled, scroll.HorizontalScrollBarVisibility);

                // The slots host exactly the window's content.
                Assert.Same(shell.MatrixContent, shell.MatrixHost.Content);
                Assert.Same(shell.PreviewContent, shell.PreviewHost.Content);
                Assert.Same(shell.StatusContent, shell.StatusHost.Content);

                // Preview draws when the canvas gets a real size (its SizeChanged runs DrawSideView over the recomposed model).
                Assert.True(window.PreviewCanvas.ActualWidth > 0.0, "preview canvas must receive a real width from the shell");
                Assert.True(window.PreviewCanvas.Children.Count > 0, "preview must draw once it has a size");

                // Resize larger: the shell re-arranges and the preview stays drawn (no layout break).
                shell.Measure(new Size(1500, 900));
                shell.Arrange(new Rect(0, 0, 1500, 900));
                shell.UpdateLayout();
                Assert.True(window.PreviewCanvas.ActualWidth > 0.0);
                Assert.True(window.PreviewCanvas.Children.Count > 0);
            });
        }

        // ---- helpers ----

        private static void SetFrontCount(RackDynamicSystemWindow window, int count)
        {
            var box = (TextBox)window.FindName("FrontCountBox");
            box.Text = count.ToString(System.Globalization.CultureInfo.InvariantCulture);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box)); // the real FrontCount_Changed handler
        }

        private static Border FirstSelectableCell(Grid grid)
            => grid.Children.OfType<Border>().FirstOrDefault(b => Grid.GetRow(b) >= 1 && Grid.GetColumn(b) >= 1);

        private static void AssertInSlot(object slotContent, RackDynamicSystemWindow window, string name, string slotName)
        {
            var target = window.FindName(name) as DependencyObject;
            Assert.True(target != null, $"named control {name} not found");
            Assert.True(slotContent is DependencyObject, $"slot {slotName} has no content");
            Assert.True(IsInLogicalSubtree((DependencyObject)slotContent, target),
                $"{name} is not hosted inside the {slotName} slot");
        }

        private static void AssertContentButton(object slotContent, string content)
        {
            Assert.True(slotContent is DependencyObject, "action slot has no content");
            var found = FindButtonByContent((DependencyObject)slotContent, content);
            Assert.True(found != null, $"no button '{content}' in the action slot");
        }

        private static bool IsInLogicalSubtree(DependencyObject root, DependencyObject target)
        {
            if (ReferenceEquals(root, target)) return true;
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is DependencyObject node && IsInLogicalSubtree(node, target)) return true;
            }
            return false;
        }

        private static Button FindButtonByContent(DependencyObject root, string content)
        {
            if (root is Button b && (b.Content as string) == content) return b;
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is DependencyObject node)
                {
                    var found = FindButtonByContent(node, content);
                    if (found != null) return found;
                }
            }
            return null;
        }
    }
}
