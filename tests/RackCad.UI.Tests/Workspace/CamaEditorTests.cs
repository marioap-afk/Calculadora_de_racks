#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.FlowBed;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, quinta tanda, part cm (prototype «camas», every question accepted): the editor of a cama de rodamiento in the window, without
    /// the window. One page with «Lo esencial», «Calculado», «Esquema de la cama» and «Avanzado»; one way to confirm; «Modificado» measured
    /// against what was read or written; the values refused marked in their field with «Actualizar» off and the reason (dolor 5); question 3
    /// (a brake every N rollers), 4 (the pitch), 6 («Restaurar layout…») and 8 (from Pushback to Dinámica).
    /// </summary>
    public class CamaEditorTests
    {
        private const string Roller19 = "RODILLO_DE_TUBO_DE_1.9_CALIBRE_14";
        private const string Roller25 = "RODILLO_DE_TUBO_DE_2.5_CALIBRE_14";

        private static FlowBedConfiguration Defaults() => CamaDraftRules.Defaults(UiSupport.LoadCatalogSafe());

        private static (CamaEditorViewModel Editor, List<CamaEditorViewModel> Updates) Drawn(FlowBedConfiguration? bed = null, string name = "Cama 1")
        {
            var updates = new List<CamaEditorViewModel>();
            var editor = new CamaEditorViewModel(new CamaDraftEditor(UiSupport.LoadCatalogSafe(), bed ?? Defaults(), name), "CAMA-1", updates.Add);
            return (editor, updates);
        }

        private static void Write(CamaFieldItem field, string text)
        {
            field.Text = text;
            field.Commit();
        }

        private static string Calc(CamaEditorViewModel editor, string label) => editor.Calculated.Single(c => c.Label == label).Value;

        [Fact]
        public void ACamaOpensCleanWithWhatTheClassicCalculatesAndItsScheme()
        {
            var (editor, _) = Drawn();

            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("Cama 1", editor.Name);
            Assert.Equal("Dynamic", editor.TypeField.SelectedValue);
            Assert.Equal(Roller19, editor.RollerField.SelectedValue);
            Assert.Equal(new[] { "Dinámica", "Pushback" }, editor.TypeField.Options.Select(o => o.Label).ToArray());
            Assert.Equal("Capacidad del catálogo: 110 kg. Paso automático: 2 in.", editor.RollerField.Hint);
            Assert.Equal(("96", "48", "", ""), (editor.LaneField.Text, editor.PalletField.Text, editor.PitchField.Text, editor.BrakesField.Text));
            Assert.Equal(new object[] { editor.TypeField, editor.RollerField, editor.LaneField, editor.PalletField }, editor.Essentials.ToArray());

            Assert.Equal("96 in", Calc(editor, "Riel"));
            Assert.Equal("42", Calc(editor, "Rodillos"));
            Assert.Equal("1", Calc(editor, "Frenos"));
            Assert.Equal("2 in", Calc(editor, "Paso"));
            Assert.Equal("1", Calc(editor, "Tope"));
            Assert.Equal("Por la tarima: cada 49 in o más.", editor.Calculated.Single(c => c.Label == "Frenos").Hint);
            Assert.Equal("El del rodillo.", editor.Calculated.Single(c => c.Label == "Paso").Hint);

            Assert.True(editor.IsSchemaOpen);
            Assert.Equal("42 rodillos · 1 freno", editor.SchemaSummary);
            Assert.Equal("Esquema de la cama: tope, 42 rodillos y 1 freno (a 56 in) en 96 in de riel.", editor.SchemaText);
            Assert.Equal("Freno 1: a 56 in.", editor.PositionLines[2]);
            Assert.False(editor.IsAdvancedOpen);
            Assert.Equal("Paso del rodillo (2 in) · Frenos por la tarima", editor.AdvancedSummary);

            Assert.False(editor.CanPrimary);
            Assert.Equal("Actualizar", editor.PrimaryLabel);
            Assert.Equal("Sin cambios que escribir.", editor.PrimaryBlockedText);
            Assert.Contains("no tiene «Insertar vista…»", editor.SubLine); // question 5
        }

        [Fact]
        public void AValueWrittenGoesToTheDraftRecomputesAndDescartarGoesBack()
        {
            var (editor, updates) = Drawn();
            Write(editor.LaneField, "120");

            Assert.Equal("Modificado", editor.StatusText);
            Assert.Equal("1 valor cambiado", editor.DiffText);
            Assert.Equal("120 in", Calc(editor, "Riel"));
            Assert.Equal(120.0, editor.Draft.LaneDepth);
            Assert.True(editor.CanPrimary);

            // Leaving a field with the text it shows changes nothing.
            Write(editor.PalletField, "48");
            Assert.Equal("1 valor cambiado", editor.DiffText);

            editor.RequestWrite();
            Assert.Same(editor, Assert.Single(updates));

            editor.Discard();
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("96", editor.LaneField.Text);
            Assert.Equal(96.0, editor.Draft.LaneDepth);
        }

        [Fact]
        public void EveryValueRefusedStaysMarkedInItsFieldAndActualizarIsOffWithTheReason()
        {
            var (editor, updates) = Drawn();
            Write(editor.PalletField, "60");
            Assert.True(editor.CanPrimary);

            Write(editor.LaneField, "0");
            Assert.Equal("0", editor.LaneField.Text);
            Assert.Equal(CamaDraftRules.LaneDepthText, editor.LaneField.Error);
            Assert.True(editor.LaneField.HasError);
            Assert.Equal(96.0, editor.Draft.LaneDepth); // the draft does not take it
            Assert.False(editor.CanPrimary);
            Assert.Equal("Corrige el valor marcado para actualizar.", editor.PrimaryBlockedText);
            Assert.Equal("—", Calc(editor, "Rodillos"));
            Assert.Null(editor.Layout);
            Assert.Equal("Corrige los valores marcados: el cálculo usa todos los campos.", editor.CalculatedNote);

            Write(editor.PitchField, "2.5");
            Assert.Equal(CamaDraftRules.PitchIntegerText, editor.PitchField.Error);
            Assert.Equal("Corrige los 2 valores marcados para actualizar.", editor.PrimaryBlockedText);
            Assert.True(editor.AdvancedHasProblem);
            Assert.EndsWith(" · con valores marcados", editor.AdvancedSummary, StringComparison.Ordinal);

            // A field that is written again with a good value, or with the value it had, is not marked any more; the other one still is.
            Write(editor.LaneField, "96");
            Assert.Equal(string.Empty, editor.LaneField.Error);
            Assert.Equal("Corrige el valor marcado para actualizar.", editor.PrimaryBlockedText);
            Write(editor.PitchField, "1");
            Assert.Equal("El paso no puede ser menor que el rodillo: mínimo 2 in.", editor.PitchField.Error);
            Write(editor.PitchField, "");
            Assert.False(editor.HasMarks);
            Assert.True(editor.CanPrimary);
            editor.RequestWrite();
            Assert.Single(updates);

            // «Descartar» takes the marks away with the changes.
            Write(editor.BrakesField, "0");
            Assert.Equal(CamaDraftRules.BrakesText, editor.BrakesField.Error);
            editor.Discard();
            Assert.False(editor.HasMarks);
            Assert.Equal(string.Empty, editor.BrakesField.Text);
        }

        [Fact]
        public void ACamaReadWithAPitchTheWindowDoesNotAcceptOpensMarkedAndAskToCorrectIt()
        {
            var bed = Defaults();
            bed.RollerPitchOverride = 2.5; // the classic accepted it
            var (editor, _) = Drawn(bed);

            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("2.5", editor.PitchField.Text);
            Assert.Equal(CamaDraftRules.PitchIntegerText, editor.PitchField.Error);
            Assert.True(editor.IsAdvancedOpen); // the problem is in «Avanzado»: it opens
            Assert.Equal("Corrige el valor marcado para actualizar.", editor.PrimaryBlockedText);
            Assert.NotNull(editor.SaveRefusal());

            Write(editor.PitchField, "3");
            Assert.Equal("Modificado", editor.StatusText);
            Assert.True(editor.CanPrimary);
            Assert.Equal("3 in", Calc(editor, "Paso"));
            Assert.True(editor.Calculated.Single(c => c.Label == "Paso").IsManual);
            Assert.Equal("Manual (en «Avanzado»).", editor.Calculated.Single(c => c.Label == "Paso").Hint);
            Assert.Equal("Paso manual: 3 in · Frenos por la tarima", editor.AdvancedSummary);
        }

        [Fact]
        public void APushbackWithNoPalletBecomesDinamicaWithFortyEightInchesAndSaysSo()
        {
            var bed = Defaults();
            bed.BedType = FlowBedType.Pushback;
            bed.PalletDepth = 0.0;
            bed.BrakeEveryRollers = 5;
            var (editor, _) = Drawn(bed, "Cama 2");

            Assert.True(editor.PalletField.IsReadOnly);
            Assert.True(editor.BrakesField.IsReadOnly);
            Assert.Equal(string.Empty, editor.PalletField.Text);
            Assert.Equal("No aplica en Pushback: no lleva frenos. Conserva su valor.", editor.PalletField.Hint);
            Assert.Equal("0", Calc(editor, "Frenos"));
            Assert.Equal("Pushback: sin frenos.", editor.Calculated.Single(c => c.Label == "Frenos").Hint);
            Assert.Equal("Paso del rodillo (2 in) · Sin frenos", editor.AdvancedSummary);

            editor.TypeField.SelectedValue = "Dynamic";

            Assert.Equal(CamaDraftRules.PalletFilledText, editor.Message);
            Assert.Equal("48", editor.PalletField.Text);
            Assert.False(editor.PalletField.IsReadOnly);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.Equal("2 valores cambiados", editor.DiffText);
            Assert.Equal("Freno cada 5 rodillos", editor.AdvancedSummary.Split(" · ")[1]);
        }

        [Fact]
        public void ARollerBiggerThanTheManualPitchIsRefusedUnderItsListAndTheRollerDoesNotChange()
        {
            var (editor, _) = Drawn();
            Write(editor.PitchField, "2");

            editor.RollerField.SelectedValue = Roller25;

            Assert.Equal(Roller19, editor.RollerField.SelectedValue);
            Assert.Equal(Roller19, editor.Draft.RollerId);
            Assert.Equal("El paso (2 in) es menor que este rodillo: cambia antes el paso. El rodillo no cambió.", editor.RollerField.Error);

            Write(editor.PitchField, "");
            Assert.Equal(string.Empty, editor.RollerField.Error);
            editor.RollerField.SelectedValue = Roller25;
            Assert.Equal(Roller25, editor.Draft.RollerId);
            Assert.Equal("29", Calc(editor, "Rodillos"));
            Assert.Contains("3 in con este", editor.PitchField.Hint);
        }

        [Fact]
        public void RestaurarLayoutOpensWithTheTwoBoxesMarkedAndTakesAwayOnlyWhatIsMarked()
        {
            var (editor, _) = Drawn();
            editor.OpenRestore();
            Assert.False(editor.IsRestoreOpen);
            Assert.Equal("No hay nada manual que quitar: la cama ya es la que sale del rodillo y de la tarima.", editor.Message);

            Write(editor.PitchField, "4");
            Write(editor.BrakesField, "8");
            Assert.Equal("Paso manual: 4 in · Freno cada 8 rodillos", editor.AdvancedSummary);
            editor.OpenRestore();
            Assert.True(editor.IsRestoreOpen);
            Assert.True(editor.RestorePitch);
            Assert.True(editor.RestoreBrakes);
            Assert.Equal("Paso de rodillos manual (4 in)", editor.RestorePitchLabel);
            Assert.Equal("Freno cada N rodillos (8)", editor.RestoreBrakesLabel);

            editor.RestoreBrakes = false;
            editor.ConfirmRestore();

            Assert.False(editor.IsRestoreOpen);
            Assert.Equal("Layout restaurado en el borrador: se quitó el paso manual. El dibujo no cambia hasta «Actualizar».", editor.Message);
            Assert.Null(editor.Draft.RollerPitch);
            Assert.Equal(8, editor.Draft.BrakeEveryRollers);
            Assert.Equal(string.Empty, editor.PitchField.Text);

            editor.OpenRestore();
            Assert.False(editor.CanRestorePitch);
            Assert.False(editor.RestorePitch);
            editor.CancelRestore();
            Assert.False(editor.IsRestoreOpen);
            Assert.Equal(8, editor.Draft.BrakeEveryRollers);
        }

        [Fact]
        public void CerrarElEditorAsksOnlyWithChanges()
        {
            var (editor, _) = Drawn();
            var closed = 0;
            editor.CloseRequested += _ => closed++;
            editor.RequestClose();
            Assert.Equal(1, closed);

            Write(editor.LaneField, "100");
            editor.RequestClose();
            Assert.True(editor.IsConfirmingClose);
            Assert.Equal(1, closed);
            editor.CancelClose();
            Assert.False(editor.IsConfirmingClose);
            editor.RequestClose();
            editor.ConfirmClose();
            Assert.Equal(2, closed);
        }

        [Fact]
        public void WhileAutoCadWritesNothingCanBeEditedAndWhatWasWrittenIsTheBaseOfLimpio()
        {
            var (editor, _) = Drawn();
            Write(editor.LaneField, "110");
            var sent = new FlowBedConfigurationStore().Serialize(editor.Draft.Snapshot());

            editor.BeginWriting();
            Assert.True(editor.IsWriting);
            Assert.True(editor.LaneField.IsReadOnly);
            Assert.False(editor.TypeField.IsEnabled);
            Assert.False(editor.CanPrimary);
            Assert.Equal("AutoCAD está escribiendo la cama: espera a que termine o pulsa «Dejar de esperar».", editor.PrimaryBlockedText);
            Write(editor.LaneField, "130");
            Assert.Equal(110.0, editor.Draft.LaneDepth);
            editor.WriteTaken();
            Assert.StartsWith("AutoCAD ya tomó la escritura de la cama", editor.PrimaryBlockedText, StringComparison.Ordinal);

            editor.EndWriting();
            editor.MarkWritten("AutoCAD redibujó la cama con el borrador: todas sus copias en el dibujo reflejan el cambio.", sent, "Cama 1");
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("110", editor.LaneField.Text);
            editor.Discard();
            Assert.Equal(110.0, editor.Draft.LaneDepth);
        }

        [Fact]
        public void ANewCamaIsInsertedWithItsOwnActionAndThenItIsACamaOfTheDrawing()
        {
            var inserted = new List<CamaEditorViewModel>();
            var editor = new CamaEditorViewModel(CamaDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "NEW-1", _ => throw new InvalidOperationException("no update"), isNewRack: true);

            Assert.Equal("Insertar en el dibujo…", editor.PrimaryLabel);
            Assert.Equal("Aquí no se puede insertar la cama nueva.", editor.PrimaryBlockedText);
            Assert.StartsWith("Cama nueva: todavía no está en el dibujo.", editor.SubLine, StringComparison.Ordinal);
            Assert.Equal("Vacío = el nombre automático al insertarla («Cama N», I-60).", editor.NameHint);

            editor.InsertNewAction = inserted.Add;
            Assert.True(editor.CanPrimary); // a new cama is inserted also with nothing changed
            Assert.False(editor.CanUpdate);
            editor.RequestWrite();
            Assert.Same(editor, Assert.Single(inserted));

            Write(editor.LaneField, "0");
            Assert.Equal("Corrige el valor marcado para insertarla.", editor.PrimaryBlockedText);
            Write(editor.LaneField, "96");

            editor.MarkInserted("Cama 3");
            Assert.False(editor.IsNewRack);
            Assert.Equal("Cama 3", editor.Name);
            Assert.Equal("Actualizar", editor.PrimaryLabel);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("Sin cambios que escribir.", editor.PrimaryBlockedText);
        }

        [Fact]
        public void ABrakeEveryNRollersMovesTheBrakesOfTheSchemeAndAPushbackKeepsIt()
        {
            var (editor, _) = Drawn();
            Write(editor.BrakesField, "10");
            Assert.Equal("Uno cada 10 rodillos.", editor.Calculated.Single(c => c.Label == "Frenos").Hint);
            Assert.True(editor.Layout!.Brakes.Count > 1);
            Assert.Equal(editor.Layout.Rollers[9] + 5.0, editor.Layout.Brakes[0]);

            editor.TypeField.SelectedValue = "Pushback";
            Assert.Equal("10", editor.BrakesField.Text);
            Assert.True(editor.BrakesField.IsReadOnly);
            Assert.Empty(editor.Layout!.Brakes);
            Assert.Equal(10, editor.Draft.BrakeEveryRollers);
        }

        [Fact]
        public void GuardarEnBibliotecaNamesTheFieldOfAValueThatWasNotApplied()
        {
            var (editor, _) = Drawn();
            var saved = 0;
            editor.SaveToLibraryAction = _ => saved++;
            Write(editor.LaneField, "abc");

            editor.SaveToLibrary();
            Assert.Equal(0, saved);
            Assert.Equal("No se puede guardar en la biblioteca: «Profundidad de la cama» tiene un valor que no se aplicó (Debe ser un número mayor que cero). Corrígelo o vuelve a escribir el que tenía.", editor.Message);

            Write(editor.LaneField, "96");
            editor.IsMoreOpen = true;
            editor.SaveToLibrary();
            Assert.Equal(1, saved);
            Assert.False(editor.IsMoreOpen);

            // A value refused in the pallet of a Pushback does not apply (the field is off): it marks nothing and stops nothing.
            Write(editor.PalletField, "0");
            Assert.True(editor.HasMarks);
            editor.TypeField.SelectedValue = "Pushback";
            Assert.False(editor.HasMarks);
            Assert.Equal(string.Empty, editor.PalletField.Error);
            Assert.Null(editor.SaveRefusal());
            editor.TypeField.SelectedValue = "Dynamic";
            Assert.Equal(CamaDraftRules.PalletDepthText, editor.PalletField.Error); // back on Dinámica, the field shows it again
        }
    }
}
