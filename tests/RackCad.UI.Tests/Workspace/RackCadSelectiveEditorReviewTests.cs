#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, Selectivo: the corrections of the independent review of the entrega 1 (I-01, M-03, M-04, M-05, M-07, M-10, M-11),
    /// on the editor of the window. The probes of the review (P1, P2, P3, P8, P9) became these tests. Generic sample racks, no data of a
    /// client.
    /// </summary>
    public class RackCadSelectiveEditorReviewTests
    {
        private const string PendingText = "Aplica los tramos con «Aplicar tramos» o descártalos antes de escribir.";

        // ------------------------------------------------------------------ I-01: «Larguero a piso en N1» that says «varía» (S-20)

        [Fact]
        public void AFloorBeamThatVariesSaysSoAndOneClickPutsItInEveryFrontOfTheSelection()
        {
            var vm = SelectiveEditor();
            Assert.Equal("Larguero a piso en N1", vm.SelectionFloorBeamLabel);

            // F1 has «Larguero a piso», F2 does not: «Todo» shows the box indeterminate, with the label of the approved draft.
            vm.SelectAll();
            Assert.Null(vm.SelectionFloorBeam);
            Assert.Equal("Larguero a piso en N1 (varía; marcar lo pone en todos)", vm.SelectionFloorBeamLabel);

            // WPF turns an indeterminate box into false: here that means «all», never «none».
            vm.SelectionFloorBeam = false;
            Assert.True(vm.Draft.Front(0, 0)!.FloorBeam);
            Assert.True(vm.Draft.Front(0, 1)!.FloorBeam);
            Assert.True(vm.SelectionFloorBeam);
            Assert.Equal("Larguero a piso en N1", vm.SelectionFloorBeamLabel);
            Assert.True(vm.IsDirty);

            // From «all», the next click takes it out of every front.
            vm.SelectionFloorBeam = false;
            Assert.False(vm.Draft.Front(0, 0)!.FloorBeam);
            Assert.False(vm.Draft.Front(0, 1)!.FloorBeam);

            // Mixed again (only F1): true also puts it in all, and «no value» changes nothing.
            vm.ClickCell(0, 0, control: false, shift: false);
            vm.SelectionFloorBeam = true;
            vm.SelectAll();
            Assert.Null(vm.SelectionFloorBeam);
            vm.SelectionFloorBeam = null;
            Assert.Null(vm.SelectionFloorBeam);
            vm.SelectionFloorBeam = true;
            Assert.True(vm.Draft.Front(0, 0)!.FloorBeam);
            Assert.True(vm.Draft.Front(0, 1)!.FloorBeam);
        }

        [Fact]
        public void OneClickOnTheMixedFloorBeamBoxOfTheViewPutsItInEveryFront()
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor();
                var view = new SelectiveEditorView(vm);
                Lay(view);
                vm.SelectAll();
                Lay(view);

                var box = (CheckBox)view.FindName("FloorBeamCheck");
                Assert.Null(box.IsChecked);
                Assert.Equal("Larguero a piso en N1 (varía; marcar lo pone en todos)", box.Content);

                // The click of the user, as WPF does it (ToggleButton.OnToggle through the automation peer).
                ((IToggleProvider)new CheckBoxAutomationPeer(box)).Toggle();
                Lay(view);

                Assert.True(vm.Draft.Front(0, 0)!.FloorBeam);
                Assert.True(vm.Draft.Front(0, 1)!.FloorBeam);
                Assert.True(box.IsChecked);
                Assert.Equal("Larguero a piso en N1", box.Content);
                Assert.Equal("Modificado", vm.StatusText);
                // The floor line of F2·N1 is gone: it has a larguero now.
                Assert.False(vm.Rows.Single(r => r.LevelIndex == 0).Cells[1].IsOnFloor);

                // Checked in every front, the next click takes it out of all of them.
                ((IToggleProvider)new CheckBoxAutomationPeer(box)).Toggle();
                Lay(view);
                Assert.False(box.IsChecked);
                Assert.False(vm.Draft.Front(0, 0)!.FloorBeam);
                Assert.False(vm.Draft.Front(0, 1)!.FloorBeam);
            });
        }

        // ------------------------------------------------------------------ M-04: tramos written and not applied (S-21)

        [Fact]
        public void PendingTramosAreAChangeThatUpdateDoesNotWriteAndThatCloseAsksAbout()
        {
            // P9 of the review.
            var written = 0;
            var vm = SelectiveEditor(update: _ => written++);
            var closed = false;
            vm.CloseRequested += _ => closed = true;

            vm.SelectFront(1);
            vm.StartSegments();
            vm.SegmentRows[0].LengthText = "20";
            Assert.True(vm.HasPendingSegments);
            Assert.True(vm.CanDiscardSegments);
            Assert.Equal("Modificado", vm.StatusText);

            // Another change meanwhile keeps the tramos, and «Actualizar» still waits for them.
            Write(Field(vm.CellFields, "cHeight"), "70");
            Assert.Equal("20", vm.SegmentRows[0].LengthText);
            Assert.False(vm.CanUpdate);
            Assert.Equal(PendingText, vm.UpdateBlockedText);
            vm.RequestWrite();
            Assert.Equal(0, written);
            Assert.Equal(PendingText, vm.Message);
            Assert.True(vm.MessageIsError);
            Assert.False(vm.Draft.Front(0, 1)!.IsSplit);

            // «Cerrar el editor» asks.
            vm.RequestClose();
            Assert.True(vm.IsConfirmingClose);
            Assert.False(closed);
            vm.CancelClose();

            // Applied, they are written: «Actualizar» is offered again.
            vm.ApplySegments();
            Assert.Equal(string.Empty, vm.SegmentError);
            Assert.False(vm.HasPendingSegments);
            Assert.True(vm.CanUpdate);
            vm.RequestWrite();
            Assert.Equal(1, written);
        }

        [Fact]
        public void OnlyPendingTramosAreStillAChangeAndUndoingThemIsCleanAgain()
        {
            var vm = SelectiveEditor();
            vm.SelectFront(1);

            vm.StartSegments();
            Assert.True(vm.IsDirty);
            Assert.False(vm.CanUpdate);

            // Taking the seed back (one tramo is a whole front) is the front as the draft has it: clean again.
            vm.RemoveSegment(0);
            vm.RemoveSegment(0);
            Assert.Empty(vm.SegmentRows);
            Assert.False(vm.HasPendingSegments);
            Assert.Equal("Limpio", vm.StatusText);
        }

        [Fact]
        public void AnotherFrontOrOtherFondosDropThePendingTramosAndSayIt()
        {
            var vm = SelectiveEditor(TwoFondos());
            vm.SelectFront(1);
            vm.StartSegments();
            vm.SegmentRows[0].LengthText = "20";

            // Another cell of the same front keeps them.
            vm.ClickCell(1, 1, control: false, shift: false);
            Assert.Equal("20", vm.SegmentRows[0].LengthText);
            Assert.True(vm.HasPendingSegments);

            // Another front drops them, with a notice in the line of messages.
            vm.ClickCell(0, 0, control: false, shift: false);
            Assert.False(vm.HasPendingSegments);
            Assert.Empty(vm.SegmentRows);
            Assert.Equal("Limpio", vm.StatusText);
            Assert.Equal("Los tramos sin aplicar de F2 se descartaron: se eligió otro frente u otros fondos de la vista.", vm.Message);
            Assert.False(vm.MessageIsError);
            Assert.False(vm.Draft.Front(0, 1)!.IsSplit);

            // Other fondos of the view drop them too.
            vm.SelectFront(1);
            vm.StartSegments();
            vm.FondoScopeIndex = 2;
            Assert.False(vm.HasPendingSegments);
            Assert.Equal("Limpio", vm.StatusText);
            Assert.Equal("Los tramos sin aplicar de F2 se descartaron: se eligió otro frente u otros fondos de la vista.", vm.Message);

            // With nothing pending, another front says nothing.
            vm.Say(string.Empty, error: false);
            vm.ClickCell(0, 0, control: false, shift: false);
            Assert.Equal(string.Empty, vm.Message);
        }

        [Fact]
        public void DiscardTramosAndDiscardOfTheEditorDropThePendingTramos()
        {
            var vm = SelectiveEditor();
            vm.SelectFront(1);
            vm.StartSegments();
            vm.SegmentRows[0].LengthText = "20";

            vm.DiscardSegments();
            Assert.Empty(vm.SegmentRows);
            Assert.False(vm.HasPendingSegments);
            Assert.False(vm.CanDiscardSegments);
            Assert.Equal("Limpio", vm.StatusText);
            Assert.Equal("Se descartaron los tramos sin aplicar de F2.", vm.Message);

            // «Descartar» of the editor: the draft and the tramos go back to what was read.
            vm.StartSegments();
            vm.SegmentRows[0].LengthText = "20";
            Write(Field(vm.CellFields, "cHeight"), "70");
            Assert.True(vm.CanDiscard);
            vm.Discard();
            Assert.Empty(vm.SegmentRows);
            Assert.False(vm.HasPendingSegments);
            Assert.Equal("Limpio", vm.StatusText);
            Assert.Equal(55.0, vm.Draft.Cell(0, 1, 0)!.Alto);
            Assert.StartsWith("Borrador descartado", vm.Message);
        }

        [Fact]
        public void TheTramosOfASplitFrontAsTheDraftHasThemAreNotPending()
        {
            var design = Structure();
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true });
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = false });
            var vm = SelectiveEditor(design);

            vm.SelectFront(0);
            Assert.Equal(2, vm.SegmentRows.Count);
            Assert.False(vm.HasPendingSegments);
            Assert.Equal("Limpio", vm.StatusText);

            // The same length written another way is the same tramo; another length, or the largueros of a tramo, is a change.
            vm.SegmentRows[0].LengthText = "30.0";
            Assert.False(vm.HasPendingSegments);
            vm.SegmentRows[0].LengthText = "31";
            Assert.True(vm.HasPendingSegments);
            vm.SegmentRows[0].LengthText = "30";
            vm.SegmentRows[1].Loaded = true;
            Assert.True(vm.HasPendingSegments);
            vm.SegmentRows[1].Loaded = false;
            Assert.False(vm.HasPendingSegments);
            Assert.Equal("Limpio", vm.StatusText);
        }

        [Fact]
        public void TheViewOffersDiscardTramosOnlyWhileTramosArePending()
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor();
                var view = new SelectiveEditorView(vm);
                Lay(view);
                var discard = (Button)view.FindName("DiscardSegmentsButton");
                Assert.False(discard.IsEnabled);

                vm.SelectFront(1);
                vm.StartSegments();
                Lay(view);
                Assert.True(discard.IsEnabled);
                Assert.False(view.UpdateButton.IsEnabled);
                Assert.Equal(PendingText, view.UpdateBlockedText.Text);

                discard.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Lay(view);
                Assert.False(discard.IsEnabled);
                Assert.Equal("Limpio", view.StatusText.Text);
            });
        }

        // ------------------------------------------------------------------ M-05: the dimension views of a legacy rack (S-22)

        [Fact]
        public void ALegacyRackWhoseDimensionBoxesGoBackToHowTheyOpenedIsCleanWithItsLoadedValue()
        {
            // P2 of the review: no policy (the legacy rack) shows the three boxes ticked.
            var design = Structure();
            design.DimensionViews = null;
            var vm = SelectiveEditor(design);
            Assert.Null(vm.Draft.Design!.DimensionViews);
            Assert.True(vm.DimensionsFrontal && vm.DimensionsLateral && vm.DimensionsPlanta);

            vm.DimensionsFrontal = false;
            Assert.Equal(DimensionViewVisibility.Lateral | DimensionViewVisibility.Planta, vm.Draft.Design!.DimensionViews);
            Assert.Equal("Modificado", vm.StatusText);

            vm.DimensionsFrontal = true;
            Assert.Null(vm.Draft.Design!.DimensionViews);
            Assert.Equal("Limpio", vm.StatusText);

            // A present policy keeps its value too (its unknown bits included), as before.
            var present = Structure();
            present.DimensionViews = DimensionViewVisibility.Frontal | (DimensionViewVisibility)64;
            var other = SelectiveEditor(present);
            other.DimensionsLateral = true;
            Assert.Equal(DimensionViewVisibility.Frontal | DimensionViewVisibility.Lateral | (DimensionViewVisibility)64, other.Draft.Design!.DimensionViews);
            other.DimensionsLateral = false;
            Assert.Equal(DimensionViewVisibility.Frontal | (DimensionViewVisibility)64, other.Draft.Design!.DimensionViews);
            Assert.Equal("Limpio", other.StatusText);
        }

        // ------------------------------------------------------------------ M-07: N1 on the floor (S-23)

        [Fact]
        public void N1OnTheFloorDoesNotOfferTheLargueroItsPeralteNorTheClear()
        {
            var vm = SelectiveEditor();

            // F2·N1: the pallet on the floor, with no larguero.
            vm.ClickCell(1, 0, control: false, shift: false);
            var beam = Choice(vm.CellFields, "cBeam");
            var peralte = Choice(vm.CellFields, "cPeralte");
            var clear = Field(vm.CellFields, "cClear");
            Assert.False(beam.IsEnabled);
            Assert.Equal("Sin larguero (piso)", beam.Hint);
            Assert.False(peralte.IsEnabled);
            Assert.Equal("Sin larguero (piso)", peralte.Hint);
            Assert.True(clear.IsReadOnly);
            Assert.Equal("No en N1", clear.Hint);
            // The pallet and the manual length of the larguero stay as they are.
            Assert.False(Field(vm.CellFields, "cHeight").IsReadOnly);
            Assert.False(Field(vm.CellFields, "cLength").IsReadOnly);

            // A write there changes nothing.
            var before = Json(vm.Draft.Design);
            beam.SelectedValue = "OTRO";
            clear.Text = "20";
            clear.Commit();
            Assert.Equal(before, Json(vm.Draft.Design));
            Assert.Equal("Limpio", vm.StatusText);

            // F1·N1 has «Larguero a piso»: its larguero is drawn, so it is offered; and a selection that is not all on the floor too.
            vm.ClickCell(0, 0, control: false, shift: false);
            Assert.True(Choice(vm.CellFields, "cBeam").IsEnabled);
            Assert.False(Field(vm.CellFields, "cClear").IsReadOnly);
            vm.ClickCell(1, 0, control: false, shift: false);
            vm.ClickCell(1, 1, control: true, shift: false);
            Assert.True(Choice(vm.CellFields, "cBeam").IsEnabled);
            // Not locked: the hint of the approved draft, not «No en N1» (tanda 3, fidelity with the draft).
            Assert.Equal("Vacío = automático. Hasta el larguero de abajo (en N2, desde el piso); al troquel de 2 in.", Field(vm.CellFields, "cClear").Hint);

            // «Larguero a piso» in F2: its N1 is offered again.
            vm.ClickCell(1, 0, control: false, shift: false);
            vm.SelectionFloorBeam = true;
            Assert.True(Choice(vm.CellFields, "cBeam").IsEnabled);
            Assert.False(Field(vm.CellFields, "cClear").IsReadOnly);
        }

        [Fact]
        public void InAllTheFondosN1IsOnlyLockedWhenNoFondoHasItsLargueroAPiso()
        {
            // Fondo 1: F1 with «Larguero a piso»; fondo 2: F1 with it too, F2 and F3 without.
            var vm = SelectiveEditor(TwoFondos());
            vm.ClickCell(1, 0, control: false, shift: false);
            Assert.False(Choice(vm.CellFields, "cBeam").IsEnabled);

            // F2 of fondo 1 gets «Larguero a piso»: in «Todos los fondos» F2·N1 has a larguero somewhere, so it is offered.
            vm.FondoScopeIndex = 1;
            vm.ClickCell(1, 0, control: false, shift: false);
            vm.SelectionFloorBeam = true;
            vm.FondoScopeIndex = 0;
            vm.ClickCell(1, 0, control: false, shift: false);
            Assert.True(Choice(vm.CellFields, "cBeam").IsEnabled);
            vm.FondoScopeIndex = 2;
            vm.ClickCell(1, 0, control: false, shift: false);
            Assert.False(Choice(vm.CellFields, "cBeam").IsEnabled);
        }

        // ------------------------------------------------------------------ M-10: the dimension styles of the drawing (S-27)

        [Fact]
        public void TheDimensionStyleIsChosenFromTheStylesOfTheDrawingAndKeepsTheOneItOpenedWith()
        {
            var design = Structure();
            design.DimensionStyle = "ESTILO_RACKCAD";
            var vm = SelectiveEditor(design, dimensionStyles: new[] { "Standard", "ISO-25", " Standard ", "" });

            var style = Choice(vm.AnnotationFields, "dimStyle");
            Assert.Equal(new[] { "(Automático)", "Standard", "ISO-25", "ESTILO_RACKCAD" }, style.Options.Select(o => o.Value).ToArray());
            Assert.Equal("ESTILO_RACKCAD", style.SelectedValue);

            style.SelectedValue = "ISO-25";
            Assert.Equal("ISO-25", vm.Draft.Design!.DimensionStyle);
            Assert.Equal("Modificado", vm.StatusText);
            // The style it opened with is still in the list, though the drawing does not have it.
            Assert.Contains(Choice(vm.AnnotationFields, "dimStyle").Options, o => o.Value == "ESTILO_RACKCAD");

            Choice(vm.AnnotationFields, "dimStyle").SelectedValue = "(Automático)";
            Assert.Null(vm.Draft.Design!.DimensionStyle);

            Choice(vm.AnnotationFields, "dimStyle").SelectedValue = "ESTILO_RACKCAD";
            Assert.Equal("ESTILO_RACKCAD", vm.Draft.Design!.DimensionStyle);
            Assert.Equal("Limpio", vm.StatusText);

            // No styles read and no style: only «(Automático)».
            var plain = SelectiveEditor();
            Assert.Equal(new[] { "(Automático)" }, Choice(plain.AnnotationFields, "dimStyle").Options.Select(o => o.Value).ToArray());
            Assert.Equal("(Automático)", Choice(plain.AnnotationFields, "dimStyle").SelectedValue);
        }

        // ------------------------------------------------------------------ M-11: «Escribiendo…» once the command took the write (S-28)

        [Fact]
        public void OnceTheCommandTookTheWriteTheEditorNoLongerSaysToStopWaiting()
        {
            var vm = SelectiveEditor();
            Write(Field(vm.CellFields, "cHeight"), "72");

            vm.BeginWriting();
            Assert.Equal("AutoCAD está escribiendo el rack: espera a que termine o pulsa «Dejar de esperar».", vm.UpdateBlockedText);

            vm.WriteTaken();
            Assert.DoesNotContain("Dejar de esperar", vm.UpdateBlockedText);
            Assert.Equal("AutoCAD ya tomó la escritura del rack: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.",
                vm.UpdateBlockedText);
            vm.RequestClose();
            Assert.Equal(vm.WritingText, vm.Message);

            // The next write starts as before.
            vm.EndWriting();
            vm.BeginWriting();
            Assert.Contains("Dejar de esperar", vm.UpdateBlockedText);
            vm.EndWriting();

            // Taken is only for a write in progress.
            vm.WriteTaken();
            Assert.Contains("Dejar de esperar", vm.WritingText);
        }

        // ------------------------------------------------------------------ M-03: a design with no fronts (S-29)

        [Fact]
        public void ADesignWithNoFrontsKeepsTheRackTheDraftHadAndSaysSo()
        {
            // P3 of the review: before, the copy through the store threw before the check.
            var draft = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
            Assert.True(draft.LoadFrom(Structure(), null, "Selectivo A"), draft.LastError);
            var kept = Json(draft.Design);

            var empty = Structure();
            empty.Bays.Clear();
            Assert.True(draft.LoadFrom(empty, null, "Vacío"), draft.LastError);

            Assert.Equal("El diseño leído no tiene frentes: el editor conserva el rack que tenía.", draft.LoadNotice);
            Assert.Equal(kept, Json(draft.Design));
            Assert.Equal(2, draft.FrontCount(0));
            Assert.Equal(3, draft.LevelCount(0, 0));
        }

        // ------------------------------------------------------------------ P1 of the review: «Descartar» and the rollback are idempotent

        [Fact]
        public void DiscardAndTheRollbackOfAFreshEditorChangeNothingInSevenShapesOfRack()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (name, design) in Shapes())
                {
                    var vm = SelectiveEditor(design);
                    var opened = Json(vm.Draft.Design);
                    Assert.True(vm.StatusText == "Limpio", name + " opens modified");

                    vm.Discard();
                    Assert.True(opened == Json(vm.Draft.Design), name + ": «Descartar» changed the design");
                    Assert.True(vm.Draft.RestoreLastValid(), name);
                    Assert.True(vm.Draft.Recompose(), name + ": " + vm.Draft.LastError);
                    Assert.True(opened == Json(vm.Draft.Design), name + ": the rollback changed the design");

                    // Every front shown in the inspector, one by one: none has tramos pending.
                    for (var f = 0; f < vm.Draft.ScopeFrontCount; f++)
                    {
                        vm.SelectFront(f);
                        Assert.False(vm.HasPendingSegments, name + " F" + (f + 1));
                    }

                    Assert.True(vm.StatusText == "Limpio", name + " is modified");
                }
            });
        }

        private static IEnumerable<(string Name, SelectivePalletDesign Design)> Shapes()
        {
            yield return ("un fondo", Structure());
            yield return ("dos fondos", TwoFondos());

            var custom = TwoFondos();
            custom.PostCabeceras.Add(Custom(120.0));
            custom.PostCabeceras.Add(null!);
            custom.PostCabeceras.Add(Custom(110.0));
            custom.ExtraFondoPostCabeceras.Add(new List<Domain.RackFrames.RackFrameConfiguration> { null!, Custom(130.0), null!, Custom(125.0) });
            yield return ("cabeceras personalizadas", custom);

            var segments = Structure();
            segments.Bays[0].Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true });
            segments.Bays[0].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = false });
            yield return ("medio frente", segments);

            var legacy = Structure();
            legacy.FloorBeamRise = 0.0;
            foreach (var bay in legacy.Bays) bay.FloorBeamRiseOverride = null;
            legacy.DimensionViews = null;
            legacy.Dimensions = DimensionDetail.Minimal;
            yield return ("heredado", legacy);

            var precision = TwoFondos();
            precision.SeparatorLengths[0] = 3.14159;
            precision.PostPeralte = 3.14159;
            precision.AnnotationScale = 1.23456;
            precision.DimensionStyle = "  ESTILO X  ";
            precision.Bays[1].Segments.Add(new SelectiveSegment { Length = 20.2441, Loaded = true });
            precision.Bays[1].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            yield return ("precisión", precision);

            var safety = Structure();
            safety.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "BOTA_GENERICA", Quantity = 2, Side = SafetySide.Both });
            yield return ("seguridad", safety);
        }

        private static void Lay(FrameworkElement view, double width = 1200, double height = 900)
        {
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
            view.UpdateLayout();
        }
    }
}
