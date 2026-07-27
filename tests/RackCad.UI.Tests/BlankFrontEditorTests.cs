using System;
using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-33 / PB-014 — STA tests of the Activo/En blanco toggle over the REAL editor windows. They lock the minimal UI
    /// contract: the box exists once per front, blanking a front empties ITS column only, the front's configuration
    /// stays dormant so unblanking restores it, and the last active front cannot be blanked.
    /// </summary>
    public sealed class BlankFrontEditorTests
    {
        private static DynamicRackDesign Structure()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 3 });
            return design;
        }

        private static CheckBox BlankBox(System.Windows.Window window, string gridName, int frontIndex)
            => ((Grid)window.FindName(gridName)).Children
                .OfType<System.Windows.FrameworkElement>()
                .Where(element => Grid.GetRow(element) == 0 && Grid.GetColumn(element) == frontIndex + 1)
                .SelectMany(Descendants)
                .OfType<CheckBox>()
                .Single();

        private static System.Collections.Generic.IEnumerable<System.Windows.FrameworkElement> Descendants(
            System.Windows.FrameworkElement element)
        {
            yield return element;
            if (element is Panel panel)
            {
                foreach (var child in panel.Children.OfType<System.Windows.FrameworkElement>()
                             .SelectMany(Descendants))
                {
                    yield return child;
                }
            }
        }

        private static Control Named(System.Windows.Window window, string name)
            => (Control)window.FindName(name)
               ?? throw new InvalidOperationException("No existe el control " + name);

        private static void AssertAllEnabled(System.Windows.Window window, bool expected, params string[] names)
        {
            foreach (var name in names)
            {
                Assert.True(
                    Named(window, name).IsEnabled == expected,
                    name + " deberia estar " + (expected ? "habilitado" : "deshabilitado"));
            }
        }

        /// <summary>
        /// The two +/- step rows the dynamic matrix header builds for one front, in order: [0] = Posiciones
        /// (structural, stays available) and [1] = Niveles (edits levels, off while the front is blank). Neither
        /// carries an x:Name, so they are located by their position in the header.
        /// </summary>
        private static System.Collections.Generic.List<System.Collections.Generic.List<Button>> StepButtonRows(
            System.Windows.Window window, string gridName, int frontIndex)
            => ((Grid)window.FindName(gridName)).Children
                .OfType<System.Windows.FrameworkElement>()
                .Where(element => Grid.GetRow(element) == 0 && Grid.GetColumn(element) == frontIndex + 1)
                .SelectMany(Descendants)
                .OfType<StackPanel>()
                .Where(panel => panel.Orientation == Orientation.Horizontal)
                .Select(panel => panel.Children.OfType<Button>().ToList())
                .Where(buttons => buttons.Count == 2)
                .ToList();

        // ---- Push Back ---------------------------------------------------------------------------------------

        private static RackPushBackSystemWindow PushBackWindow()
        {
            var window = new RackPushBackSystemWindow();
            window.LoadDesignForNew(new PushBackDesign { Structure = Structure() }, "PB-I33");
            return window;
        }

        [Fact]
        public void PushBack_EveryFrontOffersTheBlankBoxAndStartsActive()
        {
            StaTestRunner.Run(() =>
            {
                var window = PushBackWindow();
                try
                {
                    Assert.Equal(2, window.State.Structure.Count);
                    for (var front = 0; front < window.State.Structure.Count; front++)
                    {
                        Assert.False(BlankBox(window, "PushBackMatrixGrid", front).IsChecked);
                        Assert.True(window.State.Structure.IsActive(front));
                    }
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void PushBack_BlankingAFrontEmptiesOnlyItsColumnAndKeepsItsConfigurationDormant()
        {
            StaTestRunner.Run(() =>
            {
                var window = PushBackWindow();
                try
                {
                    var dormantLevels = window.State.Structure.Fronts[1].LoadLevels;
                    var dormantPositions = window.State.Structure.Fronts[1].PalletCount;

                    BlankBox(window, "PushBackMatrixGrid", 1).IsChecked = true;

                    Assert.False(window.State.Structure.IsActive(1));
                    Assert.True(window.State.Structure.IsActive(0));
                    Assert.Equal(new[] { 1 }, window.State.Structure.BlankFrontIndices().ToArray());

                    // The row keeps its values: nothing was zeroed, so no fake cell stands in for the blank front.
                    Assert.Equal(dormantLevels, window.State.Structure.Fronts[1].LoadLevels);
                    Assert.Equal(dormantPositions, window.State.Structure.Fronts[1].PalletCount);

                    // Only front 1's cards go inactive; front 0 keeps all of its own.
                    var cards = PushBackMatrixCardModel.Build(window.State);
                    Assert.All(cards.Where(card => card.FrontIndex == 1), card => Assert.False(card.IsActive));
                    Assert.Equal(3, cards.Count(card => card.FrontIndex == 0 && card.IsActive));

                    // Unblanking restores exactly the same cards.
                    BlankBox(window, "PushBackMatrixGrid", 1).IsChecked = false;
                    Assert.True(window.State.Structure.IsActive(1));
                    Assert.Equal(
                        2,
                        PushBackMatrixCardModel.Build(window.State)
                            .Count(card => card.FrontIndex == 1 && card.IsActive));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void PushBack_RefusesToBlankTheLastActiveFront()
        {
            StaTestRunner.Run(() =>
            {
                var window = PushBackWindow();
                try
                {
                    BlankBox(window, "PushBackMatrixGrid", 0).IsChecked = true;
                    Assert.False(window.State.Structure.IsActive(0));

                    // Front 1 is the only active one left: the authority refuses and it stays active.
                    Assert.False(window.State.SetActive(1, false));
                    Assert.True(window.State.Structure.IsActive(1));

                    // And through the real box, the same refusal.
                    BlankBox(window, "PushBackMatrixGrid", 1).IsChecked = true;
                    Assert.True(window.State.Structure.IsActive(1));
                }
                finally { window.Close(); }
            });
        }

        // ---- Dinámico ----------------------------------------------------------------------------------------

        private static RackDynamicSystemWindow DynamicWindow()
        {
            var window = new RackDynamicSystemWindow();
            window.LoadDesignForNew(Structure(), "DIN-I33");
            return window;
        }

        [Fact]
        public void Dynamic_EveryFrontOffersTheBlankBoxAndStartsActive()
        {
            StaTestRunner.Run(() =>
            {
                var window = DynamicWindow();
                try
                {
                    var design = window.BuildDesignForTest(out var ok);

                    Assert.True(ok);
                    Assert.Equal(2, design.Fronts.Count);
                    Assert.All(design.Fronts, front => Assert.True(front.IsActive));
                    Assert.False(BlankBox(window, "DynamicMatrixGrid", 0).IsChecked);
                    Assert.False(BlankBox(window, "DynamicMatrixGrid", 1).IsChecked);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void Dynamic_BlankingAFrontKeepsItsCellsDormantAndReactivationRestoresIt()
        {
            StaTestRunner.Run(() =>
            {
                var window = DynamicWindow();
                try
                {
                    var before = window.BuildDesignForTest(out _);
                    var dormantLevels = before.Fronts[1].Levels.Count;
                    var dormantPositions = before.Fronts[1].PalletCount;
                    Assert.True(dormantLevels > 0);

                    BlankBox(window, "DynamicMatrixGrid", 1).IsChecked = true;

                    // The built design carries the flag and STILL writes the dormant cells: no fake cell, no reset.
                    var blank = window.BuildDesignForTest(out var blankOk);
                    Assert.True(blankOk);
                    Assert.True(blank.Fronts[0].IsActive);
                    Assert.False(blank.Fronts[1].IsActive);
                    Assert.Equal(dormantLevels, blank.Fronts[1].Levels.Count);
                    Assert.Equal(dormantPositions, blank.Fronts[1].PalletCount);

                    BlankBox(window, "DynamicMatrixGrid", 1).IsChecked = false;

                    var restored = window.BuildDesignForTest(out var restoredOk);
                    Assert.True(restoredOk);
                    Assert.All(restored.Fronts, front => Assert.True(front.IsActive));
                    Assert.Equal(dormantLevels, restored.Fronts[1].Levels.Count);
                    Assert.Equal(dormantPositions, restored.Fronts[1].PalletCount);
                }
                finally { window.Close(); }
            });
        }

        // ---- Editability while a blank front is SELECTED (both directions) ----------------------------------

        private static readonly string[] DynamicLevelAndCellControls =
        {
            "SelectedLevelsBox", "FirstLevelHeightBox",
            "FrontBox", "PalletHeightBox", "WeightBox", "SelectedClearHeightBox", "SelectedBeamLengthBox",
            "SelectedInOutBeamBox", "SelectedInOutPeralteBox",
            "SelectedIntermediateBeamBox", "SelectedIntermediatePeralteBox",
            "ApplyCellButton", "ApplySelectedCellsButton", "ApplyLevelButton", "ApplyFrontButton", "ApplyAllButton"
        };

        private static readonly string[] DynamicStructuralControls =
        {
            "SelectedPositionsBox", "SelectedPalletsDeepBox", "SelectedDepthStartBox"
        };

        [Fact]
        public void Dynamic_SelectingABlankFrontKeepsAValidSelectionAndDisablesLevelAndCellEditing()
        {
            StaTestRunner.Run(() =>
            {
                var window = DynamicWindow();
                try
                {
                    AssertAllEnabled(window, true, DynamicLevelAndCellControls);

                    BlankBox(window, "DynamicMatrixGrid", 1).IsChecked = true;

                    // The selection stays VALID — the blank front is the selected one and the design still builds.
                    var design = window.BuildDesignForTest(out var ok);
                    Assert.True(ok);
                    Assert.False(design.Fronts[1].IsActive);

                    // Everything that edits a level or a non-existent cell, and every cell-bound scope, is off...
                    AssertAllEnabled(window, false, DynamicLevelAndCellControls);
                    // ...with a visible reason.
                    Assert.Contains("en blanco", (string)Named(window, "ApplyCellButton").ToolTip);

                    // In the matrix header, the NIVELES steppers go off while the POSICIONES ones — structural — stay.
                    var rows = StepButtonRows(window, "DynamicMatrixGrid", 1);
                    Assert.Equal(2, rows.Count);
                    Assert.All(rows[0], button => Assert.True(button.IsEnabled));    // Posiciones
                    Assert.All(rows[1], button => Assert.False(button.IsEnabled));   // Niveles

                    // The front's structural controls remain valid and available.
                    AssertAllEnabled(window, true, DynamicStructuralControls);

                    // Reactivating restores editing immediately, tooltips included.
                    BlankBox(window, "DynamicMatrixGrid", 1).IsChecked = false;
                    AssertAllEnabled(window, true, DynamicLevelAndCellControls);
                    AssertAllEnabled(window, true, DynamicStructuralControls);
                    Assert.All(
                        StepButtonRows(window, "DynamicMatrixGrid", 1).SelectMany(row => row),
                        button => Assert.True(button.IsEnabled));
                    Assert.DoesNotContain(
                        "en blanco",
                        (string)(Named(window, "ApplySelectedCellsButton").ToolTip ?? string.Empty));
                }
                finally { window.Close(); }
            });
        }

        private static readonly string[] PushBackLevelAndCellControls =
        {
            "LevelsBox", "FirstLevelHeightBox", "SelectedLevelBox",
            "CellPalletFrontBox", "CellPalletHeightBox", "CellPalletWeightBox", "CellClearBox",
            "CellBeamLengthOverrideBox", "CellInOutBeamBox", "CellInOutPeralteBox",
            "CellIntermediateBeamBox", "CellIntermediatePeralteBox", "RearPeralteBox",
            "ApplyCellButton", "ApplySelectedButton", "ApplyLevelButton", "ApplyFrontButton", "ApplyAllButton"
        };

        private static readonly string[] PushBackStructuralControls =
        {
            "PositionsBox", "FondosBox", "DepthStartBox"
        };

        [Fact]
        public void PushBack_SelectingABlankFrontKeepsAValidSelectionAndDisablesLevelAndCellEditing()
        {
            StaTestRunner.Run(() =>
            {
                var window = PushBackWindow();
                try
                {
                    AssertAllEnabled(window, true, PushBackLevelAndCellControls);

                    // Select front 1 first, then blank it: the selection must survive the transition.
                    window.SelectMatrixCell(1, 0, false);
                    BlankBox(window, "PushBackMatrixGrid", 1).IsChecked = true;

                    Assert.Equal(1, window.State.Structure.SelectedFrontIndex);
                    Assert.True(window.State.Structure.SelectedCellCount > 0);
                    Assert.False(window.State.Structure.IsActive(1));

                    AssertAllEnabled(window, false, PushBackLevelAndCellControls);
                    Assert.Contains("en blanco", (string)Named(window, "ApplyCellButton").ToolTip);
                    AssertAllEnabled(window, true, PushBackStructuralControls);

                    // Reactivating restores editing immediately.
                    BlankBox(window, "PushBackMatrixGrid", 1).IsChecked = false;
                    AssertAllEnabled(window, true, PushBackLevelAndCellControls);
                    AssertAllEnabled(window, true, PushBackStructuralControls);
                    Assert.DoesNotContain(
                        "en blanco",
                        (string)(Named(window, "ApplySelectedButton").ToolTip ?? string.Empty));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void Dynamic_RefusesToBlankTheLastActiveFront()
        {
            StaTestRunner.Run(() =>
            {
                var window = DynamicWindow();
                try
                {
                    BlankBox(window, "DynamicMatrixGrid", 0).IsChecked = true;
                    Assert.False(window.BuildDesignForTest(out _).Fronts[0].IsActive);

                    // Front 1 is the only active one left: the box refuses and the design keeps carrying load.
                    BlankBox(window, "DynamicMatrixGrid", 1).IsChecked = true;

                    var design = window.BuildDesignForTest(out _);
                    Assert.True(design.Fronts[1].IsActive);
                    Assert.True(DynamicFrontActivation.HasActiveFront(design.Fronts));
                }
                finally { window.Close(); }
            });
        }
    }
}
