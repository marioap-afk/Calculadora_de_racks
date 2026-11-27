using System;
using System.IO;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.VentanaRackCad;
using RackCad.UI.StructuralSections;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>T7 per: what the page «Perfil estructural» needs of the window: its door (through it), the drawing, «Nuevo» and the message of the window.</summary>
    internal interface IStructuralSectionPageHost
    {
        /// <summary>The window has the door of the page (RACKSECCIONINSERTAR).</summary>
        bool HasInsertDoor { get; }

        /// <summary>There is a drawing to insert into.</summary>
        bool HasDrawing { get; }

        /// <summary>Another request of the window waits for AutoCAD (the door takes one at a time).</summary>
        bool OtherRequestWaiting { get; }

        /// <summary>
        /// Asks AutoCAD to insert <paramref name="plan"/> of <paramref name="section"/> (a gesture of the user): null when it was handed over
        /// (the page waits for the answer), else why not. The window tells the page the number of the request BEFORE it hands it over
        /// (<see cref="StructuralSectionPageViewModel.InsertWaiting"/>), because AutoCAD may answer during the call.
        /// </summary>
        string RequestInsert(StructuralSectionDefinition section, StructuralSectionRepresentationPlan plan);

        /// <summary>The crumb «Nuevo».</summary>
        void GoHome();

        /// <summary>A failure the page contained: the message of the window says it.</summary>
        void ShowError(Exception exception);
    }

    /// <summary>
    /// T7 per (I-64, séptima tanda; the Owner, 2026-11-27: «Igual que la clásica»): the page «Perfil estructural» of the RackCad window, under
    /// «Nuevo» (the crumb «Nuevo › Perfil estructural»). It is the inspector «Secciones estructurales» of RACKSECCION without blocking AutoCAD:
    /// its state is a <see cref="StructuralSectionInspectorState"/> and its view shows the very controls of the classic window
    /// (<see cref="StructuralSectionInspectorControls"/>), so the search, the families, the length, the view, the detail, the representation,
    /// the rotation, the mirror, the axis, the envelope, the preview and the lines of weight and fidelity are the classic ones. Nothing here
    /// computes a coordinate.
    ///
    /// <para>The catalogue is read when the page is shown, through the provider of RACKSECCION, and FAILS CLOSED as the classic does: a
    /// catalogue that does not validate is said, and the page offers nothing to insert. The provider caches by the signature of the files, so
    /// showing the page again keeps what was chosen unless a file changed.</para>
    ///
    /// <para>«Insertar en el dibujo…» is a numbered request of the door of the window (RACKSECCIONINSERTAR): ONE in flight for the whole
    /// window, with the plan the preview drew and its section, «Dejar de esperar» while AutoCAD has not taken it, and the answer only for its
    /// request: what was drawn, the units advisory and the report of the flow (weight, ADR-0023, fidelity). The drawn section is a loose
    /// block with no rack data: it does not reach the list and is never reopened, and the page has no draft, so it never vetoes the close of a
    /// drawing.</para>
    /// </summary>
    public sealed class StructuralSectionPageViewModel : ObservableObject
    {
        public const string IntroText =
            "Lo mismo que «Secciones estructurales» de RACKSECCION, sin bloquear AutoCAD: busca una sección del catálogo AISC (W, HSS, C, L y S), "
            + "dale longitud, vista y detalle, y revísala en la vista previa. «Insertar en el dibujo…» pide el punto en AutoCAD y la dibuja como una "
            + "sección suelta, sin datos de rack; un «U» la deshace.";

        /// <summary>Why «Insertar en el dibujo…» is off with no selection: the words of the classic window.</summary>
        public const string NoSelectionText = "Elige una sección de la lista para poder insertarla.";

        public const string NoDoorText = "Aquí no se puede escribir el dibujo.";

        public const string NoDrawingText = "Abre o crea un dibujo para insertar la sección.";

        public const string NoCatalogText = "Sin un catálogo de secciones válido no se inserta nada.";

        public const string OtherRequestText =
            "La ventana espera otra respuesta de AutoCAD: cuando termine, vuelve a pulsar «Insertar en el dibujo…».";

        private readonly Func<StructuralSectionCatalog> _loader;
        private readonly IStructuralSectionPageHost _host;
        private StructuralSectionInspectorState _state;
        private string _catalogNotice = string.Empty;

        private long? _request;
        private bool _taken;
        private string _status = string.Empty;
        private bool _statusFailed;
        private string _report = string.Empty;
        private bool _unitsWarned;

        internal StructuralSectionPageViewModel(Func<StructuralSectionCatalog> loader, IStructuralSectionPageHost host)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        // ------------------------------------------------------------------ the catalogue and the state

        /// <summary>The state of the inspector; null until the page is shown, and when the catalogue could not be read.</summary>
        public StructuralSectionInspectorState State => _state;

        public bool HasState => _state != null;

        /// <summary>Why there is nothing to inspect: the catalogue did not validate or could not be read (fail closed).</summary>
        public string CatalogNotice => _catalogNotice;

        public bool HasCatalogNotice => _catalogNotice.Length > 0;

        /// <summary>
        /// The page was shown (a click on its card, the crumb of the help, RACKSECCION): it reads the catalogue. The same catalogue keeps the
        /// state (what was chosen stays); another one (a file changed) gives a fresh state; a failure drops the state and says why.
        /// </summary>
        public void Shown()
        {
            StructuralSectionCatalog catalog;
            string failure = null;
            try
            {
                catalog = _loader();
                if (catalog == null) failure = "No se pudo cargar el catálogo de secciones estructurales, así que no se dibujará nada.";
            }
            catch (StructuralSectionCatalogException ex)
            {
                catalog = null;
                failure = "El catálogo de secciones estructurales no es válido, así que no se dibujará nada. " + ex.Message;
            }
            catch (IOException ex)
            {
                catalog = null;
                failure = "No se pudo leer el catálogo de secciones estructurales, así que no se dibujará nada. " + ex.Message;
            }
            catch (Exception ex)
            {
                catalog = null;
                failure = "No se pudo cargar el catálogo de secciones estructurales, así que no se dibujará nada. " + ex.Message;
            }

            var before = _state;
            if (failure != null)
            {
                _state = null;
                _catalogNotice = failure.Trim();
            }
            else
            {
                _catalogNotice = string.Empty;
                if (_state == null || !ReferenceEquals(_state.Catalog, catalog)) _state = new StructuralSectionInspectorState(catalog);
            }

            if (!ReferenceEquals(before, _state))
            {
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(HasState));
            }

            OnPropertyChanged(nameof(CatalogNotice));
            OnPropertyChanged(nameof(HasCatalogNotice));
            RaiseInsert();
        }

        // ------------------------------------------------------------------ «Insertar en el dibujo…»

        /// <summary>«Insertar en el dibujo…» can be pressed now.</summary>
        public bool CanInsert => InsertRefusal == null;

        /// <summary>Why «Insertar en el dibujo…» is off (also while the page waits for AutoCAD); empty when it is on.</summary>
        public string InsertReason => InsertRefusal ?? string.Empty;

        public bool HasInsertReason => InsertReason.Length > 0;

        private string InsertRefusal
        {
            get
            {
                if (!_host.HasInsertDoor) return NoDoorText;
                if (_state == null) return HasCatalogNotice ? NoCatalogText : string.Empty;
                if (_request.HasValue) return _taken ? StructuralSectionDoorRules.TakenText : StructuralSectionDoorRules.WaitingText;
                if (!_state.HasSelection) return NoSelectionText;
                if (_host.OtherRequestWaiting) return OtherRequestText;
                if (!_host.HasDrawing) return NoDrawingText;
                return null;
            }
        }

        /// <summary>The insertion waits for AutoCAD (handed over, or taken and asking the point).</summary>
        public bool IsWaiting => _request.HasValue;

        /// <summary>
        /// «Insertar en el dibujo…» (a gesture of the user): the plan of the current settings and its section, built by the state as the
        /// classic «Insertar» builds them, to the door. A refusal is said here and nothing waits.
        /// </summary>
        public void Insert()
        {
            if (!CanInsert) return;

            var section = _state.Selected;
            var plan = _state.BuildPlan();
            var refusal = _host.RequestInsert(section, plan);
            if (refusal != null) SetStatus(refusal, failed: true, report: string.Empty, unitsWarned: false);
            RaiseInsert();
        }

        /// <summary>The crumb «Nuevo».</summary>
        public void GoHome() => _host.GoHome();

        /// <summary>The controls repainted (a selection, a filter, a value): «Insertar en el dibujo…» follows the selection.</summary>
        public void SelectionMayHaveChanged() => RaiseInsert();

        /// <summary>The window changed (the drawing presented, another request waits or not): «Insertar en el dibujo…» follows.</summary>
        internal void HostChanged() => RaiseInsert();

        /// <summary>Runs a gesture of the view, containing a failure in the message of the window.</summary>
        public void Contained(Action interaction)
        {
            try
            {
                interaction?.Invoke();
            }
            catch (Exception ex)
            {
                _host.ShowError(ex);
            }
        }

        // ------------------------------------------------------------------ what the window tells the page

        /// <summary>The request of that number waits (said BEFORE it is handed over): the answer of the last one goes.</summary>
        internal void InsertWaiting(long requestId)
        {
            _request = requestId;
            _taken = false;
            SetStatus(string.Empty, failed: false, report: string.Empty, unitsWarned: false);
            RaiseInsert();
        }

        /// <summary>RACKSECCIONINSERTAR took the request: AutoCAD asks the point.</summary>
        internal void InsertTaken(long requestId)
        {
            if (_request != requestId) return;
            _taken = true;
            RaiseInsert();
        }

        /// <summary>The answer of AutoCAD to the request that waits: what it drew (or why not), the units advisory and the report of the flow.</summary>
        internal void InsertAnswered(StructuralSectionInserted answer)
        {
            if (answer == null || _request != answer.RequestId) return;
            _request = null;
            _taken = false;
            SetStatus(answer.Message ?? string.Empty, answer.Failed, answer.Report ?? string.Empty, answer.UnitsWarned);
            RaiseInsert();
        }

        /// <summary>The request no longer waits («Dejar de esperar», its drawing closed, or the door refused it); <paramref name="text"/> says it, or nothing.</summary>
        internal void InsertStopped(string text)
        {
            _request = null;
            _taken = false;
            if (!string.IsNullOrEmpty(text)) SetStatus(text, failed: false, report: string.Empty, unitsWarned: false);
            RaiseInsert();
        }

        // ------------------------------------------------------------------ the answer, as the page shows it

        /// <summary>What happened to the last insertion: drawn, cancelled, failed, refused or abandoned.</summary>
        public string StatusText => _status;

        public bool HasStatus => _status.Length > 0;

        public bool StatusFailed => _statusFailed;

        /// <summary>The report of the flow of RACKSECCION, verbatim: the same text the command line received.</summary>
        public string ReportText => _report;

        public bool HasReport => _report.Length > 0;

        /// <summary>The units advisory (I-05), when it fired before the point.</summary>
        public string UnitsText => _unitsWarned ? StructuralSectionDoorRules.UnitsText : string.Empty;

        public bool HasUnitsNotice => _unitsWarned;

        private void SetStatus(string status, bool failed, string report, bool unitsWarned)
        {
            _status = (status ?? string.Empty).Trim();
            _statusFailed = failed && _status.Length > 0;
            _report = (report ?? string.Empty).Trim();
            _unitsWarned = unitsWarned;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(HasStatus));
            OnPropertyChanged(nameof(StatusFailed));
            OnPropertyChanged(nameof(ReportText));
            OnPropertyChanged(nameof(HasReport));
            OnPropertyChanged(nameof(UnitsText));
            OnPropertyChanged(nameof(HasUnitsNotice));
        }

        private void RaiseInsert()
        {
            OnPropertyChanged(nameof(CanInsert));
            OnPropertyChanged(nameof(InsertReason));
            OnPropertyChanged(nameof(HasInsertReason));
            OnPropertyChanged(nameof(IsWaiting));
        }
    }
}
