#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, Selectivo E1-B: «Editar aquí» of a Selectivo in the RackCad window, over a door that reads and writes a Selectivo
    /// (<see cref="IRackCadSelectiveDoorPort"/>). The same rules as the door of Dinámico, with ONE request in flight for the two systems:
    /// numbered requests, an answer only for its request (and its system), «Dejar de esperar», the refusal of the opening with its reason,
    /// copies of the draft with the final linked states, the editor locked while AutoCAD writes, the draft that survives a change of rack,
    /// the drafts of a closed drawing, and the close advisor that counts the drafts of Selectivo. Dinámico keeps its behavior.
    /// </summary>
    public class RackCadSelectiveWindowTests
    {
        private static readonly SessionId Session = new(7);

        /// <summary>A door that reads and writes Dinámico and Selectivo, as the door of the Plugin does since Selectivo E1-A.</summary>
        private sealed class SelectiveDoorDouble : IRackCadSelectiveDoorPort
        {
            public List<RackDoorRequest> Reads { get; } = new();

            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<RackDoorWrite> Writes { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public Action<RackDoorRequest>? DuringSelectiveRead { get; set; }

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write)
            {
                Writes.Add(write);
                return Answer;
            }

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                if (Answer.IsStarted) DuringSelectiveRead?.Invoke(request);
                return Answer;
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write)
            {
                SelectiveWrites.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Read(RackDesignRead read) => DesignRead?.Invoke(read);

            public void ReadSelective(SelectiveDesignRead read) => SelectiveRead?.Invoke(read);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            // What RACKSELECTIVOABRIR answers to the last read of a Selectivo: the rack of the drawing.
            public void AnswerLastSelectiveRead(Domain.Systems.Selective.SelectivePalletDesign? design = null, bool bound = false)
            {
                var request = SelectiveReads.Last();
                ReadSelective(SelectiveWindowFixtures.Read(request.RequestId, request.RackId, design, bound));
            }

            public void AnswerLastSelectiveWrite(bool written, string message)
            {
                var write = SelectiveWrites.Last();
                Written(new RackDesignWritten(write.RequestId, write.RackId, written, message));
            }
        }

        /// <summary>A door of Dinámico only: it cannot read a Selectivo, so the window does not offer to edit one.</summary>
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

        private sealed class Rig
        {
            public Rig(IRackCadRackDoorPort? door = null)
            {
                Door = door as SelectiveDoorDouble ?? new SelectiveDoorDouble();
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
                Window = new RackCadWindowViewModel(Panel, null, null, door ?? Door, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public SelectiveDoorDouble Door { get; }

            public RackCadWindowViewModel Window { get; }

            public RackListItem Item(string rack) => Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == rack);

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            // «Editar aquí» on SEL-1 and the answer of AutoCAD with the rack of the drawing.
            public SelectiveEditorViewModel OpenEditor(bool bound = false)
            {
                Window.OpenRack("SEL-1");
                Window.EditSelectiveHere();
                Door.AnswerLastSelectiveRead(bound: bound);
                Assert.True(Window.ActiveSelectiveEditor != null, Window.ActionMessage);
                return Window.ActiveSelectiveEditor!;
            }
        }

        private static void Edit(SelectiveEditorViewModel editor, string text = "72") => Write(Field(editor.CellFields, "cHeight"), text);

        // ------------------------------------------------------------------ the offer and the read

        [Fact]
        public void ASelectivoOffersToBeEditedHereOnlyWithADoorThatReadsASelectivo()
        {
            var rig = new Rig();
            rig.Window.OpenRack("SEL-1");
            Assert.True(rig.Window.IsSelectiveRack);
            Assert.True(rig.Window.ShowSelectiveOffer);
            Assert.False(rig.Window.ShowDynamicOffer);
            Assert.False(rig.Window.HasRackEditor);
            Assert.True(rig.Window.ShowPanel);

            rig.Window.OpenRack("DIN-1");
            Assert.False(rig.Window.ShowSelectiveOffer);
            Assert.True(rig.Window.ShowDynamicOffer);
            rig.Window.OpenRack("PB-1");
            Assert.False(rig.Window.ShowSelectiveOffer);

            // A door of Dinámico only: no offer for a Selectivo, and «Editar aquí» asks nothing.
            var dynamicOnly = new Rig(new DynamicOnlyDoor());
            dynamicOnly.Window.OpenRack("SEL-1");
            Assert.True(dynamicOnly.Window.IsSelectiveRack);
            Assert.False(dynamicOnly.Window.ShowSelectiveOffer);
            dynamicOnly.Window.EditSelectiveHere();
            Assert.False(dynamicOnly.Window.IsWaitingForAutoCad);
        }

        [Fact]
        public void EditHereAsksAutoCadForTheSelectivoAndOnlyTheAnswerToThatRequestOpensTheEditor()
        {
            var rig = new Rig();
            rig.Window.OpenRack("SEL-1");

            rig.Window.EditSelectiveHere();
            var request = Assert.Single(rig.Door.SelectiveReads);
            Assert.Equal("SEL-1", request.RackId);
            Assert.Equal(Session, request.Session);
            Assert.Empty(rig.Door.Reads); // not the read of Dinámico
            Assert.Equal("Leyendo el rack en AutoCAD…", rig.Window.ActionMessage);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            // An answer with another number, or the answer of the door of Dinámico with the same number, is not for this request.
            rig.Door.ReadSelective(Read(request.RequestId + 99, "SEL-1"));
            rig.Door.Read(new RackDesignRead(request.RequestId, "SEL-1", true, "dynamic", "SEL", "{}", null));
            Assert.Null(rig.Window.ActiveSelectiveEditor);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            rig.Door.AnswerLastSelectiveRead();

            var editor = Assert.IsType<SelectiveEditorViewModel>(rig.Window.ActiveSelectiveEditor);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Window.HasSelectiveEditor);
            Assert.True(rig.Window.HasRackEditor);
            Assert.False(rig.Window.ShowSelectiveOffer);
            Assert.False(rig.Window.ShowPanel);
            Assert.Equal("Selectivo A", editor.Name);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal(("Abierto en la ventana", "Limpio"), (rig.Item("SEL-1").StatusText, rig.Item("SEL-1").StatusKind));

            rig.Window.ShowPanel = true;
            Assert.Equal("Volver al editor", rig.Window.PanelToggleText);
        }

        [Fact]
        public void AnAnswerThatArrivesDuringTheCallIsTheAnswerOfTheRequest()
        {
            var rig = new Rig();
            rig.Door.DuringSelectiveRead = request => rig.Door.ReadSelective(Read(request.RequestId, request.RackId));
            rig.Window.OpenRack("SEL-1");

            rig.Window.EditSelectiveHere();

            Assert.NotNull(rig.Window.ActiveSelectiveEditor);
            Assert.False(rig.Window.IsWaitingForAutoCad);
        }

        [Fact]
        public void TheRefusalOfTheOpeningSaysWhyAndOpensNothing()
        {
            var rig = new Rig();
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere();
            var request = rig.Door.SelectiveReads.Last();

            rig.Door.ReadSelective(SelectiveDesignRead.Refused(request.RequestId, "SEL-1", "selective", "Selectivo A",
                "La variable de proyecto que gobierna la holgura vertical ya no existe."));

            Assert.Null(rig.Window.ActiveSelectiveEditor);
            Assert.True(rig.Window.ActionFailed);
            Assert.Equal("No se pudo abrir el rack: La variable de proyecto que gobierna la holgura vertical ya no existe.", rig.Window.ActionMessage);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Window.ShowSelectiveOffer);
        }

        [Fact]
        public void OneRequestWaitsAtATimeForBothSystemsAndStoppingTheWaitDropsItSoALateAnswerOpensNothing()
        {
            var rig = new Rig();
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere();
            var request = rig.Door.SelectiveReads.Last();

            rig.Window.EditSelectiveHere();
            Assert.Single(rig.Door.SelectiveReads);
            Assert.StartsWith("AutoCAD todavía no respondió", rig.Window.ActionMessage);
            // The same request blocks the door of Dinámico.
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            Assert.Empty(rig.Door.Reads);

            rig.Window.StopWaiting();
            Assert.Equal(new[] { request.RequestId }, rig.Door.Abandoned);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("Se dejó de esperar la lectura: si la orden llega a correr, no abrirá nada.", rig.Window.ActionMessage);

            rig.Window.OpenRack("SEL-1");
            rig.Door.ReadSelective(Read(request.RequestId, "SEL-1"));
            Assert.Null(rig.Window.ActiveSelectiveEditor);

            rig.Window.EditSelectiveHere();
            Assert.Equal(2, rig.Door.SelectiveReads.Count);
            Assert.NotEqual(request.RequestId, rig.Door.SelectiveReads.Last().RequestId);
        }

        [Fact]
        public void ABusyAutoCadRefusesTheReadAndTheWrite()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "x");

            editor.RequestWrite();
            Assert.False(editor.IsWriting);
            Assert.True(editor.IsDirty);
            Assert.StartsWith("Hay un comando en curso en AutoCAD", editor.Message);
            Assert.False(rig.Window.IsWaitingForAutoCad);

            editor.RequestClose();
            editor.ConfirmClose();
            rig.Window.EditSelectiveHere();
            Assert.True(rig.Window.ActionFailed);
            Assert.StartsWith("Hay un comando en curso en AutoCAD", rig.Window.ActionMessage);
            Assert.False(rig.Window.IsWaitingForAutoCad);
        }

        // ------------------------------------------------------------------ the write

        [Fact]
        public void UpdateSendsCopiesWithTheFinalLinkedStatesLocksTheEditorAndTheAnswerMakesItClean()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor(bound: true);
            Edit(editor);
            editor.Name = "Selectivo A nuevo ";

            editor.RequestWrite();

            var write = Assert.Single(rig.Door.SelectiveWrites);
            Assert.Empty(rig.Door.Writes);
            Assert.Equal("SEL-1", write.RackId);
            Assert.Equal(Session, write.Session);
            Assert.Equal("Selectivo A nuevo", write.Name);
            Assert.NotSame(editor.Draft.Design, write.Design);
            Assert.NotSame(editor.Draft.System, write.System);
            Assert.Equal(Json(editor.Draft.Design), Json(write.Design));
            Assert.Equal("Selectivo A nuevo", write.System.Name);
            Assert.True(write.LinkedPropertyFinalStates[Clearance].IsReference); // the binding goes as it was read
            Assert.True(editor.IsWriting);
            Assert.Equal("Escribiendo", editor.StatusText);
            Assert.False(editor.CanUpdate);
            Assert.Equal(("Escribiendo en el dibujo", "Modificado"), (rig.Item("SEL-1").StatusText, rig.Item("SEL-1").StatusKind));

            // While AutoCAD writes the editor cannot be edited.
            Edit(editor, "90");
            Assert.Equal(72.0, write.Design.Bays[0].Levels[0].Pallet.Alto);
            Assert.Equal(72.0, editor.Draft.Cell(0, 0, 0)!.Alto);

            // An answer with another number does not end the wait.
            rig.Door.Written(new RackDesignWritten(write.RequestId + 99, "SEL-1", true, "otro"));
            Assert.True(editor.IsWriting);

            rig.Door.AnswerLastSelectiveWrite(true, "Escrito.");
            Assert.False(editor.IsWriting);
            Assert.False(editor.IsDirty);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("Escrito.", editor.Message);
            Assert.Equal(("Abierto en la ventana", "Limpio"), (rig.Item("SEL-1").StatusText, rig.Item("SEL-1").StatusKind));

            // «Descartar» now goes back to what was written.
            Edit(editor, "80");
            editor.Discard();
            Assert.Equal(72.0, editor.Draft.Cell(0, 0, 0)!.Alto);
            Assert.Equal("Selectivo A nuevo", editor.Name);
            Assert.False(editor.IsDirty);
        }

        [Fact]
        public void AFailedWriteKeepsTheDraftModifiedAndSaysWhy()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);
            editor.RequestWrite();

            rig.Door.AnswerLastSelectiveWrite(false, "El vínculo de la holgura vertical cambió en el dibujo desde que se abrió el rack aquí: no se escribió nada.");

            Assert.False(editor.IsWriting);
            Assert.True(editor.IsDirty);
            Assert.Equal("Modificado", editor.StatusText);
            Assert.True(editor.MessageIsError);
            Assert.StartsWith("El vínculo de la holgura vertical cambió", editor.Message);
            Assert.True(editor.CanUpdate);
        }

        [Fact]
        public void StoppingTheWaitOfAWriteUnlocksTheEditorKeepsItsChangesAndALateAnswerDoesNotMarkItWritten()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);
            editor.RequestWrite();
            var write = rig.Door.SelectiveWrites.Last();

            rig.Window.StopWaiting();

            Assert.Equal(new[] { write.RequestId }, rig.Door.Abandoned);
            Assert.False(editor.IsWriting);
            Assert.True(editor.IsDirty);
            Assert.StartsWith("Se dejó de esperar a AutoCAD", editor.Message);

            rig.Door.AnswerLastSelectiveWrite(true, "Escrito tarde.");
            Assert.True(editor.IsDirty);
            Assert.NotEqual("Escrito tarde.", editor.Message);
        }

        [Fact]
        public void OnceTheCommandTookTheWriteTheWindowWaitsForItsAnswer()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);
            editor.RequestWrite();

            rig.Door.Start(rig.Door.SelectiveWrites.Last().RequestId);
            Assert.False(rig.Window.CanStopWaiting);
            rig.Window.StopWaiting();
            Assert.True(editor.IsWriting);
            Assert.Empty(rig.Door.Abandoned);

            rig.Door.AnswerLastSelectiveWrite(true, "Escrito.");
            Assert.False(editor.IsDirty);
        }

        [Fact]
        public void TheDraftOfAnotherDrawingIsNotWrittenInTheActiveOne()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);

            rig.Present(8);
            editor.RequestWrite();

            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.Equal("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", editor.Message);
        }

        // ------------------------------------------------------------------ the life of the draft

        [Fact]
        public void TheDraftSurvivesAChangeOfRackAndItsStateShowsInTheList()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);

            Assert.Equal(("Modificado (editor de la ventana)", "Modificado"), (rig.Item("SEL-1").StatusText, rig.Item("SEL-1").StatusKind));
            Assert.Equal(1, rig.Window.RackList.ChangedCount);

            rig.Window.OpenRack("PB-1");
            Assert.Null(rig.Window.ActiveSelectiveEditor);
            Assert.True(rig.Window.ShowPanel);
            rig.Window.OpenRack("SEL-1");
            Assert.Same(editor, rig.Window.ActiveSelectiveEditor);
            Assert.True(editor.IsDirty);

            // With the panel shown the rack does not offer to be edited again: it has its editor.
            rig.Window.ShowPanel = true;
            Assert.False(rig.Window.ShowSelectiveOffer);
        }

        [Fact]
        public void ClosingTheEditorForgetsTheDraftAndTheRackOffersToBeEditedAgain()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();

            editor.RequestClose();

            Assert.Null(rig.Window.ActiveSelectiveEditor);
            Assert.True(rig.Window.ShowSelectiveOffer);
            Assert.Empty(rig.Window.SelectiveEditors);
            Assert.Equal(("Sin borrador", "Limpio"), (rig.Item("SEL-1").StatusText, rig.Item("SEL-1").StatusKind));
        }

        [Fact]
        public void TheCloseAdvisorCountsTheDraftsOfSelectivoWithThoseOfDinamico()
        {
            // D-08: what the close advisor of the window asks (UnwrittenWorkOf), now with the Selectivo.
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));

            Edit(editor);
            Assert.Equal("Selectivo A", rig.Window.UnwrittenWorkOf(Session));
            Assert.Null(rig.Window.UnwrittenWorkOf(new SessionId(8)));
            // The part of Dinámico does not count it: it is the same as before.
            Assert.Null(rig.Window.UnwrittenDynamicWorkOf(Session));

            // A Dinámico with changes in the same drawing: both, by name.
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            var read = rig.Door.Reads.Last();
            rig.Door.Read(new RackDesignRead(read.RequestId, "DIN-1", true, "dynamic", "DIN 1", DynamicJson(), null));
            var dynamic = rig.Window.ActiveDynamicEditor!;
            Write(dynamic.Essentials.Single(f => f.Key == "weight"), "1200");
            Assert.Equal("DIN 1, Selectivo A", rig.Window.UnwrittenWorkOf(Session));
            Assert.Equal("DIN 1", rig.Window.UnwrittenDynamicWorkOf(Session));

            // Being written still counts; written, it does not.
            rig.Window.OpenRack("SEL-1");
            editor.RequestWrite();
            Assert.Equal("DIN 1, Selectivo A", rig.Window.UnwrittenWorkOf(Session));
            rig.Door.AnswerLastSelectiveWrite(true, "Escrito.");
            Assert.Equal("DIN 1", rig.Window.UnwrittenWorkOf(Session));
        }

        [Fact]
        public void TheDraftsOfAClosedDrawingAreForgottenAndLostWorkIsSaid()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);
            editor.RequestWrite();
            var write = rig.Door.SelectiveWrites.Single();

            rig.Summary.Dead.Add(Session);
            rig.Present(8);

            Assert.Empty(rig.Window.SelectiveEditors);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal(new[] { write.RequestId }, rig.Door.Abandoned);
            Assert.True(rig.Window.ActionFailed);
            Assert.Equal("Se cerró el dibujo de Selectivo A con cambios sin escribir en la ventana: esos cambios se perdieron.", rig.Window.ActionMessage);
        }

        [Fact]
        public void DinamicoKeepsItsDoorWithTheDoorOfSelectivo()
        {
            // The door of the Plugin is both: a Dinámico still opens and writes through its own read and write.
            var rig = new Rig();
            rig.Window.OpenRack("DIN-1");
            Assert.True(rig.Window.ShowDynamicOffer);

            rig.Window.EditDynamicHere();
            var read = Assert.Single(rig.Door.Reads);
            Assert.Empty(rig.Door.SelectiveReads);
            // The answer of the door of Selectivo with the same number is not for the read of Dinámico.
            rig.Door.ReadSelective(Read(read.RequestId, "DIN-1"));
            Assert.Null(rig.Window.ActiveDynamicEditor);

            rig.Door.Read(new RackDesignRead(read.RequestId, "DIN-1", true, "dynamic", "DIN 1", DynamicJson(), null));
            var editor = Assert.IsType<DynamicEditorViewModel>(rig.Window.ActiveDynamicEditor);
            Assert.Null(rig.Window.ActiveSelectiveEditor);
            Assert.True(rig.Window.HasRackEditor);

            Write(editor.Essentials.Single(f => f.Key == "weight"), "1200");
            editor.RequestWrite();
            var write = Assert.Single(rig.Door.Writes);
            Assert.Empty(rig.Door.SelectiveWrites);
            rig.Door.Written(new RackDesignWritten(write.RequestId, "DIN-1", true, "Escrito."));
            Assert.False(editor.IsDirty);
        }

        // ------------------------------------------------------------------ the corrections of the review of the entrega 1

        [Fact]
        public void PendingTramosAloneVetoTheCloseOfTheDrawing()
        {
            // M-04 (S-21): tramos written and not applied are work not written.
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));

            editor.SelectFront(1);
            editor.StartSegments();
            Assert.Equal("Selectivo A", rig.Window.UnwrittenWorkOf(Session));
            Assert.Equal(("Modificado (editor de la ventana)", "Modificado"), (rig.Item("SEL-1").StatusText, rig.Item("SEL-1").StatusKind));

            editor.DiscardSegments();
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        [Fact]
        public void ASelectivoWithNoInnerNameIsNamedAsTheListNamesItInTheVetoAndWhenItsDrawingCloses()
        {
            // M-09 (S-26): the inner document has no name; the envelope (the list) does. Never the id of the rack.
            var rig = new Rig();
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere();
            var request = rig.Door.SelectiveReads.Last();
            var unnamed = RackDoorRules.AnswerForSelectiveOpen(request.RequestId, "SEL-1", RackEmbedDocument.KindSelective,
                Store.Serialize(SelectivePalletDesignDocument.From(Structure(), "SEL-1", null)), ProjectVariablesReadResult.Absent());
            Assert.True(unnamed.Opens, unnamed.Reason);
            rig.Door.ReadSelective(unnamed);
            var editor = rig.Window.ActiveSelectiveEditor!;
            Assert.Equal(string.Empty, editor.Name);

            Edit(editor);
            Assert.Equal("Selectivo A", rig.Window.UnwrittenWorkOf(Session));

            // A name written in the editor is the name.
            editor.Name = "Selectivo nuevo";
            Assert.Equal("Selectivo nuevo", rig.Window.UnwrittenWorkOf(Session));
            editor.Name = string.Empty;

            rig.Summary.Dead.Add(Session);
            rig.Present(8);
            Assert.Equal("Se cerró el dibujo de Selectivo A con cambios sin escribir en la ventana: esos cambios se perdieron.", rig.Window.ActionMessage);
        }

        [Fact]
        public void TheStylesOfTheDrawingThatTheReadCarriesAreTheListOfTheDimensionStyle()
        {
            // M-10 (S-27): RACKSELECTIVOABRIR sends the dimension styles of the drawing with the read.
            var rig = new Rig();
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere();
            var request = rig.Door.SelectiveReads.Last();
            rig.Door.ReadSelective(Read(request.RequestId, "SEL-1") with { DimensionStyles = new[] { "Standard", "ISO-25" } });

            var style = Choice(rig.Window.ActiveSelectiveEditor!.AnnotationFields, "dimStyle");
            Assert.Equal(new[] { "(Automático)", "Standard", "ISO-25" }, style.Options.Select(o => o.Value).ToArray());
        }

        [Fact]
        public void OnceTheCommandTookTheWriteTheEditorSaysToWaitForItsAnswer()
        {
            // M-11 (S-28): the window no longer offers «Dejar de esperar», and the editor no longer says to press it.
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Edit(editor);
            editor.RequestWrite();
            Assert.Contains("Dejar de esperar", editor.UpdateBlockedText);

            rig.Door.Start(rig.Door.SelectiveWrites.Last().RequestId);
            Assert.False(rig.Window.CanStopWaiting);
            Assert.DoesNotContain("Dejar de esperar", editor.UpdateBlockedText);
            Assert.StartsWith("AutoCAD ya tomó la escritura del rack", editor.UpdateBlockedText);

            rig.Door.AnswerLastSelectiveWrite(true, "Escrito.");
            Assert.False(editor.IsWriting);
            Assert.Equal("Sin cambios que escribir.", editor.UpdateBlockedText);
        }

        // ------------------------------------------------------------------ the view of the window

        [Fact]
        public void ThePageOfTheRackHostsTheSelectivoEditorAndTheUserCanLookAtThePanelInstead()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenRack("SEL-1");
                Lay(view);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Assert.Equal(System.Windows.Visibility.Visible, view.SelectiveOfferPanel.Visibility);
                Assert.Equal(System.Windows.Visibility.Collapsed, view.DynamicOfferPanel.Visibility);
                Assert.Equal(System.Windows.Visibility.Collapsed, view.PanelToggleButton.Visibility);

                Click(view.EditSelectiveHereButton);
                Lay(view);
                Assert.Equal(System.Windows.Visibility.Visible, view.WaitingPanel.Visibility);
                rig.Door.AnswerLastSelectiveRead();
                Lay(view);

                var editorView = Assert.IsType<SelectiveEditorView>(view.RackPageHost.Content);
                Assert.Same(rig.Window.ActiveSelectiveEditor, editorView.ViewModel);
                Assert.Equal(System.Windows.Visibility.Collapsed, view.SelectiveOfferPanel.Visibility);
                Assert.Equal(System.Windows.Visibility.Collapsed, view.WaitingPanel.Visibility);
                Assert.Equal(System.Windows.Visibility.Visible, view.PanelToggleButton.Visibility);

                Click(view.PanelToggleButton);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Click(view.PanelToggleButton);
                Assert.Same(editorView, view.RackPageHost.Content);

                // The view survives a change of rack, and closing the editor forgets it.
                rig.Window.OpenRack("PB-1");
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                rig.Window.OpenRack("SEL-1");
                Assert.Same(editorView, view.RackPageHost.Content);
                Assert.Equal(1, view.SelectiveViewCount);
                Click(editorView.CloseEditorButton);
                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Assert.Equal(0, view.SelectiveViewCount);
                Assert.Equal(0, view.DynamicViewCount);
            });
        }

        private static void Lay(System.Windows.FrameworkElement view)
        {
            view.Measure(new System.Windows.Size(1200, 900));
            view.Arrange(new System.Windows.Rect(0, 0, 1200, 900));
            view.UpdateLayout();
        }

        private static void Click(System.Windows.Controls.Button button) =>
            button.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        // A Dinámico of the drawing, recomposed as RACKDINAMICOABRIR reads it.
        private static string DynamicJson()
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
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(design, "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            return new RackProjectStore().Serialize(RackProject.ForDynamic(draft.Design));
        }
    }
}
