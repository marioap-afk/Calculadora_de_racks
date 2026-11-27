#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, Selectivo E1-B: the editor of a Selectivo in the RackCad window, over the draft of E1-A. The same rules as the
    /// editor of Dinámico: «Modificado» is a difference of the design with what was read or written (so undoing a change is «Limpio» again),
    /// nothing can be edited while AutoCAD writes, a change that cannot be recomposed goes back to the last valid one. And its own: the
    /// scope of the fondos with «varía», the writes by cell and by front with what they omitted, the linked holgura read only and with no
    /// «=», the warning when fewer fronts drop custom cabeceras or own peraltes, and a rack that does not recompose when it opens.
    /// </summary>
    public class RackCadSelectiveEditorTests
    {
        // ------------------------------------------------------------------ what it shows

        [Fact]
        public void TheEditorOpensOnTheMatrixCleanWithThePalletPerCellTheFloorLineAndTheHighestLevelOnTop()
        {
            var vm = SelectiveEditor();

            Assert.Equal(1, vm.SelectedTab);
            Assert.Equal("Limpio", vm.StatusText);
            Assert.False(vm.CanUpdate);
            Assert.Equal("Sin cambios que escribir.", vm.UpdateBlockedText);
            Assert.Equal("Selectivo A", vm.Name);
            Assert.False(vm.HasFondos); // one fondo: no selector (decision 2)
            Assert.Equal(new[] { "F1", "F2" }, vm.Columns.Select(c => c.Title).ToArray());
            Assert.Equal(new[] { "N3", "N2", "N1" }, vm.Rows.Select(r => r.Label).ToArray());
            Assert.Equal("40×60", vm.Rows[2].Cells[0].Text);
            Assert.Equal("44×55", vm.Rows[2].Cells[1].Text);
            // F2 has two levels: its N3 does not exist.
            Assert.False(vm.Rows[0].Cells[1].Exists);
            Assert.Equal("—", vm.Rows[0].Cells[1].Text);
            // Decision 4: the floor line marks N1 on the floor with no larguero (F2); F1 has «Larguero a piso».
            Assert.False(vm.Rows[2].Cells[0].IsOnFloor);
            Assert.True(vm.Rows[2].Cells[1].IsOnFloor);
            Assert.False(vm.Rows[1].Cells[1].IsOnFloor);
            Assert.Equal("Frente 1 · Nivel 1", vm.InspectorTitle);
            Assert.True(vm.Rows[2].Cells[0].IsPrimary);
        }

        [Fact]
        public void TheDetailOfTheCellsAddsThePalletsPerLevelAndThePeralte()
        {
            // Decision 1: «Ver detalle» adds the pallets per level and the peralte.
            var vm = SelectiveEditor();
            Assert.False(vm.Rows[2].Cells[0].ShowDetail);

            vm.ShowDetail = true;

            Assert.True(vm.Rows[2].Cells[0].ShowDetail);
            Assert.Equal("×2 · P4", vm.Rows[2].Cells[0].Detail);
            Assert.Equal("×2 · piso", vm.Rows[2].Cells[1].Detail);
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void TheSelectionIsClickControlShiftColumnRowAndAll()
        {
            var vm = SelectiveEditor();

            vm.ClickCell(1, 1, control: false, shift: false);
            Assert.Equal(new[] { P(1, 1) }, vm.SelectedPositions);
            Assert.Equal("Frente 2 · Nivel 2", vm.InspectorTitle);

            vm.ClickCell(0, 2, control: true, shift: false);
            Assert.Equal(new[] { P(0, 2), P(1, 1) }, vm.SelectedPositions);
            vm.ClickCell(0, 2, control: true, shift: false);
            Assert.Equal(new[] { P(1, 1) }, vm.SelectedPositions);
            // The selection is never empty.
            vm.ClickCell(1, 1, control: true, shift: false);
            Assert.Equal(new[] { P(1, 1) }, vm.SelectedPositions);

            // Mayús from the anchor: the rectangle, only the cells that exist (F2 has no N3).
            vm.ClickCell(1, 0, control: false, shift: false);
            vm.ClickCell(0, 2, control: false, shift: true);
            Assert.Equal(new[] { P(0, 0), P(0, 1), P(0, 2), P(1, 0), P(1, 1) }, vm.SelectedPositions);
            Assert.Equal(P(0, 2), vm.PrimaryPosition);
            Assert.Equal("5 celdas", vm.InspectorTitle);
            // A second Mayús+clic extends from the same anchor, not from the last click.
            vm.ClickCell(1, 1, control: false, shift: true);
            Assert.Equal(new[] { P(1, 0), P(1, 1) }, vm.SelectedPositions);

            vm.SelectFront(0);
            Assert.Equal(new[] { 0 }, vm.SelectedFronts);
            Assert.Equal(3, vm.SelectedPositions.Count);
            vm.SelectLevel(2);
            Assert.Equal(new[] { P(0, 2) }, vm.SelectedPositions);
            vm.SelectAll();
            Assert.Equal(5, vm.SelectedPositions.Count);
            // A cell that does not exist is not selected.
            vm.ClickCell(1, 2, control: false, shift: false);
            Assert.Equal(5, vm.SelectedPositions.Count);
        }

        // ------------------------------------------------------------------ the state

        [Fact]
        public void ModifiedIsADifferenceWithWhatWasReadAndUndoingTheChangeIsCleanAgain()
        {
            var updates = 0;
            var vm = SelectiveEditor(update: _ => updates++);

            // Leaving a field without changing its text changes nothing.
            Field(vm.CellFields, "cHeight").Commit();
            Assert.Equal("Limpio", vm.StatusText);

            Write(Field(vm.CellFields, "cHeight"), "72");
            Assert.Equal("Modificado", vm.StatusText);
            Assert.True(vm.CanUpdate);
            Assert.Equal(72.0, vm.Draft.Cell(0, 0, 0)!.Alto);
            Assert.Equal(60.0, vm.Draft.Cell(0, 0, 1)!.Alto); // only the selected cell

            Write(Field(vm.CellFields, "cHeight"), "60");
            Assert.Equal("Limpio", vm.StatusText);
            Assert.False(vm.CanUpdate);

            // A name with only trailing spaces is not a change.
            vm.Name = "Selectivo A  ";
            Assert.False(vm.IsDirty);
            vm.Name = "Selectivo B";
            Assert.True(vm.IsDirty);
            vm.RequestWrite();
            Assert.Equal(1, updates);
        }

        [Fact]
        public void WhileAutoCadWritesTheEditorIsWritingAndCannotBeEdited()
        {
            var vm = SelectiveEditor();
            Write(Field(vm.CellFields, "cHeight"), "72");

            vm.BeginWriting();
            Assert.Equal("Escribiendo", vm.StatusText);
            Assert.False(vm.CanEdit);
            Assert.False(vm.CanUpdate);
            Assert.False(vm.CanClose);
            var height = Field(vm.CellFields, "cHeight");
            Assert.True(height.IsReadOnly);
            height.Text = "80";
            height.Commit();
            vm.DrawPallets = true;
            vm.SelectionFloorBeam = false;
            Assert.Equal(72.0, vm.Draft.Cell(0, 0, 0)!.Alto);
            Assert.False(vm.Draft.DrawPallets);
            Assert.True(vm.Draft.Front(0, 0)!.FloorBeam);

            vm.EndWriting();
            Assert.Equal("Modificado", vm.StatusText);
            Assert.True(vm.CanEdit);
            Assert.False(Field(vm.CellFields, "cHeight").IsReadOnly);

            // What was written becomes the baseline: the draft is clean and «Descartar» goes back to it.
            vm.MarkWritten("Escrito.", Json(vm.Draft.Design), vm.Name);
            Assert.Equal("Limpio", vm.StatusText);
            Write(Field(vm.CellFields, "cHeight"), "90");
            vm.Discard();
            Assert.Equal(72.0, vm.Draft.Cell(0, 0, 0)!.Alto);
            Assert.Equal("Limpio", vm.StatusText);
        }

        [Fact]
        public void ClosingTheEditorAsksFirstOnlyWhenTheDraftHasChanges()
        {
            var vm = SelectiveEditor();
            var closed = 0;
            vm.CloseRequested += _ => closed++;

            vm.RequestClose();
            Assert.Equal(1, closed);

            Write(Field(vm.CellFields, "cHeight"), "72");
            vm.RequestClose();
            Assert.True(vm.IsConfirmingClose);
            Assert.Equal(1, closed);
            vm.CancelClose();
            Assert.False(vm.IsConfirmingClose);
            vm.RequestClose();
            vm.ConfirmClose();
            Assert.Equal(2, closed);
        }

        // ------------------------------------------------------------------ the scope of the fondos (decision 2)

        [Fact]
        public void TheScopeOfTheFondosShowsWhereItWritesAndWhatDiffersSaysVaries()
        {
            var vm = SelectiveEditor(TwoFondos());

            Assert.True(vm.HasFondos);
            Assert.Equal(new[] { "Todos los fondos", "Fondo 1", "Fondo 2" }, vm.FondoOptions);
            Assert.Equal(0, vm.FondoScopeIndex); // it starts in «Todos los fondos»
            Assert.StartsWith("Lo que ves es donde escribes: cada celda", vm.FondoScopeText);

            // The depth and the fronts differ between the fondos: «Varía», with an empty box.
            var depth = Field(vm.Essentials, "depth");
            Assert.Equal(string.Empty, depth.Text);
            Assert.Equal("Varía", depth.Hint);
            Assert.Equal("Varía", Field(vm.Essentials, "fronts").Hint);
            // An empty «Varía» left as it is changes nothing.
            depth.Commit();
            Assert.False(vm.IsDirty);

            // F3 only exists in fondo 2, and the column says so; F2 has 2 levels in fondo 1 and 3 in fondo 2.
            Assert.Equal(3, vm.Columns.Count);
            Assert.True(vm.Columns[2].IsPartial);
            Assert.Contains("solo en el fondo 2", vm.Columns[2].AccessibleText);
            Assert.False(vm.Columns[0].IsPartial);
            vm.SelectFront(1);
            Assert.Equal("Varía", Field(vm.FrontFields, "fLevels").Hint);

            // «Fondo 2»: what is seen in the matrix is fondo 2, and a write of the inspector goes only there. «Rack» › «Lo esencial» is the
            // whole rack (the approved draft), whatever the view is: it still says «Varía»; what goes by fondo is the table «Fondos».
            vm.FondoScopeIndex = 2;
            Assert.Equal("Varía", Field(vm.Essentials, "depth").Hint);
            Assert.Equal("Varía", Field(vm.Essentials, "fronts").Hint);
            Assert.Equal("Lo que ves es donde escribes: solo en el fondo 2.", vm.FondoScopeText);
            Assert.Equal("40", vm.FondoRows[1].Depth.Text);
            Assert.Equal("3", vm.FondoRows[1].Fronts.Text);
            Write(vm.FondoRows[1].Depth, "36");
            Assert.Equal(36.0, vm.Draft.Fondo(1)!.PalletDepth);
            Assert.Equal(48.0, vm.Draft.Fondo(0)!.PalletDepth);
            Assert.True(vm.IsDirty);
            vm.ClickCell(0, 0, control: false, shift: false);
            Write(Field(vm.CellFields, "cHeight"), "66");
            Assert.Equal(66.0, vm.Draft.Cell(1, 0, 0)!.Alto);
            Assert.Equal(60.0, vm.Draft.Cell(0, 0, 0)!.Alto);

            // «Fondo 1» has two fronts: a selection of F3 is cleaned.
            vm.SelectFront(2);
            vm.FondoScopeIndex = 1;
            Assert.Equal(2, vm.Columns.Count);
            Assert.All(vm.SelectedPositions, p => Assert.True(p.FrontIndex < 2));

            // A list that is replaced loses its selection (-1): that is not a change of the scope.
            vm.FondoScopeIndex = -1;
            Assert.Equal(1, vm.FondoScopeIndex);
        }

        [Fact]
        public void AWriteOfCellsAndOfFrontsOverAllTheFondosSaysWhatItOmitted()
        {
            var vm = SelectiveEditor(TwoFondos());

            // A cell of F3 under «Todos los fondos»: written in fondo 2, omitted in fondo 1, and said.
            vm.ClickCell(2, 0, control: false, shift: false);
            Assert.Contains("F3 solo existe en el fondo 2", vm.InspectorNote);
            Write(Field(vm.CellFields, "cHeight"), "50");
            Assert.Equal(50.0, vm.Draft.Cell(1, 2, 0)!.Alto);
            Assert.Equal("Parte de lo elegido no existe en todos los fondos: se escribió solo donde existe.", vm.Message);
            Assert.False(vm.MessageIsError);

            // The same for a front: the levels of F3.
            Write(Field(vm.FrontFields, "fLevels"), "4");
            Assert.Equal(4, vm.Draft.LevelCount(1, 2));
            Assert.Equal("Parte de lo elegido no existe en todos los fondos: se escribió solo donde existe.", vm.Message);

            // A cell that both fondos have is written in both, with nothing to say.
            vm.ClickCell(0, 1, control: false, shift: false);
            Write(Field(vm.CellFields, "cCount"), "3");
            Assert.Equal(3, vm.Draft.Cell(0, 0, 1)!.PalletCount);
            Assert.Equal(3, vm.Draft.Cell(1, 0, 1)!.PalletCount);
            Assert.Equal(string.Empty, vm.Message);

            // A front write: «Larguero a piso» and the manual height of F2 in both fondos.
            vm.SelectFront(1);
            vm.SelectionFloorBeam = true;
            Assert.True(vm.Draft.Front(0, 1)!.FloorBeam);
            Assert.True(vm.Draft.Front(1, 1)!.FloorBeam);
            Write(Field(vm.FrontFields, "fHeight"), "150");
            Assert.Equal(150.0, vm.Draft.Front(0, 1)!.HeightOverride);
            Assert.Equal(150.0, vm.Draft.Front(1, 1)!.HeightOverride);
            Write(Field(vm.FrontFields, "fHeight"), string.Empty);
            Assert.Null(vm.Draft.Front(0, 1)!.HeightOverride);
        }

        [Fact]
        public void ARefusedValueStaysInItsFieldAndTheDraftIsUntouched()
        {
            var vm = SelectiveEditor();
            var valid = Json(vm.Draft.Design);

            var height = Field(vm.CellFields, "cHeight");
            Write(height, "abc");
            Assert.Equal("El alto de tarima debe ser un número mayor que cero.", height.Error);
            Write(Field(vm.FrontFields, "fLevels"), "0");
            Assert.Equal("El número de niveles debe ser un entero mayor o igual que 1.", Field(vm.FrontFields, "fLevels").Error);
            Write(Field(vm.Essentials, "fondos"), "5");
            Assert.Equal("El número de fondos debe ser un entero de 1 a 4.", Field(vm.Essentials, "fondos").Error);

            Assert.Equal(valid, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void ThePeraltesOfTheLargueroAreThoseOfTheCatalog()
        {
            var vm = SelectiveEditor();
            var allowed = vm.Draft.BeamPeraltes(BeamId);
            var peralte = Choice(vm.CellFields, "cPeralte");
            Assert.Equal(allowed.Select(p => p.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)), peralte.Options.Select(o => o.Value));
            Assert.Equal("4", peralte.SelectedValue);
            Assert.Contains(Choice(vm.CellFields, "cBeam").Options, o => o.Value == BeamId);

            peralte.SelectedValue = allowed.Last().ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal(allowed.Last(), vm.Draft.Cell(0, 0, 0)!.BeamPeralte);
            Assert.Equal(string.Empty, Choice(vm.CellFields, "cPeralte").Error);
        }

        // ------------------------------------------------------------------ the holguras (decision 12)

        [Fact]
        public void ALinkedHolguraShowsItsEffectiveValueReadOnlyWithItsVariableAndWithNoEquals()
        {
            var vm = SelectiveEditor(bound: true);

            var clearance = Field(vm.Essentials, "clearance");
            Assert.True(clearance.IsReadOnly);
            Assert.Equal("10", clearance.Text);
            Assert.Equal("La gobierna la variable de proyecto «Holgura estándar»: en esta ventana es de solo lectura.", clearance.Hint);
            // Nothing in the editor offers «=Variable»: no value is a link and no field speaks of one.
            var everyField = vm.Essentials.Concat(vm.SeparatorFields).Concat(vm.AnnotationFields).Concat(vm.CellFields).Concat(vm.FrontFields)
                .Concat(vm.PostRows.Select(r => r.Field)).OfType<DynamicFieldItem>().ToList();
            Assert.DoesNotContain(everyField, f => f.Text.Contains('=') || f.Label.Contains("variable", StringComparison.OrdinalIgnoreCase));

            clearance.Text = "7";
            clearance.Commit();
            Assert.Equal(10.0, vm.Draft.VerticalClearance);
            Assert.False(vm.IsDirty);

            // The tolerance belongs to the rack: it is written, and the final states keep the binding of the clearance.
            var tolerance = Field(vm.Essentials, "tolerance");
            Assert.False(tolerance.IsReadOnly);
            Write(tolerance, "3.5");
            Assert.Equal(3.5, vm.Draft.PalletTolerance);
            Assert.True(vm.IsDirty);
            Assert.True(vm.Draft.SnapshotForWrite()!.LinkedPropertyFinalStates[Clearance].IsReference);
        }

        // ------------------------------------------------------------------ the recomposition

        [Fact]
        public void AChangeThatCannotBeRecomposedGoesBackToTheLastValidOneWithTheSelectionAndDoesNotTurnOffUpdate()
        {
            // I-09 of Dinámico: the refused change does not leave its error behind.
            var updates = 0;
            var vm = SelectiveEditor(update: _ => updates++);
            Write(Field(vm.CellFields, "cHeight"), "72");
            Assert.True(vm.CanUpdate);
            var valid = Json(vm.Draft.Design);
            vm.SelectFront(1);
            var post = vm.Draft.PostId;

            // A post that the catalog does not have reaches the draft, which cannot recompose it.
            Choice(vm.AnnotationFields, "post").SelectedValue = "POSTE_QUE_NO_EXISTE";

            Assert.Equal("Selecciona un poste.", Choice(vm.AnnotationFields, "post").Error);
            Assert.Equal("Selecciona un poste.", vm.Message);
            Assert.True(vm.MessageIsError);
            Assert.Equal(post, vm.Draft.PostId);
            Assert.Equal(post, Choice(vm.AnnotationFields, "post").SelectedValue);
            Assert.Null(vm.Draft.LastError);
            Assert.Equal(valid, Json(vm.Draft.Design));
            Assert.Equal(new[] { 1 }, vm.SelectedFronts);
            Assert.Equal(2, vm.SelectedPositions.Count);
            Assert.True(vm.IsDirty);
            Assert.True(vm.CanUpdate, vm.UpdateBlockedText);
            Assert.Equal(string.Empty, vm.UpdateBlockedText);
            vm.RequestWrite();
            Assert.Equal(1, updates);
        }

        [Fact]
        public void ARackThatDoesNotRecomposeWhenItOpensSaysWhyAndCannotBeUpdated()
        {
            // S-08: LoadFrom answered false.
            var broken = Structure();
            broken.PostPeralte = 0.0;
            var draft = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
            Assert.False(draft.LoadFrom(broken, null, "Selectivo A"));
            var updates = 0;
            var vm = new SelectiveEditorViewModel(draft, RackId, _ => updates++);

            Assert.True(vm.IsLoadFailed);
            Assert.False(vm.CanUpdate);
            Assert.False(vm.CanEdit);
            Assert.Equal("No se pudo recomponer el rack al abrirlo: Peralte de poste inválido. Aquí queda de solo lectura; edítalo con RACKEDITAR (el editor clásico).",
                vm.UpdateBlockedText);
            Assert.Equal(vm.UpdateBlockedText, vm.Message);
            Assert.True(vm.MessageIsError);
            Assert.All(vm.Essentials.OfType<DynamicFieldItem>(), f => Assert.True(f.IsReadOnly));
            vm.RequestWrite();
            Assert.Equal(0, updates);

            // Nothing to lose: it closes at once.
            var closed = false;
            vm.CloseRequested += _ => closed = true;
            vm.RequestClose();
            Assert.True(closed);
        }

        [Fact]
        public void TheNoticeOfTheLoadIsSaidWhenTheEditorOpens()
        {
            var unknown = Structure();
            unknown.PostId = "POSTE_X";
            var vm = SelectiveEditor(unknown);

            Assert.StartsWith("El poste «POSTE_X» no está en el catálogo", vm.Message);
            Assert.False(vm.MessageIsError);
        }

        // ------------------------------------------------------------------ the loss that was silent (S-09)

        [Fact]
        public void FewerFrontsThatDropACustomCabeceraAndAnOwnPeralteSayItAfterTheWrite()
        {
            var design = Structure();
            design.Bays.Add(Bay(2));
            design.PostPeraltes.Clear();
            foreach (var peralte in new[] { 0.0, 0.0, 0.0, 4.5 }) design.PostPeraltes.Add(peralte);
            foreach (var cabecera in new[] { null, null, null, Custom(120.0) }) design.PostCabeceras.Add(cabecera);
            var vm = SelectiveEditor(design);
            Assert.True(vm.Draft.HasCustomCabecera(0, 3));
            Assert.Equal(4.5, vm.Draft.PostPeralteOverride(3));
            Assert.Contains("personalizada", vm.PostRows[3].Cabecera);

            Write(Field(vm.Essentials, "fronts"), "2");

            Assert.Equal(2, vm.Draft.FrontCount(0));
            Assert.Equal("Al reducir los frentes se quitaron 1 cabecera personalizada y 1 peralte propio de los postes que ya no existen, como en el editor clásico. "
                         + "Volver a subirlos no los recupera; «Descartar» vuelve a lo último que se leyó o se escribió.", vm.Message);
            Assert.False(vm.MessageIsError);
            Assert.Equal(3, vm.PostRows.Count);

            // Growing again does not bring them back, and says nothing.
            Write(Field(vm.Essentials, "fronts"), "3");
            Assert.False(vm.Draft.HasCustomCabecera(0, 3));
            Assert.Equal(string.Empty, vm.Message);

            // «Descartar» does.
            vm.Discard();
            Assert.True(vm.Draft.HasCustomCabecera(0, 3));
            Assert.Equal(4.5, vm.Draft.PostPeralteOverride(3));
        }

        [Fact]
        public void FewerFondosThatDropACustomCabeceraSayItToo()
        {
            var design = TwoFondos();
            design.ExtraFondoPostCabeceras.Add(new List<RackCad.Domain.RackFrames.RackFrameConfiguration> { Custom(96.0) });
            var vm = SelectiveEditor(design);
            Assert.True(vm.Draft.HasCustomCabecera(1, 0));

            Write(Field(vm.Essentials, "fondos"), "1");

            Assert.Equal(1, vm.Draft.FondoCount);
            Assert.StartsWith("Al reducir los fondos se quitaron 1 cabecera personalizada de los postes que ya no existen", vm.Message);
            Assert.False(vm.HasFondos);
        }

        [Fact]
        public void FewerFrontsWithNothingCustomSayNothing()
        {
            var vm = SelectiveEditor();
            Write(Field(vm.Essentials, "fronts"), "1");
            Assert.Equal(1, vm.Draft.FrontCount(0));
            Assert.Equal(string.Empty, vm.Message);
        }

        // ------------------------------------------------------------------ the rack, the posts and the half front

        [Fact]
        public void TheRackTabWritesTheEssentialsTheAnnotationsAndShowsTheCalculatedValues()
        {
            var vm = SelectiveEditor(TwoFondos());

            // The separation and the depth of the cabecera go by fondo: the table «Fondos» (the approved draft).
            Write(Field(vm.SeparatorFields, "sep0"), "14");
            Assert.Equal(new[] { 14.0 }, vm.Draft.Separators);
            Assert.Same(vm.FondoRows[0].Separator, Field(vm.SeparatorFields, "sep0"));
            Write(vm.FondoRows[0].Cabecera, "38");
            Assert.Equal(38.0, vm.Draft.Fondo(0)!.CabeceraDepthOverride);
            Assert.Null(vm.Draft.Fondo(1)!.CabeceraDepthOverride);
            Write(vm.FondoRows[1].Cabecera, "38");
            Assert.Equal(38.0, vm.Draft.Fondo(1)!.CabeceraDepthOverride);
            // The post is in «Poste y anotaciones» (the approved draft); T5 pp: its peralte, for every post, in «Lo esencial» › «Peralte de poste».
            Write(Field(vm.Essentials, "peralte"), "3.5");
            Assert.Equal(3.5, vm.Draft.PostPeralte);

            vm.DrawPallets = true;
            Assert.True(vm.Draft.DrawPallets);
            vm.NumberLevels = true;
            Assert.True(vm.Draft.NumberLevels);
            Choice(vm.AnnotationFields, "dims").SelectedValue = nameof(RackCad.Domain.Systems.Shared.DimensionDetail.Detailed);
            Assert.Equal(RackCad.Domain.Systems.Shared.DimensionDetail.Detailed, vm.Draft.Dimensions);
            vm.DimensionsPlanta = false;
            Assert.False(vm.Draft.ShowsDimensions(RackCad.Application.Systems.Shared.DimensionViewKind.Planta));
            Assert.True(vm.DimensionsFrontal);
            Write(Field(vm.AnnotationFields, "scale"), "2");
            Assert.Equal(2.0, vm.Draft.AnnotationScale);

            // «Calculado» as the approved draft has it (with two fondos, «Separadores» too).
            var labels = vm.Calculated.Select(c => c.Label).ToList();
            Assert.Equal(new[] { "Largo del larguero", "Altura de cabecera", "Separación N2 → N3", "Profundidad de cabecera", "Cabeceras", "Largueros",
                "Posiciones de tarima", "Separadores" }, labels);
            Assert.Equal(vm.Draft.System!.Height.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + " in", // T4 seg, SD-14: no «≈»
                vm.Calculated.Single(c => c.Label == "Altura de cabecera").Value);
            Assert.All(vm.Calculated, c => Assert.False(string.IsNullOrWhiteSpace(c.Hint)));
        }

        [Fact]
        public void TheOwnPeralteOfAPostEmptyTakesTheOneOfTheRackAndACustomCabeceraIsShown()
        {
            var vm = SelectiveEditor();
            Assert.Equal(3, vm.PostRows.Count);
            Assert.Equal("5", vm.PostRows[0].Field.Text);
            Assert.Equal("Cabecera calculada", vm.PostRows[0].Cabecera);

            Write(vm.PostRows[0].Field, string.Empty);
            Assert.Null(vm.Draft.PostPeralteOverride(0));
            Write(vm.PostRows[2].Field, "4.5");
            Assert.Equal(4.5, vm.Draft.PostPeralteOverride(2));
            // The rule of the classic box: the peralte of the rack is «takes the peralte of the rack».
            Write(vm.PostRows[1].Field, "3");
            Assert.Null(vm.Draft.PostPeralteOverride(1));
            Write(vm.PostRows[1].Field, "-2");
            Assert.Equal("Peralte de poste inválido (deja vacío para usar el peralte del rack).", vm.PostRows[1].Field.Error);
        }

        [Fact]
        public void TheTramosOfAHalfFrontAreWrittenInTheInspectorWithTheChecksOfTheClassicDialog()
        {
            var vm = SelectiveEditor();
            vm.SelectFront(0);
            Assert.Equal(0, vm.SegmentFront);
            Assert.True(vm.CanStartSegments);
            Assert.StartsWith("F1 es un frente de un solo tramo", vm.SegmentNote);

            vm.StartSegments();
            Assert.Equal(2, vm.SegmentRows.Count);
            Assert.True(vm.SegmentRows[1].IsRest);
            // The tramos are written with «Aplicar tramos»; until then they are a change that «Actualizar» does not write (S-21, M-04 of the
            // review of the entrega 1: before, the editor stayed «Limpio» here).
            Assert.False(vm.Draft.Front(0, 0)!.IsSplit);
            Assert.True(vm.HasPendingSegments);
            Assert.True(vm.IsDirty);
            Assert.False(vm.CanUpdate);

            vm.ApplySegments();
            Assert.Equal("El tramo 1 necesita una longitud mayor que 0 (el último se calcula solo).", vm.SegmentError);
            Assert.False(vm.Draft.Front(0, 0)!.IsSplit);

            vm.SegmentRows[0].LengthText = "30";
            // Another value written meanwhile keeps the tramos of the inspector.
            Write(Field(vm.CellFields, "cCount"), "1");
            Assert.Equal("30", vm.SegmentRows[0].LengthText);
            vm.ApplySegments();
            Assert.Equal(string.Empty, vm.SegmentError);
            Assert.True(vm.Draft.Front(0, 0)!.IsSplit);
            Assert.Equal("F1 ½", vm.Columns[0].Title); // decision 7
            Assert.Contains("F1: 2 tramos", vm.HalfFrontsText);
            Assert.True(vm.IsDirty);
            Assert.False(vm.HasPendingSegments);
            Assert.True(vm.CanUpdate);

            // Several fronts: the inspector asks for one.
            vm.SelectAll();
            Assert.Null(vm.SegmentFront);
            Assert.Equal("Elige un solo frente para partirlo en tramos.", vm.SegmentNote);

            vm.SelectFront(0);
            vm.ClearSegments();
            Assert.False(vm.Draft.Front(0, 0)!.IsSplit);
            Assert.Equal("F1", vm.Columns[0].Title);
        }

        // ------------------------------------------------------------------ the view

        // Binding errors of WPF, caught while the view is built and laid out (the STA thread of the tests is one, so they are this test's).
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

        [Fact]
        public void TheViewIsBuiltWithItsThreeTabsWithNoBindingErrorAndPaintsTheMatrix()
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
                    var vm = SelectiveEditor(TwoFondos(), bound: true);
                    vm.ShowDetail = true;
                    var view = new SelectiveEditorView(vm);
                    Lay(view);

                    // Census (E2 seguridad): «Extremos y seguridad» goes between Matriz and Avanzado, as in Dinámico; its bindings are checked by
                    // SelectiveSafetyTabTests.
                    // Cuarta tanda (va), VA-07: «Propiedades» goes at the end of the editor; its bindings are checked by RackCadRackPropertiesTabTests.
                    Assert.Equal(new object[] { "Rack", "Matriz", "Extremos y seguridad", "Avanzado", "Propiedades" }, view.Tabs.Items.Cast<TabItem>().Select(t => t.Header).ToArray());
                    Assert.Same(view.MatrixTab, view.Tabs.SelectedItem);

                    // The matrix: three fronts by three levels of fondo 2, with the cell buttons.
                    var cells = Descendants<Button>(view.MatrixRows).Where(b => b.DataContext is SelectiveCellItem).ToList();
                    Assert.Equal(9, cells.Count);
                    Assert.Contains(Descendants<TextBlock>(view.MatrixRows), t => t.Text == "40×60");
                    Assert.Contains(Descendants<Button>(view.ColumnHeads), b => (string)b.Content == "F3");
                    // The approved draft: segmented buttons over the matrix, «Todos los fondos» chosen (no drop-down list, O-D of the smoke).
                    var fondoViews = Descendants<Button>(view.FondoViewList).ToList();
                    Assert.Equal(new[] { "Todos los fondos", "Fondo 1", "Fondo 2" }, fondoViews.Select(b => (string)b.Content).ToArray());
                    Assert.Equal(new[] { "elegido", "", "" }, fondoViews.Select(System.Windows.Automation.AutomationProperties.GetItemStatus).ToArray());

                    // Every tab is laid out once, so every binding is evaluated.
                    view.Tabs.SelectedItem = view.RackTab;
                    view.PostExpander.IsExpanded = true;
                    Lay(view);
                    Assert.True(Descendants<ComboBox>(view.EssentialsList).Any());
                    // Decision 12: no editor of «=Variable» in the view; the linked holgura is a read-only box.
                    Assert.Empty(Descendants<LinkedPropertyEditor>(view));
                    Assert.Contains(Descendants<TextBox>(view.EssentialsList), t => t.Text == "10" && t.IsReadOnly);
                    view.Tabs.SelectedItem = view.AdvancedTab;
                    Lay(view);
                    Assert.Equal(4, Descendants<TextBox>(view.PostRowList).Count());
                    view.Tabs.SelectedItem = view.MatrixTab;
                    Lay(view);
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
        public void TheViewSelectsWithItsButtonsAndACommitKeepsTheCellButtonsAndTheFloorLine()
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor();
                var view = new SelectiveEditorView(vm);
                Lay(view);

                Click(view.SelectAllButton);
                Assert.Equal(5, vm.SelectedPositions.Count);
                Lay(view);

                var cell = Descendants<Button>(view.MatrixRows).First(b => b.DataContext is SelectiveCellItem c && c.FrontIndex == 1 && c.LevelIndex == 1);
                Click(cell);
                Assert.Equal(new[] { P(1, 1) }, vm.SelectedPositions);
                Assert.Equal("Frente 2 · Nivel 2", view.InspectorTitle.Text);

                // A commit refreshes the cells in place: the same button is still in the tree (I-05 of Dinámico).
                Write(Field(vm.CellFields, "cHeight"), "72");
                Lay(view);
                Assert.Contains(cell, Descendants<Button>(view.MatrixRows));
                Assert.Equal("Modificado", view.StatusText.Text);

                // The floor line of F2·N1 (decision 4), and none on F1·N1 (it has «Larguero a piso»).
                var floor = Descendants<Button>(view.MatrixRows).First(b => b.DataContext is SelectiveCellItem c && c.FrontIndex == 1 && c.LevelIndex == 0);
                var beam = Descendants<Button>(view.MatrixRows).First(b => b.DataContext is SelectiveCellItem c && c.FrontIndex == 0 && c.LevelIndex == 0);
                Assert.Contains(Descendants<Border>(floor), b => b.Name == "FloorLine" && b.Visibility == Visibility.Visible);
                Assert.DoesNotContain(Descendants<Border>(beam), b => b.Name == "FloorLine" && b.Visibility == Visibility.Visible);

                // Narrow, the inspector goes below the matrix, as in Dinámico.
                Lay(view, 600, 800);
                Assert.True(view.InspectorBelow);
                Lay(view, 1200, 800);
                Assert.False(view.InspectorBelow);
            });
        }
    }
}
