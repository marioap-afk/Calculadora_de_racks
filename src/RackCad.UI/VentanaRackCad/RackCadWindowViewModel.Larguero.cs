using System;
using System.ComponentModel;
using System.Globalization;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Nuevo › Larguero» of the RackCad window (I-64 A-14, line D; quinta tanda, part «lg», LG-01 to LG-03; sexta tanda, part «lg», the answers
    /// of the Owner): the page that makes a larguero from a section and a ménsula, saves it in the library (point 1), inserts it in the drawing
    /// as a loose piece through the numbered door, RACKLARGUEROINSERTAR, one «U» (point 2), and with an AISC section inserts a bare profile as
    /// RACKSECCION (point 6). It is a page of the window (<see cref="RackCadWindowPage.Larguero"/>), under «Nuevo» in the side column, and not
    /// a «Borrador N» of the drawing: a larguero is a component of the library, not a rack; its work never vetoes the close of a drawing
    /// (assumption LG-S01). Its view-model is made the first time the page opens (or a larguero of the library opens) and lives as long as the
    /// window; every opening reads the two catalogs again (review of part «lg», point 1).
    /// </summary>
    public sealed partial class RackCadWindowViewModel : ILargueroWorkbenchHost
    {
        private const string LargueroTakenText =
            "AutoCAD ya está con la inserción de «Larguero» (pide el punto en la línea de órdenes): indícalo o cancélalo con Esc; la ventana recibirá su respuesta.";

        private LargueroWorkbenchViewModel _largueroWorkbench;
        private IRackCadLargueroDoorPort _largueroDoor;
        private bool _largueroWired;

        /// <summary>The neutral catalog of structural sections the page reads (the one of RACKSECCION); the tests give theirs.</summary>
        internal Func<StructuralSectionCatalog> LargueroSectionsLoader { get; set; } = LoadStructuralSections;

        /// <summary>
        /// The catalog of RackCad the page reads: the provider itself, which caches by the signature of the files and THROWS when it cannot
        /// read them, so the page says it (review of part «lg», point 1). Not <c>UiSupport.LoadCatalogSafe</c>, which gives an empty catalog
        /// in silence. The tests give theirs.
        /// </summary>
        internal Func<RackCatalog> LargueroRackCatalogLoader { get; set; } = LoadRackCatalog;

        /// <summary>«Nuevo › Larguero» is shown.</summary>
        public bool IsLargueroPage => _page == RackCadWindowPage.Larguero;

        /// <summary>The page «Nuevo › Larguero»; null until it is opened for the first time.</summary>
        public LargueroWorkbenchViewModel LargueroWorkbench => _largueroWorkbench;

        /// <summary>
        /// «Larguero en esta ventana» of «Nuevo»: shows the page (made the first time). Every opening reads the catalogs again: an edit of a
        /// CSV shows up, and what was chosen stays (<see cref="LargueroWorkbenchViewModel.Reload"/>). Contained, as every action of the window.
        /// </summary>
        public void OpenLargueroWorkbench()
        {
            Contained(() =>
            {
                if (!EnsureLargueroWorkbench()) _largueroWorkbench.Reload();
                ShowPage(RackCadWindowPage.Larguero);
            });
        }

        // T6 lg: makes the page the first time (true), with the door of the window and what it follows.
        private bool EnsureLargueroWorkbench()
        {
            WireLarguero();
            if (_largueroWorkbench != null) return false;
            _largueroWorkbench = new LargueroWorkbenchViewModel(() => LargueroRackCatalogLoader(), () => LargueroSectionsLoader(),
                () => ShowPage(RackCadWindowPage.Nuevo), () => Run(RackCadClassicAction.NewLarguero), ShowError, this);
            OnPropertyChanged(nameof(LargueroWorkbench));
            return true;
        }

        // T6 lg: the door of the page (RACKLARGUEROINSERTAR) and what «Insertar en el dibujo…» follows, once.
        private void WireLarguero()
        {
            if (_largueroWired) return;
            _largueroWired = true;
            _largueroDoor = _door as IRackCadLargueroDoorPort;
            if (_largueroDoor != null)
            {
                _largueroDoor.LargueroInserted += OnLargueroInserted;
                _door.RequestStarted += OnLargueroRequestStarted;
            }

            PropertyChanged += OnWindowChangedForLarguero;
        }

        // Another request of the window waits, or not; the drawing presented changed (the header follows it): «Insertar en el dibujo…» follows.
        private void OnWindowChangedForLarguero(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IsWaitingForAutoCad) && e.PropertyName != nameof(HeaderText)) return;
            Contained(() => _largueroWorkbench?.HostChanged());
        }

        // ShowPage: what the center shows follows the page.
        private void RaiseLargueroPage() => OnPropertyChanged(nameof(IsLargueroPage));

        // The side column marks «Nuevo» while its page «Larguero» is shown (the crumb «Nuevo › Larguero»).
        private bool IsUnderNuevo(RackCadWindowPage section) => section == RackCadWindowPage.Nuevo && _page == RackCadWindowPage.Larguero;

        // ------------------------------------------------------------------ T6 lg: «Insertar en el dibujo…» (RACKLARGUEROINSERTAR)

        bool ILargueroWorkbenchHost.HasInsertDoor => _largueroDoor != null;

        bool ILargueroWorkbenchHost.HasDrawing => Panel.PresentedSession.HasValue;

        bool ILargueroWorkbenchHost.OtherRequestWaiting => _inFlight != null && !_inFlight.IsLarguero;

        string ILargueroWorkbenchHost.RequestInsert(LargueroPiecePlan plan)
        {
            if (_largueroDoor == null) return "Aquí no se puede escribir el dibujo.";
            var session = Panel.PresentedSession;
            if (!session.HasValue) return "Abre o crea un dibujo para insertar el larguero.";
            if (_inFlight != null) return DoorBusyText;
            if (plan == null) return "No hay una pieza completa que insertar.";

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22). No rack in it:
            // the branches of the editors look for their draft by its rack.
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, true, session.Value, null, null, null, IsLarguero: true);
            _largueroWorkbench?.InsertWaiting(id);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _largueroDoor.RequestLargueroInsert(new LargueroInsertRequest(id, session.Value, plan))
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted) return null;
            if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
            RaiseWaiting();
            return RefusalText(result);
        }

        // RACKLARGUEROINSERTAR took the request (the window already marked it Started): the page says AutoCAD asks for the point.
        private void OnLargueroRequestStarted(long requestId)
        {
            Contained(() =>
            {
                if (_inFlight == null || !_inFlight.IsLarguero || _inFlight.Id != requestId) return;
                if (!_inFlight.Started) _inFlight = _inFlight with { Started = true };
                _largueroWorkbench?.InsertTaken(requestId);
                RaiseWaiting();
            });
        }

        // The answer: only for the request that waits. The piece is in the drawing of the request (the command checked it).
        private void OnLargueroInserted(LargueroInserted answer)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (answer == null || request == null || !request.IsLarguero || answer.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();
                _largueroWorkbench?.InsertAnswered(answer);
                // The page says it; from another page the message of the window says it too.
                if (!IsLargueroPage) Say(answer.Message ?? string.Empty, failed: !answer.Inserted);
            });
        }

        // «Dejar de esperar» of the request of the page (from StopWaiting), or its drawing was closed (from PruneDeadSessions).
        private void LargueroStopped(string text)
        {
            _largueroWorkbench?.InsertStopped(text);
            if (!IsLargueroPage) Say(text, failed: false);
        }

        // ------------------------------------------------------------------ T6 lg: the library (points 1 and 5)

        void ILargueroWorkbenchHost.SaveToLibrary(LargueroWorkbenchViewModel workbench)
        {
            Contained(() =>
            {
                if (workbench == null) return;
                BeginLibrarySave(new RackCadLibrarySaveSource(workbench, workbench.EffectiveName, "Larguero", () => string.Empty,
                    name => BuildLargueroProject(workbench, name), workbench.SaveRefusal));
            });
        }

        // What is saved for a larguero: the design of the page, with the name typed in the line of the save, and the unknown fields of the
        // file it was opened from (I-11).
        private static (RackProject Project, string Error) BuildLargueroProject(LargueroWorkbenchViewModel workbench, string name)
        {
            var design = workbench.BuildDesign(name);
            if (design == null) return (null, workbench.SaveRefusal() ?? "no hay un larguero completo que guardar.");
            return (RackProject.ForLarguero(design).WithSourceMetadataFrom(workbench.LibrarySource), null);
        }

        // «Abrir» of a larguero of the library (point 5): this page, with the larguero; no drawing needed.
        private void OpenLargueroFromLibrary(RackCadLibraryRow row)
        {
            RackProject project;
            try
            {
                project = new RackProjectStore().Load(row.Path);
            }
            catch (Exception ex)
            {
                Say("No se pudo abrir «" + row.Name + "»: " + ex.Message, failed: true);
                return;
            }

            if (project?.Larguero == null)
            {
                Say("«" + row.Name + "» no es un larguero que esta página pueda abrir.", failed: true);
                return;
            }

            if (!EnsureLargueroWorkbench()) _largueroWorkbench.Reload();
            _largueroWorkbench.LoadDesign(project.Larguero, row.Name);
            _largueroWorkbench.LibrarySource = project;
            ShowPage(RackCadWindowPage.Larguero);
            Say(string.Empty, failed: false);
        }

        private static RackCatalog LoadRackCatalog() => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static StructuralSectionCatalog LoadStructuralSections() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();
    }
}
