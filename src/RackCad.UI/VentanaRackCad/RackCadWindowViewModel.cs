using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The pages of the center of the RackCad window: the active rack or one of the sections of the side column.</summary>
    public enum RackCadWindowPage
    {
        Rack,
        Nuevo,
        Variables,
        Bom,
        Biblioteca,
        Configuracion,
    }

    /// <summary>One section of the side column as the window shows it: a plain value.</summary>
    public sealed record RackCadWindowSection(RackCadWindowPage Page, string Label, bool IsActive);

    /// <summary>One card of the page "Nuevo": the system, its caption and the classic action that opens its editor.</summary>
    public sealed record RackCadNewSystemCard(RackCadClassicAction Action, string Title, string Caption);

    /// <summary>
    /// The RackCad window (I-64 A-14, phases 1 and 2): ONE window that does not block AutoCAD, with the sections of RackCad and the list of
    /// the racks of the drawing in a side column and the page of the active rack or of the section in the center. It is built over the
    /// workspace panel, whose view-model keeps every rule it had (the browser of racks, the field of the pilot, the navigation, the drafts);
    /// the window only decides what the center shows and adds the doors to the classic flows.
    ///
    /// While the sections are in their intermediate step each one opens its current window through <see cref="IRackCadClassicCommandPort"/>
    /// ("cada sección abre su ventana actual"): the window never writes the drawing by itself. Every entry is contained: a failure is said in
    /// <see cref="ActionMessage"/> and the window keeps working.
    /// </summary>
    public sealed class RackCadWindowViewModel : ObservableObject
    {
        private static readonly IReadOnlyList<(RackCadWindowPage Page, string Label)> SectionOrder = new[]
        {
            (RackCadWindowPage.Nuevo, "Nuevo"),
            (RackCadWindowPage.Variables, "Variables"),
            (RackCadWindowPage.Bom, "BOM y lista de racks"),
            (RackCadWindowPage.Biblioteca, "Biblioteca de diseños"),
            (RackCadWindowPage.Configuracion, "Configuración"),
        };

        private readonly IRackCadClassicCommandPort _classic;
        private readonly IRackCadSettingsAccess _settings;
        private readonly IRackCadRackDoorPort _door;
        private readonly Func<RackCatalog> _catalog;

        // The drafts of the Dinámico racks opened in the window, by session and rack (the id of a rack compares without case, as the list
        // does): they survive a change of rack (I-64 A-14, line D).
        private readonly Dictionary<(SessionId Session, string RackId), DynamicDraftEntry> _dynamicDrafts = new(DraftKeyComparer.Instance);
        private DoorRequest _inFlight;
        private long _lastRequestId;
        private bool _showPanel;

        private sealed class DynamicDraftEntry
        {
            public DynamicDraftEntry(SessionId session, DynamicEditorViewModel editor)
            {
                Session = session;
                Editor = editor;
            }

            public SessionId Session { get; }

            public DynamicEditorViewModel Editor { get; }
        }

        // The one request of the door that waits for AutoCAD: its number, the session and the rack it was made for (at the time it was
        // made, not when the answer arrives) and, for a write, what was sent.
        private sealed record DoorRequest(long Id, bool IsWrite, SessionId Session, string RackId, string SentJson, string SentName);

        private sealed class DraftKeyComparer : IEqualityComparer<(SessionId Session, string RackId)>
        {
            public static readonly DraftKeyComparer Instance = new();

            public bool Equals((SessionId Session, string RackId) x, (SessionId Session, string RackId) y) =>
                x.Session == y.Session && string.Equals(x.RackId, y.RackId, StringComparison.OrdinalIgnoreCase);

            public int GetHashCode((SessionId Session, string RackId) key) =>
                HashCode.Combine(key.Session, StringComparer.OrdinalIgnoreCase.GetHashCode(key.RackId ?? string.Empty));
        }

        private RackCadWindowPage _page = RackCadWindowPage.Rack;
        private string _actionMessage = string.Empty;
        private bool _actionFailed;
        private bool _sideCollapsed;
        private bool _sidePinned = true;
        private SessionId? _lastSelectionSession;
        private string _lastSelectionRack;
        private string _lastActiveRack;
        private IReadOnlyList<RackCadWindowSection> _sections;

        /// <param name="panel">
        /// The view-model of the workspace panel: the page of the rack. The list of the drawing reads through the same summary and edit
        /// ports the panel was built with, so the host hands each port over once.
        /// </param>
        /// <param name="classic">The port of the classic flows; optional: without it the doors say that they are not available.</param>
        /// <param name="settings">The settings of RackCad for "Configuración"; optional: the settings of the machine by default.</param>
        /// <param name="door">The door to one rack (read its design, write its draft) for the editors of the window; optional.</param>
        /// <param name="catalog">The catalog for the editors of the window; optional: the catalog of the plugin by default.</param>
        public RackCadWindowViewModel(
            WorkspacePanelViewModel panel,
            IRackCadClassicCommandPort classic = null,
            IRackCadSettingsAccess settings = null,
            IRackCadRackDoorPort door = null,
            Func<RackCatalog> catalog = null)
        {
            Panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _classic = classic;
            _settings = settings ?? new RackCadSettingsAccess();
            _door = door;
            _catalog = catalog ?? UiSupport.LoadCatalogSafe;
            if (_door != null)
            {
                _door.DesignRead += OnDesignRead;
                _door.DesignWritten += OnDesignWritten;
            }

            RackList = new RackListViewModel(panel.BrowserSummaryPort, panel.PilotEditPort, panel.Browser, DynamicStateOf);
            RackList.RackOpened += () => ShowPage(RackCadWindowPage.Rack);
            // The header counts the racks of the list: it follows every reading of it, also «Volver a leer la lista» (O-1 of the smoke).
            RackList.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RackListViewModel.RackCount) || e.PropertyName == nameof(RackListViewModel.ChangedCount)
                    || e.PropertyName == nameof(RackListViewModel.StatusText))
                    OnPropertyChanged(nameof(HeaderText));
            };
            panel.Browser.PropertyChanged += OnBrowserChanged;
            NewSystems = new[]
            {
                new RackCadNewSystemCard(RackCadClassicAction.NewSelective, "Selectivo", "Racks selectivos y doble profundidad."),
                new RackCadNewSystemCard(RackCadClassicAction.NewPushBack, "Push Back", "Push Back de uno o dos sentidos."),
                new RackCadNewSystemCard(RackCadClassicAction.NewDynamic, "Dinámico", "Racks dinámicos (Pallet Flow)."),
                new RackCadNewSystemCard(RackCadClassicAction.NewCantilever, "Cantilever", "Líneas Cantilever de uno o dos lados."),
                new RackCadNewSystemCard(RackCadClassicAction.NewHeader, "Cabecera", "Configurador de cabeceras (componente)."),
                new RackCadNewSystemCard(RackCadClassicAction.NewFlowBed, "Cama", "Camas de rodamiento: riel, rodillos y frenos."),
                new RackCadNewSystemCard(RackCadClassicAction.NewLarguero, "Larguero", "Larguero (perfil y ménsulas); vista y BOM."),
                new RackCadNewSystemCard(RackCadClassicAction.StructuralSection, "Perfil estructural", "Perfiles W, HSS, C y L del catálogo AISC."),
            };
            RebuildSections();
            Present();
        }

        // ------------------------------------------------------------------ what the window shows

        /// <summary>The workspace panel: the page of the rack and the state of the panel.</summary>
        public WorkspacePanelViewModel Panel { get; }

        /// <summary>The list of the racks of the drawing in the side column.</summary>
        public RackListViewModel RackList { get; }

        public IReadOnlyList<RackCadWindowSection> Sections => _sections;

        public IReadOnlyList<RackCadNewSystemCard> NewSystems { get; }

        public RackCadWindowPage Page => _page;

        public bool IsRackPage => _page == RackCadWindowPage.Rack;

        public bool IsNuevoPage => _page == RackCadWindowPage.Nuevo;

        public bool IsVariablesPage => _page == RackCadWindowPage.Variables;

        public bool IsBomPage => _page == RackCadWindowPage.Bom;

        public bool IsBibliotecaPage => _page == RackCadWindowPage.Biblioteca;

        public bool IsConfiguracionPage => _page == RackCadWindowPage.Configuracion;

        /// <summary>The line of the header: the racks of the drawing and the ones with changes that are not written yet.</summary>
        public string HeaderText
        {
            get
            {
                if (!Panel.PresentedSession.HasValue) return "Sin dibujo abierto";
                var count = RackList.RackCount;
                if (count == 0) return string.IsNullOrEmpty(RackList.StatusText) ? "Sin racks" : string.Empty;
                var racks = count.ToString(CultureInfo.InvariantCulture) + (count == 1 ? " rack" : " racks");
                var changed = RackList.ChangedCount;
                return changed == 0
                    ? racks + " · todo escrito"
                    : racks + " · " + changed.ToString(CultureInfo.InvariantCulture) + " con cambios sin escribir";
            }
        }

        /// <summary>What the last door of the window did (or why it did nothing); empty when there is nothing to say.</summary>
        public string ActionMessage => _actionMessage;

        public bool ActionFailed => _actionFailed;

        public bool HasActionMessage => _actionMessage.Length > 0;

        /// <summary>Whether the side column is hidden, to give the center the whole width.</summary>
        public bool SideCollapsed
        {
            get => _sideCollapsed;
            set
            {
                if (_sideCollapsed == value) return;
                _sideCollapsed = value;
                OnPropertyChanged(nameof(SideCollapsed));
                OnPropertyChanged(nameof(SideToggleText));
            }
        }

        public string SideToggleText => _sideCollapsed ? "Mostrar la lista" : "Ocultar la lista";

        /// <summary>
        /// «Fijar la lista» (the Owner on the prototype: an arrow to hide the list and a pin «para que se oculte sola o se quede
        /// bloqueada»). Pinned, the list stays; not pinned, it hides itself every time a rack or a section is shown, and «Mostrar la lista»
        /// brings it back until the next one.
        /// </summary>
        public bool SidePinned
        {
            get => _sidePinned;
            set
            {
                if (_sidePinned == value) return;
                _sidePinned = value;
                OnPropertyChanged(nameof(SidePinned));
            }
        }

        /// <summary>The text of the page of the rack when no rack is open.</summary>
        public string NoRackText => Panel.Browser.ActiveRackId == null
            ? "Elige un rack en la lista o selecciónalo en el dibujo de AutoCAD."
            : string.Empty;

        // ------------------------------------------------------------------ the Dinámico of the window (I-64 A-14, line D)

        private const string DoorBusyText =
            "AutoCAD todavía no respondió a la petición anterior de la ventana: espera a que termine o pulsa «Dejar de esperar».";

        /// <summary>Whether the active rack is a Dinámico (Pallet Flow), by the Kind of the list of the drawing.</summary>
        public bool IsDynamicRack =>
            string.Equals(RackList.KindOf(Panel.Browser.ActiveRackId), RackEmbedDocument.KindDynamic, StringComparison.OrdinalIgnoreCase);

        /// <summary>The editor of the active Dinámico rack in the window, when it was opened here; null otherwise.</summary>
        public DynamicEditorViewModel ActiveDynamicEditor =>
            Panel.PresentedSession.HasValue && Panel.Browser.ActiveRackId != null
            && _dynamicDrafts.TryGetValue((Panel.PresentedSession.Value, Panel.Browser.ActiveRackId), out var entry)
                ? entry.Editor
                : null;

        public bool HasDynamicEditor => ActiveDynamicEditor != null;

        /// <summary>Every Dinámico editor that the window keeps open, of any drawing.</summary>
        public IReadOnlyList<DynamicEditorViewModel> DynamicEditors => _dynamicDrafts.Values.Select(e => e.Editor).ToList();

        /// <summary>The offer to edit the active Dinámico here: a Dinámico rack with no editor open yet, and a door to read it.</summary>
        public bool ShowDynamicOffer => IsDynamicRack && ActiveDynamicEditor == null && _door != null;

        /// <summary>A request of the door waits for AutoCAD (a read or a write); the window offers «Dejar de esperar».</summary>
        public bool IsWaitingForAutoCad => _inFlight != null;

        public string WaitingText
        {
            get
            {
                if (_inFlight == null) return string.Empty;
                var what = _inFlight.IsWrite ? "Esperando a que AutoCAD escriba el rack." : "Esperando a que AutoCAD lea el rack.";
                return what + " Si no responde (por ejemplo, porque había texto a medio escribir en la línea de órdenes), pulsa «Dejar de esperar».";
            }
        }

        /// <summary>While a Dinámico editor is shown, the user may look at the panel of the rack (state and navigation) instead.</summary>
        public bool ShowPanel
        {
            get => _showPanel || ActiveDynamicEditor == null;
            set
            {
                if (_showPanel == value) return;
                _showPanel = value;
                OnPropertyChanged(nameof(ShowPanel));
                OnPropertyChanged(nameof(PanelToggleText));
            }
        }

        public string PanelToggleText => _showPanel ? "Volver al editor" : "Ver el estado y la navegación del rack";

        /// <summary>
        /// «Editar aquí»: asks AutoCAD to read the design of the active rack; the editor opens when the answer to THIS request arrives, for
        /// the session and the rack of the request. Only one request of the door waits at a time.
        /// </summary>
        public void EditDynamicHere()
        {
            var rack = Panel.Browser.ActiveRackId;
            var session = Panel.PresentedSession;
            if (_door == null || rack == null || !session.HasValue) return;
            if (_inFlight != null)
            {
                Say(DoorBusyText, failed: true);
                return;
            }

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, false, session.Value, rack, null, null);
            Say("Leyendo el rack en AutoCAD…", failed: false);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _door.RequestDesign(new RackDoorRequest(id, session.Value, rack)) ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted) return;
            if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
            RaiseWaiting();
            Say(RefusalText(result), failed: true);
        }

        /// <summary>
        /// «Dejar de esperar»: the window forgets the request that waits for AutoCAD and tells the door, so its command does nothing if it
        /// runs later. An editor that was writing can be edited again, with its draft still not written.
        /// </summary>
        public void StopWaiting()
        {
            var request = _inFlight;
            if (request == null) return;
            _inFlight = null;
            try
            {
                _door?.Abandon(request.Id);
            }
            catch (Exception ex)
            {
                Say("No se pudo avisar a AutoCAD: " + ex.Message, failed: true);
            }

            if (request.IsWrite && _dynamicDrafts.TryGetValue((request.Session, request.RackId), out var entry))
            {
                entry.Editor.EndWriting();
                entry.Editor.Say("Se dejó de esperar a AutoCAD: si la orden llega a correr, no escribirá nada. El borrador sigue sin escribir.", error: false);
            }
            else if (!request.IsWrite)
            {
                Say("Se dejó de esperar la lectura: si la orden llega a correr, no abrirá nada.", failed: false);
            }

            RaiseWaiting();
            RaiseDynamic();
        }

        // The answer of AutoCAD to «Editar aquí»: only the answer to the request that waits; the editor opens for its session and rack.
        private void OnDesignRead(RackDesignRead read)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (read == null || request == null || request.IsWrite || read.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();

                if (!read.Found)
                {
                    Say("No se pudo abrir el rack: " + (read.Reason ?? "sin detalle."), failed: true);
                    return;
                }

                if (!string.Equals(read.RackId, request.RackId, StringComparison.OrdinalIgnoreCase))
                {
                    Say("AutoCAD leyó otro rack que el pedido: no se abrió nada.", failed: true);
                    return;
                }

                var project = new RackProjectStore().Deserialize(read.DesignJson);
                if (project?.DynamicDesign == null)
                {
                    Say("El rack no es un Dinámico que este editor pueda abrir; usa RACKEDITAR.", failed: true);
                    return;
                }

                var key = (request.Session, request.RackId);
                if (_dynamicDrafts.TryGetValue(key, out var existing) && (existing.Editor.IsDirty || existing.Editor.IsWriting))
                {
                    Say("Este rack ya tiene un borrador con cambios en la ventana: no se reemplazó.", failed: false);
                    RaiseDynamic();
                    return;
                }

                if (existing != null) Forget(key, existing);

                var draft = new DynamicDraftEditor(_catalog());
                draft.LoadFrom(project.DynamicDesign, read.Name);
                var editor = new DynamicEditorViewModel(draft, request.RackId, UpdateDynamic);
                editor.PropertyChanged += OnDynamicEditorChanged;
                editor.CloseRequested += CloseDynamicEditor;
                _dynamicDrafts[key] = new DynamicDraftEntry(request.Session, editor);
                _showPanel = false;
                Say(string.Empty, failed: false);
                RaiseDynamic();
            });
        }

        private void OnDynamicEditorChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(DynamicEditorViewModel.IsDirty) && e.PropertyName != nameof(DynamicEditorViewModel.IsWriting)) return;
            RackList.RefreshStates();
            OnPropertyChanged(nameof(HeaderText));
        }

        // «Actualizar» of the Dinámico editor: COPIES of the draft go to AutoCAD, which redraws the rack with the drawing half of RACKEDITAR.
        // The editor cannot be edited until the answer arrives or the window stops waiting, so what is written is what the user saw.
        private void UpdateDynamic(DynamicEditorViewModel editor)
        {
            if (_door == null)
            {
                editor.Say("Aquí no se puede escribir el dibujo.", error: true);
                return;
            }

            var entry = _dynamicDrafts.Values.FirstOrDefault(e => ReferenceEquals(e.Editor, editor));
            if (entry == null) return;
            if (_inFlight != null)
            {
                editor.Say(DoorBusyText, error: true);
                return;
            }

            var snapshot = editor.Draft.SnapshotForWrite();
            if (snapshot == null)
            {
                editor.Say("No hay un borrador válido que escribir.", error: true);
                return;
            }

            // The request waits, and the editor is locked, BEFORE it is handed over: AutoCAD may run the command during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, true, entry.Session, editor.RackId, snapshot.DesignJson, snapshot.Name);
            editor.BeginWriting();
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _door.RequestDynamicUpdate(new RackDoorWrite(id, entry.Session, editor.RackId, snapshot.Name, snapshot.Design, snapshot.System))
                    ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted) return;
            if (_inFlight != null && _inFlight.Id == id)
            {
                _inFlight = null;
                editor.EndWriting();
            }

            RaiseWaiting();
            editor.Say(RefusalText(result), error: true);
        }

        // The answer of AutoCAD to «Actualizar»: only the answer to the request that waits. When AutoCAD redrew the rack, what was SENT is
        // the drawing now and «Descartar» goes back to it; when it redrew nothing, the draft stays not written (I-08). The draft of another
        // session with the same rack is not touched.
        private void OnDesignWritten(RackDesignWritten written)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (written == null || request == null || !request.IsWrite || written.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();

                if (_dynamicDrafts.TryGetValue((request.Session, request.RackId), out var entry))
                {
                    var editor = entry.Editor;
                    editor.EndWriting();
                    if (written.Written) editor.MarkWritten(written.Message, request.SentJson, request.SentName);
                    else editor.Say(written.Message, error: true);
                }

                RackList.RefreshStates();
                OnPropertyChanged(nameof(HeaderText));
            });
        }

        // «Cerrar el editor» (confirmed by the editor when it had changes): the window forgets the draft and shows the panel of the rack.
        private void CloseDynamicEditor(DynamicEditorViewModel editor)
        {
            Contained(() =>
            {
                foreach (var pair in _dynamicDrafts.Where(p => ReferenceEquals(p.Value.Editor, editor)).ToList())
                {
                    if (pair.Value.Editor.IsWriting) return;
                    Forget(pair.Key, pair.Value);
                }

                RaiseDynamic();
                OnPropertyChanged(nameof(HeaderText));
            });
        }

        /// <summary>
        /// What the window still has to write in the drawing of a session: the names of its Dinámico racks with changes not written (or
        /// being written), or null. The close advisor of the window asks it, and a drawing with such work does not close (decision 11 of the
        /// Owner on the prototype: «Que avise antes de cerrar»; D-08). It only reads the memory of the window.
        /// </summary>
        public string UnwrittenDynamicWorkOf(SessionId session)
        {
            var names = _dynamicDrafts
                .Where(p => p.Key.Session == session && (p.Value.Editor.IsDirty || p.Value.Editor.IsWriting))
                .Select(p => string.IsNullOrWhiteSpace(p.Value.Editor.Name) ? p.Value.Editor.RackId : p.Value.Editor.Name.Trim())
                .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            return names.Count == 0 ? null : string.Join(", ", names);
        }

        // The drafts of a drawing that was closed have nowhere to be written: they are forgotten (with their request, if one waited), and
        // work that was lost is said. A drawing with work not written in the window does not close (D-08), so this only happens when
        // AutoCAD closes it anyway (a failure, or the process ending). IsSessionAlive answers from memory; it never reads the drawing.
        private void PruneDeadSessions()
        {
            var port = Panel.BrowserSummaryPort;
            if (port == null) return;

            // A request that waits for a drawing that was closed will never be answered: it is dropped, and the door told (M-23).
            if (_inFlight != null && !IsAlive(port, _inFlight.Session))
            {
                var orphan = _inFlight;
                AbandonInFlight();
                if (!orphan.IsWrite) Say("Se cerró el dibujo del rack que se estaba leyendo: no se abrió nada.", failed: true);
            }

            if (_dynamicDrafts.Count == 0) return;

            var dead = _dynamicDrafts.Where(p => !IsAlive(port, p.Key.Session)).ToList();
            if (dead.Count == 0) return;

            var lost = new List<string>();
            foreach (var pair in dead)
            {
                var editor = pair.Value.Editor;
                if (editor.IsDirty || editor.IsWriting) lost.Add(string.IsNullOrWhiteSpace(editor.Name) ? editor.RackId : editor.Name.Trim());
                if (_inFlight != null && _inFlight.Session == pair.Key.Session) AbandonInFlight();
                Forget(pair.Key, pair.Value);
            }

            if (lost.Count > 0)
                Say("Se cerró el dibujo de " + string.Join(", ", lost) + " con cambios sin escribir en la ventana: esos cambios se perdieron.", failed: true);
            RaiseWaiting();
            RaiseDynamic();
        }

        // Forgets the request that waits and tells the door, so its command does nothing if it ever runs.
        private void AbandonInFlight()
        {
            var request = _inFlight;
            if (request == null) return;
            _inFlight = null;
            try
            {
                _door?.Abandon(request.Id);
            }
            catch (Exception)
            {
                // The door could not be told: the command still checks the drawing the request was kept for.
            }

            RaiseWaiting();
        }

        // A new selection in the drawing of the rack that is already active shows its page too (O-2 of the smoke): a change of the active
        // rack already does it (OnBrowserChanged). Only a new selection in the same drawing counts, so moving between drawings does not.
        private void FollowSelectionOfTheActiveRack()
        {
            var session = Panel.PresentedSession;
            var selected = Panel.Browser.SelectionRackId;
            var isNew = session == _lastSelectionSession && selected != null
                        && !string.Equals(selected, _lastSelectionRack, StringComparison.OrdinalIgnoreCase);
            _lastSelectionSession = session;
            _lastSelectionRack = selected;

            if (isNew && Panel.Browser.FollowSelection
                && string.Equals(selected, Panel.Browser.ActiveRackId, StringComparison.OrdinalIgnoreCase))
                ShowPage(RackCadWindowPage.Rack);
        }

        private static bool IsAlive(IWorkspaceBrowserSummaryPort port, SessionId session)
        {
            try
            {
                return port.IsSessionAlive(session);
            }
            catch (Exception)
            {
                // Unknown is not dead: the draft stays.
                return true;
            }
        }

        private void Forget((SessionId Session, string RackId) key, DynamicDraftEntry entry)
        {
            entry.Editor.PropertyChanged -= OnDynamicEditorChanged;
            entry.Editor.CloseRequested -= CloseDynamicEditor;
            _dynamicDrafts.Remove(key);
        }

        // The state of a Dinámico rack in the list, while its draft is open in the window.
        private (string Text, string Kind, bool Changed)? DynamicStateOf(SessionId session, string rackId)
        {
            if (!_dynamicDrafts.TryGetValue((session, rackId), out var entry)) return null;
            if (entry.Editor.IsWriting) return ("Escribiendo en el dibujo", "Modificado", true);
            return entry.Editor.IsDirty ? ("Modificado (editor de la ventana)", "Modificado", true) : ("Abierto en la ventana", "Limpio", false);
        }

        private void RaiseWaiting()
        {
            OnPropertyChanged(nameof(IsWaitingForAutoCad));
            OnPropertyChanged(nameof(WaitingText));
        }

        private void RaiseDynamic()
        {
            OnPropertyChanged(nameof(IsDynamicRack));
            OnPropertyChanged(nameof(ActiveDynamicEditor));
            OnPropertyChanged(nameof(HasDynamicEditor));
            OnPropertyChanged(nameof(DynamicEditors));
            OnPropertyChanged(nameof(ShowDynamicOffer));
            OnPropertyChanged(nameof(ShowPanel));
            OnPropertyChanged(nameof(PanelToggleText));
            RackList.RefreshStates();
        }

        private static string RefusalText(RackCadClassicResult result)
        {
            switch (result.Status)
            {
                case RackCadClassicStatus.Busy: return "Hay un comando en curso en AutoCAD: termínalo o cancélalo (Esc) y vuelve a intentarlo.";
                case RackCadClassicStatus.NoDocument: return "Abre o crea un dibujo para usar esta acción.";
                default: return "No se pudo: " + (string.IsNullOrWhiteSpace(result.Reason) ? "sin detalle." : result.Reason);
            }
        }

        // "Configuración"
        public string BlockLibraryPath => SafeRead(() => _settings.BlockLibraryPath, string.Empty);

        public string BlockLibraryStatus
        {
            get
            {
                var exists = SafeRead(() => _settings.BlockLibraryExists, false);
                var overridden = SafeRead(() => _settings.BlockLibraryOverridden, false);
                var where = overridden ? "Ruta personalizada (guardada)." : "Ruta predeterminada (junto a los catálogos).";
                return exists
                    ? where + " Biblioteca disponible."
                    : where + " Biblioteca no encontrada: las piezas que no existan en el dibujo activo se omitirán.";
            }
        }

        public bool BlockLibraryMissing => !SafeRead(() => _settings.BlockLibraryExists, false);

        public string DesignLibraryPath => SafeRead(() => _settings.DesignLibraryPath, string.Empty);

        // ------------------------------------------------------------------ what the host and the user tell it

        /// <summary>
        /// The host announced a change of state: the panel pulls its snapshot and the list follows its session. Contained; it never throws.
        /// </summary>
        public void Refresh()
        {
            try
            {
                Panel.Refresh();
            }
            finally
            {
                Present();
            }
        }

        /// <summary>Shows a section, or the page of the rack.</summary>
        public void ShowPage(RackCadWindowPage page)
        {
            if (!_sidePinned) SideCollapsed = true;
            if (_page == page) return;
            _page = page;
            if (page == RackCadWindowPage.Configuracion) RaiseSettings();
            RebuildSections();
            OnPropertyChanged(nameof(Page));
            OnPropertyChanged(nameof(IsRackPage));
            OnPropertyChanged(nameof(IsNuevoPage));
            OnPropertyChanged(nameof(IsVariablesPage));
            OnPropertyChanged(nameof(IsBomPage));
            OnPropertyChanged(nameof(IsBibliotecaPage));
            OnPropertyChanged(nameof(IsConfiguracionPage));
        }

        /// <summary>Opens a rack of the list: the browser makes it active and the center shows its page.</summary>
        public void OpenRack(string rackId) => Contained(() => RackList.Open(rackId));

        /// <summary>
        /// Runs a classic flow (the editor of a new system, Variables, the list, the BOM, the library, the classic menu, the help). The
        /// window says whether AutoCAD took it; it never waits for it and never writes the drawing.
        /// </summary>
        public void Run(RackCadClassicAction action)
        {
            if (_classic == null)
            {
                Say("Esta acción no está disponible aquí.", failed: true);
                return;
            }

            RackCadClassicResult result;
            try
            {
                result = _classic.Run(action) ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            switch (result.Status)
            {
                case RackCadClassicStatus.Started:
                    Say(StartedText(action), failed: false);
                    break;
                case RackCadClassicStatus.Busy:
                    Say("Hay un comando en curso en AutoCAD: termínalo o cancélalo (Esc) y vuelve a intentarlo.", failed: true);
                    break;
                case RackCadClassicStatus.NoDocument:
                    Say("Abre o crea un dibujo para usar esta acción.", failed: true);
                    break;
                default:
                    Say("No se pudo abrir: " + (string.IsNullOrWhiteSpace(result.Reason) ? "sin detalle." : result.Reason), failed: true);
                    break;
            }
        }

        /// <summary>Saves the block library that the user chose in "Configuración".</summary>
        public void SetBlockLibraryPath(string path)
        {
            Contained(() =>
            {
                _settings.SetBlockLibraryPath(path);
                RaiseSettings();
                Say("Biblioteca de bloques guardada.", failed: false);
            });
        }

        /// <summary>Goes back to the default block library, next to the catalogs.</summary>
        public void ResetBlockLibraryPath()
        {
            Contained(() =>
            {
                _settings.SetBlockLibraryPath(null);
                RaiseSettings();
                Say("Biblioteca de bloques restablecida a la ruta predeterminada.", failed: false);
            });
        }

        /// <summary>Removes the message of the last door.</summary>
        public void DismissMessage() => Say(string.Empty, failed: false);

        /// <summary>A failure that the view contained: it is said in the message of the window.</summary>
        public void ShowError(Exception exception)
        {
            var detail = string.IsNullOrWhiteSpace(exception?.Message) ? exception?.GetType().Name : exception.Message;
            Say("La ventana tuvo un fallo contenido: " + detail, failed: true);
        }

        // ------------------------------------------------------------------ internals

        private void Present()
        {
            try
            {
                RackList.Present(Panel.PresentedSession, Panel.PresentedCanRead, Panel.PresentedBlock);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }

            Contained(PruneDeadSessions);
            Contained(FollowSelectionOfTheActiveRack);
            OnPropertyChanged(nameof(HeaderText));
            OnPropertyChanged(nameof(NoRackText));
            OnPropertyChanged(nameof(IsDynamicRack));
            OnPropertyChanged(nameof(ShowDynamicOffer));
        }

        // The browser changed (a tab, the field, the active rack): the marks of the list follow, and a new active rack shows its page.
        private void OnBrowserChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(WorkspaceBrowserViewModel.ActiveRackId) || string.IsNullOrEmpty(e.PropertyName))
                {
                    var active = Panel.Browser.ActiveRackId;
                    var changed = !string.Equals(active, _lastActiveRack, StringComparison.Ordinal);
                    if (active != null && changed) ShowPage(RackCadWindowPage.Rack);
                    _lastActiveRack = active;
                    OnPropertyChanged(nameof(NoRackText));
                    if (changed)
                    {
                        _showPanel = false;
                        RaiseDynamic();
                    }
                }

                if (e.PropertyName == nameof(WorkspaceBrowserViewModel.ActiveRackId)
                    || e.PropertyName == nameof(WorkspaceBrowserViewModel.Tabs)
                    || e.PropertyName == nameof(WorkspaceBrowserViewModel.SelectionRackId)
                    || string.IsNullOrEmpty(e.PropertyName))
                {
                    RackList.RefreshStates();
                    OnPropertyChanged(nameof(HeaderText));
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void RebuildSections()
        {
            var sections = new List<RackCadWindowSection>(SectionOrder.Count);
            foreach (var (page, label) in SectionOrder) sections.Add(new RackCadWindowSection(page, label, page == _page));
            _sections = sections;
            OnPropertyChanged(nameof(Sections));
        }

        private void RaiseSettings()
        {
            OnPropertyChanged(nameof(BlockLibraryPath));
            OnPropertyChanged(nameof(BlockLibraryStatus));
            OnPropertyChanged(nameof(BlockLibraryMissing));
            OnPropertyChanged(nameof(DesignLibraryPath));
        }

        private void Say(string text, bool failed)
        {
            _actionMessage = text ?? string.Empty;
            _actionFailed = failed && _actionMessage.Length > 0;
            OnPropertyChanged(nameof(ActionMessage));
            OnPropertyChanged(nameof(ActionFailed));
            OnPropertyChanged(nameof(HasActionMessage));
        }

        private static string StartedText(RackCadClassicAction action)
        {
            switch (action)
            {
                case RackCadClassicAction.Variables: return "Se pidió a AutoCAD abrir Variables.";
                case RackCadClassicAction.RackList: return "Se pidió a AutoCAD abrir la lista de racks del dibujo.";
                case RackCadClassicAction.TotalBom: return "Se pidió a AutoCAD abrir el BOM total del dibujo.";
                case RackCadClassicAction.DesignLibrary: return "Se pidió a AutoCAD abrir la biblioteca de diseños.";
                case RackCadClassicAction.ClassicMenu: return "Se pidió a AutoCAD abrir el menú clásico de RackCad.";
                case RackCadClassicAction.Help: return "Se pidió a AutoCAD abrir la ayuda de RackCad.";
                case RackCadClassicAction.StructuralSection: return "Se pidió a AutoCAD abrir el generador de perfiles estructurales.";
                default: return "Se pidió a AutoCAD abrir el editor. Al insertar el rack, pulsa «Volver a leer la lista» para verlo en ella.";
            }
        }

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private static T SafeRead<T>(Func<T> read, T fallback)
        {
            try
            {
                return read();
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}
