using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Controls;
using RackCad.UI.Shell;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Owner decisions (2026-07-24), items 1 and 2: the rear tope is editable ONLY from Seguridad (every redundant
    /// control is gone, the cell scopes cannot carry it, and the TOPE never enters the safety list), and the sidebar
    /// separates the FRENTE's structural data from the CELL's, each with its own scopes.
    /// </summary>
    public sealed class PushBackTopeOnlyInSafetyTests
    {
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

        // ---- 1. every redundant tope control is gone ----

        [Fact]
        public void EveryRedundantTopeControl_IsGoneFromTheWindow()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    foreach (var name in new[]
                    {
                        "BulkToolsExpander", "CellSelectionMatrix", "TopeMatrix",
                        "TopesAllButton", "TopesNoneButton", "TopesHint",
                        "RearTopeActiveCheck", "SaqueBox",
                    })
                    {
                        Assert.True(w.FindName(name) == null, $"{name} must no longer exist");
                    }

                    // Seguridad is the only remaining door to the rear stop.
                    Assert.NotNull(w.FindName("SafetyButton"));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TheCardMatrix_ShowsNoTopeState_AndKeepsClickAndCtrlClick()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // No tope wording on any card.
                    for (var front = 0; front < w.State.Structure.Count; front++)
                    {
                        for (var level = 0; level < w.State.Structure.MaxLoadLevels(); level++)
                        {
                            var text = PushBackMatrixCardModel.CardText(w.State, front, level);
                            Assert.DoesNotContain("Tope", text);
                            Assert.DoesNotContain("tope", text);
                        }
                    }

                    // Multi-selection survives: plain click replaces, Ctrl+click extends, never empty.
                    w.SelectMatrixCell(0, 0, extend: false);
                    Assert.Equal(1, w.State.Structure.SelectedCellCount);
                    if (w.State.Structure.MaxLoadLevels() > 1)
                    {
                        w.SelectMatrixCell(0, 1, extend: true);
                        Assert.Equal(2, w.State.Structure.SelectedCellCount);
                        w.SelectMatrixCell(0, 1, extend: false);
                        Assert.Equal(1, w.State.Structure.SelectedCellCount);
                    }
                }
                finally { w.Close(); }
            });
        }

        // ---- 2. the TOPE is never ordinary low-end safety ----

        [Fact]
        public void TheTope_IsNeverOfferedNorAdmittedAsSafety()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // Not offered by the dialog...
                    Assert.All(w.SafetyElementsForDialog(), element => Assert.False(
                        SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.TopeType),
                        $"a TOPE must not be offered as low-end safety: {element.Id}"));

                    // ...and refused by the Push Back authority even if one reached the list.
                    var authority = new PushBackSafetyAuthority(w.Session.Catalog);
                    var tope = w.Session.Catalog?.SafetyElements?
                        .FirstOrDefault(e => e != null && SelectiveSafetyDefaults.IsType(e.Type, SelectiveSafetyDefaults.TopeType));
                    if (tope != null)
                    {
                        var selection = new SelectiveSafetySelection { ElementId = tope.Id, Quantity = 1 };
                        Assert.True(authority.IsRearStop(selection));
                        Assert.True(authority.IsUnsupported(selection));
                        Assert.DoesNotContain(authority.Authorize(new[] { selection }), s =>
                            string.Equals(s.ElementId, tope.Id, StringComparison.OrdinalIgnoreCase));
                    }
                }
                finally { w.Close(); }
            });
        }

        // ---- 3. SAQUE / OffCells round trip through the shared dialog's contract ----

        [Fact]
        public void RearTopeConfig_RoundTripsSaqueAndOffCells_ThroughTheAdapterContract()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // Project: a fresh system has every stop active, so no OffCells and the default SAQUE.
                    var projected = w.State.RearTopeConfig();
                    Assert.Empty(projected.OffCells);
                    Assert.Equal(PushBackDefaults.RearTopeSaque, PushBackRearTopeDialogAdapter.Saque(projected), 9);

                    // Recover what the shared dialog would return: a new SAQUE and one deactivated cell.
                    var result = new SafetyTopeGridWindow.TopeResult
                    {
                        Saque = 4.25,
                        OffCells = { new SelectiveGridCell { Frente = 0, Level = 1 } }
                    };
                    PushBackRearTopeDialogAdapter.Apply(result, projected);
                    w.State.LoadRearTopeConfig(projected);

                    Assert.Equal(4.25, w.State.RearTopeSaque, 9);
                    Assert.True(w.State.Cell(0, 0).RearTopeEnabled);
                    Assert.False(w.State.Cell(0, 1).RearTopeEnabled);

                    // And the projection is stable: reading it back yields the same SAQUE and the same single OffCell.
                    var again = w.State.RearTopeConfig();
                    Assert.Equal(4.25, again.Saque, 9);
                    var off = Assert.Single(again.OffCells);
                    Assert.Equal(0, off.Frente);
                    Assert.Equal(1, off.Level);

                    // A cancelled dialog (null result) leaves the config untouched.
                    PushBackRearTopeDialogAdapter.Apply(null, again);
                    Assert.Equal(4.25, again.Saque, 9);
                    Assert.Single(again.OffCells);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DeactivatedTope_ReachesTheDesign_AndSurvivesTheBuild()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var config = w.State.RearTopeConfig();
                    config.Saque = 4.5;
                    config.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 0 });
                    w.State.LoadRearTopeConfig(config);
                    w.Session.Recompute.Request();

                    var design = w.LastComputation?.Design;
                    Assert.NotNull(design);
                    Assert.Equal(4.5, design.RearTope.Saque, 9);
                    Assert.False(design.RearTope.At(0, 0));   // the deactivation persisted
                    Assert.True(design.RearTope.At(0, 1));    // and only that one
                }
                finally { w.Close(); }
            });
        }

        // ---- 4. the cell scopes never propagate the tope ----

        [Fact]
        public void CellScopes_DoNotPropagateTheTope()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // Deactivate ONE cell through the only legitimate path.
                    var config = w.State.RearTopeConfig();
                    config.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 0 });
                    w.State.LoadRearTopeConfig(config);
                    Assert.False(w.State.Cell(0, 0).RearTopeEnabled);

                    // Now apply EVERY cell scope from that cell. None of them may carry the tope to the others,
                    // nor switch the source cell back on.
                    w.SelectMatrixCell(0, 0, extend: false);
                    foreach (var button in new[] { "ApplyCellButton", "ApplyLevelButton", "ApplyFrontButton", "ApplyAllButton" })
                    {
                        EditorWindowTestSupport.ClickNamed(w, button);
                    }

                    Assert.False(w.State.Cell(0, 0).RearTopeEnabled);   // still off
                    for (var level = 1; level < w.State.Structure.MaxLoadLevels(); level++)
                    {
                        Assert.True(w.State.Cell(0, level).RearTopeEnabled, $"scope leaked the tope to level {level}");
                    }
                }
                finally { w.Close(); }
            });
        }

        // ---- 5. front data vs cell data are separated, each with its own scopes ----

        [Fact]
        public void TheHighlightedPanel_SeparatesFrontDataFromCellData()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var shell = (RackEditorVisualShell)w.Content;

                    // Both groups, and both scope rows, live in the sidebar.
                    foreach (var name in new[]
                    {
                        "PositionsBox", "LevelsBox", "FondosBox", "DepthStartBox", "FirstLevelHeightBox",
                        "ApplyFrontDataButton", "ApplySelectedFrontDataButton", "ApplyAllFrontDataButton",
                        "CellPalletFrontBox", "CellPalletHeightBox", "CellPalletWeightBox", "CellClearBox",
                        "CellBeamLengthOverrideBox", "CellInOutBeamBox", "CellIntermediateBeamBox",
                        "CellInOutPeralteBox", "CellIntermediatePeralteBox", "RearPeralteBox",
                        "ApplyCellButton", "ApplySelectedButton", "ApplyLevelButton", "ApplyFrontButton", "ApplyAllButton",
                    })
                    {
                        var element = w.FindName(name) as FrameworkElement;
                        Assert.True(element != null, $"missing {name}");
                        Assert.True(IsUnder(shell.SidebarScroll, element), $"{name} must live in the sidebar");
                    }

                    // The highlighted Border groups them (the dynamic editor's visual pattern).
                    var panel = Ancestors((FrameworkElement)w.FindName("ApplyFrontDataButton"))
                        .OfType<Border>()
                        .FirstOrDefault(b => b.Background is System.Windows.Media.SolidColorBrush brush
                                             && brush.Color == System.Windows.Media.Color.FromRgb(0xF1, 0xF4, 0xF8));
                    Assert.True(panel != null, "the front/cell groups must sit in the highlighted panel");
                    Assert.True(IsUnder(panel, (FrameworkElement)w.FindName("CellPalletFrontBox")));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void FrontScopes_WriteTheWholeFrente_WithoutTouchingCellOnlyData()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    EditorWindowTestSupport.ClickNamed(w, "AddFrontButton");
                    Assert.Equal(2, w.State.Structure.Count);

                    var palletBefore = w.State.Structure.Fronts[1].Cells[0].PalletFront;

                    // Change a FRONT field on front 0 and push it to ALL fronts.
                    ((NumericField)w.FindName("PositionsBox")).SetNumber(5);
                    EditorWindowTestSupport.ClickNamed(w, "ApplyAllFrontDataButton");

                    Assert.Equal(5, w.State.Structure.Fronts[0].PalletCount);
                    Assert.Equal(5, w.State.Structure.Fronts[1].PalletCount);

                    // The front scope did not disturb the other front's per-cell pallet data.
                    Assert.Equal(palletBefore, w.State.Structure.Fronts[1].Cells[0].PalletFront, 6);
                }
                finally { w.Close(); }
            });
        }

        private static bool IsUnder(DependencyObject ancestor, DependencyObject node)
        {
            for (var p = node; p != null; p = System.Windows.Media.VisualTreeHelper.GetParent(p))
            {
                if (ReferenceEquals(p, ancestor)) return true;
            }
            return false;
        }

        private static IEnumerable<DependencyObject> Ancestors(DependencyObject node)
        {
            for (var p = node; p != null; p = System.Windows.Media.VisualTreeHelper.GetParent(p))
            {
                yield return p;
            }
        }
    }
}
