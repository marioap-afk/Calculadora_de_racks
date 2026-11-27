using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, sexta tanda, parte pb (PB-S17): two defects of the classic dialog «Elementos de seguridad» of a Push Back of TWO sides, through the
    /// real classic window (<see cref="RackPushBackSystemWindow"/>) and the real grids it opens. Only the <c>ShowDialog</c> is replaced.
    /// <list type="bullet">
    /// <item>Point 5: «Auto» again on the face of one side, when the other face of that post is automatic, did nothing: the grid of one face
    /// dropped the row that became automatic at both ends before the section merged it, so the merge kept the measure it had.</item>
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

        // «Elementos de seguridad» with the real per-post grid of the section of side A (its face), the gesture on it, and «Aceptar».
        private static void ThroughSideA(RackPushBackSystemWindow w, Action<SafetyDefensaGridWindow> gesture)
        {
            w.DefenseDialog = section =>
            {
                var grid = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", section.PostCount, section.Posts,
                    lowEndOnly: true, autoPerEnd: true, face: section.Face());
                gesture(grid);
                grid.BuildResultForTest();
                return grid.Accepted ? grid.Result : null;
            };
            w.SafetyWindowDialog = dialog =>
            {
                dialog.WindowStartupLocation = WindowStartupLocation.Manual;
                dialog.ShowInTaskbar = false;
                dialog.Left = -10000;
                dialog.Top = -10000;
                dialog.Show();
                dialog.UpdateLayout();
                w.DefenseSectionForTest.Configure();
                dialog.BuildResultForTest();
                dialog.Close();
                return true;
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyWindowDialog = null;
            w.DefenseDialog = null;
        }

        private static SelectiveSafetySelection Defense(RackPushBackSystemWindow w)
            => w.SafetySelections.FirstOrDefault(selection =>
            {
                var element = w.Session.Catalog.SafetyElements?.FirstOrDefault(entry =>
                    string.Equals(entry?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
                return element != null && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType);
            });

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

        [Fact]
        public void AutoAgainOnTheFaceOfASideWhoseOtherFaceIsAutomaticGoesBackToTheRule()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Defenses(w);
                    Assert.NotEmpty(before);
                    Assert.Empty(Defense(w).DefensaPosts);

                    // Side A, post 2 off: a decision of its own on the face of A; the face of B of that post stays automatic.
                    ThroughSideA(w, grid => grid.ExitCheckForTest(1).IsChecked = false);
                    var record = Assert.Single(Defense(w).DefensaPosts);
                    Assert.Equal((1, false, 0.0, true), (record.PostIndex, record.ExitAuto, record.ExitLength, record.EntranceAuto));
                    Assert.NotEqual(before, Defenses(w));

                    // «Auto» again on that post of side A: the face follows the rule, and the rack draws what it drew before.
                    ThroughSideA(w, grid => grid.ExitAutoForTest(1).IsChecked = true);
                    Assert.Empty(Defense(w).DefensaPosts);
                    Assert.Equal(before, Defenses(w));
                }
                finally { w.Close(); }
            });
        }

        // ================================================================== point 4: the levels of the desviador grid

        [Fact]
        public void TheDesviadorGridOfATwoSidedRackHasTheLevelsTheDrawingPlacesFromEitherSide()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    // Front 2 of side B gets a fourth level; side A keeps three.
                    w.CompositeState.Of(PushBackSide.B).AdjustLevels(1, 1);
                    Settle(w);

                    var catalog = w.Session.Catalog;
                    var system = w.LastComputation.System;
                    var postCount = Math.Max(2, w.CompositeState.SlotCount + 1);
                    var drawn = PushBackDiverterPlan.Resolve(system, catalog);
                    var drawnPerPost = Enumerable.Range(0, postCount)
                        .Select(post => drawn.Where(d => d.PostLine == post).Select(d => d.Level).DefaultIfEmpty(0).Max())
                        .ToArray();

                    w.CompositeState.SetActiveSide(PushBackSide.A);
                    var fromA = w.DesviadorLevelsPerPost().ToArray();
                    w.CompositeState.SetActiveSide(PushBackSide.B);
                    var fromB = w.DesviadorLevelsPerPost().ToArray();
                    w.CompositeState.SetActiveSide(PushBackSide.A);

                    // The real grid as «Configurar…» of the desviador opens it from side A (EditDesviador: no resolved system, the per-post
                    // list of the window), with the desviador of N4 of post 2 off; «Aceptar» without touching anything.
                    var grid = new SafetyDesviadorGridWindow(
                        "DESVIADOR", "Desviador", system: null, catalog: catalog,
                        longitud: SelectiveSafetyDefaults.DesviadorLongitud,
                        firstHeight: SelectiveSafetyDefaults.DesviadorPrimerNivelAltura,
                        side: SafetySide.Left,
                        offCells: new[] { new SelectiveGridCell { Frente = 1, Level = 3 } },
                        fallbackPostCount: fromA.Length,
                        fallbackLevelsPerFrente: fromA,
                        fallbackLevelsArePerPost: true,
                        showSide: false,
                        allowBlankColumns: true);
                    try
                    {
                        var offered = !grid.Model.IsAbsent(1, 3);
                        var kept = grid.BuildResult().OffCells.Any(cell => cell.Frente == 1 && cell.Level == 3);
                        return (drawnPerPost, fromA, fromB, offered, kept);
                    }
                    finally { grid.Close(); }
                }
                finally { w.Close(); }
            });

            Assert.Equal(4, r.drawnPerPost[1]);                // the drawing places a desviador at N4 of post 2 (the beds of side B)
            Assert.Equal(r.drawnPerPost, r.fromA);             // the grid opened from side A has every level the drawing places
            Assert.Equal(r.fromB, r.fromA);                    // and the active side does not change it
            Assert.True(r.offered, "N4 of post 2 is drawn, so its cell can be switched off");
            Assert.True(r.kept, "an off cell at a level only side B has survives «Aceptar» from side A");
        }
    }
}
