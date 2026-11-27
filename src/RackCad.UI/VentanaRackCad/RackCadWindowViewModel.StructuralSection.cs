using System;
using System.ComponentModel;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T7 per (I-64, séptima tanda; the Owner, 2026-11-27: «Igual que la clásica» and «Abren la ventana»): «Nuevo › Perfil estructural», the
    /// page of the window with the inspector of RACKSECCION (<see cref="StructuralSectionPageViewModel"/>). The card «Perfil estructural» of
    /// «Nuevo», RACKSECCION (the pattern of the shortcuts of the sixth batch) and «En la ventana» of the help open it; the classic window stays
    /// behind «Generar perfil estructural» of RACKMENU. Its «Insertar en el dibujo…» is a request of the door like the others (ONE in flight
    /// for the whole window, numbered, «Dejar de esperar», the answer only for its request, the answer during the call and the prune of a
    /// closed drawing), to RACKSECCIONINSERTAR. Without a door that implements <see cref="IRackCadStructuralSectionDoorPort"/>, the page shows
    /// the inspector and says it cannot write the drawing.
    /// </summary>
    public sealed partial class RackCadWindowViewModel : IStructuralSectionPageHost
    {
        private IRackCadStructuralSectionDoorPort _sectionDoor;

        /// <summary>The neutral catalogue of structural sections the page reads (the one of RACKSECCION); the tests give theirs.</summary>
        internal Func<StructuralSectionCatalog> StructuralSectionCatalogLoader { get; set; } = LoadStructuralSectionCatalog;

        /// <summary>The page «Perfil estructural».</summary>
        public StructuralSectionPageViewModel StructuralSection { get; private set; }

        /// <summary>«Nuevo › Perfil estructural» is shown.</summary>
        public bool IsStructuralSectionPage => _page == RackCadWindowPage.PerfilEstructural;

        /// <summary>
        /// Shows the page «Perfil estructural» (the card of «Nuevo», RACKSECCION): it reads the catalogue every time it is shown, so an edit of
        /// a file shows up, and what was chosen stays while the catalogue is the same. Contained, as every action of the window.
        /// </summary>
        public void OpenStructuralSectionPage()
        {
            Contained(() =>
            {
                if (IsStructuralSectionPage) StructuralSection.Shown();
                else ShowPage(RackCadWindowPage.PerfilEstructural);
            });
        }

        /// <summary>
        /// A card of «Nuevo»: «Perfil estructural» opens its page in the window (T7 per); every other card runs its classic action, as before.
        /// </summary>
        public void OpenNewSystem(RackCadNewSystemCard card)
        {
            if (card == null) return;
            if (card.Action == RackCadClassicAction.StructuralSection) OpenStructuralSectionPage();
            else Run(card.Action);
        }

        // Called once by the constructor: the page, the door, and what «Insertar en el dibujo…» follows.
        private void WireStructuralSection()
        {
            _sectionDoor = _door as IRackCadStructuralSectionDoorPort;
            StructuralSection = new StructuralSectionPageViewModel(() => StructuralSectionCatalogLoader(), this);
            if (_sectionDoor != null)
            {
                _sectionDoor.StructuralSectionInserted += OnStructuralSectionInserted;
                _door.RequestStarted += OnStructuralSectionRequestStarted;
            }

            PropertyChanged += OnWindowChangedForStructuralSection;
        }

        // Another request of the window waits, or not; the drawing presented changed (the header follows it): «Insertar en el dibujo…» follows.
        private void OnWindowChangedForStructuralSection(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IsWaitingForAutoCad) && e.PropertyName != nameof(HeaderText)) return;
            Contained(() => StructuralSection?.HostChanged());
        }

        // From ShowPage: what the center shows follows the page; shown, the page reads the catalogue.
        private void StructuralSectionPageShown(RackCadWindowPage page)
        {
            OnPropertyChanged(nameof(IsStructuralSectionPage));
            if (page == RackCadWindowPage.PerfilEstructural) Contained(() => StructuralSection.Shown());
        }

        // RACKSECCION, inside the command: the page, as a click on its card. A catalogue that could not be read is said in the command line too.
        private void ShowStructuralSectionByShortcut(RackCadShortcutAsk ask)
        {
            OpenStructuralSectionPage();
            if (IsStructuralSectionPage && StructuralSection.HasCatalogNotice) ask.Said = StructuralSection.CatalogNotice;
        }

        // ------------------------------------------------------------------ the door (RACKSECCIONINSERTAR)

        // The waiting text of the window while the insertion of the page waits (one line of WaitingText).
        private string StructuralSectionWaitingText =>
            _inFlight != null && _inFlight.Started ? StructuralSectionDoorRules.TakenText : StructuralSectionDoorRules.WaitingText;

        bool IStructuralSectionPageHost.HasInsertDoor => _sectionDoor != null;

        bool IStructuralSectionPageHost.HasDrawing => Panel.PresentedSession.HasValue;

        bool IStructuralSectionPageHost.OtherRequestWaiting => _inFlight != null && !_inFlight.IsSection;

        void IStructuralSectionPageHost.GoHome() => ShowPage(RackCadWindowPage.Nuevo);

        void IStructuralSectionPageHost.ShowError(Exception exception) => ShowError(exception);

        string IStructuralSectionPageHost.RequestInsert(StructuralSectionDefinition section, StructuralSectionRepresentationPlan plan)
        {
            if (_sectionDoor == null) return StructuralSectionPageViewModel.NoDoorText;
            var session = Panel.PresentedSession;
            if (!session.HasValue) return StructuralSectionPageViewModel.NoDrawingText;
            if (_inFlight != null) return DoorBusyText;
            var refusal = StructuralSectionDoorRules.RefusalOf(new StructuralSectionInsertRequest(0, session.Value, section, plan));
            if (refusal != null) return refusal;

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22). No rack in it: the
            // branches of the editors look for their draft by its rack.
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, true, session.Value, null, null, null, IsSection: true);
            StructuralSection.InsertWaiting(id);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _sectionDoor.RequestStructuralSectionInsert(new StructuralSectionInsertRequest(id, session.Value, section, plan))
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted) return null;
            if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
            StructuralSection.InsertStopped(null);
            RaiseWaiting();
            return RefusalText(result);
        }

        // RACKSECCIONINSERTAR took the request (the window already marked it Started): the page says AutoCAD asks for the point.
        private void OnStructuralSectionRequestStarted(long requestId)
        {
            Contained(() =>
            {
                if (_inFlight == null || !_inFlight.IsSection || _inFlight.Id != requestId) return;
                if (!_inFlight.Started) _inFlight = _inFlight with { Started = true };
                StructuralSection.InsertTaken(requestId);
                RaiseWaiting();
            });
        }

        // The answer: only for the request that waits. The section is in the drawing of the request (the command checked it).
        private void OnStructuralSectionInserted(StructuralSectionInserted answer)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (answer == null || request == null || !request.IsSection || answer.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();
                StructuralSection.InsertAnswered(answer);
                // The page says it; from another page the message of the window says it too.
                if (!IsStructuralSectionPage) Say(answer.Message ?? string.Empty, failed: answer.Failed);
            });
        }

        // «Dejar de esperar» of the request of the page (from StopWaiting), or its drawing was closed (from PruneDeadSessions).
        private void StructuralSectionStopped(string text)
        {
            StructuralSection.InsertStopped(text);
            if (!IsStructuralSectionPage) Say(text, failed: false);
        }

        private static StructuralSectionCatalog LoadStructuralSectionCatalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();
    }
}
