using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The page «Variables» in the RackCad window (I-64 A-14, cuarta tanda, va). One page per drawing, as the drafts of the racks: it
    /// reads with RACKVARIABLESLEER (no undo marker) when it is first shown, and writes its whole draft with ONE RACKVARIABLESAPLICAR
    /// (VA-02). The one request of the door is shared with the editors (S-16): only one waits at a time, with its number, the drawing it
    /// was made for and «Dejar de esperar». A page with changes is work not written: the drawing does not close (D-08). Without a door
    /// that implements <see cref="IRackCadVariablesDoorPort"/>, the section keeps its intermediate step (RACKVARIABLES).
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private const string VariablesName = "Variables";

        private const string VariablesWriteTakenText =
            "AutoCAD ya está escribiendo las variables. Termina o cancela la orden en AutoCAD: la ventana recibirá su respuesta.";
        private IRackCadVariablesDoorPort _variablesDoor;
        private readonly Dictionary<SessionId, VariablesPageViewModel> _variablesPages = new();

        private void InitializeVariables()
        {
            _variablesDoor = _door as IRackCadVariablesDoorPort;
            if (_variablesDoor == null) return;
            _variablesDoor.VariablesRead += OnVariablesRead;
            _variablesDoor.VariablesWritten += OnVariablesWritten;
        }

        /// <summary>The page «Variables» is edited here: the window has the door of the page.</summary>
        public bool HasVariablesDoor => _variablesDoor != null;

        /// <summary>Without the door of the page, the section is still the intermediate step that opens RACKVARIABLES.</summary>
        public bool ShowVariablesClassicStep => _variablesDoor == null;

        /// <summary>The page «Variables» of the drawing of the window; null without a drawing or without the door of the page.</summary>
        public VariablesPageViewModel VariablesPage =>
            _variablesDoor != null && Panel.PresentedSession.HasValue ? PageFor(Panel.PresentedSession.Value) : null;

        private VariablesPageViewModel PageFor(SessionId session)
        {
            if (_variablesPages.TryGetValue(session, out var page)) return page;
            page = new VariablesPageViewModel(session, racks => VariablesConflictsOf(session, racks));
            page.ReadRequested += ReadVariables;
            page.UpdateRequested += UpdateVariables;
            page.OpenRackRequested += OpenRack;
            page.ClassicRequested += () => Run(RackCadClassicAction.Variables);
            page.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(VariablesPageViewModel.IsDirty) && e.PropertyName != nameof(VariablesPageViewModel.IsWriting)) return;
                OnPropertyChanged(nameof(HeaderText));
            };
            page.SetOtherRequestWaiting(_inFlight != null);
            _variablesPages[session] = page;
            return page;
        }

        // The page is shown: the first time for its drawing it reads it (a command without an undo mark).
        private void VariablesPageShown()
        {
            var page = VariablesPage;
            if (page != null && page.IsNotLoaded && !page.IsReading && _inFlight == null) ReadVariables(page);
        }

        // The window follows the drawing of AutoCAD: the page shown is the one of that drawing.
        private void PresentVariables()
        {
            if (_variablesDoor == null) return;
            OnPropertyChanged(nameof(VariablesPage));
            if (IsVariablesPage) VariablesPageShown();
        }

        // afterOwnWrite (smoke T4, F-2): the read that follows «Actualizar» is asked inside the answer of RACKVARIABLESAPLICAR, while that
        // command still runs in AutoCAD, and the door refuses it as Busy. That refusal is not the user's problem: the page keeps the message
        // of the write, stays not loaded, and reads the drawing when the window presents it again once the command ended.
        private void ReadVariables(VariablesPageViewModel page) => ReadVariables(page, afterOwnWrite: false);

        private void ReadVariables(VariablesPageViewModel page, bool afterOwnWrite)
        {
            Contained(() =>
            {
                if (_variablesDoor == null || page == null) return;
                if (!Panel.PresentedSession.HasValue || page.Session != Panel.PresentedSession.Value)
                {
                    page.Say("Esta página es de otro dibujo: vuelve a ese dibujo para leerla.", true);
                    return;
                }

                if (_inFlight != null)
                {
                    page.Say(DoorBusyText, true);
                    return;
                }

                // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22).
                var id = ++_lastRequestId;
                _inFlight = new DoorRequest(id, false, page.Session, null, null, null, IsVariables: true);
                page.BeginReading();
                RaiseWaiting();

                RackCadClassicResult result;
                try
                {
                    result = _variablesDoor.RequestVariables(new VariablesDoorRequest(id, page.Session))
                             ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
                }
                catch (Exception ex)
                {
                    result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
                }

                if (result.IsStarted) return;
                if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
                RaiseWaiting();
                page.EndReading(afterOwnWrite && result.Status == RackCadClassicStatus.Busy ? null : RefusalText(result));
            });
        }

        private void OnVariablesRead(VariablesDoorRead read)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (read == null || request == null || request.IsWrite || !request.IsVariables || read.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();
                if (_variablesPages.TryGetValue(request.Session, out var page)) page.Load(read, RackList.ListedNameOf);
            });
        }

        // «Actualizar» of the page: the whole draft, with what the page read, in ONE request of the door (VA-02).
        private void UpdateVariables(VariablesPageViewModel page)
        {
            Contained(() =>
            {
                if (_variablesDoor == null || page == null) return;
                if (!Panel.PresentedSession.HasValue || page.Session != Panel.PresentedSession.Value)
                {
                    page.Say("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", true);
                    return;
                }

                if (_inFlight != null)
                {
                    page.Say(DoorBusyText, true);
                    return;
                }

                var id = ++_lastRequestId;
                var write = page.BuildWrite(id);
                if (write == null)
                {
                    page.Say("No hay un borrador que escribir.", true);
                    return;
                }

                _inFlight = new DoorRequest(id, true, page.Session, null, null, null, IsVariables: true);
                page.BeginWriting();
                RaiseWaiting();

                RackCadClassicResult result;
                try
                {
                    result = _variablesDoor.RequestVariablesUpdate(write) ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
                }
                catch (Exception ex)
                {
                    result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
                }

                if (result.IsStarted) return;
                if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
                RaiseWaiting();
                page.EndWriting(new VariablesDoorWritten(id, false, RefusalText(result)));
            });
        }

        // The answer to «Actualizar»: written, the clean editors of the racks it redrew are closed (they hold what was read before) and
        // the page reads the drawing again; not written, the draft stays with the reason.
        private void OnVariablesWritten(VariablesDoorWritten written)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (written == null || request == null || !request.IsWrite || !request.IsVariables || written.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();
                if (!_variablesPages.TryGetValue(request.Session, out var page)) return;
                page.EndWriting(written);
                if (!written.Written) return;
                VariablesWrittenFor(request.Session, written); // T6 at: shared with «Actualizar todo»
            });
        }

        // What follows a write of the page (its own «Actualizar», or «Actualizar todo», T6 at): the clean editors of the racks it redrew are
        // closed and the page reads the drawing again (a read asked inside the answer of the write: the door may say Busy, smoke T4 F-2).
        private void VariablesWrittenFor(SessionId session, VariablesDoorWritten written)
        {
            Contained(() =>
            {
                if (written == null || !written.Written || !_variablesPages.TryGetValue(session, out var page)) return;
                var closed = new List<string>();
                foreach (var rack in written.RedrawnRackIds ?? Array.Empty<string>())
                {
                    var key = (session, rack);
                    if (_selectiveDrafts.TryGetValue(key, out var entry) && !entry.Editor.IsDirty && !entry.Editor.IsWriting)
                    {
                        closed.Add(SelectiveNameOf(entry));
                        ForgetSelective(key, entry);
                    }
                }

                if (closed.Count > 0)
                {
                    page.Say(page.Message + " Se cerró el editor de " + string.Join(", ", closed)
                             + ", que mostraba lo que había antes: vuelve a abrirlo con «Editar aquí».", false);
                    RaiseDynamic();
                }

                RackList.RefreshStates();
                OnPropertyChanged(nameof(HeaderText));
                ReadVariables(page, afterOwnWrite: true);
            });
        }

        // VA-04: the racks, among the ones the page would redraw, that have an unwritten draft in the window (Selectivo: the only one
        // with linked properties).
        private IReadOnlyList<(string RackId, string Name)> VariablesConflictsOf(SessionId session, IReadOnlyList<string> racks)
        {
            var conflicts = new List<(string RackId, string Name)>();
            foreach (var rack in racks ?? Array.Empty<string>())
            {
                if (_selectiveDrafts.TryGetValue((session, rack), out var entry) && (entry.Editor.IsDirty || entry.Editor.IsWriting))
                    conflicts.Add((rack, SelectiveNameOf(entry)));
            }

            return conflicts;
        }

        private void RefreshVariablesConflicts()
        {
            foreach (var page in _variablesPages.Values) page.RefreshConflicts();
        }

        // D-08: the page with changes (or writing) is work not written of its drawing, named «Variables».
        private IEnumerable<string> UnwrittenVariablesNames(SessionId session)
            => _variablesPages.TryGetValue(session, out var page) && (page.IsDirty || page.IsWriting) ? new[] { VariablesName } : Array.Empty<string>();

        // The page of a drawing that was closed has nowhere to be written: it is forgotten, and a lost draft is said.
        private void PruneVariables(IWorkspaceBrowserSummaryPort port)
        {
            var dead = _variablesPages.Where(p => !IsAlive(port, p.Key)).ToList();
            if (dead.Count == 0) return;
            var lost = dead.Any(p => p.Value.IsDirty || p.Value.IsWriting);
            foreach (var pair in dead) _variablesPages.Remove(pair.Key);
            if (lost) LostOnPrune("«" + VariablesName + "»"); // T5 cm: said once, with what the other prunes lost (RackCadWindowViewModel.Prune.cs)
            OnPropertyChanged(nameof(VariablesPage));
        }

        // «Dejar de esperar» of a request of the page.
        private void StopWaitingVariables(DoorRequest request)
        {
            if (!_variablesPages.TryGetValue(request.Session, out var page)) return;
            if (request.IsWrite) page.StoppedWaiting();
            else page.EndReading("Se dejó de esperar la lectura: si la orden llega a correr, no mostrará nada. Vuelve a leer la página.");
        }

        private void VariablesWriteTaken(DoorRequest request)
        {
            if (_variablesPages.TryGetValue(request.Session, out var page)) page.WriteTaken();
        }

        // One request at a time: every page knows whether another request of the window waits (its «Actualizar» says why it cannot).
        private void RaiseVariablesWaiting()
        {
            foreach (var page in _variablesPages.Values)
                page.SetOtherRequestWaiting(_inFlight != null && !(_inFlight.IsVariables && _inFlight.Session == page.Session));
        }
    }
}
