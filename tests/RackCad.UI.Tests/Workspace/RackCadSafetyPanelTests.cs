#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T4 seg (I-64, cuarta tanda; the prototype «seguridad», SG-01 to SG-08, with the note of the Owner on SG-02): the safety of a Selectivo and
    /// of a Dinámico is edited in «Extremos y seguridad» of the RackCad window, without the classic dialog. «Seguridad por poste» is editable
    /// (the bota and the protector lateral of each post, the levels of the desviador, the defenses of a Dinámico), the inspector «Variante y
    /// medidas» carries the variant and the measures of the six grids of the classic dialog, and each system keeps its own words for the sides.
    /// Generic sample racks, no data of a client.
    /// </summary>
    public sealed class RackCadSafetyPanelTests
    {
        private static RackCatalog Catalog => UiSupport.LoadCatalogSafe();

        private static string Id(string type) => SelectiveSafetyFamilies.VariantsOfType(Catalog.SafetyElements, type).First().Id;

        private static string Bota => SelectiveSafetyDefaults.BotaType;
        private static string Lateral => SelectiveSafetyDefaults.LateralType;
        private static string Desviador => SelectiveSafetyDefaults.DesviadorType;
        private static string Tope => SelectiveSafetyDefaults.TopeType;
        private static string Parrilla => SelectiveSafetyDefaults.ParrillaType;
        private static string Defensa => SelectiveSafetyDefaults.DefensaType;
        private static string Guia => SelectiveSafetyDefaults.GuiaType;

        private static SelectiveSafetySelection Variant(string type) => new SelectiveSafetySelection { ElementId = Id(type), Quantity = 1 };

        private static SelectivePalletDesign With(SelectivePalletDesign design, params string[] types)
        {
            foreach (var type in types) design.SafetySelections.Add(Variant(type));
            return design;
        }

        // A protector lateral as the classic dialog writes it: no general side, the two posts of the ends (Side defaults to «Ambas»).
        private static SelectiveSafetySelection EndsLateral(int posts)
        {
            var lateral = Variant(Lateral);
            lateral.Side = SafetySide.None;
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = posts - 1, Side = SafetySide.Right });
            return lateral;
        }

        private static SelectivePalletDesign With(SelectivePalletDesign design, SelectiveSafetySelection selection)
        {
            design.SafetySelections.Add(selection);
            return design;
        }

        // The safety tab of a Selectivo is shown: what it reads from the BOM is counted (P1-04), and the panel follows the draft.
        private static RackCadSafetyPanelViewModel Shown(SelectiveEditorViewModel vm)
        {
            vm.SelectedTab = SelectiveEditorViewModel.SafetyTabIndex;
            return vm.SafetyPanel;
        }

        private static RackCadSafetyRowItem Row(RackCadSafetyPanelViewModel panel, string post) => panel.Table.Single(r => r.Post == post);

        private static RackCadSafetyInspector Inspect(RackCadSafetyPanelViewModel panel, string type)
        {
            panel.Pick(type);
            return panel.Inspector;
        }

        private static DynamicFieldItem Number(RackCadSafetyInspector inspector, string key) => inspector.Fields.OfType<DynamicFieldItem>().Single(f => f.Key == key);

        private static SelectiveChoiceItem List(RackCadSafetyInspector inspector, string key) => inspector.Fields.OfType<SelectiveChoiceItem>().Single(f => f.Key == key);

        private static RackCadSafetyCheckField Box(RackCadSafetyInspector inspector, string key) => inspector.Fields.OfType<RackCadSafetyCheckField>().Single(f => f.Key == key);

        private static RackCadSafetyAction Action(RackCadSafetyInspector inspector, string key) => inspector.Actions.Single(a => a.Key == key);

        private static SelectiveSafetySelection? Selected(SelectiveEditorViewModel vm, string type) => SelectiveSafetyView.Selected(vm.Draft, type);

        // What the BOM of the last valid design of a Selectivo counts of a family, on a system of its own.
        private static int Bom(SelectiveEditorViewModel vm, string type)
            => SelectiveBomBuilder.Build(vm.Draft.ResolveCopy()!, vm.Draft.Catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Safety)
                .Where(c => vm.Draft.Catalog.SafetyElements.Any(e => string.Equals(e.Id, c.ProfileId, StringComparison.OrdinalIgnoreCase) && SelectiveSafetyDefaults.IsType(e.Type, type)))
                .Sum(c => c.Quantity);

        // ================================================================== Selectivo

        [Fact]
        public void Selectivo_choosing_a_variant_brings_the_defaults_of_the_classic_dialog_and_the_table_adds_up_to_the_BOM()
        {
            var vm = SelectiveEditor(Structure());
            var panel = Shown(vm);
            Assert.Equal(new[] { "P1", "P2", "P3", "Total" }, panel.Table.Select(r => r.Post));
            Assert.All(panel.Table, r => Assert.Equal("—", r.Boots.Text));
            Assert.False(Inspect(panel, Bota).HasVariant);
            Assert.Equal("Sin variante elegida: no se dibuja ni entra en el BOM. Elige una para ponerla con sus valores por defecto.", panel.Inspector.NoVariantText);

            // The bota: both sides of every post (in the classic it started in «Ninguno», and without changing it nothing was kept).
            Inspect(panel, Bota).Variant.SelectedValue = Id(Bota);
            Assert.True(vm.IsDirty);
            Assert.StartsWith("Botas: «", vm.Message);
            Assert.EndsWith("», con sus valores por defecto. Revísalos aquí y en la tabla.", vm.Message);
            var bota = Selected(vm, Bota)!;
            Assert.Equal(SafetySide.Both, bota.Side);
            Assert.Equal(BootPlacement.Both, bota.Bota.Placement);
            Assert.True(Bom(vm, Bota) > 0);
            Assert.Equal(Bom(vm, Bota).ToString(), panel.Table.Last().Boots.Text);
            Assert.All(panel.Table.Take(3), r => Assert.Equal("g", r.Boots.Choice!.SelectedValue));
            Assert.Equal("General (Izquierda y Derecha)", Row(panel, "P1").Boots.Choice!.Options[0].Label);

            // The protector lateral of a Selectivo: the two posts of the ends, P1 «Izquierda» and P3 «Derecha»; it replaces their botas.
            Inspect(panel, Lateral).Variant.SelectedValue = Id(Lateral);
            Assert.Equal(new[] { (0, SafetySide.Left), (2, SafetySide.Right) }, Selected(vm, Lateral)!.PostSides.Select(p => (p.PostIndex, p.Side)));
            Assert.Equal("La reemplaza el protector lateral", Row(panel, "P1").Boots.Text);
            Assert.True(Row(panel, "P2").Boots.HasChoice);
            Assert.Equal(new[] { "L", "N", "R" }, new[] { "P1", "P2", "P3" }.Select(p => Row(panel, p).SideGuard.Choice!.SelectedValue));
            Assert.Equal(Bom(vm, Lateral).ToString(), panel.Table.Last().SideGuard.Text);
            Assert.Equal(Bom(vm, Bota).ToString(), panel.Table.Last().Boots.Text);
            Assert.Equal("En el dibujo: 2 protectores, en P1, P3.", Inspect(panel, Lateral).Pieces);

            // The desviador: both faces, 18 in, the first level 18 in above the first troquel, in every level.
            Inspect(panel, Desviador).Variant.SelectedValue = Id(Desviador);
            var desviador = Selected(vm, Desviador)!;
            Assert.Equal((SafetySide.Both, 18.0, 18.0), (desviador.Side, desviador.DesviadorLongitud, desviador.DesviadorPrimerNivelAltura));
            Assert.Empty(desviador.DesviadorOffCells);
            Assert.All(panel.Table.Take(3), r => Assert.All(r.Deflectors.Levels, l => Assert.True(l.IsOn)));
            Assert.Equal(Bom(vm, Desviador).ToString(), panel.Table.Last().Deflectors.Text);
            Assert.Equal("18", Number(Inspect(panel, Desviador), "length").Text);

            // The tope and the parrilla, with the configuration of the classic grids accepted unchanged.
            Inspect(panel, Tope).Variant.SelectedValue = Id(Tope);
            var tope = Selected(vm, Tope)!;
            Assert.Equal((SafetySide.Both, true, 3.0, false, -1), (tope.Side, tope.TopeShared, tope.TopeSaque, tope.TopeFrontal, tope.TopeFondo));
            Inspect(panel, Parrilla).Variant.SelectedValue = Id(Parrilla);
            var parrilla = Selected(vm, Parrilla)!;
            Assert.Equal((SafetySide.Both, true, true, 0.0, 0), (parrilla.Side, parrilla.ParrillaFrontal, parrilla.ParrillaLateral, parrilla.ParrillaFrente, parrilla.ParrillaCantidad));
            var decks = SelectiveBomBuilder.Build(vm.Draft.ResolveCopy()!, vm.Draft.Catalog).Components.Where(c => c.Category == SelectiveBomBuilder.Parrilla).Sum(c => c.Quantity);
            Assert.True(decks > 0);
            Assert.Equal("En el dibujo: " + decks + " parrillas, en 4 de 4 celdas con larguero.", Inspect(panel, Parrilla).Pieces);

            // «Descartar» goes back to the drawing, which had no safety.
            vm.Discard();
            Assert.False(vm.IsDirty);
            Assert.All(panel.Table, r => Assert.Equal("—", r.Boots.Text));
            Assert.False(Inspect(panel, Bota).HasVariant);
        }

        [Fact]
        public void Selectivo_what_the_window_writes_is_what_the_classic_dialog_accepts_unchanged()
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor(TwoFondos());
                var panel = Shown(vm);

                // Every family in the order of the catalog (the order the classic «Aceptar» writes them), then edits of each.
                var order = Catalog.SafetyElements.Select(e => e.Type).Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(t => new[] { Bota, Lateral, Tope, Parrilla, Desviador }.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList();
                Assert.Equal(5, order.Count);
                foreach (var type in order) Inspect(panel, type).Variant.SelectedValue = Id(type);

                Row(panel, "P2").Boots.Choice!.SelectedValue = "L";
                List(Inspect(panel, Bota), "side").SelectedValue = "R";
                Row(panel, "P3").SideGuard.Choice!.SelectedValue = "B";
                Row(panel, "P1").Deflectors.Levels[1].Toggle();
                Write(Number(Inspect(panel, Desviador), "length"), "24");
                List(panel.Inspector, "side").SelectedValue = "L";
                Write(Number(Inspect(panel, Tope), "saque"), "4");
                List(panel.Inspector, "shared").SelectedValue = "f";
                Box(panel.Inspector, "frontal").IsChecked = true;
                Box(Inspect(panel, Parrilla), "lateral").IsChecked = false;
                Assert.False(vm.MessageIsError, vm.Message);

                var bota = Selected(vm, Bota)!;
                Assert.Equal((SafetySide.Right, BootPlacement.Rear), (bota.Side, bota.Bota.Placement!.Value));
                Assert.Equal(new[] { (1, SafetySide.Left) }, bota.PostSides.Select(p => (p.PostIndex, p.Side)));
                Assert.Equal(new[] { (1, BootPlacement.EntryExit) }, bota.Bota.Posts.Select(p => (p.PostIndex, p.Placement)));

                // The dialog of the classic editor over the safety of the draft, accepted unchanged, gives the same design.
                var dialog = RackCadSelectiveSafetyDialog.Build(vm.Draft);
                try
                {
                    dialog.BuildResultForTest();
                    var copy = Store.Deserialize(Json(vm.Draft.Design))!.ToDomain();
                    copy.SafetySelections.Clear();
                    foreach (var selection in dialog.Result) copy.SafetySelections.Add(selection.DeepCopy());
                    Assert.Equal(Json(copy), Json(vm.Draft.Design));
                }
                finally
                {
                    dialog.Close();
                }
            });
        }

        // P1-04: what reads the BOM is counted only while the tab is shown; a change made in another tab reaches the table when it is shown.
        [Fact]
        public void Selectivo_the_table_follows_a_change_made_in_another_tab_when_the_tab_is_shown()
        {
            var vm = SelectiveEditor(With(Structure(), Bota));
            var panel = vm.SafetyPanel; // built while «Matriz» is shown
            Assert.Equal(new[] { "P1", "P2", "P3", "Total" }, panel.Table.Select(r => r.Post));

            Write(Field(vm.Essentials, "fronts"), "3");
            Assert.Equal(new[] { "P1", "P2", "P3", "Total" }, panel.Table.Select(r => r.Post)); // not counted while hidden

            Shown(vm);
            Assert.Equal(new[] { "P1", "P2", "P3", "P4", "Total" }, panel.Table.Select(r => r.Post));
            Assert.Equal(Bom(vm, Bota).ToString(), panel.Table.Last().Boots.Text);
        }

        [Fact]
        public void Selectivo_a_post_takes_a_bota_of_its_own_and_the_general_one_again()
        {
            var vm = SelectiveEditor(With(Structure(), Bota));
            var panel = Shown(vm);
            var before = Bom(vm, Bota);
            Assert.Equal("2 piezas", Row(panel, "P2").Boots.Sub);

            Row(panel, "P2").Boots.Choice!.SelectedValue = "L";
            Assert.True(vm.IsDirty);
            Assert.Equal("Propia · 1 pieza", Row(panel, "P2").Boots.Sub);
            Assert.True(Row(panel, "P2").Boots.IsOwn);
            Assert.Equal(before - 1, Bom(vm, Bota));
            Assert.Equal((before - 1).ToString(), panel.Table.Last().Boots.Text);
            Assert.True(Action(Inspect(panel, Bota), "general").IsEnabled);
            Assert.Contains("1 poste con la suya", panel.Inspector.AskLoss);

            // The general side does not move a post with its own.
            List(panel.Inspector, "side").SelectedValue = "R";
            Assert.Equal("L", Row(panel, "P2").Boots.Choice!.SelectedValue);
            Assert.Equal("General (Derecha)", Row(panel, "P1").Boots.Choice!.Options[0].Label);

            // «General» again, and the general side back: the drawing again.
            Row(panel, "P2").Boots.Choice!.SelectedValue = "g";
            List(Inspect(panel, Bota), "side").SelectedValue = "B";
            Assert.False(vm.IsDirty);
            Assert.False(Action(panel.Inspector, "general").IsEnabled);
        }

        [Fact]
        public void Selectivo_a_protector_lateral_in_no_post_keeps_its_variant_and_Solo_en_los_extremos_brings_the_ends_back()
        {
            var vm = SelectiveEditor(Structure());
            var panel = Shown(vm);
            Inspect(panel, Lateral).Variant.SelectedValue = Id(Lateral);
            Assert.False(Action(panel.Inspector, "ends").IsEnabled);

            foreach (var post in new[] { "P1", "P3" }) Row(panel, post).SideGuard.Choice!.SelectedValue = "N";
            Assert.NotNull(Selected(vm, Lateral));
            Assert.Equal(0, Bom(vm, Lateral));
            Assert.Equal("Elegido, en ningún poste: no se dibuja.", Inspect(panel, Lateral).Pieces);
            Assert.True(Action(panel.Inspector, "ends").IsEnabled);

            // «Ambos» on an inner post: a bridge, guide on both sides.
            Row(panel, "P2").SideGuard.Choice!.SelectedValue = "B";
            Assert.Equal("Ambos (frente-puente)", Row(panel, "P2").SideGuard.Choice!.Options.Single(o => o.Value == "B").Label);
            Assert.Equal(2, Bom(vm, Lateral)); // a bridge: the guide on both sides

            Action(Inspect(panel, Lateral), "ends").Run();
            Assert.Equal(new[] { (0, SafetySide.Left), (2, SafetySide.Right) }, Selected(vm, Lateral)!.PostSides.Select(p => (p.PostIndex, p.Side)));
            Assert.Equal(2, Bom(vm, Lateral));
        }

        [Fact]
        public void Selectivo_the_levels_of_the_desviador_are_buttons_and_an_intermediate_post_keeps_its_row_with_every_level_off()
        {
            var design = Structure();
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true });
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            var desviador = Variant(Desviador);
            desviador.Side = SafetySide.Left;
            design.SafetySelections.Add(desviador);
            var vm = SelectiveEditor(design);
            var panel = Shown(vm);

            // SG-08: the intermediate post of the medio frente has its own row, with its levels and no bota nor protector.
            Assert.Equal(new[] { "P1", "F1 · intermedio 1", "P2", "P3", "Total" }, panel.Table.Select(r => r.Post));
            var inner = Row(panel, "F1 · intermedio 1");
            Assert.Equal("de medio frente", inner.Kind);
            Assert.Equal(new[] { "N1", "N2", "N3" }, inner.Deflectors.Levels.Select(l => l.Text));
            Assert.Equal("—", inner.Boots.Text);
            Assert.Equal("F1 · intermedio 1 · N2: con desviador. Pulsa para quitarlo.", inner.Deflectors.Levels[1].Name);

            var total = Bom(vm, Desviador);
            foreach (var level in inner.Deflectors.Levels.ToList()) level.Toggle();
            Assert.Equal(total - 3, Bom(vm, Desviador));
            inner = Row(panel, "F1 · intermedio 1"); // the summary of the BOM no longer lists it; the table does
            Assert.All(inner.Deflectors.Levels, l => Assert.False(l.IsOn));
            Assert.Equal("0 piezas", inner.Deflectors.Sub);
            Assert.Equal("En el dibujo: " + (total - 3) + " desviadores; 3 niveles apagados.", Inspect(panel, Desviador).Pieces);

            inner.Deflectors.Levels[0].Toggle();
            Assert.Equal(total - 2, Bom(vm, Desviador));
            Assert.Equal(panel.Table.Last().Deflectors.Text, (total - 2).ToString());

            // «En ninguno» keeps the variant (as C2-07), «En todos los niveles» brings every level back.
            Action(Inspect(panel, Desviador), "none").Run();
            Assert.Equal(0, Bom(vm, Desviador));
            Assert.NotNull(Selected(vm, Desviador));
            Action(panel.Inspector, "all").Run();
            Assert.Equal(total, Bom(vm, Desviador));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void Selectivo_a_measure_that_is_not_valid_is_refused_in_its_field_and_writes_nothing()
        {
            var vm = SelectiveEditor(With(Structure(), Desviador, Tope, Parrilla));
            var panel = Shown(vm);
            var json = Json(vm.Draft.Design);

            var length = Number(Inspect(panel, Desviador), "length");
            Write(length, "17");
            Assert.Equal("La longitud debe ser un entero par mayor de 8.", length.Error);
            Assert.Equal("17", length.Text);
            var first = Number(panel.Inspector, "first");
            Write(first, "8");
            Assert.Equal("El primer nivel debe ser un entero par mayor de 8.", first.Error);

            var saque = Number(Inspect(panel, Tope), "saque");
            Write(saque, "0");
            Assert.Equal("El saque debe ser un número mayor que cero.", saque.Error);

            var count = Number(Inspect(panel, Parrilla), "count");
            Write(count, "9");
            Assert.StartsWith("No caben 9 por nivel en F1 · larguero 1 (caben ", count.Error);
            Assert.EndsWith(". Baja la cantidad o el ancho, o quita esas celdas.", count.Error);
            var width = Number(panel.Inspector, "width");
            Write(width, "500");
            Assert.Equal("No cabe ninguna parrilla de 500 in en las celdas con parrilla. Bájalo o déjalo vacío.", width.Error);
            Write(width, "abc");
            Assert.Equal("Vacío (el ancho de la tarima) o un número mayor que cero.", width.Error);

            Assert.Equal(json, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);

            // A valid one goes to the draft.
            Write(Number(Inspect(panel, Desviador), "length"), "24");
            Assert.Equal(24.0, Selected(vm, Desviador)!.DesviadorLongitud);
            Assert.Equal(string.Empty, Number(panel.Inspector, "length").Error);
            Assert.True(vm.IsDirty);
        }

        [Fact]
        public void Selectivo_without_a_variant_asks_first_what_is_lost_and_Descartar_brings_it_back()
        {
            var vm = SelectiveEditor(With(Structure(), Bota));
            var panel = Shown(vm);
            Row(panel, "P2").Boots.Choice!.SelectedValue = "L";
            var inspector = Inspect(panel, Bota);
            var id = Id(Bota);

            inspector.Variant.SelectedValue = RackCadSafetyPanelViewModel.NoVariant;
            Assert.True(inspector.IsAsking);
            Assert.Equal("¿Quitar botas?", inspector.AskTitle);
            Assert.Equal("Quitar botas", inspector.AskYesLabel);
            Assert.Equal("Variante «" + Catalog.SafetyElements.First(e => e.Id == id).Label + "»", inspector.AskLoss[0]);
            Assert.Contains("Lado general «Izquierda y Derecha»", inspector.AskLoss);
            Assert.Contains("1 poste con la suya", inspector.AskLoss);
            Assert.Equal(id, inspector.Variant.SelectedValue); // the list shows the variant while it asks
            Assert.NotNull(Selected(vm, Bota));

            inspector.CancelRemove();
            Assert.False(inspector.IsAsking);
            Assert.NotNull(Selected(vm, Bota));

            inspector.Variant.SelectedValue = RackCadSafetyPanelViewModel.NoVariant;
            inspector.ConfirmRemove();
            Assert.False(inspector.IsAsking);
            Assert.Null(Selected(vm, Bota));
            Assert.Equal("Se quitó «Botas» del borrador, con sus medidas. «Descartar» lo recupera.", vm.Message);
            Assert.All(panel.Table, r => Assert.Equal("—", r.Boots.Text));

            vm.Discard();
            Assert.NotNull(Selected(vm, Bota));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void Selectivo_the_side_of_a_central_tope_shared_between_fondos_does_not_count_and_Ninguno_is_only_offered_when_read()
        {
            var vm = SelectiveEditor(With(TwoFondos(), Tope));
            var panel = Shown(vm);
            var inspector = Inspect(panel, Tope);
            var side = List(inspector, "side");
            Assert.False(side.IsEnabled);
            Assert.Equal("Con uno central, compartido, el lado no cuenta.", side.Hint);
            Assert.Equal(new[] { "B", "L", "R" }, side.Options.Select(o => o.Value));
            Assert.Equal(new[] { "-1", "0", "1" }, List(inspector, "fondo").Options.Select(o => o.Value));

            List(inspector, "shared").SelectedValue = "f";
            side = List(panel.Inspector, "side");
            Assert.True(side.IsEnabled);
            Assert.Equal("Izquierda = el extremo bajo del hueco entre fondos; Derecha = el alto.", side.Hint);
            Assert.False(Selected(vm, Tope)!.TopeShared);

            // A rack read with «Ninguno»: the option is there, so the list can show it.
            var none = Variant(Tope);
            none.Side = SafetySide.None;
            var read = SelectiveEditor(With(Structure(), none));
            var options = List(Inspect(Shown(read), Tope), "side").Options;
            Assert.Equal("Ninguno (no se dibuja)", options.Single(o => o.Value == "N").Label);
            Assert.Null(read.SafetyPanel.Inspector.Fields.OfType<SelectiveChoiceItem>().SingleOrDefault(f => f.Key == "shared")); // one fondo: no «Cuántos»
        }

        [Fact]
        public void Selectivo_while_AutoCAD_writes_nothing_of_the_safety_can_be_edited()
        {
            var vm = SelectiveEditor(With(Structure(), Bota, Desviador));
            var panel = Shown(vm);
            Inspect(panel, Desviador);
            vm.BeginWriting();
            Assert.False(Row(panel, "P1").Boots.Choice!.IsEnabled);
            Assert.All(Row(panel, "P1").Deflectors.Levels, l => Assert.False(l.IsEnabled));
            Assert.False(panel.Inspector.Variant.IsEnabled);
            Assert.True(Number(panel.Inspector, "length").IsReadOnly);
            Assert.All(panel.Inspector.Actions, a => Assert.False(a.IsEnabled));

            vm.EndWriting();
            Assert.True(Row(panel, "P1").Boots.Choice!.IsEnabled);
            Assert.All(Row(panel, "P1").Deflectors.Levels, l => Assert.True(l.IsEnabled));
            Assert.True(panel.Inspector.Variant.IsEnabled);
        }

        [Fact]
        public void Selectivo_keeps_its_words_for_the_sides()
        {
            // SG-02, the note of the Owner: «Izquierda / Derecha» in Selectivo.
            var vm = SelectiveEditor(With(With(TwoFondos(), Bota, Desviador), EndsLateral(4)));
            var panel = Shown(vm);
            Assert.Equal(new[] { "General (Izquierda y Derecha)", "Ninguna", "Izquierda", "Derecha", "Izquierda y Derecha" },
                Row(panel, "P2").Boots.Choice!.Options.Select(o => o.Label));
            Assert.Equal(new[] { "No lleva", "Izquierda", "Derecha", "Ambos (frente-puente)" }, Row(panel, "P2").SideGuard.Choice!.Options.Select(o => o.Label));
            Assert.Equal(new[] { "Izquierda y Derecha", "Izquierda (cara del fondo 1)", "Derecha (cara del fondo 2)" },
                List(Inspect(panel, Desviador), "side").Options.Select(o => o.Label));
            Assert.Contains("con «Izquierda y Derecha», un desviador cuenta dos piezas", panel.PostFoot);
            Assert.DoesNotContain(panel.Inspector.Notes, n => n.Contains("Salida"));
        }

        // ================================================================== Dinámico

        // Two fronts (3 and 2 levels) with the default safety of a new rack: botas, protector lateral by the rule of the ends, desviadores,
        // defenses and the guide (as DynamicSafetyTabTests).
        private static DynamicRackDesign DynamicStructure()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0,
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 3 });
            foreach (var selection in DynamicSafetyDefaults.Build(UiSupport.LoadCatalogSafe())) design.SafetySelections.Add(selection.DeepCopy());
            return design;
        }

        private static DynamicEditorViewModel DynamicEditor()
        {
            var store = new RackProjectStore();
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(DynamicStructure(), "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            var json = store.Serialize(RackProject.ForDynamic(draft.Design));
            var opened = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            opened.LoadFrom(store.Deserialize(json)!.DynamicDesign, "DIN 1");
            return new DynamicEditorViewModel(opened, "DIN-1", _ => { });
        }

        private static SelectiveSafetySelection? Selected(DynamicEditorViewModel vm, string type)
            => SelectiveSafetyFamilies.SelectedOfType(vm.Draft.SafetySelections, vm.Draft.Catalog.SafetyElements, type);

        private static int Bom(DynamicEditorViewModel vm, string type)
            => SystemBomBuilder.Build(vm.Draft.System, vm.Draft.Catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Safety)
                .Where(c => vm.Draft.Catalog.SafetyElements.Any(e => string.Equals(e.Id, c.ProfileId, StringComparison.OrdinalIgnoreCase) && SelectiveSafetyDefaults.IsType(e.Type, type)))
                .Sum(c => c.Quantity);

        [Fact]
        public void Dinamico_keeps_its_words_for_the_sides_and_the_table_adds_up_to_the_BOM()
        {
            // SG-02, the note of the Owner: «salida / entrada» in Dinámico (its classic: «Izquierda es la salida y derecha la entrada»).
            var vm = DynamicEditor();
            var panel = vm.SafetyPanel;
            Assert.True(panel.IsDynamic);
            Assert.Equal(new[] { "P1", "P2", "P3", "Total" }, panel.Table.Select(r => r.Post));
            Assert.Equal(new[] { "General (Salida y entrada)", "Ninguna", "Salida", "Entrada", "Salida y entrada" }, Row(panel, "P2").Boots.Choice!.Options.Select(o => o.Label));
            Assert.Equal(new[] { "No lleva", "Salida", "Entrada", "Salida y entrada" }, Row(panel, "P1").SideGuard.Choice!.Options.Select(o => o.Label));
            Assert.Equal(new[] { "Salida y entrada", "Salida", "Entrada" }, List(Inspect(panel, Desviador), "side").Options.Select(o => o.Label));
            Assert.Contains("Salida = extremo bajo; entrada = extremo alto.", Inspect(panel, Bota).Notes);
            Assert.DoesNotContain(panel.Table.SelectMany(r => r.Boots.Choice?.Options ?? Array.Empty<SelectiveChoiceOption>()), o => o.Label.Contains("Izquierda"));

            var total = panel.Table.Last();
            Assert.Equal(Bom(vm, Bota).ToString(), total.Boots.Text);
            Assert.Equal(Bom(vm, Lateral).ToString(), total.SideGuard.Text);
            Assert.Equal(Bom(vm, Desviador).ToString(), total.Deflectors.Text);
            Assert.Equal(Bom(vm, Defensa), int.Parse(total.DefenseExit.Text) + int.Parse(total.DefenseEntrance.Text));
            Assert.Equal(Bom(vm, Guia).ToString(), total.Guides);
        }

        [Fact]
        public void Dinamico_the_protector_lateral_leaves_the_rule_of_the_ends_with_the_ends_written_and_comes_back_to_it()
        {
            var vm = DynamicEditor();
            var panel = vm.SafetyPanel;
            Assert.Equal("Regla de los extremos", Row(panel, "P1").SideGuard.Sub);
            Assert.Equal(new[] { "L", "N", "R" }, new[] { "P1", "P2", "P3" }.Select(p => Row(panel, p).SideGuard.Choice!.SelectedValue));
            var how = Inspect(panel, Lateral).Fields.OfType<RackCadSafetyTextField>().Single();
            Assert.StartsWith("Regla de los extremos: en P1 (salida) y en P3 (entrada).", how.Text);
            Assert.False(Action(panel.Inspector, "rule").IsEnabled);

            // SG-04: a post changed → «Elegido por poste», with the two ends written, and it says so.
            Row(panel, "P2").SideGuard.Choice!.SelectedValue = "L";
            var lateral = Selected(vm, Lateral)!;
            Assert.Equal(new[] { (0, SafetySide.Left), (1, SafetySide.Left), (2, SafetySide.Right) }, lateral.PostSides.OrderBy(p => p.PostIndex).Select(p => (p.PostIndex, p.Side)));
            Assert.StartsWith("El protector lateral va ahora por poste: ya no se recalcula al añadir o quitar frentes.", vm.Message);
            Assert.Equal(3, Bom(vm, Lateral));
            Assert.Equal("Elegido por poste en la tabla. Ya no se recalcula al añadir o quitar frentes.", Inspect(panel, Lateral).Fields.OfType<RackCadSafetyTextField>().Single().Text);

            // Every post emptied stays «chosen by post» (the rule does not come back by itself).
            foreach (var post in new[] { "P1", "P2", "P3" }) Row(panel, post).SideGuard.Choice!.SelectedValue = "N";
            Assert.Equal(0, Bom(vm, Lateral));
            Assert.False(RackCadSafetyEdit.LateralFollowsRule(Selected(vm, Lateral), RackCadSafetySystem.Dynamic));

            Action(Inspect(panel, Lateral), "rule").Run();
            Assert.True(RackCadSafetyEdit.LateralFollowsRule(Selected(vm, Lateral), RackCadSafetySystem.Dynamic));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void Dinamico_an_empty_defense_is_the_automatic_one_and_a_written_one_is_kept_even_when_it_equals_the_automatic()
        {
            var vm = DynamicEditor();
            var panel = vm.SafetyPanel;
            var exit = Row(panel, "P1").DefenseExit;
            Assert.True(exit.HasDefense && exit.IsOn);
            Assert.Equal(string.Empty, exit.Length.Text);
            Assert.Equal("Automática · 12 in", exit.Sub);
            Assert.Equal("Automática · 36 in", Row(panel, "P2").DefenseExit.Sub);
            Assert.Equal("Longitud de la defensa en salida en P1, en pulgadas; vacía = automática, 12 in", exit.Length.Label);

            // SG-03: 12 in written on the edge post P3 at the exit is its own, although it equals the automatic one.
            Write(Row(panel, "P3").DefenseExit.Length, "12");
            var record = Selected(vm, Defensa)!.DefensaPosts.Single(p => p.PostIndex == 2);
            Assert.Equal((false, 12.0, true), (record.ExitAuto, record.ExitLength, record.EntranceAuto));
            Assert.Equal("Propia", Row(panel, "P3").DefenseExit.Sub);
            Assert.True(Row(panel, "P3").DefenseExit.IsOwn);

            // A third front: P3 becomes an intermediate post; its own 12 in stays, its automatic entrance becomes 36 in.
            Write(vm.Essentials.Single(f => f.Key == "fronts"), "3");
            Assert.Equal(new[] { "P1", "P2", "P3", "P4", "Total" }, panel.Table.Select(r => r.Post));
            var setting = DynamicForkliftDefensePlan.ForSelection(Selected(vm, Defensa), 2, 4);
            Assert.Equal((12.0, 36.0), (setting.ExitLength, setting.EntranceLength));
            Assert.Equal("Automática · 36 in", Row(panel, "P3").DefenseEntrance.Sub);
            Assert.Equal("12", Row(panel, "P3").DefenseExit.Length.Text);

            // The box: off is no defense at that end; on again is the automatic one; emptied, the length is automatic again.
            Row(panel, "P1").DefenseEntrance.IsOn = false;
            Assert.Equal("Sin defensa", Row(panel, "P1").DefenseEntrance.Sub);
            Assert.True(Row(panel, "P1").DefenseEntrance.Length.IsReadOnly);
            Assert.False(DynamicForkliftDefensePlan.ForSelection(Selected(vm, Defensa), 0, 4).DrawsEntrance);
            Row(panel, "P1").DefenseEntrance.IsOn = true;
            Assert.Equal("Automática · 12 in", Row(panel, "P1").DefenseEntrance.Sub);
            Write(Row(panel, "P3").DefenseExit.Length, "");
            Assert.Empty(Selected(vm, Defensa)!.DefensaPosts);

            var refused = Row(panel, "P2").DefenseExit.Length;
            Write(refused, "-3");
            Assert.Equal("Debe ser un número mayor que cero, o vacía (automática).", refused.Error);

            // «Todas automáticas».
            Write(Row(panel, "P2").DefenseExit.Length, "20");
            var inspector = Inspect(panel, Defensa);
            Assert.EndsWith("; 1 extremo con medida propia o sin defensa.", inspector.Pieces);
            Action(inspector, "auto").Run();
            Assert.Empty(Selected(vm, Defensa)!.DefensaPosts);
            Assert.Equal(Bom(vm, Defensa), int.Parse(panel.Table.Last().DefenseExit.Text) + int.Parse(panel.Table.Last().DefenseEntrance.Text));
        }

        // ------------------------------------------------------------------ the desviador of a Dinámico by post (decision of the Owner, 2026-11-26)

        // The desviador pieces of each post, as the BOM counts them (the two frontal cuts).
        private static int[] DeflectorsByPost(DynamicDraftEditor draft)
            => DynamicPostSafetySummary.Build(draft.System, draft.Catalog).Select(r => r.DeflectorsAtExit + r.DeflectorsAtEntrance).ToArray();

        private static DynamicDraftEditor Draft(DynamicRackDesign design)
        {
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(design, "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            return draft;
        }

        // The draft with its desviador changed as the classic dialog would leave it (the cells by post index of its grid).
        private static DynamicDraftEditor WithDesviador(DynamicDraftEditor draft, Action<SelectiveSafetySelection> change)
        {
            var copy = draft.CopySafety().ToList();
            change(SelectiveSafetyFamilies.SelectedOfType(copy, draft.Catalog.SafetyElements, Desviador)!);
            draft.ReplaceSafety(copy);
            Assert.True(draft.Recompose(), draft.LastError);
            return draft;
        }

        [Fact]
        public void Dinamico_a_new_rack_reads_its_desviador_by_post_and_its_last_post_governs_its_own()
        {
            // Two fronts: posts P1..P3. A new Dinámico (the window and the classic RACKNUEVO) carries the marker.
            var draft = DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe());
            Assert.True(SelectiveSafetyFamilies.SelectedOfType(draft.SafetySelections, draft.Catalog.SafetyElements, Desviador)!.Desviador.CellsByPost);
            draft.Matrix.SetFrontCount(2); // a new rack has one front
            Assert.True(draft.Recompose(), draft.LastError);
            var before = DeflectorsByPost(draft);
            Assert.Equal(3, before.Length);
            Assert.True(before[2] > 0 && before[1] > 0);

            // The last post off at N1: only the last post loses its desviador there.
            WithDesviador(draft, d => d.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 2, Level = 0 }));
            var after = DeflectorsByPost(draft);
            Assert.True(after[2] < before[2]);
            Assert.Equal(before[1], after[1]);
            Assert.Equal(before[0], after[0]);
        }

        [Fact]
        public void Dinamico_an_existing_rack_without_the_marker_is_read_as_before()
        {
            // The rack of the drawing, as every document before this tanda: no marker.
            var json = DynamicJson();
            Assert.DoesNotContain("DesviadorCellsByPost", json);
            var draft = Draft(new RackProjectStore().Deserialize(json)!.DynamicDesign);
            Assert.False(SelectiveSafetyFamilies.SelectedOfType(draft.SafetySelections, draft.Catalog.SafetyElements, Desviador)!.Desviador.CellsByPost);
            var before = DeflectorsByPost(draft);

            // The column of the last post is read by nobody (the historic reading)...
            WithDesviador(draft, d => d.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 2, Level = 0 }));
            Assert.Equal(before, DeflectorsByPost(draft));

            // ...and the column of the post before it governs both.
            WithDesviador(draft, d => d.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 1, Level = 0 }));
            var after = DeflectorsByPost(draft);
            Assert.True(after[1] < before[1] && after[2] < before[2]);
            Assert.Equal(before[0], after[0]);
        }

        [Fact]
        public void Dinamico_passing_an_existing_desviador_to_the_reading_by_post_keeps_what_it_draws_and_the_marker_is_persisted()
        {
            // A rack of the drawing with N1 off in the column of P2 (P2 and P3 off) and a cell of its own on P3 that nobody read.
            var draft = Draft(new RackProjectStore().Deserialize(DynamicJson())!.DynamicDesign);
            WithDesviador(draft, d =>
            {
                d.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 1, Level = 0 });
                d.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 2, Level = 1 });
            });
            var drawn = DeflectorsByPost(draft);

            WithDesviador(draft, d => Assert.True(RackCadSafetyEdit.DesviadorToPosts(d, 3)));
            var desviador = SelectiveSafetyFamilies.SelectedOfType(draft.SafetySelections, draft.Catalog.SafetyElements, Desviador)!;
            Assert.True(desviador.Desviador.CellsByPost);
            Assert.Equal(new[] { (1, 0), (2, 0) }, desviador.DesviadorOffCells.Select(c => (c.Frente, c.Level)).OrderBy(c => c));
            Assert.Equal(drawn, DeflectorsByPost(draft)); // the same drawing
            Assert.False(RackCadSafetyEdit.DesviadorToPosts(desviador, 3)); // once

            // The marker goes to the document and comes back; a document without it reads false.
            var json = new RackProjectStore().Serialize(RackProject.ForDynamic(draft.Design));
            Assert.Contains("DesviadorCellsByPost", json);
            var read = new RackProjectStore().Deserialize(json)!.DynamicDesign;
            Assert.True(SelectiveSafetyFamilies.SelectedOfType(read.SafetySelections, draft.Catalog.SafetyElements, Desviador)!.Desviador.CellsByPost);
        }

        [Fact]
        public void Dinamico_the_table_no_longer_ties_the_last_post_to_the_one_before_it()
        {
            // An existing rack (no marker): the table shows what the drawing reads at each post, with no note.
            var vm = DynamicEditor();
            var panel = vm.SafetyPanel;
            Assert.DoesNotContain(panel.Table, r => r.Deflectors.Sub.Contains("niveles de"));
            Assert.DoesNotContain(Inspect(panel, Desviador).Notes, n => n.Contains("penúltimo"));
            var p2 = Row(panel, "P2").Deflectors.Levels.Select(l => l.IsOn).ToList();
            var before = Bom(vm, Desviador);

            // A click on P3 N1 changes P3 only: the edit passes the desviador to the reading by post, keeping the rest of the drawing.
            Row(panel, "P3").Deflectors.Levels[0].Toggle();
            Assert.False(Row(panel, "P3").Deflectors.Levels[0].IsOn);
            Assert.Equal(p2, Row(panel, "P2").Deflectors.Levels.Select(l => l.IsOn));
            Assert.True(Selected(vm, Desviador)!.Desviador.CellsByPost);
            Assert.True(Bom(vm, Desviador) < before);
            Assert.Equal(Bom(vm, Desviador).ToString(), panel.Table.Last().Deflectors.Text);

            // And P2 N1, then, changes P2 only.
            Row(panel, "P2").Deflectors.Levels[0].Toggle();
            Assert.False(Row(panel, "P2").Deflectors.Levels[0].IsOn);
            Row(panel, "P3").Deflectors.Levels[0].Toggle();
            Assert.True(Row(panel, "P3").Deflectors.Levels[0].IsOn);
            Assert.False(Row(panel, "P2").Deflectors.Levels[0].IsOn);
        }

        [Fact]
        public void Dinamico_the_classic_dialog_keeps_the_marker_and_marks_a_desviador_chosen_for_the_first_time()
        {
            StaTestRunner.Run(() =>
            {
                // Over a desviador by post, accepted unchanged: it stays by post (without it RACKEDITAR would undo the correction).
                var draft = DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe());
                var dialog = RackCadModalDialogs.BuildDynamicSafetyDialog(draft);
                try
                {
                    dialog.BuildResultForTest();
                    Assert.True(SelectiveSafetyFamilies.SelectedOfType(dialog.Result, draft.Catalog.SafetyElements, Desviador)!.Desviador.CellsByPost);
                }
                finally
                {
                    dialog.Close();
                }

                // An existing rack keeps the historic reading through the classic dialog.
                var old = Draft(new RackProjectStore().Deserialize(DynamicJson())!.DynamicDesign);
                var classic = RackCadModalDialogs.BuildDynamicSafetyDialog(old);
                try
                {
                    classic.BuildResultForTest();
                    Assert.False(SelectiveSafetyFamilies.SelectedOfType(classic.Result, old.Catalog.SafetyElements, Desviador)!.Desviador.CellsByPost);
                }
                finally
                {
                    classic.Close();
                }
            });
        }

        [Fact]
        public void Dinamico_a_desviador_chosen_in_the_window_is_read_by_post()
        {
            var vm = DynamicEditor();
            var panel = vm.SafetyPanel;
            var inspector = Inspect(panel, Desviador);
            inspector.Variant.SelectedValue = RackCadSafetyPanelViewModel.NoVariant;
            inspector.ConfirmRemove();
            Assert.Null(Selected(vm, Desviador));
            Inspect(panel, Desviador).Variant.SelectedValue = Id(Desviador);
            Assert.True(Selected(vm, Desviador)!.Desviador.CellsByPost);
        }

        [Fact]
        public void Dinamico_the_guide_is_a_family_of_the_inspector_with_its_cells()
        {
            var vm = DynamicEditor();
            var panel = vm.SafetyPanel;
            vm.PickSafetyFamily(Guia);
            var inspector = panel.Inspector;
            Assert.Equal(("Guía de entrada", "G"), (inspector.Title, inspector.Letter));
            Assert.StartsWith("En el dibujo: " + Bom(vm, Guia) + " guías, una pareja en cada una de 5 celdas.", inspector.Pieces);
            Assert.False(Action(inspector, "all").IsEnabled);

            Action(inspector, "none").Run();
            Assert.Equal("Guía de entrada: en ninguna celda.", vm.GuideSummary);
            Assert.Equal("Guía de entrada: en ninguna celda. La variante y sus medidas se quedan; no se dibuja ni se cuenta nada.", vm.Message);
            Assert.NotNull(Selected(vm, Guia));
            Action(panel.Inspector, "matrix").Run();
            Assert.Equal(1, vm.SelectedTab);
        }

        // ================================================================== through the door (a double), as «Actualizar» writes it

        /// <summary>A door that reads and writes Dinámico and Selectivo, as the door of the Plugin; it records what goes to AutoCAD.</summary>
        private sealed class DoorDouble : IRackCadSelectiveDoorPort
        {
            public List<RackDoorRequest> Reads { get; } = new();

            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<RackDoorWrite> Writes { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted { add { } remove { } }

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write)
            {
                Writes.Add(write);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write)
            {
                SelectiveWrites.Add(write);
                return RackCadClassicResult.Started();
            }

            public void Abandon(long requestId)
            {
            }

            public void AnswerSelective(SelectivePalletDesign design)
            {
                var request = SelectiveReads.Last();
                SelectiveRead?.Invoke(Read(request.RequestId, request.RackId, design));
            }

            public void AnswerDynamic(string json)
            {
                var request = Reads.Last();
                DesignRead?.Invoke(new RackDesignRead(request.RequestId, request.RackId, true, "dynamic", "DIN 1", json, null));
            }

            public void WrittenSelective() => DesignWritten?.Invoke(new RackDesignWritten(SelectiveWrites.Last().RequestId, SelectiveWrites.Last().RackId, true, "Escrito."));

            public void WrittenDynamic() => DesignWritten?.Invoke(new RackDesignWritten(Writes.Last().RequestId, Writes.Last().RackId, true, "Escrito."));
        }

        private static RackCadWindowViewModel Window(DoorDouble door)
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
            var summary = new WorkspaceBrowserSummaryPortDouble
            {
                ListResponder = (_, _) => BrowserRackList.Ready(new[]
                {
                    new BrowserRackListEntry(RackId, "Selectivo A", "selective", new[] { "frontal" }),
                    new BrowserRackListEntry("DIN-1", "DIN 1", "dynamic", new[] { "lateral" }),
                }),
            };
            var panel = new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), summary, new WorkspacePilotEditPortDouble());
            var window = new RackCadWindowViewModel(panel, null, null, door, UiSupport.LoadCatalogSafe);
            window.Refresh();
            return window;
        }

        private static string DynamicJson()
        {
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(DynamicStructure(), "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            return new RackProjectStore().Serialize(RackProject.ForDynamic(draft.Design));
        }

        [Fact]
        public void Actualizar_hands_the_safety_edited_in_the_window_to_the_door_as_a_copy_and_the_table_waits_while_AutoCAD_writes()
        {
            var door = new DoorDouble();
            var window = Window(door);

            // Selectivo: a bota chosen in the window and one of its own on P2 go to RACKSELECTIVOAPLICAR in the copy of the draft.
            window.OpenRack(RackId);
            window.EditSelectiveHere();
            door.AnswerSelective(Structure());
            var selective = window.ActiveSelectiveEditor!;
            var panel = Shown(selective);
            Inspect(panel, Bota).Variant.SelectedValue = Id(Bota);
            Row(panel, "P2").Boots.Choice!.SelectedValue = "L";
            selective.RequestWrite();
            var write = Assert.Single(door.SelectiveWrites);
            var sent = SelectiveSafetyFamilies.SelectedOfType(write.Design.SafetySelections, Catalog.SafetyElements, Bota)!;
            Assert.Equal(new[] { (1, SafetySide.Left) }, sent.PostSides.Select(p => (p.PostIndex, p.Side)));
            Assert.True(selective.IsWriting);
            Assert.False(Row(panel, "P1").Boots.Choice!.IsEnabled);
            Assert.False(panel.Inspector.Variant.IsEnabled);
            Row(panel, "P1").Boots.Choice!.SelectedValue = "R"; // nothing is written while AutoCAD writes
            Assert.Null(RackCadSafetyEdit.BootOwn(Selected(selective, Bota), 0));
            Assert.Equal(new[] { (1, SafetySide.Left) }, sent.PostSides.Select(p => (p.PostIndex, p.Side)));
            door.WrittenSelective();
            Assert.False(selective.IsWriting);
            Assert.False(selective.IsDirty);
            Assert.True(Row(panel, "P1").Boots.Choice!.IsEnabled);

            // Dinámico: an own defense goes to RACKDINAMICOAPLICAR with its flag (SG-03).
            window.OpenRack("DIN-1");
            window.EditDynamicHere();
            door.AnswerDynamic(DynamicJson());
            var dynamic = window.ActiveDynamicEditor!;
            Write(Row(dynamic.SafetyPanel, "P3").DefenseExit.Length, "12");
            dynamic.RequestWrite();
            var dynamicWrite = Assert.Single(door.Writes);
            var defensa = SelectiveSafetyFamilies.SelectedOfType(dynamicWrite.Design.SafetySelections, Catalog.SafetyElements, Defensa)!;
            var record = defensa.DefensaPosts.Single(p => p.PostIndex == 2);
            Assert.Equal((false, 12.0, true), (record.ExitAuto, record.ExitLength, record.EntranceAuto));
            Assert.True(dynamic.IsWriting);
            Assert.False(Row(dynamic.SafetyPanel, "P1").DefenseExit.IsEnabled);
            door.WrittenDynamic();
            Assert.False(dynamic.IsDirty);
        }

        // ================================================================== the views (WPF, STA)

        private static void Lay(FrameworkElement view, double width, double height = 900)
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

        private static List<string> WithBindingErrors(Action body)
        {
            // What other tests left to lay out on the shared STA thread (a window shown by a test that runs this one inside its modal
            // loop, for example) is laid out first, so its binding errors are not counted here.
            new Border().UpdateLayout();
            var errors = new BindingErrors();
            PresentationTraceSources.Refresh();
            var source = PresentationTraceSources.DataBindingSource;
            var level = source.Switch.Level;
            source.Listeners.Add(errors);
            source.Switch.Level = SourceLevels.Error;
            try
            {
                body();
            }
            finally
            {
                source.Listeners.Remove(errors);
                source.Switch.Level = level;
            }

            return errors.Messages;
        }

        // Every control of the table and the inspector that a reader of the screen reaches has a name, and no two the same.
        private static void AssertUniqueNames(DependencyObject root)
        {
            var named = Descendants<Control>(root).Where(c => c is Button || c is ComboBox || c is CheckBox || c is TextBox)
                .Where(IsShown).Select(System.Windows.Automation.AutomationProperties.GetName).ToList();
            Assert.NotEmpty(named);
            Assert.DoesNotContain(named, string.IsNullOrWhiteSpace);
            Assert.Equal(named.Count, named.Distinct().Count());
        }

        // Shown on screen: the element and every element above it are visible (no window here, so not IsVisible).
        private static bool IsShown(DependencyObject element)
        {
            for (var at = element; at != null; at = System.Windows.Media.VisualTreeHelper.GetParent(at))
            {
                if (at is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            }

            return true;
        }

        [Fact]
        public void Selectivo_the_tab_is_built_with_no_binding_error_its_controls_write_and_the_inspector_goes_below_when_narrow()
        {
            StaTestRunner.Run(() =>
            {
                SelectiveEditorViewModel? vm = null;
                SelectiveEditorView? view = null;
                var errors = WithBindingErrors(() =>
                {
                    vm = SelectiveEditor(With(With(TwoFondos(), Bota, Desviador, Tope, Parrilla), EndsLateral(4)));
                    view = new SelectiveEditorView(vm);
                    Lay(view, 1500);
                    view.Tabs.SelectedItem = view.SafetyTab;
                    Lay(view, 1500);
                    foreach (var type in vm.SafetyPanel.Families)
                    {
                        vm.PickSafetyFamily(type);
                        Lay(view, 1500);
                    }
                });
                Assert.Empty(errors);

                Assert.False(view!.SafetyInspector.IsBelow);
                Assert.Equal(RackCadSafetyInspectorView.InspectorWidth, view.SafetyInspector.Width);
                var heads = new[] { view.SafetyTable.BootsHead, view.SafetyTable.SideGuardHead, view.SafetyTable.DeflectorsHead };
                Assert.All(heads, h => Assert.True(IsShown(h)));
                Assert.False(IsShown(view.SafetyTable.DefenseHeads)); // Dinámico only
                AssertUniqueNames(view.SafetyTable);
                AssertUniqueNames(view.SafetyInspector);

                // A title of a column shows its family; a level button writes its level; a list writes the bota of its post.
                Click(view.SafetyTable.DeflectorsHead);
                Assert.Equal("Desviadores", view.SafetyInspector.FamilyTitle.Text);
                var level = Descendants<Button>(view.SafetyTable.RowList).First(b => System.Windows.Automation.AutomationProperties.GetName(b) == "P1 · N1: con desviador. Pulsa para quitarlo.");
                Click(level);
                Lay(view, 1500);
                Assert.False(vm!.SafetyPanel.Table[0].Deflectors.Levels[0].IsOn);
                Assert.True(vm.IsDirty);
                var combo = Descendants<ComboBox>(view.SafetyTable.RowList).First(c => System.Windows.Automation.AutomationProperties.GetName(c) == "Botas en P2");
                combo.SelectedValue = "R";
                Lay(view, 1500);
                Assert.Equal(SafetySide.Right, RackCadSafetyEdit.BootOwn(Selected(vm, Bota), 1));

                // The head of a card shows its family.
                Click(Descendants<Button>(view.FamilyCardList).First(b => System.Windows.Automation.AutomationProperties.GetName(b) == "Parrillas: ver su variante y sus medidas"));
                Assert.Equal("Parrillas", view.SafetyInspector.FamilyTitle.Text);

                // Narrow (the palette starts at 600): below the tab, as the inspector of «Matriz».
                Lay(view, 600);
                Assert.True(view.SafetyInspector.IsBelow);
                Assert.Equal(1, Grid.GetRow(view.SafetyInspector));
                Lay(view, 1500);
                Assert.False(view.SafetyInspector.IsBelow);
            });
        }

        [Fact]
        public void Dinamico_the_tab_is_built_with_no_binding_error_and_the_box_of_a_defense_writes()
        {
            StaTestRunner.Run(() =>
            {
                DynamicEditorViewModel? vm = null;
                DynamicEditorView? view = null;
                var errors = WithBindingErrors(() =>
                {
                    vm = DynamicEditor();
                    view = new DynamicEditorView(vm);
                    Lay(view, 1500);
                    view.Tabs.SelectedItem = view.SafetyTab;
                    Lay(view, 1500);
                    foreach (var type in vm.SafetyPanel.Families)
                    {
                        vm.PickSafetyFamily(type);
                        Lay(view, 1500);
                    }
                });
                Assert.Empty(errors);

                Assert.True(IsShown(view!.SafetyTable.DefenseHeads));
                Assert.False(IsShown(view.SafetyTable.SeparatorsHead));
                AssertUniqueNames(view.SafetyTable);
                AssertUniqueNames(view.SafetyInspector);

                var box = Descendants<CheckBox>(view.SafetyTable.RowList).First(c => System.Windows.Automation.AutomationProperties.GetName(c) == "Defensa en entrada en P1");
                Assert.True(box.IsChecked);
                var toggle = (System.Windows.Automation.Provider.IToggleProvider)new System.Windows.Automation.Peers.CheckBoxAutomationPeer(box)
                    .GetPattern(System.Windows.Automation.Peers.PatternInterface.Toggle);
                toggle.Toggle();
                Lay(view, 1500);
                Assert.False(DynamicForkliftDefensePlan.ForSelection(Selected(vm!, Defensa), 0, 3).DrawsEntrance);
                Assert.Equal("Sin defensa", vm!.SafetyPanel.Table[0].DefenseEntrance.Sub);

                Click(view.GuidePickButton);
                Assert.Equal("Guía de entrada", view.SafetyInspector.FamilyTitle.Text);
                Assert.DoesNotContain(Descendants<Button>(view.SafetyTab.Content as DependencyObject ?? view), b => b.Content as string == "Editar seguridad…");
            });
        }
    }
}
