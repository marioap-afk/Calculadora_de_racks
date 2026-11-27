#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.PushBack;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T6 seg (I-64, sexta tanda; answer of the Owner of 2026-11-27, «Guardarla como la ventana»): the «Aceptar» of the classic dialog
    /// «Elementos de seguridad» writes what the RackCad window writes. A family whose variant is chosen and nothing else is kept with its
    /// defaults (dolor 3 of the inventory of «seguridad»: tope, parrilla and desviador without «Configurar…», the bota with its general side in
    /// «Ninguno», the protector lateral of a Selectivo without «Por poste…»), and a family left with nothing keeps its variant and its measures
    /// (dolor 4; C2-07, T4S-10, T4S-11 and PB-S17 3 of the window). Each test drives the REAL dialog (<see cref="SelectiveSafetyWindow"/>) with
    /// the arguments its classic editor gives it and compares with what the editor of the window writes. Generic sample racks.
    /// </summary>
    public sealed class ClassicSafetyAcceptTests
    {
        private const string Bota = SelectiveSafetyDefaults.BotaType;
        private const string Lateral = SelectiveSafetyDefaults.LateralType;
        private const string Desviador = SelectiveSafetyDefaults.DesviadorType;
        private const string Tope = SelectiveSafetyDefaults.TopeType;
        private const string Parrilla = SelectiveSafetyDefaults.ParrillaType;
        private const string Defensa = SelectiveSafetyDefaults.DefensaType;
        private const string Guia = SelectiveSafetyDefaults.GuiaType;

        private static RackCatalog Catalog => UiSupport.LoadCatalogSafe();

        private static IReadOnlyList<SafetyElementCatalogEntry> Variants(string type) => SelectiveSafetyFamilies.VariantsOfType(Catalog.SafetyElements, type);

        private static string Id(string type) => Variants(type).First().Id;

        private static SelectiveSafetySelection? Family(IEnumerable<SelectiveSafetySelection>? safety, string type)
            => SelectiveSafetyFamilies.SelectedOfType(safety, Catalog.SafetyElements, type);

        private static bool IsOf(SelectiveSafetySelection selection, string type)
            => Variants(type).Any(v => string.Equals(v.Id, selection?.ElementId, StringComparison.OrdinalIgnoreCase));

        // ================================================================== the classic dialog, as the user drives it

        // The variant list of a family, as the user picks it.
        private static void Choose(SelectiveSafetyWindow dialog, string type, string? id = null)
        {
            id ??= Id(type);
            var combo = VariantList(dialog, type);
            combo.SelectedItem = combo.Items.OfType<CatalogOption>().First(o => string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        private static ComboBox VariantList(SelectiveSafetyWindow dialog, string type)
            => SafetyDialogTestSupport.Descendants(dialog).OfType<ComboBox>()
                .Single(c => c.Items.OfType<CatalogOption>().Any(o => o.Id != null && Variants(type).Any(v => string.Equals(v.Id, o.Id, StringComparison.OrdinalIgnoreCase))));

        // The general side of the bota of a Selectivo («Ninguno / Izquierda / Derecha / Ambas»).
        private static ComboBox BootSide(SelectiveSafetyWindow dialog)
            => SafetyDialogTestSupport.Descendants(dialog).OfType<ComboBox>()
                .Single(c => (c.ToolTip as string ?? string.Empty).StartsWith("De qué lado del poste va la bota", StringComparison.Ordinal));

        private static Button ButtonWithTip(DependencyObject root, string start)
            => SafetyDialogTestSupport.Descendants(root).OfType<Button>()
                .Single(b => (b.ToolTip as string ?? string.Empty).StartsWith(start, StringComparison.Ordinal));

        private const string LateralPerPostTip = "Elige en qué postes va el protector lateral";
        private const string DesviadorTip = "Desviador:";

        private static void Click(ButtonBase button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        // A grid opens with the dialog as its owner, which WPF only allows once the dialog has been shown: off screen.
        private static void Offscreen(Window window)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.ShowInTaskbar = false;
            window.ShowActivated = false;
            window.Left = -10000;
            window.Top = -10000;
            window.Show();
            window.UpdateLayout();
        }

        // «Aceptar» of the classic dialog: its real OnOk, without the modal.
        private static IReadOnlyList<SelectiveSafetySelection> Accept(SelectiveSafetyWindow dialog, Action<SelectiveSafetyWindow>? gesture = null)
        {
            try
            {
                gesture?.Invoke(dialog);
                dialog.BuildResultForTest();
                return dialog.Result;
            }
            finally
            {
                dialog.Close();
            }
        }

        // «Aceptar» of the desviador grid, from inside the seam: its real BuildResult (the OnOk also sets DialogResult, which needs ShowDialog).
        private static bool? Accepted(SafetyDesviadorGridWindow grid)
        {
            var built = grid.BuildResult();
            if (built == null) return false;
            typeof(SafetyDesviadorGridWindow).GetProperty(nameof(SafetyDesviadorGridWindow.Result))!
                .GetSetMethod(nonPublic: true)!.Invoke(grid, new object[] { built });
            return true;
        }

        // ================================================================== Selectivo

        private static RackCadSafetyPanelViewModel Panel(SelectiveEditorViewModel vm)
        {
            vm.SelectedTab = SelectiveEditorViewModel.SafetyTabIndex;
            return vm.SafetyPanel;
        }

        private static RackCadSafetyInspector Inspect(RackCadSafetyPanelViewModel panel, string type)
        {
            panel.Pick(type);
            return panel.Inspector;
        }

        // The window: the variant of a family chosen in «Variante y medidas».
        private static void WindowChooses(RackCadSafetyPanelViewModel panel, string type) => Inspect(panel, type).Variant.SelectedValue = Id(type);

        // The window leaves a family with nothing: «En ninguna» / «En ninguno», the general side of the bota in «Ninguno», every protector
        // lateral in «No lleva», every end of the defense off.
        private static void WindowLeavesWithNothing(RackCadSafetyPanelViewModel panel, string type)
        {
            var inspector = Inspect(panel, type);
            switch (type)
            {
                case Bota:
                    inspector.Fields.OfType<SelectiveChoiceItem>().Single(f => f.Key == "side").SelectedValue = "N";
                    break;
                case Lateral:
                    foreach (var row in panel.Table.Where(r => !r.IsTotal && r.SideGuard.HasChoice)) row.SideGuard.Choice!.SelectedValue = "N";
                    break;
                case Defensa:
                    foreach (var row in panel.Table.Where(r => !r.IsTotal))
                    {
                        row.DefenseExit.IsOn = false;
                        row.DefenseEntrance.IsOn = false;
                    }

                    break;
                default:
                    inspector.Actions.Single(a => a.Key == "none").Run();
                    break;
            }
        }

        private static SelectiveSafetySelection? Selected(SelectiveEditorViewModel vm, string type) => SelectiveSafetyView.Selected(vm.Draft, type);

        // What the BOM of the last valid design counts of a family (the tope and the parrilla have their own categories).
        private static int Bom(SelectiveEditorViewModel vm, string type)
            => SelectiveBomBuilder.Build(vm.Draft.ResolveCopy()!, vm.Draft.Catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Safety || c.Category == SelectiveBomBuilder.Tope || c.Category == SelectiveBomBuilder.Parrilla)
                .Where(c => Variants(type).Any(v => string.Equals(v.Id, c.ProfileId, StringComparison.OrdinalIgnoreCase)))
                .Sum(c => c.Quantity);

        // The design of the window with its safety replaced by what the classic «Aceptar» gave.
        private static string WithClassic(SelectivePalletDesign design, IEnumerable<SelectiveSafetySelection> classic)
        {
            var copy = Store.Deserialize(Json(design))!.ToDomain();
            copy.SafetySelections.Clear();
            foreach (var selection in classic) copy.SafetySelections.Add(selection.DeepCopy());
            return Json(copy);
        }

        // The parrilla is not here: the classic dialog has no list of variant for it (it is not an exclusive family), so its «Configurar…» is
        // what chooses it. An untouched dialog still writes no parrilla (below).
        [Theory]
        [InlineData(Bota)]
        [InlineData(Lateral)]
        [InlineData(Tope)]
        [InlineData(Desviador)]
        public void Selectivo_choosing_only_the_variant_keeps_the_family_with_the_defaults_the_window_writes(string type)
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor(TwoFondos());
                Assert.Empty(vm.Draft.CopySafety());

                // RACKEDITAR › «Elementos de seguridad…»: the variant and «Aceptar», nothing else.
                var classic = Accept(RackCadSelectiveSafetyDialog.Build(vm.Draft), dialog => Choose(dialog, type));
                Assert.NotNull(Family(classic, type)); // dolor 3: «Aceptar» dropped it without a word

                // The window, for the same gesture: what the classic writes is what the window writes, byte for byte.
                WindowChooses(Panel(vm), type);
                Assert.False(vm.MessageIsError, vm.Message);
                Assert.Equal(Json(vm.Draft.Design), WithClassic(vm.Draft.Design, classic));
                Assert.True(Bom(vm, type) > 0);
            });
        }

        [Fact]
        public void Selectivo_an_untouched_dialog_writes_no_family_and_its_parrilla_is_chosen_by_its_Configurar()
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor(TwoFondos());
                var dialog = RackCadSelectiveSafetyDialog.Build(vm.Draft);
                Assert.Throws<InvalidOperationException>(() => VariantList(dialog, Parrilla)); // no list of variant for the parrilla
                Assert.Empty(Accept(dialog));
            });
        }

        [Fact]
        public void Selectivo_choosing_the_variant_shows_its_defaults_in_the_row_and_another_variant_keeps_what_the_row_has()
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor(TwoFondos());
                var dialog = RackCadSelectiveSafetyDialog.Build(vm.Draft);
                try
                {
                    // The bota: «Ninguno» while it has no variant; the variant brings «Ambas», the general side the window gives it.
                    var side = BootSide(dialog);
                    Assert.Equal("Ninguno", side.SelectedItem);
                    Choose(dialog, Bota);
                    Assert.Equal("Ambas", side.SelectedItem);

                    // Another variant changes only the variant (the window too); «Ninguno» and a variant again brings the defaults again
                    // (the window takes the family out, and a new one comes with them).
                    side.SelectedItem = "Izquierda";
                    Choose(dialog, Bota, Variants(Bota).Last().Id);
                    Assert.Equal("Izquierda", side.SelectedItem);
                    VariantList(dialog, Bota).SelectedIndex = 0;
                    side.SelectedItem = "Ninguno";
                    Choose(dialog, Bota);
                    Assert.Equal("Ambas", side.SelectedItem);

                    // The protector lateral of a Selectivo: the two posts of the ends, as its «Por poste…» seeds them.
                    var perPost = ButtonWithTip(dialog, LateralPerPostTip);
                    Assert.Equal("Por poste…", perPost.Content);
                    Choose(dialog, Lateral);
                    Assert.Equal("Por poste (2)…", perPost.Content);
                    dialog.BuildResultForTest();
                    var posts = Enumerable.Range(0, vm.Draft.FondoCount).Select(vm.Draft.FrontCount).Max() + 1; // the longest fondo
                    Assert.Equal(new[] { (0, SafetySide.Left), (posts - 1, SafetySide.Right) },
                        Family(dialog.Result, Lateral)!.PostSides.Select(p => (p.PostIndex, p.Side)));
                    Assert.Equal((SafetySide.Both, (BootPlacement?)BootPlacement.Both), (Family(dialog.Result, Bota)!.Side, Family(dialog.Result, Bota)!.Bota.Placement));
                }
                finally
                {
                    dialog.Close();
                }
            });
        }

        [Theory]
        [InlineData(Tope)]
        [InlineData(Parrilla)]
        [InlineData(Desviador)]
        [InlineData(Lateral)]
        [InlineData(Bota)]
        public void Selectivo_a_family_the_window_left_with_nothing_keeps_its_variant_through_the_classic_Aceptar(string type)
        {
            StaTestRunner.Run(() =>
            {
                var vm = SelectiveEditor(TwoFondos());
                var panel = Panel(vm);
                WindowChooses(panel, type);
                WindowLeavesWithNothing(panel, type);
                Assert.False(vm.MessageIsError, vm.Message);
                Assert.NotNull(Selected(vm, type));
                Assert.Equal(0, Bom(vm, type)); // nothing is drawn or counted

                // RACKEDITAR: the classic dialog over what the window wrote, accepted without touching anything.
                var classic = Accept(RackCadSelectiveSafetyDialog.Build(vm.Draft));
                Assert.NotNull(Family(classic, type)); // dolor 4: «Aceptar» dropped the variant and its measures
                Assert.Equal(Json(vm.Draft.Design), WithClassic(vm.Draft.Design, classic));
            });
        }

        // ================================================================== Dinámico

        // Two fronts (3 and 2 levels; posts P1..P3), with or without the default safety of a new rack.
        private static DynamicRackDesign DynamicStructure(bool withSafety)
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
            if (withSafety)
            {
                foreach (var selection in DynamicSafetyDefaults.Build(Catalog)) design.SafetySelections.Add(selection.DeepCopy());
            }

            return design;
        }

        // The editor of the window over that rack, opened from its document as the window opens it.
        private static DynamicEditorViewModel DynamicEditor(bool withSafety)
        {
            var store = new RackProjectStore();
            var draft = new DynamicDraftEditor(Catalog);
            draft.LoadFrom(DynamicStructure(withSafety), "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            var opened = new DynamicDraftEditor(Catalog);
            opened.LoadFrom(store.Deserialize(store.Serialize(RackProject.ForDynamic(draft.Design)))!.DynamicDesign, "DIN 1");
            return new DynamicEditorViewModel(opened, "DIN-1", _ => { });
        }

        private static string DynamicJson(DynamicRackDesign design) => new RackProjectStore().Serialize(RackProject.ForDynamic(design));

        private static string WithClassic(DynamicRackDesign design, IEnumerable<SelectiveSafetySelection> classic)
        {
            var copy = new RackProjectStore().Deserialize(DynamicJson(design))!.DynamicDesign;
            copy.SafetySelections.Clear();
            foreach (var selection in classic) copy.SafetySelections.Add(selection.DeepCopy());
            return DynamicJson(copy);
        }

        private static int Bom(DynamicEditorViewModel vm, string type)
            => SystemBomBuilder.Build(vm.Draft.System, vm.Draft.Catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Safety)
                .Where(c => Variants(type).Any(v => string.Equals(v.Id, c.ProfileId, StringComparison.OrdinalIgnoreCase)))
                .Sum(c => c.Quantity);

        [Theory]
        [InlineData(Bota)]
        [InlineData(Lateral)]
        [InlineData(Desviador)]
        [InlineData(Guia)]
        [InlineData(Defensa)]
        public void Dinamico_choosing_only_the_variant_keeps_the_family_with_the_defaults_the_window_writes(string type)
        {
            StaTestRunner.Run(() =>
            {
                var vm = DynamicEditor(withSafety: false);
                Assert.Empty(vm.Draft.CopySafety());

                var classic = Accept(RackCadModalDialogs.BuildDynamicSafetyDialog(vm.Draft), dialog => Choose(dialog, type));
                Assert.NotNull(Family(classic, type)); // dolor 3 (the bota and the desviador were dropped)

                WindowChooses(vm.SafetyPanel, type);
                Assert.False(vm.MessageIsError, vm.Message);
                Assert.Equal(DynamicJson(vm.Draft.Design), WithClassic(vm.Draft.Design, classic));
                Assert.True(Bom(vm, type) > 0);
            });
        }

        [Theory]
        [InlineData(Guia)]
        [InlineData(Lateral)]
        [InlineData(Bota)]
        [InlineData(Defensa)]
        public void Dinamico_a_family_the_window_left_with_nothing_keeps_its_variant_through_the_classic_Aceptar(string type)
        {
            StaTestRunner.Run(() =>
            {
                // The family is chosen in the window first: a bota of the default safety of a new rack has no general placement, and on it the
                // classic writes one where the window does not (T4S-02, declared in the fourth batch, not this defect).
                var vm = DynamicEditor(withSafety: false);
                WindowChooses(vm.SafetyPanel, type);
                Assert.NotNull(Family(vm.Draft.SafetySelections, type));
                WindowLeavesWithNothing(vm.SafetyPanel, type);
                Assert.False(vm.MessageIsError, vm.Message);
                Assert.NotNull(Family(vm.Draft.SafetySelections, type));
                Assert.Equal(0, Bom(vm, type));

                var classic = Accept(RackCadModalDialogs.BuildDynamicSafetyDialog(vm.Draft));
                Assert.NotNull(Family(classic, type)); // dolor 4
                Assert.Equal(DynamicJson(vm.Draft.Design), WithClassic(vm.Draft.Design, classic));
            });
        }

        [Fact]
        public void Dinamico_a_desviador_turned_off_in_every_level_of_its_grid_keeps_its_variant_and_its_measures()
        {
            StaTestRunner.Run(() =>
            {
                var vm = DynamicEditor(withSafety: true);
                var before = Family(vm.Draft.SafetySelections, Desviador)!;
                var dialog = RackCadModalDialogs.BuildDynamicSafetyDialog(vm.Draft);
                SafetyDesviadorGridWindow? grid = null;
                dialog.DesviadorDialog = window =>
                {
                    grid = window;
                    window.Model.SetAll(false); // «Ninguno» of the grid
                    return Accepted(window);
                };
                try
                {
                    Offscreen(dialog);
                    Click(ButtonWithTip(dialog, DesviadorTip));
                    Assert.NotNull(grid);
                    dialog.BuildResultForTest();
                }
                finally
                {
                    grid?.Close();
                    dialog.Close();
                }

                var desviador = Family(dialog.Result, Desviador);
                Assert.NotNull(desviador); // dolor 4: «Aceptar» dropped it
                Assert.Equal((before.ElementId, before.DesviadorLongitud, before.DesviadorPrimerNivelAltura, before.Desviador.CellsByPost),
                    (desviador!.ElementId, desviador.DesviadorLongitud, desviador.DesviadorPrimerNivelAltura, desviador.Desviador.CellsByPost));
                var levels = vm.Draft.DeflectorLevelsPerPost();
                Assert.Equal(levels.Sum(), desviador.DesviadorOffCells.Count);

                // The same drawing and the same BOM as without it.
                vm.Draft.ReplaceSafety(dialog.Result.Select(s => s.DeepCopy()).ToList());
                Assert.True(vm.Draft.Recompose(), vm.Draft.LastError);
                Assert.Equal(0, Bom(vm, Desviador));
            });
        }

        // Dolor 6 of the inventory, checked: an own length equal to the automatic one is no longer lost in the classic dialog of Dinámico. The
        // grid of defenses has the «Auto» column of each end since the fourth batch (T4S-14), so «Auto» cleared on an edge post keeps its
        // 12 in, and it stays 12 in when the post becomes intermediate. Nothing to fix: this test only documents it.
        [Fact]
        public void Dinamico_an_own_defense_equal_to_the_automatic_one_is_kept_by_the_classic_grid()
        {
            StaTestRunner.Run(() =>
            {
                var vm = DynamicEditor(withSafety: true);
                var dialog = RackCadModalDialogs.BuildDynamicSafetyDialog(vm.Draft);
                SafetyDefensaGridWindow? grid = null;
                dialog.DefensaDialog = window =>
                {
                    grid = window;
                    window.ExitAutoForTest(2)!.IsChecked = false; // P3, an edge post: its 12 in becomes its own
                    window.BuildResultForTest();
                    return window.Accepted;
                };
                try
                {
                    Offscreen(dialog);
                    Click(dialog.DefensaButtonForTest!);
                    Assert.NotNull(grid);
                    dialog.BuildResultForTest();
                }
                finally
                {
                    grid?.Close();
                    dialog.Close();
                }

                var defensa = Family(dialog.Result, Defensa)!;
                var record = defensa.DefensaPosts.Single(p => p.PostIndex == 2);
                Assert.Equal((false, 12.0, true), (record.ExitAuto, record.ExitLength, record.EntranceAuto));
                Assert.Equal(12.0, DynamicForkliftDefensePlan.ForSelection(defensa, 2, 4).ExitLength); // one front more: P3 is intermediate
            });
        }

        // ================================================================== Push Back (its classic editor, through the real dialog)

        private static RackPushBackSystemWindow PushBack()
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

        private static SelectiveSafetySelection? Designed(RackPushBackSystemWindow w, string type)
            => Family(w.LastComputation?.Design?.Structure?.SafetySelections, type);

        // «Elementos de seguridad…» of the classic Push Back editor: the real dialog, a gesture, «Aceptar».
        private static void PushBackAccepts(RackPushBackSystemWindow w, Action<SelectiveSafetyWindow> gesture)
        {
            w.SafetyWindowDialog = dialog =>
            {
                Offscreen(dialog);
                try
                {
                    gesture(dialog);
                    dialog.BuildResultForTest();
                }
                finally
                {
                    dialog.Close();
                }

                return true;
            };
            try
            {
                EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            }
            finally
            {
                w.SafetyWindowDialog = null;
            }
        }

        [Fact]
        public void PushBack_a_new_desviador_chosen_with_nothing_else_is_kept_as_its_grid_accepts_it_on_the_low_end()
        {
            StaTestRunner.Run(() =>
            {
                var w = PushBack();
                try
                {
                    w.State.SetFrontCount(3);
                    w.SafetyDialog = current => current.Where(s => !IsOf(s, Desviador)).Select(s => s.DeepCopy()).ToList();
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    w.SafetyDialog = null;
                    Assert.Null(Designed(w, Desviador));

                    PushBackAccepts(w, dialog => Choose(dialog, Desviador));
                    var desviador = Designed(w, Desviador);
                    Assert.NotNull(desviador); // dolor 3 (PB-S17, difference 6): it asked for «Configurar…» too

                    // What the window writes for a new desviador (PushBackSafetyEditTests): the side of the low end, 18 in, every cell.
                    Assert.Equal((SafetySide.Left, (SafetySide?)SafetySide.Left, 18.0, 18.0, false),
                        (desviador!.Side, desviador.AuthoredSide, desviador.DesviadorLongitud, desviador.DesviadorPrimerNivelAltura, desviador.Desviador.CellsByPost));
                    Assert.Empty(desviador.DesviadorOffCells);
                    Assert.True(PushBackDiverterPlan.Resolve(w.LastComputation.System, w.Session.Catalog).Count > 0);
                }
                finally
                {
                    w.Close();
                }
            });
        }

        [Fact]
        public void PushBack_a_protector_lateral_in_no_post_and_a_desviador_in_no_level_keep_their_variant()
        {
            StaTestRunner.Run(() =>
            {
                var w = PushBack();
                try
                {
                    w.State.SetFrontCount(3); // four posts
                    var lateralId = Designed(w, Lateral)?.ElementId ?? Family(w.EditableStateForTest.SafetySelections, Lateral)!.ElementId;

                    // Every post of the protector lateral on «Ninguno» in its «Por poste…».
                    PushBackAccepts(w, dialog =>
                    {
                        dialog.PerPostDialog = window =>
                        {
                            for (var post = 0; post < 4; post++) window.SetForTest(post, BootPlacement.None);
                            window.BuildResultForTest();
                            return true;
                        };
                        Click(ButtonWithTip(dialog, LateralPerPostTip));
                    });
                    var lateral = Designed(w, Lateral);
                    Assert.NotNull(lateral); // dolor 4 (PB-S17, difference 3)
                    Assert.Equal(lateralId, lateral!.ElementId);
                    Assert.Equal(new[] { 0, 1, 2, 3 }, lateral.PostSides.Select(p => p.PostIndex));
                    Assert.All(lateral.PostSides, p => Assert.Equal(SafetySide.None, p.Side));

                    // The desviador with every cell of its grid off.
                    PushBackAccepts(w, dialog =>
                    {
                        dialog.DesviadorDialog = grid =>
                        {
                            try
                            {
                                grid.Model.SetAll(false);
                                return Accepted(grid);
                            }
                            finally
                            {
                                grid.Close();
                            }
                        };
                        Click(ButtonWithTip(dialog, DesviadorTip));
                    });
                    var desviador = Designed(w, Desviador);
                    Assert.NotNull(desviador); // dolor 4 (PB-S17, difference 3)
                    Assert.NotEmpty(desviador!.DesviadorOffCells);
                    Assert.Empty(PushBackDiverterPlan.Resolve(w.LastComputation.System, w.Session.Catalog));
                }
                finally
                {
                    w.Close();
                }
            });
        }
    }
}
