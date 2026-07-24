using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.UI;
using RackCad.UI.Controls;
using RackCad.UI.Shell;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// PB-VAL-01 (owner retest) — the INTERNAL composition of the Push Back editor follows the structural pattern of the
    /// dynamic editor: the whole "Celda seleccionada" editor and its "Aplicar a" scopes live in the SIDEBAR, so the matrix
    /// zone is left with ONLY the toolbar and the cards; and the sidebar captures its fields in
    /// vertical sections of one/two-column grids (the dense WrapPanels are gone) with full-width controls. Structural STA
    /// assertions over the REAL window — no pixels, no screenshots.
    /// </summary>
    public sealed class PushBackSidebarCompositionTests
    {
        /// <summary>Every control of the selected-cell editor, including its scope buttons.</summary>
        private static readonly string[] CellEditorControls =
        {
            "CellSelectionPrimaryText",
            "PositionsBox", "LevelsBox", "FondosBox", "DepthStartBox", "FirstLevelHeightBox", "CellClearBox",
            "CellPalletFrontBox", "CellPalletHeightBox", "CellPalletWeightBox", "CellBeamLengthOverrideBox",
            "CellInOutBeamBox", "CellInOutPeralteBox", "CellIntermediateBeamBox", "CellIntermediatePeralteBox",
            "RearPeralteBox",
            "ApplyCellButton", "ApplySelectedButton", "ApplyLevelButton", "ApplyFrontButton", "ApplyAllButton",
        };

        /// <summary>What the matrix zone is allowed to keep: the structural toolbar and the cards. Owner decision
        /// (2026-07-24): no bulk tope tools, no tope hints, no tope marking.</summary>
        private static readonly string[] MatrixControls =
        {
            "AddFrontButton", "RemoveFrontButton", "AddLevelButton", "RemoveLevelButton",
            "SelectedFrontBox", "SelectedLevelBox", "PushBackMatrixGrid",
        };

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
        public void SelectedCellEditor_AndItsApplyScopes_LiveInTheSidebar_NotInTheMatrix()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var shell = (RackEditorVisualShell)w.Content;
                    var matrixZone = (Border)w.FindName("MatrixZone");
                    Assert.NotNull(shell.SidebarScroll);
                    Assert.NotNull(matrixZone);

                    foreach (var name in CellEditorControls)
                    {
                        var element = w.FindName(name) as FrameworkElement;
                        Assert.True(element != null, $"missing cell-editor control {name}");
                        Assert.True(IsUnder(shell.SidebarScroll, element), $"{name} must live in the sidebar (dynamic-editor pattern)");
                        Assert.False(IsUnder(matrixZone, element), $"{name} must NOT sit in the matrix zone anymore");
                    }
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void MatrixZone_KeepsOnlyToolbarAndCards()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var matrixZone = (Border)w.FindName("MatrixZone");

                    // Everything the matrix is allowed to keep is still there.
                    foreach (var name in MatrixControls)
                    {
                        var element = w.FindName(name) as FrameworkElement;
                        Assert.True(element != null, $"missing matrix control {name}");
                        Assert.True(IsUnder(matrixZone, element), $"{name} must stay in the matrix zone");
                    }

                    // And nothing else: no data-capture field survives in the matrix. The toolbar's two combos
                    // (SelectedFrontBox/SelectedLevelBox) are navigation, not cell data, so they are exempt.
                    Assert.Empty(Descendants(matrixZone).OfType<NumericField>());
                    Assert.Empty(Descendants(matrixZone).OfType<CatalogCombo>());
                    Assert.Empty(Descendants(matrixZone).OfType<TextBox>());

                    Assert.Empty(Descendants(matrixZone).OfType<SelectionMatrix>());
                    Assert.Empty(Descendants(matrixZone).OfType<Expander>());

                    var navigation = new[] { "SelectedFrontBox", "SelectedLevelBox" };
                    Assert.All(
                        Descendants(matrixZone).OfType<ComboBox>(),
                        combo => Assert.Contains(combo.Name, navigation));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Sidebar_CapturesFieldsInVerticalSections_NoDenseWrapPanels()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var shell = (RackEditorVisualShell)w.Content;

                    // The dense WrapPanels that crammed the fields are gone: the only WrapPanels left in the sidebar are
                    // the two BUTTON rows (front scopes and cell scopes), exactly like the dynamic editor.
                    var wrapPanels = Descendants(shell.SidebarScroll).OfType<WrapPanel>().ToList();
                    Assert.True(wrapPanels.Count <= 2, $"the sidebar must not cram fields into WrapPanels; found {wrapPanels.Count}");
                    Assert.All(wrapPanels, panel => Assert.All(
                        panel.Children.OfType<FrameworkElement>(),
                        child => Assert.IsType<Button>(child)));

                    // Fields are captured full-width inside their section/column instead of at hardcoded widths.
                    foreach (var name in new[] { "PositionsBox", "LevelsBox", "CellPalletFrontBox", "FrontCountBox", "ToleranceBox" })
                    {
                        var field = (NumericField)w.FindName(name);
                        Assert.True(double.IsNaN(field.Width), $"{name} must stretch inside its column, not use a fixed width");
                        Assert.True(field.ActualWidth >= 90.0, $"{name} is too narrow: {field.ActualWidth}");
                    }

                    // Two-column sections use the dynamic editor's 10px gutter.
                    var gutters = Descendants(shell.SidebarScroll).OfType<Grid>()
                        .Where(g => g.ColumnDefinitions.Count == 3)
                        .ToList();
                    Assert.True(gutters.Count >= 4, $"expected the two-column sections of the sidebar; found {gutters.Count}");
                    Assert.All(gutters, g => Assert.Equal(10.0, g.ColumnDefinitions[1].Width.Value));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Composition_PreservesTheSelectionContract_AndTheCommonControls()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // The shared capture controls survive the recomposition (ADR-0019 §5: adopting the shell must not
                    // silently swap the editor's controls).
                    Assert.IsType<NumericField>(w.FindName("PositionsBox"));
                    Assert.IsType<NumericField>(w.FindName("CellBeamLengthOverrideBox"));
                    Assert.IsType<CatalogCombo>(w.FindName("PostBox"));

                    // The optional/integer/minimum contracts of the moved fields are intact.
                    Assert.True(((NumericField)w.FindName("CellBeamLengthOverrideBox")).IsOptional);
                    Assert.True(((NumericField)w.FindName("PositionsBox")).IntegerOnly);
                    Assert.True(((NumericField)w.FindName("FondosBox")).IntegerOnly);
                    Assert.Equal(2.0, ((NumericField)w.FindName("FondosBox")).Minimum);

                    // Selection and multi-selection still run through the card matrix over the same authority: a plain
                    // click replaces the selection, Ctrl+click extends it, and the selection is never empty.
                    var cards = (Grid)w.FindName("PushBackMatrixGrid");
                    Assert.NotNull(cards);
                    Assert.True(cards.Children.Count > 0, "the card matrix must still be built");

                    // The authority is PushBackEditorState.Structure; the cards are just its rendering.
                    w.SelectMatrixCell(0, 0, extend: false);
                    Assert.True(w.State.Structure.IsSelected(0, 0));
                    Assert.Equal(1, w.State.Structure.SelectedCellCount);

                    if (w.State.Structure.MaxLoadLevels() > 1)
                    {
                        w.SelectMatrixCell(0, 1, extend: true);   // Ctrl+click extends
                        Assert.True(w.State.Structure.IsSelected(0, 0));
                        Assert.True(w.State.Structure.IsSelected(0, 1));
                        Assert.Equal(2, w.State.Structure.SelectedCellCount);

                        w.SelectMatrixCell(0, 1, extend: false);  // plain click replaces
                        Assert.Equal(1, w.State.Structure.SelectedCellCount);
                    }
                }
                finally { w.Close(); }
            });
        }
    }
}
