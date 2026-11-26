#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// «Extremos y seguridad» of a Selectivo (entrega 2, part C): T/P per front × larguero for the whole rack, the translation of the levels
    /// of the matrix with the «Larguero a piso» of each fondo (the BOM is the authority), the table by post counted with the projections of
    /// the BOM, and the safety after a recomposition that fails. Generic sample racks of <see cref="SelectiveWindowFixtures"/>, no data of
    /// a client.
    /// </summary>
    public sealed class SelectiveSafetyViewTests
    {
        private static SelectiveSafetySelection Variant(RackCatalog catalog, string type)
            => new SelectiveSafetySelection
            {
                ElementId = SelectiveSafetyFamilies.VariantsOfType(catalog.SafetyElements, type).First().Id,
                Quantity = 1,
            };

        private static SelectiveDraftEditor Editor(SelectivePalletDesign design, params string[] types)
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var editor = new SelectiveDraftEditor(catalog);
            Assert.True(editor.LoadFrom(design, null!, "SEL"), editor.LastError);
            editor.ReplaceSafety(types.Select(type => Variant(catalog, type)));
            Assert.True(editor.Recompose(), editor.LastError);
            return editor;
        }

        private static SelectiveDraftEditor Editor(params string[] types) => Editor(Structure(), types);

        /// <summary>Two fondos whose F1 disagree on «Larguero a piso»: fondo 1 has it (N1..N3 are largueros 1..3), fondo 2 not (N2, N3 are 1, 2).</summary>
        private static SelectivePalletDesign FloorDiffers()
        {
            var design = TwoFondos();
            design.ExtraFondoBays[0][0].FloorBeam = false;
            return design;
        }

        private static int BomOf(SelectiveDraftEditor editor, string category)
            => SelectiveBomBuilder.Build(editor.System!, editor.Catalog).Components.Where(c => c.Category == category).Sum(c => c.Quantity);

        // The cells of the matrix that show P on, fondo by fondo («Fondo n» of the selector), as the window shows them.
        private static int ParrillaShownOn(SelectiveDraftEditor editor)
        {
            var on = 0;
            for (var fondo = 0; fondo < editor.FondoCount; fondo++)
            {
                editor.SetFondoScope(fondo);
                for (var front = 0; front < editor.FrontCount(fondo); front++)
                {
                    for (var level = 0; level < editor.LevelCount(fondo, front); level++)
                    {
                        if (SelectiveSafetyView.Parrilla(editor, front, level) == true) on++;
                    }
                }
            }

            editor.SetFondoScope(null);
            return on;
        }

        private static string SafetyJson(IEnumerable<SelectiveSafetySelection> safety)
        {
            var design = Structure();
            design.SafetySelections.Clear();
            foreach (var selection in safety) design.SafetySelections.Add(selection);
            return Json(design);
        }

        [Fact]
        public void The_grid_is_the_one_of_the_classic_and_has_no_cell_where_there_is_no_larguero()
        {
            var editor = Editor(SelectiveSafetyDefaults.TopeType);

            var counts = SelectiveSafetyView.LevelCounts(editor);
            Assert.Equal(SelectiveSafetyGrid.LevelCounts(editor.System!), counts);
            // F1 has «Larguero a piso» (3 beam levels); F2 rests its first pallet on the floor: one level fewer is a cell.
            Assert.Equal(3, counts[0]);
            Assert.Equal(1, counts[1]);
            Assert.Equal(0, SelectiveSafetyView.GridLevel(editor, 0, 0, 0));
            Assert.Null(SelectiveSafetyView.GridLevel(editor, 0, 1, 0));
            Assert.Equal(0, SelectiveSafetyView.GridLevel(editor, 0, 1, 1));
            Assert.Null(SelectiveSafetyView.GridLevel(editor, 0, 1, 2)); // F2 has two rows
            Assert.True(SelectiveSafetyView.Tope(editor, 0, 0));
            Assert.Null(SelectiveSafetyView.Tope(editor, 1, 0)); // F2 N1 rests on the floor: no larguero, no tope (decision 4)
            Assert.True(SelectiveSafetyView.Tope(editor, 1, 1));
            Assert.Null(SelectiveSafetyView.Parrilla(editor, 0, 0)); // no parrilla chosen
        }

        [Fact]
        public void The_translation_of_each_fondo_agrees_with_the_levels_the_resolver_gives_it()
        {
            var editor = Editor(FloorDiffers(), SelectiveSafetyDefaults.ParrillaType);
            for (var fondo = 0; fondo < editor.FondoCount; fondo++)
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(editor.System!, fondo);
                for (var front = 0; front < editor.FrontCount(fondo); front++)
                {
                    var floorBeam = editor.Front(fondo, front)!.FloorBeam;
                    var grids = Enumerable.Range(0, editor.LevelCount(fondo, front)).Select(level => SelectiveSafetyView.GridLevel(editor, fondo, front, level)).ToList();
                    Assert.Equal(floorBeam ? (int?)0 : null, grids[0]);
                    Assert.Equal(Enumerable.Range(0, bays[front].Levels.Count).Select(l => (int?)l), grids.Where(g => g.HasValue));
                    // the larguero of each row is the one the resolver puts under that pallet
                    for (var level = 0; level < grids.Count; level++)
                    {
                        if (grids[level] is int grid) Assert.Equal(editor.Cell(fondo, front, level)!.Frente, bays[front].Levels[grid].PalletFrente);
                    }
                }
            }
        }

        [Fact]
        public void With_fondos_that_disagree_on_Larguero_a_piso_the_matrix_shows_and_writes_what_the_BOM_counts()
        {
            var editor = Editor(FloorDiffers(), SelectiveSafetyDefaults.ParrillaType);
            Assert.True(BomOf(editor, SelectiveBomBuilder.Parrilla) > 0);
            Assert.Equal(BomOf(editor, SelectiveBomBuilder.Parrilla), 2 * ParrillaShownOn(editor)); // two tarimas, two parrillas per cell

            // Fondo 2, F1·N2 is its FIRST larguero (no «Larguero a piso»): off there is off on the first larguero of F1 in every fondo.
            editor.SetFondoScope(1);
            var edit = SelectiveSafetyView.WriteParrilla(editor, new[] { P(0, 1) }, on: false);
            Assert.False(edit.Refused, edit.Error);
            Assert.Equal(1, edit.Written);
            Assert.Equal(new[] { 0 }, edit.ShiftedFronts);
            Assert.Contains("otro nivel", edit.Notice);
            Assert.True(editor.Recompose(), editor.LastError);

            Assert.False(SelectiveSafetyView.Parrilla(editor, 0, 1));
            Assert.True(SelectiveSafetyView.Parrilla(editor, 0, 2));
            editor.SetFondoScope(0);
            Assert.False(SelectiveSafetyView.Parrilla(editor, 0, 0)); // the same larguero, at N1 of fondo 1
            Assert.True(SelectiveSafetyView.Parrilla(editor, 0, 1));
            editor.SetFondoScope(null);
            Assert.Equal(BomOf(editor, SelectiveBomBuilder.Parrilla), 2 * ParrillaShownOn(editor));
        }

        [Fact]
        public void From_all_the_fondos_a_row_that_is_a_different_larguero_in_each_fondo_is_not_written()
        {
            var editor = Editor(FloorDiffers(), SelectiveSafetyDefaults.ParrillaType);
            var before = SafetyJson(editor.CopySafety());

            var refused = SelectiveSafetyView.WriteParrilla(editor, new[] { P(0, 1) }, on: false);
            Assert.True(refused.Refused);
            Assert.Contains("F1", refused.Error);
            Assert.Contains("Larguero a piso", refused.Error);
            Assert.Equal(before, SafetyJson(editor.CopySafety()));

            // F2 is on the floor in both fondos: its N2 is the same larguero there, and is written; F1 is left with the reason.
            var edit = SelectiveSafetyView.WriteParrilla(editor, new[] { P(0, 1), P(1, 1) }, on: false);
            Assert.Equal(1, edit.Written);
            Assert.Equal(1, edit.Ambiguous);
            Assert.Equal(new[] { 0 }, edit.AmbiguousFronts);
            Assert.Contains("elige un fondo", edit.Notice);
            Assert.True(editor.Recompose(), editor.LastError);
            Assert.False(SelectiveSafetyView.Parrilla(editor, 1, 1));
            Assert.True(SelectiveSafetyView.Parrilla(editor, 0, 1)); // F1 untouched: read over both fondos (larguero 2 of fondo 1, larguero 1 of fondo 2), on in both

            // Read over both fondos, a row whose largueros differ says «varía»: larguero 1 off (from fondo 2) is N1 of fondo 1 and N2 of fondo 2.
            editor.SetFondoScope(1);
            Assert.False(SelectiveSafetyView.WriteParrilla(editor, new[] { P(0, 1) }, on: false).Refused);
            Assert.True(editor.Recompose(), editor.LastError);
            editor.SetFondoScope(null);
            Assert.True(SelectiveSafetyView.ReadParrilla(editor, new[] { P(0, 1) }).Varies); // fondo 1: larguero 2 on; fondo 2: larguero 1 off
            Assert.Null(SelectiveSafetyView.Parrilla(editor, 0, 1));
            Assert.False(SelectiveSafetyView.Parrilla(editor, 0, 0)); // only fondo 1 has a larguero at N1, and it is off
        }

        [Fact]
        public void Turning_T_off_at_a_selection_removes_those_topes_from_what_the_BOM_counts()
        {
            var editor = Editor(SelectiveSafetyDefaults.TopeType);
            int Planned() => SelectiveTopePlan.Build(editor.System!, editor.Catalog).Sum(spot => spot.Cells.Sum(c => c.LargueroYs.Count));
            int Spots() => SelectiveTopePlan.Build(editor.System!, editor.Catalog).Count;
            int OnCells() => Enumerable.Range(0, editor.FrontCount(0)).Select(front => editor.LevelCount(0, front))
                .SelectMany((levels, front) => Enumerable.Range(0, levels).Select(level => (front, level)))
                .Count(c => SelectiveSafetyView.Tope(editor, c.front, c.level) == true);

            Assert.Equal(OnCells() * Spots(), Planned());
            Assert.Equal(Planned(), BomOf(editor, SelectiveBomBuilder.Tope));

            var edit = SelectiveSafetyView.WriteTope(editor, new[] { P(0, 0), P(0, 2), P(1, 0) }, on: false);
            Assert.False(edit.Refused, edit.Error);
            Assert.Equal(2, edit.Written);
            Assert.Equal(1, edit.OnFloor); // F2 N1 is not a cell
            Assert.Contains("en el piso", edit.Notice);
            Assert.Empty(edit.ShiftedFronts);
            Assert.True(editor.Recompose(), editor.LastError);

            Assert.False(SelectiveSafetyView.Tope(editor, 0, 0));
            Assert.True(SelectiveSafetyView.Tope(editor, 0, 1));
            Assert.False(SelectiveSafetyView.Tope(editor, 0, 2));
            Assert.True(SelectiveSafetyView.ReadTope(editor, new[] { P(0, 0), P(0, 1) }).Varies);
            Assert.Equal(OnCells() * Spots(), Planned());
            Assert.Equal(Planned(), BomOf(editor, SelectiveBomBuilder.Tope));

            Assert.False(SelectiveSafetyView.WriteTope(editor, new[] { P(0, 0) }, on: true).Refused);
            Assert.True(SelectiveSafetyView.Tope(editor, 0, 0));
        }

        [Fact]
        public void P_is_written_for_the_whole_rack_and_without_a_variant_the_write_is_refused()
        {
            var editor = Editor(SelectiveSafetyDefaults.ParrillaType);
            Assert.False(SelectiveSafetyView.WriteParrilla(editor, new[] { P(0, 1) }, on: false).Refused);
            Assert.False(SelectiveSafetyView.Parrilla(editor, 0, 1));
            Assert.True(SelectiveSafetyView.Parrilla(editor, 0, 0));

            var refused = SelectiveSafetyView.WriteTope(editor, new[] { P(0, 0) }, on: false);
            Assert.True(refused.Refused);
            Assert.Contains("Editar seguridad", refused.Error);
            var onFloor = SelectiveSafetyView.WriteParrilla(editor, new[] { P(1, 0) }, on: false); // only a cell without larguero
            Assert.True(onFloor.Refused);
            Assert.Contains("en el piso", onFloor.Error);
        }

        [Fact]
        public void En_ninguna_keeps_the_variant_places_nothing_and_En_todas_brings_every_cell_back()
        {
            var editor = Editor(SelectiveSafetyDefaults.TopeType);
            var total = SelectiveSafetyView.LevelCounts(editor).Sum();
            Assert.Equal((total, total), SelectiveSafetyView.CellsOn(editor, SelectiveSafetyDefaults.TopeType));

            Assert.False(SelectiveSafetyView.WriteAll(editor, SelectiveSafetyDefaults.TopeType, on: false).Refused);
            Assert.True(editor.Recompose(), editor.LastError);
            Assert.True(SelectiveSafetyView.AllOff(editor, SelectiveSafetyDefaults.TopeType));
            Assert.Equal((0, total), SelectiveSafetyView.CellsOn(editor, SelectiveSafetyDefaults.TopeType));
            Assert.NotNull(SelectiveSafetyView.Selected(editor, SelectiveSafetyDefaults.TopeType));
            Assert.Equal(0, BomOf(editor, SelectiveBomBuilder.Tope));
            Assert.Empty(SelectiveTopePlan.Build(editor.System!, editor.Catalog).SelectMany(spot => spot.Cells));

            Assert.False(SelectiveSafetyView.WriteAll(editor, SelectiveSafetyDefaults.TopeType, on: true).Refused);
            Assert.True(editor.Recompose(), editor.LastError);
            Assert.Equal((total, total), SelectiveSafetyView.CellsOn(editor, SelectiveSafetyDefaults.TopeType));
            Assert.True(SelectiveSafetyView.WriteAll(editor, SelectiveSafetyDefaults.ParrillaType, on: false).Refused); // no parrilla chosen
        }

        [Fact]
        public void A_recomposition_that_fails_after_ReplaceSafety_gives_back_the_safety_of_the_last_valid_one()
        {
            var editor = Editor(SelectiveSafetyDefaults.TopeType, SelectiveSafetyDefaults.BotaType);
            Assert.False(SelectiveSafetyView.WriteTope(editor, new[] { P(0, 1) }, on: false).Refused);
            Assert.True(editor.Recompose(), editor.LastError);
            var valid = SafetyJson(editor.CopySafety());

            editor.ReplaceSafety(new[] { Variant(editor.Catalog, SelectiveSafetyDefaults.ParrillaType) });
            Assert.NotEqual(valid, SafetyJson(editor.CopySafety()));
            editor.PostPeralte = -1.0; // the next recomposition cannot run
            Assert.False(editor.Recompose());

            Assert.True(editor.RestoreLastValid());
            Assert.Equal(valid, SafetyJson(editor.CopySafety()));
            Assert.False(SelectiveSafetyView.Tope(editor, 0, 1));
            Assert.Null(SelectiveSafetyView.Selected(editor, SelectiveSafetyDefaults.ParrillaType));
            Assert.True(editor.Recompose(), editor.LastError);
            Assert.Equal(valid, SafetyJson(editor.System!.SafetySelections));
        }

        [Fact]
        public void The_table_by_post_adds_up_to_what_the_BOM_counts()
        {
            var editor = Editor(SelectiveSafetyDefaults.DesviadorType, SelectiveSafetyDefaults.BotaType);

            var rows = SelectiveSafetyView.PostRows(editor);
            Assert.Equal(3, rows.Count);
            var bom = SelectiveBomBuilder.Build(editor.System!, editor.Catalog);
            int Line(string type)
            {
                var id = SelectiveSafetyFamilies.VariantsOfType(editor.Catalog.SafetyElements, type).First().Id;
                return bom.Components.Where(c => c.ProfileId == id).Sum(c => c.Quantity);
            }

            Assert.True(Line(SelectiveSafetyDefaults.DesviadorType) > 0);
            Assert.Equal(Line(SelectiveSafetyDefaults.DesviadorType), rows.Sum(r => r.Deflectors));
            Assert.Equal(Line(SelectiveSafetyDefaults.BotaType), rows.Sum(r => r.Boots));
            Assert.All(rows, r => Assert.Equal(SelectiveSafetyView.BothSides, SelectiveSafetyView.BootSide(r))); // a new bota: both faces
            Assert.Equal(SelectiveSafetyView.NoSide, SelectiveSafetyView.BootSide(null!));
        }

        [Fact]
        public void The_text_for_a_bota_on_both_faces_is_the_provisional_one_and_lives_in_one_constant()
        {
            Assert.Equal("Izquierda y Derecha", SelectiveSafetyView.BothSides);
            Assert.Equal("Izquierda", SelectiveSafetyView.Left);
            Assert.Equal("Derecha", SelectiveSafetyView.Right);
        }

        [Fact]
        public void The_editor_of_the_window_shows_the_columns_of_the_draft_with_the_total_and_the_summary()
        {
            var design = TwoFondos();
            var catalog = UiSupport.LoadCatalogSafe();
            var bota = Variant(catalog, SelectiveSafetyDefaults.BotaType);
            bota.Side = SafetySide.Both;
            bota.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Left });
            bota.PostSides.Add(new SafetyPostSide { PostIndex = 2, Side = SafetySide.Right });
            var lateral = Variant(catalog, SelectiveSafetyDefaults.LateralType);
            lateral.Side = SafetySide.None;
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 3, Side = SafetySide.Left });
            var desviador = Variant(catalog, SelectiveSafetyDefaults.DesviadorType);
            desviador.Side = SafetySide.Left;
            foreach (var selection in new[] { bota, lateral, desviador }) design.SafetySelections.Add(selection);
            var vm = SelectiveEditor(design);

            var table = vm.PostSafety;
            Assert.Equal(new[] { "P1", "P2", "P3", "P4" }, table.Select(r => r.Post));
            Assert.Equal(new[] { SelectiveSafetyView.BothSides, SelectiveSafetyView.Left, SelectiveSafetyView.Right, "—" }, table.Select(r => r.Boots));
            Assert.Equal(new[] { "—", "—", "—", "Sí, extremo" }, table.Select(r => r.SideGuards));
            Assert.All(table, r => Assert.StartsWith("N1–N", r.Deflectors));
            Assert.All(table.Take(3), r => Assert.NotEqual("—", r.Separators)); // the two fondos reach P1..P3
            Assert.Equal("—", table[3].Separators); // only fondo 2 reaches P4

            var rows = SelectiveSafetyView.PostRows(vm.Draft);
            var total = vm.PostSafetyTotal;
            Assert.Equal("Total", total.Post);
            Assert.Equal("4", total.Boots);
            Assert.Equal("1", total.SideGuards);
            Assert.Equal(rows.Sum(r => r.Deflectors).ToString(), total.Deflectors);
            Assert.Equal(rows.Sum(r => r.Separators).ToString(), total.Separators);
            Assert.Equal("4 botas · 1 protector · " + rows.Sum(r => r.Deflectors) + " desviadores · " + rows.Sum(r => r.Separators)
                         + " separadores, en 4 postes.", vm.SafetySummary);

            // A recomposition gives a new system, and the table is counted again on it.
            Assert.False(vm.Draft.SetFondoCount(1).Refused);
            Assert.True(vm.Draft.Recompose(), vm.Draft.LastError);
            Assert.Equal(3, vm.PostSafety.Count);
            Assert.Equal("—", vm.PostSafetyTotal.Separators);
            Assert.DoesNotContain("separador", vm.SafetySummary);
        }

        [Fact]
        public void The_levels_of_the_desviador_read_as_runs_and_an_intermediate_post_is_named_after_its_front()
        {
            Assert.Equal("N1–N3", SelectivePostSafetyItem.Levels(new[] { 0, 1, 2 }));
            Assert.Equal("N1, N3–N4", SelectivePostSafetyItem.Levels(new[] { 3, 0, 2 }));

            var design = Structure();
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true });
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            var catalog = UiSupport.LoadCatalogSafe();
            var desviador = Variant(catalog, SelectiveSafetyDefaults.DesviadorType);
            desviador.Side = SafetySide.Left;
            design.SafetySelections.Add(desviador);
            var vm = SelectiveEditor(design);

            Assert.Equal(new[] { "P1", "F1 · intermedio 1", "P2", "P3" }, vm.PostSafety.Select(r => r.Post));
            Assert.Equal("N1–N3 (3)", vm.PostSafety[1].Deflectors);
        }

        [Fact]
        public void The_editor_of_the_window_writes_T_on_the_selection_and_takes_the_variant_from_Editar_seguridad()
        {
            var vm = SelectiveEditor(Structure());
            var tope = SelectiveSafetyFamilies.VariantsOfType(vm.Draft.Catalog.SafetyElements, SelectiveSafetyDefaults.TopeType).First().Id;
            Assert.False(vm.CanEditSafety); // no modal port: the button is not offered
            vm.ClickCell(0, 0, control: false, shift: false);
            Assert.Null(vm.SelectionTope);
            Assert.NotNull(vm.SetTope(false)); // no tope chosen yet: refused with the reason

            vm.SafetyEditor = draft => new[] { new SelectiveSafetySelection { ElementId = tope, Quantity = 1 } };
            Assert.True(vm.CanEditSafety);
            vm.EditSafety();
            Assert.True(vm.SelectionTope);

            Assert.Null(vm.SetTope(false));
            Assert.False(vm.SelectionTope);
            Assert.False(SelectiveSafetyView.Tope(vm.Draft, 0, 0));
            Assert.True(SelectiveSafetyView.Tope(vm.Draft, 0, 1));
            Assert.Equal(3, vm.PostSafety.Count);
            Assert.Equal("P1", vm.PostSafety[0].Post);

            Assert.Null(vm.SetTopeEverywhere(false));
            Assert.True(SelectiveSafetyView.AllOff(vm.Draft, SelectiveSafetyDefaults.TopeType));
            Assert.Null(vm.SetTopeEverywhere(true));
            Assert.True(SelectiveSafetyView.Tope(vm.Draft, 0, 0));
        }
    }
}
