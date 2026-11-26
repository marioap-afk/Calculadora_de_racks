#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Views;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, Selectivo E2, part P3: «Insertar vista…» of the Selectivo editor of the RackCad window and «Selectivo en esta
    /// ventana» of «Nuevo», over the double door (<see cref="IRackCadSelectiveDoorPort"/>), as the second delivery of Dinámico did them
    /// (step 4, D-10; <c>RackCadDynamicEditorTests</c>): the views of «Insertar varias…» of the classic Selectivo editor, the write with the
    /// views (RACKSELECTIVOINSERTAR), its answer as a write, the new Selectivo inserted as a new rack (RACKSELECTIVONUEVO) that then is a
    /// rack of the drawing with the automatic name of I-60, the one request in flight, the drawing of the new rack, the close and the prune
    /// that name «un Selectivo nuevo», and the views with real WPF.
    /// </summary>
    public class RackCadSelectiveInsertTests
    {
        private static readonly SessionId Session = new(7);

        private const string Taken = "AutoCAD ya tomó la escritura del rack: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.";

        private static readonly RackViewAddress Planta = RackViewAddress.Whole(DimensionViewKind.Planta);

        /// <summary>A door that reads and writes a Selectivo, as the door of the Plugin does since Selectivo E2.</summary>
        private sealed class SelectiveDoorDouble : IRackCadSelectiveDoorPort
        {
            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<RackDoorWrite> Writes { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => Answer;

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write)
            {
                Writes.Add(write);
                return Answer;
            }

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write)
            {
                SelectiveWrites.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            public void StartLastWrite() => RequestStarted?.Invoke(SelectiveWrites.Last().RequestId);

            public void AnswerLastSelectiveRead(SelectivePalletDesign? design = null)
            {
                var request = SelectiveReads.Last();
                SelectiveRead?.Invoke(Read(request.RequestId, request.RackId, design));
            }

            public void AnswerLastSelectiveWrite(bool written, string message, string? name = null)
            {
                var write = SelectiveWrites.Last();
                Written(new RackDesignWritten(write.RequestId, write.RackId, written, message, name));
            }
        }

        /// <summary>A door of Dinámico only: it cannot write a Selectivo.</summary>
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

        /// <summary>The modal port of the host: the dialog it was given and what the user answered (cancelled by default).</summary>
        private sealed class ModalPortDouble : IRackCadModalPort
        {
            public List<Window> Shown { get; } = new();

            public Func<Window, bool?> Answer { get; set; } = _ => false;

            public bool? ShowModal(Window dialog)
            {
                Shown.Add(dialog);
                return Answer(dialog);
            }
        }

        private sealed class Rig
        {
            public Rig(bool modal = true, IRackCadRackDoorPort? door = null, bool withDoor = true)
            {
                Modal = modal ? new ModalPortDouble() : null;
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble { ListResponder = (_, _) => BrowserRackList.Ready(Listed) };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, withDoor ? door ?? Door : null, UiSupport.LoadCatalogSafe, Modal);
                Window.Refresh();
            }

            public List<BrowserRackListEntry> Listed { get; } = new()
            {
                new BrowserRackListEntry("SEL-1", "Selectivo A", "selective", new[] { "frontal" }),
                new BrowserRackListEntry("DIN-1", "DIN 1", "dynamic", new[] { "lateral" }),
            };

            public ModalPortDouble? Modal { get; }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public SelectiveDoorDouble Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            public SelectiveEditorViewModel OpenEditor(SelectivePalletDesign? design = null)
            {
                Window.OpenRack("SEL-1");
                Window.EditSelectiveHere();
                Door.AnswerLastSelectiveRead(design);
                Assert.True(Window.ActiveSelectiveEditor != null, Window.ActionMessage);
                return Window.ActiveSelectiveEditor!;
            }

            public SelectiveEditorViewModel StartNew()
            {
                Window.StartNewSelective();
                Assert.True(Window.NewSelectiveEditor != null, Window.ActionMessage);
                return Window.NewSelectiveEditor!;
            }
        }

        private static void Edit(SelectiveEditorViewModel editor, string text = "72") => Write(Field(editor.CellFields, "cHeight"), text);

        private static RackCatalog Catalog() => UiSupport.LoadCatalogSafe();

        // ------------------------------------------------------------------ the views offered and the dialog

        [Fact]
        public void TheViewsOfferedAreTheOnesOfInsertarVariasOfTheClassicSelectivoForOneAndTwoFondos()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            IReadOnlyList<RackViewBatchOption>? offered = null;
            editor.ViewChooser = options =>
            {
                offered = options;
                return null; // cancelled
            };

            editor.InsertView();
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.Equal(new[] { "Frontal · fondo 1", "Lateral · poste 1", "Lateral · poste 2", "Lateral · poste 3", "Planta" },
                offered!.Select(o => o.Label).ToArray());
            Assert.Equal(new[] { RackViewAddress.Fondo(0), RackViewAddress.Post(0), RackViewAddress.Post(1), RackViewAddress.Post(2), Planta },
                offered!.Select(o => o.Address).ToArray());

            // The classic order (InsertBatch_Click): a frontal per fondo, a lateral per post with a cut, the plan.
            var two = new Rig().OpenEditor(TwoFondos());
            two.ViewChooser = options =>
            {
                offered = options;
                return Array.Empty<RackViewAddress>(); // nothing chosen
            };
            two.InsertView();
            var expected = SelectiveDoorInsertion.ViewOptions(two.Draft.System, two.Draft.Catalog);
            Assert.Equal(expected.Select(o => o.Label).ToArray(), offered!.Select(o => o.Label).ToArray());
            Assert.Equal(new[] { "Frontal · fondo 1", "Frontal · fondo 2" }, offered!.Take(2).Select(o => o.Label).ToArray());
            Assert.Equal(new[] { "Lateral · poste 1", "Lateral · poste 2", "Lateral · poste 3", "Lateral · poste 4" },
                offered!.Where(o => o.Address.Kind == DimensionViewKind.Lateral).Select(o => o.Label).ToArray());
            Assert.Equal("Planta", offered!.Last().Label);
        }

        [Fact]
        public void TheDialogIsTheOneOfInsertarVariasOfASelectivoAndKeepsTheFrontalOfEveryFondo()
        {
            StaTestRunner.Run(() =>
            {
                var editor = new Rig().OpenEditor(TwoFondos());
                var options = SelectiveViewChoice.Options(editor.Draft.System, editor.Draft.Catalog);
                var modal = new ModalPortDouble();

                // Cancelled: nothing is chosen.
                Assert.Null(SelectiveViewChoice.Choose(options, modal));
                var presenter = Assert.IsType<RackViewBatchDialogPresenter>(Assert.IsType<RackViewBatchDialog>(Assert.Single(modal.Shown)).DataContext);
                Assert.Equal(RackSystemKind.SelectiveRack, presenter.Kind);
                Assert.Equal(options.Select(o => o.Label).ToArray(), presenter.Available.Select(o => o.Label).ToArray());

                // Accepted: the views chosen, in the order the user left them.
                modal.Answer = dialog =>
                {
                    var shown = (RackViewBatchDialogPresenter)dialog.DataContext;
                    shown.Add(shown.Available.Last());
                    shown.Add(shown.Available[1]);
                    return true;
                };
                Assert.Equal(new[] { Planta, RackViewAddress.Fondo(1) }, SelectiveViewChoice.Choose(options, modal)!.ToArray());

                // Without the port of the host the dialog is not shown.
                Assert.Null(SelectiveViewChoice.Choose(options, null!));
            });
        }

        // ------------------------------------------------------------------ «Insertar vista…» of a Selectivo of the drawing

        [Fact]
        public void InsertViewWritesTheDraftWithTheViewsChosenAndItsAnswerIsTheOneOfAWrite()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            Assert.Equal("Insertar vista…", editor.InsertViewLabel);
            Assert.True(editor.CanInsertView, editor.InsertViewBlockedText);

            // Cancelled (the real dialog of «Insertar varias…», refused by the port): nothing goes to AutoCAD.
            StaTestRunner.Run(() => editor.InsertView());
            Assert.Single(rig.Modal!.Shown);
            Assert.Empty(rig.Door.SelectiveWrites);

            editor.ViewChooser = _ => new[] { Planta, RackViewAddress.Post(1) };
            var read = Json(editor.Draft.Design);
            Edit(editor);
            Assert.True(editor.IsDirty);
            editor.InsertView();

            var write = Assert.Single(rig.Door.SelectiveWrites);
            Assert.True(write.Inserts);
            Assert.False(write.NewRack);
            Assert.Equal(new[] { Planta, RackViewAddress.Post(1) }, write.InsertViews!.ToArray());
            Assert.Equal("SEL-1", write.RackId);
            Assert.Equal(Session, write.Session);
            Assert.NotEqual(read, Json(write.Design));
            Assert.Equal(Json(editor.Draft.Design), Json(write.Design)); // a copy of the draft, with the change
            Assert.NotSame(editor.Draft.Design, write.Design);
            Assert.NotNull(write.LinkedPropertyFinalStates);
            Assert.Empty(rig.Door.Writes); // not the door of Dinámico
            Assert.True(editor.IsWriting);
            Assert.False(editor.CanInsertView);
            Assert.Equal(editor.WritingText, editor.InsertViewBlockedText);

            // The answer of RACKSELECTIVOINSERTAR is the one of a write: what was sent is the drawing now («Limpio»).
            var answer = SelectiveDoorInsertion.AnswerForInsert(write.RequestId, write.RackId, 2, 0, 2);
            rig.Door.Written(answer);
            Assert.False(editor.IsWriting);
            Assert.False(editor.IsDirty);
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal("AutoCAD redibujó 2 vistas del rack con el borrador e insertó 2 vistas nuevas.", editor.Message);
            Assert.False(editor.MessageIsError);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));

            // «Actualizar» is still an update only.
            Edit(editor, "70");
            editor.RequestWrite();
            Assert.False(rig.Door.SelectiveWrites.Last().Inserts);
            Assert.False(rig.Door.SelectiveWrites.Last().NewRack);
        }

        [Fact]
        public void WithNoChangeInsertViewStillRunsAndAnInsertThatRedrewNothingLeavesTheDraftNotWritten()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            editor.ViewChooser = _ => new[] { RackViewAddress.Fondo(0) };

            // Unlike «Actualizar», no change does not block it: the views that exist are redrawn as they are.
            Assert.False(editor.CanUpdate);
            Assert.True(editor.CanInsertView, editor.InsertViewBlockedText);
            editor.InsertView();
            var first = Assert.Single(rig.Door.SelectiveWrites);
            Assert.True(first.Inserts);

            // Placement cancelled after the redraw: written, and the answer says so.
            rig.Door.Written(SelectiveDoorInsertion.AnswerForInsert(first.RequestId, first.RackId, 1, 0, 0));
            Assert.False(editor.IsDirty);
            Assert.EndsWith("pero no insertó ninguna vista nueva: se canceló la colocación o no se pudo preparar. Revisa la línea de órdenes de AutoCAD.",
                editor.Message);

            // Nothing redrawn: the draft stays not written, and the reason is an error.
            Edit(editor);
            editor.InsertView();
            var second = rig.Door.SelectiveWrites.Last();
            rig.Door.Written(SelectiveDoorInsertion.AnswerForInsert(second.RequestId, second.RackId, 0, 0, 1));
            Assert.True(editor.IsDirty);
            Assert.True(editor.MessageIsError);
            Assert.StartsWith("AutoCAD insertó 1 vista nueva, pero no redibujó las vistas que ya había", editor.Message);
            Assert.Equal("Selectivo A", rig.Window.UnwrittenWorkOf(Session));
        }

        [Fact]
        public void TheTextsThatBlockInsertView()
        {
            // Without the modal port of the host the views cannot be chosen: «Insertar vista…» is not offered.
            var noPort = new Rig(modal: false).OpenEditor();
            Assert.False(noPort.CanInsertView);
            noPort.InsertView();
            Assert.Equal("Aquí no se pueden insertar vistas del rack.", noPort.Message);
            Assert.True(noPort.MessageIsError);

            // An editor with no action of the window (out of the window) is not offered either.
            var loose = SelectiveEditor();
            loose.ViewChooser = _ => new[] { Planta };
            Assert.Equal("Aquí no se pueden insertar vistas del rack.", loose.InsertViewBlockedText);

            var rig = new Rig();
            var editor = rig.OpenEditor();
            editor.ViewChooser = _ => new[] { Planta };

            // Tramos written in the inspector and not applied: the write would not carry them (S-21), as for «Actualizar».
            editor.SelectFront(0);
            editor.StartSegments();
            editor.AddSegment();
            Assert.True(editor.HasPendingSegments);
            Assert.Equal("Aplica los tramos con «Aplicar tramos» o descártalos antes de escribir.", editor.InsertViewBlockedText);
            editor.InsertView();
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.Equal(editor.InsertViewBlockedText, editor.Message);
            editor.DiscardSegments();
            Assert.True(editor.CanInsertView, editor.InsertViewBlockedText);

            // While AutoCAD writes: the text of the write, and once the command took it, the one that no longer says «Dejar de esperar».
            editor.InsertView();
            Assert.Equal("AutoCAD está escribiendo el rack: espera a que termine o pulsa «Dejar de esperar».", editor.InsertViewBlockedText);
            rig.Door.StartLastWrite();
            Assert.False(rig.Window.CanStopWaiting);
            Assert.Equal(Taken, editor.InsertViewBlockedText);
            Assert.Equal(Taken, editor.UpdateBlockedText);
        }

        [Fact]
        public void TheStateOfInsertViewIsRaisedWithTheStateOfTheEditor()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor();
            var raised = new List<string?>();
            editor.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            editor.ViewChooser = _ => new[] { Planta };
            editor.InsertView();
            Assert.Contains(nameof(SelectiveEditorViewModel.CanInsertView), raised);
            Assert.Contains(nameof(SelectiveEditorViewModel.InsertViewBlockedText), raised);
        }

        // ------------------------------------------------------------------ the new Selectivo of «Nuevo»

        [Fact]
        public void ANewSelectivoIsMadeHereAndInsertedWithTheViewsChosenAsANewRack()
        {
            var rig = new Rig();
            Assert.True(rig.Window.CanStartNewSelective);
            rig.Window.StartNewSelective();

            var editor = rig.Window.NewSelectiveEditor!;
            Assert.NotNull(editor);
            Assert.True(editor.IsNewRack);
            Assert.True(Guid.TryParse(editor.RackId, out _));
            Assert.True(rig.Window.IsNuevoPage);
            Assert.True(rig.Window.ShowNewSelectiveEditor);
            Assert.False(rig.Window.ShowNewDynamicEditor);
            Assert.False(rig.Window.ShowNuevoCards);
            Assert.Null(rig.Window.ActiveSelectiveEditor);
            Assert.Contains(editor, rig.Window.SelectiveEditors);
            Assert.Equal(SelectiveEditorViewModel.NewRackOpenedText, editor.Message);
            Assert.Equal("Insertar en el dibujo…", editor.InsertViewLabel);
            Assert.Equal("Insertar el rack nuevo en el dibujo", editor.InsertViewAccessibleText);
            Assert.False(editor.CanUpdate);
            Assert.Equal("Un rack nuevo se escribe en el dibujo con «Insertar en el dibujo…».", editor.UpdateBlockedText);
            Assert.True(editor.CanInsertView, editor.InsertViewBlockedText);
            Assert.False(editor.IsDirty);
            Assert.True(string.IsNullOrEmpty(editor.Name));

            // The defaults of the classic editor (SelectiveDraftEditor.CreateNew).
            Assert.Equal(Json(SelectiveDraftEditor.CreateNew(Catalog()).Design), Json(editor.Draft.Design));

            // Asked again, the window shows the same new rack.
            rig.Window.StartNewSelective();
            Assert.Same(editor, rig.Window.NewSelectiveEditor);

            // A change is work not written: the drawing does not close (D-08), and the notice names it.
            Edit(editor);
            Assert.Equal("un Selectivo nuevo", rig.Window.UnwrittenWorkOf(Session));
            Assert.Null(rig.Window.UnwrittenDynamicWorkOf(Session));

            // With changes it is still not «Actualizar»: there is nothing in the drawing to update.
            Assert.True(editor.IsDirty);
            Assert.False(editor.CanUpdate);
            editor.RequestWrite();
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.Equal(editor.UpdateBlockedText, editor.Message);

            editor.ViewChooser = _ => new[] { RackViewAddress.Fondo(0), Planta };
            editor.InsertView();
            var write = Assert.Single(rig.Door.SelectiveWrites);
            Assert.True(write.NewRack);
            Assert.True(write.Inserts);
            Assert.Equal(new[] { RackViewAddress.Fondo(0), Planta }, write.InsertViews!.ToArray());
            Assert.Equal(editor.RackId, write.RackId);
            Assert.True(string.IsNullOrEmpty(write.Name), write.Name); // no name: the drawing gives the automatic one of I-60
            Assert.NotEqual(Json(SelectiveDraftEditor.CreateNew(Catalog()).Design), Json(write.Design));
            Assert.Equal(Json(editor.Draft.Design), Json(write.Design));
            Assert.NotNull(write.System);
            // What RACKSELECTIVONUEVO decides before the dispatch of «Nuevo» accepts what the window sends.
            var plan = SelectiveDoorInsertion.PlanNewRack(write, identityInDrawing: false, Catalog());
            Assert.True(plan.CanInsert, plan.Reason);
            Assert.Equal(new[] { RackViewAddress.Fondo(0), Planta }, plan.Views.ToArray());

            rig.Door.Written(SelectiveDoorInsertion.AnswerForNewRack(write.RequestId, write.RackId, 2, "Selectivo 1"));
            Assert.False(editor.IsNewRack);
            Assert.False(editor.IsDirty);
            Assert.Equal("Selectivo 1", editor.Name); // the automatic name the drawing gave it (M-04 of Dinámico)
            Assert.Equal("Insertar vista…", editor.InsertViewLabel);
            Assert.Equal("Insertar vistas del rack", editor.InsertViewAccessibleText);
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.True(rig.Window.ShowNuevoCards);
            Assert.False(rig.Window.ShowNewSelectiveEditor);
            Assert.Contains(editor, rig.Window.SelectiveEditors);
            Assert.Equal("AutoCAD insertó el rack nuevo «Selectivo 1» con 2 vistas. Para seguir editándolo aquí, vuelve a leer la lista y ábrelo.",
                rig.Window.ActionMessage);
            Assert.False(rig.Window.ActionFailed);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));

            // D-10: the window does not read the list again by itself. Read and opened, the rack has the same editor.
            Assert.Null(rig.Window.ActiveSelectiveEditor);
            rig.Listed.Add(new BrowserRackListEntry(editor.RackId, "Selectivo 1", "selective", new[] { "frontal", "planta" }));
            rig.Window.RackList.Reload();
            rig.Window.OpenRack(editor.RackId);
            Assert.Same(editor, rig.Window.ActiveSelectiveEditor);

            // It is a rack of the drawing now: «Actualizar» writes it, as an update, and «Insertar vista…» inserts views of it.
            Edit(editor, "70");
            Assert.True(editor.CanUpdate, editor.UpdateBlockedText);
            editor.RequestWrite();
            Assert.False(rig.Door.SelectiveWrites.Last().NewRack);
            Assert.False(rig.Door.SelectiveWrites.Last().Inserts);
            rig.Door.AnswerLastSelectiveWrite(true, "AutoCAD redibujó 2 vistas del rack con el borrador; la línea de órdenes da el recuento por tipo de vista.");
            editor.ViewChooser = _ => new[] { RackViewAddress.Post(0) };
            editor.InsertView();
            Assert.False(rig.Door.SelectiveWrites.Last().NewRack);
            Assert.True(rig.Door.SelectiveWrites.Last().Inserts);
            rig.Door.AnswerLastSelectiveWrite(true, "AutoCAD redibujó 2 vistas del rack con el borrador e insertó 1 vista nueva.");

            // A new «Selectivo en esta ventana» is another rack.
            rig.Window.StartNewSelective();
            Assert.NotSame(editor, rig.Window.NewSelectiveEditor);
        }

        [Fact]
        public void ANamedNewSelectivoKeepsItsNameAndIsWrittenWithIt()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            editor.Name = "  Nave A ";
            Assert.True(editor.IsDirty);
            Assert.Equal("Nave A", rig.Window.UnwrittenWorkOf(Session));

            editor.ViewChooser = _ => new[] { Planta };
            editor.InsertView();
            var write = Assert.Single(rig.Door.SelectiveWrites);
            Assert.Equal("Nave A", write.Name); // trimmed, as the classic editor sends it

            // The name of the answer is only taken when the draft had none.
            rig.Door.Written(new RackDesignWritten(write.RequestId, write.RackId, true, "AutoCAD insertó el rack nuevo «Nave A» con 1 vista.", "Otro"));
            Assert.Equal("  Nave A ", editor.Name);
            Assert.False(editor.IsDirty);
            Assert.False(editor.IsNewRack);
        }

        [Fact]
        public void ANewSelectivoThatWasNotPlacedStaysNewInTheWindow()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            editor.ViewChooser = _ => new[] { RackViewAddress.Post(0) };
            editor.InsertView();
            var write = rig.Door.SelectiveWrites.Last();

            rig.Door.Written(SelectiveDoorInsertion.AnswerForNewRack(write.RequestId, write.RackId, 0, null!));
            Assert.True(editor.IsNewRack);
            Assert.False(editor.IsWriting);
            Assert.Same(editor, rig.Window.NewSelectiveEditor);
            Assert.True(rig.Window.ShowNewSelectiveEditor);
            Assert.Equal("AutoCAD no insertó el rack nuevo: se canceló la colocación o no se pudo preparar. El borrador sigue en la ventana; revisa la línea de órdenes de AutoCAD.",
                editor.Message);
            Assert.True(editor.MessageIsError);
            Assert.True(editor.CanInsertView, editor.InsertViewBlockedText);
            Assert.Equal("Insertar en el dibujo…", editor.InsertViewLabel);
            Assert.False(editor.CanUpdate);
        }

        // I-02 of the fifth review of Dinámico: once the command took the write (it may wait for the user placing views), the window cannot
        // stop waiting, so what the command writes is what the window records.
        [Fact]
        public void OnceTheCommandTookTheWriteOfTheNewSelectivoTheWindowWaitsForItsAnswer()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            editor.ViewChooser = _ => new[] { RackViewAddress.Post(0) };
            editor.InsertView();
            Assert.True(rig.Window.CanStopWaiting);

            rig.Door.StartLastWrite();
            Assert.False(rig.Window.CanStopWaiting);
            Assert.StartsWith("AutoCAD ya está escribiendo el rack", rig.Window.WaitingText);
            Assert.Equal(Taken, editor.WritingText);
            Assert.Equal(Taken, editor.InsertViewBlockedText);
            rig.Window.StopWaiting();
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.Empty(rig.Door.Abandoned);
            Assert.True(editor.IsWriting);

            rig.Door.AnswerLastSelectiveWrite(true, "AutoCAD insertó el rack nuevo con 1 vista.");
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.False(editor.IsNewRack);

            // A request whose command has not taken it can still be abandoned, as before, and the draft stays not written.
            Edit(editor);
            editor.RequestWrite();
            Assert.True(rig.Window.CanStopWaiting);
            rig.Window.StopWaiting();
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Single(rig.Door.Abandoned);
            Assert.False(editor.IsWriting);
            Assert.True(editor.IsDirty);
        }

        // I-03 of the fifth review of Dinámico: «Nuevo» follows the drawing, and a new rack of one drawing is never written in another.
        [Fact]
        public void TheNewSelectivoBelongsToItsDrawing()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            var raised = new List<string?>();
            rig.Window.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            rig.Present(8);
            Assert.Contains(nameof(RackCadWindowViewModel.NewSelectiveEditor), raised);
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.True(rig.Window.ShowNuevoCards);
            Assert.False(rig.Window.ShowNewSelectiveEditor);

            editor.ViewChooser = _ => new[] { RackViewAddress.Post(0) };
            editor.InsertView();
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.Equal("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", editor.Message);

            rig.Present(7);
            Assert.Same(editor, rig.Window.NewSelectiveEditor);
            editor.InsertView();
            Assert.Single(rig.Door.SelectiveWrites);
        }

        [Fact]
        public void WithoutADoorThatWritesASelectivoTheWindowDoesNotMakeANewSelectivo()
        {
            var none = new Rig(withDoor: false);
            Assert.False(none.Window.CanStartNewSelective);
            none.Window.StartNewSelective();
            Assert.Null(none.Window.NewSelectiveEditor);
            Assert.Equal("Aquí no se puede crear un rack en el dibujo.", none.Window.ActionMessage);
            Assert.True(none.Window.ActionFailed);

            // A door of Dinámico only cannot write a Selectivo: no new Selectivo, though a new Dinámico can be made.
            var dynamicOnly = new Rig(door: new DynamicOnlyDoor());
            Assert.False(dynamicOnly.Window.CanStartNewSelective);
            Assert.True(dynamicOnly.Window.CanStartNewDynamic);
            dynamicOnly.Window.StartNewSelective();
            Assert.Null(dynamicOnly.Window.NewSelectiveEditor);

            // With no drawing in the window.
            var rig = new Rig();
            rig.Host.Snapshot = WorkspacePanelFixtures.NoDocument();
            rig.Window.Refresh();
            Assert.False(rig.Window.CanStartNewSelective);
            rig.Window.StartNewSelective();
            Assert.Equal("No hay un dibujo en la ventana.", rig.Window.ActionMessage);
        }

        [Fact]
        public void NuevoHoldsOneNewRackOfTheDrawingAtATime()
        {
            var rig = new Rig();
            rig.Window.StartNewDynamic();
            var dynamic = rig.Window.NewDynamicEditor!;
            rig.Window.StartNewSelective();
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.Same(dynamic, rig.Window.NewDynamicEditor);
            Assert.Equal(RackCadWindowViewModel.SecondNewRackText, rig.Window.ActionMessage);
            Assert.True(rig.Window.IsNuevoPage);

            dynamic.RequestClose();
            Assert.Null(rig.Window.NewDynamicEditor);
            var selective = rig.StartNew();
            rig.Window.StartNewDynamic();
            Assert.Null(rig.Window.NewDynamicEditor);
            Assert.Same(selective, rig.Window.NewSelectiveEditor);
            Assert.Equal(RackCadWindowViewModel.SecondNewRackText, rig.Window.ActionMessage);
            Assert.True(rig.Window.ShowNewSelectiveEditor);
            Assert.False(rig.Window.ShowNuevoCards);
        }

        [Fact]
        public void DiscardAndCloseOfANewSelectivo()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            Edit(editor);
            editor.Discard();
            Assert.False(editor.IsDirty);
            Assert.Equal("Borrador descartado.", editor.Message);
            Assert.Equal(Json(SelectiveDraftEditor.CreateNew(Catalog()).Design), Json(editor.Draft.Design));
            Assert.True(editor.IsNewRack);

            // With changes the editor asks first; closed, the window forgets the new rack and «Nuevo» shows its systems again.
            Edit(editor);
            editor.RequestClose();
            Assert.True(editor.IsConfirmingClose);
            Assert.Same(editor, rig.Window.NewSelectiveEditor);
            editor.ConfirmClose();
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.DoesNotContain(editor, rig.Window.SelectiveEditors);
            Assert.True(rig.Window.ShowNuevoCards);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        // D-08: the close advisor of the window names the new Selectivo; a drawing that closes anyway says what was lost (the prune).
        [Fact]
        public void TheCloseOfTheDrawingNamesTheNewSelectivoAndItsPruneSaysWhatWasLost()
        {
            var rig = new Rig();
            var editor = rig.StartNew();
            Edit(editor);

            var lines = new List<string>();
            var advisor = new RackCadWindowCloseAdvisor(new AllowAll(), lines.Add, null!);
            advisor.SetWindowWork(rig.Window.UnwrittenWorkOf);
            Assert.Equal(CloseDecision.Veto, advisor.AdviseClose(Session));
            Assert.Equal(
                "RackCad: el dibujo no se cerró: la ventana RackCad tiene cambios sin escribir en un Selectivo nuevo. Escríbelos con «Actualizar» en su editor (un rack nuevo de la ventana, con «Insertar en el dibujo…») o descártalos, y vuelve a cerrar.",
                Assert.Single(lines));

            rig.Summary.Dead.Add(Session);
            rig.Window.Refresh();
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.DoesNotContain(editor, rig.Window.SelectiveEditors);
            Assert.Equal("Se cerró el dibujo de un Selectivo nuevo con cambios sin escribir en la ventana: esos cambios se perdieron.", rig.Window.ActionMessage);
            Assert.True(rig.Window.ActionFailed);
        }

        private sealed class AllowAll : IWorkspaceLifecycleAdvisor
        {
            public CloseDecision AdviseClose(SessionId session) => CloseDecision.Allow;

            public void SessionDestroyed(SessionId session, bool closeSequenceIntact)
            {
            }
        }

        // ------------------------------------------------------------------ the views, with real WPF

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

            Assert.Empty(errors.Messages);
        }

        private static void Lay(FrameworkElement view)
        {
            view.Measure(new Size(1200, 900));
            view.Arrange(new Rect(0, 0, 1200, 900));
            view.UpdateLayout();
        }

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

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

        [Fact]
        public void TheHeadOfTheEditorOffersInsertViewBetweenUpdateAndDiscardWithItsLabelAndName()
        {
            StaTestRunner.Run(() => WithBindingErrors(_ =>
            {
                var rig = new Rig();
                var editor = rig.OpenEditor();
                var view = new SelectiveEditorView(editor);
                Lay(view);

                Assert.Equal("Insertar vista…", view.InsertViewButton.Content);
                Assert.Equal("Insertar vistas del rack", Name(view.InsertViewButton));
                Assert.True(view.InsertViewButton.IsEnabled);
                var head = (Panel)view.InsertViewButton.Parent;
                var order = head.Children.OfType<Button>().Select(b => b.Name).ToList();
                Assert.True(order.IndexOf("UpdateButton") + 1 == order.IndexOf("InsertViewButton"), string.Join(", ", order));
                Assert.True(order.IndexOf("InsertViewButton") < order.IndexOf("DiscardButton"), string.Join(", ", order));
                var names = head.Children.OfType<Button>().Select(Name).ToList();
                Assert.Equal(names.Count, names.Distinct().Count());

                // A click chooses the views and the window writes them; the button is off while AutoCAD writes.
                editor.ViewChooser = _ => new[] { Planta };
                Click(view.InsertViewButton);
                Assert.True(Assert.Single(rig.Door.SelectiveWrites).Inserts);
                Lay(view);
                Assert.False(view.InsertViewButton.IsEnabled);
                rig.Door.AnswerLastSelectiveWrite(true, "AutoCAD redibujó 1 vista del rack con el borrador e insertó 1 vista nueva.");
                Lay(view);
                Assert.True(view.InsertViewButton.IsEnabled);

                // Without the modal port: shown, off.
                var noPort = new SelectiveEditorView(new Rig(modal: false).OpenEditor());
                Lay(noPort);
                Assert.False(noPort.InsertViewButton.IsEnabled);
            }));
        }

        [Fact]
        public void TheEditorOfANewSelectivoSaysInsertInTheDrawingAndTheNameJustTypedGoesWithIt()
        {
            StaTestRunner.Run(() => WithBindingErrors(_ =>
            {
                var rig = new Rig();
                var editor = rig.StartNew();
                var view = new SelectiveEditorView(editor);
                Lay(view);

                Assert.Equal("Insertar en el dibujo…", view.InsertViewButton.Content);
                Assert.Equal("Insertar el rack nuevo en el dibujo", Name(view.InsertViewButton));
                Assert.Equal("Inserta el rack nuevo en el dibujo, con una identidad nueva, con las vistas que elijas.", view.InsertViewButton.ToolTip);
                Assert.True(view.InsertViewButton.IsEnabled);
                Assert.False(view.UpdateButton.IsEnabled);
                Assert.Equal("Un rack nuevo se escribe en el dibujo con «Insertar en el dibujo…».", view.UpdateBlockedText.Text);
                Assert.Equal(SelectiveEditorViewModel.NewRackOpenedText, view.MessageText.Text);

                // The name typed and not left yet goes to the draft before the views are chosen.
                view.NameBox.Text = "Nave B";
                editor.ViewChooser = _ => new[] { Planta };
                Click(view.InsertViewButton);
                var write = Assert.Single(rig.Door.SelectiveWrites);
                Assert.True(write.NewRack);
                Assert.Equal("Nave B", write.Name);

                rig.Door.Written(SelectiveDoorInsertion.AnswerForNewRack(write.RequestId, write.RackId, 1, "Nave B"));
                Lay(view);
                Assert.Equal("Insertar vista…", view.InsertViewButton.Content);
                Assert.Equal("Insertar vistas del rack", Name(view.InsertViewButton));
                Assert.Equal("Escribe el borrador en las vistas del dibujo y después inserta las vistas que elijas, enlazadas al rack.",
                    view.InsertViewButton.ToolTip);
            }));
        }

        [Fact]
        public void TheNuevoPageHostsTheEditorOfTheNewSelectivo()
        {
            StaTestRunner.Run(() => WithBindingErrors(_ =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.NuevoPage.Visibility);
                Assert.Equal(Visibility.Collapsed, view.NewSelectiveRackHost.Visibility);
                Assert.Equal("Nuevo Selectivo en esta ventana", Name(view.NewSelectiveHereButton));
                Assert.True(view.NewSelectiveHereButton.IsEnabled);
                Assert.Equal("Abre aquí, en «Nuevo», el editor de la ventana con los valores por defecto del editor clásico. Se coloca con «Insertar en el dibujo…» y, si no le pones nombre, toma el automático (I-60).",
                    view.NewSelectiveHereText.Text);
                // The draft puts «Selectivo en esta ventana» first, over «Dinámico en esta ventana» and the cards of always.
                var page = (Panel)view.NewSelectiveHereButton.Parent;
                Assert.True(page.Children.IndexOf(view.NewSelectiveHereButton) < page.Children.IndexOf(view.NewDynamicHereButton));
                Assert.True(page.Children.IndexOf(view.NewDynamicHereButton) < page.Children.IndexOf(view.NewSystemList));

                Click(view.NewSelectiveHereButton);
                Lay(view);
                var hosted = Assert.IsType<SelectiveEditorView>(view.NewSelectiveRackHost.Content);
                Assert.Same(rig.Window.NewSelectiveEditor, hosted.DataContext);
                Assert.Equal(Visibility.Visible, view.NewSelectiveRackHost.Visibility);
                Assert.Equal(Visibility.Collapsed, view.NuevoPage.Visibility);
                Assert.Equal(Visibility.Collapsed, view.NewRackHost.Visibility);

                // Inserted: «Nuevo» shows its systems again, and the same view is the one of the rack once it is opened from the list.
                var editor = rig.Window.NewSelectiveEditor!;
                editor.ViewChooser = _ => new[] { Planta };
                Click(hosted.InsertViewButton);
                var write = rig.Door.SelectiveWrites.Last();
                rig.Door.Written(SelectiveDoorInsertion.AnswerForNewRack(write.RequestId, write.RackId, 1, "Selectivo 1"));
                Lay(view);
                Assert.Null(view.NewSelectiveRackHost.Content);
                Assert.Equal(Visibility.Collapsed, view.NewSelectiveRackHost.Visibility);
                Assert.Equal(Visibility.Visible, view.NuevoPage.Visibility);

                rig.Listed.Add(new BrowserRackListEntry(editor.RackId, "Selectivo 1", "selective", new[] { "planta" }));
                rig.Window.RackList.Reload();
                rig.Window.OpenRack(editor.RackId);
                Lay(view);
                Assert.Same(hosted, view.RackPageHost.Content);

                // Without a door that writes a Selectivo the button is off.
                var none = new RackCadWindowView(new Rig(withDoor: false).Window);
                Lay(none);
                Assert.False(none.NewSelectiveHereButton.IsEnabled);
            }));
        }
    }
}
