#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// «Extremos y seguridad» of a Selectivo in the RackCad window, the screen (I-64 A-14, line D, entrega 2, P1; decisions 4, 5, 6 and 13
    /// of the Owner): the tab between Matriz and Avanzado, the marks T and P of the cells for the whole rack, «Seguridad de la celda» of the
    /// inspector (a mixed selection «varía; marcar lo pone en todas», as S-20 and D-11), the cards of the topes and the parrillas with what
    /// the BOM counts, «Extremos y fondos», the table by post with its «Total», and «Editar seguridad…», the dialog of the classic editor
    /// through the modal port of the host (D-09). Generic sample racks of <see cref="SelectiveWindowFixtures"/>, no data of a client.
    /// </summary>
    public sealed class SelectiveSafetyTabTests
    {
        private static RackCatalog Catalog => UiSupport.LoadCatalogSafe();

        private static SelectiveSafetySelection Variant(string type)
            => new SelectiveSafetySelection { ElementId = SelectiveSafetyFamilies.VariantsOfType(Catalog.SafetyElements, type).First().Id, Quantity = 1 };

        private static SelectivePalletDesign With(SelectivePalletDesign design, params string[] types)
        {
            foreach (var type in types) design.SafetySelections.Add(Variant(type));
            return design;
        }

        private static SelectiveEditorViewModel Editor(params string[] types) => SelectiveEditor(With(Structure(), types));

        private static IEnumerable<SelectiveCellItem> Cells(SelectiveEditorViewModel vm) => vm.Rows.SelectMany(r => r.Cells);

        private static SelectiveCellItem CellAt(SelectiveEditorViewModel vm, int front, int level) => Cells(vm).Single(c => c.FrontIndex == front && c.LevelIndex == level);

        // What the BOM of the last valid design counts in a category, on a system of its own.
        private static int Bom(SelectiveEditorViewModel vm, string category)
            => SelectiveBomBuilder.Build(vm.Draft.ResolveCopy()!, vm.Draft.Catalog).Components.Where(c => c.Category == category).Sum(c => c.Quantity);

        private static SelectiveSafetyFamilyCard Card(SelectiveEditorViewModel vm, string letter) => vm.FamilyCards.Single(c => c.Letter == letter);

        // ------------------------------------------------------------------ T and P in «Matriz»

        [Fact]
        public void The_matrix_marks_T_and_P_where_there_is_a_larguero_and_not_on_the_floor()
        {
            var vm = Editor(SelectiveSafetyDefaults.TopeType, SelectiveSafetyDefaults.ParrillaType);

            // F1 has «Larguero a piso» (N1..N3 are largueros); F2 rests on the floor at N1 (decision 4): no larguero, no T nor P.
            foreach (var (front, level) in new[] { (0, 0), (0, 1), (0, 2), (1, 1) })
            {
                var cell = CellAt(vm, front, level);
                Assert.True(cell.ShowsTope && cell.ShowsParrilla, cell.AccessibleText);
                Assert.False(cell.TopeVaries || cell.ParrillaVaries);
                Assert.Contains("con tope", cell.AccessibleText);
                Assert.Contains("con parrilla", cell.AccessibleText);
            }

            var floor = CellAt(vm, 1, 0);
            Assert.True(floor.IsOnFloor);
            Assert.False(floor.ShowsTope || floor.ShowsParrilla);
            Assert.DoesNotContain("con tope", floor.AccessibleText);

            // Without a variant, no mark.
            var none = Editor();
            Assert.DoesNotContain(Cells(none), c => c.ShowsTope || c.ShowsParrilla);
        }

        [Fact]
        public void Turning_T_off_takes_its_mark_out_and_the_cell_says_it_is_modified_in_the_draft()
        {
            var vm = Editor(SelectiveSafetyDefaults.TopeType);
            Assert.DoesNotContain(Cells(vm), c => c.IsModified);

            vm.ClickCell(0, 0, control: false, shift: false);
            Assert.Null(vm.SetTope(false));
            Assert.False(CellAt(vm, 0, 0).ShowsTope);
            Assert.True(CellAt(vm, 0, 1).ShowsTope);
            Assert.True(vm.IsDirty);

            // The approved draft: a change of T or P is a change of the cell («Modificada en el borrador»), and of no other cell.
            Assert.True(CellAt(vm, 0, 0).IsModified, CellAt(vm, 0, 0).AccessibleText);
            Assert.Contains("modificada en el borrador", CellAt(vm, 0, 0).AccessibleText);
            Assert.Equal(new[] { (0, 0) }, Cells(vm).Where(c => c.IsModified).Select(c => (c.FrontIndex, c.LevelIndex)));

            // Put back, the cell is the drawing again.
            Assert.Null(vm.SetTope(true));
            Assert.False(vm.IsDirty);
            Assert.DoesNotContain(Cells(vm), c => c.IsModified);
        }

        [Fact]
        public void With_fondos_that_disagree_the_mark_of_all_the_fondos_is_in_italics_and_each_fondo_shows_its_own()
        {
            var design = TwoFondos();
            design.ExtraFondoBays[0][0].FloorBeam = false; // F1: fondo 1 with «Larguero a piso», fondo 2 on the floor
            var vm = SelectiveEditor(With(design, SelectiveSafetyDefaults.ParrillaType));

            // From «Fondo 2», P off at F1 N2: larguero 1 of F1, which is N1 of fondo 1 and N2 of fondo 2.
            vm.FondoScopeIndex = 2;
            vm.ClickCell(0, 1, control: false, shift: false);
            Assert.Null(vm.SetParrilla(false));
            Assert.False(CellAt(vm, 0, 1).ShowsParrilla);

            vm.FondoScopeIndex = 0;
            var mixed = CellAt(vm, 0, 1);
            Assert.True(mixed.ShowsParrilla && mixed.ParrillaVaries, mixed.AccessibleText);
            Assert.Equal("Parrilla: varía entre los fondos de la vista", mixed.ParrillaTip);
            Assert.Contains("la parrilla varía entre los fondos", mixed.AccessibleText);
            Assert.False(CellAt(vm, 0, 0).ShowsParrilla); // the same larguero, at N1 of fondo 1 (fondo 2 has no larguero there)

            vm.FondoScopeIndex = 1;
            Assert.True(CellAt(vm, 0, 1).ShowsParrilla && !CellAt(vm, 0, 1).ParrillaVaries);
            Assert.False(CellAt(vm, 0, 0).ShowsParrilla);
        }

        // ------------------------------------------------------------------ «Seguridad de la celda»

        [Fact]
        public void The_box_of_the_inspector_takes_T_out_of_the_selection_and_a_click_on_varies_puts_it_everywhere()
        {
            var vm = Editor(SelectiveSafetyDefaults.TopeType);
            vm.SelectFront(0);
            Assert.Equal(true, vm.SelectionTopeCheck);
            Assert.Equal("Tope (T)", vm.SelectionTopeLabel);
            Assert.True(vm.CanEditSelectionTope);

            vm.SelectionTopeCheck = false;
            Assert.Equal(false, vm.SelectionTopeCheck);
            Assert.All(new[] { 0, 1, 2 }, l => Assert.False(CellAt(vm, 0, l).ShowsTope));
            Assert.True(CellAt(vm, 1, 1).ShowsTope); // F2 untouched

            // Only N2 back on: the front varies, and says what a click does.
            vm.ClickCell(0, 1, control: false, shift: false);
            vm.SelectionTopeCheck = true;
            vm.SelectFront(0);
            Assert.Null(vm.SelectionTopeCheck);
            Assert.Equal("Tope (varía; marcar lo pone en todas)", vm.SelectionTopeLabel);

            // WPF turns a click on an indeterminate box into false: here that means «all» (S-20, D-11).
            vm.SelectionTopeCheck = false;
            Assert.Equal(true, vm.SelectionTopeCheck);
            Assert.All(new[] { 0, 1, 2 }, l => Assert.True(CellAt(vm, 0, l).ShowsTope));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void A_box_with_no_variant_or_a_selection_on_the_floor_is_disabled_and_says_why()
        {
            var vm = Editor(SelectiveSafetyDefaults.TopeType);
            vm.ClickCell(0, 0, control: false, shift: false);
            Assert.Equal("Parrilla: sin variante elegida", vm.SelectionParrillaLabel);
            Assert.False(vm.CanEditSelectionParrilla);
            Assert.Equal(false, vm.SelectionParrillaCheck);
            Assert.Equal("Van por frente y nivel en todo el rack, como hoy. Su variante y sus medidas, en «Extremos y seguridad».", vm.SafetyNote);

            // A click there writes nothing and says why (the box is disabled on screen).
            vm.SelectionParrillaCheck = true;
            Assert.True(vm.MessageIsError);
            Assert.Contains("Elige primero la variante de parrilla", vm.Message);
            Assert.False(vm.IsDirty);

            // F2 N1 rests on the floor: no larguero, no T (decision 4), and the note says so.
            vm.ClickCell(1, 0, control: false, shift: false);
            Assert.Equal("Tope: no en el piso sin larguero", vm.SelectionTopeLabel);
            Assert.False(vm.CanEditSelectionTope);
            Assert.EndsWith("Una celda de la selección está en el piso, sin larguero: no lleva tope ni parrilla y se deja fuera.", vm.SafetyNote);

            // With two fondos the note says that it is not by fondo.
            var two = SelectiveEditor(With(TwoFondos(), SelectiveSafetyDefaults.TopeType));
            Assert.StartsWith("Van por frente y nivel en todo el rack, no por fondo, como hoy.", two.SafetyNote);
        }

        // ------------------------------------------------------------------ the cards and «Extremos y fondos»

        [Fact]
        public void The_cards_say_where_T_and_P_are_and_how_many_pieces_the_BOM_counts()
        {
            var vm = Editor(SelectiveSafetyDefaults.TopeType, SelectiveSafetyDefaults.ParrillaType);
            var tope = Card(vm, "T");
            var parrilla = Card(vm, "P");
            Assert.Equal(new[] { "Topes", "Parrillas" }, vm.FamilyCards.Select(c => c.Title));

            var topes = Bom(vm, SelectiveBomBuilder.Tope);
            var parrillas = Bom(vm, SelectiveBomBuilder.Parrilla);
            Assert.True(topes > 0 && parrillas > 0);
            Assert.Equal("En 4 de 4 celdas con larguero · " + topes + " piezas.", tope.Summary);
            Assert.Equal("Saque 3 in · en los postes posteriores · en la frontal: no.", tope.Config);
            Assert.Equal("En 4 de 4 celdas con larguero · " + parrillas + " piezas.", parrilla.Summary);
            Assert.Equal("Se dibujan en la frontal y la lateral · una por tarima, con su ancho · cantidad automática.", parrilla.Config);
            Assert.False(tope.CanAll);
            Assert.True(tope.CanNone);

            // «En ninguna»: the variant stays (C2-07), nothing is placed, and the BOM counts none. The same card objects are refreshed.
            Assert.Null(vm.SetFamilyEverywhere(SelectiveSafetyDefaults.TopeType, false));
            Assert.Same(tope, Card(vm, "T"));
            Assert.Equal(0, Bom(vm, SelectiveBomBuilder.Tope));
            Assert.Equal("En 0 de 4 celdas con larguero · 0 piezas.", tope.Summary);
            Assert.True(tope.HasConfig);
            Assert.True(tope.CanAll);
            Assert.False(tope.CanNone);
            Assert.DoesNotContain(Cells(vm), c => c.ShowsTope);
            Assert.Null(vm.SetFamilyEverywhere(SelectiveSafetyDefaults.TopeType, true));
            Assert.Equal("En 4 de 4 celdas con larguero · " + topes + " piezas.", Card(vm, "T").Summary); // read again: counted when read (the tab is not shown)

            // No variant: nothing to write.
            var none = Editor(SelectiveSafetyDefaults.TopeType);
            var unchosen = Card(none, "P");
            Assert.Equal("Sin variante elegida: no se dibujan.", unchosen.Summary);
            Assert.False(unchosen.HasConfig);
            Assert.False(unchosen.CanAll || unchosen.CanNone);
        }

        [Fact]
        public void The_card_of_the_topes_of_two_fondos_says_how_they_go_and_the_parrillas_say_all_the_fondos()
        {
            var vm = SelectiveEditor(With(TwoFondos(), SelectiveSafetyDefaults.TopeType, SelectiveSafetyDefaults.ParrillaType));
            Assert.Equal("Saque 3 in · uno central, compartido entre fondos · en la frontal: no.", Card(vm, "T").Config);
            Assert.EndsWith(" piezas, en todos los fondos.", Card(vm, "P").Summary);
            Assert.EndsWith(Bom(vm, SelectiveBomBuilder.Parrilla) + " piezas, en todos los fondos.", Card(vm, "P").Summary);
        }

        [Fact]
        public void Extremos_y_fondos_says_what_each_family_places_and_its_measures()
        {
            var design = TwoFondos();
            var lateral = Variant(SelectiveSafetyDefaults.LateralType);
            lateral.Side = SafetySide.None;
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 3, Side = SafetySide.Left });
            var desviador = Variant(SelectiveSafetyDefaults.DesviadorType);
            desviador.Side = SafetySide.Left;
            design.SafetySelections.Add(lateral);
            design.SafetySelections.Add(desviador);
            var vm = SelectiveEditor(design);

            Assert.Equal(new[]
            {
                "Protector lateral: en los postes de los extremos, P1 y P4.",
                "Botas: sin variante elegida.",
                "Desviadores: longitud " + Num(SelectiveSafetyDefaults.DesviadorLongitud) + " in; el primero a "
                    + Num(SelectiveSafetyDefaults.DesviadorPrimerNivelAltura) + " in sobre el primer troquel; por poste y nivel.",
                "Separadores entre fondos: fondo 1–2, 10 in. Se cambian en «Rack» › «Fondos».",
            }, vm.EndLines);

            var one = Editor(SelectiveSafetyDefaults.BotaType);
            var booted = one.PostSafety.Count(r => r.Boots != "—");
            Assert.Equal(new[]
            {
                "Protector lateral: sin variante elegida.",
                "Botas: en " + booted + " de 3 postes, el lado de cada uno en «Seguridad por poste»; un poste puede tener la suya.",
                "Desviadores: sin variante elegida.",
                "Separadores: no hay, el rack tiene un solo fondo.",
            }, one.EndLines);
        }

        private static string Num(double value) => value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

        // ------------------------------------------------------------------ the table by post and what reads the BOM

        [Fact]
        public void The_table_of_the_view_ends_with_its_Total_and_adds_up_to_the_BOM()
        {
            var vm = Editor(SelectiveSafetyDefaults.BotaType);
            var table = vm.PostSafetyTable;
            Assert.Equal(vm.PostSafety.Select(r => r.Post).Concat(new[] { "Total" }), table.Select(r => r.Post));
            Assert.True(table.Last().IsTotal);
            Assert.DoesNotContain(table.Take(table.Count - 1), r => r.IsTotal);
            Assert.Equal(vm.PostSafetyTotal.Boots, table.Last().Boots);

            var botas = SelectiveBomBuilder.Build(vm.Draft.ResolveCopy()!, vm.Draft.Catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Safety)
                .Where(c => vm.Draft.Catalog.SafetyElements.Any(e => string.Equals(e.Id, c.ProfileId, StringComparison.OrdinalIgnoreCase)
                                                                 && SelectiveSafetyDefaults.IsType(e.Type, SelectiveSafetyDefaults.BotaType)))
                .Sum(c => c.Quantity);
            Assert.True(botas > 0);
            Assert.Equal(botas.ToString(), table.Last().Boots);
        }

        // The BOM and the plans of the safety impose the depth of its fondo on every custom cabecera they read, in place (ARQ-43-10). Read on
        // the system of the draft they would change the design that is written, and the editor would turn «Modificado» on its own when the
        // tab is shown (I-01 of the review of the polish, for «Calculado»): the safety is counted on a system of its own.
        [Fact]
        public void Showing_the_safety_tab_changes_nothing_of_the_design_with_a_custom_cabecera_out_of_its_rule()
        {
            var vm = SelectiveEditor(With(TwoFondosWithACustomCabeceraOutOfItsRule(), SelectiveSafetyDefaults.BotaType, SelectiveSafetyDefaults.LateralType,
                SelectiveSafetyDefaults.TopeType, SelectiveSafetyDefaults.ParrillaType));
            var before = Json(vm.Draft.Design);
            var system = vm.Draft.System!;
            var depth = SelectiveCabeceraAuthority.UsableCustomAt(system, 1, 0)!.Depth;

            Assert.NotEmpty(vm.PostSafety);
            Assert.NotEmpty(vm.PostSafetyTable);
            Assert.NotEmpty(vm.EndLines);
            Assert.NotEmpty(vm.FamilyCards.Select(c => c.Summary));
            Assert.NotEmpty(SelectiveSafetyView.PostRows(vm.Draft));

            Assert.Equal(depth, SelectiveCabeceraAuthority.UsableCustomAt(system, 1, 0)!.Depth);
            Assert.Equal(before, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void The_safety_tab_counts_the_BOM_once_per_recomposition_and_only_while_it_is_shown()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor(SelectiveSafetyDefaults.TopeType);
                var view = new SelectiveEditorView(vm);
                Lay(view);
                var counted = vm.SafetyCountBuilds;

                // Matriz: a write of T recomposes, and the tab that reads the BOM is not shown: nothing is counted.
                vm.ClickCell(0, 0, control: false, shift: false);
                Assert.Null(vm.SetTope(false));
                Lay(view);
                Assert.Equal(counted, vm.SafetyCountBuilds);

                // Shown: counted once, for what the draft is now.
                view.Tabs.SelectedItem = view.SafetyTab;
                Lay(view);
                Assert.Equal(SelectiveEditorViewModel.SafetyTabIndex, vm.SelectedTab);
                Assert.Equal(counted + 1, vm.SafetyCountBuilds);
                Assert.Equal("En 3 de 4 celdas con larguero · " + Bom(vm, SelectiveBomBuilder.Tope) + " piezas.", Card(vm, "T").Summary);

                // A selection does not count it again; a write counts it once.
                vm.ClickCell(1, 1, control: false, shift: false);
                Lay(view);
                Assert.Equal(counted + 1, vm.SafetyCountBuilds);
                Assert.Null(vm.SetTopeEverywhere(true));
                Lay(view);
                Assert.Equal(counted + 2, vm.SafetyCountBuilds);
            });
        }

        // ------------------------------------------------------------------ «Editar seguridad…»

        [Fact]
        public void Edit_safety_takes_the_result_of_the_dialog_and_a_cancel_changes_nothing()
        {
            var vm = Editor(SelectiveSafetyDefaults.TopeType);
            Assert.False(vm.CanEditSafety);
            Assert.Equal("El diálogo de seguridad no está disponible aquí: edita la seguridad con RACKEDITAR (el editor clásico).", vm.SafetyEditText);
            vm.EditSafety();
            Assert.True(vm.MessageIsError);
            Assert.Equal(vm.SafetyEditText, vm.Message);

            var json = Json(vm.Draft.Design);
            SelectiveDraftEditor? seen = null;
            vm.SafetyEditor = draft => { seen = draft; return null; };
            Assert.True(vm.CanEditSafety);
            Assert.StartsWith("Las botas, los protectores laterales, los desviadores, los topes y las parrillas", vm.SafetyEditText);
            vm.EditSafety();
            Assert.Same(vm.Draft, seen);
            Assert.Equal(json, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);

            // «Aceptar» with a parrilla added: the draft takes it, recomposed, and the matrix and the card show it.
            vm.SafetyEditor = draft => draft.CopySafety().Concat(new[] { Variant(SelectiveSafetyDefaults.ParrillaType) }).ToList();
            vm.EditSafety();
            Assert.True(vm.IsDirty);
            Assert.NotNull(SelectiveSafetyView.Selected(vm.Draft, SelectiveSafetyDefaults.ParrillaType));
            Assert.True(CellAt(vm, 0, 0).ShowsParrilla);
            Assert.True(Card(vm, "P").HasConfig);
        }

        [Fact]
        public void While_AutoCAD_writes_the_safety_is_not_edited_and_the_line_says_why()
        {
            var vm = Editor(SelectiveSafetyDefaults.TopeType);
            vm.SafetyEditor = draft => draft.CopySafety();
            vm.BeginWriting();

            Assert.False(vm.CanEditSafety);
            Assert.Equal(vm.WritingText, vm.SafetyEditText);
            Assert.False(vm.CanEditSelectionTope);
            Assert.False(Card(vm, "T").CanNone);
            vm.WriteTaken();
            Assert.Equal(vm.WritingText, vm.SafetyEditText);
            vm.EditSafety();
            Assert.Equal(vm.WritingText, vm.Message);

            vm.EndWriting();
            Assert.True(vm.CanEditSafety);
            Assert.True(Card(vm, "T").CanNone);
        }

        [Fact]
        public void The_safety_dialog_of_the_window_is_the_one_of_the_classic_editor()
        {
            StaTestRunner.Run(() =>
            {
                var design = With(TwoFondos(), SelectiveSafetyDefaults.BotaType, SelectiveSafetyDefaults.TopeType, SelectiveSafetyDefaults.ParrillaType);
                var vm = SelectiveEditor(design);

                // The call of the classic editor (RackSelectiveWindow.Safety_Click), word for word, over its own resolution of the design.
                var catalog = vm.Draft.Catalog;
                var resolved = new SelectiveGeometryResolver().Resolve(Store.Deserialize(Json(vm.Draft.Design))!.ToDomain(), catalog);
                var maxFronts = Math.Max(vm.Draft.FrontCount(0), vm.Draft.FrontCount(1));
                var reference = new SelectiveSafetyWindow(catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>(), vm.Draft.CopySafety(), maxFronts + 1,
                    SelectiveSafetyGrid.LevelCounts(resolved), Math.Max(1, vm.Draft.FondoCount), SelectiveParrillaPlan.Cells(resolved, catalog), catalog, resolved);
                var dialog = RackCadSelectiveSafetyDialog.Build(vm.Draft);
                try
                {
                    Assert.Equal(4, maxFronts + 1);
                    reference.BuildResultForTest();
                    dialog.BuildResultForTest();
                    Assert.Equal(WithSafety(vm.Draft.Design!, reference.Result), WithSafety(vm.Draft.Design!, dialog.Result));

                    vm.SafetyEditor = _ => dialog.Result;
                    vm.EditSafety();
                    Assert.Equal(WithSafety(vm.Draft.Design!, reference.Result), Json(vm.Draft.Design));
                }
                finally
                {
                    dialog.Close();
                    reference.Close();
                }
            });
        }

        // The design with this safety, as the drawing would hold it.
        private static string WithSafety(SelectivePalletDesign design, IEnumerable<SelectiveSafetySelection> safety)
        {
            var copy = Store.Deserialize(Json(design))!.ToDomain();
            copy.SafetySelections.Clear();
            foreach (var selection in safety) copy.SafetySelections.Add(selection.DeepCopy());
            return Json(copy);
        }

        [Fact]
        public void Edit_safety_shows_the_dialog_through_the_port_of_the_host()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor(SelectiveSafetyDefaults.TopeType);
                var port = new ModalDouble();
                Assert.Null(RackCadSelectiveSafetyDialog.Edit(vm.Draft, port));
                var shown = Assert.IsType<SelectiveSafetyWindow>(Assert.Single(port.Shown));
                shown.Close();
                Assert.Null(RackCadSelectiveSafetyDialog.Edit(vm.Draft, null!));

                // Accepted (the double presses «Aceptar»): its result, with the tope of the draft; the draft is not touched by the dialog.
                var json = Json(vm.Draft.Design);
                port.Accept = true;
                var result = RackCadSelectiveSafetyDialog.Edit(vm.Draft, port);
                Assert.NotNull(result);
                Assert.NotNull(SelectiveSafetyFamilies.SelectedOfType(result!, vm.Draft.Catalog.SafetyElements, SelectiveSafetyDefaults.TopeType));
                Assert.Equal(json, Json(vm.Draft.Design));
                Assert.False(vm.IsDirty);
                port.Shown.ForEach(w => w.Close());
            });
        }

        private sealed class ModalDouble : IRackCadModalPort
        {
            public List<Window> Shown { get; } = new();

            public bool Accept { get; set; }

            public bool? ShowModal(Window dialog)
            {
                Shown.Add(dialog);
                if (!Accept) return false;
                ((SelectiveSafetyWindow)dialog).BuildResultForTest();
                return true;
            }
        }

        [Fact]
        public void The_window_offers_the_safety_dialog_only_with_the_modal_port_of_the_host()
        {
            Assert.False(new WindowRig(null).OpenEditor().CanEditSafety);
            StaTestRunner.Run(() =>
            {
                var port = new ModalDouble { Accept = true };
                var editor = new WindowRig(port).OpenEditor();
                Assert.True(editor.CanEditSafety);
                editor.EditSafety();
                var shown = Assert.IsType<SelectiveSafetyWindow>(Assert.Single(port.Shown));
                shown.Close();
                Assert.False(editor.MessageIsError, editor.Message);
            });
        }

        /// <summary>The window over a door of Selectivo (only what «Editar aquí» needs) and, optionally, the modal port of the host.</summary>
        private sealed class WindowRig
        {
            private readonly DoorDouble _door = new();
            private readonly RackCadWindowViewModel _window;

            public WindowRig(IRackCadModalPort? modal)
            {
                var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                var summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[] { new BrowserRackListEntry(RackId, "Selectivo A", "selective", new[] { "frontal" }) }),
                };
                var panel = new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), summary, new WorkspacePilotEditPortDouble());
                _window = new RackCadWindowViewModel(panel, null, null, _door, UiSupport.LoadCatalogSafe, modal);
                _window.Refresh();
            }

            public SelectiveEditorViewModel OpenEditor()
            {
                _window.OpenRack(RackId);
                _window.EditSelectiveHere();
                var request = _door.SelectiveReads.Last();
                _door.Answer(Read(request.RequestId, request.RackId, With(Structure(), SelectiveSafetyDefaults.TopeType)));
                Assert.True(_window.ActiveSelectiveEditor != null, _window.ActionMessage);
                return _window.ActiveSelectiveEditor!;
            }
        }

        private sealed class DoorDouble : IRackCadSelectiveDoorPort
        {
            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten { add { } remove { } }

            public event Action<long>? RequestStarted { add { } remove { } }

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write) => RackCadClassicResult.Started();

            public void Abandon(long requestId)
            {
            }

            public void Answer(SelectiveDesignRead read) => SelectiveRead?.Invoke(read);
        }

        // ------------------------------------------------------------------ the view

        private static void Lay(FrameworkElement view, double width = 1200, double height = 900)
        {
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
            view.UpdateLayout();
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
        {
            for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

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

        [Fact]
        public void The_tab_of_the_safety_goes_between_the_matrix_and_Avanzado()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                var view = new SelectiveEditorView(vm);
                Lay(view);

                Assert.Equal(2, view.Tabs.Items.IndexOf(view.SafetyTab));
                Assert.Equal(3, view.Tabs.Items.IndexOf(view.AdvancedTab));
                Assert.Equal("Extremos y seguridad", view.SafetyTab.Header);
                Assert.Equal(SelectiveEditorViewModel.SafetyTabIndex, view.Tabs.Items.IndexOf(view.SafetyTab));
                Assert.Equal(1, vm.SelectedTab);
            });
        }

        [Fact]
        public void The_safety_tab_is_built_with_no_binding_error_with_the_texts_and_the_columns_of_the_draft()
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
                    var vm = SelectiveEditor(With(TwoFondos(), SelectiveSafetyDefaults.BotaType, SelectiveSafetyDefaults.TopeType));
                    vm.SafetyEditor = draft => null;
                    var view = new SelectiveEditorView(vm);
                    Lay(view);
                    view.Tabs.SelectedItem = view.SafetyTab;
                    Lay(view);

                    var texts = Descendants<TextBlock>(view.SafetyTab.Content as DependencyObject ?? view).Select(t => t.Text).ToList();
                    Assert.Contains("En las celdas: frente × nivel", texts);
                    Assert.Contains("Para elegir celdas, márcalas en «Matriz»: inspector › «Seguridad de la celda».", texts);
                    Assert.Contains("Extremos y fondos", texts);
                    Assert.Contains("Seguridad por poste", texts);
                    Assert.Contains(vm.SafetySummary, texts);
                    Assert.All(vm.EndLines, line => Assert.Contains(line, texts));

                    // The columns of the approved draft, the posts and the «Total».
                    Assert.Equal(new object[] { "Poste", "Botas", "Protector lateral", "Desviadores (niveles)", "Separadores entre fondos" },
                        view.PostSafetyGrid.Columns.Select(c => c.Header).ToArray());
                    Assert.Equal(vm.PostSafety.Count + 1, view.PostSafetyGrid.Items.Count);
                    var total = (SelectivePostSafetyItem)view.PostSafetyGrid.Items[view.PostSafetyGrid.Items.Count - 1];
                    Assert.Equal("Total", total.Post);
                    var totalRow = (DataGridRow)view.PostSafetyGrid.ItemContainerGenerator.ContainerFromItem(total);
                    Assert.Equal(FontWeights.SemiBold, totalRow.FontWeight);

                    // The two cards, each with its two buttons, whose names for UI Automation carry the family (unique in the tab).
                    var buttons = Descendants<Button>(view.FamilyCardList).ToList();
                    Assert.Equal(new[] { "En todas las celdas", "En ninguna", "En todas las celdas", "En ninguna" }, buttons.Select(b => (string)b.Content));
                    var names = buttons.Select(System.Windows.Automation.AutomationProperties.GetName).ToList();
                    Assert.Equal(new[] { "Topes en todas las celdas", "Topes en ninguna celda", "Parrillas en todas las celdas", "Parrillas en ninguna celda" }, names);
                    Assert.Equal(new[] { false, true, false, false }, buttons.Select(b => b.IsEnabled));

                    Assert.True(view.EditSafetyButton.IsEnabled);
                    Assert.Equal("Editar la seguridad en el diálogo del editor clásico", System.Windows.Automation.AutomationProperties.GetName(view.EditSafetyButton));

                    // «En ninguna» of the topes, by its button: the card and the matrix follow.
                    Click(buttons[1]);
                    Lay(view);
                    Assert.StartsWith("En 0 de ", Card(vm, "T").Summary);
                    Assert.Equal(new[] { true, false, false, false }, buttons.Select(b => b.IsEnabled));
                    Assert.Same(buttons[0], Descendants<Button>(view.FamilyCardList).First()); // refreshed in place

                    // The inspector of «Matriz», laid out too.
                    view.Tabs.SelectedItem = view.MatrixTab;
                    Lay(view);
                    Assert.Equal(vm.SelectionTopeLabel, view.TopeCheck.Content);
                    Assert.Equal(vm.SelectionParrillaLabel, System.Windows.Automation.AutomationProperties.GetName(view.ParrillaCheck));
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
        public void The_marks_T_and_P_are_badges_in_their_own_colors_and_the_legend_has_them()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor(SelectiveSafetyDefaults.TopeType, SelectiveSafetyDefaults.ParrillaType);
                var view = new SelectiveEditorView(vm);
                Lay(view);

                Border[] Badges(string name) => Descendants<Border>(view.MatrixRows).Where(b => b.Name == name).ToArray();
                Assert.Equal(4, Badges("TopeBadge").Count(b => b.Visibility == Visibility.Visible)); // F1 N1..N3 and F2 N2
                Assert.Equal(4, Badges("ParrillaBadge").Count(b => b.Visibility == Visibility.Visible));
                var t = Assert.IsType<TextBlock>(Badges("TopeBadge")[0].Child);
                var p = Assert.IsType<TextBlock>(Badges("ParrillaBadge")[0].Child);
                Assert.Equal(("T", "P"), (t.Text, p.Text));
                Assert.Equal(FontWeights.Bold, t.FontWeight);
                Assert.Equal(view.TryFindResource("ShellStatusSuccessBrush"), t.Foreground);
                Assert.Equal(view.TryFindResource("ShellStatusWarningBrush"), p.Foreground);
                Assert.Equal(HorizontalAlignment.Right, Badges("TopeBadge")[0].HorizontalAlignment);
                Assert.Equal(HorizontalAlignment.Left, Badges("ParrillaBadge")[0].HorizontalAlignment);

                var legend = Descendants<TextBlock>(view.MatrixLegend).Select(x => x.Text).ToList();
                Assert.Contains("Tope", legend);
                Assert.Contains("Parrilla", legend);

                vm.SetTopeEverywhere(false);
                Lay(view);
                Assert.All(Badges("TopeBadge"), b => Assert.Equal(Visibility.Collapsed, b.Visibility));
                Assert.Equal(4, Badges("ParrillaBadge").Count(b => b.Visibility == Visibility.Visible));
            });
        }

        [Fact]
        public void A_click_on_the_indeterminate_box_of_the_inspector_puts_the_tope_in_every_cell_of_the_selection()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor(SelectiveSafetyDefaults.TopeType);
                var view = new SelectiveEditorView(vm);
                Lay(view);
                vm.ClickCell(0, 0, control: false, shift: false);
                vm.SelectionTopeCheck = false;
                vm.SelectAll();
                Lay(view);

                var box = view.TopeCheck;
                Assert.Null(box.IsChecked);
                Assert.False(box.IsThreeState);
                Assert.True(box.IsEnabled);
                Assert.Equal("Tope (varía; marcar lo pone en todas)", box.Content);

                var toggle = (System.Windows.Automation.Provider.IToggleProvider)new System.Windows.Automation.Peers.CheckBoxAutomationPeer(box)
                    .GetPattern(System.Windows.Automation.Peers.PatternInterface.Toggle);
                toggle.Toggle();
                Lay(view);

                Assert.All(Cells(vm).Where(c => c.Exists && !c.IsOnFloor), cell => Assert.True(cell.ShowsTope, cell.AccessibleText));
                Assert.Equal(true, box.IsChecked);
                Assert.Equal("Tope (T)", box.Content);
                Assert.False(vm.IsDirty);

                // A second click takes it out of the whole selection, as any box does; the floor cell is left out and said.
                toggle.Toggle();
                Lay(view);
                Assert.DoesNotContain(Cells(vm), cell => cell.ShowsTope);
                Assert.Equal(false, box.IsChecked);
                Assert.Contains("no lleva", vm.Message);
            });
        }
    }
}
