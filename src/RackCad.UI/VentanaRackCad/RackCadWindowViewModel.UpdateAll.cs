using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One line of the list of «Actualizar todo» when it ends: the draft, its system, how it ended and why.</summary>
    /// <remarks>Review of c4985a2e, minor 6: the position makes the name of every line unique, also for two racks with the same name.</remarks>
    public sealed record UpdateAllResultRow(int Position, string Name, string SystemLabel, string OutcomeText, string Cause, bool Failed)
    {
        public string AccessibleName => Position.ToString(System.Globalization.CultureInfo.InvariantCulture) + ". " + Name + " (" + SystemLabel + "): "
                                        + OutcomeText + (string.IsNullOrWhiteSpace(Cause) ? string.Empty : ". " + Cause);
    }

    /// <summary>
    /// T6 at (I-64, sexta tanda): «Actualizar todo» of the RackCad window (the decisions of the Owner of 2026-11-23; the A-14, C-05). Outside
    /// the racks, in the header: it writes in ONE request of the door (RACKACTUALIZARTODO) every draft «Modificado» of the drawing of the
    /// window — the six editors and the page «Variables» — with exactly the write its own «Actualizar» would send. It is all or nothing:
    /// the window says which draft failed and why, and every other one «No aplicado: falló otro rack del conjunto». The one request of the
    /// door is the rule of the window: no other «Actualizar» while it lasts, «Dejar de esperar» until the command takes it. When it ends,
    /// every draft is left as after its own «Actualizar»: «Limpio» on what was sent, or «Modificado» with its cause.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private IRackCadUpdateAllDoorPort _updateAllDoor;
        private long _updateAllRequestId;
        private List<UpdateAllSent> _updateAllSent;
        private bool _confirmingUpdateAll;
        private IReadOnlyList<UpdateAllResultRow> _updateAllResults = Array.Empty<UpdateAllResultRow>();
        private string _updateAllResultText = string.Empty;
        private bool _updateAllResultFailed;
        private readonly HashSet<INotifyPropertyChanged> _updateAllWatched = new();

        // A draft that went in the request: its request as its own «Actualizar» keeps it (to leave the editor as after it), and its item.
        private sealed record UpdateAllSent(DoorRequest Request, UpdateAllItem Item, object Editor);

        // A draft of the window as «Actualizar todo» sees it: its candidate and how its write is built (null when there is none to build).
        private sealed record UpdateAllEntry(UpdateAllCandidate Candidate, Func<long, UpdateAllSent> Build, INotifyPropertyChanged Source);

        private const string UpdateAllTakenText =
            "AutoCAD está escribiendo los borradores de «Actualizar todo» en una sola operación; la barra de estado de AutoCAD muestra el progreso "
            + "y ESC lo cancela antes de confirmar. La ventana recibirá su respuesta.";

        /// <summary>
        /// T6 at: the point where the mark «posiblemente desactualizado» of the editors (part st of the sixth batch) reaches «Actualizar todo»,
        /// connected by the session when the parts are joined: (drawing, rack; null for the page «Variables») → whether the editor is marked.
        /// A marked draft is included, and «Actualizar todo» asks first, in line, as «Actualizar» does. Null: no draft is marked.
        /// </summary>
        public Func<SessionId, string, bool> UpdateAllStaleProbe { get; set; }

        // One line of the constructor: «Actualizar todo» exists only with a door that writes it.
        private void WireUpdateAllDoor(IRackCadRackDoorPort door)
        {
            _updateAllDoor = door as IRackCadUpdateAllDoorPort;
            if (_updateAllDoor == null) return;
            // T6 integration (at + st): the mark «El dibujo cambió» of the editors (st); «Variables» (rack null) has none.
            UpdateAllStaleProbe ??= (session, rackId) => !string.IsNullOrWhiteSpace(rackId) && IsDrawingChanged(session, rackId);
            _updateAllDoor.UpdateAllWritten += OnUpdateAllWritten;
            _updateAllDoor.RequestStarted += OnUpdateAllStarted;

            // The button follows the drafts: every editor says its changes through the header (and the list), and a request of the door
            // through the waiting state.
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(HeaderText) || e.PropertyName == nameof(IsWaitingForAutoCad) || e.PropertyName == nameof(HasRackEditor))
                    RaiseUpdateAll();
            };
        }

        // ------------------------------------------------------------------ what the window shows

        /// <summary>«Actualizar todo» is offered: the window has its door and a drawing.</summary>
        public bool ShowUpdateAll => _updateAllDoor != null && Panel.PresentedSession.HasValue;

        /// <summary>What «Actualizar todo» would write now, and what prevents it, for the drawing of the window.</summary>
        public UpdateAllEligibilityResult UpdateAllEligibility
            => RackCad.Application.VentanaRackCad.UpdateAllEligibility.Decide(UpdateAllEntriesOfTheDrawing().Select(e => e.Candidate));

        /// <summary>«Actualizar todo (N)»: N is the number of drafts it would write.</summary>
        public string UpdateAllLabel => UpdateAllText.Label(ShowUpdateAll ? UpdateAllEligibility.Count : 0);

        public bool CanUpdateAll => UpdateAllBlockedText.Length == 0;

        /// <summary>Why «Actualizar todo» cannot run now, in the words of the window; empty when it can.</summary>
        public string UpdateAllBlockedText
        {
            get
            {
                if (_updateAllDoor == null) return "Aquí no se puede escribir el dibujo.";
                if (!Panel.PresentedSession.HasValue) return "No hay un dibujo abierto.";
                if (_inFlight != null) return IsUpdateAllRequest(_inFlight) ? "«Actualizar todo» está escribiendo." : DoorBusyText;
                if (_confirmingUpdateAll) return "Responde antes a la pregunta de «Actualizar todo».";
                var eligibility = UpdateAllEligibility;

                // Review of c4985a2e, minor 5: a value typed in a field counts when it is confirmed (Intro, or leaving the field); the button
                // that is off because of it says so.
                return eligibility.Included.Count == 0 && eligibility.Blockers.Count == 0 ? NothingToWriteText : eligibility.BlockedText;
            }
        }

        /// <summary>The motive of the button when nothing has changes, with what to do when a value was just typed.</summary>
        public const string NothingToWriteText = UpdateAllText.NothingToWrite
            + " Un valor que acabas de escribir cuenta cuando pulsas Intro o sales de su campo.";

        /// <summary>The tip of the button: what it does, or why it is off.</summary>
        public string UpdateAllToolTip
        {
            get
            {
                var blocked = UpdateAllBlockedText;
                return blocked.Length > 0
                    ? blocked
                    : "Escribe en el dibujo, en una sola operación, todos los borradores con cambios de este dibujo. Si uno falla, no se escribe ninguno; un «U» lo deshace todo.";
            }
        }

        /// <summary>A draft with changes prevents «Actualizar todo»: the notice under the header names it with its cause.</summary>
        public bool HasUpdateAllBlockers => ShowUpdateAll && _inFlight == null && UpdateAllEligibility.Blockers.Count > 0;

        public string UpdateAllBlockersText => HasUpdateAllBlockers ? UpdateAllEligibility.BlockedText : string.Empty;

        /// <summary>A draft marked «posiblemente desactualizado» is included: «Actualizar todo» asks, in line, before writing.</summary>
        public bool IsConfirmingUpdateAll => _confirmingUpdateAll;

        public string UpdateAllConfirmText
        {
            get
            {
                if (!_confirmingUpdateAll) return string.Empty;
                var stale = UpdateAllEligibility.PossiblyStale.Select(c => UpdateAllText.Quoted(c.Name)).ToList();
                return UpdateAllText.List(stale) + (stale.Count == 1 ? " puede estar desactualizado" : " pueden estar desactualizados")
                       + ": corrió una orden en AutoCAD desde que se leyeron. «Actualizar todo» escribirá "
                       + (stale.Count == 1 ? "su borrador" : "sus borradores") + " encima de lo que hay ahora en el dibujo.";
            }
        }

        /// <summary>The list of the last «Actualizar todo»: every draft with how it ended and why.</summary>
        public IReadOnlyList<UpdateAllResultRow> UpdateAllResults => _updateAllResults;

        public bool HasUpdateAllResults => _updateAllResultText.Length > 0;

        public string UpdateAllResultText => _updateAllResultText;

        public bool UpdateAllResultFailed => _updateAllResultFailed;

        // Review of c4985a2e, minor 2: what prevents it shows only after the click (before it, the tip of the button says it).
        public bool ShowUpdateAllPanel => _confirmingUpdateAll || HasUpdateAllResults;

        // ------------------------------------------------------------------ the gestures

        /// <summary>
        /// «Actualizar todo (N)»: the gesture of the user. Nothing to write, a draft that prevents it or a request that waits say why; a draft
        /// marked «posiblemente desactualizado» asks first; otherwise the request goes.
        /// </summary>
        public void RequestUpdateAll()
        {
            Contained(() =>
            {
                var blocked = UpdateAllBlockedText;
                if (blocked.Length > 0)
                {
                    ShowUpdateAllResult(blocked, failed: true, Array.Empty<UpdateAllResultRow>());
                    return;
                }

                if (UpdateAllEligibility.NeedsConfirmation)
                {
                    _confirmingUpdateAll = true;
                    RaiseUpdateAll();
                    return;
                }

                SendUpdateAll();
            });
        }

        /// <summary>«Escribir todo de todos modos» of the question: the request goes, with the drafts of now.</summary>
        public void ConfirmUpdateAll()
        {
            Contained(() =>
            {
                if (!_confirmingUpdateAll) return;
                _confirmingUpdateAll = false;
                RaiseUpdateAll();
                if (UpdateAllBlockedText.Length > 0)
                {
                    ShowUpdateAllResult(UpdateAllBlockedText, failed: true, Array.Empty<UpdateAllResultRow>());
                    return;
                }

                SendUpdateAll();
            });
        }

        /// <summary>«Cancelar» of the question: nothing is written.</summary>
        public void CancelUpdateAll()
        {
            if (!_confirmingUpdateAll) return;
            _confirmingUpdateAll = false;
            RaiseUpdateAll();
        }

        /// <summary>«Entendido»: the list of the last «Actualizar todo» goes away.</summary>
        public void DismissUpdateAllResults()
        {
            if (!HasUpdateAllResults) return;
            ShowUpdateAllResult(string.Empty, failed: false, Array.Empty<UpdateAllResultRow>());
        }

        // The request: every included draft as its own «Actualizar» would send it, every editor locked BEFORE it is handed over (M-22).
        private void SendUpdateAll()
        {
            var session = Panel.PresentedSession.Value;
            var entries = UpdateAllEntriesOfTheDrawing().ToList();
            var eligibility = RackCad.Application.VentanaRackCad.UpdateAllEligibility.Decide(entries.Select(e => e.Candidate));
            if (!eligibility.CanRun)
            {
                ShowUpdateAllResult(eligibility.BlockedText, failed: true, Array.Empty<UpdateAllResultRow>());
                return;
            }

            var id = ++_lastRequestId;
            var sent = new List<UpdateAllSent>();
            foreach (var candidate in eligibility.Included)
            {
                var built = entries.First(e => ReferenceEquals(e.Candidate, candidate)).Build(id);
                if (built == null)
                {
                    ShowUpdateAllResult("No se escribió nada: " + UpdateAllText.Quoted(candidate.Name) + " no tiene un borrador válido que escribir.", failed: true,
                        Array.Empty<UpdateAllResultRow>());
                    return;
                }

                sent.Add(built);
            }

            _inFlight = new DoorRequest(id, true, session, null, null, null);
            _updateAllRequestId = id;
            _updateAllSent = sent;
            foreach (var item in sent) BeginWritingOf(item);
            ShowUpdateAllResult(string.Empty, failed: false, Array.Empty<UpdateAllResultRow>());
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _updateAllDoor.RequestUpdateAll(new UpdateAllWrite(id, session, sent.Select(s => s.Item).ToList()))
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted)
            {
                foreach (var item in sent) HandedOverOf(item);
                return;
            }

            // Refused before AutoCAD took it: nothing was written, every editor can be edited again.
            var refusal = RefusalText(result);
            if (_inFlight != null && _inFlight.Id == id) _inFlight = null;
            if (_updateAllRequestId == id)
            {
                _updateAllRequestId = 0;
                _updateAllSent = null;
                foreach (var item in sent) StoppedOf(item, "«Actualizar todo» no se pudo pedir: " + refusal + " El borrador sigue sin escribir.", error: true);
            }

            RaiseWaiting();
            ShowUpdateAllResult("No se escribió nada: " + refusal, failed: true, Array.Empty<UpdateAllResultRow>());
            RackList.RefreshStates();
            OnPropertyChanged(nameof(HeaderText));
        }

        // ------------------------------------------------------------------ the answers of AutoCAD

        private bool IsUpdateAllRequest(DoorRequest request) => request != null && _updateAllRequestId != 0 && request.Id == _updateAllRequestId;

        // The waiting text of the window while «Actualizar todo» waits (one line of WaitingText).
        private string UpdateAllWaitingText
            => _inFlight != null && _inFlight.Started
                ? UpdateAllTakenText
                : "Esperando a que AutoCAD escriba " + UpdateAllText.Count(_updateAllSent?.Count ?? 0, "borrador", "borradores")
                  + " con «Actualizar todo». Si no responde (por ejemplo, porque había texto a medio escribir en la línea de órdenes), pulsa «Dejar de esperar».";

        // The command took the request: no editor says to press «Dejar de esperar» any more (the window already marked the request).
        private void OnUpdateAllStarted(long requestId)
        {
            Contained(() =>
            {
                if (!IsUpdateAllRequest(_inFlight) || _inFlight.Id != requestId || _updateAllSent == null) return;
                foreach (var item in _updateAllSent) TakenOf(item);
                RaiseWaiting();
            });
        }

        // The answer: every draft is left as after its own «Actualizar», and the list says how each one ended.
        private void OnUpdateAllWritten(UpdateAllWritten written)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (written == null || !IsUpdateAllRequest(request) || written.RequestId != request.Id) return;
                var sent = _updateAllSent ?? new List<UpdateAllSent>();
                _inFlight = null;
                _updateAllRequestId = 0;
                _updateAllSent = null;
                RaiseWaiting();

                var rows = new List<UpdateAllResultRow>();
                VariablesDoorWritten variables = null;
                foreach (var item in sent)
                {
                    var result = (written.Items ?? Array.Empty<UpdateAllItemResult>())
                                     .FirstOrDefault(r => r.System == item.Item.System && string.Equals(r.RackId, item.Item.RackId, StringComparison.OrdinalIgnoreCase))
                                 ?? new UpdateAllItemResult(item.Item.System, item.Item.RackId, item.Item.Name, UpdateAllOutcome.NotAppliedOtherFailed,
                                     "AutoCAD no dijo nada de este borrador: sigue sin escribir.");
                    if (item.Item.System == UpdateAllSystem.Variables && result.IsWritten) variables = result.VariablesWritten;
                    WrittenOf(item, result);
                    rows.Add(RowOf(rows.Count + 1, item.Item, result));

                    // Review of c4985a2e, minor 1: unknown until it is read again from the drawing or discarded (condition 4).
                    if (result.Outcome == UpdateAllOutcome.Unknown) _updateAllUnknown[(request.Session, item.Item.RackId)] = item.Editor;
                    else _updateAllUnknown.Remove((request.Session, item.Item.RackId));
                }

                var allWritten = written.Committed && rows.All(r => !r.Failed);
                ShowUpdateAllResult(written.Message ?? string.Empty, !allWritten, rows);
                RackList.RefreshStates();
                OnPropertyChanged(nameof(HeaderText));
                RaiseDynamic();
                if (variables != null) VariablesWrittenFor(request.Session, variables); // T6 at: what «Actualizar» of the page does after a write
            });
        }

        // «Dejar de esperar» of «Actualizar todo» (one line of StopWaiting): nothing will be written, every editor can be edited again.
        private void StopUpdateAll(DoorRequest request, string text)
        {
            var sent = _updateAllSent ?? new List<UpdateAllSent>();
            _updateAllRequestId = 0;
            _updateAllSent = null;
            foreach (var item in sent) StoppedOf(item, text, error: false);
            ShowUpdateAllResult("Se dejó de esperar «Actualizar todo»: si la orden llega a correr, no escribirá nada. Los borradores siguen sin escribir.",
                failed: false, Array.Empty<UpdateAllResultRow>());
            RackList.RefreshStates();
            OnPropertyChanged(nameof(HeaderText));
        }

        private static UpdateAllResultRow RowOf(int position, UpdateAllItem item, UpdateAllItemResult result)
        {
            var outcome = result.Outcome switch
            {
                UpdateAllOutcome.Applied => "Escrito",
                UpdateAllOutcome.AppliedWithWarnings => "Escrito, con avisos",
                UpdateAllOutcome.Failed => "No se pudo escribir",
                UpdateAllOutcome.NotAppliedOtherFailed => "No aplicado",
                UpdateAllOutcome.Cancelled => "Cancelado",
                _ => "Se desconoce",
            };
            var cause = result.Outcome == UpdateAllOutcome.Applied ? null : UpdateAllText.Sentence(result.Cause);

            // Review of c4985a2e, minor 6: the line says «No aplicado» once; its cause starts with what failed.
            const string NotApplied = "No aplicado: ";
            if (cause != null && cause.StartsWith(NotApplied, StringComparison.Ordinal))
                cause = char.ToUpper(cause[NotApplied.Length], System.Globalization.CultureInfo.CurrentCulture) + cause.Substring(NotApplied.Length + 1);
            return new UpdateAllResultRow(position, string.IsNullOrWhiteSpace(item.Name) ? "sin nombre" : item.Name.Trim(), SystemLabel(item.System), outcome, cause,
                !result.IsWritten);
        }

        private static string SystemLabel(UpdateAllSystem system) => system switch
        {
            UpdateAllSystem.Variables => "Variables",
            UpdateAllSystem.Selective => "Selectivo",
            UpdateAllSystem.PushBack => "Push Back",
            UpdateAllSystem.Dynamic => "Dinámico",
            UpdateAllSystem.Cantilever => "Cantilever",
            UpdateAllSystem.Cabecera => "Cabecera",
            _ => "Cama",
        };

        private void ShowUpdateAllResult(string text, bool failed, IReadOnlyList<UpdateAllResultRow> rows)
        {
            _updateAllResultText = text ?? string.Empty;
            _updateAllResultFailed = failed && _updateAllResultText.Length > 0;
            _updateAllResults = rows ?? Array.Empty<UpdateAllResultRow>();
            OnPropertyChanged(nameof(UpdateAllResults));
            OnPropertyChanged(nameof(HasUpdateAllResults));
            OnPropertyChanged(nameof(UpdateAllResultText));
            OnPropertyChanged(nameof(UpdateAllResultFailed));
            RaiseUpdateAll();
        }

        private void RaiseUpdateAll()
        {
            // A request of «Actualizar todo» the window no longer waits for (the drawing closed, AbandonInFlight) is forgotten.
            if (_updateAllRequestId != 0 && (_inFlight == null || _inFlight.Id != _updateAllRequestId))
            {
                _updateAllRequestId = 0;
                _updateAllSent = null;
            }

            PruneUnknown(); // review of c4985a2e, minor 1
            WatchUpdateAllSources();
            OnPropertyChanged(nameof(ShowUpdateAll));
            OnPropertyChanged(nameof(UpdateAllLabel));
            OnPropertyChanged(nameof(CanUpdateAll));
            OnPropertyChanged(nameof(UpdateAllBlockedText));
            OnPropertyChanged(nameof(UpdateAllToolTip));
            OnPropertyChanged(nameof(HasUpdateAllBlockers));
            OnPropertyChanged(nameof(UpdateAllBlockersText));
            OnPropertyChanged(nameof(IsConfirmingUpdateAll));
            OnPropertyChanged(nameof(UpdateAllConfirmText));
            OnPropertyChanged(nameof(ShowUpdateAllPanel));
        }

        // The editors (and the pages) say when their own «Actualizar» is blocked or unblocked: the button follows them too.
        // An editor the window forgot is no longer watched, so the window does not keep it alive.
        private void WatchUpdateAllSources()
        {
            var current = new HashSet<INotifyPropertyChanged>(AllUpdateAllEntries().Select(e => e.Source).Where(s => s != null));
            foreach (var gone in _updateAllWatched.Where(s => !current.Contains(s)).ToList())
            {
                gone.PropertyChanged -= OnUpdateAllSourceChanged;
                _updateAllWatched.Remove(gone);
            }

            foreach (var source in current)
            {
                if (_updateAllWatched.Add(source)) source.PropertyChanged += OnUpdateAllSourceChanged;
            }
        }

        private void OnUpdateAllSourceChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanUpdate" || e.PropertyName == "UpdateBlockedText" || e.PropertyName == "PrimaryBlockedText"
                || e.PropertyName == "IsDirty" || e.PropertyName == "IsWriting")
                RaiseUpdateAll();
        }

        // ------------------------------------------------------------------ the drafts of the window, system by system

        private IEnumerable<UpdateAllEntry> UpdateAllEntriesOfTheDrawing()
            => Panel.PresentedSession.HasValue ? EntriesOf(Panel.PresentedSession.Value) : Enumerable.Empty<UpdateAllEntry>();

        private IEnumerable<UpdateAllEntry> AllUpdateAllEntries()
            => _dynamicDrafts.Keys.Concat(_selectiveDrafts.Keys).Concat(_pushBackDrafts.Keys).Concat(_cantileverDrafts.Keys)
                .Concat(_camaDrafts.Keys).Concat(_cabeceraDrafts.Keys).Select(k => k.Session).Concat(_variablesPages.Keys)
                .Distinct().SelectMany(EntriesOf);

        private bool IsStale(SessionId session, string rackId)
        {
            try
            {
                return UpdateAllStaleProbe?.Invoke(session, rackId) == true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Why a draft with changes cannot go now; empty when it can: its own «Actualizar» is blocked, or the confirmation of the last
        // «Actualizar todo» failed for it (review of c4985a2e, minor 1: «Se desconoce» is a state of condition 4 until it is read again from
        // the drawing or discarded; a new editor, or one that is clean again, no longer carries it).
        private string BlockedOf(SessionId session, string rackId, object editor, bool dirty, bool canUpdate, string blockedText)
        {
            if (!dirty) return string.Empty;
            if (_updateAllUnknown.TryGetValue((session, rackId), out var unknown) && ReferenceEquals(unknown, editor)) return UnknownText;
            return canUpdate ? string.Empty : string.IsNullOrWhiteSpace(blockedText) ? "Su «Actualizar» no puede escribir ahora." : blockedText;
        }

        private const string UnknownText =
            "No se sabe si el último «Actualizar todo» lo escribió (falló la confirmación de AutoCAD): vuelve a leerlo del dibujo o descarta el borrador.";

        // The drafts whose state is unknown after a confirmation that failed, by drawing and rack (null: the page «Variables»), with the
        // editor they were for; a mark whose editor changed or is clean again is forgotten.
        private readonly Dictionary<(SessionId Session, string RackId), object> _updateAllUnknown = new(DraftKeyComparer.Instance);

        private void PruneUnknown()
        {
            if (_updateAllUnknown.Count == 0) return;
            var alive = AllUpdateAllEntries().Where(e => e.Candidate.IsDirty).Select(e => e.Source).ToList();
            foreach (var pair in _updateAllUnknown.ToList())
            {
                if (!alive.Any(source => ReferenceEquals(source, pair.Value))) _updateAllUnknown.Remove(pair.Key);
            }
        }

        private IEnumerable<UpdateAllEntry> EntriesOf(SessionId session)
        {
            if (_variablesPages.TryGetValue(session, out var page))
            {
                yield return new UpdateAllEntry(
                    new UpdateAllCandidate(UpdateAllSystem.Variables, null, VariablesName, page.IsDirty, page.IsWriting, false,
                        BlockedOf(session, null, page, page.IsDirty, page.CanUpdate, page.UpdateBlockedText), IsStale(session, null)),
                    id =>
                    {
                        var write = page.BuildWrite(id);
                        return write == null ? null : new UpdateAllSent(new DoorRequest(id, true, session, null, null, null, IsVariables: true),
                            new UpdateAllItem(UpdateAllSystem.Variables, null, VariablesName, Variables: write), page);
                    },
                    page);
            }

            foreach (var pair in _selectiveDrafts.Where(p => p.Key.Session == session).ToList())
            {
                var editor = pair.Value.Editor;
                var name = SelectiveNameOf(pair.Value);
                yield return new UpdateAllEntry(
                    new UpdateAllCandidate(UpdateAllSystem.Selective, editor.RackId, name, editor.IsDirty, editor.IsWriting, editor.IsNewRack,
                        BlockedOf(session, editor.RackId, editor, editor.IsDirty, editor.CanUpdate, editor.UpdateBlockedText), IsStale(session, editor.RackId)),
                    id =>
                    {
                        var snapshot = editor.Draft.SnapshotForWrite();
                        return snapshot == null ? null : new UpdateAllSent(
                            new DoorRequest(id, true, session, editor.RackId, snapshot.DesignJson, snapshot.Name, IsSelective: true),
                            new UpdateAllItem(UpdateAllSystem.Selective, editor.RackId, name, Selective: new SelectiveDoorWrite(id, session, editor.RackId, snapshot.Name,
                                snapshot.Design, snapshot.System, snapshot.LinkedPropertyFinalStates, null, NewRack: false, Properties: editor.PropertiesWrite())),
                            editor);
                    },
                    editor);
            }

            foreach (var pair in _pushBackDrafts.Where(p => p.Key.Session == session).ToList())
            {
                var editor = pair.Value.Editor;
                var name = PushBackNameOf(pair.Value);
                yield return new UpdateAllEntry(
                    new UpdateAllCandidate(UpdateAllSystem.PushBack, editor.RackId, name, editor.IsDirty, editor.IsWriting, editor.IsNewRack,
                        BlockedOf(session, editor.RackId, editor, editor.IsDirty, editor.CanUpdate, editor.UpdateBlockedText), IsStale(session, editor.RackId)),
                    id =>
                    {
                        var snapshot = editor.Draft.SnapshotForWrite();
                        return snapshot == null ? null : new UpdateAllSent(
                            new DoorRequest(id, true, session, editor.RackId, snapshot.DesignJson, snapshot.Name, IsPushBack: true),
                            new UpdateAllItem(UpdateAllSystem.PushBack, editor.RackId, name, PushBack: new PushBackDoorWrite(id, session, editor.RackId, snapshot.Name,
                                snapshot.Design, snapshot.System, null, NewRack: false, Properties: editor.PropertiesWrite())),
                            editor);
                    },
                    editor);
            }

            foreach (var pair in _dynamicDrafts.Where(p => p.Key.Session == session).ToList())
            {
                var editor = pair.Value.Editor;
                var name = DynamicNameOf(editor);
                yield return new UpdateAllEntry(
                    new UpdateAllCandidate(UpdateAllSystem.Dynamic, editor.RackId, name, editor.IsDirty, editor.IsWriting, editor.IsNewRack,
                        BlockedOf(session, editor.RackId, editor, editor.IsDirty, editor.CanUpdate, editor.UpdateBlockedText), IsStale(session, editor.RackId)),
                    id =>
                    {
                        var snapshot = editor.Draft.SnapshotForWrite();
                        return snapshot == null ? null : new UpdateAllSent(
                            new DoorRequest(id, true, session, editor.RackId, snapshot.DesignJson, snapshot.Name),
                            new UpdateAllItem(UpdateAllSystem.Dynamic, editor.RackId, name, Dynamic: new RackDoorWrite(id, session, editor.RackId, snapshot.Name,
                                snapshot.Design, snapshot.System, null, NewRack: false, Properties: editor.PropertiesWrite())),
                            editor);
                    },
                    editor);
            }

            foreach (var pair in _cantileverDrafts.Where(p => p.Key.Session == session).ToList())
            {
                var editor = pair.Value.Editor;
                var name = CantileverNameOf(pair.Value);
                yield return new UpdateAllEntry(
                    new UpdateAllCandidate(UpdateAllSystem.Cantilever, editor.RackId, name, editor.IsDirty, editor.IsWriting, editor.IsNewRack,
                        BlockedOf(session, editor.RackId, editor, editor.IsDirty, editor.CanUpdate, editor.UpdateBlockedText), IsStale(session, editor.RackId)),
                    id =>
                    {
                        var snapshot = editor.Draft.SnapshotForWrite();
                        return snapshot == null ? null : new UpdateAllSent(
                            new DoorRequest(id, true, session, editor.RackId, snapshot.DesignJson, snapshot.Name, IsCantilever: true),
                            new UpdateAllItem(UpdateAllSystem.Cantilever, editor.RackId, name, Cantilever: new CantileverDoorWrite(id, session, editor.RackId, snapshot.Name,
                                snapshot.Design, snapshot.Line, snapshot.DesignJson, null, false, editor.PropertiesWrite())),
                            editor);
                    },
                    editor);
            }

            foreach (var pair in _cabeceraDrafts.Where(p => p.Key.Session == session && p.Value.Editor.IsStandalone).ToList())
            {
                var editor = pair.Value.Editor;
                var name = CabeceraNameOf(pair.Value);
                yield return new UpdateAllEntry(
                    new UpdateAllCandidate(UpdateAllSystem.Cabecera, editor.RackId, name, editor.IsDirty, editor.IsWriting, false,
                        BlockedOf(session, editor.RackId, editor, editor.IsDirty, editor.CanUpdate, editor.UpdateBlockedText), IsStale(session, editor.RackId)),
                    id =>
                    {
                        // As UpdateCabecera: the inner name goes as it is (blank too), and what is sent is the base of «Limpio».
                        var copy = editor.Editor.Snapshot();
                        if (copy == null) return null;
                        var sent = new RackFrameProjectStore().Serialize(copy);
                        return new UpdateAllSent(new DoorRequest(id, true, session, editor.RackId, sent, copy.Name, IsCabecera: true),
                            new UpdateAllItem(UpdateAllSystem.Cabecera, editor.RackId, name, Cabecera: new CabeceraDoorWrite(id, session, editor.RackId, copy.Name, copy)),
                            editor);
                    },
                    editor);
            }

            foreach (var pair in _camaDrafts.Where(p => p.Key.Session == session).ToList())
            {
                var editor = pair.Value.Editor;
                var name = CamaNameOf(pair.Value);
                yield return new UpdateAllEntry(
                    new UpdateAllCandidate(UpdateAllSystem.Cama, editor.RackId, name, editor.IsDirty, editor.IsWriting, editor.IsNewRack,
                        BlockedOf(session, editor.RackId, editor, editor.IsDirty, editor.CanUpdate, editor.PrimaryBlockedText), IsStale(session, editor.RackId)),
                    id =>
                    {
                        // As WriteCama: a copy of the bed, the trimmed name, and what is sent as the base of «Limpio».
                        var bed = editor.Draft.Snapshot();
                        if (bed == null) return null;
                        var camaName = (editor.Name ?? string.Empty).Trim();
                        var sent = new FlowBedConfigurationStore().Serialize(bed);
                        return new UpdateAllSent(new DoorRequest(id, true, session, editor.RackId, sent, camaName, IsCama: true),
                            new UpdateAllItem(UpdateAllSystem.Cama, editor.RackId, name, Cama: new CamaDoorWrite(id, session, editor.RackId, camaName, bed, false,
                                editor.PropertiesWrite(), null)),
                            editor);
                    },
                    editor);
            }
        }

        // ------------------------------------------------------------------ every draft as after its own «Actualizar»

        private void BeginWritingOf(UpdateAllSent item)
        {
            switch (item.Editor)
            {
                case VariablesPageViewModel page: page.BeginWriting(); break;
                case SelectiveEditorViewModel selective: selective.BeginWriting(); break;
                case PushBackEditorViewModel pushBack: pushBack.BeginWriting(); break;
                case DynamicEditorViewModel dynamic: dynamic.BeginWriting(); break;
                case CantileverEditorViewModel cantilever: cantilever.BeginWriting(); break;
                case CabeceraEditorViewModel cabecera: cabecera.SetWriting(true); break;
                case CamaEditorViewModel cama: cama.BeginWriting(); break;
            }
        }

        private static void HandedOverOf(UpdateAllSent item)
        {
            switch (item.Editor)
            {
                case SelectiveEditorViewModel selective: selective.WriteHandedOver(); break;
                case PushBackEditorViewModel pushBack: pushBack.WriteHandedOver(); break;
                case DynamicEditorViewModel dynamic: dynamic.WriteHandedOver(); break;
                case CantileverEditorViewModel cantilever: cantilever.WriteHandedOver(); break;
            }
        }

        // The command took the request (as OnRequestStarted for one draft).
        private void TakenOf(UpdateAllSent item)
        {
            switch (item.Editor)
            {
                case VariablesPageViewModel page: page.WriteTaken(); break;
                case SelectiveEditorViewModel selective: selective.WriteTaken(); break;
                case DynamicEditorViewModel dynamic: dynamic.WriteTaken(); break;
                default:
                    if (item.Request.IsPushBack) PushBackWriteTaken(item.Request);
                    else if (item.Request.IsCantilever) CantileverWriteTaken(item.Request);
                    else if (item.Request.IsCabecera) CabeceraWriteTaken(item.Request);
                    else if (item.Request.IsCama) CamaWriteTaken(item.Request);
                    break;
            }
        }

        // «Dejar de esperar», or a request refused before AutoCAD took it (as StopWaiting for one draft).
        private void StoppedOf(UpdateAllSent item, string text, bool error)
        {
            if (item.Request.IsVariables)
            {
                if (_variablesPages.TryGetValue(item.Request.Session, out var page))
                {
                    if (error) page.EndWriting(new VariablesDoorWritten(item.Request.Id, false, text));
                    else page.StoppedWaiting();
                }

                return;
            }

            if (item.Request.IsCabecera)
            {
                StopCabeceraWrite(item.Request, text);
                return;
            }

            if (item.Request.IsCama) StopCamaWrite(item.Request, text);
            else if (item.Request.IsCantilever) StopCantileverWrite(item.Request, text);
            else if (item.Request.IsPushBack) StopPushBackWrite(item.Request, text);
            else if (item.Request.IsSelective && _selectiveDrafts.TryGetValue((item.Request.Session, item.Request.RackId), out var selective))
            {
                selective.Editor.EndWriting();
                selective.Editor.Say(text, error: error);
                EchoHiddenDraft(selective.Editor, text, failed: error);
            }
            else if (_dynamicDrafts.TryGetValue((item.Request.Session, item.Request.RackId), out var dynamic))
            {
                dynamic.Editor.EndWriting();
                dynamic.Editor.Say(text, error: error);
                EchoHiddenDraft(dynamic.Editor, text, failed: error);
            }
        }

        // The answer of one draft, as the answer of its own «Actualizar» (OnDesignWritten, OnVariablesWritten): written = «Limpio» on what was
        // sent; not written = «Modificado» with its cause.
        private void WrittenOf(UpdateAllSent item, UpdateAllItemResult result)
        {
            var request = item.Request;
            if (request.IsVariables)
            {
                if (!_variablesPages.TryGetValue(request.Session, out var page)) return;
                page.EndWriting(result.VariablesWritten ?? new VariablesDoorWritten(request.Id, result.IsWritten, UpdateAllText.Sentence(result.Cause)));
                return;
            }

            var written = result.Written ?? new RackDesignWritten(request.Id, request.RackId, result.IsWritten, UpdateAllText.Sentence(result.Cause));
            if (request.IsCabecera) CabeceraWritten(request, written);
            else if (request.IsCama) CamaWritten(request, written);
            else if (request.IsCantilever) CantileverWritten(request, written);
            else if (request.IsPushBack) PushBackWritten(request, written);
            else if (request.IsSelective)
            {
                if (!_selectiveDrafts.TryGetValue((request.Session, request.RackId), out var selective)) return;
                selective.Editor.WriteHandedOver();
                selective.Editor.EndWriting();
                if (written.Written) selective.Editor.MarkWritten(written.Message, request.SentJson, request.SentName);
                else selective.Editor.Say(written.Message, error: true);
                if (!written.Written) EchoHiddenDraft(selective.Editor, written.Message, failed: true);
                selective.Editor.PropertiesWritten(written);
            }
            else if (_dynamicDrafts.TryGetValue((request.Session, request.RackId), out var entry))
            {
                var editor = entry.Editor;
                editor.WriteHandedOver();
                editor.EndWriting();
                if (written.Written) editor.MarkWritten(written.Message, request.SentJson, request.SentName);
                else editor.Say(written.Message, error: true);
                if (!written.Written) EchoHiddenDraft(editor, written.Message, failed: true);
                editor.PropertiesWritten(written);
            }
        }
    }
}
