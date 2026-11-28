#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.StructuralSections;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T7 per (I-64, séptima tanda; the Owner, 2026-11-27: «Igual que la clásica» and «Abren la ventana»): the page «Perfil estructural» of the
    /// RackCad window. It is the inspector «Secciones estructurales» of RACKSECCION without blocking AutoCAD: the same state, the same controls
    /// (extracted from the classic window, not copied), the same preview and the same lines of fidelity. «Insertar en el dibujo…» is a
    /// numbered request of the door of the window (RACKSECCIONINSERTAR) with the plan the state built and its section; the answer comes back
    /// to the page with what was drawn, the units advisory and the report of the flow. The card of «Nuevo», RACKSECCION and the help open
    /// it; the classic window stays in RACKMENU.
    /// </summary>
    public class RackCadStructuralSectionPageTests
    {
        private static readonly SessionId Session = new(7);

        private static readonly StructuralSectionCatalog Catalog = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        /// <summary>A door that inserts a section, reads a Dinámico (another request of the window) and asks the window as RACKSECCION does.</summary>
        private sealed class SectionDoor : IRackCadStructuralSectionDoorPort, IRackCadShortcutPort
        {
            public List<StructuralSectionInsertRequest> Inserts { get; } = new();

            public List<RackDoorRequest> DesignReads { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            /// <summary>What AutoCAD does during the call (M-22): it may take the request and answer before the call returns.</summary>
            public Action<StructuralSectionInsertRequest>? DuringTheCall { get; set; }

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten { add { } remove { } }

            public event Action<long>? RequestStarted;

            public event Action<StructuralSectionInserted>? StructuralSectionInserted;

            public event Action<RackCadShortcutAsk>? ShortcutAsked;

            public event Action<RackCadShortcutAsk>? ShortcutDone;

            public RackCadClassicResult RequestDesign(RackDoorRequest request)
            {
                DesignReads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestStructuralSectionInsert(StructuralSectionInsertRequest request)
            {
                Inserts.Add(request);
                DuringTheCall?.Invoke(request);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            public void Answered(StructuralSectionInserted answer) => StructuralSectionInserted?.Invoke(answer);

            /// <summary>RACKSECCION as RackCadWindowShortcuts.Open runs it: ask the window, then say it is done (there is no read to run).</summary>
            public RackCadShortcutAsk RackSeccion()
            {
                var ask = new RackCadShortcutAsk(RackCadShortcutTarget.StructuralSection, null!, null!);
                ShortcutAsked?.Invoke(ask);
                if (ask.Taken) ShortcutDone?.Invoke(ask);
                return ask;
            }
        }

        private sealed class ClassicDouble : IRackCadClassicCommandPort
        {
            public List<RackCadClassicAction> Runs { get; } = new();

            public RackCadClassicResult Run(RackCadClassicAction action)
            {
                Runs.Add(action);
                return RackCadClassicResult.Started();
            }
        }

        private sealed class SettingsDouble : IRackCadSettingsAccess
        {
            public string BlockLibraryPath => string.Empty;

            public bool BlockLibraryOverridden => false;

            public bool BlockLibraryExists => false;

            public string DesignLibraryPath => string.Empty;

            public void SetBlockLibraryPath(string? path)
            {
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
                        new BrowserRackListEntry("DIN-1", "Dinámico sur", "dynamic", new[] { "lateral", "planta" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, Classic, new SettingsDouble(), withDoor ? Door : null, UiSupport.LoadCatalogSafe);
                Window.StructuralSectionCatalogLoader = () => Loader();
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public SectionDoor Door { get; } = new();

            public ClassicDouble Classic { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public StructuralSectionPageViewModel Page => Window.StructuralSection;

            public StructuralSectionInspectorState State => Page.State!;

            /// <summary>What the page reads as its catalogue; the real one unless a test makes it fail.</summary>
            public Func<StructuralSectionCatalog> Loader { get; set; } = () => Catalog;

            public RackCadNewSystemCard Card(RackCadClassicAction action) => Window.NewSystems.Single(c => c.Action == action);

            /// <summary>The card «Perfil estructural» of «Nuevo», with W12X26 chosen at 144 in.</summary>
            public StructuralSectionDefinition OpenWithW12X26()
            {
                Window.ShowPage(RackCadWindowPage.Nuevo);
                Window.OpenNewSystem(Card(RackCadClassicAction.StructuralSection));
                State.Search = "W12X26";
                State.EnsureSelectionIsVisible();
                State.Selected = State.Matches().Single(s => s.DisplayName == "W12X26");
                State.Length = 144;
                Page.SelectionMayHaveChanged();
                return State.Selected;
            }

            public void Present(long session)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: session);
                Window.Refresh();
            }
        }

        // ------------------------------------------------------------------ opening the page

        [Fact]
        public void TheCardOfNuevoOpensThePageWithTheInspectorOfRackSeccionAndRunsNothingClassic()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);

            rig.Window.OpenNewSystem(rig.Card(RackCadClassicAction.StructuralSection));

            Assert.True(rig.Window.IsStructuralSectionPage);
            Assert.Equal(RackCadWindowPage.PerfilEstructural, rig.Window.Page);
            Assert.False(rig.Window.IsNuevoPage);
            // The crumb «Nuevo › Perfil estructural»: the side column marks «Nuevo».
            Assert.Equal(new[] { RackCadWindowPage.Nuevo }, rig.Window.Sections.Where(s => s.IsActive).Select(s => s.Page).ToArray());
            Assert.Empty(rig.Classic.Runs);
            Assert.Empty(rig.Door.Inserts);

            // The state of the classic inspector, over the catalogue of RACKSECCION, with its defaults.
            var classic = new StructuralSectionInspectorState(Catalog);
            Assert.True(rig.Page.HasState);
            Assert.Same(Catalog, rig.State.Catalog);
            Assert.Equal(classic.Matches().Count, rig.State.Matches().Count);
            Assert.Same(classic.Selected, rig.State.Selected);
            Assert.Equal((120.0, SectionViewKind.CrossSection, SectionDetailLevel.Tabulated, SectionRepresentationMode.Wireframe),
                (rig.State.Length, rig.State.View, rig.State.Detail, rig.State.Mode));
            Assert.True(rig.Page.CanInsert, rig.Page.InsertReason);
            Assert.Equal(string.Empty, rig.Page.InsertReason);
            Assert.False(rig.Page.HasCatalogNotice);

            // Every other card still runs its classic action.
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Window.OpenNewSystem(rig.Card(RackCadClassicAction.NewCantilever));
            Assert.Equal(new[] { RackCadClassicAction.NewCantilever }, rig.Classic.Runs);
        }

        [Fact]
        public void ShowingThePageAgainKeepsWhatWasChosen()
        {
            var rig = new Rig();
            var section = rig.OpenWithW12X26();
            var state = rig.State;

            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Window.OpenStructuralSectionPage();
            rig.Window.OpenStructuralSectionPage(); // already shown: it reads the catalogue again and keeps the state

            Assert.Same(state, rig.State);
            Assert.Same(section, rig.State.Selected);
            Assert.Equal(144.0, rig.State.Length);
        }

        [Fact]
        public void ACatalogueThatDoesNotValidateIsSaidAndNothingIsOfferedAsRackSeccionFailsClosed()
        {
            var rig = new Rig();
            rig.OpenWithW12X26();
            Assert.True(rig.Page.HasState);
            rig.Loader = () => throw new StructuralSectionCatalogException("catalogs", "structural-sections-w.csv, fila 3: el peralte no es un número.");

            rig.Window.OpenStructuralSectionPage(); // shown again: the catalogue no longer validates, so the inspector goes

            Assert.True(rig.Window.IsStructuralSectionPage);
            Assert.False(rig.Page.HasState);
            Assert.Null(rig.Page.State);
            Assert.True(rig.Page.HasCatalogNotice);
            Assert.StartsWith("El catálogo de secciones estructurales no es válido, así que no se dibujará nada. ", rig.Page.CatalogNotice);
            Assert.Contains("fila 3", rig.Page.CatalogNotice);
            Assert.False(rig.Page.CanInsert);
            Assert.Equal(StructuralSectionPageViewModel.NoCatalogText, rig.Page.InsertReason);
            rig.Page.Insert();
            Assert.Empty(rig.Door.Inserts);

            rig.Loader = () => throw new IOException("el archivo está en uso");
            rig.Window.OpenStructuralSectionPage();
            Assert.Equal("No se pudo leer el catálogo de secciones estructurales, así que no se dibujará nada. el archivo está en uso", rig.Page.CatalogNotice);

            // Once the catalogue reads, the page has the inspector again.
            rig.Loader = () => Catalog;
            rig.Window.OpenStructuralSectionPage();
            Assert.True(rig.Page.HasState);
            Assert.False(rig.Page.HasCatalogNotice);
            Assert.True(rig.Page.CanInsert);

            // T7 per, review I-1: anything else is not «the catalogue is wrong» but a defect. The page still fails closed (no inspector over
            // data it could not read) and the message of the window says the contained failure, as for any other defect of the window.
            rig.Loader = () => throw new UnauthorizedAccessException("sin permiso sobre catalogs");
            rig.Window.OpenStructuralSectionPage();
            Assert.False(rig.Page.HasState);
            Assert.False(rig.Page.HasCatalogNotice);
            Assert.False(rig.Page.CanInsert);
            Assert.True(rig.Window.ActionFailed);
            Assert.Equal("La ventana tuvo un fallo contenido: sin permiso sobre catalogs", rig.Window.ActionMessage);
        }

        // ------------------------------------------------------------------ «Insertar en el dibujo…»

        [Fact]
        public void InsertarHandsTheDoorThePlanTheStateBuiltWithItsSectionAndTheWindowWaits()
        {
            var rig = new Rig();
            var section = rig.OpenWithW12X26();
            rig.State.View = SectionViewKind.LongitudinalX;
            rig.State.RotationDegrees = 30;
            var expected = rig.State.BuildPlan();

            rig.Page.Insert();

            var request = Assert.Single(rig.Door.Inserts);
            Assert.Equal(Session, request.Session);
            Assert.Same(section, request.Section);
            Assert.Equal(section.SectionId, request.Plan.SectionId);
            Assert.Equal((144.0, SectionViewKind.LongitudinalX), (request.Plan.Length, request.Plan.View));
            // T7 per, review M-3: the WHOLE plan (every curve and point, the fidelity and the authority), not a few of its fields.
            Assert.Equal(expected.Signature(), request.Plan.Signature());
            Assert.Null(StructuralSectionDoorRules.RefusalOf(request));

            // One request for the whole window, with «Dejar de esperar» until AutoCAD takes it.
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Window.CanStopWaiting);
            Assert.Equal(StructuralSectionDoorRules.WaitingText, rig.Window.WaitingText);
            Assert.True(rig.Page.IsWaiting);
            Assert.False(rig.Page.CanInsert);
            Assert.Equal(StructuralSectionDoorRules.WaitingText, rig.Page.InsertReason);

            // What the user does now does not reach the request: the plan is immutable and already handed over.
            rig.State.Length = 60;
            Assert.Equal(144.0, request.Plan.Length);

            rig.Door.Start(request.RequestId);
            Assert.Equal(StructuralSectionDoorRules.TakenText, rig.Window.WaitingText);
            Assert.Equal(StructuralSectionDoorRules.TakenText, rig.Page.InsertReason);
            Assert.False(rig.Window.CanStopWaiting);
            rig.Window.StopWaiting(); // AutoCAD asks the point: the window keeps waiting and says it
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.Equal(StructuralSectionDoorRules.TakenText, rig.Window.ActionMessage);
            Assert.Empty(rig.Door.Abandoned);

            rig.Door.Answered(StructuralSectionDoorRules.Inserted(request.RequestId, section, "RACKCAD_SECCION_W12X26",
                "RackCad: W12X26 insertada como bloque 'RACKCAD_SECCION_W12X26'. Longitud 144 in, vista LongitudinalX, 4 curvas.", unitsWarned: true));

            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.False(rig.Page.IsWaiting);
            Assert.StartsWith("AutoCAD insertó «W12X26» como el bloque «RACKCAD_SECCION_W12X26». Es una sección suelta, sin datos de rack", rig.Page.StatusText);
            Assert.EndsWith("Un «U» la deshace.", rig.Page.StatusText);
            Assert.False(rig.Page.StatusFailed);
            Assert.True(rig.Page.HasReport);
            Assert.StartsWith("RackCad: W12X26 insertada como bloque 'RACKCAD_SECCION_W12X26'.", rig.Page.ReportText);
            Assert.True(rig.Page.HasUnitsNotice);
            Assert.Equal(StructuralSectionDoorRules.UnitsText, rig.Page.UnitsText);
            Assert.True(rig.Page.CanInsert);
            // The drawn section is no rack: the list is not read again and nothing reaches it.
            Assert.DoesNotContain(rig.Window.RackList.Groups.SelectMany(g => g.Items), i => i.Name.Contains("W12X26"));
        }

        [Fact]
        public void OnlyTheAnswerOfItsRequestCountsAndACancelledPointOrAFailureSayWhy()
        {
            var rig = new Rig();
            rig.OpenWithW12X26();
            rig.Page.Insert();
            var first = rig.Door.Inserts.Last().RequestId;

            rig.Door.Answered(StructuralSectionDoorRules.Cancelled(first + 5, null!, false));
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.False(rig.Page.HasStatus);

            // T7 per, review M-1: drawn, then a failure after the commit (the regen): the page says the section IS in the drawing.
            rig.Door.Answered(StructuralSectionDoorRules.DrawnButFailed(first, rig.State.Selected!, "RACKCAD_SECCION_W12X26", "eRegen", "RackCad: no se pudo dibujar la seccion. eRegen", false));
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.True(rig.Page.StatusFailed);
            Assert.StartsWith("AutoCAD insertó «W12X26» como el bloque «RACKCAD_SECCION_W12X26», pero después falló: eRegen.", rig.Page.StatusText);
            Assert.DoesNotContain("no cambió", rig.Page.StatusText);

            rig.Page.Insert();
            first = rig.Door.Inserts.Last().RequestId;
            rig.Door.Answered(StructuralSectionDoorRules.Cancelled(first, "RackCad: insercion cancelada; no se dejo nada en el dibujo.", false));
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal(StructuralSectionDoorRules.CancelledText, rig.Page.StatusText);
            Assert.False(rig.Page.StatusFailed);
            Assert.False(rig.Page.HasUnitsNotice);
            Assert.Equal("RackCad: insercion cancelada; no se dejo nada en el dibujo.", rig.Page.ReportText);

            rig.Page.Insert();
            var second = rig.Door.Inserts.Last().RequestId;
            Assert.True(second > first);
            Assert.False(rig.Page.HasStatus); // the answer of the last insertion goes when the next one waits
            rig.Door.Answered(StructuralSectionDoorRules.Failed(second, "eInvalidInput", "RackCad: no se pudo dibujar la seccion. eInvalidInput", false));
            Assert.Equal("No se pudo dibujar la sección: eInvalidInput El dibujo no cambió.", rig.Page.StatusText);
            Assert.True(rig.Page.StatusFailed);

            rig.Page.Insert();
            rig.Door.Answered(StructuralSectionDoorRules.Refused(rig.Door.Inserts.Last().RequestId, StructuralSectionDoorRules.OtherDrawingText));
            Assert.Equal(StructuralSectionDoorRules.OtherDrawingText, rig.Page.StatusText);
            Assert.True(rig.Page.StatusFailed);
            Assert.False(rig.Page.HasReport);
        }

        [Fact]
        public void DejarDeEsperarBeforeAutoCadTookItForgetsTheRequestAndALateAnswerIsIgnored()
        {
            var rig = new Rig();
            var section = rig.OpenWithW12X26();
            rig.Page.Insert();
            var id = rig.Door.Inserts.Single().RequestId;

            rig.Window.StopWaiting();

            Assert.Equal(new[] { id }, rig.Door.Abandoned);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal(StructuralSectionDoorRules.StoppedText, rig.Page.StatusText);
            Assert.True(rig.Page.CanInsert);

            rig.Door.Answered(StructuralSectionDoorRules.Inserted(id, section, "RACKCAD_SECCION_W12X26", "tarde", false));
            Assert.Equal(StructuralSectionDoorRules.StoppedText, rig.Page.StatusText);

            // From another page, the message of the window says it too.
            rig.Page.Insert();
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Window.StopWaiting();
            Assert.Equal(StructuralSectionDoorRules.StoppedText, rig.Window.ActionMessage);
        }

        [Fact]
        public void AClosedDrawingDropsTheInsertionThatWaited()
        {
            var rig = new Rig();
            rig.OpenWithW12X26();
            rig.Page.Insert();
            var id = rig.Door.Inserts.Single().RequestId;

            rig.Summary.Dead.Add(Session);
            rig.Present(session: 8);

            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Contains(id, rig.Door.Abandoned);
            Assert.Equal(StructuralSectionDoorRules.DrawingClosedText, rig.Page.StatusText);
        }

        [Fact]
        public void AnAnswerDuringTheCallReachesThePage()
        {
            var rig = new Rig();
            var section = rig.OpenWithW12X26();
            rig.Door.DuringTheCall = request =>
            {
                rig.Door.Start(request.RequestId);
                rig.Door.Answered(StructuralSectionDoorRules.Inserted(request.RequestId, section, "RACKCAD_SECCION_W12X26", "RackCad: W12X26 insertada", false));
            };

            rig.Page.Insert();

            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.StartsWith("AutoCAD insertó «W12X26»", rig.Page.StatusText);
            Assert.True(rig.Page.CanInsert);
        }

        [Fact]
        public void ADoorThatRefusesSaysWhyAndNothingWaits()
        {
            var rig = new Rig();
            rig.OpenWithW12X26();
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");

            rig.Page.Insert();

            Assert.Single(rig.Door.Inserts);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.False(rig.Page.IsWaiting);
            Assert.Equal("Hay un comando en curso en AutoCAD: termínalo o cancélalo (Esc) y vuelve a intentarlo.", rig.Page.StatusText);
            Assert.True(rig.Page.StatusFailed);
            Assert.True(rig.Page.CanInsert);
        }

        [Fact]
        public void InsertarSaysWhyItIsOffWithNoSelectionNoDrawingAnotherRequestOrNoDoor()
        {
            var rig = new Rig();
            rig.OpenWithW12X26();

            rig.State.Search = "ZZZZ-NO-EXISTE";
            rig.State.EnsureSelectionIsVisible();
            rig.Page.SelectionMayHaveChanged();
            Assert.False(rig.Page.CanInsert);
            Assert.Equal(StructuralSectionPageViewModel.NoSelectionText, rig.Page.InsertReason);
            Assert.Equal("Elige una sección de la lista para poder insertarla.", rig.Page.InsertReason); // the words of the classic window
            rig.Page.Insert();
            Assert.Empty(rig.Door.Inserts);

            rig.State.Search = string.Empty;
            rig.State.EnsureSelectionIsVisible();
            rig.Page.SelectionMayHaveChanged();
            Assert.True(rig.Page.CanInsert);

            // Another request of the window waits: «Editar aquí» of a Dinámico.
            rig.Window.OpenRack("DIN-1");
            rig.Window.EditDynamicHere();
            Assert.Single(rig.Door.DesignReads);
            Assert.False(rig.Page.CanInsert);
            Assert.Equal(StructuralSectionPageViewModel.OtherRequestText, rig.Page.InsertReason);
            rig.Window.StopWaiting();
            Assert.True(rig.Page.CanInsert);

            // No drawing: the inspector stays (it needs none), the insertion does not.
            rig.Host.Snapshot = WorkspacePanelFixtures.NoDocument();
            rig.Window.Refresh();
            Assert.True(rig.Page.HasState);
            Assert.False(rig.Page.CanInsert);
            Assert.Equal(StructuralSectionPageViewModel.NoDrawingText, rig.Page.InsertReason);

            var noDoor = new Rig(withDoor: false);
            noDoor.Window.OpenStructuralSectionPage();
            Assert.True(noDoor.Page.HasState);
            Assert.False(noDoor.Page.CanInsert);
            Assert.Equal(StructuralSectionPageViewModel.NoDoorText, noDoor.Page.InsertReason);
        }

        // ------------------------------------------------------------------ RACKSECCION and the help

        [Fact]
        public void RackSeccionOpensThePageInsideTheCommandAndSendsNothing()
        {
            var rig = new Rig();

            var ask = rig.Door.RackSeccion();

            Assert.True(ask.Taken);
            Assert.Null(ask.Said);
            Assert.True(rig.Window.IsStructuralSectionPage);
            Assert.True(rig.Page.HasState);
            Assert.Equal("RackCad: «Nuevo» › «Perfil estructural» en la ventana RackCad.", RackCadShortcutRules.CommandLineText(ask));
            Assert.Empty(rig.Door.Inserts);
            Assert.Empty(rig.Door.DesignReads);
            Assert.Empty(rig.Classic.Runs);

            // Already shown, the page stays with what was chosen; a catalogue that fails is said in the command line too.
            var state = rig.State;
            rig.Door.RackSeccion();
            Assert.Same(state, rig.State);
            rig.Loader = () => throw new StructuralSectionCatalogException("catalogs", "falta el manifiesto.");
            var failed = rig.Door.RackSeccion();
            Assert.True(failed.Taken);
            Assert.Equal(rig.Page.CatalogNotice, failed.Said);
            Assert.StartsWith("RackCad: «Nuevo» › «Perfil estructural» en la ventana RackCad: El catálogo de secciones estructurales no es válido",
                RackCadShortcutRules.CommandLineText(failed));
        }

        [Fact]
        public void TheHelpTakesRackSeccionToThePageAndSaysWhereTheClassicIs()
        {
            var rig = new Rig();
            var row = RackCadHelpViewModel.AllRows.Single(r => r.Command == "RACKSECCION");

            Assert.Equal(("Nuevo › Perfil estructural", RackCadWindowPage.PerfilEstructural), (row.WindowText, row.WindowPage!.Value));
            Assert.Contains("abre «Nuevo» › «Perfil estructural» en la ventana RackCad", row.Summary);
            Assert.EndsWith("la ventana clásica sigue en el menú clásico (RACKMENU, «Generar perfil estructural»).", row.Summary);

            rig.Window.Help.GoTo(row);

            Assert.True(rig.Window.IsStructuralSectionPage);
            Assert.True(rig.Page.HasState);
        }

        // ------------------------------------------------------------------ the view (WPF in STA)

        private static IEnumerable<T> Descendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        private static void Lay(FrameworkElement view, double width = 1000)
        {
            view.Measure(new Size(width, 1400));
            view.Arrange(new Rect(0, 0, width, 1400));
            view.UpdateLayout();
        }

        private static void Click(ButtonBase button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        // The capture panel of the classic window, as its characterization finds it.
        private static StackPanel ClassicParameters(Window window) =>
            EditorWindowTestSupport.FindAll<StackPanel>(window)
                .First(panel => panel.Children.OfType<TextBlock>().Any(label => label.Text == "Buscar designacion"));

        private static string[] Shape(Panel panel) =>
            panel.Children.Cast<UIElement>().Select(c => c is TextBlock t ? "label:" + t.Text : c is CheckBox b ? "check:" + b.Content : c.GetType().Name).ToArray();

        private static string[] Lines(Panel information) => information.Children.OfType<TextBlock>().Select(t => t.Text).ToArray();

        [Fact]
        public void ThePageShowsTheControlsOfTheClassicWindowOverItsStateWithTheSamePreviewAndFidelity()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view);

                // The card «Perfil estructural» of «Nuevo», clicked: the page, not the classic window.
                var card = Descendants<Button>(view.NewSystemList).Single(b => b.DataContext is RackCadNewSystemCard c && c.Action == RackCadClassicAction.StructuralSection);
                Click(card);
                Lay(view);
                Assert.Empty(rig.Classic.Runs);
                Assert.True(rig.Window.IsStructuralSectionPage);
                Assert.Equal(Visibility.Visible, view.StructuralSectionPage.Visibility);
                var page = Assert.IsType<StructuralSectionPageView>(view.StructuralSectionPage.Content);
                var controls = page.Controls!;
                Assert.NotNull(controls);
                Assert.Same(rig.State, controls.State);
                Assert.True(controls.IsActive);

                // The classic window, loaded, over a fresh state of the same catalogue.
                var classic = new StructuralSectionInspectorWindow(Catalog);
                classic.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, classic));

                // The same controls in the same order, with the same labels, items and defaults.
                Assert.Equal(Shape(ClassicParameters(classic)), Shape(controls.Parameters));
                Assert.Same(controls.Parameters, page.SectionParametersHost.Content);
                var pageCombos = controls.Parameters.Children.OfType<ComboBox>().ToList();
                var classicCombos = ClassicParameters(classic).Children.OfType<ComboBox>().ToList();
                for (var i = 0; i < classicCombos.Count; i++)
                {
                    Assert.Equal(classicCombos[i].Items.OfType<ComboBoxItem>().Select(o => o.Content).ToArray(),
                        pageCombos[i].Items.OfType<ComboBoxItem>().Select(o => o.Content).ToArray());
                    Assert.Equal(classicCombos[i].SelectedIndex, pageCombos[i].SelectedIndex);
                }

                var list = controls.Parameters.Children.OfType<ListBox>().Single();
                Assert.Equal(rig.State.Matches().Count, list.Items.Count);

                // The same preview (the renderer of the plan) and the same lines of weight, fidelity, authority and diagnostics.
                var preview = Assert.Single(Descendants<StructuralSectionPreview>(page));
                Assert.Same(controls.Preview, preview);
                Assert.NotNull(preview.Plan);
                Assert.Equal(rig.State.BuildPlan().Curves.Count, preview.Plan!.Curves.Count);
                var classicInformation = ClassicInformation(classic);
                Assert.Equal(4, Lines(classicInformation).Length);
                Assert.Equal(Lines(classicInformation), Lines(controls.Information));

                // An S keeps the warning of ADR-0023 in both, word for word, with the same fidelity line.
                classic.State.Selected = classic.State.Catalog.ByFamily(StructuralSectionFamily.S).First();
                classic.Refresh();
                rig.State.Selected = rig.State.Catalog.ByFamily(StructuralSectionFamily.S).First();
                controls.Refresh();

                Assert.Contains(Lines(controls.Information), l => l.StartsWith("GEOMETRIA VISUAL DERIVADA (ADR-0023)", StringComparison.Ordinal));
                Assert.Equal(Lines(classicInformation), Lines(controls.Information));
            });
        }

        // The lines under the preview of the classic window: the panel that holds its summary («…  ·  …»).
        private static StackPanel ClassicInformation(Window classic)
        {
            var summary = EditorWindowTestSupport.FindAll<TextBlock>(classic).First(t => t.Text.Contains("  ·  "));
            return EditorWindowTestSupport.FindAll<StackPanel>(classic).First(p => p.Children.Contains(summary));
        }

        [Fact]
        public void TheButtonFollowsTheSelectionSendsTheRequestAndThePageShowsTheAnswer()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenStructuralSectionPage();
                Lay(view);
                var page = (StructuralSectionPageView)view.StructuralSectionPage.Content;
                var controls = page.Controls!;

                Assert.True(page.SectionInsertButton.IsEnabled);
                Assert.Equal(Visibility.Collapsed, page.SectionInsertReasonText.Visibility);

                controls.SearchBox.Text = "ZZZZ-NO-EXISTE";
                Assert.False(page.SectionInsertButton.IsEnabled);
                Lay(view);
                Assert.Equal(Visibility.Visible, page.SectionInsertReasonText.Visibility);
                Assert.Equal("Elige una sección de la lista para poder insertarla.", page.SectionInsertReasonText.Text);

                controls.SearchBox.Text = "W12X26";
                var rotation = controls.Parameters.Children.OfType<TextBox>().ToList()[2];
                rotation.Text = "30";
                Assert.True(page.SectionInsertButton.IsEnabled);
                Click(page.SectionInsertButton);

                var request = Assert.Single(rig.Door.Inserts);
                Assert.Equal("W12X26", request.Section.DisplayName);
                // T7 per, review M-3: what reaches AutoCAD is, curve for curve and point for point, what the preview drew.
                Assert.NotNull(controls.Preview.Plan);
                Assert.Equal(controls.Preview.Plan!.Signature(), request.Plan.Signature());
                Assert.Equal(30.0, rig.State.RotationDegrees);
                Assert.False(page.SectionInsertButton.IsEnabled);

                rig.Door.Start(request.RequestId);
                rig.Door.Answered(StructuralSectionDoorRules.Inserted(request.RequestId, request.Section, "RACKCAD_SECCION_W12X26", "RackCad: W12X26 insertada", true));
                Lay(view);

                Assert.Equal(Visibility.Visible, page.SectionStatusPanel.Visibility);
                Assert.StartsWith("AutoCAD insertó «W12X26»", page.SectionStatusText.Text);
                Assert.Equal(Visibility.Visible, page.SectionUnitsText.Visibility);
                Assert.Equal("RackCad: W12X26 insertada", page.SectionReportText.Text);
                Assert.True(page.SectionInsertButton.IsEnabled);

                // The crumb goes back to «Nuevo».
                Click(page.SectionCrumbNuevoButton);
                Assert.True(rig.Window.IsNuevoPage);
                Assert.Equal(Visibility.Collapsed, view.StructuralSectionPage.Visibility);
            });
        }

        [Fact]
        public void NarrowThePreviewGoesBelowTheParametersAndTheNamesOfThePageAreUnique()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Nuevo); // the cards of «Nuevo» are built, so their names are in the tree
                Lay(view, 1100);
                rig.Window.OpenStructuralSectionPage();
                Lay(view, 1100);
                var page = (StructuralSectionPageView)view.StructuralSectionPage.Content;
                Assert.False(page.PreviewBelow);
                Assert.Equal(1, Grid.GetColumn(page.SectionPreviewPanel));

                Lay(view, 600); // the side column keeps its 250: the page has about 330
                Assert.True(page.SectionLayout.ActualWidth < StructuralSectionPageView.PreviewBesideWidth, page.SectionLayout.ActualWidth.ToString());
                Assert.True(page.PreviewBelow);
                Assert.Equal((1, 0), (Grid.GetRow(page.SectionPreviewPanel), Grid.GetColumn(page.SectionPreviewPanel)));

                // Every control of the inspector is named in the page by its label, and no name of the page (its root included) repeats one
                // of the window: T7 per, review M-4, the root is «Página Perfil estructural», not the name of the card of «Nuevo».
                var pageElements = new[] { (FrameworkElement)page }.Concat(Descendants<FrameworkElement>(page)).ToList();
                var named = pageElements
                    .Select(AutomationProperties.GetName)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();
                Assert.Equal("Página Perfil estructural", AutomationProperties.GetName(page));
                Assert.Contains("Buscar designacion (perfil estructural)", named);
                Assert.Contains("Mostrar envolvente (perfil estructural)", named);
                Assert.Equal(named.Count, named.Distinct(StringComparer.Ordinal).Count());
                var outside = Descendants<FrameworkElement>(view).Except(pageElements)
                    .Select(AutomationProperties.GetName).Where(n => !string.IsNullOrEmpty(n)).ToHashSet(StringComparer.Ordinal);
                Assert.Contains("Perfil estructural", outside); // the card of «Nuevo» is in the tree the test looks at
                Assert.DoesNotContain(named, outside.Contains);
            });
        }
    }
}
