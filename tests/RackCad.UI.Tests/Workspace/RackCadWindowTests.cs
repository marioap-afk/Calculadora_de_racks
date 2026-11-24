#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14 (phases 1 and 2): the RackCad window. The side column lists every rack of the drawing grouped by system, with the state of
    /// its draft from the memory of the edit port, a search, a filter by system, "solo con cambios" and "Seguir la selección"; a rack of the
    /// list opens in the browser of the panel; the sections open their current windows through the door to the classic commands; and the
    /// page of the rack is the workspace panel, with its strip of tabs hidden.
    /// </summary>
    public class RackCadWindowTests
    {
        private static readonly SessionId Session = new(7);

        private sealed class ClassicDouble : IRackCadClassicCommandPort
        {
            public List<RackCadClassicAction> Runs { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public Exception? Fault { get; set; }

            public RackCadClassicResult Run(RackCadClassicAction action)
            {
                Runs.Add(action);
                if (Fault != null) throw Fault;
                return Answer;
            }
        }

        private sealed class SettingsDouble : IRackCadSettingsAccess
        {
            public string? Saved { get; private set; } = "sin cambios";

            public string BlockLibraryPath { get; set; } = "C:\\bloques.dwg";

            public bool BlockLibraryOverridden { get; set; }

            public bool BlockLibraryExists { get; set; } = true;

            public string DesignLibraryPath { get; set; } = "C:\\disenos";

            public void SetBlockLibraryPath(string? path)
            {
                Saved = path;
                BlockLibraryPath = path ?? "C:\\predeterminada.dwg";
                BlockLibraryOverridden = path != null;
            }
        }

        private sealed class Rig
        {
            public Rig(SelectionContext? selection = null, params BrowserRackListEntry[] racks)
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: selection ?? SelectionContext.None) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(Racks),
                };
                Racks = racks.Length > 0 ? racks.ToList() : DefaultRacks();
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, Pilot);
                Window = new RackCadWindowViewModel(Panel, Classic, Settings);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePilotEditPortDouble Pilot { get; } = new();

            public ClassicDouble Classic { get; } = new();

            public SettingsDouble Settings { get; } = new();

            public List<BrowserRackListEntry> Racks { get; set; } = new();

            public WorkspacePanelViewModel Panel { get; }

            public RackCadWindowViewModel Window { get; }

            public RackListViewModel List => Window.RackList;

            public void Select(string? rack)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: rack == null ? SelectionContext.None : SelectionContext.One(rack));
                Window.Refresh();
            }
        }

        private static List<BrowserRackListEntry> DefaultRacks() => new()
        {
            Entry("PB-10", "PB 10", "pushback"),
            Entry("SEL-1", "Selectivo norte", "selective"),
            Entry("PB-2", "PB 2", "pushback"),
            Entry("DIN-1", "Dinámico sur", "dynamic"),
            Entry("CAN-1", "Cantilever 1", "cantilever"),
            Entry("X-1", null, "drivein"),
        };

        private static BrowserRackListEntry Entry(string id, string? name, string? kind) => new(id, name, kind, new[] { "lateral", "planta" });

        private static string[] Names(RackListViewModel list) => list.Groups.SelectMany(g => g.Items).Select(i => i.Name).ToArray();

        // ------------------------------------------------------------------ the list of the drawing

        [Fact]
        public void TheListIsAskedOnThePresentationAndGroupedBySystemInTheOrderOfTheWindowWithNaturalOrderInside()
        {
            var rig = new Rig();

            Assert.Contains(rig.Summary.ListCalls, c => c.Origin.Origin == Session && !c.UserRequested);
            Assert.Equal(new[] { "Selectivo", "Push Back", "Dinámico", "Cantilever", "drivein" }, rig.List.Groups.Select(g => g.KindLabel).ToArray());
            Assert.Equal(new[] { "Selectivo norte", "PB 2", "PB 10", "Dinámico sur", "Cantilever 1", "(sin nombre)" }, Names(rig.List));
            Assert.Equal("2 de 2", rig.List.Groups[1].CountText);
            Assert.Equal(6, rig.List.RackCount);
            Assert.Equal("Mostrando 6 de 6", rig.List.ShownText);
            Assert.Equal(string.Empty, rig.List.StatusText);
            Assert.Equal(string.Empty, rig.List.NoResultsText);
            Assert.Equal(
                new[] { string.Empty, "selective", "pushback", "dynamic", "cantilever", "drivein" },
                rig.List.KindOptions.Select(o => o.Key).ToArray());
            Assert.Equal("Todos los sistemas", rig.List.KindOptions[0].Label);
        }

        [Fact]
        public void TheSearchIgnoresCaseAndAccentsAndTheCountsFollowIt()
        {
            var rig = new Rig();

            rig.List.SearchText = "dinamico";
            Assert.Equal(new[] { "Dinámico sur" }, Names(rig.List));
            Assert.Equal("Mostrando 1 de 6", rig.List.ShownText);
            Assert.Equal("1 de 1", rig.List.Groups.Single().CountText);

            rig.List.SearchText = "pb";
            Assert.Equal(new[] { "PB 2", "PB 10" }, Names(rig.List));

            rig.List.SearchText = "nada que coincida";
            Assert.Empty(rig.List.Groups);
            Assert.Equal("Ningún rack coincide con el filtro.", rig.List.NoResultsText);
        }

        [Fact]
        public void TheFilterBySystemShowsOnlyThatSystem()
        {
            var rig = new Rig();

            rig.List.SelectedKind = "pushback";

            Assert.Equal(new[] { "Push Back" }, rig.List.Groups.Select(g => g.KindLabel).ToArray());
            Assert.Equal("Mostrando 2 de 6", rig.List.ShownText);
        }

        [Fact]
        public void TheStateOfEachRackComesFromTheMemoryOfTheEditPortAndOnlyChangedShowsTheProtectedDrafts()
        {
            var rig = new Rig();
            rig.Pilot.Set(Session, "PB-2", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, isProtected: true));
            rig.Pilot.Set(Session, "SEL-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Clean, stale: true));
            rig.Pilot.Set(Session, "DIN-1", WorkspacePilotEditPortDouble.View(PilotFieldState.Conflict, isProtected: true));
            rig.List.RefreshStates();

            var items = rig.List.Groups.SelectMany(g => g.Items).ToDictionary(i => i.RackId);
            Assert.Equal(("Modificado", "Modificado"), (items["PB-2"].StatusText, items["PB-2"].StatusKind));
            Assert.Equal(("Limpio (posiblemente obsoleto)", "Obsoleto"), (items["SEL-1"].StatusText, items["SEL-1"].StatusKind));
            Assert.Equal(("Conflicto", "Obsoleto"), (items["DIN-1"].StatusText, items["DIN-1"].StatusKind));
            Assert.Equal(("Sin borrador", "Limpio"), (items["PB-10"].StatusText, items["PB-10"].StatusKind));
            Assert.Equal(2, rig.List.ChangedCount);

            rig.List.OnlyChanged = true;
            Assert.Equal(new[] { "PB 2", "Dinámico sur" }, Names(rig.List));
            Assert.Contains("2 con cambios sin escribir", rig.Window.HeaderText);
        }

        [Fact]
        public void ABusyHostKeepsTheListItHadAndSaysItIsReadingOnlyWhileThereIsNone()
        {
            var rig = new Rig();
            rig.Summary.ListResponder = (_, _) => BrowserRackList.Without(BrowserSummaryStatus.Busy, "drain closed");

            rig.Window.Refresh();
            Assert.Equal(6, rig.List.RackCount);
            Assert.Equal(string.Empty, rig.List.StatusText);

            var fresh = new Rig();
            fresh.Summary.ListResponder = (_, _) => BrowserRackList.Without(BrowserSummaryStatus.Busy, "drain closed");
            fresh.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 8);
            fresh.Window.Refresh();
            Assert.Equal(0, fresh.List.RackCount);
            Assert.Equal("Leyendo la lista del dibujo…", fresh.List.StatusText);
        }

        [Fact]
        public void AFailedOrThrowingReadIsContainedAndSaysWhy()
        {
            var rig = new Rig();
            rig.Summary.ListResponder = (_, _) => BrowserRackList.Without(BrowserSummaryStatus.Failed, "eLockViolation");
            rig.List.Reload();
            Assert.Equal("No se pudo leer la lista: eLockViolation", rig.List.StatusText);

            rig.Summary.ThrowOnList = new InvalidOperationException("boom");
            rig.List.Reload();
            Assert.Equal("No se pudo leer la lista: boom", rig.List.StatusText);
        }

        [Fact]
        public void ReadingAgainIsAnExplicitReadOfTheUser()
        {
            var rig = new Rig();
            rig.Summary.ListCalls.Clear();

            rig.List.Reload();

            Assert.Equal(new[] { true }, rig.Summary.ListCalls.Select(c => c.UserRequested).ToArray());
        }

        [Fact]
        public void WithoutADocumentOrWhileBlockedTheListIsNotAsked()
        {
            var rig = new Rig();
            rig.Summary.ListCalls.Clear();

            rig.Host.Snapshot = WorkspacePanelFixtures.WithBlock(WorkspaceActionBlock.CommandActive);
            rig.Window.Refresh();
            rig.Host.Snapshot = WorkspacePanelFixtures.NoDocument();
            rig.Window.Refresh();

            Assert.Empty(rig.Summary.ListCalls);
            Assert.Equal("No hay un dibujo abierto.", rig.List.StatusText);
            Assert.Equal("Sin dibujo abierto", rig.Window.HeaderText);
        }

        [Fact]
        public void APortThatDoesNotOfferTheListSaysItIsNotAvailable()
        {
            var rig = new Rig();
            rig.Summary.ListResponder = null;
            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 9);

            rig.Window.Refresh();

            Assert.Equal("La lista de racks no está disponible.", rig.List.StatusText);
            Assert.Empty(rig.List.Groups);
        }

        // ------------------------------------------------------------------ opening a rack

        [Fact]
        public void OpeningARackOfTheListMakesItTheActiveRackOfTheBrowserAndShowsItsPage()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Variables);

            rig.Window.OpenRack("PB-10");

            Assert.Equal("PB-10", rig.Panel.Browser.ActiveRackId);
            Assert.True(rig.Window.IsRackPage);
            var item = rig.List.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "PB-10");
            Assert.True(item.IsActive);
            Assert.Equal(string.Empty, rig.Window.NoRackText);
        }

        [Fact]
        public void AFollowedSelectionMakesTheSelectedRackActiveAndShowsItsPage()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Bom);

            rig.Select("SEL-1");

            Assert.Equal("SEL-1", rig.Panel.Browser.ActiveRackId);
            Assert.True(rig.Window.IsRackPage);
            Assert.False(rig.List.HasSelectionBox);
        }

        [Fact]
        public void WithoutFollowingTheSelectionTheSelectedRackWaitsBehindTheActiveOneAndCanBeOpened()
        {
            var rig = new Rig();
            rig.Window.OpenRack("PB-2");
            rig.List.FollowSelection = false;

            rig.Select("SEL-1");

            Assert.Equal("PB-2", rig.Panel.Browser.ActiveRackId);
            Assert.Equal("SEL-1", rig.Panel.Browser.SelectionRackId);
            Assert.True(rig.List.HasSelectionBox);
            Assert.Equal("Seleccionado en AutoCAD: Selectivo norte", rig.List.SelectionBoxText);
            Assert.True(rig.List.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "SEL-1").IsSelected);

            rig.List.OpenSelected();

            Assert.Equal("SEL-1", rig.Panel.Browser.ActiveRackId);
            Assert.False(rig.List.HasSelectionBox);
        }

        [Fact]
        public void FollowingTheSelectionAgainAppliesTheNextRealSelection()
        {
            var rig = new Rig();
            rig.Window.OpenRack("PB-2");
            rig.List.FollowSelection = false;
            rig.Select("SEL-1");

            rig.List.FollowSelection = true;
            rig.Select("DIN-1");

            Assert.Equal("DIN-1", rig.Panel.Browser.ActiveRackId);
        }

        [Fact]
        public void WithoutARackTheRackPageSaysHowToOpenOne()
        {
            var rig = new Rig();

            Assert.Null(rig.Panel.Browser.ActiveRackId);
            Assert.True(rig.Window.IsRackPage);
            Assert.Equal("Elige un rack en la lista o selecciónalo en el dibujo de AutoCAD.", rig.Window.NoRackText);
        }

        // ------------------------------------------------------------------ the sections and the doors

        [Fact]
        public void TheSectionsAreTheOnesOfThePrototypeAndOnlyTheShownPageIsActive()
        {
            var rig = new Rig();

            Assert.Equal(new[] { "Nuevo", "Variables", "BOM y lista de racks", "Biblioteca de diseños", "Configuración" }, rig.Window.Sections.Select(s => s.Label).ToArray());
            Assert.DoesNotContain(rig.Window.Sections, s => s.IsActive);

            rig.Window.ShowPage(RackCadWindowPage.Configuracion);

            Assert.Equal(new[] { RackCadWindowPage.Configuracion }, rig.Window.Sections.Where(s => s.IsActive).Select(s => s.Page).ToArray());
            Assert.True(rig.Window.IsConfiguracionPage);
            Assert.False(rig.Window.IsRackPage);
        }

        [Fact]
        public void TheNewSystemsAreTheOnesOfTheMenuEachWithItsAction()
        {
            var rig = new Rig();

            Assert.Equal(
                new[]
                {
                    RackCadClassicAction.NewSelective, RackCadClassicAction.NewPushBack, RackCadClassicAction.NewDynamic, RackCadClassicAction.NewCantilever,
                    RackCadClassicAction.NewHeader, RackCadClassicAction.NewFlowBed, RackCadClassicAction.NewLarguero, RackCadClassicAction.StructuralSection,
                },
                rig.Window.NewSystems.Select(c => c.Action).ToArray());
        }

        [Fact]
        public void ADoorRunsItsClassicActionAndSaysThatAutoCadTookIt()
        {
            var rig = new Rig();

            rig.Window.Run(RackCadClassicAction.NewPushBack);

            Assert.Equal(new[] { RackCadClassicAction.NewPushBack }, rig.Classic.Runs);
            Assert.True(rig.Window.HasActionMessage);
            Assert.False(rig.Window.ActionFailed);
            Assert.StartsWith("Se pidió a AutoCAD abrir el editor.", rig.Window.ActionMessage);

            rig.Window.DismissMessage();
            Assert.False(rig.Window.HasActionMessage);
        }

        [Theory]
        [InlineData(RackCadClassicStatus.Busy, "Hay un comando en curso en AutoCAD")]
        [InlineData(RackCadClassicStatus.NoDocument, "Abre o crea un dibujo")]
        [InlineData(RackCadClassicStatus.Failed, "No se pudo abrir: motivo")]
        public void ARefusedDoorSaysWhy(RackCadClassicStatus status, string start)
        {
            var rig = new Rig();
            rig.Classic.Answer = RackCadClassicResult.Without(status, "motivo");

            rig.Window.Run(RackCadClassicAction.Variables);

            Assert.True(rig.Window.ActionFailed);
            Assert.StartsWith(start, rig.Window.ActionMessage);
        }

        [Fact]
        public void AThrowingDoorIsContained()
        {
            var rig = new Rig();
            rig.Classic.Fault = new InvalidOperationException("boom");

            rig.Window.Run(RackCadClassicAction.TotalBom);

            Assert.True(rig.Window.ActionFailed);
            Assert.Equal("No se pudo abrir: boom", rig.Window.ActionMessage);
        }

        [Fact]
        public void WithoutTheDoorTheWindowSaysItIsNotAvailable()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
            var panel = new WorkspacePanelViewModel(host, _ => { });
            var window = new RackCadWindowViewModel(panel, null, new SettingsDouble());

            window.Run(RackCadClassicAction.Help);

            Assert.True(window.ActionFailed);
            Assert.Equal("Esta acción no está disponible aquí.", window.ActionMessage);
        }

        [Fact]
        public void TheBlockLibraryIsSavedAndResetThroughTheSettings()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Configuracion);
            Assert.Equal("C:\\bloques.dwg", rig.Window.BlockLibraryPath);
            Assert.StartsWith("Ruta predeterminada", rig.Window.BlockLibraryStatus);

            rig.Window.SetBlockLibraryPath("D:\\otra.dwg");
            Assert.Equal("D:\\otra.dwg", rig.Settings.Saved);
            Assert.Equal("D:\\otra.dwg", rig.Window.BlockLibraryPath);
            Assert.StartsWith("Ruta personalizada", rig.Window.BlockLibraryStatus);

            rig.Window.ResetBlockLibraryPath();
            Assert.Null(rig.Settings.Saved);
            Assert.Equal("Biblioteca de bloques restablecida a la ruta predeterminada.", rig.Window.ActionMessage);

            rig.Settings.BlockLibraryExists = false;
            Assert.True(rig.Window.BlockLibraryMissing);
            Assert.EndsWith("se omitirán.", rig.Window.BlockLibraryStatus);
        }

        [Fact]
        public void HidingTheSideColumnIsAWindowSwitch()
        {
            var rig = new Rig();

            Assert.Equal("Ocultar la lista", rig.Window.SideToggleText);
            rig.Window.SideCollapsed = true;
            Assert.Equal("Mostrar la lista", rig.Window.SideToggleText);
        }

        // ------------------------------------------------------------------ review of 16aebbee: pending text, closing, the outdated list

        [Fact]
        public void OpeningAnotherRackWithPendingTextCrossesTheBlockingBoundaryAndAnAbortKeepsTheRack()
        {
            var rig = new Rig();
            rig.Window.OpenRack("PB-2");
            rig.Pilot.Set(Session, "PB-2", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:2", isProtected: true));
            rig.Pilot.BoundaryResponder = (_, rack, kind) =>
                new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, new[] { new PilotFieldProblem("scale", "La escala no es válida.") }, null);

            rig.Window.OpenRack("SEL-1");

            Assert.Equal("PB-2", rig.Panel.Browser.ActiveRackId);
            Assert.Contains(rig.Pilot.Calls, c => c.RackId == "PB-2" && c.Kind == PilotBoundaryKind.TabChange);

            rig.Pilot.BoundaryResponder = (_, rack, kind) => new PilotBoundaryResult(kind, PilotBoundaryDecision.Proceeded, false, System.Array.Empty<PilotFieldProblem>(), null);
            rig.Window.OpenRack("SEL-1");
            Assert.Equal("SEL-1", rig.Panel.Browser.ActiveRackId);
        }

        [Fact]
        public void WithoutFollowingTheSelectionPendingTextStillHoldsTheActiveRackWithoutANoticeOfTheChoice()
        {
            var rig = new Rig();
            rig.Window.OpenRack("PB-2");
            rig.Pilot.Set(Session, "PB-2", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:2", isProtected: true));
            rig.List.FollowSelection = false;

            rig.Select("SEL-1");

            Assert.Equal("PB-2", rig.Panel.Browser.ActiveRackId);
            Assert.True(rig.List.HasSelectionBox);
            // The notice is the one of the pending text (as with the selection followed), never one about the choice of not following.
            Assert.Equal("La selección de SEL-1 no cambió el rack activo, que conserva su texto pendiente.", rig.Panel.Browser.NoticeText);
        }

        [Fact]
        public void WithoutFollowingTheSelectionAndWithoutPendingTextTheSelectionWaitsWithNoNotice()
        {
            var rig = new Rig();
            rig.Window.OpenRack("PB-2");
            rig.List.FollowSelection = false;

            rig.Select("SEL-1");

            Assert.Equal("PB-2", rig.Panel.Browser.ActiveRackId);
            Assert.True(rig.List.HasSelectionBox);
            Assert.Equal(string.Empty, rig.Panel.Browser.NoticeText ?? string.Empty);
        }

        [Fact]
        public void OpeningAnotherRackGoesOnWhenThePendingTextIsKeptAndStaysWhenThePortThrows()
        {
            // M-04 of the second review: the two answers of the boundary that the first tests did not cover.
            var rig = new Rig();
            rig.Window.OpenRack("PB-2");
            rig.Pilot.Set(Session, "PB-2", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, pending: "1:2", isProtected: true));
            rig.Pilot.BoundaryResponder = (_, _, kind) =>
                new PilotBoundaryResult(kind, PilotBoundaryDecision.Kept, false, System.Array.Empty<PilotFieldProblem>(), null);

            rig.Window.OpenRack("SEL-1");
            Assert.Equal("SEL-1", rig.Panel.Browser.ActiveRackId);

            rig.Window.OpenRack("PB-2");
            rig.Pilot.BoundaryResponder = null;
            rig.Pilot.Throws[WorkspacePilotEditPortDouble.BoundaryOp] = new InvalidOperationException("eLockViolation");

            rig.Window.OpenRack("SEL-1");

            Assert.Equal("PB-2", rig.Panel.Browser.ActiveRackId);
            Assert.Equal("Error: eLockViolation", rig.Panel.Browser.Field.StateText);
        }

        [Fact]
        public void AnOpenRackCanBeClosedFromTheListAndARackWithoutATabCannot()
        {
            var rig = new Rig();
            rig.Window.OpenRack("PB-2");
            var item = rig.List.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "PB-2");
            Assert.True(item.HasTab);
            Assert.False(rig.List.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "SEL-1").HasTab);

            rig.List.Close("PB-2");

            Assert.Null(rig.Panel.Browser.ActiveRackId);
            Assert.False(rig.List.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "PB-2").HasTab);
        }

        [Fact]
        public void ClosingFromTheListARackWithWorkNotWrittenAsksFirstAndCancelKeepsIt()
        {
            // M-03 of the second review: «Cerrar» of the list follows the rules of closing a tab, so a protected draft asks Discard or
            // Cancel in the page of the rack, which the window shows.
            var rig = new Rig();
            rig.Window.OpenRack("PB-2");
            rig.Pilot.Set(Session, "PB-2", WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, isProtected: true));
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);

            rig.List.Close("PB-2");

            Assert.True(rig.Panel.Browser.HasConfirmation);
            Assert.Equal(RackCadWindowPage.Rack, rig.Window.Page);
            Assert.True(rig.List.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "PB-2").HasTab);

            rig.Panel.Browser.CancelDiscard();

            Assert.False(rig.Panel.Browser.HasConfirmation);
            Assert.Equal("PB-2", rig.Panel.Browser.ActiveRackId);
            Assert.True(rig.List.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "PB-2").HasTab);
        }

        [Fact]
        public void AnOutdatedListIsKeptAndSaysSoUntilItIsReadAgain()
        {
            var rig = new Rig();
            rig.Summary.ListResponder = (_, user) => user ? BrowserRackList.Ready(rig.Racks) : BrowserRackList.Outdated();

            rig.Window.Refresh();
            Assert.Equal(6, rig.List.RackCount);
            Assert.Equal("La lista puede no estar al día: pulsa «Volver a leer la lista».", rig.List.StatusText);

            rig.List.Reload();
            Assert.Equal(string.Empty, rig.List.StatusText);
        }

        [Fact]
        public void ComingBackToADrawingShowsTheListReadThereSaidAsPossiblyNotUpToDate()
        {
            // The sequence of the review (P12): drawing 7 read, a command ends there, drawing 8 read, back to 7. The host reads a drawing
            // on its own only once, so its answer for 7 is now "outdated"; the column must still show the 6 racks of 7, not an empty list.
            var rig = new Rig();
            var other = new List<BrowserRackListEntry> { Entry("SEL-9", "Otro dibujo", "selective") };
            var outdated = new HashSet<long>();
            rig.Summary.ListResponder = (origin, user) =>
            {
                if (!user && outdated.Contains(origin.Origin.Value)) return BrowserRackList.Outdated();
                outdated.Add(origin.Origin.Value);
                return BrowserRackList.Ready(origin.Origin.Value == 8 ? other : rig.Racks);
            };
            rig.List.Reload();
            Assert.Equal(6, rig.List.RackCount);

            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 8);
            rig.Window.Refresh();
            Assert.Equal(new[] { "Otro dibujo" }, Names(rig.List));
            Assert.Equal(string.Empty, rig.List.StatusText);

            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 7);
            rig.Window.Refresh();
            Assert.Equal(6, rig.List.RackCount);
            Assert.DoesNotContain("Otro dibujo", Names(rig.List));
            Assert.Equal("La lista puede no estar al día: pulsa «Volver a leer la lista».", rig.List.StatusText);

            rig.List.Reload();
            Assert.Equal(string.Empty, rig.List.StatusText);
        }

        [Fact]
        public void ReadingAgainWhileACommandRunsSaysWhyNothingHappened()
        {
            var rig = new Rig();
            // The session of the list (7), with a command in progress.
            rig.Host.Snapshot = new WorkspaceHostSnapshot(WorkspaceHostStatus.Ready, Session, true,
                new WorkspaceActionAvailability(false, false, WorkspaceActionBlock.CommandActive), SelectionContext.None, false, false);
            rig.Window.Refresh();

            rig.List.Reload();

            Assert.StartsWith("El dibujo no se puede leer ahora", rig.List.StatusText);
        }

        [Fact]
        public void AProtectedCleanDraftIsAChangeNotWrittenAndLooksLikeOne()
        {
            var rig = new Rig();
            rig.Pilot.Set(Session, "PB-10", WorkspacePilotEditPortDouble.View(PilotFieldState.Clean, isProtected: true));
            rig.List.RefreshStates();

            var item = rig.List.Groups.SelectMany(g => g.Items).Single(i => i.RackId == "PB-10");
            Assert.Equal(("Borrador sin escribir", "Modificado"), (item.StatusText, item.StatusKind));
            Assert.Equal(1, rig.List.ChangedCount);
        }

        [Fact]
        public void TheModalHostRunsTheWorkWithItsWindowAsOwnerAndCloses()
        {
            StaTestRunner.Run(() =>
            {
                Window? seen = null;
                var childShown = false;
                var host = new RackModalHostWindow(owner =>
                {
                    seen = owner;
                    var child = new Window { Owner = owner, Width = 50, Height = 50, ShowInTaskbar = false };
                    child.Loaded += (_, _) => { childShown = true; child.Close(); };
                    child.ShowDialog();
                    return 42;
                });

                ShowWithSafetyTimer(host.Window);

                Assert.Same(host.Window, seen);
                Assert.True(childShown);
                Assert.Equal(42, host.Result);
                Assert.Null(host.Failure);
                Assert.False(host.Window.IsVisible);
                Assert.NotNull(host.Window.Content);
            });
        }

        [Fact]
        public void AFailingWorkOfTheModalHostClosesItAndIsRethrown()
        {
            StaTestRunner.Run(() =>
            {
                var host = new RackModalHostWindow(_ => throw new InvalidOperationException("boom"));

                ShowWithSafetyTimer(host.Window);

                Assert.IsType<InvalidOperationException>(host.Failure);
                var thrown = Assert.Throws<InvalidOperationException>(() => host.ThrowIfFailed());
                Assert.Equal("boom", thrown.Message);
                Assert.Null(host.Result);
            });
        }

        [Fact]
        public void TheModalHostIsNotANewWindowType()
        {
            Assert.False(typeof(Window).IsAssignableFrom(typeof(RackModalHostWindow)));
        }

        // A host that never runs its work would leave ShowDialog open for ever: the timer closes the window and the test fails instead of
        // hanging the job of UI (M-01 of the second review).
        private static void ShowWithSafetyTimer(Window window)
        {
            var timedOut = false;
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                timedOut = true;
                window.Close();
            };
            timer.Start();
            try
            {
                window.ShowDialog();
            }
            finally
            {
                timer.Stop();
            }

            Assert.False(timedOut, "the modal host did not run its work and close by itself.");
        }
        // ------------------------------------------------------------------ the view

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

        private static void Lay(FrameworkElement view)
        {
            view.Measure(new Size(900, 900));
            view.Arrange(new Rect(0, 0, 900, 900));
            view.UpdateLayout();
        }

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        [Fact]
        public void TheViewShowsTheWorkspacePanelAsThePageOfTheRackWithoutItsStripOfTabs()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                Lay(view);

                Assert.Same(view.PanelView, view.RackPageHost.Content);
                Assert.Same(rig.Panel, view.PanelView.ViewModel);
                Assert.Equal(Visibility.Collapsed, view.PanelView.BrowserTabStrip.Visibility);
                Assert.Equal(Visibility.Collapsed, view.PanelView.RootLabelText.Visibility);
                Assert.Equal(Visibility.Visible, view.RackPage.Visibility);
                Assert.Equal(Visibility.Collapsed, view.NuevoPage.Visibility);
            });
        }

        [Fact]
        public void ClickingASectionAndARackOfTheListChangesThePage()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                Lay(view);

                var nuevo = Descendants<Button>(view.SectionList).Single(b => b.DataContext is RackCadWindowSection s && s.Page == RackCadWindowPage.Nuevo);
                Click(nuevo);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.NuevoPage.Visibility);
                Assert.Equal(Visibility.Collapsed, view.RackPage.Visibility);

                var rack = Descendants<Button>(view.RackGroupList).Single(b => b.DataContext is RackListItem i && i.RackId == "DIN-1"
                    && System.Windows.Automation.AutomationProperties.GetName(b) != "Cerrar el rack abierto");
                Click(rack);
                Lay(view);
                Assert.Equal("DIN-1", rig.Panel.Browser.ActiveRackId);
                Assert.Equal(Visibility.Visible, view.RackPage.Visibility);
            });
        }

        [Fact]
        public void ACardOfNuevoAndAButtonOfAPageRunTheirClassicAction()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view);

                var card = Descendants<Button>(view.NewSystemList).Single(b => b.DataContext is RackCadNewSystemCard c && c.Action == RackCadClassicAction.NewCantilever);
                Click(card);

                rig.Window.ShowPage(RackCadWindowPage.Bom);
                Lay(view);
                var bom = Descendants<Button>(view.BomPage).Single(b => (b.Tag as string) == "TotalBom");
                Click(bom);

                Click(view.ClassicMenuButton);

                Assert.Equal(new[] { RackCadClassicAction.NewCantilever, RackCadClassicAction.TotalBom, RackCadClassicAction.ClassicMenu }, rig.Classic.Runs);
            });
        }

        [Fact]
        public void TheSwitchOfTheSideColumnHidesItAndGivesItsWidthBack()
        {
            StaTestRunner.Run(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                Lay(view);

                Click(view.SideToggleButton);
                Assert.Equal(0, view.SideColumn.Width.Value);
                Assert.Equal(Visibility.Collapsed, view.SidePanel.Visibility);

                Click(view.SideToggleButton);
                Assert.Equal(250, view.SideColumn.Width.Value);
                Assert.Equal(Visibility.Visible, view.SidePanel.Visibility);
            });
        }
    }
}
