#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T5 pb (I-64, fifth batch; the prototype «pushback», all its questions accepted): the editor of a Push Back in the RackCad window and
    /// its numbered door (<see cref="IRackCadPushBackDoorPort"/>), with the rules of the other doors: numbered requests, an answer only for its
    /// request, «Dejar de esperar», the request the command took, copies of the draft, the editor locked while AutoCAD writes, a draft of
    /// another drawing, the prune of a closed drawing and the veto of the close (D-08); the matrix with its one scope and the side over it;
    /// «Nuevo» › «Push Back en esta ventana» as a draft; and the views with real WPF (STA).
    /// </summary>
    public partial class RackCadPushBackEditorTests // T5 pb: partial (RackCadPushBackEditorTests.Seg.cs: PB-S03 and the review)
    {
        private static readonly SessionId Session = new(7);

        private static string Fixture(string name)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            return File.ReadAllText(Path.Combine(dir!.FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json"));
        }

        private static RackEmbedDocument Envelope(string fixture) => new RackEmbedStore().Deserialize(Fixture(fixture))!;

        private sealed class PushBackDoorDouble : IRackCadPushBackDoorPort
        {
            public List<RackDoorRequest> Reads { get; } = new();

            public List<RackDoorRequest> PushBackReads { get; } = new();

            public List<PushBackDoorWrite> Writes { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<RackDesignRead>? PushBackRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => Answer;

            public RackCadClassicResult RequestPushBackDesign(RackDoorRequest request)
            {
                PushBackReads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestPushBackUpdate(PushBackDoorWrite write)
            {
                Writes.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Read(RackDesignRead read) => DesignRead?.Invoke(read);

            public void ReadPushBack(RackDesignRead read) => PushBackRead?.Invoke(read);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            // What RACKPUSHBACKABRIR answers to the last read: the design of the fixture, through the rule of the door.
            public void AnswerLastRead(string fixture = "pb-01-base", string name = "PB 1")
            {
                var request = PushBackReads.Last();
                ReadPushBack(PushBackDoorRules.AnswerForOpen(request.RequestId, request.RackId, RackEmbedDocument.KindPushBack, name, Envelope(fixture).Design));
            }

            public void AnswerLastWrite(bool written, string message, string? name = null)
            {
                var write = Writes.Last();
                Written(new RackDesignWritten(write.RequestId, write.RackId, written, message, name));
            }
        }

        private sealed class Rig
        {
            public Rig(IRackCadRackDoorPort? door = null)
            {
                Door = door as PushBackDoorDouble ?? new PushBackDoorDouble();
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[]
                    {
                        new BrowserRackListEntry("PB-1", "PB 1", RackEmbedDocument.KindPushBack, new[] { "lateral", "planta" }),
                        new BrowserRackListEntry("PB-2", "PB 2", RackEmbedDocument.KindPushBack, new[] { "lateral" }),
                        new BrowserRackListEntry("DIN-1", "DIN 1", "dynamic", new[] { "lateral" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, door ?? Door, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public PushBackDoorDouble Door { get; }

            public RackCadWindowViewModel Window { get; }

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            public PushBackEditorViewModel OpenEditor(string rack = "PB-1", string fixture = "pb-01-base", string name = "PB 1")
            {
                Window.OpenRack(rack);
                Window.EditPushBackHere();
                Door.AnswerLastRead(fixture, name);
                Assert.True(Window.ActivePushBackEditor != null, Window.ActionMessage);
                return Window.ActivePushBackEditor!;
            }
        }

        /// <summary>A door of Dinámico only: it cannot read a Push Back, so the window does not offer to edit one.</summary>
        private sealed class DynamicOnlyDoor : IRackCadRackDoorPort
        {
            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public void Abandon(long requestId)
            {
            }

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten { add { } remove { } }

            public event Action<long>? RequestStarted { add { } remove { } }
        }

        private static DynamicFieldItem Field(IEnumerable<DynamicFieldItem> fields, string key) => fields.Single(f => f.Key == key);

        private static void Write(DynamicFieldItem field, string text)
        {
            field.Text = text;
            field.Commit();
        }

        private static void Change(PushBackEditorViewModel editor) => Write(Field(editor.CellFields, "cFront"), "45");

        // ------------------------------------------------------------------ the door

        [Fact]
        public void APushBackOffersToBeEditedHereOnlyWithItsDoorAndOnlyTheAnswerToThatRequestOpensTheEditor()
        {
            var rig = new Rig();
            rig.Window.OpenRack("PB-1");
            Assert.True(rig.Window.IsPushBackRack);
            Assert.True(rig.Window.ShowPushBackOffer);
            Assert.False(rig.Window.ShowDynamicOffer);

            rig.Window.EditPushBackHere();
            var request = Assert.Single(rig.Door.PushBackReads);
            Assert.Equal("PB-1", request.RackId);
            Assert.Equal(Session, request.Session);
            Assert.Empty(rig.Door.Reads);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            // Another number, or the read of Dinámico with the same number, is not for this request.
            rig.Door.ReadPushBack(PushBackDoorRules.AnswerForOpen(request.RequestId + 9, "PB-1", RackEmbedDocument.KindPushBack, "PB 1", Envelope("pb-01-base").Design));
            rig.Door.Read(new RackDesignRead(request.RequestId, "PB-1", true, "dynamic", "X", "{}", null));
            Assert.Null(rig.Window.ActivePushBackEditor);
            Assert.Empty(rig.Window.DynamicEditors);

            rig.Door.AnswerLastRead();

            var editor = Assert.IsType<PushBackEditorViewModel>(rig.Window.ActivePushBackEditor);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Window.HasRackEditor);
            Assert.False(rig.Window.ShowPushBackOffer);
            Assert.False(rig.Window.ShowPanel);
            Assert.Equal("PB 1", editor.Name);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal(PushBackEditorViewModel.MatrixTabIndex, editor.SelectedTab);

            var dynamicOnly = new Rig(new DynamicOnlyDoor());
            dynamicOnly.Window.OpenRack("PB-1");
            Assert.False(dynamicOnly.Window.ShowPushBackOffer);
            dynamicOnly.Window.EditPushBackHere();
            Assert.False(dynamicOnly.Window.IsWaitingForAutoCad);
        }

        [Fact]
        public void ARackThatTheDoorRefusesSaysWhyAndOpensNothing()
        {
            var rig = new Rig();
            rig.Window.OpenRack("PB-1");
            rig.Window.EditPushBackHere();
            var request = rig.Door.PushBackReads.Last();
            var tail = Envelope("pb-21-cola");
            rig.Door.ReadPushBack(PushBackDoorRules.AnswerForOpen(request.RequestId, "PB-1", tail.Kind, tail.Name, tail.Design));
            Assert.Null(rig.Window.ActivePushBackEditor);
            Assert.True(rig.Window.ActionFailed);
            Assert.Contains("RACKEDITAR", rig.Window.ActionMessage);
        }

        [Fact]
        public void UpdateSendsCopiesLocksTheEditorAndOnlyAWrittenAnswerLeavesItClean()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Change(editor);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.True(editor.CanUpdate, editor.UpdateBlockedText);
            Assert.Contains("PB 1", rig.Window.UnwrittenWorkOf(Session)); // D-08
            Assert.Equal("Modificado (editor de la ventana)", rig.Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "PB-1").StatusText);

            editor.RequestWrite();

            var write = Assert.Single(rig.Door.Writes);
            Assert.Equal("PB-1", write.RackId);
            Assert.Equal(Session, write.Session);
            Assert.Equal("PB 1", write.Name);
            Assert.False(write.NewRack);
            Assert.False(write.Inserts);
            Assert.NotSame(editor.Draft.Design, write.Design);
            Assert.NotSame(editor.Draft.System, write.System);
            Assert.Equal(45.0, write.Design.Structure.Fronts[0].Levels[0].PalletFront);
            Assert.True(editor.IsWriting);
            Assert.False(editor.CanEdit);
            Assert.True(rig.Window.CanStopWaiting);

            // While AutoCAD writes nothing can be edited, and the copy that went does not change.
            Write(Field(editor.CellFields, "cFront"), "50");
            Assert.Equal(45.0, write.Design.Structure.Fronts[0].Levels[0].PalletFront);

            rig.Door.Start(write.RequestId);
            Assert.False(rig.Window.CanStopWaiting);
            Assert.Contains("ya tomó la escritura", editor.UpdateBlockedText);

            rig.Door.AnswerLastWrite(false, "AutoCAD no redibujó ninguna vista del rack.");
            Assert.False(editor.IsWriting);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.True(editor.MessageIsError);

            editor.RequestWrite();
            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó 2 vistas del rack con el borrador.");
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));

            // «Descartar» goes back to what was written, not to what was read.
            Write(Field(editor.CellFields, "cFront"), "52");
            editor.Discard();
            Assert.Equal("45", Field(editor.CellFields, "cFront").Text);
        }

        [Fact]
        public void StopWaitingForAWriteLeavesTheEditorEditableAndNotWrittenAndALateAnswerDoesNothing()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Change(editor);
            editor.RequestWrite();
            var write = rig.Door.Writes.Last();

            rig.Window.StopWaiting();
            Assert.Contains(write.RequestId, rig.Door.Abandoned);
            Assert.False(editor.IsWriting);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.Contains("Se dejó de esperar", editor.Message);

            rig.Door.AnswerLastWrite(true, "tarde");
            Assert.Equal("Modificado", editor.StatusText);
        }

        [Fact]
        public void ADraftOfAnotherDrawingIsNotWrittenInTheActiveOneAndAClosedDrawingForgetsIt()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Change(editor);

            rig.Present(8);
            editor.RequestWrite();
            Assert.Empty(rig.Door.Writes);
            Assert.Equal("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", editor.Message);

            rig.Summary.Dead.Add(Session);
            rig.Present(8);
            Assert.Empty(rig.Window.PushBackEditors);
            Assert.Contains("PB 1", rig.Window.ActionMessage);
        }

        [Fact]
        public void CloseTheEditorAsksWithChangesAndForgetsTheDraft()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Change(editor);
            editor.RequestClose();
            Assert.True(editor.IsConfirmingClose);
            Assert.NotNull(rig.Window.ActivePushBackEditor);
            editor.ConfirmClose();
            Assert.Null(rig.Window.ActivePushBackEditor);
            Assert.True(rig.Window.ShowPushBackOffer);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        // ------------------------------------------------------------------ the editor

        // T6 ui, point 10 (decided by the Owner on 2026-11-27): «Descartar» of a draft of «Nuevo» says the words of every draft.
        [Fact]
        public void DiscardingADraftOfNuevoSaysThatItGoesBackToHowItOpened()
        {
            var rig = new Rig();
            rig.Window.StartNewPushBack();
            var editor = rig.Window.NewPushBackEditor!;
            Write(Field(editor.Essentials, "fronts"), "3");
            Assert.Equal(3, editor.Columns.Count);

            editor.Discard();

            Assert.Single(editor.Columns);
            Assert.Equal("Cambios descartados: el borrador vuelve a como se abrió.", editor.Message);
        }

        [Fact]
        public void TheMatrixSelectsWithItsGesturesAndTheInspectorWritesTheSelection()
        {
            // A new Push Back of «Nuevo»: one front of three levels, as the classic editor opens it.
            var rig = new Rig();
            rig.Window.StartNewPushBack();
            var editor = rig.Window.NewPushBackEditor!;
            Assert.Single(editor.Columns);
            Assert.Equal(3, editor.Rows.Count);
            Write(Field(editor.Essentials, "fronts"), "3");
            Assert.Equal(3, editor.Columns.Count);
            Assert.Equal("Modificado", editor.StatusText);

            editor.ClickCell(0, 0, control: false, shift: false);
            editor.ClickCell(2, 1, control: false, shift: true);
            Assert.Equal("6 celdas en 3 frentes", editor.InspectorTitle);
            Write(Field(editor.CellFields, "cHeight"), "55");
            foreach (var row in editor.Rows.Where(r => r.LevelIndex <= 1))
                Assert.All(row.Cells, c => Assert.Equal("42×55", c.Text));
            Assert.Equal("42×60", editor.Rows.Single(r => r.LevelIndex == 2).Cells[0].Text);

            editor.ClickCell(1, 2, control: true, shift: false);
            Assert.Equal("7 celdas en 3 frentes", editor.InspectorTitle);
            Assert.Equal("Varía", Field(editor.CellFields, "cHeight").Hint);
            editor.ClickCell(1, 2, control: true, shift: false);
            Assert.Equal("6 celdas en 3 frentes", editor.InspectorTitle);

            editor.SelectLevel(2);
            Assert.Equal("3 celdas en 3 frentes", editor.InspectorTitle);
            editor.SelectFront(1);
            Assert.Equal("3 celdas en 1 frente", editor.InspectorTitle);
            Write(Field(editor.FrontFields, "fLevels"), "4");
            Assert.Equal(4, editor.Rows.Count);
            editor.SelectAll();
            Assert.Equal("10 celdas en 3 frentes", editor.InspectorTitle);

            // Leaving a field without changing its text changes nothing.
            var json = editor.Draft.DesignJson();
            Field(editor.CellFields, "cFront").Commit();
            Assert.Equal(json, editor.Draft.DesignJson());
        }

        [Fact]
        public void ARefusedValueStaysInItsFieldAndTheDraftDoesNotChange()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            var json = editor.Draft.DesignJson();

            var fondo = Field(editor.DepthFields, "cFondo");
            Write(fondo, "1");
            Assert.Contains("mayor o igual que 2", fondo.Error);
            var start = Field(editor.FrontFields, "fStart");
            Write(start, "3");
            Assert.StartsWith("No se pudo generar el sistema: ", start.Error);
            Assert.Equal(json, editor.Draft.DesignJson());
            Assert.Equal("Limpio", editor.StatusText);
            Assert.True(string.IsNullOrEmpty(editor.Draft.LastError));

            // The refused value is not kept anywhere: the field shows the value of the draft again and the next gesture recomposes.
            Assert.Equal("1", start.Text);
            Write(Field(editor.CellFields, "cFront"), "45");
            Assert.True(string.IsNullOrEmpty(Field(editor.CellFields, "cFront").Error), Field(editor.CellFields, "cFront").Error);
            Assert.Equal("Modificado", editor.StatusText);
        }

        [Fact]
        public void TheRearTopeIsMarkedInTheMatrixAndWrittenFromTheInspectorAndEveryCellAtOnce()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Assert.All(editor.Rows.SelectMany(r => r.Cells).Where(c => c.Exists), c => Assert.True(c.HasTope));

            editor.ClickCell(0, 0, false, false);
            Assert.True(editor.SelectionTope);
            editor.SelectionTope = false;
            Assert.False(editor.Rows.Single(r => r.LevelIndex == 0).Cells[0].HasTope);
            Assert.Contains("con tope posterior", editor.Rows.Single(r => r.LevelIndex == 1).Cells[0].AccessibleText);
            Assert.Contains(editor.Draft.Design.RearTope.OffCells, c => c.Frente == 0 && c.Level == 0);

            editor.SelectAll();
            Assert.Null(editor.SelectionTope);
            Assert.Contains("varía", editor.SelectionTopeLabel);
            editor.SelectionTope = false; // a mixed selection: a click puts it in every cell
            Assert.True(editor.SelectionTope);

            editor.TopeNowhere();
            Assert.Equal("Tope posterior: en ninguna celda.", editor.TopeSummary);
            editor.TopeEverywhere();
            Assert.StartsWith("Tope posterior: en todas las celdas", editor.TopeSummary);
        }

        [Fact]
        public void InATwoWayRackTheSideOverTheMatrixIsWhereItIsWrittenAndTheBedsAreMarked()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor("PB-2", "pb-20-ab", "PB 2");
            Assert.True(editor.IsTwoSided);
            Assert.True(editor.IsSideA);
            Assert.Single(editor.BedsChoices);

            editor.ClickCell(0, 0, false, false);
            Write(Field(editor.CellFields, "cFront"), "44");
            Assert.Equal(44.0, editor.Draft.Side(PushBackSide.A).Structure.Fronts[0].Cells[0].PalletFront);
            Assert.NotEqual(44.0, editor.Draft.Side(PushBackSide.B).Structure.Fronts[0].Cells[0].PalletFront);

            editor.IsBothSides = true;
            Assert.Equal("Se escribe en los dos lados.", editor.TargetText);
            Assert.Equal("varía", editor.Rows.Single(r => r.LevelIndex == 0).Cells[0].Text);
            Write(Field(editor.CellFields, "cFront"), "46");
            Assert.Equal(46.0, editor.Draft.Side(PushBackSide.B).Structure.Fronts[0].Cells[0].PalletFront);

            var beds = Assert.Single(editor.BedsChoices);
            beds.SelectedValue = nameof(PushBackCellBeds.CorridaAToB);
            Assert.Equal("C", editor.Rows.Single(r => r.LevelIndex == 0).Cells[0].Beds);
            Assert.Contains("cama corrida de A a B", editor.Rows.Single(r => r.LevelIndex == 0).Cells[0].AccessibleText);
            Assert.Equal(PushBackCellBeds.CorridaAToB, editor.Draft.BedsAt(0, 0));

            editor.IsSideB = true;
            Assert.True(editor.IsSideB);
            Assert.Equal("Se escribe en el lado B.", editor.TargetText);
            Assert.Equal(2, editor.StructureRows.Count);
        }

        [Fact]
        public void AModuleLengthGoesToTheDraftAtOnceAndRestoreLayoutGivesBackWhatIsMarked()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            var module = editor.Modules.First(m => m.Kind == "Separador" && m.HasLengthField);
            Write(module.LengthField!, "60");
            Assert.Equal("Modificado", editor.StatusText);
            Assert.Contains("(propia)", editor.Modules.Single(m => m.ModuleId == module.ModuleId).LengthText);
            Write(Field(editor.BeamFields, "cLength"), "100");

            editor.OpenRestore();
            Assert.True(editor.IsRestoreOpen);
            Assert.Equal(6, editor.RestoreBoxes.Count);
            Assert.All(editor.RestoreBoxes, b => Assert.True(b.IsChecked));
            foreach (var box in editor.RestoreBoxes.Where(b => b.Key != "beams")) box.IsChecked = false;
            editor.IsLossOpen = true;
            Assert.Equal(new[] { "1 largo manual de larguero" }, editor.RestoreLoss);
            editor.RestoreChosen();
            Assert.False(editor.IsRestoreOpen);
            Assert.Equal(string.Empty, Field(editor.BeamFields, "cLength").Text);
            Assert.Contains("(propia)", editor.Modules.Single(m => m.ModuleId == module.ModuleId).LengthText);

            editor.OpenRestore();
            editor.RestoreChosen();
            Assert.Equal("Limpio", editor.StatusText);
        }

        // ------------------------------------------------------------------ «Nuevo», the panel of the views, the library and the BOM

        [Fact]
        public void ANewPushBackIsADraftOfNuevoAndIsInsertedWithTheViewsChosenAsANewRack()
        {
            var rig = new Rig();
            Assert.True(rig.Window.CanStartNewPushBack);
            rig.Window.StartNewPushBack();

            var editor = Assert.IsType<PushBackEditorViewModel>(rig.Window.NewPushBackEditor);
            Assert.True(rig.Window.ShowNewPushBackEditor);
            Assert.True(editor.IsNewRack);
            Assert.False(editor.CanUpdate);
            Assert.Equal("Insertar en el dibujo…", editor.InsertViewLabel);
            var draft = Assert.Single(rig.Window.Drafts);
            Assert.Equal("Push Back", draft.SystemLabel);
            Assert.Equal("Borrador 1", draft.Title);

            editor.InsertView();
            var panel = editor.InsertPanel!;
            Assert.NotNull(panel);
            panel.Mark(RackViewAddress.Whole(DimensionViewKind.Planta), RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A));
            panel.Insert();

            var write = Assert.Single(rig.Door.Writes);
            Assert.True(write.NewRack);
            Assert.True(string.IsNullOrEmpty(write.Name)); // «Borrador N» is never sent: the automatic name of I-60
            Assert.Equal(new[] { RackViewAddress.Whole(DimensionViewKind.Planta), RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A) }, write.InsertViews);
            Assert.Null(write.Properties);

            rig.Door.AnswerLastWrite(true, "AutoCAD insertó el rack nuevo «Push Back 1» con 2 vistas.", "Push Back 1");
            Assert.False(editor.IsNewRack);
            Assert.Equal("Push Back 1", editor.Name);
            Assert.Empty(rig.Window.Drafts);
            Assert.Null(rig.Window.NewPushBackEditor);
        }

        [Fact]
        public void InsertViewOfARackOfTheDrawingSendsTheDraftWithTheViewsAndSideBOnlyInATwoWayRack()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Assert.DoesNotContain(editor.InsertOptions(), o => o.Address.Equals(RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.B)));
            editor.InsertView();
            editor.InsertPanel!.Mark(RackViewAddress.Post(0));
            editor.InsertPanel!.Insert();
            var write = Assert.Single(rig.Door.Writes);
            Assert.False(write.NewRack);
            Assert.Equal(new[] { RackViewAddress.Post(0) }, write.InsertViews);

            var twoWay = new Rig().OpenEditor("PB-2", "pb-20-ab", "PB 2");
            Assert.Contains(twoWay.InsertOptions(), o => o.Address.Equals(RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.B)));
        }

        [Fact]
        public void TheLibrarySavesThePushBackAndOpensItAsADraftAndTheBomPageSeesItsDraft()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Assert.True(editor.CanSaveToLibrary, editor.SaveToLibraryBlockedText);
            Change(editor);
            Assert.True(((IRackBomPageHost)rig.Window).HasUnwrittenDraft(Session, "PB-1"));
            Assert.NotNull(((IRackBomPageHost)rig.Window).DraftBomOf(Session, "PB-1"));
            Assert.Contains(((IRackBomPageHost)rig.Window).UnwrittenDraftsOf(Session), d => d.RackId == "PB-1");

            var project = new RackProjectStore().Deserialize(Envelope("pb-03-fondos").Design);
            Assert.True(rig.Window.OpenDesignAsNewRack(project, "Diseño PB"));
            var draft = Assert.IsType<PushBackEditorViewModel>(rig.Window.NewPushBackEditor);
            Assert.Equal("Diseño PB", draft.Name);
            Assert.True(draft.IsNewRack);
            Assert.Contains("de la biblioteca", draft.Message);
        }

        // ------------------------------------------------------------------ the views (real WPF)

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

        private static void WithBindingErrors(Action<BindingErrors> body)
        {
            var errors = new BindingErrors();
            PresentationTraceSources.Refresh();
            var source = PresentationTraceSources.DataBindingSource;
            var level = source.Switch.Level;
            source.Listeners.Add(errors);
            source.Switch.Level = SourceLevels.Error;
            try
            {
                body(errors);
            }
            finally
            {
                source.Listeners.Remove(errors);
                source.Switch.Level = level;
            }
        }

        private static void Lay(FrameworkElement view, double width = 1400, double height = 900)
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

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

        [Fact]
        public void TheEditorViewHasItsFiveTabsUniqueNamesNoTypeNameAndNoBindingError()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var rig = new Rig();
                var editor = rig.OpenEditor("PB-2", "pb-20-ab", "PB 2");
                var view = new PushBackEditorView(editor);
                Lay(view);

                Assert.Equal(new[] { "Rack", "Matriz", "Extremos y seguridad", "Avanzado" },
                    view.Tabs.Items.OfType<TabItem>().Take(4).Select(t => (string)t.Header).ToArray());
                Assert.Equal(5, view.Tabs.Items.Count);
                Assert.Same(view.MatrixTab, view.Tabs.SelectedItem);
                Assert.Equal(Visibility.Visible, view.SidesBar.Visibility);

                // The cells of the matrix: one button each, named by its position, with its marks for the screen reader.
                var cells = Descendants<Button>(view.MatrixRows).Where(b => b.DataContext is PushBackCellItem).ToList();
                Assert.Equal(editor.Rows.Sum(r => r.Cells.Count), cells.Count);
                var cellNames = cells.Select(Name).ToList();
                Assert.Equal(cellNames.Count, cellNames.Distinct().Count());
                Click(cells.First(b => ((PushBackCellItem)b.DataContext).Exists));
                Lay(view);
                Assert.StartsWith("Celda F", view.InspectorTitle.Text);

                Click(view.BothSidesButton);
                Lay(view);
                Assert.True(editor.IsBothSides);

                foreach (TabItem tab in view.Tabs.Items)
                {
                    view.Tabs.SelectedItem = tab;
                    Lay(view);
                    Assert.DoesNotContain(Descendants<TextBlock>(view), t => (t.Text ?? string.Empty).StartsWith("RackCad.", StringComparison.Ordinal)); // F-1
                }

                var named = new UIElement[]
                {
                    view.UpdateButton, view.InsertViewButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.MoreButton,
                    view.SaveToLibraryButton, view.CloseYesButton, view.CloseNoButton, view.RestoreYesButton, view.RestoreNoButton, view.SelectAllButton,
                    view.SideAButton, view.SideBButton, view.BothSidesButton, view.TopeEverywhereButton, view.TopeNowhereButton,
                    view.PalletsEverywhereButton, view.PalletsNowhereButton, view.BlankFrontButton, view.ActivateFrontButton,
                }.Select(Name).ToList();
                Assert.Equal(named.Count, named.Distinct().Count());

                // «Restaurar layout…» opens its boxes inline.
                Click(view.RestoreButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.RestorePanel.Visibility);
                Assert.Equal(6, Descendants<CheckBox>(view.RestoreBoxList).Count());

                // Narrow, the inspector goes below the matrix.
                view.Tabs.SelectedItem = view.MatrixTab;
                Lay(view, 600, 900);
                Assert.True(view.InspectorBelow);
                Assert.Empty(errors.Messages);
            }));
        }

        [Fact]
        public void TheWindowOffersEditarAquiHostsTheEditorAndNuevoMakesAPushBackHere()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenRack("PB-1");
                Lay(view, 1300, 900);
                Assert.Equal(Visibility.Visible, view.PushBackOfferPanel.Visibility);
                Assert.Equal("Editar el Push Back en la ventana", Name(view.EditPushBackHereButton));

                Click(view.EditPushBackHereButton);
                rig.Door.AnswerLastRead();
                Lay(view, 1300, 900);
                var hosted = Assert.IsType<PushBackEditorView>(view.RackPageHost.Content);
                Assert.Same(rig.Window.ActivePushBackEditor, hosted.ViewModel);
                Assert.Equal(Visibility.Collapsed, view.PushBackOfferPanel.Visibility);
                Assert.Equal(1, view.PushBackViewCount);

                // «Ver el estado y la navegación del rack» shows the panel of the pilot (question 10), and back.
                rig.Window.ShowPanel = true;
                Lay(view, 1300, 900);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                rig.Window.ShowPanel = false;
                Lay(view, 1300, 900);
                Assert.Same(hosted, view.RackPageHost.Content);

                rig.Window.ActivePushBackEditor!.RequestClose();
                Lay(view, 1300, 900);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Assert.Equal(0, view.PushBackViewCount);

                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view, 1300, 900);
                Assert.Equal("Nuevo Push Back en esta ventana", Name(view.NewPushBackHereButton));
                Assert.True(view.NewPushBackHereButton.IsEnabled);
                Click(view.NewPushBackHereButton);
                Lay(view, 1300, 900);
                Assert.Equal(Visibility.Visible, view.NewPushBackRackHost.Visibility);
                var fresh = Assert.IsType<PushBackEditorView>(view.NewPushBackRackHost.Content);
                Assert.True(fresh.ViewModel.IsNewRack);
                Assert.Empty(errors.Messages);
            }));
        }
    }
}
