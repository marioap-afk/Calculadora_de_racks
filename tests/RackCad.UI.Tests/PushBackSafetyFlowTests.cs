using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Owner decision (2026-07-24), item 1 — the INTEGRATED Seguridad flow, exercised through the REAL "Configurar
    /// seguridad…" button path (click → dialog → authority → state → recompute → design/BOM), not by poking the adapter.
    /// The two modal dialogs are replaced by seams, so the whole handler runs unchanged while the test decides what the
    /// user "chose" or cancels.
    /// </summary>
    public sealed class PushBackSafetyFlowTests
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

        /// <summary>A catalog element of a given safety type, or null when this host ships no catalog / no such family.</summary>
        private static SafetyElementCatalogEntry ElementOfType(RackPushBackSystemWindow w, string type)
            => w.Session.Catalog?.SafetyElements?
                .FirstOrDefault(e => e != null && SelectiveSafetyDefaults.IsType(e.Type, type));

        private static int BomQuantityFor(RackPushBackSystemWindow w, string elementId)
        {
            var system = w.LastComputation?.System;
            if (system == null) return 0;
            return PushBackBomBuilder.Build(system, w.Session.Catalog).Lines
                .Where(line => string.Equals(line.ProfileId, elementId, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);
        }

        // ---- the accepted safety actually reaches the design and the BOM ----

        [Fact]
        public void AcceptingSafety_ThroughTheRealButton_ChangesTheDesignAndTheBom()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // Pick real low-end families the Owner listed: bota, protector lateral, desviador, defensa.
                    var families = new[]
                    {
                        SelectiveSafetyDefaults.BotaType,
                        SelectiveSafetyDefaults.LateralType,
                        SelectiveSafetyDefaults.DesviadorType,
                        SelectiveSafetyDefaults.DefensaType,
                    };
                    var chosen = families
                        .Select(type => ElementOfType(w, type))
                        .Where(element => element != null)
                        .Select(element => new SelectiveSafetySelection { ElementId = element.Id, Quantity = 7 })
                        .ToList();
                    if (chosen.Count == 0)
                    {
                        return;   // no catalog on this host: the pure suite covers the families
                    }

                    // The seeded defaults already carry these families, so identity alone would not prove anything:
                    // the QUANTITY is what distinguishes "the dialog's result was applied" from "nothing happened".
                    var quantitiesBefore = w.SafetySelections.Select(s => s.Quantity).ToList();
                    Assert.DoesNotContain(7, quantitiesBefore);

                    // Drive the REAL button: the handler reads the dialog's RESULT, authorizes it and recomputes.
                    w.SafetyDialog = _ => chosen;
                    w.RearTopeDialog = _ => null;   // leave the tope untouched in this test
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    // The window's stored selections are now exactly the authorized chosen ones...
                    var after = w.SafetySelections.Select(s => s.ElementId).ToList();
                    Assert.All(chosen, c => Assert.Contains(c.ElementId, after));
                    Assert.All(w.SafetySelections, s => Assert.Equal(7, s.Quantity));   // the chosen quantity arrived

                    // ...and they are INDEPENDENT copies, not the dialog's own objects.
                    Assert.All(w.SafetySelections, stored =>
                        Assert.DoesNotContain(chosen, dialogObject => ReferenceEquals(dialogObject, stored)));

                    // The DESIGN carries them (the recompute ran through the real path)...
                    var design = w.LastComputation?.Design;
                    Assert.NotNull(design);
                    Assert.All(chosen, c => Assert.Contains(
                        design.Structure.SafetySelections,
                        s => string.Equals(s.ElementId, c.ElementId, StringComparison.OrdinalIgnoreCase) && s.Quantity == 7));

                    // ...and so does the BOM: at least one chosen family is actually counted.
                    Assert.Contains(chosen, c => BomQuantityFor(w, c.ElementId) > 0);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TheExcludedFamilies_NeverSurvive_EvenIfTheDialogReturnedThem()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var excluded = new[]
                    {
                        SelectiveSafetyDefaults.GuiaType,
                        SelectiveSafetyDefaults.ParrillaType,
                        SelectiveSafetyDefaults.TopeType,
                    }
                        .Select(type => ElementOfType(w, type))
                        .Where(element => element != null)
                        .Select(element => new SelectiveSafetySelection { ElementId = element.Id, Quantity = 1 })
                        .ToList();
                    if (excluded.Count == 0)
                    {
                        return;
                    }

                    w.SafetyDialog = _ => excluded;
                    w.RearTopeDialog = _ => null;
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    // The authority stripped every one of them on the real path.
                    Assert.All(excluded, e => Assert.DoesNotContain(
                        w.SafetySelections, s => string.Equals(s.ElementId, e.ElementId, StringComparison.OrdinalIgnoreCase)));

                    var design = w.LastComputation?.Design;
                    Assert.NotNull(design);
                    Assert.All(excluded, e => Assert.DoesNotContain(
                        design.Structure.SafetySelections,
                        s => string.Equals(s.ElementId, e.ElementId, StringComparison.OrdinalIgnoreCase)));
                }
                finally { w.Close(); }
            });
        }

        // ---- cancelling has an explicit, tested meaning on BOTH dialogs ----

        [Fact]
        public void CancellingTheSafetyDialog_ChangesNothing_NeitherSafetyNorTope()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var safetyBefore = w.SafetySelections.Select(s => s.ElementId).ToList();
                    var saqueBefore = w.State.RearTopeSaque;
                    var offBefore = w.State.RearTopeConfig().OffCells.Count;

                    var topeDialogOpened = false;
                    w.SafetyDialog = _ => null;                     // cancelled
                    w.RearTopeDialog = _ => { topeDialogOpened = true; return null; };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.False(topeDialogOpened, "cancelling safety must abandon the whole Seguridad step");
                    Assert.Equal(safetyBefore, w.SafetySelections.Select(s => s.ElementId).ToList());
                    Assert.Equal(saqueBefore, w.State.RearTopeSaque, 9);
                    Assert.Equal(offBefore, w.State.RearTopeConfig().OffCells.Count);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void CancellingOnlyTheTopeDialog_KeepsTheAcceptedSafety_AndLeavesTheTopeUntouched()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var bota = ElementOfType(w, SelectiveSafetyDefaults.BotaType);
                    if (bota == null)
                    {
                        return;
                    }

                    var saqueBefore = w.State.RearTopeSaque;
                    var offBefore = w.State.RearTopeConfig().OffCells.Count;

                    w.SafetyDialog = _ => new[] { new SelectiveSafetySelection { ElementId = bota.Id, Quantity = 3 } };
                    w.RearTopeDialog = _ => null;                   // only the TOPE dialog is cancelled
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    // The safety the user accepted survives...
                    Assert.Contains(w.SafetySelections, s => string.Equals(s.ElementId, bota.Id, StringComparison.OrdinalIgnoreCase));
                    // ...and the tope config is exactly as it was.
                    Assert.Equal(saqueBefore, w.State.RearTopeSaque, 9);
                    Assert.Equal(offBefore, w.State.RearTopeConfig().OffCells.Count);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void AcceptingTheDialog_AppliesSafetyAndTheRearTope_InOnePass()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var bota = ElementOfType(w, SelectiveSafetyDefaults.BotaType);
                    if (bota == null)
                    {
                        return;
                    }

                    // Owner decision (2026-07-24, final): the tope grid opens ONLY from its own visible button inside
                    // the safety dialog. The safety seam stands in for the user pressing that button and then Aceptar.
                    w.RearTopeDialog = _ => new SafetyTopeGridWindow.TopeResult
                    {
                        Saque = 4.75,
                        OffCells = { new SelectiveGridCell { Frente = 0, Level = 0 } }
                    };
                    w.SafetyDialog = _ =>
                    {
                        w.RearTopeSectionForTest.Configure();   // the user presses "Configurar…"
                        return new[] { new SelectiveSafetySelection { ElementId = bota.Id, Quantity = 1 } };
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Contains(w.SafetySelections, s => string.Equals(s.ElementId, bota.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.Equal(4.75, w.State.RearTopeSaque, 9);
                    Assert.False(w.State.Cell(0, 0).RearTopeEnabled);

                    var design = w.LastComputation?.Design;
                    Assert.NotNull(design);
                    Assert.Equal(4.75, design.RearTope.Saque, 9);
                    Assert.False(design.RearTope.At(0, 0));
                }
                finally { w.Close(); }
            });
        }
    }
}
