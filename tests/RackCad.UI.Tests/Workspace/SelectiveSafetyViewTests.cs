#nullable enable
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// «Extremos y seguridad» of a Selectivo (entrega 2, part C): T/P per front × level for the whole rack on the grid of beam levels, and the
    /// table by post counted with the plan the BOM uses. Generic sample racks of <see cref="SelectiveWindowFixtures"/>, no data of a client.
    /// </summary>
    public sealed class SelectiveSafetyViewTests
    {
        private static SelectiveDraftEditor Editor(params string[] types)
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var editor = new SelectiveDraftEditor(catalog);
            Assert.True(editor.LoadFrom(Structure(), null!, "SEL"), editor.LastError);
            editor.ReplaceSafety(types.Select(type => new SelectiveSafetySelection
            {
                ElementId = SelectiveSafetyFamilies.VariantsOfType(catalog.SafetyElements, type).First().Id,
                Quantity = 1,
            }));
            Assert.True(editor.Recompose(), editor.LastError);
            return editor;
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
            Assert.True(SelectiveSafetyView.Tope(editor, 0, 0));
            Assert.Null(SelectiveSafetyView.Tope(editor, 1, 0)); // F2 N1 rests on the floor: no larguero, no tope (decision 4)
            Assert.True(SelectiveSafetyView.Tope(editor, 1, 1));
            Assert.Null(SelectiveSafetyView.Parrilla(editor, 0, 0)); // no parrilla chosen
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

            var edit = SelectiveSafetyView.WriteTope(editor, new[] { P(0, 0), P(0, 2), P(1, 0) }, on: false);
            Assert.False(edit.Refused, edit.Error);
            Assert.Equal(2, edit.Written);
            Assert.Equal(1, edit.Omitted); // F2 N1 is not a cell
            Assert.True(editor.Recompose(), editor.LastError);

            Assert.False(SelectiveSafetyView.Tope(editor, 0, 0));
            Assert.True(SelectiveSafetyView.Tope(editor, 0, 1));
            Assert.False(SelectiveSafetyView.Tope(editor, 0, 2));
            Assert.True(SelectiveSafetyView.ReadTope(editor, new[] { P(0, 0), P(0, 1) }).Varies);
            Assert.Equal(OnCells() * Spots(), Planned());

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
            Assert.True(SelectiveSafetyView.WriteParrilla(editor, new[] { P(1, 0) }, on: false).Refused); // only a cell without larguero
        }

        [Fact]
        public void The_table_by_post_adds_up_to_the_desviadores_the_BOM_counts()
        {
            var editor = Editor(SelectiveSafetyDefaults.DesviadorType, SelectiveSafetyDefaults.BotaType);

            var rows = SelectiveSafetyView.PostRows(editor);
            Assert.Equal(3, rows.Count);
            var bom = SelectiveBomBuilder.Build(editor.System!, editor.Catalog);
            var desviadorId = SelectiveSafetyFamilies.VariantsOfType(editor.Catalog.SafetyElements, SelectiveSafetyDefaults.DesviadorType).First().Id;
            var counted = bom.Components.Where(c => c.ProfileId == desviadorId).Sum(c => c.Quantity);
            Assert.True(counted > 0);
            Assert.Equal(counted, rows.Sum(r => r.Deflectors));
            Assert.Contains(rows, r => r.BootSide != SelectiveSafetyView.NoSide);
            Assert.All(rows, r => Assert.Contains(r.BootSide, new[] { SelectiveSafetyView.Left, SelectiveSafetyView.Right, SelectiveSafetyView.BothSides, SelectiveSafetyView.NoSide }));
            Assert.Equal(SelectiveSafetyView.NoSide, SelectiveSafetyView.BootSide(null!, 0));
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
        }
    }
}
