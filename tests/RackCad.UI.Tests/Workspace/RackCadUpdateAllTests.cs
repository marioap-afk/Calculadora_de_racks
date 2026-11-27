#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
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
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T6 at (I-64, sexta tanda): «Actualizar todo» of the RackCad window, with a double of every door (the six editors, the page «Variables»
    /// and RACKACTUALIZARTODO). The window sends, in ONE request, every draft «Modificado» of the drawing with exactly the write its own
    /// «Actualizar» sends; a clean draft, a draft of «Nuevo» and a draft of another drawing do not go; a draft whose «Actualizar» is blocked
    /// prevents it, named with its cause; a draft marked «posiblemente desactualizado» (the predicate the session connects to part st) asks
    /// first, in line. The answer leaves every editor as after its own «Actualizar», and the list says how each one ended. The answers are
    /// made by the pure composition (<see cref="UpdateAllRun"/>) over a double of AutoCAD, so the window reads what the command would say.
    /// </summary>
    public class RackCadUpdateAllTests
    {
        private static readonly SessionId Session = new(7);

        // ------------------------------------------------------------------ the doors

        private sealed class AllDoors : IRackCadSelectiveDoorPort, IRackCadPushBackDoorPort, IRackCadCantileverDoorPort, IRackCadCamaDoorPort,
            IRackCadCabeceraDoorPort, IRackCadVariablesDoorPort, IRackCadUpdateAllDoorPort
        {
            public List<object> Requests { get; } = new();

            public List<object> Writes { get; } = new();

            public List<UpdateAllWrite> UpdateAll { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead;

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted;

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public event Action<RackDesignRead>? PushBackRead;

            public event Action<CantileverDesignRead>? CantileverRead;

            public event Action<CamaDesignRead>? CamaRead;

            public event Action<CabeceraDesignRead>? CabeceraRead;

            public event Action<VariablesDoorRead>? VariablesRead;

            public event Action<VariablesDoorWritten>? VariablesWritten;

            public event Action<UpdateAllWritten>? UpdateAllWritten;

            private RackCadClassicResult Read(object request)
            {
                Requests.Add(request);
                return Answer;
            }

            private RackCadClassicResult Write(object write)
            {
                Writes.Add(write);
                return Answer;
            }

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => Read(request);

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => Write(write);

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request) => Read(request);

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write) => Write(write);

            public RackCadClassicResult RequestPushBackDesign(RackDoorRequest request) => Read(request);

            public RackCadClassicResult RequestPushBackUpdate(PushBackDoorWrite write) => Write(write);

            public RackCadClassicResult RequestCantileverDesign(RackDoorRequest request) => Read(request);

            public RackCadClassicResult RequestCantileverUpdate(CantileverDoorWrite write) => Write(write);

            public RackCadClassicResult RequestCamaDesign(RackDoorRequest request) => Read(request);

            public RackCadClassicResult RequestCamaWrite(CamaDoorWrite write) => Write(write);

            public RackCadClassicResult RequestCabeceraDesign(RackDoorRequest request) => Read(request);

            public RackCadClassicResult RequestCabeceraUpdate(CabeceraDoorWrite write) => Write(write);

            public RackCadClassicResult RequestVariables(VariablesDoorRequest request) => Read(request);

            public RackCadClassicResult RequestVariablesUpdate(VariablesDoorWrite write) => Write(write);

            public RackCadClassicResult RequestUpdateAll(UpdateAllWrite write)
            {
                UpdateAll.Add(write);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            public long LastId => Requests.Last() switch
            {
                RackDoorRequest rack => rack.RequestId,
                VariablesDoorRequest variables => variables.RequestId,
                _ => throw new InvalidOperationException(),
            };

            public void ReadDynamic(RackDesignRead read) => DesignRead?.Invoke(read);

            public void ReadSelective(SelectiveDesignRead read) => SelectiveRead?.Invoke(read);

            public void ReadPushBack(RackDesignRead read) => PushBackRead?.Invoke(read);

            public void ReadCantilever(CantileverDesignRead read) => CantileverRead?.Invoke(read);

            public void ReadCama(CamaDesignRead read) => CamaRead?.Invoke(read);

            public void ReadCabecera(CabeceraDesignRead read) => CabeceraRead?.Invoke(read);

            public void ReadVariables(VariablesDoorRead read) => VariablesRead?.Invoke(read);

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);

            public void VariablesAnswer(VariablesDoorWritten written) => VariablesWritten?.Invoke(written);

            // What RACKACTUALIZARTODO answers to the last request: the pure composition over a double of AutoCAD.
            public UpdateAllWritten AnswerLastUpdateAll(Units? units = null)
            {
                var answer = UpdateAllRun.Execute(UpdateAll.Last(), units ?? new Units());
                UpdateAllWritten?.Invoke(answer);
                return answer;
            }

            public void AnswerUpdateAll(UpdateAllWritten written) => UpdateAllWritten?.Invoke(written);
        }

        /// <summary>AutoCAD as the composition sees it: every draft re-read and written, unless the test says it fails.</summary>
        private sealed class Units : IUpdateAllUnitPort
        {
            public string? RefuseRack { get; init; }

            public string? FailRack { get; init; }

            public bool CommitThrows { get; init; }

            public List<string> Applied { get; } = new();

            public bool Committed { get; private set; }

            public void Begin()
            {
            }

            public UpdateAllCheck Preflight(UpdateAllItem item)
                => item.RackId != null && string.Equals(item.RackId, RefuseRack, StringComparison.OrdinalIgnoreCase)
                    ? UpdateAllCheck.Refused("El rack ya no está en el dibujo.")
                    : UpdateAllCheck.Ready();

            public bool CancelRequested() => false;

            public UpdateAllApplied Apply(UpdateAllItem item, UpdateAllCheck check)
            {
                if (item.RackId != null && string.Equals(item.RackId, FailRack, StringComparison.OrdinalIgnoreCase))
                    return UpdateAllApplied.Failed("AutoCAD no redibujó ninguna vista del rack.");
                Applied.Add(item.RackId ?? "Variables");
                return item.System == UpdateAllSystem.Variables
                    ? new UpdateAllApplied(true, null, VariablesAnswer: VariablesDoorRules.AnswerForWrite(1, true, new[] { RackId }, 1, false, null))
                    : new UpdateAllApplied(true, null, Answer: RackDoorRules.AnswerForApply(1, item.RackId!, 1, 0));
            }

            public void Commit()
            {
                if (CommitThrows) throw new InvalidOperationException("eLockViolation");
                Committed = true;
            }

            public void Rollback()
            {
            }

            public void Progress(string step, int done, int total, UpdateAllItem? item)
            {
            }

            public string ImportOutside(IReadOnlyCollection<string> blocks) => null;

            public void Finish(bool committed)
            {
            }
        }

        // ------------------------------------------------------------------ the drawing and its racks

        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";
        private static readonly StructuralSectionCatalog Catalogue = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static string FixtureText(string name)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            return File.ReadAllText(Path.Combine(dir!.FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json"));
        }

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
            var draft = new RackCad.Application.VentanaRackCad.DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(design, "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            return new RackProjectStore().Serialize(RackProject.ForDynamic(draft.Design));
        }

        private static string CantileverJson()
        {
            var draft = new CantileverDraftEditor(Catalogue);
            Assert.Null(draft.Write(CantileverLineField.StationCount, "3"));
            Assert.Null(draft.Write(CantileverLineField.Spacing, "96"));
            Assert.Null(draft.Write(CantileverLineField.LevelCount, "3"));
            Assert.Null(draft.Write(CantileverLineField.ClearHeight, "24"));
            Assert.Null(draft.Write(CantileverLineField.ColumnSection, ColumnW));
            Assert.Null(draft.Write(CantileverLineField.BaseSection, BaseW));
            Assert.Null(draft.Write(CantileverLineField.BaseLength, "48"));
            Assert.Null(draft.WriteArm(null, CantileverArmField.Section, ArmHss));
            Assert.Null(draft.WriteArm(null, CantileverArmField.CutLength, "36"));
            Assert.True(draft.IsResolved);
            draft.SetName("Cantilever 1");
            return new RackProjectStore().Serialize(RackProject.ForCantilever(draft.Design));
        }

        private static string CabeceraJson()
        {
            var header = new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe()).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 132.0, 42.0);
            header.Name = "Cabecera 1";
            return new RackProjectStore().Serialize(RackProject.ForSelective(header));
        }

        private static string CamaJson() => new FlowBedConfigurationStore().Serialize(new FlowBedConfiguration
        {
            BedType = FlowBedType.Dynamic,
            LaneDepth = 96.0,
            PalletDepth = 48.0,
            RollerId = FlowBedDefaults.RollerId,
        });

        // The drawing «Holgura estándar» (10 in), used by SEL-1 (bound), as the page «Variables» reads it.
        private static VariablesDoorRead VariablesRead(long requestId)
        {
            var authored = Store.Deserialize(Authored(Structure(), bound: true));
            var entries = new List<ProjectVariableScanEntry> { ProjectVariableScanEntry.Selective("D1", RackId, authored, 1) };
            return new VariablesDoorRead(requestId, true, Register(10.0), entries, new CustomPropertiesStore().Read(CustomPropertiesPayload.Absent()), null);
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
                        new BrowserRackListEntry("SEL-1", "Selectivo A", RackEmbedDocument.KindSelective, new[] { "frontal" }),
                        new BrowserRackListEntry("SEL-2", "Selectivo B", RackEmbedDocument.KindSelective, new[] { "frontal" }),
                        new BrowserRackListEntry("PB-1", "PB 1", RackEmbedDocument.KindPushBack, new[] { "lateral" }),
                        new BrowserRackListEntry("DIN-1", "DIN 1", RackEmbedDocument.KindDynamic, new[] { "lateral" }),
                        new BrowserRackListEntry("CAN-1", "Cantilever 1", RackEmbedDocument.KindCantilever, new[] { "frontal" }),
                        new BrowserRackListEntry("CAB-1", "Cabecera 1", RackEmbedDocument.KindCabecera, new[] { "lateral" }),
                        new BrowserRackListEntry("CAMA-1", "Cama 1", RackEmbedDocument.KindCama, new[] { "lateral" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, null, Door, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public AllDoors Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }

            /// <summary>«Editar aquí» on the rack of a system, the answer of its door, and one change: the editor is «Modificado».</summary>
            public object OpenAndChange(UpdateAllSystem system)
            {
                var editor = Open(system);
                Change(editor);
                return editor;
            }

            public object Open(UpdateAllSystem system)
            {
                switch (system)
                {
                    case UpdateAllSystem.Selective:
                        return OpenSelective("SEL-2", bound: false);
                    case UpdateAllSystem.PushBack:
                        Window.OpenRack("PB-1");
                        Window.EditPushBackHere();
                        Door.ReadPushBack(PushBackDoorRules.AnswerForOpen(Door.LastId, "PB-1", RackEmbedDocument.KindPushBack, "PB 1",
                            new RackEmbedStore().Deserialize(FixtureText("pb-01-base"))!.Design));
                        return Assert.IsType<PushBackEditorViewModel>(Window.ActivePushBackEditor);
                    case UpdateAllSystem.Dynamic:
                        Window.OpenRack("DIN-1");
                        Window.EditDynamicHere();
                        Door.ReadDynamic(new RackDesignRead(Door.LastId, "DIN-1", true, RackEmbedDocument.KindDynamic, "DIN 1", DynamicJson(), null));
                        return Assert.IsType<DynamicEditorViewModel>(Window.ActiveDynamicEditor);
                    case UpdateAllSystem.Cantilever:
                        Window.OpenRack("CAN-1");
                        Window.EditCantileverHere();
                        Door.ReadCantilever(CantileverDoorRules.AnswerForOpen(Door.LastId, "CAN-1", RackEmbedDocument.KindCantilever, "Cantilever 1", CantileverJson()));
                        return Assert.IsType<CantileverEditorViewModel>(Window.ActiveCantileverEditor);
                    case UpdateAllSystem.Cabecera:
                        Window.OpenRack("CAB-1");
                        Window.EditCabeceraHere();
                        Door.ReadCabecera(CabeceraDoorRules.AnswerForOpen(Door.LastId, "CAB-1", RackEmbedDocument.KindCabecera, "Cabecera 1", CabeceraJson()));
                        return Assert.IsType<CabeceraEditorViewModel>(Window.ActiveCabeceraEditor);
                    case UpdateAllSystem.Cama:
                        Window.OpenRack("CAMA-1");
                        Window.EditCamaHere();
                        Door.ReadCama(CamaDoorRules.AnswerForOpen(Door.LastId, "CAMA-1", RackEmbedDocument.KindCama, "Cama 1", null, -1, CamaJson()));
                        return Assert.IsType<CamaEditorViewModel>(Window.ActiveCamaEditor);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(system));
                }
            }

            public SelectiveEditorViewModel OpenSelective(string rackId, bool bound)
            {
                Window.OpenRack(rackId);
                Window.EditSelectiveHere();
                Door.ReadSelective(SelectiveWindowFixtures.Read(Door.LastId, rackId, null, bound));
                return Assert.IsType<SelectiveEditorViewModel>(Window.ActiveSelectiveEditor);
            }

            public VariablesPageViewModel ShowVariables()
            {
                Window.ShowPage(RackCadWindowPage.Variables);
                Door.ReadVariables(VariablesRead(Door.LastId));
                var page = Window.VariablesPage!;
                Assert.True(page.IsLoaded, page.Message);
                Window.ShowPage(RackCadWindowPage.Rack);
                return page;
            }
        }

        private static void Change(object editor)
        {
            switch (editor)
            {
                case SelectiveEditorViewModel selective:
                    Write(Field(selective.CellFields, "cHeight"), "72");
                    break;
                case PushBackEditorViewModel pushBack:
                    Write(Field(pushBack.CellFields, "cFront"), "45");
                    break;
                case DynamicEditorViewModel dynamic:
                    Write(Field(dynamic.Essentials, "weight"), "1200");
                    break;
                case CantileverEditorViewModel cantilever:
                    Write(Field(cantilever.StructureItems, "Spacing"), "120");
                    break;
                case CabeceraEditorViewModel cabecera:
                    Choice(cabecera.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();
                    break;
                case CamaEditorViewModel cama:
                    cama.LaneField.Text = "120";
                    cama.LaneField.Commit();
                    break;
            }

            Assert.Equal("Modificado", StatusOf(editor));
        }

        private static void DefineVariable(VariablesPageViewModel page, string text)
        {
            page.InspectorDefinition = text;
            page.CommitInspectorDefinition();
            Assert.True(page.IsDirty);
        }

        private static string StatusOf(object editor) => editor switch
        {
            SelectiveEditorViewModel e => e.StatusText,
            PushBackEditorViewModel e => e.StatusText,
            DynamicEditorViewModel e => e.StatusText,
            CantileverEditorViewModel e => e.StatusText,
            CabeceraEditorViewModel e => e.StatusText,
            CamaEditorViewModel e => e.StatusText,
            VariablesPageViewModel e => e.StatusText,
            _ => throw new ArgumentException(nameof(editor)),
        };

        private static bool IsWriting(object editor) => editor switch
        {
            SelectiveEditorViewModel e => e.IsWriting,
            PushBackEditorViewModel e => e.IsWriting,
            DynamicEditorViewModel e => e.IsWriting,
            CantileverEditorViewModel e => e.IsWriting,
            CabeceraEditorViewModel e => e.IsWriting,
            CamaEditorViewModel e => e.IsWriting,
            VariablesPageViewModel e => e.IsWriting,
            _ => throw new ArgumentException(nameof(editor)),
        };

        private static string MessageOf(object editor) => editor switch
        {
            SelectiveEditorViewModel e => e.Message,
            PushBackEditorViewModel e => e.Message,
            DynamicEditorViewModel e => e.Message,
            CantileverEditorViewModel e => e.Message,
            CabeceraEditorViewModel e => e.Message,
            CamaEditorViewModel e => e.Message,
            VariablesPageViewModel e => e.Message,
            _ => throw new ArgumentException(nameof(editor)),
        };

        private static void RequestWrite(object editor)
        {
            switch (editor)
            {
                case SelectiveEditorViewModel e: e.RequestWrite(); break;
                case PushBackEditorViewModel e: e.RequestWrite(); break;
                case DynamicEditorViewModel e: e.RequestWrite(); break;
                case CantileverEditorViewModel e: e.RequestWrite(); break;
                case CabeceraEditorViewModel e: e.RequestWrite(); break;
                case CamaEditorViewModel e: e.RequestWrite(); break;
            }
        }

        // What a write carries, without its number: the rack, the drawing, the name, the design (as its document) and whether it carries
        // the tab «Propiedades». It is the same for the write of «Actualizar» and the item of «Actualizar todo».
        private static string Shape(object write) => write switch
        {
            RackDoorWrite w => string.Join("|", "Dynamic", w.RackId, w.Session, w.Name, w.NewRack, w.Inserts, w.Properties != null,
                new RackProjectStore().Serialize(RackProject.ForDynamic(w.Design)), w.System != null),
            SelectiveDoorWrite w => string.Join("|", "Selective", w.RackId, w.Session, w.Name, w.NewRack, w.Inserts, w.Properties != null, Json(w.Design),
                w.System != null, string.Join(",", (w.LinkedPropertyFinalStates ?? new Dictionary<PropertyId, LinkedPropertyEditState>()).Select(p => p.Key + "=" + p.Value))),
            PushBackDoorWrite w => string.Join("|", "PushBack", w.RackId, w.Session, w.Name, w.NewRack, w.Inserts, w.Properties != null,
                new RackProjectStore().Serialize(RackProject.ForPushBack(w.Design)), w.System != null),
            CantileverDoorWrite w => string.Join("|", "Cantilever", w.RackId, w.Session, w.Name, w.NewRack, w.Inserts, w.Properties != null, w.DesignJson,
                w.Line != null),
            CamaDoorWrite w => string.Join("|", "Cama", w.RackId, w.Session, w.Name, w.NewRack, w.Properties != null, new FlowBedConfigurationStore().Serialize(w.Bed),
                w.SourceDesignJson ?? "-"),
            CabeceraDoorWrite w => string.Join("|", "Cabecera", w.RackId, w.Session, w.Name, new RackFrameProjectStore().Serialize(w.Cabecera)),
            _ => throw new ArgumentException(write?.GetType().Name),
        };

        private static object WriteOf(UpdateAllItem item) => (object?)item.Dynamic ?? (object?)item.Selective ?? (object?)item.PushBack ?? (object?)item.Cantilever
            ?? (object?)item.Cama ?? (object?)item.Cabecera ?? item.Variables!;

        public static TheoryData<UpdateAllSystem> TheSixEditors() => new()
        {
            UpdateAllSystem.Selective, UpdateAllSystem.PushBack, UpdateAllSystem.Dynamic, UpdateAllSystem.Cantilever, UpdateAllSystem.Cabecera, UpdateAllSystem.Cama,
        };

        // ------------------------------------------------------------------ one editor of each system

        [Theory]
        [MemberData(nameof(TheSixEditors))]
        public void EveryEditorGoesWithTheWriteOfItsOwnActualizarAndIsLeftCleanOnWhatWasSent(UpdateAllSystem system)
        {
            var rig = new Rig();
            Assert.True(rig.Window.ShowUpdateAll);
            Assert.Equal("Actualizar todo (0)", rig.Window.UpdateAllLabel);
            Assert.False(rig.Window.CanUpdateAll);
            Assert.Equal(RackCadWindowViewModel.NothingToWriteText, rig.Window.UpdateAllBlockedText); // with the hint of a value just typed (review of c4985a2e, minor 5)

            var editor = rig.OpenAndChange(system);
            Assert.Equal("Actualizar todo (1)", rig.Window.UpdateAllLabel);
            Assert.True(rig.Window.CanUpdateAll, rig.Window.UpdateAllBlockedText);

            // The write of its own «Actualizar», then «Dejar de esperar»: the editor is «Modificado» again.
            RequestWrite(editor);
            var own = Assert.Single(rig.Door.Writes);
            rig.Window.StopWaiting();
            Assert.Equal("Modificado", StatusOf(editor));

            rig.Window.RequestUpdateAll();
            var request = Assert.Single(rig.Door.UpdateAll);
            Assert.Equal(Session, request.Session);
            var item = Assert.Single(request.Items);
            Assert.Equal(system, item.System);
            Assert.True(item.IsWellFormed);
            Assert.Equal(Shape(own), Shape(WriteOf(item)));
            Assert.True(IsWriting(editor));
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.False(rig.Window.CanUpdateAll);
            Assert.Contains("con «Actualizar todo»", rig.Window.WaitingText);

            var answer = rig.Door.AnswerLastUpdateAll();
            Assert.True(answer.Committed);
            Assert.False(IsWriting(editor));
            Assert.Equal("Limpio", StatusOf(editor));
            Assert.StartsWith("«Actualizar todo»: AutoCAD redibujó 1 vista", MessageOf(editor));
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
            Assert.Equal("Actualizar todo (0)", rig.Window.UpdateAllLabel);

            var row = Assert.Single(rig.Window.UpdateAllResults);
            Assert.Equal(("Escrito", false), (row.OutcomeText, row.Failed));
            Assert.False(rig.Window.UpdateAllResultFailed);
            Assert.Equal("AutoCAD escribió 1 borrador en una sola operación. Un «U» en AutoCAD lo deshace todo.", rig.Window.UpdateAllResultText);
            rig.Window.DismissUpdateAllResults();
            Assert.False(rig.Window.HasUpdateAllResults);
        }

        [Fact]
        public void TheSixEditorsAndThePageVariablesGoInOneRequestVariablesFirstAndOneFailureWritesNone()
        {
            var rig = new Rig();
            var page = rig.ShowVariables();
            DefineVariable(page, "12");
            var editors = new[] { UpdateAllSystem.Selective, UpdateAllSystem.PushBack, UpdateAllSystem.Dynamic, UpdateAllSystem.Cantilever,
                UpdateAllSystem.Cabecera, UpdateAllSystem.Cama }.Select(rig.OpenAndChange).ToList();
            Assert.Equal("Actualizar todo (7)", rig.Window.UpdateAllLabel);

            rig.Window.RequestUpdateAll();
            var request = Assert.Single(rig.Door.UpdateAll);
            Assert.Equal(new[] { UpdateAllSystem.Variables, UpdateAllSystem.Selective, UpdateAllSystem.PushBack, UpdateAllSystem.Dynamic,
                UpdateAllSystem.Cantilever, UpdateAllSystem.Cabecera, UpdateAllSystem.Cama }, request.Items.Select(i => i.System).ToArray());
            Assert.All(request.Items, i => Assert.Equal(request.RequestId, WriteOf(i) switch
            {
                RackDoorWrite w => w.RequestId, SelectiveDoorWrite w => w.RequestId, PushBackDoorWrite w => w.RequestId, CantileverDoorWrite w => w.RequestId,
                CamaDoorWrite w => w.RequestId, CabeceraDoorWrite w => w.RequestId, VariablesDoorWrite w => w.RequestId, _ => -1,
            }));
            Assert.True(page.IsWriting);
            Assert.All(editors, e => Assert.True(IsWriting(e)));

            // No other «Actualizar» while it lasts: the door has one request.
            rig.Window.StopWaiting(); // (not taken yet, so it can stop)
            rig.Window.RequestUpdateAll();
            rig.Door.Start(rig.Door.UpdateAll.Last().RequestId);
            Assert.False(rig.Window.CanStopWaiting);
            Assert.Contains("ESC lo cancela antes de confirmar", rig.Window.WaitingText);
            RequestWrite(editors[0]);
            Assert.Empty(rig.Door.Writes);

            // The Push Back fails when it is written: nothing is written, and the list says which one and why.
            var units = new Units { FailRack = "PB-1" };
            var answer = rig.Door.AnswerLastUpdateAll(units);
            Assert.False(answer.Committed);
            Assert.False(units.Committed);
            Assert.All(editors, e => Assert.Equal("Modificado", StatusOf(e)));
            Assert.All(editors, e => Assert.False(IsWriting(e)));
            Assert.True(page.IsDirty);
            Assert.False(page.IsWriting);
            Assert.Equal("No se escribió nada: falló «PB 1». AutoCAD no redibujó ninguna vista del rack.", rig.Window.UpdateAllResultText);
            Assert.True(rig.Window.UpdateAllResultFailed);
            var rows = rig.Window.UpdateAllResults;
            Assert.Equal(7, rows.Count);
            var failed = Assert.Single(rows, r => r.OutcomeText == "No se pudo escribir");
            Assert.Equal(("PB 1", "Push Back", "AutoCAD no redibujó ninguna vista del rack."), (failed.Name, failed.SystemLabel, failed.Cause));
            Assert.All(rows.Where(r => r != failed), r => Assert.Equal(("No aplicado", "Falló otro rack del conjunto."), (r.OutcomeText, r.Cause)));
            Assert.Contains("falló otro rack del conjunto", MessageOf(editors[0]));
            Assert.Contains("este borrador no se pudo escribir", MessageOf(editors[1]));

            // Again, and it goes well: every draft is clean, and the page reads the drawing again (the door said Busy is the real case).
            rig.Window.RequestUpdateAll();
            var ok = rig.Door.AnswerLastUpdateAll();
            Assert.True(ok.Committed);
            Assert.All(editors, e => Assert.Equal("Limpio", StatusOf(e)));
            Assert.False(page.IsWriting);
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
            Assert.Equal("AutoCAD escribió 7 borradores en una sola operación. Un «U» en AutoCAD lo deshace todo.", rig.Window.UpdateAllResultText);
            Assert.All(rig.Window.UpdateAllResults, r => Assert.Equal("Escrito", r.OutcomeText));
            Assert.IsType<VariablesDoorRequest>(rig.Door.Requests.Last());
        }

        [Fact]
        public void ARefusalOfTheReReadNamesItsDraftAndEveryOtherIsNotApplied()
        {
            var rig = new Rig();
            var pushBack = rig.OpenAndChange(UpdateAllSystem.PushBack);
            var cama = rig.OpenAndChange(UpdateAllSystem.Cama);
            rig.Window.RequestUpdateAll();
            rig.Door.AnswerLastUpdateAll(new Units { RefuseRack = "CAMA-1" });

            Assert.Equal("Modificado", StatusOf(pushBack));
            Assert.Equal("Modificado", StatusOf(cama));
            Assert.Equal("No se escribió nada: no se puede escribir «Cama 1». La lista dice por qué.", rig.Window.UpdateAllResultText);
            Assert.Equal(new[] { ("PB 1", "No aplicado"), ("Cama 1", "No se pudo escribir") },
                rig.Window.UpdateAllResults.Select(r => (r.Name, r.OutcomeText)).ToArray());
            Assert.Equal("El rack ya no está en el dibujo.", rig.Window.UpdateAllResults[1].Cause);
        }

        [Fact]
        public void AConfirmationThatFailsLeavesEveryDraftUnknownAndToBeReadAgain()
        {
            var rig = new Rig();
            var dynamic = (DynamicEditorViewModel)rig.OpenAndChange(UpdateAllSystem.Dynamic);
            var cama = (CamaEditorViewModel)rig.OpenAndChange(UpdateAllSystem.Cama);
            rig.Window.RequestUpdateAll();
            rig.Door.AnswerLastUpdateAll(new Units { CommitThrows = true });

            Assert.Equal("Modificado", StatusOf(dynamic));
            Assert.Contains("No se sabe si AutoCAD lo escribió", MessageOf(dynamic));
            Assert.StartsWith("No se sabe qué escribió AutoCAD", rig.Window.UpdateAllResultText);
            Assert.All(rig.Window.UpdateAllResults, r => Assert.Equal("Se desconoce", r.OutcomeText));

            // Review of c4985a2e, minor 1: unknown is a state of condition 4: it prevents the next «Actualizar todo» until each draft is read
            // again from the drawing or discarded.
            Assert.False(rig.Window.CanUpdateAll);
            Assert.Equal(new[] { "DIN 1", "Cama 1" }, rig.Window.UpdateAllEligibility.Blockers.Select(b => b.Candidate.Name).ToArray());
            Assert.Contains("No se sabe si el último «Actualizar todo» lo escribió", rig.Window.UpdateAllBlockedText);
            rig.Window.RequestUpdateAll();
            Assert.Single(rig.Door.UpdateAll);

            dynamic.Discard();
            Assert.Equal("Limpio", StatusOf(dynamic));
            Assert.Equal(new[] { "Cama 1" }, rig.Window.UpdateAllEligibility.Blockers.Select(b => b.Candidate.Name).ToArray());

            // Read again: the editor is closed and opened with «Editar aquí» (a new editor of the same rack carries no mark).
            cama.RequestClose();
            cama.ConfirmClose();
            var reread = (CamaEditorViewModel)rig.OpenAndChange(UpdateAllSystem.Cama);
            Assert.NotSame(cama, reread);
            Assert.Empty(rig.Window.UpdateAllEligibility.Blockers);
            Assert.True(rig.Window.CanUpdateAll, rig.Window.UpdateAllBlockedText);
        }

        // ------------------------------------------------------------------ elegibility in the window

        [Fact]
        public void CleanDraftsNewDraftsAndDraftsOfAnotherDrawingDoNotGo()
        {
            var rig = new Rig();
            rig.Open(UpdateAllSystem.PushBack); // clean
            var cantilever = rig.OpenAndChange(UpdateAllSystem.Cantilever);
            rig.Window.StartNewPushBack(); // a draft of «Nuevo», not inserted, and with changes (review of c4985a2e, minor 8)
            var fresh = Assert.IsType<PushBackEditorViewModel>(rig.Window.NewPushBackEditor);
            Write(Field(fresh.CellFields, "cFront"), "45");
            Assert.True(fresh.IsDirty);
            Assert.True(fresh.IsNewRack);
            Assert.Equal("Actualizar todo (1)", rig.Window.UpdateAllLabel);

            // Another drawing: its drafts wait for their drawing, and this one has none.
            rig.Present(8);
            Assert.Equal("Actualizar todo (0)", rig.Window.UpdateAllLabel);
            rig.Window.RequestUpdateAll();
            Assert.Empty(rig.Door.UpdateAll);
            Assert.Equal(RackCadWindowViewModel.NothingToWriteText, rig.Window.UpdateAllResultText);

            rig.Present(7);
            rig.Window.RequestUpdateAll();
            var item = Assert.Single(Assert.Single(rig.Door.UpdateAll).Items);
            Assert.Equal(("CAN-1", UpdateAllSystem.Cantilever), (item.RackId, item.System));
            Assert.True(IsWriting(cantilever));
        }

        [Fact]
        public void ADraftWhoseActualizarIsBlockedPreventsItWithItsCauseAndNothingGoes()
        {
            var rig = new Rig();
            rig.OpenAndChange(UpdateAllSystem.PushBack);
            var cama = (CamaEditorViewModel)rig.OpenAndChange(UpdateAllSystem.Cama);
            cama.LaneField.Text = "-5";
            cama.LaneField.Commit();
            Assert.False(cama.CanUpdate);

            Assert.Equal("Actualizar todo (1)", rig.Window.UpdateAllLabel);
            Assert.False(rig.Window.CanUpdateAll);
            Assert.True(rig.Window.HasUpdateAllBlockers);
            var why = "«Actualizar todo» no puede escribir mientras este borrador no se corrija: «Cama 1»: " + cama.PrimaryBlockedText + " No se escribiría nada.";
            Assert.Equal(why, rig.Window.UpdateAllBlockersText);
            Assert.Equal(why, rig.Window.UpdateAllToolTip);

            // Review of c4985a2e, minor 2: the notice shows only after the click (before it, the tip of the button says why).
            Assert.False(rig.Window.ShowUpdateAllPanel);
            rig.Window.RequestUpdateAll();
            Assert.Empty(rig.Door.UpdateAll);
            Assert.True(rig.Window.ShowUpdateAllPanel);
            Assert.Equal(why, rig.Window.UpdateAllResultText);
            Assert.True(rig.Window.UpdateAllResultFailed);

            // Corrected, it goes with the others.
            cama.LaneField.Text = "120";
            cama.LaneField.Commit();
            Assert.False(rig.Window.HasUpdateAllBlockers);
            Assert.Equal("Actualizar todo (2)", rig.Window.UpdateAllLabel);
            Assert.True(rig.Window.CanUpdateAll, rig.Window.UpdateAllBlockedText);
        }

        [Fact]
        public void TheVariablesOfARackWithADraftPreventItAsTheirOwnActualizarDoes()
        {
            var rig = new Rig();
            var page = rig.ShowVariables();
            DefineVariable(page, "12");
            var bound = rig.OpenSelective(RackId, bound: true);
            Change(bound);

            // VA-04: the page cannot write while the draft of the rack it redraws is not written; so neither can «Actualizar todo».
            Assert.False(page.CanUpdate);
            Assert.False(rig.Window.CanUpdateAll);
            Assert.Contains("«Variables»: Antes, escribe o descarta el borrador de", rig.Window.UpdateAllBlockersText);
        }

        [Fact]
        public void AnotherRequestOfTheDoorWaitingPreventsItAndStopWaitingLeavesEveryEditorEditable()
        {
            var rig = new Rig();
            var pushBack = rig.OpenAndChange(UpdateAllSystem.PushBack);
            RequestWrite(pushBack);
            Assert.False(rig.Window.CanUpdateAll);
            Assert.Equal("AutoCAD todavía no respondió a la petición anterior de la ventana: espera a que termine o pulsa «Dejar de esperar».", rig.Window.UpdateAllBlockedText);
            rig.Window.StopWaiting();

            var dynamic = rig.OpenAndChange(UpdateAllSystem.Dynamic);
            rig.Window.RequestUpdateAll();
            var request = Assert.Single(rig.Door.UpdateAll);
            Assert.True(rig.Window.CanStopWaiting);
            rig.Window.StopWaiting();
            Assert.Contains(request.RequestId, rig.Door.Abandoned);
            Assert.False(IsWriting(pushBack));
            Assert.False(IsWriting(dynamic));
            Assert.Equal("Modificado", StatusOf(dynamic));
            Assert.StartsWith("Se dejó de esperar «Actualizar todo»", rig.Window.UpdateAllResultText);

            // A late answer does nothing.
            rig.Door.AnswerUpdateAll(UpdateAllRun.Execute(request, new Units()));
            Assert.Equal("Modificado", StatusOf(dynamic));
            Assert.Equal("Actualizar todo (2)", rig.Window.UpdateAllLabel);
        }

        [Fact]
        public void ADoorThatRefusesSaysWhyAndLeavesEveryEditorEditable()
        {
            var rig = new Rig();
            var cabecera = rig.OpenAndChange(UpdateAllSystem.Cabecera);
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            rig.Window.RequestUpdateAll();
            Assert.Single(rig.Door.UpdateAll);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.False(IsWriting(cabecera));
            Assert.Equal("Modificado", StatusOf(cabecera));
            Assert.True(rig.Window.UpdateAllResultFailed);
            Assert.StartsWith("No se escribió nada:", rig.Window.UpdateAllResultText);
        }

        // ------------------------------------------------------------------ the mark of part st (the predicate the session connects)

        [Fact]
        public void ADraftMarkedPossiblyStaleIsIncludedAndAsksFirstInLine()
        {
            var rig = new Rig();
            var marked = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "DIN-1" };
            rig.Window.UpdateAllStaleProbe = (session, rack) => session == Session && rack != null && marked.Contains(rack);
            rig.OpenAndChange(UpdateAllSystem.PushBack);
            rig.OpenAndChange(UpdateAllSystem.Dynamic);
            Assert.Equal("Actualizar todo (2)", rig.Window.UpdateAllLabel);
            Assert.True(rig.Window.UpdateAllEligibility.NeedsConfirmation);

            rig.Window.RequestUpdateAll();
            Assert.Empty(rig.Door.UpdateAll);
            Assert.True(rig.Window.IsConfirmingUpdateAll);
            Assert.True(rig.Window.ShowUpdateAllPanel);
            Assert.Equal("«DIN 1» puede estar desactualizado: corrió una orden en AutoCAD desde que se leyeron. «Actualizar todo» escribirá su borrador encima de lo que hay ahora en el dibujo.",
                rig.Window.UpdateAllConfirmText);
            Assert.False(rig.Window.CanUpdateAll);

            rig.Window.CancelUpdateAll();
            Assert.False(rig.Window.IsConfirmingUpdateAll);
            Assert.Empty(rig.Door.UpdateAll);

            rig.Window.RequestUpdateAll();
            rig.Window.ConfirmUpdateAll();
            Assert.Equal(2, Assert.Single(rig.Door.UpdateAll).Items.Count);

            // Without the mark nothing asks.
            rig.Door.AnswerLastUpdateAll();
            marked.Clear();
            rig.OpenAndChange(UpdateAllSystem.Cama);
            rig.Window.RequestUpdateAll();
            Assert.False(rig.Window.IsConfirmingUpdateAll);
            Assert.Equal(2, rig.Door.UpdateAll.Count);
        }

        // ------------------------------------------------------------------ the view (real WPF, STA)

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

        private static void Lay(FrameworkElement view, double width = 1300, double height = 900)
        {
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
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
        public void TheHeaderOffersActualizarTodoTheQuestionIsInLineAndTheListSaysEveryDraft()
        {
            StaTestRunner.Run(() =>
            {
                var errors = new BindingErrors();
                PresentationTraceSources.Refresh();
                var source = PresentationTraceSources.DataBindingSource;
                var level = source.Switch.Level;
                source.Listeners.Add(errors);
                source.Switch.Level = SourceLevels.Error;
                try
                {
                    var rig = new Rig();
                    var view = new RackCadWindowView(rig.Window);
                    Lay(view);
                    Assert.Equal(Visibility.Visible, view.UpdateAllButton.Visibility);
                    Assert.Equal("Actualizar todo (0)", view.UpdateAllButton.Content);
                    Assert.False(view.UpdateAllButton.IsEnabled);
                    Assert.Equal(RackCadWindowViewModel.NothingToWriteText, view.UpdateAllButton.ToolTip);
                    Assert.Equal("Actualizar todo: escribir en el dibujo todos los borradores con cambios", Name(view.UpdateAllButton));
                    Assert.Equal(Visibility.Collapsed, view.UpdateAllPanel.Visibility);

                    rig.Window.UpdateAllStaleProbe = (_, rack) => rack == "PB-1";
                    rig.OpenAndChange(UpdateAllSystem.PushBack);
                    rig.OpenAndChange(UpdateAllSystem.Cama);
                    Lay(view);
                    Assert.Equal("Actualizar todo (2)", view.UpdateAllButton.Content);
                    Assert.True(view.UpdateAllButton.IsEnabled);

                    Click(view.UpdateAllButton);
                    Lay(view);
                    Assert.Equal(Visibility.Visible, view.UpdateAllPanel.Visibility);
                    Assert.Equal(Visibility.Visible, view.UpdateAllQuestion.Visibility);
                    Assert.Contains("«PB 1» puede estar desactualizado", view.UpdateAllQuestionText.Text);
                    var names = new UIElement[] { view.UpdateAllButton, view.ConfirmUpdateAllButton, view.CancelUpdateAllButton, view.HelpButton, view.StopWaitingButton }
                        .Select(Name).ToList();
                    Assert.Equal(names.Count, names.Distinct().Count());

                    Click(view.ConfirmUpdateAllButton);
                    rig.Door.AnswerLastUpdateAll(new Units { FailRack = "CAMA-1" });
                    Lay(view);
                    Assert.Equal(Visibility.Collapsed, view.UpdateAllQuestion.Visibility);
                    Assert.Equal(Visibility.Visible, view.UpdateAllResultPanel.Visibility);
                    Assert.StartsWith("No se escribió nada: falló «Cama 1».", view.UpdateAllResultText.Text);
                    var lines = Descendants<TextBlock>(view.UpdateAllResultList).Select(t => Name(t)).ToList();
                    Assert.Equal(new[] { "1. PB 1 (Push Back): No aplicado. Falló otro rack del conjunto.",
                        "2. Cama 1 (Cama): No se pudo escribir. AutoCAD no redibujó ninguna vista del rack." }, lines);
                    Assert.DoesNotContain(Descendants<TextBlock>(view.UpdateAllPanel), t => (t.Text ?? string.Empty).StartsWith("RackCad.", StringComparison.Ordinal));

                    // Narrow, the header still holds the button.
                    Lay(view, 600, 700);
                    Assert.Equal(Visibility.Visible, view.UpdateAllButton.Visibility);
                    Assert.True(errors.Messages.Count == 0, string.Join(Environment.NewLine, errors.Messages.Take(5)));
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }
            });
        }
    }
}
