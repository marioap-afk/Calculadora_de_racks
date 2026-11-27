#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.Larguero;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, fourth batch, part «ut» (prototype «utilidades», UT-01 to UT-12): «Biblioteca de diseños» of the RackCad window
    /// (the folder read with its unreadable files, a folder that does not exist told apart from an empty one, the search and the filter by
    /// system with the names of the window, the inspector, «Abrir» as a new Selectivo or Dinámico in «Nuevo» or in the classic editor through
    /// the numbered door, and «Guardar en biblioteca…» of the editors without the dialog of Windows, with «Reemplazar» and «Cambiar el
    /// nombre»), «Órdenes y atajos» behind «Ayuda» (the list, «Ejecutar» by the door, «En la ventana»), «Configuración» (the folder of the
    /// designs, «Abrir RackCad al iniciar AutoCAD», «Acerca de RackCad»), the additions of «Nuevo», and the views with real WPF. The folder
    /// of the library is a temporary folder of each test: nothing reads or writes the settings of the machine.
    /// </summary>
    public class RackCadUtilitiesTests
    {
        // ------------------------------------------------------------------ the doubles

        /// <summary>The door of the classic flows, with the door of the utilities (as the port of the Plugin since part «ut»).</summary>
        private sealed class UtilityDoorDouble : IRackCadClassicCommandPort, IRackCadUtilityCommandPort
        {
            public List<RackCadClassicAction> Runs { get; } = new();

            public List<string> HelpRuns { get; } = new();

            public List<RackCadLibraryDesignRequest> LibraryOpens { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public RackCadClassicResult Run(RackCadClassicAction action)
            {
                Runs.Add(action);
                return Answer;
            }

            public RackCadClassicResult RunHelpCommand(string command)
            {
                HelpRuns.Add(command);
                return Answer;
            }

            public RackCadClassicResult OpenLibraryDesign(RackCadLibraryDesignRequest request)
            {
                LibraryOpens.Add(request);
                return Answer;
            }
        }

        /// <summary>The door of the classic flows only, as before part «ut».</summary>
        private sealed class ClassicOnlyDouble : IRackCadClassicCommandPort
        {
            public RackCadClassicResult Run(RackCadClassicAction action) => RackCadClassicResult.Started();
        }

        /// <summary>The door that reads and writes a Selectivo and a Dinámico.</summary>
        private sealed class DoorDouble : IRackCadSelectiveDoorPort
        {
            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<RackDoorWrite> Writes { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten { add { } remove { } }

            public event Action<long>? RequestStarted { add { } remove { } }

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write)
            {
                Writes.Add(write);
                return RackCadClassicResult.Started();
            }

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

            public void AnswerLastSelectiveRead(Domain.Systems.Selective.SelectivePalletDesign? design = null)
            {
                var request = SelectiveReads.Last();
                SelectiveRead?.Invoke(Read(request.RequestId, request.RackId, design));
            }
        }

        /// <summary>
        /// The settings, in memory, with the utilities; the folder of the designs is a temporary folder of the test. Every read counts
        /// (<see cref="Reads"/>): the window reads them only when «Configuración» or «Biblioteca» is shown (review of part «ut», point 4).
        /// </summary>
        private sealed class SettingsDouble : IRackCadSettingsAccess, IRackCadUtilitySettings
        {
            private string _blockLibraryPath = "C:\\bloques.dwg";
            private bool _blockLibraryOverridden;
            private bool _blockLibraryExists = true;
            private bool _openOnStartup;

            public SettingsDouble(string defaultFolder) => DefaultFolder = defaultFolder;

            /// <summary>How many times a setting was read (any member of the two interfaces that reads).</summary>
            public int Reads { get; set; }

            public string DefaultFolder { get; }

            public string? CustomFolder { get; set; }

            public string BlockLibraryPath
            {
                get { Reads++; return _blockLibraryPath; }
                set => _blockLibraryPath = value;
            }

            public bool BlockLibraryOverridden
            {
                get { Reads++; return _blockLibraryOverridden; }
                set => _blockLibraryOverridden = value;
            }

            public bool BlockLibraryExists
            {
                get { Reads++; return _blockLibraryExists; }
                set => _blockLibraryExists = value;
            }

            public string DesignLibraryPath
            {
                get { Reads++; return CustomFolder ?? DefaultFolder; }
            }

            public bool DesignLibraryOverridden
            {
                get { Reads++; return CustomFolder != null; }
            }

            public void SetBlockLibraryPath(string? path)
            {
                _blockLibraryPath = path ?? "C:\\predeterminada.dwg";
                _blockLibraryOverridden = path != null;
            }

            public void SetDesignLibraryPath(string? path) => CustomFolder = path;

            public bool OpenOnStartup
            {
                get { Reads++; return _openOnStartup; }
            }

            public int StartupWrites { get; private set; }

            public void SetOpenOnStartup(bool open)
            {
                _openOnStartup = open;
                StartupWrites++;
            }

            public string SettingsFilePath
            {
                get { Reads++; return "C:\\Usuario\\AppData\\Roaming\\RackCad\\settings.json"; }
            }

            public RackCadAboutInfo About
            {
                get { Reads++; return new("1.0.0 (abc12345)", "C:\\RackCad.bundle\\Contents\\catalogs", "C:\\Usuario\\AppData\\Roaming\\RackCad\\logs"); }
            }
        }

        private sealed class Rig : IDisposable
        {
            public Rig(bool withDoor = true, bool utilityDoor = true, bool utilitySettings = true)
            {
                Root = Path.Combine(Path.GetTempPath(), "RackCadUtTest_" + Guid.NewGuid().ToString("N"));
                Folder = Path.Combine(Root, "Designs");
                Directory.CreateDirectory(Folder);
                Settings = new SettingsDouble(Folder);
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble { ListResponder = (_, _) => BrowserRackList.Ready(Listed) };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                IRackCadClassicCommandPort classic = utilityDoor ? Classic : new ClassicOnlyDouble();
                IRackCadSettingsAccess settings = utilitySettings ? Settings : new PlainSettings(Settings);
                Window = new RackCadWindowViewModel(Panel, classic, settings, withDoor ? Door : null, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public string Root { get; }

            public string Folder { get; }

            public SettingsDouble Settings { get; }

            public UtilityDoorDouble Classic { get; } = new();

            public DoorDouble Door { get; } = new();

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public RackCadWindowViewModel Window { get; }

            public RackCadLibraryViewModel Library => Window.Library;

            public List<BrowserRackListEntry> Listed { get; } = new()
            {
                new BrowserRackListEntry("SEL-1", "Selectivo A", "selective", new[] { "frontal" }),
                new BrowserRackListEntry("DIN-1", "DIN 1", "dynamic", new[] { "lateral" }),
            };

            public string File(string name) => Path.Combine(Folder, name + RackProjectStore.FileExtension);

            public SelectiveEditorViewModel OpenSelectiveEditor()
            {
                Window.OpenRack("SEL-1");
                Window.EditSelectiveHere();
                Door.AnswerLastSelectiveRead();
                Assert.True(Window.ActiveSelectiveEditor != null, Window.ActionMessage);
                return Window.ActiveSelectiveEditor!;
            }

            public void Dispose()
            {
                try
                {
                    Directory.Delete(Root, true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        /// <summary>A settings access without the utilities (the controls of the utilities are then disabled).</summary>
        private sealed class PlainSettings : IRackCadSettingsAccess
        {
            private readonly SettingsDouble _inner;

            public PlainSettings(SettingsDouble inner) => _inner = inner;

            public string BlockLibraryPath => _inner.BlockLibraryPath;

            public bool BlockLibraryOverridden => _inner.BlockLibraryOverridden;

            public bool BlockLibraryExists => _inner.BlockLibraryExists;

            public string DesignLibraryPath => _inner.DesignLibraryPath;

            public void SetBlockLibraryPath(string? path) => _inner.SetBlockLibraryPath(path);
        }

        // ------------------------------------------------------------------ designs in the folder

        private static readonly RackProjectStore Projects = new();

        private static void SaveSelective(string path, string name, DateTime? modified = null)
        {
            Projects.Save(RackProject.ForSelectiveRack(SelectiveLibraryExport.FromEffective(Structure(), Guid.NewGuid().ToString(), name)), path);
            if (modified.HasValue) System.IO.File.SetLastWriteTimeUtc(path, modified.Value);
        }

        private static void SaveDynamic(string path, DateTime? modified = null)
        {
            var draft = DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe());
            var snapshot = draft.SnapshotForWrite()!;
            Projects.Save(RackProject.ForDynamic(snapshot.Design, snapshot.System), path);
            if (modified.HasValue) System.IO.File.SetLastWriteTimeUtc(path, modified.Value);
        }

        private static void SaveCama(string path, DateTime? modified = null)
        {
            Projects.Save(RackProject.ForCama(new FlowBedConfiguration { LaneDepth = 120.0 }), path);
            if (modified.HasValue) System.IO.File.SetLastWriteTimeUtc(path, modified.Value);
        }

        private static void SaveLarguero(string path, string name, DateTime? modified = null)
        {
            Projects.Save(RackProject.ForLarguero(new LargueroDesign { Name = name, BeamProfileId = "BEAM_X" }), path);
            if (modified.HasValue) System.IO.File.SetLastWriteTimeUtc(path, modified.Value);
        }

        private static readonly DateTime T0 = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        // Four designs and a broken file: Selectivo (newest), Dinámico, Cama, Larguero (oldest).
        private static void Fill(Rig rig)
        {
            SaveSelective(rig.File("Selectivo 3 niveles"), "Selectivo 3 niveles", T0.AddHours(4));
            SaveDynamic(rig.File("Dinamico 6 fondos"), T0.AddHours(3));
            SaveCama(rig.File("Cama 48 in"), T0.AddHours(2));
            SaveLarguero(rig.File("archivo larguero"), "Larguero 96 in", T0.AddHours(1));
            System.IO.File.WriteAllText(rig.File("copia rota"), "{ esto no es un diseño");
        }

        // ------------------------------------------------------------------ the folder (UT-06, UT-07, UT-08)

        [Fact]
        public void TheLibraryReadsItsFolderWhenItsPageIsShownWithTheNamesOfTheWindowAndSaysWhichFilesItCouldNotRead()
        {
            using var rig = new Rig();
            Fill(rig);
            Assert.False(rig.Library.HasRead);

            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);

            Assert.Equal(rig.Folder, rig.Library.Folder);
            Assert.Equal(new[] { "Selectivo 3 niveles", "Dinamico 6 fondos", "Cama 48 in", "Larguero 96 in" }, rig.Library.Rows.Select(r => r.Name).ToArray());
            // UT-07: «Dinámico» and «Cama», as the column of the window, not «Sistema dinámico» nor «Cama de rodamiento».
            Assert.Equal(new[] { "Selectivo", "Dinámico", "Cama", "Larguero" }, rig.Library.Rows.Select(r => r.KindLabel).ToArray());
            Assert.Equal(DateTime.SpecifyKind(T0.AddHours(4), DateTimeKind.Utc).ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), rig.Library.Rows[0].ModifiedText);
            // UT-06: the broken file is said, by its name.
            Assert.True(rig.Library.HasUnreadable);
            Assert.Equal("1 archivo de la carpeta no se pudo leer", rig.Library.UnreadableTitle);
            Assert.Equal("copia rota.rackcad.json: no aparece en la lista. Puede estar dañado o ser de otro programa.", rig.Library.UnreadableText);
            Assert.False(rig.Library.FolderMissing);
            Assert.False(rig.Library.FolderEmpty);
            Assert.Equal("Mostrando 4 de 4", rig.Library.ShownText);
            // The first design is chosen, as the classic list does.
            Assert.Equal("Selectivo 3 niveles", rig.Library.Selected!.Name);
        }

        [Fact]
        public void AFolderThatDoesNotExistIsNotAnEmptyFolder()
        {
            using var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Assert.True(rig.Library.FolderEmpty);
            Assert.False(rig.Library.FolderMissing);
            Assert.False(rig.Library.HasAny);

            rig.Window.SetDesignLibraryPath(Path.Combine(rig.Root, "no existe"));
            Assert.True(rig.Library.FolderMissing);
            Assert.False(rig.Library.FolderEmpty);
            Assert.Equal("La carpeta todavía no existe: se crea al guardar el primer diseño. Para volver a la de siempre, «Restablecer» en Configuración.",
                rig.Library.FolderMissingText);

            // The default folder that does not exist says only the first part.
            Directory.Delete(rig.Folder);
            rig.Window.ResetDesignLibraryPath();
            Assert.True(rig.Library.FolderMissing);
            Assert.Equal("La carpeta todavía no existe: se crea al guardar el primer diseño.", rig.Library.FolderMissingText);
        }

        [Fact]
        public void TheSearchHasNoCaseNoAccentsAndTheFilterBySystemCountsEachSystem()
        {
            using var rig = new Rig();
            Fill(rig);
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);

            Assert.Equal(new[] { "Todos los sistemas (4)", "Selectivo (1)", "Push Back (0)", "Dinámico (1)", "Cantilever (0)", "Cabecera (0)", "Cama (1)", "Larguero (1)" },
                rig.Library.KindOptions.Select(o => o.Label).ToArray());

            rig.Library.SearchText = "DINÁMICO";
            Assert.Equal(new[] { "Dinamico 6 fondos" }, rig.Library.Rows.Select(r => r.Name).ToArray());
            Assert.Equal("Mostrando 1 de 4", rig.Library.ShownText);
            // The chosen design is out of the filter: the inspector has nothing.
            Assert.Null(rig.Library.Selected);
            Assert.True(rig.Library.NoSelection);

            // The file name is searched too («archivo larguero» lists «Larguero 96 in»).
            rig.Library.SearchText = "archivo";
            Assert.Equal(new[] { "Larguero 96 in" }, rig.Library.Rows.Select(r => r.Name).ToArray());

            rig.Library.SearchText = "zzz";
            Assert.True(rig.Library.NoMatch);

            rig.Library.SearchText = string.Empty;
            rig.Library.SelectedKind = RackSystemKind.Cama.ToString();
            Assert.Equal(new[] { "Cama 48 in" }, rig.Library.Rows.Select(r => r.Name).ToArray());
        }

        [Fact]
        public void TheInspectorSaysWhatOpenDoesAndWhereTheNameComesFrom()
        {
            using var rig = new Rig();
            Fill(rig);
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);

            Select(rig, "Selectivo 3 niveles");
            Assert.True(rig.Library.SelectionOpensHere);
            Assert.False(rig.Library.SelectionOpensClassic);
            Assert.Equal("Se abre en «Nuevo» con el editor de Selectivo de la ventana, sin bloquear AutoCAD, como un rack nuevo con el nombre del diseño. "
                         + "No toca el dibujo hasta «Insertar en el dibujo…».", rig.Library.SelectionNote);
            Assert.Equal("El nombre que se ve es el que lleva el diseño.", rig.Library.SelectionNameNote);

            Select(rig, "Dinamico 6 fondos");
            Assert.True(rig.Library.SelectionOpensHere);
            Assert.Equal("El nombre que se ve es el del archivo: así lo lee hoy la biblioteca para este sistema.", rig.Library.SelectionNameNote);

            Select(rig, "Cama 48 in");
            Assert.True(rig.Library.SelectionOpensClassic);
            Assert.Equal("El editor de Cama aún es el clásico: se abre aparte, ya con este diseño, y bloquea AutoCAD hasta que lo cierres, como hoy. "
                         + "Cuando Cama llegue a la ventana, se abrirá aquí.", rig.Library.SelectionNote);

            Select(rig, "Larguero 96 in");
            // T6 lg (point 5): a larguero opens in the page «Larguero» of the window, which needs no drawing.
            Assert.True(rig.Library.SelectionOpensHere);
            Assert.StartsWith("Se abre en «Nuevo › Larguero», sin bloquear AutoCAD y sin dibujo", rig.Library.SelectionNote, StringComparison.Ordinal);
            Assert.Equal("archivo larguero.rackcad.json", rig.Library.Selected!.FileName);
        }

        private static void Select(Rig rig, string name) => rig.Library.Select(rig.Library.Rows.Single(r => r.Name == name));

        // ------------------------------------------------------------------ «Abrir» (UT-03)

        [Fact]
        public void ASelectivoDesignOpensAsANewSelectivoInNuevoWithItsNameAndNeverTouchesTheDrawingUntilItIsInserted()
        {
            using var rig = new Rig();
            Fill(rig);
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Select(rig, "Selectivo 3 niveles");

            rig.Library.OpenSelected();

            var editor = rig.Window.NewSelectiveEditor;
            Assert.NotNull(editor);
            Assert.True(rig.Window.IsNuevoPage);
            Assert.True(rig.Window.ShowNewSelectiveEditor);
            Assert.True(editor!.IsNewRack);
            Assert.Equal("Selectivo 3 niveles", editor.Name);
            Assert.False(editor.IsDirty);
            Assert.Equal(2, editor.Draft.FrontCount(0));
            // The design of the library, loaded as the window loads it (the draft recomposes, as the classic load does: S-03).
            var expected = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
            Assert.True(expected.LoadFrom(Structure(), null, "Selectivo 3 niveles"));
            Assert.Equal(Json(expected.Design), Json(editor.Draft.Design));
            // T4 integration: the design opens through the entry point of the drafts (part «new»), as «Borrador 1».
            Assert.StartsWith("Borrador 1: Selectivo nuevo en esta ventana, " + RackCadWindowViewModel.LibraryOrigin("Selectivo 3 niveles") + ".", editor.Message);
            Assert.Equal(1, Assert.Single(rig.Window.Drafts).Number);
            // A new identity, and nothing was written: a design never replaces a rack of the drawing.
            Assert.NotEqual("SEL-1", editor.RackId);
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.Empty(rig.Classic.LibraryOpens);

            // «Insertar en el dibujo…» inserts it as a new rack, with the design of the library (T4 ins: the panel of «Insertar vista…»).
            InsertFirstView(editor);
            var write = Assert.Single(rig.Door.SelectiveWrites);
            Assert.True(write.NewRack);
            Assert.Equal(editor.RackId, write.RackId);
            Assert.Equal("Selectivo 3 niveles", write.Name);
            Assert.Equal(2, write.Design.Bays.Count);
        }

        [Fact]
        public void ADinamicoDesignOpensAsANewDinamicoInNuevoWithTheNameOfItsFile()
        {
            using var rig = new Rig();
            Fill(rig);
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Select(rig, "Dinamico 6 fondos");

            rig.Library.OpenSelected();

            var editor = rig.Window.NewDynamicEditor;
            Assert.NotNull(editor);
            Assert.True(rig.Window.ShowNewDynamicEditor);
            Assert.True(editor!.IsNewRack);
            Assert.Equal("Dinamico 6 fondos", editor.Name);
            Assert.False(editor.IsDirty);
            Assert.StartsWith("Borrador 1: Dinámico nuevo en esta ventana, " + RackCadWindowViewModel.LibraryOrigin("Dinamico 6 fondos") + ".", editor.Message);
            Assert.Empty(rig.Door.Writes);

            InsertFirstView(editor); // T4 ins: the panel of «Insertar vista…»
            var write = Assert.Single(rig.Door.Writes);
            Assert.True(write.NewRack);
            Assert.Equal("Dinamico 6 fondos", write.Name);
        }

        // T4 integration (SD-06): with a draft in «Nuevo», a design of the library opens as ANOTHER draft; the first one stays.
        [Fact]
        public void WithADraftInNuevoADesignOfTheLibraryOpensAsAnotherDraft()
        {
            using var rig = new Rig();
            Fill(rig);
            rig.Window.StartNewDynamic();
            var existing = rig.Window.NewDynamicEditor;
            Assert.NotNull(existing);
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Select(rig, "Selectivo 3 niveles");

            rig.Library.OpenSelected();

            var opened = rig.Window.NewSelectiveEditor;
            Assert.NotNull(opened);
            Assert.Equal("Selectivo 3 niveles", opened!.Name);
            Assert.False(rig.Window.ActionFailed);
            Assert.True(rig.Window.IsNuevoPage);
            Assert.Equal(new[] { (1, RackSystemKind.PalletFlow, existing!.RackId), (2, RackSystemKind.SelectiveRack, opened.RackId) },
                rig.Window.Drafts.Select(d => (d.Number, d.Kind, d.RackId)).ToArray());
        }

        // «Insertar en el dibujo…» with the first view the panel offers (T4 ins: the panel replaced the dialog of the classic editor).
        private static void InsertFirstView(SelectiveEditorViewModel editor)
        {
            editor.InsertView();
            var panel = editor.InsertPanel!;
            panel.Mark(panel.Groups.SelectMany(g => g.Options).First().Address);
            Assert.True(panel.Insert(), panel.Reason);
        }

        private static void InsertFirstView(DynamicEditorViewModel editor)
        {
            editor.InsertView();
            var panel = editor.InsertPanel!;
            panel.Mark(panel.Groups.SelectMany(g => g.Options).First().Address);
            Assert.True(panel.Insert(), panel.Reason);
        }

        [Fact]
        public void ADesignOfAnotherSystemAsksAutoCadForItsClassicEditorThroughTheNumberedDoor()
        {
            using var rig = new Rig();
            Fill(rig);
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Select(rig, "Cama 48 in");

            rig.Library.OpenSelected();

            // T6 lg (point 5): a larguero no longer goes to its classic editor: it opens in the page «Larguero» (RackCadLargueroPageTests).
            Assert.Equal(new[] { rig.File("Cama 48 in") }, rig.Classic.LibraryOpens.Select(r => r.Path).ToArray());
            Assert.Equal(new[] { 1L }, rig.Classic.LibraryOpens.Select(r => r.RequestId).ToArray());
            Assert.All(rig.Classic.LibraryOpens, r => Assert.Equal(new SessionId(7), r.Session));
            Assert.Null(RackCadLibraryDoorRules.RefusalOf(rig.Classic.LibraryOpens[0]));
            Assert.False(rig.Window.ActionFailed);
            Assert.StartsWith("Se pidió a AutoCAD abrir «Cama 48 in» en el editor clásico de Cama, ya cargado.", rig.Window.ActionMessage);
            Assert.Null(rig.Window.NewSelectiveEditor);
            Assert.Null(rig.Window.NewDynamicEditor);

            rig.Classic.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            rig.Library.OpenSelected();
            Assert.True(rig.Window.ActionFailed);
            Assert.Equal("Hay un comando en curso en AutoCAD: termínalo o cancélalo (Esc) y vuelve a intentarlo.", rig.Window.ActionMessage);
        }

        [Fact]
        public void WithoutTheDoorOfTheWindowASelectivoDesignGoesToItsClassicEditorAndWithoutTheDoorOfTheUtilitiesNothingOpens()
        {
            using var noDoor = new Rig(withDoor: false);
            Fill(noDoor);
            noDoor.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Select(noDoor, "Selectivo 3 niveles");
            Assert.True(noDoor.Library.SelectionOpensClassic);
            noDoor.Library.OpenSelected();
            Assert.Equal(noDoor.File("Selectivo 3 niveles"), Assert.Single(noDoor.Classic.LibraryOpens).Path);

            using var classicOnly = new Rig(utilityDoor: false);
            Fill(classicOnly);
            classicOnly.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Select(classicOnly, "Cama 48 in");
            classicOnly.Library.OpenSelected();
            Assert.True(classicOnly.Window.ActionFailed);
            Assert.Equal("Aquí no se puede abrir el editor clásico de Cama.", classicOnly.Window.ActionMessage);
        }

        // ------------------------------------------------------------------ «Guardar en biblioteca…» (UT-04)

        [Fact]
        public void SaveToLibraryOfASelectivoEditorWritesWhatTheEditorShowsWithTheDraftAsALiteralDesignWithoutTheDialogOfWindows()
        {
            using var rig = new Rig();
            var editor = rig.OpenSelectiveEditor();
            Write(Field(editor.CellFields, "cHeight"), "72");
            Assert.True(editor.IsDirty);

            editor.ToggleMore();
            Assert.True(editor.IsMoreOpen);
            editor.SaveToLibrary();

            Assert.False(editor.IsMoreOpen);
            Assert.True(rig.Window.IsBibliotecaPage);
            Assert.True(rig.Library.HasSave);
            Assert.Equal("Guardar «Selectivo A» en la biblioteca", rig.Library.SaveTitle);
            Assert.Equal("Selectivo A", rig.Library.SaveName);
            Assert.Equal("Archivo: Selectivo A.rackcad.json. Se guarda lo que ves en el editor, borrador incluido (este rack tiene cambios sin escribir). No toca el dibujo.",
                rig.Library.SaveHint);
            Assert.True(rig.Library.CanSave);
            Assert.False(System.IO.File.Exists(rig.File("Selectivo A")));

            rig.Library.Save();

            Assert.True(System.IO.File.Exists(rig.File("Selectivo A")));
            Assert.False(rig.Library.HasSave);
            Assert.Equal("Guardado en la biblioteca: «Selectivo A.rackcad.json», con lo que ves en el editor (borrador incluido). No toca el dibujo.", rig.Window.ActionMessage);
            Assert.Equal("Selectivo A", rig.Library.Selected!.Name);
            var saved = Projects.Load(rig.File("Selectivo A"));
            Assert.Equal(RackSystemKind.SelectiveRack, saved.Kind);
            Assert.Equal("Selectivo A", saved.SelectiveRack.Name);
            Assert.Null(saved.SelectiveRack.PropertyValues);
            // What the editor shows, the draft included: not what the drawing has.
            Assert.Equal(Json(editor.Draft.Design), Json(saved.SelectiveRack.ToDomain()));
            Assert.NotEqual(Json(Structure()), Json(saved.SelectiveRack.ToDomain()));
            // Nothing of the drawing: no write of the door, and the draft is still not written.
            Assert.Empty(rig.Door.SelectiveWrites);
            Assert.True(editor.IsDirty);
        }

        [Fact]
        public void AnEmptyNameIsNotSavedAndTheCharactersThatWindowsDoesNotAdmitBecomeUnderscores()
        {
            using var rig = new Rig();
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();

            rig.Library.SaveName = "   ";
            Assert.True(rig.Library.SaveNameMissing);
            Assert.Equal("Escribe un nombre: sin nombre no se guarda.", rig.Library.SaveNameError);
            Assert.False(rig.Library.CanSave);
            Assert.Equal(string.Empty, rig.Library.SaveHint);
            rig.Library.Save();
            Assert.Empty(Directory.GetFiles(rig.Folder));
            Assert.True(rig.Library.HasSave);

            rig.Library.SaveName = "Nave 3: pasillo/B";
            Assert.Equal("Nave 3_ pasillo_B.rackcad.json", rig.Library.SaveFile);
            Assert.StartsWith("Archivo: Nave 3_ pasillo_B.rackcad.json (los caracteres que Windows no admite pasan a «_»).", rig.Library.SaveHint);
            Assert.EndsWith("borrador incluido. No toca el dibujo.", rig.Library.SaveHint);
            rig.Library.Save();
            var saved = Projects.Load(Path.Combine(rig.Folder, "Nave 3_ pasillo_B.rackcad.json"));
            Assert.Equal("Nave 3: pasillo/B", saved.SelectiveRack.Name);
        }

        [Fact]
        public void ADesignOfTheSameFileAsksReplaceOrChangeTheNameAndReplaceWritesOverIt()
        {
            using var rig = new Rig();
            SaveCama(rig.File("Selectivo A"), T0);
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();

            rig.Library.Save();

            Assert.True(rig.Library.HasSaveConflict);
            Assert.False(rig.Library.CanSave);
            Assert.Equal("Ya hay un diseño «Selectivo A.rackcad.json» en la carpeta", rig.Library.SaveConflictTitle);
            Assert.Equal("Es un diseño de Cama guardado el " + T0.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + ". Si lo reemplazas, ese archivo se pierde.",
                rig.Library.SaveConflictText);
            Assert.Equal(RackSystemKind.Cama, Projects.Load(rig.File("Selectivo A")).Kind);

            rig.Library.Replace();

            Assert.False(rig.Library.HasSave);
            Assert.Equal(RackSystemKind.SelectiveRack, Projects.Load(rig.File("Selectivo A")).Kind);
            Assert.StartsWith("Reemplazado en la biblioteca: «Selectivo A.rackcad.json»", rig.Window.ActionMessage);
        }

        [Fact]
        public void ChangeTheNameTakesTheFirstFreeNameAndAnUnreadableFileIsSaidAsSuch()
        {
            using var rig = new Rig();
            System.IO.File.WriteAllText(rig.File("Selectivo A"), "roto");
            SaveCama(rig.File("Selectivo A (2)"));
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();
            rig.Library.Save();
            Assert.Equal("Es un archivo que la biblioteca no pudo leer. Si lo reemplazas, ese archivo se pierde.", rig.Library.SaveConflictText);

            rig.Library.RenameSave();

            Assert.Equal("Selectivo A (3)", rig.Library.SaveName);
            Assert.False(rig.Library.HasSaveConflict);
            rig.Library.Save();
            Assert.Equal("Selectivo A (3)", Projects.Load(rig.File("Selectivo A (3)")).SelectiveRack.Name);
            Assert.Equal("roto", System.IO.File.ReadAllText(rig.File("Selectivo A")));
        }

        [Fact]
        public void CancelSavesNothingAndAClosedEditorTakesItsLineWithIt()
        {
            using var rig = new Rig();
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();
            rig.Library.CancelSave();
            Assert.False(rig.Library.HasSave);
            Assert.Equal("No se guardó nada.", rig.Window.ActionMessage);
            Assert.Empty(Directory.GetFiles(rig.Folder));

            editor.SaveToLibrary();
            Assert.True(rig.Library.HasSave);
            editor.RequestClose(); // no changes: it closes at once
            Assert.Null(rig.Window.ActiveSelectiveEditor);
            Assert.False(rig.Library.HasSave);
            Assert.Empty(Directory.GetFiles(rig.Folder));
        }

        [Fact]
        public void AValueWrittenAndRefusedStopsTheSaveAndIsNamed()
        {
            using var rig = new Rig();
            var editor = rig.OpenSelectiveEditor();
            Write(Field(editor.CellFields, "cHeight"), "-5");

            editor.SaveToLibrary();

            Assert.False(rig.Library.HasSave);
            Assert.False(rig.Window.IsBibliotecaPage);
            Assert.True(editor.MessageIsError);
            Assert.StartsWith("No se puede guardar en la biblioteca: «", editor.Message);
            Assert.EndsWith("Corrígelo o vuelve a escribir el que tenía.", editor.Message);
        }

        [Fact]
        public void ANewDinamicoIsSavedWithTheSystemAndListedByItsFile()
        {
            using var rig = new Rig();
            rig.Window.StartNewDynamic();
            var editor = rig.Window.NewDynamicEditor!;
            editor.Name = "Dinámico patio";

            editor.SaveToLibrary();
            Assert.Equal("Archivo: Dinámico patio.rackcad.json. Se guarda lo que ves en el editor, borrador incluido (este rack nuevo todavía no está en el dibujo). No toca el dibujo.",
                rig.Library.SaveHint);
            rig.Library.Save();

            var saved = Projects.Load(rig.File("Dinámico patio"));
            Assert.Equal(RackSystemKind.PalletFlow, saved.Kind);
            Assert.NotNull(saved.DynamicDesign);
            Assert.Equal(Projects.Serialize(RackProject.ForDynamic(editor.Draft.Design)), Projects.Serialize(RackProject.ForDynamic(saved.DynamicDesign)));
            var row = rig.Library.Rows.Single();
            Assert.Equal(("Dinámico patio", "Dinámico"), (row.Name, row.KindLabel));
            Assert.Empty(rig.Door.Writes);
        }

        [Fact]
        public void BothEditorsOfferMoreAndSaveToLibraryOnlyWithTheWindow()
        {
            using var rig = new Rig();
            var selective = rig.OpenSelectiveEditor();
            Assert.True(selective.CanSaveToLibrary);
            // An editor that no window wired has no «Guardar en biblioteca…».
            var alone = SelectiveEditor();
            Assert.False(alone.CanSaveToLibrary);
            Assert.Equal("Aquí no se puede guardar en la biblioteca.", alone.SaveToLibraryBlockedText);
            alone.SaveToLibrary();
            Assert.Equal("Aquí no se puede guardar en la biblioteca.", alone.Message);
        }

        // ------------------------------------------------------------------ «Órdenes y atajos» (UT-09 to UT-11)

        [Fact]
        public void AyudaIsAPageOfTheWindowThatIsNotASectionOfTheColumn()
        {
            using var rig = new Rig();
            rig.Window.ShowHelp();

            Assert.True(rig.Window.IsAyudaPage);
            Assert.Equal(RackCadWindowPage.Ayuda, rig.Window.Page);
            Assert.Equal(new[] { "Nuevo", "Variables", "BOM y lista de racks", "Almacén", "Biblioteca de diseños", "Configuración" }, rig.Window.Sections.Select(s => s.Label).ToArray()); // T5 al: «Almacén», between the BOM and the library
            Assert.DoesNotContain(rig.Window.Sections, s => s.IsActive);

            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            Assert.False(rig.Window.IsAyudaPage);
        }

        [Fact]
        public void TheHelpListsTheCommandsOfRackAyudaWithTheMissingOnesAndWhereTheyAreInTheWindow()
        {
            using var rig = new Rig();
            var help = rig.Window.Help;

            Assert.Equal(new[] { "Diseñar", "Editar y copiar", "Almacén (layout)", "Reportes", "Ayuda" }, help.Groups.Select(g => g.Name).ToArray());
            Assert.Equal(new[] { "RACKCAD", "RACKMENU", "RACKSELECTIVO", "RACKCABECERA", "QUICKCABECERA", "RACKSISTEMADINAMICO", "QUICKCAMA", "RACKCANTILEVER",
                    "RACKPUSHBACK", "RACKSECCION" },
                help.Groups[0].Rows.Select(r => r.Command).ToArray());
            Assert.Equal("20 órdenes", help.CountText);

            var rows = RackCadHelpViewModel.AllRows.ToDictionary(r => r.Command);
            Assert.Equal("Ventana RackCad (no bloquea AutoCAD): secciones y lista de racks del dibujo.", rows["RACKCAD"].Summary);
            Assert.Equal(("sin atajo", false), (rows["RACKMENU"].AliasText, rows["RACKMENU"].HasAlias));
            Assert.Equal("sin atajo", rows["RACKSECCION"].AliasText);
            Assert.Equal("RVA", rows["RACKVARIABLES"].AliasText);
            Assert.Equal("RPB", rows["RACKPUSHBACK"].AliasText);
            // RACKCAD is the window itself: it is not run from here.
            Assert.False(rows["RACKCAD"].CanRun);
            Assert.All(rows.Values.Where(r => r.Command != "RACKCAD"), r => Assert.True(r.CanRun, r.Command));
            Assert.Equal(("Es esta ventana", false), (rows["RACKCAD"].WindowText, rows["RACKCAD"].HasWindowLink));
            Assert.Equal(("Nuevo", RackCadWindowPage.Nuevo), (rows["RACKSELECTIVO"].WindowText, rows["RACKSELECTIVO"].WindowPage!.Value));
            Assert.Equal(RackCadWindowPage.Variables, rows["RACKVARIABLES"].WindowPage);
            Assert.Equal(RackCadWindowPage.Bom, rows["RACKBOMTOTAL"].WindowPage);
            Assert.Equal("Solo como orden", rows["RACKDUPLICAR"].WindowText);
            Assert.Equal("Es esta página", rows["RACKAYUDA"].WindowText);
            // UT-11: the paragraph no longer promises a setting that does not exist.
            Assert.DoesNotContain("configuración", help.Intro, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TheCommandsTheHelpRunsAreExactlyItsListWithoutRackCad()
        {
            var listed = RackCommandReference.Commands.Select(c => c.Command).Where(c => c != "RACKCAD").ToArray();
            Assert.Equal(listed.OrderBy(c => c, StringComparer.Ordinal), RackCadHelpCommands.Runnable.OrderBy(c => c, StringComparer.Ordinal));
            Assert.Null(RackCadHelpCommands.TextOf("RACKCAD"));
        }

        [Fact]
        public void TheSearchOfTheHelpFindsByCommandAliasOrTextWithNoAccents()
        {
            using var rig = new Rig();
            var help = rig.Window.Help;

            help.SearchText = "rva";
            Assert.Equal(new[] { "RACKVARIABLES" }, help.Groups.SelectMany(g => g.Rows).Select(r => r.Command).ToArray());
            Assert.Equal("Mostrando 1 de 20", help.CountText);

            // «catalogo» finds «catálogo», and capitals do not count.
            help.SearchText = "catalogo aisc";
            Assert.Equal(new[] { "RACKSECCION" }, help.Groups.SelectMany(g => g.Rows).Select(r => r.Command).ToArray());

            help.SearchText = "zzz";
            Assert.True(help.NoMatch);
            Assert.Equal("Mostrando 0 de 20", help.CountText);
        }

        [Fact]
        public void RunHandsTheCommandToTheDoorOfTheWindowAndInTheWindowGoesToItsSection()
        {
            using var rig = new Rig();
            var help = rig.Window.Help;
            var rows = RackCadHelpViewModel.AllRows.ToDictionary(r => r.Command);

            help.Run(rows["RACKLAYOUT"]);
            help.Run(rows["RACKCAD"]); // not run
            Assert.Equal(new[] { "RACKLAYOUT" }, rig.Classic.HelpRuns);
            Assert.Equal("Se pidió a AutoCAD ejecutar RACKLAYOUT. El foco vuelve al dibujo; lo que la orden pregunte sale en la línea de órdenes.", rig.Window.ActionMessage);

            rig.Classic.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "x");
            help.Run(rows["RACKLISTA"]);
            Assert.True(rig.Window.ActionFailed);
            Assert.Equal("Hay un comando en curso en AutoCAD: termínalo o cancélalo (Esc) y vuelve a intentarlo.", rig.Window.ActionMessage);

            help.GoTo(rows["RACKVARIABLES"]);
            Assert.True(rig.Window.IsVariablesPage);
            help.GoTo(rows["RACKDUPLICAR"]); // no section: nothing changes
            Assert.True(rig.Window.IsVariablesPage);

            using var classicOnly = new Rig(utilityDoor: false);
            classicOnly.Window.Help.Run(rows["RACKLAYOUT"]);
            Assert.Equal("Esta acción no está disponible aquí.", classicOnly.Window.ActionMessage);
        }

        // ------------------------------------------------------------------ «Configuración» (UT-01, UT-02, UT-12)

        [Fact]
        public void TheFolderOfTheDesignsIsChangedAndResetInConfiguracion()
        {
            using var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Configuracion);
            Assert.Equal(rig.Folder, rig.Window.DesignLibraryPath);
            Assert.Equal("Carpeta predeterminada.", rig.Window.DesignLibraryStatus);
            Assert.True(rig.Window.CanChangeDesignLibrary);
            Assert.False(rig.Window.CanResetDesignLibrary);

            var shared = Path.Combine(rig.Root, "Compartida");
            rig.Window.SetDesignLibraryPath(shared);

            Assert.Equal(shared, rig.Settings.CustomFolder);
            Assert.Equal(shared, rig.Window.DesignLibraryPath);
            Assert.Equal("Carpeta elegida (guardada). Todavía no existe: se crea al guardar el primer diseño.", rig.Window.DesignLibraryStatus);
            Assert.True(rig.Window.CanResetDesignLibrary);
            Assert.Equal("Carpeta de diseños guardada: mira «Biblioteca de diseños».", rig.Window.ActionMessage);

            // A save creates the folder chosen.
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();
            rig.Library.Save();
            Assert.True(System.IO.File.Exists(Path.Combine(shared, "Selectivo A" + RackProjectStore.FileExtension)));
            // The state of the folder is the one read when the page was shown (point 4 of the review): shown again, it exists.
            rig.Window.ShowPage(RackCadWindowPage.Configuracion);
            Assert.Equal("Carpeta elegida (guardada).", rig.Window.DesignLibraryStatus);

            rig.Window.ResetDesignLibraryPath();
            Assert.Null(rig.Settings.CustomFolder);
            Assert.Equal("Carpeta de diseños restablecida a la predeterminada.", rig.Window.ActionMessage);
            Assert.False(rig.Window.CanResetDesignLibrary);
        }

        [Fact]
        public void OpenRackCadOnStartupIsOffByDefaultAndItsBoxWritesTheSetting()
        {
            using var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Configuracion);
            Assert.False(rig.Window.OpenOnStartup);
            Assert.Equal("Apagada (por defecto): la ventana se abre con RACKCAD o RK.", rig.Window.OpenOnStartupHint);

            rig.Window.OpenOnStartup = true;
            Assert.True(rig.Settings.OpenOnStartup);
            Assert.Equal("Encendida: al iniciar AutoCAD, RackCad abre la ventana con el primer dibujo. Se aplica desde el próximo arranque.", rig.Window.OpenOnStartupHint);
            rig.Window.OpenOnStartup = true; // the same value writes nothing
            Assert.Equal(1, rig.Settings.StartupWrites);

            rig.Window.OpenOnStartup = false;
            Assert.False(rig.Settings.OpenOnStartup);
        }

        [Fact]
        public void AboutRackCadAndTheIntroSayWhatSupportNeedsAndTheResetOfTheBlockLibraryOnlyForAChosenOne()
        {
            using var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Configuracion);
            Assert.Equal("1.0.0 (abc12345)", rig.Window.AboutVersion);
            Assert.Equal("C:\\RackCad.bundle\\Contents\\catalogs", rig.Window.AboutCatalogsFolder);
            Assert.Equal("C:\\Usuario\\AppData\\Roaming\\RackCad\\logs", rig.Window.AboutLogsFolder);
            Assert.Equal("Ajustes de este usuario en este equipo (C:\\Usuario\\AppData\\Roaming\\RackCad\\settings.json). No van en el dibujo y se guardan al cambiarlos.",
                rig.Window.SettingsIntro);

            Assert.False(rig.Window.CanResetBlockLibrary);
            rig.Window.SetBlockLibraryPath("D:\\otra.dwg");
            Assert.True(rig.Window.CanResetBlockLibrary);
        }

        [Fact]
        public void WithoutTheSettingsOfTheUtilitiesTheirControlsAreOff()
        {
            using var rig = new Rig(utilitySettings: false);
            rig.Window.ShowPage(RackCadWindowPage.Configuracion);
            Assert.False(rig.Window.HasUtilitySettings);
            Assert.False(rig.Window.CanChangeDesignLibrary);
            Assert.False(rig.Window.CanResetDesignLibrary);
            rig.Window.OpenOnStartup = true;
            Assert.False(rig.Window.OpenOnStartup);
            Assert.Equal(string.Empty, rig.Window.AboutVersion);
        }

        [Fact]
        public void TheRealSettingsAccessHasTheUtilities()
        {
            IRackCadSettingsAccess real = new RackCadSettingsAccess();
            Assert.IsAssignableFrom<IRackCadUtilitySettings>(real);
            var about = ((IRackCadUtilitySettings)real).About;
            Assert.StartsWith("1.", about.Version, StringComparison.Ordinal);
            Assert.EndsWith("logs", about.LogsFolder, StringComparison.Ordinal);
        }

        // ------------------------------------------------------------------ «Nuevo»

        [Fact]
        public void NuevoShowsTheThreeRecentDesignsAndWhereTheClassicMenuWent()
        {
            using var rig = new Rig();
            Fill(rig);
            // «Nuevo» reads nothing (point 4 of the review): before the library was shown it says how to see the recent designs.
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            Assert.False(rig.Library.HasRead);
            Assert.Empty(rig.Library.Recent);
            Assert.Equal("Abre «Biblioteca de diseños» para ver los diseños recientes.", rig.Library.RecentEmptyText);

            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            Assert.Equal(string.Empty, rig.Library.RecentEmptyText);
            Assert.Equal(new[] { "Selectivo 3 niveles", "Dinamico 6 fondos", "Cama 48 in" }, rig.Library.Recent.Select(r => r.Name).ToArray());
            Assert.Equal(11, rig.Window.ClassicMenuMap.Count);
            Assert.Equal(("Abrir de la biblioteca de diseños", "Sección «Biblioteca de diseños», sin ventana aparte"),
                (rig.Window.ClassicMenuMap[8].Menu, rig.Window.ClassicMenuMap[8].Where));

            rig.Library.SearchText = "cama";
            rig.Window.ShowRecentDesign(rig.Library.Recent[1]);
            Assert.True(rig.Window.IsBibliotecaPage);
            Assert.Equal(string.Empty, rig.Library.SearchText);
            Assert.Equal("Dinamico 6 fondos", rig.Library.Selected!.Name);
        }

        // ------------------------------------------------------------------ the corrections of the review of part «ut»

        // Point 1: the editor is asked again at «Guardar», because the user may go back to it between «Más» and «Guardar».
        [Fact]
        public void GuardarAsksTheSelectivoEditorAgainForAValueRefusedAfterMasAndTheLineStays()
        {
            using var rig = new Rig();
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();
            Assert.True(rig.Library.HasSave);

            Write(Field(editor.CellFields, "cHeight"), "-5");
            rig.Library.Save();

            Assert.False(System.IO.File.Exists(rig.File("Selectivo A")));
            Assert.True(rig.Library.HasSave);
            Assert.True(rig.Window.ActionFailed);
            Assert.StartsWith("No se puede guardar en la biblioteca: «", rig.Window.ActionMessage);
            Assert.EndsWith("Corrígelo o vuelve a escribir el que tenía.", rig.Window.ActionMessage);

            // Corrected in the editor, the same line saves.
            Write(Field(editor.CellFields, "cHeight"), "72");
            rig.Library.Save();
            Assert.True(System.IO.File.Exists(rig.File("Selectivo A")));
        }

        [Fact]
        public void ReemplazarAsksTheEditorAgainToo()
        {
            using var rig = new Rig();
            SaveCama(rig.File("Selectivo A"), T0);
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();
            rig.Library.Save();
            Assert.True(rig.Library.HasSaveConflict);

            Write(Field(editor.CellFields, "cHeight"), "-5");
            rig.Library.Replace();

            Assert.Equal(RackSystemKind.Cama, Projects.Load(rig.File("Selectivo A")).Kind);
            Assert.True(rig.Library.HasSave);
            Assert.StartsWith("No se puede guardar en la biblioteca: «", rig.Window.ActionMessage);
        }

        [Fact]
        public void TramosWrittenAfterMasAndNotAppliedStopGuardar()
        {
            using var rig = new Rig();
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();
            Assert.True(editor.CanStartSegments);

            editor.StartSegments();
            rig.Library.Save();

            Assert.False(System.IO.File.Exists(rig.File("Selectivo A")));
            Assert.Equal("Aplica los tramos con «Aplicar tramos» o descártalos antes de guardar en la biblioteca.", rig.Window.ActionMessage);
            Assert.True(rig.Library.HasSave);
        }

        [Fact]
        public void GuardarAsksTheDinamicoEditorAgainToo()
        {
            using var rig = new Rig();
            rig.Window.StartNewDynamic();
            var editor = rig.Window.NewDynamicEditor!;
            editor.Name = "Din C";
            editor.SaveToLibrary();

            var height = Field(editor.CellFields, "cHeight");
            height.Text = "alto";
            height.Commit();
            Assert.False(string.IsNullOrEmpty(height.Error));
            rig.Library.Save();

            Assert.False(System.IO.File.Exists(rig.File("Din C")));
            Assert.StartsWith("No se puede guardar en la biblioteca: «", rig.Window.ActionMessage);
            Assert.True(rig.Library.HasSave);
        }

        // Point 4: nothing of the settings nor of the disk is read when the window loads, refreshes or shows another page.
        [Fact]
        public void TheWindowReadsNoSettingUntilConfiguracionOrBibliotecaIsShownAndAgainOnlyWhenOneIsShownAgain()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                using var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                Lay(view);
                rig.Window.Refresh();
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                rig.Window.ShowHelp();
                Lay(view);

                Assert.Equal(0, rig.Settings.Reads);
                Assert.False(rig.Window.HasReadUtilitySettings);
                Assert.False(rig.Library.HasRead);
                Assert.Equal(string.Empty, rig.Window.DesignLibraryStatus);

                rig.Window.ShowPage(RackCadWindowPage.Configuracion);
                Lay(view);
                var once = rig.Settings.Reads;
                Assert.True(once > 0);
                Assert.True(rig.Window.HasReadUtilitySettings);
                Assert.Equal("Carpeta predeterminada.", view.ConfiguracionPage.DesignLibraryStatusText.Text);
                Assert.Equal(rig.Folder, view.ConfiguracionPage.DesignLibraryPathBox.Text);

                // A refresh and the bindings read the cache: no setting is read again.
                rig.Window.Refresh();
                Lay(view);
                Assert.Equal(once, rig.Settings.Reads);

                // Shown again, the settings are read again (a folder created meanwhile is seen).
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
                Assert.True(rig.Settings.Reads > once);
            }));
        }

        // Point 5: a conflict holds only while its file is in the folder read.
        [Fact]
        public void AConflictDoesNotOutliveItsFileWhenTheFolderChangesOrTheFileGoes()
        {
            using var rig = new Rig();
            SaveCama(rig.File("Selectivo A"), T0);
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();
            rig.Library.Save();
            Assert.True(rig.Library.HasSaveConflict);

            var other = Path.Combine(rig.Root, "Otra");
            rig.Window.SetDesignLibraryPath(other);
            Assert.False(rig.Library.HasSaveConflict);
            Assert.True(rig.Library.CanSave);
            rig.Library.Save();
            Assert.True(System.IO.File.Exists(Path.Combine(other, "Selectivo A" + RackProjectStore.FileExtension)));

            // The same in one folder: the file of the conflict removed outside, «Volver a leer la carpeta» drops the conflict.
            editor.SaveToLibrary();
            rig.Library.Save();
            Assert.True(rig.Library.HasSaveConflict);
            System.IO.File.Delete(Path.Combine(other, "Selectivo A" + RackProjectStore.FileExtension));
            rig.Library.Reread();
            Assert.False(rig.Library.HasSaveConflict);
        }

        // Point 7: a name that Windows reserves for a device takes «_» in its file, and the line says why.
        [Fact]
        public void ANameThatWindowsReservesIsSavedWithAnUnderscoreAndTheLineSaysWhy()
        {
            using var rig = new Rig();
            var editor = rig.OpenSelectiveEditor();
            editor.SaveToLibrary();

            rig.Library.SaveName = "con";
            Assert.Equal("con_.rackcad.json", rig.Library.SaveFile);
            Assert.StartsWith("Archivo: con_.rackcad.json («con» es un nombre que Windows reserva: el archivo lleva «_»).", rig.Library.SaveHint);
            rig.Library.Save();

            var saved = Projects.Load(Path.Combine(rig.Folder, "con_.rackcad.json"));
            Assert.Equal("con", saved.SelectiveRack.Name);
        }

        // Point 8: «Abrir» follows the drawing presented, and «Nuevo» does not call a library it could not read «vacía».
        [Fact]
        public void WhatOpenDoesFollowsTheDrawingPresented()
        {
            using var rig = new Rig();
            Fill(rig);
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Select(rig, "Selectivo 3 niveles");
            Assert.True(rig.Library.SelectionOpensHere);
            var raised = new List<string?>();
            rig.Library.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            rig.Host.Snapshot = WorkspacePanelFixtures.NoDocument();
            rig.Window.Refresh();

            Assert.Contains(nameof(RackCadLibraryViewModel.SelectionOpensHere), raised);
            Assert.Contains(nameof(RackCadLibraryViewModel.SelectionOpensClassic), raised);
            Assert.False(rig.Library.SelectionOpensHere);
            Assert.True(rig.Library.SelectionOpensClassic);
        }

        [Fact]
        public void TheRecentDesignsOfNuevoSayWhyThereAreNone()
        {
            using var rig = new Rig();
            Assert.Equal("Abre «Biblioteca de diseños» para ver los diseños recientes.", rig.Library.RecentEmptyText);

            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Assert.Equal("La biblioteca está vacía.", rig.Library.RecentEmptyText);

            System.IO.File.WriteAllText(rig.File("rota"), "{ no");
            rig.Library.Reread();
            Assert.Equal("Ningún archivo de la carpeta de la biblioteca se pudo leer.", rig.Library.RecentEmptyText);

            rig.Window.SetDesignLibraryPath(Path.Combine(rig.Root, "no existe"));
            Assert.Equal("La carpeta de la biblioteca todavía no existe.", rig.Library.RecentEmptyText);

            SaveCama(Path.Combine(rig.Folder, "Cama 1" + RackProjectStore.FileExtension));
            rig.Window.ResetDesignLibraryPath();
            Assert.Equal(string.Empty, rig.Library.RecentEmptyText);
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

        private static void Lay(FrameworkElement view, double width = 1200)
        {
            view.Measure(new Size(width, 900));
            view.Arrange(new Rect(0, 0, width, 900));
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
        public void TheHeaderOpensTheHelpPageAndItsRowsRunAndGoByClick()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                using var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                Lay(view);

                Assert.Equal("Ayuda: órdenes y atajos", Name(view.HelpButton));
                Click(view.HelpButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.AyudaPage.Visibility);
                Assert.Equal(Visibility.Collapsed, view.RackPage.Visibility);

                var run = Descendants<Button>(view.AyudaPage.HelpGroupList).Where(b => b.DataContext is RackCadHelpRow).ToList();
                var names = run.Select(Name).ToList();
                Assert.Equal(names.Count, names.Distinct(StringComparer.Ordinal).Count());
                var runLayout = run.Single(b => Name(b) == "Ejecutar RACKLAYOUT en AutoCAD");
                Click(runLayout);
                Assert.Equal(new[] { "RACKLAYOUT" }, rig.Classic.HelpRuns);
                Assert.False(run.Single(b => Name(b) == "Ejecutar RACKCAD en AutoCAD").IsEnabled);

                Click(run.Single(b => Name(b) == "RACKVARIABLES, en la ventana: Variables"));
                Lay(view);
                Assert.True(rig.Window.IsVariablesPage);
                Assert.Equal(Visibility.Collapsed, view.AyudaPage.Visibility);
            }));
        }

        [Fact]
        public void TheLibraryPageListsChoosesOpensAndSavesByClick()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                using var rig = new Rig();
                Fill(rig);
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
                Lay(view);

                var page = view.BibliotecaPage;
                Assert.Equal(Visibility.Visible, page.Visibility);
                Assert.Equal(Visibility.Visible, page.LibraryUnreadablePanel.Visibility);
                Assert.False(page.InspectorBelow);
                var rows = Descendants<Button>(page.LibraryRowList).ToList();
                Assert.Equal(4, rows.Count);
                Assert.StartsWith("Cama 48 in · Cama · ", Name(rows[2]));

                Click(rows[2]);
                Lay(view);
                Assert.Equal("Cama 48 in", rig.Library.Selected!.Name);
                Assert.Equal(Visibility.Collapsed, page.LibraryOpenHereButton.Visibility);
                Assert.Equal(Visibility.Visible, page.LibraryOpenClassicButton.Visibility);
                Click(page.LibraryOpenClassicButton);
                Assert.Equal(rig.File("Cama 48 in"), Assert.Single(rig.Classic.LibraryOpens).Path);

                // «Más» › «Guardar en biblioteca…» of a Selectivo editor, by its own view.
                var editor = rig.OpenSelectiveEditor();
                var editorView = new SelectiveEditorView(editor);
                Lay(editorView);
                Assert.Equal(Visibility.Collapsed, editorView.MorePanel.Visibility);
                Click(editorView.MoreButton);
                Lay(editorView);
                Assert.Equal(Visibility.Visible, editorView.MorePanel.Visibility);
                Click(editorView.SaveToLibraryButton);
                rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
                Lay(view);
                Assert.True(rig.Library.HasSave);
                Assert.Equal(Visibility.Visible, page.LibrarySavePanel.Visibility);
                Assert.Equal("Selectivo A", page.LibrarySaveNameBox.Text);
                Click(page.LibrarySaveButton);
                Lay(view);
                Assert.True(System.IO.File.Exists(rig.File("Selectivo A")));
                Assert.Equal(Visibility.Collapsed, page.LibrarySavePanel.Visibility);
                Assert.Equal(5, Descendants<Button>(page.LibraryRowList).Count());

                // Narrow, the inspector goes below the list.
                Lay(view, 560);
                Assert.True(page.InspectorBelow);
            }));
        }

        [Fact]
        public void TheConflictOfASaveShowsReplaceAndChangeTheName()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                using var rig = new Rig();
                SaveCama(rig.File("Selectivo A"), T0);
                var view = new RackCadWindowView(rig.Window);
                var editor = rig.OpenSelectiveEditor();
                editor.SaveToLibrary();
                Lay(view);
                var page = view.BibliotecaPage;
                Click(page.LibrarySaveButton);
                Lay(view);

                Assert.Equal(Visibility.Visible, page.LibrarySaveConflictPanel.Visibility);
                Assert.False(page.LibrarySaveButton.IsEnabled);
                Click(page.LibraryRenameButton);
                Lay(view);
                Assert.Equal("Selectivo A (2)", page.LibrarySaveNameBox.Text);
                Assert.Equal(Visibility.Collapsed, page.LibrarySaveConflictPanel.Visibility);
                Assert.True(page.LibrarySaveButton.IsEnabled);
            }));
        }

        [Fact]
        public void TheSettingsPageChangesTheBoxAndOffersTheResetOnlyForAChosenFolder()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                using var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Configuracion);
                Lay(view);
                var page = view.ConfiguracionPage;

                Assert.Equal(Visibility.Visible, page.Visibility);
                Assert.Equal(rig.Folder, page.DesignLibraryPathBox.Text);
                Assert.False(page.ResetDesignLibraryButton.IsEnabled);
                Assert.False(page.ResetBlockLibraryButton.IsEnabled);
                Assert.Equal("1.0.0 (abc12345)", page.AboutVersionBox.Text);

                page.OpenOnStartupCheck.IsChecked = true;
                Assert.True(rig.Settings.OpenOnStartup);
                Assert.StartsWith("Encendida", page.OpenOnStartupHintText.Text);

                rig.Window.SetDesignLibraryPath(Path.Combine(rig.Root, "otra"));
                Lay(view);
                Assert.True(page.ResetDesignLibraryButton.IsEnabled);
                Click(page.ResetDesignLibraryButton);
                Assert.Null(rig.Settings.CustomFolder);
            }));
        }

        [Fact]
        public void NuevoShowsItsAdditionsAndViewAllCommandsOpensTheHelpOfTheWindow()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                using var rig = new Rig();
                Fill(rig);
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view);

                // Not read yet: the line says how to see them (point 8), and there is no recent design.
                var extras = view.NuevoExtras;
                Assert.Empty(Descendants<Button>(extras.RecentDesignList));
                Assert.Equal(Visibility.Visible, extras.NoRecentText.Visibility);
                Assert.Equal("Abre «Biblioteca de diseños» para ver los diseños recientes.", extras.NoRecentText.Text);

                rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, extras.NoRecentText.Visibility);
                Assert.Equal(3, Descendants<Button>(extras.RecentDesignList).Count());
                Assert.Equal("Ver «Selectivo 3 niveles» en la biblioteca", Name(Descendants<Button>(extras.RecentDesignList).First()));
                Assert.False(extras.ClassicMenuMapExpander.IsExpanded);
                Assert.Empty(rig.Classic.Runs);

                Click(extras.ShowAllCommandsButton);
                Lay(view);
                Assert.True(rig.Window.IsAyudaPage);
                Assert.Empty(rig.Classic.Runs); // not the dialog of RACKAYUDA
            }));
        }

        [Fact]
        public void TheDinamicoEditorOffersMoreWithSaveToLibraryToo()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                using var rig = new Rig();
                rig.Window.StartNewDynamic();
                var editor = rig.Window.NewDynamicEditor!;
                editor.Name = "Din B";
                var view = new DynamicEditorView(editor);
                Lay(view);

                Assert.Equal("Más acciones del rack", Name(view.MoreButton));
                Click(view.MoreButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.MorePanel.Visibility);
                Click(view.SaveToLibraryButton);
                Assert.True(rig.Window.IsBibliotecaPage);
                Assert.Equal("Din B", rig.Library.SaveName);
            }));
        }
    }
}
