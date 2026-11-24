#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D: the Dinámico editor of the RackCad window and its door to the drawing. One way to confirm (a field goes to the
    /// draft when it is committed), one scope (the selection of the matrix), «Varía» when the selection differs, a value that cannot be
    /// recomposed refused in its field with the draft untouched, and the round trip of «Editar aquí» and «Actualizar» through the door.
    /// </summary>
    public class RackCadDynamicEditorTests
    {
        private static readonly SessionId Session = new(7);

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
            return design;
        }

        private static DynamicEditorViewModel Editor(Action<DynamicEditorViewModel>? update = null)
        {
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(Structure(), "DIN 1");
            return new DynamicEditorViewModel(draft, "DIN-1", update);
        }

        private static DynamicFieldItem Field(IReadOnlyList<DynamicFieldItem> fields, string key) => fields.Single(f => f.Key == key);

        private static void Write(DynamicEditorViewModel vm, Func<DynamicEditorViewModel, IReadOnlyList<DynamicFieldItem>> fields, string key, string text)
        {
            var field = Field(fields(vm), key);
            field.Text = text;
            field.Commit();
        }

        // ------------------------------------------------------------------ the editor

        [Fact]
        public void TheEditorOpensOnTheMatrixCleanWithOneDatumPerCellAndTheHighestLevelOnTop()
        {
            var vm = Editor();

            Assert.Equal(1, vm.SelectedTab);
            Assert.Equal("Limpio", vm.StatusText);
            Assert.False(vm.CanUpdate);
            Assert.Equal(new[] { "F1", "F2" }, vm.Columns.Select(c => c.Title).ToArray());
            Assert.Equal(new[] { "N3", "N2", "N1" }, vm.Rows.Select(r => r.Label).ToArray());
            Assert.Equal("42×60", vm.Rows[2].Cells[0].Text);
            // The second front has two levels: its third cell does not exist.
            Assert.False(vm.Rows[0].Cells[1].Exists);
            Assert.Equal("—", vm.Rows[0].Cells[1].Text);
            Assert.Equal("Celda F1 · N1", vm.InspectorTitle);
        }

        [Fact]
        public void TheSelectionIsTheOneScopeClickControlShiftColumnRowAndAll()
        {
            var vm = Editor();
            var matrix = vm.Draft.Matrix;

            vm.ClickCell(1, 1, control: false, shift: false);
            Assert.Equal(1, matrix.SelectedCellCount);
            Assert.Equal("Celda F2 · N2", vm.InspectorTitle);

            vm.ClickCell(0, 2, control: true, shift: false);
            Assert.Equal(2, matrix.SelectedCellCount);

            vm.ClickCell(0, 0, control: false, shift: false);
            vm.ClickCell(1, 1, control: false, shift: true);
            Assert.Equal(4, matrix.SelectedCellCount);

            vm.SelectFront(0);
            Assert.Equal(3, matrix.SelectedCellCount);
            Assert.Equal("3 celdas en 1 frente", vm.InspectorTitle);

            vm.SelectLevel(2);
            Assert.Equal(1, matrix.SelectedCellCount);

            vm.SelectAll();
            Assert.Equal(5, matrix.SelectedCellCount);
            Assert.Equal("5 celdas en 2 frentes", vm.InspectorTitle);
        }

        [Fact]
        public void AFieldOfTheInspectorWritesOnlyTheSelectedCellsAndRecomposes()
        {
            var vm = Editor(_ => { });
            vm.SelectLevel(0);

            Write(vm, v => v.CellFields, "cHeight", "72");

            Assert.True(vm.IsDirty, vm.Message + " | " + Field(vm.CellFields, "cHeight").Error);
            Assert.Equal("Modificado", vm.StatusText);
            Assert.True(vm.CanUpdate);
            var design = vm.Draft.Design;
            Assert.Equal(72.0, design.Fronts[0].Levels[0].PalletHeight);
            Assert.Equal(72.0, design.Fronts[1].Levels[0].PalletHeight);
            Assert.Equal(60.0, design.Fronts[0].Levels[1].PalletHeight);
            Assert.Equal("42×72", vm.Rows.Single(r => r.Label == "N1").Cells[0].Text);
        }

        [Fact]
        public void ASelectionWithDifferentValuesSaysVariesAndOneValueWritesThemAll()
        {
            var vm = Editor();
            vm.SelectLevel(0);
            Write(vm, v => v.CellFields, "cHeight", "72");

            vm.SelectFront(0);
            var height = Field(vm.CellFields, "cHeight");
            Assert.Equal(string.Empty, height.Text);
            Assert.Equal("Varía", height.Hint);

            Write(vm, v => v.CellFields, "cHeight", "66");
            Assert.All(vm.Draft.Design.Fronts[0].Levels, level => Assert.Equal(66.0, level.PalletHeight));
        }

        [Fact]
        public void AValueThatIsNotValidIsRefusedInItsFieldAndTheDraftStaysAsItWas()
        {
            var vm = Editor();
            var before = new RackProjectStore().Serialize(RackProject.ForDynamic(vm.Draft.Design));

            var field = Field(vm.CellFields, "cHeight");
            field.Text = "alto";
            field.Commit();
            Assert.Equal("Revisa los valores de tarima, claro y largueros de cada celda.", field.Error);

            var deep = Field(vm.FrontFields, "fDeep");
            deep.Text = "1";
            deep.Commit();
            Assert.Equal("Debe ser un entero mayor o igual que 2.", deep.Error);

            Assert.False(vm.IsDirty);
            Assert.Equal(before, new RackProjectStore().Serialize(RackProject.ForDynamic(vm.Draft.Design)));
        }

        [Fact]
        public void TheEssentialsWriteTheWholeRackAndTheFrontCountGrowsTheMatrix()
        {
            var vm = Editor();

            Write(vm, v => v.Essentials, "fronts", "3");
            Assert.Equal(3, vm.Draft.Matrix.Count);
            Assert.Equal(3, vm.Draft.Design.Fronts.Count);

            Write(vm, v => v.Essentials, "weight", "1200");
            Assert.All(vm.Draft.Design.Fronts.SelectMany(f => f.Levels), level => Assert.Equal(1200.0, level.PalletWeight));
            Assert.True(vm.IsDirty);
        }

        [Fact]
        public void TheLastActiveFrontCannotBeBlank()
        {
            var vm = Editor();
            vm.SetFrontBlank(1, true);
            Assert.False(vm.Draft.Matrix.IsActive(1));

            vm.SetFrontBlank(0, true);

            Assert.True(vm.Draft.Matrix.IsActive(0));
            Assert.Equal("Al menos un frente debe permanecer activo.", vm.Message);
            Assert.True(vm.MessageIsError);
        }

        [Fact]
        public void DiscardGoesBackToTheOriginalAndUpdateOnlyRunsWithChanges()
        {
            var updates = 0;
            var vm = Editor(_ => updates++);

            vm.RequestWrite();
            Assert.Equal(0, updates);
            Assert.Equal("Sin cambios que escribir.", vm.Message);

            Write(vm, v => v.Essentials, "weight", "1200");
            vm.RequestWrite();
            Assert.Equal(1, updates);

            vm.Discard(Structure(), "DIN 1");
            Assert.False(vm.IsDirty);
            Assert.All(vm.Draft.Design.Fronts.SelectMany(f => f.Levels), level => Assert.Equal(1000.0, level.PalletWeight));

            vm.MarkWritten("ok");
            Assert.Equal("ok", vm.Message);
        }

        // ------------------------------------------------------------------ the door in the window

        private sealed class DoorDouble : IRackCadRackDoorPort
        {
            public List<string> Reads { get; } = new();

            public List<(string RackId, string Name, DynamicRackDesign Design)> Writes { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public RackCadClassicResult RequestDesign(string rackId)
            {
                Reads.Add(rackId);
                return Answer;
            }

            public RackCadClassicResult RequestDynamicUpdate(string rackId, string name, DynamicRackDesign design, DynamicRackSystem system)
            {
                Writes.Add((rackId, name, design));
                return Answer;
            }

            public void Read(RackDesignRead read) => DesignRead?.Invoke(read);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);
        }

        private sealed class Rig
        {
            public Rig()
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[]
                    {
                        new BrowserRackListEntry("DIN-1", "DIN 1", "dynamic", new[] { "lateral" }),
                        new BrowserRackListEntry("PB-1", "PB 1", "pushback", new[] { "lateral" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, Door, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public DoorDouble Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public RackListItem Item(string rack) => Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == rack);
        }

        private static string Json(DynamicRackDesign design) => new RackProjectStore().Serialize(RackProject.ForDynamic(design));

        // What the drawing holds: a design recomposed by the editor (the raw sample has no module sequence and is not a usable project).
        private static string FullJson()
        {
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(Structure(), "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            return Json(draft.Design);
        }

        [Fact]
        public void ADynamicRackOffersToBeEditedHereAndAnotherSystemDoesNot()
        {
            var rig = new Rig();

            rig.Window.OpenRack("PB-1");
            Assert.False(rig.Window.IsDynamicRack);
            Assert.False(rig.Window.ShowDynamicOffer);

            rig.Window.OpenRack("DIN-1");
            Assert.True(rig.Window.IsDynamicRack);
            Assert.True(rig.Window.ShowDynamicOffer);
            Assert.False(rig.Window.HasDynamicEditor);
            Assert.True(rig.Window.ShowPanel);
        }

        [Fact]
        public void EditHereAsksAutoCadForTheDesignAndTheAnswerOpensTheEditorInTheWindow()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");

            rig.Window.EditDynamicHere();
            Assert.Equal(new[] { "DIN-1" }, rig.Door.Reads);
            Assert.Equal("Leyendo el rack en AutoCAD…", rig.Window.ActionMessage);

            rig.Door.Read(new RackDesignRead("DIN-1", true, "dynamic", "DIN 1", FullJson(), null));

            Assert.True(rig.Window.ActiveDynamicEditor != null, rig.Window.ActionMessage);
            var editor = Assert.IsType<DynamicEditorViewModel>(rig.Window.ActiveDynamicEditor);
            Assert.True(rig.Window.HasDynamicEditor);
            Assert.False(rig.Window.ShowDynamicOffer);
            Assert.False(rig.Window.ShowPanel);
            Assert.Equal("DIN 1", editor.Name);
            Assert.Equal(("Abierto en la ventana", "Limpio"), (rig.Item("DIN-1").StatusText, rig.Item("DIN-1").StatusKind));

            rig.Window.ShowPanel = true;
            Assert.True(rig.Window.ShowPanel);
            Assert.Equal("Volver al editor", rig.Window.PanelToggleText);
        }

        [Fact]
        public void AnAnswerForAnotherRackOrARackThatIsNotThereOpensNothing()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();

            rig.Door.Read(new RackDesignRead("PB-1", true, "pushback", "PB 1", "{}", null));
            Assert.Null(rig.Window.ActiveDynamicEditor);

            rig.Door.Read(new RackDesignRead("DIN-1", false, null, null, null, "El rack no está en el dibujo."));
            Assert.Null(rig.Window.ActiveDynamicEditor);
            Assert.Equal("No se pudo abrir el rack: El rack no está en el dibujo.", rig.Window.ActionMessage);
        }

        [Fact]
        public void TheDraftSurvivesAChangeOfRackAndItsStateShowsInTheList()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            rig.Door.Read(new RackDesignRead("DIN-1", true, "dynamic", "DIN 1", FullJson(), null));
            var editor = rig.Window.ActiveDynamicEditor!;
            Write(editor, v => v.Essentials, "weight", "1200");

            Assert.Equal(("Modificado (editor de la ventana)", "Modificado"), (rig.Item("DIN-1").StatusText, rig.Item("DIN-1").StatusKind));
            Assert.Equal(1, rig.Window.RackList.ChangedCount);

            rig.Window.OpenRack("PB-1");
            Assert.Null(rig.Window.ActiveDynamicEditor);
            rig.Window.OpenRack("DIN-1");
            Assert.Same(editor, rig.Window.ActiveDynamicEditor);
            Assert.True(editor.IsDirty);
        }

        [Fact]
        public void UpdateHandsTheDraftToAutoCadAndTheAnswerMakesItTheDrawing()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            rig.Door.Read(new RackDesignRead("DIN-1", true, "dynamic", "DIN 1", FullJson(), null));
            var editor = rig.Window.ActiveDynamicEditor!;
            Write(editor, v => v.Essentials, "weight", "1200");

            editor.RequestWrite();

            var write = Assert.Single(rig.Door.Writes);
            Assert.Equal("DIN-1", write.RackId);
            Assert.Same(editor.Draft.Design, write.Design);
            Assert.Equal("Escribiendo el rack en el dibujo…", editor.Message);

            rig.Door.Written(new RackDesignWritten("DIN-1", true, "Escrito."));
            Assert.False(editor.IsDirty);
            Assert.Equal("Escrito.", editor.Message);
            Assert.Equal(("Abierto en la ventana", "Limpio"), (rig.Item("DIN-1").StatusText, rig.Item("DIN-1").StatusKind));

            // «Descartar» now goes back to what was written.
            var original = rig.Window.OriginalDesignOf(editor)!;
            Assert.All(original.Fronts.SelectMany(f => f.Levels), level => Assert.Equal(1200.0, level.PalletWeight));
        }

        [Fact]
        public void AFailedWriteKeepsTheDraftAndSaysWhy()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            rig.Door.Read(new RackDesignRead("DIN-1", true, "dynamic", "DIN 1", FullJson(), null));
            var editor = rig.Window.ActiveDynamicEditor!;
            Write(editor, v => v.Essentials, "weight", "1200");
            editor.RequestWrite();

            rig.Door.Written(new RackDesignWritten("DIN-1", false, "El rack ya no está en el dibujo: no se escribió nada."));

            Assert.True(editor.IsDirty);
            Assert.True(editor.MessageIsError);
        }

        [Fact]
        public void ABusyAutoCadRefusesTheReadAndTheWrite()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "x");

            rig.Window.EditDynamicHere();

            Assert.True(rig.Window.ActionFailed);
            Assert.StartsWith("Hay un comando en curso en AutoCAD", rig.Window.ActionMessage);
        }
        // ------------------------------------------------------------------ the views

        private static IEnumerable<T> Descendants<T>(System.Windows.DependencyObject root)
            where T : System.Windows.DependencyObject
        {
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        private static void Lay(System.Windows.FrameworkElement view)
        {
            view.Measure(new System.Windows.Size(1200, 900));
            view.Arrange(new System.Windows.Rect(0, 0, 1200, 900));
            view.UpdateLayout();
        }

        private static void Click(System.Windows.Controls.Button button) =>
            button.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        [Fact]
        public void ThePageOfTheRackHostsTheDynamicEditorAndTheUserCanLookAtThePanelInstead()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenRack("DIN-1");
                Lay(view);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Assert.Equal(System.Windows.Visibility.Visible, view.DynamicOfferPanel.Visibility);

                Click(view.EditDynamicHereButton);
                rig.Door.Read(new RackDesignRead("DIN-1", true, "dynamic", "DIN 1", FullJson(), null));
                Lay(view);

                var editorView = Assert.IsType<DynamicEditorView>(view.RackPageHost.Content);
                Assert.Same(rig.Window.ActiveDynamicEditor, editorView.ViewModel);
                Assert.Equal(System.Windows.Visibility.Collapsed, view.DynamicOfferPanel.Visibility);

                Click(view.PanelToggleButton);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Click(view.PanelToggleButton);
                Assert.Same(editorView, view.RackPageHost.Content);
            });
        }

        [Fact]
        public void TheViewOfTheEditorSelectsWithItsButtons()
        {
            StaTestRunner.Run(() =>
            {
                var vm = Editor();
                var view = new DynamicEditorView(vm);
                Lay(view);

                Click(view.SelectAllButton);
                Assert.Equal(5, vm.Draft.Matrix.SelectedCellCount);
                Lay(view);

                var cell = Descendants<System.Windows.Controls.Button>(view.MatrixRows)
                    .First(b => b.DataContext is DynamicCellItem c && c.FrontIndex == 1 && c.LevelIndex == 1);
                Click(cell);
                Assert.Equal(1, vm.Draft.Matrix.SelectedCellCount);
                Assert.Equal("Celda F2 · N2", view.InspectorTitle.Text);
            });
        }
    }
}