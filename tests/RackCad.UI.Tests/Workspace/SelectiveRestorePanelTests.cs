#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, Selectivo entrega 2, P2: the screen of «Restaurar layout…» (decision 8 of the Owner; <c>sprint\spec-restaurar.md</c>).
    /// The button of the head opens a panel inline with the six boxes, every one checked when it opens, how many values each removes («(ninguno)»
    /// and disabled with nothing), «Ver lo que se pierde» folded with the lines of the checked boxes, and «Restaurar lo marcado» / «Cancelar».
    /// Provisional supuesto of the brief: it restores the WHOLE rack, whatever the fondos of the view are (the approved draft), and the switch to
    /// the selector of fondos is one value (<see cref="SelectiveEditorViewModel.RestoreFollowsFondoView"/>).
    /// </summary>
    public class SelectiveRestorePanelTests
    {
        private const string Tail = "«Descartar» vuelve a lo último que se leyó o se escribió.";

        // One fondo with the six personalisations: two manual lengths (F1·N1, F1·N2), a manual clear (F2·N2), F1 split in two tramos, a
        // manual height on F2, the own peralte of P1 (Structure has it) and a custom cabecera on P2.
        private static SelectivePalletDesign Personalised()
        {
            var design = Structure();
            design.Bays[0].Levels[0].BeamLengthOverride = 100.0;
            design.Bays[0].Levels[1].BeamLengthOverride = 100.0;
            design.Bays[1].Levels[1].ClearOverride = 80.0;
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 40.0, Loaded = true });
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            design.Bays[1].HeightOverride = 200.0;
            foreach (var cabecera in new RackFrameConfiguration?[] { null, Custom(120.0), null }) design.PostCabeceras.Add(cabecera!);
            return design;
        }

        // Two fondos: a manual length in each (F1·N2), the own peralte of P1 and the custom cabecera of P1 of fondo 2.
        private static SelectivePalletDesign TwoFondosPersonalised()
        {
            var design = TwoFondosWithACustomCabeceraOutOfItsRule();
            design.Bays[0].Levels[1].BeamLengthOverride = 100.0;
            design.ExtraFondoBays[0][0].Levels[1].BeamLengthOverride = 100.0;
            return design;
        }

        private static SelectiveRestoreBoxItem Box(SelectiveEditorViewModel vm, SelectiveLayoutRestoreBox box) => vm.RestoreBoxes.Single(b => b.Box == box);

        // ------------------------------------------------------------------ the panel (the view-model)

        [Fact]
        public void ThePanelOpensWithEveryBoxCheckedHowManyEachRemovesAndWhatIsLostFolded()
        {
            var vm = SelectiveEditor(Personalised());
            Assert.False(vm.IsRestoreOpen);

            vm.OpenRestore();

            Assert.True(vm.IsRestoreOpen);
            Assert.Equal(new[]
            {
                "Largo de larguero (2)", "Claro (1)", "Medio frente (1)", "Altura manual (1)", "Peraltes propios (1)", "Cabeceras personalizadas (1)",
            }, vm.RestoreBoxes.Select(b => b.Label).ToArray());
            Assert.All(vm.RestoreBoxes, b => Assert.True(b.IsChecked && b.IsEnabled));
            Assert.Equal("Vuelve a la geometría que sale de las tarimas en el rack. Marca lo que quieres restaurar:", vm.RestoreIntro);
            Assert.False(vm.IsRestoreLossesExpanded);
            Assert.Equal(new[]
            {
                "Medio frente en F1 (2 tramos)",
                "Largo manual de larguero en F1 · N1 (100 in)",
                "Largo manual de larguero en F1 · N2 (100 in)",
                "Altura manual de cabecera en F2 (200 in)",
                "Claro manual en F2 · N2 (80 in)",
                "Peralte propio en P1 (5 in)",
                "Cabecera personalizada en P2",
            }, vm.RestoreLosses.ToArray());
            Assert.Equal(string.Empty, vm.RestorePeralteNote);
            Assert.True(vm.CanConfirmRestore);

            // Opening it writes nothing.
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void ABoxWithNothingToRemoveSaysNingunoAndIsDisabledButChecked()
        {
            // Structure has only the own peralte of P1.
            var vm = SelectiveEditor();
            vm.OpenRestore();

            Assert.Equal("Largo de larguero (ninguno)", Box(vm, SelectiveLayoutRestoreBox.BeamLength).Label);
            Assert.Equal("Peraltes propios (1)", Box(vm, SelectiveLayoutRestoreBox.PostPeralte).Label);
            Assert.All(vm.RestoreBoxes.Where(b => b.Box != SelectiveLayoutRestoreBox.PostPeralte), b => Assert.False(b.IsEnabled));
            Assert.All(vm.RestoreBoxes, b => Assert.True(b.IsChecked));
            Assert.Equal(new[] { "Peralte propio en P1 (5 in)" }, vm.RestoreLosses.ToArray());

            // Unchecking the only box with something leaves nothing to restore: «Restaurar lo marcado» is disabled and does nothing.
            Box(vm, SelectiveLayoutRestoreBox.PostPeralte).IsChecked = false;
            Assert.False(vm.CanConfirmRestore);
            Assert.False(vm.HasRestoreLosses);
            vm.ConfirmRestore();
            Assert.True(vm.IsRestoreOpen);
            Assert.Equal(5.0, vm.Draft.PostPeralteOverride(0));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void WithNothingManualThePanelDoesNotOpenAndTheEditorSaysSo()
        {
            var plain = Structure();
            plain.PostPeraltes[0] = 0.0;
            var vm = SelectiveEditor(plain);

            vm.OpenRestore();

            Assert.False(vm.IsRestoreOpen);
            Assert.Equal("No hay nada manual que quitar: el layout ya es el que sale de las tarimas.", vm.Message);
            Assert.False(vm.MessageIsError);
        }

        [Fact]
        public void UncheckingABoxTakesItsLinesOutAndRestoringKeepsItWithTheExactNotice()
        {
            var vm = SelectiveEditor(Personalised());
            var plainJson = Json(SelectiveEditor(PlainOf(Personalised())).Draft.Design);
            vm.ClickCell(1, 1, control: false, shift: false);
            Assert.Contains(vm.Rows.SelectMany(r => r.Cells), c => c.IsManual);
            vm.OpenRestore();

            Box(vm, SelectiveLayoutRestoreBox.CustomCabecera).IsChecked = false;
            Assert.DoesNotContain("Cabecera personalizada en P2", vm.RestoreLosses);
            Assert.Equal(6, vm.RestoreLosses.Count);
            vm.ConfirmRestore();

            Assert.False(vm.IsRestoreOpen);
            Assert.Equal("Layout restaurado en el borrador: 6 valores manuales quitados (2 largos de larguero, 1 claro, 1 medio frente, 1 altura manual, "
                         + "1 peralte propio). " + Tail + " Se conservan: 1 cabecera personalizada.", vm.Message);
            Assert.False(vm.MessageIsError);
            Assert.True(vm.IsDirty);
            Assert.Equal("Modificado", vm.StatusText);
            Assert.True(vm.Draft.HasCustomCabecera(0, 1));
            Assert.Null(vm.Draft.Cell(0, 0, 0)!.BeamLength);
            Assert.Null(vm.Draft.Cell(0, 1, 1)!.Clear);
            Assert.False(vm.Draft.Front(0, 0)!.IsSplit);
            Assert.Null(vm.Draft.Front(0, 1)!.HeightOverride);
            Assert.Null(vm.Draft.PostPeralteOverride(0));
            // The cells lose their mark «M» and the selection is kept.
            Assert.DoesNotContain(vm.Rows.SelectMany(r => r.Cells), c => c.IsManual);
            Assert.Equal(new[] { P(1, 1) }, vm.SelectedPositions);

            // With every box checked it is the same rack without its personalisations.
            var all = SelectiveEditor(Personalised());
            all.OpenRestore();
            all.ConfirmRestore();
            Assert.Equal("Layout restaurado en el borrador: 7 valores manuales quitados (2 largos de larguero, 1 claro, 1 medio frente, 1 altura manual, "
                         + "1 peralte propio, 1 cabecera personalizada). " + Tail, all.Message);
            Assert.Equal(plainJson, Json(all.Draft.Design));
        }

        private static SelectivePalletDesign PlainOf(SelectivePalletDesign design)
        {
            foreach (var bay in design.Bays)
            {
                bay.Segments.Clear();
                bay.HeightOverride = null;
                foreach (var cell in bay.Levels)
                {
                    cell.BeamLengthOverride = null;
                    cell.ClearOverride = null;
                }
            }

            for (var i = 0; i < design.PostPeraltes.Count; i++) design.PostPeraltes[i] = 0.0;
            design.PostCabeceras.Clear();
            return design;
        }

        [Fact]
        public void OneValueIsSaidInTheSingularAndDiscardGoesBackToWhatWasRead()
        {
            var vm = SelectiveEditor();
            var read = Json(vm.Draft.Design);
            vm.OpenRestore();
            vm.ConfirmRestore();
            Assert.Equal("Layout restaurado en el borrador: 1 valor manual quitado (1 peralte propio). " + Tail, vm.Message);
            Assert.Equal("Modificado", vm.StatusText);

            vm.Discard();
            Assert.Equal(read, Json(vm.Draft.Design));
            Assert.Equal(5.0, vm.Draft.PostPeralteOverride(0));
            Assert.Equal("Limpio", vm.StatusText);
        }

        [Fact]
        public void ForANewSelectivoTheNoticeSaysThatDiscardGoesBackToTheDefaults()
        {
            // M-4 of the review of the entrega 2: a new Selectivo of «Nuevo» was never read nor written, and its «Descartar» goes back to the
            // defaults of the classic editor (P3-05), so the notices say that, not «lo último que se leyó o se escribió».
            const string NewTail = "«Descartar» vuelve a los valores por defecto.";
            var vm = SelectiveEditorViewModel.ForNewRack(SelectiveDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "nuevo-1");
            var defaults = Json(vm.Draft.Design);
            Write(Field(vm.FrontFields, "fHeight"), "200");
            Assert.Equal(200.0, vm.Draft.Front(0, 0)!.HeightOverride);

            vm.OpenRestore();
            Assert.True(vm.IsRestoreOpen, vm.Message);
            vm.ConfirmRestore();
            Assert.Equal("Layout restaurado en el borrador: 1 valor manual quitado (1 altura manual). " + NewTail, vm.Message);
            Assert.DoesNotContain(Tail, vm.Message);

            vm.Discard();
            Assert.Equal(defaults, Json(vm.Draft.Design));
            Assert.Equal("Borrador descartado.", vm.Message);

            // The same for what reducing the fronts drops (S-09): an own peralte of a post that no longer exists.
            Write(vm.PostRows[2].Field, "4.5");
            Assert.Equal(4.5, vm.Draft.PostPeralteOverride(2));
            Write(Field(vm.Essentials, "fronts"), "1");
            Assert.Equal("Al reducir los frentes se quitaron 1 peralte propio de los postes que ya no existen, como en el editor clásico. "
                         + "Volver a subirlos no los recupera; " + NewTail, vm.Message);

            // Inserted, it is a rack of the drawing: «Descartar» goes back to what was written again.
            vm.MarkInserted("Selectivo 1");
            Write(Field(vm.FrontFields, "fHeight"), "200");
            vm.OpenRestore();
            vm.ConfirmRestore();
            Assert.EndsWith(Tail, vm.Message);
        }

        [Fact]
        public void EveryOpeningStartsWithEveryBoxCheckedAndWhatIsLostFoldedAndCancelChangesNothing()
        {
            var vm = SelectiveEditor(Personalised());
            vm.OpenRestore();
            Box(vm, SelectiveLayoutRestoreBox.BeamLength).IsChecked = false;
            vm.IsRestoreLossesExpanded = true;
            vm.CancelRestore();
            Assert.False(vm.IsRestoreOpen);
            Assert.False(vm.IsDirty);
            Assert.Equal(100.0, vm.Draft.Cell(0, 0, 0)!.BeamLength);

            vm.OpenRestore();
            Assert.All(vm.RestoreBoxes, b => Assert.True(b.IsChecked));
            Assert.False(vm.IsRestoreLossesExpanded);
            Assert.Equal(7, vm.RestoreLosses.Count);
        }

        [Fact]
        public void TheOpenPanelFollowsTheDraftAndKeepsTheBoxesTheUserChose()
        {
            var vm = SelectiveEditor(Personalised());
            vm.OpenRestore();
            Box(vm, SelectiveLayoutRestoreBox.BeamLength).IsChecked = false;

            // The own peralte of P1 emptied while the panel is open: its box has nothing now.
            Write(vm.PostRows[0].Field, string.Empty);

            Assert.True(vm.IsRestoreOpen);
            Assert.Equal("Peraltes propios (ninguno)", Box(vm, SelectiveLayoutRestoreBox.PostPeralte).Label);
            Assert.False(Box(vm, SelectiveLayoutRestoreBox.PostPeralte).IsEnabled);
            Assert.False(Box(vm, SelectiveLayoutRestoreBox.BeamLength).IsChecked);
            Assert.DoesNotContain(vm.RestoreLosses, l => l.StartsWith("Peralte propio", StringComparison.Ordinal));
            Assert.DoesNotContain(vm.RestoreLosses, l => l.StartsWith("Largo manual", StringComparison.Ordinal));

            // «Descartar» brings the peralte back, and with nothing left the panel closes.
            vm.Discard();
            Assert.True(vm.IsRestoreOpen);
            Assert.Equal("Peraltes propios (1)", Box(vm, SelectiveLayoutRestoreBox.PostPeralte).Label);
            var single = SelectiveEditor();
            single.OpenRestore();
            Write(single.PostRows[0].Field, string.Empty);
            Assert.False(single.IsRestoreOpen);
        }

        [Fact]
        public void TheQuestionOfClosingAndThePanelCloseEachOther()
        {
            var vm = SelectiveEditor(Personalised());
            Write(Field(vm.CellFields, "cHeight"), "72");
            vm.OpenRestore();
            Assert.True(vm.IsRestoreOpen);

            vm.RequestClose();
            Assert.True(vm.IsConfirmingClose);
            Assert.False(vm.IsRestoreOpen);

            vm.OpenRestore();
            Assert.True(vm.IsRestoreOpen);
            Assert.False(vm.IsConfirmingClose);
        }

        [Fact]
        public void WhileAutoCadWritesThePanelClosesAndDoesNotOpen()
        {
            var vm = SelectiveEditor(Personalised());
            Write(Field(vm.CellFields, "cHeight"), "72");
            vm.OpenRestore();

            vm.BeginWriting();
            Assert.False(vm.IsRestoreOpen);
            Assert.False(vm.CanConfirmRestore);
            vm.OpenRestore();
            Assert.False(vm.IsRestoreOpen);
            Assert.True(vm.MessageIsError);
            Assert.Equal(vm.WritingText, vm.Message);

            vm.EndWriting();
            vm.OpenRestore();
            Assert.True(vm.IsRestoreOpen);
        }

        [Fact]
        public void AValueWrittenAndRefusedStopsItAndTheEditorNamesIt()
        {
            var vm = SelectiveEditor(Personalised());
            vm.OpenRestore();
            var height = Field(vm.CellFields, "cHeight");
            Write(height, "abc");
            Assert.True(vm.IsRestoreOpen);

            vm.ConfirmRestore();

            const string Refused = "No se puede restaurar el layout: «Alto de tarima» tiene un valor que no se aplicó (El alto de tarima debe ser un número mayor que "
                                   + "cero). Corrígelo o vuelve a escribir el que tenía.";
            Assert.Equal(Refused, vm.Message);
            Assert.True(vm.MessageIsError);
            Assert.True(vm.IsRestoreOpen);
            Assert.Equal(100.0, vm.Draft.Cell(0, 0, 0)!.BeamLength);
            Assert.Equal("abc", height.Text);

            vm.CancelRestore();
            vm.OpenRestore();
            Assert.False(vm.IsRestoreOpen);
            Assert.Equal(Refused, vm.Message);

            Write(height, "60");
            vm.OpenRestore();
            Assert.True(vm.IsRestoreOpen);
        }

        // ------------------------------------------------------------------ the scope (provisional supuesto: the whole rack)

        [Fact]
        public void ItRestoresTheWholeRackWhateverTheFondosOfTheViewAre()
        {
            var vm = SelectiveEditor(TwoFondosPersonalised());
            vm.FondoScopeIndex = 2; // «Fondo 2» over the matrix
            vm.OpenRestore();

            Assert.Equal("Vuelve a la geometría que sale de las tarimas en todos los fondos. Marca lo que quieres restaurar:", vm.RestoreIntro);
            Assert.Equal("Largo de larguero (2)", Box(vm, SelectiveLayoutRestoreBox.BeamLength).Label);
            Assert.True(Box(vm, SelectiveLayoutRestoreBox.PostPeralte).IsEnabled);
            Assert.Equal(new[]
            {
                "Largo manual de larguero en Fondo 1 · F1 · N2 (100 in)",
                "Largo manual de larguero en Fondo 2 · F1 · N2 (100 in)",
                "Peralte propio en P1 (5 in)",
                "Cabecera personalizada en Fondo 2 · P1",
            }, vm.RestoreLosses.ToArray());

            vm.ConfirmRestore();
            Assert.Equal("Layout restaurado en el borrador: 4 valores manuales quitados (2 largos de larguero, 1 peralte propio, 1 cabecera personalizada). " + Tail,
                vm.Message);
            Assert.Null(vm.Draft.Cell(0, 0, 1)!.BeamLength);
            Assert.Null(vm.Draft.Cell(1, 0, 1)!.BeamLength);
            Assert.Null(vm.Draft.PostPeralteOverride(0));
            Assert.False(vm.Draft.HasCustomCabecera(1, 0));
            // The view of the matrix is not changed.
            Assert.Equal(2, vm.FondoScopeIndex);
            Assert.Equal(1, vm.Draft.FondoScope);
        }

        [Fact]
        public void FollowingTheSelectorIsOneValueAndThenTheOwnPeraltesWaitForAllTheFondos()
        {
            var vm = SelectiveEditor(TwoFondosPersonalised());
            vm.RestoreFollowsFondoView = true;
            vm.FondoScopeIndex = 2;
            vm.OpenRestore();

            Assert.Equal("Vuelve a la geometría que sale de las tarimas en el fondo 2. Marca lo que quieres restaurar:", vm.RestoreIntro);
            Assert.Equal("Largo de larguero (1)", Box(vm, SelectiveLayoutRestoreBox.BeamLength).Label);
            var peraltes = Box(vm, SelectiveLayoutRestoreBox.PostPeralte);
            Assert.Equal("Peraltes propios (1)", peraltes.Label);
            Assert.False(peraltes.IsChecked);
            Assert.False(peraltes.IsEnabled);
            Assert.Equal("Los peraltes propios son de todo el rack: elige «Todos los fondos» para restaurarlos.", vm.RestorePeralteNote);
            Assert.Equal(new[] { "Largo manual de larguero en Fondo 2 · F1 · N2 (100 in)", "Cabecera personalizada en Fondo 2 · P1" }, vm.RestoreLosses.ToArray());

            vm.ConfirmRestore();
            Assert.Equal("Layout restaurado en el borrador: 2 valores manuales quitados (1 largo de larguero, 1 cabecera personalizada). " + Tail, vm.Message);
            Assert.Equal(100.0, vm.Draft.Cell(0, 0, 1)!.BeamLength);
            Assert.Null(vm.Draft.Cell(1, 0, 1)!.BeamLength);
            Assert.Equal(5.0, vm.Draft.PostPeralteOverride(0));
            Assert.False(vm.Draft.HasCustomCabecera(1, 0));

            // With «Todos los fondos» the box of the own peraltes is back, checked.
            vm.FondoScopeIndex = 0;
            vm.OpenRestore();
            Assert.True(Box(vm, SelectiveLayoutRestoreBox.PostPeralte).IsChecked);
            Assert.True(Box(vm, SelectiveLayoutRestoreBox.PostPeralte).IsEnabled);
            Assert.Equal(string.Empty, vm.RestorePeralteNote);
        }

        // ------------------------------------------------------------------ the view (real WPF)

        private sealed class BindingErrors : TraceListener
        {
            public List<string> Messages { get; } = new();

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
                if (message != null) Messages.Add(message);
            }
        }

        private static void Lay(FrameworkElement view, double width = 1200, double height = 900)
        {
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
            view.UpdateLayout();
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
        {
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

        [Fact]
        public void TheButtonOpensThePanelItsBoxesWriteTheChoiceAndRestoreSaysItWithNoBindingError()
        {
            StaTestRunner.Run(() =>
            {
                var errors = new BindingErrors();
                PresentationTraceSources.Refresh();
                var source = PresentationTraceSources.DataBindingSource;
                var level = source.Switch.Level;
                source.Listeners.Add(errors);
                source.Switch.Level = SourceLevels.Error;
                try
                {
                    var vm = SelectiveEditor(Personalised());
                    var view = new SelectiveEditorView(vm);
                    Lay(view);
                    Assert.Equal(Visibility.Collapsed, view.RestorePanel.Visibility);
                    Assert.True(view.RestoreButton.IsEnabled);
                    Assert.Equal("Restaurar layout…", view.RestoreButton.Content);
                    Assert.Equal("Restaurar el layout estándar", Name(view.RestoreButton));

                    Click(view.RestoreButton);
                    Lay(view);
                    Assert.Equal(Visibility.Visible, view.RestorePanel.Visibility);
                    Assert.Equal("¿Restaurar el layout?", view.RestoreTitleText.Text);
                    Assert.Equal(vm.RestoreIntro, view.RestoreIntroText.Text);
                    var boxes = Descendants<CheckBox>(view.RestoreBoxList).ToList();
                    Assert.Equal(6, boxes.Count);
                    Assert.All(boxes, b => Assert.True(b.IsChecked == true && b.IsEnabled));
                    Assert.Equal(vm.RestoreBoxes.Select(b => b.Label).ToArray(), boxes.Select(Name).ToArray());
                    Assert.False(view.RestoreLossExpander.IsExpanded);
                    Assert.True(view.RestoreConfirmButton.IsEnabled);

                    // A box unchecked in the view is the choice of the view-model; the lines of what is lost follow it.
                    boxes[5].IsChecked = false;
                    Assert.False(Box(vm, SelectiveLayoutRestoreBox.CustomCabecera).IsChecked);
                    view.RestoreLossExpander.IsExpanded = true;
                    Lay(view);
                    Assert.True(vm.IsRestoreLossesExpanded);
                    var lines = Descendants<TextBlock>(view.RestoreLossList).Select(t => t.Text).Where(t => t != "•").ToList();
                    Assert.Equal(vm.RestoreLosses.ToArray(), lines.ToArray());
                    Assert.Equal(6, lines.Count);

                    // The names of what the head and the panel offer are unique (the two «Cancelar» are told apart).
                    var named = new UIElement[]
                    {
                        view.UpdateButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.CloseYesButton, view.CloseNoButton,
                        view.RestoreConfirmButton, view.RestoreCancelButton,
                    }.Concat(boxes).Select(Name).ToList();
                    Assert.Equal(named.Count, named.Distinct().Count());
                    Assert.Equal("Cancelar la restauración del layout", Name(view.RestoreCancelButton));

                    Click(view.RestoreConfirmButton);
                    Lay(view);
                    Assert.Equal(Visibility.Collapsed, view.RestorePanel.Visibility);
                    Assert.StartsWith("Layout restaurado en el borrador: 6 valores manuales quitados", view.MessageText.Text);
                    Assert.Equal("Modificado", view.StatusText.Text);
                    Assert.True(vm.Draft.HasCustomCabecera(0, 1));

                    // «Descartar» goes back to what was read.
                    Click(view.DiscardButton);
                    Lay(view);
                    Assert.Equal("Limpio", view.StatusText.Text);
                    Assert.Equal(100.0, vm.Draft.Cell(0, 0, 0)!.BeamLength);

                    // «Cancelar» closes the panel.
                    Click(view.RestoreButton);
                    Lay(view);
                    Assert.Equal(Visibility.Visible, view.RestorePanel.Visibility);
                    Click(view.RestoreCancelButton);
                    Lay(view);
                    Assert.Equal(Visibility.Collapsed, view.RestorePanel.Visibility);
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }

                Assert.Empty(errors.Messages);
            });
        }

        [Fact]
        public void TheViewShowsABoxWithNothingDisabledTheRestoreDisabledWithNothingCheckedAndTheButtonNeedsAnEditableDraft()
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor();
                var view = new SelectiveEditorView(vm);
                Lay(view);
                Click(view.RestoreButton);
                Lay(view);

                var boxes = Descendants<CheckBox>(view.RestoreBoxList).ToList();
                var length = boxes.Single(b => (string)b.Content == "Largo de larguero (ninguno)");
                Assert.False(length.IsEnabled);
                Assert.True(length.IsChecked);
                var peraltes = boxes.Single(b => (string)b.Content == "Peraltes propios (1)");
                Assert.True(peraltes.IsEnabled);
                peraltes.IsChecked = false;
                Assert.False(view.RestoreConfirmButton.IsEnabled);
                Assert.Equal(Visibility.Collapsed, view.RestoreLossExpander.Visibility);

                // While AutoCAD writes, the button is disabled and the panel is closed.
                Write(Field(vm.CellFields, "cHeight"), "72");
                vm.BeginWriting();
                Lay(view);
                Assert.False(view.RestoreButton.IsEnabled);
                Assert.Equal(Visibility.Collapsed, view.RestorePanel.Visibility);

                // A rack that cannot be edited (it did not recompose when it opened) does not offer it.
                var broken = Structure();
                broken.PostPeralte = 0.0;
                var draft = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
                Assert.False(draft.LoadFrom(broken, null, "Selectivo A"));
                var readOnly = new SelectiveEditorView(new SelectiveEditorViewModel(draft, RackId, _ => { }));
                Lay(readOnly);
                Assert.False(readOnly.RestoreButton.IsEnabled);
            });
        }

        [Fact]
        public void TheViewSaysThereIsNothingToRestoreWithoutOpeningThePanel()
        {
            StaTestRunner.Run(() =>
            {
                var plain = Structure();
                plain.PostPeraltes[0] = 0.0;
                var vm = SelectiveEditor(plain);
                var view = new SelectiveEditorView(vm);
                Lay(view);

                Click(view.RestoreButton);
                Lay(view);

                Assert.Equal(Visibility.Collapsed, view.RestorePanel.Visibility);
                Assert.Equal("No hay nada manual que quitar: el layout ya es el que sale de las tarimas.", view.MessageText.Text);
            });
        }
    }
}
