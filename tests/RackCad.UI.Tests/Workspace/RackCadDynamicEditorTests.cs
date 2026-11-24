#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
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
    /// draft when it is committed, and a commit that changes nothing is nothing), one scope (the selection of the matrix, with Mayús from
    /// the anchor), «Varía» when the selection differs, a value that cannot be recomposed refused in its field with the draft, the matrix
    /// and the selection untouched, the fronts in blank left alone, and the round trip of «Editar aquí» and «Actualizar» through the door:
    /// numbered requests, copies of the draft, the editor locked while AutoCAD writes, and each answer for its own session and rack.
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
            // The design of the drawing: a recomposed one, as RACKDINAMICOABRIR reads it.
            draft.LoadFrom(new RackProjectStore().Deserialize(FullJson())!.DynamicDesign, "DIN 1");
            return new DynamicEditorViewModel(draft, "DIN-1", update);
        }

        // An editor opened over a design that already has the edit (as if the drawing had it): the edit is its baseline.
        private static DynamicEditorViewModel Reloaded(Action<DynamicEditorViewModel> edit)
        {
            var first = Editor();
            edit(first);
            Assert.True(first.IsDirty, first.Message);
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(new RackProjectStore().Deserialize(Json(first.Draft.Design))!.DynamicDesign, "DIN 1");
            return new DynamicEditorViewModel(draft, "DIN-1", _ => { });
        }

        private static DynamicFieldItem Field(IReadOnlyList<DynamicFieldItem> fields, string key) => fields.Single(f => f.Key == key);

        private static void Write(DynamicEditorViewModel vm, Func<DynamicEditorViewModel, IReadOnlyList<DynamicFieldItem>> fields, string key, string text)
        {
            var field = Field(fields(vm), key);
            field.Text = text;
            field.Commit();
        }

        private static string Json(DynamicRackDesign design) => new RackProjectStore().Serialize(RackProject.ForDynamic(design));

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
        public void TwoShiftClicksInARowExtendFromTheAnchorNotFromTheLastClick()
        {
            // M-13 of the second review: the anchor is the last plain click, not the primary cell that every Mayús+clic moves.
            var vm = Editor();
            var matrix = vm.Draft.Matrix;

            vm.ClickCell(0, 0, control: false, shift: false);
            vm.ClickCell(1, 1, control: false, shift: true);
            Assert.Equal(4, matrix.SelectedCellCount);

            vm.ClickCell(0, 1, control: false, shift: true);

            Assert.Equal(2, matrix.SelectedCellCount);
            Assert.True(matrix.IsSelected(0, 0));
            Assert.True(matrix.IsSelected(0, 1));
            Assert.False(matrix.IsSelected(1, 1));
            Assert.Equal((0, 1), (matrix.SelectedFrontIndex, matrix.SelectedLevelIndex));
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
            var before = Json(vm.Draft.Design);

            var field = Field(vm.CellFields, "cHeight");
            field.Text = "alto";
            field.Commit();
            Assert.Equal("Revisa los valores de tarima, claro y largueros de cada celda.", field.Error);

            var deep = Field(vm.FrontFields, "fDeep");
            deep.Text = "1";
            deep.Commit();
            Assert.Equal("Debe ser un entero mayor o igual que 2.", deep.Error);

            Assert.False(vm.IsDirty);
            Assert.Equal(before, Json(vm.Draft.Design));
        }

        [Fact]
        public void AValueThatPassesItsFieldButCannotBeRecomposedIsRolledBackWithTheMatrixAndTheSelection()
        {
            // M-14 and M-17 of the second review: the value is a valid integer, the recomposition refuses it (no front would start at 1).
            var vm = Editor();
            var before = Json(vm.Draft.Design);
            vm.SelectAll();
            var primary = (vm.Draft.Matrix.SelectedFrontIndex, vm.Draft.Matrix.SelectedLevelIndex);

            var start = Field(vm.FrontFields, "fStart");
            start.Text = "2";
            start.Commit();

            Assert.Equal("Al menos un frente debe comenzar en la posición de fondo 1.", start.Error);
            Assert.Same(start, Field(vm.FrontFields, "fStart"));
            Assert.False(vm.IsDirty);
            Assert.Equal(before, Json(vm.Draft.Design));
            Assert.Equal(new[] { 1, 3 }, vm.Draft.Matrix.Fronts.Select(f => f.DepthStartPosition).ToArray());
            Assert.Equal(5, vm.Draft.Matrix.SelectedCellCount);
            Assert.Equal(primary, (vm.Draft.Matrix.SelectedFrontIndex, vm.Draft.Matrix.SelectedLevelIndex));
            Assert.NotNull(vm.Draft.System);
        }

        [Fact]
        public void LeavingAFieldWithoutChangingItChangesNothingAndAnEmptyVariesIsNotAnError()
        {
            // I-04 of the second review: every field committed as it was, over a design opened from the drawing.
            var vm = Reloaded(v => { v.SelectLevel(0); Write(v, x => x.CellFields, "cHeight", "72"); });
            var before = Json(vm.Draft.Design);

            foreach (var field in vm.Essentials.Concat(vm.PostFields).Concat(vm.AdvancedFields).Concat(vm.CellFields).Concat(vm.FrontFields).ToList())
            {
                field.Commit();
                Assert.Equal(string.Empty, field.Error);
            }

            vm.SelectFront(0);
            var height = Field(vm.CellFields, "cHeight");
            Assert.Equal(("", "Varía"), (height.Text, height.Hint));
            height.Commit();

            Assert.Equal(string.Empty, height.Error);
            Assert.False(vm.IsDirty);
            Assert.False(vm.CanUpdate);
            Assert.Equal(before, Json(vm.Draft.Design));
        }

        [Fact]
        public void ARefusedBlankFrontLeavesTheDraftClean()
        {
            // M-09 of the second review: the second front is already blank in the drawing; the first cannot be blank too.
            var vm = Reloaded(v => v.SetFrontBlank(1, true));

            vm.SetFrontBlank(0, true);

            Assert.True(vm.Draft.Matrix.IsActive(0));
            Assert.Equal("Al menos un frente debe permanecer activo.", vm.Message);
            Assert.True(vm.MessageIsError);
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void AFrontInBlankKeepsItsCellsWhileTheRackIsEditedAndCannotBeEditedFromTheInspector()
        {
            // M-12 of the second review, the rule of the classic editor (I-33): the dormant cells keep what they will have when active again.
            var vm = Editor();
            vm.SetFrontBlank(1, true);
            vm.SelectAll();

            Write(vm, v => v.Essentials, "weight", "777");
            Assert.All(vm.Draft.Matrix.Fronts[0].Cells.Take(3), cell => Assert.Equal(777.0, cell.PalletWeight));
            Assert.All(vm.Draft.Matrix.Fronts[1].Cells.Take(2), cell => Assert.Equal(1000.0, cell.PalletWeight));

            vm.SelectFront(1);
            Assert.All(vm.CellFields, field => Assert.True(field.IsReadOnly, field.Key));
            Assert.True(Field(vm.FrontFields, "fLevels").IsReadOnly);
            Assert.False(Field(vm.FrontFields, "fDeep").IsReadOnly);
            Assert.StartsWith("F2 está en blanco", vm.InspectorNote);

            vm.SetFrontBlank(1, false);
            Assert.All(vm.Draft.Matrix.Fronts[1].Cells.Take(2), cell => Assert.Equal(1000.0, cell.PalletWeight));
            Assert.Equal(string.Empty, vm.InspectorNote);
        }

        [Fact]
        public void APeralteThatTheCatalogDoesNotOfferIsRefusedInItsField()
        {
            // M-11 of the second review: the peraltes of a beam are the ones of its catalog, as the lists of the classic editor offer them.
            var vm = Editor();
            var catalog = vm.Draft.Catalog;
            var cell = vm.Draft.Matrix.Fronts[0].Cells[0];
            var allowed = DynamicRackLevelGeometry.AllowedPeraltes(catalog, cell.InOutBeamCatalogId);
            if (allowed.Count == 0) allowed = new[] { DynamicRackDefaults.DefaultBeamDepth };
            var before = Json(vm.Draft.Design);

            var peralte = Field(vm.CellFields, "cInOut");
            peralte.Text = (allowed.Max() + 0.5).ToString(System.Globalization.CultureInfo.InvariantCulture);
            peralte.Commit();

            Assert.StartsWith("El peralte debe ser uno del catálogo: ", peralte.Error);
            Assert.Equal(before, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void ACommitKeepsTheFieldsAndTheCellsSoTheFocusAndTheClickSurviveIt()
        {
            // I-05 of the second review: the items are refreshed in place, not replaced.
            var vm = Editor();
            var cell = vm.Rows[1].Cells[1];
            var height = Field(vm.CellFields, "cHeight");
            var weight = Field(vm.Essentials, "weight");
            var rows = vm.Rows;

            Write(vm, v => v.CellFields, "cHeight", "72");
            Write(vm, v => v.Essentials, "weight", "1200");

            Assert.Same(rows, vm.Rows);
            Assert.Same(cell, vm.Rows[1].Cells[1]);
            Assert.Same(height, Field(vm.CellFields, "cHeight"));
            Assert.Same(weight, Field(vm.Essentials, "weight"));
            Assert.Equal("72", height.Text);
            Assert.Equal("42×72", vm.Rows[2].Cells[0].Text);

            // A change of the shape (a third front) gives new rows, as it must.
            Write(vm, v => v.Essentials, "fronts", "3");
            Assert.Equal(3, vm.Columns.Count);
            Assert.Equal(3, vm.Rows[0].Cells.Count);
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
        public void DiscardGoesBackToWhatWasReadAndUpdateOnlyRunsWithChanges()
        {
            var updates = 0;
            var vm = Editor(_ => updates++);

            vm.RequestWrite();
            Assert.Equal(0, updates);
            Assert.Equal("Sin cambios que escribir.", vm.Message);

            Write(vm, v => v.Essentials, "weight", "1200");
            vm.Name = "DIN 1 nuevo";
            vm.RequestWrite();
            Assert.Equal(1, updates);

            vm.Discard();
            Assert.False(vm.IsDirty);
            Assert.Equal("DIN 1", vm.Name);
            Assert.All(vm.Draft.Design.Fronts.SelectMany(f => f.Levels), level => Assert.Equal(1000.0, level.PalletWeight));
            Assert.StartsWith("Borrador descartado: se muestra lo último que se leyó", vm.Message);
        }

        [Fact]
        public void ANameWithOnlyTrailingSpacesIsNotAChange()
        {
            var vm = Editor();
            vm.Name = "DIN 1  ";
            Assert.False(vm.IsDirty);
            vm.Name = "DIN 2";
            Assert.True(vm.IsDirty);
        }

        [Fact]
        public void ClosingTheEditorAsksFirstOnlyWhenTheDraftHasChanges()
        {
            var vm = Editor();
            var closed = 0;
            vm.CloseRequested += _ => closed++;

            vm.RequestClose();
            Assert.Equal(1, closed);

            Write(vm, v => v.Essentials, "weight", "1200");
            vm.RequestClose();
            Assert.Equal(1, closed);
            Assert.True(vm.IsConfirmingClose);

            vm.CancelClose();
            Assert.False(vm.IsConfirmingClose);
            Assert.Equal(1, closed);

            vm.RequestClose();
            vm.ConfirmClose();
            Assert.Equal(2, closed);
        }

        // ------------------------------------------------------------------ the door in the window

        private sealed class DoorDouble : IRackCadRackDoorPort
        {
            public List<RackDoorRequest> Reads { get; } = new();

            public List<RackDoorWrite> Writes { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            /// <summary>What AutoCAD does DURING the call, as if it ran the command at once (M-22 of the third review).</summary>
            public Action<RackDoorRequest>? DuringRead { get; set; }

            public Action<RackDoorWrite>? DuringWrite { get; set; }

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                if (Answer.IsStarted) DuringRead?.Invoke(request);
                return Answer;
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write)
            {
                Writes.Add(write);
                if (Answer.IsStarted) DuringWrite?.Invoke(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Read(RackDesignRead read) => DesignRead?.Invoke(read);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            // What AutoCAD answers to the last read: the design of the drawing.
            public void AnswerLastRead(string json, string name = "DIN 1")
            {
                var request = Reads.Last();
                Read(new RackDesignRead(request.RequestId, request.RackId, true, "dynamic", name, json, null));
            }

            public void AnswerLastWrite(bool written, string message)
            {
                var write = Writes.Last();
                Written(new RackDesignWritten(write.RequestId, write.RackId, written, message));
            }
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

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            // «Editar aquí» on DIN-1 and the answer of AutoCAD with the design of the drawing.
            public DynamicEditorViewModel OpenEditor()
            {
                Window.OpenRack("DIN-1");
                Window.EditDynamicHere();
                Door.AnswerLastRead(FullJson());
                Assert.True(Window.ActiveDynamicEditor != null, Window.ActionMessage);
                return Window.ActiveDynamicEditor!;
            }
        }

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
        public void EditHereAsksAutoCadForTheDesignAndTheAnswerToThatRequestOpensTheEditorInTheWindow()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");

            rig.Window.EditDynamicHere();
            var request = Assert.Single(rig.Door.Reads);
            Assert.Equal("DIN-1", request.RackId);
            Assert.Equal("Leyendo el rack en AutoCAD…", rig.Window.ActionMessage);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            // An answer with another number is not for this request.
            rig.Door.Read(new RackDesignRead(request.RequestId + 99, "DIN-1", true, "dynamic", "DIN 1", FullJson(), null));
            Assert.Null(rig.Window.ActiveDynamicEditor);

            rig.Door.AnswerLastRead(FullJson());

            var editor = Assert.IsType<DynamicEditorViewModel>(rig.Window.ActiveDynamicEditor);
            Assert.False(rig.Window.IsWaitingForAutoCad);
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
        public void EachRequestCarriesTheSessionItIsFor()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Assert.Equal(Session, rig.Door.Reads.Single().Session);

            Write(editor, v => v.Essentials, "weight", "1200");
            editor.RequestWrite();
            Assert.Equal(Session, rig.Door.Writes.Single().Session);
        }

        [Fact]
        public void AnAnswerThatArrivesDuringTheCallIsTheAnswerOfTheRequest()
        {
            // M-22 of the third review: the request waits before it is handed over.
            var rig = new Rig();
            var json = FullJson();
            rig.Door.DuringRead = request => rig.Door.Read(new RackDesignRead(request.RequestId, request.RackId, true, "dynamic", "DIN 1", json, null));
            rig.Door.DuringWrite = write => rig.Door.Written(new RackDesignWritten(write.RequestId, write.RackId, true, "Escrito al momento."));

            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();

            var editor = Assert.IsType<DynamicEditorViewModel>(rig.Window.ActiveDynamicEditor);
            Assert.False(rig.Window.IsWaitingForAutoCad);

            Write(editor, v => v.Essentials, "weight", "1200");
            editor.RequestWrite();

            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.False(editor.IsWriting);
            Assert.False(editor.IsDirty);
            Assert.Equal("Escrito al momento.", editor.Message);
        }

        [Fact]
        public void AValueRefusedByTheRecompositionDoesNotTurnOffUpdate()
        {
            // I-09 of the third review: the sequence of the probe S12.
            var updates = 0;
            var vm = Editor(_ => updates++);
            Write(vm, v => v.Essentials, "weight", "1200");
            Assert.True(vm.CanUpdate);

            vm.SelectAll();
            Write(vm, v => v.FrontFields, "fStart", "2");
            Assert.Equal("Al menos un frente debe comenzar en la posición de fondo 1.", Field(vm.FrontFields, "fStart").Error);

            Assert.True(vm.IsDirty);
            Assert.True(vm.CanUpdate, vm.UpdateBlockedText);
            Assert.Equal(string.Empty, vm.UpdateBlockedText);
            vm.RequestWrite();
            Assert.Equal(1, updates);
        }

        public void AReadWaitingForADrawingThatWasClosedIsDroppedAndTheDoorIsTold()
        {
            // M-23 of the third review: without any draft, the waiting read of a closed drawing no longer blocks «Editar aquí».
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            var request = rig.Door.Reads.Single();

            rig.Summary.Dead.Add(Session);
            rig.Present(8);

            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal(new[] { request.RequestId }, rig.Door.Abandoned);
            Assert.Equal("Se cerró el dibujo del rack que se estaba leyendo: no se abrió nada.", rig.Window.ActionMessage);

            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            Assert.Equal(2, rig.Door.Reads.Count);
        }

        [Fact]
        public void AWriteWaitingForADrawingThatWasClosedIsAbandonedWithItsDraft()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor, v => v.Essentials, "weight", "1200");
            editor.RequestWrite();
            var write = rig.Door.Writes.Single();

            rig.Summary.Dead.Add(Session);
            rig.Present(8);

            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal(new[] { write.RequestId }, rig.Door.Abandoned);
            Assert.Empty(rig.Window.DynamicEditors);
            Assert.StartsWith("Se cerró el dibujo de DIN 1 con cambios sin escribir", rig.Window.ActionMessage);
        }

        [Fact]
        public void ARackThatIsNotThereOpensNothingAndSaysWhy()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            var request = rig.Door.Reads.Last();

            rig.Door.Read(new RackDesignRead(request.RequestId, "DIN-1", false, null, null, null, "El rack no está en el dibujo."));

            Assert.Null(rig.Window.ActiveDynamicEditor);
            Assert.Equal("No se pudo abrir el rack: El rack no está en el dibujo.", rig.Window.ActionMessage);
            Assert.False(rig.Window.IsWaitingForAutoCad);
        }

        [Fact]
        public void OneRequestWaitsAtATimeAndStoppingTheWaitDropsItSoALateAnswerOpensNothing()
        {
            // I-07 of the second review: a read whose command never runs (an Esc, text half written on the command line) no longer hangs.
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            var request = rig.Door.Reads.Last();

            rig.Window.EditDynamicHere();
            Assert.Single(rig.Door.Reads);
            Assert.StartsWith("AutoCAD todavía no respondió", rig.Window.ActionMessage);

            rig.Window.StopWaiting();
            Assert.Equal(new[] { request.RequestId }, rig.Door.Abandoned);
            Assert.False(rig.Window.IsWaitingForAutoCad);

            rig.Door.Read(new RackDesignRead(request.RequestId, "DIN-1", true, "dynamic", "DIN 1", FullJson(), null));
            Assert.Null(rig.Window.ActiveDynamicEditor);

            rig.Window.EditDynamicHere();
            Assert.Equal(2, rig.Door.Reads.Count);
            Assert.NotEqual(request.RequestId, rig.Door.Reads.Last().RequestId);
        }

        [Fact]
        public void AnAnswerArrivingAfterTheDrawingChangedOpensTheEditorForTheDrawingOfTheRequest()
        {
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();

            rig.Present(8);
            rig.Window.OpenRack("DIN-1");
            rig.Door.AnswerLastRead(FullJson());
            Assert.Null(rig.Window.ActiveDynamicEditor);

            rig.Present(7);
            rig.Window.OpenRack("DIN-1");
            Assert.NotNull(rig.Window.ActiveDynamicEditor);
        }

        [Fact]
        public void TheDraftSurvivesAChangeOfRackAndItsStateShowsInTheList()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
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
        public void UpdateSendsCopiesLocksTheEditorAndTheAnswerToThatRequestMakesWhatWasSentTheDrawing()
        {
            // B-02 of the second review: what AutoCAD writes is what was sent, and nothing can be edited while it writes.
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor, v => v.Essentials, "weight", "1200");
            editor.Name = "DIN 1 nuevo ";

            editor.RequestWrite();

            var write = Assert.Single(rig.Door.Writes);
            Assert.Equal("DIN-1", write.RackId);
            Assert.Equal("DIN 1 nuevo", write.Name);
            Assert.NotSame(editor.Draft.Design, write.Design);
            Assert.NotSame(editor.Draft.System, write.System);
            Assert.Equal(Json(editor.Draft.Design), Json(write.Design));
            Assert.Equal("DIN 1 nuevo", write.System.Name);
            Assert.Equal("Escribiendo el rack en el dibujo…", editor.Message);
            Assert.True(editor.IsWriting);
            Assert.False(editor.CanUpdate);
            Assert.Equal(("Escribiendo en el dibujo", "Modificado"), (rig.Item("DIN-1").StatusText, rig.Item("DIN-1").StatusKind));

            // While AutoCAD writes, the editor cannot be edited: the fields are read-only and a commit does nothing.
            var weight = Field(editor.Essentials, "weight");
            Assert.True(weight.IsReadOnly);
            weight.Text = "1300";
            weight.Commit();
            editor.SetFrontBlank(1, true);
            Assert.All(write.Design.Fronts.SelectMany(f => f.Levels), level => Assert.Equal(1200.0, level.PalletWeight));
            Assert.All(editor.Draft.Design.Fronts.SelectMany(f => f.Levels), level => Assert.Equal(1200.0, level.PalletWeight));
            Assert.True(editor.Draft.Matrix.IsActive(1));

            // An answer with another number does not end the wait.
            rig.Door.Written(new RackDesignWritten(write.RequestId + 99, "DIN-1", true, "otro"));
            Assert.True(editor.IsWriting);

            rig.Door.AnswerLastWrite(true, "Escrito.");
            Assert.False(editor.IsWriting);
            Assert.False(editor.IsDirty);
            Assert.Equal("Escrito.", editor.Message);
            Assert.Equal(("Abierto en la ventana", "Limpio"), (rig.Item("DIN-1").StatusText, rig.Item("DIN-1").StatusKind));

            // «Descartar» now goes back to what was written.
            Write(editor, v => v.Essentials, "weight", "1500");
            editor.Discard();
            Assert.All(editor.Draft.Design.Fronts.SelectMany(f => f.Levels), level => Assert.Equal(1200.0, level.PalletWeight));
            Assert.Equal("DIN 1 nuevo", editor.Name);
        }

        [Fact]
        public void StoppingTheWaitOfAWriteUnlocksTheEditorKeepsItsChangesAndALateAnswerDoesNotMarkItWritten()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor, v => v.Essentials, "weight", "1200");
            editor.RequestWrite();
            var write = rig.Door.Writes.Last();

            rig.Window.StopWaiting();

            Assert.Equal(new[] { write.RequestId }, rig.Door.Abandoned);
            Assert.False(editor.IsWriting);
            Assert.True(editor.IsDirty);
            Assert.StartsWith("Se dejó de esperar a AutoCAD", editor.Message);

            rig.Door.AnswerLastWrite(true, "Escrito tarde.");
            Assert.True(editor.IsDirty);
            Assert.NotEqual("Escrito tarde.", editor.Message);
        }

        [Fact]
        public void TheAnswerOfAWriteIsForTheDraftOfItsSessionNotForTheSameRackInAnotherDrawing()
        {
            // P4 of the second review: two drawings with the same GUID (a «Guardar como», a rack copied between drawings).
            var rig = new Rig();
            var seven = rig.OpenEditor();
            Write(seven, v => v.Essentials, "weight", "1100");

            rig.Present(8);
            var eight = rig.OpenEditor();
            Assert.NotSame(seven, eight);
            Write(eight, v => v.Essentials, "weight", "1200");
            eight.RequestWrite();
            rig.Door.AnswerLastWrite(true, "Escrito en el dibujo 8.");

            Assert.False(eight.IsDirty);
            Assert.True(seven.IsDirty);
            Assert.NotEqual("Escrito en el dibujo 8.", seven.Message);
        }

        [Fact]
        public void AFailedWriteKeepsTheDraftAndSaysWhy()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor, v => v.Essentials, "weight", "1200");
            editor.RequestWrite();

            rig.Door.AnswerLastWrite(false, "El rack ya no está en el dibujo: no se escribió nada.");

            Assert.False(editor.IsWriting);
            Assert.True(editor.IsDirty);
            Assert.True(editor.MessageIsError);
        }

        [Fact]
        public void ABusyAutoCadRefusesTheReadAndTheWrite()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor, v => v.Essentials, "weight", "1200");
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "x");

            editor.RequestWrite();
            Assert.False(editor.IsWriting);
            Assert.StartsWith("Hay un comando en curso en AutoCAD", editor.Message);
            Assert.False(rig.Window.IsWaitingForAutoCad);

            rig.Window.OpenRack("PB-1");
            rig.Window.OpenRack("DIN-1");
            editor.RequestClose();
            editor.ConfirmClose();
            rig.Window.EditDynamicHere();
            Assert.True(rig.Window.ActionFailed);
            Assert.StartsWith("Hay un comando en curso en AutoCAD", rig.Window.ActionMessage);
        }

        [Fact]
        public void ClosingTheEditorForgetsTheDraftAndTheRackOffersToBeEditedAgain()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();

            editor.RequestClose();

            Assert.Null(rig.Window.ActiveDynamicEditor);
            Assert.True(rig.Window.ShowDynamicOffer);
            Assert.Empty(rig.Window.DynamicEditors);
            Assert.Equal(("Sin borrador", "Limpio"), (rig.Item("DIN-1").StatusText, rig.Item("DIN-1").StatusKind));
        }

        [Fact]
        public void TheDraftsOfAClosedDrawingAreForgottenAndLostWorkIsSaid()
        {
            // I-06 of the second review, its part without a veto (D-07): the drafts of a drawing that no longer lives are forgotten.
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Write(editor, v => v.Essentials, "weight", "1200");

            rig.Summary.Dead.Add(Session);
            rig.Present(8);

            Assert.Empty(rig.Window.DynamicEditors);
            Assert.True(rig.Window.ActionFailed);
            Assert.Equal("Se cerró el dibujo de DIN 1 con cambios sin escribir en la ventana: esos cambios se perdieron.", rig.Window.ActionMessage);
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
                Lay(view);
                Assert.Equal(System.Windows.Visibility.Visible, view.WaitingPanel.Visibility);
                rig.Door.AnswerLastRead(FullJson());
                Lay(view);

                var editorView = Assert.IsType<DynamicEditorView>(view.RackPageHost.Content);
                Assert.Same(rig.Window.ActiveDynamicEditor, editorView.ViewModel);
                Assert.Equal(System.Windows.Visibility.Collapsed, view.DynamicOfferPanel.Visibility);
                Assert.Equal(System.Windows.Visibility.Collapsed, view.WaitingPanel.Visibility);

                Click(view.PanelToggleButton);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Click(view.PanelToggleButton);
                Assert.Same(editorView, view.RackPageHost.Content);

                // Closing the editor forgets its view too.
                Assert.Equal(1, view.DynamicViewCount);
                Click(editorView.CloseEditorButton);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Assert.Equal(0, view.DynamicViewCount);
            });
        }

        [Fact]
        public void StopWaitingInTheViewDropsTheRequest()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenRack("DIN-1");
                Click(view.EditDynamicHereButton);
                Lay(view);

                Click(view.StopWaitingButton);
                Lay(view);

                Assert.Single(rig.Door.Abandoned);
                Assert.Equal(System.Windows.Visibility.Collapsed, view.WaitingPanel.Visibility);
            });
        }

        [Fact]
        public void TheViewOfTheEditorSelectsWithItsButtonsAndACommitKeepsTheCellButtons()
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

                // A commit refreshes the cells in place: the same button is still in the tree and still clickable (I-05).
                Write(vm, v => v.CellFields, "cHeight", "72");
                Lay(view);
                Assert.Contains(cell, Descendants<System.Windows.Controls.Button>(view.MatrixRows));
                var first = Descendants<System.Windows.Controls.Button>(view.MatrixRows)
                    .First(b => b.DataContext is DynamicCellItem c && c.FrontIndex == 0 && c.LevelIndex == 0);
                Click(first);
                Assert.Equal("Celda F1 · N1", view.InspectorTitle.Text);
            });
        }
    }
}
