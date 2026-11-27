#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T6 st (I-64, sexta tanda; O-5 of the smoke of the fifth, M-16 of Dinámico; the Owner on 2026-11-27: «Avisar y ofrecer releer»): an
    /// editor of the window after a command that is not of the window. A command of AutoCAD that ran in the drawing of an open editor (a «U»,
    /// a RACKEDITAR, a MOVE…) marks the editor «posiblemente desactualizado»; the commands of the door of the window (the read of «Editar
    /// aquí», «Actualizar») do not. The six editors of the window: Selectivo, Dinámico, Push Back, Cantilever, cama and cabecera.
    /// </summary>
    public class RackCadDrawingChangeTests
    {
        // ------------------------------------------------------------------ the door of the six systems

        /// <summary>A door that reads and writes the six systems of the window, as the door of the Plugin does.</summary>
        private sealed class Door : IRackCadSelectiveDoorPort, IRackCadCabeceraDoorPort, IRackCadCamaDoorPort, IRackCadCantileverDoorPort,
            IRackCadPushBackDoorPort
        {
            public List<(string Kind, RackDoorRequest Request)> Reads { get; } = new();

            public List<(string Kind, long RequestId, string RackId)> Writes { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            /// <summary>What AutoCAD does DURING the call of a request, as if it ran the command at once (M-22).</summary>
            public Action<long>? During { get; set; }

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public event Action<CabeceraDesignRead>? CabeceraRead;

            public event Action<CamaDesignRead>? CamaRead;

            public event Action<CantileverDesignRead>? CantileverRead;

            public event Action<RackDesignRead>? PushBackRead;

            private RackCadClassicResult ReadOf(string kind, RackDoorRequest request)
            {
                Reads.Add((kind, request));
                if (Answer.IsStarted) During?.Invoke(request.RequestId);
                return Answer;
            }

            private RackCadClassicResult WriteOf(string kind, long requestId, string rackId)
            {
                Writes.Add((kind, requestId, rackId));
                if (Answer.IsStarted) During?.Invoke(requestId);
                return Answer;
            }

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => ReadOf(Dynamic, request);

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => WriteOf(Dynamic, write.RequestId, write.RackId);

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request) => ReadOf(Selective, request);

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write) => WriteOf(Selective, write.RequestId, write.RackId);

            public RackCadClassicResult RequestCabeceraDesign(RackDoorRequest request) => ReadOf(Cabecera, request);

            public RackCadClassicResult RequestCabeceraUpdate(CabeceraDoorWrite write) => WriteOf(Cabecera, write.RequestId, write.RackId);

            public RackCadClassicResult RequestCamaDesign(RackDoorRequest request) => ReadOf(Cama, request);

            public RackCadClassicResult RequestCamaWrite(CamaDoorWrite write) => WriteOf(Cama, write.RequestId, write.RackId);

            public RackCadClassicResult RequestCantileverDesign(RackDoorRequest request) => ReadOf(Cantilever, request);

            public RackCadClassicResult RequestCantileverUpdate(CantileverDoorWrite write) => WriteOf(Cantilever, write.RequestId, write.RackId);

            public RackCadClassicResult RequestPushBackDesign(RackDoorRequest request) => ReadOf(PushBack, request);

            public RackCadClassicResult RequestPushBackUpdate(PushBackDoorWrite write) => WriteOf(PushBack, write.RequestId, write.RackId);

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            /// <summary>What the «…ABRIR» command of the system answers to the last read: the rack of the drawing.</summary>
            public void AnswerLastRead(string name = "Rack 1")
            {
                var (kind, request) = Reads.Last();
                var id = request.RequestId;
                var rack = request.RackId;
                switch (kind)
                {
                    case Dynamic:
                        DesignRead?.Invoke(new RackDesignRead(id, rack, true, "dynamic", name, DynamicJson(), null));
                        break;
                    case Selective:
                        SelectiveRead?.Invoke(SelectiveWindowFixtures.Read(id, rack));
                        break;
                    case Cabecera:
                        CabeceraRead?.Invoke(CabeceraDoorRules.AnswerForOpen(id, rack, RackEmbedDocument.KindCabecera, name, CabeceraJson(name)));
                        break;
                    case Cama:
                        CamaRead?.Invoke(CamaDoorRules.AnswerForOpen(id, rack, RackEmbedDocument.KindCama, name, null, -1, CamaJson()));
                        break;
                    case Cantilever:
                        CantileverRead?.Invoke(CantileverDoorRules.AnswerForOpen(id, rack, RackEmbedDocument.KindCantilever, name, CantileverJson(name)));
                        break;
                    case PushBack:
                        PushBackRead?.Invoke(PushBackDoorRules.AnswerForOpen(id, rack, RackEmbedDocument.KindPushBack, name, PushBackJson()));
                        break;
                }
            }

            /// <summary>The answer of the «…APLICAR» command of the last write.</summary>
            public void AnswerLastWrite(bool written, string message = "AutoCAD redibujó el rack con el borrador.")
            {
                var write = Writes.Last();
                DesignWritten?.Invoke(new RackDesignWritten(write.RequestId, write.RackId, written, message));
            }
        }

        private const string Selective = "Selectivo";
        private const string Dynamic = "Dinámico";
        private const string PushBack = "Push Back";
        private const string Cantilever = "Cantilever";
        private const string Cama = "Cama";
        private const string Cabecera = "Cabecera";

        public static IEnumerable<object[]> Kinds => new[] { Selective, Dynamic, PushBack, Cantilever, Cama, Cabecera }.Select(k => new object[] { k });

        private static readonly IReadOnlyDictionary<string, string> RackOf = new Dictionary<string, string>
        {
            [Selective] = "SEL-1", [Dynamic] = "DIN-1", [PushBack] = "PB-1", [Cantilever] = "CAN-1", [Cama] = "CAMA-1", [Cabecera] = "CAB-1",
        };

        private static readonly BrowserRackListEntry[] Listed =
        {
            new("SEL-1", "Selectivo A", RackEmbedDocument.KindSelective, new[] { "frontal" }),
            new("DIN-1", "DIN 1", RackEmbedDocument.KindDynamic, new[] { "lateral" }),
            new("PB-1", "PB 1", RackEmbedDocument.KindPushBack, new[] { "lateral" }),
            new("CAN-1", "Cantilever 1", RackEmbedDocument.KindCantilever, new[] { "frontal" }),
            new("CAMA-1", "Cama 1", RackEmbedDocument.KindCama, new[] { "lateral" }),
            new("CAB-1", "Cabecera 1", RackEmbedDocument.KindCabecera, new[] { "lateral" }),
        };

        // ------------------------------------------------------------------ what the drawing holds (the fixtures of the tests of each editor)

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

        private static string CabeceraJson(string name)
        {
            var header = new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe())
                .Build(RackFrameTemplateCatalog.FindStandardOrDefault(), SelectiveWindowFixtures.PostId, 132.0, 42.0);
            header.Name = name;
            return new RackProjectStore().Serialize(RackProject.ForSelective(header));
        }

        private static string CamaJson() => new FlowBedConfigurationStore().Serialize(new FlowBedConfiguration
        {
            BedType = FlowBedType.Dynamic,
            LaneDepth = 96.0,
            PalletDepth = 48.0,
            RollerId = FlowBedDefaults.RollerId,
        });

        private static readonly Lazy<StructuralSectionCatalog> Sections =
            new(() => new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load());

        private static string CantileverJson(string name)
        {
            var draft = new CantileverDraftEditor(Sections.Value);
            Assert.Null(draft.Write(CantileverLineField.StationCount, "3"));
            Assert.Null(draft.Write(CantileverLineField.Spacing, "96"));
            Assert.Null(draft.Write(CantileverLineField.LevelCount, "3"));
            Assert.Null(draft.Write(CantileverLineField.ClearHeight, "24"));
            Assert.Null(draft.Write(CantileverLineField.ColumnSection, "AISC-W-W10X33"));
            Assert.Null(draft.Write(CantileverLineField.BaseSection, "AISC-W-W12X26"));
            Assert.Null(draft.Write(CantileverLineField.BaseLength, "48"));
            Assert.Null(draft.WriteArm(null, CantileverArmField.Section, "AISC-HSS-RECT-HSS4X4X_250"));
            Assert.Null(draft.WriteArm(null, CantileverArmField.CutLength, "36"));
            Assert.True(draft.IsResolved);
            draft.SetName(name);
            return new RackProjectStore().Serialize(RackProject.ForCantilever(draft.Design));
        }

        private static string PushBackJson()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            var text = File.ReadAllText(Path.Combine(dir!.FullName, "tests", "RackCad.Tests", "Fixtures", "I64", "pb-01-base.json"));
            return new RackEmbedStore().Deserialize(text)!.Design;
        }

        // ------------------------------------------------------------------ the window

        private sealed class Rig
        {
            public Rig()
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble { ListResponder = (_, _) => BrowserRackList.Ready(Listed) };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, Door, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public Door Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            // The changes of the database each session received (WorkspaceSession.DrawingChangeCount), as the host presents them.
            private readonly Dictionary<long, long> _changes = new();

            /// <summary>The database of the drawing of a session changes (an object appended, erased, modified…).</summary>
            public void ChangeTheDatabase(long session = 7) => _changes[session] = ChangesOf(session) + 1;

            private long ChangesOf(long session) => _changes.TryGetValue(session, out var count) ? count : 0;

            /// <summary>What the host presents: the drawing of a session, at rest or with a command running, and the changes of its database.</summary>
            public void Present(long session = 7, WorkspaceActionBlock block = WorkspaceActionBlock.None)
            {
                Host.Snapshot = new WorkspaceHostSnapshot(WorkspaceHostStatus.Ready, new SessionId(session), true,
                    new WorkspaceActionAvailability(block == WorkspaceActionBlock.None, false, block), SelectionContext.None, false, false,
                    ChangesOf(session));
                Window.Refresh();
            }

            /// <summary>
            /// A command of AutoCAD that is not of the window: its start, the changes of the database it makes (a «U», an ERASE, RACKEDITAR) or
            /// none (a ZOOM, a PAN, a REGEN), then the drawing at rest again.
            /// </summary>
            public void ForeignCommand(long session = 7, bool changesTheDrawing = true)
            {
                Present(session, WorkspaceActionBlock.CommandActive);
                if (changesTheDrawing) ChangeTheDatabase(session);
                Present(session);
            }

            /// <summary>
            /// The door runs its command as AutoCAD does: the start of the command (CommandWillStart presents the drawing with a command
            /// running), the answer inside the command (a write changes the database), and the drawing at rest at its end.
            /// </summary>
            public void DoorRunsItsCommands(Action<long> answer, long session = 7, bool changesTheDrawing = false)
            {
                Door.During = id =>
                {
                    Present(session, WorkspaceActionBlock.CommandActive);
                    if (changesTheDrawing) ChangeTheDatabase(session);
                    answer(id);
                    Present(session);
                };
            }

            public RackListItem Item(string rack) => Window.RackList.Groups.SelectMany(g => g.Items).Single(i => i.RackId == rack);

            /// <summary>«Editar aquí» of the rack of the system and the answer of AutoCAD with the rack of the drawing.</summary>
            public object Open(string kind)
            {
                Window.OpenRack(RackOf[kind]);
                EditHere(kind);
                Door.AnswerLastRead();
                var editor = Active(kind);
                Assert.True(editor != null, Window.ActionMessage);
                return editor!;
            }

            public void EditHere(string kind)
            {
                switch (kind)
                {
                    case Selective: Window.EditSelectiveHere(); break;
                    case Dynamic: Window.EditDynamicHere(); break;
                    case PushBack: Window.EditPushBackHere(); break;
                    case Cantilever: Window.EditCantileverHere(); break;
                    case Cama: Window.EditCamaHere(); break;
                    case Cabecera: Window.EditCabeceraHere(); break;
                }
            }

            public object? Active(string kind) => kind switch
            {
                Selective => Window.ActiveSelectiveEditor,
                Dynamic => Window.ActiveDynamicEditor,
                PushBack => Window.ActivePushBackEditor,
                Cantilever => Window.ActiveCantileverEditor,
                Cama => Window.ActiveCamaEditor,
                _ => Window.ActiveCabeceraEditor,
            };

            public int WritesOf(string kind) => Door.Writes.Count(w => w.Kind == kind);
        }

        // A change of the draft that every editor has: its name.
        private static void Change(object editor)
        {
            dynamic e = editor;
            e.Name = "Cambiado en la ventana";
            Assert.True((bool)e.IsDirty);
        }

        private static void RequestWrite(object editor) => ((dynamic)editor).RequestWrite();

        // ------------------------------------------------------------------ the signal

        private static RackEditorDrawingChange ChangeOf(object editor) => (RackEditorDrawingChange)((dynamic)editor).DrawingChange;

        // ------------------------------------------------------------------ the signal

        [Theory]
        [MemberData(nameof(Kinds))]
        public void AForeignCommandMarksTheOpenEditorOfItsDrawingPossiblyOutdatedInTheEditorAndInTheList(string kind)
        {
            var rig = new Rig();
            var editor = rig.Open(kind);
            var rack = RackOf[kind];
            Assert.Equal("Abierto en la ventana", rig.Item(rack).StatusText);
            Assert.False(ChangeOf(editor).IsChanged);

            // The command starts and changes the database (a «U»): nothing is said yet. It ends: the editor and the list say it.
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.ChangeTheDatabase();
            Assert.False(ChangeOf(editor).IsChanged);
            rig.Present();

            Assert.True(ChangeOf(editor).IsChanged);
            Assert.Equal(RackEditorDrawingChange.CleanText, ChangeOf(editor).Text);
            Assert.StartsWith("El dibujo cambió desde que se leyó (corrió una orden en AutoCAD).", ChangeOf(editor).Text);
            Assert.Equal("Abierto en la ventana (posiblemente obsoleto)", rig.Item(rack).StatusText);
            Assert.Equal("Obsoleto", rig.Item(rack).StatusKind);

            // The mark reads nothing by itself: no order leaves the window without a gesture of the user.
            Assert.Single(rig.Door.Reads);
            Assert.False(rig.Window.IsWaitingForAutoCad);
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void ActualizarOfADraftWithChangesAfterAForeignCommandWritesNothingWithoutAsking(string kind)
        {
            var rig = new Rig();
            var editor = rig.Open(kind);
            Change(editor);
            rig.ForeignCommand();

            RequestWrite(editor);

            Assert.Equal(0, rig.WritesOf(kind));
            Assert.Equal("Modificado (editor de la ventana) (posiblemente obsoleto)", rig.Item(RackOf[kind]).StatusText);
        }

        [Fact]
        public void TheCommandsOfTheDoorOfTheWindowDoNotMarkAndAForeignOneInAnotherDrawingDoesNotEither()
        {
            var rig = new Rig();

            // «Editar aquí»: RACKDINAMICOABRIR runs (the drawing presents a command) and answers inside its command.
            rig.DoorRunsItsCommands(_ => rig.Door.AnswerLastRead());
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            var editor = Assert.IsType<DynamicEditorViewModel>(rig.Active(Dynamic));
            Assert.Equal("Abierto en la ventana", rig.Item("DIN-1").StatusText);

            // «Actualizar»: RACKDINAMICOAPLICAR takes the request, writes (the database changes) and answers inside its command.
            rig.DoorRunsItsCommands(_ =>
            {
                rig.Door.Start(rig.Door.Writes.Last().RequestId);
                rig.Door.AnswerLastWrite(true);
            }, changesTheDrawing: true);
            Change(editor);
            editor.RequestWrite();
            Assert.Equal(1, rig.WritesOf(Dynamic));
            Assert.False(editor.IsDirty);
            Assert.False(editor.DrawingChange.IsChanged);
            Assert.Equal("Abierto en la ventana", rig.Item("DIN-1").StatusText);

            // «Editar aquí» of another rack of the same drawing (a read of the door) does not mark the open editor either.
            rig.DoorRunsItsCommands(_ => rig.Door.AnswerLastRead("Selectivo A"));
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere();
            Assert.NotNull(rig.Window.ActiveSelectiveEditor);
            Assert.False(editor.DrawingChange.IsChanged);

            // A command in another drawing marks the editors of that drawing only.
            rig.Door.During = null;
            rig.ForeignCommand(session: 8);
            rig.Present(7);
            Assert.False(editor.DrawingChange.IsChanged);
            Assert.Equal("Abierto en la ventana", rig.Item("DIN-1").StatusText);

            // And a command in its own drawing does.
            rig.ForeignCommand();
            Assert.True(editor.DrawingChange.IsChanged);
            Assert.True(rig.Window.ActiveSelectiveEditor!.DrawingChange.IsChanged);
        }

        [Fact]
        public void ACommandThatDidNotTakeTheRequestOfTheDoorThatWaitedIsForeign()
        {
            var rig = new Rig();
            var selective = (SelectiveEditorViewModel)rig.Open(Selective);

            // «Editar aquí» of DIN-1 waits (its command has not run: the command line had text), and the user runs a «U».
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            Assert.True(rig.Window.IsWaitingForAutoCad);
            rig.ForeignCommand();
            Assert.True(selective.DrawingChange.IsChanged);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            // The same when the window stops waiting while that command runs.
            var second = new Rig();
            var other = (SelectiveEditorViewModel)second.Open(Selective);
            second.Window.OpenRack("DIN-1");
            second.Window.EditDynamicHere();
            second.Present(block: WorkspaceActionBlock.CommandActive);
            second.ChangeTheDatabase();
            second.Window.StopWaiting();
            second.Present();
            Assert.True(other.DrawingChange.IsChanged);
        }

        [Fact]
        public void AModalOfRackCadCountsAsACommandAndANewDraftOfNuevoIsNeverMarked()
        {
            var rig = new Rig();
            var editor = (DynamicEditorViewModel)rig.Open(Dynamic);
            rig.Window.StartNewDynamic();
            var draft = rig.Window.NewDynamicEditor!;
            Assert.True(draft.IsNewRack);

            rig.Present(block: WorkspaceActionBlock.ModalActive);
            rig.ChangeTheDatabase();
            rig.Present();

            Assert.True(editor.DrawingChange.IsChanged);
            Assert.False(draft.DrawingChange.IsChanged);
        }

        // The Owner on 2026-11-27 («Afinarlo»): only a command that changes the drawing says it may be out of date.
        [Theory]
        [MemberData(nameof(Kinds))]
        public void AZoomThatChangesNothingDoesNotMarkAndAUOrAnEraseDoes(string kind)
        {
            var rig = new Rig();
            var editor = rig.Open(kind);

            rig.ForeignCommand(changesTheDrawing: false); // ZOOM, PAN, REGEN: no object of the database changes
            Assert.False(ChangeOf(editor).IsChanged);
            Assert.Equal("Abierto en la ventana", rig.Item(RackOf[kind]).StatusText);

            rig.ForeignCommand(); // «U» or ERASE: objects of the database change
            Assert.True(ChangeOf(editor).IsChanged);
            Assert.Equal("Abierto en la ventana (posiblemente obsoleto)", rig.Item(RackOf[kind]).StatusText);
        }

        [Fact]
        public void TheChangesOfTheDatabaseOutsideACommandDoNotMarkAndAnotherDrawingPresentedDuringACommandDoes()
        {
            var rig = new Rig();
            var editor = (DynamicEditorViewModel)rig.Open(Dynamic);

            // A change counted at rest (no command ran) is not a command of AutoCAD: nothing is said.
            rig.ChangeTheDatabase();
            rig.Present();
            Assert.False(editor.DrawingChange.IsChanged);

            // A command whose drawing stops being presented before it ends: the window cannot tell, and says it may have changed.
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present(session: 8);
            Assert.True(editor.DrawingChange.IsChanged);
        }

        // ------------------------------------------------------------------ the notice and «Volver a leer del dibujo», in the six editors

        [Theory]
        [MemberData(nameof(Kinds))]
        public void VolverALeerDelDibujoIsTheReadOfEditarAquiAndItsAnswerReplacesTheEditorWithoutTheMark(string kind)
        {
            var rig = new Rig();
            var editor = rig.Open(kind);
            rig.ForeignCommand();
            var change = ChangeOf(editor);
            Assert.True(change.CanReread);

            change.Reread();

            Assert.Equal(2, rig.Door.Reads.Count);
            Assert.Equal(kind, rig.Door.Reads.Last().Kind);
            Assert.Equal(RackOf[kind], rig.Door.Reads.Last().Request.RackId);
            Assert.True(rig.Window.IsWaitingForAutoCad);

            rig.Door.AnswerLastRead();
            var reread = rig.Active(kind);
            Assert.NotNull(reread);
            Assert.NotSame(editor, reread);
            Assert.False(ChangeOf(reread!).IsChanged);
            Assert.Equal("Abierto en la ventana", rig.Item(RackOf[kind]).StatusText);
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void VolverALeerDelDibujoOfADraftWithChangesDiscardsThemAsTheNoticeSays(string kind)
        {
            var rig = new Rig();
            var editor = rig.Open(kind);
            Change(editor);
            rig.ForeignCommand();
            Assert.Equal(RackEditorDrawingChange.DirtyText, ChangeOf(editor).Text);
            Assert.Contains("«Actualizar» escribirá el borrador encima de lo que hay en el dibujo", ChangeOf(editor).Text);

            ChangeOf(editor).Reread();
            rig.Door.AnswerLastRead();

            var reread = rig.Active(kind)!;
            Assert.NotSame(editor, reread);
            Assert.False((bool)((dynamic)reread).IsDirty);
            Assert.False(ChangeOf(reread).IsChanged);
        }

        [Fact]
        public void EditarAquiWithoutTheGestureOfVolverALeerStillKeepsADraftWithChanges()
        {
            var rig = new Rig();
            var editor = (DynamicEditorViewModel)rig.Open(Dynamic);
            Change(editor);
            rig.ForeignCommand();

            // «Editar aquí» (not the reread): the rule of before, the draft with changes is not replaced.
            rig.Window.EditDynamicHere();
            rig.Door.AnswerLastRead();
            Assert.Same(editor, rig.Active(Dynamic));
            Assert.Contains("no se reemplazó", rig.Window.ActionMessage);
            Assert.True(editor.DrawingChange.IsChanged);
        }

        [Fact]
        public void VolverALeerDelDibujoIsOffWhileAutoCadWritesAndSaysWhyWhenTheDoorIsBusy()
        {
            var rig = new Rig();
            var editor = (DynamicEditorViewModel)rig.Open(Dynamic);
            Change(editor);
            editor.RequestWrite();
            Assert.True(editor.IsWriting);

            // A foreign command before the write ran (it did not take it): the editor is marked, but nothing is offered while it writes.
            rig.ForeignCommand();
            Assert.True(editor.DrawingChange.IsChanged);
            Assert.False(editor.DrawingChange.CanReread);
            editor.DrawingChange.Reread();
            Assert.Single(rig.Door.Reads);

            // The write that waits is dropped; the read of the door is the one request of the window.
            rig.Window.StopWaiting();
            Assert.True(editor.DrawingChange.CanReread);
            rig.Window.OpenRack("SEL-1");
            rig.Window.EditSelectiveHere(); // another request waits
            rig.Window.OpenRack("DIN-1");
            editor.DrawingChange.Reread();
            Assert.Equal(2, rig.Door.Reads.Count);
            Assert.Contains("AutoCAD todavía no respondió", rig.Window.ActionMessage);
        }

        // ------------------------------------------------------------------ «Actualizar» with changes asks first, inline

        [Theory]
        [MemberData(nameof(Kinds))]
        public void ActualizarWithChangesAsksInlineWritesOnlyWhenConfirmedAndTheWriteClearsTheMark(string kind)
        {
            var rig = new Rig();
            var editor = rig.Open(kind);
            Change(editor);
            rig.ForeignCommand();
            var change = ChangeOf(editor);

            RequestWrite(editor);
            Assert.True(change.IsConfirmingUpdate);
            Assert.Equal(RackEditorDrawingChange.ConfirmUpdateText, change.ConfirmText);
            Assert.Equal(0, rig.WritesOf(kind));

            change.CancelUpdate();
            Assert.False(change.IsConfirmingUpdate);
            Assert.True(change.IsChanged);
            Assert.Equal(0, rig.WritesOf(kind));

            RequestWrite(editor);
            change.ConfirmUpdate();
            Assert.False(change.IsConfirmingUpdate);
            Assert.Equal(1, rig.WritesOf(kind));
            Assert.True(change.IsChanged); // until AutoCAD wrote it

            rig.Door.AnswerLastWrite(true);
            Assert.False(change.IsChanged);
            Assert.False((bool)((dynamic)editor).IsDirty);
            Assert.Equal("Abierto en la ventana", rig.Item(RackOf[kind]).StatusText);
        }

        [Fact]
        public void AWriteThatAutoCadDidNotDoKeepsTheMarkAndAnEditorWithoutChangesHasNothingToAsk()
        {
            var rig = new Rig();
            var editor = (DynamicEditorViewModel)rig.Open(Dynamic);
            rig.ForeignCommand();
            Assert.False(editor.CanUpdate); // «Limpio»: nothing to write, so nothing to ask
            editor.RequestWrite();
            Assert.False(editor.DrawingChange.IsConfirmingUpdate);

            Change(editor);
            editor.RequestWrite();
            editor.DrawingChange.ConfirmUpdate();
            rig.Door.AnswerLastWrite(false, "AutoCAD no redibujó ninguna vista del rack.");
            Assert.True(editor.DrawingChange.IsChanged);
            Assert.True(editor.IsDirty);

            // The question goes away by itself when the draft goes back to what was read («Descartar»).
            editor.RequestWrite();
            Assert.True(editor.DrawingChange.IsConfirmingUpdate);
            editor.Discard();
            Assert.False(editor.DrawingChange.IsConfirmingUpdate);
            Assert.Equal(RackEditorDrawingChange.CleanText, editor.DrawingChange.Text);
        }

        [Fact]
        public void InsertarVistaWaitsForTheReadOrTheWriteWhileTheMarkIsOn()
        {
            var rig = new Rig();
            var dynamic = (DynamicEditorViewModel)rig.Open(Dynamic);
            var selective = (SelectiveEditorViewModel)rig.Open(Selective);
            var pushBack = (PushBackEditorViewModel)rig.Open(PushBack);
            var cantilever = (CantileverEditorViewModel)rig.Open(Cantilever);
            Assert.True(dynamic.CanInsertView, dynamic.InsertViewBlockedText);
            Assert.True(selective.CanInsertView, selective.InsertViewBlockedText);
            Assert.True(pushBack.CanInsertView, pushBack.InsertViewBlockedText);
            Assert.True(cantilever.CanInsertView, cantilever.InsertViewBlockedText);

            rig.ForeignCommand();

            Assert.Equal(RackEditorDrawingChange.InsertCleanText, dynamic.InsertViewBlockedText);
            Assert.Equal(RackEditorDrawingChange.InsertCleanText, selective.InsertViewBlockedText);
            Assert.Equal(RackEditorDrawingChange.InsertCleanText, pushBack.InsertViewBlockedText);
            Assert.Equal(RackEditorDrawingChange.InsertCleanText, cantilever.InsertViewBlockedText);
            Change(dynamic);
            Assert.Equal(RackEditorDrawingChange.InsertDirtyText, dynamic.InsertViewBlockedText);
        }

        // ------------------------------------------------------------------ the views (real WPF)

        private static FrameworkElementPair ViewOf(object editor) => editor switch
        {
            SelectiveEditorViewModel s => Pair(new SelectiveEditorView(s), v => ((SelectiveEditorView)v).DrawingChangePanel, v => ((SelectiveEditorView)v).UpdateButton),
            DynamicEditorViewModel d => Pair(new DynamicEditorView(d), v => ((DynamicEditorView)v).DrawingChangePanel, v => ((DynamicEditorView)v).UpdateButton),
            PushBackEditorViewModel p => Pair(new PushBackEditorView(p), v => ((PushBackEditorView)v).DrawingChangePanel, v => ((PushBackEditorView)v).UpdateButton),
            CantileverEditorViewModel c => Pair(new CantileverEditorView(c), v => ((CantileverEditorView)v).DrawingChangePanel, v => ((CantileverEditorView)v).UpdateButton),
            CamaEditorViewModel m => Pair(new CamaEditorView(m), v => ((CamaEditorView)v).DrawingChangePanel, v => ((CamaEditorView)v).PrimaryButton),
            CabeceraEditorViewModel b => Pair(new CabeceraEditorView(b), v => ((CabeceraEditorView)v).DrawingChangePanel, v => ((CabeceraEditorView)v).UpdateButton),
            _ => throw new ArgumentException("editor"),
        };

        private sealed record FrameworkElementPair(System.Windows.FrameworkElement View, RackEditorDrawingChangePanel Panel, System.Windows.Controls.Button Update);

        private static FrameworkElementPair Pair(System.Windows.FrameworkElement view, Func<System.Windows.FrameworkElement, RackEditorDrawingChangePanel> panel,
            Func<System.Windows.FrameworkElement, System.Windows.Controls.Button> update) => new(view, panel(view), update(view));

        private static void Lay(System.Windows.FrameworkElement view)
        {
            view.Measure(new System.Windows.Size(1400, 900));
            view.Arrange(new System.Windows.Rect(0, 0, 1400, 900));
            view.UpdateLayout();
        }

        private static void Click(System.Windows.Controls.Button button) =>
            button.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        [Theory]
        [MemberData(nameof(Kinds))]
        public void EveryEditorViewShowsTheNoticeWithItsButtonOnlyWithTheMarkAndTheButtonReadsTheDrawing(string kind)
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var editor = rig.Open(kind);
                var (view, panel, _) = ViewOf(editor);
                Lay(view);
                Assert.Same(ChangeOf(editor), panel.DataContext);
                Assert.Equal(System.Windows.Visibility.Collapsed, panel.NoticePanel.Visibility);

                rig.ForeignCommand();
                Lay(view);
                Assert.Equal(System.Windows.Visibility.Visible, panel.NoticePanel.Visibility);
                Assert.Equal(RackEditorDrawingChange.CleanText, panel.NoticeText.Text);
                Assert.True(panel.RereadButton.IsEnabled);
                Assert.Equal("Volver a leer del dibujo", panel.RereadButton.Content);
                Assert.Equal("Volver a leer este rack del dibujo", System.Windows.Automation.AutomationProperties.GetName(panel.RereadButton));
                Assert.Equal(System.Windows.Visibility.Collapsed, panel.ConfirmPanel.Visibility);

                Click(panel.RereadButton);
                Assert.Equal(2, rig.Door.Reads.Count);
                Assert.Equal(kind, rig.Door.Reads.Last().Kind);
            });
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void EveryEditorViewAsksInlineBeforeActualizarWritesOverTheDrawing(string kind)
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var editor = rig.Open(kind);
                var (view, panel, update) = ViewOf(editor);
                Change(editor);
                rig.ForeignCommand();
                Lay(view);
                Assert.Equal(RackEditorDrawingChange.DirtyText, panel.NoticeText.Text);

                Click(update);
                Lay(view);
                Assert.Equal(System.Windows.Visibility.Visible, panel.ConfirmPanel.Visibility);
                Assert.Equal(0, rig.WritesOf(kind));

                Click(panel.CancelOverwriteButton);
                Lay(view);
                Assert.Equal(System.Windows.Visibility.Collapsed, panel.ConfirmPanel.Visibility);
                Assert.Equal(0, rig.WritesOf(kind));

                Click(update);
                Click(panel.OverwriteButton);
                Assert.Equal(1, rig.WritesOf(kind));
                rig.Door.AnswerLastWrite(true);
                Lay(view);
                Assert.Equal(System.Windows.Visibility.Collapsed, panel.NoticePanel.Visibility);

                var names = new[] { panel.RereadButton, panel.OverwriteButton, panel.CancelOverwriteButton }
                    .Select(System.Windows.Automation.AutomationProperties.GetName).ToList();
                Assert.Equal(names.Count, names.Distinct().Count());
            });
        }
    }
}
