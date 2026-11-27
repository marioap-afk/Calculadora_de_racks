#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T4 bom (I-64 A-14, line D; prototype «bom», BM-01 to BM-11): the page «BOM y lista de racks» of the RackCad window, over a door that
    /// reads the BOM of the drawing (<see cref="IRackCadBomPort"/>) and a Selectivo. The read is a request of the door like the others (ONE
    /// in flight for the window, numbered, «Dejar de esperar», the answer only for its request and its drawing); the page reads on its own the
    /// first time it is shown for a drawing, says after any command that is not its own read that it may be out of date, shows the BOM of a
    /// rack of the drawing or of the draft of its editor, follows the side column, says what prevents the total, exports next to the drawing
    /// with no dialog, and RACKLISTA / RACKBOMTOTAL open it.
    /// </summary>
    public class RackCadBomPageTests
    {
        private static readonly SessionId Session = new(7);
        private static readonly DateTime At = new(2026, 11, 26, 10, 42, 0);

        /// <summary>A door that reads the BOM, and reads and writes a Selectivo (for the drafts), as the door of the Plugin does.</summary>
        private sealed class BomDoor : IRackCadSelectiveDoorPort, IRackCadBomPort
        {
            public List<RackBomRequest> BomReads { get; } = new();

            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult BomAnswer { get; set; } = RackCadClassicResult.Started();

            public Action<RackBomRequest>? DuringBomRead { get; set; }

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public event Action<RackBomReading>? BomRead;

            public event Action<RackBomTab, RackBomReading>? BomPageAsked;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestBomRead(RackBomRequest request)
            {
                BomReads.Add(request);
                if (BomAnswer.IsStarted) DuringBomRead?.Invoke(request);
                return BomAnswer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Read(RackBomReading reading) => BomRead?.Invoke(reading);

            public void AnswerLastBomRead(RackBomReading reading) => Read(reading with { RequestId = BomReads.Last().RequestId });

            // RACKLISTA or RACKBOMTOTAL: they read the drawing inside themselves and hand the reading over.
            public void Ask(RackBomTab tab, RackBomReading reading) => BomPageAsked?.Invoke(tab, reading);

            // RACKBOMLEER took the last read (Started), as the command does before it reads.
            public void StartLastBomRead() => RequestStarted?.Invoke(BomReads.Last().RequestId);

            public void AnswerLastSelectiveRead() => SelectiveRead?.Invoke(SelectiveWindowFixtures.Read(SelectiveReads.Last().RequestId, SelectiveReads.Last().RackId));

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);
        }

        /// <summary>The files of the page, in memory: what was written, opened, shown and saved as.</summary>
        private sealed class FilesDouble : IRackBomFiles
        {
            public Dictionary<string, byte[]> Written { get; } = new(StringComparer.OrdinalIgnoreCase);

            public List<string> Opened { get; } = new();

            public List<string> Shown { get; } = new();

            public List<(string Name, string Extension, string? Folder)> SaveAsCalls { get; } = new();

            public string? SaveAsAnswer { get; set; }

            public Exception? WriteFault { get; set; }

            /// <summary>Files that are there before the page writes anything.</summary>
            public HashSet<string> Existing { get; } = new(StringComparer.OrdinalIgnoreCase);

            public string FallbackFolder => @"C:\Usuarios\Documentos";

            public bool Exists(string path) => Existing.Contains(path) || Written.ContainsKey(path);

            public void Write(string path, byte[] content)
            {
                if (WriteFault != null) throw WriteFault;
                Written[path] = content;
            }

            public void Open(string path) => Opened.Add(path);

            public void ShowInFolder(string path) => Shown.Add(path);

            public string SaveAs(string suggestedName, string extension, string initialFolder)
            {
                SaveAsCalls.Add((suggestedName, extension, initialFolder));
                return SaveAsAnswer!;
            }
        }

        private sealed class Rig
        {
            public Rig(bool withDoor = true)
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[]
                    {
                        new BrowserRackListEntry(RackId, "Selectivo A", "selective", new[] { "frontal" }),
                        new BrowserRackListEntry("DIN-1", "Dinámico 1", "dynamic", new[] { "lateral", "planta" }),
                        new BrowserRackListEntry("PB-1", "Push Back 1", "pushback", new[] { "lateral" }),
                        new BrowserRackListEntry("CAMA-1", "Cama 1", "cama", new[] { "lateral" }),
                    }),
                };
                Navigation = new WorkspaceNavigationPortDouble();
                Panel = new WorkspacePanelViewModel(Host, _ => { }, Navigation, Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, withDoor ? Door : null, UiSupport.LoadCatalogSafe);
                Window.Bom.Files = Files;
                Window.Bom.Clock = () => Now;
                Window.Refresh();
            }

            public DateTime Now { get; set; } = At;

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspaceNavigationPortDouble Navigation { get; }

            public WorkspacePanelViewModel Panel { get; }

            public BomDoor Door { get; } = new();

            public FilesDouble Files { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public RackBomPageViewModel Bom => Window.Bom;

            // T6 st (the Owner, 2026-11-27: «Afinarlo»; review of 5114345e, M-1): what the host presents of each drawing, the changes of its
            // database and the commands that started in it. A command of these tests changes the drawing while it runs (a MOVE), unless it says
            // it does not (a ZOOM); the bridge counts its start after the panel presented it (CommandWillStart).
            private readonly Dictionary<long, (long Changes, int Starts)> _counts = new();
            private bool _running;

            private (long Changes, int Starts) CountsOf(long session) => _counts.TryGetValue(session, out var counts) ? counts : (0, 0);

            /// <summary>Commands ran in the drawing while nothing was presented (the palette hidden), changing it or not.</summary>
            public void CommandsWhilePaletteHidden(long session = 7, bool changesTheDrawing = true)
            {
                var (changes, starts) = CountsOf(session);
                _counts[session] = (changes + (changesTheDrawing ? 1 : 0), starts + 1);
            }

            public void Present(long session = 7, WorkspaceActionBlock block = WorkspaceActionBlock.None, bool changesTheDrawing = true)
            {
                var (changes, starts) = CountsOf(session);
                Host.Snapshot = new WorkspaceHostSnapshot(WorkspaceHostStatus.Ready, new SessionId(session), true,
                    new WorkspaceActionAvailability(block == WorkspaceActionBlock.None, false, block), SelectionContext.None, false, false, changes, starts);
                Window.Refresh();
                var runs = block == WorkspaceActionBlock.CommandActive;
                _counts[session] = (changes + (runs && changesTheDrawing ? 1 : 0), starts + (runs && !_running ? 1 : 0));
                _running = runs;
            }

            // The page shown (a click), RACKBOMLEER taking the request and answering, and the drawing at rest again once it ended.
            public RackBomReading Open(RackBomReading? reading = null)
            {
                Window.ShowPage(RackCadWindowPage.Bom);
                var answer = reading ?? Drawing();
                Door.StartLastBomRead();
                Door.AnswerLastBomRead(answer);
                Present();
                return answer;
            }

            // «Editar aquí» of the Selectivo of the drawing and a change in its draft (not written).
            public SelectiveEditorViewModel EditSelective()
            {
                Window.OpenRack(RackId);
                Window.EditSelectiveHere();
                Door.AnswerLastSelectiveRead();
                var editor = Window.ActiveSelectiveEditor!;
                Assert.NotNull(editor);
                Write(Field(editor.CellFields, "cHeight"), "72");
                Assert.True(editor.IsDirty);
                return editor;
            }
        }

        // ------------------------------------------------------------------ the readings of the tests

        private static BomLine Line(string category, string id, int qty, double length = 0, string? description = null)
            => new() { Category = category, ProfileId = id, Quantity = qty, Length = length, Description = description ?? category };

        private static BomComponent Comp(string category, double length, int qty, string description, params BomLine[] pieces)
            => new() { Category = category, ProfileId = category.ToUpperInvariant(), Description = description, Length = length, Quantity = qty, Pieces = pieces.ToList() };

        private static BillOfMaterials Selectivo() => new(new List<BomComponent>
        {
            Comp("Cabecera", 216, 3, "Cabecera 216\" · fondo 42\"", Line("Poste", "POSTE", 2, 216), Line("Diagonal", "TRAV", 7, 52)),
            Comp("Larguero", 96, 12, "Larguero escalón · P4", Line("Perfil", "LARG", 1, 96), Line("Ménsula", "MEN", 2)),
        });

        private static BillOfMaterials Dinamico() => new(new List<BomComponent>
        {
            Comp("Cabecera", 150, 6, "Cabecera 150\" · fondo 42\"", Line("Poste", "POSTE", 2, 150)),
            Comp("Cama", 144, 6, "Cama · BFR 2\""),
        });

        private static BillOfMaterials PushBack() => new(new List<BomComponent>
        {
            Comp("Cama", 144, 9, "Cama · BFR 2.5\""),
            Comp("Larguero", 96, 9, "Larguero IN/OUT · P6", Line("Perfil", "INOUT", 1, 96)),
        });

        private static BillOfMaterials Cama() => new(new List<BomLine> { Line("Riel", "RIEL", 2, 96), Line("Rodillo", "ROD", 22, 40) });

        private static RackBomRack Rack(string id, string name, string kind, int copies, BillOfMaterials? bom, RackBomRackState state = RackBomRackState.Quoted,
            string? reason = null, string views = "frontal, lateral, planta")
            => new(id, name, kind, RackBomReadingRules.KindLabelOf(kind), views, copies, state, reason, bom);

        private static RackBomReading Reading(string? path, params RackBomRack[] racks)
        {
            var refusals = racks.Where(r => r.BlocksTheTotal).Select(r => new RackBomNotice(r.RackId, r.Name, r.Reason!)).ToList();
            var quoted = racks.Where(r => r.IsQuoted)
                .Select(r => new ConsolidatedRackBom { Name = r.Name, Kind = r.KindLabel, Copies = r.Copies, Bom = r.Bom }).ToList();
            var total = refusals.Count == 0 && quoted.Count > 0 ? ConsolidatedBomBuilder.Build(quoted) : null;
            return new RackBomReading(0, path, path == null ? "Drawing1" : RackBomFiles.NameOf(path), null, racks, refusals, total);
        }

        // Six racks as the prototype: a Selectivo, a Dinámico (3 copies), a Push Back and a Cama (4 copies).
        private static RackBomReading Drawing(string? path = @"C:\Proyectos\Planta ejemplo.dwg") => Reading(path,
            Rack("DIN-1", "Dinámico 1", "dynamic", 3, Dinamico(), views: "lateral, planta"),
            Rack(RackId, "Selectivo A", "selective", 1, Selectivo(), views: "frontal"),
            Rack("CAMA-1", "Cama 1", "cama", 4, Cama(), views: "lateral"),
            Rack("PB-1", "Push Back 1", "pushback", 1, PushBack(), views: "lateral"));

        // ------------------------------------------------------------------ BM-01: the read, its number and «Dejar de esperar»

        [Fact]
        public void TheFirstTimeThePageIsShownForADrawingItAsksAutoCadToReadItAndOnlyOneRequestWaits()
        {
            var rig = new Rig();
            Assert.Empty(rig.Door.BomReads);
            Assert.Equal("Sin leer", rig.Bom.ChipText);

            rig.Window.ShowPage(RackCadWindowPage.Bom);
            var request = Assert.Single(rig.Door.BomReads);
            Assert.Equal(Session, request.Session);
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Window.CanStopWaiting);
            Assert.StartsWith("Esperando a que AutoCAD lea el dibujo para el BOM.", rig.Window.WaitingText);
            Assert.Equal("Leyendo el dibujo", rig.Bom.ChipText);
            Assert.Equal("Escribiendo", rig.Bom.ChipKind);
            Assert.True(rig.Bom.ShowReading);
            Assert.Equal("Se pidió la lectura n.º " + request.RequestId + " con RACKBOMLEER, una orden sin marca de Deshacer. AutoCAD sigue disponible; esta página conserva lo que tenía hasta que llegue la respuesta.",
                rig.Bom.ReadingText);
            Assert.False(rig.Bom.CanRecalculate);
            Assert.False(rig.Bom.CanExportDrawing);

            // Showing the page again asks nothing more, and the one request of the window is shared with the editors.
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            Assert.Single(rig.Door.BomReads);
            rig.Window.OpenRack(RackId);
            rig.Window.EditSelectiveHere();
            Assert.Empty(rig.Door.SelectiveReads);
            Assert.StartsWith("AutoCAD todavía no respondió a la petición anterior de la ventana", rig.Window.ActionMessage);
        }

        [Fact]
        public void OnlyTheAnswerToThatRequestFillsThePage()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            var id = rig.Door.BomReads.Last().RequestId;

            rig.Door.Read(Drawing() with { RequestId = id + 5 });
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.Empty(rig.Bom.Rows);

            rig.Door.Read(Drawing() with { RequestId = id });
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("Al día", rig.Bom.ChipText);
            Assert.Equal("Limpio", rig.Bom.ChipKind);
            Assert.Equal("Leído a las 10:42", rig.Bom.ReadText);
            Assert.Equal(4, rig.Bom.Rows.Count);
            Assert.Equal("Racks del dibujo (4)", rig.Bom.RacksTabLabel);
            Assert.Equal(("4", "9", "Los 4 racks del dibujo."), (rig.Bom.StatRacks, rig.Bom.StatCopies, rig.Bom.ShowText));
            Assert.Equal("de 4 en el dibujo", rig.Bom.StatRacksHint);
            Assert.True(rig.Bom.CanExportDrawing);
            Assert.Contains("Planta ejemplo", rig.Bom.SubText);
        }

        // AutoCAD may run the command, and answer, during the call (M-22): the request waits before it is handed over.
        [Fact]
        public void AnAnswerDuringTheCallIsTheAnswerOfTheRequest()
        {
            var rig = new Rig();
            rig.Door.DuringBomRead = request =>
            {
                rig.Door.StartLastBomRead();
                rig.Door.Read(Drawing() with { RequestId = request.RequestId });
            };
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("Al día", rig.Bom.ChipText);
            Assert.Equal(4, rig.Bom.Rows.Count);
        }

        [Fact]
        public void DejarDeEsperarForgetsTheReadAndALateAnswerChangesNothing()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            var id = rig.Door.BomReads.Last().RequestId;

            rig.Bom.StopWaiting();
            Assert.Equal(new[] { id }, rig.Door.Abandoned);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("Se dejó de esperar la lectura n.º " + id + " del BOM: si la orden llega a correr, no cambiará nada.", rig.Bom.Message);
            Assert.Equal("Sin leer", rig.Bom.ChipText);
            Assert.True(rig.Bom.CanRecalculate);

            rig.Door.Read(Drawing() with { RequestId = id });
            Assert.Empty(rig.Bom.Rows);
            Assert.Equal("Sin leer", rig.Bom.ChipText);
        }

        // Correction 4 of the review: no command leaves the window without a gesture of the user. A click while AutoCAD is busy says so; a
        // presentation of the host later sends nothing on its own; the next click reads.
        [Fact]
        public void ABusyAutoCadIsSaidAndNothingIsSentLaterWithoutAClick()
        {
            var rig = new Rig();
            rig.Door.BomAnswer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            Assert.Single(rig.Door.BomReads);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("AutoCAD está ocupado (Hay un comando en curso): termina o cancela la orden en curso y pulsa «Calcular».", rig.Bom.Message);
            Assert.Equal("El dibujo todavía no se leyó para el BOM. Pulsa «Calcular»: AutoCAD lo lee sin bloquearse.", rig.Bom.UnreadText);
            Assert.Equal("Calcular", rig.Bom.RecalculateLabel);

            rig.Door.BomAnswer = RackCadClassicResult.Started();
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present();
            rig.Present();
            Assert.Single(rig.Door.BomReads); // nothing on its own

            rig.Bom.Recalculate();
            Assert.Equal(2, rig.Door.BomReads.Count);
            Assert.True(rig.Window.IsWaitingForAutoCad);
        }

        // BM-01 and correction 1 of the review: after any command that is not the read of the page, the BOM may be out of date. The read is
        // its own command only from the moment RACKBOMLEER says it took the request (Started) until the drawing is at rest after its answer.
        [Fact]
        public void ACommandAfterTheReadingSaysItMayBeOutOfDateAndOnlyTheReadThatStartedIsNotOne()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            rig.Present(block: WorkspaceActionBlock.CommandActive); // RACKBOMLEER begins
            rig.Door.StartLastBomRead();
            Assert.False(rig.Window.CanStopWaiting); // the command took it: its answer comes
            Assert.Equal("AutoCAD está leyendo el dibujo para el BOM: la respuesta llega al terminar la lectura.", rig.Window.WaitingText);
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Door.AnswerLastBomRead(Drawing());
            rig.Present(block: WorkspaceActionBlock.CommandActive); // still RACKBOMLEER, after it answered
            Assert.Equal("Al día", rig.Bom.ChipText);
            Assert.Equal("Calcular de nuevo", rig.Bom.RecalculateLabel);
            rig.Present();
            Assert.Equal("Al día", rig.Bom.ChipText);
            Assert.False(rig.Bom.ShowStale);

            rig.Now = At.AddMinutes(9);
            rig.Present(block: WorkspaceActionBlock.CommandActive); // a MOVE of the user
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText);
            Assert.Equal("Modificado", rig.Bom.ChipKind);
            Assert.True(rig.Bom.ShowStale);
            Assert.Equal("Leído a las 10:42 · corrió una orden a las 10:51", rig.Bom.ReadText);
            Assert.Equal("El BOM es de las 10:42 y a las 10:51 corrió una orden en el dibujo. Se vuelve a leer solo cuando lo pides, para no bloquear AutoCAD con cada orden.",
                rig.Bom.StaleText);
            Assert.True(rig.Bom.RecalculateIsPrimary);
            Assert.Single(rig.Door.BomReads); // not on its own

            rig.Present();
            rig.Bom.Recalculate();
            Assert.Equal(2, rig.Door.BomReads.Count);
            rig.Now = At.AddMinutes(12);
            rig.Door.StartLastBomRead();
            rig.Door.AnswerLastBomRead(Drawing());
            Assert.Equal("Al día", rig.Bom.ChipText);
            Assert.Equal("Leído a las 10:54", rig.Bom.ReadText);

            // A command seen after a read was handed over but before RACKBOMLEER took it is no read of the page: the reading the drawing has
            // may be out of date (if the read never runs, it stays so).
            rig.Present();
            rig.Bom.Recalculate();
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            Assert.Equal("Leído a las 10:54 · corrió una orden a las 10:54", rig.Bom.ReadText);
            rig.Bom.StopWaiting();
            Assert.True(rig.Bom.ShowStale);
            Assert.StartsWith("El BOM es de las 10:54 y a las 10:54 corrió una orden en el dibujo.", rig.Bom.StaleText);
        }

        // T6 st (the Owner, 2026-11-27: «Afinarlo»; review of 5114345e, I-3): while a command runs the BOM may be out of date (provisionally);
        // when it ends, a command that changed nothing of the drawing (a ZOOM, a PAN, a REGEN) leaves it «Al día» again, and a «U» or an ERASE
        // does not.
        [Fact]
        public void AZoomLeavesTheBomUpToDateAndAUOrAnEraseDoesNot()
        {
            var rig = new Rig();
            rig.Open();
            Assert.Equal("Al día", rig.Bom.ChipText);

            rig.Present(block: WorkspaceActionBlock.CommandActive, changesTheDrawing: false); // ZOOM
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText); // while it runs
            rig.Present();
            Assert.Equal("Al día", rig.Bom.ChipText);
            Assert.False(rig.Bom.ShowStale);
            Assert.False(rig.Bom.RecalculateIsPrimary);

            rig.Now = At.AddMinutes(3);
            rig.Present(block: WorkspaceActionBlock.CommandActive); // «U» (or ERASE)
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText);
            rig.Present();
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText);
            Assert.Equal("Leído a las 10:42 · corrió una orden a las 10:45", rig.Bom.ReadText);
            Assert.Single(rig.Door.BomReads); // nothing is read on its own
        }

        // Review of 5114345e, I-3: «Exportar» during a COPY that already appended its objects asks, as with any BOM that may be out of date.
        [Fact]
        public void ExportingDuringACopyThatRunsAsksAndTheMarkStaysWhenItEnds()
        {
            var rig = new Rig();
            rig.Open();
            rig.Present(block: WorkspaceActionBlock.CommandActive); // COPY: its objects are appended while it runs

            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Bom.HasAsk);
            Assert.Empty(rig.Files.Written);
            rig.Bom.AskCancel();

            rig.Present();
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText);
            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Bom.HasAsk);
        }

        // Review of 5114345e, M-1: a read of the page run together with another command (a script, a macro) is not only the read: when that
        // other command changed the drawing, the BOM may be out of date.
        [Fact]
        public void TheReadRunTogetherWithAnotherCommandThatChangedTheDrawingLeavesTheBomPossiblyOutOfDate()
        {
            var rig = new Rig();
            rig.Open();
            rig.Bom.Recalculate();
            rig.Present(block: WorkspaceActionBlock.CommandActive); // a command of a script (it changes the drawing)…
            rig.CommandsWhilePaletteHidden(); // …and RACKBOMLEER right after it, in the same run: one more command start
            rig.Door.StartLastBomRead();
            rig.Door.AnswerLastBomRead(Drawing());
            Assert.Equal("Al día", rig.Bom.ChipText); // the reading that answered
            rig.Present();
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText);
        }

        // Review of 5114345e, M-2 and A2: coming back to a drawing, or showing the palette again, says the BOM may be out of date only when
        // commands changed the drawing meanwhile.
        [Fact]
        public void ComingBackOrShowingThePaletteAgainSaysItOnlyWhenCommandsChangedTheDrawing()
        {
            var rig = new Rig();
            rig.Open();
            rig.Present(8);
            rig.Present(7);
            Assert.Equal("Al día", rig.Bom.ChipText);

            rig.Present(8);
            rig.CommandsWhilePaletteHidden(7); // the palette was hidden and the user went back to drawing 7 and ran an ERASE there
            rig.Present(7);
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText);
            Assert.StartsWith("Volviste a este dibujo", rig.Bom.StaleText);

            var again = new Rig();
            again.Open();
            again.CommandsWhilePaletteHidden(changesTheDrawing: false); // a ZOOM while the palette was hidden
            again.Present();
            Assert.Equal("Al día", again.Bom.ChipText);
            again.CommandsWhilePaletteHidden(); // an ERASE while the palette was hidden
            again.Present();
            Assert.Equal("Puede no estar al día", again.Bom.ChipText);
            Assert.StartsWith("El BOM es de las 10:42 y a las 10:42 corrió una orden en el dibujo.", again.Bom.StaleText);
        }

        // Correction 1 of the review (the BOM that came out «Al día» and was exported with no question): a read that is abandoned, or that is
        // not handed over, leaves nothing counted as the read, and the reading the drawing had is said to be possibly out of date.
        [Fact]
        public void AReadThatIsAbandonedOrNotHandedOverLeavesTheReadingPossiblyOutOfDateAndExportAsks()
        {
            var rig = new Rig();
            rig.Open();
            rig.Bom.Recalculate();
            rig.Bom.StopWaiting();
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText);
            Assert.StartsWith("El BOM es de las 10:42 y una lectura posterior no se completó", rig.Bom.StaleText);
            rig.Present(block: WorkspaceActionBlock.CommandActive); // a MOVE: nothing counts as the read any more
            rig.Present();
            Assert.True(rig.Bom.ShowStale);
            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Bom.HasAsk);
            Assert.Empty(rig.Files.Written);

            // The read not handed over (AutoCAD busy).
            var busy = new Rig();
            busy.Open();
            busy.Door.BomAnswer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            busy.Bom.Recalculate();
            Assert.True(busy.Bom.ShowStale);
            busy.Bom.ExportDrawing(excel: false);
            Assert.True(busy.Bom.HasAsk);
        }

        [Fact]
        public void TheReadingsAreKeptPerDrawingAndComingBackToOneSaysItMayBeOutOfDate()
        {
            var rig = new Rig();
            rig.Open();

            // The page shown for another drawing, never read: correction 4, changing drawing is no gesture, so nothing is sent.
            rig.Present(8);
            Assert.True(rig.Window.IsBomPage);
            Assert.Single(rig.Door.BomReads);
            Assert.Equal("Sin leer", rig.Bom.ChipText);
            Assert.Equal("Calcular", rig.Bom.RecalculateLabel);
            Assert.Empty(rig.Bom.Rows);
            rig.Bom.Recalculate();
            Assert.Equal(new SessionId(8), rig.Door.BomReads.Last().Session);
            rig.Door.AnswerLastBomRead(Reading(@"C:\P\Otra.dwg", Rack("DIN-1", "Dinámico 1", "dynamic", 1, Dinamico())));
            Assert.Single(rig.Bom.Rows);

            rig.Present(7);
            Assert.Equal(4, rig.Bom.Rows.Count);
            Assert.Equal("Al día", rig.Bom.ChipText); // T6 st (review of 5114345e, M-2): nothing ran in it meanwhile

            rig.Present(8);
            rig.CommandsWhilePaletteHidden(7); // commands changed drawing 7 while the window presented 8
            rig.Present(7);
            Assert.Equal("Puede no estar al día", rig.Bom.ChipText);
            Assert.StartsWith("Volviste a este dibujo: el BOM es de las 10:42", rig.Bom.StaleText);

            // A drawing that was closed forgets its reading, and a read that waited for it is dropped.
            rig.Bom.Recalculate();
            Assert.True(rig.Window.IsWaitingForAutoCad);
            rig.Summary.Dead.Add(Session);
            rig.Present(8);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Contains(rig.Door.BomReads.Last().RequestId, rig.Door.Abandoned);
            rig.Summary.Dead.Clear();
            var reads = rig.Door.BomReads.Count;
            rig.Present(7);
            Assert.Empty(rig.Bom.Rows);
            Assert.Equal("Sin leer", rig.Bom.ChipText);
            Assert.Equal(reads, rig.Door.BomReads.Count);
        }

        // Correction 5 of the review: the answer is kept only for the drawing of the session of its request. The window pulls the state of
        // the host when it arrives (inside RACKBOMLEER, in the drawing that is active).
        [Fact]
        public void AnAnswerThatArrivesWithAnotherDrawingActiveIsNotUsed()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            var id = rig.Door.BomReads.Last().RequestId;
            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 8); // the active drawing changed, and the window was not told yet
            rig.Door.StartLastBomRead();
            rig.Door.AnswerLastBomRead(Drawing());

            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("La lectura n.º " + id + " no es del dibujo para el que se pidió (el dibujo activo cambió): no se usó. Pulsa «Calcular» en el dibujo que quieras.",
                rig.Bom.Message);
            Assert.Empty(rig.Bom.Rows);
            rig.Present(7);
            Assert.Equal("Sin leer", rig.Bom.ChipText);
            Assert.Empty(rig.Bom.Rows);
        }

        // ------------------------------------------------------------------ BM-05: what prevents the total, and what stays out

        [Fact]
        public void ABlockedRackShowsItsReasonWithOpenAndNoTotalButTheTableKeepsTheBomOfTheOthers()
        {
            var rig = new Rig();
            rig.Open(Reading(@"C:\P\Planta.dwg",
                Rack("DIN-1", "Dinámico 1", "dynamic", 1, Dinamico()),
                Rack("PB-1", "Push Back 1", "pushback", 1, null, RackBomRackState.Blocked, "su cama pide más longitud de la que tiene la estructura.")));

            Assert.Equal("Sin total", rig.Bom.ChipText);
            Assert.Equal("Error", rig.Bom.ChipKind);
            Assert.True(rig.Bom.HasRefusals);
            Assert.Equal("1 rack tiene un problema que impide cotizarlo. Un total al que le falta un rack no puede parecer completo: no hay total ni exportación del dibujo hasta corregirlo.",
                rig.Bom.RefusalHead);
            var refusal = Assert.Single(rig.Bom.Refusals);
            Assert.Equal(("Push Back 1", "Abrir Push Back 1", true), (refusal.Name, refusal.OpenLabel, refusal.CanOpen));
            Assert.Equal("Abrir Push Back 1 (impide el total, 1)", refusal.OpenAutomationName);
            Assert.False(rig.Bom.CanExportDrawing);
            Assert.False(rig.Bom.HasTotal);
            Assert.StartsWith("No hay total: corrige lo que dice el aviso de arriba", rig.Bom.NoTotalText);
            Assert.Equal(("—", "—"), (rig.Bom.StatRacks, rig.Bom.StatPieces));
            Assert.True(rig.Bom.RecalculateIsPrimary);

            var pb = rig.Bom.Rows.Single(r => r.RackId == "PB-1");
            Assert.Equal(("Bloqueado", "—", "—", "Obsoleto"), (pb.Tag, pb.Pieces, pb.InDrawing, pb.StatusKind));
            var din = rig.Bom.Rows.Single(r => r.RackId == "DIN-1");
            Assert.Equal("12", din.Pieces);

            rig.Bom.Select("PB-1");
            Assert.Equal("No se puede cotizar: su cama pide más longitud de la que tiene la estructura. Ábrelo, corrígelo y pulsa «Calcular de nuevo».", rig.Bom.BlockedNote);
            Assert.False(rig.Bom.HasRackBom);

            rig.Bom.OpenRack(refusal.RackId);
            Assert.True(rig.Window.IsRackPage);
            Assert.Equal("PB-1", rig.Panel.Browser.ActiveRackId);
        }

        [Fact]
        public void ARackThatStaysOutOfTheTotalIsNamedAboveItAndTheTotalIsMadeWithoutIt()
        {
            var rig = new Rig();
            rig.Open(Reading(@"C:\P\Planta.dwg",
                Rack("DIN-1", "Dinámico 1", "dynamic", 1, null, RackBomRackState.NoSingleDesign, "no tiene un diseño único entre sus vistas y queda fuera del total."),
                Rack("PB-1", "Push Back 1", "pushback", 1, PushBack())));

            Assert.True(rig.Bom.HasSkipped);
            Assert.Equal("1 rack queda fuera del total", rig.Bom.SkippedHead);
            Assert.Equal("Dinámico 1", Assert.Single(rig.Bom.Skipped).Name);
            Assert.True(rig.Bom.HasTotal);
            Assert.Equal("1", rig.Bom.StatRacks);
            Assert.Equal("de 2 en el dibujo", rig.Bom.StatRacksHint);
            Assert.Equal("Fuera del total", rig.Bom.Rows.Single(r => r.RackId == "DIN-1").Tag);
            rig.Bom.Select("DIN-1");
            Assert.StartsWith("Queda fuera del total: no tiene un diseño único entre sus vistas", rig.Bom.OutNote);
            Assert.EndsWith("ábrelo con RACKEDITAR desde la vista que quieras conservar.", rig.Bom.OutNote);
        }

        [Fact]
        public void AReadingThatFailedSaysWhyAndShowsNoTable()
        {
            var rig = new Rig();
            rig.Open(RackBomReading.Failed(0, null, "Drawing1", "No hay BOM: el registro de variables del dibujo existe y no se puede leer."));
            Assert.Equal("No se pudo leer", rig.Bom.ChipText);
            Assert.True(rig.Bom.HasFailure);
            Assert.False(rig.Bom.ShowContent);
            Assert.Empty(rig.Bom.Rows);
            Assert.False(rig.Bom.CanExportDrawing);
        }

        // ------------------------------------------------------------------ BM-05, BM-09: the table follows the side column

        [Fact]
        public void TheTableIsInTheOrderOfTheSideColumnAndAHeaderOrdersIt()
        {
            var rig = new Rig();
            rig.Open();
            Assert.Equal(new[] { RackId, "PB-1", "DIN-1", "CAMA-1" }, rig.Bom.Rows.Select(r => r.RackId)); // Selectivo, Push Back, Dinámico, Cama
            Assert.Equal("Sistema ▲", rig.Bom.KindHeader);

            rig.Bom.SortBy(RackBomPageViewModel.SortPieces);
            Assert.Equal("Piezas ▼", rig.Bom.PiecesHeader);
            var pieces = rig.Bom.Rows.Select(r => int.Parse(r.Pieces.Replace(((char)0x00A0).ToString(), string.Empty))).ToList();
            Assert.Equal(pieces.OrderByDescending(p => p).ToList(), pieces);

            rig.Bom.SortBy(RackBomPageViewModel.SortName);
            Assert.Equal(new[] { "Cama 1", "Dinámico 1", "Push Back 1", "Selectivo A" }, rig.Bom.Rows.Select(r => r.Name));
            rig.Bom.SortBy(RackBomPageViewModel.SortName);
            Assert.Equal(new[] { "Selectivo A", "Push Back 1", "Dinámico 1", "Cama 1" }, rig.Bom.Rows.Select(r => r.Name));

            rig.Bom.SortBy(RackBomPageViewModel.SortCopies);
            Assert.Equal("CAMA-1", rig.Bom.Rows[0].RackId); // 4 copies first
        }

        [Fact]
        public void TheTableFollowsTheSearchAndTheFiltersOfTheSideColumnAndTheTotalStaysThatOfTheDrawing()
        {
            var rig = new Rig();
            rig.Open();

            rig.Window.RackList.SearchText = "dinamico"; // without the accent, as the search of the column
            Assert.Equal(new[] { "DIN-1" }, rig.Bom.Rows.Select(r => r.RackId));
            Assert.Equal("Mostrando 1 de 4, con el filtro de la columna lateral. El total sigue siendo el del dibujo completo.", rig.Bom.ShowText);
            Assert.Equal("4", rig.Bom.StatRacks);

            rig.Window.RackList.SearchText = string.Empty;
            rig.Window.RackList.SelectedKind = "pushback";
            Assert.Equal(new[] { "PB-1" }, rig.Bom.Rows.Select(r => r.RackId));

            rig.Window.RackList.SelectedKind = string.Empty;
            rig.Window.RackList.SearchText = "nada";
            Assert.Empty(rig.Bom.Rows);
            Assert.Equal("Ningún rack coincide con el filtro de la columna.", rig.Bom.NoRowsText);
        }

        [Fact]
        public void TheRowsSayComponentsAndPiecesOfOneCopyAndInTheDrawingAndAFlatRackIsAPiezaSuelta()
        {
            var rig = new Rig();
            rig.Open();
            var din = rig.Bom.Rows.Single(r => r.RackId == "DIN-1");
            Assert.Equal(("Dinámico 1", "lateral, planta", "Dinámico", "3", "12", "12", "36"), (din.Name, din.Views, din.Kind, din.Copies, din.Components, din.Pieces, din.InDrawing));
            var cama = rig.Bom.Rows.Single(r => r.RackId == "CAMA-1");
            Assert.Equal(("Cama", "pieza suelta", "24", "96"), (cama.Kind, cama.Components, cama.Pieces, cama.InDrawing));
            Assert.NotNull(cama.ComponentsTip);
            Assert.Equal("Push Back", rig.Bom.Rows.Single(r => r.RackId == "PB-1").Kind);
            Assert.Equal(rig.Bom.Rows.Count, rig.Bom.Rows.Select(r => r.AutomationName).Distinct().Count());
            Assert.StartsWith("Rack 3 de 4: Dinámico 1: Dinámico, 3 copias", din.AutomationName); // the position, for two racks with one name
        }

        // ------------------------------------------------------------------ the BOM of a rack in the inspector

        [Fact]
        public void TheInspectorUnfoldsSeveralComponentsAndSaysEachPiecePerUnitAndPerCopy()
        {
            var rig = new Rig();
            rig.Open();
            Assert.Equal("Elige un rack de la tabla para ver su BOM.", rig.Bom.NoSelectionText);

            rig.Bom.SelectedRackId = RackId;
            Assert.Equal("BOM de Selectivo A", rig.Bom.InspectorTitle);
            Assert.Equal("Selectivo · frontal · 1 copia", rig.Bom.InspectorSub);
            Assert.True(rig.Bom.IsTree);
            Assert.Equal("15 componentes y 63 piezas por copia · 1 copia en el dibujo.", rig.Bom.RackSummary);
            Assert.False(rig.Bom.HasNextCopy);
            var components = rig.Bom.Components;
            Assert.Equal(2, components.Count);
            Assert.Equal(("Cabecera", "216 in", "× 3"), (components[0].Category, components[0].Length, components[0].Quantity));

            components[0].IsExpanded = true;
            components[1].IsExpanded = true;
            Assert.True(rig.Bom.Components.All(c => c.IsExpanded)); // several at once (dolor 9)
            var diagonal = components[0].Pieces.Single(p => p.Category == "Diagonal");
            Assert.Equal(("7", "21", "52"), (diagonal.Each, diagonal.Amount, diagonal.Length));
            Assert.Equal("—", components[1].Pieces.Single(p => p.Category == "Ménsula").Length);
            Assert.Equal("Plegar componente 1: Cabecera Cabecera 216\" · fondo 42\"", components[0].ToggleName);

            // The unfolded components are remembered per rack.
            rig.Bom.Select("DIN-1");
            Assert.Equal("12 componentes y 12 piezas por copia · × 3 copias = 36 piezas en el dibujo.", rig.Bom.RackSummary);
            Assert.True(rig.Bom.HasNextCopy);
            Assert.DoesNotContain(rig.Bom.Components, c => c.IsExpanded);
            Assert.True(rig.Bom.Components.Single(c => c.Category == "Cama").NoPiecesText.Length > 0);
            rig.Bom.ExpandAll();
            Assert.True(rig.Bom.Components.All(c => c.IsExpanded));
            rig.Bom.Select(RackId);
            Assert.True(rig.Bom.Components.All(c => c.IsExpanded));
            rig.Bom.CollapseAll();
            Assert.DoesNotContain(rig.Bom.Components, c => c.IsExpanded);

            rig.Bom.Select("CAMA-1");
            Assert.True(rig.Bom.IsFlat);
            Assert.Equal("Pieza suelta: su BOM son sus piezas. En el total entra como un componente «Cama» por copia.", rig.Bom.FlatNote);
            Assert.Equal(new[] { ("Riel", "2"), ("Rodillo", "22") }, rig.Bom.FlatLines.Select(l => (l.Category, l.Amount)));
            Assert.Equal("24 piezas por copia · × 4 copias = 96 piezas en el dibujo.", rig.Bom.RackSummary);
        }

        // BM-02 / SD-02 (the BOM of the draft of the classic Selectivo): «Escrito en el dibujo» or «Borrador del editor», computed here.
        [Fact]
        public void ARackWithADraftOfTheWindowShowsTheBomOfTheDrawingOrOfTheDraftComputedHere()
        {
            var rig = new Rig();
            var editor = rig.EditSelective();
            rig.Open();

            Assert.True(rig.Bom.HasDrafts);
            Assert.Equal("Selectivo A tiene cambios sin escribir en el editor de la ventana: aquí cuenta lo escrito en el dibujo. Su borrador se ve en su BOM con «Borrador del editor».",
                rig.Bom.DraftsText);
            Assert.Equal(("Con cambios", "Modificado"), (rig.Bom.Rows.Single(r => r.RackId == RackId).Tag, rig.Bom.Rows.Single(r => r.RackId == RackId).StatusKind));

            rig.Bom.ShowFirstDraft();
            Assert.Equal(RackId, rig.Bom.SelectedRackId);
            Assert.True(rig.Bom.HasDraftChoice);
            Assert.True(rig.Bom.ShowsDrawing);
            Assert.Equal("Lo escrito en el dibujo, como el total. El editor de la ventana tiene cambios sin escribir.", rig.Bom.SourceNote);
            Assert.StartsWith("15 componentes", rig.Bom.RackSummary);

            rig.Bom.ShowDraftBom();
            Assert.True(rig.Bom.ShowsDraft);
            Assert.Equal("Calculado en la ventana con el borrador del editor, sin leer el dibujo. No entra en el total.", rig.Bom.SourceNote);
            var expected = SelectiveBomBuilder.Build(editor.Draft.ResolveCopy(), editor.Draft.Catalog);
            Assert.EndsWith("piezas por copia · del borrador: no entra en el total.", rig.Bom.RackSummary);
            Assert.StartsWith(expected.TotalComponents + " componentes y " + expected.TotalPieces + " piezas", rig.Bom.RackSummary.Replace(((char)0x00A0).ToString(), string.Empty));
            Assert.Equal(expected.Components.Count, rig.Bom.Components.Count);
            Assert.Contains("(borrador)", rig.Bom.ExportRackHint);
            var before = new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(editor.Draft.Design, null, null));

            // Exporting what is seen: the draft, with «(borrador)» in the name, and with no question even when the drawing may have changed.
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present();
            Assert.True(rig.Bom.ShowStale);
            rig.Bom.ExportRack(excel: true);
            Assert.False(rig.Bom.HasAsk);
            var file = Assert.Single(rig.Files.Written);
            Assert.Equal(@"C:\Proyectos\Planta ejemplo - Selectivo A - BOM (borrador).xlsx", file.Key);
            Assert.Equal(BomXlsxExporter.ToXlsx(expected), file.Value);
            Assert.Equal("Junto al dibujo. Una hoja «Lista de materiales», como hoy (D-2 de I-68), del borrador del editor (no está escrito en el dibujo).", rig.Bom.ExportedDetail);

            // Seeing the BOM of the draft changes nothing of the draft.
            Assert.True(editor.IsDirty);
            Assert.Equal(before, new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(editor.Draft.Design, null, null)));

            // Once the draft is written or discarded there is no choice: the page shows the drawing.
            editor.Discard();
            Assert.False(editor.IsDirty);
            Assert.False(rig.Bom.HasDraftChoice);
            Assert.False(rig.Bom.ShowsDraft);
            Assert.False(rig.Bom.HasDrafts);
            Assert.Equal(string.Empty, rig.Bom.Rows.Single(r => r.RackId == RackId).Tag);
        }

        // ------------------------------------------------------------------ BM-08: the total of the drawing

        [Fact]
        public void TheTotalSaysWhereEachPartidaComesFromAndMarksTheOnesThatJoinDescriptions()
        {
            var rig = new Rig();
            rig.Open();
            rig.Bom.ShowTotalTab();
            Assert.True(rig.Bom.IsTotalTab);
            Assert.True(rig.Bom.HasTotal);

            var cama = rig.Bom.TotalItems.Single(t => t.Category == "Cama" && t.Length == "144");
            Assert.True(cama.JoinsDescriptions);
            Assert.Equal("Dinámico 1, Push Back 1", cama.Racks);
            Assert.Equal("Sale de: Dinámico 1: 6 × 3 copias = 18 · Push Back 1: 9", cama.From);
            Assert.Equal("27", cama.Quantity); // the sum does not change: 6 × 3 + 9
            Assert.StartsWith("Esta partida junta descripciones distintas: «Cama · BFR 2\"» y «Cama · BFR 2.5\"».", cama.JoinNote);
            var flat = rig.Bom.TotalItems.Single(t => t.Description == "Cama 1"); // a flat rack is one component of its system per copy
            Assert.Equal(("Cama", "4", "Cama 1"), (flat.Category, flat.Quantity, flat.Racks));
            Assert.False(flat.JoinsDescriptions);

            var options = rig.Bom.CategoryOptions.Select(o => o.Label).ToList();
            Assert.StartsWith("Todos (", options[0]);
            rig.Bom.SelectedCategory = "Larguero";
            Assert.All(rig.Bom.TotalItems, t => Assert.Equal("Larguero", t.Category));
            Assert.Contains(" de ", rig.Bom.TotalShownText);
            rig.Bom.SelectedCategory = string.Empty;

            rig.Bom.ExpandAllTotal();
            Assert.True(rig.Bom.TotalItems.All(t => t.IsExpanded));
            var larguero = rig.Bom.TotalItems.First(t => t.Category == "Larguero");
            Assert.Equal(larguero.Quantity, larguero.Pieces.Single(p => p.Category == "Perfil").Amount);
            rig.Bom.CollapseAllTotal();
            Assert.DoesNotContain(rig.Bom.TotalItems, t => t.IsExpanded);
            Assert.Contains("piezas en 9 copias de 4 racks", rig.Bom.TotalFooterText);
            Assert.Equal(rig.Bom.TotalItems.Count, rig.Bom.TotalItems.Select(t => t.ToggleName).Distinct().Count());
        }

        // ------------------------------------------------------------------ BM-03, BM-04: the files, next to the drawing, with no dialog

        [Fact]
        public void ExportingTheDrawingSavesNextToItWithItsNameAndOffersOpenTheFolderAndSaveAs()
        {
            var rig = new Rig();
            var reading = rig.Open();

            rig.Bom.ExportDrawing(excel: true);
            var path = @"C:\Proyectos\Planta ejemplo - BOM.xlsx";
            Assert.Equal(ConsolidatedBomXlsxExporter.ToXlsx(reading.Total), rig.Files.Written[path]);
            Assert.True(rig.Bom.HasExported);
            Assert.Equal("Se guardó «Planta ejemplo - BOM.xlsx»", rig.Bom.ExportedTitle);
            Assert.Equal("Junto al dibujo. Tres hojas: «Por rack» (tabla RackCadBOM, con «Cantidad total»), «Total del dibujo» y «Resumen» (tabla dinámica ResumenBOM). Del BOM de las 10:42.",
                rig.Bom.ExportedDetail);
            Assert.Empty(rig.Files.SaveAsCalls); // no dialog unless asked

            rig.Bom.OpenExported();
            rig.Bom.ShowExportedInFolder();
            Assert.Equal(new[] { path }, rig.Files.Opened);
            Assert.Equal(new[] { path }, rig.Files.Shown);

            rig.Files.SaveAsAnswer = @"D:\Cotizaciones\otra.xlsx";
            rig.Bom.SaveExportedAs();
            Assert.Equal(("Planta ejemplo - BOM.xlsx", ".xlsx", @"C:\Proyectos"), Assert.Single(rig.Files.SaveAsCalls));
            Assert.Equal(rig.Files.Written[path], rig.Files.Written[@"D:\Cotizaciones\otra.xlsx"]);
            Assert.Equal("Se guardó «otra.xlsx»", rig.Bom.ExportedTitle);

            // The CSV of the drawing, with «Cantidad_total» (BM-04).
            rig.Bom.ExportDrawing(excel: false);
            var csv = Encoding.UTF8.GetString(rig.Files.Written[@"C:\Proyectos\Planta ejemplo - BOM.csv"]);
            Assert.StartsWith("Rack,Tipo,Copias,Nivel,Categoria,Perfil,Descripcion,Longitud_in,Cantidad,Cantidad_total", csv);
            Assert.Equal(ConsolidatedBomCsvExporter.ToCsv(reading.Total), csv);

            rig.Bom.CloseExported();
            Assert.False(rig.Bom.HasExported);

            // A rack of the drawing.
            rig.Bom.Select("DIN-1");
            rig.Bom.ExportRack(excel: false);
            Assert.Equal(BomCsvExporter.ToCsv(Dinamico()), Encoding.UTF8.GetString(rig.Files.Written[@"C:\Proyectos\Planta ejemplo - Dinámico 1 - BOM.csv"]));
        }

        [Fact]
        public void ADrawingNeverSavedExportsToTheDocumentsAndAFailureOffersSaveAs()
        {
            var rig = new Rig();
            rig.Open(Drawing(path: null));
            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Files.Written.ContainsKey(@"C:\Usuarios\Documentos\Drawing1 - BOM.xlsx"));
            Assert.StartsWith("En Documentos, porque el dibujo no está guardado todavía.", rig.Bom.ExportedDetail);

            // The second file does not replace the first one in the documents (correction 3): its name is numbered.
            rig.Files.WriteFault = new System.IO.IOException("El archivo está en uso.");
            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Bom.ExportedFailed);
            Assert.Equal("No se pudo guardar «Drawing1 - BOM (2).xlsx»", rig.Bom.ExportedTitle);
            Assert.Contains("El archivo está en uso.", rig.Bom.ExportedDetail);
            Assert.Contains("«Guardar como…»", rig.Bom.ExportedDetail);
            Assert.False(rig.Bom.CanOpenExported);
        }

        // Exporting a BOM that may be out of date asks in line: read again and export, export what was read, or cancel.
        [Fact]
        public void ExportingWhenTheBomMayBeOutOfDateAsksInLine()
        {
            var rig = new Rig();
            rig.Open();
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present();

            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Bom.HasAsk);
            Assert.Equal("¿Exportar el BOM de las 10:42?", rig.Bom.AskTitle);
            Assert.Equal("Exportar el de las 10:42", rig.Bom.AskExportLabel);
            Assert.Empty(rig.Files.Written);
            rig.Bom.AskCancel();
            Assert.False(rig.Bom.HasAsk);

            rig.Bom.ExportDrawing(excel: true);
            rig.Bom.AskExportAnyway();
            Assert.Single(rig.Files.Written);

            // «Calcular y exportar»: the file is made with the reading that answers.
            rig.Files.Written.Clear();
            rig.Bom.ExportDrawing(excel: false);
            rig.Bom.AskRecalculateAndExport();
            Assert.Equal(2, rig.Door.BomReads.Count);
            Assert.Empty(rig.Files.Written);
            rig.Now = At.AddMinutes(20);
            var fresh = Reading(@"C:\Proyectos\Planta ejemplo.dwg", Rack("DIN-1", "Dinámico 1", "dynamic", 1, Dinamico()));
            rig.Door.AnswerLastBomRead(fresh);
            Assert.Equal(ConsolidatedBomCsvExporter.ToCsv(fresh.Total), Encoding.UTF8.GetString(rig.Files.Written[@"C:\Proyectos\Planta ejemplo - BOM.csv"]));
            Assert.EndsWith("Del BOM de las 11:02.", rig.Bom.ExportedDetail);
            rig.Present(); // RACKBOMLEER ended

            // With a refused total, the read made nothing.
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present();
            rig.Files.Written.Clear();
            rig.Bom.ExportDrawing(excel: true);
            rig.Bom.AskRecalculateAndExport();
            rig.Door.AnswerLastBomRead(Reading(null, Rack("PB-1", "Push Back 1", "pushback", 1, null, RackBomRackState.Blocked, "motivo.")));
            Assert.Empty(rig.Files.Written);
            Assert.Equal("No se exportó: la lectura nueva no tiene total del dibujo (lo dice el aviso).", rig.Bom.Message);
        }

        // Correction 2 of the review: «Calcular y exportar» arms its export BEFORE the read is handed over, so an answer during the call
        // exports at once; nothing stays armed to go off later, also when the read is not handed over, and «Calcular» alone never exports.
        [Fact]
        public void AnExportThatWaitsForTheReadIsMadeOnceAndNeverGoesOffLater()
        {
            var rig = new Rig();
            rig.Open();
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present();
            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Bom.HasAsk);
            rig.Door.DuringBomRead = request =>
            {
                rig.Door.StartLastBomRead();
                rig.Door.Read(Drawing() with { RequestId = request.RequestId });
            };
            rig.Bom.AskRecalculateAndExport();
            Assert.Single(rig.Files.Written); // exported with the answer of the call

            rig.Files.Written.Clear();
            rig.Door.DuringBomRead = null;
            rig.Present();
            rig.Bom.Recalculate();
            rig.Door.StartLastBomRead();
            rig.Door.AnswerLastBomRead(Drawing());
            Assert.Empty(rig.Files.Written); // nothing armed went off

            // A read that is not handed over arms nothing either.
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present();
            rig.Bom.ExportDrawing(excel: false);
            rig.Door.BomAnswer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            rig.Bom.AskRecalculateAndExport();
            Assert.Empty(rig.Files.Written);
            rig.Door.BomAnswer = RackCadClassicResult.Started();
            rig.Bom.Recalculate();
            rig.Door.AnswerLastBomRead(Drawing());
            Assert.Empty(rig.Files.Written);
        }

        // Correction 3 of the review: a file of the user is never lost. In the documents (a drawing never saved: every one is «Drawing1»)
        // an existing file is not replaced; two racks whose names give the same file get the start of their identity.
        [Fact]
        public void AFileOfTheUserIsNeverReplacedInTheDocumentsAndTwoRacksWithOneNameGetTwoFiles()
        {
            var rig = new Rig();
            rig.Open(Drawing(path: null));
            rig.Files.Existing.Add(@"C:\Usuarios\Documentos\Drawing1 - BOM.xlsx");
            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Files.Written.ContainsKey(@"C:\Usuarios\Documentos\Drawing1 - BOM (2).xlsx"));
            Assert.Equal("Se guardó «Drawing1 - BOM (2).xlsx»", rig.Bom.ExportedTitle);
            Assert.Contains("Ya había un archivo con ese nombre: este se guardó como «Drawing1 - BOM (2).xlsx».", rig.Bom.ExportedDetail);
            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Files.Written.ContainsKey(@"C:\Usuarios\Documentos\Drawing1 - BOM (3).xlsx"));

            // Next to a saved drawing, the file of the same name is the earlier export of the same BOM: it is replaced.
            var saved = new Rig();
            saved.Open();
            saved.Files.Existing.Add(@"C:\Proyectos\Planta ejemplo - BOM.xlsx");
            saved.Bom.ExportDrawing(excel: true);
            Assert.True(saved.Files.Written.ContainsKey(@"C:\Proyectos\Planta ejemplo - BOM.xlsx"));

            // Two racks named «PB/1» and «PB:1» clean to the same file name: each gets the start of its identity.
            var twins = new Rig();
            twins.Open(Reading(@"C:\Proyectos\Planta ejemplo.dwg",
                Rack("a1b2c3d4-0000-0000-0000-000000000001", "PB/1", "pushback", 1, PushBack()),
                Rack("ffee0011-0000-0000-0000-000000000002", "PB:1", "pushback", 1, PushBack())));
            twins.Bom.Select("a1b2c3d4-0000-0000-0000-000000000001");
            twins.Bom.ExportRack(excel: true);
            twins.Bom.Select("ffee0011-0000-0000-0000-000000000002");
            twins.Bom.ExportRack(excel: true);
            Assert.Equal(new[] { @"C:\Proyectos\Planta ejemplo - PB_1 (a1b2c3d4) - BOM.xlsx", @"C:\Proyectos\Planta ejemplo - PB_1 (ffee0011) - BOM.xlsx" },
                twins.Files.Written.Keys.OrderBy(k => k, StringComparer.Ordinal));
            Assert.Contains("«Planta ejemplo - PB_1 (ffee0011) - BOM.xlsx»", twins.Bom.ExportRackHint);
        }

        // Correction 3 of the review: the files of the machine are written atomically, as AtomicFile does (a temporary file in the same
        // folder, then Replace or Move): a file replaced has the new bytes, and no temporary file is left.
        [Fact]
        public void TheFilesOfTheMachineAreWrittenAtomically()
        {
            var folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "rackcad-bom-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(folder);
            try
            {
                var files = new RackBomFileShell();
                var path = System.IO.Path.Combine(folder, "Planta - BOM.xlsx");
                Assert.False(files.Exists(path));
                files.Write(path, new byte[] { 1, 2, 3 });
                Assert.True(files.Exists(path));
                files.Write(path, new byte[] { 4, 5 });
                Assert.Equal(new byte[] { 4, 5 }, System.IO.File.ReadAllBytes(path));
                Assert.Equal(new[] { path }, System.IO.Directory.GetFiles(folder));
                Assert.ThrowsAny<Exception>(() => files.Write(System.IO.Path.Combine(folder, "no-existe", "x.xlsx"), new byte[] { 1 }));
                Assert.Equal(new[] { path }, System.IO.Directory.GetFiles(folder));
            }
            finally
            {
                System.IO.Directory.Delete(folder, true);
            }

            // What the disk cannot show in a test (an interrupted write), the source says: a temporary file in the same folder, swapped in.
            var dir = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            var code = System.IO.File.ReadAllText(System.IO.Path.Combine(dir!.FullName, "src", "RackCad.UI", "VentanaRackCad", "RackBomFiles.cs"));
            var write = code.Substring(code.IndexOf("public void Write(string path, byte[] content)", StringComparison.Ordinal));
            write = write.Substring(0, write.IndexOf("public bool Exists(string path)", StringComparison.Ordinal));
            Assert.Contains("Path.Combine(string.IsNullOrEmpty(folder) ? \".\" : folder, Path.GetFileName(path) + \".tmp-\" + Guid.NewGuid().ToString(\"N\"))", write);
            Assert.Contains("File.WriteAllBytes(temp, ", write);
            Assert.Contains("if (File.Exists(path)) File.Replace(temp, path, null);", write);
            Assert.Contains("else File.Move(temp, path);", write);
            Assert.DoesNotContain("File.WriteAllBytes(path", write);
        }

        // ------------------------------------------------------------------ BM-10, BM-11 and the navigation

        // BM-11 and correction 4 of the review: RACKLISTA and RACKBOMTOTAL read the drawing inside their own command and hand the reading to
        // the page, for the drawing active now; the window sends no second command, and the rest of their own command is no change.
        [Fact]
        public void RackListaAndRackBomTotalHandTheirReadingToThePageOnTheirTabWithNoSecondCommand()
        {
            var rig = new Rig();
            rig.Host.Snapshot = new WorkspaceHostSnapshot(WorkspaceHostStatus.Ready, Session, true,
                new WorkspaceActionAvailability(false, false, WorkspaceActionBlock.CommandActive), SelectionContext.None, false, false);
            rig.Door.Ask(RackBomTab.Total, Drawing()); // RACKBOMTOTAL, while it runs
            Assert.True(rig.Window.IsBomPage);
            Assert.True(rig.Bom.IsTotalTab);
            Assert.Empty(rig.Door.BomReads);
            Assert.Equal("Al día", rig.Bom.ChipText);
            Assert.Equal(4, rig.Bom.Rows.Count);
            rig.Present(block: WorkspaceActionBlock.CommandActive); // the rest of RACKBOMTOTAL
            Assert.Equal("Al día", rig.Bom.ChipText);
            rig.Present();
            Assert.Empty(rig.Door.BomReads);

            // RACKLISTA later, with another reading: its tab and its reading.
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Door.Ask(RackBomTab.Racks, Reading(@"C:\Proyectos\Planta ejemplo.dwg", Rack("PB-1", "Push Back 1", "pushback", 2, PushBack())));
            Assert.True(rig.Window.IsBomPage);
            Assert.True(rig.Bom.IsRacksTab);
            Assert.Equal(new[] { "PB-1" }, rig.Bom.Rows.Select(r => r.RackId));
            Assert.Empty(rig.Door.BomReads);

            // A command that failed to read still opens the page, and says why.
            rig.Door.Ask(RackBomTab.Total, RackBomReading.Failed(0, null, "Drawing1", "No hay BOM: no se pudo cargar el catálogo de RackCad."));
            Assert.True(rig.Bom.HasFailure);
            Assert.Empty(rig.Door.BomReads);
        }

        [Fact]
        public void VerSuBomOfThePageOfARackOpensThePageWithThatRackChosen()
        {
            var rig = new Rig();
            Assert.False(rig.Window.CanShowBomOfRack);
            rig.Window.OpenRack("DIN-1");
            Assert.True(rig.Window.CanShowBomOfRack);

            rig.Window.ShowBomOfActiveRack();
            Assert.True(rig.Window.IsBomPage);
            Assert.True(rig.Bom.IsRacksTab);
            Assert.Equal("DIN-1", rig.Bom.SelectedRackId);
            rig.Door.AnswerLastBomRead(Drawing());
            Assert.Equal("BOM de Dinámico 1", rig.Bom.InspectorTitle);
        }

        [Fact]
        public void VerEnElDibujoAndSiguienteCopiaUseTheNavigationOfTheRackAndStayOnThePage()
        {
            var rig = new Rig();
            rig.Open();
            rig.Bom.Select("DIN-1");

            rig.Bom.ViewSelected();
            Assert.True(rig.Window.IsBomPage);
            Assert.Equal("DIN-1", rig.Panel.Browser.ActiveRackId);
            var call = Assert.Single(rig.Navigation.Calls);
            Assert.Equal(("DIN-1", NavigationActionKind.View, 0), (call.RackId, call.Action, call.CopyIndex));
            Assert.True(rig.Bom.HasMessage);

            rig.Bom.NextCopySelected();
            Assert.Equal(1, rig.Navigation.Calls.Last().CopyIndex);
            Assert.True(rig.Window.IsBomPage);

            // Another rack: it becomes the active one, still on this page.
            rig.Bom.Select("PB-1");
            rig.Bom.ViewSelected();
            Assert.Equal(("PB-1", 0), (rig.Navigation.Calls.Last().RackId, rig.Navigation.Calls.Last().CopyIndex));
            Assert.True(rig.Window.IsBomPage);
        }

        [Fact]
        public void WithoutTheDoorOfTheBomThePageSaysItCannotRead()
        {
            var rig = new Rig(withDoor: false);
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            Assert.False(rig.Bom.CanRecalculate);
            Assert.Equal("Aquí no se puede leer el dibujo.", rig.Bom.UnreadText);
            rig.Bom.Recalculate();
            Assert.Equal("Aquí no se puede leer el dibujo.", rig.Bom.Message);
        }

        [Fact]
        public void TheWordsAndTheOrderOfTheSystemsAreTheSameInTheSideColumnAndInThePage()
        {
            Assert.Equal(RackListViewModel.KindOrder, RackBomReadingRules.KindOrder);
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

        private static void WithBindingErrors(Action body)
        {
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

            Assert.Empty(errors.Messages);
        }

        private static void Lay(FrameworkElement view)
        {
            view.Measure(new Size(1300, 1400));
            view.Arrange(new Rect(0, 0, 1300, 1400));
            view.UpdateLayout();
        }

        private static void Click(ButtonBase button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

        // Shown on the page: it and every element above it are visible (IsVisible needs a window).
        private static bool Shown(DependencyObject element)
        {
            for (var current = element; current != null; current = System.Windows.Media.VisualTreeHelper.GetParent(current))
                if (current is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            return true;
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

        [Fact]
        public void ThePageBuildsWithNoBindingErrorAndItsButtonsDoWhatTheySay()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var editor = rig.EditSelective();
                var view = new RackBomPageView(rig.Bom);
                Lay(view);
                Assert.Equal("Sin leer", view.ChipText.Text);
                Assert.Equal(Visibility.Visible, view.UnreadBanner.Visibility);

                rig.Window.ShowPage(RackCadWindowPage.Bom);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.ReadingBanner.Visibility);
                Assert.Equal("Dejar de esperar la lectura del BOM", Name(view.BomStopWaitingButton));
                Assert.False(view.RecalculateButton.IsEnabled);
                rig.Door.AnswerLastBomRead(Drawing());
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.ReadingBanner.Visibility);
                Assert.Equal("Al día", view.ChipText.Text);
                Assert.True(view.RecalculateButton.IsEnabled);
                Assert.True(view.ExportExcelButton.IsEnabled);
                Assert.Equal(Visibility.Visible, view.DraftsBanner.Visibility);
                Assert.Equal(4, view.RackTable.Items.Count);

                // A row chosen in the table: its BOM, the choice of the draft and a component unfolded with its toggle.
                view.RackTable.SelectedValue = RackId;
                Lay(view);
                Assert.Equal(RackId, rig.Bom.SelectedRackId);
                Assert.Equal("BOM de Selectivo A", view.InspectorTitle.Text);
                Click(view.SourceDraftButton);
                Lay(view);
                Assert.True(rig.Bom.ShowsDraft);
                Click(view.SourceDrawingButton);
                Lay(view);
                var toggles = Descendants<ToggleButton>(view.ComponentList).ToList();
                Assert.Equal(2, toggles.Count);
                Assert.Equal(toggles.Count, toggles.Select(Name).Distinct().Count());
                ((IToggleProvider)UIElementAutomationPeer.CreatePeerForElement(toggles[1]).GetPattern(PatternInterface.Toggle)).Toggle();
                Lay(view);
                Assert.True(rig.Bom.Components[1].IsExpanded);

                // The export of the drawing, with its notice and no dialog.
                Click(view.ExportExcelButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.ExportedBanner.Visibility);
                Assert.Equal("Se guardó «Planta ejemplo - BOM.xlsx»", view.ExportedTitle.Text);
                Click(view.OpenExportedButton);
                Assert.Single(rig.Files.Opened);

                // The tabs and the header that orders the table.
                Click(view.TotalTabButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.TotalTab.Visibility);
                Assert.Equal(Visibility.Collapsed, view.RacksTab.Visibility);
                Assert.True(view.TotalList.Items.Count > 0);
                Click(view.RacksTabButton);
                Click(view.SortNameButton);
                Lay(view);
                Assert.Equal("Rack y vistas ▲", view.SortNameButton.Content);

                // A new list of rows (an order, a reading, a filter, a presentation of the host) keeps the rack chosen in the table.
                Assert.Equal(RackId, rig.Bom.SelectedRackId);
                Assert.Equal(RackId, view.RackTable.SelectedValue);
                rig.Present();
                rig.Window.RackList.SearchText = "selectivo";
                Lay(view);
                Assert.Equal(RackId, rig.Bom.SelectedRackId);
                Assert.Equal(RackId, view.RackTable.SelectedValue);
                rig.Window.RackList.SearchText = string.Empty;
                Lay(view);
                Assert.True(Shown(view.InspectorOpenButton));

                // Every button of the page has a name of its own.
                var names = Descendants<Button>(view).Where(Shown).Select(Name).ToList();
                Assert.True(names.Count > 20, names.Count.ToString() + ": " + string.Join(" | ", names));
                Assert.Equal(names.Count, names.Distinct().Count());
                GC.KeepAlive(editor);
            }));
        }

        // As the inspector of the editors (O-3 of the smoke of Dinámico): on a narrow page the BOM of the rack goes under the table.
        [Fact]
        public void OnANarrowPageTheBomOfTheRackGoesUnderTheTable()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                rig.Open();
                rig.Bom.Select("DIN-1");
                var view = new RackBomPageView(rig.Bom);
                Lay(view);
                Assert.False(view.InspectorBelow);
                Assert.Equal(1, Grid.GetColumn(view.InspectorBorder));

                view.Measure(new Size(600, 1400));
                view.Arrange(new Rect(0, 0, 600, 1400));
                view.UpdateLayout();
                Assert.True(view.InspectorBelow);
                Assert.Equal((1, 0), (Grid.GetRow(view.InspectorBorder), Grid.GetColumn(view.InspectorBorder)));
                Assert.True(double.IsNaN(view.InspectorBorder.Width));

                Lay(view);
                Assert.False(view.InspectorBelow);
                Assert.Equal(380, view.InspectorBorder.Width);
            }));
        }

        [Fact]
        public void TheWindowHostsThePageAndThePageOfARackOffersVerSuBom()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                Lay(view);
                Assert.IsType<RackBomPageView>(view.BomPage.Content);
                Assert.Same(rig.Bom, ((RackBomPageView)view.BomPage.Content).ViewModel);
                Assert.Equal(Visibility.Collapsed, view.ShowBomOfRackButton.Visibility);
                Assert.DoesNotContain(Descendants<Button>(view.BomPage), b => (b.Tag as string) == "TotalBom" || (b.Tag as string) == "RackList");

                rig.Window.OpenRack("PB-1");
                Lay(view);
                Assert.Equal(Visibility.Visible, view.ShowBomOfRackButton.Visibility);
                Assert.Equal("Ver el BOM de este rack", Name(view.ShowBomOfRackButton));
                Click(view.ShowBomOfRackButton);
                Lay(view);
                Assert.True(rig.Window.IsBomPage);
                Assert.Equal(Visibility.Visible, view.BomPage.Visibility);
                Assert.Equal("PB-1", rig.Bom.SelectedRackId);
                Assert.Single(rig.Door.BomReads);
            }));
        }
    }
}
