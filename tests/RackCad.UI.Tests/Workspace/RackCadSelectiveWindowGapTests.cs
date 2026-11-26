#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// I-64 A-14, line D, Selectivo, third wave (β): the tests of the editor and of its window that were missing (priority A of the gap
    /// analysis of the view-model: those that protect the work of the user). «Editar aquí» again over a draft with changes does not replace
    /// it; «Descartar» that cannot read its baseline, or cannot recompose it, keeps or locks the draft as it says; «Cerrar el editor» while
    /// AutoCAD writes closes nothing; «Actualizar» of an editor the window forgot, or while the door waits, writes nothing; the tramos added
    /// and removed are those of their row; and closing the tab of the rack from the list does not lose the draft.
    /// </summary>
    public class RackCadSelectiveWindowGapTests
    {
        private static readonly SessionId Session = new(7);

        private sealed class Door : IRackCadSelectiveDoorPort
        {
            public List<RackDoorRequest> Reads { get; } = new();

            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

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

            public void AnswerLastSelectiveRead(SelectivePalletDesign? design = null)
            {
                var request = SelectiveReads.Last();
                SelectiveRead?.Invoke(Read(request.RequestId, request.RackId, design));
            }

            public void AnswerLastSelectiveWrite(bool written, string message)
            {
                var write = SelectiveWrites.Last();
                DesignWritten?.Invoke(new RackDesignWritten(write.RequestId, write.RackId, written, message));
            }

            // Only to keep the compiler quiet about the events this test does not raise.
            public void Touch()
            {
                DesignRead?.Invoke(null!);
                RequestStarted?.Invoke(0);
            }
        }

        private sealed class Rig
        {
            public Rig()
            {
                Door = new Door();
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[]
                    {
                        new BrowserRackListEntry("SEL-1", "Selectivo A", "selective", new[] { "frontal" }),
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

            public Door Door { get; }

            public RackCadWindowViewModel Window { get; }

            public RackListItem Item(string rack) => Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == rack);

            public SelectiveEditorViewModel OpenEditor()
            {
                Window.OpenRack("SEL-1");
                Window.EditSelectiveHere();
                Door.AnswerLastSelectiveRead();
                Assert.True(Window.ActiveSelectiveEditor != null, Window.ActionMessage);
                return Window.ActiveSelectiveEditor!;
            }
        }

        private static void Edit(SelectiveEditorViewModel editor, string text = "72") => Write(Field(editor.CellFields, "cHeight"), text);

        // ------------------------------------------------------------------ «Editar aquí» again

        [Fact]
        public void EditHereAgainOverADraftWithChangesDoesNotReplaceItAndOverACleanOneReplacesItAndForgetsTheOld()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);

            // The offer is not shown with an editor; a second read (a stale gesture) still must not drop the work.
            rig.Window.EditSelectiveHere();
            rig.Door.AnswerLastSelectiveRead();
            Assert.Same(editor, rig.Window.ActiveSelectiveEditor);
            Assert.True(editor.IsDirty);
            Assert.Equal(72.0, editor.Draft.Cell(0, 0, 0)!.Alto);
            Assert.Equal("Este rack ya tiene un borrador con cambios en la ventana: no se reemplazó.", rig.Window.ActionMessage);

            // Being written is work too.
            editor.RequestWrite();
            Assert.True(editor.IsWriting);
            rig.Door.AnswerLastSelectiveWrite(true, "Escrito.");
            Assert.False(editor.IsDirty);

            // Clean: the read opens a new editor, and the old one no longer moves the window.
            rig.Window.EditSelectiveHere();
            rig.Door.AnswerLastSelectiveRead();
            var fresh = rig.Window.ActiveSelectiveEditor!;
            Assert.NotSame(editor, fresh);
            Assert.Single(rig.Window.SelectiveEditors);
            var heads = 0;
            rig.Window.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(RackCadWindowViewModel.HeaderText)) heads++; };
            Edit(editor, "80"); // the forgotten editor: unhooked, it no longer refreshes the window
            Assert.Equal(0, heads);
            Assert.Equal(("Abierto en la ventana", "Limpio"), (rig.Item("SEL-1").StatusText, rig.Item("SEL-1").StatusKind));
            editor.RequestClose();
            editor.ConfirmClose();
            Assert.Same(fresh, rig.Window.ActiveSelectiveEditor);
            Edit(fresh);
            Assert.True(heads > 0); // the editor of the window still does
        }

        // ------------------------------------------------------------------ «Descartar» that cannot go back

        [Fact]
        public void DiscardThatCannotReadItsBaselineKeepsTheDraftAsItIs()
        {
            var editor = SelectiveEditor();
            Edit(editor);
            editor.MarkWritten("Escrito.", "{ esto no es un diseño", editor.Name);
            Edit(editor, "80");

            editor.Discard();

            Assert.StartsWith("No se pudo volver a lo leído del dibujo: ", editor.Message);
            Assert.EndsWith(" El borrador sigue como está.", editor.Message);
            Assert.True(editor.MessageIsError);
            Assert.Equal(80.0, editor.Draft.Cell(0, 0, 0)!.Alto);
            Assert.True(editor.CanEdit);
        }

        [Fact]
        public void DiscardWhoseBaselineCannotBeRecomposedLeavesTheEditorReadOnlyAndSaysWhy()
        {
            var editor = SelectiveEditor();
            var broken = Structure();
            broken.PostPeralte = 0.0;
            Edit(editor);
            editor.MarkWritten("Escrito.", Json(broken), editor.Name);

            editor.Discard();

            Assert.True(editor.IsLoadFailed);
            Assert.False(editor.CanEdit);
            Assert.False(editor.CanUpdate);
            Assert.StartsWith("No se pudo recomponer el rack al abrirlo: Peralte de poste inválido.", editor.Message);
            Assert.True(editor.MessageIsError);
        }

        // ------------------------------------------------------------------ closing and writing

        [Fact]
        public void ConfirmingTheCloseWhileAutoCadWritesClosesNothing()
        {
            var editor = SelectiveEditor();
            var closed = 0;
            editor.CloseRequested += _ => closed++;
            Edit(editor);
            editor.RequestClose();
            Assert.True(editor.IsConfirmingClose);

            editor.BeginWriting();
            Assert.False(editor.IsConfirmingClose);
            editor.ConfirmClose();
            editor.RequestClose();
            Assert.Equal(0, closed);
            Assert.True(editor.MessageIsError);

            editor.EndWriting();
            editor.RequestClose();
            editor.ConfirmClose();
            Assert.Equal(1, closed);
        }

        [Fact]
        public void UpdateOfAnEditorTheWindowForgotOrWhileTheDoorWaitsWritesNothing()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);

            // The door waits for a read of a Dinámico: the Selectivo is not handed over, and the editor stays editable.
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            Assert.True(rig.Window.IsWaitingForAutoCad);
            editor.RequestWrite();
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.False(editor.IsWriting);
            Assert.Equal("AutoCAD todavía no respondió a la petición anterior de la ventana: espera a que termine o pulsa «Dejar de esperar».", editor.Message);
            Assert.True(editor.MessageIsError);
            rig.Window.StopWaiting();

            // An editor that was closed (the window forgot it) writes nothing either.
            rig.Window.OpenRack("SEL-1");
            editor.RequestClose();
            editor.ConfirmClose();
            Assert.Empty(rig.Window.SelectiveEditors);
            editor.RequestWrite();
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.False(editor.IsWriting);
        }

        [Fact]
        public void ClosingTheTabOfTheRackFromTheListKeepsItsDraft()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);

            rig.Window.RackList.Close("SEL-1");

            Assert.Single(rig.Window.SelectiveEditors);
            Assert.Equal("Selectivo A", rig.Window.UnwrittenWorkOf(Session)); // still vetoes the close of the drawing
            rig.Window.OpenRack("SEL-1");
            Assert.Same(editor, rig.Window.ActiveSelectiveEditor);
            Assert.Equal(72.0, editor.Draft.Cell(0, 0, 0)!.Alto);
        }

        // ------------------------------------------------------------------ the tramos of the inspector

        [Fact]
        public void AddingAndRemovingTramosKeepsTheOthersAndTheRestIsTheLast()
        {
            var editor = SelectiveEditor();
            editor.SelectFront(1); // F2, 100 in
            editor.StartSegments();
            editor.SegmentRows[0].LengthText = "20";
            editor.AddSegment();
            editor.SegmentRows[1].LengthText = "30";
            editor.SegmentRows[1].Loaded = false;
            editor.AddSegment();
            editor.SegmentRows[2].LengthText = "25";
            Assert.Equal(4, editor.SegmentRows.Count);
            Assert.True(editor.SegmentRows[3].IsRest);
            Assert.Equal("(resto) ≈ 25 in", editor.SegmentRows[3].RestText);

            Assert.Equal(new[] { true, false, true, true }, editor.SegmentRows.Select(r => r.Loaded).ToArray());

            // Removing the second (30 in, with no largueros) keeps the first and the third, in order, with their «con largueros».
            editor.RemoveSegment(1);
            Assert.Equal(new[] { "20", "25", string.Empty }, editor.SegmentRows.Select(r => r.LengthText).ToArray());
            Assert.Equal(new[] { true, true, true }, editor.SegmentRows.Select(r => r.Loaded).ToArray());
            Assert.Equal("(resto) ≈ 55 in", editor.SegmentRows[2].RestText);
            editor.RemoveSegment(9); // a row that does not exist: nothing
            Assert.Equal(3, editor.SegmentRows.Count);

            editor.ApplySegments();
            Assert.Equal(string.Empty, editor.SegmentError);
            var segments = editor.Draft.Front(0, 1)!.Segments;
            Assert.Equal(new[] { 20.0, 25.0, 0.0 }, segments.Select(s => s.Length).ToArray());
            Assert.Equal(new[] { true, true, true }, segments.Select(s => s.Loaded).ToArray());
        }
    }
}
