#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Larguero;
using RackCad.UI.StructuralSections;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, sexta tanda, part «lg» (the answers of the Owner): the page «Larguero» finished. «Insertar en el dibujo…» is a gesture of the user
    /// through the numbered door of the window (RACKLARGUEROINSERTAR) with the plan the page drew — the larguero with its ménsulas, or a bare
    /// AISC profile, the plan of the inspector of RACKSECCION —; «Guardar en la biblioteca…» writes the new field (the catalog and the section
    /// of an AISC section), «Abrir» of the library brings a larguero here, «Más» exports its BOM, and the name takes the automatic one as a
    /// suggestion. With the double of the door, and the views with real WPF (STA) and without binding errors.
    /// </summary>
    public class RackCadLargueroPageTests
    {
        // ------------------------------------------------------------------ the doubles

        /// <summary>The door of the window with the door of the page «Larguero».</summary>
        private sealed class LargueroDoorDouble : IRackCadLargueroDoorPort
        {
            public List<LargueroInsertRequest> Inserts { get; } = new();

            public List<RackDoorRequest> Reads { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten { add { } remove { } }

            public event Action<long>? RequestStarted;

            public event Action<LargueroInserted>? LargueroInserted;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                Reads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestLargueroInsert(LargueroInsertRequest request)
            {
                Inserts.Add(request);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            public void Answers(LargueroInserted answer) => LargueroInserted?.Invoke(answer);
        }

        /// <summary>The settings, in memory, with the folder of the designs in a temporary folder of the test.</summary>
        private sealed class SettingsDouble : IRackCadSettingsAccess, IRackCadUtilitySettings
        {
            public SettingsDouble(string folder) => Folder = folder;

            public string Folder { get; set; }

            public string BlockLibraryPath => "C:\\bloques.dwg";

            public bool BlockLibraryOverridden => false;

            public bool BlockLibraryExists => true;

            public string DesignLibraryPath => Folder;

            public bool DesignLibraryOverridden => true;

            public void SetBlockLibraryPath(string? path)
            {
            }

            public void SetDesignLibraryPath(string path) => Folder = path;

            public bool OpenOnStartup => false;

            public void SetOpenOnStartup(bool open)
            {
            }

            public string SettingsFilePath => "C:\\RackCad\\settings.json";

            public RackCadAboutInfo About => new("1.0.0", "C:\\catalogs", "C:\\logs");
        }

        /// <summary>The files of the export, in memory.</summary>
        private sealed class FilesDouble : IRackBomFiles
        {
            public Dictionary<string, byte[]> Written { get; } = new(StringComparer.OrdinalIgnoreCase);

            public List<string> Opened { get; } = new();

            public List<string> Shown { get; } = new();

            public string? SaveAsAnswer { get; set; }

            public string FallbackFolder => "C:\\Docs";

            public void Write(string path, byte[] content) => Written[path] = content;

            public bool Exists(string path) => Written.ContainsKey(path);

            public void Open(string path) => Opened.Add(path);

            public void ShowInFolder(string path) => Shown.Add(path);

            public string? SaveAs(string suggestedName, string extension, string initialFolder) => SaveAsAnswer;
        }

        private sealed class ClassicDouble : IRackCadClassicCommandPort
        {
            public RackCadClassicResult Run(RackCadClassicAction action) => RackCadClassicResult.Started();
        }

        private sealed class Rig : IDisposable
        {
            public Rig(bool withDoor = true, bool withDrawing = true)
            {
                Root = Path.Combine(Path.GetTempPath(), "RackCadLgTest_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Root);
                Settings = new SettingsDouble(Root);
                Host = new WorkspacePortDouble { Snapshot = withDrawing ? WorkspacePanelFixtures.Ready(session: 7) : WorkspacePanelFixtures.NoDocument() };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new List<BrowserRackListEntry>
                    {
                        new BrowserRackListEntry("DIN-1", "DIN 1", "dynamic", new[] { "lateral" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, new ClassicDouble(), Settings, withDoor ? Door : null, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public string Root { get; }

            public SettingsDouble Settings { get; }

            public LargueroDoorDouble Door { get; } = new();

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public RackCadWindowViewModel Window { get; }

            public FilesDouble Files { get; } = new();

            public LargueroWorkbenchViewModel Open()
            {
                Window.OpenLargueroWorkbench();
                Assert.True(Window.LargueroWorkbench != null, Window.ActionMessage);
                Window.LargueroWorkbench!.Files = Files;
                return Window.LargueroWorkbench!;
            }

            public string File(string name) => Path.Combine(Root, name + RackProjectStore.FileExtension);

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

        private static StructuralSectionCatalog Aisc() => new CsvStructuralSectionCatalogProvider(RackCad.Application.Catalogs.CatalogDirectory.Resolve()).Load();

        private static void ChooseAisc(LargueroWorkbenchViewModel workbench, string name)
        {
            workbench.SelectedCatalog = "aisc";
            workbench.SearchText = name;
            workbench.SelectedRow = workbench.Rows.Single(r => r.IsSection && r.Label == name);
            workbench.SearchText = "";
        }

        // ------------------------------------------------------------------ «Insertar en el dibujo…» (point 2)

        [Fact]
        public void InsertingIsAGestureThroughTheNumberedDoorWithThePlanThePageDrew()
        {
            using var rig = new Rig();
            var workbench = rig.Open();

            // Opening the page and choosing send nothing: no command leaves the window without a gesture.
            workbench.LengthText = "120";
            workbench.SelectedMensula = "MENSULA_TROQUEL_REDONDO_CAL_10";
            Assert.Empty(rig.Door.Inserts);
            Assert.True(workbench.CanInsert, workbench.InsertReason);
            Assert.Equal(string.Empty, workbench.InsertReason);
            var plan = workbench.Plan!;
            Assert.Equal(LargueroPieceKind.Larguero, plan.Kind);
            Assert.Equal("Larguero 120 in · P3", plan.Name);

            workbench.Insert();

            var request = Assert.Single(rig.Door.Inserts);
            Assert.Equal(1L, request.RequestId);
            Assert.Equal(new SessionId(7), request.Session);
            Assert.Same(plan, request.Plan); // the one the page drew, not a second one
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("Esperando a que AutoCAD inserte la pieza de «Larguero». Si no responde (por ejemplo, porque había texto a medio escribir en la "
                         + "línea de órdenes), pulsa «Dejar de esperar».", rig.Window.WaitingText);
            Assert.True(workbench.IsInserting);
            Assert.False(workbench.CanInsert);
            Assert.Equal("Esperando a que AutoCAD tome la petición n.º 1 (RACKLARGUEROINSERTAR).", workbench.InsertStatus);

            // The command took it: AutoCAD asks for the point, and «Dejar de esperar» would no longer stop it.
            rig.Door.Start(1);
            Assert.Equal("AutoCAD pide el punto de inserción en la línea de órdenes: indícalo en el dibujo (Esc cancela).", workbench.InsertStatus);
            Assert.False(rig.Window.CanStopWaiting);
            Assert.StartsWith("AutoCAD ya está con la inserción de «Larguero»", rig.Window.WaitingText, StringComparison.Ordinal);

            // An answer to another request is not this one.
            rig.Door.Answers(LargueroDoorRules.Inserted(99, plan, "OTRO"));
            Assert.True(workbench.IsInserting);

            rig.Door.Answers(LargueroDoorRules.Inserted(1, plan, "RACKCAD_LARGUERO_X"));
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.False(workbench.IsInserting);
            Assert.False(workbench.InsertFailed);
            Assert.StartsWith("AutoCAD insertó el larguero «Larguero 120 in · P3» como una pieza suelta (bloque «RACKCAD_LARGUERO_X»)", workbench.InsertStatus,
                StringComparison.Ordinal);
            Assert.True(workbench.CanInsert);
            Assert.Equal(string.Empty, rig.Window.ActionMessage); // the page says it; the window adds nothing while the page is shown

            // Cancelled in AutoCAD: said, and the page can insert again.
            workbench.Insert();
            rig.Door.Start(2);
            rig.Door.Answers(LargueroDoorRules.Cancelled(2));
            Assert.True(workbench.InsertFailed);
            Assert.Equal("No se insertó nada: se canceló el punto de inserción. El dibujo no cambió.", workbench.InsertStatus);
            Assert.Equal(new[] { 1L, 2L }, rig.Door.Inserts.Select(r => r.RequestId).ToArray());
        }

        [Fact]
        public void WithoutTheDoorOrADrawingOrWhileAnotherRequestWaitsInsertingIsOffAndSaysWhy()
        {
            using (var noDoor = new Rig(withDoor: false))
            {
                var workbench = noDoor.Open();
                Assert.False(workbench.CanInsert);
                Assert.Equal("Aquí no se puede escribir el dibujo.", workbench.InsertReason);
                workbench.Insert();
                Assert.True(workbench.InsertFailed);
                Assert.Equal("Aquí no se puede escribir el dibujo.", workbench.InsertStatus);
            }

            using (var noDrawing = new Rig(withDrawing: false))
            {
                var workbench = noDrawing.Open();
                Assert.Equal("Abre o crea un dibujo para insertar el larguero.", workbench.InsertReason);
                // A drawing opens: «Insertar en el dibujo…» follows (the header of the window follows the drawing).
                noDrawing.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 7);
                noDrawing.Window.Refresh();
                Assert.True(workbench.CanInsert, workbench.InsertReason);
                Assert.Empty(noDrawing.Door.Inserts);
            }

            using var rig = new Rig();
            var page = rig.Open();
            // Another request of the window waits (a read of a rack): the door takes one at a time.
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            Assert.Single(rig.Door.Reads);
            rig.Window.OpenLargueroWorkbench();
            Assert.False(page.CanInsert);
            Assert.Equal("La ventana espera a AutoCAD por otra petición: espera a que termine o pulsa «Dejar de esperar».", page.InsertReason);
            rig.Window.StopWaiting();
            Assert.True(page.CanInsert, page.InsertReason);

            // An incomplete larguero says what it misses.
            page.LengthText = "abc";
            Assert.False(page.CanInsert);
            Assert.Equal("Falta: " + LargueroPiecePlanner.NoLengthText, page.InsertReason);
            Assert.Null(page.Plan);
            Assert.False(page.HasSketch);
            Assert.Empty(rig.Door.Inserts);
        }

        [Fact]
        public void ARefusalOfTheDoorAndDejarDeEsperarAreSaidOnThePage()
        {
            using var rig = new Rig();
            var workbench = rig.Open();

            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            workbench.Insert();
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.False(workbench.IsInserting);
            Assert.True(workbench.InsertFailed);
            Assert.Equal("Hay un comando en curso en AutoCAD: termínalo o cancélalo (Esc) y vuelve a intentarlo.", workbench.InsertStatus);

            rig.Door.Answer = RackCadClassicResult.Started();
            workbench.Insert();
            Assert.True(rig.Window.CanStopWaiting);
            rig.Window.StopWaiting();
            Assert.Equal(new[] { 2L }, rig.Door.Abandoned.ToArray());
            Assert.False(workbench.IsInserting);
            Assert.Equal("Se dejó de esperar a AutoCAD: si la orden llega a correr, no insertará nada.", workbench.InsertStatus);
            // A late answer of the abandoned request changes nothing.
            rig.Door.Answers(LargueroDoorRules.Inserted(2, workbench.Plan!, "TARDE"));
            Assert.Equal("Se dejó de esperar a AutoCAD: si la orden llega a correr, no insertará nada.", workbench.InsertStatus);

            // Closed drawing while it waits: the request is dropped and said.
            workbench.Insert();
            rig.Host.Snapshot = WorkspacePanelFixtures.NoDocument();
            rig.Summary.Dead.Add(new SessionId(7));
            rig.Window.Refresh();
            Assert.False(workbench.IsInserting);
            Assert.Equal("Se cerró el dibujo de la inserción: no se insertó nada.", workbench.InsertStatus);
        }

        // ------------------------------------------------------------------ point 6: «Sí, una sola página»

        [Fact]
        public void ABareAiscProfileIsThePlanOfTheInspectorOfRackseccion()
        {
            using var rig = new Rig();
            var workbench = rig.Open();
            Assert.False(workbench.CanChooseProfile); // a larguero of RackCad
            workbench.IsProfilePiece = true;
            Assert.Equal(LargueroPieceKind.Larguero, workbench.PieceKind);
            Assert.False(workbench.ShowViewChoice);

            ChooseAisc(workbench, "W10X12");
            Assert.True(workbench.CanChooseProfile);
            Assert.True(workbench.ShowDetailChoice);
            workbench.IsProfilePiece = true;
            Assert.Equal(LargueroPieceKind.Profile, workbench.PieceKind);
            Assert.True(workbench.ShowViewChoice);
            workbench.LengthText = "60";
            workbench.SelectedView = "LongitudinalX";
            workbench.SelectedDetail = "Simplified";

            // No ménsula needed for a bare profile: the page can insert it.
            Assert.Null(workbench.SelectedMensula);
            Assert.True(workbench.CanInsert, workbench.InsertReason);
            Assert.Equal("Insertar el perfil en el dibujo", workbench.InsertAutomationName);

            // The plan of the inspector of RACKSECCION for the same section, length, view and detail.
            var inspector = new StructuralSectionInspectorState(Aisc())
            {
                Selected = workbench.Section!.Aisc,
                View = SectionViewKind.LongitudinalX,
                Detail = SectionDetailLevel.Simplified,
            };
            inspector.Length = 60;
            Assert.Equal(inspector.BuildPlan().Signature(), workbench.Plan!.SectionPlan.Signature());

            workbench.Insert();
            Assert.Equal(LargueroPieceKind.Profile, Assert.Single(rig.Door.Inserts).Plan.Kind);

            // A section of RackCad again: the larguero, and the view of the profile is not offered.
            rig.Door.Answers(LargueroDoorRules.Cancelled(1));
            workbench.SelectedCatalog = "rackcad";
            workbench.SelectedRow = workbench.Rows.First(r => r.IsSection);
            Assert.Equal(LargueroPieceKind.Larguero, workbench.PieceKind);
            Assert.False(workbench.ShowViewChoice);
        }

        [Fact]
        public void TheSFamilyShowsTheWarningOfAdr0023OnThePageAndTheOthersDoNot()
        {
            using var rig = new Rig();
            var workbench = rig.Open();
            ChooseAisc(workbench, "S10X25.4");
            workbench.SelectedMensula = "MENSULA_3_REMACHES_CAL_10";
            Assert.True(workbench.HasAuthorityText);
            Assert.Contains("ADR-0023", workbench.AuthorityText, StringComparison.Ordinal);
            Assert.Contains("NO es apta para CNC ni fabricación", workbench.AuthorityText, StringComparison.Ordinal);
            workbench.IsProfilePiece = true;
            Assert.True(workbench.HasAuthorityText); // the bare profile too

            ChooseAisc(workbench, "W10X12");
            Assert.False(workbench.HasAuthorityText);
            Assert.Equal(string.Empty, workbench.AuthorityText);
        }

        // ------------------------------------------------------------------ points 1 and 5: the library

        [Fact]
        public void ThePageSavesTheLargueroOfAiscWithTheNewFieldAndTheLibraryOpensItBackHere()
        {
            using var rig = new Rig();
            var workbench = rig.Open();
            ChooseAisc(workbench, "C10X15.3");
            workbench.SelectedMensula = "MENSULA_TROQUEL_REDONDO_CAL_10";
            workbench.LengthText = "144";
            workbench.NameText = "Mi larguero";

            workbench.SaveToLibrary();
            Assert.True(rig.Window.IsBibliotecaPage);
            Assert.True(rig.Window.Library.HasSave);
            Assert.Equal("Mi larguero", rig.Window.Library.SaveName);
            rig.Window.Library.Save();
            Assert.False(rig.Window.ActionFailed, rig.Window.ActionMessage);

            var path = rig.File("Mi larguero");
            using (var saved = JsonDocument.Parse(File.ReadAllText(path)))
            {
                var payload = saved.RootElement.GetProperty("Larguero");
                Assert.Equal("Mi larguero", payload.GetProperty("Name").GetString());
                Assert.Equal("AISC", payload.GetProperty("SectionCatalog").GetString());
                Assert.Equal(workbench.Section!.Id, payload.GetProperty("SectionId").GetString());
                Assert.Equal(JsonValueKind.Null, payload.GetProperty("BeamProfileId").ValueKind);
                Assert.Equal(10.0, payload.GetProperty("Peralte").GetDouble(), 6); // the depth of the section
                Assert.Equal(144.0, payload.GetProperty("Length").GetDouble());
                Assert.Equal("MENSULA_TROQUEL_REDONDO_CAL_10", payload.GetProperty("MensulaOverride").GetString());
            }

            // A field of a future build in the file; the page forgets the larguero; «Abrir» of the library brings it back here.
            var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            node["Larguero"]!.AsObject()["futureField"] = 7;
            File.WriteAllText(path, node.ToJsonString());
            workbench.ResetToDefaults();
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            rig.Window.Library.Select(rig.Window.Library.Rows.Single(r => r.Name == "Mi larguero"));
            Assert.True(rig.Window.Library.SelectionOpensHere);
            rig.Window.Library.OpenSelected();

            Assert.True(rig.Window.IsLargueroPage);
            Assert.Same(workbench, rig.Window.LargueroWorkbench);
            Assert.Equal("C10X15.3", workbench.Section!.Name);
            Assert.Equal("aisc", workbench.SelectedCatalog);
            Assert.Equal("144", workbench.LengthText);
            Assert.Equal("MENSULA_TROQUEL_REDONDO_CAL_10", workbench.SelectedMensula);
            Assert.Equal("Mi larguero", workbench.NameText);
            Assert.StartsWith("Abierto de la biblioteca: «Mi larguero».", workbench.LoadedNotice, StringComparison.Ordinal);
            Assert.True(workbench.IsComplete);
            Assert.Empty(rig.Door.Inserts);

            // Saved again over the same file: the field of the future build survives (I-11).
            workbench.SaveToLibrary();
            rig.Window.Library.Save();
            Assert.True(rig.Window.Library.HasSaveConflict);
            rig.Window.Library.Replace();
            var again = JsonNode.Parse(File.ReadAllText(path))!["Larguero"]!.AsObject();
            Assert.Equal(7, again["futureField"]!.GetValue<int>());

            // «Volver a los valores por defecto» forgets the file: a new larguero does not carry its fields.
            workbench.ResetToDefaults();
            Assert.Null(workbench.LibrarySource);
            Assert.Equal(string.Empty, workbench.LoadedNotice);
        }

        [Fact]
        public void ALargueroOfRackCadSavedTodayOpensWithTheMensulaOfItsRowAndASectionThatIsNotThereIsSaid()
        {
            using var rig = new Rig();
            var store = new RackProjectStore();
            store.Save(RackProject.ForLarguero(new LargueroDesign
            {
                Name = "Larguero de hoy", BeamProfileId = "LARGUERO_ESCALON_INFINITO", Peralte = 4.5, Length = 108,
            }), rig.File("hoy"));
            store.Save(RackProject.ForLarguero(new LargueroDesign { Name = "Viejo", BeamProfileId = "BEAM_X", Peralte = 3, Length = 96 }), rig.File("viejo"));
            store.Save(RackProject.ForLarguero(new LargueroDesign { Name = "Otro catálogo", SectionCatalog = "OTRO", SectionId = "X1", Length = 96 }), rig.File("otro"));

            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            rig.Window.Library.Select(rig.Window.Library.Rows.Single(r => r.Name == "Larguero de hoy"));
            rig.Window.Library.OpenSelected();
            var workbench = rig.Window.LargueroWorkbench!;
            Assert.Equal("LARGUERO_ESCALON_INFINITO", workbench.Section!.Id);
            Assert.Equal("4.5", workbench.SelectedPeralte);
            Assert.Equal("108", workbench.LengthText);
            Assert.Equal("MENSULA_AJUSTE_INFINITO_CAL_10", workbench.SelectedMensula); // blank in the file: the one of its row
            Assert.True(workbench.IsComplete);
            // Saved again, the ménsula stays «según perfil» (blank), as the classic «(auto — según perfil)».
            Assert.Null(workbench.BuildDesign("x")!.MensulaOverride);
            workbench.SelectedMensula = "MENSULA_3_REMACHES_CAL_10";
            Assert.Equal("MENSULA_3_REMACHES_CAL_10", workbench.BuildDesign("x")!.MensulaOverride);
            Assert.Equal("LARGUERO_ESCALON_INFINITO", workbench.BuildDesign("x")!.BeamProfileId);
            Assert.Null(workbench.BuildDesign("x")!.SectionCatalog);

            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            rig.Window.Library.Select(rig.Window.Library.Rows.Single(r => r.Name == "Viejo"));
            rig.Window.Library.OpenSelected();
            Assert.Null(workbench.Section);
            Assert.Contains("El larguero «BEAM_X» no está en el catálogo de RackCad (secciones.csv): elige otra sección.", workbench.LoadedNotice, StringComparison.Ordinal);
            Assert.Equal("96", workbench.LengthText);
            Assert.Null(workbench.BuildDesign("x")); // nothing to save until a section is chosen
            Assert.NotNull(workbench.SaveRefusal());

            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            rig.Window.Library.Select(rig.Window.Library.Rows.Single(r => r.Name == "Otro catálogo"));
            rig.Window.Library.OpenSelected();
            Assert.Contains("El larguero usa una sección del catálogo «OTRO», que esta versión no conoce: elige otra.", workbench.LoadedNotice, StringComparison.Ordinal);
        }

        // ------------------------------------------------------------------ point 4: «Más» › «Exportar el BOM»

        [Fact]
        public void TheBomIsExportedToExcelAndCsvFromMasAsTheBomOfTheWindow()
        {
            using var rig = new Rig();
            var workbench = rig.Open();
            Assert.True(workbench.CanExportBom);
            workbench.ToggleMore();
            Assert.True(workbench.IsMoreOpen);

            workbench.ExportBom(excel: true);
            Assert.False(workbench.IsMoreOpen);
            var excel = "C:\\Docs\\Larguero - Larguero 96 in · P3 - BOM.xlsx";
            Assert.True(rig.Files.Written.ContainsKey(excel), string.Join(", ", rig.Files.Written.Keys));
            Assert.Equal((byte)'P', rig.Files.Written[excel][0]); // a zip: the workbook of BomXlsxExporter
            Assert.Equal("BOM exportado: «Larguero - Larguero 96 in · P3 - BOM.xlsx»", workbench.ExportedTitle);
            Assert.StartsWith("En Documentos. Una hoja «Lista de materiales»", workbench.ExportedDetail, StringComparison.Ordinal);
            workbench.OpenExported();
            workbench.ShowExportedInFolder();
            Assert.Equal(new[] { excel }, rig.Files.Opened.ToArray());
            Assert.Equal(new[] { excel }, rig.Files.Shown.ToArray());

            // The CSV: the larguero, its profile and its ménsulas, with the names of the page. A second export never replaces the first.
            workbench.ExportBom(excel: false);
            var csv = Encoding.UTF8.GetString(rig.Files.Written["C:\\Docs\\Larguero - Larguero 96 in · P3 - BOM.csv"]);
            Assert.Contains("Escalón calibre 14 · P3", csv, StringComparison.Ordinal);
            Assert.Contains("Ménsula 3 remaches de cinta calibre 10", csv, StringComparison.Ordinal);
            Assert.Equal(LargueroBomBuilder.Larguero, workbench.Summary!.Bill.Components.Single().Category);
            Assert.Equal(new[] { 1, 2 }, workbench.Summary.Bill.Components.Single().Pieces.Select(p => p.Quantity).ToArray());
            workbench.ExportBom(excel: false);
            Assert.True(rig.Files.Written.ContainsKey("C:\\Docs\\Larguero - Larguero 96 in · P3 - BOM (2).csv"));
            Assert.Contains("Ya había un archivo con ese nombre", workbench.ExportedDetail, StringComparison.Ordinal);

            // «Guardar como…» writes the same file elsewhere.
            rig.Files.SaveAsAnswer = "D:\\Otra\\mio.csv";
            workbench.SaveExportedAs();
            Assert.True(rig.Files.Written.ContainsKey("D:\\Otra\\mio.csv"));
            Assert.Equal("BOM exportado: «mio.csv»", workbench.ExportedTitle);
            workbench.CloseExported();
            Assert.False(workbench.HasExported);

            // An incomplete larguero is not exported.
            workbench.LengthText = "";
            Assert.False(workbench.CanExportBom);
            var before = rig.Files.Written.Count;
            workbench.ExportBom(excel: true);
            Assert.Equal(before, rig.Files.Written.Count);
        }

        // ------------------------------------------------------------------ point 5: the name

        [Fact]
        public void TheNameTakesTheAutomaticOneAsASuggestion()
        {
            using var rig = new Rig();
            var workbench = rig.Open();
            Assert.Equal(string.Empty, workbench.NameText);
            Assert.Equal("Larguero 96 in · P3", workbench.EffectiveName);
            Assert.Equal("Larguero 96 in · P3", workbench.DisplayName);
            Assert.Equal("Vacío: se llama «Larguero 96 in · P3», el nombre automático.", workbench.NameHint);
            Assert.True(workbench.CanUseProposedName);

            workbench.NameText = "  Frente A  ";
            Assert.Equal("Frente A", workbench.EffectiveName);
            Assert.Equal("Frente A", workbench.DisplayName);
            Assert.Equal("Frente A", workbench.Plan!.Name);
            Assert.Equal("El automático sería «Larguero 96 in · P3».", workbench.NameHint);

            workbench.UseProposedName();
            Assert.Equal("Larguero 96 in · P3", workbench.NameText);
            Assert.False(workbench.CanUseProposedName);
            Assert.Equal(string.Empty, workbench.NameHint);

            workbench.NameText = "";
            workbench.LengthText = "x";
            Assert.Equal("Larguero sin terminar", workbench.DisplayName);
            Assert.Equal("Vacío: tomará el nombre automático cuando el larguero esté completo.", workbench.NameHint);
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
            view.Measure(new Size(width, 1400));
            view.Arrange(new Rect(0, 0, width, 1400));
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

        private static bool Shown(UIElement element)
        {
            for (DependencyObject? node = element; node != null; node = System.Windows.Media.VisualTreeHelper.GetParent(node))
                if (node is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            return true;
        }

        private static Button ButtonNamed(DependencyObject root, string name)
            => Descendants<Button>(root).Single(b => Shown(b) && Name(b) == name);

        private static List<string> ShownTexts(DependencyObject root) => Descendants<TextBlock>(root).Where(Shown).Select(t => t.Text).ToList();

        [Fact]
        public void ThePageShowsItsViewTheNameMasAndInsertarWithRealWpf()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                using var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                var workbench = rig.Open();
                Lay(view);
                var page = view.LargueroPage;
                Assert.Same(workbench, page.DataContext);

                // The view: the sketch of the plan, drawn to one scale, with its caption.
                Assert.Same(workbench.Sketch, page.LargueroSketch.Sketch);
                Assert.True(page.LargueroSketch.Scale > 0.0);
                Assert.StartsWith("Alzado: los dos extremos a escala, el centro cortado. Perfil de 3 in de peralte y 96 in a corte; ménsulas de 6 in de alto",
                    page.LargueroSketchCaption.Text, StringComparison.Ordinal);
                Assert.Equal("Vista del larguero", Name(page.LargueroSketch));
                Assert.Equal(AutomationControlType.Image, UIElementAutomationPeer.CreatePeerForElement(page.LargueroSketch).GetAutomationControlType());

                // The name, with the automatic one as a suggestion.
                Assert.Contains("Vacío: se llama «Larguero 96 in · P3», el nombre automático.", ShownTexts(page));
                page.LargueroNameBox.Text = "Frente B";
                Lay(view);
                Assert.Equal("Frente B", workbench.NameText);
                Assert.Equal("Frente B", page.LargueroProposedName.Text);

                // «Más» opens inline with the two exports.
                Assert.False(Shown(page.LargueroMorePanel));
                Click(ButtonNamed(page, "Más acciones del larguero"));
                Lay(view);
                Assert.True(Shown(page.LargueroExportExcelButton));
                Assert.True(page.LargueroExportExcelButton.IsEnabled);
                Click(ButtonNamed(page, "Exportar a CSV el BOM del larguero"));
                Lay(view);
                Assert.True(Shown(page.LargueroExportedBanner));
                Assert.Single(rig.Files.Written);

                // The piece: the profile alone is offered with an AISC section only.
                Assert.False(page.LargueroPieceProfile.IsEnabled);
                Assert.True(page.LargueroPieceLarguero.IsChecked);
                Assert.False(Shown(page.LargueroViewBox));

                // «Insertar en el dibujo…»: the primary button, by the door, with the plan.
                Assert.True(page.LargueroInsertButton.IsEnabled);
                Assert.Equal("Insertar el larguero en el dibujo", Name(page.LargueroInsertButton));
                Click(page.LargueroInsertButton);
                Lay(view);
                Assert.Single(rig.Door.Inserts);
                Assert.False(page.LargueroInsertButton.IsEnabled);
                Assert.Equal("Esperando a que AutoCAD tome la petición n.º 1 (RACKLARGUEROINSERTAR).", page.LargueroInsertStatus.Text);
                rig.Door.Answers(LargueroDoorRules.Cancelled(1));
                Lay(view);

                // An AISC section: the profile alone, its view and its detail.
                ChooseAisc(workbench, "S10X25.4");
                Lay(view);
                Assert.True(page.LargueroPieceProfile.IsEnabled);
                Assert.True(Shown(page.LargueroDetailBox));
                page.LargueroPieceProfile.IsChecked = true;
                Lay(view);
                Assert.Equal(LargueroPieceKind.Profile, workbench.PieceKind);
                Assert.True(Shown(page.LargueroViewBox));
                page.LargueroViewBox.SelectedValue = "Isometric";
                Lay(view);
                Assert.Equal("Isometric", workbench.SelectedView);
                Assert.True(Shown(page.LargueroAuthorityPanel)); // the S family: the warning of ADR-0023, always
                Assert.Equal("Insertar el perfil en el dibujo", Name(page.LargueroInsertButton));

                // Unique names; the mark «En construcción» is gone.
                var names = Descendants<Button>(page).Where(Shown).Select(Name).ToList();
                Assert.Equal(names.Count, names.Distinct().Count());
                Assert.DoesNotContain(ShownTexts(page), t => t.Contains("construcción", StringComparison.Ordinal));
            }));
        }

        [Fact]
        public void TheClassicEditorSaysThatALargueroOfAiscIsEditedInTheWindow()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackCad.UI.Systems.Larguero.RackLargueroWindow();
                try
                {
                    window.LoadExisting(new LargueroDesign { Name = "C", SectionCatalog = "AISC", SectionId = "AISC-C-C10X15_3", Peralte = 10, Length = 96 });
                    Assert.Equal("Este larguero usa la sección «AISC-C-C10X15_3» del catálogo AISC: este editor solo edita largueros de RackCad. Ábrelo en «Nuevo › "
                                 + "Larguero» de la ventana RackCad.", window.StatusText.Text);
                    // A larguero of RackCad says nothing of it.
                    var other = new RackCad.UI.Systems.Larguero.RackLargueroWindow();
                    try
                    {
                        other.LoadExisting(new LargueroDesign { BeamProfileId = "LARGUERO_ESCALON_CAL14_3_REMACHES", Peralte = 3, Length = 96 });
                        Assert.DoesNotContain("AISC", other.StatusText.Text ?? string.Empty, StringComparison.Ordinal);
                    }
                    finally
                    {
                        other.Close();
                    }
                }
                finally
                {
                    window.Close();
                }
            });
        }
    }
}
