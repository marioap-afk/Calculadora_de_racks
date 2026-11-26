#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Step 4 of Dinámico in the RackCad window, «Extremos y seguridad» (decision 6 of the prototype: «Seguridad por poste en sección propia;
    /// guía de entrada marcada en la matriz» → «De acuerdo»): the cells with an entrance guide are marked in the matrix as the drawing places
    /// them; the guide is put in or taken out of cells; the safety of each post is what the BOM counts; and «Editar seguridad…» is the dialog
    /// of the classic editor, whose result the draft takes.
    /// </summary>
    public class DynamicSafetyTabTests
    {
        // Two fronts (3 and 2 levels) with the default safety of a new rack (botas, protectors, deflectors, defenses and the guide).
        private static DynamicRackDesign Structure()
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

        private static string Json(DynamicRackDesign design) => new RackProjectStore().Serialize(RackProject.ForDynamic(design));

        private static string FullJson()
        {
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(Structure(), "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            return Json(draft.Design);
        }

        private static DynamicEditorViewModel Editor()
        {
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(new RackProjectStore().Deserialize(FullJson())!.DynamicDesign, "DIN 1");
            return new DynamicEditorViewModel(draft, "DIN-1", _ => { });
        }

        private static IEnumerable<DynamicCellItem> Cells(DynamicEditorViewModel vm) => vm.Rows.SelectMany(r => r.Cells).Where(c => c.Exists);

        private static int BomQuantityOfType(DynamicEditorViewModel vm, string type)
        {
            var catalog = vm.Draft.Catalog;
            return SystemBomBuilder.Build(vm.Draft.System, catalog).Components
                .Where(c => c.Category == SelectiveBomBuilder.Safety)
                .Where(c => catalog.SafetyElements.Any(e => string.Equals(e.Id, c.ProfileId, StringComparison.OrdinalIgnoreCase)
                                                            && SelectiveSafetyDefaults.IsType(e.Type, type)))
                .Sum(c => c.Quantity);
        }

        [Fact]
        public void TheMatrixMarksTheCellsWithAnEntranceGuideAsTheDrawingPlacesThem()
        {
            var vm = Editor();
            Assert.NotNull(vm.Draft.GuideSelection);

            Assert.All(Cells(vm), cell => Assert.True(cell.HasGuide, cell.AccessibleText));
            Assert.Equal("Guía de entrada: en todas las celdas (5).", vm.GuideSummary);
            Assert.Equal(true, vm.SelectionGuide);
            Assert.Contains(", con guía de entrada", Cells(vm).First().AccessibleText);

            // The marks are the cells of the plan every view and the BOM use: two pieces per cell.
            Assert.Equal(2 * Cells(vm).Count(c => c.HasGuide), BomQuantityOfType(vm, SelectiveSafetyDefaults.GuiaType));
        }

        [Fact]
        public void TheGuideOfTheInspectorTakesItOutOfTheSelectedCellsAndPutsItBack()
        {
            var vm = Editor();
            var baseline = Json(vm.Draft.Design);
            var guides = BomQuantityOfType(vm, SelectiveSafetyDefaults.GuiaType);
            vm.ClickCell(0, 0, control: false, shift: false);

            vm.SelectionGuide = false;

            Assert.False(Cells(vm).Single(c => c.FrontIndex == 0 && c.LevelIndex == 0).HasGuide);
            Assert.Equal(4, Cells(vm).Count(c => c.HasGuide));
            Assert.Equal("Guía de entrada: en 4 de 5 celdas.", vm.GuideSummary);
            Assert.Equal(false, vm.SelectionGuide);
            Assert.True(vm.IsDirty);
            Assert.Equal(guides - 2, BomQuantityOfType(vm, SelectiveSafetyDefaults.GuiaType));
            var off = Assert.Single(vm.Draft.Design.SafetySelections.Single(s => s.GuiaEntradaOffCells.Count > 0).GuiaEntradaOffCells);
            Assert.Equal((0, 0), (off.Frente, off.Level));

            vm.SelectAll();
            Assert.Null(vm.SelectionGuide);

            vm.ClickCell(0, 0, control: false, shift: false);
            vm.SelectionGuide = true;
            Assert.All(Cells(vm), cell => Assert.True(cell.HasGuide));
            Assert.Equal(guides, BomQuantityOfType(vm, SelectiveSafetyDefaults.GuiaType));
            Assert.Equal(baseline, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);
        }

        // I-01 of the review of Selectivo, in Dinámico. The approved prototype: «Guía de entrada (varía; marcar la pone en todas)». WPF turns
        // a click on an indeterminate box into false; the box must put the guide in every cell of the selection, never take it out.
        private static DynamicEditorViewModel MixedSelection()
        {
            var vm = Editor();
            vm.ClickCell(0, 0, control: false, shift: false);
            vm.SelectionGuide = false;
            vm.SelectAll();
            Assert.Null(vm.SelectionGuide);
            Assert.Equal(4, Cells(vm).Count(c => c.HasGuide));
            return vm;
        }

        [Fact]
        public void WhenTheGuideVariesAWriteOfTheBoxPutsItInEveryCellOfTheSelection()
        {
            var vm = MixedSelection();
            Assert.Equal("Guía de entrada (varía; marcar la pone en todas)", vm.SelectionGuideLabel);

            vm.SelectionGuide = false; // what WPF writes when the indeterminate box is clicked
            Assert.All(Cells(vm), cell => Assert.True(cell.HasGuide));
            Assert.Equal(true, vm.SelectionGuide);
            Assert.Equal("Guía de entrada", vm.SelectionGuideLabel);
            Assert.Equal("Guía de entrada: en todas las celdas (5).", vm.GuideSummary);
            Assert.False(vm.IsDirty); // the rack is back as it was opened: the guide in every cell

            // Marking it says the same; a null write changes nothing.
            vm = MixedSelection();
            vm.SelectionGuide = true;
            Assert.All(Cells(vm), cell => Assert.True(cell.HasGuide));
            vm = MixedSelection();
            vm.SelectionGuide = null;
            Assert.Null(vm.SelectionGuide);
            Assert.Equal(4, Cells(vm).Count(c => c.HasGuide));

            // Once they agree the box toggles as any box: unmarking takes it out of the selection.
            vm = MixedSelection();
            vm.SelectionGuide = false;
            Assert.Equal(true, vm.SelectionGuide);
            vm.SelectionGuide = false;
            Assert.DoesNotContain(Cells(vm), cell => cell.HasGuide);
            Assert.Equal(false, vm.SelectionGuide);
        }

        [Fact]
        public void AClickOnTheIndeterminateGuideBoxPutsTheGuideInEveryCellOfTheSelection()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                var view = LaidOut(vm);
                vm.ClickCell(0, 0, control: false, shift: false);
                vm.SelectionGuide = false;
                vm.SelectAll();
                Lay(view);

                var box = view.SelectionGuideBox;
                Assert.Null(box.IsChecked);
                Assert.False(box.IsThreeState);
                Assert.True(box.IsEnabled);
                Assert.Equal("Guía de entrada (varía; marcar la pone en todas)", box.Content);

                var toggle = (System.Windows.Automation.Provider.IToggleProvider)new System.Windows.Automation.Peers.CheckBoxAutomationPeer(box)
                    .GetPattern(System.Windows.Automation.Peers.PatternInterface.Toggle);
                toggle.Toggle();
                Lay(view);

                Assert.All(Cells(vm), cell => Assert.True(cell.HasGuide, cell.AccessibleText));
                Assert.Equal(true, box.IsChecked);
                Assert.Equal("Guía de entrada", box.Content);
                Assert.False(vm.IsDirty);

                // A second click takes it out of the whole selection, as any box does.
                toggle.Toggle();
                Lay(view);
                Assert.DoesNotContain(Cells(vm), cell => cell.HasGuide);
                Assert.Equal(false, box.IsChecked);
            });
        }

        // M-01 of the review of the fix of Dinámico: a selection with no active cell (here, a cell of a front that is blank in the drawing)
        // has nothing to put the guide in. The box is disabled, and a write of it changes nothing: no recomposition, no message, no
        // «Modificado».
        [Fact]
        public void WithNoActiveCellInTheSelectionTheGuideBoxWritesNothing()
        {
            var structure = Structure();
            structure.Fronts[1].IsActive = false;
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(structure, "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            var opened = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            opened.LoadFrom(new RackProjectStore().Deserialize(Json(draft.Design))!.DynamicDesign, "DIN 1");
            var vm = new DynamicEditorViewModel(opened, "DIN-1", _ => { });
            Assert.False(vm.IsDirty);
            var baseline = Json(vm.Draft.Design);
            var offCells = vm.Draft.GuideSelection!.GuiaEntradaOffCells.Select(c => (c.Frente, c.Level)).OrderBy(c => c).ToArray();

            // A cell with the guide: the box can be written.
            Assert.True(vm.CanEditSelectionGuide);
            vm.ClickCell(1, 0, control: false, shift: false);
            Assert.True(vm.CanEditGuide);
            Assert.False(vm.CanEditSelectionGuide);
            Assert.Equal(false, vm.SelectionGuide);

            vm.EditSafety(); // a message the write would replace
            var message = vm.Message;
            Assert.False(string.IsNullOrEmpty(message));
            var raised = new List<string?>();
            vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            vm.SelectionGuide = true;
            vm.SelectionGuide = false;

            Assert.All(raised, name => Assert.Equal(nameof(DynamicEditorViewModel.SelectionGuide), name));
            Assert.Equal(message, vm.Message);
            Assert.False(vm.IsDirty);
            Assert.Equal(baseline, Json(vm.Draft.Design));
            Assert.Equal(offCells, vm.Draft.GuideSelection!.GuiaEntradaOffCells.Select(c => (c.Frente, c.Level)).OrderBy(c => c).ToArray());

            // Back on an active cell, the box writes as before.
            vm.ClickCell(0, 0, control: false, shift: false);
            Assert.True(vm.CanEditSelectionGuide);
            vm.SelectionGuide = false;
            Assert.True(vm.IsDirty);
        }

        [Fact]
        public void TheGuideBoxIsDisabledWithNoActiveCellInTheSelection()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                vm.SetFrontBlank(1, true);
                vm.ClickCell(0, 0, control: false, shift: false);
                var view = LaidOut(vm);
                Assert.True(view.SelectionGuideBox.IsEnabled);

                vm.ClickCell(1, 0, control: false, shift: false);
                Lay(view);
                Assert.False(view.SelectionGuideBox.IsEnabled);

                vm.ClickCell(0, 1, control: false, shift: false);
                Lay(view);
                Assert.True(view.SelectionGuideBox.IsEnabled);
            });
        }

        [Fact]
        public void InNoCellAndInEveryCellWriteTheActiveCellsAndLeaveABlankFrontDormant()
        {
            var vm = Editor();
            vm.SetFrontBlank(1, true);

            vm.GuideNowhere();
            Assert.DoesNotContain(Cells(vm), cell => cell.HasGuide);
            Assert.Equal("Guía de entrada: en ninguna celda.", vm.GuideSummary);
            Assert.Equal(0, BomQuantityOfType(vm, SelectiveSafetyDefaults.GuiaType));
            // Only the cells of F1 were taken out: the dormant cells of the blank F2 keep what they had.
            Assert.Equal(new[] { (0, 0), (0, 1), (0, 2) },
                vm.Draft.GuideSelection!.GuiaEntradaOffCells.Select(c => (c.Frente, c.Level)).OrderBy(c => c).ToArray());

            vm.GuideEverywhere();
            Assert.Equal("Guía de entrada: en todas las celdas (3).", vm.GuideSummary);
            Assert.Empty(vm.Draft.GuideSelection!.GuiaEntradaOffCells);
        }

        [Fact]
        public void TheSafetyPostByPostAddsUpToTheSafetyOfTheBom()
        {
            var vm = Editor();
            var rows = vm.Draft.PostSafety();

            Assert.Equal(3, rows.Count);
            Assert.Equal(3, vm.PostSafety.Count);
            Assert.Equal(new[] { "P1", "P2", "P3" }, vm.PostSafety.Select(p => p.Post).ToArray());
            Assert.Equal(BomQuantityOfType(vm, SelectiveSafetyDefaults.BotaType), rows.Sum(r => r.Boots));
            Assert.Equal(BomQuantityOfType(vm, SelectiveSafetyDefaults.LateralType), rows.Sum(r => r.SideGuards));
            Assert.Equal(BomQuantityOfType(vm, SelectiveSafetyDefaults.DesviadorType), rows.Sum(r => r.DeflectorsAtExit + r.DeflectorsAtEntrance));
            Assert.Equal(BomQuantityOfType(vm, SelectiveSafetyDefaults.DefensaType), rows.Sum(r => r.Defenses));
            Assert.Equal(BomQuantityOfType(vm, SelectiveSafetyDefaults.GuiaType), rows.Sum(r => r.EntranceGuides));
            Assert.EndsWith(", de 3 postes.", vm.SafetySummary);

            // Every enabled cell gives one guide at each of the two posts of its front: F1 (3 levels) at P1 and P2, F2 (2) at P2 and P3.
            Assert.Equal(new[] { 3, 5, 2 }, rows.Select(r => r.EntranceGuides).ToArray());
        }

        [Fact]
        public void EditSafetyTakesTheResultOfTheDialogAndACancelChangesNothing()
        {
            var vm = Editor();
            Assert.False(vm.CanEditSafety);
            vm.EditSafety();
            Assert.Equal(vm.SafetyEditText, vm.Message);
            Assert.StartsWith("El diálogo de seguridad no está disponible aquí", vm.SafetyEditText);

            var baseline = Json(vm.Draft.Design);
            DynamicDraftEditor? seen = null;
            vm.SafetyEditor = draft => { seen = draft; return null; };
            Assert.True(vm.CanEditSafety);
            vm.EditSafety();
            Assert.Same(vm.Draft, seen);
            Assert.False(vm.IsDirty);
            Assert.Equal(baseline, Json(vm.Draft.Design));

            // Accepted without the guide: the draft takes it, the matrix loses its marks and the rack is «Modificado».
            vm.SafetyEditor = draft => draft.CopySafety().Where(s => !IsGuide(draft, s)).ToList();
            vm.EditSafety();
            Assert.Null(vm.Draft.GuideSelection);
            Assert.DoesNotContain(Cells(vm), cell => cell.HasGuide);
            Assert.False(vm.CanEditGuide);
            Assert.StartsWith("Este rack no lleva guía de entrada", vm.GuideSummary);
            Assert.True(vm.IsDirty);
            Assert.Equal(0, BomQuantityOfType(vm, SelectiveSafetyDefaults.GuiaType));

            vm.GuideNowhere();
            Assert.Equal(vm.GuideSummary, vm.Message);
        }

        private static bool IsGuide(DynamicDraftEditor draft, SelectiveSafetySelection selection)
            => draft.Catalog.SafetyElements.Any(e => string.Equals(e.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase)
                                                     && SelectiveSafetyDefaults.IsType(e.Type, SelectiveSafetyDefaults.GuiaType));

        [Fact]
        public void WhileAutoCadWritesTheSafetyIsNotEdited()
        {
            var vm = Editor();
            vm.SafetyEditor = draft => draft.CopySafety();
            vm.ClickCell(0, 0, control: false, shift: false);
            vm.SelectionGuide = false;
            vm.BeginWriting();
            Assert.True(vm.IsWriting);

            Assert.False(vm.CanEditGuide);
            Assert.False(vm.CanEditSafety);
            var json = Json(vm.Draft.Design);
            vm.GuideEverywhere();
            vm.EditSafety();
            Assert.Equal(json, Json(vm.Draft.Design));
        }

        [Fact]
        public void TheSafetyDialogOfTheWindowIsTheOneOfTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                var classic = new RackDynamicSystemWindow();
                IReadOnlyList<int> levels, deflectors;
                try
                {
                    classic.LoadExisting(new RackProjectStore().Deserialize(FullJson())!.DynamicDesign, "DIN-1", "DIN 1");
                    classic.BuildDesignForTest(out var ok);
                    Assert.True(ok);
                    levels = classic.SafetyLevelsPerFrente();
                    deflectors = classic.DesviadorLevelsPerPost();
                    Assert.Equal(levels.ToArray(), vm.Draft.SafetyLevelsPerFront().ToArray());
                    Assert.Equal(deflectors.ToArray(), vm.Draft.DeflectorLevelsPerPost().ToArray());
                }
                finally { classic.Close(); }

                // The dialog of the window and the one the classic editor builds (its call, word for word), accepted with no change,
                // give the same safety. Accepting normalizes some values, in both (the general placement of the boot, for instance).
                var catalog = vm.Draft.Catalog;
                var elements = (catalog.SafetyElements ?? new List<RackCad.Application.Catalogs.SafetyElementCatalogEntry>())
                    .Where(element => element != null
                                      && (SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.BotaType)
                                          || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.LateralType)
                                          || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DesviadorType)
                                          || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType)
                                          || SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.GuiaType)))
                    .ToList();
                var reference = new SelectiveSafetyWindow(elements, vm.Draft.CopySafety(), postCount: Math.Max(2, vm.Draft.System.Fronts.Count + 1),
                    levelsPerFrente: levels, fondoCount: 1, catalog: catalog, fallbackLevelsArePerPost: false,
                    introduction: RackCadModalDialogs.DynamicSafetyIntroduction, includeDefensa: true, includeGuia: true,
                    useDynamicSafetyDefaults: true, bootUsesPlacementNames: true, desviadorLevelsPerPost: deflectors,
                    allowBlankFrontColumns: true, showDesviadorSide: true);
                var dialog = RackCadModalDialogs.BuildDynamicSafetyDialog(vm.Draft);
                try
                {
                    reference.BuildResultForTest();
                    dialog.BuildResultForTest();
                    Assert.Equal(WithSafety(vm.Draft.Design, reference.Result), WithSafety(vm.Draft.Design, dialog.Result));

                    vm.SafetyEditor = _ => dialog.Result;
                    vm.EditSafety();
                    Assert.Equal(WithSafety(vm.Draft.Design, reference.Result), Json(vm.Draft.Design));
                }
                finally
                {
                    dialog.Close();
                    reference.Close();
                }
            });
        }

        // The design with this safety, as the drawing would hold it.
        private static string WithSafety(DynamicRackDesign design, IEnumerable<SelectiveSafetySelection> safety)
        {
            var copy = new RackProjectStore().Deserialize(Json(design))!.DynamicDesign;
            copy.SafetySelections.Clear();
            foreach (var selection in safety) copy.SafetySelections.Add(selection.DeepCopy());
            return Json(copy);
        }

        [Fact]
        public void EditSafetyShowsTheDialogThroughThePortOfTheHost()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                var port = new ModalDouble();
                Assert.Null(RackCadModalDialogs.EditDynamicSafety(vm.Draft, port));
                var shown = Assert.IsType<SelectiveSafetyWindow>(Assert.Single(port.Shown));
                shown.Close();
                Assert.Null(RackCadModalDialogs.EditDynamicSafety(vm.Draft, null!));
            });
        }

        private sealed class ModalDouble : IRackCadModalPort
        {
            public List<System.Windows.Window> Shown { get; } = new();

            public bool? ShowModal(System.Windows.Window dialog)
            {
                Shown.Add(dialog);
                return false;
            }
        }

        // ------------------------------------------------------------------ O-6, O-7 and O-8 of the smoke of the second delivery

        private static DynamicEditorView LaidOut(DynamicEditorViewModel vm)
        {
            var view = new DynamicEditorView(vm);
            Lay(view);
            return view;
        }

        private static void Lay(System.Windows.FrameworkElement view)
        {
            view.Measure(new System.Windows.Size(1200, 900));
            view.Arrange(new System.Windows.Rect(0, 0, 1200, 900));
            view.UpdateLayout();
        }

        private static IEnumerable<T> Descendants<T>(System.Windows.DependencyObject root) where T : System.Windows.DependencyObject
        {
            for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        private static System.Windows.Rect BoundsIn(System.Windows.FrameworkElement element, System.Windows.Media.Visual root)
            => element.TransformToAncestor(root).TransformBounds(new System.Windows.Rect(0, 0, element.ActualWidth, element.ActualHeight));

        [Fact]
        public void TheEditorDoesNotMoveWhenItBecomesModified()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                var view = LaidOut(vm);
                var before = BoundsIn(view.Tabs, view).Top;
                Assert.Equal("Sin cambios que escribir.", view.UpdateBlockedText.Text);

                vm.GuideNowhere();
                Lay(view);
                Assert.True(vm.IsDirty);
                Assert.Equal(string.Empty, view.UpdateBlockedText.Text);
                Assert.Equal(System.Windows.Visibility.Hidden, view.UpdateBlockedText.Visibility);
                Assert.Equal(before, BoundsIn(view.Tabs, view).Top, 3);
            });
        }

        [Fact]
        public void ACalculatedValueKeepsAGapFromItsLabel()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                vm.SelectedTab = 0; // «Calculado» is in the tab Rack, which the tab control lays out only when it is shown
                var view = LaidOut(vm);
                var texts = Descendants<System.Windows.Controls.TextBlock>(view.CalculatedList).ToList();
                var label = texts.First(t => t.Text == "Altura de cabecera");
                var value = texts.First(t => t.Text.EndsWith("(la más alta)", StringComparison.Ordinal));
                Assert.True(BoundsIn(value, view).Left - BoundsIn(label, view).Right >= 8.0,
                    BoundsIn(label, view) + " / " + BoundsIn(value, view));
            });
        }

        [Fact]
        public void TheMarkOfTheGuideIsABadgeInItsOwnColor()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                var view = LaidOut(vm);
                var badges = Descendants<System.Windows.Controls.Border>(view.MatrixRows).Where(b => b.Name == "GuideBadge").ToList();
                Assert.Equal(5, badges.Count(b => b.Visibility == System.Windows.Visibility.Visible)); // F1 with 3 levels, F2 with 2
                var mark = Assert.IsType<System.Windows.Controls.TextBlock>(badges[0].Child);
                Assert.Equal("G", mark.Text);
                Assert.True(mark.FontSize >= 10.0);
                Assert.Equal(System.Windows.FontWeights.Bold, mark.FontWeight);
                Assert.Equal(view.TryFindResource("ShellStatusSuccessBrush"), mark.Foreground);

                vm.GuideNowhere();
                Lay(view);
                Assert.All(badges, b => Assert.Equal(System.Windows.Visibility.Collapsed, b.Visibility));
            });
        }

        [Fact]
        public void TheTabOfTheSafetyGoesBetweenTheMatrixAndAvanzado()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                var view = new DynamicEditorView(vm);
                view.Measure(new System.Windows.Size(1200, 900));
                view.Arrange(new System.Windows.Rect(0, 0, 1200, 900));
                view.UpdateLayout();

                Assert.Equal(2, view.Tabs.Items.IndexOf(view.SafetyTab));
                Assert.Equal(3, view.Tabs.Items.IndexOf(view.AdvancedTab));
                Assert.Equal("Extremos y seguridad", view.SafetyTab.Header);
                Assert.Equal(1, vm.SelectedTab);
            });
        }
    }
}
