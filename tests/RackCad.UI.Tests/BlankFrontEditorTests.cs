using System;
using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI;
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
