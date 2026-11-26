using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T4 cb (I-64, fourth batch; prototype «cabecera», CB-01 A): a cabecera of its own (a rack «Cabecera N» of the list) edited in the window,
    /// with its numbered door, as Dinámico and Selectivo: «Editar aquí» asks AutoCAD to read it (RACKCABECERAABRIR, no undo marker), the
    /// editor of cabeceras opens on the answer to THAT request, and «Actualizar» hands a COPY to RACKCABECERAAPLICAR, which redraws its views
    /// in place. The one request in flight, «Dejar de esperar», the request the command already took, a draft of another drawing, the prune
    /// of a closed drawing and the veto of the close (D-08) are the rules of the window. «Nuevo › Cabecera en esta ventana» waits for the
    /// drafts of the part «new»; «Insertar vista…» of a cabecera waits for the panel of the part «ins» (DG-07).
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private IRackCadCabeceraDoorPort _cabeceraDoor;

        // The cabeceras of their own opened in the window, with the key and the life of the other drafts.
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

        /// <summary>The editor of the active cabecera in the window, when it was opened here; null otherwise.</summary>
        public CabeceraEditorViewModel ActiveCabeceraEditor =>
            Panel.PresentedSession.HasValue && Panel.Browser.ActiveRackId != null
            && _cabeceraDrafts.TryGetValue((Panel.PresentedSession.Value, Panel.Browser.ActiveRackId), out var entry)
                ? entry.Editor
                : null;

        public bool HasCabeceraEditor => ActiveCabeceraEditor != null;

        /// <summary>Every cabecera editor that the window keeps open, of any drawing.</summary>
        public IReadOnlyList<CabeceraEditorViewModel> CabeceraEditors => _cabeceraDrafts.Values.Select(e => e.Editor).ToList();

        /// <summary>«Editar aquí» of a cabecera: a cabecera with no editor open yet, and a door that reads and writes a cabecera.</summary>
        public bool ShowCabeceraOffer => IsCabeceraRack && ActiveCabeceraEditor == null && _cabeceraDoor != null;

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
            _inFlight = new DoorRequest(id, false, session.Value, rack, null, null, IsCabecera: true);
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
                if (_cabeceraDrafts.TryGetValue(key, out var existing) && (existing.Editor.IsDirty || existing.Editor.IsWriting))
                {
                    Say("Esta cabecera ya tiene un borrador con cambios en la ventana: no se reemplazó.", failed: false);
                    RaiseCabecera();
                    return;
                }

                if (existing != null) ForgetCabecera(key, existing);
                var draft = new CabeceraDraftEditor(header, _catalog(), CabeceraTemplates.Load(), CabeceraDraftContext.Standalone);
                var editor = CabeceraEditorViewModel.Standalone(draft, request.RackId, UpdateCabecera);
                editor.PropertyChanged += OnCabeceraEditorChanged;
                editor.CloseRequested += CloseCabeceraEditor;
                _cabeceraDrafts[key] = new CabeceraDraftEntry(request.Session, editor, RackList.ListedNameOf(request.RackId));
                _showPanel = false;
                Say(string.Empty, failed: false);
                RaiseDynamic();
            });
        }

        private void OnCabeceraEditorChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CabeceraEditorViewModel.IsDirty) && e.PropertyName != nameof(CabeceraEditorViewModel.IsWriting)) return;
            RackList.RefreshStates();
            OnPropertyChanged(nameof(HeaderText));
        }

        // «Actualizar» of a cabecera of its own: a COPY of the cabecera goes to AutoCAD (RACKCABECERAAPLICAR); the editor is locked until the
        // answer arrives or the window stops waiting.
        private void UpdateCabecera(CabeceraEditorViewModel editor)
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
                editor.MarkWritten(false, "Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.");
                return;
            }

            if (_inFlight != null)
            {
                editor.MarkWritten(false, DoorBusyText);
                return;
            }

            var copy = editor.Editor.Snapshot();
            // Correction 7 of the review: the inner name goes as it is (blank too); the command names the blocks as RACKEDITAR does, with
            // the name of the envelope when the inner one is blank. What is sent is the base of «Limpio» once it is written.
            var name = copy.Name;
            var sent = new RackCad.Application.Persistence.RackFrameProjectStore().Serialize(copy);
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, true, entry.Session, editor.RackId, sent, name, IsCabecera: true);
            editor.SetWriting(true);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _cabeceraDoor.RequestCabeceraUpdate(new CabeceraDoorWrite(id, entry.Session, editor.RackId, name, copy))
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted) return;
            if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
            RaiseWaiting();
            editor.MarkWritten(false, RefusalText(result));
        }

        // The answer to the write of a cabecera: written = «Limpio» on what was sent; else it stays «Modificado» with the reason.
        private void CabeceraWritten(DoorRequest request, RackDesignWritten written)
        {
            if (_cabeceraDrafts.TryGetValue((request.Session, request.RackId), out var entry)) entry.Editor.MarkWritten(written.Written, written.Message, request.SentJson);
        }

        // «Dejar de esperar» of a write of a cabecera: the editor can be edited again, with its draft still not written.
        private void StopCabeceraWrite(DoorRequest request, string text)
        {
            if (_cabeceraDrafts.TryGetValue((request.Session, request.RackId), out var entry)) entry.Editor.MarkWritten(false, text);
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

        private static string CabeceraNameOf(CabeceraDraftEntry entry)
            => !string.IsNullOrWhiteSpace(entry.Editor.Name) ? entry.Editor.Name.Trim() : entry.ListedName ?? entry.Editor.RackId;

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

            if (lost.Count > 0)
                Say("Se cerró el dibujo de " + string.Join(", ", lost) + " con cambios sin escribir en la ventana: esos cambios se perdieron.", failed: true);
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

        private void RaiseCabecera()
        {
            OnPropertyChanged(nameof(IsCabeceraRack));
            OnPropertyChanged(nameof(ActiveCabeceraEditor));
            OnPropertyChanged(nameof(HasCabeceraEditor));
            OnPropertyChanged(nameof(CabeceraEditors));
            OnPropertyChanged(nameof(ShowCabeceraOffer));
        }
    }
}
