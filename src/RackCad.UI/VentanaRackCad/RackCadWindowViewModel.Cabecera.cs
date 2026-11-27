using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.RackFrames;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T4 cb (I-64, fourth batch; prototype «cabecera», CB-01 A): a cabecera of its own (a rack «Cabecera N» of the list) edited in the window,
    /// with its numbered door, as Dinámico and Selectivo: «Editar aquí» asks AutoCAD to read it (RACKCABECERAABRIR, no undo marker), the
    /// editor of cabeceras opens on the answer to THAT request, and «Actualizar» hands a COPY to RACKCABECERAAPLICAR, which redraws its views
    /// in place. The one request in flight, «Dejar de esperar», the request the command already took, a draft of another drawing, the prune
    /// of a closed drawing and the veto of the close (D-08) are the rules of the window.
    /// <para>T7 cab (I-64, séptima tanda; the Owner on 2026-11-27: «Las dos»), what was missing (DG-07):</para>
    /// <list type="bullet">
    /// <item>«Nuevo» › «Cabecera en esta ventana» is a draft («Borrador N», SD-06) by the one entry point of the drafts
    /// (<see cref="OpenAsNewDraft"/>), with the defaults of the classic configurator, inserted with RACKCABECERANUEVO (one «U»);</item>
    /// <item>«Insertar vista…» of a cabecera of the drawing is RACKCABECERAINSERTAR (the cabecera in the views that exist, then the new ones,
    /// B-1 and the rule of the incomplete redraw);</item>
    /// <item>the tab «Propiedades» (VA-07), read with «Editar aquí» and written with «Actualizar» and, for a new one, at its insertion (VA-S9);</item>
    /// <item>RACKCABECERA and RCB open «Cabecera en esta ventana» (<see cref="IRackCadShortcutPort"/>, the pattern of rl).</item>
    /// </list>
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private IRackCadCabeceraDoorPort _cabeceraDoor;

        // The cabeceras of their own opened in the window (and, T7 cab, the new ones of «Nuevo»), with the key and the life of the other drafts.
        private readonly Dictionary<(SessionId Session, string RackId), CabeceraDraftEntry> _cabeceraDrafts = new(DraftKeyComparer.Instance);

        private sealed class CabeceraDraftEntry
        {
            public CabeceraDraftEntry(SessionId session, CabeceraEditorViewModel editor, string listedName)
            {
                Session = session;
                Editor = editor;
                ListedName = string.IsNullOrWhiteSpace(listedName) ? null : listedName.Trim();
            }

            public SessionId Session { get; }

            public CabeceraEditorViewModel Editor { get; }

            public string ListedName { get; }
        }

        // One line of the constructor: a cabecera is edited here only through a door that reads and writes it.
        private void WireCabeceraDoor(IRackCadRackDoorPort door)
        {
            _cabeceraDoor = door as IRackCadCabeceraDoorPort;
            if (_cabeceraDoor != null) _cabeceraDoor.CabeceraRead += OnCabeceraRead;
        }

        /// <summary>Whether the active rack is a cabecera of its own, by the Kind of the list of the drawing.</summary>
        public bool IsCabeceraRack =>
            string.Equals(RackList.KindOf(Panel.Browser.ActiveRackId), RackEmbedDocument.KindCabecera, StringComparison.OrdinalIgnoreCase);

        /// <summary>The editor of the active cabecera in the window, when it was opened here; null otherwise (and for a new one of «Nuevo»).</summary>
        public CabeceraEditorViewModel ActiveCabeceraEditor =>
            Panel.PresentedSession.HasValue && Panel.Browser.ActiveRackId != null
            && _cabeceraDrafts.TryGetValue((Panel.PresentedSession.Value, Panel.Browser.ActiveRackId), out var entry)
            && !entry.Editor.IsNewRack // T7 cab
                ? entry.Editor
                : null;

        public bool HasCabeceraEditor => ActiveCabeceraEditor != null;

        /// <summary>Every cabecera editor that the window keeps open, of any drawing.</summary>
        public IReadOnlyList<CabeceraEditorViewModel> CabeceraEditors => _cabeceraDrafts.Values.Select(e => e.Editor).ToList();

        /// <summary>«Editar aquí» of a cabecera: a cabecera with no editor open yet, and a door that reads and writes a cabecera.</summary>
        public bool ShowCabeceraOffer => IsCabeceraRack && ActiveCabeceraEditor == null && _cabeceraDoor != null;

        // ------------------------------------------------------------------ T7 cab: «Nuevo» › «Cabecera en esta ventana»

        /// <summary>The new cabecera that «Nuevo» shows: the editor of the active draft when it is a cabecera; null otherwise.</summary>
        public CabeceraEditorViewModel NewCabeceraEditor => ActiveDraftEditor as CabeceraEditorViewModel;

        public bool ShowNewCabeceraEditor => IsNuevoPage && NewCabeceraEditor != null;

        /// <summary>A new cabecera can be made here: the window has the door of the cabecera and a drawing.</summary>
        public bool CanStartNewCabecera => _cabeceraDoor != null && Panel.PresentedSession.HasValue;

        /// <summary>«Cabecera en esta ventana» of «Nuevo» (and RACKCABECERA): a new draft («Borrador N») with the defaults of the classic configurator.</summary>
        public RackCadDraftOpening StartNewCabecera() => OpenAsNewDraft(RackCadDraftSeed.Defaults(RackCad.Domain.Systems.Shared.RackSystemKind.Selective));

        // The new draft of «Nuevo» of a cabecera (a case of CreateDraftEditor): the defaults of RACKCABECERA, or a design (the copy of «Duplicar»).
        private CabeceraEditorViewModel CreateCabeceraDraft(RackCadDraftSeed seed, SessionId session, string rackId, string label, out string failure)
        {
            failure = null;
            var store = new RackFrameProjectStore();
            RackFrameConfiguration opened;
            if (seed.Project == null)
            {
                // T7cab-S1, S2: the template of RACKCABECERA (HardcodedStandardRackFrameService, the standard «STD-3P» with its post, height and
                // depth), with no name: at the insertion it takes «Cabecera N» (I-60), as the other drafts. The classic configurator shows the
                // name of the template, which I-60 also takes as no name.
                opened = new HardcodedStandardRackFrameService(_catalog()).CreateDefault();
                opened.Name = string.Empty;
            }
            else
            {
                opened = seed.Project.Header == null ? null : store.DeepCopy(seed.Project.Header);
                if (opened == null)
                {
                    failure = "El diseño no tiene una cabecera que abrir.";
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(seed.Name)) opened.Name = seed.Name.Trim();
            }

            CabeceraDraftEditor draft;
            try
            {
                draft = new CabeceraDraftEditor(opened, _catalog(), CabeceraTemplates.Load(), CabeceraDraftContext.Standalone);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                failure = "No se pudo abrir la cabecera como borrador: " + ex.Message;
                return null;
            }

            var editor = CabeceraEditorViewModel.ForNewRack(draft, rackId, UpdateCabecera);
            if (seed.Edit != null)
            {
                // «Duplicar»: the frame of the original as an edit; the copy keeps its own name (none), as the copies of the other systems.
                var edited = seed.Edit.Header == null ? null : store.DeepCopy(seed.Edit.Header);
                if (edited != null) edited.Name = opened.Name;
                if (edited == null || !editor.TakeDesignAsEdit(edited))
                {
                    failure = "No se pudo copiar el diseño.";
                    return null;
                }
            }

            WireCabecera(editor);
            _cabeceraDrafts[(session, rackId)] = new CabeceraDraftEntry(session, editor, null);
            if (seed.Project != null) editor.SayOpened(OpenedText(label, seed, null, null));
            return editor;
        }

        // A draft of «Nuevo» of a cabecera is live while its editor is a new rack in the dictionary.
        private bool IsLiveCabecera(SessionId session, string rackId, CabeceraEditorViewModel editor)
            => editor.IsNewRack && _cabeceraDrafts.TryGetValue((session, rackId), out var entry) && ReferenceEquals(entry.Editor, editor);

        // For «Duplicar»: the frame of a draft as a project, and the base of its «Modificado» (what its «Descartar» goes back to).
        private static (RackProject Base, RackProject Copied) CopyOfCabecera(CabeceraEditorViewModel editor)
        {
            var store = new RackFrameProjectStore();
            var baseline = string.IsNullOrWhiteSpace(editor.Editor.BaselineJson) ? null : store.Deserialize(editor.Editor.BaselineJson);
            var copy = editor.Editor.Snapshot();
            return (baseline == null ? null : RackProject.ForSelective(baseline), copy == null ? null : RackProject.ForSelective(copy));
        }

        // ------------------------------------------------------------------ the editors of the window

        // What every cabecera editor of its own gets: the writes, the panel «Insertar vista…» and the close.
        private void WireCabecera(CabeceraEditorViewModel editor)
        {
            editor.PropertyChanged += OnCabeceraEditorChanged;
            editor.CloseRequested += CloseCabeceraEditor;
            editor.ExistingViewKinds = () => RackList.ViewKindsOf(editor.RackId); // T7 cab: DG-04
            editor.OtherRequestText = OtherRequestText;
            editor.OtherDrawingText = () => OtherDrawingText(_cabeceraDrafts.Values.FirstOrDefault(e => ReferenceEquals(e.Editor, editor))?.Session);
            editor.InsertViewsAction = InsertCabeceraViews;
            editor.InsertNewAction = InsertNewCabecera;
        }

        /// <summary>«Editar aquí» of a cabecera: asks AutoCAD to read it (RACKCABECERAABRIR); the one request of the door is shared.</summary>
        public void EditCabeceraHere()
        {
            var rack = Panel.Browser.ActiveRackId;
            var session = Panel.PresentedSession;
            if (_cabeceraDoor == null || rack == null || !session.HasValue) return;
            if (_inFlight != null)
            {
                Say(DoorBusyText, failed: true);
                return;
            }

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, false, session.Value, rack, null, null, IsCabecera: true, IsReread: _rereading); // T6 st
            Say("Leyendo la cabecera en AutoCAD…", failed: false);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _cabeceraDoor.RequestCabeceraDesign(new RackDoorRequest(id, session.Value, rack))
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
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

        // The answer of AutoCAD to «Editar aquí» of a cabecera: only the answer to the cabecera read that waits.
        private void OnCabeceraRead(CabeceraDesignRead read)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (read == null || request == null || request.IsWrite || !request.IsCabecera || read.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();

                if (!read.Opens)
                {
                    Say("No se pudo abrir la cabecera: " + (read.Reason ?? "sin detalle."), failed: true);
                    return;
                }

                if (!string.Equals(read.RackId, request.RackId, StringComparison.OrdinalIgnoreCase))
                {
                    Say("AutoCAD leyó otro rack que el pedido: no se abrió nada.", failed: true);
                    return;
                }

                var header = read.Header();
                if (header == null)
                {
                    Say("La cabecera no se puede abrir en este editor; usa RACKEDITAR.", failed: true);
                    return;
                }

                var key = (request.Session, request.RackId);
                if (_cabeceraDrafts.TryGetValue(key, out var existing) && ((existing.Editor.IsDirty && !request.IsReread) || existing.Editor.IsWriting)) // T6 st
                {
                    Say("Esta cabecera ya tiene un borrador con cambios en la ventana: no se reemplazó.", failed: false);
                    RaiseCabecera();
                    return;
                }

                if (existing != null) ForgetCabecera(key, existing);
                var draft = new CabeceraDraftEditor(header, _catalog(), CabeceraTemplates.Load(), CabeceraDraftContext.Standalone);
                var editor = CabeceraEditorViewModel.Standalone(draft, request.RackId, UpdateCabecera);
                editor.LoadProperties(read.Properties); // T7 cab: the tab «Propiedades» (VA-07)
                WireCabecera(editor); // T7 cab: and «Insertar vista…»
                _cabeceraDrafts[key] = new CabeceraDraftEntry(request.Session, editor, RackList.ListedNameOf(request.RackId));
                _showPanel = false;
                Say(string.Empty, failed: false);
                RaiseDynamic();
            });
        }

        private void OnCabeceraEditorChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshDraftOf(sender, e.PropertyName); // T7 cab: the item of a draft follows its name and its state
            if (e.PropertyName != nameof(CabeceraEditorViewModel.IsDirty) && e.PropertyName != nameof(CabeceraEditorViewModel.IsWriting)) return;
            RackList.RefreshStates();
            OnPropertyChanged(nameof(HeaderText));
        }

        private void UpdateCabecera(CabeceraEditorViewModel editor) => WriteCabecera(editor, null);

        // T7 cab: «Insertar vista…» of a cabecera of the drawing: the same write, with the views to insert after it.
        private void InsertCabeceraViews(CabeceraEditorViewModel editor, IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0) return;
            WriteCabecera(editor, views);
        }

        // T7 cab: «Insertar en el dibujo…» of a new cabecera: the same write, as the insertion of a new rack with those views.
        private void InsertNewCabecera(CabeceraEditorViewModel editor, IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0 || !editor.IsNewRack) return;
            WriteCabecera(editor, views);
        }

        // «Actualizar», «Insertar vista…» and «Insertar en el dibujo…» of a cabecera of its own: a COPY of the cabecera goes to AutoCAD
        // (RACKCABECERAAPLICAR, RACKCABECERAINSERTAR, RACKCABECERANUEVO); the editor is locked until the answer arrives or the window stops
        // waiting.
        private void WriteCabecera(CabeceraEditorViewModel editor, IReadOnlyList<RackViewAddress> insertViews)
        {
            if (_cabeceraDoor == null)
            {
                editor.MarkWritten(false, "Aquí no se puede escribir el dibujo.");
                return;
            }

            var entry = _cabeceraDrafts.Values.FirstOrDefault(e => ReferenceEquals(e.Editor, editor));
            if (entry == null) return;
            if (!Panel.PresentedSession.HasValue || entry.Session != Panel.PresentedSession.Value)
            {
                editor.MarkWritten(false, OtherDrawingWriteText);
                return;
            }

            if (_inFlight != null)
            {
                editor.MarkWritten(false, DoorBusyText);
                return;
            }

            var copy = editor.Editor.Snapshot();
            // Correction 7 of the review: the inner name goes as it is (blank too); the command names the blocks as RACKEDITAR does, with
            // the name of the envelope when the inner one is blank (a new cabecera takes «Cabecera N», I-60). What is sent is the base of
            // «Limpio» once it is written.
            var name = copy.Name;
            var sent = new RackFrameProjectStore().Serialize(copy);
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, true, entry.Session, editor.RackId, sent, name, IsCabecera: true);
            editor.SetWriting(true);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _cabeceraDoor.RequestCabeceraUpdate(new CabeceraDoorWrite(id, entry.Session, editor.RackId, name, copy,
                             insertViews == null ? null : new List<RackViewAddress>(insertViews), NewRack: editor.IsNewRack, // T7 cab
                             Properties: insertViews == null || editor.IsNewRack ? editor.PropertiesWrite() : null)) // T7 cab: VA-07, VA-S9 (not «Insertar vista…», VA-S10)
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted)
            {
                editor.WriteHandedOver(); // T7 cab: the panel of «Insertar vista…» does not come back
                return;
            }

            if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
            RaiseWaiting();
            editor.MarkWritten(false, RefusalText(result));
        }

        // The answer to the write of a cabecera: written = «Limpio» on what was sent; else it stays «Modificado» with the reason. T7 cab: the
        // tab «Propiedades» follows, and a new cabecera that AutoCAD placed is a cabecera of the drawing now.
        private void CabeceraWritten(DoorRequest request, RackDesignWritten written)
        {
            if (!_cabeceraDrafts.TryGetValue((request.Session, request.RackId), out var entry)) return;
            var editor = entry.Editor;
            editor.WriteHandedOver();
            editor.MarkWritten(written.Written, written.Message, request.SentJson);
            if (!written.Written) EchoHiddenDraft(editor, written.Message, failed: true);
            editor.PropertiesWritten(written);

            if (written.Written && editor.IsNewRack)
            {
                editor.MarkInserted(written.Name);
                Say(written.Message, failed: false);
                RaiseDynamic();
            }
        }

        // «Dejar de esperar» of a write of a cabecera: the editor can be edited again, with its draft still not written.
        private void StopCabeceraWrite(DoorRequest request, string text)
        {
            if (!_cabeceraDrafts.TryGetValue((request.Session, request.RackId), out var entry)) return;
            entry.Editor.MarkWritten(false, text);
            EchoHiddenDraft(entry.Editor, text, failed: false); // T7 cab: a draft not on screen says it in the window too
        }

        private void CabeceraWriteTaken(DoorRequest request)
        {
            if (_cabeceraDrafts.TryGetValue((request.Session, request.RackId), out var entry)) entry.Editor.WriteTaken();
        }

        private void CloseCabeceraEditor(CabeceraEditorViewModel editor)
        {
            Contained(() =>
            {
                foreach (var pair in _cabeceraDrafts.Where(p => ReferenceEquals(p.Value.Editor, editor)).ToList())
                {
                    if (pair.Value.Editor.IsWriting) return;
                    ForgetCabecera(pair.Key, pair.Value);
                }

                RaiseDynamic();
                OnPropertyChanged(nameof(HeaderText));
            });
        }

        private void ForgetCabecera((SessionId Session, string RackId) key, CabeceraDraftEntry entry)
        {
            entry.Editor.PropertyChanged -= OnCabeceraEditorChanged;
            entry.Editor.CloseRequested -= CloseCabeceraEditor;
            _cabeceraDrafts.Remove(key);
        }

        // The name of a cabecera in the notices; T7 cab: a draft of «Nuevo» by its own name, «Borrador N (Cabecera nueva)».
        private string CabeceraNameOf(CabeceraDraftEntry entry) =>
            DraftNoticeName(entry.Editor) is string draft ? draft
            : !string.IsNullOrWhiteSpace(entry.Editor.Name) ? entry.Editor.Name.Trim()
            : entry.Editor.IsNewRack ? "una cabecera nueva"
            : entry.ListedName ?? entry.Editor.RackId;

        // D-08: the cabeceras of the session with changes not written (or being written).
        private IEnumerable<string> UnwrittenCabeceraNames(SessionId session)
            => _cabeceraDrafts.Where(p => p.Key.Session == session && (p.Value.Editor.IsDirty || p.Value.Editor.IsWriting)).Select(p => CabeceraNameOf(p.Value));

        // The cabeceras of a drawing that was closed are forgotten, and work that was lost is said.
        private void PruneCabeceraDrafts(IWorkspaceBrowserSummaryPort port)
        {
            var dead = _cabeceraDrafts.Where(p => !IsAlive(port, p.Key.Session)).ToList();
            if (dead.Count == 0) return;
            var lost = new List<string>();
            foreach (var pair in dead)
            {
                if (pair.Value.Editor.IsDirty || pair.Value.Editor.IsWriting) lost.Add(CabeceraNameOf(pair.Value));
                if (_inFlight != null && _inFlight.Session == pair.Key.Session) AbandonInFlight();
                ForgetCabecera(pair.Key, pair.Value);
            }

            lost.ForEach(LostOnPrune); // T5 cm: said once, with what the other prunes lost (RackCadWindowViewModel.Prune.cs)
            RaiseWaiting();
            RaiseCabecera();
        }

        // The state of a cabecera in the list while its draft is open in the window (the marks of the other drafts).
        private bool CabeceraDraftState(SessionId session, string rackId, out bool writing, out bool dirty)
        {
            writing = dirty = false;
            if (!_cabeceraDrafts.TryGetValue((session, rackId), out var entry)) return false;
            writing = entry.Editor.IsWriting;
            dirty = entry.Editor.IsDirty;
            return true;
        }

        // T7 cab: the open panels «Insertar vista…» of the cabecera editors follow the door (one line of RefreshInsertPanels).
        private void RefreshCabeceraInsertPanels()
        {
            foreach (var entry in _cabeceraDrafts.Values) entry.Editor.RefreshInsertState();
        }

        private void RaiseCabecera()
        {
            OnPropertyChanged(nameof(IsCabeceraRack));
            OnPropertyChanged(nameof(ActiveCabeceraEditor));
            OnPropertyChanged(nameof(HasCabeceraEditor));
            OnPropertyChanged(nameof(CabeceraEditors));
            OnPropertyChanged(nameof(ShowCabeceraOffer));
            RaiseNewCabecera(); // T7 cab
        }

        // T7 cab: one line of RaiseNewDynamic: the new cabecera of «Nuevo» follows the drafts and the page.
        private void RaiseNewCabecera()
        {
            OnPropertyChanged(nameof(NewCabeceraEditor));
            OnPropertyChanged(nameof(ShowNewCabeceraEditor));
            OnPropertyChanged(nameof(CanStartNewCabecera));
        }
    }
}
