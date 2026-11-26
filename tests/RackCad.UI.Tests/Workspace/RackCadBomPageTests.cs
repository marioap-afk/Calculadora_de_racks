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

            public event Action<long>? RequestStarted { add { } remove { } }

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public event Action<RackBomReading>? BomRead;

            public event Action<RackBomTab>? BomPageAsked;

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

            public void Ask(RackBomTab tab) => BomPageAsked?.Invoke(tab);

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

            public string FallbackFolder => @"C:\Usuarios\Documentos";

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

            public void Present(long session = 7, WorkspaceActionBlock block = WorkspaceActionBlock.None)
            {
                Host.Snapshot = new WorkspaceHostSnapshot(WorkspaceHostStatus.Ready, new SessionId(session), true,
                    new WorkspaceActionAvailability(block == WorkspaceActionBlock.None, false, block), SelectionContext.None, false, false);
                Window.Refresh();
            }

            // The page shown, the answer of AutoCAD to its first read, and the drawing at rest again once RACKBOMLEER ended.
            public RackBomReading Open(RackBomReading? reading = null)
            {
                Window.ShowPage(RackCadWindowPage.Bom);
                var answer = reading ?? Drawing();
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
            rig.Door.DuringBomRead = request => rig.Door.Read(Drawing() with { RequestId = request.RequestId });
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

        // BM-01 and BM-11: busy AutoCAD (a command in progress, as RACKBOMTOTAL itself) leaves the read wanted, asked when the drawing is free.
        [Fact]
        public void ABusyAutoCadLeavesTheReadWantedAndThePageAsksItWhenTheDrawingIsFree()
        {
            var rig = new Rig();
            rig.Door.BomAnswer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            Assert.Single(rig.Door.BomReads);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("AutoCAD está ocupado: el BOM se leerá en cuanto termine la orden en curso.", rig.Bom.UnreadText);

            rig.Door.BomAnswer = RackCadClassicResult.Started();
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            Assert.Single(rig.Door.BomReads);
            rig.Present();
            Assert.Equal(2, rig.Door.BomReads.Count);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            // Off the page, a wanted read is not asked on its own.
            var other = new Rig();
            other.Door.BomAnswer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "x");
            other.Window.ShowPage(RackCadWindowPage.Bom);
            other.Window.ShowPage(RackCadWindowPage.Nuevo);
            other.Door.BomAnswer = RackCadClassicResult.Started();
            other.Present();
            Assert.Single(other.Door.BomReads);
        }

        // BM-01: after any command that is not the read of the page, the BOM may be out of date; it is read again only when asked.
        [Fact]
        public void ACommandAfterTheReadingSaysItMayBeOutOfDateAndTheReadItselfDoesNot()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            rig.Present(block: WorkspaceActionBlock.CommandActive); // RACKBOMLEER runs
            rig.Door.AnswerLastBomRead(Drawing());
            rig.Present(block: WorkspaceActionBlock.CommandActive); // still RACKBOMLEER, after it answered
            Assert.Equal("Al día", rig.Bom.ChipText);
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
            rig.Door.AnswerLastBomRead(Drawing());
            Assert.Equal("Al día", rig.Bom.ChipText);
            Assert.Equal("Leído a las 10:54", rig.Bom.ReadText);
        }

        [Fact]
        public void TheReadingsAreKeptPerDrawingAndComingBackToOneSaysItMayBeOutOfDate()
        {
            var rig = new Rig();
            rig.Open();

            // The page shown for another drawing, for the first time: it reads that one.
            rig.Present(8);
            Assert.True(rig.Window.IsBomPage);
            Assert.Equal(new SessionId(8), rig.Door.BomReads.Last().Session);
            Assert.Equal("Leyendo el dibujo", rig.Bom.ChipText);
            Assert.Empty(rig.Bom.Rows);
            rig.Door.AnswerLastBomRead(Reading(@"C:\P\Otra.dwg", Rack("DIN-1", "Dinámico 1", "dynamic", 1, Dinamico())));
            Assert.Single(rig.Bom.Rows);

            rig.Present(7);
            Assert.Equal(4, rig.Bom.Rows.Count);
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
            rig.Present(7);
            Assert.Empty(rig.Bom.Rows);
            Assert.Equal("Leyendo el dibujo", rig.Bom.ChipText); // read again, as a drawing never read
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

            rig.Files.WriteFault = new System.IO.IOException("El archivo está en uso.");
            rig.Bom.ExportDrawing(excel: true);
            Assert.True(rig.Bom.ExportedFailed);
            Assert.Equal("No se pudo guardar «Drawing1 - BOM.xlsx»", rig.Bom.ExportedTitle);
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

        // ------------------------------------------------------------------ BM-10, BM-11 and the navigation

        [Fact]
        public void RackListaAndRackBomTotalOpenThePageOnTheirTabAndReadAgain()
        {
            var rig = new Rig();
            rig.Door.BomAnswer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            rig.Door.Ask(RackBomTab.Total); // RACKBOMTOTAL, while it runs
            Assert.True(rig.Window.IsBomPage);
            Assert.True(rig.Bom.IsTotalTab);
            Assert.Single(rig.Door.BomReads);
            rig.Door.BomAnswer = RackCadClassicResult.Started();
            rig.Present(); // RACKBOMTOTAL ended: the drawing is free
            Assert.Equal(2, rig.Door.BomReads.Count);
            rig.Door.AnswerLastBomRead(Drawing());

            // RACKLISTA with a reading already there reads again (the command asks for it now).
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Door.Ask(RackBomTab.Racks);
            Assert.True(rig.Window.IsBomPage);
            Assert.True(rig.Bom.IsRacksTab);
            rig.Present();
            Assert.Equal(3, rig.Door.BomReads.Count);
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
