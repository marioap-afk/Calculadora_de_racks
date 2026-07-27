using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Owner decision (2026-07-24, FINAL), item 1 — the rear stop is configured INSIDE the visible "Elementos de
    /// seguridad" experience: a "Topes posteriores" section with its state and a "Configurar…" button. It is never a
    /// <see cref="SelectiveSafetySelection"/>, accepting applies safety and the stop in one operation, cancelling
    /// changes neither, and no second dialog opens by itself after Aceptar.
    /// </summary>
    public sealed class PushBackTopeSectionInSafetyTests
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

        // ---- the section is really VISIBLE inside the safety dialog ----

        [Fact]
        public void TheSafetyDialog_ShowsTheRearTopeSection_WithItsStateAndButton()
        {
            StaTestRunner.Run(() =>
            {
                var config = new PushBackRearTopeConfig { Saque = 3.5 };
                var section = new PushBackRearTopeSection(config, _ => null);

                // The dialog is COMPOSED with the section, exactly as the editor composes it.
                var dialog = new SelectiveSafetyWindow(
                    new List<SafetyElementCatalogEntry>(), new List<SelectiveSafetySelection>(), postCount: 2,
                    extraSection: section.View);
                try
                {
                    dialog.Show();
                    dialog.UpdateLayout();

                    var texts = Descendants(dialog).OfType<TextBlock>().Select(t => t.Text ?? string.Empty).ToList();
                    Assert.Contains(texts, t => t.Contains(PushBackRearTopeSection.HeadingText, StringComparison.Ordinal));
                    Assert.Contains(texts, t => t.Contains("SAQUE", StringComparison.Ordinal));

                    var button = Descendants(dialog).OfType<Button>()
                        .FirstOrDefault(b => string.Equals(b.Content as string, PushBackRearTopeSection.ConfigureButtonText, StringComparison.Ordinal));
                    Assert.True(button != null, "the section must expose a visible Configurar… button");
                    Assert.True(button.IsVisible);
                }
                finally { dialog.Close(); }
            });
        }

        [Fact]
        public void TheSectionState_ReflectsSaqueAndDeactivatedCells()
        {
            var allActive = new PushBackRearTopeConfig { Saque = 3.0 };
            Assert.Contains("todas las celdas", PushBackRearTopeSection.StatusText(allActive), StringComparison.Ordinal);

            var withOff = new PushBackRearTopeConfig { Saque = 4.25 };
            withOff.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var text = PushBackRearTopeSection.StatusText(withOff);
            Assert.Contains("1 celda", text, StringComparison.Ordinal);
            Assert.Contains("SAQUE", text, StringComparison.Ordinal);
            // The number is formatted for the UI's culture, so assert the digits rather than the separator.
            Assert.Contains("4", text, StringComparison.Ordinal);
            Assert.Contains("25", text, StringComparison.Ordinal);
        }

        [Fact]
        public void TheSection_EditsOnlySaqueAndOffCells_OnAWorkingCopy()
        {
            StaTestRunner.Run(() =>
            {
            var original = new PushBackRearTopeConfig { Saque = 3.0 };
            original.OffCells.Add(new SelectiveGridCell { Frente = 1, Level = 1 });

            var section = new PushBackRearTopeSection(original, _ => new SafetyTopeGridWindow.TopeResult
            {
                Saque = 5.5,
                OffCells = { new SelectiveGridCell { Frente = 0, Level = 0 } }
            });

            section.Configure();

            // The working copy took the edit...
            Assert.Equal(5.5, section.Config.Saque, 9);
            Assert.Single(section.Config.OffCells);
            Assert.Equal(0, section.Config.OffCells[0].Frente);

            // ...and the caller's own config is untouched until the host applies it.
            Assert.Equal(3.0, original.Saque, 9);
            Assert.Single(original.OffCells);
            Assert.Equal(1, original.OffCells[0].Frente);
            });
        }

        [Fact]
        public void CancellingTheTopeGrid_LeavesTheWorkingCopyUntouched()
        {
            StaTestRunner.Run(() =>
            {
            var original = new PushBackRearTopeConfig { Saque = 3.0 };
            var section = new PushBackRearTopeSection(original, _ => null);   // cancelled

            section.Configure();

            Assert.False(section.Edited);
            Assert.Equal(3.0, section.Config.Saque, 9);
            Assert.Empty(section.Config.OffCells);
            });
        }

        // ---- the stop is never ordinary safety ----

        [Fact]
        public void TheStop_NeverBecomesASafetySelection()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    w.RearTopeDialog = _ => new SafetyTopeGridWindow.TopeResult
                    {
                        Saque = 4.0,
                        OffCells = { new SelectiveGridCell { Frente = 0, Level = 0 } }
                    };
                    w.SafetyDialog = _ =>
                    {
                        w.RearTopeSectionForTest.Configure();
                        return Array.Empty<SelectiveSafetySelection>();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    // The stop was applied...
                    Assert.Equal(4.0, w.State.RearTopeSaque, 9);
                    Assert.False(w.State.Cell(0, 0).RearTopeEnabled);

                    // ...and it is nowhere in the safety list, nor in the design's safety.
                    var authority = new RackCad.Application.Systems.PushBack.PushBackSafetyAuthority(w.Session.Catalog);
                    Assert.All(w.SafetySelections, s => Assert.False(authority.IsRearStop(s)));
                    Assert.All(w.LastComputation.Design.Structure.SafetySelections,
                        s => Assert.False(authority.IsRearStop(s)));
                }
                finally { w.Close(); }
            });
        }

        // ---- no unexpected second dialog after Aceptar ----

        [Fact]
        public void NoTopeDialog_OpensByItself_AfterAccepting()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var topeDialogOpened = 0;
                    w.RearTopeDialog = _ => { topeDialogOpened++; return null; };
                    w.SafetyDialog = _ => Array.Empty<SelectiveSafetySelection>();   // accepted, button never pressed

                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(0, topeDialogOpened);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void CancellingTheMainDialog_KeepsBothStates()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var safetyBefore = w.SafetySelections.Select(s => s.ElementId).ToList();
                    var saqueBefore = w.State.RearTopeSaque;
                    var offBefore = w.State.RearTopeConfig().OffCells.Count;

                    // Even if the user edited the stop in the section, cancelling the MAIN dialog discards it.
                    w.RearTopeDialog = _ => new SafetyTopeGridWindow.TopeResult
                    {
                        Saque = 9.0,
                        OffCells = { new SelectiveGridCell { Frente = 0, Level = 0 } }
                    };
                    w.SafetyDialog = _ =>
                    {
                        w.RearTopeSectionForTest.Configure();
                        return null;   // cancelled
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(safetyBefore, w.SafetySelections.Select(s => s.ElementId).ToList());
                    Assert.Equal(saqueBefore, w.State.RearTopeSaque, 9);
                    Assert.Equal(offBefore, w.State.RearTopeConfig().OffCells.Count);
                    Assert.True(w.State.Cell(0, 0).RearTopeEnabled);
                }
                finally { w.Close(); }
            });
        }
    }
}
