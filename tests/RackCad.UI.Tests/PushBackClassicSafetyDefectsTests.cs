using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, sexta tanda, parte pb (PB-S17): two defects of the classic dialog «Elementos de seguridad» of a Push Back of TWO sides, through the
    /// real classic window (<see cref="RackPushBackSystemWindow"/>), the real dialog and the real grids it opens. Only the <c>ShowDialog</c> is
    /// replaced.
    /// <list type="bullet">
    /// <item>Point 5: «Auto» again on the face of one side, when the other face of that post is automatic, did nothing: the grid of one face
    /// dropped the row that became automatic at both ends before the section merged it, so the merge kept the measure it had. And (review of
    /// 2f1e9e4e, M1) the host merged the sections over the records the selection had: with a dialog that kept DEFENSA, the same loss.</item>
    /// <item>Point 4: the desviador grid had the levels of the ACTIVE side, so from the lower side a desviador the drawing places (at a level
    /// only the other side has) could not be switched off, and an off cell there was lost on «Aceptar».</item>
    /// </list>
    /// </summary>
    public sealed class PushBackClassicSafetyDefectsTests
    {
        private static RackPushBackSystemWindow Composite()
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
            w.CompositeState.SetSlotCount(3);
            w.CompositeState.SetSideBPresent(true);
            for (var slot = 0; slot < 3; slot++)
            {
                w.CompositeState.SetSlotPresent(PushBackSide.B, slot, true);
            }

            Settle(w);
            return w;
        }

        // The window recomputes as soon as a control changes; these tests write the composite state directly, so they ask for it.
        private static void Settle(RackPushBackSystemWindow w)
        {
            w.SafetyDialog = selections => selections.ToList();
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyDialog = null;
        }

        private static PushBackDefenseSection Section(RackPushBackSystemWindow w, PushBackSide side)
            => side == PushBackSide.B ? w.DefenseSectionBForTest : w.DefenseSectionForTest;

        // The real per-post grid of the defense section of a side (its face), with the gesture on it; null when it does not accept.
        private static void DefenseGrid(RackPushBackSystemWindow w, PushBackSide side, Action<SafetyDefensaGridWindow> gesture)
            => w.DefenseDialog = section =>
            {
                Assert.Equal(side, section.Side);
                var grid = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", section.PostCount, section.Posts,
                    lowEndOnly: true, autoPerEnd: true, face: section.Face());
                gesture(grid);
                grid.BuildResultForTest();
                return grid.Accepted ? grid.Result : null;
            };

        // Shows the real dialog off screen, runs what it is given on it (with the dialog), accepts it and closes it.
        private static void ThroughTheDialog(RackPushBackSystemWindow w, Action<SelectiveSafetyWindow> inside)
        {
            w.SafetyWindowDialog = dialog =>
            {
                dialog.WindowStartupLocation = WindowStartupLocation.Manual;
                dialog.ShowInTaskbar = false;
                dialog.Left = -10000;
                dialog.Top = -10000;
                dialog.Show();
                dialog.UpdateLayout();
                inside(dialog);
                dialog.BuildResultForTest();
                dialog.Close();
                return true;
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyWindowDialog = null;
            w.DefenseDialog = null;
        }

        // «Elementos de seguridad», «Configurar…» of the defense of a side, the gesture on its grid, and «Aceptar».
        private static void ThroughSide(RackPushBackSystemWindow w, PushBackSide side, Action<SafetyDefensaGridWindow> gesture)
        {
            DefenseGrid(w, side, gesture);
            ThroughTheDialog(w, _ => Section(w, side).Configure());
        }

        private static SelectiveSafetySelection Defense(RackPushBackSystemWindow w)
            => SelectiveSafetyFamilies.SelectedOfType(w.SafetySelections, w.Session.Catalog.SafetyElements, SelectiveSafetyDefaults.DefensaType);

        private static SelectiveSafetySelection Desviador(RackPushBackSystemWindow w)
            => SelectiveSafetyFamilies.SelectedOfType(w.SafetySelections, w.Session.Catalog.SafetyElements, SelectiveSafetyDefaults.DesviadorType);

        // The defenses of the drawing (the plan view), as «X|Y».
        private static IReadOnlyList<string> Defenses(RackPushBackSystemWindow w)
        {
            var catalog = w.Session.Catalog;
            var id = catalog.SafetyElements.First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)).Id;
            return new PushBackSystemPlantaBuilder().BuildPlan(w.LastComputation.System, catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Safety && string.Equals(instance.PieceId, id, StringComparison.OrdinalIgnoreCase))
                .Select(instance => FormattableString.Invariant($"{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        // ================================================================== point 5: «Auto» again on one face

        [Theory]
        [InlineData(PushBackSide.A)]
        [InlineData(PushBackSide.B)]
        public void AutoAgainOnTheFaceOfASideWhoseOtherFaceIsAutomaticGoesBackToTheRule(PushBackSide side)
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Defenses(w);
                    Assert.NotEmpty(before);
                    Assert.Empty(Defense(w).DefensaPosts);

                    // Post 2 off on the face of the side: a decision of its own there; the other face of that post stays automatic.
                    ThroughSide(w, side, grid => grid.ExitCheckForTest(1).IsChecked = false);
                    var record = Assert.Single(Defense(w).DefensaPosts);
                    Assert.Equal(1, record.PostIndex);
                    Assert.False(PushBackDefenseSides.AutoOf(record, side));
                    Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, side));
                    Assert.True(PushBackDefenseSides.AutoOf(record, side == PushBackSide.A ? PushBackSide.B : PushBackSide.A));
                    Assert.NotEqual(before, Defenses(w));

                    // «Auto» again on that post: the face follows the rule, and the rack draws what it drew before.
                    ThroughSide(w, side, grid => grid.ExitAutoForTest(1).IsChecked = true);
                    Assert.Empty(Defense(w).DefensaPosts);
                    Assert.Equal(before, Defenses(w));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TheSectionsAreMergedFromNothingSoADialogThatKeepsTheDefenseDoesNotBringTheOldFaceBack()
        {
            // Review of 2f1e9e4e, M1. Today the «Aceptar» of the dialog does not return DEFENSA, so the selection the host merges on is new and
            // empty. The seam returns the list as it came, DEFENSA included, as a dialog that kept the families would: the merge must still
            // start from the sections (they carry every record of the rack), not from the records the selection had.
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Defenses(w);
                    ThroughSide(w, PushBackSide.A, grid => grid.ExitCheckForTest(1).IsChecked = false);
                    Assert.Single(Defense(w).DefensaPosts);

                    DefenseGrid(w, PushBackSide.A, grid => grid.ExitAutoForTest(1).IsChecked = true);
                    w.SafetyDialog = selections =>
                    {
                        w.DefenseSectionForTest.Configure();
                        Assert.Contains(selections, s => s.DefensaPosts.Count > 0); // DEFENSA travels with its old record
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    w.SafetyDialog = null;
                    w.DefenseDialog = null;

                    Assert.Empty(Defense(w).DefensaPosts);
                    Assert.Equal(before, Defenses(w));
                }
                finally { w.Close(); }
            });
        }

        // ================================================================== point 4: the levels of the desviador grid

        // «Configurar…» of the desviador inside the real dialog: its real grid (EditDesviador) through the seam of the dialog.
        private static void ConfigureDesviador(SelectiveSafetyWindow dialog, Func<SafetyDesviadorGridWindow, SafetyDesviadorGridWindow.DesviadorResult> grid)
        {
            var button = EditorWindowTestSupport.Find<Button>(dialog, b => b.ToolTip is string tip && tip.StartsWith("Desviador:", StringComparison.Ordinal))
                         ?? throw new InvalidOperationException("No «Configurar…» of the desviador in the dialog.");
            dialog.DesviadorDialog = grid;
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
            dialog.DesviadorDialog = null;
        }

        [Fact]
        public void TheDesviadorGridOfATwoSidedRackHasTheLevelsTheDrawingPlacesFromEitherSide()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    // Front 2 of side B gets a fourth level; side A keeps three. The drawing places a desviador at N4 of posts 2 and 3.
                    w.CompositeState.Of(PushBackSide.B).AdjustLevels(1, 1);
                    Settle(w);
                    var catalog = w.Session.Catalog;
                    var postCount = Math.Max(2, w.CompositeState.SlotCount + 1);
                    int[] DrawnPerPost()
                    {
                        var drawn = PushBackDiverterPlan.Resolve(w.LastComputation.System, catalog);
                        return Enumerable.Range(0, postCount)
                            .Select(post => drawn.Where(d => d.PostLine == post).Select(d => d.Level).DefaultIfEmpty(0).Max()).ToArray();
                    }

                    var drawnPerPost = DrawnPerPost();
                    Assert.Equal(new[] { 3, 4, 4, 3 }, drawnPerPost);

                    // From side A: «Elementos de seguridad» › «Configurar…» of the desviador, the real grid; N4 of post 2 off, «Aceptar».
                    w.CompositeState.SetActiveSide(PushBackSide.A);
                    int[] shownFromA = null;
                    ThroughTheDialog(w, dialog => ConfigureDesviador(dialog, grid =>
                    {
                        shownFromA = Enumerable.Range(0, grid.Model.Columns)
                            .Select(post => Enumerable.Range(0, grid.Model.Rows).Count(level => !grid.Model.IsAbsent(post, level))).ToArray();
                        grid.Model.SetSelected(1, 3, false);
                        return grid.BuildResult();
                    }));

                    Assert.Equal(drawnPerPost, shownFromA);   // every level the drawing places, from the lower side
                    Assert.Contains(Desviador(w).DesviadorOffCells, cell => cell.Frente == 1 && cell.Level == 3);
                    Assert.Equal(new[] { 3, 3, 4, 3 }, DrawnPerPost()); // the desviador at N4 of post 2 is gone; the one of post 3 stays

                    // From side A again, «Aceptar» without touching the grid: the off cell at a level only side B has stays.
                    ThroughTheDialog(w, dialog => ConfigureDesviador(dialog, grid => grid.BuildResult()));
                    Assert.Contains(Desviador(w).DesviadorOffCells, cell => cell.Frente == 1 && cell.Level == 3);

                    // And from side B the grid is the same one.
                    w.CompositeState.SetActiveSide(PushBackSide.B);
                    int[] shownFromB = null;
                    ThroughTheDialog(w, dialog => ConfigureDesviador(dialog, grid =>
                    {
                        shownFromB = Enumerable.Range(0, grid.Model.Columns)
                            .Select(post => Enumerable.Range(0, grid.Model.Rows).Count(level => !grid.Model.IsAbsent(post, level))).ToArray();
                        return null; // «Cancelar» of the grid
                    }));
                    Assert.Equal(shownFromA, shownFromB);
                }
                finally { w.Close(); }
            });
        }
    }
}
