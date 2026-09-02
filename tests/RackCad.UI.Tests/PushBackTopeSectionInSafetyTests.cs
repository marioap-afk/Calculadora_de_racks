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
using RackCad.UI.Systems.PushBack;
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
                    w.RearTopeDialog = (_, __) => new SafetyTopeGridWindow.TopeResult
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
                    w.RearTopeDialog = (_, __) => { topeDialogOpened++; return null; };
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
                    w.RearTopeDialog = (_, __) => new SafetyTopeGridWindow.TopeResult
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

        // ---- I-42 (ronda 7B): un rack COMPUESTO edita AQUI los topes de LOS DOS lados -------------------------

        /// <summary>Declara el lado B para que la ventana tenga los dos topes que ofrecer.</summary>
        private static RackPushBackSystemWindow Composite()
        {
            var w = Shown();
            w.CompositeState.SetSideBPresent(true);
            for (var slot = 0; slot < w.CompositeState.SlotCount; slot++)
            {
                w.CompositeState.SetSlotPresent(PushBackSide.B, slot, true);
            }

            return w;
        }

        /// <summary>
        /// I-42 (ronda 7B, decision del dueño) — el tope se edita SOLO aqui, y en un rack compuesto la ventana
        /// ofrece UNA seccion POR LADO. Era la unica capacidad que le faltaba, y por la que existia una segunda
        /// superficie en la ventana principal: ahora la decision se toma en un solo sitio.
        /// </summary>
        [Fact]
        public void RearTope_IsEditedFromSafetyWindow_WithOneSectionPerSide()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.SafetyDialog = _ => null;   // se cancela: solo interesa QUE se ofrece
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.NotNull(w.RearTopeSectionForTest);
                    Assert.NotNull(w.RearTopeSectionBForTest);
                    Assert.NotSame(w.RearTopeSectionForTest, w.RearTopeSectionBForTest);
                    Assert.NotSame(w.RearTopeSectionForTest.Config, w.RearTopeSectionBForTest.Config);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Un rack de un solo sentido sigue ofreciendo UNA seccion, sin etiqueta: nada cambia para el.</summary>
        [Fact]
        public void SingleSidedRack_StillOffersOneSection()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    w.SafetyDialog = _ => null;
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.NotNull(w.RearTopeSectionForTest);
                    Assert.Null(w.RearTopeSectionBForTest);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Editar el tope de A no toca el de B: es el contrato StopA/StopB de las rondas anteriores.</summary>
        [Fact]
        public void EditingStopA_DoesNotChangeStopB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var saqueB = w.CompositeState.Of(PushBackSide.B).RearTopeSaque;

                    // Solo la seccion de A abre su rejilla y cambia su SAQUE.
                    w.RearTopeDialog = (_, __) => new SafetyTopeGridWindow.TopeResult { Saque = 7.0 };
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.Configure();
                        return selections.ToList();   // aceptado
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(7.0, w.CompositeState.Of(PushBackSide.A).RearTopeSaque, 9);
                    Assert.Equal(saqueB, w.CompositeState.Of(PushBackSide.B).RearTopeSaque, 9);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y al reves: editar el de B no toca el de A.</summary>
        [Fact]
        public void EditingStopB_DoesNotChangeStopA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var saqueA = w.CompositeState.Of(PushBackSide.A).RearTopeSaque;

                    w.RearTopeDialog = (_, __) => new SafetyTopeGridWindow.TopeResult { Saque = 5.0 };
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionBForTest.Configure();
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(5.0, w.CompositeState.Of(PushBackSide.B).RearTopeSaque, 9);
                    Assert.Equal(saqueA, w.CompositeState.Of(PushBackSide.A).RearTopeSaque, 9);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cancelar la ventana no persiste NINGUNO de los dos topes.</summary>
        [Fact]
        public void RearTope_CancelIsTransactional_ForBothSides()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var saqueA = w.CompositeState.Of(PushBackSide.A).RearTopeSaque;
                    var saqueB = w.CompositeState.Of(PushBackSide.B).RearTopeSaque;

                    w.RearTopeDialog = (_, __) => new SafetyTopeGridWindow.TopeResult { Saque = 11.0 };
                    w.SafetyDialog = _ =>
                    {
                        w.RearTopeSectionForTest.Configure();
                        w.RearTopeSectionBForTest.Configure();
                        return null;   // cancelado
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(saqueA, w.CompositeState.Of(PushBackSide.A).RearTopeSaque, 9);
                    Assert.Equal(saqueB, w.CompositeState.Of(PushBackSide.B).RearTopeSaque, 9);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y aceptarla persiste los DOS.</summary>
        [Fact]
        public void RearTope_AcceptPersists_ForBothSides()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.RearTopeDialog = (_, __) => new SafetyTopeGridWindow.TopeResult { Saque = 8.0 };
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.Configure();
                        w.RearTopeSectionBForTest.Configure();
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(8.0, w.CompositeState.Of(PushBackSide.A).RearTopeSaque, 9);
                    Assert.Equal(8.0, w.CompositeState.Of(PushBackSide.B).RearTopeSaque, 9);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// MOVER el control no cambia la geometria ni el BOM: la fisica del tope es la de las rondas 4B, 5 y 5D y
        /// esta ronda no la toca. Sin editar nada, el dibujo es identico antes y despues de abrir y cancelar.
        /// </summary>
        [Fact]
        public void MovingControl_DoesNotChangeTopeGeometryOrBom()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Topes(w);
                    w.SafetyDialog = _ => null;
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(before, Topes(w));
                }
                finally { w.Close(); }
            });
        }

        private static IReadOnlyList<string> Topes(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return new List<string>();
            }

            return new PushBackSystemPlantaBuilder().BuildPlan(system, w.Session.Catalog).Flatten().Instances
                .Where(instance => instance.Role == RackCad.Application.Drawing.HeaderBlockRole.Tope)
                .Select(instance => FormattableString.Invariant(
                    $"{instance.PieceId}|{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}|{instance.MirroredX}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }
    }
}
